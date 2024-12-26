using EssPortal.App_Start;
using ReportPayroll;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using Payroll;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
namespace EssPortal.Process
{
    public class ReportRDLCObjectClass
    {
        public static List<GenericField100> GenerateBankStatement(Int32 CompId, List<int> EmpPayrollIdList, string mPayMonth, DataBaseContext db)
        {
            using (DataBaseContext db1 = new DataBaseContext())
            {
                var OSalHead = db1.CompanyPayroll
                    .Include(e => e.SalaryHead)
                    .Include(e => e.SalaryHead.Select(r => r.Type))
                    .Where(d => d.Id == CompId).SingleOrDefault();


                var OEmpData = db1.EmployeePayroll
                        .Include(e => e.SalaryT)
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.EmpOffInfo)
                        .Include(e => e.Employee.EmpOffInfo.Bank)
                        .Include(e => e.Employee.EmpOffInfo.Branch)
                        .Include(e => e.Employee.EmpOffInfo.AccountType)
                        .Include(e => e.SalaryT.Select(r => r.Geostruct.Region))
                        .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                        .Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                        .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                        .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                        .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                        .ToList();

                var OSalTchk = OEmpData.Select(e => e.SalaryT).ToList();
                var mHeader = false;
                if (OEmpData.Count() == 0 || OSalHead == null || OSalTchk == null)
                {
                    return null;
                }
                List<GenericField100> OGenericSalaryStatement = new List<GenericField100>();
                foreach (var OEmpsingle in OEmpData)
                {
                    var OEarnDedData = OEmpsingle.SalaryT.Where(g => g.PayMonth == mPayMonth)
                             .Select(e => e.SalEarnDedT.Where(y => y.SalaryHead.InPayslip == true)
                            .Select(r => new
                            {
                                regionid = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Id.ToString() : null,
                                regioncode = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Code : null,
                                regionname = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Name : null,
                                emppayrollid = OEmpsingle.Id.ToString(),
                                empid = OEmpsingle.Employee.Id.ToString(),
                                empcode = OEmpsingle.Employee.EmpCode,
                                empname = OEmpsingle.Employee.EmpName != null ? OEmpsingle.Employee.EmpName.FullNameFML : null,
                                paymonth = e.PayMonth,
                                locid = e.Geostruct != null && e.Geostruct.Location != null ? e.Geostruct.Location.Id.ToString() : null,
                                loccode = e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.LocationObj != null ? e.Geostruct.Location.LocationObj.LocCode : null,
                                locname = e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.LocationObj != null ? e.Geostruct.Location.LocationObj.LocDesc : null,
                                DepartmentId = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.Id.ToString() : null,
                                DepartmentCode = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.DeptCode.ToString() : null,
                                DepartmentName = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                                GroupId = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Id.ToString() : null,
                                GroupName = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Name.ToString() : null,
                                GroupCode = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Code.ToString() : null,
                                JobStatusName = e.PayStruct != null && e.PayStruct.JobStatus != null && e.PayStruct.JobStatus.EmpActingStatus != null ? e.PayStruct.JobStatus.EmpActingStatus.LookupVal : null,
                                JobStatusId = e.PayStruct != null && e.PayStruct.JobStatus != null ? e.PayStruct.JobStatus.Id.ToString() : null,
                                GradeId = e.PayStruct != null && e.PayStruct.Grade != null ? e.PayStruct.Grade.Id.ToString() : null,
                                GradeName = e.PayStruct != null && e.PayStruct.Grade != null ? e.PayStruct.Grade.Name : null,
                                jobid = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Id.ToString() : null,
                                jobcode = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Code : null,
                                jobname = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Name : null,
                                salcodeid = r.SalaryHead != null ? r.SalaryHead.Id.ToString() : null,
                                salcode = r.SalaryHead != null ? r.SalaryHead.Code.ToString() : null,
                                salname = r.SalaryHead != null ? r.SalaryHead.Name.ToString() : null,
                                amount = r.Amount,
                                saltype = r.SalaryHead != null && r.SalaryHead.Type != null ? r.SalaryHead.Type.LookupVal.ToString() : null,
                                salheadseqno = r.SalaryHead != null ? r.SalaryHead.SeqNo.ToString() : null,
                                salOptype = r.SalaryHead != null && r.SalaryHead.SalHeadOperationType != null ? r.SalaryHead.SalHeadOperationType.LookupVal.ToString() : null,
                                AccountType = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.AccountType != null ? OEmpsingle.Employee.EmpOffInfo.AccountType.LookupVal : null,
                                AccountNo = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null ? OEmpsingle.Employee.EmpOffInfo.AccountNo : null,
                                TotalEarning = e.TotalEarning.ToString(),
                                TotalDeduction = e.TotalDeduction.ToString(),
                                TotalNet = e.TotalNet.ToString(),
                                BranchId = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Branch != null ? OEmpsingle.Employee.EmpOffInfo.Branch.Id.ToString() : null,
                                BankId = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Bank != null ? OEmpsingle.Employee.EmpOffInfo.Bank.Id.ToString() : null,
                                BranchCode = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Branch != null ? OEmpsingle.Employee.EmpOffInfo.Branch.Code : null,
                                BankCode = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Bank != null ? OEmpsingle.Employee.EmpOffInfo.Bank.Code : null,
                            }).FirstOrDefault());

                    var OEarnData = OEarnDedData.ToList();
                    if (OEarnData.Count > 0)
                    {
                        foreach (var ca in OEarnData)
                        {
                            GenericField100 OEarnStatement = new GenericField100()
                            {
                                Fld4 = ca.regionid,
                                Fld5 = ca.regioncode,
                                Fld6 = ca.regionname,
                                Fld7 = ca.locid,
                                Fld8 = ca.loccode,
                                Fld9 = ca.locname,
                                Fld10 = ca.DepartmentId,
                                Fld11 = ca.DepartmentCode,
                                Fld12 = ca.DepartmentName,
                                Fld13 = ca.GroupId,
                                Fld14 = ca.GroupCode,
                                Fld15 = ca.GroupName,
                                Fld16 = ca.jobid,
                                Fld17 = ca.jobcode,
                                Fld18 = ca.jobname,
                                Fld19 = ca.JobStatusId,
                                Fld20 = ca.GradeName,
                                Fld21 = ca.jobname,
                                Fld22 = ca.emppayrollid,
                                Fld23 = ca.empcode,
                                Fld24 = ca.empname,
                                Fld25 = ca.paymonth,
                                Fld26 = ca.AccountType,
                                Fld27 = ca.AccountNo,
                                Fld28 = ca.BankId,
                                Fld29 = ca.BankCode,
                                Fld30 = ca.BranchId,
                                Fld31 = ca.BranchCode,
                                Fld32 = ca.TotalEarning,
                                Fld33 = ca.TotalDeduction,
                                Fld34 = ca.TotalNet,
                            };
                            OGenericSalaryStatement.Add(OEarnStatement);
                        }
                    }
                }
                if (OGenericSalaryStatement != null && OGenericSalaryStatement.Count > 0)
                {

                    return OGenericSalaryStatement.Distinct().ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        //public static List<GenericField100> GenerateBankStatement(int CompId, List<int> EmpPayrollIdList, string mPayMonth)
        //{
        //    var OSalHead = db.CompanyPayroll
        //        .Include(e => e.SalaryHead)
        //        .Include(e => e.SalaryHead.Select(r => r.Type))
        //        .Where(d => d.Id == CompId).SingleOrDefault();


        //    var OEmpData = db.EmployeePayroll
        //            .Include(e => e.SalaryT)
        //            .Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
        //            .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
        //            .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
        //            .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
        //            .Include(e => e.Employee)
        //        .Include(e => e.Employee.EmpName)
        //        .Include(e => e.Employee.EmpOffInfo)
        //        .Include(e => e.Employee.EmpOffInfo.Bank)
        //        .Include(e => e.Employee.EmpOffInfo.Branch)
        //            .Include(e => e.SalaryT.Select(r => r.Geostruct.Region))
        //            .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
        //            .Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
        //            .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
        //            .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
        //            .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
        //            .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
        //            .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
        //            .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
        //            .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))

        //            .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
        //            .ToList();

        //    var OSalTchk = OEmpData.Select(e => e.SalaryT).ToList();
        //    var mHeader = false;
        //    if (OEmpData.Count() == 0 || OSalHead == null || OSalTchk == null)
        //    {
        //        return null;
        //    }
        //    List<GenericField50> OGenericSalaryStatement = new List<GenericField50>();
        //    foreach (var OEmpsingle in OEmpData)
        //    {
        //        var OEarnDedData = OEmpsingle.SalaryT.Where(g => g.PayMonth == mPayMonth)
        //                 .Select(e => e.SalEarnDedT.Where(y => y.SalaryHead.InPayslip == true)
        //                .Select(r => new
        //                {
        //                    regionid = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Id.ToString() : null,
        //                    regioncode = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Code : null,
        //                    regionname = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Name : null,
        //                    emppayrollid = OEmpsingle.Id.ToString(),
        //                    empid = OEmpsingle.Employee.Id.ToString(),
        //                    empcode = OEmpsingle.Employee.EmpCode,
        //                    empname = OEmpsingle.Employee.EmpName != null ? OEmpsingle.Employee.EmpName.FullNameFML : null,
        //                    paymonth = e.PayMonth,
        //                    locid = e.Geostruct != null && e.Geostruct.Location != null ? e.Geostruct.Location.Id.ToString() : null,
        //                    loccode = e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.LocationObj != null ? e.Geostruct.Location.LocationObj.LocCode : null,
        //                    locname = e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.LocationObj != null ? e.Geostruct.Location.LocationObj.LocDesc : null,
        //                    DepartmentId = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.Id.ToString() : null,
        //                    DepartmentCode = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.DeptCode.ToString() : null,
        //                    DepartmentName = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.DeptDesc.ToString() : null,
        //                    GroupId = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Id.ToString() : null,
        //                    GroupName = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Name.ToString() : null,
        //                    GroupCode = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Code.ToString() : null,
        //                    JobStatusName = e.PayStruct != null && e.PayStruct.JobStatus != null && e.PayStruct.JobStatus.EmpActingStatus != null ? e.PayStruct.JobStatus.EmpActingStatus.LookupVal : null,
        //                    JobStatusId = e.PayStruct != null && e.PayStruct.JobStatus != null ? e.PayStruct.JobStatus.Id.ToString() : null,
        //                    jobid = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Id.ToString() : null,
        //                    jobcode = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Code : null,
        //                    jobname = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Name : null,
        //                    salcodeid = r.SalaryHead != null ? r.SalaryHead.Id.ToString() : null,
        //                    salcode = r.SalaryHead != null ? r.SalaryHead.Code.ToString() : null,
        //                    salname = r.SalaryHead != null ? r.SalaryHead.Name.ToString() : null,
        //                    amount = r.Amount,
        //                    saltype = r.SalaryHead != null && r.SalaryHead.Type != null ? r.SalaryHead.Type.LookupVal.ToString() : null,
        //                    salheadseqno = r.SalaryHead != null ? r.SalaryHead.SeqNo.ToString() : null,
        //                    salOptype = r.SalaryHead != null && r.SalaryHead.SalHeadOperationType != null ? r.SalaryHead.SalHeadOperationType.LookupVal.ToString() : null,
        //                    AccountType = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.AccountType != null ? OEmpsingle.Employee.EmpOffInfo.AccountType.LookupVal : null,
        //                    AccountNo = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null ? OEmpsingle.Employee.EmpOffInfo.AccountNo : null,
        //                    TotalEarning = e.TotalEarning.ToString(),
        //                    TotalDeduction = e.TotalDeduction.ToString(),
        //                    TotalNet = e.TotalNet.ToString(),
        //                    BranchId = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Branch != null ? OEmpsingle.Employee.EmpOffInfo.Branch.Id.ToString() : null,
        //                    BankId = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Bank != null ? OEmpsingle.Employee.EmpOffInfo.Bank.Id.ToString() : null,
        //                    BranchCode = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Branch != null ? OEmpsingle.Employee.EmpOffInfo.Branch.Code : null,
        //                    BankCode = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpOffInfo != null && OEmpsingle.Employee.EmpOffInfo.Bank != null ? OEmpsingle.Employee.EmpOffInfo.Bank.Code : null,
        //                })).SingleOrDefault();

        //        var OEarnData = OEarnDedData.ToList();
        //        if (OEarnData.Count > 0)
        //        {
        //            foreach (var ca in OEarnData)
        //            {
        //                GenericField50 OEarnStatement = new GenericField50()
        //                {
        //                    Fld4 = ca.regionid,
        //                    Fld5 = ca.regioncode,
        //                    Fld6 = ca.regionname,
        //                    Fld7 = ca.locid,
        //                    Fld8 = ca.loccode,
        //                    Fld9 = ca.locname,
        //                    Fld10 = ca.DepartmentId,
        //                    Fld11 = ca.DepartmentCode,
        //                    Fld12 = ca.DepartmentName,
        //                    Fld13 = ca.GroupId,
        //                    Fld14 = ca.GroupCode,
        //                    Fld15 = ca.GroupName,
        //                    Fld16 = ca.jobid,
        //                    Fld17 = ca.jobcode,
        //                    Fld18 = ca.jobname,
        //                    Fld19 = ca.JobStatusId,
        //                    Fld20 = ca.JobStatusName,
        //                    Fld21 = ca.jobname,
        //                    Fld22 = ca.emppayrollid,
        //                    Fld23 = ca.empcode,
        //                    Fld24 = ca.empname,
        //                    Fld25 = ca.paymonth,
        //                    Fld26 = ca.AccountType,
        //                    Fld27 = ca.AccountNo,
        //                    Fld28 = ca.BankId,
        //                    Fld29 = ca.BankCode,
        //                    Fld30 = ca.BranchId,
        //                    Fld31 = ca.BranchCode,
        //                    Fld32 = ca.TotalEarning,
        //                    Fld33 = ca.TotalDeduction,
        //                    Fld34 = ca.TotalNet,
        //                };
        //                OGenericSalaryStatement.Add(OEarnStatement);
        //            }
        //        }
        //    }
        //    return OGenericSalaryStatement.ToList();
        //}

        public static List<GenericField100> GenerateSalRegister(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, DataBaseContext db)
        {
            using (DataBaseContext db1 = new DataBaseContext())
            {
                var OSalHead = db1.CompanyPayroll
                    .Include(e => e.SalaryHead)
                    .Include(e => e.SalaryHead.Select(r => r.Type))
                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                var OEmpData = db1.EmployeePayroll
                        .Include(e => e.SalaryT)
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                        .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                        .Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                        .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                        .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                        .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                        .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                        .ToList();

                var OSalTchk = OEmpData.Select(e => e.SalaryT).ToList();
                var mHeader = false;
                if (OEmpData.Count() == 0 || OSalHead == null || OSalTchk == null)
                {
                    return null;
                }
                List<GenericField100> OEmpSalReg = new List<GenericField100>();
                foreach (var OEmpsingle in OEmpData)
                {


                    var OEarnDedData = OEmpsingle.SalaryT.Where(g => mPayMonth.Contains(g.PayMonth))
                             .Select(e => e.SalEarnDedT.Where(y => y.SalaryHead.InPayslip == true)
                            .Select(r => new
                            {
                                regionid = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Id.ToString() : null,
                                regioncode = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Code : null,
                                regionname = e.Geostruct != null && e.Geostruct.Region != null ? e.Geostruct.Region.Name : null,
                                emppayrollid = OEmpsingle.Id.ToString(),
                                empid = OEmpsingle.Employee.Id.ToString(),
                                empcode = OEmpsingle.Employee.EmpCode,
                                empname = OEmpsingle.Employee != null && OEmpsingle.Employee.EmpName != null ? OEmpsingle.Employee.EmpName.FullNameFML : null,
                                paymonth = e.PayMonth,
                                locid = e.Geostruct != null && e.Geostruct.Location != null ? e.Geostruct.Location.Id.ToString() : null,
                                loccode = e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.LocationObj != null ? e.Geostruct.Location.LocationObj.LocCode : null,
                                locname = e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.LocationObj != null ? e.Geostruct.Location.LocationObj.LocDesc : null,
                                DepartmentId = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.Id.ToString() : null,
                                DepartmentCode = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.DeptCode.ToString() : null,
                                DepartmentName = e.Geostruct != null && e.Geostruct.Department != null && e.Geostruct.Department.DepartmentObj != null ? e.Geostruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                                GroupId = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Id.ToString() : null,
                                GroupName = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Name.ToString() : null,
                                GroupCode = e.Geostruct != null && e.Geostruct.Group != null ? e.Geostruct.Group.Code.ToString() : null,
                                JobStatusName = e.PayStruct != null && e.PayStruct.JobStatus != null && e.PayStruct.JobStatus.EmpActingStatus != null ? e.PayStruct.JobStatus.EmpActingStatus.LookupVal : null,
                                JobStatusId = e.PayStruct != null && e.PayStruct.JobStatus != null ? e.PayStruct.JobStatus.Id.ToString() : null,
                                jobid = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Id.ToString() : null,
                                jobcode = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Code : null,
                                jobname = e.FuncStruct != null && e.FuncStruct.Job != null ? e.FuncStruct.Job.Name : null,
                                salcodeid = r.SalaryHead != null ? r.SalaryHead.Id.ToString() : null,
                                salcode = r.SalaryHead != null ? r.SalaryHead.Code.ToString() : null,
                                salname = r.SalaryHead != null ? r.SalaryHead.Name.ToString() : null,
                                amount = r.Amount,
                                saltype = r.SalaryHead != null && r.SalaryHead.Type != null ? r.SalaryHead.Type.LookupVal.ToString() : null,
                                salheadseqno = r.SalaryHead != null ? r.SalaryHead.SeqNo.ToString() : null,
                                salOptype = r.SalaryHead != null && r.SalaryHead.SalHeadOperationType != null ? r.SalaryHead.SalHeadOperationType.LookupVal.ToString() : null,

                            }
                            )).FirstOrDefault();
                    GenericField100 OEarnStatement = new GenericField100();
                    if (mHeader == false)//write header to first two lines
                    { }
                    foreach (var ca in OEarnDedData)
                    {
                        OEarnStatement.Fld4 = ca.regionid;
                        OEarnStatement.Fld5 = ca.regioncode;
                        OEarnStatement.Fld6 = ca.regionname;
                        OEarnStatement.Fld7 = ca.locid;
                        OEarnStatement.Fld8 = ca.loccode;
                        OEarnStatement.Fld9 = ca.locname;
                        OEarnStatement.Fld10 = ca.DepartmentId;
                        OEarnStatement.Fld11 = ca.DepartmentCode;
                        OEarnStatement.Fld12 = ca.DepartmentName;
                        OEarnStatement.Fld13 = ca.GroupId;
                        OEarnStatement.Fld14 = ca.GroupCode;
                        OEarnStatement.Fld15 = ca.GroupName;
                        OEarnStatement.Fld16 = ca.jobid;
                        OEarnStatement.Fld17 = ca.jobcode;
                        OEarnStatement.Fld18 = ca.jobname;
                        OEarnStatement.Fld19 = ca.JobStatusId;
                        OEarnStatement.Fld20 = ca.JobStatusName;
                        OEarnStatement.Fld21 = ca.jobname;
                        OEarnStatement.Fld22 = ca.emppayrollid;
                        OEarnStatement.Fld23 = ca.empcode;
                        OEarnStatement.Fld24 = ca.empname;
                        OEarnStatement.Fld25 = ca.paymonth;

                    }
                    //GenericField100 OSalRegObj = new GenericField100();
                    var OEarnData = OEarnDedData.Where(e => e.saltype.ToUpper() == "EARNING").ToList();

                    foreach (var ca in OEarnData)
                    {
                        switch (ca.salheadseqno)
                        {
                            case "1":
                                OEarnStatement.Fld26 = ca.salcode.ToString();
                                break;
                            case "2":
                                OEarnStatement.Fld27 = ca.salcode.ToString();
                                break;
                            case "3":
                                OEarnStatement.Fld28 = ca.salcode.ToString();
                                break;
                            case "4":
                                OEarnStatement.Fld29 = ca.salcode.ToString();
                                break;
                            case "5":
                                OEarnStatement.Fld30 = ca.salcode.ToString();
                                break;
                            case "6":
                                OEarnStatement.Fld31 = ca.salcode.ToString();
                                break;
                            case "7":
                                OEarnStatement.Fld32 = ca.salcode.ToString();
                                break;
                            case "8":
                                OEarnStatement.Fld33 = ca.salcode.ToString();
                                break;
                            case "9":
                                OEarnStatement.Fld34 = ca.salcode.ToString();
                                break;
                            case "10":
                                OEarnStatement.Fld35 = ca.salcode.ToString();
                                break;
                            case "11":
                                OEarnStatement.Fld36 = ca.salcode.ToString();
                                break;
                            case "12":
                                OEarnStatement.Fld37 = ca.salcode.ToString();
                                break;
                            case "13":
                                OEarnStatement.Fld38 = ca.salcode.ToString();
                                break;
                            case "14":
                                OEarnStatement.Fld39 = ca.salcode.ToString();
                                break;
                            case "15":
                                OEarnStatement.Fld40 = ca.salcode.ToString();

                                break;
                        }

                    }
                    var ODedData = OEarnDedData.Where(e => e.saltype.ToUpper() == "DEDUCTION").ToList();
                    foreach (var ca in ODedData)
                    {

                        switch (ca.salheadseqno)
                        {
                            case "1":
                                OEarnStatement.Fld41 = ca.salcode.ToString();
                                break;
                            case "2":
                                OEarnStatement.Fld42 = ca.salcode.ToString();
                                break;
                            case "3":
                                OEarnStatement.Fld43 = ca.salcode.ToString();
                                break;
                            case "4":
                                OEarnStatement.Fld44 = ca.salcode.ToString();
                                break;
                            case "5":
                                OEarnStatement.Fld45 = ca.salcode.ToString();
                                break;
                            case "6":
                                OEarnStatement.Fld46 = ca.salcode.ToString();
                                break;
                            case "7":
                                OEarnStatement.Fld47 = ca.salcode.ToString();
                                break;
                            case "8":
                                OEarnStatement.Fld48 = ca.salcode.ToString();
                                break;
                            case "9":
                                OEarnStatement.Fld49 = ca.salcode.ToString();
                                break;
                            case "10":
                                OEarnStatement.Fld50 = ca.salcode.ToString();
                                break;
                            case "11":
                                OEarnStatement.Fld51 = ca.salcode.ToString();
                                break;
                            case "12":
                                OEarnStatement.Fld52 = ca.salcode.ToString();
                                break;
                            case "13":
                                OEarnStatement.Fld53 = ca.salcode.ToString();
                                break;
                            case "14":
                                OEarnStatement.Fld54 = ca.salcode.ToString();
                                break;
                            case "15":
                                OEarnStatement.Fld55 = ca.salcode.ToString();

                                break;
                        }

                    }
                    mHeader = true;
                    var OEarnAmtData = OEarnDedData.Where(e => e.saltype.ToUpper() == "EARNING").ToList();
                    if (OEarnAmtData.Count() > 0)
                    {
                        foreach (var ca in OEarnAmtData)
                        {

                            switch (ca.salheadseqno)
                            {
                                case "1":
                                    OEarnStatement.Fld56 = ca.amount.ToString();
                                    break;
                                case "2":
                                    OEarnStatement.Fld57 = ca.amount.ToString();
                                    break;
                                case "3":
                                    OEarnStatement.Fld58 = ca.amount.ToString();
                                    break;
                                case "4":
                                    OEarnStatement.Fld59 = ca.amount.ToString();
                                    break;
                                case "5":
                                    OEarnStatement.Fld60 = ca.amount.ToString();
                                    break;
                                case "6":
                                    OEarnStatement.Fld61 = ca.amount.ToString();
                                    break;
                                case "7":
                                    OEarnStatement.Fld62 = ca.amount.ToString();
                                    break;
                                case "8":
                                    OEarnStatement.Fld63 = ca.amount.ToString();
                                    break;
                                case "9":
                                    OEarnStatement.Fld64 = ca.amount.ToString();
                                    break;
                                case "10":
                                    OEarnStatement.Fld65 = ca.amount.ToString();
                                    break;
                                case "11":
                                    OEarnStatement.Fld66 = ca.amount.ToString();
                                    break;
                                case "12":
                                    OEarnStatement.Fld67 = ca.amount.ToString();
                                    break;
                                case "13":
                                    OEarnStatement.Fld68 = ca.amount.ToString();
                                    break;
                                case "14":
                                    OEarnStatement.Fld69 = ca.amount.ToString();
                                    break;
                                case "15":
                                    OEarnStatement.Fld70 = ca.amount.ToString();

                                    break;
                            }
                        }
                    }
                    var ODedAmtData = OEarnDedData.Where(e => e.saltype.ToUpper() == "DEDUCTION").ToList();
                    if (ODedAmtData.Count() > 0)
                    {

                        foreach (var ca in OEarnDedData)
                        {

                            switch (ca.salheadseqno)
                            {
                                case "1":
                                    OEarnStatement.Fld71 = ca.amount.ToString();
                                    break;
                                case "2":
                                    OEarnStatement.Fld72 = ca.amount.ToString();
                                    break;
                                case "3":
                                    OEarnStatement.Fld73 = ca.amount.ToString();
                                    break;
                                case "4":
                                    OEarnStatement.Fld74 = ca.amount.ToString();
                                    break;
                                case "5":
                                    OEarnStatement.Fld75 = ca.amount.ToString();
                                    break;
                                case "6":
                                    OEarnStatement.Fld76 = ca.amount.ToString();
                                    break;
                                case "7":
                                    OEarnStatement.Fld77 = ca.amount.ToString();
                                    break;
                                case "8":
                                    OEarnStatement.Fld78 = ca.amount.ToString();
                                    break;
                                case "9":
                                    OEarnStatement.Fld79 = ca.amount.ToString();
                                    break;
                                case "10":
                                    OEarnStatement.Fld80 = ca.amount.ToString();
                                    break;
                                case "11":
                                    OEarnStatement.Fld81 = ca.amount.ToString();
                                    break;
                                case "12":
                                    OEarnStatement.Fld82 = ca.amount.ToString();
                                    break;
                                case "13":
                                    OEarnStatement.Fld83 = ca.amount.ToString();
                                    break;
                                case "14":
                                    OEarnStatement.Fld84 = ca.amount.ToString();
                                    break;
                                case "15":
                                    OEarnStatement.Fld85 = ca.amount.ToString();

                                    break;

                            }
                        }

                    }
                    //}
                    OEmpSalReg.Add(OEarnStatement);
                }
                if (OEmpSalReg.Count > 0 && OEmpSalReg != null)
                {
                    // db.GenericField100.AddRange(OEmpSalReg.Distinct());
                    //db.SaveChanges();
                    return OEmpSalReg.Distinct().ToList();

                }
                else
                {
                    return null;
                }

            }
        }
        public static List<GenericField100>
           GenerateCoreReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, DataBaseContext db)
        {
            List<GenericField100> OGenericCoreStatement = new List<GenericField100>();
            switch (mObjectName)
            {

                case "COMPANY":
                    var OGeoCompData = db.Company
                    .Include(e => e.Address.FullAddress)
                    .Include(e => e.ContactDetails.FullContactDetails)
                    .Where(d => d.Id == CompanyId).SingleOrDefault();

                    if (OGeoCompData == null)
                    {
                        return null;
                    }
                    else
                    {
                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                        {
                            Fld1 = OGeoCompData.Id.ToString(),
                            Fld2 = OGeoCompData.Code,
                            Fld3 = OGeoCompData.Name,
                            Fld4 = OGeoCompData.Address == null ? "" : OGeoCompData.Address.FullAddress,
                            Fld5 = OGeoCompData.ContactDetails == null ? "" : OGeoCompData.ContactDetails.FullContactDetails,
                            Fld6 = OGeoCompData.CINNo,
                            Fld7 = OGeoCompData.CSTNo,
                            Fld8 = OGeoCompData.ESICNo,
                            Fld9 = OGeoCompData.EstablishmentNo,
                            Fld10 = OGeoCompData.LBTNO,
                            Fld11 = OGeoCompData.PANNo,
                            Fld12 = OGeoCompData.PTECNO,
                            Fld13 = OGeoCompData.PTRCNO,
                            Fld14 = OGeoCompData.RegistrationNo,
                            Fld15 = OGeoCompData.RegistrationDate.ToString(),
                            Fld16 = OGeoCompData.ServiceTaxNo,
                            Fld17 = OGeoCompData.TANNo,
                            Fld18 = OGeoCompData.VATNo,
                        };
                        OGenericCoreStatement.Add(OGenericGeoObjStatement);
                        return OGenericCoreStatement;
                    }

                    break;

                case "LOCATION":
                    var OGeoLocData = db.Company
                    .Include(e => e.Location)
                    .Include(e => e.Location.Select(r => r.Address))
                    .Include(e => e.Location.Select(r => r.ContactDetails))
                    .Include(e => e.Location.Select(r => r.BusinessCategory))
                    .Include(e => e.Location.Select(r => r.Incharge))
                    .Include(e => e.Location.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Location.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Location.Select(r => r.LocationObj))
                    .Include(e => e.Location.Select(r => r.Type))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoLocData.Location == null || OGeoLocData.Location.Count == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OGeoLocData.Location)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.LocationObj == null ? "" : ca.LocationObj.LocCode,
                                Fld3 = ca.LocationObj == null ? "" : ca.LocationObj.LocDesc,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.BusinessCategory == null ? "" : ca.BusinessCategory.LookupVal.ToUpper(),
                                Fld7 = ca.Address == null ? "" : ca.Address.FullAddress,
                                Fld8 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld11 = ca.Incharge == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                //Fld12 = ca.WeeklyOffCalendar == null ? "" : ca.WeeklyOffCalendar.Id.ToString(),
                                //Fld13 = ca.HolidayCalendar == null ? "" : ca.HolidayCalendar.Id.ToString(),

                            };
                            //write data to generic class
                            OGenericCoreStatement.Add(OGenericGeoObjStatement);

                        }
                        return OGenericCoreStatement;
                    }
                    break;
                case "DIVISION":
                    var OGeoDivData = db.Company
                    .Include(e => e.Divisions)
                    .Include(e => e.Divisions.Select(r => r.Address))
                    .Include(e => e.Divisions.Select(r => r.ContactDetails))
                    .Include(e => e.Divisions.Select(r => r.Incharge))
                    .Include(e => e.Divisions.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Divisions.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Divisions.Select(r => r.Incharge.CorAddr))
                    .Include(e => e.Divisions.Select(r => r.Type))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoDivData.Divisions == null || OGeoDivData.Divisions.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OGeoDivData.Divisions)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.Code,
                                Fld3 = ca.Name,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.Address == null ? "" : ca.Address.FullAddress,
                                Fld7 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld8 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                Fld11 = ca.Incharge == null ? "" : ca.Incharge.CorAddr.FullAddress,

                            };
                            //write data to generic class
                            OGenericCoreStatement.Add(OGenericGeoObjStatement);
                        }
                        return OGenericCoreStatement;

                    }
                    break;

                case "DEPARTMENT":
                    var OGeoDeptData = db.Company
                    .Include(e => e.Department)
                    .Include(e => e.Department.Select(r => r.Address))
                    .Include(e => e.Department.Select(r => r.ContactDetails))
                    .Include(e => e.Department.Select(r => r.Incharge))
                    .Include(e => e.Department.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Department.Select(r => r.Type))
                    .Include(e => e.Department.Select(r => r.DepartmentObj))
                    .Include(e => e.Department.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Department.Select(r => r.Incharge.CorAddr))

                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoDeptData.Department == null || OGeoDeptData.Department.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OGeoDeptData.Department)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.DepartmentObj == null ? "" : ca.DepartmentObj.DeptCode,
                                Fld3 = ca.DepartmentObj == null ? "" : ca.DepartmentObj.DeptDesc,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.Address == null ? "" : ca.Address.FullAddress,
                                Fld7 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld8 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                Fld11 = ca.Incharge == null ? "" : ca.Incharge.CorAddr.FullAddress,

                            };
                            OGenericCoreStatement.Add(OGenericGeoObjStatement);


                            //write data to generic class

                        }
                        return OGenericCoreStatement;

                    }
                    break;
                case "GROUPR":
                    var OGeoGrpData = db.Company
                    .Include(e => e.Group)
                    .Include(e => e.Group.Select(r => r.ContactDetails))
                    .Include(e => e.Group.Select(r => r.Incharge))
                    .Include(e => e.Group.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Group.Select(r => r.Incharge.CorAddr))
                    .Include(e => e.Group.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Group.Select(r => r.Type))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoGrpData.Group == null || OGeoGrpData.Group.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OGeoGrpData.Group)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.Code,
                                Fld3 = ca.Name,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld7 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld8 = ca.Incharge == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.CorAddr.FullAddress,
                            };
                            //write data to generic class
                            OGenericCoreStatement.Add(OGenericGeoObjStatement);
                        }

                        return OGenericCoreStatement;

                    }
                    break;
                default:
                    return null;
                    break;
            }
            //return null;
        }
        public static List<GenericField100>
           GeneratePayrollReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, DataBaseContext db)
        {
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();

            switch (mObjectName)
            {
                ////////////////////////////////Core///////////////////////////

                case "GRADE":
                    var OGeoGradeData = db.Company
                    .Include(e => e.Address)
                    .Include(e => e.ContactDetails)
                    .Include(e => e.Grade)
                    .Where(d => d.Id == CompanyId).SingleOrDefault();

                    if (OGeoGradeData == null)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OGeoGradeData.Grade)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.Code,
                                Fld3 = ca.Name,

                            };
                            //write data to generic class
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                        }
                        return OGenericPayrollStatement;
                    }

                    break;

                case "JOBSTATUS":
                    var OGeoJobStatusData = db.Company
                    .Include(e => e.Address)
                    .Include(e => e.ContactDetails)
                    .Include(e => e.JobStatus)
                    .Include(e => e.JobStatus.Select(r => r.EmpActingStatus))
                     .Include(e => e.JobStatus.Select(r => r.EmpStatus))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();

                    if (OGeoJobStatusData == null)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OGeoJobStatusData.JobStatus)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.EmpActingStatus != null ? ca.EmpActingStatus.LookupVal.ToString() : null,
                                Fld3 = ca.EmpStatus != null ? ca.EmpStatus.LookupVal.ToString() : null

                            };
                            //write data to generic class
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                        }
                        return OGenericPayrollStatement;
                    }

                    break;
                case "COMPANY":
                    var OGeoCompData = db.Company
                    .Include(e => e.Address)
                    .Include(e => e.ContactDetails)
                    .Where(d => d.Id == CompanyId).SingleOrDefault();

                    if (OGeoCompData == null)
                    {
                        return null;
                    }
                    else
                    {
                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                        {
                            Fld1 = OGeoCompData.Id.ToString(),
                            Fld2 = OGeoCompData.Code,
                            Fld3 = OGeoCompData.Name == null ? "" : OGeoCompData.Name.ToString(),
                            Fld4 = OGeoCompData.Address == null ? "" : OGeoCompData.Address.FullAddress,
                            Fld5 = OGeoCompData.ContactDetails == null ? "" : OGeoCompData.ContactDetails.FullContactDetails,
                            Fld6 = OGeoCompData.CINNo == null ? "" : OGeoCompData.CINNo.ToString(),
                            Fld7 = OGeoCompData.CSTNo == null ? "" : OGeoCompData.CSTNo.ToString(),
                            Fld8 = OGeoCompData.ESICNo == null ? "" : OGeoCompData.ESICNo.ToString(),
                            Fld9 = OGeoCompData.EstablishmentNo == null ? "" : OGeoCompData.EstablishmentNo.ToString(),
                            Fld10 = OGeoCompData.LBTNO == null ? "" : OGeoCompData.LBTNO.ToString(),
                            Fld11 = OGeoCompData.PANNo == null ? "" : OGeoCompData.PANNo.ToString(),
                            Fld12 = OGeoCompData.PTECNO == null ? "" : OGeoCompData.PTECNO.ToString(),
                            Fld13 = OGeoCompData.PTRCNO == null ? "" : OGeoCompData.PTRCNO.ToString(),
                            Fld14 = OGeoCompData.RegistrationNo == null ? "" : OGeoCompData.RegistrationNo.ToString(),
                            Fld15 = OGeoCompData.RegistrationDate == null ? "" : OGeoCompData.RegistrationDate.ToString(),
                            Fld16 = OGeoCompData.ServiceTaxNo == null ? "" : OGeoCompData.ServiceTaxNo.ToString(),
                            Fld17 = OGeoCompData.TANNo == null ? "" : OGeoCompData.TANNo.ToString(),
                            Fld18 = OGeoCompData.VATNo == null ? "" : OGeoCompData.VATNo.ToString(),
                        };
                        OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                        return OGenericPayrollStatement;
                    }

                    break;

                case "LOCATION":
                    var OGeoLocData = db.Company
                    .Include(e => e.Location)
                    .Include(e => e.Location.Select(r => r.Address))
                    .Include(e => e.Location.Select(r => r.ContactDetails))
                    .Include(e => e.Location.Select(r => r.BusinessCategory))
                    .Include(e => e.Location.Select(r => r.Incharge))
                    .Include(e => e.Location.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Location.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Location.Select(r => r.LocationObj))
                    .Include(e => e.Location.Select(r => r.Type))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoLocData.Location == null || OGeoLocData.Location.Count == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OGeoLocData.Location)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.LocationObj == null ? "" : ca.LocationObj.LocCode,
                                Fld3 = ca.LocationObj == null ? "" : ca.LocationObj.LocDesc,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.BusinessCategory == null ? "" : ca.BusinessCategory.LookupVal.ToUpper(),
                                Fld7 = ca.Address == null ? "" : ca.Address.FullAddress,
                                Fld8 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.EmpName == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld11 = ca.Incharge == null ? "" : ca.Incharge.CorContact == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                //Fld12 = ca.WeeklyOffCalendar == null ? "" : ca.WeeklyOffCalendar.Id.ToString(),
                                //Fld13 = ca.HolidayCalendar == null ? "" : ca.HolidayCalendar.Id.ToString(),

                            };
                            //write data to generic class
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "DIVISION":
                    var OGeoDivData = db.Company
                    .Include(e => e.Divisions)
                    .Include(e => e.Divisions.Select(r => r.Address))
                    .Include(e => e.Divisions.Select(r => r.ContactDetails))
                    .Include(e => e.Divisions.Select(r => r.Incharge))
                    .Include(e => e.Divisions.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Divisions.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Divisions.Select(r => r.Incharge.CorAddr))
                    .Include(e => e.Divisions.Select(r => r.Type))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoDivData.Divisions == null || OGeoDivData.Divisions.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OGeoDivData.Divisions)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.Code,
                                Fld3 = ca.Name,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.Address == null ? "" : ca.Address.FullAddress,
                                Fld7 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld8 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.EmpName == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.CorContact == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                Fld11 = ca.Incharge == null ? "" : ca.Incharge.CorAddr == null ? "" : ca.Incharge.CorAddr.FullAddress,

                            };
                            //write data to generic class
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                        }
                        return OGenericPayrollStatement;

                    }
                    break;

                case "DEPARTMENT":
                    var OGeoDeptData = db.Company
                    .Include(e => e.Department)
                    .Include(e => e.Department.Select(r => r.Address))
                    .Include(e => e.Department.Select(r => r.ContactDetails))
                    .Include(e => e.Department.Select(r => r.Incharge))
                    .Include(e => e.Department.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Department.Select(r => r.Type))
                    .Include(e => e.Department.Select(r => r.DepartmentObj))
                    .Include(e => e.Department.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Department.Select(r => r.Incharge.CorAddr))

                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoDeptData.Department == null || OGeoDeptData.Department.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OGeoDeptData.Department)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.DepartmentObj == null ? "" : ca.DepartmentObj.DeptCode,
                                Fld3 = ca.DepartmentObj == null ? "" : ca.DepartmentObj.DeptDesc,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.Address == null ? "" : ca.Address.FullAddress,
                                Fld7 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld8 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.EmpName == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.CorContact == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                Fld11 = ca.Incharge == null ? "" : ca.Incharge.CorAddr == null ? "" : ca.Incharge.CorAddr.FullAddress,

                            };
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);


                            //write data to generic class

                        }
                        return OGenericPayrollStatement;

                    }
                    break;
                case "GROUPR":
                    var OGeoGrpData = db.Company
                    .Include(e => e.Group)
                    .Include(e => e.Group.Select(r => r.ContactDetails))
                    .Include(e => e.Group.Select(r => r.Incharge))
                    .Include(e => e.Group.Select(r => r.Incharge.EmpName))
                    .Include(e => e.Group.Select(r => r.Incharge.CorAddr))
                    .Include(e => e.Group.Select(r => r.Incharge.CorContact))
                    .Include(e => e.Group.Select(r => r.Type))
                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OGeoGrpData.Group == null || OGeoGrpData.Group.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OGeoGrpData.Group)
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.Code,
                                Fld3 = ca.Name,
                                Fld4 = ca.OpeningDate.ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                Fld6 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                Fld7 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                Fld8 = ca.Incharge == null ? "" : ca.Incharge.EmpName == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                Fld9 = ca.Incharge == null ? "" : ca.Incharge.CorContact == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                Fld10 = ca.Incharge == null ? "" : ca.Incharge.CorAddr == null ? "" : ca.Incharge.CorAddr.FullAddress,
                            };
                            //write data to generic class
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                        }

                        return OGenericPayrollStatement;

                    }
                    break;


                case "JOBPOSITION":
                    var OFunJobPositionData = db.Company
                    .Include(e => e.JobPosition)

                    .Where(d => d.Id == CompanyId).SingleOrDefault();
                    if (OFunJobPositionData.Job == null || OFunJobPositionData.Job.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OFunJobPositionData.JobPosition)
                        {
                            GenericField100 OGenericObjStatement = new GenericField100()
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.JobPositionCode == null ? "" : ca.JobPositionCode,
                                Fld3 = ca.JobPositionDesc == null ? "" : ca.JobPositionDesc

                            };
                            OGenericPayrollStatement.Add(OGenericObjStatement);
                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "BONUSACT":
                    var OSalBonusActData = db.CompanyPayroll
                .Include(e => e.BonusAct)
                .Include(e => e.BonusAct.Select(r => r.BonusCalendar))
                .Include(e => e.BonusAct.Select(r => r.BonusWages))
                .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster))
                .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead.Code)))
                        //.Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead.Name)))
            .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalBonusActData.BonusAct == null || OSalBonusActData.BonusAct.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalBonusActData.BonusAct)
                        {
                            foreach (var ca1 in ca.BonusWages.RateMaster)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.BonusName,
                                    Fld3 = ca.BonusCalendar == null ? "" : ca.BonusCalendar.FullDetails,
                                    Fld4 = ca.MaximumBonus.ToString(),
                                    Fld5 = ca.MinimumBonusAmount.ToString(),
                                    Fld6 = ca.MaxPercentage.ToString(),
                                    Fld7 = ca.MinPercentage.ToString(),
                                    Fld8 = ca.MinimumWorkingDays.ToString(),
                                    Fld9 = ca.QualiAmount.ToString(),
                                    Fld10 = ca.BonusWages.Id.ToString(),
                                    Fld11 = ca.BonusWages.CeilingMax.ToString(),
                                    Fld12 = ca.BonusWages.CeilingMin.ToString(),
                                    Fld13 = ca.BonusWages.Percentage.ToString(),

                                    Fld14 = ca1.SalHead.Id.ToString(),
                                    Fld15 = ca1.SalHead.Code,
                                    Fld16 = ca1.SalHead.Name,
                                    Fld17 = ca1.Amount.ToString(),
                                    Fld18 = ca1.Percentage.ToString(),


                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            };

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "CPIRULE":
                    var OSalCPIRuleData = db.CompanyPayroll
                        .Include(e => e.CPIRule)

                        .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails))
                        .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages)))
                        .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster)))
                        .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster.Select(s => s.SalHead))))
                        //.Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster.Select(s => s.SalHead.Id))))
                        //.Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster.Select(s => s.SalHead.Code))))
                        //.Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster.Select(s => s.SalHead.Name))))
                        .Include(e => e.CPIRule.Select(r => r.CPIUnitCalc))
                        .Include(e => e.CPIRule.Select(r => r.RoundingMethod))//modify roundingmethod in other code

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalCPIRuleData.CPIRule == null || (OSalCPIRuleData.CPIRule.Count() == 0))
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OSalCPIRuleData.CPIRule)
                        {
                            foreach (var ca1 in ca.CPIRuleDetails)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name,
                                    Fld3 = ca1.MaxAmountIBase == null ? "" : ca1.MaxAmountIBase.ToString(),
                                    Fld4 = ca1.MinAmountIBase == null ? "" : ca1.MinAmountIBase.ToString(),
                                    Fld5 = ca.IBaseDigit == null ? "" : ca.IBaseDigit.ToString(),
                                    Fld6 = ca.RoundingMethod == null ? "" : ca.RoundingMethod.LookupVal.ToUpper(),
                                    Fld7 = ca1.Id.ToString(),
                                    Fld8 = ca1.SalFrom == null ? "" : ca1.SalFrom.ToString(),
                                    Fld9 = ca1.SalTo == null ? "" : ca1.SalTo.ToString(),
                                    Fld10 = ca1.IncrPercent == null ? "" : ca1.IncrPercent.ToString(),
                                    Fld11 = ca1.AdditionalIncrAmount == null ? "" : ca1.AdditionalIncrAmount.ToString(),
                                    Fld12 = ca1.CPIWages.FullDetails == null ? "" : ca1.CPIWages.FullDetails,
                                    Fld13 = ca.CPIUnitCalc == null ? "" : ca.CPIUnitCalc.Select(e => e.BaseIndex).FirstOrDefault().ToString(),

                                    Fld14 = ca.CPIUnitCalc == null ? "" : ca.CPIUnitCalc.Select(e => e.Unit).FirstOrDefault().ToString(),
                                    Fld15 = ca.CPIUnitCalc == null ? "" : ca.CPIUnitCalc.Select(e => e.IndexMaxCeiling).FirstOrDefault().ToString(),
                                    Fld16 = ca.CPIUnitCalc == null ? "" : ca.CPIUnitCalc.Select(e => e.Id).FirstOrDefault().ToString(),


                                };

                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "ESICMASTER":
                    var OSalESICMasterData = db.CompanyPayroll
                        .Include(e => e.ESICMaster)
                        .Include(e => e.ESICMaster.Select(r => r.ESICStatutoryEffectiveMonths))
                        .Include(e => e.ESICMaster.Select(r => r.ESICStatutoryEffectiveMonths.Select(t => t.EffectiveMonth)))
                        .Include(e => e.ESICMaster.Select(r => r.ESICStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange)))
                        //.Include(e => e.ESICMaster.Select(r => r.LocationObj.Select(w => w.LocCode)))
                        //.Include(e => e.ESICMaster.Select(r => r.LocationObj.Select(w => w.LocDesc)))
                        .Include(e => e.ESICMaster.Select(r => r.WageMasterPay))
                        .Include(e => e.ESICMaster.Select(r => r.WageMasterPay.RateMaster))
                        .Include(e => e.ESICMaster.Select(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead)))
                        //.Include(e => e.ESICMaster.Select(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead.Code)))
                        //.Include(e => e.ESICMaster.Select(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead.Name)))
                        .Include(e => e.ESICMaster.Select(r => r.WageMasterQualify))
                        .Include(e => e.ESICMaster.Select(r => r.WageMasterQualify.RateMaster))
                        .Include(e => e.ESICMaster.Select(r => r.WageMasterQualify.RateMaster.Select(w => w.SalHead)))
                        //.Include(e => e.ESICMaster.Select(r => r.WageMasterQualify.RateMaster.Select(w => w.SalHead.Code)))
                        //.Include(e => e.ESICMaster.Select(r => r.WageMasterQualify.RateMaster.Select(w => w.SalHead.Name)))
                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalESICMasterData.ESICMaster == null || OSalESICMasterData.ESICMaster.Count() == 0)
                    {
                        return null;
                    }

                    else
                    {
                        foreach (var ca in OSalESICMasterData.ESICMaster)
                        {
                            foreach (var ca1 in ca.ESICStatutoryEffectiveMonths)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                 //   Fld2 = ca.LocationObj.Select(e => e.LocDesc).ToString(),
                                    Fld3 = ca.EffectiveDate.ToString(),
                                    Fld4 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                    Fld5 = ca.WageMasterPay == null ? "" : ca.WageMasterPay.FullDetails,
                                    Fld6 = ca.WageMasterQualify == null ? "" : ca.WageMasterQualify.FullDetails,
                                    Fld7 = ca1.Id.ToString(),
                                    Fld8 = ca1.EffectiveMonth == null ? "" : ca1.EffectiveMonth.LookupVal.ToUpper(),
                                    Fld9 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.CompSharePercentage).ToString(),
                                    Fld10 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.CompShareAmount).ToString(),
                                    Fld11 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.EmpSharePercentage).ToString(),
                                    Fld12 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.EmpShareAmount).ToString(),
                                    Fld13 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.RangeFrom).ToString(),
                                    Fld14 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.RangeTo).ToString(),

                                };

                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "GRATUITYACT":
                    var OSalGratuityActData = db.CompanyPayroll

                        .Include(e => e.GratuityAct)
                        .Include(e => e.GratuityAct.Select(r => r.GratuityWages))
                        .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster))
                        .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster.Select(t => t.SalHead)))
                        .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster.Select(t => t.Wages)))
                        .Include(e => e.GratuityAct.Select(r => r.LvHead))


                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalGratuityActData.GratuityAct == null || OSalGratuityActData.GratuityAct.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalGratuityActData.GratuityAct)
                        {
                            foreach (var ca1 in ca.GratuityWages.RateMaster)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.GratuityActName == null ? "" : ca.GratuityActName,
                                    Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.ToString(),
                                    Fld4 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                    Fld5 = ca.GratuityWages == null ? "" : ca.GratuityWages.FullDetails,
                                    Fld6 = ca.IsDateOfConfirm == null ? "" : ca.IsDateOfConfirm.ToString(),
                                    Fld7 = ca.IsDateOfJoin == null ? "" : ca.IsDateOfJoin.ToString(),
                                    Fld8 = ca.IsLVInclude == null ? "" : ca.IsLVInclude.ToString(),
                                    Fld9 = ca.IsLWPInclude == null ? "" : ca.IsLWPInclude.ToString(),
                                    Fld10 = ca.ITExemptionAmount == null ? "" : ca.ITExemptionAmount.ToString(),
                                    Fld11 = ca.MaxGratuityAmount == null ? "" : ca.MaxGratuityAmount.ToString(),
                                    Fld12 = ca.MonthDays == null ? "" : ca.MonthDays.ToString(),
                                    Fld13 = ca.PayableDays == null ? "" : ca.PayableDays.ToString(),
                                    Fld14 = ca.ServiceFrom == null ? "" : ca.ServiceFrom.ToString(),
                                    Fld15 = ca.ServiceTo == null ? "" : ca.ServiceTo.ToString(),
                                    Fld16 = ca.IsDateOfConfirm == null ? "" : ca.IsDateOfConfirm.ToString(),

                                    Fld17 = ca1.Id.ToString(),
                                    Fld18 = ca1.Amount == null ? "" : ca1.Amount.ToString(),
                                    Fld19 = ca1.Percentage == null ? "" : ca1.Percentage.ToString(),
                                    Fld20 = ca1.FullDetails == null ? "" : ca1.FullDetails,
                                    Fld21 = ca1.Code == null ? "" : ca1.Code,
                                    Fld22 = ca1.SalHead.Id.ToString(),
                                    Fld23 = ca1.SalHead.Code == null ? "" : ca1.SalHead.Code,
                                    Fld24 = ca1.SalHead.Name == null ? "" : ca1.SalHead.Name,


                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "ITSECTION":
                    var OITSectionData = db.CompanyPayroll
                        .Include(e => e.IncomeTax)
                        .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionList)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionListType)))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OITSectionData.IncomeTax == null || OITSectionData.IncomeTax.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OITSectionData.IncomeTax)
                        {
                            foreach (var ca1 in ca.ITSection)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name == null ? "" : ca.FyCalendar.Name.LookupVal,
                                    Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate == null ? "" : ca.FyCalendar.FromDate.ToString(),
                                    Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate == null ? "" : ca.FyCalendar.ToDate.ToString(),
                                    Fld5 = ca1.ITSectionList == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                    Fld6 = ca1.ITSectionListType == null ? "" : ca1.ITSectionListType.LookupVal.ToUpper(),
                                    Fld7 = ca1.ExemptionLimit == null ? "" : ca1.ExemptionLimit.ToString(),
                                    Fld8 = ca1.FullDetails == null ? "" : ca1.FullDetails,
                                    //Fld9 = ca1.Id.ToString(),


                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "ITTDS":
                    var OITTdsData = db.CompanyPayroll
                        .Include(e => e.IncomeTax)
                        .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                        .Include(e => e.IncomeTax.Select(r => r.ITTDS))
                        .Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(t => t.Category)))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OITTdsData.IncomeTax == null || OITTdsData.IncomeTax.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OITTdsData.IncomeTax)
                        {
                            foreach (var ca1 in ca.ITTDS)
                            {

                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name == null ? "" : ca.FyCalendar.Name.LookupVal,
                                    Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate == null ? "" : ca.FyCalendar.FromDate.ToString(),
                                    Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate == null ? "" : ca.FyCalendar.ToDate.ToString(),
                                    Fld5 = ca1.Category == null ? "" : ca1.Category.LookupVal == null ? "" : ca1.Category.LookupVal.ToUpper(),
                                    Fld6 = ca1.IncomeRangeFrom == null ? "" : ca1.IncomeRangeFrom.ToString(),
                                    Fld7 = ca1.IncomeRangeTo == null ? "" : ca1.IncomeRangeTo.ToString(),
                                    Fld8 = ca1.Percentage == null ? "" : ca1.Percentage.ToString(),
                                    Fld9 = ca1.Id.ToString(),
                                    Fld10 = ca1.Amount == null ? "" : ca1.Amount.ToString(),
                                    Fld11 = ca1.EduCessAmount == null ? "" : ca1.EduCessAmount.ToString(),
                                    Fld12 = ca1.EduCessPercent == null ? "" : ca1.EduCessPercent.ToString(),
                                    Fld13 = ca1.SurchargeAmount == null ? "" : ca1.SurchargeAmount.ToString(),
                                    Fld14 = ca1.SurchargePercent == null ? "" : ca1.SurchargePercent.ToString(),
                                    Fld15 = ca1.FullDetails == null ? "" : ca1.FullDetails,


                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "ITSECTION10B":
                    var OITSection10BData = db.CompanyPayroll
                        .Include(e => e.IncomeTax)
                        .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionList)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionListType)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead))))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead.Select(x => x.SalHead)))))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead.Select(x => x.Frequency)))))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();


                    if (OITSection10BData.IncomeTax == null || OITSection10BData.IncomeTax.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OITSection10BData.IncomeTax)
                        {
                            foreach (var ca1 in ca.ITSection)
                            {
                                if (ca1.ITSection10 != null && ca1.ITSection10.Count() > 0)
                                {
                                    foreach (var ca2 in ca1.ITSection10)
                                    {
                                        foreach (var ca3 in ca2.Itsection10salhead)
                                        {
                                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Id.ToString(),
                                                //Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name,
                                                Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate == null ? "" : ca.FyCalendar.FromDate.ToString(),
                                                Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate == null ? "" : ca.FyCalendar.ToDate.ToString(),
                                                Fld5 = ca1.ITSectionList == null ? "" : ca1.ITSectionList.LookupVal == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                                Fld6 = ca1.ITSectionListType == null ? "" : ca1.ITSectionListType.LookupVal == null ? "" : ca1.ITSectionListType.LookupVal.ToUpper(),
                                                Fld7 = ca1.Id.ToString(),
                                                Fld8 = ca2.ExemptionCode == null ? "" : ca2.ExemptionCode,
                                                Fld9 = ca2.Id.ToString(),

                                                Fld10 = ca2.MaxAmount == null ? "" : ca2.MaxAmount.ToString(),
                                                Fld11 = ca3.Id.ToString(),
                                                Fld12 = ca3.SalHead.Id == null ? "" : ca3.SalHead.Id.ToString(),
                                                Fld13 = ca3.SalHead == null ? "" : ca3.SalHead.Code == null ? "" : ca3.SalHead.Code,
                                                Fld14 = ca3.SalHead == null ? "" : ca3.SalHead.Name == null ? "" : ca3.SalHead.Name,
                                                Fld15 = ca3.Amount == null ? "" : ca3.Amount.ToString(),
                                                Fld16 = ca3.AutoPick == null ? "" : ca3.AutoPick.ToString(),
                                                Fld17 = ca3.Frequency == null ? "" : ca3.Frequency.LookupVal == null ? "" : ca3.Frequency.LookupVal.ToUpper(),
                                                Fld18 = ca3.Months == null ? "" : ca3.Months.ToString(),
                                                Fld19 = ca3.Percent == null ? "" : ca3.Percent.ToString(),
                                                Fld20 = ca3.FullDetails == null ? "" : ca3.FullDetails,


                                            };

                                            //write data to generic class
                                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                        }
                                    }
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "ITINVESTMENT":
                    var OITInvestmentData = db.CompanyPayroll
                        .Include(e => e.IncomeTax)
                        .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionList)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionListType)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITInvestments)))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITInvestments.Select(y => y.ITSubInvestment))))
                        .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITInvestments.Select(y => y.SalaryHead))))


                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();


                    if (OITInvestmentData.IncomeTax == null || OITInvestmentData.IncomeTax.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OITInvestmentData.IncomeTax)
                        {
                            foreach (var ca1 in ca.ITSection)
                            {
                                if (ca1.ITInvestments != null && ca1.ITInvestments.Count() > 0)
                                {
                                    foreach (var ca2 in ca1.ITInvestments)
                                    {
                                        if (ca2.ITSubInvestment == null || ca2.ITSubInvestment.Count() == 0)
                                        {

                                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Id.ToString(),
                                                //Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name,
                                                Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate == null ? "" : ca.FyCalendar.FromDate.ToString(),
                                                Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate == null ? "" : ca.FyCalendar.ToDate.ToString(),
                                                Fld5 = ca1.ITSectionList == null ? "" : ca1.ITSectionList.LookupVal == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                                Fld6 = ca1.ITSectionListType == null ? "" : ca1.ITSectionListType.LookupVal == null ? "" : ca1.ITSectionListType.LookupVal.ToUpper(),
                                                Fld7 = ca1.Id.ToString(),
                                                Fld8 = ca2.FullDetails == null ? "" : ca2.FullDetails,
                                                Fld9 = ca2.Id.ToString(),

                                                Fld10 = ca2.MaxAmount == null ? "" : ca2.MaxAmount.ToString(),
                                                Fld11 = ca2.MaxPercentage == null ? "" : ca2.MaxPercentage.ToString(),
                                                Fld12 = ca2.IsSalaryHead == null ? "" : ca2.IsSalaryHead.ToString(),
                                                Fld13 = ca2.SalaryHead.Id.ToString(),
                                                Fld14 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Code == null ? "" : ca2.SalaryHead.Code,
                                                Fld15 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Name == null ? "" : ca2.SalaryHead.Name,
                                                Fld16 = ca2.ITInvestmentName == null ? "" : ca2.ITInvestmentName,
                                                Fld17 = "",
                                                Fld18 = "",
                                                Fld19 = ""

                                            };

                                            //write data to generic class
                                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                        }
                                        else
                                        {
                                            foreach (var ca3 in ca2.ITSubInvestment)
                                            {
                                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Id.ToString(),
                                                    //Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name,
                                                    Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate == null ? "" : ca.FyCalendar.FromDate.ToString(),
                                                    Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate == null ? "" : ca.FyCalendar.ToDate.ToString(),
                                                    Fld5 = ca1.ITSectionList == null ? "" : ca1.ITSectionList.LookupVal == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                                    Fld6 = ca1.ITSectionListType == null ? "" : ca1.ITSectionList.LookupVal == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                                    Fld7 = ca1.Id.ToString(),
                                                    Fld8 = ca2.FullDetails == null ? "" : ca2.FullDetails,
                                                    Fld9 = ca2.Id.ToString(),

                                                    Fld10 = ca2.MaxAmount == null ? "" : ca2.MaxAmount.ToString(),
                                                    Fld11 = ca2.MaxPercentage == null ? "" : ca2.MaxPercentage.ToString(),
                                                    Fld12 = ca2.IsSalaryHead == null ? "" : ca2.IsSalaryHead.ToString(),
                                                    Fld13 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Id == null ? "" : ca2.SalaryHead.Id.ToString(),
                                                    Fld14 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Code == null ? "" : ca2.SalaryHead.Code,
                                                    Fld15 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Name == null ? "" : ca2.SalaryHead.Name,
                                                    Fld16 = ca2.ITInvestmentName == null ? "" : ca2.ITInvestmentName,
                                                    Fld17 = ca3.Id.ToString(),
                                                    Fld18 = ca3.SubInvestmentName == null ? "" : ca3.SubInvestmentName,
                                                    Fld19 = ca3.FullDetails == null ? "" : ca3.FullDetails


                                                };

                                                //write data to generic class
                                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "JVPARAMETER":
                    var OSalJVParameterData = db.CompanyPayroll
                        .Include(e => e.JVParameter)

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalJVParameterData == null)
                        return null;
                    else
                    {
                        GenericField100 OGenericSalMasterObjStatement = new GenericField100();
                        //write data to generic class
                        OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                        return OGenericPayrollStatement;
                    }
                    break;
                case "LOANADVANCEHEAD":
                    var OSalLoanAdvanceHeadData = db.CompanyPayroll
                        .Include(e => e.LoanAdvanceHead)
                        .Include(e => e.LoanAdvanceHead.Select(r => r.ITLoan))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.InterestType)))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages)))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages.RateMaster)))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages.RateMaster.Select(w => w.SalHead))))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.SalaryHead))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.ITSection.Select(w => w.ITSectionList)))
                        .Include(e => e.LoanAdvanceHead.Select(r => r.ITSection.Select(w => w.ITSectionListType)))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalLoanAdvanceHeadData.LoanAdvanceHead == null && OSalLoanAdvanceHeadData.LoanAdvanceHead.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalLoanAdvanceHeadData.LoanAdvanceHead)
                        {
                            foreach (var ca1 in ca.LoanAdvancePolicy)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld3 = ca.Name,
                                    Fld2 = ca.Code,
                                    Fld4 = ca.SalaryHead.Code,
                                    Fld5 = ca.SalaryHead.Name,
                                    Fld6 = ca.SalaryHead.InPayslip.ToString(),
                                    Fld7 = ca1.Id.ToString(),
                                    Fld8 = ca1.EffectiveDate.ToString(),
                                    Fld9 = ca1.EndDate == null ? "" : ca1.EndDate.ToString(),
                                    Fld10 = ca1.YrsOfServ == null ? "" : ca1.YrsOfServ.ToString(),
                                    Fld11 = ca1.IntAppl == null ? "" : ca1.IntAppl.ToString(),
                                    Fld12 = ca1.IntAppl == null ? "" : ca1.IntAppl.ToString(),
                                    Fld13 = ca1.InterestType == null ? "" : ca1.InterestType.LookupVal.ToUpper(),
                                    Fld14 = ca1.IntRate == null ? "" : ca1.IntRate.ToString(),
                                    Fld15 = ca1.IsFine == null ? "" : ca1.IsFine.ToString(),
                                    Fld16 = ca1.FineAmt == null ? "" : ca1.FineAmt.ToString(),
                                    Fld17 = ca1.IsPerkOnInt == null ? "" : ca1.IsPerkOnInt.ToString(),
                                    Fld18 = ca1.GovtIntRate == null ? "" : ca1.GovtIntRate.ToString(),
                                    Fld19 = ca1.IsFixAmount == null ? "" : ca1.IsFixAmount.ToString(),
                                    Fld20 = ca1.MaxLoanAmount == null ? "" : ca1.MaxLoanAmount.ToString(),
                                    Fld21 = ca1.IsLoanLimit == null ? "" : ca1.IsLoanLimit.ToString(),
                                    Fld22 = ca1.IsOnWages == null ? "" : ca1.IsOnWages.ToString(),
                                    Fld23 = ca1.LoanSanctionWages == null ? "" : ca1.LoanSanctionWages.FullDetails,
                                    Fld24 = ca1.NoOfTimes == null ? "" : ca1.NoOfTimes.ToString(),
                                    Fld25 = ca.ITLoan == null ? "" : ca.ITLoan.FullDetails,
                                    Fld26 = ca.ITSection == null ? "" : ca.ITSection.Select(e => e.ITSectionList.LookupVal.ToUpper()).FirstOrDefault(),
                                    Fld27 = ca.ITSection == null ? "" : ca.ITSection.Select(e => e.ITSectionListType.LookupVal.ToUpper()).FirstOrDefault(),
                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "LWFMASTER":
                    var OSalLWFMasterData = db.CompanyPayroll
                        .Include(e => e.LWFMaster)
                        .Include(e => e.LWFMaster.Select(r => r.LWFStates))
                        .Include(e => e.LWFMaster.Select(r => r.LWFStatutoryEffectiveMonths))
                        .Include(e => e.LWFMaster.Select(r => r.WagesMaster))
                        .Include(e => e.LWFMaster.Select(r => r.LWFStatutoryEffectiveMonths.Select(w => w.StatutoryWageRange)))
                         .Include(e => e.LWFMaster.Select(r => r.LWFStatutoryEffectiveMonths.Select(w => w.EffectiveMonth)))
                        .Include(e => e.LWFMaster.Select(r => r.WagesMaster.RateMaster))
                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalLWFMasterData.LWFMaster == null || OSalLWFMasterData.LWFMaster.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalLWFMasterData.LWFMaster)
                        {
                            var mWageRate = "";
                            if (ca.WagesMaster != null && ca.WagesMaster.RateMaster != null && ca.WagesMaster.RateMaster.Count() > 0)
                            {
                                foreach (var ca5 in ca.WagesMaster.RateMaster)
                                {
                                    mWageRate = ca5.SalHead.Name + " x " + ca5.Percentage + " % " + Environment.NewLine;
                                }
                            }
                            foreach (var ca1 in ca.LWFStatutoryEffectiveMonths)
                            {
                                foreach (var ca2 in ca1.StatutoryWageRange)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.EffectiveDate.ToString(),
                                        Fld3 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                        Fld4 = ca.LWFStates == null ? "" : ca.LWFStates.Name,
                                        Fld5 = ca.WagesMaster == null ? "" : ca.WagesMaster.WagesName,
                                        Fld6 = mWageRate,
                                        Fld7 = ca.WagesMaster.CeilingMax == null ? "" : ca.WagesMaster.CeilingMax.ToString(),
                                        Fld8 = ca.WagesMaster.CeilingMin == null ? "" : ca.WagesMaster.CeilingMin.ToString(),
                                        Fld9 = ca.WagesMaster.Percentage == null ? "" : ca.WagesMaster.Percentage.ToString(),

                                        Fld10 = ca1.Id.ToString(),
                                        Fld11 = ca1.EffectiveMonth == null ? "" : ca1.EffectiveMonth.LookupVal.ToUpper(),
                                        Fld12 = ca2.Id.ToString(),
                                        Fld13 = ca2.RangeFrom == null ? "" : ca2.RangeFrom.ToString(),
                                        Fld14 = ca2.RangeTo == null ? "" : ca2.RangeTo.ToString(),
                                        Fld15 = ca2.CompShareAmount == null ? "" : ca2.CompShareAmount.ToString(),
                                        Fld16 = ca2.CompSharePercentage == null ? "" : ca2.CompSharePercentage.ToString(),
                                        Fld17 = ca2.EmpShareAmount == null ? "" : ca2.EmpShareAmount.ToString(),
                                        Fld18 = ca2.EmpSharePercentage == null ? "" : ca2.EmpSharePercentage.ToString(),


                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "NEGSALACT":
                    var OSalNegSalActData = db.CompanyPayroll
                        .Include(e => e.NegSalAct)

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalNegSalActData.NegSalAct == null || OSalNegSalActData.NegSalAct.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalNegSalActData.NegSalAct)
                        {
                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.NegSalActname == null ? "" : ca.NegSalActname,
                                Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.ToString(),
                                Fld4 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                Fld5 = ca.MinAmount == null ? "" : ca.MinAmount.ToString(),
                                Fld6 = ca.SalPercentage == null ? "" : ca.SalPercentage.ToString(),

                            };
                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "PAYSCALEASSIGNMENT":
                    var OSalPayScaleAssignmentData = db.CompanyPayroll
                        .Include(e => e.PayScaleAssignment)
                        .Include(e => e.PayScaleAssignment.Select(r => r.PayScaleAgreement))
                        .Include(e => e.PayScaleAssignment.Select(r => r.SalaryHead))
                        .Include(e => e.PayScaleAssignment.Select(r => r.SalHeadFormula))
                        .Include(e => e.PayScaleAssignment.Select(r => r.SalHeadFormula.Select(t => t.SalWages)))


                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalPayScaleAssignmentData.PayScaleAssignment == null || OSalPayScaleAssignmentData.PayScaleAssignment.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalPayScaleAssignmentData.PayScaleAssignment)
                        {
                            if (ca.SalHeadFormula == null || ca.SalHeadFormula.Count() == 0)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.PayScaleAgreement == null ? "" : ca.PayScaleAgreement.FullDetails,
                                    Fld3 = ca.SalaryHead == null ? "" : ca.SalaryHead.Id.ToString(),
                                    Fld4 = ca.SalaryHead == null ? "" : ca.SalaryHead.Code,
                                    Fld5 = ca.SalaryHead == null ? "" : ca.SalaryHead.Name,
                                    Fld6 = "",
                                    Fld7 = "",
                                    Fld8 = "",
                                    Fld9 = "",
                                    Fld10 = "",
                                    Fld11 = "",
                                    Fld12 = "",
                                    Fld13 = "",
                                    Fld14 = "",
                                    Fld15 = "",
                                    Fld16 = "",
                                    Fld17 = "",
                                    Fld18 = "",
                                    Fld19 = "",

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            else
                            {
                                foreach (var ca1 in ca.SalHeadFormula)
                                {

                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.PayScaleAgreement == null ? "" : ca.PayScaleAgreement.FullDetails,
                                        Fld3 = ca.SalaryHead == null ? "" : ca.SalaryHead.Id.ToString(),
                                        Fld4 = ca.SalaryHead == null ? "" : ca.SalaryHead.Code,
                                        Fld5 = ca.SalaryHead == null ? "" : ca.SalaryHead.Name,
                                        Fld6 = ca1.Id == null ? "" : ca1.Id.ToString(),
                                        Fld7 = ca1.Name == null ? "" : ca1.Name.ToString(),
                                        Fld8 = ca1.CeilingMin == null ? "" : ca1.CeilingMin.ToString(),
                                        Fld9 = ca1.CeilingMax == null ? "" : ca1.CeilingMax.ToString(),
                                        Fld10 = ca1.SalWages == null ? "" : ca1.SalWages.FullDetails.ToString(),
                                        Fld11 = ca1.AmountDependRule == null ? "" : ca1.AmountDependRule.SalAmount.ToString(),
                                        Fld12 = ca1.PercentDependRule == null ? "" : ca1.PercentDependRule.SalPercent.ToString(),
                                        Fld13 = ca1.SlabDependRule == null ? "" : ca1.SlabDependRule.WageRange.Select(e => e.FullDetails).FirstOrDefault().ToString(),
                                        Fld14 = ca1.ServiceDependRule == null ? "" : ca1.ServiceDependRule.ServiceRange.Select(e => e.FullDetails).FirstOrDefault().ToString(),
                                        Fld15 = ca1.VDADependRule == null ? "" : ca1.VDADependRule.CPIRule.FullDetails.ToString(),
                                        Fld16 = ca1.BASICDependRule == null ? "" : ca1.BASICDependRule.BasicScale.ScaleName.ToString(),
                                        Fld17 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.FullDetails.ToString(),
                                        Fld18 = ca1.PayStruct == null ? "" : ca1.PayStruct.FullDetails.ToString(),
                                        Fld19 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.FullDetails.ToString(),


                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "PFMASTER":
                    var OSalPFMasterData = db.CompanyPayroll
                        .Include(e => e.PFMaster)
                        .Include(e => e.PFMaster.Select(r => r.EDLISTrustType))
                        .Include(e => e.PFMaster.Select(r => r.PensionTrustType))

                        .Include(e => e.PFMaster.Select(r => r.PFTrustType))

                        .Include(e => e.PFMaster.Select(r => r.EPFWages))
                        .Include(e => e.PFMaster.Select(r => r.EPFWages.RateMaster))
                        .Include(e => e.PFMaster.Select(r => r.EPFWages.RateMaster.Select(t => t.SalHead)))
                        .Include(e => e.PFMaster.Select(r => r.EPSWages))
                        .Include(e => e.PFMaster.Select(r => r.EPSWages.RateMaster))
                        .Include(e => e.PFMaster.Select(r => r.EPSWages.RateMaster.Select(t => t.SalHead)))
                        .Include(e => e.PFMaster.Select(r => r.PFAdminWages))
                        .Include(e => e.PFMaster.Select(r => r.PFAdminWages.RateMaster))
                        .Include(e => e.PFMaster.Select(r => r.PFAdminWages.RateMaster.Select(t => t.SalHead)))
                        .Include(e => e.PFMaster.Select(r => r.PFEDLIWages))
                        .Include(e => e.PFMaster.Select(r => r.PFEDLIWages.RateMaster))
                        .Include(e => e.PFMaster.Select(r => r.PFEDLIWages.RateMaster.Select(t => t.SalHead)))
                        .Include(e => e.PFMaster.Select(r => r.PFInspWages))
                        .Include(e => e.PFMaster.Select(r => r.PFInspWages.RateMaster))
                        .Include(e => e.PFMaster.Select(r => r.PFInspWages.RateMaster.Select(t => t.SalHead)))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalPFMasterData.PFMaster == null || OSalPFMasterData.PFMaster.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalPFMasterData.PFMaster)
                        {
                            var mWageRateEPF = "";
                            if (ca.EPFWages != null && ca.EPFWages.RateMaster != null && ca.EPFWages.RateMaster.Count() > 0)
                            {
                                foreach (var ca5 in ca.EPFWages.RateMaster)
                                {
                                    mWageRateEPF = ca5.SalHead.Name + " x " + ca5.Percentage + " % " + Environment.NewLine;
                                }
                            }
                            var mWageRateEPS = "";
                            if (ca.EPSWages != null && ca.EPSWages.RateMaster != null && ca.EPSWages.RateMaster.Count() > 0)
                            {
                                foreach (var ca5 in ca.EPSWages.RateMaster)
                                {
                                    mWageRateEPS = ca5.SalHead.Name + " x " + ca5.Percentage + " % " + Environment.NewLine;
                                }
                            }
                            var mWageRateEDLI = "";
                            if (ca.PFEDLIWages != null && ca.PFEDLIWages.RateMaster != null && ca.PFEDLIWages.RateMaster.Count() > 0)
                            {
                                foreach (var ca5 in ca.PFEDLIWages.RateMaster)
                                {
                                    mWageRateEDLI = ca5.SalHead.Name + " x " + ca5.Percentage + " % " + Environment.NewLine;
                                }
                            }
                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.EstablishmentID == null ? "" : ca.EstablishmentID.ToString(),
                                Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.ToString(),
                                Fld4 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                Fld5 = ca.PFTrustType == null ? "" : ca.PFTrustType.LookupVal.ToUpper().ToString(),
                                Fld6 = ca.PensionTrustType == null ? "" : ca.PensionTrustType.ToString(),
                                Fld7 = ca.EDLISTrustType == null ? "" : ca.EDLISTrustType.ToString(),
                                Fld8 = ca.CompPFNo == null ? "" : ca.CompPFNo.ToString(),
                                Fld9 = ca.CPFPerc == null ? "" : ca.CPFPerc.ToString(),
                                Fld10 = ca.CompPFCeiling == null ? "" : ca.CompPFCeiling.ToString(),
                                Fld11 = ca.RegDate == null ? "" : ca.RegDate.ToString(),
                                //Fld12=ca.EPFWages==null?"":ca.EPFWages.FullDetails.ToString(),
                                Fld13 = ca.EmpPFPerc == null ? "" : ca.EmpPFPerc.ToString(),
                                Fld14 = ca.EPFCeiling == null ? "" : ca.EPFCeiling.ToString(),
                                Fld15 = ca.EPFAdminCharges == null ? "" : ca.EPFAdminCharges.ToString(),
                                Fld16 = ca.EPFAdminMin == null ? "" : ca.EPFAdminMin.ToString(),
                                Fld17 = ca.EPFInspCharges == null ? "" : ca.EPFInspCharges.ToString(),
                                Fld18 = ca.EPFInspMin == null ? "" : ca.EPFInspMin.ToString(),
                                Fld19 = ca.PensionAge == null ? "" : ca.PensionAge.ToString(),
                                //Fld20=ca.EPSWages==null?"":ca.EPSWages.FullDetails.ToString(),
                                Fld21 = ca.EPSPerc == null ? "" : ca.EPSPerc.ToString(),
                                Fld22 = ca.EPSCeiling == null ? "" : ca.EPSCeiling.ToString(),
                                //Fld23 = ca.PFEDLIWages == null ? "" : ca.PFEDLIWages.FullDetails.ToString(),
                                Fld24 = ca.EDLICharges == null ? "" : ca.EDLICharges.ToString(),
                                Fld25 = ca.EDLIAdmin == null ? "" : ca.EDLIAdmin.ToString(),
                                Fld26 = ca.EDLIAdminMin == null ? "" : ca.EDLIAdminMin.ToString(),
                                Fld27 = ca.EDLISInsp == null ? "" : ca.EDLISInsp.ToString(),
                                Fld28 = ca.EDLIInspMin == null ? "" : ca.EDLIInspMin.ToString(),
                                Fld29 = ca.EPFWages == null ? "" : ca.EPFWages.WagesName.ToString(),
                                Fld30 = ca.EPFWages == null ? "" : ca.EPFWages.CeilingMax.ToString(),
                                Fld31 = ca.EPFWages == null ? "" : ca.EPFWages.CeilingMin.ToString(),
                                Fld32 = ca.EPFWages == null ? "" : ca.EPFWages.Percentage.ToString(),
                                Fld33 = mWageRateEPF.ToString(),
                                Fld34 = ca.EPSWages == null ? "" : ca.EPFWages.WagesName.ToString(),
                                Fld35 = ca.EPSWages == null ? "" : ca.EPFWages.CeilingMax.ToString(),
                                Fld36 = ca.EPSWages == null ? "" : ca.EPFWages.CeilingMin.ToString(),
                                Fld37 = ca.EPSWages == null ? "" : ca.EPFWages.Percentage.ToString(),
                                Fld38 = mWageRateEPS.ToString(),
                                Fld39 = ca.PFEDLIWages == null ? "" : ca.PFEDLIWages.WagesName.ToString(),
                                Fld40 = ca.PFEDLIWages == null ? "" : ca.PFEDLIWages.CeilingMax.ToString(),
                                Fld41 = ca.PFEDLIWages == null ? "" : ca.PFEDLIWages.CeilingMin.ToString(),
                                Fld42 = ca.PFEDLIWages == null ? "" : ca.PFEDLIWages.Percentage.ToString(),
                                Fld43 = mWageRateEDLI.ToString(),
                            };
                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "PTAXMASTER":
                    var OSalPTaxMasterData = db.CompanyPayroll
                         .Include(e => e.PTaxMaster)
                         .Include(e => e.PTaxMaster.Select(r => r.Frequency))
                         .Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths))
                         .Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange)))
                         .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster))
                         .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster))
                         .Include(e => e.PTaxMaster.Select(r => r.States))
                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalPTaxMasterData.PTaxMaster == null || OSalPTaxMasterData.PTaxMaster.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalPTaxMasterData.PTaxMaster)
                        {
                            var mWageRatePT = "";
                            if (ca.PTWagesMaster != null && ca.PTWagesMaster.RateMaster != null && ca.PTWagesMaster.RateMaster.Count() > 0)
                            {
                                foreach (var ca5 in ca.PTWagesMaster.RateMaster)
                                {
                                    mWageRatePT = ca5.SalHead.Name + " x " + ca5.Percentage + " % " + Environment.NewLine;
                                }
                            }
                            foreach (var ca1 in ca.PTStatutoryEffectiveMonths)
                            {
                                foreach (var ca2 in ca1.StatutoryWageRange)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.ToString(),
                                        Fld3 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                        Fld4 = ca.Frequency == null ? "" : ca.Frequency.LookupVal.ToUpper().ToString(),
                                        Fld5 = ca.PTWagesMaster == null ? "" : ca.PTWagesMaster.WagesName.ToString(),
                                        Fld6 = ca.PTWagesMaster == null ? "" : ca.PTWagesMaster.CeilingMax.ToString(),
                                        Fld7 = ca.PTWagesMaster == null ? "" : ca.PTWagesMaster.CeilingMin.ToString(),
                                        Fld8 = ca.PTWagesMaster == null ? "" : ca.PTWagesMaster.Percentage.ToString(),
                                        Fld9 = mWageRatePT,
                                        Fld10 = ca1.Id.ToString(),
                                        Fld11 = ca1.EffectiveMonth == null ? "" : ca1.EffectiveMonth.LookupVal.ToUpper().ToString(),
                                        Fld12 = ca2.RangeFrom == null ? "" : ca2.RangeFrom.ToString(),
                                        Fld13 = ca2.RangeTo == null ? "" : ca2.RangeTo.ToString(),
                                        Fld14 = ca2.EmpShareAmount == null ? "" : ca2.EmpShareAmount.ToString(),
                                        Fld15 = ca2.EmpSharePercentage == null ? "" : ca2.EmpSharePercentage.ToString(),
                                        Fld16 = ca2.CompShareAmount == null ? "" : ca2.CompShareAmount.ToString(),
                                        Fld17 = ca2.CompSharePercentage == null ? "" : ca2.CompSharePercentage.ToString(),
                                        Fld18 = ca2.Id.ToString(),

                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALARYHEAD":
                    var OSalSalaryHeadData = db.CompanyPayroll
                        .Include(e => e.SalaryHead)
                        .Include(e => e.SalaryHead.Select(r => r.Frequency))
                        .Include(e => e.SalaryHead.Select(r => r.ProcessType))
                        .Include(e => e.SalaryHead.Select(r => r.RoundingMethod))
                        .Include(e => e.SalaryHead.Select(r => r.SalHeadOperationType))
                        .Include(e => e.SalaryHead.Select(r => r.Type))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalaryHeadData.SalaryHead == null || OSalSalaryHeadData.SalaryHead.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalaryHeadData.SalaryHead)
                        {
                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.Code == null ? "" : ca.Code.ToString(),
                                Fld3 = ca.Name == null ? "" : ca.Name.ToString(),
                                Fld4 = ca.Frequency == null ? "" : ca.Frequency.LookupVal.ToUpper().ToString(),
                                Fld5 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper().ToString(),
                                Fld6 = ca.InPayslip == null ? "" : ca.InPayslip.ToString(),
                                Fld7 = ca.OnAttend == null ? "" : ca.OnAttend.ToString(),
                                Fld8 = ca.InITax == null ? "" : ca.InITax.ToString(),
                                Fld9 = ca.OnLeave == null ? "" : ca.OnLeave.ToString(),
                                Fld10 = ca.RoundingMethod == null ? "" : ca.RoundingMethod.LookupVal.ToUpper().ToString(),
                                Fld11 = ca.RoundDigit == null ? "" : ca.RoundDigit.ToString(),
                                Fld12 = ca.SalHeadOperationType == null ? "" : ca.SalHeadOperationType.LookupVal.ToUpper().ToString(),
                                Fld13 = ca.ProcessType == null ? "" : ca.ProcessType.LookupVal.ToUpper().ToString(),
                                Fld14 = ca.SeqNo == null ? "" : ca.SeqNo.ToString(),

                            };
                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "SALHEADFORMULAAMOUNT":
                    var OSalSalHeadFormulaData = db.CompanyPayroll
                        .Include(e => e.SalHeadFormula)
                        .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.AmountDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.PercentDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.SlabDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.ServiceDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalHeadFormulaData.SalHeadFormula == null || OSalSalHeadFormulaData.SalHeadFormula.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalHeadFormulaData.SalHeadFormula)
                        {
                            if (ca.SalWages == null || ca.SalWages.RateMaster == null || ca.SalWages.RateMaster.Count() == 0)
                            {

                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                    Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                    Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                    Fld5 = "",
                                    Fld6 = "",
                                    Fld7 = "",
                                    Fld8 = "",
                                    Fld9 = "",
                                    Fld10 = "",
                                    Fld11 = "",
                                    Fld12 = "",
                                    Fld13 = ca.AmountDependRule == null ? "" : ca.AmountDependRule.SalAmount.ToString(),
                                    Fld14 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            else
                            {
                                foreach (var ca1 in ca.SalWages.RateMaster)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                        Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                        Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                        Fld5 = ca.SalWages == null ? "" : ca.Name.ToString(),
                                        Fld6 = ca1.SalHead == null ? "" : ca1.SalHead.Id.ToString(),
                                        Fld7 = ca1.SalHead == null ? "" : ca1.SalHead.Code.ToString(),
                                        Fld8 = ca1.SalHead == null ? "" : ca1.SalHead.Name.ToString(),
                                        Fld9 = ca1.Percentage == null ? "" : ca1.Percentage.ToString(),
                                        Fld10 = ca.GeoStruct == null ? "" : ca.GeoStruct.FullDetails.ToString(),
                                        Fld11 = ca.PayStruct == null ? "" : ca.PayStruct.FullDetails.ToString(),
                                        Fld12 = ca.FuncStruct == null ? "" : ca.FuncStruct.FullDetails.ToString(),

                                        Fld13 = ca.AmountDependRule == null ? "" : ca.AmountDependRule.SalAmount.ToString(),
                                        Fld14 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALHEADFORMULAPERCENT":
                    var OSalSalHeadFormulaData1 = db.CompanyPayroll
                        .Include(e => e.SalHeadFormula)
                        .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.SalHeadFormula.Select(r => r.AmountDependRule))
                        .Include(e => e.SalHeadFormula.Select(r => r.PercentDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.SlabDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.ServiceDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalHeadFormulaData1.SalHeadFormula == null || OSalSalHeadFormulaData1.SalHeadFormula.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalHeadFormulaData1.SalHeadFormula)
                        {
                            if (ca.SalWages == null || ca.SalWages.RateMaster == null || ca.SalWages.RateMaster.Count() == 0)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                    Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                    Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                    Fld5 = "",
                                    Fld6 = "",
                                    Fld7 = "",
                                    Fld8 = "",
                                    Fld9 = "",
                                    Fld10 = "",
                                    Fld11 = "",
                                    Fld12 = "",
                                    Fld13 = ca.PercentDependRule == null ? "" : ca.PercentDependRule.SalPercent.ToString(),
                                    Fld14 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            else
                            {
                                foreach (var ca1 in ca.SalWages.RateMaster)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                        Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                        Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                        Fld5 = ca.SalWages == null ? "" : ca.Name.ToString(),
                                        Fld6 = ca1.SalHead == null ? "" : ca1.SalHead.Id.ToString(),
                                        Fld7 = ca1.SalHead == null ? "" : ca1.SalHead.Code.ToString(),
                                        Fld8 = ca1.SalHead == null ? "" : ca1.SalHead.Name.ToString(),
                                        Fld9 = ca1.Percentage == null ? "" : ca1.Percentage.ToString(),
                                        Fld10 = ca.GeoStruct == null ? "" : ca.GeoStruct.FullDetails.ToString(),
                                        Fld11 = ca.PayStruct == null ? "" : ca.PayStruct.FullDetails.ToString(),
                                        Fld12 = ca.FuncStruct == null ? "" : ca.FuncStruct.FullDetails.ToString(),

                                        Fld13 = ca.PercentDependRule == null ? "" : ca.PercentDependRule.SalPercent.ToString(),
                                        Fld14 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALHEADFORMULASlAB":
                    var OSalSalHeadFormulaData2 = db.CompanyPayroll
                        .Include(e => e.SalHeadFormula)
                        .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.SalHeadFormula.Select(r => r.AmountDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.PercentDependRule))
                        .Include(e => e.SalHeadFormula.Select(r => r.SlabDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.ServiceDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalHeadFormulaData2.SalHeadFormula == null || OSalSalHeadFormulaData2.SalHeadFormula.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalHeadFormulaData2.SalHeadFormula)
                        {
                            if (ca.SalWages == null || ca.SalWages.RateMaster == null || ca.SalWages.RateMaster.Count() == 0)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                    Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                    Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                    Fld5 = "",
                                    Fld6 = "",
                                    Fld7 = "",
                                    Fld8 = "",
                                    Fld9 = "",
                                    Fld10 = "",
                                    Fld11 = "",
                                    Fld12 = "",
                                    Fld13 = "",
                                    Fld14 = "",
                                    Fld15 = "",
                                    Fld16 = "",

                                    Fld17 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            else
                            {
                                var mSalWage = "";
                                foreach (var ca1 in ca.SalWages.RateMaster)
                                {
                                    mSalWage = ca1.SalHead.Name.ToString() + " X " + ca1.Percentage.ToString() + " % " + Environment.NewLine;
                                }
                                foreach (var ca2 in ca.SlabDependRule.WageRange)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                        Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                        Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                        Fld5 = ca.SalWages == null ? "" : ca.Name.ToString(),
                                        Fld6 = mSalWage,
                                        Fld7 = ca2.CeilingMax == null ? "" : ca2.CeilingMax.ToString(),
                                        Fld8 = ca2.CeilingMin == null ? "" : ca2.CeilingMin.ToString(),
                                        Fld9 = ca2.Amount == null ? "" : ca2.Amount.ToString(),
                                        Fld10 = ca2.Percentage == null ? "" : ca2.Percentage.ToString(),
                                        Fld11 = ca2.WageFrom == null ? "" : ca2.WageFrom.ToString(),
                                        Fld12 = ca2.WageTo == null ? "" : ca2.WageTo.ToString(),
                                        Fld13 = ca2.Id == null ? "" : ca2.Id.ToString(),

                                        Fld14 = ca.GeoStruct == null ? "" : ca.GeoStruct.FullDetails.ToString(),
                                        Fld15 = ca.PayStruct == null ? "" : ca.PayStruct.FullDetails.ToString(),
                                        Fld16 = ca.FuncStruct == null ? "" : ca.FuncStruct.FullDetails.ToString(),

                                        Fld17 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALHEADFORMULASERVICE":
                    var OSalSalHeadFormulaData3 = db.CompanyPayroll
                        .Include(e => e.SalHeadFormula)
                        .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.SalHeadFormula.Select(r => r.AmountDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.PercentDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.SlabDependRule))
                        .Include(e => e.SalHeadFormula.Select(r => r.ServiceDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalHeadFormulaData3.SalHeadFormula == null || OSalSalHeadFormulaData3.SalHeadFormula.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalHeadFormulaData3.SalHeadFormula)
                        {
                            if (ca.SalWages == null || ca.SalWages.RateMaster == null || ca.SalWages.RateMaster.Count() == 0)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                    Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                    Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                    Fld5 = "",
                                    Fld6 = "",
                                    Fld7 = "",
                                    Fld8 = "",
                                    Fld9 = "",
                                    Fld10 = "",
                                    Fld11 = "",
                                    Fld12 = "",
                                    Fld13 = "",
                                    Fld14 = "",
                                    Fld15 = "",
                                    Fld16 = "",
                                    Fld17 = "",
                                    Fld18 = "",
                                    Fld19 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            else
                            {
                                var mSalWage = "";
                                foreach (var ca1 in ca.SalWages.RateMaster)
                                {
                                    mSalWage = ca1.SalHead.Name.ToString() + " X " + ca1.Percentage.ToString() + " % " + Environment.NewLine;
                                }
                                foreach (var ca2 in ca.ServiceDependRule.ServiceRange)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                        Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                        Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                        Fld5 = ca.SalWages == null ? "" : ca.Name.ToString(),
                                        Fld6 = mSalWage,
                                        Fld7 = ca2.CeilingMax == null ? "" : ca2.CeilingMax.ToString(),
                                        Fld8 = ca2.CeilingMin == null ? "" : ca2.CeilingMin.ToString(),
                                        Fld9 = ca2.Amount == null ? "" : ca2.Amount.ToString(),
                                        Fld10 = ca2.Percentage == null ? "" : ca2.Percentage.ToString(),
                                        Fld11 = ca2.WagesFrom == null ? "" : ca2.WagesFrom.ToString(),
                                        Fld12 = ca2.WagesTo == null ? "" : ca2.WagesTo.ToString(),
                                        Fld13 = ca2.Id == null ? "" : ca2.Id.ToString(),
                                        Fld14 = ca2.ServiceFrom == null ? "" : ca2.ServiceFrom.ToString(),
                                        Fld15 = ca2.ServiceTo == null ? "" : ca2.ServiceTo.ToString(),

                                        Fld16 = ca.GeoStruct == null ? "" : ca.GeoStruct.FullDetails.ToString(),
                                        Fld17 = ca.PayStruct == null ? "" : ca.PayStruct.FullDetails.ToString(),
                                        Fld18 = ca.FuncStruct == null ? "" : ca.FuncStruct.FullDetails.ToString(),

                                        Fld19 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                }
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALHEADFORMULAVDA":
                    var OSalSalHeadFormulaData4 = db.CompanyPayroll
                        .Include(e => e.SalHeadFormula)
                        .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.SalHeadFormula.Select(r => r.AmountDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.PercentDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.SlabDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.ServiceDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                        .Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalHeadFormulaData4.SalHeadFormula == null || OSalSalHeadFormulaData4.SalHeadFormula.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalHeadFormulaData4.SalHeadFormula)
                        {
                            var mSalWage = "";
                            //if(ca.SalWages!=null && ca.SalWages.RateMaster!=null && ca.SalWages.RateMaster.Count()>0)
                            //{
                            //foreach (var ca1 in ca.SalWages.RateMaster)
                            //    {
                            //        mSalWage = ca1.SalHead.Name.ToString() + " X " + ca1.Percentage.ToString() + " % " + Environment.NewLine;
                            //    }
                            //}
                            foreach (var ca2 in ca.VDADependRule.CPIRule.CPIRuleDetails)
                            {
                                foreach (var ca3 in ca2.CPIWages.RateMaster)
                                {
                                    mSalWage = ca3.SalHead.Name.ToString() + " X " + ca3.Percentage.ToString() + " % " + Environment.NewLine;
                                }

                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                    Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                    Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                    Fld5 = ca2.CPIWages == null ? "" : ca2.CPIWages.WagesName.ToString(),
                                    Fld6 = mSalWage,
                                    Fld7 = ca2.CPIWages.CeilingMax == null ? "" : ca2.CPIWages.CeilingMax.ToString(),
                                    Fld8 = ca2.CPIWages.CeilingMin == null ? "" : ca2.CPIWages.CeilingMin.ToString(),
                                    Fld9 = ca2.CPIWages.Percentage == null ? "" : ca2.CPIWages.Percentage.ToString(),
                                    Fld10 = ca2.Id == null ? "" : ca2.Id.ToString(),
                                    Fld11 = ca2.SalFrom == null ? "" : ca2.SalFrom.ToString(),
                                    Fld12 = ca2.SalTo == null ? "" : ca2.SalTo.ToString(),
                                    Fld13 = ca2.IncrPercent == null ? "" : ca2.IncrPercent.ToString(),
                                    Fld14 = ca2.AdditionalIncrAmount == null ? "" : ca2.AdditionalIncrAmount.ToString(),
                                    Fld15 = ca.VDADependRule.CPIRule.CPIUnitCalc == null ? "" : ca.VDADependRule.CPIRule.CPIUnitCalc.Select(e => e.BaseIndex).ToString(),
                                    Fld16 = ca.VDADependRule.CPIRule.CPIUnitCalc == null ? "" : ca.VDADependRule.CPIRule.CPIUnitCalc.Select(e => e.IndexMaxCeiling).ToString(),

                                    Fld17 = ca.GeoStruct == null ? "" : ca.GeoStruct.FullDetails.ToString(),
                                    Fld18 = ca.PayStruct == null ? "" : ca.PayStruct.FullDetails.ToString(),
                                    Fld19 = ca.FuncStruct == null ? "" : ca.FuncStruct.FullDetails.ToString(),

                                    Fld20 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }

                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALHEADFORMULABASICSCALE":
                    var OSalSalHeadFormulaData5 = db.CompanyPayroll
                        .Include(e => e.SalHeadFormula)
                        .Include(e => e.SalHeadFormula.Select(r => r.GeoStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.FuncStruct))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                        .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                        .Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.SalHeadFormula.Select(r => r.AmountDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.PercentDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.SlabDependRule))
                        //.Include(e => e.SalHeadFormula.Select(r => r.ServiceDependRule))
                        .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                        // .Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))

                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSalHeadFormulaData5.SalHeadFormula == null || OSalSalHeadFormulaData5.SalHeadFormula.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSalHeadFormulaData5.SalHeadFormula)
                        {
                            var mSalWage = "";
                            //if(ca.SalWages!=null && ca.SalWages.RateMaster!=null && ca.SalWages.RateMaster.Count()>0)
                            //{
                            //foreach (var ca1 in ca.SalWages.RateMaster)
                            //    {
                            //        mSalWage = ca1.SalHead.Name.ToString() + " X " + ca1.Percentage.ToString() + " % " + Environment.NewLine;
                            //    }
                            //}
                            foreach (var ca2 in ca.BASICDependRule.BasicScale.BasicScaleDetails)
                            {


                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.ToString(),
                                    Fld3 = ca.CeilingMin == null ? "" : ca.CeilingMin.ToString(),
                                    Fld4 = ca.CeilingMax == null ? "" : ca.CeilingMax.ToString(),
                                    Fld5 = ca.BASICDependRule.BasicScale == null ? "" : ca.BASICDependRule.BasicScale.ScaleName.ToString(),
                                    Fld6 = ca2.Id.ToString(),
                                    Fld7 = ca2.StartingSlab == null ? "" : ca2.StartingSlab.ToString(),
                                    Fld8 = ca2.EndingSlab == null ? "" : ca2.EndingSlab.ToString(),
                                    Fld9 = ca2.IncrementAmount == null ? "" : ca2.IncrementAmount.ToString(),
                                    Fld10 = ca2.IncrementCount == null ? "" : ca2.IncrementCount.ToString(),
                                    Fld11 = ca2.EBMark == null ? "" : ca2.EBMark.ToString(),

                                    Fld12 = ca.GeoStruct == null ? "" : ca.GeoStruct.FullDetails.ToString(),
                                    Fld13 = ca.PayStruct == null ? "" : ca.PayStruct.FullDetails.ToString(),
                                    Fld14 = ca.FuncStruct == null ? "" : ca.FuncStruct.FullDetails.ToString(),

                                    Fld15 = ca.PayScaleAssignment == null ? "" : ca.PayScaleAssignment.Select(e => e.SalaryHead.Name).FirstOrDefault().ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "SUSPENSIONSALPOLICY":
                    var OSalSuspensionSalPolicyData = db.CompanyPayroll
                        .Include(e => e.SuspensionSalPolicy)
                        .Include(e => e.SuspensionSalPolicy.Select(r => r.DayRange))
                        .Include(e => e.SuspensionSalPolicy.Select(r => r.SuspensionWages))
                        .Include(e => e.SuspensionSalPolicy.Select(r => r.SuspensionWages.RateMaster))


                    .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                    if (OSalSuspensionSalPolicyData.SuspensionSalPolicy == null || OSalSuspensionSalPolicyData.SuspensionSalPolicy.Count() == 0)
                        return null;
                    else
                    {
                        foreach (var ca in OSalSuspensionSalPolicyData.SuspensionSalPolicy)
                        {
                            foreach (var ca1 in ca.DayRange)
                            {

                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.PolicyName == null ? "" : ca.PolicyName.ToString(),
                                    Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.ToString(),
                                    Fld4 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                    Fld5 = ca1.Id == null ? "" : ca1.Id.ToString(),
                                    Fld6 = ca1.RangeFrom == null ? "" : ca1.RangeFrom.ToString(),
                                    Fld7 = ca1.RangeTo == null ? "" : ca1.RangeTo.ToString(),
                                    Fld8 = ca1.EmpShareAmount == null ? "" : ca1.EmpShareAmount.ToString(),
                                    Fld9 = ca1.EmpSharePercentage == null ? "" : ca1.EmpSharePercentage.ToString(),
                                    Fld10 = ca1.CompShareAmount == null ? "" : ca1.CompShareAmount.ToString(),
                                    Fld11 = ca1.CompSharePercentage == null ? "" : ca1.CompSharePercentage.ToString(),
                                    Fld12 = ca.SuspensionWages == null ? "" : ca.SuspensionWages.WagesName.ToString(),
                                    Fld13 = ca.SuspensionWages == null ? "" : ca.SuspensionWages.CeilingMax.ToString(),
                                    Fld14 = ca.SuspensionWages == null ? "" : ca.SuspensionWages.CeilingMin.ToString(),
                                    Fld15 = ca.SuspensionWages == null ? "" : ca.SuspensionWages.Percentage.ToString(),
                                    Fld16 = ca.SuspensionWages == null ? "" : ca.SuspensionWages.Id.ToString(),





                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                        }
                        return OGenericPayrollStatement;
                    }
                    break;



                /////////////Employeee////////////////////////////////////

                //case "EMPSALSTRUCT":
                //    var OEmpSalStructData = db.EmployeePayroll
                //        .Include(e => e.Employee)
                //        //.Include(e => e.Employee)
                //        .Include(e => e.Employee.EmpName)
                //        .Include(e => e.EmpSalStruct)
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Division))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Division.Id))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Division.Code))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Division.Name))
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Location))
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Location.LocationObj))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Location.LocationObj.LocDesc))
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Department))
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Department.DepartmentObj))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Department.DepartmentObj.DeptDesc))
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Group))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Group.Code))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Group.Name))
                //        .Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Unit))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Unit.Code))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.GeoStruct.Unit.Name))
                //        .Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Grade))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Grade.Code))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Grade.Name))
                //        .Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Level))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Level.Code))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.Level.Name))
                //        .Include(e => e.EmpSalStruct.Select(r => r.PayStruct.JobStatus))
                //        .Include(e => e.EmpSalStruct.Select(r => r.PayStruct.JobStatus.EmpStatus))
                //        .Include(e => e.EmpSalStruct.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                //        .Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.Job))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.Job.Name))
                //        .Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.Job.JobPosition))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.Job.JobPosition.Select(t => t.JobPositionCode)))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.FuncStruct.Job.JobPosition.Select(t => t.JobPositionDesc)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Code)))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Name)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.FullDetails)))
                //        //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.Amount)))
                //        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))



                //    .Where(d => EmpPayrollIdList.Contains(d.Employee.Id))// && (mPayMonth.Contains(d.EmpSalStruct.SelectMany(r => r.EffectiveDate.Value.ToString("MM/yyyy")))))
                //    .ToList();

                //    if (OEmpSalStructData.Select(e => e.EmpSalStruct) == null || OEmpSalStructData.Select(e => e.EmpSalStruct).Count() == 0)
                //        return null;
                //    else
                //    {
                //        var a=OEmpSalStructData.Select(e=>e.EmpSalStruct).Where(e =>mPayMonth.Contains(e.EmpSalStruct.SelectMany(t=>t.EffectiveDate.Value.ToString("MM/YYYY")))).ToList();
                //        foreach (var ca in a)
                //        {
                //            foreach (var ca1 in ca.EmpSalStruct)
                //            {
                //                foreach (var ca2 in ca1.EmpSalStructDetails)
                //                {
                //                    GenericField100 OGenericObj = new GenericField100()
                //                    //write data to generic class
                //                    {
                //                        Fld1 = ca.Employee.Id.ToString(),
                //                        Fld2 = ca.Employee.EmpCode,
                //                        Fld3 = ca.Employee.EmpName.FullNameFML,
                //                        Fld4 = ca1.Id.ToString(),
                //                        Fld5 = ca1.EffectiveDate == null ? "" : ca1.EffectiveDate.Value.ToShortDateString(),
                //                        Fld6 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                //                        //Fld7 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.FullDetails.ToString(),
                //                        //Fld8 = ca1.PayStruct == null ? "" : ca1.PayStruct.FullDetails.ToString(),
                //                        //Fld9 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.FullDetails.ToString(),
                //                        Fld10 = ca1.PayDays == null ? "" : ca1.PayDays.ToString(),
                //                        Fld51 = ca2.Id.ToString(),
                //                        Fld52 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Id.ToString(),
                //                        Fld53 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Code.ToString(),
                //                        Fld54 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Name.ToString(),
                //                        Fld55 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Type.LookupVal.ToUpper().ToString(),
                //                        Fld56 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Frequency.LookupVal.ToUpper().ToString(),
                //                        Fld57 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString(),
                //                        Fld58 = ca2.Amount == null ? "" : ca2.Amount.ToString(),

                //                        //Fld11 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Id.ToString() : null,
                //                        //Fld12 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Code : null,
                //                        //Fld13 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Name : null,

                //                        //Fld14 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null ? ca1.GeoStruct.Location.Id.ToString() : null,

                //                        Fld15 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocCode : null,
                //                        Fld16 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocDesc : null,
                //                        Fld17 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null ? ca1.GeoStruct.Department.Id.ToString() : null,
                //                        Fld18 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptCode : null,
                //                        Fld19 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptDesc : null,


                //                        //Fld20 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Id.ToString() : null,
                //                        //Fld21 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Code : null,
                //                        //Fld22 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Name : null,

                //                        //Fld23 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Id.ToString() : null,
                //                        //Fld24 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Code : null,
                //                        //Fld25 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Name : null,

                //                        Fld26 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Id.ToString() : null,
                //                        Fld27 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Code : null,
                //                        Fld28 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Name : null,
                //                        Fld29 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.Id.ToString() : null,
                //                        Fld30 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionCode : null,
                //                        Fld31 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionDesc : null,

                //                        Fld32 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Id.ToString() : null,
                //                        Fld33 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Code : null,
                //                        Fld34 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Name : null,

                //                        //Fld35 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Id.ToString() : null,
                //                        //Fld36 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Code : null,
                //                        //Fld37 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Name : null,

                //                        Fld38 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null ? ca1.PayStruct.JobStatus.Id.ToString() : null,
                //                        Fld39 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpStatus != null ? ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                //                        //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                //                        Fld40 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpActingStatus != null ? ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,

                //                        //Fld19 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Id.ToString(),
                //                        //Fld20 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Code,
                //                        //Fld21 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                //                        //Fld22 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.Id.ToString(),
                //                        //Fld23 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocCode,
                //                        //Fld24 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                //                        //Fld25 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                //                        //Fld26 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                //                        //Fld27 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                //                        //Fld28 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                //                        //Fld29 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                //                        //Fld30 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                //                        //Fld31 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                //                        //Fld32 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                //                        //Fld33 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                //                        //Fld34 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                //                        //Fld35 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                //                        //Fld36 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                //                        //Fld37 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                //                        //Fld38 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                //                        //Fld39 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                //                        //Fld40 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                //                        //Fld41 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                //                        //Fld42 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                //                        //Fld43 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                //                        //Fld44 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                //                        //Fld45 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                //                        //Fld46 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                //                        //Fld47 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                //                        //Fld48 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                //                    };
                //                    OGenericPayrollStatement.Add(OGenericObj);
                //                }
                //            }

                //        }

                //        return OGenericPayrollStatement;
                //    }
                //    break;


                ///////////////////////////////Payroll///////////////////////
               // case "PAYSLIPR":
                    //var OPayslipData = db.EmployeePayroll
                    //.Include(e => e.SalaryT.Select(r => r.PayslipR))
                    //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailEarnR)))
                    //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailDedR)))
                    //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailLeaveR)))
                    //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    //.ToList();

                    //OGenericPayrollStatement = new List<GenericField100>();
                    //if (OPayslipData == null || OPayslipData.Count() == 0)
                    //{
                    //    return null;
                    //}
                    //else
                    //{

                    //    foreach (var ca in OPayslipData)
                    //    {
                    //        if (ca.SalaryT != null && ca.SalaryT.Count() != 0)
                    //        {

                    //            foreach (var ca1 in ca.SalaryT)
                    //            {
                    //                if (ca1.PayslipR != null && ca1.PayslipR.Count() != 0)
                    //                {
                    //                    var OSalPayslip = ca1.PayslipR.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                    //                    foreach (var ca2 in OSalPayslip)
                    //                    {
                    //                        if (ca2.PaySlipDetailDedR != null && ca2.PaySlipDetailDedR.ToList().Count() != null)
                    //                        {
                    //                            foreach (var item in ca2.PaySlipDetailDedR)
                    //                            {
                    //                                GenericField100 oGenericField100 = new GenericField100()
                    //                                {
                    //                                    Fld1 = ca2.EmpCode,
                    //                                    Fld2 = ca2.EmpName,
                    //                                    Fld3 = ca2.EmpStatus,
                    //                                    Fld4 = ca2.EmpActingStatus,
                    //                                    Fld5 = ca2.PANNo,
                    //                                    Fld6 = ca2.PFNo,
                    //                                    Fld7 = ca2.PensionNo,
                    //                                    Fld8 = ca2.AadharNo,
                    //                                    Fld9 = ca2.ESICNo,
                    //                                    Fld10 = ca2.GISNo,
                    //                                    Fld11 = ca2.SuperAnnuNo,
                    //                                    Fld12 = ca2.MediclaimNo,
                    //                                    Fld13 = ca2.PaymentBank,
                    //                                    Fld14 = ca2.PaymentBranch,
                    //                                    Fld15 = ca2.BankAcType,
                    //                                    Fld16 = ca2.BankAcNo,
                    //                                    Fld17 = ca2.PayScale,
                    //                                    Fld18 = ca2.CPIndex,
                    //                                    Fld19 = ca2.CPIndexIBase,
                    //                                    Fld20 = ca2.MonthYear != null ? ca2.MonthYear.Value.ToShortDateString() : null,
                    //                                    Fld21 = ca2.PayMonth,
                    //                                    Fld22 = ca2.ProcessDate != null ? ca2.ProcessDate.Value.ToShortDateString() : null,
                    //                                    Fld23 = ca2.PaymentDate != null ? ca2.PaymentDate.Value.ToShortDateString() : null,
                    //                                    Fld24 = ca2.Corporate,
                    //                                    Fld25 = ca2.Region,
                    //                                    Fld26 = ca2.Company,
                    //                                    Fld27 = ca2.Division,
                    //                                    Fld28 = ca2.Location,
                    //                                    Fld29 = ca2.Department,
                    //                                    Fld30 = ca2.Group,
                    //                                    Fld31 = ca2.Unit,
                    //                                    Fld32 = ca2.Grade,
                    //                                    Fld33 = ca2.Level,
                    //                                    Fld34 = ca2.Job,
                    //                                    Fld35 = ca2.JobStatus,
                    //                                    Fld36 = ca2.DateJoining != null ? ca2.DateJoining.Value.ToShortDateString() : null,
                    //                                    Fld37 = ca2.DateIncrement != null ? ca2.DateIncrement.Value.ToShortDateString() : null,
                    //                                    Fld38 = ca2.DateRetirement != null ? ca2.DateRetirement.Value.ToShortDateString() : null,
                    //                                    Fld39 = ca2.DateBirth != null ? ca2.DateBirth.Value.ToShortDateString() : null,
                    //                                    Fld40 = ca2.DateConfirmation != null ? ca2.DateConfirmation.Value.ToShortDateString() : null,
                    //                                    Fld41 = ca2.DateProbation != null ? ca2.DateProbation.Value.ToShortDateString() : null,
                    //                                    Fld42 = ca2.DateTransfer != null ? ca2.DateTransfer.Value.ToShortDateString() : null,
                    //                                    Fld43 = ca2.DatePromotion != null ? ca2.DatePromotion.Value.ToShortDateString() : null,
                    //                                    Fld44 = ca2.PayableDays.ToString(),
                    //                                    Fld45 = ca2.LWPDays.ToString(),
                    //                                    Fld46 = ca2.StdPay.ToString(),
                    //                                    Fld47 = ca2.GrossPay.ToString(),
                    //                                    Fld48 = ca2.DeductionPay.ToString(),
                    //                                    Fld49 = ca2.NetPay.ToString(),
                    //                                    Fld50 = ca2.NetPayInWords,
                    //                                    Fld51 = ca2.ProvisionalGratuity.ToString(),
                    //                                    Fld52 = ca2.PayslipNote,
                    //                                    Fld53 = ca2.PayslipRemark,
                    //                                    Fld54 = ca2.TotalITaxLiability.ToString(),
                    //                                    Fld55 = ca2.TotalITaxPaid.ToString(),
                    //                                    Fld56 = ca2.TotalITaxBalance.ToString(),
                    //                                    Fld57 = ca2.ITaxPerMonth.ToString(),
                    //                                    Fld58 = ca2.PaySlipLock.ToString(),
                    //                                    Fld59 = ca2.PayslipLockDate.ToString(),

                    //                                    Fld60 = item.SalHeadDesc,
                    //                                    Fld61 = item.StdSalAmount.ToString(),
                    //                                    Fld62 = item.DedAmount.ToString(),
                    //                                };
                    //                                OGenericPayrollStatement.Add(oGenericField100);
                    //                            }
                    //                        }
                    //                        if (ca2.PaySlipDetailEarnR != null)
                    //                        {
                    //                            foreach (var item in ca2.PaySlipDetailEarnR)
                    //                            {
                    //                                GenericField100 oGenericField100 = new GenericField100()
                    //                                {
                    //                                    Fld1 = ca2.EmpCode,
                    //                                    Fld2 = ca2.EmpName,
                    //                                    Fld3 = ca2.EmpStatus,
                    //                                    Fld4 = ca2.EmpActingStatus,
                    //                                    Fld5 = ca2.PANNo,
                    //                                    Fld6 = ca2.PFNo,
                    //                                    Fld7 = ca2.PensionNo,
                    //                                    Fld8 = ca2.AadharNo,
                    //                                    Fld9 = ca2.ESICNo,
                    //                                    Fld10 = ca2.GISNo,
                    //                                    Fld11 = ca2.SuperAnnuNo,
                    //                                    Fld12 = ca2.MediclaimNo,
                    //                                    Fld13 = ca2.PaymentBank,
                    //                                    Fld14 = ca2.PaymentBranch,
                    //                                    Fld15 = ca2.BankAcType,
                    //                                    Fld16 = ca2.BankAcNo,
                    //                                    Fld17 = ca2.PayScale,
                    //                                    Fld18 = ca2.CPIndex,
                    //                                    Fld19 = ca2.CPIndexIBase,
                    //                                    Fld20 = ca2.MonthYear != null ? ca2.MonthYear.Value.ToShortDateString() : null,
                    //                                    Fld21 = ca2.PayMonth,
                    //                                    Fld22 = ca2.ProcessDate != null ? ca2.ProcessDate.Value.ToShortDateString() : null,
                    //                                    Fld23 = ca2.PaymentDate != null ? ca2.PaymentDate.Value.ToShortDateString() : null,
                    //                                    Fld24 = ca2.Corporate,
                    //                                    Fld25 = ca2.Region,
                    //                                    Fld26 = ca2.Company,
                    //                                    Fld27 = ca2.Division,
                    //                                    Fld28 = ca2.Location,
                    //                                    Fld29 = ca2.Department,
                    //                                    Fld30 = ca2.Group,
                    //                                    Fld31 = ca2.Unit,
                    //                                    Fld32 = ca2.Grade,
                    //                                    Fld33 = ca2.Level,
                    //                                    Fld34 = ca2.Job,
                    //                                    Fld35 = ca2.JobStatus,
                    //                                    Fld36 = ca2.DateJoining != null ? ca2.DateJoining.Value.ToShortDateString() : null,
                    //                                    Fld37 = ca2.DateIncrement != null ? ca2.DateIncrement.Value.ToShortDateString() : null,
                    //                                    Fld38 = ca2.DateRetirement != null ? ca2.DateRetirement.Value.ToShortDateString() : null,
                    //                                    Fld39 = ca2.DateBirth != null ? ca2.DateBirth.Value.ToShortDateString() : null,
                    //                                    Fld40 = ca2.DateConfirmation != null ? ca2.DateConfirmation.Value.ToShortDateString() : null,
                    //                                    Fld41 = ca2.DateProbation != null ? ca2.DateProbation.Value.ToShortDateString() : null,
                    //                                    Fld42 = ca2.DateTransfer != null ? ca2.DateTransfer.Value.ToShortDateString() : null,
                    //                                    Fld43 = ca2.DatePromotion != null ? ca2.DatePromotion.Value.ToShortDateString() : null,
                    //                                    Fld44 = ca2.PayableDays.ToString(),
                    //                                    Fld45 = ca2.LWPDays.ToString(),
                    //                                    Fld46 = ca2.StdPay.ToString(),
                    //                                    Fld47 = ca2.GrossPay.ToString(),
                    //                                    Fld48 = ca2.DeductionPay.ToString(),
                    //                                    Fld49 = ca2.NetPay.ToString(),
                    //                                    Fld50 = ca2.NetPayInWords,
                    //                                    Fld51 = ca2.ProvisionalGratuity.ToString(),
                    //                                    Fld52 = ca2.PayslipNote,
                    //                                    Fld53 = ca2.PayslipRemark,
                    //                                    Fld54 = ca2.TotalITaxLiability.ToString(),
                    //                                    Fld55 = ca2.TotalITaxPaid.ToString(),
                    //                                    Fld56 = ca2.TotalITaxBalance.ToString(),
                    //                                    Fld57 = ca2.ITaxPerMonth.ToString(),
                    //                                    Fld58 = ca2.PaySlipLock.ToString(),
                    //                                    Fld59 = ca2.PayslipLockDate.ToString(),

                    //                                    Fld63 = item.SalHeadDesc,
                    //                                    Fld64 = item.StdSalAmount.ToString(),
                    //                                    Fld65 = item.EarnAmount.ToString(),
                    //                                };
                    //                                OGenericPayrollStatement.Add(oGenericField100);
                    //                            }
                    //                        }

                    //                        if (ca2.PaySlipDetailLeaveR != null)
                    //                        {
                    //                            foreach (var item in ca2.PaySlipDetailLeaveR)
                    //                            {
                    //                                GenericField100 oGenericField100 = new GenericField100()
                    //                                {
                    //                                    Fld1 = ca2.EmpCode,
                    //                                    Fld2 = ca2.EmpName,
                    //                                    Fld3 = ca2.EmpStatus,
                    //                                    Fld4 = ca2.EmpActingStatus,
                    //                                    Fld5 = ca2.PANNo,
                    //                                    Fld6 = ca2.PFNo,
                    //                                    Fld7 = ca2.PensionNo,
                    //                                    Fld8 = ca2.AadharNo,
                    //                                    Fld9 = ca2.ESICNo,
                    //                                    Fld10 = ca2.GISNo,
                    //                                    Fld11 = ca2.SuperAnnuNo,
                    //                                    Fld12 = ca2.MediclaimNo,
                    //                                    Fld13 = ca2.PaymentBank,
                    //                                    Fld14 = ca2.PaymentBranch,
                    //                                    Fld15 = ca2.BankAcType,
                    //                                    Fld16 = ca2.BankAcNo,
                    //                                    Fld17 = ca2.PayScale,
                    //                                    Fld18 = ca2.CPIndex,
                    //                                    Fld19 = ca2.CPIndexIBase,
                    //                                    Fld20 = ca2.MonthYear != null ? ca2.MonthYear.Value.ToShortDateString() : null,
                    //                                    Fld21 = ca2.PayMonth,
                    //                                    Fld22 = ca2.ProcessDate != null ? ca2.ProcessDate.Value.ToShortDateString() : null,
                    //                                    Fld23 = ca2.PaymentDate != null ? ca2.PaymentDate.Value.ToShortDateString() : null,
                    //                                    Fld24 = ca2.Corporate,
                    //                                    Fld25 = ca2.Region,
                    //                                    Fld26 = ca2.Company,
                    //                                    Fld27 = ca2.Division,
                    //                                    Fld28 = ca2.Location,
                    //                                    Fld29 = ca2.Department,
                    //                                    Fld30 = ca2.Group,
                    //                                    Fld31 = ca2.Unit,
                    //                                    Fld32 = ca2.Grade,
                    //                                    Fld33 = ca2.Level,
                    //                                    Fld34 = ca2.Job,
                    //                                    Fld35 = ca2.JobStatus,
                    //                                    Fld36 = ca2.DateJoining != null ? ca2.DateJoining.Value.ToShortDateString() : null,
                    //                                    Fld37 = ca2.DateIncrement != null ? ca2.DateIncrement.Value.ToShortDateString() : null,
                    //                                    Fld38 = ca2.DateRetirement != null ? ca2.DateRetirement.Value.ToShortDateString() : null,
                    //                                    Fld39 = ca2.DateBirth != null ? ca2.DateBirth.Value.ToShortDateString() : null,
                    //                                    Fld40 = ca2.DateConfirmation != null ? ca2.DateConfirmation.Value.ToShortDateString() : null,
                    //                                    Fld41 = ca2.DateProbation != null ? ca2.DateProbation.Value.ToShortDateString() : null,
                    //                                    Fld42 = ca2.DateTransfer != null ? ca2.DateTransfer.Value.ToShortDateString() : null,
                    //                                    Fld43 = ca2.DatePromotion != null ? ca2.DatePromotion.Value.ToShortDateString() : null,
                    //                                    Fld44 = ca2.PayableDays.ToString(),
                    //                                    Fld45 = ca2.LWPDays.ToString(),
                    //                                    Fld46 = ca2.StdPay.ToString(),
                    //                                    Fld47 = ca2.GrossPay.ToString(),
                    //                                    Fld48 = ca2.DeductionPay.ToString(),
                    //                                    Fld49 = ca2.NetPay.ToString(),
                    //                                    Fld50 = ca2.NetPayInWords,
                    //                                    Fld51 = ca2.ProvisionalGratuity.ToString(),
                    //                                    Fld52 = ca2.PayslipNote,
                    //                                    Fld53 = ca2.PayslipRemark,
                    //                                    Fld54 = ca2.TotalITaxLiability.ToString(),
                    //                                    Fld55 = ca2.TotalITaxPaid.ToString(),
                    //                                    Fld56 = ca2.TotalITaxBalance.ToString(),
                    //                                    Fld57 = ca2.ITaxPerMonth.ToString(),
                    //                                    Fld58 = ca2.PaySlipLock.ToString(),
                    //                                    Fld59 = ca2.PayslipLockDate.ToString(),

                    //                                    Fld67 = item.LeaveHead,
                    //                                    Fld68 = item.LeaveOpenBal.ToString(),
                    //                                    Fld69 = item.LeaveUtilized.ToString(),
                    //                                };
                    //                                OGenericPayrollStatement.Add(oGenericField100);
                    //                            }
                    //                        }
                    //                    }
                    //                }

                    //            }
                    //        }

                    //    }

                    //    if (OGenericPayrollStatement.Count > 0 && OGenericPayrollStatement != null)
                    //    {
                    //        return OGenericPayrollStatement.Distinct().ToList();
                    //    }
                    //    else
                    //    {
                    //        return null;
                    //    }
                    //}
                    //break;
                case "PAYSLIPR":
                     List<EmployeePayroll> OPayslipData = new List<EmployeePayroll>();
                        Parallel.ForEach(EmpPayrollIdList, (item) =>
                        {
                            //foreach (int item in EmpPayrollIdList)
                            //{
                            //var OPayslipData_temp = db.EmployeePayroll
                            //    .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                            //.Include(e => e.SalaryT.Select(r => r.PayslipR))
                            //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(a => a.GeoStruct.Location.LocationObj)))
                            //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailEarnR)))
                            //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailDedR)))
                            //.Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailLeaveR)))
                            //.Where(e => e.Employee.Id == item).AsParallel()
                            //.FirstOrDefault();
                            string MonYear = mPayMonth.First();
                            EmployeePayroll oEmployeePayroll = ReportRDLCObjectClass._returnGetEmployeeData(item, MonYear);
                            if (oEmployeePayroll != null)
                            {
                                OPayslipData.Add(oEmployeePayroll);
                            }
                        });
                        //}
                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OPayslipData == null || OPayslipData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            List<PaySlipR> PayslipRList = new List<PaySlipR>();
                            bool emp = false;
                            bool loca = false;
                            //if (OrderBy == "EMPLOYEE")
                            //{
                            //    emp = true;
                            //}
                            //if (OrderBy == "LOCATION")
                            //{
                            //    loca = true;
                            //}
                            //else
                            //{
                            //    loca = true;
                            //}
                            //  PayslipRList.AddRange(oSalaryT.PayslipR.Where(e => e.PayMonth == item).ToList());

                            foreach (EmployeePayroll ca in OPayslipData)
                            {
                                if (ca.SalaryT != null && ca.SalaryT.Count() != 0)
                                {
                                    foreach (SalaryT oSalaryT in ca.SalaryT)
                                    {
                                        foreach (string item in mPayMonth)
                                        {
                                            PayslipRList.AddRange(oSalaryT.PayslipR.Where(e => e.PayMonth == item).ToList());
                                        }
                                    }
                                }
                            }
                            if (PayslipRList.Count > 0)
                            {
                                if (loca)
                                {
                                    PayslipRList = PayslipRList.OrderBy(e => e.GeoStruct.Location.LocationObj.LocCode).ToList();
                                }
                                else if (emp)
                                {
                                    PayslipRList = PayslipRList.OrderBy(e => e.EmpCode).ToList();
                                }
                            }

                            var OSalPayslip = new List<PaySlipR>();
                            foreach (var ca2 in PayslipRList)
                            {
                                var UANNO = db.Employee.Where(e => e.EmpCode == ca2.EmpCode).OrderBy(e => e.Id
                                    ).Select(a => a.EmpOffInfo.NationalityID.UANNo).AsNoTracking().SingleOrDefault();
                                string v = DateTime.Parse("01/" + ca2.PayMonth).ToShortDateString();

                                string dt = DateTime.Parse(v.Trim()).ToString("MMMM/yyyy", CultureInfo.InvariantCulture);

                                GenericField100 oGenericField1001 = new GenericField100()
                                {
                                    Fld1 = ca2.EmpCode,
                                    Fld2 = ca2.EmpName,
                                    //Fld3 = ca2.EmpStatus,
                                    //Fld4 = ca2.EmpActingStatus,
                                    Fld5 = ca2.PANNo,
                                    Fld6 = ca2.PFNo,
                                    //Fld7 = ca2.PensionNo,
                                    Fld8 = ca2.AadharNo,
                                    // Fld9 = ca2.ESICNo,
                                    //Fld10 = ca2.GISNo,
                                    //Fld11 = ca2.SuperAnnuNo,
                                    //Fld12 = ca2.MediclaimNo,
                                    //Fld13 = ca2.PaymentBank,
                                    //Fld14 = ca2.PaymentBranch,
                                    //Fld15 = ca2.BankAcType,
                                    Fld16 = ca2.BankAcNo,
                                    //Fld17 = ca2.PayScale,
                                    Fld18 = ca2.CPIndex,
                                    //Fld19 = ca2.CPIndexIBase,
                                    //Fld20 = ca2.MonthYear != null ? ca2.MonthYear.Value.ToShortDateString() : null,

                                    Fld21 = dt,

                                    Fld22 = ca2.ProcessDate != null ? ca2.ProcessDate.Value.ToShortDateString() : null,
                                    //Fld23 = ca2.PaymentDate != null ? ca2.PaymentDate.Value.ToShortDateString() : null,
                                    //Fld24 = ca2.Corporate,
                                    //Fld25 = ca2.Region,
                                    //Fld26 = ca2.Company,
                                    //Fld27 = ca2.Division,
                                    Fld28 = ca2.Location,
                                    Fld29 = ca2.Department,
                                    //Fld30 = ca2.Group,
                                    //Fld31 = ca2.Unit,
                                    Fld32 = ca2.Grade,
                                    // Fld33 = ca2.Level,
                                    Fld34 = ca2.Job,
                                    //Fld35 = ca2.JobStatus,
                                    Fld36 = ca2.DateJoining != null ? ca2.DateJoining.Value.ToShortDateString() : null,
                                    //Fld37 = ca2.DateIncrement != null ? ca2.DateIncrement.Value.ToShortDateString() : null,
                                    Fld38 = ca2.DateRetirement != null ? ca2.DateRetirement.Value.ToShortDateString() : null,
                                    Fld39 = ca2.DateBirth != null ? ca2.DateBirth.Value.ToShortDateString() : null,
                                    Fld40 = ca2.DateConfirmation != null ? ca2.DateConfirmation.Value.ToShortDateString() : null,
                                    Fld41 = ca2.DateProbation != null ? ca2.DateProbation.Value.ToShortDateString() : null,
                                    //Fld42 = ca2.DateTransfer != null ? ca2.DateTransfer.Value.ToShortDateString() : null,
                                    //Fld43 = ca2.DatePromotion != null ? ca2.DatePromotion.Value.ToShortDateString() : null,
                                    Fld44 = ca2.PayableDays.ToString(),
                                    //Fld45 = ca2.LWPDays.ToString(),
                                    //Fld46 = ca2.StdPay.ToString(),
                                    //Fld47 = ca2.GrossPay.ToString(),
                                    //Fld48 = ca2.DeductionPay.ToString(),
                                    Fld49 = ca2.NetPay.ToString(),
                                    Fld50 = ca2.NetPayInWords,
                                    //Fld51 = ca2.ProvisionalGratuity.ToString(),
                                    //Fld52 = ca2.PayslipNote,
                                    //Fld53 = ca2.PayslipRemark,
                                    //Fld54 = ca2.TotalITaxLiability.ToString(),
                                    //Fld55 = ca2.TotalITaxPaid.ToString(),
                                    //Fld56 = ca2.TotalITaxBalance.ToString(),
                                    //Fld57 = ca2.ITaxPerMonth.ToString(),
                                    //Fld58 = ca2.PaySlipLock.ToString(),
                                    Fld59 = ca2.PayslipLockDate.ToString(),
                                    Fld70 = ca2.BasicScale,
                                    Fld72 = UANNO
                                };
                                OGenericPayrollStatement.Add(oGenericField1001);

                                if (ca2.PaySlipDetailDedR != null && ca2.PaySlipDetailDedR.ToList().Count() != null)
                                {
                                    foreach (PaySlipDetailDedR item in ca2.PaySlipDetailDedR)
                                    {
                                        if (item.DedAmount != 0)
                                        {
                                            GenericField100 oGenericField100 = new GenericField100()
                                            {
                                                Fld1 = ca2.EmpCode,
                                                Fld21 = dt,
                                                Fld28 = ca2.Location,
                                                Fld60 = item.SalHeadDesc,
                                                Fld61 = item.StdSalAmount.ToString(),
                                                Fld62 = item.DedAmount.ToString(),

                                            };

                                            OGenericPayrollStatement.Add(oGenericField100);
                                        }

                                    }
                                }
                                if (ca2.PaySlipDetailEarnR != null)
                                {
                                    foreach (PaySlipDetailEarnR item in ca2.PaySlipDetailEarnR)
                                    {
                                        if (item.EarnAmount != 0)
                                        {
                                            GenericField100 oGenericField100 = new GenericField100()
                                            {
                                                Fld1 = ca2.EmpCode,
                                                Fld21 = dt,
                                                Fld28 = ca2.Location,
                                                Fld63 = item.SalHeadDesc,
                                                Fld64 = item.StdSalAmount.ToString(),
                                                Fld65 = item.EarnAmount.ToString(),
                                            };

                                            OGenericPayrollStatement.Add(oGenericField100);
                                        }
                                    }
                                }

                                if (ca2.PaySlipDetailLeaveR != null)
                                {

                                    foreach (PaySlipDetailLeaveR item in ca2.PaySlipDetailLeaveR)
                                    {
                                        GenericField100 oGenericField100 = new GenericField100()
                                        {
                                            Fld1 = ca2.EmpCode,
                                            Fld21 = dt,
                                            Fld28 = ca2.Location,
                                            Fld67 = item.LeaveHead,
                                            Fld68 = item.LeaveCloseBal.ToString(),
                                            Fld69 = item.LeaveUtilized.ToString(),
                                        };
                                        OGenericPayrollStatement.Add(oGenericField100);
                                    }
                                }
                            }

                            if (OGenericPayrollStatement.Count > 0 && OGenericPayrollStatement != null)
                            {
                                return OGenericPayrollStatement.Distinct().ToList();
                            }
                            else
                            {
                                return null;
                            }
                        }
                        break;
                ///New By Sudhir04042017

                //Gratuity Transaction
                case "GRATUITYT":
                    var OGratuityData = db.EmployeePayroll
                       .Include(e => e.Employee)
                       .Include(e => e.Employee.EmpName)
                       .Include(e => e.GratuityT)
                        //.Include(e => e.Employee.GeoStruct)
                        // .Include(e => e.Employee.GeoStruct.Location)
                        // .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //  .Include(e => e.Employee.GeoStruct.Department)
                        //  .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        //  .Include(e => e.Employee.PayStruct)
                        // .Include(e => e.Employee.PayStruct.Grade)
                        //    .Include(e => e.Employee.FuncStruct)
                        // .Include(e => e.Employee.FuncStruct.Job)

                        .Include(e => e.GratuityT.Select(t => t.GeoStruct.Location.LocationObj))
                        .Include(e => e.GratuityT.Select(t => t.GeoStruct.Department.DepartmentObj))
                        .Include(e => e.GratuityT.Select(t => t.FuncStruct.Job))
                        .Include(e => e.GratuityT.Select(t => t.FuncStruct.JobPosition))
                        .Include(e => e.GratuityT.Select(t => t.PayStruct.Grade))
                        .Include(e => e.GratuityT.Select(t => t.PayStruct.JobStatus))
                        .Include(e => e.Employee.ServiceBookDates)
                         .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();


                    if (OGratuityData == null || OGratuityData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OGratuityData)
                        {
                            if (ca.GratuityT == null || ca.GratuityT.Count() != 0)
                            {
                                var OGratuityDatails = ca.GratuityT.ToList();

                                if (OGratuityDatails != null && OGratuityDatails.Count() != 0)
                                {

                                    foreach (var ca1 in OGratuityDatails)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca1.ActualService == null ? "" : ca1.ActualService.ToString(),
                                            Fld2 = ca1.RoundedService == null ? "" : ca1.RoundedService.ToString(),
                                            Fld3 = ca1.Amount == null ? "" : ca1.Amount.ToString(),
                                            Fld4 = ca1.ProcessDate == null ? "" : ca1.ProcessDate.Value.ToShortDateString(),
                                            Fld5 = ca1.TotalLWP == null ? "" : ca1.TotalLWP.ToString(),


                                            Fld26 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                            Fld27 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                            Fld28 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                            //Fld29 = ca.Employee.GeoStruct.Location.LocationObj.Id == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.Id.ToString(),
                                            //Fld30 = ca.Employee.GeoStruct.Location.LocationObj.LocCode == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                                            //Fld31 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),

                                            //Fld32 = ca.Employee.GeoStruct.Department.DepartmentObj.Id == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString(),
                                            //Fld33 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString(),
                                            //Fld34 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString(),

                                            Fld29 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null ? ca.Employee.GeoStruct.Location.Id.ToString() : null,
                                            Fld30 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocCode : null,
                                            Fld31 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : null,

                                            Fld32 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString() : null,
                                            Fld33 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString() : null,
                                            Fld34 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,

                                            Fld35 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Id.ToString() : null,
                                            Fld36 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Code.ToString() : null,
                                            Fld37 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,


                                            //Fld35 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            //Fld36 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                                            //Fld37 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                                            //Fld38 = ca.Employee.FuncStruct.Job.Id == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                            //Fld39 = ca.Employee.FuncStruct.Job.Code == null ? "" : ca.Employee.FuncStruct.Job.Code.ToString(),
                                            //Fld40 = ca.Employee.FuncStruct.Job.Name == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),
                                            Fld38 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Id.ToString() : null,
                                            Fld39 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Code : null,
                                            Fld40 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : null,

                                            Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                            Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                            Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                            //Fld31 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                                            //Fld32 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                                            //Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            //Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                                            //Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                                            //Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                            //Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            //Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            //Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                            //Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                                            //Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                                            //Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                            //Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                                            //Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                                            //Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                            //Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                                            //Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                                            //Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                            //Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                                            //Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                                            //Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                            //Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                                            //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            //Fld40 = ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            ////Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            ////Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            ////Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}
                        }

                        return OGenericPayrollStatement;
                    }

                    break;

                //BonusCheckT




                //End

                case "PFECRR":

                    //case "ITTRANST":
                    var OPFECRData = db.PFECRSummaryR
                    .Include(e => e.PFECRR)
                        //.Include(e => e.Employee)
                        //.Include(e => e.Employee.EmpName)
                    .Include(e => e.PFECRR.Select(t => t.PFCalendar))


                    //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                        //.ToList();
                    .Where(e => mPayMonth.Contains(e.Wage_Month))
                    .ToList();

                    if (OPFECRData == null || OPFECRData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OPFECRData)
                        {
                            if (ca.PFECRR != null && ca.PFECRR.Count() != 0)
                            {
                                var OPFECRR = ca.PFECRR.ToList();
                                if (OPFECRR != null && OPFECRR.Count() != 0)
                                {
                                    foreach (var ca1 in OPFECRR)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca1.Establishment_ID,
                                            Fld2 = ca1.Establishment_Name,
                                            Fld3 = ca1.Wage_Month,
                                            Fld4 = ca1.Return_Month == null ? "" : ca1.Return_Month,
                                            Fld5 = ca1.Exemption_Status == null ? "" : ca1.Exemption_Status,
                                            Fld6 = ca1.Total_UANs == null ? "" : ca1.Total_UANs.ToString(),
                                            Fld7 = ca1.UAN == null ? "" : ca1.UAN.ToString(),
                                            Fld8 = ca1.UAN_Name == null ? "" : ca1.UAN_Name.ToString(),
                                            Fld9 = ca1.Gross_Wages == null ? "" : ca1.Gross_Wages.ToString(),
                                            Fld10 = ca1.EPF_Wages == null ? "" : ca1.EPF_Wages.ToString(),

                                            Fld11 = ca1.EDLI_Wages == null ? "" : ca1.EDLI_Wages.ToString(),
                                            Fld12 = ca1.EE_Share == null ? "" : ca1.EE_Share.ToString(),
                                            Fld13 = ca1.EE_VPF_Share == null ? "" : ca1.EE_VPF_Share.ToString(),
                                            Fld14 = ca1.EPS_Share == null ? "" : ca1.EPS_Share.ToString(),
                                            Fld15 = ca1.ER_Share == null ? "" : ca1.ER_Share.ToString(),
                                            Fld16 = ca1.NCP_Days == null ? "" : ca1.NCP_Days.ToString(),
                                            Fld17 = ca1.Refund_of_Advances == null ? "" : ca1.Refund_of_Advances.ToString(),
                                            Fld18 = ca1.Arrear_EPF_Wages == null ? "" : ca1.Arrear_EPF_Wages.ToString(),
                                            Fld19 = ca1.Arrear_EPS_Wages == null ? "" : ca1.Arrear_EPS_Wages.ToString(),
                                            Fld20 = ca1.Arrear_EE_Share == null ? "" : ca1.Arrear_EE_Share.ToString(),
                                            Fld21 = ca1.Arrear_EPS_Share == null ? "" : ca1.Arrear_EPS_Share.ToString(),
                                            Fld22 = ca1.Arrear_ER_Share == null ? "" : ca1.Arrear_ER_Share.ToString(),
                                            Fld23 = ca1.Father_Husband_Name == null ? "" : ca1.Father_Husband_Name.ToString(),
                                            Fld24 = ca1.Relationship_with_the_Member == null ? "" : ca1.Relationship_with_the_Member.ToString(),
                                            Fld25 = ca1.Date_of_Birth == null ? "" : ca1.Date_of_Birth.ToString(),
                                            Fld26 = ca1.Gender == null ? "" : ca1.Gender.ToString(),
                                            Fld27 = ca1.Date_of_Joining_EPF == null ? "" : ca1.Date_of_Joining_EPF.Value.ToShortDateString(),
                                            Fld28 = ca1.Date_of_Joining_EPS == null ? "" : ca1.Date_of_Joining_EPS.Value.ToShortDateString(),
                                            Fld29 = ca1.Date_of_Exit_from_EPF == null ? "" : ca1.Date_of_Exit_from_EPF.Value.ToShortDateString(),
                                            Fld30 = ca1.Date_of_Exit_from_EPS == null ? "" : ca1.Date_of_Exit_from_EPS.Value.ToShortDateString(),
                                            Fld31 = ca1.Reason_for_leaving == null ? "" : ca1.Reason_for_leaving.ToString()


                                            //Fld11 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Id.ToString(),
                                            //Fld12 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Code,
                                            //Fld13 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                            //Fld14 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.Id.ToString(),
                                            //Fld15 = ca1.GeoStruct.Location.LocationObj == null ? "" : ca1.GeoStruct.Location.LocationObj.LocCode,
                                            //Fld16 = ca1.GeoStruct.Location.LocationObj == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                                            //Fld17 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                                            //Fld18 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                                            //Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            //Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                                            //Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                                            //Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                            //Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            //Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            //Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                            //Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                                            //Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                                            //Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                            //Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                                            //Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                                            //Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                            //Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                                            //Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                                            //Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                            //Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                                            //Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                                            //Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                            //Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                                            //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            //Fld40 = ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            ////Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            ////Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            ////Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                else
                                {
                                    //return null;
                                }

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;


                //case "ITPROJECTION":

                //    //case "ITTRANST":
                //    var OITPROJECT = db.EmployeePayroll
                //        .Include(e => e.Employee)
                //        .Include(e => e.Employee.EmpName)
                //        .Include(e => e.ITProjection)
                //        .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                //        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                //         .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                //        .Include(e => e.Employee.PayStruct.Grade)
                //        .Include(e => e.Employee.FuncStruct.Job)
                //        .Include(e => e.Employee.ServiceBookDates)
                //        .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                //    .ToList();

                //    //.Include(e=>e.)



                //    if (OITPROJECT == null || OITPROJECT.Count() == 0)
                //    {
                //        return null;
                //    }
                //    else
                //    {
                //        foreach (var ca in OITPROJECT)
                //        {
                //            foreach (var ca1 in ca.ITProjection)
                //            {
                //                GenericField100 OGenericObjStatement = new GenericField100()
                //                //write data to generic class
                //                {

                //                    Fld1 = ca1.Id.ToString(),
                //                    Fld2 = ca1.FromPeriod.ToString(),
                //                    Fld3 = ca1.ToPeriod.ToString(),
                //                    Fld4 = (ca1.Tiltle == null ? "" : ca1.Tiltle.ToString()) + (ca1.ChapterName == null ? "" : ca1.ChapterName.ToString()).ToString(),
                //                    Fld5 = ca1.Form16Header == null ? "" : ca1.Form16Header,
                //                    Fld6 = ca1.Form24Header == null ? "" : ca1.Form24Header.ToString(),
                //                    Fld7 = ca1.SectionType == null ? "" : ca1.SectionType.ToString(),
                //                    Fld8 = ca1.Section == null ? "" : ca1.Section.ToString(),
                //                    Fld9 = ca1.ChapterName == null ? "" : ca1.ChapterName.ToString(),
                //                    Fld10 = ca1.SubChapter == null ? "" : ca1.SubChapter.ToString(),

                //                    Fld11 = ca1.SalayHead == null ? "" : ca1.SalayHead.ToString(),
                //                    Fld12 = ca1.ProjectedAmount == null ? "" : ca1.ProjectedAmount.ToString(),
                //                    Fld13 = ca1.ActualAmount == null ? "" : ca1.ActualAmount.ToString(),
                //                    Fld14 = ca1.ProjectedQualifyingAmount == null ? "" : ca1.ProjectedQualifyingAmount.ToString(),
                //                    Fld15 = ca1.ActualQualifyingAmount == null ? "" : ca1.ActualQualifyingAmount.ToString(),
                //                    Fld16 = ca1.QualifiedAmount == null ? "" : ca1.QualifiedAmount.ToString(),
                //                    Fld17 = ca1.TDSComponents == null ? "" : ca1.TDSComponents.ToString(),
                //                    Fld18 = ca1.ProjectionDate == null ? "" : ca1.ProjectionDate.ToString(),
                //                    Fld19 = ca1.IsLocked == null ? "" : ca1.IsLocked.ToString(),
                //                    Fld20 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                //                    Fld21 = ca1.PickupId == null ? "" : ca1.PickupId.ToString(),
                //                    Fld22 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToString(),



                //                    Fld26 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                //                    Fld27 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                //                    Fld28 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                //                    Fld29 = ca.Employee.GeoStruct.Location.LocationObj.Id == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.Id.ToString(),
                //                    Fld30 = ca.Employee.GeoStruct.Location.LocationObj.LocCode == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                //                    Fld31 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),

                //                    //Fld32 = ca.Employee.GeoStruct.Department.DepartmentObj.Id == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString(),
                //                    //Fld33 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString(),
                //                    //Fld34 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString(),

                //                    Fld35 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                //                    Fld36 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                //                    Fld37 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                //                    //Fld38 = ca.Employee.FuncStruct.Job.Id == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                //                    //Fld39 = ca.Employee.FuncStruct.Job.Code == null ? "" : ca.Employee.FuncStruct.Job.Code.ToString(),
                //                    //Fld40 = ca.Employee.FuncStruct.Job.Name == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),

                //                    Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.ToString(),
                //                    Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.ToString(),
                //                    Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.ToString(),
                //                    //Fld31 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                //                    //Fld32 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                //                    //Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                //                    //Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                //                    //Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                //                    //Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                //                    //Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                //                    //Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                //                    //Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                //                    //Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                //                    //Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                //                    //Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                //                    //Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                //                    //Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                //                    //Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                //                    //Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                //                    //Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                //                    //Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                //                    //Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                //                    //Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                //                    //Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                //                    //Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                //                    //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                //                    //Fld40 = ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                //                    ////Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                //                    ////Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                //                    ////Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,



                //                };
                //                OGenericPayrollStatement.Add(OGenericObjStatement);
                //            }
                //        }
                //        //}
                //        //else
                //        //{
                //        //    //return null;
                //        //}

                //    }
                //    //else
                //    //{
                //    //    return null;
                //    //}

                //    //}
                //    return OGenericPayrollStatement;
                //    //}
                //    break;


                ///PF ECR Challan//

                case "PFECRCHALLAN":

                    //case "ITTRANST":
                    var OPFECRChallanData = db.PFECRSummaryR

                    .Include(e => e.PFECRR)
                        //.Include(e => e.Employee)
                        //.Include(e => e.Employee.EmpName)
                    .Include(e => e.PFECRR.Select(t => t.PFCalendar))


                    //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                        //.ToList();
                    .Where(e => mPayMonth.Contains(e.Wage_Month))
                    .ToList();

                    if (OPFECRChallanData == null || OPFECRChallanData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OPFECRChallanData)
                        {

                            GenericField100 OGenericObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Establishment_ID,
                                Fld2 = ca.Establishment_Name,
                                Fld3 = ca.Wage_Month,
                                Fld4 = ca.Return_Month == null ? "" : ca.Return_Month,
                                Fld5 = ca.Exemption_Status == null ? "" : ca.Exemption_Status,
                                Fld6 = ca.Total_UANs == null ? "" : ca.Total_UANs.ToString(),
                                Fld7 = ca.Gross_Wages == null ? "" : ca.Gross_Wages.ToString(),
                                Fld8 = ca.EPF_Wages == null ? "" : ca.EPF_Wages.ToString(),
                                Fld9 = ca.EPS_Wages == null ? "" : ca.EPS_Wages.ToString(),
                                //Fld10 = ca.EPF_Wages == null ? "" : ca.EPF_Wages.ToString(),

                                Fld11 = ca.EDLI_Wages == null ? "" : ca.EDLI_Wages.ToString(),
                                Fld12 = ca.EE_Share == null ? "" : ca.EE_Share.ToString(),
                                Fld13 = ca.EE_VPF_Share == null ? "" : ca.EE_VPF_Share.ToString(),
                                Fld14 = ca.EPS_Share == null ? "" : ca.EPS_Share.ToString(),
                                Fld15 = ca.ER_Share == null ? "" : ca.ER_Share.ToString(),
                                Fld16 = ca.NCP_Days == null ? "" : ca.NCP_Days.ToString(),
                                Fld17 = ca.Refund_of_Advances == null ? "" : ca.Refund_of_Advances.ToString(),
                                Fld18 = ca.Arrear_EPF_Wages == null ? "" : ca.Arrear_EPF_Wages.ToString(),
                                Fld19 = ca.Arrear_EPS_Wages == null ? "" : ca.Arrear_EPS_Wages.ToString(),
                                Fld20 = ca.Arrear_EE_Share == null ? "" : ca.Arrear_EE_Share.ToString(),
                                Fld21 = ca.Arrear_EPS_Share == null ? "" : ca.Arrear_EPS_Share.ToString(),
                                Fld22 = ca.Arrear_ER_Share == null ? "" : ca.Arrear_ER_Share.ToString(),
                                Fld23 = ca.Administrative_Charges_AC2 == null ? "" : ca.Administrative_Charges_AC2.ToString(),
                                Fld24 = ca.Administrative_Charges_AC22 == null ? "" : ca.Administrative_Charges_AC22.ToString(),
                                Fld25 = ca.Inspection_Charges_AC2 == null ? "" : ca.Inspection_Charges_AC2.ToString(),
                                Fld26 = ca.Inspection_Charges_AC22 == null ? "" : ca.Inspection_Charges_AC22.ToString(),
                                Fld27 = ca.Total_Employees == null ? "" : ca.Total_Employees.ToString(),
                                Fld28 = ca.Total_Employees_Excluded == null ? "" : ca.Total_Employees_Excluded.ToString(),
                                Fld29 = ca.Total_Gross_Wages_Excluded == null ? "" : ca.Total_Gross_Wages_Excluded.ToString(),
                                Fld30 = ca.EDLI_Contribution_AC21 == null ? "" : ca.EDLI_Contribution_AC21.ToString()
                                //Fld31 = ca.Reason_for_leaving == null ? "" : ca.Reason_for_leaving.ToString()
                            };
                            OGenericPayrollStatement.Add(OGenericObjStatement);
                            //            };
                            //            OGenericPayrollStatement.Add(OGenericObjStatement);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        //return null;
                            //    }

                            //}
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "SALATTENDANCE":
                    var OSalAttendanceData = db.EmployeePayroll

                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.SalAttendance)
                    .Include(e => e.SalAttendance.Select(t => t.FuncStruct.Job))

                    .Include(e => e.SalAttendance.Select(t => t.FuncStruct.JobPosition))
                   .Include(e => e.SalAttendance.Select(t => t.PayStruct.Grade))

                    .Include(e => e.SalAttendance.Select(t => t.PayStruct.Level))

                    .Include(e => e.SalAttendance.Select(t => t.PayStruct.JobStatus))
                    .Include(e => e.SalAttendance.Select(t => t.PayStruct.JobStatus.EmpStatus))
                    .Include(e => e.SalAttendance.Select(t => t.PayStruct.JobStatus.EmpActingStatus))

                    .Include(e => e.SalAttendance.Select(t => t.GeoStruct.Division))

                    .Include(e => e.SalAttendance.Select(t => t.GeoStruct.Location))
                    .Include(e => e.SalAttendance.Select(t => t.GeoStruct.Location.LocationObj))

                    .Include(e => e.SalAttendance.Select(t => t.GeoStruct.Department))

                    .Include(e => e.SalAttendance.Select(t => t.GeoStruct.Group))

                    .Include(e => e.SalAttendance.Select(t => t.GeoStruct.Unit))

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();


                    if (OSalAttendanceData == null || OSalAttendanceData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OSalAttendanceData)
                        {
                            if (ca.SalAttendance != null && ca.SalAttendance.Count() != 0)
                            {
                                var OSalAttend = ca.SalAttendance.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                if (OSalAttend != null && OSalAttend.Count() != 0)
                                {
                                    foreach (var ca1 in OSalAttend)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.PayMonth == null ? "" : ca1.PayMonth,
                                            Fld5 = ca1.PaybleDays == null ? "" : ca1.PaybleDays.ToString(),
                                            Fld6 = ca1.LWPDays == null ? "" : ca1.LWPDays.ToString(),
                                            Fld7 = ca1.MonthDays == null ? "" : ca1.MonthDays.ToString(),
                                            Fld8 = ca1.OTHours == null ? "" : ca1.OTHours.ToString(),
                                            Fld9 = ca1.PaybleHours == null ? "" : ca1.PaybleHours.ToString(),
                                            Fld10 = ca1.WeeklyOff_Cnt == null ? "" : ca1.WeeklyOff_Cnt.ToString(),

                                            //Fld11 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Id.ToString(),
                                            //Fld12 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Code,
                                            //Fld13 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                            //Fld14 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.Id.ToString(),
                                            //Fld15 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocCode,
                                            //Fld16 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                                            //Fld17 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                                            //Fld18 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                                            //Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            //Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                                            //Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                                            //Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                            //Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            //Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            //Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                            //Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                                            //Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                                            //Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                            //Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                                            //Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                                            //Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                            //Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                                            //Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                                            //Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                            //Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                                            //Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                                            //Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                            //Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                                            //Fld39 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            //Fld40 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            ////Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            ////Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            ////Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,
                                            //Fld11 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Id.ToString() : null,
                                            //Fld12 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Code : null,
                                            //Fld13 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Name : null,
                                            //Fld14 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null ? ca1.GeoStruct.Location.Id.ToString() : null,

                                            Fld15 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocCode : null,
                                            Fld16 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocDesc : null,
                                            Fld17 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null ? ca1.GeoStruct.Department.Id.ToString() : null,
                                            Fld18 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptCode : null,
                                            Fld19 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptDesc : null,

                                            //Fld20 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Id.ToString() : null,
                                            //Fld21 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Code : null,
                                            //Fld22 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Name : null,

                                            //Fld23 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Id.ToString() : null,
                                            //Fld24 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Code : null,
                                            //Fld25 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Name : null,

                                            Fld26 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Id.ToString() : null,
                                            Fld27 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Code : null,
                                            Fld28 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Name : null,
                                            Fld29 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.Id.ToString() : null,
                                            Fld30 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionCode : null,
                                            Fld31 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionDesc : null,

                                            Fld32 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Id.ToString() : null,
                                            Fld33 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Code : null,
                                            Fld34 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Name : null,

                                            //Fld35 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Id.ToString() : null,
                                            //Fld36 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Code : null,
                                            //Fld37 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Name : null,

                                            Fld38 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null ? ca1.PayStruct.JobStatus.Id.ToString() : null,
                                            Fld39 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpStatus != null ? ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                            //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld40 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpActingStatus != null ? ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,

                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "SALARYARREART":
                    var OSalArrearData = db.EmployeePayroll
                    .Include(e => e.Employee)
                        //.Include(e => e.Employee.EmpCode)
                    .Include(e => e.Employee.EmpName)

                    .Include(r => r.SalaryArrearT)
                    .Include(r => r.SalaryArrearT.Select(t => t.ArrearType))

                    .Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.Job))
                        //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.Job.Code))
                    .Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.JobPosition))
                        //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.JobPosition.Id))
                        //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.JobPosition.JobPositionCode))
                        // .Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.JobPosition.JobPositionDesc))
                    .Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Grade))
                        //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Grade.Code))
                        //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Grade.Name))
                    .Include(r => r.SalaryArrearT.Select(t => t.PayStruct.JobStatus))
                    .Include(r => r.SalaryArrearT.Select(t => t.PayStruct.JobStatus.EmpStatus))
                    .Include(r => r.SalaryArrearT.Select(t => t.PayStruct.JobStatus.EmpActingStatus))

                    .Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Level))
                        //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Level.Code))
                        //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Level.Name))
                    .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Division))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Division.Code))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Division.Name))
                    .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Location))
                    .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Location.LocationObj))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Location.LocationObj.LocDesc))
                    .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Department))
                    .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Department.DepartmentObj))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Department.DepartmentObj.DeptDesc))
                    .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Group))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Group.Code))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Group.Name))
                        // .Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Unit))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Unit.Code))
                        //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Unit.Name))

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OSalArrearData == null || OSalArrearData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OSalArrearData)
                        {
                            if (ca.SalaryArrearT != null && ca.SalaryArrearT.Count() != 0)
                            {
                                var OSalArrear = ca.SalaryArrearT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                if (OSalArrear != null && OSalArrear.Count() != 0)
                                {
                                    foreach (var ca1 in OSalArrear)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee == null ? "" : ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee == null ? "" : ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                            Fld3 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.PayMonth == null ? "" : ca1.PayMonth,
                                            Fld5 = ca1.ArrearType == null ? "" : ca1.ArrearType.LookupVal == null ? "" : ca1.ArrearType.LookupVal.ToUpper(),
                                            Fld6 = ca1.FromDate == null ? "" : ca1.FromDate.ToString(),
                                            Fld7 = ca1.ToDate == null ? "" : ca1.ToDate.ToString(),
                                            Fld8 = ca1.TotalDays == null ? "" : ca1.TotalDays.ToString(),
                                            Fld9 = ca1.IsRecovery == null ? "" : ca1.IsRecovery.ToString(),
                                            Fld10 = ca1.IsAuto == null ? "" : ca1.IsAuto.ToString(),
                                            Fld11 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Id.ToString(),
                                            Fld12 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Code,
                                            Fld13 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                            Fld14 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.Id.ToString(),
                                            Fld15 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocCode,
                                            Fld16 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld17 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                                            Fld18 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                                            Fld19 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld20 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                                            Fld21 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                                            Fld22 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                            Fld23 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            Fld24 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            Fld25 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                            Fld26 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                                            Fld27 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                                            Fld28 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                            Fld29 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                                            Fld30 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                                            Fld31 = ca1.FuncStruct == null ? "" : ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                            Fld32 = ca1.PayStruct == null ? "" : ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                                            Fld33 = ca1.PayStruct == null ? "" : ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                                            Fld34 = ca1.PayStruct == null ? "" : ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                            Fld35 = ca1.PayStruct == null ? "" : ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                                            Fld36 = ca1.PayStruct == null ? "" : ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                                            Fld37 = ca1.PayStruct == null ? "" : ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                            Fld38 = ca1.PayStruct == null ? "" : ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                                            Fld39 = ca1.PayStruct == null ? "" : ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld40 = ca1.PayStruct == null ? "" : ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            Fld41 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            Fld42 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            Fld43 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                else
                                {
                                    //return null;
                                }

                            }
                            else
                            {
                                //return null;
                            }

                        }
                        return OGenericPayrollStatement;
                    }
                    break;


                case "FUNCTATTENDANCET":
                    var OFuncAttData = db.EmployeePayroll

                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)

                    .Include(r => r.FunctAttendanceT)
                    .Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))


                    .Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct.Job))

                    .Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct.JobPosition))

                    .Include(r => r.FunctAttendanceT.Select(t => t.PayStruct.Grade))

                    .Include(r => r.FunctAttendanceT.Select(t => t.PayStruct.JobStatus.EmpStatus))
                    .Include(r => r.FunctAttendanceT.Select(t => t.PayStruct.JobStatus.EmpActingStatus))

                    .Include(r => r.FunctAttendanceT.Select(t => t.PayStruct.Level))

                    .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Division))

                    .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Location))
                     .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Location.LocationObj))

                    .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Department))
                    .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Department.DepartmentObj))
                    .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Group))

                    .Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct.Unit))


                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OFuncAttData == null || OFuncAttData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OFuncAttData)
                        {
                            if (ca.FunctAttendanceT != null && ca.FunctAttendanceT.Count() != 0)
                            {
                                var OOFuncAtt = ca.FunctAttendanceT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                if (OOFuncAtt != null && OOFuncAtt.Count() != 0)
                                {
                                    foreach (var ca1 in OOFuncAtt)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                            Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                            Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                            Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                            Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                            Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                            Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                            Fld10 = ca1.FromDate != null ? ca1.FromDate.ToString() : null,
                                            Fld44 = ca1.ToDate != null ? ca1.ToDate.ToString() : null,
                                            Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                            Fld46 = ca1.PayProcessGroup != null ? ca1.PayProcessGroup.Name : null,
                                            Fld47 = ca1.Reason != null ? ca1.Reason : null,

                                            Fld11 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Id.ToString() : null,
                                            Fld12 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Code : null,
                                            Fld13 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Name : null,
                                            Fld14 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null ? ca1.GeoStruct.Location.Id.ToString() : null,
                                            Fld15 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocCode : null,
                                            Fld16 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocDesc : null,
                                            Fld17 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null ? ca1.GeoStruct.Department.Id.ToString() : null,
                                            Fld18 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptCode : null,
                                            Fld19 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                            Fld20 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Id.ToString() : null,
                                            Fld21 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Code : null,
                                            Fld22 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Name : null,
                                            Fld23 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Id.ToString() : null,
                                            Fld24 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Code : null,
                                            Fld25 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Name : null,

                                            Fld26 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Id.ToString() : null,
                                            Fld27 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Code : null,
                                            Fld28 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Name : null,

                                            Fld29 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.Id.ToString() : null,
                                            Fld30 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionCode : null,
                                            Fld31 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionDesc : null,

                                            Fld32 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Id.ToString() : null,
                                            Fld33 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Code : null,
                                            Fld34 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Name : null,

                                            Fld35 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Id.ToString() : null,
                                            Fld36 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Code : null,
                                            Fld37 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Name : null,

                                            Fld38 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null ? ca1.PayStruct.JobStatus.Id.ToString() : null,
                                            Fld39 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpStatus != null ? ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                            Fld40 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpActingStatus != null ? ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,
                                            //Fld41 = ca1.GeoStruct.Unit != null ?     ca1.GeoStruct.Unit.Id.ToString():null,,
                                            //Fld42 = ca1.GeoStruct.Unit != null ?     ca1.GeoStruct.Unit.Code,
                                            //Fld43 = ca1.GeoStruct.Unit != null ?     ca1.GeoStruct.Unit.Name,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                else
                                {
                                    //return null;
                                }

                            }
                            else
                            {
                                //return null;
                            }

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "INSURANCEDETAILST":
                    var OInsuranceData = db.EmployeePayroll

                    .Include(e => e.Employee)
                        //.Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)

                    .Include(r => r.InsuranceDetailsT)
                    .Include(r => r.InsuranceDetailsT.Select(t => t.OperationStatus))
                    .Include(r => r.InsuranceDetailsT.Select(t => t.Frequency))
                    .Include(r => r.InsuranceDetailsT.Select(t => t.InsuranceProduct))


                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OInsuranceData == null || OInsuranceData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OInsuranceData)
                        {
                            if (ca.InsuranceDetailsT != null && ca.InsuranceDetailsT.Count() != 0)
                            {
                                var m = mPayMonth.Count();
                                var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                //var OInsurData = ca.InsuranceDetailsT.Where(e => e.FromDate >= mstart && e.ToDate >= mend).ToList();
                                var OInsurData = ca.InsuranceDetailsT.Where(e => e.FromDate >= mstart && e.ToDate >= mend && e.OperationStatus.LookupVal.ToUpper() == "ACTIVE").ToList();

                                if (OInsurData != null && OInsurData.Count() != 0)
                                {
                                    foreach (var ca1 in OInsurData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee == null ? "" : ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                            Fld3 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.InsuranceProduct == null ? "" : ca1.InsuranceProduct.InsuranceProductDesc == null ? "" : ca1.InsuranceProduct.InsuranceProductDesc,
                                            Fld5 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                            Fld6 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                            Fld7 = ca1.Frequency == null ? "" : ca1.Frequency.LookupVal == null ? "" : ca1.Frequency.LookupVal.ToUpper(),
                                            Fld8 = ca1.OperationStatus == null ? "" : ca1.OperationStatus.LookupVal == null ? "" : ca1.OperationStatus.LookupVal.ToUpper(),
                                            Fld9 = ca1.PolicyNo == null ? "" : ca1.PolicyNo.ToString(),
                                            Fld10 = ca1.Premium == null ? "" : ca1.Premium.ToString(),

                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                else
                                {
                                    //return null;
                                }

                            }
                            else
                            {
                                //return null;
                            }

                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                case "ITTRANST":
                    var OITTransData = db.EmployeePayroll
                    .Include(e => e.ITaxTransT)
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(r => r.ITaxTransT.Select(t => t.FuncStruct))
                    .Include(r => r.ITaxTransT.Select(t => t.FuncStruct.Job))
                    .Include(r => r.ITaxTransT.Select(t => t.FuncStruct.JobPosition))
                    .Include(r => r.ITaxTransT.Select(t => t.PayStruct.Grade))
                    .Include(r => r.ITaxTransT.Select(t => t.PayStruct.JobStatus))

                    .Include(r => r.ITaxTransT.Select(t => t.PayStruct))
                    .Include(r => r.ITaxTransT.Select(t => t.PayStruct.Level))

                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct))
                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct.Division))
                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct.Location))
                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct.Location.LocationObj))
                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct.Department))
                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct.Group))
                    .Include(r => r.ITaxTransT.Select(t => t.GeoStruct.Unit))

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OITTransData == null || OITTransData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OITTransData)
                        {
                            if (ca.ITaxTransT != null && ca.ITaxTransT.Count() != 0)
                            {
                                var OITTrans = ca.ITaxTransT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id != null ? ca.Employee.Id.ToString() : null,
                                            Fld2 = ca.Employee.EmpCode != null ? ca.Employee.EmpCode : null,
                                            Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                            Fld4 = ca1.Id != null ? ca1.Id.ToString() : null,
                                            Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                            Fld6 = ca1.TaxOnIncome != null ? ca1.TaxOnIncome.ToString() : null,
                                            Fld7 = ca1.EduCess != null ? ca1.EduCess.ToString() : null,
                                            Fld8 = ca1.Surcharge != null ? ca1.Surcharge.ToString() : null,
                                            Fld9 = ca1.TaxPaid != null ? ca1.TaxPaid.ToString() : null,
                                            Fld10 = ca1.Mode != null ? ca1.Mode : null,

                                            Fld11 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Id.ToString() : null,
                                            Fld12 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Code : null,
                                            Fld13 = ca1.GeoStruct != null && ca1.GeoStruct.Division != null ? ca1.GeoStruct.Division.Name : null,

                                            Fld14 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null ? ca1.GeoStruct.Location.Id.ToString() : null,
                                            Fld15 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocCode : null,
                                            Fld16 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocDesc : null,

                                            Fld17 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null ? ca1.GeoStruct.Department.Id.ToString() : null,
                                            Fld18 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptCode : null,
                                            Fld19 = ca1.GeoStruct != null && ca1.GeoStruct.Department != null && ca1.GeoStruct.Department.DepartmentObj != null ? ca1.GeoStruct.Department.DepartmentObj.DeptDesc : null,

                                            Fld20 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Id.ToString() : null,
                                            Fld21 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Code : null,
                                            Fld22 = ca1.GeoStruct != null && ca1.GeoStruct.Group != null ? ca1.GeoStruct.Group.Name : null,

                                            Fld23 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Id.ToString() : null,
                                            Fld24 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Code : null,
                                            Fld25 = ca1.GeoStruct != null && ca1.GeoStruct.Unit != null ? ca1.GeoStruct.Unit.Name : null,

                                            Fld26 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Id.ToString() : null,
                                            Fld27 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Code : null,
                                            Fld28 = ca1.FuncStruct != null && ca1.FuncStruct.Job != null ? ca1.FuncStruct.Job.Name : null,

                                            Fld29 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.Id.ToString() : null,
                                            Fld30 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionCode : null,
                                            Fld31 = ca1.FuncStruct != null && ca1.FuncStruct.JobPosition != null ? ca1.FuncStruct.JobPosition.JobPositionDesc : null,

                                            Fld32 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Id.ToString() : null,
                                            Fld33 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Code : null,
                                            Fld34 = ca1.PayStruct != null && ca1.PayStruct.Grade != null ? ca1.PayStruct.Grade.Name : null,

                                            Fld35 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Id.ToString() : null,
                                            Fld36 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Code : null,
                                            Fld37 = ca1.PayStruct != null && ca1.PayStruct.Level != null ? ca1.PayStruct.Level.Name : null,

                                            Fld38 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null ? ca1.PayStruct.JobStatus.Id.ToString() : null,
                                            Fld39 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpStatus != null ? ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                            Fld40 = ca1.PayStruct != null && ca1.PayStruct.JobStatus != null && ca1.PayStruct.JobStatus.EmpActingStatus != null ? ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,
                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                else
                                {
                                    //return null;
                                }

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                //case "INVESTMENTPAYMENT":
                //    var OITInvestmentPaymentData = db.EmployeePayroll
                //    .Include(e => e.ITInvestmentPayment)
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear.FullDetails))
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.ITInvestmentName))
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead.Id))
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead.Code))
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead.Name))
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                //    .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                //    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                //    .ToList();

                //    if (OITInvestmentPaymentData != null || OITInvestmentPaymentData.Count() != 0)
                //    {
                //        return null;
                //    }
                //    else
                //    {
                //        foreach (var ca in OITInvestmentPaymentData)
                //        {
                //            if (ca.ITInvestmentPayment != null && ca.ITInvestmentPayment.Count() != 0)
                //            {
                //                var m = mPayMonth.Count();
                //                var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                //                var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                //                var OInvestData = ca.ITInvestmentPayment.Where(e => e.FinancialYear.FromDate.Value >= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                //                if (OInvestData != null && OInvestData.Count() != 0)
                //                {
                //                    foreach (var ca1 in OInvestData)
                //                    {
                //                        GenericField100 OGenericObjStatement = new GenericField100()
                //                        //write data to generic class
                //                        {
                //                            Fld1 = ca.Employee.Id.ToString(),
                //                            Fld2 = ca.Employee.EmpCode,
                //                            Fld3 = ca.Employee.EmpName.FullNameFML,
                //                            Fld4 = ca1.Id.ToString(),
                //                            Fld5 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.FromDate.Value.ToShortDateString(),
                //                            Fld6 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToDate.Value.ToShortDateString(),
                //                            Fld7 = ca1.ITSection.ITSectionList.LookupVal.ToUpper(),
                //                            Fld8 = ca1.ITSection.ITSectionListType.LookupVal.ToUpper(),
                //                            Fld9 = ca1.ITInvestment.ITInvestmentName,
                //                            Fld10 = ca1.ITInvestment.SalaryHead == null ? "" : ca1.ITInvestment.SalaryHead.Id.ToString(),
                //                            Fld11 = ca1.ITInvestment.SalaryHead == null ? "" : ca1.ITInvestment.SalaryHead.Code,
                //                            Fld12 = ca1.ITInvestment.SalaryHead == null ? "" : ca1.ITInvestment.SalaryHead.Name,
                //                            Fld13 = ca1.ActualInvestment.ToString(),
                //                            Fld14 = ca1.DeclaredInvestment.ToString(),
                //                            Fld15 = ca1.Narration,


                //                        };
                //                        OGenericPayrollStatement.Add(OGenericObjStatement);
                //                    }
                //                }
                //                else
                //                {
                //                    return null;
                //                }

                //            }
                //            else
                //            {
                //                return null;
                //            }

                //        }
                //        return OGenericPayrollStatement;
                //    }
                //    break;

                case "INVESTMENTPAYMENT":
                    var OITInvestmentPaymentData = db.EmployeePayroll
                     .Include(e => e.Employee)
                     .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITInvestmentPayment)
                    .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                    .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                    .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))
                    .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                    .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OITInvestmentPaymentData == null || OITInvestmentPaymentData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OITInvestmentPaymentData)
                        {
                            if (ca.ITInvestmentPayment != null && ca.ITInvestmentPayment.Count() != 0)
                            {
                                var m = mPayMonth.Count();
                                var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OInvestData = ca.ITInvestmentPayment.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                if (OInvestData != null && OInvestData.Count() != 0)
                                {
                                    foreach (var ca1 in OInvestData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            //Fld1 = ca.Employee.Id.ToString(),
                                            Fld1 = ca.Employee != null ? ca.Employee.Id.ToString() : null,
                                            Fld2 = ca.Employee != null ? ca.Employee.EmpCode : null,
                                            Fld3 = ca.Employee != null && ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                            Fld4 = ca1.Id != null ? ca1.Id.ToString() : null,
                                            //Fld5 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.FromDate.ToString(),
                                            Fld5 = ca1.FinancialYear != null ? ca1.FinancialYear.FromDate.Value.ToShortDateString() : null,
                                            Fld6 = ca1.FinancialYear != null ? ca1.FinancialYear.FromDate.Value.ToShortDateString() : null,
                                            Fld7 = ca1.ITSection != null && ca1.ITSection.ITSectionList != null ? ca1.ITSection.ITSectionList.LookupVal.ToUpper() : null,
                                            Fld8 = ca1.ITSection != null && ca1.ITSection.ITSectionList != null ? ca1.ITSection.ITSectionListType.LookupVal.ToUpper() : null,
                                            Fld9 = ca1.ITInvestment != null && ca1.ITInvestment.ITInvestmentName != null ? ca1.ITInvestment.ITInvestmentName : null,
                                            //Fld10 = caITInvestment.SalaryHead == null ? "" : ca1.ITInvestment.SalaryHead.Id.ToString(),
                                            Fld10 = ca1.ITInvestment != null && ca1.ITInvestment.SalaryHead != null ? ca1.ITInvestment.SalaryHead.Id.ToString() : null,
                                            Fld11 = ca1.ITInvestment != null && ca1.ITInvestment.SalaryHead != null ? ca1.ITInvestment.SalaryHead.Code : null,
                                            Fld12 = ca1.ITInvestment != null && ca1.ITInvestment.SalaryHead != null ? ca1.ITInvestment.SalaryHead.Name : null,
                                            //Fld11 = ca1.ITInvestment.SalaryHead == null ? "" : ca1.ITInvestment.SalaryHead.Code,
                                            //Fld12 = ca1.ITInvestment.SalaryHead == null ? "" : ca1.ITInvestment.SalaryHead.Name,
                                            Fld13 = ca1.ActualInvestment != null ? ca1.ActualInvestment.ToString() : null,
                                            Fld14 = ca1.DeclaredInvestment != null ? ca1.DeclaredInvestment.ToString() : null,
                                            Fld15 = ca1.Narration != null ? ca1.Narration : null,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        if (OGenericPayrollStatement.Count == 0)
                        {
                            return null;
                        }
                        else
                        {

                            return OGenericPayrollStatement;
                        }
                    }
                    break;

                case "ITSECTION10PAYMENT":
                    var OITSection10PaymentData = db.EmployeePayroll
                         .Include(e => e.Employee)
                     .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITSection10Payment)
                    .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear))
                    .Include(e => e.ITSection10Payment.Select(r => r.ITSection10))
                    .Include(e => e.ITSection10Payment.Select(r => r.ITSection10.Itsection10salhead.Select(t => t.SalHead)))
                    .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSectionList))
                    .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSectionListType))
                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OITSection10PaymentData == null || OITSection10PaymentData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OITSection10PaymentData)
                        {
                            if (ca.ITSection10Payment != null && ca.ITSection10Payment.Count() != 0)
                            {
                                var m = mPayMonth.Count();
                                var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OInvestData = ca.ITSection10Payment.Where(e => e.FinancialYear.FromDate.Value >= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                if (OInvestData != null && OInvestData.Count() != 0)
                                {
                                    foreach (var ca1 in OInvestData)
                                    {
                                        if (ca1.ITSection10.Itsection10salhead == null || ca1.ITSection10.Itsection10salhead.Count() == 0)
                                        {
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = ca.Employee.Id.ToString(),
                                                Fld2 = ca.Employee.EmpCode,
                                                Fld3 = ca.Employee.EmpName.FullNameFML,
                                                Fld4 = ca1.Id.ToString(),
                                                Fld5 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.FromDate.Value.ToShortDateString(),
                                                Fld6 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToDate.Value.ToShortDateString(),
                                                Fld7 = ca1.ITSection.ITSectionList == null ? "" : ca1.ITSection.ITSectionList.LookupVal.ToUpper(),
                                                Fld8 = ca1.ITSection.ITSectionListType == null ? "" : ca1.ITSection.ITSectionListType.LookupVal.ToUpper(),
                                                Fld9 = "",
                                                Fld10 = "",
                                                Fld11 = "",
                                                Fld12 = ca1.ActualInvestment.ToString(),
                                                Fld13 = ca1.DeclaredInvestment.ToString(),
                                                Fld14 = ca1.InvestmentDate.ToString(),
                                                Fld15 = ca1.Narration,


                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }
                                        else
                                        {
                                            foreach (var ca2 in ca1.ITSection10.Itsection10salhead)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode,
                                                    Fld3 = ca.Employee.EmpName.FullNameFML,
                                                    Fld4 = ca1.Id.ToString(),
                                                    Fld5 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.FromDate.Value.ToShortDateString(),
                                                    Fld6 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToDate.Value.ToShortDateString(),
                                                    Fld7 = ca1.ITSection.ITSectionList == null ? "" : ca1.ITSection.ITSectionList.LookupVal.ToUpper(),
                                                    Fld8 = ca1.ITSection.ITSectionListType == null ? "" : ca1.ITSection.ITSectionListType.LookupVal.ToUpper(),
                                                    Fld9 = ca2.SalHead == null ? "" : ca2.SalHead.Id.ToString(),
                                                    Fld10 = ca2.SalHead == null ? "" : ca2.SalHead.Code,
                                                    Fld11 = ca2.SalHead == null ? "" : ca2.SalHead.Name,
                                                    Fld12 = ca1.ActualInvestment.ToString(),
                                                    Fld13 = ca1.DeclaredInvestment.ToString(),
                                                    Fld14 = ca1.InvestmentDate.ToString(),
                                                    Fld15 = ca1.Narration,
                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }


                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "ITSECTION24PAYMENT":
                    var OITSection24PaymentData = db.EmployeePayroll
                         .Include(e => e.Employee)
                     .Include(e => e.Employee.EmpName)
                    .Include(e => e.ITSection24Payment)
                    .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                    .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                   .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionList))
                    .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))
                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OITSection24PaymentData == null || OITSection24PaymentData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OITSection24PaymentData)
                        {
                            if (ca.ITSection24Payment != null && ca.ITSection24Payment.Count() != 0)
                            {
                                var m = mPayMonth.Count();
                                var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OInvestData = ca.ITSection24Payment.Where(e => e.FinancialYear.FromDate.Value >= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                if (OInvestData != null && OInvestData.Count() != 0)
                                {
                                    foreach (var ca1 in OInvestData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.Id.ToString(),
                                            Fld5 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.FromDate.Value.ToShortDateString(),
                                            Fld6 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToDate.Value.ToShortDateString(),
                                            Fld7 = ca1.ITSection.ITSectionList.LookupVal.ToUpper(),
                                            Fld8 = ca1.ITSection.ITSectionListType.LookupVal.ToUpper(),
                                            Fld9 = ca1.PaymentName,
                                            Fld10 = ca1.LoanAdvanceHead == null ? "" : ca1.LoanAdvanceHead.Id.ToString(),
                                            Fld11 = ca1.LoanAdvanceHead == null ? "" : ca1.LoanAdvanceHead.Code,
                                            Fld12 = ca1.LoanAdvanceHead == null ? "" : ca1.LoanAdvanceHead.Name,
                                            Fld13 = ca1.ActualInterest.ToString(),
                                            Fld14 = ca1.DeclaredInterest.ToString(),
                                            Fld15 = ca1.InvestmentDate.Value.ToShortDateString(),
                                            Fld16 = ca1.Narration,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "LOANADVREQUEST":
                    var OLoanAdvRequestData = db.EmployeePayroll
                    .Include(e => e.LoanAdvRequest)

                    .Include(e => e.Employee.EmpName)

                    .Include(r => r.LoanAdvRequest.Select(t => t.LoanAccBranch))
                    .Include(r => r.LoanAdvRequest.Select(t => t.LoanAdvanceHead))


                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OLoanAdvRequestData == null || OLoanAdvRequestData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLoanAdvRequestData)
                        {
                            if (ca.LoanAdvRequest != null && ca.LoanAdvRequest.Count() != 0)
                            {
                                var OITTrans = ca.LoanAdvRequest.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.Id == null ? "" : ca1.Id.ToString(),
                                            Fld5 = ca1.RequisitionDate == null ? "" : ca1.RequisitionDate.Value.ToShortDateString(),
                                            Fld6 = ca1.EnforcementDate == null ? "" : ca1.EnforcementDate.Value.ToShortDateString(),
                                            Fld7 = ca1.LoanAdvanceHead == null ? "" : ca1.LoanAdvanceHead.Id.ToString(),
                                            Fld8 = ca1.LoanAdvanceHead == null ? "" : ca1.LoanAdvanceHead.Code,
                                            Fld9 = ca1.LoanAdvanceHead == null ? "" : ca1.LoanAdvanceHead.Name,
                                            Fld10 = ca1.IsInstallment == null ? "" : ca1.IsInstallment.ToString(),
                                            Fld11 = ca1.SanctionedDate == null ? "" : ca1.SanctionedDate.Value.ToShortDateString(),
                                            Fld12 = ca1.LoanAppliedAmount == null ? "" : ca1.LoanAppliedAmount.ToString(),
                                            Fld13 = ca1.LoanSanctionedAmount == null ? "" : ca1.LoanSanctionedAmount.ToString(),
                                            Fld14 = ca1.TotalInstall == null ? "" : ca1.TotalInstall.ToString(),
                                            Fld15 = ca1.MonthlyInstallmentAmount == null ? "" : ca1.MonthlyInstallmentAmount.ToString(),
                                            Fld16 = ca1.MonthlyPricipalAmount == null ? "" : ca1.MonthlyPricipalAmount.ToString(),
                                            Fld17 = ca1.MonthlyInterest == null ? "" : ca1.MonthlyInterest.ToString(),
                                            Fld18 = ca1.LoanAccBranch == null ? "" : ca1.LoanAccBranch.FullDetails,
                                            Fld19 = ca1.LoanAccNo == null ? "" : ca1.LoanAccNo.ToString(),
                                            Fld20 = ca1.Narration == null ? "" : ca1.Narration,
                                            Fld21 = ca1.IsActive == null ? "" : ca1.IsActive.ToString(),
                                            Fld22 = ca1.CloserDate == null ? "" : ca1.CloserDate.Value.ToShortDateString(),
                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            else
                            {
                                return null;
                            }

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "LOANADVREPAYMENT":
                    var OLoanAdvRepaymentData = db.EmployeePayroll
                .Include(e => e.Employee)
                .Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.GeoStruct)
                    .Include(e => e.Employee.GeoStruct.Location)
                    .Include(e => e.Employee.FuncStruct)
                    .Include(e => e.Employee.FuncStruct.Job)
                    .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                    .Include(e => e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT))
                    .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead)).ToList()

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                   .ToList(); ;
                    //.Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                    // .Include(e => e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT))
                    //.Include(e => e.Employee)
                    //.Include(e => e.Employee.EmpName)
                    //  .Include(e => e.Employee.FuncStruct)
                    //    .Include(e => e.Employee.FuncStruct.Job)

                    //    .Include(e => e.Employee.FuncStruct.JobPosition)
                    //    .Include(e => e.Employee.PayStruct)
                    //    .Include(e => e.Employee.PayStruct.Grade)

                    //    //.Include(e => e.Employee.PayStruct.Level)

                    //    .Include(e => e.Employee.PayStruct.JobStatus)
                    //    .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                    //    .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                    //    .Include(e => e.Employee.GeoStruct)
                    //    //.Include(e => e.Employee.GeoStruct.Division)

                    //    .Include(e => e.Employee.GeoStruct.Location)
                    //    .Include(e => e.Employee.GeoStruct.Location.LocationObj)

                    //    .Include(e => e.Employee.GeoStruct.Department)
                    //    .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                    //     .Include(e => e.LoanAdvRequest)
                    //   // .Include(e => e.Employee.GeoStruct.Group)

                    //    //.Include(e => e.Employee.GeoStruct.Unit)
                    //    // .Include(r => r.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT))

                    //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    //.ToList();


                    if (OLoanAdvRepaymentData == null || OLoanAdvRepaymentData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLoanAdvRepaymentData)
                        {
                            if (ca.LoanAdvRequest != null && ca.LoanAdvRequest.Count() != 0)
                            {
                                var OITTrans = ca.LoanAdvRequest; //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                //  var OITTrans = ca.l //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        if (ca1.LoanAdvRepaymentT == null || ca1.LoanAdvRepaymentT.Count() == 0)
                                        { }
                                        else
                                        {
                                            foreach (var ca2 in ca1.LoanAdvRepaymentT.Where(e => mPayMonth.Contains(e.PayMonth.ToString())).ToList())
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = ca.Employee == null ? null : ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee != null ? ca.Employee.EmpCode : null,
                                                    Fld3 = ca.Employee != null && ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                    Fld4 = ca1.Id != null ? ca1.Id.ToString() : null,
                                                    Fld5 = ca1.RequisitionDate != null ? ca1.RequisitionDate.ToString() : null,
                                                    Fld6 = ca1.EnforcementDate != null ? ca1.EnforcementDate.ToString() : null,
                                                    Fld7 = ca1.LoanAdvanceHead != null ? ca1.LoanAdvanceHead.Id.ToString() : null,
                                                    Fld8 = ca1.LoanAdvanceHead != null ? ca1.LoanAdvanceHead.Code : null,
                                                    Fld9 = ca1.LoanAdvanceHead != null ? ca1.LoanAdvanceHead.Name : null,
                                                    Fld10 = ca1.IsInstallment != null ? ca1.IsInstallment.ToString() : null,
                                                    Fld11 = ca1.SanctionedDate != null ? ca1.SanctionedDate.Value.ToShortDateString() : null,
                                                    Fld12 = ca1.LoanAppliedAmount != null ? ca1.LoanAppliedAmount.ToString() : null,
                                                    Fld13 = ca1.LoanSanctionedAmount != null ? ca1.LoanSanctionedAmount.ToString() : null,
                                                    Fld14 = ca1.TotalInstall != null ? ca1.TotalInstall.ToString() : null,
                                                    Fld15 = ca1.IsActive != null ? ca1.IsActive.ToString() : null,
                                                    Fld16 = ca1.CloserDate != null ? ca1.CloserDate.ToString() : null,
                                                    Fld17 = ca2.Id != null ? ca2.Id.ToString() : null,
                                                    Fld18 = ca2.InstallementDate != null ? ca2.InstallementDate.Value.ToShortDateString() : null,
                                                    Fld19 = ca2.TotalLoanBalance != null ? ca2.TotalLoanBalance.ToString() : null,
                                                    Fld20 = ca2.TotalLoanPaid != null ? ca2.TotalLoanPaid.ToString() : null,
                                                    Fld21 = ca2.InstallmentAmount != null ? ca2.InstallmentAmount.ToString() : null,
                                                    Fld22 = ca2.InstallmentPaid != null ? ca2.InstallmentPaid.ToString() : null,
                                                    Fld23 = ca2.InstallmentCount != null ? ca2.InstallmentCount.ToString() : null,
                                                    Fld24 = ca2.PayMonth != null ? ca2.PayMonth : null,
                                                    Fld25 = ca2.RepaymentDate != null ? ca2.RepaymentDate.ToString() : null,
                                                    Fld26 = ca1.LoanAccBranch != null ? ca1.LoanAccBranch.ToString() : null,
                                                    Fld27 = ca1.LoanAccNo != null ? ca1.LoanAccNo.ToString() : null,
                                                    Fld30 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
                                                    Fld31 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                                    Fld28 = ca.Employee != null && ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                    Fld29 = ca.Employee != null && ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : null
                                                    //Fld28 = ca.Employee.PayStruct.Grade.Name == null ? null : ca.Employee.PayStruct.Grade.Name.ToString(),
                                                    //Fld29 = ca.Employee.FuncStruct.Job.Name == null ? null : ca.Employee.FuncStruct.Job.Name.ToString(),
                                                    //Fld30 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? null : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),
                                                    //Fld31 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc == null ? null : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString()


                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }

                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;



                case "INCRDUELIST":
                    var OIncrDueList = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.IncrDataCalc)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                         .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        .Include(e => e.Employee.PayStruct.Grade)
                        .Include(e => e.Employee.FuncStruct.Job)

                         .Where(e => (EmpPayrollIdList.Contains(e.Employee.Id)))// && (mPayMonth.Contains(e.IncrDataCalc.ProcessIncrDate.Value.ToString("MM/yyyy"))))
                         .ToList();

                    if (OIncrDueList == null || OIncrDueList.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OIncrDueList.Where(e => (e.IncrDataCalc != null) && (e.IncrDataCalc.ProcessIncrDate != null) && (mPayMonth.Contains(e.IncrDataCalc.ProcessIncrDate.Value.ToString("MM/yyyy")))))
                        {

                            if (ca.IncrDataCalc != null)
                            {

                                var OITTrans = ca.IncrDataCalc;
                                //  var OITTrans = ca.IncrDataCalc.Where(e => mPayMonth.Contains(e.ProcessIncrDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null)
                                {

                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Employee.Id.ToString(),
                                        Fld2 = ca.Employee.EmpCode,
                                        Fld3 = ca.Employee.EmpName.FullNameFML,
                                        Fld4 = ca.Id.ToString(),
                                        Fld5 = ca.IncrDataCalc.OrignalIncrDate.Value.ToShortDateString(),
                                        Fld6 = ca.IncrDataCalc.ProcessIncrDate.Value.ToShortDateString(),
                                        Fld7 = ca.IncrDataCalc.LWPDays.ToString(),
                                        Fld8 = ca.IncrDataCalc.OldBasic.ToString(),
                                        Fld9 = ca.IncrDataCalc.NewBasic.ToString(),
                                        Fld10 = ca.IncrDataCalc.StagnancyAppl.ToString(),
                                        Fld11 = ca.IncrDataCalc.StagnancyCount.ToString(),
                                        //Fld12 = ca.ReleaseDate.ToString(),
                                        //Fld13 = ca.Narration,
                                        //Fld14 = ca.StagnancyAppl.ToString(),
                                        //Fld15 = ca.StagnancyCount.ToString(),


                                        Fld16 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                                        Fld17 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                        Fld18 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                        Fld19 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                                        Fld20 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                                        Fld21 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                                        Fld22 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                        Fld23 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                        Fld24 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                        Fld25 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                        Fld26 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                    };
                                    OGenericPayrollStatement.Add(OGenericObjStatement);

                                }
                                //    else
                                //    {
                                //        return null;
                                //    }

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }



                    break;





                case "INCREMENTSERVICEBOOK":
                    var OIncrementServiceBookData = db.EmployeePayroll
                    .Include(e => e.IncrementServiceBook)
                    .Include(e => e.Employee)
                        //.Include(e => e.Employee.EmpCode)
                    .Include(e => e.Employee.EmpName)
                    .Include(r => r.IncrementServiceBook.Select(t => t.IncrActivity))
                    .Include(r => r.IncrementServiceBook.Select(t => t.IncrActivity.IncrList))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.IncrActivity))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.FuncStruct.Job.Id))
                    .Include(r => r.IncrementServiceBook.Select(t => t.FuncStruct.Job))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.FuncStruct.JobPosition))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.FuncStruct.JobPosition.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.FuncStruct.JobPosition.JobPositionCode))
                    .Include(r => r.IncrementServiceBook.Select(t => t.FuncStruct.JobPosition))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.PayStruct.Grade.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.PayStruct.Grade.Code))
                    .Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.Grade))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.PayStruct.JobStatus.Id))
                    .Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.JobStatus.EmpStatus))
                    .Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.JobStatus.EmpActingStatus))

                    //.Include(r=>r.IncrementServiceBook.Select(t=>t.PayStruct.Level.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.PayStruct.Level.Code))
                    .Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.Level))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Division.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Division.Code))
                    .Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Division))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Location.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Location.LocationObj.LocCode))
                    .Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Location.LocationObj))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Department.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Department.DepartmentObj.DeptCode))
                    .Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Department.DepartmentObj))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Group.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Group.Code))
                    .Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Group))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Unit.Id))
                        //.Include(r=>r.IncrementServiceBook.Select(t=>t.GeoStruct.Unit.Code))
                    .Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Unit))

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OIncrementServiceBookData == null || OIncrementServiceBookData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OIncrementServiceBookData)
                        {
                            if (ca.IncrementServiceBook != null && ca.IncrementServiceBook.Count() != 0)
                            {
                                var OITTrans = ca.IncrementServiceBook.Where(e => mPayMonth.Contains(e.ProcessIncrDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.Id.ToString(),
                                            Fld5 = ca1.IncrActivity == null ? "" : ca1.IncrActivity.Name,
                                            Fld6 = ca1.IncrActivity == null ? "" : ca1.IncrActivity.IncrList.LookupVal.ToUpper(),
                                            Fld7 = ca1.OrignalIncrDate.Value.ToShortDateString(),
                                            Fld8 = ca1.ProcessIncrDate.Value.ToShortDateString(),
                                            Fld9 = ca1.OldBasic.ToString(),
                                            Fld10 = ca1.NewBasic.ToString(),
                                            Fld11 = ca1.ReleaseFlag.ToString(),
                                            Fld12 = ca1.ReleaseDate.Value.ToShortDateString(),
                                            Fld13 = ca1.Narration,
                                            Fld14 = ca1.StagnancyAppl.ToString(),
                                            Fld15 = ca1.StagnancyCount.ToString(),


                                            Fld16 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                            Fld17 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld18 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld19 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                            Fld20 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                            Fld21 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                            Fld22 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                            Fld23 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                            Fld24 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                            Fld25 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld26 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "PROMOTIONSERVICEBOOK":
                    var OPromotionServiceBookData = db.EmployeePayroll

                    .Include(e => e.PromotionServiceBook)

                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity))
                    .Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity.PromoList))
                    .Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity.PromoPolicy.IncrActivity))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldFuncStruct.Job))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewFuncStruct.JobPosition))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewFuncStruct))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.Grade))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpStatus))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpActingStatus))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.Grade))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpStatus))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpActingStatus))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.Level))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.Level))
                    .Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Division))
                        //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Location))
                    .Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Location.LocationObj))
                     .Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Department.DepartmentObj))
                        //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Department))
                    .Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Group))
                        //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Unit))
                    .Include(r => r.PromotionServiceBook.Select(t => t.OldPayScale))
                           .Include(r => r.PromotionServiceBook.Select(t => t.OldPayScale.PayScaleType))
                           .Include(r => r.PromotionServiceBook.Select(t => t.OldPayScaleAgreement))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewPayScale))
                    .Include(r => r.PromotionServiceBook.Select(t => t.NewPayScale.PayScaleType))
                   .Include(r => r.PromotionServiceBook.Select(t => t.NewPayScaleAgreement))

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OPromotionServiceBookData == null || OPromotionServiceBookData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OPromotionServiceBookData)
                        {
                            if (ca.PromotionServiceBook != null && ca.PromotionServiceBook.Count() != 0)
                            {
                                var OITTrans = ca.PromotionServiceBook.Where(e => mPayMonth.Contains(e.ProcessPromoDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.Id.ToString(),
                                            Fld5 = ca1.PromotionActivity == null ? "" : ca1.PromotionActivity.Name,
                                            Fld6 = ca1.PromotionActivity == null ? "" : ca1.PromotionActivity.PromoList == null ? "" : ca1.PromotionActivity.PromoList.LookupVal.ToUpper(),
                                            Fld7 = ca1.ProcessPromoDate == null ? "" : ca1.ProcessPromoDate.Value.ToShortDateString(),
                                            Fld8 = ca1.OldBasic == null ? "" : ca1.OldBasic.ToString(),
                                            Fld9 = ca1.NewBasic == null ? "" : ca1.NewBasic.ToString(),
                                            Fld10 = ca1.ReleaseFlag == null ? "" : ca1.ReleaseFlag.ToString(),
                                            Fld11 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                            Fld12 = ca1.Narration == null ? "" : ca1.Narration,
                                            Fld13 = ca1.StagnancyAppl == null ? "" : ca1.StagnancyAppl.ToString(),
                                            Fld14 = ca1.StagnancyCount == null ? "" : ca1.StagnancyCount.ToString(),
                                            Fld15 = ca1.Fittment == null ? "" : ca1.Fittment.ToString(),
                                            Fld16 = ca1.IncrNewBasic == null ? "" : ca1.IncrNewBasic.ToString(),
                                            Fld17 = ca1.IncrOldBasic == null ? "" : ca1.IncrOldBasic.ToString(),

                                            Fld18 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                            Fld19 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld20 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld21 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                            //Fld22 = ca1.GeoStruct == null ? "" : ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                            Fld23 = ca1.OldFuncStruct == null ? "" : ca1.OldFuncStruct.Job == null ? "" : ca1.OldFuncStruct.Job.Name,
                                            Fld24 = ca1.NewFuncStruct == null ? "" : ca1.NewFuncStruct.Job == null ? "" : ca1.NewFuncStruct.Job.Name,

                                            Fld25 = ca1.OldFuncStruct == null ? "" : ca1.OldFuncStruct.JobPosition == null ? "" : ca1.OldFuncStruct.JobPosition.JobPositionDesc,
                                            Fld26 = ca1.NewFuncStruct == null ? "" : ca1.NewFuncStruct.JobPosition == null ? "" : ca1.NewFuncStruct.JobPosition.JobPositionDesc,



                                            Fld27 = ca1.OldPayStruct == null ? "" : ca1.OldPayStruct.Grade == null ? "" : ca1.OldPayStruct.Grade.Name,
                                            Fld28 = ca1.NewPayStruct == null ? "" : ca1.NewPayStruct.Grade == null ? "" : ca1.NewPayStruct.Grade.Name,

                                            Fld29 = ca1.OldPayStruct == null ? "" : ca1.OldPayStruct.Level == null ? "" : ca1.OldPayStruct.Level.Name,
                                            Fld30 = ca1.NewPayStruct == null ? "" : ca1.NewPayStruct.Level == null ? "" : ca1.NewPayStruct.Level.Name,

                                            Fld31 = ca1.OldPayStruct == null ? "" : ca1.OldPayStruct.JobStatus == null ? "" : ca1.OldPayStruct.JobStatus.EmpStatus == null ? "" : ca1.OldPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld33 = ca1.OldPayStruct == null ? "" : ca1.OldPayStruct.JobStatus == null ? "" : ca1.OldPayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.OldPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            Fld32 = ca1.NewPayStruct == null ? "" : ca1.NewPayStruct.JobStatus == null ? "" : ca1.NewPayStruct.JobStatus.EmpStatus == null ? "" : ca1.NewPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld34 = ca1.NewPayStruct == null ? "" : ca1.NewPayStruct.JobStatus == null ? "" : ca1.NewPayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            Fld35 = ca1.OldPayScale == null ? "" : ca1.OldPayScale.PayScaleType == null ? "" : ca1.OldPayScale.PayScaleType.LookupVal.ToUpper(),
                                            Fld36 = ca1.NewPayScale == null ? "" : ca1.NewPayScale.PayScaleType == null ? "" : ca1.NewPayScale.PayScaleType.LookupVal.ToUpper(),
                                            Fld37 = ca1.OldPayScaleAgreement == null ? "" : ca1.OldPayScaleAgreement.FullDetails,
                                            Fld38 = ca1.NewPayScaleAgreement == null ? "" : ca1.NewPayScaleAgreement.FullDetails,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;
                //Sunil Grade End //

                case "TRANSFERSERVICEBOOK":
                    var OTransferServiceBookData = db.EmployeePayroll
                        //.Include(e => e.TransferServiceBook)
                    .Include(e => e.Employee)
                        //.Include(e => e.Employee.EmpCode)
                    .Include(e => e.Employee.EmpName)
                    .Include(r => r.TransferServiceBook.Select(t => t.TransActivity))
                    .Include(r => r.TransferServiceBook.Select(t => t.TransActivity.TransList))
                        //.Include(r=>r.TransferServiceBook.Select(t=>t.IncrActivity))
                    .Include(r => r.TransferServiceBook.Select(t => t.OldFuncStruct.Job))
                     .Include(r => r.TransferServiceBook.Select(t => t.NewFuncStruct.Job))

                    .Include(r => r.TransferServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                    .Include(r => r.TransferServiceBook.Select(t => t.NewFuncStruct.JobPosition))

                    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Division))
                    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Location.LocationObj))
                    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Department.DepartmentObj))
                    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Group))
                    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Unit))

                    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Division))
                    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Location.LocationObj))
                    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Department.DepartmentObj))
                    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Group))
                    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Unit))

                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OTransferServiceBookData == null || OTransferServiceBookData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OTransferServiceBookData)
                        {
                            if (ca.TransferServiceBook != null && ca.TransferServiceBook.Count() != 0)
                            {
                                var OITTrans = ca.TransferServiceBook.Where(e => mPayMonth.Contains(e.ProcessTransDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML,
                                            Fld4 = ca1.Id.ToString(),
                                            Fld5 = ca1.TransActivity == null ? "" : ca1.TransActivity.Name,
                                            Fld6 = ca1.TransActivity == null ? "" : ca1.TransActivity.TransList.LookupVal.ToUpper(),
                                            Fld7 = ca1.ProcessTransDate.Value.ToShortDateString(),

                                            Fld8 = ca1.ReleaseFlag.ToString(),
                                            Fld9 = ca1.ReleaseDate.Value.ToShortDateString(),
                                            Fld10 = ca1.Narration,


                                            Fld11 = ca1.OldGeoStruct.Division == null ? "" : ca1.OldGeoStruct.Division.Name,
                                            Fld12 = ca1.NewGeoStruct.Division == null ? "" : ca1.NewGeoStruct.Division.Name,
                                            Fld13 = ca1.OldGeoStruct.Location == null ? "" : ca1.OldGeoStruct.Location.LocationObj.LocDesc,
                                            Fld14 = ca1.NewGeoStruct.Location == null ? "" : ca1.NewGeoStruct.Location.LocationObj.LocDesc,
                                            Fld15 = ca1.OldGeoStruct.Department == null ? "" : ca1.OldGeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld16 = ca1.NewGeoStruct.Department == null ? "" : ca1.NewGeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld17 = ca1.OldGeoStruct.Group == null ? "" : ca1.OldGeoStruct.Group.Name,
                                            Fld18 = ca1.NewGeoStruct.Group == null ? "" : ca1.NewGeoStruct.Group.Name,
                                            Fld19 = ca1.OldGeoStruct.Unit == null ? "" : ca1.OldGeoStruct.Unit.Name,
                                            Fld20 = ca1.NewGeoStruct.Unit == null ? "" : ca1.NewGeoStruct.Unit.Name,

                                            Fld23 = ca1.OldFuncStruct.Job == null ? "" : ca1.OldFuncStruct.Job.Name,
                                            Fld24 = ca1.NewFuncStruct.Job == null ? "" : ca1.NewFuncStruct.Job.Name,

                                            Fld25 = ca1.OldFuncStruct.JobPosition == null ? "" : ca1.OldFuncStruct.JobPosition.JobPositionDesc,
                                            Fld26 = ca1.NewFuncStruct.JobPosition == null ? "" : ca1.NewFuncStruct.JobPosition.JobPositionDesc,
                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "OTHERSERVICEBOOK":
                    var OOtherServiceBookData = db.EmployeePayroll
                    .Include(e => e.OtherServiceBook)
                    .Include(e => e.Employee)
                        //.Include(e => e.Employee.EmpCode)
                    .Include(e => e.Employee.EmpName)
                    .Include(r => r.OtherServiceBook.Select(t => t.OthServiceBookActivity))
                    .Include(r => r.OtherServiceBook.Select(t => t.OthServiceBookActivity.OtherSerBookActList))

                    .Include(r => r.OtherServiceBook.Select(t => t.OldFuncStruct.Job))
                    .Include(r => r.OtherServiceBook.Select(t => t.NewFuncStruct.Job))

                    .Include(r => r.OtherServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                    .Include(r => r.OtherServiceBook.Select(t => t.NewFuncStruct.JobPosition))

                    .Include(r => r.OtherServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpStatus))
                    .Include(r => r.OtherServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpActingStatus))

                    .Include(r => r.OtherServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpStatus))
                    .Include(r => r.OtherServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpActingStatus))


                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    if (OOtherServiceBookData == null || OOtherServiceBookData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OOtherServiceBookData)
                        {
                            if (ca.OtherServiceBook != null && ca.OtherServiceBook.Count() != 0)
                            {
                                var OITTrans = ca.OtherServiceBook.Where(e => mPayMonth.Contains(e.ProcessOthDate.Value.ToString("MM/yyyy"))).ToList();
                                if (OITTrans != null && OITTrans.Count() != 0)
                                {
                                    foreach (var ca1 in OITTrans)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                            Fld4 = ca1.Id.ToString(),
                                            Fld5 = ca1.OthServiceBookActivity != null ? ca1.OthServiceBookActivity.Name : null,
                                            Fld6 = ca1.OthServiceBookActivity != null && ca1.OthServiceBookActivity.OtherSerBookActList != null ? ca1.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() : null,
                                            Fld7 = ca1.ProcessOthDate != null ? ca1.ProcessOthDate.Value.ToShortDateString() : null,
                                            Fld8 = ca1.ReleaseFlag.ToString(),
                                            Fld9 = ca1.ReleaseDate != null ? ca1.ReleaseDate.Value.ToShortDateString() : null,
                                            Fld10 = ca1.Narration,
                                            Fld11 = ca1.IsBasicChangeAppl.ToString(),
                                            Fld12 = ca1.NewBasic.ToString(),

                                            Fld13 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : null,
                                            Fld14 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : null,

                                            Fld15 = ca1.NewFuncStruct != null && ca1.OldFuncStruct.JobPosition != null ? ca1.OldFuncStruct.JobPosition.JobPositionDesc : null,
                                            Fld16 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.JobPosition != null ? ca1.NewFuncStruct.JobPosition.JobPositionDesc : null,

                                            Fld17 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpStatus != null ? ca1.OldPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                            Fld18 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpActingStatus != null ? ca1.OldPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,
                                            Fld19 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpStatus != null ? ca1.NewPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                            Fld20 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpActingStatus != null ? ca1.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
                            //}

                        }
                        return OGenericPayrollStatement;
                    }
                    break;

                case "EARNSALARYSTATEMENT":

                    var OSalHead = db.CompanyPayroll
               .Include(e => e.SalaryHead)
               .Include(e => e.SalaryHead.Select(r => r.Type))
               .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();


                    var OEmpData = db.EmployeePayroll
                    .Include(e => e.SalaryT)
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                    .Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                    .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                    .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    var OSalTchk = OEmpData.Select(e => e.SalaryT).ToList();
                    var mHeader = false;
                    if (OEmpData.Count() == 0 || OSalHead == null || OSalTchk == null)
                    {
                        return null;
                    }
                    List<GenericField100> OGenericSalaryStatement = new List<GenericField100>();
                    foreach (var OEmpsingle in OEmpData)
                    {

                        //var OEarnDedData = db.SalEarnDedT.Include(d => d.SalaryHead)
                        //                       .OrderBy(e => e.SalaryHead.SeqNo)
                        //                       .Where(e => e.SalaryHead.InPayslip == true)
                        //                       .Select(r => new { code = r.SalaryHead.Code, amout = r.Amount, type = r.SalaryHead.Type.LookupVal.ToUpper(), seqno = r.SalaryHead.SeqNo });
                        //var OEmpDataFinal= OEmpData.Select(e=>e.SalaryT.Where(d=>d.PayMonth==mPayMonth));
                        var OEarnDedData = OEmpsingle.SalaryT.Where(g => mPayMonth.Contains(g.PayMonth))
                                 .Select(e => e.SalEarnDedT.Where(y => (y.SalaryHead.InPayslip == true) && y.SalaryHead.Type.LookupVal == "Earning")
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

                                }
                                )).SingleOrDefault();
                        if (OEarnDedData != null)
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
                                        Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString()
                                    };
                                    OGenericSalaryStatement.Add(OEarnStatement);
                                }
                            }
                        }



                    }
                    return OGenericSalaryStatement.ToList();
                    break;
                case "DEDSALARYSTATEMENT":

                    var OSaldedHead = db.CompanyPayroll
               .Include(e => e.SalaryHead)
               .Include(e => e.SalaryHead.Select(r => r.Type))
               .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();


                    var OEmpdedData = db.EmployeePayroll
                    .Include(e => e.SalaryT)
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                    .Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                    .Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                    .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                    .Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                    .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus))
                    .Where(e => EmpPayrollIdList.Contains(e.Employee.Id)) //&& e.SalaryT.Select(t=>t.SalEarnDedT.Select(r=>r.SalaryHead.Type.LookupVal="Earning")))
                    .ToList();

                    var OSalTdedchk = OEmpdedData.Select(e => e.SalaryT).ToList();
                    //var mHeader = false;
                    if (OEmpdedData.Count() == 0 || OSalTdedchk == null || OSalTdedchk == null)
                    {
                        return null;
                    }
                    foreach (var OEmpsingle in OEmpdedData)
                    {
                        var OEarnDedData = OEmpsingle.SalaryT.Where(g => mPayMonth.Contains(g.PayMonth))
                                 .Select(e => e.SalEarnDedT.Where(y => (y.SalaryHead.InPayslip == true) && y.SalaryHead.Type.LookupVal == "Deduction")
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

                                }
                                )).SingleOrDefault();
                        if (OEarnDedData != null)
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
                                        Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString()
                                    };
                                    OGenericPayrollStatement.Add(OEarnStatement);
                                }
                            }
                        }
                    }
                    return OGenericPayrollStatement.ToList();
                    break;

                default:
                    return null;
                    break;



                /////////////////////Employee Information////////////////////////////////

                //case "EMPLOYEEINFO":
                //    var OEmpInfoData = db.EmployeePayroll
                //       .Include(e => e.Employee)
                //       .Include(e => e.Employee.EmpName)
                //       .Include(e=> e.Employee.EmpOffInfo)
                //        .Include(e => e.Employee.ServiceBookDates)
                //       //.Include(e => e.GratuityT)
                //        .Include(e => e.Employee.GeoStruct)
                //         .Include(e => e.Employee.GeoStruct.Location)
                //         .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                //          .Include(e => e.Employee.GeoStruct.Department)
                //          .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                //          .Include(e => e.Employee.PayStruct)
                //         .Include(e => e.Employee.PayStruct.Grade)
                //            .Include(e => e.Employee.FuncStruct)
                //         .Include(e => e.Employee.FuncStruct.Job)

                //        //.Include(e => e.GratuityT.Select(t => t.GeoStruct.Location.LocationObj))
                //        //.Include(e => e.GratuityT.Select(t => t.GeoStruct.Department.DepartmentObj))
                //        //.Include(e => e.GratuityT.Select(t => t.FuncStruct.Job))
                //        //.Include(e => e.GratuityT.Select(t => t.FuncStruct.JobPosition))
                //        //.Include(e => e.GratuityT.Select(t => t.PayStruct.Grade))
                //        //.Include(e => e.GratuityT.Select(t => t.PayStruct.JobStatus))
                //        //.Include(e => e.Employee.ServiceBookDates)
                //         .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                //    .ToList();


                //    if (OEmpInfoData == null || OEmpInfoData.Count() == 0)
                //    {
                //        return null;
                //    }
                //    else
                //    {
                //        foreach (var ca in OEmpInfoData)
                //        {
                //            if (ca.Employee == null || ca.Employee.Count() != 0)
                //            {
                //                var OEmpDetails = ca.Employee.ToList();

                //                if (OGratuityDatails != null && OGratuityDatails.Count() != 0)
                //                {

                //                    foreach (var ca1 in OGratuityDatails)
                //                    {
                //                        GenericField100 OGenericObjStatement = new GenericField100()
                //                        {
                //                            Fld1 = ca1.ActualService == null ? "" : ca1.ActualService.ToString(),
                //                            Fld2 = ca1.RoundedService == null ? "" : ca1.RoundedService.ToString(),
                //                            Fld3 = ca1.Amount == null ? "" : ca1.Amount.ToString(),
                //                            Fld4 = ca1.ProcessDate == null ? "" : ca1.ProcessDate.Value.ToShortDateString(),
                //                            Fld5 = ca1.TotalLWP == null ? "" : ca1.TotalLWP.ToString(),


                //                            Fld26 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                //                            Fld27 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                //                            Fld28 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                //                            //Fld29 = ca.Employee.GeoStruct.Location.LocationObj.Id == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.Id.ToString(),
                //                            //Fld30 = ca.Employee.GeoStruct.Location.LocationObj.LocCode == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                //                            //Fld31 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),

                //                            //Fld32 = ca.Employee.GeoStruct.Department.DepartmentObj.Id == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString(),
                //                            //Fld33 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString(),
                //                            //Fld34 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString(),

                //                            Fld29 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null ? ca.Employee.GeoStruct.Location.Id.ToString() : null,
                //                            Fld30 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocCode : null,
                //                            Fld31 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : null,

                //                            Fld32 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString() : null,
                //                            Fld33 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString() : null,
                //                            Fld34 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,

                //                            Fld35 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Id.ToString() : null,
                //                            Fld36 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Code.ToString() : null,
                //                            Fld37 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,


                //                            //Fld35 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                //                            //Fld36 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                //                            //Fld37 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                //                            //Fld38 = ca.Employee.FuncStruct.Job.Id == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                //                            //Fld39 = ca.Employee.FuncStruct.Job.Code == null ? "" : ca.Employee.FuncStruct.Job.Code.ToString(),
                //                            //Fld40 = ca.Employee.FuncStruct.Job.Name == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),
                //                            Fld38 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Id.ToString() : null,
                //                            Fld39 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Code : null,
                //                            Fld40 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : null,

                //                            Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                //                            Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                //                            Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                //                            //Fld31 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                //                            //Fld32 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                //                            //Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                //                            //Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                //                            //Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                //                            //Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                //                            //Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                //                            //Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                //                            //Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                //                            //Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                //                            //Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                //                            //Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                //                            //Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                //                            //Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                //                            //Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                //                            //Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                //                            //Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                //                            //Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                //                            //Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                //                            //Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                //                            //Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                //                            //Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                //                            //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                //                            //Fld40 = ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                //                            ////Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                //                            ////Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                //                            ////Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,


                //                        };
                //                        OGenericPayrollStatement.Add(OGenericObjStatement);
                //                    }
                //                }
                //                //else
                //                //{
                //                //    return null;
                //                //}

                //            }
                //            //else
                //            //{
                //            //    return null;
                //            //}
                //        }

                //        return OGenericPayrollStatement;
                //    }

                //    break;













            }
            //return null;
        }
        public static List<GenericField100> GenerateLeaveReport(int CompanyLeaveId, string mObjectName, List<int> EmpLeaveIdList, DateTime mFromDate, DateTime mToDate, DataBaseContext db)
        {
            ///////////////////////Master Report////////////////////

            List<GenericField100> OGenericLvTransStatement = new List<GenericField100>();
            switch (mObjectName)
            {


                case "LVCREDITPOLICY":
                    var OLvCreditPolicyData = db.CompanyLeave
                        .Include(e => e.LvCreditPolicy)
                        .Include(e => e.LvCreditPolicy.Select(r => r.ConvertLeaveHead))
                        .Include(e => e.LvCreditPolicy.Select(r => r.ExcludeLeaveHeads))
                        // .Include(e => e.LvCreditPolicy.Select(r => r.LVConvert))
                        .Include(e => e.LvCreditPolicy.Select(r => r.LvHead))
                        .Include(e => e.LvCreditPolicy.Select(r => r.ConvertLeaveHeadBal))
                        .Include(e => e.LvCreditPolicy.Select(r => r.CreditDate))


                    .Where(d => d.Id == CompanyLeaveId).SingleOrDefault();

                    if (OLvCreditPolicyData.LvCreditPolicy == null || OLvCreditPolicyData.LvCreditPolicy.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvCreditPolicyData.LvCreditPolicy)
                        {
                            var mExcludeLv = "";
                            if (ca.ExcludeLeaveHeads != null && ca.ExcludeLeaveHeads.Count() > 0)
                            {
                                foreach (var ca1 in ca.ExcludeLeaveHeads)
                                {
                                    mExcludeLv = mExcludeLv == "" ? ca1.LvName : mExcludeLv + " : " + ca1.LvName;
                                }
                            }
                            GenericField100 OGenericLvMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.PolicyName,
                                Fld3 = ca.LvHead.LvCode,
                                Fld4 = ca.LvHead.LvName,
                                Fld5 = ca.Accumalation.ToString(),
                                Fld6 = ca.AccumalationLimit.ToString(),
                                Fld7 = ca.AccumulationWithCredit.ToString(),
                                Fld8 = ca.IsCreditDatePolicy.ToString(),
                                Fld9 = ca.CreditDate == null ? "" : ca.CreditDate.LookupVal.ToUpper(),
                                Fld10 = ca.ProCreditFrequency.ToString(),
                                Fld11 = ca.ProdataFlag.ToString(),
                                Fld12 = ca.FixedCreditDays.ToString(),
                                Fld13 = ca.CreditDays.ToString(),
                                Fld14 = ca.WorkingDays.ToString(),

                                Fld15 = ca.ExcludeLeaves.ToString(),
                                Fld16 = ca.LVConvert.ToString(),
                                Fld17 = ca.ConvertLeaveHead.LvName.ToString(),
                                Fld18 = ca.ConvertedDays.ToString(),
                                Fld19 = ca.ConvertLeaveHeadBal.LvName.ToString(),
                                Fld20 = ca.AboveServiceMaxYears.ToString(),
                                Fld21 = ca.AboveServiceSteps.ToString(),
                                Fld22 = ca.LVConvertBal.ToString(),
                                Fld23 = ca.LvConvertLimit.ToString(),
                                Fld24 = ca.LvConvertLimitBal.ToString(),
                                Fld25 = ca.MaxLeaveDebitInService.ToString(),
                                Fld26 = ca.ServiceLink.ToString(),
                                Fld27 = ca.ServiceYearsLimit.ToString(),
                                Fld28 = ca.OccInServ.ToString(),
                                Fld29 = ca.OccInServAppl.ToString(),
                                Fld30 = mExcludeLv,


                            };
                            OGenericLvTransStatement.Add(OGenericLvMasterObjStatement);
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;
                case "LVDEBITPOLICY":
                    var OLvDebitPolicyData = db.CompanyLeave
                        .Include(e => e.LvDebitPolicy)
                        .Include(e => e.LvDebitPolicy.Select(r => r.CombinedLvHead.Select(t => t.LvHead)))
                        .Include(e => e.LvDebitPolicy.Select(r => r.LvHead))


                    .Where(d => d.Id == CompanyLeaveId).SingleOrDefault();

                    if (OLvDebitPolicyData.LvDebitPolicy == null || OLvDebitPolicyData.LvDebitPolicy.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvDebitPolicyData.LvDebitPolicy)
                        {
                            var mCombinedLv = "";
                            if (ca.CombinedLvHead != null && ca.CombinedLvHead.Count() > 0)
                            {
                                foreach (var ca1 in ca.CombinedLvHead)
                                {
                                    mCombinedLv = mCombinedLv == "" ? ca1.LvHead.LvName : mCombinedLv + " : " + ca1.LvHead.LvName;
                                }
                            }
                            GenericField100 OGenericLvMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.PolicyName,
                                Fld3 = ca.LvHead.LvCode,
                                Fld4 = ca.LvHead.LvName,
                                Fld5 = ca.MinUtilDays.ToString(),
                                Fld6 = ca.MaxUtilDays.ToString(),
                                Fld7 = ca.HalfDayAppl.ToString(),
                                Fld8 = ca.HolidayInclusive.ToString(),
                                Fld9 = ca.Combined.ToString(),
                                Fld10 = mCombinedLv,
                                Fld11 = ca.PreApplied.ToString(),
                                Fld12 = ca.PreDays.ToString(),
                                Fld13 = ca.PostApplied.ToString(),
                                Fld14 = ca.PostDays.ToString(),

                                Fld15 = ca.PrefixSuffix.ToString(),
                                Fld16 = ca.PreApplyPrefixSuffix.ToString(),
                                Fld17 = ca.PostApplyPrefixSuffix.ToString(),
                                Fld18 = ca.PrefixGraceCount.ToString(),
                                Fld19 = ca.PrefixMaxCount.ToString(),
                                Fld20 = ca.Sandwich.ToString(),
                                Fld21 = ca.YearlyOccurances.ToString(),
                                Fld22 = ca.ApplyFutureGraceMonths.ToString(),
                                Fld23 = ca.ApplyPastGraceMonths.ToString(),



                            };
                            OGenericLvTransStatement.Add(OGenericLvMasterObjStatement);
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;
                case "LVENCASHPOLICY":
                    var OLvEncashPolicyData = db.CompanyLeave
                        .Include(e => e.LvEncashPolicy)
                        .Include(e => e.LvEncashPolicy.Select(r => r.LvHead))

                    .Where(d => d.Id == CompanyLeaveId).SingleOrDefault();

                    if (OLvEncashPolicyData.LvEncashPolicy == null || OLvEncashPolicyData.LvEncashPolicy.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvEncashPolicyData.LvEncashPolicy)
                        {

                            GenericField100 OGenericLvMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.PolicyName,
                                Fld3 = ca.LvHead.LvCode,
                                Fld4 = ca.LvHead.LvName,
                                Fld5 = ca.MinEncashment.ToString(),
                                Fld6 = ca.MaxEncashment.ToString(),
                                Fld7 = ca.MinBalance.ToString(),
                                Fld8 = ca.MinUtilized.ToString(),
                                Fld9 = ca.EncashSpanYear.ToString(),
                                Fld10 = ca.IsLvMultiple.ToString(),
                                Fld11 = ca.LvMultiplier.ToString(),
                                Fld12 = ca.IsOnBalLv.ToString(),
                                Fld13 = ca.LvBalPercent.ToString(),
                                Fld14 = ca.IsLvRequestAppl.ToString(),

                            };
                            OGenericLvTransStatement.Add(OGenericLvMasterObjStatement);
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;
                case "LVHEAD":
                    var OLvHeadData = db.LvHead
                        .Include(e => e.LvHeadOprationType)
                        .ToList();
                    //.Where(d => d.Id == CompanyLeaveId).SingleOrDefault();

                    if (OLvHeadData == null || OLvHeadData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvHeadData)
                        {

                            GenericField100 OGenericLvMasterObjStatement = new GenericField100()
                            //write data to generic class
                            {
                                Fld1 = ca.Id.ToString(),
                                Fld2 = ca.LvCode,
                                Fld3 = ca.LvName,
                                Fld4 = ca.LvHeadAlias,
                                Fld5 = ca.LvHeadOprationType == null ? "" : ca.LvHeadOprationType.LookupVal.ToUpper(),
                                Fld6 = ca.HFPay.ToString(),
                                Fld7 = ca.LTAAppl.ToString(),
                                Fld8 = ca.ApplAtt.ToString(),
                                Fld9 = ca.EncashRegular.ToString(),
                                Fld10 = ca.EncashRetirement.ToString(),
                                Fld11 = ca.ESS.ToString(),
                                Fld12 = ca.FullDetails,

                            };
                            OGenericLvTransStatement.Add(OGenericLvMasterObjStatement);
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;

                //////////////////////Transaction Reports//////////////////////////
                case "LVOPENBAL":
                    var OLvOpenBalData = db.EmployeeLeave
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)

                        .Include(e => e.LvOpenBal)
                        .Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                        .Include(e => e.LvOpenBal.Select(r => r.LvHead))
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)

                        .Include(e => e.Employee.FuncStruct.JobPosition)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)

                        .Include(e => e.Employee.PayStruct.Level)

                        .Include(e => e.Employee.PayStruct.JobStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Division)

                        .Include(e => e.Employee.GeoStruct.Location)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)

                        .Include(e => e.Employee.GeoStruct.Department)
                        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                        .Include(e => e.Employee.GeoStruct.Group)

                        .Include(e => e.Employee.GeoStruct.Unit)


                        .Where(e => EmpLeaveIdList.Contains(e.Employee.Id))
                                .ToList();

                    if (OLvOpenBalData == null || OLvOpenBalData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvOpenBalData)
                        {
                            if (ca.LvOpenBal != null && ca.LvOpenBal.Count() != 0)
                            {
                                var OLvOpen = ca.LvOpenBal.Where(e => e.LvCalendar.FromDate.Value <= mFromDate && e.LvCalendar.ToDate.Value >= mToDate).ToList();
                                if (OLvOpen != null && OLvOpen.Count() != 0)
                                {
                                    foreach (var ca1 in OLvOpen)
                                    {

                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca1.Id.ToString(),
                                            Fld2 = ca1.LvCalendar == null ? "" : ca1.LvCalendar.FullDetails,
                                            Fld3 = ca1.LvHead.LvCode,
                                            Fld4 = ca1.LvHead.LvName,
                                            Fld5 = ca1.LvOpening.ToString(),
                                            Fld6 = ca1.LvCredit.ToString(),
                                            Fld7 = ca1.LvClosing.ToString(),
                                            Fld8 = ca1.AboveServiceStepsCount.ToString(),
                                            Fld9 = ca1.LvCreditDate == null ? "" : ca1.LvCreditDate.ToString(),
                                            Fld10 = ca1.LvOccurances.ToString(),
                                            Fld11 = ca1.LvLapseBal.ToString(),
                                            Fld12 = ca1.LVCount.ToString(),
                                            Fld13 = ca1.AboveServiceStepsCount.ToString(),
                                            Fld14 = ca1.MaxDays.ToString(),
                                            Fld15 = ca.Id.ToString(),
                                            Fld16 = ca.Employee.EmpCode,
                                            Fld17 = ca.Employee.EmpName.FullNameFML,
                                            Fld18 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Id.ToString(),
                                            Fld19 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Code,
                                            Fld20 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                                            Fld21 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.Id.ToString(),
                                            Fld22 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                            Fld23 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld24 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.Id.ToString(),
                                            Fld25 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                            Fld26 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld27 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Id.ToString(),
                                            Fld28 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Code,
                                            Fld29 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                                            Fld30 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Id.ToString(),
                                            Fld31 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Code,
                                            Fld32 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                                            Fld33 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                            Fld34 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Code,
                                            Fld35 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                                            Fld36 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                                            Fld37 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                            Fld38 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                            Fld39 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            Fld40 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Code,
                                            Fld41 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                            Fld42 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                                            Fld43 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Code,
                                            Fld44 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                            Fld45 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                                            Fld46 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld47 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            //Fld48=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Id.ToString(),
                                            //Fld49=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                                            //Fld50=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Name,


                                        };
                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    }
                                }

                                else
                                {
                                    //return null;
                                }
                            }

                            else
                            {
                                //return null;
                            }
                        }
                        return OGenericLvTransStatement;
                    }

                    break;
                case "LVNEWREQ":

                    var OLvNewReqData = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                        .Include(e => e.LvNewReq.Select(r => r.FromStat))
                        .Include(e => e.LvNewReq.Select(r => r.ToStat))
                        //.Include(e => e.LvNewReq.Select(r => r.FuncStruct))
                        //.Include(e => e.LvNewReq.Select(r => r.GeoStruct))
                        //.Include(e => e.LvNewReq.Select(r => r.PayStruct))
                        .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)

                        .Include(e => e.Employee.FuncStruct.JobPosition)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)

                        .Include(e => e.Employee.PayStruct.Level)

                        .Include(e => e.Employee.PayStruct.JobStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Division)

                        .Include(e => e.Employee.GeoStruct.Location)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)

                        .Include(e => e.Employee.GeoStruct.Department)
                        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                        .Include(e => e.Employee.GeoStruct.Group)

                        .Include(e => e.Employee.GeoStruct.Unit)


                        .Where(e => EmpLeaveIdList.Contains(e.Employee.Id))
                        .ToList();
                    if (OLvNewReqData == null || OLvNewReqData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvNewReqData)
                        {
                            if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                            {
                                var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                if (OLvReq != null && OLvReq.Count() != 0)
                                {
                                    foreach (var ca1 in OLvReq)
                                    {


                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            //Fld1 = ca1.Id.ToString(),
                                            Fld2 = ca1.Id.ToString(),
                                            Fld3 = ca1.LeaveHead.LvCode,
                                            Fld4 = ca1.LeaveHead.LvName,
                                            Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                            Fld6 = ca1.ReqDate.ToString(),
                                            Fld7 = ca1.FromDate.ToString(),
                                            Fld8 = ca1.FromStat.LookupVal.ToUpper(),
                                            Fld9 = ca1.ToDate.ToString(),
                                            Fld10 = ca1.ToStat.LookupVal.ToUpper(),
                                            Fld11 = ca1.DebitDays.ToString(),
                                            Fld12 = ca1.ResumeDate.ToString(),
                                            Fld13 = ca1.OpenBal.ToString(),
                                            Fld14 = ca1.CloseBal.ToString(),
                                            Fld15 = ca1.InputMethod == null ? "" : ca1.InputMethod.ToString(),
                                            Fld16 = ca1.LVCount == null ? "" : ca1.LVCount.ToString(),
                                            Fld17 = ca1.LvOccurances.ToString(),
                                            Fld18 = ca1.PrefixCount == null ? "" : ca1.PrefixCount.ToString(),
                                            Fld19 = ca1.SufixCount == null ? "" : ca1.SufixCount.ToString(),
                                            Fld20 = ca1.TrClosed == null ? "" : ca1.TrClosed.ToString(),
                                            Fld21 = ca1.TrReject == null ? "" : ca1.TrReject.ToString(),
                                            Fld22 = ca1.WFStatus == null ? "" : ca1.WFStatus.LookupVal.ToUpper(),
                                            Fld23 = ca1.IsCancel == null ? "" : ca1.IsCancel.ToString(),
                                            Fld24 = ca1.ContactNo == null ? "" : ca1.ContactNo.FullContactNumbers,
                                            Fld25 = ca1.Reason.ToString(),


                                            Fld26 = ca.Id.ToString(),
                                            Fld27 = ca.Employee.EmpCode,
                                            Fld28 = ca.Employee.EmpName.FullNameFML,
                                            Fld29 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Id.ToString(),
                                            //Fld19=ca.Employee.GeoStruct.Division==null?"":ca.Employee.GeoStruct.Division.Code,
                                            Fld30 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                                            Fld31 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.Id.ToString(),
                                            //Fld22=ca.Employee.GeoStruct.Location==null?"":ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                            Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld33 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.Id.ToString(),
                                            //Fld25=ca.Employee.GeoStruct.Department==null?"":ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                            Fld34 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld35 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Id.ToString(),
                                            //Fld28=ca.Employee.GeoStruct.Group==null?"":ca.Employee.GeoStruct.Group.Code,
                                            Fld36 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                                            Fld37 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Id.ToString(),
                                            //Fld31=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                                            Fld38 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                                            Fld39 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                            //Fld34=ca.Employee.FuncStruct.Job==null?"":ca.Employee.FuncStruct.Job.Code,
                                            Fld40 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                                            Fld41 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                                            //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                            Fld42 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                            Fld43 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                            Fld44 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                            Fld45 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                                            //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                            Fld46 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                            Fld47 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                                            Fld48 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld49 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                        };
                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    }
                                }
                            }
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;


                case "LVLEDGER":

                    var OLvLedger = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                        .Include(e => e.LvNewReq.Select(r => r.FromStat))
                        .Include(e => e.LvNewReq.Select(r => r.ToStat))
                        //.Include(e => e.LvNewReq.Select(r => r.FuncStruct))
                        //.Include(e => e.LvNewReq.Select(r => r.GeoStruct))
                        //.Include(e => e.LvNewReq.Select(r => r.PayStruct))
                        .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)

                        .Include(e => e.Employee.FuncStruct.JobPosition)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)

                        .Include(e => e.Employee.PayStruct.Level)

                        .Include(e => e.Employee.PayStruct.JobStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Division)

                        .Include(e => e.Employee.GeoStruct.Location)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)

                        .Include(e => e.Employee.GeoStruct.Department)
                        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                        .Include(e => e.Employee.GeoStruct.Group)

                        .Include(e => e.Employee.GeoStruct.Unit)


                        .Where(e => EmpLeaveIdList.Contains(e.Employee.Id))
                        .ToList();
                    if (OLvLedger == null || OLvLedger.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvLedger)
                        {
                            if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                            {
                                var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                if (OLvReq != null && OLvReq.Count() != 0)
                                {
                                    foreach (var ca1 in OLvReq)
                                    {


                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            //Fld1 = ca1.Id.ToString(),
                                            Fld2 = ca1.Id.ToString(),
                                            Fld3 = ca1.LeaveHead.LvCode,
                                            Fld4 = ca1.LeaveHead.LvName,
                                            Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                            Fld6 = ca1.ReqDate.ToString(),
                                            Fld7 = ca1.FromDate.ToString(),
                                            Fld8 = ca1.FromStat.LookupVal.ToUpper(),
                                            Fld9 = ca1.ToDate.ToString(),
                                            Fld10 = ca1.ToStat.LookupVal.ToUpper(),
                                            Fld11 = ca1.DebitDays.ToString(),
                                            Fld12 = ca1.ResumeDate.ToString(),
                                            Fld13 = ca1.OpenBal.ToString(),
                                            Fld14 = ca1.CloseBal.ToString(),
                                            Fld15 = ca1.InputMethod == null ? "" : ca1.InputMethod.ToString(),
                                            Fld16 = ca1.LVCount == null ? "" : ca1.LVCount.ToString(),
                                            Fld17 = ca1.LvOccurances.ToString(),
                                            Fld18 = ca1.PrefixCount == null ? "" : ca1.PrefixCount.ToString(),
                                            Fld19 = ca1.SufixCount == null ? "" : ca1.SufixCount.ToString(),
                                            Fld20 = ca1.TrClosed == null ? "" : ca1.TrClosed.ToString(),
                                            Fld21 = ca1.TrReject == null ? "" : ca1.TrReject.ToString(),
                                            Fld22 = ca1.WFStatus == null ? "" : ca1.WFStatus.LookupVal.ToUpper(),
                                            Fld23 = ca1.IsCancel == null ? "" : ca1.IsCancel.ToString(),
                                            Fld24 = ca1.ContactNo == null ? "" : ca1.ContactNo.FullContactNumbers,
                                            Fld25 = ca1.Reason.ToString(),


                                            Fld26 = ca.Id.ToString(),
                                            Fld27 = ca.Employee.EmpCode,
                                            Fld28 = ca.Employee.EmpName.FullNameFML,
                                            Fld29 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Id.ToString(),
                                            //Fld19=ca.Employee.GeoStruct.Division==null?"":ca.Employee.GeoStruct.Division.Code,
                                            Fld30 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                                            Fld31 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.Id.ToString(),
                                            //Fld22=ca.Employee.GeoStruct.Location==null?"":ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                            Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld33 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.Id.ToString(),
                                            //Fld25=ca.Employee.GeoStruct.Department==null?"":ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                            Fld34 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld35 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Id.ToString(),
                                            //Fld28=ca.Employee.GeoStruct.Group==null?"":ca.Employee.GeoStruct.Group.Code,
                                            Fld36 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                                            Fld37 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Id.ToString(),
                                            //Fld31=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                                            Fld38 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                                            Fld39 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                            //Fld34=ca.Employee.FuncStruct.Job==null?"":ca.Employee.FuncStruct.Job.Code,
                                            Fld40 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                                            Fld41 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                                            //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                            Fld42 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                            Fld43 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                            Fld44 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                            Fld45 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                                            //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                            Fld46 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                            Fld47 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                                            Fld48 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld49 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                        };
                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    }
                                }
                            }
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;




                case "LVCANCELREQ":

                    var OLvCancelReqData = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                        .Include(e => e.LvNewReq.Select(r => r.FromStat))
                        .Include(e => e.LvNewReq.Select(r => r.ToStat))
                        .Include(e => e.LvNewReq.Select(r => r.LvOrignal))
                        //.Include(e => e.LvNewReq.Select(r => r.FuncStruct))
                        //.Include(e => e.LvNewReq.Select(r => r.GeoStruct))
                        //.Include(e => e.LvNewReq.Select(r => r.PayStruct))
                        .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)

                        .Include(e => e.Employee.FuncStruct.JobPosition)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)

                        .Include(e => e.Employee.PayStruct.Level)

                        .Include(e => e.Employee.PayStruct.JobStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Division)

                        .Include(e => e.Employee.GeoStruct.Location)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)

                        .Include(e => e.Employee.GeoStruct.Department)
                        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                        .Include(e => e.Employee.GeoStruct.Group)

                        .Include(e => e.Employee.GeoStruct.Unit)


                        .Where(e => EmpLeaveIdList.Contains(e.Employee.Id))
                        .ToList();

                    if (OLvCancelReqData == null || OLvCancelReqData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OLvCancelReqData)
                        {
                            if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                            {
                                var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value <= mToDate)).ToList();
                                if (OLvReq != null && OLvReq.Count() != 0)
                                {
                                    foreach (var ca1 in OLvReq)
                                    {


                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            //Fld1 = ca1.Id.ToString(),
                                            Fld2 = ca1.Id.ToString(),
                                            Fld3 = ca1.LeaveHead.LvCode,
                                            Fld4 = ca1.LeaveHead.LvName,
                                            Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                            Fld6 = ca1.ReqDate.ToString(),
                                            Fld7 = ca1.FromDate.ToString(),
                                            Fld8 = ca1.FromStat.LookupVal.ToUpper(),
                                            Fld9 = ca1.ToDate.ToString(),
                                            Fld10 = ca1.ToStat.LookupVal.ToUpper(),
                                            Fld11 = ca1.DebitDays.ToString(),
                                            Fld12 = ca1.ResumeDate.ToString(),
                                            Fld13 = ca1.OpenBal.ToString(),
                                            Fld14 = ca1.CloseBal.ToString(),
                                            Fld15 = ca1.InputMethod == null ? "" : ca1.InputMethod.ToString(),
                                            Fld16 = ca1.LVCount == null ? "" : ca1.LVCount.ToString(),
                                            Fld17 = ca1.LvOccurances.ToString(),
                                            Fld18 = ca1.PrefixCount == null ? "" : ca1.PrefixCount.ToString(),
                                            Fld19 = ca1.SufixCount == null ? "" : ca1.SufixCount.ToString(),
                                            Fld20 = ca1.TrClosed == null ? "" : ca1.TrClosed.ToString(),
                                            Fld21 = ca1.TrReject == null ? "" : ca1.TrReject.ToString(),
                                            Fld22 = ca1.WFStatus == null ? "" : ca1.WFStatus.LookupVal.ToUpper(),
                                            Fld23 = ca1.IsCancel == null ? "" : ca1.IsCancel.ToString(),
                                            Fld24 = ca1.ContactNo == null ? "" : ca1.ContactNo.FullContactNumbers,
                                            Fld25 = ca1.Reason.ToString(),


                                            Fld26 = ca.Id.ToString(),
                                            Fld27 = ca.Employee.EmpCode,
                                            Fld28 = ca.Employee.EmpName.FullNameFML,
                                            Fld29 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Id.ToString(),
                                            //Fld19=ca.Employee.GeoStruct.Division==null?"":ca.Employee.GeoStruct.Division.Code,
                                            Fld30 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                                            Fld31 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.Id.ToString(),
                                            //Fld22=ca.Employee.GeoStruct.Location==null?"":ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                            Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                            Fld33 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.Id.ToString(),
                                            //Fld25=ca.Employee.GeoStruct.Department==null?"":ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                            Fld34 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                            Fld35 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Id.ToString(),
                                            //Fld28=ca.Employee.GeoStruct.Group==null?"":ca.Employee.GeoStruct.Group.Code,
                                            Fld36 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                                            Fld37 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Id.ToString(),
                                            //Fld31=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                                            Fld38 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                                            Fld39 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                            //Fld34=ca.Employee.FuncStruct.Job==null?"":ca.Employee.FuncStruct.Job.Code,
                                            Fld40 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                                            Fld41 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                                            //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                            Fld42 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                            Fld43 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                            Fld44 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                            Fld45 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                                            //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                            Fld46 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                            Fld47 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                                            Fld48 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                            Fld49 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                            Fld50 = ca1.LvOrignal.FullDetails,

                                        };
                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    }
                                }
                            }
                        }
                        return OGenericLvTransStatement;
                    }
                    return null;
                    break;

                //case "LeaveEncashReq":

                //    var OLeaveEncashReqData = db.EmployeeLeave
                //        .Include(e => e.LeaveEncashReq)
                //        .Include(e => e.LeaveEncashReq.Select(r => r.LvNewReq))
                //        .Include(e => e.LeaveEncashReq.Select(r => r.LvHead))
                //        .Include(e => e.LeaveEncashReq.Select(r => r.LeaveCalendar))
                //        .Include(e => e.LeaveEncashReq.Select(r => r.WFStatus))

                //        //.Include(e => e.LvNewReq.Select(r => r.FuncStruct))
                //        //.Include(e => e.LvNewReq.Select(r => r.GeoStruct))
                //        //.Include(e => e.LvNewReq.Select(r => r.PayStruct))

                //        .Include(e => e.Employee)
                //        .Include(e => e.Employee.EmpName)
                //        .Include(e => e.Employee.FuncStruct)
                //        .Include(e => e.Employee.FuncStruct.Job)

                //        .Include(e => e.Employee.FuncStruct.JobPosition)
                //        .Include(e => e.Employee.PayStruct)
                //        .Include(e => e.Employee.PayStruct.Grade)

                //        .Include(e => e.Employee.PayStruct.Level)

                //        .Include(e => e.Employee.PayStruct.JobStatus)
                //        .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                //        .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                //        .Include(e => e.Employee.GeoStruct)
                //        .Include(e => e.Employee.GeoStruct.Division)

                //        .Include(e => e.Employee.GeoStruct.Location)
                //        .Include(e => e.Employee.GeoStruct.Location.LocationObj)

                //        .Include(e => e.Employee.GeoStruct.Department)
                //        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                //        .Include(e => e.Employee.GeoStruct.Group)

                //        .Include(e => e.Employee.GeoStruct.Unit)


                //        .Where(e => EmpLeaveIdList.Contains(e.Employee.Id))
                //        .ToList();
                //    if (OLeaveEncashReqData == null || OLeaveEncashReqData.Count() == 0)
                //    {
                //        return null;
                //    }
                //    else
                //    {
                //        foreach (var ca in OLeaveEncashReqData)
                //        {
                //            if (ca.LeaveEncashReq != null && ca.LeaveEncashReq.Count() != 0)
                //            {
                //                var OLvEncashReq = ca.LeaveEncashReq.Where(e => (e.ReqDate >= mFromDate && e.ReqDate <= mToDate)).ToList();
                //                if (OLvEncashReq != null && OLvEncashReq.Count() != 0)
                //                {
                //                    foreach (var ca1 in OLvEncashReq)
                //                    {


                //                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                //                        //write data to generic class
                //                        {
                //                            Fld1 = ca1.Id.ToString(),
                //                            Fld2 = ca1.Id.ToString(),
                //                            Fld3 = ca1.LvHead.LvCode,
                //                            Fld4 = ca1.LvHead.LvName,
                //                            Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                //                            Fld6 = ca1.ReqDate.ToString(),
                //                            Fld7 = ca1.FromPeriod.ToString(),

                //                            Fld9 = ca1.ToPeriod.ToString(),

                //                            Fld11 = ca1.EncashDays.ToString(),
                //                            Fld12 = ca1.LvNewReq == null ? "" : ca1.LvNewReq.FullDetails,
                //                            Fld13 = ca1.Narration.ToString(),

                //                            Fld15 = ca1.InputMethod == null ? "" : ca1.InputMethod.ToString(),

                //                            Fld20 = ca1.TrClosed == null ? "" : ca1.TrClosed.ToString(),
                //                            Fld21 = ca1.TrReject == null ? "" : ca1.TrReject.ToString(),
                //                            Fld22 = ca1.WFStatus == null ? "" : ca1.WFStatus.LookupVal.ToUpper(),



                //                            Fld26 = ca.Id.ToString(),
                //                            Fld27 = ca.Employee.EmpCode,
                //                            Fld28 = ca.Employee.EmpName.FullNameFML,
                //                            Fld29 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Id.ToString(),
                //                            //Fld19=ca.Employee.GeoStruct.Division==null?"":ca.Employee.GeoStruct.Division.Code,
                //                            Fld30 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                //                            Fld31 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.Id.ToString(),
                //                            //Fld22=ca.Employee.GeoStruct.Location==null?"":ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                //                            Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                //                            Fld33 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.Id.ToString(),
                //                            //Fld25=ca.Employee.GeoStruct.Department==null?"":ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                //                            Fld34 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                //                            Fld35 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Id.ToString(),
                //                            //Fld28=ca.Employee.GeoStruct.Group==null?"":ca.Employee.GeoStruct.Group.Code,
                //                            Fld36 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                //                            Fld37 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Id.ToString(),
                //                            //Fld31=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                //                            Fld38 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                //                            Fld39 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                //                            //Fld34=ca.Employee.FuncStruct.Job==null?"":ca.Employee.FuncStruct.Job.Code,
                //                            Fld40 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                //                            Fld41 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                //                            //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                //                            Fld42 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                //                            Fld43 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                //                            //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                //                            Fld44 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                //                            Fld45 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                //                            //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                //                            Fld46 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                //                            Fld47 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                //                            Fld48 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                //                            Fld49 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),

                //                        };
                //                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                //                    }
                //                }
                //            }
                //        }
                //        return OGenericLvTransStatement;
                //    }
                //    return null;
                //    break;


                default:
                    return null;
                    break;
            }
        }
        public static List<GenericField100> GenerateAnnualStatementReport(int CompanyPayrollId, List<int> EmpPayrollIdList, string mObjectName, DateTime mFromDate, DateTime mToDate, DataBaseContext db)
        {
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            switch (mObjectName)
            {
                case "ANNUALSALARY":
                    var OAnnualSalaryData = db.EmployeePayroll
                        .Include(e => e.AnnualSalary)
                        .Include(e => e.AnnualSalary.Select(t => t.AnnualSalaryDetailsR))
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.AnnualSalary.Select(t => t.FinancialYear))
                        //.Include(e => e.AnnualSalary.Select(t => t.EmpName))
                        .Include(e => e.AnnualSalary.Select(t => t.GeoStruct.Location.LocationObj))
                        .Include(e => e.AnnualSalary.Select(t => t.GeoStruct.Department.DepartmentObj))
                        .Include(e => e.AnnualSalary.Select(t => t.FuncStruct.Job))
                        .Include(e => e.AnnualSalary.Select(t => t.FuncStruct.JobPosition))
                        .Include(e => e.AnnualSalary.Select(t => t.PayStruct.Grade))
                        .Include(e => e.AnnualSalary.Select(t => t.PayStruct.JobStatus))

                        .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                        .ToList();

                    if (OAnnualSalaryData == null || OAnnualSalaryData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OAnnualSalaryData)
                        {
                            if (ca.AnnualSalary != null && ca.AnnualSalary.Count() != 0)
                            {
                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OAnnualSalData = ca.AnnualSalary.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                // e.FinancialYear.FromDate == mstart && e.FinancialYear.ToDate.Value >= mend).ToList();

                                // foreach (var ca1 in ca.AnnualSalary)//.Where(e=> mPayMonth.Contains(e.FinancialYear))
                                foreach (var ca1 in OAnnualSalData)
                                {


                                    if (ca1.AnnualSalaryDetailsR != null && ca1.AnnualSalaryDetailsR.Count() != 0)
                                    {
                                        var OAnnualSalaryDetails = ca1.AnnualSalaryDetailsR; //.Where(e=> mPayMonth.Contains(e.) )
                                        //var m = mPayMonth.Count();

                                        foreach (var ca2 in OAnnualSalaryDetails)
                                        {

                                            GenericField100 oGenericField100 = new GenericField100()
                                            {
                                                Fld1 = ca1.EmpCode,
                                                Fld2 = ca1.EmpName,
                                                Fld3 = ca1.FinancialYear.FromDate.Value.ToShortDateString(),
                                                Fld4 = ca1.FinancialYear.ToDate.Value.ToShortDateString(),


                                                Fld11 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Id.ToString(),
                                                Fld12 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Code,
                                                Fld13 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                                Fld14 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.Id.ToString(),
                                                Fld15 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocCode,
                                                Fld16 = ca1.GeoStruct.Location == null ? "" : ca1.GeoStruct.Location.LocationObj.LocDesc,
                                                Fld17 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                                                Fld18 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                                                Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                                Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                                                Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                                                Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                                Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                                Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                                Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                                Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                                                Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                                                Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                                Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                                                Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                                                Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                                Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                                                Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                                                Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                                Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                                                Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                                                Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                                Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                                                Fld39 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                Fld40 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                                //Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                                //Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                                //Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                                Fld5 = ca2.SalCode,
                                                Fld6 = ca2.SalHeadDesc,
                                                Fld7 = ca2.SalType,
                                                Fld8 = ca2.ITAppl.ToString(),
                                                Fld9 = ca2.PaySlipAppl.ToString(),
                                                Fld10 = ca2.SalAmountTotal.ToString(),

                                                Fld51 = ca2.AmountM01.ToString(),
                                                Fld52 = ca2.AmountM02.ToString(),
                                                Fld53 = ca2.AmountM03.ToString(),
                                                Fld54 = ca2.AmountM04.ToString(),
                                                Fld55 = ca2.AmountM05.ToString(),
                                                Fld56 = ca2.AmountM06.ToString(),
                                                Fld57 = ca2.AmountM07.ToString(),
                                                Fld58 = ca2.AmountM08.ToString(),
                                                Fld59 = ca2.AmountM09.ToString(),
                                                Fld60 = ca2.AmountM10.ToString(),
                                                Fld61 = ca2.AmountM11.ToString(),
                                                Fld62 = ca2.AmountM12.ToString(),
                                                Fld63 = ca2.SalAmountTotal.ToString()

                                            };

                                            OGenericPayrollStatement.Add(oGenericField100);
                                            //if (ca1.AnnualSalaryDetailsR != null && ca1.AnnualSalaryDetailsR.Count() != 0)
                                            //{
                                            //    var OAnnualSalaryDetails=ca1.AnnualSalaryDetailsR; //.Where(e=> mPayMonth.Contains(e.) )
                                            //        foreach ( var ca2 in OAnnualSalaryDetails)
                                            //        {
                                            //            GenericField100 ooGenericField100 = new GenericField100()
                                            //            {
                                            //                Fld5 = ca2.SalCode,
                                            //                Fld6 = ca2.SalHeadDesc,
                                            //                Fld7 = ca2.SalType,
                                            //                Fld8 = ca2.ITAppl.ToString(),
                                            //                Fld9 = ca2.PaySlipAppl.ToString(),
                                            //                Fld10 = ca2.SalAmountTotal.ToString(),

                                            //                Fld51 = ca2.AmountM01.ToString(),
                                            //                Fld52 = ca2.AmountM02.ToString(),
                                            //                Fld53 = ca2.AmountM03.ToString(),
                                            //                Fld54 = ca2.AmountM04.ToString(),
                                            //                Fld55 = ca2.AmountM05.ToString(),
                                            //                Fld56 = ca2.AmountM06.ToString(),
                                            //                Fld57 = ca2.AmountM07.ToString(),
                                            //                Fld58 = ca2.AmountM08.ToString(),
                                            //                Fld59 = ca2.AmountM09.ToString(),
                                            //                Fld60 = ca2.AmountM10.ToString(),
                                            //                Fld61 = ca2.AmountM11.ToString(),
                                            //                Fld62 = ca2.AmountM12.ToString(),
                                            //                Fld63 = ca2.SalAmountTotal.ToString()
                                            //            };

                                            //        }

                                            //}
                                        }

                                    }
                                }
                            }

                        }

                        return OGenericPayrollStatement;
                    }


                    break;

                case "ITPROJECTION":

                    //case "ITTRANST":
                    var OITPROJECT = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.ITProjection)
                        .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                         .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        .Include(e => e.Employee.PayStruct.Grade)
                        .Include(e => e.Employee.FuncStruct.Job)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();

                    //.Include(e=>e.)


                    OGenericPayrollStatement = new List<GenericField100>();
                    if (OITPROJECT == null || OITPROJECT.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OITPROJECT)
                        {
                            var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                            var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                            var OITPROJECTData = ca.ITProjection.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();

                            foreach (var ca1 in OITPROJECTData)
                            {
                                GenericField100 OGenericObjStatement = new GenericField100()
                                //write data to generic class
                                {

                                    Fld1 = ca1.Id.ToString(),
                                    Fld2 = ca1.FromPeriod.ToString(),
                                    Fld3 = ca1.ToPeriod.ToString(),
                                    Fld4 = (ca1.Tiltle == null ? "" : ca1.Tiltle.ToString()) + (ca1.ChapterName == null ? "" : ca1.ChapterName.ToString()).ToString(),
                                    Fld5 = ca1.Form16Header == null ? "" : ca1.Form16Header,
                                    Fld6 = ca1.Form24Header == null ? "" : ca1.Form24Header.ToString(),
                                    Fld7 = ca1.SectionType == null ? "" : ca1.SectionType.ToString(),
                                    Fld8 = ca1.Section == null ? "" : ca1.Section.ToString(),
                                    Fld9 = ca1.ChapterName == null ? "" : ca1.ChapterName.ToString(),
                                    Fld10 = ca1.SubChapter == null ? "" : ca1.SubChapter.ToString(),

                                    Fld11 = ca1.SalayHead == null ? "" : ca1.SalayHead.ToString(),
                                    Fld12 = ca1.ProjectedAmount == null ? "" : ca1.ProjectedAmount.ToString(),
                                    Fld13 = ca1.ActualAmount == null ? "" : ca1.ActualAmount.ToString(),
                                    Fld14 = ca1.ProjectedQualifyingAmount == null ? "" : ca1.ProjectedQualifyingAmount.ToString(),
                                    Fld15 = ca1.ActualQualifyingAmount == null ? "" : ca1.ActualQualifyingAmount.ToString(),
                                    Fld16 = ca1.QualifiedAmount == null ? "" : ca1.QualifiedAmount.ToString(),
                                    Fld17 = ca1.TDSComponents == null ? "" : ca1.TDSComponents.ToString(),
                                    Fld18 = ca1.ProjectionDate == null ? "" : ca1.ProjectionDate.ToString(),
                                    Fld19 = ca1.IsLocked == null ? "" : ca1.IsLocked.ToString(),
                                    Fld20 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                    Fld21 = ca1.PickupId == null ? "" : ca1.PickupId.ToString(),
                                    Fld22 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToString(),



                                    Fld26 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                    Fld27 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                    Fld28 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                    Fld29 = ca.Employee.GeoStruct.Location.LocationObj.Id == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.Id.ToString(),
                                    Fld30 = ca.Employee.GeoStruct.Location.LocationObj.LocCode == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                                    Fld31 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),

                                    //Fld32 = ca.Employee.GeoStruct.Department.DepartmentObj.Id == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString(),
                                    //Fld33 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString(),
                                    //Fld34 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString(),

                                    Fld35 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                    Fld36 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                                    Fld37 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                                    //Fld38 = ca.Employee.FuncStruct.Job.Id == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                    //Fld39 = ca.Employee.FuncStruct.Job.Code == null ? "" : ca.Employee.FuncStruct.Job.Code.ToString(),
                                    //Fld40 = ca.Employee.FuncStruct.Job.Name == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),

                                    Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.ToString(),
                                    Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.ToString(),
                                    Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.ToString(),
                                    //Fld31 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.Id.ToString(),
                                    //Fld32 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptCode,
                                    //Fld19 = ca1.GeoStruct.Department == null ? "" : ca1.GeoStruct.Department.DepartmentObj.DeptDesc,
                                    //Fld20 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Id.ToString(),
                                    //Fld21 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Code,
                                    //Fld22 = ca1.GeoStruct.Group == null ? "" : ca1.GeoStruct.Group.Name,
                                    //Fld23 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                    //Fld24 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                    //Fld25 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,

                                    //Fld26 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Id.ToString(),
                                    //Fld27 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Code,
                                    //Fld28 = ca1.FuncStruct.Job == null ? "" : ca1.FuncStruct.Job.Name,

                                    //Fld29 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.Id.ToString(),
                                    //Fld30 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionCode,
                                    //Fld31 = ca1.FuncStruct.JobPosition == null ? "" : ca1.FuncStruct.JobPosition.JobPositionDesc,

                                    //Fld32 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Id.ToString(),
                                    //Fld33 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Code,
                                    //Fld34 = ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,

                                    //Fld35 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Id.ToString(),
                                    //Fld36 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Code,
                                    //Fld37 = ca1.PayStruct.Level == null ? "" : ca1.PayStruct.Level.Name,

                                    //Fld38 = ca1.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.Id.ToString(),
                                    //Fld39 = ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                    //Fld40 = ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                    ////Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                    ////Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                    ////Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,



                                };
                                OGenericPayrollStatement.Add(OGenericObjStatement);
                            }
                        }
                        //}
                        //else
                        //{
                        //    //return null;
                        //}

                    }
                    //else
                    //{
                    //    return null;
                    //}

                    //}
                    return OGenericPayrollStatement;
                    //}
                    break;



                case "BONUSCHKT":
                    var OBonusChkData = db.EmployeePayroll
                       .Include(e => e.Employee)
                       .Include(e => e.Employee.EmpName)
                       .Include(e => e.BonusChkT)

                    //    .Include(e => e.BonusChkT.Select(t => t.GeoStruct.Location.LocationObj))
                        //.Include(e => e.BonusChkT.Select(t => t.GeoStruct.Department.DepartmentObj))
                        //.Include(e => e.BonusChkT.Select(t => t.FuncStruct.Job))
                        //.Include(e => e.BonusChkT.Select(t => t.FuncStruct.JobPosition))
                        //.Include(e => e.BonusChkT.Select(t => t.PayStruct.Grade))
                        //.Include(e => e.BonusChkT.Select(t => t.PayStruct.JobStatus))
                          .Include(e => e.BonusChkT.Select(r => r.BonusCalendar))
                        //.Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                         //.Include(e => e.Employee.GeoStruct.Location)

                         //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        // .Include(e => e.Employee.GeoStruct.Department)
                        //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        //.Include(e => e.Employee.PayStruct)
                         .Include(e => e.Employee.PayStruct.Grade)
                        //.Include(e => e.Employee.FuncStruct)
                         .Include(e => e.Employee.FuncStruct.Job)
                        //.Include(e => e.Employee.ServiceBookDates)
                         .Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                    .ToList();


                    if (OBonusChkData == null || OBonusChkData.Count() == 0)
                    {
                        return null;
                    }
                    else
                    {
                        foreach (var ca in OBonusChkData)
                        {
                            if (ca.BonusChkT == null || ca.BonusChkT.Count() != 0)
                            {
                                var OBonusChkDatails = ca.BonusChkT.ToList();
                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OBonusSalData = ca.BonusChkT.Where(e => e.BonusCalendar.FromDate.Value <= mstart && e.BonusCalendar.ToDate.Value >= mend).ToList();


                                if (OBonusSalData != null && OBonusSalData.Count() != 0)
                                {

                                    foreach (var ca1 in OBonusSalData)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            //Fld1 = ca1.ActualService == null ? "" : ca1.ActualService.ToString(),
                                            //Fld2 = ca1.RoundedService == null ? "" : ca1.RoundedService.ToString(),
                                            //Fld3 = ca1.Amount == null ? "" : ca1.Amount.ToString(),
                                            //Fld4 = ca1.ProcessDate == null ? "" : ca1.ProcessDate.Value.ToShortDateString(),
                                            //Fld5 = ca1.TotalLWP == null ? "" : ca1.TotalLWP.ToString(),


                                            Fld1 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                            //Fld4 = ca.Employee.GeoStruct.Location.LocationObj.Id == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.Id.ToString(),
                                            //Fld5 = ca.Employee.GeoStruct.Location.LocationObj.LocCode == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                                            //Fld6 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),

                                            //Fld7 = ca.Employee.GeoStruct.Department.DepartmentObj.Id == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString(),
                                            //Fld8 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString(),
                                            //Fld9 = ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc == null ? "" : ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString(),

                                            //Fld10 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            //Fld11 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                                            //Fld12 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                                            Fld4 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null ? ca.Employee.GeoStruct.Location.Id.ToString() : null,
                                            Fld5 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocCode : null,
                                            Fld6 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : null,

                                            Fld7 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString() : null,
                                            Fld8 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString() : null,
                                            Fld9 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,

                                            Fld10 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Id.ToString() : null,
                                            Fld11 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Code.ToString() : null,
                                            Fld12 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,


                                            //Fld13 = ca.Employee.FuncStruct.Job.Id == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                            //Fld14 = ca.Employee.FuncStruct.Job.Code == null ? "" : ca.Employee.FuncStruct.Job.Code.ToString(),
                                            //Fld15 = ca.Employee.FuncStruct.Job.Name == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),

                                            Fld13 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Id.ToString() : null,
                                            Fld14 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Code : null,
                                            Fld15 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : null,


                                            //Fld16 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                            //Fld17 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                            //Fld18 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                            //Fld19 = ca.Employee.ServiceBookDates.BirthDate == null ? "" : ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),


                                            Fld25 = ca1.ProcessDate == null ? "" : ca1.ProcessDate.Value.ToShortDateString(),

                                            Fld26 = ca1.TotalBonusWages == null ? "" : ca1.TotalBonusWages.ToString(),
                                            Fld27 = ca1.TotalWorkingDays == null ? "" : ca1.TotalWorkingDays.ToString(),
                                            Fld28 = ca1.TotalBonus == null ? "" : ca1.TotalBonus.ToString(),

                                            Fld29 = ca1.TotalExGracia == null ? "" : ca1.TotalExGracia.ToString(),
                                            Fld30 = ca1.TotalAmount.ToString(),

                                            Fld31 = ca1.BonusWages_01.ToString(),

                                            Fld32 = ca1.WorkingDays_01.ToString(),
                                            Fld33 = ca1.Bonus_01.ToString(),
                                            Fld34 = ca1.ExGracia_01.ToString(),

                                            Fld35 = ca1.TotalAmount_01.ToString(),
                                            Fld36 = ca1.BonusWages_02.ToString(),
                                            Fld37 = ca1.WorkingDays_02.ToString(),

                                            Fld38 = ca1.Bonus_02.ToString(),
                                            Fld39 = ca1.ExGracia_02.ToString(),
                                            Fld40 = ca1.TotalAmount_02.ToString(),

                                            Fld41 = ca1.BonusWages_03.ToString(),
                                            Fld42 = ca1.WorkingDays_03.ToString(),
                                            Fld43 = ca1.Bonus_03.ToString(),
                                            Fld44 = ca1.ExGracia_03.ToString(),
                                            Fld45 = ca1.TotalAmount_03.ToString(),

                                            Fld46 = ca1.BonusWages_04.ToString(),
                                            Fld47 = ca1.WorkingDays_04.ToString(),
                                            Fld48 = ca1.Bonus_04.ToString(),
                                            Fld49 = ca1.ExGracia_04.ToString(),
                                            Fld50 = ca1.TotalAmount_04.ToString(),

                                            Fld51 = ca1.BonusWages_05.ToString(),
                                            Fld52 = ca1.WorkingDays_05.ToString(),
                                            Fld53 = ca1.Bonus_05.ToString(),
                                            Fld54 = ca1.ExGracia_05.ToString(),
                                            Fld55 = ca1.TotalAmount_06.ToString(),

                                            Fld56 = ca1.BonusWages_06.ToString(),
                                            Fld57 = ca1.WorkingDays_06.ToString(),
                                            Fld58 = ca1.Bonus_06.ToString(),
                                            Fld59 = ca1.ExGracia_06.ToString(),
                                            Fld60 = ca1.TotalAmount_06.ToString(),

                                            Fld61 = ca1.BonusWages_07.ToString(),
                                            Fld62 = ca1.WorkingDays_07.ToString(),
                                            Fld63 = ca1.Bonus_07.ToString(),
                                            Fld64 = ca1.ExGracia_07.ToString(),
                                            Fld65 = ca1.TotalAmount_07.ToString(),

                                            Fld66 = ca1.BonusWages_08.ToString(),
                                            Fld67 = ca1.WorkingDays_08.ToString(),
                                            Fld68 = ca1.Bonus_08.ToString(),
                                            Fld69 = ca1.ExGracia_08.ToString(),
                                            Fld70 = ca1.TotalAmount_08.ToString(),

                                            Fld71 = ca1.BonusWages_09.ToString(),
                                            Fld72 = ca1.WorkingDays_09.ToString(),
                                            Fld73 = ca1.Bonus_09.ToString(),
                                            Fld74 = ca1.ExGracia_09.ToString(),
                                            Fld75 = ca1.TotalAmount_09.ToString(),

                                            Fld76 = ca1.BonusWages_10.ToString(),
                                            Fld77 = ca1.WorkingDays_10.ToString(),
                                            Fld78 = ca1.Bonus_10.ToString(),
                                            Fld79 = ca1.ExGracia_10.ToString(),
                                            Fld80 = ca1.TotalAmount_10.ToString(),

                                            Fld81 = ca1.BonusWages_11.ToString(),
                                            Fld82 = ca1.WorkingDays_11.ToString(),
                                            Fld83 = ca1.Bonus_11.ToString(),
                                            Fld84 = ca1.ExGracia_11.ToString(),
                                            Fld85 = ca1.TotalAmount_11.ToString(),

                                            Fld86 = ca1.BonusWages_12.ToString(),
                                            Fld87 = ca1.WorkingDays_12.ToString(),
                                            Fld88 = ca1.Bonus_12.ToString(),
                                            Fld89 = ca1.ExGracia_12.ToString(),
                                            Fld90 = ca1.TotalAmount_12.ToString()
                                            //Fld41 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Id.ToString(),
                                            //Fld42 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Code,
                                            //Fld43 = ca1.GeoStruct.Unit == null ? "" : ca1.GeoStruct.Unit.Name,


                                        };
                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                    }
                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            //else
                            //{
                            //    return null;
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
        public static EmployeePayroll _returnGetEmployeeData(Int32 item, string MonYr)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //////var OPayslipData_temp = db.EmployeePayroll
                //////               .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(a => a.GeoStruct.Location.LocationObj)))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailEarnR)))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailDedR)))
                //////           .Include(e => e.SalaryT.Select(r => r.PayslipR.Select(t => t.PaySlipDetailLeaveR)))
                //////           .Where(e => e.Employee.Id == item).AsParallel()
                //////           .FirstOrDefault();
                //////return OPayslipData_temp;

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
