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
    public class ObjectReportGen
    {
        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };
        public static List<GenericField100> GenerateObjectReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist, DateTime mFromDate, DateTime mToDate)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {

                    case "CORPORATE":
                        var OGeoCORPData = db.Corporate
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.BusinessType).AsNoTracking().ToList();
                        //  .Where(d => d.Id == CompanyId).AsParallel().ToList();

                        if (OGeoCORPData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OGeoCORPData)
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

                    case "REGION":
                        var OGeoREGData = db.Region

                        .Include(e => e.Incharge)
                        .Include(e => e.Incharge.EmpName)
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails).AsNoTracking().AsParallel().ToList();

                        //.Where(d => d.Id == CompanyId).ToList();

                        if (OGeoREGData == null)
                        {
                            return null;
                        }
                        else
                        {
                            List<Region> regiondata = OGeoREGData.ToList();

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


                    case "COMPANY":
                        var OGeoComp = db.Company
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

                        if (OGeoComp == null)
                        {
                            return null;
                        }
                        else
                        {
                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                            {
                                Fld1 = OGeoComp.Id.ToString(),
                                Fld2 = OGeoComp.Code,
                                Fld3 = OGeoComp.Name == null ? "" : OGeoComp.Name.ToString(),
                                Fld4 = OGeoComp.Address == null ? "" : OGeoComp.Address.FullAddress,
                                Fld5 = OGeoComp.ContactDetails == null ? "" : OGeoComp.ContactDetails.FullContactDetails,
                                Fld6 = OGeoComp.CINNo == null ? "" : OGeoComp.CINNo.ToString(),
                                Fld7 = OGeoComp.CSTNo == null ? "" : OGeoComp.CSTNo.ToString(),
                                Fld8 = OGeoComp.ESICNo == null ? "" : OGeoComp.ESICNo.ToString(),
                                Fld9 = OGeoComp.EstablishmentNo == null ? "" : OGeoComp.EstablishmentNo.ToString(),
                                Fld10 = OGeoComp.LBTNO == null ? "" : OGeoComp.LBTNO.ToString(),
                                Fld11 = OGeoComp.PANNo == null ? "" : OGeoComp.PANNo.ToString(),
                                Fld12 = OGeoComp.PTECNO == null ? "" : OGeoComp.PTECNO.ToString(),
                                Fld13 = OGeoComp.PTRCNO == null ? "" : OGeoComp.PTRCNO.ToString(),
                                Fld14 = OGeoComp.RegistrationNo == null ? "" : OGeoComp.RegistrationNo.ToString(),
                                Fld15 = OGeoComp.RegistrationDate == null ? "" : OGeoComp.RegistrationDate.ToString(),
                                Fld16 = OGeoComp.ServiceTaxNo == null ? "" : OGeoComp.ServiceTaxNo.ToString(),
                                Fld17 = OGeoComp.TANNo == null ? "" : OGeoComp.TANNo.ToString(),
                                Fld18 = OGeoComp.VATNo == null ? "" : OGeoComp.VATNo.ToString(),
                            };
                            OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                            return OGenericPayrollStatement;
                        }

                        break;

                    case "DIVISION":
                        var OGeoDivData = db.Division

                        .Include(r => r.Address)
                        .Include(r => r.ContactDetails)
                        .Include(r => r.Incharge)
                        .Include(r => r.Incharge.EmpName)
                        .Include(r => r.Incharge.CorContact)
                        .Include(r => r.Incharge.CorAddr)
                        .Include(r => r.Type).AsNoTracking().AsParallel().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoDivData == null || OGeoDivData.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OGeoDivData)
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

                    case "LOCATION":
                        var OGeoLocData = db.Location
                     .Include(e => e.Address)
                     .Include(e => e.ContactDetails)
                     .Include(e => e.BusinessCategory)
                     .Include(e => e.Incharge)
                     .Include(e => e.Incharge.EmpName)
                     .Include(e => e.Incharge.CorContact)
                     .Include(e => e.LocationObj)
                     .Include(e => e.Type).ToList();
                        //.Where(d => d.Id == CompanyId).SingleOrDefault();
                        if (OGeoLocData == null || OGeoLocData.Count == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OGeoLocData)
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


                    case "JVPARAMETER":

                        var SpGroup = SpecialGroupslist.ToList();

                        var check2 = false;

                        foreach (var item1 in SpGroup)
                        {
                            if (item1 == "NONSTANDARD")
                            {
                                check2 = true;
                            }
                        }
                        //// non standard
                        if (check2 == true)
                        {
                            var OJV = db.JVParameter
                                 .Include(e => e.JVGroup)
                                  .Include(e => e.JVNonStandardEmp)
                                   .Include(e => e.PaymentBank)
                                 .Include(e => e.SalaryHead).ToList();

                            if (OJV == null || OJV.Count == 0)
                                return null;
                            else
                            {
                                foreach (var ca in OJV)
                                {
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var ca2 in salheadlist)
                                        {
                                            var check = ca.JVGroup.LookupVal.ToUpper() == ca2.ToString() && ca.Irregular == true;
                                            if (check == true)
                                            {
                                                var test = "";
                                                if (ca.Irregular == true)
                                                {
                                                    test = "Non Standard";
                                                }

                                                var Sal = ca.SalaryHead.ToList();

                                                foreach (var item in Sal)
                                                {
                                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.Id.ToString(),
                                                        Fld2 = ca.JVGroup == null ? "" : ca.JVGroup.LookupVal.ToString(),
                                                        Fld3 = ca.JVProductCode == null ? "" : ca.JVProductCode.ToString(),
                                                        Fld4 = ca.JVName.ToString(),
                                                        Fld5 = ca.CreditDebitFlag == null ? "" : ca.CreditDebitFlag.ToString(),
                                                        Fld6 = test.ToString(),
                                                        Fld10 = ca.LocationIn == null ? "" : ca.LocationIn.ToString(),
                                                        Fld11 = ca.LocationOut == null ? "" : ca.LocationOut.ToString(),
                                                        Fld7 = item.FullDetails == null ? "" : item.FullDetails.ToString(), ///sal head
                                                        Fld8 = ca.AccountNo == null ? "" : ca.AccountNo.ToString(),
                                                        Fld9 = ca.SubAccountNo == null ? "" : ca.SubAccountNo.ToString(),
                                                        Fld13 = ca.PaymentBank == null ? "" : ca.PaymentBank.Name.ToString(),
                                                    };
                                                    //write data to generic class
                                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var check = ca.Irregular == true;
                                        if (check == true)
                                        {
                                            var test = "";
                                            if (ca.Irregular == true)
                                            {
                                                test = "Non Standard";
                                            }


                                            var Sal = ca.SalaryHead.ToList();

                                            foreach (var item in Sal)
                                            {
                                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Id.ToString(),
                                                    Fld2 = ca.JVGroup == null ? "" : ca.JVGroup.LookupVal.ToString(),
                                                    Fld3 = ca.JVProductCode == null ? "" : ca.JVProductCode.ToString(),
                                                    Fld4 = ca.JVName.ToString(),
                                                    Fld5 = ca.CreditDebitFlag == null ? "" : ca.CreditDebitFlag.ToString(),
                                                    Fld6 = test.ToString(),
                                                    Fld10 = ca.LocationIn == null ? "" : ca.LocationIn.ToString(),
                                                    Fld11 = ca.LocationOut == null ? "" : ca.LocationOut.ToString(),
                                                    Fld7 = item.FullDetails == null ? "" : item.FullDetails.ToString(), ///sal head
                                                    Fld8 = ca.AccountNo == null ? "" : ca.AccountNo.ToString(),
                                                    Fld9 = ca.SubAccountNo == null ? "" : ca.SubAccountNo.ToString(),
                                                    Fld13 = ca.PaymentBank == null ? "" : ca.PaymentBank.Name.ToString(),

                                                };
                                                //write data to generic class
                                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                            }
                                        }
                                    }
                                }

                                return OGenericPayrollStatement;
                            }
                        }

                            ///////standard
                        else
                        {

                            var OJV = db.JVParameter
                                 .Include(e => e.JVGroup)
                                  .Include(e => e.JVNonStandardEmp)
                                  .Include(e => e.PaymentBank)
                                 .Include(e => e.SalaryHead).ToList();

                            if (OJV == null || OJV.Count == 0)
                                return null;
                            else
                            {
                                foreach (var ca in OJV)
                                {
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var ca2 in salheadlist)
                                        {
                                            var check = ca.JVGroup.LookupVal.ToUpper() == ca2.ToString() && ca.Irregular == false;
                                            if (check == true)
                                            {
                                                var test = "";
                                                if (ca.Irregular == false)
                                                {
                                                    test = "Standard";
                                                }

                                                var Sal = ca.SalaryHead.ToList();

                                                foreach (var item in Sal)
                                                {
                                                    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                    {
                                                        Fld1 = ca.Id.ToString(),
                                                        Fld2 = ca.JVGroup == null ? "" : ca.JVGroup.LookupVal.ToString(),
                                                        Fld3 = ca.JVProductCode == null ? "" : ca.JVProductCode.ToString(),
                                                        Fld4 = ca.JVName.ToString(),
                                                        Fld5 = ca.CreditDebitFlag == null ? "" : ca.CreditDebitFlag.ToString(),
                                                        Fld6 = test.ToString(),
                                                        Fld7 = item.FullDetails == null ? "" : item.FullDetails.ToString(), ///sal head
                                                        Fld8 = ca.AccountNo == null ? "" : ca.AccountNo.ToString(),
                                                        Fld9 = ca.SubAccountNo == null ? "" : ca.SubAccountNo.ToString(),
                                                        Fld13 = ca.PaymentBank == null ? "" : ca.PaymentBank.Name.ToString(),

                                                    };
                                                    //write data to generic class
                                                    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var check = ca.Irregular == false;
                                        if (check == true)
                                        {
                                            var test = "";
                                            if (ca.Irregular == false)
                                            {
                                                test = "Standard";
                                            }

                                            var Sal = ca.SalaryHead.ToList();

                                            foreach (var item in Sal)
                                            {
                                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Id.ToString(),
                                                    Fld2 = ca.JVGroup == null ? "" : ca.JVGroup.LookupVal.ToString(),
                                                    Fld3 = ca.JVProductCode == null ? "" : ca.JVProductCode.ToString(),
                                                    Fld4 = ca.JVName.ToString(),
                                                    Fld5 = ca.CreditDebitFlag == null ? "" : ca.CreditDebitFlag.ToString(),
                                                    Fld6 = test.ToString(),
                                                    Fld7 = item.FullDetails == null ? "" : item.FullDetails.ToString(), ///sal head
                                                    Fld8 = ca.AccountNo == null ? "" : ca.AccountNo.ToString(),
                                                    Fld9 = ca.SubAccountNo == null ? "" : ca.SubAccountNo.ToString(),
                                                    Fld13 = ca.PaymentBank == null ? "" : ca.PaymentBank.Name.ToString(),

                                                };
                                                //write data to generic class
                                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                            }
                                        }
                                    }
                                }

                                return OGenericPayrollStatement;
                            }
                        }
                        break;


                    case "DEPARTMENT":
                        var OGeoDeptData = db.Department

                        .Include(r => r.Address)
                        .Include(r => r.ContactDetails.ContactNumbers)
                        .Include(r => r.Incharge)
                        .Include(r => r.Incharge.EmpName)
                        .Include(r => r.Type)
                        .Include(r => r.DepartmentObj)
                        .Include(r => r.Incharge.CorContact)
                        .Include(r => r.Incharge.CorAddr).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoDeptData == null || OGeoDeptData.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OGeoDeptData)
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
                        var OGeoGrpData = db.Group
                      .Include(e => e.ContactDetails)
                      .Include(e => e.Incharge)
                      .Include(e => e.Incharge.EmpName)
                      .Include(e => e.Incharge.CorAddr)
                      .Include(e => e.Incharge.CorContact)
                      .Include(e => e.Type).AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoGrpData == null || OGeoGrpData.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OGeoGrpData)
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


                    case "UNIT":
                        var OGeoUnitData = db.Unit
                        .Include(e => e.ContactDetails)
                        .Include(e => e.Incharge)
                        .Include(e => e.Incharge.EmpName)
                        .Include(e => e.Incharge.CorAddr)
                        .Include(e => e.Incharge.CorContact)
                        .Include(e => e.Type).AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGeoUnitData == null || OGeoUnitData.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            List<Unit> unit = OGeoUnitData.ToList();
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


                    case "GRADE":
                        var OGeoGradeData = db.Grade
                            //.Include(e => e.Address)
                            //.Include(e => e.ContactDetails)
                            //.Include(e => e.Grade)
                        .AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

                        if (OGeoGradeData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OGeoGradeData)
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

                    case "LEVEL":
                        var rptleveldata = db.Level
                       .AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

                        if (rptleveldata == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in rptleveldata)
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


                    case "JOBSTATUS":
                        var OGeoJobStatusData = db.Company
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.JobStatus)
                        .Include(e => e.JobStatus.Select(r => r.EmpActingStatus))
                         .Include(e => e.JobStatus.Select(r => r.EmpStatus)).AsNoTracking()
                        .Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();

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


                    case "JOB":
                        var OGjobData = db.Job
                        .AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (OGjobData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OGjobData)
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




                    case "JOBPOSITION":
                        var OFunJobPositionData = db.JobPosition.ToList();
                        //.Include(e => e.JobPosition)

                        //.Where(d => d.Id == CompanyId).SingleOrDefault();
                        if (OFunJobPositionData == null || OFunJobPositionData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OFunJobPositionData)
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


                    case "SALARYHEAD":
                        var OSalSalaryHeadData = db.SalaryHead

                            .Include(r => r.Frequency)
                            .Include(r => r.ProcessType)
                            .Include(r => r.RoundingMethod)
                            .Include(r => r.SalHeadOperationType)
                            .Include(r => r.Type).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalSalaryHeadData == null || OSalSalaryHeadData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalSalaryHeadData)
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


                    case "LOANADVANCEDHEAD":
                        var OSalLoanAdvanceHeadData = db.LoanAdvanceHead

                            .Include(r => r.ITLoan)
                            .Include(r => r.LoanAdvancePolicy)
                            .Include(r => r.LoanAdvancePolicy.Select(t => t.InterestType))
                            .Include(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages))
                            .Include(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages.RateMaster))
                            .Include(r => r.LoanAdvancePolicy.Select(t => t.LoanSanctionWages.RateMaster.Select(w => w.SalHead)))
                            .Include(r => r.SalaryHead)
                            .Include(r => r.ITSection.Select(w => w.ITSectionList))
                            .Include(r => r.ITSection.Select(w => w.ITSectionListType)).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().ToList();

                        if (OSalLoanAdvanceHeadData.Count == null && OSalLoanAdvanceHeadData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            //foreach (var item in OSalLoanAdvanceHeadData)
                            //{


                            foreach (var ca in OSalLoanAdvanceHeadData)
                            {
                                if (ca.LoanAdvancePolicy != null && ca.LoanAdvancePolicy.Count() > 0)
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

                                else
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
                                        Fld7 = "0",
                                        Fld8 = "",
                                        Fld9 = "",
                                        Fld10 = "0",
                                        Fld11 = "0",
                                        Fld12 = "0",
                                        Fld13 = "",
                                        Fld14 = "0",
                                        Fld15 = "0",
                                        Fld16 = "0",
                                        Fld17 = "0",
                                        Fld18 = "0",
                                        Fld19 = "0",
                                        Fld20 = "0",
                                        Fld21 = "0",
                                        Fld22 = "0",
                                        Fld23 = "",
                                        Fld24 = "0",
                                        Fld25 = ca.ITLoan == null ? "" : ca.ITLoan.FullDetails,
                                        Fld26 = ca.ITSection == null ? "" : ca.ITSection.Select(e => e.ITSectionList.LookupVal.ToUpper()).FirstOrDefault(),
                                        Fld27 = ca.ITSection == null ? "" : ca.ITSection.Select(e => e.ITSectionListType.LookupVal.ToUpper()).FirstOrDefault(),
                                    };
                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);


                                }

                            }
                            //}
                            return OGenericPayrollStatement;
                        }
                        break;
                    case "SALHEADFORMULA":
                        var OSalHeadFormulaData = db.SalHeadFormula
                            .Include(r => r.GeoStruct)
                            .Include(r => r.PayStruct)
                            .Include(r => r.FuncStruct)
                             .Include(r => r.SalWages)
                             .Include(r => r.SalWages.RateMaster)
                          .AsNoTracking().ToList();//modify roundingmethod in other code

                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalHeadFormulaData == null || (OSalHeadFormulaData.Count() == 0))
                        {
                            return null;
                        }

                        else
                        {
                            Utility.GetOrganizationDataClass GeoDataIndGeo = new Utility.GetOrganizationDataClass();
                            Utility.GetOrganizationDataClass GeoDataIndPay = new Utility.GetOrganizationDataClass();
                            Utility.GetOrganizationDataClass GeoDataIndFun = new Utility.GetOrganizationDataClass();
                            foreach (var ca in OSalHeadFormulaData)
                            {
                                if (salheadlist != null && salheadlist.Count() != 0)
                                {
                                    foreach (var ca2 in salheadlist)
                                    {
                                        var filterhead = ca2.Trim();
                                        var formulaname = ca.Name.Trim();
                                        if (filterhead == formulaname)
                                        {


                                            if (ca.GeoStruct_Id != null)
                                            {
                                                int? geoid = ca.GeoStruct_Id;
                                                GeoStruct geostruct = db.GeoStruct.Find(geoid);
                                                GeoDataIndGeo = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, null, null, 0);
                                            }

                                            if (ca.PayStruct_Id != null)
                                            {
                                                int? payid = ca.PayStruct_Id;
                                                PayStruct paystruct = db.PayStruct.Find(payid);
                                                GeoDataIndPay = ReportRDLCObjectClass.GetOrganizationDataInd(null, paystruct, null, 2);

                                            }
                                            if (ca.FuncStruct_Id != null)
                                            {
                                                int? funid = ca.FuncStruct_Id;
                                                FuncStruct funstruct = db.FuncStruct.Find(funid);
                                                GeoDataIndFun = ReportRDLCObjectClass.GetOrganizationDataInd(null, null, funstruct, 3);
                                            }





                                            if (ca.SalWages != null)
                                            {
                                                var rate = ca.SalWages.RateMaster.ToList();
                                                foreach (var ra in rate)
                                                {
                                                    GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Name.ToString(),
                                                        Fld2 = GeoDataIndGeo.GeoStruct_Location_Name,
                                                        Fld3 = GeoDataIndPay.PayStruct_Grade_Name + ' ' + GeoDataIndPay.PayStruct_JobStatus_EmpStatus + ' ' + GeoDataIndPay.PayStruct_JobStatus_EmpActingStatus,
                                                        Fld4 = GeoDataIndFun.FuncStruct_Job_Name,
                                                        Fld25 = ca.SalWages_Id.ToString(),
                                                        Fld26 = ca.SalWages == null ? "" : ca.SalWages.FullDetails.ToString(),
                                                        Fld27 = ra.Id.ToString(),
                                                        Fld28 = ra.FullDetails.ToString(),

                                                    };

                                                    OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                                }
                                            }
                                            else
                                            {
                                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    Fld1 = ca.Name.ToString(),
                                                    Fld2 = GeoDataIndGeo.GeoStruct_Location_Name,
                                                    Fld3 = GeoDataIndPay.PayStruct_Grade_Name + ' ' + GeoDataIndPay.PayStruct_JobStatus_EmpStatus + ' ' + GeoDataIndPay.PayStruct_JobStatus_EmpActingStatus,
                                                    Fld4 = GeoDataIndFun.FuncStruct_Job_Name,
                                                    Fld25 = ca.SalWages_Id.ToString(),
                                                    Fld26 = ca.SalWages == null ? "" : ca.SalWages.FullDetails.ToString(),

                                                };

                                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);
                                            }


                                        }
                                    }
                                }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "CPIRULE":
                        var OSalCPIRuleData = db.CPIRule


                            .Include(r => r.CPIRuleDetails)
                            .Include(r => r.CPIRuleDetails.Select(t => t.CPIWages))
                            .Include(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster))
                            .Include(r => r.CPIRuleDetails.Select(t => t.CPIWages.RateMaster.Select(s => s.SalHead)))

                            .Include(r => r.CPIUnitCalc)
                            .Include(r => r.RoundingMethod).AsNoTracking().ToList();//modify roundingmethod in other code

                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalCPIRuleData == null || (OSalCPIRuleData.Count() == 0))
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in OSalCPIRuleData)
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


                    case "PAYBANK":
                        var OBankData = db.Bank
                        .Include(e => e.Address)
                        .Include(e => e.ContactDetails)
                        .Include(e => e.Branches).AsNoTracking().ToList();
                        // .Where(d => d.Id == CompanyId).AsParallel().ToList().AsParallel();

                        if (OBankData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OBankData)
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


                    case "PAYPROCESSGROUP":
                        var OPayData = db.PayProcessGroup
                        .Include(e => e.PayFrequency)
                        .Include(e => e.PayMonthConcept)
                        .Include(e => e.PayrollPeriod).AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().ToList().AsParallel();

                        if (OPayData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPayData)
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


                    case "PAYSCALE":
                        var OPaySCALEData = db.PayScale
                        .Include(e => e.CPIEntryT)
                        .Include(e => e.PayScaleType)
                        .Include(e => e.PayScaleArea)
                        .Include(e => e.PayScaleArea.Select(c => c.LocationObj))
                          .Include(e => e.Rounding).AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().ToList().AsParallel();

                        if (OPaySCALEData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPaySCALEData)
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


                    case "PAYSCALEASSIGNMENT":
                        var OSalPayScaleAssignmentData1 = db.PayScaleAssignment
                            .Include(e => e.PayScaleAgreement)
                            .Include(e => e.SalaryHead)
                            .Include(e => e.SalHeadFormula)
                            .Include(e => e.SalHeadFormula.Select(t => t.SalWages)).AsNoTracking()
                            .ToList();

                        if (OSalPayScaleAssignmentData1 == null || OSalPayScaleAssignmentData1.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalPayScaleAssignmentData1)
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


                    case "ITSECTION":
                        var OITSectionData = db.IncomeTax

                            .Include(r => r.FyCalendar)
                            .Include(r => r.ITSection)
                            .Include(r => r.ITSection.Select(t => t.ITSectionList))
                            .Include(r => r.ITSection.Select(t => t.ITSectionListType)).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyPayrollId && d.IncomeTax.Count != 0).AsParallel().SingleOrDefault();
                        if (OITSectionData == null)
                        {
                            return null;
                        }
                        else
                        {
                            if (OITSectionData == null || OITSectionData.Count() == 0)
                                return null;
                            else
                            {

                                foreach (var ca in OITSectionData)
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


                    case "ITTDS":
                        var OITTdsData = db.IncomeTax

                            .Include(r => r.FyCalendar)
                            .Include(r => r.ITTDS)
                            .Include(r => r.ITTDS.Select(t => t.Category)).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyPayrollId && d.IncomeTax.Count != 0).AsParallel().SingleOrDefault();
                        if (OITTdsData == null)
                        {
                            return null;
                        }
                        else
                        {
                            if (OITTdsData == null || OITTdsData.Count() == 0)
                                return null;
                            else
                            {
                                foreach (var ca in OITTdsData)
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


                    case "ITSECTION10B":
                        var OITSection10BData = db.IncomeTax

                            .Include(r => r.FyCalendar)
                            .Include(r => r.ITSection)
                            .Include(r => r.ITSection.Select(t => t.ITSectionList))
                            .Include(r => r.ITSection.Select(t => t.ITSectionListType))
                            .Include(r => r.ITSection.Select(t => t.ITSection10))
                            .Include(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead)))
                            .Include(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead.Select(x => x.SalHead))))
                            .Include(r => r.ITSection.Select(t => t.ITSection10.Select(y => y.Itsection10salhead.Select(x => x.Frequency)))).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();


                        if (OITSection10BData == null || OITSection10BData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OITSection10BData)
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


                    case "ITINVESTMENT":
                        var OITInvestmentData = db.ITInvestment

                            .Include(r => r.SalaryHead)
                            .ToList();

                        //.Where(d => d.Id == CompanyPayrollId && d.IncomeTax.Count != 0).AsParallel().SingleOrDefault();


                        if (OITInvestmentData == null || OITInvestmentData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OITInvestmentData)
                            {


                                GenericField100 OGenericSalMasterObjStatement = new GenericField100()
                                {
                                    Fld1 = ca.Id.ToString(),
                                    Fld2 = ca.SalaryHead == null ? "" : ca.SalaryHead.Code == null ? "" : ca.SalaryHead.Code.ToString(),
                                    Fld3 = ca.SalaryHead == null ? "" : ca.SalaryHead.Name == null ? "" : ca.SalaryHead.Name.ToString(),
                                    Fld4 = ca.ITInvestmentName == null ? "" : ca.ITInvestmentName.ToString(),
                                    Fld5 = ca.MaxAmount == null ? "" : ca.MaxAmount.ToString(),
                                    Fld6 = ca.MaxPercentage == null ? "" : ca.MaxPercentage.ToString(),
                                    Fld7 = ca.FullDetails == null ? "" : ca.FullDetails.ToString(),


                                };
                                //write data to generic class
                                OGenericPayrollStatement.Add(OGenericSalMasterObjStatement);

                            }

                            return OGenericPayrollStatement;
                        }
                        break;

                    ////new
                    case "PF":
                        var OSalPFMasterData = db.PFMaster

                            .Include(r => r.EDLISTrustType)
                            .Include(r => r.PensionTrustType)

                            .Include(r => r.PFTrustType)

                            .Include(r => r.EPFWages)
                            .Include(r => r.EPFWages.RateMaster)
                            .Include(r => r.EPFWages.RateMaster.Select(t => t.SalHead))
                            .Include(r => r.EPSWages)
                            .Include(r => r.EPSWages.RateMaster)
                            .Include(r => r.EPSWages.RateMaster.Select(t => t.SalHead))
                            .Include(r => r.PFAdminWages)
                            .Include(r => r.PFAdminWages.RateMaster)
                            .Include(r => r.PFAdminWages.RateMaster.Select(t => t.SalHead))
                            .Include(r => r.PFEDLIWages)
                            .Include(r => r.PFEDLIWages.RateMaster)
                            .Include(r => r.PFEDLIWages.RateMaster.Select(t => t.SalHead))
                            .Include(r => r.PFInspWages)
                            .Include(r => r.PFInspWages.RateMaster)
                            .Include(r => r.PFInspWages.RateMaster.Select(t => t.SalHead)).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyPayrollId && d.Count != 0).AsParallel().SingleOrDefault();

                        if (OSalPFMasterData == null || OSalPFMasterData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalPFMasterData)
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

                    ////new
                    case "JVEMPACCOUNT":
                        var jvempacc = db.JVParameter
                        .Include(r => r.JVNonStandardEmp.Select(t => t.AccountType))
                       .Include(r => r.JVNonStandardEmp.Select(t => t.EmployeePayroll))
                       .Include(r => r.JVNonStandardEmp.Select(t => t.EmployeePayroll.Employee.EmpName))
                        .Include(r => r.JVNonStandardEmp.Select(t => t.Branch))
                       .AsNoTracking().AsParallel().ToList();
                        //.Where(d => d.Id == CompanyId).AsParallel().SingleOrDefault();
                        if (jvempacc == null || jvempacc.Count() == 0)
                        {
                            return null;
                        }

                        else
                        {
                            foreach (var ca in jvempacc)
                            {
                                var jvadata = ca.JVNonStandardEmp.ToList();
                                if (jvadata != null && jvadata.Count() > 0)
                                {
                                    foreach (var item in jvadata)
                                    {
                                        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                        {
                                            Fld1 = ca.Id.ToString(),
                                            Fld3 = item.EmployeePayroll == null ? "" : item.EmployeePayroll.Employee == null ? "" : item.EmployeePayroll.Employee.EmpName == null ? "" : item.EmployeePayroll.Employee.EmpName.FullNameFML.ToString(),
                                            Fld2 = item.EmployeePayroll == null ? "" : item.EmployeePayroll.Employee == null ? "" : item.EmployeePayroll.Employee.EmpName == null ? "" : item.EmployeePayroll.Employee.EmpCode.ToString(),
                                            Fld4 = item.ProductCode.ToString(),
                                            Fld5 = item.AccountType == null ? "" : item.AccountType.LookupVal.ToString(),
                                            Fld6 = item.AccountNo.ToString(),
                                            Fld7 = item.SubAccountNo.ToString(),
                                            Fld8 = item.Branch == null ? "" : item.Branch.Name.ToString(),
                                            Fld9 = ca.JVName.ToString()


                                        };
                                        //write data to generic class
                                        OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                    }
                                }
                            }
                            return OGenericPayrollStatement;

                        }
                        break;

                    case "PTAX":
                        var OSalPTaxMasterData = db.PTaxMaster

                             .Include(r => r.Frequency)
                             .Include(r => r.PTStatutoryEffectiveMonths.Select(t => t.EffectiveMonth))
                             .Include(r => r.PTStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange))
                             .Include(r => r.PTWagesMaster)
                             .Include(r => r.PTWagesMaster.RateMaster)
                              .Include(r => r.PTWagesMaster.RateMaster.Select(t => t.SalHead))
                             .Include(r => r.States).AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyPayrollId && d.PTaxMaster.Count != 0).AsParallel().SingleOrDefault();

                        if (OSalPTaxMasterData == null || OSalPTaxMasterData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalPTaxMasterData)
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



                    ///new

                    case "LWFMASTER":
                        var OSalLWFMasterData = db.LWFMaster

                           .Include(r => r.LWFStates)
                           .Include(r => r.LWFStatutoryEffectiveMonths)
                           .Include(r => r.WagesMaster)
                           .Include(r => r.LWFStatutoryEffectiveMonths.Select(w => w.StatutoryWageRange))
                            .Include(r => r.LWFStatutoryEffectiveMonths.Select(w => w.EffectiveMonth))
                           .Include(r => r.WagesMaster.RateMaster)
                             .Include(r => r.WagesMaster.RateMaster.Select(t => t.SalHead)).AsNoTracking().ToList();
                        //  .Include(e => e.PTaxMaster.Select(r => r.PTWagesMaster.RateMaster.Select(t => t.SalHead)))
                        //.Where(d => d.Id == CompanyPayrollId && d.LWFMaster.Count != 0).AsParallel().SingleOrDefault();
                        if (OSalLWFMasterData == null)
                            return null;
                        else
                        {
                            if (OSalLWFMasterData == null || OSalLWFMasterData.Count() == 0)
                                return null;
                            else
                            {
                                foreach (var ca in OSalLWFMasterData)
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

                    ///new

                    case "ESICMASTER":
                        var OSalESICMasterData = db.ESICMaster

                            .Include(r => r.ESICStatutoryEffectiveMonths)
                            .Include(r => r.ESICStatutoryEffectiveMonths.Select(t => t.EffectiveMonth))
                            .Include(r => r.ESICStatutoryEffectiveMonths.Select(t => t.StatutoryWageRange))
                            .Include(r => r.Location)
                            .Include(e => e.Location.Select(r => r.LocationObj))
                            .Include(r => r.WageMasterPay)
                            .Include(r => r.WageMasterPay.RateMaster)
                            .Include(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead))
                            //.Include(e => e.ESICMaster.Select(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead.Code)))
                            //.Include(e => e.ESICMaster.Select(r => r.WageMasterPay.RateMaster.Select(w => w.SalHead.Name)))
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


                    case "BONUSACT":
                        var OSalBonusActData = db.BonusAct

                    .Include(r => r.BonusCalendar)
                    .Include(r => r.BonusWages)
                    .Include(r => r.BonusWages.RateMaster)
                    .Include(r => r.BonusWages.RateMaster.Select(t => t.SalHead)).AsNoTracking().ToList();
                        //.Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead.Code)))
                        //.Include(e => e.BonusAct.Select(r => r.BonusWages.RateMaster.Select(t => t.SalHead.Name)))
                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

                        if (OSalBonusActData == null || OSalBonusActData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalBonusActData)
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


                    case "GRATUITYACT":
                        var OSalGratuityActData = db.GratuityAct
                            .Include(r => r.GratuityWages)
                            .Include(r => r.GratuityWages.RateMaster)
                            .Include(r => r.GratuityWages.RateMaster.Select(t => t.SalHead))
                            .Include(r => r.GratuityWages.RateMaster.Select(t => t.Wages))
                            .Include(r => r.LvHead).ToList();
                        // .Where(d => d.Id == CompanyPayrollId && d.GratuityAct.Count != 0).SingleOrDefault();

                        if (OSalGratuityActData == null || OSalGratuityActData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalGratuityActData)
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


                    case "SUSPENSIONSALPOLICY":
                        var OSalSuspensionSalPolicyData = db.SuspensionSalPolicy
                            //.Include(e => e.SuspensionSalPolicy)
                            .Include(r => r.DayRange)
                            .Include(r => r.SuspensionWages)
                            .Include(r => r.SuspensionWages.RateMaster).AsNoTracking().ToList();


                        //.Where(d => d.Id == CompanyPayrollId).SingleOrDefault();

                        if (OSalSuspensionSalPolicyData == null || OSalSuspensionSalPolicyData.Count() == 0)
                            return null;
                        else
                        {
                            foreach (var ca in OSalSuspensionSalPolicyData)
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


                    case "NEGSALACT":
                        var OSalNegSalActData = db.CompanyPayroll
                            .Include(e => e.NegSalAct).AsNoTracking()

                        .Where(d => d.Id == CompanyPayrollId).AsParallel().SingleOrDefault();

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


                    case "RETIREMENTDAYACT":
                        var ORetireActData = db.RetirementDay.AsNoTracking().ToList();
                        //.Where(d => d.Id == CompanyPayrollId).AsParallel().ToList();

                        if (ORetireActData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in ORetireActData)
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


                    case "CALENDAR":
                        var OCalData = db.Calendar.Include(e => e.Name).AsNoTracking().ToList();

                        //.Where(d => d.Id == CompanyId).AsParallel().ToList();

                        if (OCalData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OCalData)
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


                    case "HOLIDAYCALENDAR":
                        var OHoliData = db.HolidayCalendar
                            .Include(a => a.HoliCalendar).Include(a => a.HolidayList)
                             .Include(a => a.HoliCalendar.Name)
                            .Include(a => a.HolidayList.Select(b => b.Holiday))
                             .Include(a => a.HolidayList.Select(b => b.Holiday.HolidayName))
                               .Include(a => a.HolidayList.Select(b => b.Holiday.HolidayName)).AsNoTracking()
                        .Where(d => d.Id != null).AsParallel().ToList();

                        if (OHoliData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OHoliData)
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


                    case "WEEKLYOFFCALENDAR":
                        var OWeeklyData = db.WeeklyOffCalendar
                            .Include(a => a.WeeklyOffList)
                           .Include(a => a.WeeklyOffList.Select(b => b.WeekDays))
                             .Include(a => a.WeeklyOffList.Select(b => b.WeeklyOffStatus))
                            .Include(a => a.WOCalendar)
                            .Include(a => a.WOCalendar.Name).AsNoTracking()


                        .Where(d => d.Id != null).AsParallel().ToList();

                        if (OWeeklyData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OWeeklyData)
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