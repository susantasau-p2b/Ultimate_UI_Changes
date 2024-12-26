using Attendance;
using Microsoft.Reporting.WebForms;
using Leave;
using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using ReportPayroll;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Training;
using System.IO;

namespace P2BUltimate.Process
{
    public class SummaryReportGen
    {

        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };

        public static List<GenericField100> GenerateSummaryReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime pFromDate, DateTime pToDate, List<string> Sortingslist)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Database.CommandTimeout = 3600;
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {
                    case "SALEARNDEDSUMMARY":
                        List<int?> OEmpPayrollIdList = new List<int?>();
                        List<EmployeePayroll> OEmpEarnDedSum = new List<EmployeePayroll>();
                        string PayMon = mPayMonth.FirstOrDefault();
                        // OEmpPayrollIdList = db.SalaryT.Where(e => e.PayMonth == PayMon).Select(r => r.EmployeePayroll_Id).ToList();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    var OSalaryData_temp = db.EmployeePayroll

                        //      .Include(e => e.Employee)
                        //        .Include(e => e.Employee.EmpName)
                        //        .Include(r => r.FunctAttendanceT)
                        //        .Include(r => r.SalaryT.Select(t => t.SalEarnDedT))
                        //        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                        //        .Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))
                        //        .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct))
                        //        .Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct))
                        //        .Include(r => r.FunctAttendanceT.Select(t => t.PayStruct))
                        //    .Where(e => e.Employee_Id == item).AsNoTracking().AsParallel()
                        //    .FirstOrDefault();
                        //    List<string> BusiCategory = new List<string>();

                        //    if (OSalaryData_temp != null)
                        //    {
                        //        if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //        {
                        //            BusiCategory.AddRange(salheadlist);

                        //            foreach (string ca1 in BusiCategory)
                        //            {
                        //                OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                        //                OSalaryData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                        //                OSalaryData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo.Bank_Id).AsNoTracking().FirstOrDefault();
                        //                OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();

                        //                OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).AsNoTracking().ToList();

                        //                foreach (var item1 in OSalaryData_temp.SalaryT)
                        //                {
                        //                    item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0).AsNoTracking().ToList();
                        //                    foreach (var item2 in item1.SalEarnDedT)
                        //                    {
                        //                        item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                        //                        item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                        //                        item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                        //                    }
                        //                    item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true).ToList();
                        //                    item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                        //                    item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                        //                    item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                        //                    if (item1.Geostruct.Location != null)
                        //                    {
                        //                        item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                        //                    }
                        //                    item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                        //                    if (item1.Geostruct.Department != null)
                        //                    {
                        //                        item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                        //                    }

                        //                    item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                        //                    item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                        //                    item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                        //                    item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                        //                    item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                        //                    if (item1.FuncStruct.Job != null)
                        //                    {
                        //                        item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                        //                    }
                        //                }

                        //                if (OSalaryData_temp.Employee != null && OSalaryData_temp.Employee.EmpOffInfo != null && OSalaryData_temp.Employee.EmpOffInfo.Bank != null)
                        //                {
                        //                    if (OSalaryData_temp.Employee.EmpOffInfo.Bank.Name.ToString().ToUpper() == ca1.ToUpper())
                        //                    {

                        //                        OEmpEarnDedSum.Add(OSalaryData_temp);

                        //                        break;
                        //                    }

                        //                }
                        //            }

                        //        }

                        //        else
                        //        {
                        //            OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                        //            OSalaryData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                        //            OSalaryData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo.Bank_Id).AsNoTracking().FirstOrDefault();
                        //            OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();

                        //            OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).AsNoTracking().ToList();

                        //            foreach (var item1 in OSalaryData_temp.SalaryT)
                        //            {
                        //                item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0).AsNoTracking().ToList();
                        //                foreach (var item2 in item1.SalEarnDedT)
                        //                {
                        //                    item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                        //                    item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                        //                    item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                        //                }
                        //                item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true).ToList();
                        //                item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                        //                item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                        //                item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                        //                if (item1.Geostruct.Location != null)
                        //                {
                        //                    item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                        //                }
                        //                item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                        //                if (item1.Geostruct.Department != null)
                        //                {
                        //                    item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                        //                }

                        //                item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                        //                item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                        //                item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                        //                item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                        //                item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                        //                if (item1.FuncStruct.Job != null)
                        //                {
                        //                    item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                        //                }
                        //            }
                        //            OEmpEarnDedSum.Add(OSalaryData_temp);
                        //        }

                        //    }
                        //}


                        //if (OEmpEarnDedSum == null)
                        //{
                        //    return null;
                        //}
                        //else
                        //{

                        //    var SpGroup = SpecialGroupslist.ToList();
                        //    var JOB = false;
                        //    var GRADE = false;
                        //    var JOBPOSITION = false;


                        //    foreach (var item1 in SpGroup)
                        //    {
                        //        if (item1 == "JOB")
                        //        {

                        //            JOB = true;
                        //        }
                        //        if (item1 == "GRADE")
                        //        {

                        //            GRADE = true;
                        //        }
                        //        if (item1 == "JOB POSITION")
                        //        {

                        //            JOBPOSITION = true;
                        //        }
                        //        if (item1 == "DEPARTMENT")
                        //        {

                        //            JOBPOSITION = true;
                        //        }
                        //        if (item1 == "LOCATION")
                        //        {

                        //            JOBPOSITION = true;
                        //        }

                        //    }

                        //    var dept = false;
                        //    var loca = false;
                        //    var div = false;
                        //    var job = false;

                        //    var filter = "";

                        //    var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).AsNoTracking().ToList();
                        //    var header = "";
                        //    var group = "";
                        //    foreach (var item in vc)
                        //    {
                        //        if (item.LookupVal.ToUpper() == "LOCATION")
                        //        {
                        //            loca = true;
                        //            header = "Location";
                        //        }
                        //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                        //        {
                        //            dept = true;
                        //            header = "Department";
                        //        }
                        //        if (item.LookupVal.ToUpper() == "DIVISION")
                        //        {
                        //            div = true;
                        //            header = "Division";
                        //        }
                        //        if (item.LookupVal.ToUpper() == "JOB")
                        //        {
                        //            job = true;
                        //            header = "Job";
                        //        }
                        //    }


                        //    Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                        //    List<Utility.GetSalaryDataClass> SalaryData = new List<Utility.GetSalaryDataClass>();


                        //    foreach (var ca in OEmpEarnDedSum)
                        //    {

                        //        SalaryT OSalT = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault();
                        //        if (OSalT != null)
                        //        {
                        //            int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                        //            SalaryData = ReportRDLCObjectClass.GetSalaryData(OSalT, ca.Employee, OSalT.EmployeePayroll_Id);

                        //        }

                        //        if (SalaryData != null)
                        //        {
                        //            foreach (var item in SalaryData)
                        //            {
                        //                var paybank = "";
                        //                if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //                {
                        //                    paybank = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Bank_Id != null ? ca.Employee.EmpOffInfo.Bank.Name.ToString() : "";
                        //                }
                        //                GenericField100 OGenericObjStatement = new GenericField100()
                        //                {
                        //                    Fld1 = item.Company_Code.ToString(),
                        //                    Fld2 = item.Company_Name,
                        //                    Fld4 = item.DeptCode,
                        //                    Fld5 = item.DeptDesc,
                        //                    Fld6 = item.Division_Code,
                        //                    Fld7 = item.Division_Name,
                        //                    Fld8 = item.EarnAmount,
                        //                    Fld9 = item.EarnCode,
                        //                    Fld10 = item.EarnHead,
                        //                    Fld11 = item.EmpCode,
                        //                    Fld12 = item.Employee_Id,
                        //                    Fld13 = item.EmployeePayroll_Id,
                        //                    Fld14 = item.EmpName,
                        //                    Fld15 = item.EmpName_Id,
                        //                    Fld16 = item.EPS_Share,
                        //                    Fld17 = item.ER_Share,
                        //                    Fld18 = item.GeoStruct_Id,
                        //                    Fld19 = item.Grade_Code,
                        //                    Fld20 = item.Grade_Name,
                        //                    Fld21 = item.InPayslip,
                        //                    Fld22 = item.Job_Code,
                        //                    Fld23 = item.Job_Name,
                        //                    Fld24 = item.JobPositionCode,
                        //                    Fld25 = item.JobPositionDesc,
                        //                    Fld26 = item.LocCode,
                        //                    Fld27 = item.LocDesc + " " + paybank,
                        //                    Fld28 = item.LookupVal,
                        //                    Fld29 = item.PayMonth,
                        //                    Fld30 = item.PayStruct_Id,
                        //                    Fld31 = item.ProcessDate,
                        //                    Fld32 = item.ReleaseDate,
                        //                    Fld33 = item.SalaryHead_Id,
                        //                    Fld34 = item.SalHead_FullDetails,
                        //                    Fld35 = item.SalHead_SeqNo,
                        //                    Fld36 = item.StdAmount,
                        //                    Fld37 = item.TotalDeduction,
                        //                    Fld38 = item.TotalEarning,
                        //                    Fld39 = item.TotalNet,
                        //                };

                        //                OGenericPayrollStatement.Add(OGenericObjStatement);
                        //            }
                        //        }
                        //    }
                        //    return OGenericPayrollStatement
                        //}
                        #region ReportData Collection and Forwarding to Reporting Object

                        // zero atendance employee record will show code start
                        List<string> salaryheadappearinpayslipsum = new List<string>();
                        var EmpDataListzerosum = db.SalaryT.Where(e => e.PayMonth == PayMon
                            )
                                .Select(e => new
                                {
                                    OSalaryDataList = e.SalEarnDedT
                                   .Select(d => new
                                   {
                                       EarnAmount = d.Amount.ToString(),
                                       SalaryHead_Id = d.SalaryHead_Id.ToString(),
                                       SalHead_SeqNo = d.SalaryHead.SeqNo.ToString(),
                                       LookupVal = d.SalaryHead.Type.LookupVal.ToString(),
                                       InPayslip = d.SalaryHead.InPayslip.ToString(),

                                   })
                                   .Where(d => d.EarnAmount != "0" && d.InPayslip == "true").ToList()
                                }).AsNoTracking().ToList();
                        foreach (var EmpData in EmpDataListzerosum)
                        {
                            foreach (var OSalaryData in EmpData.OSalaryDataList.OrderBy(e => e.LookupVal).ThenBy(x => x.SalHead_SeqNo))
                            {
                                salaryheadappearinpayslipsum.Add(OSalaryData.SalaryHead_Id);
                            }

                        }
                        salaryheadappearinpayslipsum = salaryheadappearinpayslipsum.Distinct().ToList();
                        // zero atendance employee record will show code start

                        List<string> business_category = new List<string>();

                        if (salaryheadappearinpayslipsum.Count() == 0 )
                        {
                            return null;
                        }
                        else
                        {
                            var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "MONTH")
                                {
                                    month = true;
                                }
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }




                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0) //Business Category Selection i.e. Payment Bank Name
                            {//business category
                                business_category.AddRange(salheadlist);
                                var EmpDataList = db.SalaryT.Where(e => e.PayMonth == PayMon && business_category.Contains(e.EmployeePayroll.Employee.EmpOffInfo.Bank.Name.ToString()) && EmpPayrollIdList.Contains(e.EmployeePayroll.Employee_Id.Value)
                                    )
                                     .Select(e => new
                                     {
                                         EmpCode = e.EmployeePayroll.Employee.EmpCode,
                                         //EmpName = e.EmployeePayroll.Employee.EmpName.FullNameFML,
                                         //EmpName_Id = e.EmployeePayroll.Employee.EmpName_Id.ToString(),
                                         //Employee_Id = e.EmployeePayroll.Employee.Id.ToString(),                                     
                                         //EmployeePayroll_Id = e.EmployeePayroll_Id.ToString(),
                                         //Grade_Name = e.PayStruct.Grade.Name,
                                         //Grade_Code = e.PayStruct.Grade.Code,
                                         GeoStruct_Id = e.Geostruct_Id.ToString(),
                                        FuncStruct_Id = e.FuncStruct_Id.ToString(),
                                         //Job_Code = e.FuncStruct.Job.Code,
                                         //Job_Name = e.FuncStruct.Job.Name,
                                         PayStruct_Id = e.PayStruct_Id.ToString(),
                                         //JobPositionCode = e.FuncStruct.JobPosition.JobPositionCode,
                                         //JobPositionDesc = e.FuncStruct.JobPosition.JobPositionDesc,
                                         LocCode = e.Geostruct.Location.LocationObj.LocCode,
                                         LocDesc = e.Geostruct.Location.LocationObj.LocDesc + " : " + e.EmployeePayroll.Employee.EmpOffInfo.Bank.Name,
                                         //Division_Code = e.Geostruct.Division.Code,
                                         //Division_Name = e.Geostruct.Division.Name,
                                         //DeptCode = e.Geostruct.Department.DepartmentObj.DeptCode,
                                         //DeptDesc = e.Geostruct.Department.DepartmentObj.DeptDesc,
                                         //Company_Code = e.Geostruct.Company.Code,
                                         //Company_Name = e.Geostruct.Company.Name,
                                         //TotalDeduction = e.TotalDeduction.ToString(),
                                         //TotalEarning = e.TotalEarning.ToString(),
                                         //TotalNet = e.TotalNet.ToString(),
                                         PayMonth = e.PayMonth,
                                         //ProcessDate = e.ProcessDate.ToString(),
                                         //ReleaseDate = e.ReleaseDate.ToString(),
                                         OSalaryDataList = e.SalEarnDedT
                                         .Select(d => new
                                         {
                                             //EarnCode = d.SalaryHead.Code,
                                             EarnHead = d.SalaryHead.Name,
                                             EarnAmount = d.Amount.ToString(),
                                             SalaryHead_Id = d.SalaryHead_Id.ToString(),
                                             //SalHead_FullDetails = d.SalaryHead.FullDetails,
                                             //SalHead_SeqNo = d.SalaryHead.SeqNo.ToString(),
                                             StdAmount = d.StdAmount.ToString(),
                                             LookupVal = d.SalaryHead.Type.LookupVal.ToString(),
                                             InPayslip = d.SalaryHead.InPayslip.ToString(),
                                             SalHeadOperationType = d.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString()
                                         })
                                             // .Where(d => d.EarnAmount != "0" && d.InPayslip == "true").ToList(),
                                          .Where(d => salaryheadappearinpayslipsum.Contains(d.SalaryHead_Id) && d.InPayslip == "true").ToList(),
                                         EPS_Share = e.PFECRR.EPS_Share.ToString(),
                                         ER_Share = e.PFECRR.ER_Share.ToString(),
                                         Arrear_EPS_Share = e.PFECRR.Arrear_EPS_Share.ToString(),
                                         Arrear_ER_Share = e.PFECRR.Arrear_ER_Share.ToString(),
                                         CompAmt = e.LWFTransT.CompAmt.ToString(),
                                         Bankwf = e.SalEarnDedT
                                       .Select(d => new
                                       {
                                           EarnCode = d.SalaryHead.Code,
                                           EarnAmount = d.Amount.ToString(),
                                           InPayslip = d.SalaryHead.InPayslip.ToString(),
                                       })
                                       .Where(d => d.EarnAmount != "0" && d.InPayslip != "true" && d.EarnCode == "BANKWF").Select(z => z.EarnAmount).FirstOrDefault(),

                                     }).ToList();

                                Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                                foreach (var EmpData in EmpDataList)
                                {
                                    int? geoid = Int32.Parse(EmpData.GeoStruct_Id);

                                    int? payid = Int32.Parse(EmpData.PayStruct_Id);

                                    int? funid = Int32.Parse(EmpData.FuncStruct_Id);

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);




                                    double ERSHARE = Convert.ToDouble((EmpData.ER_Share == "" ? "0" : EmpData.ER_Share)) + Convert.ToDouble((EmpData.Arrear_ER_Share == "" ? "0" : EmpData.Arrear_ER_Share));
                                    double EPSSHARE = Convert.ToDouble((EmpData.EPS_Share == "" ? "0" : EmpData.EPS_Share)) + Convert.ToDouble((EmpData.Arrear_EPS_Share == "" ? "0" : EmpData.Arrear_EPS_Share));
                                    double COMPLWF = Convert.ToDouble(EmpData.CompAmt == "" ? "0" : EmpData.CompAmt);
                                    double Bankwf = Convert.ToDouble(EmpData.Bankwf == "" ? "0" : EmpData.Bankwf);
                                    foreach (var OSalaryData in EmpData.OSalaryDataList)
                                    {

                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            //Fld1 = EmpData.Company_Code.ToString(),
                                            //Fld2 = EmpData.Company_Name,
                                            //Fld4 = EmpData.DeptCode,
                                            //Fld5 = EmpData.DeptDesc,
                                            //Fld6 = EmpData.Division_Code,
                                            //Fld7 = EmpData.Division_Name,
                                            Fld8 = OSalaryData.EarnAmount.ToString(),
                                            //Fld9 = OSalaryData.EarnCode,
                                            Fld10 = OSalaryData.EarnHead,
                                            Fld11 = EmpData.EmpCode,
                                            //Fld12 = EmpData.Employee_Id,
                                            //Fld13 = EmpData.EmployeePayroll_Id,
                                            //Fld14 = EmpData.EmpName,
                                            //Fld15 = EmpData.EmpName_Id,
                                            Fld16 = OSalaryData.SalHeadOperationType == "EPF" ? EPSSHARE.ToString() : "0",
                                            Fld17 = OSalaryData.SalHeadOperationType == "EPF" ? ERSHARE.ToString() : "0",
                                            //Fld18 = EmpData.GeoStruct_Id,
                                            //Fld19 = EmpData.Grade_Code,
                                            //Fld20 = EmpData.Grade_Name,
                                            Fld21 = OSalaryData.InPayslip,
                                            //Fld22 = EmpData.Job_Code,
                                            //Fld23 = EmpData.Job_Name,
                                            //Fld24 = EmpData.JobPositionCode,
                                            //Fld25 = EmpData.JobPositionDesc,
                                            Fld26 = EmpData.LocCode,
                                            Fld27 = EmpData.LocDesc,
                                            Fld28 = OSalaryData.LookupVal,
                                            Fld29 = EmpData.PayMonth,
                                            //Fld30 = EmpData.PayStruct_Id,
                                            //Fld31 = EmpData.ProcessDate,
                                            //Fld32 = EmpData.ReleaseDate,
                                            //Fld33 = OSalaryData.SalaryHead_Id,
                                            //Fld34 = OSalaryData.SalHead_FullDetails,
                                            //Fld35 = OSalaryData.SalHead_SeqNo,
                                            Fld36 = OSalaryData.StdAmount,
                                            //Fld37 = EmpData.TotalDeduction,
                                            //Fld38 = EmpData.TotalEarning,
                                            //Fld39 = EmpData.TotalNet,
                                            Fld40 = OSalaryData.SalHeadOperationType == "LWF" ? COMPLWF.ToString() : "0",
                                            Fld41 = OSalaryData.SalHeadOperationType == "BASIC" ? Bankwf.ToString() : "0",
                                        };

                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }


                            }
                            else
                            {

                                var EmpDataList = db.SalaryT.Where(e => e.PayMonth == PayMon && e.IsHold == false && EmpPayrollIdList.Contains(e.EmployeePayroll.Employee_Id.Value)
                                )
                                     .Select(e => new
                                     {
                                         EmpCode = e.EmployeePayroll.Employee.EmpCode,
                                         //EmpName = e.EmployeePayroll.Employee.EmpName.FullNameFML,
                                         //EmpName_Id = e.EmployeePayroll.Employee.EmpName_Id.ToString(),
                                         //Employee_Id = e.EmployeePayroll.Employee.Id.ToString(),
                                         //EmployeePayroll_Id = e.EmployeePayroll_Id.ToString(),
                                         //Grade_Name = e.PayStruct.Grade.Name,
                                         //Grade_Code = e.PayStruct.Grade.Code,
                                         GeoStruct_Id = e.Geostruct_Id.ToString(),
                                         FuncStruct_Id = e.FuncStruct_Id.ToString(),
                                         //Job_Code = e.FuncStruct.Job.Code,
                                         //Job_Name = e.FuncStruct.Job.Name,
                                         PayStruct_Id = e.PayStruct_Id.ToString(),
                                         //JobPositionCode = e.FuncStruct.JobPosition.JobPositionCode,
                                         //JobPositionDesc = e.FuncStruct.JobPosition.JobPositionDesc,
                                         LocCode = e.Geostruct.Location.LocationObj.LocCode,
                                         LocDesc = e.Geostruct.Location.LocationObj.LocDesc + " : " + e.EmployeePayroll.Employee.EmpOffInfo.Bank.Name,
                                         //Division_Code = e.Geostruct.Division.Code,
                                         //Division_Name = e.Geostruct.Division.Name,
                                         //DeptCode = e.Geostruct.Department.DepartmentObj.DeptCode,
                                         //DeptDesc = e.Geostruct.Department.DepartmentObj.DeptDesc,
                                         //Company_Code = e.Geostruct.Company.Code,
                                         //Company_Name = e.Geostruct.Company.Name,
                                         //TotalDeduction = e.TotalDeduction.ToString(),
                                         //TotalEarning = e.TotalEarning.ToString(),
                                         //TotalNet = e.TotalNet.ToString(),
                                         PayMonth = e.PayMonth,
                                         //ProcessDate = e.ProcessDate.ToString(),
                                         //ReleaseDate = e.ReleaseDate.ToString(),
                                         OSalaryDataList = e.SalEarnDedT
                                         .Select(d => new
                                         {
                                             //EarnCode = d.SalaryHead.Code,
                                             EarnHead = d.SalaryHead.Name,
                                             EarnAmount = d.Amount.ToString(),
                                             SalaryHead_Id = d.SalaryHead_Id.ToString(),
                                             //SalHead_FullDetails = d.SalaryHead.FullDetails,
                                             //SalHead_SeqNo = d.SalaryHead.SeqNo.ToString(),
                                             StdAmount = d.StdAmount.ToString(),
                                             LookupVal = d.SalaryHead.Type.LookupVal.ToString(),
                                             InPayslip = d.SalaryHead.InPayslip.ToString(),
                                             SalHeadOperationType = d.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString()
                                         })
                                             // .Where(d => d.EarnAmount != "0" && d.InPayslip == "true").ToList(),
                                          .Where(d => salaryheadappearinpayslipsum.Contains(d.SalaryHead_Id) && d.InPayslip == "true").ToList(),
                                         EPS_Share = e.PFECRR.EPS_Share.ToString(),
                                         ER_Share = e.PFECRR.ER_Share.ToString(),
                                         Arrear_EPS_Share = e.PFECRR.Arrear_EPS_Share.ToString(),
                                         Arrear_ER_Share = e.PFECRR.Arrear_ER_Share.ToString(),
                                         CompAmt = e.LWFTransT.CompAmt.ToString(),
                                         Bankwf = e.SalEarnDedT
                                        .Select(d => new
                                        {
                                            EarnCode = d.SalaryHead.Code,
                                            EarnAmount = d.Amount.ToString(),
                                            InPayslip = d.SalaryHead.InPayslip.ToString(),
                                        })
                                        .Where(d => d.EarnAmount != "0" && d.InPayslip != "true" && d.EarnCode == "BANKWF").Select(z => z.EarnAmount).FirstOrDefault(),

                                     }).ToList();
                                Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                                foreach (var EmpData in EmpDataList)
                                {
                                    int? geoid = Int32.Parse(EmpData.GeoStruct_Id);

                                    int? payid = Int32.Parse(EmpData.PayStruct_Id);

                                    int? funid = Int32.Parse(EmpData.FuncStruct_Id);

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);



                                    double ERSHARE = Convert.ToDouble((EmpData.ER_Share == "" ? "0" : EmpData.ER_Share)) + Convert.ToDouble((EmpData.Arrear_ER_Share == "" ? "0" : EmpData.Arrear_ER_Share));
                                    double EPSSHARE = Convert.ToDouble((EmpData.EPS_Share == "" ? "0" : EmpData.EPS_Share)) + Convert.ToDouble((EmpData.Arrear_EPS_Share == "" ? "0" : EmpData.Arrear_EPS_Share));
                                    double COMPLWF = Convert.ToDouble(EmpData.CompAmt == "" ? "0" : EmpData.CompAmt);
                                    double Bankwf = Convert.ToDouble(EmpData.Bankwf == "" ? "0" : EmpData.Bankwf);

                                    foreach (var OSalaryData in EmpData.OSalaryDataList)
                                    {

                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            //Fld1 = EmpData.Company_Code.ToString(),
                                            //Fld2 = EmpData.Company_Name,
                                            //Fld4 = EmpData.DeptCode,
                                            //Fld5 = EmpData.DeptDesc,
                                            //Fld6 = EmpData.Division_Code,
                                            //Fld7 = EmpData.Division_Name,
                                            Fld8 = OSalaryData.EarnAmount.ToString(),
                                            //  Fld9 = OSalaryData.EarnCode,
                                            Fld10 = OSalaryData.EarnHead,
                                            Fld11 = EmpData.EmpCode,
                                            //Fld12 = EmpData.Employee_Id,
                                            //Fld13 = EmpData.EmployeePayroll_Id,
                                            //Fld14 = EmpData.EmpName,
                                            //Fld15 = EmpData.EmpName_Id,
                                            Fld16 = OSalaryData.SalHeadOperationType == "EPF" ? EPSSHARE.ToString() : "0",
                                            Fld17 = OSalaryData.SalHeadOperationType == "EPF" ? ERSHARE.ToString() : "0",
                                            //Fld18 = EmpData.GeoStruct_Id,
                                            //Fld19 = EmpData.Grade_Code,
                                            //Fld20 = EmpData.Grade_Name,
                                            Fld21 = OSalaryData.InPayslip,
                                            //Fld22 = EmpData.Job_Code,
                                            //Fld23 = EmpData.Job_Name,
                                            //Fld24 = EmpData.JobPositionCode,
                                            //Fld25 = EmpData.JobPositionDesc,
                                            Fld26 = EmpData.LocCode,
                                            Fld27 = EmpData.LocDesc,
                                            Fld28 = OSalaryData.LookupVal,
                                            Fld29 = EmpData.PayMonth,
                                            //Fld30 = EmpData.PayStruct_Id,
                                            //Fld31 = EmpData.ProcessDate,
                                            //Fld32 = EmpData.ReleaseDate,
                                            //Fld33 = OSalaryData.SalaryHead_Id,
                                            //Fld34 = OSalaryData.SalHead_FullDetails,
                                            //Fld35 = OSalaryData.SalHead_SeqNo,
                                            Fld36 = OSalaryData.StdAmount,
                                            //Fld37 = EmpData.TotalDeduction,
                                            //Fld38 = EmpData.TotalEarning,
                                            //Fld39 = EmpData.TotalNet,
                                            Fld40 = OSalaryData.SalHeadOperationType == "LWF" ? COMPLWF.ToString() : "0",
                                            Fld41 = OSalaryData.SalHeadOperationType == "BASIC" ? Bankwf.ToString() : "0",
                                        };

                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }


                            }



                            if (OGenericPayrollStatement != null && OGenericPayrollStatement.Count > 0)
                            {
                                var SpGroup = SpecialGroupslist.ToList();
                                var JOB = false;
                                var GRADE = false;
                                var JOBPOSITION = false;

                                #region Special Group
                                foreach (var item1 in SpGroup)
                                {
                                    if (item1 == "JOB")
                                    {

                                        JOB = true;
                                    }
                                    if (item1 == "GRADE")
                                    {

                                        GRADE = true;
                                    }
                                    if (item1 == "JOB POSITION")
                                    {

                                        JOBPOSITION = true;
                                    }
                                    if (item1 == "DEPARTMENT")
                                    {

                                        JOBPOSITION = true;
                                    }
                                    if (item1 == "LOCATION")
                                    {

                                        JOBPOSITION = true;
                                    }

                                }
                                #endregion Special Group

                              
                                var SpSort = Sortingslist.ToList();
                                var LOCCODE = false;
                                var DEPARTMENTCODE = false;
                                var GRADECODE = false;
                                var DESIGNATIONCODE = false;
                                var EMPCODE = false;
                                foreach (var item1 in SpSort)
                                {
                                    if (item1 == "LOCCODE")
                                    {
                                        LOCCODE = true;
                                    }
                                    if (item1 == "DEPARTMENTCODE")
                                    {
                                        DEPARTMENTCODE = true;
                                    }
                                    if (item1 == "GRADECODE")
                                    {
                                        GRADECODE = true;
                                    }
                                    if (item1 == "DESIGNATIONCODE")
                                    {
                                        DESIGNATIONCODE = true;
                                    }
                                    if (item1 == "EMPCODE")
                                    {
                                        EMPCODE = true;
                                    }
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ThenBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && GRADECODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld19).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld19).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld19).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (GRADECODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ToList();
                                }
                                if (DEPARTMENTCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ToList();
                                }
                                if (GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ToList();
                                }
                                if (DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld22).ToList();
                                }
                                if (EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld11).ToList();
                                }
                            }
                        }
                        return OGenericPayrollStatement;


                        #endregion ReportData Collection and Forwarding to Reporting Object

                    case "SALEARNDEDSUMMARYDETAILS":
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    var OSalaryData_temp = db.EmployeePayroll

                        //    .Where(e => e.Employee_Id == item).AsNoTracking().AsParallel()
                        //    .FirstOrDefault();

                        //    List<string> BusiCategory = new List<string>();
                        //    if (OSalaryData_temp != null)
                        //    {
                        //        if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //        {
                        //            BusiCategory.AddRange(salheadlist);

                        //            foreach (string ca1 in BusiCategory)
                        //            {
                        //                OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                        //                OSalaryData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Include(e => e.Branch).Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                        //                OSalaryData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo.Bank_Id).AsNoTracking().FirstOrDefault();
                        //                OSalaryData_temp.Employee.EmpOffInfo.Branch = db.Branch.Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo.Branch_Id).AsNoTracking().FirstOrDefault();
                        //                OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();
                        //                OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).AsNoTracking().ToList();


                        //                foreach (var item1 in OSalaryData_temp.SalaryT)
                        //                {
                        //                    item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0).AsNoTracking().ToList();
                        //                    foreach (var item2 in item1.SalEarnDedT)
                        //                    {
                        //                        item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                        //                        item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                        //                        item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                        //                    }
                        //                    item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true).ToList();
                        //                    item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                        //                    item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                        //                    item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                        //                    if (item1.Geostruct.Location != null)
                        //                    {
                        //                        item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                        //                    }
                        //                    item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                        //                    if (item1.Geostruct.Department != null)
                        //                    {
                        //                        item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                        //                    }

                        //                    item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                        //                    item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                        //                    item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                        //                    item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                        //                    item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                        //                    if (item1.FuncStruct.Job != null)
                        //                    {
                        //                        item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                        //                    }
                        //                }
                        //                if (OSalaryData_temp.Employee != null && OSalaryData_temp.Employee.EmpOffInfo != null && OSalaryData_temp.Employee.EmpOffInfo.Bank != null)
                        //                {
                        //                    if (OSalaryData_temp.Employee.EmpOffInfo.Bank.Name.ToUpper().ToString() == ca1.ToUpper())
                        //                    {
                        //                        OEmpEarnDedSum.Add(OSalaryData_temp);
                        //                        break;
                        //                    }

                        //                }
                        //            }

                        //        }
                        //        else
                        //        {

                        //            OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                        //            OSalaryData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Include(e => e.Branch).Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                        //            OSalaryData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo.Bank_Id).AsNoTracking().FirstOrDefault();
                        //            OSalaryData_temp.Employee.EmpOffInfo.Branch = db.Branch.Where(e => e.Id == OSalaryData_temp.Employee.EmpOffInfo.Branch_Id).AsNoTracking().FirstOrDefault();
                        //            OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();
                        //            OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).AsNoTracking().ToList();


                        //            foreach (var item1 in OSalaryData_temp.SalaryT)
                        //            {
                        //                item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0).AsNoTracking().ToList();
                        //                foreach (var item2 in item1.SalEarnDedT)
                        //                {
                        //                    item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                        //                    item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                        //                    item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                        //                }
                        //                item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.InPayslip == true).ToList();
                        //                item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                        //                item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                        //                item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                        //                if (item1.Geostruct.Location != null)
                        //                {
                        //                    item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                        //                }
                        //                item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                        //                if (item1.Geostruct.Department != null)
                        //                {
                        //                    item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                        //                }

                        //                item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                        //                item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                        //                item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                        //                item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                        //                item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);

                        //                if (item1.FuncStruct.Job != null)
                        //                {
                        //                    item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                        //                }

                        //            }
                        //            OEmpEarnDedSum.Add(OSalaryData_temp);
                        //        }

                        //    }
                        //}


                        //if (OEmpEarnDedSum == null)
                        //{
                        //    return null;
                        //}
                        //else
                        //{

                        //    var SpGroup = SpecialGroupslist.ToList();
                        //    var JOB = false;
                        //    var GRADE = false;
                        //    var JOBPOSITION = false;


                        //    foreach (var item1 in SpGroup)
                        //    {
                        //        if (item1 == "JOB")
                        //        {

                        //            JOB = true;
                        //        }
                        //        if (item1 == "GRADE")
                        //        {

                        //            GRADE = true;
                        //        }
                        //        if (item1 == "JOB POSITION")
                        //        {

                        //            JOBPOSITION = true;
                        //        }
                        //        if (item1 == "DEPARTMENT")
                        //        {

                        //            JOBPOSITION = true;
                        //        }
                        //        if (item1 == "LOCATION")
                        //        {

                        //            JOBPOSITION = true;
                        //        }

                        //    }

                        //    var dept = false;
                        //    var loca = false;
                        //    var div = false;
                        //    var job = false;

                        //    var filter = "";

                        //    var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).AsNoTracking().ToList();
                        //    var header = "";
                        //    var group = "";
                        //    foreach (var item in vc)
                        //    {
                        //        if (item.LookupVal.ToUpper() == "LOCATION")
                        //        {
                        //            loca = true;
                        //            header = "Location";
                        //        }
                        //        if (item.LookupVal.ToUpper() == "DEPARTMENT")
                        //        {
                        //            dept = true;
                        //            header = "Department";
                        //        }
                        //        if (item.LookupVal.ToUpper() == "DIVISION")
                        //        {
                        //            div = true;
                        //            header = "Division";
                        //        }
                        //        if (item.LookupVal.ToUpper() == "JOB")
                        //        {
                        //            job = true;
                        //            header = "Job";
                        //        }
                        //    }


                        //    Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                        //    List<Utility.GetSalaryDataClass> SalaryData = new List<Utility.GetSalaryDataClass>();

                        //    foreach (var ca in OEmpEarnDedSum)
                        //    {

                        //        SalaryT OSalT = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault();
                        //        if (OSalT != null)
                        //        {
                        //            int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                        //            SalaryData = ReportRDLCObjectClass.GetSalaryData(OSalT, ca.Employee, OSalT.EmployeePayroll_Id);
                        //        }
                        //        if (SalaryData != null)
                        //        {
                        //            var paybank = "";
                        //            if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //            {
                        //                if (ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Bank != null)
                        //                {
                        //                    paybank = ca.Employee.EmpOffInfo.Bank.Name != null ? ca.Employee.EmpOffInfo.Bank.Name.ToString() : "";
                        //                }

                        //            }
                        //            foreach (var item in SalaryData)
                        //            {
                        //                GenericField100 OGenericObjStatement = new GenericField100()
                        //                {
                        //                    Fld1 = item.Company_Code.ToString(),
                        //                    Fld2 = item.Company_Name,
                        //                    Fld4 = item.DeptCode,
                        //                    Fld5 = item.DeptDesc,
                        //                    Fld6 = item.Division_Code,
                        //                    Fld7 = item.Division_Name,
                        //                    Fld8 = item.EarnAmount,
                        //                    Fld9 = item.EarnCode,
                        //                    Fld10 = item.EarnHead,
                        //                    Fld11 = item.EmpCode,
                        //                    Fld12 = item.Employee_Id,
                        //                    Fld13 = item.EmployeePayroll_Id,
                        //                    Fld14 = item.EmpName,
                        //                    Fld15 = item.EmpName_Id,
                        //                    Fld16 = item.EPS_Share,
                        //                    Fld17 = item.ER_Share,
                        //                    Fld18 = item.GeoStruct_Id,
                        //                    Fld19 = item.Grade_Code,
                        //                    Fld20 = item.Grade_Name,
                        //                    Fld21 = item.InPayslip,
                        //                    Fld22 = item.Job_Code,
                        //                    Fld23 = item.Job_Name,
                        //                    Fld24 = item.JobPositionCode,
                        //                    Fld25 = item.JobPositionDesc,
                        //                    Fld26 = item.LocCode,
                        //                    Fld27 = item.LocDesc + " " + paybank,
                        //                    Fld28 = item.LookupVal,
                        //                    Fld29 = item.PayMonth,
                        //                    Fld30 = item.PayStruct_Id,
                        //                    Fld31 = item.ProcessDate,
                        //                    Fld32 = item.ReleaseDate,
                        //                    Fld33 = item.SalaryHead_Id,
                        //                    Fld34 = item.SalHead_FullDetails,
                        //                    Fld35 = item.SalHead_SeqNo,
                        //                    Fld36 = item.StdAmount,
                        //                    Fld37 = item.TotalDeduction,
                        //                    Fld38 = item.TotalEarning,
                        //                    Fld39 = item.TotalNet,
                        //                    Fld51 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Branch != null ? ca.Employee.EmpOffInfo.Branch.Code.ToString() + " " + ca.Employee.EmpOffInfo.Branch.Name.ToString() : "",
                        //                    Fld52 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.AccountNo != null ? ca.Employee.EmpOffInfo.AccountNo.ToString() : "",
                        //                };

                        //                OGenericPayrollStatement.Add(OGenericObjStatement);
                        //            }
                        //        }
                        //    }
                        //    return OGenericPayrollStatement;
                        //}
                        #region ReportData Collection and Forwarding to Reporting Object
                        // zero atendance employee record will show code start
                        List<string> salaryheadappearinpayslip = new List<string>();
                        var EmpDataListzero = db.SalaryT.Where(e => e.PayMonth == PayMon
                            )
                                .Select(e => new
                                {
                                    OSalaryDataList = e.SalEarnDedT
                                   .Select(d => new
                                   {
                                       EarnAmount = d.Amount.ToString(),
                                       SalaryHead_Id = d.SalaryHead_Id.ToString(),
                                       SalHead_SeqNo = d.SalaryHead.SeqNo.ToString(),
                                       LookupVal = d.SalaryHead.Type.LookupVal.ToString(),
                                       InPayslip = d.SalaryHead.InPayslip.ToString(),
                                      
                                   })
                                   .Where(d => d.EarnAmount != "0" && d.InPayslip == "true").ToList()
                                }).AsNoTracking().ToList();
                        foreach (var EmpData in EmpDataListzero)
                        {
                            foreach (var OSalaryData in EmpData.OSalaryDataList.OrderBy(e => e.LookupVal).ThenBy(x => x.SalHead_SeqNo))
                            {
                                salaryheadappearinpayslip.Add(OSalaryData.SalaryHead_Id);
                            }

                        }
                        salaryheadappearinpayslip = salaryheadappearinpayslip.Distinct().ToList();
                        // zero atendance employee record will show code start
                        List<string> BusinessCategory = new List<string>();

                        if (salaryheadappearinpayslip.Count() == 0 )
                        {
                            return null;
                        }
                        else
                        {
                            var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "MONTH")
                                {
                                    month = true;
                                }
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }


                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0) //Business Category Selection i.e. Payment Bank Name
                            {//business category


                                BusinessCategory.AddRange(salheadlist);
                                var EmpDataList = db.SalaryT.Where(e => e.PayMonth == PayMon && BusinessCategory.Contains(e.EmployeePayroll.Employee.EmpOffInfo.Bank.Name.ToString()) && EmpPayrollIdList.Contains(e.EmployeePayroll.Employee_Id.Value))
                                     .Select(e => new
                                     {
                                         EmpCode = e.EmployeePayroll.Employee.EmpCode,
                                         EmpName = e.EmployeePayroll.Employee.EmpName.FullNameFML,
                                         EmpName_Id = e.EmployeePayroll.Employee.EmpName_Id.ToString(),
                                         Employee_Id = e.EmployeePayroll.Employee.Id.ToString(),
                                         BranchCode = e.EmployeePayroll.Employee.EmpOffInfo.Branch.Code,
                                         BranchName = e.EmployeePayroll.Employee.EmpOffInfo.Branch.Name,
                                         AccountNo = e.EmployeePayroll.Employee.EmpOffInfo.AccountNo,
                                         EmployeePayroll_Id = e.EmployeePayroll_Id.ToString(),
                                         Grade_Name = e.PayStruct.Grade.Name,
                                         Grade_Code = e.PayStruct.Grade.Code,
                                         GeoStruct_Id = e.Geostruct_Id.ToString(),
                                         FuncStruct_Id = e.FuncStruct_Id.ToString(),
                                         Job_Code = e.FuncStruct.Job.Code,
                                         Job_Name = e.FuncStruct.Job.Name,
                                         PayStruct_Id = e.PayStruct_Id.ToString(),
                                         JobPositionCode = e.FuncStruct.JobPosition.JobPositionCode,
                                         JobPositionDesc = e.FuncStruct.JobPosition.JobPositionDesc,
                                         LocCode = e.Geostruct.Location.LocationObj.LocCode,
                                         LocDesc = e.Geostruct.Location.LocationObj.LocDesc + " : " + e.EmployeePayroll.Employee.EmpOffInfo.Bank.Name,
                                         Division_Code = e.Geostruct.Division.Code,
                                         Division_Name = e.Geostruct.Division.Name,
                                         DeptCode = e.Geostruct.Department.DepartmentObj.DeptCode,
                                         DeptDesc = e.Geostruct.Department.DepartmentObj.DeptDesc,
                                         Company_Code = e.Geostruct.Company.Code,
                                         Company_Name = e.Geostruct.Company.Name,
                                         TotalDeduction = e.TotalDeduction.ToString(),
                                         TotalEarning = e.TotalEarning.ToString(),
                                         TotalNet = e.TotalNet.ToString(),
                                         PayMonth = e.PayMonth,
                                         ProcessDate = e.ProcessDate.ToString(),
                                         ReleaseDate = e.ReleaseDate.ToString(),
                                         PaybleDays = e.PaybleDays.ToString(),
                                         OSalaryDataList = e.SalEarnDedT
                                         .Select(d => new
                                         {
                                             EarnCode = d.SalaryHead.Code,
                                             EarnHead = d.SalaryHead.Name,
                                             EarnAmount = d.Amount.ToString(),
                                             SalaryHead_Id = d.SalaryHead_Id.ToString(),
                                             SalHead_FullDetails = d.SalaryHead.FullDetails,
                                             SalHead_SeqNo = d.SalaryHead.SeqNo.ToString(),
                                             StdAmount = d.StdAmount.ToString(),
                                             LookupVal = d.SalaryHead.Type.LookupVal.ToString(),
                                             InPayslip = d.SalaryHead.InPayslip.ToString(),
                                             SalHeadOperationType = d.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString()
                                         })
                                             // .Where(d => d.EarnAmount != "0" && d.InPayslip == "true").ToList(),
                                         .Where(d => salaryheadappearinpayslip.Contains(d.SalaryHead_Id) && d.InPayslip == "true").ToList(),
                                         EPS_Share = e.PFECRR.EPS_Share.ToString(),
                                         ER_Share = e.PFECRR.ER_Share.ToString(),
                                         Arrear_EPS_Share = e.PFECRR.Arrear_EPS_Share.ToString(),
                                         Arrear_ER_Share = e.PFECRR.Arrear_ER_Share.ToString(),
                                         CompAmt = e.LWFTransT.CompAmt.ToString(),
                                         Bankwf = e.SalEarnDedT
                                         .Select(d => new
                                         {
                                             EarnCode = d.SalaryHead.Code,
                                             EarnAmount = d.Amount.ToString(),
                                             InPayslip = d.SalaryHead.InPayslip.ToString(),
                                         })
                                         .Where(d => d.EarnAmount != "0" && d.InPayslip != "true" && d.EarnCode == "BANKWF").Select(z => z.EarnAmount).FirstOrDefault(),

                                     }).AsNoTracking().ToList();





                                Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                                foreach (var EmpData in EmpDataList)
                                {
                                    int? geoid = Int32.Parse(EmpData.GeoStruct_Id);

                                    int? payid = Int32.Parse(EmpData.PayStruct_Id);

                                    int? funid = Int32.Parse(EmpData.FuncStruct_Id);

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    double ERSHARE = Convert.ToDouble((EmpData.ER_Share == "" ? "0" : EmpData.ER_Share)) + Convert.ToDouble((EmpData.Arrear_ER_Share == "" ? "0" : EmpData.Arrear_ER_Share));
                                    double EPSSHARE = Convert.ToDouble((EmpData.EPS_Share == "" ? "0" : EmpData.EPS_Share)) + Convert.ToDouble((EmpData.Arrear_EPS_Share == "" ? "0" : EmpData.Arrear_EPS_Share));
                                    var actbasic = EmpData.OSalaryDataList.Where(e => e.SalHeadOperationType == "BASIC").FirstOrDefault();
                                    string Actualbasic = "0";
                                    if (actbasic != null)
                                    {
                                        Actualbasic = actbasic.StdAmount.ToString();
                                    }



                                    foreach (var OSalaryData in EmpData.OSalaryDataList.OrderBy(e => e.LookupVal).ThenBy(x => x.SalHead_SeqNo))
                                    {


                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = EmpData.Company_Code.ToString(),
                                            Fld2 = EmpData.Company_Name,
                                            Fld4 = EmpData.DeptCode,
                                            Fld5 = EmpData.DeptDesc,
                                            Fld6 = EmpData.Division_Code,
                                            Fld7 = EmpData.Division_Name,
                                            Fld8 = OSalaryData.EarnAmount.ToString(),
                                            Fld9 = OSalaryData.EarnCode,
                                            Fld10 = OSalaryData.EarnHead,
                                            Fld11 = EmpData.EmpCode,
                                            Fld12 = EmpData.Employee_Id,
                                            Fld13 = EmpData.EmployeePayroll_Id,
                                            Fld14 = EmpData.EmpName,
                                            Fld15 = EmpData.EmpName_Id,
                                            Fld16 = EPSSHARE.ToString(),
                                            Fld17 = ERSHARE.ToString(),
                                            Fld18 = EmpData.GeoStruct_Id,
                                            Fld19 = EmpData.Grade_Code,
                                            Fld20 = EmpData.Grade_Name,
                                            Fld21 = OSalaryData.InPayslip,
                                            Fld22 = EmpData.Job_Code,
                                            Fld23 = EmpData.Job_Name,
                                            Fld24 = EmpData.JobPositionCode,
                                            Fld25 = EmpData.JobPositionDesc,
                                            Fld26 = EmpData.LocCode,
                                            Fld27 = EmpData.LocDesc,
                                            Fld28 = OSalaryData.LookupVal,
                                            Fld29 = EmpData.PayMonth,
                                            Fld30 = EmpData.PayStruct_Id,
                                            Fld31 = EmpData.ProcessDate,
                                            Fld32 = EmpData.ReleaseDate,
                                            Fld33 = OSalaryData.SalaryHead_Id,
                                            Fld34 = OSalaryData.SalHead_FullDetails,
                                            Fld35 = OSalaryData.SalHead_SeqNo,
                                            Fld36 = OSalaryData.StdAmount,
                                            Fld37 = EmpData.TotalDeduction,
                                            Fld38 = EmpData.TotalEarning,
                                            Fld39 = EmpData.TotalNet,
                                            Fld51 = EmpData.BranchCode != null ? EmpData.BranchCode.ToString() + " " + EmpData.BranchName.ToString() : "",
                                            Fld52 = EmpData.AccountNo,
                                            Fld53 = Actualbasic,
                                            Fld54 = EmpData.CompAmt == null ? "0" : EmpData.CompAmt,
                                            Fld55 = EmpData.Bankwf == null ? "0" : EmpData.Bankwf,
                                            Fld56 = EmpData.PaybleDays,
                                        };

                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }


                            }
                            else
                            {

                                var EmpDataList = db.SalaryT.Where(e => e.PayMonth == PayMon && EmpPayrollIdList.Contains(e.EmployeePayroll.Employee_Id.Value)
                                 )
                                     .Select(e => new
                                     {
                                         EmpCode = e.EmployeePayroll.Employee.EmpCode,
                                         EmpName = e.EmployeePayroll.Employee.EmpName.FullNameFML,
                                         EmpName_Id = e.EmployeePayroll.Employee.EmpName_Id.ToString(),
                                         Employee_Id = e.EmployeePayroll.Employee.Id.ToString(),
                                         BranchCode = e.EmployeePayroll.Employee.EmpOffInfo.Branch.Code,
                                         BranchName = e.EmployeePayroll.Employee.EmpOffInfo.Branch.Name,
                                         AccountNo = e.EmployeePayroll.Employee.EmpOffInfo.AccountNo,
                                         EmployeePayroll_Id = e.EmployeePayroll_Id.ToString(),
                                         Grade_Name = e.PayStruct.Grade.Name,
                                         Grade_Code = e.PayStruct.Grade.Code,
                                         GeoStruct_Id = e.Geostruct_Id.ToString(),
                                         FuncStruct_Id = e.FuncStruct_Id.ToString(),
                                         Job_Code = e.FuncStruct.Job.Code,
                                         Job_Name = e.FuncStruct.Job.Name,
                                         PayStruct_Id = e.PayStruct_Id.ToString(),
                                         JobPositionCode = e.FuncStruct.JobPosition.JobPositionCode,
                                         JobPositionDesc = e.FuncStruct.JobPosition.JobPositionDesc,
                                         LocCode = e.Geostruct.Location.LocationObj.LocCode,
                                         LocDesc = e.Geostruct.Location.LocationObj.LocDesc + " : " + e.EmployeePayroll.Employee.EmpOffInfo.Bank.Name,
                                         Division_Code = e.Geostruct.Division.Code,
                                         Division_Name = e.Geostruct.Division.Name,
                                         DeptCode = e.Geostruct.Department.DepartmentObj.DeptCode,
                                         DeptDesc = e.Geostruct.Department.DepartmentObj.DeptDesc,
                                         Company_Code = e.Geostruct.Company.Code,
                                         Company_Name = e.Geostruct.Company.Name,
                                         TotalDeduction = e.TotalDeduction.ToString(),
                                         TotalEarning = e.TotalEarning.ToString(),
                                         TotalNet = e.TotalNet.ToString(),
                                         PayMonth = e.PayMonth,
                                         ProcessDate = e.ProcessDate.ToString(),
                                         ReleaseDate = e.ReleaseDate.ToString(),
                                         PaybleDays = e.PaybleDays.ToString(),
                                         OSalaryDataList = e.SalEarnDedT
                                         .Select(d => new
                                         {
                                             EarnCode = d.SalaryHead.Code,
                                             EarnHead = d.SalaryHead.Name,
                                             EarnAmount = d.Amount.ToString(),
                                             SalaryHead_Id = d.SalaryHead_Id.ToString(),
                                             SalHead_FullDetails = d.SalaryHead.FullDetails,
                                             SalHead_SeqNo = d.SalaryHead.SeqNo.ToString(),
                                             StdAmount = d.StdAmount.ToString(),
                                             LookupVal = d.SalaryHead.Type.LookupVal.ToString(),
                                             InPayslip = d.SalaryHead.InPayslip.ToString(),
                                             SalHeadOperationType = d.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString()
                                         })
                                             // .Where(d => d.EarnAmount != "0" && d.InPayslip == "true").ToList(),
                                          .Where(d => salaryheadappearinpayslip.Contains(d.SalaryHead_Id) && d.InPayslip == "true").ToList(),
                                         EPS_Share = e.PFECRR.EPS_Share.ToString(),
                                         ER_Share = e.PFECRR.ER_Share.ToString(),
                                         Arrear_EPS_Share = e.PFECRR.Arrear_EPS_Share.ToString(),
                                         Arrear_ER_Share = e.PFECRR.Arrear_ER_Share.ToString(),
                                         CompAmt = e.LWFTransT.CompAmt.ToString(),
                                         Bankwf = e.SalEarnDedT
                                        .Select(d => new
                                        {
                                            EarnCode = d.SalaryHead.Code,
                                            EarnAmount = d.Amount.ToString(),
                                            InPayslip = d.SalaryHead.InPayslip.ToString(),
                                        })
                                        .Where(d => d.EarnAmount != "0" && d.InPayslip != "true" && d.EarnCode == "BANKWF").Select(z => z.EarnAmount).FirstOrDefault(),

                                     }).AsNoTracking().ToList();

                                Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                                foreach (var EmpData in EmpDataList)
                                {
                                    int? geoid = Int32.Parse(EmpData.GeoStruct_Id);

                                    int? payid = Int32.Parse(EmpData.PayStruct_Id);

                                    int? funid = Int32.Parse(EmpData.FuncStruct_Id);

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);




                                    double ERSHARE = Convert.ToDouble((EmpData.ER_Share == "" ? "0" : EmpData.ER_Share)) + Convert.ToDouble((EmpData.Arrear_ER_Share == "" ? "0" : EmpData.Arrear_ER_Share));
                                    double EPSSHARE = Convert.ToDouble((EmpData.EPS_Share == "" ? "0" : EmpData.EPS_Share)) + Convert.ToDouble((EmpData.Arrear_EPS_Share == "" ? "0" : EmpData.Arrear_EPS_Share));
                                    var actbasic = EmpData.OSalaryDataList.Where(e => e.SalHeadOperationType == "BASIC").FirstOrDefault();
                                    string Actualbasic = "0";
                                    if (actbasic != null)
                                    {
                                        Actualbasic = actbasic.StdAmount.ToString();
                                    }
                                    foreach (var OSalaryData in EmpData.OSalaryDataList.OrderBy(e => e.LookupVal).ThenBy(x => x.SalHead_SeqNo))
                                    {
                                        


                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = EmpData.Company_Code.ToString(),
                                            Fld2 = EmpData.Company_Name,
                                            Fld4 = EmpData.DeptCode,
                                            Fld5 = EmpData.DeptDesc,
                                            Fld6 = EmpData.Division_Code,
                                            Fld7 = EmpData.Division_Name,
                                            Fld8 = OSalaryData.EarnAmount.ToString(),
                                            Fld9 = OSalaryData.EarnCode,
                                            Fld10 = OSalaryData.EarnHead,
                                            Fld11 = EmpData.EmpCode,
                                            Fld12 = EmpData.Employee_Id,
                                            Fld13 = EmpData.EmployeePayroll_Id,
                                            Fld14 = EmpData.EmpName,
                                            Fld15 = EmpData.EmpName_Id,
                                            Fld16 = EPSSHARE.ToString(),
                                            Fld17 = ERSHARE.ToString(),
                                            Fld18 = EmpData.GeoStruct_Id,
                                            Fld19 = EmpData.Grade_Code,
                                            Fld20 = EmpData.Grade_Name,
                                            Fld21 = OSalaryData.InPayslip,
                                            Fld22 = EmpData.Job_Code,
                                            Fld23 = EmpData.Job_Name,
                                            Fld24 = EmpData.JobPositionCode,
                                            Fld25 = EmpData.JobPositionDesc,
                                            Fld26 = EmpData.LocCode,
                                            Fld27 = EmpData.LocDesc,
                                            Fld28 = OSalaryData.LookupVal,
                                            Fld29 = EmpData.PayMonth,
                                            Fld30 = EmpData.PayStruct_Id,
                                            Fld31 = EmpData.ProcessDate,
                                            Fld32 = EmpData.ReleaseDate,
                                            Fld33 = OSalaryData.SalaryHead_Id,
                                            Fld34 = OSalaryData.SalHead_FullDetails,
                                            Fld35 = OSalaryData.SalHead_SeqNo,
                                            Fld36 = OSalaryData.StdAmount,
                                            Fld37 = EmpData.TotalDeduction,
                                            Fld38 = EmpData.TotalEarning,
                                            Fld39 = EmpData.TotalNet,
                                            Fld51 = EmpData.BranchCode != null ? EmpData.BranchCode.ToString() + " " + EmpData.BranchName.ToString() : "",
                                            Fld52 = EmpData.AccountNo,
                                            Fld53 = Actualbasic,
                                            Fld54 = EmpData.CompAmt == null ? "0" : EmpData.CompAmt,
                                            Fld55 = EmpData.Bankwf == null ? "0" : EmpData.Bankwf,
                                            Fld56 = EmpData.PaybleDays,
                                        };

                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }


                            }

                            if (OGenericPayrollStatement != null && OGenericPayrollStatement.Count > 0)
                            {


                                ////
                                var SpSort = Sortingslist.ToList();
                                var LOCCODE = false;
                                var DEPARTMENTCODE = false;
                                var GRADECODE = false;
                                var DESIGNATIONCODE = false;
                                var EMPCODE = false;
                                foreach (var item1 in SpSort)
                                {
                                    if (item1 == "LOCCODE")
                                    {
                                        LOCCODE = true;
                                    }
                                    if (item1 == "DEPARTMENTCODE")
                                    {
                                        DEPARTMENTCODE = true;
                                    }
                                    if (item1 == "GRADECODE")
                                    {
                                        GRADECODE = true;
                                    }
                                    if (item1 == "DESIGNATIONCODE")
                                    {
                                        DESIGNATIONCODE = true;
                                    }
                                    if (item1 == "EMPCODE")
                                    {
                                        EMPCODE = true;
                                    }
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ThenBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld19).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && GRADECODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld19).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld19).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld19).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true && DEPARTMENTCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ThenBy(e => e.Fld4).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (GRADECODE == true && DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ThenBy(e => e.Fld22).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (GRADECODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (DESIGNATIONCODE == true && EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld22).ThenBy(e => e.Fld11).ToList();
                                    return OGenericPayrollStatement.ToList();
                                }
                                if (LOCCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld26).ToList();
                                }
                                if (DEPARTMENTCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld4).ToList();
                                }
                                if (GRADECODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld19).ToList();
                                }
                                if (DESIGNATIONCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld22).ToList();
                                }
                                if (EMPCODE == true)
                                {
                                    OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld11).ToList();
                                }

                            }
                        }
                        return OGenericPayrollStatement;


                        #endregion ReportData Collection and Forwarding to Reporting Object

                    case "SALEARNSUMMARY":
                        OEmpPayrollIdList = new List<int?>();
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();
                        OEmpPayrollIdList = db.SalaryT.Include(e => e.PFECRR).Where(e => e.PayMonth == PayMon).Select(r => r.EmployeePayroll_Id).ToList();
                        foreach (var item in OEmpPayrollIdList)
                        {
                            var OSalaryData_temp = db.EmployeePayroll

                            //  .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.FunctAttendanceT)
                                //.Include(r => r.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.PayStruct))
                            .Where(e => e.Employee_Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();


                            if (OSalaryData_temp != null)
                            {

                                OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                                OSalaryData_temp.Employee.EmpOffInfo = db.EmpOff.Find(OSalaryData_temp.Employee.EmpOffInfo_Id);
                                OSalaryData_temp.Employee.EmpOffInfo.Bank = db.Bank.Find(OSalaryData_temp.Employee.EmpOffInfo.Bank_Id);
                                OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).FirstOrDefault();
                                OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).ToList();

                                foreach (var item1 in OSalaryData_temp.SalaryT)
                                {
                                    item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0 && salheadlist.Contains(e.SalaryHead.Code)).ToList();
                                    foreach (var item2 in item1.SalEarnDedT)
                                    {
                                        item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                                        item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                                        item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                                    }
                                    item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                    item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                                    item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                                    item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                                    if (item1.Geostruct.Location != null)
                                    {
                                        item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                                    }
                                    item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                                    if (item1.Geostruct.Department != null)
                                    {
                                        item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                                    }

                                    item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                                    item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                                    item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                                    item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                                    item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                                    if (item1.FuncStruct.Job != null)
                                    {
                                        item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                                    }
                                }

                                OEmpEarnDedSum.Add(OSalaryData_temp);
                            }
                        }
                        if (loanadvidlist.Count()!=0)
                        {
                            OEmpEarnDedSum = OEmpEarnDedSum.Where(e => loanadvidlist.Contains(e.Employee.EmpOffInfo.Bank.Name)).ToList();
                        }
                        else
                        {
                            OEmpEarnDedSum = OEmpEarnDedSum.ToList();    
                        }

                        if (OEmpEarnDedSum == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }



                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetSalaryDataClass> SalaryData = new List<Utility.GetSalaryDataClass>();


                            foreach (var ca in OEmpEarnDedSum)
                            {

                                SalaryT OSalT = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                if (OSalT != null)
                                {
                                    int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                                    SalaryData = ReportRDLCObjectClass.GetSalaryData(OSalT, ca.Employee, OSalT.EmployeePayroll_Id);
                                }


                                if (SalaryData != null)
                                {
                                    foreach (var item in SalaryData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = item.Company_Code.ToString(),
                                            Fld2 = item.Company_Name,
                                            Fld4 = item.DeptCode,
                                            Fld5 = item.DeptDesc,
                                            Fld6 = item.Division_Code,
                                            Fld7 = item.Division_Name,
                                            Fld8 = item.EarnAmount,
                                            Fld9 = item.EarnCode,
                                            Fld10 = item.EarnHead,
                                            Fld11 = item.EmpCode,
                                            Fld12 = item.Employee_Id,
                                            Fld13 = item.EmployeePayroll_Id,
                                            Fld14 = item.EmpName,
                                            Fld15 = item.EmpName_Id,
                                            Fld16 = item.EPS_Share,
                                            Fld17 = item.ER_Share,
                                            Fld18 = item.GeoStruct_Id,
                                            Fld19 = item.Grade_Code,
                                            Fld20 = item.Grade_Name,
                                            Fld21 = item.InPayslip,
                                            Fld22 = item.Job_Code,
                                            Fld23 = item.Job_Name,
                                            Fld24 = item.JobPositionCode,
                                            Fld25 = item.JobPositionDesc,
                                            Fld26 = item.LocCode,
                                            Fld27 = item.LocDesc,
                                            Fld28 = item.LookupVal,
                                            Fld29 = item.PayMonth,
                                            Fld30 = item.PayStruct_Id,
                                            Fld31 = item.ProcessDate,
                                            Fld32 = item.ReleaseDate,
                                            Fld33 = item.SalaryHead_Id,
                                            Fld34 = item.SalHead_FullDetails,
                                            Fld35 = item.SalHead_SeqNo,
                                            Fld36 = item.StdAmount,
                                            Fld37 = item.TotalDeduction,
                                            Fld38 = item.TotalEarning,
                                            Fld39 = item.TotalNet,
                                        };

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                    case "SALEARNSUMMARYDETAILS":
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OSalaryData_temp = db.EmployeePayroll

                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.FunctAttendanceT)
                                //.Include(r => r.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.PayStruct))
                            .Where(e => e.Employee_Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();


                            if (OSalaryData_temp != null)
                            {
                                OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                                OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).FirstOrDefault();
                                OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                                foreach (var item1 in OSalaryData_temp.SalaryT)
                                {
                                    item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0).ToList();
                                    foreach (var item2 in item1.SalEarnDedT)
                                    {
                                        item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                                        item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                                        item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                                    }
                                    item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").OrderBy(e => e.SalaryHead.SeqNo).AsParallel().ToList();
                                    item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                                    item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                                    item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                                    if (item1.Geostruct.Location != null)
                                    {
                                        item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                                    }
                                    item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                                    if (item1.Geostruct.Department != null)
                                    {
                                        item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                                    }

                                    item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                                    item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                                    item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                                    item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                                    item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                                    if (item1.FuncStruct.Job != null)
                                    {
                                        item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                                    }
                                }

                                OEmpEarnDedSum.Add(OSalaryData_temp);
                            }
                        }


                        if (OEmpEarnDedSum == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetSalaryDataClass> SalaryData = new List<Utility.GetSalaryDataClass>();

                            foreach (var ca in OEmpEarnDedSum)
                            {
                                SalaryT OSalT = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                if (OSalT != null)
                                {
                                    //int SalId = OSalT.Id;
                                    SalaryData = ReportRDLCObjectClass.GetSalaryData(OSalT, ca.Employee, OSalT.EmployeePayroll_Id);

                                    if (SalaryData != null)
                                    {
                                        foreach (var item in SalaryData)
                                        {
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            {
                                                Fld1 = item.Company_Code.ToString(),
                                                Fld2 = item.Company_Name,
                                                Fld4 = item.DeptCode,
                                                Fld5 = item.DeptDesc,
                                                Fld6 = item.Division_Code,
                                                Fld7 = item.Division_Name,
                                                Fld8 = item.EarnAmount,
                                                Fld9 = item.EarnCode,
                                                Fld10 = item.EarnHead,
                                                Fld11 = item.EmpCode,
                                                Fld12 = item.Employee_Id,
                                                Fld13 = item.EmployeePayroll_Id,
                                                Fld14 = item.EmpName,
                                                Fld15 = item.EmpName_Id,
                                                Fld16 = item.EPS_Share,
                                                Fld17 = item.ER_Share,
                                                Fld18 = item.GeoStruct_Id,
                                                Fld19 = item.Grade_Code,
                                                Fld20 = item.Grade_Name,
                                                Fld21 = item.InPayslip,
                                                Fld22 = item.Job_Code,
                                                Fld23 = item.Job_Name,
                                                Fld24 = item.JobPositionCode,
                                                Fld25 = item.JobPositionDesc,
                                                Fld26 = item.LocCode,
                                                Fld27 = item.LocDesc,
                                                Fld28 = item.LookupVal,
                                                Fld29 = item.PayMonth,
                                                Fld30 = item.PayStruct_Id,
                                                Fld31 = item.ProcessDate,
                                                Fld32 = item.ReleaseDate,
                                                Fld33 = item.SalaryHead_Id,
                                                Fld34 = item.SalHead_FullDetails,
                                                Fld35 = item.SalHead_SeqNo,
                                                Fld36 = item.StdAmount,
                                                Fld37 = item.TotalDeduction,
                                                Fld38 = item.TotalEarning,
                                                Fld39 = item.TotalNet,
                                            };

                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }

                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }


                    case "SALDEDSUMMARY":
                        OEmpPayrollIdList = new List<int?>();
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();
                        OEmpPayrollIdList = db.SalaryT.Where(e => e.PayMonth == PayMon).Select(r => r.EmployeePayroll_Id).ToList();
                        foreach (var item in OEmpPayrollIdList)
                        {
                            var OSalaryData_temp = db.EmployeePayroll

                            //  .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.FunctAttendanceT)
                                //.Include(r => r.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.PayStruct))
                            .Where(e => e.Employee_Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();

                            if (OSalaryData_temp != null)
                            {
                                OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                                OSalaryData_temp.Employee.EmpOffInfo = db.EmpOff.Find(OSalaryData_temp.Employee.EmpOffInfo_Id);
                                OSalaryData_temp.Employee.EmpOffInfo.Bank = db.Bank.Find(OSalaryData_temp.Employee.EmpOffInfo.Bank_Id);                                
                                OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).FirstOrDefault();
                                OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).ToList();

                                foreach (var item1 in OSalaryData_temp.SalaryT)
                                {
                                    item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0 && salheadlist.Contains(e.SalaryHead.Name)).ToList();
                                    foreach (var item2 in item1.SalEarnDedT)
                                    {
                                        item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                                        item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                                        item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                                    }
                                    item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                    item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                                    item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                                    item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                                    if (item1.Geostruct.Location != null)
                                    {
                                        item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                                    }
                                    item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                                    if (item1.Geostruct.Department != null)
                                    {
                                        item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                                    }

                                    item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                                    item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                                    item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                                    item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                                    item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                                    if (item1.FuncStruct.Job != null)
                                    {
                                        item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                                    }
                                }

                                OEmpEarnDedSum.Add(OSalaryData_temp);
                            }
                        }

                        if (loanadvidlist.Count()!=0)
                        {
                            OEmpEarnDedSum = OEmpEarnDedSum.Where(e => loanadvidlist.Contains(e.Employee.EmpOffInfo.Bank.Name)).ToList();
                        }
                        else
                        {
                            OEmpEarnDedSum = OEmpEarnDedSum.ToList();
                        }
                        if (OEmpEarnDedSum == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetSalaryDataClass> SalaryData = new List<Utility.GetSalaryDataClass>();


                            foreach (var ca in OEmpEarnDedSum)
                            {



                                SalaryT OSalT = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                if (OSalT != null)
                                {
                                    int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                                    SalaryData = ReportRDLCObjectClass.GetSalaryData(OSalT, ca.Employee, OSalT.EmployeePayroll_Id);
                                }


                                if (SalaryData != null)
                                {
                                    foreach (var item in SalaryData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = item.Company_Code.ToString(),
                                            Fld2 = item.Company_Name,
                                            Fld4 = item.DeptCode,
                                            Fld5 = item.DeptDesc,
                                            Fld6 = item.Division_Code,
                                            Fld7 = item.Division_Name,
                                            Fld8 = item.EarnAmount,
                                            Fld9 = item.EarnCode,
                                            Fld10 = item.EarnHead,
                                            Fld11 = item.EmpCode,
                                            Fld12 = item.Employee_Id,
                                            Fld13 = item.EmployeePayroll_Id,
                                            Fld14 = item.EmpName,
                                            Fld15 = item.EmpName_Id,
                                            Fld16 = item.EPS_Share,
                                            Fld17 = item.ER_Share,
                                            Fld18 = item.GeoStruct_Id,
                                            Fld19 = item.Grade_Code,
                                            Fld20 = item.Grade_Name,
                                            Fld21 = item.InPayslip,
                                            Fld22 = item.Job_Code,
                                            Fld23 = item.Job_Name,
                                            Fld24 = item.JobPositionCode,
                                            Fld25 = item.JobPositionDesc,
                                            Fld26 = item.LocCode,
                                            Fld27 = item.LocDesc,
                                            Fld28 = item.LookupVal,
                                            Fld29 = item.PayMonth,
                                            Fld30 = item.PayStruct_Id,
                                            Fld31 = item.ProcessDate,
                                            Fld32 = item.ReleaseDate,
                                            Fld33 = item.SalaryHead_Id,
                                            Fld34 = item.SalHead_FullDetails,
                                            Fld35 = item.SalHead_SeqNo,
                                            Fld36 = item.StdAmount,
                                            Fld37 = item.TotalDeduction,
                                            Fld38 = item.TotalEarning,
                                            Fld39 = item.TotalNet,
                                        };

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }


                    case "SALDEDSUMMARYDETAILS":
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OSalaryData_temp = db.EmployeePayroll

                            //  .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.FunctAttendanceT)
                                //.Include(r => r.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.PayStruct))
                            .Where(e => e.Employee_Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();

                            if (OSalaryData_temp != null)
                            {
                                OSalaryData_temp.Employee = db.Employee.Find(OSalaryData_temp.Employee_Id);
                                OSalaryData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalaryData_temp.Employee.EmpName_Id).FirstOrDefault();
                                OSalaryData_temp.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).ToList();

                                foreach (var item1 in OSalaryData_temp.SalaryT)
                                {
                                    item1.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == item1.Id && e.Amount != 0).ToList();
                                    foreach (var item2 in item1.SalEarnDedT)
                                    {
                                        item2.SalaryHead = db.SalaryHead.Find(item2.SalaryHead_Id);
                                        item2.SalaryHead.Type = db.LookupValue.Find(item2.SalaryHead.Type_Id);
                                        item2.SalaryHead.ProcessType = db.LookupValue.Find(item2.SalaryHead.ProcessType_Id);
                                    }
                                    item1.SalEarnDedT = item1.SalEarnDedT.Where(e => e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").OrderBy(e => e.SalaryHead.SeqNo).ToList();
                                    item1.Geostruct = db.GeoStruct.Find(item1.Geostruct_Id);
                                    item1.Geostruct.Company = db.Company.Find(item1.Geostruct.Company_Id);
                                    item1.Geostruct.Location = db.Location.Find(item1.Geostruct.Location_Id);
                                    if (item1.Geostruct.Location != null)
                                    {
                                        item1.Geostruct.Location.LocationObj = db.LocationObj.Find(item1.Geostruct.Location.LocationObj_Id);
                                    }
                                    item1.Geostruct.Department = db.Department.Find(item1.Geostruct.Department_Id);
                                    if (item1.Geostruct.Department != null)
                                    {
                                        item1.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(item1.Geostruct.Department.DepartmentObj_Id);
                                    }

                                    item1.Geostruct.Division = db.Division.Find(item1.Geostruct.Division_Id);
                                    item1.PayStruct = db.PayStruct.Find(item1.PayStruct_Id);
                                    item1.PayStruct.Grade = db.Grade.Find(item1.PayStruct.Grade_Id);
                                    item1.FuncStruct = db.FuncStruct.Find(item1.FuncStruct_Id);
                                    item1.FuncStruct.Job = db.Job.Find(item1.FuncStruct.Job_Id);
                                    if (item1.FuncStruct.Job != null)
                                    {
                                        item1.FuncStruct.JobPosition = db.JobPosition.Find(item1.FuncStruct.JobPosition_Id);
                                    }
                                }

                                OEmpEarnDedSum.Add(OSalaryData_temp);
                            }
                        }


                        if (OEmpEarnDedSum == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetSalaryDataClass> SalaryData = new List<Utility.GetSalaryDataClass>();

                            foreach (var ca in OEmpEarnDedSum)
                            {
                                SalaryT OSalT = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                if (OSalT != null)
                                {
                                    int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                                    SalaryData = ReportRDLCObjectClass.GetSalaryData(OSalT, ca.Employee, OSalT.EmployeePayroll_Id);
                                }


                                if (SalaryData != null)
                                {
                                    foreach (var item in SalaryData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = item.Company_Code.ToString(),
                                            Fld2 = item.Company_Name,
                                            Fld4 = item.DeptCode,
                                            Fld5 = item.DeptDesc,
                                            Fld6 = item.Division_Code,
                                            Fld7 = item.Division_Name,
                                            Fld8 = item.EarnAmount,
                                            Fld9 = item.EarnCode,
                                            Fld10 = item.EarnHead,
                                            Fld11 = item.EmpCode,
                                            Fld12 = item.Employee_Id,
                                            Fld13 = item.EmployeePayroll_Id,
                                            Fld14 = item.EmpName,
                                            Fld15 = item.EmpName_Id,
                                            Fld16 = item.EPS_Share,
                                            Fld17 = item.ER_Share,
                                            Fld18 = item.GeoStruct_Id,
                                            Fld19 = item.Grade_Code,
                                            Fld20 = item.Grade_Name,
                                            Fld21 = item.InPayslip,
                                            Fld22 = item.Job_Code,
                                            Fld23 = item.Job_Name,
                                            Fld24 = item.JobPositionCode,
                                            Fld25 = item.JobPositionDesc,
                                            Fld26 = item.LocCode,
                                            Fld27 = item.LocDesc,
                                            Fld28 = item.LookupVal,
                                            Fld29 = item.PayMonth,
                                            Fld30 = item.PayStruct_Id,
                                            Fld31 = item.ProcessDate,
                                            Fld32 = item.ReleaseDate,
                                            Fld33 = item.SalaryHead_Id,
                                            Fld34 = item.SalHead_FullDetails,
                                            Fld35 = item.SalHead_SeqNo,
                                            Fld36 = item.StdAmount,
                                            Fld37 = item.TotalDeduction,
                                            Fld38 = item.TotalEarning,
                                            Fld39 = item.TotalNet,
                                        };

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }

                    case "DEDSALARYSTATEMENT":

                        var OSaldedHead = db.CompanyPayroll
                   .Include(e => e.SalaryHead)
                   .Include(e => e.SalaryHead.Select(r => r.Type)).AsNoTracking()
                   .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        var OEmpdedData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            //var OEmpdedData_t = db.EmployeePayroll
                            //.Include(e => e.SalaryT)
                            //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                            //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                            //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                            //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                            //.Include(e => e.Employee)
                            //.Include(e => e.Employee.EmpName)
                            //.Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                            //.Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                            //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                            //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus)).AsNoTracking()
                            //  .Where(e => e.Employee.Id == item).AsParallel().FirstOrDefault();

                            var OEmpdedData_t = db.EmployeePayroll.Where(e => e.Employee.Id == item)
                                //.Include(e => e.SalaryT)
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                             .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                                //.Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                                //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                                //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                             .AsNoTracking().OrderBy(e => e.Id)
                              .FirstOrDefault();

                            if (OEmpdedData_t != null)
                            {
                                OEmpdedData_t.Employee.EmpName = db.NameSingle.Find(OEmpdedData_t.Employee.EmpName_Id);
                               // OEmpdedData_t.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => e.EmployeePayroll_Id == OEmpdedData_t.Id).ToList();
                                OEmpdedData_t.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OEmpdedData_t.Id).ToList();
                                foreach (var OSal in OEmpdedData_t.SalaryT)
                                {
                                    OSal.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == OSal.Id && e.Amount != 0).ToList();
                                    foreach (var OSalEarn in OSal.SalEarnDedT)
                                    {
                                        OSalEarn.SalaryHead = db.SalaryHead.Find(OSalEarn.SalaryHead_Id);
                                        OSalEarn.SalaryHead.Type = db.LookupValue.Find(OSalEarn.SalaryHead.Type_Id);
                                        OSalEarn.SalaryHead.SalHeadOperationType = db.LookupValue.Find(OSalEarn.SalaryHead.SalHeadOperationType_Id);
                                    }
                                    OSal.Geostruct = db.GeoStruct.Find(OSal.Geostruct_Id);
                                    OSal.Geostruct.Location = db.Location.Find(OSal.Geostruct.Location_Id);
                                    if (OSal.Geostruct != null && OSal.Geostruct.Location != null)
                                    {
                                        OSal.Geostruct.Location.LocationObj = db.LocationObj.Find(OSal.Geostruct.Location.LocationObj_Id);
                                    }
                                    OSal.Geostruct.Department = db.Department.Find(OSal.Geostruct.Department_Id);
                                    if (OSal.Geostruct != null && OSal.Geostruct.Department != null)
                                    {
                                        OSal.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(OSal.Geostruct.Department.DepartmentObj_Id);
                                    }
                                    OSal.FuncStruct = db.FuncStruct.Find(OSal.FuncStruct_Id);
                                    OSal.FuncStruct.Job = db.Job.Find(OSal.FuncStruct.Job_Id);
                                    OSal.FuncStruct.Job.JobPosition = db.JobPosition.Where(e => e.Id == OSal.FuncStruct.Job_Id).ToList();
                                    OSal.PayStruct = db.PayStruct.Find(OSal.PayStruct_Id);
                                    OSal.PayStruct.Grade = db.Grade.Find(OSal.PayStruct.Grade_Id);
                                    OSal.PayStruct.Level = db.Level.Find(OSal.PayStruct.Level_Id);
                                    OSal.PayStruct.JobStatus = db.JobStatus.Find(OSal.PayStruct.JobStatus_Id);
                                    if (OSal.PayStruct != null && OSal.PayStruct.JobStatus != null)
                                    {
                                        OSal.PayStruct.JobStatus.EmpActingStatus = db.LookupValue.Find(OSal.PayStruct.JobStatus.EmpActingStatus_Id);
                                    }
                                }


                                OEmpdedData.Add(OEmpdedData_t);
                            }
                        }
                        var OSalTdedchk = OEmpdedData.Select(e => e.SalaryT).ToList();
                        //var mHeader = false;
                        if (OEmpdedData.Count() == 0 || OSalTdedchk == null || OSalTdedchk == null)
                        {
                            return null;
                        }
                        else
                        {
                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            foreach (var item in vc)
                            {
                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }
                           



                            var SpSort = Sortingslist.ToList();
                            var LOCCODE = false;
                            var DEPARTMENTCODE = false;
                            var GRADECODE = false;
                            var DESIGNATIONCODE = false;
                            var EMPCODE = false;
                            foreach (var item1 in SpSort)
                            {
                                if (item1 == "LOCCODE")
                                {
                                    LOCCODE = true;
                                }
                                if (item1 == "DEPARTMENTCODE")
                                {
                                    DEPARTMENTCODE = true;
                                }
                                if (item1 == "GRADECODE")
                                {
                                    GRADECODE = true;
                                }
                                if (item1 == "DESIGNATIONCODE")
                                {
                                    DESIGNATIONCODE = true;
                                }
                                if (item1 == "EMPCODE")
                                {
                                    EMPCODE = true;
                                }
                            }

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();


                            foreach (var OEmpsingle in OEmpdedData)
                            {
                                int? geoid = OEmpsingle.Employee.GeoStruct_Id;


                                int? payid = OEmpsingle.Employee.PayStruct_Id;

                                int? funid = OEmpsingle.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                foreach (var month in mPayMonth)
                                {

                                    // var month = mPayMonth.FirstOrDefault();
                                    var OEarnDedData = OEmpsingle.SalaryT.Where(g => month == g.PayMonth && g.SalEarnDedT != null)
                                             .Select(e => e.SalEarnDedT.Where(y => (y.SalaryHead.InPayslip == true && y.Amount != 0) && y.SalaryHead.Type.LookupVal == "Deduction")
                                            .Select(r => new
                                            {
                                                emppayrollid = OEmpsingle.Id,
                                                empid = OEmpsingle.Employee.Id,
                                                empcode = OEmpsingle.Employee.EmpCode,
                                                empname = OEmpsingle.Employee.EmpName.FullNameFML,
                                                paymonth = e.PayMonth,
                                                locid = e.Geostruct.Location.Id,
                                                loccode = e.Geostruct.Location.LocationObj.LocCode,
                                                locname = e.Geostruct.Location.LocationObj.LocDesc,
                                                Gradecode = e.PayStruct.Grade.Code,
                                                GradeName = e.PayStruct.Grade.Name,
                                                jobid = e.FuncStruct.Job.Id,
                                                jobcode = e.FuncStruct.Job.Code,
                                                jobname = e.FuncStruct.Job.Name,
                                                salcodeid = r.SalaryHead.Id,
                                                salcode = r.SalaryHead.Code,
                                                salname = r.SalaryHead.Name,
                                                amount = r.Amount,
                                                saltype = r.SalaryHead.Type,
                                                salheadseqno = r.SalaryHead.SeqNo,
                                                salOptype = r.SalaryHead.SalHeadOperationType,
                                                deptcode = e.Geostruct.Department == null ? "" : e.Geostruct.Department.DepartmentObj == null ? "" : e.Geostruct.Department.DepartmentObj.DeptCode
                                            }
                                            )).AsParallel().SingleOrDefault();
                                    if (OEarnDedData != null)
                                    {
                                       
                                        if (salheadlist.Count() > 0)
                                        {
                                            foreach (var item in salheadlist)
                                            {
                                                var OEarnData = OEarnDedData.Where(a => a.salcode.ToString() == item.ToString()).AsParallel().ToList();
                                                if (OEarnData.Count > 0)
                                                {
                                                  
                                                    foreach (var ca in OEarnData)
                                                    {
                                                       
                                                        GenericField100 OEarnStatement = new GenericField100()
                                                        {
                                                            Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                            Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                            Fld3 = ca.empname == null ? "" : ca.empname,
                                                            Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                            Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                            Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                            Fld7 = ca.locname == null ? "" : ca.locname,
                                                            Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                            Fld9 = ca.GradeName == null ? "" : ca.GradeName,
                                                            Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                            Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                            Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                            Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                            Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                            Fld15 = ca.salname == null ? "" : ca.salname,
                                                            Fld16 = ca.amount == null ? "" : ca.amount.ToString(),
                                                            Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                            Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                            Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                            Fld20 = ca.deptcode == null ? "" : ca.deptcode
                                                        };

                                                        if (comp)
                                                        {
                                                            OEarnStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OEarnStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OEarnStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                        }
                                                        if (dept)
                                                        {
                                                            OEarnStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                        }
                                                        if (grp)
                                                        {
                                                            OEarnStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OEarnStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                        }
                                                        if (grade)
                                                        {
                                                            OEarnStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                        }
                                                        if (lvl)
                                                        {
                                                            OEarnStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                        }
                                                        if (jobstat)
                                                        {
                                                            OEarnStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                        }
                                                        if (job)
                                                        {
                                                            OEarnStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                        }
                                                        if (jobpos)
                                                        {
                                                            OEarnStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                        }
                                                      

                                                       
                                                        OGenericPayrollStatement.Add(OEarnStatement);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var OEarnData = OEarnDedData.ToList();
                                            if (OEarnData.Count > 0)
                                            {
                                                foreach (var ca in OEarnData)
                                                {
                                                    GenericField100 OEarnStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                        Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                        Fld3 = ca.empname == null ? "" : ca.empname,
                                                        Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                        Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                        Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                        Fld7 = ca.locname == null ? "" : ca.locname,
                                                        Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                        Fld9 = ca.GradeName == null ? "" : ca.GradeName,
                                                        Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                        Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                        Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                        Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                        Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                        Fld15 = ca.salname == null ? "" : ca.salname,
                                                        Fld16 = ca.amount == null ? "" : ca.amount.ToString(),
                                                        Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                        Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                        Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                        Fld20 = ca.deptcode == null ? "" : ca.deptcode
                                                    };

                                                    if (comp)
                                                    {
                                                        OEarnStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OEarnStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OEarnStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OEarnStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OEarnStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OEarnStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OEarnStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OEarnStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OEarnStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OEarnStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OEarnStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                   


                                                    OGenericPayrollStatement.Add(OEarnStatement);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                         
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld8).ThenBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && GRADECODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld8).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && GRADECODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld8).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && GRADECODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld8).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (GRADECODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld8).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericPayrollStatement.ToList();
                            }
                            if (LOCCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld6).ToList();
                            }
                            if (DEPARTMENTCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld20).ToList();
                            }
                            if (GRADECODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld8).ToList();
                            }
                            if (DESIGNATIONCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld11).ToList();
                            }
                            if (EMPCODE == true)
                            {
                                OGenericPayrollStatement = OGenericPayrollStatement.OrderBy(e => e.Fld2).ToList();
                            }

                            return OGenericPayrollStatement.ToList();
                        }
                        break;

                    case "EARNSALARYSTATEMENT":

                        var OSalHead = db.CompanyPayroll
                                               .Include(e => e.SalaryHead).AsNoTracking()
                                               .Where(d => d.Id == CompanyPayrollId).AsParallel().FirstOrDefault();



                        var OEmpData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {

                            var OEmpData_t = db.EmployeePayroll.Where(e => e.Employee.Id == item)
                                //.Include(e => e.SalaryT)
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                              .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                                //.Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                                //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                                //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                              .AsNoTracking().OrderBy(e => e.Id)
                               .FirstOrDefault();


                            if (OEmpData_t != null)
                            {
                                OEmpData_t.Employee.EmpName = db.NameSingle.Find(OEmpData_t.Employee.EmpName_Id);
                                OEmpData_t.SalaryT = db.SalaryT.Include(e => e.PFECRR).Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OEmpData_t.Id).ToList();
                                foreach (var OSal in OEmpData_t.SalaryT)
                                {
                                    OSal.SalEarnDedT = db.SalEarnDedT.Where(e => e.SalaryT_Id == OSal.Id && e.Amount != 0).ToList();
                                    foreach (var OSalEarn in OSal.SalEarnDedT)
                                    {
                                        OSalEarn.SalaryHead = db.SalaryHead.Find(OSalEarn.SalaryHead_Id);
                                        OSalEarn.SalaryHead.Type = db.LookupValue.Find(OSalEarn.SalaryHead.Type_Id);
                                        OSalEarn.SalaryHead.SalHeadOperationType = db.LookupValue.Find(OSalEarn.SalaryHead.SalHeadOperationType_Id);
                                    }
                                    OSal.Geostruct = db.GeoStruct.Find(OSal.Geostruct_Id);
                                    OSal.Geostruct.Location = db.Location.Find(OSal.Geostruct.Location_Id);
                                    if (OSal.Geostruct != null && OSal.Geostruct.Location != null)
                                    {
                                        OSal.Geostruct.Location.LocationObj = db.LocationObj.Find(OSal.Geostruct.Location.LocationObj_Id);
                                    }
                                    OSal.Geostruct.Department = db.Department.Find(OSal.Geostruct.Department_Id);
                                    if (OSal.Geostruct != null && OSal.Geostruct.Department != null)
                                    {
                                        OSal.Geostruct.Department.DepartmentObj = db.DepartmentObj.Find(OSal.Geostruct.Department.DepartmentObj_Id);
                                    }
                                    OSal.FuncStruct = db.FuncStruct.Find(OSal.FuncStruct_Id);
                                    OSal.FuncStruct.Job = db.Job.Find(OSal.FuncStruct.Job_Id);
                                    OSal.FuncStruct.Job.JobPosition = db.JobPosition.Where(e => e.Id == OSal.FuncStruct.Job_Id).ToList();
                                    OSal.PayStruct = db.PayStruct.Find(OSal.PayStruct_Id);
                                    OSal.PayStruct.Grade = db.Grade.Find(OSal.PayStruct.Grade_Id);
                                    OSal.PayStruct.Level = db.Level.Find(OSal.PayStruct.Level_Id);
                                    OSal.PayStruct.JobStatus = db.JobStatus.Find(OSal.PayStruct.JobStatus_Id);
                                    if (OSal.PayStruct != null && OSal.PayStruct.JobStatus != null)
                                    {
                                        OSal.PayStruct.JobStatus.EmpActingStatus = db.LookupValue.Find(OSal.PayStruct.JobStatus.EmpActingStatus_Id);
                                    }
                                }

                                OEmpData.Add(OEmpData_t);
                            }
                        }
                        // var OSalTchk = OEmpData.Select(e => e.SalaryT).ToList();
                        var mHeader = false;
                        if (OEmpData.Count() == 0 || OSalHead == null)
                        {
                            return null;
                        }
                        else
                        {

                            //var month = false;
                            var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            var regn = false;
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                            foreach (var item in vc)
                            {
                                //if (item.LookupVal.ToUpper() == "MONTH")
                                //{
                                //    month = true;
                                //}
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {

                                    loca = true;
                                }
                                if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                {
                                    emp = true;
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                }
                                if (item.LookupVal.ToUpper() == "COMPANY")
                                {
                                    comp = true;
                                }
                                if (item.LookupVal.ToUpper() == "GROUP")
                                {
                                    grp = true;
                                }
                                if (item.LookupVal.ToUpper() == "UNIT")
                                {
                                    unit = true;
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                }
                                if (item.LookupVal.ToUpper() == "REGION")
                                {
                                    regn = true;
                                }
                                if (item.LookupVal.ToUpper() == "GRADE")
                                {
                                    grade = true;
                                }
                                if (item.LookupVal.ToUpper() == "LEVEL")
                                {
                                    lvl = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBSTATUS")
                                {
                                    jobstat = true;
                                }

                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                }
                                if (item.LookupVal.ToUpper() == "JOBPOSITION")
                                {
                                    jobpos = true;
                                }
                            }
                            

                            var SpSort = Sortingslist.ToList();
                            var LOCCODE = false;
                            var DEPARTMENTCODE = false;
                            var GRADECODE = false;
                            var DESIGNATIONCODE = false;
                            var EMPCODE = false;
                            foreach (var item1 in SpSort)
                            {
                                if (item1 == "LOCCODE")
                                {
                                    LOCCODE = true;
                                }
                                if (item1 == "DEPARTMENTCODE")
                                {
                                    DEPARTMENTCODE = true;
                                }
                                if (item1 == "GRADECODE")
                                {
                                    GRADECODE = true;
                                }
                                if (item1 == "DESIGNATIONCODE")
                                {
                                    DESIGNATIONCODE = true;
                                }
                                if (item1 == "EMPCODE")
                                {
                                    EMPCODE = true;
                                }
                            }

                            var SpGroup = SpecialGroupslist.ToList();
                            //var LOCCODE = false;
                            //var GRADECODE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "LOCCODE")
                                {

                                    LOCCODE = true;
                                }
                                if (item1 == "GRADECODE")
                                {

                                    GRADECODE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<GenericField100> OGenericSalaryStatement = new List<GenericField100>();
                            foreach (var OEmpsingle in OEmpData)
                            {

                                int? geoid = OEmpsingle.Employee.GeoStruct_Id;


                                int? payid = OEmpsingle.Employee.PayStruct_Id;

                                int? funid = OEmpsingle.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                foreach (var month in mPayMonth)
                                {

                                    var OEarnDedData = OEmpsingle.SalaryT.Where(g => g.PayMonth == month && g.SalEarnDedT != null)
                                             .Select(e => e.SalEarnDedT.Where(y => (y.SalaryHead.InPayslip == true && y.Amount != 0) && y.SalaryHead.Type.LookupVal == "Earning")
                                            .Select(r => new
                                            {
                                                emppayrollid = OEmpsingle.Id,
                                                empid = OEmpsingle.Employee.Id,
                                                empcode = OEmpsingle.Employee.EmpCode,
                                                empname = OEmpsingle.Employee.EmpName.FullNameFML,
                                                paymonth = e.PayMonth,
                                                locid = e.Geostruct.Location.Id,
                                                loccode = e.Geostruct.Location.LocationObj == null ? "" : e.Geostruct.Location.LocationObj.LocCode,
                                                locname = e.Geostruct.Location.LocationObj == null ? "" : e.Geostruct.Location.LocationObj.LocDesc,
                                                Gradecode = e.PayStruct.Grade.Code,
                                                GradeName = e.PayStruct.Grade.Name,
                                                jobid = e.FuncStruct.Job.Id,
                                                jobcode = e.FuncStruct.Job.Code,
                                                jobname = e.FuncStruct.Job.Name,
                                                salcodeid = r.SalaryHead.Id,
                                                salcode = r.SalaryHead.Code,
                                                salname = r.SalaryHead.Name,
                                                amount = r.Amount,
                                                saltype = r.SalaryHead.Type,
                                                salheadseqno = r.SalaryHead.SeqNo,
                                                salOptype = r.SalaryHead.SalHeadOperationType,
                                                deptcode = e.Geostruct.Department == null ? "" : e.Geostruct.Department.DepartmentObj == null ? "" : e.Geostruct.Department.DepartmentObj.DeptCode

                                            }
                                            )).AsParallel().SingleOrDefault();
                                    if (OEarnDedData != null)
                                    {
                                        if (salheadlist.Count() > 0)
                                        {
                                            foreach (var item in salheadlist)
                                            {

                                                var OEarnData = OEarnDedData.Where(a => a.salcode.ToString() == item.ToString()).AsParallel().ToList();
                                                if (OEarnData.Count > 0)
                                                {
                                                    foreach (var ca in OEarnData)
                                                    {
                                                        GenericField100 OEarnStatement = new GenericField100()
                                                        {
                                                            Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                            Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                            Fld3 = ca.empname == null ? "" : ca.empname,
                                                            Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                            Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                            Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                            Fld7 = ca.locname == null ? "" : ca.locname,
                                                            Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                            Fld9 = ca.GradeName == null ? "" : ca.GradeName,
                                                            Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                            Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                            Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                            Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                            Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                            Fld15 = ca.salname == null ? "" : ca.salname,
                                                            Fld16 = ca.amount == null ? "" : ca.amount.ToString(),
                                                            Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                            Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                            Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                            Fld20 = ca.deptcode == null ? "" : ca.deptcode
                                                        };

                                                        if (comp)
                                                        {
                                                            OEarnStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OEarnStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OEarnStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                        }
                                                        if (dept)
                                                        {
                                                            OEarnStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                        }
                                                        if (grp)
                                                        {
                                                            OEarnStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OEarnStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                        }
                                                        if (grade)
                                                        {
                                                            OEarnStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                        }
                                                        if (lvl)
                                                        {
                                                            OEarnStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                        }
                                                        if (jobstat)
                                                        {
                                                            OEarnStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                        }
                                                        if (job)
                                                        {
                                                            OEarnStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                        }
                                                        if (jobpos)
                                                        {
                                                            OEarnStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                        }
                                                        OGenericSalaryStatement.Add(OEarnStatement);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var OEarnData = OEarnDedData.ToList();
                                            if (OEarnData.Count > 0)
                                            {
                                                foreach (var ca in OEarnData)
                                                {
                                                    GenericField100 OEarnStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                        Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                        Fld3 = ca.empname == null ? "" : ca.empname,
                                                        Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                        Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                        Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                        Fld7 = ca.locname == null ? "" : ca.locname,
                                                        Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                        Fld9 = ca.GradeName == null ? "" : ca.GradeName,
                                                        Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                        Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                        Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                        Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                        Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                        Fld15 = ca.salname == null ? "" : ca.salname,
                                                        Fld16 = ca.amount == null ? "" : ca.amount.ToString(),
                                                        Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                        Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                        Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                        Fld20 = ca.deptcode == null ? "" : ca.deptcode
                                                    };

                                                    if (comp)
                                                    {
                                                        OEarnStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OEarnStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OEarnStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OEarnStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OEarnStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OEarnStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OEarnStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OEarnStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OEarnStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OEarnStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OEarnStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    OGenericSalaryStatement.Add(OEarnStatement);
                                                }
                                            }
                                        }

                                    }
                                }


                            }
                       
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (GRADECODE == true && DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld8).ThenBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && GRADECODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld8).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && GRADECODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld8).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && GRADECODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld8).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (DEPARTMENTCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld20).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && GRADECODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld8).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (LOCCODE == true && DEPARTMENTCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ThenBy(e => e.Fld20).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (GRADECODE == true && DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld8).ThenBy(e => e.Fld11).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (GRADECODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld8).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }
                            if (DESIGNATIONCODE == true && EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld11).ThenBy(e => e.Fld2).ToList();
                                return OGenericSalaryStatement.ToList();
                            }                        
                            if (LOCCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld6).ToList();
                            }
                            if (DEPARTMENTCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld20).ToList();
                            }
                            if (GRADECODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld8).ToList();
                            }                          
                            if (DESIGNATIONCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld11).ToList();
                            }
                          
                            if (EMPCODE == true)
                            {
                                OGenericSalaryStatement = OGenericSalaryStatement.OrderBy(e => e.Fld2).ToList();
                            }
                            
                           
                            return OGenericSalaryStatement.ToList();
                        }
                        break;

                    case "BRANCHSUMMARYDETAILS":
                        OEmpPayrollIdList = new List<int?>();
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();
                        List<int> SplLocId = new List<int>();
                        // OEmpPayrollIdList = db.SalaryT.Where(e => e.PayMonth == PayMon).OrderBy(e => e.Id).AsNoTracking().AsParallel().Select(r => r.EmployeePayroll_Id).ToList();
                        // OEmpPayrollIdList = db.SalaryT.Include(e => e.Geostruct).Where(e => e.PayMonth == PayMon).OrderBy(e => e.Id).AsNoTracking().AsParallel().Select(r => r.Geostruct.Location_Id).Distinct().ToList();

                   

                        OEmpPayrollIdList = db.GeoStruct.Include(e => e.Location).Include(e => e.Location.LocationObj)
                            .Where(e => e.Location_Id != null).OrderBy(e => e.Id).AsNoTracking().Select(r => r.Location_Id).Distinct().ToList();

                        if (SpecialGroupslist.Count() > 0)
                        {
                            foreach (var item in SpecialGroupslist.ToList())
                            {
                                int LocId = db.Location.Include(e => e.LocationObj).Where(e => item.Contains(e.LocationObj.LocCode)).FirstOrDefault().Id;
                                SplLocId.Add(LocId);
                            } 
                            OEmpPayrollIdList = OEmpPayrollIdList.Where(e => SplLocId.Contains(e.Value)).ToList();
                        }
                        


                        
                        //foreach (var item in OEmpPayrollIdList)
                        //{
                        //    var OSalaryData_temp = db.EmployeePayroll
                        //    .Where(e => e.Employee_Id == item).OrderBy(e => e.Id).AsNoTracking().AsParallel()
                        //    .FirstOrDefault();


                        //    OSalaryData_temp.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OSalaryData_temp.Id && e.PayMonth == PayMon && e.IsHold == false).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();

                        //    if (OSalaryData_temp != null)
                        //    {
                        //        OEmpEarnDedSum.Add(OSalaryData_temp);
                        //    }
                        //}


                        if (OEmpEarnDedSum == null)
                        {

                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;
                            var LOCATION = false;

                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            


                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetJobPosBrSummaryClass> SalaryData = new List<Utility.GetJobPosBrSummaryClass>();


                            foreach (var ca in OEmpPayrollIdList)
                            {
                                //var loc = db.Location.Include(e => e.BusinessCategory).Where(e => e.Id == ca).FirstOrDefault();  

                                var BankL = db.Bank.ToList();
                                List<string> BusiCategory = new List<string>();

                                if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                                {
                                    if (BankL != null)
                                    {

                                        BusiCategory.AddRange(salheadlist);

                                        foreach (string ca1 in BusiCategory)
                                        {
                                            //if (loc.BusinessCategory != null )

                                            foreach (var pbankname in BankL)
                                            {

                                                // if (loc.BusinessCategory.LookupVal.ToString().ToUpper() == ca1.ToUpper())
                                                if (pbankname.Name.ToUpper() == ca1.ToUpper())
                                                {
                                                    SalaryData = ReportRDLCObjectClass.GetJobPosBranchSumDatapaybank(ca, PayMon, pbankname.Id);

                                                }

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //SalaryT OSalT = ca..Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                    //int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                                    SalaryData = ReportRDLCObjectClass.GetJobPosBranchSumData(ca, PayMon, 0);
                                }

                             

                                string readstr = string.Empty;
                                string getFilepathLocation = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2bUltimate\\App_Data\\Menu_Json";
                                bool IsDirExist = System.IO.Directory.Exists(getFilepathLocation);

                                string localpath = string.Empty;
                                if (!IsDirExist) // If Dir not exist i.e. False then create dir w.r.t above path.
                                {
                                    localpath = new Uri(getFilepathLocation).LocalPath;
                                    System.IO.Directory.CreateDirectory(localpath);
                                }

                                string path = getFilepathLocation + @"\GrandSummaryDetailsReport" + ".ini";
                                localpath = new Uri(path).LocalPath;
                                if (!System.IO.File.Exists(localpath))
                                {

                                    using (var fs = new FileStream(localpath, FileMode.OpenOrCreate))
                                    {
                                        StreamWriter str = new StreamWriter(fs);
                                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                                        str.Flush();
                                        str.Close();
                                        fs.Close();
                                    }

                                }

                                else
                                {
                                    using (var streamReader = new StreamReader(localpath))
                                    {
                                        string line;
                                        while ((line = streamReader.ReadLine()) != null)
                                        {
                                            readstr = line;
                                        }

                                    }
                                }

                                string CompanyCode = string.Empty;

                                if (!String.IsNullOrEmpty(readstr))
                                {
                                    CompanyCode = readstr.ToUpper();
                                }

                                foreach (var item in SalaryData)
                                {
                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld1 = item.Company_Code,
                                        Fld2 = item.Company_Name,
                                        Fld4 = item.Geostruct_Id,
                                        Fld5 = item.Location_Id,
                                        Fld6 = item.LocCode,
                                        Fld7 = item.LocDesc + "  " + item.BusinessCategory,
                                        Fld8 = item.FuncStruct_Id,
                                        Fld9 = item.FunStruct_Details,
                                        Fld10 = item.Job_Id,
                                        Fld11 = item.Job_Code,
                                        Fld12 = item.Job_Name,
                                        Fld13 = item.JobPosition_Id,
                                        Fld14 = item.JobPositionCode,
                                        Fld15 = item.JobPositionDesc,
                                        Fld16 = item.PayMonth,
                                        Fld17 = item.SalHeadCode,
                                        Fld18 = item.EarnHead,
                                        Fld19 = item.SalHead_FullDetails,
                                        Fld20 = item.LookupVal,
                                        Fld21 = item.InPayslip,
                                        Fld22 = item.StdAmount,
                                        Fld23 = item.EarnAmount,
                                        Fld24 = item.LocTotalEarning,
                                        Fld25 = item.LocTotalDeduction,
                                        Fld26 = item.LocTotalNet,
                                        Fld27 = item.LocTotalEmployee,
                                        Fld28 = item.JobPosTotalEarning,
                                        Fld29 = item.JobPosTotalDeduction,
                                        Fld30 = item.JobPosTotalNet,
                                        Fld31 = item.JobPosTotalEmployee,

                                        Fld32 = item.SequenceNo,
                                        Fld33 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Bank Contribution: " : "",
                                        Fld34 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalPFBankContribution : "",

                                        Fld35 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Pension Amount: " : "",
                                        Fld36 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalPfPensionAmount : "",

                                        Fld37 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Relief Fund Bank Contribution: " : "",
                                        Fld38 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalReliefFundBankContribution : "",

                                        Fld39 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "SWF Bank Contribution: " : "",
                                        Fld40 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalSWFBankContribution : "",

                                        Fld41 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Total Bank Contribution: " : "",
                                        Fld42 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalBankCONTRIBUTION : "",

                                    };

                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                }

                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "EARNINGSUMMARY":

                        List<EmployeePayroll> OEmpSum = new List<EmployeePayroll>();
                        string paymon = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {

                            var OOEmpSum_temp = db.EmployeePayroll
                           .Include(e => e.Employee)
                           .Include(e => e.SalaryT)
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(q => q.SalaryHead)))
                           .Include(e => e.SalaryT.Select(t => t.Geostruct))
                           .Include(e => e.SalaryT.Select(t => t.Geostruct.Location))
                           .Include(e => e.SalaryT.Select(t => t.Geostruct.Location.LocationObj))
                           .Include(e => e.SalaryT.Select(t => t.Geostruct.Department))
                           .Include(e => e.SalaryT.Select(t => t.Geostruct.Department.DepartmentObj))
                           .Include(e => e.SalaryT.Select(t => t.Geostruct.Division))
                           .Include(e => e.SalaryT.Select(t => t.PayStruct))
                           .Include(e => e.SalaryT.Select(t => t.PayStruct.Grade))
                           .Include(e => e.SalaryT.Select(t => t.FuncStruct))
                           .Include(e => e.SalaryT.Select(t => t.FuncStruct.Job))
                            .Include(e => e.SalaryT.Select(t => t.FuncStruct.JobPosition))
                           .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel().FirstOrDefault();

                            if (OOEmpSum_temp != null)
                            {
                                OEmpSum.Add(OOEmpSum_temp);
                            }

                        }


                        if (OEmpSum == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            foreach (var ca in OEmpSum)
                            {
                                int geoid = ca.SalaryT.FirstOrDefault().Geostruct.Id;

                                int payid = ca.SalaryT.FirstOrDefault().PayStruct.Id;

                                int funid = ca.SalaryT.FirstOrDefault().FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                {
                                    foreach (var item1 in salheadlist)
                                    {
                                        int SalId = ca.SalaryT.Where(e => e.PayMonth == paymon).FirstOrDefault().Id;

                                        var EmpSum = db.SalaryT
                                             .Include(e => e.SalEarnDedT)
                           .Include(e => e.SalEarnDedT.Select(q => q.SalaryHead))
                           .Include(e => e.Geostruct)
                           .Include(e => e.Geostruct.Location)
                           .Include(e => e.Geostruct.Location.LocationObj)
                           .Include(e => e.Geostruct.Department)
                           .Include(e => e.Geostruct.Department.DepartmentObj)
                           .Include(e => e.Geostruct.Division)
                           .Include(e => e.PayStruct)
                           .Include(e => e.PayStruct.Grade)
                           .Include(e => e.FuncStruct)
                           .Include(e => e.FuncStruct.Job)
                            .Include(e => e.FuncStruct.JobPosition)
                                            .Where(e => e.Id == SalId).FirstOrDefault();

                                        if (EmpSum != null)
                                        {

                                            //foreach (var item in EmpSum)
                                            //{
                                            var salamt = EmpSum.SalEarnDedT.Where(t => t.SalaryHead.Name == item1).FirstOrDefault().Amount;
                                            ////filter

                                            if (loca == true)
                                            {
                                                group = EmpSum.Geostruct.Location.LocationObj.LocDesc;
                                            }
                                            if (div == true)
                                            {
                                                group = EmpSum.Geostruct.Division.Name;
                                            }
                                            if (dept == true)
                                            {
                                                group = EmpSum.Geostruct.Department.DepartmentObj.DeptDesc;
                                            }
                                            if (job == true)
                                            {
                                                group = EmpSum.FuncStruct.Job.Name;
                                            }
                                            ///////special group 

                                            //if (JOB == true)
                                            //{
                                            //    filter = item.FuncStruct.Job.Name;
                                            //}
                                            //if (GRADE == true)
                                            //{
                                            //    filter = item.PayStruct.Grade.Name;
                                            //}
                                            //if (JOBPOSITION == true)
                                            //{
                                            //    filter = item.FuncStruct.JobPosition.JobPositionDesc;
                                            //}

                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            {
                                                Fld1 = header.ToString(),
                                                Fld2 = ca.Employee.EmpCode.ToString(),
                                                Fld4 = group.ToString(),
                                                Fld5 = filter.ToString(),
                                                Fld6 = salamt.ToString(),
                                            };

                                            //write data to generic class
                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                            //}
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "GRANDSUMMARYDETAILSBYJOBPOSITION":

                        List<int> OEmpPayrollIdListNew = new List<int>();
                        OEmpEarnDedSum = new List<EmployeePayroll>();
                        PayMon = mPayMonth.FirstOrDefault();

                        // OEmpPayrollIdList = db.GeoStruct.OrderBy(e => e.Id).AsNoTracking().Select(r => r.Location_Id).Distinct().ToList();
                        OEmpPayrollIdListNew = db.JobPosition.Select(x => x.Id).ToList();
                        if (OEmpEarnDedSum == null)
                        {

                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetJobPosBrSummaryClass> SalaryData = new List<Utility.GetJobPosBrSummaryClass>();


                            foreach (var ca in OEmpPayrollIdListNew)
                            {
                                //var loc = db.Location.Include(e => e.BusinessCategory).Where(e => e.Id == ca).FirstOrDefault();  

                                //SalaryT OSalT = ca..Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                //int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;


                                SalaryData = ReportRDLCObjectClass.GetJobPosBranchSumDataJob(0, PayMon, ca);

                                if (SalaryData != null)
                                {
                                    foreach (var item in SalaryData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = item.Company_Code,
                                            Fld2 = item.Company_Name,
                                            Fld4 = item.Geostruct_Id,
                                            Fld5 = item.Location_Id,
                                            Fld6 = item.JobPositionCode,
                                            Fld7 = item.JobPositionDesc,
                                            Fld8 = item.FuncStruct_Id,
                                            Fld9 = item.FunStruct_Details,
                                            Fld10 = item.Job_Id,
                                            Fld11 = item.Job_Code,
                                            Fld12 = item.Job_Name,
                                            Fld13 = item.JobPosition_Id,
                                            Fld14 = item.JobPositionCode,
                                            Fld15 = item.JobPositionDesc,
                                            Fld16 = item.PayMonth,
                                            Fld17 = item.SalHeadCode,
                                            Fld18 = item.EarnHead,
                                            Fld19 = item.SalHead_FullDetails,
                                            Fld20 = item.LookupVal,
                                            Fld21 = item.InPayslip,
                                            Fld22 = item.StdAmount,
                                            Fld23 = item.EarnAmount,
                                            Fld24 = item.JobPosTotalEarning,
                                            Fld25 = item.JobPosTotalDeduction,
                                            Fld26 = item.JobPosTotalNet,
                                            Fld27 = item.JobPosTotalEmployee,
                                            //Fld28 = item.JobPosTotalEarning,
                                            //Fld29 = item.JobPosTotalDeduction,
                                            //Fld30 = item.JobPosTotalNet,
                                            //Fld31 = item.JobPosTotalEmployee

                                        };

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "BRANCHSUMMARYDETAILSBYJOBPOSITION":

                        List<int?> OEmpPayrollIdListNew1 = new List<int?>();
                        List<int> OEmpPayrollIdListNew2 = new List<int>();
                        OEmpEarnDedSum = new List<EmployeePayroll>();

                        PayMon = mPayMonth.FirstOrDefault();

                        OEmpPayrollIdListNew1 = db.GeoStruct.AsNoTracking().Select(r => r.Location_Id).Distinct().ToList();

                        if (OEmpEarnDedSum == null)
                        {

                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetJobPosBrSummaryClass> SalaryData = new List<Utility.GetJobPosBrSummaryClass>();


                            foreach (var ca in OEmpPayrollIdListNew1)
                            {
                                //var loc = db.Location.Include(e => e.BusinessCategory).Where(e => e.Id == ca).FirstOrDefault();  

                                //SalaryT OSalT = ca..Where(e => e.PayMonth == PayMon).FirstOrDefault();
                                //int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                                OEmpPayrollIdListNew2 = db.JobPosition.Select(x => x.Id).ToList();
                                foreach (var ca1 in OEmpPayrollIdListNew2)
                                {
                                    SalaryData = ReportRDLCObjectClass.GetJobPosBranchSumDataJobbranch(ca, PayMon, ca1);

                                    if (SalaryData != null)
                                    {
                                        foreach (var item in SalaryData)
                                        {
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            {
                                                Fld1 = item.Company_Code.ToString(),
                                                Fld2 = item.Company_Name,
                                                Fld4 = item.Geostruct_Id,
                                                Fld5 = item.Location_Id,
                                                Fld6 = item.LocCode,
                                                Fld7 = item.LocDesc,
                                                //Fld8 = item.FuncStruct_Id,
                                                //Fld9 = item.FunStruct_Details,
                                                Fld8 = item.JobPositionCode,
                                                Fld9 = item.JobPositionDesc,
                                                Fld10 = item.Job_Id,
                                                Fld11 = item.Job_Code,
                                                Fld12 = item.Job_Name,
                                                Fld13 = item.JobPosition_Id,
                                                //Fld14 = item.JobPositionCode,
                                                //Fld15 = item.JobPositionDesc,
                                                Fld16 = item.PayMonth,
                                                Fld17 = item.SalHeadCode,
                                                Fld18 = item.EarnHead,
                                                Fld19 = item.SalHead_FullDetails,
                                                Fld20 = item.LookupVal,
                                                Fld21 = item.InPayslip,
                                                Fld22 = item.StdAmount,
                                                Fld23 = item.EarnAmount,
                                                Fld24 = item.JobPosTotalEarning,
                                                Fld25 = item.JobPosTotalDeduction,
                                                Fld26 = item.JobPosTotalNet,
                                                Fld27 = item.JobPosTotalEmployee,
                                                //Fld28 = item.JobPosTotalEarning,
                                                //Fld29 = item.JobPosTotalDeduction,
                                                //Fld30 = item.JobPosTotalNet,
                                                //Fld31 = item.JobPosTotalEmployee

                                            };

                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "GRANDSUMMARYDETAILS":

                        PayMon = mPayMonth.FirstOrDefault();

                        var OEmpPayrollIdList1 = db.SalaryT.Include(e => e.Geostruct).Where(e => e.PayMonth == PayMon && e.Geostruct.Company_Id.Value == CompanyId).OrderBy(e => e.Id).AsNoTracking().AsParallel().Select(r => r.Id).Distinct().ToList();
                        if (OEmpPayrollIdList1 == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var JOB = false;
                            var GRADE = false;
                            var JOBPOSITION = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "JOB")
                                {

                                    JOB = true;
                                }
                                if (item1 == "GRADE")
                                {

                                    GRADE = true;
                                }
                                if (item1 == "JOB POSITION")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "DEPARTMENT")
                                {

                                    JOBPOSITION = true;
                                }
                                if (item1 == "LOCATION")
                                {

                                    JOBPOSITION = true;
                                }

                            }

                            var dept = false;
                            var loca = false;
                            var div = false;
                            var job = false;

                            var filter = "";

                            var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();
                            var header = "";
                            var group = "";
                            foreach (var item in vc)
                            {
                                if (item.LookupVal.ToUpper() == "LOCATION")
                                {
                                    loca = true;
                                    header = "Location";
                                }
                                if (item.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    dept = true;
                                    header = "Department";
                                }
                                if (item.LookupVal.ToUpper() == "DIVISION")
                                {
                                    div = true;
                                    header = "Division";
                                }
                                if (item.LookupVal.ToUpper() == "JOB")
                                {
                                    job = true;
                                    header = "Job";
                                }
                            }


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            List<Utility.GetJobPosBrSummaryClass> SalaryData = new List<Utility.GetJobPosBrSummaryClass>();
                            var paylable = "";

                            //foreach (var ca in OEmpPayrollIdList)
                            //{

                            //SalaryT OSalT = ca..Where(e => e.PayMonth == PayMon).FirstOrDefault();
                            //int SalId = ca.SalaryT.Where(e => e.PayMonth == PayMon).FirstOrDefault().Id;
                            List<string> BusiCategory = new List<string>();
                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                            {


                                BusiCategory.AddRange(salheadlist);
                                foreach (string ca1 in BusiCategory)
                                {
                                    // var category=db.Lookup.Include(e=>e.LookupValues).Where(e=>e.Code=="408").FirstOrDefault();
                                    var category = db.Bank.ToList();
                                    foreach (var item in category)
                                    {
                                        //var cat = category.LookupValues.Where(e => e.LookupVal.ToUpper() == ca1.ToUpper()).FirstOrDefault();
                                        paylable = "";
                                        if (item.Name.ToUpper() == ca1.ToUpper())
                                        {
                                            paylable = "Payment bank" + " :";
                                            int catid = item.Id;
                                            SalaryData = ReportRDLCObjectClass.GetGrandSumDatacat(PayMon, catid);
                                            break;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                SalaryData = ReportRDLCObjectClass.GetGrandSumData(PayMon, CompanyId);

                            }

                            string readstr = string.Empty;
                            string getFilepathLocation = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2bUltimate\\App_Data\\Menu_Json";
                            bool IsDirExist = System.IO.Directory.Exists(getFilepathLocation);

                            string localpath = string.Empty;
                            if (!IsDirExist) // If Dir not exist i.e. False then create dir w.r.t above path.
                            {
                                localpath = new Uri(getFilepathLocation).LocalPath;
                                System.IO.Directory.CreateDirectory(localpath);
                            }

                            string path = getFilepathLocation + @"\GrandSummaryDetailsReport" + ".ini";
                            localpath = new Uri(path).LocalPath;
                            if (!System.IO.File.Exists(localpath))
                            {

                                using (var fs = new FileStream(localpath, FileMode.OpenOrCreate))
                                {
                                    StreamWriter str = new StreamWriter(fs);
                                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                                    str.Flush();
                                    str.Close();
                                    fs.Close();
                                }

                            }

                            else
                            {
                                using (var streamReader = new StreamReader(localpath))
                                {
                                    string line;
                                    while ((line = streamReader.ReadLine()) != null)
                                    {
                                        readstr = line;
                                    }

                                }
                            }

                            string CompanyCode = string.Empty;

                            if (!String.IsNullOrEmpty(readstr))
                            {
                                CompanyCode = readstr.ToUpper();
                            }


                            if (SalaryData != null)
                            {
                                foreach (var item in SalaryData)
                                {
                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        //Fld1 = item.Company_Code.ToString(),
                                        //Fld2 = item.Company_Name,
                                        //Fld4 = item.Geostruct_Id,
                                        //Fld5 = item.Location_Id,
                                        //Fld6 = item.LocCode,
                                        Fld7 = item.LocDesc,
                                        Fld8 = paylable,
                                        //Fld9 = item.FunStruct_Details,
                                        //Fld10 = item.Job_Id,
                                        //Fld11 = item.Job_Code,
                                        //Fld12 = item.Job_Name,
                                        //Fld13 = item.JobPosition_Id,
                                        //Fld14 = item.JobPositionCode,
                                        //Fld15 = item.JobPositionDesc,
                                        Fld16 = item.PayMonth,
                                        //  Fld17 = item.SalHeadCode,
                                        Fld18 = item.EarnHead,
                                        Fld32 = item.SequenceNo,
                                        //  Fld19 = item.SalHead_FullDetails,
                                        Fld20 = item.LookupVal,
                                        //  Fld21 = item.InPayslip,
                                        // Fld22 = item.StdAmount,
                                        Fld23 = item.EarnAmount,
                                        Fld24 = item.LocTotalEarning,
                                        Fld25 = item.LocTotalDeduction,
                                        Fld26 = item.LocTotalNet,
                                        Fld27 = item.LocTotalEmployee,
                                        //Fld28 = item.JobPosTotalEarning,
                                        //Fld29 = item.JobPosTotalDeduction,
                                        //Fld30 = item.JobPosTotalNet,
                                        //Fld31 = item.JobPosTotalEmployee 

                                        Fld33 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Bank Contribution: " : "",
                                        Fld34 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalPFBankContribution : "",

                                        Fld35 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Pension Amount: " : "",
                                        Fld36 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalPfPensionAmount : "",

                                        Fld37 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Relief Fund Bank Contribution: " : "",
                                        Fld38 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalReliefFundBankContribution : "",

                                        Fld39 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "SWF Bank Contribution: " : "",
                                        Fld40 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalSWFBankContribution : "",

                                        Fld41 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? "Total Bank Contribution: " : "",
                                        Fld42 = !String.IsNullOrEmpty(CompanyCode) && CompanyCode == "PDCC" ? item.TotalBankCONTRIBUTION : "",


                                    };

                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    default:
                        return null;
                        break;

                }

            }

        }

        public static List<Utility.ReportClass> GetViewData(int Flag)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Flag == 0)
                {
                    List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                    var query1 = "select * from VGeoStructD ";
                    if (query1 != "")
                    {
                        GeoData = db.Database.SqlQuery<Utility.ReportClass>(query1).ToList<Utility.ReportClass>();
                    }

                    return GeoData;
                }
                //}
                //if (chkpaystruct == 1)
                //{
                if (Flag == 1)
                {
                    List<Utility.ReportClass> PayData = new List<Utility.ReportClass>();

                    var query2 = "select * from VPayStructD ";
                    if (query2 != "")
                    {
                        PayData = db.Database.SqlQuery<Utility.ReportClass>(query2).ToList<Utility.ReportClass>();
                    }
                    return PayData;
                }

                // }

                //if (chkfunstruct == 1)
                //{
                if (Flag == 2)
                {
                    List<Utility.ReportClass> FuncData = new List<Utility.ReportClass>();
                    var query3 = "select * from VFuncStructD ";
                    if (query3 != "")
                    {
                        FuncData = db.Database.SqlQuery<Utility.ReportClass>(query3).ToList<Utility.ReportClass>();
                    }
                    return FuncData;
                }

                return null;

            }
        }

        public static Utility.ReportClass GetViewDataIndiv(int geoid, int payid, int funid, List<Utility.ReportClass> GrpInfoPass, int Flag)
        {

            Utility.ReportClass GrpInfo = new Utility.ReportClass();
            if (Flag == 0)
            {
                GrpInfo = GrpInfoPass.Where(e => e.Geo_Struct_Id == geoid).SingleOrDefault();
                return GrpInfo;
            }


            if (Flag == 1)
            {
                GrpInfo = GrpInfoPass.Where(e => e.PayStruct_Id == payid).SingleOrDefault();
                return GrpInfo;
            }


            if (Flag == 2)
            {
                GrpInfo = GrpInfoPass.Where(e => e.FuncStruct_Id == funid).SingleOrDefault();
                return GrpInfo;
            }


            return null;
        }

        public static GenericField100 GetFilterData(List<string> forithead, GenericField100 OGenericObjStatement, string paymonth, string employee, Utility.ReportClass GeodataInd, Utility.ReportClass PaydataInd, Utility.ReportClass FuncdataInd)
        {
            var month = false;
            var emp = false;
            var dept = false;
            var loca = false;
            var comp = false;
            var grp = false;
            var unit = false;
            var div = false;
            var regn = false;
            var grade = false;
            var lvl = false;
            var jobstat = false;
            var job = false;
            var jobpos = false;

            using (DataBaseContext db = new DataBaseContext())
            {

                var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                foreach (var item in vc)
                {
                    if (item.LookupVal.ToUpper() == "MONTH")
                    {
                        month = true;
                    }
                    if (item.LookupVal.ToUpper() == "LOCATION")
                    {

                        loca = true;
                    }
                    if (item.LookupVal.ToUpper() == "EMPLOYEE")
                    {
                        emp = true;
                    }
                    if (item.LookupVal.ToUpper() == "DEPARTMENT")
                    {
                        dept = true;
                    }
                    if (item.LookupVal.ToUpper() == "COMPANY")
                    {
                        comp = true;
                    }
                    if (item.LookupVal.ToUpper() == "GROUP")
                    {
                        grp = true;
                    }
                    if (item.LookupVal.ToUpper() == "UNIT")
                    {
                        unit = true;
                    }
                    if (item.LookupVal.ToUpper() == "DIVISION")
                    {
                        div = true;
                    }
                    if (item.LookupVal.ToUpper() == "REGION")
                    {
                        regn = true;
                    }
                    if (item.LookupVal.ToUpper() == "GRADE")
                    {
                        grade = true;
                    }
                    if (item.LookupVal.ToUpper() == "LEVEL")
                    {
                        lvl = true;
                    }
                    if (item.LookupVal.ToUpper() == "JOBSTATUS")
                    {
                        jobstat = true;
                    }

                    if (item.LookupVal.ToUpper() == "JOB")
                    {
                        job = true;
                    }
                    if (item.LookupVal.ToUpper() == "JOBPOSITION")
                    {
                        jobpos = true;
                    }
                }


                if (month)
                {
                    OGenericObjStatement.Fld100 = paymonth;
                }
                if (comp)
                {
                    OGenericObjStatement.Fld99 = GeodataInd.Company_Name;
                }
                if (div)
                {
                    OGenericObjStatement.Fld98 = GeodataInd.Division_Name;
                }
                if (loca)
                {
                    OGenericObjStatement.Fld97 = GeodataInd.LocDesc;
                }
                if (dept)
                {
                    OGenericObjStatement.Fld96 = GeodataInd.DeptDesc;
                }
                if (grp)
                {
                    OGenericObjStatement.Fld95 = GeodataInd.Group_Name;
                }
                if (unit)
                {
                    OGenericObjStatement.Fld94 = GeodataInd.Unit_Name;
                }
                if (grade)
                {
                    OGenericObjStatement.Fld93 = PaydataInd.Grade_Name;
                }
                if (lvl)
                {
                    OGenericObjStatement.Fld92 = PaydataInd.Level_Name;
                }
                if (jobstat)
                {
                    OGenericObjStatement.Fld91 = GeodataInd.JobStatus_Id.ToString();
                }
                if (job)
                {
                    OGenericObjStatement.Fld90 = FuncdataInd.Job_Name;
                }
                if (jobpos)
                {
                    OGenericObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                }
                if (emp)
                {
                    OGenericObjStatement.Fld88 = employee;
                }

                return OGenericObjStatement;
            }
        }

        public class returnClass
        {
            public Int32 Id { get; set; }
            public string EmpCode { get; set; }
            public string PFNo { get; set; }
            public string PTNo { get; set; }
            public string PANNo { get; set; }
            public string AdharNo { get; set; }
            public string UANNo { get; set; }
            public string PensionNo { get; set; }
            public string ESICNo { get; set; }
            public string LWFNo { get; set; }
            public string EDLINo { get; set; }
            public string VCNo { get; set; }

            public string BirthDate { get; set; }
            public string JoiningDate { get; set; }
            public string ProbationPeriod { get; set; }
            public string ProbationDate { get; set; }
            public string ConfirmPeriod { get; set; }
            public string ConfirmationDate { get; set; }
            public string LastIncrementDate { get; set; }
            public string LastPromotionDate { get; set; }
            public string LastTransferDate { get; set; }
            public string SeniorityDate { get; set; }
            public string SeniorityNo { get; set; }
            public string RetirementDate { get; set; }
            public string ServiceLastDate { get; set; }
            public string PFJoingDate { get; set; }
            public string PensionJoingDate { get; set; }
            public string PFExitDate { get; set; }
            public string PensionExitDate { get; set; }
            // public ServiceBookDates ServiceBookDates { get; set; }
        }

        public static EmployeePayroll _returnGetEmployeeData(Int32 item, string MonYr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var empPayroll = db.EmployeePayroll.Where(e => e.Employee.Id == item).OrderBy(e => e.Id).SingleOrDefault();

                db.Entry(empPayroll).Collection(x => x.SalaryT).Query().Where(t => t.PayMonth == MonYr).Load();
                if (empPayroll.SalaryT != null)
                {
                    db.Entry(empPayroll.SalaryT.First()).Collection(x => x.PayslipR).Load();
                    if (db.Entry(empPayroll.SalaryT.First()).Collection(x => x.PayslipR).Query().Count() != 0)
                    {
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Collection(x => x.PaySlipDetailEarnR).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Collection(x => x.PaySlipDetailDedR).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Collection(x => x.PaySlipDetailLeaveR).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First()).Reference(x => x.GeoStruct).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First().GeoStruct).Reference(x => x.Location).Load();
                        db.Entry(empPayroll.SalaryT.First().PayslipR.First().GeoStruct.Location).Reference(x => x.LocationObj).Load();
                    }
                }


                return empPayroll;
            }
        }
    }
}