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
using P2BUltimate.Process;
using System.Web;
using System.IO;
using System.Configuration;
using P2BUltimate.Controllers.Leave.MainController;
using P2B.UTILS;

namespace P2BUltimate.Process
{
    public class PayrollProcessReportGen
    {

        public class JvDataForBranchWise
        {
            public string BranchCodeInward { get; set; }
            public string NarrationInward { get; set; }
            public string AmountInward { get; set; }
            public string BranchCodeOutward { get; set; }
            public string NarrationOutward { get; set; }
            public string AmountOutward { get; set; }
            public string Flag { get; set; }
            public string LocationCode { get; set; }
        }
        public class CORE_EmpDMS
        {
            // public CORE_EmpDMS();

            public string FunctionalModule { get; set; }
            public int EmployeeDMS_Doc_Id { get; set; }
            public int Emp_Id { get; set; }
            public int? DocumentType_Id { get; set; }
            public string DocumentType { get; set; }
            public string DocumentTypeDesc { get; set; }
            public int Document_Size_Appl { get; set; }
            public string DMS_ProcessType { get; set; }
            public List<CORE_EmpDMS_SubDoc> CORE_EmpDMS_SubDoc { get; set; }
        }
        public class CORE_EmpDMS_SubDoc
        {
            //  public CORE_EmpDMS_SubDoc();

