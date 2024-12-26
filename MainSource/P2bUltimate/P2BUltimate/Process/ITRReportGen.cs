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
    public class ITRReportGen
    {

        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };
        public static List<GenericField100> GenerateITReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime mFromDate, DateTime mToDate)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {

                    case "ITPROJECTION":

                        var OITPROJECT = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITPROJECT_t = db.EmployeePayroll
                                .Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.ITProjection)
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
                            foreach (var ca in OITPROJECT)
                            {
                                var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                var OITPROJECTData = ca.ITProjection.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).OrderBy(e => e.Id).ToList();

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

                                        Fld35 = ca.Employee.PayStruct.Grade.Id == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                        Fld36 = ca.Employee.PayStruct.Grade.Code == null ? "" : ca.Employee.PayStruct.Grade.Code.ToString(),
                                        Fld37 = ca.Employee.PayStruct.Grade.Name == null ? "" : ca.Employee.PayStruct.Grade.Name.ToString(),

                                        Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.ToString(),
                                        Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.ToString(),
                                        Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.ToString(),


                                    };
                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                }
                            }

                        }

                        return OGenericPayrollStatement;

                        break;

                    case "ITCHALLANMONTHLY":
                        string Paymonth = mPayMonth.FirstOrDefault();
                        var itchallanNo = db.ITChallan.Where(e => e.SalaryMonth == Paymonth).ToList();
                        if (itchallanNo == null)
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);

                            var itchallan = db.EmployeePayroll
                                .Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.PayStruct)
                                .Include(e => e.Employee.FuncStruct)
                                .Include(e => e.ITChallanEmpDetails);

                            var ITChallanMonthly = itchallan.Where(e => EmpPayrollIdList.Contains(e.Employee.Id)).ToList();

                            OGenericPayrollStatement = new List<GenericField100>();
                            if (ITChallanMonthly == null || ITChallanMonthly.Count() == 0)
                            {
                                return null;
                            }
                            else
                            {
                                foreach (var item in itchallanNo)
                                {
                                    foreach (var ca in ITChallanMonthly)
                                    {
                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                        var empitchallndata = ca.ITChallanEmpDetails.Where(e => e.ChallanNo == item.ChallanNo).ToList();
                                        foreach (var item1 in empitchallndata)
                                        {
                                            //foreach (var ca1 in OITPROJECTData)
                                            //{
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {

                                                Fld1 = item.Id.ToString(),
                                                Fld2 = ca.Employee.EmpCode,
                                                Fld3 = ca.Employee.EmpName.FullNameFML,
                                                Fld4 = item1.TaxAmount.ToString(),
                                                Fld5 = item.SalaryMonth,
                                                Fld6 = item.BankBSRCode,
                                                Fld7 = item.ChallanNo,
                                                Fld8 = item.TaxAmount.ToString(),
                                                Fld9 = item.ExtraChallan.ToString()
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
                                    //}
                                }

                            }
                        }
                        return OGenericPayrollStatement;
                        break;

                    case "ITSECTION10PAYMENT":
                        var OITSection10PaymentData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITSection10PaymentData_temp = db.EmployeePayroll
                            .Include(e => e.Employee)
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.FuncStruct)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.ITSection10Payment)
                            .Include(e => e.ITSection10Payment.Select(r => r.FinancialYear))
                            .Include(e => e.ITSection10Payment.Select(r => r.ITSection10))
                            .Include(e => e.ITSection10Payment.Select(r => r.ITSection10.Itsection10salhead.Select(t => t.SalHead)))
                            .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSectionList))
                            .Include(e => e.ITSection10Payment.Select(r => r.ITSection.ITSectionListType)).AsNoTracking()
                            .Where(e => e.Employee.Id == item)
                            .FirstOrDefault();
                            if (OITSection10PaymentData_temp != null)
                            {
                                OITSection10PaymentData.Add(OITSection10PaymentData_temp);
                            }
                        }
                        if (OITSection10PaymentData == null || OITSection10PaymentData.Count() == 0)
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


                            foreach (var ca in OITSection10PaymentData)
                            {
                                if (ca.ITSection10Payment != null && ca.ITSection10Payment.Count() != 0)
                                {
                                    var m = mPayMonth.Count();
                                    var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                    var mend = Convert.ToDateTime("01/" + (mPayMonth.Skip(m - 1).SingleOrDefault().ToString())).Date;
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            // var OInvestData = ca.ITSection10Payment.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                            var OInvestData = ca.ITSection10Payment.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend && e.ITSection.ITSectionList.LookupVal == item.ToString()).ToList();
                                            if (OInvestData != null && OInvestData.Count() != 0)
                                            {
                                                int geoid = ca.Employee.GeoStruct.Id;

                                                int payid = ca.Employee.PayStruct.Id;

                                                int funid = ca.Employee.FuncStruct.Id;

                                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                                {

                                                    foreach (var ca1 in OInvestData)
                                                    {
                                                        if (ca1.ITSection10 != null)
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
                                                                    Fld14 = ca1.InvestmentDate.Value.ToShortDateString(),
                                                                    Fld15 = ca1.Narration,


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
                                                                        Fld14 = ca1.InvestmentDate.Value.ToShortDateString(),
                                                                        Fld15 = ca1.Narration,
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

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // var OInvestData = ca.ITSection10Payment.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                        var OInvestData = ca.ITSection10Payment.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                        if (OInvestData != null && OInvestData.Count() != 0)
                                        {
                                            int geoid = ca.Employee.GeoStruct.Id;

                                            int payid = ca.Employee.PayStruct.Id;

                                            int funid = ca.Employee.FuncStruct.Id;

                                            GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                            PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                            FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                            if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                            {

                                                foreach (var ca1 in OInvestData)
                                                {
                                                    if (ca1.ITSection10 != null)
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
                                                                Fld14 = ca1.InvestmentDate.Value.ToShortDateString(),
                                                                Fld15 = ca1.Narration,


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
                                                                    Fld14 = ca1.InvestmentDate.Value.ToShortDateString(),
                                                                    Fld15 = ca1.Narration,
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


                    case "ITSECTION24PAYMENT":
                        var OITSection24PaymentData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITSection24PaymentData_temp = db.EmployeePayroll
                             .Include(e => e.Employee)
                              .Include(e => e.Employee.GeoStruct)
                               .Include(e => e.Employee.PayStruct)
                                .Include(e => e.Employee.FuncStruct)
                             .Include(e => e.Employee.EmpName)
                            .Include(e => e.ITSection24Payment)
                            .Include(e => e.ITSection24Payment.Select(r => r.FinancialYear))
                            .Include(e => e.ITSection24Payment.Select(r => r.LoanAdvanceHead))
                           .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionList))
                            .Include(e => e.ITSection24Payment.Select(r => r.ITSection.ITSectionListType))
                            .Where(e => e.Employee.Id == item)
                            .FirstOrDefault();
                            if (OITSection24PaymentData_temp != null)
                            {
                                OITSection24PaymentData.Add(OITSection24PaymentData_temp);
                            }
                        }
                        if (OITSection24PaymentData == null || OITSection24PaymentData.Count() == 0)
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

                            foreach (var ca in OITSection24PaymentData)
                            {
                                if (ca.ITSection24Payment != null && ca.ITSection24Payment.Count() != 0)
                                {
                                    var m = mPayMonth.Count();
                                    var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                    var mend = Convert.ToDateTime("01/" + (mPayMonth.Skip(m - 1).SingleOrDefault().ToString())).Date;

                                    var OInvestData = ca.ITSection24Payment.Where(e => e.FinancialYear.FromDate.Value <= mstart && e.FinancialYear.ToDate.Value >= mend).ToList();
                                    if (OInvestData != null && OInvestData.Count() != 0)
                                    {
                                        int geoid = ca.Employee.GeoStruct.Id;

                                        int payid = ca.Employee.PayStruct.Id;

                                        int funid = ca.Employee.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);


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
                                            if (month)
                                            {
                                                OGenericObjStatement.Fld100 = m.ToString();
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
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "INVESTMENTPAYMENT":
                        var OITInvestmentPaymentData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OITInvestmentPaymentData_temp = db.EmployeePayroll
                             .Include(e => e.Employee)
                             .Include(e => e.Employee.EmpName)
                             .Include(e => e.Employee.GeoStruct)
                             .Include(e => e.Employee.FuncStruct)
                             .Include(e => e.Employee.PayStruct)
                            .Include(e => e.ITInvestmentPayment)
                            .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))
                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                            .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();
                            if (OITInvestmentPaymentData_temp != null)
                            {
                                OITInvestmentPaymentData.Add(OITInvestmentPaymentData_temp);
                            }
                        }
                        if (OITInvestmentPaymentData == null || OITInvestmentPaymentData.Count() == 0)
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
                            foreach (var ca in OITInvestmentPaymentData)
                            {
                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (ca.ITInvestmentPayment != null && ca.ITInvestmentPayment.Count() != 0)
                                {
                                    var OInvestDatachk = ca.ITInvestmentPayment.Where(q => q.ITInvestment != null);
                                    // var m = mPayMonth.Count();
                                    // var mstart = Convert.ToDateTime("01/" + (mPayMonth.Take(1).SingleOrDefault().ToString())).Date;
                                    // var mend = Convert.ToDateTime("01/" + (mPayMonth.Skip(m - 1).SingleOrDefault().ToString())).Date;
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            var OInvestData = OInvestDatachk.Where(e => e.FinancialYear.FromDate.Value <= mFromDate && e.FinancialYear.ToDate.Value >= mToDate && e.ITInvestment.ITInvestmentName == item.ToString()).ToList();
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
                                                        Fld6 = ca1.FinancialYear != null ? ca1.FinancialYear.ToDate.Value.ToShortDateString() : null,
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
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var OInvestData = ca.ITInvestmentPayment.Where(e => e.FinancialYear.FromDate.Value <= mFromDate && e.FinancialYear.ToDate.Value >= mToDate).ToList();
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
                                                    Fld6 = ca1.FinancialYear != null ? ca1.FinancialYear.ToDate.Value.ToShortDateString() : null,
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
                                        }
                                    }

                                }


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