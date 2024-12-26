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
    public class AttendanceReportGen
    {
        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };


        public static List<GenericField100> GenerateAttendanceReport(int CompanyAttendanceId, List<int> EmpAttendanceIdList, List<string> mPayMonth, List<string> forithead, List<string> salheadlist, DateTime mFromDate, DateTime mToDate, DateTime pFromDate, DateTime pToDate, string FromTime, string ToTime, string mObjectName, int CompanyId, string MoreThanT, List<string> SpecialGroupslist)
        {
            List<GenericField100> OGenericAttendanceStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (mObjectName)
                {

                    //////////////////Process Reports

                    case "DAILYIN":
                        var OAttReportData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttReportData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.ProcessedData.Select(q => q.TimingCode))
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                 .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            if (OAttReportData_t != null)
                            {

                                OAttReportData_t.Employee.EmpName = db.NameSingle.Find(OAttReportData_t.Employee.EmpName_Id);
                                OAttReportData_t.ProcessedData = db.ProcessedData.Where(e => (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate) && e.EmployeeAttendance_Id == OAttReportData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttReportData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                }
                                OAttReportData.Add(OAttReportData_t);
                            }
                        }
                        if (OAttReportData == null || OAttReportData.Count() == 0)
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


                            foreach (var ca in OAttReportData)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.ProcessedData.OrderBy(e => e.SwipeDate).ToList();
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            {
                                                Fld2 = ca1.Id.ToString(),

                                                Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld4 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld6 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld7 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld8 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld9 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                Fld11 = ca.Employee.CardCode,
                                                Fld10 = ca1.TimingCode.InTime.Value.ToShortTimeString(),
                                                Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }

                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;


                    case "DAILYOUT":
                        var OAttOutReportData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttOutReportData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.ProcessedData.Select(q => q.TimingCode))
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                  .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                    .FirstOrDefault();
                            if (OAttOutReportData_t != null)
                            {
                                OAttOutReportData_t.Employee.EmpName = db.NameSingle.Find(OAttOutReportData_t.Employee.EmpName_Id);
                                OAttOutReportData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttOutReportData_t.Id && (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttOutReportData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                }
                                OAttOutReportData.Add(OAttOutReportData_t);
                            }
                        }

                        if (OAttOutReportData == null || OAttOutReportData.Count() == 0)
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

                            foreach (var ca in OAttOutReportData)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.ProcessedData;
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            {
                                                //Fld1 = ca1.Id.ToString(),
                                                //Fld2 = ca1.Id.ToString(),
                                                Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld4 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld6 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld7 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld8 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld9 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld11 = ca.Employee.CardCode,
                                                Fld10 = ca1.TimingCode.OutTime.Value.ToShortTimeString(),
                                                Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd.FuncStruct_Job_Name,
                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "OUTDOORDUTY":
                        var OUTDOORDUTYData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttReportData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                 .Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                // .Include(e => e.ProcessedData.Select(q => q.TimingCode))
                                 .Include(e => e.OutDoorDutyReq)
                                  .Include(e => e.OutDoorDutyReq.Select(x => x.TimingPolicy))
                                   .Include(e => e.OutDoorDutyReq.Select(x => x.ProcessedData))
                                   .Include(e => e.OutDoorDutyReq.Select(x => x.ProcessedData.TimingCode))
                                   .Include(e => e.OutDoorDutyReq.Select(x => x.WFDetails))
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                 .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            if (OAttReportData_t != null)
                            {
                                //OAttReportData_t.Employee.EmpName = db.NameSingle.Find(OAttReportData_t.Employee.EmpName_Id);
                                //OAttReportData_t.OutDoorDutyReq = db.OutDoorDutyReq.Where(e => e.EmployeeAttendance_Id == OAttReportData_t.Id).OrderBy(e => e.Id).AsNoTracking().AsParallel().ToList();
                                //foreach (var item1 in OAttReportData_t.OutDoorDutyReq)
                                //{ 
                                //    item1.TimingPolicy = db.TimingPolicy.Find(item1.TimingPolicy_Id);
                                //    item1.ProcessedData = db.ProcessedData.Find(item1.ProcessedData_Id);
                                //    item1.ProcessedData.TimingCode = db.TimingPolicy.Find(item1.ProcessedData.TimingCode_Id);
                                //    item1.WFDetails = db.OutDoorDutyReq.Where(e => e.Id == item1.Id).Select(e => e.WFDetails).FirstOrDefault();
                                //}
                                OUTDOORDUTYData.Add(OAttReportData_t);
                            }
                        }
                        if (OUTDOORDUTYData == null || OUTDOORDUTYData.Count() == 0)
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

                            foreach (var ca in OUTDOORDUTYData)
                            {
                                if (ca.OutDoorDutyReq != null && ca.OutDoorDutyReq.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.OutDoorDutyReq.Where(e => (e.ProcessedData != null && e.ProcessedData.SwipeDate.Value.Date >= pFromDate && e.ProcessedData.SwipeDate.Value.Date <= pToDate)).OrderBy(e => e.ProcessedData.SwipeDate).ToList();

                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {
                                            string WFStatus = "";
                                            if (ca1.WFDetails.Count() > 0)
                                            {
                                                if (ca1.WFDetails.LastOrDefault().WFStatus == 0)
                                                    WFStatus = "Applied";
                                                else if (ca1.WFDetails.LastOrDefault().WFStatus == 1)
                                                    WFStatus = "Sanctioned And Approved";
                                                else if (ca1.WFDetails.LastOrDefault().WFStatus == 2)
                                                    WFStatus = "Rejected By Sanction";
                                                else if (ca1.WFDetails.LastOrDefault().WFStatus == 3)
                                                    WFStatus = "Apporved By HR";
                                                else if (ca1.WFDetails.LastOrDefault().WFStatus == 4)
                                                    WFStatus = "Rejected By HR";
                                                else if (ca1.WFDetails.LastOrDefault().WFStatus == 5)
                                                    WFStatus = "HRM(M)";
                                                else if (ca1.WFDetails.LastOrDefault().WFStatus == 6)
                                                    WFStatus = "Cancel";
                                            }
                                            else
                                            {
                                                WFStatus = "Apporved By HR";
                                            }
                                            if (salheadlist.Count() > 0)
                                            {
                                                foreach (var expression in salheadlist)
                                                {
                                                    if (expression.ToUpper() == WFStatus.ToUpper())
                                                    {
                                                        GenericField100 OGenericAttObjStatement = new GenericField100()
                                                        {
                                                            Fld2 = ca1.Id.ToString(),
                                                            Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                            Fld4 = GeoDataInd.GeoStruct_Department_Name,
                                                            Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                            Fld6 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                            Fld7 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                            Fld8 = ca1.ProcessedData.SwipeDate == null ? "" : ca1.ProcessedData.SwipeDate.Value.ToShortDateString(),
                                                            Fld9 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                            Fld10 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                            Fld11 = ca.Employee.CardCode,
                                                            Fld12 = ca1.ProcessedData.InTime != null ? ca1.ProcessedData.InTime.Value.ToShortTimeString() : "",
                                                            Fld13 = ca1.ProcessedData.OutTime != null ? ca1.ProcessedData.OutTime.Value.ToShortTimeString() : "",
                                                            Fld14 = ca1.ProcessedData.TimingCode.InTime != null ? ca1.ProcessedData.TimingCode.InTime.Value.ToShortTimeString() : "",
                                                            Fld15 = ca1.ProcessedData.TimingCode.OutTime != null ? ca1.ProcessedData.TimingCode.OutTime.Value.ToShortTimeString() : "",
                                                            Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                            Fld17 = GeoDataInd.FuncStruct_Job_Name,
                                                            Fld18 = ca1.Reason,
                                                            Fld19 = WFStatus
                                                        };
                                                        //if (month)
                                                        //{
                                                        //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                        //}
                                                        if (comp)
                                                        {
                                                            OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                        }
                                                        if (grade)
                                                        {
                                                            OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                        }
                                                        if (lvl)
                                                        {
                                                            OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                        }
                                                        if (jobstat)
                                                        {
                                                            OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                        }
                                                        if (job)
                                                        {
                                                            OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                        }
                                                        if (jobpos)
                                                        {
                                                            OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                        }
                                                        if (emp)
                                                        {
                                                            OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                        }

                                                        OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {

                                                    Fld2 = ca1.Id.ToString(),
                                                    Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld4 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld6 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld7 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    Fld8 = ca1.ProcessedData.SwipeDate == null ? "" : ca1.ProcessedData.SwipeDate.Value.ToShortDateString(),
                                                    Fld9 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                    Fld10 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                    Fld11 = ca.Employee.CardCode,
                                                    Fld12 = ca1.ProcessedData.InTime != null ? ca1.ProcessedData.InTime.Value.ToShortTimeString() : "",
                                                    Fld13 = ca1.ProcessedData.OutTime != null ? ca1.ProcessedData.OutTime.Value.ToShortTimeString() : "",
                                                    Fld14 = ca1.ProcessedData.TimingCode.InTime != null ? ca1.ProcessedData.TimingCode.InTime.Value.ToShortTimeString() : "",
                                                    Fld15 = ca1.ProcessedData.TimingCode.OutTime != null ? ca1.ProcessedData.TimingCode.OutTime.Value.ToShortTimeString() : "",
                                                    Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld17 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld18 = ca1.Reason,
                                                    Fld19 = WFStatus
                                                };
                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "ODNOTFILL":
                        List<string> MusterRemark = new List<string>();
                        var ORemarkConfig = db.RemarkConfig.Include(e => e.MusterRemarks).Where(e => e.IsODAppl == true).ToList();
                        if (ORemarkConfig != null)
                        {
                            MusterRemark = ORemarkConfig.Select(y => y.MusterRemarks.LookupVal).ToList();
                        }
                        var EmpODnotfillRecord = db.EmployeeAttendance.Select(p => new
                        {
                            EmployeeId = p.Employee.Id,
                            EmpCode = p.Employee.EmpCode,
                            EmpName = p.Employee.EmpName.FullNameFML,
                            PayProcessGroupId = p.Employee.EmpOffInfo.PayProcessGroup.Id,
                            GeoStructId = p.Employee.GeoStruct.Id,
                            PayStructId = p.Employee.PayStruct.Id,
                            FuncStructId = p.Employee.FuncStruct.Id,
                            ProcessedData = p.ProcessedData.Select(r => new
                            {
                                Id = r.Id,
                                SwipeDate = r.SwipeDate,
                                MusterRemarks = r.MusterRemarks.LookupVal,
                            }).Where(e => MusterRemark.Contains(e.MusterRemarks)).ToList(),
                        }).Where(e => EmpAttendanceIdList.Contains(e.EmployeeId)).ToList();

                        if (EmpODnotfillRecord == null || EmpODnotfillRecord.Count() == 0)
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

                            foreach (var ca in EmpODnotfillRecord)
                            {
                                var ODfillrecordexclude = db.OutDoorDutyReq.Where(e => e.TrReject != true || e.TrClosed != true).ToList().Select(r => r.ProcessedData_Id);

                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.GeoStructId;

                                    int? payid = ca.PayStructId;

                                    int? funid = ca.FuncStructId;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == ca.PayProcessGroupId)
                                    .SingleOrDefault().PayrollPeriod.FirstOrDefault();
                                    int startday = query1.StartDate;
                                    int endday = query1.EndDate;
                                    string mPayMonth1 = mPayMonth.FirstOrDefault();
                                    DateTime _PayMonth = Convert.ToDateTime("01/" + mPayMonth1);
                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date;
                                    DateTime FromPeriod;
                                    DateTime EndPeriod;
                                    DateTime CurrentmonthEnd;
                                    DateTime Prevmonthstart;
                                    int daysInEndMonth = end.Day;
                                    int daysInstartMonth = 1;

                                    int daym = (Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date).Day;
                                    if (endday > daym)
                                    {
                                        endday = daym;
                                    }
                                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                                    {
                                        FromPeriod = _PayMonth;
                                        EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                                    }
                                    else
                                    {
                                        DateTime prvmonth = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(-1).Date;
                                        startday = endday + 1;
                                        string pmonth = prvmonth.ToString("MM/yyyy");
                                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);
                                        EndPeriod = Convert.ToDateTime(endday + "/" + mPayMonth1);

                                    }
                                    CurrentmonthEnd = EndPeriod;
                                    Prevmonthstart = FromPeriod;

                                    foreach (var ca1 in ca.ProcessedData.Where(a => (a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                         a.SwipeDate.Value.Date <= CurrentmonthEnd) && !ODfillrecordexclude.Contains(a.Id)))
                                    {
                                        GenericField100 OGenericAttObjStatement = new GenericField100()
                                        {
                                            Fld2 = ca.EmpCode == null ? "" : ca.EmpCode,
                                            Fld3 = ca.EmpName == null ? "" : ca.EmpName,
                                            Fld4 = GeoDataInd.GeoStruct_Location_Name,
                                            Fld5 = GeoDataInd.GeoStruct_Department_Name,
                                            Fld6 = GeoDataInd.FuncStruct_Job_Name,
                                            Fld7 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                            Fld8 = ca1.MusterRemarks == null ? "" : ca1.MusterRemarks,
                                        };
                                        //if (month)
                                        //{
                                        //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                        //}
                                        if (comp)
                                        {
                                            OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        if (emp)
                                        {
                                            OGenericAttObjStatement.Fld88 = ca.EmpName;
                                        }

                                        OGenericAttendanceStatement.Add(OGenericAttObjStatement);




                                    }

                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "EXCEPTIONALREMARK":
                        //var OAttendanceReportExceptionalRemark = new List<EmployeeAttendance>();
                        //foreach (var item in EmpAttendanceIdList)
                        //{
                        //    var OAttendanceReportBuilderData_t = db.EmployeeAttendance
                        //         .Include(e => e.Employee)
                        //         .Include(e => e.Employee.EmpName)
                        //.Include(e => e.Employee.GeoStruct)
                        //.Include(e => e.Employee.GeoStruct.Location)
                        //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //.Include(e => e.Employee.GeoStruct.Department)
                        //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        //.Include(e => e.Employee.PayStruct.Grade)
                        //.Include(e => e.Employee.PayStruct)
                        // .Include(e => e.Employee.FuncStruct)
                        //  .Include(e => e.Employee.FuncStruct.Job)
                        //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                        //.Include(e => e.ProcessedData.Select(r => r.TimingCode))
                        //          .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                        // .FirstOrDefault();
                        //if (OAttendanceReportBuilderData_t != null)
                        //{
                        //    OAttendanceReportBuilderData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceReportBuilderData_t.Employee.EmpName_Id);
                        //    OAttendanceReportBuilderData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceReportBuilderData_t.Id && (e.SwipeDate.Value >= pFromDate && e.SwipeDate.Value <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                        //    foreach (var item1 in OAttendanceReportBuilderData_t.ProcessedData)
                        //    {
                        //        item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                        //        item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                        //    }
                        //    OAttendanceReportExceptionalRemark.Add(OAttendanceReportBuilderData_t);
                        //}

                        //}

                        var OAttendanceReportExceptionalRemark = db.EmployeeAttendance.Select(r => new
                        {
                            iEmployeeId = r.Employee.Id,
                            iEmpCode = r.Employee.EmpCode,
                            iEmpName = r.Employee.EmpName.FullNameFML,
                            iGeoStruct_Id = r.Employee.GeoStruct_Id,
                            iPayStruct_Id = r.Employee.PayStruct_Id,
                            iFuncStruct_Id = r.Employee.FuncStruct_Id,
                            iProcessedData = r.ProcessedData.Select(l => new
                            {
                                iSwipeDate = l.SwipeDate,
                                iMusterRemarks = l.MusterRemarks.LookupVal,
                                iTimingCodeInTime = l.TimingCode.InTime,
                                iTimingCodeOutTime = l.TimingCode.OutTime,
                                iOutTime = l.OutTime,
                                iInTime = l.InTime,
                                iTotWhLunch = l.TotWhLunch

                            }).Where(e => e.iSwipeDate >= pFromDate && e.iSwipeDate <= pToDate).ToList()
                        }).Where(e => EmpAttendanceIdList.Contains(e.iEmployeeId)).ToList();

                        if (OAttendanceReportExceptionalRemark == null || OAttendanceReportExceptionalRemark.Count() == 0)
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



                            foreach (var ca in OAttendanceReportExceptionalRemark)
                            {
                                if (ca.iProcessedData != null && ca.iProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.iGeoStruct_Id;

                                    int? payid = ca.iPayStruct_Id;

                                    int? funid = ca.iFuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    //if (salheadlist != null && salheadlist.Count() != 0)
                                    //{
                                    //foreach (var item in salheadlist)
                                    //{
                                    var OAttData = ca.iProcessedData.Where(e => salheadlist.Contains(e.iMusterRemarks)).ToList();

                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        if (SpecialGroupslist != null && SpecialGroupslist.Count() != 0)
                                        {
                                            foreach (var item1 in SpecialGroupslist)
                                            {
                                                if (OAttData.Count >= Convert.ToInt32(item1))
                                                {
                                                    foreach (var ca1 in OAttData)
                                                    {
                                                        var CompTimediff = Convert.ToDateTime(ca1.iTimingCodeOutTime) - Convert.ToDateTime(ca1.iTimingCodeInTime);
                                                        var EmpTimediff = Convert.ToDateTime(ca1.iOutTime) - Convert.ToDateTime(ca1.iInTime);
                                                        var compare = "";
                                                        if (ca1.iMusterRemarks == "HO")
                                                        {
                                                            compare = CompTimediff.ToString("hh\\:mm");
                                                        }
                                                        else if (ca1.iMusterRemarks == "WO")
                                                        {
                                                            compare = CompTimediff.ToString("hh\\:mm");
                                                        }
                                                        else if (ca1.iMusterRemarks == "LV")
                                                        {
                                                            compare = CompTimediff.ToString("hh\\:mm");
                                                        }
                                                        else
                                                        {
                                                            compare = EmpTimediff.ToString("hh\\:mm");
                                                        }

                                                        GenericField100 OGenericAttObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                            Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                            Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                            Fld4 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                                            Fld5 = ca.iEmpName == null ? "" : ca.iEmpName,
                                                            Fld6 = ca1.iSwipeDate == null ? "" : ca1.iSwipeDate.Value.ToShortDateString(),
                                                            Fld8 = ca1.iOutTime == null ? "" : ca1.iOutTime.Value.ToShortTimeString(),
                                                            Fld7 = ca1.iInTime == null ? "" : ca1.iInTime.Value.ToShortTimeString(),
                                                            Fld9 = ca1.iTimingCodeInTime == null ? "" : ca1.iTimingCodeInTime.Value.ToShortTimeString(),
                                                            // Fld9 = ca1.MInTime == null ? "" : ca1.MInTime.Value.ToShortTimeString(),
                                                            Fld10 = ca1.iTimingCodeOutTime == null ? "" : ca1.iTimingCodeOutTime.Value.ToShortTimeString(),
                                                            //Fld10 = ca1.MOutTime == null ? "" : ca1.MOutTime.Value.ToShortTimeString(),
                                                            Fld12 = ca1.iMusterRemarks == null ? "" : ca1.iMusterRemarks,
                                                            Fld13 = ca1.iTotWhLunch == null ? "" : ca1.iTotWhLunch.Value.ToShortDateString(),
                                                            Fld14 = CompTimediff.ToString("hh\\:mm"),
                                                            Fld15 = compare,
                                                            Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                            Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                                        };

                                                        if (comp)
                                                        {
                                                            OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                        }
                                                        if (div)
                                                        {
                                                            OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                        }
                                                        if (loca)
                                                        {
                                                            OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Code;
                                                        }
                                                        if (dept)
                                                        {
                                                            OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                        }
                                                        if (grp)
                                                        {
                                                            OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                        }
                                                        if (unit)
                                                        {
                                                            OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                        }
                                                        if (grade)
                                                        {
                                                            OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                        }
                                                        if (lvl)
                                                        {
                                                            OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                        }
                                                        if (jobstat)
                                                        {
                                                            OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                        }
                                                        if (job)
                                                        {
                                                            OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                        }
                                                        if (jobpos)
                                                        {
                                                            OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                        }
                                                        if (emp)
                                                        {
                                                            OGenericAttObjStatement.Fld88 = ca.iEmpName;
                                                        }

                                                        OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (var ca1 in OAttData)
                                            {
                                                var CompTimediff = Convert.ToDateTime(ca1.iTimingCodeOutTime) - Convert.ToDateTime(ca1.iTimingCodeInTime);
                                                var EmpTimediff = Convert.ToDateTime(ca1.iOutTime) - Convert.ToDateTime(ca1.iInTime);
                                                var compare = "";
                                                if (ca1.iMusterRemarks == "HO")
                                                {
                                                    compare = CompTimediff.ToString("hh\\:mm");
                                                }
                                                else if (ca1.iMusterRemarks == "WO")
                                                {
                                                    compare = CompTimediff.ToString("hh\\:mm");
                                                }
                                                else if (ca1.iMusterRemarks == "LV")
                                                {
                                                    compare = CompTimediff.ToString("hh\\:mm");
                                                }
                                                else
                                                {
                                                    compare = EmpTimediff.ToString("hh\\:mm");
                                                }

                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld4 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                                    Fld5 = ca.iEmpName == null ? "" : ca.iEmpName,
                                                    Fld6 = ca1.iSwipeDate == null ? "" : ca1.iSwipeDate.Value.ToShortDateString(),
                                                    Fld8 = ca1.iOutTime == null ? "" : ca1.iOutTime.Value.ToShortTimeString(),
                                                    Fld7 = ca1.iInTime == null ? "" : ca1.iInTime.Value.ToShortTimeString(),
                                                    Fld9 = ca1.iTimingCodeInTime == null ? "" : ca1.iTimingCodeInTime.Value.ToShortTimeString(),
                                                    // Fld9 = ca1.MInTime == null ? "" : ca1.MInTime.Value.ToShortTimeString(),
                                                    Fld10 = ca1.iTimingCodeOutTime == null ? "" : ca1.iTimingCodeOutTime.Value.ToShortTimeString(),
                                                    //Fld10 = ca1.MOutTime == null ? "" : ca1.MOutTime.Value.ToShortTimeString(),
                                                    Fld12 = ca1.iMusterRemarks == null ? "" : ca1.iMusterRemarks,
                                                    Fld13 = ca1.iTotWhLunch == null ? "" : ca1.iTotWhLunch.Value.ToShortDateString(),
                                                    Fld14 = CompTimediff.ToString("hh\\:mm"),
                                                    Fld15 = compare,
                                                    Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                                };

                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Code;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.iEmpName;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }


                                        }
                                    }
                                    //}
                                    //}
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;


                    case "EXCEPTIONAL":

                        var PayMnthex = mPayMonth.SingleOrDefault();
                        //var FrmTime = DateTime.Parse(FromTime);
                        //var TTime = Convert.ToDateTime(ToTime).ToString("hh\\:mm");


                        var OPunchDataex = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OPunchData_t = db.EmployeeAttendance
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                // .Include(e => e.Employee.PayStruct)
                                //  .Include(e => e.Employee.FuncStruct)
                                //   .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                // .Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.ProcessedData)
                                //.Include(e => e.ProcessedData.Select(x => x.TimingCode))
                                .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            if (OPunchData_t != null)
                            {
                                OPunchData_t.Employee.EmpName = db.NameSingle.Find(OPunchData_t.Employee.EmpName_Id);
                                OPunchData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OPunchData_t.Id && (e.SwipeDate.Value >= pFromDate && e.SwipeDate.Value <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OPunchData_t.ProcessedData)
                                {
                                    //item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                }
                                OPunchDataex.Add(OPunchData_t);
                            }

                        }
                        if (OPunchDataex == null || OPunchDataex.Count() == 0)
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

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();


                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            foreach (var ca in OPunchDataex)
                            {

                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                    //var OAttRawData_T = ca.RawData.Where(e =>e.SwipeTime >= FromTime && e.SwipeTime <= ToTime).ToString();

                                    //var OAttRawData = ca.ProcessedData.Where(e => e.SwipeDate.Value.ToString("MM/yyyy") == PayMnthex && e.InTime != null).OrderBy(e => e.SwipeDate).ToList();
                                    var OAttRawData = ca.ProcessedData;//.Where(e => e.SwipeDate.Value.Date >= pFromDate && e.SwipeDate.Value.Date <= pToDate && e.InTime != null).OrderBy(e => e.SwipeDate).ToList();


                                    if (OAttRawData != null && OAttRawData.Count() != 0)
                                    {

                                        foreach (var ca1 in OAttRawData)
                                        {
                                            Boolean t3t4 = false;
                                            TimeSpan t1 = new TimeSpan(Convert.ToInt32(MoreThanT.Split(':')[0]), Convert.ToInt32(MoreThanT.Split(':')[1]), 0);
                                            //  TimeSpan t2 = new TimeSpan(Convert.ToInt32(ToTime.Split(':')[0]), Convert.ToInt32(ToTime.Split(':')[1]), 0);
                                            if (salheadlist.ToList().FirstOrDefault().ToUpper() == "LATECOME")
                                            {
                                                //company in and out time
                                                //  TimeSpan t2 = new TimeSpan(Convert.ToInt32(ca1.MInTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.MInTime.Value.ToShortTimeString().Split(':')[1]), 0);
                                                if (ca1.TimingCode.InTime != null && ca1.InTime != null)
                                                {
                                                    TimeSpan t2 = new TimeSpan(Convert.ToInt32(ca1.TimingCode.InTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.TimingCode.InTime.Value.ToShortTimeString().Split(':')[1]), 0);


                                                    TimeSpan t3 = new TimeSpan(Convert.ToInt32(ca1.InTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.InTime.Value.ToShortTimeString().Split(':')[1]), 0);
                                                    TimeSpan t4 = t1 + t2;
                                                    if (t3 >= t4)
                                                    {
                                                        t3t4 = true;
                                                    }
                                                }
                                            }
                                            else if (salheadlist.ToList().FirstOrDefault().ToUpper() == "EARLYGONE")
                                            {
                                                //company in and out time
                                                // TimeSpan t2 = new TimeSpan(Convert.ToInt32(ca1.MOutTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.MOutTime.Value.ToShortTimeString().Split(':')[1]), 0);
                                                if (ca1.TimingCode.OutTime != null && ca1.OutTime != null)
                                                {
                                                    TimeSpan t2 = new TimeSpan(Convert.ToInt32(ca1.TimingCode.OutTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.TimingCode.OutTime.Value.ToShortTimeString().Split(':')[1]), 0);

                                                    TimeSpan t3 = new TimeSpan(Convert.ToInt32(ca1.OutTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.OutTime.Value.ToShortTimeString().Split(':')[1]), 0);
                                                    TimeSpan t4 = t2 - t1;
                                                    if (t3 <= t4)
                                                    {
                                                        t3t4 = true;
                                                    }
                                                }


                                            }

                                            string Compinout = string.Empty;
                                            string Empinout = string.Empty;
                                            if (salheadlist.ToList().FirstOrDefault().ToUpper() == "LATECOME")
                                            {
                                                // Compinout = ca1.MInTime == null ? "" : ca1.MInTime.Value.ToShortTimeString();
                                                Compinout = ca1.TimingCode.InTime == null ? "" : ca1.TimingCode.InTime.Value.ToShortTimeString();

                                                Empinout = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString();
                                            }
                                            else if (salheadlist.ToList().FirstOrDefault().ToUpper() == "EARLYGONE")
                                            {
                                                //Compinout = ca1.MOutTime == null ? "" : ca1.MOutTime.Value.ToShortTimeString();
                                                Compinout = ca1.TimingCode.OutTime == null ? "" : ca1.TimingCode.OutTime.Value.ToShortTimeString();
                                                Empinout = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString();
                                            }


                                            if (t3t4 == true)
                                            {

                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {
                                                    //Fld2 = ca.Id.ToString(),
                                                    Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld4 = GeoDataInd.GeoStruct_Department_Name,

                                                    Fld6 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld10 = ca.Employee.CardCode,
                                                    Fld7 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    Fld8 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                    Fld9 = Empinout,
                                                    Fld12 = Compinout,
                                                    Fld11 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                                };

                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;


                    case "DAILYSWIPE":
                        List<EmployeeAttendance> OdailyswipeReportData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OdailyswipeReportData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                // .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.ProcessedData.Select(q => q.TimingCode))
                                 .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OdailyswipeReportData_t != null)
                            {
                                OdailyswipeReportData_t.Employee.EmpName = db.NameSingle.Find(OdailyswipeReportData_t.Employee.EmpName_Id);
                                OdailyswipeReportData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OdailyswipeReportData_t.Id && (e.SwipeDate.Value >= pFromDate && e.SwipeDate.Value <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OdailyswipeReportData_t.ProcessedData)
                                {
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                }
                                OdailyswipeReportData.Add(OdailyswipeReportData_t);
                            }
                        }
                        if (OdailyswipeReportData == null || OdailyswipeReportData.Count() == 0)
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

                            foreach (var ca in OdailyswipeReportData)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
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

                                    var OAttData = ca.ProcessedData;//.Where(e => (e.SwipeDate.Value.Date >= pFromDate && e.SwipeDate.Value.Date <= pToDate)).OrderBy(e => e.SwipeDate).ToList();
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            {
                                                Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld4 = ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca.Employee.EmpCode,
                                                Fld13 = ca.Employee.CardCode,
                                                Fld7 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld9 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld11 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                Fld10 = ca1.TimingCode != null ? ca1.TimingCode.InTime.Value.ToShortTimeString() : "",
                                                Fld12 = ca1.TimingCode != null ? ca1.TimingCode.OutTime.Value.ToShortTimeString() : "",
                                                Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "INTIMEBASEDMONTHLYROASTER":
                        var OAttendanceDataRt = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceDataRt_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.EmpTimingRoasterData)
                                //.Include(e => e.EmpTimingRoasterData.Select(z => z.DayType))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceDataRt_t != null)
                            {
                                OAttendanceDataRt_t.Employee.EmpName = db.NameSingle.Find(OAttendanceDataRt_t.Employee.EmpName_Id);
                                OAttendanceDataRt_t.EmpTimingRoasterData = db.EmpTimingRoasterData.Where(e => e.EmployeeAttendance_Id == OAttendanceDataRt_t.Id && (e.RoasterDate.Value >= pFromDate && e.RoasterDate.Value <= pToDate)).OrderBy(e => e.RoasterDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttendanceDataRt_t.EmpTimingRoasterData)
                                {
                                    item1.DayType = db.LookupValue.Find(item1.DayType_Id);
                                }
                                OAttendanceDataRt.Add(OAttendanceDataRt_t);
                            }
                        }
                        if (OAttendanceDataRt == null || OAttendanceDataRt.Count() == 0)
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



                            foreach (var ca in OAttendanceDataRt)
                            {
                                if (ca.EmpTimingRoasterData != null && ca.EmpTimingRoasterData.Count() != 0)
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

                                    var OAttData = ca.EmpTimingRoasterData.Where(e => (e.RoasterDate.Value.Date >= pFromDate && e.RoasterDate.Value.Date <= pToDate)).ToList();
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {

                                        foreach (var ca1 in OAttData)
                                        {

                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca1.RoasterDate == null ? "" : ca1.RoasterDate.Value.ToShortDateString(),
                                                Fld8 = ca1.DayType == null ? "" : ca1.DayType.LookupVal.ToString(),
                                                Fld7 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }


                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "TIMEPOLICYBASEDMONTHLYROASTER":
                        var OAttendanceDataR = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceDataR_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.EmpTimingMonthlyRoaster)
                                //.Include(e => e.EmpTimingMonthlyRoaster.Select(z => z.TimingPolicy))
                                //.Include(e => e.EmpTimingMonthlyRoaster.Select(z => z.DayType))
                                //.Include(e => e.EmpTimingMonthlyRoaster.Select(x => x.TimingMonthlyRoaster))
                                //.Include(e => e.EmpTimingMonthlyRoaster.Select(x => x.TimingMonthlyRoaster.TimingGroup))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceDataR_t != null)
                            {
                                OAttendanceDataR_t.Employee.EmpName = db.NameSingle.Find(OAttendanceDataR_t.Employee.EmpName_Id);
                                OAttendanceDataR_t.EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster.Where(e => e.EmployeeAttendance_Id == OAttendanceDataR_t.Id && (e.RoasterDate.Value >= pFromDate && e.RoasterDate.Value <= pToDate)).OrderBy(e => e.RoasterDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttendanceDataR_t.EmpTimingMonthlyRoaster)
                                {
                                    item1.TimingPolicy = db.TimingPolicy.Find(item1.TimingPolicy_Id);
                                    item1.DayType = db.LookupValue.Find(item1.DayType_Id);
                                    item1.TimingMonthlyRoaster = db.TimingMonthlyRoaster.Find(item1.TimingMonthlyRoaster_Id);
                                    if (item1.TimingMonthlyRoaster != null)
                                    {
                                        item1.TimingMonthlyRoaster.TimingGroup = db.TimingGroup.Find(item1.TimingMonthlyRoaster.TimingGroup_Id);
                                    }
                                }
                                OAttendanceDataR.Add(OAttendanceDataR_t);
                            }
                        }
                        if (OAttendanceDataR == null || OAttendanceDataR.Count() == 0)
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
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);


                            foreach (var ca in OAttendanceDataR)
                            {
                                if (ca.EmpTimingMonthlyRoaster != null && ca.EmpTimingMonthlyRoaster.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.EmpTimingMonthlyRoaster;//.Where(e => (e.RoasterDate.Value.Date >= pFromDate && e.RoasterDate.Value.Date <= pToDate)).ToList();
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {

                                        foreach (var ca1 in OAttData)
                                        {

                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                //Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                //Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                //Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld1 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Location_Name + (GeoDataInd.GeoStruct_Department_Name == null ? "" : "/") + GeoDataInd.GeoStruct_Department_Name,
                                                Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca1.RoasterDate == null ? "" : ca1.RoasterDate.Value.ToShortDateString(),
                                                Fld8 = ca1.DayType == null ? "" : ca1.DayType.LookupVal.ToString(),
                                                Fld7 = ca1.TimingPolicy == null ? "" : ca1.TimingPolicy.TimingCode.ToString(),
                                                Fld9 = ca1.TimingMonthlyRoaster != null && ca1.TimingMonthlyRoaster.TimingGroup != null ? ca1.TimingMonthlyRoaster.TimingGroup.GroupName.ToString() : "",

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }


                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "ROASTERGROUP":

                        var fall = db.TimingMonthlyRoaster
                        .Include(e => e.TimingGroup)
                        .ToList();

                        var ORoastergrpData = fall.GroupBy(e => e.TimingGroup.GroupCode).Select(e => e.FirstOrDefault()).ToList();


                        if (ORoastergrpData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in ORoastergrpData)
                            {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    // Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.RoasterName == null ? "" : ca.RoasterName.ToString(),
                                    Fld3 = ca.TimingGroup == null ? "" : ca.TimingGroup.GroupCode.ToString(),
                                    Fld4 = ca.TimingGroup.GroupName.ToString()

                                };
                                //write data to generic class
                                OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "TIMINGPOLICYBATCH":

                        var OTimingpolicybatchData = db.TimingPolicyBatchAssignment
                        .Include(e => e.TimingweeklySchedule)
                        .Include(e => e.TimingGroup)
                        .ToList();

                        if (OTimingpolicybatchData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OTimingpolicybatchData)
                            {
                                var tw = ca.TimingweeklySchedule.ToList();
                                if (tw.Count() > 0)
                                {
                                    foreach (var item in tw)
                                    {
                                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                        {

                                            Fld2 = ca.PolicyBatchName == null ? "" : ca.PolicyBatchName.ToString(),
                                            Fld3 = ca.IsTimingGroup == false ? "No" : "Yes",
                                            Fld4 = ca.TimingGroup != null ? ca.TimingGroup.GroupCode.ToString() : "",
                                            Fld5 = ca.TimingGroup != null ? ca.TimingGroup.GroupName.ToString() : "",
                                            Fld6 = ca.IsWeeklyTimingSchedule == false ? "No" : "Yes",
                                            Fld7 = item.Description.ToString(),
                                            Fld8 = ca.IsRoaster == false ? "No" : "Yes"

                                        };
                                        //write data to generic class
                                        OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                                    }
                                }
                                else
                                {
                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {

                                        Fld2 = ca.PolicyBatchName == null ? "" : ca.PolicyBatchName.ToString(),
                                        Fld3 = ca.IsTimingGroup == false ? "No" : "Yes",
                                        Fld4 = ca.TimingGroup != null ? ca.TimingGroup.GroupCode.ToString() : "",
                                        Fld5 = ca.TimingGroup != null ? ca.TimingGroup.GroupName.ToString() : "",
                                        Fld6 = ca.IsWeeklyTimingSchedule == false ? "No" : "Yes",

                                        Fld8 = ca.IsRoaster == false ? "No" : "Yes"
                                    };
                                    //write data to generic class
                                    OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                                }

                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "TIMINGPOLICYBATCHASSIGNMENT":

                        var OTimingpolicybatchAssignmentData = db.OrgTimingPolicyBatchAssignment
                        .Include(e => e.TimingPolicyBatchAssignment)
                        .Include(e => e.Geostruct)
                        .Include(e => e.Geostruct.Location)
                        .Include(e => e.Geostruct.Location.LocationObj)
                        .Include(e => e.FuncStruct)
                        .Include(e => e.FuncStruct.Job)
                        .ToList();

                        if (OTimingpolicybatchAssignmentData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OTimingpolicybatchAssignmentData)
                            {
                                var tw = ca.TimingPolicyBatchAssignment.ToList();
                                if (tw.Count() > 0)
                                {
                                    foreach (var item in tw)
                                    {
                                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                        {
                                            Fld2 = ca.Geostruct != null && ca.Geostruct.Location != null && ca.Geostruct.Location.LocationObj != null ? ca.Geostruct.Location.LocationObj.LocDesc.ToString() : "",
                                            Fld3 = ca.FuncStruct != null && ca.FuncStruct.Job != null ? ca.FuncStruct.Job.Name.ToString() : "",
                                            Fld4 = item.PolicyBatchName == null ? "" : item.PolicyBatchName.ToString(),
                                            Fld5 = item.IsTimingGroup == false ? "No" : "Yes",
                                            Fld6 = item.IsWeeklyTimingSchedule == false ? "No" : "Yes",
                                            Fld7 = item.IsRoaster == false ? "No" : "Yes"

                                        };
                                        //write data to generic class
                                        OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                                    }
                                }

                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "MACHINEINTERFACE":

                        var OMachineinterfaceData = db.MachineInterface
                            .Include(e => e.DatabaseType)
                            .Include(e => e.InterfaceName)
                            .ToList();

                        if (OMachineinterfaceData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OMachineinterfaceData)
                            {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld2 = ca.DatabaseType != null ? ca.DatabaseType.LookupVal.ToString() : "",
                                    Fld3 = ca.InterfaceName != null ? ca.InterfaceName.LookupVal.ToString() : "",
                                    Fld4 = ca.CardCode == null ? "" : ca.CardCode.ToString(),
                                    Fld5 = ca.DatabaseName == null ? "" : ca.DatabaseName.ToString(),
                                    Fld6 = ca.DateField == null ? "" : ca.DateField.ToString(),
                                    Fld7 = ca.InTimeField == null ? "" : ca.InTimeField.ToString(),
                                    Fld8 = ca.OutTimeField == null ? "" : ca.OutTimeField.ToString(),
                                    Fld9 = ca.UnitNoField == null ? "" : ca.UnitNoField.ToString(),
                                    Fld10 = ca.ServerName == null ? "" : ca.ServerName.ToString(),
                                    Fld11 = ca.TableName == null ? "" : ca.TableName.ToString(),
                                    Fld12 = ca.UserId == null ? "" : ca.UserId.ToString(),
                                    Fld13 = ca.Password == null ? "" : ca.Password.ToString(),
                                };
                                //write data to generic class
                                OGenericAttendanceStatement.Add(OGenericGeoObjStatement);


                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "OVERTIMEPOLICY":

                        var OOvertimepolicyData = db.OTPolicy
                            .ToList();

                        if (OOvertimepolicyData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OOvertimepolicyData)
                            {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld2 = ca.OTPoilicyName != null ? ca.OTPoilicyName.ToString() : "",
                                    Fld3 = ca.BreakTime != null ? ca.BreakTime.Value.ToShortTimeString() : "",
                                    Fld4 = ca.COffOTHours == null ? "" : ca.COffOTHours.Value.ToShortTimeString(),
                                    Fld5 = ca.compulsoryStay == null ? "" : ca.compulsoryStay.Value.ToShortTimeString(),
                                    Fld6 = ca.CompensatoryOff == false ? "No" : "Yes"

                                };
                                //write data to generic class
                                OGenericAttendanceStatement.Add(OGenericGeoObjStatement);


                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "REPORTINGTIMINGSTRUCTURE":
                        //DateTime paymonth = Convert.ToDateTime("01/" + mPayMonth.FirstOrDefault());
                        List<EmployeeAttendance> OEmpReportingTimingStructData = new List<EmployeeAttendance>();
                        //EmpAttendanceIdList = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(t => t.Id).ToList();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OEmpReportingTimingStructData_t = db.EmployeeAttendance
                           .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct.JobStatus)
                                //.Include(e => e.EmpReportingTimingStruct)
                                //.Include(e => e.EmpReportingTimingStruct.Select(x => x.ReportingTimingStruct))
                           .OrderBy(e => e.Id)
                           .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel().FirstOrDefault();

                            if (OEmpReportingTimingStructData_t != null)
                            {
                                OEmpReportingTimingStructData_t.Employee.EmpName = db.NameSingle.Find(OEmpReportingTimingStructData_t.Employee.EmpName_Id);
                                OEmpReportingTimingStructData_t.EmpReportingTimingStruct = db.EmpReportingTimingStruct.Where(e => e.EmployeeAttendance_Id == OEmpReportingTimingStructData_t.Id).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OEmpReportingTimingStructData_t.EmpReportingTimingStruct)
                                {
                                    item1.ReportingTimingStruct = db.EmpReportingTimingStruct.Where(e => e.Id == item1.Id).Select(t => t.ReportingTimingStruct).FirstOrDefault();
                                }
                                OEmpReportingTimingStructData.Add(OEmpReportingTimingStructData_t);
                            }

                        }

                        if (OEmpReportingTimingStructData.Select(e => e.EmpReportingTimingStruct) == null || OEmpReportingTimingStructData.Select(e => e.EmpReportingTimingStruct).Count() == 0)
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
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            //List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            //Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            //Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            //Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            foreach (var l in OEmpReportingTimingStructData)
                            {
                                IEnumerable<EmpReportingTimingStruct> Oempall11 = l.EmpReportingTimingStruct.Where(e => e.EffectiveDate >= pFromDate && e.EffectiveDate <= pToDate).ToList();
                                if (Oempall11.Count() != 0)
                                {
                                    //foreach (var ca9 in Oempall.Where(e => e.EffectiveDate >= pFromDate && e.EffectiveDate <= pToDate))
                                    //{                                  
                                    foreach (var ca9 in Oempall11)
                                    {
                                        //int geoid = l.Employee.GeoStruct.Id;

                                        //int payid = l.Employee.PayStruct.Id;

                                        //int funid = l.Employee.FuncStruct.Id;

                                        //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        int? geoid = l.Employee.GeoStruct_Id;

                                        int? payid = l.Employee.PayStruct_Id;

                                        int? funid = l.Employee.FuncStruct_Id;

                                        GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                        PayStruct paystruct = db.PayStruct.Find(payid);

                                        FuncStruct funstruct = db.FuncStruct.Find(funid);

                                        GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                        using (DataBaseContext db1 = new DataBaseContext())
                                        {
                                            var empall = db1.EmpReportingTimingStruct

                                                     .Include(r => r.ReportingTimingStruct.GeoGraphList)
                                                     .Include(r => r.ReportingTimingStruct.TimingPolicy)
                                                   .Include(r => r.ReportingTimingStruct).Where(e => e.Id == ca9.Id).AsNoTracking().AsParallel().FirstOrDefault();

                                            //foreach (var ca2 in empall.ReportingTimingStruct)
                                            //{

                                            if (empall.ReportingTimingStruct.RSName != null)
                                            {

                                                GenericField100 OGenericObj = new GenericField100()
                                                //write data to generic class
                                                {

                                                    Fld1 = l.Employee.Id.ToString(),
                                                    Fld2 = l.Employee.EmpCode,
                                                    Fld3 = l.Employee.EmpName.FullNameFML,
                                                    Fld5 = ca9.EffectiveDate.Value.ToShortDateString(),
                                                    Fld6 = ca9.EndDate != null ? ca9.EndDate.Value.ToShortDateString() : "",
                                                    //Fld52 = ca2.SalaryHead.Id.ToString(),
                                                    Fld53 = empall.ReportingTimingStruct.IndividualAppl == false ? "No" : "Yes",
                                                    Fld54 = empall.ReportingTimingStruct.IsTimeRoaster == false ? "No" : "Yes",
                                                    //Fld55 = ca2.SalaryHead.Type.LookupVal.ToUpper().ToString(),
                                                    Fld56 = empall.ReportingTimingStruct.GeographicalAppl == false ? "No" : "Yes",
                                                    Fld57 = empall.ReportingTimingStruct.GeoGraphList != null ? empall.ReportingTimingStruct.GeoGraphList.LookupVal.ToString() : "",
                                                    Fld58 = empall.ReportingTimingStruct.TimingPolicy != null ? empall.ReportingTimingStruct.TimingPolicy.TimingCode.ToString() : "",
                                                    Fld16 = GeoDataInd.GeoStruct_Location_Name + (GeoDataInd.GeoStruct_Department_Name == null ? "" : "/") + GeoDataInd.GeoStruct_Department_Name,
                                                    Fld19 = GeoDataInd.FuncStruct_Job_Name
                                                    //Fld19 = GeoDataInd.GeoStruct_Department_Name,
                                                    //Fld34 = GeoDataInd.PayStruct_Grade_Name,

                                                };
                                                if (comp)
                                                {
                                                    OGenericObj.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObj.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObj.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObj.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObj.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObj.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObj.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObj.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObj.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObj.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObj.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObj.Fld88 = l.Employee.EmpName.FullNameFML;
                                                }
                                                OGenericAttendanceStatement.Add(OGenericObj);
                                            }
                                            //  }

                                        }
                                    }
                                }

                                else
                                {


                                    //foreach (var ca9 in item)
                                    //{
                                    //int geoid = l.Employee.GeoStruct.Id;

                                    //int payid = l.Employee.PayStruct.Id;

                                    //int funid = l.Employee.FuncStruct.Id;

                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    int? geoid = l.Employee.GeoStruct_Id;

                                    int? payid = l.Employee.PayStruct_Id;

                                    int? funid = l.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    using (DataBaseContext db1 = new DataBaseContext())
                                    {
                                        IEnumerable<EmpReportingTimingStruct> Oempall12 = l.EmpReportingTimingStruct.OrderByDescending(e => e.EffectiveDate).ToList();
                                        if (Oempall12.Count() > 0)
                                        {

                                            var ca9 = Oempall12.FirstOrDefault();
                                            var empall = db1.EmpReportingTimingStruct

                                                     .Include(r => r.ReportingTimingStruct.GeoGraphList)
                                                     .Include(r => r.ReportingTimingStruct.TimingPolicy)
                                                   .Include(r => r.ReportingTimingStruct).Where(e => e.Id == ca9.Id).AsNoTracking().AsParallel().FirstOrDefault();

                                            //foreach (var ca2 in empall.ReportingTimingStruct)
                                            //{

                                            if (empall.ReportingTimingStruct.RSName != null)
                                            {

                                                GenericField100 OGenericObj = new GenericField100()
                                                //write data to generic class
                                                {

                                                    Fld1 = l.Employee.Id.ToString(),
                                                    Fld2 = l.Employee.EmpCode,
                                                    Fld3 = l.Employee.EmpName.FullNameFML,
                                                    Fld5 = ca9.EffectiveDate.Value.ToShortDateString(),
                                                    Fld6 = ca9.EndDate != null ? ca9.EndDate.Value.ToShortDateString() : "",
                                                    //Fld52 = ca2.SalaryHead.Id.ToString(),
                                                    Fld53 = empall.ReportingTimingStruct.IndividualAppl == false ? "No" : "Yes",
                                                    Fld54 = empall.ReportingTimingStruct.IsTimeRoaster == false ? "No" : "Yes",
                                                    //Fld55 = ca2.SalaryHead.Type.LookupVal.ToUpper().ToString(),
                                                    Fld56 = empall.ReportingTimingStruct.GeographicalAppl == false ? "No" : "Yes",
                                                    Fld57 = empall.ReportingTimingStruct.GeoGraphList != null ? empall.ReportingTimingStruct.GeoGraphList.LookupVal.ToString() : "",
                                                    Fld58 = empall.ReportingTimingStruct.TimingPolicy != null ? empall.ReportingTimingStruct.TimingPolicy.TimingCode.ToString() : "",
                                                    Fld16 = GeoDataInd.GeoStruct_Location_Name + (GeoDataInd.GeoStruct_Department_Name == null ? "" : "/") + GeoDataInd.GeoStruct_Department_Name,
                                                    Fld19 = GeoDataInd.FuncStruct_Job_Name
                                                    //Fld19 = GeoDataInd.GeoStruct_Department_Name,
                                                    //Fld34 = GeoDataInd.PayStruct_Grade_Name,

                                                };
                                                if (comp)
                                                {
                                                    OGenericObj.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObj.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericObj.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericObj.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericObj.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericObj.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericObj.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericObj.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericObj.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericObj.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericObj.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericObj.Fld88 = l.Employee.EmpName.FullNameFML;
                                                }
                                                OGenericAttendanceStatement.Add(OGenericObj);
                                            }
                                        }
                                        //  }

                                    }
                                    //}
                                }

                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "ATTENDANCENOTPUNCH":
                        //var ONotPunchData = new List<EmployeeAttendance>();
                        //foreach (var item in EmpAttendanceIdList)
                        //{

                        //    var ONotPunchData_t = db.EmployeeAttendance
                        //        .Include(e => e.Employee)
                        //        //.Include(e => e.Employee.EmpName)
                        //        //.Include(e => e.Employee.GeoStruct)
                        //        // .Include(e => e.Employee.PayStruct)
                        //        //  .Include(e => e.Employee.FuncStruct)
                        //        //   .Include(e => e.Employee.FuncStruct.Job)
                        //        //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //        // .Include(e => e.Employee.PayStruct.Grade)
                        //        //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        //        //.Include(e => e.RawData)
                        //        .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                        //        .FirstOrDefault();
                        //    if (ONotPunchData_t != null)
                        //    {
                        //        ONotPunchData_t.Employee.EmpName = db.NameSingle.Find(ONotPunchData_t.Employee.EmpName_Id);
                        //        ONotPunchData_t.RawData = db.RawData.Where(e => e.EmployeeAttendance_Id == ONotPunchData_t.Id && (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();

                        //        ONotPunchData.Add(ONotPunchData_t);
                        //    }

                        //}
                        //var idate = DbFunctions.TruncateTime(pFromDate);

                        var ONotPunchData = db.EmployeeAttendance.Select(r => new
                        {
                            iId = r.Id,
                            iEmployeeId = r.Employee.Id,
                            GeoStructId = r.Employee.GeoStruct_Id,
                            PayStructId = r.Employee.PayStruct_Id,
                            FuncStructId = r.Employee.FuncStruct_Id,
                            iEmpCode = r.Employee.EmpCode,
                            iEmpName = r.Employee.EmpName.FullNameFML,
                            iRawData = r.RawData.Select(t => new
                            {
                                iSwipeDate = t.SwipeDate
                            }).Where(t => DbFunctions.TruncateTime(t.iSwipeDate) >= pFromDate && DbFunctions.TruncateTime(t.iSwipeDate) <= pToDate).OrderBy(t => t.iSwipeDate).ToList()
                        }).Where(r => EmpAttendanceIdList.Contains(r.iEmployeeId)).ToList();



                        if (ONotPunchData == null || ONotPunchData.Count() == 0)
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

                            foreach (var ca in ONotPunchData)
                            {

                                //int geoid = ca.Employee.GeoStruct.Id;

                                //int payid = ca.Employee.PayStruct.Id;

                                //int funid = ca.Employee.FuncStruct.Id;

                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                int? geoid = ca.GeoStructId;

                                int? payid = ca.PayStructId;

                                int? funid = ca.FuncStructId;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                       .Where(e => e.Employee.Id == ca.iEmployeeId).OrderBy(e => e.Id).SingleOrDefault();


                                //var OAttRawData_T = ca.RawData.Where(e =>e.SwipeTime >= FromTime && e.SwipeTime <= ToTime).ToString();

                                // var OAttRawData = ca.RawData.Where(e => e.SwipeDate.Value.ToString("MM/yyyy") == PayMnth).OrderBy(e => e.SwipeDate).ToList();
                                for (DateTime mTempDate = pFromDate; mTempDate <= pToDate; mTempDate = mTempDate.AddDays(1))
                                {

                                    var OAttRawData = ca.iRawData.Where(e => e.iSwipeDate.Value.Date == mTempDate.Date).ToList();
                                    var result = OAttRawData.GroupBy(x => x.iSwipeDate.Value.Date).Select(y => y.First()).ToList();

                                    if (result.Count() == 0)
                                    {
                                        var roster = db.EmpTimingMonthlyRoaster.Include(e => e.DayType).Where(e => e.EmployeeAttendance_Id == ca.iId && e.RoasterDate == mTempDate.Date).FirstOrDefault();
                                        if (roster != null && roster.DayType.LookupVal.ToUpper() == "WORKING")
                                        {
                                            Boolean Leavetaken = false;
                                            //Leave Checking
                                            if (oEmployeeLeave != null)
                                            {
                                                var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                                                var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                                                var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                                                var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();

                                                if (listLvs.Where(e => e.FromDate <= mTempDate && e.ToDate >= mTempDate).Count() != 0)
                                                {
                                                    //already exits
                                                    Leavetaken = true;
                                                }
                                            }
                                            if (Leavetaken == false)
                                            {

                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {
                                                    Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld4 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                                    Fld5 = ca.iEmpName == null ? "" : ca.iEmpName,

                                                    // Fld17 = ca.Employee.GeoStruct == null ? "" : ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                    Fld18 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld19 = mTempDate.ToShortDateString(),


                                                };

                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.iEmpName;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }
                                        }
                                    }
                                }

                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "ATTENDANCE":
                        var OAttendanceData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                  .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                //.Include(e => e.ProcessedData.Select(r => r.TimingCode))
                                      .Where(e => e.Employee_Id == item).OrderBy(e => e.Id).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                //OAttendanceData_t.Employee = db.Employee.Find(OAttendanceData_t.Employee_Id);
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id && (e.SwipeDate.Value >= pFromDate && e.SwipeDate.Value <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                }
                                OAttendanceData.Add(OAttendanceData_t);
                            }
                        }
                        if (OAttendanceData == null || OAttendanceData.Count() == 0)
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

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();




                            foreach (var ca in OAttendanceData)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);



                                    var OAttData = ca.ProcessedData; //ca.ProcessedData.Where(e => (e.SwipeDate.Value.Date >= pFromDate && e.SwipeDate.Value.Date <= pToDate)).ToList();

                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        double totalminutes = 0;
                                        double totalminutesTOT = 0;
                                        string finaltotalhours = "";
                                        string finaltotalhoursTOT = "";
                                        List<double> totalemphours1 = new List<double>();
                                        List<double> totalempminutes = new List<double>();
                                        List<double> totalemphours1TOT = new List<double>();
                                        List<double> totalempminutesTOT = new List<double>();
                                        string PunchType = "";

                                        double employeetotalwh = 0, totalemphours = 0, totalhours = 0;
                                        double employeetotalwhTOT = 0, totalemphoursTOT = 0, totalhoursTOT = 0;
                                        foreach (var ca1 in OAttData)
                                        {
                                            //var intim = db.TimingPolicy.Where(e => e.Id == ca1.TimingCode.Id).Select(r => r.InTime).FirstOrDefault();
                                            //var CompTimediff = Convert.ToDateTime(ca1.MOutTime) - Convert.ToDateTime(ca1.MInTime);
                                            var CompTimediff = Convert.ToDateTime(ca1.TimingCode.OutTime) - Convert.ToDateTime(ca1.TimingCode.InTime);
                                            var CompTimediffTOT = Convert.ToDateTime(ca1.TimingCode.OutTime) - Convert.ToDateTime(ca1.TimingCode.InTime);
                                            //if (ca1.MusterRemarks == "HO") 
                                            //{
                                            var EmpTimediff = Convert.ToDateTime(ca1.OutTime) - Convert.ToDateTime(ca1.InTime);
                                            //}
                                            var compare = "";
                                            var compareTOT = "";
                                            compareTOT = CompTimediffTOT.ToString("hh\\:mm");
                                            if (ca1.MusterRemarks.LookupVal.ToString() == "HO")
                                            {
                                                compare = CompTimediff.ToString("hh\\:mm");
                                            }
                                            else if (ca1.MusterRemarks.LookupVal.ToString() == "WO")
                                            {
                                                compare = CompTimediff.ToString("hh\\:mm");
                                            }
                                            else if (ca1.MusterRemarks.LookupVal.ToString() == "LV")
                                            {
                                                compare = CompTimediff.ToString("hh\\:mm");
                                            }
                                            else
                                            {
                                                compare = EmpTimediff.ToString("hh\\:mm");
                                            }
                                            // Company Total Hours
                                            var sdTOT = compareTOT.Split(':');
                                            double minutesTOT = Convert.ToDouble(sdTOT[1]);
                                            int hoursTOT = Convert.ToInt32(sdTOT[0]);
                                            totalminutesTOT += minutesTOT;
                                            if (totalminutesTOT != null)
                                            {
                                                totalhoursTOT += hoursTOT;
                                                totalemphours1TOT.Add(totalhoursTOT);
                                                totalempminutesTOT.Add(totalminutesTOT);
                                            }

                                            // Employee Total Hours
                                            var sd = compare.Split(':');
                                            double minutes = Convert.ToDouble(sd[1]);
                                            int hours = Convert.ToInt32(sd[0]);
                                            // int Coverthourtominute = hours * 60;
                                            totalminutes += minutes;
                                            if (totalminutes != null)
                                            {
                                                //TimeSpan spWorkMin = TimeSpan.FromMinutes(totalminutes);
                                                totalhours += hours;
                                                // totalemphours+=spWorkMin.Hours;
                                                //  employeetotalwh +=Convert.ToDouble(totalhours);
                                                totalemphours1.Add(totalhours);
                                                totalempminutes.Add(totalminutes);
                                            }
                                            if (OAttData.LastOrDefault().Id == ca1.Id)
                                            {
                                                TimeSpan SWorkhours = TimeSpan.FromMinutes(totalempminutes.LastOrDefault());
                                                double hours1 = SWorkhours.Hours + totalemphours1.LastOrDefault();
                                                finaltotalhours = hours1.ToString() + ":" + SWorkhours.Minutes.ToString();

                                                TimeSpan SWorkhoursTOT = TimeSpan.FromMinutes(totalempminutesTOT.LastOrDefault());
                                                double hours1TOT = SWorkhoursTOT.Hours + totalemphours1TOT.LastOrDefault();
                                                finaltotalhoursTOT = hours1TOT.ToString() + ":" + SWorkhoursTOT.Minutes.ToString();
                                            }

                                            string IntimePunchtype = "";
                                            string OuttimePunchtype = "";

                                            List<RawData> ORawdata = db.RawData.Include(e => e.InputType).Where(e => e.EmployeeAttendance_Id == ca.Id && DbFunctions.TruncateTime(e.SwipeDate) == ca1.SwipeDate).ToList();
                                            if (ORawdata.Count() > 0)
                                            {

                                                if (ca1.TimingCode.InTime != null)
                                                {
                                                    IntimePunchtype = ORawdata.FirstOrDefault().InputType.LookupVal.ToUpper();
                                                    if (IntimePunchtype == "MOBILE")
                                                    {
                                                        IntimePunchtype = IntimePunchtype + "-" + ORawdata.FirstOrDefault().Narration;
                                                    }
                                                }

                                                if (ca1.TimingCode.OutTime != null)
                                                {
                                                    OuttimePunchtype = ORawdata.LastOrDefault().InputType.LookupVal.ToUpper();
                                                    if (OuttimePunchtype == "MOBILE")
                                                    {
                                                        OuttimePunchtype = OuttimePunchtype + "-" + ORawdata.LastOrDefault().Narration;
                                                    }
                                                }
                                            }




                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld8 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld7 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                //Fld9 = ca1.MInTime == null ? "" : ca1.MInTime.Value.ToShortTimeString(),
                                                //Fld10 = ca1.MOutTime == null ? "" : ca1.MOutTime.Value.ToShortTimeString(),
                                                Fld9 = ca1.TimingCode.InTime == null ? "" : ca1.TimingCode.InTime.Value.ToShortTimeString(),
                                                Fld10 = ca1.TimingCode.OutTime == null ? "" : ca1.TimingCode.OutTime.Value.ToShortTimeString(),
                                                Fld12 = ca1.MusterRemarks.LookupVal.ToString() == null ? "" : ca1.MusterRemarks.LookupVal.ToString(),
                                                Fld13 = ca1.TotWhLunch == null ? "" : ca1.TotWhLunch.Value.ToShortDateString(),
                                                Fld14 = CompTimediff.ToString("hh\\:mm"),
                                                Fld15 = compare,
                                                Fld16 = finaltotalhours,
                                                Fld20 = finaltotalhoursTOT,
                                                Fld17 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld18 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld19 = IntimePunchtype,
                                                Fld21 = OuttimePunchtype

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }

                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }

                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "MUSTERCORRECTION":
                        var OAttendanceDatauploadMuster = new List<EmployeeAttendance>();
                        //EmpAttendanceIdList = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(t => t.Id).ToList();
                        var mPayMon = mPayMonth.FirstOrDefault();
                        var _PayMonthprv = Convert.ToDateTime("01/" + mPayMon).AddMonths(-1).ToString("MM/yyyy");

                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                // .Include(e => e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                //  .Include(e => e.Employee.EmpOffInfo)
                                //  .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                // .Include(e => e.ProcessedData.Select(r => r.PresentStatus))
                                //.Include(e => e.ProcessedData.Select(r => r.TimingCode))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OAttendanceData_t.Employee.ServiceBookDates_Id);
                                OAttendanceData_t.Employee.EmpOffInfo = db.EmpOff.Find(OAttendanceData_t.Employee.EmpOffInfo_Id);
                                OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup = db.PayProcessGroup.Find(OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == mPayMon.ToString() || t.SwipeDate.Value.ToString("MM/yyyy") == _PayMonthprv.ToString())).ToList();
                                // OR Condition insert if attenedence upload 26 to 25
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.OldMusterRemarks = db.LookupValue.Find(item1.OldMusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                    item1.PresentStatus = db.LookupValue.Find(item1.PresentStatus_Id);
                                }
                                OAttendanceDatauploadMuster.Add(OAttendanceData_t);
                            }
                        }
                        if (OAttendanceDatauploadMuster == null || OAttendanceDatauploadMuster.Count() == 0)
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
                            foreach (var ca in OAttendanceDatauploadMuster)
                            {
                                int Processgrpid;
                                if ((ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.PayProcessGroup != null))
                                {
                                    Processgrpid = ca.Employee.EmpOffInfo.PayProcessGroup.Id;


                                }
                                else
                                {
                                    continue;
                                }

                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                                    .SingleOrDefault().PayrollPeriod.FirstOrDefault();
                                    int startday = query1.StartDate;
                                    int endday = query1.EndDate;
                                    string mPayMonth1 = mPayMonth.FirstOrDefault();
                                    DateTime _PayMonth = Convert.ToDateTime("01/" + mPayMonth1);

                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date;
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
                                    int daym = (Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date).Day;
                                    Currentmonthstart = Convert.ToDateTime("01/" + mPayMonth1);


                                    if (endday > daym)
                                    {
                                        endday = daym;
                                    }
                                    ProDays = daym - endday;

                                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                                    {
                                        FromPeriod = _PayMonth;
                                        EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                                    }
                                    else
                                    {
                                        DateTime prvmonth = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(-1).Date;
                                        startday = endday + 1;
                                        string pmonth = prvmonth.ToString("MM/yyyy");
                                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                                        EndPeriod = Convert.ToDateTime(endday + "/" + mPayMonth1);

                                    }
                                    CurrentmonthEnd = EndPeriod;
                                    Prevmonthstart = FromPeriod;
                                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                                    {
                                        PrevmonthEnd = FromPeriod.AddDays(ProDays);
                                    }
                                    else
                                    {
                                        PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                                    }

                                    double Fday = 0;
                                    double Hday = 0;
                                    double ABday = 0;
                                    double Recday = 0;
                                    double Paybledays = 0;
                                    Boolean LockStatus = false;

                                    var chklockstatus = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                         a.SwipeDate.Value.Date <= CurrentmonthEnd).Select(y => y.IsLocked).FirstOrDefault();

                                    if (chklockstatus != null)
                                    {
                                        LockStatus = chklockstatus;
                                    }

                                    if (ca.Employee.ServiceBookDates.RetirementDate != null)
                                    {

                                        if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                        {
                                            CurrentmonthEnd = ca.Employee.ServiceBookDates.RetirementDate.Value.Date;
                                            ProDays = 0;

                                        }
                                        else if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date >= CurrentmonthEnd.Date && ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= end.Date)
                                        {
                                            ProDays = (ca.Employee.ServiceBookDates.RetirementDate.Value.Date - CurrentmonthEnd).Days;

                                        }
                                    }
                                    var chkk = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Currentmonthstart &&
                                                                                  a.SwipeDate.Value.Date <= CurrentmonthEnd).ToList(); ;
                                    foreach (var item in chkk)
                                    {
                                        if (item.PresentStatus.LookupVal.ToUpper() == "-" && (item.MusterRemarks.LookupVal.ToString() != "UA" && item.MusterRemarks.LookupVal.ToString() != "AA"))
                                        {
                                            if (item.OldMusterRemarks == null)
                                            {
                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    Fld6 = item.SwipeDate == null ? "" : item.SwipeDate.Value.ToShortDateString(),
                                                    Fld8 = item.OutTime == null ? "" : item.OutTime.Value.ToShortTimeString(),
                                                    Fld7 = item.InTime == null ? "" : item.InTime.Value.ToShortTimeString(),
                                                    Fld9 = item.TimingCode.InTime == null ? "" : item.TimingCode.InTime.Value.ToShortTimeString(),
                                                    Fld10 = item.TimingCode.OutTime == null ? "" : item.TimingCode.OutTime.Value.ToShortTimeString(),
                                                    Fld12 = item.MusterRemarks.LookupVal.ToString() == null ? "" : item.MusterRemarks.LookupVal.ToString(),
                                                    Fld13 = item.TotWhLunch == null ? "" : item.TotWhLunch.Value.ToShortDateString(),
                                                    Fld17 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld18 = GeoDataInd.FuncStruct_Job_Name,



                                                };
                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);

                                            }
                                        }
                                    }


                                    if (ProDays != 0)//if 1 to 30 or 31 month then 01 date two time pick above and here
                                    {
                                        var chkkrec = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                                     a.SwipeDate.Value.Date <= PrevmonthEnd).ToList();
                                        foreach (var rec in chkkrec)
                                        {
                                            if (rec.PresentStatus.LookupVal.ToUpper() == "-" && (rec.MusterRemarks.LookupVal.ToString() != "UA" && rec.MusterRemarks.LookupVal.ToString() != "AA"))
                                            {
                                                if (rec.OldMusterRemarks == null)
                                                {
                                                    GenericField100 OGenericAttObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                        Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                        Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                        Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                        Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                        Fld6 = rec.SwipeDate == null ? "" : rec.SwipeDate.Value.ToShortDateString(),
                                                        Fld8 = rec.OutTime == null ? "" : rec.OutTime.Value.ToShortTimeString(),
                                                        Fld7 = rec.InTime == null ? "" : rec.InTime.Value.ToShortTimeString(),
                                                        Fld9 = rec.TimingCode.InTime == null ? "" : rec.TimingCode.InTime.Value.ToShortTimeString(),
                                                        Fld10 = rec.TimingCode.OutTime == null ? "" : rec.TimingCode.OutTime.Value.ToShortTimeString(),
                                                        Fld12 = rec.MusterRemarks.LookupVal.ToString() == null ? "" : rec.MusterRemarks.LookupVal.ToString(),
                                                        Fld13 = rec.TotWhLunch == null ? "" : rec.TotWhLunch.Value.ToShortDateString(),
                                                        Fld17 = GeoDataInd.GeoStruct_Location_Code,
                                                        Fld18 = GeoDataInd.FuncStruct_Job_Name,



                                                    };
                                                    //if (month)
                                                    //{
                                                    //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                    //}
                                                    if (comp)
                                                    {
                                                        OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericAttendanceStatement.Add(OGenericAttObjStatement);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "ATTENDANCEUPLOAD":
                        //EmpAttendanceIdList = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(t => t.Id).ToList();
                        var OAttendanceDataupload = new List<EmployeeAttendance>();
                        mPayMon = mPayMonth.FirstOrDefault();
                        var _PayMonthprvm = Convert.ToDateTime("01/" + mPayMon).AddMonths(-1).ToString("MM/yyyy");
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                //  .Include(e => e.Employee.EmpOffInfo)
                                //  .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                // .Include(e => e.ProcessedData.Select(r => r.PresentStatus))
                                //.Include(e => e.ProcessedData.Select(r => r.TimingCode))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OAttendanceData_t.Employee.ServiceBookDates_Id);
                                OAttendanceData_t.Employee.EmpOffInfo = db.EmpOff.Find(OAttendanceData_t.Employee.EmpOffInfo_Id);
                                OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup = db.PayProcessGroup.Find(OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == mPayMon.ToString() || t.SwipeDate.Value.ToString("MM/yyyy") == _PayMonthprvm.ToString())).ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                    item1.PresentStatus = db.LookupValue.Find(item1.PresentStatus_Id);
                                }
                                OAttendanceDataupload.Add(OAttendanceData_t);
                            }
                        }
                        if (OAttendanceDataupload == null || OAttendanceDataupload.Count() == 0)
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



                            var RemarkConfig = db.RemarkConfig.Include(a => a.AlterMusterRemark).Include(a => a.MusterRemarks).Include(a => a.PresentStatus)
                                   .AsNoTracking().ToList();
                            string remstatus = "";
                            foreach (var restatus in RemarkConfig)
                            {
                                if (restatus.PresentStatus.LookupVal == "-")
                                {
                                    if (remstatus == "")
                                    {
                                        remstatus = restatus.MusterRemarks.LookupVal.ToUpper();
                                    }
                                    else
                                    {
                                        remstatus = remstatus + "," + restatus.MusterRemarks.LookupVal.ToUpper();
                                    }
                                }
                            }
                            foreach (var ca in OAttendanceDataupload)
                            {
                                int Processgrpid;
                                if ((ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.PayProcessGroup != null))
                                {
                                    Processgrpid = ca.Employee.EmpOffInfo.PayProcessGroup.Id;


                                }
                                else
                                {
                                    continue;
                                }

                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                    var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                                    .SingleOrDefault().PayrollPeriod.FirstOrDefault();
                                    int startday = query1.StartDate;
                                    int endday = query1.EndDate;
                                    string mPayMonth1 = mPayMonth.FirstOrDefault();
                                    DateTime _PayMonth = Convert.ToDateTime("01/" + mPayMonth1);

                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date;
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
                                    int daym = (Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date).Day;
                                    Currentmonthstart = Convert.ToDateTime("01/" + mPayMonth1);


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
                                        DateTime prvmonth = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(-1).Date;
                                        startday = endday + 1;
                                        string pmonth = prvmonth.ToString("MM/yyyy");
                                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                                        EndPeriod = Convert.ToDateTime(endday + "/" + mPayMonth1);

                                    }
                                    CurrentmonthEnd = EndPeriod;
                                    Prevmonthstart = FromPeriod;
                                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                                    {
                                        PrevmonthEnd = FromPeriod.AddDays(ProDays);
                                    }
                                    else
                                    {
                                        PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                                    }
                                    double Fday = 0;
                                    double Hday = 0;
                                    double AAday = 0;
                                    double UAday = 0;
                                    double Recday = 0;
                                    double Paybledays = 0;
                                    double Musterclear = 0;
                                    double HOWO = 0;
                                    Boolean LockStatus = false;

                                    var chklockstatus = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                         a.SwipeDate.Value.Date <= CurrentmonthEnd).Select(y => y.IsLocked).FirstOrDefault();

                                    if (chklockstatus != null)
                                    {
                                        LockStatus = chklockstatus;
                                    }
                                    if (ca.Employee.ServiceBookDates.RetirementDate != null)
                                    {

                                        if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                        {
                                            CurrentmonthEnd = ca.Employee.ServiceBookDates.RetirementDate.Value.Date;
                                            RetProDays = 0;

                                        }
                                        else if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date >= CurrentmonthEnd.Date && ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= end.Date)
                                        {
                                            RetProDays = (ca.Employee.ServiceBookDates.RetirementDate.Value.Date - CurrentmonthEnd).Days;

                                        }
                                    }
                                    var chkk = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Currentmonthstart &&
                                                                      a.SwipeDate.Value.Date <= CurrentmonthEnd).ToList();
                                    foreach (var item in chkk)
                                    {
                                        if (item.MusterRemarks.LookupVal.ToUpper() == "HO" || item.MusterRemarks.LookupVal.ToUpper() == "WO")
                                        {
                                            HOWO = HOWO + 1;
                                        }

                                        if (item.PresentStatus.LookupVal.ToUpper() == "F")
                                        {
                                            Fday = Fday + 1;
                                        }
                                        else if (item.PresentStatus.LookupVal.ToUpper() == "H")
                                        {
                                            Hday = Hday + 0.5;
                                            AAday = AAday + 0.5;
                                        }
                                        else if (item.PresentStatus.LookupVal.ToUpper() == "-" && (item.MusterRemarks.LookupVal.ToUpper() != "UA" && item.MusterRemarks.LookupVal.ToUpper() != "AA"))
                                        {
                                            Musterclear = Musterclear + 1;
                                        }
                                        else if (item.PresentStatus.LookupVal.ToUpper() == "-" && item.MusterRemarks.LookupVal.ToUpper() == "AA")
                                        {
                                            AAday = AAday + 1;
                                        }
                                        else
                                        {
                                            UAday = UAday + 1;
                                        }
                                    }


                                    var chkkrec = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                                  a.SwipeDate.Value.Date <= PrevmonthEnd).ToList();
                                    if (ProDays != 0)//if 1 to 30 or 31 month then 01 date two time pick above and here
                                    {
                                        foreach (var rec in chkkrec)
                                        {
                                            if (rec.MusterRemarks.LookupVal.ToUpper() == "HO" || rec.MusterRemarks.LookupVal.ToUpper() == "WO")
                                            {
                                                HOWO = HOWO + 1;
                                            }
                                            if (rec.PresentStatus.LookupVal.ToUpper() == "F")
                                            {

                                            }
                                            else if (rec.PresentStatus.LookupVal.ToUpper() == "H")
                                            {
                                                // Recday = Recday + 0.5;
                                                AAday = AAday + 0.5;
                                            }
                                            else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && (rec.MusterRemarks.LookupVal.ToUpper() != "UA" && rec.MusterRemarks.LookupVal.ToUpper() != "AA"))
                                            {
                                                Musterclear = Musterclear + 1;
                                            }
                                            else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && rec.MusterRemarks.LookupVal.ToUpper() == "AA")
                                            {
                                                AAday = AAday + 1;
                                            }
                                            else
                                            {
                                                // Recday = Recday + 1;
                                                UAday = UAday + 1;
                                            }
                                        }
                                    }
                                    if (Prevmonthstart.Date == PrevmonthEnd.Date)
                                    {
                                        Recday = 0;
                                    }


                                    if (chkk.Count() != 0 || chkkrec.Count() != 0)
                                    {
                                        var uploadpolicy = db.AttendancePayrollPolicy.Where(e => e.PayProcessGroup.Id == Processgrpid).SingleOrDefault();
                                        if (uploadpolicy != null)
                                        {
                                            //if (uploadpolicy.LWPAdjustCurSal == true)
                                            //{
                                            //   // Paybledays = Fday + Hday + ProDays - Recday;
                                            //    Paybledays = Fday + Hday + ProDays ;
                                            //    if (Paybledays < 0)
                                            //    {
                                            //        Paybledays = 0;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    Paybledays = Fday + Hday + ProDays;
                                            //}
                                            if (RetProDays == ProDays)
                                            {
                                                if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                                {
                                                    Paybledays = CurrentmonthEnd.Day - (AAday + UAday + Musterclear);
                                                }
                                                else
                                                {
                                                    Paybledays = daym - (AAday + UAday + Musterclear);
                                                }

                                            }
                                            else
                                            {
                                                Paybledays = daym - (ProDays - RetProDays) - (AAday + UAday + Musterclear);
                                            }
                                            var paymonconcept = db.PayProcessGroup.Include(e => e.PayMonthConcept).Where(e => e.Id == Processgrpid)
                                   .SingleOrDefault();
                                            if (paymonconcept != null)
                                            {
                                                if (paymonconcept.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                                {
                                                    if (Paybledays > 30)
                                                    {
                                                        Paybledays = 30;
                                                    }

                                                }
                                                else if (paymonconcept.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                                {
                                                    if (Paybledays > 30)
                                                    {
                                                        Paybledays = 30;
                                                    }

                                                }
                                            }

                                            if (Paybledays < 0)
                                            {
                                                Paybledays = 0;
                                            }
                                            if ((HOWO + AAday + UAday) >= daym)
                                            {
                                                Paybledays = 0;
                                            }

                                        }

                                        var paymonconceptchk = db.PayProcessGroup.Include(e => e.PayMonthConcept).Where(e => e.Id == Processgrpid)
                                .SingleOrDefault();

                                        GenericField100 OGenericAttObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                            Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                            Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                            Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                            Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            //Fld6 = Fday.ToString(),
                                            //Fld8 = ProDays.ToString(),
                                            Fld7 = AAday.ToString(),
                                            Fld9 = UAday.ToString(),
                                            Fld10 = Musterclear.ToString(),
                                            Fld12 = LockStatus.ToString() == "False" ? "No" : "Yes",
                                            Fld14 = daym.ToString(),
                                            Fld15 = Paybledays.ToString(),
                                            Fld17 = GeoDataInd.GeoStruct_Location_Code,
                                            Fld18 = GeoDataInd.FuncStruct_Job_Name,
                                            Fld19 = Prevmonthstart.Date.ToShortDateString(),
                                            Fld20 = CurrentmonthEnd.Date.ToShortDateString(),
                                            Fld21 = remstatus.ToString(),
                                            Fld22 = paymonconceptchk.PayMonthConcept.LookupVal.ToString().ToUpper(),
                                        };
                                        //if (month)
                                        //{
                                        //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                        //}
                                        if (comp)
                                        {
                                            OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                        }
                                        if (dept)
                                        {
                                            OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                        }
                                        if (grp)
                                        {
                                            OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                        }
                                        if (emp)
                                        {
                                            OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                        }

                                        OGenericAttendanceStatement.Add(OGenericAttObjStatement);


                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "ATTENDANCEUPLOADLWP":
                        //EmpAttendanceIdList = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(t => t.Id).ToList();
                        var OAttendanceDatauploadlwp = new List<EmployeeAttendance>();
                        mPayMon = mPayMonth.FirstOrDefault();
                        var _PayMonthprvmlwp = Convert.ToDateTime("01/" + mPayMon).AddMonths(-1).ToString("MM/yyyy");
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                // .Include(e => e.Employee.ServiceBookDates)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                //  .Include(e => e.Employee.EmpOffInfo)
                                //  .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                // .Include(e => e.ProcessedData.Select(r => r.PresentStatus))
                                //.Include(e => e.ProcessedData.Select(r => r.TimingCode))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OAttendanceData_t.Employee.ServiceBookDates_Id);
                                OAttendanceData_t.Employee.EmpOffInfo = db.EmpOff.Find(OAttendanceData_t.Employee.EmpOffInfo_Id);
                                OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup = db.PayProcessGroup.Find(OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == mPayMon.ToString() || t.SwipeDate.Value.ToString("MM/yyyy") == _PayMonthprvmlwp.ToString())).ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.OldMusterRemarks = db.LookupValue.Find(item1.OldMusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                    item1.PresentStatus = db.LookupValue.Find(item1.PresentStatus_Id);
                                }
                                OAttendanceDatauploadlwp.Add(OAttendanceData_t);
                            }
                        }
                        if (OAttendanceDatauploadlwp == null || OAttendanceDatauploadlwp.Count() == 0)
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


                            var RemarkConfig = db.RemarkConfig.Include(a => a.AlterMusterRemark).Include(a => a.MusterRemarks).Include(a => a.PresentStatus)
                                  .ToList();
                            string remstatus = "";
                            foreach (var restatus in RemarkConfig)
                            {
                                if (restatus.PresentStatus.LookupVal == "-")
                                {
                                    if (remstatus == "")
                                    {
                                        remstatus = restatus.MusterRemarks.LookupVal.ToUpper();
                                    }
                                    else
                                    {
                                        remstatus = remstatus + "," + restatus.MusterRemarks.LookupVal.ToUpper();
                                    }
                                }
                            }
                            foreach (var ca in OAttendanceDatauploadlwp)
                            {
                                int Processgrpid;
                                if ((ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.PayProcessGroup != null))
                                {
                                    Processgrpid = ca.Employee.EmpOffInfo.PayProcessGroup.Id;


                                }
                                else
                                {
                                    continue;
                                }

                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                                    .SingleOrDefault().PayrollPeriod.FirstOrDefault();
                                    int startday = query1.StartDate;
                                    int endday = query1.EndDate;
                                    string mPayMonth1 = mPayMonth.FirstOrDefault();
                                    DateTime _PayMonth = Convert.ToDateTime("01/" + mPayMonth1);

                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date;
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
                                    int daym = (Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date).Day;
                                    Currentmonthstart = Convert.ToDateTime("01/" + mPayMonth1);


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
                                        DateTime prvmonth = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(-1).Date;
                                        startday = endday + 1;
                                        string pmonth = prvmonth.ToString("MM/yyyy");
                                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                                        EndPeriod = Convert.ToDateTime(endday + "/" + mPayMonth1);

                                    }
                                    CurrentmonthEnd = EndPeriod;
                                    Prevmonthstart = FromPeriod;
                                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                                    {
                                        PrevmonthEnd = FromPeriod.AddDays(ProDays);
                                    }
                                    else
                                    {
                                        PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                                    }

                                    double Fday = 0;
                                    double Hday = 0;
                                    double AAday = 0;
                                    double UAday = 0;
                                    double Recday = 0;
                                    double Paybledays = 0;
                                    double Musterclear = 0;
                                    double HOWO = 0;
                                    Boolean LockStatus = false;

                                    var chklockstatus = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                         a.SwipeDate.Value.Date <= CurrentmonthEnd).Select(y => y.IsLocked).FirstOrDefault();

                                    if (chklockstatus != null)
                                    {
                                        LockStatus = chklockstatus;
                                    }

                                    if (ca.Employee.ServiceBookDates.RetirementDate != null)
                                    {

                                        if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                        {
                                            CurrentmonthEnd = ca.Employee.ServiceBookDates.RetirementDate.Value.Date;
                                            RetProDays = 0;

                                        }
                                        else if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date >= CurrentmonthEnd.Date && ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= end.Date)
                                        {
                                            RetProDays = (ca.Employee.ServiceBookDates.RetirementDate.Value.Date - CurrentmonthEnd).Days;

                                        }
                                    }
                                    var chkk = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Currentmonthstart &&
                                                                      a.SwipeDate.Value.Date <= CurrentmonthEnd).ToList();
                                    foreach (var item in chkk)
                                    {
                                        if (item.MusterRemarks.LookupVal.ToUpper() == "HO" || item.MusterRemarks.LookupVal.ToUpper() == "WO")
                                        {
                                            HOWO = HOWO + 1;
                                        }
                                        if (item.PresentStatus.LookupVal.ToUpper() == "F")
                                        {
                                            Fday = Fday + 1;
                                        }
                                        else if (item.PresentStatus.LookupVal.ToUpper() == "H")
                                        {
                                            Hday = Hday + 0.5;
                                            AAday = AAday + 0.5;
                                        }
                                        else if (item.PresentStatus.LookupVal.ToUpper() == "-" && (item.MusterRemarks.LookupVal.ToUpper() != "UA" && item.MusterRemarks.LookupVal.ToUpper() != "AA"))
                                        {
                                            Musterclear = Musterclear + 1;
                                        }
                                        else if (item.PresentStatus.LookupVal.ToUpper() == "-" && item.MusterRemarks.LookupVal.ToUpper() == "AA")
                                        {
                                            AAday = AAday + 1;
                                        }
                                        else
                                        {
                                            UAday = UAday + 1;
                                        }
                                    }


                                    var chkkrec = ca.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                                  a.SwipeDate.Value.Date <= PrevmonthEnd).ToList();
                                    if (ProDays != 0)//if 1 to 30 or 31 month then 01 date two time pick above and here
                                    {
                                        foreach (var rec in chkkrec)
                                        {
                                            if (rec.MusterRemarks.LookupVal.ToUpper() == "HO" || rec.MusterRemarks.LookupVal.ToUpper() == "WO")
                                            {
                                                HOWO = HOWO + 1;
                                            }
                                            if (rec.PresentStatus.LookupVal.ToUpper() == "F")
                                            {

                                            }
                                            else if (rec.PresentStatus.LookupVal.ToUpper() == "H")
                                            {
                                                // Recday = Recday + 0.5;
                                                AAday = AAday + 0.5;
                                            }
                                            else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && (rec.MusterRemarks.LookupVal.ToUpper() != "UA" && rec.MusterRemarks.LookupVal.ToUpper() != "AA"))
                                            {
                                                Musterclear = Musterclear + 1;
                                            }
                                            else if (rec.PresentStatus.LookupVal.ToUpper() == "-" && rec.MusterRemarks.LookupVal.ToUpper() == "AA")
                                            {
                                                AAday = AAday + 1;
                                            }
                                            else
                                            {
                                                UAday = UAday + 1;
                                            }
                                        }
                                    }
                                    if (Prevmonthstart.Date == PrevmonthEnd.Date)
                                    {
                                        Recday = 0;
                                    }


                                    if (chkk.Count() != 0 || chkkrec.Count() != 0)
                                    {
                                        var uploadpolicy = db.AttendancePayrollPolicy.Where(e => e.PayProcessGroup.Id == Processgrpid).SingleOrDefault();
                                        if (uploadpolicy != null)
                                        {
                                            //if (uploadpolicy.LWPAdjustCurSal == true)
                                            //{
                                            //   // Paybledays = Fday + Hday + ProDays - Recday;
                                            //    Paybledays = Fday + Hday + ProDays ;
                                            //    if (Paybledays < 0)
                                            //    {
                                            //        Paybledays = 0;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    Paybledays = Fday + Hday + ProDays;
                                            //}
                                            if (RetProDays == ProDays)
                                            {
                                                if (ca.Employee.ServiceBookDates.RetirementDate.Value.Date <= CurrentmonthEnd.Date)
                                                {
                                                    Paybledays = CurrentmonthEnd.Day - (AAday + UAday + Musterclear);
                                                }
                                                else
                                                {
                                                    Paybledays = daym - (AAday + UAday + Musterclear);
                                                }

                                            }
                                            else
                                            {
                                                Paybledays = daym - (ProDays - RetProDays) - (AAday + UAday + Musterclear);
                                            }
                                            var paymonconcept = db.PayProcessGroup.Include(e => e.PayMonthConcept).Where(e => e.Id == Processgrpid)
                                  .SingleOrDefault();
                                            if (paymonconcept != null)
                                            {
                                                if (paymonconcept.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                                {
                                                    if (Paybledays > 30)
                                                    {
                                                        Paybledays = 30;
                                                    }

                                                }
                                                else if (paymonconcept.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                                {
                                                    if (Paybledays > 30)
                                                    {
                                                        Paybledays = 30;
                                                    }

                                                }
                                            }

                                            if (Paybledays < 0)
                                            {
                                                Paybledays = 0;
                                            }
                                            if ((HOWO + AAday + UAday) >= daym)
                                            {
                                                Paybledays = 0;
                                            }
                                        }
                                        // ABday = ABday + Recday;
                                        var paymonconceptchk = db.PayProcessGroup.Include(e => e.PayMonthConcept).Where(e => e.Id == Processgrpid)
                                 .SingleOrDefault();

                                        if (paymonconceptchk.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                        {


                                            if (Paybledays != daym)
                                            {


                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                    Fld9 = Musterclear.ToString(),
                                                    Fld10 = UAday.ToString(),
                                                    Fld11 = AAday.ToString(),
                                                    Fld12 = LockStatus.ToString() == "False" ? "No" : "Yes",
                                                    Fld14 = daym.ToString(),
                                                    Fld15 = Paybledays.ToString(),
                                                    Fld17 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld18 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld19 = Prevmonthstart.Date.ToShortDateString(),
                                                    Fld20 = CurrentmonthEnd.Date.ToShortDateString(),
                                                    Fld21 = remstatus.ToString(),
                                                    Fld22 = paymonconceptchk.PayMonthConcept.LookupVal.ToString().ToUpper(),

                                                };
                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }
                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);

                                            }
                                        }
                                        else
                                        {
                                            if (Paybledays != 30)
                                            {


                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                    Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,

                                                    Fld9 = Musterclear.ToString(),
                                                    Fld10 = UAday.ToString(),
                                                    Fld11 = AAday.ToString(),
                                                    Fld12 = LockStatus.ToString() == "False" ? "No" : "Yes",
                                                    Fld14 = daym.ToString(),
                                                    Fld15 = Paybledays.ToString(),
                                                    Fld17 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld18 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld19 = Prevmonthstart.Date.ToShortDateString(),
                                                    Fld20 = CurrentmonthEnd.Date.ToShortDateString(),
                                                    Fld21 = remstatus.ToString(),
                                                    Fld22 = paymonconceptchk.PayMonthConcept.LookupVal.ToString().ToUpper(),
                                                };
                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);

                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "MONTHLYMUSTER":


                        var date = mPayMonth.SingleOrDefault();
                        var oEmployeeAttendance = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                // .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.ProcessedData)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == date.ToString())).ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                }
                                oEmployeeAttendance.Add(OAttendanceData_t);
                            }
                        }
                        if (oEmployeeAttendance == null || oEmployeeAttendance.Count() == 0)
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

                            //var lastDate = Convert.ToDateTime(DateTime.DaysInMonth(date.Year, date.Month) + "/" + date.Month + "/" + date.Year);
                            foreach (var ca in oEmployeeAttendance)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {

                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.ProcessedData;
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld3 = ca1.PayStruct == null ? "" : ca1.PayStruct.Grade.Name,
                                                Fld13 = mFromDate.ToString("MM/yyyy"),
                                                Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld12 = ca1.MusterRemarks == null ? "" : ca1.MusterRemarks.LookupVal.ToString(),
                                                Fld14 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                Fld15 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Code + " " + GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Code + " " + GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Code + " " + GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Code + " " + GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Code + " " + GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Code + " " + GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Code + " " + GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Code + " " + GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_EmpStatus.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Code + " " + GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Code + " " + GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                    case "MONTHLYMUSTERTIMEWISE":

                        var datetimewise = mPayMonth.SingleOrDefault();
                        var oEmployeeAttendancetimewise = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                // .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.ProcessedData)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == datetimewise.ToString())).ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                }
                                oEmployeeAttendancetimewise.Add(OAttendanceData_t);
                            }
                        }
                        if (oEmployeeAttendancetimewise == null || oEmployeeAttendancetimewise.Count() == 0)
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

                            //var lastDate = Convert.ToDateTime(DateTime.DaysInMonth(date.Year, date.Month) + "/" + date.Month + "/" + date.Year);
                            foreach (var ca in oEmployeeAttendancetimewise)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {

                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.ProcessedData;
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld3 = ca1.PayStruct == null ? "" : ca1.PayStruct.Grade.Name,
                                                Fld13 = mFromDate.ToString("MM/yyyy"),
                                                Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld12 = ca1.MusterRemarks == null ? "" : ca1.MusterRemarks.LookupVal.ToString(),
                                                Fld14 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                Fld15 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }



                    case "OLDANDNEWREMARKS":

                        // var remardatewise = mPayMonth.SingleOrDefault();
                        mPayMon = mPayMonth.FirstOrDefault();
                        var PayMonthprvm = Convert.ToDateTime("01/" + mPayMon).AddMonths(-1).ToString("MM/yyyy");
                        var OEmployeeAttendanceremarkswise = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                 .Include(e => e.Employee.EmpName)
                                 .Include(e => e.Employee.EmpOffInfo)
                                 .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                                 .Where(e => e.Employee.Id == item)
                                 .FirstOrDefault();

                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.Employee.EmpOffInfo = db.EmpOff.Find(OAttendanceData_t.Employee.EmpOffInfo_Id);
                                OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup = db.PayProcessGroup.Find(OAttendanceData_t.Employee.EmpOffInfo.PayProcessGroup_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == mPayMon.ToString() || t.SwipeDate.Value.ToString("MM/yyyy") == PayMonthprvm.ToString())).ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.OldMusterRemarks = db.LookupValue.Find(item1.OldMusterRemarks_Id);
                                }
                                OEmployeeAttendanceremarkswise.Add(OAttendanceData_t);
                            }
                        }
                        if (OEmployeeAttendanceremarkswise == null || OEmployeeAttendanceremarkswise.Count() == 0)
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


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();


                            foreach (var ca in OEmployeeAttendanceremarkswise)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {

                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == ca.Employee.EmpOffInfo.PayProcessGroup_Id)
                                                                       .SingleOrDefault().PayrollPeriod.FirstOrDefault();
                                    int startday = query1.StartDate;
                                    int endday = query1.EndDate;
                                    string mPayMonth1 = mPayMonth.FirstOrDefault();
                                    DateTime _PayMonth = Convert.ToDateTime("01/" + mPayMonth1);
                                    DateTime end = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date;
                                    DateTime FromPeriod;
                                    DateTime EndPeriod;
                                    DateTime CurrentmonthEnd;
                                    DateTime Prevmonthstart;
                                    int daysInEndMonth = end.Day;
                                    int daysInstartMonth = 1;

                                    int daym = (Convert.ToDateTime("01/" + mPayMonth1).AddMonths(1).AddDays(-1).Date).Day;
                                    if (endday > daym)
                                    {
                                        endday = daym;
                                    }
                                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                                    {
                                        FromPeriod = _PayMonth;
                                        EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                                    }
                                    else
                                    {
                                        DateTime prvmonth = Convert.ToDateTime("01/" + mPayMonth1).AddMonths(-1).Date;
                                        startday = endday + 1;
                                        string pmonth = prvmonth.ToString("MM/yyyy");
                                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);
                                        EndPeriod = Convert.ToDateTime(endday + "/" + mPayMonth1);

                                    }
                                    CurrentmonthEnd = EndPeriod;
                                    Prevmonthstart = FromPeriod;

                                    var OAttData = ca.ProcessedData.Where(t => (t.MusterRemarks != t.OldMusterRemarks) && (t.MInTime != null) && (t.MOutTime != null));

                                    //var OAttDataid = ca.ProcessedData.Where(t => (t.MusterRemarks != t.OldMusterRemarks) && (t.MInTime != null) && (t.MOutTime != null)).ToList().Select(r => r.Id);

                                    //var odreason = db.OutDoorDutyReq.Select(p => new
                                    //{
                                    //    Reason = p.Reason == null ? " " : p.Reason,
                                    //    ProcessedDataId = p.ProcessedData_Id == null ? 0 : Convert.ToInt32(p.ProcessedData_Id),
                                    //}).Where(p => OAttDataid.Contains(p.ProcessedDataId)).ToList();


                                    if (OAttData != null && OAttData.Count() != 0)
                                    {

                                        foreach (var ca1 in OAttData.Where(b => (b.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                         b.SwipeDate.Value.Date <= CurrentmonthEnd)))
                                        {
                                            var reason = db.OutDoorDutyReq.Where(e => e.ProcessedData_Id == ca1.Id).FirstOrDefault().Reason;
                                            //string swipedate = ca1.SwipeDate != null ? ca1.SwipeDate.Value.ToString("dd/MM/yyyy") : "";
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                //Fld3 = ca1.PayStruct == null ? "" : ca1.PayStruct.Grade.Name,
                                                Fld13 = mFromDate.ToString("MM/yyyy"),
                                                Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld6 = ca1.SwipeDate != null ? ca1.SwipeDate.Value.ToString("dd/MM/yyyy") : "",
                                                Fld7 = ca1.OldMusterRemarks == null ? "" : ca1.OldMusterRemarks.LookupVal.ToString(),
                                                Fld12 = ca1.MusterRemarks == null ? "" : ca1.MusterRemarks.LookupVal.ToString(),
                                                Fld14 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                Fld15 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld18 = ca1.MInTime == null ? "" : ca1.MInTime.Value.ToShortTimeString(),
                                                Fld19 = ca1.MOutTime == null ? "" : ca1.MOutTime.Value.ToShortTimeString(),
                                                Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld20 = reason == null ? "" : reason,

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }


                        break;


                    case "MONTHLYMUSTERREMARKWISE":

                        var dateremarkwise = mPayMonth.SingleOrDefault();
                        var oEmployeeAttendanceremarkwise = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                // .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.ProcessedData)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceData_t != null)
                            {
                                OAttendanceData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceData_t.Employee.EmpName_Id);
                                OAttendanceData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceData_t.Id).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                OAttendanceData_t.ProcessedData = OAttendanceData_t.ProcessedData.Where(t => (t.SwipeDate.Value.ToString("MM/yyyy") == dateremarkwise.ToString())).ToList();
                                foreach (var item1 in OAttendanceData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                }
                                oEmployeeAttendanceremarkwise.Add(OAttendanceData_t);
                            }
                        }
                        if (oEmployeeAttendanceremarkwise == null || oEmployeeAttendanceremarkwise.Count() == 0)
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

                            //var lastDate = Convert.ToDateTime(DateTime.DaysInMonth(date.Year, date.Month) + "/" + date.Month + "/" + date.Year);
                            foreach (var ca in oEmployeeAttendanceremarkwise)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {

                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OAttData = ca.ProcessedData;
                                    if (OAttData != null && OAttData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttData)
                                        {

                                            string swipedate = ca1.SwipeDate != null ? ca1.SwipeDate.Value.ToString("dd") + "  " + ca1.SwipeDate.Value.ToString("ddd") : "";
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = GeoDataInd == null ? "" : GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                Fld2 = GeoDataInd == null ? "" : GeoDataInd.GeoStruct_Department_Name == null ? "" : GeoDataInd.GeoStruct_Department_Name,
                                                Fld3 = ca1 == null ? "" : ca1.PayStruct == null ? "" : ca1.PayStruct.Grade == null ? "" : ca1.PayStruct.Grade.Name,
                                                Fld13 = mFromDate == null ? "" : mFromDate.ToString("MM/yyyy"),
                                                Fld4 = ca.Employee == null ? "" : ca.Employee.EmpCode,
                                                Fld5 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                //Fld6 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld6 = swipedate == null ? "" : swipedate,
                                                Fld12 = ca1.MusterRemarks == null ? "" : ca1.MusterRemarks.LookupVal == null ? "" : ca1.MusterRemarks.LookupVal.ToString(),
                                                Fld14 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                Fld15 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                Fld16 = GeoDataInd == null ? "" : GeoDataInd.GeoStruct_Location_Code == null ? "" : GeoDataInd.GeoStruct_Location_Code,
                                                Fld17 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }


                        break;
                    case "REMARKWISESUMMARY":
                        var ORemarkSumReportData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var ORemarkSumReportData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                // .Include(e => e.Employee.FuncStruct.Job)
                                //    .Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                //     .Include(e => e.ProcessedData.Select(r => r.PresentStatus))
                               .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (ORemarkSumReportData_t != null)
                            {
                                ORemarkSumReportData_t.Employee.EmpName = db.NameSingle.Find(ORemarkSumReportData_t.Employee.EmpName_Id);
                                ORemarkSumReportData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == ORemarkSumReportData_t.Id && (e.SwipeDate.Value >= pFromDate && e.SwipeDate.Value <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in ORemarkSumReportData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.PresentStatus = db.LookupValue.Find(item1.PresentStatus_Id);
                                }
                                ORemarkSumReportData.Add(ORemarkSumReportData_t);
                            }
                        }

                        if (ORemarkSumReportData == null || ORemarkSumReportData.Count() == 0)
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

                            foreach (var ca in ORemarkSumReportData)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                    var OAttProcessData = ca.ProcessedData;

                                    if (OAttProcessData != null && OAttProcessData.Count() != 0)
                                    {
                                        int PP = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "PP" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int LV = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "LV" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int HO = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "HO" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int WO = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "WO" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int IL = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "IL" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int OE = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "OE" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int UA = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "UA" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int FH = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "FH" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int LF = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "LF" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int LH = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "LH" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int SH = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "SH" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int M = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "M" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int IM = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "I?" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int OM = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "O?" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int WH = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "WH" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int WE = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "**" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int AA = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "AA" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int WL = (from n in OAttProcessData where n.MusterRemarks.LookupVal.ToUpper() == "*L" group n by 1 into g select g.Count()).FirstOrDefault();

                                        int TotF = (from n in OAttProcessData where n.PresentStatus.LookupVal.ToUpper() == "F" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int TotH = (from n in OAttProcessData where n.PresentStatus.LookupVal.ToUpper() == "H" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int Totdash = (from n in OAttProcessData where n.PresentStatus.LookupVal.ToUpper() == "-" group n by 1 into g select g.Count()).FirstOrDefault();
                                        int TotHAB = 0;

                                        if (TotH != 0)
                                        {
                                            TotHAB = TotH / 2;
                                        }
                                        TotHAB = TotHAB + Totdash;// total absent
                                        int TotCnt = TotF + TotH + Totdash;
                                        int payblecnt = TotCnt - TotHAB;

                                        foreach (var ca1 in OAttProcessData)
                                        {
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                //Fld1 = ca1.Id.ToString(),
                                                Fld2 = ca1.Id.ToString(),
                                                Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld4 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld6 = ca.Employee.EmpCode,
                                                Fld7 = ca.Employee.EmpName.FullNameFML,
                                                Fld8 = PP.ToString(),
                                                Fld9 = LV.ToString(),
                                                Fld10 = HO.ToString(),
                                                Fld11 = WO.ToString(),
                                                Fld12 = IL.ToString(),
                                                Fld13 = OE.ToString(),
                                                Fld14 = UA.ToString(),
                                                Fld15 = FH.ToString(),
                                                Fld16 = LF.ToString(),
                                                Fld17 = LH.ToString(),
                                                Fld18 = SH.ToString(),
                                                Fld19 = M.ToString(),
                                                Fld20 = IM.ToString(),
                                                Fld21 = OM.ToString(),
                                                Fld22 = WH.ToString(),
                                                Fld23 = WE.ToString(),
                                                //Fld24 = ca.Employee.FuncStruct == null ? "" : ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,
                                                Fld24 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld25 = AA.ToString(),
                                                Fld28 = TotCnt.ToString(),
                                                Fld29 = TotHAB.ToString(),
                                                Fld30 = payblecnt.ToString(),
                                                Fld31 = WL.ToString(),


                                            };

                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            break;
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "ATTENDANCEREPORTBUILDER":
                        var OAttendanceReportBuilderData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttendanceReportBuilderData_t = db.EmployeeAttendance
                                 .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.ProcessedData.Select(r => r.MusterRemarks))
                                //.Include(e => e.ProcessedData.Select(r => r.TimingCode))
                                      .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OAttendanceReportBuilderData_t != null)
                            {
                                OAttendanceReportBuilderData_t.Employee.EmpName = db.NameSingle.Find(OAttendanceReportBuilderData_t.Employee.EmpName_Id);
                                OAttendanceReportBuilderData_t.ProcessedData = db.ProcessedData.Where(e => e.EmployeeAttendance_Id == OAttendanceReportBuilderData_t.Id && (e.SwipeDate.Value >= pFromDate && e.SwipeDate.Value <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttendanceReportBuilderData_t.ProcessedData)
                                {
                                    item1.MusterRemarks = db.LookupValue.Find(item1.MusterRemarks_Id);
                                    item1.TimingCode = db.TimingPolicy.Find(item1.TimingCode_Id);
                                }
                                OAttendanceReportBuilderData.Add(OAttendanceReportBuilderData_t);
                            }
                        }
                        if (OAttendanceReportBuilderData == null || OAttendanceReportBuilderData.Count() == 0)
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
                            foreach (var ca in OAttendanceReportBuilderData)
                            {
                                if (ca.ProcessedData != null && ca.ProcessedData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {

                                            var OAttData = ca.ProcessedData.Where(e => (e.MusterRemarks.LookupVal.ToUpper().ToString() == item)).ToList();
                                            if (OAttData != null && OAttData.Count() != 0)
                                            {
                                                foreach (var ca1 in OAttData)
                                                {
                                                    //var intim = db.TimingPolicy.Where(e => e.Id == ca1.TimingCode.Id).Select(r => r.InTime).FirstOrDefault();
                                                    //  var CompTimediff = Convert.ToDateTime(ca1.MOutTime) - Convert.ToDateTime(ca1.MInTime);
                                                    var CompTimediff = Convert.ToDateTime(ca1.TimingCode.OutTime) - Convert.ToDateTime(ca1.TimingCode.InTime);
                                                    //if (ca1.MusterRemarks == "HO") 
                                                    //{
                                                    var EmpTimediff = Convert.ToDateTime(ca1.OutTime) - Convert.ToDateTime(ca1.InTime);
                                                    //}
                                                    var compare = "";
                                                    if (ca1.MusterRemarks.LookupVal.ToString() == "HO")
                                                    {
                                                        compare = CompTimediff.ToString("hh\\:mm");
                                                    }
                                                    else if (ca1.MusterRemarks.LookupVal.ToString() == "WO")
                                                    {
                                                        compare = CompTimediff.ToString("hh\\:mm");
                                                    }
                                                    else if (ca1.MusterRemarks.LookupVal.ToString() == "LV")
                                                    {
                                                        compare = CompTimediff.ToString("hh\\:mm");
                                                    }
                                                    else
                                                    {
                                                        compare = EmpTimediff.ToString("hh\\:mm");
                                                    }

                                                    GenericField100 OGenericAttObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = GeoDataInd.GeoStruct_Location_Name,
                                                        Fld2 = GeoDataInd.GeoStruct_Department_Name,
                                                        Fld3 = GeoDataInd.PayStruct_Grade_Name,
                                                        Fld4 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                        Fld5 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                        Fld6 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                        Fld8 = ca1.OutTime == null ? "" : ca1.OutTime.Value.ToShortTimeString(),
                                                        Fld7 = ca1.InTime == null ? "" : ca1.InTime.Value.ToShortTimeString(),
                                                        //Fld9 = ca1.MInTime == null ? "" : ca1.MInTime.Value.ToShortTimeString(),
                                                        //Fld10 = ca1.MOutTime == null ? "" : ca1.MOutTime.Value.ToShortTimeString(),
                                                        Fld9 = ca1.TimingCode.InTime == null ? "" : ca1.TimingCode.InTime.Value.ToShortTimeString(),
                                                        Fld10 = ca1.TimingCode.OutTime == null ? "" : ca1.TimingCode.OutTime.Value.ToShortTimeString(),
                                                        Fld12 = ca1.MusterRemarks.LookupVal.ToString() == null ? "" : ca1.MusterRemarks.LookupVal.ToString(),
                                                        Fld13 = ca1.TotWhLunch == null ? "" : ca1.TotWhLunch.Value.ToShortDateString(),
                                                        Fld14 = CompTimediff.ToString("hh\\:mm"),
                                                        Fld15 = compare,
                                                        Fld16 = GeoDataInd.GeoStruct_Location_Code,
                                                        Fld17 = GeoDataInd.FuncStruct_Job_Name,

                                                    };

                                                    if (comp)
                                                    {
                                                        OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }

                                                    OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    /////////////process report end

                    ///////////////Master Report Start

                    case "TIMINGGROUP":

                        var OTimingGroupData = db.TimingGroup
                        .Include(e => e.TimingPolicy).AsNoTracking().AsParallel().ToList();

                        if (OTimingGroupData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OTimingGroupData)
                            {
                                foreach (var ca1 in ca.TimingPolicy)
                                {
                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.GroupCode,
                                        Fld3 = ca.GroupName,
                                        Fld4 = ca1.FullDetails,
                                        Fld5 = ca.IsManualRotateShift.ToString(),
                                        Fld6 = ca.IsAutoShift.ToString()

                                    };
                                    //write data to generic class
                                    OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "TIMINGWEEKLYSCHEDULE":

                        var OWeeklyScheduledData = db.TimingWeeklySchedule
                        .Include(e => e.TimingGroup)
                         .Include(e => e.WeekDays)
                         .Include(e => e.WeeklyOffType)
                        .Include(e => e.TimingGroup.TimingPolicy).AsNoTracking().AsParallel().ToList();

                        if (OWeeklyScheduledData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OWeeklyScheduledData)
                            {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Description,
                                    Fld3 = ca.WeekDays.LookupVal.ToString(),
                                    Fld4 = ca.WeeklyOffType.LookupVal.ToString(),
                                    Fld5 = ca.TimingGroup.FullDetails.ToString(),
                                    Fld6 = ca.Is7x24WeeklyOff.ToString(),
                                    Fld7 = ca.IsFixedWeeklyOff.ToString()

                                };
                                //write data to generic class
                                OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "REMARKCONFIG":

                        var ORemarkConfigdData = db.RemarkConfig
                        .Include(e => e.MusterRemarks)
                         .Include(e => e.PresentStatus)
                         .Include(e => e.AlterMusterRemark).AsNoTracking().AsParallel().ToList();

                        if (ORemarkConfigdData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in ORemarkConfigdData)
                            {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.MusterRemarks.LookupVal.ToString(),
                                    Fld3 = ca.IsAppl.ToString(),
                                    Fld4 = ca.AlterMusterRemark.LookupVal.ToString(),
                                    Fld5 = ca.IsODAppl.ToString(),
                                    Fld6 = ca.PresentStatus.LookupVal.ToString(),
                                    Fld7 = ca.RemarkDesc == null ? "" : ca.RemarkDesc.ToString()

                                };
                                //write data to generic class
                                OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "TIMINGPOLICY":

                        var OTimingPolicyData = db.TimingPolicy
                        .Include(e => e.EarlyAction)
                         .Include(e => e.FlexiAction)
                         .Include(e => e.GraceLunchLateAction)
                         .Include(e => e.LateAction)
                         .Include(e => e.LunchEarlyAction)
                         .Include(e => e.TimingType)
                         .Include(e => e.TimingGroup).AsNoTracking().AsParallel().ToList();

                        if (OTimingPolicyData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OTimingPolicyData)
                            {
                                foreach (var ca1 in ca.TimingGroup)
                                {

                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.TimingCode == null ? "" : ca.TimingCode.ToString(),
                                        Fld2 = ca.InTime == null ? "" : ca.InTime.Value.ToShortTimeString(),
                                        Fld3 = ca.InTimeSpan == null ? "" : ca.InTimeSpan.Value.ToShortTimeString(),
                                        Fld4 = ca.InTimeStart == null ? "" : ca.InTimeStart.Value.ToShortTimeString(),
                                        Fld5 = ca.GraceNoAction == null ? "" : ca.GraceNoAction.Value.ToShortTimeString(),
                                        Fld6 = ca.IsLateCountInit == null ? "" : ca.IsLateCountInit.ToString(),
                                        Fld7 = ca.LateCount == null ? "" : ca.LateCount.ToString(),
                                        Fld8 = ca.LateAction == null ? "" : ca.LateAction.LookupVal.ToString(),

                                        Fld9 = ca.OutTime == null ? "" : ca.OutTime.Value.ToShortTimeString(),
                                        Fld10 = ca.OutTimeSpanTime == null ? "" : ca.OutTimeSpanTime.Value.ToShortTimeString(),
                                        Fld11 = ca.GraceEarlyAction == null ? "" : ca.GraceEarlyAction.Value.ToShortTimeString(),
                                        Fld12 = ca.GraceLateAction == null ? "" : ca.GraceLateAction.Value.ToShortTimeString(),
                                        Fld13 = ca.IsEarlyCountInit == null ? "" : ca.IsEarlyCountInit.ToString(),
                                        Fld14 = ca.EarlyCount == null ? "" : ca.EarlyCount.ToString(),
                                        Fld15 = ca.EarlyAction == null ? "" : ca.EarlyAction.LookupVal.ToString(),

                                        Fld16 = ca.LunchStartTime == null ? "" : ca.LunchStartTime.Value.ToShortTimeString(),
                                        Fld17 = ca.LunchEndTime == null ? "" : ca.LunchEndTime.Value.ToShortTimeString(),
                                        Fld18 = ca.GraceLunchEarly == null ? "" : ca.GraceLunchEarly.Value.ToShortTimeString(),
                                        Fld19 = ca.GraceLunchEarlyCount == null ? "" : ca.GraceLunchEarlyCount.ToString(),
                                        Fld20 = ca.LunchEarlyAction == null ? "" : ca.LunchEarlyAction.LookupVal.ToString(),
                                        Fld21 = ca.GraceLunchLateAction == null ? "" : ca.GraceLunchLateAction.LookupVal.ToString(),
                                        Fld22 = ca.GraceLunchLate == null ? "" : ca.GraceLunchLate.Value.ToShortTimeString(),
                                        Fld23 = ca.GraceLunchLateCount == null ? "" : ca.GraceLunchLateCount.ToString(),

                                        Fld24 = ca.MissingEntryMarker == null ? "" : ca.MissingEntryMarker.Value.ToShortTimeString(),
                                        Fld25 = ca.TimingType == null ? "" : ca.TimingType.LookupVal.ToString(),

                                        Fld26 = ca.WorkingHrs == null ? "" : ca.WorkingHrs.Value.ToShortTimeString(),

                                        Fld27 = ca.FlexiDailyTiming == null ? "" : ca.FlexiDailyTiming.ToString(),
                                        Fld28 = ca.FlexiWeeklyTiming == null ? "" : ca.FlexiWeeklyTiming.ToString(),
                                        Fld29 = ca.FlexiDailyHours == null ? "" : ca.FlexiDailyHours.ToString(),
                                        Fld30 = ca.FlexiWeeklyHours == null ? "" : ca.FlexiWeeklyHours.ToString(),
                                        Fld31 = ca.FlexiAction == null ? "" : ca.FlexiAction.LookupVal.ToString(),

                                        Fld32 = ca.IsEmpTiming == null ? "" : ca.IsEmpTiming.ToString(),
                                        Fld33 = ca1.FullDetails == null ? "" : ca1.FullDetails,
                                    };
                                    //write data to generic class
                                    OGenericAttendanceStatement.Add(OGenericGeoObjStatement);
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    ///////////////Master Report End
                    ///////////////transaction Report Start

                    case "MULTIPLESWIPE":
                        var OAttMSwipeReportData = new List<EmployeeAttendance>();
                        var swiptime = "";
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OAttMSwipeReportData_t = db.EmployeeAttendance
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                // .Include(e => e.Employee.PayStruct)
                                //  .Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                // .Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.RawData)
                                //.Include(e => e.RawData.Select(t => t.InputType))
                                .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            if (OAttMSwipeReportData_t != null)
                            {
                                OAttMSwipeReportData_t.Employee.EmpName = db.NameSingle.Find(OAttMSwipeReportData_t.Employee.EmpName_Id);
                                OAttMSwipeReportData_t.RawData = db.RawData.Where(e => e.EmployeeAttendance_Id == OAttMSwipeReportData_t.Id && (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate)).OrderBy(e => e.SwipeDate.Value).AsNoTracking().AsParallel().ToList();
                                foreach (var item1 in OAttMSwipeReportData_t.RawData)
                                {
                                    item1.InputType = db.LookupValue.Find(item1.InputType_Id);
                                }
                                OAttMSwipeReportData.Add(OAttMSwipeReportData_t);
                            }
                        }

                        var SpGroup = SpecialGroupslist.ToList();
                        string InputType = "";
                        foreach (var item1 in SpGroup)
                        {
                            InputType = item1;
                        }
                        if (OAttMSwipeReportData == null || OAttMSwipeReportData.Count() == 0)
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


                            foreach (var ca in OAttMSwipeReportData)
                            {
                                if (ca.RawData != null && ca.RawData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    List<RawData> OAttRawData = new List<RawData>();

                                    if (InputType == "Machine")
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType == null || e.InputType.LookupVal.ToUpper() == "MACHINE")).OrderBy(e => e.SwipeDate.Value).OrderBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                                    }
                                    else if (InputType == "Mobile")
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType != null && e.InputType.LookupVal.ToUpper() == "MOBILE")).OrderBy(e => e.SwipeDate.Value).OrderBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                                    }
                                    else if (InputType == "ESS")
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType != null && e.InputType.LookupVal.ToUpper() == "ESS")).OrderBy(e => e.SwipeDate.Value).OrderBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                                    }
                                    else
                                    {
                                        OAttRawData = ca.RawData.OrderBy(e => e.SwipeDate.Value).OrderBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                                    }


                                    if (OAttRawData != null && OAttRawData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttRawData)
                                        {
                                            //punch location
                                            var Plocation = "";
                                            if (ca1.InputType.LookupVal.ToUpper() == "MACHINE")
                                            {
                                                var punitid = Convert.ToString(ca1.UnitCode);
                                                var PPLoc = db.UnitIdAssignment
                                                 .Include(e => e.GeoStruct)
                                                 .Include(e => e.GeoStruct.Select(y => y.Location))
                                                  .Include(e => e.GeoStruct.Select(y => y.Location.LocationObj))
                                                  .Include(e => e.GeoStruct.Select(y => y.Department))
                                                  .Include(e => e.GeoStruct.Select(y => y.Department.DepartmentObj))
                                                 .Where(e => e.UnitId == punitid).FirstOrDefault();
                                                string punchdeptname = "";
                                                string punchlocname = "";
                                                if (PPLoc != null)
                                                {
                                                    var loc = PPLoc.GeoStruct.FirstOrDefault();
                                                    var PLoc = db.GeoStruct.Select(w => new
                                                    {
                                                        Id = w.Id,
                                                        Location = w.Location,
                                                        LocationObj = w.Location.LocationObj,
                                                        Department = w.Department,
                                                        DepartmentObj = w.Department.DepartmentObj
                                                    }).Where(e => e.Id == loc.Id).FirstOrDefault();

                                                    punchdeptname = PLoc != null && PLoc.Department != null && PLoc.Department.DepartmentObj != null ? PLoc.Department.DepartmentObj.DeptDesc : "";
                                                    punchlocname = PLoc != null && PLoc.Location != null && PLoc.Location.LocationObj != null ? PLoc.Location.LocationObj.LocDesc : "";
                                                    Plocation = punchlocname + " " + punchdeptname;
                                                }
                                            }
                                            // working location
                                            int? workgeoid = 0;
                                            var employeeattendance = db.EmployeeAttendance.Include(r =>r.Employee).Where(e => e.Id == ca1.EmployeeAttendance_Id).FirstOrDefault();
                                            var emppayrollid = db.EmployeePayroll.Where(e => e.Employee_Id == employeeattendance.Employee_Id).SingleOrDefault().Id;

                                            var empsalstruct = db.EmpSalStruct.Where(e => (e.EmployeePayroll_Id == emppayrollid && e.EndDate != null) && (ca1.SwipeDate >= e.EffectiveDate && ca1.SwipeDate <= e.EndDate)).FirstOrDefault();
                                            if (empsalstruct != null)
                                            {
                                                workgeoid = empsalstruct.GeoStruct_Id;
                                            }
                                            var empsalstructcur = db.EmpSalStruct.Where(e => (e.EmployeePayroll_Id == emppayrollid && e.EndDate == null) && ca1.SwipeDate >= e.EffectiveDate).FirstOrDefault();
                                            if (empsalstructcur != null)
                                            {
                                                workgeoid = empsalstructcur.GeoStruct_Id;
                                            }

                                            //GeoStruct geostructactvity = db.GeoStruct.Find(workgeoid);
                                            string deptname = "";
                                            string locname = "";
                                            var actdept = db.GeoStruct.Select(w => new
                                            {
                                                Id = w.Id,
                                                Location = w.Location,
                                                LocationObj = w.Location.LocationObj,
                                                Department = w.Department,
                                                DepartmentObj = w.Department.DepartmentObj
                                            }).Where(e => e.Id == workgeoid).FirstOrDefault();

                                            deptname = actdept != null && actdept.Department != null && actdept.Department.DepartmentObj != null ? actdept.Department.DepartmentObj.DeptDesc : "";
                                            locname = actdept != null && actdept.Location != null && actdept.Location.LocationObj != null ? actdept.Location.LocationObj.LocDesc : "";

                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            {
                                                //Fld2 = ca.Id.ToString(),
                                                Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld4 = GeoDataInd.GeoStruct_Department_Name,
                                                Fld6 = ca.Employee.EmpCode != null ? ca.Employee.EmpCode : "",
                                                Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld10 = ca.Employee.CardCode,
                                                Fld7 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld8 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld9 = ca1.SwipeTime == null ? "" : ca1.SwipeTime.Value.ToShortTimeString(),
                                                Fld11 = ca1.InputType.LookupVal.ToUpper() == "MACHINE" ? Plocation : ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                Fld12 = ca1.InputType == null ? "Machine" : ca1.InputType.LookupVal.ToString(),
                                                Fld13 = locname + " " + deptname,

                                            };

                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }

                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;

                    case "MOBILESWIPE":
                        var OAttMobileSwipeReportData = new List<EmployeeAttendance>();

                        var SpGroupm = SpecialGroupslist.ToList();
                        string InputTypem = "";
                        foreach (var item1 in SpGroupm)
                        {
                            InputTypem = item1;
                        }
                        if (InputTypem == "GeoverifyFalseInADay")
                        {
                            foreach (var item in EmpAttendanceIdList)
                            {
                                int geoVerify = 0;
                                var OAttMSwipeReportData_t = db.EmployeeAttendance
                                    .Include(e => e.Employee)
                                    .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                    .FirstOrDefault();
                                if (OAttMSwipeReportData_t != null)
                                {
                                    OAttMSwipeReportData_t.Employee.EmpName = db.NameSingle.Find(OAttMSwipeReportData_t.Employee.EmpName_Id);
                                    OAttMSwipeReportData_t.RawData = db.RawData.Include(x => x.GeoFencingParameter).Where(e => e.EmployeeAttendance_Id == OAttMSwipeReportData_t.Id && (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate.Date)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                    foreach (var item1 in OAttMSwipeReportData_t.RawData)
                                    {
                                        item1.InputType = db.LookupValue.Find(item1.InputType_Id);
                                    }

                                    foreach (var item1 in OAttMSwipeReportData_t.RawData)
                                    {
                                        if (item1.GeoFencingVerify == false)
                                        {
                                            geoVerify += 1;
                                        }
                                    }
                                    if (OAttMSwipeReportData_t.RawData.Count > 0 && OAttMSwipeReportData_t.RawData.Count == geoVerify)
                                    {
                                        OAttMobileSwipeReportData.Add(OAttMSwipeReportData_t);
                                    }

                                }
                            }
                        }
                        else
                        {

                            foreach (var item in EmpAttendanceIdList)
                            {
                                var OAttMSwipeReportData_t = db.EmployeeAttendance
                                    .Include(e => e.Employee)
                                    //.Include(e => e.Employee.EmpName)
                                    //.Include(e => e.Employee.GeoStruct)
                                    // .Include(e => e.Employee.PayStruct)
                                    //  .Include(e => e.Employee.FuncStruct)
                                    //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                    // .Include(e => e.Employee.PayStruct.Grade)
                                    //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                    //.Include(e => e.RawData)
                                    //.Include(e => e.RawData.Select(t => t.InputType))
                                    .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                    .FirstOrDefault();
                                if (OAttMSwipeReportData_t != null)
                                {
                                    OAttMSwipeReportData_t.Employee.EmpName = db.NameSingle.Find(OAttMSwipeReportData_t.Employee.EmpName_Id);
                                    OAttMSwipeReportData_t.RawData = db.RawData.Include(x => x.GeoFencingParameter).Where(e => e.EmployeeAttendance_Id == OAttMSwipeReportData_t.Id && (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();
                                    foreach (var item1 in OAttMSwipeReportData_t.RawData)
                                    {
                                        item1.InputType = db.LookupValue.Find(item1.InputType_Id);
                                    }
                                    OAttMobileSwipeReportData.Add(OAttMSwipeReportData_t);
                                }
                            }
                        }

                        if (OAttMobileSwipeReportData == null || OAttMobileSwipeReportData.Count() == 0)
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


                            foreach (var ca in OAttMobileSwipeReportData)
                            {
                                if (ca.RawData != null && ca.RawData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    List<RawData> OAttRawData = new List<RawData>();

                                    //if (InputTypem == "Machine")
                                    //{
                                    //    OAttRawData = ca.RawData.Where(e => (e.InputType == null || e.InputType.LookupVal.ToUpper() == "MACHINE")).OrderBy(e => e.SwipeDate).ToList();
                                    //}
                                    if (InputTypem == "GeoverifyTrue")
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType != null && e.InputType.LookupVal.ToUpper() == "MOBILE" && e.GeoFencingVerify == true)).OrderBy(e => e.SwipeDate).ToList();
                                    }
                                    else if (InputTypem == "GeoverifyFalse")
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType != null && e.InputType.LookupVal.ToUpper() == "MOBILE" && e.GeoFencingVerify == false)).OrderBy(e => e.SwipeDate).ToList();
                                    }
                                    else if (InputTypem == "GeoverifyFalseInADay")
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType != null && e.InputType.LookupVal.ToUpper() == "MOBILE" && e.GeoFencingVerify == false)).OrderBy(e => e.SwipeDate).ToList();
                                    }
                                    else
                                    {
                                        OAttRawData = ca.RawData.Where(e => (e.InputType != null && e.InputType.LookupVal.ToUpper() == "MOBILE")).OrderBy(e => e.SwipeDate).ToList();
                                    }


                                    if (OAttRawData != null && OAttRawData.Count() != 0)
                                    {
                                        foreach (var ca1 in OAttRawData)
                                        {
                                            string loccode = "";
                                            string locdesc = "";
                                            var loc = db.Location.Include(e => e.LocationObj).Where(e => e.GeoFencingParameter_Id == ca1.GeoFencingParameter_Id).FirstOrDefault();
                                            if (loc != null)
                                            {
                                                locdesc = loc.LocationObj.LocDesc.ToString();
                                                loccode = loc.LocationObj.LocCode.ToString();
                                            }
                                            else
                                            {
                                                var deptc = db.Department.Include(e => e.DepartmentObj).Where(e => e.GeoFencingParameter_Id == ca1.GeoFencingParameter_Id).FirstOrDefault();

                                                if (deptc != null)
                                                {
                                                    locdesc = deptc.DepartmentObj.DeptDesc.ToString();
                                                    loccode = deptc.DepartmentObj.DeptCode.ToString();
                                                }
                                            }
                                            GenericField100 OGenericAttObjStatement = new GenericField100()
                                            {


                                                //Fld2 = ca.Id.ToString(),
                                                Fld2 = GeoDataInd.FuncStruct_Job_Name,
                                                //Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                Fld4 = GeoDataInd.GeoStruct_Department_Name + (GeoDataInd.GeoStruct_Location_Name != null ? "/" : "") + GeoDataInd.GeoStruct_Location_Name,
                                                Fld6 = ca.Employee.EmpCode != null ? ca.Employee.EmpCode : "",
                                                Fld5 = GeoDataInd.PayStruct_Grade_Name,
                                                Fld10 = ca.Employee.CardCode,
                                                Fld7 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                Fld8 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                Fld9 = ca1.SwipeTime == null ? "" : ca1.SwipeTime.Value.ToShortTimeString(),
                                                Fld11 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                Fld12 = ca1.InputType == null ? "Machine" : ca1.InputType.LookupVal.ToString(),
                                                Fld20 = ca1.LongitudeOriginal == null ? "" : ca1.LongitudeOriginal.ToString(),
                                                Fld21 = ca1.LatitudeOriginal == null ? "" : ca1.LatitudeOriginal.ToString(),
                                                Fld22 = ca1.DistanceMeter == null ? "" : ca1.DistanceMeter.ToString(),
                                                Fld23 = ca1.GeoFencingVerify == null ? "" : ca1.GeoFencingVerify.ToString(),
                                                Fld24 = ca1.GeoFencingParameter.LongitudeOriginal == null ? "" : ca1.GeoFencingParameter.LongitudeOriginal.ToString(),
                                                Fld25 = ca1.GeoFencingParameter.LatitudeOriginal == null ? "" : ca1.GeoFencingParameter.LatitudeOriginal.ToString(),
                                                Fld26 = ca1.GeoFencingParameter.DistanceMeter == null ? "" : ca1.GeoFencingParameter.DistanceMeter.ToString(),
                                                Fld27 = loccode,
                                                Fld28 = locdesc,

                                            };

                                            //if (month)
                                            //{
                                            //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
                                            if (comp)
                                            {
                                                OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                            }
                                            if (dept)
                                            {
                                                OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                            }
                                            if (grp)
                                            {
                                                OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                            }
                                            if (emp)
                                            {
                                                OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }

                                            OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                        }
                                    }
                                }
                            }

                            return OGenericAttendanceStatement;
                        }

                        break;
                    case "OTHERDEPTPUNCH":

                        var IOAttMSwipeReportData_t = db.EmployeeAttendance.Select(p => new
                       {
                           EmployeeId = p.Employee.Id,
                           EmpCode = p.Employee.EmpCode,
                           CardCode = p.Employee.CardCode,
                           EmpName = p.Employee.EmpName.FullNameFML,
                           PayProcessGroupId = p.Employee.EmpOffInfo.PayProcessGroup.Id,
                           GeoStructId = p.Employee.GeoStruct.Id,
                           PayStructId = p.Employee.PayStruct.Id,
                           FuncStructId = p.Employee.FuncStruct.Id,
                           RawData = p.RawData.Select(r => new
                           {
                               Id = r.Id,
                               SwipeDate = r.SwipeDate,
                               SwipeTime = r.SwipeTime,
                               InputType = r.InputType.LookupVal,
                               UnitCode = r.UnitCode,
                               CardCode = r.CardCode,
                           }).ToList()
                       }).Where(e => EmpAttendanceIdList.Contains(e.EmployeeId)).ToList();

                        if (IOAttMSwipeReportData_t == null || IOAttMSwipeReportData_t.Count() == 0)
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
                            var grade = false;
                            var lvl = false;
                            var jobstat = false;
                            var job = false;
                            var jobpos = false;
                            var regn = false;

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


                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            //Utility.GetOrganizationDataClass GeoDataIndActivity = new Utility.GetOrganizationDataClass();

                            foreach (var ca in IOAttMSwipeReportData_t)
                            {
                                if (ca.RawData != null && ca.RawData.Count() != 0)
                                {
                                    int? geoid = ca.GeoStructId;

                                    int? payid = ca.PayStructId;

                                    int? funid = ca.FuncStructId;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                    //List<RawData> OAttRawData = new List<RawData>();
                                    // if current month transfer on 10/mm/yyyy and previous record should not other dept punch show
                                    int? workgeoid = 0;
                                    //List<RawData> OAttRawDataActivity = new List<RawData>();
                                    var emppayrollid = db.EmployeePayroll.Where(e => e.Employee_Id == ca.EmployeeId).SingleOrDefault().Id;

                                    //    var OAttRawDataActivity = ca.RawData.Where(e => (e.InputType == null || e.InputType.ToUpper() == "MACHINE") && (e.SwipeDate>=pFromDate && e.SwipeDate<=pToDate)).OrderBy(e => e.SwipeDate.Value).OrderBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                                    var OAttRawDataActivity = ca.RawData.Where(e => (e.InputType == null || e.InputType.ToUpper() == "MACHINE") && (e.SwipeDate >= pFromDate && e.SwipeDate <= pToDate)).OrderBy(e => e.SwipeDate.Value).Select(e => e.SwipeDate).Distinct().ToList();

                                    foreach (var ca1 in OAttRawDataActivity)
                                    {
                                        //var comparedate = Ractivity.SwipeDate.Value.Date;
                                        var empsalstruct = db.EmpSalStruct.Where(e => (e.EmployeePayroll_Id == emppayrollid && e.EndDate != null) && (ca1 >= e.EffectiveDate && ca1 <= e.EndDate)).FirstOrDefault();
                                        if (empsalstruct != null)
                                        {
                                            workgeoid = empsalstruct.GeoStruct_Id;
                                        }
                                        var empsalstructcur = db.EmpSalStruct.Where(e => (e.EmployeePayroll_Id == emppayrollid && e.EndDate == null) && ca1 >= e.EffectiveDate).FirstOrDefault();
                                        if (empsalstructcur != null)
                                        {
                                            workgeoid = empsalstructcur.GeoStruct_Id;
                                        }

                                        //GeoStruct geostructactvity = db.GeoStruct.Find(workgeoid);
                                        string deptname = "";
                                        string locname = "";
                                        var actdept = db.GeoStruct.Select(w => new
                                        {
                                            Id = w.Id,
                                            Location = w.Location,
                                            LocationObj = w.Location.LocationObj,
                                            Department = w.Department,
                                            DepartmentObj = w.Department.DepartmentObj
                                        }).Where(e => e.Id == workgeoid).FirstOrDefault();

                                        //var actdept = db.GeoStruct.Include(e => e.Location)
                                        //   .Include(e => e.Location.LocationObj)
                                        //   .Include(e => e.Department)
                                        //   .Include(e => e.Department.DepartmentObj)
                                        //    .Where(e => e.Id == workgeoid).FirstOrDefault();
                                        deptname = actdept != null && actdept.Department != null && actdept.Department.DepartmentObj != null ? actdept.Department.DepartmentObj.DeptDesc : "";
                                        locname = actdept != null && actdept.Location != null && actdept.Location.LocationObj != null ? actdept.Location.LocationObj.LocDesc : "";
                                        //GeoDataIndActivity = ReportRDLCObjectClass.GetOrganizationDataInd(geostructactvity, paystruct, funstruct, 0);

                                        var unitloc = db.UnitIdAssignment
                                            .Include(e => e.GeoStruct)
                                            .ToList();

                                        int unitid = 0;
                                        foreach (var LOcunit in unitloc)
                                        {
                                            var unitidobj = LOcunit.GeoStruct.Where(e => e.Id == workgeoid).FirstOrDefault();
                                            if (unitidobj != null)
                                            {
                                                unitid = Convert.ToInt32(LOcunit.UnitId);
                                                break;
                                            }
                                        }
                                        var OAttRawDataall = ca.RawData.Where(e => (e.InputType == null || e.InputType.ToUpper() == "MACHINE") && e.UnitCode != unitid && e.SwipeDate == ca1).OrderBy(e => e.SwipeDate.Value).OrderBy(e => e.SwipeTime.Value.ToShortTimeString()).ToList();
                                        var OAttRawData = OAttRawDataall.Select(e => new
                                        {
                                            CardCode = e.CardCode,
                                            SwipeDate = e.SwipeDate,
                                            SwipeTime = e.SwipeTime,
                                            UnitCode = e.UnitCode,
                                        }).Distinct().ToList();

                                        if (OAttRawData != null && OAttRawData.Count() != 0)
                                        {
                                            foreach (var ca2 in OAttRawData)
                                            {
                                                var Plocation = "";
                                                var punitid = Convert.ToString(ca2.UnitCode);
                                                var PLoc = db.UnitIdAssignment
                                                 .Include(e => e.GeoStruct)
                                                 .Include(e => e.GeoStruct.Select(y => y.Location))
                                                  .Include(e => e.GeoStruct.Select(y => y.Location.LocationObj))
                                                  .Include(e => e.GeoStruct.Select(y => y.Department))
                                                  .Include(e => e.GeoStruct.Select(y => y.Department.DepartmentObj))
                                                 .Where(e => e.UnitId == punitid).FirstOrDefault();

                                                if (PLoc != null)
                                                {
                                                    var loc = PLoc.GeoStruct.FirstOrDefault();
                                                    var Pdept = loc != null && loc.Department != null ? loc.Department.DepartmentObj.DeptDesc : "";
                                                    Plocation = loc != null ? loc.Location.LocationObj.LocDesc : "" + " " + Pdept;

                                                }
                                                string SwipeMonth = "";
                                                string Month = "";
                                                string Swipedate = "";
                                                if (ca2.SwipeDate.Value.Month < 10)
                                                    Month = "0" + ca2.SwipeDate.Value.Month;
                                                else
                                                    Month = ca2.SwipeDate.Value.Month.ToString();
                                                SwipeMonth = Month + "/" + ca2.SwipeDate.Value.Year;
                                                Swipedate = "01" + "/" + SwipeMonth;
                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {
                                                    //Fld2 = ca.Id.ToString(),

                                                    //Fld3 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                    //Fld4 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                    //Fld5 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : "",
                                                    //Fld10 = ca.Employee.CardCode,
                                                    //Fld7 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    // Fld11 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                                    // Fld12 = ca1.InputType == null ? "Machine" : ca1.InputType.LookupVal.ToString()
                                                    Fld2 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,
                                                    Fld3 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                    Fld4 = GeoDataInd.GeoStruct_Department_Name == null ? "" : GeoDataInd.GeoStruct_Department_Name,
                                                    Fld6 = ca.EmpCode != null ? ca.EmpCode : "",
                                                    Fld5 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                    Fld10 = ca.CardCode,
                                                    Fld7 = ca.EmpName == null ? "" : ca.EmpName,
                                                    Fld8 = ca2.SwipeDate == null ? "" : ca2.SwipeDate.Value.ToShortDateString(),
                                                    Fld9 = ca2.SwipeTime == null ? "" : ca2.SwipeTime.Value.ToShortTimeString(),
                                                    //Fld11 = GeoDataIndActivity.GeoStruct_Location_Name + " " + deptname,
                                                    Fld11 = locname + " " + deptname,
                                                    Fld12 = Plocation,
                                                    Fld13 = Swipedate,
                                                    Fld14 = SwipeMonth,
                                                };

                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.EmpName;
                                                }

                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;


                    case "BEFOREACTIONONATTENDANCELV":
                        string PayMonth = mPayMonth.FirstOrDefault();
                        DateTime Pswipedate = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1);

                        List<int> ids = null;

                        ids = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(r => r.Id).ToList();

                        List<string> remark = new List<string>();
                        List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).ToList();
                        foreach (var item in RC)
                        {
                            remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                        }

                        foreach (var i in ids)
                        {
                            var employeeLeave = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == i).FirstOrDefault();


                            var BindEmpList = db.EmployeeAttendance.Select(e => new
                            {
                                Id = e.Id,
                                Employee = e.Employee,
                                EmpName = e.Employee.EmpName,
                                EmpOffInfo = e.Employee.EmpOffInfo,
                                ServiceBookDates = e.Employee.ServiceBookDates,
                                PayProcessGroup = e.Employee.EmpOffInfo.PayProcessGroup,
                                ProcessedData = e.ProcessedData.Select(r => new
                                {
                                    PresentStatus = r.PresentStatus,
                                    MusterRemarks = r.MusterRemarks,
                                    SwipeDate = r.SwipeDate,
                                    Id = r.Id,
                                    ManualReason = r.ManualReason
                                }).Where(r => r.SwipeDate >= Pswipedate).ToList()
                            }).Where(e => e.Employee.Id == i).FirstOrDefault();
                            int Processgrpid = BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id;
                            var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid).SingleOrDefault().PayrollPeriod.FirstOrDefault();
                            int startday = query1.StartDate;
                            int endday = query1.EndDate;
                            DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                            DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
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
                            if (startday == daysInstartMonth && endday == daysInEndMonth)
                            {
                                PrevmonthEnd = FromPeriod.AddDays(ProDays);
                            }
                            else
                            {
                                PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                            }

                            if ((BindEmpList.Employee != null && BindEmpList.Employee.EmpOffInfo != null && BindEmpList.Employee.EmpOffInfo.PayProcessGroup != null
                           && BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                           (BindEmpList.Employee != null && BindEmpList.Employee.ServiceBookDates != null && BindEmpList.Employee.ServiceBookDates.ServiceLastDate != null))
                            {
                                continue;
                            }
                            Int32 GeoStruct = 0;
                            Int32 PayStruct = 0;
                            Int32 FuncStruct = 0;
                            GeoStruct = Convert.ToInt32(BindEmpList.Employee.GeoStruct_Id.ToString());
                            PayStruct = Convert.ToInt32(BindEmpList.Employee.PayStruct_Id.ToString());
                            FuncStruct = Convert.ToInt32(BindEmpList.Employee.FuncStruct_Id.ToString());

                            var chkk = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                               a.SwipeDate.Value.Date <= CurrentmonthEnd && a.MusterRemarks.LookupVal.ToUpper() == "LF" || a.MusterRemarks.LookupVal.ToUpper() == "LH").OrderBy(e => e.SwipeDate).ToList();

                            EmployeeLeave OEmployeeLeave = null;
                            OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == i).SingleOrDefault();
                            EmployeeAttendance OemloyeeAttendance = null;
                            OemloyeeAttendance = db.EmployeeAttendance.Where(e => e.Employee.Id == i).SingleOrDefault();

                            var empActionstruct = db.EmployeeAttendanceActionPolicyStruct
                                .Include(e => e.EmployeeAttendanceActionPolicyStructDetails)
                                .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula))
                                 .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.PolicyName))
                                .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula.AttendanceAbsentPolicy))
                                  .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula.AttendanceLeavePriority))
                                .Where(e => e.EmployeeAttendance_Id == OemloyeeAttendance.Id && e.EndDate == null).FirstOrDefault();

                            if (empActionstruct == null)
                            {
                                continue;

                            }
                            var lvheadassign = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id)).ToList();
                            if (lvheadassign.Count() == 0)
                            {
                                continue;
                            }

                            //foreach (var item in chkk)
                            //{
                            //    ProcessedData processdatanotodfill = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == item.Id && remark.Contains(e.MusterRemarks.LookupVal.ToString())).FirstOrDefault();

                            //if (processdatanotodfill != null)
                            //{


                            List<int> idslv = new List<int>();
                            var lvhead = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id).ToList()).FirstOrDefault();

                            //foreach (var LVH in lvhead)
                            //{

                            //var LvNewReqDebitLast = db.LvNewReq
                            //    .Include(e => e.EmployeeLeave)
                            //    .Include(e => e.EmployeeLeave.Employee)
                            //    .Include(e => e.EmployeeLeave.Employee.EmpName)
                            //    .Where(e => e.EmployeeLeave_Id == employeeLeave.Id && e.LeaveHead_Id == LVH).OrderByDescending(e => e.Id).FirstOrDefault();

                            //if (LvNewReqDebitLast != null && LvNewReqDebitLast.CloseBal > 0)
                            //{

                            double closebal = 0;
                            double newbal = 0;
                            int LvId = 0;
                            LvNewReq LvNewReqDebitLast = null;
                            foreach (var item in chkk)
                            {
                                double dbtday = 0; LookupValue getState = null;
                                if (item.MusterRemarks.LookupVal.ToUpper() == "LH")
                                {
                                    dbtday = 0.5;
                                    getState = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FIRSTSESSION".ToUpper()).FirstOrDefault();
                                }
                                else
                                {
                                    dbtday = 1;
                                    getState = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION".ToUpper()).FirstOrDefault();
                                }

                                newbal = closebal;
                                foreach (var LVH in lvhead.ToList())
                                {
                                    if (newbal != 0 && newbal >= dbtday)
                                    {
                                        closebal = newbal;
                                    }
                                    else
                                    {
                                        if (dbtday > closebal && LvId == LVH.Value)
                                        {
                                            lvhead.Remove(LVH);
                                            newbal = 0;
                                            continue;
                                        }
                                        LvNewReqDebitLast = db.LvNewReq
                                          .Include(e => e.EmployeeLeave)
                                          .Include(e => e.EmployeeLeave.Employee)
                                          .Include(e => e.EmployeeLeave.Employee.EmpName)
                                          .Where(e => e.EmployeeLeave_Id == employeeLeave.Id && e.LeaveHead_Id == LVH).OrderByDescending(e => e.Id).FirstOrDefault();
                                        closebal = LvNewReqDebitLast != null ? LvNewReqDebitLast.CloseBal : 0;
                                        LvId = LVH.Value;
                                        if (dbtday > closebal && LvId == LVH.Value)
                                        {
                                            lvhead.Remove(LVH);
                                            newbal = 0;
                                            continue;
                                        }

                                    }

                                    ProcessedData processdatanotodfill = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == item.Id && remark.Contains(e.MusterRemarks.LookupVal.ToString())).FirstOrDefault();

                                    if (processdatanotodfill != null)
                                    {

                                        if (closebal > 0)
                                        {



                                            if (dbtday <= closebal)
                                            {

                                                closebal = closebal - dbtday;
                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {
                                                    Fld1 = LvNewReqDebitLast.EmployeeLeave.Employee.EmpCode,
                                                    Fld2 = LvNewReqDebitLast.EmployeeLeave.Employee.EmpName.FullNameFML,
                                                    Fld3 = item.SwipeDate.Value.ToShortDateString(),
                                                    Fld4 = item.SwipeDate.Value.ToShortDateString(),
                                                    Fld5 = db.LvHead.Where(e => e.Id == LVH).SingleOrDefault().LvCode,
                                                    Fld6 = dbtday.ToString()
                                                };
                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            //break;
                            //}
                            //else
                            //{
                            //    continue;
                            //}
                            //}
                            //  }
                            //  }
                        }

                        return OGenericAttendanceStatement;

                        break;


                    case "PUNCHTIMEWISE":

                        var PayMnth = mPayMonth.SingleOrDefault();
                        //var FrmTime = DateTime.Parse(FromTime);
                        //var TTime = Convert.ToDateTime(ToTime).ToString("hh\\:mm");


                        var OPunchData = new List<EmployeeAttendance>();
                        foreach (var item in EmpAttendanceIdList)
                        {
                            var OPunchData_t = db.EmployeeAttendance
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                // .Include(e => e.Employee.PayStruct)
                                //  .Include(e => e.Employee.FuncStruct)
                                //   .Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                // .Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.RawData)
                                .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            if (OPunchData_t != null)
                            {
                                OPunchData_t.Employee.EmpName = db.NameSingle.Find(OPunchData_t.Employee.EmpName_Id);
                                OPunchData_t.RawData = db.RawData.Where(e => e.EmployeeAttendance_Id == OPunchData_t.Id && (DbFunctions.TruncateTime(e.SwipeDate.Value) >= pFromDate && DbFunctions.TruncateTime(e.SwipeDate.Value) <= pToDate)).OrderBy(e => e.SwipeDate).AsNoTracking().AsParallel().ToList();

                                OPunchData.Add(OPunchData_t);
                            }

                        }
                        if (OPunchData == null || OPunchData.Count() == 0)
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


                            foreach (var ca in OPunchData)
                            {
                                if (ca.RawData != null && ca.RawData.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    //var OAttRawData_T = ca.RawData.Where(e =>e.SwipeTime >= FromTime && e.SwipeTime <= ToTime).ToString();

                                    // var OAttRawData = ca.RawData.Where(e => e.SwipeDate.Value.ToString("MM/yyyy") == PayMnth).OrderBy(e => e.SwipeDate).ToList();
                                    var OAttRawData = ca.RawData.ToList();


                                    if (OAttRawData != null && OAttRawData.Count() != 0)
                                    {

                                        foreach (var ca1 in OAttRawData)
                                        {
                                            TimeSpan t1 = new TimeSpan(Convert.ToInt32(FromTime.Split(':')[0]), Convert.ToInt32(FromTime.Split(':')[1]), 0);
                                            TimeSpan t2 = new TimeSpan(Convert.ToInt32(ToTime.Split(':')[0]), Convert.ToInt32(ToTime.Split(':')[1]), 0);

                                            TimeSpan t3 = new TimeSpan(Convert.ToInt32(ca1.SwipeTime.Value.ToShortTimeString().Split(':')[0]), Convert.ToInt32(ca1.SwipeTime.Value.ToShortTimeString().Split(':')[1]), 0);



                                            if (t1 <= t3 && t3 <= t2)
                                            {

                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {
                                                    //Fld2 = ca.Id.ToString(),
                                                    Fld3 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld4 = GeoDataInd.GeoStruct_Department_Name,

                                                    Fld6 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                    Fld5 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : "",
                                                    Fld10 = ca.Employee.CardCode,
                                                    Fld7 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                    Fld8 = ca1.SwipeDate == null ? "" : ca1.SwipeDate.Value.ToShortDateString(),
                                                    Fld9 = ca1.SwipeTime == null ? "" : ca1.SwipeTime.Value.ToShortTimeString(),
                                                    Fld11 = GeoDataInd.FuncStruct_Job_Name
                                                };

                                                //if (month)
                                                //{
                                                //    OGenericAttObjStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericAttObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericAttObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericAttObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                }
                                                if (dept)
                                                {
                                                    OGenericAttObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                }
                                                if (grp)
                                                {
                                                    OGenericAttObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericAttObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericAttObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericAttObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericAttObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericAttObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericAttObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                }
                                                if (emp)
                                                {
                                                    OGenericAttObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }


                                                OGenericAttendanceStatement.Add(OGenericAttObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericAttendanceStatement;
                        }

                        break;



                    ///////////////transaction Report end
                    default:
                        return null;
                        break;
                }
                //return null;
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

        public static GenericField100 GetFilterData(List<string> forithead, GenericField100 OGenericAttObjStatement, string paymonth, string employee, Utility.ReportClass GeodataInd, Utility.ReportClass PaydataInd, Utility.ReportClass FuncdataInd)
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
                    OGenericAttObjStatement.Fld100 = paymonth;
                }
                if (comp)
                {
                    OGenericAttObjStatement.Fld99 = GeodataInd.Company_Name;
                }
                if (div)
                {
                    OGenericAttObjStatement.Fld98 = GeodataInd.Division_Name;
                }
                if (loca)
                {
                    OGenericAttObjStatement.Fld97 = GeodataInd.LocDesc;
                }
                if (dept)
                {
                    OGenericAttObjStatement.Fld96 = GeodataInd.DeptDesc;
                }
                if (grp)
                {
                    OGenericAttObjStatement.Fld95 = GeodataInd.Group_Name;
                }
                if (unit)
                {
                    OGenericAttObjStatement.Fld94 = GeodataInd.Unit_Name;
                }
                if (grade)
                {
                    OGenericAttObjStatement.Fld93 = PaydataInd.Grade_Name;
                }
                if (lvl)
                {
                    OGenericAttObjStatement.Fld92 = PaydataInd.Level_Name;
                }
                if (jobstat)
                {
                    OGenericAttObjStatement.Fld91 = GeodataInd.JobStatus_Id.ToString();
                }
                if (job)
                {
                    OGenericAttObjStatement.Fld90 = FuncdataInd.Job_Name;
                }
                if (jobpos)
                {
                    OGenericAttObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                }
                if (emp)
                {
                    OGenericAttObjStatement.Fld88 = employee;
                }

                return OGenericAttObjStatement;
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