            public int SubDocument_Id { get; set; }
            public int SubDocumentName_Id { get; set; }
            public string SubDocumentName { get; set; }
            public string SubDocumentTypeDesc { get; set; }
            public DateTime? UploadDate { get; set; }
            public string DocumentPath { get; set; }
            public string FileName { get; set; }
            public string FileArray { get; set; }
            public string FileExt { get; set; }
            public bool IsActive { get; set; }
            public string DMSWF_Status { get; set; }
            public string DMSWFDetail_Status { get; set; }
            public string DNSWFDetail_Comments { get; set; }
        }
        public class SalHeadFormulaT
        {
            public int Id { get; set; }
            public GeoStruct GeoStruct { get; set; }
            public PayStruct PayStruct { get; set; }
            public FuncStruct FuncStruct { get; set; }
            public LookupValue FormulaType { get; set; }
            public string Name { get; set; }
        }
        public static List<GenericField100> GenerateProcessReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime mFromDate, DateTime mToDate, DateTime pfromdate, DateTime ptodate, string Sorting, List<string> salheadlistLevel1)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();

            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {
                    case "EMPBRANCHWISE":

                        var OLocationObj = db.LocationObj.OrderBy(e => e.LocCode).ToList().Select(r => r.LocCode);
                        foreach (var item1 in OLocationObj)
                        {
                            var OLocationId = db.Location.Include(e => e.LocationObj).Where(e => e.LocationObj.LocCode == item1).FirstOrDefault().Id;
                            var OGeostructId = db.GeoStruct.Include(e => e.Location).Where(e => e.Location.Id == OLocationId).ToList().Select(r => r.Id);

                            var EmpData = new List<Employee>();

                            if (OGeostructId.Count() > 0)
                            {

                                EmpData = db.Employee
                                          .Include(e => e.EmpName)
                                          .Include(e => e.GeoStruct)
                                          .Include(e => e.PayStruct)
                                          .Include(e => e.FuncStruct)
                                          .Include(e => e.FuncStruct.Job)
                                          .Where(e => OGeostructId.Contains(e.GeoStruct.Id)).ToList();

                                var EmpData1 = new List<Employee>();

                                if (salheadlistLevel1 != null && salheadlistLevel1.Count() > 0)
                                {
                                    EmpData1 = EmpData.Where(e => salheadlistLevel1.Contains(e.FuncStruct.Job.Name)).ToList();
                                }
                                else
                                {
                                    EmpData1 = EmpData.ToList();
                                }


                                if (EmpData1 != null && EmpData1.Count() > 0)
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



                                    if (salheadlist != null && salheadlist.Count() > 0)
                                    {
                                        foreach (var EmpCount in salheadlist)
                                        {
                                            string[] values = EmpCount.Split('-');

                                            int v1 = Convert.ToInt32(values[0]);
                                            int v2 = Convert.ToInt32(values[1]);

                                            if (EmpData1.Count() == v2)
                                            {
                                                foreach (var a in EmpData1)
                                                {
                                                    //int geoid = a.GeoStruct.Id;

                                                    //int payid = a.PayStruct.Id;

                                                    //int funid = a.FuncStruct.Id;

                                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                    int? geoid = a.GeoStruct_Id;

                                                    int? payid = a.PayStruct_Id;

                                                    int? funid = a.FuncStruct_Id;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);



                                                    GenericField100 OGenericObjStatement = new GenericField100
                                                    {

                                                        Fld1 = a.EmpCode == null ? "" : a.EmpCode,
                                                        Fld2 = a.EmpName == null ? "" : a.EmpName.FullNameFML,
                                                        Fld3 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                        Fld4 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name,
                                                        Fld5 = GeoDataInd == null ? "" : GeoDataInd.GeoStruct_Location_Name,

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
                                    }

                                    else
                                    {
                                        foreach (var a in EmpData1)
                                        {
                                            //int geoid = a.GeoStruct.Id;

                                            //int payid = a.PayStruct.Id;

                                            //int funid = a.FuncStruct.Id;

                                            //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                            //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                            //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                            int? geoid = a.GeoStruct_Id;

                                            int? payid = a.PayStruct_Id;

                                            int? funid = a.FuncStruct_Id;

                                            GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                            PayStruct paystruct = db.PayStruct.Find(payid);

                                            FuncStruct funstruct = db.FuncStruct.Find(funid);

                                            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                            GenericField100 OGenericObjStatement = new GenericField100
                                            {

                                                Fld1 = a.EmpCode == null ? "" : a.EmpCode,
                                                Fld2 = a.EmpName == null ? "" : a.EmpName.FullNameFML,
                                                Fld3 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                Fld4 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name,
                                                Fld5 = GeoDataInd == null ? "" : GeoDataInd.GeoStruct_Location_Name,
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
                            }
                        }
                        return OGenericPayrollStatement;


                        break;

                    case "PAYSLIP":
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
                            //bool emp = false;
                            //bool loca = false;
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
                            var paymentbank = false;
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
                                if (item.LookupVal.ToUpper() == "PAYMENTBANK")
                                {
                                    paymentbank = true;
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

                            //  PayslipRList.AddRange(oSalaryT.PayslipR.Where(e => e.PayMonth == item).ToList());

                            List<string> BusiCategory = new List<string>();
                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                            {
                                BusiCategory.AddRange(salheadlist);

                                foreach (EmployeePayroll ca in OPayslipData)
                                {
                                    if (ca != null && ca.SalaryT != null && ca.SalaryT.Count() != 0)
                                    {
                                        //  foreach (SalaryT oSalaryT in ca.SalaryT.Where(e => e.Geostruct != null && e.Geostruct.Location != null && e.Geostruct.Location.BusinessCategory != null))
                                        foreach (SalaryT oSalaryT in ca.SalaryT)
                                        {
                                            foreach (string item in mPayMonth)
                                            {
                                                foreach (string ca1 in BusiCategory)
                                                {
                                                    if (ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Bank != null && ca.Employee.EmpOffInfo.Bank.Name != null)
                                                    {
                                                        if (ca.Employee.EmpOffInfo.Bank.Name.ToUpper() == ca1.ToUpper())
                                                        {
                                                            PayslipRList.AddRange(oSalaryT.PayslipR.Where(e => e.PayMonth == item).ToList());
                                                        }
                                                    }
                                                    //  PayslipRList.AddRange(oSalaryT.PayslipR.Where(e => e.PayMonth == item && e.GeoStruct.Location.BusinessCategory.LookupVal.ToString().ToUpper() == ca1.ToUpper()).ToList());
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (EmployeePayroll ca in OPayslipData)
                                {
                                    if (ca != null && ca.SalaryT != null && ca.SalaryT.Count() != 0)
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
                                List<GenericField100> OGenericPayrollStatementPay = new List<GenericField100>();
                                var UANNO = db.Employee.Where(e => e.EmpCode == ca2.EmpCode).OrderBy(e => e.Id
                                    ).Select(a => a.EmpOffInfo.NationalityID.UANNo).AsNoTracking().SingleOrDefault();
                                var PaymentBank = db.Employee.Where(e => e.EmpCode == ca2.EmpCode).OrderBy(e => e.Id
                                   ).Select(a => a.EmpOffInfo.Bank.Name).AsNoTracking().SingleOrDefault();
                                string v = DateTime.Parse("01/" + ca2.PayMonth).ToShortDateString();

                                string dt = DateTime.Parse(v.Trim()).ToString("MMMM/yyyy", CultureInfo.InvariantCulture);

                                int geoid = ca2.GeoStruct.Id;

                                int payid = ca2.PayStruct.Id;

                                int funid = ca2.FuncStruct.Id;
                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                var business = db.GeoStruct.Include(e => e.Location).Include(e => e.Location.BusinessCategory)
                                    .Include(e => e.Location.LocationObj).Where(e => e.Id == geoid).FirstOrDefault();

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
                                    Fld35 = FuncdataInd.Job_Code + " " + FuncdataInd.Job_Name,
                                    Fld36 = ca2.DateJoining != null ? ca2.DateJoining.Value.ToShortDateString() : null,
                                    //Fld37 = ca2.DateIncrement != null ? ca2.DateIncrement.Value.ToShortDateString() : null,
                                    Fld38 = ca2.DateRetirement != null ? ca2.DateRetirement.Value.ToShortDateString() : null,
                                    Fld39 = ca2.DateBirth != null ? ca2.DateBirth.Value.ToShortDateString() : null,
                                    Fld40 = ca2.DateConfirmation != null ? ca2.DateConfirmation.Value.ToShortDateString() : null,
                                    Fld41 = ca2.DateProbation != null ? ca2.DateProbation.Value.ToShortDateString() : null,
                                    //Fld42 = ca2.DateTransfer != null ? ca2.DateTransfer.Value.ToShortDateString() : null,
                                    //Fld43 = ca2.DatePromotion != null ? ca2.DatePromotion.Value.ToShortDateString() : null,
                                    Fld44 = ca2.PayableDays.ToString(),
                                    Fld45 = DateTime.Parse("01/" + ca2.PayMonth).AddMonths(1).AddDays(-1).Day.ToString(),//month days
                                    //Fld45 = ca2.LWPDays.ToString(),
                                    //Fld46 = ca2.StdPay.ToString(),
                                    //Fld47 = ca2.GrossPay.ToString(),
                                    //Fld48 = ca2.DeductionPay.ToString(),
                                    Fld49 = ca2.NetPay.ToString(),
                                    Fld50 = ca2.NetPayInWords,
                                    //Fld51 = ca2.ProvisionalGratuity.ToString(),
                                    //Fld52 = ca2.PayslipNote,
                                    Fld53 = PaydataInd.Grade_Code==null ? "" : PaydataInd.Grade_Code,
                                    //Fld54 = ca2.TotalITaxLiability.ToString(),
                                    //Fld55 = ca2.TotalITaxPaid.ToString(),
                                    //Fld56 = ca2.TotalITaxBalance.ToString(),
                                    //Fld57 = ca2.ITaxPerMonth.ToString(),
                                    //Fld58 = ca2.PaySlipLock.ToString(),
                                    Fld59 = ca2.PayslipLockDate.ToString(),
                                    Fld70 = ca2.BasicScale,
                                    Fld72 = UANNO,
                                    Fld75 = ca2.Division != null ? ca2.Division.ToString() : null,
                                    //  Fld85 = business != null && business.Location != null && business.Location.BusinessCategory != null ? business.Location.BusinessCategory.LookupVal.ToString() : null,
                                    Fld85 = PaymentBank,
                                    Fld87 = PaymentBank


                                };
                                if (comp)
                                {
                                    oGenericField1001.Fld99 = GeoDataInd.Company_Name;
                                }
                                if (div)
                                {
                                    oGenericField1001.Fld98 = GeoDataInd.Division_Name;
                                }
                                if (loca)
                                {
                                    oGenericField1001.Fld97 = GeoDataInd.LocDesc;
                                }
                                if (dept)
                                {
                                    oGenericField1001.Fld96 = GeoDataInd.DeptDesc;
                                }
                                if (grp)
                                {
                                    oGenericField1001.Fld95 = GeoDataInd.Group_Name;
                                }
                                if (unit)
                                {
                                    oGenericField1001.Fld94 = GeoDataInd.Unit_Name;
                                }
                                if (grade)
                                {
                                    oGenericField1001.Fld93 = PaydataInd.Grade_Name;
                                }
                                if (lvl)
                                {
                                    oGenericField1001.Fld92 = PaydataInd.Level_Name;
                                }
                                if (jobstat)
                                {
                                    oGenericField1001.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                }
                                if (job)
                                {
                                    oGenericField1001.Fld90 = FuncdataInd.Job_Name;
                                }
                                if (jobpos)
                                {
                                    oGenericField1001.Fld89 = FuncdataInd.JobPositionDesc;
                                }
                                if (paymentbank)
                                {
                                    oGenericField1001.Fld87 = PaymentBank;
                                }


                                OGenericPayrollStatement.Add(oGenericField1001);
                                OGenericPayrollStatementPay.Add(oGenericField1001);

                                if (ca2.PaySlipDetailDedR != null && ca2.PaySlipDetailDedR.ToList().Count() != null)
                                {
                                    foreach (PaySlipDetailDedR item in ca2.PaySlipDetailDedR)
                                    {
                                        if (item.DedAmount != 0)
                                        {
                                            int geoid1 = ca2.GeoStruct.Id;

                                            int payid1 = ca2.PayStruct.Id;

                                            int funid1 = ca2.FuncStruct.Id;
                                            GeoDataInd = GetViewDataIndiv(geoid1, payid1, funid1, GeoData, 0);
                                            PaydataInd = GetViewDataIndiv(geoid1, payid1, funid1, Paydata, 1);
                                            FuncdataInd = GetViewDataIndiv(geoid1, payid1, funid1, Funcdata, 2);

                                            GenericField100 oGenericField100 = new GenericField100()
                                            {
                                                Fld1 = ca2.EmpCode,
                                                Fld21 = dt,
                                                Fld28 = ca2.Location,
                                                Fld60 = item.SalHeadDesc,
                                                Fld61 = item.StdSalAmount.ToString(),
                                                Fld62 = item.DedAmount.ToString(),
                                                //  Fld85 = business != null && business.Location != null && business.Location.BusinessCategory != null ? business.Location.BusinessCategory.LookupVal.ToString() : null,
                                                Fld85 = PaymentBank,
                                                Fld87 = PaymentBank
                                            };
                                            if (comp)
                                            {
                                                oGenericField100.Fld99 = GeoDataInd.Company_Name;
                                            }
                                            if (div)
                                            {
                                                oGenericField100.Fld98 = GeoDataInd.Division_Name;
                                            }
                                            if (loca)
                                            {
                                                oGenericField100.Fld97 = GeoDataInd.LocDesc;
                                            }
                                            if (dept)
                                            {
                                                oGenericField100.Fld96 = GeoDataInd.DeptDesc;
                                            }
                                            if (grp)
                                            {
                                                oGenericField100.Fld95 = GeoDataInd.Group_Name;
                                            }
                                            if (unit)
                                            {
                                                oGenericField100.Fld94 = GeoDataInd.Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                oGenericField100.Fld93 = PaydataInd.Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                oGenericField100.Fld92 = PaydataInd.Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                oGenericField100.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                oGenericField100.Fld90 = FuncdataInd.Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                oGenericField100.Fld89 = FuncdataInd.JobPositionDesc;
                                            }
                                            if (paymentbank)
                                            {
                                                oGenericField1001.Fld87 = PaymentBank;
                                            }


                                            OGenericPayrollStatement.Add(oGenericField100);
                                            OGenericPayrollStatementPay.Add(oGenericField100);
                                        }

                                    }
                                }
                                if (ca2.PaySlipDetailEarnR != null)
                                {
                                    foreach (PaySlipDetailEarnR item in ca2.PaySlipDetailEarnR)
                                    {
                                        if (item.EarnAmount != 0)
                                        {
                                            int geoid2 = ca2.GeoStruct.Id;

                                            int payid2 = ca2.PayStruct.Id;

                                            int funid2 = ca2.FuncStruct.Id;

                                            GeoDataInd = GetViewDataIndiv(geoid2, payid2, funid2, GeoData, 0);
                                            PaydataInd = GetViewDataIndiv(geoid2, payid2, funid2, Paydata, 1);
                                            FuncdataInd = GetViewDataIndiv(geoid2, payid2, funid2, Funcdata, 2);

                                            GenericField100 oGenericField100 = new GenericField100()
                                            {
                                                Fld1 = ca2.EmpCode,
                                                Fld21 = dt,
                                                Fld28 = ca2.Location,
                                                Fld63 = item.SalHeadDesc,
                                                Fld64 = item.StdSalAmount.ToString(),
                                                Fld65 = item.EarnAmount.ToString(),
                                                // Fld85 = business != null && business.Location != null && business.Location.BusinessCategory != null ? business.Location.BusinessCategory.LookupVal.ToString() : null,
                                                Fld85 = PaymentBank,
                                                Fld87 = PaymentBank
                                            };
                                            if (comp)
                                            {
                                                oGenericField100.Fld99 = GeoDataInd.Company_Name;
                                            }
                                            if (div)
                                            {
                                                oGenericField100.Fld98 = GeoDataInd.Division_Name;
                                            }
                                            if (loca)
                                            {
                                                oGenericField100.Fld97 = GeoDataInd.LocDesc;
                                            }
                                            if (dept)
                                            {
                                                oGenericField100.Fld96 = GeoDataInd.DeptDesc;
                                            }
                                            if (grp)
                                            {
                                                oGenericField100.Fld95 = GeoDataInd.Group_Name;
                                            }
                                            if (unit)
                                            {
                                                oGenericField100.Fld94 = GeoDataInd.Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                oGenericField100.Fld93 = PaydataInd.Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                oGenericField100.Fld92 = PaydataInd.Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                oGenericField100.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                oGenericField100.Fld90 = FuncdataInd.Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                oGenericField100.Fld89 = FuncdataInd.JobPositionDesc;
                                            }
                                            if (paymentbank)
                                            {
                                                oGenericField1001.Fld87 = PaymentBank;
                                            }
                                            OGenericPayrollStatement.Add(oGenericField100);
                                            OGenericPayrollStatementPay.Add(oGenericField100);
                                        }
                                    }
                                }

                                if (ca2.PaySlipDetailLeaveR != null)
                                {

                                    foreach (PaySlipDetailLeaveR item in ca2.PaySlipDetailLeaveR)
                                    {
                                        int geoid3 = ca2.GeoStruct.Id;

                                        int payid3 = ca2.PayStruct.Id;

                                        int funid3 = ca2.FuncStruct.Id;

                                        GeoDataInd = GetViewDataIndiv(geoid3, payid3, funid3, GeoData, 0);
                                        PaydataInd = GetViewDataIndiv(geoid3, payid3, funid3, Paydata, 1);
                                        FuncdataInd = GetViewDataIndiv(geoid3, payid3, funid3, Funcdata, 2);

                                        GenericField100 oGenericField100 = new GenericField100()
                                        {
                                            Fld1 = ca2.EmpCode,
                                            Fld21 = dt,
                                            Fld28 = ca2.Location,
                                            Fld67 = item.LeaveHead,
                                            Fld68 = item.LeaveCloseBal.ToString(),
                                            Fld69 = item.LeaveUtilized.ToString(),
                                            //  Fld85 = business != null && business.Location != null && business.Location.BusinessCategory != null ? business.Location.BusinessCategory.LookupVal.ToString() : null,
                                            Fld85 = PaymentBank,
                                            Fld87 = PaymentBank
                                        };
                                        if (comp)
                                        {
                                            oGenericField100.Fld99 = GeoDataInd.Company_Name;
                                        }
                                        if (div)
                                        {
                                            oGenericField100.Fld98 = GeoDataInd.Division_Name;
                                        }
                                        if (loca)
                                        {
                                            oGenericField100.Fld97 = GeoDataInd.LocDesc;
                                        }
                                        if (dept)
                                        {
                                            oGenericField100.Fld96 = GeoDataInd.DeptDesc;
                                        }
                                        if (grp)
                                        {
                                            oGenericField100.Fld95 = GeoDataInd.Group_Name;
                                        }
                                        if (unit)
                                        {
                                            oGenericField100.Fld94 = GeoDataInd.Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            oGenericField100.Fld93 = PaydataInd.Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            oGenericField100.Fld92 = PaydataInd.Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            oGenericField100.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            oGenericField100.Fld90 = FuncdataInd.Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            oGenericField100.Fld89 = FuncdataInd.JobPositionDesc;
                                        }
                                        if (paymentbank)
                                        {
                                            oGenericField1001.Fld87 = PaymentBank;
                                        }
                                        OGenericPayrollStatement.Add(oGenericField100);
                                        OGenericPayrollStatementPay.Add(oGenericField100);
                                    }
                                }

                                if (GeoDataInd.Comp_Code == "AWDCC")
                                {

                                    string newmonth = Convert.ToDateTime(mPayMonth.FirstOrDefault()).ToString("MMM") + "-" + Convert.ToDateTime(mPayMonth.FirstOrDefault()).Year;
                                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", OGenericPayrollStatementPay);
                                    ReportViewer ReportViewer1 = new ReportViewer();
                                    ReportViewer1.LocalReport.DataSources.Clear();
                                    ReportViewer1.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/" + "Payroll" + "/Rpt" + "PAYSLIP" + ".rdlc");
                                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                                    ReportParameter[] param = new ReportParameter[6];
                                    param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);
                                    param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                                    param[4] = new ReportParameter("ReportHeaderName", "Payslip For Month - " + newmonth, true);
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
                                                                       System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + @"\P2BUltimate\App_Data\Payslip\";
                                    String localPath1 = "";
                                    bool exists2 = System.IO.Directory.Exists(requiredPath1);
                                    if (!exists2)
                                    {
                                        localPath1 = new Uri(requiredPath1).LocalPath;
                                        System.IO.Directory.CreateDirectory(localPath1);
                                    }
                                    ///

                                    string FolderName1 = ca2.PayMonth.Replace("/", "-");

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


                                    string Locationcombination = Path.Combine(localPath, ca2.Location);
                                    bool LocationExists = System.IO.Directory.Exists(Locationcombination);

                                    if (!LocationExists)
                                    {
                                        System.IO.Directory.CreateDirectory(Locationcombination);
                                    }


                                    Byte[] mybytes = ReportViewer1.LocalReport.Render("PDF");

                                    string combination = Path.Combine(Locationcombination, ca2.EmpCode + ".pdf");

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
                                    OGenericPayrollStatementPay = null;
                                }

                                //API call
                                if ((GeoDataInd.Comp_Code != "" || GeoDataInd.Comp_Code != null) && ca2.PaySlipLock == true)
                                {
                                    string newmonth = Convert.ToDateTime(mPayMonth.FirstOrDefault()).ToString("MMM") + "-" + Convert.ToDateTime(mPayMonth.FirstOrDefault()).Year;
                                    ReportDataSource rdc = new ReportDataSource("P2BDataSet", OGenericPayrollStatementPay);
                                    ReportViewer ReportViewer1 = new ReportViewer();
                                    ReportViewer1.LocalReport.DataSources.Clear();
                                    ReportViewer1.LocalReport.ReportPath = HttpContext.Current.Server.MapPath("~/Reports/" + "Payroll" + "/Rpt" + "PAYSLIP" + ".rdlc");
                                    Utility.ReportClass oReportClass = new Utility.ReportClass();
                                    ReportParameter[] param = new ReportParameter[6];
                                    param[0] = new ReportParameter("ReportCompanyName", oReportClass.ReportCompanyName, true);
                                    param[1] = new ReportParameter("ReportCreatedBy", oReportClass.ReportCreatedBy, true);
                                    param[4] = new ReportParameter("ReportHeaderName", "Payslip For Month - " + newmonth, true);
                                    ReportViewer1.LocalReport.EnableExternalImages = true;
                                    Uri pathAsUri = new Uri(HttpContext.Current.Server.MapPath("~/Content/Login/Images/PCBL.png"));
                                    if (pathAsUri != null)
                                    {
                                        param[5] = new ReportParameter("ReportCompanyLogo", pathAsUri.AbsoluteUri);
                                    }

                                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { param[0], param[1], param[4], param[5] });

                                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                                    ///
                                    LookupValue documenttype = new LookupValue();
                                    LookupValue Subdocumenttype = new LookupValue();
                                    LookupValue Functionalmoduletype = new LookupValue();
                                    var Functionalmodule = db.Lookup
                                    .Include(e => e.LookupValues)
                                    .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();

                                    Functionalmoduletype = Functionalmodule.LookupValues.Where(e => e.LookupVal.ToUpper() == "EPMS").FirstOrDefault();

                                    var document_type = db.Lookup
                                     .Include(e => e.LookupValues)
                                     .Where(e => e.Code == "1004").AsNoTracking().FirstOrDefault();
                                    if (document_type != null)
                                    {
                                        documenttype = document_type.LookupValues.Where(e => e.LookupVal.ToUpper() == "PAYSLIP" && e.LookupValData.ToUpper() == "EPMS").FirstOrDefault();
                                        if (documenttype != null)
                                        {


                                            var empid = db.Employee.Where(e => e.EmpCode == ca2.EmpCode).FirstOrDefault();
                                            string payslipmonth = Convert.ToDateTime(mPayMonth.FirstOrDefault()).ToString("MM") + "-" + Convert.ToDateTime(mPayMonth.FirstOrDefault()).Year;

                                            Byte[] mybytes = ReportViewer1.LocalReport.Render("PDF");
                                            string filearray = Convert.ToBase64String(mybytes);

                                            LvNewReqController.ServiceResult<P2B.MOBILE.CORE_EmpDMS> responseDeserializeData = new LvNewReqController.ServiceResult<P2B.MOBILE.CORE_EmpDMS>();
                                            int? errorno = 0;
                                            int? Infono = 0;
                                            var ShowMessage = "";



                                            string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                                            using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                                            {
                                                List<CORE_EmpDMS_SubDoc> subd = new List<CORE_EmpDMS_SubDoc>();

                                                subd.Add(new CORE_EmpDMS_SubDoc
                                                {
                                                    FileName = payslipmonth.ToString() + ".pdf",
                                                    FileArray = filearray,
                                                    FileExt = "pdf",
                                                    SubDocumentName = payslipmonth.ToString(),
                                                    //  SubDocument_Id = Subdocumenttype.Id,
                                                    IsActive = true,
                                                });

                                                var response = p2BHttpClient.request("EDMS/Write_EmployeeDMS",
                                                    new CORE_EmpDMS()
                                                    {
                                                        Emp_Id = empid.Id,
                                                        FunctionalModule = Functionalmoduletype.LookupVal.ToUpper(),
                                                        DocumentType = documenttype.LookupVal.ToUpper(),
                                                        DocumentType_Id = documenttype.Id,
                                                        CORE_EmpDMS_SubDoc = subd,


                                                    }
                                                    );

                                                responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<LvNewReqController.ServiceResult<P2B.MOBILE.CORE_EmpDMS>>(response.Content.ReadAsStringAsync().Result);
                                                if (responseDeserializeData.Data != null)
                                                {
                                                    //errorno = responseDeserializeData.Data.ErrNo;
                                                    //ShowMessage = responseDeserializeData.Data.ErrMsg;
                                                    //Infono = responseDeserializeData.Data.InfoNo;
                                                }
                                                else
                                                {
                                                    //errorno = 1;
                                                    //ShowMessage = responseDeserializeData.Message.ToString();
                                                }







                                            }

                                            // Api call for insert record in 

                                            //string requiredPath1 = ConfigurationManager.AppSettings["DocumentPath"];


                                            //String localPath1 = "";
                                            //bool exists2 = System.IO.Directory.Exists(requiredPath1);
                                            //if (!exists2)
                                            //{
                                            //    localPath1 = new Uri(requiredPath1).LocalPath;
                                            //    System.IO.Directory.CreateDirectory(localPath1);
                                            //}
                                            /////


                                            //string FolderName1 = empid.Id + "\\" + documenttype.LookupValData + "\\" + document_type.Id;//ca2.PayMonth.Replace("/", "-");

                                            //string requiredPath = Path.Combine(requiredPath1, FolderName1);

                                            //String localPath = "";
                                            //bool exists = System.IO.Directory.Exists(requiredPath);
                                            //if (!exists)
                                            //{
                                            //    localPath = new Uri(requiredPath).LocalPath;
                                            //    System.IO.Directory.CreateDirectory(localPath);
                                            //}


                                            //string payslipmonth = Convert.ToDateTime(mPayMonth.FirstOrDefault()).ToString("MM") + "-" + Convert.ToDateTime(mPayMonth.FirstOrDefault()).Year;

                                            //Byte[] mybytes = ReportViewer1.LocalReport.Render("PDF");

                                            //string combination = Path.Combine(requiredPath, payslipmonth + ".pdf");

                                            //bool exists1 = System.IO.Directory.Exists(combination);

                                            //if (!exists1)
                                            //{
                                            //    using (FileStream fs = File.Create(combination))
                                            //    {
                                            //        fs.Write(mybytes, 0, mybytes.Length);
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    System.IO.Directory.Delete(combination);
                                            //    using (FileStream fs = File.Create(combination))
                                            //    {
                                            //        fs.Write(mybytes, 0, mybytes.Length);
                                            //    }
                                            //}


                                        }
                                    }



                                    OGenericPayrollStatementPay = null;
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

                    case "BONUSCHKT":
                        var OBonusChkData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OBonusChkData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.BonusChkT)

                               //   .Include(e => e.BonusChkT.Select(r => r.BonusCalendar))
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)

                                 //.Include(e => e.Employee.ServiceBookDates)
                                 .Where(e => e.Employee.Id == item).AsNoTracking()
                                 .FirstOrDefault();
                            if (OBonusChkData_t != null)
                            {
                                OBonusChkData_t.Employee.EmpName = db.NameSingle.Find(OBonusChkData_t.Employee.EmpName_Id);
                                OBonusChkData_t.BonusChkT = db.BonusChkT.Include(e => e.BonusCalendar).Where(e => e.BonusCalendar.FromDate.Value <= mFromDate && e.BonusCalendar.ToDate.Value >= mToDate && e.EmployeePayroll_Id == OBonusChkData_t.Id).AsNoTracking().ToList();
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

                            foreach (var ca in OBonusChkData)
                            {
                                if (ca.BonusChkT == null || ca.BonusChkT.Count() != 0)
                                {
                                    var OBonusSalData = ca.BonusChkT.ToList();
                                    //var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                    //var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                    //var OBonusSalData = ca.BonusChkT.Where(e => e.BonusCalendar.FromDate.Value <= mstart && e.BonusCalendar.ToDate.Value >= mend).ToList();


                                    if (OBonusSalData != null && OBonusSalData.Count() != 0)
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

                                            foreach (var ca1 in OBonusSalData)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {



                                                    Fld1 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = GeoDataInd.FuncStruct_Job_Name != null ? GeoDataInd.FuncStruct_Job_Name : null,


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
                                                    Fld55 = ca1.TotalAmount_05.ToString(),

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

                    case "BONUSCHKTPROCESSREPORT":
                        var OBonusChkProData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OBonusChkProData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.BonusChkT)

                               //   .Include(e => e.BonusChkT.Select(r => r.BonusCalendar))
                                // .Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)

                                 //.Include(e => e.Employee.ServiceBookDates)
                                 .Where(e => e.Employee.Id == item)
                                 .AsNoTracking().FirstOrDefault();
                            if (OBonusChkProData_t != null)
                            {
                                OBonusChkProData_t.Employee.EmpName = db.NameSingle.Find(OBonusChkProData_t.Employee.EmpName_Id);
                                OBonusChkProData_t.BonusChkT = db.BonusChkT.Include(e => e.BonusCalendar).Where(e => e.BonusCalendar.FromDate.Value <= mFromDate && e.BonusCalendar.ToDate.Value >= mToDate && e.EmployeePayroll_Id == OBonusChkProData_t.Id).AsNoTracking().ToList();
                                OBonusChkProData.Add(OBonusChkProData_t);
                            }
                        }


                        if (OBonusChkProData == null || OBonusChkProData.Count() == 0)
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

                            foreach (var ca in OBonusChkProData)
                            {
                                if (ca.BonusChkT == null || ca.BonusChkT.Count() != 0)
                                {
                                    var OBonusSalData = ca.BonusChkT.ToList();
                                    //var mstart = mFromDate;//Convert.ToDateTime("01/" + (mFromDate.Take(1).SingleOrDefault().ToString())).Date;
                                    //var mend = mToDate;//Convert.ToDateTime("01/" + (mPayMonth.Take(m).SingleOrDefault().ToString())).Date;

                                    //var OBonusSalData = ca.BonusChkT.Where(e => e.BonusCalendar.FromDate.Value <= mstart && e.BonusCalendar.ToDate.Value >= mend).ToList();


                                    if (OBonusSalData != null && OBonusSalData.Count() != 0)
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

                                            foreach (var ca1 in OBonusSalData)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {



                                                    Fld1 = ca.Employee.Id == null ? "" : ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = GeoDataInd.FuncStruct_Job_Name != null ? GeoDataInd.FuncStruct_Job_Name : null,


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
                                                    Fld55 = ca1.TotalAmount_05.ToString(),

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
                                                    Fld14 = ca1.TotalAmount_12.ToString(),

                                                    Fld15 = ca1.TDSAmount == null ? "" : ca1.TDSAmount.ToString()
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
                                                    OGenericObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
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

                    case "GRATUITYT":
                        var OGratuityData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OGratuityData_temp = db.EmployeePayroll
                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.GratuityT)
                                // .Include(e => e.GratuityT.Select(t => t.GeoStruct))
                                // .Include(e => e.GratuityT.Select(t => t.FuncStruct))
                                // .Include(e => e.GratuityT.Select(t => t.PayStruct))
                                // .Include(e => e.Employee.ServiceBookDates)
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel().FirstOrDefault();

                            if (OGratuityData != null)
                            {
                                OGratuityData_temp.Employee.EmpName = db.NameSingle.Find(OGratuityData_temp.Employee.EmpName_Id);
                                OGratuityData_temp.GratuityT = db.GratuityT.Where(e => e.EmployeePayroll_Id == OGratuityData_temp.Id).AsNoTracking().ToList();
                                OGratuityData_temp.Employee.ServiceBookDates = db.ServiceBookDates.Find(OGratuityData_temp.Employee.ServiceBookDates_Id);
                                OGratuityData.Add(OGratuityData_temp);
                            }
                        }
                        if (OGratuityData == null || OGratuityData.Count() == 0)
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

                            foreach (var ca in OGratuityData)
                            {
                                if (ca.GratuityT == null || ca.GratuityT.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                    //int geoid = ca.GratuityT.FirstOrDefault().GeoStruct.Id;

                                    //int payid = ca.GratuityT.FirstOrDefault().PayStruct.Id;

                                    //int funid = ca.GratuityT.FirstOrDefault().FuncStruct.Id;

                                    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

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

                                                // Fld29 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null ? ca.Employee.GeoStruct.Location.Id.ToString() : null,
                                                // Fld30 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocCode : null,
                                                //  Fld31 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : null,

                                                Fld31 = GeoDataInd == null ? "" : GeoDataInd.GeoStruct_Location_Name,

                                                //  Fld32 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.Id.ToString() : null,
                                                // Fld33 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode.ToString() : null,
                                                //Fld34 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                                                Fld34 = GeoDataInd != null && GeoDataInd.GeoStruct_Department_Name != null ? GeoDataInd.GeoStruct_Department_Name : null,

                                                //  Fld35 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Id.ToString() : null,
                                                // Fld36 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Code.ToString() : null,
                                                //  Fld37 = ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                Fld37 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,

                                                // Fld38 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Id.ToString() : null,
                                                //  Fld39 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Code : null,
                                                // Fld40 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : null,
                                                Fld40 = GeoDataInd != null && GeoDataInd.FuncStruct_Job_Name != null ? GeoDataInd.FuncStruct_Job_Name : null,

                                                Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                                Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                                Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),

                                            };
                                            //if (month)
                                            //{
                                            //    OGenericObjStatement.Fld100 = ca.PayMonth.ToString();
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



                    case "GRATUITYTDETAILS":
                        var OGratuityDatadet = new List<EmployeePayroll>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OGratuityData_temp = db.EmployeePayroll
                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.GratuityT)
                                // .Include(e => e.GratuityT.Select(t => t.GeoStruct))
                                // .Include(e => e.GratuityT.Select(t => t.FuncStruct))
                                // .Include(e => e.GratuityT.Select(t => t.PayStruct))
                                // .Include(e => e.Employee.ServiceBookDates)
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel().FirstOrDefault();

                            if (OGratuityDatadet != null)
                            {
                                OGratuityData_temp.Employee.EmpName = db.NameSingle.Find(OGratuityData_temp.Employee.EmpName_Id);
                                OGratuityData_temp.GratuityT = db.GratuityT.Where(e => e.EmployeePayroll_Id == OGratuityData_temp.Id).AsNoTracking().ToList();
                                OGratuityData_temp.Employee.ServiceBookDates = db.ServiceBookDates.Find(OGratuityData_temp.Employee.ServiceBookDates_Id);
                                OGratuityData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == OGratuityData_temp.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();

                                OGratuityDatadet.Add(OGratuityData_temp);
                            }
                        }
                        if (OGratuityDatadet == null || OGratuityDatadet.Count() == 0)
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

                            foreach (var ca in OGratuityDatadet)
                            {
                                if (ca.GratuityT == null || ca.GratuityT.Count() != 0)
                                {
                                    int? geoid = ca.Employee.GeoStruct_Id;

                                    int? payid = ca.Employee.PayStruct_Id;

                                    int? funid = ca.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                    var OGratuityDatails = ca.GratuityT.ToList();

                                    if (OGratuityDatails != null && OGratuityDatails.Count() != 0)
                                    {


                                        foreach (var ca1 in OGratuityDatails)
                                        {
                                            DateTime mTillDate;
                                            mTillDate = ca1.ProcessDate.Value.Date;
                                            var OCompanyPayrollGr = db.CompanyPayroll
                                    .Include(e => e.GratuityAct)
                                    .Include(e => e.GratuityAct.Select(r => r.GratuityWages))
                                    .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster))
                                    .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster.Select(q => q.SalHead)))
                                    .Where(e => e.Id == CompanyPayrollId).SingleOrDefault();

                                            var OGratuityAct = OCompanyPayrollGr.GratuityAct
                                  .Where(e => e.EndDate == null).SingleOrDefault();

                                            var OEmployeePayroll = db.EmployeePayroll
                                                          .Include(e => e.Employee)
                                                                .Include(e => e.Employee.ServiceBookDates)
                                                                .Include(e => e.EmpSalStruct)
                                                                .Include(e => e.GratuityT)
                                                                .Where(e => e.Id == ca1.EmployeePayroll_Id).SingleOrDefault();

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
                                            var mGratuityWages = SalaryHeadGenProcess.WagecalcDirect(OGratuityAct.GratuityWages, null, OEmpSalDetails);


                                            DateTime? mEffectiveDate;
                                            if (OGratuityAct.IsDateOfJoin == true)
                                            {
                                                if (OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value == null)
                                                {
                                                    continue;
                                                }
                                                mEffectiveDate = OEmployeePayroll.Employee.ServiceBookDates.JoiningDate.Value;
                                            }
                                            else if (OGratuityAct.IsDateOfConfirm == true)
                                            {
                                                if (OEmployeePayroll.Employee.ServiceBookDates.BirthDate.Value == null)
                                                {
                                                    continue;
                                                }
                                                mEffectiveDate = OEmployeePayroll.Employee.ServiceBookDates.ConfirmationDate.Value;
                                            }

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

                                            int yearsc = end.Year - start.Year;
                                            int monthsc = end.Month - start.Month;
                                            int daysc = end.Day - start.Day;

                                            if (daysc < 0)
                                            {
                                                monthsc--;
                                                daysc += DateTime.DaysInMonth(end.Year, end.Month);
                                            }

                                            if (monthsc < 0)
                                            {
                                                yearsc--;
                                                monthsc += 12;
                                            }

                                            
                                            var grat = db.GratuityAct
                                .Include(e => e.GratuityWages)
                                .Include(e => e.GratuityWages.RateMaster)
                                 .Include(e => e.GratuityWages.RateMaster.Select(x => x.SalHead))
                                .AsNoTracking().ToList();
                                           // string formula = " ( " + " ( " + mGratuityWages + " * " + OGratuityAct.PayableDays + " ) " + " / " + OGratuityAct.MonthDays + " ) " + " ) " + " * " + mTotEffectiveService + " = " + ca1.Amount;
                                            string formula = " ( " + " ( " + mGratuityWages + " / " + OGratuityAct.MonthDays + " ) " + " * " + OGratuityAct.PayableDays + " ) " + " ) " + " * " + mTotEffectiveService + " = " + ca1.Amount;
                                            string amtword = NumToWords.ConvertAmount(ca1.Amount);
                                            if (OEmpSalStruct != null)
                                            {
                                                foreach (var g in grat)
                                                {
                                                    var grsalhead = g.GratuityWages.RateMaster.Select(z => z.SalHead).ToList();
                                                    double amount = 0;
                                                    foreach (var salid in grsalhead)
                                                    {
                                                        amount = db.EmpSalStructDetails.Where(a => a.SalaryHead.Id == salid.Id && a.EmpSalStruct_Id == OEmpSalStruct.Id).Select(a => a.Amount).FirstOrDefault();


                                                        if (amount != 0)
                                                        {


                                                            //var Bal = empsalstructdata.EmpSalStructDetails.Where(a => a.SalaryHead.Code.ToUpper() == "BASIC").Select(a => a.Amount).SingleOrDefault();
                                                            //var DA = empsalstructdata.EmpSalStructDetails.Where(a => a.SalaryHead.Code.ToUpper() == "DA").Select(a => a.Amount).SingleOrDefault();
                                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                            {
                                                                Fld1 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                                                Fld2 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode.ToString(),
                                                                Fld3 = ca.Employee.ServiceBookDates.BirthDate == null ? "" : ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                                                Fld4 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                                                Fld15 = salid == null ? "" : salid.Code.ToString(),
                                                                Fld16 = amount == null ? "" : amount.ToString(),
                                                                Fld7 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                                                Fld8 = ca.Employee.EmpOffInfo.NationalityID == null ? "" : ca.Employee.EmpOffInfo.NationalityID.No1 == null ? "" : ca.Employee.EmpOffInfo.NationalityID.No1.ToString(),
                                                                Fld20 = yearsc.ToString(),
                                                                Fld21 = monthsc.ToString(),
                                                                Fld22 = daysc.ToString(),
                                                                Fld23 = formula,
                                                                Fld24 = ca1.Amount.ToString(),
                                                                Fld25 = ca1.ProcessDate.Value.Date.ToShortDateString(),
                                                                Fld26 = amtword,
                                                                Fld28 = ca.Employee.ServiceBookDates.ServiceLastDate == null ? "" : ca.Employee.ServiceBookDates.ServiceLastDate.Value.ToShortDateString(),
                                                                Fld29 = GeoDataInd.PayStruct_JobStatus_EmpActingStatus,
                                                                Fld30 = mTotEffectiveService.ToString(),
                                                                Fld31 = ca1.TotalLWP.ToString(),

                                                            };
                                                            //if (month)
                                                            //{
                                                            //    OGenericGeoObjStatement.Fld100 = ca.PayMonth.ToString();
                                                            //}
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
                                                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                                        }
                                                    }
                                                }
                                            }



                                        }
                                    }

                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "GRATUITYPREMIUMREPORT":
                        List<EmployeePayroll> OPayrolldata = new List<EmployeePayroll>();

                        var paymonth1 = mPayMonth.FirstOrDefault();

                        var Paydays = paymonth1.Split('/');

                        var days = DateTime.DaysInMonth(Convert.ToInt32(Paydays[1]), Convert.ToInt32(Paydays[0]));

                        var Effectivedate = Convert.ToDateTime("01/" + paymonth1).Date;

                        var Enddate = Convert.ToDateTime(days + "/" + paymonth1).Date;

                        foreach (var item in EmpPayrollIdList)
                        {
                            EmployeePayroll OPayrolldata_temp = db.EmployeePayroll
                                .Include(e => e.Employee)
                                //.Include(a => a.SalaryT.Select(e => e.SalEarnDedT))
                                //.Include(a => a.EmpSalStruct)
                                //.Include(a => a.EmpSalStruct.Select(q => q.EmpSalStructDetails))
                                //.Include(a => a.EmpSalStruct.Select(q => q.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                //.Include(a => a.SalaryT.Select(e => e.SalEarnDedT.Select(b => b.SalaryHead)))
                                //.Include(a => a.Employee.EmpName)
                                //.Include(a => a.Employee.GeoStruct)
                                //.Include(a => a.Employee.FuncStruct)
                                //.Include(a => a.Employee.PayStruct)
                                //.Include(a => a.Employee.ServiceBookDates)
                                //.Include(a => a.Employee.EmpOffInfo)
                                // .Include(a => a.Employee.EmpOffInfo.NationalityID)
                                .Where(e => e.Employee.Id == item).AsNoTracking()
                                 .FirstOrDefault();

                            if (OPayrolldata_temp != null)
                            {
                                OPayrolldata_temp.Employee.EmpName = db.NameSingle.Find(OPayrolldata_temp.Employee.EmpName_Id);
                                OPayrolldata_temp.Employee.ServiceBookDates = db.ServiceBookDates.Find(OPayrolldata_temp.Employee.ServiceBookDates_Id);
                                OPayrolldata_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == OPayrolldata_temp.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                                OPayrolldata_temp.EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OPayrolldata_temp.Id
                                    && e.EffectiveDate.Value >= Effectivedate && e.EffectiveDate.Value <= Enddate).OrderBy(e => e.Id).AsNoTracking().ToList();
                                foreach (var item2 in OPayrolldata_temp.EmpSalStruct)
                                {
                                    item2.EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == item2.Id).AsNoTracking().ToList();
                                    foreach (var item3 in item2.EmpSalStructDetails)
                                    {
                                        item3.SalaryHead = db.SalaryHead.Find(item3.SalaryHead_Id);
                                    }

                                }

                                OPayrolldata.Add(OPayrolldata_temp);
                            }
                        }
                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OPayrolldata == null || OPayrolldata.Count() == 0)
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



                            foreach (var GA in OPayrolldata)
                            {
                                int? geoid = GA.Employee.GeoStruct_Id;

                                int? payid = GA.Employee.PayStruct_Id;

                                int? funid = GA.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                //int geoid = GA.Employee.GeoStruct.Id;

                                //int payid = GA.Employee.PayStruct.Id;

                                //int funid = GA.Employee.FuncStruct.Id;

                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                //var paymonth1 = mPayMonth.FirstOrDefault();

                                //var Paydays = paymonth1.Split('/');

                                //var days = DateTime.DaysInMonth(Convert.ToInt32(Paydays[1]), Convert.ToInt32(Paydays[0]));

                                var empsalstructdata = GA.EmpSalStruct.LastOrDefault();
                                var grat = db.GratuityAct
                                    .Include(e => e.GratuityWages)
                                    .Include(e => e.GratuityWages.RateMaster)
                                     .Include(e => e.GratuityWages.RateMaster.Select(x => x.SalHead))
                                    .AsNoTracking().ToList();

                                if (empsalstructdata != null)
                                {
                                    foreach (var g in grat)
                                    {
                                        var grsalhead = g.GratuityWages.RateMaster.Select(z => z.SalHead).ToList();
                                        double amount = 0;
                                        foreach (var salid in grsalhead)
                                        {
                                            amount = db.EmpSalStructDetails.Where(a => a.SalaryHead.Id == salid.Id && a.EmpSalStruct_Id == empsalstructdata.Id).Select(a => a.Amount).FirstOrDefault();



                                            //var Bal = empsalstructdata.EmpSalStructDetails.Where(a => a.SalaryHead.Code.ToUpper() == "BASIC").Select(a => a.Amount).SingleOrDefault();
                                            //var DA = empsalstructdata.EmpSalStructDetails.Where(a => a.SalaryHead.Code.ToUpper() == "DA").Select(a => a.Amount).SingleOrDefault();
                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                            {
                                                Fld1 = GA.Employee.EmpName.FullNameFML == null ? "" : GA.Employee.EmpName.FullNameFML.ToString(),
                                                Fld2 = GA.Employee.EmpCode == null ? "" : GA.Employee.EmpCode.ToString(),
                                                Fld3 = GA.Employee.ServiceBookDates.BirthDate == null ? "" : GA.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                                Fld4 = GA.Employee.ServiceBookDates.JoiningDate == null ? "" : GA.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                                Fld15 = salid == null ? "" : salid.Code.ToString(),
                                                Fld16 = amount == null ? "" : amount.ToString(),
                                                Fld7 = GA.Employee.ServiceBookDates.RetirementDate == null ? "" : GA.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                                Fld8 = GA.Employee.EmpOffInfo.NationalityID == null ? "" : GA.Employee.EmpOffInfo.NationalityID.No1 == null ? "" : GA.Employee.EmpOffInfo.NationalityID.No1.ToString(),
                                            };
                                            //if (month)
                                            //{
                                            //    OGenericGeoObjStatement.Fld100 = ca.PayMonth.ToString();
                                            //}
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
                                                OGenericGeoObjStatement.Fld88 = GA.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;




                    case "PTAXSTATEMENTSLABWISE":


                        //var OPTaxSlabData = db.CompanyPayroll
                        //.Include(e => e.PTaxMaster)
                        //.Include(e => e.PTaxMaster.Select(r => r.Frequency))
                        //.Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths))
                        //.Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(t => t.EffectiveMonth)))
                        //.Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(t => t.Gender)))
                        //.Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange)))
                        //.Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster))
                        //.Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster))
                        // .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster.Select(t => t.SalHead)))
                        //.Include(e => e.PTaxMaster.Select(r => r.States))
                        //.Where(d => d.Id == CompanyPayrollId).AsNoTracking().SingleOrDefault();

                        string m_PayMonth = mPayMonth.FirstOrDefault();
                        var OPTaxSlab_Data = db.PTaxMaster.Select(r => new
                        {
                            StatesCode = r.States.Code,
                            PTStatutoryEffectiveMonths = r.PTStatutoryEffectiveMonths.Select(d => new
                            {
                                GenderLookupVal = d.Gender.LookupVal,
                                EffectiveMonthLookupVal = d.EffectiveMonth.LookupVal,
                                StatutoryWageRange = d.StatutoryWageRange.Select(z => new
                                {
                                    RangeFrom = z.RangeFrom,
                                    RangeTo = z.RangeTo
                                }).ToList()
                            }).ToList()
                        }).ToList();

                        var OTOTEmpdedData_Temp = db.EmployeePayroll.Select(d => new
                        {
                            EmployeeId = d.Employee.Id,
                            EmpGender = d.Employee.Gender.LookupVal,
                            GeoStructId = d.Employee.GeoStruct.Id,
                            PayStructId = d.Employee.PayStruct.Id,
                            FuncStructId = d.Employee.FuncStruct.Id,
                            EmpCode = d.Employee.EmpCode,
                            EmpName = d.Employee.EmpName.FullNameFML,
                            LocDesc = d.Employee.GeoStruct.Location.LocationObj.LocDesc,
                            SalaryT = d.SalaryT.Select(r => new
                            {
                                PayMonth = r.PayMonth,
                                SalaytStateCode = r.Geostruct.Location.Address.State.Code,
                                TotalEarning = r.TotalEarning == null ? "0" : r.TotalEarning.ToString(),
                                State = r.PTaxTransT.State,
                                PTAmount = r.PTaxTransT.PTAmount == null ? "0" : r.PTaxTransT.PTAmount.ToString(),
                                ArrearPTAmount = r.PTaxTransT.ArrearPTAmount == null ? "0" : r.PTaxTransT.ArrearPTAmount.ToString()
                            }).ToList()

                        }).Where(e => EmpPayrollIdList.Contains(e.EmployeeId)).ToList();


                        if (OPTaxSlab_Data == null || OPTaxSlab_Data.Count() == 0)
                        {

                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            //var state = false;
                            var slab = false;

                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "slabwise")
                                {
                                    slab = true;
                                }
                                //if (item1 == "statewise")
                                //{
                                //    state = true;
                                //}
                            }

                            var month = false;
                            //var emp = false;
                            var dept = false;
                            var loca = false;
                            var comp = false;
                            var grp = false;
                            var unit = false;
                            var div = false;
                            //var regn = false;
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
                                //if (item.LookupVal.ToUpper() == "EMPLOYEE")
                                //{
                                //    emp = true;
                                //}
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
                                //if (item.LookupVal.ToUpper() == "REGION")
                                //{
                                //    regn = true;
                                //}
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


                            foreach (var ca in OPTaxSlab_Data)
                            {
                                foreach (var ca1 in ca.PTStatutoryEffectiveMonths.Where(x => x.EffectiveMonthLookupVal == Convert.ToDateTime("01/" + m_PayMonth).ToString("MMMM")))
                                {

                                    foreach (var otm in OTOTEmpdedData_Temp)
                                    {
                                        foreach (var singlemonth in mPayMonth)
                                        {
                                            var OSalaryTData = otm.SalaryT.Where(q => q.PayMonth == singlemonth && otm.EmpGender.ToUpper() == ca1.GenderLookupVal.ToUpper()).SingleOrDefault();

                                            if (OSalaryTData != null)
                                            {

                                                int? geoid = otm.GeoStructId;

                                                int? payid = otm.PayStructId;

                                                int? funid = otm.FuncStructId;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                                foreach (var ca2 in ca1.StatutoryWageRange)
                                                {

                                                    string checkrangefromto = ca2.RangeFrom + " TO " + ca2.RangeTo;

                                                    if (Convert.ToDouble(OSalaryTData.TotalEarning) >= ca2.RangeFrom && Convert.ToDouble(OSalaryTData.TotalEarning) <= ca2.RangeTo && ca.StatesCode == OSalaryTData.SalaytStateCode)
                                                    {

                                                        string AMT = OSalaryTData.PTAmount;
                                                        string ArrAMT = OSalaryTData.ArrearPTAmount;
                                                        Double totpt = Convert.ToDouble(AMT) + Convert.ToDouble(ArrAMT);
                                                        string sta = OSalaryTData.TotalEarning;

                                                        if (salheadlist.Count() != 0)
                                                        {
                                                            foreach (var item in salheadlist)
                                                            {
                                                                if (checkrangefromto == item)
                                                                {
                                                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                                    {

                                                                        Fld1 = otm.EmpCode == null ? "" : otm.EmpCode,
                                                                        Fld2 = otm.EmpName == null ? "" : otm.EmpName,
                                                                        Fld3 = sta,
                                                                        Fld4 = AMT,
                                                                        Fld10 = ArrAMT,
                                                                        Fld11 = totpt.ToString(),
                                                                        Fld5 = otm.LocDesc == null ? "" : otm.LocDesc,
                                                                        Fld6 = OSalaryTData.PayMonth == null ? "" : OSalaryTData.PayMonth,
                                                                        Fld7 = ca2.RangeFrom.ToString(),
                                                                        Fld8 = ca2.RangeTo.ToString()

                                                                    };

                                                                    if (month)
                                                                    {
                                                                        OGenericGeoObjStatement.Fld100 = OSalaryTData.PayMonth.ToString();
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
                                                                    //if (emp)
                                                                    //{
                                                                    //    OGenericGeoObjStatement.Fld88 = otm.EmpName;
                                                                    //}
                                                                    if (slab)
                                                                    {
                                                                        OGenericGeoObjStatement.Fld87 = checkrangefromto;
                                                                    }
                                                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);

                                                                }
                                                            }
                                                        }

                                                        else
                                                        {
                                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                            {

                                                                Fld1 = otm.EmpCode == null ? "" : otm.EmpCode,
                                                                Fld2 = otm.EmpName == null ? "" : otm.EmpName,
                                                                Fld3 = sta,
                                                                Fld4 = AMT,
                                                                Fld10 = ArrAMT,
                                                                Fld11 = totpt.ToString(),
                                                                Fld5 = otm.LocDesc == null ? "" : otm.LocDesc,
                                                                Fld6 = OSalaryTData.PayMonth == null ? "" : OSalaryTData.PayMonth,
                                                                Fld7 = ca2.RangeFrom.ToString(),
                                                                Fld8 = ca2.RangeTo.ToString()

                                                            };

                                                            if (month)
                                                            {
                                                                OGenericGeoObjStatement.Fld100 = OSalaryTData.PayMonth.ToString();
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
                                                            //if (emp)
                                                            //{
                                                            //    OGenericGeoObjStatement.Fld88 = otm.EmpName;
                                                            //}
                                                            if (slab)
                                                            {
                                                                OGenericGeoObjStatement.Fld87 = checkrangefromto;
                                                            }
                                                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);

                                                        }
                                                    }
                                                }

                                            }
                                        }

                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "NEGATIVESALARY":

                        var Pmnt = mPayMonth.FirstOrDefault();

                        var Onegativesal = new List<EmployeePayroll>();

                        foreach (var item in EmpPayrollIdList)
                        {

                            var NegSalData = db.EmployeePayroll
                                .Include(a => a.Employee)
                                //.Include(a => a.Employee.EmpName)
                                //.Include(a => a.SalaryT)
                                .Where(e => e.Id == item).FirstOrDefault();

                            if (NegSalData != null)
                            {
                                NegSalData.Employee.EmpName = db.NameSingle.Where(e => e.Id == NegSalData.Employee.EmpName_Id).FirstOrDefault();
                                NegSalData.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == NegSalData.Id && e.PayMonth == Pmnt).ToList();
                            }
                            Onegativesal.Add(NegSalData);
                        }


                        if (Onegativesal == null)
                        {
                            return null;
                        }
                        else
                        {
                            var negsal = db.NegSalAct.FirstOrDefault();
                            if (negsal == null)
                            {
                                return null;
                            }

                            foreach (var ca in Onegativesal)
                            {
                                var Sal = ca.SalaryT.SingleOrDefault();
                                if (Sal != null)
                                {


                                    // double salperc = 100 - negsal.SalPercentage;

                                    double perc = (Sal.TotalEarning * negsal.SalPercentage) / 100;
                                    double excess = 0;
                                    double excedd = 0;
                                    excess = Sal.TotalNet - perc;

                                    //if (Sal.TotalDeduction > perc)
                                    //{
                                    //    excess = Sal.TotalEarning - perc;
                                    //}
                                    if (excess < 0)
                                    {
                                        excedd = Math.Round(excess - 0.001, 0);
                                    }
                                    else
                                    {
                                        excedd = Math.Round(excess + 0.001, 0);
                                    }

                                    if (excedd < 0)
                                    {

                                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                        {

                                            Fld1 = ca.Employee.EmpCode,
                                            Fld2 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                            Fld3 = Sal.TotalEarning == null ? "" : Sal.TotalEarning.ToString(),
                                            Fld4 = Sal.TotalNet == null ? "" : Sal.TotalNet.ToString(),
                                            Fld5 = Sal.TotalDeduction == null ? "" : Sal.TotalDeduction.ToString(),
                                            Fld6 = excedd.ToString()
                                        };
                                        //write data to generic class
                                        OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;

                        }

                        break;

                    ////NEW 23/08/2019
                    case "NEGATIVESALARYNEW":
                        string Paymnt = mPayMonth.FirstOrDefault();

                        var NegSal = new List<SalaryT>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var NegSal_temp1 = db.EmployeePayroll
                                //.Include(a => a.Employee)
                                //.Include(a => a.Employee.EmpName)
                                //.Include(a => a.SalaryT)
                                //.Include(a => a.SalaryT.Select(q => q.SalEarnDedT))
                                //.Include(a => a.SalaryT.Select(q => q.SalEarnDedT.Select(z => z.SalaryHead)))
                                //.Include(a => a.SalaryT.Select(q => q.SalEarnDedT.Select(z => z.NegSalData)))
                            .Where(e => e.Employee.Id == item).AsNoTracking().FirstOrDefault().Id;

                            SalaryT NegSal_temp = db.SalaryT
                                //.Include(a => a.SalEarnDedT)
                                //.Include(a => a.SalEarnDedT.Select(q => q.SalaryHead))
                                //.Include(a => a.SalEarnDedT.Select(q => q.NegSalData))
                                //.Include(e => e.EmployeePayroll)
                                //.Include(e => e.EmployeePayroll.Employee)
                                //.Include(e => e.EmployeePayroll.Employee.EmpName).AsNoTracking()
                                //.Where(e => e.PayMonth == Paymnt && e.EmployeePayroll_Id == NegSal_temp1).FirstOrDefault();
                                .Where(e => e.EmployeePayroll_Id == NegSal_temp1 && e.PayMonth == Paymnt).AsNoTracking().FirstOrDefault();

                            if (NegSal_temp != null)
                            {
                                NegSal_temp.SalEarnDedT = db.SalEarnDedT.Include(e => e.NegSalData).Where(x => x.SalaryT_Id == NegSal_temp.Id).ToList();
                                //NegSal_temp.SalEarnDedT = db.SalaryT.Include(e=>e.SalEarnDedT).Include(e=>e.SalEarnDedT.Select(r=>r.NegSalData)).Where(e => e.Id == NegSal_temp.Id).Select(r => r.SalEarnDedT.ToList()).AsNoTracking().FirstOrDefault();
                                NegSal_temp.EmployeePayroll = db.EmployeePayroll.Where(e => e.Id == NegSal_temp.EmployeePayroll_Id).AsNoTracking().FirstOrDefault();
                                NegSal_temp.EmployeePayroll.Employee = db.Employee.Where(e => e.Id == NegSal_temp.EmployeePayroll.Employee_Id).AsNoTracking().FirstOrDefault();
                                NegSal_temp.EmployeePayroll.Employee.EmpName = db.NameSingle.Where(e => e.Id == NegSal_temp.EmployeePayroll.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();
                                foreach (var i in NegSal_temp.SalEarnDedT)
                                {
                                    i.SalaryHead = db.SalaryHead.Where(e => e.Id == i.SalaryHead_Id).AsNoTracking().FirstOrDefault();
                                    i.NegSalData = db.NegSalData.Where(e => e.Id == i.NegSalData_Id).AsNoTracking().FirstOrDefault();
                                }
                                NegSal.Add(NegSal_temp);
                            }
                        }


                        if (NegSal == null || NegSal.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in NegSal)
                            {
                                //var sal = ca.SalaryT.ToList();
                                ////
                                //foreach (var ca2 in sal)
                                //{
                                var neg = ca.SalEarnDedT.ToList();

                                foreach (var ca1 in neg)
                                {
                                    if (ca1.NegSalData != null)
                                    {
                                        if (ca1.NegSalData.Amount != 0 || ca1.NegSalData.DiffAmount != 0 || ca1.NegSalData.StdAmount != 0)
                                        {

                                            // NegSal_temp1.where
                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                            {

                                                Fld1 = ca.EmployeePayroll.Employee.EmpCode,
                                                Fld2 = ca.EmployeePayroll.Employee.EmpName == null ? "" : ca.EmployeePayroll.Employee.EmpName.FullNameFML.ToString(),
                                                Fld3 = ca1.NegSalData.StdAmount.ToString(),
                                                Fld4 = ca1.NegSalData.DiffAmount.ToString(),
                                                Fld5 = ca1.NegSalData.Amount.ToString(),
                                                Fld6 = ca1.SalaryHead.Name.ToString(),
                                            };
                                            //write data to generic class
                                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    ////NEW 23/08/2019
                    case "SUSPENDEDEMPLOYEE":
                        var mnt = mPayMonth.FirstOrDefault();
                        var SuspendEmp = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var SuspendEmp_temp = db.EmployeePayroll
                                .Include(a => a.Employee)

                            //.Include(a => a.Employee.EmpName)
                                //.Include(a => a.Employee.PayStruct)
                                //.Include(a => a.Employee.PayStruct.JobStatus)
                                //.Include(a => a.SalaryT)


                         //.Where(e => e.Id == item && e.SalaryT.Any(a => a.PayMonth == mnt.ToString()) && e.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "SUSPEND"
                                //       || e.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "SUSPENDED").AsNoTracking()
                                //                         .FirstOrDefault();
                         .Where(e => e.Id == item).AsNoTracking().FirstOrDefault();


                            if (SuspendEmp_temp != null)
                            {
                                SuspendEmp_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == SuspendEmp_temp.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();
                                SuspendEmp_temp.Employee.PayStruct = db.PayStruct.Where(e => e.Id == SuspendEmp_temp.Employee.PayStruct_Id).AsNoTracking().FirstOrDefault();
                                SuspendEmp_temp.Employee.PayStruct.JobStatus = db.PayStruct.Where(e => e.Id == SuspendEmp_temp.Employee.PayStruct_Id).Select(r => r.JobStatus).AsNoTracking().FirstOrDefault();
                                SuspendEmp_temp.Employee.PayStruct.JobStatus.EmpActingStatus = db.JobStatus.Where(e => e.Id == SuspendEmp_temp.Employee.PayStruct.JobStatus_Id).Select(r => r.EmpActingStatus).AsNoTracking().FirstOrDefault();
                                SuspendEmp_temp.Employee.PayStruct.JobStatus.EmpActingStatus = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "SUSPEND" || e.LookupVal.ToUpper() == "SUSPENDED").AsNoTracking().FirstOrDefault();
                                SuspendEmp_temp.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == SuspendEmp_temp.Id && e.PayMonth == mnt.ToString()).AsNoTracking().ToList();
                                SuspendEmp.Add(SuspendEmp_temp);
                            }
                        }
                        if (SuspendEmp == null || SuspendEmp.Count() == 0)
                        {

                            return null;
                        }
                        else
                        {

                            foreach (var ca in SuspendEmp)
                            {
                                var SuspDetails = ca.SalaryT.LastOrDefault();

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Employee == null ? "" : ca.Employee.EmpCode.ToString(),
                                    Fld2 = ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                    Fld3 = SuspDetails == null ? "0 " : SuspDetails.TotalEarning.ToString(),
                                    Fld4 = SuspDetails == null ? "0 " : SuspDetails.TotalDeduction.ToString(),
                                    Fld5 = SuspDetails == null ? "0 " : SuspDetails.TotalNet.ToString(),

                                };

                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);

                            }
                            return OGenericPayrollStatement;

                        }

                        break;


                    case "LWFSTATEMENT":
                        var OLWFSTATData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var LWFSTATEMENTS = db.EmployeePayroll
                                //.Include(e => e.SalaryT)
                                //.Include(e => e.SalaryT.Select(t => t.LWFTransT))
                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.EmpOffInfo)
                                //.Include(e => e.Employee.EmpOffInfo.Bank)
                                //.Include(e => e.Employee.EmpOffInfo.Branch)
                                //.Include(e => e.Employee.EmpOffInfo.AccountType)
                                //.Include(e => e.SalaryT.Select(r => r.Geostruct))
                                //.Include(e => e.SalaryT.Select(r => r.PayStruct))
                                //.Include(e => e.SalaryT.Select(r => r.FuncStruct))
                               .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();

                            if (LWFSTATEMENTS != null)
                            {
                                LWFSTATEMENTS.Employee.EmpName = db.NameSingle.Find(LWFSTATEMENTS.Employee.EmpName_Id);
                                LWFSTATEMENTS.Employee.EmpOffInfo = db.EmpOff.Where(e => e.Id == LWFSTATEMENTS.Employee.EmpOffInfo_Id).AsNoTracking().FirstOrDefault();
                                LWFSTATEMENTS.Employee.EmpOffInfo.AccountType = db.LookupValue.Find(LWFSTATEMENTS.Employee.EmpOffInfo.AccountType_Id);
                                LWFSTATEMENTS.Employee.EmpOffInfo.Branch = db.Branch.Find(LWFSTATEMENTS.Employee.EmpOffInfo.Branch_Id);
                                LWFSTATEMENTS.Employee.EmpOffInfo.Bank = db.Bank.Find(LWFSTATEMENTS.Employee.EmpOffInfo.Bank_Id);
                                LWFSTATEMENTS.SalaryT = db.SalaryT.Where(e => mPayMonth.Contains(e.PayMonth) && e.EmployeePayroll_Id == LWFSTATEMENTS.Id).AsNoTracking().ToList();
                                foreach (var item1 in LWFSTATEMENTS.SalaryT)
                                {
                                    item1.LWFTransT = db.LWFTransT.Find(item1.LWFTransT_Id);
                                }
                                OLWFSTATData.Add(LWFSTATEMENTS);
                            }

                        }
                        if (OLWFSTATData == null)
                            return null;
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

                            foreach (var ca in OLWFSTATData)
                            {

                                int? geoid = ca.Employee.GeoStruct_Id;

                                int? payid = ca.Employee.PayStruct_Id;

                                int? funid = ca.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                //int geoid = l.SalaryT.FirstOrDefault().Geostruct.Id;

                                //int payid = l.SalaryT.FirstOrDefault().PayStruct.Id;

                                //int funid = l.SalaryT.FirstOrDefault().FuncStruct.Id;

                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (OLWFSTATData != null && OLWFSTATData.Count() > 0)
                                {

                                    SalaryT salmonth = ca.SalaryT.FirstOrDefault();

                                    if (salmonth != null && salmonth.LWFTransT != null)
                                    {

                                        //List<SalaryT> empall = l.SalaryT.Where(r => mPayMonth.Contains(r.PayMonth)).ToList();

                                        //foreach (var ls in empall)
                                        //{
                                        GenericField100 OGenericObj = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Employee.Id.ToString() == null ? "" : ca.Employee.Id.ToString(),
                                            Fld23 = ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                            Fld24 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                            //  Fld20 = salmonth.PayStruct != null && salmonth.PayStruct.Grade != null ? salmonth.PayStruct.Grade.Name : null,
                                            Fld20 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                            Fld32 = salmonth.LWFTransT.EmpAmt.ToString() == null ? "" : salmonth.LWFTransT.EmpAmt.ToString(),
                                            Fld33 = salmonth.LWFTransT.CompAmt.ToString() == null ? "" : salmonth.LWFTransT.CompAmt.ToString(),
                                            Fld25 = salmonth.PayMonth.ToString() == null ? "" : salmonth.PayMonth.ToString(),
                                            //Fld26 = salmonth.Geostruct.Location != null && salmonth.Geostruct.Location.Address != null && salmonth.Geostruct.Location.Address.State != null ? salmonth.Geostruct.Location.Address.State.Name : "", 
                                            Fld26 = GeoDataInd.GeoStruct_Location_State_Name == null ? "" : GeoDataInd.GeoStruct_Location_State_Name,
                                            //Fld54 = ca2.SalaryHead.Name.ToString(),
                                            //Fld55 = ca2.SalaryHead.Type.LookupVal.ToUpper().ToString(),
                                            //Fld56 = ca2.SalaryHead.Frequency.LookupVal.ToUpper().ToString(),
                                            //Fld57 = ca2.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString(),
                                            //Fld58 = ca2.Amount == null ? "" : ca2.Amount.ToString(),
                                            //Fld9 = salmonth != null && salmonth.Geostruct.Location != null && salmonth.Geostruct.Location.LocationObj != null ? salmonth.Geostruct.Location.LocationObj.LocDesc : "",
                                            Fld9 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name
                                            //Fld19 = ca9 != null && ca9.Department != null && ca9.Department.DepartmentObj != null ? ca9.Department.DepartmentObj.DeptDesc : "",
                                            //Fld34 = ca8 != null && ca8.Grade != null ? ca8.Grade.Name : "",
                                        };
                                        //if (month)
                                        //{
                                        //    OGenericObj.Fld100 = ca.PayMonth.ToString();
                                        //}
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
                                            OGenericObj.Fld91 = GeoDataInd.FuncStruct_JobPosition_Id.ToString();
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
                                            OGenericObj.Fld88 = ca.Employee.EmpName.FullNameFML;
                                        }
                                        OGenericPayrollStatement.Add(OGenericObj);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "ESICSTATEMENT":

                        var ESICSTATEMENTS = db.EmployeePayroll
                         .Include(e => e.SalaryT)
                         .Include(e => e.SalaryT.Select(t => t.ESICTransT))
                         .Include(e => e.Employee)
                         .Include(e => e.Employee.EmpName)
                         .Include(e => e.Employee.EmpOffInfo)
                         .Include(e => e.Employee.EmpOffInfo.Bank)
                         .Include(e => e.Employee.EmpOffInfo.Branch)
                         .Include(e => e.Employee.EmpOffInfo.AccountType)
                         .Include(e => e.SalaryT.Select(r => r.Geostruct.Region))
                         .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.LocationObj))
                         .Include(e => e.SalaryT.Select(r => r.Geostruct.Location.Address.State))
                            // .Include(e => e.SalaryT.Select(r => r.Geostruct.Department.DepartmentObj))
                            //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job))
                            //.Include(e => e.SalaryT.Select(r => r.FuncStruct.Job.JobPosition))
                         .Include(e => e.SalaryT.Select(r => r.PayStruct.Grade))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                            //.Include(e => e.SalaryT.Select(r => r.PayStruct.Grade.Levels))
                         .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus))
                         .Include(e => e.SalaryT.Select(r => r.PayStruct.JobStatus.EmpActingStatus)).AsNoTracking()
                          .Where(d => EmpPayrollIdList.Contains(d.Employee.Id))
                          .ToList();
                        if (ESICSTATEMENTS == null)
                            return null;
                        else
                        {
                            foreach (var l in ESICSTATEMENTS)
                            {
                                if (l.SalaryT.Count > 0)
                                {

                                    SalaryT salmonth = l.SalaryT.Where(a => mPayMonth.Contains(a.PayMonth)).SingleOrDefault();


                                    if (salmonth != null && salmonth.ESICTransT != null)
                                    {
                                        //List<SalaryT> empall = l.SalaryT.Where(r => mPayMonth.Contains(r.PayMonth)).ToList();

                                        //foreach (var ls in empall)
                                        //{
                                        GenericField100 OGenericObj = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = l.Employee.Id.ToString(),
                                            Fld23 = l.Employee.EmpCode,
                                            Fld24 = l.Employee.EmpName.FullNameFML,
                                            Fld20 = salmonth.PayStruct != null && salmonth.PayStruct.Grade != null ? salmonth.PayStruct.Grade.Name : null,
                                            Fld32 = salmonth.ESICTransT.EmpAmt.ToString(),
                                            Fld33 = salmonth.ESICTransT.CompAmt.ToString(),
                                            Fld25 = salmonth.PayMonth.ToString(),
                                            Fld26 = salmonth.Geostruct.Location != null && salmonth.Geostruct.Location.Address != null && salmonth.Geostruct.Location.Address.State != null ? salmonth.Geostruct.Location.Address.State.Name : "",
                                            //Fld54 = ca2.SalaryHead.Name.ToString(),
                                            //Fld55 = ca2.SalaryHead.Type.LookupVal.ToUpper().ToString(),
                                            //Fld56 = ca2.SalaryHead.Frequency.LookupVal.ToUpper().ToString(),
                                            //Fld57 = ca2.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString(),
                                            //Fld58 = ca2.Amount == null ? "" : ca2.Amount.ToString(),
                                            Fld9 = salmonth != null && salmonth.Geostruct.Location != null && salmonth.Geostruct.Location.LocationObj != null ? salmonth.Geostruct.Location.LocationObj.LocDesc : "",
                                            //Fld19 = ca9 != null && ca9.Department != null && ca9.Department.DepartmentObj != null ? ca9.Department.DepartmentObj.DeptDesc : "",
                                            //Fld34 = ca8 != null && ca8.Grade != null ? ca8.Grade.Name : "",
                                        };
                                        OGenericPayrollStatement.Add(OGenericObj);
                                        //}



                                        // }

                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "SALARYARREART":

                        var Pmonth = mPayMonth.FirstOrDefault();
                        var OSalArrearData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OSalArrearData_temp = db.EmployeePayroll
                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.SalaryArrearT)
                                //.Include(r => r.SalaryArrearT.Select(t => t.ArrearType))
                                //.Include(r => r.SalaryArrearT.Select(e => e.GeoStruct))
                                //.Include(r => r.SalaryArrearT.Select(t => t.PayStruct))
                                //.Include(r => r.SalaryArrearT.Select(t => t.FuncStruct))
                                //.Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT))
                                //.Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(u => u.SalaryHead.Type)))
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();
                            if (OSalArrearData_temp != null)
                            {
                                OSalArrearData_temp.Employee.EmpName = db.NameSingle.Find(OSalArrearData_temp.Employee.EmpName_Id);
                                OSalArrearData_temp.SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll_Id == OSalArrearData_temp.Id && e.PayMonth == Pmonth).AsNoTracking().ToList();
                                OSalArrearData_temp.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Where(e => e.Id == OSalArrearData_temp.Employee.EmpOffInfo_Id).SingleOrDefault();
                                OSalArrearData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OSalArrearData_temp.Employee.EmpOffInfo.Bank_Id).SingleOrDefault();
                                foreach (var i in OSalArrearData_temp.SalaryArrearT)
                                {
                                    var ArrearType = db.LookupValue.Where(e => e.Id == i.ArrearType_Id).AsNoTracking().FirstOrDefault();
                                    var SalaryArrearPaymentT = db.SalaryArrearT.Where(e => e.Id == i.Id).Select(r => r.SalaryArrearPaymentT.ToList()).AsNoTracking().FirstOrDefault();
                                    i.SalaryArrearPaymentT = SalaryArrearPaymentT;
                                    i.ArrearType = ArrearType;
                                    foreach (var j in i.SalaryArrearPaymentT)
                                    {
                                        var SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).SingleOrDefault();
                                        var Type = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).Select(r => r.Type).SingleOrDefault();
                                        j.SalaryHead = SalaryHead;
                                        j.SalaryHead.Type = Type;
                                    }
                                }
                                if (salheadlistLevel1.Count() > 0)
                                {
                                    foreach (var paybankname in salheadlistLevel1)
                                    {
                                        if (OSalArrearData_temp.Employee.EmpOffInfo.Bank != null && OSalArrearData_temp.Employee.EmpOffInfo.Bank.Name.ToUpper() == paybankname.ToUpper())
                                        {
                                            OSalArrearData.Add(OSalArrearData_temp);

                                        }
                                    }

                                }
                                else
                                {
                                    OSalArrearData.Add(OSalArrearData_temp);
                                }


                            }
                        }



                        if (OSalArrearData == null || OSalArrearData.Count() == 0)
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

                            foreach (var ca in OSalArrearData)
                            {
                                if (ca.SalaryArrearT != null && ca.SalaryArrearT.Count() != 0)
                                {
                                    var OSalArrT = ca.SalaryArrearT.ToList();
                                    //if (OSalAttend != null && OSalAttend.Count() != 0)
                                    //{

                                    int? geoid = OSalArrT.FirstOrDefault().GeoStruct_Id;

                                    int? payid = OSalArrT.FirstOrDefault().PayStruct_Id;

                                    int? funid = OSalArrT.FirstOrDefault().FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);



                                    if (GeoDataInd != null)
                                    {
                                        if (salheadlist != null && salheadlist.Count() != 0)
                                        {
                                            foreach (var item in salheadlist)
                                            {


                                                var OSalArrear = ca.SalaryArrearT.Where(e => e.ArrearType.LookupVal.ToUpper() == item.ToString()).ToList();

                                                if (OSalArrear != null && OSalArrear.Count() != 0)
                                                {
                                                    foreach (var ca1 in OSalArrear)
                                                    {
                                                        foreach (var item1 in ca1.SalaryArrearPaymentT.Where(e => e.SalaryHead.InPayslip == true))
                                                        {
                                                            GenericField100 OGenericObjStatement = new GenericField100()
                                                            //write data to generic class
                                                            {
                                                                //Fld1 = ca.Employee == null ? "" : ca.Employee.Id.ToString(),
                                                                Fld2 = ca.Employee == null ? "" : ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                                Fld3 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                                Fld4 = ca1.PayMonth == null ? "" : ca1.PayMonth,
                                                                Fld5 = ca1.ArrearType == null ? "" : ca1.ArrearType.LookupVal == null ? "" : ca1.ArrearType.LookupVal.ToUpper(),
                                                                Fld6 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                                Fld7 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                                Fld8 = ca1.TotalDays == null ? "" : ca1.TotalDays.ToString(),
                                                                Fld9 = ca1.IsRecovery == null ? "" : ca1.IsRecovery.ToString(),
                                                                Fld10 = ca1.IsAuto == null ? "" : ca1.IsAuto.ToString(),

                                                                Fld44 = item1.SalaryHead.Name.ToString(),
                                                                Fld45 = item1.SalHeadAmount.ToString(),
                                                                Fld46 = item1.SalaryHead.Type.LookupVal.ToString(),
                                                                Fld48 = ca.Employee.EmpOffInfo.Bank.Name

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
                                                                OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                            }


                                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var OSalArrear = ca.SalaryArrearT.ToList();

                                            if (OSalArrear != null && OSalArrear.Count() != 0)
                                            {
                                                foreach (var ca1 in OSalArrear)
                                                {
                                                    foreach (var item1 in ca1.SalaryArrearPaymentT.Where(e => e.SalaryHead.InPayslip == true))
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = ca.Employee == null ? "" : ca.Employee.Id.ToString(),
                                                            Fld2 = ca.Employee == null ? "" : ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                            Fld3 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                            Fld4 = ca1.PayMonth == null ? "" : ca1.PayMonth,
                                                            Fld5 = ca1.ArrearType == null ? "" : ca1.ArrearType.LookupVal == null ? "" : ca1.ArrearType.LookupVal.ToUpper(),
                                                            Fld6 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                            Fld7 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                            Fld8 = ca1.TotalDays == null ? "" : ca1.TotalDays.ToString(),
                                                            Fld9 = ca1.IsRecovery == null ? "" : ca1.IsRecovery.ToString(),
                                                            Fld10 = ca1.IsAuto == null ? "" : ca1.IsAuto.ToString(),

                                                            Fld44 = item1.SalaryHead.Name.ToString(),
                                                            Fld45 = item1.SalHeadAmount.ToString(),
                                                            Fld46 = item1.SalaryHead.Type.LookupVal.ToString(),
                                                            Fld48 = ca.Employee.EmpOffInfo.Bank.Name
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
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "SALARYARREARHEADWISE":

                        var Paymonth = mPayMonth.FirstOrDefault();
                        var OSalArrearHeadwise = new List<EmployeePayroll>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    var OSalArrearData_temp = db.EmployeePayroll
                        //    .Include(e => e.Employee)
                        //    .Include(e => e.Employee.EmpName)
                        //    .Include(r => r.SalaryArrearT)
                        //    .Include(r => r.SalaryArrearT.Select(t => t.ArrearType))
                        //    .Include(r => r.SalaryArrearT.Select(e => e.GeoStruct))
                        //    .Include(r => r.SalaryArrearT.Select(t => t.PayStruct))
                        //    .Include(r => r.SalaryArrearT.Select(t => t.FuncStruct))
                        //    .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT))
                        //    .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(u => u.SalaryHead.Type)))
                        //    .Where(e => e.Employee.Id == item).AsParallel()
                        //    .FirstOrDefault();
                        //    if (OSalArrearData_temp != null)
                        //    {
                        //        OSalArrearHeadwise.Add(OSalArrearData_temp);
                        //    }
                        //}


                        var osalarreartempdata = db.EmployeePayroll.Where(e => EmpPayrollIdList.Contains(e.Employee.Id) && e.SalaryArrearT.Count() > 0).Select(t => new
                        {
                            Empcode = t.Employee.EmpCode,
                            Empname = t.Employee.EmpName.FullNameFML,
                            SalaryArrearT = t.SalaryArrearT.Where(z => z.PayMonth == Paymonth).Select(m => new
                            {
                                GeoStruct = m.GeoStruct.Id,
                                FuncStruct = m.FuncStruct.Id,
                                PayStruct = m.PayStruct.Id,
                                Paymonth = m.PayMonth,
                                IsPaySlip = m.IsPaySlip,
                                SalaryArrearPaymentT = m.SalaryArrearPaymentT.Where(x => x.SalHeadAmount != 0).Select(y => new
                                {
                                    SalaryHead = y.SalaryHead.Name,
                                    SalHeadAmount = y.SalHeadAmount,
                                    SalheadType = y.SalaryHead.Type.LookupVal
                                }).ToList()
                            }),
                        }).AsNoTracking().ToList();

                        if (osalarreartempdata.Count() > 0)
                        {

                        }
                        if (osalarreartempdata == null || osalarreartempdata.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpecialList1 = SpecialGroupslist.SingleOrDefault();
                            bool test = false;
                            if (SpecialList1 == "AppearInPaySlip")
                            {
                                test = true;
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

                            List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            GeoData = GetViewData(0);
                            Paydata = GetViewData(1);
                            Funcdata = GetViewData(2);
                            List<string> salhead = new List<string>();
                            if (salheadlist.Count() != 0)
                            {
                                salhead.AddRange(salheadlist);
                            }
                            else
                            {
                                var a = db.SalaryArrearPaymentT.Select(e => e.SalaryHead.Name).Distinct().ToList();
                                foreach (var item in a)
                                {
                                    salhead.Add(item);
                                }
                            }

                            foreach (var ca in osalarreartempdata)
                            {
                                if (ca.SalaryArrearT != null && ca.SalaryArrearT.Count() != 0)
                                {
                                    int geoid = ca.SalaryArrearT.FirstOrDefault().GeoStruct;

                                    int payid = ca.SalaryArrearT.FirstOrDefault().PayStruct;

                                    int funid = ca.SalaryArrearT.FirstOrDefault().FuncStruct;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                    {

                                        var OSalArrear = ca.SalaryArrearT.Where(e => e.IsPaySlip == test).ToList();

                                        if (OSalArrear != null && OSalArrear.Count() != 0)
                                        {
                                            foreach (var ca1 in OSalArrear)
                                            {
                                                foreach (var item in salhead)
                                                {
                                                    var osal = ca1.SalaryArrearPaymentT.Where(e => e.SalaryHead == item).ToList();
                                                    foreach (var item1 in osal)
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            //Fld1 = ca.Employee == null ? "" : ca.Employee.Id.ToString(),
                                                            Fld2 = ca.Empcode,
                                                            Fld3 = ca.Empname,
                                                            Fld4 = ca1.Paymonth == null ? "" : ca1.Paymonth,
                                                            Fld44 = item1.SalaryHead.ToString(),
                                                            Fld45 = item1.SalHeadAmount.ToString(),
                                                            Fld46 = item1.SalheadType.ToString()

                                                        };
                                                        if (month)
                                                        {
                                                            OGenericObjStatement.Fld100 = ca1.Paymonth.ToString();
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
                                                            OGenericObjStatement.Fld88 = ca.Empname.ToString();
                                                        }


                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                }
                                            }
                                            // }
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;
                    case "SALARYARREARTSUMMARY":

                        //var OSalArrearDataSum = new List<EmployeePayroll>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    var OSalArrearDataSum_temp = db.EmployeePayroll
                        //    .Include(e => e.Employee)
                        //    .Include(r => r.SalaryArrearT)
                        //    .Include(r => r.SalaryArrearT.Select(t => t.ArrearType))
                        //    .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT))
                        //    .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(u => u.SalaryHead.Type)))
                        //    .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                        //    .FirstOrDefault();
                        //    if (OSalArrearDataSum_temp != null)
                        //    {
                        //        OSalArrearDataSum.Add(OSalArrearDataSum_temp);
                        //    }
                        //}
                        string paymonarr = mPayMonth.FirstOrDefault();
                        List<string> lvname = new List<string>();
                        if (SpecialGroupslist.Count() != 0)
                        {
                            lvname.AddRange(SpecialGroupslist);
                        }
                        else
                        {
                            var a = db.SalaryArrearT.Select(e => e.ArrearType.LookupVal).Distinct().ToList();
                            List<string> te = new List<string>();
                            foreach (var item in a)
                            {
                                lvname.Add(item);
                            }
                        }

                        var osalarrearSummaryData = db.EmployeePayroll.Where(e => EmpPayrollIdList.Contains(e.Employee.Id) && e.SalaryArrearT.Count() > 0).Select(t => new
                        {
                            Empcode = t.Employee.EmpCode,
                            Empname = t.Employee.EmpName.FullNameFML,
                            // SalaryArrearT = t.SalaryArrearT.Where(z => z.FromDate.Value >= pfromdate && z.ToDate.Value <= ptodate).Select(m => new
                            SalaryArrearT = t.SalaryArrearT.Where(z => z.PayMonth == paymonarr).Select(m => new
                            {
                                GeoStruct = m.GeoStruct.Id,
                                FuncStruct = m.FuncStruct.Id,
                                PayStruct = m.PayStruct.Id,
                                IsPaySlip = m.IsPaySlip,
                                IsRecovery = m.IsRecovery,
                                TotalDays = m.TotalDays,
                                SalaryArrearPaymentT = m.SalaryArrearPaymentT.Where(x => x.SalHeadAmount != 0).Select(y => new
                                {
                                    SalaryHead = y.SalaryHead.Name,
                                    SalHeadAmount = y.SalHeadAmount,
                                    SalheadType = y.SalaryHead.Type.LookupVal
                                }).ToList()
                            }),
                        }).AsNoTracking().ToList();
                        if (osalarrearSummaryData == null || osalarrearSummaryData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpecialList1 = SpecialGroupslist.SingleOrDefault();
                            bool test = false;
                            if (Sorting.ToUpper() == "RECOVERY")
                            {
                                test = true;
                            }

                            var salaryarreartdata = osalarrearSummaryData.Where(e => e.SalaryArrearT.Count() > 0).ToList();
                            foreach (var ca in salaryarreartdata)
                            {
                                if (ca.SalaryArrearT != null && ca.SalaryArrearT.Count() != 0)
                                {


                                    var OSalArrear = ca.SalaryArrearT.Where(e => e.IsRecovery == test).ToList();
                                    if (OSalArrear != null && OSalArrear.Count() != 0)
                                    {
                                        foreach (var ca1 in OSalArrear)
                                        {
                                            if (salheadlist != null && salheadlist.Count() != 0)
                                            {
                                                foreach (var ca2 in salheadlist)
                                                {
                                                    var item = ca1.SalaryArrearPaymentT.Where(e => e.SalaryHead == ca2.ToString()).ToList();
                                                    foreach (var item1 in item)
                                                    {
                                                        var Salhead = "";
                                                        double SalAmt = 0;
                                                        var Salhead1 = "";
                                                        double SalAmt1 = 0;

                                                        if (item1.SalheadType.ToUpper() == "EARNING")
                                                        {
                                                            Salhead = item1.SalaryHead.ToString();
                                                            SalAmt = item1.SalHeadAmount;
                                                        }
                                                        if (item1.SalheadType.ToUpper() == "DEDUCTION")
                                                        {
                                                            Salhead1 = item1.SalaryHead.ToString();
                                                            SalAmt1 = item1.SalHeadAmount;
                                                        }


                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            //Fld1 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                            Fld44 = Salhead,///Earning head
                                                            Fld45 = SalAmt.ToString(),///Earning amount

                                                            Fld46 = Salhead1,///Deduction head
                                                            Fld47 = SalAmt1.ToString(),///Deduction amount
                                                        };
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }
                                                    GenericField100 OGenericObjStatement1 = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        //;Fld1 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),

                                                        ///Fld44 = Salhead,///Earning head
                                                        // Fld45 = SalAmt.ToString(),///Earning amount

                                                        // Fld46 = Salhead1,///Deduction head
                                                        //Fld47 = SalAmt1.ToString(),///Deduction amount
                                                        Fld48 = ca1.TotalDays.ToString(),
                                                        Fld49 = ca1.IsRecovery.ToString(),
                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObjStatement1);
                                                }
                                            }
                                            else
                                            {
                                                var item = ca1.SalaryArrearPaymentT.ToList();
                                                foreach (var item1 in item)
                                                {
                                                    var Salhead = "";
                                                    double SalAmt = 0;
                                                    var Salhead1 = "";
                                                    double SalAmt1 = 0;

                                                    if (item1.SalheadType.ToUpper() == "EARNING")
                                                    {
                                                        Salhead = item1.SalaryHead.ToString();
                                                        SalAmt = item1.SalHeadAmount;
                                                    }
                                                    if (item1.SalheadType.ToUpper() == "DEDUCTION")
                                                    {
                                                        Salhead1 = item1.SalaryHead.ToString();
                                                        SalAmt1 = item1.SalHeadAmount;
                                                    }

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        //;Fld1 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),

                                                        Fld44 = Salhead,///Earning head
                                                        Fld45 = SalAmt.ToString(),///Earning amount

                                                        Fld46 = Salhead1,///Deduction head
                                                        Fld47 = SalAmt1.ToString(),///Deduction amount

                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                                int totaldays = -Convert.ToInt32(ca1.TotalDays);
                                                GenericField100 OGenericObjStatement1 = new GenericField100()
                                                //write data to generic class
                                                {
                                                    //;Fld1 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),

                                                    ///Fld44 = Salhead,///Earning head
                                                    // Fld45 = SalAmt.ToString(),///Earning amount

                                                    // Fld46 = Salhead1,///Deduction head
                                                    //Fld47 = SalAmt1.ToString(),///Deduction amount
                                                    Fld48 = ca1.IsRecovery == true ? totaldays.ToString() : ca1.TotalDays.ToString(),
                                                    Fld49 = ca1.IsRecovery.ToString(),
                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement1);
                                            }

                                        }
                                    }

                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "ARREARPFT":

                        var OSalArrearPFTData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OSalArrearDataSum_temp = db.EmployeePayroll
                            .Include(e => e.Employee)
                                ////.Include(e => e.Employee.EmpName)
                                ////.Include(r => r.SalaryArrearT)
                                ////.Include(r => r.SalaryArrearT.Select(t => t.ArrearType))
                                //// .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPFT))
                                //  .Include(r => r.SalaryArrearT.Select(t => t.SalaryArrearPaymentT.Select(u => u.SalaryHead.Type)))
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();

                            if (OSalArrearDataSum_temp != null)
                            {
                                OSalArrearDataSum_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OSalArrearDataSum_temp.Employee.EmpName_Id).FirstOrDefault();
                                OSalArrearDataSum_temp.SalaryArrearT = db.SalaryArrearT.Where(e => e.EmployeePayroll_Id == OSalArrearDataSum_temp.Id).ToList();
                                foreach (var i in OSalArrearDataSum_temp.SalaryArrearT)
                                {
                                    i.ArrearType = db.LookupValue.Where(e => e.Id == i.ArrearType_Id).FirstOrDefault();
                                    i.SalaryArrearPFT = db.SalaryArrearPFT.Where(e => e.Id == i.SalaryArrearPFT_Id).FirstOrDefault();
                                    i.SalaryArrearPaymentT = db.SalaryArrearT.Where(e => e.Id == i.Id).Select(r => r.SalaryArrearPaymentT.ToList()).FirstOrDefault();
                                    foreach (var j in i.SalaryArrearPaymentT)
                                    {
                                        j.SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
                                        j.SalaryHead.Type = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).Select(r => r.Type).FirstOrDefault();
                                    }

                                }
                                OSalArrearPFTData.Add(OSalArrearDataSum_temp);
                            }
                        }

                        if (OSalArrearPFTData == null || OSalArrearPFTData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var SpecialList1 = SpecialGroupslist.SingleOrDefault();
                            bool test = false;
                            if (SpecialList1 == "AppearInPaySlip")
                            {
                                test = true;
                            }
                            foreach (var ca in OSalArrearPFTData)
                            {
                                if (ca.SalaryArrearT.Count() != 0)
                                {
                                    foreach (var paym in mPayMonth)
                                    {
                                        var OSalArrear = ca.SalaryArrearT.ToList();
                                        foreach (var ca1 in OSalArrear)
                                        {
                                            if (ca1.SalaryArrearPFT != null)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = ca.Employee.EmpCode,
                                                    Fld2 = ca.Employee.EmpName.FullNameFML,
                                                    Fld3 = ca1.PayMonth == null ? "" : ca1.PayMonth.ToString(),
                                                    Fld4 = ca1.SalaryArrearPFT.EPFWages.ToString(),
                                                    Fld5 = ca1.SalaryArrearPFT.EmpPF.ToString(),
                                                    Fld6 = ca1.SalaryArrearPFT.EmpEPS.ToString(),
                                                    Fld7 = ca1.SalaryArrearPFT.CompPF.ToString(),
                                                    Fld8 = ca1.SalaryArrearPFT.EmpVPF.ToString(),
                                                    Fld9 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                    Fld10 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "LOANADVREPAYMENTPROCESS":
                        var OLoanAdvRepaymentProcess = new List<EmployeePayroll>();

                        foreach (var item in EmpPayrollIdList)
                        {
                            var OLoanAdvRepaymentData_temp = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.LoanAdvRequest.Select(t => t.LoanAdvRepaymentT))
                                //.Include(e => e.LoanAdvRequest.Select(t => t.LoanAccBranch))
                                // .Include(e => e.LoanAdvRequest.Select(t => t.LoanAccBranch.LocationObj))
                                //.Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                // .Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                           .SingleOrDefault();
                            if (OLoanAdvRepaymentData_temp != null)
                            {
                                OLoanAdvRepaymentData_temp.Employee.EmpName = db.NameSingle.Find(OLoanAdvRepaymentData_temp.Employee.EmpName_Id);
                                OLoanAdvRepaymentData_temp.LoanAdvRequest = db.LoanAdvRequest.Where(e => e.EmployeePayroll_Id == OLoanAdvRepaymentData_temp.Id).AsNoTracking().ToList();
                                foreach (var i in OLoanAdvRepaymentData_temp.LoanAdvRequest)
                                {
                                    var LoanAccBranch = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAccBranch).FirstOrDefault();
                                    var LoanAccBranch_LocationObj = db.Location.Where(e => e.Id == i.LoanAccBranch_Id).Select(r => r.LocationObj).FirstOrDefault();
                                    var LoanAdvanceHead = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvanceHead).FirstOrDefault();
                                    var SalaryHead = db.LoanAdvanceHead.Where(e => e.Id == i.LoanAdvanceHead_Id).Select(r => r.SalaryHead).FirstOrDefault();
                                    var LoanAdvRepaymentT = db.LoanAdvRequest.Where(e => e.Id == i.Id).Select(r => r.LoanAdvRepaymentT.Where(t => mPayMonth.Contains(t.PayMonth.ToString()) && t.RepaymentDate != null).ToList()).FirstOrDefault();
                                    i.LoanAccBranch = LoanAccBranch;
                                    i.LoanAccBranch.LocationObj = LoanAccBranch_LocationObj;
                                    i.LoanAdvanceHead = LoanAdvanceHead;
                                    i.LoanAdvanceHead.SalaryHead = SalaryHead;
                                    i.LoanAdvRepaymentT = LoanAdvRepaymentT;
                                }
                                OLoanAdvRepaymentProcess.Add(OLoanAdvRepaymentData_temp);
                            }
                        }


                        if (OLoanAdvRepaymentProcess == null || OLoanAdvRepaymentProcess.Count() == 0)
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

                            foreach (var ca in OLoanAdvRepaymentProcess)
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
                                                    if (ca1.LoanAdvRepaymentT == null || ca1.LoanAdvRepaymentT.Count() == 0)
                                                    { }
                                                    else
                                                    {
                                                        var loanprocessdata = ca1.LoanAdvRepaymentT.ToList();
                                                        foreach (var ca2 in loanprocessdata)
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
                                                                Fld30 = ca1.LoanAccBranch == null ? "" : ca1.LoanAccBranch.LocationObj == null ? "" : ca1.LoanAccBranch.LocationObj.LocDesc == null ? "" : ca1.LoanAccBranch.LocationObj.LocDesc.ToString(),
                                                                Fld31 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                                                Fld28 = ca.Employee != null && ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                                Fld29 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name

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
                                    else
                                    {
                                        var OITTrans = ca.LoanAdvRequest.ToList();
                                        //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
                                        //  var OITTrans = ca.l //.Where(e => mPayMonth.Contains(e.RequisitionDate.Value.ToString("MM/yyyy"))).ToList();
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

                                            foreach (var ca1 in OITTrans)
                                            {
                                                if (ca1.LoanAdvRepaymentT != null || ca1.LoanAdvRepaymentT.Count() > 0)
                                                {
                                                    // else
                                                    //   {
                                                    var loanprocessdata = ca1.LoanAdvRepaymentT.ToList();

                                                    foreach (var ca2 in loanprocessdata)
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
                                                            Fld30 = ca1.LoanAccBranch == null ? "" : ca1.LoanAccBranch.LocationObj == null ? "" : ca1.LoanAccBranch.LocationObj.LocDesc == null ? "" : ca1.LoanAccBranch.LocationObj.LocDesc.ToString(),
                                                            Fld31 = ca.Employee != null && ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : null,
                                                            Fld28 = ca.Employee != null && ca.Employee.PayStruct != null && ca.Employee.PayStruct.Grade != null ? ca.Employee.PayStruct.Grade.Name : null,
                                                            Fld29 = GeoDataInd == null ? "" : GeoDataInd.FuncStruct_Job_Name

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
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "FUNCTIONALATTENDANCEPROCESS":

                        var OFuncAttProcess = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OFuncAttData_temp = db.EmployeePayroll

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

                            OFuncAttData_temp.Employee = db.Employee.Find(OFuncAttData_temp.Employee_Id);
                            OFuncAttData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OFuncAttData_temp.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();
                            OFuncAttData_temp.FunctAttendanceT = db.FunctAttendanceT.Where(e => e.EmployeePayroll_Id == OFuncAttData_temp.Id).AsNoTracking().ToList();
                            OFuncAttData_temp.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OFuncAttData_temp.Id).AsNoTracking().ToList();

                            if (OFuncAttData_temp != null)
                            {
                                OFuncAttProcess.Add(OFuncAttData_temp);
                            }
                        }
                        if (OFuncAttProcess == null || OFuncAttProcess.Count() == 0)
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

                            //List<Utility.GetOrganizationDataClass> GeoData = new List<Utility.GetOrganizationDataClass>();
                            //List<Utility.GetOrganizationDataClass> Paydata = new List<Utility.ReportClass>();
                            //List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            //Utility.GetOrganizationDataClass PaydataInd = new Utility.GetOrganizationDataClass();
                            //Utility.GetOrganizationDataClass FuncdataInd = new Utility.GetOrganizationDataClass();

                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            //GeoData = ReportRDLCObjectClass.GetOrganizationData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            //   var functhead = db.FunctAttendanceT.Select(e => e.SalaryHead.Id).ToList();
                            foreach (var ca in OFuncAttProcess)
                            {
                                //foreach (var id in functhead)
                                //{
                                //    foreach (var functid in ca.SalaryT.Where(e => mPayMonth.Contains(e.PayMonth)))
                                //    {
                                //        var ca3 = functid.SalEarnDedT.Where(e => e.SalaryHead.Id == id).ToList();
                                //        foreach (var item3 in ca3)
                                //        {

                                if (ca.FunctAttendanceT != null && ca.FunctAttendanceT.Count() != 0)
                                {

                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var item2 in salheadlist)
                                        {
                                            foreach (var item in mPayMonth)
                                            {
                                                //foreach (var ca1 in ca.FunctAttendanceT)
                                                //{
                                                //    ca1.SalaryHead = db.SalaryHead.Find(ca1.SalaryHead_Id);
                                                //}
                                                //var OOFuncAtt = ca.FunctAttendanceT.Where(e => item == e.PayMonth && e.SalaryHead.Code == item2.ToString()).ToList();
                                                int SalHeadId = db.SalaryHead.Where(e => e.Code == item2.ToString()).FirstOrDefault().Id;
                                                var OOFuncAtt = ca.FunctAttendanceT.Where(e => item == e.PayMonth && e.SalaryHead_Id == SalHeadId).ToList();
                                                if (OOFuncAtt != null && OOFuncAtt.Count() != 0)
                                                {

                                                    int? geoid = OOFuncAtt.FirstOrDefault().GeoStruct_Id;

                                                    int? payid = OOFuncAtt.FirstOrDefault().PayStruct_Id;

                                                    int? funid = OOFuncAtt.FirstOrDefault().FuncStruct_Id;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                                                    //PaydataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geoid, payid, funid, 1);
                                                    //FuncdataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geoid, payid, funid, 2);

                                                    //if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                                    if (GeoDataInd != null)
                                                    {
                                                        foreach (var ca1 in OOFuncAtt)
                                                        {
                                                            foreach (var functid in ca.SalaryT.Where(e => mPayMonth.Contains(e.PayMonth)))
                                                            {
                                                                List<SalaryHead> OSalHead = new List<SalaryHead>();
                                                                functid.SalEarnDedT = db.SalaryT.Where(e => e.Id == functid.Id).Select(e => e.SalEarnDedT).ToList().FirstOrDefault();
                                                                foreach (var itemS in functid.SalEarnDedT)
                                                                {
                                                                    itemS.SalaryHead = db.SalaryHead.Find(itemS.SalaryHead_Id);
                                                                    //OSalHead.Add(itemS.SalaryHead);
                                                                }
                                                                var ca3 = functid.SalEarnDedT.Where(e => e.SalaryHead_Id == ca1.SalaryHead.Id).ToList();

                                                                foreach (var item3 in ca3)
                                                                {

                                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                                    //write data to generic class
                                                                    {
                                                                        Fld1 = ca1.Id.ToString(),
                                                                        Fld2 = ca.Employee.EmpCode,
                                                                        Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFM : null,
                                                                        Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                                                        Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                                                        Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                                                        Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                                                        Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                                                        Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                                                        Fld10 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                                        Fld44 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                                        Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                                                        Fld46 = item3 == null ? "" : item3.Amount.ToString(),
                                                                        Fld47 = ca1.Reason != null ? ca1.Reason : null,
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
                                                //    //return null;
                                                //}
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in mPayMonth)
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
                                                //PaydataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geoid, payid, funid, 1);
                                                //FuncdataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geoid, payid, funid, 2);

                                                //if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                                if (GeoDataInd != null)
                                                {
                                                    foreach (var ca1 in OOFuncAtt)
                                                    {
                                                        foreach (var functid in ca.SalaryT.Where(e => mPayMonth.Contains(e.PayMonth)))
                                                        {
                                                            List<SalaryHead> OSalHead = new List<SalaryHead>();
                                                            functid.SalEarnDedT = db.SalaryT.Where(e => e.Id == functid.Id).Select(e => e.SalEarnDedT).ToList().FirstOrDefault();
                                                            foreach (var itemS in functid.SalEarnDedT)
                                                            {
                                                                itemS.SalaryHead = db.SalaryHead.Find(itemS.SalaryHead_Id);
                                                                //OSalHead.Add(itemS.SalaryHead);
                                                            }

                                                            var ca3 = functid.SalEarnDedT.Where(e => e.SalaryHead_Id == ca1.SalaryHead.Id).ToList();
                                                            foreach (var item3 in ca3)
                                                            {
                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                //write data to generic class
                                                                {
                                                                    Fld1 = ca1.Id.ToString(),
                                                                    Fld2 = ca.Employee.EmpCode,
                                                                    Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFM : null,
                                                                    Fld4 = ca1.ProcessMonth != null ? ca1.ProcessMonth : null,
                                                                    Fld5 = ca1.PayMonth != null ? ca1.PayMonth : null,
                                                                    Fld6 = ca1.SalaryHead != null ? ca1.SalaryHead.Id.ToString() : null,
                                                                    Fld7 = ca1.SalaryHead != null ? ca1.SalaryHead.Code : null,
                                                                    Fld8 = ca1.SalaryHead != null ? ca1.SalaryHead.Name : null,
                                                                    Fld9 = ca1.ReqDate != null ? ca1.ReqDate.ToString() : null,
                                                                    Fld10 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                                    Fld44 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                                    Fld45 = ca1.HourDays != null ? ca1.HourDays.ToString() : null,
                                                                    //  Fld46 = item3 == null ? "" : item3.Amount.ToString(),
                                                                    Fld47 = ca1.Reason != null ? ca1.Reason : null,
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
                                //    //return null;
                                //}

                                // }
                                //  }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "INSURANCEPROCESS":
                        //var OInsuranceProcess = new List<EmployeePayroll>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    var OInsuranceData_temp = db.EmployeePayroll

                        //    .Include(e => e.Employee)
                        //        //.Include(e => e.Employee)
                        //        //.Include(e => e.Employee.EmpName)
                        //        //.Include(r => r.SalaryT)
                        //        //.Include(r => r.SalaryT.Select(t => t.SalEarnDedT))
                        //        //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                        //        //.Include(r => r.InsuranceDetailsT)                            
                        //        //.Include(r => r.InsuranceDetailsT.Select(t => t.OperationStatus))
                        //        //.Include(r => r.InsuranceDetailsT.Select(t => t.Frequency))
                        //        //.Include(r => r.InsuranceDetailsT.Select(t => t.InsuranceProduct))
                        //        //.Include(e => e.Employee.GeoStruct)
                        //        //.Include(e => e.Employee.PayStruct)
                        //        //.Include(e => e.Employee.FuncStruct)

                        //    .Where(e => e.Employee.Id == item).AsNoTracking()
                        //    .FirstOrDefault();
                        //    if (OInsuranceData_temp != null)
                        //    {
                        //        if (salheadlistLevel1.Count() != 0 && salheadlistLevel1.Count() > 0)
                        //        {
                        //            OInsuranceData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OInsuranceData_temp.Employee.EmpName_Id).FirstOrDefault();
                        //            OInsuranceData_temp.Employee.EmpOffInfo = db.EmpOff.Find(OInsuranceData_temp.Employee.EmpOffInfo_Id);
                        //            OInsuranceData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OInsuranceData_temp.Employee.EmpOffInfo.Bank_Id && salheadlistLevel1.Contains(e.Name.ToString())).FirstOrDefault();
                        //            OInsuranceData_temp.InsuranceDetailsT = db.InsuranceDetailsT.Where(e => e.EmployeePayroll_Id == OInsuranceData_temp.Id && e.OperationStatus.LookupVal.ToUpper() == "ACTIVE").ToList();
                        //            OInsuranceData_temp.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OInsuranceData_temp.Id && mPayMonth.Contains(e.PayMonth)).ToList();
                        //            foreach (var i in OInsuranceData_temp.InsuranceDetailsT)
                        //            {
                        //                var OperationStatus = db.InsuranceDetailsT.Where(e => e.Id == i.Id).Select(r => r.OperationStatus).FirstOrDefault();
                        //                var Frequency = db.InsuranceDetailsT.Where(e => e.Id == i.Id).Select(r => r.Frequency).FirstOrDefault();
                        //                var InsuranceProduct = db.InsuranceDetailsT.Where(e => e.Id == i.Id).Select(r => r.InsuranceProduct).FirstOrDefault();
                        //                i.OperationStatus = OperationStatus;
                        //                i.Frequency = Frequency;
                        //                i.InsuranceProduct = InsuranceProduct;
                        //            }
                        //            foreach (var j in OInsuranceData_temp.SalaryT)
                        //            {
                        //                var SalEarnDedT = db.SalaryT.Where(e => e.Id == j.Id).Select(t => t.SalEarnDedT.ToList()).FirstOrDefault();
                        //                j.SalEarnDedT = SalEarnDedT;
                        //                foreach (var k in j.SalEarnDedT)
                        //                {
                        //                    k.SalaryHead = db.SalaryHead.Where(e => e.Id == k.SalaryHead_Id).FirstOrDefault();
                        //                }

                        //            }
                        //            if (OInsuranceData_temp.Employee != null && OInsuranceData_temp.Employee.EmpOffInfo != null && OInsuranceData_temp.Employee.EmpOffInfo.Bank != null)
                        //            {
                        //                OInsuranceProcess.Add(OInsuranceData_temp);
                        //            }

                        //        }
                        //        else
                        //        {
                        //            OInsuranceData_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OInsuranceData_temp.Employee.EmpName_Id).FirstOrDefault();
                        //            OInsuranceData_temp.Employee.EmpOffInfo = db.EmpOff.Find(OInsuranceData_temp.Employee.EmpOffInfo_Id);
                        //            OInsuranceData_temp.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OInsuranceData_temp.Employee.EmpOffInfo.Bank_Id).FirstOrDefault();
                        //            OInsuranceData_temp.InsuranceDetailsT = db.InsuranceDetailsT.Where(e => e.EmployeePayroll_Id == OInsuranceData_temp.Id && e.OperationStatus.LookupVal.ToUpper() == "ACTIVE").ToList();
                        //            OInsuranceData_temp.SalaryT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OInsuranceData_temp.Id && mPayMonth.Contains(e.PayMonth)).ToList();
                        //            foreach (var i in OInsuranceData_temp.InsuranceDetailsT)
                        //            {
                        //                var OperationStatus = db.InsuranceDetailsT.Where(e => e.Id == i.Id).Select(r => r.OperationStatus).FirstOrDefault();
                        //                var Frequency = db.InsuranceDetailsT.Where(e => e.Id == i.Id).Select(r => r.Frequency).FirstOrDefault();
                        //                var InsuranceProduct = db.InsuranceDetailsT.Where(e => e.Id == i.Id).Select(r => r.InsuranceProduct).FirstOrDefault();
                        //                i.OperationStatus = OperationStatus;
                        //                i.Frequency = Frequency;
                        //                i.InsuranceProduct = InsuranceProduct;
                        //            }
                        //            foreach (var j in OInsuranceData_temp.SalaryT)
                        //            {
                        //                var SalEarnDedT = db.SalaryT.Where(e => e.Id == j.Id).Select(t => t.SalEarnDedT.ToList()).FirstOrDefault();
                        //                j.SalEarnDedT = SalEarnDedT;
                        //                foreach (var k in j.SalEarnDedT)
                        //                {
                        //                    k.SalaryHead = db.SalaryHead.Where(e => e.Id == k.SalaryHead_Id).FirstOrDefault();
                        //                }

                        //            }

                        //                OInsuranceProcess.Add(OInsuranceData_temp);                                   
                        //        }
                        //    }
                        //}
                        string PayMon = mPayMonth.FirstOrDefault();
                        var OInsuranceProcess = db.EmployeePayroll.Where(e => EmpPayrollIdList.Contains(e.Employee_Id.Value)).Select(t => new
                           {
                               EmpId = t.Employee_Id.Value,
                               EmpCode = t.Employee.EmpCode,
                               EmpName = t.Employee.EmpName.FullNameFML,
                               FuncStruct = t.Employee.FuncStruct.Id,
                               PayStruct = t.Employee.PayStruct.Id,
                               GeoStruct = t.Employee.GeoStruct.Id,
                               Bank = t.Employee.EmpOffInfo.Bank.Name.ToString(),
                               OInsuranceDetailsT = t.InsuranceDetailsT.Where(e => e.OperationStatus.LookupVal.ToUpper() == "ACTIVE").Select(y => new
                               {
                                   InsuranceProductDesc = y.InsuranceProduct.InsuranceProductDesc,
                                   FromDate = y.FromDate,
                                   ToDate = y.ToDate,
                                   Frequency = y.Frequency.LookupVal.ToUpper().ToString(),
                                   OperationStatus = y.OperationStatus.LookupVal.ToUpper().ToString(),
                                   PolicyNo = y.PolicyNo.ToString(),
                                   Premium = y.Premium.ToString(),
                               }).ToList(),
                               SalaryT = t.SalaryT.Where(e => PayMon.Contains(e.PayMonth)).Select(y => new
                               {
                                   OSalEarnDedT = y.SalEarnDedT.Select(x => new
                                   {
                                       OsalaryHeadId = x.SalaryHead_Id.Value
                                   }).ToList(),

                               }).ToList()
                           }).AsNoTracking().ToList();
                        if (OInsuranceProcess == null || OInsuranceProcess.Count() == 0)
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
                                else if (item1 == "OperationStatus")
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
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                            //GeoData = GetViewData(0);
                            //Paydata = GetViewData(1);
                            //Funcdata = GetViewData(2);

                            ///

                            foreach (var ca in OInsuranceProcess)
                            {
                                var insurancehead = db.Insurance.Select(e => e.SalaryHead.Id).ToList();
                                if (insurancehead != null && insurancehead.Count() > 0)
                                {
                                    foreach (var salheadid in insurancehead)
                                    {
                                        foreach (var insurance in ca.SalaryT)
                                        {
                                            var ca3 = insurance.OSalEarnDedT.Where(e => e.OsalaryHeadId == salheadid).ToList();

                                            foreach (var item1 in ca3)
                                            {
                                                if (ca.OInsuranceDetailsT != null && ca.OInsuranceDetailsT.Count() != 0)
                                                {

                                                    int? geoid = ca.GeoStruct;

                                                    int? payid = ca.PayStruct;

                                                    int? funid = ca.FuncStruct;

                                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                                    if (GeoDataInd != null)
                                                    {

                                                        foreach (var ca1 in ca.OInsuranceDetailsT)
                                                        {
                                                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                                                            {
                                                                foreach (var Bank in salheadlistLevel1)
                                                                {
                                                                    if (ca.Bank == Bank)
                                                                    {
                                                                        foreach (var Lic in salheadlist)
                                                                        {
                                                                            if (ca1.InsuranceProductDesc == Lic)
                                                                            {
                                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                                //write data to generic class
                                                                                {
                                                                                    Fld1 = ca.EmpId.ToString(),
                                                                                    Fld2 = ca.EmpCode == null ? "" : ca.EmpCode,
                                                                                    Fld3 = ca.EmpName == null ? "" : ca.EmpName,
                                                                                    Fld4 = ca1.InsuranceProductDesc == null ? "" : ca1.InsuranceProductDesc == null ? "" : ca1.InsuranceProductDesc,
                                                                                    Fld5 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                                                    Fld6 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                                                    Fld7 = ca1.Frequency == null ? "" : ca1.Frequency,
                                                                                    Fld8 = ca1.OperationStatus == null ? "" : ca1.OperationStatus,
                                                                                    Fld9 = ca1.PolicyNo == null ? "" : ca1.PolicyNo.ToString(),
                                                                                    Fld10 = ca1.Premium.ToString(),
                                                                                    Fld11 = ca.Bank,

                                                                                };

                                                                                //if (month)
                                                                                //{
                                                                                //    OGenericObjStatement.Fld100 = insurance.PayMonth.ToString();
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
                                                                                //if (emp)
                                                                                //{
                                                                                //    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                                //}
                                                                                //if (Insurancehead)
                                                                                //{
                                                                                //    OGenericObjStatement.Fld87 = ca1.InsuranceProduct.InsuranceProductDesc;
                                                                                //}
                                                                                //if (OperationStatus)
                                                                                //{
                                                                                //    OGenericObjStatement.Fld86 = ca1.OperationStatus.LookupVal.ToUpper();
                                                                                //}

                                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            else if (salheadlistLevel1.Count() != 0 && salheadlistLevel1.Count() > 0)
                                                            {
                                                                foreach (var Bank in salheadlistLevel1)
                                                                {
                                                                    if (ca.Bank == Bank)
                                                                    {
                                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                                        //write data to generic class
                                                                        {
                                                                            Fld1 = ca.EmpId.ToString(),
                                                                            Fld2 = ca.EmpCode == null ? "" : ca.EmpCode,
                                                                            Fld3 = ca.EmpName == null ? "" : ca.EmpName,
                                                                            Fld4 = ca1.InsuranceProductDesc == null ? "" : ca1.InsuranceProductDesc == null ? "" : ca1.InsuranceProductDesc,
                                                                            Fld5 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                                            Fld6 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                                            Fld7 = ca1.Frequency == null ? "" : ca1.Frequency,
                                                                            Fld8 = ca1.OperationStatus == null ? "" : ca1.OperationStatus,
                                                                            Fld9 = ca1.PolicyNo == null ? "" : ca1.PolicyNo.ToString(),
                                                                            Fld10 = ca1.Premium.ToString(),
                                                                            Fld11 = ca.Bank,

                                                                        };

                                                                        //if (month)
                                                                        //{
                                                                        //    OGenericObjStatement.Fld100 = insurance.PayMonth.ToString();
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
                                                                        //if (emp)
                                                                        //{
                                                                        //    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                        //}
                                                                        //if (Insurancehead)
                                                                        //{
                                                                        //    OGenericObjStatement.Fld87 = ca1.InsuranceProduct.InsuranceProductDesc;
                                                                        //}
                                                                        //if (OperationStatus)
                                                                        //{
                                                                        //    OGenericObjStatement.Fld86 = ca1.OperationStatus.LookupVal.ToUpper();
                                                                        //}

                                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                                    }
                                                                }
                                                            }

                                                            else
                                                            {
                                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                                //write data to generic class
                                                                {
                                                                    Fld1 = ca.EmpId.ToString(),
                                                                    Fld2 = ca.EmpCode == null ? "" : ca.EmpCode,
                                                                    Fld3 = ca.EmpName == null ? "" : ca.EmpName,
                                                                    Fld4 = ca1.InsuranceProductDesc == null ? "" : ca1.InsuranceProductDesc == null ? "" : ca1.InsuranceProductDesc,
                                                                    Fld5 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                                    Fld6 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                                    Fld7 = ca1.Frequency == null ? "" : ca1.Frequency,
                                                                    Fld8 = ca1.OperationStatus == null ? "" : ca1.OperationStatus,
                                                                    Fld9 = ca1.PolicyNo == null ? "" : ca1.PolicyNo.ToString(),
                                                                    Fld10 = ca1.Premium.ToString(),
                                                                    Fld11 = ca.Bank,

                                                                };

                                                                //if (month)
                                                                //{
                                                                //    OGenericObjStatement.Fld100 = insurance.PayMonth.ToString();
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
                                                                //if (emp)
                                                                //{
                                                                //    OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                                //}
                                                                //if (Insurancehead)
                                                                //{
                                                                //    OGenericObjStatement.Fld87 = ca1.InsuranceProduct.InsuranceProductDesc;
                                                                //}
                                                                //if (OperationStatus)
                                                                //{
                                                                //    OGenericObjStatement.Fld86 = ca1.OperationStatus.LookupVal.ToUpper();
                                                                //}

                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            }

                                                        }
                                                    }


                                                }

                                                //var OInsurance = ca.InsuranceDetailsT.ToList();

                                                //if (OInsurance != null && OInsurance.Count() != 0)
                                                //{

                                                //    //int geoid = ca.Employee.GeoStruct.Id;

                                                //    //int payid = ca.Employee.PayStruct.Id;

                                                //    //int funid = ca.Employee.FuncStruct.Id;

                                                //    //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //    //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //    //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                                //    int? geoid = ca.Employee.GeoStruct_Id;

                                                //    int? payid = ca.Employee.PayStruct_Id;

                                                //    int? funid = ca.Employee.FuncStruct_Id;

                                                //    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                //    PayStruct paystruct = db.PayStruct.Find(payid);

                                                //    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                //    if (GeoDataInd != null)
                                                //    {
                                                //        foreach (var ca1 in OInsurData)
                                                //        {
                                                //            GenericField100 OGenericObjStatement = new GenericField100()
                                                //            //write data to generic class
                                                //            {
                                                //                Fld1 = ca.Employee.Id.ToString(),
                                                //                Fld2 = ca.Employee == null ? "" : ca.Employee.EmpCode == null ? "" : ca.Employee.EmpCode,
                                                //                Fld3 = ca.Employee == null ? "" : ca.Employee.EmpName == null ? "" : ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML,
                                                //                Fld4 = ca1.InsuranceProduct == null ? "" : ca1.InsuranceProduct.InsuranceProductDesc == null ? "" : ca1.InsuranceProduct.InsuranceProductDesc,
                                                //                Fld5 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                //                Fld6 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                //                Fld7 = ca1.Frequency == null ? "" : ca1.Frequency.LookupVal == null ? "" : ca1.Frequency.LookupVal.ToUpper(),
                                                //                Fld8 = ca1.OperationStatus == null ? "" : ca1.OperationStatus.LookupVal == null ? "" : ca1.OperationStatus.LookupVal.ToUpper(),
                                                //                Fld9 = ca1.PolicyNo == null ? "" : ca1.PolicyNo.ToString(),
                                                //                Fld10 = ca1.Premium.ToString(),
                                                //                Fld11 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Bank != null ? ca.Employee.EmpOffInfo.Bank.Name.ToString() : "",
                                                //            };

                                                //            if (month)
                                                //            {
                                                //                OGenericObjStatement.Fld100 = insurance.PayMonth.ToString();
                                                //            }
                                                //            if (comp)
                                                //            {
                                                //                OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                //            }
                                                //            if (div)
                                                //            {
                                                //                OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                                                //            }
                                                //            if (loca)
                                                //            {
                                                //                OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                                                //            }
                                                //            if (dept)
                                                //            {
                                                //                OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                                                //            }
                                                //            if (grp)
                                                //            {
                                                //                OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                                                //            }
                                                //            if (unit)
                                                //            {
                                                //                OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                                                //            }
                                                //            if (grade)
                                                //            {
                                                //                OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                                                //            }
                                                //            if (lvl)
                                                //            {
                                                //                OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                                                //            }
                                                //            if (jobstat)
                                                //            {
                                                //                OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                //            }
                                                //            if (job)
                                                //            {
                                                //                OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                                                //            }
                                                //            if (jobpos)
                                                //            {
                                                //                OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                                                //            }
                                                //            if (emp)
                                                //            {
                                                //                OGenericObjStatement.Fld88 = ca.Employee.ToString();
                                                //            }
                                                //            //if (Insurancehead)
                                                //            //{
                                                //            //    OGenericObjStatement.Fld87 = ca1.InsuranceProduct.InsuranceProductDesc;
                                                //            //}
                                                //            //if (OperationStatus)
                                                //            //{
                                                //            //    OGenericObjStatement.Fld86 = ca1.OperationStatus.LookupVal.ToUpper();
                                                //            //}
                                                //            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                //        }
                                                //    }
                                                // }
                                            }
                                        }
                                    }

                                }


                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "OFFICIATINGPAYMENT":
                        var OPmonth = mPayMonth.FirstOrDefault();
                        var OofficiatingPay = new List<EmployeePayroll>();
                        var BMSPaymentR = db.BMSPaymentReq.Select(r => new
                        {
                            iEmployeeId = r.EmployeePayroll.Employee.Id,
                            iEmpCode = r.EmployeePayroll.Employee.EmpCode,
                            iEmpName = r.EmployeePayroll.Employee.EmpName.FullNameFML,
                            iPaybank = r.EmployeePayroll.Employee.EmpOffInfo.Bank.Name,
                            iPayMonth = r.PayMonth,
                            iFromPeriod = r.FromPeriod,
                            iToPeriod = r.ToPeriod,
                            iGeoStructId = r.EmployeePayroll.Employee.GeoStruct_Id,
                            iPayStructId = r.EmployeePayroll.Employee.PayStruct_Id,
                            iFuncStructId = r.EmployeePayroll.Employee.FuncStruct_Id,
                            iIsCancel = r.IsCancel,
                            iTrClosed = r.TrClosed,
                            iofficiatingpayment = r.OfficiatingPaymentT.Select(v => new
                            {
                                SalaryHead = v.SalaryHead,
                                SalHeadAmount = v.SalHeadAmount,
                                SalaryHeadType = v.SalaryHead.Type

                            }).ToList()

                        }).Where(e => EmpPayrollIdList.Contains(e.iEmployeeId) && e.iPayMonth == OPmonth && e.iIsCancel == false && e.iTrClosed == true ).ToList();

                        if (BMSPaymentR != null)
                        {

                        }

                        if (BMSPaymentR == null || BMSPaymentR.Count() == 0)
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

                            foreach (var ca in BMSPaymentR)
                            {
                                int? geoid = ca.iGeoStructId;

                                int? payid = ca.iPayStructId;

                                int? funid = ca.iFuncStructId;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                double totalDays = (ca.iToPeriod.Value.Date - ca.iFromPeriod.Value.Date).TotalDays + 1;

                                var Oofficiatingpay = ca.iofficiatingpayment.ToList();

                                if (Oofficiatingpay != null && Oofficiatingpay.Count() != 0)
                                {

                                    foreach (var item in ca.iofficiatingpayment.Where(e => e.SalaryHead.InPayslip == true))
                                    {

                                        // if (item.SalaryHead.InPayslip == true)
                                        //{


                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {

                                            Fld2 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                            Fld3 = ca.iEmpName == null ? "" : ca.iEmpName,
                                            Fld4 = ca.iPayMonth == null ? "" : ca.iPayMonth,
                                            Fld6 = ca.iFromPeriod.Value.ToShortDateString(),
                                            Fld7 = ca.iToPeriod.Value.ToShortDateString(),
                                            Fld8 = totalDays.ToString(),

                                            Fld44 = item.SalaryHead.Name.ToString(),
                                            Fld45 = item.SalHeadAmount.ToString(),
                                            Fld46 = item.SalaryHeadType.LookupVal,

                                            Fld48 = ca.iPaybank == null ? "" : ca.iPaybank.ToString()

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
                                        // }

                                    }

                                }

                            }

                        }

                        return OGenericPayrollStatement;

                        break;

                    case "EARNINGSTATEMENT":



                        var OSalHead1 = db.CompanyPayroll
                            //.Include(e => e.SalaryHead)
                            //.Include(e => e.SalaryHead.Select(r => r.Type)).AsNoTracking()
                                           .Where(d => d.Id == CompanyPayrollId).AsParallel().FirstOrDefault();

                        if (OSalHead1 != null)
                        {
                            OSalHead1.SalaryHead = db.CompanyPayroll.Where(e => e.Id == OSalHead1.Id).Select(r => r.SalaryHead.ToList()).FirstOrDefault();
                            foreach (var i in OSalHead1.SalaryHead)
                            {
                                i.Type = db.LookupValue.Where(e => e.Id == i.Type_Id).FirstOrDefault();
                            }

                        }
                        var OEmpDataEarning = new List<EmployeePayroll>();

                        foreach (var item in EmpPayrollIdList)
                        {

                            var OEmpData_t = db.EmployeePayroll
                                //.Include(e => e.SalaryT)
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                              .Include(e => e.Employee).AsNoTracking()
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)
                              .Where(e => e.Employee.Id == item).OrderBy(e => e.Id).FirstOrDefault();



                            if (OEmpData_t != null)
                            {
                                OEmpData_t.Employee.EmpName = db.NameSingle.Where(e => e.Id == OEmpData_t.Employee.EmpName_Id).FirstOrDefault();
                                OEmpData_t.SalaryT = db.SalaryT.Where(g => g.PayMonth == mPayMonth.FirstOrDefault() && g.EmployeePayroll_Id == OEmpData_t.Id).ToList();
                                foreach (var i in OEmpData_t.SalaryT)
                                {
                                    var SalEarnDedT = db.SalaryT.Where(e => e.Id == i.Id).Select(r => r.SalEarnDedT.Where(y => y.Amount != 0 && y.SalaryHead.InPayslip == true && y.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").ToList()).FirstOrDefault();
                                    i.SalEarnDedT = SalEarnDedT;
                                    foreach (var j in i.SalEarnDedT)
                                    {
                                        var SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).AsNoTracking().FirstOrDefault();
                                        var SalaryHeadType = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).Select(t => t.Type).FirstOrDefault();
                                        var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).Select(t => t.SalHeadOperationType).FirstOrDefault();
                                        j.SalaryHead = SalaryHead;
                                        j.SalaryHead.Type = SalaryHeadType;
                                        j.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    }
                                }


                                OEmpDataEarning.Add(OEmpData_t);
                            }
                        }
                        var OSalTchk = OEmpDataEarning.Select(e => e.SalaryT).ToList();
                        // var mHeader = false;
                        if (OEmpDataEarning.Count() == 0 || OSalHead1 == null || OSalTchk == null)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var loanhead = false;
                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "EarningHead")
                                {
                                    loanhead = true;
                                }
                            }
                            var SortingField = "";
                            if (Sorting.ToUpper() == "VERTICAL")
                            {
                                SortingField = "VERTICAL";
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

                            foreach (var OEmpsingle in OEmpDataEarning)
                            {
                                //int geoid = OEmpsingle.Employee.GeoStruct.Id;

                                //int payid = OEmpsingle.Employee.PayStruct.Id;

                                //int funid = OEmpsingle.Employee.FuncStruct.Id;

                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                double paiddays = OEmpsingle.SalaryT.Select(e => e.PaybleDays).SingleOrDefault();
                                int? geoid = OEmpsingle.Employee.GeoStruct_Id;

                                int? payid = OEmpsingle.Employee.PayStruct_Id;

                                int? funid = OEmpsingle.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                var OEarnDedData = OEmpsingle.SalaryT
                                         .Select(e => e.SalEarnDedT
                                        .Select(r => new
                                        {
                                            emppayrollid = OEmpsingle.Id,
                                            empid = OEmpsingle.Employee.Id,
                                            empcode = OEmpsingle.Employee.EmpCode,
                                            empname = OEmpsingle.Employee.EmpName.FullNameFML,
                                            paymonth = e.PayMonth,
                                            //geoid = e.Geostruct.Id,
                                            //loccode = e.Geostruct.Location.LocationObj.LocCode,
                                            //locname = e.Geostruct.Location.LocationObj.LocDesc,
                                            //payid = e.PayStruct.Id,
                                            //GradeName = e.PayStruct.Grade.Name,
                                            //funcid = e.FuncStruct.Id,
                                            //  jobcode = e.FuncStruct.Job.Code,
                                            // jobname = e.FuncStruct.Job.Name,
                                            salcodeid = r.SalaryHead.Id,
                                            salcode = r.SalaryHead.Code,
                                            salname = r.SalaryHead.Name,
                                            amount = r.Amount,
                                            //saltype = r.SalaryHead.Type,
                                            //salheadseqno = r.SalaryHead.SeqNo,
                                            //salOptype = r.SalaryHead.SalHeadOperationType,

                                        }
                                        )).SingleOrDefault();

                                if (OEarnDedData != null)
                                {
                                    if (salheadlist != null && salheadlist.Count > 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            var items = item.Split('-');
                                            string salcode = items[1];
                                            string salname = items[0];
                                            var OEarnData = OEarnDedData.Where(a => a.salcode.ToString() == salcode).ToList();
                                            if (OEarnData.Count > 0)
                                            {
                                                foreach (var ca in OEarnData)
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {
                                                        //Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                        Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                        Fld3 = ca.empname == null ? "" : ca.empname,
                                                        //Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                        //Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                        //Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                        //Fld7 = ca.locname == null ? "" : ca.locname,                                                        //Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,

                                                        Fld9 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                        //Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                        //Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                        //Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                        //Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                        //Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                        Fld15 = ca.salname == null ? "" : ca.salname,
                                                        Fld16 = ca.amount.ToString(),
                                                        //Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                        //Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                        //Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                        Fld50 = SortingField,
                                                        Fld51 = paiddays.ToString(),
                                                    };
                                                    //if (month)
                                                    //{
                                                    //    OEarnStatement.Fld100 = ca.PayMonth.ToString();
                                                    //}
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Department_Name;
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
                                                        OGenericObjStatement.Fld88 = ca.empname;
                                                    }
                                                    if (loanhead)
                                                    {
                                                        OGenericObjStatement.Fld86 = ca.salcode;
                                                    }
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
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
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {
                                                    //Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                    Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                    Fld3 = ca.empname == null ? "" : ca.empname,
                                                    //Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                    //Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                    //Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                    //Fld7 = ca.locname == null ? "" : ca.locname,
                                                    //Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                    Fld9 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                    //Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                    //Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                    //Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                    //Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                    //Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                    Fld15 = ca.salname == null ? "" : ca.salname,
                                                    Fld16 = ca.amount.ToString(),
                                                    //Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                    //Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                    //Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                    Fld50 = SortingField,
                                                    Fld51 = paiddays.ToString(),
                                                };
                                                //if (month)
                                                //{
                                                //    OEarnStatement.Fld100 = ca.PayMonth.ToString();
                                                //}
                                                if (comp)
                                                {
                                                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Department_Name;
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
                                                    OGenericObjStatement.Fld88 = ca.empname;
                                                }
                                                if (loanhead)
                                                {
                                                    OGenericObjStatement.Fld86 = ca.salcode;
                                                }
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                    }
                                }

                            }
                            return OGenericPayrollStatement.ToList();
                        }
                        break;


                    case "DEDUCTIONSTATEMENT":

                        var OSaldedDED = db.CompanyPayroll
                            //.Include(e => e.SalaryHead)
                            //.Include(e => e.SalaryHead.Select(r => r.Type)).AsNoTracking()
                       .Where(d => d.Id == CompanyPayrollId).AsNoTracking().FirstOrDefault();
                        if (OSaldedDED != null)
                        {
                            OSaldedDED.SalaryHead = db.CompanyPayroll.Where(e => e.Id == OSaldedDED.Id).Select(r => r.SalaryHead.ToList()).AsNoTracking().FirstOrDefault();
                            foreach (var i in OSaldedDED.SalaryHead)
                            {
                                i.Type = db.LookupValue.Where(e => e.Id == i.Type_Id).AsNoTracking().FirstOrDefault();
                            }

                        }

                        var OEmpdedDED = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OEmpdedData_t = db.EmployeePayroll
                                //.Include(e => e.SalaryT)
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.Type)))
                                //.Include(e => e.SalaryT.Select(t => t.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)))
                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                            .Where(e => e.Employee.Id == item).AsNoTracking().FirstOrDefault();


                            if (OEmpdedData_t != null)
                            {
                                OEmpdedData_t.Employee.EmpName = db.NameSingle.Where(e => e.Id == OEmpdedData_t.Employee.EmpName_Id).AsNoTracking().FirstOrDefault();
                                OEmpdedData_t.SalaryT = db.SalaryT.Where(g => g.PayMonth == mPayMonth.FirstOrDefault() && g.EmployeePayroll_Id == OEmpdedData_t.Id).AsNoTracking().ToList();
                                foreach (var i in OEmpdedData_t.SalaryT)
                                {
                                    var SalEarnDedT = db.SalaryT.Where(e => e.Id == i.Id).Select(b => b.SalEarnDedT.Where(y => y.Amount != 0 && y.SalaryHead.InPayslip == true && y.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").ToList()).AsNoTracking().FirstOrDefault();
                                    i.SalEarnDedT = SalEarnDedT;
                                    foreach (var j in i.SalEarnDedT)
                                    {
                                        var SalaryHead = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).AsNoTracking().FirstOrDefault();
                                        var SalaryHeadType = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).Select(c => c.Type).AsNoTracking().FirstOrDefault();
                                        // var SalHeadOperationType = db.SalaryHead.Where(e => e.Id == j.SalaryHead_Id).Select(c => c.SalHeadOperationType).FirstOrDefault();
                                        j.SalaryHead = SalaryHead;
                                        j.SalaryHead.Type = SalaryHeadType;
                                        //  j.SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                    }

                                }
                                OEmpdedDED.Add(OEmpdedData_t);
                            }
                        }
                        //  var OSalTdedchk = OEmpdedData.Select(e => e.SalaryT).ToList();
                        //var mHeader = false;
                        if (OEmpdedDED.Count() == 0 || OSaldedDED == null || OSaldedDED == null)
                        {
                            return null;
                        }
                        else
                        {
                            var SpGroup = SpecialGroupslist.ToList();
                            var loanhead = false;
                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "DeductionHead")
                                {
                                    loanhead = true;
                                }
                            }
                            var SortingField = "";
                            if (Sorting.ToUpper() == "VERTICAL")
                            {
                                SortingField = "VERTICAL";
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


                            foreach (var OEmpsingle in OEmpdedDED)
                            {
                                double paiddays = OEmpsingle.SalaryT.Select(e => e.PaybleDays).SingleOrDefault();
                                int? geoid = OEmpsingle.Employee.GeoStruct_Id;

                                int? payid = OEmpsingle.Employee.PayStruct_Id;

                                int? funid = OEmpsingle.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                var OEarnDedData = OEmpsingle.SalaryT
                                         .Select(e => e.SalEarnDedT
                                        .Select(r => new
                                        {
                                            emppayrollid = OEmpsingle.Id,
                                            empid = OEmpsingle.Employee.Id,
                                            empcode = OEmpsingle.Employee.EmpCode,
                                            empname = OEmpsingle.Employee.EmpName.FullNameFML,
                                            //paymonth = e.PayMonth,
                                            //GEOID = e.Geostruct.Id,
                                            //  loccode = e.Geostruct.Location.LocationObj.LocCode,
                                            // locname = e.Geostruct.Location.LocationObj.LocDesc,
                                            // Gradecode = e.PayStruct.Grade.Code,
                                            //PAYID = e.PayStruct.Id,
                                            //FUNID = e.FuncStruct.Id,
                                            // jobcode = e.FuncStruct.Job.Code,
                                            // jobname = e.FuncStruct.Job.Name,
                                            salcodeid = r.SalaryHead.Id,
                                            salcode = r.SalaryHead.Code,
                                            salname = r.SalaryHead.Name,
                                            amount = r.Amount,
                                            //saltype = r.SalaryHead.Type,
                                            //salheadseqno = r.SalaryHead.SeqNo,
                                            //salOptype = r.SalaryHead.SalHeadOperationType,
                                        }
                                        )).SingleOrDefault();
                                if (OEarnDedData != null)
                                {
                                    if (salheadlist != null && salheadlist.Count() > 0)
                                    {
                                        foreach (var item in salheadlist)
                                        {
                                            var items = item.Split('-');
                                            string salname = items[0];
                                            string salcode = items[1];
                                            var OEarnData = OEarnDedData.Where(a => a.salcode.ToString() == salcode && a.amount != 0).ToList();
                                            if (OEarnData.Count > 0)
                                            {
                                                foreach (var ca in OEarnData)
                                                {

                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                        Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                        Fld3 = ca.empname == null ? "" : ca.empname,
                                                        //Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                        // Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                        // Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                        // Fld7 = GeoDataInd == null ? "" : GeoDataInd.LocDesc,
                                                        //  Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                        Fld9 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                        // Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                        //  Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                        //  Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                        //Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                        //Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                        Fld15 = ca.salname == null ? "" : ca.salname,
                                                        Fld16 = ca.amount.ToString(),
                                                        //Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                        //Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                        //Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                        Fld50 = SortingField,
                                                        Fld51 = paiddays.ToString(),
                                                    };
                                                    //if (month)
                                                    //{
                                                    //    OGenericObjStatement.Fld100 = ca.PayMonth.ToString();
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
                                                        OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name; ;
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
                                                        OGenericObjStatement.Fld88 = ca.empname;
                                                    }
                                                    if (loanhead)
                                                    {
                                                        OGenericObjStatement.Fld86 = ca.salname;
                                                    }
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
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
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {
                                                    //Fld1 = ca.empid == null ? "" : ca.empid.ToString(),
                                                    Fld2 = ca.empcode == null ? "" : ca.empcode,
                                                    Fld3 = ca.empname == null ? "" : ca.empname,
                                                    //Fld4 = ca.paymonth == null ? "" : ca.paymonth,
                                                    //   Fld5 = ca.locid == null ? "" : ca.locid.ToString(),
                                                    //  Fld6 = ca.loccode == null ? "" : ca.loccode,
                                                    //   Fld7 = ca.locname == null ? "" : ca.locname,
                                                    //  Fld8 = ca.Gradecode == null ? "" : ca.Gradecode,
                                                    Fld9 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                    //Fld10 = ca.jobid == null ? "" : ca.jobid.ToString(),
                                                    // Fld11 = ca.jobcode == null ? "" : ca.jobcode,
                                                    //  Fld12 = ca.jobname == null ? "" : ca.jobname,
                                                    //Fld13 = ca.salcodeid == null ? "" : ca.salcodeid.ToString(),
                                                    //Fld14 = ca.salcode == null ? "" : ca.salcode,
                                                    Fld15 = ca.salname == null ? "" : ca.salname,
                                                    Fld16 = ca.amount.ToString(),
                                                    //Fld17 = ca.saltype == null ? "" : ca.saltype.ToString(),
                                                    //Fld18 = ca.salheadseqno == null ? "" : ca.salheadseqno.ToString(),
                                                    //Fld19 = ca.salOptype == null ? "" : ca.salOptype.ToString(),
                                                    Fld50 = SortingField,
                                                    Fld51 = paiddays.ToString(),
                                                };
                                                //if (month)
                                                //{
                                                //    ODedStatement.Fld100 = ca.PayMonth.ToString();
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
                                                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name; ;
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
                                                    OGenericObjStatement.Fld88 = ca.empname;
                                                }
                                                if (loanhead)
                                                {
                                                    OGenericObjStatement.Fld86 = ca.salname;
                                                }
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement.ToList();
                        }
                        break;

                    case "JVREPORTDETAILBRANCHWISE":
                        //List<CompanyPayroll> OJVData = new List<CompanyPayroll>();

                        var OJVData = new CompanyPayroll();

                        //OJVData = db.CompanyPayroll
                        //.Include(e => e.JVProcessData)
                        //.Include(e => e.JVProcessData.Select(r => r.SalaryHead))
                        //.Include(e => e.JVProcessData.Select(r => r.JVParameter))
                        //.Include(e => e.JVProcessData.Select(r => r.JVParameter.JVGroup))
                        //.Include(e => e.JVProcessData.Select(r => r.JVParameter.SalaryHead))
                        //.Include(e => e.JVProcessDataSummary).AsNoTracking()
                        //.Where(e => e.Company.Id == CompanyId)
                        //.ToList();

                        //if (OJVData_temp != null)
                        //{
                        //    OJVData.Add(OJVData_temp);
                        //}

                        var paymonthJV = mPayMonth.FirstOrDefault();

                        OJVData = db.CompanyPayroll.Where(e => e.Company.Id == CompanyId).AsNoTracking().FirstOrDefault();
                        List<string> JVParameter2 = new List<string>();
                        if (OJVData != null)
                        {
                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                            {
                                JVParameter2.AddRange(salheadlist);

                                foreach (string ca1 in JVParameter2)
                                {
                                    List<JVProcessData> JVProcessData = db.CompanyPayroll.Where(e => e.Id == CompanyPayrollId).Select(r => r.JVProcessData.Where(e => salheadlist.Contains(e.BatchName.ToString()) && e.ProcessMonth == paymonthJV && e.BatchName.Contains(ca1)).ToList()).AsNoTracking().FirstOrDefault();
                                    foreach (var i in JVProcessData)
                                    {
                                        i.SalaryHead = db.SalaryHead.Find(i.SalaryHead_Id);
                                        i.JVParameter = db.JVParameter.Find(i.JVParameter_Id);
                                        i.JVParameter.JVGroup = db.LookupValue.Find(i.JVParameter.JVGroup_Id);
                                        i.JVParameter.SalaryHead = db.SalaryHead.Where(e => e.Id == i.SalaryHead_Id).AsNoTracking().ToList();

                                    }

                                    OJVData.JVProcessData = JVProcessData;
                                    OJVData.JVProcessDataSummary = db.JVProcessDataSummary.Where(e => salheadlist.Contains(e.BatchName) && e.ProcessMonth == paymonthJV.ToString()).AsNoTracking().ToList();
                                }
                            }
                        }
                        OGenericPayrollStatement = new List<GenericField100>();

                        if (OJVData == null)
                        {
                            return null;
                        }
                        else
                        {
                            //    foreach (var ca in OJVData)
                            //    {
                            //        var paymon = mPayMonth.FirstOrDefault();
                            //        List<JVProcessData> OJVReportData = ca.JVProcessData.Where(e => e.ProcessMonth == paymon).ToList();
                            List<JVProcessData> OJVReportData = OJVData.JVProcessData.ToList();

                            if (OJVReportData != null && OJVReportData.Count() != 0)
                            {
                                var JVPARACODELIST = db.JVParameter.AsNoTracking().ToList();

                                // var batchnames = OJVReportData.Select(q => q.BatchName).Distinct().ToList();
                                var alllocations = OJVReportData.Select(q => q.BranchCode).Distinct().ToList();

                                var SelectedBatch = "";
                                if (salheadlist != null && salheadlist.Count() != 0)
                                {
                                    foreach (var item1 in salheadlist)
                                    {
                                        SelectedBatch = item1;
                                    }
                                }
                                else
                                {
                                    var batchnames = OJVReportData.Select(q => q.BatchName).Distinct().ToList();
                                    foreach (var item2 in batchnames)
                                    {
                                        SelectedBatch = item2;
                                    }
                                }
                                //foreach (var batch in OJVReportData.Where(e => e.BatchName == item1).Select(q => q.BatchName).Distinct().ToList())
                                //{

                                var jvdataforperticularJVProductCode = OJVReportData
                                    .Where(q => q.BatchName == SelectedBatch
                              && (q.JVParameter.JVGroup.LookupVal.ToUpper() == "LOCATION" || q.JVParameter.JVGroup.LookupVal.ToUpper() == "COMPANY" || q.JVParameter.JVGroup.LookupVal.ToUpper() == "PAYMENTBANK")
                              ).GroupBy(q => q.JVParameter.JVProductCode).ToList();

                                foreach (var item in jvdataforperticularJVProductCode)
                                {
                                    //var checkbranch = ;
                                    foreach (var EachBranchCode in item.GroupBy(q => q.BranchCode).ToList())
                                    {
                                        // var AcCodeList = ;
                                        foreach (var EachAccCode in EachBranchCode.GroupBy(a => a.AccountCode).ToList())
                                        {
                                            foreach (var EachRecord in EachAccCode)
                                            {
                                                string credit = "";
                                                string debit = "";
                                                if (EachRecord.CreditDebitFlag == "C")
                                                {
                                                    credit = EachRecord.TransactionAmount;
                                                    debit = "0";
                                                }
                                                else
                                                {
                                                    credit = "0";
                                                    debit = EachRecord.TransactionAmount;
                                                }
                                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                {
                                                    Fld1 = EachRecord.JVParameter.JVProductCode,
                                                    Fld2 = EachRecord.SalaryHead.Name,
                                                    Fld3 = EachRecord.BranchCode == null ? "" : EachRecord.BranchCode.ToString(),
                                                    // Fld4 = item.JVParameter.AccountNo == null ? "" : item.JVParameter.AccountNo.ToString(),
                                                    Fld4 = EachRecord.AccountCode == null ? "" : EachRecord.AccountCode,
                                                    Fld5 = credit,
                                                    Fld6 = debit,
                                                    Fld7 = SelectedBatch,
                                                    Fld8 = paymonthJV,
                                                    Fld9 = EachRecord.JVParameter.JVName
                                                };
                                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                            }


                                        }
                                        //}
                                    }
                                }
                            }
                            //  }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "BRANCHWISEJV":

                        string paymonth = mPayMonth.FirstOrDefault();
                        List<JVProcessData> OJVData_temp2 = db.JVProcessData
                            //.Include(e => e.JVProcessData)
                            //.Include(e => e.JVProcessData.Select(r => r.SalaryHead))
                            //.Include(e => e.JVProcessData.Select(r => r.JVParameter))
                            //.Include(e => e.JVProcessData.Select(r => r.JVParameter.JVGroup))
                            //.Include(e => e.JVProcessData.Select(r => r.JVParameter.SalaryHead))
                            //.Include(e => e.JVProcessDataSummary)
                        .AsNoTracking()
                        .Where(e => e.ProcessMonth == paymonth && e.CreditDebitFlag == "C")
                        .ToList();



                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OJVData_temp2.Count == 0)
                        {
                            return null;
                        }
                        else
                        {
                            List<JvDataForBranchWise> JvDataForBranchWiseList = new List<JvDataForBranchWise>();

                            var Allbranch = db.Location
                                .Include(q => q.LocationObj).AsNoTracking()
                                .Where(q => q.LocationObj.LocCode != "EZ"
                                    && q.LocationObj.LocCode != "WZ"
                                    && q.LocationObj.LocCode != "NZ"
                                    && q.LocationObj.LocCode != "GZ"
                                    && q.LocationObj.LocCode != "CZ"
                            )
                                .Select(q => q.LocationObj.LocCode).OrderBy(q => q).ToList();

                            var jvparmSalheadList = db.JVParameter
                                            .Include(q => q.SalaryHead)
                                            .Include(q => q.JVGroup)
                                            .AsNoTracking().Where(q => q.JVGroup.LookupVal.ToUpper() == "COMPANY" && q.CreditDebitFlag == "C")
                                            .SelectMany(q => q.SalaryHead).ToList();

                            List<Utility.ReportClass> GetQueryData = new List<Utility.ReportClass>();
                            var query1 = "select LocCode,Code,EarnAmount from QLocEarnDedSummary where paymonth=" + "'" + paymonth + "'";
                            if (query1 != "")
                            {
                                GetQueryData = db.Database.SqlQuery<Utility.ReportClass>(query1).ToList<Utility.ReportClass>();
                            }

                            var QueryDataOfBranch1 = GetQueryData.Where(q => q.LocCode == "099" && q.Code == "WF").Sum(q => q.EarnAmount);

                            var ForPerti11 = new JvDataForBranchWise
                            {
                                BranchCodeInward = "099",
                                NarrationInward = "WF",
                                AmountInward = QueryDataOfBranch1.ToString(),
                                Flag = "Inword",
                                LocationCode = "039"
                            };
                            JvDataForBranchWiseList.Add(ForPerti11);

                            foreach (var eachbranch in Allbranch)
                            {
                                var BranchcodewiseInward = OJVData_temp2
                                    .Where(q => q.BranchCode == eachbranch
                                    && !q.Narration.Contains(":" + paymonth + "-Org_Br_" + eachbranch)).ToList();


                                foreach (var eachrecofjv in BranchcodewiseInward)
                                {
                                    var ForPerti = new JvDataForBranchWise
                                    {
                                        BranchCodeInward = eachrecofjv.BranchCode,
                                        NarrationInward = eachrecofjv.Narration,
                                        AmountInward = eachrecofjv.TransactionAmount,
                                        Flag = "Inword",
                                        LocationCode = eachbranch
                                    };
                                    JvDataForBranchWiseList.Add(ForPerti);
                                }

                                var BranchcodewiseOutward = OJVData_temp2
                                   .Where(q => q.BranchCode != eachbranch
                                   && q.Narration.Contains(":" + paymonth + "-Org_Br_" + eachbranch)).ToList();


                                foreach (var eachrecofjv in BranchcodewiseOutward)
                                {
                                    var ForPerti = new JvDataForBranchWise
                                    {
                                        BranchCodeOutward = eachrecofjv.BranchCode,
                                        NarrationOutward = eachrecofjv.Narration,
                                        AmountOutward = eachrecofjv.TransactionAmount,
                                        Flag = "Outword",
                                        LocationCode = eachbranch
                                    };
                                    JvDataForBranchWiseList.Add(ForPerti);
                                }


                                foreach (var item in jvparmSalheadList)
                                {
                                    var QueryDataOfBranch = GetQueryData.Where(q => q.LocCode == eachbranch && q.Code == item.Code).Sum(q => q.EarnAmount);

                                    var ForPerti = new JvDataForBranchWise
                                    {
                                        BranchCodeOutward = eachbranch,
                                        NarrationOutward = item.Code,
                                        AmountOutward = QueryDataOfBranch.ToString(),
                                        Flag = "Outword",
                                        LocationCode = eachbranch
                                    };
                                    JvDataForBranchWiseList.Add(ForPerti);
                                }

                            }


                            foreach (var EachRecord in JvDataForBranchWiseList.OrderBy(q => q.LocationCode))
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = EachRecord.BranchCodeInward,
                                    Fld2 = EachRecord.NarrationInward,
                                    Fld3 = EachRecord.AmountInward,
                                    Fld4 = EachRecord.Flag,
                                    Fld5 = EachRecord.LocationCode,
                                    Fld6 = EachRecord.BranchCodeOutward,
                                    Fld7 = EachRecord.NarrationOutward,
                                    Fld8 = EachRecord.AmountOutward
                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "JVSUMMARY":
                        //List<CompanyPayroll> OJVDetails = new List<CompanyPayroll>();
                        var paymon = mPayMonth.FirstOrDefault();
                        //CompanyPayroll OJVDetails_temp = db.CompanyPayroll.Include(e=>e.Company)
                        //    .Include(e => e.JVProcessData)
                        //    .Include(e => e.JVProcessData.Select(r => r.SalaryHead))
                        //.Where(e => e.Company.Id == CompanyId).AsNoTracking().AsParallel()
                        //.FirstOrDefault();

                        //List<string> JVParameter = new List<string>();

                        //if (OJVDetails_temp != null)
                        //{
                        //    if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //    {
                        //        JVParameter.AddRange(salheadlist);

                        //        foreach (string ca1 in JVParameter)
                        //        {
                        //            OJVDetails_temp.JVProcessData = db.JVProcessData
                        //                .Where(e => e.CompanyPayroll_Id == OJVDetails_temp.Id && e.ProcessMonth == paymon && e.BatchName.Contains(ca1)).AsNoTracking().ToList();
                        //            if (OJVDetails_temp.JVProcessData != null)
                        //            {
                        //                foreach (var i in OJVDetails_temp.JVProcessData)
                        //                {
                        //                    i.SalaryHead = db.SalaryHead.Find(i.SalaryHead_Id);

                        //                }

                        //                OJVDetails.Add(OJVDetails_temp);

                        //            }

                        //        }
                        //    }

                        //    else
                        //    {
                        //        OJVDetails_temp.JVProcessData = db.JVProcessData.Where(e => e.CompanyPayroll_Id == OJVDetails_temp.Id && e.ProcessMonth == paymon).AsNoTracking().ToList();

                        //        if (OJVDetails_temp.JVProcessData != null)
                        //        {
                        //            foreach (var i in OJVDetails_temp.JVProcessData)
                        //            {
                        //                i.SalaryHead = db.SalaryHead.Find(i.SalaryHead_Id);

                        //            }

                        //            OJVDetails.Add(OJVDetails_temp);

                        //        }
                        //    }

                        //}

                        List<GenericField100> JVSummaryGenericField100 = new List<GenericField100>();

                        var OJVDetails = db.CompanyPayroll.Select(d => new
                        {
                            OCompanyId = d.Company.Id,
                            OJVProcessData = d.JVProcessData.Select(r => new
                            {
                                OProcessMonth = r.ProcessMonth,
                                OSalaryHeadName = r.SalaryHead.Name,
                                OBatchName = r.BatchName,
                                OCreditDebitFlag = r.CreditDebitFlag,
                                OTransactionAmount = r.TransactionAmount
                            }).Where(a => a.OProcessMonth == paymon).ToList()
                        }).Where(e => e.OCompanyId == CompanyId).SingleOrDefault();

                        //if (OJVDetails == null || OJVDetails.Count() == 0)
                        //{
                        //    return null;
                        //}
                        if (OJVDetails == null)
                        {
                            return null;
                        }
                        else
                        {
                            double cr = 0;
                            double dr = 0;
                            //foreach (var ca in OJVDetails)
                            //{
                            //List<JVProcessData> OJVReportData = ca.JVProcessData.ToList();
                            var OJVReportData = OJVDetails.OJVProcessData.ToList();
                            if (salheadlist.Count() > 0)
                            {
                                OJVReportData = OJVReportData.Where(e => salheadlist.Contains(e.OBatchName)).ToList();
                            }
                            else
                            {
                                OJVReportData = OJVDetails.OJVProcessData.ToList();
                            }
                            if (OJVReportData != null && OJVReportData.Count() != 0)
                            {
                                //var batchnames = OJVReportData.Select(q => q.BatchName).Distinct().ToList();
                                var batchnames = OJVReportData.Select(q => q.OBatchName).Distinct().ToList();
                                foreach (var batch in batchnames)
                                {
                                    var jvdataforperticularbatch = OJVReportData.Where(q => q.OBatchName == batch).ToList();
                                    foreach (var item in jvdataforperticularbatch)
                                    {

                                        string credit = "";
                                        string debit = "";
                                        if (item.OCreditDebitFlag == "C")
                                        {
                                            credit = item.OTransactionAmount;
                                            debit = "0";
                                            cr = cr + Convert.ToDouble(credit);
                                        }
                                        else
                                        {
                                            credit = "0";
                                            debit = item.OTransactionAmount;
                                            dr = dr + Convert.ToDouble(debit);
                                        }



                                        GenericField100 GenericGeoObjStatement = new GenericField100()
                                        {
                                            //Fld2 = item.SalaryHead == null ? "" : item.SalaryHead.Name.ToString(),
                                            Fld2 = item.OSalaryHeadName == null ? "" : item.OSalaryHeadName,
                                            Fld5 = credit,
                                            Fld6 = debit,
                                            Fld7 = batch,
                                            Fld8 = paymon

                                        };
                                        JVSummaryGenericField100.Add(GenericGeoObjStatement);

                                    }

                                }
                            }
                            //}
                            return JVSummaryGenericField100;
                        }

                        break;


                    case "JVREPORTDETAILINDIVIDUAL":
                        // List<CompanyPayroll> OJVData1 = new List<CompanyPayroll>();

                        //OJVData1 = db.CompanyPayroll
                        //.Include(e => e.JVProcessData)
                        //.Include(e => e.JVProcessData.Select(r => r.JVParameter))
                        //.Include(e => e.JVProcessData.Select(r => r.SalaryHead))
                        //.Include(e => e.JVProcessData.Select(r => r.JVParameter.JVGroup))
                        //.Include(e => e.JVProcessData.Select(r => r.JVParameter.SalaryHead))
                        //.Include(e => e.JVProcessDataSummary).AsNoTracking()
                        //.Where(e => e.Company.Id == CompanyId)
                        //.ToList();

                        //if (OJVData_temp1 != null)
                        //{
                        //    OJVData1.Add(OJVData_temp1);
                        //}

                        var OJVDat = new CompanyPayroll();
                        var paymonthJ = mPayMonth.FirstOrDefault();
                        OJVDat = db.CompanyPayroll.Where(e => e.Company.Id == CompanyId).AsNoTracking().FirstOrDefault();
                        List<string> JVParameter1 = new List<string>();
                        if (OJVDat != null)
                        {
                            if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                            {
                                JVParameter1.AddRange(salheadlist);
                                foreach (string ca1 in JVParameter1)
                                {
                                    List<JVProcessData> JVProcessDat = db.CompanyPayroll.Where(e => e.Id == CompanyPayrollId).Select(r => r.JVProcessData.Where(e => salheadlist.Contains(e.BatchName.ToString()) && e.ProcessMonth == paymonthJ && e.BatchName.Contains(ca1)).ToList()).AsNoTracking().FirstOrDefault();

                                    foreach (var i in JVProcessDat)
                                    {
                                        i.SalaryHead = db.SalaryHead.Find(i.SalaryHead_Id);
                                        i.JVParameter = db.JVParameter.Find(i.JVParameter_Id);
                                        i.JVParameter.JVGroup = db.LookupValue.Find(i.JVParameter.JVGroup_Id);
                                        i.JVParameter.SalaryHead = db.SalaryHead.Where(e => e.Id == i.SalaryHead_Id).AsNoTracking().ToList();

                                    }
                                    OJVDat.JVProcessData = JVProcessDat;
                                    OJVDat.JVProcessDataSummary = db.JVProcessDataSummary.Where(e => salheadlist.Contains(e.BatchName) && e.ProcessMonth == paymonthJ.ToString()).AsNoTracking().ToList();
                                }
                            }
                        }



                        OGenericPayrollStatement = new List<GenericField100>();
                        if (OJVDat == null)
                        {
                            return null;
                        }
                        else
                        {
                            //foreach (var ca in OJVData)
                            //{
                            //    var paymon = mPayMonth.FirstOrDefault();
                            //    List<JVProcessData> OJVReportData = ca.JVProcessData.Where(e => e.ProcessMonth == paymon).ToList();

                            List<JVProcessData> OJVReportData = OJVDat.JVProcessData.ToList();

                            if (OJVReportData != null && OJVReportData.Count() != 0)
                            {

                                var JVPARACODELIST = db.JVParameter.Select(q => q.JVProductCode).Distinct().ToList();
                                // var batchnames = OJVReportData.Select(q => q.BatchName).Distinct().ToList();
                                //var batchnames = OJVReportData.Where(e => e.BatchName == item).Select(q => q.BatchName).Distinct().ToList();
                                var alllocations = OJVReportData.Select(q => q.BranchCode).Distinct().ToList();

                                var EmpLoanlIST = db.LoanAdvRequest
                                               .Include(e => e.EmployeePayroll)
                                    //.Include(e => e.LoanAdvanceHead)
                                    //.Include(e => e.LoanAccBranch)
                                    //.Include(e => e.LoanAccBranch.LocationObj)
                                    // .Include(e => e.LoanAdvanceHead.SalaryHead)
                                .Include(e => e.EmployeePayroll.Employee)
                                    //.Include(e => e.EmployeePayroll.Employee.EmpName)
                                    //.Include(e => e.EmployeePayroll.Employee.GeoStruct)
                                    //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                                    //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                                    //.Include(e => e.LoanAdvanceHead)
                                .AsNoTracking().ToList();


                                Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();
                                foreach (var itemLOAN in EmpLoanlIST)
                                {

                                    int? geoid = itemLOAN.EmployeePayroll.Employee.GeoStruct_Id;

                                    int? payid = itemLOAN.EmployeePayroll.Employee.PayStruct_Id;

                                    int? funid = itemLOAN.EmployeePayroll.Employee.FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    itemLOAN.LoanAdvanceHead = db.LoanAdvanceHead.Find(itemLOAN.LoanAdvanceHead_Id);
                                    itemLOAN.LoanAccBranch = db.Location.Find(itemLOAN.LoanAccBranch_Id);
                                    itemLOAN.LoanAccBranch.LocationObj = db.LocationObj.Find(itemLOAN.LoanAccBranch.LocationObj_Id);
                                    itemLOAN.LoanAdvanceHead.SalaryHead = db.SalaryHead.Find(itemLOAN.LoanAdvanceHead.SalaryHead_Id);
                                    itemLOAN.EmployeePayroll.Employee = db.Employee.Find(itemLOAN.EmployeePayroll.Employee_Id);
                                    itemLOAN.EmployeePayroll.Employee.EmpName = db.NameSingle.Find(itemLOAN.EmployeePayroll.Employee.EmpName_Id);
                                }



                                var empofflist = db.Employee.AsNoTracking().ToList();
                                //.Include(e => e.EmpOffInfo)
                                //.Include(e => e.EmpOffInfo.Branch)
                                //.Include(e => e.EmpName)
                                //.Include(e => e.GeoStruct)
                                //.Include(e => e.GeoStruct.Location)
                                //.Include(e => e.GeoStruct.Location.LocationObj).AsNoTracking().ToList();
                                foreach (var itemEOFF in empofflist)
                                {
                                    itemEOFF.EmpOffInfo = db.EmpOff.Find(itemEOFF.EmpOffInfo_Id);
                                    itemEOFF.EmpOffInfo.Branch = db.Branch.Find(itemEOFF.EmpOffInfo.Branch_Id);
                                    itemEOFF.EmpName = db.NameSingle.Find(itemEOFF.EmpName_Id);
                                    itemEOFF.GeoStruct = db.GeoStruct.Find(itemEOFF.GeoStruct_Id);
                                    itemEOFF.GeoStruct.Location = db.Location.Find(itemEOFF.GeoStruct.Location_Id);
                                    itemEOFF.GeoStruct.Location.LocationObj = db.LocationObj.Find(itemEOFF.GeoStruct.Location.LocationObj_Id);
                                }
                                var SelectedBatch = "";
                                if (salheadlist != null && salheadlist.Count() != 0)
                                {
                                    foreach (var item1 in salheadlist)
                                    {
                                        SelectedBatch = item1;
                                    }
                                }
                                else
                                {
                                    var batchnames = OJVReportData.Select(q => q.BatchName).Distinct().ToList();
                                    foreach (var item2 in batchnames)
                                    {
                                        SelectedBatch = item2;
                                    }
                                }
                                //foreach (var batch in batchnames)
                                //{

                                var jvdataforperticularbatch = OJVReportData.Where(q => q.BatchName == SelectedBatch
                                && (q.JVParameter.JVGroup.LookupVal.ToUpper() == "INDIVIDUAL"
                                || q.JVParameter.JVProductCode == "LOAN"
                                || q.JVParameter.JVProductCode == "NET")).ToList();

                                foreach (var item in jvdataforperticularbatch)
                                {
                                    //string checkfilter = item.JVParameter.JVGroup.LookupVal.ToUpper();
                                    //if (checkfilter == "INDIVIDUAL")
                                    //{
                                    var JVCODE = item.JVParameter.JVProductCode;
                                    var AccCode = item.AccountCode;
                                    var CreditBranchCode = item.BranchCode;
                                    string narr = item.Narration;
                                    string ecode = narr.Substring(narr.LastIndexOf("Br_") + 3);
                                    string empcode = ecode.Split('_')[1];

                                    //foreach (var jvli in JVPARACODELIST)
                                    //    {
                                    var EmployeeCode = "";
                                    var EmployeeName = "";
                                    var EmployeeLoc = "";
                                    var EmployeeDesc = "";

                                    if (JVCODE == "LOAN")
                                    {

                                        var EmpLoan = EmpLoanlIST.Where(e => e.LoanAccNo == AccCode && e.LoanAdvanceHead.SalaryHead.Id == item.SalaryHead.Id
                                            && e.LoanAccBranch.LocationObj.LocCode == CreditBranchCode && e.EmployeePayroll.Employee.EmpCode == empcode).FirstOrDefault();



                                        EmployeeCode = EmpLoan == null ? "" : EmpLoan.EmployeePayroll.Employee.EmpCode.ToString();
                                        EmployeeName = EmpLoan == null ? "" : EmpLoan.EmployeePayroll.Employee.EmpName.FullNameFML.ToString();
                                        // EmployeeLoc = EmpLoan == null ? "" : EmpLoan.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode.ToString();
                                        EmployeeLoc = EmpLoan == null ? "" : GeoDataInd.GeoStruct_Location_Code.ToString();
                                        EmployeeDesc = EmpLoan == null ? "" : EmpLoan.LoanAdvanceHead.Name.ToString();
                                    }
                                    if (JVCODE == "NET")
                                    {

                                        var empoff = empofflist.Where(e => AccCode == e.EmpOffInfo.AccountNo && e.EmpOffInfo.Branch != null && e.EmpOffInfo.Branch.Code == CreditBranchCode && e.EmpCode == empcode)
                                              .FirstOrDefault();
                                        EmployeeCode = empoff == null ? "" : empoff.EmpCode.ToString();
                                        EmployeeName = empoff == null ? "" : empoff.EmpName.FullNameFML.ToString();
                                        EmployeeLoc = empoff == null ? "" : empoff.GeoStruct.Location.LocationObj.LocDesc.ToString();

                                        EmployeeDesc = "NET";
                                    }

                                    //if (JVCODE == jvli)
                                    //{
                                    foreach (var loc in alllocations)
                                    {
                                        if (loc == item.BranchCode)
                                        {
                                            string credit = "";
                                            string debit = "";
                                            if (item.CreditDebitFlag == "C")
                                            {
                                                credit = item.TransactionAmount;
                                                debit = "0";
                                            }
                                            else
                                            {
                                                credit = "0";
                                                debit = item.TransactionAmount;
                                            }
                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                            {
                                                Fld1 = JVCODE,
                                                // Fld2 = sal.Code.ToString(),
                                                Fld3 = item.BranchCode == null ? "" : item.BranchCode.ToString(),
                                                // Fld4 = item.JVParameter.AccountNo == null ? "" : item.JVParameter.AccountNo.ToString(),

                                                Fld4 = item.AccountCode == null ? "" : item.AccountCode,
                                                Fld5 = credit,
                                                Fld6 = debit,
                                                Fld7 = SelectedBatch,
                                                Fld8 = paymonthJ,

                                                Fld9 = EmployeeCode,
                                                Fld10 = EmployeeName,
                                                Fld11 = EmployeeLoc,
                                                Fld12 = EmployeeDesc,
                                                Fld13 = item.AccountProductCode == null ? "" : item.AccountProductCode.ToString(),
                                            };
                                            //vinayak end
                                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                        }
                                        // }////
                                        //}
                                        //}
                                        //}
                                    }

                                }
                            }
                            // }

                            return OGenericPayrollStatement;
                        }

                        break;

                        //KDCC
                    case "SLABWISEDA":
                        var SLABWISEDAData = db.CompanyPayroll
                            .Include(e => e.PayScaleAssignment)
                            .Include(e => e.PayScaleAssignment.Select(y => y.SalaryHead))
                             .Include(e => e.PayScaleAssignment.Select(y => y.SalaryHead.SalHeadOperationType))
                            //.Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                            //.Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                            //.Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                            //.Include(e => e.EmployeePayroll)
                            //.Include(e => e.EmployeePayroll.Select(q => q.Employee.PayStruct.Grade))
                            //.Include(e => e.EmployeePayroll.Select(q => q.EmpSalStruct))
                            //.Include(e => e.EmployeePayroll.Select(q => q.EmpSalStruct.Select(qa => qa.EmpSalStructDetails)))
                            //.Include(l => l.EmployeePayroll.Select(e => e.EmpSalStruct.Select(o => o.EmpSalStructDetails.Select(m => m.SalaryHead))))
                           .Include(e => e.SalHeadFormula)
                           .Include(e => e.SalHeadFormula.Select(r => r.PayStruct.Grade))
                           .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                           .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule.BasicScale))
                           .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule.BasicScale.BasicScaleDetails)).AsNoTracking()
                       .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();
                        if (SLABWISEDAData.SalHeadFormula == null || SLABWISEDAData.SalHeadFormula.Count() == 0)
                            return null;
                        else
                        {
                            double totalcount = 0;
                            List<int?> Paystrid = new List<int?>();
                            var active = db.PayStruct.Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Where(e=> e.Grade!=null && e.JobStatus!=null && e.JobStatus.EmpActingStatus!=null).ToList();
                            var activestatus = active.Where(e => e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "ACTIVE").Select(s=>s.Id).ToList();
                            foreach (var pid in activestatus)
                            {
                                Paystrid.Add(pid);
                            }
                            var grades = db.Grade.ToList();
                          //  var grades = db.Grade.Where(q => q.Code == "40").ToList();
                            List<int?> Foumulaid = new List<int?>();
                            var payscaleagreementid = db.PayScaleAgreement.Where(e => e.EndDate == null).FirstOrDefault();
                            var OPayScaleAgreementId = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == payscaleagreementid.Id).FirstOrDefault();

                            var OPayScaleAssignment = db.PayScaleAssignment
                   .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                  .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId.Id && p.p.CompanyPayroll.Id == SLABWISEDAData.Id)
                  .Select(m => new
                  {
                      PayScaleAssignment = m.p,
                      SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType,
                      Id = m.p.Id, // or m.ppc.pc.ProdId
                      Salheadformula = m.p.SalHeadFormula.Select(t => new SalHeadFormulaT
                      {
                          GeoStruct = t.GeoStruct,
                          PayStruct = t.PayStruct,
                          FuncStruct = t.FuncStruct,
                          FormulaType = t.FormulaType,
                          Name = t.Name,
                          Id = t.Id,
                      }),
                      SalaryHead = m.p.SalaryHead
                      // other assignments
                  }).Where(d=>d.SalHeadOperationType.LookupVal.ToUpper()=="BASIC").ToList();

                            foreach (var applyFormula in OPayScaleAssignment)
                            {
                                var frmulaid = applyFormula.Salheadformula.ToList();

                                foreach (var formid in frmulaid)
                                {
                                    Foumulaid.Add(formid.Id);
                                }

                            }

                            foreach (var gr in grades)
                            {
                                double gradecount = 0;
                                List<string> salcode = new List<string>();
                                List<int?> salheaid = new List<int?>();
                                List<EmpSalStruct> EmpSalStructTotal = new List<EmpSalStruct>();
                                List<EmpSalStructDetails> EmpSalStructDetails = new List<EmpSalStructDetails>();
                                List<PayScaleAssignment> PayScaleAssignment = new List<PayScaleAssignment>();
                                var PayScaleAssignmentObj = new PayScaleAssignment();
                                List<SalaryHead> SalaryHead = new List<SalaryHead>();
                                var SalaryHeadObj = new SalaryHead();
                                List<SalHeadFormula> SalHeadFormula = new List<SalHeadFormula>();
                                var SalaryHeadFormulaObj = new SalHeadFormula();
                                int gradeid = gr.Id;
                                salcode.Add("BASIC");
                                salcode.Add("DA");
                                salcode.Add("HRA");
                                var salheaids = db.SalaryHead.Where(e => salcode.Contains(e.Code)).Select(x => x.Id).ToList();

                                foreach (var item in salheaids)
                                {
                                    salheaid.Add(item);
                                }



                                var slabwithsamegrade = SLABWISEDAData.SalHeadFormula.Where(q => q.PayStruct != null && q.PayStruct.Grade != null && q.PayStruct.Grade.Id == gr.Id && q.BASICDependRule != null && Foumulaid.Contains(q.Id) && Paystrid.Contains(q.PayStruct_Id)).ToList();

                                EmpSalStructTotal = db.EmpSalStruct.Include(q => q.PayStruct.Grade).Where(q => q.PayStruct.Grade.Id == gr.Id && q.EndDate == null).ToList();
                                foreach (var i in EmpSalStructTotal)
                                {
                                    EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct.Id == i.Id && salheaid.Contains(e.SalaryHead_Id)).ToList();
                                    foreach (var j in EmpSalStructDetails)
                                    {
                                        var SalHeadTmp = new SalaryHead();
                                        j.PayScaleAssignment = db.PayScaleAssignment.Where(e => e.Id == j.PayScaleAssignment_Id).FirstOrDefault();
                                        j.SalHeadFormula = db.SalHeadFormula.Where(e => e.Id == j.SalHeadFormula_Id).FirstOrDefault();
                                        j.SalaryHead = db.SalaryHead.Include(e => e.LeaveDependPolicy).Where(e => e.Id == j.SalaryHead_Id).FirstOrDefault();
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

                                    }
                                    i.EmpSalStructDetails = EmpSalStructDetails;
                                }
                                var getempsalwithpay = EmpSalStructTotal.ToList();

                                //  var getempsalwithpay1 = db.EmpSalStruct.Include(q => q.EmpSalStructDetails).Include(q => q.EmpSalStructDetails.Select(qc => qc.SalaryHead)).Include(q => q.PayStruct.Grade).Where(q => q.PayStruct.Grade.Id == gr.Id && q.EndDate == null).ToList();
                                //  var slabwithsamegrade = SLABWISEDAData.SalHeadFormula.Where(q => q.PayStruct.Grade.Id == gr.Id && q.BASICDependRule != null).ToList();

                                foreach (var salform in slabwithsamegrade)
                                {
                                    foreach (var ca2 in salform.BASICDependRule.BasicScale.BasicScaleDetails)
                                    {
                                        var lastdata = salform.BASICDependRule.BasicScale.BasicScaleDetails.Last();

                                        double StartingSlab = ca2.StartingSlab;
                                        double EndingSlab = ca2.EndingSlab;
                                        double IncrementAmount = ca2.IncrementAmount;
                                        decimal IncrementCount = ca2.IncrementCount;
                                        if (ca2.Id == lastdata.Id)
                                        {
                                            IncrementCount = IncrementCount + 1;
                                        }
                                        if (StartingSlab == EndingSlab)
                                        {
                                            IncrementCount = ca2.IncrementCount + 16;
                                        }
                                        for (int i = 0; i < IncrementCount; i++)
                                        {
                                            double basicamt = 0;
                                            double DAcamt = 0;
                                            double HRAamt = 0;
                                            int Count = 0;
                                            double Total = 0;
                                            double basic = 0;
                                            double TDA = 0;
                                            double THRA = 0;

                                            if (i != 0)
                                            {
                                                StartingSlab = StartingSlab + ca2.IncrementAmount;
                                            }
                                            EmpSalStructDetails OEmpSalDet = null;
                                            foreach (EmpSalStruct EmpSal in getempsalwithpay.Where(e=>e.PayStruct_Id==salform.PayStruct_Id).ToList())
                                            {

                                                OEmpSalDet = EmpSal.EmpSalStructDetails.Where(q => q.SalaryHead.Code == "BASIC" && q.Amount == StartingSlab).SingleOrDefault();
                                                if (OEmpSalDet != null)
                                                {
                                                    basicamt = OEmpSalDet.Amount;
                                                    DAcamt = EmpSal.EmpSalStructDetails.Where(q => q.SalaryHead.Code == "DA").Select(e => e.Amount).SingleOrDefault();

                                                    HRAamt = EmpSal.EmpSalStructDetails.Where(q => q.SalaryHead.Code == "HRA").Select(e => e.Amount).SingleOrDefault();

                                                    Count = Count + 1;
                                                    basic = basicamt * Count;
                                                    totalcount = totalcount + 1;
                                                    TDA = DAcamt * Count;
                                                    THRA = HRAamt * Count;
                                                    Total = basic + TDA + THRA;
                                                }

                                                //   DAcamt=  SalaryHeadGenProcess.SlabwiseDacalc(salform.Id,ca2, EmpSal);
                                                //break;
                                            }
                                            gradecount = gradecount + Count;
                                            if (Count == 0)
                                            {
                                                //gradecount = gradecount + 1;
                                                //totalcount = totalcount + 1;
                                                basicamt = StartingSlab;
                                                DAcamt = 0;
                                                HRAamt = 0;
                                                Count = 0;
                                                Total = basic;
                                            }

                                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                            {
                                                //  Fld1 = ca.Id.ToString(),
                                                Fld1 = Count != null ? Count.ToString() : "",       //count
                                                Fld2 = gr.Name.ToString()+""+gr.Levels,      //grade
                                                Fld3 = StartingSlab.ToString(),  //basicpay
                                                Fld4 = TDA != null ? TDA.ToString() : "",            //da
                                                Fld5 = THRA != null ? THRA.ToString() : "",           //HRA
                                                Fld6 = Total != null ? Total.ToString() : "",
                                                Fld7 = gradecount.ToString(),       //count
                                                Fld8 = totalcount.ToString(),       //count
                                                Fld9 = basic.ToString(),
                                                Fld10 = basicamt.ToString(),       //basic
                                                Fld11 = DAcamt.ToString(),       //da
                                                Fld12 = HRAamt.ToString(),//hra

                                            };
                                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                            //}
                                        }
                                    }
                                }
                                //GenericField100 OGenericSalMasterObjStatement1 = new GenericField100()
                                //{
                                //    Fld8 = gradecount.ToString(),  //grade
                                //};
                                //OGenericPayrollStatement.Add(OGenericSalMasterObjStatement1);
                            }

                            //GenericField100 OGenericSalMasterObjStatement2 = new GenericField100()
                            //{
                            //    Fld7 = totalcount.ToString(),       //count
                            //};
                            //OGenericPayrollStatement.Add(OGenericSalMasterObjStatement2);

                            return OGenericPayrollStatement;
                        }

                        break;

                        //KDCC NEW AGREEMENT
                    case "SLABWISEDACAL":
                         var SLABWISEDADataCal = db.CompanyPayroll
                             .Include(e => e.PayScaleAssignment)
                            .Include(e => e.PayScaleAssignment.Select(y => y.SalaryHead))
                             .Include(e => e.PayScaleAssignment.Select(y => y.SalaryHead.SalHeadOperationType))
                            //.Include(e => e.SalHeadFormula.Select(r => r.SalWages))
                            //.Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster))
                            //.Include(e => e.SalHeadFormula.Select(r => r.SalWages.RateMaster.Select(t => t.SalHead)))
                            //.Include(e => e.EmployeePayroll)
                            //.Include(e => e.EmployeePayroll.Select(q => q.Employee.PayStruct.Grade))
                            //.Include(e => e.EmployeePayroll.Select(q => q.EmpSalStruct))
                            //.Include(e => e.EmployeePayroll.Select(q => q.EmpSalStruct.Select(qa => qa.EmpSalStructDetails)))
                            //.Include(l => l.EmployeePayroll.Select(e => e.EmpSalStruct.Select(o => o.EmpSalStructDetails.Select(m => m.SalaryHead))))
                           .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment))
                           .Include(e => e.SalHeadFormula.Select(r => r.PayScaleAssignment.Select(t => t.SalaryHead)))
                           .Include(e => e.SalHeadFormula.Select(r => r.PayStruct.Grade))
                           .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule))
                           .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule.BasicScale))
                           .Include(e => e.SalHeadFormula.Select(r => r.BASICDependRule.BasicScale.BasicScaleDetails)).AsNoTracking()
                       .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();
                        if (SLABWISEDADataCal.SalHeadFormula == null || SLABWISEDADataCal.SalHeadFormula.Count() == 0)
                            return null;
                        else
                        {
                            double totalcount = 0;
                            List<int?> Paystrid = new List<int?>();
                            var active = db.PayStruct.Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.JobStatus.EmpActingStatus).Where(e => e.Grade != null && e.JobStatus != null && e.JobStatus.EmpActingStatus != null).ToList();
                            var activestatus = active.Where(e => e.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "ACTIVE").Select(s => s.Id).ToList();
                            foreach (var pid in activestatus)
                            {
                                Paystrid.Add(pid);
                            }
                            List<int?> Foumulaid = new List<int?>();
                            var payscaleagreementid = db.PayScaleAgreement.Where(e => e.EndDate == null).FirstOrDefault();
                            var OPayScaleAgreementId = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == payscaleagreementid.Id).FirstOrDefault();

                            var OPayScaleAssignment = db.PayScaleAssignment
                   .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                  .Where(p => p.p.PayScaleAgreement.Id == OPayScaleAgreementId.Id && p.p.CompanyPayroll.Id == SLABWISEDADataCal.Id)
                  .Select(m => new
                  {
                      PayScaleAssignment = m.p,
                      SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType,
                      Id = m.p.Id, // or m.ppc.pc.ProdId
                      Salheadformula = m.p.SalHeadFormula.Select(t => new SalHeadFormulaT
                      {
                          GeoStruct = t.GeoStruct,
                          PayStruct = t.PayStruct,
                          FuncStruct = t.FuncStruct,
                          FormulaType = t.FormulaType,
                          Name = t.Name,
                          Id = t.Id,
                      }),
                      SalaryHead = m.p.SalaryHead
                      // other assignments
                  }).Where(d => d.SalHeadOperationType.LookupVal.ToUpper() == "BASIC").ToList();

                            foreach (var applyFormula in OPayScaleAssignment)
                            {
                                var frmulaid = applyFormula.Salheadformula.ToList();

                                foreach (var formid in frmulaid)
                                {
                                    Foumulaid.Add(formid.Id);
                                }

                            }


                            var grades = db.Grade.OrderBy(e => e.Id).ToList();
                            //var grades = db.Grade.Where(q => q.Code == "100").ToList();

                            foreach (var gr in grades)
                            {
                                double gradecount = 0;

                                int gradeid = gr.Id;
                                var slabwithsamegrade = SLABWISEDADataCal.SalHeadFormula.Where(q => q.PayStruct != null && q.PayStruct.Grade != null && q.PayStruct.Grade.Id == gr.Id && q.BASICDependRule != null && Foumulaid.Contains(q.Id) && Paystrid.Contains(q.PayStruct_Id)).ToList();
                                // var getempsalwithpay = db.EmpSalStruct.Include(q => q.EmpSalStructDetails).Include(q => q.EmpSalStructDetails.Select(qc => qc.SalaryHead)).Include(q => q.PayStruct.Grade).Where(q => q.PayStruct.Grade.Id == gr.Id && q.EndDate == null).ToList();
                                //  var slabwithsamegrade = SLABWISEDAData.SalHeadFormula.Where(q => q.PayStruct.Grade.Id == gr.Id && q.BASICDependRule != null).ToList();

                                foreach (var salform in slabwithsamegrade)
                                {
                                    foreach (var ca2 in salform.BASICDependRule.BasicScale.BasicScaleDetails)
                                    {
                                        var lastdata = salform.BASICDependRule.BasicScale.BasicScaleDetails.Last();

                                        double StartingSlab = ca2.StartingSlab;
                                        double EndingSlab = ca2.EndingSlab;
                                        double IncrementAmount = ca2.IncrementAmount;
                                        decimal IncrementCount = ca2.IncrementCount;
                                        if (ca2.Id == lastdata.Id)
                                        {
                                            IncrementCount = IncrementCount + 1;
                                        }
                                        if (StartingSlab == EndingSlab)
                                        {
                                            IncrementCount = ca2.IncrementCount + 16;
                                        }
                                        for (int i = 0; i < IncrementCount; i++)
                                        {
                                            double basicamt = 0;

                                            int Count = 0;
                                            double Total = 0;
                                            double basic = 0;
                                            double TDA = 0;
                                            double THRA = 0;

                                            if (i != 0)
                                            {
                                                StartingSlab = StartingSlab + ca2.IncrementAmount;
                                            }
                                            // EmpSalStructDetails OEmpSalDet = null;
                                            var salaryheadlist = db.SalaryHead.Where(e => e.Code == "DA").FirstOrDefault();
                                            //foreach (var salheadid in salaryheadlist)
                                            //{
                                            double DAcamt = 0;
                                            double HRAamt = 0;
                                            var payscaleassigment = db.PayScaleAssignment
                                                .Include(e => e.SalHeadFormula)
                                                .Include(e => e.SalHeadFormula.Select(t => t.PayStruct))
                                                .Where(e => e.SalaryHead.Id == salaryheadlist.Id).FirstOrDefault();
                                            var salheadformulaid = payscaleassigment.SalHeadFormula.Where(e => e.PayStruct != null && e.PayStruct.Id == salform.PayStruct.Id).FirstOrDefault();
                                            if (salheadformulaid != null)
                                            {
                                                double SalAmount = SalHeadAmountDASLAB(salheadformulaid.Id, null, StartingSlab, salform.PayStruct, mPayMonth.FirstOrDefault());

                                                SalAmount = SalaryHeadGenProcess.RoundingFunction(salaryheadlist, SalAmount);
                                                // basicamt = OEmpSalDet.Amount;
                                                if (salaryheadlist.Code == "DA")
                                                {
                                                    DAcamt = SalAmount;
                                                }
                                                //if (salheadid.Code == "HRA")
                                                //{
                                                //    HRAamt = SalAmount;
                                                //}
                                            }
                                            // }
                                            //DAcamt = EmpSal.EmpSalStructDetails.Where(q => q.SalaryHead.Code == "DA").Select(e => e.Amount).SingleOrDefault();

                                            //HRAamt = EmpSal.EmpSalStructDetails.Where(q => q.SalaryHead.Code == "HRA").Select(e => e.Amount).SingleOrDefault();

                                            //Count = Count + 1;
                                            //basic = basicamt * Count;
                                            //totalcount = totalcount + 1;
                                            //TDA = DAcamt * Count;
                                            //THRA = HRAamt * Count;
                                            //Total = basic + TDA + THRA;


                                            //   DAcamt=  SalaryHeadGenProcess.SlabwiseDacalc(salform.Id,ca2, EmpSal);
                                            //break;
                                            //}
                                            //gradecount = gradecount + Count;
                                            //if (Count == 0)
                                            //{
                                            //    //gradecount = gradecount + 1;
                                            //    //totalcount = totalcount + 1;
                                            //    basicamt = StartingSlab;
                                            //    DAcamt = 0;
                                            //    HRAamt = 0;
                                            //    Count = 0;
                                            //    Total = basic;
                                            //}

                                            GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                            {
                                                //  Fld1 = ca.Id.ToString(),
                                                Fld1 = gr.Name.ToString(),       //count
                                                Fld2 = StartingSlab.ToString(),      //grade
                                                Fld3 = DAcamt.ToString(),  //basicpay
                                                //Fld4 = HRAamt.ToString(),            //da
                                            };
                                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                            //}

                                        }
                                    }
                                }
                                //GenericField100 OGenericSalMasterObjStatement1 = new GenericField100()
                                //{
                                //    Fld8 = gradecount.ToString(),  //grade
                                //};
                                //OGenericPayrollStatement.Add(OGenericSalMasterObjStatement1);
                            }

                            //GenericField100 OGenericSalMasterObjStatement2 = new GenericField100()
                            //{
                            //    Fld7 = totalcount.ToString(),       //count
                            //};
                            //OGenericPayrollStatement.Add(OGenericSalMasterObjStatement2);

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "INCRDUELIST":
                        var OIncrDueList = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OIncrDueList_temp = db.EmployeePayroll
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.IncrDataCalc)
                                //.Include(e => e.StagIncrDataCalc)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)

                                 .Where(e => e.Employee.Id == item)// && (mPayMonth.Contains(e.IncrDataCalc.ProcessIncrDate.Value.ToString("MM/yyyy"))))
                                 .SingleOrDefault();
                            if (OIncrDueList_temp != null)
                            {
                                OIncrDueList_temp.Employee.EmpName = db.NameSingle.Where(e => e.Id == OIncrDueList_temp.Employee.EmpName_Id).FirstOrDefault();
                                OIncrDueList_temp.IncrDataCalc = db.IncrDataCalc.Where(e => e.Id == OIncrDueList_temp.IncrDataCalc_Id).FirstOrDefault();
                                OIncrDueList_temp.StagIncrDataCalc = db.StagIncrDataCalc.Where(e => e.Id == OIncrDueList_temp.StagIncrDataCalc_Id).FirstOrDefault();
                                OIncrDueList.Add(OIncrDueList_temp);
                            }
                        }

                        if (OIncrDueList == null || OIncrDueList.Count() == 0)
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
                            /////
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OIncrDueList.Where(e => (e.IncrDataCalc != null) && (e.IncrDataCalc.ProcessIncrDate != null) && (mPayMonth.Contains(e.IncrDataCalc.ProcessIncrDate.Value.ToString("MM/yyyy")))))
                            {

                                if (ca.IncrDataCalc != null)
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
                                        ////

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
                                                Fld12 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld13 = "Regular",
                                                Fld14 = "0",
                                                Fld15 = "0"
                                            };
                                            //if (month)
                                            //{
                                            //    OGenericObjStatement.Fld100 = ca.PayMonth.ToString();
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
                            List<string> MonList = new List<string>();
                            foreach (string a in mPayMonth)
                            {
                                MonList.Add(a.Split('/')[0]);
                            }
                            foreach (var ca in OIncrDueList.Where(e => (e.StagIncrDataCalc != null) && (e.StagIncrDataCalc.ProcessIncrDate != null) && (MonList.Contains(e.StagIncrDataCalc.ProcessIncrDate.Value.ToString("MM"))) && e.StagIncrDataCalc.ProcessIncrDate.Value >= Convert.ToDateTime("01/" + mPayMonth[0])))
                            {

                                if (ca.StagIncrDataCalc != null)
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
                                        ////

                                        var OITTrans = ca.StagIncrDataCalc;
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
                                                Fld5 = ca.StagIncrDataCalc.OrignalIncrDate.Value.ToShortDateString(),
                                                Fld6 = ca.StagIncrDataCalc.ProcessIncrDate.Value.ToShortDateString(),
                                                Fld7 = ca.StagIncrDataCalc.LWPDays.ToString(),
                                                Fld8 = ca.StagIncrDataCalc.OldBasic.ToString(),
                                                Fld9 = ca.StagIncrDataCalc.NewBasic.ToString(),
                                                Fld10 = ca.StagIncrDataCalc.StagnancyAppl.ToString(),
                                                Fld11 = ca.StagIncrDataCalc.StagnancyCount.ToString(),
                                                Fld12 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld13 = "Stagnant",
                                                Fld14 = ca.StagIncrDataCalc.ParaSpanYear.ToString(),
                                                Fld15 = ca.StagIncrDataCalc.CalcSpanYear.ToString()
                                            };
                                            //if (month)
                                            //{
                                            //    OGenericObjStatement.Fld100 = ca.PayMonth.ToString();
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
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "PFECRCHALLAN":

                        //case "ITTRANST":
                        var OPFECRChallanData = db.PFECRSummaryR

                           ////.Include(e => e.PFECRR)
                            //.Include(e => e.Employee)
                            //.Include(e => e.Employee.EmpName)
                            ////.Include(e => e.PFECRR.Select(t => t.PFCalendar))


                        //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                            //.ToList();
                        .Where(e => mPayMonth.Contains(e.Wage_Month))
                        .ToList();

                        if (OPFECRChallanData != null)
                        {
                            foreach (var i in OPFECRChallanData)
                            {
                                i.PFECRR = db.PFECRSummaryR.Where(e => e.Id == i.Id).Select(r => r.PFECRR.ToList()).FirstOrDefault();
                                foreach (var j in i.PFECRR)
                                {
                                    j.PFCalendar = db.PFECRR.Where(e => e.Id == j.Id).Select(r => r.PFCalendar).FirstOrDefault();
                                }
                            }

                        }

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


                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "PFECRR":

                        //case "ITTRANST":
                        var OPFECRData = db.PFECRSummaryR
                            //.Include(e => e.PFECRR)

                        //.Include(e => e.Employee.EmpName)
                            //.Include(e => e.PFECRR.Select(t => t.PFCalendar))


                        //.Where(e => EmpPayrollIdList.Contains(e.Employee.Id))
                            //.ToList();
                        .Where(e => mPayMonth.Contains(e.Wage_Month))
                        .ToList();

                        if (OPFECRData != null)
                        {
                            foreach (var i in OPFECRData)
                            {
                                i.PFECRR = db.PFECRSummaryR.Where(e => e.Id == i.Id).Select(r => r.PFECRR.ToList()).FirstOrDefault();
                                foreach (var j in i.PFECRR)
                                {
                                    j.PFCalendar = db.PFECRR.Where(e => e.Id == j.Id).Select(r => r.PFCalendar).FirstOrDefault();
                                }
                            }

                        }

                        var OCompanyPayroll = db.CompanyPayroll
                            ////.Include(e => e.PFMaster)
                            // .Include(e => e.PFMaster.Select(r => r.EPSWages))
                            // .Include(e => e.PFMaster.Select(r => r.PFAdminWages))
                            // .Include(e => e.PFMaster.Select(r => r.PFEDLIWages))
                            // .Include(e => e.PFMaster.Select(r => r.PFInspWages))
                            // .Include(e => e.PFMaster.Select(r => r.PFTrustType))
                            // .Include(e => e.PFMaster.Select(r => r.EPFWages))
                            ////.Include(e => e.Company)
                            //.Include(e=>e.Company.Employee)
                            //  .Include(e => e.PFECRSummaryR)
                            ////.Include(e => e.PFECRSummaryR.Select(r => r.PFECRR))
                       .Where(e => e.Company.Id == CompanyId)
                       .SingleOrDefault();

                        if (OCompanyPayroll != null)
                        {

                            var PFMaster = db.CompanyPayroll.Where(e => e.Id == OCompanyPayroll.Id).Select(r => r.PFMaster.ToList()).FirstOrDefault();
                            var Company = db.Company.Where(e => e.Id == OCompanyPayroll.Company_Id).FirstOrDefault();
                            var PFECRSummaryR = db.CompanyPayroll.Where(e => e.Id == OCompanyPayroll.Id).Select(r => r.PFECRSummaryR.ToList()).FirstOrDefault();
                            foreach (var j in PFECRSummaryR)
                            {
                                j.PFECRR = db.PFECRSummaryR.Where(e => e.Id == j.Id).Select(r => r.PFECRR.ToList()).FirstOrDefault();
                            }
                        }

                        // var OPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + mPayMonth).Date).SingleOrDefault();


                        //if (OPFMaster == null)
                        //{
                        //    OPFMaster = OCompanyPayroll.PFMaster.LastOrDefault();
                        //}

                        if (OPFECRData == null || OPFECRData.Count() == 0)
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

                            foreach (var ca in OPFECRData)
                            {
                                if (ca.PFECRR != null && ca.PFECRR.Count() != 0)
                                {
                                    var OPFECRR = ca.PFECRR.ToList();
                                    if (OPFECRR != null && OPFECRR.Count() != 0)
                                    {
                                        foreach (var ca1 in OPFECRR)
                                        {
                                            PFMaster OPFMaster = null;
                                            foreach (var itemPFmas in OCompanyPayroll.PFMaster.ToList())
                                            {
                                                if (itemPFmas.EstablishmentID == ca1.Establishment_ID && (itemPFmas.EndDate == null || itemPFmas.EndDate.Value.Date > Convert.ToDateTime("01/" + mPayMonth).Date))
                                                {
                                                    OPFMaster = itemPFmas;
                                                }
                                            }

                                            //if (OPFMaster == null)
                                            //{
                                            //    OPFMaster = OCompanyPayroll.PFMaster.Where(e=>e.EstablishmentID==ca1.Establishment_ID).SingleOrDefault();
                                            //}

                                            var code = db.Employee.Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).Include(e => e.EmpOffInfo).Include(q => q.EmpOffInfo.NationalityID).Where(e => e.EmpOffInfo.NationalityID.UANNo.Replace(" ", string.Empty).ToUpper() == ca1.UAN.Replace(" ", string.Empty).ToUpper()).FirstOrDefault();
                                            if (code != null)
                                            {
                                                //int geoid = code.GeoStruct.Id;

                                                //int payid = code.PayStruct.Id;

                                                //int funid = code.FuncStruct.Id;

                                                //GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                                //PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                                //FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                                //if (OSalAttend != null && OSalAttend.Count() != 0)
                                                //{

                                                int? geoid = code.GeoStruct_Id;

                                                int? payid = code.PayStruct_Id;

                                                int? funid = code.FuncStruct_Id;

                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                                PayStruct paystruct = db.PayStruct.Find(payid);

                                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                                if (GeoDataInd != null)
                                                {

                                                    double EPF_Wages = ca1.EPF_Wages;
                                                    double EDLI_Wages = ca1.EDLI_Wages;
                                                    double EPS_Wages = ca1.EPS_Wages;
                                                    double EE_Share = ca1.EE_Share;
                                                    double EPS_Share = ca1.EPS_Share;
                                                    double ER_Share = ca1.ER_Share;
                                                    double EE_VPF_Share = ca1.EE_VPF_Share;

                                                    double Officiating_EPF_Wages = ca1.Officiating_EPF_Wages;
                                                    double Officiating_EDLI_Wages = ca1.Officiating_EDLI_Wages;
                                                    double Officiating_EPS_Wages = ca1.Officiating_EPS_Wages;
                                                    double Officiating_EE_Share = ca1.Officiating_EE_Share;
                                                    double Officiating_EPS_Share = ca1.Officiating_EPS_Share;
                                                    double Officiating_ER_Share = ca1.Officiating_ER_Share;
                                                    double Officiating_VPF_Share = ca1.Officiating_VPF_Share;

                                                    double Arrear_EPF_Wages = ca1.Arrear_EPF_Wages;
                                                    double Arrear_EPS_Wages = ca1.Arrear_EPS_Wages;
                                                    double Arrear_EE_Share = ca1.Arrear_EE_Share;
                                                    double Arrear_EPS_Share = ca1.Arrear_EPS_Share;
                                                    double Arrear_ER_Share = ca1.Arrear_ER_Share;
                                                    double Arrear_EDLI_Wages = ca1.Arrear_EDLI_Wages;

                                                    if (Arrear_EE_Share < 0)
                                                    {
                                                        EPF_Wages = EPF_Wages + Arrear_EPF_Wages + Officiating_EPF_Wages;
                                                        EDLI_Wages = EDLI_Wages + Arrear_EDLI_Wages + Officiating_EDLI_Wages;
                                                        EPS_Wages = EPS_Wages + Arrear_EPS_Wages + Officiating_EPS_Wages;
                                                        EE_Share = EE_Share + Arrear_EE_Share + EE_VPF_Share + Officiating_EE_Share + Officiating_VPF_Share;
                                                        EPS_Share = EPS_Share + Arrear_EPS_Share + Officiating_EPS_Share;
                                                        ER_Share = ER_Share + Arrear_ER_Share + Officiating_ER_Share;
                                                        if (EPF_Wages < EPS_Wages)
                                                        {
                                                            EPS_Wages = EPF_Wages;
                                                            EPS_Share = Math.Round(EPS_Wages * OPFMaster.EPSPerc / 100, 0);
                                                            EE_Share = Math.Round(EPF_Wages * OPFMaster.EmpPFPerc / 100, 0);
                                                            ER_Share = EE_Share - EPS_Share;
                                                        }
                                                        Arrear_EPF_Wages = 0;
                                                        Arrear_EPS_Wages = 0;
                                                        Arrear_EE_Share = 0;
                                                        Arrear_EPS_Share = 0;
                                                        Arrear_ER_Share = 0;
                                                        Arrear_EDLI_Wages = 0;
                                                        EE_VPF_Share = 0;
                                                    }

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
                                                        Fld32 = code.EmpCode,
                                                        Fld8 = ca1.UAN_Name == null ? "" : ca1.UAN_Name.ToString(),
                                                        Fld9 = ca1.Gross_Wages == null ? "" : ca1.Gross_Wages.ToString(),

                                                        Fld10 = EPF_Wages.ToString(),//ca1.EPF_Wages == null ? "" : ca1.EPF_Wages.ToString(),
                                                        Fld11 = EDLI_Wages.ToString(),//ca1.EDLI_Wages == null ? "" : ca1.EDLI_Wages.ToString(),
                                                        Fld33 = EPS_Wages.ToString(),//ca1.EPS_Wages == null ? "" : ca1.EPS_Wages.ToString(),
                                                        Fld12 = EE_Share.ToString(),//ca1.EE_Share == null ? "" : ca1.EE_Share.ToString(),
                                                        Fld13 = EE_VPF_Share.ToString(),//ca1.EE_VPF_Share == null ? "" : ca1.EE_VPF_Share.ToString(),
                                                        Fld14 = EPS_Share.ToString(),//ca1.EPS_Share == null ? "" : ca1.EPS_Share.ToString(),
                                                        Fld15 = ER_Share.ToString(),//ca1.ER_Share == null ? "" : ca1.ER_Share.ToString(),

                                                        Fld18 = Arrear_EPF_Wages.ToString(),//ca1.Arrear_EPF_Wages == null ? "" : ca1.Arrear_EPF_Wages.ToString(),
                                                        Fld19 = Arrear_EPS_Wages.ToString(),//ca1.Arrear_EPS_Wages == null ? "" : ca1.Arrear_EPS_Wages.ToString(),
                                                        Fld20 = Arrear_EE_Share.ToString(),//ca1.Arrear_EE_Share == null ? "" : ca1.Arrear_EE_Share.ToString(),
                                                        Fld21 = Arrear_EPS_Share.ToString(),//ca1.Arrear_EPS_Share == null ? "" : ca1.Arrear_EPS_Share.ToString(),
                                                        Fld22 = Arrear_ER_Share.ToString(),//ca1.Arrear_ER_Share == null ? "" : ca1.Arrear_ER_Share.ToString(),
                                                        Fld34 = Arrear_EDLI_Wages.ToString(),//ca1.Arrear_EDLI_Wages == null ? "" : ca1.Arrear_EDLI_Wages.ToString(),
                                                        Fld23 = ca1.Father_Husband_Name == null ? "" : ca1.Father_Husband_Name.ToString(),
                                                        Fld24 = ca1.Relationship_with_the_Member == null ? "" : ca1.Relationship_with_the_Member.ToString(),
                                                        Fld25 = ca1.Date_of_Birth.Value.ToShortDateString() == null ? "" : ca1.Date_of_Birth.Value.ToShortDateString(),
                                                        Fld26 = ca1.Gender == null ? "" : ca1.Gender.ToString(),
                                                        Fld27 = ca1.Date_of_Joining_EPF == null ? "" : ca1.Date_of_Joining_EPF.Value.ToShortDateString(),
                                                        Fld28 = ca1.Date_of_Joining_EPS == null ? "" : ca1.Date_of_Joining_EPS.Value.ToShortDateString(),
                                                        Fld29 = ca1.Date_of_Exit_from_EPF == null ? "" : ca1.Date_of_Exit_from_EPF.Value.ToShortDateString(),
                                                        Fld30 = ca1.Date_of_Exit_from_EPS == null ? "" : ca1.Date_of_Exit_from_EPS.Value.ToShortDateString(),
                                                        Fld16 = ca1.NCP_Days == null ? "" : ca1.NCP_Days.ToString(),
                                                        Fld17 = ca1.Refund_of_Advances == null ? "" : ca1.Refund_of_Advances.ToString(),
                                                        Fld31 = ca1.Reason_for_leaving == null ? "" : ca1.Reason_for_leaving.ToString(),

                                                        Fld40 = Officiating_EPF_Wages.ToString(),//ca1.EPF_Wages == null ? "" : ca1.EPF_Wages.ToString(),
                                                        Fld41 = Officiating_EDLI_Wages.ToString(),//ca1.EDLI_Wages == null ? "" : ca1.EDLI_Wages.ToString(),
                                                        Fld42 = Officiating_EPS_Wages.ToString(),//ca1.EPS_Wages == null ? "" : ca1.EPS_Wages.ToString(),
                                                        Fld43 = Officiating_EE_Share.ToString(),//ca1.EE_Share == null ? "" : ca1.EE_Share.ToString(),
                                                        Fld44 = Officiating_VPF_Share.ToString(),//ca1.EE_VPF_Share == null ? "" : ca1.EE_VPF_Share.ToString(),
                                                        Fld45 = Officiating_EPS_Share.ToString(),//ca1.EPS_Share == null ? "" : ca1.EPS_Share.ToString(),
                                                        Fld46 = Officiating_ER_Share.ToString(),

                                                    };
                                                    //if (month)
                                                    //{
                                                    //    OGenericObjStatement.Fld100 = ca.PayMonth.ToString();
                                                    //}
                                                    if (comp)
                                                    {
                                                        OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Department_Name;
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
                                                    //if (emp)
                                                    //{
                                                    //    OGenericObjStatement.Fld88 = code.Employee.ToString();
                                                    //}
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

        public static double SalHeadAmountDASLAB(Int32 OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, double mSalWages,
                                          PayStruct PayStruct, string mCPIMonth, bool VPFApplicable = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var OSalHeadFormualQuery = SalaryHeadGenProcess._returnSalHeadFormula(OSalHeadFormula);

                // double mSalWages = 0;
                double mSalHeadAmount = 0;
                // var _Empoffdetils = new Vpfclass();
                // mSalWages = Wagecalc(OSalHeadFormualQuery, OSalaryDetails, OEmpSalStructDetails);

                if (OSalHeadFormualQuery.PercentDependRule != null && mSalWages != 0)
                {
                    mSalHeadAmount = (mSalWages * OSalHeadFormualQuery.PercentDependRule.SalPercent) / 100;
                    mSalHeadAmount = SalaryHeadGenProcess.CellingCkeck(OSalHeadFormualQuery, mSalHeadAmount);
                    return mSalHeadAmount;
                }
                if (OSalHeadFormualQuery.VDADependRule != null && mSalWages != 0)
                {
                    var oemployee = db.EmployeePayroll
                                    .Include(e => e.CPIEntryT)
                                 .Include(e => e.Employee)
                                 .Where(e => e.Employee.PayStruct.Id == PayStruct.Id).FirstOrDefault();

                    CPIEntryT OCPIEntryObj = null;

                    if (oemployee != null)
                    {
                        OCPIEntryObj = oemployee.CPIEntryT.Where(e => e.PayMonth == mCPIMonth).SingleOrDefault();
                    }

                    double indexpoint = 0;
                    double mVDA = 0;
                    if (OCPIEntryObj != null)
                    {
                        indexpoint = OCPIEntryObj.CalIndexPoint;
                    }
                    else
                    {
                        List<double> calindex = db.CPIEntryT.Where(e => e.PayMonth == mCPIMonth).Select(e => e.CalIndexPoint).Distinct().ToList();

                        if (calindex.Count() == 1)
                        {
                            indexpoint = calindex.FirstOrDefault();
                        }
                        else
                        {
                            return mVDA;
                        }

                    }

                    double mVDAWages = 0;
                    var mPayMonth = "";
                    mPayMonth = mCPIMonth;
                    if (OSalaryDetails != null)
                    {
                        var mSalayM = db.SalaryT.Where(e => e.SalEarnDedT == OSalaryDetails);
                        foreach (var ca in mSalayM)
                        {
                            mPayMonth = ca.PayMonth;
                        }
                    }

                    // EmployeePayroll OEmpCpi = db.EmployeePayroll.Include(e => e.CPIEntryT).Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault(); //asd

                    //CPIEntryT OCPIEntryObj = db.CPIEntryT.Where(e => e.PayMonth == mPayMonth && e.pa == OEmployeePayroll.Id).SingleOrDefault();
                    /* if (OCPIEntryObj == null)
                     {
                         //modified by prashant on 13042017
                         OCPIEntryObj = OEmpCpi.CPIEntryT != null ? OEmpCpi.CPIEntryT.LastOrDefault() : null;
                         //return 0;//modified by prashant 13042017
                         if (OCPIEntryObj == null)
                         {
                             return 0;
						 
                         }
                     }*/
                    //.Select(m=>new{CalIndexPoint=m.CalIndexPoint });
                    var OCPIRule = OSalHeadFormualQuery.VDADependRule.CPIRule;
                    var OCPIRuleUnitCalc = OSalHeadFormualQuery.VDADependRule.CPIRule.CPIUnitCalc;
                    var OCPIRuleDetail = OSalHeadFormualQuery.VDADependRule.CPIRule.CPIRuleDetails;
                    double mIBase = 0;

                    foreach (var ca in OCPIRuleUnitCalc)
                    {
                        if (indexpoint <= ca.IndexMaxCeiling)
                        {
                            mIBase = mIBase + ((indexpoint - ca.BaseIndex) / ca.Unit);
                            break;
                        }
                        else
                        {
                            mIBase = mIBase + ((ca.IndexMaxCeiling - ca.BaseIndex) / ca.Unit);
                        }
                    }
                    mIBase = Math.Round(mIBase, OCPIRule.IBaseDigit);
                    double Mtempbasic = 0;
                    double tempVDA = 0;
                    var OCPIRuleDetailList = OCPIRuleDetail.OrderBy(e => e.SalFrom).ToList();
                    var CompId = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCode = db.Company.Where(e => e.Id == CompId).AsNoTracking().AsParallel().Select(a => a.Code.ToUpper()).SingleOrDefault();  //asd
                    for (int i = 0; i < OCPIRuleDetailList.Count(); i++)
                    {
                        var ca = OCPIRuleDetailList[i];
                        if (CompCode == "KDCC")// Kolhapur DCC
                        {
                            if (i == 0)
                            {
                                if (mSalWages > ca.SalTo)//110
                                {

                                    Mtempbasic = ca.SalTo;//50

                                    mVDA = mVDA + (((Mtempbasic * (ca.IncrPercent * mIBase)) / 100));
                                    if (((Mtempbasic * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                    {
                                        mVDA = ca.MinAmountIBase * mIBase;
                                    }
                                    else if (((Mtempbasic * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                    {
                                        mVDA = ca.MaxAmountIBase * mIBase;
                                    }
                                    mSalWages = mSalWages - ca.SalTo;
                                }
                                else if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                                {

                                    mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                                    if (((mSalWages * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                    {
                                        mVDA = ca.MinAmountIBase * mIBase;
                                    }
                                    else if (((mSalWages * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                    {
                                        mVDA = ca.MaxAmountIBase * mIBase;
                                    }

                                    mSalWages = mSalWages - ca.SalTo;
                                    if (mSalWages < 0)
                                    {
                                        mSalWages = 0;

                                    }

                                }
                            }
                            else
                            {
                                mVDA = mVDA + (mSalWages * 0.25);
                            }
                        }
                        else
                        {
                            //if (mSalWages > ca.SalTo)//110
                            //{

                            //    Mtempbasic = ca.SalTo;//50

                            //    mVDA = mVDA + (((Mtempbasic * (ca.IncrPercent * mIBase)) / 100));
                            //    if (((Mtempbasic * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                            //    {
                            //        mVDA = ca.MinAmountIBase * mIBase;
                            //    }
                            //    else if (((Mtempbasic * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                            //    {
                            //        mVDA = ca.MaxAmountIBase * mIBase;
                            //    }
                            //    mSalWages = mSalWages - ca.SalTo;
                            //}
                            if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                            {

                                mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                                if (((mSalWages * ca.IncrPercent) / 100) <= ca.MinAmountIBase)
                                {
                                    mVDA = ca.MinAmountIBase * mIBase;
                                }
                                else if (((mSalWages * ca.IncrPercent) / 100) >= ca.MaxAmountIBase)
                                {
                                    mVDA = ca.MaxAmountIBase * mIBase;
                                }

                            }
                        }
                    }

                    //foreach (var ca in OCPIRuleDetail)
                    //{

                    //    if (mSalWages > ca.SalFrom && mSalWages <= ca.SalTo)
                    //    {
                    //        mVDA = mVDA + (((mSalWages * (ca.IncrPercent * mIBase)) / 100));
                    //        if (((mSalWages * ca.IncrPercent) / 100) <= OCPIRule.MinAmountIBase)
                    //        {
                    //            mVDA = OCPIRule.MinAmountIBase * mIBase;
                    //        }
                    //        else if (((mSalWages * ca.IncrPercent) / 100) >= OCPIRule.MaxAmountIBase)
                    //        {
                    //            mVDA = OCPIRule.MaxAmountIBase * mIBase;
                    //        }
                    //    }
                    //}
                    return mVDA;
                }

                return 0;
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
    }
}