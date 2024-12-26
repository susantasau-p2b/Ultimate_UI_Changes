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

namespace P2BUltimate.Process
{
    public class PayrollTransactionEntryReportGen
    {
        public static List<GenericField100> GenerateTransactionEntryReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime pFromDate, DateTime pToDate, DateTime mFromDate, DateTime mToDate, string ProcessType)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {


                    case "REGIMESCHEME":

                        var ORegimeScheme = new List<EmployeePayroll>();
                        var RegimeschemeR = db.EmployeePayroll.Select(r => new
                        {
                            iEmployeeId = r.Employee.Id,
                            iEmpCode = r.Employee.EmpCode,
                            iEmpName = r.Employee.EmpName.FullNameFML,
                            //iLocation = r.Employee.GeoStruct.Location.LocationObj.LocDesc,
                            //iLocCode = r.Employee.GeoStruct.Location.LocationObj.LocCode,
                            //iDeptCode = r.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                            //iDepartment = r.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                            //iDesignation = r.Employee.FuncStruct.Job.Name,
                            iGeoStructId = r.Employee.GeoStruct_Id,
                            iPayStructId = r.Employee.PayStruct_Id,
                            iFuncStructId = r.Employee.FuncStruct_Id,

                            iRegimeScheme = r.RegimiScheme.Select(v => new
                            {
                                Scheme = v.Scheme.LookupVal,
                                FYFromDate = v.FinancialYear.FromDate,
                                FYToDate = v.FinancialYear.ToDate

                            }).ToList()

                        }).Where(e => EmpPayrollIdList.Contains(e.iEmployeeId)).ToList();

                        if (RegimeschemeR != null)
                        {

                        }

                        if (RegimeschemeR == null || RegimeschemeR.Count() == 0)
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

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in RegimeschemeR)
                            {
                                int? geoid = ca.iGeoStructId;

                                int? payid = ca.iPayStructId;

                                int? funid = ca.iFuncStructId;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                foreach (var item in ca.iRegimeScheme.Where(r=>r.FYFromDate>=mFromDate && r.FYToDate<=mToDate))
                                {
                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld4 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                        Fld5 = ca.iEmpName == null ? "" : ca.iEmpName,
                                        Fld6 = GeoDataInd.GeoStruct_Location_Name,
                                        Fld7 = GeoDataInd.GeoStruct_Department_Name,
                                        Fld8 = GeoDataInd.FuncStruct_Job_Name,
                                        Fld9 = item.Scheme,
                                        Fld10 = GeoDataInd.GeoStruct_Location_Code,
                                        Fld11 = GeoDataInd.GeoStruct_Department_Code



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
                                    if (emp)
                                    {
                                        OGenericObjStatement.Fld88 = ca.iEmpName;
                                    }

                                    OGenericPayrollStatement.Add(OGenericObjStatement);

                                }
                            }

                        }

                        return OGenericPayrollStatement;



                        break;
                    case "SALATTENDANCE":
                        var OSalAttendanceData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OSalAttendanceData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.SalAttendance)
                                // .Include(e => e.SalAttendance.Select(t => t.GeoStruct))
                                // .Include(e => e.SalAttendance.Select(t => t.PayStruct))
                                // .Include(e => e.SalAttendance.Select(t => t.FuncStruct))
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OSalAttendanceData_temp != null)
                            {
                                OSalAttendanceData_temp.Employee.EmpName = db.NameSingle.Find(OSalAttendanceData_temp.Employee.EmpName_Id);
                                OSalAttendanceData_temp.SalAttendance = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == OSalAttendanceData_temp.Id).AsNoTracking().ToList();
                                foreach (var Attitem in OSalAttendanceData_temp.SalAttendance)
                                {
                                    Attitem.GeoStruct = db.GeoStruct.Find(Attitem.GeoStruct_Id);
                                    Attitem.PayStruct = db.PayStruct.Find(Attitem.PayStruct_Id);
                                    Attitem.FuncStruct = db.FuncStruct.Find(Attitem.FuncStruct_Id);
                                }

                                OSalAttendanceData.Add(OSalAttendanceData_temp);
                            }
                        }


                        if (OSalAttendanceData == null || OSalAttendanceData.Count() == 0)
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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OSalAttendanceData)
                            {
                                if (ca.SalAttendance != null && ca.SalAttendance.Count() != 0)
                                {
                                    var OSalAttend = ca.SalAttendance.ToList();

                                    //if (OSalAttend != null && OSalAttend.Count() != 0)
                                    //{

                                    int? geoid = OSalAttend.FirstOrDefault().GeoStruct_Id;

                                    int? payid = OSalAttend.FirstOrDefault().PayStruct_Id;

                                    int? funid = OSalAttend.FirstOrDefault().FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                    //int geoid = OSalAttend.FirstOrDefault().GeoStruct.Id;

                                    //int payid = OSalAttend.FirstOrDefault().PayStruct.Id;

                                    //int funid = OSalAttend.FirstOrDefault().FuncStruct.Id;

                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    if (GeoDataInd != null)
                                    {

                                        /////new
                                        DateTime pFMonYear = Convert.ToDateTime(pFromDate.ToString("MM/yyyy"));
                                        DateTime pTMonYear = Convert.ToDateTime(pToDate.ToString("MM/yyyy"));
                                        foreach (var ca1 in OSalAttend.Where(e => Convert.ToDateTime(e.PayMonth) >= pFMonYear && Convert.ToDateTime(e.PayMonth) <= pTMonYear))
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
                                                Fld11 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name/////new

                                            };

                                            //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                            if (month)
                                            {
                                                OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                            }
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
                                            if (emp)
                                            {
                                                OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }


                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                            //  }
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

                    case "CPIENTRYT":

                        var OCPIEntryData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OCPIEntryData_temp = db.EmployeePayroll
                                                        .Include(e => e.Employee)
                                                        .Include(e => e.Employee.GeoStruct)
                                                        .Include(e => e.Employee.FuncStruct)
                                                        .Include(e => e.Employee.PayStruct)
                                                        .Include(e => e.Employee.EmpName)
                                                        .Include(e => e.CPIEntryT)
                                                        .Where(e => e.Employee.Id == item)
                                                        .AsNoTracking()
                                                        .FirstOrDefault();

                            if (OCPIEntryData_temp != null)
                            {
                                OCPIEntryData_temp.Employee.EmpName = db.NameSingle.Find(OCPIEntryData_temp.Employee.EmpName_Id);
                                OCPIEntryData_temp.CPIEntryT = db.CPIEntryT.Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OCPIEntryData_temp.Id).AsNoTracking().ToList();
                                OCPIEntryData.Add(OCPIEntryData_temp);
                            }
                        }

                        if (OCPIEntryData == null || OCPIEntryData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);
                            foreach (var cp in OCPIEntryData)
                            {
                                int geoid = cp.Employee.GeoStruct.Id;

                                int payid = cp.Employee.PayStruct.Id;

                                int funid = cp.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);


                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                {
                                    var CPI = cp.CPIEntryT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();


                                    foreach (var cp1 in CPI)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld11 = cp1.CalIndexPoint == null ? "" : cp1.CalIndexPoint.ToString(),
                                            Fld7 = cp1.ActualIndexPoint == null ? "" : cp1.ActualIndexPoint.ToString(),
                                            Fld2 = cp.Employee.EmpCode == null ? "" : cp.Employee.EmpCode.ToString(),
                                            Fld3 = cp.Employee.EmpName.FullNameFML == null ? "" : cp.Employee.EmpName.FullNameFML
                                        };
                                        if (month)
                                        {
                                            OGenericObjStatement.Fld100 = cp1.PayMonth.ToString();
                                        }
                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                            OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                            OGenericObjStatement.Fld88 = cp.Employee.ToString();
                                        }

                                        OGenericPayrollStatement.Add(OGenericObjStatement);

                                    }
                                }


                            }
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "TADAADVANCECLAIM":
                        var OTADAAdvanceClaimData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OTADAAdvanceClaimData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.TADAAdvanceClaim)
                            .Include(e => e.TADAAdvanceClaim.Select(x => x.TADAType))
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OTADAAdvanceClaimData_temp != null)
                            {
                                OTADAAdvanceClaimData.Add(OTADAAdvanceClaimData_temp);
                            }
                        }


                        if (OTADAAdvanceClaimData == null || OTADAAdvanceClaimData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OTADAAdvanceClaimData)
                            {


                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                {

                                    /////new


                                    //var ltcsettlementdata = ca2.LTCAdvanceClaim.Where(e => e.TravelStartDate.Value > pFromDate && pToDate <= e.TravelEndDate.Value).ToList();
                                    var tadaadvdata = ca.TADAAdvanceClaim.Where(e => e.DateOfAppl >= pFromDate && e.DateOfAppl <= pToDate).ToList();
                                    foreach (var item in tadaadvdata)
                                    {
                                        // item.
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString(),
                                            Fld2 = ca.Employee.EmpCode,
                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            Fld4 = item.TravelEndDate == null ? "" : item.TravelEndDate.Value.ToShortDateString(),
                                            Fld5 = item.TravelStartDate == null ? "" : item.TravelStartDate.Value.ToShortDateString(),
                                            Fld6 = item.ProposedPlace == null ? "" : item.ProposedPlace,
                                            Fld7 = item.EligibleAmt == null ? "" : item.EligibleAmt.ToString(),
                                            Fld8 = item.AdvAmt == null ? "" : item.AdvAmt.ToString(),
                                            Fld9 = item.SanctionedAmt == null ? "" : item.SanctionedAmt.ToString(),
                                            Fld10 = item.Remark == null ? "" : item.Remark.ToString(),
                                            Fld11 = item.DateOfAppl == null ? "" : item.DateOfAppl.Value.ToShortDateString(),/////new
                                            Fld12 = item.TADAType == null ? "" : item.TADAType.LookupVal.ToString(),/////new

                                        };

                                        //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                        if (month)
                                        {
                                            // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                        }
                                        if (comp)
                                        {
                                            OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                        }
                                        if (dept)
                                        {
                                            OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                        }
                                        if (grp)
                                        {
                                            OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                            OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                            OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                        }

                                        OGenericPayrollStatement.Add(OGenericObjStatement);
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
                    case "LTCADVANCECLAIM":
                        var OLTCAdvanceClaimData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OLTCAdvanceClaimData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.EmpLTCBlock)
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCAdvanceClaim)))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCAdvanceClaim.Select(a => a.LTC_TYPE))))
                                //  .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCSettlementClaim)))
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OLTCAdvanceClaimData_temp != null)
                            {
                                OLTCAdvanceClaimData.Add(OLTCAdvanceClaimData_temp);
                            }
                        }


                        if (OLTCAdvanceClaimData == null || OLTCAdvanceClaimData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OLTCAdvanceClaimData)
                            {
                                if (ca.EmpLTCBlock != null && ca.EmpLTCBlock.Count() != 0)
                                {
                                    var OEmpLTCBlock = ca.EmpLTCBlock
                                        .ToList();

                                    if (OEmpLTCBlock != null && OEmpLTCBlock.Count() != 0)
                                    {

                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                        {

                                            /////new

                                            foreach (var ca1 in OEmpLTCBlock)
                                            {
                                                foreach (var ca2 in ca1.EmpLTCBlockT)
                                                {
                                                    //var ltcsettlementdata = ca2.LTCAdvanceClaim.Where(e => e.TravelStartDate.Value > pFromDate && pToDate <= e.TravelEndDate.Value).ToList();
                                                    var ltcsettlementdata = ca2.LTCAdvanceClaim.ToList();
                                                    foreach (var item in ltcsettlementdata)
                                                    {
                                                        // item.
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = ca.Employee.Id.ToString(),
                                                            Fld2 = ca.Employee.EmpCode,
                                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                            Fld4 = item.TravelEndDate == null ? "" : item.TravelEndDate.Value.ToShortDateString(),
                                                            Fld5 = item.TravelStartDate == null ? "" : item.TravelStartDate.Value.ToShortDateString(),
                                                            Fld6 = item.ProposedPlace == null ? "" : item.ProposedPlace,
                                                            Fld7 = item.LTC_Eligible_Amt == null ? "" : item.LTC_Eligible_Amt.ToString(),
                                                            Fld8 = item.LTCAdvAmt == null ? "" : item.LTCAdvAmt.ToString(),
                                                            Fld9 = item.LTCSanctionedAmt == null ? "" : item.LTCSanctionedAmt.ToString(),
                                                            Fld10 = item.Remark == null ? "" : item.Remark.ToString(),
                                                            Fld11 = item.DateOfAppl == null ? "" : item.DateOfAppl.Value.ToShortDateString(),/////new
                                                            Fld12 = item.LTC_TYPE == null ? "" : item.LTC_TYPE.LookupVal.ToString(),/////new

                                                        };

                                                        //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                        if (month)
                                                        {
                                                            // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                        }
                                                        if (comp)
                                                        {
                                                            OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                            OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                            OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
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
                    case "TADASETTLEMENTCLAIM":
                        var OTADASETTLEClaimData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OTADAAdvanceClaimData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.TADASettlementClaim)
                             .Include(e => e.TADASettlementClaim.Select(a => a.TADAAdvanceClaim))
                            .Include(e => e.TADASettlementClaim.Select(a => a.TADAType))
                            .Include(e => e.TADASettlementClaim.Select(a => a.JourneyDetails))
                            .Include(e => e.TADASettlementClaim.Select(a => a.JourneyDetails.DAObject))
                            .Include(e => e.TADASettlementClaim.Select(a => a.JourneyDetails.MisExpenseObject))
                            .Include(e => e.TADASettlementClaim.Select(a => a.JourneyDetails.Travel_Hotel_Booking))
                            .Include(e => e.TADASettlementClaim.Select(a => a.JourneyDetails.Travel_Hotel_Booking.Select(x => x.HotelObject)))
                            .Include(e => e.TADASettlementClaim.Select(a => a.JourneyDetails.JourneyObject))

                          //  .Include(e => e.TADASettlementClaim.Select(a => a.Travel_Hotel_Booking))

                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OTADAAdvanceClaimData_temp != null)
                            {
                                OTADASETTLEClaimData.Add(OTADAAdvanceClaimData_temp);
                            }
                        }


                        if (OTADASETTLEClaimData == null || OTADASETTLEClaimData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OTADASETTLEClaimData)
                            {

                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                {

                                    /////new


                                    var tadasettlementdata = ca.TADASettlementClaim.Where(e => e.DateOfAppl >= pFromDate && e.DateOfAppl <= pToDate && e.TrReject == false).ToList();
                                    foreach (var item in tadasettlementdata)
                                    {
                                        DateTime Jstart = item.JourneyDetails.JourneyStart.Value.Date;
                                        DateTime Jend = item.JourneyDetails.JourneyEnd.Value.Date;
                                        double misexptot = 0;
                                        double Staytot = 0;
                                        double Datot = 0;
                                        for (DateTime? mTempDate = Jstart; mTempDate <= Jend; mTempDate = mTempDate.Value.AddDays(1))
                                        {

                                            var journeyhotel = item.JourneyDetails.Travel_Hotel_Booking.Select(e => e.HotelObject).FirstOrDefault();
                                            var journeyhotelobjtot = journeyhotel != null ? journeyhotel.Sum(e => e.HotelSettleAmt) : 0;
                                            string hotelsanction = "0";
                                            if (journeyhotel != null)
                                            {
                                                var journeyhotelobj = journeyhotel.Where(e => e.HotelDate.Value.Date == mTempDate.Value.Date).FirstOrDefault();


                                                if (journeyhotelobj != null)
                                                {
                                                    hotelsanction = journeyhotelobj.HotelSettleAmt.ToString();
                                                }
                                            }
                                            string advamt = item.TADAAdvanceClaim == null ? "0" : item.TADAAdvanceClaim.SanctionedAmt.ToString();
                                            var journeyobj = item.JourneyDetails.JourneyObject.Where(e => e.FromDate.Value.Date == mTempDate.Value.Date).ToList();

                                            var journeydaobj = item.JourneyDetails.DAObject.Where(e => e.DADate.Value.Date == mTempDate.Value.Date).FirstOrDefault();
                                            var journeymisobj = item.JourneyDetails.MisExpenseObject.Where(e => e.MisExpenseDate.Value.Date == mTempDate.Value.Date).FirstOrDefault();

                                            var journeymisobjtot = item.JourneyDetails.MisExpenseObject.Count() > 0 ? item.JourneyDetails.MisExpenseObject.Sum(e => e.MisExpenseSettleAmt) : 0;
                                            var journeydaobjtot = item.JourneyDetails.DAObject.Count() > 0 ? item.JourneyDetails.DAObject.Sum(e => e.DASettleAmt) : 0;



                                            if (journeyobj != null && journeyobj.Count() > 0)
                                            {

                                                foreach (var jo in journeyobj)
                                                {
                                                    //var journydet=item.JourneyDetails.
                                                    // item.
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.Id.ToString(),
                                                        Fld2 = ca.Employee.EmpCode,
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                        Fld4 = FuncdataInd.Job_Name,
                                                        Fld5 = PaydataInd.Grade_Name,
                                                        Fld6 = mTempDate.Value.Date.ToShortDateString(),
                                                        Fld7 = journeyobj == null ? "" : jo.PlaceFrom,
                                                        Fld8 = journeyobj == null ? "" : jo.FromDate.Value.ToShortTimeString(),
                                                        Fld9 = journeyobj == null ? "" : jo.PlaceTo,
                                                        Fld10 = journeyobj == null ? "" : jo.ToDate.Value.ToShortTimeString(),
                                                        Fld11 = journeyobj == null ? "0" : jo.JourneyDist.ToString(),
                                                        Fld12 = item.TADAType.LookupVal.ToString(),
                                                        Fld13 = journeyobj == null ? "0" : jo.TASettleAmt.ToString(),
                                                        Fld14 = journeymisobj == null ? "0" : journeymisobj.MisExpenseSettleAmt.ToString(),
                                                        Fld15 = hotelsanction,
                                                        Fld16 = journeydaobj == null ? "0" : journeydaobj.DASettleAmt.ToString(),
                                                        Fld17 = item.SettlementAmt == null ? "0" : item.SettlementAmt.ToString(),
                                                        Fld18 = item.Remark == null ? "" : item.Remark.ToString(),
                                                        Fld19 = journeymisobjtot != null ? journeymisobjtot.ToString() : "0",
                                                        Fld20 = journeyhotelobjtot != null ? journeyhotelobjtot.ToString() : "0",
                                                        Fld21 = journeydaobjtot != null ? journeydaobjtot.ToString() : "0",
                                                        Fld22 = advamt,
                                                        Fld23 = item.DateOfAppl.Value.Date.ToShortDateString(),

                                                    };

                                                    //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                    if (month)
                                                    {
                                                        // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                        OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                        OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }
                                            else
                                            {
                                                // not journey but hotel book
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode,
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    Fld4 = FuncdataInd.Job_Name,
                                                    Fld5 = PaydataInd.Grade_Name,
                                                    Fld6 = mTempDate.Value.Date.ToShortDateString(),
                                                    Fld7 = journeyobj == null ? "" : "",
                                                    Fld8 = journeyobj == null ? "" : "",
                                                    Fld9 = journeyobj == null ? "" : "",
                                                    Fld10 = journeyobj == null ? "" : "",
                                                    Fld11 = journeyobj == null ? "0" : "0",
                                                    Fld12 = item.TADAType.LookupVal.ToString(),
                                                    Fld13 = journeyobj == null ? "0" : "0",
                                                    Fld14 = journeymisobj == null ? "0" : journeymisobj.MisExpenseSettleAmt.ToString(),
                                                    Fld15 = hotelsanction,
                                                    Fld16 = journeydaobj == null ? "0" : journeydaobj.DASettleAmt.ToString(),
                                                    Fld17 = item.SettlementAmt == null ? "0" : item.SettlementAmt.ToString(),
                                                    Fld18 = item.Remark == null ? "" : item.Remark.ToString(),
                                                    Fld19 = journeymisobjtot != null ? journeymisobjtot.ToString() : "0",
                                                    Fld20 = journeyhotelobjtot != null ? journeyhotelobjtot.ToString() : "0",
                                                    Fld21 = journeydaobjtot != null ? journeydaobjtot.ToString() : "0",
                                                    Fld22 = advamt,
                                                    Fld23 = item.DateOfAppl.Value.Date.ToShortDateString(),


                                                };

                                                //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                if (month)
                                                {
                                                    // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                }
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                    OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);

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
                    case "LTCSETTLEMENTCLAIM":
                        var OLTCSETTLEClaimData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OLTCAdvanceClaimData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.EmpLTCBlock)
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT))
                                //.Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCAdvanceClaim)))
                                //.Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCAdvanceClaim.Select(a => a.LTC_TYPE))))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCSettlementClaim)))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCSettlementClaim.Select(a => a.LTC_TYPE))))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCSettlementClaim.Select(a => a.JourneyDetails))))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT.Select(t => t.LTCSettlementClaim.Select(a => a.Travel_Hotel_Booking))))
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OLTCAdvanceClaimData_temp != null)
                            {
                                OLTCSETTLEClaimData.Add(OLTCAdvanceClaimData_temp);
                            }
                        }


                        if (OLTCSETTLEClaimData == null || OLTCSETTLEClaimData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OLTCSETTLEClaimData)
                            {
                                if (ca.EmpLTCBlock != null && ca.EmpLTCBlock.Count() != 0)
                                {
                                    var OEmpLTCBlock = ca.EmpLTCBlock
                                        .ToList();

                                    if (OEmpLTCBlock != null && OEmpLTCBlock.Count() != 0)
                                    {

                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                        {

                                            /////new

                                            foreach (var ca1 in OEmpLTCBlock)
                                            {
                                                foreach (var ca2 in ca1.EmpLTCBlockT)
                                                {
                                                    var ltcsettlementdata = ca2.LTCSettlementClaim.ToList();
                                                    foreach (var item in ltcsettlementdata)
                                                    {
                                                        // item.
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = ca.Employee.Id.ToString(),
                                                            Fld2 = ca.Employee.EmpCode,
                                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                            Fld4 = item.DateOfAppl == null ? "" : item.DateOfAppl.Value.ToShortDateString(),
                                                            Fld5 = item.EncashmentAmount == null ? "" : item.EncashmentAmount.ToString(),
                                                            Fld6 = item.JourneyDetails == null ? "" : item.JourneyDetails.FullDetails,
                                                            Fld7 = item.LTC_Eligible_Amt == null ? "" : item.LTC_Eligible_Amt.ToString(),
                                                            Fld8 = item.LTCAdvAmt == null ? "" : item.LTCAdvAmt.ToString(),
                                                            Fld9 = item.NoOfDays == null ? "" : item.NoOfDays.ToString(),
                                                            Fld10 = item.Remark == null ? "" : item.Remark.ToString(),
                                                            Fld11 = item.LTC_Claim_Amt == null ? "" : item.LTC_Claim_Amt.ToString(),
                                                            Fld12 = item.LTC_TYPE == null ? "" : item.LTC_TYPE.LookupVal.ToString(),/////new
                                                            Fld13 = item.LTC_Settle_Amt == null ? "" : item.LTC_Settle_Amt.ToString(),/////new
                                                            Fld14 = item.LTC_Sanction_Amt == null ? "" : item.LTC_Sanction_Amt.ToString(),/////new

                                                        };

                                                        //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                        if (month)
                                                        {
                                                            // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                        }
                                                        if (comp)
                                                        {
                                                            OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                            OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                            OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
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

                    case "HOTELBOOKINGREQUEST":
                        var OHotelbookData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OHotelbookData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.HotelBookingRequest)
                            .Include(e => e.HotelBookingRequest.Select(m => m.State))
                            .Include(e => e.HotelBookingRequest.Select(m => m.Country))
                            .Include(e => e.HotelBookingRequest.Select(m => m.City))
                            .Include(e => e.HotelBookingRequest.Select(m => m.FamilyDetails))
                            .Include(e => e.HotelBookingRequest.Select(m => m.FamilyDetails.Select(x => x.MemberName)))
                            .Include(e => e.EmpLTCBlock.Select(r => r.EmpLTCBlockT))

                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OHotelbookData_temp != null)
                            {
                                OHotelbookData.Add(OHotelbookData_temp);
                            }
                        }


                        if (OHotelbookData == null || OHotelbookData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OHotelbookData)
                            {
                                if (ca.HotelBookingRequest != null && ca.HotelBookingRequest.Count() != 0)
                                {
                                    var OEmpHotel = ca.HotelBookingRequest.Where(e => e.StartDate >= pFromDate && e.EndDate <= pToDate)
                                        .ToList();

                                    if (OEmpHotel != null && OEmpHotel.Count() != 0)
                                    {

                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                        {

                                            /////new

                                            foreach (var ca1 in OEmpHotel)
                                            {

                                                var ltcsettlementdata = ca1.FamilyDetails.Select(x => x.MemberName).ToList();

                                                if (ltcsettlementdata != null && ltcsettlementdata.Count() > 0)
                                                {


                                                    foreach (var item in ltcsettlementdata)
                                                    {
                                                        // item.
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = ca.Employee.Id.ToString(),
                                                            Fld2 = ca.Employee.EmpCode,
                                                            Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                            Fld4 = ca1.StartDate == null ? "" : ca1.StartDate.Value.ToShortDateString(),
                                                            Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                            Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),
                                                            Fld7 = ca1.Country == null ? "" : ca1.Country.Name.ToString(),
                                                            Fld8 = ca1.State == null ? "" : ca1.State.Name.ToString(),
                                                            Fld9 = ca1.City == null ? "" : ca1.City.Name.ToString(),
                                                            Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                            Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                            Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                            Fld13 = ca1.Eligible_BillAmount == null ? "" : ca1.Eligible_BillAmount.ToString(),/////new
                                                            Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                            Fld15 = item.FullNameFML == null ? "" : item.FullNameFML.ToString(),
                                                            Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                        };

                                                        //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                        if (month)
                                                        {
                                                            // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                        }
                                                        if (comp)
                                                        {
                                                            OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                            OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                            OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                }
                                                else
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.Id.ToString(),
                                                        Fld2 = ca.Employee.EmpCode,
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                        Fld4 = ca1.StartDate == null ? "" : ca1.StartDate.Value.ToShortDateString(),
                                                        Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                        Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),
                                                        Fld7 = ca1.Country == null ? "" : ca1.Country.Name.ToString(),
                                                        Fld8 = ca1.State == null ? "" : ca1.State.Name.ToString(),
                                                        Fld9 = ca1.City == null ? "" : ca1.City.Name.ToString(),
                                                        Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                        Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                        Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                        Fld13 = ca1.Eligible_BillAmount == null ? "" : ca1.Eligible_BillAmount.ToString(),/////new
                                                        Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                        Fld15 = "",
                                                        Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),


                                                    };

                                                    //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                    if (month)
                                                    {
                                                        // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                        OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                        OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                    }

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
                    case "TICKETBOOKINGREQUEST":
                        var OTicketbookData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OTicketbookData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.TicketBookingRequest)
                            .Include(e => e.TicketBookingRequest.Select(m => m.JourneyDetails))
                            .Include(e => e.TicketBookingRequest.Select(m => m.JourneyDetails.Select(x => x.JourneyObject)))
                            .Include(e => e.TicketBookingRequest.Select(m => m.JourneyDetails.Select(c => c.FamilyDetails)))
                            .Include(e => e.TicketBookingRequest.Select(m => m.JourneyDetails.Select(c => c.FamilyDetails.Select(z => z.MemberName))))
                            .Include(e => e.TicketBookingRequest.Select(m => m.FamilyDetails))
                            .Include(e => e.TicketBookingRequest.Select(m => m.FamilyDetails.Select(x => x.MemberName)))
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OTicketbookData_temp != null)
                            {
                                OTicketbookData.Add(OTicketbookData_temp);
                            }
                        }


                        if (OTicketbookData == null || OTicketbookData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OTicketbookData)
                            {
                                if (ca.TicketBookingRequest != null && ca.TicketBookingRequest.Count() != 0)
                                {
                                    var OEmpTicket = ca.TicketBookingRequest.Where(e => e.ReqDate >= pFromDate && e.ReqDate <= pToDate)
                                        .ToList();

                                    if (OEmpTicket != null && OEmpTicket.Count() != 0)
                                    {

                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                        {

                                            /////new

                                            foreach (var ca1 in OEmpTicket)
                                            {
                                                if (ca1.JourneyDetails.Count() > 0)
                                                {

                                                    foreach (var item in ca1.JourneyDetails)
                                                    {
                                                        var jc = item.JourneyObject.ToList();

                                                        foreach (var item1 in jc)
                                                        {
                                                            if (item.FamilyDetails.Count() > 0)
                                                            {


                                                                foreach (var item2 in item.FamilyDetails)
                                                                {
                                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                                    //write data to generic class
                                                                    {
                                                                        Fld1 = ca.Employee.Id.ToString(),
                                                                        Fld2 = ca.Employee.EmpCode,
                                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                                        Fld4 = ca1.ReqDate == null ? "" : ca1.ReqDate.Value.ToShortDateString(),
                                                                        // Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                                        //Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),

                                                                        Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                                        //Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                                        Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                                        Fld13 = ca1.Elligible_BillAmount == null ? "" : ca1.Elligible_BillAmount.ToString(),/////new
                                                                        Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                                        Fld17 = item.JourneyStart == null ? "" : item.JourneyStart.Value.ToShortDateString(),
                                                                        Fld18 = item.JourneyEnd == null ? "" : item.JourneyEnd.Value.ToShortDateString(),
                                                                        Fld19 = item1.PlaceFrom == null ? "" : item1.PlaceFrom.ToString(),
                                                                        Fld20 = item1.PlaceTo == null ? "" : item1.PlaceTo.ToString(),
                                                                        Fld21 = item1.JourneyDist == null ? "" : item1.JourneyDist.ToString(),
                                                                        Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                                        Fld15 = item2.MemberName == null ? "" : item2.MemberName.FullNameFML.ToString(),
                                                                    };

                                                                    //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                                    if (month)
                                                                    {
                                                                        // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                                    }
                                                                    if (comp)
                                                                    {
                                                                        OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                                    }
                                                                    if (div)
                                                                    {
                                                                        OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                                    }
                                                                    if (loca)
                                                                    {
                                                                        OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                                    }
                                                                    if (dept)
                                                                    {
                                                                        OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                                    }
                                                                    if (grp)
                                                                    {
                                                                        OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                                    }
                                                                    if (unit)
                                                                    {
                                                                        OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                                        OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                                        OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                    }

                                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                //write data to generic class
                                                                {
                                                                    Fld1 = ca.Employee.Id.ToString(),
                                                                    Fld2 = ca.Employee.EmpCode,
                                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                                    Fld4 = ca1.ReqDate == null ? "" : ca1.ReqDate.Value.ToShortDateString(),
                                                                    // Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                                    //Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),

                                                                    Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                                    //Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                                    Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                                    Fld13 = ca1.Elligible_BillAmount == null ? "" : ca1.Elligible_BillAmount.ToString(),/////new
                                                                    Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                                    Fld17 = item.JourneyStart == null ? "" : item.JourneyStart.Value.ToShortDateString(),
                                                                    Fld18 = item.JourneyEnd == null ? "" : item.JourneyEnd.Value.ToShortDateString(),
                                                                    Fld19 = item1.PlaceFrom == null ? "" : item1.PlaceFrom.ToString(),
                                                                    Fld20 = item1.PlaceTo == null ? "" : item1.PlaceTo.ToString(),
                                                                    Fld21 = item1.JourneyDist == null ? "" : item1.JourneyDist.ToString(),
                                                                    Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                                    Fld15 = "",
                                                                };

                                                                //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                                if (month)
                                                                {
                                                                    // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                                }
                                                                if (comp)
                                                                {
                                                                    OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                                }
                                                                if (div)
                                                                {
                                                                    OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                                }
                                                                if (loca)
                                                                {
                                                                    OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                                }
                                                                if (dept)
                                                                {
                                                                    OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                                }
                                                                if (grp)
                                                                {
                                                                    OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                                }
                                                                if (unit)
                                                                {
                                                                    OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                                    OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                                    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                }

                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            }

                                                        }



                                                    }

                                                }
                                                else
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.Id.ToString(),
                                                        Fld2 = ca.Employee.EmpCode,
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                        Fld4 = ca1.ReqDate == null ? "" : ca1.ReqDate.Value.ToShortDateString(),
                                                        // Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                        //Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),

                                                        Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                        //Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                        Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                        Fld13 = ca1.Elligible_BillAmount == null ? "" : ca1.Elligible_BillAmount.ToString(),/////new
                                                        Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                        Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                    };

                                                    //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                    if (month)
                                                    {
                                                        // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                        OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                        OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }



                                        }

                                    }


                                }


                            }
                            return OGenericPayrollStatement;
                        }
                        break;
                    case "VEHICLEBOOKINGREQUEST":
                        var OVehiclebookData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OVehiclebookData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.VehicleBookingRequest)
                            .Include(e => e.VehicleBookingRequest.Select(m => m.JourneyDetails))
                            .Include(e => e.VehicleBookingRequest.Select(m => m.JourneyDetails.Select(x => x.JourneyObject)))
                            .Include(e => e.VehicleBookingRequest.Select(m => m.JourneyDetails.Select(c => c.FamilyDetails)))
                            .Include(e => e.VehicleBookingRequest.Select(m => m.JourneyDetails.Select(c => c.FamilyDetails.Select(z => z.MemberName))))
                            .Include(e => e.VehicleBookingRequest.Select(m => m.AgencyAddress))
                            .Include(e => e.VehicleBookingRequest.Select(m => m.AgencyContactDetails))
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OVehiclebookData_temp != null)
                            {
                                OVehiclebookData.Add(OVehiclebookData_temp);
                            }
                        }


                        if (OVehiclebookData == null || OVehiclebookData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            //////////// new

                            foreach (var ca in OVehiclebookData)
                            {
                                if (ca.VehicleBookingRequest != null && ca.VehicleBookingRequest.Count() != 0)
                                {
                                    var OEmpVehicle = ca.VehicleBookingRequest.Where(e => e.ReqDate >= pFromDate && e.ReqDate <= pToDate)
                                        .ToList();

                                    if (OEmpVehicle != null && OEmpVehicle.Count() != 0)
                                    {

                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                        {

                                            /////new

                                            foreach (var ca1 in OEmpVehicle)
                                            {
                                                if (ca1.JourneyDetails.Count() > 0)
                                                {

                                                    foreach (var item in ca1.JourneyDetails)
                                                    {
                                                        var jc = item.JourneyObject.ToList();

                                                        foreach (var item1 in jc)
                                                        {
                                                            if (item.FamilyDetails.Count() > 0)
                                                            {


                                                                foreach (var item2 in item.FamilyDetails)
                                                                {
                                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                                    //write data to generic class
                                                                    {
                                                                        Fld1 = ca.Employee.Id.ToString(),
                                                                        Fld2 = ca.Employee.EmpCode,
                                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                                        Fld4 = ca1.ReqDate == null ? "" : ca1.ReqDate.Value.ToShortDateString(),
                                                                        // Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                                        //Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),
                                                                        Fld25 = ca1.AgencyName == null ? "" : ca1.AgencyName.ToString(),
                                                                        Fld24 = ca1.ContactPerson == null ? "" : ca1.ContactPerson.ToString(),
                                                                        Fld23 = ca1.AgencyAddress == null ? "" : ca1.AgencyAddress.FullAddress.ToString(),
                                                                        Fld22 = ca1.AgencyContactDetails == null ? "" : ca1.AgencyContactDetails.FullContactDetails.ToString(),
                                                                        Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                                        //Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                                        Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                                        Fld13 = ca1.Elligible_BillAmount == null ? "" : ca1.Elligible_BillAmount.ToString(),/////new
                                                                        Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                                        Fld17 = item.JourneyStart == null ? "" : item.JourneyStart.Value.ToShortDateString(),
                                                                        Fld18 = item.JourneyEnd == null ? "" : item.JourneyEnd.Value.ToShortDateString(),
                                                                        Fld19 = item1.PlaceFrom == null ? "" : item1.PlaceFrom.ToString(),
                                                                        Fld20 = item1.PlaceTo == null ? "" : item1.PlaceTo.ToString(),
                                                                        Fld21 = item1.JourneyDist == null ? "" : item1.JourneyDist.ToString(),
                                                                        Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                                        Fld15 = item2.MemberName == null ? "" : item2.MemberName.FullNameFML.ToString(),
                                                                    };

                                                                    //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                                    if (month)
                                                                    {
                                                                        // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                                    }
                                                                    if (comp)
                                                                    {
                                                                        OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                                    }
                                                                    if (div)
                                                                    {
                                                                        OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                                    }
                                                                    if (loca)
                                                                    {
                                                                        OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                                    }
                                                                    if (dept)
                                                                    {
                                                                        OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                                    }
                                                                    if (grp)
                                                                    {
                                                                        OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                                    }
                                                                    if (unit)
                                                                    {
                                                                        OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                                        OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                                        OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                    }

                                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                //write data to generic class
                                                                {
                                                                    Fld1 = ca.Employee.Id.ToString(),
                                                                    Fld2 = ca.Employee.EmpCode,
                                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                                    Fld4 = ca1.ReqDate == null ? "" : ca1.ReqDate.Value.ToShortDateString(),
                                                                    // Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                                    //Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),
                                                                    Fld25 = ca1.AgencyName == null ? "" : ca1.AgencyName.ToString(),
                                                                    Fld24 = ca1.ContactPerson == null ? "" : ca1.ContactPerson.ToString(),
                                                                    Fld23 = ca1.AgencyAddress == null ? "" : ca1.AgencyAddress.FullAddress.ToString(),
                                                                    Fld22 = ca1.AgencyContactDetails == null ? "" : ca1.AgencyContactDetails.FullContactDetails.ToString(),
                                                                    Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                                    //Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                                    Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                                    Fld13 = ca1.Elligible_BillAmount == null ? "" : ca1.Elligible_BillAmount.ToString(),/////new
                                                                    Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                                    Fld17 = item.JourneyStart == null ? "" : item.JourneyStart.Value.ToShortDateString(),
                                                                    Fld18 = item.JourneyEnd == null ? "" : item.JourneyEnd.Value.ToShortDateString(),
                                                                    Fld19 = item1.PlaceFrom == null ? "" : item1.PlaceFrom.ToString(),
                                                                    Fld20 = item1.PlaceTo == null ? "" : item1.PlaceTo.ToString(),
                                                                    Fld21 = item1.JourneyDist == null ? "" : item1.JourneyDist.ToString(),
                                                                    Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                                    Fld15 = "",
                                                                };

                                                                //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                                if (month)
                                                                {
                                                                    // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                                }
                                                                if (comp)
                                                                {
                                                                    OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                                }
                                                                if (div)
                                                                {
                                                                    OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                                }
                                                                if (loca)
                                                                {
                                                                    OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                                }
                                                                if (dept)
                                                                {
                                                                    OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                                }
                                                                if (grp)
                                                                {
                                                                    OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                                }
                                                                if (unit)
                                                                {
                                                                    OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                                    OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                                    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                }

                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            }

                                                        }



                                                    }

                                                }
                                                else
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.Id.ToString(),
                                                        Fld2 = ca.Employee.EmpCode,
                                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                        Fld4 = ca1.ReqDate == null ? "" : ca1.ReqDate.Value.ToShortDateString(),
                                                        // Fld5 = ca1.EndDate == null ? "" : ca1.EndDate.Value.ToShortDateString(),
                                                        //Fld6 = ca1.HotelDesc == null ? "" : ca1.HotelDesc.ToString(),
                                                        Fld25 = ca1.AgencyName == null ? "" : ca1.AgencyName.ToString(),
                                                        Fld24 = ca1.ContactPerson == null ? "" : ca1.ContactPerson.ToString(),
                                                        Fld23 = ca1.AgencyAddress == null ? "" : ca1.AgencyAddress.FullAddress.ToString(),
                                                        Fld22 = ca1.AgencyContactDetails == null ? "" : ca1.AgencyContactDetails.FullContactDetails.ToString(),
                                                        Fld10 = ca1.BillNo == null ? "" : ca1.BillNo.ToString(),
                                                        //Fld11 = ca1.NoOfRooms == null ? "" : ca1.NoOfRooms.ToString(),
                                                        Fld12 = ca1.TotFamilyMembers == null ? "" : ca1.TotFamilyMembers.ToString(),/////new
                                                        Fld13 = ca1.Elligible_BillAmount == null ? "" : ca1.Elligible_BillAmount.ToString(),/////new
                                                        Fld14 = ca1.BillAmount == null ? "" : ca1.BillAmount.ToString(),/////new
                                                        Fld16 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                    };

                                                    //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                    if (month)
                                                    {
                                                        // OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                        OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                        OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }



                                        }

                                    }


                                }


                            }
                            return OGenericPayrollStatement;
                        }
                        break;
                    case "FUNCTATTENDANCET":

                        var OFuncAttData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OFuncAttData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.FunctAttendanceT)
                                //.Include(r => r.FunctAttendanceT.Select(t => t.SalaryHead))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.GeoStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.FuncStruct))
                                //.Include(r => r.FunctAttendanceT.Select(t => t.PayStruct))
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();
                            if (OFuncAttData_temp != null)
                            {
                                OFuncAttData_temp.Employee.EmpName = db.NameSingle.Find(OFuncAttData_temp.Employee.EmpName_Id);
                                OFuncAttData_temp.FunctAttendanceT = db.FunctAttendanceT.Include(e => e.FunctAllWFDetails).Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OFuncAttData_temp.Id).AsNoTracking().ToList();

                                foreach (var Attitem in OFuncAttData_temp.FunctAttendanceT)
                                {
                                    Attitem.GeoStruct = db.GeoStruct.Find(Attitem.GeoStruct_Id);
                                    Attitem.PayStruct = db.PayStruct.Find(Attitem.PayStruct_Id);
                                    Attitem.FuncStruct = db.FuncStruct.Find(Attitem.FuncStruct_Id);
                                    Attitem.SalaryHead = db.SalaryHead.Find(Attitem.SalaryHead_Id);

                                }


                                OFuncAttData.Add(OFuncAttData_temp);
                            }
                        }
                        if (OFuncAttData == null || OFuncAttData.Count() == 0)
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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OFuncAttData)
                            {
                                if (ca.FunctAttendanceT != null && ca.FunctAttendanceT.Count() != 0)
                                {
                                    string Status = string.Empty;
                                    string getBossName = string.Empty;

                                    foreach (var item in mPayMonth)
                                    {
                                        #region salheadlist TRUE and SpecialGroupslist FALSE
                                        if (salheadlist != null && salheadlist.Count() != 0 && SpecialGroupslist.Count() == 0)
                                        {
                                            foreach (var item2 in salheadlist)
                                            {
                                                var OOFuncAtt = ca.FunctAttendanceT.Where(e => item == e.PayMonth && e.SalaryHead.Code == item2.ToString()).ToList();
                                                if (OOFuncAtt != null && OOFuncAtt.Count() != 0)
                                                {

                                                    int? geoid = OOFuncAtt.FirstOrDefault().GeoStruct_Id;

                                                    int? payid = OOFuncAtt.FirstOrDefault().PayStruct_Id;

                                                    int? funid = OOFuncAtt.FirstOrDefault().FuncStruct_Id;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                                    //int geoid = OOFuncAtt.FirstOrDefault().GeoStruct.Id;

                                                    //int payid = OOFuncAtt.FirstOrDefault().PayStruct.Id;

                                                    //int funid = OOFuncAtt.FirstOrDefault().FuncStruct.Id;

                                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                    if (GeoDataInd != null)
                                                    {
                                                        foreach (var ca1 in OOFuncAtt)
                                                        {

                                                            var getFunctAllWFDetails = ca1.FunctAllWFDetails.OrderByDescending(o => o.Id).FirstOrDefault();

                                                            if (getFunctAllWFDetails != null)
                                                            {

                                                                var wStatus = getFunctAllWFDetails.WFStatus;

                                                                var bossName = getFunctAllWFDetails.DBTrack.CreatedBy;
                                                                switch (wStatus)
                                                                {
                                                                    case 0:
                                                                        Status = "Applied";
                                                                        break;
                                                                    case 1:
                                                                        Status = "Sanction Apporved";
                                                                        break;
                                                                    case 2:
                                                                        Status = "Sanction Rejected";
                                                                        break;
                                                                    case 3:
                                                                        Status = "HR Apporved";
                                                                        break;
                                                                    case 4:
                                                                        Status = "HR Rejected";
                                                                        break;
                                                                    case 5:
                                                                        Status = "HRM(M)";
                                                                        break;
                                                                    case 6:
                                                                        Status = "Cancelled";
                                                                        break;
                                                                    case 7:
                                                                        Status = "Recommendation Apporved";
                                                                        break;
                                                                    case 8:
                                                                        Status = "Recommendation Rejected";
                                                                        break;


                                                                }
                                                                int bId = Convert.ToInt32(bossName);
                                                                if (bId != 0)
                                                                {
                                                                    getBossName = db.Employee.Include(e => e.EmpName).Where(e => e.Id == bId).FirstOrDefault().EmpName.FullNameFML.ToString();
                                                                }
                                                                else
                                                                {
                                                                    getBossName = string.Empty;
                                                                }

                                                            }
                                                            else
                                                            {
                                                                Status = "HRM(M)";
                                                            }

                                                            if (Status == "Applied")
                                                            {
                                                                getBossName = "";
                                                            }
                                                            GenericField100 OGenericObjStatement = new GenericField100()
                                                            //write data to generic class
                                                            {
                                                                Fld1 = ca1.Id.ToString(),
                                                                Fld2 = ca.Employee.EmpCode,
                                                                Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                                Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                                                Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                                                Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                                                Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                                                Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                                                Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                                                Fld10 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                                Fld44 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                                Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                                                Fld46 = ca1.PayProcessGroup != null ? ca1.PayProcessGroup.Name : null,
                                                                Fld47 = ca1.Reason != null ? ca1.Reason : null,

                                                                Fld48 = GeoDataInd != null ? GeoDataInd.GeoStruct_Location_Name : "",
                                                                Fld49 = Status != null ? Status : "",
                                                                Fld50 = getBossName != null ? getBossName : "",
                                                                Fld51 = getBossName != null ? "BossName" : "",
                                                                Fld53 = getFunctAllWFDetails!=null?(getFunctAllWFDetails.Comments!=null?getFunctAllWFDetails.Comments:""):"",
                                                                // Fld80=
                                                            };
                                                            //  OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                            if (month)
                                                            {
                                                                OGenericObjStatement.Fld100 = mPayMonth.ToString();
                                                            }
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
                                                            if (emp)
                                                            {
                                                                OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                                //OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                            }

                                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                        }
                                                    }
                                                }
                                                //else
                                                //{
                                                //    //return null;
                                                //}
                                            }
                                        }
                                        #endregion salheadlist TRUE and SpecialGroupslist FALSE

                                        #region salheadlist FALSE and SpecialGroupslist TRUE
                                        if (salheadlist.Count() == 0 && SpecialGroupslist != null && SpecialGroupslist.Count() != 0)
                                        {
                                            foreach (var item2 in SpecialGroupslist)
                                            {
                                                string tempchk = item2;
                                                var OOFuncAtt = ca.FunctAttendanceT.Where(e => item == e.PayMonth).ToList();
                                                if (OOFuncAtt != null && OOFuncAtt.Count() != 0)
                                                {

                                                    int? geoid = OOFuncAtt.FirstOrDefault().GeoStruct_Id;

                                                    int? payid = OOFuncAtt.FirstOrDefault().PayStruct_Id;

                                                    int? funid = OOFuncAtt.FirstOrDefault().FuncStruct_Id;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                                    //int geoid = OOFuncAtt.FirstOrDefault().GeoStruct.Id;

                                                    //int payid = OOFuncAtt.FirstOrDefault().PayStruct.Id;

                                                    //int funid = OOFuncAtt.FirstOrDefault().FuncStruct.Id;

                                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                    if (GeoDataInd != null)
                                                    {
                                                        foreach (var ca1 in OOFuncAtt)
                                                        {

                                                            var getFunctAllWFDetails = ca1.FunctAllWFDetails.OrderByDescending(o => o.Id).FirstOrDefault();

                                                            if (getFunctAllWFDetails != null)
                                                            {

                                                                var wStatus = getFunctAllWFDetails.WFStatus;

                                                                var bossName = getFunctAllWFDetails.DBTrack.CreatedBy;
                                                                switch (wStatus)
                                                                {
                                                                    case 0:
                                                                        Status = "Applied";
                                                                        break;
                                                                    case 1:
                                                                        Status = "Sanction Apporved";
                                                                        break;
                                                                    case 2:
                                                                        Status = "Sanction Rejected";
                                                                        break;
                                                                    case 3:
                                                                        Status = "HR Apporved";
                                                                        break;
                                                                    case 4:
                                                                        Status = "HR Rejected";
                                                                        break;
                                                                    case 5:
                                                                        Status = "HRM(M)";
                                                                        break;
                                                                    case 6:
                                                                        Status = "Cancelled";
                                                                        break;
                                                                    case 7:
                                                                        Status = "Recommendation Apporved";
                                                                        break;
                                                                    case 8:
                                                                        Status = "Recommendation Rejected";
                                                                        break;


                                                                }
                                                                int bId = Convert.ToInt32(bossName);
                                                                if (bId != 0)
                                                                {
                                                                    getBossName = db.Employee.Include(e => e.EmpName).Where(e => e.Id == bId).FirstOrDefault().EmpName.FullNameFML.ToString();
                                                                }
                                                                else
                                                                {
                                                                    getBossName = string.Empty;
                                                                }


                                                            }
                                                            else
                                                            {
                                                                Status = "HRM(M)";
                                                            }
                                                            if (Status == "Applied")
                                                            {
                                                                getBossName = "";
                                                            }
                                                            if (item2 == Status)
                                                            {
                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                //write data to generic class
                                                                {
                                                                    Fld1 = ca1.Id.ToString(),
                                                                    Fld2 = ca.Employee.EmpCode,
                                                                    Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                                    Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                                                    Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                                                    Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                                                    Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                                                    Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                                                    Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                                                    Fld10 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                                    Fld44 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                                    Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                                                    Fld46 = ca1.PayProcessGroup != null ? ca1.PayProcessGroup.Name : null,
                                                                    Fld47 = ca1.Reason != null ? ca1.Reason : null,
                                                                    Fld48 = GeoDataInd != null ? GeoDataInd.GeoStruct_Location_Name : "",
                                                                    Fld49 = Status != null ? Status : "",
                                                                    Fld50 = getBossName != null ? getBossName : "",
                                                                    Fld51 = getBossName != null ? "BossName" : "",
                                                                    Fld52 = GeoDataInd != null ? GeoDataInd.GeoStruct_Department_Name : "",
                                                                    Fld53 = getFunctAllWFDetails != null ? (getFunctAllWFDetails.Comments != null ? getFunctAllWFDetails.Comments : "") : "",
                                                                    // Fld80=
                                                                };
                                                                //  OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                                if (month)
                                                                {
                                                                    OGenericObjStatement.Fld100 = mPayMonth.ToString();
                                                                }
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
                                                                if (emp)
                                                                {
                                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                                    //OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                }

                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            }

                                                        }
                                                    }
                                                }
                                                //else
                                                //{
                                                //    //return null;
                                                //}
                                            }
                                        }
                                        #endregion salheadlist FALSE and SpecialGroupslist TRUE

                                        #region salheadlist AND SpecialGroupslist TRUE
                                        if (salheadlist != null && salheadlist.Count() != 0 && SpecialGroupslist != null && SpecialGroupslist.Count() != 0)
                                        {
                                            foreach (var item2 in salheadlist)
                                            {
                                                var OOFuncAtt = ca.FunctAttendanceT.Where(e => item == e.PayMonth && e.SalaryHead.Code == item2.ToString()).ToList();
                                                foreach (var item3 in SpecialGroupslist)
                                                {

                                                    if (OOFuncAtt != null && OOFuncAtt.Count() != 0)
                                                    {


                                                        int? geoid = OOFuncAtt.FirstOrDefault().GeoStruct_Id;

                                                        int? payid = OOFuncAtt.FirstOrDefault().PayStruct_Id;

                                                        int? funid = OOFuncAtt.FirstOrDefault().FuncStruct_Id;

                                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                                        //int geoid = OOFuncAtt.FirstOrDefault().GeoStruct.Id;

                                                        //int payid = OOFuncAtt.FirstOrDefault().PayStruct.Id;

                                                        //int funid = OOFuncAtt.FirstOrDefault().FuncStruct.Id;

                                                        //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                        //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                        //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                        if (GeoDataInd != null)
                                                        {
                                                            foreach (var ca1 in OOFuncAtt)
                                                            {

                                                                var getFunctAllWFDetails = ca1.FunctAllWFDetails.OrderByDescending(o => o.Id).FirstOrDefault();

                                                                if (getFunctAllWFDetails != null)
                                                                {

                                                                    var wStatus = getFunctAllWFDetails.WFStatus;

                                                                    var bossName = getFunctAllWFDetails.DBTrack.CreatedBy;

                                                                    switch (wStatus)
                                                                    {
                                                                        case 0:
                                                                            Status = "Applied";
                                                                            break;
                                                                        case 1:
                                                                            Status = "Sanction Apporved";
                                                                            break;
                                                                        case 2:
                                                                            Status = "Sanction Rejected";
                                                                            break;
                                                                        case 3:
                                                                            Status = "HR Apporved";
                                                                            break;
                                                                        case 4:
                                                                            Status = "HR Rejected";
                                                                            break;
                                                                        case 5:
                                                                            Status = "HRM(M)";
                                                                            break;
                                                                        case 6:
                                                                            Status = "Cancelled";
                                                                            break;
                                                                        case 7:
                                                                            Status = "Recommendation Apporved";
                                                                            break;
                                                                        case 8:
                                                                            Status = "Recommendation Rejected";
                                                                            break;


                                                                    }
                                                                    int bId = Convert.ToInt32(bossName);
                                                                    if (bId != 0)
                                                                    {
                                                                        getBossName = db.Employee.Include(e => e.EmpName).Where(e => e.Id == bId).FirstOrDefault().EmpName.FullNameFML.ToString();
                                                                    }
                                                                    else
                                                                    {
                                                                        getBossName = string.Empty;
                                                                    }


                                                                }
                                                                else
                                                                {
                                                                    Status = "HRM(M)";
                                                                }
                                                                if (Status == "Applied")
                                                                {
                                                                    getBossName = "";
                                                                }
                                                                if (item3 == Status)
                                                                {
                                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                                    //write data to generic class
                                                                    {
                                                                        Fld1 = ca1.Id.ToString(),
                                                                        Fld2 = ca.Employee.EmpCode,
                                                                        Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                                        Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                                                        Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                                                        Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                                                        Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                                                        Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                                                        Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                                                        Fld10 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                                        Fld44 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                                        Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                                                        Fld46 = ca1.PayProcessGroup != null ? ca1.PayProcessGroup.Name : null,
                                                                        Fld47 = ca1.Reason != null ? ca1.Reason : null,

                                                                        Fld48 = GeoDataInd != null ? GeoDataInd.GeoStruct_Location_Name : "",
                                                                        Fld49 = Status != null ? Status : "",
                                                                        Fld50 = getBossName != null ? getBossName : "",
                                                                        Fld51 = getBossName != null ? "BossName" : "",
                                                                        Fld52 = GeoDataInd != null ? GeoDataInd.GeoStruct_Department_Name : "",
                                                                        Fld53 = getFunctAllWFDetails != null ? (getFunctAllWFDetails.Comments != null ? getFunctAllWFDetails.Comments : "") : "",
                                                                        // Fld80=
                                                                    };
                                                                    //  OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                                    if (month)
                                                                    {
                                                                        OGenericObjStatement.Fld100 = mPayMonth.ToString();
                                                                    }
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
                                                                    if (emp)
                                                                    {
                                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                                        //OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                    }

                                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                                }
                                                            }

                                                        }
                                                    }
                                                    //else
                                                    //{
                                                    //    //return null;
                                                    //}
                                                }

                                            }

                                        }
                                        #endregion salheadlist AND SpecialGroupslist TRUE

                                        #region salheadlist AND SpecialGroupslist FALSE
                                        if (salheadlist.Count() == 0 && SpecialGroupslist.Count() == 0)
                                        {
                                            var OOFuncAtt = ca.FunctAttendanceT.Where(e => item == e.PayMonth).ToList();
                                            if (OOFuncAtt != null && OOFuncAtt.Count() != 0)
                                            {
                                                int? geoid = OOFuncAtt.FirstOrDefault().GeoStruct_Id;

                                                int? payid = OOFuncAtt.FirstOrDefault().PayStruct_Id;

                                                int? funid = OOFuncAtt.FirstOrDefault().FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                                //int geoid = OOFuncAtt.FirstOrDefault().GeoStruct.Id;

                                                //int payid = OOFuncAtt.FirstOrDefault().PayStruct.Id;

                                                //int funid = OOFuncAtt.FirstOrDefault().FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                if (GeoDataInd != null)
                                                {
                                                    foreach (var ca1 in OOFuncAtt)
                                                    {
                                                        var getFunctAllWFDetails = ca1.FunctAllWFDetails.OrderByDescending(o => o.Id).FirstOrDefault();

                                                        if (getFunctAllWFDetails != null)
                                                        {

                                                            var wStatus = getFunctAllWFDetails.WFStatus;

                                                            var bossName = getFunctAllWFDetails.DBTrack.CreatedBy;
                                                            switch (wStatus)
                                                            {
                                                                case 0:
                                                                    Status = "Applied";
                                                                    break;
                                                                case 1:
                                                                    Status = "Sanction Apporved";
                                                                    break;
                                                                case 2:
                                                                    Status = "Sanction Rejected";
                                                                    break;
                                                                case 3:
                                                                    Status = "HR Apporved";
                                                                    break;
                                                                case 4:
                                                                    Status = "HR Rejected";
                                                                    break;
                                                                case 5:
                                                                    Status = "HRM(M)";
                                                                    break;
                                                                case 6:
                                                                    Status = "Cancelled";
                                                                    break;
                                                                case 7:
                                                                    Status = "Recommendation Apporved";
                                                                    break;
                                                                case 8:
                                                                    Status = "Recommendation Rejected";
                                                                    break;


                                                            }
                                                            int bId = Convert.ToInt32(bossName);
                                                            if (bId != 0 && bId != null)
                                                            {
                                                                getBossName = db.Employee.Include(e => e.EmpName).Where(e => e.Id == bId).FirstOrDefault().EmpName.FullNameFML.ToString();
                                                            }
                                                            else
                                                            {
                                                                getBossName = string.Empty;
                                                            }


                                                        }
                                                        else
                                                        {
                                                            Status = "HRM(M)";
                                                        }
                                                        if (Status == "Applied")
                                                        {
                                                            getBossName = "";
                                                        }
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = ca1.Id.ToString(),
                                                            Fld2 = ca.Employee.EmpCode,
                                                            Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                            Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                                            Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                                            Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                                            Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                                            Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                                            Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                                            Fld10 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                            Fld44 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                            Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                                            Fld46 = ca1.PayProcessGroup != null ? ca1.PayProcessGroup.Name : null,
                                                            Fld47 = ca1.Reason != null ? ca1.Reason : null,
                                                            // Fld80=
                                                            Fld48 = GeoDataInd != null ? GeoDataInd.GeoStruct_Location_Name : "",
                                                            Fld49 = Status != null ? Status : "",
                                                            Fld50 = getBossName != null ? getBossName : "",
                                                            Fld51 = getBossName != "" ? "BossName" : "",
                                                            Fld52 = GeoDataInd != null ? GeoDataInd.GeoStruct_Department_Name : "",
                                                            Fld53 = getFunctAllWFDetails != null ? (getFunctAllWFDetails.Comments != null ? getFunctAllWFDetails.Comments : "") : "",
                                                        };
                                                        //  OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                                        if (month)
                                                        {
                                                            OGenericObjStatement.Fld100 = mPayMonth.ToString();
                                                        }
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
                                                        if (emp)
                                                        {
                                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                            //OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                }
                                            }

                                        }
                                        #endregion salheadlist AND SpecialGroupslist FALSE
                                    }
                                }
                                //else
                                //{
                                //    //return null;
                                //}

                            }
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "INSURANCEDETAILST":
                        var OInsuranceData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OInsuranceData_temp = db.EmployeePayroll

                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)

                            //.Include(r => r.InsuranceDetailsT)
                                // .Include(r => r.InsuranceDetailsT.Select(t => t.InsuranceProduct))
                                //.Include(r => r.InsuranceDetailsT.Select(t => t.OperationStatus))
                                //.Include(r => r.InsuranceDetailsT.Select(t => t.Frequency))
                                //.Include(r => r.InsuranceDetailsT.Select(t => t.InsuranceProduct))
                                // .Include(e => e.Employee.GeoStruct)
                                //  .Include(e => e.Employee.PayStruct)
                                //  .Include(e => e.Employee.FuncStruct)

                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OInsuranceData_temp != null)
                            {

                                OInsuranceData_temp.Employee.EmpName = db.NameSingle.Find(OInsuranceData_temp.Employee.EmpName_Id);

                                //var month = mPayMonth.Count();
                                //var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                //var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(month).SingleOrDefault().ToString())).Date;
                                OInsuranceData_temp.InsuranceDetailsT = db.InsuranceDetailsT.Include(o => o.OperationStatus).Include(e => e.InsuranceProduct).Include(e => e.Frequency).Where(e => (e.FromDate.Value >= pFromDate && e.ToDate.Value <= pToDate) && e.EmployeePayroll_Id == OInsuranceData_temp.Id).AsNoTracking().ToList();

                                OInsuranceData.Add(OInsuranceData_temp);
                            }
                        }
                        if (OInsuranceData == null || OInsuranceData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var Insurancehead = false;
                            var OperationStatus = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "InsuranceHead")
                                {

                                    Insurancehead = true;
                                }

                            }
                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "OperationStatus")
                                {

                                    OperationStatus = true;
                                }

                            }

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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OInsuranceData)
                            {
                                if (ca.InsuranceDetailsT != null && ca.InsuranceDetailsT.Count() != 0)
                                {
                                    //var m = mPayMonth.Count();
                                    //var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                    //var mend = Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            //var OInsurData = ca.InsuranceDetailsT.Where(e => e.FromDate >= mstart && e.ToDate >= mend).ToList();
                                            var OInsurData = ca.InsuranceDetailsT.Where(e => e.FromDate.Value.Date >= pFromDate && e.ToDate.Value.Date <= pToDate && e.OperationStatus.LookupVal.ToUpper() == "ACTIVE" && e.InsuranceProduct.InsuranceProductDesc.ToString() == item.ToString()).ToList();

                                            if (OInsurData != null && OInsurData.Count() != 0)
                                            {
                                                int? geoid = ca.Employee.GeoStruct_Id;

                                                int? payid = ca.Employee.PayStruct_Id;

                                                int? funid = ca.Employee.FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                                //int geoid = ca.Employee.GeoStruct.Id;

                                                //int payid = ca.Employee.PayStruct.Id;

                                                //int funid = ca.Employee.FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                if (GeoDataInd != null)
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

                                                        if (month)
                                                        {
                                                            OGenericObjStatement.Fld100 = mPayMonth.ToString();
                                                        }
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
                                                        if (emp)
                                                        {
                                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                        }
                                                        if (Insurancehead)
                                                        {
                                                            OGenericObjStatement.Fld87 = ca1.InsuranceProduct.InsuranceProductDesc;
                                                        }
                                                        if (OperationStatus)
                                                        {
                                                            OGenericObjStatement.Fld86 = ca1.OperationStatus.LookupVal.ToUpper();
                                                        }


                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                }
                                            }
                                            //else
                                            //{
                                            //    //return null;
                                            //}
                                        }
                                    }
                                    else
                                    {
                                        var OInsurData = ca.InsuranceDetailsT.Where(e => e.FromDate.Value.Date >= pFromDate && e.ToDate.Value.Date <= pToDate && e.OperationStatus.LookupVal.ToUpper() == "ACTIVE").ToList();

                                        if (OInsurData != null && OInsurData.Count() != 0)
                                        {
                                            int? geoid = ca.Employee.GeoStruct_Id;

                                            int? payid = ca.Employee.PayStruct_Id;

                                            int? funid = ca.Employee.FuncStruct_Id;

                                            GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                            PayStruct paystruct = db.PayStruct.Find(payid);

                                            FuncStruct funstruct = db.FuncStruct.Find(funid);

                                            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                            //int geoid = ca.Employee.GeoStruct.Id;

                                            //int payid = ca.Employee.PayStruct.Id;

                                            //int funid = ca.Employee.FuncStruct.Id;

                                            //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                            //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                            //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                            if (GeoDataInd != null)
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

                                                    if (month)
                                                    {
                                                        OGenericObjStatement.Fld100 = mPayMonth.ToString();
                                                    }
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
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }
                                                    if (Insurancehead)
                                                    {
                                                        OGenericObjStatement.Fld87 = ca1.InsuranceProduct.InsuranceProductDesc;
                                                    }
                                                    if (OperationStatus)
                                                    {
                                                        OGenericObjStatement.Fld86 = ca1.OperationStatus.LookupVal.ToUpper();
                                                    }


                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }
                                        }
                                    }
                                }
                                //else
                                //{
                                //    //return null;
                                //}

                            }
                            return OGenericPayrollStatement;
                        }
                        break;
                    case "REQUISITIONT":
                        var OSalArrearData1 = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OSalArrearData_temp = db.EmployeePayroll
                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.SalaryArrearT)
                                //.Include(r => r.SalaryArrearT.Select(t => t.ArrearType))
                                //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct))


                            //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.Job))
                                //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct.JobPosition))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Grade))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.JobStatus))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.JobStatus.EmpStatus))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.JobStatus.EmpActingStatus))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct.Level))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Division))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Location))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Location.LocationObj))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Department))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Department.DepartmentObj))
                                //.Include(r => r.SalaryArrearT.Select(t => t.GeoStruct.Group)).AsNoTracking()
                                //.Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT))
                                // .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(u => u.SalaryHead.Type)))
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OSalArrearData_temp != null)
                            {
                                OSalArrearData_temp.Employee.EmpName = db.NameSingle.Find(OSalArrearData_temp.Employee.EmpName_Id);
                                OSalArrearData_temp.SalaryArrearT = db.SalaryArrearT.Include(a => a.ArrearType).Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OSalArrearData_temp.Id).AsNoTracking().ToList();

                                foreach (var Attitem in OSalArrearData_temp.SalaryArrearT)
                                {
                                    Attitem.GeoStruct = db.GeoStruct.Find(Attitem.GeoStruct_Id);
                                    Attitem.PayStruct = db.PayStruct.Find(Attitem.PayStruct_Id);
                                    Attitem.FuncStruct = db.FuncStruct.Find(Attitem.FuncStruct_Id);
                                }
                                OSalArrearData1.Add(OSalArrearData_temp);
                            }
                        }
                        if (OSalArrearData1 == null || OSalArrearData1.Count() == 0)
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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);
                            /////

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OSalArrearData1)
                            {
                                if (ca.SalaryArrearT != null && ca.SalaryArrearT.Count() != 0)
                                {
                                    var OSalArrear = ca.SalaryArrearT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();

                                    var v2 = OSalArrear.Select(a => a.SalaryArrearPaymentT).ToList();

                                    // var Osalarrearpayment = db.SalaryArrearT.Include(a => a.SalaryArrearPaymentT).Select(a => a.SalaryArrearPaymentT).ToList();

                                    if (OSalArrear != null && OSalArrear.Count() != 0)
                                    {
                                        int? geoid = OSalArrear.FirstOrDefault().GeoStruct_Id;

                                        int? payid = OSalArrear.FirstOrDefault().PayStruct_Id;

                                        int? funid = OSalArrear.FirstOrDefault().FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                        //int geoid = OSalArrear.FirstOrDefault().GeoStruct.Id;

                                        //int payid = OSalArrear.FirstOrDefault().PayStruct.Id;

                                        //int funid = OSalArrear.FirstOrDefault().FuncStruct.Id;

                                        //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null)
                                        {

                                            foreach (var ca1 in OSalArrear)
                                            {

                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                //write data to generic class
                                                {

                                                    Fld2 = ca.Employee == null ? "" : ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld3 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    Fld4 = ca1.PayMonth == null ? "" : ca1.PayMonth,
                                                    Fld5 = ca1.ArrearType == null ? "" : ca1.ArrearType.LookupVal == null ? "" : ca1.ArrearType.LookupVal.ToUpper(),
                                                    Fld6 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                    Fld7 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                    Fld8 = ca1.TotalDays == null ? "" : ca1.TotalDays.ToString(),
                                                    Fld9 = ca1.IsRecovery == null ? "" : ca1.IsRecovery.ToString(),
                                                    Fld10 = ca1.IsAuto == null ? "" : ca1.IsAuto.ToString(),
                                                    Fld11 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString()
                                                };

                                                if (month)
                                                {
                                                    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                }
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
                                                if (emp)
                                                {
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);


                                            }
                                        }
                                    }

                                }

                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "ITTRANST":
                        var OITTransData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITTransData_temp = db.EmployeePayroll
                                //.Include(e => e.ITaxTransT)
                            .Include(e => e.Employee)
                                // .Include(e => e.SalaryT)
                                //.Include(e => e.Employee.EmpOffInfo)
                                //.Include(e => e.Employee.EmpOffInfo.NationalityID)
                                //.Include(e => e.Employee.EmpName)
                                // .Include(e => e.Employee.GeoStruct)
                                //   .Include(e => e.Employee.FuncStruct)
                                //   .Include(e => e.Employee.PayStruct)
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OITTransData_temp != null)
                            {
                                OITTransData_temp.Employee.EmpName = db.NameSingle.Find(OITTransData_temp.Employee.EmpName_Id);
                                OITTransData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == OITTransData_temp.Employee.EmpOffInfo_Id).FirstOrDefault();
                                //OITTransData_temp.Employee.EmpOffInfo.NationalityID = db.NationalityID.Find(OITTransData_temp.Employee.EmpOffInfo.NationalityID.Id);
                                OITTransData_temp.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OITTransData_temp.Id && mPayMonth.Contains(e.PayMonth)).ToList();
                                OITTransData_temp.ITaxTransT = db.ITaxTransT.Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OITTransData_temp.Id).AsNoTracking().ToList();
                                foreach (var Attitem in OITTransData_temp.ITaxTransT)
                                {
                                    Attitem.GeoStruct = db.GeoStruct.Find(Attitem.GeoStruct_Id);
                                    Attitem.PayStruct = db.PayStruct.Find(Attitem.PayStruct_Id);
                                    Attitem.FuncStruct = db.FuncStruct.Find(Attitem.FuncStruct_Id);

                                }
                                OITTransData.Add(OITTransData_temp);
                            }
                        }

                        if (OITTransData == null || OITTransData.Count() == 0)
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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            ///new
                            ///
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OITTransData)
                            {

                                if (ca.ITaxTransT != null && ca.ITaxTransT.Count() != 0)
                                {
                                    var Empsal = ca.SalaryT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                    if (Empsal.Count() > 0)
                                    {


                                        foreach (var ca2 in Empsal)
                                        {
                                            var OITTrans = ca.ITaxTransT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                            if (OITTrans != null && OITTrans.Count() != 0)
                                            {

                                                int? geoid = OITTrans.FirstOrDefault().GeoStruct_Id;

                                                int? payid = OITTrans.FirstOrDefault().PayStruct_Id;

                                                int? funid = OITTrans.FirstOrDefault().FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                                //int geoid = ca.Employee.GeoStruct.Id;

                                                //int payid = ca.Employee.PayStruct.Id;

                                                //int funid = ca.Employee.FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                if (GeoDataInd != null)
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
                                                            Fld11 = GeoDataInd.FuncStruct_Job_Name != null ? GeoDataInd.FuncStruct_Job_Name : null,
                                                            Fld12 = ca.Employee.EmpOffInfo != null ? ca.Employee.EmpOffInfo.NationalityID.PANNo : null,
                                                            Fld13 = ca2.TotalEarning == null ? "0" : ca2.TotalEarning.ToString(),
                                                            Fld14 = ca2.TotalNet == null ? "0" : ca2.TotalNet.ToString(),
                                                        };

                                                        if (month)
                                                        {
                                                            OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                        }
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
                                                        if (emp)
                                                        {
                                                            OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                        }

                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                }
                                            }
                                        }//
                                    }
                                    else
                                    {
                                        var OITTrans = ca.ITaxTransT.Where(e => mPayMonth.Contains(e.PayMonth)).ToList();
                                        if (OITTrans != null && OITTrans.Count() != 0)
                                        {

                                            int? geoid = OITTrans.FirstOrDefault().GeoStruct_Id;

                                            int? payid = OITTrans.FirstOrDefault().PayStruct_Id;

                                            int? funid = OITTrans.FirstOrDefault().FuncStruct_Id;

                                            GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                            PayStruct paystruct = db.PayStruct.Find(payid);

                                            FuncStruct funstruct = db.FuncStruct.Find(funid);

                                            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                            //int geoid = ca.Employee.GeoStruct.Id;

                                            //int payid = ca.Employee.PayStruct.Id;

                                            //int funid = ca.Employee.FuncStruct.Id;

                                            //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                            //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                            //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                            if (GeoDataInd != null)
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
                                                        Fld11 = GeoDataInd.FuncStruct_Job_Name != null ? GeoDataInd.FuncStruct_Job_Name : null,
                                                        Fld12 = ca.Employee.EmpOffInfo != null ? ca.Employee.EmpOffInfo.NationalityID.PANNo : null,
                                                        Fld13 = "0",//ca2.TotalEarning == null ? "0" : ca2.TotalEarning.ToString(),
                                                        Fld14 = "0",//ca2.TotalNet == null ? "0" : ca2.TotalNet.ToString(),
                                                    };

                                                    if (month)
                                                    {
                                                        OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
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
                                                    if (emp)
                                                    {
                                                        OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "HRAEXEMPTION":
                        var OHRAData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OHRAData_temp = db.EmployeePayroll
                                 .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.HRATransT)
                            .Include(e => e.HRATransT.Select(q => q.Financialyear))
                           .Include(e => e.HRATransT.Select(q => q.City))
                           .Include(e => e.HRATransT.Select(q => q.HRAMonthRent))
                             .Include(e => e.Employee.GeoStruct)
                               .Include(e => e.Employee.FuncStruct)
                               .Include(e => e.Employee.PayStruct)
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OHRAData_temp != null)
                            {

                                OHRAData.Add(OHRAData_temp);
                            }
                        }

                        if (OHRAData == null || OHRAData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {

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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            ///new

                            foreach (var ca in OHRAData)
                            {
                                var OHRA = ca.HRATransT.Where(e => e.Financialyear.FromDate >= mFromDate && e.Financialyear.ToDate <= mToDate).ToList();

                                if (OHRA != null && OHRA.Count() != 0)
                                {
                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                    {


                                        foreach (var ca1 in OHRA)
                                        {
                                            var RentData = ca1.HRAMonthRent.ToList();

                                            foreach (var ca2 in RentData)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = ca.Employee.Id != null ? ca.Employee.Id.ToString() : null,
                                                    Fld2 = ca.Employee.EmpCode != null ? ca.Employee.EmpCode : null,
                                                    Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                    Fld4 = ca1.City.FullDetails != null ? ca1.City.FullDetails : null,
                                                    Fld5 = ca2.RentFromDate != null ? ca2.RentFromDate.Value.ToShortDateString() : null,
                                                    Fld6 = ca2.RentToDate != null ? ca2.RentToDate.Value.ToShortDateString() : null,
                                                    Fld7 = ca2.RentAmount != null ? ca2.RentAmount.ToString() : null,
                                                };


                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObjStatement.Fld94 = GeoDataInd.Unit_Name;
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
                                                    OGenericObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
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
                                                    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                }

                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "PERKTRANSMONTHLY":
                        //var mnt1 = mPayMonth.FirstOrDefault();
                        var OPerkTransData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var Empd = db.EmployeePayroll
                                .Include(e => e.Employee)
                                //.Include(e => e.PerkTransT)
                                //.Include(e => e.PerkTransT.Select(q => q.SalaryHead))
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)
                                .Where(e => e.Employee.Id == item).AsNoTracking().FirstOrDefault();
                            // .Where(e => EmpPayrollIdList.Contains(e.Id) && e.PerkTransT.Any(a => a.PayMonth == mnt1.ToString())).ToList();

                            if (Empd != null)
                            {
                                Empd.Employee.EmpName = db.NameSingle.Find(Empd.Employee.EmpName_Id);
                                Empd.PerkTransT = db.PerkTransT.Include(e => e.SalaryHead).Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == Empd.Id).AsNoTracking().ToList();

                                OPerkTransData.Add(Empd);
                            }
                        }
                        if (OPerkTransData == null || OPerkTransData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var Perkhead = false;
                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "PerkHeadwise")
                                {

                                    Perkhead = true;
                                }

                            }

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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OPerkTransData)
                            {

                                foreach (var ca1 in ca.PerkTransT)
                                {

                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            var perkt = ca.PerkTransT.Where(a => a.SalaryHead.Code.ToString() == item.ToString()).ToList();
                                            if (perkt != null && perkt.Count() != 0)
                                            {

                                                int? geoid = ca.Employee.GeoStruct_Id;

                                                int? payid = ca.Employee.PayStruct_Id;

                                                int? funid = ca.Employee.FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                                //int geoid = ca.Employee.GeoStruct.Id;

                                                //int payid = ca.Employee.PayStruct.Id;

                                                //int funid = ca.Employee.FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                if (GeoDataInd != null)
                                                {

                                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.Employee.EmpCode.ToString(),
                                                        Fld2 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                        Fld3 = ca1.SalaryHead == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                        Fld4 = ca1.ProjectedAmount == null ? "" : ca1.ProjectedAmount.ToString(),
                                                        Fld5 = ca1.ActualAmount == null ? "" : ca1.ActualAmount.ToString()
                                                    };

                                                    if (month)
                                                    {
                                                        OGenericGeoObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericGeoObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericGeoObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericGeoObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericGeoObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericGeoObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericGeoObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericGeoObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericGeoObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericGeoObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericGeoObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericGeoObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericGeoObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    if (Perkhead)
                                                    {
                                                        OGenericGeoObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                    }
                                                    //write data to generic class
                                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var perkt = ca.PerkTransT.Where(a => a.Id != null).ToList();
                                        if (perkt != null && perkt.Count() != 0)
                                        {
                                            int? geoid = ca.Employee.GeoStruct_Id;

                                            int? payid = ca.Employee.PayStruct_Id;

                                            int? funid = ca.Employee.FuncStruct_Id;

                                            GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                            PayStruct paystruct = db.PayStruct.Find(payid);

                                            FuncStruct funstruct = db.FuncStruct.Find(funid);

                                            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                            //int geoid = ca.Employee.GeoStruct.Id;

                                            //int payid = ca.Employee.PayStruct.Id;

                                            //int funid = ca.Employee.FuncStruct.Id;

                                            //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                            //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                            //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                            if (GeoDataInd != null)
                                            {

                                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Employee.EmpCode.ToString(),
                                                    Fld2 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld3 = ca1.SalaryHead == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                    Fld4 = ca1.ProjectedAmount == null ? "" : ca1.ProjectedAmount.ToString(),
                                                    Fld5 = ca1.ActualAmount == null ? "" : ca1.ActualAmount.ToString()
                                                };

                                                if (month)
                                                {
                                                    OGenericGeoObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                }
                                                if (comp)
                                                {
                                                    OGenericGeoObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericGeoObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericGeoObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericGeoObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericGeoObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericGeoObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericGeoObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericGeoObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericGeoObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericGeoObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericGeoObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericGeoObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                if (Perkhead)
                                                {
                                                    OGenericGeoObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                }
                                                //write data to generic class
                                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                            }
                                        }
                                    }
                                }

                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "OTHERDEDUCTIONT":
                        var OOtherEarningDataded = new List<EmployeePayroll>();
                        //List<OthEarningDeductionT> OtherEarningDeductionTded = new List<OthEarningDeductionT>();
                        //var monded = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OOtherEarningDeductionTData_t = db.EmployeePayroll
                                //.Include(a => a.OtherEarningDeductionT)
                                //.Include(a => a.OtherEarningDeductionT.Select(b => b.SalaryHead))
                                //.Include(a => a.OtherEarningDeductionT.Select(b => b.SalaryHead.Type))
                                //.Include(a => a.OtherEarningDeductionT.Select(b => b.SalaryHead.SalHeadOperationType))
                                .Include(e => e.Employee).AsNoTracking()
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct).AsNoTracking()
                                .Where(e => e.Employee.Id == item)
                                                   .ToList().FirstOrDefault();


                            //var OEmpPayroll = new EmployeePayroll();
                            //OEmpPayroll = db.EmployeePayroll.Where(e => e.Employee.Id == item).FirstOrDefault();
                            //var OEmp = db.EmployeePayroll.Where(e => e.Employee.Id == item).Select(r => r.Employee).FirstOrDefault();
                            //var EmpName = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.EmpName).FirstOrDefault();
                            //var FuncStruct = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.FuncStruct).FirstOrDefault();
                            //var GeoStruct = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.GeoStruct).FirstOrDefault();
                            //var PayStruct = db.Employee.Where(e => e.Id == OEmp.Id).Select(r => r.PayStruct).FirstOrDefault();

                            //OEmp.EmpName = EmpName;
                            //OEmp.FuncStruct = FuncStruct;
                            //OEmp.GeoStruct = GeoStruct;
                            //OEmp.PayStruct = PayStruct;
                            //OEmpPayroll.Employee = OEmp;


                            //var OtherEarningDeductionTObj = new OthEarningDeductionT();


                            //OtherEarningDeductionT = db.OthEarningDeductionT.Where(e => e.EmployeePayroll_Id == OEmpPayroll.Id && e.PayMonth == mon).ToList();
                            //foreach (var i in OtherEarningDeductionT)
                            //{


                            //    var OthEarningDeductionTObj = db.OthEarningDeductionT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                            //    var SalaryHead = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).FirstOrDefault();
                            //    var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                            //    var Type = db.SalaryHead.Where(e => e.Id == OthEarningDeductionTObj.Id).Select(r => r.Type).FirstOrDefault();
                            //    i.SalaryHead = SalaryHead;
                            //    i.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                            //    i.SalaryHead.Type = Type;

                            //}
                            //if (OtherEarningDeductionT!=null)
                            //{

                            //}

                            if (OOtherEarningDeductionTData_t != null)
                            {
                                OOtherEarningDeductionTData_t.Employee.EmpName = db.NameSingle.Find(OOtherEarningDeductionTData_t.Employee.EmpName_Id);
                                OOtherEarningDeductionTData_t.OtherEarningDeductionT = db.OthEarningDeductionT.Where(e => mPayMonth.Contains(e.PayMonth) && OOtherEarningDeductionTData_t.Id == e.EmployeePayroll_Id).AsNoTracking().ToList();
                                foreach (var i in OOtherEarningDeductionTData_t.OtherEarningDeductionT)
                                {


                                    var SalaryHead = db.SalaryHead.Where(e => e.Id == i.SalaryHead_Id).FirstOrDefault();
                                    var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == i.SalaryHead_Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                                    var Type = db.SalaryHead.Where(e => e.Id == i.SalaryHead_Id).Select(r => r.Type).FirstOrDefault();
                                    i.SalaryHead = SalaryHead;
                                    i.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    i.SalaryHead.Type = Type;

                                }
                                OOtherEarningDataded.Add(OOtherEarningDeductionTData_t);
                            }
                        }

                        if (OOtherEarningDataded == null || OOtherEarningDataded.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var Yearlyhead = false;

                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "Headwise")
                                {

                                    Yearlyhead = true;
                                }

                            }

                            var ProcessField = "";
                            // var paymonth = mPayMonth.SingleOrDefault();
                            List<OthEarningDeductionT> OOtherEarningDeductionT = null;

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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OOtherEarningDataded)
                            {
                                if (ca.OtherEarningDeductionT != null && ca.OtherEarningDeductionT.Count() != 0)
                                {
                                    var OOtherEarningDeduction = ca.OtherEarningDeductionT.ToList();

                                    int? geoid = OOtherEarningDeduction.FirstOrDefault().GeoStruct_Id;

                                    int? payid = OOtherEarningDeduction.FirstOrDefault().PayStruct_Id;

                                    int? funid = OOtherEarningDeduction.FirstOrDefault().FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    //int geoid = ca.Employee.GeoStruct.Id;

                                    //int payid = ca.Employee.PayStruct.Id;

                                    //int funid = ca.Employee.FuncStruct.Id;

                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);


                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {



                                            OOtherEarningDeductionT = ca.OtherEarningDeductionT.Where(a => a.Id != null && a.SalaryHead.Code.ToString() == item.ToString()).ToList();


                                            //   var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                            //var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null && a.SalaryHead.Code.ToString() == item.ToString() && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).ToList();

                                            // var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null).ToList();
                                            if (OOtherEarningDeductionT != null && OOtherEarningDeductionT.Count() != 0)
                                            {
                                                foreach (var ca1 in OOtherEarningDeductionT)
                                                {


                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    {

                                                        Fld1 = ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML,
                                                        Fld3 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                        Fld9 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                        Fld6 = ca1.SalAmount == null ? "" : ca1.SalAmount.ToString(),
                                                        Fld7 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString(),


                                                    };
                                                    if (month)
                                                    {
                                                        OGenericLvTransObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericLvTransObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericLvTransObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericLvTransObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericLvTransObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericLvTransObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericLvTransObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericLvTransObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericLvTransObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }
                                                    if (Yearlyhead)
                                                    {
                                                        OGenericLvTransObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }

                                        }
                                    }
                                    else
                                    {
                                        OOtherEarningDeductionT = ca.OtherEarningDeductionT.Where(a => a.Id != null && a.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION" && a.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR").ToList();


                                        //var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).ToList();
                                        if (OOtherEarningDeductionT != null && OOtherEarningDeductionT.Count() != 0)
                                        {
                                            foreach (var ca1 in OOtherEarningDeductionT)
                                            {

                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Employee.EmpCode,
                                                    Fld2 = ca.Employee.EmpName.FullNameFML,
                                                    Fld3 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                    Fld9 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                    Fld6 = ca1.SalAmount == null ? "" : ca1.SalAmount.ToString(),
                                                    Fld7 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString(),


                                                };
                                                if (month)
                                                {
                                                    OGenericLvTransObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                }
                                                if (comp)
                                                {
                                                    OGenericLvTransObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericLvTransObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericLvTransObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericLvTransObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericLvTransObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericLvTransObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericLvTransObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericLvTransObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericLvTransObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericLvTransObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }
                                                if (Yearlyhead)
                                                {
                                                    OGenericLvTransObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                }

                                                OGenericPayrollStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }
                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "OTHEREARNINGT":
                        var OOtherEarningData = new List<EmployeePayroll>();
                        //List<OthEarningDeductionT> OtherEarningDeductionT = new List<OthEarningDeductionT>();
                        //var mon = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OOtherEarningDeductionTData_t = db.EmployeePayroll
                                //.Include(a => a.OtherEarningDeductionT)
                                //.Include(a => a.OtherEarningDeductionT.Select(b => b.SalaryHead))
                                //.Include(a => a.OtherEarningDeductionT.Select(b => b.SalaryHead.Type))
                                //.Include(a => a.OtherEarningDeductionT.Select(b => b.SalaryHead.SalHeadOperationType))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct).AsNoTracking()
                                .Where(e => e.Employee.Id == item).AsNoTracking().ToList().FirstOrDefault();

                            if (OOtherEarningDeductionTData_t != null)
                            {
                                OOtherEarningDeductionTData_t.Employee.EmpName = db.NameSingle.Find(OOtherEarningDeductionTData_t.Employee.EmpName_Id);
                                OOtherEarningDeductionTData_t.OtherEarningDeductionT = db.OthEarningDeductionT.Include(e => e.SalaryHead).Include(e => e.SalaryHead.SalHeadOperationType).Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == OOtherEarningDeductionTData_t.Id).AsNoTracking().ToList();
                                OOtherEarningData.Add(OOtherEarningDeductionTData_t);
                            }
                        }
                        if (OOtherEarningData == null || OOtherEarningData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var Yearlyhead = false;

                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "Headwise")
                                {

                                    Yearlyhead = true;
                                }

                            }

                            var ProcessField = "";
                            // var paymonth = mPayMonth.SingleOrDefault();
                            // List<OthEarningDeductionT> OOtherEarningDeductionT = null;

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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OOtherEarningData)
                            {
                                if (ca.OtherEarningDeductionT != null && ca.OtherEarningDeductionT.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                    //int geoid = ca.Employee.GeoStruct.Id;

                                    //int payid = ca.Employee.PayStruct.Id;

                                    //int funid = ca.Employee.FuncStruct.Id;

                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);



                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {



                                            var OOtherEarningDeductionT = ca.OtherEarningDeductionT.Where(a => a.Id != null && a.SalaryHead.Code.ToString() == item.ToString()).ToList();


                                            //   var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                            //var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null && a.SalaryHead.Code.ToString() == item.ToString() && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).ToList();

                                            // var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null).ToList();
                                            if (OOtherEarningDeductionT != null && OOtherEarningDeductionT.Count() != 0)
                                            {
                                                foreach (var ca1 in OOtherEarningDeductionT)
                                                {


                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    {

                                                        Fld1 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                        Fld3 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                        Fld9 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                        Fld6 = ca1.SalAmount == null ? "" : ca1.SalAmount.ToString(),
                                                        Fld7 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString(),


                                                    };
                                                    if (month)
                                                    {
                                                        OGenericLvTransObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericLvTransObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericLvTransObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericLvTransObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericLvTransObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericLvTransObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericLvTransObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericLvTransObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericLvTransObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }
                                                    if (Yearlyhead)
                                                    {
                                                        OGenericLvTransObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }
                                        }


                                    }
                                    else
                                    {
                                        var OOtherEarningDeductionT = ca.OtherEarningDeductionT.Where(a => a.Id != null && a.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && a.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "NONREGULAR").ToList();


                                        //var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).ToList();
                                        if (OOtherEarningDeductionT != null && OOtherEarningDeductionT.Count() != 0)
                                        {

                                            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                            if (GeoDataInd != null)
                                            {
                                                foreach (var ca1 in OOtherEarningDeductionT)
                                                {

                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                        Fld3 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                        Fld9 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                        Fld6 = ca1.SalAmount == null ? "" : ca1.SalAmount.ToString(),
                                                        Fld7 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString(),


                                                    };
                                                    if (month)
                                                    {
                                                        OGenericLvTransObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                    }
                                                    if (comp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericLvTransObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericLvTransObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericLvTransObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericLvTransObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericLvTransObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericLvTransObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericLvTransObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericLvTransObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }
                                                    if (Yearlyhead)
                                                    {
                                                        OGenericLvTransObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                    }

                                                    OGenericPayrollStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "YEARLYPAYMENTPAY":

                        //var Emp_y = db.EmployeeLeave.Include(a => a.Employee).Where(a => EmpPayrollIdList.Contains(a.Id)).Select(a => a.Employee.Id).ToList();
                        //var EmpPay_y = db.EmployeePayroll.Include(a => a.Employee).Include(a => a.SalaryT).Where(a => Emp_y.Contains(a.Employee.Id)).Select(a => a.Id).ToList();

                        var OyearlypaymentData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OyearlypaymentData_t = db.EmployeePayroll
                                //.Include(a => a.YearlyPaymentT)
                                //  .Include(a => a.YearlyPaymentT.Select(b => b.LvEncashReq))
                                //.Include(a => a.YearlyPaymentT.Select(b => b.SalaryHead))
                                // .Include(e => e.YearlyPaymentT.Select(r => r.LvEncashReq).Select(a => a.LvNewReq).Select(c => c.LeaveHead))
                                .Include(e => e.Employee)
                                // .Include(e => e.Employee.EmpOffInfo)
                                // .Include(e => e.Employee.EmpOffInfo.AccountType)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                                 .FirstOrDefault();


                            //if (OyearlypaymentData_t != null)
                            //{
                            OyearlypaymentData_t.Employee.EmpName = db.NameSingle.Find(OyearlypaymentData_t.Employee.EmpName_Id);
                            OyearlypaymentData_t.Employee.EmpOffInfo = db.EmpOff.Include(e => e.AccountType).Where(e => e.Id == OyearlypaymentData_t.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                            //  OyearlypaymentData_t.YearlyPaymentT = db.YearlyPaymentT.Where(e => e.EmployeePayroll_Id == OyearlypaymentData_t.Id).AsNoTracking().ToList();

                            //if (salheadlist != null && salheadlist.Count() != 0)
                            //{
                            //    foreach (var salitem in salheadlist)
                            //    {
                            if (ProcessType.ToUpper() == "PAID")
                            {
                                OyearlypaymentData_t.YearlyPaymentT = db.YearlyPaymentT.Include(e => e.SalaryHead).Where(a => a.ReleaseDate != null && a.EmployeePayroll_Id == OyearlypaymentData_t.Id && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).AsNoTracking().AsParallel().ToList();
                            }
                            else
                            {
                                OyearlypaymentData_t.YearlyPaymentT = db.YearlyPaymentT.Include(e => e.SalaryHead).Where(a => a.ReleaseDate == null && a.EmployeePayroll_Id == OyearlypaymentData_t.Id && a.ReleaseFlag == false && (a.ProcessMonth == mPayMonth.FirstOrDefault())).AsNoTracking().AsParallel().ToList();
                            }

                            //}
                            //else
                            //{

                            //    if (ProcessType.ToUpper() == "PAID")
                            //    {
                            //        OyearlypaymentData_t.YearlyPaymentT = db.YearlyPaymentT.Include(e => e.SalaryHead).Where(a => a.ReleaseDate != null && a.EmployeePayroll_Id == OyearlypaymentData_t.Id && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).AsNoTracking().AsParallel().ToList();
                            //    }
                            //    else
                            //    {
                            //        OyearlypaymentData_t.YearlyPaymentT = db.YearlyPaymentT.Include(e => e.SalaryHead).Where(a => a.ReleaseDate == null && a.EmployeePayroll_Id == OyearlypaymentData_t.Id && a.ReleaseFlag == false && (a.ProcessMonth == mPayMonth.FirstOrDefault())).AsNoTracking().AsParallel().ToList();
                            //    }
                            //}



                            OyearlypaymentData.Add(OyearlypaymentData_t);
                        }

                        if (OyearlypaymentData == null || OyearlypaymentData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var Yearlyhead = false;

                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "Headwise")
                                {

                                    Yearlyhead = true;
                                }

                            }

                            //var ProcessField = "";
                            // var paymonth = mPayMonth.SingleOrDefault();
                            //List<YearlyPaymentT> OLvEncash = null;

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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OyearlypaymentData)
                            {
                                if (ca.YearlyPaymentT != null && ca.YearlyPaymentT.Count() != 0)
                                {

                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    //int geoid = ca.Employee.GeoStruct.Id;

                                    //int payid = ca.Employee.PayStruct.Id;

                                    //int funid = ca.Employee.FuncStruct.Id;

                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    //var CheckReleaseDate = ca.YearlyPaymentT.ToList();17102023
                                    //if (CheckReleaseDate.Count() > 0 && CheckReleaseDate.Count() != 0)17102023
                                    //{
                                    //if (salheadlist != null && salheadlist.Count() != 0)
                                    //{
                                    //    foreach (var item in salheadlist)
                                    //    {
                                    //        OLvEncash = ca.YearlyPaymentT.Where(a => a.SalaryHead.Code.ToString() == item.ToString()).ToList();
                                    //OLvEncash = ca.YearlyPaymentT.ToList();
                                    //if (ProcessType.ToUpper() == "PAID")
                                    //{
                                    //    OLvEncash = ca.YearlyPaymentT.Where(a => a.SalaryHead.Code.ToString() == item.ToString()).ToList();
                                    //}
                                    ////else
                                    //{
                                    //    OLvEncash = ca.YearlyPaymentT.Where(a => a.SalaryHead.Code.ToString() == item.ToString()).ToList();
                                    //}

                                    //   var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                    //var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null && a.SalaryHead.Code.ToString() == item.ToString() && (a.ReleaseDate.Value >= pFromDate && a.ReleaseDate.Value <= pToDate) && a.ReleaseFlag == true).ToList();

                                    //var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null).ToList();

                                    //if (OLvEncash != null && OLvEncash.Count() != 0)
                                    //{
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {

                                            foreach (var ca1 in ca.YearlyPaymentT.Where(e => e.SalaryHead.Code == item))
                                            {
                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                {
                                                    Fld13 = ca.Id.ToString(),
                                                    Fld1 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld2 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    //  Fld3 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                    // Fld4 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                    Fld5 = ca1.FromPeriod == null ? "" : ca1.FromPeriod.Value.ToShortDateString(),
                                                    Fld6 = ca1.ToPeriod == null ? "" : ca1.ToPeriod.Value.ToShortDateString(),
                                                    Fld7 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                    Fld8 = ca.Employee.EmpOffInfo != null ? ca.Employee.EmpOffInfo.AccountType.LookupVal.ToString() : "",
                                                    Fld9 = ca.Employee.EmpOffInfo.AccountNo != null ? ca.Employee.EmpOffInfo.AccountNo.ToString() : "",
                                                    Fld10 = ca1.AmountPaid == null ? "" : ca1.AmountPaid.ToString(),
                                                    Fld12 = ca1.OtherDeduction == null ? "" : ca1.OtherDeduction.ToString(),
                                                    Fld11 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString(),
                                                    Fld14 = ca1.SalaryHead.Code == null ? "" : ca1.SalaryHead.Code.ToString(),
                                                    Fld15 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                    Fld17 = ca1.ProcessMonth == null ? "" : ca1.ProcessMonth.ToString(),

                                                };
                                                if (month)
                                                {
                                                    OGenericLvTransObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                }
                                                if (comp)
                                                {
                                                    OGenericLvTransObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericLvTransObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericLvTransObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericLvTransObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericLvTransObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericLvTransObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericLvTransObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericLvTransObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericLvTransObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericLvTransObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }
                                                if (Yearlyhead)
                                                {
                                                    OGenericLvTransObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                                }

                                                OGenericPayrollStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }
                                    }
                                    else
                                    {

                                        foreach (var ca1 in ca.YearlyPaymentT)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            {
                                                Fld13 = ca.Id.ToString(),
                                                Fld1 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                //  Fld3 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                // Fld4 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                Fld5 = ca1.FromPeriod == null ? "" : ca1.FromPeriod.Value.ToShortDateString(),
                                                Fld6 = ca1.ToPeriod == null ? "" : ca1.ToPeriod.Value.ToShortDateString(),
                                                Fld7 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                Fld8 = ca.Employee.EmpOffInfo != null ? ca.Employee.EmpOffInfo.AccountType.LookupVal.ToString() : "",
                                                Fld9 = ca.Employee.EmpOffInfo.AccountNo != null ? ca.Employee.EmpOffInfo.AccountNo.ToString() : "",
                                                Fld10 = ca1.AmountPaid == null ? "" : ca1.AmountPaid.ToString(),
                                                Fld12 = ca1.OtherDeduction == null ? "" : ca1.OtherDeduction.ToString(),
                                                Fld11 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString(),
                                                Fld14 = ca1.SalaryHead.Code == null ? "" : ca1.SalaryHead.Code.ToString(),
                                                Fld15 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name.ToString(),
                                                Fld17 = ca1.ProcessMonth == null ? "" : ca1.ProcessMonth.ToString(),

                                            };
                                            if (month)
                                            {
                                                OGenericLvTransObjStatement.Fld100 = ca1.PayMonth.ToString();
                                            }
                                            if (comp)
                                            {
                                                OGenericLvTransObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericLvTransObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericLvTransObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericLvTransObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericLvTransObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericLvTransObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericLvTransObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericLvTransObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericLvTransObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericLvTransObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            if (Yearlyhead)
                                            {
                                                OGenericLvTransObjStatement.Fld87 = ca1.SalaryHead.Name.ToString();
                                            }

                                            OGenericPayrollStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }



                                    //}
                                    //}
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "LOANADVREQUEST":
                        var OLoanAdvRequestData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OLoanAdvRequestData_temp = db.EmployeePayroll
                                //.Include(e => e.LoanAdvRequest)

                            .Include(e => e.Employee)

                            //.Include(r => r.LoanAdvRequest.Select(t => t.LoanAccBranch))
                                //.Include(r => r.LoanAdvRequest.Select(t => t.LoanAccBranch.LocationObj))
                                //.Include(r => r.LoanAdvRequest.Select(t => t.LoanAdvanceHead))
                                //.Include(r => r.LoanAdvRequest.Select(t => t.LoanAdvanceHead.SalaryHead))
                                //.Include(r => r.Employee.GeoStruct)
                                // .Include(r => r.Employee.FuncStruct)
                                // .Include(r => r.Employee.PayStruct)
                            .Where(e => e.Employee.Id == item).AsNoTracking().ToList()
                            .FirstOrDefault();

                            if (OLoanAdvRequestData_temp != null)
                            {
                                OLoanAdvRequestData_temp.Employee.EmpName = db.NameSingle.Find(OLoanAdvRequestData_temp.Employee.EmpName_Id);
                                OLoanAdvRequestData_temp.LoanAdvRequest = db.LoanAdvRequest.Where(e => OLoanAdvRequestData_temp.Id == e.EmployeePayroll_Id).AsNoTracking().ToList();

                                foreach (var i in OLoanAdvRequestData_temp.LoanAdvRequest)
                                {


                                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).AsNoTracking().FirstOrDefault();
                                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).AsNoTracking().FirstOrDefault();
                                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).AsNoTracking().FirstOrDefault();
                                    var SalaryHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).AsNoTracking().FirstOrDefault();

                                    i.LoanAccBranch = LoanAccBranch;
                                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                                    i.LoanAdvanceHead = LoanAdvanceHead;
                                    i.LoanAdvanceHead.SalaryHead = SalaryHead;

                                }
                                OLoanAdvRequestData.Add(OLoanAdvRequestData_temp);
                            }
                        }

                        if (OLoanAdvRequestData == null || OLoanAdvRequestData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();
                            var loanhead = false;
                            var accno = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "LoanHeadwise")
                                {

                                    loanhead = true;
                                }

                            }

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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OLoanAdvRequestData)
                            {
                                if (ca.LoanAdvRequest != null && ca.LoanAdvRequest.Count() != 0)
                                {
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            if (SpGroup != null && SpGroup.Count() != 0)
                                            {
                                                foreach (var spg in SpGroup)
                                                {
                                                    Boolean isact = false;
                                                    if (spg == "Active")
                                                    {
                                                        isact = true;
                                                    }
                                                    else
                                                    {
                                                        isact = false;
                                                    }

                                                    var OITTrans = ca.LoanAdvRequest.Where(a => a.LoanAdvanceHead.SalaryHead.Name.ToString() == item.ToString() && a.IsActive == isact && mPayMonth.Contains(a.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                                    //var OITTrans = ca.LoanAdvRequest.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                                    if (OITTrans != null && OITTrans.Count() != 0)
                                                    {
                                                        //int geoid = ca.Employee.GeoStruct.Id;

                                                        //int payid = ca.Employee.PayStruct.Id;

                                                        //int funid = ca.Employee.FuncStruct.Id;

                                                        //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                        //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                        //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);



                                                        int? geoid = ca.Employee.GeoStruct_Id;

                                                        int? payid = ca.Employee.PayStruct_Id;

                                                        int? funid = ca.Employee.FuncStruct_Id;

                                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                                        if (GeoDataInd != null)
                                                        {
                                                            foreach (var ca1 in OITTrans)
                                                            {
                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                //write data to generic class
                                                                {
                                                                    Fld1 = ca.Employee.Id.ToString(),
                                                                    Fld2 = ca.Employee.EmpCode,
                                                                    Fld3 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
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
                                                                    Fld18 = ca1.LoanAccBranch == null ? "" : ca1.LoanAccBranch.LocationObj == null ? "" : ca1.LoanAccBranch.LocationObj.LocDesc.ToString(),
                                                                    Fld19 = ca1.LoanAccNo == null ? "" : ca1.LoanAccNo.ToString(),
                                                                    Fld20 = ca1.Narration == null ? "" : ca1.Narration,
                                                                    Fld21 = ca1.IsActive == null ? "" : ca1.IsActive.ToString(),
                                                                    Fld22 = ca1.CloserDate == null ? "" : ca1.CloserDate.Value.ToShortDateString(),
                                                                };

                                                                //if (month)
                                                                //{
                                                                //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                                //}
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
                                                                if (emp)
                                                                {
                                                                    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                }
                                                                if (loanhead)
                                                                {
                                                                    OGenericObjStatement.Fld87 = ca1.LoanAdvanceHead.Name;
                                                                }

                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {


                                                var OITTrans = ca.LoanAdvRequest.Where(a => a.LoanAdvanceHead.SalaryHead.Name.ToString() == item.ToString() && mPayMonth.Contains(a.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                                //var OITTrans = ca.LoanAdvRequest.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                                if (OITTrans != null && OITTrans.Count() != 0)
                                                {
                                                    //int geoid = ca.Employee.GeoStruct.Id;

                                                    //int payid = ca.Employee.PayStruct.Id;

                                                    //int funid = ca.Employee.FuncStruct.Id;

                                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                                    int? geoid = ca.Employee.GeoStruct_Id;

                                                    int? payid = ca.Employee.PayStruct_Id; ;

                                                    int? funid = ca.Employee.FuncStruct_Id;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                                    if (GeoDataInd != null)
                                                    {
                                                        foreach (var ca1 in OITTrans)
                                                        {
                                                            GenericField100 OGenericObjStatement = new GenericField100()
                                                            //write data to generic class
                                                            {
                                                                Fld1 = ca.Employee.Id.ToString(),
                                                                Fld2 = ca.Employee.EmpCode,
                                                                Fld3 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
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
                                                                Fld18 = ca1.LoanAccBranch == null ? "" : ca1.LoanAccBranch.LocationObj == null ? "" : ca1.LoanAccBranch.LocationObj.LocDesc.ToString(),
                                                                Fld19 = ca1.LoanAccNo == null ? "" : ca1.LoanAccNo.ToString(),
                                                                Fld20 = ca1.Narration == null ? "" : ca1.Narration,
                                                                Fld21 = ca1.IsActive == null ? "" : ca1.IsActive.ToString(),
                                                                Fld22 = ca1.CloserDate == null ? "" : ca1.CloserDate.Value.ToShortDateString(),
                                                            };

                                                            //if (month)
                                                            //{
                                                            //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                            //}
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
                                                            if (emp)
                                                            {
                                                                OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                            }
                                                            if (loanhead)
                                                            {
                                                                OGenericObjStatement.Fld87 = ca1.LoanAdvanceHead.Name;
                                                            }

                                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                        }
                                                    }
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

                    case "LOANADVREPAYMENT":
                        var OLoanAdvRepaymentData = new List<EmployeePayroll>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OLoanAdvRepaymentData_temp = db.EmployeePayroll
                        .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //    .Include(e => e.Employee.GeoStruct)
                                //    .Include(e => e.Employee.PayStruct)
                                //    .Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT)
                                //    .Include(e => e.LoanAdvRequest.Select(t => t.LoanAccBranch))
                                //     .Include(e => e.LoanAdvRequest.Select(t => t.LoanAccBranch.LocationObj))
                                //    .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                //     .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                           .SingleOrDefault();
                            if (OLoanAdvRepaymentData_temp != null)
                            {
                                OLoanAdvRepaymentData_temp.Employee.EmpName = db.NameSingle.Find(OLoanAdvRepaymentData_temp.Employee.EmpName_Id);

                                List<LoanAdvRequest> LoanAdvRequest = new List<LoanAdvRequest>();
                                var LoanAdvRequestObj = new LoanAdvRequest();
                                LoanAdvRequest = db.LoanAdvRequest.Where(e => OLoanAdvRepaymentData_temp.Id == e.EmployeePayroll_Id).ToList();
                                foreach (var i in LoanAdvRequest)
                                {

                                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).FirstOrDefault();
                                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                                    var SalaryHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                    List<LoanAdvRepaymentT> LoanAdvRepaymentTList = new List<LoanAdvRepaymentT>();

                                    var LoanAdvRepaymentT = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvRepaymentT.Where(t => mPayMonth.Contains(t.PayMonth.ToString())).ToList()).FirstOrDefault();
                                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                                    i.LoanAdvanceHead = LoanAdvanceHead;
                                    i.LoanAdvanceHead.SalaryHead = SalaryHead;

                                    i.LoanAdvRepaymentT = (LoanAdvRepaymentT);

                                }
                                OLoanAdvRepaymentData_temp.LoanAdvRequest = LoanAdvRequest;

                                OLoanAdvRepaymentData.Add(OLoanAdvRepaymentData_temp);
                            }
                        }


                        if (OLoanAdvRepaymentData == null || OLoanAdvRepaymentData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var loanhead = false;
                            var loanLocation = false;
                            var accno = false;


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "LoanHeadwise")
                                {

                                    loanhead = true;
                                }
                                if (item1 == "LoanLoc")
                                {

                                    loanLocation = true;
                                }

                            }



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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OLoanAdvRepaymentData)
                            {

                                if (ca.LoanAdvRequest != null && ca.LoanAdvRequest.Count() != 0)
                                {
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {

                                            var OITTrans = ca.LoanAdvRequest.Where(a => a.LoanAdvanceHead.SalaryHead.Name.ToString() == item.ToString()).ToList();
                                            //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                            //  var OITTrans = ca.l //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                            if (OITTrans != null && OITTrans.Count() != 0)
                                            {
                                                if (ca.Employee.GeoStruct_Id != null && ca.Employee.FuncStruct_Id != null && ca.Employee.PayStruct_Id != null)
                                                {
                                                    //int geoid = ca.Employee.GeoStruct.Id;

                                                    //int payid = ca.Employee.PayStruct.Id;

                                                    //int funid = ca.Employee.FuncStruct.Id;

                                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                                    int? geoid = ca.Employee.GeoStruct_Id;

                                                    int? payid = ca.Employee.PayStruct_Id;

                                                    int? funid = ca.Employee.FuncStruct_Id;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);



                                                    foreach (var ca1 in OITTrans)
                                                    {
                                                        var LoanBranch = "";
                                                        var LocBranch = "";
                                                        var Loc = "";
                                                        var LocBranchcode = "";
                                                        var Loccode = "";
                                                        if (loanLocation == true)
                                                        {
                                                            LoanBranch = GeoDataInd.GeoStruct_Location_Name;
                                                            Loc = "Working Location";
                                                            LocBranch = ca1.LoanAccBranch.LocationObj.LocDesc.ToString();
                                                            Loccode = "Working Location Code";
                                                            LocBranchcode = GeoDataInd.GeoStruct_Location_Code.ToString();
                                                        }
                                                        else
                                                        {
                                                            LoanBranch = ca1.LoanAccBranch.LocationObj.LocDesc.ToString();
                                                            Loc = "Loan Location";
                                                            LocBranch = GeoDataInd.GeoStruct_Location_Name;
                                                            Loccode = "Loan Location Code";
                                                            LocBranchcode = ca1.LoanAccBranch.LocationObj.LocCode;

                                                        }

                                                        if (ca1.LoanAdvRepaymentT != null && ca1.LoanAdvRepaymentT.Count() != 0)
                                                        {
                                                            // else
                                                            // {
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
                                                                    //Fld30 = GeoDataInd == null ? "" : GeoDataInd.LocDesc,
                                                                    Fld30 = LoanBranch,                                                /////loan branch
                                                                    Fld31 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                                                    Fld28 = ca.Employee != null && ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                                    Fld29 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name,
                                                                    Fld32 = Loc.ToString(),
                                                                    Fld34 = Loccode.ToString(),
                                                                    Fld35 = LocBranchcode.ToString(),

                                                                };
                                                                //if (month)
                                                                //{
                                                                //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                                //}
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
                                                                    OGenericObjStatement.Fld97 = LocBranch;
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
                                                                if (emp)
                                                                {
                                                                    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                }

                                                                if (loanhead)
                                                                {
                                                                    OGenericObjStatement.Fld87 = ca1.LoanAdvanceHead.Name;
                                                                }

                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            }

                                                        }

                                                    }
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        var OITTrans = ca.LoanAdvRequest.ToList();
                                        //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                        //  var OITTrans = ca.l //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                        if (OITTrans != null && OITTrans.Count() != 0)
                                        {
                                            if (ca.Employee.GeoStruct_Id != null && ca.Employee.FuncStruct_Id != null && ca.Employee.PayStruct_Id != null)
                                            {
                                                //int geoid = ca.Employee.GeoStruct.Id;

                                                //int payid = ca.Employee.PayStruct.Id;

                                                //int funid = ca.Employee.FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                                int? geoid = ca.Employee.GeoStruct_Id;

                                                int? payid = ca.Employee.PayStruct_Id;

                                                int? funid = ca.Employee.FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);



                                                foreach (var ca1 in OITTrans)
                                                {
                                                    var LoanBranch = "";
                                                    var LocBranch = "";
                                                    var Loc = "";
                                                    if (loanLocation == true)
                                                    {
                                                        LoanBranch = GeoDataInd.GeoStruct_Location_Name;
                                                        Loc = "Working Location";
                                                        LocBranch = ca1.LoanAccBranch.LocationObj.LocDesc.ToString();
                                                    }
                                                    else
                                                    {
                                                        LoanBranch = ca1.LoanAccBranch.LocationObj.LocDesc.ToString();
                                                        Loc = "Loan Location";
                                                        LocBranch = GeoDataInd.GeoStruct_Location_Name;

                                                    }

                                                    if (ca1.LoanAdvRepaymentT != null && ca1.LoanAdvRepaymentT.Count() != 0)

                                                        // else
                                                        // {
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
                                                                Fld30 = LoanBranch,
                                                                Fld31 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                                                Fld28 = ca.Employee != null && ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                                Fld29 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name,
                                                                Fld32 = Loc,

                                                            };
                                                            //if (month)
                                                            //{
                                                            //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                            //}
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
                                                                OGenericObjStatement.Fld97 = LocBranch;
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
                                                            if (emp)
                                                            {
                                                                OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                            }

                                                            if (loanhead)
                                                            {
                                                                OGenericObjStatement.Fld87 = ca1.LoanAdvanceHead.Name;
                                                            }

                                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                        }

                                                    // }

                                                }
                                            }

                                        }
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

                    //Manish
                    case "LVDEPENDONSALARYHEADDAYS":
                        //string salcode = "";
                        var OLVDEPENDONSALARYDAYSDATA = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var Lvdependsaldata = db.EmployeePayroll

                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                                //.Include(e => e.EmpSalStruct)


                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();

                            if (Lvdependsaldata != null)
                            {
                                Lvdependsaldata.Employee.EmpName = db.NameSingle.Find(Lvdependsaldata.Employee.EmpName_Id);
                                // Lvdependsaldata.EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == Lvdependsaldata.Id).ToList();

                                Lvdependsaldata.EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == Lvdependsaldata.Id).AsNoTracking().ToList();

                                string PayMonth = mPayMonth.FirstOrDefault();
                                var FromDate = Convert.ToDateTime("01/" + PayMonth);

                                var ToDate = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;

                                var frmdate = FromDate.Date;
                                var todate = ToDate.Date;
                                Lvdependsaldata.EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == Lvdependsaldata.Id
                                                               && e.EffectiveDate >= frmdate && e.EffectiveDate <= todate).ToList();


                                foreach (var empSalStructitem in Lvdependsaldata.EmpSalStruct)
                                {
                                    empSalStructitem.EmpSalStructDetails = db.EmpSalStructDetails.Include(e => e.SalaryHead).Where(e => e.EmpSalStruct_Id == empSalStructitem.Id).ToList();

                                    foreach (var salaryheaditems in empSalStructitem.EmpSalStructDetails)
                                    {

                                        salaryheaditems.SalaryHead = db.SalaryHead.Include(e => e.LeaveDependPolicy)
                                            .Include(e => e.LeaveDependPolicy.Select(z => z.LvHead))
                                            .Where(e => e.Id == salaryheaditems.SalaryHead_Id && e.OnLeave == true).FirstOrDefault();


                                    }
                                }

                                OLVDEPENDONSALARYDAYSDATA.Add(Lvdependsaldata);

                            }
                        }


                        if (OLVDEPENDONSALARYDAYSDATA == null || OLVDEPENDONSALARYDAYSDATA.Count() == 0)
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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();


                            foreach (var ca in OLVDEPENDONSALARYDAYSDATA)
                            {
                                if (ca.EmpSalStruct.Count > 0)
                                {
                                    var empsalstructdetails = ca.EmpSalStruct.Select(r => r.EmpSalStructDetails.ToList()).FirstOrDefault();
                                    var salaryheaddata = empsalstructdetails.Where(a => a.SalaryHead != null).Select(x => x.SalaryHead).ToList();

                                    PayProcessGroup OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                                    double LvDays = 0;

                                    string PayMonth = mPayMonth.FirstOrDefault();


                                    foreach (var ca1 in salaryheaddata.Where(s => s.OnLeave == true))
                                    {
                                        //LvDays = SalaryGen.LvChk(ca1, OPayProcGrp, PayMonth, ca.Id);

                                        LvDays = SalaryGen.LvChk(ca1, OPayProcGrp, PayMonth, ca.Id, ca.EmpSalStruct.FirstOrDefault());
                                        // if (ca.EmpSalStruct != null && ca.EmpSalStruct.Count() != 0)

                                        //var OLvDependOnSalaryDays = ca.Employee.
                                        //var Oempsal = ca.EmpSalStruct.ToString();

                                        //int? geoid = OLvDependOnSalaryDays.FirstOrDefault().GeoStruct_Id;

                                        //int? payid = OLvDependOnSalaryDays.FirstOrDefault().PayStruct_Id;

                                        //int? funid = OLvDependOnSalaryDays.FirstOrDefault().FuncStruct_Id;

                                        //GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        //PayStruct paystruct = db.PayStruct.Find(payid);

                                        //FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        //GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                        //int geoid = OSalAttend.FirstOrDefault().GeoStruct.Id;

                                        //int payid = OSalAttend.FirstOrDefault().PayStruct.Id;

                                        //int funid = OSalAttend.FirstOrDefault().FuncStruct.Id;

                                        //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && LvDays != 0)
                                        {
                                            var enddate = ca.EmpSalStruct.Select(r => r.EndDate).FirstOrDefault();

                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = ca.Employee.Id.ToString(),
                                                Fld2 = ca.Employee.EmpCode,
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld4 = ca1 != null ? ca1.Code.ToString() : "",
                                                Fld5 = ca.EmpSalStruct.Select(e => e.EffectiveDate.Value.ToShortDateString()).FirstOrDefault(),
                                                //Fld6 = ca.EmpSalStruct.Select(e => e.EndDate.Value.ToShortDateString()).FirstOrDefault() == null ? "" : ca.EmpSalStruct.Select(e => e.EndDate.Value.ToShortDateString()).FirstOrDefault(), 
                                                Fld11 = LvDays.ToString(),
                                                Fld6 = enddate != null ? enddate.Value.ToShortDateString() : "",
                                                Fld12 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name

                                            };

                                            //OGenericObjStatement = GetFilterData(forithead, OGenericObjStatement, ca1.PayMonth.ToString(), ca.Employee.ToString(), GeoDataInd, PaydataInd, FuncdataInd);
                                            if (month)
                                            {
                                                //OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                            }
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
                                            if (emp)
                                            {
                                                OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }


                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }
                                    }
                                }

                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "LOANADVREPAYMENTPRINCIPLEANDINTEREST":
                        var OLoanAdvRepaymentDataPrincipleInterest = new List<EmployeePayroll>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OLoanAdvRepaymentData_temp = db.EmployeePayroll
                        .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //    .Include(e => e.Employee.GeoStruct)
                                //    .Include(e => e.Employee.PayStruct)
                                //    .Include(e => e.Employee.FuncStruct)
                                //    .Include(e => e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT))
                                //    .Include(e => e.LoanAdvRequest.Select(t => t.LoanAccBranch))
                                //     .Include(e => e.LoanAdvRequest.Select(t => t.LoanAccBranch.LocationObj))
                                //    .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                //     .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                           .FirstOrDefault();
                            if (OLoanAdvRepaymentData_temp != null)
                            {
                                OLoanAdvRepaymentData_temp.Employee.EmpName = db.NameSingle.Find(OLoanAdvRepaymentData_temp.Employee.EmpName_Id);
                                List<LoanAdvRequest> LoanAdvRequest = new List<LoanAdvRequest>();

                                LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OLoanAdvRepaymentData_temp.Id).ToList();
                                foreach (var i in LoanAdvRequest)
                                {

                                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == LoanAccBranch.Id).Select(r => r.LocationObj).FirstOrDefault();
                                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                                    var SalaryHead = db.LoanAdvanceHead.Where(e => e.Id == LoanAdvanceHead.Id).Select(r => r.SalaryHead).FirstOrDefault();

                                    var LoanAdvRepaymentT = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvRepaymentT.Where(e => mPayMonth.Contains(e.PayMonth.ToString())).ToList()).AsNoTracking().FirstOrDefault();
                                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                                    i.LoanAdvanceHead = LoanAdvanceHead;
                                    i.LoanAdvanceHead.SalaryHead = SalaryHead;

                                    i.LoanAdvRepaymentT = (LoanAdvRepaymentT);

                                }
                                OLoanAdvRepaymentData_temp.LoanAdvRequest = LoanAdvRequest;// db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT).Where(e => e.EmployeePayroll_Id == OLoanAdvRepaymentData_temp.Id && mPayMonth.Contains(e.LoanAdvRepaymentT.Select(p => p.PayMonth).ToString())).AsNoTracking().ToList();
                                OLoanAdvRepaymentDataPrincipleInterest.Add(OLoanAdvRepaymentData_temp);
                            }
                        }


                        if (OLoanAdvRepaymentDataPrincipleInterest == null || OLoanAdvRepaymentDataPrincipleInterest.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var loanhead = false;
                            var loanLocation = false;
                            var accno = false;
                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "LoanHeadwise")
                                {

                                    loanhead = true;
                                }
                                if (item1 == "LoanLoc")
                                {

                                    loanLocation = true;
                                }

                            }
                            List<string> HeadName = new List<string>();
                            if (salheadlist.Count() != 0)
                            {
                                HeadName.AddRange(salheadlist);
                            }
                            else
                            {
                                var a = db.LoanAdvRequest.Select(b => b.LoanAdvanceHead.SalaryHead.Name).Distinct().ToList();
                                List<string> te = new List<string>();
                                foreach (var item in a)
                                {
                                    HeadName.Add(item);
                                }
                            }
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

                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OLoanAdvRepaymentDataPrincipleInterest)
                            {

                                if (ca.LoanAdvRequest != null && ca.LoanAdvRequest.Count() != 0)
                                {
                                    //if (HeadName != null && HeadName.Count() != 0)
                                    //{
                                    foreach (var item in HeadName)
                                    {
                                        var OITTrans = ca.LoanAdvRequest.Where(a => a.LoanAdvanceHead.SalaryHead.Name.ToString() == item.ToString()).ToList();
                                        //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                        //  var OITTrans = ca.l //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                        if (OITTrans != null && OITTrans.Count() != 0)
                                        {
                                            if (ca.Employee.GeoStruct_Id != null && ca.Employee.FuncStruct_Id != null && ca.Employee.PayStruct_Id != null)
                                            {
                                                int? geoid = ca.Employee.GeoStruct_Id;

                                                int? payid = ca.Employee.PayStruct_Id;

                                                int? funid = ca.Employee.FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                                //int geoid = ca.Employee.GeoStruct.Id;

                                                //int payid = ca.Employee.PayStruct.Id;

                                                //int funid = ca.Employee.FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                foreach (var ca1 in OITTrans)
                                                {
                                                    var LoanBranch = "";
                                                    var LocBranch = "";
                                                    var Loc = "";
                                                    if (loanLocation == true)
                                                    {
                                                        LoanBranch = GeoDataInd.GeoStruct_Location_Name;
                                                        Loc = "Working Location";
                                                        LocBranch = ca1.LoanAccBranch.LocationObj.LocDesc.ToString();
                                                    }
                                                    else
                                                    {
                                                        LoanBranch = ca1.LoanAccBranch.LocationObj.LocDesc.ToString();
                                                        Loc = "Loan Location";
                                                        LocBranch = GeoDataInd.GeoStruct_Location_Name;

                                                    }

                                                    if (ca1.LoanAdvRepaymentT != null && ca1.LoanAdvRepaymentT.Count() != 0)
                                                    {
                                                        // else
                                                        // {
                                                        foreach (var ca2 in ca1.LoanAdvRepaymentT.Where(e => mPayMonth.Contains(e.PayMonth.ToString())).ToList())
                                                        {
                                                            GenericField100 OGenericObjStatement = new GenericField100()
                                                            //write data to generic class
                                                            {
                                                                Fld1 = ca.Employee == null ? null : ca.Employee.Id.ToString(),
                                                                Fld2 = ca.Employee != null ? ca.Employee.EmpCode : null,
                                                                Fld3 = ca.Employee != null && ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                                // Fld4 = ca1.Id != null ? ca1.Id.ToString() : null,
                                                                //  Fld5 = ca1.RequisitionDate != null ? ca1.RequisitionDate.ToString() : null,
                                                                //  Fld6 = ca1.EnforcementDate != null ? ca1.EnforcementDate.ToString() : null,
                                                                //  Fld7 = ca1.LoanAdvanceHead != null ? ca1.LoanAdvanceHead.Id.ToString() : null,
                                                                // Fld8 = ca1.LoanAdvanceHead != null ? ca1.LoanAdvanceHead.Code : null,
                                                                Fld9 = ca1.LoanAdvanceHead != null ? ca1.LoanAdvanceHead.Name : null,
                                                                // Fld10 = ca1.IsInstallment != null ? ca1.IsInstallment.ToString() : null,
                                                                //Fld11 = ca1.SanctionedDate != null ? ca1.SanctionedDate.Value.ToShortDateString() : null,
                                                                // Fld12 = ca1.LoanAppliedAmount != null ? ca1.LoanAppliedAmount.ToString() : null,
                                                                //  Fld13 = ca1.LoanSanctionedAmount != null ? ca1.LoanSanctionedAmount.ToString() : null,
                                                                //  Fld14 = ca1.TotalInstall != null ? ca1.TotalInstall.ToString() : null,
                                                                ////  Fld15 = ca1.IsActive != null ? ca1.IsActive.ToString() : null,
                                                                //  Fld16 = ca1.CloserDate != null ? ca1.CloserDate.ToString() : null,
                                                                //  Fld17 = ca2.Id != null ? ca2.Id.ToString() : null,
                                                                //  Fld18 = ca2.InstallementDate != null ? ca2.InstallementDate.Value.ToShortDateString() : null,
                                                                //  Fld19 = ca2.TotalLoanBalance != null ? ca2.TotalLoanBalance.ToString() : null,
                                                                //  Fld20 = ca2.TotalLoanPaid != null ? ca2.TotalLoanPaid.ToString() : null,
                                                                Fld21 = ca2.MonthlyPricipalAmount != null ? ca2.MonthlyPricipalAmount.ToString() : "0",
                                                                Fld22 = ca2.InstallmentPaid != null ? ca2.InstallmentPaid.ToString() : null,
                                                                Fld23 = ca2.MonthlyInterest != null ? ca2.MonthlyInterest.ToString() : "0",
                                                                Fld24 = ca2.PayMonth != null ? ca2.PayMonth : null,
                                                                // Fld25 = ca2.RepaymentDate != null ? ca2.RepaymentDate.ToString() : null,
                                                                Fld26 = ca1.LoanAccBranch != null ? ca1.LoanAccBranch.ToString() : null,
                                                                Fld27 = ca1.LoanAccNo != null ? ca1.LoanAccNo.ToString() : null,
                                                                //Fld30 = GeoDataInd == null ? "" : GeoDataInd.LocDesc,
                                                                Fld30 = LoanBranch,                                                /////loan branch
                                                                Fld31 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                                                // Fld28 = ca.Employee != null && ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                                Fld29 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,
                                                                Fld32 = Loc.ToString(),

                                                            };
                                                            //if (month)
                                                            //{
                                                            //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                            //}
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
                                                                OGenericObjStatement.Fld97 = LocBranch;
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
                                                            if (emp)
                                                            {
                                                                OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                            }

                                                            if (loanhead)
                                                            {
                                                                OGenericObjStatement.Fld87 = ca1.LoanAdvanceHead.Name;
                                                            }

                                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                        }

                                                    }

                                                }
                                            }
                                        }
                                    }

                                    // }

                                }
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            return OGenericPayrollStatement;
                        }
                        break;
                }

            }
            return null;
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