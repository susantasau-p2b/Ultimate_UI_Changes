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
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Web.Script.Serialization;


namespace P2BUltimate.Process
{
    public class MasterReportGen
    {
        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };

        public class DeserializeClass
        {
            public string id { get; set; }
            public string text { get; set; }
            public string li_attr { get; set; }
            public string data { get; set; }
            public string Category { get; set; }
            public Array children { get; set; }
        }
        public static List<GenericField100> GenerateMasterReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> salheadlistLevel1, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime mFromDate, DateTime mToDate, DateTime pFromDate, DateTime pToDate, string ReportType)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {                   
                    case "EMPLOYEE":

                        var OEmpInfoData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OEmpInfoData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                                // .Include(e => e.Employee.EmpName)
                                //   .Include(e => e.Employee.Gender)
                                //   .Include(e => e.Employee.MaritalStatus)
                                //.Include(e => e.Employee.EmpOffInfo)
                                //  .Include(e => e.Employee.ServiceBookDates)
                                //  .Include(e => e.Employee.GeoStruct)
                                //    .Include(e => e.Employee.PayStruct)
                                //      .Include(e => e.Employee.FuncStruct)
                                 .Where(e => e.Employee.Id == item).AsNoTracking()
                                                   .FirstOrDefault();

                            //List<string> BusiCategory = new List<string>();

                            if (OEmpInfoData_t != null)
                            {
                                if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                                {
                                    //BusiCategory.AddRange(salheadlist);
                                    foreach (string ca1 in salheadlist)
                                    {
                                        OEmpInfoData_t.Employee.EmpName = db.NameSingle.Find(OEmpInfoData_t.Employee.EmpName_Id);
                                        OEmpInfoData_t.Employee.Gender = db.LookupValue.Find(OEmpInfoData_t.Employee.Gender_Id);
                                        OEmpInfoData_t.Employee.MaritalStatus = db.LookupValue.Find(OEmpInfoData_t.Employee.MaritalStatus_Id);
                                        OEmpInfoData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OEmpInfoData_t.Employee.ServiceBookDates_Id);
                                        OEmpInfoData_t.Employee.GeoStruct = db.GeoStruct.Find(OEmpInfoData_t.Employee.GeoStruct_Id);
                                        OEmpInfoData_t.Employee.GeoStruct.Location = db.Location.Find(OEmpInfoData_t.Employee.GeoStruct.Location_Id);
                                        OEmpInfoData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Find(OEmpInfoData_t.Employee.GeoStruct.Location.LocationObj_Id);
                                        OEmpInfoData_t.Employee.EmpOffInfo = db.EmpOff.Find(OEmpInfoData_t.Employee.EmpOffInfo_Id);
                                        OEmpInfoData_t.Employee.EmpOffInfo.Bank = db.Bank.Where(e => e.Id == OEmpInfoData_t.Employee.EmpOffInfo.Bank_Id && salheadlist.Contains(e.Name)).AsNoTracking().FirstOrDefault();
                                        //OEmpInfoData_t.Employee.GeoStruct.Location.BusinessCategory = db.LookupValue.Where(e => e.Id == OEmpInfoData_t.Employee.GeoStruct.Location.BusinessCategory_Id).FirstOrDefault();
                                        //if (OEmpInfoData_t.Employee.GeoStruct!=null && OEmpInfoData_t.Employee.GeoStruct.Location !=null && OEmpInfoData_t.Employee.GeoStruct.Location.BusinessCategory!=null)
                                        //{
                                        //    if (OEmpInfoData_t.Employee.GeoStruct.Location.BusinessCategory.LookupVal.ToString().ToUpper()==ca1.ToUpper())
                                        //    {
                                        //        OEmpInfoData.Add(OEmpInfoData_t);
                                        //        break;
                                        //    }

                                        //}
                                        //if (OEmpInfoData_t.Employee != null && OEmpInfoData_t.Employee.EmpOffInfo != null && OEmpInfoData_t.Employee.EmpOffInfo.Bank_Id!= null)
                                        //{

                                        if (OEmpInfoData_t.Employee.EmpOffInfo != null && OEmpInfoData_t.Employee.EmpOffInfo.Bank != null)
                                        {
                                            OEmpInfoData.Add(OEmpInfoData_t);
                                        }
                                    }

                                }
                                else
                                {
                                    OEmpInfoData_t.Employee.EmpName = db.NameSingle.Find(OEmpInfoData_t.Employee.EmpName_Id);
                                    OEmpInfoData_t.Employee.Gender = db.LookupValue.Find(OEmpInfoData_t.Employee.Gender_Id);
                                    OEmpInfoData_t.Employee.MaritalStatus = db.LookupValue.Find(OEmpInfoData_t.Employee.MaritalStatus_Id);
                                    OEmpInfoData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OEmpInfoData_t.Employee.ServiceBookDates_Id);
                                    OEmpInfoData_t.Employee.GeoStruct = db.GeoStruct.Find(OEmpInfoData_t.Employee.GeoStruct_Id);
                                    OEmpInfoData_t.Employee.GeoStruct.Location = db.Location.Find(OEmpInfoData_t.Employee.GeoStruct.Location_Id);
                                    OEmpInfoData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Find(OEmpInfoData_t.Employee.GeoStruct.Location.LocationObj_Id);
                                    OEmpInfoData_t.Employee.EmpOffInfo = db.EmpOff.Find(OEmpInfoData_t.Employee.EmpOffInfo_Id);
                                    OEmpInfoData_t.Employee.EmpOffInfo.Bank = db.Bank.Find(OEmpInfoData_t.Employee.EmpOffInfo.Bank_Id);
                                    OEmpInfoData.Add(OEmpInfoData_t);

                                }

                            }
                        }

                        if (OEmpInfoData == null || OEmpInfoData.Count() == 0)
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



                            foreach (var ca in OEmpInfoData.OrderBy(e => e.Employee.EmpCode))
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


                                GenericField100 OGenericObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Employee.EmpCode,
                                    Fld2 = ca.Employee.BirthPlace == null ? "" : ca.Employee.BirthPlace,
                                    Fld3 = ca.Employee.CardCode,
                                    Fld4 = ca.Employee.ValidFromDate == null ? "" : ca.Employee.ValidFromDate.Value.ToShortDateString(),
                                    Fld5 = ca.Employee.ValidToDate == null ? "" : ca.Employee.ValidToDate.Value.ToShortDateString(),

                                    Fld6 = ca.Employee.TimingCode == null ? "" : ca.Employee.TimingCode.ToString(),
                                    Fld7 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                    Fld8 = ca.Employee != null && ca.Employee.Gender != null ? ca.Employee.Gender.LookupVal.ToString() : "",
                                    Fld9 = ca.Employee.ServiceBookDates.BirthDate == null ? "" : ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                    Fld10 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                    Fld11 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                    Fld12 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                    Fld14 = ca.Employee.ServiceBookDates.LastIncrementDate == null ? "" : ca.Employee.ServiceBookDates.LastIncrementDate.Value.ToShortDateString(),
                                    Fld15 = ca.Employee.ServiceBookDates.ProbationDate == null ? "" : ca.Employee.ServiceBookDates.ProbationDate.Value.ToShortDateString(),
                                    Fld13 = ca.Employee.MaritalStatus == null ? "" : ca.Employee.MaritalStatus.LookupVal.ToString(),
                                    Fld16 = ca.Employee.ServiceBookDates.LastTransferDate == null ? "" : ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                    Fld17 = GeoDataInd.GeoStruct_Division_Name == null ? "" : GeoDataInd.GeoStruct_Division_Name,
                                    Fld18 = GeoDataInd.FuncStruct_JobPosition_Name == null ? "" : GeoDataInd.FuncStruct_JobPosition_Name,
                                    Fld31 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                    Fld34 = GeoDataInd.GeoStruct_Department_Name == null ? "" : GeoDataInd.GeoStruct_Department_Name,
                                    Fld35 = GeoDataInd.GeoStruct_Location_Code == null ? "" : GeoDataInd.GeoStruct_Location_Code,
                                    Fld37 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,

                                    Fld40 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,

                                    Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                    Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                    Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                    // Fld54 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.BusinessCategory_Id != null?  ca.Employee.GeoStruct.Location.BusinessCategory.LookupVal.ToString():""  ,
                                    Fld54 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Bank != null ? ca.Employee.EmpOffInfo.Bank.Name.ToString() : "",

                                    Fld42 = ca.Employee != null && ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.ResignationDate != null ? ca.Employee.ServiceBookDates.ResignationDate.Value.ToShortDateString() : "",
                                    Fld43 = GeoDataInd != null ? GeoDataInd.PayStruct_JobStatus_EmpStatus + ", " + GeoDataInd.PayStruct_JobStatus_EmpActingStatus : ""


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


                            return OGenericPayrollStatement;
                        }

                        break;


                    case "EMPOFF":



                        var OEmpoffData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OEmpoffData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.EmpOffInfo)
                                // .Include(e => e.Employee.ServiceBookDates)
                                // .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                // .Include(e => e.Employee.GeoStruct)
                                //  .Include(e => e.Employee.GeoStruct.Location)
                                //  .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //   .Include(e => e.Employee.GeoStruct.Department)
                                //   .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //   .Include(e => e.Employee.PayStruct)
                                //  .Include(e => e.Employee.PayStruct.Grade)
                                //     .Include(e => e.Employee.FuncStruct)
                                //  .Include(e => e.Employee.FuncStruct.Job).AsNoTracking()


                                    .Where(e => e.Employee.Id == item).AsParallel()
                                                   .FirstOrDefault();

                            List<string> BusiCategory = new List<string>();

                            if (OEmpoffData_t != null)
                            {

                                if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                                {
                                    BusiCategory.AddRange(salheadlist);

                                    foreach (string ca1 in BusiCategory)
                                    {
                                        OEmpoffData_t.Employee.EmpName = db.NameSingle.Where(e => e.Id == OEmpoffData_t.Employee.EmpName_Id).FirstOrDefault();
                                        OEmpoffData_t.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Include(e => e.Branch).Where(e => e.Id == OEmpoffData_t.Employee.EmpOffInfo_Id).FirstOrDefault();
                                        OEmpoffData_t.Employee.EmpOffInfo.NationalityID = db.EmpOff.Where(e => e.Id == OEmpoffData_t.Employee.EmpOffInfo_Id).Select(r => r.NationalityID).FirstOrDefault();
                                        OEmpoffData_t.Employee.ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == OEmpoffData_t.Employee.ServiceBookDates_Id).FirstOrDefault();
                                        OEmpoffData_t.Employee.GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct_Id).FirstOrDefault();
                                        OEmpoffData_t.Employee.GeoStruct.Location = db.Location.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct.Location_Id).FirstOrDefault();
                                        OEmpoffData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct.Location.LocationObj_Id).FirstOrDefault();
                                        //OEmpoffData_t.Employee.GeoStruct.Location.BusinessCategory = db.LookupValue.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct.Location.BusinessCategory_Id).FirstOrDefault();
                                        //if (OEmpoffData_t.Employee.GeoStruct != null && OEmpoffData_t.Employee.GeoStruct.Location != null && OEmpoffData_t.Employee.GeoStruct.Location.BusinessCategory != null)
                                        //{
                                        //    if (OEmpoffData_t.Employee.GeoStruct.Location.BusinessCategory.LookupVal.ToString().ToUpper() == ca1.ToUpper())
                                        //    {
                                        //        OEmpoffData.Add(OEmpoffData_t);
                                        //        break;
                                        //    }

                                        //}
                                        if (OEmpoffData_t.Employee != null && OEmpoffData_t.Employee.EmpOffInfo != null && OEmpoffData_t.Employee.EmpOffInfo.Bank_Id != null)
                                        {
                                            if (OEmpoffData_t.Employee.EmpOffInfo.Bank.Name.ToString().ToUpper() == ca1.ToUpper())
                                            {
                                                OEmpoffData.Add(OEmpoffData_t);
                                                break;
                                            }

                                        }

                                    }

                                }
                                else
                                {
                                    OEmpoffData_t.Employee.EmpName = db.NameSingle.Where(e => e.Id == OEmpoffData_t.Employee.EmpName_Id).FirstOrDefault();
                                    OEmpoffData_t.Employee.EmpOffInfo = db.EmpOff.Include(e => e.Bank).Include(e => e.Branch).Where(e => e.Id == OEmpoffData_t.Employee.EmpOffInfo_Id).FirstOrDefault();
                                    OEmpoffData_t.Employee.EmpOffInfo.NationalityID = db.EmpOff.Where(e => e.Id == OEmpoffData_t.Employee.EmpOffInfo_Id).Select(r => r.NationalityID).FirstOrDefault();
                                    OEmpoffData_t.Employee.ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == OEmpoffData_t.Employee.ServiceBookDates_Id).FirstOrDefault();
                                    OEmpoffData_t.Employee.GeoStruct = db.GeoStruct.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct_Id).FirstOrDefault();
                                    OEmpoffData_t.Employee.GeoStruct.Location = db.Location.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct.Location_Id).FirstOrDefault();
                                    OEmpoffData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct.Location.LocationObj_Id).FirstOrDefault();
                                    //OEmpoffData_t.Employee.GeoStruct.Location.BusinessCategory = db.LookupValue.Where(e => e.Id == OEmpoffData_t.Employee.GeoStruct.Location.BusinessCategory_Id).FirstOrDefault();

                                    OEmpoffData.Add(OEmpoffData_t);
                                }

                            }

                        }
                        if (OEmpoffData == null || OEmpoffData.Count() == 0)
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

                            foreach (var ca in OEmpoffData.OrderBy(e => e.Employee.EmpCode))
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

                                GenericField100 OGenericObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Employee.EmpOffInfo.AccountNo,
                                    Fld2 = ca.Employee.EmpOffInfo.PFAppl.ToString(),
                                    Fld3 = ca.Employee.EmpOffInfo.SelfHandicap.ToString(),
                                    Fld4 = ca.Employee.EmpOffInfo.FamilyHandicap.ToString(),
                                    //   Fld5 = ca.Employee.EmpOffInfo.HandicapRemark.ToString(),
                                    Fld5 = ca.Employee.EmpOffInfo.FamilyHandicap == null ? "" : ca.Employee.EmpOffInfo.FamilyHandicap.ToString(),

                                    Fld6 = ca.Employee.EmpOffInfo.PTAppl.ToString(),
                                    Fld7 = ca.Employee.EmpOffInfo.ESICAppl.ToString(),
                                    Fld8 = ca.Employee.EmpOffInfo.LWFAppl.ToString(),
                                    Fld9 = ca.Employee.EmpOffInfo.VPFAppl.ToString(),
                                    //  Fld10 = ca.Employee.EmpOffInfo.VPFAmt.ToString(),
                                    Fld10 = ca.Employee.EmpOffInfo.VPFAmt == null ? "" : ca.Employee.EmpOffInfo.VPFAmt.ToString(),
                                    Fld11 = ca.Employee.EmpOffInfo.VPFPerc == null ? "" : ca.Employee.EmpOffInfo.VPFPerc.ToString(),

                                    Fld12 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                    Fld13 = ca.Employee.EmpCode,
                                    Fld14 = ca.Employee.EmpOffInfo.NationalityID.PANNo == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PANNo.ToString(),
                                    Fld15 = ca.Employee.EmpOffInfo.NationalityID.AdharNo == null ? "" : ca.Employee.EmpOffInfo.NationalityID.AdharNo.ToString(),
                                    Fld16 = ca.Employee.EmpOffInfo.NationalityID.PFNo == null ? "" : ca.Employee.EmpOffInfo.NationalityID.PFNo.ToString(),

                                    //Fld31 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                    Fld31 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Branch_Id != null ? ca.Employee.EmpOffInfo.Branch.Code.ToString() : "",
                                    Fld32 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Branch_Id != null ? ca.Employee.EmpOffInfo.Branch.Name.ToString() : "",
                                    Fld51 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                    Fld52 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                    Fld53 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                    //Fld54 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.BusinessCategory_Id != null ? ca.Employee.GeoStruct.Location.BusinessCategory.LookupVal.ToString() : "",
                                    Fld54 = ca.Employee != null && ca.Employee.EmpOffInfo != null && ca.Employee.EmpOffInfo.Bank_Id != null ? ca.Employee.EmpOffInfo.Bank.Name.ToString() : "",



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


                            return OGenericPayrollStatement;
                        }

                        break;


                    case "HRAEXEMPTIONMASTER":

                        var HRAMasterdata = db.HRAExemptionMaster.Include(e => e.City.Select(t => t.Category)).ToList();

                        if (HRAMasterdata == null || HRAMasterdata.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in HRAMasterdata)
                            {
                                var citydetails = ca.City.ToList();
                                foreach (var item in citydetails)
                                {
                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.Ctypeper.ToString(),
                                        Fld2 = ca.RentPer.ToString(),
                                        Fld3 = item.Name,
                                        Fld4 = item.Category == null ? "" : item.Category.LookupVal
                                    };
                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "EMPSALSTRUCT":

                        List<EmployeePayroll> OEmpSalStructData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OEmpSalStructData_t = db.EmployeePayroll
                           .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.EmpSalStruct)

                           .OrderBy(e => e.Id)
                           .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel().FirstOrDefault();

                            if (OEmpSalStructData_t != null)
                            {
                                OEmpSalStructData.Add(OEmpSalStructData_t);
                            }

                        }

                        if (OEmpSalStructData.Select(e => e.EmpSalStruct) == null || OEmpSalStructData.Select(e => e.EmpSalStruct).Count() == 0)
                        {

                            return null;
                        }
                        else
                        {
                            foreach (var l in OEmpSalStructData)
                            {

                                string paymonth = mPayMonth.FirstOrDefault();
                                IEnumerable<EmpSalStruct> Oempall = l.EmpSalStruct.Where(r => paymonth == r.EffectiveDate.Value.ToString("MM/yyyy")).ToList();
                                if (Oempall != null)
                                {
                                    foreach (var ca9 in Oempall)
                                    {
                                        using (DataBaseContext db1 = new DataBaseContext())
                                        {
                                            var empall = db1.EmpSalStruct
                                                    .Include(r => r.GeoStruct.Location)
                                                   .Include(r => r.GeoStruct.Location.LocationObj)
                                                   .Include(r => r.GeoStruct.Department)
                                                   .Include(r => r.GeoStruct.Department.DepartmentObj)
                                                   .Include(r => r.PayStruct.Grade)
                                                   .Include(r => r.PayStruct.JobStatus)
                                                   .Include(r => r.EmpSalStructDetails.Select(t => t.SalaryHead))
                                                   .Include(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type))
                                                   .Include(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency))
                                                   .Include(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))
                                                   .Include(r => r.EmpSalStructDetails).Where(e => e.Id == ca9.Id).AsNoTracking().AsParallel().FirstOrDefault();

                                            foreach (var ca2 in empall.EmpSalStructDetails)
                                            {

                                                if (ca2.SalaryHead != null && ca2.Amount != 0)
                                                {

                                                    GenericField100 OGenericObj = new GenericField100()
                                                    //write data to generic class
                                                    {

                                                        Fld1 = l.Employee.Id.ToString(),
                                                        Fld2 = l.Employee.EmpCode,
                                                        Fld3 = l.Employee.EmpName.FullNameFML,
                                                        Fld5 = ca9.EffectiveDate.Value.ToShortDateString(),
                                                        Fld6 = ca9.EndDate == null ? "" : ca9.EndDate.Value.ToShortDateString(),
                                                        Fld52 = ca2.SalaryHead.Id.ToString(),
                                                        Fld53 = ca2.SalaryHead.Code.ToString(),
                                                        Fld54 = ca2.SalaryHead.Name.ToString(),
                                                        Fld55 = ca2.SalaryHead.Type.LookupVal.ToUpper().ToString(),
                                                        Fld56 = ca2.SalaryHead.Frequency.LookupVal.ToUpper().ToString(),
                                                        Fld57 = ca2.SalaryHead.SalHeadOperationType.LookupVal.ToUpper().ToString(),
                                                        Fld58 = ca2.Amount == null ? "" : ca2.Amount.ToString(),
                                                        Fld16 = empall.GeoStruct != null && empall.GeoStruct.Location != null && empall.GeoStruct.Location.LocationObj != null ? empall.GeoStruct.Location.LocationObj.LocDesc : "",
                                                        Fld19 = empall.GeoStruct != null && empall.GeoStruct.Department != null && empall.GeoStruct.Department.DepartmentObj != null ? empall.GeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                        Fld34 = empall.PayStruct != null && empall.PayStruct.Grade != null ? empall.PayStruct.Grade.Name : "",

                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObj);
                                                }
                                            }

                                        }
                                    }
                                }

                            }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "EMPLOYEESENIORITY":



                        List<EmployeePayroll> OEmpSeniorityData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OEmpSeniorityData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                               .Include(e => e.Employee.EmpName)
                               .Include(e => e.Employee.ServiceBookDates)
                               .Include(e => e.PromotionServiceBook)
                               .Include(e => e.PromotionServiceBook.Select(q => q.OldPayStruct))
                               .Include(e => e.PromotionServiceBook.Select(q => q.OldPayStruct.Grade))
                               .Include(e => e.PromotionServiceBook.Select(q => q.NewPayStruct))
                               .Include(e => e.PromotionServiceBook.Select(q => q.NewPayStruct.Grade))
                               .Include(e => e.Employee.EmpSocialInfo)
                               .Include(e => e.Employee.EmpAcademicInfo)
                               .Include(e => e.Employee.EmpAcademicInfo.QualificationDetails)
                               .Include(e => e.Employee.EmpAcademicInfo.QualificationDetails.Select(q => q.Qualification))
                               .Include(e => e.Employee.EmpSocialInfo.Category)
                               .Include(e => e.Employee.GeoStruct)
                               .Include(e => e.Employee.PayStruct)
                               .Include(e => e.Employee.FuncStruct).AsNoTracking()
                                 .Where(e => e.Employee.Id == item && e.Employee.EmpAcademicInfo != null)
                                                   .FirstOrDefault();

                            if (OEmpSeniorityData_t != null)
                            {
                                OEmpSeniorityData.Add(OEmpSeniorityData_t);
                            }
                        }

                        if (OEmpSeniorityData == null || OEmpSeniorityData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var Exp1 = 0;
                            var Exp2 = 0;
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

                            foreach (var ca in OEmpSeniorityData)
                            {
                                string qualificationdesc = "";
                                var EmpAcademic = ca.Employee.EmpAcademicInfo.QualificationDetails;
                                string senioritydate = "";
                                var compname = db.Company.Select(e => e.Code).SingleOrDefault();
                                var Seniorityno = ca.Employee.ServiceBookDates.SeniorityNo == null ? "" : ca.Employee.ServiceBookDates.SeniorityNo;

                                if (ca.Employee.ServiceBookDates.SeniorityDate != null)
                                {
                                    senioritydate = ca.Employee.ServiceBookDates.SeniorityDate.Value.ToShortDateString();
                                }
                                ////// for kdcc
                                if (compname.ToUpper() == "KDCC")
                                {
                                    //foreach (var ca1 in EmpAcademic)
                                    //{

                                    var EmpGrade = "";
                                    var promotionDate = "";

                                    var RetDate = ca.Employee != null && ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate : null;
                                    var Experi = 0;
                                    var ExperiMonth = 0;
                                    var ExperiCurrMonth = 0;
                                    Experi = RetDate.Value.Year - DateTime.Now.Year;
                                    ExperiMonth = RetDate.Value.Month;
                                    ExperiCurrMonth = 12 - DateTime.Now.Month;
                                    if (DateTime.Now.Year < RetDate.Value.Year)
                                    {
                                        Exp1 = Experi - 1;
                                    }
                                    else
                                    {
                                        Exp1 = 0;
                                    }
                                    if (ExperiMonth > 0)
                                    {
                                        Exp2 = ExperiMonth + ExperiCurrMonth;
                                    }

                                    else
                                    {
                                        Exp2 = 0;
                                    }



                                    string checkdate = "01/01/2020";
                                    var promodate1 = "";
                                    var promodate2 = "";
                                    var promodate3 = "";
                                    var promodate4 = "";
                                    var promodate5 = "";
                                    var promodate6 = "";
                                    var promodate7 = "";

                                    var check = ca.PromotionServiceBook.Where(e => e.ReleaseFlag == true).ToList();

                                    if (ca.Employee.ServiceBookDates.ConfirmationDate.Value > Convert.ToDateTime(checkdate))
                                    {
                                        foreach (var item in ca.PromotionServiceBook)
                                        {
                                            EmpGrade = item.OldPayStruct == null ? "" : item.OldPayStruct.Grade.Code;
                                            promotionDate = ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString();
                                            if (EmpGrade == "150")
                                            {
                                                promodate1 = promotionDate;
                                            }
                                            if (EmpGrade == "140")
                                            {
                                                promodate2 = promotionDate;
                                            }
                                            if (EmpGrade == "130")
                                            {
                                                promodate3 = promotionDate;
                                            }
                                            if (EmpGrade == "120")
                                            {
                                                promodate4 = promotionDate;
                                            }
                                            if (EmpGrade == "100")
                                            {
                                                promodate5 = promotionDate;
                                            }
                                            if (EmpGrade == "40")
                                            {
                                                promodate6 = promotionDate;
                                            }
                                            if (EmpGrade == "20")
                                            {
                                                promodate7 = promotionDate;
                                            }
                                        }

                                    }
                                    //if (check.Count() == 0)
                                    //{
                                    //    var dfk = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Code;


                                    //    EmpGrade = dfk.ToString();
                                    //    promotionDate = ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString();
                                    //    if (EmpGrade == "150")
                                    //    {
                                    //        promodate1 = promotionDate;
                                    //    }
                                    //    if (EmpGrade == "140")
                                    //    {
                                    //        promodate2 = promotionDate;
                                    //    }
                                    //    if (EmpGrade == "130")
                                    //    {
                                    //        promodate3 = promotionDate;
                                    //    }
                                    //    if (EmpGrade == "120")
                                    //    {
                                    //        promodate4 = promotionDate;
                                    //    }
                                    //    if (EmpGrade == "100")
                                    //    {
                                    //        promodate5 = promotionDate;
                                    //    }

                                    //}
                                    if (check.Count() > 0)
                                    {
                                        foreach (var item in ca.PromotionServiceBook)
                                        {
                                            EmpGrade = item.NewPayStruct == null ? "" : item.NewPayStruct.Grade.Code;
                                            promotionDate = item.ReleaseDate.Value.ToShortDateString();
                                            if (EmpGrade == "150")
                                            {
                                                promodate1 = promotionDate;
                                            }
                                            if (EmpGrade == "140")
                                            {
                                                promodate2 = promotionDate;
                                            }
                                            if (EmpGrade == "130")
                                            {
                                                promodate3 = promotionDate;
                                            }
                                            if (EmpGrade == "120")
                                            {
                                                promodate4 = promotionDate;
                                            }
                                            if (EmpGrade == "100")
                                            {
                                                promodate5 = promotionDate;
                                            }
                                            if (EmpGrade == "40")
                                            {
                                                promodate6 = promotionDate;
                                            }
                                            if (EmpGrade == "20")
                                            {
                                                promodate7 = promotionDate;
                                            }

                                        }
                                    }

                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                    
                                    foreach (var ca1 in EmpAcademic)
                                    {
                                       
                                        if (qualificationdesc == "")
                                        {
                                            qualificationdesc = string.Join(", ", ca1.Qualification.Select(q => q.QualificationDesc).ToList());
                                        
                                        }
                                        else
                                        {
                                            qualificationdesc = qualificationdesc + "," + string.Join(", ", ca1.Qualification.Select(q => q.QualificationDesc).ToList());
                                           
                                        }
                                    }

                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld2 = ca.Employee.EmpCode,
                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                        Fld4 = qualificationdesc,
                                        Fld5 = ca.Employee.EmpSocialInfo == null ? "" : ca.Employee.EmpSocialInfo.Category == null ? "" : ca.Employee.EmpSocialInfo.Category.LookupVal.ToString(),

                                        Fld6 = ca.Employee.ServiceBookDates.BirthDate == null ? "" : ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                        Fld7 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                        Fld8 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),

                                        Fld9 = promodate1,
                                        Fld10 = promodate2,
                                        Fld13 = promodate3,
                                        Fld14 = promodate4,
                                        Fld15 = promodate5,
                                        Fld16 = promodate6,
                                        Fld17 = promodate7,

                                        Fld11 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                        Fld12 = Exp1.ToString() + "year" + Exp2.ToString() + "month",
                                        Fld50 = "",
                                        Fld19 = Seniorityno,
                                        Fld20 = senioritydate,
                                        Fld21 = GeoDataInd!=null ? GeoDataInd.LocDesc : "",
                                        Fld22 = FuncdataInd!=null ? FuncdataInd.Job_Name : ""
                                        
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

                                    //}


                                }

                                /////////for assam

                                else
                                {
                                    //foreach (var ca1 in EmpAcademic)
                                    //{


                                    var EmpGrade = "";
                                    var promotionDate = "";


                                    var RetDate = ca.Employee != null && ca.Employee.ServiceBookDates != null && ca.Employee.ServiceBookDates.RetirementDate != null ? ca.Employee.ServiceBookDates.RetirementDate : null;
                                    var Experi = 0;
                                    var ExperiMonth = 0;
                                    var ExperiCurrMonth = 0;
                                    Experi = RetDate.Value.Year - DateTime.Now.Year;
                                    ExperiMonth = RetDate.Value.Month;
                                    ExperiCurrMonth = 12 - DateTime.Now.Month;
                                    if (DateTime.Now.Year < RetDate.Value.Year)
                                    {
                                        Exp1 = Experi - 1;
                                    }
                                    else
                                    {
                                        Exp1 = 0;
                                    }
                                    if (ExperiMonth > 0)
                                    {
                                        Exp2 = ExperiMonth + ExperiCurrMonth;
                                    }

                                    else
                                    {
                                        Exp2 = 0;
                                    }
                                    var check = ca.PromotionServiceBook.Where(e => e.ReleaseFlag == true).ToList();



                                    if (check.Count() > 0)
                                    {
                                        foreach (var item in check)
                                        {
                                            EmpGrade = item.OldPayStruct == null ? "" : item.OldPayStruct.Grade.Name;
                                            promotionDate = item.ReleaseDate.Value.ToShortDateString();

                                        }
                                    }

                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    foreach (var ca1 in EmpAcademic)
                                    {

                                        if (qualificationdesc == "")
                                        {
                                            qualificationdesc = string.Join(", ", ca1.Qualification.Select(q => q.QualificationDesc).ToList());

                                        }
                                        else
                                        {
                                            qualificationdesc = qualificationdesc + "," + string.Join(", ", ca1.Qualification.Select(q => q.QualificationDesc).ToList());

                                        }
                                    }

                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld2 = ca.Employee.EmpCode,
                                        Fld3 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),

                                        //Fld4 = string.Join(", ", ca1.Qualification.Select(q => q.QualificationDesc).ToList()),
                                        Fld4 = qualificationdesc,
                                        Fld5 = ca.Employee.EmpSocialInfo == null ? "" : ca.Employee.EmpSocialInfo.Category == null ? "" : ca.Employee.EmpSocialInfo.Category.LookupVal.ToString(),

                                        Fld6 = ca.Employee.ServiceBookDates.BirthDate == null ? "" : ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                        Fld7 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                        Fld8 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),

                                        Fld9 = EmpGrade,
                                        Fld10 = promotionDate,

                                        Fld11 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                        Fld12 = Exp1.ToString() + "year" + Exp2.ToString() + "month",
                                        Fld50 = "Assam",
                                        Fld19 = Seniorityno,
                                        Fld20 = senioritydate,
                                        Fld21 = GeoDataInd != null ? GeoDataInd.LocDesc : "",
                                        Fld22 = FuncdataInd != null ? FuncdataInd.Job_Name : ""
                                        
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
                            }
                            return OGenericPayrollStatement;
                        }

                        break;



                    case "SERVICESECURITYMASTER":

                        List<EmployeePayroll> OServiceSec = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OServiceSec_temp = db.EmployeePayroll
                           .Include(e => e.Employee)
                           .Include(e => e.Employee.GeoStruct)
                           .Include(e => e.Employee.PayStruct)
                           .Include(e => e.Employee.FuncStruct)
                           .Include(e => e.ServiceSecurity)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.ServiceSecurity.Select(t => t.Location))
                            .Include(e => e.ServiceSecurity.Select(t => t.Location.LocationObj))

                          .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel().FirstOrDefault();

                            if (OServiceSec_temp != null)
                            {
                                OServiceSec.Add(OServiceSec_temp);
                            }

                        }


                        if (OServiceSec == null)
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

                            foreach (var ca in OServiceSec)
                            {
                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                {
                                    var Service = ca.ServiceSecurity.ToList();
                                    if (Service.Count() > 0)
                                    {
                                        foreach (var item in Service)
                                        {

                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Id.ToString(),
                                                Fld2 = ca.Employee.EmpCode.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = item.Amount == null ? "" : item.Amount.ToString(),
                                                Fld5 = item.FDR_No == null ? "" : item.FDR_No.ToString(),
                                                Fld6 = item.Date == null ? "" : item.Date.Value.ToShortDateString(),
                                                Fld7 = item.DateOfMaturity == null ? "" : item.DateOfMaturity.Value.ToShortDateString(),
                                                Fld8 = item.Location == null ? "" : item.Location.LocationObj.LocDesc.ToString(),
                                                Fld9 = item.Closer == null ? "" : item.Closer.ToString(),
                                                Fld10 = item.DateOfCloser == null ? "" : item.DateOfCloser.Value.ToShortDateString(),
                                                Fld11 = item.Remark == null ? "" : item.Remark.ToString(),
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

                                            //write data to generic class
                                            OGenericPayrollStatement.Add(OGenericObjStatement);

                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;

                        }
                        break;

                    case "SERVICESECURITYFDCLOSERMASTER":

                        string pmonth = mPayMonth.FirstOrDefault();
                        List<EmployeePayroll> OServiceSecFD = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OServiceSecFD_temp = db.EmployeePayroll
                           .Include(e => e.Employee)
                           .Include(e => e.Employee.GeoStruct)
                           .Include(e => e.Employee.GeoStruct.Location)
                           .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                           .Include(e => e.Employee.PayStruct)
                           .Include(e => e.Employee.FuncStruct)
                           .Include(e => e.ServiceSecurity)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.ServiceSecurity.Select(t => t.Location))
                           .Include(e => e.ServiceSecurity.Select(t => t.Location.LocationObj))
                           .Include(e => e.Employee.EmpOffInfo)
                                //  .Include(e => e.Employee.EmpOffInfo.Bank)
                           .Include(e => e.Employee.EmpOffInfo.Branch)
                          .AsNoTracking()
                          .Where(e => e.Employee.Id == item)
                          .FirstOrDefault();

                            if (OServiceSecFD_temp != null)
                            {
                                OServiceSecFD.Add(OServiceSecFD_temp);
                            }

                        }


                        if (OServiceSecFD == null)
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


                            var SpGroup = SpecialGroupslist.ToList();
                            var WorkLocation = false;
                            //var loanLocation = false;
                            var accno = false;

                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "WorkingLocation")
                                {
                                    WorkLocation = true;
                                }
                                if (item1 == "FDOpenLocation")
                                {
                                    WorkLocation = false;
                                }
                            }
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

                            foreach (var ca in OServiceSecFD)
                            {
                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                {
                                    var LoanBranch = "";
                                    var LocBranch = "";
                                    var Loc = "";

                                    var Service = ca.ServiceSecurity.ToList();
                                    if (Service.Count() > 0)
                                    {
                                        foreach (var item in Service)
                                        {

                                            if (item.DateOfMaturity >= pFromDate && item.DateOfMaturity <= pToDate)
                                            {
                                                if (WorkLocation == true)
                                                {
                                                    LoanBranch = GeoDataInd.LocDesc;
                                                    Loc = "Working Location";
                                                    LocBranch = ca.Employee.GeoStruct == null || ca.Employee.GeoStruct.Location == null || ca.Employee.GeoStruct.Location.LocationObj == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc;
                                                }
                                                else
                                                {
                                                    LoanBranch = item.Location == null || item.Location.LocationObj == null ? "" : item.Location.LocationObj.LocDesc;
                                                    Loc = "FD Open Branch";
                                                    LocBranch = GeoDataInd.LocDesc;
                                                }
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode.ToString(),
                                                    Fld3 = ca.Employee.EmpName.FullNameFML.ToString(),
                                                    Fld4 = item.Amount == null ? "" : item.Amount.ToString(),
                                                    Fld5 = item.FDR_No == null ? "" : item.FDR_No.ToString(),
                                                    Fld6 = item.Date == null ? "" : item.Date.Value.ToShortDateString(),
                                                    Fld7 = item.DateOfMaturity == null ? "" : item.DateOfMaturity.Value.ToShortDateString(),
                                                    Fld8 = LoanBranch,
                                                    Fld9 = item.Closer == null ? "" : item.Closer.ToString(),
                                                    Fld10 = item.DateOfCloser == null ? "" : item.DateOfCloser.Value.ToShortDateString(),
                                                    Fld11 = item.Remark == null ? "" : item.Remark.ToString(),
                                                    Fld12 = Loc,
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
                                                    OGenericObjStatement.Fld97 = LocBranch;
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

                                                //write data to generic class
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;

                        }
                        break;

                    case "EMPLOYEESUMMARYMASTER":

                        var type = ReportType;
                        if (type == "DETAILS")
                        {
                            var OEmpData = new List<EmployeePayroll>();
                            foreach (var item in EmpPayrollIdList)
                            {
                                var OEmpInfoData_t = db.EmployeePayroll
                                   .Include(e => e.Employee)
                                    .Include(e => e.Employee.GeoStruct)
                                   .Include(e => e.Employee.PayStruct)
                                   .Include(e => e.Employee.FuncStruct)
                                   .Include(e => e.Employee.EmpName)
                                   .Include(e => e.Employee.Gender)
                                   .Include(e => e.Employee.MaritalStatus)
                                   .Include(e => e.Employee.EmpOffInfo)
                                   .Include(e => e.Employee.ServiceBookDates)
                                   .Include(e => e.SalaryT)
                                    //.Include(e => e.SalaryT.Select(t => t.Geostruct))
                                    //.Include(e => e.SalaryT.Select(t => t.PayStruct))
                                    //.Include(e => e.SalaryT.Select(t => t.FuncStruct))
                                   .Where(e => e.Employee.Id == item).AsNoTracking().FirstOrDefault();

                                if (OEmpInfoData_t != null)
                                {
                                    OEmpData.Add(OEmpInfoData_t);
                                }
                            }

                            if (OEmpData == null || OEmpData.Count() == 0)
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

                                foreach (var ca in OEmpData.OrderBy(e => e.Employee.EmpCode))
                                {
                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.Employee.EmpCode,
                                        Fld2 = ca.Employee.BirthPlace,
                                        Fld3 = ca.Employee.CardCode,
                                        Fld4 = ca.Employee.ValidFromDate == null ? "" : ca.Employee.ValidFromDate.Value.ToShortDateString(),
                                        Fld5 = ca.Employee.ValidToDate == null ? "" : ca.Employee.ValidToDate.Value.ToShortDateString(),


                                        Fld6 = ca.Employee.TimingCode == null ? "" : ca.Employee.TimingCode.ToString(),
                                        Fld7 = ca.Employee.EmpName.FullNameFML == null ? "" : ca.Employee.EmpName.FullNameFML.ToString(),
                                        Fld8 = ca.Employee.Gender.LookupVal.ToString() == null ? "" : ca.Employee.Gender.LookupVal.ToString(),
                                        Fld9 = ca.Employee.ServiceBookDates.BirthDate == null ? "" : ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                        Fld10 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                        Fld11 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                        Fld12 = ca.Employee.ServiceBookDates.JoiningDate == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                        Fld14 = ca.Employee.ServiceBookDates.LastIncrementDate == null ? "" : ca.Employee.ServiceBookDates.LastIncrementDate.Value.ToShortDateString(),
                                        Fld15 = ca.Employee.ServiceBookDates.ProbationDate == null ? "" : ca.Employee.ServiceBookDates.ProbationDate.Value.ToShortDateString(),
                                        Fld13 = ca.Employee.MaritalStatus == null ? "" : ca.Employee.MaritalStatus.LookupVal.ToString(),
                                        Fld16 = ca.Employee.ServiceBookDates.LastTransferDate == null ? "" : ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                        Fld31 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                        Fld34 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.DeptDesc,
                                        Fld35 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                        Fld37 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,

                                        Fld40 = FuncdataInd.Job_Name == null ? "" : FuncdataInd.Job_Name,

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


                                return OGenericPayrollStatement;
                            }
                        }
                        else
                        {
                            List<EmployeePayroll> OEmpSum = new List<EmployeePayroll>();
                            string paymon = mPayMonth.FirstOrDefault();
                            foreach (var item in EmpPayrollIdList)
                            {

                                var OOEmpSum_temp = db.EmployeePayroll
                               .Include(e => e.Employee)
                               .Include(e => e.Employee.GeoStruct)
                               .Include(e => e.Employee.PayStruct)
                               .Include(e => e.Employee.FuncStruct)
                               .Include(e => e.SalaryT)
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
                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    if (GeoDataInd != null && PaydataInd != null && FuncdataInd != null)
                                    {

                                        var EmpSum = ca.SalaryT.Where(e => e.PayMonth == paymon);

                                        if (EmpSum.Count() > 0)
                                        {
                                            foreach (var item in EmpSum)
                                            {
                                                ////filter

                                                if (loca == true)
                                                {
                                                    group = item.Geostruct.Location.LocationObj.LocDesc;
                                                }
                                                if (div == true)
                                                {
                                                    group = item.Geostruct.Division.Name;
                                                }
                                                if (dept == true)
                                                {
                                                    group = item.Geostruct.Department.DepartmentObj.DeptDesc;
                                                }
                                                if (job == true)
                                                {
                                                    group = item.FuncStruct.Job.Name;
                                                }
                                                ///////special group 

                                                if (JOB == true)
                                                {
                                                    filter = item.FuncStruct.Job.Name;
                                                }
                                                if (GRADE == true)
                                                {
                                                    filter = item.PayStruct.Grade.Name;
                                                }
                                                if (JOBPOSITION == true)
                                                {
                                                    filter = item.FuncStruct.JobPosition.JobPositionDesc;
                                                }

                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {
                                                    Fld1 = header.ToString(),
                                                    Fld2 = ca.Employee.EmpCode.ToString(),
                                                    Fld4 = group.ToString(),
                                                    Fld5 = filter.ToString(),

                                                    Fld50 = type.ToString(),

                                                };

                                                //write data to generic class
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                        }
                                    }
                                }
                                return OGenericPayrollStatement;
                            }
                        }
                        break;

                    case "GRADEMASTER":
                        var OGeoGradeM = db.Company
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.Grade).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

                        if (OGeoGradeM == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OGeoGradeM.Grade)
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



                    case "CALENDARMASTER":
                        var OCalMASTER = db.Company
                            .Include(e => e.Calendar)
                            .Include(e => e.Calendar.Select(t => e.Name))
                        .Where(d => d.Id == CompanyId).AsNoTracking().SingleOrDefault();

                        if (OCalMASTER == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OCalMASTER.Calendar)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Name == null ? "" : ca.Name.LookupVal.ToString(),
                                    Fld3 = ca.FromDate == null ? "" : ca.FromDate.Value.ToShortDateString(),
                                    Fld4 = ca.ToDate == null ? "" : ca.ToDate.Value.ToShortDateString(),
                                    Fld5 = ca.Default == null ? "" : ca.Default.ToString(),
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "TRAVELMODERATECEILINGPOLICYMASTER":
                        var OtravelmoderateceilingpolicyMaster = db.TravelModeRateCeilingPolicy
                             .Include(e => e.TravelModeEligibilityPolicy)
                              .Include(e => e.TA_TMRC_Eligibility_Wages)
                              .Include(e => e.DistanceRange)
                        .AsNoTracking().ToList();

                        if (OtravelmoderateceilingpolicyMaster == null)
                        {
                            return null;
                        }
                        else
                        {

                            foreach (var ca in OtravelmoderateceilingpolicyMaster)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.TA_TMRC_Elligibilty_Code.ToString(),
                                    Fld3 = ca.TravelModeEligibilityPolicy == null ? "" : ca.TravelModeEligibilityPolicy.FullDetails.ToString(),
                                    Fld4 = ca.TA_TMRC_Eligibility_Wages == null ? "" : ca.TA_TMRC_Eligibility_Wages.FullDetails.ToString(),
                                  //  Fld5 = ca.DistanceRange == null ? "" : ca.DistanceRange.FullDetails.ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);


                                //write data to generic class

                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "TRAVELELIGIBILITYPOLICYMASTER":
                        var OtraveleligibilitypolicyMaster = db.TravelEligibilityPolicy
                             .Include(e => e.TA_Eligibility_Wages)
                              .Include(e => e.WagesRange)
                              .Include(e => e.TravelModeEligibilityPolicy)
                        .AsNoTracking().ToList();

                        if (OtraveleligibilitypolicyMaster == null)
                        {
                            return null;
                        }
                        else
                        {

                            foreach (var ca in OtraveleligibilitypolicyMaster)
                            {
                                if (ca.TravelModeEligibilityPolicy.Count() > 0)
                                {
                                    foreach (var ca1 in ca.TravelModeEligibilityPolicy)
                                    {
                                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca.Id.ToString(),
                                            Fld2 = ca.TA_TC_Eligibilty_Code.ToString(),
                                            Fld3 = ca.TA_Eligibility_Wages == null ? "" : ca.TA_Eligibility_Wages.FullDetails.ToString(),
                                           // Fld4 = ca.WagesRange == null ? "" : ca.WagesRange.FullDetails.ToString(),

                                            Fld5 = ca1.FullDetails == null ? "" : ca1.FullDetails.ToString(),

                                        };
                                        OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                    }
                                }
                                else
                                {
                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.TA_TC_Eligibilty_Code.ToString(),
                                        Fld3 = ca.TA_Eligibility_Wages == null ? "" : ca.TA_Eligibility_Wages.FullDetails.ToString(),
                                      //  Fld4 = ca.WagesRange == null ? "" : ca.WagesRange.FullDetails.ToString(),

                                        Fld5 = "",

                                    };
                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                                //write data to generic class

                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "TRAVELMODEELIGIBILITYPOLICYMASTER":
                        var OtravelmodeeligibilitypolicyMaster = db.TravelModeEligibilityPolicy
                             .Include(e => e.TravelMode)
                              .Include(e => e.ClassOfTravel)
                        .AsNoTracking().ToList();

                        if (OtravelmodeeligibilitypolicyMaster == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OtravelmodeeligibilitypolicyMaster)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.TA_TM_Elligibilty_Code.ToString(),
                                    Fld3 = ca.TravelMode == null ? "" : ca.TravelMode.LookupVal.ToString(),
                                    Fld4 = ca.ClassOfTravel == null ? "" : ca.ClassOfTravel.LookupVal.ToString(),

                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "HOTELELIGIBILITYPOLICYMASTER":
                        var OhoteleligibilitypolicyMASTER = db.HotelEligibilityPolicy
                             .Include(e => e.HotelType)
                              .Include(e => e.RoomType)
                        .AsNoTracking().ToList();

                        if (OhoteleligibilitypolicyMASTER == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OhoteleligibilitypolicyMASTER)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.HotelEligibilityCode.ToString(),
                                    Fld3 = ca.HotelType == null ? "" : ca.HotelType.LookupVal.ToString(),
                                    Fld4 = ca.RoomType == null ? "" : ca.RoomType.LookupVal.ToString(),
                                    Fld5 = ca.Lodging_Eligible_Amt_PerDay.ToString(),
                                    Fld6 = ca.Food_Eligible_Amt_PerDay.ToString(),
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "LTCGLOBALBLOCKMASTER":
                        var OLTCGlobalblockMaster = db.GlobalLTCBlock
                            .AsNoTracking().ToList();

                        if (OLTCGlobalblockMaster == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OLTCGlobalblockMaster)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    // Fld2 = ca.Name == null ? "" : ca.Name.LookupVal.ToString(),
                                    Fld3 = ca.BlockStart == null ? "" : ca.BlockStart.Value.ToShortDateString(),
                                    Fld4 = ca.BlockEnd == null ? "" : ca.BlockEnd.Value.ToShortDateString(),
                                    Fld5 = ca.BlockYear == null ? "" : ca.BlockYear.ToString(),
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "HOLIDAYCALENDARMASTER":
                        var OHoliDataM = db.Company
                            .Include(a => a.HolidayCalendar.Select(t => t.HoliCalendar.Name))
                             .Include(a => a.HolidayCalendar.Select(t => t.HolidayList.Select(s => s.Holiday)))
                             .Include(a => a.HolidayCalendar.Select(t => t.HolidayList.Select(s => s.Holiday.HolidayName)))
                             .Include(a => a.HolidayCalendar.Select(t => t.HolidayList.Select(s => s.Holiday)))
                            .Where(d => d.Id == CompanyId).AsNoTracking().SingleOrDefault();

                        if (OHoliDataM == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OHoliDataM.HolidayCalendar)
                            {
                                if (ca.HolidayList.Count > 0)
                                {
                                    foreach (var ca1 in ca.HolidayList)
                                    {
                                        if (ca1.HolidayDate >= mFromDate && ca1.HolidayDate <= mToDate)
                                        {
                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Id.ToString(),
                                                Fld2 = ca.HoliCalendar == null ? "" : ca.HoliCalendar.Name == null ? "" : ca.HoliCalendar.Name.LookupVal.ToString(),
                                                Fld3 = ca1.Holiday == null ? "" : ca1.Holiday.HolidayName == null ? "" : ca1.Holiday.HolidayName.LookupVal.ToString(),
                                                Fld4 = ca1.HolidayDate == null ? "" : ca1.HolidayDate.Value.ToShortDateString(),

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



                    case "WEEKLYOFFCALENDARMASTER":
                        var OWeeklyM = db.Company
                            .Include(a => a.WeeklyOffCalendar.Select(t => t.WOCalendar.Name))
                             .Include(a => a.WeeklyOffCalendar.Select(t => t.WeeklyOffList.Select(s => s.WeekDays)))
                             .Include(a => a.WeeklyOffCalendar.Select(t => t.WeeklyOffList.Select(s => s.WeeklyOffStatus)))
                              .Where(d => d.Id == CompanyId).AsNoTracking().AsParallel().SingleOrDefault();

                        if (OWeeklyM.WeeklyOffCalendar == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OWeeklyM.WeeklyOffCalendar)
                            {
                                if (ca.WeeklyOffList.Count > 0)
                                {
                                    foreach (var ca1 in ca.WeeklyOffList)
                                    {
                                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca.Id.ToString(),
                                            Fld2 = ca.WOCalendar == null ? "" : ca.WOCalendar.Name == null ? "" : ca.WOCalendar.Name.LookupVal.ToString(),
                                            Fld3 = ca1.WeekDays == null ? "" : ca1.WeekDays.LookupVal.ToString(),
                                            Fld4 = ca1.WeeklyOffStatus == null ? "" : ca1.WeeklyOffStatus.LookupVal.ToString(),
                                        };
                                        //write data to generic class
                                        OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;

                        }
                        break;




                    case "PAYBANKMASTER":
                        var OBankM = db.Company
                            .Include(e => e.Bank)
                            .Include(e => e.Bank.Select(t => t.Address))
                            .Include(e => e.Bank.Select(t => t.ContactDetails))
                            .Include(e => e.Bank.Select(t => t.Branches))
                          .Where(d => d.Id == CompanyId).AsNoTracking().AsParallel().SingleOrDefault();

                        if (OBankM.Bank == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OBankM.Bank)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Code,
                                    Fld3 = ca.Name,
                                    Fld4 = ca.Address == null ? "" : ca.Address.FullAddress,
                                    Fld5 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }
                        break;




                    case "PAYPROCESSGROUPMASTER":
                        var OPayDataM = db.Company
                            .Include(t => t.PayProcessGroup)
                            .Include(t => t.PayProcessGroup.Select(s => s.PayFrequency))
                            .Include(t => t.PayProcessGroup.Select(s => s.PayMonthConcept))
                            .Include(t => t.PayProcessGroup.Select(s => s.PayrollPeriod))
                        .Where(d => d.Id == CompanyId).AsNoTracking().AsParallel().SingleOrDefault();

                        if (OPayDataM.PayProcessGroup == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPayDataM.PayProcessGroup)
                            {
                                foreach (var ca1 in ca.PayrollPeriod)
                                {
                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.Name,
                                        Fld3 = ca.PayFrequency.LookupVal == null ? "" : ca.PayFrequency.LookupVal.ToString(),
                                        Fld4 = ca.PayMonthConcept.LookupVal == null ? "" : ca.PayMonthConcept.LookupVal.ToString(),
                                        Fld5 = ca1.StartDate.ToString(),
                                        Fld6 = ca1.EndDate.ToString()
                                    };
                                    //write data to generic class
                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;



                    case "PAYSCALEMASTER":
                        var OPaySCALEM = db.Company
                            .Include(e => e.PayScale)
                            .Include(e => e.PayScale.Select(t => t.CPIEntryT))
                            .Include(e => e.PayScale.Select(t => t.PayScaleArea.Select(c => c.LocationObj)))
                            .Include(e => e.PayScale.Select(t => t.PayScaleType))
                        .Where(d => d.Id == CompanyId).AsNoTracking().AsParallel().SingleOrDefault();

                        if (OPaySCALEM.PayScale == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPaySCALEM.PayScale)
                            {
                                foreach (var ca1 in ca.PayScaleArea)
                                {
                                    //  var area = ca.PayScaleArea.Select(a => a.LocationObj).FirstOrDefault();
                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld3 = ca.BasicScaleAppl.ToString(),
                                        Fld5 = ca.ActualIndexAppl.ToString(),
                                        Fld2 = ca.PayScaleType == null ? "" : ca.PayScaleType.LookupVal.ToString(),
                                        Fld7 = ca.Rounding == null ? "" : ca.Rounding.LookupVal.ToString(),
                                        Fld4 = ca.CPIAppl.ToString(),
                                        Fld6 = ca.MultiplyingFactor.ToString(),
                                        Fld8 = ca1.LocationObj == null ? "" : ca1.LocationObj.LocDesc.ToString(),
                                        // Fld6 = ca1.EndDate.ToString()
                                    };
                                    //write data to generic class
                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;




                    case "CORPORATEMASTER":
                        var OGeoCORPM = db.Corporate
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.BusinessType).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().ToList();

                        if (OGeoCORPM == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OGeoCORPM)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Code,
                                    Fld3 = ca.Name,
                                    Fld4 = ca.Address == null ? "" : ca.Address.FullAddress,
                                    Fld5 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                    Fld6 = ca.BusinessType.LookupVal == null ? "" : ca.BusinessType.LookupVal.ToString()

                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "REGIONMASTER":
                        var OGeoREGM = db.Region

                        .Include(e => e.Incharge)
                        .Include(e => e.Incharge.EmpName)
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails).AsNoTracking().AsParallel().ToList();

                        //.Where(d => d.Id == CompanyId).ToList();

                        if (OGeoREGM == null)
                        {
                            return null;
                        }
                        else
                        {
                            List<Region> regiondata = OGeoREGM.ToList();

                            foreach (var ca in regiondata)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {

                                    Fld2 = ca.Code,
                                    Fld3 = ca.Name,
                                    Fld4 = ca.OpeningDate.Value.ToShortDateString(),
                                    Fld5 = ca.Incharge == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                    Fld6 = ca.Address == null ? "" : ca.Address.FullAddress,
                                    Fld7 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }

                        break;



                    case "JOBSTATUSMASTER":
                        var OGeoJobStatusM = db.Company
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.JobStatus)
                        .Include(e => e.JobStatus.Select(r => r.EmpActingStatus))
                         .Include(e => e.JobStatus.Select(r => r.EmpStatus))
                        .Where(d => d.Id == CompanyId).SingleOrDefault();

                        if (OGeoJobStatusM == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OGeoJobStatusM.JobStatus)
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



                    case "LEVELMASTER":
                        var rptlevelM = db.Company
                        .Include(e => e.Level).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

                        if (rptlevelM == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in rptlevelM.Level)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Code,
                                    Fld3 = ca.Name
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "LOOKUP":
                        var OLookup = db.Lookup
                        .Include(e => e.LookupValues).OrderBy(e => e.Code)
                        .AsNoTracking().ToList();

                        if (OLookup == null && OLookup.Count != 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var Ilookcode in OLookup)
                            {
                                var loockupdet = Ilookcode.LookupValues.ToList();
                                foreach (var item in loockupdet)
                                {
                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = Ilookcode.Code.ToString(),
                                        Fld2 = Ilookcode.Name.ToString(),
                                        Fld3 = item.LookupVal.ToString(),
                                        Fld4 = item.IsActive == false ? "No" : "Yes",

                                    };
                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }

                            }

                            return OGenericPayrollStatement;
                        }

                        break;


                    case "COMPANYMASTER":
                        var OGeoCompM = db.Company
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.ContactDetails.ContactNumbers).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

                        if (OGeoCompM == null)
                        {
                            return null;
                        }
                        else
                        {


                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = OGeoCompM.Id.ToString(),
                                Fld2 = OGeoCompM.Code,
                                Fld3 = OGeoCompM.Name == null ? "" : OGeoCompM.Name.ToString(),
                                Fld4 = OGeoCompM.Address == null ? "" : OGeoCompM.Address.FullAddress,
                                Fld5 = OGeoCompM.ContactDetails == null ? "" : OGeoCompM.ContactDetails.FullContactDetails,
                                Fld6 = OGeoCompM.CINNo == null ? "" : OGeoCompM.CINNo.ToString(),
                                Fld7 = OGeoCompM.CSTNo == null ? "" : OGeoCompM.CSTNo.ToString(),
                                Fld8 = OGeoCompM.ESICNo == null ? "" : OGeoCompM.ESICNo.ToString(),
                                Fld9 = OGeoCompM.EstablishmentNo == null ? "" : OGeoCompM.EstablishmentNo.ToString(),
                                Fld10 = OGeoCompM.LBTNO == null ? "" : OGeoCompM.LBTNO.ToString(),
                                Fld11 = OGeoCompM.PANNo == null ? "" : OGeoCompM.PANNo.ToString(),
                                Fld12 = OGeoCompM.PTECNO == null ? "" : OGeoCompM.PTECNO.ToString(),
                                Fld13 = OGeoCompM.PTRCNO == null ? "" : OGeoCompM.PTRCNO.ToString(),
                                Fld14 = OGeoCompM.RegistrationNo == null ? "" : OGeoCompM.RegistrationNo.ToString(),
                                Fld15 = OGeoCompM.RegistrationDate == null ? "" : OGeoCompM.RegistrationDate.ToString(),
                                Fld16 = OGeoCompM.ServiceTaxNo == null ? "" : OGeoCompM.ServiceTaxNo.ToString(),
                                Fld17 = OGeoCompM.TANNo == null ? "" : OGeoCompM.TANNo.ToString(),
                                Fld18 = OGeoCompM.VATNo == null ? "" : OGeoCompM.VATNo.ToString(),
                            };
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);

                            return OGenericPayrollStatement;
                        }

                        break;

                    case "LOCATIONMASTER":
                        var OGeoLocM = db.Company
                        .Include(e => e.Location)
                        .Include(e => e.Location.Select(r => r.Address))
                        .Include(e => e.Location.Select(r => r.ContactDetails.ContactNumbers))
                        .Include(e => e.Location.Select(r => r.BusinessCategory))
                        .Include(e => e.Location.Select(r => r.Incharge))
                        .Include(e => e.Location.Select(r => r.Incharge.EmpName))
                        .Include(e => e.Location.Select(r => r.Incharge.CorContact))
                        .Include(e => e.Location.Select(r => r.LocationObj))
                        .Include(e => e.Location.Select(r => r.Type))
                        .Where(d => d.Id == CompanyId).SingleOrDefault();
                        if (OGeoLocM.Location == null || OGeoLocM.Location.Count == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OGeoLocM.Location)
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
                                    Fld12 = ca.WeeklyOffCalendar == null ? "" : ca.WeeklyOffCalendar.LastOrDefault().Id.ToString(),
                                    Fld13 = ca.HolidayCalendar == null ? "" : ca.HolidayCalendar.LastOrDefault().Id.ToString(),
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);

                            }
                            return OGenericPayrollStatement;
                        }
                        break;




                    case "DIVISIONMASTER":
                        var OGeoDivM = db.Company
                        .Include(e => e.Divisions)
                        .Include(e => e.Divisions.Select(r => r.Address))
                        .Include(e => e.Divisions.Select(r => r.ContactDetails))
                        .Include(e => e.Divisions.Select(r => r.ContactDetails.ContactNumbers))
                        .Include(e => e.Divisions.Select(r => r.Incharge))
                        .Include(e => e.Divisions.Select(r => r.Incharge.EmpName))
                        .Include(e => e.Divisions.Select(r => r.Incharge.CorContact))
                        .Include(e => e.Divisions.Select(r => r.Incharge.CorAddr))
                        .Include(e => e.Divisions.Select(r => r.Type)).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoDivM.Divisions == null || OGeoDivM.Divisions.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OGeoDivM.Divisions)
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




                    case "JOBMASTER":
                        var OGjobM = db.Company
                        .Include(e => e.Job)
                        .Where(d => d.Id == CompanyId).AsNoTracking().SingleOrDefault();
                        if (OGjobM.Job.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OGjobM.Job)
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


                    case "DEPARTMENTMASTER":
                        var OGeoDeptM = db.Company
                        .Include(e => e.Department)
                        .Include(e => e.Department.Select(r => r.Address))
                        .Include(e => e.Department.Select(r => r.ContactDetails))
                        .Include(e => e.Department.Select(r => r.ContactDetails.ContactNumbers))
                        .Include(e => e.Department.Select(r => r.Incharge))
                        .Include(e => e.Department.Select(r => r.Incharge.EmpName))
                        .Include(e => e.Department.Select(r => r.Type))
                        .Include(e => e.Department.Select(r => r.DepartmentObj))
                        .Include(e => e.Department.Select(r => r.Incharge.CorContact))
                        .Include(e => e.Department.Select(r => r.Incharge.CorAddr)).AsNoTracking()

                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoDeptM.Department == null || OGeoDeptM.Department.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OGeoDeptM.Department)
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



                    case "GROUPRMASTER":
                        var OGeoGrpM = db.Company
                        .Include(e => e.Group)
                        .Include(e => e.Group.Select(r => r.ContactDetails))
                        .Include(e => e.Group.Select(r => r.Incharge))
                        .Include(e => e.Group.Select(r => r.Incharge.EmpName))
                        .Include(e => e.Group.Select(r => r.Incharge.CorAddr))
                        .Include(e => e.Group.Select(r => r.Incharge.CorContact))
                        .Include(e => e.Group.Select(r => r.Type)).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoGrpM.Group == null || OGeoGrpM.Group.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OGeoGrpM.Group)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Code,
                                    Fld3 = ca.Name,
                                    Fld4 = ca.OpeningDate.Value.ToShortDateString(),
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



                    case "UNITMASTER":
                        var OGeoUnitM = db.Company
                        .Include(e => e.Unit)
                        .Include(e => e.Unit.Select(r => r.ContactDetails))
                        .Include(e => e.Unit.Select(r => r.Incharge))
                        .Include(e => e.Unit.Select(r => r.Incharge.EmpName))
                        .Include(e => e.Unit.Select(r => r.Incharge.CorAddr))
                        .Include(e => e.Unit.Select(r => r.Incharge.CorContact))
                        .Include(e => e.Unit.Select(r => r.Type)).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoUnitM.Unit == null || OGeoUnitM.Unit.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            List<Unit> unit = OGeoUnitM.Unit.ToList();
                            foreach (var ca in unit)
                            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Code,
                                    Fld3 = ca.Name,
                                    Fld4 = ca.Type == null ? "" : ca.Type.LookupVal.ToUpper(),
                                    Fld5 = ca.ContactDetails == null ? "" : ca.ContactDetails.FullContactDetails,
                                    Fld6 = ca.Incharge == null ? "" : ca.Incharge.EmpCode,
                                    Fld7 = ca.Incharge == null ? "" : ca.Incharge.EmpName == null ? "" : ca.Incharge.EmpName.FullNameFML,
                                    Fld8 = ca.Incharge == null ? "" : ca.Incharge.CorContact == null ? "" : ca.Incharge.CorContact.FullContactDetails,
                                    Fld9 = ca.Incharge == null ? "" : ca.Incharge.CorAddr == null ? "" : ca.Incharge.CorAddr.FullAddress,
                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            }

                            return OGenericPayrollStatement;

                        }
                        break;



                    case "JOBPOSITIONMASTER":
                        var OFunJobPositionM = db.Company
                        .Include(e => e.JobPosition)

                        .Where(d => d.Id == CompanyId).SingleOrDefault();
                        if (OFunJobPositionM.JobPosition == null || OFunJobPositionM.JobPosition.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OFunJobPositionM.JobPosition)
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



                    case "BONUSACTMASTER":
                        var OSalBonusActM = db.CompanyPayroll
                    .Include(e => e.BonusAct)
                    .Include(e => e.BonusAct.Select(r => r.BonusCalendar))
                    .Include(e => e.BonusAct.Select(r => r.BonusWages))
                    .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster))
                    .Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead))).AsNoTracking()
                            //.Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead.Code)))
                            //.Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead.Name)))
                .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalBonusActM.BonusAct == null || OSalBonusActM.BonusAct.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalBonusActM.BonusAct)
                            {
                                foreach (var ca1 in ca.BonusWages.RateMaster)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.BonusWages.Id.ToString(),
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



                    case "CPIRULEMASTER":
                        var OSalCPIRuleM = db.CompanyPayroll
                            .Include(e => e.CPIRule)

                            .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails))
                            .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages)))
                            .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster)))
                            .Include(e => e.CPIRule.Select(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster.Select(s => s.SalHead))))

                            .Include(e => e.CPIRule.Select(r => r.CPIUnitCalc))
                            .Include(e => e.CPIRule.Select(r => r.RoundingMethod)).AsNoTracking()//modify roundingmethod in other code

                        .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalCPIRuleM.CPIRule == null || (OSalCPIRuleM.CPIRule.Count() == 0))
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OSalCPIRuleM.CPIRule)
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
                        var OSalESICMasterData = db.ESICMaster

                            .Include(r => r.ESICStatutoryEffectiveMonths)
                            .Include(r => r.ESICStatutoryEffectiveMonths.Select(t => t.EffectiveMonth))
                            .Include(r => r.ESICStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange))
                            .Include(r => r.Location)
                            .Include(r => r.Location.Select(t => t.LocationObj))
                            .Include(r => r.WageMasterPay)
                            .Include(r => r.WageMasterPay.RateMaster)
                            .Include(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead))
                            //.Includt(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead.Code))
                            //.Includt(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead.Name))
                            .Include(r => r.WageMasterQualify)
                            .Include(r => r.WageMasterQualify.RateMaster)
                            .Include(r => r.WageMasterQualify.RateMaster.Select(w => w.SalHead)).AsNoTracking().ToList();
                        //.Include(e => e.ESICMaster.Select(r => r.WageMasterQualify.RateMaster.Select(w => w.SalHead.Code)))
                        //.Include(e => e.ESICMaster.Select(r => r.WageMasterQualify.RateMaster.Select(w => w.SalHead.Name)))
                        //.Where(d => d.Id == CompanyPayrollId && d.ESICMaster.Count != 0).AsParallel().SingleOrDefault();

                        if (OSalESICMasterData == null)
                        {
                            return null;
                        }

                        if (OSalESICMasterData == null || OSalESICMasterData.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OSalESICMasterData)
                            {
                                var locname = ca.Location.Select(e => e.LocationObj.LocDesc).SingleOrDefault();
                                foreach (var ca1 in ca.ESICStatutoryEffectiveMonths)
                                {
                                    foreach (var item in ca1.StatutoryWageRange)
                                    {


                                        //    var compshare = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.CompSharePercentage).ToString();
                                        GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = ca.Id.ToString(),
                                            //  Fld2 = ca.LocationObj.Select(e => e.LocDesc).ToString(),
                                            Fld2 = locname.ToString(),
                                            Fld3 = ca.EffectiveDate.Value.ToShortDateString(),
                                            Fld4 = ca.EndDate == null ? "" : ca.EndDate.Value.ToShortDateString(),
                                            Fld5 = ca.WageMasterPay == null ? "" : ca.WageMasterPay.FullDetails,
                                            Fld6 = ca.WageMasterQualify == null ? "" : ca.WageMasterQualify.FullDetails,
                                            //Fld7 = ca1.Id.ToString(),
                                            //Fld8 = ca1.EffectiveMonth == null ? "" : ca1.EffectiveMonth.LookupVal.ToUpper(),
                                            //Fld9 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.CompSharePercentage).ToString(),
                                            //Fld10 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.CompShareAmount).ToString(),
                                            //Fld11 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.EmpSharePercentage).ToString(),
                                            //Fld12 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.EmpShareAmount).ToString(),
                                            //Fld13 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.RangeFrom).ToString(),
                                            //Fld14 = ca1.StatutoryWageRange == null ? "" : ca1.StatutoryWageRange.Select(e => e.RangeTo).ToString(),
                                            Fld7 = ca1.Id.ToString(),
                                            Fld8 = ca1.EffectiveMonth == null ? "" : ca1.EffectiveMonth.LookupVal.ToString(),
                                            Fld9 = item.CompSharePercentage == null ? "" : item.CompSharePercentage.ToString(),
                                            Fld10 = item.CompShareAmount == null ? "" : item.CompShareAmount.ToString(),
                                            Fld11 = item.EmpSharePercentage == null ? "" : item.EmpSharePercentage.ToString(),
                                            Fld12 = item.EmpShareAmount == null ? "" : item.EmpShareAmount.ToString(),
                                            Fld13 = item.RangeFrom == null ? "" : item.RangeFrom.ToString(),
                                            Fld14 = item.RangeTo == null ? "" : item.RangeTo.ToString(),
                                        };

                                        OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }

                        break;
                    case "GRATUITYACTMASTER":
                        var OSalGratuityActM = db.CompanyPayroll

                            .Include(e => e.GratuityAct)
                            .Include(e => e.GratuityAct.Select(r => r.GratuityWages))
                            .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster))
                            .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster.Select(t => t.SalHead)))
                            .Include(e => e.GratuityAct.Select(r => r.GratuityWages.RateMaster.Select(t => t.Wages)))
                            .Include(e => e.GratuityAct.Select(r => r.LvHead))
                             .Where(d => d.Id == CompanyPayrollId && d.GratuityAct.Count != 0).SingleOrDefault();

                        if (OSalGratuityActM.GratuityAct == null || OSalGratuityActM.GratuityAct.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalGratuityActM.GratuityAct)
                            {
                                foreach (var ca1 in ca.GratuityWages.RateMaster)
                                {
                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.GratuityWages.Id.ToString(),
                                        Fld2 = ca.GratuityActName == null ? "" : ca.GratuityActName,
                                        Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.Value.ToShortDateString(),
                                        Fld4 = ca.EndDate == null ? "" : ca.EndDate.Value.ToShortDateString(),
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
                                        //Fld16 = ca.IsDateOfConfirm == null ? "" : ca.IsDateOfConfirm.ToString(),

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



                    case "ITSECTIONMASTER":
                        var OITSectionM = db.CompanyPayroll
                            .Include(e => e.IncomeTax)
                            .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionList)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionListType))).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId && d.IncomeTax.Count != 0).AsParallel().SingleOrDefault();
                        if (OITSectionM == null)
                        {
                            return null;
                        }
                        else
                        {
                            if (OITSectionM.IncomeTax == null || OITSectionM.IncomeTax.Count() == 0)
                                return null;
                            else
                            {

                                foreach (var ca in OITSectionM.IncomeTax)
                                {
                                    foreach (var ca1 in ca.ITSection)
                                    {
                                        GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca.Id.ToString(),
                                            Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name == null ? "" : ca.FyCalendar.Name.LookupVal,
                                            Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString().ToString(),
                                            Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString().ToString(),
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
                            }
                            return OGenericPayrollStatement;
                        }
                        break;



                    case "ITTDSMASTER":
                        var OITTdsM = db.CompanyPayroll
                            .Include(e => e.IncomeTax)
                            .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                            .Include(e => e.IncomeTax.Select(r => r.ITTDS))
                            .Include(e => e.IncomeTax.Select(r => r.ITTDS.Select(t => t.Category))).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId && d.IncomeTax.Count != 0).AsParallel().SingleOrDefault();
                        if (OITTdsM == null)
                        {
                            return null;
                        }
                        else
                        {
                            if (OITTdsM.IncomeTax == null || OITTdsM.IncomeTax.Count() == 0)
                                return null;
                            else
                            {
                                foreach (var ca in OITTdsM.IncomeTax)
                                {
                                    foreach (var ca1 in ca.ITTDS)
                                    {

                                        GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca.Id.ToString(),
                                            Fld2 = ca.FyCalendar == null ? "" : ca.FyCalendar.Name == null ? "" : ca.FyCalendar.Name.LookupVal,
                                            Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString(),
                                            Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString(),
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
                            }
                            return OGenericPayrollStatement;
                        }
                        break;



                    case "ITSECTION10BMASTER":
                        var OITSection10BM = db.CompanyPayroll
                            .Include(e => e.IncomeTax)
                            .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionList)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionListType)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead))))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead.Select(x => x.SalHead)))))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead.Select(x => x.Frequency))))).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();


                        if (OITSection10BM.IncomeTax == null || OITSection10BM.IncomeTax.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OITSection10BM.IncomeTax)
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
                                                    Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString(),
                                                    Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString(),
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


                    case "ITINVESTMENTMASTER":
                        var OITInvestmentM = db.CompanyPayroll
                            .Include(e => e.IncomeTax)
                            .Include(e => e.IncomeTax.Select(r => r.FyCalendar))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionList)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITSectionListType)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITInvestments)))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITInvestments.Select(y => y.ITSubInvestment))))
                            .Include(e => e.IncomeTax.Select(r => r.ITSection.Select(t => t.ITInvestments.Select(y => y.SalaryHead))))
                        .Where(d => d.Id == CompanyPayrollId).AsNoTracking().SingleOrDefault();


                        if (OITInvestmentM.IncomeTax == null || OITInvestmentM.IncomeTax.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OITInvestmentM.IncomeTax)
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
                                                    Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString(),
                                                    Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString() == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString(),
                                                    Fld5 = ca1.ITSectionList == null ? "" : ca1.ITSectionList.LookupVal == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                                    Fld6 = ca1.ITSectionListType == null ? "" : ca1.ITSectionListType.LookupVal == null ? "" : ca1.ITSectionListType.LookupVal.ToUpper(),
                                                    Fld7 = ca1.Id.ToString(),
                                                    Fld8 = ca2.FullDetails == null ? "" : ca2.FullDetails,
                                                    Fld9 = ca2.Id.ToString(),

                                                    Fld10 = ca2.MaxAmount == null ? "" : ca2.MaxAmount.ToString(),
                                                    Fld11 = ca2.MaxPercentage == null ? "" : ca2.MaxPercentage.ToString(),
                                                    Fld12 = ca2.IsSalaryHead == null ? "" : ca2.IsSalaryHead.ToString(),

                                                    Fld13 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Id.ToString(),
                                                    Fld14 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Code == null ? "" : ca2.SalaryHead.Code,
                                                    Fld15 = ca2.SalaryHead == null ? "" : ca2.SalaryHead.Name == null ? "" : ca2.SalaryHead.Name,





                                                    Fld16 = ca2.ITInvestmentName == null ? "" : ca2.ITInvestmentName,
                                                    //Fld17 = "",
                                                    //Fld18 = "",
                                                    //Fld19 = ""

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
                                                        Fld3 = ca.FyCalendar == null ? "" : ca.FyCalendar.FromDate == null ? "" : ca.FyCalendar.FromDate.Value.ToShortDateString(),
                                                        Fld4 = ca.FyCalendar == null ? "" : ca.FyCalendar.ToDate == null ? "" : ca.FyCalendar.ToDate.Value.ToShortDateString(),
                                                        Fld5 = ca1.ITSectionList == null ? "" : ca1.ITSectionList.LookupVal == null ? "" : ca1.ITSectionList.LookupVal.ToUpper(),
                                                        Fld6 = ca1.ITSectionListType == null ? "" : ca1.ITSectionListType.LookupVal == null ? "" : ca1.ITSectionListType.LookupVal.ToUpper(),
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

                    case "MENUAUTHORITYRIGHTS":
                        var MenuAuthoritydata = new List<Employee>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OBonusChkData_t = db.Employee
                                 .Include(a => a.Login)
                               .Include(e => e.EmpName)
                                .Include(e => e.GeoStruct)
                               .Include(e => e.FuncStruct)
                               .Include(e => e.PayStruct)
                               .Include(a => a.ServiceBookDates)
                                 .Where(e => e.Id == item && e.Login != null && e.ServiceBookDates != null && e.ServiceBookDates.ServiceLastDate == null)
                                 .FirstOrDefault();
                            if (OBonusChkData_t != null)
                            {
                                MenuAuthoritydata.Add(OBonusChkData_t);
                            }
                        }


                        if (MenuAuthoritydata == null || MenuAuthoritydata.Count() == 0)
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
                            var path1 = HttpContext.Current.Server.MapPath("~/App_Data/Menu_Json/Default.json");
                            string json1 = System.IO.File.ReadAllText(path1);
                            //  JArray jsonfileData = JArray.Parse(json1);
                            //  string serialized = JsonConvert.SerializeObject(json1);
                            var serialize = new JavaScriptSerializer();
                            // string serialized = JsonConvert.SerializeObject(json1);
                            var obj = serialize.Deserialize<List<ArtistElement>>(json1);

                            foreach (var ca in MenuAuthoritydata)
                            {
                                //-----only for admin start
                                if (ca.Login.UserId == "admin")
                                {
                                    int geoid = ca.GeoStruct.Id;

                                    int payid = ca.PayStruct.Id;

                                    int funid = ca.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);


                                    var firstlevel = obj.Where(e => e.Text == "Menu").SingleOrDefault();

                                    foreach (var FirstMenu in firstlevel.Children)
                                    {

                                        //string firstmenu = FirstMenu.Text;

                                        //OGenericObjStatement.Fld10 = firstmenu.ToString();
                                        //OGenericPayrollStatement.Add(OGenericObjStatement);





                                        for (int i = 0; i < FirstMenu.Children.Count(); i++)
                                        {
                                            if (FirstMenu.Children[i].Children.Count() == 0)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                {

                                                    Fld2 = ca.EmpCode == null ? "" : ca.EmpCode.ToString(),
                                                    Fld3 = ca.EmpName.FullNameFML == null ? "" : ca.EmpName.FullNameFML.ToString(),



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
                                                    OGenericObjStatement.Fld88 = ca.ToString();
                                                }

                                                string secondmenu = FirstMenu.Children[i].Text;
                                                OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                OGenericObjStatement.Fld11 = secondmenu.ToString();
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }

                                            for (int j = 0; j < FirstMenu.Children[i].Children.Count(); j++)
                                            {
                                                if (FirstMenu.Children[i].Children[j].Children.Count() == 0)
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {

                                                        Fld2 = ca.EmpCode == null ? "" : ca.EmpCode.ToString(),
                                                        Fld3 = ca.EmpName.FullNameFML == null ? "" : ca.EmpName.FullNameFML.ToString(),



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
                                                        OGenericObjStatement.Fld88 = ca.ToString();
                                                    }
                                                    string thirdmenu = FirstMenu.Children[i].Children[j].Text;
                                                    OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                    OGenericObjStatement.Fld11 = FirstMenu.Children[i].Text;
                                                    OGenericObjStatement.Fld12 = thirdmenu.ToString();
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }

                                                for (int k = 0; k < FirstMenu.Children[i].Children[j].Children.Count(); k++)
                                                {
                                                    if (FirstMenu.Children[i].Children[j].Children[k].Children.Count() == 0)
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        {

                                                            Fld2 = ca.EmpCode == null ? "" : ca.EmpCode.ToString(),
                                                            Fld3 = ca.EmpName.FullNameFML == null ? "" : ca.EmpName.FullNameFML.ToString(),



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
                                                            OGenericObjStatement.Fld88 = ca.ToString();
                                                        }
                                                        string Fourthmenu = FirstMenu.Children[i].Children[j].Children[k].Text;
                                                        OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                        OGenericObjStatement.Fld11 = FirstMenu.Children[i].Text;
                                                        OGenericObjStatement.Fld12 = FirstMenu.Children[i].Children[j].Text;
                                                        OGenericObjStatement.Fld13 = Fourthmenu.ToString();
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    }


                                                    for (int l = 0; l < FirstMenu.Children[i].Children[j].Children[k].Children.Count(); l++)
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        {

                                                            Fld2 = ca.EmpCode == null ? "" : ca.EmpCode.ToString(),
                                                            Fld3 = ca.EmpName.FullNameFML == null ? "" : ca.EmpName.FullNameFML.ToString(),



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
                                                            OGenericObjStatement.Fld88 = ca.ToString();
                                                        }
                                                        string Fifthmenu = FirstMenu.Children[i].Children[j].Children[k].Children[l].Text;
                                                        OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                        OGenericObjStatement.Fld11 = FirstMenu.Children[i].Text;
                                                        OGenericObjStatement.Fld12 = FirstMenu.Children[i].Children[j].Text;
                                                        OGenericObjStatement.Fld13 = FirstMenu.Children[i].Children[j].Children[k].Text;
                                                        OGenericObjStatement.Fld14 = Fifthmenu.ToString();
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);


                                                    }
                                                }
                                            }

                                        }

                                    }


                                }
                                //-----only for admin end
                                var EmpJsonPath = HttpContext.Current.Server.MapPath("~/App_Data/Menu_Json/" + ca.EmpCode + ".json");
                                if (System.IO.File.Exists(EmpJsonPath))
                                {
                                    int geoid = ca.GeoStruct.Id;

                                    int payid = ca.PayStruct.Id;

                                    int funid = ca.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                    var ReadEmpJson = System.IO.File.ReadAllText(EmpJsonPath);

                                    var OldFiledata = JObject.Parse(ReadEmpJson);
                                    var DeleteOldUrlAuthoritydata = OldFiledata["urlauthority"];
                                    var OldUrlAuthoritydata = OldFiledata["urlauthority"].ToList();



                                    foreach (JProperty item in OldUrlAuthoritydata)
                                    {
                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {

                                            Fld2 = ca.EmpCode == null ? "" : ca.EmpCode.ToString(),
                                            Fld3 = ca.EmpName.FullNameFML == null ? "" : ca.EmpName.FullNameFML.ToString(),



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
                                            OGenericObjStatement.Fld88 = ca.ToString();
                                        }

                                        var empwiseurl = item.Name.ToUpper();
                                        bool exit = false;
                                        var firstlevel = obj.Where(e => e.Text == "Menu").SingleOrDefault();

                                        foreach (var FirstMenu in firstlevel.Children)
                                        {
                                            if (!exit && empwiseurl != null && FirstMenu.Data.Url.ToUpper() != "#" && empwiseurl == FirstMenu.Data.Url.ToUpper())
                                            {
                                                string firstmenu = FirstMenu.Text;

                                                OGenericObjStatement.Fld10 = firstmenu.ToString();
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                exit = true;
                                                break;
                                            }



                                            for (int i = 0; i < FirstMenu.Children.Count(); i++)
                                            {
                                                if (!exit && empwiseurl != null && FirstMenu.Children[i].Data.Url.ToUpper() != "#" && empwiseurl == FirstMenu.Children[i].Data.Url.ToUpper())
                                                {
                                                    string secondmenu = FirstMenu.Children[i].Text;
                                                    OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                    OGenericObjStatement.Fld11 = secondmenu.ToString();
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                    exit = true;
                                                    break;
                                                }
                                                for (int j = 0; j < FirstMenu.Children[i].Children.Count(); j++)
                                                {
                                                    if (!exit && empwiseurl != null && FirstMenu.Children[i].Children[j].Data.Url.ToUpper() != "#" && empwiseurl == FirstMenu.Children[i].Children[j].Data.Url.ToUpper())
                                                    {
                                                        string thirdmenu = FirstMenu.Children[i].Children[j].Text;
                                                        OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                        OGenericObjStatement.Fld11 = FirstMenu.Children[i].Text;
                                                        OGenericObjStatement.Fld12 = thirdmenu.ToString();
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
                                                        exit = true;
                                                        break;
                                                    }
                                                    for (int k = 0; k < FirstMenu.Children[i].Children[j].Children.Count(); k++)
                                                    {
                                                        if (!exit && empwiseurl != null && FirstMenu.Children[i].Children[j].Children[k].Data.Url.ToUpper() != "#" && empwiseurl == FirstMenu.Children[i].Children[j].Children[k].Data.Url.ToUpper())
                                                        {
                                                            string Fourthmenu = FirstMenu.Children[i].Children[j].Children[k].Text;
                                                            OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                            OGenericObjStatement.Fld11 = FirstMenu.Children[i].Text;
                                                            OGenericObjStatement.Fld12 = FirstMenu.Children[i].Children[j].Text;
                                                            OGenericObjStatement.Fld13 = Fourthmenu.ToString();
                                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                                            exit = true;
                                                            break;
                                                        }

                                                        for (int l = 0; l < FirstMenu.Children[i].Children[j].Children[k].Children.Count(); l++)
                                                        {
                                                            if (!exit && empwiseurl != null && FirstMenu.Children[i].Children[j].Children[k].Children[l].Data.Url.ToUpper() != "#" && empwiseurl == FirstMenu.Children[i].Children[j].Children[k].Children[l].Data.Url.ToUpper())
                                                            {
                                                                string Fifthmenu = FirstMenu.Children[i].Children[j].Children[k].Children[l].Text;
                                                                OGenericObjStatement.Fld10 = FirstMenu.Text;
                                                                OGenericObjStatement.Fld11 = FirstMenu.Children[i].Text;
                                                                OGenericObjStatement.Fld12 = FirstMenu.Children[i].Children[j].Text;
                                                                OGenericObjStatement.Fld13 = FirstMenu.Children[i].Children[j].Children[k].Text;
                                                                OGenericObjStatement.Fld14 = Fifthmenu.ToString();
                                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                                                exit = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                            }

                                        }
                                    }


                                    // }
                                    //}
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;




                    case "LOANADVANCEDHEADMASTER":
                        var OSalLoanAdvanceHeadM = db.CompanyPayroll
                            .Include(e => e.LoanAdvanceHead)
                            .Include(e => e.LoanAdvanceHead.Select(r => r.ITLoan))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.InterestType)))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages)))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages.RateMaster)))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages.RateMaster.Select(w => w.SalHead))))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.SalaryHead))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.ITSection.Select(w => w.ITSectionList)))
                            .Include(e => e.LoanAdvanceHead.Select(r => r.ITSection.Select(w => w.ITSectionListType))).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId).AsParallel().ToList();

                        if (OSalLoanAdvanceHeadM.Count == null && OSalLoanAdvanceHeadM.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var item in OSalLoanAdvanceHeadM)
                            {


                                foreach (var ca in item.LoanAdvanceHead)
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
                                            Fld8 = ca1.EffectiveDate.Value.ToShortDateString(),
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
                             .Include(e => e.LWFMaster.Select(r => r.WagesMaster.RateMaster.Select(t => t.SalHead))).AsNoTracking()
                            //  .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster.Select(t => t.SalHead)))
                       .Where(d => d.Id == CompanyPayrollId && d.LWFMaster.Count != 0).AsParallel().SingleOrDefault();
                        if (OSalLWFMasterData == null)
                            return null;
                        else
                        {
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
                                                Fld2 = ca.EffectiveDate.Value.ToShortDateString(),
                                                Fld3 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                                Fld4 = ca.LWFStates == null ? "" : ca.LWFStates.Name,
                                                Fld5 = ca.WagesMaster == null ? "" : ca.WagesMaster.WagesName,
                                                Fld6 = mWageRate,
                                                Fld7 = ca.WagesMaster.CeilingMax == null ? "" : ca.WagesMaster.CeilingMax.ToString(),
                                                Fld8 = ca.WagesMaster.CeilingMin == null ? "" : ca.WagesMaster.CeilingMin.ToString(),
                                                Fld9 = ca.WagesMaster.Percentage == null ? "" : ca.WagesMaster.Percentage.ToString(),



                                                Fld10 = ca1.Id.ToString(),
                                                Fld11 = ca1.EffectiveMonth == null ? "" : ca1.EffectiveMonth.LookupVal.ToUpper(),
                                                //Fld12 = ca2.Id.ToString(),
                                                //Fld13 = ca2.RangeFrom == null ? "" : ca2.RangeFrom.ToString(),
                                                //Fld14 = ca2.RangeTo == null ? "" : ca2.RangeTo.ToString(),
                                                //Fld15 = ca2.CompShareAmount == null ? "" : ca2.CompShareAmount.ToString(),
                                                //Fld16 = ca2.CompSharePercentage == null ? "" : ca2.CompSharePercentage.ToString(),
                                                //Fld17 = ca2.EmpShareAmount == null ? "" : ca2.EmpShareAmount.ToString(),
                                                //Fld18 = ca2.EmpSharePercentage == null ? "" : ca2.EmpSharePercentage.ToString(),


                                            };
                                            OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);

                                        }
                                    }
                                }
                            }
                            return OGenericPayrollStatement;
                        }
                        break;



                    case "NEGSALACTMASTER":
                        var OSalNegSalActM = db.CompanyPayroll
                            .Include(e => e.NegSalAct).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalNegSalActM.NegSalAct == null || OSalNegSalActM.NegSalAct.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalNegSalActM.NegSalAct)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.NegSalActname == null ? "" : ca.NegSalActname,
                                    Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.Value.ToShortDateString(),
                                    Fld4 = ca.EndDate == null ? "" : ca.EndDate.ToString(),
                                    Fld5 = ca.MinAmount == null ? "" : ca.MinAmount.ToString(),
                                    Fld6 = ca.SalPercentage == null ? "" : ca.SalPercentage.ToString(),

                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }
                        break;




                    case "RETIREMENTDAYACTMASTER":
                        var ORetireActM = db.RetirementDay.AsNoTracking()
                        .Where(d => d.Id == CompanyPayrollId).AsParallel().ToList();

                        if (ORetireActM == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in ORetireActM)
                            {
                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                //write data to generic class
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.ServiceEndMonRegDay == null ? "" : ca.ServiceEndMonRegDay.ToString(),
                                    Fld3 = ca.ServiceEndMonLastDay == null ? "" : ca.ServiceEndMonLastDay.ToString(),
                                    Fld4 = ca.Year == null ? "" : ca.Year.ToString(),


                                };
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                            }
                            return OGenericPayrollStatement;
                        }
                        break;



                    case "PAYSCALEASSIGNMENTMASTER":
                        var OSalPayScaleAssignmentMaster = db.CompanyPayroll
                            .Include(e => e.PayScaleAssignment)
                            .Include(e => e.PayScaleAssignment.Select(r => r.PayScaleAgreement))
                            .Include(e => e.PayScaleAssignment.Select(r => r.SalaryHead))
                            .Include(e => e.PayScaleAssignment.Select(r => r.SalHeadFormula))
                            .Include(e => e.PayScaleAssignment.Select(r => r.SalHeadFormula.Select(t => t.SalWages)))
                        .Where(d => d.Id == CompanyPayrollId).AsNoTracking().SingleOrDefault();

                        if (OSalPayScaleAssignmentMaster.PayScaleAssignment == null || OSalPayScaleAssignmentMaster.PayScaleAssignment.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalPayScaleAssignmentMaster.PayScaleAssignment)
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
                            .Include(e => e.PFMaster.Select(r => r.PFInspWages.RateMaster.Select(t => t.SalHead))).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId && d.PFMaster.Count != 0).AsParallel().SingleOrDefault();

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
                                    Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.Value.ToShortDateString(),
                                    Fld4 = ca.EndDate == null ? "" : ca.EndDate.Value.ToShortDateString(),
                                    Fld5 = ca.PFTrustType == null ? "" : ca.PFTrustType.LookupVal.ToUpper().ToString(),
                                    Fld6 = ca.PensionTrustType == null ? "" : ca.PensionTrustType.ToString(),
                                    Fld7 = ca.EDLISTrustType == null ? "" : ca.EDLISTrustType.ToString(),
                                    Fld8 = ca.CompPFNo == null ? "" : ca.CompPFNo.ToString(),
                                    Fld9 = ca.CPFPerc == null ? "" : ca.CPFPerc.ToString(),
                                    Fld10 = ca.CompPFCeiling == null ? "" : ca.CompPFCeiling.ToString(),
                                    Fld11 = ca.RegDate == null ? "" : ca.RegDate.Value.ToShortDateString(),
                                    Fld12 = ca.EPFWages == null ? "" : ca.EPFWages.FullDetails.ToString(),
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
                             .Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(t => t.EffectiveMonth)))
                             .Include(e => e.PTaxMaster.Select(r => r.PTStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange)))
                             .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster))
                             .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster))
                              .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster.Select(t => t.SalHead)))
                             .Include(e => e.PTaxMaster.Select(r => r.States)).AsNoTracking()
                        .Where(d => d.Id == CompanyPayrollId && d.PTaxMaster.Count != 0).AsParallel().SingleOrDefault();

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
                                            Fld2 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.Value.ToShortDateString(),
                                            Fld3 = ca.EndDate == null ? "" : ca.EndDate.Value.ToShortDateString(),
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



                    case "SALARYHEADMASTER":
                        var OSalSalaryHeadM = db.CompanyPayroll
                            .Include(e => e.SalaryHead)
                            .Include(e => e.SalaryHead.Select(r => r.Frequency))
                            .Include(e => e.SalaryHead.Select(r => r.ProcessType))
                            .Include(e => e.SalaryHead.Select(r => r.RoundingMethod))
                            .Include(e => e.SalaryHead.Select(r => r.SalHeadOperationType))
                            .Include(e => e.SalaryHead.Select(r => r.Type)).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalSalaryHeadM.SalaryHead == null || OSalSalaryHeadM.SalaryHead.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalSalaryHeadM.SalaryHead)
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


                    case "SUSPENSIONSALPOLICYMASTER":
                        var OSalSuspensionSalPolicyM = db.CompanyPayroll
                            .Include(e => e.SuspensionSalPolicy)
                            .Include(e => e.SuspensionSalPolicy.Select(r => r.DayRange))
                            .Include(e => e.SuspensionSalPolicy.Select(r => r.SuspensionWages))
                            .Include(e => e.SuspensionSalPolicy.Select(r => r.SuspensionWages.RateMaster))
                        .Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                        if (OSalSuspensionSalPolicyM.SuspensionSalPolicy == null || OSalSuspensionSalPolicyM.SuspensionSalPolicy.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalSuspensionSalPolicyM.SuspensionSalPolicy)
                            {
                                foreach (var ca1 in ca.DayRange)
                                {

                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.PolicyName == null ? "" : ca.PolicyName.ToString(),
                                        Fld3 = ca.EffectiveDate == null ? "" : ca.EffectiveDate.Value.ToShortDateString(),
                                        Fld4 = ca.EndDate == null ? "" : ca.EndDate.Value.ToShortDateString(),
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



                    case "INCREMENTACTIVITYMASTER":


                        var OIncrementServiceData = db.PayScaleAgreement
                            .Include(t => t.IncrActivity)


                         .Include(t => t.IncrActivity.Select(e => e.IncrPolicy))
                         .Include(t => t.IncrActivity.Select(e => e.IncrList))

                          .Include(t => t.IncrActivity.Select(e => e.IncrPolicy.RegIncrPolicy))
                          .Include(t => t.IncrActivity.Select(e => e.IncrPolicy.NonRegIncrPolicy))
                          .Include(t => t.IncrActivity.Select(e => e.IncrPolicy.IncrPolicyDetails))
                         .Include(t => t.IncrActivity.Select(e => e.StagIncrPolicy))

                        .Where(d => d.Id == CompanyId).AsNoTracking().ToList();

                        if (OIncrementServiceData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OIncrementServiceData)
                            {
                                var OITTrans = ca.IncrActivity.ToList();
                                //if (OITTrans != null && OITTrans.Count() != 0)
                                //{
                                foreach (var ca1 in OITTrans)
                                {


                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.EffDate == null ? "" : ca.EffDate.Value.ToShortDateString(),

                                        Fld2 = ca1.Name == null ? "" : ca1.Name,
                                        Fld3 = ca1.IncrList == null ? "" : ca1.IncrList.LookupVal.ToUpper(),

                                        Fld4 = ca1.StagIncrPolicy == null ? "" : ca1.StagIncrPolicy.Name.ToString(),
                                        Fld5 = ca1.StagIncrPolicy == null ? "" : ca1.StagIncrPolicy.SpanYears.ToString(),
                                        Fld6 = ca1.StagIncrPolicy == null ? "" : ca1.StagIncrPolicy.MaxStagIncr.ToString(),
                                        Fld7 = ca1.StagIncrPolicy == null ? "" : ca1.StagIncrPolicy.IsLastIncr.ToString(),
                                        Fld8 = ca1.StagIncrPolicy == null ? "" : ca1.StagIncrPolicy.IsFixAmount.ToString(),
                                        Fld9 = ca1.StagIncrPolicy == null ? "" : ca1.StagIncrPolicy.IncrAmount.ToString(),

                                        Fld10 = ca1.IncrPolicy.Name == null ? "" : ca1.IncrPolicy.Name.ToString(),
                                        Fld11 = ca1.IncrPolicy.IsRegularIncr == null ? "" : ca1.IncrPolicy.IsRegularIncr.ToString(),

                                        Fld12 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsJoiningDate == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsJoiningDate.ToString(),
                                        Fld13 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsConfirmDate == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsConfirmDate.ToString(),
                                        Fld14 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsFixMonth == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsFixMonth.ToString(),
                                        Fld15 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IncrMonth == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IncrMonth.ToString(),
                                        Fld16 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsMidMonthEffect == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsMidMonthEffect.ToString(),
                                        Fld17 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.MidMonthLockDay == null ? "" : ca1.IncrPolicy.RegIncrPolicy.MidMonthLockDay.ToString(),
                                        Fld18 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.CurMonStartDay == null ? "" : ca1.IncrPolicy.RegIncrPolicy.CurMonStartDay.ToString(),
                                        Fld19 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.NextMonStartDay == null ? "" : ca1.IncrPolicy.RegIncrPolicy.NextMonStartDay.ToString(),
                                        Fld20 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsMidQuarterEffect == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsMidQuarterEffect.ToString(),
                                        Fld21 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.CurrQuarterStart == null ? "" : ca1.IncrPolicy.RegIncrPolicy.CurrQuarterStart.ToString(),
                                        Fld22 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsLWPEffectDateAsIncrDate == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsLWPEffectDateAsIncrDate.ToString(),
                                        Fld23 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsLWPIncl == null ? "" : ca1.IncrPolicy.RegIncrPolicy.IsLWPIncl.ToString(),
                                        Fld24 = ca1.IncrPolicy.RegIncrPolicy == null ? "" : ca1.IncrPolicy.RegIncrPolicy.LWPMinCeiling == null ? "" : ca1.IncrPolicy.RegIncrPolicy.LWPMinCeiling.ToString(),

                                        Fld25 = ca1.IncrPolicy.NonRegIncrPolicy == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MaxIncrInService == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MaxIncrInService.ToString(),
                                        Fld26 = ca1.IncrPolicy.NonRegIncrPolicy == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MinSerAppl == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MinSerAppl.ToString(),
                                        Fld27 = ca1.IncrPolicy.NonRegIncrPolicy == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MinService == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MinService.ToString(),
                                        Fld28 = ca1.IncrPolicy.NonRegIncrPolicy == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MaxSerLockAppl == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MaxSerLockAppl.ToString(),
                                        Fld29 = ca1.IncrPolicy.NonRegIncrPolicy == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MaxService == null ? "" : ca1.IncrPolicy.NonRegIncrPolicy.MaxService.ToString(),

                                        Fld30 = ca1.IncrPolicy.IncrPolicyDetails == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IsIncrAmount == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IsIncrAmount.ToString(),
                                        Fld31 = ca1.IncrPolicy.IncrPolicyDetails == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IncrAmount == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IncrAmount.ToString(),
                                        Fld32 = ca1.IncrPolicy.IncrPolicyDetails == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IsIncrPercent == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IsIncrPercent.ToString(),
                                        Fld33 = ca1.IncrPolicy.IncrPolicyDetails == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IncrPercent == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IncrPercent.ToString(),
                                        Fld34 = ca1.IncrPolicy.IncrPolicyDetails == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IsIncrSteps == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IsIncrSteps.ToString(),
                                        Fld35 = ca1.IncrPolicy.IncrPolicyDetails == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IncrSteps == null ? "" : ca1.IncrPolicy.IncrPolicyDetails.IncrSteps.ToString(),




                                    };

                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }

                        break;




                    case "PROMOTIONACTIVITYMASTER":


                        var OPromotionData = db.PayScaleAgreement
                           .Include(t => t.PromoActivity)
                         .Include(t => t.PromoActivity.Select(e => e.PromoPolicy))
                         .Include(t => t.PromoActivity.Select(e => e.PromoList))
                        .Where(d => d.Id == CompanyId).AsNoTracking().ToList();

                        if (OPromotionData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPromotionData)
                            {
                                var OITTrans = ca.PromoActivity.ToList();
                                //if (OITTrans != null && OITTrans.Count() != 0)
                                //{
                                foreach (var ca1 in OITTrans)
                                {


                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.EffDate == null ? "" : ca.EffDate.Value.ToShortDateString(),

                                        Fld2 = ca1.Name == null ? "" : ca1.Name,
                                        Fld3 = ca1.PromoList == null ? "" : ca1.PromoList.LookupVal.ToUpper(),

                                        Fld4 = ca1.PromoPolicy.Name == null ? "" : ca1.PromoPolicy.Name.ToString(),
                                        Fld5 = ca1.PromoPolicy.FullDetails == null ? "" : ca1.PromoPolicy.FullDetails.ToString(),
                                        Fld6 = ca1.PromoPolicy.IsActionDateAsIncrDate == null ? "" : ca1.PromoPolicy.IsActionDateAsIncrDate.ToString(),
                                        Fld7 = ca1.PromoPolicy.IsFuncStructChange == null ? "" : ca1.PromoPolicy.IsFuncStructChange.ToString(),
                                        Fld8 = ca1.PromoPolicy.IsNewScaleIncrAction == null ? "" : ca1.PromoPolicy.IsNewScaleIncrAction.ToString(),
                                        Fld9 = ca1.PromoPolicy.IsOldScaleIncrAction == null ? "" : ca1.PromoPolicy.IsOldScaleIncrAction.ToString(),
                                        Fld10 = ca1.PromoPolicy.IsPayJobStatusChange == null ? "" : ca1.PromoPolicy.IsPayJobStatusChange.ToString(),
                                        Fld11 = ca1.PromoPolicy.IsPayStructChange == null ? "" : ca1.PromoPolicy.IsPayStructChange.ToString(),

                                    };

                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }

                        break;



                    case "TRANSFERACTIVITYMASTER":


                        var OTransferData = db.PayScaleAgreement
                        .Include(t => t.TransActivity)
                         .Include(t => t.TransActivity.Select(e => e.TranPolicy))
                         .Include(t => t.TransActivity.Select(e => e.TransList))
                        .Where(d => d.Id == CompanyId).AsNoTracking().ToList();

                        if (OTransferData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OTransferData)
                            {
                                var OITTrans = ca.TransActivity.ToList();
                                //if (OITTrans != null && OITTrans.Count() != 0)
                                //{
                                foreach (var ca1 in OITTrans)
                                {


                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.EffDate == null ? "" : ca.EffDate.Value.ToShortDateString(),

                                        Fld2 = ca1.Name == null ? "" : ca1.Name,
                                        Fld3 = ca1.TransList == null ? "" : ca1.TransList.LookupVal.ToUpper(),

                                        Fld4 = ca1.TranPolicy.Name == null ? "" : ca1.TranPolicy.Name.ToString(),
                                        Fld5 = ca1.TranPolicy.IsDepartmentChange == null ? "" : ca1.TranPolicy.IsDepartmentChange.ToString(),
                                        Fld6 = ca1.TranPolicy.IsDivsionChange == null ? "" : ca1.TranPolicy.IsDivsionChange.ToString(),
                                        Fld7 = ca1.TranPolicy.IsFuncStructChange == null ? "" : ca1.TranPolicy.IsFuncStructChange.ToString(),
                                        Fld8 = ca1.TranPolicy.IsGroupChange == null ? "" : ca1.TranPolicy.IsGroupChange.ToString(),
                                        Fld9 = ca1.TranPolicy.IsLocationChange == null ? "" : ca1.TranPolicy.IsLocationChange.ToString(),
                                        Fld10 = ca1.TranPolicy.IsUnitChange == null ? "" : ca1.TranPolicy.IsUnitChange.ToString(),


                                    };

                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }

                        break;




                    case "OTHERSERVICEBOOKACTIVITYMASTER":


                        var OOtherServiceData = db.PayScaleAgreement
                            .Include(t => t.OthServiceBookActivity)

                         .Include(t => t.OthServiceBookActivity.Select(e => e.OthServiceBookPolicy))
                          .Include(t => t.OthServiceBookActivity.Select(e => e.OtherSerBookActList))
                        .Where(d => d.Id == CompanyId).AsNoTracking().ToList();

                        if (OOtherServiceData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OOtherServiceData)
                            {
                                var OITTrans = ca.OthServiceBookActivity.ToList();
                                //if (OITTrans != null && OITTrans.Count() != 0)
                                //{
                                foreach (var ca1 in OITTrans)
                                {


                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.EffDate == null ? "" : ca.EffDate.Value.ToShortDateString(),

                                        Fld2 = ca1.Name == null ? "" : ca1.Name,
                                        Fld3 = ca1.OtherSerBookActList == null ? "" : ca1.OtherSerBookActList.LookupVal.ToString(),

                                        Fld4 = ca1.OthServiceBookPolicy.Name == null ? "" : ca1.OthServiceBookPolicy.Name.ToString(),
                                        Fld5 = ca1.OthServiceBookPolicy.IsPayJobStatusChange == null ? "" : ca1.OthServiceBookPolicy.IsPayJobStatusChange.ToString(),
                                        Fld6 = ca1.OthServiceBookPolicy.IsFuncStructChange == null ? "" : ca1.OthServiceBookPolicy.IsFuncStructChange.ToString(),



                                    };

                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "EXTNREDNACTIVITYMASTER":


                        var OExtnRednServiceData = db.PayScaleAgreement
                            .Include(t => t.ExtnRednActivity)


                         .Include(t => t.ExtnRednActivity.Select(e => e.ExtnRednPolicy))
                         .Include(t => t.ExtnRednActivity.Select(e => e.ExtnRednList))
                           .Include(t => t.ExtnRednActivity.Select(e => e.ExtnRednPolicy.ExtnRednCauseType))
                         .Include(t => t.ExtnRednActivity.Select(e => e.ExtnRednPolicy.ExtnRednPeriodUnit))
                        .Where(d => d.Id == CompanyId).AsNoTracking().ToList();

                        if (OExtnRednServiceData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OExtnRednServiceData)
                            {
                                var OITTrans = ca.ExtnRednActivity.ToList();
                                //if (OITTrans != null && OITTrans.Count() != 0)
                                //{
                                foreach (var ca1 in OITTrans)
                                {


                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.EffDate == null ? "" : ca.EffDate.Value.ToShortDateString(),

                                        Fld2 = ca1.Name == null ? "" : ca1.Name,
                                        Fld3 = ca1.ExtnRednList == null ? "" : ca1.ExtnRednList.LookupVal.ToUpper(),

                                        Fld4 = ca1.ExtnRednPolicy.Name == null ? "" : ca1.ExtnRednPolicy.Name.ToString(),
                                        Fld5 = ca1.ExtnRednPolicy.ExtnRednCauseType == null ? "" : ca1.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToString(),
                                        Fld6 = ca1.ExtnRednPolicy.ExtnRednPeriod == null ? "" : ca1.ExtnRednPolicy.ExtnRednPeriod.ToString(),
                                        Fld7 = ca1.ExtnRednPolicy.ExtnRednPeriodUnit == null ? "" : ca1.ExtnRednPolicy.ExtnRednPeriodUnit.LookupVal.ToString(),
                                        Fld8 = ca1.ExtnRednPolicy.IsExtn == false ? "false" : "true",
                                        Fld9 = ca1.ExtnRednPolicy.IsRedn == false ? "false" : "true",

                                        Fld10 = ca1.ExtnRednPolicy.MaxCount == null ? "" : ca1.ExtnRednPolicy.MaxCount.ToString()
                                    };

                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "EMPLOYEESTAFFLIST":

                        var OEmpStaffInfoData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OEmpStaffInfoData_t = db.EmployeePayroll
                               .Include(e => e.Employee)
                               .Include(e => e.Employee.EmpName)
                               .Include(e => e.Employee.Gender)
                               .Include(e => e.Employee.MaritalStatus)
                               .Include(e => e.Employee.EmpOffInfo)
                               .Include(e => e.Employee.ServiceBookDates)
                               .Include(e => e.Employee.GeoStruct)
                               .Include(e => e.Employee.PayStruct)
                               .Include(e => e.Employee.FuncStruct)
                               .Include(e => e.SalaryT)
                               .Include(e => e.Employee.EmpAcademicInfo)
                               .Include(e => e.Employee.EmpAcademicInfo.QualificationDetails)
                               .Include(e => e.Employee.EmpAcademicInfo.QualificationDetails.Select(q => q.Qualification))
                               .Where(e => e.Employee.Id == item).AsNoTracking().FirstOrDefault();

                            if (OEmpStaffInfoData_t != null)
                            {
                                OEmpStaffInfoData.Add(OEmpStaffInfoData_t);
                            }
                        }

                        if (OEmpStaffInfoData == null || OEmpStaffInfoData.Count() == 0)
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


                            foreach (var ca in OEmpStaffInfoData.OrderBy(e => e.Employee.EmpCode))
                            {
                                var paymonth = ca.SalaryT.Select(q => q.PayMonth).ToList();
                                var paymonth1 = mPayMonth.FirstOrDefault();
                                if (paymonth.Contains(paymonth1))
                                {
                                    int geoid = ca.Employee.GeoStruct.Id;

                                    int payid = ca.Employee.PayStruct.Id;

                                    int funid = ca.Employee.FuncStruct.Id;

                                    GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                    PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                    FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                    var bcat = db.LookupValue.Where(e => e.Id == GeoDataInd.BusinessCategory_Id).Select(e => e.LookupVal).FirstOrDefault();
                                    if (ca.Employee.EmpAcademicInfo != null)
                                    {
                                        var QualificationInfo = ca.Employee.EmpAcademicInfo.QualificationDetails.ToList();
                                        if (QualificationInfo.Count() > 0)
                                        {
                                            foreach (var ca1 in QualificationInfo)
                                            {
                                                if (ca1.Qualification.Count() > 0)
                                                {
                                                    foreach (var ca2 in ca1.Qualification)
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        {
                                                            Fld1 = ca.Employee.EmpCode,
                                                            Fld2 = ca.Employee.EmpName.FullNameFML,

                                                            Fld3 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                                            Fld4 = FuncdataInd.Job_Name == null ? "" : FuncdataInd.Job_Name,

                                                            Fld5 = ca2.QualificationDesc == null ? "" : ca2.QualificationDesc, ///qualification

                                                            Fld6 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                                            Fld7 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),

                                                            Fld8 = ca.Employee.ServiceBookDates.LastTransferDate == null ? "" : ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                                            Fld9 = PaydataInd.Grade_Code == null ? "" : PaydataInd.Grade_Code,
                                                            Fld10 = FuncdataInd.Job_Code == null ? "" : FuncdataInd.Job_Code,
                                                            Fld11 = bcat == null ? "" : bcat,
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

                                                else
                                                {
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML,

                                                        Fld3 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                                        Fld4 = FuncdataInd.Job_Name == null ? "" : FuncdataInd.Job_Name,

                                                        Fld5 = "", ///qualification

                                                        Fld6 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                                        Fld7 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),

                                                        Fld8 = ca.Employee.ServiceBookDates.LastTransferDate == null ? "" : ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                                        Fld9 = PaydataInd.Grade_Code == null ? "" : PaydataInd.Grade_Code,
                                                        Fld10 = FuncdataInd.Job_Code == null ? "" : FuncdataInd.Job_Code,
                                                        Fld11 = bcat == null ? "" : bcat,
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

                                        else
                                        {
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML,

                                                Fld3 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                                Fld4 = FuncdataInd.Job_Name == null ? "" : FuncdataInd.Job_Name,

                                                Fld5 = "",

                                                Fld6 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                                Fld7 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),

                                                Fld8 = ca.Employee.ServiceBookDates.LastTransferDate == null ? "" : ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                                Fld9 = PaydataInd.Grade_Code == null ? "" : PaydataInd.Grade_Code,
                                                Fld10 = FuncdataInd.Job_Code == null ? "" : FuncdataInd.Job_Code,
                                                Fld11 = bcat == null ? "" : bcat,
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


                                    else
                                    {

                                        GenericField100 OGenericObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca.Employee.EmpCode,
                                            Fld2 = ca.Employee.EmpName.FullNameFML,

                                            Fld3 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                            Fld4 = FuncdataInd.Job_Name == null ? "" : FuncdataInd.Job_Name,

                                            Fld5 = "", ///qualification

                                            Fld6 = ca.Employee.ServiceBookDates.ConfirmationDate == null ? "" : ca.Employee.ServiceBookDates.ConfirmationDate.Value.ToShortDateString(),
                                            Fld7 = ca.Employee.ServiceBookDates.RetirementDate == null ? "" : ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),

                                            Fld8 = ca.Employee.ServiceBookDates.LastTransferDate == null ? "" : ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                            Fld9 = PaydataInd.Grade_Code == null ? "" : PaydataInd.Grade_Code,
                                            Fld10 = FuncdataInd.Job_Code == null ? "" : FuncdataInd.Job_Code,
                                            Fld11 = bcat == null ? "" : bcat,
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
                            return OGenericPayrollStatement;
                        }

                        break;


                    case "GEOFENCINGMASTER":


                        var OGeoFencing = db.Android_Application
                           .Include(e => e.Employee)
                           .Include(e => e.Employee.GeoStruct)
                           .Include(e => e.Employee.GeoStruct.Location)
                           .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                           .Include(e => e.Employee.PayStruct)
                           .Include(e => e.Employee.FuncStruct)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.Employee.EmpOffInfo)
                           .Include(e => e.Employee.EmpOffInfo.Branch)
                          .AsNoTracking()
                          .ToList();

                        if (OGeoFencing == null)
                        {
                            return null;
                        }
                        else
                        {

                            var SpGroup = SpecialGroupslist.ToList();


                            foreach (var item1 in SpGroup)
                            {
                                if (item1 == "GeoFencingApplicable")
                                {
                                    OGeoFencing = OGeoFencing.Where(e => e.IsGeoFenceAttendApplicable == true && e.Active_User == true).ToList();
                                }
                                if (item1 == "GeoFencingNotApplicable")
                                {
                                    OGeoFencing = OGeoFencing.Where(e => e.IsGeoFenceAttendApplicable == false && e.Active_User == true).ToList();
                                }
                                if (item1 == "EmployeeNotAssigned")
                                {
                                    OGeoFencing = OGeoFencing.Where(e => e.Active_User == false).ToList();
                                }
                            }




                            foreach (var ca in OGeoFencing)
                            {

                                GenericField100 OGenericObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.Employee != null ? ca.Employee.EmpCode.ToString() : "",
                                    Fld3 = ca.Employee != null ? ca.Employee.EmpName.FullNameFML.ToString() : "",
                                    Fld4 = ca.Phone_No == null ? "" : ca.Phone_No.ToString(),
                                    Fld5 = ca.IMEI_No == null ? "" : ca.IMEI_No.ToString(),
                                    Fld6 = ca.Active_User == false ? "No" : "Yes",
                                    Fld7 = ca.IsGeoFenceAttendApplicable == false ? "No" : "Yes",

                                };
                                OGenericPayrollStatement.Add(OGenericObjStatement);
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
        public partial class ArtistElement
        {
            public long Id { get; set; }
            public string Text { get; set; }
            // public bool Icon { get; set; }
            //public LiAttr LiAttr { get; set; }
            //  public AAttr AAttr { get; set; }
            public ArtistData Data { get; set; }
            public List<Child> Children { get; set; }
        }

        //public partial class AAttr
        //{
        //    public string Href { get; set; }
        //    public string Id { get; set; }
        //}

        public partial class Child
        {
            public string Id { get; set; }
            public string Text { get; set; }
            // public bool Icon { get; set; }
            //  public LiAttr LiAttr { get; set; }
            //  public AAttr AAttr { get; set; }
            public ChildData Data { get; set; }
            public List<Child> Children { get; set; }
        }

        public partial class ChildData
        {
            public string Url { get; set; }
            public string Authority { get; set; }
        }

        public partial class LiAttr
        {
            public string Id { get; set; }
        }

        public partial class ArtistData
        {
            public string Url { get; set; }
        }



    }
}