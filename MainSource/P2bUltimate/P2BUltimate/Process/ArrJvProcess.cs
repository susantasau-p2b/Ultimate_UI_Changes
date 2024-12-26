using System;
using P2b.Global;
using P2BUltimate.App_Start;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using P2BUltimate.Models;
using Payroll;
using P2BUltimate.Security;
using System.IO;

namespace P2BUltimate.Process
{
    public static class ArrJvProcess
    {
        public static ArrJVParameter _returnArrJvParm(Int32 Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ArrJVParameter ArrJVParameter = db.ArrJVParameter.Include(e => e.ArrSalaryHead)
                    .Include(e => e.JVGroup)
                    .Include(e => e.ArrSalaryHead.Select(a => a.SalHeadOperationType))
                    .Include(e => e.ArrFuncStruct).Include(e => e.ArrPayStruct)
                    .Include(e => e.ArrFuncStruct.JobPosition)
                    .Include(e => e.ArrJobPosition)
                    .Where(e => e.Id == Id).FirstOrDefault();
                return ArrJVParameter;
            }
        }

        public static EmployeePayroll _returnEmpArrLoanAndOtherDed(Int32 EmpPayId, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var _NextMonth_PayMonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).Date;

                EmployeePayroll OEmployeePayroll_loan = db.EmployeePayroll.Where(q => q.Id == EmpPayId)
                    .Include(e => e.Employee.EmpOffInfo.NationalityID)
                     .Include(e => e.LoanAdvRequest)
                     .Include(e => e.LoanAdvRequest.Select(r => r.LoanAccBranch.LocationObj))
                         .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                         .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                         .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                         .Include(e => e.SalaryArrearT.Select(r => r.GeoStruct.Location.LocationObj))
                         .Include(e => e.SalaryArrearT.Select(r => r.GeoStruct.Company))
                         .Include(e => e.SalaryArrearT.Select(r => r.FuncStruct))
                         .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPFT))
                    //.Include(e => e.SalaryArrearT.Select(r => r.PaymentBranch))
                    // .Include(e => e.SalaryArrearT.Select(s => s.PTaxTransT))
                    //.Include(e => e.SalaryArrearT.Select(r => r.LWFTransT))
                    //.Include(e => e.SalaryArrearT.Select(r => r.ESICTransT))
                    //.Include(e => e.SalaryArrearT.Select(r => r.ITaxTransT))
                    //.Include(e => e.SalaryArrearT.Select(r => r.PerkTransT))
                         .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))//VR
                        .AsParallel()
                     .FirstOrDefault();
                return OEmployeePayroll_loan;
            }
        }

        public static EmployeePayroll _returnEmpArrYearly(Int32 EmpPayId, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                EmployeePayroll EmployeePayroll = db.EmployeePayroll.Where(q => q.Id == EmpPayId && q.SalaryArrearT.Any(e => e.PayMonth == PayMonth))
                    .Include(e => e.Employee.EmpOffInfo)
                    .Include(e => e.SalaryArrearT)
                    .Include(e => e.SalaryArrearT.Select(r => r.EmployeePayroll))
                    .Include(e => e.SalaryArrearT.Select(r => r.EmployeePayroll.Employee))
                    .Include(e => e.SalaryArrearT.Select(r => r.EmployeePayroll.Employee.EmpOffInfo))
                    .Include(e => e.SalaryArrearT.Select(r => r.EmployeePayroll.Employee.EmpOffInfo.AccountType))
                    .Include(e => e.SalaryArrearT.Select(r => r.EmployeePayroll.Employee.EmpOffInfo.Branch))
                    .Include(e => e.SalaryArrearT.Select(r => r.GeoStruct.Location))
                    .Include(e => e.SalaryArrearT.Select(r => r.GeoStruct.Location.LocationObj))
                    .Include(e => e.SalaryArrearT.Select(r => r.GeoStruct.Company))
                    .Include(e => e.SalaryArrearT.Select(r => r.FuncStruct))
                    .Include(e => e.SalaryArrearT.Select(r => r.FuncStruct.JobPosition))
                    .Include(e => e.SalaryArrearT.Select(r => r.PayStruct))
                    .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                    .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead)))
                    .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead.SalHeadOperationType)))
                    .Include(e => e.SalaryArrearT.Select(s => s.SalaryArrearPFT))
                    //.Include(e => e.SalaryArrearT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.SalHeadOperationType)))
                    //.Include(e => e.SalaryArrearT.Select(r => r.AccType))
                    //.Include(e => e.SalaryArrearT.Select(r => r.PayMode))
                    .Include(e => e.YearlyPaymentT)
                    .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                    .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead.SalHeadOperationType))
                    .AsParallel()
                    .FirstOrDefault();
                return EmployeePayroll;
            }
        }

        public static void GenerateArrJV(string mPayMonth, int mCompanyPayrollId, List<int> OEmployeePayrollId, string mBatchName, List<int> JVCodeList)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ArrJVProcessData> OArrJVProcessData = new List<ArrJVProcessData>();
                var OJVParameterComp = db.CompanyPayroll
                                       .Include(e => e.ArrJVProcessData)
                                       .Include(e => e.ArrJVProcessDataSummary).AsParallel()
                    .Where(e => e.Id == mCompanyPayrollId)
                    .SingleOrDefault();
                var OJVParameter = new List<ArrJVParameter>();
                foreach (var item in JVCodeList)
                {
                    var aa = _returnArrJvParm(item);
                    if (aa != null)
                    {
                        OJVParameter.Add(aa);
                    }
                }
                //Delete old record
                var OProcessDataSumDel = OJVParameterComp.ArrJVProcessDataSummary.Where(e => e.ArrProcessMonth == mPayMonth && e.ArrBatchName == mBatchName).ToList();

                var OLockCheck = OProcessDataSumDel.Where(e => e.IsLock == true).SingleOrDefault();
                if (OLockCheck != null)
                {
                    return; // JV is locked for the month mpaymonth
                }
                if (OProcessDataSumDel != null && OProcessDataSumDel.Count() > 0)
                {
                    db.ArrJVProcessDataSummary.RemoveRange(OProcessDataSumDel);
                    //foreach (var ca in OProcessDataSumDel)
                    //{
                    //    db.ArrJVProcessDataSummary.Attach(ca);
                    //    db.Entry(ca).State = System.Data.Entity.EntityState.Deleted;
                    //}
                    db.SaveChanges();
                }

                //  var OProcessDataSum = db.ArrJVProcessData.Where(e => e.ArrProcessMonth == mPayMonth && e.ArrBatchName == mBatchName).ToList();

                var OProcessDataDel = OJVParameterComp.ArrJVProcessData.Where(e => e.ArrProcessMonth == mPayMonth && e.ArrBatchName == mBatchName).ToList();
                if (OProcessDataDel != null && OProcessDataDel.Count() > 0)
                {
                    db.ArrJVProcessData.RemoveRange(OProcessDataDel);
                    //foreach (var ca in OProcessDataDel)
                    //{
                    //    db.ArrJVProcessData.Attach(ca);
                    //    db.Entry(ca).State = System.Data.Entity.EntityState.Deleted;
                    //}
                    db.SaveChanges();
                }


                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\JVFile";
                //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                string localPath = new Uri(requiredPath).LocalPath;
                if (!System.IO.Directory.Exists(localPath))
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + "\\JV_" + Convert.ToDateTime("01/" + mPayMonth).ToString("MMyyyy") + ".txt";
                //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                path = new Uri(path).LocalPath;
                if (System.IO.File.Exists(path))
                {
                    File.Delete(path);
                }
                List<EmployeePayroll> OSalaryTF = new List<EmployeePayroll>();
                List<EmployeePayroll> OEmployeePayroll = new List<EmployeePayroll>();
                List<SalaryArrearT> OSalaryArrearTemp = new List<SalaryArrearT>();
                //List<EmployeePayroll> OArrSalaryTF = new List<EmployeePayroll>();
                for (int i = 0; i < OEmployeePayrollId.Count; i++)
                {
                    var EmployeePayroll = _returnEmpArrYearly(OEmployeePayrollId[i], mPayMonth);
                    if (EmployeePayroll != null)
                    {
                        OSalaryTF.Add(EmployeePayroll);
                    }
                }

                var Id = Convert.ToInt32(SessionManager.CompanyId);
                string _CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();
                string _CustomeCompCode = "BDCB";
                var JVParameter_SalaryHead_List = new List<ArrJVParameter>();
                var OSalaryT = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList(); //changes reqq
                //var OSalaryT = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).FirstOrDefault() }).ToList(); //changes reqq
                foreach (var id in JVCodeList)
                {
                    var JVParameter_SalaryHead_List_temp = _returnArrJvParm(id);
                    if (JVParameter_SalaryHead_List_temp != null)
                    {
                        JVParameter_SalaryHead_List.Add(JVParameter_SalaryHead_List_temp);
                    }
                }

                var JVParameter_SalaryHead = JVParameter_SalaryHead_List.SelectMany(a => a.ArrSalaryHead).Select(a => a.Id).Distinct().ToList();

                var OYearlyT = OSalaryTF.Select(e => new
                {
                    Osal = e.YearlyPaymentT != null ? e.YearlyPaymentT.Where(d => d.PayMonth == mPayMonth &&
                        //   d.ArrSalaryHead.InPayslip == false
                       JVParameter_SalaryHead.Contains(d.SalaryHead.Id)
                        ).ToList() : null,
                    AccType = e.Employee.EmpOffInfo.AccountType,
                    AccNo = e.Employee.EmpOffInfo.AccountNo,
                    Branch = e.Employee != null && e.Employee.EmpOffInfo != null && e.Employee.EmpOffInfo.Branch != null ? e.Employee.EmpOffInfo.Branch.Code : null
                }).ToList();
                //for (int i = 0; i < OEmployeePayrollId.Count; i++)
                //{
                //    var OArrSalaryTF_temp = _returnEmpSalaryArrear(OEmployeePayrollId[i], mPayMonth);
                //    if (OArrSalaryTF_temp != null)
                //    {
                //        OArrSalaryTF.Add(OArrSalaryTF_temp);
                //    }
                //}
                // var OArrSalaryT = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();

                foreach (var item in OSalaryTF)
                {
                    if (item.SalaryArrearT != null && item.SalaryArrearT.Count > 0)
                    {
                        List<SalaryArrearT> dfds = item.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList();
                        OSalaryArrearTemp.AddRange(dfds);
                    }
                }
                var OArrSalaryT = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() });
                //var _PayMonth = Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).Date;
                for (int i = 0; i < OEmployeePayrollId.Count; i++)
                {
                    EmployeePayroll OEmployeePayroll_loan = _returnEmpArrLoanAndOtherDed(OEmployeePayrollId[i], mPayMonth);
                    if (OEmployeePayroll_loan != null)
                    {
                        OEmployeePayroll.Add(OEmployeePayroll_loan);
                    }
                }
                if (_CompCode == _CustomeCompCode)
                {
                    foreach (var JvCode in JVCodeList)
                    {
                        ArrJVParameter _Find_jv_Data = _returnArrJvParm(JvCode);
                        foreach (var _SalHead in _Find_jv_Data.ArrSalaryHead)
                        {
                            var _SalaryHeadData = _Find_jv_Data.ArrSalaryHead.Where(e => e.Id == _SalHead.Id)
                                .Select(e => new { _Id = e.Id, _OperationType = e.SalHeadOperationType.LookupVal.ToUpper(), _Name = e.Name, _Code = e.Code }).SingleOrDefault();
                            #region ind
                            if (_Find_jv_Data.JVGroup.LookupVal.ToUpper() == "INDIVIDUAL")
                            {
                                switch (_SalaryHeadData._OperationType)
                                {
                                    case "NET":
                                        //var OSal = OSalaryT.Select(e => e.Osal).ToList();
                                        var OSalaryT1 = OSalaryTF.Select(e => new
                                        {
                                            ArrSubAccountNo = e.Employee != null && e.Employee.EmpOffInfo != null ? e.Employee.EmpOffInfo.AccountNo : null,
                                            Osal = e.SalaryArrearT != null ? e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).SingleOrDefault() : null,
                                            //  SalaryArrearPFT = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).Select(a => a.SalaryArrearPFT).ToList(),
                                        }).ToList();
                                        List<ArrJVProcessData> OArrJVProcessDataNet = OSalaryT1
                                            .Select(e => new ArrJVProcessData
                                            {
                                                ArrBatchName = mBatchName,
                                                ArrProcessMonth = mPayMonth,
                                                ArrProcessDate = DateTime.Now.Date,
                                                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                //ArrBranchCode = e.PaymentBranch != null ? e.PaymentBranch.Code : "",
                                                //ccountProductCode = e.AccType != null ? e.AccType.LookupVal.ToUpper() : "",
                                                ArrAccountCustomerNo = "",
                                                ArrAccountCode = e.ArrSubAccountNo,
                                                ArrSubAccountCode = "",
                                                ArrTransactionAmount =PayrollReportGen._returnTransctionAmt(e.Osal.ArrTotalNet).ToString(),
                                                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                ArrNarration = "Net Salary For Month :" + mPayMonth,
                                                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "NET").SingleOrDefault()
                                            }).ToList();

                                        OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                        break;
                                    case "LOAN":
                                        var OLoanReq = OEmployeePayroll.Select(a => a.LoanAdvRequest).Count() > 0 ?
                                            OEmployeePayroll
                                            .SelectMany(s => s.LoanAdvRequest.Where(e => (e.CloserDate == null ||
                                                e.CloserDate >= Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).Date)
                                                && e.LoanAdvanceHead.SalaryHead != null &&
                                            e.LoanAdvanceHead.SalaryHead.Id == _SalaryHeadData._Id
                                            )).ToList() : null;

                                        if (OLoanReq != null && OLoanReq.Count() > 0)
                                        {
                                            foreach (var ca6 in OLoanReq.ToList())
                                            {

                                                if (ca6 != null)
                                                {

                                                    var OLoanData = ca6.LoanAdvRepaymentT.Count > 0 ? ca6.LoanAdvRepaymentT.Where(r => r.PayMonth == mPayMonth &&
                                                        r.RepaymentDate != null && r.InstallmentPaid != 0).ToList() : null;
                                                    if (OLoanData != null && OLoanData.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataLoan = OLoanData
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                            // ArrBranchCode = ca5.LoanAccBranch != null ? ca5.LoanAccBranch.LocationObj.LocCode : null,
                                                            // ArrAccountProductCode = ca5.LoanAdvanceHead.Code,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca6.LoanAccNo != null ? ca6.LoanAccNo : "",
                                                            ArrSubAccountCode = ca6.LoanSubAccNo == null ? "" : ca6.LoanSubAccNo,
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.InstallmentPaid).ToString(),
                                                            ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                            ArrNarration = "Loan" + ca6.LoanAdvanceHead.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca6.LoanAdvanceHead.SalaryHead.Id).SingleOrDefault()
                                                        }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataLoan);
                                                    }

                                                }
                                            }
                                        }



                                        break;
                                    case "NONREGULAR":
                                        //SalaryArrearPaymentT VR
                                        var JvDataPerticular = db.ArrJVParameter.Include(q => q.ArrJVNonStandardEmp)
                                           .Include(q => q.ArrJVNonStandardEmp.Select(a => a.Branch))
                                           .Include(q => q.ArrJVNonStandardEmp.Select(a => a.EmployeePayroll))
                                           .Where(a => a.Id == _Find_jv_Data.Id).SingleOrDefault();

                                        List<EmployeePayroll> OSalaryTFNonreg = new List<EmployeePayroll>();

                                        var EmpOSalaryTNonreg = JvDataPerticular.ArrJVNonStandardEmp.Select(q => q.EmployeePayroll.Id).ToList();
                                        if (EmpOSalaryTNonreg.Count > 0)
                                        {
                                            for (int i = 0; i < EmpOSalaryTNonreg.Count; i++)
                                            {
                                                var EmployeePayroll = _returnEmpArrYearly(EmpOSalaryTNonreg[i], mPayMonth);
                                                if (EmployeePayroll != null)
                                                {
                                                    OSalaryTFNonreg.Add(EmployeePayroll);
                                                }
                                            }
                                        }
                                        var OSalaryT1Nonreg = OSalaryTFNonreg.Select(e => new
                                        {
                                            ArrBranchCode = JvDataPerticular.ArrJVNonStandardEmp.Where(q => q.EmployeePayroll.Id == e.Id).Select(r => r.Branch.Code).SingleOrDefault(),
                                            ArrSubAccountNo = JvDataPerticular.ArrJVNonStandardEmp.Where(q => q.EmployeePayroll.Id == e.Id).Select(r => r.ArrSubAccountNo).SingleOrDefault(),
                                            Osal = e.SalaryArrearT != null ? e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).SingleOrDefault() : null,
                                            //  SalaryArrearPFT = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).Select(a => a.SalaryArrearPFT).ToList(),
                                        }).ToList();

                                        List<ArrJVProcessData> OArrJVProcessDataNetNonrreg = OSalaryT1Nonreg
                                            .Select(e => new ArrJVProcessData
                                            {
                                                ArrBatchName = mBatchName,
                                                ArrProcessMonth = mPayMonth,
                                                ArrProcessDate = DateTime.Now.Date,
                                                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                //   ArrBranchCode = e.PaymentBranch != null ? e.PaymentBranch.Code : "",
                                                //ccountProductCode = e.AccType != null ? e.AccType.LookupVal.ToUpper() : "",
                                                ArrAccountCustomerNo = "",
                                                ArrAccountCode = e.ArrSubAccountNo,
                                                ArrSubAccountCode = "",
                                                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.Osal.ArrTotalNet).ToString(),
                                                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                ArrNarration = "Net Salary For Month :" + mPayMonth,
                                                //ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca6.LoanAdvanceHead.SalaryHead.Id).SingleOrDefault()

                                            }).ToList();

                                        OArrJVProcessData.AddRange(OArrJVProcessDataNetNonrreg);
                                        break;
                                }
                            }
                            #endregion ind
                            #region location
                            else if (_Find_jv_Data.JVGroup.LookupVal.ToUpper() == "LOCATION")
                            {
                                switch (_SalaryHeadData._OperationType)
                                {

                                    case "GROSS":
                                        if (OSalaryT != null && OSalaryT.Count() > 0)
                                        {
                                            var OSal = OSalaryT
                                                .Select(e => e.Osal).ToList();
                                            foreach (var item in OSal)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataNet = item
                                         .Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                            .Select(e => new ArrJVProcessData
                                            {
                                                ArrBatchName = mBatchName,
                                                ArrProcessMonth = mPayMonth,
                                                ArrProcessDate = DateTime.Now.Date,
                                                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                ////ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,
                                                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                ArrAccountCustomerNo = "",
                                                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.Sum(r => r.ArrTotalEarning)).ToString(),
                                                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                            }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                            }
                                        }
                                        //Emp_Monthsal;
                                        break;
                                    case "REGULAR":

                                        var OSal_re = OSalaryT
                                            .Select(e => e.Osal).ToList();
                                        foreach (var item in OSal_re)
                                        {
                                            var OGrp = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                  .Select(e => new
                                  {
                                      location = e.Key,
                                      SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                  }).ToList();
                                            List<ArrJVProcessData> OArrJVProcessDataNet_r = OGrp
                                               .Select(e => new ArrJVProcessData
                                               {
                                                   ArrBatchName = mBatchName,
                                                   ArrProcessMonth = mPayMonth,
                                                   ArrProcessDate = DateTime.Now.Date,
                                                   ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                   //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                   //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,
                                                   //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                   ArrAccountCustomerNo = "",
                                                   ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                   ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                   ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),  //ask
                                                   ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                   ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                   ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                               }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataNet_r);

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                   .Select(e => e.Osal).ToList();

                                                //var OGrpArr = OSal2.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                //    .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList() }).ToList();

                                                var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id)
                                                   .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList() }).ToList();

                                                if (OGrpArr != null && OGrpArr.Count() > 0)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                            //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                            //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,
                                                            //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            //ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),
                                                            ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                            ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                }
                                            }
                                        }

                                        break;
                                    case "BASIC":

                                        var OSalBASIC = OSalaryT
                                              .Select(e => e.Osal).ToList();
                                        foreach (var item in OSalBASIC)
                                        {
                                            var OGrpBASIC = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn)
                                      .GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                      .Select(e => new
                                      {
                                          location = e.Key,
                                          SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                      }).ToList();
                                            foreach (var ca10 in OGrpBASIC)
                                            {
                                                ArrJVProcessData OArrJVProcessDataBASIC = new ArrJVProcessData()
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //      ArrBranchCode = ca10.location.LocationObj.LocCode,
                                                    //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(ca10.SalDetails.Sum(d => d.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                };
                                                OArrJVProcessData.Add(OArrJVProcessDataBASIC);
                                            }

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                   .Select(e => e.Osal).ToList();
                                                if (OSal2 != null)
                                                {

                                                    var OGrpArr = OSal2.Where(x => x.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn)
                                                        //.GroupBy(a => a.GeoStruct.Location.LocationObj.LocCode)
                                                        .GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id)
                                                        .Select(e => new
                                                        {
                                                            location = e.Key,
                                                            SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList()
                                                        }).ToList();

                                                    if (OGrpArr != null && OGrpArr.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                                //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),
                                                                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                                ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);

                                                    }
                                                }
                                            }

                                        }
                                        break;
                                    case "DA":

                                        var OSalVDA = OSalaryT
                                                  .Select(e => e.Osal).ToList();
                                        foreach (var item in OSalVDA)
                                        {
                                            var OGrpVDA = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn)
                                     .GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                     .Select(e => new
                                     {
                                         location = e.Key,
                                         SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                     }).ToList();
                                            List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                    //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();
                                            OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                   .Select(e => e.Osal).ToList();
                                                if (OSal2 != null)
                                                {
                                                    var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id)
                                                        .Select(e => new
                                                        {
                                                            location = e.Key,
                                                            SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList()
                                                        }).ToList();

                                                    if (OGrpArr != null && OGrpArr.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                                //   ArrBranchCode = e.location.LocationObj.LocCode,
                                                                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                                //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString(),
                                                                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),
                                                                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                                ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);

                                                    }
                                                }
                                            }
                                        }

                                        break;

                                    case "NONREGULAR":

                                        var OSal3 = OSalaryT
                                        .Select(e => e.Osal).ToList();
                                        foreach (var item in OSal3)
                                        {
                                            var OGrp1 = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                      .Select(e => new
                                      {
                                          location = e.Key,
                                          SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                      }).ToList();
                                            if (OGrp1 != null && OGrp1.Count() > 0)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                        //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                        //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                        //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                        ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                        ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                        ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                        ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                            }
                                        }

                                        break;
                                    case "EPF":

                                        var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal4 = OSalaryT1
                                               .SelectMany(e => e.Osal).ToList();
                                        var OEPF = OSal4.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(r => r.GeoStruct.Location.LocationObj.Id)
                                            .Select(e => new
                                            {
                                                SalDetails = e.Select(a => a.SalaryArrearPFT)
                                            }).ToList();

                                        if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                        {


                                            List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    // ArrBranchCode = e.location.LocCode,
                                                    //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    // ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.EmpPF + e.SalDetails.Arrear_EE_Share).ToString(),
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpPF)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Employee Share Provident Fund including Arrears" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                        }

                                        break;

                                    case "CPF":

                                        var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal5 = OSalaryT2
                                               .SelectMany(e => e.Osal).ToList();
                                        var OCPF = OSal5.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                            .ToList();

                                        if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                        {
                                            List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //   ArrBranchCode = e.location.LocCode,
                                                    //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.CompPF)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Company Share Provident Fund" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                        }

                                        break;
                                    case "PENSION":

                                        var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal6 = OSalaryT3
                                               .SelectMany(e => e.Osal).ToList();

                                        var OEPS = OSal6.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                            .ToList();
                                        if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                        {
                                            List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //   ArrBranchCode = e.location.LocCode,
                                                    //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpEPS)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Pension Share Provident Fund" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                        }

                                        break;

                                    //case "LWF":


                                    //    var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalEMPLWF = OSalaryTEMPLWF
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPEMPLWF = OSalEMPLWF.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                    //        .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                    //        .ToList();
                                    //    if (OEMPEMPLWF != null && OEMPEMPLWF.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,
                                    //                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employee Share LWF" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                    //    }

                                    //    break;
                                    //case "COMPLWF":

                                    //    var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPLWF = OSalaryTCOMPLWF
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OCOMPLWF = OSalCOMPLWF.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                    //         .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                    //        .ToList();
                                    //    if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                    //    {

                                    //        List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                // ArrBranchCode = e.location.LocCode,
                                    //                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                    //                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.CompAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employee Share LWF" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                    //    }

                                    //    break;
                                    //case "COMPESIC":

                                    //    var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPESIS = OSalaryTCOMPESIS
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OCOMPESIS = OSalCOMPESIS.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                    //         .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                    //        .ToList();

                                    //    if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //   ArrBranchCode = e.location.LocCode,
                                    //                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                    //                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.CompAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employer Share ESIS" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                    //    }

                                    //    break;
                                    //case "ESIC":

                                    //    var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPESIS = OSalCOMPEMPESIS.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                    //        .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                    //        .ToList();

                                    //    if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //       ArrBranchCode = e.location.LocCode,
                                    //                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                    //                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employees Share ESIS" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                    //    }

                                    //    break;
                                    //case "PTAX":  //ask

                                    //    var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalPTAX = OSalaryTPTAX
                                    //           .Select(e => e.Osal).ToList();
                                    //   // var OEMPPTAX = OSalPTAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn && r.PTaxTransT != null).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)  ask
                                    //    var OEMPPTAX = OSalPTAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn && r.SalaryArrearPaymentT != null).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                    //        .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT).ToList() })
                                    //        .ToList();
                                    //    if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //     ArrBranchCode = e.location.LocCode,
                                    //                //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                    //                //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.PTAmount)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employees Share PTAX including Arrears" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                    //    }

                                    //    break;
                                    case "ITAX":


                                        var OSalaryTITAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSalITAX = OSalaryTITAX
                                               .SelectMany(e => e.Osal).ToList();
                                        var OEMPITAX = OSalITAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                            .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList() })
                                            .ToList();
                                        if (OEMPITAX != null && OEMPITAX.Count() > 0) //Changes for Null
                                        {
                                            List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OEMPITAX
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                    //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,

                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    // ArrTransactionAmount = e.SalDetails.Sum(r => r.Amoount).ToString(),
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Employees ITAX" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                        }

                                        break;
                                    case "INSURANCE":

                                        var OSal_in = OSalaryT
                                            .Select(e => e.Osal).ToList();
                                        foreach (var item in OSal_in)
                                        {
                                            var OGrpi = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                                               .Select(e => new
                                                                               {
                                                                                   location = e.Key,
                                                                                   SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                                                               }).ToList();
                                            List<ArrJVProcessData> OArrJVProcessDataNet_ri = OGrpi
                                               .Select(e => new ArrJVProcessData
                                               {
                                                   ArrBatchName = mBatchName,
                                                   ArrProcessMonth = mPayMonth,
                                                   ArrProcessDate = DateTime.Now.Date,
                                                   ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                   //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                   //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,
                                                   //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                   ArrAccountCustomerNo = "",
                                                   ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                   ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                   ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                   ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                   ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                   ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                               }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataNet_ri);

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                   .Select(e => e.Osal).ToList();

                                                //var OGrpArr = OSal2.Where(r => r.GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                //    .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList() }).ToList();

                                                var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == _Find_jv_Data.ArrLocationIn).GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id)
                                                   .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList() }).ToList();

                                                if (OGrpArr != null && OGrpArr.Count() > 0)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                            //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                            //ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == _Find_jv_Data.LocationOut).SingleOrDefault().LocationObj.LocCode,
                                                            //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            //ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),
                                                            ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                            ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                }
                                            }
                                        }

                                        break;
                                    default:

                                        break;
                                }
                            }
                            #endregion
                            #region Company
                            else if (_Find_jv_Data.JVGroup.LookupVal.ToUpper() == "COMPANY")
                            {
                                switch (_SalaryHeadData._OperationType)
                                {
                                    case "GROSS":
                                        //if Location

                                        if (OSalaryT != null && OSalaryT.Count() > 0)
                                        {
                                            var OSal = OSalaryT
                                                .Select(e => e.Osal).ToList();
                                            foreach (var item in OSal)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataNet = item.GroupBy(t => t.GeoStruct.Company.Id)

                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    // ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    //ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    ////ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.Sum(r => r.ArrTotalEarning)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                                }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                            }
                                        }
                                        //Emp_Monthsal;
                                        break;
                                    case "BASIC":
                                        var OSalBASIC = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        foreach (var item in OSalBASIC)
                                        {
                                            var OGrpBASIC = item.GroupBy(t => t.GeoStruct.Company.Id).ToList().Select(e => new
                                            {
                                                SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                            });
                                            if (OGrpBASIC != null && OGrpBASIC.Count() > 0) //Changes for Null
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataBASIC = OGrpBASIC

                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                        //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                        // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                        //ArrAccountCustomerNo = "",
                                                        ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                        ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                        ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                        ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                        ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                    }).ToList();
                                                OArrJVProcessData.AddRange(OArrJVProcessDataBASIC);
                                            }
                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT
                                                   .Select(e => e.Osal).ToList();
                                                var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).ToList().Select(e => new
                                                {

                                                    SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList()
                                                }).ToList();

                                                if (OGrpArr != null && OGrpArr.Count() > 0)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                            //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                            // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                            //ArrAccountCustomerNo = "",
                                                            ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),
                                                            ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                            ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                }
                                            }
                                        }

                                        break;
                                    case "DA":
                                        var OSalVDA = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        foreach (var item in OSalVDA)
                                        {
                                            var OGrpVDA = item.GroupBy(t => t.GeoStruct.Company.Id).Select(e => new
                                            {
                                                SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                            }).ToList();
                                            List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();
                                            OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT
                                                   .Select(e => e.Osal).ToList();
                                                var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).ToList().Select(e => new { SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList() }).ToList();

                                                if (OGrpArr != null && OGrpArr.Count() > 0)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                            //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                            // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                            //ArrAccountCustomerNo = "",
                                                            ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),
                                                            ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                            ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                }
                                            }
                                        }

                                        break;

                                    case "REGULAR":
                                        var OSal1 = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        foreach (var item in OSal1)
                                        {
                                            var OGrp = item.GroupBy(t => t.GeoStruct.Company.Id).ToList().Select(e => new { SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList() }).ToList();

                                            List<ArrJVProcessData> OArrJVProcessDataRegular = OGrp

                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();
                                            OArrJVProcessData.AddRange(OArrJVProcessDataRegular);

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT.Where(r => r.Osal != null)
                                                   .Select(e => e.Osal).ToList();

                                                var OGrpArr = OSal2 == null ? null : OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).ToList()
                                                    .Select(e => new { SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == _SalaryHeadData._Id))).ToList() }).ToList();

                                                if (OGrpArr != null && OGrpArr.Count() > 0)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                            //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                            // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                            //ArrAccountCustomerNo = "",
                                                            ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.Sum(r => r.SalHeadAmount))).ToString(),

                                                            //  ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            //ArrTransactionAmount = e.SalDetails.Sum(a => (a.Select(d => Convert.ToDecimal(d.SalHeadAmount)))).ToString("0.00"),
                                                            ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                            ArrNarration = _SalaryHeadData._Name + " Arrear for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegular);
                                                }
                                            }
                                        }

                                        break;
                                    case "NONREGULAR":
                                        var OSal3 = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        foreach (var item in OSal3)
                                        {
                                            var OGrp1 = item.GroupBy(t => t.GeoStruct.Company.Id).ToList().Select(e => new { SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == _SalaryHeadData._Id)).ToList() }).ToList();
                                            if (OGrp1 != null && OGrp1.Count() > 0)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                        //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                        // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                        //ArrAccountCustomerNo = "",
                                                        ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                        ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                        ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),

                                                        ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                        ArrNarration = _SalaryHeadData._Name + " Salary for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                            }
                                        }
                                        break;
                                    case "EPF":

                                        var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal4 = OSalaryT1
                                               .SelectMany(e => e.Osal).ToList();
                                        var OEPF = OSal4.GroupBy(e => e.GeoStruct.Company.Id)
                                            .Select(e => new { SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() }).ToList();

                                        if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                        {
                                            List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpPF)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Employee Share Provident Fund including Arrears" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                        }
                                        break;

                                    case "CPF":
                                        var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal5 = OSalaryT2
                                               .SelectMany(e => e.Osal).ToList();
                                        var OCPF = OSal5.GroupBy(e => e.GeoStruct.Company.Id)
                                            .Select(e => new { SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                            .ToList();
                                        if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                        {

                                            List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.CompPF)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Company Share Provident Fund" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                        }
                                        break;
                                    case "PENSION":

                                        var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal6 = OSalaryT3
                                               .SelectMany(e => e.Osal).ToList();

                                        var OEPS = OSal6.GroupBy(e => e.GeoStruct.Company.Id)
                                            .Select(e => new { SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                            .ToList();

                                        if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                        {

                                            List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpEPS)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Pension Share Provident Fund" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                        }
                                        break;

                                    //case "LWF":
                                    //    var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new
                                    //    {
                                    //        Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList()
                                    //    }).ToList();
                                    //    var OSalEMPLWF = OSalaryTEMPLWF
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPEMPLWF = OSalEMPLWF.GroupBy(e => e.GeoStruct.Company.Id)
                                    //        .Select(e => new { SalDetails = e.Where(a => a.LWFTransT != null).Select(r => r.LWFTransT).ToList() })
                                    //        .ToList();
                                    //    var asa = OEMPEMPLWF.Where(e => e.SalDetails.Count > 0).Count();
                                    //    if (OEMPEMPLWF != null && OEMPEMPLWF.Count > 0 && asa > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                    //                // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                //ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employee Share LWF" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();


                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                    //    }

                                    //    break;
                                    //case "COMPLWF":
                                    //    var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPLWF = OSalaryTCOMPLWF
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OCOMPLWF = OSalCOMPLWF.GroupBy(e => e.GeoStruct.Company.Id)
                                    //         .Select(e => new { SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                    //        .ToList();
                                    //    if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                    //                // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                //ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.CompAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employee Share LWF" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                    //    }
                                    //    break;

                                    //case "COMPESIC":
                                    //    var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPESIS = OSalaryTCOMPESIS
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OCOMPESIS = OSalCOMPESIS.GroupBy(e => e.GeoStruct.Company.Id)
                                    //         .Select(e => new { SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                    //        .ToList();

                                    //    if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                    //    {

                                    //        List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                    //                // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                //ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.CompAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employer Share ESIS" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                    //    }
                                    //    break;
                                    //case "ESIC":
                                    //    var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPESIS = OSalCOMPEMPESIS.GroupBy(e => e.GeoStruct.Company.Id)
                                    //        .Select(e => new { SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                    //        .ToList();
                                    //    if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                    //                // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                //ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.EmpAmt)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employees Share ESIS" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                    //    }
                                    //    break;
                                    //case "PTAX":
                                    //    var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalPTAX = OSalaryTPTAX
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPPTAX = OSalPTAX.GroupBy(e => e.GeoStruct.Company.Id)
                                    //        .Select(e => new { SalDetails = e.Where(r => r.PTaxTransT != null).Select(r => r.PTaxTransT).ToList() })
                                    //        .ToList();

                                    //    if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                    //                //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                    //                // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                    //                //ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                    //                ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(r => r.PTAmount) + e.SalDetails.Sum(r => r.ArrearPTAmount)).ToString(),
                                    //                ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employees Share PTAX including Arrears" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                    //    }
                                    //    break;
                                    case "ITAX":
                                        var OSalaryTITAX = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSalITAX = OSalaryTITAX
                                               .SelectMany(e => e.Osal).ToList();
                                        var OEMPITAX = OSalITAX.Where(e => e.SalaryArrearPaymentT != null && e.SalaryArrearPaymentT.Count > 0).GroupBy(e => e.GeoStruct.Company.Id)
                                         .Select(e => new
                                         {

                                             SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead != null && t.SalaryHead.SalHeadOperationType != null && t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList()
                                         })
                                            .ToList();
                                        if (OEMPITAX != null && OEMPITAX.Count() > 0) //Changes for Null
                                        {
                                            List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OEMPITAX
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    // ArrTransactionAmount = e.SalDetails.Sum(r => r.SalHeadAmount).ToString("0.00"),

                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Employees ITAX" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                        }
                                        break;

                                    //Added By Sudhir

                                    case "LOAN":
                                        var OSalaryTLoan = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //var OSalaryTLoan = OEmployeePayroll.Select(s => new { Osal = s.LoanAdvRequest.Where(e => e.LoanAdvanceHead.SalaryHead.Id == _SalaryHeadData.Id && (e.CloserDate == null || e.CloserDate >= Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).Date)) }).ToList();
                                        var OSalLoan = OSalaryTLoan
                                               .SelectMany(e => e.Osal).ToList();
                                        var OEMPLoan = OSalLoan.GroupBy(e => e.GeoStruct.Company.Id)
                                            // .Select(e => new {   SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).FirstOrDefault() })
                                            //.ToList();
                                         .Select(e => new
                                         {

                                             SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(t => (t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN") &&
                                                 t.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                         })
                                            .ToList();
                                        if (OEMPLoan != null && OEMPLoan.Count() > 0) //Changes for Null
                                        {


                                            List<ArrJVProcessData> OArrJVProcessDataEMPLoan = OEMPLoan
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    // ArrTransactionAmount = e.SalDetails.Sum(r => r.SalHeadAmount).ToString("0.00"),
                                                    // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),

                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Company Loan" + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEMPLoan);
                                        }
                                        break;
                                    // Added by Rekha 25072017
                                    case "INSURANCE":
                                        var OSalaryTInsurance = OSalaryTF.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //var OSalaryTLoan = OEmployeePayroll.Select(s => new { Osal = s.LoanAdvRequest.Where(e => e.LoanAdvanceHead.SalaryHead.Id == _SalaryHeadData.Id && (e.CloserDate == null || e.CloserDate >= Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).Date)) }).ToList();
                                        var OSalInsurance = OSalaryTInsurance
                                               .SelectMany(e => e.Osal).ToList();
                                        var OEMPInsurance = OSalInsurance.Where(e => e.SalaryArrearPaymentT != null && e.SalaryArrearPaymentT.Count > 0).GroupBy(e => e.GeoStruct.Company.Id)
                                         .Select(e => new
                                         {

                                             SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(t => (t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "INSURANCE") &&
                                                 t.SalaryHead.Id == _SalaryHeadData._Id)).ToList()
                                         })
                                            .ToList();
                                        if (OEMPInsurance != null && OEMPInsurance.Count() > 0) //Changes for Null
                                        {


                                            List<ArrJVProcessData> OArrJVProcessDataEMPInsurance = OEMPInsurance
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(_Find_jv_Data.Id),
                                                    //ArrBranchCode = _Find_jv_Data.ArrCreditDebitBranchCode,
                                                    // ArrAccountProductCode = _Find_jv_Data.JVProductCode,
                                                    //ArrAccountCustomerNo = "",
                                                    ArrAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    ArrSubAccountCode = _Find_jv_Data.ArrSubAccountNo,
                                                    // ArrTransactionAmount = e.SalDetails.Sum(r => r.SalHeadAmount).ToString("0.00"),
                                                    // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),

                                                    ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.SalDetails.Sum(a => a.SalHeadAmount)).ToString(),
                                                    ArrCreditDebitFlag = _Find_jv_Data.ArrCreditDebitFlag,
                                                    ArrNarration = "Employees " + _SalaryHeadData._Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == _SalaryHeadData._Id).SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEMPInsurance);
                                        }
                                        break;
                                    default:

                                        break;
                                }
                            }
                            #endregion
                        }
                    }

                }
                else
                {
                    //normal
                    foreach (var ca in OJVParameter)
                    {

                        //Individual JV Vouchers
                        if (ca.JVGroup.LookupVal.ToUpper() == "INDIVIDUAL")
                        {
                            foreach (var ca1 in ca.ArrSalaryHead)
                            {
                                switch (ca1.SalHeadOperationType.LookupVal.ToUpper())
                                {
                                    //VR
                                    case "NONREGULAR":
                                    case "REGULAR":
                                        //SalaryArrearPaymentT
                                        var JvDataPerticular = db.ArrJVParameter.Include(q => q.ArrJVNonStandardEmp)
                                           .Include(q => q.ArrJVNonStandardEmp.Select(a => a.Branch))
                                           .Include(q => q.ArrJVNonStandardEmp.Select(a => a.EmployeePayroll))
                                           .Where(a => a.Id == ca.Id).SingleOrDefault();

                                        List<EmployeePayroll> OSalaryTFNonreg = new List<EmployeePayroll>();

                                        var EmpOSalaryTNonreg = JvDataPerticular.ArrJVNonStandardEmp.Select(q => q.EmployeePayroll.Id).ToList();
                                        if (EmpOSalaryTNonreg.Count > 0)
                                        {
                                            for (int i = 0; i < EmpOSalaryTNonreg.Count; i++)
                                            {
                                                var EmployeePayroll = _returnEmpArrYearly(EmpOSalaryTNonreg[i], mPayMonth);
                                                if (EmployeePayroll != null)
                                                {
                                                    OSalaryTFNonreg.Add(EmployeePayroll);
                                                }
                                            }
                                        }
                                        var OSalaryT1Nonreg = OSalaryTFNonreg.Select(e => new
                                        {
                                            ArrBranchCode = JvDataPerticular.ArrJVNonStandardEmp.Where(q => q.EmployeePayroll.Id == e.Id).Select(r => r.Branch.Code).SingleOrDefault(),
                                            ArrSubAccountNo = JvDataPerticular.ArrJVNonStandardEmp.Where(q => q.EmployeePayroll.Id == e.Id).Select(r => r.ArrSubAccountNo).SingleOrDefault(),
                                            Osal = e.SalaryArrearT != null ? e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).SelectMany(a => a.SalaryArrearPaymentT).ToList() : null,
                                            //  SalaryArrearPFT = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).Select(a => a.SalaryArrearPFT).ToList(),
                                        }).ToList();
                                        List<ArrJVProcessData> OArrJVProcessDataNonreg = new List<ArrJVProcessData>();
                                        foreach (var item in OSalaryT1Nonreg)
                                        {
                                            var ArrTransactionAmount = item.Osal.Where(q => q.SalaryHead.Code == ca1.Code).Select(a => a.SalHeadAmount.ToString("0.00")).SingleOrDefault();
                                            SalaryHead SalHead = item.Osal.Where(q => q.SalaryHead.Code == ca1.Code).Select(q => q.SalaryHead).SingleOrDefault();
                                            OArrJVProcessDataNonreg.Add(new ArrJVProcessData
                                            {
                                                ArrBatchName = mBatchName,
                                                ArrProcessMonth = mPayMonth,
                                                ArrProcessDate = DateTime.Now.Date,
                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                ArrBranchCode = item.ArrBranchCode != null ? item.ArrBranchCode : "",
                                                ArrAccountCustomerNo = "",
                                                ArrAccountCode = item.ArrSubAccountNo != null ? item.ArrSubAccountNo.ToString() : null,
                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                ArrTransactionAmount = ArrTransactionAmount,
                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                ArrNarration = ca1.Code + " for Month :" + mPayMonth,
                                                ArrSalaryHead = SalHead
                                            });
                                        }

                                        //List<ArrJVProcessData> OArrJVProcessDataNetNonrreg = OSalaryT1Nonreg
                                        //    .Select(e => new ArrJVProcessData
                                        //    {
                                        //        ArrBatchName = mBatchName,
                                        //        ArrProcessMonth = mPayMonth,
                                        //        ArrProcessDate = DateTime.Now.Date,
                                        //        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //        ArrBranchCode = e.ArrBranchCode != null ? e.ArrBranchCode : "",
                                        //        //ccountProductCode = e.AccType != null ? e.AccType.LookupVal.ToUpper() : "",
                                        //        ArrAccountProductCode = ca.ArrJVProductCode,
                                        //        ArrAccountCustomerNo = "",
                                        //        ArrAccountCode = e.ArrSubAccountNo,
                                        //        ArrSubAccountCode = "",
                                        //        ArrTransactionAmount = Osal.SalaryArrearPFT.Sum(r => r.EmpPF)
                                        //        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //        ArrNarration = ca1.Code + " For Month :" + mPayMonth,
                                        //    }).ToList();

                                        OArrJVProcessData.AddRange(OArrJVProcessDataNonreg);
                                        break;

                                    case "EPF":
                                        var OSalaryT1 = OEmployeePayroll.Select(e => new
                                        {
                                            PFNo = e.Employee.EmpOffInfo.NationalityID.PFNo,
                                            AccountNo = e.Employee.EmpOffInfo.AccountNo,
                                            Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList(),
                                            SalaryArrearPFT = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).Select(a => a.SalaryArrearPFT).ToList(),
                                        }).ToList();
                                        List<ArrJVProcessData> OArrJVProcessDataEPF = new List<ArrJVProcessData>();
                                        foreach (var item in OSalaryT1)
                                        {
                                            if (item.SalaryArrearPFT != null && item.PFNo != null)
                                            {
                                                OArrJVProcessDataEPF.Add(new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                    ArrBranchCode = item.AccountNo,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = item.PFNo != null ? item.PFNo.ToString() : null,
                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                    ArrTransactionAmount = (item.SalaryArrearPFT.Sum(r => r.EmpPF)).ToString("0.00"),
                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                    ArrNarration = "Employee Share Provident Fund including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                });
                                            }
                                        }
                                        OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                        break;
                                    case "CPF":
                                        var OSalaryT2 = OEmployeePayroll.Select(e => new
                                        {
                                            PFNo = e.Employee.EmpOffInfo.NationalityID.PFNo,
                                            Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList(),
                                            SalaryArrearPFT = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).Select(a => a.SalaryArrearPFT).ToList(),
                                        }).ToList();

                                        List<ArrJVProcessData> OArrJVProcessDataEPF_OSalaryT1 = new List<ArrJVProcessData>();
                                        foreach (var item in OSalaryT2)
                                        {
                                            if (item.SalaryArrearPFT != null && item.PFNo != null)
                                            {
                                                OArrJVProcessDataEPF_OSalaryT1.Add(new ArrJVProcessData
                                                {

                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                    ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = item.PFNo.ToString(),
                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                    ArrTransactionAmount = (item.SalaryArrearPFT.Sum(r => r.CompPF)).ToString("0.00"),
                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                    ArrNarration = "Company Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                });
                                            }

                                        }
                                        OArrJVProcessData.AddRange(OArrJVProcessDataEPF_OSalaryT1);
                                        break;
                                    case "NET":
                                        if (OSalaryT != null && OSalaryT.Count() > 0)
                                        {
                                            if (_CompCode == _CustomeCompCode)
                                            {
                                                var OSal = OSalaryT.Select(e => e.Osal).ToList();
                                                foreach (var item in OSal)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataNet = item
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = e.EmployeePayroll.Employee.EmpOffInfo.Branch.Code.ToString(),
                                                            ArrAccountProductCode = e.EmployeePayroll.Employee.EmpOffInfo.AccountType.ToString(),
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = e.EmployeePayroll.Employee.EmpOffInfo.AccountNo.ToString(),
                                                            ArrSubAccountCode = "",
                                                            ArrTransactionAmount =  PayrollReportGen._returnTransctionAmt(e.ArrTotalNet).ToString(),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Net Salary For Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "NET").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                }
                                            }
                                            else
                                            {
                                                var OSal = OSalaryT.Select(e => e.Osal).ToList();
                                                foreach (var item in OSal)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataNet = item
                                                        .GroupBy(q => q.PayMonth)
                                                      .Select(e => new ArrJVProcessData
                                                      {
                                                          ArrBatchName = mBatchName,
                                                          ArrProcessMonth = mPayMonth,
                                                          ArrProcessDate = DateTime.Now.Date,
                                                          ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                          ArrBranchCode = e.Select(q => q.EmployeePayroll.Employee.EmpOffInfo.Branch.Code.ToString()).First(),
                                                          ArrAccountProductCode = e.Select(q => q.EmployeePayroll.Employee.EmpOffInfo.AccountType.LookupVal.ToString()).First(),// e.EmployeePayroll.Employee.EmpOffInfo.AccountType.LookupVal.ToString(),
                                                          ArrAccountCustomerNo = "",
                                                          ArrAccountCode = e.Select(q => q.EmployeePayroll.Employee.EmpOffInfo.AccountNo.ToString()).First(), //e.EmployeePayroll.Employee.EmpOffInfo.AccountNo.ToString(),
                                                          ArrSubAccountCode = "",
                                                          ArrTransactionAmount = e.Sum(r => r.ArrTotalNet).ToString("0.00"),
                                                          ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                          ArrNarration = "Net Salary for Month :" + mPayMonth,
                                                          ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "NET").SingleOrDefault()

                                                      }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                }
                                            }
                                        }
                                        break;
                                    case "LVENCASH":
                                        if (OYearlyT != null && OYearlyT.Count() > 0)
                                        {
                                            var OSal = OYearlyT.Select(e => new
                                            {
                                                YearPay = e.Osal
                                                    .Where(r => r.SalaryHead.Id == ca1.Id),
                                                AccNo = e.AccNo,
                                                AccType = e.AccType,
                                                Branch = e.Branch
                                            })
                                                .ToList();
                                            if (OSal != null && OSal.Count() > 0)
                                            {

                                                List<ArrJVProcessData> OArrJVProcessDataYear = OSal.Where(r => r.YearPay.Count() > 0)
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = e.Branch,
                                                        ArrAccountProductCode = e.AccType.LookupVal.ToUpper(),
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = e.AccNo,
                                                        ArrSubAccountCode = "",
                                                        ArrTransactionAmount = e.YearPay.Sum(r => r.AmountPaid).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Yearly Payment for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataYear);
                                            }
                                        }
                                        break;
                                    case "LTA":
                                        if (OYearlyT != null && OYearlyT.Count() > 0)
                                        {
                                            var OSal = OYearlyT.Select(e => new
                                            {
                                                YearPay = e.Osal
                                                    .Where(r => r.SalaryHead.Id == ca1.Id),
                                                AccNo = e.AccNo,
                                                AccType = e.AccType,
                                                Branch = e.Branch
                                            })
                                                .ToList();
                                            if (OSal != null && OSal.Count() > 0)
                                            {

                                                List<ArrJVProcessData> OArrJVProcessDataYear = OSal.Where(r => r.YearPay.Count() > 0)
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = e.Branch,
                                                        ArrAccountProductCode = e.AccType.LookupVal.ToUpper(),
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = e.AccNo,
                                                        ArrSubAccountCode = "",
                                                        ArrTransactionAmount = e.YearPay.Sum(r => r.AmountPaid).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Yearly Payment for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LTA").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataYear);
                                            }
                                        }
                                        break;
                                    case "MEDALLOW":
                                        if (OYearlyT != null && OYearlyT.Count() > 0)
                                        {
                                            var OSal = OYearlyT.Select(e => new
                                            {
                                                YearPay = e.Osal
                                                    .Where(r => r.SalaryHead.Id == ca1.Id),
                                                AccNo = e.AccNo,
                                                AccType = e.AccType,
                                                Branch = e.Branch
                                            })
                                                .ToList();
                                            if (OSal != null && OSal.Count() > 0)
                                            {

                                                List<ArrJVProcessData> OArrJVProcessDataYear = OSal.Where(r => r.YearPay.Count() > 0)
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = e.Branch,
                                                        ArrAccountProductCode = e.AccType.LookupVal.ToUpper(),
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = e.AccNo,
                                                        ArrSubAccountCode = "",
                                                        ArrTransactionAmount = e.YearPay.Sum(r => r.AmountPaid).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Yearly Payment for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "MEDALLOW").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataYear);
                                            }
                                        }
                                        break;
                                    case "LTC":
                                        if (OYearlyT != null && OYearlyT.Count() > 0)
                                        {
                                            var OSal = OYearlyT.Select(e => new
                                            {
                                                YearPay = e.Osal
                                                    .Where(r => r.SalaryHead.Id == ca1.Id),
                                                AccNo = e.AccNo,
                                                AccType = e.AccType,
                                                Branch = e.Branch
                                            })
                                                .ToList();
                                            if (OSal != null && OSal.Count() > 0)
                                            {

                                                List<ArrJVProcessData> OArrJVProcessDataYear = OSal.Where(r => r.YearPay.Count() > 0)
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = e.Branch,
                                                        ArrAccountProductCode = e.AccType.LookupVal.ToUpper(),
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = e.AccNo,
                                                        ArrSubAccountCode = "",
                                                        ArrTransactionAmount = e.YearPay.Sum(r => r.AmountPaid).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Yearly Payment for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LTC").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataYear);
                                            }
                                        }
                                        break;
                                    case "GRATUITY":
                                        if (OYearlyT != null && OYearlyT.Count() > 0)
                                        {
                                            var OSal = OYearlyT.Select(e => new
                                            {
                                                YearPay = e.Osal
                                                    .Where(r => r.SalaryHead.Id == ca1.Id),
                                                AccNo = e.AccNo,
                                                AccType = e.AccType,
                                                Branch = e.Branch
                                            })
                                                .ToList();
                                            if (OSal != null && OSal.Count() > 0)
                                            {

                                                List<ArrJVProcessData> OArrJVProcessDataYear = OSal.Where(r => r.YearPay.Count() > 0)
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = e.Branch,
                                                        ArrAccountProductCode = e.AccType.LookupVal.ToUpper(),
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = e.AccNo,
                                                        ArrSubAccountCode = "",
                                                        ArrTransactionAmount = e.YearPay.Sum(r => r.AmountPaid).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Yearly Payment for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GRATUITY").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataYear);
                                            }
                                        }
                                        break;
                                    case "BONUS":
                                        if (OYearlyT != null && OYearlyT.Count() > 0)
                                        {
                                            var OSal = OYearlyT.Select(e => new
                                            {
                                                YearPay = e.Osal
                                                    .Where(r => r.SalaryHead.Id == ca1.Id),
                                                AccNo = e.AccNo,
                                                AccType = e.AccType,
                                                Branch = e.Branch
                                            })
                                                .ToList();
                                            if (OSal != null && OSal.Count() > 0)
                                            {

                                                List<ArrJVProcessData> OArrJVProcessDataYear = OSal.Where(r => r.YearPay.Count() > 0)
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = e.Branch,
                                                        ArrAccountProductCode = e.AccType.LookupVal.ToUpper(),
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = e.AccNo,
                                                        ArrSubAccountCode = "",
                                                        ArrTransactionAmount = e.YearPay.Sum(r => r.AmountPaid).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Yearly Payment for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "BONUS").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataYear);
                                            }
                                        }
                                        break;
                                    case "LOAN":
                                        var OLoanReq = OEmployeePayroll.Select(a => a.LoanAdvRequest).Count() > 0 ? OEmployeePayroll.Select(s =>
                                            s.LoanAdvRequest.Where(e => e.LoanAdvanceHead.SalaryHead != null && e.LoanAdvanceHead.SalaryHead.Id == ca1.Id &&
                                            (e.CloserDate == null || e.CloserDate >= Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).Date))).ToList() : null;

                                        if (OLoanReq != null && OLoanReq.Count() > 0)
                                        {
                                            foreach (var ca6 in OLoanReq.ToList())
                                            {

                                                if (ca6 != null && ca6.Count() > 0)
                                                {
                                                    foreach (var ca5 in ca6.ToList())
                                                    {
                                                        var OLoanData = ca5.LoanAdvRepaymentT.Count > 0 ? ca5.LoanAdvRepaymentT.Where(r => r.PayMonth == mPayMonth && r.RepaymentDate != null).ToList() : null;
                                                        if (OLoanData != null && OLoanData.Count() > 0)
                                                        {
                                                            List<ArrJVProcessData> OArrJVProcessDataLoan = OLoanData
                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                ArrBranchCode = ca5.LoanAccBranch != null ? ca5.LoanAccBranch.LocationObj.LocCode : null,
                                                                ArrAccountProductCode = ca5.LoanAdvanceHead.Code,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca5.LoanAccNo != null ? ca5.LoanAccNo : "",
                                                                ArrSubAccountCode = ca5.LoanSubAccNo == null ? "" : ca5.LoanSubAccNo,
                                                                ArrTransactionAmount = e.InstallmentPaid.ToString("0.00"),
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = "Loan" + ca5.LoanAdvanceHead.Code + " for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca5.LoanAdvanceHead.SalaryHead.Id).SingleOrDefault()
                                                            }).ToList();

                                                            OArrJVProcessData.AddRange(OArrJVProcessDataLoan);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        //Location JV Vouchers
                        #region location
                        if (ca.JVGroup.LookupVal.ToUpper() == "LOCATION")
                        {
                            //jobposition wise grouping
                            if (ca.ArrJobPosition != null)
                            {
                                foreach (var ca1 in ca.ArrSalaryHead)
                                {
                                    switch (ca1.SalHeadOperationType.LookupVal.ToUpper())
                                    {

                                        case "GROSS":

                                            if (OSalaryT != null && OSalaryT.Count() > 0)
                                            {
                                                if (ca.ArrIrregular == true)
                                                {
                                                    var OSal = OSalaryT
                                                        .Select(e => e.Osal).ToList();
                                                    foreach (var item in OSal)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNet = item
                                                        .Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                        .GroupBy(a => new { a = a.GeoStruct.Location.LocationObj.Id, b = a.FuncStruct.JobPosition.Id })
                                                           .Select(e => new ArrJVProcessData
                                                           {
                                                               ArrBatchName = mBatchName,
                                                               ArrProcessMonth = mPayMonth,
                                                               ArrProcessDate = DateTime.Now.Date,
                                                               ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                               //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                               ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                                               ArrAccountProductCode = ca.ArrJVProductCode,
                                                               ArrAccountCustomerNo = "",
                                                               ArrAccountCode = ca.ArrAccountNo,
                                                               ArrSubAccountCode = ca.ArrSubAccountNo,
                                                               ArrTransactionAmount = e.Sum(r => r.ArrTotalEarning).ToString("0.00"),
                                                               ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                               ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                               ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                                           }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                    }
                                                }
                                                else
                                                {
                                                    var OSal = OSalaryT
                                                        .Select(e => e.Osal).ToList();
                                                    foreach (var item in OSal)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNet = item
                                                          .Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                          .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.Id, FuncStruct_Id = a.FuncStruct.JobPosition.Id })
                                                            //.GroupBy(t => t.GeoStruct.Location)

                                                          .Select(e => new ArrJVProcessData
                                                          {
                                                              ArrBatchName = mBatchName,
                                                              ArrProcessMonth = mPayMonth,
                                                              ArrProcessDate = DateTime.Now.Date,
                                                              ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                              ArrBranchCode = db.Location.Where(a => a.Id == e.Key.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                                              ArrAccountProductCode = ca.ArrJVProductCode,
                                                              ArrAccountCustomerNo = "",
                                                              ArrAccountCode = ca.ArrAccountNo,
                                                              ArrSubAccountCode = ca.ArrSubAccountNo,
                                                              ArrTransactionAmount = e.Sum(r => r.ArrTotalEarning).ToString("0.00"),
                                                              ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                              ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                              ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                                          }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                    }
                                                }
                                            }
                                            //Emp_Monthsal;
                                            break;
                                        case "REGULAR":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSal = OSalaryT
                                                    .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal)
                                                {
                                                    var OGrp = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                     .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })

                                                  .Select(e => new
                                                  {
                                                      location = e.Key,
                                                      SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList()
                                                  }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataNet = OGrp
                                                       .Select(e => new ArrJVProcessData
                                                       {
                                                           ArrBatchName = mBatchName,
                                                           ArrProcessMonth = mPayMonth,
                                                           ArrProcessDate = DateTime.Now.Date,
                                                           ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                           //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                           ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                                           ArrAccountProductCode = ca.ArrJVProductCode,
                                                           ArrAccountCustomerNo = "",
                                                           ArrAccountCode = ca.ArrAccountNo,
                                                           ArrSubAccountCode = ca.ArrSubAccountNo,
                                                           ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                           ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                           ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                           ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                       }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataNet);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FirstOrDefault().FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                            .GroupBy(a => new { Loc_Id = a.FirstOrDefault().GeoStruct.Location.LocationObj.Id, Func_Id = a.FirstOrDefault().FuncStruct.JobPosition.Id })
                                                            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.Select(t => t.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                        if (OGrpArr != null && OGrpArr.Count() > 0)
                                                        {
                                                            List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                .Select(e => new ArrJVProcessData
                                                                {
                                                                    ArrBatchName = mBatchName,
                                                                    ArrProcessMonth = mPayMonth,
                                                                    ArrProcessDate = DateTime.Now.Date,
                                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                    //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                                    ArrAccountCustomerNo = "",
                                                                    ArrAccountCode = ca.ArrAccountNo,
                                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                    ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.Sum(t => t.SalHeadAmount)))).ToString("0.00"),
                                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                    ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                }).ToList();

                                                            OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);

                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var OSal1 = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal1)
                                                {
                                                    var OGrp = item
                                                     .Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                     .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                                     .Select(e => new
                                                     {
                                                         location = e.Key,
                                                         SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList()
                                                     }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataRegular = OGrp

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                        }).ToList();
                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegular);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).Where(r => r.FirstOrDefault().FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).ToList();
                                                        var OGrpArr = OSal2.GroupBy(a => new { Loc_Id = a.FirstOrDefault().GeoStruct.Location.Id, Func_Id = a.FirstOrDefault().FuncStruct.JobPosition.Id }).Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                        if (OGrpArr != null && OGrpArr.Count() > 0)
                                                        {
                                                            List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                .Select(e => new ArrJVProcessData
                                                                {
                                                                    ArrBatchName = mBatchName,
                                                                    ArrProcessMonth = mPayMonth,
                                                                    ArrProcessDate = DateTime.Now.Date,
                                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                    ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                                    ArrAccountCustomerNo = "",
                                                                    ArrAccountCode = ca.ArrAccountNo,
                                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                    //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                    ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                    ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                }).ToList();

                                                            OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                        }
                                                    }
                                                }
                                            }

                                            break;
                                        case "BASIC":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalBASIC = OSalaryT
                                                      .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalBASIC)
                                                {
                                                    var OGrpBASIC = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                     .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                                     .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    foreach (var ca10 in OGrpBASIC)
                                                    {
                                                        ArrJVProcessData OArrJVProcessDataBASIC = new ArrJVProcessData()
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //      ArrBranchCode = ca10.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = ca10.SalDetails.Select(a => a.Sum(d => d.SalHeadAmount)).FirstOrDefault().ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").SingleOrDefault()
                                                        };
                                                        OArrJVProcessData.Add(OArrJVProcessDataBASIC);
                                                    }

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {

                                                            var OGrpArr = OSal2.Where(x => x.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && x.FirstOrDefault().FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                                .GroupBy(a => new { Loc_Id = a.FirstOrDefault().GeoStruct.Location.LocationObj.Id, Func_Id = a.FirstOrDefault().FuncStruct.JobPosition.Id }).
                                                                Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),

                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var OSalBASIC = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalBASIC)
                                                {
                                                    var OGrpBASIC = item.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                     .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                                     .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    foreach (var ca10 in OGrpBASIC)
                                                    {
                                                        ArrJVProcessData OArrJVProcessDataBASIC = new ArrJVProcessData()
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == ca10.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = ca10.SalDetails.Select(a => a.Sum(d => d.SalHeadAmount)).FirstOrDefault().ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").SingleOrDefault()
                                                        };
                                                        OArrJVProcessData.Add(OArrJVProcessDataBASIC);
                                                    }

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {

                                                            var OGrpArr = OSal2.Where(r => r.FirstOrDefault().FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(t => new
                                                            {
                                                                Loc_Id = t.FirstOrDefault().GeoStruct.Location.Id,
                                                                Func_Id = t.FirstOrDefault().FuncStruct.JobPosition.Id
                                                            })
                                                                .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        case "VDA":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalVDA = OSalaryT
                                                          .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalVDA)
                                                {
                                                    var OGrpVDA = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                      .GroupBy(a => new
                                                      {
                                                          Loc_Id = a.GeoStruct.Location.LocationObj.Id,
                                                          Func_Id = a.FuncStruct.JobPosition.Id
                                                      }).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                        }).ToList();
                                                    OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {
                                                            var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FirstOrDefault().FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                                .GroupBy(a => new { Loc_Id = a.FirstOrDefault().GeoStruct.Location.Id, Func_Id = a.FirstOrDefault().FuncStruct.JobPosition.Id })
                                                                .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        //   ArrBranchCode = e.location.LocationObj.LocCode,
                                                                        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var OSalVDA = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalVDA)
                                                {
                                                    var OGrpVDA = item.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(t => new { Loc_Id = t.GeoStruct.Location.Id, Func_Id = t.FuncStruct.JobPosition.Id })
                                                      .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                        }).ToList();
                                                    OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {
                                                            var OGrpArr = OSal2.Where(r => r.FirstOrDefault().FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(t => new { Loc_Id = t.FirstOrDefault().GeoStruct.Location.Id, Func_Id = t.FirstOrDefault().FuncStruct.JobPosition.Id })
                                                                .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case "NONREGULAR":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSal3 = OSalaryT
                                                .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal3)
                                                {
                                                    var OGrp1 = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                      .GroupBy(a => new
                                                      {
                                                          Loc_Id = a.GeoStruct.Location.LocationObj.Id,
                                                          Func_Id = a.FuncStruct.JobPosition.Id
                                                      })
                                                      .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    if (OGrp1 != null && OGrp1.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca.ArrAccountNo,
                                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                                    }
                                                }
                                            }
                                            else
                                            {


                                                var OSal3 = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal3)
                                                {
                                                    var OGrp1 = item.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(t => new
                                                    {
                                                        Loc_Id = t.GeoStruct.Location.Id,
                                                        Func_Id = t.FuncStruct.JobPosition.Id
                                                    }).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    if (OGrp1 != null && OGrp1.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca.ArrAccountNo,
                                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                                    }
                                                }
                                            }
                                            break;
                                        case "EPF":
                                            if (ca.ArrIrregular == true)
                                            {

                                                var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal4 = OSalaryT1
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEPF = OSal4.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                    .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() }).ToList();

                                                if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                                {


                                                    List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            // ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employee Share Provident Fund including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                                }
                                            }
                                            else
                                            {


                                                var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal4 = OSalaryT1
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEPF = OSal4.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() }).ToList();

                                                if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                                {


                                                    List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employee Share Provident Fund including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                                }
                                            }
                                            break;

                                        case "CPF":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal5 = OSalaryT2
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OCPF = OSal5.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                    .GroupBy(a => new
                                                    {
                                                        Loc_Id = a.GeoStruct.Location.LocationObj.Id,
                                                        Func_Id = a.FuncStruct.JobPosition.Id
                                                    })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();

                                                if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //   ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            //ask  ArrTransactionAmount = (e.SalDetails.Sum(r => r.) ).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Company Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                                }
                                            }
                                            else
                                            {
                                                var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal5 = OSalaryT2
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OCPF = OSal5.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new
                                                {
                                                    Loc_Id = e.GeoStruct.Location.Id,
                                                    Func_Id = e.FuncStruct.JobPosition.Id
                                                })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();

                                                if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Include(r => r.LocationObj).Where(r => r.Id == e.location.Loc_Id).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.CompPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Company Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                                }
                                            }
                                            break;
                                        case "PENSION":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal6 = OSalaryT3
                                                       .SelectMany(e => e.Osal).ToList();

                                                var OEPS = OSal6.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                    .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();
                                                if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //   ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpEPS)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Pension Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                                }
                                            }
                                            else
                                            {


                                                var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal6 = OSalaryT3
                                                       .SelectMany(e => e.Osal).ToList();

                                                var OEPS = OSal6.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();
                                                if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpEPS)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Pension Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                                }
                                            }
                                            break;

                                        //case "LWF":
                                        //    if (ca.ArrIrregular == true)
                                        //    {

                                        //        var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalEMPLWF = OSalaryTEMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPEMPLWF = OSalEMPLWF.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                        //            .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPEMPLWF != null && OEMPEMPLWF.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalEMPLWF = OSalaryTEMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPEMPLWF = OSalEMPLWF.Where(r =>r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPEMPLWF != null && OEMPEMPLWF.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "COMPLWF":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPLWF = OSalaryTCOMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPLWF = OSalCOMPLWF.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                        //            .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                        //        {

                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    // ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPLWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                        //        }
                                        //    }
                                        //    else
                                        //    {


                                        //        var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPLWF = OSalaryTCOMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPLWF = OSalCOMPLWF.Where(r =>r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                        //        {

                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPLWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "COMPESIC":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPESIS = OSalaryTCOMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPESIS = OSalCOMPESIS.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                        //            .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //   ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employer Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                        //        }
                                        //    }
                                        //    else
                                        //    {


                                        //        var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPESIS = OSalaryTCOMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPESIS = OSalCOMPESIS.Where(r =>r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employer Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "ESIC":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPESIS = OSalCOMPEMPESIS.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                        //            .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //       ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                        //        }
                                        //    }
                                        //    else
                                        //    {


                                        //        var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPESIS = OSalCOMPEMPESIS.Where(r =>r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "PTAX":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalPTAX = OSalaryTPTAX
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPPTAX = OSalPTAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                        //            .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.PTaxTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //     ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = (e.SalDetails.Sum(r => r.PTAmount) + e.SalDetails.Sum(r => r.ArrearPTAmount)).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share PTAX including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalPTAX = OSalaryTPTAX
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPPTAX = OSalPTAX.Where(r =>r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.PTaxTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = (e.SalDetails.Sum(r => r.PTAmount) + e.SalDetails.Sum(r => r.ArrearPTAmount)).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share PTAX including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                        //        }
                                        //    }
                                        //    break;
                                        case "ITAX":
                                            if (ca.ArrIrregular == true)
                                            {

                                                var OSalaryTITAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSalITAX = OSalaryTITAX
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEMPITAX = OSalITAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn && r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id)
                                                    .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.LocationObj.Id, Func_Id = a.FuncStruct.JobPosition.Id })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList() })
                                                    .ToList();
                                                if (OEMPITAX != null && OEMPITAX.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OEMPITAX
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            // ArrTransactionAmount = e.SalDetails.Sum(r => r.Amoount).ToString("0.00"),
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employees ITAX" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                                }
                                            }
                                            else
                                            {


                                                var OSalaryTITAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSalITAX = OSalaryTITAX
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEMPITAX = OSalITAX.Where(r => r.FuncStruct.JobPosition.Id == ca.ArrJobPosition.Id).GroupBy(e => new { Loc_Id = e.GeoStruct.Location.Id, Func_Id = e.FuncStruct.JobPosition.Id })
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList() })
                                                    .ToList();
                                                if (OEMPITAX != null && OEMPITAX.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OEMPITAX
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Where(a => a.Id == e.location.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            // ArrTransactionAmount = e.SalDetails.Sum(r => r.Amoount).ToString("0.00"),
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employees ITAX" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                                }
                                            }
                                            break;
                                        default:

                                            break;
                                    }

                                }
                            }
                            //jobposition wise grouping end

                            //gradewise grouping
                            else if (ca.ArrPayStruct != null)
                            {
                            }
                            //gradewise grouping end
                            //locationwise grouping
                            else if (ca.ArrJobPosition == null && ca.ArrPayStruct == null)
                            {
                                foreach (var ca1 in ca.ArrSalaryHead)
                                {
                                    switch (ca1.SalHeadOperationType.LookupVal.ToUpper())
                                    {

                                        case "GROSS":
                                            //if Location

                                            if (OSalaryT != null && OSalaryT.Count() > 0)
                                            {
                                                if (ca.ArrIrregular == true)
                                                {

                                                    var OSal = OSalaryT
                                                        .Select(e => e.Osal).ToList();
                                                    foreach (var item in OSal)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNet = item
                                                                   .Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn
                                                                       ).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                                      .Select(e => new ArrJVProcessData
                                                                      {
                                                                          ArrBatchName = mBatchName,
                                                                          ArrProcessMonth = mPayMonth,
                                                                          ArrProcessDate = DateTime.Now.Date,
                                                                          ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                          //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                                          ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                                                          ArrAccountProductCode = ca.ArrJVProductCode,
                                                                          ArrAccountCustomerNo = "",
                                                                          ArrAccountCode = ca.ArrAccountNo,
                                                                          ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                          ArrTransactionAmount = e.Sum(r => r.ArrTotalEarning).ToString("0.00"),
                                                                          ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                          ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                                          ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                                                      }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                    }
                                                }
                                                else
                                                {
                                                    var OSal = OSalaryT
                                                        .Select(e => e.Osal).ToList();

                                                    foreach (var item in OSal)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNet = item
                                                                   .GroupBy(a => new { Loc_Id = a.GeoStruct.Location.Id })

                                                                   .Select(e => new ArrJVProcessData
                                                                   {
                                                                       ArrBatchName = mBatchName,
                                                                       ArrProcessMonth = mPayMonth,
                                                                       ArrProcessDate = DateTime.Now.Date,
                                                                       ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                       // ArrBranchCode = e.Key.LocationObj.LocCode,
                                                                       ArrBranchCode = db.Location.Where(a => a.Id == e.Key.Loc_Id).Include(a => a.LocationObj).SingleOrDefault().LocationObj.LocCode,
                                                                       // ArrBranchCode =  db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                                       ArrAccountProductCode = ca.ArrJVProductCode,
                                                                       ArrAccountCustomerNo = "",
                                                                       ArrAccountCode = ca.ArrAccountNo,
                                                                       ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                       ArrTransactionAmount = e.Sum(r => r.ArrTotalEarning).ToString("0.00"),
                                                                       ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                       ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                                       ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                                                   }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                    }
                                                }
                                            }
                                            //Emp_Monthsal;
                                            break;
                                        case "REGULAR":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSal = OSalaryT
                                                    .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal)
                                                {
                                                    var OGrp = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                             .Select(e => new
                                             {
                                                 location = e.Key,
                                                 SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList()
                                             }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataNet = OGrp
                                                       .Select(e => new ArrJVProcessData
                                                       {
                                                           ArrBatchName = mBatchName,
                                                           ArrProcessMonth = mPayMonth,
                                                           ArrProcessDate = DateTime.Now.Date,
                                                           ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                           //ArrBranchCode = e.Key.LocationObj.LocCode,
                                                           ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                                           ArrAccountProductCode = ca.ArrJVProductCode,
                                                           ArrAccountCustomerNo = "",
                                                           ArrAccountCode = ca.ArrAccountNo,
                                                           ArrSubAccountCode = ca.ArrSubAccountNo,
                                                           ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                           ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                           ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                           ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                       }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataNet);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id)
                                                            .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                        if (OGrpArr != null && OGrpArr.Count() > 0)
                                                        {
                                                            List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                .Select(e => new ArrJVProcessData
                                                                {
                                                                    ArrBatchName = mBatchName,
                                                                    ArrProcessMonth = mPayMonth,
                                                                    ArrProcessDate = DateTime.Now.Date,
                                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                    //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                                    ArrAccountCustomerNo = "",
                                                                    ArrAccountCode = ca.ArrAccountNo,
                                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                    ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                    ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                }).ToList();

                                                            OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var OSal1 = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                //foreach (var item in OSal1)
                                                //   {
                                                var OGrpArr = OSal1.GroupBy(t => t.FirstOrDefault().GeoStruct.Location.Id).Select(e => new
                                                {
                                                    location = e.Key,
                                                    SalDetails = e.SelectMany(r => r
                                                        .Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))
                                                    ).ToList()
                                                }).ToList();

                                                List<ArrJVProcessData> OArrJVProcessDataRegular = OGrpArr

                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        //ArrBranchCode = e.location.LocationObj.LocCode,
                                                        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = ca.ArrAccountNo,
                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                        // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                    }).ToList();
                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegular);

                                                //if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                //{
                                                //    var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                //       .Select(e => e.Osal).ToList();
                                                //    var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Location.Id).Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                //    if (OGrpArr != null && OGrpArr.Count() > 0)
                                                //    {
                                                //        List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                //            .Select(e => new ArrJVProcessData
                                                //            {
                                                //                ArrBatchName = mBatchName,
                                                //                ArrProcessMonth = mPayMonth,
                                                //                ArrProcessDate = DateTime.Now.Date,
                                                //                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                //                //ArrBranchCode = e.location.LocationObj.LocCode,
                                                //                ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                //                ArrAccountProductCode = ca.ArrJVProductCode,
                                                //                ArrAccountCustomerNo = "",
                                                //                ArrAccountCode = ca.ArrSubAccountNo,
                                                //                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                //                // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                //                ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                //                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                //                ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                //                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                //            }).ToList();

                                                //        OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                //    //}
                                                //    }
                                                //}
                                            }

                                            break;
                                        case "BASIC":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalBASIC = OSalaryT
                                                      .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalBASIC)
                                                {
                                                    var OGrpBASIC = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                      .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    foreach (var ca10 in OGrpBASIC)
                                                    {
                                                        ArrJVProcessData OArrJVProcessDataBASIC = new ArrJVProcessData()
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //      ArrBranchCode = ca10.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = ca10.SalDetails.Select(a => a.Sum(d => d.SalHeadAmount)).FirstOrDefault().ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").SingleOrDefault()
                                                        };
                                                        OArrJVProcessData.Add(OArrJVProcessDataBASIC);
                                                    }

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {

                                                            var OGrpArr = OSal2.Where(x => x.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id).Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var OSalBASIC = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                //foreach (var item in OSalBASIC)
                                                //    {
                                                //var OGrpBASIC = item.GroupBy(t => t.GeoStruct.Location.Id).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                //foreach (var ca10 in OGrpBASIC)
                                                //{
                                                //    ArrJVProcessData OArrJVProcessDataBASIC = new ArrJVProcessData()
                                                //    {
                                                //        ArrBatchName = mBatchName,
                                                //        ArrProcessMonth = mPayMonth,
                                                //        ArrProcessDate = DateTime.Now.Date,
                                                //        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                //        ArrBranchCode = ca10.location.LocationObj.LocCode,
                                                //        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == ca10.location).SingleOrDefault().LocationObj.LocCode,
                                                //        ArrAccountProductCode = ca.ArrJVProductCode,
                                                //        ArrAccountCustomerNo = "",
                                                //        ArrAccountCode = ca.ArrSubAccountNo,
                                                //        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                //        ArrTransactionAmount = ca10.SalDetails.Select(a => a.Sum(d => d.SalHeadAmount)).FirstOrDefault().ToString("0.00"),
                                                //        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                //        ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                //        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                //    };
                                                //    OArrJVProcessData.Add(OArrJVProcessDataBASIC);
                                                //}

                                                if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                {
                                                    var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                       .Select(e => e.Osal).ToList();
                                                    if (OSal2 != null)
                                                    {
                                                        var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Location.Id)
                                                            .Select(e => new
                                                            {
                                                                location = e.Key,
                                                                SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))
                                                                ).ToList()
                                                            }).ToList();

                                                        if (OGrpArr != null && OGrpArr.Count() > 0)
                                                        {
                                                            List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                .Select(e => new ArrJVProcessData
                                                                {
                                                                    ArrBatchName = mBatchName,
                                                                    ArrProcessMonth = mPayMonth,
                                                                    ArrProcessDate = DateTime.Now.Date,
                                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                    //ArrBranchCode = e.location.LocationObj.LocCode,
                                                                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                                    ArrAccountCustomerNo = "",
                                                                    ArrAccountCode = ca.ArrAccountNo,
                                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                    //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                    ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                    ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                    ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                }).ToList();

                                                            OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                        }
                                                        //}
                                                    }
                                                }
                                            }
                                            break;
                                        case "VDA":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalVDA = OSalaryT
                                                          .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalVDA)
                                                {
                                                    var OGrpVDA = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA").SingleOrDefault()
                                                        }).ToList();
                                                    OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {
                                                            var OGrpArr = OSal2.Where(r => r.FirstOrDefault().GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.FirstOrDefault().GeoStruct.Location.LocationObj.Id)
                                                                .Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        //   ArrBranchCode = e.location.LocationObj.LocCode,
                                                                        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                var OSalVDA = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                foreach (var item in OSalVDA)
                                                {
                                                    var OGrpVDA = item.GroupBy(t => t.GeoStruct.Location.Id).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //ArrBranchCode = e.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA").SingleOrDefault()
                                                        }).ToList();
                                                    OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                                    if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                                    {
                                                        var OSal2 = OArrSalaryT.Where(e => e.Osal != null)
                                                           .Select(e => e.Osal).ToList();
                                                        if (OSal2 != null)
                                                        {
                                                            var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Location.Id).Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                                            {
                                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                                    .Select(e => new ArrJVProcessData
                                                                    {
                                                                        ArrBatchName = mBatchName,
                                                                        ArrProcessMonth = mPayMonth,
                                                                        ArrProcessDate = DateTime.Now.Date,
                                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                        //ArrBranchCode = e.location.LocationObj.LocCode,
                                                                        ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                                        ArrAccountCustomerNo = "",
                                                                        ArrAccountCode = ca.ArrAccountNo,
                                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                                    }).ToList();

                                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            break;

                                        case "NONREGULAR":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSal3 = OSalaryT
                                                .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal3)
                                                {
                                                    var OGrp1 = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                       .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    if (OGrp1 != null && OGrp1.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca.ArrAccountNo,
                                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                                    }
                                                }
                                            }
                                            else
                                            {


                                                var OSal3 = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal3)
                                                {
                                                    var OGrp1 = item.GroupBy(t => t.GeoStruct.Location.Id).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                    if (OGrp1 != null && OGrp1.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                //ArrBranchCode = e.location.LocationObj.LocCode,
                                                                ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca.ArrAccountNo,
                                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                                    }
                                                }
                                            }
                                            break;
                                        case "EPF":
                                            if (ca.ArrIrregular == true)
                                            {

                                                var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal4 = OSalaryT1
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEPF = OSal4.Where(e => e.SalaryArrearPFT != null).Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() }).ToList();

                                                if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                                {


                                                    List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            // ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employee Share Provident Fund including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                                }
                                            }
                                            else
                                            {
                                                var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal4 = OSalaryT1.SelectMany(e => e.Osal).ToList();
                                                //  item.Select(q=>q.Select(a=>a.sa))
                                                var OEPF = OSal4.Where(e => e.SalaryArrearPFT != null).GroupBy(e => e.GeoStruct.Location.Id)
                                                .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() }).ToList();

                                                if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                                {


                                                    List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employee Share Provident Fund including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                                }
                                            }
                                            break;

                                        case "CPF":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal5 = OSalaryT2
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OCPF = OSal5.Where(e => e.SalaryArrearPFT != null).Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();
                                                var loc = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault();
                                                var fdf = "";
                                                if (loc != null)
                                                {
                                                    fdf = loc.LocationObj.LocCode;
                                                }
                                                if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //   ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = fdf,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.CompPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Company Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                                }
                                            }
                                            else
                                            {
                                                var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal5 = OSalaryT2
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OCPF = OSal5.Where(e => e.SalaryArrearPFT != null).GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();

                                                if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.CompPF)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Company Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                                }
                                            }
                                            break;
                                        case "PENSION":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal6 = OSalaryT3
                                                       .SelectMany(e => e.Osal).ToList();

                                                var OEPS = OSal6.Where(e => e.SalaryArrearPFT != null).Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();
                                                if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //   ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpEPS)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Pension Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                                }
                                            }
                                            else
                                            {


                                                var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSal6 = OSalaryT3
                                                       .SelectMany(e => e.Osal).ToList();

                                                var OEPS = OSal6.Where(e => e.SalaryArrearPFT != null).GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPFT).ToList() })
                                                    .ToList();
                                                if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            // ArrBranchCode = e.location.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpEPS)).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Pension Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                                }
                                            }
                                            break;

                                        //case "LWF":
                                        //    if (ca.ArrIrregular == true)
                                        //    {

                                        //        var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalEMPLWF = OSalaryTEMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPEMPLWF = OSalEMPLWF.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPEMPLWF != null && OEMPEMPLWF.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LWF?").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalEMPLWF = OSalaryTEMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPEMPLWF = OSalEMPLWF.GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPEMPLWF != null && OEMPEMPLWF.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "COMPLWF":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPLWF = OSalaryTCOMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPLWF = OSalCOMPLWF.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                        //        {

                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    // ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPLWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                        //        }
                                        //    }
                                        //    else
                                        //    {


                                        //        var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPLWF = OSalaryTCOMPLWF
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPLWF = OSalCOMPLWF.GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                        //            .ToList();
                                        //        if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                        //        {

                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPLWF").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "COMPESIC":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPESIS = OSalaryTCOMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPESIS = OSalCOMPESIS.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //   ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employer Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                        //        }
                                        //    }
                                        //    else
                                        //    {


                                        //        var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPESIS = OSalaryTCOMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OCOMPESIS = OSalCOMPESIS.GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                        //             .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employer Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "ESIC":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPESIS = OSalCOMPEMPESIS.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //       ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                        //        }
                                        //    }
                                        //    else
                                        //    {


                                        //        var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPESIS = OSalCOMPEMPESIS.GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                        //            .ToList();

                                        //        if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                        //        }
                                        //    }
                                        //    break;
                                        //case "PTAX":
                                        //    if (ca.ArrIrregular == true)
                                        //    {
                                        //        var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalPTAX = OSalaryTPTAX
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPPTAX = OSalPTAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.PTaxTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //     ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = (e.SalDetails.Sum(r => r.PTAmount)).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share PTAX including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //        var OSalPTAX = OSalaryTPTAX
                                        //               .Select(e => e.Osal).ToList();
                                        //        var OEMPPTAX = OSalPTAX.GroupBy(e => e.GeoStruct.Location.LocationObj.Id)
                                        //            .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.PTaxTransT).ToList() })
                                        //            .ToList();
                                        //        if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                        //        {
                                        //            List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                        //                .Select(e => new ArrJVProcessData
                                        //                {
                                        //                    ArrBatchName = mBatchName,
                                        //                    ArrProcessMonth = mPayMonth,
                                        //                    ArrProcessDate = DateTime.Now.Date,
                                        //                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                    //ArrBranchCode = e.location.LocCode,
                                        //                    ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                        //                    ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                    ArrAccountCustomerNo = "",
                                        //                    ArrAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                    ArrTransactionAmount = (e.SalDetails.Sum(r => r.PTAmount)).ToString("0.00"),
                                        //                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                    ArrNarration = "Employees Share PTAX including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                        //                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault()
                                        //                }).ToList();

                                        //            OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                        //        }
                                        //    }
                                        //    break;
                                        case "ITAX":
                                            if (ca.ArrIrregular == true)
                                            {

                                                var OSalaryTITAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSalITAX = OSalaryTITAX
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEMPITAX = OSalITAX.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList() })
                                                    .ToList();
                                                if (OEMPITAX != null && OEMPITAX.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OEMPITAX
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            // ArrTransactionAmount = e.SalDetails.Sum(r => r.Amoount).ToString("0.00"),
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employees ITAX" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                                }
                                            }
                                            else
                                            {


                                                var OSalaryTITAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                                var OSalITAX = OSalaryTITAX
                                                       .SelectMany(e => e.Osal).ToList();
                                                var OEMPITAX = OSalITAX.GroupBy(e => e.GeoStruct.Location.Id)
                                                    .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList() })
                                                    .ToList();
                                                if (OEMPITAX != null && OEMPITAX.Count() > 0) //Changes for Null
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OEMPITAX
                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            //ArrBranchCode = e.location.LocationObj.LocCode,
                                                            ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            // ArrTransactionAmount = e.SalDetails.Sum(r => r.Amoount).ToString("0.00"),
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = "Employees ITAX" + ca1.Code + " for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                                }
                                            }
                                            break;
                                        case "INSURANCE":
                                            if (ca.ArrIrregular == true)
                                            {
                                                var OSal3 = OSalaryT
                                              .Select(e => e.Osal).ToList();
                                                foreach (var item in OSal3)
                                                {
                                                    var OGrp1 = item.Where(r => r.GeoStruct.Location.LocationObj.LocCode == ca.ArrLocationIn).GroupBy(a => a.GeoStruct.Location.LocationObj.Id)
                                                             .Select(e => new
                                                             {
                                                                 location = e.Key,
                                                                 SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id
                                                                     && d.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "INSURANCE"
                                                                     )).ToList()
                                                             }).ToList();
                                                    if (OGrp1 != null && OGrp1.Count() > 0)
                                                    {
                                                        List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrp1

                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                //  ArrBranchCode = e.location.LocationObj.LocCode,
                                                                ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.LocationObj.LocCode == ca.ArrLocationOut).SingleOrDefault().LocationObj.LocCode,

                                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca.ArrAccountNo,
                                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                                ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                            }).ToList();

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var OSalInsurance = OSalaryT
                                                       .Select(e => e.Osal).ToList();
                                                //  Utility.DumpProcessStatus(LineNo: 5501);
                                                foreach (var item in OSalInsurance)
                                                {
                                                    var OEMPInsurance = item.Where(e => e.SalaryArrearPaymentT != null && e.SalaryArrearPaymentT.Count() > 0)
                                                            .GroupBy(e => e.GeoStruct.Location.Id).Select(e => new
                                                            {
                                                                location = e.Key,
                                                                SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(t =>
                                                                    t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "INSURANCE" &&
                                                                    t.SalaryHead.Id == ca1.Id)).ToList()
                                                            }).ToList();

                                                    Utility.DumpProcessStatus(LineNo: 5511);

                                                    if (OEMPInsurance != null && OEMPInsurance.Count() > 0) //Changes for Null
                                                    {
                                                        Utility.DumpProcessStatus(LineNo: 5516);
                                                        List<ArrJVProcessData> OArrJVProcessDataEMPInsurance = OEMPInsurance
                                                            .Select(e => new ArrJVProcessData
                                                            {
                                                                ArrBatchName = mBatchName,
                                                                ArrProcessMonth = mPayMonth,
                                                                ArrProcessDate = DateTime.Now.Date,
                                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                                ArrBranchCode = db.Location.Include(a => a.LocationObj).Where(a => a.Id == e.location).SingleOrDefault().LocationObj.LocCode,
                                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                                ArrAccountCustomerNo = "",
                                                                ArrAccountCode = ca.ArrAccountNo,
                                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                                ArrTransactionAmount = e.SalDetails.Sum(da => Convert.ToDecimal(da.SalHeadAmount)).ToString("0.00"),
                                                                //ArrTransactionAmount = Amt.ToString("0.00"),
                                                                // ArrTransactionAmount = e.SalDetails.Count() > 0 ? e.SalDetails.Sum(a => Convert.ToDecimal(a.SalHeadAmount)).ToString("0.00") : "0.00",
                                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                                ArrNarration = "Employees " + ca1.Code + " for Month :" + mPayMonth,
                                                                ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                            }).ToList();
                                                        Utility.DumpProcessStatus(LineNo: 5531);

                                                        OArrJVProcessData.AddRange(OArrJVProcessDataEMPInsurance);
                                                    }
                                                }
                                            }
                                            break;
                                        default:

                                            break;
                                    }

                                }

                            }
                            //locationwise grouping end
                        #endregion
                        }
                        if (ca.JVGroup.LookupVal.ToUpper() == "COMPANY")
                        {
                            foreach (var ca1 in ca.ArrSalaryHead)
                            {
                                switch (ca1.SalHeadOperationType.LookupVal.ToUpper())
                                {

                                    case "GROSS":
                                        //if Location

                                        if (OSalaryT != null && OSalaryT.Count() > 0)
                                        {
                                            var OSal = OSalaryT
                                                .Select(e => e.Osal).ToList();
                                            //foreach (var item in OSal)
                                            //{
                                            var OGrpArr = OSal.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new
                                            {
                                                location = e.Key,
                                                SalDetails = e.SelectMany(r => r.ToList()
                                                    //.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))
                                                    ).ToList()
                                            }).ToList();

                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataNet = OGrpArr
                                                    // .GroupBy(t => t.GeoStruct.Company.Id)
                                            .Select(e => new ArrJVProcessData
                                            {
                                                ArrBatchName = mBatchName,
                                                ArrProcessMonth = mPayMonth,
                                                ArrProcessDate = DateTime.Now.Date,
                                                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                ArrAccountProductCode = ca.ArrJVProductCode,
                                                ArrAccountCustomerNo = "",
                                                ArrAccountCode = ca.ArrAccountNo,
                                                ArrSubAccountCode = ca.ArrSubAccountNo,
                                                ArrTransactionAmount = e.SalDetails.Sum(r => r.ArrTotalEarning).ToString("0.00"),
                                                // e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                ArrNarration = "Gross Salary for Month :" + mPayMonth,
                                                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "GROSS").SingleOrDefault()
                                            }).ToList();


                                                OArrJVProcessData.AddRange(OArrJVProcessDataNet);
                                                //}
                                            }
                                        }
                                        //Emp_Monthsal;
                                        break;
                                    case "BASIC":
                                        //     var OSalBASIC = OSalaryT
                                        //            .Select(e => e.Osal).ToList();
                                        //     //foreach (var item in OSalBASIC)
                                        //     //{
                                        //     var OGrpBASIC = OSalBASIC.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new
                                        //{
                                        //    location = e.Key,
                                        //    SalDetails = e.SelectMany(q => q.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id))).ToList()
                                        //}).ToList();
                                        //     if (OGrpBASIC != null && OGrpBASIC.Count() > 0) //Changes for Null
                                        //     {
                                        //         List<ArrJVProcessData> OArrJVProcessDataBASIC = OGrpBASIC

                                        //             .Select(e => new ArrJVProcessData
                                        //             {
                                        //                 ArrBatchName = mBatchName,
                                        //                 ArrProcessMonth = mPayMonth,
                                        //                 ArrProcessDate = DateTime.Now.Date,
                                        //                 ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //                 ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                        //                 ArrAccountProductCode = ca.ArrJVProductCode,
                                        //                 ArrAccountCustomerNo = "",
                                        //                 ArrAccountCode = ca.ArrSubAccountNo,
                                        //                 ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //                 // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                        //                 ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                        //                 ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //                 ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                        //                 ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").SingleOrDefault()
                                        //             }).ToList();
                                        //         OArrJVProcessData.AddRange(OArrJVProcessDataBASIC);
                                        //     }
                                        if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                        {
                                            var OSal2 = OArrSalaryT
                                               .Select(e => e.Osal).ToList();
                                            var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new { location = e.Key, SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList() }).ToList();

                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = ca.ArrAccountNo,
                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                        // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                //}
                                            }
                                        }

                                        break;
                                    case "VDA":
                                        var OSalVDA = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        foreach (var item in OSalVDA)
                                        {
                                            var OGrpVDA = item.GroupBy(t => t.GeoStruct.Company.Id).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                            List<ArrJVProcessData> OArrJVProcessDataVDA = OGrpVDA

                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                    ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = ca.ArrAccountNo,
                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                    // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                    ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                    ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA").SingleOrDefault()
                                                }).ToList();
                                            OArrJVProcessData.AddRange(OArrJVProcessDataVDA);

                                            if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                            {
                                                var OSal2 = OArrSalaryT
                                                   .Select(e => e.Osal).ToList();
                                                var OGrpArr = OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new
                                                {
                                                    location = e.Key,
                                                    SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList()
                                                }).ToList();

                                                if (OGrpArr != null && OGrpArr.Count() > 0)
                                                {
                                                    List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            //ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                            ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                }
                                            }
                                        }

                                        break;

                                    case "REGULAR":
                                        // var OSal1 = OSalaryT
                                        //        .Select(e => e.Osal).ToList();
                                        //foreach (var item in OSal1)
                                        //             {
                                        //   var OGrp = item.GroupBy(t => t.GeoStruct.Company.Id).Select(e => new { SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();

                                        // List<ArrJVProcessData> OArrJVProcessDataRegular = OGrp

                                        //     .Select(e => new ArrJVProcessData
                                        //     {
                                        //         ArrBatchName = mBatchName,
                                        //         ArrProcessMonth = mPayMonth,
                                        //         ArrProcessDate = DateTime.Now.Date,
                                        //         ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                        //         ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                        //         ArrAccountProductCode = ca.ArrJVProductCode,
                                        //         ArrAccountCustomerNo = "",
                                        //         ArrAccountCode = ca.ArrSubAccountNo,
                                        //         ArrSubAccountCode = ca.ArrSubAccountNo,
                                        //         ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                        //         ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                        //         ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                        //         ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                        //     }).ToList();
                                        // OArrJVProcessData.AddRange(OArrJVProcessDataRegular);

                                        if (OArrSalaryT != null && OArrSalaryT.Count() > 1)
                                        {
                                            var OSal2 = OArrSalaryT.Where(r => r.Osal != null)
                                               .Select(e => e.Osal).ToList();

                                            var OGrpArr = OSal2 == null ? null : OSal2.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new
                                            {
                                                location = e.Key,
                                                SalDetails = e.SelectMany(r => r.Select(d => d.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))).ToList()
                                            }).ToList();

                                            if (OGrpArr != null && OGrpArr.Count() > 0)
                                            {
                                                List<ArrJVProcessData> OArrJVProcessDataRegularArr = OGrpArr

                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = ca.ArrAccountNo,
                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),

                                                        //  ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),
                                                        //ArrTransactionAmount = e.SalDetails.Sum(a => (a.Select(d => Convert.ToDecimal(d.SalHeadAmount)))).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = ca1.Name + " Arrear for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataRegularArr);
                                                //}
                                            }
                                        }

                                        break;
                                    case "NONREGULAR":
                                        if (OSalaryT != null && OSalaryT.Count() > 0)
                                        {
                                            var OSal3 = OSalaryT
                                                   .Select(e => e.Osal).ToList();
                                            //foreach (var item in OSal3)
                                            //{
                                                //var OGrp1 = item.GroupBy(t => t.GeoStruct.Company.Id).Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(d => d.SalaryHead.Id == ca1.Id)).ToList() }).ToList();
                                                var OGrpArr = OSal3.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new
                                                {
                                                    location = e.Key,
                                                    SalDetails = e.SelectMany(x => x.ToList()
                                                        .Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.Id == ca1.Id))
                                                        ).ToList()
                                                }).ToList();
                                                if (OGrpArr != null && OGrpArr.Count() > 0) //Changes for Null
                                                {
                                                    var a = OGrpArr.Select(r => r.SalDetails).SingleOrDefault();
                                                    double Amt = 0;
                                                    foreach (var item1 in a)
                                                    {
                                                        var test = item1.FirstOrDefault();
                                                        if (test != null)
                                                        {
                                                            Amt = Amt + test.SalHeadAmount;
                                                        }
                                                    }
                                                    List<ArrJVProcessData> OArrJVProcessDataNonRegular = OGrpArr

                                                        .Select(e => new ArrJVProcessData
                                                        {
                                                            ArrBatchName = mBatchName,
                                                            ArrProcessMonth = mPayMonth,
                                                            ArrProcessDate = DateTime.Now.Date,
                                                            ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                            ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                            ArrAccountProductCode = ca.ArrJVProductCode,
                                                            ArrAccountCustomerNo = "",
                                                            ArrAccountCode = ca.ArrAccountNo,
                                                            ArrSubAccountCode = ca.ArrSubAccountNo,
                                                            // ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                            ArrTransactionAmount = Amt.ToString("0.00"),

                                                            ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                            ArrNarration = ca1.Name + " Salary for Month :" + mPayMonth,
                                                            ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                        }).ToList();

                                                    OArrJVProcessData.AddRange(OArrJVProcessDataNonRegular);
                                                }
                                           
                                        }
                                        break;
                                    case "EPF":

                                        var OSalaryT1 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();  //change reqq
                                        var OSal4 = OSalaryT1
                                               .SelectMany
                                               (e => e.Osal).ToList();
                                        var OEPF = OSal4.GroupBy(e => e.GeoStruct.Company.Id)
                                            .Select(e => new
                                            {
                                                location = e.Key,
                                                SalDetails = e.Where(a => a.SalaryArrearPFT != null)
                                                    .Select(r => r.SalaryArrearPFT).ToList()
                                            }).ToList();

                                        if (OEPF != null && OEPF.Count() > 0) //Changes for Null
                                        {
                                            var a = OEPF.Select(r => r.SalDetails).SingleOrDefault();
                                            double Amt = 0;
                                            foreach (var item in a)
                                            {
                                                if (item != null)
                                                {
                                                    double total = item.EmpPF;
                                                    Amt = Amt + total;
                                                }
                                            }
                                            List<ArrJVProcessData> OArrJVProcessDataEPF = OEPF
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                    ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = ca.ArrAccountNo,
                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                    ArrTransactionAmount = Amt.ToString("0.00"),
                                                    //ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpPF) ).ToString("0.00"),
                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                    ArrNarration = "Employee Share Provident Fund including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "EPF").SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEPF);
                                        }
                                        break;

                                    case "CPF":
                                        var OSalaryT2 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal5 = OSalaryT2
                                               .SelectMany(e => e.Osal).ToList();
                                        var OCPF = OSal5.GroupBy(e => e.GeoStruct.Company.Id)
                                            .Select(e => new
                                            {
                                                location = e.Key,
                                                SalDetails = e.Where(a => a.SalaryArrearPFT != null).Select(r => r.SalaryArrearPFT).ToList()
                                            })
                                            .ToList();
                                        if (OCPF != null && OCPF.Count() > 0) //Changes for Null
                                        {

                                            List<ArrJVProcessData> OArrJVProcessDataCPF = OCPF
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                    ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = ca.ArrAccountNo,
                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                    ArrTransactionAmount = (e.SalDetails.Sum(r => r.CompPF)).ToString("0.00"),
                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                    ArrNarration = "Company Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "CPF").SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataCPF);
                                        }
                                        break;
                                    case "PENSION":

                                        var OSalaryT3 = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        var OSal6 = OSalaryT3
                                               .SelectMany(e => e.Osal).ToList();

                                        var OEPS = OSal6.GroupBy(e => e.GeoStruct.Company.Id)
                                            .Select(e => new
                                            {
                                                location = e.Key,
                                                SalDetails = e.Where(a => a.SalaryArrearPFT != null).Select(r => r.SalaryArrearPFT).ToList()
                                            })
                                            .ToList();

                                        if (OEPS != null && OEPS.Count() > 0) //Changes for Null
                                        {

                                            List<ArrJVProcessData> OArrJVProcessDataEPS = OEPS
                                                .Select(e => new ArrJVProcessData
                                                {
                                                    ArrBatchName = mBatchName,
                                                    ArrProcessMonth = mPayMonth,
                                                    ArrProcessDate = DateTime.Now.Date,
                                                    ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                    ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                    ArrAccountProductCode = ca.ArrJVProductCode,
                                                    ArrAccountCustomerNo = "",
                                                    ArrAccountCode = ca.ArrAccountNo,
                                                    ArrSubAccountCode = ca.ArrSubAccountNo,
                                                    ArrTransactionAmount = (e.SalDetails.Sum(r => r.EmpEPS)).ToString("0.00"),
                                                    ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                    ArrNarration = "Pension Share Provident Fund" + ca1.Code + " for Month :" + mPayMonth,
                                                    ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PENSION").SingleOrDefault()
                                                }).ToList();

                                            OArrJVProcessData.AddRange(OArrJVProcessDataEPS);
                                        }
                                        break;

                                    //case "LWF":
                                    //    var OSalaryTEMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalEMPLWF = OSalaryTEMPLWF
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPEMPLWF = OSalEMPLWF.GroupBy(e => e.GeoStruct.Company.Id)
                                    //        .Select(e => new { location = e.Key, SalDetails = e.Where(a => a.LWFTransT != null).Select(r => r.LWFTransT).ToList() })
                                    //        .ToList();
                                    //    var asa = OEMPEMPLWF.Where(e => e.SalDetails.Count > 0).Count();
                                    //    if (OEMPEMPLWF != null && OEMPEMPLWF.Count > 0 && asa > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPLWF = OEMPEMPLWF
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                    //                ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                    //                ArrAccountProductCode = ca.ArrJVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = ca.ArrSubAccountNo,
                                    //                ArrSubAccountCode = ca.ArrSubAccountNo,
                                    //                ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                    //                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "LWF").SingleOrDefault()
                                    //            }).ToList();


                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPLWF);
                                    //    }

                                    //    break;
                                    //case "COMPLWF":
                                    //    var OSalaryTCOMPLWF = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPLWF = OSalaryTCOMPLWF
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OCOMPLWF = OSalCOMPLWF.GroupBy(e => e.GeoStruct.Company.Id)
                                    //         .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.LWFTransT).ToList() })
                                    //        .ToList();
                                    //    if (OCOMPLWF != null && OCOMPLWF.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataCOMPLWF = OCOMPLWF
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                    //                ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                    //                ArrAccountProductCode = ca.ArrJVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = ca.ArrSubAccountNo,
                                    //                ArrSubAccountCode = ca.ArrSubAccountNo,
                                    //                ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                    //                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employee Share LWF" + ca1.Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPLWF").SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataCOMPLWF);
                                    //    }
                                    //    break;
                                    //            //

                                    //case "COMPESIC":
                                    //    var OSalaryTCOMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPESIS = OSalaryTCOMPESIS
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OCOMPESIS = OSalCOMPESIS.GroupBy(e => e.GeoStruct.Company.Id)
                                    //         .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                    //        .ToList();

                                    //    if (OCOMPESIS != null && OCOMPESIS.Count() > 0) //Changes for Null
                                    //    {

                                    //        List<ArrJVProcessData> OArrJVProcessDataCOMPESIS = OCOMPESIS
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                    //                ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                    //                ArrAccountProductCode = ca.ArrJVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = ca.ArrSubAccountNo,
                                    //                ArrSubAccountCode = ca.ArrSubAccountNo,
                                    //                ArrTransactionAmount = e.SalDetails.Sum(r => r.CompAmt).ToString("0.00"),
                                    //                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employer Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "COMPESIC").SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataCOMPESIS);
                                    //    }
                                    //    break;
                                    //case "ESIC":
                                    //    var OSalaryTEMPESIS = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalCOMPEMPESIS = OSalaryTEMPESIS
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPESIS = OSalCOMPEMPESIS.GroupBy(e => e.GeoStruct.Company.Id)
                                    //        .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.ESICTransT).ToList() })
                                    //        .ToList();
                                    //    if (OEMPESIS != null && OEMPESIS.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPESIS = OEMPESIS
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                    //                ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                    //                ArrAccountProductCode = ca.ArrJVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = ca.ArrSubAccountNo,
                                    //                ArrSubAccountCode = ca.ArrSubAccountNo,
                                    //                ArrTransactionAmount = e.SalDetails.Sum(r => r.EmpAmt).ToString("0.00"),
                                    //                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employees Share ESIS" + ca1.Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ESIC").SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPESIS);
                                    //    }
                                    //    break;
                                    //case "PTAX":
                                    //    var OSalaryTPTAX = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                    //    var OSalPTAX = OSalaryTPTAX
                                    //           .Select(e => e.Osal).ToList();
                                    //    var OEMPPTAX = OSalPTAX.GroupBy(e => e.GeoStruct.Company.Id)
                                    //        .Select(e => new { location = e.Key, SalDetails = e.Where(r => r.PTaxTransT != null).Select(r => r.PTaxTransT).ToList() })
                                    //        .ToList();

                                    //    if (OEMPPTAX != null && OEMPPTAX.Count() > 0) //Changes for Null
                                    //    {
                                    //        List<ArrJVProcessData> OArrJVProcessDataEMPPTAX = OEMPPTAX
                                    //            .Select(e => new ArrJVProcessData
                                    //            {
                                    //                ArrBatchName = mBatchName,
                                    //                ArrProcessMonth = mPayMonth,
                                    //                ArrProcessDate = DateTime.Now.Date,
                                    //                ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                    //                ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                    //                ArrAccountProductCode = ca.ArrJVProductCode,
                                    //                ArrAccountCustomerNo = "",
                                    //                ArrAccountCode = ca.ArrSubAccountNo,
                                    //                ArrSubAccountCode = ca.ArrSubAccountNo,
                                    //                ArrTransactionAmount = (e.SalDetails.Sum(r => r.PTAmount)).ToString("0.00"),
                                    //                ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                    //                ArrNarration = "Employees Share PTAX including Arrears" + ca1.Code + " for Month :" + mPayMonth,
                                    //                ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "PTAX").SingleOrDefault()
                                    //            }).ToList();

                                    //        OArrJVProcessData.AddRange(OArrJVProcessDataEMPPTAX);
                                    //    }
                                    //    break;
                                    case "ITAX":
                                        // var OSalaryTITAX = OSalaryT.Select(e => new { Osal = e.Osal.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        
                                         if (OSalaryT != null && OSalaryT.Count() > 0)
                                        {
                                            var OSalITAX = OSalaryT
                                              .Select(e => e.Osal).ToList();

                                            var OGrpArr = OSalITAX.GroupBy(t => t.FirstOrDefault().GeoStruct.Company.Id).Select(e => new
                                            {
                                                location = e.Key,
                                                SalDetails = e.SelectMany(x => x.ToList()
                                                    .Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX"))
                                                    ).ToList()
                                            }).ToList();

                                        //foreach (var item in OSalITAX)
                                        //{
                                        //    var OEMPITAX = item.GroupBy(e => e.GeoStruct.Company.Id)
                                        //.Select(e => new
                                        //{
                                        //    location = e.Key,
                                        //    SalDetails = e.Where(a => a.SalaryArrearPaymentT != null && a.SalaryArrearPaymentT.Count > 0)
                                        //        .Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).ToList()
                                        //})
                                        //   .ToList();
                                            if (OGrpArr != null && OGrpArr.Count() > 0) //Changes for Null
                                            {
                                                var a = OGrpArr.Select(r => r.SalDetails).SingleOrDefault();
                                                double Amt = 0;
                                                foreach (var item1 in a)
                                                {
                                                    var test = item1.FirstOrDefault();
                                                    if (test != null)
                                                    {
                                                        Amt = Amt + test.SalHeadAmount;
                                                    }
                                                }
                                                List<ArrJVProcessData> OArrJVProcessDataEMPITAX = OGrpArr
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = ca.ArrAccountNo,
                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                        // ArrTransactionAmount = e.SalDetails.Sum(r => r.SalHeadAmount).ToString("0.00"),

                                                        // ArrTransactionAmount = e.SalDetails.Count() > 0 ? e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00") : "0.00",

                                                        ArrTransactionAmount = Amt.ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Employees ITAX" + ca1.Code + " for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Include(r => r.SalHeadOperationType).Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataEMPITAX);
                                            }
                                        }
                                        break;

                                    //Added By Sudhir

                                    case "LOAN":
                                        //  var OSalaryTLoan = OEmployeePayroll.Select(e => new { Osal = e.SalaryArrearT.Where(d => d.PayMonth == mPayMonth).ToList() }).ToList();
                                        //var OSalaryTLoan = OEmployeePayroll.Select(s => new { Osal = s.LoanAdvRequest.Where(e => e.LoanAdvanceHead.SalaryHead.Id == ca1.Id && (e.CloserDate == null || e.CloserDate >= Convert.ToDateTime("01/" + mPayMonth).AddMonths(1).Date)) }).ToList();
                                        var OSalLoan = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        foreach (var item in OSalLoan)
                                        {
                                            var OEMPLoan = item.GroupBy(e => e.GeoStruct.Company.Id)
                                                // .Select(e => new { location = e.Key, SalDetails = e.Select(r => r.SalaryArrearPaymentT.Where(t => t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")).FirstOrDefault() })
                                                //.ToList();
                                      .Select(e => new
                                      {
                                          location = e.Key,
                                          SalDetails = e.Where(a => a.SalaryArrearPaymentT != null && a.SalaryArrearPaymentT.Count > 0)
                                              .Select(r => r.SalaryArrearPaymentT.Where(t => (t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN") && t.SalaryHead.Id == ca1.Id)).ToList()
                                      })
                                         .ToList();
                                            if (OEMPLoan != null && item.Count() > 0) //Changes for Null
                                            {


                                                List<ArrJVProcessData> OArrJVProcessDataEMPLoan = OEMPLoan
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = ca.ArrAccountNo,
                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                        // ArrTransactionAmount = e.SalDetails.Sum(r => r.SalHeadAmount).ToString("0.00"),
                                                        // ArrTransactionAmount = e.SalDetails.Sum(a => Convert.ToDecimal(a.Select(d => d.SalHeadAmount))).ToString("0.00"),

                                                        ArrTransactionAmount = e.SalDetails.Sum(a => a.Sum(d => Convert.ToDecimal(d.SalHeadAmount))).ToString("0.00"),
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Company Loan" + ca1.Code + " for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Code == ca1.Code).SingleOrDefault()
                                                    }).ToList();

                                                OArrJVProcessData.AddRange(OArrJVProcessDataEMPLoan);
                                            }
                                        }
                                        break;
                                    // Added by Rekha 25072017
                                    case "INSURANCE":
                                        var OSalInsurance = OSalaryT
                                               .Select(e => e.Osal).ToList();
                                        //   Utility.DumpProcessStatus(LineNo: 5501);
                                        foreach (var item1 in OSalInsurance)
                                        {
                                            var OEMPInsurance = item1.Where(e => e.SalaryArrearPaymentT != null && e.SalaryArrearPaymentT.Count() > 0)
                                        .GroupBy(e => e.GeoStruct.Company.Id).Select(e => new
                                        {
                                            SalDetails = e.SelectMany(r => r.SalaryArrearPaymentT.Where(t =>
                                                t.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "INSURANCE" &&
                                                t.SalaryHead.Id == ca1.Id)).ToList()
                                        }).ToList();

                                            Utility.DumpProcessStatus(LineNo: 5511);

                                            if (OEMPInsurance != null && OEMPInsurance.Count() > 0) //Changes for Null
                                            {
                                                Utility.DumpProcessStatus(LineNo: 5516);
                                                var a = OEMPInsurance.Select(r => r.SalDetails).SingleOrDefault();
                                                double Amt = 0;
                                                foreach (var item in a)
                                                {
                                                    if (item != null)
                                                    {
                                                        double total = item.SalHeadAmount;
                                                        Amt = Amt + total;
                                                    }
                                                }
                                                List<ArrJVProcessData> OArrJVProcessDataEMPInsurance = OEMPInsurance
                                                    .Select(e => new ArrJVProcessData
                                                    {
                                                        ArrBatchName = mBatchName,
                                                        ArrProcessMonth = mPayMonth,
                                                        ArrProcessDate = DateTime.Now.Date,
                                                        ArrJVParameter = db.ArrJVParameter.Find(ca.Id),
                                                        ArrBranchCode = ca.ArrCreditDebitBranchCode,
                                                        ArrAccountProductCode = ca.ArrJVProductCode,
                                                        ArrAccountCustomerNo = "",
                                                        ArrAccountCode = ca.ArrAccountNo,
                                                        ArrSubAccountCode = ca.ArrSubAccountNo,
                                                        ArrTransactionAmount = Amt.ToString("0.00"),
                                                        // ArrTransactionAmount = e.SalDetails.Count() > 0 ? e.SalDetails.Sum(a => Convert.ToDecimal(a.SalHeadAmount)).ToString("0.00") : "0.00",
                                                        ArrCreditDebitFlag = ca.ArrCreditDebitFlag,
                                                        ArrNarration = "Employees " + ca1.Code + " for Month :" + mPayMonth,
                                                        ArrSalaryHead = db.SalaryHead.Where(r => r.Id == ca1.Id).SingleOrDefault()
                                                    }).ToList();
                                                Utility.DumpProcessStatus(LineNo: 5531);

                                                OArrJVProcessData.AddRange(OArrJVProcessDataEMPInsurance);
                                            }
                                        }
                                        break;
                                    default:

                                        break;
                                }
                            }
                        }
                    }
                }
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                foreach (var ca in OArrJVProcessData)
                {
                    ca.DBTrack = dbt;
                }
                db.ArrJVProcessData.AddRange(OArrJVProcessData);

                db.SaveChanges();
                OJVParameterComp.ArrJVProcessData = OArrJVProcessData;
                db.CompanyPayroll.Attach(OJVParameterComp);
                db.Entry(OJVParameterComp).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                P2BUltimate.Models.JVFile OJVFile = new P2BUltimate.Models.JVFile();
                var mfilestring = "";
                if (_CompCode == _CustomeCompCode)
                {
                    mfilestring = OJVFile.CreateArrJVFileBhavNagarPatch(OArrJVProcessData, mPayMonth);
                }
                else if (_CompCode == "KDCC")
                {
                    mfilestring = OJVFile.CreateArrJVFileKDCCPatch(OArrJVProcessData, mPayMonth);
                }
                else if (_CompCode == "ASBL")
                {
                    mfilestring = OJVFile.CreateArrJVFileASBLPatch(OArrJVProcessData, mPayMonth);
                }
                else
                {
                    mfilestring = OJVFile.CreateArrJVFile(OArrJVProcessData, mPayMonth);
                }
                //write summary

                var OJVDataCredit = OArrJVProcessData
                    .Where(e => e.ArrCreditDebitFlag == "C")
                    .Sum(e => Convert.ToDouble(e.ArrTransactionAmount));

                var OJVDataDebit = OArrJVProcessData
                    .Where(e => e.ArrCreditDebitFlag == "D")
                    .Sum(e => Convert.ToDouble(e.ArrTransactionAmount));

                ArrJVProcessDataSummary OArrJVProcessDataSum = new ArrJVProcessDataSummary
                {
                    ArrBatchName = mBatchName,
                    ArrProcessMonth = mPayMonth,
                    ArrProcessDate = DateTime.Now.Date,
                    ArrCreditAmount = Math.Round(OJVDataCredit, 2),
                    ArrDebitAmount = Math.Round(OJVDataDebit, 2),
                    ArrJVFileName = mfilestring,
                    DBTrack = dbt,
                };
                List<ArrJVProcessDataSummary> OArrJVProcessDataSummary = new List<ArrJVProcessDataSummary>();
                OArrJVProcessDataSummary.Add(OArrJVProcessDataSum);
                db.ArrJVProcessDataSummary.Add(OArrJVProcessDataSum);
                db.SaveChanges();
                OJVParameterComp.ArrJVProcessDataSummary = OArrJVProcessDataSummary;
                db.CompanyPayroll.Attach(OJVParameterComp);
                db.Entry(OJVParameterComp).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }


    }
}