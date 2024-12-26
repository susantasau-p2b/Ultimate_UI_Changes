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
using System.Web.UI;
using System.Web;


namespace P2BUltimate.Process
{
    public class AnnualStatementReportGen
    {

        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };

        public static List<GenericField100> GenerateAnnualStatementReport(int CompanyPayrollId, List<int> EmpPayrollIdList, string mObjectName, DateTime mFromDate, DateTime mToDate, List<string> forithead)
        {
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            List<GenericField100> OGenericPayrollStatementForm16 = new List<GenericField100>();
            List<GenericField100> OGenericPayrollStatement12BA = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                switch (mObjectName)
                {

                    case "ITSUMMARY":

                        //case "ITTRANST":
                        var OITSUM = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITSUM_t = db.EmployeePayroll
                                .Include(e => e.Employee)
                                 .Include(e => e.Employee.EmpOffInfo)
                                 .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.ITProjection)
                                .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                                .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                 .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                .Include(e => e.Employee.PayStruct.Grade)
                                .Include(e => e.Employee.FuncStruct.Job)
                                .Include(e => e.Employee.ServiceBookDates)
                                  .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                             .FirstOrDefault();
                            if (OITSUM_t != null)
                            {
                                OITSUM.Add(OITSUM_t);
                            }

                            //.Include(e=>e.)
                        }

                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OITSUM == null || OITSUM.Count() == 0)
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

                            foreach (var ca in OITSUM)
                            {
                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OITSUMData = ca.ITProjection.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();

                                var TotSal = OITSUMData.Where(e => e.PickupId == 35).Count()>0 ? OITSUMData.Where(e => e.PickupId == 35).Sum(e=>e.ProjectedAmount):0;
                                double ExemptSec10 = OITSUMData.Where(e => e.PickupId == 42).Count()>0? OITSUMData.Where(e => e.PickupId == 42).Sum(e=>e.ProjectedAmount):0;
                                double Sec24i = 0;// OITSUMData.Where(e => e.PickupId == 61).Count() > 0 ? OITSUMData.Where(e => e.PickupId == 61).Sum(e => e.ProjectedAmount) : 0;
                                //ar result = Store.FirstOrDefault(x => x.Products.Coupon != null && x.Products.Coupon.Any() && x.Products.Coupon[0] == 100)
                                var Sec24ii = OITSUMData.Where(e => e.PickupId == 62).Count() > 0 ? OITSUMData.Where(e => e.PickupId == 62).Sum(e=>e.ProjectedAmount): 0;
                                var Sec24iii = OITSUMData.Where(e => e.PickupId == 70).Count() > 0 ? OITSUMData.Where(e => e.PickupId == 70).Sum(e=>e.ProjectedAmount) : 0;
                                var Sec24iv = 0;// OITSUMData.Where(e => e.PickupId == 60).Count() > 0 ? OITSUMData.Where(e => e.PickupId == 60).Sum(e => e.ProjectedAmount) : 0;
                                var Sec24v = Sec24i + Sec24ii + Sec24iii + Sec24iv;
                                var Sec80C = OITSUMData.Where(e => e.PickupId == 93).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 93).Sum(e=>e.ProjectedAmount):0;
                                var Sec80CCG = OITSUMData.Where(e => e.PickupId == 102).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 102).Sum(e=>e.ProjectedAmount):0;
                                var Sec80DU = OITSUMData.Where(e => e.PickupId == 112).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 112).Sum(e=>e.ProjectedAmount):0;
                                var TotIncome = OITSUMData.Where(e => e.PickupId == 120).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 120).Sum(e=>e.ProjectedAmount):0;
                                // var TaxWReb = OITSUMData.Where(e => e.PickupId == 100).Count() > 0  ? 0 : OITSUMData.Where(e => e.PickupId == 100).Count() > 0.Sum(e=>e.ProjectedAmount);
                                var Sec87 = OITSUMData.Where(e => e.PickupId == 132).Count() > 0 ?  OITSUMData.Where(e => e.PickupId == 132).Sum(e=>e.ProjectedAmount):0;
                                var TotTax = OITSUMData.Where(e => e.PickupId == 136).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 136).Sum(e=>e.ProjectedAmount):0;
                                var TaxPaid = OITSUMData.Where(e => e.PickupId == 140).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 140).Sum(e=>e.ProjectedAmount):0;
                                var TaxPayable = OITSUMData.Where(e => e.PickupId == 141).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 141).Sum(e=>e.ProjectedAmount):0;
                                var TaxPerMon = OITSUMData.Where(e => e.PickupId == 143).Count() > 0  ?  OITSUMData.Where(e => e.PickupId == 143).Sum(e=>e.ProjectedAmount):0;
                                GenericField100 OGenericObjStatement = new GenericField100()
                                //write data to generic class
                                {

                                    Fld2 = ca.Employee.EmpCode,
                                    Fld3 = ca.Employee.EmpName.FullNameFML,
                                    Fld4 = TotSal != null ? TotSal.ToString() : null,
                                    Fld5 = ExemptSec10 != null ? ExemptSec10.ToString() : null,
                                    Fld6 = Sec24v.ToString(),
                                    Fld7 = Sec80C != null ? Sec80C.ToString() : null,
                                    Fld8 = Sec80CCG != null ? Sec80CCG.ToString() : null,
                                    Fld9 = Sec80DU != null ? Sec80DU.ToString() : null,
                                    Fld10 = TotIncome != null ? TotIncome.ToString() : null,
                                    //  Fld11 = TaxWReb != null ? TaxWReb.ToString() : null,
                                    Fld12 = Sec87 != null ? Sec87.ToString() : null,
                                    Fld13 = TotTax != null ? TotTax.ToString() : null,
                                    Fld14 = TaxPaid != null ? TaxPaid.ToString() : null,
                                    Fld15 = TaxPayable != null ? TaxPayable.ToString() : null,
                                    Fld16 = TaxPerMon != null ? TaxPerMon.ToString() : null,
                                    Fld17 = ca.Employee.EmpOffInfo == null ? "" : ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo.ToString()
                                };
                                //if (month)
                                //{
                                //    OGenericObjStatement.Fld100 = ca.PayMonth.ToString();
                                //}
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
                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                }

                                OGenericPayrollStatement.Add(OGenericObjStatement);

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

                    case "ANNUALSALARY":
                        var mstart1 = mFromDate;
                        var mend1 = mToDate;
                        List<EmployeePayroll> OAnnualSalaryData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            EmployeePayroll OAnnualSalaryData_t = db.EmployeePayroll
                                //.Include(e => e.AnnualSalary)
                                //.Include(e => e.AnnualSalary.Select(t => t.AnnualSalaryDetailsR))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.AnnualSalary.Select(t => t.FinancialYear))
                                 //.Include(e => e.AnnualSalary.Select(t => t.GeoStruct))
                                 // .Include(e => e.AnnualSalary.Select(t => t.PayStruct))
                                 //  .Include(e => e.AnnualSalary.Select(t => t.FuncStruct))
                                    .Where(e => e.Employee.Id == item).AsParallel()
                                 .FirstOrDefault();
                            if (OAnnualSalaryData_t != null)
                            {
                                OAnnualSalaryData_t.Employee.EmpName = db.NameSingle.Find(OAnnualSalaryData_t.Employee.EmpName_Id);
                                OAnnualSalaryData_t.AnnualSalary = db.AnnualSalaryR.Where(e => e.EmployeePayroll_Id == OAnnualSalaryData_t.Id && e.FinancialYear.FromDate.Value <= mstart1 && e.FinancialYear.ToDate.Value >= mend1).AsNoTracking().ToList();
                                foreach( var i in OAnnualSalaryData_t.AnnualSalary)
                                {
                                    i.AnnualSalaryDetailsR = db.AnnualSalaryR.Where(e => e.Id == i.Id).Select(r => r.AnnualSalaryDetailsR).AsNoTracking().FirstOrDefault();
                                    i.FinancialYear = db.Calendar.Find(i.FinancialYear_Id);
                                    i.GeoStruct = db.GeoStruct.Find(i.GeoStruct_Id);
                                    i.PayStruct = db.PayStruct.Find(i.PayStruct_Id);
                                    i.FuncStruct = db.FuncStruct.Find(i.FuncStruct_Id);
                                }
                                OAnnualSalaryData.Add(OAnnualSalaryData_t);
                            }
                        }

                        if (OAnnualSalaryData == null || OAnnualSalaryData.Count() == 0)
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

                            foreach (var ca in OAnnualSalaryData)
                            {
                                if (ca.AnnualSalary != null && ca.AnnualSalary.Count() != 0)
                                {
                                    //var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                    //var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                    var OAnnualSalData = ca.AnnualSalary.ToList();
                                    // e.FinancialYear.FromDate == mstart && e.FinancialYear.ToDate.Value >= mend).ToList();

                                    // foreach (var ca1 in ca.AnnualSalary)//.Where(e=> mPayMonth.Contains(e.FinancialYear))
                                    foreach (var ca1 in OAnnualSalData)
                                    {


                                        if (ca1.AnnualSalaryDetailsR != null && ca1.AnnualSalaryDetailsR.Count() != 0)
                                        {
                                            var OAnnualSalaryDetails = ca1.AnnualSalaryDetailsR; //.Where(e=> mPayMonth.Contains(e.) )
                                            //var m = mPayMonth.Count();
                                            //int geoid = ca1.GeoStruct.Id;

                                            //int payid = ca1.PayStruct.Id;

                                            //int funid = ca1.FuncStruct.Id;

                                            //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                            //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                            //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                            int? geoid = ca1.GeoStruct_Id;

                                            int? payid = ca1.PayStruct_Id;

                                            int? funid =ca1.FuncStruct_Id;

                                            GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                            PayStruct paystruct = db.PayStruct.Find(payid);

                                            FuncStruct funstruct = db.FuncStruct.Find(funid);

                                            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                            if (GeoDataInd != null)
                                            {
                                                foreach (var ca2 in OAnnualSalaryDetails)
                                                {

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca1.EmpCode,
                                                        Fld2 = ca1.EmpName,
                                                        Fld3 = ca1.FinancialYear.FromDate.Value.ToShortDateString(),
                                                        Fld4 = ca1.FinancialYear.ToDate.Value.ToShortDateString(),

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


                    case "ITPROJECTION":

                        var OITPROJECT = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITPROJECT_t = db.EmployeePayroll
                                .Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.ITProjection)
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.FuncStruct)
                                .Include(e => e.Employee.PayStruct)
                                .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                                .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                 .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                .Include(e => e.Employee.PayStruct.Grade)
                                .Include(e => e.Employee.FuncStruct.Job)
                                .Include(e => e.Employee.ServiceBookDates).AsNoTracking()
                                 .Where(e => e.Employee.Id == item && e.ITProjection.Count != 0).AsParallel()
                                 .FirstOrDefault();
                            if (OITPROJECT_t != null)
                            {
                                OITPROJECT.Add(OITPROJECT_t);
                            }
                        }
                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OITPROJECT == null || OITPROJECT.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            foreach (var ca in OITPROJECT)
                            {
                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OITPROJECTData = ca.ITProjection.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();

                                foreach (var ca1 in OITPROJECTData)
                                {
                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
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
                                            //Fld16 = ca1.QualifiedAmount == null ? "" : ca1.QualifiedAmount.ToString(),
                                            //Fld17 = ca1.TDSComponents == null ? "" : ca1.TDSComponents.ToString(),
                                            //Fld18 = ca1.ProjectionDate == null ? "" : ca1.ProjectionDate.ToString(),
                                            //Fld19 = ca1.IsLocked == null ? "" : ca1.IsLocked.ToString(),
                                            //Fld20 = ca1.Narration == null ? "" : ca1.Narration.ToString(),
                                            //Fld21 = ca1.PickupId == null ? "" : ca1.PickupId.ToString(),
                                            //Fld22 = ca1.FinancialYear == null ? "" : ca1.FinancialYear.ToString(),



                                            Fld26 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                            Fld27 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                            Fld28 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                            //Fld29 = ca.Employee.GeoStruct.Location.LocationObj.Id == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.Id.ToString(),
                                            //Fld30 = ca.Employee.GeoStruct.Location.LocationObj.LocCode == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                                            //Fld31 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),

                                            //Fld35 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                            //Fld36 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                                            //Fld37 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                                            //Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.ToString(),
                                            //Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.ToString(),
                                            //Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.ToString(),


                                        };

                                        //if (month)
                                        //{
                                        //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                        //}
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

                            return OGenericPayrollStatement;
                        }


                        break;



                    case "BONUSCHKT":
                        var OBonusChkData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OBonusChkData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                               .Include(e => e.Employee.EmpName)
                               .Include(e => e.BonusChkT)

                                  .Include(e => e.BonusChkT.Select(r => r.BonusCalendar))
                                .Include(e => e.Employee.GeoStruct)
                               .Include(e => e.Employee.FuncStruct)
                               .Include(e => e.Employee.PayStruct)

                                //.Include(e => e.Employee.ServiceBookDates)
                                 .Where(e => e.Employee.Id == item && e.BonusChkT.Count != 0)
                                 .FirstOrDefault();
                            if (OBonusChkData_t != null)
                            {
                                OBonusChkData.Add(OBonusChkData_t);
                            }
                        }


                        if (OBonusChkData == null || OBonusChkData.Count() == 0)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);


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
                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                        {

                                            foreach (var ca1 in OBonusSalData)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {



                                                    Fld1 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = FuncdataInd.Job_Name != null ? FuncdataInd.Job_Name : null,


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


                                                    Fld5 = ca1.BonusWages_11.ToString(),
                                                    Fld6 = ca1.WorkingDays_11.ToString(),
                                                    Fld7 = ca1.Bonus_11.ToString(),
                                                    Fld8 = ca1.ExGracia_11.ToString(),
                                                    Fld9 = ca1.TotalAmount_11.ToString(),

                                                    Fld10 = ca1.BonusWages_12.ToString(),
                                                    Fld11 = ca1.WorkingDays_12.ToString(),
                                                    Fld12 = ca1.Bonus_12.ToString(),
                                                    Fld13 = ca1.ExGracia_12.ToString(),
                                                    Fld14 = ca1.TotalAmount_12.ToString()
                                                };

                                                //if (month)
                                                //{
                                                //    OGenericObjStatement.Fld100 = ca1.PayMonth.ToString();
                                                //}
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

                    case "FORM16":

                        var OITForm16 = new List<EmployeePayroll>();
                         var OFinancialYear = db.Calendar.Where(e => e.FromDate.Value == mFromDate && e.ToDate.Value == mToDate && e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                        DateTime FromPeriodyear = Convert.ToDateTime(OFinancialYear.FromDate);
                        DateTime ToPeriodyear = Convert.ToDateTime(OFinancialYear.ToDate);
                        List<string> mPeriodYear = new List<string>();
                        DateTime mEndDateYear = Convert.ToDateTime("01/" + Convert.ToDateTime(ToPeriodyear).ToString("MM/yyyy")).AddMonths(1).Date;
                        mEndDateYear = mEndDateYear.AddDays(-1).Date;
                        string mPeriodRangeYear = "";
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
                        foreach (var item in EmpPayrollIdList)
                        {
                            //var OITForm16_t = db.EmployeePayroll
                            //    .Include(e => e.Employee)
                            //    .Include(e => e.Employee.EmpName)
                            //    .Include(e => e.ITForm16Data)
                            //    .Include(e => e.ITForm16Data.Select(r => r.FinancialYear))
                            //    .Include(e => e.ITForm16Data.Select(r => r.ITForm16DataDetails))
                            //    .Include(e => e.ITForm16Data.Select(r => r.ITForm16QuarterEmpDetails))
                            //    .Include(e => e.ITChallanEmpDetails)
                            //    .Include(e => e.ITForm16Data.Select(r => r.ITForm16SigningPerson))
                            //    .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                            //     .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                            //    .Include(e => e.Employee.PayStruct.Grade)
                            //    .Include(e => e.Employee.FuncStruct.Job)
                            //    .Include(e => e.Employee.ServiceBookDates)
                            //    .Include(e => e.Employee.EmpOffInfo)
                            //    .Include(e => e.Employee.EmpOffInfo.NationalityID)
                            //    .Include(e => e.Employee.ResAddr)
                            //    .Include(e => e.Employee.ResAddr.Area)
                            //    .Include(e => e.Employee.ResAddr.Country).Include(e => e.Employee.ResAddr.City).Include(e => e.Employee.ResAddr.State)
                            //    .Include(e => e.SalaryT)
                            //    .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                            //   //.Include(e => e.SalaryT.Select(a => a.SalEarnDedT.Select(z => z.SalaryHead)))
                            //   //.Include(e => e.SalaryT.Select(a => a.SalEarnDedT.Select(z => z.SalaryHead.SalHeadOperationType)))
                            //   //.Include(e => e.SalaryT.Select(a => a.SalEarnDedT.Select(z => z.SalaryHead.Type)))
                            //   .Include(e => e.SalaryArrearT)
                            //   .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                            //   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                            //   .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead.Type)))
                            //   .Include(e => e.ITChallanEmpDetails.Select(r => r.Calendar))
                            //    .Include(e => e.PerkTransT).AsNoTracking()
                            //     .Where(e => e.Employee.Id == item && e.ITForm16Data.Count != 0)
                            //     .FirstOrDefault();
                            //if (OITForm16_t != null)
                            //{
                            //    OITForm16.Add(OITForm16_t);
                            //}
                            EmployeePayroll OITForm16_t = db.EmployeePayroll
                              .Include(e => e.Employee)
                              .Where(e => e.Employee.Id == item).AsNoTracking()
                               .FirstOrDefault();

                            if (OITForm16_t != null)
                            {
                                OITForm16_t.Employee = db.Employee.Include(e => e.EmpName)
                                    .Include(e => e.PayStruct.Grade)
                                    .Include(e => e.FuncStruct.Job)
                                    .Include(e => e.GeoStruct.Location.LocationObj)
                                    .Include(e => e.GeoStruct.Department.DepartmentObj)
                                    .Include(e => e.ResAddr)
                                    .Include(e => e.ResAddr.Country)
                                     .Include(e => e.ResAddr.State)
                                      .Include(e => e.ResAddr.City)
                                    .Where(e => e.Id == OITForm16_t.Employee.Id).AsNoTracking().SingleOrDefault();
                                // OITForm16_t.Employee.EmpName = db.NameSingle.Find(OITForm16_t.Employee.EmpName_Id);
                                // OITForm16_t.Employee.GeoStruct = db.GeoStruct.Find(OITForm16_t.Employee.GeoStruct_Id);
                                //OITForm16_t.Employee.GeoStruct.Location = db.Location.Find(OITForm16_t.Employee.GeoStruct.Location_Id);
                                // OITForm16_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Find(OITForm16_t.Employee.GeoStruct.Location.LocationObj_Id);
                                // OITForm16_t.Employee.GeoStruct.Department = db.Department.Find(OITForm16_t.Employee.GeoStruct.Department_Id);
                                //if (OITForm16_t.Employee.GeoStruct.Department != null)
                                //{
                                //    OITForm16_t.Employee.GeoStruct.Department.DepartmentObj = db.DepartmentObj.Find(OITForm16_t.Employee.GeoStruct.Department.DepartmentObj_Id);
                                //}
                                //OITForm16_t.Employee.FuncStruct = db.FuncStruct.Find(OITForm16_t.Employee.FuncStruct_Id);
                                //OITForm16_t.Employee.FuncStruct.Job = db.Job.Find(OITForm16_t.Employee.FuncStruct.Job_Id);
                                //OITForm16_t.Employee.PayStruct = db.PayStruct.Find(OITForm16_t.Employee.PayStruct_Id);
                                //OITForm16_t.Employee.PayStruct.Grade = db.Grade.Find(OITForm16_t.Employee.PayStruct.Grade_Id);
                                OITForm16_t.Employee.EmpOffInfo = db.EmpOff.Find(OITForm16_t.Employee.EmpOffInfo_Id);
                                OITForm16_t.Employee.EmpOffInfo.NationalityID = db.EmpOff.Where(e => e.Id == OITForm16_t.Employee.EmpOffInfo_Id).Select(r => r.NationalityID).FirstOrDefault();
                                OITForm16_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OITForm16_t.Employee.ServiceBookDates_Id);
                                //OITForm16_t.Employee.ResAddr = db.Address.Find(OITForm16_t.Employee.ResAddr_Id);
                                //if (OITForm16_t.Employee.ResAddr != null)
                                //{

                                //    OITForm16_t.Employee.ResAddr.Area = db.Address.Where(e => e.Id == OITForm16_t.Employee.ResAddr_Id).Select(a => a.Area).FirstOrDefault();
                                //    OITForm16_t.Employee.ResAddr.Country = db.Address.Where(e => e.Id == OITForm16_t.Employee.ResAddr_Id).Select(a => a.Country).FirstOrDefault();
                                //    OITForm16_t.Employee.ResAddr.State = db.Address.Where(e => e.Id == OITForm16_t.Employee.ResAddr_Id).Select(a => a.State).FirstOrDefault();
                                //    OITForm16_t.Employee.ResAddr.City = db.Address.Where(e => e.Id == OITForm16_t.Employee.ResAddr_Id).Select(a => a.City).FirstOrDefault();
                                //}
                                OITForm16_t.ITForm16Data = db.ITForm16Data.Where(e => e.EmployeePayroll_Id == OITForm16_t.Id && e.FinancialYear.FromDate.Value >= mFromDate && e.FinancialYear.ToDate.Value <= mToDate).AsNoTracking().ToList();
                                OITForm16_t.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OITForm16_t.Id && mPeriodYear.Contains(e.PayMonth)).AsNoTracking().ToList();

                                OITForm16_t.YearlyPaymentT = db.YearlyPaymentT.Include(e => e.SalaryHead).Include(e => e.SalaryHead.Type).Where(e => e.EmployeePayroll_Id == OITForm16_t.Id && mPeriodYear.Contains(e.PayMonth)).AsNoTracking().ToList();
                                OITForm16_t.PerkTransT = db.PerkTransT.Where(e => e.EmployeePayroll_Id == OITForm16_t.Id && mPeriodYear.Contains(e.PayMonth)).ToList();
                                OITForm16_t.SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll_Id == OITForm16_t.Id && mPeriodYear.Contains(e.PayMonth)).ToList();
                                foreach (var j in OITForm16_t.SalaryArrearT)
                                {
                                    var ArrearTypeobj = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(x => x.ArrearType).FirstOrDefault();

                                    List<SalaryArrearPaymentT> SalaryArrearPaymentT = db.SalaryArrearT.Where(e => e.Id == j.Id).Select(r => r.SalaryArrearPaymentT.Where(x => x.SalHeadAmount != 0).ToList()).FirstOrDefault();
                                    foreach (var i in SalaryArrearPaymentT)
                                    {
                                        var SalaryArrearPaymentTObj = db.SalaryArrearPaymentT.Where(e => e.Id == i.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                        var SalaryHeadarrrear = db.SalaryHead.Include(e => e.Type).Where(e => e.Id == SalaryArrearPaymentTObj.Id).FirstOrDefault();

                                        i.SalaryHead = SalaryHeadarrrear;

                                    }
                                    j.SalaryArrearPaymentT = SalaryArrearPaymentT;
                                    j.ArrearType = ArrearTypeobj;

                                }
                                OITForm16_t.ITChallanEmpDetails = db.ITChallanEmpDetails.Include(x => x.Calendar).Where(e => e.EmployeePayroll_Id == OITForm16_t.Id && e.Calendar.FromDate.Value >= mFromDate && e.Calendar.ToDate.Value <= mToDate).AsNoTracking().ToList();

                                foreach (var i in OITForm16_t.ITForm16Data)
                                {
                                    i.ITForm16DataDetails = db.ITForm16Data.Where(e => e.Id == i.Id).Select(e => e.ITForm16DataDetails).AsNoTracking().FirstOrDefault();
                                    i.ITForm16QuarterEmpDetails = db.ITForm16Data.Where(e => e.Id == i.Id).Select(e => e.ITForm16QuarterEmpDetails).AsNoTracking().FirstOrDefault();
                                    i.FinancialYear = db.Calendar.Find(i.FinancialYear_Id);
                                    i.ITForm16SigningPerson = db.ITForm16Data.Where(e => e.Id == i.Id).Select(e => e.ITForm16SigningPerson).AsNoTracking().FirstOrDefault();
                                }
                                OITForm16.Add(OITForm16_t);
                            }

                        }
                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OITForm16 == null || OITForm16.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var CompDet = db.Company.Include(e => e.Address).Include(e => e.Address.Area)
                                   .Include(e => e.Address.Country).Include(e => e.Address.City).Include(e => e.Address.State).Where(e => e.Id.ToString() == SessionManager.CompanyId).SingleOrDefault();

                            foreach (var ca in OITForm16)
                            {

                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OITForm16Data = ca.ITForm16Data.Where(e => e.FinancialYear.FromDate.Value >= mstart && e.FinancialYear.ToDate.Value <= mend)
                                        .ToList();
                               
                                double TotTax = 0;

                                foreach (var ca1 in OITForm16Data)
                                {
                                    foreach (var ca2 in ca1.ITForm16DataDetails)
                                    {
                                        if (ca1.ITForm16SigningPerson != null)
                                        {
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {

                                                // Fld1 = ca1.Id.ToString(),
                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                //Fld3 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca.Employee.ResAddr == null ? "" : ca.Employee.ResAddr.FullAddress.ToString(),
                                                Fld5 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),


                                                Fld6 = ca1.CompPAN,
                                                Fld7 = ca1.CompTAN,
                                                Fld8 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld12 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + "-" + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld13 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + "-" + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld14 = ca2.HeaderCol1,
                                                Fld15 = ca2.ActualAmountCol2,
                                                Fld16 = ca2.QualifyAmountCol3,
                                                Fld17 = ca2.DeductibleAmountCol4,
                                                Fld18 = ca2.FinalAmountCol5,

                                                Fld26 = "0",
                                                Fld27 = "0",
                                                Fld28 = "0",
                                                Fld29 = "0",
                                                Fld30 = "0",

                                                Fld32 = "0",
                                                Fld33 = "0",
                                                Fld34 = "0",
                                                Fld35 = "0",

                                                Fld36 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld37 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld38 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld39 = ca1.ITForm16SigningPerson.Place,//place 
                                                Fld40 = NumToWords.ConvertAmount(TotTax)
                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);

                                            GenericField100 OGenericObjStatement1 = new GenericField100()
                                            //write data to generic class
                                            {

                                                // Fld1 = ca1.Id.ToString(),
                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                //Fld3 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca.Employee.ResAddr == null ? "" : ca.Employee.ResAddr.FullAddress.ToString(),
                                                Fld5 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),


                                                Fld6 = ca1.CompPAN,
                                                Fld7 = ca1.CompTAN,
                                                Fld8 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld12 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + "-" + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld13 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + "-" + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld14 = ca2.HeaderCol1,
                                                Fld15 = ca2.ActualAmountCol2,
                                                Fld16 = ca2.QualifyAmountCol3,
                                                Fld17 = ca2.DeductibleAmountCol4,
                                                Fld18 = ca2.FinalAmountCol5,

                                                Fld26 = "0",
                                                Fld27 = "0",
                                                Fld28 = "0",
                                                Fld29 = "0",
                                                Fld30 = "0",

                                                Fld32 = "0",
                                                Fld33 = "0",
                                                Fld34 = "0",
                                                Fld35 = "0",

                                                Fld36 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld37 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld38 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld39 = ca1.ITForm16SigningPerson.Place,//place 
                                                Fld40 = NumToWords.ConvertAmount(TotTax)
                                            };
                                            OGenericPayrollStatementForm16.Add(OGenericObjStatement1);
                                        }
                                    }

                                    if (ca1.ITForm16QuarterEmpDetails != null)
                                    {
                                        int i = 1;
                                        double GrossSal = 0;
                                        var itfor16quarter = ca1.ITForm16QuarterEmpDetails.OrderBy(x=>x.QuarterFromDate).ToList();
                                        foreach (var ca3 in itfor16quarter)
                                        {
                                            TotTax = 0;

                                            if (ca.SalaryT != null && ca.SalaryT.Count > 0)
                                            {
                                                //double TotEarn = ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).Sum(e => e.TotalEarning);
                                                //double TotDed = ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).Sum(e => e.TotalDeduction);
                                                // GrossSal = TotEarn + TotDed;
                                            
                                                double TotEarn = 0;
                                                foreach (var oSalaryt in ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value &&
                                                  Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).ToList())
                                                {

                                                    //var earnded = db.SalaryT.Include(e => e.SalEarnDedT).Where(e => e.Id == oSalaryt.Id)
                                                    //    .Include(e => e.SalEarnDedT.Select(t =>  t.SalaryHead))
                                                    //    .Include(e => e.SalEarnDedT.Select(t => t.SalaryHead.SalHeadOperationType))
                                                    //    .Include(e => e.SalEarnDedT.Select(t => t.SalaryHead.Type)).AsNoTracking()
                                                    //    .FirstOrDefault();
                                                    var earnded = new SalaryT();
                                                    earnded = db.SalaryT.Where(e => e.Id == oSalaryt.Id).FirstOrDefault();

                                                    List<SalEarnDedT> SalEarnDedT = db.SalaryT.Where(e => e.Id == oSalaryt.Id).Select(r => r.SalEarnDedT.Where(e => e.Amount != 0).ToList()).FirstOrDefault();
                                                    foreach (var m in SalEarnDedT)
                                                    {
                                                        var SalEarnDedTObj = db.SalEarnDedT.Where(e => e.Id == m.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                                        var SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).FirstOrDefault();
                                                        var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                                                        var SType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).Select(r => r.Type).FirstOrDefault();
                                                        m.SalaryHead = SalaryHead;
                                                        m.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                                        m.SalaryHead.Type = SType;
                                                    }
                                                    earnded.SalEarnDedT = SalEarnDedT;



                                                    var aa1 = earnded.SalEarnDedT
                                                        .Where(sal => sal.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && sal.SalaryHead.InITax.ToString() == "True"
                                                        && sal.SalaryHead.SalHeadOperationType.LookupVal.ToString() != "PERK")
                                                        .Select(a => a.Amount).Sum();

                                                    if (aa1 != null)
                                                    {
                                                        TotEarn = TotEarn + Convert.ToDouble(aa1);

                                                    }

                                                }


                                                GrossSal = TotEarn;
                                            }

                                            if (ca.SalaryArrearT!=null && ca.SalaryArrearT.Count>0)
                                            {
                                                double TotEarnArr = 0;
                                                foreach (var oSalarytArr in ca.SalaryArrearT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value &&
                                                  Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).ToList())
                                                {


                                                    var aa1Arr = oSalarytArr.SalaryArrearPaymentT
                                                        .Where(sal => sal.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && sal.SalaryHead.InITax.ToString() == "True")
                                                        .Select(a => a.SalHeadAmount).Sum();

                                                    if (aa1Arr != null)
                                                    {
                                                        TotEarnArr = TotEarnArr + Convert.ToDouble(aa1Arr);

                                                    }

                                                }
                                                GrossSal = GrossSal + TotEarnArr;
                                            }
                                           
                                            if (ca.YearlyPaymentT != null && ca.YearlyPaymentT.Count > 0)
                                            {
                                                //var yearpay = ca.YearlyPaymentT.Where(e => e.SalaryHead.InITax == true && e.ReleaseFlag == true);
                                                //double Totyear = yearpay.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).Sum(e => e.AmountPaid);
                                                double Totyear = ca.YearlyPaymentT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value && e.SalaryHead.InITax == true && e.ReleaseFlag == true && e.SalaryHead.InPayslip == false && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.AmountPaid);
                                                GrossSal = GrossSal + Totyear;
                                            }
                                            if (ca.PerkTransT != null && ca.PerkTransT.Count > 0)
                                            {
                                                double TotPerk = ca.PerkTransT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).Sum(e => e.ActualAmount);
                                                GrossSal = GrossSal + TotPerk;
                                            }

                                            foreach (var oSalaryt in ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value &&
                                                    Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).ToList())
                                            {

                                                //var earnded = db.SalaryT.Include(e => e.SalEarnDedT).Where(e => e.Id == oSalaryt.Id)
                                                //     .Include(e => e.SalEarnDedT.Select(t => t.SalaryHead))
                                                //     .Include(e => e.SalEarnDedT.Select(t => t.SalaryHead.SalHeadOperationType))
                                                //     .Include(e => e.SalEarnDedT.Select(t => t.SalaryHead.Type)).AsNoTracking()
                                                //     .FirstOrDefault();
                                                var earnded = new SalaryT();
                                                earnded = db.SalaryT.Where(e => e.Id == oSalaryt.Id).FirstOrDefault();

                                                List<SalEarnDedT> SalEarnDedT = db.SalaryT.Where(e => e.Id == oSalaryt.Id).Select(r => r.SalEarnDedT.Where(e => e.Amount != 0).ToList()).FirstOrDefault();
                                                foreach (var m in SalEarnDedT)
                                                {
                                                    var SalEarnDedTObj = db.SalEarnDedT.Where(e => e.Id == m.Id).Select(r => r.SalaryHead).FirstOrDefault();
                                                    var SalaryHead = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).FirstOrDefault();
                                                    var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).Select(r => r.SalHeadOperationType).FirstOrDefault();
                                                    var SType = db.SalaryHead.Where(e => e.Id == SalEarnDedTObj.Id).Select(r => r.Type).FirstOrDefault();
                                                    m.SalaryHead = SalaryHead;
                                                    m.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                                    m.SalaryHead.Type = SType;
                                                }
                                                earnded.SalEarnDedT = SalEarnDedT;



                                                var aa = earnded.SalEarnDedT
                                                    .Where(sal => sal.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")
                                                    .Select(a => a.Amount).Sum();

                                                if (aa != null)
                                                {
                                                    TotTax = TotTax + Convert.ToDouble(aa);

                                                }

                                            }

                                            if (ca.YearlyPaymentT != null && ca.YearlyPaymentT.Count > 0)
                                            {

                                                TotTax = TotTax + ca.YearlyPaymentT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value && e.SalaryHead.InITax == true && e.ReleaseFlag == true).Sum(e => e.TDSAmount);


                                            }

                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                //Fld3 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca.Employee.ResAddr == null ? "" : ca.Employee.ResAddr.FullAddress.ToString(),
                                                Fld5 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),


                                                Fld6 = ca1.CompPAN,
                                                Fld7 = ca1.CompTAN,
                                                Fld8 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld12 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + "-" + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld13 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + "-" + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld14 = "0",
                                                Fld15 = "0",
                                                Fld16 = "0",
                                                Fld17 = "0",
                                                Fld18 = "0",

                                                Fld26 = "Quarter" + i,
                                                Fld27 = ca3.QuarterAckNo.ToString(),
                                                Fld28 = GrossSal.ToString(),
                                                Fld29 = TotTax.ToString(),
                                                Fld30 = TotTax.ToString(),
                                                //Fld29 = ca3.EmpTaxDeducted.ToString(),
                                                //Fld30 = ca3.TaxableIncome.ToString(),

                                                Fld32 = "0",
                                                Fld33 = "0",
                                                Fld34 = "0",
                                                Fld35 = "0",

                                                Fld36 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld37 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld38 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld39 = ca1.ITForm16SigningPerson.Place,//place 
                                                Fld40 = NumToWords.ConvertAmount(TotTax)
                                            };
                                           
                                            OGenericPayrollStatement.Add(OGenericObjStatement);

                                            GenericField100 OGenericObjStatement1 = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                //Fld3 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca.Employee.ResAddr == null ? "" : ca.Employee.ResAddr.FullAddress.ToString(),
                                                Fld5 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),


                                                Fld6 = ca1.CompPAN,
                                                Fld7 = ca1.CompTAN,
                                                Fld8 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld12 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + "-" + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld13 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + "-" + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld14 = "0",
                                                Fld15 = "0",
                                                Fld16 = "0",
                                                Fld17 = "0",
                                                Fld18 = "0",

                                                Fld26 = "Quarter" + i,
                                                Fld27 = ca3.QuarterAckNo.ToString(),
                                                Fld28 = GrossSal.ToString(),
                                                Fld29 = TotTax.ToString(),
                                                Fld30 = TotTax.ToString(),
                                                //Fld29 = ca3.EmpTaxDeducted.ToString(),
                                                //Fld30 = ca3.TaxableIncome.ToString(),

                                                Fld32 = "0",
                                                Fld33 = "0",
                                                Fld34 = "0",
                                                Fld35 = "0",

                                                Fld36 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld37 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld38 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld39 = ca1.ITForm16SigningPerson.Place,//place 
                                                Fld40 = NumToWords.ConvertAmount(TotTax)
                                            };
                                            i++;
                                            OGenericPayrollStatementForm16.Add(OGenericObjStatement1);
                                        }
                                    }

                                    if (ca.ITChallanEmpDetails != null)
                                    {
                                        int i = 1;
                                        TotTax = 0;

                                        IList<ITChallanEmpDetails> OITChallanDet = ca.ITChallanEmpDetails.Where(e => e.Calendar.FromDate.Value <= mstart && e.Calendar.ToDate.Value >= mend).OrderBy(e => e.TaxDepositDate).ToList();
                                        foreach (var ca3 in OITChallanDet)
                                        {
                                            TotTax = TotTax + ca3.TaxAmount;
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            {

                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                //Fld3 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca.Employee.ResAddr == null ? "" : ca.Employee.ResAddr.FullAddress.ToString(),
                                                Fld5 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),


                                                Fld6 = ca1.CompPAN,
                                                Fld7 = ca1.CompTAN,
                                                Fld8 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld12 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + "-" + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld13 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + "-" + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld14 = "0",
                                                Fld15 = "0",
                                                Fld16 = "0",
                                                Fld17 = "0",
                                                Fld18 = "0",

                                                Fld26 = "0",
                                                Fld27 = "0",
                                                Fld28 = "0",
                                                Fld29 = "0",
                                                Fld30 = "0",

                                                Fld31 = i.ToString(),
                                                Fld32 = ca3.TaxAmount.ToString(),
                                                Fld33 = ca3.BankBSRCode.ToString(),
                                                Fld34 = ca3.TaxDepositDate.Value.ToString("dd/MM/yyyy"),
                                                Fld35 = ca3.ChallanNo.ToString(),

                                                Fld36 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld37 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld38 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld39 = ca1.ITForm16SigningPerson.Place,//place 
                                                Fld40 = NumToWords.ConvertAmount(TotTax)


                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);

                                            GenericField100 OGenericObjStatement1 = new GenericField100()
                                            {

                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                //Fld3 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca.Employee.ResAddr == null ? "" : ca.Employee.ResAddr.FullAddress.ToString(),
                                                Fld5 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),


                                                Fld6 = ca1.CompPAN,
                                                Fld7 = ca1.CompTAN,
                                                Fld8 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld12 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + "-" + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld13 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + "-" + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld14 = "0",
                                                Fld15 = "0",
                                                Fld16 = "0",
                                                Fld17 = "0",
                                                Fld18 = "0",

                                                Fld26 = "0",
                                                Fld27 = "0",
                                                Fld28 = "0",
                                                Fld29 = "0",
                                                Fld30 = "0",

                                                Fld31 = i.ToString(),
                                                Fld32 = ca3.TaxAmount.ToString(),
                                                Fld33 = ca3.BankBSRCode.ToString(),
                                                Fld34 = ca3.TaxDepositDate.Value.ToString("dd/MM/yyyy"),
                                                Fld35 = ca3.ChallanNo.ToString(),

                                                Fld36 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld37 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld38 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld39 = ca1.ITForm16SigningPerson.Place,//place 
                                                Fld40 = NumToWords.ConvertAmount(TotTax)


                                            };
                                            OGenericPayrollStatementForm16.Add(OGenericObjStatement1);

                                            i++;
                                           // TotTax = TotTax + ca3.TaxAmount;
                                        }
                                    }


                                }
                                /////
                                ReportDataSource rdc = new ReportDataSource("P2BDataSet", OGenericPayrollStatement);
                                ReportViewer ReportViewer1 = new ReportViewer();
                                ReportViewer1.LocalReport.DataSources.Clear();
                                ReportViewer1.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/" + "Payroll" + "/Rpt" + "FORM16" + ".rdlc");
                                Utility.ReportClass oReportClass = new Utility.ReportClass();
                                ReportParameter[] param = new ReportParameter[6];
                                param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);
                                param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                                param[4] = new ReportParameter("ReportHeaderName", "Form 16", true);
                                ReportViewer1.LocalReport.EnableExternalImages = true;
                                Uri pathAsUri = new Uri(HttpContext.Current.Server.MapPath("~/Content/Login/Images/PCBL.png"));
                                if (pathAsUri != null)
                                {
                                    param[5] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                                }

                                ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4], param[5] });

                                ReportViewer1.LocalReport.DataSources.Add(rdc);
                                ///
                                string requiredPath1 = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                                                                   System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Form16\\";
                                String localPath1 = "";
                                bool exists2 = System.IO.Directory.Exists(requiredPath1);
                                if (!exists2)
                                {
                                    localPath1 = new Uri(requiredPath1).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath1);
                                }
                                ///
                                string FolderName = "FromPeriod " + mFromDate.ToShortDateString() + " To " + mToDate.ToShortDateString();

                                string FolderName1 = FolderName.Replace("/", "-");

                                //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                                //    System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Form16\\" + FolderName1 + "\\";

                                string requiredPath = Path.Combine(requiredPath1, FolderName1);

                                String localPath = "";
                                bool exists = System.IO.Directory.Exists(requiredPath);
                                if (!exists)
                                {
                                    localPath = new Uri(requiredPath).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath);
                                }

                                string ServerSavePath = localPath;
                                ////for location

                                string LocationName = ca.Employee.GeoStruct.Location.LocationObj.LocDesc;

                                string Locationcombination = Path.Combine(ServerSavePath, LocationName);


                                bool LocationExists = System.IO.Directory.Exists(Locationcombination);

                                if (!LocationExists)
                                {
                                    System.IO.Directory.CreateDirectory(Locationcombination);
                                }


                                Byte[] mybytes = ReportViewer1.LocalReport.Render("PDF");

                                string combination = Path.Combine(Locationcombination, ca.Employee.EmpCode + ".pdf");

                                bool exists1 = System.IO.Directory.Exists(combination);
                               
                                if (!exists1)
                                {
                                    using (FileStream fs = File.Create(combination))
                                    {
                                        fs.Write(mybytes, 0, mybytes.Length);
                                    }
                                }
                                else
                                {
                                    System.IO.Directory.Delete(combination);
                                    using (FileStream fs = File.Create(combination))
                                    {
                                        fs.Write(mybytes, 0, mybytes.Length);
                                    }
                                }
                                OGenericPayrollStatement.Clear();
                            }

                        }

                        return OGenericPayrollStatementForm16;

                        break;

                    case "FORM12BA":

                        var OITForm12BA = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITForm12BA_t = db.EmployeePayroll
                                .Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.ITForm16Data)
                                .Include(e => e.ITForm16Data.Select(r => r.FinancialYear))
                                .Include(e => e.ITForm16Data.Select(r => r.ITForm12BADataDetails))
                                .Include(e => e.ITForm16Data.Select(r => r.ITForm12BADataDetails.Select(t => t.ITForm12BACaptionMapping)))
                                .Include(e => e.ITForm16Data.Select(r => r.ITForm12BADataDetails.Select(t => t.ITForm12BACaptionMapping.PerquisiteName)))
                                .Include(e => e.ITForm16Data.Select(r => r.ITForm16SigningPerson))
                                .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                .Include(e => e.Employee.PayStruct.Grade)
                                .Include(e => e.Employee.FuncStruct.Job)
                                .Include(e => e.Employee.ServiceBookDates)
                                .Include(e => e.Employee.EmpOffInfo)
                                .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                .Include(e => e.ITChallanEmpDetails)
                                .Include(e => e.ITChallanEmpDetails.Select(r => r.Calendar))
                                .Include(e => e.SalaryT)
                                .Include(e => e.ITForm16Data.Select(r => r.ITForm16SigningPerson))
                                .Include(e => e.YearlyPaymentT.Select(r => r.SalaryHead))
                               .Include(e => e.SalaryT.Select(a => a.SalEarnDedT.Select(z => z.SalaryHead)))
                               .Include(e => e.SalaryT.Select(a => a.SalEarnDedT.Select(z => z.SalaryHead.SalHeadOperationType)))
                               .Include(e => e.SalaryT.Select(a => a.SalEarnDedT.Select(z => z.SalaryHead.Type)))
                                .Include(e => e.SalaryArrearT)
                               .Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                               .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                               .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT.Select(t => t.SalaryHead.Type)))
                               .AsNoTracking()
                                 .Where(e => e.Employee.Id == item && e.ITForm16Data.Count != 0)
                                 .FirstOrDefault();
                            if (OITForm12BA_t != null)
                            {
                                OITForm12BA.Add(OITForm12BA_t);
                            }
                        }
                        OGenericPayrollStatement = new List<GenericField100>();
                        OGenericPayrollStatement12BA = new List<GenericField100>();
                        if (OITForm12BA == null || OITForm12BA.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OITForm12BA)
                            {

                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OITForm12BAData = ca.ITForm16Data.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend)
                                        .ToList();
                                var CompDet = db.Company.Include(e => e.Address).Include(e => e.Address.Area).Include(e => e.Address.City)
                                    .Include(e => e.Address.Country).Include(e => e.Address.District).Include(e => e.Address.State)
                                    .Include(e => e.Address.StateRegion).Include(e => e.Address.Taluka)
                                    .Where(e => e.Id.ToString() == SessionManager.CompanyId).SingleOrDefault();
                                double GrossSal = 0;
                                if (ca.SalaryT != null && ca.SalaryT.Count > 0)
                                {
                                    //double TotEarn = ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= mstart && Convert.ToDateTime("01/" + e.PayMonth) <= mend).Sum(e => e.TotalEarning);
                                    //double TotDed = ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= mstart && Convert.ToDateTime("01/" + e.PayMonth) <= mend).Sum(e => e.TotalDeduction);
                                    //GrossSal = TotEarn + TotDed;
                                    double TotEarn = 0;
                                    foreach (var oSalaryt in ca.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= mstart &&
                                      Convert.ToDateTime("01/" + e.PayMonth) <= mend).ToList())
                                    {


                                        var aa1 = oSalaryt.SalEarnDedT
                                            .Where(sal => sal.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && sal.SalaryHead.InITax.ToString() == "True"
                                             && sal.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "PERK")
                                            .Select(a => a.Amount).Sum();

                                        if (aa1 != null)
                                        {
                                            TotEarn = TotEarn + Convert.ToDouble(aa1);

                                        }

                                    }


                                    GrossSal = TotEarn;
                                }

                                if (ca.SalaryArrearT != null && ca.SalaryArrearT.Count > 0)
                                {
                                    double TotEarnArr = 0;
                                    foreach (var oSalarytArr in ca.SalaryArrearT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= mstart &&
                                      Convert.ToDateTime("01/" + e.PayMonth) <= mend).ToList())
                                    {


                                        var aa1Arr = oSalarytArr.SalaryArrearPaymentT
                                            .Where(sal => sal.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && sal.SalaryHead.InITax.ToString() == "True")
                                            .Select(a => a.SalHeadAmount).Sum();

                                        if (aa1Arr != null)
                                        {
                                            TotEarnArr = TotEarnArr + Convert.ToDouble(aa1Arr);

                                        }

                                    }
                                    GrossSal = GrossSal + TotEarnArr;
                                }

                                if (ca.YearlyPaymentT != null && ca.YearlyPaymentT.Count > 0)
                                {
                                    //var yearpay = ca.YearlyPaymentT.Where(e => e.SalaryHead.InITax == true && e.ReleaseFlag == true);
                                    //double Totyear = yearpay.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= ca3.QuarterFromDate.Value && Convert.ToDateTime("01/" + e.PayMonth) <= ca3.QuarterToDate.Value).Sum(e => e.AmountPaid);
                                    double Totyear = ca.YearlyPaymentT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= mstart && Convert.ToDateTime("01/" + e.PayMonth) <= mend && e.SalaryHead.InITax == true && e.ReleaseFlag == true && e.SalaryHead.InPayslip == false && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(e => e.AmountPaid);
                                    GrossSal = GrossSal + Totyear;
                                }


                                double TotTax = ca.ITChallanEmpDetails.Where(e => e.Calendar.FromDate.Value <= mstart && e.Calendar.ToDate.Value >= mend).Sum(e => e.TaxAmount);

                                foreach (var ca1 in OITForm12BAData)
                                {
                                    foreach (var ca2 in ca1.ITForm12BADataDetails)
                                    {
                                        if (ca1.ITForm16SigningPerson != null)
                                        {

                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {

                                                // Fld1 = ca1.Id.ToString(),
                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                Fld3 = CompDet.TANNo,
                                                Fld4 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + " - " + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld5 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld6 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld7 = ca.Employee.FuncStruct == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),
                                                Fld8 = GrossSal.ToString(),
                                                Fld9 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + " - " + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld12 = ca2.ITForm12BACaptionMapping.PerquisiteName.LookupVal.ToString(),
                                                Fld13 = ca2.PerquisiteRule.ToString(),
                                                Fld14 = ca2.PerquisiteActual.ToString(),
                                                Fld15 = ca2.PerquisiteChargable.ToString(),

                                                Fld16 = TotTax.ToString(),
                                                Fld17 = TotTax.ToString(),
                                                Fld18 = TotTax.ToString(),
                                                Fld19 = "As Per Form 16",

                                                Fld20 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld21 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld22 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld24 = ca1.ITForm16SigningPerson.Place//place 
                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);

                                            GenericField100 OGenericObjStatement12BA = new GenericField100()
                                            //write data to generic class
                                            {

                                                // Fld1 = ca1.Id.ToString(),
                                                Fld1 = CompDet.Name.ToString(),
                                                Fld2 = CompDet.Address == null ? "" : CompDet.Address.FullAddress.ToString(),
                                                Fld3 = CompDet.TANNo,
                                                Fld4 = ca1.PeriodFrom.Value.AddYears(1).Year.ToString() + " - " + ca1.PeriodTo.Value.AddYears(1).Year.ToString(),
                                                Fld5 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld6 = ca.Employee.EmpOffInfo == null || ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo,
                                                Fld7 = ca.Employee.FuncStruct == null ? "" : ca.Employee.FuncStruct.Job.Name.ToString(),
                                                Fld8 = GrossSal.ToString(),
                                                Fld9 = ca1.PeriodFrom.Value.ToString("dd/MM/yyyy") + " - " + ca1.PeriodTo.Value.ToString("dd/MM/yyyy"),

                                                Fld12 = ca2.ITForm12BACaptionMapping.PerquisiteName.LookupVal.ToString(),
                                                Fld13 = ca2.PerquisiteRule.ToString(),
                                                Fld14 = ca2.PerquisiteActual.ToString(),
                                                Fld15 = ca2.PerquisiteChargable.ToString(),

                                                Fld16 = TotTax.ToString(),
                                                Fld17 = TotTax.ToString(),
                                                Fld18 = TotTax.ToString(),
                                                Fld19 = "As Per Form 16",

                                                Fld20 = ca1.ITForm16SigningPerson.SigningPersonFullName, //signingpersname
                                                Fld21 = ca1.ITForm16SigningPerson.SigningPersonFMHName,  //signpersfathername
                                                Fld22 = ca1.ITForm16SigningPerson.Designation,  //signpersdesig
                                                Fld24 = ca1.ITForm16SigningPerson.Place//place 
                                            };
                                            OGenericPayrollStatement12BA.Add(OGenericObjStatement12BA);

                                        }

                                    }



                                }

                                /// pdf code start
                                ReportDataSource rdc = new ReportDataSource("P2BDataSet", OGenericPayrollStatement12BA);
                                ReportViewer ReportViewer1 = new ReportViewer();
                                ReportViewer1.LocalReport.DataSources.Clear();
                                ReportViewer1.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/" + "Payroll" + "/Rpt" + "FORM12BA" + ".rdlc");
                                Utility.ReportClass oReportClass = new Utility.ReportClass();
                                ReportParameter[] param = new ReportParameter[6];
                                param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);
                                param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                                param[4] = new ReportParameter("ReportHeaderName", "Form12BA FOR PERIOD " + mFromDate.ToShortDateString() + " To " + mToDate.ToShortDateString(), true);
                                ReportViewer1.LocalReport.EnableExternalImages = true;
                                Uri pathAsUri = new Uri(HttpContext.Current.Server.MapPath("~/Content/Login/Images/PCBL.png"));
                                if (pathAsUri != null)
                                {
                                    param[5] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                                }

                                ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4], param[5] });

                                ReportViewer1.LocalReport.DataSources.Add(rdc);
                                ///
                                string requiredPath1 = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                                                                   System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\FORM12BA\\";
                                String localPath1 = "";
                                bool exists2 = System.IO.Directory.Exists(requiredPath1);
                                if (!exists2)
                                {
                                    localPath1 = new Uri(requiredPath1).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath1);
                                }
                                ///
                                string FolderName = "FromPeriod " + mFromDate.ToShortDateString() + " To " + mToDate.ToShortDateString();

                                string FolderName1 = FolderName.Replace("/", "-");

                                //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                                //    System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Form16\\" + FolderName1 + "\\";

                                string requiredPath = Path.Combine(requiredPath1, FolderName1);

                                String localPath = "";
                                bool exists = System.IO.Directory.Exists(requiredPath);
                                if (!exists)
                                {
                                    localPath = new Uri(requiredPath).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath);
                                }

                                string ServerSavePath = localPath;
                                ////for location

                                string LocationName = ca.Employee.GeoStruct.Location.LocationObj.LocDesc;

                                string Locationcombination = Path.Combine(ServerSavePath, LocationName);


                                bool LocationExists = System.IO.Directory.Exists(Locationcombination);

                                if (!LocationExists)
                                {
                                    System.IO.Directory.CreateDirectory(Locationcombination);
                                }


                                Byte[] mybytes = ReportViewer1.LocalReport.Render("PDF");

                                string combination = Path.Combine(Locationcombination, ca.Employee.EmpCode + ".pdf");

                                bool exists1 = System.IO.Directory.Exists(combination);

                                if (!exists1)
                                {
                                    using (FileStream fs = File.Create(combination))
                                    {
                                        fs.Write(mybytes, 0, mybytes.Length);
                                    }
                                }
                                else
                                {
                                    System.IO.Directory.Delete(combination);
                                    using (FileStream fs = File.Create(combination))
                                    {
                                        fs.Write(mybytes, 0, mybytes.Length);
                                    }
                                }
                                OGenericPayrollStatement12BA.Clear();

                                /// pdf code end
                            }

                        }

                        return OGenericPayrollStatement;

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