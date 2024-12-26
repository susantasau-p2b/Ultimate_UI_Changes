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
    public class ServiceBookReportGen
    {
        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };
        public static List<GenericField100> GenerateServiceBookReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime pFromDate, DateTime pToDate)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {


                    //case "INCREMENTLETTER":

                    //    var IncrementBookData = new List<EmployeePayroll>();
                    //      var IncrementDataR = db.EmployeePayroll.Select(r => new
                    //    {
                    //        iEmployeeId = r.Employee.Id,
                    //        iEmpCode = r.Employee.EmpCode,
                    //        iEmpName = r.Employee.EmpName.FullNameFML,
                    //        iGeoStructId = r.Employee.GeoStruct_Id,
                    //        iPayStructId = r.Employee.PayStruct_Id,
                    //        iFuncStructId = r.Employee.FuncStruct_Id,

                    //        iIncrement = r.IncrementServiceBook.Select(v => new
                    //        {
                    //            LocationCode = v.GeoStruct.Location.LocationObj.LocCode,
                    //            LocationDesc = v.GeoStruct.Location.LocationObj.LocDesc,

                    //            GradeCode = v.PayStruct.Grade.Code,
                    //            GradeDesc = v.EmployeePayroll.Employee.PayStruct.Grade.Name,

                    //            DesignationCode = v.FuncStruct.Job.Code,
                    //            DesignationDesc = v.FuncStruct.Job.Name,
                    //            ProcessIncrDate = v.ProcessIncrDate,
                    //            OldBasic = v.OldBasic,
                    //            NewBasic = v.NewBasic,
                      
                    //            ReleaseFlag =   v.ReleaseFlag


                    //        }).Where(e => e.ProcessIncrDate >= pFromDate && e.ProcessIncrDate <= pToDate && e.ReleaseFlag == true).ToList()

                    //    }).Where(e => EmpPayrollIdList.Contains(e.iEmployeeId)).ToList();

                    //      if (IncrementDataR != null)
                    //    {

                    //    }

                    //      if (IncrementDataR == null || IncrementDataR.Count() == 0)
                    //    {
                    //        return null;
                    //    }
                    //    else
                    //    {
                    //        var month = false;
                    //        var emp = false;
                    //        var dept = false;
                    //        var loca = false;
                    //        var comp = false;
                    //        var grp = false;
                    //        var unit = false;
                    //        var div = false;
                    //        var regn = false;
                    //        var grade = false;
                    //        var lvl = false;
                    //        var jobstat = false;
                    //        var job = false;
                    //        var jobpos = false;
                    //        var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).ToList();

                    //        foreach (var item in vc)
                    //        {
                    //            if (item.LookupVal.ToUpper() == "MONTH")
                    //            {
                    //                month = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "LOCATION")
                    //            {

                    //                loca = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "EMPLOYEE")
                    //            {
                    //                emp = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "DEPARTMENT")
                    //            {
                    //                dept = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "COMPANY")
                    //            {
                    //                comp = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "GROUP")
                    //            {
                    //                grp = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "UNIT")
                    //            {
                    //                unit = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "DIVISION")
                    //            {
                    //                div = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "REGION")
                    //            {
                    //                regn = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "GRADE")
                    //            {
                    //                grade = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "LEVEL")
                    //            {
                    //                lvl = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "JOBSTATUS")
                    //            {
                    //                jobstat = true;
                    //            }

                    //            if (item.LookupVal.ToUpper() == "JOB")
                    //            {
                    //                job = true;
                    //            }
                    //            if (item.LookupVal.ToUpper() == "JOBPOSITION")
                    //            {
                    //                jobpos = true;
                    //            }
                    //        }

                    //        Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                    //        foreach (var ca in IncrementDataR)
                    //        {
                    //            int? geoid = ca.iGeoStructId;

                    //            int? payid = ca.iPayStructId;

                    //            int? funid = ca.iFuncStructId;

                    //            GeoStruct geostruct = db.GeoStruct.Find(geoid);

                    //            PayStruct paystruct = db.PayStruct.Find(payid);

                    //            FuncStruct funstruct = db.FuncStruct.Find(funid);

                    //            GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                    //            foreach (var item in ca.iIncrement.Where(r => r.ProcessIncrDate >= pFromDate && r.ProcessIncrDate <= pToDate))
                    //            {
                    //                GenericField100 OGenericObjStatement = new GenericField100()
                    //                {
                    //                    Fld1 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                    //                    Fld2 = ca.iEmpName == null ? "" : ca.iEmpName,
                    //                    Fld3 = item.DesignationCode.ToString(),
                    //                    Fld4 = item.DesignationDesc.ToString(),
                    //                    Fld5 = item.GradeCode.ToString(),
                    //                    Fld6 = GeoDataInd.PayStruct_Grade_Name,
                    //                    //Fld6 = item.GradeDesc.ToString(),
                    //                    Fld7 = item.LocationCode.ToString(),
                    //                    Fld8 = item.LocationDesc.ToString(),
                    //                    Fld9 = item.ProcessIncrDate.Value.ToShortDateString().ToString(),
                    //                    Fld10 = item.OldBasic.ToString(),
                    //                    Fld11 = item.NewBasic.ToString(),
                    //                    Fld12 = (item.NewBasic - item.OldBasic).ToString(),

                    //                    Fld13 = item.ProcessIncrDate.Value.Date.ToString("ddd") + " " + item.ProcessIncrDate.Value.ToShortDateString().ToString(),

                    //                };

                    //                if (comp)
                    //                {
                    //                    OGenericObjStatement.Fld99 = GeoDataInd.GeoStruct_Company_Name;
                    //                }
                    //                if (div)
                    //                {
                    //                    OGenericObjStatement.Fld98 = GeoDataInd.GeoStruct_Division_Name;
                    //                }
                    //                if (loca)
                    //                {
                    //                    OGenericObjStatement.Fld97 = GeoDataInd.GeoStruct_Location_Name;
                    //                }
                    //                if (dept)
                    //                {
                    //                    OGenericObjStatement.Fld96 = GeoDataInd.GeoStruct_Department_Name;
                    //                }
                    //                if (grp)
                    //                {
                    //                    OGenericObjStatement.Fld95 = GeoDataInd.GeoStruct_Group_Name;
                    //                }
                    //                if (unit)
                    //                {
                    //                    OGenericObjStatement.Fld94 = GeoDataInd.GeoStruct_Unit_Name;
                    //                }
                    //                if (grade)
                    //                {
                    //                    OGenericObjStatement.Fld93 = GeoDataInd.PayStruct_Grade_Name;
                    //                }
                    //                if (lvl)
                    //                {
                    //                    OGenericObjStatement.Fld92 = GeoDataInd.PayStruct_Level_Name;
                    //                }
                    //                if (jobstat)
                    //                {
                    //                    OGenericObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                    //                }
                    //                if (job)
                    //                {
                    //                    OGenericObjStatement.Fld90 = GeoDataInd.FuncStruct_Job_Name;
                    //                }
                    //                if (jobpos)
                    //                {
                    //                    OGenericObjStatement.Fld89 = GeoDataInd.FuncStruct_JobPosition_Name;
                    //                }
                    //                if (emp)
                    //                {
                    //                    OGenericObjStatement.Fld88 = ca.iEmpName;
                    //                }

                    //                OGenericPayrollStatement.Add(OGenericObjStatement);

                    //            }
                    //        }

                    //    }

                    //    return OGenericPayrollStatement;


                    //    break;
                    case "PROMOTIONLETTER":
                        var PromotionBookData = new List<EmployeePayroll>();
                        if (salheadlist.Count() == 0) 
                        {
                            var promo = db.PromoActivity.Select(r => r.Name).ToList();
                            foreach (var item in promo)
                            {
                                salheadlist.Add(item.ToString());

                            }
                            
                        }

                        var PromotionDataR = db.EmployeePayroll.Select(r => new
                        {
                            iEmployeeId = r.Employee.Id,
                            iEmpCode = r.Employee.EmpCode,
                            iEmpName = r.Employee.EmpName.FullNameFML,
                            iGeoStructId = r.Employee.GeoStruct_Id,
                            iPayStructId = r.Employee.PayStruct_Id,
                            iFuncStructId = r.Employee.FuncStruct_Id,
                           

                            iPromotion = r.PromotionServiceBook.Select(v => new
                            {
                                OldFuncstruct = v.OldFuncStruct_Id,
                                OldPayStruct = v.OldPayStruct_Id,

                                NewFuncStruct = v.NewFuncStruct_Id,
                                NewPayStruct = v.NewPayStruct_Id,

                                ReleaseFlag =   v.ReleaseFlag,
                                ProcessPromoDate = v.ProcessPromoDate,
                                PromotionActivity = v.PromotionActivity.PromoPolicy.Name

                            }).Where(e => e.ProcessPromoDate >= pFromDate && e.ProcessPromoDate <= pToDate && salheadlist.Contains(e.PromotionActivity) &&  e.ReleaseFlag == true).ToList()

                        }).Where(e => EmpPayrollIdList.Contains(e.iEmployeeId)).ToList();

                       

                     
                        if (PromotionDataR == null || PromotionDataR.Count() == 0)
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
                            Utility.GetOrganizationDataClass OldGeoDataInd = new Utility.GetOrganizationDataClass();
                            Utility.GetOrganizationDataClass NewGeoDataInd = new Utility.GetOrganizationDataClass();
                        
                            foreach (var ca in PromotionDataR)
                            {
                                int? geoid = ca.iGeoStructId;

                                int? payid = ca.iPayStructId;

                                int? funid = ca.iFuncStructId;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                foreach (var item in ca.iPromotion.Where(r => r.ProcessPromoDate >= pFromDate && r.ProcessPromoDate <= pToDate))
                                {
                                    GeoStruct ogeostruct = db.GeoStruct.Find(geoid);

                                    PayStruct opaystruct = db.PayStruct.Find(item.OldPayStruct);

                                    FuncStruct ofunstruct = db.FuncStruct.Find(item.OldFuncstruct);

                                    OldGeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(ogeostruct, opaystruct, ofunstruct, 0);

                                    GeoStruct Ngeostruct = db.GeoStruct.Find(geoid);

                                    PayStruct Npaystruct = db.PayStruct.Find(item.NewPayStruct);

                                    FuncStruct Nfunstruct = db.FuncStruct.Find(item.NewFuncStruct);

                                    NewGeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(Ngeostruct, Npaystruct, Nfunstruct, 0);


                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                        Fld2 = ca.iEmpName == null ? "" : ca.iEmpName,
                                        Fld3 = OldGeoDataInd.FuncStruct_Job_Code,
                                        Fld4 = OldGeoDataInd.FuncStruct_Job_Name,
                                        Fld5 = NewGeoDataInd.FuncStruct_Job_Code,
                                        Fld6 = NewGeoDataInd.FuncStruct_Job_Name,

                                        Fld7 = OldGeoDataInd.PayStruct_Grade_Code.ToString()+" "+OldGeoDataInd.PayStruct_Grade_Name.ToString(),
                                        Fld8 = NewGeoDataInd.PayStruct_Grade_Code.ToString() + " " + NewGeoDataInd.PayStruct_Grade_Name.ToString(),
                                        Fld9 = item.PromotionActivity.ToString(),
                                       
                                        Fld13 = item.ProcessPromoDate.Value.Date.ToString("dddd") + " " + item.ProcessPromoDate.Value.ToShortDateString().ToString(),

                                        Fld14 = item.ProcessPromoDate.Value.Date.ToString("MMMM") + " " + item.ProcessPromoDate.Value.Year,
                                        Fld15 = OldGeoDataInd.PayStruct_JobStatus_EmpStatus,
                                        Fld16 = NewGeoDataInd.PayStruct_JobStatus_EmpStatus,
                                       


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



                    case "TRANSFERLETTER":
                         var TransferBookData = new List<EmployeePayroll>();

                         if (salheadlist.Count() == 0)
                         {
                             var Trans = db.TransPolicy.Select(b => b.Name).Distinct().ToList();
                             foreach (var item in Trans )
                             {                               
                                 salheadlist.Add(item.ToString());
                             }
                             
                         }
                       
                         var TransferDataR = db.EmployeePayroll.Select(r => new
                         {
                             iEmployeeId = r.Employee.Id,
                             iEmpCode = r.Employee.EmpCode,
                             iEmpName = r.Employee.EmpName.FullNameFML,                       
                             iGeoStructId = r.Employee.GeoStruct_Id,
                             iPayStructId = r.Employee.PayStruct_Id,
                             iFuncStructId = r.Employee.FuncStruct_Id,



                             iTransfer = r.TransferServiceBook.Select(v => new
                             {
                                 OldFuncstruct = v.OldFuncStruct_Id,
                                 OldGeoStruct = v.OldGeoStruct_Id,
                                 OldPayStruct = v.OldPayStruct_Id,
                                 NewFuncStruct = v.NewFuncStruct_Id,
                                 NewGeoStruct = v.NewGeoStruct_Id,
                                 NewPayStruct = v.NewPayStruct_Id,
                                 ReleaseFlag = v.ReleaseFlag,
                                
                                 ProcessTransDate = v.ProcessTransDate,
                                 TransActivity = v.TransActivity.TranPolicy.Name

                             }).Where(e => e.ProcessTransDate>=pFromDate && e.ProcessTransDate<=pToDate && salheadlist.Contains(e.TransActivity) && e.ReleaseFlag == true).ToList()

                         }).Where(e => EmpPayrollIdList.Contains(e.iEmployeeId)).ToList();

                        

                         if (TransferDataR != null)
                        {

                        }

                         if (TransferDataR == null || TransferDataR.Count() == 0)
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
                            Utility.GetOrganizationDataClass OldGeoDataInd = new Utility.GetOrganizationDataClass();
                            Utility.GetOrganizationDataClass NewGeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in TransferDataR)
                            {
                                int? geoid = ca.iGeoStructId;

                                int? payid = ca.iPayStructId;

                                int? funid = ca.iFuncStructId;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                foreach (var item in ca.iTransfer.Where(r =>r.ProcessTransDate>= pFromDate && r.ProcessTransDate<= pToDate))
                                {
                                    GeoStruct ogeostruct = db.GeoStruct.Find(item.OldGeoStruct);

                                    PayStruct opaystruct = db.PayStruct.Find(item.OldPayStruct);

                                    FuncStruct ofunstruct = db.FuncStruct.Find(item.OldFuncstruct);

                                    OldGeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(ogeostruct, opaystruct, ofunstruct, 0);

                                    GeoStruct Ngeostruct = db.GeoStruct.Find(item.NewGeoStruct);

                                    PayStruct Npaystruct = db.PayStruct.Find(item.NewPayStruct);

                                    FuncStruct Nfunstruct = db.FuncStruct.Find(item.NewFuncStruct);

                                    NewGeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(Ngeostruct, Npaystruct, Nfunstruct, 0);


                                    GenericField100 OGenericObjStatement = new GenericField100()
                                    {
                                        Fld1 = ca.iEmpCode == null ? "" : ca.iEmpCode,
                                        Fld2 = ca.iEmpName == null ? "" : ca.iEmpName,

                                        Fld3=OldGeoDataInd.FuncStruct_Job_Code,
                                        Fld4 = OldGeoDataInd.FuncStruct_Job_Name,
                                        Fld5 = NewGeoDataInd.FuncStruct_Job_Code,
                                        Fld6 = NewGeoDataInd.FuncStruct_Job_Name,

                                        Fld7 = OldGeoDataInd.PayStruct_Grade_Code+" "+OldGeoDataInd.PayStruct_Grade_Name,
                                        Fld8 = NewGeoDataInd.PayStruct_Grade_Code+" "+NewGeoDataInd.PayStruct_Grade_Name,

                                        Fld9 = OldGeoDataInd.GeoStruct_Location_Code,
                                        Fld10 = OldGeoDataInd.GeoStruct_Location_Name,

                                        Fld11 = NewGeoDataInd.GeoStruct_Location_Code,
                                        Fld12 = NewGeoDataInd.GeoStruct_Location_Name,


                                        Fld13= item.ProcessTransDate.Value.Date.ToString("dddd") + " " + item.ProcessTransDate.Value.ToShortDateString().ToString(),
                                        
                                        Fld14 = item.ProcessTransDate.Value.Date.ToString("MMMM") + " " + item.ProcessTransDate.Value.Year,
                                        Fld15 = item.TransActivity.ToString(),

                                        Fld16 = OldGeoDataInd.PayStruct_JobStatus_EmpStatus,
                                        Fld17 = NewGeoDataInd.PayStruct_JobStatus_EmpStatus,


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


                    case "INCREMENTLETTER":


                        //List<string> business_category = new List<string>();
                        //if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //{
                        //var EmpDataListw = db.IncrementServiceBook.Where(w => EmpPayrollIdList.Contains(w.EmployeePayroll.Employee_Id.Value) && mPayMonth.Contains(w.ProcessIncrDate.Value.ToString("MM/yyyy")))


                        //         .Select(d => new
                        //         {

                        //         }).ToList();


                        //    foreach (var ca in IncDataList)
                        //    {
                        //        GenericField100 OGenericObjStatement = new GenericField100()
                        //           {
                        //               Fld1 = ca.EmpCode,
                        //               Fld2 = ca.EmpName,
                        //               Fld3 = ca.Designation,
                        //               Fld4 = ca.OldBasic,
                        //               Fld5 = ca.NewBasic,
                        //               Fld6 = ca.IncrementAmount,
                        //               Fld7 = ca.EffectiveDate,
                        //               Fld8 = ca.BranchCode,
                        //           };

                        //        OGenericPayrollStatement.Add(OGenericObjStatement);
                        //    }
                        //}
                        //if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                        //{
                        //    business_category.AddRange(salheadlist);
                        //    var IncDataList = db.IncrementServiceBook.Where(e => EmpPayrollIdList.Contains(e.EmployeePayroll.Employee_Id.Value) && salheadlist.Contains(e.IncrActivity.Name) && date > FromDate && date < ToDate && e.ReleaseFlag.ToString() == "1")
                        //        .Select(e => new
                        //        {
                        //            EmpCode = e.EmployeePayroll.Employee.EmpCode,
                        //            EmpName = e.EmployeePayroll.Employee.EmpName.FullNameFML,
                        //            BranchCode = e.EmployeePayroll.Employee.EmpOffInfo.Branch.Code.ToString(),
                        //            Designation = e.FuncStruct.Job.Name,
                        //            OldBasic = e.OldBasic.ToString(),
                        //            NewBasic = e.NewBasic.ToString(),
                        //            IncrementAmount = (e.NewBasic - e.OldBasic).ToString(),
                        //            EffectiveDate = date.ToString()
                        //        }).ToList();

                        //    foreach (var ca in IncDataList)
                        //    {
                        //        GenericField100 OGenericObjStatement = new GenericField100()
                        //           {
                        //               Fld1 = ca.EmpCode,
                        //               Fld2 = ca.EmpName,
                        //               Fld3 = ca.Designation,
                        //               Fld4 = ca.OldBasic,
                        //               Fld5 = ca.NewBasic,
                        //               Fld6 = ca.IncrementAmount,
                        //               Fld7 = ca.EffectiveDate.ToString,
                        //               Fld8 = ca.BranchCode,
                        //           };

                        //        OGenericPayrollStatement.Add(OGenericObjStatement);
                        //    }
                        //}


                        //return OGenericPayrollStatement;

                        //break;

                        var IncrementServiceBookData = new List<EmployeePayroll>();
                        //var PAYmonth = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OIncrementServiceBookData_t = db.EmployeePayroll

                            .Include(e => e.Employee).AsNoTracking()

                            .Where(e => e.Employee.Id == item).AsParallel()
                            .FirstOrDefault();

                            if (OIncrementServiceBookData_t != null)
                            {
                                OIncrementServiceBookData_t.Employee.EmpName = db.NameSingle.Find(OIncrementServiceBookData_t.Employee.EmpName_Id);
                                OIncrementServiceBookData_t.Employee.EmpOffInfo = db.EmpOff.Find(OIncrementServiceBookData_t.Employee.EmpOffInfo_Id);
                                OIncrementServiceBookData_t.Employee.GeoStruct = db.GeoStruct.Find(OIncrementServiceBookData_t.Employee.GeoStruct_Id);
                                OIncrementServiceBookData_t.Employee.GeoStruct.Location = db.Location.Find(OIncrementServiceBookData_t.Employee.GeoStruct.Location_Id);
                                OIncrementServiceBookData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Find(OIncrementServiceBookData_t.Employee.GeoStruct.Location.LocationObj_Id);
                                OIncrementServiceBookData_t.Employee.FuncStruct = db.FuncStruct.Find(OIncrementServiceBookData_t.Employee.FuncStruct_Id);
                                OIncrementServiceBookData_t.Employee.FuncStruct.Job = db.Job.Find(OIncrementServiceBookData_t.Employee.FuncStruct.Job_Id);
                                OIncrementServiceBookData_t.Employee.PayStruct = db.PayStruct.Find(OIncrementServiceBookData_t.Employee.PayStruct_Id);

                                if (salheadlist.Count() != 0 && salheadlist.Count() > 0)
                                {
                                    foreach (var incactivity in salheadlist)
                                    {
                                        OIncrementServiceBookData_t.IncrementServiceBook = db.IncrementServiceBook.Include(e => e.IncrActivity).Include(e => e.PayStruct).Where(e => e.EmployeePayroll_Id == OIncrementServiceBookData_t.Id && e.ReleaseFlag == true && e.IncrActivity.Name.ToUpper().ToString() == incactivity.ToUpper().ToString()).AsNoTracking().ToList();

                                        if (OIncrementServiceBookData_t.IncrementServiceBook != null)
                                        {
                                            IncrementServiceBookData.Add(OIncrementServiceBookData_t);
                                        }

                                    }

                                }

                                else
                                {
                                    OIncrementServiceBookData_t.Employee.EmpName = db.NameSingle.Find(OIncrementServiceBookData_t.Employee.EmpName_Id);
                                    OIncrementServiceBookData_t.Employee.EmpOffInfo = db.EmpOff.Find(OIncrementServiceBookData_t.Employee.EmpOffInfo_Id);
                                    OIncrementServiceBookData_t.Employee.GeoStruct = db.GeoStruct.Find(OIncrementServiceBookData_t.Employee.GeoStruct_Id);
                                    OIncrementServiceBookData_t.Employee.GeoStruct.Location = db.Location.Find(OIncrementServiceBookData_t.Employee.GeoStruct.Location_Id);
                                    OIncrementServiceBookData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Find(OIncrementServiceBookData_t.Employee.GeoStruct.Location.LocationObj_Id);
                                    OIncrementServiceBookData_t.Employee.FuncStruct = db.FuncStruct.Find(OIncrementServiceBookData_t.Employee.FuncStruct_Id);
                                    OIncrementServiceBookData_t.Employee.FuncStruct.Job = db.Job.Find(OIncrementServiceBookData_t.Employee.FuncStruct.Job_Id);
                                    OIncrementServiceBookData_t.IncrementServiceBook = db.IncrementServiceBook.Include(e => e.IncrActivity).Include(e => e.PayStruct).Where(e => e.EmployeePayroll_Id == OIncrementServiceBookData_t.Id && e.ReleaseFlag == true).AsNoTracking().ToList();
                                    IncrementServiceBookData.Add(OIncrementServiceBookData_t);
                                }
                            }
                        }
                        if (IncrementServiceBookData == null || IncrementServiceBookData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in IncrementServiceBookData)
                            {


                                if (ca.IncrementServiceBook != null && ca.IncrementServiceBook.Count() != 0)
                                {
                                    int? geoid = ca.IncrementServiceBook.FirstOrDefault().GeoStruct_Id;

                                    int? payid = ca.IncrementServiceBook.FirstOrDefault().PayStruct_Id;

                                    int? funid = ca.IncrementServiceBook.FirstOrDefault().FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OITTrans = ca.IncrementServiceBook.Where(e => mPayMonth.Contains(e.ProcessIncrDate.Value.ToString("MM/yyyy"))).ToList();

                                    if (OITTrans != null && OITTrans.Count() != 0)
                                    {
                                        foreach (var ca1 in OITTrans)
                                        {
                                            var PayScalelist = db.PayScaleAssignment.Include(e => e.SalHeadFormula)
                                                             .Include(e => e.SalHeadFormula.Select(p => p.PayStruct))
                                                             .Include(e => e.SalHeadFormula.Select(p => p.BASICDependRule))
                                                             .Include(e => e.SalHeadFormula.Select(p => p.BASICDependRule.BasicScale))
                                                             .Include(e => e.SalHeadFormula.Select(p => p.BASICDependRule.BasicScale.BasicScaleDetails))
                                                             .Include(e => e.SalaryHead)
                                                             .Include(e => e.SalaryHead.SalHeadOperationType)
                                                             .Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "Basic".ToUpper()).ToList();
                                            string BasicScale = "";
                                            var _CompId = Convert.ToInt32(SessionManager.CompanyId);
                                            var _CompCode = db.Company.Where(e => e.Id == _CompId).SingleOrDefault().Code.ToUpper();
                                            foreach (var basicitem in PayScalelist)
                                            {
                                                var sal = basicitem.SalHeadFormula.Where(e => e.PayStruct.Id == ca1.PayStruct.Id).ToList();
                                                if (sal.Count() > 0 && sal != null)
                                                {
                                                    foreach (var Basicitem in sal)
                                                    {
                                                        var Basicdetails = Basicitem.BASICDependRule.BasicScale.BasicScaleDetails.OrderBy(e => e.StartingSlab).ToList();
                                                        foreach (var a in Basicdetails)
                                                        {
                                                            BasicScale += PayrollReportGen._returnFormateOfBasicScale(_CompCode, a);
                                                        }
                                                        if (BasicScale != "")
                                                            BasicScale = BasicScale.Remove(BasicScale.Length - 1, 1);

                                                    }
                                                }

                                            }

                                            GenericField100 OGenericObjStatement = new GenericField100()

                                            {
                                                Fld1 = ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML,
                                                Fld3 = ca.Employee.FuncStruct.Job.Name,
                                                Fld4 = ca1.OldBasic.ToString(),
                                                Fld5 = ca1.NewBasic.ToString(),
                                                Fld6 = (ca1.NewBasic - ca1.OldBasic).ToString(),
                                                Fld7 = ca1.ProcessIncrDate.Value.ToShortDateString(),
                                                Fld8 = ca.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString(),
                                                Fld9 = ca1.IncrActivity != null ? ca1.IncrActivity.Name.ToString() : "",
                                                Fld10 = BasicScale != null ? BasicScale : "",
                                                Fld11 = ca1.Narration != null ? ca1.Narration : "",
                                                Fld12 = ca.Employee.GeoStruct.Location.LocationObj.LocCode.ToString(),
                                              
                                                Fld14 = GeoDataInd.FuncStruct_Job_Code,
                                                Fld15 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld16 = GeoDataInd.PayStruct_Grade_Code,
                                                Fld17 = GeoDataInd.PayStruct_Grade_Name
                                               

                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);

                                        }
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

                                var dept = false;
                                var loca = false;
                                var div = false;
                                var job = false;

                                var filter = "";

                                var vc = db.LookupValue.Where(a => forithead.Contains(a.Id.ToString())).AsNoTracking().ToList();
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
                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "INCREMENTSERVICEBOOK":
                        var OIncrementServiceBookData = new List<EmployeePayroll>();
                        //var PAYmonth = mPayMonth.FirstOrDefault();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OIncrementServiceBookData_t = db.EmployeePayroll
                                //.Include(e => e.IncrementServiceBook)
                            .Include(e => e.Employee).AsNoTracking()
                                //.Include(e => e.Employee.EmpName)

                            //.Include(r => r.IncrementServiceBook.Select(t => t.IncrActivity))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.IncrActivity.IncrList))

                            //.Include(r => r.IncrementServiceBook.Select(t => t.FuncStruct.Job))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.FuncStruct.JobPosition))

                            //.Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.Grade))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.JobStatus.EmpStatus))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.JobStatus.EmpActingStatus))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.Level))

                            //.Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Division))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Location.LocationObj))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Department.DepartmentObj))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Group))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Unit)).AsNoTracking()

                            .Where(e => e.Employee.Id == item).AsParallel()
                            .FirstOrDefault();

                            if (OIncrementServiceBookData_t != null)
                            {
                                OIncrementServiceBookData_t.Employee.EmpName = db.NameSingle.Find(OIncrementServiceBookData_t.Employee.EmpName_Id);
                                OIncrementServiceBookData_t.IncrementServiceBook = db.IncrementServiceBook.Where(e => e.EmployeePayroll_Id == OIncrementServiceBookData_t.Id).AsNoTracking().ToList();

                                foreach (var itemOISBD in OIncrementServiceBookData_t.IncrementServiceBook)
                                {
                                    itemOISBD.IncrActivity = db.IncrActivity.Find(itemOISBD.IncrActivity_Id);
                                    itemOISBD.IncrActivity.IncrList = db.LookupValue.Find(itemOISBD.IncrActivity.IncrList_Id);
                                }
                                OIncrementServiceBookData.Add(OIncrementServiceBookData_t);
                            }
                        }
                        if (OIncrementServiceBookData == null || OIncrementServiceBookData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OIncrementServiceBookData)
                            {

                                if (ca.IncrementServiceBook != null && ca.IncrementServiceBook.Count() != 0)
                                {
                                    int? geoid = ca.IncrementServiceBook.FirstOrDefault().GeoStruct_Id;

                                    int? payid = ca.IncrementServiceBook.FirstOrDefault().PayStruct_Id;

                                    int? funid = ca.IncrementServiceBook.FirstOrDefault().FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OITTrans = ca.IncrementServiceBook.Where(e => mPayMonth.Contains(e.ProcessIncrDate.Value.ToString("MM/yyyy"))).ToList();

                                    //var OITTrans = ca.IncrementServiceBook.ToList();

                                    if (OITTrans != null && OITTrans.Count() != 0)
                                    {
                                        foreach (var ca1 in OITTrans)
                                        {
                                            if (salheadlist.Count() > 0)
                                            {

                                                foreach (var incrActNameitem in salheadlist)
                                                {
                                                    if (ca1.IncrActivity.Name.ToUpper() == incrActNameitem.ToUpper())
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
                                                            //  Fld12 = ca1.ReleaseDate.Value.ToShortDateString(),
                                                            Fld12 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                            Fld13 = ca1.Narration == null ? "" : ca1.Narration,
                                                            Fld14 = ca1.StagnancyAppl.ToString(),
                                                            Fld15 = ca1.StagnancyCount.ToString(),


                                                            //Fld16 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                                            Fld16 = GeoDataInd.GeoStruct_Division_Name == null ? "" : GeoDataInd.GeoStruct_Division_Name,
                                                            Fld17 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                            Fld18 = GeoDataInd.GeoStruct_Department_Name == null ? "" : GeoDataInd.GeoStruct_Department_Name,
                                                            Fld19 = GeoDataInd.GeoStruct_Group_Name == null ? "" : GeoDataInd.GeoStruct_Group_Name,
                                                            Fld20 = GeoDataInd.GeoStruct_Unit_Name == null ? "" : GeoDataInd.GeoStruct_Unit_Name,

                                                            Fld21 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,

                                                            Fld22 = GeoDataInd.FuncStruct_JobPosition_Name == null ? "" : GeoDataInd.FuncStruct_JobPosition_Name,

                                                            Fld23 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,

                                                            Fld24 = GeoDataInd.PayStruct_Level_Name == null ? "" : GeoDataInd.PayStruct_Level_Name,

                                                            //Fld25 = GeoDataInd.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                            //Fld26 = GeoDataInd.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                                            Fld25 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpStatus,
                                                            Fld26 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpActingStatus,


                                                        };
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
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
                                                    Fld3 = ca.Employee.EmpName.FullNameFML,
                                                    Fld4 = ca1.Id.ToString(),
                                                    Fld5 = ca1.IncrActivity == null ? "" : ca1.IncrActivity.Name,
                                                    Fld6 = ca1.IncrActivity == null ? "" : ca1.IncrActivity.IncrList.LookupVal.ToUpper(),
                                                    Fld7 = ca1.OrignalIncrDate.Value.ToShortDateString(),
                                                    Fld8 = ca1.ProcessIncrDate.Value.ToShortDateString(),
                                                    Fld9 = ca1.OldBasic.ToString(),
                                                    Fld10 = ca1.NewBasic.ToString(),
                                                    Fld11 = ca1.ReleaseFlag.ToString(),
                                                    //  Fld12 = ca1.ReleaseDate.Value.ToShortDateString(),
                                                    Fld12 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                    Fld13 = ca1.Narration == null ? "" : ca1.Narration,
                                                    Fld14 = ca1.StagnancyAppl.ToString(),
                                                    Fld15 = ca1.StagnancyCount.ToString(),


                                                    //Fld16 = ca1.GeoStruct.Division == null ? "" : ca1.GeoStruct.Division.Name,
                                                    Fld16 = GeoDataInd.GeoStruct_Division_Name == null ? "" : GeoDataInd.GeoStruct_Division_Name,
                                                    Fld17 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                    Fld18 = GeoDataInd.GeoStruct_Department_Name == null ? "" : GeoDataInd.GeoStruct_Department_Name,
                                                    Fld19 = GeoDataInd.GeoStruct_Group_Name == null ? "" : GeoDataInd.GeoStruct_Group_Name,
                                                    Fld20 = GeoDataInd.GeoStruct_Unit_Name == null ? "" : GeoDataInd.GeoStruct_Unit_Name,

                                                    Fld21 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,

                                                    Fld22 = GeoDataInd.FuncStruct_JobPosition_Name == null ? "" : GeoDataInd.FuncStruct_JobPosition_Name,

                                                    Fld23 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,

                                                    Fld24 = GeoDataInd.PayStruct_Level_Name == null ? "" : GeoDataInd.PayStruct_Level_Name,

                                                    //Fld25 = GeoDataInd.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus == null ? "" : ca1.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                    //Fld26 = GeoDataInd.PayStruct.JobStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca1.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                                    Fld25 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpStatus,
                                                    Fld26 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpActingStatus,


                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                        //else
                                        //{
                                        //    return null;
                                        //}

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

                    case "OFFICIATINGPAYMENTDETAILS":
                        //var PayMonth = mPayMonth.FirstOrDefault();
                         var FromDate = pFromDate;
                         var ToDate = pToDate;
                        // var OOffDetails = new List<BMSPaymentReq>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        var OOffDetailst1 = db.BMSPaymentReq
                            .Include(e => e.EmployeePayroll)
                            .Include(e => e.EmployeePayroll.Employee)
                            .Include(e => e.EmployeePayroll.Employee.EmpName)
                            .Include(e => e.EmployeePayroll.Employee.GeoStruct)
                            .Include(e => e.EmployeePayroll.Employee.PayStruct)
                            .Include(e => e.EmployeePayroll.Employee.FuncStruct)
                            .Include(e => e.OffWFDetails)
                            .Where(e => EmpPayrollIdList.Contains(e.EmployeePayroll.Employee.Id)).AsNoTracking().AsParallel()
                            .ToList();

                     
                        List<int> salheadListInt = salheadlist.Select(int.Parse).ToList();

                       
                        var OOffDetailst = db.BMSPaymentReq
                            .Where(r => EmpPayrollIdList.Contains(r.EmployeePayroll.Employee.Id) &&
                                        (salheadListInt.Any() ? salheadListInt.Contains(r.SalaryHead.Id) : true))
                            .Select(r => new
                            {
                                EmployeeId = r.EmployeePayroll.Employee.Id,
                                EmpCode = r.EmployeePayroll.Employee.EmpCode,
                                EmpName = r.EmployeePayroll.Employee.EmpName,
                                GeoStruct_Id = r.EmployeePayroll.Employee.GeoStruct_Id,
                                PayStruct_Id = r.EmployeePayroll.Employee.PayStruct_Id,
                                FuncStruct_Id = r.EmployeePayroll.Employee.FuncStruct_Id,
                                SalName = r.SalaryHead.Name,
                                Salheadid = r.SalaryHead.Id,
                                SalCode = r.SalaryHead.Code,
                                FromPeriod = r.FromPeriod,
                                ToPeriod = r.ToPeriod,
                                PayMonth = r.PayMonth,
                                OffWFDetails = r.OffWFDetails.Select(a => new { 
                                   WFStatus = a.WFStatus,
                                   Id = a.Id,
                                }).ToList(),
                            }).ToList();

                        if (OOffDetailst == null || OOffDetailst.Count() == 0)
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

                            foreach (var ca in OOffDetailst.Where(e => e.FromPeriod >= pFromDate && e.ToPeriod <= pToDate))
                            {
                                int? geoid = ca.GeoStruct_Id;

                                int? payid = ca.PayStruct_Id;

                                int? funid = ca.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);
                          
                                            string WFStatus = "";

                                            if (ca.OffWFDetails.Count() > 0)
                                            {
                                                if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 0)
                                                    WFStatus = "Applied";
                                                else if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 1)
                                                    WFStatus = "Sanctioned";
                                                else if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 2)
                                                    WFStatus = "Sanction Rejected";
                                                else if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 3)
                                                    WFStatus = "Apporved";
                                                else if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 4)
                                                    WFStatus = "Approved Rejected";
                                                else if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 5)
                                                    WFStatus = "Approved By HRM (M)";
                                                else if (ca.OffWFDetails.OrderByDescending(e => e.Id).FirstOrDefault().WFStatus == 6)
                                                    WFStatus = "Cancel";
                                            }

                                            else
                                            {
                                                WFStatus = "Apporved By HR";
                                            }
                                            if (SpecialGroupslist.Count() > 0)
                                            {
                                                foreach (var expression in SpecialGroupslist)
                                                {
                                                    if (expression.ToUpper() == WFStatus.ToUpper())
                                                    {
                                                        GenericField100 OGenericAttObjStatement = new GenericField100()
                                                        {
                                                            Fld1 = ca.EmpCode,
                                                            Fld2 = ca.EmpName.FullNameFML,
                                                            Fld5 = ca.FromPeriod.Value.ToShortDateString(),
                                                            Fld6 = ca.ToPeriod.Value.ToShortDateString(),
                                                            Fld7 = WFStatus,
                                                            Fld11 = GeoDataInd.GeoStruct_Location_Code,
                                                            Fld3 = GeoDataInd.GeoStruct_Department_Code,
                                                            Fld12 = ca.PayMonth == null ? " " : ca.PayMonth,
                                                            Fld10 = GeoDataInd.GeoStruct_Location_Name,
                                                            Fld13 = GeoDataInd.GeoStruct_Department_Name,
                                                            Fld14 = GeoDataInd.FuncStruct_Job_Name,
                                                            Fld15 = ca.SalName,
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
                                                            OGenericAttObjStatement.Fld88 = ca.EmpName.FullNameFML;
                                                        }


                                                        OGenericPayrollStatement.Add(OGenericAttObjStatement);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                GenericField100 OGenericAttObjStatement = new GenericField100()
                                                {

                                                    Fld1 = ca.EmpCode,
                                                    Fld2 = ca.EmpName.FullNameFML,
                                                    Fld5 = ca.FromPeriod.Value.ToShortDateString(),
                                                    Fld6 = ca.ToPeriod.Value.ToShortDateString(),
                                                    Fld7 = WFStatus,
                                                    Fld11 = GeoDataInd.GeoStruct_Location_Code,
                                                    Fld3 = GeoDataInd.GeoStruct_Department_Code,
                                                    Fld12 = ca.PayMonth == null ? " " : ca.PayMonth,
                                                    Fld10 = GeoDataInd.GeoStruct_Location_Name,
                                                    Fld13 = GeoDataInd.GeoStruct_Department_Name,
                                                    Fld14 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld15 = ca.SalName,
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
                                                    OGenericAttObjStatement.Fld88 = ca.EmpName.FullNameFML;
                                                }

                                                OGenericPayrollStatement.Add(OGenericAttObjStatement);
                                            }
                                        
                                   
                               }
                            return OGenericPayrollStatement;
                        }
                        return null;
                        break;


                    case "INCREMENTSERVICEBOOKDETAILS":
                        var OIncrementServiceBookDetails = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OIncrementServiceBookDetails_t = db.EmployeePayroll
                                //.Include(e => e.IncrementServiceBook)
                            .Include(e => e.Employee)
                                //.Include(r => r.IncrementServiceBook.Select(t => t.PayStruct.Grade))

                            //.Include(r => r.IncrementServiceBook.Select(t => t.GeoStruct.Location.LocationObj))
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.IncrementServiceBook.Select(t => t.IncrActivity))
                                //.Include(r => r.IncrementServiceBook.Select(t => t.IncrActivity.IncrList))
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();

                            if (OIncrementServiceBookDetails_t != null)
                            {
                                OIncrementServiceBookDetails_t.Employee.EmpName = db.NameSingle.Find(OIncrementServiceBookDetails_t.Employee.EmpName_Id);
                                OIncrementServiceBookDetails_t.IncrementServiceBook = db.IncrementServiceBook.Where(e => e.EmployeePayroll_Id == OIncrementServiceBookDetails_t.Id).AsNoTracking().ToList();

                                foreach (var itemOISBD in OIncrementServiceBookDetails_t.IncrementServiceBook)
                                {
                                    itemOISBD.IncrActivity = db.IncrActivity.Find(itemOISBD.IncrActivity_Id);
                                    itemOISBD.IncrActivity.IncrList = db.LookupValue.Find(itemOISBD.IncrActivity.IncrList_Id);
                                }

                                OIncrementServiceBookDetails.Add(OIncrementServiceBookDetails_t);
                            }
                        }
                        if (OIncrementServiceBookDetails == null || OIncrementServiceBookDetails.Count() == 0)
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

                            foreach (var ca in OIncrementServiceBookDetails)
                            {

                                if (ca.IncrementServiceBook != null && ca.IncrementServiceBook.Count() != 0)
                                {
                                    int? geoid = ca.IncrementServiceBook.FirstOrDefault().GeoStruct_Id;

                                    int? payid = ca.IncrementServiceBook.FirstOrDefault().PayStruct_Id;

                                    int? funid = ca.IncrementServiceBook.FirstOrDefault().FuncStruct_Id;

                                    GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                    PayStruct paystruct = db.PayStruct.Find(payid);

                                    FuncStruct funstruct = db.FuncStruct.Find(funid);

                                    GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);

                                    var OITTrans = ca.IncrementServiceBook.Where(e => mPayMonth.Contains(e.ProcessIncrDate.Value.ToString("MM/yyyy"))).ToList();
                                    //  var OITTrans = ca.IncrementServiceBook.Contains(ProcessI)
                                    if (OITTrans != null && OITTrans.Count() != 0)
                                    {
                                        foreach (var ca1 in OITTrans)
                                        {
                                            if (salheadlist.Count() > 0)
                                            {
                                                foreach (var incrActNameitem in salheadlist)
                                                {
                                                    if (ca1.IncrActivity.Name.ToUpper() == incrActNameitem.ToUpper())
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
                                                            Fld12 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                            Fld13 = ca1.Narration == null ? "" : ca1.Narration,
                                                            Fld14 = ca1.StagnancyAppl.ToString(),
                                                            Fld15 = ca1.StagnancyCount.ToString(),
                                                            Fld16 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                            Fld17 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
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
                                            else
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
                                                    Fld12 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                    Fld13 = ca1.Narration == null ? "" : ca1.Narration,
                                                    Fld14 = ca1.StagnancyAppl.ToString(),
                                                    Fld15 = ca1.StagnancyCount.ToString(),
                                                    Fld16 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                    Fld17 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
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

                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "PROMOTIONSERVICEBOOK":
                        var OPromotionServiceBookData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPromotionServiceBookData_t = db.EmployeePayroll

                            //.Include(e => e.PromotionServiceBook)

                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity.PromoList))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity.PromoPolicy.IncrActivity))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldFuncStruct.Job))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewFuncStruct.JobPosition))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewFuncStruct))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.Grade))

                            //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpStatus))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpActingStatus))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.Grade))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpStatus))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpActingStatus))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayStruct.Level))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewPayStruct.Level))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Division))

                            //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Location.LocationObj))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Department.DepartmentObj))

                            //.Include(r => r.PromotionServiceBook.Select(t => t.GeoStruct.Group))

                            //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayScale))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayScale.PayScaleType))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.OldPayScaleAgreement))
                                // .Include(r => r.PromotionServiceBook.Select(t => t.NewPayScale))
                                // .Include(r => r.PromotionServiceBook.Select(t => t.NewPayScale.PayScaleType))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.NewPayScaleAgreement))

                            //.Where(e => e.Employee.Id == item && e.PromotionServiceBook.Count > 0)
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();


                            if (OPromotionServiceBookData_t != null)
                            {
                                OPromotionServiceBookData_t.PromotionServiceBook = db.PromotionServiceBook.Where(e => e.EmployeePayroll_Id == OPromotionServiceBookData_t.Id).AsNoTracking().ToList();
                                OPromotionServiceBookData_t.Employee.EmpName = db.NameSingle.Find(OPromotionServiceBookData_t.Employee.EmpName_Id);
                                foreach (var itemPro in OPromotionServiceBookData_t.PromotionServiceBook)
                                {

                                    itemPro.PromotionActivity = db.PromoActivity.Find(itemPro.PromotionActivity_Id);
                                    itemPro.PromotionActivity.PromoList = db.PromoActivity.Where(e => e.Id == itemPro.PromotionActivity_Id).Select(r => r.PromoList).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.PromotionActivity.PromoPolicy = db.PromoActivity.Where(e => e.Id == itemPro.PromotionActivity_Id).Select(r => r.PromoPolicy).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.PromotionActivity.PromoPolicy.IncrActivity = db.PromoPolicy.Where(e => e.Id == itemPro.PromotionActivity.PromoPolicy_Id).Select(r => r.IncrActivity).AsNoTracking().ToList().FirstOrDefault();

                                    itemPro.OldFuncStruct = db.FuncStruct.Find(itemPro.OldPayStruct_Id);
                                    if (itemPro.OldFuncStruct != null)
                                        itemPro.OldFuncStruct.Job = db.FuncStruct.Where(e => e.Id == itemPro.OldFuncStruct_Id).Select(r => r.Job).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.OldFuncStruct != null)
                                        itemPro.OldFuncStruct.JobPosition = db.FuncStruct.Where(e => e.Id == itemPro.OldFuncStruct_Id).Select(r => r.JobPosition).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.NewFuncStruct = db.FuncStruct.Where(e => e.Id == itemPro.NewFuncStruct_Id).ToList().FirstOrDefault();
                                    if (itemPro.NewFuncStruct != null)
                                        itemPro.NewFuncStruct.JobPosition = db.FuncStruct.Where(e => e.Id == itemPro.NewFuncStruct_Id).Select(r => r.JobPosition).AsNoTracking().ToList().FirstOrDefault();

                                    itemPro.OldPayStruct = db.PayStruct.Find(itemPro.OldPayStruct_Id);
                                    if (itemPro.OldPayStruct != null)
                                        itemPro.OldPayStruct.Grade = db.PayStruct.Where(e => e.Id == itemPro.OldPayStruct_Id).Select(r => r.Grade).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.OldPayStruct != null)
                                        itemPro.OldPayStruct.JobStatus = db.PayStruct.Where(e => e.Id == itemPro.OldPayStruct_Id).Select(r => r.JobStatus).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.OldPayStruct != null && itemPro.OldPayStruct.JobStatus != null)
                                        itemPro.OldPayStruct.JobStatus.EmpStatus = db.JobStatus.Where(e => e.Id == itemPro.OldPayStruct.JobStatus_Id).Select(a => a.EmpStatus).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.OldPayStruct != null && itemPro.OldPayStruct.JobStatus != null)
                                        itemPro.OldPayStruct.JobStatus.EmpActingStatus = db.JobStatus.Where(e => e.Id == itemPro.OldPayStruct.JobStatus_Id).Select(a => a.EmpActingStatus).AsNoTracking().ToList().FirstOrDefault();

                                    itemPro.NewPayStruct = db.PayStruct.Find(itemPro.NewPayStruct_Id);
                                    itemPro.NewPayStruct.Grade = db.PayStruct.Where(e => e.Id == itemPro.NewPayStruct_Id).Select(r => r.Grade).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.NewPayStruct.JobStatus = db.PayStruct.Where(e => e.Id == itemPro.OldPayStruct_Id).Select(r => r.JobStatus).ToList().FirstOrDefault();
                                    if (itemPro.NewPayStruct.JobStatus != null)
                                        itemPro.NewPayStruct.JobStatus.EmpStatus = db.JobStatus.Where(e => e.Id == itemPro.NewPayStruct.JobStatus_Id).Select(r => r.EmpStatus).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.NewPayStruct.JobStatus != null)
                                        itemPro.NewPayStruct.JobStatus.EmpActingStatus = db.JobStatus.Where(e => e.Id == itemPro.NewPayStruct.JobStatus_Id).Select(r => r.EmpActingStatus).AsNoTracking().ToList().FirstOrDefault();

                                    if (itemPro.OldPayStruct != null)
                                        itemPro.OldPayStruct.Level = db.PayStruct.Where(e => e.Id == itemPro.OldPayStruct_Id).Select(r => r.Level).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.NewPayStruct.Level = db.PayStruct.Where(e => e.Id == itemPro.NewPayStruct_Id).Select(r => r.Level).AsNoTracking().ToList().FirstOrDefault();

                                    itemPro.GeoStruct = db.GeoStruct.Find(itemPro.GeoStruct_Id);
                                    if (itemPro.GeoStruct != null)
                                        itemPro.GeoStruct.Division = db.GeoStruct.Where(e => e.Id == itemPro.GeoStruct_Id).Select(r => r.Division).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.GeoStruct != null)
                                        itemPro.GeoStruct.Location = db.GeoStruct.Where(e => e.Id == itemPro.GeoStruct_Id).Select(r => r.Location).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.GeoStruct != null && itemPro.GeoStruct.Location != null)
                                        itemPro.GeoStruct.Location.LocationObj = db.Location.Where(e => e.Id == itemPro.GeoStruct.Location_Id).Select(r => r.LocationObj).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.GeoStruct != null)
                                        itemPro.GeoStruct.Department = db.GeoStruct.Where(e => e.Id == itemPro.GeoStruct_Id).Select(r => r.Department).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.GeoStruct != null && itemPro.GeoStruct.Department != null)
                                        itemPro.GeoStruct.Department.DepartmentObj = db.Department.Where(e => e.Id == itemPro.GeoStruct.Department_Id).Select(r => r.DepartmentObj).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.GeoStruct != null)
                                        itemPro.GeoStruct.Group = db.GeoStruct.Where(e => e.Id == itemPro.GeoStruct_Id).Select(r => r.Group).AsNoTracking().ToList().FirstOrDefault();

                                    itemPro.OldPayScale = db.PayScale.Where(e => e.Id == itemPro.OldPayScale_Id).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.OldPayScale != null)
                                        itemPro.OldPayScale.PayScaleType = db.PayScale.Where(e => e.Id == itemPro.OldPayScale_Id).Select(r => r.PayScaleType).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.OldPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == itemPro.OldPayScaleAgreement_Id).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.NewPayScale = db.PayScale.Where(e => e.Id == itemPro.NewPayScale_Id).AsNoTracking().ToList().FirstOrDefault();
                                    if (itemPro.NewPayScale != null)
                                        itemPro.NewPayScale.PayScaleType = db.PayScale.Where(e => e.Id == itemPro.NewPayScale_Id).Select(r => r.PayScaleType).AsNoTracking().ToList().FirstOrDefault();
                                    itemPro.NewPayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == itemPro.NewPayScaleAgreement_Id).AsNoTracking().ToList().FirstOrDefault();
                                }

                                OPromotionServiceBookData.Add(OPromotionServiceBookData_t);
                            }
                        }
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
                                            if (salheadlist.Count > 0)
                                            {
                                                foreach (var transActNameitem in salheadlist)
                                                {
                                                    if (ca1.PromotionActivity.Name.ToUpper() == transActNameitem.ToUpper())
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
                                            }

                                            else
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

                    case "PROMOTIONSERVICEBOOKDETAILS":
                        var OPromotionServiceBookDetails = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OPromotionServiceBookDetails_t = db.EmployeePayroll
                                //.Include(e => e.PromotionServiceBook)
                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.PromotionServiceBook.Select(t => t.OldPayStruct.Grade))
                                //.Include(e => e.PromotionServiceBook.Select(t => t.NewPayStruct.Grade))
                                //.Include(e => e.PromotionServiceBook.Select(t => t.NewFuncStruct.Job))
                                //.Include(e => e.PromotionServiceBook.Select(t => t.OldFuncStruct.Job))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity))
                                //.Include(r => r.PromotionServiceBook.Select(t => t.PromotionActivity.PromoList))
                                //.Include(e => e.PromotionServiceBook.Select(t => t.GeoStruct))
                                //.Include(e => e.PromotionServiceBook.Select(t => t.GeoStruct.Location))
                                //.Include(e => e.PromotionServiceBook.Select(t => t.GeoStruct.Location.LocationObj))
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();

                            if (OPromotionServiceBookDetails_t != null)
                            {
                                OPromotionServiceBookDetails_t.PromotionServiceBook = db.PromotionServiceBook.Where(e => e.EmployeePayroll_Id == OPromotionServiceBookDetails_t.Id).AsNoTracking().ToList();
                                OPromotionServiceBookDetails_t.Employee.EmpName = db.NameSingle.Find(OPromotionServiceBookDetails_t.Employee.EmpName_Id);
                                foreach (var itemPSD in OPromotionServiceBookDetails_t.PromotionServiceBook)
                                {
                                    itemPSD.OldPayStruct = db.PayStruct.Find(itemPSD.OldPayStruct_Id);
                                    if (itemPSD.OldPayStruct != null)
                                        itemPSD.OldPayStruct.Grade = db.Grade.Find(itemPSD.OldPayStruct.Grade_Id);
                                    itemPSD.NewPayStruct = db.PayStruct.Find(itemPSD.NewPayStruct_Id);
                                    if (itemPSD.NewPayStruct != null)
                                        itemPSD.NewPayStruct.Grade = db.Grade.Find(itemPSD.NewPayStruct.Grade_Id);

                                    itemPSD.NewFuncStruct = db.FuncStruct.Find(itemPSD.NewFuncStruct_Id);
                                    if (itemPSD.NewFuncStruct != null)
                                        itemPSD.NewFuncStruct.Job = db.Job.Find(itemPSD.NewFuncStruct.Job_Id);
                                    itemPSD.OldFuncStruct = db.FuncStruct.Find(itemPSD.OldFuncStruct_Id);
                                    if (itemPSD.OldFuncStruct != null)
                                        itemPSD.OldFuncStruct.Job = db.Job.Find(itemPSD.OldFuncStruct.Job_Id);

                                    itemPSD.PromotionActivity = db.PromoActivity.Find(itemPSD.PromotionActivity_Id);
                                    if (itemPSD.PromotionActivity != null)
                                        itemPSD.PromotionActivity.PromoList = db.LookupValue.Find(itemPSD.PromotionActivity.PromoList_Id);

                                    itemPSD.GeoStruct = db.GeoStruct.Find(itemPSD.GeoStruct_Id);
                                    if (itemPSD.GeoStruct != null)
                                        itemPSD.GeoStruct.Location = db.Location.Find(itemPSD.GeoStruct.Location_Id);
                                    if (itemPSD.GeoStruct != null && itemPSD.GeoStruct.Location != null)
                                        itemPSD.GeoStruct.Location.LocationObj = db.LocationObj.Find(itemPSD.GeoStruct.Location.LocationObj_Id);
                                }


                                OPromotionServiceBookDetails.Add(OPromotionServiceBookDetails_t);
                            }
                        }
                        if (OPromotionServiceBookDetails == null || OPromotionServiceBookDetails.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPromotionServiceBookDetails)
                            {
                                if (ca.PromotionServiceBook != null && ca.PromotionServiceBook.Count() != 0)
                                {
                                    var OITTrans = ca.PromotionServiceBook.Where(e => mPayMonth.Contains(e.ProcessPromoDate.Value.ToString("MM/yyyy"))).ToList();
                                    //  var OITTrans = ca.IncrementServiceBook.Contains(ProcessI)
                                    if (OITTrans != null && OITTrans.Count() != 0)
                                    {
                                        foreach (var ca1 in OITTrans)
                                        {
                                            if (salheadlist.Count() > 0)
                                            {
                                                foreach (var profilteritem in salheadlist)
                                                {
                                                    if (ca1.PromotionActivity.Name.ToUpper() == profilteritem.ToUpper())
                                                    {
                                                        GenericField100 OGenericObjStatement = new GenericField100()
                                                        //write data to generic class
                                                        {
                                                            Fld1 = ca.Employee.Id.ToString(),
                                                            Fld2 = ca.Employee.EmpCode,
                                                            Fld3 = ca.Employee.EmpName.FullNameFML,
                                                            Fld4 = ca1.Id.ToString(),
                                                            Fld5 = ca1.ProcessPromoDate.Value.ToShortDateString(),
                                                            Fld6 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : "",
                                                            Fld7 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : "",
                                                            Fld8 = ca1.OldPayStruct != null && ca1.OldPayStruct.Grade != null ? ca1.OldPayStruct.Grade.Name : "",
                                                            Fld9 = ca1.NewPayStruct != null && ca1.NewPayStruct.Grade != null ? ca1.NewPayStruct.Grade.Name : "",
                                                            Fld10 = ca1.OldBasic.ToString(),
                                                            Fld11 = ca1.NewBasic.ToString(),
                                                            Fld12 = ca1.Fittment.ToString(),
                                                            Fld13 = ca1.ReleaseFlag.ToString(),
                                                            Fld14 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                            Fld15 = ca1.Narration == null ? "" : ca1.Narration,
                                                            Fld16 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocDesc : "",
                                                            Fld17 = ca1.PromotionActivity != null ? ca1.PromotionActivity.Name.ToUpper() : "",
                                                        };
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
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
                                                    Fld3 = ca.Employee.EmpName.FullNameFML,
                                                    Fld4 = ca1.Id.ToString(),
                                                    Fld5 = ca1.ProcessPromoDate.Value.ToShortDateString(),
                                                    Fld6 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : "",
                                                    Fld7 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : "",
                                                    Fld8 = ca1.OldPayStruct != null && ca1.OldPayStruct.Grade != null ? ca1.OldPayStruct.Grade.Name : "",
                                                    Fld9 = ca1.NewPayStruct != null && ca1.NewPayStruct.Grade != null ? ca1.NewPayStruct.Grade.Name : "",
                                                    Fld10 = ca1.OldBasic.ToString(),
                                                    Fld11 = ca1.NewBasic.ToString(),
                                                    Fld12 = ca1.Fittment.ToString(),
                                                    Fld13 = ca1.ReleaseFlag.ToString(),
                                                    Fld14 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                    Fld15 = ca1.Narration == null ? "" : ca1.Narration,
                                                    Fld16 = ca1.GeoStruct != null && ca1.GeoStruct.Location != null && ca1.GeoStruct.Location.LocationObj != null ? ca1.GeoStruct.Location.LocationObj.LocDesc : "",
                                                    Fld17 = ca1.PromotionActivity != null ? ca1.PromotionActivity.Name.ToUpper() : "",
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


                    case "TRANSFERSERVICEBOOK":

                        var OTransferServiceBookData = new List<EmployeePayroll>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    var OTransferServiceBookData_t = db.EmployeePayroll
                        //        .Include(e => e.TransferServiceBook)
                        //    .Include(e => e.Employee)
                        //    .Include(e => e.Employee.EmpName)
                        //    .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //   .Include(e => e.Employee.GeoStruct.Division)
                        //   .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        //   .Include(e => e.Employee.GeoStruct.Unit)
                        //   .Include(e => e.Employee.FuncStruct.Job)
                        //   .Include(e => e.Employee.FuncStruct.Job.JobPosition)
                        //   .Include(e => e.Employee.GeoStruct.Group)
                        //   .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        //    .Include(e => e.Employee.ServiceBookDates)
                        //    .Include(r => r.TransferServiceBook.Select(t => t.TransActivity))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.TransActivity.TransList))

                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldFuncStruct.Job))
                        //     .Include(r => r.TransferServiceBook.Select(t => t.NewFuncStruct.Job))

                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.NewFuncStruct.JobPosition))

                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Division))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Location.LocationObj))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Department.DepartmentObj))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Group))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Unit))

                        //    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Division))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Location.LocationObj))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Department.DepartmentObj))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Group))
                        //    .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Unit))

                        //  .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                        //    .FirstOrDefault();

                        //    if (OTransferServiceBookData_t != null)
                        //    {
                        //        OTransferServiceBookData.Add(OTransferServiceBookData_t);
                        //    }
                        //}
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OTransferServiceBookData_t = db.EmployeePayroll
                                //.Include(e => e.TransferServiceBook)
                              .Include(e => e.Employee)
                                // .Include(e => e.Employee.EmpName)
                                // .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Division)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.GeoStruct.Unit)
                                //.Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.Employee.FuncStruct.Job.JobPosition)
                                //.Include(e => e.Employee.GeoStruct.Group)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                // .Include(e => e.Employee.ServiceBookDates)
                                // .Include(r => r.TransferServiceBook.Select(t => t.TransActivity))
                                // .Include(r => r.TransferServiceBook.Select(t => t.TransActivity.TransList))

                             // .Include(r => r.TransferServiceBook.Select(t => t.OldFuncStruct.Job))
                                //  .Include(r => r.TransferServiceBook.Select(t => t.NewFuncStruct.Job))

                             // .Include(r => r.TransferServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                                // .Include(r => r.TransferServiceBook.Select(t => t.NewFuncStruct.JobPosition))

                             // .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Division))
                                // .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Location.LocationObj))
                                // .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Department.DepartmentObj))
                                // .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Group))
                                // .Include(r => r.TransferServiceBook.Select(t => t.OldGeoStruct.Unit))

                             // .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Division))
                                // .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Location.LocationObj))
                                // .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Department.DepartmentObj))
                                // .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Group))
                                // .Include(r => r.TransferServiceBook.Select(t => t.NewGeoStruct.Unit));
                            .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            // var OTransferServiceBookData = OTransferServiceBookData_t.Where(e => EmpPayrollIdList.Contains(e.Employee.Id)).AsNoTracking().ToList();

                            if (OTransferServiceBookData_t != null)
                            {
                                OTransferServiceBookData_t.TransferServiceBook = db.TransferServiceBook.Where(e => e.EmployeePayroll_Id == OTransferServiceBookData_t.Id).AsNoTracking().ToList();
                                OTransferServiceBookData_t.Employee.EmpName = db.NameSingle.Find(OTransferServiceBookData_t.Employee.EmpName_Id);

                                OTransferServiceBookData_t.Employee.GeoStruct = db.GeoStruct.Find(OTransferServiceBookData_t.Employee.GeoStruct_Id);
                                OTransferServiceBookData_t.Employee.GeoStruct.Location = db.Location.Find(OTransferServiceBookData_t.Employee.GeoStruct.Location_Id);
                                if (OTransferServiceBookData_t.Employee.GeoStruct != null && OTransferServiceBookData_t.Employee.GeoStruct.Location != null)
                                    OTransferServiceBookData_t.Employee.GeoStruct.Location.LocationObj = db.LocationObj.Find(OTransferServiceBookData_t.Employee.GeoStruct.Location.LocationObj_Id);
                                if (OTransferServiceBookData_t.Employee.GeoStruct != null)
                                    OTransferServiceBookData_t.Employee.GeoStruct.Division = db.Division.Find(OTransferServiceBookData_t.Employee.GeoStruct.Division_Id);
                                if (OTransferServiceBookData_t.Employee.GeoStruct != null && OTransferServiceBookData_t.Employee.GeoStruct.Department != null)
                                    OTransferServiceBookData_t.Employee.GeoStruct.Department.DepartmentObj = db.DepartmentObj.Find(OTransferServiceBookData_t.Employee.GeoStruct.Department.DepartmentObj_Id);
                                if (OTransferServiceBookData_t.Employee.GeoStruct != null)
                                    OTransferServiceBookData_t.Employee.GeoStruct.Unit = db.Unit.Find(OTransferServiceBookData_t.Employee.GeoStruct.Unit_Id);

                                OTransferServiceBookData_t.Employee.FuncStruct = db.FuncStruct.Find(OTransferServiceBookData_t.Employee.FuncStruct_Id);
                                if (OTransferServiceBookData_t.Employee.FuncStruct != null)
                                    OTransferServiceBookData_t.Employee.FuncStruct.Job = db.Job.Find(OTransferServiceBookData_t.Employee.FuncStruct.Job_Id);
                                OTransferServiceBookData_t.Employee.FuncStruct.Job.JobPosition = db.Job.Where(e => e.Id == OTransferServiceBookData_t.Employee.FuncStruct.Job_Id).Select(a => a.JobPosition).AsNoTracking().ToList().FirstOrDefault();

                                if (OTransferServiceBookData_t.Employee.GeoStruct != null)
                                    OTransferServiceBookData_t.Employee.GeoStruct.Group = db.Group.Find(OTransferServiceBookData_t.Employee.GeoStruct.Group_Id);
                                //OTransferServiceBookData_t.Employee.GeoStruct.Department.DepartmentObj = db.DepartmentObj.Find(OTransferServiceBookData_t.Employee.GeoStruct.Department.DepartmentObj_Id);
                                OTransferServiceBookData_t.Employee.ServiceBookDates = db.ServiceBookDates.Find(OTransferServiceBookData_t.Employee.ServiceBookDates_Id);
                                foreach (var itemTran in OTransferServiceBookData_t.TransferServiceBook)
                                {
                                    itemTran.TransActivity = db.TransActivity.Find(itemTran.TransActivity_Id);
                                    itemTran.TransActivity.TransList = db.LookupValue.Find(itemTran.TransActivity.TransList_Id);
                                    itemTran.OldFuncStruct = db.FuncStruct.Find(itemTran.OldFuncStruct_Id);
                                    itemTran.OldFuncStruct.Job = db.Job.Find(itemTran.OldFuncStruct.Job_Id);
                                    itemTran.NewFuncStruct = db.FuncStruct.Find(itemTran.NewFuncStruct_Id);
                                    itemTran.NewFuncStruct.Job = db.Job.Find(itemTran.NewFuncStruct.Job_Id);

                                    if (itemTran.OldFuncStruct != null)
                                        itemTran.OldFuncStruct.JobPosition = db.JobPosition.Find(itemTran.OldFuncStruct.JobPosition_Id);
                                    if (itemTran.NewFuncStruct != null)
                                        itemTran.NewFuncStruct.JobPosition = db.JobPosition.Find(itemTran.NewFuncStruct.JobPosition_Id);

                                    itemTran.OldGeoStruct = db.GeoStruct.Find(itemTran.OldGeoStruct_Id);
                                    if (itemTran.OldGeoStruct != null)
                                        itemTran.OldGeoStruct.Division = db.Division.Find(itemTran.OldGeoStruct.Division_Id);
                                    itemTran.OldGeoStruct.Location = db.Location.Find(itemTran.OldGeoStruct.Location_Id);
                                    if (itemTran.OldGeoStruct != null && itemTran.OldGeoStruct.Location != null)
                                        itemTran.OldGeoStruct.Location.LocationObj = db.LocationObj.Find(itemTran.OldGeoStruct.Location.LocationObj_Id);
                                    itemTran.OldGeoStruct.Department = db.Department.Find(itemTran.OldGeoStruct.Department_Id);
                                    if (itemTran.OldGeoStruct != null && itemTran.OldGeoStruct.Department != null)
                                        itemTran.OldGeoStruct.Department.DepartmentObj = db.DepartmentObj.Find(itemTran.OldGeoStruct.Department.DepartmentObj_Id);
                                    if (itemTran.OldGeoStruct != null)
                                        itemTran.OldGeoStruct.Group = db.Group.Find(itemTran.OldGeoStruct.Group_Id);
                                    if (itemTran.OldGeoStruct != null)
                                        itemTran.OldGeoStruct.Unit = db.Unit.Find(itemTran.OldGeoStruct.Unit_Id);

                                    itemTran.NewGeoStruct = db.GeoStruct.Find(itemTran.NewGeoStruct_Id);
                                    if (itemTran.NewGeoStruct != null)
                                        itemTran.NewGeoStruct.Division = db.Division.Find(itemTran.NewGeoStruct.Division_Id);
                                    itemTran.NewGeoStruct.Location = db.Location.Find(itemTran.NewGeoStruct.Location_Id);
                                    if (itemTran.NewGeoStruct != null && itemTran.NewGeoStruct.Location != null)
                                        itemTran.NewGeoStruct.Location.LocationObj = db.LocationObj.Find(itemTran.NewGeoStruct.Location.LocationObj_Id);
                                    itemTran.NewGeoStruct.Department = db.Department.Find(itemTran.NewGeoStruct.Department_Id);
                                    if (itemTran.NewGeoStruct != null && itemTran.NewGeoStruct.Department != null)
                                        itemTran.NewGeoStruct.Department.DepartmentObj = db.DepartmentObj.Find(itemTran.NewGeoStruct.Department.DepartmentObj_Id);
                                    if (itemTran.NewGeoStruct != null)
                                        itemTran.NewGeoStruct.Group = db.Group.Find(itemTran.NewGeoStruct.Group_Id);
                                    if (itemTran.NewGeoStruct != null)
                                        itemTran.NewGeoStruct.Unit = db.Unit.Find(itemTran.NewGeoStruct.Unit_Id);

                                }
                                OTransferServiceBookData.Add(OTransferServiceBookData_t);
                            }
                        }

                        if (OTransferServiceBookData == null || OTransferServiceBookData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OTransferServiceBookData)
                            {
                                var Sqgroup = SpecialGroupslist.ToList();
                                if (Sqgroup != null && Sqgroup.Count() > 0)
                                {
                                    foreach (var item in Sqgroup)
                                    {
                                        if (item == "Not Transfer Between")
                                        {
                                            if (ca.Employee.ServiceBookDates.LastTransferDate < pFromDate)
                                            {
                                                var da = ca.Employee.ServiceBookDates.LastTransferDate.Value.ToString("MM/yyyy");
                                                //if (ca.TransferServiceBook != null && ca.TransferServiceBook.Count() != 0)
                                                //{
                                                var ca1 = ca.TransferServiceBook.LastOrDefault();
                                                if (ca1 != null)
                                                {

                                                    //if (OITTrans != null && OITTrans.Count() != 0)
                                                    //{
                                                    //foreach (var ca1 in OITTrans)
                                                    //{
                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.Id.ToString(),
                                                        Fld2 = ca.Employee.EmpCode,
                                                        Fld3 = ca.Employee.EmpName.FullNameFML,
                                                        Fld4 = ca1.Id.ToString(),
                                                        Fld5 = ca1.TransActivity == null ? "" : ca1.TransActivity.Name,
                                                        Fld6 = ca1.TransActivity == null ? "" : ca1.TransActivity.TransList.LookupVal.ToUpper(),
                                                        Fld7 = ca1.ProcessTransDate == null ? "" : ca1.ProcessTransDate.Value.ToShortDateString(),

                                                        Fld8 = ca1.ReleaseFlag.ToString(),
                                                        Fld9 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),

                                                        Fld10 = ca1.Narration,


                                                        Fld11 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Division != null ? ca1.OldGeoStruct.Division.Name : "",
                                                        Fld12 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Division != null ? ca1.NewGeoStruct.Division.Name : "",
                                                        Fld13 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Location != null && ca1.OldGeoStruct.Location.LocationObj != null ? ca1.OldGeoStruct.Location.LocationObj.LocDesc : "",
                                                        Fld14 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Location != null && ca1.NewGeoStruct.Location.LocationObj != null ? ca1.NewGeoStruct.Location.LocationObj.LocDesc : "",
                                                        Fld15 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Department != null && ca1.OldGeoStruct.Department.DepartmentObj != null ? ca1.OldGeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                        Fld16 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Department != null && ca1.NewGeoStruct.Department.DepartmentObj != null ? ca1.NewGeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                        Fld17 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Group != null ? ca1.OldGeoStruct.Group.Name : "",
                                                        Fld18 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Group != null ? ca1.NewGeoStruct.Group.Name : "",
                                                        Fld19 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Unit != null ? ca1.OldGeoStruct.Unit.Name : "",
                                                        Fld20 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Unit != null ? ca1.NewGeoStruct.Unit.Name : "",

                                                        Fld23 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : "",
                                                        Fld24 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : "",

                                                        Fld25 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.JobPosition != null ? ca1.OldFuncStruct.JobPosition.JobPositionDesc : "",
                                                        Fld26 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.JobPosition != null ? ca1.NewFuncStruct.JobPosition.JobPositionDesc : "",
                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                                // }
                                                // }
                                            }
                                        }
                                        else if (item == "Not Transfer")
                                        {
                                            if (ca.Employee.ServiceBookDates.LastTransferDate == null)
                                            {
                                                GenericField100 OGenericObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = ca.Employee.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode,
                                                    Fld3 = ca.Employee.EmpName.FullNameFML,



                                                    //  Fld11 = ca1.OldGeoStruct.Division == null ? "" : ca1.OldGeoStruct.Division.Name,
                                                    Fld12 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Division != null ? ca.Employee.GeoStruct.Division.Name : "",
                                                    // Fld13 = ca1.OldGeoStruct.Location == null ? "" : ca1.OldGeoStruct.Location.LocationObj.LocDesc,
                                                    Fld14 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                    // Fld15 = ca1.OldGeoStruct.Department == null ? "" : ca1.OldGeoStruct.Department.DepartmentObj.DeptDesc,
                                                    Fld16 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                    //  Fld17 = ca1.OldGeoStruct.Group == null ? "" : ca1.OldGeoStruct.Group.Name,
                                                    Fld18 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Group != null ? ca.Employee.GeoStruct.Group.Name : "",
                                                    //  Fld19 = ca1.OldGeoStruct.Unit == null ? "" : ca1.OldGeoStruct.Unit.Name,
                                                    // Fld20 = ca1.NewGeoStruct.Unit == null ? "" : ca1.NewGeoStruct.Unit.Name,

                                                    // Fld23 = ca1.OldFuncStruct.Job == null ? "" : ca1.OldFuncStruct.Job.Name,
                                                    Fld24 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : "",

                                                    // Fld25 = ca1.OldFuncStruct.JobPosition == null ? "" : ca1.OldFuncStruct.JobPosition.JobPositionDesc,
                                                    Fld26 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.JobPosition != null ? ca.Employee.FuncStruct.JobPosition.JobPositionDesc : "",
                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                        else if (item == "Last Transfer Date")
                                        {
                                            if (ca.Employee.ServiceBookDates.LastTransferDate != null)
                                            {

                                                if (ca.Employee.ServiceBookDates.LastTransferDate >= pFromDate && ca.Employee.ServiceBookDates.LastTransferDate <= pToDate)
                                                {
                                                    DateTime Now = DateTime.Now;
                                                    DateTime Dob = Convert.ToDateTime(ca.Employee.ServiceBookDates.LastTransferDate);
                                                    int _Years = new DateTime(DateTime.Now.Subtract(Dob).Ticks).Year - 1;
                                                    DateTime _DOBDateNow = Dob.AddYears(_Years);
                                                    int _Months = 0;
                                                    for (int i = 1; i <= 12; i++)
                                                    {
                                                        if (_DOBDateNow.AddMonths(i) == Now)
                                                        {
                                                            _Months = i;
                                                            break;
                                                        }
                                                        else if (_DOBDateNow.AddMonths(i) >= Now)
                                                        {
                                                            _Months = i - 1;
                                                            break;
                                                        }
                                                    }
                                                    int Days = Now.Subtract(_DOBDateNow.AddMonths(_Months)).Days;

                                                    //  return $"Age is {_Years} Years {_Months} Months {Days} Days";    
                                                    string year = _Years.ToString() + " Years" + " " + _Months + " Month" + " " + Days + " Days";


                                                    GenericField100 OGenericObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        // Fld1 = ca.Employee.Id.ToString(),
                                                        Fld1 = ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML,
                                                        Fld3 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                        Fld4 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocCode : "",
                                                        Fld5 = ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null ? ca.Employee.FuncStruct.Job.Name : "",
                                                        Fld6 = ca.Employee.ServiceBookDates.LastTransferDate.Value.ToShortDateString(),
                                                        Fld27 = ca.Employee.GeoStruct!=null && ca.Employee.GeoStruct.Department!=null && ca.Employee.GeoStruct.Department.DepartmentObj!=null ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc: "",
                                                        Fld7 = year,
                                                        Fld37 = "Last Transfer Date"
                                                    };
                                                    OGenericPayrollStatement.Add(OGenericObjStatement);
                                                }
                                            }
                                        }
                                        //}
                                    }
                                }
                                else
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
                                                    Fld7 = ca1.ProcessTransDate == null ? "" : ca1.ProcessTransDate.Value.ToShortDateString(),

                                                    Fld8 = ca1.ReleaseFlag.ToString(),
                                                    Fld9 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),

                                                    Fld10 = ca1.Narration == null ? "" : ca1.Narration,


                                                    Fld11 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Division != null ? ca1.OldGeoStruct.Division.Name : "",
                                                    Fld12 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Division != null ? ca1.NewGeoStruct.Division.Name : "",
                                                    Fld13 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Location != null && ca1.OldGeoStruct.Location.LocationObj != null ? ca1.OldGeoStruct.Location.LocationObj.LocDesc : "",
                                                    Fld14 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Location != null && ca1.NewGeoStruct.Location.LocationObj != null ? ca1.NewGeoStruct.Location.LocationObj.LocDesc : "",
                                                    Fld15 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Department != null && ca1.OldGeoStruct.Department.DepartmentObj != null ? ca1.OldGeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                    Fld16 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Department != null && ca1.NewGeoStruct.Department.DepartmentObj != null ? ca1.NewGeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                    Fld17 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Group != null ? ca1.OldGeoStruct.Group.Name : "",
                                                    Fld18 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Group != null ? ca1.NewGeoStruct.Group.Name : "",
                                                    Fld19 = ca1.OldGeoStruct != null && ca1.OldGeoStruct.Unit != null ? ca1.OldGeoStruct.Unit.Name : "",
                                                    Fld20 = ca1.NewGeoStruct != null && ca1.NewGeoStruct.Unit != null ? ca1.NewGeoStruct.Unit.Name : "",

                                                    Fld23 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : "",
                                                    Fld24 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : "",

                                                    Fld25 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.JobPosition != null ? ca1.OldFuncStruct.JobPosition.JobPositionDesc : "",
                                                    Fld26 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.JobPosition != null ? ca1.NewFuncStruct.JobPosition.JobPositionDesc : "",
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

                                //}
                                //else
                                //{
                                //    return null;
                                //}

                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "OTHERSERVICEBOOK":
                        var OOtherServiceBookData = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OOtherServiceBookData_t = db.EmployeePayroll
                                //.Include(e => e.OtherServiceBook)
                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpCode)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.OtherServiceBook.Select(t => t.OthServiceBookActivity))
                                //.Include(r => r.OtherServiceBook.Select(t => t.OthServiceBookActivity.OtherSerBookActList))

                            //.Include(r => r.OtherServiceBook.Select(t => t.OldFuncStruct.Job))
                                //.Include(r => r.OtherServiceBook.Select(t => t.NewFuncStruct.Job))

                            //.Include(r => r.OtherServiceBook.Select(t => t.OldFuncStruct.JobPosition))
                                //.Include(r => r.OtherServiceBook.Select(t => t.NewFuncStruct.JobPosition))

                            //.Include(r => r.OtherServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpStatus))
                                //.Include(r => r.OtherServiceBook.Select(t => t.OldPayStruct.JobStatus.EmpActingStatus))

                            //.Include(r => r.OtherServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpStatus))
                                //.Include(r => r.OtherServiceBook.Select(t => t.NewPayStruct.JobStatus.EmpActingStatus)).AsNoTracking()
                           .Where(e => e.Employee.Id == item).AsNoTracking().AsParallel()
                            .FirstOrDefault();


                            if (OOtherServiceBookData_t != null)
                            {
                                OOtherServiceBookData_t.OtherServiceBook = db.OtherServiceBook.Where(e => e.EmployeePayroll_Id == OOtherServiceBookData_t.Id).AsNoTracking().ToList();
                                OOtherServiceBookData_t.Employee.EmpName = db.NameSingle.Find(OOtherServiceBookData_t.Employee.EmpName_Id);
                                foreach (var itemOthr in OOtherServiceBookData_t.OtherServiceBook)
                                {
                                    itemOthr.OthServiceBookActivity = db.OthServiceBookActivity.Find(itemOthr.OthServiceBookActivity_Id);
                                    if (itemOthr.OthServiceBookActivity != null)
                                        itemOthr.OthServiceBookActivity.OtherSerBookActList = db.LookupValue.Find(itemOthr.OthServiceBookActivity.OtherSerBookActList_Id);
                                    itemOthr.OldFuncStruct = db.FuncStruct.Find(itemOthr.OldFuncStruct_Id);
                                    if (itemOthr.OldFuncStruct != null)
                                        itemOthr.OldFuncStruct.Job = db.Job.Find(itemOthr.OldFuncStruct.Job_Id);
                                    itemOthr.NewFuncStruct = db.FuncStruct.Find(itemOthr.NewFuncStruct_Id);
                                    if (itemOthr.NewFuncStruct != null)
                                        itemOthr.NewFuncStruct.Job = db.Job.Find(itemOthr.NewFuncStruct.Job_Id);

                                    if (itemOthr.OldFuncStruct != null)
                                        itemOthr.OldFuncStruct.JobPosition = db.JobPosition.Find(itemOthr.OldFuncStruct.JobPosition_Id);
                                    if (itemOthr.NewFuncStruct != null)
                                        itemOthr.NewFuncStruct.JobPosition = db.JobPosition.Find(itemOthr.NewFuncStruct.JobPosition_Id);

                                    itemOthr.OldPayStruct = db.PayStruct.Find(itemOthr.OldPayStruct_Id);
                                    if (itemOthr.OldPayStruct != null)
                                        itemOthr.OldPayStruct.JobStatus = db.JobStatus.Find(itemOthr.OldPayStruct.JobStatus_Id);
                                    if (itemOthr.OldPayStruct != null && itemOthr.OldPayStruct.JobStatus != null)
                                        itemOthr.OldPayStruct.JobStatus.EmpStatus = db.LookupValue.Find(itemOthr.OldPayStruct.JobStatus.EmpStatus_Id);
                                    if (itemOthr.OldPayStruct != null && itemOthr.OldPayStruct.JobStatus != null)
                                        itemOthr.OldPayStruct.JobStatus.EmpActingStatus = db.LookupValue.Find(itemOthr.OldPayStruct.JobStatus.EmpActingStatus_Id);

                                    itemOthr.NewPayStruct = db.PayStruct.Find(itemOthr.NewPayStruct_Id);
                                    if (itemOthr.NewPayStruct != null)
                                        itemOthr.NewPayStruct.JobStatus = db.JobStatus.Find(itemOthr.NewPayStruct.JobStatus_Id);
                                    if (itemOthr.NewPayStruct != null && itemOthr.NewPayStruct.JobStatus != null)
                                        itemOthr.NewPayStruct.JobStatus.EmpStatus = db.LookupValue.Find(itemOthr.NewPayStruct.JobStatus.EmpStatus_Id);
                                    if (itemOthr.NewPayStruct != null && itemOthr.NewPayStruct.JobStatus != null)
                                        itemOthr.NewPayStruct.JobStatus.EmpActingStatus = db.LookupValue.Find(itemOthr.NewPayStruct.JobStatus.EmpActingStatus_Id);
                                }


                                OOtherServiceBookData.Add(OOtherServiceBookData_t);
                            }
                        }
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
                                    //if (oth_idlist == null)
                                    //{
                                    if (OITTrans != null && OITTrans.Count() != 0)
                                    {

                                        foreach (var ca1 in OITTrans)
                                        {
                                            if (salheadlist.Count() > 0)
                                            {
                                                foreach (var othActlistitem in salheadlist)
                                                {
                                                    if (ca1.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == othActlistitem.ToUpper())
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
                                                            Fld10 = ca1.Narration != null ? ca1.Narration : null,
                                                            Fld11 = ca1.IsBasicChangeAppl.ToString(),
                                                            Fld12 = ca1.NewBasic.ToString(),

                                                            Fld13 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : null,
                                                            Fld14 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : null,

                                                            Fld15 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.JobPosition != null ? ca1.OldFuncStruct.JobPosition.JobPositionDesc : null,
                                                            Fld16 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.JobPosition != null ? ca1.NewFuncStruct.JobPosition.JobPositionDesc : null,

                                                            Fld17 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpStatus != null ? ca1.OldPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                                            Fld18 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpActingStatus != null ? ca1.OldPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,
                                                            Fld19 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpStatus != null ? ca1.NewPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                                            Fld20 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpActingStatus != null ? ca1.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,


                                                        };
                                                        OGenericPayrollStatement.Add(OGenericObjStatement);
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
                                                    Fld3 = ca.Employee.EmpName != null ? ca.Employee.EmpName.FullNameFML : null,
                                                    Fld4 = ca1.Id.ToString(),
                                                    Fld5 = ca1.OthServiceBookActivity != null ? ca1.OthServiceBookActivity.Name : null,
                                                    Fld6 = ca1.OthServiceBookActivity != null && ca1.OthServiceBookActivity.OtherSerBookActList != null ? ca1.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() : null,
                                                    Fld7 = ca1.ProcessOthDate != null ? ca1.ProcessOthDate.Value.ToShortDateString() : null,
                                                    Fld8 = ca1.ReleaseFlag.ToString(),
                                                    Fld9 = ca1.ReleaseDate != null ? ca1.ReleaseDate.Value.ToShortDateString() : null,
                                                    Fld10 = ca1.Narration != null ? ca1.Narration : null,
                                                    Fld11 = ca1.IsBasicChangeAppl.ToString(),
                                                    Fld12 = ca1.NewBasic.ToString(),

                                                    Fld13 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.Job != null ? ca1.OldFuncStruct.Job.Name : null,
                                                    Fld14 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.Job != null ? ca1.NewFuncStruct.Job.Name : null,

                                                    Fld15 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.JobPosition != null ? ca1.OldFuncStruct.JobPosition.JobPositionDesc : null,
                                                    Fld16 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.JobPosition != null ? ca1.NewFuncStruct.JobPosition.JobPositionDesc : null,

                                                    Fld17 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpStatus != null ? ca1.OldPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                                    Fld18 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpActingStatus != null ? ca1.OldPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,
                                                    Fld19 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpStatus != null ? ca1.NewPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                                    Fld20 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpActingStatus != null ? ca1.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,


                                                };
                                                OGenericPayrollStatement.Add(OGenericObjStatement);
                                            }
                                        }
                                        //   }
                                    }
                                    //  if (oth_id.ToString() != null)
                                    else
                                    {
                                        foreach (var item in oth_idlist)
                                        {
                                            var ottt1 = OITTrans.Where(a => a.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToString() == item.ToString()).ToList();
                                            if (ottt1 != null && ottt1.Count() != 0)
                                            {
                                                foreach (var ca1 in ottt1)
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

                                                        Fld15 = ca1.OldFuncStruct != null && ca1.OldFuncStruct.JobPosition != null ? ca1.OldFuncStruct.JobPosition.JobPositionDesc : null,
                                                        Fld16 = ca1.NewFuncStruct != null && ca1.NewFuncStruct.JobPosition != null ? ca1.NewFuncStruct.JobPosition.JobPositionDesc : null,

                                                        Fld17 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpStatus != null ? ca1.OldPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                                        Fld18 = ca1.OldPayStruct != null && ca1.OldPayStruct.JobStatus != null && ca1.OldPayStruct.JobStatus.EmpActingStatus != null ? ca1.OldPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,
                                                        Fld19 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpStatus != null ? ca1.NewPayStruct.JobStatus.EmpStatus.LookupVal.ToUpper() : null,
                                                        Fld20 = ca1.NewPayStruct != null && ca1.NewPayStruct.JobStatus != null && ca1.NewPayStruct.JobStatus.EmpActingStatus != null ? ca1.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() : null,


                                                    };
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

                    case "EXTNREDNSERVICEBOOKDETAILS":
                        var OExtnRednServiceBookDetails = new List<EmployeePayroll>();
                        foreach (var item in EmpPayrollIdList)
                        {
                            var OExtnRednServiceBookDetails_t = db.EmployeePayroll
                                //.Include(e => e.ExtnRednServiceBook)
                            .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(r => r.ExtnRednServiceBook.Select(t => t.ExtnRednActivity))
                                //.Include(r => r.ExtnRednServiceBook.Select(t => t.ExtnRednActivity.ExtnRednList))
                                //.Include(r => r.ExtnRednServiceBook.Select(t => t.Frequency))
                            .Where(e => e.Employee.Id == item).AsNoTracking()
                            .FirstOrDefault();

                            if (OExtnRednServiceBookDetails_t != null)
                            {
                                OExtnRednServiceBookDetails_t.ExtnRednServiceBook = db.ExtnRednServiceBook.Where(e => e.EmployeePayroll_Id == OExtnRednServiceBookDetails_t.Id).AsNoTracking().ToList();
                                OExtnRednServiceBookDetails_t.Employee.EmpName = db.NameSingle.Find(OExtnRednServiceBookDetails_t.Employee.EmpName_Id);
                                foreach (var itemEXTN in OExtnRednServiceBookDetails_t.ExtnRednServiceBook)
                                {
                                    itemEXTN.ExtnRednActivity = db.ExtnRednActivity.Find(itemEXTN.ExtnRednActivity_Id);
                                    if (itemEXTN.ExtnRednActivity != null)
                                        itemEXTN.ExtnRednActivity.ExtnRednList = db.LookupValue.Find(itemEXTN.ExtnRednActivity.ExtnRednList_Id);
                                    itemEXTN.Frequency = db.LookupValue.Find(itemEXTN.Frequency_Id);
                                }

                                OExtnRednServiceBookDetails.Add(OExtnRednServiceBookDetails_t);
                            }
                        }
                        if (OExtnRednServiceBookDetails == null || OExtnRednServiceBookDetails.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OExtnRednServiceBookDetails)
                            {
                                if (ca.ExtnRednServiceBook != null && ca.ExtnRednServiceBook.Count() != 0)
                                {
                                    var OITTrans = ca.ExtnRednServiceBook.Where(e => mPayMonth.Contains(e.ProcessDate.Value.ToString("MM/yyyy"))).ToList();
                                    //  var OITTrans = ca.IncrementServiceBook.Contains(ProcessI)
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
                                                Fld5 = ca1.ExtnRednActivity == null ? "" : ca1.ExtnRednActivity.Name,
                                                Fld6 = ca1.ExtnRednActivity == null ? "" : ca1.ExtnRednActivity.ExtnRednList.LookupVal.ToUpper(),
                                                Fld7 = ca1.ProcessDate.Value.ToShortDateString(),
                                                Fld8 = ca1.ReleaseFlag.ToString(),
                                                Fld9 = ca1.ReleaseDate == null ? "" : ca1.ReleaseDate.Value.ToShortDateString(),
                                                Fld10 = ca1.Narration == null ? "" : ca1.Narration,
                                                Fld11 = ca1.ExtnRednCount == null ? "" : ca1.ExtnRednCount.ToString(),
                                                Fld12 = ca1.Period == null ? "" : ca1.Period.ToString(),
                                                Fld13 = ca1.Frequency == null ? "" : ca1.Frequency.LookupVal.ToString(),
                                            };
                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }
                                    }

                                }

                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "HOLDINCREMENTRELEASE":

                        var Holereleid = db.IncrementHoldReleaseDetails.Include(e => e.IncrementServiceBook).ToList().Select(x => x.IncrementServiceBook_Id);
                        var OINCREMENTHOLDData = db.EmployeePayroll
                            .Where(t => t.IncrementServiceBook.Count() > 0 && EmpPayrollIdList.Contains(t.Employee.Id)).Select(e => new
                        {
                            //IncrementServiceBook = e.IncrementServiceBook.Where(s => s.IsHold == true).ToList(),
                            IncrementServiceBook = e.IncrementServiceBook.Where(s => Holereleid.Contains(s.Id)).ToList(),
                            Id = e.Id,
                            EmpCode = e.Employee.EmpCode,
                            EmpName = e.Employee.EmpName.FullNameFML
                        }).ToList();


                        if (OINCREMENTHOLDData == null || OINCREMENTHOLDData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {

                            var IncrServBookids = OINCREMENTHOLDData.Where(e => e.IncrementServiceBook.Count() > 0).ToList();
                            //List<int> IncrServBookidsdata = IncrServBookids.Select(e => e.Id).ToList();
                            var Sqgroup = SpecialGroupslist.FirstOrDefault();

                            foreach (var ca in IncrServBookids)
                            {
                                foreach (var item1 in ca.IncrementServiceBook)
                                {


                                    if (Sqgroup.ToUpper() == "RELEASE")
                                    {
                                        var incrementholdreleasedetails = db.IncrementHoldReleaseDetails.Include(e => e.IncrementServiceBook).Where(e => e.IncrementServiceBook.Id == item1.Id && item1.IsHold == false).FirstOrDefault();
                                        if (incrementholdreleasedetails != null && incrementholdreleasedetails.IncrementServiceBook.ReleaseDate >= pFromDate && incrementholdreleasedetails.IncrementServiceBook.ReleaseDate <= pToDate)
                                        {

                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {

                                                Fld2 = ca.EmpCode.ToString(),
                                                Fld3 = ca.EmpName.ToString(),
                                                Fld4 = incrementholdreleasedetails.HoldDate.Value.ToShortDateString(),
                                                Fld5 = incrementholdreleasedetails.HoldNarration == null ? "" : incrementholdreleasedetails.HoldNarration.ToString(),
                                                Fld6 = incrementholdreleasedetails.IncrementServiceBook.ReleaseDate.Value.ToShortDateString(),
                                                //Fld7 = incrementholdreleasedetails.ReleaseNarration.ToString(),

                                            };

                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }
                                    }
                                    else
                                    {
                                        var incrementholdreleasedetails = db.IncrementHoldReleaseDetails.Include(e => e.IncrementServiceBook).Where(e => e.IncrementServiceBook.Id == item1.Id && item1.IsHold == true).FirstOrDefault();
                                        if (incrementholdreleasedetails != null)
                                        {
                                            GenericField100 OGenericObjStatement = new GenericField100()
                                            //write data to generic class
                                            {

                                                Fld2 = ca.EmpCode.ToString(),
                                                Fld3 = ca.EmpName.ToString(),
                                                Fld4 = incrementholdreleasedetails.HoldDate.Value.ToShortDateString(),
                                                Fld5 = incrementholdreleasedetails.HoldNarration == null ? "" : incrementholdreleasedetails.HoldNarration.ToString(),
                                                Fld6 = incrementholdreleasedetails.ReleaseDate.Value.ToShortDateString(),
                                                //Fld7 = incrementholdreleasedetails.ReleaseNarration.ToString(),

                                            };

                                            OGenericPayrollStatement.Add(OGenericObjStatement);
                                        }
                                    }
                                }
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
    }
}