using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using P2BUltimate.Models;
using Payroll;
using P2BUltimate.Security;
using System.IO;
using System.Transactions;
using P2BUltimate.Controllers.Payroll.MainController;
using System.Data.Entity.Infrastructure;
using P2B.SERVICES.Factory;

namespace P2BUltimate.Process
{
    public static class SalaryGen
    {
        public static List<string> PreSalCheck(int OEmployeePayrollId, string PayMonth, bool AutoIncomeTax, int OCompanyPayrollID, string AmountChk, int ProcType, List<int> EmpIds)
        {
            List<string> Msg = new List<string>();


            foreach (var Ids in EmpIds)
            {
                int Count = 0;
                try
                {

                    using (DataBaseContext db = new DataBaseContext())
                    {
                        var OEmployee = db.Employee.Where(e => e.Id == Ids)
                            .Select(e => new
                            {
                                EmpName = e.EmpName.FullNameFML,
                                EmpCode = e.EmpCode,
                                GeoStruct_Id = e.GeoStruct.Id,
                                PayStruct_Id = e.PayStruct.Id,
                                FuncStruct_Id = e.FuncStruct.Id,
                                EmpOffInfo_Id = e.EmpOffInfo.Id


                            }).AsNoTracking().FirstOrDefault();

                        //var OEmployee = db.Employee.Where(e => e.Id == Ids).FirstOrDefault();


                        var OCompanyPayroll = db.CompanyPayroll.Where(d => d.Id == OCompanyPayrollID)
                                        .Select(e => new
                                        {
                                            Company_Id = e.Company.Id,
                                            PFMaster = e.PFMaster,
                                            PTaxMaster = e.PTaxMaster,
                                            LWFMaster = e.LWFMaster,
                                            ESICMaster = e.ESICMaster

                                        }).AsNoTracking().FirstOrDefault();
                        var OEmpOffInfo = db.EmpOff.Where(r => r.Id == OEmployee.EmpOffInfo_Id).AsNoTracking().Select(e => new
                        {
                            Id = e.Id,
                            NationalityID = e.NationalityID,
                            PFAppl = e.PFAppl,
                            PFNo = e.NationalityID.PPNO,
                            LWFAppl = e.LWFAppl,
                            ESICAppl = e.ESICAppl,
                            PTAppl = e.PTAppl,
                            Branch = e.Branch,
                            AccountType = e.AccountType,
                            AccountNo = e.AccountNo,
                            PayMode = e.PayMode,
                            UANNo = e.NationalityID.UANNo,
                            pfEstablishmentId = e.PFTrust_EstablishmentId

                        }).FirstOrDefault();

                        var State = new State();
                        //int Address_Id = 0;
                        // int JobStatus_Id = 0;
                        int PTMaster_Id = 0;
                        var Location_Id = new Int32();
                        List<Location> Location = new List<Location>();

                        Msg.Add(OEmployee.EmpCode + " - " + OEmployee.EmpName);
                        //#region GeoStruct_Check
                        if (OEmployee.GeoStruct_Id == null)
                        {
                            Msg.Add("-GeoStruct is not assigned for Employee ");
                            Count = 1;
                            continue;
                        }
                        var Loc_Id = db.GeoStruct.Where(e => e.Id == OEmployee.GeoStruct_Id).Select(e => e.Location_Id).FirstOrDefault();
                        Location_Id = (int)Loc_Id;
                        Address Address_Id = db.Location.Where(e => e.Id == Loc_Id).Select(e => e.Address).FirstOrDefault();
                        if (Address_Id == null)
                        {
                            Msg.Add("-State in GeoStruct-Location-Address-State is not assigned ");
                            Count = 1;
                            continue;
                        }
                        if (Address_Id != null)
                        {
                            var State1 = db.Address.Where(e => e.Id == Address_Id.Id).Select(e => e.State).FirstOrDefault();

                            State = State1;
                            if (State == null)
                            {
                                Msg.Add("-State in GeoStruct-Location-Address-State is not assigned ");
                                Count = 1;
                                continue;
                            }
                        }
                        //endregion GeoStruct_Check

                        // PayStruct_Check
                        if (OEmployee.PayStruct_Id == null)
                        {
                            Msg.Add("-PayStruct is not assigned for Employee ");
                            Count = 1;
                            continue;
                        }
                        JobStatus JobStatus_Id = db.PayStruct.Where(e => e.Id == OEmployee.PayStruct_Id).Select(e => e.JobStatus).FirstOrDefault();

                        if (JobStatus_Id == null)
                        {
                            Msg.Add("-PayStruct-JobStatus is not assigned for Employee ");
                            Count = 1;
                            continue;

                        }

                        //endregion PayStruct_Check

                        //region FuncStruct_Check
                        if (OEmployee.FuncStruct_Id == null)
                        {
                            Msg.Add("-FuncStruct is not assigned for Employee ");
                            Count = 1;
                            continue;
                        }
                        //endregion FuncStruct_Check

                        //region CPIEntry_Check
                        OEmployeePayrollId = db.EmployeePayroll.Where(e => e.Employee_Id == Ids).Select(e => e.Id).FirstOrDefault();
                        var CPEntry = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth).Select(e => e.Id).FirstOrDefault();
                        if (CPEntry == 0)
                        {
                            Msg.Add("-CPIndex is not defined for month " + PayMonth);
                            Count = 1;
                            continue;
                        }
                        //endregion CPIEntry_Check

                        //region Attendance_Check
                        var OSalattendanceT = db.SalAttendanceT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth).Select(e => e.Id).FirstOrDefault();
                        if (OSalattendanceT == 0)
                        {
                            Msg.Add("Attendance not available");
                            Count = 1;
                            continue;
                        }
                        //endregion Attendance_Check

                        //region Attendance_Check from joining date
                        var OSalattendanceTjoin = db.SalAttendanceT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth).FirstOrDefault();
                        var OEmployeejoin = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.Id == Ids).FirstOrDefault();
                        if (OEmployeejoin.ServiceBookDates != null && OEmployeejoin.ServiceBookDates.JoiningDate != null)
                        {
                            string joinmonth = OEmployeejoin.ServiceBookDates.JoiningDate.Value.ToString("MM/yyyy");
                            string jday = OEmployeejoin.ServiceBookDates.JoiningDate.Value.ToString("dd");
                            if (joinmonth == PayMonth)
                            {
                                double daysdiff = Convert.ToInt32(Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date.Day - Convert.ToInt32(jday)) + 1;
                                if ((OSalattendanceTjoin.PaybleDays - OSalattendanceTjoin.LWPDays) > Convert.ToDouble(daysdiff))
                                {
                                    Msg.Add("Attendance Payble Days greater than joining month days");
                                    Count = 1;
                                    continue;
                                }
                            }
                        }


                        //region Attendance_Check from joining date

                        //region SalStruct_Check
                        var OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EndDate == null).Select(e => e.Id).FirstOrDefault();
                        //  .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                        //.Where(e => e.EmployeePayroll.Id == EmpList1.Id && e.EndDate == null)
                        //      .SingleOrDefault();
                        if (OEmpSalStruct == 0)
                        {
                            Msg.Add("No salary structure available"); //return 2;//no salary structure available
                            Count = 1;
                            continue;
                        }
                        //endregion SalStruct_Check
                        // Manual arrearday enter
                        var salArrear = db.SalaryArrearT.Include(e => e.SalaryArrearPaymentT).Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.IsAuto == false && e.PayMonth == PayMonth).ToList();
                        if (salArrear != null && salArrear.Count() > 0)
                        {
                            var arrnotprocess = "";
                            foreach (var item in salArrear)
                            {

                                if (item.SalaryArrearPaymentT.Count() == 0)
                                {
                                    arrnotprocess = arrnotprocess + " " + OEmployee.EmpCode + " Paymonth: " + item.PayMonth + " From Date: " + item.FromDate.Value.ToShortDateString();
                                }
                            }
                            if (arrnotprocess != "")
                            {
                                Msg.Add("Arrear Not Process or not manualy arrear payment enter for  - " + arrnotprocess);
                                Count = 1;
                                continue;
                            }

                        }
                        //region SalStructHead_Check
                        List<Int32> SalHeadId = db.SalaryHead.Select(e => e.Id).ToList();
                        List<PayScaleAssignment> OPayScaleAssign = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.EndDate == null && !SalHeadId.Contains(e.SalaryHead.Id)).ToList();
                        if (OPayScaleAssign.Count() > 0)
                        {
                            var SalHeadNames = "";
                            foreach (var i in OPayScaleAssign)
                            {
                                SalHeadNames = SalHeadNames + " " + i.SalaryHead.Name;
                            }
                            Msg.Add("Payscale assignment not done for New SalHead - " + SalHeadNames); //return 1;//Payscale assignment not done for new head
                            Count = 1;
                            continue;
                        }
                        //endregion SalStructHead_Check


                        //region EmpOff_Check
                        if (OEmpOffInfo.Id == 0)
                        {
                            Msg.Add("EmpOff Info is not assigned for Employee ");
                            Count = 1;
                            continue;
                        }
                        //#region Nationality_Check
                        if (OEmpOffInfo.NationalityID == null)
                        {
                            Msg.Add("NationalId is not assigned for Employee ");
                            Count = 1;
                            continue;
                        }
                        //region PF_Check
                        if (OEmpOffInfo.PFAppl == true)
                        {
                            if (OEmpOffInfo.NationalityID.PFNo == null)
                            {
                                Msg.Add("PF No. is not assigned for Employee ");
                                Count = 1;
                                continue;
                            }

                            if (OEmpOffInfo.pfEstablishmentId == null || OEmpOffInfo.pfEstablishmentId == "")
                            {
                                Msg.Add("PF EstablishmentId is not assigned for Employee. ");
                                Count = 1;
                                continue;
                            }

                            if (OCompanyPayroll.PFMaster == null)
                            {
                                Msg.Add("PF Master is not assigned for Company ");
                                Count = 1;
                                continue;
                            }

                        }
                        //endregion PF_Check
                        // ESIC_Check
                        if (OEmpOffInfo.ESICAppl == true)
                        {
                            if (OEmpOffInfo.NationalityID.ESICNo == null)
                            {
                                Msg.Add("ESIC No. is not assigned for Employee ");
                                Count = 1;
                                continue;
                            }

                            if (OCompanyPayroll.ESICMaster == null)
                            {
                                Msg.Add("ESIC Master is not assigned for Company ");
                                Count = 1;
                                continue;
                            }
                            // ESIC_Location_Check
                            var ESICLoc = db.ESICMaster.Where(e => e.CompanyPayroll_Id == OCompanyPayrollID && e.EndDate == null).Select(e => e.Location.Where(r => r.Id == Location_Id)).ToList();

                            if (ESICLoc == null)
                            {
                                Msg.Add("ESIC Location is not assigned for Employee ");
                                Count = 1;
                                continue;
                            }
                            // ESIC_Location_Check


                        }
                        //endregion ESIC_Check
                        //region PT_Check
                        if (OEmpOffInfo.PTAppl == true)
                        {

                            if (OCompanyPayroll.PTaxMaster == null)
                            {
                                Msg.Add("Ptax Master is not assigned for Company ");
                                Count = 1;
                                continue;
                            }

                            PTMaster_Id = db.PTaxMaster.Where(e => e.States.Id == State.Id && e.EndDate == null).Select(e => e.Id).SingleOrDefault();
                            if (PTMaster_Id == 0)
                            {
                                Msg.Add("Ptax Master is not assigned for Employee State ");
                                Count = 1;
                                continue;
                            }



                        }
                        //endregion PT_Check
                        //region LWF_Check
                        if (db.PayScaleAssignment.Where(e => e.CompanyPayroll.Id == OCompanyPayrollID && e.SalaryHead.SalHeadOperationType.LookupVal == "LWF").ToList() == null)
                        {
                            Msg.Add("LWF Master is not assigned for Company ");
                            Count = 1;
                            continue;
                        }

                        //endregion LWF_Check


                        //endregion Nationality_Check

                        //region Account_Check
                        if (OEmpOffInfo.PayMode == null)
                        {
                            Msg.Add("Account Type. is not assigned for Employee ");
                            Count = 1;
                            continue;
                        }
                        if (OEmpOffInfo.PayMode.LookupVal.ToUpper() == "BANK")
                        {
                            if (OEmpOffInfo.AccountType == null)
                            {
                                Msg.Add("Account Type is not assigned for Employee ");
                                Count = 1;
                                continue;
                            }
                            if (OEmpOffInfo.AccountNo == null)
                            {
                                Msg.Add("Account Number is not assigned for Employee ");
                                Count = 1;
                                continue;
                            }
                        }

                        int FinYearId = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).AsNoTracking().FirstOrDefault().Id;
                        EmployeePayroll OEmpR = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.RegimiScheme).AsNoTracking().FirstOrDefault();
                        RegimiScheme ORegSch = OEmpR.RegimiScheme.Where(e => e.FinancialYear_Id == FinYearId).FirstOrDefault();
                        if (ORegSch == null)
                        {
                            Msg.Add("RegimePolicy is not defined for this financial year");
                            Count = 1;
                            continue;
                        }


                        //endregion  #region Account_Check
                    }
                    //endregion EmpOff_Check



                    if (Count == 0)
                    {
                        Msg.RemoveAt(Msg.Count - 1);
                    }
                }
                catch (Exception e)
                {

                    Msg.Add(e.Message);
                    Count = 1;
                    continue;
                }

            }
            return Msg;

        }
        public class EmpLogData
        {
            public int EmployeePayrollId { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Branch { get; set; }
        }

        public static List<string> SalaryProcess(int OEmployeePayrollId, string PayMonth, bool AutoIncomeTax, int OCompanyPayrollID, string AmountChk, int ProcType, List<int> EmpIds)
        {
            List<string> Msg = new List<string>();

            int ErrNo = 0;
            int ProcEmpCount = 0;
            string EmpDet = "";
            Msg.Add("Emp Salary Process Started");
            var mSuccessEmp = 0;
            // P2B.UTILS.P2BLogger logger = new P2B.UTILS.P2BLogger();
            EmpLogData EmpLogData = new EmpLogData();
            var mTotalEmp = 0;
            string LogMsg = "";
            foreach (var Ids in EmpIds)
            {
                mTotalEmp = EmpIds.Count();
                using (DataBaseContext db = new DataBaseContext())
                {
                    EmpLogData = db.EmployeePayroll.Where(e => e.Employee.Id == Ids).Select(e => new EmpLogData { EmployeePayrollId = e.Id, EmpCode = e.Employee.EmpCode, EmpName = e.Employee.EmpName.FullNameFML }).FirstOrDefault();
                }
                try
                {
                    LogMsg = "Salary Process Started: EmployeePayrollId=" + EmpLogData.EmployeePayrollId + " EmpCode=" + EmpLogData.EmpCode + " EmpName=" + EmpLogData.EmpName;
                    //  logger.Logging(LogMsg);
                    P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Info, LogMsg, typeof(SalaryGen));
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(4, 30, 0)))
                    {

                        using (DataBaseContext db = new DataBaseContext())
                        {
                            //Employee OEmployee = db.Employee
                            //      .Include(e => e.EmpName)
                            //      .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                            //      .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                            //      .Include(e => e.ServiceBookDates)
                            //      .Where(r => r.Id == Ids).SingleOrDefault();
                            var OEmployee = db.Employee.Where(e => e.Id == Ids)
                          .Select(e => new
                          {
                              EmpName = e.EmpName.FullNameFML,
                              EmpCode = e.EmpCode,
                              CardCode = e.CardCode,
                              GeoStruct_Id = e.GeoStruct.Id,
                              PayStruct_Id = e.PayStruct.Id,
                              FuncStruct_Id = e.FuncStruct.Id

                          }).FirstOrDefault();


                            int EmpList1 = db.EmployeePayroll.Where(e => e.Employee.Id == Ids).Select(x => x.Id).SingleOrDefault();
                            var cpi = db.CPIEntryT.Where(e => e.EmployeePayroll_Id == EmpList1 && e.PayMonth == PayMonth).FirstOrDefault();

                            if (cpi == null)
                            {
                                Msg.Add(OEmployee.CardCode + " " + OEmployee.EmpName + "-cpindex is not defined for month " + PayMonth);
                                continue;
                            }


                            Utility.DumpProcessStatus("Emp Salary Process Started");
                            Msg.Add(OEmployee.EmpCode + " - " + OEmployee.EmpName);
                            EmpDet = OEmployee.EmpCode + " - " + OEmployee.EmpName;
                            ErrNo = SalaryGen.EmployeePayrollProcess(EmpList1, PayMonth, AutoIncomeTax, OCompanyPayrollID, AmountChk, ProcType);
                            //if (ErrNo == 0)
                            //{
                            //    ProcEmpCount += 1;
                            //    Msg.Remove(OEmployee.EmpCode + " - " + OEmployee.EmpName);
                            //}
                            //    ts.Complete();
                            //}

                        }

                        using (DataBaseContext db = new DataBaseContext())
                        {

                            if (ErrNo == 0 && AutoIncomeTax == true)
                            {
                                Employee OEmployee = db.Employee
                                 .Include(e => e.EmpName)
                                 .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                                 .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                 .Include(e => e.ServiceBookDates)
                                 .Where(r => r.Id == Ids).SingleOrDefault();


                                int CompId = Convert.ToInt32(SessionManager.CompanyId);
                                CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Id == OCompanyPayrollID).SingleOrDefault();
                                var OFinancia = OCompanyPayroll.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();

                                DateTime FromPeriod = Convert.ToDateTime(OFinancia.FromDate);
                                DateTime ToPeriod = Convert.ToDateTime(OFinancia.ToDate);

                                if (OEmployee.ServiceBookDates.JoiningDate >= OFinancia.FromDate)
                                {
                                    FromPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.JoiningDate);
                                }
                                else if (OEmployee.ServiceBookDates.ServiceLastDate >= OFinancia.FromDate &&
                                   OEmployee.ServiceBookDates.ServiceLastDate <= OFinancia.ToDate)
                                {
                                    ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.ServiceLastDate);
                                }
                                else if (OEmployee.ServiceBookDates.RetirementDate >= OFinancia.FromDate &&
                                   OEmployee.ServiceBookDates.RetirementDate <= OFinancia.ToDate)
                                {
                                    ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.RetirementDate);
                                }

                                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                //                 new System.TimeSpan(0, 30, 0)))
                                //{
                                Utility.DumpProcessStatus("Emp IncomeTax Process Started");
                                //EmployeePayroll OEmployeePayroll1 = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == OEmployee.Id)//.Include(e => e.EmpSalStruct)
                                // .Include(e => e.Employee.EmpOffInfo)
                                // .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                // .Include(e => e.Employee.Gender)
                                // .Include(e => e.Employee.ServiceBookDates)
                                // .Include(e => e.Employee.EmpName)
                                // .Include(e => e.ITProjection)
                                // .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                                // .FirstOrDefault();//.AsParallel().SingleOrDefault();

                                var OEmployeePayroll = new EmployeePayroll();
                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault();
                                var OEmp = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).Select(r => r.Employee).FirstOrDefault();
                                var OEmpOff = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).Select(r => r.Employee.EmpOffInfo).FirstOrDefault();
                                var NationalityID = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpOffInfo.NationalityID).FirstOrDefault();
                                var Gender = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.Gender).FirstOrDefault();
                                var EmpName = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpName).FirstOrDefault();
                                var ServiceBookDates = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.ServiceBookDates).FirstOrDefault();
                                //List<ITProjection> ITProjection = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(r => r.ITProjection.Where(e => e.FinancialYear_Id == OFinancia.Id).ToList()).FirstOrDefault();
                                List<ITProjection> ITProjection = db.ITProjection.Include(e => e.FinancialYear).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.FinancialYear_Id == OFinancia.Id).ToList();
                                  var Regimi = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme)).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();

                                  List<RegimiScheme> RegimiScheme = Regimi.RegimiScheme.Where(e => e.FinancialYear_Id == OFinancia.Id).ToList();
                                //foreach (var i in ITProjection)
                                //{
                                //    var ITProjectionObj = db.ITProjection.Where(e => e.Id == i.Id).Select(r => r.FinancialYear).FirstOrDefault();
                                //    var FinancialYear = db.Calendar.Where(e => e.Id == ITProjectionObj.Id).FirstOrDefault();
                                //    i.FinancialYear = FinancialYear;

                                //}
                                OEmp.EmpName = EmpName;
                                OEmp.ServiceBookDates = ServiceBookDates;
                                OEmp.Gender = Gender;
                                OEmpOff.NationalityID = NationalityID;
                                OEmp.EmpOffInfo = OEmpOff;
                                OEmployeePayroll.Employee = OEmp;
                                OEmployeePayroll.ITProjection = ITProjection;
                                OEmployeePayroll.RegimiScheme = RegimiScheme;

                                List<ITProjection> FinalOITProjectionDataList = new List<ITProjection>();

                                double SalAttendanceT_monthDays = 0;
                                double SalAttendanceT_PayableDays = 0;
                                //        SalAttendanceT OSalattendanceT = db.EmployeePayroll.Where(t => t.Id == OEmployeePayroll.Id).AsNoTracking().OrderBy(e => e.Id)
                                //.Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                                SalAttendanceT OSalattendanceT = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).FirstOrDefault();

                                if (OSalattendanceT != null)
                                {
                                    SalAttendanceT_monthDays = OSalattendanceT.MonthDays;
                                    SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays;
                                }
                                else
                                {
                                    //SalAttendanceT_monthDays=
                                    SalAttendanceT_monthDays = Convert.ToDouble(DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0])));
                                    SalAttendanceT_PayableDays = 0;//Changed by Rohit
                                }

                                double TaxPaidAmt = 0;
                                SalaryT OSal = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayroll.Id).SingleOrDefault();

                                TaxPaidAmt = IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, OFinancia.Id, FromPeriod, ToPeriod, DateTime.Now, OSal, AmountChk, 1, ProcType);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    TaxPaidAmt = 0;
                                }
                                //if (AmountChk == "Actual")
                                //{
                                //    TaxPaidAmt = FinalOITProjectionDataList.Where(e => e.PickupId == 143).SingleOrDefault().ActualAmount;
                                //}
                                //else
                                //{
                                //    TaxPaidAmt = FinalOITProjectionDataList.Where(e => e.PickupId == 143).SingleOrDefault().ProjectedAmount;
                                //}

                                using (DataBaseContext db1 = new DataBaseContext())
                                {
                                    ITaxTransT ITaxEmp = db1.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(e => e.ITaxTransT.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                                    if (ITaxEmp.Mode == "AUTO")
                                    {
                                        ITaxTransT OITaxT = db1.ITaxTransT.Find(ITaxEmp.Id);
                                        OITaxT.TaxPaid = TaxPaidAmt;
                                        db1.ITaxTransT.Attach(OITaxT);
                                        db1.Entry(OITaxT).State = System.Data.Entity.EntityState.Modified;
                                        db1.SaveChanges();
                                        db1.Entry(OITaxT).State = System.Data.Entity.EntityState.Detached;

                                        SalaryT OSalT = db.SalaryT.Where(e => e.Id == OSal.Id)
                                            .Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.SalaryHead))
                                            .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)).SingleOrDefault();
                                        SalEarnDedT OSalEarnDet = OSalT.SalEarnDedT.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
                                        OSalT.TotalDeduction = OSalT.TotalDeduction + TaxPaidAmt;
                                        OSalT.TotalNet = OSalT.TotalEarning - OSalT.TotalDeduction;
                                        db1.SalaryT.Attach(OSalT);
                                        db1.Entry(OSalT).State = System.Data.Entity.EntityState.Modified;
                                        db1.SaveChanges();
                                        OSalEarnDet.Amount = TaxPaidAmt;
                                        OSalEarnDet.StdAmount = TaxPaidAmt;
                                        db1.SalEarnDedT.Attach(OSalEarnDet);
                                        db1.Entry(OSalEarnDet).State = System.Data.Entity.EntityState.Modified;
                                        db1.SaveChanges();
                                    }

                                    //db1.Entry(OSalT).State = System.Data.Entity.EntityState.Detached;



                                }

                            }
                        }
                        if (ErrNo == 0)
                        {
                            ts.Complete();
                            LogMsg = "Salary Process Completed: EmployeePayrollId=" + EmpLogData.EmployeePayrollId + " EmpCode=" + EmpLogData.EmpCode + " EmpName=" + EmpLogData.EmpName;
                            // logger.Logging(LogMsg);
                            P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Info, LogMsg, typeof(SalaryGen));
                            ProcEmpCount += 1;
                            Msg.Remove(EmpDet);
                        }
                        else
                        {
                            ts.Complete();
                            LogMsg = "Salary Process Errored: EmployeePayrollId=" + EmpLogData.EmployeePayrollId + " EmpCode=" + EmpLogData.EmpCode + " EmpName=" + EmpLogData.EmpName + "Error No:" + ErrNo.ToString();
                            P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Error, LogMsg, typeof(SalaryGen));
                            //  logger.Logging(LogMsg);

                        }
                        //if (ErrNo == 0)
                        //{
                        //    ProcEmpCount += 1;
                        //    Msg.Remove(EmpDet);
                        //}
                        //ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    LogMsg = "Salary Process Errored: EmployeePayrollId=" + EmpLogData.EmployeePayrollId + " EmpCode=" + EmpLogData.EmpCode + " EmpName=" + EmpLogData.EmpName + "Error :" + ex;
                    //  logger.Logging(LogMsg);
                    if (LogMsg.Length > 2048)
                    {
                        P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Error, LogMsg.Substring(1, 2024), typeof(SalaryGen));
                        P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Error, LogMsg.Substring(1025, 2048), typeof(SalaryGen));
                    }
                    else
                    {
                        if (LogMsg.Length > 1024)
                        {
                            P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Error, LogMsg.Substring(1, 2024), typeof(SalaryGen));
                            P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Error, LogMsg.Substring(1025, LogMsg.Length), typeof(SalaryGen));
                        }
                        else
                        {
                            P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Error, LogMsg, typeof(SalaryGen));
                        }
                    }

                    int line = Convert.ToInt32(ex.ToString().Substring(ex.ToString().IndexOf("line")).Substring(0, ex.ToString().Substring(ex.ToString().IndexOf("line")).ToString().IndexOf("\r\n")).Replace("line ", ""));
                    Msg.Add(ex.Message + "-" + line + "--Err");
                    continue;
                    //throw;
                }


                //if (ErrNo == 0)
                //{
                //    Msg.Add("Salary generated successfully");
                //}



                if (ErrNo == 1)
                    Msg.Add("Attendance not available. --Err");
                if (ErrNo == 2)
                    Msg.Add("No salary structure available. --Err");
                if (ErrNo == 3)
                    Msg.Add("define ARREAREARN/ARREARDED salary head. --Err");
                if (ErrNo == 4)
                    Msg.Add("Define IT salary head. --Err");

                if (ErrNo == 5)
                    Msg.Add("No pf head defined. --Err");

                if (ErrNo == 6)
                    Msg.Add("No pf master. --Err");
                if (ErrNo == 7)
                    Msg.Add("Salary released for this month. You can't reprocess. --Err");
                if (ErrNo == 8)
                    Msg.Add("PTAX not defined. --Err");
                if (ErrNo == 9)
                    Msg.Add("LWF not defined. --Err");
                if (ErrNo == 10)
                    Msg.Add("State is not defined in Ptax Master. --Err");
                if (ErrNo == 11)
                    Msg.Add("Negative salary already processed. --Err");

            }
            //Utility.DumpProcessStatus("Emp Salary Process Completed");
            Msg.Add(ProcEmpCount + " - Employee's Salary Generated Successfully.");
            if (EmpIds.Count - ProcEmpCount > 0)
            {
                Msg.Add(EmpIds.Count - ProcEmpCount + " - Employee's Having issue in Salary Generation.");
            }

            LogMsg = "Total Salary Process Completed: " + "Sucessfully Processed Employees :" + ProcEmpCount.ToString() + " Of Total Employees: " + mTotalEmp.ToString();
            //  logger.Logging(LogMsg);
            P2BLogger.Logger.Log(P2B.SERVICES.Interface.LogLevel.Info, LogMsg, typeof(SalaryGen));

            return Msg;
        }



        public static void DeleteSalaryListPDCC(int? emppayrollid, string PayMonth)
        {
            List<string> Msg = new List<string>();
            if (emppayrollid != 0)
            {
                try
                {

                    using (DataBaseContext db1 = new DataBaseContext())
                    {
                        // var OSalaryTChkpdcc = db1.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == emppayrollid).SingleOrDefault();
                        var CompId = Convert.ToInt32(SessionManager.CompanyId);

                        Company OCompany = null;
                        OCompany = db1.Company.Find(CompId);

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

                            Calendar OFinancia = db1.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault();
                            int OFinancialYear = OFinancia.Id;
                            var loanadvancesalhead = db1.LoanAdvanceHead.Include(x => x.LoanAdvancePolicy).Include(e => e.SalaryHead).Where(e => e.LoanAdvancePolicy.Count() > 0).ToList();

                            foreach (var ca in loanadvancesalhead)
                            {
                                if (ca.SalaryHead != null && ca.LoanAdvancePolicy.Count() > 0)
                                {
                                    List<LoanAdvRequest> OLoanRequest = db1.LoanAdvRequest.Include(e => e.LoanAdvanceHead).Include(e => e.LoanAdvanceHead.LoanAdvancePolicy).Where(e => e.EmployeePayroll_Id == emppayrollid && e.LoanAdvanceHead.SalaryHead_Id == ca.SalaryHead.Id && e.IsActive == true).ToList();

                                    //var OLoanReq = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Include(e => e.LoanAdvanceHead).Include(e => e.LoanAdvanceHead.LoanAdvancePolicy)
                                    //    .Where(e => e.EmployeePayroll_Id == OSalaryTChkpdcc.EmployeePayroll_Id && e.LoanAdvanceHead.SalaryHead_Id == ca.SalaryHead.Id).ToList();
                                    if (OLoanRequest.Count() > 0)
                                    {

                                        foreach (var loanreqid in OLoanRequest)
                                        {
                                            DateTime mLoanEndDate = OFinancia.ToDate.Value;
                                            if (loanreqid.CloserDate < mLoanEndDate)//check for loan closerdate
                                            {
                                                mLoanEndDate = loanreqid.CloserDate.Value;
                                            }

                                            // var loanrepaymentsalarymonth = loanreqid.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).FirstOrDefault();
                                            var loanrepaymentsalarymonth = db1.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == loanreqid.Id && e.PayMonth == PayMonth).AsNoTracking().FirstOrDefault();
                                            if (loanrepaymentsalarymonth != null)
                                            {

                                                //double MonthlyInterest = loanreqid.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth && e.SalaryT_Id == OSalaryT).FirstOrDefault().MonthlyInterest;
                                                //double MonthlyPrinciple = loanreqid.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth && e.SalaryT_Id == OSalaryT).FirstOrDefault().MonthlyPricipalAmount;

                                                double MonthlyInterest = db1.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == loanreqid.Id && e.InstallementDate >= OFinancia.FromDate && e.InstallementDate <= mLoanEndDate && e.SalaryT_Id != null).Select(a => a.MonthlyInterest).ToList().Sum();
                                                double MonthlyPrinciple = db1.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == loanreqid.Id && e.InstallementDate >= OFinancia.FromDate && e.InstallementDate <= mLoanEndDate && e.SalaryT_Id != null).Select(a => a.MonthlyPricipalAmount).ToList().Sum();
                                                double ActualInterestSettlement = 0, ActualPrincipleSettlement = 0;
                                                ActualInterestSettlement = db1.LoanAdvRepaymentTSettlement.Where(t => t.LoanAdvRequest_Id == loanreqid.Id && t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).Select(a => a.MonthlyInterest).ToList().Sum();
                                                ActualPrincipleSettlement = db1.LoanAdvRepaymentTSettlement.Where(t => t.LoanAdvRequest_Id == loanreqid.Id && t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mLoanEndDate && t.RepaymentDate != null).Select(a => a.MonthlyPricipalAmount).ToList().Sum();


                                                // Principle
                                                var OEmpITInvestment = db1.ITInvestmentPayment.Where(e => e.EmployeePayroll_Id == emppayrollid).Select(z => new
                                                {
                                                    Id = z.Id,
                                                    FinancialYearId = z.FinancialYear_Id,
                                                    LoanAdvanceHeadSalaryHead_Id = z.LoanAdvanceHead.SalaryHead_Id,
                                                    ITSectionListTypeLookupVal = z.ITSection.ITSectionListType.LookupVal,

                                                }).Where(e => e.FinancialYearId == OFinancialYear && e.ITSectionListTypeLookupVal.ToUpper() == "REBATE").ToList();

                                                var OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.LoanAdvanceHeadSalaryHead_Id == ca.SalaryHead_Id).SingleOrDefault();

                                                if (OEmpSalInvestmentChk != null)
                                                {

                                                    var OEmpSalInvestmentObjpri = db1.ITInvestmentPayment.Include(e => e.FinancialYear).Include(e => e.ITSection).Include(e => e.LoanAdvanceHead).Include(e => e.ITSubInvestmentPayment).Include(e => e.ITInvestment).Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();
                                                    // ITInvestmentPayment OEmpSalInvestmentObjpri = db1.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).SingleOrDefault();

                                                    OEmpSalInvestmentObjpri.ActualInvestment = (MonthlyPrinciple + ActualPrincipleSettlement);
                                                    db1.ITInvestmentPayment.Attach(OEmpSalInvestmentObjpri);
                                                    db1.Entry(OEmpSalInvestmentObjpri).State = System.Data.Entity.EntityState.Modified;
                                                    db1.SaveChanges();

                                                    db1.Entry(OEmpSalInvestmentObjpri).State = System.Data.Entity.EntityState.Detached;
                                                }

                                                // interest
                                                var OEmpITSection24EmpData = db1.ITSection24Payment.Where(e => e.EmployeePayroll_Id == emppayrollid).Select(r => new
                                                {
                                                    Id = r.Id,
                                                    FinancialYearId = r.FinancialYear_Id,
                                                    LoanAdvanceHeadId = r.LoanAdvanceHead_Id,
                                                    SalaryHeadId = r.LoanAdvanceHead.SalaryHead_Id,
                                                    ITSectionListTypeLookupVal = r.ITSection.ITSectionListType.LookupVal,

                                                }).ToList();

                                                CompanyPayroll OIncomeTax = IncomeTaxCalc._returnCompanyPayroll_IncomeTax_New(Convert.ToInt32(SessionManager.CompanyId), OFinancialYear);
                                                IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).SingleOrDefault();
                                                List<ITSection> OITSection24 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "LOAN").ToList();
                                                List<LoanAdvanceHead> OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && a.Id == ca.Id && a.ITLoan == null)).ToList();
                                                var OEmpIT24Investment = OEmpITSection24EmpData.Where(e => e.FinancialYearId == OFinancialYear && e.ITSectionListTypeLookupVal.ToUpper() == "LOAN" && e.LoanAdvanceHeadId == ca.Id).ToList();
                                                int Flag = 1;
                                                foreach (LoanAdvanceHead OITInvestments1 in OITInvestmentsList)
                                                {
                                                    var OEmpSalInvestmentChki = OEmpIT24Investment.Where(e => e.SalaryHeadId == OITInvestments1.SalaryHead.Id).SingleOrDefault();

                                                    if (OEmpSalInvestmentChk != null)
                                                    {
                                                        // ITSection24Payment OEmpSalInvestmentObjint = db1.ITSection24Payment.Where(a => a.Id == OEmpSalInvestmentChki.Id).SingleOrDefault();
                                                        var OEmpSalInvestmentObjint = db1.ITSection24Payment.Include(e => e.FinancialYear).Include(e => e.ITSection).Include(e => e.LoanAdvanceHead).Where(a => a.Id == OEmpSalInvestmentChki.Id).SingleOrDefault();

                                                        OEmpSalInvestmentObjint.ActualInterest = (MonthlyInterest + ActualInterestSettlement);

                                                        if (Flag == 1)
                                                        {
                                                            db1.ITSection24Payment.Attach(OEmpSalInvestmentObjint);
                                                            db1.Entry(OEmpSalInvestmentObjint).State = System.Data.Entity.EntityState.Modified;
                                                            db1.SaveChanges();
                                                            db1.Entry(OEmpSalInvestmentObjint).State = System.Data.Entity.EntityState.Detached;
                                                        }

                                                    }
                                                }





                                            }
                                        }
                                    }
                                }

                            }


                        }

                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        //ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);

                }
            }
        }

        public static void DeleteSalaryList(int OSalaryT, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                SalaryT OSalaryTChkemp = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT).SingleOrDefault();
                Int32? emppayrollid = OSalaryTChkemp.EmployeePayroll_Id;
                // Pdcc bank start

                Utility.DumpProcessStatus(LineNo: 132);
                // Pdcc bank end

                if (OSalaryT != 0)
                {
                    //int SalId = OSalaryT.Id;//.FirstOrDefault().Id;
                    SalaryT OSalaryTChk = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT).SingleOrDefault();
                    var OSalaryTObj = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT)
                        .Select(r => new
                        {
                            ESICTransT = r.ESICTransT,
                            ITaxTransT = r.ITaxTransT,
                            LWFTransT = r.LWFTransT,
                            //PayslipR = r.PayslipR,
                            //PerkTransT = r.PerkTransT,
                            PFECRR = r.PFECRR,
                            PTaxTransT = r.PTaxTransT,
                            //SalEarnDedT = r.SalEarnDedT,
                            //LoanAdvRepayment = r.LoanAdvRepayment
                        }).SingleOrDefault();

                    var PayslipR = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT).Select(r => r.PayslipR).SingleOrDefault().SingleOrDefault();
                    List<PaySlipDetailDedR> PaySlipDetailDedR = new List<PaySlipDetailDedR>();
                    List<PaySlipDetailEarnR> PaySlipDetailEarnR = new List<PaySlipDetailEarnR>();
                    List<PaySlipDetailLeaveR> PaySlipDetailLeaveR = new List<PaySlipDetailLeaveR>();

                    if (PayslipR != null)
                    {
                        var PayslipRdata = db.PaySlipR.Where(e => e.Id == PayslipR.Id)
                            .Select(r => new
                            {
                                PaySlipDetailDedR = r.PaySlipDetailDedR,
                                PaySlipDetailEarnR = r.PaySlipDetailEarnR,
                                PaySlipDetailLeaveR = r.PaySlipDetailLeaveR

                            }).SingleOrDefault();

                        //PaySlipDetailDedR = db.PaySlipDetailDedR.Where(e => PayslipR.Contains(e.PaySlipR)).ToList();
                        if (PayslipRdata.PaySlipDetailDedR != null)
                        {
                            db.PaySlipDetailDedR.RemoveRange(PayslipRdata.PaySlipDetailDedR);
                        }

                        //PaySlipDetailEarnR = db.PaySlipDetailEarnR.Where(e => PayslipR.Contains(e.PaySlipR)).ToList();
                        if (PayslipRdata.PaySlipDetailEarnR != null)
                        {
                            db.PaySlipDetailEarnR.RemoveRange(PayslipRdata.PaySlipDetailEarnR);
                        }
                        //PaySlipDetailLeaveR = db.PaySlipDetailLeaveR.Where(e => PayslipR.Contains(e.PaySlipR)).ToList();
                        if (PayslipRdata.PaySlipDetailLeaveR != null)
                        {
                            db.PaySlipDetailLeaveR.RemoveRange(PayslipRdata.PaySlipDetailLeaveR);
                        }
                        db.PaySlipR.Remove(PayslipR);
                    }
                    if (OSalaryTChk.ESICTransT_Id != null)
                    {
                        var ESICTransT = OSalaryTObj.ESICTransT; //db.ESICTransT.Where(e => e.Id == OSalaryTChk.ESICTransT_Id).FirstOrDefault();
                        db.ESICTransT.Remove(ESICTransT);
                    }
                    if (OSalaryTChk.ITaxTransT_Id != null)
                    {
                        var ITaxTransT = OSalaryTObj.ITaxTransT; //db.ITaxTransT.Where(e => e.Id == OSalaryTChk.ITaxTransT_Id).FirstOrDefault();
                        db.ITaxTransT.Remove(ITaxTransT);
                    }
                    if (OSalaryTChk.LWFTransT_Id != null)
                    {
                        var LWFTransT = OSalaryTObj.LWFTransT;//db.LWFTransT.Where(e => e.Id == OSalaryTChk.LWFTransT_Id).FirstOrDefault();
                        db.LWFTransT.Remove(LWFTransT);
                    }
                    if (OSalaryTChk.PFECRR_Id != null)
                    {
                        var PFECRR = OSalaryTObj.PFECRR;//db.PFECRR.Where(e => e.Id == OSalaryTChk.PFECRR_Id).FirstOrDefault();
                        db.PFECRR.Remove(PFECRR);
                    }
                    if (OSalaryTChk.PTaxTransT_Id != null)
                    {
                        var PTaxTransT = OSalaryTObj.PTaxTransT; //db.PTaxTransT.Where(e => e.Id == OSalaryTChk.PTaxTransT_Id).FirstOrDefault();
                        db.PTaxTransT.Remove(PTaxTransT);
                    }


                    var LoanAdvRepayment = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT).Select(r => r.LoanAdvRepayment).ToList().SingleOrDefault(); //db.LoanAdvRepaymentT.Where(e => e.SalaryT_Id == OSalaryTChk.Id).ToList();

                    var SalEarnDedT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT).Select(r => r.SalEarnDedT).ToList().SingleOrDefault();  //db.SalEarnDedT.Where(e => e.SalaryT_Id == OSalaryTChk.Id).ToList();


                    Utility.DumpProcessStatus(LineNo: 55);

                    if (LoanAdvRepayment != null && LoanAdvRepayment.Count() > 0)
                    {
                        LoanAdvRepayment.ToList().ForEach(x =>
                        {
                            x.RepaymentDate = null;
                            if (x.TotalLoanPaid != 0 && x.TotalLoanPaid != null)
                            {
                                x.TotalLoanPaid = x.TotalLoanPaid - x.InstallmentPaid;
                            }
                            if (x.TotalLoanBalance != 0 && x.TotalLoanBalance != null)
                            {
                                x.TotalLoanBalance = x.TotalLoanBalance + x.InstallmentPaid;
                            }
                        });
                        //db.SaveChanges();
                    }
                    Utility.DumpProcessStatus(LineNo: 66);


                    if (SalEarnDedT != null)
                    {
                        var salheadIds = db.SalaryHead.Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").Select(e => e.Id).ToList();
                        //List<SalEarnDedT> OSalEDT = OSalaryTChk.SalEarnDedT.Where(e => salheadIds.Contains(e.SalaryHead_Id.Value)).ToList();
                        List<int?> OSalEDT = OSalaryTChk.SalEarnDedT.Where(e => salheadIds.Contains(e.SalaryHead_Id.Value)).Select(e => e.SalaryHead_Id).Distinct().ToList();


                        if (OSalEDT != null && OSalEDT.Count() > 0)
                        {
                            foreach (var i in OSalEDT)
                            {

                                //var salhead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == i.SalaryHead_Id && e.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").SingleOrDefault();
                                //if (salhead != null)
                                //{
                                var loanHead = db.LoanAdvanceHead.Where(e => e.SalaryHead_Id == i).FirstOrDefault();
                                var loanReq = db.LoanAdvRequest.Where(e => e.LoanAdvanceHead_Id == loanHead.Id && e.EmployeePayroll_Id == OSalaryTChk.EmployeePayroll_Id).ToList();
                                foreach (var k in loanReq)
                                {
                                    // var LoanRpay = db.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == k.Id && e.PayMonth == PayMonth).SingleOrDefault();
                                    //var LoanAdvRepaySave = LoanAdvRepayment.Where(e => e.Id == LoanRpay.Id).FirstOrDefault();
                                    var LoanRpay = db.LoanAdvRequest.Where(e => e.Id == k.Id).Select(r => r.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth)).ToList().SingleOrDefault();
                                    List<SalEarnDedT> OSalEarnDedList = db.SalEarnDedT.Where(e => e.SalaryHead_Id == i && e.SalaryT_Id == OSalaryT).ToList();
                                    foreach (var item in OSalEarnDedList)
                                    {
                                        foreach (var F in LoanRpay)
                                        {
                                            var LoanAdvRepaySave = LoanAdvRepayment.Where(e => e.Id == F.Id).FirstOrDefault();

                                            if (LoanAdvRepaySave != null)
                                            {
                                                var NegSalData = db.NegSalData.Where(e => e.Id == item.NegSalData_Id).FirstOrDefault();
                                                if (NegSalData != null)
                                                {
                                                    LoanAdvRepaySave.InstallmentPaid = NegSalData.StdAmount;
                                                    LoanAdvRepaySave.InstallmentAmount = NegSalData.StdAmount;
                                                }

                                                LoanAdvRepaySave.RepaymentDate = null;
                                                LoanAdvRepaySave.SalaryT = null;
                                                db.LoanAdvRepaymentT.Attach(LoanAdvRepaySave);
                                                db.Entry(LoanAdvRepaySave).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(LoanAdvRepaySave).State = System.Data.Entity.EntityState.Detached;
                                            }
                                            // }
                                        }
                                    }
                                }
                            }
                        }




                        //////

                        //foreach (var i in LoanAdvRepayment)
                        //{
                        //    i.RepaymentDate = null;
                        //    i.SalaryT = null;
                        //    db.LoanAdvRepaymentT.Attach(i);
                        //    db.Entry(i).State = System.Data.Entity.EntityState.Modified;
                        //    db.SaveChanges();
                        //    db.Entry(i).State = System.Data.Entity.EntityState.Detached;
                        //}

                        db.SalEarnDedT.RemoveRange(SalEarnDedT);
                    }




                    db.SalaryT.Remove(OSalaryTChk);
                    db.SaveChanges();
                    //94
                    Utility.DumpProcessStatus(LineNo: 97);

                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
                    string localPath = new Uri(requiredPath).LocalPath;
                    if (!System.IO.Directory.Exists(localPath))
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + "\\ECR_PF_" + Convert.ToDateTime(PayMonth).ToString("MMyyyy") + ".txt";
                    path = Path.GetFullPath(new Uri(path).LocalPath);
                    if (System.IO.File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    //108
                    Utility.DumpProcessStatus(LineNo: 112);

                    requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
                    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                    localPath = new Uri(requiredPath).LocalPath;
                    if (!System.IO.Directory.Exists(localPath))
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    path = requiredPath + "\\JV_" + Convert.ToDateTime(PayMonth).ToString("MMyyyy") + ".txt";
                    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                    path = new Uri(path).LocalPath;
                    if (System.IO.File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    if (OSalaryT != 0)
                    {
                        DeleteSalaryListPDCC(emppayrollid, PayMonth);
                    }
                    //124
                    Utility.DumpProcessStatus(LineNo: 131);

                }



            }
        }

        public static void DeleteSalary(int OSalaryT, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (OSalaryT != 0)
                {
                    //int SalId = OSalaryT.Id;//.FirstOrDefault().Id;

                    //SalaryT OSalaryTChk1 = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == OSalaryT)
                    //    .Include(e => e.PayslipR)
                    //   .Include(e => e.PayslipR.Select(r => r.PaySlipDetailDedR))
                    //   .Include(e => e.PayslipR.Select(r => r.PaySlipDetailEarnR))
                    //   .Include(e => e.PayslipR.Select(r => r.PaySlipDetailLeaveR))
                    //   .Include(e => e.ESICTransT)
                    //   .Include(e => e.ITaxTransT)
                    //   .Include(e => e.LoanAdvRepayment)
                    //   .Include(e => e.LWFTransT)
                    //   .Include(e => e.PerkTransT)
                    //   .Include(e => e.PFECRR)
                    //   .Include(e => e.PTaxTransT)
                    //   .Include(e => e.SalEarnDedT)
                    //   .Include(e => e.SalEarnDedT.Select(r => r.NegSalData))
                    //   .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead))
                    //   .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType))
                    //   .SingleOrDefault();
                    //*

                    var SalaryT = new SalaryT();
                    SalaryT = db.SalaryT.Where(e => e.Id == OSalaryT && e.PayMonth == PayMonth).FirstOrDefault();
                    var GeoStruct = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.Geostruct).FirstOrDefault();
                    var PayStruct = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.PayStruct).FirstOrDefault();
                    var Company = db.GeoStruct.Where(e => e.Id == GeoStruct.Id).Select(r => r.Company).FirstOrDefault();
                    var Location = db.GeoStruct.Where(e => e.Id == GeoStruct.Id).Select(r => r.Location).FirstOrDefault();
                    var Location_LocationObj = db.Location.Where(e => e.Id == Location.Id).Select(r => r.LocationObj).FirstOrDefault();

                    var FuncStruct = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.FuncStruct).FirstOrDefault();
                    var JobPosition = db.FuncStruct.Where(e => e.Id == FuncStruct.Id).Select(r => r.JobPosition).FirstOrDefault();
                    var PFECRR = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.PFECRR).FirstOrDefault();
                    var PaymentBranch = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.PaymentBranch).FirstOrDefault();
                    var PTaxTransT = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.PTaxTransT).FirstOrDefault();
                    var LWFTransT = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.LWFTransT).FirstOrDefault();
                    var ESICTransT = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.ESICTransT).FirstOrDefault();
                    var ITaxTransT = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.ITaxTransT).FirstOrDefault();
                    List<PerkTransT> PerkTransT = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.PerkTransT.ToList()).FirstOrDefault();
                    List<LoanAdvRepaymentT> LoanAdvRepayment = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.LoanAdvRepayment.ToList()).FirstOrDefault();
                    List<SalEarnDedT> SalEarnDedT = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.SalEarnDedT.ToList()).FirstOrDefault();
                    foreach (var i in SalEarnDedT)
                    {
                        var SalEarnDedTObj = db.SalEarnDedT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                        var NegSalDataObj = db.SalEarnDedT.Where(e => e.Id == i.Id).Select(r => r.NegSalData).FirstOrDefault();
                        var SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).FirstOrDefault();
                        var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                        i.SalaryHead = SalaryHead;
                        i.NegSalData = NegSalDataObj;
                        i.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                    }

                    List<PaySlipR> PayslipR = db.SalaryT.Where(e => e.Id == SalaryT.Id).Select(r => r.PayslipR.ToList()).FirstOrDefault();
                    foreach (var j in PayslipR)
                    {
                        var PaySlipDetailDedRObj = db.PaySlipR.Where(e => e.Id == j.Id).Select(r => r.PaySlipDetailDedR).FirstOrDefault();
                        var PaySlipDetailEarnRObj = db.PaySlipR.Where(e => e.Id == j.Id).Select(r => r.PaySlipDetailEarnR).FirstOrDefault();
                        var PaySlipDetailLeaveRObj = db.PaySlipR.Where(e => e.Id == j.Id).Select(r => r.PaySlipDetailLeaveR).FirstOrDefault();
                        j.PaySlipDetailDedR = PaySlipDetailDedRObj;
                        j.PaySlipDetailEarnR = PaySlipDetailEarnRObj;
                        j.PaySlipDetailLeaveR = PaySlipDetailLeaveRObj;

                    }

                    SalaryT.Geostruct.Location = Location;
                    SalaryT.Geostruct.Location.LocationObj = Location_LocationObj;
                    SalaryT.Geostruct.Company = Company;
                    SalaryT.PayStruct = PayStruct;
                    SalaryT.FuncStruct = FuncStruct;
                    SalaryT.FuncStruct.JobPosition = JobPosition;
                    SalaryT.PFECRR = PFECRR;
                    SalaryT.PaymentBranch = PaymentBranch;
                    SalaryT.PTaxTransT = PTaxTransT;
                    SalaryT.LoanAdvRepayment = LoanAdvRepayment;
                    SalaryT.LWFTransT = LWFTransT;
                    SalaryT.ESICTransT = ESICTransT;
                    SalaryT.ITaxTransT = ITaxTransT;
                    SalaryT.PerkTransT = PerkTransT;
                    SalaryT.SalEarnDedT = SalEarnDedT;
                    SalaryT.PayslipR = PayslipR;

                    SalaryT OSalaryTChk = SalaryT;

                    Utility.DumpProcessStatus(LineNo: 55);

                    if (OSalaryTChk.LoanAdvRepayment != null && OSalaryTChk.LoanAdvRepayment.Count() > 0)
                    {
                        //foreach (var ca in OSalaryTChk.LoanAdvRepayment)
                        //{
                        //    ca.RepaymentDate = null;
                        //    ca.TotalLoanPaid = ca.TotalLoanPaid - ca.InstallmentPaid;
                        //    ca.TotalLoanBalance = ca.TotalLoanBalance + ca.InstallmentPaid;
                        //}

                        OSalaryTChk.LoanAdvRepayment.ToList().ForEach(x =>
                        {
                            x.RepaymentDate = null;
                            if (x.TotalLoanPaid != 0 && x.TotalLoanPaid != null)
                            {
                                x.TotalLoanPaid = x.TotalLoanPaid - x.InstallmentPaid;
                            }
                            if (x.TotalLoanBalance != 0 && x.TotalLoanBalance != null)
                            {
                                x.TotalLoanBalance = x.TotalLoanBalance + x.InstallmentPaid;
                            }

                        });
                        //db.SaveChanges();
                    }
                    Utility.DumpProcessStatus(LineNo: 66);
                    var OPayslipDetails = OSalaryTChk.PayslipR == null ? null : OSalaryTChk.PayslipR.SingleOrDefault();
                    if (OPayslipDetails != null)
                    {
                        if (OPayslipDetails.PaySlipDetailDedR != null)
                        { db.PaySlipDetailDedR.RemoveRange(OPayslipDetails.PaySlipDetailDedR); }

                        if (OPayslipDetails.PaySlipDetailEarnR != null)
                        { db.PaySlipDetailEarnR.RemoveRange(OPayslipDetails.PaySlipDetailEarnR); }

                        if (OPayslipDetails.PaySlipDetailLeaveR != null)
                        { db.PaySlipDetailLeaveR.RemoveRange(OPayslipDetails.PaySlipDetailLeaveR); }

                        db.PaySlipR.Remove(OPayslipDetails);
                    }
                    //77
                    Utility.DumpProcessStatus(LineNo: 79);


                    if (OSalaryTChk.ESICTransT != null)
                        db.ESICTransT.Remove(OSalaryTChk.ESICTransT);
                    if (OSalaryTChk.ITaxTransT != null)
                        db.ITaxTransT.Remove(OSalaryTChk.ITaxTransT);
                    if (OSalaryTChk.LWFTransT != null)
                        db.LWFTransT.Remove(OSalaryTChk.LWFTransT);
                    //db.per.Remove(OSalaryTChk.PerkTransT);//Added on 011217 by Rekha
                    if (OSalaryTChk.PFECRR != null)
                        db.PFECRR.Remove(OSalaryTChk.PFECRR);
                    if (OSalaryTChk.PTaxTransT != null)
                        db.PTaxTransT.Remove(OSalaryTChk.PTaxTransT);
                    if (OSalaryTChk.SalEarnDedT != null)
                    {
                        List<SalEarnDedT> OSalEDT = OSalaryTChk.SalEarnDedT.Where(r => r.NegSalData != null && r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").ToList();
                        if (OSalEDT.Count() > 0)
                        {
                            //EmployeePayroll OEmpLoan1 = db.EmployeePayroll.Where(d => d.Id == OSalaryTChk.EmployeePayroll_Id)
                            //.Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                            // .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                            // .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead)).AsNoTracking().OrderBy(e => e.Id)
                            // .FirstOrDefault();
                            //*
                            var OEmpPayroll = new EmployeePayroll();
                            OEmpPayroll = db.EmployeePayroll.Where(e => e.Id == OSalaryTChk.EmployeePayroll_Id).FirstOrDefault();
                            List<LoanAdvRequest> LoanAdvRequest = new List<LoanAdvRequest>();
                            var LoanAdvRequestObj = new LoanAdvRequest();
                            LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OSalaryTChk.EmployeePayroll_Id && e.IsActive == true)
                                .Include(e => e.LoanAdvRepaymentT)
                                .ToList();
                            foreach (var i in LoanAdvRequest)
                            {

                                var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                                var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).FirstOrDefault();
                                var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                                var SalaryHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                List<LoanAdvRepaymentT> LoanAdvRepaymentTList = new List<LoanAdvRepaymentT>();

                                // var LoanAdvRepaymentT = db.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == i.Id && e.PayMonth == PayMonth).FirstOrDefault();

                                i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                                i.LoanAdvanceHead = LoanAdvanceHead;
                                i.LoanAdvanceHead.SalaryHead = SalaryHead;

                                //if (LoanAdvRepaymentT != null)
                                //{
                                //    LoanAdvRepaymentTList.Add(LoanAdvRepaymentT);
                                //    i.LoanAdvRepaymentT = (LoanAdvRepaymentTList);
                                //}

                            }
                            OEmpPayroll.LoanAdvRequest = LoanAdvRequest;
                            EmployeePayroll OEmpLoan = OEmpPayroll;


                            List<LoanAdvRequest> OLoanAdvReuest = OEmpLoan.LoanAdvRequest.ToList();

                            if (OLoanAdvReuest != null && OLoanAdvReuest.Count() > 0)
                            {
                                foreach (var L in OSalEDT)
                                {
                                    List<LoanAdvRequest> OloanRe = OLoanAdvReuest.Where(x => x.LoanAdvRepaymentT != null && x.LoanAdvanceHead.SalaryHead.Id == L.SalaryHead.Id).ToList();
                                    foreach (var item in OloanRe)
                                    {
                                        LoanAdvRepaymentT OLoanRepay = item.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                                        if (OLoanRepay != null)
                                        {
                                            OLoanRepay.InstallmentPaid = L.NegSalData.StdAmount;
                                            OLoanRepay.InstallmentAmount = L.NegSalData.StdAmount;
                                            OLoanRepay.RepaymentDate = null;
                                            OLoanRepay.SalaryT = null;
                                            db.LoanAdvRepaymentT.Attach(OLoanRepay);
                                            db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Detached;

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            OSalEDT = OSalaryTChk.SalEarnDedT.Where(r => r.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").ToList();
                            if (OSalEDT.Count() > 0)
                            {
                                //EmployeePayroll OEmpLoan1 = db.EmployeePayroll.Where(d => d.Id == OSalaryTChk.EmployeePayroll_Id)
                                //                          .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                                //                           .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                //                           .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead)).AsNoTracking().OrderBy(e => e.Id)
                                //                           .FirstOrDefault();
                                //*
                                //var OEmpLoan = new EmployeePayroll();

                                // *****
                                //*****
                                var OEmpLoan = db.EmployeePayroll.Where(e => e.Id == OSalaryTChk.EmployeePayroll_Id).FirstOrDefault();
                                List<LoanAdvRequest> LoanAdvRequest = new List<LoanAdvRequest>();
                                var LoanAdvRequestObj = new LoanAdvRequest();
                                LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OSalaryTChk.EmployeePayroll_Id && e.IsActive == true)
                                    .Include(e => e.LoanAdvRepaymentT)
                                     .ToList();
                                foreach (var i in LoanAdvRequest)
                                {

                                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).FirstOrDefault();
                                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                                    var SalaryHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                    List<LoanAdvRepaymentT> LoanAdvRepaymentTList = new List<LoanAdvRepaymentT>();

                                    // var LoanAdvRepaymentT = db.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == i.Id && e.PayMonth == PayMonth).FirstOrDefault();
                                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                                    i.LoanAdvanceHead = LoanAdvanceHead;
                                    i.LoanAdvanceHead.SalaryHead = SalaryHead;

                                    //if (LoanAdvRepaymentT != null)
                                    //{
                                    //    LoanAdvRepaymentTList.Add(LoanAdvRepaymentT);
                                    //    i.LoanAdvRepaymentT = (LoanAdvRepaymentTList);
                                    //}

                                }
                                OEmpLoan.LoanAdvRequest = LoanAdvRequest;
                                //    EmployeePayroll OEmpLoan = OEmpPayroll;

                                List<LoanAdvRequest> OLoanAdvReuest = OEmpLoan.LoanAdvRequest.ToList();

                                if (OLoanAdvReuest != null && OLoanAdvReuest.Count() > 0)
                                {


                                    foreach (var L in OSalEDT)
                                    {
                                        List<LoanAdvRequest> OloanRe = OLoanAdvReuest.Where(x => x.LoanAdvRepaymentT != null && x.LoanAdvanceHead.SalaryHead.Id == L.SalaryHead.Id).ToList();
                                        foreach (var item in OloanRe)
                                        {
                                            LoanAdvRepaymentT OLoanRepay = item.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                                            if (OLoanRepay != null)
                                            {
                                                OLoanRepay.RepaymentDate = null;
                                                OLoanRepay.SalaryT = null;
                                                db.LoanAdvRepaymentT.Attach(OLoanRepay);
                                                db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }

                                    }
                                }
                            }
                        }
                        db.SalEarnDedT.RemoveRange(OSalaryTChk.SalEarnDedT);
                    }


                    db.SalaryT.Remove(OSalaryTChk);
                    db.SaveChanges();
                    //94
                    Utility.DumpProcessStatus(LineNo: 97);

                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
                    string localPath = new Uri(requiredPath).LocalPath;
                    if (!System.IO.Directory.Exists(localPath))
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + "\\ECR_PF_" + Convert.ToDateTime(PayMonth).ToString("MMyyyy") + ".txt";
                    path = Path.GetFullPath(new Uri(path).LocalPath);
                    if (System.IO.File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    //108
                    Utility.DumpProcessStatus(LineNo: 112);

                    requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
                    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                    localPath = new Uri(requiredPath).LocalPath;
                    if (!System.IO.Directory.Exists(localPath))
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    path = requiredPath + "\\JV_" + Convert.ToDateTime(PayMonth).ToString("MMyyyy") + ".txt";
                    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                    path = new Uri(path).LocalPath;
                    if (System.IO.File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    //124
                    Utility.DumpProcessStatus(LineNo: 131);

                }
            }
        }
        public static List<SalEarnDedT> SaveSalayDetails(List<SalEarnDedT> OSalEarnDedT, double mAmount, double mStdAmount, SalaryHead mSalaryHead)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var mSalaryHeadDetails = db.SalaryHead.Include(e => e.Frequency).Include(e => e.Type).Where(e => e.Id == mSalaryHead).SingleOrDefault();

                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                SalEarnDedT OSalEarnDedTObj = new SalEarnDedT()
                {
                    Amount = mAmount,
                    StdAmount = mStdAmount,
                    SalaryHead = mSalaryHead,
                    DBTrack = dbt

                };
                OSalEarnDedT.Add(OSalEarnDedTObj);//add ref
                return OSalEarnDedT;
            }
        }
        public static List<SalEarnDedTMultiStructure> SaveSalayDetailsMultiStructure(List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructure, double mAmount, double mStdAmount, SalaryHead mSalaryHead)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
            using (DataBaseContext db = new DataBaseContext())
            {
                // var mSalaryHeadDetails = db.SalaryHead.Include(e => e.Frequency).Include(e => e.Type).Where(e => e.Id == mSalaryHead).SingleOrDefault();

                SalEarnDedTMultiStructure OSalEarnDedTMultiStructureObj = new SalEarnDedTMultiStructure()
                {
                    Amount = mAmount,
                    StdAmount = mStdAmount,
                    SalaryHead = mSalaryHead

                };
                OSalEarnDedTMultiStructure.Add(OSalEarnDedTMultiStructureObj);//add ref
                return OSalEarnDedTMultiStructure;
            }
        }
        //send filtered pfmaster
        public static PFECRR PFcalc(PFMaster OCompanyPFMaster, EmpSalStruct OEmpSalstruct, int OEmployeePayrollId, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails, string UANNo)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //EmployeePayroll OEmployeePayroll1 = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                //                                   .Include(e => e.Employee)
                //                                   .Include(e => e.Employee.ServiceBookDates)
                //                                   .Include(e => e.Employee.Gender)
                //                                   .Include(e => e.Employee.EmpName)
                //                                   .Include(e => e.Employee.FatherName)
                //                                   .Include(e => e.SalaryArrearT)
                //                                   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPFT))
                //                                   .AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                //*
                var OEmployeePayroll = new EmployeePayroll();
                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                var OEmp = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Select(r => r.Employee).FirstOrDefault();
                var ServiceBookDates = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Select(r => r.Employee.ServiceBookDates).FirstOrDefault();
                var EmpName = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpName).FirstOrDefault();
                var FatherName = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.FatherName).FirstOrDefault();
                var Gender = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.Gender).FirstOrDefault();
                var empoff = db.EmpOff.Include(e => e.NationalityID).Include(e => e.PayProcessGroup).Include(e => e.PayProcessGroup.PayMonthConcept).Where(e => e.Id == OEmp.EmpOffInfo_Id).FirstOrDefault();
                // var SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll_Id == OEmp.Id && e.PayMonth==PayMonth).FirstOrDefault();
                List<SalaryArrearT> SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth).ToList();
                List<SalaryArrearT> SalaryArrearTnew = new List<SalaryArrearT>();
                foreach (var J in SalaryArrearT)
                {
                    var SalaryArrearTobj = db.SalaryArrearT.Where(e => e.Id == J.Id).FirstOrDefault();

                    if (SalaryArrearTobj != null)
                    {
                        var SalaryArrearTPfobj = db.SalaryArrearPFT.Where(e => e.Id == SalaryArrearTobj.SalaryArrearPFT_Id).FirstOrDefault();
                        if (SalaryArrearTPfobj != null)
                        {
                            var SalaryArrearPFT = db.SalaryArrearPFT.Where(e => e.Id == SalaryArrearTobj.SalaryArrearPFT.Id).FirstOrDefault();
                            J.SalaryArrearPFT = SalaryArrearPFT;
                        }

                        SalaryArrearTnew.Add(SalaryArrearTobj);
                    }


                }


                OEmp.EmpName = EmpName;
                OEmp.FatherName = FatherName;
                OEmp.Gender = Gender;
                OEmp.ServiceBookDates = ServiceBookDates;
                OEmployeePayroll.Employee = OEmp;
                OEmployeePayroll.SalaryArrearT = SalaryArrearTnew;
                OEmployeePayroll.Employee.EmpOffInfo = empoff;
                var mPFMaster1 = OCompanyPFMaster;

                //var OPFMaster1 = db.PFMaster
                //                        .Include(e => e.EPSWages)
                //                        .Include(e => e.EPSWages.RateMaster.Select(a => a.SalHead))
                //                        .Include(e => e.PFAdminWages)
                //                        .Include(e => e.PFEDLIWages)
                //                        .Include(e => e.PFInspWages)
                //                        .Include(e => e.EPFWages)
                //                        .Include(e => e.PFAdminWages.RateMaster)
                //                        .Include(e => e.PFEDLIWages.RateMaster)
                //                        .Include(e => e.PFInspWages.RateMaster)
                //                        .Include(e => e.EPFWages.RateMaster)
                //                        .Include(e => e.EPFWages.RateMaster.Select(a => a.SalHead))
                //                        .Include(e => e.PFTrustType)
                //                        .Where(e => e.Id == mPFMaster1.Id).OrderBy(e => e.Id)
                //                        .FirstOrDefault();

                var OPFMaster = new PFMaster();
                OPFMaster = db.PFMaster.Include(e => e.PFTrustType).Where(e => e.Id == mPFMaster1.Id).FirstOrDefault();

                var EPSWages = db.Wages.Where(e => e.Id == OPFMaster.EPSWages_Id).Include(x => x.RateMaster)
                            .Include(x => x.RateMaster.Select(z => z.SalHead)).FirstOrDefault();
                if (EPSWages != null)
                {
                    OPFMaster.EPSWages = EPSWages;
                }

                var PFAdminWages = db.Wages.Include(x => x.RateMaster).Where(e => e.Id == OPFMaster.PFAdminWages_Id).FirstOrDefault();
                if (PFAdminWages != null)
                {
                    OPFMaster.PFAdminWages = PFAdminWages;

                }
                var PFEDLIWages = db.Wages.Include(x => x.RateMaster).Where(e => e.Id == OPFMaster.PFEDLIWages_Id).FirstOrDefault();
                if (PFEDLIWages != null)
                {
                    OPFMaster.PFEDLIWages = PFEDLIWages;

                }
                var PFInspWages = db.Wages.Include(x => x.RateMaster).Where(e => e.Id == OPFMaster.PFInspWages_Id).FirstOrDefault();
                if (PFInspWages != null)
                {
                    OPFMaster.PFInspWages = PFInspWages;

                }
                var EPFWages = db.Wages.Include(x => x.RateMaster).Include(x => x.RateMaster.Select(z => z.SalHead)).Where(e => e.Id == OPFMaster.EPFWages_Id).FirstOrDefault();
                if (EPFWages != null)
                {
                    OPFMaster.EPFWages = EPFWages;

                }

                //get PF Master from companypayroll by passing companyID from session



                var OSalaryHead = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                   .SingleOrDefault();

                var OLWPAttend1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id).AsNoTracking().OrderBy(e => e.Id)
                    .Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth))
                    .FirstOrDefault();
                var OLWPAttend = OLWPAttend1.FirstOrDefault();
                //LWP leave upload given so this Comment (Goa Urban)
                double NCPDays = 0;
                string Emppayconceptf30 = OEmployeePayroll.Employee.EmpOffInfo.PayProcessGroup.PayMonthConcept.LookupVal.ToString();
                if (OLWPAttend.LWPDays == 0)
                {
                    if (Emppayconceptf30 == "FIXED30DAYS")
                    {
                        NCPDays = 30 - OLWPAttend.PaybleDays;
                    }
                    if (Emppayconceptf30 == "CALENDAR")
                    {
                        NCPDays = OLWPAttend.MonthDays - OLWPAttend.PaybleDays;
                    }
                    if (Emppayconceptf30 == "30DAYS")
                    {
                        NCPDays = OLWPAttend.MonthDays - OLWPAttend.PaybleDays;
                    }
                }
                else
                {
                    NCPDays = OLWPAttend.LWPDays;
                }
                if (NCPDays < 0)
                {
                    NCPDays = 0;
                }

                if (OPFMaster == null)
                {
                    //pf master not exist

                }
                else
                {
                    //pf arrear data call newly added on 25032017
                    double ArrCompPF = 0;
                    double ArrEmpPF = 0;
                    double ArrEmpEPS = 0;
                    double ArrEPFWages = 0;
                    double ArrEPSWages = 0;
                    double ArrEDLIWages = 0;
                    double ArrSalaryWages = 0;
                    double ArrEmpVPF = 0;

                    var OPFArrECR = OEmployeePayroll.SalaryArrearT.Where(e => e.PayMonth == PayMonth && e.SalaryArrearPFT != null && e.IsPaySlip == true).ToList();

                    if (OPFArrECR != null && OPFArrECR.Count() > 0)
                    {
                        var OPFA = OPFArrECR.Select(r => r.SalaryArrearPFT).ToList();
                        if (OPFA != null && OPFA.Count() > 0)
                        {
                            ArrCompPF = Convert.ToDouble(OPFA.Sum(r => r.CompPF));
                            ArrEDLIWages = Convert.ToDouble(OPFA.Sum(r => r.EDLIWages));
                            ArrEmpEPS = Convert.ToDouble(OPFA.Sum(r => r.EmpEPS));
                            ArrEmpPF = Convert.ToDouble(OPFA.Sum(r => r.EmpPF));
                            ArrEmpVPF = Convert.ToDouble(OPFA.Sum(r => r.EmpVPF));
                            ArrEPFWages = Convert.ToDouble(OPFA.Sum(r => r.EPFWages));
                            ArrEPSWages = Convert.ToDouble(OPFA.Sum(r => r.EPSWages));
                            ArrSalaryWages = Convert.ToDouble(OPFA.Sum(r => r.SalaryWages));


                        }
                    }
                    else
                    {


                    }
                    //officiating PF start
                    double OffCompPF = 0;
                    double OffEmpPF = 0;
                    double OffEmpEPS = 0;
                    double OffEPFWages = 0;
                    double OffEPSWages = 0;
                    double OffEDLIWages = 0;
                    double OffSalaryWages = 0;
                    double OffEmpVPF = 0;

                    var OPFOffECR = db.BMSPaymentReq.Include(e => e.OfficiatingPFT).Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth && e.OfficiatingPFT != null && e.IsCancel == false && e.TrClosed == true && e.TrReject == false).ToList();

                    if (OPFOffECR != null && OPFOffECR.Count() > 0)
                    {
                        var OPFA = OPFOffECR.Select(r => r.OfficiatingPFT).ToList();
                        if (OPFA != null && OPFA.Count() > 0)
                        {
                            OffCompPF = Convert.ToDouble(OPFA.Sum(r => r.CompPF));
                            OffEDLIWages = Convert.ToDouble(OPFA.Sum(r => r.EDLIWages));
                            OffEmpEPS = Convert.ToDouble(OPFA.Sum(r => r.EmpEPS));
                            OffEmpPF = Convert.ToDouble(OPFA.Sum(r => r.EmpPF));
                            OffEmpVPF = Convert.ToDouble(OPFA.Sum(r => r.EmpVPF));
                            OffEPFWages = Convert.ToDouble(OPFA.Sum(r => r.EPFWages));
                            OffEPSWages = Convert.ToDouble(OPFA.Sum(r => r.EPSWages));
                            OffSalaryWages = Convert.ToDouble(OPFA.Sum(r => r.SalaryWages));


                        }
                    }
                    // Nkgsb Bank officiating allowance eps value should 0
                    OffCompPF = OffCompPF + OffEmpEPS;
                    OffEmpEPS = 0;
                    OffEDLIWages = 0;
                    OffEPSWages = 0;
                    //officiating PF end

                    // newly added completed


                    double mAge = 0;
                    var mDateofBirth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate;// db.Employee.Where(e => e.Id == mEmployeeID).Select(e => e.ServiceBookDates.BirthDate);
                    DateTime start = mDateofBirth.Value;
                    DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                    //   double daysInEndMonth = (end - end.AddMonths(1)).Days;
                    double daysInEndMonth = (end.AddMonths(1) - end).Days;
                    double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                    mAge = months / 12;
                    double pensioncompletedays = 0;
                    double paidsalarydays = 0;
                    if (compMonth == OPFMaster.PensionAge * 12 && start.Month == end.Month)
                    {
                        mAge = compMonth / 12;
                        // pension age complete in Salary month then pension will applicable only till that days
                        pensioncompletedays = start.Day;
                        var salatt = db.SalAttendanceT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayrollId).FirstOrDefault();
                        paidsalarydays = salatt.PaybleDays;

                    }
                    //mAge = (DateTime.Now.Date - mDateofBirth.Value.Date).Days;
                    //mAge = mAge / 365;

                    double mCompPF = 0;
                    double mEmpPension = 0;
                    double mEmpPF = 0;
                    double mEmpVPF = 0;
                    double mTotalCompPF = 0;
                    double highpension = 0;
                    double highpensionper = 0;
                    double mGrossWages = OSalaryDetails.Where(r => r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && r.SalaryHead.InPayslip == true)
                        .Sum(e => e.Amount);
                    mGrossWages = Math.Round(mGrossWages, 0);
                    //double mAdminWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.PFAdminWages, OSalaryDetails, null);
                    //SalHeadFormula PFFormula = Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalstruct, OSalaryHead.SalaryHead.Id);
                    //double mPFWages = Process.SalaryHeadGenProcess.Wagecalc(PFFormula, OSalaryDetails, null);
                    double mPFWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPFWages, OSalaryDetails, null);

                    var EmpPFHead = OEmpSalstruct.EmpSalStructDetails
                                    .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault();
                    if (EmpPFHead != null && EmpPFHead.SalHeadFormula_Id != null)
                    {
                        Int32 Pfformulaid = EmpPFHead.SalHeadFormula.Id;
                        var OSalHeadFormualQuery = SalaryHeadGenProcess._returnSalHeadFormula(Pfformulaid);
                        mPFWages = SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, null);
                        mPFWages = SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPFWages);
                    }
                    //mPFWages = Math.Round(mPFWages, 0);
                    mPFWages = Process.SalaryHeadGenProcess.RoundingFunction(OSalaryHead.SalaryHead, mPFWages);
                    double mPensionWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPSWages, OSalaryDetails, null);

                    var EmpPENSIONHead = OEmpSalstruct.EmpSalStructDetails
                                  .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault();
                    if (EmpPENSIONHead != null && EmpPENSIONHead.SalHeadFormula_Id != null)
                    {
                        Int32 Pensionformulaid = EmpPENSIONHead.SalHeadFormula.Id;
                        var OSalHeadFormualQuery = SalaryHeadGenProcess._returnSalHeadFormula(Pensionformulaid);
                        mPensionWages = SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, null);
                        mPensionWages = SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPensionWages);
                    }

                    mPensionWages = Math.Round(mPensionWages, 0);
                    double mEDLIWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.PFEDLIWages, OSalaryDetails, null);
                    mEDLIWages = Math.Round(mEDLIWages, 0);
                    //check database
                    //mEmpPF = Process.SalaryHeadGenProcess.SalHeadAmountCalc(PFFormula, OSalaryDetails, null, OEmployeePayroll);
                    mEmpPF = Math.Round(mPFWages * OPFMaster.EmpPFPerc / 100, 0);
                    //mTotalCompPF= mPFWages * (mPFMaster.CPFPerc + mPFMaster.EPSPerc); //applay rounding
                    mEmpPF = Math.Round(mEmpPF, 0, MidpointRounding.AwayFromZero);

                    // pension age complete in Salary month then pension will applicable only till that days
                    if (pensioncompletedays != 0)
                    {
                        if (paidsalarydays < pensioncompletedays)
                        {
                            pensioncompletedays = paidsalarydays;
                        }
                        double OEPSWages = 0;
                        if (OSalaryDetails != null)
                        {
                            OEPSWages = OPFMaster.EPSWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Percentage / 100 * e.uir.Amount).Sum();

                            OEPSWages = OEPSWages + OPFMaster.EPSWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Amount).Sum();
                        }
                        //
                        double OEDLIWages = 0;
                        if (OSalaryDetails != null)
                        {
                            OEDLIWages = OPFMaster.PFEDLIWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Percentage / 100 * e.uir.Amount).Sum();

                            OEDLIWages = OEDLIWages + OPFMaster.PFEDLIWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Amount).Sum();
                        }


                        OEPSWages = (OEPSWages / paidsalarydays) * pensioncompletedays;
                        OEDLIWages = (OEDLIWages / paidsalarydays) * pensioncompletedays;
                        OEPSWages = Math.Round(OEPSWages, 0);
                        OEDLIWages = Math.Round(OEDLIWages, 0);

                        if (OEPSWages < mPensionWages)
                        {
                            mPensionWages = OEPSWages;
                        }
                        if (OEDLIWages < mEDLIWages)
                        {
                            mEDLIWages = OEDLIWages;
                        }

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

                    if (OEmployeePayroll.Employee.EmpOffInfo.PensionAppl == false)
                    {
                        mEmpPension = 0;
                        mPensionWages = 0;
                    }
                    mCompPF = mEmpPF - mEmpPension;

                    if (mCompPF > OPFMaster.CompPFCeiling)
                    {
                        mCompPF = OPFMaster.CompPFCeiling;
                    }

                    mEmpVPF = OSalaryDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault() != null ?
                        OSalaryDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault().Amount : 0;
                    // vpf amount calc start
                    var OSalaryHeadvppf = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF")
                  .SingleOrDefault();
                    if (OSalaryHeadvppf != null)
                    {
                        if (OSalaryHeadvppf.SalHeadFormula != null)
                        {
                            mEmpVPF = SalaryHeadGenProcess.SalHeadAmountCalc(OSalaryHeadvppf.SalHeadFormula.Id, OSalaryDetails, null, OEmployeePayroll,
                                    PayMonth, OSalaryHeadvppf.SalaryHead.Code.ToUpper() == "VPF" ? true : false);
                            mEmpVPF = SalaryHeadGenProcess.RoundingFunction(OSalaryHeadvppf.SalaryHead, mEmpVPF);


                        }
                    }

                    //vpf amount calc end

                    // Surat Dcc comppf amount celling

                    var OSalaryHeadcomppf = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CPF")
                .SingleOrDefault();
                    if (OSalaryHeadcomppf != null)
                    {

                        if (OSalaryHeadcomppf.SalHeadFormula != null)
                        {
                            if ((mCompPF + mEmpPension) > OSalaryHeadcomppf.Amount)
                            {
                                mCompPF = OSalaryHeadcomppf.Amount - mEmpPension;
                            }

                        }
                    }

                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                    int Comp_Id = Convert.ToInt32(HttpContext.Current.Session["CompId"].ToString());
                    PFECRR OPFECRR = new PFECRR()
                    {
                        //PFCalendar = db.Calendar.Find(cal.Id),
                        UAN = UANNo,//OEmpOff.NationalityID.UANNo,
                        UAN_Name = OEmployeePayroll.Employee.EmpName.FullNameFML,
                        Establishment_ID = OPFMaster.EstablishmentID,
                        Establishment_Name = db.Company.Where(e => e.Id == Comp_Id).Select(r => r.Name).SingleOrDefault(),

                        Gross_Wages = mGrossWages,
                        EPF_Wages = mPFWages,
                        EPS_Wages = mPensionWages,
                        EDLI_Wages = mEDLIWages,
                        EE_Share = (mEmpPF),
                        EPS_Share = (mEmpPension),
                        ER_Share = (mCompPF),
                        EE_VPF_Share = (mEmpVPF),
                        NCP_Days = NCPDays,//OLWPAttend == null ? 0 : OLWPAttend.LWPDays == null ? 0 : OLWPAttend.LWPDays,
                        Refund_of_Advances = 0,
                        //part of arrears file newly added on 25032017
                        Arrear_EPF_Wages = ArrEPFWages,
                        Arrear_EPS_Wages = ArrEPSWages,
                        Arrear_EDLI_Wages = ArrEDLIWages,
                        Arrear_EE_Share = ArrEmpPF + ArrEmpVPF,
                        Arrear_EPS_Share = ArrEmpEPS,
                        Arrear_ER_Share = ArrCompPF,

                        Officiating_EPF_Wages = OffEPFWages,
                        Officiating_EPS_Wages = OffEPSWages,
                        Officiating_EDLI_Wages = OffEDLIWages,
                        Officiating_EE_Share = OffEmpPF,
                        Officiating_EPS_Share = OffEmpEPS,
                        Officiating_ER_Share = OffCompPF,
                        Officiating_VPF_Share = OffEmpVPF,

                        //required at the time of uan
                        Father_Husband_Name = OEmployeePayroll.Employee.FatherName == null ? "" : OEmployeePayroll.Employee.FatherName.FullNameFML,//.Employee.MaritalStatus.LookupVal.ToUpper() == "MARRIED" ? OEmployeePayroll.Employee.HusbandName.ToString() : OEmployeePayroll.Employee.FatherName.FullNameFML,
                        Relationship_with_the_Member = "F",
                        Date_of_Joining_EPF = OEmployeePayroll.Employee.ServiceBookDates.PFJoingDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PFJoingDate,
                        Date_of_Joining_EPS = OEmployeePayroll.Employee.ServiceBookDates.PensionJoingDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PensionJoingDate,
                        Date_of_Exit_from_EPF = OEmployeePayroll.Employee.ServiceBookDates.PFExitDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PFExitDate,
                        Date_of_Exit_from_EPS = OEmployeePayroll.Employee.ServiceBookDates.PensionExitDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PensionExitDate,
                        Reason_for_leaving = "",
                        //calculate at the time of ECR generation
                        //Administrative_Charges_AC2= Math.Round( mAdminWages * OPFMaster.AdminCharges/100,0),
                        //Inspection_Charges_AC2 = Math.Round(mAdminWages * OPFMaster.InspCharges / 100, 0),
                        //EDLI_Contribution_AC21 = Math.Round(mEDLIWages * OPFMaster.EDLICharges / 100, 0),
                        //Administrative_Charges_AC22 = Math.Round( mEDLIWages * OPFMaster.EDLICharges/100,0),
                        //Inspection_Charges_AC22 = Math.Round( mEDLIWages * OPFMaster.PaymentEDLIS/100,0),
                        //Exemption_Status = OPFMaster.PFTrustType.LookupVal,

                        Date_of_Birth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value,
                        Gender = OEmployeePayroll.Employee.Gender == null ? "" : OEmployeePayroll.Employee.Gender.LookupVal.ToUpper(),
                        Return_Month = "",
                        Wage_Month = PayMonth,

                        DBTrack = dbt
                    };

                    return OPFECRR;

                }


                return null;
            }
        }
        //PT Calculation Process
        public static PFECRR PFcalcArr(PFMaster OCompanyPFMaster, EmpSalStruct OEmpSalstruct, int OEmployeePayrollId, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails, string UANNo)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var mPFMaster1 = OCompanyPFMaster;

                var OPFMaster = db.PFMaster
                                        .Include(e => e.EPSWages)
                                        .Include(e => e.EPSWages.RateMaster)
                                        .Include(e => e.PFAdminWages)
                                        .Include(e => e.PFEDLIWages)
                                        .Include(e => e.PFInspWages)
                                        .Include(e => e.EPFWages)
                                        .Include(e => e.PFAdminWages.RateMaster)
                                        .Include(e => e.PFEDLIWages.RateMaster)
                                        .Include(e => e.PFInspWages.RateMaster)
                                        .Include(e => e.EPFWages.RateMaster)
                                        .Include(e => e.EPFWages.RateMaster.Select(r => r.SalHead))
                                        .Include(e => e.PFTrustType)
                                        .Where(e => e.Id == mPFMaster1.Id)

                                        .SingleOrDefault();
                //get PF Master from companypayroll by passing companyID from session



                var OSalaryHead = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                   .SingleOrDefault();


                var OLWPAttend = db.SalAttendanceT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Id == OEmployeePayrollId).FirstOrDefault();

                if (OPFMaster == null)
                {
                    //pf master not exist

                }
                else
                {
                    //pf arrear data call newly added on 25032017
                    double ArrCompPF = 0;
                    double ArrEmpPF = 0;
                    double ArrEmpEPS = 0;
                    double ArrEPFWages = 0;
                    double ArrEPSWages = 0;
                    double ArrEDLIWages = 0;
                    double ArrSalaryWages = 0;
                    double ArrEmpVPF = 0;

                    EmployeePayroll OEmployeePayroll = db.EmployeePayroll
                                           .Include(e => e.Employee)
                                            .Include(e => e.Employee.EmpOffInfo)
                                             .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                           .Include(e => e.Employee.EmpName)
                                           .Include(e => e.Employee.Gender)
                                           .Include(e => e.Employee.MaritalStatus)
                                           .Include(e => e.Employee.ServiceBookDates)
                                           .Where(d => d.Id == OEmployeePayrollId).FirstOrDefault();

                    double mAge = 0;
                    var mDateofBirth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate;// db.Employee.Where(e => e.Id == mEmployeeID).Select(e => e.ServiceBookDates.BirthDate);
                    DateTime start = mDateofBirth.Value;
                    DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                    int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                    //  double daysInEndMonth = (end - end.AddMonths(1)).Days;
                    double daysInEndMonth = (end.AddMonths(1) - end).Days;
                    double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                    mAge = months / 12;

                    double pensioncompletedays = 0;
                    double paidsalarydays = 0;
                    if (compMonth == OPFMaster.PensionAge * 12 && start.Month == end.Month)
                    {
                        mAge = compMonth / 12;
                        // pension age complete in Salary month then pension will applicable only till that days
                        pensioncompletedays = start.Day;
                        var salatt = db.SalAttendanceT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayrollId).FirstOrDefault();
                        paidsalarydays = salatt.PaybleDays;

                    }

                    //mAge = (DateTime.Now.Date - mDateofBirth.Value.Date).Days;
                    //mAge = mAge / 365;

                    double mCompPF = 0;
                    double mEmpPension = 0;
                    double mEmpPF = 0;
                    double mEmpVPF = 0;
                    double mTotalCompPF = 0;
                    double highpension = 0;
                    double highpensionper = 0;

                    mEmpVPF = OSalaryDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault() != null ?
                        OSalaryDetails.Where(e => e.SalaryHead.Code.ToUpper() == "VPF").SingleOrDefault().Amount : 0;


                    double mGrossWages = OSalaryDetails.Where(r => r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && r.SalaryHead.InPayslip == true)
                        .Sum(e => e.Amount);
                    mGrossWages = Math.Round(mGrossWages, 0);
                    //double mAdminWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.PFAdminWages, OSalaryDetails, null);
                    //SalHeadFormula PFFormula = Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalstruct, OSalaryHead.SalaryHead.Id);
                    //double mPFWages = Process.SalaryHeadGenProcess.Wagecalc(PFFormula, OSalaryDetails, null);
                    double mPFWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPFWages, OSalaryDetails, null);

                    var EmpPFHead = OEmpSalstruct.EmpSalStructDetails
                                    .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault();
                    if (EmpPFHead != null && EmpPFHead.SalHeadFormula_Id != null)
                    {
                        Int32 Pfformulaid = EmpPFHead.SalHeadFormula.Id;
                        var OSalHeadFormualQuery = SalaryHeadGenProcess._returnSalHeadFormula(Pfformulaid);
                        mPFWages = SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, null);
                        mPFWages = SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPFWages);
                    }

                    mPFWages = Math.Round(mPFWages, 0);
                    double mPensionWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.EPSWages, OSalaryDetails, null);

                    var EmpPENSIONHead = OEmpSalstruct.EmpSalStructDetails
                                .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault();
                    if (EmpPENSIONHead != null && EmpPENSIONHead.SalHeadFormula_Id != null)
                    {
                        Int32 Pensionformulaid = EmpPENSIONHead.SalHeadFormula.Id;
                        var OSalHeadFormualQuery = SalaryHeadGenProcess._returnSalHeadFormula(Pensionformulaid);
                        mPensionWages = SalaryHeadGenProcess.Wagecalc(OSalHeadFormualQuery, OSalaryDetails, null);
                        mPensionWages = SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mPensionWages);
                    }
                    mPensionWages = Math.Round(mPensionWages, 0);
                    double mEDLIWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPFMaster.PFEDLIWages, OSalaryDetails, null);
                    mEDLIWages = Math.Round(mEDLIWages, 0);
                    //check database
                    //mEmpPF = Process.SalaryHeadGenProcess.SalHeadAmountCalc(PFFormula, OSalaryDetails, null, OEmployeePayroll);
                    mEmpPF = Math.Round(mPFWages * OPFMaster.EmpPFPerc / 100, 0);
                    //mTotalCompPF= mPFWages * (mPFMaster.CPFPerc + mPFMaster.EPSPerc); //applay rounding
                    mEmpPF = Math.Round(mEmpPF, 0);

                    // pension age complete in Salary month then pension will applicable only till that days
                    if (pensioncompletedays != 0)
                    {
                        if (paidsalarydays < pensioncompletedays)
                        {
                            pensioncompletedays = paidsalarydays;
                        }
                        double OEPSWages = 0;
                        if (OSalaryDetails != null)
                        {
                            OEPSWages = OPFMaster.EPSWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Percentage / 100 * e.uir.Amount).Sum();

                            OEPSWages = OEPSWages + OPFMaster.EPSWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Amount).Sum();
                        }
                        //
                        double OEDLIWages = 0;
                        if (OSalaryDetails != null)
                        {
                            OEDLIWages = OPFMaster.PFEDLIWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Percentage / 100 * e.uir.Amount).Sum();

                            OEDLIWages = OEDLIWages + OPFMaster.PFEDLIWages.RateMaster
                                .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                        (u, uir) => new { u, uir })
                                .Select(e => e.u.Amount).Sum();
                        }


                        OEPSWages = (OEPSWages / paidsalarydays) * pensioncompletedays;
                        OEDLIWages = (OEDLIWages / paidsalarydays) * pensioncompletedays;
                        OEPSWages = Math.Round(OEPSWages, 0);
                        OEDLIWages = Math.Round(OEDLIWages, 0);

                        if (OEPSWages < mPensionWages)
                        {
                            mPensionWages = OEPSWages;
                        }
                        if (OEDLIWages < mEDLIWages)
                        {
                            mEDLIWages = OEDLIWages;
                        }

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

                    if (OEmployeePayroll.Employee.EmpOffInfo.PensionAppl == false)
                    {
                        mEmpPension = 0;
                        mPensionWages = 0;
                    }

                    mCompPF = mEmpPF - mEmpPension;

                    if (mCompPF > OPFMaster.CompPFCeiling)
                    {
                        mCompPF = OPFMaster.CompPFCeiling;
                    }

                    // Surat Dcc comppf amount celling

                    var OSalaryHeadcomppf = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "CPF")
                .SingleOrDefault();
                    if (OSalaryHeadcomppf != null)
                    {
                        if (OSalaryHeadcomppf.SalHeadFormula != null)
                        {
                            if ((mCompPF + mEmpPension) > OSalaryHeadcomppf.Amount)
                            {
                                mCompPF = OSalaryHeadcomppf.Amount - mEmpPension;
                            }

                        }
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                    int Comp_Id = Convert.ToInt32(HttpContext.Current.Session["CompId"].ToString());
                    PFECRR OPFECRR = new PFECRR()
                    {
                        //PFCalendar = db.Calendar.Find(cal.Id),
                        UAN = UANNo,// OEmpOff.NationalityID==null?null: OEmpOff.NationalityID.UANNo,
                        UAN_Name = OEmployeePayroll.Employee.EmpName.FullNameFML,
                        Establishment_ID = OPFMaster.EstablishmentID,
                        Establishment_Name = db.Company.Where(e => e.Id == Comp_Id).Select(r => r.Name).SingleOrDefault(),

                        Gross_Wages = mGrossWages,
                        EPF_Wages = mPFWages,
                        EPS_Wages = mPensionWages,
                        EDLI_Wages = mEDLIWages,
                        EE_Share = (mEmpPF),
                        EPS_Share = (mEmpPension),
                        ER_Share = (mCompPF),
                        EE_VPF_Share = mEmpVPF,
                        NCP_Days = OLWPAttend.LWPDays,
                        Refund_of_Advances = 0,
                        //part of arrears file newly added on 25032017
                        Arrear_EPF_Wages = ArrEPFWages,
                        Arrear_EPS_Wages = ArrEPSWages,
                        Arrear_EDLI_Wages = ArrEDLIWages,
                        Arrear_EE_Share = ArrEmpPF + ArrEmpVPF,
                        Arrear_EPS_Share = ArrEmpEPS,
                        Arrear_ER_Share = ArrCompPF,


                        //required at the time of uan
                        Father_Husband_Name = OEmployeePayroll.Employee.FatherName == null ? "" : OEmployeePayroll.Employee.FatherName.FullNameFML,//.Employee.MaritalStatus.LookupVal.ToUpper() == "MARRIED" ? OEmployeePayroll.Employee.HusbandName.ToString() : OEmployeePayroll.Employee.FatherName.FullNameFML,
                        Relationship_with_the_Member = "F",
                        Date_of_Joining_EPF = OEmployeePayroll.Employee.ServiceBookDates.PFJoingDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PFJoingDate,
                        Date_of_Joining_EPS = OEmployeePayroll.Employee.ServiceBookDates.PensionJoingDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PensionJoingDate,
                        Date_of_Exit_from_EPF = OEmployeePayroll.Employee.ServiceBookDates.PFExitDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PFExitDate,
                        Date_of_Exit_from_EPS = OEmployeePayroll.Employee.ServiceBookDates.PensionExitDate == null ? null : OEmployeePayroll.Employee.ServiceBookDates.PensionExitDate,
                        Reason_for_leaving = "",
                        //calculate at the time of ECR generation
                        //Administrative_Charges_AC2= Math.Round( mAdminWages * OPFMaster.AdminCharges/100,0),
                        //Inspection_Charges_AC2 = Math.Round(mAdminWages * OPFMaster.InspCharges / 100, 0),
                        //EDLI_Contribution_AC21 = Math.Round(mEDLIWages * OPFMaster.EDLICharges / 100, 0),
                        //Administrative_Charges_AC22 = Math.Round( mEDLIWages * OPFMaster.EDLICharges/100,0),
                        //Inspection_Charges_AC22 = Math.Round( mEDLIWages * OPFMaster.PaymentEDLIS/100,0),
                        //Exemption_Status = OPFMaster.PFTrustType.LookupVal,

                        Date_of_Birth = OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value,
                        Gender = OEmployeePayroll.Employee.Gender == null ? "" : OEmployeePayroll.Employee.Gender.LookupVal.ToUpper(),
                        Return_Month = "",
                        Wage_Month = PayMonth,

                        DBTrack = dbt
                    };

                    return OPFECRR;

                }


                return null;
            }
        }
        //PT Calculation Process
        public static PTaxTransT PTcalc(List<PTaxMaster> OCompanyPTaxMaster, EmpSalStruct OEmpSalstruct, int OEmployeePayrollId, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails, double PayableDays)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmpSalStruct OState = db.EmpSalStruct
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.Address)
                    .Include(e => e.GeoStruct.Location.Address.State)
                    .Where(e => e.Id == OEmpSalstruct.Id)
                    .SingleOrDefault();

                var mState = OState.GeoStruct.Location == null ? null : OState.GeoStruct.Location.Address == null ? null : OState.GeoStruct.Location.Address.State == null ? null : OState.GeoStruct.Location.Address.State;
                if (mState == null)
                {
                    return null;
                }


                PTaxMaster mPTMaster1 = OCompanyPTaxMaster
                .Where(e => e.EffectiveDate < Convert.ToDateTime("01/" + PayMonth).AddMonths(1).Date
                    && (e.EndDate >= Convert.ToDateTime("01/" + PayMonth).Date || e.EndDate == null) && e.States.Id == mState.Id
                    ).SingleOrDefault();
                if (mPTMaster1 == null)
                {

                    return null;
                }
                PTaxMaster OPTaxMaster = db.PTaxMaster
                                .Include(e => e.PTWagesMaster)
                                .Include(e => e.PTWagesMaster.RateMaster)
                                 .Include(e => e.PTWagesMaster.RateMaster.Select(t => t.SalHead))
                                .Include(e => e.PTStatutoryEffectiveMonths)
                                .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.StatutoryWageRange))
                                .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.EffectiveMonth))
                                .Include(e => e.PTStatutoryEffectiveMonths.Select(f => f.Gender))
                                .Include(e => e.States)
                                .Include(e => e.Frequency)
                                .Where(e => e.Id == mPTMaster1.Id)
                                .SingleOrDefault();

                double mPTAmount = 0;
                var mMonthPay = Convert.ToDateTime("01/" + PayMonth).ToString("MMMMM").ToUpper();
                string Gender = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.Gender).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault().Employee.Gender.LookupVal;
                var mEffectiveMonthObj = OPTaxMaster.PTStatutoryEffectiveMonths.Where(d => d.EffectiveMonth.LookupVal.ToUpper() == mMonthPay && d.Gender.LookupVal.ToUpper() == Gender).SingleOrDefault();
                double mPTWages = Process.SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, OSalaryDetails, null);

                if (OPTaxMaster == null)
                {
                    //ptax master not exist
                    return null;
                }
                else
                {//PT logic

                    //EmployeePayroll ArrearDataT_temp1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId && d.SalaryArrearT.Any(e => e.PayMonth == PayMonth))
                    //       .Include(e => e.SalaryArrearT)
                    //       .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT))
                    //       .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT.Select(z => z.SalaryHead)))
                    //       .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT.Select(z => z.SalaryHead.Type)))
                    //       .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT.Select(z => z.SalaryHead.SalHeadOperationType)))
                    //       .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPFT))
                    //       .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                    //*
                    List<SalaryArrearT> SalaryArrearT = new List<SalaryArrearT>();
                    SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth).ToList();
                    foreach (var j in SalaryArrearT)
                    {
                        var SalaryArrearPFTobj = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(x => x.SalaryArrearPFT).FirstOrDefault();
                        var SalaryArrearPFT = db.SalaryArrearPFT.Where(e => e.Id == SalaryArrearPFTobj.Id).FirstOrDefault();
                        List<SalaryArrearPaymentT> SalaryArrearPaymentT = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(r => r.SalaryArrearPaymentT.ToList()).FirstOrDefault();
                        foreach (var i in SalaryArrearPaymentT)
                        {
                            var SalaryArrearPaymentTObj = db.SalaryArrearPaymentT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                            var SalaryHeadarrrear = db.SalaryHead.Where(e => e.Id == SalaryArrearPaymentTObj.Id).FirstOrDefault();
                            var SalHeadType = db.SalaryHead.Where(e => e.Id == SalaryArrearPaymentTObj.Id).Select(r => r.Type).FirstOrDefault();
                            var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == SalaryArrearPaymentTObj.Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                            i.SalaryHead = SalaryHeadarrrear;
                            i.SalaryHead.Type = SalHeadType;
                            i.SalaryHead.SalHeadOperationType = SalHeadOperationType;

                        }
                        j.SalaryArrearPaymentT = SalaryArrearPaymentT;
                        j.SalaryArrearPFT = SalaryArrearPFT;

                    }



                    double ArrearPTAmount = 0;
                    //  List<SalaryArrearT> ArrearDataT = ArrearDataT_temp != null ? ArrearDataT_temp.SalaryArrearT.Where(e => e.PayMonth == PayMonth).ToList() : null;
                    List<SalaryArrearT> ArrearDataT = SalaryArrearT != null ? SalaryArrearT.Where(e => e.PayMonth == PayMonth).ToList() : null;
                    if (ArrearDataT != null && ArrearDataT.Count() > 0)
                    {
                        foreach (SalaryArrearT ca in ArrearDataT)
                        {
                            var EmpArrEarnHead = ca.SalaryArrearPaymentT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                                && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").ToList();
                            if (EmpArrEarnHead != null && EmpArrEarnHead.Count > 0 && ca.IsPaySlip == true)
                            {
                                ArrearPTAmount = ArrearPTAmount + EmpArrEarnHead.Sum(r => r.SalHeadAmount);
                            }
                        }
                    }
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
                        DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        PTaxTransT OPTTransT = new PTaxTransT()
                        {
                            ArrearPTAmount = ArrearPTAmount,
                            PTAmount = mPTAmount,
                            PTWages = mPTWages,
                            State = mState.Name,
                            DBTrack = dbt

                        };
                        return OPTTransT;
                    }
                    else if (OPTaxMaster.Frequency.LookupVal.ToUpper() == "HALFYEARLY")//Halfyearly ptax calculation
                    {
                        if (mEffectiveMonthObj != null)
                        {
                            //calculate Last five month wages Comment*****
                            for (DateTime mdate = Convert.ToDateTime("01/" + PayMonth).AddMonths(-5).Date; mdate < Convert.ToDateTime("01/" + PayMonth); mdate = mdate.AddMonths(1))
                            {
                                //EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.SalaryT)
                                //    .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                                //    .Where(e => e.Id == OEmployeePayrollId).SingleOrDefault();
                                var PayMonthChk = mdate.ToString("MM/yyyy");
                                var OSalaryTForPT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonthChk)
                                   .Select(e => e.Id)
                                   .SingleOrDefault();

                                //  SalaryT OSalaryTForPT = OEmpPayroll.SalaryT.Where(e => e.PayMonth == mdate.ToString("MM/yyyy")).SingleOrDefault();

                                if (OSalaryTForPT != null && OSalaryTForPT > 0)
                                {
                                    // var OSalaryTForPTDetails = OSalaryTForPT.SalEarnDedT.ToList();
                                    var OSalaryTForPTDetails = db.SalEarnDedT.Include(e => e.SalaryHead).Where(e => e.SalaryT_Id == OSalaryTForPT).ToList();
                                    mPTWages = mPTWages + Process.SalaryHeadGenProcess.WagecalcDirect(OPTaxMaster.PTWagesMaster, OSalaryTForPTDetails, null);
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
                        if (PayableDays == 0)
                        {
                            mPTAmount = 0;
                            mPTWages = 0;
                        }
                        DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        PTaxTransT OPTTransT = new PTaxTransT()
                        {
                            ArrearPTAmount = ArrearPTAmount,
                            PTAmount = mPTAmount,
                            PTWages = mPTWages,
                            State = mState.Name,
                            DBTrack = dbt

                        };
                        return OPTTransT;
                    }

                }
                return null;
            }
        }
        //LWF Calculation Process
        public static LWFTransT LWFcalc(List<LWFMaster> OCompanyLWFMaster, EmpSalStruct OEmpSalstruct, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmpSalStruct OState = db.EmpSalStruct
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.Address)
                    .Include(e => e.GeoStruct.Location.Address.State)
                    .Where(e => e.Id == OEmpSalstruct.Id)
                    .SingleOrDefault();

                //var mState = OState.GeoStruct.Location.Address.State;
                State mState = OState.GeoStruct.Location == null ? null : OState.GeoStruct.Location.Address == null ? null : OState.GeoStruct.Location.Address.State == null ? null : OState.GeoStruct.Location.Address.State;
                if (mState == null)
                {
                    return null;
                }
                LWFMaster mLWFMaster1 = OCompanyLWFMaster
                .Where(e => e.EffectiveDate <= Convert.ToDateTime("01/" + PayMonth).Date
                    && (e.EndDate >= Convert.ToDateTime("01/" + PayMonth).Date || e.EndDate == null) && e.LWFStates.Code == mState.Code
                    ).SingleOrDefault();
                if (mLWFMaster1 == null)
                {
                    return null;
                }
                LWFMaster OLWFMaster = db.LWFMaster
                                        .Include(e => e.WagesMaster)
                                        .Include(e => e.WagesMaster.RateMaster)
                                           .Include(e => e.WagesMaster.RateMaster.Select(v => v.SalHead))
                                        .Include(e => e.LWFStatutoryEffectiveMonths.Select(f => f.StatutoryWageRange))
                                        .Include(e => e.LWFStatutoryEffectiveMonths.Select(f => f.EffectiveMonth))
                                        .Include(e => e.LWFStates)
                                        .Where(e => e.Id == mLWFMaster1.Id)
                                        .SingleOrDefault();

                EmpSalStructDetails OSalaryHead = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LWF")
                   .SingleOrDefault();



                double mLWFEmpAmount = 0;
                double mLWFCompAmount = 0;
                var mMonthPay = Convert.ToDateTime("01/" + PayMonth).ToString("MMMM").ToUpper();
                var mEffectiveMonthObj = OLWFMaster.LWFStatutoryEffectiveMonths.Where(d => d.EffectiveMonth.LookupVal.ToUpper() == mMonthPay).SingleOrDefault();
                double mLWFWages = Process.SalaryHeadGenProcess.WagecalcDirect(OLWFMaster.WagesMaster, OSalaryDetails, null);

                if (OLWFMaster == null)
                {
                    //lwf master not exist

                }
                else
                {//LWF logic
                    if (mEffectiveMonthObj != null)
                    {
                        foreach (Range ca in mEffectiveMonthObj.StatutoryWageRange)//check tomorrow
                        {
                            if (mLWFWages >= ca.RangeFrom && mLWFWages <= ca.RangeTo)
                            {
                                if (ca.EmpShareAmount != null && ca.EmpShareAmount != 0 && ca.EmpShareAmount != null && ca.EmpShareAmount != 0)
                                {
                                    mLWFEmpAmount = ca.EmpShareAmount;
                                    mLWFCompAmount = ca.CompShareAmount;
                                    break;
                                }
                                if (ca.EmpSharePercentage != null || ca.EmpSharePercentage != 0 && ca.CompSharePercentage != null || ca.CompSharePercentage != 0)
                                {
                                    mLWFEmpAmount = mLWFWages * ca.EmpSharePercentage;
                                    mLWFCompAmount = mLWFWages * ca.CompSharePercentage;
                                    break;
                                }

                            }
                        }
                    }
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    LWFTransT OLWFTransT = new LWFTransT()
                    {
                        CompAmt = mLWFCompAmount,
                        EmpAmt = mLWFEmpAmount,
                        LWFWages = mLWFWages,
                        DBTrack = dbt
                    };
                    return OLWFTransT;
                }
                return null;
            }
        }
        //ESIC Calculation Process
        public static string ESICQualify(List<ESICMaster> OCompanyESISMaster, EmpSalStruct OEmpSalstruct, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmpSalStruct OState = db.EmpSalStruct
                   .Include(e => e.GeoStruct)
                   .Include(e => e.GeoStruct.Location)
                    //.Include(e => e.GeoStruct.Location.LocationObj)
                   .Where(e => e.Id == OEmpSalstruct.Id)
                   .SingleOrDefault();

                Location mLocation = OState.GeoStruct.Location == null ? null : OState.GeoStruct.Location;
                if (mLocation == null)
                {
                    return "NOACTION";
                }
                List<ESICMaster> mESICMaster2 = OCompanyESISMaster
                .Where(e => e.EffectiveDate <= Convert.ToDateTime("01/" + PayMonth).Date
                    && (e.EndDate >= Convert.ToDateTime("01/" + PayMonth).Date || e.EndDate == null)
                    ).ToList();
                ESICMaster mESICMaster1 = null;

                foreach (ESICMaster ca in mESICMaster2)
                {
                    if (ca.Location != null)
                    {
                        foreach (Location ca1 in ca.Location)
                        {
                            if (ca1.Id == mLocation.Id)
                            {
                                mESICMaster1 = ca;
                                break;
                            }
                        }
                    }
                }

                if (mESICMaster1 == null)
                {
                    return "NOACTION";
                }

                ESICMaster OESICMaster = db.ESICMaster

                                        .Include(e => e.WageMasterQualify)

                                        .Include(e => e.WageMasterQualify.RateMaster)
                                         .Include(e => e.WageMasterQualify.RateMaster.Select(x => x.SalHead))
                                        .Include(e => e.ESICStatutoryEffectiveMonths.Select(f => f.StatutoryWageRange))
                                        .Include(e => e.ESICStatutoryEffectiveMonths.Select(f => f.EffectiveMonth))
                                        .Include(e => e.Location)
                                        .Where(e => e.Id == mESICMaster1.Id)
                                        .SingleOrDefault();

                string mMonthPay = Convert.ToDateTime("01/" + PayMonth).ToString("MMMM").ToUpper();

                StatutoryEffectiveMonths mEffectiveMonthObj = OESICMaster.ESICStatutoryEffectiveMonths.Where(d => d.EffectiveMonth.LookupVal.ToUpper() == mMonthPay).SingleOrDefault();

                List<EmpSalStructDetails> OEmpSalDetails = OEmpSalstruct.EmpSalStructDetails.ToList();
                double mESICQualifyWages = Process.SalaryHeadGenProcess.WagecalcDirect(OESICMaster.WageMasterQualify, null, OEmpSalDetails);

                if (OESICMaster == null)
                {
                    //esic master not defined for current period
                    return "NOACTION";
                    //ESIC not 

                }
                else
                {//LWF logic
                    //double mPTWages=WagecalcDirect(mPTMaster.PTWagesMaster,OSalaryDetails,null);
                    if (mEffectiveMonthObj != null)
                    {
                        //check for qualify 
                        foreach (Range ca in mEffectiveMonthObj.StatutoryWageRange)
                        {
                            if (mESICQualifyWages >= ca.RangeFrom && mESICQualifyWages <= ca.RangeTo)
                            {
                                //applay ESIC applicable status in Empoff record
                                return "YES";
                            }

                        }
                        return "NO";
                    }
                    else
                    {
                        return "NOACTION";
                    }
                }
            }
        }
        public static ESICTransT ESICcalc(List<ESICMaster> OCompanyESISMaster, EmpSalStruct OEmpSalstruct, int OEmployeePayrollId, Calendar cal, string PayMonth, List<SalEarnDedT> OSalaryDetails)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmpSalStruct OState = db.EmpSalStruct
                   .Include(e => e.GeoStruct)
                   .Include(e => e.GeoStruct.Location)
                   .Include(e => e.GeoStruct.Location.LocationObj)

                   .Where(e => e.Id == OEmpSalstruct.Id)
                   .SingleOrDefault();
                var OAttendance1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId)
                .Select(r => r.SalAttendance.Where(e => e.PayMonth == PayMonth))
                .FirstOrDefault();
                SalAttendanceT OAttendance = OAttendance1.FirstOrDefault();
                //var mLocation = OState.GeoStruct.Location.LocationObj;
                LocationObj mLocation = OState.GeoStruct.Location == null ? null : OState.GeoStruct.Location.LocationObj;
                if (mLocation == null)
                {
                    return null;
                }
                ESICMaster mESICMaster1 = OCompanyESISMaster
                    .Where(e => e.EffectiveDate <= Convert.ToDateTime("01/" + PayMonth).Date
                    && (e.EndDate >= Convert.ToDateTime("01/" + PayMonth).Date || e.EndDate == null))
                    .SingleOrDefault();

                if (mESICMaster1 != null)
                {

                    ESICMaster OESICMaster = db.ESICMaster
                                            .Include(e => e.WageMasterPay)
                                            .Include(e => e.WageMasterQualify)
                                            .Include(e => e.WageMasterPay.RateMaster)
                                             .Include(e => e.WageMasterPay.RateMaster.Select(x => x.SalHead))
                                            .Include(e => e.WageMasterQualify.RateMaster)
                                            .Include(e => e.WageMasterQualify.RateMaster.Select(y => y.SalHead))
                                            .Include(e => e.ESICStatutoryEffectiveMonths.Select(f => f.StatutoryWageRange))
                                            .Include(e => e.ESICStatutoryEffectiveMonths.Select(f => f.EffectiveMonth))
                                            .Include(e => e.Location)

                                            //.Include(e => e.DBTrack)

                                            .Where(e => e.Id == mESICMaster1.Id)
                                            .SingleOrDefault();


                    EmpSalStructDetails OSalaryHead = OEmpSalstruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ESIC")
                       .SingleOrDefault();


                    double mESICEmpAmount = 0;
                    double mESICCompAmount = 0;

                    StatutoryEffectiveMonths mEffectiveMonthObj = OESICMaster.ESICStatutoryEffectiveMonths.LastOrDefault();

                    List<EmpSalStructDetails> OEmpSalDetails = OEmpSalstruct.EmpSalStructDetails.ToList();
                    double mESICQualifyWages = Process.SalaryHeadGenProcess.WagecalcDirect(OESICMaster.WageMasterQualify, null, OEmpSalDetails);
                    double mESICPayWages = Process.SalaryHeadGenProcess.WagecalcDirect(OESICMaster.WageMasterPay, OSalaryDetails, null);

                    if (OESICMaster == null)
                    {
                        //lwf master not exist

                    }
                    else
                    {//LWF logic
                        //double mPTWages=WagecalcDirect(mPTMaster.PTWagesMaster,OSalaryDetails,null);
                        if (mEffectiveMonthObj != null)
                        {
                            //check for qualify 
                            foreach (Range ca in mEffectiveMonthObj.StatutoryWageRange)
                            {
                                if (mESICPayWages >= ca.RangeFrom && mESICPayWages <= ca.RangeTo)
                                {

                                    if (ca.EmpShareAmount != null && ca.EmpShareAmount != 0 && ca.EmpShareAmount != null && ca.EmpShareAmount != 0)
                                    {
                                        mESICEmpAmount = ca.EmpShareAmount;
                                        mESICCompAmount = ca.CompShareAmount;
                                        break;
                                    }
                                    if (ca.EmpSharePercentage != null || ca.EmpSharePercentage != 0)
                                    {
                                        mESICEmpAmount = mESICPayWages * ca.EmpSharePercentage / 100;
                                        mESICCompAmount = mESICPayWages * ca.CompSharePercentage / 100;

                                        mESICEmpAmount = Process.SalaryHeadGenProcess.RoundingFunction(OSalaryHead.SalaryHead, mESICEmpAmount);
                                        mESICCompAmount = Process.SalaryHeadGenProcess.RoundingFunction(OSalaryHead.SalaryHead, mESICCompAmount);
                                        break;
                                    }
                                }
                            }
                            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                            ESICTransT OESICTransT = new ESICTransT()
                            {
                                DayPresent = OAttendance.PaybleDays,//from attendance
                                CompAmt = mESICCompAmount,
                                EmpAmt = mESICEmpAmount,
                                ESICWagesPay = mESICPayWages,
                                ESICQualify = mESICQualifyWages,
                                DBTrack = dbt
                            };
                            return OESICTransT;
                        }
                        return null;
                    }
                }
                return null;
            }
        }
        // total calculation Process
        public static double TotalOnPaidSalary(List<SalEarnDedT> OSalaryDetails, string OTotType)
        {
            double mTotal = 0;
            switch (OTotType.ToUpper())
            {
                case "GROSS":
                    mTotal = OSalaryDetails
                    .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                    .Sum(e => e.Amount);
                    break;
                case "DEDUCTION":
                    mTotal = OSalaryDetails
                    .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION")
                    .Sum(e => e.Amount);
                    break;
                case "ITWAGES":
                    mTotal = OSalaryDetails
                   .Where(e => e.SalaryHead.InITax == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                   .Sum(e => e.Amount);
                    break;
                default:
                    break;
            }
            return mTotal;
        }
        public static void FinalSalaryUpdate(CompanyPayroll OCompanyPayroll, SalaryHead OSalaryHead, EmpSalStruct OEmpSalstruct, Calendar cal, string PayMonth, SalaryT OSalaryT, List<SalEarnDedT> OSalaryDetails, EmpSalStructDetails OEmpSalstructDetails, int mEmployeeID)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Utility.DumpProcessStatus(LineNo: 969);


                OSalaryT.ChequeNo = null;//key cheque no
                OSalaryT.TotalEarning = Math.Round((TotalOnPaidSalary(OSalaryDetails, "GROSS")), 2);
                OSalaryT.TotalDeduction = Math.Round((TotalOnPaidSalary(OSalaryDetails, "DEDUCTION")), 2);
                OSalaryT.TotalNet = Math.Round((OSalaryT.TotalEarning - OSalaryT.TotalDeduction), 2);
                OSalaryT.TotalPFNet = 0;
                OSalaryT.TotalBonus = 0;// call bonus formula WagecalcDirect;
                OSalaryT.TotalGratuity = 0;
                OSalaryT.TotalITWages = TotalOnPaidSalary(OSalaryDetails, "ITWAGES");
                OSalaryT.WeeklyOFFDays = 0;
                OSalaryT.IsHold = false;
                OSalaryT.HoldDate = null;
                OSalaryT.ReleaseDate = null;
                OSalaryT.PayMonth = PayMonth;
                Utility.DumpProcessStatus(LineNo: 985);

            }
        }
        public static double LvChk(SalaryHead SalHead, PayProcessGroup PayGrp, string PayMonth, int OEmployeePayrollId, EmpSalStruct OEmpSalStruct)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //995
                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.Employee).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                var PayrollPeriod = PayGrp.PayrollPeriod.FirstOrDefault();
                // DateTime FromDt = Convert.ToDateTime(PayrollPeriod.PayDate + "/" + PayMonth).AddMonths(-1);
                // DateTime ToDt = Convert.ToDateTime(PayrollPeriod.PayDate + "/" + PayMonth).AddDays(-1);

                // pay month concept as attendance upload start
                var Id = Convert.ToInt32(SessionManager.CompanyId);
                string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                int startday = PayrollPeriod.StartDate;
                int endday = PayrollPeriod.EndDate;
                DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                //int daysInEndMonth = (end - end.AddMonths(1)).Days
                int daysInEndMonth = end.Day;
                int daysInstartMonth = 1;
                DateTime FromPeriod;
                DateTime EndPeriod;
                DateTime Currentmonthstart;
                DateTime CurrentmonthEnd;
                DateTime Prevmonthstart;
                DateTime PrevmonthEnd;
                int ProDays = 0;
                int RetProDays = 0;
                int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                if (endday > daym)
                {
                    endday = daym;
                }
                ProDays = daym - endday;
                RetProDays = ProDays;
                if (startday == daysInstartMonth && endday == daysInEndMonth)
                {
                    FromPeriod = _PayMonth;
                    EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                }
                else
                {
                    DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                    startday = endday + 1;
                    string pmonth = prvmonth.ToString("MM/yyyy");
                    FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                    EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                }
                CurrentmonthEnd = EndPeriod;
                Prevmonthstart = FromPeriod;
                PrevmonthEnd = FromPeriod.AddDays(ProDays);

                DateTime FromDt = Prevmonthstart;
                DateTime ToDt = CurrentmonthEnd;

                //************* MultilStructure/RegularStructure Leave take as EffectiveDate As discussed with Sir ***************
                if (OEmpSalStruct.EndDate == null)
                {
                    FromDt = OEmpSalStruct.EffectiveDate.Value.Date;
                    ToDt = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date);

                }
                else
                {
                    FromDt = OEmpSalStruct.EffectiveDate.Value.Date;
                    ToDt = OEmpSalStruct.EndDate.Value.Date;


                }
                // pay month concept as attendance upload End


                double LvDays = 0;
                if (SalHead.OnLeave == true && SalHead.LeaveDependPolicy != null && SalHead.LeaveDependPolicy.Count() > 0)
                {
                    var EmpLv = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        .Include(e => e.LvNewReq.Select(c => c.WFStatus))
                          .Include(e => e.LvNewReq.Select(c => c.FromStat))
                          .Include(e => e.LvNewReq.Select(c => c.ToStat))
                        .Where(e => e.Employee.Id == OEmployeePayroll.Employee.Id).SingleOrDefault();
                    foreach (var a in SalHead.LeaveDependPolicy)
                    {
                        var LvCalendarFilter = EmpLv.LvNewReq.OrderBy(e => e.Id).ToList();

                        var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();

                        var LvReq = EmpLv.LvNewReq.Where(e => e.LeaveHead.Id == a.LvHead.Id && !LvOrignal_id.Contains(e.Id) && (e.FromDate >= FromDt || e.FromDate <= ToDt) && e.IsCancel == false && e.TrReject == false && e.TrClosed == true && e.WFStatus.LookupVal != "2").ToList();

                        if (a.IsAccumulated == true)
                        {
                            foreach (Leave.LvNewReq c in LvReq)
                            {
                                double LvDayscal = 0;
                                if (c.FromDate == FromDt && c.ToDate == ToDt)
                                {
                                    LvDayscal = (ToDt.Date - FromDt.Date).Days + 1;
                                }
                                if (c.FromDate <= FromDt && c.ToDate >= ToDt)
                                {
                                    LvDayscal = (ToDt.Date - FromDt.Date).Days + 1;
                                }
                                if (c.FromDate <= FromDt && c.ToDate <= ToDt)
                                {
                                    LvDayscal = (c.ToDate.Value.Date - FromDt.Date).Days + 1;
                                }
                                if (c.FromDate >= FromDt && c.ToDate >= ToDt)
                                {
                                    LvDayscal = (ToDt.Date - c.FromDate.Value.Date).Days + 1;
                                }
                                if (c.FromDate >= FromDt && c.ToDate <= ToDt)
                                {
                                    LvDayscal = (c.ToDate.Value.Date - c.FromDate.Value.Date).Days + 1;
                                }


                                if (c.FromDate == FromDt && c.FromStat.LookupVal != "FULLSESSION")
                                {
                                    LvDayscal = LvDayscal - 0.5;
                                }
                                if (c.ToDate == ToDt && c.ToStat.LookupVal != "FULLSESSION")
                                {
                                    LvDayscal = LvDayscal - 0.5;
                                }

                                if (LvDayscal < 0)
                                {
                                    LvDayscal = 0;
                                }

                                if (a.LvHead.HFPay == true)
                                {
                                    // LvDays = LvDays + c.DebitDays / 2;
                                    LvDays = LvDays + LvDayscal / 2;
                                }
                                else
                                {
                                    //  LvDays = LvDays + c.DebitDays;
                                    LvDays = LvDays + LvDayscal;
                                }

                            }
                            if (LvDays >= a.AccMinLvDaysAppl && LvDays <= a.MaxDays)
                            {
                                LvDays = LvDays;
                            }
                            else
                            {
                                LvDays = 0;
                            }
                        }
                        if (a.IsContinous == true)
                        {
                            foreach (Leave.LvNewReq c in LvReq)
                            {
                                if (c.DebitDays >= a.MinLvDaysAppl && c.DebitDays <= a.MaxDays)
                                {
                                    double LvDayscal = 0;
                                    if (c.FromDate == FromDt && c.ToDate == ToDt)
                                    {
                                        LvDayscal = (ToDt.Date - FromDt.Date).Days + 1;
                                    }
                                    if (c.FromDate <= FromDt && c.ToDate >= ToDt)
                                    {
                                        LvDayscal = (ToDt.Date - FromDt.Date).Days + 1;
                                    }
                                    if (c.FromDate <= FromDt && c.ToDate <= ToDt)
                                    {
                                        LvDayscal = (c.ToDate.Value.Date - FromDt.Date).Days + 1;
                                    }
                                    if (c.FromDate >= FromDt && c.ToDate >= ToDt)
                                    {
                                        LvDayscal = (ToDt.Date - c.FromDate.Value.Date).Days + 1;
                                    }
                                    if (c.FromDate >= FromDt && c.ToDate <= ToDt)
                                    {
                                        LvDayscal = (c.ToDate.Value.Date - c.FromDate.Value.Date).Days + 1;
                                    }


                                    if (c.FromDate == FromDt && c.FromStat.LookupVal != "FULLSESSION")
                                    {
                                        LvDayscal = LvDayscal - 0.5;
                                    }
                                    if (c.ToDate == ToDt && c.ToStat.LookupVal != "FULLSESSION")
                                    {
                                        LvDayscal = LvDayscal - 0.5;
                                    }
                                    if (LvDayscal < 0)
                                    {
                                        LvDayscal = 0;
                                    }

                                    if (a.LvHead.HFPay == true)
                                    {
                                        //  LvDays = LvDays + c.DebitDays / 2;
                                        LvDays = LvDays + LvDayscal / 2;
                                    }
                                    else
                                    {
                                        // LvDays = LvDays + c.DebitDays;
                                        LvDays = LvDays + LvDayscal;
                                    }
                                }

                            }
                        }



                    }
                    return LvDays;
                }
            }
            //1017
            return 0;
        }
        public static List<SalEarnDedT> EarnSalHeadMDProcess(EmpSalStruct OEmpSalStruct, List<SalEarnDedT> OSalEarnDedT, int OEmployeePayrollId, double SalAttendanceT_PayableDays, double SalAttendanceT_monthDays, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //1024
                Utility.DumpProcessStatus(LineNo: 1024);

                List<EmpSalStructDetails> OEmpsalhead = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR"
                    || s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC"
                    || s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "VDA"
                    || s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DA"
                    //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING"
                    // || s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PFWAGESOFFICIATING"
                    //  || s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "SUPANNOFFICIATING"
                    )).OrderBy(e => e.SalaryHead.SeqNo).ToList();

                //1033
                Utility.DumpProcessStatus(LineNo: 1033);

                PayProcessGroup OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                double LvDays = 0;
                foreach (EmpSalStructDetails ca in OEmpsalhead)
                {
                    LvDays = 0;
                    if (ca.SalaryHead.OnAttend == true && ca.SalaryHead.OnLeave == true)
                    {
                        //1040
                        //Utility.DumpProcessStatus(LineNo: 1042);

                        LvDays = LvChk(ca.SalaryHead, OPayProcGrp, PayMonth, OEmployeePayrollId, OEmpSalStruct);
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                        {
                            if (LvDays > 30)
                            {
                                LvDays = 30;
                            }

                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                        {
                            if (LvDays > 30)
                            {
                                LvDays = 30;
                            }
                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                        {
                            if (LvDays > SalAttendanceT_monthDays)
                            {
                                LvDays = SalAttendanceT_monthDays;
                            }

                        }

                        //1043
                        //Utility.DumpProcessStatus(LineNo: 1046);

                        //  SalAttendanceT_PayableDays = SalAttendanceT_PayableDays - LvDays;
                    }
                    //1048
                    double CalAmount = 0;
                    if (OPayProcGrp != null)
                    {
                        //Utility.DumpProcessStatus(LineNo: 1084);

                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                        {
                            //Utility.DumpProcessStatus(LineNo: 1058);

                            if (ca.SalaryHead.OnAttend == true)
                            {
                                //Utility.DumpProcessStatus(LineNo: 1062);
                                CalAmount = (ca.Amount / 30) * (SalAttendanceT_PayableDays - LvDays);
                            }
                            else
                            {
                                //Utility.DumpProcessStatus(LineNo: 1067);
                                CalAmount = ca.Amount;
                            }

                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                        {
                            //Utility.DumpProcessStatus(LineNo: 1074);

                            if (ca.SalaryHead.OnAttend == true)
                            {
                                //Utility.DumpProcessStatus(LineNo: 1078);

                                CalAmount = (ca.Amount) - ((SalAttendanceT_monthDays - SalAttendanceT_PayableDays - LvDays) / 30) * ca.Amount;
                            }
                            else
                            {
                                //Utility.DumpProcessStatus(LineNo: 1084);

                                CalAmount = ca.Amount;
                            }

                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                        {
                            //Utility.DumpProcessStatus(LineNo: 1092);
                            if (ca.SalaryHead.OnAttend == true)
                            {
                                //Utility.DumpProcessStatus(LineNo: 1095);

                                CalAmount = ca.Amount * ((SalAttendanceT_PayableDays - LvDays) / (SalAttendanceT_monthDays));
                            }
                            else
                            {
                                //Utility.DumpProcessStatus(LineNo: 1101);
                                CalAmount = ca.Amount;
                            }
                        }
                    }
                    //Amount = ca.a
                    // kerala bank For DA
                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool exists = System.IO.Directory.Exists(requiredPath);
                    string localPath;
                    if (!exists)
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + @"\SalRule" + ".txt";
                    localPath = new Uri(path).LocalPath;
                    using (var streamReader = new StreamReader(localPath))
                    {
                        string line;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var comp = line.Split('_')[0];
                            if (comp == "KERALA")
                            {
                                //var comp = line.Split('_')[0];
                                double basic = Convert.ToDouble(line.Split('_')[1]);
                                var SalheadOperation = line.Split('_')[2];
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && ca.SalHeadFormula != null)
                                {
                                    var OEmpsalheadbasic = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                                                        && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                                                        && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC"
                                                        )).OrderBy(e => e.SalaryHead.SeqNo).FirstOrDefault();
                                    if (OEmpsalheadbasic != null)
                                    {
                                        if (OEmpsalheadbasic.Amount >= basic)
                                        {
                                            double LVDaysChk = 0;
                                            LVDaysChk = LvChk(OEmpsalheadbasic.SalaryHead, OPayProcGrp, PayMonth, OEmployeePayrollId, OEmpSalStruct);
                                            //if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                                            if (LVDaysChk != 0)
                                            {
                                                EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                                CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                                            }
                                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount); //rounding function  
                                        }
                                    }

                                }
                            }


                        }
                    }
                    // kerala bank For DA

                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && ca.SalHeadFormula != null)
                    {

                        //Utility.DumpProcessStatus(LineNo: 1110);

                        //SalHeadFormula SalHeadForm1 = db.SalHeadFormula.Where(e => e.Id == ca.SalHeadFormula.Id)
                        //                  .Include(e => e.SalWages)
                        //                  .Include(e => e.PercentDependRule)
                        //                .Include(e => e.SalWages.RateMaster)
                        //                .Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).SingleOrDefault();
                        //*
                        var SalHeadForm = new SalHeadFormula();
                        SalHeadForm = db.SalHeadFormula.Where(e => e.Id == ca.SalHeadFormula.Id).FirstOrDefault();
                        var SalWages = db.Wages.Where(e => e.Id == SalHeadForm.SalWages_Id).Include(x => x.RateMaster)
                            .Include(x => x.RateMaster.Select(z => z.SalHead)).FirstOrDefault();

                        var PercentDependRule = db.PercentDependRule.Where(e => e.Id == SalHeadForm.PercentDependRule_Id).FirstOrDefault();
                        SalHeadForm.SalWages = SalWages;
                        SalHeadForm.PercentDependRule = PercentDependRule;


                        var SalHead = SalHeadForm.SalWages.RateMaster.Select(r => r.SalHead);
                        //Utility.DumpProcessStatus(LineNo: 1115);

                        foreach (SalaryHead S in SalHead)
                        {
                            var OthEarningT = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId)
                                            .Select(s => s.OtherEarningDeductionT.Where(e => e.SalaryHead_Id == S.Id
                                             && e.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                            if (OthEarningT != null)
                            {
                                //foreach (var ca1 in OthEarningT)
                                //{
                                CalAmount = CalAmount + ((OthEarningT.SalAmount * SalHeadForm.PercentDependRule.SalPercent) / 100);
                                //Utility.DumpProcessStatus(LineNo: 1127);

                                //}
                            }

                        }
                    }
                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "OFFICIATING")
                    {
                        EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                        //  CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                        CalAmount = OfficiatingServiceBookController.officiateprocess(OEmpPayroll, PayMonth, CalAmount, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                    }
                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PFWAGESOFFICIATING")
                    {
                        EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                        //  CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                        CalAmount = OfficiatingServiceBookController.officiateprocess(OEmpPayroll, PayMonth, CalAmount, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                    }
                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "SUPANNOFFICIATING")
                    {
                        EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                        //  CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                        CalAmount = OfficiatingServiceBookController.officiateprocess(OEmpPayroll, PayMonth, CalAmount, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                    }
                    if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                    {
                        EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                        CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                    }
                    CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount); //rounding function
                    //details save query
                    if (SalAttendanceT_PayableDays == 0)
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, ca.Amount, ca.SalaryHead);

                    }
                    else
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, ca.Amount, ca.SalaryHead);
                    }
                }





                return OSalEarnDedT;
            }
        }
        //multiple structure
        public static List<SalEarnDedTMultiStructure> EarnSalHeadMDProcessMS(EmpSalStruct OEmpSalStruct, List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructure, int OEmployeePayrollId, double SalAttendanceT_PayableDays, double SalAttendanceT_monthDays, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmpsalhead = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                    && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                    && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR"
                    || s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC" ||
                    s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "VDA" ||
                    s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "DA"
                    //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFICIATING"
                    //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PFWAGESOFFICIATING"
                    //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "SUPANNOFFICIATING"
                    )).OrderBy(e => e.SalaryHead.SeqNo).ToList();
                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                Utility.DumpProcessStatus(LineNo: 1412);
                List<SalEarnDedT> OSalEarnDedT = new List<SalEarnDedT>();

                foreach (EmpSalStructDetails ca in OEmpsalhead)
                {
                    double LvDays = 0;
                    double CalAmount = 0;

                    if (OPayProcGrp != null)
                    {
                        //Utility.DumpProcessStatus(LineNo: 1420);

                        if (ca.SalaryHead.OnAttend == true && ca.SalaryHead.OnLeave == true)
                        {
                            //Utility.DumpProcessStatus(LineNo: 1424);

                            LvDays = LvChk(ca.SalaryHead, OPayProcGrp, PayMonth, OEmployeePayrollId, OEmpSalStruct);
                            // SalAttendanceT_PayableDays = SalAttendanceT_PayableDays - LvDays;
                            if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                            {
                                if (LvDays > 30)
                                {
                                    LvDays = 30;
                                }

                            }
                            if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                            {
                                if (LvDays > 30)
                                {
                                    LvDays = 30;
                                }
                            }
                            if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                            {
                                if (LvDays > SalAttendanceT_monthDays)
                                {
                                    LvDays = SalAttendanceT_monthDays;
                                }

                            }
                        }

                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                        {
                            //Utility.DumpProcessStatus(LineNo: 1432);

                            if (ca.SalaryHead.OnAttend == true)
                            {
                                //Utility.DumpProcessStatus(LineNo: 1437);

                                CalAmount = (ca.Amount / 30) * (SalAttendanceT_PayableDays - LvDays);
                            }
                            else
                            {
                                CalAmount = ca.Amount;
                            }

                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                        {
                            //Utility.DumpProcessStatus(LineNo: 1448);

                            if (ca.SalaryHead.OnAttend == true)
                            {

                                //Utility.DumpProcessStatus(LineNo: 1453);
                                CalAmount = (ca.Amount) - ((SalAttendanceT_monthDays - SalAttendanceT_PayableDays - LvDays) / 30) * ca.Amount;
                            }
                            else
                            {
                                CalAmount = ca.Amount;
                            }

                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                        {
                            //Utility.DumpProcessStatus(LineNo: 1465);

                            if (ca.SalaryHead.OnAttend == true)
                            {

                                //Utility.DumpProcessStatus(LineNo: 1469);
                                CalAmount = ca.Amount * ((SalAttendanceT_PayableDays - LvDays) / (SalAttendanceT_monthDays));
                            }
                            else
                            {
                                CalAmount = ca.Amount;
                            }
                        }
                    }

                    // kerala bank For DA
                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool exists = System.IO.Directory.Exists(requiredPath);
                    string localPath;
                    if (!exists)
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + @"\SalRule" + ".txt";
                    localPath = new Uri(path).LocalPath;
                    using (var streamReader = new StreamReader(localPath))
                    {
                        string line;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var comp = line.Split('_')[0];
                            if (comp == "KERALA")
                            {
                                //  var comp = line.Split('_')[0];
                                double basic = Convert.ToDouble(line.Split('_')[1]);
                                var SalheadOperation = line.Split('_')[2];
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && ca.SalHeadFormula != null)
                                {
                                    var OEmpsalheadbasic = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                                                        && s.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                                                        && (s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BASIC"
                                                        )).OrderBy(e => e.SalaryHead.SeqNo).FirstOrDefault();
                                    if (OEmpsalheadbasic != null)
                                    {
                                        if (OEmpsalheadbasic.Amount >= basic)
                                        {
                                            double LVDaysChk = 0;
                                            LVDaysChk = LvChk(OEmpsalheadbasic.SalaryHead, OPayProcGrp, PayMonth, OEmployeePayrollId, OEmpSalStruct);
                                            //if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                                            if (LVDaysChk != 0)
                                            {
                                                EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                                CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                                            }
                                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount); //rounding function  
                                        }
                                    }

                                }
                            }


                        }
                    }
                    // kerala bank For DA


                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "DA" && ca.SalHeadFormula != null)
                    {
                        //Utility.DumpProcessStatus(LineNo: 1481);

                        //SalHeadFormula SalHeadForm = db.SalHeadFormula.Where(e => e.Id == ca.SalHeadFormula.Id)
                        //    .Include(e => e.SalWages).Include(e => e.PercentDependRule)
                        //                .Include(e => e.SalWages.RateMaster)
                        //                .Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();

                        var SalHeadForm = new SalHeadFormula();
                        SalHeadForm = db.SalHeadFormula.Where(e => e.Id == ca.SalHeadFormula.Id).FirstOrDefault();
                        var SalWages = db.Wages.Where(e => e.Id == SalHeadForm.SalWages_Id).Include(x => x.RateMaster)
                            .Include(x => x.RateMaster.Select(z => z.SalHead)).FirstOrDefault();

                        var PercentDependRule = db.PercentDependRule.Where(e => e.Id == SalHeadForm.PercentDependRule_Id).FirstOrDefault();
                        SalHeadForm.SalWages = SalWages;
                        SalHeadForm.PercentDependRule = PercentDependRule;


                        List<SalaryHead> SalHead = SalHeadForm.SalWages.RateMaster.Select(r => r.SalHead).ToList();

                        foreach (SalaryHead S in SalHead)
                        {
                            var OthEarningT = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId)
                                            .Select(s => s.OtherEarningDeductionT.Where(e => e.SalaryHead.Id == S.Id
                                             && e.PayMonth == PayMonth).FirstOrDefault()).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                            //Utility.DumpProcessStatus(LineNo: 1492);

                            if (OthEarningT != null)
                            {
                                //foreach (var ca1 in OthEarningT)
                                //{
                                CalAmount = CalAmount + ((OthEarningT.SalAmount * SalHeadForm.PercentDependRule.SalPercent) / 100);
                                //}
                            }
                        }
                    }
                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "OFFICIATING")
                    {
                        CalAmount = 0;//Multistructure 0 value pass beacuse on multistructure group by code i have called
                    }
                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PFWAGESOFFICIATING")
                    {
                        CalAmount = 0;//Multistructure 0 value pass beacuse on multistructure group by code i have called
                    }
                    if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "SUPANNOFFICIATING")
                    {
                        CalAmount = 0;//Multistructure 0 value pass beacuse on multistructure group by code i have called
                    }
                    if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                    {
                        EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                        CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                    }
                    if (SalAttendanceT_PayableDays == 0)
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, ca.Amount, ca.SalaryHead);

                    }
                    else
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, ca.Amount, ca.SalaryHead);
                    }
                    // CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount); //rounding function
                    //details save query
                    OSalEarnDedTMultiStructure = SaveSalayDetailsMultiStructure(OSalEarnDedTMultiStructure, CalAmount, ca.Amount, ca.SalaryHead);

                }

                //change the process as per new concept of from & todate 
                //Daily/hourly earnings


                return OSalEarnDedTMultiStructure;

            }
        }
        public static double FunctionalwdayCellingCkeck(FunctionalAllowancePolicy OFunctionpolicy, double mSalHeadAmount)
        {

            if (OFunctionpolicy.MinDays != null)
            {
                if (mSalHeadAmount < OFunctionpolicy.MinDays)
                {
                    mSalHeadAmount = OFunctionpolicy.MinDays;
                }
            }
            if (OFunctionpolicy.MaxDays != null)
            {
                if (mSalHeadAmount > OFunctionpolicy.MaxDays)
                {
                    mSalHeadAmount = OFunctionpolicy.MaxDays;
                }
            }
            return mSalHeadAmount;
        }
        public static int EmployeePayrollProcess(int OEmployeePayrollId, string PayMonth, bool AutoIncomeTax, int OCompanyPayrollID, string AmountChk, int ProcType)
        {
            //try
            //{
            using (DataBaseContext db = new DataBaseContext())
            {
                double SalAttendanceT_monthDays = 0;
                double SalAttendanceT_PayableDays = 0;
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\SalRule" + ".txt";
                localPath = new Uri(path).LocalPath;
                if (!File.Exists(localPath))
                {

                    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }
                //1538
                string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existss = System.IO.Directory.Exists(requiredPaths);
                string localPaths;
                if (!existss)
                {
                    localPaths = new Uri(requiredPaths).LocalPath;
                    System.IO.Directory.CreateDirectory(localPaths);
                }
                string paths = requiredPaths + @"\LoanInterfacePerkCalc" + ".ini";
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

                Utility.DumpProcessStatus("" + OEmployeePayrollId + "", 1538);

                //Utility.DumpProcessStatus(LineNo: 1562);

                var OCompanyPayroll = new CompanyPayroll();
                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID).FirstOrDefault();
                var OCompany = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID).Select(r => r.Company).FirstOrDefault();

                OCompanyPayroll.Company = OCompany;
                List<Calendar> Calendar = db.Company.Where(e => e.Id == OCompany.Id).Select(r => r.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).ToList()).FirstOrDefault();
                foreach (var i in Calendar)
                {
                    var calName = db.Calendar.Where(e => e.Id == i.Id).Select(r => r.Name).FirstOrDefault();
                    i.Name = calName;
                }

                // List<LWFMaster> LWFMaster = db.CompanyPayroll.Where(e => e.Id == OCompany.Id).Select(r => r.LWFMaster.ToList()).FirstOrDefault();
                //List<LWFMaster> LWFMaster = db.LWFMaster.Where(e => e.CompanyPayroll_Id == OCompany.Id &&
                //    e.EffectiveDate <= Convert.ToDateTime("01/" + PayMonth).Date
                //    && (e.EndDate >= Convert.ToDateTime("01/" + PayMonth).Date || e.EndDate == null)
                //    ).ToList();

                List<LWFMaster> LWFMaster = db.LWFMaster.Where(e => e.CompanyPayroll_Id == OCompanyPayroll.Id
                    && e.EndDate == null
                    ).ToList();

                foreach (var j in LWFMaster)
                {
                    var LWFStates = db.LWFMaster.Where(e => e.Id == j.Id).Select(r => r.LWFStates).FirstOrDefault();
                    var LWFStatutoryEffectiveMonths = db.LWFMaster.Where(e => e.Id == j.Id).Select(r => r.LWFStatutoryEffectiveMonths).FirstOrDefault();
                    j.LWFStates = LWFStates;
                    j.LWFStatutoryEffectiveMonths = LWFStatutoryEffectiveMonths;
                }
                var OPFMaster = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID).Select(r => r.PFMaster).FirstOrDefault();

                var OESICMaster = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID).Select(r => r.ESICMaster).FirstOrDefault();

                List<PTaxMaster> PTaxMaster = db.CompanyPayroll.Where(e => e.Id == OCompany.Id).Select(r => r.PTaxMaster.ToList()).FirstOrDefault();

                foreach (var j in PTaxMaster)
                {
                    var States = db.PTaxMaster.Where(e => e.Id == j.Id).Select(r => r.States).FirstOrDefault();
                    j.States = States;

                }
                OCompanyPayroll.Company.Calendar = Calendar;
                OCompanyPayroll.LWFMaster = LWFMaster;
                OCompanyPayroll.PTaxMaster = PTaxMaster;
                OCompanyPayroll.PFMaster = OPFMaster;
                OCompanyPayroll.ESICMaster = OESICMaster;
                //*
                //CompanyPayroll OCompanyPayroll1 = db.CompanyPayroll.Include(e => e.Company)
                //                        .Include(e => e.Company.Calendar.Select(r => r.Name))
                //                        .Include(e => e.Company.Calendar)
                //                         .Include(e => e.PFMaster)
                //                         .Include(e => e.PTaxMaster)
                //                         .Include(e => e.LWFMaster.Select(t => t.LWFStates))
                //                         .Include(e => e.LWFMaster.Select(t => t.LWFStatutoryEffectiveMonths))
                //                         .Include(e => e.ESICMaster)
                //                         .Include(e => e.PTaxMaster.Select(a => a.States))

                //                          .Where(d => d.Id == OCompanyPayrollID).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                //1576
                //Utility.DumpProcessStatus(LineNo: 1576);

                //var OEmpOff1 = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId)
                //                          .Select(e => new
                //                          {
                //                              Id = e.Employee.EmpOffInfo.Id,
                //                              PFAppl = e.Employee.EmpOffInfo.PFAppl,
                //                              LWFAppl = e.Employee.EmpOffInfo.LWFAppl,
                //                              ESICAppl = e.Employee.EmpOffInfo.ESICAppl,
                //                              PTAppl = e.Employee.EmpOffInfo.PTAppl,
                //                              Branch = e.Employee.EmpOffInfo.Branch,
                //                              AccountType = e.Employee.EmpOffInfo.AccountType,
                //                              AccountNo = e.Employee.EmpOffInfo.AccountNo,
                //                              PayMode = e.Employee.EmpOffInfo.PayMode,
                //                              UANNo = e.Employee.EmpOffInfo.NationalityID.UANNo

                //                          }).FirstOrDefault();
                var mEmpCodeOff = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId)
                                        .Select(e => e.Employee.EmpOffInfo.Id).FirstOrDefault();

                var OEmpOff = db.EmpOff.Where(r => r.Id == mEmpCodeOff)
                                          .Select(e => new
                                          {
                                              Id = e.Id,
                                              PFAppl = e.PFAppl,
                                              LWFAppl = e.LWFAppl,
                                              ESICAppl = e.ESICAppl,
                                              PTAppl = e.PTAppl,
                                              Branch = e.Branch,
                                              AccountType = e.AccountType,
                                              AccountNo = e.AccountNo,
                                              PayMode = e.PayMode,
                                              UANNo = e.NationalityID.UANNo,
                                              PFTrust_EstablishmentId = e.PFTrust_EstablishmentId
                                          }).FirstOrDefault();


                //1593
                //Utility.DumpProcessStatus(LineNo: 1593);

                ////////var SalaryDetails = db.EmployeePayroll.AsNoTracking().Where(r => r.Id == OEmployeePayrollId).OrderBy(e => e.Id).Select(t => t.SalaryT.Where(e => e.PayMonth == PayMonth)).SingleOrDefault();//.ToList();

                ////////if (SalaryDetails != null && SalaryDetails.Count() > 0)
                ////////{
                ////////    int SalId = SalaryDetails.SingleOrDefault().Id;//.FirstOrDefault().Id;
                ////////    var OSalaryTChk = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == SalId).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                ////////    if (OSalaryTChk.ReleaseDate != null)
                ////////    {
                ////////        return 7;
                ////////    }
                ////////    DeleteSalary(SalaryDetails.SingleOrDefault().Id, PayMonth);

                ////////}

                //var SalaryDetails1 = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.NegSalData))
                //  .Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Id == OEmployeePayrollId).SingleOrDefault();
                var SalaryDetails = new SalaryT();
                SalaryDetails = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth).FirstOrDefault();

                if (SalaryDetails != null)
                {
                    List<SalEarnDedT> SalEarnDedT = db.SalaryT.Where(e => e.Id == SalaryDetails.Id).Select(r => r.SalEarnDedT.ToList()).FirstOrDefault();
                    foreach (var i in SalEarnDedT)
                    {
                        var SalEarnDedTObj = db.SalEarnDedT.Where(e => e.Id == i.Id).Select(r => r.NegSalData).FirstOrDefault();
                        if (SalEarnDedTObj != null)
                        {

                            var NegSalData = db.NegSalData.Where(e => e.Id == SalEarnDedTObj.Id).FirstOrDefault();
                            i.NegSalData = NegSalData;
                        }

                    }
                    SalaryDetails.SalEarnDedT = SalEarnDedT;

                    if (SalaryDetails.ReleaseDate != null)
                    {
                        return 7;
                    }
                    if (SalaryDetails.SalEarnDedT.Where(r => r.NegSalData != null && r.NegSalData.ReleaseFlag == true).Count() > 0)
                    {
                        return 11;
                    }
                    // DeleteSalary(SalaryDetails.Id, PayMonth);
                    DeleteSalaryList(SalaryDetails.Id, PayMonth);
                }


                //if (AutoIncomeTax == true)
                //{
                //ITaxTransT ITaxDetails = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId).Include(e => e.ITaxTransT).AsParallel().OrderBy(e => e.Id).Select(t => t.ITaxTransT.Where(e => e.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();//.ToList();
                //if (ITaxDetails != null)
                //{
                //    db.ITaxTransT.Remove(ITaxDetails);
                //    db.SaveChanges();
                //}
                //}
                //1608
                Utility.DumpProcessStatus(LineNo: 1608);

                List<Int32> SalHeadId = db.SalaryHead.Select(e => e.Id).ToList();
                List<PayScaleAssignment> OPayScaleAssign = db.PayScaleAssignment.Where(e => !SalHeadId.Contains(e.SalaryHead.Id)).AsNoTracking().OrderBy(e => e.Id).ToList();
                if (OPayScaleAssign.Count() > 0)
                {
                    return 1;//Payscale assignment not done for new head
                }

                //SalAttendanceT OSalattendanceT1 = db.EmployeePayroll.Where(t => t.Id == OEmployeePayrollId).AsNoTracking().OrderBy(e => e.Id)
                //    .Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                SalAttendanceT OSalattendanceT = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth).FirstOrDefault();

                if (OSalattendanceT != null)
                {
                    SalAttendanceT_monthDays = OSalattendanceT.MonthDays;
                    //SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays;
                    SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays - OSalattendanceT.LWPDays;//LWP Leave process button on manual attendance page Goa urban bank
                }
                else
                {
                    //SalAttendanceT_monthDays=
                    SalAttendanceT_monthDays = Convert.ToDouble(DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0])));
                    SalAttendanceT_PayableDays = 0;//Changed by Rohit
                }


                //1629
                //Utility.DumpProcessStatus(LineNo: 1629);

                //EmployeePayroll EmpSalStructList = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                //                            .Include(e => e.EmpSalStruct)
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead)))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalHeadFormula)))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency)))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.Type)))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod)))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType)))
                //                            .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead)))
                //    .SingleOrDefault();


                ////////IEnumerable<EmpSalStruct> bb = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).SelectMany(e => e.EmpSalStruct);
                ////////List<int> abcd = bb.Select(r => r.Id).ToList();

                ////////// List<int> abcd = bb.Select(r => r.Select(t => t.Id)).ToList();
                ////////var EmpSalStructTotal = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                ////////                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead)).Where(e => abcd.Contains(e.Id)).ToList();

                var mdelme = 0;
                mdelme = 45;
                //var comparedate = (Convert.ToDateTime("01/" + PayMonth).Date);
                //--old code 30/08/2022 start
                //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                //                            .Include(e => e.EmpSalStructDetails)
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                //                            //.Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead))
                //                            .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.ProcessType));
                ////.Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                //var comparedate = (Convert.ToDateTime("01/" + PayMonth).Date);
                //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                //                            .ToList();
                //--old code 30/08/2022 end


                // new code 30/08/2022 start
                var comparedate = (Convert.ToDateTime("01/" + PayMonth).Date);
                List<EmpSalStruct> EmpSalStructTotal = new List<EmpSalStruct>();
                List<EmpSalStructDetails> EmpSalStructDetails = new List<EmpSalStructDetails>();
                List<PayScaleAssignment> PayScaleAssignment = new List<PayScaleAssignment>();
                var PayScaleAssignmentObj = new PayScaleAssignment();
                List<SalaryHead> SalaryHead = new List<SalaryHead>();
                var SalaryHeadObj = new SalaryHead();
                List<SalHeadFormula> SalHeadFormula = new List<SalHeadFormula>();
                var SalaryHeadFormulaObj = new SalHeadFormula();

                EmpSalStructTotal = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.EffectiveDate >= comparedate).ToList();
                foreach (var i in EmpSalStructTotal)
                {
                    //  EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                    EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id && e.SalHeadFormula_Id > 0).ToList();
                    var x = db.EmpSalStructDetails.Where(e =>
                    e.EmpSalStruct.Id == i.Id && (e.SalHeadFormula_Id == 0 || e.SalHeadFormula_Id == null) && (e.Amount > 0 && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR")
                    ).ToList();
                    if (x.Count > 0)
                    { EmpSalStructDetails.AddRange(x); }
                    var xx = db.EmpSalStructDetails.Where(e =>
                    e.EmpSalStruct.Id == i.Id && (e.SalHeadFormula_Id == 0 || e.SalHeadFormula_Id == null) && (e.Amount > 0 && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR")
                    ).ToList();
                    if (xx.Count > 0)
                    { EmpSalStructDetails.AddRange(xx); }
                    var xxx = db.EmpSalStructDetails.Where(e =>
                                        e.EmpSalStruct.Id == i.Id && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "REGULAR"
                                        && (e.SalHeadFormula_Id == 0 || e.SalHeadFormula_Id == null)
                                        ).ToList();
                    if (xxx.Count > 0)
                    { EmpSalStructDetails.AddRange(xxx); }
                    foreach (var j in EmpSalStructDetails)
                    {
                        var SalHeadTmp = new SalaryHead();
                        j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                        j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                        j.SalaryHead = db.SalaryHead.Include(e => e.LeaveDependPolicy).Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                        //var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                        var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                            .Select(r => new
                            {
                                Id = r.Id,
                                SalHeadOperationType = r.SalHeadOperationType,
                                Frequency = r.Frequency,
                                Type = r.Type,
                                RoundingMethod = r.RoundingMethod,
                                ProcessType = r.ProcessType,
                                LvHead = r.LvHead

                            }).FirstOrDefault();
                        j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                        j.SalaryHead.Frequency = SalHeadData.Frequency;
                        j.SalaryHead.Type = SalHeadData.Type;
                        j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                        j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                        //SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                        foreach (var item in j.SalaryHead.LeaveDependPolicy)
                        {
                            item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                        }
                        //  var SalHeadTmp = new SalaryHead();
                        ////  PayScaleAssignmentObj = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                        ////  SalaryHeadFormulaObj = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();

                        //  var id = db.EmpSalStructDetails.Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();

                        //  SalHeadTmp = db.SalaryHead.Where(e => e.Id == id).FirstOrDefault();

                        //  SalHeadTmp.SalHeadOperationType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.SalHeadOperationType).FirstOrDefault();
                        //  SalHeadTmp.Frequency = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Frequency).FirstOrDefault();
                        //  SalHeadTmp.Type = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Type).FirstOrDefault();
                        //  SalHeadTmp.RoundingMethod = db.SalaryHead.Where(e => e.Id == id).Select(e => e.RoundingMethod).FirstOrDefault();
                        //  SalHeadTmp.ProcessType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.ProcessType).FirstOrDefault();
                        //  SalHeadTmp.LvHead = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LvHead.ToList()).FirstOrDefault();// to be check for output

                        //  j.SalaryHead = SalHeadTmp;

                    }
                    i.EmpSalStructDetails = EmpSalStructDetails;
                }



                // new code 30/08/2022 end


                // var EmpSalStructTotal = EmpSalStructList; //Total salary structure
                var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault(); //single salary structure

                if (OEmpSalStruct == null)
                {
                    return 2;//no salary structure available
                }
                SalaryT OSalaryT = new SalaryT(); //salary record store
                ESICTransT OESICTransT = new ESICTransT();
                OESICTransT = null;
                LWFTransT OLWFTransT = new LWFTransT();
                OLWFTransT = null;
                PTaxTransT OPTaxTransT = new PTaxTransT();
                OPTaxTransT = null;
                PFECRR OPFTransT = new PFECRR();
                OPFTransT = null;
                ITaxTransT OITaxTransT = new ITaxTransT();
                OITaxTransT = null;

                OSalaryT.PaybleDays = SalAttendanceT_PayableDays;
                OSalaryT.TotalDays = SalAttendanceT_monthDays;
                List<SalEarnDedT> OSalEarnDedT = new List<SalEarnDedT>();
                List<PerkTransT> OPerkTransT = new List<PerkTransT>();
                bool suspenddays = false;

                //CompanyPayroll SuspSalPolicy1 = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID)
                //                         .Include(e => e.SuspensionSalPolicy)
                //                         .Include(e => e.SuspensionSalPolicy.Select(a => a.SuspensionWages))
                //                         .Include(e => e.SuspensionSalPolicy.Select(a => a.SuspensionWages.RateMaster))
                //                         .Include(e => e.SuspensionSalPolicy.Select(a => a.SuspensionWages.RateMaster.Select(r => r.SalHead)))
                //                         .Include(e => e.SuspensionSalPolicy.Select(a => a.DayRange)).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                //*


                var SuspSalPolicy = new CompanyPayroll();
                SuspSalPolicy = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID).FirstOrDefault();
                List<SuspensionSalPolicy> SuspensionSalPolicy = db.CompanyPayroll.Where(e => e.Id == OCompanyPayrollID).Select(r => r.SuspensionSalPolicy.ToList()).FirstOrDefault();

                foreach (var i in SuspensionSalPolicy)
                {

                    if (i.SuspensionWages_Id != null)
                    {

                        var SuspensionWages = db.Wages.Where(e => e.Id == i.SuspensionWages_Id).Include(x => x.RateMaster)
                            .Include(x => x.RateMaster.Select(z => z.SalHead)).FirstOrDefault();
                        i.SuspensionWages = SuspensionWages;
                        List<Range> DayRange = db.SuspensionSalPolicy.Where(e => e.Id == i.Id).Select(x => x.DayRange.ToList()).FirstOrDefault();
                        List<Range> DayRangenew = new List<Range>();
                        foreach (var k in DayRange)
                        {
                            var rangobj = db.Range.Where(e => e.Id == k.Id).FirstOrDefault();
                            DayRangenew.Add(rangobj);

                        }
                        i.DayRange = DayRangenew;


                    }
                }


                if (Convert.ToDateTime("01/" + PayMonth).Date >= OEmpSalStruct.EffectiveDate.Value.Date)//Normal Salary for regular monthly and daily/hourly salary components
                {
                    // Suspend days calc start 04/03/2019
                    if (OCompanyPayroll.Company.Code != "KDCC")
                    {
                        double[] Payblesusdays = dateoffirstdaysus(OEmployeePayrollId, PayMonth, SalAttendanceT_PayableDays, SuspSalPolicy, suspenddays);
                        SalAttendanceT_PayableDays = Payblesusdays[0];
                        if (Payblesusdays[1] == 1)
                        {
                            suspenddays = true;
                        }
                    }
                    // Suspend days calc end 04/03/2019

                    //1652
                    //Utility.DumpProcessStatus(LineNo: 1652);
                    OSalEarnDedT = EarnSalHeadMDProcess(OEmpSalStruct, OSalEarnDedT, OEmployeePayrollId, SalAttendanceT_PayableDays, SalAttendanceT_monthDays, PayMonth);
                    //1654
                    //Utility.DumpProcessStatus(LineNo: 1745);

                }
                else
                {
                    //Utility.DumpProcessStatus(LineNo: 1999);
                    //List<EmpSalStruct> EmpSalStructTotal = EmpSalStructList.EmpSalStruct.ToList(); //Total salary structure
                    List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructure = new List<SalEarnDedTMultiStructure>();
                    double mPayDaysTotal = 0;
                    double mPayDaysRunning = 0;
                    List<EmpSalStruct> mEmpSalStructTotal = EmpSalStructTotal.Where(e => e.EffectiveDate.Value.Date >= (Convert.ToDateTime("01/" + PayMonth).Date))
                                         .OrderBy(r => r.EffectiveDate)
                                         .ToList();
                    foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotal)
                    {
                        if (OMultiEmpStruct.EffectiveDate.Value.Date < Convert.ToDateTime("01/" + PayMonth).Date)
                        {
                            //Utility.DumpProcessStatus(LineNo: 2011);

                            mPayDaysRunning = (OMultiEmpStruct.EndDate.Value.Date - Convert.ToDateTime("01/" + PayMonth).Date).Days + 1; //check for date result else add 1 day in result
                            mPayDaysTotal = mPayDaysTotal + mPayDaysRunning;
                        }
                        else
                        {
                            //Utility.DumpProcessStatus(LineNo: 2018);

                            if (OMultiEmpStruct.EndDate != null)
                            {
                                //Utility.DumpProcessStatus(LineNo: 2022);

                                // mPayDaysRunning = mPayDaysRunning +(OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1; //check for date result else add 1 day in result
                                mPayDaysRunning = (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1; //check for date result else add 1 day in result
                                // Check Attendance start
                                if (mPayDaysRunning < SalAttendanceT_PayableDays)
                                {
                                    mPayDaysTotal = mPayDaysTotal + mPayDaysRunning;
                                    SalAttendanceT_PayableDays = SalAttendanceT_PayableDays - mPayDaysRunning;
                                }
                                else
                                {
                                    mPayDaysRunning = SalAttendanceT_PayableDays;
                                    mPayDaysTotal = mPayDaysTotal + mPayDaysRunning;
                                    SalAttendanceT_PayableDays = SalAttendanceT_PayableDays - mPayDaysRunning;
                                }

                                // mPayDaysTotal = mPayDaysTotal + (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                // Check Attendance End

                                // Suspend days calc start 04/03/2019
                                double[] mrundays = dateofMiddledaysus_enddatenotnull(OEmployeePayrollId, PayMonth, mPayDaysRunning, SuspSalPolicy, OMultiEmpStruct, suspenddays);
                                mPayDaysRunning = mrundays[0];
                                if (mrundays[1] == 1)
                                {
                                    suspenddays = true;
                                }
                                // Suspend days calc end 04/03/2019
                            }
                            else
                            {
                                //Utility.DumpProcessStatus(LineNo: 2178);
                                // Check Attendance start 
                                // mPayDaysRunning = SalAttendanceT_monthDays - mPayDaysTotal;
                                mPayDaysRunning = SalAttendanceT_PayableDays;

                                // Check Attendance End
                                // Suspend days calc start 04/03/2019
                                double[] murndayss = dateofMiddledaysus_enddatenull(OEmployeePayrollId, PayMonth, mPayDaysRunning, SuspSalPolicy, OMultiEmpStruct, suspenddays);
                                mPayDaysRunning = murndayss[0];
                                if (murndayss[1] == 1)
                                {
                                    suspenddays = true;
                                }
                            }
                        }
                        //cal salgen process on derived empsalstruct
                        //Utility.DumpProcessStatus(LineNo: 2402);

                        OSalEarnDedTMultiStructure = EarnSalHeadMDProcessMS(OMultiEmpStruct, OSalEarnDedTMultiStructure, OEmployeePayrollId, mPayDaysRunning, SalAttendanceT_monthDays, PayMonth);

                    }
                    //combined the output data and dump in SalEarnDedT ==== check query  for duplicate record
                    //var OSalEarnDedTMulti = OSalEarnDedTMultiStructure
                    //                        .GroupBy(e => e.SalaryHead);
                    //Utility.DumpProcessStatus(LineNo: 1833);

                    var OSalEarnDedTMulti = OSalEarnDedTMultiStructure.GroupBy(x => new { x.SalaryHead })
                                               .Select(g => new
                                               {
                                                   SalaryHead = g.Key.SalaryHead,
                                                   StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                   TotalAmount = g.Sum(x => x.Amount),
                                                   Count = g.Count()
                                               }).ToList();

                    foreach (var ca in OSalEarnDedTMulti)
                    {
                        SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays;// if middle date activity 15 and employee present 13 days 
                        if (SalAttendanceT_PayableDays == 0)
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, ca.StdAmount, ca.SalaryHead);

                        }
                        else
                        {
                            double Amt = 0;
                            if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED")
                            {
                                SalHeadFormula OSalHeadForm = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id).FirstOrDefault().SalHeadFormula;
                                if (OSalHeadForm != null)
                                {
                                    Amt = Process.SalaryHeadGenProcess.CellingCkeck(OSalHeadForm, ca.TotalAmount);
                                }
                            }
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "OFFICIATING")
                            {
                                EmployeePayroll OEmpPayrolloff = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                Amt = OfficiatingServiceBookController.officiateprocess(OEmpPayrolloff, PayMonth, Amt, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                            }
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PFWAGESOFFICIATING")
                            {
                                EmployeePayroll OEmpPayrolloff = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                Amt = OfficiatingServiceBookController.officiateprocess(OEmpPayrolloff, PayMonth, Amt, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                            }
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "SUPANNOFFICIATING")
                            {
                                EmployeePayroll OEmpPayrolloff = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                Amt = OfficiatingServiceBookController.officiateprocess(OEmpPayrolloff, PayMonth, Amt, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                            }
                            // OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, ca.TotalAmount, ca.StdAmount, ca.SalaryHead);
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, (Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, Amt == 0 ? ca.TotalAmount : Amt)), ca.StdAmount, ca.SalaryHead);
                        }

                    }
                }
                //Utility.DumpProcessStatus(LineNo: 1867);

                //Funcattendance \\\
                //EmployeePayroll OEmpFuncT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.FunctAttendanceT)
                //             .Include(e => e.FunctAttendanceT.Select(r => r.EmpSalStruct))
                //             .Include(e => e.FunctAttendanceT.Select(r => r.SalaryHead))
                //             .Include(e => e.FunctAttendanceT.Select(r => r.SalaryHead.Type))
                //             .Include(e => e.FunctAttendanceT.Select(r => r.SalaryHead.Frequency))
                //            .Include(e => e.FunctAttendanceT.Select(r => r.SalaryHead.SalHeadOperationType))
                //             .OrderBy(e => e.Id).ToList();

                List<SalEarnDedT> OSalEarnDedTFunc = new List<SalEarnDedT>();

                List<FunctAttendanceT> EmpFuncT = db.FunctAttendanceT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth && e.isCancel == false && e.TrClosed == true && e.TrReject == false)
                             .Include(e => e.EmpSalStruct)
                             .Include(e => e.SalaryHead)
                             .Include(e => e.SalaryHead.Type)
                             .Include(e => e.SalaryHead.Frequency)
                            .Include(e => e.SalaryHead.SalHeadOperationType)
                             .OrderBy(e => e.Id).ToList();

                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).AsNoTracking().SingleOrDefault();
                //List<FunctAttendanceT> EmpFuncT = OEmpFuncT.FunctAttendanceT != null ? OEmpFuncT.FunctAttendanceT.Where(e => e.PayMonth == PayMonth && e.isCancel == false && e.TrClosed == true).ToList() : null;
                if (OEmpSalStruct.EndDate == null)
                {
                    if (EmpFuncT != null && EmpFuncT.Count() > 0)
                    {
                        #region foreach
                        Utility.DumpProcessStatus(LineNo: 1517);

                        foreach (FunctAttendanceT ca in EmpFuncT)
                        {
                            var functionpolicy = db.FunctionalAllowancePolicy.Include(e => e.PaymonthConcept).Where(e => e.SalaryHead_Id == ca.SalaryHead_Id).SingleOrDefault();


                            if (ca.ProcessMonth == PayMonth)
                            {

                                Utility.DumpProcessStatus(LineNo: 1526);



                                var EmpHDhead = OEmpSalStruct.EmpSalStructDetails
                                .Where(e => e.SalaryHead.Id == ca.SalaryHead.Id)
                                .Select(m => new { Amount = m.Amount, SalaryHead = m.SalaryHead, HourDays = ca.HourDays }).FirstOrDefault();
                                //if (EmpHDhead != null && ca.EmpSalStruct.Id == OEmpSalStruct.Id)
                                if (EmpHDhead != null)
                                {
                                    double LvDays = 0;
                                    double CalAmount = 0;

                                    if (OPayProcGrp != null)
                                    {
                                        if (ca.SalaryHead.OnAttend == true && ca.SalaryHead.OnLeave == true)
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1540);

                                            LvDays = LvChk(ca.SalaryHead, OPayProcGrp, PayMonth, OEmployeePayrollId, OEmpSalStruct);
                                            //SalAttendanceT_PayableDays = SalAttendanceT_PayableDays - LvDays;
                                            //Utility.DumpProcessStatus(LineNo: 1544);

                                        }
                                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                        {
                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = (EmpHDhead.Amount * (ca.HourDays) / (30));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                                }
                                                //  CalAmount = (EmpHDhead.Amount / 30) * SalAttendanceT_PayableDays;
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }

                                        }
                                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                        {
                                            // Utility.DumpProcessStatus(LineNo: 1561);

                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = (EmpHDhead.Amount * (ca.HourDays) / (30));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                                }
                                                // CalAmount = (EmpHDhead.Amount) - ((SalAttendanceT_monthDays - SalAttendanceT_PayableDays) / 30) * EmpHDhead.Amount;
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }

                                        }
                                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1575);

                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = (EmpHDhead.Amount * (ca.HourDays) / (SalAttendanceT_monthDays));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                                }
                                                // CalAmount = EmpHDhead.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }
                                        }
                                    }

                                    // Funtional allowance policy start

                                    if (functionpolicy != null)
                                    {
                                        LvDays = 0;
                                        CalAmount = 0;
                                        double functionalalwdays = 0;
                                        if (ca.HourDays > functionpolicy.MinDays && ca.HourDays <= functionpolicy.MaxDays)
                                        {
                                            functionalalwdays = ca.HourDays;
                                        }

                                        // functionalalwdays = FunctionalwdayCellingCkeck(functionpolicy, ca.HourDays);

                                        if (functionpolicy.PaymonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                        {
                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = (EmpHDhead.Amount * (functionalalwdays) / (30));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (functionalalwdays);
                                                }
                                                //  CalAmount = (EmpHDhead.Amount / 30) * SalAttendanceT_PayableDays;
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }

                                        }
                                        if (functionpolicy.PaymonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                        {
                                            // Utility.DumpProcessStatus(LineNo: 1561);

                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = (EmpHDhead.Amount * (functionalalwdays) / (30));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (functionalalwdays);
                                                }
                                                // CalAmount = (EmpHDhead.Amount) - ((SalAttendanceT_monthDays - SalAttendanceT_PayableDays) / 30) * EmpHDhead.Amount;
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }

                                        }
                                        if (functionpolicy.PaymonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                        {
                                            //Utility.DumpProcessStatus(LineNo: 1575);

                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = (EmpHDhead.Amount * (functionalalwdays) / (SalAttendanceT_monthDays));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (functionalalwdays);
                                                }
                                                // CalAmount = EmpHDhead.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }
                                        }




                                    }
                                    // Funtional allowance policy End


                                    CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                                    // OSalEarnDedTMultiStructure = SaveSalayDetailsMultiStructure(OSalEarnDedTMultiStructure, CalAmount, EmpHDhead.Amount, EmpHDhead.SalaryHead);

                                    if (SalAttendanceT_PayableDays == 0)
                                    {
                                        OSalEarnDedTFunc = SaveSalayDetails(OSalEarnDedTFunc, 0, EmpHDhead.Amount, ca.SalaryHead);

                                    }
                                    else
                                    {
                                        OSalEarnDedTFunc = SaveSalayDetails(OSalEarnDedTFunc, CalAmount, EmpHDhead.Amount, ca.SalaryHead);
                                    }

                                }
                                //}
                            }
                            else
                            {

                                // EmpSalStruct EmpSalaryT = QEmpSalStruct1.Where(e => e.Id == ca.EmpSalStruct.Id).SingleOrDefault();

                                List<EmpSalStruct> EmpSalStructTotal1 = new List<EmpSalStruct>();
                                List<EmpSalStructDetails> EmpSalStructDetails1 = new List<EmpSalStructDetails>();
                                EmpSalStructTotal1 = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.Id == ca.EmpSalStruct.Id).ToList();
                                foreach (var i in EmpSalStructTotal1)
                                {
                                    EmpSalStructDetails1 = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                                    foreach (var j in EmpSalStructDetails1)
                                    {
                                        var SalHeadTmp = new SalaryHead();
                                        j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                                        j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                                        j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                                        var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                                        var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                                            .Select(r => new
                                            {
                                                Id = r.Id,
                                                SalHeadOperationType = r.SalHeadOperationType,
                                                Frequency = r.Frequency,
                                                Type = r.Type,
                                                RoundingMethod = r.RoundingMethod,
                                                ProcessType = r.ProcessType,
                                                LvHead = r.LvHead

                                            }).FirstOrDefault();
                                        j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                                        j.SalaryHead.Frequency = SalHeadData.Frequency;
                                        j.SalaryHead.Type = SalHeadData.Type;
                                        j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                                        j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                                        SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                                        foreach (var item in SalHeadTmp.LeaveDependPolicy)
                                        {
                                            item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                                        }
                                    }
                                    i.EmpSalStructDetails = EmpSalStructDetails1;
                                }

                                EmpSalStruct EmpSalaryT = EmpSalStructTotal1.Where(e => e.Id == ca.EmpSalStruct.Id).SingleOrDefault();

                                //EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                                //                       .Include(e => e.EmpSalStruct).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                //                       .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                //                       .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))).AsNoTracking()
                                //                       .SingleOrDefault();
                                //EmpSalStruct EmpSalaryT = OEmployeePayroll.EmpSalStruct.Where(e => e.EffectiveDate.Value.ToString("MM/yyyy") == ca.ProcessMonth).LastOrDefault();
                                if (EmpSalaryT != null)
                                {
                                    //if (((ca.FromDate >= EmpSalaryT.EffectiveDate && (ca.FromDate <= EmpSalaryT.EndDate || EmpSalaryT.EndDate == null)))
                                    //    || ((ca.ToDate >= EmpSalaryT.EffectiveDate && (ca.ToDate <= EmpSalaryT.EndDate || EmpSalaryT.EndDate == null))))
                                    //    {
                                    Utility.DumpProcessStatus(LineNo: 1601);

                                    var mAttend1 = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId)
                                        .Select(t => t.SalAttendance.Where(e => e.PayMonth == ca.ProcessMonth)).SingleOrDefault();
                                    SalAttendanceT mAttend = mAttend1.FirstOrDefault();

                                    var EmpHDhead = EmpSalaryT.EmpSalStructDetails
                                    .Where(e => e.SalaryHead.Id == ca.SalaryHead.Id)
                                    .Select(m => new { Amount = m.Amount, SalaryHead = m.SalaryHead, HourDays = ca.HourDays }).FirstOrDefault();
                                    if (EmpHDhead != null && ca.EmpSalStruct.Id == EmpSalaryT.Id)
                                    {
                                        // Utility.DumpProcessStatus(LineNo: 1612);

                                        double CalAmount = 0;
                                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                        {
                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                //Utility.DumpProcessStatus(LineNo: 1617);

                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = EmpHDhead.Amount * ((ca.HourDays) / (30));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                                }
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }
                                        }
                                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                        {
                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                //Utility.DumpProcessStatus(LineNo: 1617);

                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = EmpHDhead.Amount * ((ca.HourDays) / (30));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                                }
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }
                                        }
                                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                        {
                                            if (ca.SalaryHead.OnAttend == true)
                                            {
                                                //Utility.DumpProcessStatus(LineNo: 1617);

                                                if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                {
                                                    CalAmount = EmpHDhead.Amount * ((ca.HourDays) / (mAttend.MonthDays));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                                }
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount;
                                            }
                                        }

                                        // Funtional allowance policy start

                                        if (functionpolicy != null)
                                        {

                                            CalAmount = 0;
                                            double functionalalwdays = 0;
                                            if (ca.HourDays > functionpolicy.MinDays && ca.HourDays <= functionpolicy.MaxDays)
                                            {
                                                functionalalwdays = ca.HourDays;
                                            }
                                            // functionalalwdays = FunctionalwdayCellingCkeck(functionpolicy, ca.HourDays);

                                            if (functionpolicy.PaymonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                            {
                                                if (ca.SalaryHead.OnAttend == true)
                                                {
                                                    if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                    {
                                                        CalAmount = (EmpHDhead.Amount * (functionalalwdays) / (30));
                                                    }
                                                    else
                                                    {
                                                        CalAmount = EmpHDhead.Amount * (functionalalwdays);
                                                    }
                                                    //  CalAmount = (EmpHDhead.Amount / 30) * SalAttendanceT_PayableDays;
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount;
                                                }

                                            }
                                            if (functionpolicy.PaymonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                            {
                                                // Utility.DumpProcessStatus(LineNo: 1561);

                                                if (ca.SalaryHead.OnAttend == true)
                                                {
                                                    if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                    {
                                                        CalAmount = (EmpHDhead.Amount * (functionalalwdays) / (30));
                                                    }
                                                    else
                                                    {
                                                        CalAmount = EmpHDhead.Amount * (functionalalwdays);
                                                    }
                                                    // CalAmount = (EmpHDhead.Amount) - ((SalAttendanceT_monthDays - SalAttendanceT_PayableDays) / 30) * EmpHDhead.Amount;
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount;
                                                }

                                            }
                                            if (functionpolicy.PaymonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                            {
                                                //Utility.DumpProcessStatus(LineNo: 1575);

                                                if (ca.SalaryHead.OnAttend == true)
                                                {
                                                    if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                                    {
                                                        CalAmount = (EmpHDhead.Amount * (functionalalwdays) / (SalAttendanceT_monthDays));
                                                    }
                                                    else
                                                    {
                                                        CalAmount = EmpHDhead.Amount * (functionalalwdays);
                                                    }
                                                    // CalAmount = EmpHDhead.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                                }
                                                else
                                                {
                                                    CalAmount = EmpHDhead.Amount;
                                                }
                                            }




                                        }
                                        // Funtional allowance policy End


                                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                                        if (SalAttendanceT_PayableDays == 0)
                                        {
                                            OSalEarnDedTFunc = SaveSalayDetails(OSalEarnDedTFunc, 0, EmpHDhead.Amount, ca.SalaryHead);

                                        }
                                        else
                                        {
                                            OSalEarnDedTFunc = SaveSalayDetails(OSalEarnDedTFunc, CalAmount, EmpHDhead.Amount, ca.SalaryHead);
                                        }
                                    }
                                    //}
                                }
                            }

                        }//foreach
                        #endregion foreach

                    }

                }

                var OSalEarnDedFunc = OSalEarnDedTFunc.GroupBy(x => new { x.SalaryHead })
                                               .Select(g => new
                                               {
                                                   SalaryHead = g.Key.SalaryHead,
                                                   StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                   TotalAmount = g.Sum(x => x.Amount),
                                                   Count = g.Count()
                                               }).ToList();

                foreach (var ca in OSalEarnDedFunc)
                {

                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, ca.TotalAmount, ca.StdAmount, ca.SalaryHead);


                }


                //Other irregular earnings

                //EmployeePayroll OthEarningTlist1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId)
                //    .Include(e => e.OtherEarningDeductionT.Select(a => a.SalaryHead))
                //    .Include(e => e.OtherEarningDeductionT.Select(a => a.SalaryHead.SalHeadOperationType))
                //    .Include(e => e.OtherEarningDeductionT.Select(a => a.SalaryHead.Type))
                //    .Include(e => e.OtherEarningDeductionT.Select(a => a.SalaryHead.Frequency)).AsParallel().OrderBy(e => e.Id)
                //    .SingleOrDefault();
                //*
                List<OthEarningDeductionT> OtherEarningDeductionT = new List<OthEarningDeductionT>();
                OtherEarningDeductionT = db.OthEarningDeductionT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth).ToList();
                foreach (var j in OtherEarningDeductionT)
                {
                    var OthEarningDeductionTObj = db.OthEarningDeductionT.Where(e => e.Id == j.Id).Select(r => r.SalaryHead).FirstOrDefault();
                    var SalaryHeadOtherearnded = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).FirstOrDefault();
                    var SalHeadType = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).Select(r => r.Type).FirstOrDefault();
                    var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                    var Frequency = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).Select(r => r.Frequency).FirstOrDefault();
                    j.SalaryHead = SalaryHeadOtherearnded;
                    j.SalaryHead.Type = SalHeadType;
                    j.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                    j.SalaryHead.Frequency = Frequency;

                }

                // List<OthEarningDeductionT> OthEarningT = OthEarningTlist.OtherEarningDeductionT
                List<OthEarningDeductionT> OthEarningT = OtherEarningDeductionT
                    .Where(e => e.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                                           && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                                            && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR"
                                            && e.PayMonth == PayMonth).ToList();
                //OEmployeePayroll.OtherEarningDeductionT
                if (OthEarningT.Count > 0 && OthEarningT != null)
                {
                    var EmpOthEarnhead = OEmpSalStruct.EmpSalStructDetails.
                   Join(OthEarningT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                   (u, uir) => new { u, uir })
                   .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, SalAmount = m.uir.SalAmount }).ToList();
                    foreach (var ca in EmpOthEarnhead)
                    {
                        double CalAmount = ca.SalAmount;
                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                        // if attendance 0 and other earning insert then this will go in salary as discuss with sir

                        if (SalAttendanceT_PayableDays == 0)
                        {
                            if (CalAmount > 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);
                            }


                        }
                        else
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                        }
                    }
                }
                //Utility.DumpProcessStatus(LineNo: 1896);

                //yearly payments



                //EmployeePayroll YearlyEarnPaymentT1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId)
                //    .Include(e => e.YearlyPaymentT.Select(a => a.SalaryHead))
                //    .Include(e => e.YearlyPaymentT.Select(a => a.SalaryHead.Type))
                //    .Include(e => e.YearlyPaymentT.Select(a => a.SalaryHead.Frequency)).AsParallel().OrderBy(e => e.Id)
                //    .SingleOrDefault();
                //*
                List<YearlyPaymentT> YearlyEarnPaymentT = new List<YearlyPaymentT>();
                YearlyEarnPaymentT = db.YearlyPaymentT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth).ToList();
                foreach (var j in YearlyEarnPaymentT)
                {
                    var YearlyPaymentTObj = db.YearlyPaymentT.Where(e => e.Id == j.Id).Select(r => r.SalaryHead).FirstOrDefault();
                    var SalaryHeadOtherearnded = db.SalaryHead.Where(e => e.Id == YearlyPaymentTObj.Id).FirstOrDefault();
                    var SalHeadType = db.SalaryHead.Where(e => e.Id == YearlyPaymentTObj.Id).Select(r => r.Type).FirstOrDefault();
                    var Frequency = db.SalaryHead.Where(e => e.Id == YearlyPaymentTObj.Id).Select(r => r.Frequency).FirstOrDefault();
                    j.SalaryHead = SalaryHeadOtherearnded;
                    j.SalaryHead.Type = SalHeadType;
                    j.SalaryHead.Frequency = Frequency;

                }

                //     var OYearlyEarnPaymentT = YearlyEarnPaymentT.YearlyPaymentT
                var OYearlyEarnPaymentT = YearlyEarnPaymentT
                    .Where(e => e.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                                           && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                                            && e.PayMonth == PayMonth && e.SalaryHead.InPayslip == true && e.ReleaseFlag == true).ToList();

                if (OYearlyEarnPaymentT != null && OYearlyEarnPaymentT.Count() > 0)
                {
                    var EmpYearlyEarnhead1 = OEmpSalStruct.EmpSalStructDetails
                            .Join(OYearlyEarnPaymentT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                            .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, AmountPaid = m.uir.AmountPaid }).ToList();

                    var EmpYearlyEarnhead = EmpYearlyEarnhead1
                                                  .GroupBy(a => new { SalaryHead = a.SalaryHead })
                                                  .Select(e => new { SalaryHead = e.Key, AmountPaid = e.Select(r => r.AmountPaid).ToList().Sum() })
                                                  .ToList();
                    foreach (var ca in EmpYearlyEarnhead)
                    {

                        double CalAmount = ca.AmountPaid;
                        SalaryHead SH = db.SalaryHead.Where(e => e.Id == ca.SalaryHead.SalaryHead.Id).SingleOrDefault();
                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(SH, CalAmount);
                        // if Attendance 0 yearly payment can pay if appear in payslip

                        if (SalAttendanceT_PayableDays == 0)
                        {
                            if (CalAmount > 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, SH);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, SH);
                            }


                        }
                        else
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, SH);
                        }

                    }
                }
                // Utility.DumpProcessStatus(LineNo: 2477);

                //Arrears payments earnings
                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.SalaryArrearT).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                if (OEmployeePayroll.SalaryArrearT != null && OEmployeePayroll.SalaryArrearT.Count > 0)
                {
                    //var ArrearDataT = OEmployeePayroll.SalaryArrearT.Where(e => e.PayMonth == PayMonth).ToList();
                    if (db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)
                      .Select(s => s.SalaryArrearT.Where(e => e.PayMonth == PayMonth)).Count() > 0)
                    {
                        //  var ArrearDataT = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)
                        //.Select(s => s.SalaryArrearT.Where(e => e.PayMonth == PayMonth))
                        //.Include(v => v.Select(w => w.SalaryArrearPaymentT))
                        //.Include(v => v.Select(w => w.SalaryArrearPaymentT.Select(x => x.SalaryHead)))
                        //.Include(v => v.Select(w => w.SalaryArrearPaymentT.Select(x => x.SalaryHead.Type)))
                        //.FirstOrDefault();
                        //  EmployeePayroll ArrearDataT_temp = new EmployeePayroll(); 


                        List<SalaryArrearT> SalaryArrearT = new List<SalaryArrearT>();
                        SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).ToList();
                        foreach (var j in SalaryArrearT)
                        {
                            var SalaryArrearPFTobj = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(x => x.SalaryArrearPFT).FirstOrDefault();
                            var SalaryArrearPFT = db.SalaryArrearPFT.Where(e => e.Id == SalaryArrearPFTobj.Id).FirstOrDefault();
                            List<SalaryArrearPaymentT> SalaryArrearPaymentT = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(r => r.SalaryArrearPaymentT.ToList()).FirstOrDefault();
                            foreach (var i in SalaryArrearPaymentT)
                            {
                                var SalaryArrearPaymentTObj = db.SalaryArrearPaymentT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                var SalaryHeadarrrear = db.SalaryHead.Where(e => e.Id == SalaryArrearPaymentTObj.Id).FirstOrDefault();
                                var SalHeadType = db.SalaryHead.Where(e => e.Id == SalaryArrearPaymentTObj.Id).Select(r => r.Type).FirstOrDefault();
                                i.SalaryHead = SalaryHeadarrrear;
                                i.SalaryHead.Type = SalHeadType;
                            }
                            j.SalaryArrearPaymentT = SalaryArrearPaymentT;
                            j.SalaryArrearPFT = SalaryArrearPFT;

                        }

                        //EmployeePayroll ArrearDataT_temp = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id && d.SalaryArrearT.Any(e => e.PayMonth == PayMonth))
                        //    .Include(e => e.SalaryArrearT)
                        //    .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT))
                        //    .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT.Select(z => z.SalaryHead)))
                        //    .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT.Select(z => z.SalaryHead.Type)))
                        //    .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT.Select(z => z.SalaryHead.SalHeadOperationType)))
                        //    .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPFT))
                        //    .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                        //List<SalaryArrearT> ArrearDataT = ArrearDataT_temp != null ? ArrearDataT_temp.SalaryArrearT.Where(e => e.PayMonth == PayMonth).ToList() : null;
                        //var ArrearDataTatt = ArrearDataT_temp != null ? ArrearDataT_temp.SalaryArrearT.Where(e => e.PayMonth == PayMonth).Select(x => x.FromDate.Value.ToString("MM/yyyy")).ToList() : null;
                        List<SalaryArrearT> ArrearDataT = SalaryArrearT != null ? SalaryArrearT.Where(e => e.PayMonth == PayMonth).ToList() : null;
                        var ArrearDataTatt = SalaryArrearT != null ? SalaryArrearT.Where(e => e.PayMonth == PayMonth).Select(x => x.FromDate.Value.ToString("MM/yyyy")).ToList() : null;
                        if (ArrearDataTatt != null)
                        {
                            var attmon = ArrearDataTatt.Distinct().ToList();
                            foreach (var item in attmon)
                            {
                                DateTime paymon = Convert.ToDateTime("01/" + item).Date;

                                EmployeePayroll ArrearDataT_tempchk = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)
                           .Include(e => e.SalaryArrearT)
                           .Include(e => e.SalaryArrearT.Select(x => x.ArrearType))
                           .Include(e => e.SalaryArrearT.Select(a => a.SalaryArrearPaymentT))
                           .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();
                                //  var ArrearDataTattchk = ArrearDataT_temp != null ? ArrearDataT_tempchk.SalaryArrearT.Where(e => e.ArrearType.LookupVal.ToUpper() == "LWP" && e.FromDate.Value.ToString("MM/yyyy") == paymon.ToString("MM/yyyy"))
                                var ArrearDataTattchk = SalaryArrearT != null ? ArrearDataT_tempchk.SalaryArrearT.Where(e => e.ArrearType.LookupVal.ToUpper() == "LWP" && e.FromDate.Value.ToString("MM/yyyy") == paymon.ToString("MM/yyyy"))
                                    .Select(x => new
                                    {
                                        PayMonth = x.FromDate.Value.ToString("MM/yyyy"),
                                        IsRecovery = x.IsRecovery,
                                        TotalDays = x.TotalDays
                                    }).ToList() : null;
                                if (ArrearDataTattchk != null && ArrearDataTattchk.Count() > 0)
                                {
                                    double arrattnotrec = 0;
                                    double arrattrec = 0;
                                    foreach (var item1 in ArrearDataTattchk)
                                    {
                                        if (item1.IsRecovery == false)
                                        {
                                            arrattnotrec = arrattnotrec + item1.TotalDays;
                                        }
                                        if (item1.IsRecovery == true)
                                        {
                                            arrattrec = arrattrec + item1.TotalDays;
                                        }
                                    }
                                    SalAttendanceT SalAttT = db.SalAttendanceT.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.PayMonth == item).FirstOrDefault();

                                    SalAttT.ArrearDays = arrattnotrec + (-arrattrec);

                                    db.SalAttendanceT.Attach(SalAttT);
                                    db.Entry(SalAttT).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(SalAttT).State = System.Data.Entity.EntityState.Detached;


                                }




                            }
                        }
                        if (ArrearDataT != null && ArrearDataT.Count() > 0)
                        {
                            EmpSalStructDetails EmpArrEarnHead = OEmpSalStruct.EmpSalStructDetails
                                .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ARREAREARN").SingleOrDefault();
                            EmpSalStructDetails EmpArrDedHead = OEmpSalStruct.EmpSalStructDetails
                               .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ARREARDED").SingleOrDefault();
                            if (EmpArrEarnHead == null || EmpArrDedHead == null)
                            {
                                return 3; //define ARREAREARN/ARREARDED salary head
                            }
                            else
                            {
                                double mArrRunningEarnAmount = 0;
                                double mArrRunningDedAmount = 0;
                                foreach (SalaryArrearT ca in ArrearDataT)
                                {
                                    if (ca.IsPaySlip == true)
                                    {
                                        mArrRunningEarnAmount = mArrRunningEarnAmount + ca.SalaryArrearPaymentT.Where(e => e.IsRecovery == false && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && e.SalaryHead.InPayslip == true)
                                            .Sum(r => r.SalHeadAmount);
                                        mArrRunningDedAmount = mArrRunningDedAmount + ca.SalaryArrearPaymentT.Where(e => e.IsRecovery == false && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && e.SalaryHead.InPayslip == true)
                                            .Sum(r => r.SalHeadAmount);
                                        mArrRunningEarnAmount = mArrRunningEarnAmount + ca.SalaryArrearPaymentT.Where(e => e.IsRecovery == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && e.SalaryHead.InPayslip == true)
                                            .Sum(r => r.SalHeadAmount);
                                        mArrRunningDedAmount = mArrRunningDedAmount + ca.SalaryArrearPaymentT.Where(e => e.IsRecovery == true && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && e.SalaryHead.InPayslip == true)
                                            .Sum(r => r.SalHeadAmount);
                                    }
                                }
                                //arrear earning
                                double CalAmount = 0;
                                CalAmount = mArrRunningEarnAmount;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpArrEarnHead.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    if (CalAmount > 0)
                                    {
                                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpArrEarnHead.SalaryHead);

                                    }
                                    else
                                    {
                                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpArrEarnHead.SalaryHead);
                                    }

                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpArrEarnHead.SalaryHead);
                                }
                                //arrear deduction
                                CalAmount = 0;
                                CalAmount = mArrRunningDedAmount;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpArrDedHead.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    if (CalAmount > 0)
                                    {
                                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpArrDedHead.SalaryHead);

                                    }
                                    else
                                    {
                                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpArrDedHead.SalaryHead);
                                    }

                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpArrDedHead.SalaryHead);
                                }
                            }
                        }
                    }
                }
                //Utility.DumpProcessStatus(LineNo: 2608);


                // officiating Start
                var OEmployeePayrollBms = db.BMSPaymentReq.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth && e.IsCancel == false && e.TrClosed == true && e.TrReject == false).ToList();
                if (OEmployeePayrollBms != null && OEmployeePayrollBms.Count > 0)
                {
                    List<BMSPaymentReq> BMSPaymentReq = new List<BMSPaymentReq>();
                    BMSPaymentReq = db.BMSPaymentReq.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.PayMonth == PayMonth && e.IsCancel == false && e.TrClosed == true && e.TrReject == false).ToList();
                    foreach (var j in BMSPaymentReq)
                    {
                        var OfficiatingPFTobj = db.BMSPaymentReq.Where(e => e.Id == j.Id).Select(x => x.OfficiatingPFT).FirstOrDefault();
                        var SalaryArrearPFT = OfficiatingPFTobj != null ? db.SalaryArrearPFT.Where(e => e.Id == OfficiatingPFTobj.Id).FirstOrDefault() : null;
                        List<OfficiatingPaymentT> OfficiatingPaymentT = db.BMSPaymentReq.Where(e => e.Id == j.Id).Select(r => r.OfficiatingPaymentT.ToList()).FirstOrDefault();
                        foreach (var i in OfficiatingPaymentT)
                        {
                            var OfficiatingPaymentTTObj = db.OfficiatingPaymentT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                            var SalaryHeadarrrear = db.SalaryHead.Where(e => e.Id == OfficiatingPaymentTTObj.Id).FirstOrDefault();
                            var SalHeadType = db.SalaryHead.Where(e => e.Id == OfficiatingPaymentTTObj.Id).Select(r => r.Type).FirstOrDefault();
                            i.SalaryHead = SalaryHeadarrrear;
                            i.SalaryHead.Type = SalHeadType;
                        }
                        j.OfficiatingPaymentT = OfficiatingPaymentT;
                        j.OfficiatingPFT = SalaryArrearPFT;

                    }

                    List<BMSPaymentReq> BMSPaymentReqT = BMSPaymentReq != null ? BMSPaymentReq.Where(e => e.PayMonth == PayMonth && e.IsCancel == false && e.TrClosed == true && e.TrReject == false).ToList() : null;
                    if (BMSPaymentReqT != null && BMSPaymentReqT.Count() > 0)
                    {
                        EmpSalStructDetails EmpOFFEarnHead = OEmpSalStruct.EmpSalStructDetails
                            .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFEARN").SingleOrDefault();
                        EmpSalStructDetails EmpOFFDedHead = OEmpSalStruct.EmpSalStructDetails
                           .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "OFFDED").SingleOrDefault();
                        if (EmpOFFEarnHead == null || EmpOFFDedHead == null)
                        {
                            return 3; //define ARREAREARN/ARREARDED salary head
                        }
                        else
                        {
                            double moffRunningEarnAmount = 0;
                            double moffRunningDedAmount = 0;
                            foreach (BMSPaymentReq ca in BMSPaymentReqT)
                            {
                                if (ca.IsCancel == false)
                                {
                                    moffRunningEarnAmount = moffRunningEarnAmount + ca.OfficiatingPaymentT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && e.SalaryHead.InPayslip == true)
                                        .Sum(r => r.SalHeadAmount);
                                    moffRunningDedAmount = moffRunningDedAmount + ca.OfficiatingPaymentT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && e.SalaryHead.InPayslip == true)
                                        .Sum(r => r.SalHeadAmount);

                                }
                            }
                            //arrear earning
                            double CalAmount = 0;
                            CalAmount = moffRunningEarnAmount;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpOFFEarnHead.SalaryHead, CalAmount);
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                if (CalAmount > 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpOFFEarnHead.SalaryHead);

                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpOFFEarnHead.SalaryHead);
                                }

                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpOFFEarnHead.SalaryHead);
                            }
                            //arrear deduction
                            CalAmount = 0;
                            CalAmount = moffRunningDedAmount;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpOFFDedHead.SalaryHead, CalAmount);
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                if (CalAmount > 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpOFFDedHead.SalaryHead);

                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpOFFDedHead.SalaryHead);
                                }

                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpOFFDedHead.SalaryHead);
                            }
                        }
                    }

                }

                // officiating End

                List<Int32> CurSalHead = OSalEarnDedT.Select(r => r.SalaryHead.Id).ToList();
                var ONonAttHeadList = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.OnAttend == false && s.SalaryHead.Frequency.LookupVal.ToUpper() != "YEARLY" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "NONREGULAR"
                   && CurSalHead.Contains(s.SalaryHead.Id) && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "ARREAREARN" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "ARREARDED" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "OFFEARN" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "OFFDED")
                  .ToList();
                foreach (var a in ONonAttHeadList)
                {
                    double NewAmt = 0;
                    var SalHead = OSalEarnDedT.Where(e => e.SalaryHead.Id == a.SalaryHead.Id).SingleOrDefault();
                    if (SalHead != null)
                    {
                        if (Convert.ToDateTime("01/" + PayMonth).Date >= OEmpSalStruct.EffectiveDate.Value.Date)//Normal Salary for regular monthly and daily/hourly salary components
                        {
                            if (a.Amount == SalHead.Amount)
                            {
                                NewAmt = a.Amount;
                            }
                            else
                            {
                                NewAmt = SalHead.Amount;
                            }
                        }
                        else
                        {
                            //List<EmpSalStruct> EmpSalStructTotal = EmpSalStructList.EmpSalStruct.ToList(); //Total salary structure
                            List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructure = new List<SalEarnDedTMultiStructure>();
                            double mPayDaysTotal = 0;
                            double mPayDaysRunning = 0;

                            List<EmpSalStruct> mEmpSalStructTotal = EmpSalStructTotal.Where(e => e.EffectiveDate.Value.Date >= (Convert.ToDateTime("01/" + PayMonth).Date))
                                                 .OrderBy(r => r.Id)
                                                 .ToList();
                            foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotal)
                            {
                                if (OMultiEmpStruct.EffectiveDate.Value.Date < Convert.ToDateTime("01/" + PayMonth).Date)
                                {
                                    //Utility.DumpProcessStatus(LineNo: 2640);

                                    mPayDaysRunning = (OMultiEmpStruct.EndDate.Value.Date - Convert.ToDateTime("01/" + PayMonth).Date).Days + 1; //check for date result else add 1 day in result
                                    mPayDaysTotal = mPayDaysTotal + mPayDaysRunning;
                                    if (a.Amount <= SalHead.Amount)
                                    {
                                        NewAmt = (a.Amount / SalAttendanceT_monthDays) * mPayDaysRunning;
                                    }
                                }
                                else
                                {
                                    //Utility.DumpProcessStatus(LineNo: 2651);

                                    if (OMultiEmpStruct.EndDate != null)
                                    {
                                        //Utility.DumpProcessStatus(LineNo: 2655);

                                        mPayDaysRunning = (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1; //check for date result else add 1 day in result
                                        mPayDaysTotal = mPayDaysTotal + (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;

                                        if (a.Amount <= SalHead.Amount)
                                        {
                                            NewAmt = NewAmt + ((a.Amount / SalAttendanceT_monthDays) * mPayDaysRunning);
                                        }
                                    }
                                    else
                                    {
                                        // Utility.DumpProcessStatus(LineNo: 2178);
                                        mPayDaysRunning = SalAttendanceT_monthDays - mPayDaysTotal;
                                        if (a.Amount <= SalHead.Amount)
                                        {
                                            NewAmt = NewAmt + ((a.Amount / SalAttendanceT_monthDays) * mPayDaysRunning);
                                        }
                                    }
                                }
                            }
                        }

                        SalHead.Amount = SalaryHeadGenProcess.RoundingFunction(a.SalaryHead, NewAmt);
                    }
                }

                if (OCompanyPayroll.Company.Code == "KDCC")
                {
                    //Utility.DumpProcessStatus(LineNo: 2684);
                    //EmployeePayroll othserK1 = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployeePayrollId)
                    //                            .Include(e => e.OtherServiceBook)
                    //                            .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct))
                    //                            .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus))
                    //                            .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus.EmpActingStatus))
                    //                            .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
                    //                            .SingleOrDefault();
                    //*

                    var othserK = new EmployeePayroll();
                    othserK = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                    List<OtherServiceBook> OtherServiceBookList = new List<OtherServiceBook>();

                    OtherServiceBookList = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId).ToList();
                    if (OtherServiceBookList != null && OtherServiceBookList.Count() > 0)
                    {
                        foreach (var item in OtherServiceBookList)
                        {
                            var NewPayStruct = db.OtherServiceBook.Where(e => e.Id == item.Id).Select(r => r.NewPayStruct).FirstOrDefault();
                            var JobStatus = db.PayStruct.Where(e => e.Id == NewPayStruct.Id).Select(r => r.JobStatus).FirstOrDefault();
                            var JobStatus_EmpActingStatus = db.JobStatus.Where(e => e.Id == JobStatus.Id).Select(r => r.EmpActingStatus).FirstOrDefault();
                            var OthServiceBookActivity = db.OtherServiceBook.Where(e => e.Id == item.Id).Select(r => r.OthServiceBookActivity).FirstOrDefault();
                            item.NewPayStruct = NewPayStruct;
                            item.NewPayStruct.JobStatus = JobStatus;
                            item.NewPayStruct.JobStatus.EmpActingStatus = JobStatus_EmpActingStatus;
                            item.OthServiceBookActivity = OthServiceBookActivity;
                            // OtherServiceBookList.Add(OtherServiceBook);

                        }
                        othserK.OtherServiceBook = OtherServiceBookList;
                    }



                    if (othserK.OtherServiceBook != null && othserK.OtherServiceBook.Count() > 0)
                    {
                        //List<OtherServiceBook> OthServBkSus = othserK.OtherServiceBook.Where(e => ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null).ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED") || ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null).ToString().ToUpper() == "ACTIVE" && e.OthServiceBookActivity.Name.ToUpper() == "REJOIN")).OrderByDescending(e => e.ReleaseDate).ToList();

                        List<OtherServiceBook> OthServBkSus = othserK.OtherServiceBook.Where(e => ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED") || ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null & e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "ACTIVE" && e.OthServiceBookActivity.Name.ToUpper() == "REJOIN")).OrderByDescending(e => e.ReleaseDate).ToList();

                        //OthServBkSus.OrderByDescending(e => e.ReleaseDate);
                        if (OthServBkSus.Count > 0)
                        {
                            string ff = OthServBkSus.First().NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString();
                            if (ff.ToUpper() == "SUSPEND" && suspenddays == false)
                            {
                                DateTime? compRdate = OthServBkSus.First().ReleaseDate;
                                if (OEmpSalStruct.EffectiveDate.Value.Date >= Convert.ToDateTime(compRdate).Date)
                                {
                                    foreach (var a in OSalEarnDedT.ToList())
                                    {
                                        // a.Amount = (a.Amount / 3);
                                        a.Amount = SalaryHeadGenProcess.RoundingFunction(a.SalaryHead, (a.Amount / 3));
                                    }
                                }

                            }
                        }
                    }
                }

                double Grossearnamt = 0;
                Grossearnamt = OSalEarnDedT
                                   .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                                   .Sum(e => e.Amount);

                //standard monthly deductions
                List<EmpSalStructDetails> EmpsalDedhead = OEmpSalStruct.EmpSalStructDetails
                    .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                        && s.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                        && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "INSURANCE").OrderBy(e => e.SalaryHead.SeqNo).ToList(); //|| s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN").ToList();
                if (EmpsalDedhead != null && EmpsalDedhead.Count() > 0)
                {
                    foreach (EmpSalStructDetails ca in EmpsalDedhead)
                    {
                        double CalAmount = 0;
                        if (ca.SalaryHead.OnAttend == true)
                        {
                            CalAmount = ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                        }
                        else
                        {
                            CalAmount = ca.Amount;
                        }
                        if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                        {
                            EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                            CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                        }

                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                        if (OSalEarnDedT != null)
                        {
                            // if gross income available then deduction will apply not check attendance
                            if (Grossearnamt == 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);

                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                            }

                        }
                    }
                }
                // Utility.DumpProcessStatus(LineNo: 2048);

                //Other Deductions

                //var OthDeductionT = db.EmployeePayroll.Include(d => d.OtherEarningDeductionT.Select(t => t.SalaryHead))
                //    .Where(d => d.Id == OEmployeePayroll.Id).AsParallel().OrderBy(e => e.Id)// OEmployeePayroll.OtherEarningDeductionT
                //    .Select(s => s.OtherEarningDeductionT.Where(e => e.PayMonth.ToString() == PayMonth
                //    && e.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                //    && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                //    && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR"))
                //    //.Include(e => e.Select(d => d.SalaryHead))
                //    .FirstOrDefault();

                List<OthEarningDeductionT> OthDeductionT = OtherEarningDeductionT
                   .Where(e => e.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                                          && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                                           && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR"
                                           && e.PayMonth == PayMonth).ToList();

                // OthDeductionT = OthDeductionT.Select(r => r.SalaryHead);
                if (OthDeductionT != null && OthDeductionT.Count() > 0)
                {
                    var EmpOthDedhead = OEmpSalStruct.EmpSalStructDetails.
                   Join(OthDeductionT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                   (u, uir) => new { u, uir })
                   .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, SalAmount = m.uir.SalAmount }).ToList();
                    foreach (var ca in EmpOthDedhead)
                    {
                        double CalAmount = 0;
                        if (ca.SalaryHead.OnAttend == true)
                        {
                            CalAmount = ca.SalAmount;
                        }
                        else
                        {
                            CalAmount = ca.SalAmount;
                        }
                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                        // if gross income available then deduction will apply not check attendance
                        if (Grossearnamt == 0)
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, ca.Amount, ca.SalaryHead);

                        }
                        else
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                        }
                    }
                }
                //Utility.DumpProcessStatus(LineNo: 2096);

                //Yearly deductions

                //var YearlyDeductionPaymentT = db.EmployeePayroll.Include(d => d.YearlyPaymentT.Select(t => t.SalaryHead)).Where(d => d.Id == OEmployeePayroll.Id).AsParallel().OrderBy(e => e.Id)// OEmployeePayroll.YearlyPaymentT
                //    .Select(s => s.YearlyPaymentT.Where(e => e.PayMonth.ToString() == PayMonth
                //    && e.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                //    && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                //    && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR"))
                //    //.Include(e => e.Select(d => d.SalaryHead))
                //    .FirstOrDefault();
                var YearlyDeductionPaymentT = YearlyEarnPaymentT
                    .Where(e => e.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                                           && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                                            && e.PayMonth == PayMonth && e.SalaryHead.InPayslip == true && e.ReleaseFlag == true).ToList();

                if (YearlyDeductionPaymentT != null && YearlyDeductionPaymentT.Count() > 0)
                {
                    //correct linq
                    var EmpYearlyDedhead = OEmpSalStruct.EmpSalStructDetails.
                   Join(YearlyDeductionPaymentT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                   (u, uir) => new { u, uir })
                   .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, AmountPaid = m.uir.AmountPaid }).ToList();
                    foreach (var ca in EmpYearlyDedhead)
                    {
                        double CalAmount = 0;
                        if (ca.SalaryHead.OnAttend == true)
                        {
                            CalAmount = ca.AmountPaid;
                        }
                        else
                        {
                            CalAmount = ca.AmountPaid;
                        }
                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                        // if gross income available then deduction will apply not check attendance
                        if (Grossearnamt == 0)
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);
                        }
                        else
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, ca.Amount, ca.SalaryHead);
                        }
                    }
                }
                //Utility.DumpProcessStatus(LineNo: 2142);

                //Loan Deductions,

                //EmployeePayroll OEmpLoan1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)
                //    .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                //     .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                //     .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead)).AsNoTracking().OrderBy(e => e.Id)
                //     .FirstOrDefault();
                //*
                var OEmpLoan = new EmployeePayroll();
                OEmpLoan = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                List<LoanAdvRequest> LoanAdvRequest = new List<LoanAdvRequest>();
                var LoanAdvRequestObj = new LoanAdvRequest();
                LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.IsActive == true)
                    //            .Include(e=>e.LoanAdvRepaymentT)
                    .ToList();
                foreach (var i in LoanAdvRequest)
                {

                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).FirstOrDefault();
                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                    var SalaryHeadL = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).FirstOrDefault();
                    List<LoanAdvRepaymentT> LoanAdvRepaymentTList = new List<LoanAdvRepaymentT>();

                    //var LoanAdvRepaymentT = db.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == i.Id && e.PayMonth == PayMonth).FirstOrDefault();
                    var LoanAdvRepaymentT = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvRepaymentT.Where(t => t.PayMonth == PayMonth).ToList()).FirstOrDefault();
                    i.LoanAdvRepaymentT = LoanAdvRepaymentT;
                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                    i.LoanAdvanceHead = LoanAdvanceHead;
                    i.LoanAdvanceHead.SalaryHead = SalaryHeadL;
                    //if (LoanAdvRepaymentT != null)
                    //{
                    //    LoanAdvRepaymentTList.Add(LoanAdvRepaymentT);
                    //    i.LoanAdvRepaymentT = (LoanAdvRepaymentTList);
                    //}


                }
                OEmpLoan.LoanAdvRequest = LoanAdvRequest;

                List<LoanAdvRequest> OLoanAdvReuest = OEmpLoan.LoanAdvRequest.ToList();

                if (OLoanAdvReuest != null && OLoanAdvReuest.Count() > 0)
                {
                    OLoanAdvReuest.Where(x => x.LoanAdvRepaymentT != null).ToList().ForEach(x =>
                    {
                        LoanAdvRepaymentT OLoanRepay = x.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                        // LoanAdvRepaymentT OLoanRepay = db.LoanAdvRepaymentT.Where(e => e.Id == OLoanRepayold.Id).SingleOrDefault();
                        if (OLoanRepay != null)
                        {
                            OLoanRepay.RepaymentDate = DateTime.Now;
                            OLoanRepay.InstallmentPaid = OLoanRepay.InstallmentAmount;
                            // if gross income available then deduction will apply not check attendance
                            if (Grossearnamt == 0)
                            {
                                OLoanRepay.RepaymentDate = null;
                                OLoanRepay.InstallmentPaid = 0;
                            }

                            db.LoanAdvRepaymentT.Attach(OLoanRepay);
                            db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    });


                    var EmpLoanHead = OEmpSalStruct.EmpSalStructDetails.
                        Join(OLoanAdvReuest, u => u.SalaryHead.Id, uir => uir.LoanAdvanceHead.SalaryHead.Id,
                        (u, uir) => new { u, uir })
                        .Where(r => r.uir.IsActive == true && r.uir.LoanAdvRepaymentT != null)
                        .Select(m => new { AdvLoanRepayment = m.uir.LoanAdvRepaymentT, SalaryHead = m.u.SalaryHead })
                        .ToList();
                    var EmpLoanRepay = EmpLoanHead.Select(e => e.AdvLoanRepayment
                         .Where(m => m.PayMonth == PayMonth)
                         .Select(s => new { Amount = s.InstallmentAmount, SalaryHead = e.SalaryHead, AmountPaid = s.InstallmentPaid }))
                         .ToList();
                    foreach (var ca1 in EmpLoanRepay)
                    {
                        foreach (var ca in ca1)
                        {

                            double CalAmount = 0;
                            CalAmount = ca.AmountPaid;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                            // if gross income available then deduction will apply not check attendance
                            if (Grossearnamt == 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, ca.Amount, ca.SalaryHead);
                            }
                        }
                    }
                }

                //PERK DEDUCTIONS
                var PerkTransT = db.EmployeePayroll.Include(d => d.PerkTransT.Select(t => t.SalaryHead)).Where(d => d.Id == OEmployeePayroll.Id).AsParallel().OrderBy(e => e.Id).FirstOrDefault();

                List<PerkTransT> YearlyPerkEarnPaymentT = PerkTransT.PerkTransT
                    .Where(e => e.PayMonth.ToString() == PayMonth &&
                        (e.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY" || e.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY") &&
                        e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").ToList();
                if (YearlyPerkEarnPaymentT != null && YearlyPerkEarnPaymentT.Count() > 0)
                {
                    var EmpYearlyPerkEarnhead = OEmpSalStruct.EmpSalStructDetails
                            .Join(YearlyPerkEarnPaymentT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                            .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, AmountPaid = m.uir.ActualAmount }).ToList();
                    foreach (var ca in EmpYearlyPerkEarnhead)
                    {
                        double CalAmount = ca.AmountPaid;
                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                        if (SalAttendanceT_PayableDays == 0)
                        {
                            if (CalAmount > 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);
                                var PerkTransTUp = db.PerkTransT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).FirstOrDefault();
                                PerkTransTUp.ActualAmount = 0;
                                db.PerkTransT.Attach(PerkTransTUp);
                                db.Entry(PerkTransTUp).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                        }


                    }
                }
                //Utility.DumpProcessStatus(LineNo: 2174);


                //Utility.DumpProcessStatus(LineNo: 2237);

                //insurance deduction
                EmployeePayroll OEmpInsurance = db.EmployeePayroll.Include(e => e.InsuranceDetailsT.Select(r => r.InsuranceProduct))
                    .Include(e => e.InsuranceDetailsT.Select(a => a.OperationStatus)).Where(d => d.Id == OEmployeePayroll.Id).AsNoTracking().OrderBy(e => e.Id)
                    .FirstOrDefault();

                List<InsuranceDetailsT> OInsurancePaymentT = OEmpInsurance.InsuranceDetailsT.ToList();
                if (OInsurancePaymentT != null && OInsurancePaymentT.Count() > 0)
                {
                    List<Insurance> InsuranceMaster = db.Insurance
                                .Include(e => e.InsuranceProduct)
                                .Include(e => e.SalaryHead)
                                .ToList();
                    var EmpInsuranceHead1 = OEmpSalStruct.EmpSalStructDetails.
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

                    foreach (var ca in EmpInsuranceHead)
                    {
                        foreach (var ca1 in ca)
                        {
                            double CalAmount = ca1.Permium;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca1.SalaryHead, CalAmount);
                            // if gross income available then deduction will apply not check attendance
                            if (Grossearnamt == 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca1.SalaryHead);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, CalAmount, ca1.SalaryHead);
                            }
                        }

                    }
                }


                //PF deductions
                if (OEmpOff != null)
                {
                    if (OEmpOff.PFAppl == true)
                    {
                        EmpSalStructDetails EmpPFHead = OEmpSalStruct.EmpSalStructDetails
                                    .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault();
                        if (EmpPFHead == null)
                        {
                            return 5; //no pf head defined

                        }
                        else
                        {
                            //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EndDate != null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date).SingleOrDefault();
                            PFMaster OCompPFMaster = null;
                            //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e =>e.EstablishmentID==OEmpOff.PFTrust_EstablishmentId && e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date).FirstOrDefault();
                            foreach (var itemPFmas in OCompanyPayroll.PFMaster.ToList())
                            {
                                if (itemPFmas.EstablishmentID == OEmpOff.PFTrust_EstablishmentId && (itemPFmas.EndDate == null || itemPFmas.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date))
                                {
                                    OCompPFMaster = itemPFmas;
                                }
                            }
                            if (OCompPFMaster == null)
                            {
                                return 6;//no pf master
                            }
                            else
                            {
                                //PFTransT OPFTransT = new PFTransT();
                                OPFTransT = PFcalc(OCompPFMaster, OEmpSalStruct, OEmployeePayrollId, null, PayMonth, OSalEarnDedT, OEmpOff.UANNo);
                                if (OPFTransT != null)
                                {
                                    double CalAmount = 0;
                                    CalAmount = Convert.ToDouble(OPFTransT.EE_Share);
                                    CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpPFHead.SalaryHead, CalAmount);
                                    // if Attendance 0 yearly payment can pay if appear in payslip
                                    if (SalAttendanceT_PayableDays == 0)
                                    {
                                        if (CalAmount > 0)
                                        {
                                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpPFHead.SalaryHead);
                                        }
                                        else
                                        {
                                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpPFHead.SalaryHead);
                                        }

                                    }
                                    else
                                    {
                                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpPFHead.SalaryHead);
                                    }
                                }
                            }
                        }

                    }
                }
                //PT deductions
                // Utility.DumpProcessStatus(LineNo: 2372);

                EmpSalStructDetails EmpPTHead = OEmpSalStruct.EmpSalStructDetails
                           .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault();
                if (OEmpOff != null)
                {
                    if (OEmpOff.PTAppl == true && EmpPTHead != null)
                    {
                        //OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                        //                                 .Include(e => e.EmpSalStruct).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                        List<PTaxMaster> OCompPTMaster = OCompanyPayroll.PTaxMaster.ToList();
                        //List<EmpSalStruct> EmpSalStructTotal = OEmployeePayroll.EmpSalStruct.ToList(); //Total salary structure

                        //Aj
                        //EmpSalStruct OEmpSalStruct1 = EmpSalStructTotal.Where(e => e.EffectiveDate == Convert.ToDateTime("01/" + PayMonth).Date).SingleOrDefault(); //single salary structure
                        EmpSalStruct OEmpSalStruct1 = EmpSalStructTotal.Where(e => e.EndDate == null).SingleOrDefault(); //single salary structure


                        List<PTaxMaster> OCompanyPTaxMaster = new List<PTaxMaster>();
                        EmpSalStruct OState = db.EmpSalStruct
                       .Include(e => e.GeoStruct)
                       .Include(e => e.GeoStruct.Location)
                       .Include(e => e.GeoStruct.Location.Address)
                       .Include(e => e.GeoStruct.Location.Address.State)
                       .Where(e => e.Id == OEmpSalStruct1.Id)
                       .SingleOrDefault();

                        State mState = OState.GeoStruct.Location == null ? null : OState.GeoStruct.Location.Address == null ? null : OState.GeoStruct.Location.Address.State == null ? null : OState.GeoStruct.Location.Address.State;
                        if (mState == null)
                        {
                            return 8;
                        }
                        int state = OCompPTMaster.Where(x => x.States.Id == mState.Id).Select(e => e.States.Id).SingleOrDefault();
                        if (state != mState.Id)
                        {

                            return 10;

                        }

                        //PTaxTransT OPTaxTransT = new PTaxTransT();
                        OPTaxTransT = PTcalc(OCompPTMaster, OEmpSalStruct, OEmployeePayrollId, null, PayMonth, OSalEarnDedT, SalAttendanceT_PayableDays);
                        if (OPTaxTransT != null)
                        {
                            double CalAmount = 0;

                            CalAmount = OPTaxTransT.PTAmount;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpPTHead.SalaryHead, CalAmount);
                            // if Attendance 0 yearly payment can pay if appear in payslip
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                if (CalAmount > 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpPTHead.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpPTHead.SalaryHead);
                                }

                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpPTHead.SalaryHead);
                            }
                        }
                        else
                        {
                            return 8;
                        }
                    }
                }
                //Utility.DumpProcessStatus(LineNo: 2432);

                ////LWF deductions
                EmpSalStructDetails EmpLWFHead = OEmpSalStruct.EmpSalStructDetails
                           .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LWF").SingleOrDefault();
                if (OEmpOff != null)
                {
                    if (OEmpOff.LWFAppl == true && EmpLWFHead != null && EmpLWFHead.SalHeadFormula_Id != null)
                    {
                        List<LWFMaster> OCompLWFMaster = OCompanyPayroll.LWFMaster.ToList();
                        //LWFTransT OLWFTransT = new LWFTransT();

                        // if Attendance 0 yearly payment can pay if appear in payslip
                        OLWFTransT = LWFcalc(OCompLWFMaster, OEmpSalStruct, null, PayMonth, OSalEarnDedT);
                        //if (SalAttendanceT_PayableDays != 0)
                        //{
                        //    OLWFTransT = LWFcalc(OCompLWFMaster, OEmpSalStruct, null, PayMonth, OSalEarnDedT);
                        //}
                        //else
                        //{
                        //    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        //    OLWFTransT = new LWFTransT()
                        //    {
                        //        CompAmt = 0,
                        //        EmpAmt = 0,
                        //        LWFWages = 0,
                        //        DBTrack = dbt
                        //    };
                        //}
                        if (OLWFTransT != null)
                        {
                            double CalAmount = 0;
                            CalAmount = OLWFTransT.EmpAmt;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpLWFHead.SalaryHead, CalAmount);
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                if (CalAmount > 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpLWFHead.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpLWFHead.SalaryHead);
                                }

                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpLWFHead.SalaryHead);
                            }
                        }
                        else
                        {
                            return 9;
                        }
                    }
                }
                // Utility.DumpProcessStatus(LineNo: 2464);

                ////ESIC deductions
                EmpSalStructDetails EmpESICHead = OEmpSalStruct.EmpSalStructDetails
                           .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault();
                if (EmpESICHead != null)
                {
                    List<ESICMaster> OCompESICMaster = OCompanyPayroll.ESICMaster.ToList();
                    string mESICQualy = ESICQualify(OCompESICMaster, OEmpSalStruct, null, PayMonth, OSalEarnDedT);
                    EmpOff oEmpOffSave = db.EmpOff.Where(e => e.Id == OEmpOff.Id).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                    if (mESICQualy == "NO")
                    {

                        oEmpOffSave.ESICAppl = false;
                        db.SaveChanges();
                        //OEmpOff.ESICAppl= false;
                    }
                    else if (mESICQualy == "YES")
                    {
                        // OEmpOff.ESICAppl = true;

                        oEmpOffSave.ESICAppl = true;
                        db.SaveChanges();
                    }


                    if (oEmpOffSave.ESICAppl == true)//OEmpOff.ESICAppl
                    {
                        //ESICTransT OESICTransT = new ESICTransT();
                        OESICTransT = ESICcalc(OCompESICMaster, OEmpSalStruct, OEmployeePayrollId, null, PayMonth, OSalEarnDedT);
                        if (OESICTransT != null)
                        {
                            double CalAmount = 0;
                            CalAmount = OESICTransT.EmpAmt;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpESICHead.SalaryHead, CalAmount);
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                // if Attendance 0 yearly payment can pay if appear in payslip
                                if (CalAmount > 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpESICHead.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpESICHead.SalaryHead);
                                }

                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpESICHead.SalaryHead);
                            }
                        }
                    }

                }//last for saleffect
                // Utility.DumpProcessStatus(LineNo: 2512);



                //add official data to OSalaryT object
                OSalaryT.PayMonth = PayMonth;
                OSalaryT.PayMode = OEmpOff.PayMode == null ? null : OEmpOff.PayMode;
                OSalaryT.AccType = OEmpOff.AccountType == null ? null : OEmpOff.AccountType;
                OSalaryT.ProcessDate = DateTime.Now;
                OSalaryT.AccNo = OEmpOff.AccountNo == null ? null : OEmpOff.AccountNo;
                OSalaryT.PaymentBranch = OEmpOff.Branch == null ? null : OEmpOff.Branch;
                //Utility.DumpProcessStatus(LineNo: 2513);
                //EmpSalStruct mEmpSalStruct = db.EmpSalStruct
                // //.Include(e => e.GeoStruct)
                // //.Include(e => e.FuncStruct)
                // //.Include(e => e.PayStruct)
                // //.Include(e => e.PayStruct.JobStatus)
                // //.Include(e => e.EmpSalStructDetails)
                // //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment))
                // //.Include(e => e.EmpSalStructDetails.Select(r => r.PayScaleAssignment.PayScaleAgreement))
                // .Where(e => e.Id == OEmpSalStruct.Id).SingleOrDefault();

                //  Utility.DumpProcessStatus(LineNo: 2524);

                OSalaryT.PayScaleAgreement = db.PayScaleAgreement.Find(OEmpSalStruct.EmpSalStructDetails.FirstOrDefault().PayScaleAssignment.PayScaleAgreement_Id);//mEmpSalStruct.EmpSalStructDetails.FirstOrDefault().PayScaleAssignment.PayScaleAgreement;
                OSalaryT.FinnanceYearId = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).SingleOrDefault();
                OSalaryT.Geostruct = db.GeoStruct.Find(OEmpSalStruct.GeoStruct_Id);
                OSalaryT.PayStruct = db.PayStruct.Find(OEmpSalStruct.PayStruct_Id);
                OSalaryT.FuncStruct = db.FuncStruct.Find(OEmpSalStruct.FuncStruct_Id);
                OSalaryT.JobStatus = db.JobStatus.Find(OSalaryT.PayStruct.JobStatus_Id);  //verify
                //Utility.DumpProcessStatus(LineNo: 2532);



                //save total salary
                if (OSalEarnDedT != null)
                {
                    db.SalEarnDedT.AddRange(OSalEarnDedT);
                }

                if (OPTaxTransT != null)
                {
                    db.PTaxTransT.Add(OPTaxTransT);
                }
                if (OESICTransT != null)
                {
                    db.ESICTransT.Add(OESICTransT);
                }
                if (OLWFTransT != null)
                {
                    db.LWFTransT.Add(OLWFTransT);
                }
                if (OPFTransT != null)
                {
                    db.PFECRR.Add(OPFTransT);
                }
                if (OPerkTransT != null)
                {
                    db.PerkTransT.AddRange(OPerkTransT);
                }

                // Utility.DumpProcessStatus(LineNo: 2571);
                db.SaveChanges();
                OSalaryT.PTaxTransT = OPTaxTransT;
                OSalaryT.PFECRR = OPFTransT;
                OSalaryT.ESICTransT = OESICTransT;
                OSalaryT.LWFTransT = OLWFTransT;
                OSalaryT.SalEarnDedT = OSalEarnDedT;

                OSalaryT.PerkTransT = OPerkTransT;
                //incometax


                double CalAmountTax = 0;
                List<ITProjection> FinalOITProjectionDataList = new List<ITProjection>();
                double TaxPaidAmt = 0;
                ITaxTransT ITaxPaymentT = db.EmployeePayroll.Where(d => d.Id == OEmployeePayrollId)// OEmployeePayroll.ITaxTransT
                   .Select(e => e.ITaxTransT.Where(r => r.PayMonth == PayMonth).FirstOrDefault())
                   .SingleOrDefault();
                if (AutoIncomeTax == true && ITaxPaymentT == null)
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    OITaxTransT = new ITaxTransT()
                    {
                        PayMonth = PayMonth,
                        TaxPaid = TaxPaidAmt,
                        FuncStruct = db.FuncStruct.Find(OEmpSalStruct.FuncStruct_Id),// mEmpSalStruct.FuncStruct,
                        GeoStruct = db.GeoStruct.Find(OEmpSalStruct.GeoStruct_Id),// mEmpSalStruct.GeoStruct,
                        PayStruct = db.PayStruct.Find(OEmpSalStruct.PayStruct_Id),//mEmpSalStruct.PayStruct,
                        DBTrack = dbt,
                        Mode = "AUTO"
                    };
                    CalAmountTax = TaxPaidAmt;
                }
                else
                {
                    if (ITaxPaymentT != null) //&& added instead of || (Rekha)
                    {
                        CalAmountTax = ITaxPaymentT.TaxPaid;
                    }

                }
                EmpSalStructDetails EmpITHead = OEmpSalStruct.EmpSalStructDetails
                   .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
                if (EmpITHead == null)
                {
                    return 4;//define IT salary head
                }
                else
                {
                    CalAmountTax = Process.SalaryHeadGenProcess.RoundingFunction(EmpITHead.SalaryHead, CalAmountTax);
                    // if gross income available then deduction will apply not check attendance
                    if (Grossearnamt == 0)
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpITHead.SalaryHead);
                    }
                    else
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmountTax, 0, EmpITHead.SalaryHead);
                    }
                }



                FinalSalaryUpdate(OCompanyPayroll, null, OEmpSalStruct, null, PayMonth, OSalaryT, OSalEarnDedT, null, 1);
                // FinalSalaryUpdate(OCompanyPayroll, null, OEmpSalStruct, OEmployeePayroll, null, PayMonth, OSalaryT, OSalEarnDedT, null, 1);
                // Utility.DumpProcessStatus(LineNo: 2536);

                if (OITaxTransT != null)
                {
                    db.ITaxTransT.Add(OITaxTransT);
                }
                db.SaveChanges();
                OSalaryT.ITaxTransT = OITaxTransT;

                DBTrack dbtrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                OSalaryT.DBTrack = dbtrack;
                db.SalaryT.Add(OSalaryT);
                db.SaveChanges();

                EmployeePayroll OEmployeePayroll1 = db.EmployeePayroll.Include(e => e.SalaryT).Include(e => e.ITaxTransT).Where(e => e.Id == OEmployeePayrollId).SingleOrDefault();
                List<SalaryT> SalT = new List<SalaryT>();
                if (OEmployeePayroll1.SalaryT != null)
                {
                    SalT.AddRange(OEmployeePayroll1.SalaryT);
                }
                SalT.Add(OSalaryT);
                List<ITaxTransT> ITaxT = new List<ITaxTransT>();
                if (OEmployeePayroll1.ITaxTransT != null)
                {
                    ITaxT.AddRange(OEmployeePayroll1.ITaxTransT);
                }
                ITaxT.Add(OITaxTransT);

                OEmployeePayroll1.SalaryT = SalT;
                OEmployeePayroll1.ITaxTransT = ITaxT;
                db.EmployeePayroll.Attach(OEmployeePayroll1);
                db.Entry(OEmployeePayroll1).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                // Salaryt id in loan repaymentT Start
                var SalarytId = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == PayMonth).FirstOrDefault();
                var OEmpLoanSal = new EmployeePayroll();
                OEmpLoanSal = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                List<LoanAdvRequest> LoanAdvRequestSal = new List<LoanAdvRequest>();
                var LoanAdvRequestObjSal = new LoanAdvRequest();
                LoanAdvRequestSal = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.IsActive == true)
                    .Include(e => e.LoanAdvRepaymentT)
                    .ToList();
                foreach (var i in LoanAdvRequestSal)
                {

                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).FirstOrDefault();
                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                    var SalaryHeadL = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).FirstOrDefault();
                    List<LoanAdvRepaymentT> LoanAdvRepaymentTList = new List<LoanAdvRepaymentT>();

                    //  var LoanAdvRepaymentT = db.LoanAdvRepaymentT.Where(e => e.LoanAdvRequest_Id == i.Id && e.PayMonth == PayMonth).FirstOrDefault();
                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                    i.LoanAdvanceHead = LoanAdvanceHead;
                    i.LoanAdvanceHead.SalaryHead = SalaryHeadL;
                    //if (LoanAdvRepaymentT!=null)
                    //{
                    //    LoanAdvRepaymentTList.Add(LoanAdvRepaymentT);
                    //    i.LoanAdvRepaymentT = (LoanAdvRepaymentTList);
                    //}


                }
                OEmpLoanSal.LoanAdvRequest = LoanAdvRequestSal;

                List<LoanAdvRequest> OLoanAdvReuestSal = OEmpLoanSal.LoanAdvRequest.ToList();

                if (OLoanAdvReuestSal != null && OLoanAdvReuestSal.Count() > 0)
                {
                    OLoanAdvReuestSal.Where(x => x.LoanAdvRepaymentT != null).ToList().ForEach(x =>
                    {
                        LoanAdvRepaymentT OLoanRepay = x.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                        if (OLoanRepay != null)
                        {
                            // if gross income available then deduction will apply not check attendance
                            if (Grossearnamt == 0)
                            {

                            }
                            else
                            {
                                OLoanRepay.SalaryT = SalarytId;
                                db.LoanAdvRepaymentT.Attach(OLoanRepay);
                                db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                        }
                    });
                }

                // Salaryt id in loan repaymentT End 
                //db.Entry(OEmployeePayroll1).State = System.Data.Entity.EntityState.Detached;

                //Pdcc Code Start
                if (SalarytId != null)
                {
                    SalaryT OSalaryTChkpdcc = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.Id == SalarytId.Id).SingleOrDefault();
                    var CompId = Convert.ToInt32(SessionManager.CompanyId);

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

                    if (OCompanyPayroll.Company.Code == FileCompCode)
                    {
                        var OFinancia = OCompanyPayroll.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                        String mPeriodRange = "";
                        List<string> mPeriod = new List<string>();
                        DateTime FromPeriod = Convert.ToDateTime(OFinancia.FromDate);
                        DateTime ToPeriod = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1);
                        DateTime mEndDate = ToPeriod.Date;
                        for (DateTime mTempDate = FromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
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

                        int OFinancialYear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;
                        var loanadvancesalhead = db.LoanAdvanceHead.Include(x => x.LoanAdvancePolicy).Include(e => e.SalaryHead).ToList();
                        foreach (var ca in loanadvancesalhead)
                        {
                            if (ca.SalaryHead != null && ca.LoanAdvancePolicy.Count() > 0)
                            {

                                var OLoanReq = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Include(e => e.LoanAdvanceHead).Include(e => e.LoanAdvanceHead.LoanAdvancePolicy)
                                    .Where(e => e.EmployeePayroll_Id == OSalaryTChkpdcc.EmployeePayroll_Id && e.LoanAdvanceHead.SalaryHead_Id == ca.SalaryHead.Id).ToList();
                                if (OLoanReq.Count() > 0)
                                {
                                    foreach (var loanreqid in OLoanReq)
                                    {
                                        var loanrepaymentsalarymonth = loanreqid.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).FirstOrDefault();
                                        if (loanrepaymentsalarymonth != null)
                                        {

                                            //double MonthlyInterest = loanreqid.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth && e.SalaryT_Id == SalarytId.Id).FirstOrDefault().MonthlyInterest;
                                            //double MonthlyPrinciple = loanreqid.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth && e.SalaryT_Id == SalarytId.Id).FirstOrDefault().MonthlyPricipalAmount;
                                            double MonthlyInterest = loanreqid.LoanAdvRepaymentT.Where(e => mPeriod.Contains(e.PayMonth)).Select(a => a.MonthlyInterest).ToList().Sum();
                                            double MonthlyPrinciple = loanreqid.LoanAdvRepaymentT.Where(e => mPeriod.Contains(e.PayMonth)).Select(a => a.MonthlyPricipalAmount).ToList().Sum();
                                            double ActualInterestSettlement = 0, ActualPrincipleSettlement = 0;
                                            ActualInterestSettlement = db.LoanAdvRepaymentTSettlement.Where(t => t.LoanAdvRequest_Id == loanreqid.Id && t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mEndDate && t.RepaymentDate != null).Select(a => a.MonthlyInterest).ToList().Sum();
                                            ActualPrincipleSettlement = db.LoanAdvRepaymentTSettlement.Where(t => t.LoanAdvRequest_Id == loanreqid.Id && t.InstallementDate >= OFinancia.FromDate && t.InstallementDate <= mEndDate && t.RepaymentDate != null).Select(a => a.MonthlyPricipalAmount).ToList().Sum();

                                            // Principle
                                            var OEmpITInvestment = db.ITInvestmentPayment.Where(e => e.EmployeePayroll_Id == OSalaryTChkpdcc.EmployeePayroll_Id).Select(z => new
                                            {
                                                Id = z.Id,
                                                FinancialYearId = z.FinancialYear_Id,
                                                LoanAdvanceHeadSalaryHead_Id = z.LoanAdvanceHead.SalaryHead_Id,
                                                ITSectionListTypeLookupVal = z.ITSection.ITSectionListType.LookupVal,
                                            }).Where(e => e.FinancialYearId == OFinancialYear && e.ITSectionListTypeLookupVal.ToUpper() == "REBATE").ToList();

                                            var OEmpSalInvestmentChk = OEmpITInvestment.Where(e => e.LoanAdvanceHeadSalaryHead_Id == ca.SalaryHead_Id).FirstOrDefault();

                                            if (OEmpSalInvestmentChk != null)
                                            {

                                                ITInvestmentPayment OEmpSalInvestmentObj = db.ITInvestmentPayment.Where(a => a.Id == OEmpSalInvestmentChk.Id).FirstOrDefault();

                                                OEmpSalInvestmentObj.ActualInvestment = MonthlyPrinciple + ActualPrincipleSettlement;

                                                db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }

                                            // interest
                                            var OEmpITSection24EmpData = db.ITSection24Payment.Where(e => e.EmployeePayroll_Id == OSalaryTChkpdcc.EmployeePayroll_Id).Select(r => new
                                            {
                                                Id = r.Id,
                                                FinancialYearId = r.FinancialYear_Id,
                                                LoanAdvanceHeadId = r.LoanAdvanceHead_Id,
                                                SalaryHeadId = r.LoanAdvanceHead.SalaryHead_Id,
                                                ITSectionListTypeLookupVal = r.ITSection.ITSectionListType.LookupVal,

                                            }).ToList();

                                            CompanyPayroll OIncomeTax = IncomeTaxCalc._returnCompanyPayroll_IncomeTax_New(Convert.ToInt32(SessionManager.CompanyId), OFinancialYear);
                                            IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == OFinancialYear).FirstOrDefault();
                                            List<ITSection> OITSection24 = OITMaster.ITSection.Where(e => e.ITSectionList.LookupVal.ToUpper() == "SECTION24" && e.ITSectionListType.LookupVal.ToUpper() == "LOAN").ToList();
                                            List<LoanAdvanceHead> OITInvestmentsList = OITSection24.SelectMany(e => e.LoanAdvanceHead.Where(a => a.SalaryHead != null && a.Id == ca.Id && a.ITLoan == null)).ToList();
                                            var OEmpIT24Investment = OEmpITSection24EmpData.Where(e => e.FinancialYearId == OFinancialYear && e.ITSectionListTypeLookupVal.ToUpper() == "LOAN" && e.LoanAdvanceHeadId == ca.Id).ToList();

                                            foreach (LoanAdvanceHead OITInvestments1 in OITInvestmentsList)
                                            {
                                                var OEmpSalInvestmentChki = OEmpIT24Investment.Where(e => e.SalaryHeadId == OITInvestments1.SalaryHead.Id).FirstOrDefault();

                                                if (OEmpSalInvestmentChk != null)
                                                {
                                                    ITSection24Payment OEmpSalInvestmentObj = db.ITSection24Payment.Where(a => a.Id == OEmpSalInvestmentChki.Id).FirstOrDefault();

                                                    OEmpSalInvestmentObj.ActualInterest = MonthlyInterest + ActualInterestSettlement;
                                                    db.Entry(OEmpSalInvestmentObj).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                            }





                                        }
                                    }
                                }
                            }

                        }


                    }

                }

                //Pdcc Code End

                return 0;
            }
            //}
            //catch (Exception e)
            //{
            //    return e.Message;
            //   // throw;
            //}

        }
        public static void EmployeeArrearProcess(int OEmployeePayrollId, string PayMonth, bool AutoIncomeTax, int OCompanyPayrollID, SalaryArrearT OSalArrearT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                double SalAttendanceT_monthDays = 0;
                double SalAttendanceT_PayableDays = 0;
                db.Database.CommandTimeout = 3600;
                ////var OEmployeePayroll = db.EmployeePayroll
                ////                          .Include(e => e.Employee)
                ////                          .Include(e => e.Employee.EmpName)
                ////                          .Include(e => e.Employee.Gender)
                ////                          .Include(e => e.Employee.MaritalStatus)
                ////                          .Include(e => e.Employee.ServiceBookDates)
                ////    //.Include(e => e.Employee.EmpOffInfo)
                ////    //.Include(e => e.Employee.EmpOffInfo.Branch)
                ////    //.Include(e => e.Employee.EmpOffInfo.AccountType)
                ////    //.Include(e => e.Employee.EmpOffInfo.PayMode)

                ////                          //.Include(e => e.Employee.EmpOffInfo.NationalityID)
                ////                          .Include(e => e.EmpSalStruct)

                ////                         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.Type)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.PayScaleAssignment)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.PayScaleAssignment.SalHeadFormula)))
                ////                          .Include(e => e.SalaryT.Select(r => r.PFECRR))
                ////                          .Include(e => e.SalaryT)
                ////                          .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                ////                          .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead)))

                ////                           .Where(d => d.Id == OEmployeePayrollId).SingleOrDefault();

                var OCompanyPayroll = db.CompanyPayroll
                                        .Include(e => e.Company)
                                         .Include(e => e.PFMaster)
                                         .Include(e => e.PTaxMaster)
                                         .Include(e => e.LWFMaster)
                                         .Include(e => e.LWFMaster.Select(t => t.LWFStates))
                                         .Include(e => e.LWFMaster.Select(t => t.LWFStatutoryEffectiveMonths))
                                         .Include(e => e.ESICMaster)
                                         .Include(e => e.PTaxMaster.Select(a => a.States))
                                          .Where(d => d.Id == OCompanyPayrollID).FirstOrDefault();

                var OEmpOff = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId)
                                          .Select(e => new
                                          {
                                              PFAppl = e.Employee.EmpOffInfo.PFAppl,
                                              LWFAppl = e.Employee.EmpOffInfo.LWFAppl,
                                              ESICAppl = e.Employee.EmpOffInfo.ESICAppl,
                                              PTAppl = e.Employee.EmpOffInfo.PTAppl,
                                              Branch = e.Employee.EmpOffInfo.Branch,
                                              AccountType = e.Employee.EmpOffInfo.AccountType,
                                              AccountNo = e.Employee.EmpOffInfo.AccountNo,
                                              PayMode = e.Employee.EmpOffInfo.PayMode,
                                              UANNo = e.Employee.EmpOffInfo.NationalityID.UANNo,
                                              PFTrust_EstablishmentId = e.Employee.EmpOffInfo.PFTrust_EstablishmentId
                                              //PFNo=e.Employee.EmpOffInfo.NationalityID.PFNo
                                              //PFNo=e.Employee.EmpOffInfo.NationalityID.PFNo
                                              //PFNo=e.Employee.EmpOffInfo.NationalityID.PFNo
                                              //PFNo=e.Employee.EmpOffInfo.NationalityID.PFNo
                                              //PFNo=e.Employee.EmpOffInfo.NationalityID.PFNo
                                              //PFNo=e.Employee.EmpOffInfo.NationalityID.PFNo


                                          }).FirstOrDefault();




                //var SalaryDetails = OEmployeePayroll.SalaryT == null ? null : OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).ToList();

                var SalaryDetails = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayrollId)
                    .Include(e => e.PFECRR)
                    .Include(e => e.SalEarnDedT)
                    .Include(e => e.SalEarnDedT.Select(t => t.SalaryHead)).FirstOrDefault();

                //open arreaday master to get increment type
                //check salary process during auto process with alert on payment month
                //pass one by one process month to calculate arrears in OSalareears object



                var OSalArrT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.SalaryArrearT)
                .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPFT)).SingleOrDefault();

                var OSalaryArrearT = OSalArrT.SalaryArrearT == null ? null : OSalArrT.SalaryArrearT.Where(e => e.Id == OSalArrearT.Id).SingleOrDefault();
                /*
                *    Remove Salary arrear PFT Data added by Rohit
               */
                if (OSalaryArrearT.SalaryArrearPFT != null)
                {
                    db.SalaryArrearPFT.Remove(OSalaryArrearT.SalaryArrearPFT);
                }

                double mArrearDay = 0;
                if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "LWP")
                {
                    if (OSalaryArrearT.IsRecovery == true)
                    {
                        mArrearDay = -OSalaryArrearT.TotalDays;
                    }
                    else
                    {
                        mArrearDay = OSalaryArrearT.TotalDays;
                    }
                }
                if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "HOLIDAY")
                {
                    mArrearDay = OSalaryArrearT.TotalDays;
                }
                //attendance data

                //var OSalattendanceT = db.EmployeePayroll.Where(t => t.Id == OEmployeePayrollId)
                //    .Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();

                var OSalattendanceT = db.SalAttendanceT.Where(t => t.EmployeePayroll.Id == OEmployeePayrollId && t.PayMonth == PayMonth).SingleOrDefault();

                if (OSalattendanceT != null)
                {
                    SalAttendanceT_monthDays = OSalattendanceT.MonthDays;// SalattendanceT.Select(e => e.MonthDays).SingleOrDefault();
                    if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "LWP")
                    {
                        //  if lwp process then arrear paid day not require as discuss with sir
                        // SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays + mArrearDay;
                        SalAttendanceT_PayableDays = (OSalattendanceT.PaybleDays - OSalattendanceT.LWPDays) + mArrearDay;//LWP Leave process button on manual attendance page Goa urban bank
                    }
                    else if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "HOLIDAY")
                    {
                        //  if Holiday process then arrear paid day not require as discuss with sir
                        // SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays + mArrearDay;
                        SalAttendanceT_PayableDays = (OSalattendanceT.PaybleDays - OSalattendanceT.LWPDays) + mArrearDay;
                    }
                    else
                    {
                        //  if increment,promotion, transfer, process then arrear paid day require as discuss with sir
                        // SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays + OSalattendanceT.ArrearDays + mArrearDay;
                        SalAttendanceT_PayableDays = (OSalattendanceT.PaybleDays - OSalattendanceT.LWPDays) + OSalattendanceT.ArrearDays + mArrearDay;
                    }
                }
                else
                {
                    //attendance not available
                }
                //employee  SalaryStructure checking
                //var EmpSalStructTotal = OEmployeePayroll.EmpSalStruct; //Total salary structure
                //var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EffectiveDate == Convert.ToDateTime("01/" + PayMonth).Date).SingleOrDefault(); //single salary structure

                ////                         .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.Type)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.PayScaleAssignment)))
                ////                          .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(q => q.PayScaleAssignment.SalHeadFormula)))

                //start code 
                //var QEmpSalStruct1 = db.EmpSalStruct//.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                //                           .Include(e => e.EmpSalStructDetails)
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LeaveDependPolicy))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LeaveDependPolicy.Select(y => y.LvHead)))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalHeadFormula))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.ProcessType))
                //                           .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.LvHead));//.ToList();
                //var comparedate = (Convert.ToDateTime("01/" + PayMonth).Date);
                //var EmpSalStructTotal = QEmpSalStruct1.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.EffectiveDate >= comparedate)
                //                            .ToList();
                //end code

                var comparedate = (Convert.ToDateTime("01/" + PayMonth).Date);
                var compareenddate = (Convert.ToDateTime(SalAttendanceT_monthDays + "/" + PayMonth).Date);
                List<EmpSalStruct> EmpSalStructTotal = new List<EmpSalStruct>();
                List<EmpSalStructDetails> EmpSalStructDetails = new List<EmpSalStructDetails>();
                List<PayScaleAssignment> PayScaleAssignment = new List<PayScaleAssignment>();
                var PayScaleAssignmentObj = new PayScaleAssignment();
                List<SalaryHead> SalaryHead = new List<SalaryHead>();
                var SalaryHeadObj = new SalaryHead();
                List<SalHeadFormula> SalHeadFormula = new List<SalHeadFormula>();
                var SalaryHeadFormulaObj = new SalHeadFormula();

                EmpSalStructTotal = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.EffectiveDate.Value >= comparedate && e.EffectiveDate.Value <= compareenddate).ToList();
                foreach (var i in EmpSalStructTotal)
                {
                    //  EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                    EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id && e.SalHeadFormula_Id > 0).ToList();
                    var x = db.EmpSalStructDetails.Where(e =>
                    e.EmpSalStruct.Id == i.Id && (e.SalHeadFormula_Id == 0 || e.SalHeadFormula_Id == null) && (e.Amount > 0 && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR")
                    ).ToList();
                    if (x.Count > 0)
                    { EmpSalStructDetails.AddRange(x); }
                    var xx = db.EmpSalStructDetails.Where(e =>
                    e.EmpSalStruct.Id == i.Id && (e.SalHeadFormula_Id == 0 || e.SalHeadFormula_Id == null) && (e.Amount > 0 && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR")
                    ).ToList();
                    if (xx.Count > 0)
                    { EmpSalStructDetails.AddRange(xx); }
                    var xxx = db.EmpSalStructDetails.Where(e =>
                                        e.EmpSalStruct.Id == i.Id && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "REGULAR"
                                        && (e.SalHeadFormula_Id == 0 || e.SalHeadFormula_Id == null)
                                        ).ToList();
                    if (xxx.Count > 0)
                    { EmpSalStructDetails.AddRange(xxx); }
                    foreach (var j in EmpSalStructDetails)
                    {
                        var SalHeadTmp = new SalaryHead();
                        j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                        j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                        j.SalaryHead = db.SalaryHead.Include(e => e.LeaveDependPolicy).Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                        //var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();
                        var SalHeadData = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id)
                            .Select(r => new
                            {
                                Id = r.Id,
                                SalHeadOperationType = r.SalHeadOperationType,
                                Frequency = r.Frequency,
                                Type = r.Type,
                                RoundingMethod = r.RoundingMethod,
                                ProcessType = r.ProcessType,
                                LvHead = r.LvHead

                            }).FirstOrDefault();
                        j.SalaryHead.SalHeadOperationType = SalHeadData.SalHeadOperationType;
                        j.SalaryHead.Frequency = SalHeadData.Frequency;
                        j.SalaryHead.Type = SalHeadData.Type;
                        j.SalaryHead.RoundingMethod = SalHeadData.RoundingMethod;
                        j.SalaryHead.ProcessType = SalHeadData.ProcessType;
                        //SalHeadTmp.LeaveDependPolicy = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LeaveDependPolicy).FirstOrDefault();
                        foreach (var item in j.SalaryHead.LeaveDependPolicy)
                        {
                            item.LvHead = db.LeaveDependPolicy.Where(e => e.Id == item.Id).Select(t => t.LvHead).FirstOrDefault();
                        }


                    }
                    i.EmpSalStructDetails = EmpSalStructDetails;
                }



                //var EmpSalStructTotal = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId)
                //    .Include(e => e.EmpSalStructDetails)
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead))
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Frequency))
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.Type))
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.RoundingMethod))
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.SalaryHead.SalHeadOperationType))
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.PayScaleAssignment))
                //    .Include(e => e.EmpSalStructDetails.Select(q => q.PayScaleAssignment.SalHeadFormula)).ToList();

                //   var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EffectiveDate == Convert.ToDateTime("01/" + PayMonth).Date).SingleOrDefault();
                var OEmpSalStruct = EmpSalStructTotal.Where(e => e.EffectiveDate.Value.Date >= (Convert.ToDateTime("01/" + PayMonth).Date) && e.EffectiveDate.Value.Date <= (Convert.ToDateTime(SalAttendanceT_monthDays + "/" + PayMonth).Date)).OrderBy(r => r.Id).FirstOrDefault();
                if (OEmpSalStruct == null)
                {
                    //no salary structure available
                }
                //New Salary Saving Object


                SalaryT OSalaryT = new SalaryT(); //salary record store
                ESICTransT OESICTransT = new ESICTransT();
                OESICTransT = null;
                LWFTransT OLWFTransT = new LWFTransT();
                OLWFTransT = null;
                PTaxTransT OPTaxTransT = new PTaxTransT();
                OPTaxTransT = null;
                PFECRR OPFTransT = new PFECRR();
                OPFTransT = null;
                OSalaryT.PaybleDays = SalAttendanceT_PayableDays;
                OSalaryT.TotalDays = SalAttendanceT_monthDays;
                //add salary details into the list



                List<SalEarnDedT> OSalEarnDedT = new List<SalEarnDedT>();

                if (SalAttendanceT_PayableDays != 0) //no zero attendance then calculate salary
                {
                    //if (Convert.ToDateTime("01/" + PayMonth).Date >= OEmpSalStruct.EffectiveDate.Value.Date)//Normal Salary for regular monthly and daily/hourly salary components
                    //{
                    //    OSalEarnDedT = EarnSalHeadMDProcess(OEmpSalStruct, OSalEarnDedT, OEmployeePayrollId, SalAttendanceT_PayableDays, SalAttendanceT_monthDays, PayMonth);
                    //}
                    // emp struct effective date check over
                    // else
                    //  {//check for multiple salary structures 
                    //present assumption => payable days will be diff between end and start date or 1st of month. LWP will be adjusted in last structure payable days
                    //perfect solution => get attendance data from attendance muster
                    List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructure = new List<SalEarnDedTMultiStructure>();
                    double mPayDaysTotal = 0;
                    double mPayDaysRunning = 0;
                    // var mEmpSalStructTotal = EmpSalStructTotal.Where(e => e.EffectiveDate.Value.Date >= (Convert.ToDateTime("01/" + PayMonth).Date)).ToList();
                    var mEmpSalStructTotal = EmpSalStructTotal.Where(e => e.EffectiveDate.Value.Date >= (Convert.ToDateTime("01/" + PayMonth).Date) && e.EffectiveDate.Value.Date <= (Convert.ToDateTime(SalAttendanceT_monthDays + "/" + PayMonth).Date))
                      .OrderBy(r => r.Id)
                      .ToList();

                    foreach (var OMultiEmpStruct in mEmpSalStructTotal)
                    {
                        if (OMultiEmpStruct.EffectiveDate.Value.Date < Convert.ToDateTime("01/" + PayMonth).Date)
                        {
                            mPayDaysRunning = (OMultiEmpStruct.EndDate.Value.Date - Convert.ToDateTime("01/" + PayMonth).Date).Days + 1; //check for date result else add 1 day in result
                            if (SalAttendanceT_PayableDays <= mPayDaysRunning)
                            {
                                mPayDaysRunning = SalAttendanceT_PayableDays;
                            }

                            mPayDaysTotal = mPayDaysTotal + mPayDaysRunning;
                        }
                        else
                        {
                            if (OMultiEmpStruct.EndDate != null)
                            {
                                mPayDaysRunning = (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1; //check for date result else add 1 day in result

                                if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "HOLIDAY" && mEmpSalStructTotal.LastOrDefault().Id == OMultiEmpStruct.Id)
                                {
                                    if (SalAttendanceT_PayableDays > SalAttendanceT_monthDays)
                                    {
                                        mPayDaysRunning = mPayDaysRunning + SalAttendanceT_PayableDays - SalAttendanceT_monthDays;
                                    }
                                }
                                //mPayDaysTotal = mPayDaysTotal + (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                mPayDaysTotal = mPayDaysTotal + mPayDaysRunning;

                                if (mPayDaysTotal > SalAttendanceT_PayableDays)
                                {
                                    mPayDaysRunning = SalAttendanceT_PayableDays - (mPayDaysTotal - mPayDaysRunning);


                                    if (mPayDaysRunning < 0)
                                    {
                                        mPayDaysRunning = 0;
                                    }
                                }

                            }
                            else
                            {
                                mPayDaysRunning = SalAttendanceT_monthDays - mPayDaysTotal;
                            }
                        }
                        //cal salgen process on derived empsalstruct
                        OSalEarnDedTMultiStructure = EarnSalHeadMDProcessMS(OMultiEmpStruct, OSalEarnDedTMultiStructure, OEmployeePayrollId, mPayDaysRunning, SalAttendanceT_monthDays, PayMonth);

                    }
                    //combined the output data and dump in SalEarnDedT ==== check query  for duplicate record
                    //var OSalEarnDedTMulti = OSalEarnDedTMultiStructure
                    //                        .GroupBy(e => e.SalaryHead);

                    var OSalEarnDedTMulti = OSalEarnDedTMultiStructure.GroupBy(x => new { x.SalaryHead })
                                               .Select(g => new
                                               {
                                                   SalaryHead = g.Key.SalaryHead,
                                                   StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                   TotalAmount = g.Sum(x => x.Amount),
                                                   Count = g.Count()
                                               }).ToList();

                    foreach (var ca in OSalEarnDedTMulti)
                    {
                        // SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays;// if middle date activity 15 and employee present 13 days 
                        if (SalAttendanceT_PayableDays == 0)
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, ca.StdAmount, ca.SalaryHead);
                        }
                        else
                        {
                            double Amt = 0;
                            if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED")
                            {
                                SalHeadFormula OSalHeadForm = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id).FirstOrDefault().SalHeadFormula;
                                if (OSalHeadForm != null)
                                {
                                    Amt = Process.SalaryHeadGenProcess.CellingCkeck(OSalHeadForm, ca.TotalAmount);
                                }
                            }
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "OFFICIATING")
                            {
                                EmployeePayroll OEmpPayrolloff = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                Amt = OfficiatingServiceBookController.officiateprocess(OEmpPayrolloff, PayMonth, Amt, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                            }
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PFWAGESOFFICIATING")
                            {
                                EmployeePayroll OEmpPayrolloff = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                Amt = OfficiatingServiceBookController.officiateprocess(OEmpPayrolloff, PayMonth, Amt, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                            }
                            if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "SUPANNOFFICIATING")
                            {
                                EmployeePayroll OEmpPayrolloff = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                Amt = OfficiatingServiceBookController.officiateprocess(OEmpPayrolloff, PayMonth, Amt, ca.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper());

                            }

                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, (Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, Amt == 0 ? ca.TotalAmount : Amt)), ca.StdAmount, ca.SalaryHead);
                            //OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, ca.TotalAmount, ca.StdAmount, ca.SalaryHead);
                            //  OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, (Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, ca.TotalAmount)), ca.StdAmount, ca.SalaryHead);
                        }
                        //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                        //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                        //{
                        //    Amount = ca.TotalAmount,
                        //    StdAmount = ca.StdAmount,
                        //    SalaryHead = ca.SalaryHead,
                        //    DBTrack = dbt

                        //};
                        //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                    }

                    //OSalEarnDedT.AddRange(OSalEarnDedTMulti);

                    // }

                    // Function Attendance start
                    List<SalEarnDedT> OSalEarnDedTFunc = new List<SalEarnDedT>();
                    List<FunctAttendanceT> EmpFuncT = db.FunctAttendanceT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && e.PayMonth == PayMonth && e.isCancel == false && e.TrClosed == true)
                            .Include(e => e.EmpSalStruct)
                            .Include(e => e.SalaryHead)
                            .Include(e => e.SalaryHead.Type)
                            .Include(e => e.SalaryHead.Frequency)
                           .Include(e => e.SalaryHead.SalHeadOperationType)
                            .OrderBy(e => e.Id).ToList();

                    if (OEmpSalStruct.EndDate != null)
                    {
                        if (EmpFuncT != null && EmpFuncT.Count() > 0)
                        {
                            #region foreach
                            Utility.DumpProcessStatus(LineNo: 1517);

                            foreach (FunctAttendanceT ca in EmpFuncT)
                            {
                                EmpSalStruct EmpSalaryT = EmpSalStructTotal.Where(e => e.Id == ca.EmpSalStruct.Id).SingleOrDefault();
                                //  EmpSalStruct EmpSalaryT = EmpSalStructTotal.Where(e => e.Id == ca.EmpSalStruct.Id).SingleOrDefault();

                                if (EmpSalaryT != null)
                                {

                                    Utility.DumpProcessStatus(LineNo: 1601);

                                    var mAttend1 = db.EmployeePayroll.Where(r => r.Id == OEmployeePayrollId)
                                        .Select(t => t.SalAttendance.Where(e => e.PayMonth == ca.ProcessMonth)).SingleOrDefault();
                                    SalAttendanceT mAttend = mAttend1.FirstOrDefault();

                                    var EmpHDhead = EmpSalaryT.EmpSalStructDetails
                                    .Where(e => e.SalaryHead.Id == ca.SalaryHead.Id)
                                    .Select(m => new { Amount = m.Amount, SalaryHead = m.SalaryHead, HourDays = ca.HourDays }).FirstOrDefault();
                                    if (EmpHDhead != null && ca.EmpSalStruct.Id == EmpSalaryT.Id)
                                    {

                                        double CalAmount = 0;
                                        if (ca.SalaryHead.OnAttend == true)
                                        {


                                            if (ca.SalaryHead.Frequency.LookupVal.ToUpper() == "DAILY")
                                            {
                                                CalAmount = EmpHDhead.Amount * ((ca.HourDays) / (mAttend.MonthDays));
                                            }
                                            else
                                            {
                                                CalAmount = EmpHDhead.Amount * (ca.HourDays);
                                            }
                                        }
                                        else
                                        {
                                            CalAmount = EmpHDhead.Amount;
                                        }
                                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                                        if (SalAttendanceT_PayableDays == 0)
                                        {
                                            OSalEarnDedTFunc = SaveSalayDetails(OSalEarnDedTFunc, 0, EmpHDhead.Amount, ca.SalaryHead);

                                        }
                                        else
                                        {
                                            OSalEarnDedTFunc = SaveSalayDetails(OSalEarnDedTFunc, CalAmount, EmpHDhead.Amount, ca.SalaryHead);
                                        }
                                    }
                                    //}
                                }


                            }//foreach
                            #endregion foreach

                        }

                    }

                    var OSalEarnDedFunc = OSalEarnDedTFunc.GroupBy(x => new { x.SalaryHead })
                                              .Select(g => new
                                              {
                                                  SalaryHead = g.Key.SalaryHead,
                                                  StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                  TotalAmount = g.Sum(x => x.Amount),
                                                  Count = g.Count()
                                              }).ToList();

                    foreach (var ca in OSalEarnDedFunc)
                    {

                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, ca.TotalAmount, ca.StdAmount, ca.SalaryHead);


                    }

                    // Function Attendance end

                    //Other irregular earnings
                    //var OOthEarnT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.OtherEarningDeductionT)
                    //    .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead))
                    //    .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead.Type))
                    //    .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead.Frequency))
                    //    .Include(e => e.OtherEarningDeductionT.Select(r => r.SalaryHead.SalHeadOperationType)).SingleOrDefault();

                    var OthEarningT = db.OthEarningDeductionT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId)
                         .Include(e => e.SalaryHead)
                        .Include(e => e.SalaryHead.Type)
                        .Include(e => e.SalaryHead.Frequency)
                        .Include(e => e.SalaryHead.SalHeadOperationType)
                            .Where(e => e.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                            && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"
                            && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR"
                            && e.PayMonth == PayMonth)
                            .ToList();
                    if (OthEarningT != null)
                    {
                        var EmpOthEarnhead = OEmpSalStruct.EmpSalStructDetails.
                       Join(OthEarningT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                       (u, uir) => new { u, uir })
                       .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, SalAmount = m.uir.SalAmount }).ToList();
                        foreach (var ca in EmpOthEarnhead)
                        {
                            double CalAmount = ca.SalAmount;
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                            }
                            //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                            //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                            //{
                            //    Amount = CalAmount,
                            //    StdAmount = 0,
                            //    SalaryHead = ca.SalaryHead,
                            //    DBTrack = dbt

                            //};
                            //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                        }
                    }
                    //yearly payments

                    //var OEmpYrlyPayT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).Include(e => e.YearlyPaymentT)
                    //    .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                    //    .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Type))
                    //    .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Frequency)).SingleOrDefault();

                    var YearlyEarnPaymentT = db.YearlyPaymentT.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId)
                        .Include(e => e.SalaryHead)
                        .Include(e => e.SalaryHead.Type)
                        .Include(e => e.SalaryHead.Frequency)
                        .Where(e => e.PayMonth.ToString() == PayMonth
                        && e.SalaryHead.Frequency.LookupVal.ToUpper() == "YEARLY"
                        && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && e.SalaryHead.InPayslip == true && e.ReleaseFlag == true)
                        .ToList();
                    if (YearlyEarnPaymentT != null)
                    {
                        var EmpYearlyEarnhead1 = OEmpSalStruct.EmpSalStructDetails
                                .Join(YearlyEarnPaymentT, u => u.SalaryHead.Id, uir => uir.SalaryHead.Id,
                                (u, uir) => new { u, uir })
                                .Select(m => new { Amount = m.u.Amount, SalaryHead = m.u.SalaryHead, AmountPaid = m.uir.AmountPaid }).ToList();
                        var EmpYearlyEarnhead = EmpYearlyEarnhead1
                                                .GroupBy(a => new { SalaryHead = a.SalaryHead })
                                                .Select(e => new { SalaryHead = e.Key, AmountPaid = e.Select(r => r.AmountPaid).ToList().Sum() })
                                                .ToList();
                        foreach (var ca in EmpYearlyEarnhead)
                        {

                            double CalAmount = ca.AmountPaid;
                            SalaryHead SH = db.SalaryHead.Where(e => e.Id == ca.SalaryHead.SalaryHead.Id).SingleOrDefault();
                            CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(SH, CalAmount);
                            if (SalAttendanceT_PayableDays == 0)
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, SH);
                            }
                            else
                            {
                                OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, SH);
                            }
                            //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                            //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                            //{
                            //    Amount = CalAmount,
                            //    StdAmount = 0,
                            //    SalaryHead = ca.SalaryHead,
                            //    DBTrack = dbt

                            //};
                            //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                        }
                    }
                    //Arrears payments Pending


                    List<Int32> CurSalHead = OSalEarnDedT.Select(r => r.SalaryHead.Id).ToList();
                    var ONonAttHeadList = OEmpSalStruct.EmpSalStructDetails.Where(s => s.SalaryHead.OnAttend == false && s.SalaryHead.Frequency.LookupVal.ToUpper() != "YEARLY" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "NONREGULAR"
                       && CurSalHead.Contains(s.SalaryHead.Id) && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "ARREAREARN" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "ARREARDED" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "OFFEARN" && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "OFFDED")
                      .ToList();
                    foreach (var a in ONonAttHeadList)
                    {
                        double NewAmt = 0;
                        var SalHead = OSalEarnDedT.Where(e => e.SalaryHead.Id == a.SalaryHead.Id).SingleOrDefault();
                        if (SalHead != null)
                        {

                            //List<EmpSalStruct> EmpSalStructTotal = EmpSalStructList.EmpSalStruct.ToList(); //Total salary structure
                            //List<SalEarnDedTMultiStructure> OSalEarnDedTMultiStructure = new List<SalEarnDedTMultiStructure>();
                            double mPayDaysTotalAr = 0;
                            double mPayDaysRunningAr = 0;

                            List<EmpSalStruct> mEmpSalStructTotalAr = EmpSalStructTotal.Where(e => e.EffectiveDate.Value.Date >= (Convert.ToDateTime("01/" + PayMonth).Date) && e.EffectiveDate.Value.Date <= (Convert.ToDateTime(SalAttendanceT_monthDays + "/" + PayMonth).Date))
                                                 .OrderBy(r => r.Id)
                                                 .ToList();
                            foreach (EmpSalStruct OMultiEmpStruct in mEmpSalStructTotalAr)
                            {
                                if (OMultiEmpStruct.EffectiveDate.Value.Date < Convert.ToDateTime("01/" + PayMonth).Date)
                                {
                                    //Utility.DumpProcessStatus(LineNo: 2640);

                                    mPayDaysRunningAr = (OMultiEmpStruct.EndDate.Value.Date - Convert.ToDateTime("01/" + PayMonth).Date).Days + 1; //check for date result else add 1 day in result
                                    mPayDaysTotalAr = mPayDaysTotalAr + mPayDaysRunningAr;
                                    if (a.Amount <= SalHead.Amount)
                                    {
                                        NewAmt = NewAmt + ((a.Amount / SalAttendanceT_monthDays) * mPayDaysRunningAr);
                                    }
                                }
                                else
                                {
                                    //Utility.DumpProcessStatus(LineNo: 2651);

                                    if (OMultiEmpStruct.EndDate != null)
                                    {
                                        //Utility.DumpProcessStatus(LineNo: 2655);

                                        mPayDaysRunningAr = (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1; //check for date result else add 1 day in result
                                        mPayDaysTotalAr = mPayDaysTotalAr + (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;

                                        if (a.Amount <= SalHead.Amount)
                                        {
                                            NewAmt = NewAmt + ((a.Amount / SalAttendanceT_monthDays) * mPayDaysRunningAr);
                                        }
                                    }
                                    else
                                    {
                                        // Utility.DumpProcessStatus(LineNo: 2178);
                                        mPayDaysRunningAr = SalAttendanceT_monthDays - mPayDaysTotalAr;
                                        if (a.Amount <= SalHead.Amount)
                                        {
                                            NewAmt = NewAmt + ((a.Amount / SalAttendanceT_monthDays) * mPayDaysRunningAr);
                                        }
                                    }
                                }
                            }


                            SalHead.Amount = SalaryHeadGenProcess.RoundingFunction(a.SalaryHead, NewAmt);
                        }
                    }

                    //standard monthly deductions
                    List<SalEarnDedT> OSalEarnDedTmultided = new List<SalEarnDedT>();
                    double mPayDaysTotald = 0;
                    double mPayDaysRunningd = 0;
                    foreach (var OMultiEmpStructd in mEmpSalStructTotal)
                    {
                        if (OMultiEmpStructd.EffectiveDate.Value.Date < Convert.ToDateTime("01/" + PayMonth).Date)
                        {
                            mPayDaysRunningd = (OMultiEmpStructd.EndDate.Value.Date - Convert.ToDateTime("01/" + PayMonth).Date).Days + 1; //check for date result else add 1 day in result
                            if (SalAttendanceT_PayableDays <= mPayDaysRunningd)
                            {
                                mPayDaysRunningd = SalAttendanceT_PayableDays;
                            }

                            mPayDaysTotald = mPayDaysTotald + mPayDaysRunningd;
                        }
                        else
                        {
                            if (OMultiEmpStructd.EndDate != null)
                            {
                                mPayDaysRunningd = (OMultiEmpStructd.EndDate.Value.Date - OMultiEmpStructd.EffectiveDate.Value.Date).Days + 1; //check for date result else add 1 day in result

                                if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "HOLIDAY" && mEmpSalStructTotal.LastOrDefault().Id == OMultiEmpStructd.Id)
                                {
                                    if (SalAttendanceT_PayableDays > SalAttendanceT_monthDays)
                                    {
                                        mPayDaysRunningd = mPayDaysRunningd + SalAttendanceT_PayableDays - SalAttendanceT_monthDays;
                                    }
                                }
                                //mPayDaysTotal = mPayDaysTotal + (OMultiEmpStruct.EndDate.Value.Date - OMultiEmpStruct.EffectiveDate.Value.Date).Days + 1;
                                mPayDaysTotald = mPayDaysTotald + mPayDaysRunningd;

                                if (mPayDaysTotald > SalAttendanceT_PayableDays)
                                {
                                    mPayDaysRunningd = SalAttendanceT_PayableDays - (mPayDaysTotald - mPayDaysRunningd);


                                    if (mPayDaysRunningd < 0)
                                    {
                                        mPayDaysRunningd = 0;
                                    }
                                }

                            }
                            else
                            {
                                mPayDaysRunningd = SalAttendanceT_monthDays - mPayDaysTotald;
                            }
                        }
                        // ====

                        var EmpsalDedhead = OMultiEmpStructd.EmpSalStructDetails
                            .Where(s => s.SalaryHead.Frequency.LookupVal.ToUpper() == "MONTHLY"
                                && s.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"
                                && s.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "REGULAR")
                                .OrderBy(e => e.SalaryHead.SeqNo).ToList();
                        if (EmpsalDedhead != null)
                        {
                            foreach (var ca in EmpsalDedhead)
                            {
                                double CalAmount = 0;
                                if (ca.SalaryHead.OnAttend == true)
                                {
                                    //CalAmount = ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                    CalAmount = ca.Amount * ((mPayDaysRunningd) / (SalAttendanceT_monthDays));
                                }
                                else
                                {
                                    CalAmount = ca.Amount;
                                    if (OSalEarnDedTmultided.Count() > 0)// not on attendance and multistructure then
                                    {
                                        var prevheadnotatt = OSalEarnDedTmultided.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id && e.Amount == CalAmount).SingleOrDefault();
                                        if (prevheadnotatt != null)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                if (ca.SalaryHead.ProcessType.LookupVal.ToUpper() == "EARNED" && ca.SalHeadFormula != null)
                                {
                                    List<SalEarnDedT> OSalEarnDedTmultidedU = OSalEarnDedT.Union(OSalEarnDedTmultided).ToList();
                                    EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                                    // CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedT, null, OEmpPayroll, PayMonth);
                                    CalAmount = Process.SalaryHeadGenProcess.SalHeadAmountCalc(ca.SalHeadFormula.Id, OSalEarnDedTmultidedU, null, OEmpPayroll, PayMonth);
                                    if (OSalEarnDedTmultided.Count() > 0)// not on attendance and multistructure then
                                    {
                                        var prevheadnotatt = OSalEarnDedTmultided.Where(e => e.SalaryHead.Id == ca.SalaryHead.Id && e.Amount == CalAmount).SingleOrDefault();
                                        if (prevheadnotatt != null)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                // CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    OSalEarnDedTmultided = SaveSalayDetails(OSalEarnDedTmultided, 0, 0, ca.SalaryHead);
                                    //OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);

                                }
                                else
                                {
                                    OSalEarnDedTmultided = SaveSalayDetails(OSalEarnDedTmultided, CalAmount, 0, ca.SalaryHead);
                                    //  OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, ca.SalaryHead);
                                }
                                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                //{
                                //    Amount = CalAmount,
                                //    StdAmount = 0,
                                //    SalaryHead = ca.SalaryHead,
                                //    DBTrack = dbt

                                //};
                                //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                            }
                        }
                    }

                    var OSalEarnDedTMultidedd = OSalEarnDedTmultided.GroupBy(x => new { x.SalaryHead })
                                              .Select(g => new
                                              {
                                                  SalaryHead = g.Key.SalaryHead,
                                                  StdAmount = (g.Sum(x => x.StdAmount)) / g.Count(),
                                                  TotalAmount = g.Sum(x => x.Amount),
                                                  Count = g.Count()
                                              }).ToList();
                    foreach (var ca in OSalEarnDedTMultidedd)
                    {
                        OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, (Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, ca.TotalAmount)), ca.StdAmount, ca.SalaryHead);
                    }


                    //PF deductions
                    if (OEmpOff != null)
                    {
                        if (OEmpOff.PFAppl == true)
                        {
                            var EmpPFHead = OEmpSalStruct.EmpSalStructDetails
                                        .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault();
                            if (EmpPFHead == null)
                            {
                                //no pf head defined

                            }
                            else
                            {
                                //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EndDate != null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date).SingleOrDefault();
                                //PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e =>e.EstablishmentID==OEmpOff.PFTrust_EstablishmentId && e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date).FirstOrDefault();
                                PFMaster OCompPFMaster = null;
                                foreach (var itemPFmas in OCompanyPayroll.PFMaster.ToList())
                                {
                                    if (itemPFmas.EstablishmentID == OEmpOff.PFTrust_EstablishmentId && (itemPFmas.EndDate == null || itemPFmas.EndDate.Value.Date > Convert.ToDateTime("01/" + PayMonth).Date))
                                    {
                                        OCompPFMaster = itemPFmas;
                                    }
                                }
                                if (OCompPFMaster == null)
                                {
                                    //no pf master
                                }
                                else
                                {
                                    //PFTransT OPFTransT = new PFTransT();
                                    OPFTransT = PFcalcArr(OCompPFMaster, OEmpSalStruct, OEmployeePayrollId, null, PayMonth, OSalEarnDedT, OEmpOff.UANNo);
                                    if (OPFTransT != null)
                                    {
                                        double CalAmount = 0;
                                        CalAmount = Convert.ToDouble(OPFTransT.EE_Share);
                                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpPFHead.SalaryHead, CalAmount);
                                        if (SalAttendanceT_PayableDays == 0)
                                        {
                                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpPFHead.SalaryHead);
                                        }
                                        else
                                        {
                                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpPFHead.SalaryHead);
                                        }
                                        //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                        //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                        //{
                                        //    Amount = CalAmount,
                                        //    StdAmount = 0,
                                        //    SalaryHead = EmpPFHead.SalaryHead,
                                        //    DBTrack = dbt

                                        //};
                                        //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                                    }
                                }
                            }

                        }
                    }
                    //PT deductions

                    var EmpPTHead = OEmpSalStruct.EmpSalStructDetails
                               .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault();
                    if (OEmpOff != null)
                    {
                        if (OEmpOff.PTAppl == true && EmpPTHead != null)
                        {
                            List<PTaxMaster> OCompPTMaster = OCompanyPayroll.PTaxMaster.ToList();


                            OPTaxTransT = PTcalc(OCompPTMaster, OEmpSalStruct, OEmployeePayrollId, null, PayMonth, OSalEarnDedT, SalAttendanceT_PayableDays);
                            if (OPTaxTransT != null)
                            {
                                double CalAmount = 0;

                                CalAmount = OPTaxTransT.PTAmount;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpPTHead.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpPTHead.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpPTHead.SalaryHead);
                                }
                                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                //{
                                //    Amount = CalAmount,
                                //    StdAmount = 0,
                                //    SalaryHead = EmpPTHead.SalaryHead,
                                //    DBTrack = dbt

                                //};
                                //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                            }
                        }
                    }
                    ////LWF deductions
                    var EmpLWFHead = OEmpSalStruct.EmpSalStructDetails
                               .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LWF").SingleOrDefault();
                    if (OEmpOff != null)
                    {
                        if (OEmpOff.LWFAppl == true && EmpLWFHead != null && EmpLWFHead.SalHeadFormula_Id != null)
                        {
                            List<LWFMaster> OCompLWFMaster = OCompanyPayroll.LWFMaster.ToList();
                            //LWFTransT OLWFTransT = new LWFTransT();
                            OLWFTransT = LWFcalc(OCompLWFMaster, OEmpSalStruct, null, PayMonth, OSalEarnDedT);
                            if (OLWFTransT != null)
                            {
                                double CalAmount = 0;
                                CalAmount = OLWFTransT.EmpAmt;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpLWFHead.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpLWFHead.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpLWFHead.SalaryHead);
                                }
                                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                //{
                                //    Amount = CalAmount,
                                //    StdAmount = 0,
                                //    SalaryHead = EmpLWFHead.SalaryHead,
                                //    DBTrack = dbt

                                //};
                                //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                            }
                        }
                    }

                    ////ESIC deductions
                    var EmpESICHead = OEmpSalStruct.EmpSalStructDetails
                               .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault();
                    if (EmpESICHead != null)
                    {
                        List<ESICMaster> OCompESICMaster = OCompanyPayroll.ESICMaster.ToList();
                        var mESICQualy = ESICQualify(OCompESICMaster, OEmpSalStruct, null, PayMonth, OSalEarnDedT);
                        if (mESICQualy == "NO")
                        {
                            // OEmpOff.ESICAppl = false;
                        }
                        else if (mESICQualy == "YES")
                        {
                            // OEmpOff.ESICAppl = true;
                        }


                        if (OEmpOff.ESICAppl == true)
                        {
                            //ESICTransT OESICTransT = new ESICTransT();
                            OESICTransT = ESICcalc(OCompESICMaster, OEmpSalStruct, OEmployeePayrollId, null, PayMonth, OSalEarnDedT);
                            if (OESICTransT != null)
                            {
                                double CalAmount = 0;
                                CalAmount = OESICTransT.EmpAmt;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpESICHead.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpESICHead.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpESICHead.SalaryHead);
                                }
                                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                //{
                                //    Amount = CalAmount,
                                //    StdAmount = 0,
                                //    SalaryHead = EmpESICHead.SalaryHead,
                                //    DBTrack = dbt

                                //};
                                //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                            }
                        }

                    }//last for saleffect

                    //Loan Deductions,
                    //var OLoanAdvReuest = OEmployeePayroll.LoanAdvRequest.Where(e => e.IsActive == true);

                    var OEmpLoan = db.LoanAdvRequest.Where(d => d.EmployeePayroll_Id == OEmployeePayrollId)
                        .Include(e => e.LoanAdvRepaymentT)
                         .Include(e => e.LoanAdvanceHead)
                         .Include(e => e.LoanAdvanceHead.SalaryHead)
                         .FirstOrDefault();

                    var OLoanAdvReuest = db.LoanAdvRequest.Where(d => d.EmployeePayroll_Id == OEmployeePayrollId)
                        .Include(e => e.LoanAdvRepaymentT)
                         .Include(e => e.LoanAdvanceHead)
                         .Include(e => e.LoanAdvanceHead.SalaryHead)
                         .ToList();
                    //var OLoanAdvReuest = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)
                    //    .Include(v => v.LoanAdvRequest.Select(e => e.LoanAdvRepaymentT))
                    //    .Select(e => e.LoanAdvRequest.Where(r => r.IsActive == true))
                    //    .Include(v => v.Select(w => w.LoanAdvRepaymentT))
                    //    .Include(v => v.Select(w => w.LoanAdvanceHead))
                    //    .Include(v => v.Select(w => w.LoanAdvanceHead.SalaryHead))
                    //    .FirstOrDefault();
                    if (OLoanAdvReuest != null && OLoanAdvReuest.Count() > 0)
                    {
                        foreach (var a in OLoanAdvReuest)
                        {
                            var OLoanRepay = a.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                            if (OLoanRepay != null)
                            {
                                OLoanRepay.RepaymentDate = DateTime.Now;
                                OLoanRepay.InstallmentPaid = OLoanRepay.InstallmentAmount;
                                db.LoanAdvRepaymentT.Attach(OLoanRepay);
                                db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                //db.Entry(OLoanRepay).State = System.Data.Entity.EntityState.Detached;
                            }
                        }

                        var EmpLoanHead = OEmpSalStruct.EmpSalStructDetails.
                            Join(OLoanAdvReuest, u => u.SalaryHead.Id, uir => uir.LoanAdvanceHead.SalaryHead.Id,
                            (u, uir) => new { u, uir })
                            .Where(r => r.uir.IsActive == true)
                            .Select(m => new { AdvLoanRepayment = m.uir.LoanAdvRepaymentT, SalaryHead = m.u.SalaryHead })
                            .ToList();
                        var EmpLoanRepay = EmpLoanHead.Select(e => e.AdvLoanRepayment
                             .Where(m => m.PayMonth == PayMonth)
                             .Select(s => new { Amount = s.InstallmentAmount, SalaryHead = e.SalaryHead, AmountPaid = s.InstallmentPaid }))
                             .ToList();
                        foreach (var ca1 in EmpLoanRepay)
                        {
                            foreach (var ca in ca1)
                            {
                                double CalAmount = 0;
                                CalAmount = ca.AmountPaid;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, ca.Amount, ca.SalaryHead);
                                }
                                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                //{
                                //    Amount = CalAmount,
                                //    StdAmount = ca.Amount,
                                //    SalaryHead = EmpESICHead.SalaryHead,
                                //    DBTrack = dbt

                                //};
                                //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                            }
                        }
                    }
                    //insurance deduction

                    var OInsurancePaymentT = db.InsuranceDetailsT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId)
                        .Include(e => e.InsuranceProduct)
                        .Include(e => e.OperationStatus).ToList();
                    if (OInsurancePaymentT != null && OInsurancePaymentT.Count() > 0)
                    {
                        var InsuranceMaster = db.Insurance
                                    .Include(e => e.InsuranceProduct)
                                    .Include(e => e.SalaryHead)
                                    .ToList();
                        var EmpInsuranceHead1 = OEmpSalStruct.EmpSalStructDetails.
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

                        foreach (var ca in EmpInsuranceHead)
                        {

                            foreach (var ca1 in ca)
                            {
                                double CalAmount = ca1.Permium;
                                CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca1.SalaryHead, CalAmount);
                                if (SalAttendanceT_PayableDays == 0)
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, ca1.SalaryHead);
                                }
                                else
                                {
                                    OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, CalAmount, ca1.SalaryHead);
                                }
                                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                                //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                                //{
                                //    Amount = CalAmount,
                                //    StdAmount = CalAmount,
                                //    SalaryHead = EmpESICHead.SalaryHead,
                                //    DBTrack = dbt

                                //};
                                //OSalEarnDedT.Add(oSalEarnDedT);//add ref
                            }

                        }
                    }

                    //Statutory deductions deduction

                    //Incometax

                    //var ITaxPaymentT1 = db.EmployeePayroll.Where(d => d.Id == OEmployeePayroll.Id)// OEmployeePayroll.ITaxTransT
                    //    .Select(e => e.ITaxTransT.Where(r => r.PayMonth == PayMonth))
                    //    .FirstOrDefault();
                    var ITaxPaymentT = db.ITaxTransT.Where(r => r.PayMonth == PayMonth && r.EmployeePayroll.Id == OEmployeePayrollId).FirstOrDefault();
                    if (ITaxPaymentT != null) //&& added instead of || (Rekha)
                    {
                        double CalAmount = 0;

                        if (AutoIncomeTax == true)
                        {
                            //ITcalculation Process Pending
                        }
                        CalAmount = ITaxPaymentT.TaxPaid;
                        var EmpITHead = OEmpSalStruct.EmpSalStructDetails
                        .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();

                        CalAmount = Process.SalaryHeadGenProcess.RoundingFunction(EmpITHead.SalaryHead, CalAmount);
                        if (SalAttendanceT_PayableDays == 0)
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, 0, 0, EmpITHead.SalaryHead);
                        }
                        else
                        {
                            OSalEarnDedT = SaveSalayDetails(OSalEarnDedT, CalAmount, 0, EmpITHead.SalaryHead);
                        }
                        //DBTrack dbts = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                        //SalEarnDedT oSalEarnDedT = new SalEarnDedT()
                        //{
                        //    Amount = CalAmount,
                        //    StdAmount = 0,
                        //    SalaryHead = EmpITHead.SalaryHead,
                        //    DBTrack = dbts

                        //};
                        //OSalEarnDedT.Add(oSalEarnDedT);//add ref

                    }


                    //


                    //do arrears calculation with available objects
                    //get arrearpayment and arrear pf
                    List<SalaryArrearPFT> PaidArrearPFT = new List<SalaryArrearPFT>();
                    DateTime Fromdt = Convert.ToDateTime("01/" + PayMonth);
                    DateTime Todt = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;

                    var Salaryarrpft = db.SalaryArrearT.Include(e => e.SalaryArrearPFT).Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && DbFunctions.TruncateTime(e.FromDate) >= Fromdt && DbFunctions.TruncateTime(e.FromDate) <= Todt && e.Id != OSalArrearT.Id).ToList();
                    foreach (var item in Salaryarrpft)
                    {
                        int pftid = item.SalaryArrearPFT != null ? item.SalaryArrearPFT.Id : 0;
                        //var arrpf = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmployeePayrollId && e.PayMonth == item.PayMonth && e.ReleaseDate != null).FirstOrDefault();
                        var arrpf = db.SalaryArrearPFT.Where(e => e.Id == pftid).FirstOrDefault();

                        if (arrpf != null)
                        {
                            SalaryArrearPFT SalArrearPFT = new SalaryArrearPFT()
                            {
                                SalaryWages = 0,
                                EPFWages = arrpf.EPFWages,// arrpf.PFECRR.Arrear_EPF_Wages,
                                EPSWages = arrpf.EPSWages,//arrpf.PFECRR.Arrear_EPS_Wages,
                                EDLIWages = arrpf.EDLIWages,//arrpf.PFECRR.Arrear_EDLI_Wages,
                                CompPF = arrpf.CompPF,//arrpf.PFECRR.Arrear_ER_Share,
                                EmpPF = arrpf.EmpPF,//arrpf.PFECRR.Arrear_EE_Share,
                                EmpEPS = arrpf.EmpEPS,//arrpf.PFECRR.Arrear_EPS_Share,
                                EmpVPF = arrpf.EmpVPF,
                            };
                            PaidArrearPFT.Add(SalArrearPFT);
                        }
                    }


                    //get original salary data
                    var OOriginalSalaryT = db.SalaryT.Include(e => e.SalEarnDedT)
                        .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType))
                        .Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayrollId)
                        .SingleOrDefault();
                    String[] mSalHeadOperationType = new String[8];
                    mSalHeadOperationType[0] = "REGULAR";
                    mSalHeadOperationType[1] = "BASIC";
                    mSalHeadOperationType[2] = "VDA";
                    mSalHeadOperationType[3] = "EPF";
                    mSalHeadOperationType[4] = "PTAX";
                    mSalHeadOperationType[5] = "ESIC";
                    mSalHeadOperationType[6] = "LWF";
                    mSalHeadOperationType[7] = "DA";
                    List<SalEarnDedT> OOriginalSalEarnDedData = new List<Payroll.SalEarnDedT>();
                    if (OOriginalSalaryT != null)
                    {
                        OOriginalSalEarnDedData = OOriginalSalaryT.SalEarnDedT
                                       .Where(e => e.SalaryHead.SalHeadOperationType != null && e.SalaryHead.Frequency.LookupVal.ToUpper() != "DAILY" && e.SalaryHead.Frequency.LookupVal.ToUpper() != "HOURLY" && mSalHeadOperationType.Contains(e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper()))
                                       .ToList();
                    }
                    List<SalaryArrearPaymentT> OFinalArrearPayementT = new List<SalaryArrearPaymentT>();
                    SalaryArrearPFT OFinalOSalaryArrearPFT = new SalaryArrearPFT();
                    OFinalOSalaryArrearPFT.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    //check for promotion structure change
                    //Old Components not in new salary
                    // E

                    if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "PROMOTION" || OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "TRANSFER" || OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "AGREEMENTARREAR")
                    {
                        List<int> OSalheadadd = new List<int>();
                        foreach (var item in OOriginalSalEarnDedData)
                        {
                            OSalheadadd.Add(item.SalaryHead.Id);
                        }
                        DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        foreach (var ca in OSalEarnDedT.Where(e => e.SalaryHead.SalHeadOperationType != null && e.SalaryHead.Frequency.LookupVal.ToUpper() != "DAILY" && e.SalaryHead.Frequency.LookupVal.ToUpper() != "HOURLY" && mSalHeadOperationType.Contains(e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper())))
                        {
                            if (OSalheadadd.Contains(ca.SalaryHead.Id) == false)
                            {
                                //new data
                                SalaryArrearPaymentT OSalArrearPaymentT = new SalaryArrearPaymentT();
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                {
                                    SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                    {
                                        SalaryWages = OPFTransT.Gross_Wages,
                                        EPFWages = OPFTransT.EPF_Wages,
                                        EPSWages = OPFTransT.EPS_Wages,
                                        EDLIWages = OPFTransT.EDLI_Wages,

                                        CompPF = OPFTransT.ER_Share,
                                        EmpPF = OPFTransT.EE_Share,
                                        EmpVPF = OPFTransT.EE_VPF_Share,
                                        EmpEPS = OPFTransT.EPS_Share,

                                        DBTrack = dbt,
                                    };
                                    db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                    //OSalArrearPaymentT.SalaryArrearPFT = OSalaryArrearPFT;
                                    OFinalOSalaryArrearPFT = OSalaryArrearPFT;
                                }

                                OSalArrearPaymentT.ProcessMonthYear = PayMonth;
                                OSalArrearPaymentT.SalaryHead = db.SalaryHead.Find(ca.SalaryHead.Id);
                                OSalArrearPaymentT.SalHeadAmount = ca.Amount;

                                OSalArrearPaymentT.PayMonth = OSalaryArrearT.PayMonth;
                                OSalArrearPaymentT.IsRecovery = OSalaryArrearT.IsRecovery;
                                OSalArrearPaymentT.DBTrack = dbt;
                                OFinalArrearPayementT.Add(OSalArrearPaymentT);

                            }
                        }

                        //new salary componenets not in old salary
                        List<int> OSalheadminus = new List<int>();
                        foreach (var item in OSalEarnDedT.Where(e => e.SalaryHead.SalHeadOperationType != null && e.SalaryHead.Frequency.LookupVal.ToUpper() != "DAILY" && e.SalaryHead.Frequency.LookupVal.ToUpper() != "HOURLY" && mSalHeadOperationType.Contains(e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper())))
                        {
                            OSalheadminus.Add(item.SalaryHead.Id);
                        }

                        foreach (var ca in OOriginalSalEarnDedData)
                        {
                            if (OSalheadminus.Contains(ca.SalaryHead.Id) == false)
                            {
                                //old data data
                                SalaryArrearPaymentT OSalArrearPaymentT = new SalaryArrearPaymentT();
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                {
                                    SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                    {
                                        SalaryWages = OOriginalSalaryT.PFECRR.Gross_Wages,
                                        EPFWages = OOriginalSalaryT.PFECRR.EPF_Wages,
                                        EPSWages = OOriginalSalaryT.PFECRR.EPF_Wages,
                                        EDLIWages = OOriginalSalaryT.PFECRR.EDLI_Wages,

                                        CompPF = -OOriginalSalaryT.PFECRR.ER_Share,
                                        EmpPF = -OOriginalSalaryT.PFECRR.EE_Share,
                                        EmpVPF = -OOriginalSalaryT.PFECRR.EE_VPF_Share,
                                        EmpEPS = -OOriginalSalaryT.PFECRR.EPS_Share,

                                        DBTrack = dbt,
                                    };
                                    db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                    //OSalArrearPaymentT.SalaryArrearPFT = OSalaryArrearPFT;
                                    OFinalOSalaryArrearPFT = OSalaryArrearPFT;
                                }

                                OSalArrearPaymentT.ProcessMonthYear = PayMonth;
                                OSalArrearPaymentT.SalaryHead = db.SalaryHead.Find(ca.SalaryHead.Id);
                                OSalArrearPaymentT.SalHeadAmount = -ca.Amount;

                                OSalArrearPaymentT.PayMonth = OSalaryArrearT.PayMonth;
                                OSalArrearPaymentT.IsRecovery = OSalaryArrearT.IsRecovery;
                                OSalArrearPaymentT.DBTrack = dbt;
                                OFinalArrearPayementT.Add(OSalArrearPaymentT);

                            }
                        }
                    }

                    //   DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

                    //check for existing salary componenets
                    var OSalaryarr = db.SalaryArrearT.Include(e => e.SalaryArrearPaymentT).Include(e => e.SalaryArrearPaymentT.Select(a=>a.SalaryHead)).Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && DbFunctions.TruncateTime(e.FromDate) >= Fromdt && DbFunctions.TruncateTime(e.FromDate) <= Todt && e.Id != OSalArrearT.Id).ToList();

                    foreach (var ca in OSalEarnDedT)
                    {
                        foreach (var ca1 in OOriginalSalEarnDedData)
                        {
                            if (ca.SalaryHead.Id == ca1.SalaryHead.Id)
                            {
                                SalaryArrearPaymentT OSalArrearPaymentT = new SalaryArrearPaymentT();
                                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                                {
                                    if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "LWP" || OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "HOLIDAY")
                                    {
                                        //  if lwp process then arrear paid not require as discuss with sir
                                        SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                        {
                                            SalaryWages = (OPFTransT.Gross_Wages - OOriginalSalaryT.PFECRR.Gross_Wages),
                                            EPFWages = (OPFTransT.EPF_Wages - OOriginalSalaryT.PFECRR.EPF_Wages),
                                            EPSWages = (OPFTransT.EPS_Wages - OOriginalSalaryT.PFECRR.EPS_Wages),
                                            EDLIWages = (OPFTransT.EDLI_Wages - OOriginalSalaryT.PFECRR.EDLI_Wages),

                                            CompPF = (OPFTransT.ER_Share - OOriginalSalaryT.PFECRR.ER_Share),
                                            EmpPF = (OPFTransT.EE_Share - OOriginalSalaryT.PFECRR.EE_Share),
                                            EmpVPF = (OPFTransT.EE_VPF_Share - OOriginalSalaryT.PFECRR.EE_VPF_Share),
                                            EmpEPS = (OPFTransT.EPS_Share - OOriginalSalaryT.PFECRR.EPS_Share),

                                            DBTrack = dbt,

                                        };
                                        db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                        OFinalOSalaryArrearPFT = OSalaryArrearPFT;
                                    }
                                    else
                                    {
                                        SalaryArrearPFT OSalaryArrearPFT = new SalaryArrearPFT()
                                        {
                                            //SalaryWages = OSalaryArrearT.IsRecovery == true ? (OPFTransT.Gross_Wages - OOriginalSalaryT.PFECRR.Gross_Wages) : (OOriginalSalaryT.PFECRR.Gross_Wages - OPFTransT.Gross_Wages) ,
                                            //EPFWages = OSalaryArrearT.IsRecovery == true ? (OPFTransT.EPF_Wages - OOriginalSalaryT.PFECRR.EPF_Wages): (OOriginalSalaryT.PFECRR.EPF_Wages - OPFTransT.EPF_Wages) ,
                                            //EPSWages = OSalaryArrearT.IsRecovery == true ?  (OPFTransT.EPS_Wages - OOriginalSalaryT.PFECRR.EPS_Wages): (OOriginalSalaryT.PFECRR.EPS_Wages - OPFTransT.EPS_Wages) ,
                                            //EDLIWages = OSalaryArrearT.IsRecovery == true ? (OPFTransT.EDLI_Wages - OOriginalSalaryT.PFECRR.EDLI_Wages): (OOriginalSalaryT.PFECRR.EDLI_Wages - OPFTransT.EDLI_Wages) ,

                                            //CompPF = OSalaryArrearT.IsRecovery == true ?  (OPFTransT.ER_Share - OOriginalSalaryT.PFECRR.ER_Share) : (OOriginalSalaryT.PFECRR.ER_Share - OPFTransT.ER_Share) ,
                                            //EmpPF = OSalaryArrearT.IsRecovery == true ? (OPFTransT.EE_Share - OOriginalSalaryT.PFECRR.EE_Share) : (OOriginalSalaryT.PFECRR.EE_Share - OPFTransT.EE_Share) ,
                                            //EmpVPF = OSalaryArrearT.IsRecovery == true ? (OPFTransT.EE_VPF_Share - OOriginalSalaryT.PFECRR.EE_VPF_Share) : (OOriginalSalaryT.PFECRR.EE_VPF_Share - OPFTransT.EE_VPF_Share) ,
                                            //EmpEPS = OSalaryArrearT.IsRecovery == true ? (OPFTransT.EPS_Share - OOriginalSalaryT.PFECRR.EPS_Share) : (OOriginalSalaryT.PFECRR.EPS_Share - OPFTransT.EPS_Share),

                                            SalaryWages = (OPFTransT.Gross_Wages - OOriginalSalaryT.PFECRR.Gross_Wages),
                                            EPFWages = (OPFTransT.EPF_Wages - OOriginalSalaryT.PFECRR.EPF_Wages - PaidArrearPFT.Sum(e => e.EPFWages)),
                                            EPSWages = (OPFTransT.EPS_Wages - OOriginalSalaryT.PFECRR.EPS_Wages - PaidArrearPFT.Sum(e => e.EPSWages)),
                                            EDLIWages = (OPFTransT.EDLI_Wages - OOriginalSalaryT.PFECRR.EDLI_Wages - PaidArrearPFT.Sum(e => e.EDLIWages)),

                                            CompPF = (OPFTransT.ER_Share - OOriginalSalaryT.PFECRR.ER_Share - PaidArrearPFT.Sum(e => e.CompPF)),
                                            EmpPF = (OPFTransT.EE_Share - OOriginalSalaryT.PFECRR.EE_Share - PaidArrearPFT.Sum(e => e.EmpPF)),
                                            EmpVPF = (OPFTransT.EE_VPF_Share - OOriginalSalaryT.PFECRR.EE_VPF_Share - PaidArrearPFT.Sum(e => e.EmpVPF)),
                                            EmpEPS = (OPFTransT.EPS_Share - OOriginalSalaryT.PFECRR.EPS_Share - PaidArrearPFT.Sum(e => e.EmpEPS)),

                                            DBTrack = dbt,
                                        };
                                        db.SalaryArrearPFT.Add(OSalaryArrearPFT);
                                        //OSalArrearPaymentT.SalaryArrearPFT = OSalaryArrearPFT;
                                        OFinalOSalaryArrearPFT = OSalaryArrearPFT;
                                    }
                                }

                                OSalArrearPaymentT.ProcessMonthYear = PayMonth;
                                OSalArrearPaymentT.SalaryHead = db.SalaryHead.Find(ca1.SalaryHead.Id);
                                //OSalArrearPaymentT.SalHeadAmount = OSalaryArrearT.IsRecovery == true ? (ca1.Amount - ca.Amount) : (ca.Amount - ca1.Amount);
                                // OSalArrearPaymentT.SalHeadAmount = OSalaryArrearT.IsRecovery == true ? (ca.Amount - ca1.Amount) : (ca1.Amount - ca.Amount);
                                // Old Paid arrear should minus
                                double Arrpaid = 0;
                                if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "LWP")
                                {
                                    //  if lwp process then arrear paid not require as discuss with sir
                                }
                                else if (OSalaryArrearT.ArrearType.LookupVal.ToUpper() == "HOLIDAY")
                                {
                                    //  if Holiday process then arrear paid not require as discuss with sir
                                }
                                else
                                {
                                    //  if increment,promotion, transfer, process then arrear paid require as discuss with sir

                                    //var OSalaryarr = db.SalaryArrearT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId && DbFunctions.TruncateTime(e.FromDate) >= Fromdt && DbFunctions.TruncateTime(e.FromDate) <= Todt && e.Id != OSalArrearT.Id).Select(e => e.SalaryArrearPaymentT.GroupBy(x => new { x.SalaryHead }).Select(g => new
                                    //{
                                    //    SalaryHead = g.Key.SalaryHead,

                                    //    TotalAmount = g.Sum(x => x.SalHeadAmount)

                                    //}).ToList()).AsNoTracking().FirstOrDefault();

                                    if (OSalaryarr.Count() > 0)
                                    {

                                        foreach (var item in OSalaryarr)
                                        {
                                            foreach (var item1 in item.SalaryArrearPaymentT)
                                            {
                                                if (ca.SalaryHead.Id == item1.SalaryHead.Id)
                                                {
                                                    Arrpaid = Arrpaid + Convert.ToDouble(item1.SalHeadAmount.ToString());
                                                }
                                            }

                                        }
                                    }
                                }
                                if (ca.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX" && OSalaryArrearT.IsRecovery == true)
                                {
                                    OSalArrearPaymentT.SalHeadAmount = 0;
                                }
                                else
                                {
                                    OSalArrearPaymentT.SalHeadAmount = Process.SalaryHeadGenProcess.RoundingFunction(ca.SalaryHead, (ca.Amount - ca1.Amount - Arrpaid));

                                }

                                OSalArrearPaymentT.PayMonth = OSalaryArrearT.PayMonth;
                                OSalArrearPaymentT.IsRecovery = OSalaryArrearT.IsRecovery;
                                OSalArrearPaymentT.DBTrack = dbt;
                                OFinalArrearPayementT.Add(OSalArrearPaymentT);

                                break;
                            }

                        }

                    }
                    if (OFinalArrearPayementT.Count() > 0)
                    {
                        db.SalaryArrearPaymentT.AddRange(OFinalArrearPayementT);
                        db.SaveChanges();
                        OSalaryArrearT.SalaryArrearPFT = OFinalOSalaryArrearPFT;
                        OSalaryArrearT.SalaryArrearPaymentT = OFinalArrearPayementT;
                        OSalaryArrearT.ArrTotalDeduction = OFinalArrearPayementT
                  .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION")
                  .Sum(e => e.SalHeadAmount);
                        OSalaryArrearT.ArrTotalEarning = OFinalArrearPayementT
                    .Where(e => e.SalaryHead.InPayslip == true && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                    .Sum(e => e.SalHeadAmount);
                        OSalaryArrearT.ArrTotalNet = Math.Round((OSalaryArrearT.ArrTotalEarning - OSalaryArrearT.ArrTotalDeduction), 0);
                        OSalaryArrearT.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        db.SalaryArrearT.Attach(OSalaryArrearT);
                        db.Entry(OSalaryArrearT).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(OSalaryArrearT).State = System.Data.Entity.EntityState.Detached;


                    }
                }
            }
        }

        public static double[] dateoffirstdaysus(int OEmployeePayrollId, string PayMonth, double SalAttendanceT_PayableDay, CompanyPayroll SuspSalPolicy, bool suspenddays)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //EmployeePayroll othser1 = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                //                           .Include(e => e.OtherServiceBook)
                //                           .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct))
                //                           .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus))
                //                           .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus.EmpActingStatus))
                //                           .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
                //                           .SingleOrDefault();
                //*
                var othser = new EmployeePayroll();
                othser = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                List<OtherServiceBook> OtherServiceBookList = new List<OtherServiceBook>();
                List<OtherServiceBook> ObjOtherServiceBooklist = new List<OtherServiceBook>();
                ObjOtherServiceBooklist = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId).ToList();
                if (ObjOtherServiceBooklist.Count() > 0)
                {
                    foreach (var othSERitem in ObjOtherServiceBooklist)
                    {
                        var NewPayStruct = db.OtherServiceBook.Where(e => e.Id == othSERitem.Id).Select(r => r.NewPayStruct).FirstOrDefault();
                        var JobStatus = db.PayStruct.Where(e => e.Id == NewPayStruct.Id).Select(r => r.JobStatus).FirstOrDefault();
                        if (JobStatus != null)
                        {
                            var JobStatus_EmpActingStatus = db.JobStatus.Where(e => e.Id == JobStatus.Id).Select(r => r.EmpActingStatus).FirstOrDefault();
                            othSERitem.NewPayStruct.JobStatus = JobStatus;
                            othSERitem.NewPayStruct.JobStatus.EmpActingStatus = JobStatus_EmpActingStatus;
                        }

                        var OthServiceBookActivity = db.OtherServiceBook.Where(e => e.Id == othSERitem.Id).Select(r => r.OthServiceBookActivity).FirstOrDefault();
                        othSERitem.NewPayStruct = NewPayStruct;

                        othSERitem.OthServiceBookActivity = OthServiceBookActivity;
                        OtherServiceBookList.Add(othSERitem);
                        othser.OtherServiceBook = OtherServiceBookList;
                    }

                }
                Double[] mPayDaysRunningsp = new Double[5];
                mPayDaysRunningsp[1] = 0;

                //var OthServBkSus = othser.OtherServiceBook.Where(e => e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND").SingleOrDefault();
                if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
                {
                    OtherServiceBook OthServBkSus = othser.OtherServiceBook.Where(e => (e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").OrderByDescending(d => d.Id).FirstOrDefault();

                    if (OthServBkSus != null)
                    {
                        // var OthServBkRejoin = othser.OtherServiceBook.Where(e => e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "REJOIN").SingleOrDefault();
                        OtherServiceBook OthServBkRejoin = othser.OtherServiceBook.Where(e => (e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "ACTIVE" && e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").SingleOrDefault();


                        double pd = Convert.ToDouble(DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0])));
                        string Emppayconceptf30 = db.PayProcessGroup.Include(a => a.PayMonthConcept).AsNoTracking().OrderByDescending(e => e.Id).FirstOrDefault().PayMonthConcept.LookupVal;
                        if (Emppayconceptf30 == "FIXED30DAYS")
                        {
                            pd = 30;
                        }

                        if (Emppayconceptf30 == "30DAYS")
                        {
                            pd = 30;
                        }
                        DateTime salaryMonth = Convert.ToDateTime("01" + "/" + PayMonth).Date;
                        if (salaryMonth.Month == 2)
                        {
                            pd = Convert.ToDouble((salaryMonth.AddMonths(1).AddDays(-1)).Day);
                        }
                        double susday = 0;
                        double mEffDay = 0;
                        double prvmEffDay = 0;
                        double mSusDayFull = 0;
                        double Presentdays = 0;
                        Presentdays = SalAttendanceT_PayableDay;

                        mSusDayFull = Math.Round((Convert.ToDateTime(pd + "/" + PayMonth).Date - OthServBkSus.ReleaseDate.Value).TotalDays) + 1;

                        if (mSusDayFull > pd)
                        {
                            susday = mSusDayFull - pd;
                        }
                        else
                        {
                            susday = mSusDayFull;
                        }



                        if (SuspSalPolicy.SuspensionSalPolicy.Count() > 0)
                        {
                            foreach (SuspensionSalPolicy a in SuspSalPolicy.SuspensionSalPolicy)
                            {
                                if (a.DayRange.Count() > 0)
                                {
                                    foreach (Range c1 in a.DayRange)
                                    {

                                        if (susday > c1.RangeFrom && susday < c1.RangeTo)
                                        {
                                            suspenddays = true;
                                            mPayDaysRunningsp[1] = 1;
                                            if ((susday + pd) > c1.RangeTo)
                                            {
                                                if ((susday + pd) > mSusDayFull)
                                                {
                                                    mEffDay = Math.Round((mSusDayFull - susday + pd) * c1.EmpSharePercentage / 100, 2);
                                                    if (Presentdays >= mEffDay)
                                                    {
                                                        SalAttendanceT_PayableDay = mEffDay;

                                                    }
                                                    continue;
                                                }
                                                else
                                                {
                                                    mEffDay = Math.Round((c1.RangeTo - susday) * c1.EmpSharePercentage / 100, 2);

                                                }

                                                mEffDay = mEffDay;
                                            }
                                            else
                                            {
                                                if (pd > susday)
                                                {

                                                    mEffDay = Math.Round((pd) * c1.EmpSharePercentage / 100, 2);
                                                }
                                                else
                                                {


                                                    if ((susday - c1.RangeFrom + 0) > pd)
                                                    {
                                                        mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                    }
                                                    else if (mSusDayFull > c1.RangeFrom && (mSusDayFull - c1.RangeFrom) > pd)
                                                    {
                                                        mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                    }
                                                    else
                                                    {

                                                        mEffDay = prvmEffDay + Math.Round((susday - c1.RangeFrom + 0) * c1.EmpSharePercentage / 100, 2);
                                                    }



                                                }

                                            }
                                            if (Presentdays >= mEffDay)
                                            {
                                                SalAttendanceT_PayableDay = mEffDay;

                                            }
                                            susday = susday + pd;
                                            //susday = susday + Math.Round((days * c1.EmpSharePercentage / 100) + 0.001, 0);
                                        }
                                        prvmEffDay = mEffDay;
                                        // susday = susday + pd;



                                    }
                                }
                                // SalAttendanceT_PayableDays = susday;
                            }
                        }

                        if (OthServBkRejoin != null)
                        {
                            SalAttendanceT_PayableDay = Presentdays;
                        }
                    }
                }
                mPayDaysRunningsp[0] = SalAttendanceT_PayableDay;
                return mPayDaysRunningsp;
                //return SalAttendanceT_PayableDay;
            }
        }

        public static double[] dateofMiddledaysus_enddatenotnull(int OEmployeePayrollId, string PayMonth, double mPayDaysRunnings, CompanyPayroll SuspSalPolicy, EmpSalStruct OMultiEmpStruct, bool suspenddays)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //EmployeePayroll othser = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                //              .Include(e => e.OtherServiceBook)
                //              .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct))
                //              .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus))
                //              .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus.EmpActingStatus))
                //              .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
                //              .SingleOrDefault();
                var othser = new EmployeePayroll();
                othser = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                List<OtherServiceBook> OtherServiceBookList = new List<OtherServiceBook>();
                List<OtherServiceBook> OtherServiceBookOBJlist = new List<OtherServiceBook>();
                OtherServiceBookOBJlist = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId).ToList();
                if (OtherServiceBookOBJlist.Count() > 0)
                {
                    foreach (var Othseritem in OtherServiceBookOBJlist)
                    {
                        var NewPayStruct = db.OtherServiceBook.Where(e => e.Id == Othseritem.Id).Select(r => r.NewPayStruct).FirstOrDefault();
                        var JobStatus = db.PayStruct.Where(e => e.Id == NewPayStruct.Id).Select(r => r.JobStatus).FirstOrDefault();
                        var JobStatus_EmpActingStatus = db.JobStatus.Where(e => e.Id == JobStatus.Id).Select(r => r.EmpActingStatus).FirstOrDefault();
                        var OthServiceBookActivity = db.OtherServiceBook.Where(e => e.Id == Othseritem.Id).Select(r => r.OthServiceBookActivity).FirstOrDefault();
                        Othseritem.NewPayStruct = NewPayStruct;
                        Othseritem.NewPayStruct.JobStatus = JobStatus;
                        Othseritem.NewPayStruct.JobStatus.EmpActingStatus = JobStatus_EmpActingStatus;
                        Othseritem.OthServiceBookActivity = OthServiceBookActivity;

                        OtherServiceBookList.Add(Othseritem);
                        othser.OtherServiceBook = OtherServiceBookList;
                    }

                }
                Double[] mPayDaysRunningsp = new Double[5];
                mPayDaysRunningsp[1] = 0;
                // var OthServBkSus = othser.OtherServiceBook.Where(e => e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" || e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "REJOIN").OrderByDescending(e => e.ReleaseDate).ToList();
                if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
                {
                    List<OtherServiceBook> OthServBkSus = othser.OtherServiceBook.Where(e => ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED") || ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "ACTIVE" && e.OthServiceBookActivity.Name.ToUpper() == "REJOIN")).OrderByDescending(e => e.ReleaseDate).ToList();

                    //OthServBkSus.OrderByDescending(e => e.ReleaseDate);
                    if (OthServBkSus.Count > 0)
                    {

                        // string ff = OthServBkSus.First().NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString();
                        string ff = OthServBkSus.First().OthServiceBookActivity.Name;
                        // if (ff.ToUpper() == "SUSPEND")
                        if (ff.ToUpper() == "SUSPENDED")
                        {


                            DateTime? compRdate = OthServBkSus.First().ReleaseDate;
                            if (OMultiEmpStruct.EffectiveDate.Value.Date == Convert.ToDateTime(compRdate).Date)
                            {

                            }

                        }
                        else if (ff.ToUpper() == "REJOIN")
                        {
                            DateTime? compRdate = OthServBkSus.First().ReleaseDate;
                            //if (OMultiEmpStruct.EffectiveDate.Value.Date != Convert.ToDateTime(compRdate).Date)
                            if (OMultiEmpStruct.EffectiveDate.Value.Date <= Convert.ToDateTime(compRdate).Date)
                            {
                                //var OthServBkPrvSus = othser.OtherServiceBook.Where(e => e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND").OrderByDescending(e => e.ReleaseDate).ToList();
                                List<OtherServiceBook> OthServBkPrvSus = othser.OtherServiceBook.Where(e => (e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").OrderByDescending(e => e.ReleaseDate).ToList();

                                //  OthServBkPrvSus.OrderByDescending(e => e.ReleaseDate);
                                DateTime? compOdate = OthServBkPrvSus.First().ReleaseDate;
                                // days calc start
                                double pd = mPayDaysRunnings;
                                double susday = 0;
                                double mEffDay = 0;
                                double prvmEffDay = 0;
                                double mSusDayFull = 0;
                                double Presentdays1 = 0;
                                Presentdays1 = mPayDaysRunnings;
                                mSusDayFull = Math.Round((Convert.ToDateTime(compRdate).Date - Convert.ToDateTime(compOdate).Date).TotalDays) + 1;

                                if (mSusDayFull > pd)
                                {
                                    susday = mSusDayFull - pd;
                                }
                                else
                                {
                                    susday = mSusDayFull;
                                }


                                if (SuspSalPolicy.SuspensionSalPolicy.Count() > 0)
                                {
                                    foreach (SuspensionSalPolicy a in SuspSalPolicy.SuspensionSalPolicy)
                                    {
                                        if (a.DayRange.Count() > 0)
                                        {
                                            foreach (Range c1 in a.DayRange)
                                            {

                                                if (susday > c1.RangeFrom && susday < c1.RangeTo)
                                                {
                                                    suspenddays = true;
                                                    mPayDaysRunningsp[1] = 1;
                                                    if ((susday + pd) > c1.RangeTo)
                                                    {
                                                        if ((susday + pd) > mSusDayFull)
                                                        {
                                                            mEffDay = Math.Round((mSusDayFull - susday + pd) * c1.EmpSharePercentage / 100, 2);
                                                            if (Presentdays1 >= mEffDay)
                                                            {
                                                                mPayDaysRunnings = mEffDay;

                                                            }
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            mEffDay = Math.Round((c1.RangeTo - susday) * c1.EmpSharePercentage / 100, 2);

                                                        }

                                                        mEffDay = mEffDay;
                                                    }
                                                    else
                                                    {
                                                        if (pd > susday)
                                                        {

                                                            mEffDay = Math.Round((pd) * c1.EmpSharePercentage / 100, 2);
                                                        }
                                                        else
                                                        {


                                                            if ((susday - c1.RangeFrom + 0) > pd)
                                                            {
                                                                mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                            else if (mSusDayFull > c1.RangeFrom && (mSusDayFull - c1.RangeFrom) > pd)
                                                            {
                                                                mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                            else
                                                            {

                                                                mEffDay = prvmEffDay + Math.Round((susday - c1.RangeFrom + 0) * c1.EmpSharePercentage / 100, 2);
                                                            }



                                                        }

                                                    }
                                                    if (Presentdays1 >= mEffDay)
                                                    {
                                                        mPayDaysRunnings = mEffDay;

                                                    }
                                                    susday = susday + pd;
                                                    //susday = susday + Math.Round((days * c1.EmpSharePercentage / 100) + 0.001, 0);
                                                }
                                                prvmEffDay = mEffDay;
                                                // susday = susday + pd;



                                            }
                                        }
                                        // SalAttendanceT_PayableDays = susday;
                                    }
                                }

                                // days calc end


                            }
                        }


                    }
                }
                mPayDaysRunningsp[0] = mPayDaysRunnings;
                return mPayDaysRunningsp;

                // return mPayDaysRunnings;
            }
        }

        public static double[] dateofMiddledaysus_enddatenull(int OEmployeePayrollId, string PayMonth, double mPayDaysRunnings, CompanyPayroll SuspSalPolicy, EmpSalStruct OMultiEmpStruct, bool suspenddays)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //EmployeePayroll othser = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId)
                //                         .Include(e => e.OtherServiceBook)
                //                         .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct))
                //                         .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus))
                //                         .Include(e => e.OtherServiceBook.Select(x => x.NewPayStruct.JobStatus.EmpActingStatus))
                //                         .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
                //                         .SingleOrDefault();
                var othser = new EmployeePayroll();
                othser = db.EmployeePayroll.Where(e => e.Id == OEmployeePayrollId).FirstOrDefault();
                List<OtherServiceBook> OtherServiceBookList = new List<OtherServiceBook>();
                List<OtherServiceBook> OtherServiceBookObJlist = new List<OtherServiceBook>();
                OtherServiceBookObJlist = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OEmployeePayrollId).ToList();
                if (OtherServiceBookObJlist.Count() > 0)
                {
                    foreach (var OTHSeritem in OtherServiceBookObJlist)
                    {
                        var NewPayStruct = db.OtherServiceBook.Where(e => e.Id == OTHSeritem.Id).Select(r => r.NewPayStruct).FirstOrDefault();
                        var JobStatus = db.PayStruct.Where(e => e.Id == NewPayStruct.Id).Select(r => r.JobStatus).FirstOrDefault();
                        var JobStatus_EmpActingStatus = db.JobStatus.Where(e => e.Id == JobStatus.Id).Select(r => r.EmpActingStatus).FirstOrDefault();
                        var OthServiceBookActivity = db.OtherServiceBook.Where(e => e.Id == OTHSeritem.Id).Select(r => r.OthServiceBookActivity).FirstOrDefault();
                        OTHSeritem.NewPayStruct = NewPayStruct;
                        OTHSeritem.NewPayStruct.JobStatus = JobStatus;
                        OTHSeritem.NewPayStruct.JobStatus.EmpActingStatus = JobStatus_EmpActingStatus;
                        OTHSeritem.OthServiceBookActivity = OthServiceBookActivity;

                        OtherServiceBookList.Add(OTHSeritem);
                        othser.OtherServiceBook = OtherServiceBookList;
                    }

                }
                Double[] mPayDaysRunningsp = new Double[5];
                mPayDaysRunningsp[1] = 0;
                // var OthServBkSus = othser.OtherServiceBook.Where(e => e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" || e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "REJOIN").OrderByDescending(e => e.ReleaseDate).ToList();
                if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
                {
                    List<OtherServiceBook> OthServBkSus = othser.OtherServiceBook.Where(e => ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED") || ((e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "ACTIVE" && e.OthServiceBookActivity.Name.ToUpper() == "REJOIN")).OrderByDescending(e => e.ReleaseDate).ToList();

                    //  OthServBkSus.OrderByDescending(e => e.ReleaseDate);
                    if (OthServBkSus.Count > 0)
                    {
                        string ff = OthServBkSus.First().NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString();
                        if (ff.ToUpper() == "SUSPEND")
                        {
                            DateTime? compRdate = OthServBkSus.First().ReleaseDate;
                            if (OMultiEmpStruct.EffectiveDate.Value.Date == Convert.ToDateTime(compRdate).Date)
                            {
                                // days calc start
                                double pd = mPayDaysRunnings;
                                double susday = 0;
                                double mEffDay = 0;
                                double prvmEffDay = 0;
                                double mSusDayFull = 0;
                                double Presentdays2 = 0;
                                Presentdays2 = mPayDaysRunnings;
                                mSusDayFull = mPayDaysRunnings;

                                if (mSusDayFull > pd)
                                {
                                    susday = mSusDayFull - pd;
                                }
                                else
                                {
                                    susday = mSusDayFull;
                                }

                                if (SuspSalPolicy.SuspensionSalPolicy.Count() > 0)
                                {
                                    foreach (SuspensionSalPolicy a in SuspSalPolicy.SuspensionSalPolicy)
                                    {
                                        if (a.DayRange.Count() > 0)
                                        {
                                            foreach (Range c1 in a.DayRange)
                                            {

                                                if (susday > c1.RangeFrom && susday < c1.RangeTo)
                                                {
                                                    suspenddays = true;
                                                    mPayDaysRunningsp[1] = 1;
                                                    if ((susday + pd) > c1.RangeTo)
                                                    {
                                                        if ((susday + pd) > mSusDayFull)
                                                        {
                                                            mEffDay = Math.Round((mSusDayFull - susday + pd) * c1.EmpSharePercentage / 100, 2);
                                                            if (Presentdays2 >= mEffDay)
                                                            {
                                                                mPayDaysRunnings = mEffDay;

                                                            }
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            mEffDay = Math.Round((c1.RangeTo - susday) * c1.EmpSharePercentage / 100, 2);

                                                        }

                                                        mEffDay = mEffDay;
                                                    }
                                                    else
                                                    {
                                                        if (pd > susday)
                                                        {

                                                            mEffDay = Math.Round((pd) * c1.EmpSharePercentage / 100, 2);
                                                        }
                                                        else
                                                        {


                                                            if ((susday - c1.RangeFrom + 0) > pd)
                                                            {
                                                                mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                            else if (mSusDayFull > c1.RangeFrom && (mSusDayFull - c1.RangeFrom) > pd)
                                                            {
                                                                mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                            else
                                                            {

                                                                mEffDay = prvmEffDay + Math.Round((susday - c1.RangeFrom + 0) * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                        }
                                                    }
                                                    if (Presentdays2 >= mEffDay)
                                                    {
                                                        mPayDaysRunnings = mEffDay;

                                                    }
                                                    susday = susday + pd;
                                                }
                                                prvmEffDay = mEffDay;
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else if (ff.ToUpper() == "REJOIN")
                        {
                            DateTime? compRdate = OthServBkSus.First().ReleaseDate;
                            if (OMultiEmpStruct.EffectiveDate.Value.Date != Convert.ToDateTime(compRdate).Date)
                            {
                                //var OthServBkPrvSus = othser.OtherServiceBook.Where(e => e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND").OrderByDescending(e => e.ReleaseDate).ToList();
                                List<OtherServiceBook> OthServBkPrvSus = othser.OtherServiceBook.Where(e => (e.NewPayStruct != null && e.NewPayStruct.JobStatus != null && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) && e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" && e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").OrderByDescending(e => e.ReleaseDate).ToList();

                                // OthServBkPrvSus.OrderByDescending(e => e.ReleaseDate);
                                DateTime? compOdate = OthServBkPrvSus.First().ReleaseDate;
                                // days calc start
                                double pd = mPayDaysRunnings;
                                double susday = 0;
                                double mEffDay = 0;
                                double prvmEffDay = 0;
                                double mSusDayFull = 0;
                                double Presentdays3 = 0;
                                Presentdays3 = mPayDaysRunnings;
                                mSusDayFull = Math.Round((Convert.ToDateTime(compOdate).Date - Convert.ToDateTime(compRdate).Date).TotalDays) + 1;

                                if (mSusDayFull > pd)
                                {
                                    susday = mSusDayFull - pd;
                                }
                                else
                                {
                                    susday = mSusDayFull;
                                }

                                if (SuspSalPolicy.SuspensionSalPolicy.Count() > 0)
                                {
                                    foreach (SuspensionSalPolicy a in SuspSalPolicy.SuspensionSalPolicy)
                                    {
                                        if (a.DayRange.Count() > 0)
                                        {
                                            foreach (Range c1 in a.DayRange)
                                            {

                                                if (susday > c1.RangeFrom && susday < c1.RangeTo)
                                                {
                                                    suspenddays = true;
                                                    mPayDaysRunningsp[1] = 1;
                                                    if ((susday + pd) > c1.RangeTo)
                                                    {
                                                        if ((susday + pd) > mSusDayFull)
                                                        {
                                                            mEffDay = Math.Round((mSusDayFull - susday + pd) * c1.EmpSharePercentage / 100, 2);
                                                            if (Presentdays3 >= mEffDay)
                                                            {
                                                                mPayDaysRunnings = mEffDay;

                                                            }
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            mEffDay = Math.Round((c1.RangeTo - susday) * c1.EmpSharePercentage / 100, 2);
                                                        }
                                                        mEffDay = mEffDay;
                                                    }
                                                    else
                                                    {
                                                        if (pd > susday)
                                                        {
                                                            mEffDay = Math.Round((pd) * c1.EmpSharePercentage / 100, 2);
                                                        }
                                                        else
                                                        {
                                                            if ((susday - c1.RangeFrom + 0) > pd)
                                                            {
                                                                mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                            else if (mSusDayFull > c1.RangeFrom && (mSusDayFull - c1.RangeFrom) > pd)
                                                            {
                                                                mEffDay = Math.Round(pd * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                            else
                                                            {

                                                                mEffDay = prvmEffDay + Math.Round((susday - c1.RangeFrom + 0) * c1.EmpSharePercentage / 100, 2);
                                                            }
                                                        }
                                                    }
                                                    if (Presentdays3 >= mEffDay)
                                                    {
                                                        mPayDaysRunnings = mEffDay;

                                                    }
                                                    susday = susday + pd;
                                                }
                                                prvmEffDay = mEffDay;
                                            }
                                        }
                                    }
                                }
                                // days calc end 
                            }
                        }
                    }
                }
                mPayDaysRunningsp[0] = mPayDaysRunnings;
                return mPayDaysRunningsp;
                //return mPayDaysRunnings;
            }

        }
    }
}