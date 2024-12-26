using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Collections;
using Payroll;
using P2BUltimate.Security;
using P2BUltimate.Models;
using System.Diagnostics;
using EMS;
using Leave;
using Attendance;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Web.Mvc;
using System.Windows.Interop;
using System.Data.Entity.Infrastructure;
using System.IO;

namespace P2BUltimate.Process
{
    public static class ServiceBook
    {
        //private static DataBaseContext db = new DataBaseContext();
        //regular increment with manual increment date entry
        public static Double[] IncrementRegularManualDateDataCalc(Company OCompanyPayroll, EmployeePayroll OEmployeePayroll, List<BasicScaleDetails> OBasicScale, RegIncrPolicy OIncrPolicyRegular, NonRegIncrPolicy OIncrPolicyNonRegular, StagIncrPolicy OIncrPolicyStagRegular, double mOldBasic, DateTime? mIncrementDate)
        {
            DateTime? mProcessIncrDate = mIncrementDate;
            DateTime? mOrignalIncrDate = mIncrementDate;
            double mLWPDays = 0;

            var OEmpIncrementHistoryRegular = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR")
                                        .FirstOrDefault();
            //.LastOrDefault();
            var OEmpIncrementHistoryStagnancy = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .Where(e => e.IncrActivity.StagIncrPolicy != null)
                                        .FirstOrDefault();
            //.LastOrDefault();
            var OEmpIncrementHistory = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .FirstOrDefault();
            //.LastOrDefault();

            double mNewBasic = 0;
            //mOldBasic = OBasicScale.Select(e => e.OldBasic).FirstOrDefault();

            //get new basic by passing increment steps

            if (OEmpIncrementHistoryStagnancy != null && OEmpIncrementHistoryStagnancy.StagnancyAppl == true)
            {
                mNewBasic = mOldBasic;
            }
            else
            {
                mNewBasic = BasicSelector(1, OBasicScale, mOldBasic);//regular increment=1 count

            }

            int mTotalincrInService = 0;
            var OTotalincrInService = OEmployeePayroll.IncrementServiceBook

                                    .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR")
                                    .GroupBy(m => m.IncrActivity.IncrList.LookupVal.ToString())
                                    .Count();
            mTotalincrInService = Convert.ToInt16(OTotalincrInService.ToString());
            //var OIncrPolicyRegular = db.P_RegIncrPolicyStagancyPolicy.SingleOrDefault();
            bool mVerifyNonRegularParameterCheck = false;
            if (OIncrPolicyNonRegular != null)
            {
                int mErrorNonRegIncr = 0;
                int mService = Convert.ToInt16(DifferenceTotalYears(OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date, DateTime.Now.Date));
                //int mService = DateTime.Now.Date.Subtract(OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date).ToString("yyyy");
                if (OIncrPolicyNonRegular.MinSerAppl == true)
                {
                    if (mService < OIncrPolicyNonRegular.MinService)
                    {
                        mErrorNonRegIncr = 1;
                        mVerifyNonRegularParameterCheck = true;
                    }
                }
                if (OIncrPolicyNonRegular.MaxSerLockAppl == true)
                {
                    if (mService > OIncrPolicyNonRegular.MaxService)
                    {
                        mErrorNonRegIncr = 2;
                        mVerifyNonRegularParameterCheck = true;
                    }
                }
                if (OIncrPolicyNonRegular.MaxIncrInService == mTotalincrInService)
                {
                    mErrorNonRegIncr = 3;
                    mVerifyNonRegularParameterCheck = true;
                }
            }
            if (mVerifyNonRegularParameterCheck == false)
            {

                //call stagnanacy policy check
                //array 0 = newbasic amount
                //array 1 = stag count
                //array 2 = stag applicable
                Double[] mStagnancyData = new Double[4];
                mStagnancyData = IncrStagnancyFittmentPolicy(mNewBasic, OEmpIncrementHistoryStagnancy, OIncrPolicyStagRegular, OBasicScale);
                //mNewBasic = mStagnancyData[0];
                //bool mStagBool = false;
                //int mstagCount = Convert.ToInt16(mStagnancyData[1]);
                //if (mStagnancyData[2] == 0)
                //{
                //    mStagBool = false;
                //}
                //else
                //{
                //    mStagBool = true;
                //}

                return mStagnancyData;


            }
            else
            {
                //error = mErrorNonRegincr variable
                //message on nonregular policy failure
            }

            return null;
        }

        //regular increment
        public static IncrDataCalc IncrementRegularDataCalc(Company OCompanyPayroll, EmployeePayroll OEmployeePayroll, List<BasicScaleDetails> OBasicScale, RegIncrPolicy OIncrPolicyRegular, NonRegIncrPolicy OIncrPolicyNonRegular, StagIncrPolicy OIncrPolicyStagRegular, double mOldBasic)
        {
            DateTime mProcessIncrDate;
            DateTime mOrignalIncrDate;
            double mLWPDays = 0;
            // due to migration increment date issue(from system increment done 01/04/2023 after migrate old data 2002 to 2022 so 4/2024 increment due list employee not display)
            // so order by id desc replace process increment date
            var OEmpIncrementHistoryRegular = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR")
                // .LastOrDefault();
                                       .FirstOrDefault();
            var OEmpIncrementHistoryStagnancy = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .Where(e => e.IncrActivity.StagIncrPolicy != null)
                //  .LastOrDefault();
                                      .FirstOrDefault();
            var OEmpIncrementHistory = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                //  .LastOrDefault();
                                      .FirstOrDefault();
            DateTime[] IncrProcessActualDates = new DateTime[2];

            double mNewBasic = 0;

            if (OEmpIncrementHistoryStagnancy != null && OEmpIncrementHistoryStagnancy.StagnancyAppl == true)
            {
                mNewBasic = mOldBasic;
            }
            else
            {
                mNewBasic = BasicSelector(1, OBasicScale, mOldBasic);//regular increment=1 count

            }

            int mTotalincrInService = 0;
            var OTotalincrInService = OEmployeePayroll.IncrementServiceBook

                                    .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR")
                                    .GroupBy(m => m.IncrActivity.IncrList.LookupVal.ToString())
                                    .Count();
            mTotalincrInService = Convert.ToInt16(OTotalincrInService.ToString());
            //var OIncrPolicyRegular = db.P_RegIncrPolicyStagancyPolicy.SingleOrDefault();
            bool mVerifyNonRegularParameterCheck = false;
            if (OIncrPolicyNonRegular != null)
            {
                int mErrorNonRegIncr = 0;
                int mService = Convert.ToInt16(DifferenceTotalYears(OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date, DateTime.Now.Date));
                //int mService = DateTime.Now.Date.Subtract(OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date).ToString("yyyy");
                if (OIncrPolicyNonRegular.MinSerAppl == true)
                {
                    if (mService < OIncrPolicyNonRegular.MinService)
                    {
                        mErrorNonRegIncr = 1;
                        mVerifyNonRegularParameterCheck = true;
                    }
                }
                if (OIncrPolicyNonRegular.MaxSerLockAppl == true)
                {
                    if (mService > OIncrPolicyNonRegular.MaxService)
                    {
                        mErrorNonRegIncr = 2;
                        mVerifyNonRegularParameterCheck = true;
                    }
                }
                if (OIncrPolicyNonRegular.MaxIncrInService == mTotalincrInService)
                {
                    mErrorNonRegIncr = 3;
                    mVerifyNonRegularParameterCheck = true;
                }
            }
            if (mVerifyNonRegularParameterCheck == false)
            {
                //derive Original and Process increment dates with LWP effect

                IncrProcessActualDates = IncrDueDate(OEmpIncrementHistoryRegular, OEmployeePayroll, OIncrPolicyRegular, OIncrPolicyNonRegular, OIncrPolicyStagRegular);
                mOrignalIncrDate = IncrProcessActualDates[0];
                mProcessIncrDate = IncrProcessActualDates[1];

                //call stagnanacy policy check
                //array 0 = newbasic amount
                //array 1 = stag count
                //array 2 = stag applicable
                Double[] mStagnancyData = new Double[4];
                mStagnancyData = IncrStagnancyFittmentPolicy(mNewBasic, OEmpIncrementHistoryStagnancy, OIncrPolicyStagRegular, OBasicScale);
                if (mStagnancyData != null)
                {
                    mNewBasic = mStagnancyData[0];
                    bool mStagBool = false;
                    int mstagCount = Convert.ToInt16(mStagnancyData[1]);
                    if (mStagnancyData[2] == 0)
                    {
                        mStagBool = false;
                    }
                    else
                    {
                        mStagBool = true;
                    }
                    if (mStagnancyData[3] != 0)
                    {
                        int mSpanyear = Convert.ToInt16(mStagnancyData[3]);
                        mOrignalIncrDate = mOrignalIncrDate.AddYears(mSpanyear - 1);
                        mProcessIncrDate = mProcessIncrDate.AddYears(mSpanyear - 1);

                    }
                    //save the record in IncrDataCalc
                    IncrDataCalc OIncrDataCalc = new IncrDataCalc()
                    {

                        LWPDays = (mProcessIncrDate - mOrignalIncrDate).Days,
                        NewBasic = mNewBasic,
                        OldBasic = mOldBasic,
                        OrignalIncrDate = mOrignalIncrDate,
                        ProcessIncrDate = mProcessIncrDate,
                        StagnancyAppl = mStagBool,
                        StagnancyCount = mstagCount

                    };
                    return OIncrDataCalc;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                //error = mErrorNonRegincr variable
                //message on nonregular policy failure
            }

            return null;
        }
        //Basic Fittment Amount Calculation in promotion activity
        public static double BasicFittmentSelector(double mBasicAmount, IEnumerable<BasicScaleDetails> OBasicScale)
        {

            double mFittmentAmount = 0;
            double mOldBasic = mBasicAmount;//db.P_BasicScaleDetails.Select(e=>e.OldBasic).SingleOrDefault();
            // get current basic in parameter: uses- in case current basic is above endslab i.e. stagnanat
            OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
            Boolean fitment = false;
            foreach (var OBasicScaleRange in OBasicScale)
            {
                if (mOldBasic < OBasicScaleRange.StartingSlab)
                {
                    mFittmentAmount = (OBasicScaleRange.StartingSlab - mOldBasic);
                    fitment = true;
                    break;
                }
                for (int i = 1; i <= OBasicScaleRange.IncrementCount; i++)
                {

                    if (mOldBasic > OBasicScaleRange.StartingSlab && mOldBasic <= (OBasicScaleRange.StartingSlab + (OBasicScaleRange.IncrementAmount * i)))
                    {
                        mFittmentAmount = (OBasicScaleRange.StartingSlab + (OBasicScaleRange.IncrementAmount * i) - mOldBasic);
                        fitment = true;
                        break;

                    }
                }
                if (mFittmentAmount >= 0 && fitment == true)
                {
                    break;
                }
            }

            return mFittmentAmount;
        }
        //new basic calculation as per basic slab
        public static double BasicSelector(int IncrCount, List<BasicScaleDetails> OBasicScale, double mBasicAmount)
        {
            OBasicScale = OBasicScale.OrderBy(e => e.StartingSlab).ToList();
            double mNewBasic = 0;
            double mOldBasic = mBasicAmount;// OBasicScale.Select(e=>e.OldBasic).SingleOrDefault();//db.P_BasicScaleDetails.Select(e=>e.OldBasic).SingleOrDefault();
            mNewBasic = mOldBasic;// get current basic in parameter: uses- in case current basic is above endslab i.e. stagnanat
            for (int i = 0; i < IncrCount; i++)
            {
                foreach (var OBasicScaleRange in OBasicScale)
                {
                    mNewBasic = mOldBasic + OBasicScaleRange.IncrementAmount;
                    if (mNewBasic > OBasicScaleRange.StartingSlab && mNewBasic <= OBasicScaleRange.EndingSlab)
                    {
                        mOldBasic = mNewBasic;
                        break;
                    }
                    else if (mNewBasic <= OBasicScaleRange.StartingSlab)
                    {
                        mNewBasic = OBasicScaleRange.StartingSlab;
                        break;
                    }
                    else if (mNewBasic > OBasicScaleRange.EndingSlab)
                    {
                        mNewBasic = mOldBasic;

                    }
                }
            }
            return mNewBasic;
        }
        //item 0= original date item 1 = process date
        //increment due date finder without fittment i.e. mid month, mid quarter
        public static DateTime[] IncrDueDate(IncrementServiceBook OEmpIncrementHistoryRegular, EmployeePayroll OEmployeePayroll, RegIncrPolicy OIncrPolicyRegular, NonRegIncrPolicy OIncrPolicyNonRegular, StagIncrPolicy OIncrPolicyStagRegular)
        {
            DateTime mProcessIncrDate = DateTime.Now.Date;
            DateTime[] IncrProcessActualDates = new DateTime[2];

            if (OEmpIncrementHistoryRegular == null)// || OEmpIncrementHistory.IncrementActivity.IncrList.LookupVal.ToUpper()=="REGULAR)) //First time increment
            {
                if (OIncrPolicyRegular.IsJoiningDate == true)//joinind date policy
                {
                    mProcessIncrDate = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date;
                    var mYear = "";
                    var dtmm = "";
                    dtmm = (mProcessIncrDate).ToString("dd/MM");
                    mYear = DateTime.Now.AddYears(-1).Year.ToString();

                    string month = (mProcessIncrDate).ToString("MM");
                    // int newdt = Convert.ToDateTime("01/" + month + "/" + mYear).Day;

                    int monch = Convert.ToInt32(month);
                    int yerch = Convert.ToInt32(mYear);
                    int newdt = DateTime.DaysInMonth(yerch, monch);

                    string olddd = (mProcessIncrDate).ToString("dd");
                    int oldddint = Convert.ToInt32(olddd);
                    if (month == "02")
                    {
                        if (oldddint > newdt)
                        {
                            mProcessIncrDate = Convert.ToDateTime(newdt + "/" + month + "/" + mYear);
                        }
                        else
                        {
                            mProcessIncrDate = Convert.ToDateTime(dtmm + "/" + mYear);
                        }
                    }
                    else
                    {
                        mProcessIncrDate = Convert.ToDateTime(dtmm + "/" + mYear);
                    }

                }
                else if (OIncrPolicyRegular.IsConfirmDate == true) //confirmation date policy
                {
                    if (OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate != null)
                    {
                        mProcessIncrDate = OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value.Date;
                        var mYear = "";
                        var dtmm = "";
                        dtmm = (mProcessIncrDate).ToString("dd/MM");
                        mYear = DateTime.Now.AddYears(-1).Year.ToString();

                        string month = (mProcessIncrDate).ToString("MM");
                        //  int newdt = Convert.ToDateTime("01/" + month +  "/" + mYear).Day;
                        int monch = Convert.ToInt32(month);
                        int yerch = Convert.ToInt32(mYear);
                        int newdt = DateTime.DaysInMonth(yerch, monch);

                        string olddd = (mProcessIncrDate).ToString("dd");
                        int oldddint = Convert.ToInt32(olddd);
                        if (month == "02")
                        {
                            if (oldddint > newdt)
                            {
                                mProcessIncrDate = Convert.ToDateTime(newdt + "/" + month + "/" + mYear);
                            }
                            else
                            {
                                mProcessIncrDate = Convert.ToDateTime(dtmm + "/" + mYear);
                            }
                        }
                        else
                        {
                            mProcessIncrDate = Convert.ToDateTime(dtmm + "/" + mYear);
                        }


                    }
                    else
                    {
                        mProcessIncrDate = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date;
                        var mYear = "";
                        var dtmm = "";
                        dtmm = (mProcessIncrDate).ToString("dd/MM");
                        mYear = DateTime.Now.AddYears(-1).Year.ToString();

                        string month = (mProcessIncrDate).ToString("MM");
                        // int newdt = Convert.ToDateTime("01/" + month + "/" + mYear).Day;
                        int monch = Convert.ToInt32(month);
                        int yerch = Convert.ToInt32(mYear);
                        int newdt = DateTime.DaysInMonth(yerch, monch);
                        string olddd = (mProcessIncrDate).ToString("dd");
                        int oldddint = Convert.ToInt32(olddd);
                        if (month == "02")
                        {
                            if (oldddint > newdt)
                            {
                                mProcessIncrDate = Convert.ToDateTime(newdt + "/" + month + "/" + mYear);
                            }
                            else
                            {
                                mProcessIncrDate = Convert.ToDateTime(dtmm + "/" + mYear);
                            }
                        }
                        else
                        {
                            mProcessIncrDate = Convert.ToDateTime(dtmm + "/" + mYear);
                        }


                    }
                }
                else if (OIncrPolicyRegular.IsFixMonth == true)
                {
                    var mYear = "";
                    if (OIncrPolicyRegular.IncrMonth >= DateTime.Now.Month)
                    {
                        mYear = DateTime.Now.AddYears(-1).Year.ToString();
                    }
                    else
                    {
                        mYear = DateTime.Now.Year.ToString();
                    }

                    mProcessIncrDate = Convert.ToDateTime("01/" + Convert.ToString(OIncrPolicyRegular.IncrMonth).PadLeft(2, '0') + "/" + mYear);
                }
                //amount policy yet to implement
            }
            else //Incr history available
            {

                if (OIncrPolicyRegular.IsLWPEffectDateAsIncrDate == false) //due to lwp change increment date will not be increment date for next year
                {
                    mProcessIncrDate = OEmpIncrementHistoryRegular.OrignalIncrDate.Value.Date;
                }
                else  //due to lwp change increment date will be new increment date for next year
                {
                    mProcessIncrDate = OEmpIncrementHistoryRegular.ProcessIncrDate.Value.Date;
                }
            }
            if (OIncrPolicyRegular.IsLWPIncl == true)
            {
                IncrProcessActualDates = IncrLWPFittmentPolicy(OEmployeePayroll, mProcessIncrDate, OIncrPolicyRegular);
                mProcessIncrDate = IncrProcessActualDates[1];
            }
            else
            {
                mProcessIncrDate = mProcessIncrDate.AddYears(1);
                IncrProcessActualDates[0] = mProcessIncrDate;
                IncrProcessActualDates[1] = mProcessIncrDate;
            }
            mProcessIncrDate = IncrFittmentPolicy(mProcessIncrDate, OIncrPolicyRegular);
            IncrProcessActualDates[1] = mProcessIncrDate;
            return IncrProcessActualDates;
        }
        //increment due date fittment with lwp days and lwf days ceiling
        public static DateTime[] IncrLWPFittmentPolicy(EmployeePayroll OEmployeePayroll, DateTime mRawProcessIncrDate, RegIncrPolicy OIncrPolicyRegular)
        {
            //ArrayList IncrProcessActualDates=new ArrayList();
            DateTime[] IncrProcessActualDates = new DateTime[2];
            DateTime mProcessIncrDate = mRawProcessIncrDate;
            IncrProcessActualDates[0] = mProcessIncrDate;
            IncrProcessActualDates[1] = mProcessIncrDate;
            double mLWPDays = LWPDaysCalc(OEmployeePayroll, mRawProcessIncrDate, mRawProcessIncrDate.AddYears(1));
             mLWPDays = Math.Round(mLWPDays, 0);// halfday round as 1
            if (mLWPDays > OIncrPolicyRegular.LWPMinCeiling)
            {
                //if (OIncrPolicyRegular.IsLWPIncl == true)
                //{
                //Satara shahakri bank LWP day add fix 30 day month not calenadar month start
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\InrementDateFix30days" + ".ini";
                localPath = new Uri(path).LocalPath;
                string Incrementdatefix30day = "";
                using (var streamReader = new StreamReader(localPath))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var comp = line;
                        if (comp == "SDCCB")
                        {
                            Incrementdatefix30day = comp;
                            double Fmonth = mLWPDays / 30;
                            double Monthint = Math.Truncate(Fmonth);
                            int Monthint1 = Convert.ToInt32(Monthint);
                            double Remdays = mLWPDays - (30 * Monthint1);

                            mProcessIncrDate = mProcessIncrDate.AddYears(1);
                            IncrProcessActualDates[0] = mProcessIncrDate; //item 0 =actual date
                            mProcessIncrDate = mProcessIncrDate.AddMonths(Monthint1);
                            mProcessIncrDate = mProcessIncrDate.AddDays(Remdays);
                            IncrProcessActualDates[1] = mProcessIncrDate;//item 1 =Process date

                        }
                    }
                }
                //Satara shahakri bank LWP day add fix 30 day month not calenadar month end
                if (Incrementdatefix30day == "")
                {
                
                    mProcessIncrDate = mProcessIncrDate.AddYears(1);
                    IncrProcessActualDates[0] = mProcessIncrDate; //item 0 =actual date
                    mProcessIncrDate = mProcessIncrDate.AddDays(mLWPDays);
                    IncrProcessActualDates[1] = mProcessIncrDate;//item 1 =Process date
                }


                //}
                //else
                //{
                //    mProcessIncrDate = mProcessIncrDate.AddYears(1);
                //    IncrProcessActualDates[0] = mProcessIncrDate;
                //    IncrProcessActualDates[1] = mProcessIncrDate;
                //}
            }
            else
            {
                mProcessIncrDate = mProcessIncrDate.AddYears(1);
                IncrProcessActualDates[0] = mProcessIncrDate;
                IncrProcessActualDates[1] = mProcessIncrDate;
            }

            return IncrProcessActualDates;
        }

        //LWP days calculation on attendance LWP days
        public static double LWPDaysCalc(EmployeePayroll OEmployeePayroll, DateTime mFromDate, DateTime mToDate)
        {

            double mLWPDays = 0;
            DateTime mFromPeriod = mFromDate;
            DateTime mEndDate = mToDate.AddMonths(-1);
            String mPeriodRange = "";
            List<string> mPeriod = new List<string>();
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

            mLWPDays = OEmployeePayroll.SalAttendance
                      .Where(e =>   mPeriod.Contains(e.PayMonth))
                      .Sum(m => m.LWPDays - m.ArrearDays);
            
            //mLWPDays = OEmployeePayroll.SalAttendance
            //          .Where(e => Convert.ToDateTime("01/" + e.PayMonth).Date >= mFromDate.Date && Convert.ToDateTime("01/" + e.PayMonth).Date <= mToDate.Date)
            //          .Sum(m => m.LWPDays - m.ArrearDays);
            // Rajgunagar bank LWP leave continus 10 days then increment date increse 10 days
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool exists = System.IO.Directory.Exists(requiredPath);
            string localPath;
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string path = requiredPath + @"\InrementDate" + ".ini";
            localPath = new Uri(path).LocalPath;
            using (var streamReader = new StreamReader(localPath))
            {
                string line;

                while ((line = streamReader.ReadLine()) != null)
                {
                    var comp = line.Split('_')[0];
                    if (comp == "RNSBL")
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            double LvDays = 0;
                            double Lwpdebitdays = Convert.ToDouble(line.Split('_')[1]);
                            var LVCODE = line.Split('_')[2];
                            var EmpLv = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                          .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                          .Include(e => e.LvNewReq.Select(c => c.WFStatus))
                            .Include(e => e.LvNewReq.Select(c => c.FromStat))
                            .Include(e => e.LvNewReq.Select(c => c.ToStat))
                          .Where(e => e.Employee.Id == OEmployeePayroll.Employee.Id).SingleOrDefault();

                            var LvCalendarFilter = EmpLv.LvNewReq.OrderBy(e => e.Id).ToList();

                            var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();

                            var LvReq = EmpLv.LvNewReq.Where(e => e.LeaveHead.LvCode == LVCODE && !LvOrignal_id.Contains(e.Id) && (e.FromDate >= mFromDate || e.FromDate <= mToDate) && e.IsCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2").ToList();
                            foreach (Leave.LvNewReq c in LvReq)
                            {
                                double LvDayscal = 0;
                                if (c.FromDate == mFromDate && c.ToDate == mToDate)
                                {
                                    LvDayscal = (mToDate.Date - mFromDate.Date).Days + 1;
                                }
                                if (c.FromDate <= mFromDate && c.ToDate >= mToDate)
                                {
                                    LvDayscal = (c.ToDate.Value.Date - c.FromDate.Value.Date).Days + 1;
                                }
                                if (c.FromDate <= mFromDate && c.ToDate <= mToDate)
                                {
                                    LvDayscal = (c.ToDate.Value.Date - mFromDate.Date).Days + 1;
                                }
                                if (c.FromDate >= mFromDate && c.ToDate >= mToDate)
                                {
                                    LvDayscal = (mToDate.Date - c.FromDate.Value.Date).Days + 1;
                                }
                                if (c.FromDate >= mFromDate && c.ToDate <= mToDate)
                                {
                                    LvDayscal = (c.ToDate.Value.Date - c.FromDate.Value.Date).Days + 1;
                                }


                                if (c.FromDate == mFromDate && c.FromStat.LookupVal != "FULLSESSION")
                                {
                                    LvDayscal = LvDayscal - 0.5;
                                }
                                if (c.ToDate == mToDate && c.ToStat.LookupVal != "FULLSESSION")
                                {
                                    LvDayscal = LvDayscal - 0.5;
                                }

                                if (LvDayscal < 0)
                                {
                                    LvDayscal = 0;
                                }
                                if (LvDayscal >= Lwpdebitdays)
                                {
                                    LvDays = LvDays + LvDayscal;
                                }
                            }
                            mLWPDays = LvDays;


                        }

                    }
                }
            }

            return mLWPDays;

        }

        public static double DifferenceTotalYears(this DateTime start, DateTime end)
        {
            // Get difference in total months.
            int months = ((end.Year - start.Year) * 12) + (end.Month - start.Month);

            // substract 1 month if end month is not completed
            if (end.Day < start.Day)
            {
                months--;
            }

            double totalyears = months / 12d;
            return totalyears;
        }
        //increment due date fittment as per mid month,quarter mid
        public static DateTime IncrFittmentPolicy(DateTime mRawProcessIncrDate, RegIncrPolicy OIncrPolicyRegular)
        {
            DateTime mProcessIncrDate;
            mProcessIncrDate = mRawProcessIncrDate;

            if (OIncrPolicyRegular.IsMidMonthEffect == true)
            {
                if (mRawProcessIncrDate.Day <= OIncrPolicyRegular.MidMonthLockDay)
                {
                    mProcessIncrDate = Convert.ToDateTime(OIncrPolicyRegular.CurMonStartDay.ToString().PadLeft(2, '0') + "/" + mProcessIncrDate.ToString("MM/yyyy")).Date;
                }
                else
                {
                    mProcessIncrDate = Convert.ToDateTime(OIncrPolicyRegular.NextMonStartDay.ToString().PadLeft(2, '0') + "/" + mProcessIncrDate.AddMonths(1).ToString("MM/yyyy"));
                }
            }
            if (OIncrPolicyRegular.IsMidQuarterEffect == true)
            {
                if (OIncrPolicyRegular.CurrQuarterStart == true)
                {
                    if (mRawProcessIncrDate.Month >= 1 && mRawProcessIncrDate.Month <= 3)
                    {
                        mProcessIncrDate = Convert.ToDateTime("01/01/" + mRawProcessIncrDate.Year.ToString());
                    }
                    else if (mRawProcessIncrDate.Month >= 4 && mRawProcessIncrDate.Month <= 6)
                    {
                        mProcessIncrDate = Convert.ToDateTime("01/04/" + mRawProcessIncrDate.Year.ToString());
                    }
                    else if (mRawProcessIncrDate.Month >= 7 && mRawProcessIncrDate.Month <= 9)
                    {
                        mProcessIncrDate = Convert.ToDateTime("01/07/" + mRawProcessIncrDate.Year.ToString());
                    }
                    else if (mRawProcessIncrDate.Month >= 10 && mRawProcessIncrDate.Month <= 12)
                    {
                        mProcessIncrDate = Convert.ToDateTime("01/10/" + mRawProcessIncrDate.Year.ToString());
                    }

                }
                else
                {
                    if (OIncrPolicyRegular.NextQuarterStart == true)
                    {
                        if (mRawProcessIncrDate.Month >= 1 && mRawProcessIncrDate.Month <= 3)
                        {
                            mProcessIncrDate = Convert.ToDateTime("01/04/" + mRawProcessIncrDate.Year.ToString());
                        }
                        else if (mRawProcessIncrDate.Month >= 4 && mRawProcessIncrDate.Month <= 6)
                        {
                            mProcessIncrDate = Convert.ToDateTime("01/07/" + mRawProcessIncrDate.Year.ToString());
                        }
                        else if (mRawProcessIncrDate.Month >= 7 && mRawProcessIncrDate.Month <= 9)
                        {
                            mProcessIncrDate = Convert.ToDateTime("01/10/" + mRawProcessIncrDate.Year.ToString());
                        }
                        else if (mRawProcessIncrDate.Month >= 10 && mRawProcessIncrDate.Month <= 12)
                        {
                            mProcessIncrDate = Convert.ToDateTime("01/01/" + mRawProcessIncrDate.AddYears(1).Year.ToString());
                        }
                    }
                }
            }
            return mProcessIncrDate;
        }
        //stagnancy checking for staganacy increment, new stagnanat marking
        public static Double[] IncrStagnancyFittmentPolicy(double mOldBasic, IncrementServiceBook OEmpIncrementHistoryStagnancy,
             StagIncrPolicy OIncrPolicyStagRegular, List<BasicScaleDetails> OBasicScale)
        {
            Double[] StagData = new Double[4];
            StagData[0] = mOldBasic; StagData[1] = 0; StagData[2] = 0; ; StagData[3] = 0;


            if (OIncrPolicyStagRegular != null && OEmpIncrementHistoryStagnancy != null)
            {
                if (OEmpIncrementHistoryStagnancy.StagnancyAppl == true && (OEmpIncrementHistoryStagnancy.StagnancyCount != OIncrPolicyStagRegular.MaxStagIncr))
                {

                    if ((DateTime.Now.Year - OEmpIncrementHistoryStagnancy.ProcessIncrDate.Value.Year) == OIncrPolicyStagRegular.SpanYears)
                    {
                        if (OIncrPolicyStagRegular.IsLastIncr == true)
                        {
                            //get scale last increment
                            var ca = OBasicScale.OrderBy(e => e.EndingSlab).LastOrDefault();
                            var mStagIncrAmount = ca.IncrementAmount;
                            var mNewBasic = mOldBasic + mStagIncrAmount;
                            StagData[0] = mNewBasic; //array 0 = newbasic amount
                            var mStagCount = OEmpIncrementHistoryStagnancy.StagnancyCount + 1;
                            StagData[1] = mStagCount; //array 1 = stag count
                            StagData[2] = 1; //array 2 = stag applicable
                            StagData[3] = OIncrPolicyStagRegular.SpanYears; //array 2 = stag applicable span year
                        }
                        else if (OIncrPolicyStagRegular.IsFixAmount == true)
                        {
                            var mNewBasic = mOldBasic + OIncrPolicyStagRegular.IncrAmount;
                            var mStagCount = OEmpIncrementHistoryStagnancy.StagnancyCount + 1;
                            StagData[0] = mNewBasic; //array 0 = newbasic amount
                            StagData[1] = mStagCount; //array 1 = stag count
                            StagData[2] = 1; //array 2 = stag applicable
                            StagData[3] = OIncrPolicyStagRegular.SpanYears; //array 2 = stag applicable span year
                        }
                        //else if (OIncrPolicyStagRegular.IsIncrPercent == true)
                        //{
                        //    var mNewBasic = mOldBasic + mOldBasic * OIncrPolicyStagRegular.IncrPercent;
                        //    var mStagCount = OEmpIncrementHistoryStagnancy.StagnancyCount + 1;
                        //    StagData[0] = mNewBasic; //array 0 = newbasic amount
                        //    StagData[1] = mStagCount; //array 1 = stag count
                        //    StagData[2] = 1; //array 2 = stag applicable
                        //}

                    }
                    else { return null; }
                }
                else //stag appl check
                {
                    var ca = OBasicScale.OrderBy(e => e.EndingSlab).LastOrDefault();
                    var mLastBasic = ca.EndingSlab;
                    if (mOldBasic == mLastBasic)
                    {
                        StagData[0] = mOldBasic; //array 0 = newbasic amount
                        if (OEmpIncrementHistoryStagnancy != null)
                        {
                            StagData[1] = 0; //array 1 = stag count
                        }
                        else
                        {
                            StagData[1] = OEmpIncrementHistoryStagnancy.StagnancyCount; //array 1 = stag count
                        }

                        StagData[2] = 1; //array 2 = stag applicable
                    }
                }
            }
            return StagData;
        }
        //increment process
        public static void IncrementProcess(string Narration,int EmployeePayrollId, int CompanyId, string IncrTypeList,
            DateTime? mProcessIncrementDate, bool mPopUpAutoIncr, bool mRegularIncrDataEntry)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var delme = 2;
                //Company OCompanyPayroll = db.Company
                //        .Include(e => e.PayScale)
                //        .Include(e => e.PayScaleAgreement)
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrList)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy.IncrPolicyDetails)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy.NonRegIncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy.RegIncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.StagIncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.PayScale))
                //        .Where(r => r.Id == CompanyId).SingleOrDefault();
                // delme = 2;

                Company OCompanyPayroll = db.Company.Where(r => r.Id == CompanyId).SingleOrDefault();
                List<PayScale> PayScale = db.PayScale.Where(e => e.Company_Id == CompanyId).ToList();
                OCompanyPayroll.PayScale = PayScale;
                foreach (var PayScaleitem in PayScale)
                {
                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.PayScale_Id == PayScaleitem.Id).SingleOrDefault();
                    List<IncrActivity> IncrActivity2 = db.PayScaleAgreement.Where(e => e.PayScale_Id == PayScaleitem.Id).Select(e => e.IncrActivity.ToList()).SingleOrDefault();
                    foreach (var IncrActivityitem in IncrActivity2)
                    {
                        IncrPolicy IncrPolicy2 = db.IncrPolicy.Where(e => e.Id == IncrActivityitem.IncrPolicy_Id).SingleOrDefault();
                        IncrActivityitem.IncrPolicy = IncrPolicy2;
                        IncrPolicyDetails IncrPolicyDetails2 = db.IncrPolicyDetails.Where(e => e.Id == IncrPolicy2.IncrPolicyDetails_Id).SingleOrDefault();
                        NonRegIncrPolicy NonRegIncrPolicy = db.NonRegIncrPolicy.Where(e => e.Id == IncrPolicy2.NonRegIncrPolicy_Id).SingleOrDefault();
                        RegIncrPolicy RegIncrPolicy = db.RegIncrPolicy.Where(e => e.Id == IncrPolicy2.RegIncrPolicy_Id).SingleOrDefault();
                        StagIncrPolicy StagIncrPolicy2 = db.StagIncrPolicy.Where(e => e.Id == IncrActivityitem.StagIncrPolicy_Id).SingleOrDefault();
                        IncrPolicy2.IncrPolicyDetails = IncrPolicyDetails2;
                        IncrPolicy2.NonRegIncrPolicy = NonRegIncrPolicy;
                        IncrPolicy2.RegIncrPolicy = RegIncrPolicy;
                        IncrActivityitem.IncrPolicy = IncrPolicy2;
                        IncrActivityitem.StagIncrPolicy = StagIncrPolicy2;
                        LookupValue IncrList = db.LookupValue.Where(e => e.Id == IncrActivityitem.IncrList_Id).SingleOrDefault();
                        IncrActivityitem.IncrList = IncrList;
                    }
                    PayScaleAgreement.IncrActivity = IncrActivity2;
                    PayScaleAgreement.PayScale = PayScaleitem;
                    OCompanyPayroll.PayScaleAgreement.Add(PayScaleAgreement);
                }


                delme = 2;



                //EmployeePayroll OEmployeePayroll = new EmployeePayroll();
                //OEmployeePayroll
                //= db.EmployeePayroll
                //    .Include(e => e.Employee)
                //    .Include(e => e.Employee.ServiceBookDates)
                //    .Include(e => e.EmpSalStruct)
                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                //    .Include(e => e.IncrementServiceBook)
                //    .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                //    .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.StagIncrPolicy))
                //    .Include(e => e.IncrDataCalc)

                //    .Where(e => e.Id == EmployeePayrollId).SingleOrDefault();

                delme = 2;
                EmployeePayroll OEmployeePayroll = new EmployeePayroll();
                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == EmployeePayrollId).SingleOrDefault();
                Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = Employee;
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                Employee.ServiceBookDates = ServiceBookDates;
                //EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).OrderByDescending(e => e.Id).FirstOrDefault();03/02/2024
                EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EndDate==null).SingleOrDefault();
                OEmployeePayroll.EmpSalStruct.Add(EmpSalStruct);
               // OEmployeePayroll.EmpSalStruct.Add(EmpSalStruct);
                List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStruct.Id).ToList();
                EmpSalStruct.EmpSalStructDetails = EmpSalStructDetails;
                foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                {
                    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                    PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsitem.PayScaleAssignment_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.PayScaleAssignment = PayScaleAssignment;
                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAssignment.PayScaleAgreement_Id).SingleOrDefault();
                    PayScaleAssignment.PayScaleAgreement = PayScaleAgreement;

                }
                IncrDataCalc IncrDataCalc = db.IncrDataCalc.Where(e => e.Id == OEmployeePayroll.IncrDataCalc_Id).SingleOrDefault();
                OEmployeePayroll.IncrDataCalc = IncrDataCalc;

                List<IncrementServiceBook> IncrementServiceBook = db.IncrementServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.IncrementServiceBook = IncrementServiceBook;
                foreach (var IncrementServiceBookitem in IncrementServiceBook)
                {
                    IncrActivity IncrActivity1 = db.IncrActivity.Where(e => e.Id == IncrementServiceBookitem.IncrActivity_Id).SingleOrDefault();
                    IncrementServiceBookitem.IncrActivity = IncrActivity1;
                    LookupValue IncrList = db.LookupValue.Where(e => e.Id == IncrActivity1.IncrList_Id).SingleOrDefault();
                    IncrActivity1.IncrList = IncrList;
                    StagIncrPolicy StagIncrPolicy1 = db.StagIncrPolicy.Where(e => e.Id == IncrActivity1.StagIncrPolicy_Id).SingleOrDefault();
                    IncrActivity1.StagIncrPolicy = StagIncrPolicy1;

                }



                ////new incr employee policies
                //var EmpIncrPolicyStruct = db.EmployeePolicyStruct.Where(e => e.EmployeePayroll_Id == EmployeePayrollId && e.EndDate == null)
                //    .SingleOrDefault();

                //var EmpIncrPolicyStructDetails = db.EmployeePolicyStructDetails.Where(e => e.EmployeePolicyStruct_Id == EmpIncrPolicyStruct.Id)
                //   .ToList();
                //EmpIncrPolicyStruct.EmployeePolicyStructDetails = EmpIncrPolicyStructDetails;
                //foreach (var EmpIncrPolRecord in EmpIncrPolicyStructDetails)
                //{
                //    //PolicyAssignment contain policyname
                //    var PolicyAssignment = db.PolicyAssignment.Where(e =>e.Id == EmpIncrPolRecord.PolicyAssignment_Id).SingleOrDefault();
                //    var PolicyFormula = db.PolicyFormula.Where(e => e.Id == EmpIncrPolRecord.PolicyFormula_Id).SingleOrDefault();
                //    PolicyFormula.IncrActivity = db.IncrActivity.Where(e=> e.PolicyFormula.Contains(EmpIncrPolRecord.PolicyFormula_Id)).SingleOrDefault();
                //    var incracti=db.IncrActivity.Where(e=>e.p== PolicyFormula.)
                //    EmpIncrPolRecord.PolicyAssignment = PolicyAssignment;
                //    EmpIncrPolRecord.PolicyFormula = PolicyFormula;


                //}
                //var EmpIncrPolicyAssignment=db.PolicyAssignment.Where(e=>e.PolicyName== EmpIncrPolicyStructDetails)


                EmpSalStruct OEmpSalStruct = new EmpSalStruct();
                OEmpSalStruct = OEmployeePayroll.EmpSalStruct.SingleOrDefault();
                //if (OEmployeePayroll.Employee.ServiceBookDates.ResignationDate != null || OEmployeePayroll.Employee.ServiceBookDates.RetirementDate != null)
                //{
                //    OEmpSalStruct = OEmployeePayroll.EmpSalStruct.LastOrDefault();
                //}
                //else
                //{
                //    OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                //}


                var OSalaryHead = OEmpSalStruct.EmpSalStructDetails
                        .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                        .SingleOrDefault();
                //GRADEPAY
                var OSalaryHead2 = new EmpSalStructDetails();
                var oCompanyId = int.Parse(SessionManager.CompanyId);
                var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();

                var OSalHeadFormulaResult = OSalaryHead.SalHeadFormula;
                List<BasicScaleDetails> OBasicScale = null;
                if (OSalHeadFormulaResult != null)
                {

                    //var OSalHeadFormula = db.SalHeadFormula
                    //                    .Include(e => e.BASICDependRule)
                    //                    .Include(e => e.BASICDependRule.BasicScale)
                    //                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                    //                    .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();
                    var OSalHeadFormula = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();
                    BASICDependRule BASICDependRule = db.BASICDependRule.Where(e => e.Id == OSalHeadFormula.BASICDependRule_Id).SingleOrDefault();
                    OSalHeadFormula.BASICDependRule = BASICDependRule;
                    BasicScale BasicScale = db.BasicScale.Where(e => e.Id == BASICDependRule.BasicScale_Id).SingleOrDefault();
                    BASICDependRule.BasicScale = BasicScale;
                    List<BasicScaleDetails> BasicScaleDetails = db.BasicScale.Where(e => e.Id == BASICDependRule.BasicScale_Id).Select(e => e.BasicScaleDetails.ToList()).SingleOrDefault();
                    BasicScale.BasicScaleDetails = BasicScaleDetails;



                    OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                              .Select(t => new BasicScaleDetails
                                              {
                                                  StartingSlab = t.StartingSlab,
                                                  EndingSlab = t.EndingSlab,
                                                  IncrementAmount = t.IncrementAmount,
                                                  IncrementCount = t.IncrementCount,
                                                  EBMark = t.EBMark,
                                              }).ToList();
                }
                double mOldBasic = 0;

                double oAmount = 0;
                if (OBasicScale != null)
                {

                    mOldBasic = OSalaryHead.Amount;

                    if (CompCode.ToUpper() == "ACABL")
                    {
                        oAmount = OEmpSalStruct.EmpSalStructDetails
                        .Where(r => r.SalaryHead.Code.ToUpper() == "GRADEPAY").Select(e => e.Amount)
                        .SingleOrDefault();
                    }
                    // mOldBasic += oAmount;

                }
                else
                {
                    mOldBasic = OSalaryHead.Amount;
                }
                int OPayscalAgreementId = OSalaryHead.PayScaleAssignment.PayScaleAgreement.Id;
                var OCompanyAgreement = OCompanyPayroll.PayScaleAgreement.Where(e => e.Id == OPayscalAgreementId).SingleOrDefault();


                //var OIncrPolicy = OCompanyAgreement.IncrActivity
                //                    .Where(r => r.IncrList.LookupVal.ToUpper() == "REGULAR")
                //                    .SingleOrDefault();
                var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity.Select(t => t.IncrList)))
                   .Where(e => e.EndDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault().EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity).ToList();

                var OIncrPolicy = test.SelectMany(e => e).Where(e => e.IncrList.LookupVal.ToUpper() == "REGULAR").SingleOrDefault();

                //var OIncrPolicyOther = OCompanyAgreement.IncrActivity
                //                    .Where(r => r.IncrList.LookupVal.ToUpper() == IncrTypeList.ToUpper())
                //                    .SingleOrDefault();


                var OIncrPolicyOther = test.SelectMany(e => e)
                                    .Where(r => r.IncrList.LookupVal.ToUpper() == IncrTypeList.ToUpper())
                                    .SingleOrDefault();

                IncrPolicyDetails OIncrPolicyOthers = OIncrPolicyOther.IncrPolicy.IncrPolicyDetails;

                StagIncrPolicy OIncrPolicyStagOther = OIncrPolicyOther.StagIncrPolicy;


                var OEmpIncrementHistoryRegular = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                            .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR" ||
                                                (e.IncrActivity.IncrList.LookupVal.ToUpper() == "ONPROMOTION" && e.IsRegularIncrDate == true))
                                            .FirstOrDefault();
                var OEmpIncrementHistoryStagnancy = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                            .Where(e => e.StagnancyAppl == true)
                                            .FirstOrDefault();
                var OEmpIncrementHistory = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                            .FirstOrDefault();



                //int OPayscalAgreementId = OSalaryHead.PayScaleAssignment.PayScaleAgreement.Id;
                //var OCompanyAgreement = OCompanyPayroll.PayScaleAgreement.Where(e => e.Id == OPayscalAgreementId).SingleOrDefault();


                ////var OIncrPolicy = OCompanyAgreement.IncrActivity
                ////                    .Where(r => r.IncrList.LookupVal.ToUpper() == "REGULAR")
                ////                    .SingleOrDefault();
                ////var test = db.EmployeePolicyStruct
                ////    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity.Select(t => t.IncrList)))
                ////   .Where(e => e.EndDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault().EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity).ToList();

                //var EmployeePolicyStruct = db.EmployeePolicyStruct.Where(e => e.EndDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault();
                //var md = 383838;
                //EmployeePolicyStructDetails EmployeePolicyStructDetails = db.EmployeePolicyStructDetails.Include(r=>r.PolicyName).Where(e => e.EmployeePolicyStruct_Id == EmployeePolicyStruct.Id && e.PolicyName.LookupVal.ToUpper()=="INCREMENT").SingleOrDefault();
                //md = 373737;
                //PolicyFormula PolicyFormula = db.PolicyFormula.Where(e => e.Id == EmployeePolicyStructDetails.PolicyFormula_Id).FirstOrDefault();
                //md = 373737;
                //LookupValue IncrListData = db.LookupValue.Where(e => e.Id == IncrActivity1.IncrList_Id).SingleOrDefault();
                //List<IncrActivity> IncrActivityList = db.IncrActivity.Where(e => e.IncrList_Id== .Id == PolicyFormula.Id).Select(e=>e.IncrActivity).SingleOrDefault();
                //md = 373737;
                //IncrActivity IncrActivity= db.IncrActivity.Include(e=>e.IncrList).Where(e => IncrActivityList.Contains(e.Id) && e.IncrList.LookupVal.ToUpper() == "REGULAR").SingleOrDefault();
                //md = 373737;
                //IncrPolicy IncrPolicy = db.IncrPolicy.Where(e => e.Id == IncrActivity.IncrPolicy_Id).SingleOrDefault();
                //md = 373737;
                //IncrPolicyDetails IncrPolicyDetails = db.IncrPolicyDetails.Where(e => e.Id == IncrPolicy.IncrPolicyDetails_Id).SingleOrDefault();
                //md = 373737;
                //StagIncrPolicy StagIncrPolicy = db.StagIncrPolicy.Where(e => e.Id == IncrActivity.StagIncrPolicy_Id).SingleOrDefault();
                //IncrPolicy.IncrPolicyDetails = IncrPolicyDetails;
                //IncrActivity.IncrPolicy = IncrPolicy;
                //IncrActivity.StagIncrPolicy = StagIncrPolicy;


                //var OIncrPolicy = IncrActivity;//.Where(e => e.IncrList.LookupVal.ToUpper() == "REGULAR").SingleOrDefault();

                ////var OIncrPolicyOther = OCompanyAgreement.IncrActivity
                ////                    .Where(r => r.IncrList.LookupVal.ToUpper() == IncrTypeList.ToUpper())
                ////                    .SingleOrDefault();
                ////Other Increment activities
                //IncrActivity OtherIncrAcitity = IncrActivityList.Where(r => r.IncrList.LookupVal.ToUpper() == IncrTypeList.ToUpper()).SingleOrDefault();
                //IncrPolicy OIncrPolicyOther = db.IncrPolicy.Where(e => e.Id == OtherIncrAcitity.IncrPolicy_Id).SingleOrDefault();
                //IncrPolicyDetails OIncrPolicyOthers = db.IncrPolicyDetails.Where(e => e.Id == OIncrPolicyOther.IncrPolicyDetails_Id).SingleOrDefault();
                //StagIncrPolicy OIncrPolicyStagOther = db.StagIncrPolicy.Where(e => e.Id == OtherIncrAcitity.StagIncrPolicy_Id).SingleOrDefault();

                //                //var OIncrPolicyOther = IncrActivityList
                //                //                    .Where(r => r.IncrList.LookupVal.ToUpper() == IncrTypeList.ToUpper())
                //                //                    .SingleOrDefault();

                ////IncrPolicyDetails OIncrPolicyOthers = OIncrPolicyOther.IncrPolicy.IncrPolicyDetails;

                ////StagIncrPolicy OIncrPolicyStagOther = OIncrPolicyOther.StagIncrPolicy;


                //var OEmpIncrementHistoryRegular = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.Id).ToList()
                //                            .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR" ||
                //                                (e.IncrActivity.IncrList.LookupVal.ToUpper() == "ONPROMOTION" && e.IsRegularIncrDate == true))
                //                            .FirstOrDefault();
                //var OEmpIncrementHistoryStagnancy = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.Id).ToList()
                //                            .Where(e => e.StagnancyAppl == true)
                //                            .FirstOrDefault();
                //var OEmpIncrementHistory = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.Id).ToList()
                //                            .FirstOrDefault();


                double mNewBasic = 0;
                double mNewBasic1 = 0;

                if (mPopUpAutoIncr == true)//from pop screen process
                {//verify
                    var OIncrDataCalc = OEmployeePayroll.IncrDataCalc;
                    /*
                            * Round Function 10rs Rounding
                            */
                    if (CompCode.ToUpper() == "ACABL")
                    {
                        if (mNewBasic > 0 && mNewBasic % 10 != 0)
                        {
                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                            if (OBasicScale != null)
                            {
                                Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                if (mNewBasic >= acblendslab)
                                {
                                    mNewBasic = acblendslab;
                                }
                            }
                        }
                    }
                    SaveIncrementServiceBook(Narration,OEmployeePayroll.Id, OIncrDataCalc.IncrementActivity.Id, OIncrDataCalc.ProcessIncrDate.Value, OIncrDataCalc.OrignalIncrDate.Value,
                        OIncrDataCalc.OldBasic, OIncrDataCalc.NewBasic, OIncrDataCalc.StagnancyAppl, OIncrDataCalc.StagnancyCount, true);

                }
                else
                {
                    switch (IncrTypeList.ToUpper())
                    {
                        case "REGULAR":
                            RegIncrPolicy OIncrPolicyRegular = null;
                            NonRegIncrPolicy OIncrPolicyNonRegular = null;
                            StagIncrPolicy OIncrPolicyStagRegular = null;
                            IncrActivity OIncrActivity = new IncrActivity();

                            if (OIncrPolicy != null)
                            {
                                OIncrPolicyRegular = OIncrPolicy.IncrPolicy.RegIncrPolicy;
                                OIncrPolicyNonRegular = OIncrPolicy.IncrPolicy.NonRegIncrPolicy;
                                OIncrPolicyStagRegular = OIncrPolicy.StagIncrPolicy;
                                OIncrActivity = OIncrPolicy;

                            }
                            if (mRegularIncrDataEntry == false)//take from increment date incrdatacalc
                            {
                                var OIncrDataCalc = OEmployeePayroll.IncrDataCalc;
                                /*
                            * Round Function 10rs Rounding
                            */
                                if (CompCode.ToUpper() == "ACABL")
                                {
                                    if (mNewBasic > 0 && mNewBasic % 10 != 0)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                        if (OBasicScale != null)
                                        {
                                            Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                            if (mNewBasic >= acblendslab)
                                            {
                                                mNewBasic = acblendslab;
                                            }
                                        }
                                    }
                                }
                                SaveIncrementServiceBook(Narration,OEmployeePayroll.Id, OIncrDataCalc.IncrementActivity.Id, OIncrDataCalc.ProcessIncrDate.Value.Date, OIncrDataCalc.OrignalIncrDate.Value.Date,
                                    OIncrDataCalc.OldBasic, OIncrDataCalc.NewBasic, OIncrDataCalc.StagnancyAppl, OIncrDataCalc.StagnancyCount, true);

                            }
                            else //take it as input
                            {
                                //var OIncrPolicy = OCompanyPayroll.PayScaleAgreement
                                //    .Select(e => e.IncrActivity
                                //        .Where(r => r.IncrList.LookupVal.ToUpper() == "REGULAR"))
                                //        .LastOrDefault();
                                //RegIncrPolicy OIncrPolicyRegular = OIncrPolicy
                                //                                    .Select(r => r.IncrPolicy.RegIncrPolicy)
                                //                                    .SingleOrDefault();
                                //var OIncrActivity = OIncrPolicy.SingleOrDefault();

                                //NonRegIncrPolicy OIncrPolicyNonRegular = OIncrPolicy
                                //                                    .Select(r => r.IncrPolicy.NonRegIncrPolicy)
                                //                                    .SingleOrDefault();

                                //StagIncrPolicy OIncrPolicyStagRegular = OIncrPolicy
                                //                                  .Select(r => r.StagIncrPolicy)
                                //                                  .SingleOrDefault();

                                Double[] mStagnancyData = new Double[3];

                                mStagnancyData = IncrementRegularManualDateDataCalc(OCompanyPayroll, OEmployeePayroll, OBasicScale, OIncrPolicyRegular,
                                    OIncrPolicyNonRegular, OIncrPolicyStagRegular, mOldBasic, mProcessIncrementDate);

                                if (mStagnancyData != null)
                                {
                                    mNewBasic = mStagnancyData[0];
                                    bool mStagBool = false;
                                    int mstagCount = Convert.ToInt16(mStagnancyData[1]);
                                    if (mStagnancyData[2] == 0)
                                    {
                                        mStagBool = false;
                                    }
                                    else
                                    {
                                        mStagBool = true;
                                    }
                                    /*
                            * Round Function 10rs Rounding
                            */

                                    if (CompCode.ToUpper() == "ACABL")
                                    {
                                        if (mNewBasic > 0 && mNewBasic % 10 != 0)
                                        {
                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(Convert.ToInt32(mNewBasic) / 10).ToString() + "0") + 10;
                                            if (OBasicScale != null)
                                            {
                                                Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                if (mNewBasic >= acblendslab)
                                                {
                                                    mNewBasic = acblendslab;
                                                }
                                            }
                                            mNewBasic = mNewBasic - oAmount;
                                            mOldBasic = OSalaryHead.Amount;
                                        }
                                        if (OIncrPolicy != null)
                                        {
                                            IncrPolicyDetails OIncrPolDet = OIncrPolicy.IncrPolicy.IncrPolicyDetails;


                                            if (OIncrPolDet.IncrPercent > 0)
                                            {
                                                mNewBasic = mOldBasic + ((mOldBasic + oAmount) / 100 * OIncrPolDet.IncrPercent);

                                                //mNewBasic = Math.Round(mNewBasic1 + 0.01, 0);

                                                //Math.Round(mNewBasic + 0.01, 0);
                                                string newb = mNewBasic.ToString("0.00");
                                                string a1 = mNewBasic.ToString().Split('.')[0];
                                                int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                int b = Convert.ToInt32(newb.Split('.')[1]);



                                                if (mNewBasic > 0 && mNewBasic % 10 != 0)
                                                {
                                                    if (b > 0 && a == 0)
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                    }
                                                    else if (b > 0 && a < 5)
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                    }
                                                    else if (b == 0 && a <= 5)
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                    }
                                                    else
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                    }
                                                    // mNewBasic = Convert.ToInt32(Convert.ToInt32(Convert.ToInt32(mNewBasic) / 10).ToString() + "0") + 10;
                                                    if (OBasicScale != null)
                                                    {
                                                        Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                        if (mNewBasic >= acblendslab)
                                                        {
                                                            mNewBasic = acblendslab;
                                                        }
                                                    }
                                                }

                                            }

                                        }

                                    }
                                    SaveIncrementServiceBook(Narration,OEmployeePayroll.Id, OIncrActivity.Id, mProcessIncrementDate, mProcessIncrementDate,
                                    mOldBasic, mNewBasic, mStagBool, mstagCount, false);
                                }


                            }
                            break;
                        default://other increments like qualification, request , onpromotion etc
                            //get new basic by passing increment steps
                            //get increment date as input
                            Double[] mStagData = new double[4];
                            mStagData[0] = mOldBasic; mStagData[1] = 0; mStagData[2] = 0; mStagData[3] = 0;
                            mNewBasic = mOldBasic;
                            var OIncrPolicyOthersDetails = OIncrPolicyOthers;
                            if (OIncrPolicyOthersDetails.IsIncrSteps == true)
                            {//check it
                                mNewBasic = BasicSelector(OIncrPolicyOthersDetails.IncrSteps, OBasicScale, mOldBasic);//other increment

                                mStagData[0] = mNewBasic; mStagData[1] = 0; mStagData[2] = 0; mStagData[3] = 0;
                                //check it
                                mStagData = IncrStagnancyFittmentPolicy(mNewBasic, OEmpIncrementHistoryStagnancy, OIncrPolicyStagOther, OBasicScale);
                                if (mStagData != null)
                                {
                                    mNewBasic = mStagData[0];

                                }
                                else
                                {
                                    mNewBasic = 0;
                                }
                            }
                            else if (OIncrPolicyOthersDetails.IsIncrAmount == true)
                            {
                                mNewBasic = mOldBasic + OIncrPolicyOthersDetails.IncrAmount;
                            }
                            else if (OIncrPolicyOthersDetails.IsIncrPercent == true)
                            {
                                mNewBasic = Math.Round(mOldBasic + (OIncrPolicyOthersDetails.IncrPercent * mNewBasic / 100), 0);
                            }
                            /*
                             * Round Function 10rs Rounding
                             */
                            if (CompCode.ToUpper() == "ACABL")
                            {
                                if (mNewBasic > 0 && mNewBasic % 10 != 0)
                                {
                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(Convert.ToInt32(mNewBasic) / 10).ToString() + "0") + 10;
                                    if (OBasicScale != null)
                                    {
                                        Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                        if (mNewBasic >= acblendslab)
                                        {
                                            mNewBasic = acblendslab;
                                        }
                                    }
                                    mNewBasic = mNewBasic - oAmount;
                                    mOldBasic = OSalaryHead.Amount;
                                }
                            }

                            if (mNewBasic > 0)
                            {
                                mStagData[0] = mNewBasic;
                                bool mStagBool1 = false;
                                int mstagCount1 = Convert.ToInt16(mStagData[1]);
                                if (mStagData[2] == 0) { mStagBool1 = false; } else { mStagBool1 = true; }
                                //check it   
                                SaveIncrementServiceBook(Narration,OEmployeePayroll.Id, OIncrPolicyOther.Id, mProcessIncrementDate, mProcessIncrementDate,
                                        mOldBasic, mNewBasic, mStagBool1, mstagCount1, false);
                            }
                            break;

                    }
                }
            }
        }
        //  private static Object Employeepayrollfilter;
        public static void SaveIncrementServiceBook(string Narration,Int32? OEmployeePayrollId, Int32? OIncrActivity,
                            DateTime? mProcessIncrDate, DateTime? mOrigionalIncrDate, double mOldBasic, double mNewBasic, bool mStagnancyAppl, int mStagnancyCount, bool IsRegIncr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmployeePayroll
                    = db.EmployeePayroll
                        .Include(e => e.Employee)
                     .Include(e => e.Employee.GeoStruct)
                     .Include(e => e.Employee.PayStruct)
                     .Include(e => e.Employee.FuncStruct)
                     .Include(e => e.SalAttendance)
                     .Include(e => e.Employee.ServiceBookDates)
                    //.Include(e => e.EmpSalStruct)
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                        .Include(e => e.IncrementServiceBook)
                        .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                        .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.StagIncrPolicy))
                        .Include(e => e.IncrDataCalc)
                    //.Include(e => e.PromotionServiceBook)
                    //.Include(e => e.TransferServiceBook)
                    //.Include(e => e.OtherServiceBook)
                        .Where(e => e.Id == OEmployeePayrollId).SingleOrDefault();
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                IncrementServiceBook OIncrementServiceBook = new IncrementServiceBook()
                {
                    FuncStruct = OEmployeePayroll.Employee.FuncStruct,
                    PayStruct = OEmployeePayroll.Employee.PayStruct,
                    GeoStruct = OEmployeePayroll.Employee.GeoStruct,
                    // IncrActivity = db.IncrActivity.Find(OIncrActivity.Id),
                    ProcessIncrDate = mProcessIncrDate,
                    OrignalIncrDate = mOrigionalIncrDate,
                    NewBasic = mNewBasic,
                    OldBasic = mOldBasic,
                    StagnancyAppl = mStagnancyAppl,
                    StagnancyCount = mStagnancyCount,
                    DBTrack = dbt,
                    ReleaseDate = null,
                    IsRegularIncrDate = IsRegIncr,
                    Narration=Narration

                };
                try
                {

                    OIncrementServiceBook.IncrActivity = db.IncrActivity.Find(OIncrActivity);
                    db.IncrementServiceBook.Add(OIncrementServiceBook);

                    db.SaveChanges();


                    OEmployeePayroll.IncrementServiceBook.Add(OIncrementServiceBook);//add ref
                    //db.EmployeePayroll(OEmployeePayroll)
                    db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);


                }
                catch (Exception e)
                {

                    throw (e);

                }
            }
        }

        //increment release process. Employee structure will be updated
        public static void IncrementReleaseProcess(Int32? EmployeePayrollId, Int32? OIncrementServiceBook, DateTime? mReleaseDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var OISSave = db.IncrementServiceBook.Find(OIncrementServiceBook);
                    OISSave.ReleaseFlag = true;
                    OISSave.IsHold = false;
                    OISSave.ReleaseDate = mReleaseDate;
                    db.IncrementServiceBook.Attach(OISSave);
                    db.Entry(OISSave).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    var a = db.EmployeePayroll.Include(e => e.IncrDataCalc).Where(e => e.Id == EmployeePayrollId).SingleOrDefault();
                    if (a != null && a.IncrDataCalc != null)
                    {
                        db.IncrDataCalc.Remove(a.IncrDataCalc);
                        db.SaveChanges();
                    }

                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);

                    //db.LWFTransT.Attach(OSalaryTChk.LWFTransT);
                    //db.Entry(OSalaryTChk.LWFTransT).State = System.Data.Entity.EntityState.Deleted;
                    //var OEPSave = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                    //write changes in employee master data
                    EmployeeMasterUpdation(EmployeePayrollId, "INCREMENT", null, null, null, OIncrementServiceBook, null);

                    ServiceBookEmpSalStructChange(EmployeePayrollId, OISSave.ProcessIncrDate.Value.Date, OISSave.NewBasic, "INCREMENT", null, null, null, OIncrementServiceBook);


                }
                catch (Exception ex)
                {
                    throw (ex);
                }

            }
        }

        public static void ExtnRednReleaseProcess(Int32? EmployeePayrollId, Int32? OExtnRednServiceBook, DateTime? mReleaseDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int ExtnRednCount = 0;
                    ExtnRednServiceBook OExReServBook = db.ExtnRednServiceBook.Include(e => e.ExtnRednActivity)
                        .Include(e => e.ExtnRednActivity.ExtnRednPolicy).Where(e => e.EmployeePayroll_Id == EmployeePayrollId && e.Id == OExtnRednServiceBook).FirstOrDefault();
                    if (OExReServBook.ExtnRednActivity.ExtnRednPolicy.IsExtn == true)
                    {
                        ExtnRednServiceBook OExt = db.ExtnRednServiceBook.Where(e => e.EmployeePayroll_Id == EmployeePayrollId && e.ExtnRednActivity.ExtnRednPolicy.IsExtn == true && e.ReleaseFlag == true).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (OExt != null)
                        {
                            ExtnRednCount = OExt.ExtnRednCount;

                        }
                    }
                    else
                    {
                        ExtnRednServiceBook OExt = db.ExtnRednServiceBook.Where(e => e.EmployeePayroll_Id == EmployeePayrollId && e.ExtnRednActivity.ExtnRednPolicy.IsRedn == true && e.ReleaseFlag == true).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (OExt != null)
                        {
                            ExtnRednCount = OExt.ExtnRednCount;

                        }
                    }


                    var OISSave = db.ExtnRednServiceBook.Find(OExtnRednServiceBook);
                    OISSave.ReleaseFlag = true;
                    OISSave.IsHold = false;
                    OISSave.ReleaseDate = mReleaseDate;
                    OISSave.ExtnRednCount = ExtnRednCount + 1;
                    db.ExtnRednServiceBook.Attach(OISSave);
                    db.Entry(OISSave).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();


                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);

                    //db.LWFTransT.Attach(OSalaryTChk.LWFTransT);
                    //db.Entry(OSalaryTChk.LWFTransT).State = System.Data.Entity.EntityState.Deleted;
                    //var OEPSave = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                    //write changes in employee master data
                    EmployeeMasterUpdation(EmployeePayrollId, "EXTNREDN", null, null, null, null, OExtnRednServiceBook);

                    //ServiceBookEmpSalStructChange(EmployeePayrollId, OISSave.ProcessIncrDate.Value.Date, OISSave.NewBasic, "INCREMENT", null, null, null, OIncrementServiceBook);


                }
                catch (Exception ex)
                {
                    throw (ex);
                }

            }
        }
        //changes due service book activities in Employee salary structure
        public static void ServiceBookEmpSalStructChange(Int32? EmployeePayrollId, DateTime mEffectiveDate, double mNewBasic, string mActivityType, Int32? OPromotionServiceBook_id, Int32? OTransferServiceBook_id, Int32? OOtherServiceBook_id, Int32? OIncrementServiceBook_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmpSalStruct OEmpSalStructCurrent = null;
                //var OEmployeePayroll
                //  = db.EmployeePayroll
                //    //.Include(e => e.Employee)
                //    //.Include(e => e.Employee.EmpOffInfo)
                //    //.Include(e => e.Employee.GeoStruct)
                //    //.Include(e => e.Employee.PayStruct)
                //    //.Include(e => e.Employee.FuncStruct)
                //    //.Include(e => e.SalAttendance)
                //    //.Include(e => e.Employee.ServiceBookDates)
                //    //.Include(e => e.EmpSalStruct)
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula.FormulaType)))
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.RoundingMethod)))
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment)))
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula)))//added by prashant 13/4/17
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(a => a.FuncStruct))))//added by prashant 13/4/17
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(a => a.GeoStruct))))//added by prashant 13/4/17
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(a => a.PayStruct))))//added by prashant 13/4/17
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(x => x.FormulaType))))//added by prashant 13/4/17
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //    //.Include(e => e.IncrementServiceBook)
                //    //.Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                //    //.Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.StagIncrPolicy))//.AsNoTracking()
                //     .Where(e => e.Id == EmployeePayrollId).SingleOrDefault();



                ////IEnumerable<EmpSalStructDetails> OEmpSalStructDetailsCurrent=null;
                switch (mActivityType)
                {
                    case "INCREMENT":
                        //      var OEmployeePayrollForincr
                        //= db.EmployeePayroll
                        //    .Include(e => e.Employee)

                        //    .Include(e => e.Employee.ServiceBookDates)
                        //    .Include(e => e.EmpSalStruct)

                        //    .Include(e => e.IncrementServiceBook)
                        //    .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                        //    .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.StagIncrPolicy))//.AsNoTracking()
                        //   .Where(e => e.Id == EmployeePayrollId).SingleOrDefault();
                        var OEmployeePayrollForincr
                        = db.EmployeePayroll.Where(e => e.Id == EmployeePayrollId).SingleOrDefault();
                        Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayrollForincr.Employee_Id).SingleOrDefault();
                        OEmployeePayrollForincr.Employee = Employee;
                        ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                        OEmployeePayrollForincr.Employee.ServiceBookDates = ServiceBookDates;
                        List<EmpSalStruct> EmpSalStructList = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayrollForincr.Id).ToList();
                        OEmployeePayrollForincr.EmpSalStruct = EmpSalStructList;
                        List<IncrementServiceBook> IncrementServiceBookList = db.IncrementServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayrollForincr.Id).ToList();
                        OEmployeePayrollForincr.IncrementServiceBook = IncrementServiceBookList;
                        foreach (var IncrementServiceBookListitem in IncrementServiceBookList)
                        {
                            IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == IncrementServiceBookListitem.IncrActivity_Id).SingleOrDefault();
                            IncrementServiceBookListitem.IncrActivity = IncrActivity;
                            StagIncrPolicy StagIncrPolicy = db.StagIncrPolicy.Where(e => e.Id == IncrActivity.StagIncrPolicy_Id).SingleOrDefault();
                            IncrementServiceBookListitem.IncrActivity.StagIncrPolicy = StagIncrPolicy;
                        }



                        //checking for increment date is exist in empsalstruct or new increment date
                        //OEmpSalStructCurrent = OEmployeePayrollForincr.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                        if (OEmployeePayrollForincr.Employee.ServiceBookDates.ResignationDate != null || OEmployeePayrollForincr.Employee.ServiceBookDates.RetirementDate != null)
                        {
                            OEmpSalStructCurrent = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId).OrderByDescending(e => e.EffectiveDate)
                                                      .FirstOrDefault();
                            List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStructCurrent.Id).ToList();
                            OEmpSalStructCurrent.EmpSalStructDetails = EmpSalStructDetailsList;
                            GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStructCurrent.GeoStruct_Id).SingleOrDefault();
                            OEmpSalStructCurrent.GeoStruct = GeoStruct;
                            FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStructCurrent.FuncStruct_Id).SingleOrDefault();
                            OEmpSalStructCurrent.FuncStruct = FuncStruct;
                            PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStructCurrent.PayStruct_Id).SingleOrDefault();
                            OEmpSalStructCurrent.PayStruct = PayStruct;
                            foreach (var EmpSalStructDetailsListitem in EmpSalStructDetailsList)
                            {
                                SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsListitem.SalaryHead_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalaryHead = SalaryHead;
                                LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalaryHead.RoundingMethod = RoundingMethod;
                                SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsListitem.SalHeadFormula_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalHeadFormula = SalHeadFormula;
                                PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.PayScaleAssignment = PayScaleAssignment;

                            }
                        }
                        else
                        {
                            OEmpSalStructCurrent = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId).Where(e => e.EndDate == null).SingleOrDefault();
                            List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStructCurrent.Id).ToList();
                            OEmpSalStructCurrent.EmpSalStructDetails = EmpSalStructDetailsList;
                            GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpSalStructCurrent.GeoStruct_Id).SingleOrDefault();
                            OEmpSalStructCurrent.GeoStruct = GeoStruct;
                            FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpSalStructCurrent.FuncStruct_Id).SingleOrDefault();
                            OEmpSalStructCurrent.FuncStruct = FuncStruct;
                            PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpSalStructCurrent.PayStruct_Id).SingleOrDefault();
                            OEmpSalStructCurrent.PayStruct = PayStruct;
                            foreach (var EmpSalStructDetailsListitem in EmpSalStructDetailsList)
                            {
                                SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsListitem.SalaryHead_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalaryHead = SalaryHead;
                                LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalaryHead.RoundingMethod = RoundingMethod;
                                SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsListitem.SalHeadFormula_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.SalHeadFormula = SalHeadFormula;
                                PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                                EmpSalStructDetailsListitem.PayScaleAssignment = PayScaleAssignment;

                            }
                        }

                        var OEmpSalStructDetailsCurrent = OEmpSalStructCurrent.EmpSalStructDetails.ToList();
                        if (OEmpSalStructCurrent.EffectiveDate < mEffectiveDate)
                        {
                            var OEmpSalStructOld = db.EmpSalStruct.Find(OEmpSalStructCurrent.Id);
                            //go for copying old structre program
                            // EmpStructBackup(OEmpSalStructCurrent);
                            //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                            var OEmpSalStructNew = new EmpSalStruct();
                            var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                            OEmpSalStructNew.DBTrack = OEmpSalStructOld.DBTrack;
                            OEmpSalStructNew.FuncStruct = OEmpSalStructOld.FuncStruct;
                            OEmpSalStructNew.GeoStruct = OEmpSalStructOld.GeoStruct;
                            OEmpSalStructNew.PayDays = OEmpSalStructOld.PayDays;
                            OEmpSalStructNew.PayStruct = OEmpSalStructOld.PayStruct;

                            OEmpSalStructNew.EffectiveDate = mEffectiveDate;

                            //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                            //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent;

                            foreach (var ca in OEmpSalStructDetailsCurrent)
                            {
                                var OEmpSalDetailsObj = new EmpSalStructDetails();
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                {
                                    OEmpSalDetailsObj.Amount = mNewBasic;
                                }
                                else
                                {
                                    if (ca.SalHeadFormula != null)
                                    {
                                        OEmpSalDetailsObj.Amount = 0;
                                    }
                                    else
                                    {
                                        OEmpSalDetailsObj.Amount = ca.Amount;
                                    }

                                }
                                OEmpSalDetailsObj.DBTrack = ca.DBTrack;
                                OEmpSalDetailsObj.PayScaleAssignment = ca.PayScaleAssignment;
                                OEmpSalDetailsObj.SalaryHead = ca.SalaryHead;
                                OEmpSalDetailsObj.SalHeadFormula = ca.SalHeadFormula;
                                OEmpSalStructDetailsNew.Add(OEmpSalDetailsObj);
                            }
                            OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                            var OEmpPayrollSave = db.EmployeePayroll.Find(OEmployeePayrollForincr.Id);
                            db.EmployeePayroll.Attach(OEmpPayrollSave).EmpSalStruct.Add(OEmpSalStructNew);
                            db.SaveChanges();

                            //db.Entry(OEmpSalStructNew).State = System.Data.Entity.EntityState.Detached;

                            OEmpSalStructOld.EndDate = mEffectiveDate.AddDays(-1);
                            db.Entry(OEmpSalStructOld).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //    db.Entry(OEmpSalStructOld).State = System.Data.Entity.EntityState.Detached; //Commented on 21042017 by sir

                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17
                            //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17 //Commented on 21042017 by sir
                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForincr, mEffectiveDate);
                            OEmployeePayrollForincr.EmpSalStruct.Add(OEmpSalStructNew);

                            db.SaveChanges();
                            //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17

                        }
                        else//check for empsalstruct
                        {
                            var OOldEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId).Where(e => (e.EndDate >= mEffectiveDate || e.EndDate == null))
                                .ToList();
                            foreach (var OOldEmpSalStructitem in OOldEmpSalStruct)
                            {

                                List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OOldEmpSalStructitem.Id).ToList();
                                OOldEmpSalStructitem.EmpSalStructDetails = EmpSalStructDetailsList;
                                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OOldEmpSalStructitem.GeoStruct_Id).SingleOrDefault();
                                OOldEmpSalStructitem.GeoStruct = GeoStruct;
                                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OOldEmpSalStructitem.FuncStruct_Id).SingleOrDefault();
                                OOldEmpSalStructitem.FuncStruct = FuncStruct;
                                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OOldEmpSalStructitem.PayStruct_Id).SingleOrDefault();
                                OOldEmpSalStructitem.PayStruct = PayStruct;
                                foreach (var EmpSalStructDetailsListitem in EmpSalStructDetailsList)
                                {
                                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsListitem.SalaryHead_Id).SingleOrDefault();
                                    EmpSalStructDetailsListitem.SalaryHead = SalaryHead;
                                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                    EmpSalStructDetailsListitem.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    LookupValue RoundingMethod = db.LookupValue.Where(e => e.Id == SalaryHead.RoundingMethod_Id).SingleOrDefault();
                                    EmpSalStructDetailsListitem.SalaryHead.RoundingMethod = RoundingMethod;
                                    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsListitem.SalHeadFormula_Id).SingleOrDefault();
                                    EmpSalStructDetailsListitem.SalHeadFormula = SalHeadFormula;
                                    PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == EmpSalStructDetailsListitem.PayScaleAssignment_Id).SingleOrDefault();
                                    EmpSalStructDetailsListitem.PayScaleAssignment = PayScaleAssignment;

                                }


                            }

                            //.OrderByDescending(e=>e.EffectiveDate)


                            var OIncrementServiceBook = db.IncrementServiceBook
                                                        .Include(q => q.IncrActivity)
                                                        .Include(q => q.IncrActivity.IncrList)
                                                        .Include(q => q.IncrActivity.IncrPolicy)
                                                        .Include(q => q.IncrActivity.IncrPolicy.IncrPolicyDetails)
                                                        .Where(e => e.Id == OIncrementServiceBook_id).SingleOrDefault();

                            foreach (var OOldEmpSalStructRecords in OOldEmpSalStruct)
                            {
                                if (OOldEmpSalStructRecords.EffectiveDate == mEffectiveDate || OOldEmpSalStructRecords.EffectiveDate > mEffectiveDate)
                                {
                                    var OEmpSalStructDetailsT = OOldEmpSalStructRecords.EmpSalStructDetails.ToList();
                                    // var OEmpSalStructModi = db.EmpSalStruct.Find(OOldEmpSalStructRecords.Id);
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    foreach (var OOldEmpSalStructRecordsDetails in OEmpSalStructDetailsT)
                                    {
                                        if (OOldEmpSalStructRecordsDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            var OEmpSalstructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalstructDetailsSave = db.EmpSalStructDetails.Find(OOldEmpSalStructRecordsDetails.Id);

                                            //GRADEPAY
                                            var OSalHeadFormulaResult = OOldEmpSalStructRecordsDetails.SalHeadFormula;
                                            List<BasicScaleDetails> OBasicScale = null;
                                            if (OSalHeadFormulaResult != null)
                                            {


                                                var OSalHeadFormula = db.SalHeadFormula
                                                                    .Include(e => e.BASICDependRule)
                                                                    .Include(e => e.BASICDependRule.BasicScale)
                                                                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                    .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                                                OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                            .Select(t => new BasicScaleDetails
                                                                            {
                                                                                StartingSlab = t.StartingSlab,
                                                                                EndingSlab = t.EndingSlab,
                                                                                IncrementAmount = t.IncrementAmount,
                                                                                IncrementCount = t.IncrementCount,
                                                                                EBMark = t.EBMark,
                                                                            }).ToList();
                                            }
                                            double mOldBasic = OOldEmpSalStructRecordsDetails.Amount;
                                            double oAmount = 0;
                                            IncrPolicyDetails OIncrPolicyOthers = OIncrementServiceBook.IncrActivity.IncrPolicy.IncrPolicyDetails;

                                            var OEmpIncrementHistoryStagnancy = OEmployeePayrollForincr.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                                           .Where(e => e.StagnancyAppl == true && e.ReleaseFlag == true && e.Id != OIncrementServiceBook_id)
                                                           .FirstOrDefault();
                                            var OEmpIncrementHistory = OEmployeePayrollForincr.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                                                        .FirstOrDefault();

                                            StagIncrPolicy OIncrPolicyStagOther = OIncrementServiceBook.IncrActivity.StagIncrPolicy;

                                            var OIncrPolicyOthersDetails = OIncrPolicyOthers;

                                            var oCompanyId = int.Parse(SessionManager.CompanyId);

                                            var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();
                                            double oAmount1 = 0;
                                            double mNewBasic1 = 0;
                                            if (OIncrPolicyOthers.IsIncrSteps == true)
                                            {//check it
                                                mNewBasic = BasicSelector(OIncrPolicyOthers.IncrSteps, OBasicScale, mOldBasic);//other increment

                                                Double[] mStagData = new double[4];
                                                mStagData[0] = mNewBasic; mStagData[1] = 0; mStagData[2] = 0; mStagData[3] = 0;
                                                //check it
                                                mStagData = IncrStagnancyFittmentPolicy(mNewBasic, OEmpIncrementHistoryStagnancy, OIncrPolicyStagOther, OBasicScale);
                                                if (mStagData != null)
                                                {
                                                    mNewBasic = mStagData[0];

                                                }
                                                else
                                                {
                                                    mNewBasic = 0;
                                                }
                                            }
                                            else if (OIncrPolicyOthersDetails.IsIncrAmount == true)
                                            {
                                                mNewBasic = mOldBasic + OIncrPolicyOthersDetails.IncrAmount;
                                            }


                                            else if (OIncrPolicyOthersDetails.IsIncrPercent == true)
                                            {
                                                if (CompCode.ToUpper() == "ACABL")
                                                {
                                                    if (OIncrPolicyOthersDetails.IncrPercent > 0)
                                                    {
                                                        oAmount1 = OOldEmpSalStructRecordsDetails.EmpSalStruct.EmpSalStructDetails
                                                                    .Where(r => r.SalaryHead.Code.ToUpper() == "GRADEPAY").Select(e => e.Amount)
                                                                    .SingleOrDefault();

                                                        mNewBasic = mOldBasic + ((mOldBasic + oAmount1) / 100 * OIncrPolicyOthersDetails.IncrPercent);

                                                        //mNewBasic = Math.Round(mNewBasic1 + 0.01, 0);

                                                        //Math.Round(mNewBasic + 0.01, 0);
                                                        string newb = mNewBasic.ToString("0.00");
                                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                        int b = Convert.ToInt32(newb.Split('.')[1]);




                                                        if (mNewBasic > 0 && mNewBasic % 10 != 0)
                                                        {
                                                            if (b > 0 && a == 0)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b > 0 && a < 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b == 0 && a <= 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                            }

                                                            //  mNewBasic = Convert.ToInt32(Convert.ToInt32(Convert.ToInt32(mNewBasic) / 10).ToString() + "0") + 10;
                                                            if (OBasicScale != null)
                                                            {
                                                                Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                                if (mNewBasic >= acblendslab)
                                                                {
                                                                    mNewBasic = acblendslab;
                                                                }
                                                            }
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    mNewBasic = Math.Round(mOldBasic + (OIncrPolicyOthersDetails.IncrPercent * mNewBasic / 100), 0);
                                                }



                                            }
                                            //my code
                                            if (OEmpSalstructDetailsSave.Amount != mNewBasic)
                                            {

                                                db.EmpSalStructDetails.Attach(OEmpSalstructDetailsSave);
                                                //OEmpSalstructDetailsSave.Amount = mNewBasic; 
                                                OEmpSalstructDetailsSave.Amount = mNewBasic;

                                                db.Entry(OEmpSalstructDetailsSave).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                //  db.Entry(OEmpSalstructDetailsSave).State = System.Data.Entity.EntityState.Detached; //Commented on 21042017 by sir
                                                //db.Entry<EmployeePayroll>(OEmployeePayroll).Reload();

                                                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                                                Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OOldEmpSalStructRecords, OEmployeePayrollForincr, OOldEmpSalStructRecords.EffectiveDate.Value);
                                                OEmployeePayrollForincr.EmpSalStruct.Add(OOldEmpSalStructRecords);

                                                db.SaveChanges();
                                                //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date

                                    var OEmpSalStructOld = db.EmpSalStruct.Find(OOldEmpSalStructRecords.Id);
                                    //go for copying old structre program
                                    // EmpStructBackup(OEmpSalStructCurrent);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                                    var OEmpSalStructNew = new EmpSalStruct();
                                    var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                                    OEmpSalStructNew.DBTrack = OEmpSalStructOld.DBTrack;
                                    OEmpSalStructNew.FuncStruct = OEmpSalStructOld.FuncStruct;
                                    OEmpSalStructNew.GeoStruct = OEmpSalStructOld.GeoStruct;
                                    OEmpSalStructNew.PayDays = OEmpSalStructOld.PayDays;
                                    OEmpSalStructNew.PayStruct = OEmpSalStructOld.PayStruct;
                                    OEmpSalStructNew.EndDate = OEmpSalStructOld.EndDate;
                                    OEmpSalStructNew.EffectiveDate = mEffectiveDate;

                                    //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                                    //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent;

                                    foreach (var OOldEmpSalStructRecordsDetails in OEmpSalStructOld.EmpSalStructDetails)
                                    {
                                        var OEmpSalDetailsObj = new EmpSalStructDetails();
                                        if (OOldEmpSalStructRecordsDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {

                                            //OEmpSalDetailsObj.Amount = mNewBasic;

                                            var OSalHeadFormulaResult = OOldEmpSalStructRecordsDetails.SalHeadFormula;
                                            List<BasicScaleDetails> OBasicScale = null;
                                            if (OSalHeadFormulaResult != null)
                                            {


                                                var OSalHeadFormula = db.SalHeadFormula
                                                                    .Include(e => e.BASICDependRule)
                                                                    .Include(e => e.BASICDependRule.BasicScale)
                                                                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                    .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                                                OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                           .Select(t => new BasicScaleDetails
                                                                           {
                                                                               StartingSlab = t.StartingSlab,
                                                                               EndingSlab = t.EndingSlab,
                                                                               IncrementAmount = t.IncrementAmount,
                                                                               IncrementCount = t.IncrementCount,
                                                                               EBMark = t.EBMark,
                                                                           }).ToList();
                                            }
                                            double mOldBasic = OOldEmpSalStructRecordsDetails.Amount;
                                            double oAmount = 0;
                                            IncrPolicyDetails OIncrPolicyOthers = OIncrementServiceBook.IncrActivity.IncrPolicy.IncrPolicyDetails;

                                            var OEmpIncrementHistoryStagnancy = OEmployeePayrollForincr.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                                           .Where(e => e.StagnancyAppl == true && e.ReleaseFlag == true && e.Id != OIncrementServiceBook_id)
                                                           .FirstOrDefault();
                                            var OEmpIncrementHistory = OEmployeePayrollForincr.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                                                        .FirstOrDefault();

                                            StagIncrPolicy OIncrPolicyStagOther = OIncrementServiceBook.IncrActivity.StagIncrPolicy;

                                            var oCompanyId = int.Parse(SessionManager.CompanyId);
                                            var OIncrPolicyOthersDetails = OIncrPolicyOthers;
                                            var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();
                                            double oAmount1 = 0;
                                            double mNewBasic1 = 0;
                                            if (OIncrPolicyOthers.IsIncrSteps == true)
                                            {//check it
                                                mNewBasic = BasicSelector(OIncrPolicyOthers.IncrSteps, OBasicScale, mOldBasic);//other increment

                                                Double[] mStagData = new double[4];
                                                mStagData[0] = mNewBasic; mStagData[1] = 0; mStagData[2] = 0; mStagData[3] = 0;
                                                //check it
                                                mStagData = IncrStagnancyFittmentPolicy(mNewBasic, OEmpIncrementHistoryStagnancy, OIncrPolicyStagOther, OBasicScale);
                                                if (mStagData != null)
                                                {
                                                    mNewBasic = mStagData[0];

                                                }
                                                else
                                                {
                                                    mNewBasic = 0;
                                                }
                                            }
                                            else if (OIncrPolicyOthersDetails.IsIncrAmount == true)
                                            {
                                                mNewBasic = mOldBasic + OIncrPolicyOthersDetails.IncrAmount;
                                            }
                                            else if (OIncrPolicyOthersDetails.IsIncrPercent == true)
                                            {
                                                if (CompCode.ToUpper() == "ACABL")
                                                {
                                                    if (OIncrPolicyOthersDetails.IncrPercent > 0)
                                                    {
                                                        oAmount1 = OOldEmpSalStructRecordsDetails.EmpSalStruct.EmpSalStructDetails
                                                                    .Where(r => r.SalaryHead.Code.ToUpper() == "GRADEPAY").Select(e => e.Amount)
                                                                    .SingleOrDefault();

                                                        mNewBasic = mOldBasic + ((mOldBasic + oAmount1) / 100 * OIncrPolicyOthersDetails.IncrPercent);

                                                        //mNewBasic = Math.Round(mNewBasic1 + 0.01, 0);

                                                        //Math.Round(mNewBasic + 0.01, 0);
                                                        string newb = mNewBasic.ToString("0.00");
                                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                        int b = Convert.ToInt32(newb.Split('.')[1]);




                                                        if (mNewBasic > 0 && mNewBasic % 10 != 0)
                                                        {
                                                            if (b > 0 && a == 0)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b > 0 && a < 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b == 0 && a <= 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                            }
                                                            // mNewBasic = Convert.ToInt32(Convert.ToInt32(Convert.ToInt32(mNewBasic) / 10).ToString() + "0") + 10;
                                                            if (OBasicScale != null)
                                                            {
                                                                Double acblendslab = OBasicScale.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                                if (mNewBasic >= acblendslab)
                                                                {
                                                                    mNewBasic = acblendslab;
                                                                }
                                                            }
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    mNewBasic = Math.Round(mOldBasic + (OIncrPolicyOthersDetails.IncrPercent * mNewBasic / 100), 0);

                                                }
                                            }
                                            OEmpSalDetailsObj.Amount = mNewBasic;
                                        }
                                        else
                                        {
                                            if (OOldEmpSalStructRecordsDetails.SalHeadFormula != null)
                                            {
                                                OEmpSalDetailsObj.Amount = 0;
                                            }
                                            else
                                            {
                                                OEmpSalDetailsObj.Amount = OOldEmpSalStructRecordsDetails.Amount;
                                            }
                                        }
                                        OEmpSalDetailsObj.DBTrack = OOldEmpSalStructRecordsDetails.DBTrack;
                                        OEmpSalDetailsObj.PayScaleAssignment = OOldEmpSalStructRecordsDetails.PayScaleAssignment;
                                        OEmpSalDetailsObj.SalaryHead = OOldEmpSalStructRecordsDetails.SalaryHead;
                                        OEmpSalDetailsObj.SalHeadFormula = OOldEmpSalStructRecordsDetails.SalHeadFormula;
                                        OEmpSalStructDetailsNew.Add(OEmpSalDetailsObj);
                                    }
                                    var OEmpPayrollSave = new EmployeePayroll();
                                    OEmpPayrollSave = db.EmployeePayroll.Find(OEmployeePayrollForincr.Id);
                                    OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                                    db.EmployeePayroll.Attach(OEmpPayrollSave).EmpSalStruct.Add(OEmpSalStructNew);
                                    db.SaveChanges();
                                    // db.Entry(OEmpSalStructNew).State = System.Data.Entity.EntityState.Detached; //Commented on 21042017 by sir

                                    //db.EmpSalStruct.Add(OEmpSalStructNew);
                                    //db.Entry(OEmpPayrollSave).State = System.Data.Entity.EntityState.Modified;
                                    //db.Entry(OEmpSalStructNew).State = System.Data.Entity.EntityState.Added;
                                    //OEmpPayrollSave.EmpSalStruct.Add(OEmpSalStructNew);
                                    //db.SaveChanges();
                                    //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;

                                    //db.EmpSalStruct.Attach(OEmpSalStructOld);

                                    OEmpSalStructOld.EndDate = mEffectiveDate.AddDays(-1);
                                    db.Entry(OEmpSalStructOld).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    // db.Entry(OEmpSalStructOld).State = System.Data.Entity.EntityState.Detached; //Commented on 21042017 by sir

                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                                    // Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdate(OEmployeePayroll, mEffectiveDate);
                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForincr, mEffectiveDate);
                                    OEmployeePayrollForincr.EmpSalStruct.Add(OEmpSalStructNew);

                                    db.SaveChanges();
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                    //db.Entry(OEmpPayrollSave).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }
                        break;
                    case "PROMOTION":
                        //checking for promotion date is exist in empsalstruct or new promotion date with geo,fun,paystruct changes
                        var OEmployeePayrollForPromotion
                = db.EmployeePayroll
                    .Include(e => e.Employee)
                            // .Include(e => e.Employee.EmpOffInfo)
                            //.Include(e => e.Employee.GeoStruct)
                            //.Include(e => e.Employee.PayStruct)
                            //.Include(e => e.Employee.FuncStruct)
                            //.Include(e => e.SalAttendance)
                    .Include(e => e.Employee.ServiceBookDates)
                    .Include(e => e.EmpSalStruct)
                            //  .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                            // .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula.FormulaType)))
                            //.Include(e => e.EmpSalStruct.Select(r => r.Emp[SalStructDetails.Select(t => t.SalaryHead.RoundingMethod)))
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment)))
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula)))//added by prashant 13/4/17
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(a => a.FuncStruct))))//added by prashant 13/4/17
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(a => a.GeoStruct))))//added by prashant 13/4/17
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(a => a.PayStruct))))//added by prashant 13/4/17
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(w => w.PayScaleAssignment.SalHeadFormula.Select(x => x.FormulaType))))//added by prashant 13/4/17
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                            //.Include(e => e.IncrementServiceBook)
                            //.Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                            //.Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.StagIncrPolicy))//.AsNoTracking()
                   .Where(e => e.Id == EmployeePayrollId).SingleOrDefault();

                        var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id)
                      .Include(e => e.GeoStruct)
                      .Include(e => e.NewPayScale)
                      .Include(e => e.NewPayScaleAgreement)
                      .Include(e => e.OldPayScale)
                      .Include(e => e.OldPayScaleAgreement)
                      .Include(e => e.NewPayStruct)
                      .Include(e => e.OldPayStruct)
                      .Include(e => e.OldFuncStruct)
                      .Include(e => e.NewFuncStruct)
                      .Include(e => e.NewJobStatus)
                      .Include(e => e.OldJobStatus)
                      .Include(e => e.PromotionActivity)
                      .Include(e => e.PromotionActivity.PromoList)
                      .Include(e => e.PromotionActivity.PromoPolicy)
                      .Include(e => e.PromotionActivity.PromoPolicy.IncrActivity)
                      .SingleOrDefault();
                        OEmpSalStructCurrent = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && e.EndDate == null)
                                                      .Include(e => e.EmpSalStructDetails)
                                                      .Include(e => e.GeoStruct)
                                                      .Include(e => e.FuncStruct)
                                                      .Include(e => e.PayStruct)
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment)).SingleOrDefault();
                        //var OEmpSalStructTemp = OEmployeePayroll.EmpSalStruct
                        //                                    .Where(e => e.EndDate == null).SingleOrDefault();

                        List<EmpSalStructDetails> OEmpSalStructDetailsCurrent1 = OEmpSalStructCurrent.EmpSalStructDetails.ToList();
                        if (OEmpSalStructCurrent.EffectiveDate < mEffectiveDate)
                        {
                            //go for copying old structre program
                            //EmpStructBackup(OEmpSalStructCurrent);
                            //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                            EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                            var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                            //OEmpSalStructNew = OEmpSalStructCurrent;
                            OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                            OEmpSalStructNew.GeoStruct = OPromotionServiceBook.GeoStruct;
                            OEmpSalStructNew.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                            OEmpSalStructNew.PayStruct = OPromotionServiceBook.NewPayStruct;
                            OEmpSalStructNew.DBTrack = OPromotionServiceBook.DBTrack;
                            //db.EmpSalStruct.Add(OEmpSalStructNew);
                            //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                            //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent1;
                            foreach (var ca in OEmpSalStructDetailsCurrent1)
                            {
                                var OEmpSalStructDetail = new EmpSalStructDetails();
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                {
                                    // OEmpSalStructDetail.Amount = mNewBasic;
                                    //06122023 Start
                                    double mOldBasic = 0;
                                    double mFittmentAmount = 0;


                                    var OSalHeadFormulaResult = ca.SalHeadFormula;
                                    List<BasicScaleDetails> OBasicScaleOld = null;
                                    if (OSalHeadFormulaResult != null)
                                    {


                                        var OSalHeadFormula = db.SalHeadFormula
                                                            .Include(e => e.BASICDependRule)
                                                            .Include(e => e.BASICDependRule.BasicScale)
                                                            .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                            .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();




                                        OBasicScaleOld = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                    .Select(t => new BasicScaleDetails
                                                                    {
                                                                        StartingSlab = t.StartingSlab,
                                                                        EndingSlab = t.EndingSlab,
                                                                        IncrementAmount = t.IncrementAmount,
                                                                        IncrementCount = t.IncrementCount,
                                                                        EBMark = t.EBMark,

                                                                    }
                                                                        ).ToList();
                                    }

                                    var oCompanyId = int.Parse(SessionManager.CompanyId);
                                    var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();

                                    double oAmount = 0;

                                    if (OBasicScaleOld != null)
                                    {

                                        mOldBasic = ca.Amount;
                                        if (CompCode.ToUpper() == "ACABL")
                                        {
                                            if (ca.SalaryHead.Code.ToUpper() == "GRADEPAY")
                                            {
                                                oAmount = ca.Amount;
                                            }

                                        }
                                        //double oAmount = 0;

                                        // mOldBasic += oAmount;
                                    }

                                    //after saving changes in promotion service book SalFormulaFinderPromotion
                                    var OSalHeadFormulaResult1 = SalFormulaFinderPromotion(OPromotionServiceBook_id, ca.SalaryHead.Id);
                                    List<BasicScaleDetails> OBasicScale1 = null;
                                    if (OSalHeadFormulaResult1 != null)
                                    {
                                        var OSalHeadFormula1 = db.SalHeadFormula
                                                        .Include(e => e.BASICDependRule)
                                                        .Include(e => e.BASICDependRule.BasicScale)
                                                        .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                        .Where(e => e.Id == OSalHeadFormulaResult1.Id).SingleOrDefault();

                                        OBasicScale1 = OSalHeadFormula1.BASICDependRule.BasicScale.BasicScaleDetails
                                                                     .Select(t => new BasicScaleDetails
                                                                     {
                                                                         StartingSlab = t.StartingSlab,
                                                                         EndingSlab = t.EndingSlab,
                                                                         IncrementAmount = t.IncrementAmount,
                                                                         IncrementCount = t.IncrementCount,
                                                                         EBMark = t.EBMark,
                                                                     }).ToList();

                                    }

                                    List<BasicScaleDetails> OBasicScaleNew = OBasicScale1;
                                    // var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                                    var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.PromoActivity))

                                                       .Where(e => e.EndDate == null && e.EmployeePayroll_Id == EmployeePayrollId).FirstOrDefault().EmployeePolicyStructDetails.SelectMany(r => r.PolicyFormula.PromoActivity.Where(n => n.Id == OPromotionServiceBook.PromotionActivity.Id && n != null)).ToList();

                                    var OPromoPolicyT = test.Select(y => y.PromoPolicy).SingleOrDefault();
                                    var OPromoPolicy1 = new PromoPolicy();
                                    OPromoPolicy1 = OPromoPolicyT;
                                    var OPromoPolicy = db.PromoPolicy
                                                                    .Where(e => e.Id == OPromoPolicy1.Id)
                                                                    .Include(e => e.IncrActivity)
                                                                    .Include(e => e.IncrActivity.IncrPolicy.IncrPolicyDetails)
                                                                    .Include(e => e.IncrActivity.IncrPolicy)
                                                                    .SingleOrDefault();

                                    if (OPromoPolicy != null)
                                    {
                                        if (OPromoPolicy.IsOldScaleIncrAction == true)
                                        {
                                            double mTempNewBasic = 0;
                                            //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                            if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                            {
                                                mTempNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleOld, mOldBasic);

                                            }
                                            else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                            {
                                                mTempNewBasic = mOldBasic + ((mOldBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                            }
                                            else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                            {
                                                mTempNewBasic = mOldBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                            }

                                            //mTempNewBasic=BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps,OBasicScaleOld,mOldBasic);
                                            mFittmentAmount = BasicFittmentSelector(mTempNewBasic, OBasicScaleNew);

                                            mNewBasic = mTempNewBasic + mFittmentAmount;
                                            // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true start
                                            if (OPromoPolicy.IsNewScaleIncrAction == true)
                                            {

                                                mTempNewBasic = mNewBasic;

                                                if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                {
                                                    mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                                }
                                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                {
                                                    mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                }
                                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                {
                                                    mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                }



                                            }
                                            // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true end

                                            if (CompCode.ToUpper() == "ACABL")
                                            {
                                                string newb = mNewBasic.ToString("0.00");
                                                string a1 = mNewBasic.ToString().Split('.')[0];
                                                int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                int b = Convert.ToInt32(newb.Split('.')[1]);

                                                if (b > 0 && a == 0)
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                }
                                                else if (b > 0 && a < 5)
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                }
                                                else if (b == 0 && a <= 5)
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                }
                                                else
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                }
                                                if (OBasicScaleNew != null)
                                                {
                                                    Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                    if (mNewBasic >= acblendslab)
                                                    {
                                                        mNewBasic = acblendslab;
                                                    }
                                                }
                                            }



                                        }
                                        else if (OPromoPolicy.IsNewScaleIncrAction == true)
                                        {
                                            double mTempNewBasic = 0;
                                            //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                            mFittmentAmount = BasicFittmentSelector(mOldBasic, OBasicScaleNew);


                                            mTempNewBasic = mOldBasic + mFittmentAmount;


                                            if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                            {
                                                mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                            }
                                            else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                            {
                                                mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                            }
                                            else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                            {
                                                mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                            }

                                            if (CompCode.ToUpper() == "ACABL")
                                            {
                                                string newb = mNewBasic.ToString("0.00");
                                                string a1 = mNewBasic.ToString().Split('.')[0];
                                                int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                int b = Convert.ToInt32(newb.Split('.')[1]);

                                                if (b > 0 && a == 0)
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                }
                                                else if (b > 0 && a < 5)
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                }
                                                else if (b == 0 && a <= 5)
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                }
                                                else
                                                {
                                                    mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                }
                                                if (OBasicScaleNew != null)
                                                {
                                                    Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                    if (mNewBasic >= acblendslab)
                                                    {
                                                        mNewBasic = acblendslab;
                                                    }
                                                }
                                            }



                                        }
                                        else //normal without any paystruct changes
                                        {
                                            double mTempNewBasic = 0;
                                            //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                            mTempNewBasic = mOldBasic;

                                            if (OPromoPolicy.IncrActivity != null)
                                            {
                                                if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                {
                                                    mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                }
                                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                {
                                                    mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                }

                                                if (CompCode.ToUpper() == "ACABL")
                                                {
                                                    string newb = mNewBasic.ToString("0.00");
                                                    string a1 = mNewBasic.ToString().Split('.')[0];
                                                    int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                    int b = Convert.ToInt32(newb.Split('.')[1]);

                                                    if (b > 0 && a == 0)
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                    }
                                                    else if (b > 0 && a < 5)
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                    }
                                                    else if (b == 0 && a <= 5)
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                    }
                                                    else
                                                    {
                                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                    }
                                                    if (OBasicScaleNew != null)
                                                    {
                                                        Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                        if (mNewBasic >= acblendslab)
                                                        {
                                                            mNewBasic = acblendslab;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                mNewBasic = mOldBasic;
                                            }


                                        }
                                    }
                                    OEmpSalStructDetail.Amount = mNewBasic;


                                    //06122023 end

                                }
                                else
                                {
                                    OEmpSalStructDetail.Amount = ca.Amount;
                                }
                                OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                OEmpSalStructDetail.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);// ca.SalHeadFormula;
                                OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);

                            }

                            OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                            OEmployeePayrollForPromotion.EmpSalStruct.Add(OEmpSalStructNew);
                            OEmpSalStructCurrent.EndDate = mEffectiveDate.AddDays(-1).Date;
                            db.Entry(OEmpSalStructCurrent).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForPromotion, mEffectiveDate);
                            OEmployeePayrollForPromotion.EmpSalStruct.Add(OEmpSalStructNew);

                            db.SaveChanges();
                            // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                        }
                        else//check for empsalstruct
                        {
                            var OOldEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && (e.EndDate >= mEffectiveDate || e.EndDate == null))
                                                      .Include(e => e.EmpSalStructDetails)
                                //.Include(e => e.GeoStruct)
                                //.Include(e => e.FuncStruct)
                                //.Include(e => e.PayStruct)
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                                      .ToList();
                            // var OOldEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate >= mEffectiveDate || e.EndDate == null).ToList();
                            // var OOldEmpSalStruct = OOldEmpSalStructdata.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && e.EndDate >= mEffectiveDate || e.EndDate == null).ToList();
                            foreach (var OOldEmpSalStructRecords in OOldEmpSalStruct)
                            {
                                if (OOldEmpSalStructRecords.EffectiveDate == mEffectiveDate || OOldEmpSalStructRecords.EffectiveDate > mEffectiveDate)
                                {
                                    OOldEmpSalStructRecords.GeoStruct = OPromotionServiceBook.GeoStruct;
                                    OOldEmpSalStructRecords.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                                    OOldEmpSalStructRecords.PayStruct = OPromotionServiceBook.NewPayStruct;
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    foreach (var OOldEmpSalStructRecordsDetails in OOldEmpSalStructRecords.EmpSalStructDetails)
                                    {
                                        if (OOldEmpSalStructRecordsDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            // OOldEmpSalStructRecordsDetails.Amount = mNewBasic;
                                            //OOldEmpSalStructRecordsDetails.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OOldEmpSalStructRecordsDetails.PayScaleAssignment, OOldEmpSalStructRecordsDetails.SalaryHead.Id);
                                            //06122023 Start
                                            double mOldBasic = 0;
                                            double mFittmentAmount = 0;


                                            var OSalHeadFormulaResult = OOldEmpSalStructRecordsDetails.SalHeadFormula;
                                            List<BasicScaleDetails> OBasicScaleOld = null;
                                            if (OSalHeadFormulaResult != null)
                                            {


                                                var OSalHeadFormula = db.SalHeadFormula
                                                                    .Include(e => e.BASICDependRule)
                                                                    .Include(e => e.BASICDependRule.BasicScale)
                                                                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                    .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();




                                                OBasicScaleOld = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                            .Select(t => new BasicScaleDetails
                                                                            {
                                                                                StartingSlab = t.StartingSlab,
                                                                                EndingSlab = t.EndingSlab,
                                                                                IncrementAmount = t.IncrementAmount,
                                                                                IncrementCount = t.IncrementCount,
                                                                                EBMark = t.EBMark,

                                                                            }
                                                                                ).ToList();
                                            }

                                            var oCompanyId = int.Parse(SessionManager.CompanyId);
                                            var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();

                                            double oAmount = 0;

                                            if (OBasicScaleOld != null)
                                            {

                                                mOldBasic = OOldEmpSalStructRecordsDetails.Amount;
                                                if (CompCode.ToUpper() == "ACABL")
                                                {
                                                    if (OOldEmpSalStructRecordsDetails.SalaryHead.Code.ToUpper() == "GRADEPAY")
                                                    {
                                                        oAmount = OOldEmpSalStructRecordsDetails.Amount;
                                                    }

                                                }
                                                //double oAmount = 0;

                                                // mOldBasic += oAmount;
                                            }

                                            //after saving changes in promotion service book SalFormulaFinderPromotion
                                            var OSalHeadFormulaResult1 = SalFormulaFinderPromotion(OPromotionServiceBook_id, OOldEmpSalStructRecordsDetails.SalaryHead.Id);
                                            List<BasicScaleDetails> OBasicScale1 = null;
                                            if (OSalHeadFormulaResult1 != null)
                                            {
                                                var OSalHeadFormula1 = db.SalHeadFormula
                                                                .Include(e => e.BASICDependRule)
                                                                .Include(e => e.BASICDependRule.BasicScale)
                                                                .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                .Where(e => e.Id == OSalHeadFormulaResult1.Id).SingleOrDefault();

                                                OBasicScale1 = OSalHeadFormula1.BASICDependRule.BasicScale.BasicScaleDetails
                                                                             .Select(t => new BasicScaleDetails
                                                                             {
                                                                                 StartingSlab = t.StartingSlab,
                                                                                 EndingSlab = t.EndingSlab,
                                                                                 IncrementAmount = t.IncrementAmount,
                                                                                 IncrementCount = t.IncrementCount,
                                                                                 EBMark = t.EBMark,
                                                                             }).ToList();

                                            }

                                            List<BasicScaleDetails> OBasicScaleNew = OBasicScale1;
                                            // var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                                            var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.PromoActivity))

                                                               .Where(e => e.EndDate == null && e.EmployeePayroll_Id == EmployeePayrollId).FirstOrDefault().EmployeePolicyStructDetails.SelectMany(r => r.PolicyFormula.PromoActivity.Where(n => n.Id == OPromotionServiceBook.PromotionActivity.Id && n != null)).ToList();

                                            var OPromoPolicyT = test.Select(y => y.PromoPolicy).SingleOrDefault();
                                            var OPromoPolicy1 = new PromoPolicy();
                                            OPromoPolicy1 = OPromoPolicyT;
                                            var OPromoPolicy = db.PromoPolicy
                                                                            .Where(e => e.Id == OPromoPolicy1.Id)
                                                                            .Include(e => e.IncrActivity)
                                                                            .Include(e => e.IncrActivity.IncrPolicy.IncrPolicyDetails)
                                                                            .Include(e => e.IncrActivity.IncrPolicy)
                                                                            .SingleOrDefault();

                                            if (OPromoPolicy != null)
                                            {
                                                if (OPromoPolicy.IsOldScaleIncrAction == true)
                                                {
                                                    double mTempNewBasic = 0;
                                                    //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                                    if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                    {
                                                        mTempNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleOld, mOldBasic);

                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                    {
                                                        mTempNewBasic = mOldBasic + ((mOldBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                    {
                                                        mTempNewBasic = mOldBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                    }

                                                    //mTempNewBasic=BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps,OBasicScaleOld,mOldBasic);
                                                    mFittmentAmount = BasicFittmentSelector(mTempNewBasic, OBasicScaleNew);

                                                    mNewBasic = mTempNewBasic + mFittmentAmount;
                                                    // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true start
                                                    if (OPromoPolicy.IsNewScaleIncrAction == true)
                                                    {

                                                        mTempNewBasic = mNewBasic;

                                                        if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                        {
                                                            mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                                        }
                                                        else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                        }
                                                        else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                        }



                                                    }
                                                    // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true end

                                                    if (CompCode.ToUpper() == "ACABL")
                                                    {
                                                        string newb = mNewBasic.ToString("0.00");
                                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                        int b = Convert.ToInt32(newb.Split('.')[1]);

                                                        if (b > 0 && a == 0)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b > 0 && a < 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b == 0 && a <= 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                        }
                                                        if (OBasicScaleNew != null)
                                                        {
                                                            Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                            if (mNewBasic >= acblendslab)
                                                            {
                                                                mNewBasic = acblendslab;
                                                            }
                                                        }
                                                    }



                                                }
                                                else if (OPromoPolicy.IsNewScaleIncrAction == true)
                                                {
                                                    double mTempNewBasic = 0;
                                                    //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                                    mFittmentAmount = BasicFittmentSelector(mOldBasic, OBasicScaleNew);


                                                    mTempNewBasic = mOldBasic + mFittmentAmount;


                                                    if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                    {
                                                        mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                    {
                                                        mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                    {
                                                        mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                    }

                                                    if (CompCode.ToUpper() == "ACABL")
                                                    {
                                                        string newb = mNewBasic.ToString("0.00");
                                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                        int b = Convert.ToInt32(newb.Split('.')[1]);

                                                        if (b > 0 && a == 0)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b > 0 && a < 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b == 0 && a <= 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                        }
                                                        if (OBasicScaleNew != null)
                                                        {
                                                            Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                            if (mNewBasic >= acblendslab)
                                                            {
                                                                mNewBasic = acblendslab;
                                                            }
                                                        }
                                                    }



                                                }
                                                else //normal without any paystruct changes
                                                {
                                                    double mTempNewBasic = 0;
                                                    //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                                    mTempNewBasic = mOldBasic;

                                                    if (OPromoPolicy.IncrActivity != null)
                                                    {
                                                        if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                        }
                                                        else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                        }

                                                        if (CompCode.ToUpper() == "ACABL")
                                                        {
                                                            string newb = mNewBasic.ToString("0.00");
                                                            string a1 = mNewBasic.ToString().Split('.')[0];
                                                            int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                            int b = Convert.ToInt32(newb.Split('.')[1]);

                                                            if (b > 0 && a == 0)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b > 0 && a < 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b == 0 && a <= 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                            }
                                                            if (OBasicScaleNew != null)
                                                            {
                                                                Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                                if (mNewBasic >= acblendslab)
                                                                {
                                                                    mNewBasic = acblendslab;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        mNewBasic = mOldBasic;
                                                    }


                                                }
                                            }
                                            OOldEmpSalStructRecordsDetails.Amount = mNewBasic;
                                            OOldEmpSalStructRecordsDetails.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OOldEmpSalStructRecordsDetails.PayScaleAssignment, OOldEmpSalStructRecordsDetails.SalaryHead.Id);

                                            //06122023 end

                                            //OOldEmpSalStructRecords.GeoStruct = OPromotionServiceBook.GeoStruct;
                                            //OOldEmpSalStructRecords.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                                            //OOldEmpSalStructRecords.PayStruct = OPromotionServiceBook.NewPayStruct;
                                            //db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17
                                        }
                                        else
                                        {

                                            //OOldEmpSalStructRecordsDetails.Amount = ca.Amount;
                                            OOldEmpSalStructRecordsDetails.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OOldEmpSalStructRecordsDetails.PayScaleAssignment, OOldEmpSalStructRecordsDetails.SalaryHead.Id);
                                        }
                                    }
                                    //db.Entry(OOldEmpSalStructRecordsDetails).State = System.Data.Entity.EntityState.Modified;
                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OOldEmpSalStructRecords, OEmployeePayrollForPromotion, OOldEmpSalStructRecords.EffectiveDate.Value.Date);
                                    OEmployeePayrollForPromotion.EmpSalStruct.Add(OOldEmpSalStructRecords);

                                    db.SaveChanges();
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                    // break;



                                }
                                else
                                {
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                                    EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                    var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                                    //OEmpSalStructNew = OOldEmpSalStructRecords;
                                    OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                                    OEmpSalStructNew.GeoStruct = OPromotionServiceBook.GeoStruct;
                                    OEmpSalStructNew.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                                    OEmpSalStructNew.PayStruct = OPromotionServiceBook.NewPayStruct;
                                    OEmpSalStructNew.DBTrack = OPromotionServiceBook.DBTrack;
                                    OEmpSalStructNew.EndDate = OOldEmpSalStructRecords.EndDate;
                                    //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                                    var OEmpSalStructDetailsTemp = OOldEmpSalStructRecords.EmpSalStructDetails;
                                    foreach (var ca in OEmpSalStructDetailsTemp)
                                    {
                                        var OEmpSalStructDetail = new EmpSalStructDetails();
                                        if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            //OEmpSalStructDetail.Amount = mNewBasic;
                                            //06122023 Start
                                            double mOldBasic = 0;
                                            double mFittmentAmount = 0;


                                            var OSalHeadFormulaResult = ca.SalHeadFormula;
                                            List<BasicScaleDetails> OBasicScaleOld = null;
                                            if (OSalHeadFormulaResult != null)
                                            {


                                                var OSalHeadFormula = db.SalHeadFormula
                                                                    .Include(e => e.BASICDependRule)
                                                                    .Include(e => e.BASICDependRule.BasicScale)
                                                                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                    .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();




                                                OBasicScaleOld = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                                            .Select(t => new BasicScaleDetails
                                                                            {
                                                                                StartingSlab = t.StartingSlab,
                                                                                EndingSlab = t.EndingSlab,
                                                                                IncrementAmount = t.IncrementAmount,
                                                                                IncrementCount = t.IncrementCount,
                                                                                EBMark = t.EBMark,

                                                                            }
                                                                                ).ToList();
                                            }

                                            var oCompanyId = int.Parse(SessionManager.CompanyId);
                                            var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();

                                            double oAmount = 0;

                                            if (OBasicScaleOld != null)
                                            {

                                                mOldBasic = ca.Amount;
                                                if (CompCode.ToUpper() == "ACABL")
                                                {
                                                    if (ca.SalaryHead.Code.ToUpper() == "GRADEPAY")
                                                    {
                                                        oAmount = ca.Amount;
                                                    }

                                                }
                                                //double oAmount = 0;

                                                // mOldBasic += oAmount;
                                            }

                                            //after saving changes in promotion service book SalFormulaFinderPromotion
                                            var OSalHeadFormulaResult1 = SalFormulaFinderPromotion(OPromotionServiceBook_id, ca.SalaryHead.Id);
                                            List<BasicScaleDetails> OBasicScale1 = null;
                                            if (OSalHeadFormulaResult1 != null)
                                            {
                                                var OSalHeadFormula1 = db.SalHeadFormula
                                                                .Include(e => e.BASICDependRule)
                                                                .Include(e => e.BASICDependRule.BasicScale)
                                                                .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                                                .Where(e => e.Id == OSalHeadFormulaResult1.Id).SingleOrDefault();

                                                OBasicScale1 = OSalHeadFormula1.BASICDependRule.BasicScale.BasicScaleDetails
                                                                             .Select(t => new BasicScaleDetails
                                                                             {
                                                                                 StartingSlab = t.StartingSlab,
                                                                                 EndingSlab = t.EndingSlab,
                                                                                 IncrementAmount = t.IncrementAmount,
                                                                                 IncrementCount = t.IncrementCount,
                                                                                 EBMark = t.EBMark,
                                                                             }).ToList();

                                            }

                                            List<BasicScaleDetails> OBasicScaleNew = OBasicScale1;
                                            // var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                                            var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.PromoActivity))

                                                               .Where(e => e.EndDate == null && e.EmployeePayroll_Id == EmployeePayrollId).FirstOrDefault().EmployeePolicyStructDetails.SelectMany(r => r.PolicyFormula.PromoActivity.Where(n => n.Id == OPromotionServiceBook.PromotionActivity.Id && n != null)).ToList();

                                            var OPromoPolicyT = test.Select(y => y.PromoPolicy).SingleOrDefault();
                                            var OPromoPolicy1 = new PromoPolicy();
                                            OPromoPolicy1 = OPromoPolicyT;
                                            var OPromoPolicy = db.PromoPolicy
                                                                            .Where(e => e.Id == OPromoPolicy1.Id)
                                                                            .Include(e => e.IncrActivity)
                                                                            .Include(e => e.IncrActivity.IncrPolicy.IncrPolicyDetails)
                                                                            .Include(e => e.IncrActivity.IncrPolicy)
                                                                            .SingleOrDefault();

                                            if (OPromoPolicy != null)
                                            {
                                                if (OPromoPolicy.IsOldScaleIncrAction == true)
                                                {
                                                    double mTempNewBasic = 0;
                                                    //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                                    if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                    {
                                                        mTempNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleOld, mOldBasic);

                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                    {
                                                        mTempNewBasic = mOldBasic + ((mOldBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                    {
                                                        mTempNewBasic = mOldBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                    }

                                                    //mTempNewBasic=BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps,OBasicScaleOld,mOldBasic);
                                                    mFittmentAmount = BasicFittmentSelector(mTempNewBasic, OBasicScaleNew);

                                                    mNewBasic = mTempNewBasic + mFittmentAmount;
                                                    // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true start
                                                    if (OPromoPolicy.IsNewScaleIncrAction == true)
                                                    {

                                                        mTempNewBasic = mNewBasic;

                                                        if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                        {
                                                            mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                                        }
                                                        else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                        }
                                                        else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                        }



                                                    }
                                                    // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true end

                                                    if (CompCode.ToUpper() == "ACABL")
                                                    {
                                                        string newb = mNewBasic.ToString("0.00");
                                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                        int b = Convert.ToInt32(newb.Split('.')[1]);

                                                        if (b > 0 && a == 0)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b > 0 && a < 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b == 0 && a <= 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                        }
                                                        if (OBasicScaleNew != null)
                                                        {
                                                            Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                            if (mNewBasic >= acblendslab)
                                                            {
                                                                mNewBasic = acblendslab;
                                                            }
                                                        }
                                                    }



                                                }
                                                else if (OPromoPolicy.IsNewScaleIncrAction == true)
                                                {
                                                    double mTempNewBasic = 0;
                                                    //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                                    mFittmentAmount = BasicFittmentSelector(mOldBasic, OBasicScaleNew);


                                                    mTempNewBasic = mOldBasic + mFittmentAmount;


                                                    if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                                    {
                                                        mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                    {
                                                        mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                    }
                                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                    {
                                                        mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                    }

                                                    if (CompCode.ToUpper() == "ACABL")
                                                    {
                                                        string newb = mNewBasic.ToString("0.00");
                                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                        int b = Convert.ToInt32(newb.Split('.')[1]);

                                                        if (b > 0 && a == 0)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b > 0 && a < 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else if (b == 0 && a <= 5)
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                        }
                                                        else
                                                        {
                                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                        }
                                                        if (OBasicScaleNew != null)
                                                        {
                                                            Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                            if (mNewBasic >= acblendslab)
                                                            {
                                                                mNewBasic = acblendslab;
                                                            }
                                                        }
                                                    }



                                                }
                                                else //normal without any paystruct changes
                                                {
                                                    double mTempNewBasic = 0;
                                                    //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                                    mTempNewBasic = mOldBasic;

                                                    if (OPromoPolicy.IncrActivity != null)
                                                    {
                                                        if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                                        }
                                                        else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                                        {
                                                            mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                                        }

                                                        if (CompCode.ToUpper() == "ACABL")
                                                        {
                                                            string newb = mNewBasic.ToString("0.00");
                                                            string a1 = mNewBasic.ToString().Split('.')[0];
                                                            int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                                            int b = Convert.ToInt32(newb.Split('.')[1]);

                                                            if (b > 0 && a == 0)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b > 0 && a < 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else if (b == 0 && a <= 5)
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                                            }
                                                            else
                                                            {
                                                                mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                                            }
                                                            if (OBasicScaleNew != null)
                                                            {
                                                                Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                                                if (mNewBasic >= acblendslab)
                                                                {
                                                                    mNewBasic = acblendslab;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        mNewBasic = mOldBasic;
                                                    }


                                                }
                                            }
                                            OEmpSalStructDetail.Amount = mNewBasic;


                                            //06122023 end
                                        }
                                        else
                                        {
                                            OEmpSalStructDetail.Amount = ca.Amount;
                                        }
                                        OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                        OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                        OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                        OEmpSalStructDetail.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);// ca.SalHeadFormula;
                                        OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);

                                    }

                                    OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                                    OEmployeePayrollForPromotion.EmpSalStruct.Add(OEmpSalStructNew);


                                    OOldEmpSalStructRecords.EndDate = mEffectiveDate.AddDays(-1).Date;
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForPromotion, mEffectiveDate);
                                    OEmployeePayrollForPromotion.EmpSalStruct.Add(OEmpSalStructNew);

                                    db.SaveChanges();
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                }
                            }
                        }
                        break;
                    case "TRANSFER":
                        var OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == OTransferServiceBook_id)
                                .Include(e => e.NewGeoStruct)
                            //.Include(e => e.NewPayScale)
                            //.Include(e => e.NewPayScaleAgreement)
                            //.Include(e => e.OldPayScale)
                            //.Include(e => e.OldPayScaleAgreement)
                            //.Include(e => e.NewPayStruct)
                                .Include(e => e.OldPayStruct)
                                .Include(e => e.OldFuncStruct)
                                .Include(e => e.NewFuncStruct)
                                .Include(e => e.NewPayStruct)
                            //.Include(e => e.OldJobStatus)
                                .Include(e => e.OldGeoStruct)
                                .Include(e => e.TransActivity)
                                .Include(e => e.TransActivity.TransList)
                                .Include(e => e.TransActivity.TranPolicy)
                                .Include(e => e.TransActivity.TranPolicy)
                       .SingleOrDefault();
                        var OEmployeePayrollForTransfer
                                      = db.EmployeePayroll
                   .Include(e => e.Employee)
                   .Include(e => e.Employee.ServiceBookDates)
                   .Include(e => e.EmpSalStruct)
                  .Where(e => e.Id == EmployeePayrollId).FirstOrDefault();

                        //checking for promotion date is exist in empsalstruct or new promotion date with geo,fun,paystruct changes
                        // OEmpSalStructCurrent = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                        OEmpSalStructCurrent = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && e.EndDate == null)
                                                      .Include(e => e.EmpSalStructDetails)
                                                      .Include(e => e.GeoStruct)
                                                      .Include(e => e.FuncStruct)
                                                      .Include(e => e.PayStruct)
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct)))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct)))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct)))
                            //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType)))
                                                      .FirstOrDefault();

                        var OEmpSalStructDetailsCurrent2 = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == OEmpSalStructCurrent.Id)
                             .Include(e => e.SalaryHead)
                                                      .Include(e => e.SalaryHead.SalHeadOperationType)
                                                      .Include(e => e.SalaryHead.RoundingMethod)
                                                      .Include(e => e.SalHeadFormula)
                                                      .Include(e => e.PayScaleAssignment)
                                                      .Include(e => e.PayScaleAssignment.SalHeadFormula)
                                                      .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct))
                                                      .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct))
                                                      .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct))
                                                      .Include(e => e.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType))
                                                      .OrderBy(r => r.SalaryHead.SeqNo).ToList();
                        //  var OEmpSalStructDetailsCurrent2 = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).Select(e => e.EmpSalStructDetails.OrderBy(r => r.SalaryHead.SeqNo)).SingleOrDefault();
                        if (OEmpSalStructCurrent.EffectiveDate < mEffectiveDate)
                        {
                            //go for copying old structre program
                            //Process.SalaryHeadGenProcess.EmpStructBackup(OEmpSalStructCurrent);
                            //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date

                            EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                            var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                            //OEmpSalStructNew = OEmpSalStructCurrent;
                            //OEmpSalStructNew.Id=0;
                            OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                            OEmpSalStructNew.GeoStruct = OTransferServiceBook.NewGeoStruct != null ? OTransferServiceBook.NewGeoStruct : OTransferServiceBook.OldGeoStruct;
                            OEmpSalStructNew.FuncStruct = OTransferServiceBook.NewFuncStruct != null ? OTransferServiceBook.NewFuncStruct : OTransferServiceBook.OldFuncStruct;
                            OEmpSalStructNew.PayStruct = OTransferServiceBook.NewPayStruct != null ? OTransferServiceBook.NewPayStruct : OTransferServiceBook.OldPayStruct;
                            OEmpSalStructNew.DBTrack = OEmpSalStructCurrent.DBTrack;
                            //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                            //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent2.Select(e=>e.;
                            OEmployeePayrollForTransfer.EmpSalStruct.Add(OEmpSalStructNew);//check for ids
                            // db.EmpSalStruct.Add(OEmpSalStructNew);
                            db.SaveChanges();

                            foreach (var ca in OEmpSalStructDetailsCurrent2)
                            {
                                var OEmpSalStructDetail = new EmpSalStructDetails();
                                //PayScaleAssignment OPayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == ca.Id).Include(e => e.SalHeadFormula)
                                //    .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                                //    .Include(e => e.SalHeadFormula.Select(r => r.PayStruct)).Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                                //    .Include(e => e.SalHeadFormula.Select(r => r.FormulaType))
                                //    .AsNoTracking()
                                //    .FirstOrDefault();

                                //OEmpSalStructDetail.Amount = ca.Amount;
                                OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                OEmpSalStructDetail.SalaryHead = ca.SalaryHead;

                                OEmpSalStructDetail.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);
                                if (OEmpSalStructDetail.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                {
                                    OEmpSalStructDetail.Amount = ca.Amount;
                                }
                                else
                                {
                                    if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula != null)
                                    {
                                        OEmpSalStructDetail.Amount = 0;
                                    }
                                    else if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula == null)
                                    {
                                        OEmpSalStructDetail.Amount = ca.Amount;
                                    }
                                    else
                                    {
                                        OEmpSalStructDetail.Amount = 0;
                                    }
                                }
                                OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);

                            }

                            db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                            db.SaveChanges();
                            List<EmpSalStructDetails> OFAT = new List<EmpSalStructDetails>();
                            var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                            OFAT.AddRange(OEmpSalStructDetailsNew);
                            aa.EmpSalStructDetails = OFAT;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.EmpSalStruct.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                            //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                            //OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);


                            OEmpSalStructCurrent.EndDate = mEffectiveDate.AddDays(-1);
                            db.Entry(OEmpSalStructCurrent).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForTransfer, mEffectiveDate);
                            OEmployeePayrollForTransfer.EmpSalStruct.Add(OEmpSalStructNew);

                            db.SaveChanges();
                            //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                        }
                        else//check for empsalstruct
                        {
                            var OOldEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && (e.EndDate >= mEffectiveDate || e.EndDate == null))
                                                     .Include(e => e.EmpSalStructDetails)
                                //.Include(e => e.GeoStruct)
                                //.Include(e => e.FuncStruct)
                                //.Include(e => e.PayStruct)
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                                     .ToList();
                            //var OOldEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate >= mEffectiveDate || e.EndDate == null).ToList();

                            foreach (var OOldEmpSalStructRecords in OOldEmpSalStruct)
                            {
                                if (OOldEmpSalStructRecords.EffectiveDate == mEffectiveDate || OOldEmpSalStructRecords.EffectiveDate > mEffectiveDate)
                                {
                                    //Process.SalaryHeadGenProcess.EmpStructBackup(OOldEmpSalStructRecords);
                                    OOldEmpSalStructRecords.GeoStruct = OTransferServiceBook.NewGeoStruct != null ? OTransferServiceBook.NewGeoStruct : OTransferServiceBook.OldGeoStruct;
                                    OOldEmpSalStructRecords.FuncStruct = OTransferServiceBook.NewFuncStruct != null ? OTransferServiceBook.NewFuncStruct : OTransferServiceBook.OldFuncStruct;
                                    OOldEmpSalStructRecords.PayStruct = OTransferServiceBook.NewPayStruct = OTransferServiceBook.NewPayStruct != null ? OTransferServiceBook.NewPayStruct : OTransferServiceBook.OldPayStruct;
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    foreach (var OOldEmpSalStructRecordsDetails in OOldEmpSalStructRecords.EmpSalStructDetails)
                                    {


                                        //OOldEmpSalStructRecords.GeoStruct = OTransferServiceBook.NewGeoStruct != null ? OTransferServiceBook.NewGeoStruct : OTransferServiceBook.OldGeoStruct;
                                        //OOldEmpSalStructRecords.FuncStruct = OTransferServiceBook.NewFuncStruct != null ? OTransferServiceBook.NewFuncStruct : OTransferServiceBook.OldFuncStruct;
                                        //OOldEmpSalStructRecords.PayStruct = OTransferServiceBook.NewPayStruct = OTransferServiceBook.NewPayStruct != null ? OTransferServiceBook.NewPayStruct : OTransferServiceBook.OldPayStruct;



                                        SalHeadFormula OldSalForm = OOldEmpSalStructRecordsDetails.SalHeadFormula;
                                        OOldEmpSalStructRecordsDetails.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OOldEmpSalStructRecordsDetails.PayScaleAssignment, OOldEmpSalStructRecordsDetails.SalaryHead.Id);
                                        if (OOldEmpSalStructRecordsDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                        {
                                            if (OOldEmpSalStructRecordsDetails.SalHeadFormula == null && OldSalForm != null)
                                            {
                                                OOldEmpSalStructRecordsDetails.Amount = 0;
                                            }
                                            else if (OOldEmpSalStructRecordsDetails.SalHeadFormula == null && OldSalForm == null)
                                            {
                                                OOldEmpSalStructRecordsDetails.Amount = OOldEmpSalStructRecordsDetails.Amount;
                                            }
                                            else
                                            {
                                                OOldEmpSalStructRecordsDetails.Amount = 0;
                                            }
                                        }

                                        //db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                        //db.SaveChanges();

                                        //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                                        Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OOldEmpSalStructRecords, OEmployeePayrollForTransfer, OOldEmpSalStructRecords.EffectiveDate.Value.Date);
                                        OEmployeePayrollForTransfer.EmpSalStruct.Add(OOldEmpSalStructRecords);

                                        db.SaveChanges();
                                        //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                    }

                                }
                                else
                                {
                                    //Process.SalaryHeadGenProcess.EmpStructBackup(OOldEmpSalStructRecords);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                                    EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                    var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                                    //OEmpSalStructNew = OOldEmpSalStructRecords;
                                    //OEmpSalStructNew.Id=0;
                                    OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                                    OEmpSalStructNew.GeoStruct = OTransferServiceBook.NewGeoStruct;
                                    OEmpSalStructNew.FuncStruct = OTransferServiceBook.NewFuncStruct;
                                    OEmpSalStructNew.PayStruct = OTransferServiceBook.NewPayStruct;
                                    OEmpSalStructNew.DBTrack = OOldEmpSalStructRecords.DBTrack;
                                    OEmpSalStructNew.EndDate = OOldEmpSalStructRecords.EndDate;
                                    OEmployeePayrollForTransfer.EmpSalStruct.Add(OEmpSalStructNew);
                                    db.SaveChanges();
                                    foreach (var ca in OOldEmpSalStructRecords.EmpSalStructDetails)
                                    {
                                        var OEmpSalStructDetail = new EmpSalStructDetails();
                                        //OEmpSalStructDetail.Amount = ca.Amount;
                                        OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                        OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                        OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                        OEmpSalStructDetail.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);
                                        if (OEmpSalStructDetail.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            OEmpSalStructDetail.Amount = ca.Amount;
                                        }
                                        else
                                        {
                                            if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula != null)
                                            {
                                                OEmpSalStructDetail.Amount = 0;
                                            }
                                            else if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula == null)
                                            {
                                                OEmpSalStructDetail.Amount = ca.Amount;
                                            }
                                            else
                                            {
                                                OEmpSalStructDetail.Amount = 0;
                                            }
                                        }
                                        OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);

                                    }
                                    db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                                    db.SaveChanges();
                                    List<EmpSalStructDetails> OFAT = new List<EmpSalStructDetails>();
                                    var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                                    OFAT.AddRange(OEmpSalStructDetailsNew);
                                    aa.EmpSalStructDetails = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmpSalStruct.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                                    //OEmpSalStructDetailsNew = OOldEmpSalStructRecords.EmpSalStructDetails;
                                    //db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                                    //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                                    //OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                                    OOldEmpSalStructRecords.EndDate = mEffectiveDate.AddDays(-1);
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForTransfer, mEffectiveDate);
                                    OEmployeePayrollForTransfer.EmpSalStruct.Add(OEmpSalStructNew);

                                    db.SaveChanges();
                                    // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                }
                            }
                        }
                        break;
                    case "OTHER":
                        //checking for promotion date is exist in empsalstruct or new promotion date with geo,fun,paystruct changes
                        //code is to be changed for not allowing create structure for condition like lastday 
                        var OEmployeePayrollForother
                        = db.EmployeePayroll
                       .Include(e => e.Employee)
                      .Include(e => e.Employee.ServiceBookDates)
                       .Include(e => e.EmpSalStruct)
                     .Where(e => e.Id == EmployeePayrollId).FirstOrDefault();
                        OEmpSalStructCurrent = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && e.EndDate == null)
                                                      .Include(e => e.EmpSalStructDetails)
                                                      .Include(e => e.GeoStruct)
                                                      .Include(e => e.FuncStruct)
                                                      .Include(e => e.PayStruct)
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType)))
                                                      .Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment)).FirstOrDefault();

                        var OEmpSalStructDetailsCurrent3 = OEmpSalStructCurrent.EmpSalStructDetails.OrderBy(r => r.SalaryHead.SeqNo).ToList();
                        // OEmpSalStructCurrent = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                        // var OEmpSalStructDetailsCurrent3 = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).Select(e => e.EmpSalStructDetails).SingleOrDefault();

                        if (OEmpSalStructCurrent.EffectiveDate < mEffectiveDate)
                        {

                            //go for copying old structre program
                            //Process.SalaryHeadGenProcess.EmpStructBackup(OEmpSalStructCurrent);
                            //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                            var OOtherServiceBook = db.OtherServiceBook
                          .Where(e => e.Id == OOtherServiceBook_id)
                   .Include(e => e.GeoStruct)
                   .Include(e => e.NewPayScale)
                   .Include(e => e.NewPayScaleAgreement)
                   .Include(e => e.OldPayScale)
                   .Include(e => e.OldPayScaleAgreement)
                   .Include(e => e.NewPayStruct)
                   .Include(e => e.OldPayStruct)
                   .Include(e => e.OldFuncStruct)
                   .Include(e => e.NewFuncStruct)

                   .Include(e => e.OthServiceBookActivity)
                   .Include(e => e.OthServiceBookActivity.OtherSerBookActList)
                   .Include(e => e.OthServiceBookActivity.OthServiceBookPolicy)

                   .SingleOrDefault();
                            EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                            var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                            //OEmpSalStructNew = OEmpSalStructCurrent;
                            OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                            OEmpSalStructNew.GeoStruct = OOtherServiceBook.GeoStruct;
                            OEmpSalStructNew.FuncStruct = OOtherServiceBook.NewFuncStruct != null ? OOtherServiceBook.NewFuncStruct : OOtherServiceBook.OldFuncStruct;
                            OEmpSalStructNew.PayStruct = OOtherServiceBook.NewPayStruct != null ? OOtherServiceBook.NewPayStruct : OOtherServiceBook.OldPayStruct;
                            OEmpSalStructNew.DBTrack = OOtherServiceBook.DBTrack;
                            db.EmpSalStruct.Add(OEmpSalStructNew);
                            //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic

                            OEmployeePayrollForother.EmpSalStruct.Add(OEmpSalStructNew);
                            db.SaveChanges();
                            foreach (var ca in OEmpSalStructDetailsCurrent3)
                            {
                                var OEmpSalStructDetail = new EmpSalStructDetails();

                                OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                OEmpSalStructDetail.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);
                                if (OEmpSalStructDetail.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                {
                                    OEmpSalStructDetail.Amount = ca.Amount;
                                }
                                else
                                {
                                    if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula != null)
                                    {
                                        OEmpSalStructDetail.Amount = 0;
                                    }
                                    else if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula == null)
                                    {
                                        OEmpSalStructDetail.Amount = ca.Amount;
                                    }
                                    else
                                    {
                                        OEmpSalStructDetail.Amount = 0;
                                    }
                                }
                                OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);

                            }
                            db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                            db.SaveChanges();
                            List<EmpSalStructDetails> OFAT = new List<EmpSalStructDetails>();
                            var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                            OFAT.AddRange(OEmpSalStructDetailsNew);
                            aa.EmpSalStructDetails = OFAT;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.EmpSalStruct.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                            //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent3.ToList();
                            ////db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                            //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                            //OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);



                            OEmpSalStructCurrent.EndDate = mEffectiveDate.AddDays(-1);
                            db.Entry(OEmpSalStructCurrent).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForother, mEffectiveDate);
                            OEmployeePayrollForother.EmpSalStruct.Add(OEmpSalStructNew);

                            db.SaveChanges();

                            //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                        }
                        else//check for empsalstruct
                        {
                            var OOtherServiceBook = db.OtherServiceBook
                          .Where(e => e.Id == OOtherServiceBook_id)
                   .Include(e => e.GeoStruct)
                   .Include(e => e.NewPayScale)
                   .Include(e => e.NewPayScaleAgreement)
                   .Include(e => e.OldPayScale)
                   .Include(e => e.OldPayScaleAgreement)
                   .Include(e => e.NewPayStruct)
                   .Include(e => e.OldPayStruct)
                   .Include(e => e.OldFuncStruct)
                   .Include(e => e.NewFuncStruct)

                   .Include(e => e.OthServiceBookActivity)
                   .Include(e => e.OthServiceBookActivity.OtherSerBookActList)
                   .Include(e => e.OthServiceBookActivity.OthServiceBookPolicy)

                   .SingleOrDefault();
                            var OOldEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmployeePayrollId && (e.EndDate >= mEffectiveDate || e.EndDate == null))
                                                    .Include(e => e.EmpSalStructDetails)
                                //.Include(e => e.GeoStruct)
                                //.Include(e => e.FuncStruct)
                                //.Include(e => e.PayStruct)
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.RoundingMethod))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.GeoStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.PayStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FuncStruct)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.SalHeadFormula.Select(t => t.FormulaType)))
                                //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                                                    .ToList();
                            // var OOldEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate <= mEffectiveDate && (e.EndDate > mEffectiveDate || e.EndDate == null)).ToList();
                            //   var OOldEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate >= mEffectiveDate || e.EndDate == null).ToList();

                            foreach (var OOldEmpSalStructRecords in OOldEmpSalStruct)
                            {
                                if (OOldEmpSalStructRecords.EffectiveDate == mEffectiveDate || OOldEmpSalStructRecords.EffectiveDate > mEffectiveDate)
                                {
                                    var OEmpSalStructModi = db.EmpSalStruct.Find(OOldEmpSalStructRecords.Id);

                                    db.EmpSalStruct.Attach(OEmpSalStructModi);
                                    OEmpSalStructModi.GeoStruct = OOtherServiceBook.GeoStruct;
                                    OEmpSalStructModi.FuncStruct = OOtherServiceBook.NewFuncStruct;
                                    OEmpSalStructModi.PayStruct = OOtherServiceBook.NewPayStruct;

                                    db.Entry(OEmpSalStructModi).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //Process.SalaryHeadGenProcess.EmpStructBackup(OOldEmpSalStructRecords);
                                    foreach (var OOldEmpSalStructRecordsDetails in OOldEmpSalStructRecords.EmpSalStructDetails)
                                    {
                                        //if (OOldEmpSalStructRecordsDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                        //{
                                        //    OOldEmpSalStructRecordsDetails.Amount = 0;
                                        //}
                                        //OOldEmpSalStructRecordsDetails.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OOldEmpSalStructRecordsDetails.PayScaleAssignment, OOldEmpSalStructRecordsDetails.SalaryHead.Id);
                                        SalHeadFormula OldSalForm = OOldEmpSalStructRecordsDetails.SalHeadFormula;
                                        OOldEmpSalStructRecordsDetails.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OOldEmpSalStructRecordsDetails.PayScaleAssignment, OOldEmpSalStructRecordsDetails.SalaryHead.Id);
                                        if (OOldEmpSalStructRecordsDetails.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "BASIC")
                                        {
                                            if (OOldEmpSalStructRecordsDetails.SalHeadFormula == null && OldSalForm != null)
                                            {
                                                OOldEmpSalStructRecordsDetails.Amount = 0;
                                            }
                                            else if (OOldEmpSalStructRecordsDetails.SalHeadFormula == null && OldSalForm == null)
                                            {
                                                OOldEmpSalStructRecordsDetails.Amount = OOldEmpSalStructRecordsDetails.Amount;
                                            }
                                            else
                                            {
                                                OOldEmpSalStructRecordsDetails.Amount = 0;
                                            }
                                        }

                                    }
                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OOldEmpSalStructRecords, OEmployeePayrollForother, OOldEmpSalStructRecords.EffectiveDate.Value.Date);
                                    OEmployeePayrollForother.EmpSalStruct.Add(OOldEmpSalStructRecords);

                                    db.SaveChanges();
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17


                                }
                                else
                                {
                                    //Process.SalaryHeadGenProcess.EmpStructBackup(OOldEmpSalStructRecords);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                                    EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                    var OEmpSalStructDetailsNew = new List<EmpSalStructDetails>();
                                    //OEmpSalStructNew = OOldEmpSalStructRecords;
                                    OEmpSalStructNew.EffectiveDate = mEffectiveDate;
                                    OEmpSalStructNew.GeoStruct = OOtherServiceBook.GeoStruct;
                                    OEmpSalStructNew.FuncStruct = OOtherServiceBook.NewFuncStruct;
                                    OEmpSalStructNew.PayStruct = OOtherServiceBook.NewPayStruct;
                                    OEmpSalStructNew.DBTrack = OOtherServiceBook.DBTrack;
                                    OEmpSalStructNew.EndDate = OOldEmpSalStructRecords.EndDate;

                                    var OEmpSalStructDetailsTemp = OOldEmpSalStructRecords.EmpSalStructDetails;
                                    foreach (var ca in OEmpSalStructDetailsTemp)
                                    {
                                        var OEmpSalStructDetail = new EmpSalStructDetails();

                                        //OEmpSalStructDetail.Amount = ca.Amount;
                                        OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                        OEmpSalStructDetail.PayScaleAssignment = ca.PayScaleAssignment;
                                        OEmpSalStructDetail.SalaryHead = ca.SalaryHead;
                                        OEmpSalStructDetail.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);// ca.SalHeadFormula;
                                        if (OEmpSalStructDetail.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                                        {
                                            OEmpSalStructDetail.Amount = ca.Amount;
                                        }
                                        else
                                        {
                                            if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula != null)
                                            {
                                                OEmpSalStructDetail.Amount = 0;
                                            }
                                            else if (OEmpSalStructDetail.SalHeadFormula == null && ca.SalHeadFormula == null)
                                            {
                                                OEmpSalStructDetail.Amount = ca.Amount;
                                            }
                                            else
                                            {
                                                OEmpSalStructDetail.Amount = 0;
                                            }
                                        }
                                        OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);

                                    }

                                    OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsNew;
                                    OEmployeePayrollForother.EmpSalStruct.Add(OEmpSalStructNew);


                                    OOldEmpSalStructRecords.EndDate = mEffectiveDate.AddDays(-1).Date;
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    // OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                                    // db.SaveChanges();

                                    // foreach (var ca in OOldEmpSalStructRecords.EmpSalStructDetails)
                                    // {
                                    //     var OEmpSalStructDetail = new EmpSalStructDetails();
                                    //     OEmpSalStructDetail.DBTrack = ca.DBTrack;
                                    //     ca.SalHeadFormula = SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, ca.PayScaleAssignment, ca.SalaryHead.Id);
                                    //     OEmpSalStructDetailsNew.Add(OEmpSalStructDetail);
                                    //     //db.Entry(ca).State = System.Data.Entity.EntityState.Modified;
                                    //    // db.SaveChanges();

                                    // }
                                    // db.EmpSalStructDetails.AddRange(OEmpSalStructDetailsNew);
                                    // db.SaveChanges();
                                    // List<EmpSalStructDetails> OFAT = new List<EmpSalStructDetails>();
                                    // var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                                    // OFAT.AddRange(OEmpSalStructDetailsNew);
                                    //// OFAT.AddRange(OOldEmpSalStructRecords.EmpSalStructDetails);
                                    // aa.EmpSalStructDetails = OFAT;
                                    // //OEmployeePayroll.DBTrack = dbt;
                                    // db.EmpSalStruct.Attach(aa);
                                    // db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    // db.SaveChanges();

                                    // OOldEmpSalStructRecords.EndDate = Convert.ToDateTime((mEffectiveDate).AddDays(-1));
                                    // db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    // db.SaveChanges();
                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//added by prashant 13/4/17

                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayrollForother, mEffectiveDate);
                                    OEmployeePayrollForother.EmpSalStruct.Add(OEmpSalStructNew);

                                    db.SaveChanges();
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13/4/17
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        //salary backup
        public static void EmpStructBackup(EmpSalStruct OEmpSalStruct)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //go for copying old structre program
                EmpPrevSalStruct oEmpPrevSalStruct = new EmpPrevSalStruct()
                {
                    Id = OEmpSalStruct.Id,
                    EffectiveDate = OEmpSalStruct.EffectiveDate,
                    EndDate = OEmpSalStruct.EndDate,
                    FuncStruct = OEmpSalStruct.FuncStruct,
                    GeoStruct = OEmpSalStruct.GeoStruct,
                    PayDays = OEmpSalStruct.PayDays,
                    PayStruct = OEmpSalStruct.PayStruct

                };
                //create in betwwen structure
                foreach (var ca in OEmpSalStruct.EmpSalStructDetails)
                {
                    EmpPrevSalStructDetails oEmpPrevSalStructDetails = new EmpPrevSalStructDetails()
                    {
                        Id = ca.Id,
                        Amount = ca.Amount,
                        PayScaleAssignment = ca.PayScaleAssignment,
                        SalaryHead = ca.SalaryHead

                    };
                    db.EmpPrevSalStructDetails.Add(oEmpPrevSalStructDetails);
                }

                db.EmpPrevSalStruct.Add(oEmpPrevSalStruct);
                db.SaveChanges();

            }
        }
        public static void SeniorityProcess(PromotionServiceBook OPromotionServiceBook, DateTime mReleaseDate, OtherServiceBook OOtherServiceBook)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (OPromotionServiceBook != null)
                {

                    //  List<string> EmpActingStatus = new List<string>() { "ACTIVE", "SUSPEND", "ONPROBATION" };
                    var seniorityapp = db.Senioritypolicy.Include(e => e.JobStatus).ToList();
                    List<int> EmpActingStatus = new List<int>();
                    if (seniorityapp != null && seniorityapp.Count() > 0)
                    {
                        foreach (var Jobst in seniorityapp)
                        {
                            var Actingjobstat = Jobst.JobStatus.ToList();
                            foreach (var item in Actingjobstat)
                            {
                                EmpActingStatus.Add(item.Id);
                            }
                        }
                    }
                    var Employee_Id = db.EmployeePayroll.Where(e => e.Id == OPromotionServiceBook.EmployeePayroll_Id).Select(e => e.Employee_Id).SingleOrDefault();
                    var SelectEmpSeniorityData = db.Employee.Where(e => e.Id == Employee_Id)
                        .Select(e => new
                        {
                            Emp_Id = e.Id,
                            EmpCode = e.EmpCode,
                            EmpName = e.EmpName.FullNameFML.ToString(),
                            ServiceBookDates_Id = e.ServiceBookDates_Id,
                            OldSeniorityNo = e.ServiceBookDates.SeniorityNo,
                            OldSeniorityDate = e.ServiceBookDates.SeniorityDate,
                            OlsPayStruct_Id = e.PayStruct_Id
                        }).SingleOrDefault();


                    #region Logic on oldpaystruct for seniority change

                    int newSeniorityNo = 0;
                    int OldSeniorityNo = 0;
                    bool IsSuspend = false;
                    int SuspendCount = 0;

                    var EmpOldPayStruct = db.Employee.Include(e => e.PayStruct).Include(e => e.PayStruct.Grade).Where(e => e.PayStruct.Grade.Code == OPromotionServiceBook.OldPayStruct.Grade.Code
                     && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id))
                        //&& e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT"
                        //&& EmpActingStatus.Contains(e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper()))
                        //   // && Convert.ToInt32(e.ServiceBookDates.SeniorityNo) > Convert.ToInt32(SelectEmpSeniorityData.OldSeniorityNo))
                     .Select(e => new
                     {
                         Emp_Id = e.Id,
                         ServiceBookDates_Id = e.ServiceBookDates_Id,
                         SeniorityNo = e.ServiceBookDates.SeniorityNo,
                         SeniorityDate = e.ServiceBookDates.SeniorityDate,
                         OldPayStruct_Id = e.PayStruct_Id,
                         EmpActingStatus = e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                         EmpCode = e.EmpCode,
                         EmpName = e.EmpName.FullNameFML.ToString()
                     })
                        // .OrderBy(e => Convert.ToInt32(e.SeniorityNo))
                        .ToList();

                    foreach (var EmpOldPayStructitem in EmpOldPayStruct.Where(e => Convert.ToInt32(e.SeniorityNo) > Convert.ToInt32(SelectEmpSeniorityData.OldSeniorityNo)).OrderBy(e => Convert.ToInt32(e.SeniorityNo)).ToList())
                    {
                        if (EmpOldPayStructitem.EmpActingStatus != "SUSPEND")
                        {
                            if (IsSuspend == true)
                            {
                                OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                                newSeniorityNo = Convert.ToInt32(OldSeniorityNo) - 1 - SuspendCount;
                            }
                            else
                            {
                                OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                                newSeniorityNo = Convert.ToInt32(OldSeniorityNo) - 1;
                            }
                            ServiceBookDates servicebook = db.ServiceBookDates.Where(q => q.Id == EmpOldPayStructitem.ServiceBookDates_Id).SingleOrDefault();
                            //OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                            servicebook.SeniorityNo = newSeniorityNo.ToString();
                            servicebook.SeniorityDate = mReleaseDate;
                            db.ServiceBookDates.Attach(servicebook);
                            db.Entry(servicebook).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            SuspendCount = 0;
                            IsSuspend = false;
                        }
                        else
                        {
                            IsSuspend = true;
                            SuspendCount = SuspendCount + 1;

                        }
                    }


                    #endregion

                    #region NewPaystruct change

                    var EmpNewPayStructnew = db.Employee.Include(e => e.PayStruct).Include(e => e.PayStruct.Grade).Where(e => e.PayStruct.Grade.Code == OPromotionServiceBook.NewPayStruct.Grade.Code
                      && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id))
                        //&& e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT"
                        //&& EmpActingStatus.Contains(e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper()))
                     .Select(e => new
                     {
                         Emp_Id = e.Id,
                         ServiceBookDates_Id = e.ServiceBookDates_Id,
                         SeniorityNo = e.ServiceBookDates.SeniorityNo,
                         SeniorityDate = e.ServiceBookDates.SeniorityDate,
                         NewPayStruct_Id = e.PayStruct_Id,
                         EmpActingStatus = e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                         EmpCode = e.EmpCode,
                         EmpName = e.EmpName.FullNameFML.ToString()
                     })

                    // .OrderBy(e => Convert.ToInt32(e.SeniorityNo))
                      .ToList();

                    var EmpNewPayStruct = EmpNewPayStructnew.OrderBy(e => Convert.ToInt32(e.SeniorityNo)).LastOrDefault();
                    if (EmpNewPayStruct == null)
                    {
                        newSeniorityNo = 1;
                    }
                    else
                    {
                        newSeniorityNo = Convert.ToInt32(EmpNewPayStruct.SeniorityNo) + 1;
                    }
                    ServiceBookDates servicebookNew = db.ServiceBookDates.Where(q => q.Id == SelectEmpSeniorityData.ServiceBookDates_Id).SingleOrDefault();
                    //OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                    servicebookNew.SeniorityNo = newSeniorityNo.ToString();
                    servicebookNew.SeniorityDate = mReleaseDate;
                    db.ServiceBookDates.Attach(servicebookNew);
                    db.Entry(servicebookNew).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    #endregion

                    #region Seniority insert
                    var SeniorityT = new SeniorityT()
                    {
                        Emp_Id = SelectEmpSeniorityData.Emp_Id,
                        EmpCode = SelectEmpSeniorityData.EmpCode,
                        EmpName = SelectEmpSeniorityData.EmpName,
                        OldSeniorityNo = SelectEmpSeniorityData.OldSeniorityNo,
                        OldSeniorityDate = SelectEmpSeniorityData.OldSeniorityDate,
                        OldPayStruct_Id = SelectEmpSeniorityData.OlsPayStruct_Id.Value,
                        ProcessDate = DateTime.Now.Date,
                        NewPayStruct_Id = OPromotionServiceBook.NewPayStruct_Id.Value,
                        NewSeniorityNo = newSeniorityNo.ToString(),
                        NewSeniorityDate = mReleaseDate,
                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },

                    };
                    db.SeniorityT.Add(SeniorityT);
                    db.SaveChanges();

                    List<SeniorityT> _List_oSeniorityT = new List<SeniorityT>();
                    _List_oSeniorityT.Add(db.SeniorityT.Find(SeniorityT.Id));



                    if (Employee_Id == null)
                    {
                        EmployeePayroll OTEP = new EmployeePayroll()
                        {
                            Employee = db.Employee.Find(SelectEmpSeniorityData.Emp_Id),
                            SeniorityT = _List_oSeniorityT,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                        };


                        db.EmployeePayroll.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeePayroll.Include(e => e.SeniorityT).Where(e => e.Id == SelectEmpSeniorityData.Emp_Id).FirstOrDefault();
                        _List_oSeniorityT.AddRange(aa.SeniorityT);
                        aa.SeniorityT = _List_oSeniorityT;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                    }

                    //var mEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OPromotionServiceBook.EmployeePayroll_Id).SingleOrDefault();
                    //mEmployeePayroll.SeniorityT.Add(SeniorityT);
                    //db.Entry(mEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    #endregion
                }
                if (OOtherServiceBook != null)// For Retired ,Resigned,EXPIRED,DISMISS,SERVICELASTDAY,TERMINATION
                {
                    string check = OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper().ToString();
                    if (check == "SERVICELASTDAY" || check == "RETIRED" || check == "TERMINATION" || check == "VRS" || check == "RESIGNED" || check == "EXPIRED" || check == "DISMISS")
                    {

                        // List<string> EmpActingStatus = new List<string>() { "ACTIVE", "SUSPEND", "ONPROBATION" };
                        var seniorityapp = db.Senioritypolicy.Include(e => e.JobStatus).ToList();
                        List<int> EmpActingStatus = new List<int>();
                        if (seniorityapp != null && seniorityapp.Count() > 0)
                        {
                            foreach (var Jobst in seniorityapp)
                            {
                                var Actingjobstat = Jobst.JobStatus.ToList();
                                foreach (var item in Actingjobstat)
                                {
                                    EmpActingStatus.Add(item.Id);
                                }
                            }
                        }
                        var Employee_Id = db.EmployeePayroll.Where(e => e.Id == OOtherServiceBook.EmployeePayroll_Id).Select(e => e.Employee_Id).SingleOrDefault();
                        var SelectEmpSeniorityData = db.Employee.Where(e => e.Id == Employee_Id)
                            .Select(e => new
                            {
                                Emp_Id = e.Id,
                                EmpCode = e.EmpCode,
                                EmpName = e.EmpName.FullNameFML.ToString(),
                                ServiceBookDates_Id = e.ServiceBookDates_Id,
                                OldSeniorityNo = e.ServiceBookDates.SeniorityNo,
                                OldSeniorityDate = e.ServiceBookDates.SeniorityDate,
                                OlsPayStruct_Id = e.PayStruct_Id
                            }).SingleOrDefault();


                        #region Logic on oldpaystruct for seniority change

                        int newSeniorityNo = 0;
                        int OldSeniorityNo = 0;
                        bool IsSuspend = false;
                        int SuspendCount = 0;

                        var EmpOldPayStruct = db.Employee.Include(e => e.PayStruct).Include(e => e.PayStruct.Grade).Where(e => e.PayStruct.Grade.Code == OOtherServiceBook.OldPayStruct.Grade.Code
                          && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id))
                            //&& e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT"
                            //&& EmpActingStatus.Contains(e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper()))
                            //   //  && Convert.ToInt32(e.ServiceBookDates.SeniorityNo) > Convert.ToInt32(SelectEmpSeniorityData.OldSeniorityNo))
                         .Select(e => new
                         {
                             Emp_Id = e.Id,
                             ServiceBookDates_Id = e.ServiceBookDates_Id,
                             SeniorityNo = e.ServiceBookDates.SeniorityNo,
                             SeniorityDate = e.ServiceBookDates.SeniorityDate,
                             OldPayStruct_Id = e.PayStruct_Id,
                             EmpActingStatus = e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                             EmpCode = e.EmpCode,
                             EmpName = e.EmpName.FullNameFML.ToString()
                         })
                            //   .OrderBy(e => Convert.ToInt32(e.SeniorityNo))
                            .ToList();

                        foreach (var EmpOldPayStructitem in EmpOldPayStruct.Where(e => Convert.ToInt32(e.SeniorityNo) > Convert.ToInt32(SelectEmpSeniorityData.OldSeniorityNo)).OrderBy(e => Convert.ToInt32(e.SeniorityNo)).ToList())
                        {
                            if (EmpOldPayStructitem.EmpActingStatus != "SUSPEND")
                            {
                                if (IsSuspend == true)
                                {
                                    OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                                    newSeniorityNo = Convert.ToInt32(OldSeniorityNo) - 1 - SuspendCount;
                                }
                                else
                                {
                                    OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                                    newSeniorityNo = Convert.ToInt32(OldSeniorityNo) - 1;
                                }
                                ServiceBookDates servicebook = db.ServiceBookDates.Where(q => q.Id == EmpOldPayStructitem.ServiceBookDates_Id).SingleOrDefault();
                                //OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                                servicebook.SeniorityNo = newSeniorityNo.ToString();
                                servicebook.SeniorityDate = mReleaseDate;
                                db.ServiceBookDates.Attach(servicebook);
                                db.Entry(servicebook).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                SuspendCount = 0;
                                IsSuspend = false;
                            }
                            else
                            {
                                IsSuspend = true;
                                SuspendCount = SuspendCount + 1;

                            }
                        }


                        #endregion

                        #region NewPaystruct change
                        //var EmpNewPayStruct = db.Employee.Where(e => e.PayStruct_Id == OPromotionServiceBook.NewPayStruct_Id
                        // && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT"
                        // && EmpActingStatus.Contains(e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper()))
                        // .Select(e => new
                        // {
                        //     e.Id,
                        //     ServiceBookDates_Id = e.ServiceBookDates_Id,
                        //     SeniorityNo = e.ServiceBookDates.SeniorityNo,
                        //     SeniorityDate = e.ServiceBookDates.SeniorityDate,
                        //     NewPayStruct_Id = e.PayStruct_Id,
                        //     EmpActingStatus = e.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                        //     EmpCode = e.EmpCode,
                        //     EmpName = e.EmpName.FullNameFML.ToString()
                        // })
                        // .OrderBy(e => e.SeniorityNo)
                        //  .LastOrDefault();
                        //if (EmpNewPayStruct == null)
                        //{
                        //    newSeniorityNo = 1;
                        //}
                        //else
                        //{
                        //    newSeniorityNo = Convert.ToInt32(EmpNewPayStruct.SeniorityNo) + 1;
                        //}
                        //ServiceBookDates servicebookNew = db.ServiceBookDates.Where(q => q.Id == EmpNewPayStruct.ServiceBookDates_Id).SingleOrDefault();
                        ////OldSeniorityNo = Convert.ToInt32(EmpOldPayStructitem.SeniorityNo);
                        //servicebookNew.SeniorityNo = newSeniorityNo.ToString();
                        //servicebookNew.SeniorityDate = mReleaseDate;
                        //db.ServiceBookDates.Attach(servicebookNew);
                        //db.Entry(servicebookNew).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        #endregion

                        #region Seniority insert
                        var SeniorityT = new SeniorityT()
                        {
                            Emp_Id = SelectEmpSeniorityData.Emp_Id,
                            EmpCode = SelectEmpSeniorityData.EmpCode,
                            EmpName = SelectEmpSeniorityData.EmpName,
                            OldSeniorityNo = SelectEmpSeniorityData.OldSeniorityNo,
                            OldSeniorityDate = SelectEmpSeniorityData.OldSeniorityDate,
                            OldPayStruct_Id = SelectEmpSeniorityData.OlsPayStruct_Id.Value,
                            ProcessDate = DateTime.Now.Date,
                            NewPayStruct_Id = OOtherServiceBook.NewPayStruct_Id.Value,
                            NewSeniorityNo = newSeniorityNo.ToString(),
                            NewSeniorityDate = mReleaseDate,
                            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now }

                        };
                        db.SeniorityT.Add(SeniorityT);
                        db.SaveChanges();
                        List<SeniorityT> _List_oSeniorityT = new List<SeniorityT>();
                        _List_oSeniorityT.Add(db.SeniorityT.Find(SeniorityT.Id));



                        if (Employee_Id == null)
                        {
                            EmployeePayroll OTEP = new EmployeePayroll()
                            {
                                Employee = db.Employee.Find(SelectEmpSeniorityData.Emp_Id),
                                SeniorityT = _List_oSeniorityT,
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                            };


                            db.EmployeePayroll.Add(OTEP);
                            db.SaveChanges();
                        }
                        else
                        {
                            var aa = db.EmployeePayroll.Include(e => e.SeniorityT).Where(e => e.Id == SelectEmpSeniorityData.Emp_Id).FirstOrDefault();
                            _List_oSeniorityT.AddRange(aa.SeniorityT);
                            aa.SeniorityT = _List_oSeniorityT;
                            db.EmployeePayroll.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                        }

                        //var mEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OOtherServiceBook.EmployeePayroll_Id).SingleOrDefault();
                        //mEmployeePayroll.SeniorityT.Add(SeniorityT);
                        //db.Entry(mEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                        //db.SaveChanges();
                        #endregion
                    }

                }

            }
        }

        public static void ServiceBookProcess(string Narration,int mCompanyID, string mServiceBookAction, Int32? OIncrementServiceBook,
            Int32? OPromotionServiceBook, Int32? OTransferServiceBook, Int32? OOtherServiceBookData_id,
           int EmployeePayrollId, string ActivityTypeList, DateTime? mActivityDate, bool AutoIncr, bool mRegularIncrCalc, int PayScaleAgrId, Int32? OExtnRednServiceBook, bool irregular = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmployeePayroll = new EmployeePayroll();
                var OCompanyPayroll = new Company();

                switch (mServiceBookAction)
                //increment process
                {
                    case "INCREMENT_DUEDATE":
                        IncrementDueDateProcess(EmployeePayrollId, mCompanyID, "REGULAR");
                        break;
                    case "INCREMENT_PROCESS"://ok

                        IncrementProcess(Narration,EmployeePayrollId, mCompanyID, ActivityTypeList, mActivityDate, AutoIncr, mRegularIncrCalc);
                        break;
                    case "INCREMENT_RELEASE":
                        IncrementReleaseProcess(EmployeePayrollId, OIncrementServiceBook, mActivityDate);
                        break;
                    case "PROMOTION_PROCESS":
                        PromotionProcess(EmployeePayrollId, mCompanyID, ActivityTypeList, OPromotionServiceBook, mActivityDate, mRegularIncrCalc, PayScaleAgrId, irregular);
                        break;
                    case "PROMOTION_RELEASE":
                        PromotionReleaseProcess(EmployeePayrollId, OPromotionServiceBook, mActivityDate);
                        break;
                    case "TRANSFER_PROCESS":
                        //TransferProcess(OEmployeePayroll, OCompanyPayroll, ActivityTypeList, OTransferServiceBook, mActivityDate, mRegularIncrCalc);
                        break;
                    case "TRANSFER_RELEASE":

                        TransferReleaseProcess(EmployeePayrollId, OTransferServiceBook, mActivityDate);
                        break;
                    case "OTHER_PROCESS":
                        //OtherProcess(OEmployeePayroll, OCompanyPayroll, ActivityTypeList, OOtherServiceBook, mActivityDate, mRegularIncrCalc);
                        break;
                    case "OTHER_RELEASE":
                        OCompanyPayroll = db.Company
                               .Include(e => e.PayScale)
                               .Include(e => e.PayScaleAgreement)
                               .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity))
                               .Include(e => e.PayScaleAgreement.Select(d => d.OthServiceBookActivity))
                               .Include(e => e.PayScaleAgreement.Select(d => d.OthServiceBookActivity.Select(w => w.OtherSerBookActList)))
                               .Include(e => e.PayScaleAgreement.Select(d => d.OthServiceBookActivity.Select(w => w.OthServiceBookPolicy)))

                               .Where(r => r.Id == mCompanyID).SingleOrDefault();

                        OtherReleaseProcess(EmployeePayrollId, OOtherServiceBookData_id, mActivityDate);
                        break;
                    case "EXTNREDN_PROCESS":
                        //TransferProcess(OEmployeePayroll, OCompanyPayroll, ActivityTypeList, OTransferServiceBook, mActivityDate, mRegularIncrCalc);
                        break;
                    case "EXTNREDN_RELEASE":

                        ExtnRednReleaseProcess(EmployeePayrollId, OExtnRednServiceBook, mActivityDate);
                        break;
                }

            }
        }
        //increment process
        public static void IncrementDueDateProcess(int EmployeePayrollId, int CompanyId, string IncrTypeList)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //Company OCompanyPayroll = new Company();
                //OCompanyPayroll = db.Company
                //        .Include(e => e.PayScale)
                //        .Include(e => e.PayScaleAgreement)
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrList)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy.IncrPolicyDetails)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy.NonRegIncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.IncrPolicy.RegIncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.IncrActivity.Select(w => w.StagIncrPolicy)))
                //        .Include(e => e.PayScaleAgreement.Select(d => d.PayScale))
                //        .Where(r => r.Id == CompanyId).SingleOrDefault();

                Company OCompanyPayroll = db.Company.Where(r => r.Id == CompanyId).SingleOrDefault();
                List<PayScale> PayScale = db.PayScale.Where(e => e.Company_Id == CompanyId).ToList();
                OCompanyPayroll.PayScale = PayScale;
                foreach (var PayScaleitem in PayScale)
                {
                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.PayScale_Id == PayScaleitem.Id).SingleOrDefault();
                    List<IncrActivity> IncrActivity2 = db.PayScaleAgreement.Where(e => e.PayScale_Id == PayScaleitem.Id).Select(e => e.IncrActivity.ToList()).SingleOrDefault();
                    foreach (var IncrActivityitem in IncrActivity2)
                    {
                        IncrPolicy IncrPolicy2 = db.IncrPolicy.Where(e => e.Id == IncrActivityitem.IncrPolicy_Id).SingleOrDefault();
                        IncrActivityitem.IncrPolicy = IncrPolicy2;
                        IncrPolicyDetails IncrPolicyDetails2 = db.IncrPolicyDetails.Where(e => e.Id == IncrPolicy2.IncrPolicyDetails_Id).SingleOrDefault();
                        NonRegIncrPolicy NonRegIncrPolicy = db.NonRegIncrPolicy.Where(e => e.Id == IncrPolicy2.NonRegIncrPolicy_Id).SingleOrDefault();
                        RegIncrPolicy RegIncrPolicy = db.RegIncrPolicy.Where(e => e.Id == IncrPolicy2.RegIncrPolicy_Id).SingleOrDefault();
                        StagIncrPolicy StagIncrPolicy2 = db.StagIncrPolicy.Where(e => e.Id == IncrActivityitem.StagIncrPolicy_Id).SingleOrDefault();
                        IncrPolicy2.IncrPolicyDetails = IncrPolicyDetails2;
                        IncrPolicy2.NonRegIncrPolicy = NonRegIncrPolicy;
                        IncrPolicy2.RegIncrPolicy = RegIncrPolicy;
                        IncrActivityitem.IncrPolicy = IncrPolicy2;
                        IncrActivityitem.StagIncrPolicy = StagIncrPolicy2;
                        LookupValue IncrList = db.LookupValue.Where(e => e.Id == IncrActivityitem.IncrList_Id).SingleOrDefault();
                        IncrActivityitem.IncrList = IncrList;
                    }
                    PayScaleAgreement.IncrActivity = IncrActivity2;
                    PayScaleAgreement.PayScale = PayScaleitem;
                    OCompanyPayroll.PayScaleAgreement.Add(PayScaleAgreement);
                }


                EmployeePayroll OEmployeePayroll = new EmployeePayroll();
                OEmployeePayroll
                = db.EmployeePayroll.Where(e => e.Id == EmployeePayrollId).SingleOrDefault();
                Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = Employee;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == Employee.GeoStruct_Id).SingleOrDefault();
                OEmployeePayroll.Employee.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == Employee.PayStruct_Id).SingleOrDefault();
                OEmployeePayroll.Employee.PayStruct = PayStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == Employee.FuncStruct_Id).SingleOrDefault();
                OEmployeePayroll.Employee.FuncStruct = FuncStruct;
                List<SalAttendanceT> SalAttendance = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.SalAttendance = SalAttendance;
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                OEmployeePayroll.Employee.ServiceBookDates = ServiceBookDates;
                EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EndDate == null).SingleOrDefault();
                OEmployeePayroll.EmpSalStruct.Add(EmpSalStruct);
                List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStruct.Id).ToList();
                EmpSalStruct.EmpSalStructDetails = EmpSalStructDetailsList;
                foreach (var EmpSalStructDetailsitem in EmpSalStructDetailsList)
                {
                    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    SalaryHead.SalHeadOperationType = SalHeadOperationType;

                }
                List<IncrementServiceBook> IncrementServiceBook = db.IncrementServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.IncrementServiceBook = IncrementServiceBook;
                foreach (var IncrementServiceBookitem in IncrementServiceBook)
                {
                    IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == IncrementServiceBookitem.IncrActivity_Id).SingleOrDefault();
                    IncrementServiceBookitem.IncrActivity = IncrActivity;
                    StagIncrPolicy StagIncrPolicy = db.StagIncrPolicy.Where(e => e.Id == IncrActivity.StagIncrPolicy_Id).SingleOrDefault();
                    IncrActivity.StagIncrPolicy = StagIncrPolicy;

                }
                IncrDataCalc IncrDataCalc = db.IncrDataCalc.Where(e => e.Id == OEmployeePayroll.IncrDataCalc_Id).SingleOrDefault();
                OEmployeePayroll.IncrDataCalc = IncrDataCalc;
                List<PromotionServiceBook> PromotionServiceBook = db.PromotionServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                List<TransferServiceBook> TransferServiceBook = db.TransferServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                List<OtherServiceBook> OtherServiceBook = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.PromotionServiceBook = PromotionServiceBook;
                OEmployeePayroll.TransferServiceBook = TransferServiceBook;
                OEmployeePayroll.OtherServiceBook = OtherServiceBook;

                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                var OSalaryHead = OEmpSalStruct.EmpSalStructDetails
                        .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                        .SingleOrDefault();
                //var OSalHeadFormulaResult = Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OSalaryHead.SalaryHead.Id, db);
                var OSalHeadFormulaResult = OSalaryHead.SalHeadFormula; //added by Rekha13092017 as per incrementprocess
                if (OSalHeadFormulaResult == null)
                {
                    return;
                }
                var OSalHeadFormula = db.SalHeadFormula
                                    .Include(e => e.BASICDependRule)
                                    .Include(e => e.BASICDependRule.BasicScale)
                                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                    .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();

                List<BasicScaleDetails> OBasicScale = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                            .Select(t => new BasicScaleDetails
                                            {
                                                StartingSlab = t.StartingSlab,
                                                EndingSlab = t.EndingSlab,
                                                IncrementAmount = t.IncrementAmount,
                                                IncrementCount = t.IncrementCount,
                                                EBMark = t.EBMark,

                                            }
                                                ).ToList();
                double mOldBasic = 0;
                if (OBasicScale != null)
                {
                    mOldBasic = OSalaryHead.Amount;
                }



                var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity.Select(t => t.IncrList)))
                   .Where(e => e.EndDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault();
                if (test != null)
                {
                    var test1 = test.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity).ToList();
                    if (test1 != null)
                    {
                        var OIncrPolicy = test1.SelectMany(e => e).Where(e => e.IncrList.LookupVal.ToUpper() == "REGULAR").SingleOrDefault();
                        if (OIncrPolicy != null)
                        {


                            RegIncrPolicy OIncrPolicyRegular = OIncrPolicy.IncrPolicy.RegIncrPolicy;

                            var OIncrActivity = OIncrPolicy;

                            NonRegIncrPolicy OIncrPolicyNonRegular = OIncrPolicy.IncrPolicy.NonRegIncrPolicy;

                            StagIncrPolicy OIncrPolicyStagRegular = OIncrPolicy.StagIncrPolicy;

                            IncrDataCalc OIncrDataCalc = new IncrDataCalc();
                            OIncrDataCalc = IncrementRegularDataCalc(OCompanyPayroll, OEmployeePayroll, OBasicScale, OIncrPolicyRegular, OIncrPolicyNonRegular, OIncrPolicyStagRegular, mOldBasic);
                            if (OIncrDataCalc != null)
                            {
                                OIncrDataCalc.IncrementActivity = OIncrActivity;
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                OIncrDataCalc.DBTrack = dbt;
                                try
                                {

                                    db.IncrDataCalc.Add(OIncrDataCalc);
                                    OEmployeePayroll.IncrDataCalc = OIncrDataCalc;
                                    db.SaveChanges();

                                }
                                catch (Exception e)
                                {

                                    throw (e);

                                }

                            }
                            else
                            {
                                StagIncrDataCalc OStagIncrDataCalc = new StagIncrDataCalc();
                                OStagIncrDataCalc = StagIncrementRegularDataCalc(OCompanyPayroll, OEmployeePayroll, OBasicScale, OIncrPolicyRegular, OIncrPolicyNonRegular, OIncrPolicyStagRegular, mOldBasic);
                                OStagIncrDataCalc.IncrementActivity = OIncrActivity;
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                OStagIncrDataCalc.DBTrack = dbt;
                                try
                                {

                                    db.StagIncrDataCalc.Add(OStagIncrDataCalc);
                                    OEmployeePayroll.StagIncrDataCalc = OStagIncrDataCalc;
                                    db.SaveChanges();

                                }
                                catch (Exception e)
                                {

                                    throw (e);

                                }
                            }

                        }
                    }
                }
            }

        }

        public static StagIncrDataCalc StagIncrementRegularDataCalc(Company OCompanyPayroll, EmployeePayroll OEmployeePayroll, List<BasicScaleDetails> OBasicScale, RegIncrPolicy OIncrPolicyRegular, NonRegIncrPolicy OIncrPolicyNonRegular, StagIncrPolicy OIncrPolicyStagRegular, double mOldBasic)
        {
            DateTime mProcessIncrDate;
            DateTime mOrignalIncrDate;
            double mLWPDays = 0;

            var OEmpIncrementHistoryRegular = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .Where(e => e.IncrActivity.IncrList.LookupVal.ToUpper() == "REGULAR")
                // .LastOrDefault();
                                       .FirstOrDefault();
            var OEmpIncrementHistoryStagnancy = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                                        .Where(e => e.IncrActivity.StagIncrPolicy != null)
                //  .LastOrDefault();
                                      .FirstOrDefault();
            var OEmpIncrementHistory = OEmployeePayroll.IncrementServiceBook.OrderByDescending(e => e.ProcessIncrDate).ToList()
                //  .LastOrDefault();
                                      .FirstOrDefault();
            DateTime[] IncrProcessActualDates = new DateTime[2];

            double mNewBasic = 0;

            if (OEmpIncrementHistoryStagnancy != null && OEmpIncrementHistoryStagnancy.StagnancyAppl == true)
            {
                mNewBasic = mOldBasic;
            }
            else
            {
                mNewBasic = BasicSelector(1, OBasicScale, mOldBasic);//regular increment=1 count

            }


            IncrProcessActualDates = IncrDueDate(OEmpIncrementHistoryRegular, OEmployeePayroll, OIncrPolicyRegular, OIncrPolicyNonRegular, OIncrPolicyStagRegular);
            mOrignalIncrDate = IncrProcessActualDates[0];
            mProcessIncrDate = IncrProcessActualDates[1];


            Double[] mStagnancyData = new Double[4];
            bool mStagBool = true;
            int mstagCount = OEmpIncrementHistoryStagnancy.StagnancyCount + 1;
            string dt = OEmpIncrementHistoryStagnancy.ProcessIncrDate.Value.Day.ToString();
            string month = OEmpIncrementHistoryStagnancy.ProcessIncrDate.Value.Month.ToString();
            string yr = OEmpIncrementHistoryStagnancy.ProcessIncrDate.Value.Year.ToString();
            DateTime CalcDate = Convert.ToDateTime(dt + "/" + month + "/" + yr);
            int SpanYr = Convert.ToInt32(OIncrPolicyStagRegular.SpanYears);
            mOrignalIncrDate = OEmpIncrementHistoryStagnancy.ProcessIncrDate.Value;
            mProcessIncrDate = CalcDate.AddYears(SpanYr);


            //save the record in IncrDataCalc
            StagIncrDataCalc OStagIncrDataCalc = new StagIncrDataCalc()
            {

                LWPDays = (mProcessIncrDate - mOrignalIncrDate).Days,
                NewBasic = mNewBasic,
                OldBasic = mOldBasic,
                OrignalIncrDate = mOrignalIncrDate,
                ProcessIncrDate = mProcessIncrDate,
                StagnancyAppl = mStagBool,
                StagnancyCount = mstagCount,
                ParaSpanYear = SpanYr,
                CalcSpanYear = (mProcessIncrDate.Year - mOrignalIncrDate.Year)
            };
            return OStagIncrDataCalc;


        }

        //on promotion salheadformula search
        public static SalHeadFormula SalFormulaFinderPromotion(Int32? OPromotionServiceBook, int OSalaryHeadID)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //check it again
                string mFormulaName = "";

                var OEmployeePromotion = db.PromotionServiceBook.Where(t => t.Id == OPromotionServiceBook).SingleOrDefault();
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmployeePromotion.GeoStruct_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct = GeoStruct;
                Corporate Corporate = db.Corporate.Where(e => e.Id == GeoStruct.Corporate_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Corporate = Corporate;
                Region Region = db.Region.Where(e => e.Id == GeoStruct.Region_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Region = Region;
                Company Company = db.Company.Where(e => e.Id == GeoStruct.Company_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Company = Company;
                Division Division = db.Division.Where(e => e.Id == GeoStruct.Division_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Division = Division;
                Location Location = db.Location.Where(e => e.Id == GeoStruct.Location_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Location = Location;
                LocationObj LocationObj = db.LocationObj.Where(e => e.Id == Location.LocationObj_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Location.LocationObj = LocationObj;
                if (GeoStruct.Department_Id != null)
                {
                    Department Department = db.Department.Where(e => e.Id == GeoStruct.Department_Id).SingleOrDefault();
                    OEmployeePromotion.GeoStruct.Department = Department;
                    DepartmentObj DepartmentObj = db.DepartmentObj.Where(e => e.Id == Department.DepartmentObj_Id).SingleOrDefault();
                    OEmployeePromotion.GeoStruct.Department.DepartmentObj = DepartmentObj;
                }
                Group Group = db.Group.Where(e => e.Id == GeoStruct.Group_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Group = Group;
                Unit Unit = db.Unit.Where(e => e.Id == GeoStruct.Unit_Id).SingleOrDefault();
                OEmployeePromotion.GeoStruct.Unit = Unit;

                PayStruct NewPayStruct = db.PayStruct.Where(e => e.Id == OEmployeePromotion.NewPayStruct_Id).SingleOrDefault();
                OEmployeePromotion.NewPayStruct = NewPayStruct;
                Company NewPayStructCompany = db.Company.Where(e => e.Id == NewPayStruct.Company_Id).SingleOrDefault();
                OEmployeePromotion.NewPayStruct.Company = NewPayStructCompany;
                Grade NewPayStructGrade = db.Grade.Where(e => e.Id == NewPayStruct.Grade_Id).SingleOrDefault();
                OEmployeePromotion.NewPayStruct.Grade = NewPayStructGrade;
                Level NewPayStructLevel = db.Level.Where(e => e.Id == NewPayStruct.Level_Id).SingleOrDefault();
                OEmployeePromotion.NewPayStruct.Level = NewPayStructLevel;
                JobStatus NewPayStructJobStatus = db.JobStatus.Where(e => e.Id == NewPayStruct.JobStatus_Id).SingleOrDefault();
                OEmployeePromotion.NewPayStruct.JobStatus = NewPayStructJobStatus;

                FuncStruct NewFuncStruct = db.FuncStruct.Where(e => e.Id == OEmployeePromotion.NewFuncStruct_Id).SingleOrDefault();
                OEmployeePromotion.NewFuncStruct = NewFuncStruct;
                Company NewFuncStructCompany = db.Company.Where(e => e.Id == NewFuncStruct.Company_Id).SingleOrDefault();
                OEmployeePromotion.NewFuncStruct.Company = NewFuncStructCompany;
                Job NewFuncStructtJob = db.Job.Where(e => e.Id == NewFuncStruct.Job_Id).SingleOrDefault();
                OEmployeePromotion.NewFuncStruct.Job = NewFuncStructtJob;
                JobPosition NewFuncStructJobPosition = db.JobPosition.Where(e => e.Id == NewFuncStruct.JobPosition_Id).SingleOrDefault();
                OEmployeePromotion.NewFuncStruct.JobPosition = NewFuncStructJobPosition;

                PayScaleAgreement NewPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OEmployeePromotion.NewPayScaleAgreement_Id).SingleOrDefault();
                OEmployeePromotion.NewPayScaleAgreement = NewPayScaleAgreement;





                var OPayScaleAssignment = db.PayScaleAssignment.Include(e => e.SalHeadFormula)
                                        .Where(e => e.PayScaleAgreement.Id == OEmployeePromotion.NewPayScaleAgreement.Id
                                        && e.SalaryHead.Id == OSalaryHeadID).ToList();
                foreach (var OPayScaleAssignmentitem in OPayScaleAssignment)
                {


                    List<SalHeadFormula> SalHeadFormulaList = db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentitem.Id).Select(r => r.SalHeadFormula.ToList()).SingleOrDefault();
                    //OPayScaleAssignmentitem.SalHeadFormula = SalHeadFormulaList;
                    foreach (var SalHeadFormulaListitem in SalHeadFormulaList)
                    {
                        OPayScaleAssignmentitem.SalHeadFormula.Add(SalHeadFormulaListitem);
                        BASICDependRule BASICDependRule = db.BASICDependRule.Where(e => e.Id == SalHeadFormulaListitem.BASICDependRule_Id).SingleOrDefault();
                        SalHeadFormulaListitem.BASICDependRule = BASICDependRule;
                        FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == SalHeadFormulaListitem.FuncStruct_Id).SingleOrDefault();
                        SalHeadFormulaListitem.FuncStruct = FuncStruct;
                        PayStruct PayStruct = db.PayStruct.Where(e => e.Id == SalHeadFormulaListitem.PayStruct_Id).SingleOrDefault();
                        SalHeadFormulaListitem.PayStruct = PayStruct;
                        GeoStruct GeoStruct1 = db.GeoStruct.Where(e => e.Id == SalHeadFormulaListitem.GeoStruct_Id).SingleOrDefault();
                        SalHeadFormulaListitem.GeoStruct = GeoStruct1;
                        if (GeoStruct1 != null)
                        {
                            Division Division1 = db.Division.Where(e => e.Id == GeoStruct1.Division_Id).SingleOrDefault();
                            SalHeadFormulaListitem.GeoStruct.Division = Division1;
                            Location Location1 = db.Location.Where(e => e.Id == GeoStruct1.Location_Id).SingleOrDefault();
                            SalHeadFormulaListitem.GeoStruct.Location = Location1;
                            Department Department1 = db.Department.Where(e => e.Id == GeoStruct1.Department_Id).SingleOrDefault();
                            SalHeadFormulaListitem.GeoStruct.Department = Department1;
                            Group Group1 = db.Group.Where(e => e.Id == GeoStruct1.Group_Id).SingleOrDefault();
                            SalHeadFormulaListitem.GeoStruct.Group = Group1;
                            Unit Unit1 = db.Unit.Where(e => e.Id == GeoStruct1.Unit_Id).SingleOrDefault();
                            SalHeadFormulaListitem.GeoStruct.Unit = Unit1;
                        }
                        if (PayStruct != null)
                        {
                            Grade Grade1 = db.Grade.Where(e => e.Id == PayStruct.Grade_Id).SingleOrDefault();
                            SalHeadFormulaListitem.PayStruct.Grade = Grade1;
                            Level Level1 = db.Level.Where(e => e.Id == PayStruct.Level_Id).SingleOrDefault();
                            SalHeadFormulaListitem.PayStruct.Level = Level1;
                            JobStatus JobStatus1 = db.JobStatus.Where(e => e.Id == PayStruct.JobStatus_Id).SingleOrDefault();
                            SalHeadFormulaListitem.PayStruct.JobStatus = JobStatus1;
                        }
                        if (FuncStruct != null)
                        {
                            Job Job1 = db.Job.Where(e => e.Id == FuncStruct.Job_Id).SingleOrDefault();
                            SalHeadFormulaListitem.FuncStruct.Job = Job1;
                            JobPosition JobPosition1 = db.JobPosition.Where(e => e.Id == FuncStruct.JobPosition_Id).SingleOrDefault();
                            SalHeadFormulaListitem.FuncStruct.JobPosition = JobPosition1;
                        }

                    }

                }

                var OSalHeadFormulaGeo = OPayScaleAssignment
                         .Select(e => e.SalHeadFormula
                  .Where(r => r.BASICDependRule != null && r.GeoStruct != null)).SingleOrDefault();


                OSalHeadFormulaGeo = OSalHeadFormulaGeo.Where(e => e.GeoStruct.Id == OEmployeePromotion.GeoStruct.Id);

                var OSalHeadFormulaPay = OSalHeadFormulaGeo;
                OSalHeadFormulaPay = null;

                var OSalHeadFormula = OSalHeadFormulaGeo;
                if (OSalHeadFormulaGeo.SingleOrDefault() != null && OSalHeadFormulaGeo.Count() > 0)
                {
                    mFormulaName = OSalHeadFormulaGeo.Select(e => e.Name).FirstOrDefault();
                    OSalHeadFormulaPay = OPayScaleAssignment
                       .Select(e => e.SalHeadFormula
                       .Where(r => r.BASICDependRule != null && r.PayStruct != null && OEmployeePromotion.NewPayStruct != null && r.Name == mFormulaName)).SingleOrDefault();

                    if (OSalHeadFormulaPay != null || OSalHeadFormulaPay.Count() > 0)
                    {
                        OSalHeadFormulaPay = OSalHeadFormulaPay.Where(e => e.PayStruct.Id == OEmployeePromotion.NewPayStruct.Id);
                    }

                }
                else
                {
                    OSalHeadFormulaPay = OPayScaleAssignment
                         .Select(e => e.SalHeadFormula
                                         .Where(r => (r.BASICDependRule != null && r.PayStruct != null && OEmployeePromotion.NewPayStruct != null))).SingleOrDefault();

                    if (OSalHeadFormulaPay != null || OSalHeadFormulaPay.Count() > 0)
                    {
                        OSalHeadFormulaPay = OSalHeadFormulaPay.Where(e => e.PayStruct.Id == OEmployeePromotion.NewPayStruct.Id);
                    }
                }

                var OSalHeadFormulaFunc = OSalHeadFormulaGeo;
                OSalHeadFormulaFunc = null;

                if (OSalHeadFormula.SingleOrDefault() == null)
                {

                    OSalHeadFormulaFunc = OPayScaleAssignment
                                    .Select(e => e.SalHeadFormula
                  .Where(r => (r.BASICDependRule != null && r.FuncStruct != null && OEmployeePromotion.NewFuncStruct != null))
                    ).SingleOrDefault();

                    if (OSalHeadFormulaFunc != null || OSalHeadFormulaFunc.Count() > 0)
                    {
                        OSalHeadFormulaFunc = OSalHeadFormulaFunc.Where(e => e.FuncStruct.Id == OEmployeePromotion.NewFuncStruct.Id);
                    }
                }
                else
                {
                    mFormulaName = OSalHeadFormula.Select(e => e.Name).FirstOrDefault();
                    OSalHeadFormulaFunc = OPayScaleAssignment
                         .Select(e => e.SalHeadFormula
                         .Where(r => r.BASICDependRule != null && r.FuncStruct != null && OEmployeePromotion.NewFuncStruct != null && r.Name == mFormulaName)).SingleOrDefault();

                    if (OSalHeadFormulaFunc != null || OSalHeadFormulaFunc.Count() > 0)
                    {
                        OSalHeadFormulaFunc = OSalHeadFormulaFunc.Where(r => r.FuncStruct.Id == OEmployeePromotion.NewFuncStruct.Id);
                    }
                }

                if (OSalHeadFormulaPay.Count() > 0 && OSalHeadFormulaFunc.Count() > 0)
                    OSalHeadFormula = OSalHeadFormulaFunc;

                if (OSalHeadFormulaPay.Count() > 0 && OSalHeadFormulaFunc.Count() == 0)
                    OSalHeadFormula = OSalHeadFormulaPay;

                if (OSalHeadFormulaPay.Count() == 0 && OSalHeadFormulaFunc.Count() > 0)
                    OSalHeadFormula = OSalHeadFormulaFunc;

                foreach (var ca in OSalHeadFormula)
                {

                    return ca;

                }
                return null;
            }
        }//return salheadformula



        //Promotion Process
        public static void PromotionProcess(Int32? OEmployeePayroll_id, Int32? OCompanyPayroll_id, string PromoTypeList, Int32? OPromotionServiceBook_id, DateTime? mPromotionDate, bool mRegularIncrCalc, int PayScaleAgrId, bool Irregular = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                double mOldBasic = 0;
                double mFittmentAmount = 0;
                double mNewBasic = 0;
                //var OCompanyPayroll = db.Company
                //             .Include(e => e.PayScale)
                //             .Include(e => e.PayScaleAgreement)
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity))
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity.Select(w => w.PromoList)))
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity.Select(w => w.PromoPolicy)))
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity.Select(w => w.PromoPolicy.IncrActivity)))
                //             .Where(r => r.Id == OCompanyPayroll_id).SingleOrDefault();
                //var OCompanyPayroll = db.Company.Where(r => r.Id == OCompanyPayroll_id).SingleOrDefault();
                //List<PayScale> PayScale=db.PayScale.Where(e=>e.Company_Id==)
                //             .Include(e => e.PayScale)
                //             .Include(e => e.PayScaleAgreement)
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity))
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity.Select(w => w.PromoList)))
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity.Select(w => w.PromoPolicy)))
                //             .Include(e => e.PayScaleAgreement.Select(d => d.PromoActivity.Select(w => w.PromoPolicy.IncrActivity)))

                Company OCompanyPayroll = db.Company.Where(r => r.Id == OCompanyPayroll_id).SingleOrDefault();
                List<PayScale> PayScale = db.PayScale.Where(e => e.Company_Id == OCompanyPayroll_id).ToList();
                OCompanyPayroll.PayScale = PayScale;
                foreach (var PayScaleitem in PayScale)
                {
                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.PayScale_Id == PayScaleitem.Id).SingleOrDefault();
                    List<PromoActivity> PromoActivity = db.PayScaleAgreement.Where(e => e.PayScale_Id == PayScaleitem.Id).Select(e => e.PromoActivity.ToList()).SingleOrDefault();
                    foreach (var PromoActivityitem in PromoActivity)
                    {
                        PromoPolicy PromoPolicy = db.PromoPolicy.Where(e => e.Id == PromoActivityitem.PromoPolicy_Id).SingleOrDefault();
                        PromoActivityitem.PromoPolicy = PromoPolicy;
                        IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                        PromoPolicy.IncrActivity = IncrActivity;
                        PromoActivityitem.PromoList = db.PromoActivity.Where(e => e.Id == PromoActivityitem.Id).Select(r => r.PromoList).SingleOrDefault();

                    }
                    PayScaleAgreement.PromoActivity = PromoActivity;
                    PayScaleAgreement.PayScale = PayScaleitem;
                    OCompanyPayroll.PayScaleAgreement.Add(PayScaleAgreement);
                }


                //var OEmployeePayroll = db.EmployeePayroll

                //   .Include(e => e.EmpSalStruct)
                //   .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //   .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //   .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                //   .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))

                //   .Include(e => e.PromotionServiceBook)
                //   .Include(e => e.PromotionServiceBook.Select(a => a.PromotionActivity))
                //   .Where(e => e.Id == OEmployeePayroll_id).SingleOrDefault();
                var OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_id).SingleOrDefault();
                EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EndDate == null).SingleOrDefault();
                OEmployeePayroll.EmpSalStruct.Add(EmpSalStruct);
                List<EmpSalStructDetails> EmpSalStructDetailsList = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStruct.Id).ToList();
                EmpSalStruct.EmpSalStructDetails = EmpSalStructDetailsList;
                foreach (var EmpSalStructDetailsitem in EmpSalStructDetailsList)
                {
                    SalHeadFormula SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                    SalaryHead.SalHeadOperationType = SalHeadOperationType;

                }
                List<PromotionServiceBook> PromotionServiceBook = db.PromotionServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.PromotionServiceBook = PromotionServiceBook;
                foreach (var PromotionServiceBookBookitem in PromotionServiceBook)
                {
                    PromoActivity PromotionActivity = db.PromoActivity.Where(e => e.Id == PromotionServiceBookBookitem.PromotionActivity_Id).SingleOrDefault();
                    PromotionServiceBookBookitem.PromotionActivity = PromotionActivity;

                }

                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                var OSalaryHead = OEmpSalStruct.EmpSalStructDetails
                        .Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC")
                        .SingleOrDefault();
                var OSalHeadFormulaResult = OSalaryHead.SalHeadFormula;
                List<BasicScaleDetails> OBasicScaleOld = null;
                if (OSalHeadFormulaResult != null)
                {


                    var OSalHeadFormula = db.SalHeadFormula
                                        .Include(e => e.BASICDependRule)
                                        .Include(e => e.BASICDependRule.BasicScale)
                                        .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                        .Where(e => e.Id == OSalHeadFormulaResult.Id).SingleOrDefault();




                    OBasicScaleOld = OSalHeadFormula.BASICDependRule.BasicScale.BasicScaleDetails
                                                .Select(t => new BasicScaleDetails
                                                {
                                                    StartingSlab = t.StartingSlab,
                                                    EndingSlab = t.EndingSlab,
                                                    IncrementAmount = t.IncrementAmount,
                                                    IncrementCount = t.IncrementCount,
                                                    EBMark = t.EBMark,

                                                }
                                                    ).ToList();
                }
                var oCompanyId = int.Parse(SessionManager.CompanyId);
                var CompCode = db.Company.Where(e => e.Id == oCompanyId).Select(e => e.Code).SingleOrDefault();

                double oAmount = 0;

                if (OBasicScaleOld != null)
                {

                    mOldBasic = OSalaryHead.Amount;
                    if (CompCode.ToUpper() == "ACABL")
                    {
                        oAmount = OEmpSalStruct.EmpSalStructDetails
                        .Where(r => r.SalaryHead.Code.ToUpper() == "GRADEPAY").Select(e => e.Amount)
                        .SingleOrDefault();
                    }
                    //double oAmount = 0;

                    // mOldBasic += oAmount;
                }

                //after saving changes in promotion service book SalFormulaFinderPromotion
                var OSalHeadFormulaResult1 = SalFormulaFinderPromotion(OPromotionServiceBook_id, OSalaryHead.SalaryHead.Id);
                List<BasicScaleDetails> OBasicScale1 = null;
                if (OSalHeadFormulaResult1 != null)
                {
                    var OSalHeadFormula1 = db.SalHeadFormula
                                    .Include(e => e.BASICDependRule)
                                    .Include(e => e.BASICDependRule.BasicScale)
                                    .Include(e => e.BASICDependRule.BasicScale.BasicScaleDetails)
                                    .Where(e => e.Id == OSalHeadFormulaResult1.Id).SingleOrDefault();

                    OBasicScale1 = OSalHeadFormula1.BASICDependRule.BasicScale.BasicScaleDetails
                                                 .Select(t => new BasicScaleDetails
                                                 {
                                                     StartingSlab = t.StartingSlab,
                                                     EndingSlab = t.EndingSlab,
                                                     IncrementAmount = t.IncrementAmount,
                                                     IncrementCount = t.IncrementCount,
                                                     EBMark = t.EBMark,
                                                 }).ToList();

                }


                List<BasicScaleDetails> OBasicScaleNew = OBasicScale1;
                //OBasicScaleNew = OBasicScale1;
                var OPromotionServiceBook = OEmployeePayroll.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                //var OPromoPolicyTemp = OCompanyPayroll.PayScaleAgreement
                //                .Select(e => e.PromoActivity.Where(r => r.Id == OPromotionServiceBook.PromotionActivity.Id)).ToList();

                //var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity.Select(t => t.IncrList)))
                //  .Where(e => e.EndDate == null).FirstOrDefault().EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity).ToList();

                //var OIncrPolicy = test.SelectMany(e => e).Where(e => e.IncrList.LookupVal.ToUpper() == "REGULAR").SingleOrDefault();

                var test = db.EmployeePolicyStruct.Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.PromoActivity))

                    .Where(e => e.EndDate == null && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault().EmployeePolicyStructDetails.SelectMany(r => r.PolicyFormula.PromoActivity.Where(n => n.Id == OPromotionServiceBook.PromotionActivity.Id && n != null)).ToList();

                var OPromoPolicyT = test.Select(y => y.PromoPolicy).SingleOrDefault();

                //var OPromoPolicyT = OCompanyPayroll.PayScaleAgreement.Where(r => r.Id == PayScaleAgrId)
                //               .Select(e => e.PromoActivity.Where(r => r.Id == OPromotionServiceBook.PromotionActivity.Id))
                //                .Select(e => e.Select(r => r.PromoPolicy))
                //                .SingleOrDefault(); //Changed by Rekha 25012017

                // var OPromoPolicyT = OPromoPolicyTemp.Select(e => e.Select(r => r.PromoPolicy)).ToList();
                var OPromoPolicy1 = new PromoPolicy();
                //foreach (var ca in OPromoPolicyT)
                //{
                OPromoPolicy1 = OPromoPolicyT;
                //}

                var OPromoPolicy = db.PromoPolicy
                                .Where(e => e.Id == OPromoPolicy1.Id)
                                .Include(e => e.IncrActivity)
                                .Include(e => e.IncrActivity.IncrPolicy.IncrPolicyDetails)
                                .Include(e => e.IncrActivity.IncrPolicy)
                                .SingleOrDefault();

                switch (PromoTypeList.ToUpper())//remove this paramater
                {
                    case "REGULAR"://stagnancy not checked

                        //check for promo policy
                        if (OPromoPolicy != null)
                        {
                            if (OPromoPolicy.IsOldScaleIncrAction == true)
                            {
                                double mTempNewBasic = 0;
                                //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                {
                                    mTempNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleOld, mOldBasic);

                                }
                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                {
                                    mTempNewBasic = mOldBasic + ((mOldBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                }
                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                {
                                    mTempNewBasic = mOldBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                }

                                //mTempNewBasic=BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps,OBasicScaleOld,mOldBasic);
                                mFittmentAmount = BasicFittmentSelector(mTempNewBasic, OBasicScaleNew);

                                mNewBasic = mTempNewBasic + mFittmentAmount;
                                // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true start
                                if (OPromoPolicy.IsNewScaleIncrAction == true)
                                {

                                    mTempNewBasic = mNewBasic;

                                    if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                    {
                                        mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                    }
                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                    {
                                        mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                    }
                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                    {
                                        mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                    }



                                }
                                // kerla bank IsOldScaleIncrAction true and IsNewScaleIncrAction true end

                                if (CompCode.ToUpper() == "ACABL")
                                {
                                    string newb = mNewBasic.ToString("0.00");
                                    string a1 = mNewBasic.ToString().Split('.')[0];
                                    int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                    int b = Convert.ToInt32(newb.Split('.')[1]);

                                    if (b > 0 && a == 0)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                    }
                                    else if (b > 0 && a < 5)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                    }
                                    else if (b == 0 && a <= 5)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                    }
                                    else
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                    }
                                    if (OBasicScaleNew != null)
                                    {
                                        Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                        if (mNewBasic >= acblendslab)
                                        {
                                            mNewBasic = acblendslab;
                                        }
                                    }
                                }

                                //modify data to promotionservicebook
                                var OPromotionServiceBookSave = db.PromotionServiceBook.Find(OPromotionServiceBook.Id);
                                OPromotionServiceBookSave.OldBasic = mOldBasic;
                                OPromotionServiceBookSave.Fittment = mFittmentAmount;
                                OPromotionServiceBookSave.NewBasic = mNewBasic;

                                db.Entry(OPromotionServiceBookSave).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                            }
                            else if (OPromoPolicy.IsNewScaleIncrAction == true)
                            {
                                double mTempNewBasic = 0;
                                //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                mFittmentAmount = BasicFittmentSelector(mOldBasic, OBasicScaleNew);


                                mTempNewBasic = mOldBasic + mFittmentAmount;


                                if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrSteps == true)
                                {
                                    mNewBasic = BasicSelector(OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrSteps, OBasicScaleNew, mTempNewBasic);

                                }
                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                {
                                    mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                }
                                else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                {
                                    mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                }

                                if (CompCode.ToUpper() == "ACABL")
                                {
                                    string newb = mNewBasic.ToString("0.00");
                                    string a1 = mNewBasic.ToString().Split('.')[0];
                                    int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                    int b = Convert.ToInt32(newb.Split('.')[1]);

                                    if (b > 0 && a == 0)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                    }
                                    else if (b > 0 && a < 5)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                    }
                                    else if (b == 0 && a <= 5)
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                    }
                                    else
                                    {
                                        mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                    }
                                    if (OBasicScaleNew != null)
                                    {
                                        Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                        if (mNewBasic >= acblendslab)
                                        {
                                            mNewBasic = acblendslab;
                                        }
                                    }
                                }

                                //mNewBasic=mOldBasic+mFittmentAmount;
                                //modify data to promotionservicebook
                                var OPromotionServiceBookSave = db.PromotionServiceBook.Find(OPromotionServiceBook.Id);
                                OPromotionServiceBookSave.OldBasic = mOldBasic;
                                OPromotionServiceBookSave.Fittment = mFittmentAmount;
                                OPromotionServiceBookSave.NewBasic = mNewBasic;

                                db.Entry(OPromotionServiceBookSave).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 14042017
                                // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 04042017

                            }
                            else //normal without any paystruct changes
                            {
                                double mTempNewBasic = 0;
                                //mOldBasic = OBasicScaleOld.Select(e => e.OldBasic).SingleOrDefault();
                                mTempNewBasic = mOldBasic;

                                if (OPromoPolicy.IncrActivity != null)
                                {
                                    if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrPercent == true)
                                    {
                                        mNewBasic = mTempNewBasic + ((mTempNewBasic + oAmount) * OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrPercent / 100);
                                    }
                                    else if (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IsIncrAmount == true)
                                    {
                                        mNewBasic = mTempNewBasic + (OPromoPolicy.IncrActivity.IncrPolicy.IncrPolicyDetails.IncrAmount);
                                    }

                                    if (CompCode.ToUpper() == "ACABL")
                                    {
                                        string newb = mNewBasic.ToString("0.00");
                                        string a1 = mNewBasic.ToString().Split('.')[0];
                                        int a = Convert.ToInt32(a1.Substring(a1.ToString().Length - 1));
                                        int b = Convert.ToInt32(newb.Split('.')[1]);

                                        if (b > 0 && a == 0)
                                        {
                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                        }
                                        else if (b > 0 && a < 5)
                                        {
                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                        }
                                        else if (b == 0 && a <= 5)
                                        {
                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0") + 10;
                                        }
                                        else
                                        {
                                            mNewBasic = Convert.ToInt32(Convert.ToInt32(mNewBasic / 10).ToString() + "0");
                                        }
                                        if (OBasicScaleNew != null)
                                        {
                                            Double acblendslab = OBasicScaleNew.OrderBy(e => e.StartingSlab).LastOrDefault().EndingSlab;
                                            if (mNewBasic >= acblendslab)
                                            {
                                                mNewBasic = acblendslab;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    mNewBasic = mOldBasic;
                                }
                                //modify data to promotionservicebook
                                var OPromotionServiceBookSave = db.PromotionServiceBook.Find(OPromotionServiceBook.Id);
                                OPromotionServiceBookSave.OldBasic = mOldBasic;
                                OPromotionServiceBookSave.Fittment = mNewBasic - mOldBasic;
                                OPromotionServiceBookSave.NewBasic = mNewBasic;

                                db.Entry(OPromotionServiceBookSave).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        break;
                    case "NONREGULAR":

                        var OPromotionServiceBookSave1 = db.PromotionServiceBook.Find(OPromotionServiceBook.Id);

                        OPromotionServiceBookSave1.OldBasic = mOldBasic;
                        OPromotionServiceBookSave1.Fittment = mFittmentAmount;
                        OPromotionServiceBookSave1.NewBasic = mOldBasic;
                        db.Entry(OPromotionServiceBookSave1).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        break;
                    default://other increments like qualification, request etc

                        var OPromotionServiceBookSave2 = db.PromotionServiceBook.Find(OPromotionServiceBook.Id);

                        OPromotionServiceBookSave2.OldBasic = mOldBasic;
                        OPromotionServiceBookSave2.Fittment = mFittmentAmount;
                        OPromotionServiceBookSave2.NewBasic = mOldBasic;
                        db.Entry(OPromotionServiceBookSave2).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 14042017
                        // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 04042017

                        break;
                    //
                }
            }
        }
        //Promotion release process. Employee structure will be updated
        public static void PromotionReleaseProcess(Int32? OEmployeePayroll, Int32? OPromotionServiceBookData, DateTime? mReleaseDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBookData).SingleOrDefault();
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OPromotionServiceBook.GeoStruct_Id).SingleOrDefault();
                OPromotionServiceBook.GeoStruct = GeoStruct;
                PayScale NewPayScale = db.PayScale.Where(e => e.Id == OPromotionServiceBook.NewPayScale_Id).SingleOrDefault();
                OPromotionServiceBook.NewPayScale = NewPayScale;
                PayScaleAgreement NewPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                OPromotionServiceBook.NewPayScaleAgreement = NewPayScaleAgreement;
                PayScale OldPayScale = db.PayScale.Where(e => e.Id == OPromotionServiceBook.OldPayScale_Id).SingleOrDefault();
                OPromotionServiceBook.OldPayScale = OldPayScale;
                PayScaleAgreement OldPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                OPromotionServiceBook.OldPayScaleAgreement = OldPayScaleAgreement;
                PayStruct NewPayStruct = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == OPromotionServiceBook.NewPayStruct_Id).SingleOrDefault();
                OPromotionServiceBook.NewPayStruct = NewPayStruct;
                PayStruct OldPayStruct = db.PayStruct.Include(e => e.Grade).Where(e => e.Id == OPromotionServiceBook.OldPayStruct_Id).SingleOrDefault();
                OPromotionServiceBook.OldPayStruct = OldPayStruct;
                FuncStruct NewFuncStruct = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.NewFuncStruct_Id).SingleOrDefault();
                OPromotionServiceBook.NewFuncStruct = NewFuncStruct;
                FuncStruct OldFuncStruct = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.OldFuncStruct_Id).SingleOrDefault();
                OPromotionServiceBook.OldFuncStruct = OldFuncStruct;
                JobStatus NewJobStatus = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                OPromotionServiceBook.NewJobStatus = NewJobStatus;
                JobStatus OldJobStatus = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                OPromotionServiceBook.OldJobStatus = OldJobStatus;
                PromoActivity PromotionActivity = db.PromoActivity.Include(e => e.PromoList).Where(e => e.Id == OPromotionServiceBook.PromotionActivity_Id).SingleOrDefault();
                OPromotionServiceBook.PromotionActivity = PromotionActivity;
                //LookupValue PromoList = db.LookupValue.Where(e => e.Id == PromotionActivity.PromoList_Id).SingleOrDefault();
                PromotionActivity.PromoList = PromotionActivity.PromoList;
                PromoPolicy PromoPolicy = db.PromoPolicy.Where(e => e.Id == PromotionActivity.PromoPolicy_Id).SingleOrDefault();
                PromotionActivity.PromoPolicy = PromoPolicy;
                IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                PromoPolicy.IncrActivity = IncrActivity;



                //OPromotionServiceBook.ReleaseFlag = true;
                //OPromotionServiceBook.ReleaseDate = mReleaseDate;
                //db.Entry(OPromotionServiceBook).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();
                var OISSave = db.PromotionServiceBook.Find(OPromotionServiceBook.Id);
                OISSave.ReleaseFlag = true;
                OISSave.ReleaseDate = mReleaseDate;
                db.PromotionServiceBook.Attach(OISSave);
                db.Entry(OISSave).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var seniorityapp = db.Senioritypolicy.Include(e => e.JobStatus).ToList();
                List<int> EmpActingStatus = new List<int>();
                if (seniorityapp != null && seniorityapp.Count() > 0)
                {
                    foreach (var Jobst in seniorityapp)
                    {
                        var Actingjobstat = Jobst.JobStatus.ToList();
                        foreach (var item in Actingjobstat)
                        {
                            EmpActingStatus.Add(item.Id);
                        }
                    }
                    //var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();
                    var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id) && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();
                    var empstatus = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.JobStatus).Include(e => e.Employee.PayStruct.JobStatus.EmpStatus).Where(e => e.Id == OEmployeePayroll && EmpActingStatus.Contains(e.Employee.PayStruct.JobStatus.Id)).SingleOrDefault();
                    if (Seniorityobj.Count() > 0)
                    {
                        //if (empstatus.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT")
                        if (empstatus != null)
                        {
                            SeniorityProcess(OPromotionServiceBook, Convert.ToDateTime(mReleaseDate), null);
                        }
                    }
                }
                //var chk = db.Employee
                //             .Include(q => q.PayStruct)
                //             .Include(q => q.PayStruct.JobStatus)
                //             .Include(q => q.PayStruct.JobStatus.EmpActingStatus)
                //             .Include(q => q.PayStruct.JobStatus.EmpStatus)
                //             .Include(q => q.ServiceBookDates)
                //             .Where(q => q.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT"
                //                     && q.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "ACTIVE")
                //             .ToList();

                //var newpayschl = chk.Where(q => q.PayStruct.Id == OPromotionServiceBook.NewPayStruct.Id).ToList();
                //var oldpayschl = chk.Where(q => q.PayStruct.Id == OPromotionServiceBook.OldPayStruct.Id).ToList();

                //var ChkIfSeniorityNoDefinedOrNot = newpayschl.Where(q => q.ServiceBookDates.SeniorityNo != null).ToList();
                //int newSeniorityNo = 0;
                //int OldSeniorityNo = 0;
                //if (ChkIfSeniorityNoDefinedOrNot.Count() > 0)
                //{
                //    //var dfds = ChkIfSeniorityNoDefinedOrNot.OrderByDescending(q => q.ServiceBookDates.SeniorityNo).FirstOrDefault().ServiceBookDates.SeniorityNo;
                //    int ccc = Convert.ToInt16(ChkIfSeniorityNoDefinedOrNot.OrderByDescending(q => q.ServiceBookDates.SeniorityNo).FirstOrDefault().ServiceBookDates.SeniorityNo);
                //    newSeniorityNo = ccc + 1;
                //}

                //int UpdatePrevSeniority = db.EmployeePayroll
                //    //.Include(q => q.Employee)
                //    .Include(q => q.Employee.ServiceBookDates)
                //    .Where(q => q.Id == OEmployeePayroll)
                //    .Select(q => q.Employee.ServiceBookDates.Id).SingleOrDefault();
                //if (UpdatePrevSeniority != 0)
                //{
                //    ServiceBookDates servicebook = db.ServiceBookDates.Where(q => q.Id == UpdatePrevSeniority).SingleOrDefault();
                //    OldSeniorityNo = Convert.ToInt32(servicebook.SeniorityNo);
                //    servicebook.SeniorityNo = newSeniorityNo.ToString();
                //    servicebook.SeniorityDate = mReleaseDate;
                //    db.ServiceBookDates.Attach(servicebook);
                //    db.Entry(servicebook).State = System.Data.Entity.EntityState.Modified;
                //    db.SaveChanges();
                //}

                //List<ServiceBookDates> ChkIfSeniorityNoDefinedOrNotForOld = oldpayschl
                //    .Where(q => q.ServiceBookDates.SeniorityNo != null && q.ServiceBookDates.Id != UpdatePrevSeniority)
                //    .Select(q => q.ServiceBookDates).ToList();
                //if (ChkIfSeniorityNoDefinedOrNotForOld.Count() > 0)
                //{
                //    foreach (var item in ChkIfSeniorityNoDefinedOrNotForOld.Where(q => Convert.ToInt32(q.SeniorityNo) > OldSeniorityNo).OrderBy(q => q.SeniorityNo))
                //    {
                //        int DeductSeniorityNo = Convert.ToInt16(item.SeniorityNo) - 1;

                //        ServiceBookDates servicebook1 = db.ServiceBookDates.Where(q => q.Id == item.Id).SingleOrDefault();
                //        servicebook1.SeniorityNo = DeductSeniorityNo.ToString();
                //        servicebook1.SeniorityDate = mReleaseDate;
                //        db.ServiceBookDates.Attach(servicebook1);
                //        db.Entry(servicebook1).State = System.Data.Entity.EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //}


                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                //save increment if applicable
                if (OPromotionServiceBook.PromotionActivity.PromoPolicy.IncrActivity != null)
                {
                    SaveIncrementServiceBookOnPromotion(OEmployeePayroll, OPromotionServiceBook.Id);
                }

                //check for payscale changes
                if (OPromotionServiceBook.OldPayScale != null && OPromotionServiceBook.NewPayScale != null)
                {
                    if (OPromotionServiceBook.NewPayScale.Id != OPromotionServiceBook.OldPayScale.Id)
                    {

                        EmployeeSalaryStructUpdationOnPayscaleChange(OEmployeePayroll, "PROMOTION", OPromotionServiceBookData, null);
                        //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017
                        //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
                    }
                }
                //write changes in employee master data
                EmployeeMasterUpdation(OEmployeePayroll, "PROMOTION", OPromotionServiceBook.Id, null, null, null, null);
                //write process for new paystruct changes
                ServiceBookEmpSalStructChange(OEmployeePayroll, OPromotionServiceBook.ProcessPromoDate.Value, OPromotionServiceBook.NewBasic, "PROMOTION", OPromotionServiceBook.Id, null, null, null);
                //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017


                int Comp_Id = Convert.ToInt32(SessionManager.CompanyId);
                var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                int EmpId = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == OEmployeePayroll).FirstOrDefault().Employee.Id;
                int OPayScaleAgrId = db.PayScaleAgreement.Where(e => e.EndDate == null).FirstOrDefault().Id;
                //New Policy structure creation
                EmployeePayroll OEmployeePayrollObj = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                EmployeePolicyStruct OEmpPolicyStruct = db.EmployeePolicyStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpPolicyStruct != null)
                {
                    OEmpPolicyStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmployeePolicyStructCreationTest(OEmployeePayrollObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }


                EmployeeAttendance OEmployeeAttendanceObj = db.EmployeeAttendance.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                EmployeeAttendanceActionPolicyStruct OEmpAttPolicyStruct = db.EmployeeAttendanceActionPolicyStruct.Where(e => e.EmployeeAttendance_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpAttPolicyStruct != null)
                {
                    OEmpAttPolicyStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpAttPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmpolyeeAttendacePolicyStructCreationTest(OEmployeeAttendanceObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }




                EmployeeLTCStruct OEmpLTCStruct = db.EmployeeLTCStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpLTCStruct != null)
                {
                    OEmpLTCStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpLTCStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmployeeLTCStructCreationTest(OEmployeePayrollObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeLeave OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                EmployeeLvStruct OEmpLVStruct = db.EmployeeLvStruct.Where(e => e.EmployeeLeave_Id == OEmployeeLeave.Id && e.EndDate == null).FirstOrDefault();
                if (OEmpLVStruct != null)
                {
                    OEmpLVStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpLVStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Employee_Id == EmpId).SingleOrDefault();
                EmpReportingTimingStruct OEmpReportingTimingStruct = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == OEmployeeAttendance.Id && e.EndDate == null).FirstOrDefault();
                if (OEmpReportingTimingStruct != null)
                {
                    //OEmpReportingTimingStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    //db.Entry(OEmpReportingTimingStruct).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();

                    ServiceBook.EmployeeAttStructCreationTest(OEmployeeAttendance, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id, OEmpReportingTimingStruct.ReportingTimingStruct_Id);
                }
            }
        }
        //increment service book updation on Promtion increment
        public static void SaveIncrementServiceBookOnPromotion(Int32? OEmployeePayroll_id, Int32? OPromotionServiceBook_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                //var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id)
                //       .Include(e => e.GeoStruct)
                //       .Include(e => e.NewPayScale)
                //       .Include(e => e.NewPayScaleAgreement)
                //       .Include(e => e.OldPayScale)
                //       .Include(e => e.OldPayScaleAgreement)
                //       .Include(e => e.NewPayStruct)
                //       .Include(e => e.OldPayStruct)
                //       .Include(e => e.OldFuncStruct)
                //       .Include(e => e.NewFuncStruct)
                //       .Include(e => e.NewJobStatus)
                //       .Include(e => e.OldJobStatus)
                //       .Include(e => e.PromotionActivity)
                //       .Include(e => e.PromotionActivity.PromoList)
                //       .Include(e => e.PromotionActivity.PromoPolicy)
                //       .Include(e => e.PromotionActivity.PromoPolicy.IncrActivity)
                //       .SingleOrDefault();
                var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OPromotionServiceBook.GeoStruct_Id).SingleOrDefault();
                OPromotionServiceBook.GeoStruct = GeoStruct;
                PayScale NewPayScale = db.PayScale.Where(e => e.Id == OPromotionServiceBook.NewPayScale_Id).SingleOrDefault();
                OPromotionServiceBook.NewPayScale = NewPayScale;
                PayScaleAgreement NewPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                OPromotionServiceBook.NewPayScaleAgreement = NewPayScaleAgreement;
                PayScale OldPayScale = db.PayScale.Where(e => e.Id == OPromotionServiceBook.OldPayScale_Id).SingleOrDefault();
                OPromotionServiceBook.OldPayScale = OldPayScale;
                PayScaleAgreement OldPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                OPromotionServiceBook.OldPayScaleAgreement = OldPayScaleAgreement;
                PayStruct NewPayStruct = db.PayStruct.Where(e => e.Id == OPromotionServiceBook.NewPayStruct_Id).SingleOrDefault();
                OPromotionServiceBook.NewPayStruct = NewPayStruct;
                PayStruct OldPayStruct = db.PayStruct.Where(e => e.Id == OPromotionServiceBook.OldPayStruct_Id).SingleOrDefault();
                OPromotionServiceBook.OldPayStruct = OldPayStruct;
                FuncStruct NewFuncStruct = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.NewFuncStruct_Id).SingleOrDefault();
                OPromotionServiceBook.NewFuncStruct = NewFuncStruct;
                FuncStruct OldFuncStruct = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.OldFuncStruct_Id).SingleOrDefault();
                OPromotionServiceBook.OldFuncStruct = OldFuncStruct;
                JobStatus NewJobStatus = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                OPromotionServiceBook.NewJobStatus = NewJobStatus;
                JobStatus OldJobStatus = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                OPromotionServiceBook.OldJobStatus = OldJobStatus;
                PromoActivity PromotionActivity = db.PromoActivity.Include(e => e.PromoList).Where(e => e.Id == OPromotionServiceBook.PromotionActivity_Id).SingleOrDefault();
                OPromotionServiceBook.PromotionActivity = PromotionActivity;
                //LookupValue PromoList = db.LookupValue.Where(e => e.Id == PromotionActivity.PromoList_Id).SingleOrDefault();
                PromotionActivity.PromoList = PromotionActivity.PromoList;
                PromoPolicy PromoPolicy = db.PromoPolicy.Where(e => e.Id == PromotionActivity.PromoPolicy_Id).SingleOrDefault();
                PromotionActivity.PromoPolicy = PromoPolicy;
                IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                PromoPolicy.IncrActivity = IncrActivity;

                if (OPromotionServiceBook != null)
                {
                    bool IsIncrDate = OPromotionServiceBook.PromotionActivity.PromoPolicy.IsActionDateAsIncrDate;
                    IncrementServiceBook OIncrementServiceBook = new IncrementServiceBook()
                    {
                        FuncStruct = OPromotionServiceBook.NewFuncStruct,
                        PayStruct = OPromotionServiceBook.NewPayStruct,
                        GeoStruct = OPromotionServiceBook.GeoStruct,
                        IncrActivity = OPromotionServiceBook.PromotionActivity.PromoPolicy.IncrActivity,
                        ProcessIncrDate = OPromotionServiceBook.ProcessPromoDate,
                        OrignalIncrDate = OPromotionServiceBook.ProcessPromoDate,
                        NewBasic = OPromotionServiceBook.NewBasic - OPromotionServiceBook.Fittment,
                        OldBasic = OPromotionServiceBook.OldBasic,
                        StagnancyAppl = OPromotionServiceBook.StagnancyAppl,
                        StagnancyCount = OPromotionServiceBook.StagnancyCount,
                        DBTrack = dbt,
                        ReleaseFlag = true,
                        ReleaseDate = DateTime.Now,
                        IsRegularIncrDate = IsIncrDate
                    };
                    try
                    {
                        db.IncrementServiceBook.Add(OIncrementServiceBook);
                        db.SaveChanges();


                    }
                    catch (Exception e)
                    {

                        throw (e);

                    }
                    var OEmployeePayroll
                     = db.EmployeePayroll

                         .Include(e => e.IncrementServiceBook)
                         .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                         .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.StagIncrPolicy))

                         .Where(e => e.Id == OEmployeePayroll_id).SingleOrDefault();

                    OEmployeePayroll.IncrementServiceBook.Add(OIncrementServiceBook);//add ref
                    db.SaveChanges();
                }
                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
            }
        }
        //payscale change
        public static void EmployeeSalaryStructUpdationOnPayscaleChange(Int32? OEmployeePayroll_id, string mServiceBookActivity, Int32? OPromotionServiceBook_id, Int32? OOtherServiceBook_id)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

            IEnumerable<PayScaleAssignment> OPayScaleAssignment = null;
            // EmployeePayroll OEmployeePayroll = new EmployeePayroll();

            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_id).SingleOrDefault();
                Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = Employee;
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                Employee.ServiceBookDates = ServiceBookDates;
                List<EmpSalStruct> EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll_id).ToList();
                //.Include(e => e.Employee.EmpOffInfo.PayScale)
                foreach (var EmpSalStructitem in EmpSalStruct)
                {
                    List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStructitem.Id).ToList();
                    EmpSalStructitem.EmpSalStructDetails = EmpSalStructDetails;
                    foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                    {
                        SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                        EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                        SalHeadFormula SalHeadFormula = db.SalHeadFormula.Include(e => e.FormulaType).Where(e => e.Id == EmpSalStructDetailsitem.SalHeadFormula_Id).SingleOrDefault();
                        EmpSalStructDetailsitem.SalHeadFormula = SalHeadFormula;
                        SalHeadFormula.FormulaType = SalHeadFormula.FormulaType;
                        LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                        SalaryHead.SalHeadOperationType = SalHeadOperationType;
                        PayScaleAssignment PayScaleAssignment = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.SalHeadFormula.Select(r => r.FormulaType)).Where(e => e.Id == EmpSalStructDetailsitem.PayScaleAssignment_Id).SingleOrDefault();
                        EmpSalStructDetailsitem.PayScaleAssignment = PayScaleAssignment;

                    }
                }
                List<OtherServiceBook> OtherServiceBook = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.OtherServiceBook = OtherServiceBook;
                //.Include(e => e.OtherServiceBook)

                switch (mServiceBookActivity)
                {
                    case "PROMOTION":
                        var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                        GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OPromotionServiceBook.GeoStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.GeoStruct = GeoStruct;
                        PayScale NewPayScale = db.PayScale.Where(e => e.Id == OPromotionServiceBook.NewPayScale_Id).SingleOrDefault();
                        OPromotionServiceBook.NewPayScale = NewPayScale;
                        PayScaleAgreement NewPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                        OPromotionServiceBook.NewPayScaleAgreement = NewPayScaleAgreement;
                        PayScale OldPayScale = db.PayScale.Where(e => e.Id == OPromotionServiceBook.OldPayScale_Id).SingleOrDefault();
                        OPromotionServiceBook.OldPayScale = OldPayScale;
                        PayScaleAgreement OldPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                        OPromotionServiceBook.OldPayScaleAgreement = OldPayScaleAgreement;
                        PayStruct NewPayStruct = db.PayStruct.Where(e => e.Id == OPromotionServiceBook.NewPayStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.NewPayStruct = NewPayStruct;
                        PayStruct OldPayStruct = db.PayStruct.Where(e => e.Id == OPromotionServiceBook.OldPayStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.OldPayStruct = OldPayStruct;
                        FuncStruct NewFuncStruct = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.NewFuncStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.NewFuncStruct = NewFuncStruct;
                        FuncStruct OldFuncStruct = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.OldFuncStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.OldFuncStruct = OldFuncStruct;
                        JobStatus NewJobStatus = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                        OPromotionServiceBook.NewJobStatus = NewJobStatus;
                        JobStatus OldJobStatus = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                        OPromotionServiceBook.OldJobStatus = OldJobStatus;
                        PromoActivity PromotionActivity = db.PromoActivity.Include(e => e.PromoList).Where(e => e.Id == OPromotionServiceBook.PromotionActivity_Id).SingleOrDefault();
                        OPromotionServiceBook.PromotionActivity = PromotionActivity;
                        //LookupValue PromoList = db.LookupValue.Where(e => e.Id == PromotionActivity.PromoList_Id).SingleOrDefault();
                        PromotionActivity.PromoList = PromotionActivity.PromoList;
                        PromoPolicy PromoPolicy = db.PromoPolicy.Where(e => e.Id == PromotionActivity.PromoPolicy_Id).SingleOrDefault();
                        PromotionActivity.PromoPolicy = PromoPolicy;
                        IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                        PromoPolicy.IncrActivity = IncrActivity;

                        //  .Include(e => e.GeoStruct)
                        //.Include(e => e.NewPayScale)
                        //.Include(e => e.NewPayScaleAgreement)
                        //.Include(e => e.OldPayScale)
                        //.Include(e => e.OldPayScaleAgreement)
                        //.Include(e => e.NewPayStruct)
                        //.Include(e => e.OldPayStruct)
                        //.Include(e => e.OldFuncStruct)
                        //.Include(e => e.NewFuncStruct)
                        //.Include(e => e.NewJobStatus)
                        //.Include(e => e.OldJobStatus)
                        //.Include(e => e.PromotionActivity)
                        //.Include(e => e.PromotionActivity.PromoList)
                        //.Include(e => e.PromotionActivity.PromoPolicy)
                        //.Include(e => e.PromotionActivity.PromoPolicy.IncrActivity)
                        //.SingleOrDefault();

                        OPayScaleAssignment = db.PayScaleAssignment
                            .Include(x => x.SalHeadFormula)
                            .Include(x => x.SalHeadFormula.Select(y => y.FormulaType))
                        .Where(e => e.PayScaleAgreement.Id == OPromotionServiceBook.NewPayScaleAgreement.Id).ToList();

                        EmpSalStruct OEmpSalStructCurrent = null;
                        OEmpSalStructCurrent = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                        var OEmpSalStructDetailsCurrent = OEmpSalStructCurrent.EmpSalStructDetails;
                        if (OEmpSalStructCurrent.EffectiveDate < OPromotionServiceBook.ProcessPromoDate)
                        {
                            //go for copying old structre program
                            //EmpStructBackup(OEmpSalStructCurrent);
                            //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                            EmpSalStruct OEmpSalStructNew = new EmpSalStruct();

                            //OEmpSalStructNew = OEmpSalStructCurrent;
                            OEmpSalStructNew.EffectiveDate = OPromotionServiceBook.ProcessPromoDate;
                            OEmpSalStructNew.GeoStruct = OPromotionServiceBook.GeoStruct;
                            OEmpSalStructNew.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                            OEmpSalStructNew.PayStruct = OPromotionServiceBook.NewPayStruct;
                            OEmpSalStructNew.DBTrack = OEmpSalStructCurrent.DBTrack;

                            OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                            db.SaveChanges();
                            //IEnumerable<EmpSalStructDetails>OEmpSalStructDetailsCurrentTemp= null;


                            //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                            //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent;
                            var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                            var OEmpSalStructDetailsSaveList = new List<EmpSalStructDetails>();
                            //Save Old Records which is not exists in New Payscale Agreement

                            foreach (var OPayScaleAssignmentHead in OPayScaleAssignment)
                            {
                                var OEmpSalStructDetailsAdd = OEmpSalStructDetailsCurrent.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead.SalaryHead.Id).SingleOrDefault();
                                if (OEmpSalStructDetailsAdd != null)
                                {
                                    OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    OEmpSalStructDetailsSave.Amount = OEmpSalStructDetailsAdd.Amount;
                                    OEmpSalStructDetailsSave.DBTrack = OEmpSalStructDetailsAdd.DBTrack;
                                    OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                    OEmpSalStructDetailsSave.SalaryHead = OEmpSalStructDetailsAdd.SalaryHead;
                                    OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, OPayScaleAssignmentHead, OEmpSalStructDetailsAdd.SalaryHead.Id);
                                    OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);

                                }
                                else
                                {
                                    OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    OEmpSalStructDetailsSave.Amount = 0;
                                    OEmpSalStructDetailsSave.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                    OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                    OEmpSalStructDetailsSave.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                    OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);

                                    OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);
                                }
                            }

                            var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                            aa.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.EmpSalStruct.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();


                            OEmpSalStructCurrent.EndDate = OPromotionServiceBook.ProcessPromoDate.Value.AddDays(-1);
                            db.Entry(OEmpSalStructCurrent).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayroll, OPromotionServiceBook.ProcessPromoDate.Value);
                            OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);

                            db.SaveChanges();
                            //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
                        }
                        else//check for empsalstruct
                        {
                            var OOldEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate >= OPromotionServiceBook.ProcessPromoDate.Value || e.EndDate == null).ToList();
                            foreach (var OOldEmpSalStructRecords in OOldEmpSalStruct)
                            {
                                if (OOldEmpSalStructRecords.EffectiveDate == OPromotionServiceBook.ProcessPromoDate || OOldEmpSalStructRecords.EffectiveDate > OPromotionServiceBook.ProcessPromoDate.Value)
                                {
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    OOldEmpSalStructRecords.GeoStruct = OPromotionServiceBook.GeoStruct;
                                    OOldEmpSalStructRecords.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                                    OOldEmpSalStructRecords.PayStruct = OPromotionServiceBook.NewPayStruct;

                                    var OOldEmpSalStructRecordsDetails = OOldEmpSalStructRecords.EmpSalStructDetails;
                                    var OEmpSalStructDetailsList = new List<EmpSalStructDetails>();
                                    //var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    foreach (var OPayScaleAssignmentHead in OPayScaleAssignment)
                                    {
                                        var OEmpSalStructDetailsAdd = OOldEmpSalStructRecordsDetails.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead.SalaryHead.Id).SingleOrDefault();
                                        if (OEmpSalStructDetailsAdd != null)
                                        {

                                            //OEmpSalStructDetailsAdd.Amount = 0;
                                            OEmpSalStructDetailsAdd.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                            OEmpSalStructDetailsAdd.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsAdd.SalaryHead = OPayScaleAssignmentHead.SalaryHead;

                                            OEmpSalStructDetailsAdd.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);

                                            //OOldEmpSalStructRecords.EmpSalStructDetails.Add(OEmpSalStructDetailsSave);
                                            db.Entry(OEmpSalStructDetailsAdd).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalStructDetailsSave.Amount = 0;
                                            OEmpSalStructDetailsSave.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                            OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsSave.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                            OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);


                                            OOldEmpSalStructRecords.EmpSalStructDetails.Add(OEmpSalStructDetailsSave);
                                            //db.Entry(OEmpSalStructDetailsAdd).State = System.Data.Entity.EntityState.Modified;

                                        }
                                    }
                                    foreach (var OPayScaleAssignmentHead1 in OOldEmpSalStructRecordsDetails)
                                    {
                                        var OEmpSalStructDetailsAdd = OPayScaleAssignment.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead1.SalaryHead.Id).SingleOrDefault();
                                        if (OEmpSalStructDetailsAdd == null)
                                        {
                                            //OOldEmpSalStructRecords.EmpSalStructDetails.Remove(OPayScaleAssignmentHead1);
                                            //db.Entry(OPayScaleAssignmentHead1).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            OEmpSalStructDetailsList.Add(OPayScaleAssignmentHead1);
                                        }
                                    }
                                    foreach (var ca in OEmpSalStructDetailsList)
                                    {
                                        OOldEmpSalStructRecords.EmpSalStructDetails.Remove(ca);
                                    }
                                    //OOldEmpSalStructRecords.EmpSalStructDetails = OEmpSalStructDetailsList;
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                                }
                                else
                                {
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                                    EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                    //IEnumerable<EmpSalStructDetails> OEmpSalStructDetailsNew=null;
                                    //OEmpSalStructNew = OOldEmpSalStructRecords;
                                    OEmpSalStructNew.EffectiveDate = OPromotionServiceBook.ProcessPromoDate;
                                    OEmpSalStructNew.GeoStruct = OPromotionServiceBook.GeoStruct;
                                    OEmpSalStructNew.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                                    OEmpSalStructNew.PayStruct = OPromotionServiceBook.NewPayStruct;
                                    OEmpSalStructNew.EndDate = OOldEmpSalStructRecords.EndDate;
                                    OEmpSalStructNew.DBTrack = dbt;
                                    OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                                    db.SaveChanges();

                                    var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    var OEmpSalStructDetailsSaveList = new List<EmpSalStructDetails>();
                                    //Save Old Records which is not exists in New Payscale Agreement

                                    foreach (var OPayScaleAssignmentHead in OPayScaleAssignment)
                                    {
                                        var OEmpSalStructDetailsAdd = OOldEmpSalStructRecords.EmpSalStructDetails.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead.SalaryHead.Id).SingleOrDefault();
                                        if (OEmpSalStructDetailsAdd != null)
                                        {
                                            OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalStructDetailsSave.Amount = OEmpSalStructDetailsAdd.Amount;
                                            OEmpSalStructDetailsSave.DBTrack = OEmpSalStructDetailsAdd.DBTrack;
                                            OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsSave.SalaryHead = OEmpSalStructDetailsAdd.SalaryHead;
                                            OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OEmpSalStructDetailsAdd.SalaryHead.Id);

                                            OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);

                                        }
                                        else
                                        {
                                            OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalStructDetailsSave.Amount = 0;
                                            OEmpSalStructDetailsSave.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                            OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsSave.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                            OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);

                                            OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);
                                        }
                                    }
                                    //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                                    //OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);

                                    var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                                    aa.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmpSalStruct.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();


                                    OOldEmpSalStructRecords.EndDate = OPromotionServiceBook.ProcessPromoDate.Value.AddDays(-1);
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    // var OEmployeePayrollsave = db.EmployeePayroll.Find(OEmployeePayroll.Id);


                                    //db.SaveChanges();

                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayroll, OEmpSalStructNew.EffectiveDate.Value);
                                    OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);

                                    db.SaveChanges();
                                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
                                }
                            }
                        }
                        break;
                    default:
                        var OOtherServiceBook = db.OtherServiceBook
                           .Where(e => e.Id == OOtherServiceBook_id).SingleOrDefault();
                        GeoStruct GeoStruct2 = db.GeoStruct.Where(e => e.Id == OOtherServiceBook.GeoStruct_Id).SingleOrDefault();
                        OOtherServiceBook.GeoStruct = GeoStruct2;
                        PayScale NewPayScale2 = db.PayScale.Where(e => e.Id == OOtherServiceBook.NewPayScale_Id).SingleOrDefault();
                        OOtherServiceBook.NewPayScale = NewPayScale2;
                        PayScaleAgreement NewPayScaleAgreement2 = db.PayScaleAgreement.Where(e => e.Id == OOtherServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                        OOtherServiceBook.NewPayScaleAgreement = NewPayScaleAgreement2;
                        PayScale OldPayScale2 = db.PayScale.Where(e => e.Id == OOtherServiceBook.OldPayScale_Id).SingleOrDefault();
                        OOtherServiceBook.OldPayScale = OldPayScale2;
                        PayScaleAgreement OldPayScaleAgreement2 = db.PayScaleAgreement.Where(e => e.Id == OOtherServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                        OOtherServiceBook.OldPayScaleAgreement = OldPayScaleAgreement2;
                        PayStruct NewPayStruct2 = db.PayStruct.Where(e => e.Id == OOtherServiceBook.NewPayStruct_Id).SingleOrDefault();
                        OOtherServiceBook.NewPayStruct = NewPayStruct2;
                        PayStruct OldPayStruct2 = db.PayStruct.Where(e => e.Id == OOtherServiceBook.OldPayStruct_Id).SingleOrDefault();
                        OOtherServiceBook.OldPayStruct = OldPayStruct2;
                        FuncStruct OldFuncStruct2 = db.FuncStruct.Where(e => e.Id == OOtherServiceBook.OldFuncStruct_Id).SingleOrDefault();
                        OOtherServiceBook.OldFuncStruct = OldFuncStruct2;
                        FuncStruct NewFuncStruct2 = db.FuncStruct.Where(e => e.Id == OOtherServiceBook.NewFuncStruct_Id).SingleOrDefault();
                        OOtherServiceBook.NewFuncStruct = NewFuncStruct2;
                        OthServiceBookActivity OthServiceBookActivity = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Where(e => e.Id == OOtherServiceBook.OthServiceBookActivity_Id).SingleOrDefault();
                        OOtherServiceBook.OthServiceBookActivity = OthServiceBookActivity;
                        OthServiceBookActivity.OtherSerBookActList = OthServiceBookActivity.OtherSerBookActList;
                        OthServiceBookPolicy OthServiceBookPolicy = db.OthServiceBookPolicy.Where(e => e.Id == OthServiceBookActivity.OthServiceBookPolicy_Id).SingleOrDefault();
                        OthServiceBookActivity.OthServiceBookPolicy = OthServiceBookPolicy;


                        OPayScaleAssignment = db.PayScaleAssignment
                        .Where(e => e.PayScaleAgreement.Id == OOtherServiceBook.NewPayScaleAgreement.Id).ToList();

                        var OEmpSalStructCurrent1 = new EmpSalStruct();
                        OEmpSalStructCurrent1 = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                        var OEmpSalStructDetailsCurrent1 = OEmpSalStructCurrent1.EmpSalStructDetails;
                        if (OEmpSalStructCurrent1.EffectiveDate < OOtherServiceBook.ProcessOthDate)
                        {
                            //go for copying old structre program
                            //EmpStructBackup(OEmpSalStructCurrent);
                            //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                            EmpSalStruct OEmpSalStructNew = new EmpSalStruct();

                            //OEmpSalStructNew = OEmpSalStructCurrent;
                            OEmpSalStructNew.EffectiveDate = OOtherServiceBook.ProcessOthDate;
                            OEmpSalStructNew.GeoStruct = OEmpSalStructCurrent1.GeoStruct;
                            OEmpSalStructNew.FuncStruct = OOtherServiceBook.NewFuncStruct;
                            OEmpSalStructNew.PayStruct = OOtherServiceBook.NewPayStruct;
                            OEmpSalStructNew.DBTrack = OEmpSalStructCurrent1.DBTrack;

                            OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                            db.SaveChanges();

                            //IEnumerable<EmpSalStructDetails>OEmpSalStructDetailsCurrentTemp= null;


                            //copy old salstruct details to new salstructdetails and modify new salstruct basic with new basic
                            //OEmpSalStructDetailsNew = OEmpSalStructDetailsCurrent;
                            var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                            var OEmpSalStructDetailsSaveList = new List<EmpSalStructDetails>();
                            //Save Old Records which is not exists in New Payscale Agreement

                            foreach (var OPayScaleAssignmentHead in OPayScaleAssignment)
                            {
                                var OEmpSalStructDetailsAdd = OEmpSalStructDetailsCurrent1.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead.SalaryHead.Id).SingleOrDefault();
                                if (OEmpSalStructDetailsAdd != null)
                                {
                                    OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    OEmpSalStructDetailsSave.Amount = OEmpSalStructDetailsAdd.Amount;
                                    OEmpSalStructDetailsSave.DBTrack = OEmpSalStructDetailsAdd.DBTrack;
                                    OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                    OEmpSalStructDetailsSave.SalaryHead = OEmpSalStructDetailsAdd.SalaryHead;
                                    OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, OPayScaleAssignmentHead, OEmpSalStructDetailsAdd.SalaryHead.Id);
                                    OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);

                                }
                                else
                                {
                                    OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    OEmpSalStructDetailsSave.Amount = 0;
                                    OEmpSalStructDetailsSave.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                    OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                    OEmpSalStructDetailsSave.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                    OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OEmpSalStructNew, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);


                                    OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);
                                }
                            }
                            //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                            //OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                            //db.SaveChanges();
                            // var OEmployeePayrollsave = db.EmployeePayroll.Find(OEmployeePayroll.Id);


                            var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                            aa.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                            //OEmployeePayroll.DBTrack = dbt;
                            db.EmpSalStruct.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();



                            OEmpSalStructCurrent1.EndDate = OOtherServiceBook.ProcessOthDate.Value.AddDays(-1);
                            db.Entry(OEmpSalStructCurrent1).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                            Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayroll, OOtherServiceBook.ProcessOthDate.Value);
                            OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);

                            db.SaveChanges();
                            //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
                        }
                        else//check for empsalstruct
                        {
                            var OOldEmpSalStruct = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate >= OOtherServiceBook.ProcessOthDate.Value || e.EndDate == null).ToList();
                            foreach (var OOldEmpSalStructRecords in OOldEmpSalStruct)
                            {
                                if (OOldEmpSalStructRecords.EffectiveDate == OOtherServiceBook.ProcessOthDate.Value || OOldEmpSalStructRecords.EffectiveDate > OOtherServiceBook.ProcessOthDate.Value)
                                {
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    OOldEmpSalStructRecords.GeoStruct = OOldEmpSalStructRecords.GeoStruct;// OOtherServiceBook.GeoStruct;
                                    OOldEmpSalStructRecords.FuncStruct = OOtherServiceBook.NewFuncStruct;
                                    OOldEmpSalStructRecords.PayStruct = OOtherServiceBook.NewPayStruct;

                                    var OOldEmpSalStructRecordsDetails = OOldEmpSalStructRecords.EmpSalStructDetails;
                                    var OEmpSalStructDetailsList = new List<EmpSalStructDetails>();
                                    //var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    foreach (var OPayScaleAssignmentHead in OPayScaleAssignment)
                                    {
                                        var OEmpSalStructDetailsAdd = OOldEmpSalStructRecordsDetails.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead.SalaryHead.Id).SingleOrDefault();
                                        if (OEmpSalStructDetailsAdd != null)
                                        {

                                            //OEmpSalStructDetailsAdd.Amount = 0;
                                            OEmpSalStructDetailsAdd.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                            OEmpSalStructDetailsAdd.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsAdd.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                            //OOldEmpSalStructRecords.EmpSalStructDetails.Add(OEmpSalStructDetailsSave);
                                            OEmpSalStructDetailsAdd.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);

                                            db.Entry(OEmpSalStructDetailsAdd).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                                        }
                                        else
                                        {
                                            var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalStructDetailsSave.Amount = 0;
                                            OEmpSalStructDetailsSave.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                            OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsSave.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                            OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);


                                            OOldEmpSalStructRecords.EmpSalStructDetails.Add(OEmpSalStructDetailsSave);

                                            //db.Entry(OEmpSalStructDetailsAdd).State = System.Data.Entity.EntityState.Modified;

                                        }
                                    }
                                    foreach (var OOldEmpSalStructRecordsDetailsData in OOldEmpSalStructRecordsDetails)
                                    {
                                        var OEmpSalStructDetailsAdd = OPayScaleAssignment.Where(e => e.SalaryHead.Id == OOldEmpSalStructRecordsDetailsData.SalaryHead.Id).SingleOrDefault();
                                        if (OEmpSalStructDetailsAdd == null)
                                        {
                                            //OOldEmpSalStructRecords.EmpSalStructDetails.Remove(OPayScaleAssignmentHead1);
                                            //db.Entry(OPayScaleAssignmentHead1).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            OEmpSalStructDetailsList.Add(OOldEmpSalStructRecordsDetailsData);
                                        }
                                    }
                                    foreach (var ca in OEmpSalStructDetailsList)
                                    {
                                        OOldEmpSalStructRecords.EmpSalStructDetails.Remove(ca);
                                    }
                                    //OOldEmpSalStructRecords.EmpSalStructDetails = OEmpSalStructDetailsList;
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                                }
                                else
                                {
                                    //EmpStructBackup(OOldEmpSalStructRecords);
                                    //copy old sal struct to new struct and do change oldstruct end date and new sal struct effective date
                                    EmpSalStruct OEmpSalStructNew = new EmpSalStruct();
                                    //IEnumerable<EmpSalStructDetails> OEmpSalStructDetailsNew=null;
                                    //OEmpSalStructNew = OOldEmpSalStructRecords;
                                    OEmpSalStructNew.EffectiveDate = OOtherServiceBook.ProcessOthDate;
                                    OEmpSalStructNew.GeoStruct = OOldEmpSalStructRecords.GeoStruct;//OOtherServiceBook.GeoStruct;
                                    OEmpSalStructNew.FuncStruct = OOtherServiceBook.NewFuncStruct;
                                    OEmpSalStructNew.PayStruct = OOtherServiceBook.NewPayStruct;
                                    OEmpSalStructNew.EndDate = OOldEmpSalStructRecords.EndDate;
                                    OEmpSalStructNew.DBTrack = dbt;
                                    OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                                    db.SaveChanges();

                                    var OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                    var OEmpSalStructDetailsSaveList = new List<EmpSalStructDetails>();
                                    //Save Old Records which is not exists in New Payscale Agreement

                                    foreach (var OPayScaleAssignmentHead in OPayScaleAssignment)
                                    {
                                        var OEmpSalStructDetailsAdd = OOldEmpSalStructRecords.EmpSalStructDetails.Where(e => e.SalaryHead.Id == OPayScaleAssignmentHead.SalaryHead.Id).SingleOrDefault();
                                        if (OEmpSalStructDetailsAdd != null)
                                        {
                                            OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalStructDetailsSave.Amount = OEmpSalStructDetailsAdd.Amount;
                                            OEmpSalStructDetailsSave.DBTrack = OEmpSalStructDetailsAdd.DBTrack;
                                            OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsSave.SalaryHead = OEmpSalStructDetailsAdd.SalaryHead;
                                            OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OEmpSalStructDetailsAdd.SalaryHead.Id);



                                            OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);

                                        }
                                        else
                                        {
                                            OEmpSalStructDetailsSave = new EmpSalStructDetails();
                                            OEmpSalStructDetailsSave.Amount = 0;
                                            OEmpSalStructDetailsSave.DBTrack = OPayScaleAssignmentHead.DBTrack;
                                            OEmpSalStructDetailsSave.PayScaleAssignment = OPayScaleAssignmentHead;
                                            OEmpSalStructDetailsSave.SalaryHead = OPayScaleAssignmentHead.SalaryHead;
                                            OEmpSalStructDetailsSave.SalHeadFormula = OPayScaleAssignmentHead.SalHeadFormula == null ? null : SalaryHeadGenProcess.SalFormulaFinderNew(OOldEmpSalStructRecords, OPayScaleAssignmentHead, OPayScaleAssignmentHead.SalaryHead.Id);

                                            OEmpSalStructDetailsSaveList.Add(OEmpSalStructDetailsSave);
                                        }
                                    }
                                    //OEmpSalStructNew.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                                    //OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);
                                    var aa = db.EmpSalStruct.Find(OEmpSalStructNew.Id);
                                    aa.EmpSalStructDetails = OEmpSalStructDetailsSaveList;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmpSalStruct.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();


                                    OOldEmpSalStructRecords.EndDate = OOtherServiceBook.ProcessOthDate.Value.AddDays(-1);
                                    db.Entry(OOldEmpSalStructRecords).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    // var OEmployeePayrollsave = db.EmployeePayroll.Find(OEmployeePayroll.Id);


                                    //db.SaveChanges();
                                    //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017


                                    Process.SalaryHeadGenProcess.EmployeeSalaryStructUpdateNew(OEmpSalStructNew, OEmployeePayroll, OEmpSalStructNew.EffectiveDate.Value);
                                    OEmployeePayroll.EmpSalStruct.Add(OEmpSalStructNew);

                                    db.SaveChanges();
                                    // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017

                                }
                            }
                        }



                        break;

                }
            }
        }
        //employee master data updation due to service activities
        public static void EmployeeMasterUpdation(Int32? EmployeePayrollId, string mServiceBookActivity, Int32? OPromotionServiceBook_id, Int32? OTransferServiceBook_id, Int32? OOtherServiceBook_id, Int32? OIncrementServiceBook_id, Int32? OExtnRednServiceBook_id)
        {
            //backup employee master
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmployeePayroll
                   = db.EmployeePayroll.Where(e => e.Id == EmployeePayrollId).SingleOrDefault();
                Employee Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = Employee;
                Login Login = db.Login.Where(e => e.Id == Employee.Login_Id).SingleOrDefault();
                Employee.Login = Login;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == Employee.GeoStruct_Id).SingleOrDefault();
                Employee.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == Employee.PayStruct_Id).SingleOrDefault();
                Employee.PayStruct = PayStruct;
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == Employee.FuncStruct_Id).SingleOrDefault();
                Employee.FuncStruct = FuncStruct;
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                Employee.EmpOffInfo = EmpOffInfo;
                PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                EmpOffInfo.PayScale = PayScale;
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                Employee.ServiceBookDates = ServiceBookDates;


                var OEmpData = OEmployeePayroll.Employee;// db.Employee.Find(OEmployeePayroll.Employee.Id);

                switch (mServiceBookActivity)
                {
                    case "INCREMENT":
                        var OIncrementServiceBook = db.IncrementServiceBook.Find(OIncrementServiceBook_id);
                        OEmpData.ServiceBookDates.LastIncrementDate = OIncrementServiceBook.ProcessIncrDate.Value;
                        db.Entry(OEmpData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "PROMOTION":
                        var OPromotionServiceBook = db.PromotionServiceBook.Where(e => e.Id == OPromotionServiceBook_id).SingleOrDefault();
                        GeoStruct GeoStruct3 = db.GeoStruct.Where(e => e.Id == OPromotionServiceBook.GeoStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.GeoStruct = GeoStruct3;
                        PayScale NewPayScale3 = db.PayScale.Where(e => e.Id == OPromotionServiceBook.NewPayScale_Id).SingleOrDefault();
                        OPromotionServiceBook.NewPayScale = NewPayScale3;
                        //PayScaleAgreement NewPayScaleAgreement3 = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                        //OPromotionServiceBook.NewPayScaleAgreement = NewPayScaleAgreement3;
                        PayScale OldPayScale3 = db.PayScale.Where(e => e.Id == OPromotionServiceBook.OldPayScale_Id).SingleOrDefault();
                        OPromotionServiceBook.OldPayScale = OldPayScale3;
                        //PayScaleAgreement OldPayScaleAgreement3 = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldPayScaleAgreement = OldPayScaleAgreement3;
                        PayStruct NewPayStruct3 = db.PayStruct.Where(e => e.Id == OPromotionServiceBook.NewPayStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.NewPayStruct = NewPayStruct3;
                        //PayStruct OldPayStruct3 = db.PayStruct.Where(e => e.Id == OPromotionServiceBook.OldPayStruct_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldPayStruct = OldPayStruct3;
                        //FuncStruct OldFuncStruct3 = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.OldFuncStruct_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldFuncStruct = OldFuncStruct3;
                        FuncStruct NewFuncStruct3 = db.FuncStruct.Where(e => e.Id == OPromotionServiceBook.NewFuncStruct_Id).SingleOrDefault();
                        OPromotionServiceBook.NewFuncStruct = NewFuncStruct3;
                        //JobStatus NewJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                        //OPromotionServiceBook.NewJobStatus = NewJobStatus3;
                        //JobStatus OldJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldJobStatus = OldJobStatus3;
                        PromoActivity PromotionActivity = db.PromoActivity.Include(e => e.PromoList).Where(e => e.Id == OPromotionServiceBook.PromotionActivity_Id).SingleOrDefault();
                        OPromotionServiceBook.PromotionActivity = PromotionActivity;
                        OPromotionServiceBook.PromotionActivity.PromoList = PromotionActivity.PromoList;
                        //PromoPolicy PromoPolicy = db.PromoPolicy.Where(e => e.Id == PromotionActivity.PromoPolicy_Id).SingleOrDefault();
                        //PromotionActivity.PromoPolicy = PromoPolicy;
                        //IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                        //PromoPolicy.IncrActivity = IncrActivity;


                        OEmpData.GeoStruct = OPromotionServiceBook.GeoStruct;
                        OEmpData.FuncStruct = OPromotionServiceBook.NewFuncStruct;
                        OEmpData.PayStruct = OPromotionServiceBook.NewPayStruct;
                        if (OPromotionServiceBook.OldPayScale != null && OPromotionServiceBook.NewPayScale != null)
                        {
                            if (OPromotionServiceBook.OldPayScale != OPromotionServiceBook.NewPayScale)
                            {
                                OEmpData.EmpOffInfo.PayScale = OPromotionServiceBook.NewPayScale;
                                db.Entry(OEmpData.EmpOffInfo).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                            }
                        }
                        if (OPromotionServiceBook.PromotionActivity.PromoList.LookupVal.ToUpper() == "REGULAR")// ahamadnagar dcc vda calculation on joining and last promot date
                        {
                            OEmpData.ServiceBookDates.LastPromotionDate = OPromotionServiceBook.ProcessPromoDate.Value;
                        }
                        db.Entry(OEmpData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "TRANSFER":
                        var OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == OTransferServiceBook_id).SingleOrDefault();

                        GeoStruct NewGeoStruct = db.GeoStruct.Where(e => e.Id == OTransferServiceBook.NewGeoStruct_Id).SingleOrDefault();
                        OTransferServiceBook.NewGeoStruct = NewGeoStruct;
                        GeoStruct OldGeoStruct = db.GeoStruct.Where(e => e.Id == OTransferServiceBook.OldGeoStruct_Id).SingleOrDefault();
                        OTransferServiceBook.OldGeoStruct = OldGeoStruct;

                        //PayScale NewPayScale3 = db.PayScale.Where(e => e.Id == OPromotionServiceBook.NewPayScale_Id).SingleOrDefault();
                        //OPromotionServiceBook.NewPayScale = NewPayScale3;
                        //PayScaleAgreement NewPayScaleAgreement3 = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                        //OPromotionServiceBook.NewPayScaleAgreement = NewPayScaleAgreement3;
                        //PayScale OldPayScale3 = db.PayScale.Where(e => e.Id == OPromotionServiceBook.OldPayScale_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldPayScale = OldPayScale3;
                        //PayScaleAgreement OldPayScaleAgreement3 = db.PayScaleAgreement.Where(e => e.Id == OPromotionServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldPayScaleAgreement = OldPayScaleAgreement3;
                        //PayStruct NewPayStruct4 = db.PayStruct.Where(e => e.Id == OTransferServiceBook.NewPayStruct_Id).SingleOrDefault();
                        //OTransferServiceBook.NewPayStruct = NewPayStruct4;
                        //PayStruct OldPayStruct4 = db.PayStruct.Where(e => e.Id == OTransferServiceBook.OldPayStruct_Id).SingleOrDefault();
                        //OTransferServiceBook.OldPayStruct = OldPayStruct4;
                        FuncStruct OldFuncStruct4 = db.FuncStruct.Where(e => e.Id == OTransferServiceBook.OldFuncStruct_Id).SingleOrDefault();
                        OTransferServiceBook.OldFuncStruct = OldFuncStruct4;
                        FuncStruct NewFuncStruct4 = db.FuncStruct.Where(e => e.Id == OTransferServiceBook.NewFuncStruct_Id).SingleOrDefault();
                        OTransferServiceBook.NewFuncStruct = NewFuncStruct4;
                        //JobStatus NewJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                        //OPromotionServiceBook.NewJobStatus = NewJobStatus3;
                        //JobStatus OldJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldJobStatus = OldJobStatus3;
                        TransActivity TransActivity = db.TransActivity.Where(e => e.Id == OTransferServiceBook.TransActivity_Id).SingleOrDefault();
                        OTransferServiceBook.TransActivity = TransActivity;
                        //OPromotionServiceBook.PromotionActivity.PromoList = PromotionActivity.PromoList;
                        ////PromoPolicy PromoPolicy = db.PromoPolicy.Where(e => e.Id == PromotionActivity.PromoPolicy_Id).SingleOrDefault();
                        //PromotionActivity.PromoPolicy = PromoPolicy;
                        //IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                        //PromoPolicy.IncrActivity = IncrActivity;

                        OEmpData.GeoStruct = OTransferServiceBook.NewGeoStruct != null ? OTransferServiceBook.NewGeoStruct : OTransferServiceBook.OldGeoStruct;
                        OEmpData.FuncStruct = OTransferServiceBook.NewFuncStruct != null ? OTransferServiceBook.NewFuncStruct : OTransferServiceBook.NewFuncStruct;
                        OEmpData.ServiceBookDates.LastTransferDate = OTransferServiceBook.ProcessTransDate.Value;
                        //OEmployeePayroll.JobStatus=OTransferServiceBook.NewJobStatus;
                        db.Entry(OEmpData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "OTHER":
                        var OOtherServiceBook = db.OtherServiceBook
                            .Where(e => e.Id == OOtherServiceBook_id).SingleOrDefault();



                        GeoStruct GeoStruct5 = db.GeoStruct.Where(e => e.Id == OOtherServiceBook.GeoStruct_Id).SingleOrDefault();
                        OOtherServiceBook.GeoStruct = GeoStruct5;

                        PayScale NewPayScale5 = db.PayScale.Where(e => e.Id == OOtherServiceBook.NewPayScale_Id).SingleOrDefault();
                        OOtherServiceBook.NewPayScale = NewPayScale5;
                        PayScaleAgreement NewPayScaleAgreement5 = db.PayScaleAgreement.Where(e => e.Id == OOtherServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                        OOtherServiceBook.NewPayScaleAgreement = NewPayScaleAgreement5;
                        PayScale OldPayScale5 = db.PayScale.Where(e => e.Id == OOtherServiceBook.OldPayScale_Id).SingleOrDefault();
                        OOtherServiceBook.OldPayScale = OldPayScale5;
                        PayScaleAgreement OldPayScaleAgreement5 = db.PayScaleAgreement.Where(e => e.Id == OOtherServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                        OOtherServiceBook.OldPayScaleAgreement = OldPayScaleAgreement5;
                        PayStruct NewPayStruct5 = db.PayStruct.Where(e => e.Id == OOtherServiceBook.NewPayStruct_Id).SingleOrDefault();
                        OOtherServiceBook.NewPayStruct = NewPayStruct5;
                        PayStruct OldPayStruct5 = db.PayStruct.Where(e => e.Id == OOtherServiceBook.OldPayStruct_Id).SingleOrDefault();
                        OOtherServiceBook.OldPayStruct = OldPayStruct5;
                        FuncStruct OldFuncStruct5 = db.FuncStruct.Where(e => e.Id == OOtherServiceBook.OldFuncStruct_Id).SingleOrDefault();
                        OOtherServiceBook.OldFuncStruct = OldFuncStruct5;
                        FuncStruct NewFuncStruct5 = db.FuncStruct.Where(e => e.Id == OOtherServiceBook.NewFuncStruct_Id).SingleOrDefault();
                        OOtherServiceBook.NewFuncStruct = NewFuncStruct5;
                        //JobStatus NewJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                        //OPromotionServiceBook.NewJobStatus = NewJobStatus3;
                        //JobStatus OldJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                        //OPromotionServiceBook.OldJobStatus = OldJobStatus3;
                        OthServiceBookActivity OthServiceBookActivity = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Where(e => e.Id == OOtherServiceBook.OthServiceBookActivity_Id).SingleOrDefault();
                        OOtherServiceBook.OthServiceBookActivity = OthServiceBookActivity;
                        OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList = OthServiceBookActivity.OtherSerBookActList;
                        OthServiceBookPolicy OthServiceBookPolicy = db.OthServiceBookPolicy.Where(e => e.Id == OthServiceBookActivity.OthServiceBookPolicy_Id).SingleOrDefault();
                        OthServiceBookActivity.OthServiceBookPolicy = OthServiceBookPolicy;
                        //IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                        //PromoPolicy.IncrActivity = IncrActivity;


                        string PayMonth = OOtherServiceBook.ProcessOthDate.Value.ToString("MM/yyyy");
                        OEmpData.FuncStruct = OOtherServiceBook.NewFuncStruct;
                        OEmpData.PayStruct = OOtherServiceBook.NewPayStruct;
                        if (OOtherServiceBook.OldPayScale != OOtherServiceBook.NewPayScale)
                        {
                            OEmpData.EmpOffInfo.PayScale = OOtherServiceBook.NewPayScale;
                            db.Entry(OEmpData.EmpOffInfo).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RETIRED")
                        {
                            PFECRR OPFECR = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).SingleOrDefault().PFECRR;
                            OEmpData.ServiceBookDates.RetirementDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.ServiceLastDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PFExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PensionExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.ResignReason = OOtherServiceBook.Narration;
                            if (OPFECR != null)
                            {
                                OPFECR.Reason_for_leaving = OOtherServiceBook.Narration;
                                OPFECR.Date_of_Exit_from_EPF = OOtherServiceBook.ProcessOthDate.Value;
                                OPFECR.Date_of_Exit_from_EPS = OOtherServiceBook.ProcessOthDate.Value;
                                db.Entry(OPFECR).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED")
                        {
                            PFECRR OPFECR = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).SingleOrDefault().PFECRR;
                            OEmpData.ServiceBookDates.ResignationDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.ServiceLastDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.EMPRESIGNSTAT = true;
                            OEmpData.ServiceBookDates.ResignReason = OOtherServiceBook.Narration;
                            if (OPFECR != null)
                            {
                                OPFECR.Reason_for_leaving = OOtherServiceBook.Narration;
                                OPFECR.Date_of_Exit_from_EPF = OOtherServiceBook.ProcessOthDate.Value;
                                OPFECR.Date_of_Exit_from_EPS = OOtherServiceBook.ProcessOthDate.Value;
                                db.Entry(OPFECR).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED")
                        {
                            PFECRR OPFECR = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).SingleOrDefault().PFECRR;
                            OEmpData.ServiceBookDates.ServiceLastDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PFExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PensionExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.ResignReason = OOtherServiceBook.Narration;
                            if (OPFECR != null)
                            {
                                OPFECR.Reason_for_leaving = OOtherServiceBook.Narration;
                                OPFECR.Date_of_Exit_from_EPF = OOtherServiceBook.ProcessOthDate.Value;
                                OPFECR.Date_of_Exit_from_EPS = OOtherServiceBook.ProcessOthDate.Value;
                                db.Entry(OPFECR).State = System.Data.Entity.EntityState.Modified;
                            }

                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "CONFIRMATION")
                        {
                            OEmpData.ServiceBookDates.ConfirmationDate = OOtherServiceBook.ProcessOthDate.Value;

                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "PROBATION")
                        {
                            OEmpData.ServiceBookDates.ProbationDate = OOtherServiceBook.ProcessOthDate.Value;

                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION")
                        {
                            PFECRR OPFECR = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).SingleOrDefault().PFECRR;
                            OEmpData.ServiceBookDates.ServiceLastDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PFExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PensionExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.ResignReason = OOtherServiceBook.Narration;
                            OEmpData.Login.IsActive = false;
                            if (OPFECR != null)
                            {

                                OPFECR.Reason_for_leaving = OOtherServiceBook.Narration;
                                OPFECR.Date_of_Exit_from_EPF = OOtherServiceBook.ProcessOthDate.Value;
                                OPFECR.Date_of_Exit_from_EPS = OOtherServiceBook.ProcessOthDate.Value;
                                db.Entry(OPFECR).State = System.Data.Entity.EntityState.Modified;
                            }

                        }

                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "VRS")
                        {
                            PFECRR OPFECR = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).SingleOrDefault().PFECRR;
                            OEmpData.ServiceBookDates.ServiceLastDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PFExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PensionExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.ResignReason = OOtherServiceBook.Narration;
                            OEmpData.Login.IsActive = false;
                            if (OPFECR != null)
                            {

                                OPFECR.Reason_for_leaving = OOtherServiceBook.Narration;
                                OPFECR.Date_of_Exit_from_EPF = OOtherServiceBook.ProcessOthDate.Value;
                                OPFECR.Date_of_Exit_from_EPS = OOtherServiceBook.ProcessOthDate.Value;
                                db.Entry(OPFECR).State = System.Data.Entity.EntityState.Modified;
                            }

                        }
                        if (OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "SERVICELASTDAY")
                        {
                            OEmpData.ServiceBookDates.ServiceLastDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PFExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.ServiceBookDates.PensionExitDate = OOtherServiceBook.ProcessOthDate.Value;
                            OEmpData.Login.IsActive = false;
                        }

                        db.Entry(OEmpData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        break;
                    case "EXTNREDN":
                        var OExtnRednServiceBook = db.ExtnRednServiceBook.Include(e => e.Frequency)
                            .Where(e => e.Id == OExtnRednServiceBook_id).SingleOrDefault();
                        ExtnRednActivity ExtnRednActivity = db.ExtnRednActivity.Include(e => e.ExtnRednList).Where(e => e.Id == OExtnRednServiceBook.ExtnRednActivity_Id).SingleOrDefault();
                        OExtnRednServiceBook.ExtnRednActivity = ExtnRednActivity;
                        OExtnRednServiceBook.ExtnRednActivity.ExtnRednList = ExtnRednActivity.ExtnRednList;
                        ExtnRednPolicy ExtnRednPolicy = db.ExtnRednPolicy.Include(e => e.ExtnRednCauseType).Where(e => e.Id == ExtnRednActivity.ExtnRednPolicy_Id).SingleOrDefault();
                        ExtnRednActivity.ExtnRednPolicy = ExtnRednPolicy;
                        ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType = ExtnRednPolicy.ExtnRednCauseType;



                        // string PayMonth = OOtherServiceBook.ProcessOthDate.Value.ToString("MM/yyyy");
                        bool IsExtn = false;
                        bool IsRedn = false;
                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.IsExtn == true)
                            IsExtn = true;
                        else if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.IsRedn == true)
                            IsRedn = true;

                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "TRAINEEPERIOD")
                        {
                            if (IsExtn == true)
                            {
                                OEmpData.ServiceBookDates.ProbationDate = null;
                                // OEmpData.ServiceBookDates.ConfirmationDate = null;  
                            }

                        }
                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "PROBATIONPERIOD")
                        {
                            if (IsExtn == true)
                            {
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "DAYS")
                                    OEmpData.ServiceBookDates.ProbationDate = OEmpData.ServiceBookDates.ProbationDate.Value.AddDays(OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "MONTHS")
                                    OEmpData.ServiceBookDates.ProbationDate = OEmpData.ServiceBookDates.ProbationDate.Value.AddMonths(OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "YEARS")
                                    OEmpData.ServiceBookDates.ProbationDate = OEmpData.ServiceBookDates.ProbationDate.Value.AddYears(OExtnRednServiceBook.Period);

                                OEmpData.ServiceBookDates.ProbationPeriod = OEmpData.ServiceBookDates.ProbationPeriod + OExtnRednServiceBook.Period;
                            }
                            else if (IsRedn == true)
                            {
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "DAYS")
                                    OEmpData.ServiceBookDates.ProbationDate = OEmpData.ServiceBookDates.ProbationDate.Value.AddDays(-OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "MONTHS")
                                    OEmpData.ServiceBookDates.ProbationDate = OEmpData.ServiceBookDates.ProbationDate.Value.AddMonths(-OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "YEARS")
                                    OEmpData.ServiceBookDates.ProbationDate = OEmpData.ServiceBookDates.ProbationDate.Value.AddYears(-OExtnRednServiceBook.Period);

                                OEmpData.ServiceBookDates.ProbationPeriod = OEmpData.ServiceBookDates.ProbationPeriod - OExtnRednServiceBook.Period;
                            }


                        }
                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "RETIREMENTPERIOD")
                        {
                            if (IsExtn == true)
                            {
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "DAYS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddDays(OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "MONTHS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddMonths(OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "YEARS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddYears(OExtnRednServiceBook.Period);
                            }
                            else if (IsRedn == true)
                            {
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "DAYS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddDays(-OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "MONTHS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddMonths(-OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "YEARS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddYears(-OExtnRednServiceBook.Period);
                            }
                        }
                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "CONTRACTPERIOD")
                        {
                            if (IsExtn == true)
                            {
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "DAYS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddDays(OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "MONTHS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddMonths(OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "YEARS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddYears(OExtnRednServiceBook.Period);
                            }
                            else if (IsRedn == true)
                            {
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "DAYS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddDays(-OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "MONTHS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddMonths(-OExtnRednServiceBook.Period);
                                if (OExtnRednServiceBook.Frequency.LookupVal.ToUpper() == "YEARS")
                                    OEmpData.ServiceBookDates.RetirementDate = OEmpData.ServiceBookDates.RetirementDate.Value.AddYears(-OExtnRednServiceBook.Period);
                            }
                        }
                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "ONPROBATIONPERIOD")
                        {

                        }
                        if (OExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "ONDEPUTATIONPERIOD")
                        {

                        }


                        db.Entry(OEmpData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        break;
                }
                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017
                // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
            }
        }

        //transfer process
        //Transfer release process. Employee structure will be updated
        public static void TransferReleaseProcess(Int32? OEmployeePayroll, Int32? OTransferServiceBookData, DateTime? mReleaseDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OTransferServiceBook = db.TransferServiceBook.Where(e => e.Id == OTransferServiceBookData)
                        .SingleOrDefault();
                int Comp_Id = Convert.ToInt32(SessionManager.CompanyId);
                var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                int EmpId = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == OEmployeePayroll).FirstOrDefault().Employee.Id;

                OTransferServiceBook.ReleaseFlag = true;
                OTransferServiceBook.ReleaseDate = mReleaseDate;
                db.Entry(OTransferServiceBook).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                //write changes in employee master data
                EmployeeMasterUpdation(OEmployeePayroll, "TRANSFER", null, OTransferServiceBook.Id, null, null, null);

                ServiceBookEmpSalStructChange(OEmployeePayroll, OTransferServiceBook.ProcessTransDate.Value, 0, "TRANSFER", null, OTransferServiceBook.Id, null, null);

                int OPayScaleAgrId = db.PayScaleAgreement.Where(e => e.EndDate == null).FirstOrDefault().Id;
                //New Policy structure creation
                EmployeePayroll OEmployeePayrollObj = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                EmployeePolicyStruct OEmpPolicyStruct = db.EmployeePolicyStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpPolicyStruct != null)
                {
                    OEmpPolicyStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmployeePolicyStructCreationTest(OEmployeePayrollObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }


                EmployeeAttendance OEmployeeAttendanceObj = db.EmployeeAttendance.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                EmployeeAttendanceActionPolicyStruct OEmpAttPolicyStruct = db.EmployeeAttendanceActionPolicyStruct.Where(e => e.EmployeeAttendance_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpAttPolicyStruct != null)
                {
                    OEmpAttPolicyStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpAttPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmpolyeeAttendacePolicyStructCreationTest(OEmployeeAttendanceObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }



                EmployeeLTCStruct OEmpLTCStruct = db.EmployeeLTCStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpLTCStruct != null)
                {
                    OEmpLTCStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpLTCStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmployeeLTCStructCreationTest(OEmployeePayrollObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeLeave OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                EmployeeLvStruct OEmpLVStruct = db.EmployeeLvStruct.Where(e => e.EmployeeLeave_Id == OEmployeeLeave.Id && e.EndDate == null).FirstOrDefault();
                if (OEmpLVStruct != null)
                {
                    OEmpLVStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpLVStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Employee_Id == EmpId).SingleOrDefault();
                EmpReportingTimingStruct OEmpReportingTimingStruct = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == OEmployeeAttendance.Id && e.EndDate == null).FirstOrDefault();
                if (OEmpReportingTimingStruct != null)
                {
                    //OEmpReportingTimingStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    //db.Entry(OEmpReportingTimingStruct).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();

                    ServiceBook.EmployeeAttStructCreationTest(OEmployeeAttendance, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id, OEmpReportingTimingStruct.ReportingTimingStruct_Id);
                }

            }
        }
        //Transfer Process
        public static void TransferProcess(EmployeePayroll OEmployeePayroll, CompanyPayroll OCompanyPayroll, string TransTypeList, TransferServiceBook OTransferServiceBook, DateTime mPromotionDate, bool mRegularIncrCalc)
        {
        }
        //other activity process i.e. suspension, confirmation, rejoining, termination,
        public static void OtherReleaseProcess(Int32? OEmployeePayroll, Int32? OOtherServiceBookData_id, DateTime? mReleaseDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OOtherServiceBook = db.OtherServiceBook.Include(e => e.OldPayStruct).Include(e => e.OldPayStruct.Grade).Where(e => e.Id == OOtherServiceBookData_id).SingleOrDefault();


                PayScale NewPayScale6 = db.PayScale.Where(e => e.Id == OOtherServiceBook.NewPayScale_Id).SingleOrDefault();
                OOtherServiceBook.NewPayScale = NewPayScale6;
                PayScaleAgreement NewPayScaleAgreement5 = db.PayScaleAgreement.Where(e => e.Id == OOtherServiceBook.NewPayScaleAgreement_Id).SingleOrDefault();
                OOtherServiceBook.NewPayScaleAgreement = NewPayScaleAgreement5;
                PayScale OldPayScale5 = db.PayScale.Where(e => e.Id == OOtherServiceBook.OldPayScale_Id).SingleOrDefault();
                OOtherServiceBook.OldPayScale = OldPayScale5;
                //PayScaleAgreement OldPayScaleAgreement5 = db.PayScaleAgreement.Where(e => e.Id == OOtherServiceBook.OldPayScaleAgreement_Id).SingleOrDefault();
                //OOtherServiceBook.OldPayScaleAgreement = OldPayScaleAgreement5;
                //PayStruct NewPayStruct5 = db.PayStruct.Where(e => e.Id == OOtherServiceBook.NewPayStruct_Id).SingleOrDefault();
                //OOtherServiceBook.NewPayStruct = NewPayStruct5;
                //PayStruct OldPayStruct5 = db.PayStruct.Where(e => e.Id == OOtherServiceBook.OldPayStruct_Id).SingleOrDefault();
                //OOtherServiceBook.OldPayStruct = OldPayStruct5;
                //FuncStruct OldFuncStruct5 = db.FuncStruct.Where(e => e.Id == OOtherServiceBook.OldFuncStruct_Id).SingleOrDefault();
                //OOtherServiceBook.OldFuncStruct = OldFuncStruct5;
                //FuncStruct NewFuncStruct5 = db.FuncStruct.Where(e => e.Id == OOtherServiceBook.NewFuncStruct_Id).SingleOrDefault();
                //OOtherServiceBook.NewFuncStruct = NewFuncStruct5;
                ////JobStatus NewJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.NewJobStatus_Id).SingleOrDefault();
                //OPromotionServiceBook.NewJobStatus = NewJobStatus3;
                //JobStatus OldJobStatus3 = db.JobStatus.Where(e => e.Id == OPromotionServiceBook.OldJobStatus_Id).SingleOrDefault();
                //OPromotionServiceBook.OldJobStatus = OldJobStatus3;
                OthServiceBookActivity OthServiceBookActivity = db.OthServiceBookActivity.Include(e => e.OtherSerBookActList).Where(e => e.Id == OOtherServiceBook.OthServiceBookActivity_Id).SingleOrDefault();
                OOtherServiceBook.OthServiceBookActivity = OthServiceBookActivity;
                OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList = OthServiceBookActivity.OtherSerBookActList;
                //OthServiceBookPolicy OthServiceBookPolicy = db.OthServiceBookPolicy.Where(e => e.Id == OthServiceBookActivity.OthServiceBookPolicy_Id).SingleOrDefault();
                //OthServiceBookActivity.OthServiceBookPolicy = OthServiceBookPolicy;
                //IncrActivity IncrActivity = db.IncrActivity.Where(e => e.Id == PromoPolicy.IncrActivity_Id).SingleOrDefault();
                //PromoPolicy.IncrActivity = IncrActivity;


                //OOtherServiceBook.ReleaseFlag = true;
                //OOtherServiceBook.ReleaseDate = mReleaseDate;
                //db.Entry(OOtherServiceBook).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();

                var OISSave = db.OtherServiceBook.Find(OOtherServiceBook.Id);
                OISSave.ReleaseFlag = true;
                OISSave.ReleaseDate = mReleaseDate;
                db.OtherServiceBook.Attach(OISSave);
                db.Entry(OISSave).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var seniorityapp = db.Senioritypolicy.Include(e => e.JobStatus).ToList();
                List<int> EmpActingStatus = new List<int>();
                if (seniorityapp != null && seniorityapp.Count() > 0)
                {
                    foreach (var Jobst in seniorityapp)
                    {
                        var Actingjobstat = Jobst.JobStatus.ToList();
                        foreach (var item in Actingjobstat)
                        {
                            EmpActingStatus.Add(item.Id);
                        }
                    }
                    //  var empstatus = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.JobStatus).Include(e => e.Employee.PayStruct.JobStatus.EmpStatus).Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                    var empstatus = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.JobStatus).Include(e => e.Employee.PayStruct.JobStatus.EmpStatus).Where(e => e.Id == OEmployeePayroll && EmpActingStatus.Contains(e.Employee.PayStruct.JobStatus.Id)).SingleOrDefault();
                    //  var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && e.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() == "PERMANENT" && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();
                    var Seniorityobj = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus).Include(e => e.PayStruct.JobStatus.EmpStatus).Where(e => e.ServiceBookDates.ServiceLastDate == null && EmpActingStatus.Contains(e.PayStruct.JobStatus.Id) && (e.ServiceBookDates.SeniorityNo != "0" && e.ServiceBookDates.SeniorityNo != null && e.ServiceBookDates.SeniorityNo != "")).ToList();

                    if (Seniorityobj.Count() > 0)
                    {
                        if (empstatus != null)
                        {
                            SeniorityProcess(null, Convert.ToDateTime(mReleaseDate), OOtherServiceBook);
                        }
                    }
                }
                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017
                //write changes in employee master data
                EmployeeMasterUpdation(OEmployeePayroll, "OTHER", null, null, OOtherServiceBook.Id, null, null);

                if (OOtherServiceBook.NewPayScale != OOtherServiceBook.OldPayScale)
                {
                    EmployeeSalaryStructUpdationOnPayscaleChange(OEmployeePayroll, "OTHER", null, OOtherServiceBook.Id);//to be modify
                    //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
                }
                //DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);//removed by prashant 13042017

                string check = OOtherServiceBook.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper().ToString();
                if (check == "SERVICELASTDAY" || check == "RETIRED" || check == "TERMINATION" || check == "VRS")
                {
                    var OEmpSalStructCurrent = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                        .Where(e => e.EmployeePayroll.Id == OEmployeePayroll && e.EndDate == null).SingleOrDefault();
                    if (OEmpSalStructCurrent != null)
                    {
                        //OEmpSalStructCurrent.EndDate = OOtherServiceBook.ProcessOthDate.Value.AddDays(-1).Date;
                        OEmpSalStructCurrent.EndDate = OOtherServiceBook.ProcessOthDate.Value.Date;
                        db.Entry(OEmpSalStructCurrent).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    if (check == "EXPIRED")
                    {
                        if (OEmpSalStructCurrent != null)
                        {
                            if (OEmpSalStructCurrent.EmpSalStructDetails != null && OEmpSalStructCurrent.EmpSalStructDetails.Count > 0)
                            {
                                db.EmpSalStructDetails.RemoveRange(OEmpSalStructCurrent.EmpSalStructDetails);
                            }
                            db.EmpSalStruct.Remove(OEmpSalStructCurrent);
                        }
                    }

                }
                else if (check == "EXPIRED")
                {
                    var OEmpSalStructCurrent = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Where(e => e.EmployeePayroll.Id == OEmployeePayroll && e.EndDate == null).SingleOrDefault();
                    if (mReleaseDate < OEmpSalStructCurrent.EffectiveDate)
                    {
                        if (OEmpSalStructCurrent.EmpSalStructDetails != null && OEmpSalStructCurrent.EmpSalStructDetails.Count > 0)
                        {
                            db.EmpSalStructDetails.RemoveRange(OEmpSalStructCurrent.EmpSalStructDetails);
                        }
                        db.EmpSalStruct.Remove(OEmpSalStructCurrent);
                        DateTime StrDate = Convert.ToDateTime("01/" + mReleaseDate.Value.ToString("MM/yyyy")).Date;
                        var OPrevEmpStruct = db.EmpSalStruct.Where(e => e.EffectiveDate == StrDate && e.EmployeePayroll.Id == OEmployeePayroll).SingleOrDefault();
                        if (OPrevEmpStruct != null)
                        {
                            OPrevEmpStruct.EndDate = OOtherServiceBook.ProcessOthDate.Value.Date;
                            db.Entry(OPrevEmpStruct).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {

                        if (OEmpSalStructCurrent != null)
                        {
                            //OEmpSalStructCurrent.EndDate = OOtherServiceBook.ProcessOthDate.Value.AddDays(-1).Date;
                            OEmpSalStructCurrent.EndDate = OOtherServiceBook.ProcessOthDate.Value.Date;
                            db.Entry(OEmpSalStructCurrent).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                }
                else
                {
                    ServiceBookEmpSalStructChange(OEmployeePayroll, OOtherServiceBook.ProcessOthDate.Value, OOtherServiceBook.NewBasic, "OTHER", null, null, OOtherServiceBook.Id, null);
                }

                int Comp_Id = Convert.ToInt32(SessionManager.CompanyId);
                var ComPanyLeave_Id = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).SingleOrDefault();
                int EmpId = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == OEmployeePayroll).FirstOrDefault().Employee.Id;
                int OPayScaleAgrId = db.PayScaleAgreement.Where(e => e.EndDate == null).FirstOrDefault().Id;
                //New Policy structure creation
                EmployeePayroll OEmployeePayrollObj = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                EmployeePolicyStruct OEmpPolicyStruct = db.EmployeePolicyStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpPolicyStruct != null)
                {
                    OEmpPolicyStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmployeePolicyStructCreationTest(OEmployeePayrollObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeAttendance OEmployeeAttendanceObj = db.EmployeeAttendance.Where(e => e.Id == OEmployeePayroll).SingleOrDefault();
                EmployeeAttendanceActionPolicyStruct OEmpAttPolicyStruct = db.EmployeeAttendanceActionPolicyStruct.Where(e => e.EmployeeAttendance_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpAttPolicyStruct != null)
                {
                    OEmpAttPolicyStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpAttPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmpolyeeAttendacePolicyStructCreationTest(OEmployeeAttendanceObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }



                EmployeeLTCStruct OEmpLTCStruct = db.EmployeeLTCStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll && e.EndDate == null).FirstOrDefault();
                if (OEmpLTCStruct != null)
                {
                    OEmpLTCStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpLTCStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    ServiceBook.EmployeeLTCStructCreationTest(OEmployeePayrollObj, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeLeave OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                EmployeeLvStruct OEmpLVStruct = db.EmployeeLvStruct.Where(e => e.EmployeeLeave_Id == OEmployeeLeave.Id && e.EndDate == null).FirstOrDefault();
                if (OEmpLVStruct != null)
                {
                    OEmpLVStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    db.Entry(OEmpLVStruct).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    LeaveStructureProcess.EmployeeLeaveStructCreationTest(OEmployeeLeave, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id);
                }

                EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Employee_Id == EmpId).SingleOrDefault();
                EmpReportingTimingStruct OEmpReportingTimingStruct = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == OEmployeeAttendance.Id && e.EndDate == null).FirstOrDefault();
                if (OEmpReportingTimingStruct != null)
                {
                    //OEmpReportingTimingStruct.EndDate = mReleaseDate.Value.AddDays(-1);
                    //db.Entry(OEmpReportingTimingStruct).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();

                    ServiceBook.EmployeeAttStructCreationTest(OEmployeeAttendance, EmpId, OPayScaleAgrId, Convert.ToDateTime(mReleaseDate), Comp_Id, OEmpReportingTimingStruct.ReportingTimingStruct_Id);
                }
                //db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashant 13042017
            }
        }
        //Transfer Process
        public static void OtherProcess(EmployeePayroll OEmployeePayroll, CompanyPayroll OCompanyPayroll, string TransTypeList, OtherServiceBook OOtherServiceBook, DateTime mPromotionDate, bool mRegularIncrCalc)
        {
        }


        /// <summary>
        /// polcystructurecreation
        /// </summary>
        /// 
        public class PolicyFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public ICollection<IncrActivity> IncrActivity { get; set; }
            public ICollection<PromoActivity> PromoActivity { get; set; }
            public ICollection<TransActivity> TransActivity { get; set; }
            public ICollection<OthServiceBookActivity> OthServiceBookActivity { get; set; }
            public ICollection<ExtnRednActivity> ExtnRednActivity { get; set; }
            public ICollection<OfficiatingParameter> OfficiatingParameter { get; set; }
            public string Name { get; set; }
        }

        #region AttendanceActionPolicyFormulaT
        public class AttendanceActionPolicyFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public ICollection<AttendanceAbsentPolicy> AttAbsentPolicy { get; set; }
            public ICollection<AttendanceLeavePriority> AttLeavePriority { get; set; }

            public string Name { get; set; }
        }


        public static List<AttendanceActionPolicyFormulaT> PolicyAttendanceActionPolicyFinderTest(EmployeeAttendanceActionPolicyStruct OEmpPolicystruct, List<AttendanceActionPolicyFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeePolicyStruct = OEmpPolicystruct;
            List<AttendanceActionPolicyFormulaT> OPolicyFormula = new List<AttendanceActionPolicyFormulaT>();
            List<AttendanceActionPolicyFormulaT> OPolicyFormulaGeo = new List<AttendanceActionPolicyFormulaT>();
            List<AttendanceActionPolicyFormulaT> OPolicyFormulaPay = new List<AttendanceActionPolicyFormulaT>();
            List<AttendanceActionPolicyFormulaT> OPolicyFormulaFunc = new List<AttendanceActionPolicyFormulaT>();
            List<AttendanceActionPolicyFormulaT> OPolicyFormulaGeopay = new List<AttendanceActionPolicyFormulaT>();
            List<AttendanceActionPolicyFormulaT> OPolicyFormulaGeofun = new List<AttendanceActionPolicyFormulaT>();
            List<AttendanceActionPolicyFormulaT> OPolicyFormulaGeopayfun = new List<AttendanceActionPolicyFormulaT>();

            OPolicyFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).ToList();

            OPolicyFormula.AddRange(OPolicyFormulaGeo);


            if (OPolicyFormula.Count() == 0)
            {
                OPolicyFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OPolicyFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }

            if (OPolicyFormulaPay.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaPay);
            }
            if (OPolicyFormula.Count() == 0)
            {
                OPolicyFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OPolicyFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }

            if (OPolicyFormulaFunc.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaFunc);
            }

            OPolicyFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.FuncStruct == null).ToList();

            if (OPolicyFormulaGeopay.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaGeopay);
            }

            OPolicyFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.PayStruct == null).ToList();

            if (OPolicyFormulaGeofun.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaGeofun);
            }

            OPolicyFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id).ToList();

            if (OPolicyFormulaGeopayfun.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaGeopayfun);
            }


            return OPolicyFormula;
        }//return salheadformula




        #endregion AttendanceActionPolicyFormulaT

        public static PolicyFormulaT PolicyFinderNew(EmployeePolicyStruct OEmpPolicystruct, List<PolicyFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeePolicyStruct = OEmpPolicystruct;
            PolicyFormulaT OPolicyFormula = null;
            PolicyFormulaT OPolicyFormulaGeo = null;
            PolicyFormulaT OPolicyFormulaPay = null;
            PolicyFormulaT OPolicyFormulaFunc = null;
            PolicyFormulaT OPolicyFormulaGeopay = null;
            PolicyFormulaT OPolicyFormulaGeofun = null;
            PolicyFormulaT OPolicyFormulaGeopayfun = null;

            OPolicyFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).FirstOrDefault();

            OPolicyFormula = OPolicyFormulaGeo;


            if (OPolicyFormula == null)
            {
                OPolicyFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OPolicyFormulaPay = OPayScaleAssignment
              .Where(r => r.Name == OPolicyFormula.Name && r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OPolicyFormulaPay != null)
            {
                OPolicyFormula = OPolicyFormulaPay;
            }
            if (OPolicyFormula == null)
            {
                OPolicyFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OPolicyFormulaFunc = OPayScaleAssignment
              .Where(r => r.Name == OPolicyFormula.Name && r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OPolicyFormulaFunc != null)
            {
                OPolicyFormula = OPolicyFormulaFunc;
            }

            OPolicyFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.FuncStruct == null).FirstOrDefault();

            if (OPolicyFormulaGeopay != null)
            {
                OPolicyFormula = OPolicyFormulaGeopay;
            }

            OPolicyFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.PayStruct == null).FirstOrDefault();

            if (OPolicyFormulaGeofun != null)
            {
                OPolicyFormula = OPolicyFormulaGeofun;
            }

            OPolicyFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id).FirstOrDefault();

            if (OPolicyFormulaGeopayfun != null)
            {
                OPolicyFormula = OPolicyFormulaGeopayfun;
            }


            return OPolicyFormula;
        }//return salheadformula
        public static List<SeprationFormulaT> SeparationFinderNewTest(EmployeeSeperationStruct OEmpSeperationstruct, List<SeprationFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeSeprationStruct = OEmpSeperationstruct;
            List<SeprationFormulaT> OSeprationFormula = new List<SeprationFormulaT>();
            List<SeprationFormulaT> OSeprationFormulaGeo = new List<SeprationFormulaT>();
            List<SeprationFormulaT> OSeprationFormulaPay = new List<SeprationFormulaT>();
            List<SeprationFormulaT> OSeprationFormulaFunc = new List<SeprationFormulaT>();
            List<SeprationFormulaT> OSeprationFormulaGeopay = new List<SeprationFormulaT>();
            List<SeprationFormulaT> OSeprationFormulaGeofun = new List<SeprationFormulaT>();
            List<SeprationFormulaT> OSeprationFormulaGeopayfun = new List<SeprationFormulaT>();

            OSeprationFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeeSeprationStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSeprationStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).ToList();

            OSeprationFormula.AddRange(OSeprationFormulaGeo);


            if (OSeprationFormula.Count() == 0)
            {
                OSeprationFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeSeprationStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSeprationStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OSeprationFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeSeprationStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSeprationStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }

            if (OSeprationFormulaPay.Count() > 0)
            {
                OSeprationFormula.AddRange(OSeprationFormulaPay);
            }
            if (OSeprationFormula.Count() == 0)
            {
                OSeprationFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSeprationStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSeprationStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OSeprationFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSeprationStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSeprationStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }

            if (OSeprationFormulaFunc.Count() > 0)
            {
                OSeprationFormula.AddRange(OSeprationFormulaFunc);
            }

            OSeprationFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeeSeprationStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSeprationStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeSeprationStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSeprationStruct.GeoStruct.Id && r.FuncStruct == null).ToList();

            if (OSeprationFormulaGeopay.Count() > 0)
            {
                OSeprationFormula.AddRange(OSeprationFormulaGeopay);
            }

            OSeprationFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSeprationStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSeprationStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSeprationStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSeprationStruct.GeoStruct.Id && r.PayStruct == null).ToList();

            if (OSeprationFormulaGeofun.Count() > 0)
            {
                OSeprationFormula.AddRange(OSeprationFormulaGeofun);
            }

            OSeprationFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeSeprationStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeSeprationStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeSeprationStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeSeprationStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeSeprationStruct.PayStruct != null && r.PayStruct.Id == OEmployeeSeprationStruct.PayStruct.Id).ToList();

            if (OSeprationFormulaGeopayfun.Count() > 0)
            {
                OSeprationFormula.AddRange(OSeprationFormulaGeopayfun);
            }


            return OSeprationFormula;
        }//return salheadformula

        public static List<PolicyFormulaT> PolicyFinderNewTest(EmployeePolicyStruct OEmpPolicystruct, List<PolicyFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeePolicyStruct = OEmpPolicystruct;
            List<PolicyFormulaT> OPolicyFormula = new List<PolicyFormulaT>();
            List<PolicyFormulaT> OPolicyFormulaGeo = new List<PolicyFormulaT>();
            List<PolicyFormulaT> OPolicyFormulaPay = new List<PolicyFormulaT>();
            List<PolicyFormulaT> OPolicyFormulaFunc = new List<PolicyFormulaT>();
            List<PolicyFormulaT> OPolicyFormulaGeopay = new List<PolicyFormulaT>();
            List<PolicyFormulaT> OPolicyFormulaGeofun = new List<PolicyFormulaT>();
            List<PolicyFormulaT> OPolicyFormulaGeopayfun = new List<PolicyFormulaT>();

            OPolicyFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).ToList();

            OPolicyFormula.AddRange(OPolicyFormulaGeo);


            if (OPolicyFormula.Count() == 0)
            {
                OPolicyFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OPolicyFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }

            if (OPolicyFormulaPay.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaPay);
            }
            if (OPolicyFormula.Count() == 0)
            {
                OPolicyFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OPolicyFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }

            if (OPolicyFormulaFunc.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaFunc);
            }

            OPolicyFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.FuncStruct == null).ToList();

            if (OPolicyFormulaGeopay.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaGeopay);
            }

            OPolicyFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.PayStruct == null).ToList();

            if (OPolicyFormulaGeofun.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaGeofun);
            }

            OPolicyFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeePolicyStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeePolicyStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeePolicyStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeePolicyStruct.GeoStruct.Id && r.PayStruct != null && OEmployeePolicyStruct.PayStruct != null && r.PayStruct.Id == OEmployeePolicyStruct.PayStruct.Id).ToList();

            if (OPolicyFormulaGeopayfun.Count() > 0)
            {
                OPolicyFormula.AddRange(OPolicyFormulaGeopayfun);
            }


            return OPolicyFormula;
        }//return salheadformula


        #region EmpolyeeAttendacePolicyStruct
        public static void EmpolyeeAttendacePolicyStructCreationTest(EmployeeAttendance OEmployeePayroll, int OEmployee_Id,
          int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyLeave_Id)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var OPayScaleAssignment = db.AttendanceActionPolicyAssignment.Include(e => e.AttendanceActionPolicyFormula)
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreement_Id && p.p.CompanyAttendance.Id == ComPanyLeave_Id)
                       .Select(m => new
                       {
                           PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                           PolicyFormula = m.p.AttendanceActionPolicyFormula.Select(t => new AttendanceActionPolicyFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               AttLeavePriority = t.AttendanceLeavePriority,
                               AttAbsentPolicy = t.AttendanceAbsentPolicy,
                               //PromoActivity = t.PromoActivity,
                               //TransActivity = t.TransActivity,
                               //ExtnRednActivity = t.ExtnRednActivity
                           }),
                           PolicyName = m.p.PolicyName
                       }).ToList();


                var OEmployee = db.Employee
                    .Where(r => r.Id == OEmployee_Id).Select(a => new
                    {
                        Id = a.Id,
                        GeoStruct = a.GeoStruct,
                        FuncStruct = a.FuncStruct,
                        PayStruct = a.PayStruct
                    }).FirstOrDefault();


                if (OPayScaleAssignment.Count() == 0)
                {
                    return;
                }

                EmployeeAttendanceActionPolicyStruct EmployeePolicyStruct = new EmployeeAttendanceActionPolicyStruct();
                {
                    EmployeePolicyStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        EmployeePolicyStruct.GeoStruct = OEmployee.GeoStruct;
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        EmployeePolicyStruct.FuncStruct = OEmployee.FuncStruct;
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        EmployeePolicyStruct.PayStruct = OEmployee.PayStruct;
                    };
                    EmployeePolicyStruct.DBTrack = dbt;
                    //db.EmpSalStruct.Add(OEmpSalStruct);
                    //db.SaveChanges();
                    List<EmployeeAttendanceActionPolicyStructDetails> OEmpPolicyStructDetails = new List<EmployeeAttendanceActionPolicyStructDetails>();
                    EmployeeAttendanceActionPolicyStructDetails OEmpPolicyStructDetailsObj = null;

                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyBankPolicy != null).Count() > 0)
                        //{

                        //  OEmpSalStructDetailsObj.Amount = 0;


                        if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.PolicyFormula.Count() > 0)//newly added by prashant on 24042017
                        {

                            List<AttendanceActionPolicyFormulaT> Policyformula = new List<AttendanceActionPolicyFormulaT>();

                            Policyformula = PolicyAttendanceActionPolicyFinderTest(EmployeePolicyStruct, OPayScaleAssignmentData.PolicyFormula.ToList(), OPayScaleAssignmentData.PolicyName.Id);

                            //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                            foreach (var item in Policyformula)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeeAttendanceActionPolicyStructDetails();
                                {
                                    if (OPayScaleAssignmentData != null)
                                    {
                                        OEmpPolicyStructDetailsObj.AttendanceActionPolicyAssignment = OPayScaleAssignmentData.PolicyAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (OPayScaleAssignmentData.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.AttendanceActionPolicyFormula = Policyformula == null ? null : db.AttendanceActionPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }
                        else
                        {
                            OEmpPolicyStructDetailsObj.AttendanceActionPolicyFormula = null;
                        }

                    }
                    EmployeePolicyStruct.EmployeeAttendanceActionPolicyStructDetails = OEmpPolicyStructDetails;
                    db.EmployeeAttendanceActionPolicyStruct.Add(EmployeePolicyStruct);
                    db.SaveChanges();

                }
                try
                {

                    List<EmployeeAttendanceActionPolicyStruct> OTemp2 = new List<EmployeeAttendanceActionPolicyStruct>();
                    OTemp2.Add(db.EmployeeAttendanceActionPolicyStruct.Where(e => e.Id == EmployeePolicyStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeeAttendance OTEP = new EmployeeAttendance()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmployeeAttendanceActionPolicyStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeeAttendance.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeeAttendance.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmployeeAttendanceActionPolicyStruct = OTemp2;
                        db.EmployeeAttendance.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeePolicyStructCreation - LeaveStructureProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }
        #endregion EmpolyeeAttendacePolicyStruct






        public static void EmployeePolicyStructCreationTest(EmployeePayroll OEmployeePayroll, int OEmployee_Id,
           int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyPayroll_Id)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var OPayScaleAssignment = db.PolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreement_Id && p.p.CompanyPayroll.Id == ComPanyPayroll_Id)
                       .Select(m => new
                       {
                           PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                           PolicyFormula = m.p.PolicyFormula.Select(t => new PolicyFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               OthServiceBookActivity = t.OthServiceBookActivity,
                               IncrActivity = t.IncrActivity,
                               PromoActivity = t.PromoActivity,
                               TransActivity = t.TransActivity,
                               ExtnRednActivity = t.ExtnRednActivity
                           }),
                           PolicyName = m.p.PolicyName
                       }).ToList();


                var OEmployee = db.Employee
                    .Where(r => r.Id == OEmployee_Id).Select(a => new
                    {
                        Id = a.Id,
                        GeoStruct = a.GeoStruct,
                        FuncStruct = a.FuncStruct,
                        PayStruct = a.PayStruct
                    }).FirstOrDefault();


                if (OPayScaleAssignment.Count() == 0)
                {
                    return;
                }

                EmployeePolicyStruct EmployeePolicyStruct = new EmployeePolicyStruct();
                {
                    EmployeePolicyStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        EmployeePolicyStruct.GeoStruct = OEmployee.GeoStruct; //db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        EmployeePolicyStruct.FuncStruct = OEmployee.FuncStruct; // db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        EmployeePolicyStruct.PayStruct = OEmployee.PayStruct; // db.PayStruct.Where(e => e.Id == OEmployee.PayStruct.Id).SingleOrDefault();
                    };
                    EmployeePolicyStruct.DBTrack = dbt;
                    //db.EmpSalStruct.Add(OEmpSalStruct);
                    //db.SaveChanges();
                    List<EmployeePolicyStructDetails> OEmpPolicyStructDetails = new List<EmployeePolicyStructDetails>();
                    EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = null;

                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyBankPolicy != null).Count() > 0)
                        //{

                        //  OEmpSalStructDetailsObj.Amount = 0;


                        if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.PolicyFormula.Count() > 0)//newly added by prashant on 24042017
                        {

                            List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                            Policyformula = PolicyFinderNewTest(EmployeePolicyStruct, OPayScaleAssignmentData.PolicyFormula.ToList(), OPayScaleAssignmentData.PolicyName.Id);

                            //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                            foreach (var item in Policyformula)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (OPayScaleAssignmentData != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = OPayScaleAssignmentData.PolicyAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (OPayScaleAssignmentData.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }
                        else
                        {
                            OEmpPolicyStructDetailsObj.PolicyFormula = null;
                        }

                    }
                    EmployeePolicyStruct.EmployeePolicyStructDetails = OEmpPolicyStructDetails;
                    db.EmployeePolicyStruct.Add(EmployeePolicyStruct);
                    db.SaveChanges();

                }
                try
                {

                    List<EmployeePolicyStruct> OTemp2 = new List<EmployeePolicyStruct>();
                    OTemp2.Add(db.EmployeePolicyStruct.Where(e => e.Id == EmployeePolicyStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeePayroll OTEP = new EmployeePayroll()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmployeePolicyStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeePayroll.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmployeePolicyStruct = OTemp2;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeePolicyStructCreation - LeaveStructureProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }


        public static void EmployeePolicyStructureCreationWithUpdationTest(int OEmpPolicyStructId, int OEmployeeId,
        int OPayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int CompanyPayroll_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int CompId = Convert.ToInt32(SessionManager.CompanyId);


                var OPayScaleAssignment = db.PolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyPayroll.Id == CompanyPayroll_Id)
                       .Select(m => new
                       {
                           PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                           Id = m.p.Id,
                           PolicyFormula = m.p.PolicyFormula.Select(t => new PolicyFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               OthServiceBookActivity = t.OthServiceBookActivity,
                               IncrActivity = t.IncrActivity,
                               PromoActivity = t.PromoActivity,
                               TransActivity = t.TransActivity,
                               ExtnRednActivity = t.ExtnRednActivity,
                               OfficiatingParameter = t.OfficiatingParameter
                           }),
                           PolicyName = m.p.PolicyName
                           // other assignments
                       }).ToList();


                EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)
                               .Include(e => e.EmployeePolicyStruct)
                               .Include(e => e.Employee.EmpOffInfo)
                               .Include(e => e.Employee.EmpOffInfo.PayScale).AsNoTracking().OrderBy(e => e.Id)
                               .Where(e => e.Id == OEmployeePayroll_Id).SingleOrDefault();



                Employee OEmployee = db.Employee
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeePayroll.Employee.Id)
                                                    .SingleOrDefault();

                List<EmployeePolicyStructDetails> OEmpPolicyStructDetails = new List<EmployeePolicyStructDetails>();
                int Count = 0;
                EmployeePolicyStruct OEmpPolicyStruct = db.EmployeePolicyStruct.Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 
                List<EmployeePolicyStructDetails> EmployeePolicyStructDetails = db.EmployeePolicyStructDetails.Where(e => e.EmployeePolicyStruct_Id == OEmpPolicyStruct.Id).ToList();
                OEmpPolicyStruct.EmployeePolicyStructDetails = EmployeePolicyStructDetails;
                foreach (var EmployeePolicyStructDetailsitem in EmployeePolicyStructDetails)
                {
                    PolicyFormula PolicyFormula = db.PolicyFormula.Where(e => e.Id == EmployeePolicyStructDetailsitem.PolicyFormula_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.PolicyFormula = PolicyFormula;
                    EmployeePolicyStructDetailsitem.PolicyName = EmployeePolicyStructDetailsitem.PolicyName;
                    PolicyAssignment PolicyAssignment = db.PolicyAssignment.Where(e => e.Id == EmployeePolicyStructDetailsitem.PolicyAssignment_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.PolicyAssignment = PolicyAssignment;
                }
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpPolicyStruct.FuncStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.FuncStruct = FuncStruct;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpPolicyStruct.GeoStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpPolicyStruct.PayStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.PayStruct = PayStruct;



                OEmpPolicyStruct.EmployeePolicyStructDetails.Where(x => x.PolicyAssignment == null).ToList().ForEach(x =>
                {
                    OEmpPolicyStruct.EmployeePolicyStructDetails.Remove(x);
                });
                db.SaveChanges();
                EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = null;
                foreach (var a in OPayScaleAssignment)
                {
                    Boolean datainstructdetails = false;
                    //foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails)
                    //{
                    //    if (a.PolicyName.Id == b.PolicyName.Id)
                    //    {
                    //        Count = 0;
                    //        break;
                    //    }
                    //    Count = 1;
                    //}
                    //if (Count == 1)
                    //{
                    //if (a.PolicyHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                    //{
                    //    continue;
                    //}
                    //if (a.PolicyFormula.Where(r => r.OthServiceBookActivity != null).Count() > 0)
                    //{

                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.IncrActivity.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                        Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.IncrActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.PolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();

                                        //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }



                    }
                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.PromoActivity.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                        Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.PromoActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.PolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }

                    }
                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.OthServiceBookActivity.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                        Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.OthServiceBookActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.PolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }



                    }

                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.TransActivity.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                        Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.TransActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.PolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }



                    }

                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.ExtnRednActivity.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                        Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.ExtnRednActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.PolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }



                    }

                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.OfficiatingParameter.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                        Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.OfficiatingParameter.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.PolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                                            .Include(e => e.PolicyFormula)
                                        .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }



                    }
                }
                db.EmployeePolicyStructDetails.AddRange(OEmpPolicyStructDetails);
                OEmpPolicyStructDetails.AddRange(OEmpPolicyStruct.EmployeePolicyStructDetails);
                OEmpPolicyStruct.EmployeePolicyStructDetails = OEmpPolicyStructDetails;

                db.EmployeePolicyStruct.Attach(OEmpPolicyStruct);
                db.Entry(OEmpPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //   EmployeeSalaryStructUpdateNew(OEmpPolicyStruct, OEmployeePayroll, mEffectiveDate);
                OEmployeePayroll.EmployeePolicyStruct.Add(OEmpPolicyStruct);

                db.SaveChanges();

            }

        }



        #region EmployeeAttendanceActionPolicyStructUpdation

        public static void EmployeeAttendancePolicyStructureCreationWithUpdationTest(int OEmpPolicyStructId, int OEmployeeId,
       int OPayScaleAgreementId, DateTime mEffectiveDate, int OEmployeeAttendance_Id, int CompanyAttendance_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int CompId = Convert.ToInt32(SessionManager.CompanyId);


                var OPayScaleAssignment = db.AttendanceActionPolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyAttendance.Id == CompanyAttendance_Id)
                       .Select(m => new
                       {
                           PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                           Id = m.p.Id,
                           PolicyFormula = m.p.AttendanceActionPolicyFormula.Select(t => new AttendanceActionPolicyFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               AttAbsentPolicy = t.AttendanceAbsentPolicy,
                               AttLeavePriority = t.AttendanceLeavePriority,
                           }),
                           PolicyName = m.p.PolicyName
                           // other assignments
                       }).ToList();


                EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance
                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)
                               .Include(e => e.EmployeeAttendanceActionPolicyStruct)
                               .Include(e => e.Employee.EmpOffInfo)
                               .Include(e => e.Employee.EmpOffInfo.PayScale).AsNoTracking().OrderBy(e => e.Id)
                               .Where(e => e.Id == OEmployeeAttendance_Id).SingleOrDefault();



                Employee OEmployee = db.Employee
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeeAttendance.Employee.Id)
                                                    .SingleOrDefault();

                List<EmployeeAttendanceActionPolicyStructDetails> OEmpPolicyStructDetails = new List<EmployeeAttendanceActionPolicyStructDetails>();
                int Count = 0;
                EmployeeAttendanceActionPolicyStruct OEmpPolicyStruct = db.EmployeeAttendanceActionPolicyStruct.Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 
                List<EmployeeAttendanceActionPolicyStructDetails> EmployeePolicyStructDetails = db.EmployeeAttendanceActionPolicyStructDetails.Where(e => e.EmployeeAttendanceActionPolicyStruct_Id == OEmpPolicyStruct.Id).ToList();
                OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails = EmployeePolicyStructDetails;
                foreach (var EmployeePolicyStructDetailsitem in EmployeePolicyStructDetails)
                {
                    AttendanceActionPolicyFormula PolicyFormula = db.AttendanceActionPolicyFormula.Where(e => e.Id == EmployeePolicyStructDetailsitem.AttendanceActionPolicyFormula_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.AttendanceActionPolicyFormula = PolicyFormula;
                    EmployeePolicyStructDetailsitem.PolicyName = EmployeePolicyStructDetailsitem.PolicyName;
                    AttendanceActionPolicyAssignment PolicyAssignment = db.AttendanceActionPolicyAssignment.Where(e => e.Id == EmployeePolicyStructDetailsitem.AttendanceActionPolicyAssignment_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.AttendanceActionPolicyAssignment = PolicyAssignment;
                }
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpPolicyStruct.FuncStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.FuncStruct = FuncStruct;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpPolicyStruct.GeoStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpPolicyStruct.PayStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.PayStruct = PayStruct;



                OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.AttendanceActionPolicyAssignment == null).ToList().ForEach(x =>
                {
                    OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.Remove(x);
                });
                db.SaveChanges();
                EmployeeAttendanceActionPolicyStructDetails OEmpPolicyStructDetailsObj = null;
                foreach (var a in OPayScaleAssignment)
                {
                    Boolean datainstructdetails = false;

                    //if (a.PolicyFormula.Where(r => r.OthServiceBookActivity != null).Count() > 0)
                    //{

                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.AttAbsentPolicy.Count() > 0).Count() > 0)
                    {
                        List<AttendanceActionPolicyFormulaT> Policyformula = new List<AttendanceActionPolicyFormulaT>();

                        Policyformula = PolicyAttendanceActionPolicyFinderTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.AttAbsentPolicy.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.AttendanceActionPolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeeAttendanceActionPolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.AttendanceActionPolicyAssignment = db.AttendanceActionPolicyAssignment
                                            .Include(e => e.AttendanceActionPolicyFormula)
                                        .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.AttendanceActionPolicyFormula = Policyformula == null ? null : db.AttendanceActionPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeeAttendanceActionPolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.AttendanceActionPolicyAssignment = db.AttendanceActionPolicyAssignment
                                            .Include(e => e.AttendanceActionPolicyFormula)
                                        .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();

                                        //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.AttendanceActionPolicyFormula = Policyformula == null ? null : db.AttendanceActionPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }



                    }
                    if (a.PolicyName != null && a.PolicyFormula.Where(r => r.AttLeavePriority.Count() > 0).Count() > 0)
                    {
                        List<AttendanceActionPolicyFormulaT> Policyformula = new List<AttendanceActionPolicyFormulaT>();

                        Policyformula = PolicyAttendanceActionPolicyFinderTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.AttLeavePriority.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Policyformula)
                        {
                            foreach (var b in OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.AttendanceActionPolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeeAttendanceActionPolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.AttendanceActionPolicyAssignment = db.AttendanceActionPolicyAssignment
                                            .Include(e => e.AttendanceActionPolicyFormula)
                                        .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.AttendanceActionPolicyFormula = Policyformula == null ? null : db.AttendanceActionPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpPolicyStructDetailsObj = new EmployeeAttendanceActionPolicyStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpPolicyStructDetailsObj.AttendanceActionPolicyAssignment = db.AttendanceActionPolicyAssignment
                                            .Include(e => e.AttendanceActionPolicyFormula)
                                        .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.PayStruct))
                                          .Include(e => e.AttendanceActionPolicyFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpPolicyStructDetailsObj.AttendanceActionPolicyFormula = Policyformula == null ? null : db.AttendanceActionPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                                    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                                };
                            }
                        }

                    }




                    //if (a.PolicyName != null && a.PolicyFormula.Where(r => r.OfficiatingParameter.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    //{
                    //    List<PolicyFormulaT> Policyformula = new List<PolicyFormulaT>();

                    //    Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.OfficiatingParameter.Count() > 0).ToList(), a.PolicyName.Id);
                    //    foreach (var item in Policyformula)
                    //    {
                    //        foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).ToList())
                    //        {
                    //            if (item.Id == b.PolicyFormula.Id)
                    //            {
                    //                datainstructdetails = true;
                    //                Count = 0;
                    //                break;
                    //            }
                    //            Count = 1;
                    //        }
                    //        if (Count == 1)
                    //        {
                    //            OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                    //            {
                    //                if (a != null)
                    //                {
                    //                    OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                    //                        .Include(e => e.PolicyFormula)
                    //                    .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                    //                   .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                    //                      .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                    //                       .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                    //                }
                    //                if (a.PolicyName != null)
                    //                {
                    //                    OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                    //                }
                    //                OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                    //                OEmpPolicyStructDetailsObj.DBTrack = dbt;
                    //                OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                    //            };
                    //        }
                    //        if (Count == 0 && Policyformula.Count() > 0 && datainstructdetails == false)
                    //        {
                    //            OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                    //            {
                    //                if (a != null)
                    //                {
                    //                    OEmpPolicyStructDetailsObj.PolicyAssignment = db.PolicyAssignment
                    //                        .Include(e => e.PolicyFormula)
                    //                    .Include(e => e.PolicyFormula.Select(z => z.GeoStruct))
                    //                   .Include(e => e.PolicyFormula.Select(z => z.PayStruct))
                    //                      .Include(e => e.PolicyFormula.Select(z => z.FuncStruct))
                    //                       .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                    //                }
                    //                if (a.PolicyName != null)
                    //                {
                    //                    OEmpPolicyStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                    //                }
                    //                OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                    //                OEmpPolicyStructDetailsObj.DBTrack = dbt;
                    //                OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                    //            };
                    //        }
                    //    }



                    //}
                }
                db.EmployeeAttendanceActionPolicyStructDetails.AddRange(OEmpPolicyStructDetails);
                OEmpPolicyStructDetails.AddRange(OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails);
                OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails = OEmpPolicyStructDetails;

                db.EmployeeAttendanceActionPolicyStruct.Attach(OEmpPolicyStruct);
                db.Entry(OEmpPolicyStruct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //   EmployeeSalaryStructUpdateNew(OEmpPolicyStruct, OEmployeePayroll, mEffectiveDate);

                OEmployeeAttendance.EmployeeAttendanceActionPolicyStruct.Add(OEmpPolicyStruct);

                db.SaveChanges();

            }

        }
        #endregion EmployeeAttendanceActionPolicyStructUpdation



        #region EMP_PolicyStructureCreationWithUpdateFormulaTest

        public static void EMP_PolicyStructureCreationWithUpdateFormulaTest(int OEmpPolicyStructId, int OEmployee_Id,
         int PayScaleAgreementId, DateTime mEffectiveDate, int OEmployeeAttendance_Id, int CompanyAttendance_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int Count = 0;

                EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Id == OEmployeeAttendance_Id).SingleOrDefault();
                List<EmployeeAttendanceActionPolicyStruct> EmployeePolicyStruct = db.EmployeeAttendanceActionPolicyStruct.Where(e => e.EmployeeAttendance_Id == OEmployeeAttendance.Id).ToList();
                OEmployeeAttendance.EmployeeAttendanceActionPolicyStruct = EmployeePolicyStruct;

                Employee OEmployee = db.Employee.Where(e => e.Id == OEmployeeAttendance.Employee_Id).SingleOrDefault();
                OEmployeeAttendance.Employee = OEmployee;
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == OEmployee.EmpOffInfo_Id).SingleOrDefault();
                OEmployee.EmpOffInfo = EmpOffInfo;


                OEmployeeAttendance.Employee.EmpOffInfo = EmpOffInfo;
                PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                EmpOffInfo.PayScale = PayScale;



                var OPayScaleAssignment = db.AttendanceActionPolicyAssignment
                       .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                      .Where(p => p.p.PayScaleAgreement.Id == PayScaleAgreementId && p.p.CompanyAttendance.Id == CompanyAttendance_Id)
                      .Select(m => new
                      {
                          PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                          Id = m.p.Id,
                          PolicyFormula = m.p.AttendanceActionPolicyFormula.Select(t => new AttendanceActionPolicyFormulaT
                          {
                              GeoStruct = t.GeoStruct,
                              PayStruct = t.PayStruct,
                              FuncStruct = t.FuncStruct,
                              Name = t.Name,
                              Id = t.Id,
                              AttAbsentPolicy = t.AttendanceAbsentPolicy,
                              AttLeavePriority = t.AttendanceLeavePriority,
                          }),
                          PolicyName = m.p.PolicyName
                          // other assignments
                      }).ToList();



                List<EmployeeAttendanceActionPolicyStructDetails> OEmpPolicyStructDetails = new List<EmployeeAttendanceActionPolicyStructDetails>();


                EmployeeAttendanceActionPolicyStruct OEmpPolicyStruct = db.EmployeeAttendanceActionPolicyStruct.Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();
                List<EmployeeAttendanceActionPolicyStructDetails> EmployeePolicyStructDetails = db.EmployeeAttendanceActionPolicyStructDetails.Where(e => e.EmployeeAttendanceActionPolicyStruct_Id == OEmpPolicyStruct.Id).ToList();
                OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails = EmployeePolicyStructDetails;
                foreach (var EmployeePolicyStructDetailsitem in EmployeePolicyStructDetails)
                {
                    AttendanceActionPolicyFormula PolicyFormula = db.AttendanceActionPolicyFormula.Where(e => e.Id == EmployeePolicyStructDetailsitem.AttendanceActionPolicyFormula_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.AttendanceActionPolicyFormula = PolicyFormula;
                    EmployeePolicyStructDetailsitem.PolicyName = EmployeePolicyStructDetailsitem.PolicyName;
                    AttendanceActionPolicyAssignment PolicyAssignment = db.AttendanceActionPolicyAssignment.Include(e => e.PayScaleAgreement).Where(e => e.Id == EmployeePolicyStructDetailsitem.AttendanceActionPolicyAssignment_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.AttendanceActionPolicyAssignment = PolicyAssignment;
                    EmployeePolicyStructDetailsitem.AttendanceActionPolicyAssignment.PayScaleAgreement = PolicyAssignment.PayScaleAgreement;
                }
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpPolicyStruct.FuncStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.FuncStruct = FuncStruct;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpPolicyStruct.GeoStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpPolicyStruct.PayStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.PayStruct = PayStruct;




                OEmpPolicyStructDetails = OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.ToList();
                foreach (var a in OPayScaleAssignment)
                {

                    //if (a.PolicyName.Id == b.PolicyName.Id)
                    //{
                    //Count = 0;
                    List<int> PolicyFormulaIds = new List<int>();
                    if (a.PolicyFormula.Where(r => r.AttAbsentPolicy.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyAttendanceActionPolicyFinderTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.AttAbsentPolicy != null).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.AttendanceActionPolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeeAttendanceActionPolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }

                    }

                    if (a.PolicyFormula.Where(r => r.AttLeavePriority.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyAttendanceActionPolicyFinderTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.AttLeavePriority.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeeAttendanceActionPolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.AttendanceActionPolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeeAttendanceActionPolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }


                    //if (a.PolicyFormula.Where(r => r.ExtnRednActivity.Count() > 0).Count() > 0)
                    //{
                    //    var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.ExtnRednActivity.Count() > 0).ToList(), a.PolicyName.Id);
                    //    PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                    //    var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                    //    if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                    //    {
                    //        db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                    //        db.SaveChanges();
                    //    }
                    //}


                }


            }

        }


        #endregion EMP_PolicyStructureCreationWithUpdateFormulaTest



        public static void EmployeePolicyStructureCreationWithUpdateFormulaTest(int OEmpPolicyStructId, int OEmployee_Id,
           int PayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int CompanyPayroll_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int Count = 0;

                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll_Id).SingleOrDefault();
                List<EmployeePolicyStruct> EmployeePolicyStruct = db.EmployeePolicyStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).ToList();
                OEmployeePayroll.EmployeePolicyStruct = EmployeePolicyStruct;

                Employee OEmployee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).SingleOrDefault();
                OEmployeePayroll.Employee = OEmployee;
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == OEmployee.EmpOffInfo_Id).SingleOrDefault();
                OEmployee.EmpOffInfo = EmpOffInfo;

                //  EmpOff EmpOffInfo = db.Employee.Where(e =>e.Id  == OEmployeePayroll.Employee_Id).Select(e=>e.EmpOffInfo).SingleOrDefault();
                OEmployeePayroll.Employee.EmpOffInfo = EmpOffInfo;
                PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                EmpOffInfo.PayScale = PayScale;



                var OPayScaleAssignment = db.PolicyAssignment
                       .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                      .Where(p => p.p.PayScaleAgreement.Id == PayScaleAgreementId && p.p.CompanyPayroll.Id == CompanyPayroll_Id)
                      .Select(m => new
                      {
                          PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                          Id = m.p.Id,
                          PolicyFormula = m.p.PolicyFormula.Select(t => new PolicyFormulaT
                          {
                              GeoStruct = t.GeoStruct,
                              PayStruct = t.PayStruct,
                              FuncStruct = t.FuncStruct,
                              Name = t.Name,
                              Id = t.Id,
                              OthServiceBookActivity = t.OthServiceBookActivity,
                              IncrActivity = t.IncrActivity,
                              PromoActivity = t.PromoActivity,
                              TransActivity = t.TransActivity,
                              ExtnRednActivity = t.ExtnRednActivity,
                              OfficiatingParameter = t.OfficiatingParameter
                          }),
                          PolicyName = m.p.PolicyName
                          // other assignments
                      }).ToList();

                //Employee OEmployee = db.Employee
                //    //.Include(e => e.GeoStruct)
                //    //.Include(e => e.FuncStruct)
                //    //.Include(e => e.PayStruct)
                //                                   .Include(e => e.EmpOffInfo)
                //                                    .Where(e => e.Id == OEmployeePayroll.Employee.Id).AsNoTracking().OrderBy(e => e.Id)
                //                                    .SingleOrDefault();

                List<EmployeePolicyStructDetails> OEmpPolicyStructDetails = new List<EmployeePolicyStructDetails>();

                //EmployeePolicyStruct OEmpPolicyStruct = db.EmployeePolicyStruct
                //    .Include(e => e.EmployeePolicyStructDetails)
                //    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyName))
                //    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula))
                //     .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyAssignment))
                //      .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyAssignment.PayScaleAgreement))
                //    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();

                EmployeePolicyStruct OEmpPolicyStruct = db.EmployeePolicyStruct.Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();
                List<EmployeePolicyStructDetails> EmployeePolicyStructDetails = db.EmployeePolicyStructDetails.Where(e => e.EmployeePolicyStruct_Id == OEmpPolicyStruct.Id).ToList();
                OEmpPolicyStruct.EmployeePolicyStructDetails = EmployeePolicyStructDetails;
                foreach (var EmployeePolicyStructDetailsitem in EmployeePolicyStructDetails)
                {
                    PolicyFormula PolicyFormula = db.PolicyFormula.Where(e => e.Id == EmployeePolicyStructDetailsitem.PolicyFormula_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.PolicyFormula = PolicyFormula;
                    EmployeePolicyStructDetailsitem.PolicyName = EmployeePolicyStructDetailsitem.PolicyName;
                    PolicyAssignment PolicyAssignment = db.PolicyAssignment.Include(e => e.PayScaleAgreement).Where(e => e.Id == EmployeePolicyStructDetailsitem.PolicyAssignment_Id).SingleOrDefault();
                    EmployeePolicyStructDetailsitem.PolicyAssignment = PolicyAssignment;
                    EmployeePolicyStructDetailsitem.PolicyAssignment.PayScaleAgreement = PolicyAssignment.PayScaleAgreement;
                }
                FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpPolicyStruct.FuncStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.FuncStruct = FuncStruct;
                GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpPolicyStruct.GeoStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.GeoStruct = GeoStruct;
                PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpPolicyStruct.PayStruct_Id).SingleOrDefault();
                OEmpPolicyStruct.PayStruct = PayStruct;


                //OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 

                OEmpPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.ToList();
                foreach (var a in OPayScaleAssignment)
                {

                    //if (a.PolicyName.Id == b.PolicyName.Id)
                    //{
                    //Count = 0;
                    List<int> PolicyFormulaIds = new List<int>();
                    if (a.PolicyFormula.Where(r => r.IncrActivity.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.IncrActivity != null).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                        //foreach (var item in PolicyFormulaNotInPolicyStructDetails)
                        //{
                        //    foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).OrderBy(e => e.PolicyName.Id).ToList())
                        //    {
                        //        if (item.Id != null && b.PolicyFormula != null)
                        //        {

                        //            if (b.PolicyFormula.Id == item.Id)
                        //            {
                        //                break;
                        //            }
                        //            else
                        //            {
                        //                EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = db.EmployeePolicyStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                        //                OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).FirstOrDefault();
                        //                db.EmployeePolicyStructDetails.Attach(OEmpPolicyStructDetailsObj);
                        //                db.Entry(OEmpPolicyStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                        //                db.SaveChanges();
                        //            }
                        //        }
                        //        if (item.Id != null && b.PolicyFormula == null)
                        //        {
                        //            EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = db.EmployeePolicyStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                        //            OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).FirstOrDefault();
                        //            db.EmployeePolicyStructDetails.Attach(OEmpPolicyStructDetailsObj);
                        //            db.Entry(OEmpPolicyStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                        //            db.SaveChanges();
                        //        }
                        //    }
                        //}
                    }

                    if (a.PolicyFormula.Where(r => r.PromoActivity.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.PromoActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.PolicyFormula.Where(r => r.OthServiceBookActivity.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.OthServiceBookActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.PolicyFormula.Where(r => r.TransActivity.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.TransActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.PolicyFormula.Where(r => r.ExtnRednActivity.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.ExtnRednActivity.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.PolicyFormula.Where(r => r.OfficiatingParameter.Count() > 0).Count() > 0)
                    {
                        var Policyformula = PolicyFinderNewTest(OEmpPolicyStruct, a.PolicyFormula.Where(r => r.OfficiatingParameter.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.PolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeePolicyStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }

                }


            }

        }


        public class SeprationFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public ICollection<ExitProcess_CheckList_Policy> ExitProcess_CheckList_Policy { get; set; }
            public ICollection<ExitProcess_Config_Policy> ExitProcess_Config_Policy { get; set; }
            public ICollection<ExitProcess_Process_Policy> ExitProcess_Process_Policy { get; set; }
            public ICollection<NoticePeriod_Object> NoticePeriod_Object { get; set; }
            public string Name { get; set; }
        }

        public static void EmployeeSeperationStructCreationTest(EmployeeExit OEmployeePayroll, int OEmployee_Id,
          int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyPayroll_Id)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var OPayScaleAssignment = db.SeperationPolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreement_Id && p.p.CompanyExit.Id == ComPanyPayroll_Id)
                       .Select(m => new
                       {
                           SeperationAssignment = m.p, // or m.ppc.pc.ProdId
                           Id = m.p.Id,
                           SeperationFormula = m.p.SeperationFormula.Select(t => new SeprationFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               ExitProcess_CheckList_Policy = t.ExitProcess_CheckList_Policy,
                               ExitProcess_Config_Policy = t.ExitProcess_Config_Policy,
                               ExitProcess_Process_Policy = t.ExitProcess_Process_Policy,
                               NoticePeriod_Object = t.NoticePeriod_Object
                           }),
                           SeparationName = m.p.SeperationMaster
                       }).ToList();


                var OEmployee = db.Employee
                    .Where(r => r.Id == OEmployee_Id).Select(a => new
                    {
                        Id = a.Id,
                        GeoStruct = a.GeoStruct,
                        FuncStruct = a.FuncStruct,
                        PayStruct = a.PayStruct
                    }).FirstOrDefault();


                if (OPayScaleAssignment.Count() == 0)
                {
                    return;
                }

                EmployeeSeperationStruct EmployeeSeperationStruct = new EmployeeSeperationStruct();
                {
                    EmployeeSeperationStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        EmployeeSeperationStruct.GeoStruct = OEmployee.GeoStruct; //db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        EmployeeSeperationStruct.FuncStruct = OEmployee.FuncStruct; // db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        EmployeeSeperationStruct.PayStruct = OEmployee.PayStruct; // db.PayStruct.Where(e => e.Id == OEmployee.PayStruct.Id).SingleOrDefault();
                    };
                    EmployeeSeperationStruct.DBTrack = dbt;
                    //db.EmpSalStruct.Add(OEmpSalStruct);
                    //db.SaveChanges();
                    List<EmployeeSeperationStructDetails> OEmpSeperationStructDetails = new List<EmployeeSeperationStructDetails>();
                    EmployeeSeperationStructDetails OEmpSeperationStructDetailsObj = null;

                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyBankPolicy != null).Count() > 0)
                        //{

                        //  OEmpSalStructDetailsObj.Amount = 0;


                        if (OPayScaleAssignmentData.SeparationName != null && OPayScaleAssignmentData.SeperationFormula.Count() > 0)//newly added by prashant on 24042017
                        {

                            List<SeprationFormulaT> Seprationformula = new List<SeprationFormulaT>();

                            Seprationformula = SeparationFinderNewTest(EmployeeSeperationStruct, OPayScaleAssignmentData.SeperationFormula.ToList(), OPayScaleAssignmentData.SeparationName.Id);

                            //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                            foreach (var item in Seprationformula)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {

                                    if (OPayScaleAssignmentData != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = OPayScaleAssignmentData.SeperationAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (OPayScaleAssignmentData.SeparationName != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationMaster = OPayScaleAssignmentData.SeparationName == null ? null : db.SeperationMaster.Where(e => e.Id == OPayScaleAssignmentData.SeparationName.Id).SingleOrDefault(); //OPayScaleAssignmentData.SeparationName.Id;   //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = Seprationformula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                        }
                        else
                        {
                            OEmpSeperationStructDetailsObj.SeperationPolicyFormula = null;
                        }
                    }
                    EmployeeSeperationStruct.EmployeeSeperationStructDetails = OEmpSeperationStructDetails;
                    db.EmployeeSeperationStruct.Add(EmployeeSeperationStruct);
                    db.SaveChanges();

                }
                try
                {

                    List<EmployeeSeperationStruct> OTemp2 = new List<EmployeeSeperationStruct>();
                    OTemp2.Add(db.EmployeeSeperationStruct.Where(e => e.Id == EmployeeSeperationStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeeExit OTEP = new EmployeeExit()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmployeeSeperationStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeeExit.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeeExit.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmployeeSeperationStruct = OTemp2;
                        db.EmployeeExit.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeePolicyStructCreation - LeaveStructureProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }

        public static void EmployeeSeparationStructureCreationWithUpdationTest(int OEmpPolicyStructId, int OEmployeeId,
        int OPayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int CompanyPayroll_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int CompId = Convert.ToInt32(SessionManager.CompanyId);


                var OPayScaleAssignment = db.SeperationPolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyExit.Id == CompanyPayroll_Id)
                       .Select(m => new
                       {
                           PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                           Id = m.p.Id,
                           SeperationFormula = m.p.SeperationFormula.Select(t => new SeprationFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               ExitProcess_CheckList_Policy = t.ExitProcess_CheckList_Policy,
                               ExitProcess_Config_Policy = t.ExitProcess_Config_Policy,
                               ExitProcess_Process_Policy = t.ExitProcess_Process_Policy,
                               NoticePeriod_Object = t.NoticePeriod_Object
                           }),
                           PolicyName = m.p.SeperationMaster
                           // other assignments
                       }).ToList();


                EmployeeExit OEmployee = db.EmployeeExit
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.Employee)
                                                    .Where(e => e.Id == OEmployeePayroll_Id)
                                                    .SingleOrDefault();

                List<EmployeeSeperationStructDetails> OEmpSeperationStructDetails = new List<EmployeeSeperationStructDetails>();
                int Count = 0;
                EmployeeSeperationStruct OEmpSeperationStruct = db.EmployeeSeperationStruct.Include(e => e.EmployeeSeperationStructDetails)
                    .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment))
                      .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment.SeperationMaster))
                    .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyFormula))
                    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 
                //**** check EmployeeSeperationStruct_ID****//
                //EmployeeSeperationStruct OEmpSeperationStruct = db.EmployeeSeperationStruct.Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();
                //List<EmployeeSeperationStructDetails> EmployeeSeperationStructDetails = db.EmployeeSeperationStructDetails.Where(e => e.EmployeeSeperationStruct_ == OEmpPolicyStruct.Id).ToList();
                //OEmpPolicyStruct.EmployeePolicyStructDetails = EmployeePolicyStructDetails;
                //foreach (var EmployeePolicyStructDetailsitem in EmployeePolicyStructDetails)
                //{
                //    PolicyFormula PolicyFormula = db.PolicyFormula.Where(e => e.Id == EmployeePolicyStructDetailsitem.PolicyFormula_Id).SingleOrDefault();
                //    EmployeePolicyStructDetailsitem.PolicyFormula = PolicyFormula;
                //    EmployeePolicyStructDetailsitem.PolicyName = EmployeePolicyStructDetailsitem.PolicyName;
                //    PolicyAssignment PolicyAssignment = db.PolicyAssignment.Include(e => e.PayScaleAgreement).Where(e => e.Id == EmployeePolicyStructDetailsitem.PolicyAssignment_Id).SingleOrDefault();
                //    EmployeePolicyStructDetailsitem.PolicyAssignment = PolicyAssignment;
                //    EmployeePolicyStructDetailsitem.PolicyAssignment.PayScaleAgreement = PolicyAssignment.PayScaleAgreement;
                //}
                //FuncStruct FuncStruct = db.FuncStruct.Where(e => e.Id == OEmpPolicyStruct.FuncStruct_Id).SingleOrDefault();
                //OEmpPolicyStruct.FuncStruct = FuncStruct;
                //GeoStruct GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpPolicyStruct.GeoStruct_Id).SingleOrDefault();
                //OEmpPolicyStruct.GeoStruct = GeoStruct;
                //PayStruct PayStruct = db.PayStruct.Where(e => e.Id == OEmpPolicyStruct.PayStruct_Id).SingleOrDefault();
                //OEmpPolicyStruct.PayStruct = PayStruct;



                OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(x => x.SeperationPolicyAssignment == null).ToList().ForEach(x =>
                {
                    OEmpSeperationStruct.EmployeeSeperationStructDetails.Remove(x);
                });
                db.SaveChanges();
                EmployeeSeperationStructDetails OEmpSeperationStructDetailsObj = null;
                foreach (var a in OPayScaleAssignment)
                {
                    Boolean datainstructdetails = false;
                    //foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails)
                    //{
                    //    if (a.PolicyName.Id == b.PolicyName.Id)
                    //    {
                    //        Count = 0;
                    //        break;
                    //    }
                    //    Count = 1;
                    //}
                    //if (Count == 1)
                    //{
                    //if (a.PolicyHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                    //{
                    //    continue;
                    //}
                    //if (a.PolicyFormula.Where(r => r.OthServiceBookActivity != null).Count() > 0)
                    //{

                    if (a.PolicyName != null && a.SeperationFormula.Where(r => r.ExitProcess_CheckList_Policy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<SeprationFormulaT> Seprationformula = new List<SeprationFormulaT>();

                        Seprationformula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeperationFormula.Where(r => r.ExitProcess_CheckList_Policy.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in Seprationformula)
                        {
                            foreach (var b in OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.SeperationPolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        // OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = Seprationformula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                            if (Count == 0 && Seprationformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        // OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = Seprationformula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                        }



                    }
                    if (a.PolicyName != null && a.SeperationFormula.Where(r => r.ExitProcess_Config_Policy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<SeprationFormulaT> SeprationFormula = new List<SeprationFormulaT>();

                        SeprationFormula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeperationFormula.Where(r => r.ExitProcess_Config_Policy.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in SeprationFormula)
                        {
                            foreach (var b in OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.SeperationPolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        //  OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = SeprationFormula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                            if (Count == 0 && SeprationFormula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        // OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = SeprationFormula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                        }

                    }
                    if (a.PolicyName != null && a.SeperationFormula.Where(r => r.ExitProcess_Process_Policy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<SeprationFormulaT> SeprationFormula = new List<SeprationFormulaT>();

                        SeprationFormula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeperationFormula.Where(r => r.ExitProcess_Process_Policy.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in SeprationFormula)
                        {
                            foreach (var b in OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.SeperationPolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        // OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = SeprationFormula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                            if (Count == 0 && SeprationFormula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                           .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        //  OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = SeprationFormula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                        }

                    }

                    if (a.PolicyName != null && a.SeperationFormula.Where(r => r.NoticePeriod_Object.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<SeprationFormulaT> SeprationFormula = new List<SeprationFormulaT>();

                        SeprationFormula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeperationFormula.Where(r => r.NoticePeriod_Object.Count() > 0).ToList(), a.PolicyName.Id);
                        foreach (var item in SeprationFormula)
                        {
                            foreach (var b in OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id).ToList())
                            {
                                if (item.Id == b.SeperationPolicyFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        //  OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = SeprationFormula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                            if (Count == 0 && SeprationFormula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpSeperationStructDetailsObj = new EmployeeSeperationStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpSeperationStructDetailsObj.SeperationPolicyAssignment = db.SeperationPolicyAssignment
                                            .Include(e => e.SeperationMaster)
                                            .Include(e => e.SeperationFormula)
                                        .Include(e => e.SeperationFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.SeperationFormula.Select(z => z.PayStruct))
                                          .Include(e => e.SeperationFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.PolicyName != null)
                                    {
                                        //OEmpSeperationStructDetailsObj.SeperationPolicyAssignment.SeperationMaster.TypeOfSeperation = db.LookupValue.Where(e => e.Id == a.PolicyName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                        OEmpSeperationStructDetailsObj.SeperationMaster = a.PolicyName == null ? null : db.SeperationMaster.Where(e => e.Id == a.PolicyName.Id).SingleOrDefault();
                                    }
                                    OEmpSeperationStructDetailsObj.SeperationPolicyFormula = SeprationFormula == null ? null : db.SeperationPolicyFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpSeperationStructDetailsObj.DBTrack = dbt;
                                    OEmpSeperationStructDetails.Add(OEmpSeperationStructDetailsObj);
                                };
                            }
                        }

                    }
                }
                db.EmployeeSeperationStructDetails.AddRange(OEmpSeperationStructDetails);
                OEmpSeperationStructDetails.AddRange(OEmpSeperationStruct.EmployeeSeperationStructDetails);
                OEmpSeperationStruct.EmployeeSeperationStructDetails = OEmpSeperationStructDetails;

                db.EmployeeSeperationStruct.Attach(OEmpSeperationStruct);
                db.Entry(OEmpSeperationStruct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //   EmployeeSalaryStructUpdateNew(OEmpPolicyStruct, OEmployeePayroll, mEffectiveDate);
                OEmployee.EmployeeSeperationStruct.Add(OEmpSeperationStruct);

                db.SaveChanges();

            }

        }

        public static void EmployeeSeparationStructureCreationWithUpdateFormulaTest(int OEmpPolicyStructId, int OEmployee_Id,
          int PayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int CompanyPayroll_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int Count = 0;

                EmployeeExit OEmployeeExit = db.EmployeeExit
                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)
                                  .Include(e => e.EmployeeSeperationStruct)
                                  .Include(e => e.Employee.EmpOffInfo)
                                  .Include(e => e.Employee.EmpOffInfo.PayScale)
                                  .Where(e => e.Id == OEmployeePayroll_Id).SingleOrDefault();



                var OPayScaleAssignment = db.SeperationPolicyAssignment
                       .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                      .Where(p => p.p.PayScaleAgreement.Id == PayScaleAgreementId && p.p.CompanyExit.Id == CompanyPayroll_Id)
                      .Select(m => new
                      {
                          SeparationAssignment = m.p, // or m.ppc.pc.ProdId
                          Id = m.p.Id,
                          SeparationFormula = m.p.SeperationFormula.Select(t => new SeprationFormulaT
                          {
                              GeoStruct = t.GeoStruct,
                              PayStruct = t.PayStruct,
                              FuncStruct = t.FuncStruct,
                              Name = t.Name,
                              Id = t.Id,
                              ExitProcess_CheckList_Policy = t.ExitProcess_CheckList_Policy,
                              ExitProcess_Config_Policy = t.ExitProcess_Config_Policy,
                              ExitProcess_Process_Policy = t.ExitProcess_Process_Policy,
                              NoticePeriod_Object = t.NoticePeriod_Object
                          }),
                          PolicyName = m.p.SeperationMaster
                          // other assignments
                      }).ToList();

                Employee OEmployee = db.Employee
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeeExit.Employee.Id).AsNoTracking().OrderBy(e => e.Id)
                                                    .SingleOrDefault();

                List<EmployeeSeperationStructDetails> OEmpSeperationStructDetails = new List<EmployeeSeperationStructDetails>();



                EmployeeSeperationStruct OEmpSeperationStruct = db.EmployeeSeperationStruct.Include(e => e.EmployeeSeperationStructDetails)
                    .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationMaster))
                    .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyFormula))
                     .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment))
                      .Include(e => e.EmployeeSeperationStructDetails.Select(r => r.SeperationPolicyAssignment.PayScaleAgreement))
                    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                    .Where(e => e.Id == OEmpPolicyStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 

                OEmpSeperationStructDetails = OEmpSeperationStruct.EmployeeSeperationStructDetails.ToList();
                foreach (var a in OPayScaleAssignment)
                {

                    //if (a.PolicyName.Id == b.PolicyName.Id)
                    //{
                    //Count = 0;
                    List<int> PolicyFormulaIds = new List<int>();
                    if (a.SeparationFormula.Where(r => r.ExitProcess_CheckList_Policy.Count() > 0).Count() > 0)
                    {
                        var Separationformula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeparationFormula.Where(r => r.ExitProcess_CheckList_Policy != null).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Separationformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.SeperationPolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeeSeperationStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                        //foreach (var item in PolicyFormulaNotInPolicyStructDetails)
                        //{
                        //    foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).OrderBy(e => e.PolicyName.Id).ToList())
                        //    {
                        //        if (item.Id != null && b.PolicyFormula != null)
                        //        {

                        //            if (b.PolicyFormula.Id == item.Id)
                        //            {
                        //                break;
                        //            }
                        //            else
                        //            {
                        //                EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = db.EmployeePolicyStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                        //                OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).FirstOrDefault();
                        //                db.EmployeePolicyStructDetails.Attach(OEmpPolicyStructDetailsObj);
                        //                db.Entry(OEmpPolicyStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                        //                db.SaveChanges();
                        //            }
                        //        }
                        //        if (item.Id != null && b.PolicyFormula == null)
                        //        {
                        //            EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = db.EmployeePolicyStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                        //            OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).FirstOrDefault();
                        //            db.EmployeePolicyStructDetails.Attach(OEmpPolicyStructDetailsObj);
                        //            db.Entry(OEmpPolicyStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                        //            db.SaveChanges();
                        //        }
                        //    }
                        //}
                    }

                    if (a.SeparationFormula.Where(r => r.ExitProcess_Config_Policy.Count() > 0).Count() > 0)
                    {
                        var Policyformula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeparationFormula.Where(r => r.ExitProcess_Config_Policy.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.SeperationPolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeeSeperationStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.SeparationFormula.Where(r => r.ExitProcess_Process_Policy.Count() > 0).Count() > 0)
                    {
                        var Policyformula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeparationFormula.Where(r => r.ExitProcess_Process_Policy.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.SeperationPolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeeSeperationStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.SeparationFormula.Where(r => r.NoticePeriod_Object.Count() > 0).Count() > 0)
                    {
                        var Policyformula = SeparationFinderNewTest(OEmpSeperationStruct, a.SeparationFormula.Where(r => r.NoticePeriod_Object.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(Policyformula.Select(e => e.Id));

                        var PolicyFormulaNotInPolicyStructDetails = OEmpSeperationStruct.EmployeeSeperationStructDetails.Where(e => e.SeperationMaster.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.SeperationPolicyFormula.Id)).ToList();

                        if (PolicyFormulaNotInPolicyStructDetails.Count() > 0)
                        {
                            db.EmployeeSeperationStructDetails.RemoveRange(PolicyFormulaNotInPolicyStructDetails);
                            db.SaveChanges();
                        }
                    }
                }

            }

        }
        /// <summary>
        /// LTC Policy creation
        /// </summary>
        public class LTCFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public ICollection<TravelEligibilityPolicy> TravelEligibilityPolicy { get; set; }
            public ICollection<HotelEligibilityPolicy> HotelEligibilityPolicy { get; set; }
            public ICollection<TravelModeEligibilityPolicy> TravelModeEligibilityPolicy { get; set; }
            public ICollection<TravelModeRateCeilingPolicy> TravelModeRateCeilingPolicy { get; set; }
            public ICollection<GlobalLTCBlock> GlobalLTCBlock { get; set; }
            public ICollection<DAEligibilityPolicy> DAEligibilityPolicy { get; set; }
            public string Name { get; set; }
        }

        public static LTCFormulaT LTCFinderNew(EmployeeLTCStruct OEmpLTCstruct, List<LTCFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeLTCStruct = OEmpLTCstruct;
            LTCFormulaT OLTCFormula = null;
            LTCFormulaT OLTCFormulaGeo = null;
            LTCFormulaT OLTCFormulaPay = null;
            LTCFormulaT OLTCFormulaFunc = null;
            LTCFormulaT OLTCFormulaGeopay = null;
            LTCFormulaT OLTCFormulaGeofun = null;
            LTCFormulaT OLTCFormulaGeopayfun = null;

            OLTCFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).FirstOrDefault();

            OLTCFormula = OLTCFormulaGeo;


            if (OLTCFormula == null)
            {
                OLTCFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OLTCFormulaPay = OPayScaleAssignment
              .Where(r => r.Name == OLTCFormula.Name && r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OLTCFormulaPay != null)
            {
                OLTCFormula = OLTCFormulaPay;
            }
            if (OLTCFormula == null)
            {
                OLTCFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }
            else
            {
                OLTCFormulaFunc = OPayScaleAssignment
              .Where(r => r.Name == OLTCFormula.Name && r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).FirstOrDefault();

            }

            if (OLTCFormulaFunc != null)
            {
                OLTCFormula = OLTCFormulaFunc;
            }

            OLTCFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.FuncStruct == null).FirstOrDefault();

            if (OLTCFormulaGeopay != null)
            {
                OLTCFormula = OLTCFormulaGeopay;
            }

            OLTCFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.PayStruct == null).FirstOrDefault();

            if (OLTCFormulaGeofun != null)
            {
                OLTCFormula = OLTCFormulaGeofun;
            }

            OLTCFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id).FirstOrDefault();

            if (OLTCFormulaGeopayfun != null)
            {
                OLTCFormula = OLTCFormulaGeopayfun;
            }


            return OLTCFormula;
        }//return salheadformula
        public static List<LTCFormulaT> LTCFinderNewTest(EmployeeLTCStruct OEmpLTCstruct, List<LTCFormulaT> OPayScaleAssignment, int OSalaryHeadID)
        {
            var OEmployeeLTCStruct = OEmpLTCstruct;
            List<LTCFormulaT> OLTCFormula = new List<LTCFormulaT>();
            List<LTCFormulaT> OLTCFormulaGeo = new List<LTCFormulaT>();
            List<LTCFormulaT> OLTCFormulaPay = new List<LTCFormulaT>();
            List<LTCFormulaT> OLTCFormulaFunc = new List<LTCFormulaT>();
            List<LTCFormulaT> OLTCFormulaGeopay = new List<LTCFormulaT>();
            List<LTCFormulaT> OLTCFormulaGeofun = new List<LTCFormulaT>();
            List<LTCFormulaT> OLTCFormulaGeopayfun = new List<LTCFormulaT>();

            OLTCFormulaGeo = OPayScaleAssignment
              .Where(r => r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.FuncStruct == null && r.PayStruct == null).ToList();

            OLTCFormula.AddRange(OLTCFormulaGeo);


            if (OLTCFormula.Count() == 0)
            {
                OLTCFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OLTCFormulaPay = OPayScaleAssignment
              .Where(r => r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id && r.FuncStruct == null && r.GeoStruct == null).ToList();

            }

            if (OLTCFormulaPay.Count() > 0)
            {
                OLTCFormula.AddRange(OLTCFormulaPay);
            }
            if (OLTCFormula.Count() == 0)
            {
                OLTCFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }
            else
            {
                OLTCFormulaFunc = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.PayStruct == null && r.GeoStruct == null).ToList();

            }

            if (OLTCFormulaFunc.Count() > 0)
            {
                OLTCFormula.AddRange(OLTCFormulaFunc);
            }

            OLTCFormulaGeopay = OPayScaleAssignment
                .Where(r => r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id && r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.FuncStruct == null).ToList();

            if (OLTCFormulaGeopay.Count() > 0)
            {
                OLTCFormula.AddRange(OLTCFormulaGeopay);
            }

            OLTCFormulaGeofun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.PayStruct == null).ToList();

            if (OLTCFormulaGeofun.Count() > 0)
            {
                OLTCFormula.AddRange(OLTCFormulaGeofun);
            }

            OLTCFormulaGeopayfun = OPayScaleAssignment
              .Where(r => r.FuncStruct != null && OEmployeeLTCStruct.FuncStruct != null && r.FuncStruct.Id == OEmployeeLTCStruct.FuncStruct.Id && r.GeoStruct != null && OEmployeeLTCStruct.GeoStruct != null && r.GeoStruct.Id == OEmployeeLTCStruct.GeoStruct.Id && r.PayStruct != null && OEmployeeLTCStruct.PayStruct != null && r.PayStruct.Id == OEmployeeLTCStruct.PayStruct.Id).ToList();

            if (OLTCFormulaGeopayfun.Count() > 0)
            {
                OLTCFormula.AddRange(OLTCFormulaGeopayfun);
            }


            return OLTCFormula;
        }//return salheadformula

        public static void EmployeeLTCStructCreationTest(EmployeePayroll OEmployeePayroll, int OEmployee_Id,
           int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyPayroll_Id)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var OPayScaleAssignment = db.LTCPolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreement_Id && p.p.CompanyPayroll.Id == ComPanyPayroll_Id)
                       .Select(m => new
                       {
                           LTCPolicyAssignment = m.p, // or m.ppc.pc.ProdId
                           LTCFormula = m.p.LTCFormula.Select(t => new LTCFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               TravelModeEligibilityPolicy = t.TravelModeEligibilityPolicy,
                               TravelEligibilityPolicy = t.TravelEligibilityPolicy,
                               TravelModeRateCeilingPolicy = t.TravelModeRateCeilingPolicy,
                               HotelEligibilityPolicy = t.HotelEligibilityPolicy,
                               GlobalLTCBlock = t.GlobalLTCBlock,
                               DAEligibilityPolicy = t.DAEligibilityPolicy
                           }),
                           LTCPolicyName = m.p.PolicyName
                       }).ToList();


                var OEmployee = db.Employee
                    .Where(r => r.Id == OEmployee_Id).Select(a => new
                    {
                        Id = a.Id,
                        GeoStruct = a.GeoStruct,
                        FuncStruct = a.FuncStruct,
                        PayStruct = a.PayStruct
                    }).FirstOrDefault();


                if (OPayScaleAssignment.Count() == 0)
                {
                    return;
                }

                EmployeeLTCStruct EmployeeLTCStruct = new EmployeeLTCStruct();
                {
                    EmployeeLTCStruct.EffectiveDate = mEffectiveDate;
                    if (OEmployee.GeoStruct != null)
                    {
                        EmployeeLTCStruct.GeoStruct = OEmployee.GeoStruct; //db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.FuncStruct != null)
                    {
                        EmployeeLTCStruct.FuncStruct = OEmployee.FuncStruct; // db.FuncStruct.Where(e => e.Id == OEmployee.FuncStruct.Id).SingleOrDefault();
                    };
                    if (OEmployee.PayStruct != null)
                    {
                        EmployeeLTCStruct.PayStruct = OEmployee.PayStruct; // db.PayStruct.Where(e => e.Id == OEmployee.PayStruct.Id).SingleOrDefault();
                    };
                    EmployeeLTCStruct.DBTrack = dbt;
                    //db.EmpSalStruct.Add(OEmpSalStruct);
                    //db.SaveChanges();
                    List<EmployeeLTCStructDetails> OEmpLTCStructDetails = new List<EmployeeLTCStructDetails>();
                    EmployeeLTCStructDetails OEmpLTCStructDetailsObj = null;

                    foreach (var OPayScaleAssignmentData in OPayScaleAssignment)
                    {
                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyBankPolicy != null).Count() > 0)
                        //{

                        //  OEmpSalStructDetailsObj.Amount = 0;


                        if (OPayScaleAssignmentData.LTCPolicyName != null && OPayScaleAssignmentData.LTCFormula.Count() > 0)//newly added by prashant on 24042017
                        {

                            List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                            LTCformula = LTCFinderNewTest(EmployeeLTCStruct, OPayScaleAssignmentData.LTCFormula.ToList(), OPayScaleAssignmentData.LTCPolicyName.Id);

                            //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                            foreach (var item in LTCformula)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (OPayScaleAssignmentData != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = OPayScaleAssignmentData.LTCPolicyAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (OPayScaleAssignmentData.LTCPolicyName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = OPayScaleAssignmentData.LTCPolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }
                        //else
                        //{
                        //    OEmpLTCStructDetailsObj.LTCFormula = null;
                        //}


                        //}


                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyCreditPolicy != null).Count() > 0)
                        //{
                        //OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                        //{
                        //    //  OEmpSalStructDetailsObj.Amount = 0;
                        //    if (OPayScaleAssignmentData != null)
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyAssignment = OPayScaleAssignmentData.PolicyAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                        //    }
                        //    if (OPayScaleAssignmentData.PolicyName != null)
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                        //    }

                        //    if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.PolicyFormula.Count() > 0)//newly added by prashant on 24042017
                        //    {

                        //        PolicyFormulaT Policyformula = new PolicyFormulaT();

                        //        Policyformula = PolicyFinderNew(EmployeeSeperationStruct, OPayScaleAssignmentData.PolicyFormula.Where(r => r.IncrActivity != null).ToList(), OPayScaleAssignmentData.PolicyName.Id);
                        //        OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == Policyformula.Id).SingleOrDefault();
                        //        //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                        //    }
                        //    else
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyFormula = null;
                        //    }
                        //    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                        //    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);

                        //};
                        // }
                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyDebitPolicy != null).Count() > 0)
                        //{

                        //OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                        //{
                        //    //  OEmpSalStructDetailsObj.Amount = 0;
                        //    if (OPayScaleAssignmentData != null)
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyAssignment = OPayScaleAssignmentData.PolicyAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                        //    }
                        //    if (OPayScaleAssignmentData.PolicyName != null)
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                        //    }

                        //    if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.PolicyFormula.Count() > 0)//newly added by prashant on 24042017
                        //    {

                        //        PolicyFormulaT Policyformula = new PolicyFormulaT();

                        //        Policyformula = PolicyFinderNew(EmployeeSeperationStruct, OPayScaleAssignmentData.PolicyFormula.Where(r => r.PromoActivity != null).ToList(), OPayScaleAssignmentData.PolicyName.Id);
                        //        OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == Policyformula.Id).SingleOrDefault();
                        //        //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                        //    }
                        //    else
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyFormula = null;
                        //    }
                        //    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                        //    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);

                        //};
                        // }
                        //if (OPayScaleAssignmentData.PolicyFormula.Where(r => r.PolicyEncashPolicy != null).Count() > 0)
                        //{
                        //OEmpPolicyStructDetailsObj = new EmployeePolicyStructDetails();
                        //{
                        //    //  OEmpSalStructDetailsObj.Amount = 0;
                        //    if (OPayScaleAssignmentData != null)
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyAssignment = OPayScaleAssignmentData.PolicyAssignment; //db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                        //    }
                        //    if (OPayScaleAssignmentData.PolicyName != null)
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyName = OPayScaleAssignmentData.PolicyName;  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                        //    }

                        //    if (OPayScaleAssignmentData.PolicyName != null && OPayScaleAssignmentData.PolicyFormula.Count() > 0)//newly added by prashant on 24042017
                        //    {

                        //        PolicyFormulaT Policyformula = new PolicyFormulaT();

                        //        Policyformula = PolicyFinderNew(EmployeeSeperationStruct, OPayScaleAssignmentData.PolicyFormula.Where(r => r.TransActivity != null).ToList(), OPayScaleAssignmentData.PolicyName.Id);
                        //        OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == Policyformula.Id).SingleOrDefault();
                        //        //OEmpPolicyStructDetailsObj.PolicyNewReq == null;
                        //    }
                        //    else
                        //    {
                        //        OEmpPolicyStructDetailsObj.PolicyFormula = null;
                        //    }
                        //    OEmpPolicyStructDetailsObj.DBTrack = dbt;
                        //    OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);
                        //};
                        // }
                        //  OEmpPolicyStructDetails.Add(OEmpPolicyStructDetailsObj);

                    }
                    EmployeeLTCStruct.EmployeeLTCStructDetails = OEmpLTCStructDetails;
                    db.EmployeeLTCStruct.Add(EmployeeLTCStruct);
                    db.SaveChanges();

                }
                try
                {

                    List<EmployeeLTCStruct> OTemp2 = new List<EmployeeLTCStruct>();
                    OTemp2.Add(db.EmployeeLTCStruct.Where(e => e.Id == EmployeeLTCStruct.Id).SingleOrDefault());
                    if (OEmployeePayroll == null)
                    {
                        EmployeePayroll OTEP = new EmployeePayroll()
                        {
                            Employee = db.Employee.Where(e => e.Id == OEmployee.Id).SingleOrDefault(),
                            EmployeeLTCStruct = OTemp2,
                            DBTrack = dbt
                        };
                        db.EmployeePayroll.Add(OTEP);
                        db.SaveChanges();
                    }
                    else
                    {
                        var aa = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();
                        aa.EmployeeLTCStruct = OTemp2;
                        db.EmployeePayroll.Attach(aa);
                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeeLTCStructCreation - LTCStructureProcess",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }
            }
        }


        public static void EmployeeLTCStructureCreationWithUpdationTest(int OEmpLTCStructId, int OEmployeeId,
        int OPayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int CompanyPayroll_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int CompId = Convert.ToInt32(SessionManager.CompanyId);


                var OPayScaleAssignment = db.LTCPolicyAssignment
                        .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                       .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId && p.p.CompanyPayroll.Id == CompanyPayroll_Id)
                       .Select(m => new
                       {
                           LTCAssignment = m.p, // or m.ppc.pc.ProdId
                           Id = m.p.Id,
                           LTCFormula = m.p.LTCFormula.Select(t => new LTCFormulaT
                           {
                               GeoStruct = t.GeoStruct,
                               PayStruct = t.PayStruct,
                               FuncStruct = t.FuncStruct,
                               Name = t.Name,
                               Id = t.Id,
                               HotelEligibilityPolicy = t.HotelEligibilityPolicy,
                               TravelEligibilityPolicy = t.TravelEligibilityPolicy,
                               TravelModeEligibilityPolicy = t.TravelModeEligibilityPolicy,
                               TravelModeRateCeilingPolicy = t.TravelModeRateCeilingPolicy,
                               GlobalLTCBlock = t.GlobalLTCBlock,
                               DAEligibilityPolicy = t.DAEligibilityPolicy
                           }),
                           LTCName = m.p.PolicyName
                           // other assignments
                       }).ToList();


                EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)
                               .Include(e => e.EmployeeLTCStruct)
                               .Include(e => e.Employee.EmpOffInfo)
                               .Include(e => e.Employee.EmpOffInfo.PayScale).AsNoTracking().OrderBy(e => e.Id)
                               .Where(e => e.Id == OEmployeePayroll_Id).SingleOrDefault();



                Employee OEmployee = db.Employee
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeePayroll.Employee.Id)
                                                    .SingleOrDefault();

                List<EmployeeLTCStructDetails> OEmpLTCStructDetails = new List<EmployeeLTCStructDetails>();
                int Count = 0;
                EmployeeLTCStruct OEmpLTCStruct = db.EmployeeLTCStruct.Include(e => e.EmployeeLTCStructDetails)
                    .Include(e => e.EmployeeLTCStructDetails.Select(r => r.PolicyName))
                    .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpLTCStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 


                if (OEmpLTCStruct != null)
                {


                    OEmpLTCStruct.EmployeeLTCStructDetails.Where(x => x.LTCPolicyAssignment == null).ToList().ForEach(x =>
                    {
                        OEmpLTCStruct.EmployeeLTCStructDetails.Remove(x);
                    });
                    db.SaveChanges();
                }
                EmployeeLTCStructDetails OEmpLTCStructDetailsObj = null;
                foreach (var a in OPayScaleAssignment)
                {
                    Boolean datainstructdetails = false;
                    //foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails)
                    //{
                    //    if (a.LTCName.Id == b.LTCName.Id)
                    //    {
                    //        Count = 0;
                    //        break;
                    //    }
                    //    Count = 1;
                    //}
                    //if (Count == 1)
                    //{
                    //if (a.LTCHead.Code.ToUpper() == "VPF" && !OEmployee.EmpOffInfo.VPFAppl)
                    //{
                    //    continue;
                    //}
                    //if (a.LTCFormula.Where(r => r.OthServiceBookActivity != null).Count() > 0)
                    //{

                    if (a.LTCName != null && a.LTCFormula.Where(r => r.HotelEligibilityPolicy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                        LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.HotelEligibilityPolicy.Count() > 0).ToList(), a.LTCName.Id);
                        foreach (var item in LTCformula)
                        {
                            foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.LTCName.Id).ToList())
                            {
                                if (item.Id == b.LTCFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                            if (Count == 0 && LTCformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }



                    }
                    if (a.LTCName != null && a.LTCFormula.Where(r => r.TravelModeRateCeilingPolicy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                        LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.TravelModeRateCeilingPolicy.Count() > 0).ToList(), a.LTCName.Id);
                        foreach (var item in LTCformula)
                        {
                            foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.LTCName.Id).ToList())
                            {
                                if (item.Id == b.LTCFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                            if (Count == 0 && LTCformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }

                    }
                    if (a.LTCName != null && a.LTCFormula.Where(r => r.TravelEligibilityPolicy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                        LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.TravelEligibilityPolicy.Count() > 0).ToList(), a.LTCName.Id);
                        foreach (var item in LTCformula)
                        {
                            foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.LTCName.Id).ToList())
                            {
                                if (item.Id == b.LTCFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                            if (Count == 0 && LTCformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }



                    }

                    if (a.LTCName != null && a.LTCFormula.Where(r => r.TravelModeEligibilityPolicy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                        LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.TravelModeEligibilityPolicy.Count() > 0).ToList(), a.LTCName.Id);
                        foreach (var item in LTCformula)
                        {
                            foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.LTCName.Id).ToList())
                            {
                                if (item.Id == b.LTCFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                            if (Count == 0 && LTCformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }



                    }
                    /// Global ltc

                    if (a.LTCName != null && a.LTCFormula.Where(r => r.GlobalLTCBlock.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                        LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.GlobalLTCBlock.Count() > 0).ToList(), a.LTCName.Id);
                        foreach (var item in LTCformula)
                        {
                            foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.LTCName.Id).ToList())
                            {
                                if (item.Id == b.LTCFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                            if (Count == 0 && LTCformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }



                    }

                    // DA ELGibility

                    if (a.LTCName != null && a.LTCFormula.Where(r => r.DAEligibilityPolicy.Count() > 0).Count() > 0)//newly added by prashant on 24042017
                    {
                        List<LTCFormulaT> LTCformula = new List<LTCFormulaT>();

                        LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.DAEligibilityPolicy.Count() > 0).ToList(), a.LTCName.Id);
                        foreach (var item in LTCformula)
                        {
                            foreach (var b in OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.LTCName.Id).ToList())
                            {
                                if (item.Id == b.LTCFormula.Id)
                                {
                                    datainstructdetails = true;
                                    Count = 0;
                                    break;
                                }
                                Count = 1;
                            }
                            if (Count == 1)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                            if (Count == 0 && LTCformula.Count() > 0 && datainstructdetails == false)
                            {
                                OEmpLTCStructDetailsObj = new EmployeeLTCStructDetails();
                                {
                                    if (a != null)
                                    {
                                        OEmpLTCStructDetailsObj.LTCPolicyAssignment = db.LTCPolicyAssignment
                                            .Include(e => e.LTCFormula)
                                        .Include(e => e.LTCFormula.Select(z => z.GeoStruct))
                                       .Include(e => e.LTCFormula.Select(z => z.PayStruct))
                                          .Include(e => e.LTCFormula.Select(z => z.FuncStruct))
                                           .Where(e => e.Id == a.Id).FirstOrDefault();//db.PayScaleAssignment.Where(e => e.Id == OPayScaleAssignmentData.Id).SingleOrDefault();
                                    }
                                    if (a.LTCName != null)
                                    {
                                        OEmpLTCStructDetailsObj.PolicyName = db.LookupValue.Where(e => e.Id == a.LTCName.Id).FirstOrDefault();  //db.SalaryHead.Where(e => e.Id == OPayScaleAssignmentData.SalaryHead.Id).SingleOrDefault();
                                    }
                                    OEmpLTCStructDetailsObj.LTCFormula = LTCformula == null ? null : db.LTCFormula.Where(e => e.Id == item.Id).SingleOrDefault();
                                    OEmpLTCStructDetailsObj.DBTrack = dbt;
                                    OEmpLTCStructDetails.Add(OEmpLTCStructDetailsObj);
                                };
                            }
                        }



                    }




                }
                db.EmployeeLTCStructDetails.AddRange(OEmpLTCStructDetails);
                OEmpLTCStructDetails.AddRange(OEmpLTCStruct.EmployeeLTCStructDetails);
                OEmpLTCStruct.EmployeeLTCStructDetails = OEmpLTCStructDetails;

                db.EmployeeLTCStruct.Attach(OEmpLTCStruct);
                db.Entry(OEmpLTCStruct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //   EmployeeSalaryStructUpdateNew(OEmpLTCStruct, OEmployeePayroll, mEffectiveDate);
                OEmployeePayroll.EmployeeLTCStruct.Add(OEmpLTCStruct);

                db.SaveChanges();

            }

        }


        public static void EmployeeLTCStructureCreationWithUpdateFormulaTest(int OEmpLTCStructId, int OEmployee_Id,
           int PayScaleAgreementId, DateTime mEffectiveDate, int OEmployeePayroll_Id, int CompanyPayroll_Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                int Count = 0;

                EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                    //.Include(e => e.Employee.GeoStruct)
                    //.Include(e => e.Employee.PayStruct)
                    //.Include(e => e.Employee.FuncStruct)
                                  .Include(e => e.EmployeeLTCStruct)
                                  .Include(e => e.Employee.EmpOffInfo)
                                  .Include(e => e.Employee.EmpOffInfo.PayScale)
                                  .Where(e => e.Id == OEmployeePayroll_Id).SingleOrDefault();



                var OPayScaleAssignment = db.LTCPolicyAssignment
                       .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                      .Where(p => p.p.PayScaleAgreement.Id == PayScaleAgreementId && p.p.CompanyPayroll.Id == CompanyPayroll_Id)
                      .Select(m => new
                      {
                          PolicyAssignment = m.p, // or m.ppc.pc.ProdId
                          Id = m.p.Id,
                          LTCFormula = m.p.LTCFormula.Select(t => new LTCFormulaT
                          {
                              GeoStruct = t.GeoStruct,
                              PayStruct = t.PayStruct,
                              FuncStruct = t.FuncStruct,
                              Name = t.Name,
                              Id = t.Id,
                              HotelEligibilityPolicy = t.HotelEligibilityPolicy,
                              TravelEligibilityPolicy = t.TravelEligibilityPolicy,
                              TravelModeEligibilityPolicy = t.TravelModeEligibilityPolicy,
                              TravelModeRateCeilingPolicy = t.TravelModeRateCeilingPolicy,
                              GlobalLTCBlock = t.GlobalLTCBlock,
                              DAEligibilityPolicy = t.DAEligibilityPolicy
                          }),
                          PolicyName = m.p.PolicyName
                          // other assignments
                      }).ToList();

                Employee OEmployee = db.Employee
                    //.Include(e => e.GeoStruct)
                    //.Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                                                   .Include(e => e.EmpOffInfo)
                                                    .Where(e => e.Id == OEmployeePayroll.Employee.Id).AsNoTracking().OrderBy(e => e.Id)
                                                    .SingleOrDefault();

                List<EmployeeLTCStructDetails> OEmpLTCStructDetails = new List<EmployeeLTCStructDetails>();

                EmployeeLTCStruct OEmpLTCStruct = db.EmployeeLTCStruct.Include(e => e.EmployeeLTCStructDetails)
                    .Include(e => e.EmployeeLTCStructDetails.Select(r => r.PolicyName))
                    .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCFormula))
                     .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCPolicyAssignment))
                      .Include(e => e.EmployeeLTCStructDetails.Select(r => r.LTCPolicyAssignment.PayScaleAgreement))
                    .Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.Id == OEmpLTCStructId).FirstOrDefault();//OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate == mEffectiveDate).SingleOrDefault(); 

                OEmpLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.ToList();
                foreach (var a in OPayScaleAssignment)
                {

                    //if (a.PolicyName.Id == b.PolicyName.Id)
                    //{
                    //Count = 0;
                    List<int> PolicyFormulaIds = new List<int>();
                    if (a.LTCFormula.Where(r => r.HotelEligibilityPolicy.Count() > 0).Count() > 0)
                    {
                        var LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.HotelEligibilityPolicy != null).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(LTCformula.Select(e => e.Id));

                        var LTCFormulaNotInLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LTCFormula.Id)).ToList();

                        if (LTCFormulaNotInLTCStructDetails.Count() > 0)
                        {
                            db.EmployeeLTCStructDetails.RemoveRange(LTCFormulaNotInLTCStructDetails);
                            db.SaveChanges();
                        }
                        //foreach (var item in PolicyFormulaNotInPolicyStructDetails)
                        //{
                        //    foreach (var b in OEmpPolicyStruct.EmployeePolicyStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id).OrderBy(e => e.PolicyName.Id).ToList())
                        //    {
                        //        if (item.Id != null && b.PolicyFormula != null)
                        //        {

                        //            if (b.PolicyFormula.Id == item.Id)
                        //            {
                        //                break;
                        //            }
                        //            else
                        //            {
                        //                EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = db.EmployeePolicyStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                        //                OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).FirstOrDefault();
                        //                db.EmployeePolicyStructDetails.Attach(OEmpPolicyStructDetailsObj);
                        //                db.Entry(OEmpPolicyStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                        //                db.SaveChanges();
                        //            }
                        //        }
                        //        if (item.Id != null && b.PolicyFormula == null)
                        //        {
                        //            EmployeePolicyStructDetails OEmpPolicyStructDetailsObj = db.EmployeePolicyStructDetails.Where(e => e.Id == b.Id).SingleOrDefault();
                        //            OEmpPolicyStructDetailsObj.PolicyFormula = Policyformula == null ? null : db.PolicyFormula.Where(e => e.Id == item.Id).FirstOrDefault();
                        //            db.EmployeePolicyStructDetails.Attach(OEmpPolicyStructDetailsObj);
                        //            db.Entry(OEmpPolicyStructDetailsObj).State = System.Data.Entity.EntityState.Modified;
                        //            db.SaveChanges();
                        //        }
                        //    }
                        //}
                    }

                    if (a.LTCFormula.Where(r => r.TravelEligibilityPolicy.Count() > 0).Count() > 0)
                    {
                        var LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.TravelEligibilityPolicy.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(LTCformula.Select(e => e.Id));

                        var LTCFormulaNotInLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LTCFormula.Id)).ToList();

                        if (LTCFormulaNotInLTCStructDetails.Count() > 0)
                        {
                            db.EmployeeLTCStructDetails.RemoveRange(LTCFormulaNotInLTCStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.LTCFormula.Where(r => r.TravelModeEligibilityPolicy.Count() > 0).Count() > 0)
                    {
                        var LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.TravelModeEligibilityPolicy.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(LTCformula.Select(e => e.Id));

                        var LTCFormulaNotInLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LTCFormula.Id)).ToList();

                        if (LTCFormulaNotInLTCStructDetails.Count() > 0)
                        {
                            db.EmployeeLTCStructDetails.RemoveRange(LTCFormulaNotInLTCStructDetails);
                            db.SaveChanges();
                        }
                    }
                    if (a.LTCFormula.Where(r => r.TravelModeRateCeilingPolicy.Count() > 0).Count() > 0)
                    {
                        var LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.TravelModeRateCeilingPolicy.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(LTCformula.Select(e => e.Id));

                        var LTCFormulaNotInLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LTCFormula.Id)).ToList();

                        if (LTCFormulaNotInLTCStructDetails.Count() > 0)
                        {
                            db.EmployeeLTCStructDetails.RemoveRange(LTCFormulaNotInLTCStructDetails);
                            db.SaveChanges();
                        }
                    }

                    if (a.LTCFormula.Where(r => r.GlobalLTCBlock.Count() > 0).Count() > 0)
                    {
                        var LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.GlobalLTCBlock.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(LTCformula.Select(e => e.Id));

                        var LTCFormulaNotInLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LTCFormula.Id)).ToList();

                        if (LTCFormulaNotInLTCStructDetails.Count() > 0)
                        {
                            db.EmployeeLTCStructDetails.RemoveRange(LTCFormulaNotInLTCStructDetails);
                            db.SaveChanges();
                        }
                    }

                    if (a.LTCFormula.Where(r => r.DAEligibilityPolicy.Count() > 0).Count() > 0)
                    {
                        var LTCformula = LTCFinderNewTest(OEmpLTCStruct, a.LTCFormula.Where(r => r.DAEligibilityPolicy.Count() > 0).ToList(), a.PolicyName.Id);
                        PolicyFormulaIds.AddRange(LTCformula.Select(e => e.Id));

                        var LTCFormulaNotInLTCStructDetails = OEmpLTCStruct.EmployeeLTCStructDetails.Where(e => e.PolicyName.Id == a.PolicyName.Id && !PolicyFormulaIds.Contains(e.LTCFormula.Id)).ToList();

                        if (LTCFormulaNotInLTCStructDetails.Count() > 0)
                        {
                            db.EmployeeLTCStructDetails.RemoveRange(LTCFormulaNotInLTCStructDetails);
                            db.SaveChanges();
                        }
                    }


                }

            }

        }

        public static void EmployeeAttStructCreation(EmployeeAttendance OEmployeeAttendancec, int OEmployee_Id,
             int OPayScaleAgreement_Id, DateTime mEffectiveDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Employee OEmployee = null;
                var data = db.ReportingTimingStruct.Where(e => e.GeographicalAppl == true).FirstOrDefault();

                try
                {
                    if (data != null && data.GeographicalAppl == true)
                    {
                        var TimingPolicyBatch = db.TimingPolicyBatchAssignment
                         .Include(e => e.OrgTimingPolicyBatchAssignment)
                         .Include(e => e.OrgTimingPolicyBatchAssignment.Select(s => s.FuncStruct))
                          .Include(e => e.OrgTimingPolicyBatchAssignment.Select(s => s.Geostruct))
                         .ToList();
                        OEmployee = db.Employee.Where(r => r.Id == OEmployee_Id).FirstOrDefault();
                        EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Employee_Id == OEmployee_Id).SingleOrDefault();
                        EmpReportingTimingStruct OEmpRepoTimingStructNew = new EmpReportingTimingStruct();
                        OEmpRepoTimingStructNew.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        OEmpRepoTimingStructNew.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct_Id);
                        OEmpRepoTimingStructNew.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct_Id);
                        OEmpRepoTimingStructNew.PayStruct = db.PayStruct.Find(OEmployee.PayStruct_Id);

                        OEmpRepoTimingStructNew.EffectiveDate = Convert.ToDateTime(mEffectiveDate);
                        foreach (var batch in TimingPolicyBatch)
                        {
                            if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.FuncStruct_Id == null))
                            {
                                if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                {
                                    OEmpRepoTimingStructNew.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                    OEmpRepoTimingStructNew.ReportingTimingStruct = data;
                                    break;
                                }
                            }
                            if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.Geostruct_Id == OEmployee.GeoStruct_Id && e.FuncStruct_Id == OEmployee.FuncStruct_Id))
                            {
                                if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                {
                                    OEmpRepoTimingStructNew.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id && e.FuncStruct_Id == OEmployee.FuncStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                    OEmpRepoTimingStructNew.ReportingTimingStruct = data;
                                    break;
                                }
                            }
                        }

                        var OEmpPayrollSave = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct).Where(e => e.Employee_Id == OEmployee_Id).SingleOrDefault();
                        db.EmployeeAttendance.Attach(OEmpPayrollSave).EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);
                        db.SaveChanges();
                        OEmployeeAttendance.EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);

                        db.SaveChanges();

                    }
                    else
                    {
                        var data1 = db.ReportingTimingStruct.Where(e => e.GeographicalAppl == false).FirstOrDefault();
                        if (data1 != null)
                        {

                            OEmployee = db.Employee.Where(r => r.Id == OEmployee_Id).FirstOrDefault();
                            EmployeeAttendance OEmployeeAttendance = db.EmployeeAttendance.Where(e => e.Employee_Id == OEmployee_Id).SingleOrDefault();
                            EmpReportingTimingStruct OEmpRepoTimingStructNew = new EmpReportingTimingStruct();
                            OEmpRepoTimingStructNew.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            OEmpRepoTimingStructNew.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct_Id);
                            OEmpRepoTimingStructNew.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct_Id);
                            OEmpRepoTimingStructNew.PayStruct = db.PayStruct.Find(OEmployee.PayStruct_Id);
                            OEmpRepoTimingStructNew.EffectiveDate = Convert.ToDateTime(mEffectiveDate);
                            OEmpRepoTimingStructNew.ReportingTimingStruct = data1;
                            var OEmpPayrollSave = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct).Where(e => e.Employee_Id == OEmployee_Id).SingleOrDefault();
                            db.EmployeeAttendance.Attach(OEmpPayrollSave).EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);
                            db.SaveChanges();
                            OEmployeeAttendance.EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);

                            db.SaveChanges();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = "EmployeeAttStructCreation",
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                }

            }
        }
        public static void EmployeeAttStructCreationTest(EmployeeAttendance OEmployeeAttendance, int OEmployee_Id,
          int OPayScaleAgreement_Id, DateTime mEffectiveDate, int ComPanyPayroll_Id, int? TimingStruct_Id)
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                var data = db.ReportingTimingStruct.Find(TimingStruct_Id);
                DateTime? OldEndDate = null;
                if (data != null && data.GeographicalAppl == true)
                {
                    //if same location,dept two or more batch create then system will take last batch
                    DateTime? EndDateOld = null;
                    var TimingPolicyBatch = db.TimingPolicyBatchAssignment
                        .Include(e => e.OrgTimingPolicyBatchAssignment)
                         .Include(e => e.OrgTimingPolicyBatchAssignment.Select(s => s.FuncStruct))
                           .Include(e => e.OrgTimingPolicyBatchAssignment.Select(s => s.Geostruct))
                        .OrderByDescending(e=>e.Id).ToList();

                    var OEmployee = db.Employee.Where(r => r.Id == OEmployee_Id).FirstOrDefault();

                    try
                    {
                        List<EmpReportingTimingStruct> OEmpRepoStructTot = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == OEmployeeAttendance.Id).OrderBy(e => e.EffectiveDate).ToList();
                        foreach (var item in OEmpRepoStructTot)
                        {
                            if (item.EffectiveDate < mEffectiveDate && item.EndDate < mEffectiveDate)
                            {
                            }
                            else if (item.EffectiveDate < mEffectiveDate && item.EndDate >= mEffectiveDate)
                            {
                                EmpReportingTimingStruct ORepoOld = db.EmpReportingTimingStruct.Find(item.Id);
                                EndDateOld = ORepoOld.EndDate;
                                ORepoOld.EndDate = mEffectiveDate.AddDays(-1);
                                db.Entry(ORepoOld).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                // var OOldEmpRepoStruct = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == OEmployeeAttendance.Id).Where(e => (e.EffectiveDate >= mEffectiveDate || e.EndDate == null))
                                //.ToList();

                                // foreach (var itemOld in OOldEmpRepoStruct)
                                // {
                                if (item.EffectiveDate == mEffectiveDate)
                                {

                                }
                                else if (item.EffectiveDate > mEffectiveDate && item.EndDate == null)
                                {
                                    EmpReportingTimingStruct ORepoOld = db.EmpReportingTimingStruct.Find(item.Id);
                                    EmpReportingTimingStruct OEmpRepoTimingStructNew = new EmpReportingTimingStruct();
                                    OEmpRepoTimingStructNew.DBTrack = ORepoOld.DBTrack;
                                    OEmpRepoTimingStructNew.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct_Id);
                                    OEmpRepoTimingStructNew.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct_Id);
                                    OEmpRepoTimingStructNew.PayStruct = db.PayStruct.Find(OEmployee.PayStruct_Id);
                                    OEmpRepoTimingStructNew.EndDate = EndDateOld;
                                    OEmpRepoTimingStructNew.EffectiveDate = mEffectiveDate;

                                    foreach (var batch in TimingPolicyBatch)
                                    {
                                        if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.FuncStruct_Id == null))
                                        {
                                            if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                            {
                                                OEmpRepoTimingStructNew.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                                OEmpRepoTimingStructNew.ReportingTimingStruct = data;
                                                break;
                                            }
                                        }
                                        if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.Geostruct_Id == OEmployee.GeoStruct_Id && e.FuncStruct_Id == OEmployee.FuncStruct_Id))
                                        {
                                            if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                            {
                                                OEmpRepoTimingStructNew.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id && e.FuncStruct_Id == OEmployee.FuncStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                                OEmpRepoTimingStructNew.ReportingTimingStruct = data;
                                                break;
                                            }
                                        }
                                    }

                                    var OEmpPayrollSave = db.EmployeeAttendance.Find(OEmployeeAttendance.Id);
                                    db.EmployeeAttendance.Attach(OEmpPayrollSave).EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);
                                    db.SaveChanges();

                                    //db.Entry(OEmpSalStructNew).State = System.Data.Entity.EntityState.Detached;

                                    // ORepoOld.EndDate = EndDateOld;
                                    //db.Entry(ORepoOld).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();

                                    OEmployeeAttendance.EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);

                                    db.SaveChanges();
                                }
                                else if (item.EffectiveDate < mEffectiveDate && item.EndDate == null)
                                {
                                    EmpReportingTimingStruct ORepoOld = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct).Include(e => e.TimingPolicyBatchAssignment).Where(e => e.Id == item.Id).FirstOrDefault();
                                    EmpReportingTimingStruct OEmpRepoTimingStructNew = new EmpReportingTimingStruct();
                                    OEmpRepoTimingStructNew.DBTrack = ORepoOld.DBTrack;
                                    OEmpRepoTimingStructNew.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct_Id);
                                    OEmpRepoTimingStructNew.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct_Id);
                                    OEmpRepoTimingStructNew.PayStruct = db.PayStruct.Find(OEmployee.PayStruct_Id);
                                    //OEmpRepoTimingStructNew.EndDate = EndDateOld;
                                    OEmpRepoTimingStructNew.EffectiveDate = mEffectiveDate;

                                    foreach (var batch in TimingPolicyBatch)
                                    {
                                        if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.FuncStruct_Id == null))
                                        {
                                            if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                            {
                                                OEmpRepoTimingStructNew.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                                OEmpRepoTimingStructNew.ReportingTimingStruct = data;
                                                break;
                                            }
                                        }
                                        if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.Geostruct_Id == OEmployee.GeoStruct_Id && e.FuncStruct_Id == OEmployee.FuncStruct_Id))
                                        {
                                            if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                            {
                                                OEmpRepoTimingStructNew.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == OEmployee.GeoStruct_Id && e.FuncStruct_Id == OEmployee.FuncStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                                OEmpRepoTimingStructNew.ReportingTimingStruct = data;
                                                break;
                                            }
                                        }
                                    }

                                    var OEmpPayrollSave = db.EmployeeAttendance.Find(OEmployeeAttendance.Id);
                                    db.EmployeeAttendance.Attach(OEmpPayrollSave).EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);
                                    db.SaveChanges();

                                    //db.Entry(OEmpSalStructNew).State = System.Data.Entity.EntityState.Detached;

                                    ORepoOld.EndDate = mEffectiveDate.AddDays(-1);
                                    db.Entry(ORepoOld).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    OEmployeeAttendance.EmpReportingTimingStruct.Add(OEmpRepoTimingStructNew);

                                    db.SaveChanges();
                                }
                                //}


                            }
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        LogFile Logfile = new LogFile();
                        ErrorLog Err = new ErrorLog()
                        {
                            ControllerName = "EmployeeAttStructCreation",
                            ExceptionMessage = ex.Message,
                            ExceptionStackTrace = ex.StackTrace,
                            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            LogTime = DateTime.Now
                        };
                        Logfile.CreateLogFile(Err);
                    }


                    //EmpReportingTimingStruct emprep = new EmpReportingTimingStruct()
                    //{ 
                    //    EffectiveDate = mEffectiveDate, 
                    //    DBTrack = dbt,
                    //    FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct_Id),
                    //    GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct_Id),
                    //    PayStruct = db.PayStruct.Find(OEmployee.PayStruct_Id),
                    //};





                    //try
                    //{
                    //    db.EmpReportingTimingStruct.Add(emprep);
                    //    var EmployeeAttendance = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct).Where(e => e.Employee_Id == OEmployee_Id).FirstOrDefault();
                    //    EmployeeAttendance.EmpReportingTimingStruct.Add(emprep);
                    //    db.Entry(EmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                    //    db.SaveChanges();
                    //}


                }
            }
        }
    }
}
