using System;
using P2b.Global;
using P2BUltimate.App_Start;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using P2BUltimate.Models;
using Payroll;
using Leave;
using P2BUltimate.Security;
using System.IO;
using P2BUltimate.Models;
using System.Data.Entity.Validation;
using System.Transactions;
using System.Data;
using P2BUltimate.Controllers.Payroll.MainController;
using EMS;
namespace P2BUltimate.Process
{
    public class PayrollReportGen
    {

        public static string _returnFormateOfBasicScale(string CompCode, BasicScaleDetails oBasicScaleDetails)
        {
            string _returnString = string.Empty;
            if (CompCode == "BDCB")
            {
                _returnString = oBasicScaleDetails.StartingSlab + "-" + oBasicScaleDetails.IncrementAmount + "(" + Convert.ToInt32(oBasicScaleDetails.IncrementCount) + ")-" + oBasicScaleDetails.EndingSlab + ",";
            }
            else
            {
                _returnString = oBasicScaleDetails.StartingSlab + "-" + oBasicScaleDetails.IncrementCount + "-" + oBasicScaleDetails.IncrementAmount + "-" + oBasicScaleDetails.EndingSlab + ",";
            }
            return _returnString;
        }
        public static void GereratePaySlipR(int mEmployeePayroll_Id, string mPayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var OEmployeePayroll = new EmployeePayroll();
                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == mEmployeePayroll_Id).FirstOrDefault();
                var Employee = db.Employee.Where(e => e.Id == OEmployeePayroll.Employee_Id).FirstOrDefault();
                var EmpName = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Employee.EmpName_Id).FirstOrDefault();
                var ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).FirstOrDefault();
                var EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).FirstOrDefault();

                var Bank = db.EmpOff.Where(e => e.Id == EmpOffInfo.Id).Select(r => r.Bank).FirstOrDefault();
                var Branch = db.EmpOff.Where(e => e.Id == EmpOffInfo.Id).Select(r => r.Branch).FirstOrDefault();
                var AccountType = db.EmpOff.Where(e => e.Id == EmpOffInfo.Id).Select(r => r.AccountType).FirstOrDefault();
                var NationalityID = db.EmpOff.Where(e => e.Id == EmpOffInfo.Id).Select(r => r.NationalityID).FirstOrDefault();
                var PayScale = db.EmpOff.Where(e => e.Id == EmpOffInfo.Id).Select(r => r.PayScale).FirstOrDefault();
                var PayScaleType = db.PayScale.Where(e => e.Id == PayScale.Id).Select(r => r.PayScaleType).FirstOrDefault();
                DateTime comparedate = (Convert.ToDateTime("01/" + mPayMonth).Date);
                var AccountNo = EmpOffInfo.AccountNo;


                DateTime LastDay = Convert.ToDateTime(DateTime.DaysInMonth(comparedate.Year, comparedate.Month) + ("/" + mPayMonth));
                var OEmpSalStruct = new EmpSalStruct();
                OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == mEmployeePayroll_Id && e.EffectiveDate >= comparedate && e.EffectiveDate <= LastDay).OrderByDescending(e => e.Id).FirstOrDefault();

                var OEmpAttInfo = db.SalAttendanceT.Where(e => e.PayMonth == mPayMonth && e.EmployeePayroll.Id == mEmployeePayroll_Id).FirstOrDefault();
                var OCPIEntry = db.CPIEntryT.Where(e => e.PayMonth == mPayMonth && e.EmployeePayroll_Id == mEmployeePayroll_Id).FirstOrDefault();
                var OSalaryTemp = db.SalaryT.Where(e => e.PayMonth == mPayMonth && e.EmployeePayroll_Id == mEmployeePayroll_Id).FirstOrDefault();

                if (OSalaryTemp != null)
                {
                    var OSalaryT = new SalaryT();
                    OSalaryT = db.SalaryT.Include(r => r.PayslipR).Where(e => e.Id == OSalaryTemp.Id).FirstOrDefault();
                    List<SalEarnDedT> SalEarnDedT = new List<SalEarnDedT>();
                    SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT.Id == OSalaryT.Id).ToList();
                    foreach (var i in SalEarnDedT)
                    {

                        var SalaryHead = db.SalEarnDedT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                        var Type = db.SalaryHead.Where(e => e.Id == SalaryHead.Id).Select(r => r.Type).FirstOrDefault();
                        var Frequency = db.SalaryHead.Where(e => e.Id == SalaryHead.Id).Select(r => r.Frequency).FirstOrDefault();
                        i.SalaryHead = SalaryHead;
                        i.SalaryHead.Type = Type;
                        i.SalaryHead.Frequency = Frequency;


                    }
                    OSalaryT.SalEarnDedT = SalEarnDedT;
                    var FinnanceYearId = db.SalaryT.Where(e => e.Id == OSalaryT.Id).Select(r => r.FinnanceYearId).FirstOrDefault();
                    OSalaryT.FinnanceYearId = FinnanceYearId;


                    if (OSalaryT != null)
                    {
                        var OPaySlipR = new PaySlipR();
                        OPaySlipR = db.PaySlipR.Where(e => e.SalaryT_Id == OSalaryT.Id).FirstOrDefault();

                        if (OPaySlipR != null)
                        {
                            var PaySlipDetailDedR = db.PaySlipDetailDedR.Where(e => e.PaySlipR_Id == OPaySlipR.Id).ToList();
                            if (PaySlipDetailDedR != null)
                            { db.PaySlipDetailDedR.RemoveRange(PaySlipDetailDedR); }
                            var PaySlipDetailEarnR = db.PaySlipDetailEarnR.Where(e => e.PaySlipR_Id == OPaySlipR.Id).ToList();
                            if (PaySlipDetailEarnR != null)
                            { db.PaySlipDetailEarnR.RemoveRange(PaySlipDetailEarnR); }
                            var PaySlipDetailLeaveR = db.PaySlipDetailLeaveR.Where(e => e.PaySlipR_Id == OPaySlipR.Id).ToList();
                            if (PaySlipDetailLeaveR != null)
                            { db.PaySlipDetailLeaveR.RemoveRange(PaySlipDetailLeaveR); }

                            db.PaySlipR.Remove(OPaySlipR);
                        }


                    }

                    // PayslipNameFormat
                    string requiredPathLoan = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool exists = System.IO.Directory.Exists(requiredPathLoan);
                    string localPathLoan;
                    if (!exists)
                    {
                        localPathLoan = new Uri(requiredPathLoan).LocalPath;
                        System.IO.Directory.CreateDirectory(localPathLoan);
                    }
                    string pathLoan = requiredPathLoan + @"\PayslipNameFormat" + ".ini";
                    localPathLoan = new Uri(pathLoan).LocalPath;
                    if (!System.IO.File.Exists(localPathLoan))
                    {

                        using (var fs = new FileStream(localPathLoan, FileMode.OpenOrCreate))
                        {
                            StreamWriter str = new StreamWriter(fs);
                            str.BaseStream.Seek(0, SeekOrigin.Begin);

                            str.Flush();
                            str.Close();
                            fs.Close();
                        }


                    }


                    //Utilities.NumberToEnglish num = new Utilities.NumberToEnglish();

                    #region PaySlip_Data_Start
                    PaySlipR PSlipT = new PaySlipR();
                    {
                        PSlipT.PayMonth = mPayMonth;
                        PSlipT.AadharNo = NationalityID.AdharNo != null ? NationalityID.AdharNo : "";
                        PSlipT.ESICNo = NationalityID.ESICNo != null ? NationalityID.ESICNo : "";
                        PSlipT.GISNo = NationalityID.GINo != null ? NationalityID.GINo : "";
                        PSlipT.PANNo = NationalityID.PANNo != null ? NationalityID.PANNo : "";
                        PSlipT.PensionNo = NationalityID.PensionNo != null ? NationalityID.PensionNo : "";
                        PSlipT.PFNo = NationalityID.PFNo != null ? NationalityID.PFNo : "";
                        PSlipT.SuperAnnuNo = "";
                        PSlipT.MediclaimNo = "";
                        PSlipT.BankAcNo = AccountNo != null ? AccountNo : "";
                        PSlipT.BankAcType = AccountType != null ? AccountType.LookupVal : "";
                        PSlipT.PaymentBank = Bank != null ? Bank.Name : "";
                        PSlipT.PaymentBranch = Branch != null ? Branch.Name : "";
                        PSlipT.PayScale = PayScaleType != null ? PayScaleType.LookupVal : "";
                        var OSalaryTGeo = db.SalaryT.Where(e => e.Id == OSalaryTemp.Id).FirstOrDefault();
                        var GeoStruct = db.SalaryT.Where(e => e.Id == OSalaryTemp.Id).Select(r => r.Geostruct).FirstOrDefault();
                        var Corporate = db.Corporate.Where(e => e.Id == GeoStruct.Corporate_Id).FirstOrDefault();
                        var Region = db.Region.Where(e => e.Id == GeoStruct.Region_Id).FirstOrDefault();
                        var Company = db.Company.Where(e => e.Id == GeoStruct.Company_Id).FirstOrDefault();
                        var Division = db.Division.Where(e => e.Id == GeoStruct.Division_Id).FirstOrDefault();
                        var Location = db.Location.Where(e => e.Id == GeoStruct.Location_Id).FirstOrDefault();
                        var LocationObj = Location != null ? db.LocationObj.Where(e => e.Id == Location.LocationObj_Id).FirstOrDefault() : null;
                        var Department = db.Department.Where(e => e.Id == GeoStruct.Department_Id).FirstOrDefault();
                        var DepartmentObj = Department != null ? db.DepartmentObj.Where(e => e.Id == Department.DepartmentObj_Id).FirstOrDefault() : null;
                        var Unit = db.Unit.Where(e => e.Id == GeoStruct.Unit_Id).FirstOrDefault();
                        var Group = db.Group.Where(e => e.Id == GeoStruct.Group_Id).FirstOrDefault();
                        var PayStruct = db.SalaryT.Where(e => e.Id == OSalaryTemp.Id).Select(r => r.PayStruct).FirstOrDefault();
                        var Grade = db.Grade.Where(e => e.Id == PayStruct.Grade_Id).FirstOrDefault();
                        var Level = db.Level.Where(e => e.Id == PayStruct.Level_Id).FirstOrDefault();
                        var JobStatus = db.JobStatus.Where(e => e.Id == PayStruct.JobStatus_Id).FirstOrDefault();
                        var EmpActingStatus = JobStatus != null ? db.JobStatus.Where(e => e.Id == JobStatus.Id).Select(r => r.EmpActingStatus).FirstOrDefault() : null;
                        var EmpStatus = JobStatus != null ? db.JobStatus.Where(e => e.Id == JobStatus.Id).Select(r => r.EmpStatus).FirstOrDefault() : null;
                        var FuncStruct = db.SalaryT.Where(e => e.Id == OSalaryTemp.Id).Select(r => r.FuncStruct).FirstOrDefault();
                        var Job = db.Job.Where(e => e.Id == FuncStruct.Job_Id).FirstOrDefault();
                        var JobPosition = db.JobPosition.Where(e => e.Id == FuncStruct.JobPosition_Id).FirstOrDefault();
                        PSlipT.GeoStruct = GeoStruct;
                        PSlipT.PayStruct = PayStruct;
                        PSlipT.FuncStruct = FuncStruct;
                        PSlipT.Corporate = Corporate == null ? "" : Corporate.Name;
                        PSlipT.Region = Region == null ? "" : Region.Name;
                        PSlipT.Company = Company == null ? "" : Company.Name;
                        PSlipT.Division = Division == null ? "" : Division.Name;
                        PSlipT.Location = Location == null ? "" : LocationObj.LocCode + " " + LocationObj.LocDesc;
                        PSlipT.Department = Department == null ? "" : DepartmentObj.DeptCode + " " + DepartmentObj.DeptDesc;
                        PSlipT.Unit = Unit == null ? "" : Unit.Name;
                        PSlipT.Group = Group == null ? "" : Group.Name;
                        PSlipT.Job = Job == null ? "" : Job.Name;
                        PSlipT.JobStatus = JobPosition == null ? "" : JobPosition.JobPositionDesc;
                        PSlipT.Grade = Grade == null ? "" : Grade.Name;
                        PSlipT.Level = Level == null ? "" : Level.Name;
                        PSlipT.EmpStatus = JobStatus == null ? "" : EmpStatus.LookupVal;
                        PSlipT.EmpActingStatus = EmpActingStatus == null ? "" : EmpActingStatus.LookupVal;
                        #region CPInex Calculation
                        if (OCPIEntry != null)
                        {
                            if (OCPIEntry.ActualIndexPoint == 0.0)
                            {
                                var SalHeadDA = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "DA").FirstOrDefault();
                                var EmpSalStructDetailsDA = new EmpSalStructDetails();
                                EmpSalStructDetailsDA = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == OEmpSalStruct.Id && e.SalaryHead_Id == SalHeadDA.Id).SingleOrDefault();
                                if (EmpSalStructDetailsDA != null)
                                {
                                    if (EmpSalStructDetailsDA.SalHeadFormula_Id != null)
                                    {

                                        var PercentDependRule = new PercentDependRule();
                                        PercentDependRule = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetailsDA.SalHeadFormula_Id).Select(r => r.PercentDependRule).FirstOrDefault();
                                        dynamic DAamt = "";

                                        if (PercentDependRule != null)
                                        {
                                            DAamt = PercentDependRule.SalPercent.ToString();
                                        }

                                        PSlipT.CPIndex = DAamt;
                                    }
                                }
                            }
                            else
                            {
                                PSlipT.CPIndex = OCPIEntry.ActualIndexPoint.ToString();

                            }

                        }
                        #endregion CPInex Calculation
                        //  Utility.DumpProcessStatus(LineNo: 387);

                        PSlipT.CPIndexIBase = "";
                        if (ServiceBookDates != null)
                        {
                            if (ServiceBookDates.BirthDate != null)
                                PSlipT.DateBirth = ServiceBookDates.BirthDate.Value;
                            if (ServiceBookDates.ConfirmationDate != null)
                                PSlipT.DateConfirmation = ServiceBookDates.ConfirmationDate.Value;
                            if (ServiceBookDates.LastIncrementDate != null)
                                PSlipT.DateIncrement = ServiceBookDates.LastIncrementDate.Value;
                            if (ServiceBookDates.JoiningDate != null)
                                PSlipT.DateJoining = ServiceBookDates.JoiningDate.Value;
                            if (ServiceBookDates.ProbationDate != null)
                                PSlipT.DateProbation = ServiceBookDates.ProbationDate.Value;
                            if (ServiceBookDates.LastPromotionDate != null)
                                PSlipT.DatePromotion = ServiceBookDates.LastPromotionDate.Value;
                            if (ServiceBookDates.RetirementDate != null)
                                PSlipT.DateRetirement = ServiceBookDates.RetirementDate.Value;
                            if (ServiceBookDates.LastTransferDate != null)
                                PSlipT.DateTransfer = ServiceBookDates.LastTransferDate.Value;
                        }
                        //  Utility.DumpProcessStatus(LineNo: 410);
                        PSlipT.GrossPay = OSalaryT.TotalEarning;
                        PSlipT.DeductionPay = OSalaryT.TotalDeduction;
                        PSlipT.NetPay = OSalaryT.TotalNet;
                        PSlipT.NetPayInWords = NumToWords.ConvertAmount(OSalaryT.TotalNet);//num.changeNumericToWords(OSalaryT.TotalNet);
                        PSlipT.StdPay = OSalaryT.TotalNet;

                        PSlipT.EmpCode = OEmployeePayroll.Employee.EmpCode;

                        string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                        bool existschk = System.IO.Directory.Exists(requiredPathchk);
                        string localPathchk;
                        if (!existschk)
                        {
                            localPathchk = new Uri(requiredPathchk).LocalPath;
                            System.IO.Directory.CreateDirectory(localPathchk);
                        }
                        string pathchk = requiredPathchk + @"\PayslipNameFormat" + ".ini";
                        localPathchk = new Uri(pathchk).LocalPath;
                        string Comp_code = "";
                        int Lvreqcnt = 0;
                        int paramcnt = 0;
                        using (var streamReader = new StreamReader(localPathchk))
                        {
                            string line;

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var CompCode = line;

                                if (CompCode != "")
                                {
                                    Comp_code = CompCode;
                                }
                            }
                        }

                        if (Comp_code != "" && OSalaryTGeo.Geostruct.Company.Code.ToUpper() == Comp_code.ToUpper())//OSalaryTGeo.Geostruct.Company.Code == "GCUB" || OSalaryTGeo.Geostruct.Company.Code == "KB"
                        {
                            PSlipT.EmpName = (OEmployeePayroll.Employee.EmpName.EmpTitle != null ? OEmployeePayroll.Employee.EmpName.EmpTitle.LookupVal : "") + " " + OEmployeePayroll.Employee.EmpName.FullNameFML;//keralbank title require
                        }
                        else { PSlipT.EmpName = OEmployeePayroll.Employee.EmpName.FullNameLFM; }


                        PSlipT.PayableDays = OSalaryT.PaybleDays;
                        if (OEmpAttInfo != null)
                        {
                            PSlipT.LWPDays = OEmpAttInfo.LWPDays;
                        }
                        //   Utility.DumpProcessStatus(LineNo: 425);


                        PSlipT.PaySlipLock = false;
                        PSlipT.PayslipLockDate = null;
                        PSlipT.PayslipNote = "";
                        PSlipT.PayslipRemark = "Salary For month :-> " + OSalaryT.PayMonth;
                        PSlipT.ProcessDate = OSalaryT.ProcessDate;
                        PSlipT.PaymentDate = OSalaryT.PaymentDate;
                        #region BasicScale Calculation
                        string BasicScale = "";

                        var _CompId = Convert.ToInt32(SessionManager.CompanyId);
                        var _CompCode = db.Company.Where(e => e.Id == _CompId).SingleOrDefault().Code.ToUpper();
                        var SalHeadBasic = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").FirstOrDefault();
                        var EmpSalStructDetails = new EmpSalStructDetails();
                        EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == OEmpSalStruct.Id && e.SalaryHead_Id == SalHeadBasic.Id).SingleOrDefault();
                        if (EmpSalStructDetails != null)
                        {
                            if (EmpSalStructDetails.SalHeadFormula_Id != null)
                            {
                                var SalHeadFormula = new SalHeadFormula();
                                SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetails.SalHeadFormula_Id).FirstOrDefault();
                                var BASICDependRule = new BASICDependRule();
                                BASICDependRule = db.SalHeadFormula.Where(e => e.Id == EmpSalStructDetails.SalHeadFormula_Id).Select(r => r.BASICDependRule).FirstOrDefault();
                                var BasicScale_1 = new BasicScale();
                                BasicScale_1 = db.BASICDependRule.Where(e => e.Id == BASICDependRule.Id).Select(r => r.BasicScale).FirstOrDefault();
                                List<BasicScaleDetails> BasicScaleDetails = new List<BasicScaleDetails>();
                                BasicScaleDetails = db.BasicScale.Where(e => e.Id == BasicScale_1.Id).Select(r => r.BasicScaleDetails.ToList()).FirstOrDefault();
                                if (BasicScaleDetails != null)
                                {
                                    BasicScaleDetails = BasicScaleDetails.OrderBy(e => e.StartingSlab).ToList();
                                    if (BasicScaleDetails.Count > 0)
                                    {
                                        foreach (var a in BasicScaleDetails)
                                        {
                                            BasicScale += _returnFormateOfBasicScale(_CompCode, a);
                                        }
                                        if (BasicScale != "")
                                            BasicScale = BasicScale.Remove(BasicScale.Length - 1, 1);
                                    }
                                }
                            }

                        }
                        #endregion BasicScale

                        PSlipT.BasicScale = BasicScale;

                        PSlipT.TotalITaxBalance = 0;
                        PSlipT.TotalITaxLiability = 0;
                        PSlipT.TotalITaxPaid = 0;
                        PSlipT.ITaxPerMonth = 0;
                        PSlipT.FinancialYear = FinnanceYearId != null ? FinnanceYearId : null;



                        PSlipT.PaySlipDetailEarnR = GereratePaySlipDetailEarnR(OSalaryT);
                        PSlipT.PaySlipDetailDedR = GereratePaySlipDetailDedR(OSalaryT);
                        PSlipT.PaySlipDetailLeaveR = GereratePaySlipDetailLeaveR(OEmployeePayroll);
                        PSlipT.DBTrack = dbt;
                    #endregion Payslip_Data_End

                        OSalaryT.PayslipR.Add(PSlipT);
                        db.SaveChanges();
                        //db.PaySlipR.Add(PSlipT);
                        //db.SaveChanges();
                        //var OSalaryTSave = db.SalaryT.Find(OSalaryT.Id);

                        //OSalaryTSave.PayslipR.Add(PSlipT);
                        //db.SaveChanges();
                    }
                }
            }
        }
        public static List<PaySlipDetailEarnR> GereratePaySlipDetailEarnR(SalaryT OSalaryT)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

            if (OSalaryT == null)
            {
                return null;
            }
            var OSalEarn = new List<PaySlipDetailEarnR>();

            var OEmpSalDetails = OSalaryT.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true)
                                    .OrderBy(e => e.SalaryHead.SeqNo).ToList();
            var OEmpSalDetailsEarn = OEmpSalDetails.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").ToList();
            if (OEmpSalDetailsEarn.Count > 0 && OEmpSalDetailsEarn != null)
            {
                foreach (var ca in OEmpSalDetailsEarn)
                {
                    if (ca.Amount != 0)
                    {


                        PaySlipDetailEarnR OSlipEarnT = new PaySlipDetailEarnR()
                        {
                            SalHeadDesc = ca.SalaryHead.Name,
                            EarnAmount = ca.Amount,
                            StdSalAmount = ca.StdAmount,
                            DBTrack = dbt
                        };
                        OSalEarn.Add(OSlipEarnT);
                    }
                }
            }
            return OSalEarn;
        }
        public static List<PaySlipDetailDedR> GereratePaySlipDetailDedR(SalaryT OSalaryT)
        {
            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };

            if (OSalaryT == null)
            {
                return null;
            }
            var OSalDed = new List<PaySlipDetailDedR>();
            var OEmpSalDetails = OSalaryT.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true)
                                    .OrderBy(e => e.SalaryHead.SeqNo).ToList();
            var OEmpSalDetailsDed = OEmpSalDetails.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").ToList();
            if (OEmpSalDetailsDed.Count > 0 && OEmpSalDetailsDed != null)
            {
                foreach (var ca in OEmpSalDetailsDed)
                {
                    if (ca.Amount != 0)
                    {
                        PaySlipDetailDedR OSlipDedT = new PaySlipDetailDedR()
                        {
                            SalHeadDesc = ca.SalaryHead.Name,
                            DedAmount = ca.Amount,
                            StdSalAmount = ca.StdAmount,
                            DBTrack = dbt
                        };
                        OSalDed.Add(OSlipDedT);
                    }
                }
            }
            return OSalDed;
        }
        public static List<PaySlipDetailLeaveR> GereratePaySlipDetailLeaveR(EmployeePayroll OEmployeePayroll)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();
                var Emps = db.EmployeeLeave
                      .Where(e => e.Employee.Id == OEmployeePayroll.Employee.Id)
                      .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                      .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                      .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                      .Include(e => e.Employee.EmpName)
                      .FirstOrDefault();

                var allLvHead = db.LvHead.ToList();
                PaySlipDetailLeaveR oPaySlipDetailLeaveR = new PaySlipDetailLeaveR();
                var OSalLeave = new List<PaySlipDetailLeaveR>();

                //foreach (var ca in Emps)
                //{
                foreach (var lvhead in allLvHead)
                {
                    var openinbal = Emps.LvOpenBal.Where(e => e.LvHead.Id == lvhead.Id).OrderBy(e => e.Id).LastOrDefault();
                    if (openinbal != null)
                    {
                        var bal = Emps.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id && e.LeaveCalendar.FromDate <= DateTime.Now.Date
                                && e.LeaveCalendar.ToDate >= DateTime.Now.Date)
                            .Select(e => new { LvOpening = openinbal.LvOpening, e.CloseBal, e.LvOccurances }).LastOrDefault();
                        var LvData = bal != null ?
                            Emps.LvNewReq.Where(e => e.LeaveCalendar.Id == LvCalendar.Id && e.LeaveHead.Id == lvhead.Id).OrderByDescending(e => e.Id)
                           .Select(e => new
                           {
                               LvCode = e.LeaveHead.LvCode,
                               LvOpening = bal.LvOpening,
                               LvOccurances = bal.LvOccurances,
                               CloseBal = bal.CloseBal
                           }).FirstOrDefault() : null;
                        if (LvData != null)
                        {
                            oPaySlipDetailLeaveR.LeaveCloseBal = LvData.CloseBal;
                            oPaySlipDetailLeaveR.LeaveUtilized = LvData.LvOccurances;
                            oPaySlipDetailLeaveR.LeaveOpenBal = LvData.LvOpening;
                            oPaySlipDetailLeaveR.DBTrack = dbt;
                            oPaySlipDetailLeaveR.LeaveHead = LvData.LvCode;
                            OSalLeave.Add(oPaySlipDetailLeaveR);

                        }
                        else
                        {
                            var LvDataOpening = Emps.LvOpenBal.Where(e => e.LvHead.Id == lvhead.Id && e.LvCalendar.FromDate <= DateTime.Now.Date
                                && e.LvCalendar.ToDate >= DateTime.Now.Date).OrderByDescending(e => e.Id)
                                 .Select(e => new
                                 {
                                     LvCode = e.LvHead.LvCode,
                                     LvOpening = e.LvOpening,
                                     LvOccurances = e.LvOccurances,
                                     CloseBal = e.LvOpening
                                 }).FirstOrDefault();

                            if (LvDataOpening != null)
                            {
                                oPaySlipDetailLeaveR.LeaveCloseBal = LvDataOpening.CloseBal;
                                oPaySlipDetailLeaveR.LeaveUtilized = LvDataOpening.LvOccurances;
                                oPaySlipDetailLeaveR.LeaveOpenBal = LvDataOpening.LvOpening;
                                oPaySlipDetailLeaveR.LeaveHead = LvDataOpening.LvCode;
                                oPaySlipDetailLeaveR.DBTrack = dbt;
                                OSalLeave.Add(oPaySlipDetailLeaveR);

                            }
                        }
                        oPaySlipDetailLeaveR = new PaySlipDetailLeaveR();
                    }
                }
                //}

                return OSalLeave;
            }
        }
        public static void GererateAnnualSalaryR(int mEmployeePayroll_Id, Calendar OFinancialYear)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var OEmployeePayroll = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.GeoStruct)
                    .Include(e => e.Employee.FuncStruct)
                    .Include(e => e.Employee.PayStruct)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.EmpOffInfo)

                    .Include(e => e.SalaryT)

                    .Include(e => e.AnnualSalary)
                    .Include(e => e.AnnualSalary.Select(r => r.FinancialYear))
                    .Include(e => e.AnnualSalary.Select(r => r.AnnualSalaryDetailsR))

                    .Where(e => e.Id == mEmployeePayroll_Id)
                    .SingleOrDefault();

                var OAnnualCheck = OEmployeePayroll.AnnualSalary.Where(e => e.FinancialYear.Id == OFinancialYear.Id).SingleOrDefault();
                if (OAnnualCheck != null)
                {
                    if (OAnnualCheck.AnnualSalaryDetailsR != null)
                    {
                        // OSalaryTChk.SalEarnDedT.ToList().ForEach(r => db.SalEarnDedT.(r));

                        foreach (var ca in OAnnualCheck.AnnualSalaryDetailsR.ToList())
                        {
                            db.AnnualSalaryDetailsR.Attach(ca);
                            db.Entry(ca).State = System.Data.Entity.EntityState.Deleted;
                        }
                    }
                    db.AnnualSalaryR.Attach(OAnnualCheck);
                    db.Entry(OAnnualCheck).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }

                var OSalaryT = OEmployeePayroll.SalaryT.Where(e => OFinancialYear.FromDate.Value <= Convert.ToDateTime("01/" + e.PayMonth)
                    && OFinancialYear.ToDate.Value >= Convert.ToDateTime("01/" + e.PayMonth)).ToList();



                var OSalMonthData = SalaryHeadMonthDataForAnnual(OEmployeePayroll, OFinancialYear);
                //OSalMonthData = OSalMonthData.OrderBy(e => e.SalaryHead.Id);
                List<AnnualSalaryDetailsR> OEmpAnnualSalList = new List<AnnualSalaryDetailsR>();
                if (OSalMonthData.Count > 0 || OSalMonthData != null)
                {
                    AnnualSalaryR OEmpAnnualSal = new AnnualSalaryR()
                    {
                        DBTrack = dbt,
                        EmpCode = OEmployeePayroll.Employee.EmpCode,
                        EmpName = OEmployeePayroll.Employee.EmpName.FullNameFML,
                        FinancialYear = db.Calendar.Find(OFinancialYear.Id),
                        FuncStruct = OEmployeePayroll.Employee.FuncStruct == null ? null : db.FuncStruct.Find(OEmployeePayroll.Employee.FuncStruct.Id),
                        GeoStruct = db.GeoStruct.Find(OEmployeePayroll.Employee.GeoStruct.Id),
                        PayStruct = db.PayStruct.Find(OEmployeePayroll.Employee.PayStruct.Id)
                    };

                    var OSalHeadTotal = OSalMonthData.GroupBy(e => e.SalaryHead)
                        .Select(r => new { SalaryHead = r.Key, TotAmout = r.Sum(t => t.ActualAmount) }).ToList();

                    foreach (var ca in OSalHeadTotal)
                    {
                        var OSalHeadType = db.SalaryHead
                        .Include(e => e.Type)
                        .Where(e => e.Id == ca.SalaryHead.Id)
                        .SingleOrDefault();

                        var OAnnualSalarySalHeadCheck = OSalMonthData.Where(e => e.SalaryHead == ca.SalaryHead).ToList();
                        AnnualSalaryDetailsR OEmpAnnualSalDetails = new AnnualSalaryDetailsR();
                        foreach (var ca1 in OAnnualSalarySalHeadCheck)
                        {
                            OEmpAnnualSalDetails.DBTrack = dbt;
                            OEmpAnnualSalDetails.SalCode = ca.SalaryHead.Code;
                            OEmpAnnualSalDetails.SalHeadDesc = ca.SalaryHead.Name;
                            OEmpAnnualSalDetails.SalType = OSalHeadType.Type.LookupVal;
                            OEmpAnnualSalDetails.SalAmountTotal = ca.TotAmout;
                            OEmpAnnualSalDetails.ITAppl = ca.SalaryHead.InITax;
                            OEmpAnnualSalDetails.PaySlipAppl = ca.SalaryHead.InPayslip;

                            switch (Convert.ToDateTime("01/" + ca1.PayMonth).Date.ToString("MM"))
                            {
                                case "01":
                                    OEmpAnnualSalDetails.AmountM01 = ca1.ActualAmount;
                                    break;
                                case "02":
                                    OEmpAnnualSalDetails.AmountM02 = ca1.ActualAmount;
                                    break;
                                case "03":
                                    OEmpAnnualSalDetails.AmountM03 = ca1.ActualAmount;
                                    break;
                                case "04":
                                    OEmpAnnualSalDetails.AmountM04 = ca1.ActualAmount;
                                    break;
                                case "05":
                                    OEmpAnnualSalDetails.AmountM05 = ca1.ActualAmount;
                                    break;
                                case "06":
                                    OEmpAnnualSalDetails.AmountM06 = ca1.ActualAmount;
                                    break;
                                case "07":
                                    OEmpAnnualSalDetails.AmountM07 = ca1.ActualAmount;
                                    break;
                                case "08":
                                    OEmpAnnualSalDetails.AmountM08 = ca1.ActualAmount;
                                    break;
                                case "09":
                                    OEmpAnnualSalDetails.AmountM09 = ca1.ActualAmount;
                                    break;
                                case "10":
                                    OEmpAnnualSalDetails.AmountM10 = ca1.ActualAmount;
                                    break;
                                case "11":
                                    OEmpAnnualSalDetails.AmountM11 = ca1.ActualAmount;

                                    break;
                                case "12":
                                    OEmpAnnualSalDetails.AmountM12 = ca1.ActualAmount;
                                    break;

                                default:
                                    break;

                            }

                        };
                        OEmpAnnualSalList.Add(OEmpAnnualSalDetails);


                    }
                    OEmpAnnualSal.AnnualSalaryDetailsR = OEmpAnnualSalList;
                    db.AnnualSalaryR.Add(OEmpAnnualSal);
                    db.SaveChanges();

                    var OEmpPayrollAnnual = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                    OEmpPayrollAnnual.AnnualSalary.Add(OEmpAnnualSal);
                    db.Entry(OEmpPayrollAnnual).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
        public static List<ITSalaryHeadData> SalaryHeadMonthDataForAnnual(EmployeePayroll OEmployeePayroll, Calendar OFinancialYear)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var OSalaryTEmp = db.EmployeePayroll
                //     .Include(e => e.SalaryT)
                //     .Include(e => e.SalaryT.Select(r => r.PerkTransT))
                //     .Include(e => e.SalaryT.Select(r => r.PerkTransT.Select(t => t.SalaryHead)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Frequency)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.Type)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(w => w.SalaryHead.ProcessType)))
                //     .Include(e => e.SalaryArrearT)
                //     .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                //     .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                //    //.Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(w => w.SalaryArrearPFT)))
                //     .Where(e => e.Id == OEmployeePayroll.Id)
                //     .SingleOrDefault();



                var OITSalIncome = new List<ITSalaryHeadData>(); //Salary income array
                ITSalaryHeadData OSalayIncomeObj = null;

                var mPeriodRange = "";
                var mPeriod = new List<string>();

                for (DateTime mTempDate = OFinancialYear.FromDate.Value; mTempDate <= OFinancialYear.ToDate.Value; mTempDate = mTempDate.AddMonths(1))
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

                //**** Actual salary earnings/deductions ****************//
                var OSalaryT = new List<SalaryT>();

                foreach (var item in mPeriod)
                {
                    var temp = db.SalaryT.Include(e => e.SalEarnDedT)
                             .Include(e => e.SalEarnDedT.Select(w => w.SalaryHead))
                        //.Include(e => e.SalEarnDedT.Select(w => w.SalaryHead.Frequency))
                             .Include(e => e.SalEarnDedT.Select(w => w.SalaryHead.Type))
                             .Include(e => e.SalEarnDedT.Select(w => w.SalaryHead.SalHeadOperationType))
                        //.Include(e => e.SalEarnDedT.Select(w => w.SalaryHead.RoundingMethod))
                        //.Include(e => e.SalEarnDedT.Select(w => w.SalaryHead.ProcessType))
                    .Where(s => s.PayMonth == item && s.EmployeePayroll_Id == OEmployeePayroll.Id)
                    .SingleOrDefault();
                    if (temp != null)
                    {
                        OSalaryT.Add(temp);
                    }
                }


                //.SelectMany(e=>e.SalEarnDedT).ToList();
                foreach (var ca in OSalaryT)
                {
                    var OSalaryHeadTotalActualEarning = ca.SalEarnDedT
                     .Where(q => q.SalaryHead.InITax == true && q.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && q.Amount != 0 && q.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PERK")
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadData
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead,
                        ActualAmount = i.Sum(w => w.Amount),
                        ProjectedAmount = i.Sum(w => w.Amount),

                    }).ToList();

                    OITSalIncome.AddRange(OSalaryHeadTotalActualEarning);
                }

                foreach (var ca in OSalaryT)
                {
                    var OSalaryHeadTotalActualEarning = ca.SalEarnDedT
                     .Where(q => q.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && q.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "ARREARDED" && q.Amount != 0 && q.SalaryHead.InPayslip == true)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadData
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead,
                        ActualAmount = i.Sum(w => w.Amount),
                        ProjectedAmount = i.Sum(w => w.Amount),

                    }).ToList();

                    OITSalIncome.AddRange(OSalaryHeadTotalActualEarning);
                }



                //calculate salary head total from salaryt details headwise

                var OSalaryArrearT = new List<SalaryArrearT>();
                EmployeePayroll OSalaryTEmp = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                   .Include(e => e.SalaryArrearT)
                   .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead.SalHeadOperationType)))
                   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead))).SingleOrDefault();

                foreach (var item in mPeriod)
                {


                    var temp = OSalaryTEmp.SalaryArrearT
                        .Where(s => s.PayMonth == item && s.IsRecovery == false)
                        .ToList();
                    if (temp != null)
                    {
                        OSalaryArrearT.AddRange(temp);
                    }

                }

                foreach (var ca in OSalaryArrearT)
                {
                    var OSalArrearPayment = ca.SalaryArrearPaymentT
                         .Where(q => q.SalaryHead.InITax == true && q.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && q.SalHeadAmount != 0)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadData
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead,
                        ActualAmount = i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OSalArrearPayment);
                }
                foreach (var ca in OSalaryArrearT)
                {
                    var OSalArrearPayment = ca.SalaryArrearPaymentT
                         .Where(q => q.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && q.SalHeadAmount != 0 && q.SalaryHead.InPayslip == true)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadData
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead,
                        ActualAmount = i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OSalArrearPayment);
                }
                //arrears deductions
                var OSalaryArrearTDeduction = new List<SalaryArrearT>();
                foreach (var item in mPeriod)
                {
                    var temp = OSalaryTEmp.SalaryArrearT
                         .Where(s => s.PayMonth == item && s.IsRecovery == true)
                         .ToList();
                    if (temp != null)
                    {
                        OSalaryArrearTDeduction.AddRange(temp);
                    }
                }
                foreach (var ca in OSalaryArrearTDeduction)
                {
                    var OSalArrearPayment = ca.SalaryArrearPaymentT
                         .Where(q => q.SalaryHead.InITax == true && q.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && q.SalHeadAmount != 0)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadData
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead,
                        ActualAmount = -i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = -i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OSalArrearPayment);
                }
                foreach (var ca in OSalaryArrearTDeduction)
                {
                    var OSalArrearPayment = ca.SalaryArrearPaymentT
                         .Where(q => q.SalaryHead.Type.LookupVal.ToUpper() == "DEDUTION" && q.SalHeadAmount != 0 && q.SalaryHead.InPayslip == true)
                    .GroupBy(e => new { e.SalaryHead })

                    .Select(i => new ITSalaryHeadData
                    {
                        PayMonth = ca.PayMonth,
                        SalaryHead = i.Key.SalaryHead,
                        ActualAmount = -i.Sum(w => w.SalHeadAmount),
                        ProjectedAmount = -i.Sum(w => w.SalHeadAmount),

                    }).ToList();
                    OITSalIncome.AddRange(OSalArrearPayment);
                }


                //yearlypayment
                List<YearlyPaymentT> OSalaryYearlyPaymenttDeduction = new List<YearlyPaymentT>();
                EmployeePayroll OYearlyPaymentT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                      .Include(e => e.YearlyPaymentT)
                      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.SalHeadOperationType))
                    // .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Frequency))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Type))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.RoundingMethod))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.ProcessType))
                      .SingleOrDefault();
                foreach (string item in mPeriod)
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
                    if (ca.ReleaseFlag == true)
                    {

                        int a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                        if (a <= 0)
                        {
                            var OSalYearPayment = OSalaryYearlyPaymenttDeduction
                                               .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth && q.SalaryHead.Id == ca.SalaryHead.Id)
                                               .GroupBy(e => new { e.SalaryHead })

                                               .Select(i => new ITSalaryHeadData
                                               {
                                                   PayMonth = ca.PayMonth,
                                                   SalaryHead = i.Key.SalaryHead,
                                                   ActualAmount = i.Sum(w => w.AmountPaid),
                                                   ProjectedAmount = i.Sum(w => w.AmountPaid),

                                               }).ToList();
                            OITSalIncome.AddRange(OSalYearPayment);
                        }
                    }
                    else
                    {
                        Int32 a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                        if (a <= 0)
                        {
                            var OSalYearPayment = OSalaryYearlyPaymenttDeduction
                                               .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth && q.SalaryHead.Id == ca.SalaryHead.Id)
                                               .GroupBy(e => new { e.SalaryHead })

                                               .Select(i => new ITSalaryHeadData
                                               {
                                                   PayMonth = ca.PayMonth,
                                                   SalaryHead = i.Key.SalaryHead,
                                                   ActualAmount = 0,
                                                   ProjectedAmount = i.Sum(w => w.AmountPaid),

                                               }).ToList();
                            OITSalIncome.AddRange(OSalYearPayment);
                        }
                    }
                }

                // Yearly payment tax 
                List<YearlyPaymentT> OSalaryYearlyPaymenttDeductionTax = new List<YearlyPaymentT>();
                EmployeePayroll OYearlyPaymentTTAX = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                      .Include(e => e.YearlyPaymentT)
                      .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.SalHeadOperationType))
                    // .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Frequency))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.Type))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.RoundingMethod))
                    //.Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.ProcessType))
                      .SingleOrDefault();
                foreach (string item in mPeriod)
                {

                    List<YearlyPaymentT> OSalaryYearlyPaymenttDeduction_2 = OYearlyPaymentTTAX.YearlyPaymentT
                           .Where(s => s.PayMonth == item)
                           .ToList();
                    if (OSalaryYearlyPaymenttDeduction_2 != null)
                    {
                        OSalaryYearlyPaymenttDeductionTax.AddRange(OSalaryYearlyPaymenttDeduction_2);
                    }
                }

                var Anualtax = db.SalaryHead.Where(r => r.Code == "OTHERTAX").SingleOrDefault();
                if (Anualtax != null)
                {
                    foreach (YearlyPaymentT ca in OSalaryYearlyPaymenttDeductionTax)
                    {
                        if (ca.ReleaseFlag == true)
                        {

                            int a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                            if (a <= 1)
                            {
                                var OSalYearPayment = OSalaryYearlyPaymenttDeductionTax
                                                   .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth && q.TDSAmount != 0 && q.SalaryHead.Id == ca.SalaryHead.Id)
                                                   .GroupBy(e => new { e.SalaryHead })

                                                   .Select(i => new ITSalaryHeadData
                                                   {
                                                       PayMonth = ca.PayMonth,
                                                       SalaryHead = Anualtax,
                                                       ActualAmount = i.Sum(w => w.TDSAmount),
                                                       ProjectedAmount = i.Sum(w => w.TDSAmount),

                                                   }).ToList();
                                OITSalIncome.AddRange(OSalYearPayment);
                            }
                        }
                        else
                        {
                            Int32 a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                            if (a <= 1)
                            {
                                var OSalYearPayment = OSalaryYearlyPaymenttDeductionTax
                                                   .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth && q.TDSAmount != 0 && q.SalaryHead.Id == ca.SalaryHead.Id)
                                                   .GroupBy(e => new { e.SalaryHead })

                                                   .Select(i => new ITSalaryHeadData
                                                   {
                                                       PayMonth = ca.PayMonth,
                                                       SalaryHead = Anualtax,
                                                       ActualAmount = 0,
                                                       ProjectedAmount = i.Sum(w => w.TDSAmount),

                                                   }).ToList();
                                OITSalIncome.AddRange(OSalYearPayment);
                            }
                        }
                    }

                }
                ////**************** Perk salary ******************//

                //calculate salary head total from PerkTransT details headwise
                //**** Actual salary earnings/deductions ****************//
                var OPerkT = new List<SalaryT>();
                foreach (var item in mPeriod)
                {
                    var temp = db.SalaryT.Include(e => e.PerkTransT)
                        .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                    .Where(s => s.PayMonth == item && s.EmployeePayroll_Id == OEmployeePayroll.Id).SingleOrDefault();
                    if (temp != null)
                    {
                        OPerkT.Add(temp);
                    }
                }
                var OPerkT1 = OPerkT.Where(e => e.PerkTransT != null).ToList();
                if (OPerkT1.Count > 0)
                {

                    foreach (var ca in OPerkT)
                    {
                        var OSalaryHeadTotalActualEarning = ca.PerkTransT
                        .Where(q => q.SalaryHead.InITax == true)
                        .GroupBy(e => new { e.SalaryHead })

                        .Select(i => new ITSalaryHeadData
                        {
                            PayMonth = ca.PayMonth,
                            SalaryHead = i.Key.SalaryHead,
                            ActualAmount = i.Sum(w => w.ActualAmount),
                            ProjectedAmount = i.Sum(w => w.ProjectedAmount),

                        }).ToList();

                        OITSalIncome.AddRange(OSalaryHeadTotalActualEarning);
                    }
                }

                List<PerkTransT> OPerkTransT = new List<PerkTransT>();
                EmployeePayroll OPerkdata = db.EmployeePayroll
                     .AsNoTracking().Where(e => e.Id == OEmployeePayroll.Id)
                       .Include(e => e.PerkTransT)
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead))
                       .Include(e => e.PerkTransT.Select(r => r.SalaryHead.SalHeadOperationType))
                   .SingleOrDefault();
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
                foreach (PerkTransT ca in OPerkTransT)
                {
                    int a = OITSalIncome.Where(r => r.SalaryHead.Id == ca.SalaryHead.Id && r.PayMonth == ca.PayMonth).Distinct().Count();
                    if (a <= 0)
                    {
                        var OSalYearPayment = OPerkTransT
                                           .Where(q => q.SalaryHead.InITax == true && q.PayMonth == ca.PayMonth)
                                           .GroupBy(e => new { e.SalaryHead })

                                           .Select(i => new ITSalaryHeadData
                                           {
                                               PayMonth = ca.PayMonth,
                                               SalaryHead = i.Key.SalaryHead,
                                               ActualAmount = i.Sum(w => w.ActualAmount),
                                               ProjectedAmount = i.Sum(w => w.ProjectedAmount),

                                           }).ToList();
                        OITSalIncome.AddRange(OSalYearPayment);
                    }
                }

                //Add all Salary Components monthwise
                var OITSalMonthWise = new List<ITSalaryHeadData>();
                var OSalTotalT = OITSalIncome.GroupBy(e => new { PayMonth = e.PayMonth, SalHead = e.SalaryHead })
                    .Select(r => new ITSalaryHeadData
                    {
                        PayMonth = r.Key.PayMonth,
                        SalaryHead = r.Key.SalHead,
                        ActualAmount = r.Sum(d => d.ActualAmount),
                        ProjectedAmount = r.Sum(d => d.ProjectedAmount)
                    }
                    ).ToList();
                OITSalMonthWise.AddRange(OSalTotalT);

                return OITSalMonthWise;
            }
        }


        public static void GratuityCalc(int mCompanyPayroll_Id, int mEmployeePayroll_Id, DateTime mTillDate)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //db.RefreshAllEntites(RefreshMode.StoreWins);
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var OCompanyPayroll = db.CompanyPayroll
                                    .Include(e => e.GratuityAct)
                                    .Include(e => e.GratuityAct.Select(r => r.GratuityWages))
                                    .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster))
                                    .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster.Select(q => q.SalHead)))
                                    .Where(e => e.Id == mCompanyPayroll_Id).SingleOrDefault();

                var OGratuityAct = OCompanyPayroll.GratuityAct
                                .Where(e => e.EndDate == null).SingleOrDefault();

                var OEmployeePayroll = db.EmployeePayroll
                    .Include(e =>e.Employee)
                        .Include(e =>e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.JobStatus)
                         .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                                    .Include(e => e.Employee.ServiceBookDates)
                                    .Include(e => e.EmpSalStruct)
                                    .Include(e => e.GratuityT)
                                    .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();


                var OEmpSalStructChk = OEmployeePayroll.EmpSalStruct
                                .Where(e => mTillDate >= e.EffectiveDate && (mTillDate <= e.EndDate || e.EndDate == null)).SingleOrDefault();

                var OEmpSalStruct = db.EmpSalStruct
                                .Include(e => e.FuncStruct)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.PayStruct)
                                .Include(e => e.EmpSalStructDetails)
                                .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                                .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                                .Where(e => e.Id == OEmpSalStructChk.Id).SingleOrDefault();
                var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.ToList();

                var OEmpGratuituChak = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "GRATUITY").SingleOrDefault();
                if (OEmpGratuituChak == null)//gratuity head present
                {
                    return;
                }
                if (OGratuityAct == null)//gratuity act present
                {
                    return;
                }
                //employee date of joing and birthdate should not be null
                DateTime? mEffectiveDate;
                if (OGratuityAct.IsDateOfJoin == true)
                {
                    if (OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value == null)
                    {
                        return;
                    }
                    mEffectiveDate = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value;
                }
                else if (OGratuityAct.IsDateOfConfirm == true)
                {
                    if (OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value == null)
                    {
                        return;
                    }
                    mEffectiveDate = OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value;
                }
                //existing record deletion
                if (OEmployeePayroll.GratuityT != null && OEmployeePayroll.GratuityT.Count() > 0)
                {
                    var OEmpGratuityDel = OEmployeePayroll.GratuityT.SingleOrDefault();

                    db.GratuityT.Attach(OEmpGratuityDel);
                    db.Entry(OEmpGratuityDel).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                //both conditions are ok.
                double mGratuity = 0;
                double mTotActService = 0;
                Int32 mTotEffectiveService = 0;
                //calculate service and do rounding to six month
                DateTime start = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value;
                DateTime end = mTillDate;
                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                double daysInEndMonth = (end - end.AddMonths(1)).Days;
                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                int mRemainder = 0;
                mTotEffectiveService = Convert.ToInt32(Math.DivRem(Convert.ToInt32(months), 12, out mRemainder));
                mTotActService = mTotEffectiveService;
                if (mRemainder > 6)
                {
                    mTotEffectiveService = mTotEffectiveService + 1;
                }
                if (mTotEffectiveService >= OGratuityAct.ServiceFrom || OEmployeePayroll.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal == "EXPIRED")
                {
                    
               
                //take wages to calculate gratuity
                var mGratuityWages = SalaryHeadGenProcess.WagecalcDirect(OGratuityAct.GratuityWages, null, OEmpSalDetails);

                mGratuity = ((mGratuityWages * OGratuityAct.PayableDays) / OGratuityAct.MonthDays) * mTotEffectiveService;
                mGratuity = SalaryHeadGenProcess.RoundingFunction(OEmpGratuituChak.SalaryHead, mGratuity);
                //save gratuity data

                GratuityT OGratuityT = new GratuityT()
                {
                    ActualService = mTotActService,
                    Amount = mGratuity,
                    FuncStruct = db.FuncStruct.Find(OEmpSalStruct.FuncStruct.Id),
                    GeoStruct = db.GeoStruct.Find(OEmpSalStruct.GeoStruct.Id),
                    PayStruct = db.PayStruct.Find(OEmpSalStruct.PayStruct.Id),
                    ProcessDate = mTillDate,
                    RoundedService = mTotEffectiveService,
                    TotalLWP = 0,
                    DBTrack = dbt
                };
                db.GratuityT.Add(OGratuityT);
                db.SaveChanges();
                var OEmpPayrollSave1 = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                OEmpPayrollSave1.GratuityT.Add(OGratuityT);
                db.Entry(OEmpPayrollSave1).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                }
                else
                {
                   var empcode =  OEmployeePayroll.Employee.EmpCode;
                   System.Web.HttpContext.Current.Session["empcodeMsg"] = empcode;
                }
            }
        }
        public static EmployeePayroll _returnITInvestmentPayment(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpOffInfo)
                        .Include(e => e.Employee.EmpOffInfo.NationalityID)
                        .Include(e => e.Employee.Gender)
                          .Include(e => e.Employee.ServiceBookDates)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.ITProjection)
                           .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                            .Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme))
                        .Where(e => e.Employee.Id == Emp).SingleOrDefault();
            }
        }

        public static Employee _returnEmployeePayroll(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.Employee.Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Company)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.PayStruct)
                    .Include(e => e.ServiceBookDates)
                    .Where(r => r.Id == Emp).SingleOrDefault();
            }
        }

        public static Double[] TDSCalc(EmployeePayroll OEmployeePayroll, IncomeTax OITMaster, double mTotalITIncome, DateTime mToPeriod)
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
                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
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
                var FinYear = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == FinYear.Id).SingleOrDefault();
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
                List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails)
                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead))
                    .Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead.SalHeadOperationType))
                    .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id).ToList();
                ////EmpSalStruct OEmpSal = OEmployeePayroll.EmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                ////if (OEmpSal == null)
                ////{
                ////    OEmpSal = OEmployeePayroll.EmpSalStruct.LastOrDefault();
                ////}

                EmpSalStruct OEmpSal = OEmpSalStruct.Where(e => e.EndDate == null).SingleOrDefault();
                if (OEmpSal == null)
                {
                    OEmpSal = OEmpSalStruct.LastOrDefault();
                }
                EmpSalStructDetails OSalaryHead = OEmpSal.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
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

                double[] mPerc = new Double[5];
                mPerc[0] = 0;
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
                        mPerc[0] = OITTDSData.Percentage;
                        mPerc[1] = OITTDSData.EduCessPercent;

                        break;
                    }
                    else
                    {

                        double OTDSTemp = ((OITTDSData.IncomeRangeTo - OITTDSData.IncomeRangeFrom) * OITTDSData.Percentage / 100) + OITTDSData.Amount;
                        mITTax = mITTax + OTDSTemp;
                        mPerc[0] = OITTDSData.Percentage;
                        mPerc[1] = OITTDSData.EduCessPercent;
                    }
                }
                // OTDSDetails[0] = Process.SalaryHeadGenProcess.RoundingFunction(OSalaryHead.SalaryHead, mITTax);

                // return OTDSDetails;
                return mPerc;
            }
        }

        public static void BonusCalc(int mCompanyPayroll_Id, int mEmployeePayroll_Id, Calendar OBonusCalendar, bool TaxCalcIsallowed, int ProcType)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var OCompanyPayroll = db.CompanyPayroll
                                    .Include(e => e.BonusAct)
                                    .Include(e => e.BonusAct.Select(r => r.BonusCalendar))
                                    .Include(e => e.BonusAct.Select(r => r.BonusWages))
                                    .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster))
                                    .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(z => z.SalHead)))
                                    .Where(e => e.Id == mCompanyPayroll_Id).SingleOrDefault();



                var OBonusAct = OCompanyPayroll.BonusAct
                                .Where(e => e.BonusCalendar.Id == OBonusCalendar.Id).FirstOrDefault();

                int strid = db.EmployeePayroll
                    .Include(e => e.EmpSalStruct)
                    .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault().EmpSalStruct.Select(r => r.Id).LastOrDefault();

                //var EmpSalStructlist = db.EmpSalStruct
                //    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))
                //    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Type))
                //    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType))
                //    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                //    .Where(r => r.Id == strid).SingleOrDefault();

                List<EmpSalStruct> EmpSalStructTotal = new List<EmpSalStruct>();
                List<EmpSalStructDetails> EmpSalStructDetails = new List<EmpSalStructDetails>();
                List<PayScaleAssignment> PayScaleAssignment = new List<PayScaleAssignment>();
                var PayScaleAssignmentObj = new PayScaleAssignment();
                List<SalaryHead> SalaryHead = new List<SalaryHead>();
                var SalaryHeadObj = new SalaryHead();
                List<SalHeadFormula> SalHeadFormula = new List<SalHeadFormula>();
                var SalaryHeadFormulaObj = new SalHeadFormula();

                EmpSalStructTotal = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == mEmployeePayroll_Id && e.Id == strid).ToList();
                foreach (var i in EmpSalStructTotal)
                {
                    EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id).ToList();
                    foreach (var j in EmpSalStructDetails)
                    {
                        var SalHeadTmp = new SalaryHead();
                        //PayScaleAssignmentObj = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                        // SalaryHeadFormulaObj = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                        var id = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(r => r.Id == j.Id).Select(r => r.SalaryHead.Id).FirstOrDefault();


                        SalHeadTmp = db.SalaryHead.Where(e => e.Id == id).FirstOrDefault();

                        SalHeadTmp.SalHeadOperationType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.SalHeadOperationType).FirstOrDefault();
                        SalHeadTmp.Frequency = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Frequency).FirstOrDefault();
                        SalHeadTmp.Type = db.SalaryHead.Where(e => e.Id == id).Select(e => e.Type).FirstOrDefault();
                        // SalHeadTmp.RoundingMethod = db.SalaryHead.Where(e => e.Id == id).Select(e => e.RoundingMethod).FirstOrDefault();
                        SalHeadTmp.ProcessType = db.SalaryHead.Where(e => e.Id == id).Select(e => e.ProcessType).FirstOrDefault();
                        //SalHeadTmp.LvHead = db.SalaryHead.Where(e => e.Id == id).Select(e => e.LvHead.ToList()).FirstOrDefault();// to be check for output
                        //SalaryHead.Add(SalHeadTmp);
                        //SalHeadFormula.Add(j.SalHeadFormula)  ;
                        j.SalaryHead = SalHeadTmp;

                    }
                    i.EmpSalStructDetails = EmpSalStructDetails;
                }
                var EmpSalStructlist = EmpSalStructTotal.SingleOrDefault();



                var OEmployeePayroll = db.EmployeePayroll
                                    .Include(e => e.BonusChkT)
                    //.Include(e => e.ITSalaryHeadData)
                    // .Include(e => e.ITSalaryHeadData.Select(r => r.SalaryHead))
                    // .Include(e => e.EmpSalStruct)
                                    .Include(e => e.SalAttendance)
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                    .Include(e => e.Employee.EmpOffInfo)
                                    .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                    .Include(e => e.Employee.ServiceBookDates)
                                    .Include(e => e.Employee.EmpName)
                                    .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();


                //var OEmpSalStructChk = OEmployeePayroll.ITSalaryHeadData
                //                .Where(e => Convert.ToDateTime("01/" + e.PayMonth).Date >= OBonusCalendar.FromDate.Value && Convert.ToDateTime("01/" + e.PayMonth).Date <= OBonusCalendar.ToDate.Value).ToList();

                //var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                //    //.Where(e => e.EndDate == null).SingleOrDefault();
                //                   .LastOrDefault();
                var OEmpSalDetails = EmpSalStructlist.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "BONUS").SingleOrDefault();




                if (OEmpSalDetails == null)//bonus head present
                {
                    return;
                }
                if (OBonusAct == null)//bonus act present
                {
                    return;
                }
                //employee date of joing and birthdate should not be null
                DateTime? mEffectiveDate;

                //existing record deletion
                if (OEmployeePayroll.BonusChkT != null && OEmployeePayroll.BonusChkT.Count() > 0)
                {
                    var OEmpBonusDel = OEmployeePayroll.BonusChkT.SingleOrDefault();

                    db.BonusChkT.Attach(OEmpBonusDel);
                    db.Entry(OEmpBonusDel).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();

                }
                //both conditions are ok.
                double mGratuity = 0;
                double mTotActService = 0;
                Int32 mTotEffectiveService = 0;
                // List<ITSalaryHeadData> OSalaryHeadData = new List<ITSalaryHeadData>();
                // OSalaryHeadData = IncomeTaxCalc.SalaryHeadMonthData(OEmployeePayroll, DateTime.Now, OBonusCalendar.FromDate.Value, OBonusCalendar.ToDate.Value, null);
                // for yearly payment month period start
                Calendar FinYr = db.Calendar.Where(e => e.Name.LookupVal.ToUpper().ToString() == "BONUSYEAR" && e.Default == true).SingleOrDefault();

                DateTime? FromDateyear = db.Calendar.Where(e => e.Id == FinYr.Id)
                                 .Select(e => e.FromDate)
                                .SingleOrDefault();
                DateTime? ToDateyear = db.Calendar.Where(e => e.Id == FinYr.Id)
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


                List<IncomeTaxCalc.ITSalaryHeadDataTemp> OSalaryHeadData = new List<IncomeTaxCalc.ITSalaryHeadDataTemp>();
                OSalaryHeadData = IncomeTaxCalc.SalaryHeadMonthData(OEmployeePayroll, DateTime.Now, OBonusCalendar.FromDate.Value, OBonusCalendar.ToDate.Value, null, mPeriodYear, 0);

                if (OSalaryHeadData == null || OSalaryHeadData.Count() == 0)
                {
                    return;
                }
                else
                {
                    BonusChkT OBonusCheckList = new BonusChkT();

                    OBonusCheckList.BonusCalendar = db.Calendar.Find(OBonusCalendar.Id);
                    for (var mdate = OBonusCalendar.FromDate.Value; mdate <= OBonusCalendar.ToDate.Value; mdate = mdate.AddMonths(1))
                    {
                        var OSalForCal = OSalaryHeadData.Where(e => e.PayMonth == mdate.ToString("MM/yyyy")).ToList();
                        if (OSalForCal != null && OSalForCal.Count() > 0)
                        {
                            double mWorkingDays = 0;
                            var OWorkDay = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == mdate.ToString("MM/yyyy")).SingleOrDefault();
                            mWorkingDays = OWorkDay == null ? 0 : (OWorkDay.PaybleDays + OWorkDay.ArrearDays);

                            switch (mdate.ToString("MM"))
                            {
                                case "01":
                                    OBonusCheckList.WorkingDays_01 = mWorkingDays;
                                    OBonusCheckList.BonusWages_01 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_01 = 0;
                                        OBonusCheckList.TotalAmount_01 = OBonusCheckList.Bonus_01;
                                    }
                                    else if (OBonusCheckList.BonusWages_01 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_01;
                                        OBonusCheckList.TotalAmount_01 = OBonusCheckList.Bonus_01 + OBonusCheckList.ExGracia_01;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_01 = 0;
                                        OBonusCheckList.ExGracia_01 = Math.Round(OBonusCheckList.BonusWages_01 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_01;
                                        OBonusCheckList.TotalAmount_01 = OBonusCheckList.Bonus_01 + OBonusCheckList.ExGracia_01;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_01 = 0;
                                        OBonusCheckList.ExGracia_01 = 0;
                                        OBonusCheckList.TotalAmount_01 = 0;
                                    }
                                    break;
                                case "02":
                                    OBonusCheckList.WorkingDays_02 = mWorkingDays;
                                    OBonusCheckList.BonusWages_02 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_02 = 0;
                                        OBonusCheckList.TotalAmount_02 = OBonusCheckList.Bonus_02;
                                    }
                                    else if (OBonusCheckList.BonusWages_02 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_02;
                                        OBonusCheckList.TotalAmount_02 = OBonusCheckList.Bonus_02 + OBonusCheckList.ExGracia_02;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_02 = 0;
                                        OBonusCheckList.ExGracia_02 = Math.Round(OBonusCheckList.BonusWages_02 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_02;
                                        OBonusCheckList.TotalAmount_02 = OBonusCheckList.Bonus_02 + OBonusCheckList.ExGracia_02;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_02 = 0;
                                        OBonusCheckList.ExGracia_02 = 0;
                                        OBonusCheckList.TotalAmount_02 = 0;
                                    }
                                    break;
                                case "03":
                                    OBonusCheckList.WorkingDays_03 = mWorkingDays;
                                    OBonusCheckList.BonusWages_03 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_03 = 0;
                                        OBonusCheckList.TotalAmount_03 = OBonusCheckList.Bonus_03;
                                    }
                                    else if (OBonusCheckList.BonusWages_03 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_03;
                                        OBonusCheckList.TotalAmount_03 = OBonusCheckList.Bonus_03 + OBonusCheckList.ExGracia_03;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_03 = 0;
                                        OBonusCheckList.ExGracia_03 = Math.Round(OBonusCheckList.BonusWages_03 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_03;
                                        OBonusCheckList.TotalAmount_03 = OBonusCheckList.Bonus_03 + OBonusCheckList.ExGracia_03;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_03 = 0;
                                        OBonusCheckList.ExGracia_03 = 0;
                                        OBonusCheckList.TotalAmount_03 = 0;
                                    }
                                    break;
                                case "04":
                                    OBonusCheckList.WorkingDays_04 = mWorkingDays;
                                    OBonusCheckList.BonusWages_04 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_04 = 0;
                                        OBonusCheckList.TotalAmount_04 = OBonusCheckList.Bonus_04;
                                    }
                                    else if (OBonusCheckList.BonusWages_04 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_04;
                                        OBonusCheckList.TotalAmount_04 = OBonusCheckList.Bonus_04 + OBonusCheckList.ExGracia_04;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_04 = 0;
                                        OBonusCheckList.ExGracia_04 = Math.Round(OBonusCheckList.BonusWages_04 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_04;
                                        OBonusCheckList.TotalAmount_04 = OBonusCheckList.Bonus_04 + OBonusCheckList.ExGracia_04;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_04 = 0;
                                        OBonusCheckList.ExGracia_04 = 0;
                                        OBonusCheckList.TotalAmount_04 = 0;
                                    }
                                    break;
                                case "05":
                                    OBonusCheckList.WorkingDays_05 = mWorkingDays;
                                    OBonusCheckList.BonusWages_05 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_05 = 0;
                                        OBonusCheckList.TotalAmount_05 = OBonusCheckList.Bonus_05;
                                    }
                                    else if (OBonusCheckList.BonusWages_05 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_05;
                                        OBonusCheckList.TotalAmount_05 = OBonusCheckList.Bonus_05 + OBonusCheckList.ExGracia_05;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_05 = 0;
                                        OBonusCheckList.ExGracia_05 = Math.Round(OBonusCheckList.BonusWages_05 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_05;
                                        OBonusCheckList.TotalAmount_05 = OBonusCheckList.Bonus_05 + OBonusCheckList.ExGracia_05;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_05 = 0;
                                        OBonusCheckList.ExGracia_05 = 0;
                                        OBonusCheckList.TotalAmount_05 = 0;
                                    }
                                    break;
                                case "06":
                                    OBonusCheckList.WorkingDays_06 = mWorkingDays;
                                    OBonusCheckList.BonusWages_06 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_06 = 0;
                                        OBonusCheckList.TotalAmount_06 = OBonusCheckList.Bonus_06;
                                    }
                                    else if (OBonusCheckList.BonusWages_06 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_06;
                                        OBonusCheckList.TotalAmount_06 = OBonusCheckList.Bonus_06 + OBonusCheckList.ExGracia_06;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_06 = 0;
                                        OBonusCheckList.ExGracia_06 = Math.Round(OBonusCheckList.BonusWages_06 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_06;
                                        OBonusCheckList.TotalAmount_06 = OBonusCheckList.Bonus_06 + OBonusCheckList.ExGracia_06;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_06 = 0;
                                        OBonusCheckList.ExGracia_06 = 0;
                                        OBonusCheckList.TotalAmount_06 = 0;
                                    }
                                    break;
                                case "07":
                                    OBonusCheckList.WorkingDays_07 = mWorkingDays;
                                    OBonusCheckList.BonusWages_07 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_07 = 0;
                                        OBonusCheckList.TotalAmount_07 = OBonusCheckList.Bonus_07;
                                    }
                                    else if (OBonusCheckList.BonusWages_07 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_07;
                                        OBonusCheckList.TotalAmount_07 = OBonusCheckList.Bonus_07 + OBonusCheckList.ExGracia_07;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_07 = 0;
                                        OBonusCheckList.ExGracia_07 = Math.Round(OBonusCheckList.BonusWages_07 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_07;
                                        OBonusCheckList.TotalAmount_07 = OBonusCheckList.Bonus_07 + OBonusCheckList.ExGracia_07;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_07 = 0;
                                        OBonusCheckList.ExGracia_07 = 0;
                                        OBonusCheckList.TotalAmount_07 = 0;
                                    }
                                    break;
                                case "08":
                                    OBonusCheckList.WorkingDays_08 = mWorkingDays;
                                    OBonusCheckList.BonusWages_08 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_08 = 0;
                                        OBonusCheckList.TotalAmount_08 = OBonusCheckList.Bonus_08;
                                    }
                                    else if (OBonusCheckList.BonusWages_08 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_08;
                                        OBonusCheckList.TotalAmount_08 = OBonusCheckList.Bonus_08 + OBonusCheckList.ExGracia_08;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_08 = 0;
                                        OBonusCheckList.ExGracia_08 = Math.Round(OBonusCheckList.BonusWages_08 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_08;
                                        OBonusCheckList.TotalAmount_08 = OBonusCheckList.Bonus_08 + OBonusCheckList.ExGracia_08;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_08 = 0;
                                        OBonusCheckList.ExGracia_08 = 0;
                                        OBonusCheckList.TotalAmount_08 = 0;
                                    }
                                    break;
                                case "09":
                                    OBonusCheckList.WorkingDays_09 = mWorkingDays;
                                    OBonusCheckList.BonusWages_09 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_09 = 0;
                                        OBonusCheckList.TotalAmount_09 = OBonusCheckList.Bonus_09;
                                    }
                                    else if (OBonusCheckList.BonusWages_09 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_09;
                                        OBonusCheckList.TotalAmount_09 = OBonusCheckList.Bonus_09 + OBonusCheckList.ExGracia_09;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_09 = 0;
                                        OBonusCheckList.ExGracia_09 = Math.Round(OBonusCheckList.BonusWages_09 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_09;
                                        OBonusCheckList.TotalAmount_09 = OBonusCheckList.Bonus_09 + OBonusCheckList.ExGracia_09;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_09 = 0;
                                        OBonusCheckList.ExGracia_09 = 0;
                                        OBonusCheckList.TotalAmount_09 = 0;
                                    }
                                    break;
                                case "10":
                                    OBonusCheckList.WorkingDays_10 = mWorkingDays;
                                    OBonusCheckList.BonusWages_10 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_10 = 0;
                                        OBonusCheckList.TotalAmount_10 = OBonusCheckList.Bonus_10;
                                    }
                                    else if (OBonusCheckList.BonusWages_10 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_10;
                                        OBonusCheckList.TotalAmount_10 = OBonusCheckList.Bonus_10 + OBonusCheckList.ExGracia_10;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_10 = 0;
                                        OBonusCheckList.ExGracia_10 = Math.Round(OBonusCheckList.BonusWages_10 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_10;
                                        OBonusCheckList.TotalAmount_10 = OBonusCheckList.Bonus_10 + OBonusCheckList.ExGracia_10;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_10 = 0;
                                        OBonusCheckList.ExGracia_10 = 0;
                                        OBonusCheckList.TotalAmount_10 = 0;
                                    }
                                    break;
                                case "11":
                                    OBonusCheckList.WorkingDays_11 = mWorkingDays;
                                    OBonusCheckList.BonusWages_11 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_11 = 0;
                                        OBonusCheckList.TotalAmount_11 = OBonusCheckList.Bonus_11;
                                    }
                                    else if (OBonusCheckList.BonusWages_11 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_11;
                                        OBonusCheckList.TotalAmount_11 = OBonusCheckList.Bonus_11 + OBonusCheckList.ExGracia_11;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_11 = 0;
                                        OBonusCheckList.ExGracia_11 = Math.Round(OBonusCheckList.BonusWages_11 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_11;
                                        OBonusCheckList.TotalAmount_11 = OBonusCheckList.Bonus_11 + OBonusCheckList.ExGracia_11;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_11 = 0;
                                        OBonusCheckList.ExGracia_11 = 0;
                                        OBonusCheckList.TotalAmount_11 = 0;
                                    }
                                    break;
                                case "12":
                                    OBonusCheckList.WorkingDays_12 = mWorkingDays;
                                    OBonusCheckList.BonusWages_12 = WagecalcDirectOnSalMonthData(OBonusAct.BonusWages, OSalForCal);
                                    //bonus act application
                                    if (OBonusAct.ApplicationForBonus == false)
                                    {
                                        OBonusCheckList.Bonus_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MaxPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_12 = 0;
                                        OBonusCheckList.TotalAmount_12 = OBonusCheckList.Bonus_12;
                                    }
                                    else if (OBonusCheckList.BonusWages_12 <= OBonusAct.QualiAmount)
                                    {
                                        OBonusCheckList.Bonus_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MinPercentage / 100, 2);
                                        OBonusCheckList.ExGracia_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_12;
                                        OBonusCheckList.TotalAmount_12 = OBonusCheckList.Bonus_12 + OBonusCheckList.ExGracia_12;
                                    }
                                    else if (OBonusAct.WantToGiveExgratia == true)
                                    {
                                        OBonusCheckList.Bonus_12 = 0;
                                        OBonusCheckList.ExGracia_12 = Math.Round(OBonusCheckList.BonusWages_12 * OBonusAct.MaxPercentage / 100, 2) - OBonusCheckList.Bonus_12;
                                        OBonusCheckList.TotalAmount_12 = OBonusCheckList.Bonus_12 + OBonusCheckList.ExGracia_12;
                                    }
                                    else
                                    {
                                        OBonusCheckList.Bonus_12 = 0;
                                        OBonusCheckList.ExGracia_12 = 0;
                                        OBonusCheckList.TotalAmount_12 = 0;
                                    }
                                    break;
                            }

                        }
                        //else
                        //{
                        //    OBonusCheckList.Bonus_01 = 0;
                        //    OBonusCheckList.ExGracia_01 = 0;
                        //    OBonusCheckList.TotalAmount_01 = 0;
                        //}
                    }//for loop
                    OBonusCheckList.TotalAmount = Math.Round(OBonusCheckList.TotalAmount_01 + OBonusCheckList.TotalAmount_02 + OBonusCheckList.TotalAmount_03
                                            + OBonusCheckList.TotalAmount_04 + OBonusCheckList.TotalAmount_05 + OBonusCheckList.TotalAmount_06
                                            + OBonusCheckList.TotalAmount_07 + OBonusCheckList.TotalAmount_08 + OBonusCheckList.TotalAmount_09
                                            + OBonusCheckList.TotalAmount_10 + OBonusCheckList.TotalAmount_11 + OBonusCheckList.TotalAmount_12, 0);

                    OBonusCheckList.TotalBonus = Math.Round(OBonusCheckList.Bonus_01 + OBonusCheckList.Bonus_02 + OBonusCheckList.Bonus_03
                                            + OBonusCheckList.Bonus_04 + OBonusCheckList.Bonus_05 + OBonusCheckList.Bonus_06
                                            + OBonusCheckList.Bonus_07 + OBonusCheckList.Bonus_08 + OBonusCheckList.Bonus_09
                                            + OBonusCheckList.Bonus_10 + OBonusCheckList.Bonus_11 + OBonusCheckList.Bonus_12, 0);

                    OBonusCheckList.TotalBonusWages = Math.Round(OBonusCheckList.BonusWages_01 + OBonusCheckList.BonusWages_02 + OBonusCheckList.BonusWages_03
                                            + OBonusCheckList.BonusWages_04 + OBonusCheckList.BonusWages_05 + OBonusCheckList.BonusWages_06
                                            + OBonusCheckList.BonusWages_07 + OBonusCheckList.BonusWages_08 + OBonusCheckList.BonusWages_09
                                            + OBonusCheckList.BonusWages_10 + OBonusCheckList.BonusWages_11 + OBonusCheckList.BonusWages_12, 0);

                    OBonusCheckList.TotalExGracia = Math.Round(OBonusCheckList.ExGracia_01 + OBonusCheckList.ExGracia_02 + OBonusCheckList.ExGracia_03
                                            + OBonusCheckList.ExGracia_04 + OBonusCheckList.ExGracia_05 + OBonusCheckList.ExGracia_06
                                            + OBonusCheckList.ExGracia_07 + OBonusCheckList.ExGracia_08 + OBonusCheckList.ExGracia_09
                                            + OBonusCheckList.ExGracia_10 + OBonusCheckList.ExGracia_11 + OBonusCheckList.ExGracia_12, 0);

                    OBonusCheckList.TotalWorkingDays = Math.Round(OBonusCheckList.WorkingDays_01 + OBonusCheckList.WorkingDays_02 + OBonusCheckList.WorkingDays_03
                                            + OBonusCheckList.WorkingDays_04 + OBonusCheckList.WorkingDays_05 + OBonusCheckList.WorkingDays_06
                                            + OBonusCheckList.WorkingDays_07 + OBonusCheckList.WorkingDays_08 + OBonusCheckList.WorkingDays_09
                                            + OBonusCheckList.WorkingDays_10 + OBonusCheckList.WorkingDays_11 + OBonusCheckList.WorkingDays_12, 0);
                    //minimum working days
                    if (OBonusCheckList.TotalWorkingDays < OBonusAct.MinimumWorkingDays)
                    {
                        OBonusCheckList.TotalAmount = 0;
                        OBonusCheckList.TotalBonus = 0;
                        OBonusCheckList.TotalExGracia = 0;

                    }
                    //minimum bonus amount
                    if (OBonusCheckList.TotalBonus < OBonusAct.MinimumBonusAmount)
                    {
                        OBonusCheckList.TotalBonus = OBonusAct.MinimumBonusAmount;

                    }

                    double TDSAmount1 = 0;
                    double TDSAmount = 0;
                    double EduCess = 0;
                    double FinalTDSAmount = 0;
                    EmployeePayroll OEmployeePayrollTDS = null;
                    if (TaxCalcIsallowed == true)
                    {

                        var FinYear = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault(); // financial year id
                        DateTime FromPeriod = Convert.ToDateTime(FinYear.FromDate);
                        DateTime ToPeriod = Convert.ToDateTime(FinYear.ToDate);

                        var OEmployee = _returnEmployeePayroll(OEmployeePayroll.Employee.Id);
                        OEmployeePayrollTDS = _returnITInvestmentPayment(OEmployee.Id);
                        OEmployeePayrollTDS.RegimiScheme = OEmployeePayrollTDS.RegimiScheme.Where(e => e.FinancialYear_Id == FinYear.Id).ToList();

                        DateTime processdate = DateTime.Today;
                        // double status = 1;
                        if (OEmployee.ServiceBookDates.JoiningDate >= FinYear.FromDate)
                        {
                            FromPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.JoiningDate);
                            //IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear, FromPeriod, ToPeriod, processdate, db);
                            IncomeTaxCalc.ITCalculation(OEmployeePayrollTDS, OCompanyPayroll.Id, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                        }
                        else if (OEmployee.ServiceBookDates.ServiceLastDate >= FinYear.FromDate &&
                           OEmployee.ServiceBookDates.ServiceLastDate <= FinYear.ToDate)
                        {
                            ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.ServiceLastDate);
                            //IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, FinYear, FromPeriod, ToPeriod, processdate, db);
                            IncomeTaxCalc.ITCalculation(OEmployeePayrollTDS, OCompanyPayroll.Id, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                        }
                        else if (OEmployee.ServiceBookDates.RetirementDate >= FinYear.FromDate &&
                           OEmployee.ServiceBookDates.RetirementDate <= FinYear.ToDate)
                        {
                            ToPeriod = Convert.ToDateTime(OEmployee.ServiceBookDates.RetirementDate);
                            IncomeTaxCalc.ITCalculation(OEmployeePayrollTDS, OCompanyPayroll.Id, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                        }
                        else
                        {
                            IncomeTaxCalc.ITCalculation(OEmployeePayrollTDS, OCompanyPayroll.Id, FinYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, ProcType);
                        }

                        double EmpIncome = db.ITProjection.Where(e => e.EmployeePayroll_Id == OEmployeePayrollTDS.Id && e.FinancialYear.Id == FinYear.Id && e.PickupId == 121).FirstOrDefault().ProjectedAmount;
                        CompanyPayroll OIncomeTax = db.CompanyPayroll
                             .Include(e => e.IncomeTax)
                             .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                             .Include(e => e.IncomeTax.Select(r => r.ITTDS))
                             .Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(d => d.Category))).Where(e => e.Company.Id == OCompanyPayroll.Id)
                             .SingleOrDefault();

                        IncomeTax OITMaster = OIncomeTax.IncomeTax.Where(e => e.FyCalendar.Id == FinYear.Id).SingleOrDefault();
                        Double[] ITPerc = new Double[5];

                        // var OEmployeePayrollTDS = _returnITInvestmentPayment(mEmployeePayroll_Id);
                        ITPerc = TDSCalc(OEmployeePayrollTDS, OITMaster, EmpIncome, ToPeriod);
                        var ITaxPerc = ITPerc[0];
                        var EduCessPerc = ITPerc[1];

                        double finaltax = db.ITProjection
                            .Where(e => e.EmployeePayroll_Id == OEmployeePayrollTDS.Id && e.FinancialYear.Id == FinYear.Id && e.PickupId == 133)
                            .FirstOrDefault().ProjectedAmount;

                        if (finaltax == 0)
                        {
                            ITaxPerc = 0;
                        }
                        double AmtPaid = OBonusCheckList.TotalBonus + OBonusCheckList.TotalExGracia;
                        TDSAmount1 = Math.Round((AmtPaid * ITaxPerc) / 100);

                        EduCess = Math.Round((TDSAmount1 * EduCessPerc) / 100);

                        double BalTax = db.ITProjection
                            .Where(e => e.EmployeePayroll_Id == OEmployeePayrollTDS.Id && e.FinancialYear.Id == FinYear.Id && e.PickupId == 141)
                            .FirstOrDefault().ProjectedAmount;

                        TDSAmount = EduCess + TDSAmount1;

                        FinalTDSAmount = TDSAmount;
                        //if (BalTax > 0)
                        //{
                        //    if (TDSAmount > BalTax)
                        //    {

                        //        FinalTDSAmount = BalTax;
                        //    }
                        //    else
                        //    {
                        //        FinalTDSAmount = TDSAmount;
                        //    }
                        //}
                        //else
                        //{
                        //    FinalTDSAmount = 0;
                        //}


                    }

                    //save  bonus data

                    OBonusCheckList.TDSAmount = FinalTDSAmount;
                    OBonusCheckList.DBTrack = dbt;
                    OBonusCheckList.ProcessDate = DateTime.Now;
                    db.BonusChkT.Add(OBonusCheckList);
                    db.SaveChanges();
                    if (TaxCalcIsallowed == true)
                    {
                        var OEmpPayrollSave1 = db.EmployeePayroll.Find(OEmployeePayrollTDS.Id);
                        OEmpPayrollSave1.BonusChkT.Add(OBonusCheckList);
                    }
                    else
                    {
                        var OEmpPayrollSave1 = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                        OEmpPayrollSave1.BonusChkT.Add(OBonusCheckList);
                    }

                    db.SaveChanges();
                }
            }
        }


        public static double WagecalcDirectOnSalMonthData(Wages OWagesMaster, List<IncomeTaxCalc.ITSalaryHeadDataTemp> OSalaryEarnDedT)
        {

            double OWages = 0;
            if (OSalaryEarnDedT != null && OSalaryEarnDedT.Count() > 0)
            {

                OWages = OWagesMaster.RateMaster
                    .Join(OSalaryEarnDedT, u => u.SalHead.Id, uir => uir.SalaryHead,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Percentage / 100 * e.uir.ProjectedAmount).Sum();

                OWages = OWages + OWagesMaster.RateMaster
                    .Join(OSalaryEarnDedT, u => u.SalHead.Id, uir => uir.SalaryHead,
                            (u, uir) => new { u, uir })
                    .Select(e => e.u.Amount).Sum();


                if (OWagesMaster.CeilingMin != null)
                {
                    if (OWages < OWagesMaster.CeilingMin)
                    {
                        OWages = OWagesMaster.CeilingMin;
                    }
                }
                if (OWagesMaster.CeilingMax != null)
                {
                    if (OWages > OWagesMaster.CeilingMax)
                    {
                        OWages = OWagesMaster.CeilingMax;
                    }
                }
                return OWages;
            }

            return 0;
        }
        public static double LeaveEncashCalcSepration(int mEmployeePayroll_Id, LvEncashPayment OLvEncashData, int salaryheadid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OEmployeePayroll = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.LvEncashPayment)
                        .Include(e => e.SalaryT)
                        .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                        .Include(e => e.EmpSalStruct)
                        .Include(e => e.SalAttendance)
                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))

                        .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();


                //var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                //                   .Where(e => e.EndDate == null).SingleOrDefault();
                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                                  .OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                // var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").SingleOrDefault();
                var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.Id == salaryheadid).SingleOrDefault();
                double mLvEncashAmount = 0;
                if (OEmpSalDetails == null)//lvencash head present
                {
                    return 0;
                }

                LvEncashPolicy encashpolicy = null;
                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == OEmployeePayroll.Employee.Id)
                                                   .SingleOrDefault();

                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                      .Include(e => e.EmployeeLvStructDetails)
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.PayMonthConcept))
                      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).SingleOrDefault();

                encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == OLvEncashData.LvEncashReq.LvHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();


                //check salary data
                var OSalChk = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).SingleOrDefault();
                var CompanyId = Convert.ToInt32(SessionManager.CompanyId);
                var Loc = db.Company.Include(e => e.Address).Include(e => e.Address.State)
                    .Where(e => e.Id == CompanyId).SingleOrDefault().Address.State.Code.ToUpper();

                var companyCode = db.Company.Where(e => e.Id == CompanyId).SingleOrDefault();
                if (OSalChk == null)
                {
                    SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                    if (LVEncashFormula != null)
                    {
                        double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, null, OEmpSalStruct.EmpSalStructDetails.ToList());
                        //check for month days
                        var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
                        if (OPayProcGrp != null)
                        {

                        }
                        //  25863/1*15
                        //var oDay = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth).Day;
                        //mLvEncashAmount = (mLvEncashWages / oDay * OLvEncashData.LvEncashReq.EncashDays);
                        DateTime processdt = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth);
                        int year = processdt.Year;
                        int month = processdt.Month;
                        var oDay = DateTime.DaysInMonth(year, month);
                        if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                        {
                            mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
                        }
                        else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                        {
                            mLvEncashAmount = (mLvEncashWages) - ((oDay - OLvEncashData.LvEncashReq.EncashDays) / 30) * mLvEncashWages;
                        }
                        else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                        {
                            if (companyCode.Code == "KDCC")
                            {
                                mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
                            }
                            else
                            {
                                mLvEncashAmount = (mLvEncashWages / oDay) * OLvEncashData.LvEncashReq.EncashDays;
                            }
                        }

                        mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
                    }

                }
                else
                {
                    SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                    if (LVEncashFormula != null)
                    {
                        double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, OSalChk.SalEarnDedT.ToList(), null);
                        var OAttChk = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).SingleOrDefault();
                        var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
                        if (OPayProcGrp != null)
                        {
                            //For Chennai hardcode
                            if (Loc == "TAMILNADU")
                            {
                                // OPayProcGrp.PayMonthConcept.LookupVal = "FIXED30DAYS";
                                encashpolicy.PayMonthConcept.LookupVal = "FIXED30DAYS";
                            }
                            if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                            {
                                mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
                            }
                            else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                            {
                                mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                            }
                            else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                            {
                                //ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                //mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                                if (companyCode.Code == "KDCC")
                                {

                                    mLvEncashAmount = (((mLvEncashWages * OAttChk.MonthDays) / OAttChk.PaybleDays) / 30) * OLvEncashData.LvEncashReq.EncashDays;
                                }
                                else
                                {
                                    mLvEncashAmount = (mLvEncashWages / OAttChk.PaybleDays) * OLvEncashData.LvEncashReq.EncashDays;
                                }
                            }
                        }
                        mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
                    }
                }
                return mLvEncashAmount;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="EmpId"> Employee Id</param>
        /// <param name="LeaveHeadId"> Encah Leave Id</param>
        /// <param name="mLvEncashAmount"> </param>
        /// <param name="SalaryHeadId"></param>
        /// <param name="FinYrId"></param>
        /// <param name="Flag"></param>
        /// <param name="LeaveEncashExemptionSettings"></param>
        /// <returns></returns>

        public static LvEncashExemptReturnClass LeaveEncashExempt(int EmpId, int LeaveHeadId, double mLvEncashAmount, int SalaryHeadId, int FinYrId, double LvUtilised, double LvEntitled, LeaveEncashExemptionSettings LeaveEncashExemptionSettings)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                 
                var OEmployeePayroll = db.EmployeePayroll
                       .Include(e => e.Employee) 
                       .Include(e => e.Employee.ServiceBookDates) 
                       .Where(e => e.Employee_Id == EmpId).FirstOrDefault();

                int EmpLvId = db.EmployeeLeave.Where(e => e.Employee_Id == EmpId).FirstOrDefault().Id;

                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                         .Include(e => e.EmployeeLvStructDetails)
                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
                         .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.PayMonthConceptAsGovtAct)).AsNoTracking()
                         .Where(e => e.EndDate == null && e.EmployeeLeave_Id == EmpLvId).FirstOrDefault();

                LvEncashPolicy OLvEncashPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == LeaveHeadId && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();

                double PaidEncashAmt = mLvEncashAmount;


                double mService = 0;
              
                DateTime start = OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate != null ? OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value.Date : OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value.Date;
                DateTime end = Convert.ToDateTime("01/" + OEmployeePayroll.Employee.ServiceBookDates.ServiceLastDate.Value.Date.ToString("MM/yyyy"));// DateTime.Now.Date;
                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
            
                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                mService = months / 12;

                 DBTrack dbt = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                 PayStruct oPaystruct = db.Employee.Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                     .Include(e => e.PayStruct.JobStatus.EmpActingStatus).Where(e => e.Id == EmpId).FirstOrDefault().PayStruct;
                 string EmpStatus = oPaystruct != null && oPaystruct.JobStatus != null && oPaystruct.JobStatus.EmpActingStatus != null ? oPaystruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : "";

                List<LvEncashExemptDetails> oLvEncashExemptDetails = new List<LvEncashExemptDetails>();
                if (LeaveEncashExemptionSettings.DOrgType.ToUpper() == "PRIVATE")
                {
                    if (LeaveEncashExemptionSettings.DExemptionLimit.ToUpper() == "LEAST")
                    {
                        if (LeaveEncashExemptionSettings.DSeperationType.ToUpper().Contains(EmpStatus))
                        {
                            /// Note 1 : Amount notified by the Government
                            double AmtByGovt = Convert.ToDouble(LeaveEncashExemptionSettings.DCertificateA);

                            LvEncashExemptDetails LvEncashExemptDetails = new LvEncashExemptDetails()
                            { 
                                ActualAmount = AmtByGovt, 
                                PickupId = 1,
                                DBTrack = dbt,
                                Title = "Amount notified by the Government"
                            };
                            //db.LvEncashExemptDetails.Add(LvEncashExemptDetails);
                            //db.SaveChanges();
                            oLvEncashExemptDetails.Add(LvEncashExemptDetails);

                            /// Note 2 : Leave Salary atually received
                            double ActualLvEncashAmt = PaidEncashAmt;

                            LvEncashExemptDetails = new LvEncashExemptDetails()
                          {
                              ActualAmount = ActualLvEncashAmt, 
                              PickupId = 2,
                              DBTrack = dbt,
                              Title = "Actual leave encashment"
                          };
                            //db.LvEncashExemptDetails.Add(LvEncashExemptDetails);
                            //db.SaveChanges();
                            oLvEncashExemptDetails.Add(LvEncashExemptDetails);

                            /// Note 3 : 10 months salary based on average salary of last 10 months preceding retirement
                       
                            double AvgSal = 0;
                            List<string> mPeriod = new List<string>();

                            
                                DateTime StartDate = end.AddMonths(-10);
                                for (DateTime mTempDate = StartDate; mTempDate < end; mTempDate = mTempDate.AddMonths(1))
                                { 
                                    mPeriod.Add(Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));  
                                }
                                List<SalaryT> OSalaryT = new List<SalaryT>();
                                OSalaryT = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(t => t.SalaryHead)).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && mPeriod.Contains(e.PayMonth)).ToList();
                                var OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(t => t.SalHeadFormula)).Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id).OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                                 
                                var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead_Id == SalaryHeadId).FirstOrDefault();
                                if (OEmpSalDetails.SalHeadFormula != null)
                                {
                                    foreach (var oSal in OSalaryT)
                                    {
                                        double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(OEmpSalDetails.SalHeadFormula, oSal.SalEarnDedT.ToList(), null);
                                        AvgSal = AvgSal + mLvEncashWages;
                                    }
                                    LvEncashExemptDetails = new LvEncashExemptDetails()
                                    {
                                        ActualAmount = AvgSal,
                                        PickupId = 3,
                                        DBTrack = dbt,
                                        Title = "Average salary for 10 months"
                                    };
                                    oLvEncashExemptDetails.Add(LvEncashExemptDetails);
                                } 
                               
                              

                                //4. Cash equivalent of unavailed leave(based on last 10 months average salary) to his credit at the time of retirement
                                double OneMonthSal = 0, OneDaySalary = 0, mLvEncashAmountOneDay = 0;
                                if (AvgSal != 0)
                                {
                                    OneMonthSal = AvgSal / 10;
                                    int days = 0;
                                    if (OLvEncashPolicy.PayMonthConceptAsGovtAct.LookupVal.ToString().ToUpper() == "FIXED30DAYS") 
                                        days = 30; 
                                    else if (OLvEncashPolicy.PayMonthConceptAsGovtAct.LookupVal.ToString().ToUpper() == "30DAYS")
                                        days = 30; 
                                    else if (OLvEncashPolicy.PayMonthConceptAsGovtAct.LookupVal.ToString().ToUpper() == "CALENDAR")
                                        days = 30; 
                                    OneDaySalary = (OneMonthSal / days);

                                    double TotalEncashDays = LvEntitled - LvUtilised;

                                    mLvEncashAmountOneDay = SalaryHeadGenProcess.GovtRound(OneDaySalary * TotalEncashDays, 2);

                                    LvEncashExemptDetails = new LvEncashExemptDetails()
                                    {
                                        ActualAmount = mLvEncashAmountOneDay,
                                        PickupId = 4,
                                        DBTrack = dbt,
                                        Title = "Encashment Amount as per Total year of service - total service utilised leave."
                                    };

                                    //db.LvEncashExemptDetails.Add(LvEncashExemptDetails);
                                    //db.SaveChanges();
                                    oLvEncashExemptDetails.Add(LvEncashExemptDetails);
                                }
                                mLvEncashAmount = GetMinValue(AmtByGovt, ActualLvEncashAmt, AvgSal, mLvEncashAmountOneDay); // get minimum value from above 4

                                LvEncashExemptDetails = new LvEncashExemptDetails()
                                {
                                    ActualAmount = ActualLvEncashAmt < mLvEncashAmountOneDay ? 0 :  ActualLvEncashAmt - mLvEncashAmountOneDay,
                                    PickupId = 5,
                                    DBTrack = dbt,
                                    Title = "Leave encashment taxable as income from salary"
                                };

                                //db.LvEncashExemptDetails.Add(LvEncashExemptDetails);
                                //db.SaveChanges();
                                oLvEncashExemptDetails.Add(LvEncashExemptDetails);
                             
                        }
                    }
                }
                LvEncashExemptReturnClass oExemptClass = new LvEncashExemptReturnClass();
                oExemptClass.LvEncashExemptDetails = oLvEncashExemptDetails;
                oExemptClass.mLvEncashAmount = mLvEncashAmount;
                return oExemptClass; 
            }
           
        }

        public class LvEncashExemptReturnClass
        {
            public List<LvEncashExemptDetails> LvEncashExemptDetails { get; set; }
            public double mLvEncashAmount { get; set; }
        }

        public static double GetMinValue(double AmtByGovt, double ActualLvEncashAmt, double AvgSal, double mLvEncashAmount)
        {
            List<double> Numbers = new List<double>();
            Numbers.Add(AmtByGovt);
            Numbers.Add(ActualLvEncashAmt);
            Numbers.Add(AvgSal);
            Numbers.Add(mLvEncashAmount);
            var q = (from Num in Numbers
                     select Num).Min();
            return q;
        }

        //public static double LeaveEncashCalc(int mEmployeePayroll_Id, LvEncashPayment OLvEncashData)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var OEmployeePayroll = db.EmployeePayroll
        //                .Include(e => e.Employee)
        //                .Include(e => e.LvEncashPayment)
        //                .Include(e => e.SalaryT)
        //                .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
        //                .Include(e => e.EmpSalStruct)
        //                .Include(e => e.SalAttendance)
        //                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
        //                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))

        //                .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();


        //        //var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
        //        //                   .Where(e => e.EndDate == null).SingleOrDefault();
        //        var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
        //                          .OrderByDescending(e => e.Id).FirstOrDefault();
        //        var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").SingleOrDefault();
        //        double mLvEncashAmount = 0;
        //        if (OEmpSalDetails == null)//lvencash head present
        //        {
        //            return 0;
        //        }

        //        LvEncashPolicy encashpolicy = null;
        //        EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == OEmployeePayroll.Employee.Id)
        //                                           .SingleOrDefault();

        //        EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
        //              .Include(e => e.EmployeeLvStructDetails)
        //              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
        //              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
        //              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
        //              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.PayMonthConcept))
        //              .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).SingleOrDefault();

        //        encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == OLvEncashData.LvEncashReq.LvHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();


        //        //check salary data
        //        var OSalChk = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).SingleOrDefault();
        //        var CompanyId = Convert.ToInt32(SessionManager.CompanyId);
        //        var Loc = db.Company.Include(e => e.Address).Include(e => e.Address.State)
        //            .Where(e => e.Id == CompanyId).SingleOrDefault().Address.State.Code.ToUpper();

        //        var companyCode = db.Company.Where(e => e.Id == CompanyId).SingleOrDefault();
        //        if (OSalChk == null)
        //        {
        //            SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
        //            if (LVEncashFormula != null)
        //            {
        //                double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, null, OEmpSalStruct.EmpSalStructDetails.ToList());
        //                //check for month days
        //                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
        //                if (OPayProcGrp != null)
        //                {

        //                }
        //                //  25863/1*15
        //                //var oDay = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth).Day;
        //                //mLvEncashAmount = (mLvEncashWages / oDay * OLvEncashData.LvEncashReq.EncashDays);
        //                DateTime processdt = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth);
        //                int year = processdt.Year;
        //                int month = processdt.Month;
        //                var oDay = DateTime.DaysInMonth(year, month);
        //                if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
        //                {
        //                    mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
        //                }
        //                else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
        //                {
        //                    mLvEncashAmount = (mLvEncashWages) - ((oDay - OLvEncashData.LvEncashReq.EncashDays) / 30) * mLvEncashWages;
        //                }
        //                else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
        //                {
        //                    if (companyCode.Code == "KDCC")
        //                    {
        //                        mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
        //                    }
        //                    else
        //                    {
        //                        mLvEncashAmount = (mLvEncashWages / oDay) * OLvEncashData.LvEncashReq.EncashDays;
        //                    }
        //                }

        //                mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
        //            }

        //        }
        //        else
        //        {
        //            SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
        //            if (LVEncashFormula != null)
        //            {
        //                double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, OSalChk.SalEarnDedT.ToList(), null);
        //                var OAttChk = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).SingleOrDefault();
        //                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
        //                if (OPayProcGrp != null)
        //                {
        //                    //For Chennai hardcode
        //                    if (Loc == "TAMILNADU")
        //                    {
        //                        // OPayProcGrp.PayMonthConcept.LookupVal = "FIXED30DAYS";
        //                        encashpolicy.PayMonthConcept.LookupVal = "FIXED30DAYS";
        //                    }
        //                    if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
        //                    {
        //                        mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
        //                    }
        //                    else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
        //                    {
        //                        mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
        //                    }
        //                    else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
        //                    {
        //                        //ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
        //                        //mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
        //                        if (companyCode.Code == "KDCC")
        //                        {

        //                            mLvEncashAmount = (((mLvEncashWages * OAttChk.MonthDays) / OAttChk.PaybleDays) / 30) * OLvEncashData.LvEncashReq.EncashDays;
        //                        }
        //                        else
        //                        {
        //                            mLvEncashAmount = (mLvEncashWages / OAttChk.PaybleDays) * OLvEncashData.LvEncashReq.EncashDays;
        //                        }
        //                    }
        //                }
        //                mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
        //            }
        //        }
        //        return mLvEncashAmount;
        //    }
        //}

        public static double LeaveEncashCalc(int mEmployeePayroll_Id, LvEncashPayment OLvEncashData)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var OEmployeePayroll = db.EmployeePayroll
                //        .Include(e => e.Employee)
                //        .Include(e => e.LvEncashPayment)
                //        .Include(e => e.SalaryT)
                //        .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                //        .Include(e => e.EmpSalStruct)
                //        .Include(e => e.SalAttendance)
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))

                //        .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();



                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Find(mEmployeePayroll_Id);
                OEmployeePayroll.SalaryT = db.EmployeePayroll.Where(e => e.Id == mEmployeePayroll_Id).Select(t => t.SalaryT.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).ToList()).AsNoTracking().FirstOrDefault();
                OEmployeePayroll.SalAttendance = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == mEmployeePayroll_Id && e.PayMonth == OLvEncashData.ProcessMonth).AsNoTracking().ToList();

                EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == mEmployeePayroll_Id).OrderByDescending(e => e.Id).AsNoTracking().FirstOrDefault();
                EmpSalStruct.EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == EmpSalStruct.Id).AsNoTracking().ToList();
                foreach (var item in EmpSalStruct.EmpSalStructDetails)
                {
                    item.SalaryHead = db.SalaryHead.Find(item.SalaryHead_Id);
                    item.SalaryHead.SalHeadOperationType = db.LookupValue.Find(item.SalaryHead.SalHeadOperationType_Id);
                    item.SalHeadFormula = db.SalHeadFormula.Find(item.SalHeadFormula_Id);
                }
                // OEmployeePayroll.EmpSalStruct = EmpSalStruct;

                //var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                //                   .Where(e => e.EndDate == null).SingleOrDefault();
                var OEmpSalStruct = EmpSalStruct;
                var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").FirstOrDefault();
                double mLvEncashAmount = 0;
                if (OEmpSalDetails == null)//lvencash head present
                {
                    return 0;
                }

                LvEncashPolicy encashpolicy = null;
                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == OEmployeePayroll.Employee_Id).AsNoTracking()
                                                   .FirstOrDefault();

                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                      .Include(e => e.EmployeeLvStructDetails)
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
                      .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.PayMonthConcept))
                      .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).AsNoTracking().FirstOrDefault();

                encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == OLvEncashData.LvEncashReq.LvHead.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();


                //check salary data
                var OSalChk = OEmployeePayroll.SalaryT.FirstOrDefault();

                var CompanyId = Convert.ToInt32(SessionManager.CompanyId);
                var Loc = db.Company.Include(e => e.Address).Include(e => e.Address.State)
                    .Where(e => e.Id == CompanyId).AsNoTracking().FirstOrDefault().Address.State.Code.ToUpper();

                var companyCode = db.Company.Where(e => e.Id == CompanyId).FirstOrDefault();
                if (OSalChk == null)
                {
                    SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                    if (LVEncashFormula != null)
                    {
                        double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, null, OEmpSalStruct.EmpSalStructDetails.ToList());
                        //check for month days
                        //var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).FirstOrDefault();
                        //if (OPayProcGrp != null)
                        //{

                        //}
                        //  25863/1*15
                        //var oDay = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth).Day;
                        //mLvEncashAmount = (mLvEncashWages / oDay * OLvEncashData.LvEncashReq.EncashDays);
                        DateTime processdt = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth);
                        int year = processdt.Year;
                        int month = processdt.Month;
                        var oDay = DateTime.DaysInMonth(year, month);
                        if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                        {
                            mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
                        }
                        else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                        {
                            mLvEncashAmount = (mLvEncashWages) - ((oDay - OLvEncashData.LvEncashReq.EncashDays) / 30) * mLvEncashWages;
                        }
                        else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                        {
                            if (companyCode.Code == "KDCC")
                            {
                                mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
                            }
                            else
                            {
                                mLvEncashAmount = (mLvEncashWages / oDay) * OLvEncashData.LvEncashReq.EncashDays;
                            }
                        }

                        mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
                    }

                }
                else
                {
                    List<SalEarnDedT> OSalEarnDedT = db.SalEarnDedT.Include(e => e.SalaryHead).Where(e => e.SalaryT_Id == OSalChk.Id).AsNoTracking().ToList();
                    OSalChk.SalEarnDedT = OSalEarnDedT;
                    SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                    if (LVEncashFormula != null)
                    {
                        double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, OSalChk.SalEarnDedT.ToList(), null);
                        var OAttChk = OEmployeePayroll.SalAttendance.FirstOrDefault();
                        var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).AsNoTracking().FirstOrDefault();
                        if (OPayProcGrp != null)
                        {
                            //For Chennai hardcode
                            if (Loc == "TAMILNADU")
                            {
                                // OPayProcGrp.PayMonthConcept.LookupVal = "FIXED30DAYS";
                                encashpolicy.PayMonthConcept.LookupVal = "FIXED30DAYS";
                            }
                            if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                            {
                                mLvEncashAmount = (mLvEncashWages / 30) * OLvEncashData.LvEncashReq.EncashDays;
                            }
                            else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                            {
                                mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                            }
                            else if (encashpolicy.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")//(OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                            {
                                //ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                //mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                                if (OAttChk.PaybleDays == 0)
                                {
                                    mLvEncashAmount = 0;
                                }
                                else
                                {
                                    if (companyCode.Code == "KDCC")
                                    {

                                        mLvEncashAmount = (((mLvEncashWages * OAttChk.MonthDays) / OAttChk.PaybleDays) / 30) * OLvEncashData.LvEncashReq.EncashDays;
                                    }
                                    else
                                    {
                                        mLvEncashAmount = (mLvEncashWages / OAttChk.PaybleDays) * OLvEncashData.LvEncashReq.EncashDays;
                                    }
                                }
                            }
                        }
                        mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
                    }
                }
                return mLvEncashAmount;
            }
        }
        public static void GeneratePFECR(int mCompId, string mPayMonth, string mReturnMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                var OCompanyPayroll = db.CompanyPayroll
                    .Include(e => e.PFMaster)
                    .Include(e => e.PFMaster.Select(r => r.EPSWages))
                    .Include(e => e.PFMaster.Select(r => r.PFAdminWages))
                    .Include(e => e.PFMaster.Select(r => r.PFEDLIWages))
                    .Include(e => e.PFMaster.Select(r => r.PFInspWages))
                    .Include(e => e.PFMaster.Select(r => r.PFTrustType))
                    .Include(e => e.PFMaster.Select(r => r.EPFWages))
                    .Include(e => e.Company)
                    //.Include(e=>e.Company.Employee)
                    .Include(e => e.PFECRSummaryR)
                    .Include(e => e.PFECRSummaryR.Select(r => r.PFECRR))
                    .Where(e => e.Company.Id == mCompId)
                    .SingleOrDefault();
                var OPFMasterList = OCompanyPayroll.PFMaster.Where(e => e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + mPayMonth).Date).ToList();
                foreach (var OPFMaster in OPFMasterList)
                {


                    //if (OPFMaster == null)
                    //{
                    //    OPFMaster = OCompanyPayroll.PFMaster.LastOrDefault();
                    //}
                    //var OSalaryTQuery = db.EmployeePayroll
                    //                       .Include(e => e.SalaryT)
                    //                       .Include(e => e.SalaryT.Select(r => r.PFECRR))
                    //                       .Include(e => e.SalaryT.Select(r => r.PFECRR.PFCalendar)).ToList();
                    //var OPFECRDataListLastMonth = db.SalaryT.Where(r => r.PayMonth == Convert.ToDateTime("01/" + mPayMonth).AddMonths(-1).ToString("MM/yyyy") && r.PFECRR != null)
                    //    .Select(d => new
                    //    {
                    //        PFECRR = d.PFECRR,
                    //        PFCalendar=d.PFECRR.PFCalendar
                    //    }
                    //    )
                    //    .ToList();

                    var OPFECRDataCurrentMonth = db.SalaryT.Include(s=>s.PFECRR).Where(r => r.PayMonth == mPayMonth && r.PFECRR != null && r.PFECRR.Establishment_ID == OPFMaster.EstablishmentID)
                       .Select(d => new
                       {
                           PFECRR = d.PFECRR,
                           PFCalendar = d.PFECRR.PFCalendar,
                           employeepayrollid=d.EmployeePayroll_Id
                       }
                       )
                       .ToList();

                    //add company filter
                    //.Where(e => e.Employee.com ) //OCompanyPayroll.Company.Employee.Contains(e.Employee.Id)).ToList();

                    //Last Month
                    //var OSalaryTDataLastMonth = OSalaryTQuery.Select(e => e.SalaryT
                    //    .Where(r => r.PayMonth == Convert.ToDateTime("01/" + mPayMonth).AddMonths(-1).ToString("MM/yyyy") &&
                    //    r.PFECRR != null)).ToList();
                    //var OPFECRDataListLastMonth = OSalaryTDataLastMonth.Select(e => e.Select(r => r.PFECRR)).ToList();
                    //current Month
                    //var OSalaryTData = OSalaryTQuery.Select(e => e.SalaryT
                    //    .Where(r => r.PayMonth == mPayMonth && r.PFECRR != null && r.PFECRR.Establishment_ID == OPFMaster.EstablishmentID)).ToList();
                    //var OPFECRDataCurrentMonth = OSalaryTData.Select(e => e.Select(r => r.PFECRR)).ToList();

                    if (OPFECRDataCurrentMonth == null)
                    {
                        var aa = 3;
                    }



                    var OPFECRData = new List<PFECRR>();
                    List<int?> oemppayrollidlist = new List<int?>();
                    foreach (var ca in OPFECRDataCurrentMonth)
                    {
                        if (ca.PFECRR != null)
                        {
                            OPFECRData.Add(ca.PFECRR);
                            oemppayrollidlist.Add(ca.employeepayrollid);

                        }
                    }

                    if (OPFECRData != null && OPFECRData.Count > 0)
                    {
                        var OECRSummayDel = OCompanyPayroll.PFECRSummaryR.Where(e => e.Wage_Month == mPayMonth && e.Establishment_ID == OPFMaster.EstablishmentID).SingleOrDefault();
                        if (OECRSummayDel != null)
                        {

                            OECRSummayDel.PFECRR = null;
                            db.Entry(OECRSummayDel).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            OCompanyPayroll.PFECRSummaryR.Remove(OECRSummayDel);
                            db.Entry(OCompanyPayroll).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.PFECRSummaryR.Attach(OECRSummayDel);
                            db.Entry(OECRSummayDel).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();

                            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
                            //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                            string localPath = new Uri(requiredPath).LocalPath;
                            if (!System.IO.Directory.Exists(localPath))
                            {
                                localPath = new Uri(requiredPath).LocalPath;
                                System.IO.Directory.CreateDirectory(localPath);
                            }
                            string path = requiredPath + "\\ECR_PF_" + OPFMaster.EstablishmentID+"_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
                            //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                            path = new Uri(path).LocalPath;
                            if (System.IO.File.Exists(path))
                            {
                                File.Delete(path);
                            }

                            path = requiredPath + "\\PFECRArrear_" + OPFMaster.EstablishmentID + "_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
                            //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                            path = new Uri(path).LocalPath;
                            if (System.IO.File.Exists(path))
                            {
                                File.Delete(path);
                            }

                            path = requiredPath + "\\PFECRForm10_" + OPFMaster.EstablishmentID + "_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
                            //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                            path = new Uri(path).LocalPath;
                            if (System.IO.File.Exists(path))
                            {
                                File.Delete(path);
                            }


                        }

                        //ecr text file creation
                        ECRFile OECRFile = new ECRFile();
                        string mSalFileName = OECRFile.CreateECRFile(OPFMaster, OPFECRData, mPayMonth);
                        var dataaa = PayrollReportGen.GenerateArrPFECR(OPFMaster, mCompId, mPayMonth, oemppayrollidlist);
                        string mArrFileName = dataaa.mArrFileName;
                        //string mArrFileName = OECRFile.CreateECRArrearFile(OPFECRData, mPayMonth);
                        string mRetFileName = OECRFile.CreateForm10ECRFile(OPFMaster,OPFECRData, mPayMonth);
                        string mJoinFileName = "";//OECRFile.CreateECRFile(OPFECRData, mPayMonth);

                        //.ToList();
                        //calculate pfecr summary



                        var OPFSummary = OPFECRData.GroupBy(t => t.Establishment_ID)
                            .Select(e => new
                            {

                                TotEPFWages = e.Sum(r => r.EPF_Wages),
                                TotEPSWages = e.Sum(r => r.EPS_Wages),
                                TotEDLIWages = e.Sum(r => r.EDLI_Wages),
                                TotGrossWages = e.Sum(r => r.Gross_Wages),
                                TotEPS = e.Sum(r => r.EPS_Share),
                                TotEPF = e.Sum(r => r.EE_Share),
                                TotER = e.Sum(r => r.ER_Share),
                                TotVPF = e.Sum(r => r.EE_VPF_Share),
                                TotArrEPFWages = e.Sum(r => r.Arrear_EPF_Wages),
                                TotArrEPSWages = e.Sum(r => r.Arrear_EPS_Wages),
                                TotArrEDLIWages = e.Sum(r => r.Arrear_EDLI_Wages),
                                TotArrEPF = e.Sum(r => r.Arrear_EE_Share),
                                TotArrEPS = e.Sum(r => r.Arrear_EPS_Share),
                                TotArrER = e.Sum(r => r.Arrear_ER_Share),
                                TotOffEPFWages = e.Sum(r => r.Officiating_EPF_Wages),
                                TotOffEPSWages = e.Sum(r => r.Officiating_EPS_Wages),
                                TotOffEDLIWages = e.Sum(r => r.Officiating_EDLI_Wages),
                                TotOffEPF = e.Sum(r => r.Officiating_EE_Share),
                                TotOffEPS = e.Sum(r => r.Officiating_EPS_Share),
                                TotOffER = e.Sum(r => r.Officiating_ER_Share),
                                TotOffVPF = e.Sum(r => r.Officiating_VPF_Share),
                                TotNCP = e.Sum(r => r.NCP_Days),
                                TotEmpCount = e.Count(),

                                //EstablishmentName=e.Select(r=>r.PFECRR.Establishment_Name).SingleOrDefault(),
                                //PFCalendar = db.Calendar.Find( e.Select(r => r.PFECRR.PFCalendar).SingleOrDefault().Id),

                            }
                            ).SingleOrDefault();

                        if (OPFMaster.PFTrustType.LookupVal.ToUpper() != "EXEMPTED")
                        {
                            var OPFECRSummary = new PFECRSummaryR()
                            {
                                EPF_Wages = OPFSummary.TotEPFWages+ OPFSummary.TotOffEPFWages,
                                EPS_Wages = OPFSummary.TotEPSWages + OPFSummary.TotOffEPSWages,
                                EDLI_Wages = OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages,
                                Gross_Wages = OPFSummary.TotGrossWages ,
                                EE_Share = OPFSummary.TotEPF + OPFSummary.TotOffEPF,
                                EPS_Share = OPFSummary.TotEPS + OPFSummary.TotOffEPS,
                                ER_Share = OPFSummary.TotER + OPFSummary.TotOffER,
                                EE_VPF_Share = OPFSummary.TotVPF + OPFSummary.TotOffVPF,
                                NCP_Days = OPFSummary.TotNCP,

                                Arrear_EPF_Wages = OPFSummary.TotArrEPFWages + dataaa.EPFWages,
                                Arrear_EPS_Wages = OPFSummary.TotArrEPSWages + dataaa.EPSWages,
                                Arrear_EDLI_Wages = OPFSummary.TotArrEDLIWages + dataaa.EDLIWages,
                                Arrear_EE_Share = OPFSummary.TotArrEPF + dataaa.EmpPF,
                                Arrear_EPS_Share = OPFSummary.TotArrEPS + dataaa.EmpEPS,
                                Arrear_ER_Share = OPFSummary.TotArrER + dataaa.CompPF,

                                Administrative_Charges_AC2 = Math.Round((OPFSummary.TotEPFWages+ OPFSummary.TotOffEPFWages) * OPFMaster.EPFAdminCharges / 100, 0),
                                Inspection_Charges_AC2 = Math.Round((OPFSummary.TotEPFWages + OPFSummary.TotOffEPFWages) * OPFMaster.EPFInspCharges / 100, 0),
                                Administrative_Charges_AC22 = Math.Round((OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages) * OPFMaster.EDLIAdmin / 100, 0),
                                Inspection_Charges_AC22 = Math.Round((OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages) * OPFMaster.EDLISInsp / 100, 0),
                                EDLI_Contribution_AC21 = Math.Round((OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages) * OPFMaster.EDLICharges / 100, 0),//to checked
                                ECRProcessDate = DateTime.Now,
                                Establishment_ID = OPFMaster.EstablishmentID,
                                Establishment_Name = OPFECRData.FirstOrDefault().Establishment_Name,
                                Exemption_Status = OPFMaster.PFTrustType == null ? null : OPFMaster.PFTrustType.LookupVal,
                                PFCalendar = OPFECRData.FirstOrDefault().PFCalendar == null ? null : db.Calendar.Find(OPFECRData.FirstOrDefault().PFCalendar.Id),
                                PFECRR = OPFECRData.ToList(),
                                Return_Month = mReturnMonth,
                                Total_Employees = Convert.ToDouble(OPFSummary.TotEmpCount),
                                Total_Employees_Excluded = 0,
                                Total_Gross_Wages_Excluded = 0,
                                Total_UANs = Convert.ToDouble(OPFSummary.TotEmpCount),
                                Wage_Month = mPayMonth,
                                SalECRFileName = mSalFileName,
                                ArrECRFileName = mArrFileName,
                                RetECRFileName = mRetFileName,
                                JoinECRFileName = mJoinFileName,
                                ECRPaymentReleaseDate = null,
                                DBTrack = dbt,
                            };
                            db.PFECRSummaryR.Add(OPFECRSummary);
                            db.SaveChanges();
                            var OCompanyPayrollSave = db.CompanyPayroll.Find(OCompanyPayroll.Id);
                            OCompanyPayrollSave.PFECRSummaryR.Add(OPFECRSummary);
                            db.Entry(OCompanyPayrollSave).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var OPFECRSummary = new PFECRSummaryR()
                            {
                                EPF_Wages = OPFSummary.TotEPFWages + OPFSummary.TotOffEPFWages,
                                EPS_Wages = OPFSummary.TotEPSWages + OPFSummary.TotOffEPSWages,
                                EDLI_Wages = OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages,//ahamadnagar edliwages 
                                Gross_Wages = OPFSummary.TotGrossWages,
                                EE_Share = 0,//OPFSummary.TotEPF,
                                EPS_Share = OPFSummary.TotEPS + OPFSummary.TotOffEPS,
                                ER_Share = 0,//OPFSummary.TotER,
                                EE_VPF_Share = 0,//OPFSummary.TotVPF,
                                NCP_Days = OPFSummary.TotNCP,

                                Arrear_EPF_Wages = OPFSummary.TotArrEPFWages + dataaa.EPFWages,
                                Arrear_EPS_Wages = OPFSummary.TotArrEPSWages + dataaa.EPSWages,
                                Arrear_EDLI_Wages = 0,//OPFSummary.TotArrEDLIWages + dataaa.EDLIWages,
                                Arrear_EE_Share = 0,//OPFSummary.TotArrEPF + dataaa.EmpPF,
                                Arrear_EPS_Share = OPFSummary.TotArrEPS + dataaa.EmpEPS,
                                Arrear_ER_Share = 0,//OPFSummary.TotArrER + dataaa.CompPF,

                                Administrative_Charges_AC2 = Math.Round((OPFSummary.TotEPFWages + OPFSummary.TotOffEPFWages) * OPFMaster.EPFAdminCharges / 100, 0),
                                Inspection_Charges_AC2 = Math.Round((OPFSummary.TotEPFWages + OPFSummary.TotOffEPFWages) * OPFMaster.EPFInspCharges / 100, 0),
                                Administrative_Charges_AC22 = Math.Round((OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages) * OPFMaster.EDLIAdmin / 100, 0),
                                Inspection_Charges_AC22 = Math.Round((OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages) * OPFMaster.EDLISInsp / 100, 0),
                                EDLI_Contribution_AC21 = Math.Round((OPFSummary.TotEDLIWages + OPFSummary.TotOffEDLIWages) * OPFMaster.EDLICharges / 100, 0),//to checked
                                ECRProcessDate = DateTime.Now,
                                Establishment_ID = OPFMaster.EstablishmentID,
                                Establishment_Name = OPFECRData.FirstOrDefault().Establishment_Name,
                                Exemption_Status = OPFMaster.PFTrustType == null ? null : OPFMaster.PFTrustType.LookupVal,
                                PFCalendar = OPFECRData.FirstOrDefault().PFCalendar == null ? null : db.Calendar.Find(OPFECRData.FirstOrDefault().PFCalendar.Id),
                                PFECRR = OPFECRData.ToList(),
                                Return_Month = mReturnMonth,
                                Total_Employees = Convert.ToDouble(OPFSummary.TotEmpCount),
                                Total_Employees_Excluded = 0,
                                Total_Gross_Wages_Excluded = 0,
                                Total_UANs = Convert.ToDouble(OPFSummary.TotEmpCount),
                                Wage_Month = mPayMonth,
                                SalECRFileName = mSalFileName,
                                ArrECRFileName = mArrFileName,
                                RetECRFileName = mRetFileName,
                                JoinECRFileName = mJoinFileName,
                                ECRPaymentReleaseDate = null,
                                DBTrack = dbt,
                            };
                            db.PFECRSummaryR.Add(OPFECRSummary);
                            db.SaveChanges();
                            var OCompanyPayrollSave = db.CompanyPayroll.Find(OCompanyPayroll.Id);
                            OCompanyPayrollSave.PFECRSummaryR.Add(OPFECRSummary);
                            db.Entry(OCompanyPayrollSave).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }


                    }
                }
            }
        }

        public class SalaryArrearPFTClass
        {

            public string UAN { get; set; }
            public string UAN_Name { get; set; }
            public string Establishment_ID { get; set; }
            public double CompPF { get; set; }
            public double EDLIWages { get; set; }
            public double EmpEPS { get; set; }
            public double EmpPF { get; set; }
            public double EmpVPF { get; set; }
            public double EPFWages { get; set; }
            public double EPSWages { get; set; }
            public double SalaryWages { get; set; }
            public string mArrFileName { get; set; }
        }
        public static SalaryArrearPFTClass GenerateArrPFECR(PFMaster OPFMasterARR, int mCompId, string mPayMonth, List<int?> emppayrollidlist)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                //var OCompanyPayroll = db.CompanyPayroll
                //    .Include(e => e.PFMaster)
                //    .Include(e => e.PFMaster.Select(r => r.EPSWages))
                //    .Include(e => e.PFMaster.Select(r => r.PFAdminWages))
                //    .Include(e => e.PFMaster.Select(r => r.PFEDLIWages))
                //    .Include(e => e.PFMaster.Select(r => r.PFInspWages))
                //    .Include(e => e.PFMaster.Select(r => r.PFTrustType))
                //    .Include(e => e.PFMaster.Select(r => r.EPFWages))
                //    .Include(e => e.Company)
                //    //.Include(e=>e.Company.Employee)
                //    .Include(e => e.PFECRSummaryR)
                //    .Include(e => e.PFECRSummaryR.Select(r => r.PFECRR))
                //    .Where(e => e.Company.Id == mCompId)
                //    .SingleOrDefault();
                //var OPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + mPayMonth).Date).SingleOrDefault();

                //if (OPFMaster == null)
                //{
                //    OPFMaster = OCompanyPayroll.PFMaster.LastOrDefault();
                //}

                //var OSalaryTQuery = db.EmployeePayroll
                //        .Include(e => e.SalaryT)
                //        .Include(e => e.SalaryArrearT)
                //        .Include(e => e.SalaryArrearT.Select(q => q.SalaryArrearPFT))
                //        .Include(e => e.SalaryT.Select(r => r.PFECRR))
                //        .Include(e => e.SalaryT.Select(r => r.PFECRR.PFCalendar)).ToList();
                //add company filter
                //.Where(e => e.Employee.com ) //OCompanyPayroll.Company.Employee.Contains(e.Employee.Id)).ToList();

                //Last Month
                List<SalaryArrearPFT> OOSalaryTQuery = new List<SalaryArrearPFT>();
                List<SalaryArrearPFTClass> OSalaryArrearPFTClass = new List<SalaryArrearPFTClass>();
                List<int> OOEmployeePayroll = db.EmployeePayroll.Where(e => emppayrollidlist.Contains(e.Id)).AsNoTracking().Select(q => q.Id).ToList();
                //List<int> OOEmployeePayroll = new List<int>();
                //OOEmployeePayroll.Add(1416);
                //OOEmployeePayroll.Add(1445);
                //OOEmployeePayroll.Add(1);
                //OOEmployeePayroll.Add(2);
                List<int> OOEmployeePayrollList = new List<int>();
                double CompPFT = 0;
                double EDLIWagesT = 0;
                double EmpEPST = 0;
                double EmpPFT = 0;
                double EPFWagesT = 0;
                double EPSWagesT = 0;

                foreach (var item in OOEmployeePayroll)
                {
                    //var OSaQuery = db.EmployeePayroll
                    //            .Include(e => e.SalaryArrearT)
                    //            .Include(e => e.SalaryArrearT.Select(q => q.SalaryArrearPFT))
                    //            .Include(e => e.SalaryT)
                    //            .Include(e => e.SalaryT.Select(r => r.PFECRR))
                    //            .Include(e => e.SalaryT.Select(r => r.PFECRR.PFCalendar)).AsNoTracking()
                    //            .Where(q => q.Id == item).SingleOrDefault();
                    var OPFECRRData = db.SalaryT.Include(e => e.PFECRR).Where(r => r.EmployeePayroll_Id == item && r.PayMonth == mPayMonth && r.PFECRR != null && r.PFECRR.Establishment_ID == OPFMasterARR.EstablishmentID).Select(q => q.PFECRR).SingleOrDefault();
                 //   OOSalaryTQuery = OSaQuery.SalaryArrearT.Where(q => q.PayMonth == mPayMonth && q.IsPaySlip == true && q.SalaryArrearPFT != null).Select(q => q.SalaryArrearPFT).ToList();

                  //  var OPFECRRData = OSaQuery.SalaryT.Where(r => r.PayMonth == mPayMonth && r.PFECRR != null && r.PFECRR.Establishment_ID==OPFMasterARR.EstablishmentID).Select(q => q.PFECRR).SingleOrDefault();
                    if (OPFECRRData != null && OPFECRRData.Arrear_EE_Share > 0)
                    {

                        CompPFT = OPFECRRData.Arrear_ER_Share;
                        EDLIWagesT = OPFECRRData.Arrear_EDLI_Wages;
                        EmpEPST = OPFECRRData.Arrear_EPS_Share;
                        EmpPFT = OPFECRRData.Arrear_EE_Share;
                        EPFWagesT = OPFECRRData.Arrear_EPF_Wages;
                        EPSWagesT = OPFECRRData.Arrear_EPS_Wages;

                        SalaryArrearPFTClass salclassTotal = new SalaryArrearPFTClass()
                        {
                            UAN = OPFECRRData.UAN,
                            UAN_Name = OPFECRRData.UAN_Name,
                            Establishment_ID = OPFECRRData.Establishment_ID,
                            CompPF = OPFECRRData.Arrear_ER_Share,
                            EDLIWages = OPFECRRData.Arrear_EDLI_Wages,
                            EmpEPS = OPFECRRData.Arrear_EPS_Share,
                            EmpPF = OPFECRRData.Arrear_EE_Share,
                            EPFWages = OPFECRRData.Arrear_EPF_Wages,
                            EPSWages = OPFECRRData.Arrear_EPS_Wages,
                        };
                        OSalaryArrearPFTClass.Add(salclassTotal);

                        //CompPFT = CompPFT + salclassTotal.CompPF;
                        //EDLIWagesT = EDLIWagesT + salclassTotal.EDLIWages;
                        //EmpEPST = EmpEPST +salclassTotal.EmpEPS;
                        //EmpPFT = EmpPFT + salclassTotal.EmpPF;
                        //EPFWagesT = EPFWagesT +salclassTotal.EPFWages;
                        //EPSWagesT = EPSWagesT + salclassTotal.EPSWages;
                        //}
                        //else
                        //{
                        //    SalaryArrearPFTClass salclassTotal = new SalaryArrearPFTClass()
                        //    {
                        //        UAN = "",
                        //        UAN_Name = "",
                        //        CompPF = OOSalaryTQuery.Sum(q => q.CompPF),
                        //        EDLIWages = OOSalaryTQuery.Sum(q => q.EDLIWages),
                        //        EmpEPS = OOSalaryTQuery.Sum(q => q.EmpEPS),
                        //        EmpPF = OOSalaryTQuery.Sum(q => q.EmpPF),
                        //        EmpVPF = OOSalaryTQuery.Sum(q => q.EmpVPF),
                        //        EPFWages = OOSalaryTQuery.Sum(q => q.EPFWages),
                        //        EPSWages = OOSalaryTQuery.Sum(q => q.EPSWages),
                        //    };
                        //    OSalaryArrearPFTClass.Add(salclassTotal);
                        //}

                        //OOSalaryTQuery.Select(q => q.SalaryArrearPFT);
                    }
                }

                //if (OPFECRData != null && OPFECRData.Count > 0)
                //{

                //var OECRSummayDel = OCompanyPayroll.PFECRSummaryR.Where(e => e.Wage_Month == mPayMonth).SingleOrDefault();
                //if (OECRSummayDel != null)
                //{

                //    OECRSummayDel.PFECRR = null;
                //    db.Entry(OECRSummayDel).State = System.Data.Entity.EntityState.Modified;
                //    db.SaveChanges();

                //    OCompanyPayroll.PFECRSummaryR.Remove(OECRSummayDel);
                //    db.Entry(OCompanyPayroll).State = System.Data.Entity.EntityState.Modified;
                //    db.SaveChanges();
                //    db.PFECRSummaryR.Attach(OECRSummayDel);
                //    db.Entry(OECRSummayDel).State = System.Data.Entity.EntityState.Deleted;
                //    db.SaveChanges();

                //    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\PFECRRFile";
                //    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                //    string localPath = new Uri(requiredPath).LocalPath;
                //    if (!System.IO.Directory.Exists(localPath))
                //    {
                //        localPath = new Uri(requiredPath).LocalPath;
                //        System.IO.Directory.CreateDirectory(localPath);
                //    }
                //    string path = requiredPath + "\\ECR_PF_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
                //    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                //    path = new Uri(path).LocalPath;
                //    if (System.IO.File.Exists(path))
                //    {
                //        File.Delete(path);
                //    }

                //    path = requiredPath + "\\PFECRArrear_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
                //    //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                //    path = new Uri(path).LocalPath;
                //    if (System.IO.File.Exists(path))
                //    {
                //        File.Delete(path);
                //    }

                //}

                //ecr text file creation
                ECRFile OECRFile = new ECRFile();
                string mArrFileName = OECRFile.OCreateECRArrearFile(OPFMasterARR,OSalaryArrearPFTClass, mPayMonth);
                SalaryArrearPFTClass OOSalaryTQueryList = new SalaryArrearPFTClass()
                {

                    mArrFileName = mArrFileName,
                    CompPF = CompPFT,
                    EDLIWages = EDLIWagesT,
                    EmpEPS = EmpEPST,
                    EmpPF = EmpPFT,
                    EPFWages = EPFWagesT,
                    EPSWages = EPSWagesT,
                    Establishment_ID=OPFMasterARR.EstablishmentID
                };
                return OOSalaryTQueryList;
                //.ToList();
                //calculate pfecr summary
                //---------------------------------------


                //}
            }
        }

        public static Int32 _returnTransctionAmt(double oTransctionAmt)
        {
            Int32 temp = 0;
            var aa = oTransctionAmt - (int)oTransctionAmt;
            if (aa != 0.0)
            {
                temp = Convert.ToInt32(Convert.ToDouble(oTransctionAmt.ToString("0.00")) * 100);
            }
            else
            {
                temp = Convert.ToInt32(oTransctionAmt) * 100;
            }
            return temp;
        }
        ///company tds end


    }
}