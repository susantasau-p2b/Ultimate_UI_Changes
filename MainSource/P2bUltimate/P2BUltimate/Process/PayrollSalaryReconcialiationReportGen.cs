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
    public class PayrollSalaryReconcialiationReportGen
    {
        public static List<GenericField100> GenerateSalaryReconciliationReReport(int CompanyPayrollId, List<int> EmpPayrollIdList, List<string> mPayMonth, string mObjectName, int CompanyId, List<string> oth_idlist, List<string> salheadlist, List<string> loanadvidlist, List<string> forithead, List<string> SpecialGroupslist)
        {
            string OrderBy = "";
            List<GenericField100> OGenericPayrollStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {
                    case "SALARYRECONCILIATION":
                        var paymnth = mPayMonth.SingleOrDefault();

                        //List<SalaryReconcilation> SalReconData = new List<SalaryReconcilation>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    List<SalaryReconcilation> SalReconData_temp = db.SalaryReconcilation
                        //    .Include(e => e.EmployeePayroll)
                        //    .Include(e => e.EmployeePayroll.Employee)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                        //    .Include(e => e.EmployeePayroll.Employee.EmpName)
                        //    .Include(e => e.SalaryHead)
                        //     .Include(e => e.SalaryHead.Type)
                        //            .Where(e => e.EmployeePayroll.Employee.Id == item && e.CurrentMonth == paymnth && (e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" || e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION"))
                        //         .ToList();
                        //    if (SalReconData_temp != null)
                        //    {
                        //        SalReconData.AddRange(SalReconData_temp);
                        //    }
                        //}

                        ///////////////




                        var SalReconData = db.SalaryReconcilation
    .Where(z => EmpPayrollIdList.Contains(z.EmployeePayroll.Employee.Id) &&
                (salheadlist.Any() ? salheadlist.Contains(z.SalaryHead.Code + "-" + z.SalaryHead.Name) : true) && z.CurrentMonth == paymnth)
    .Select(z => new
    {
        EmpCode = z.EmployeePayroll.Employee.EmpCode,
        EmpName = z.EmployeePayroll.Employee.EmpName.FullNameFML,
        LocName = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc,
        CurrentMonth = z.CurrentMonth,
        PrevMonth = z.PrevMonth,
        SalHdcode = z.SalaryHead.Code + "-" + z.SalaryHead.Name,
        CurrentAmt = z.CurrentAmt,
        PrevAmt = z.PrevAmt,
        DiffAmt = z.DiffAmt,
    }).Where(e=>e.CurrentAmt!=0 || e.PrevAmt!=0).ToList();
    
                        if (SalReconData.Count == 0 || SalReconData == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var salR in SalReconData)
                            {
                                //if (salheadlist != null && salheadlist.Count() != 0)
                                //{
                                //foreach (var item2 in salheadlist)
                                //{
                                //var salhead = salR.SalaryHead.Name;
                                //var salheadcode = salR.SalaryHead.Code;
                                //var items = item2.Split('-');
                                //string salcode = items[0];
                                //string salname = items[1];

                                //if (salheadcode == salcode)
                                //{
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = salR.EmpCode == null ? "" : salR.EmpCode, //EmpCode
                                    Fld2 = salR.EmpName == null ? "" : salR.EmpName, //EmpName
                                    Fld18 = salR.LocName == null ? "" : salR.LocName,//LocationName
                                    Fld13 = salR.CurrentMonth, //Curr month
                                    Fld14 = salR.PrevMonth, //Prev month
                                    Fld17 = salR.SalHdcode, //salhead
                                    Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                    Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                    Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //}
                                //}
                                //}
                                //else
                                //{
                                //    var salhead = salR.SalaryHead.Name;
                                //    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                //    {
                                //        Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                //        Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                //        Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                //        Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                //        Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                //        Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                //        Fld14 = salR.PrevMonth.ToString(), //Prev month
                                //        Fld17 = salhead.ToString(), //salhead
                                //        Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc //LocationName
                                //    };
                                //    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "SALARYRECONCILIATIONEARN":
                        var paymnth1 = mPayMonth.SingleOrDefault();

                        //List<SalaryReconcilation> SalReconDataEarn = new List<SalaryReconcilation>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    List<SalaryReconcilation> SalReconDataEarn_temp = db.SalaryReconcilation
                        //    .Include(e => e.EmployeePayroll)
                        //    .Include(e => e.EmployeePayroll.Employee)
                        //    .Include(e => e.EmployeePayroll.Employee.GeoStruct)
                        //    .Include(e => e.EmployeePayroll.Employee.PayStruct)
                        //     .Include(e => e.EmployeePayroll.Employee.PayStruct.Grade)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                        //    .Include(e => e.EmployeePayroll.Employee.EmpName)
                        //    .Include(e => e.SalaryHead)
                        //     .Include(e => e.SalaryHead.Type)
                        //      .Where(e => e.EmployeePayroll.Employee.Id == item && e.CurrentMonth == paymnth1 && e.SalaryHead.Type.LookupVal.ToUpper() == "EARNING")
                        //         .ToList();
                        //    if (SalReconDataEarn_temp.Count > 0)
                        //    {
                        //        SalReconDataEarn.AddRange(SalReconDataEarn_temp);
                        //    }
                        //}
                        var SalReconDataEarn = db.SalaryReconcilation.Where(z => EmpPayrollIdList.Contains(z.EmployeePayroll.Employee.Id) &&

                            (salheadlist.Any() ? salheadlist.Contains(z.SalaryHead.Code + "-" + z.SalaryHead.Name) : true) && z.CurrentMonth == paymnth1)
                            .Select(z => new
                {
                    EmpCode = z.EmployeePayroll.Employee.EmpCode,
                    EmpName = z.EmployeePayroll.Employee.EmpName.FullNameFML,
                    LocName = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc,
                    LocCode = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode,
                    GradeCode = z.EmployeePayroll.Employee.PayStruct.Grade.Code,
                    Gradename = z.EmployeePayroll.Employee.PayStruct.Grade.Name,
                    CurrentMonth = z.CurrentMonth,
                    PrevMonth = z.PrevMonth,
                    salhead = z.SalaryHead.Code + "-" + z.SalaryHead.Name,
                    CurrentAmt = z.CurrentAmt,
                    PrevAmt = z.PrevAmt,
                    DiffAmt = z.DiffAmt,
                    Reason = z.Reason,

                }).Where(e=> e.CurrentAmt!=0 || e.PrevAmt!=0).ToList();

                        if (SalReconDataEarn.Count == 0 || SalReconDataEarn == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var salR in SalReconDataEarn)
                            {
                                //if (salheadlist != null && salheadlist.Count() != 0)
                                //{
                                //    //foreach (var item2 in salheadlist)
                                //{
                                //    var salhead = salR.SalaryHead.Name;
                                //    var salheadcode = salR.SalaryHead.Code;
                                //    var items = item2.Split('-');
                                //    string salcode = items[0];
                                //    string salname = items[1];

                                //    if (salheadcode == salcode)
                                //    {
                                //        if (salR.DiffAmt != 0)
                                //        {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = salR.EmpCode == null ? "" : salR.EmpCode, //EmpCode
                                    Fld2 = salR.EmpName == null ? "" : salR.EmpName, //EmpName
                                    Fld5 = salR.Gradename == null ? "" : salR.Gradename,//gradename
                                    Fld6 = salR.GradeCode == null ? "" : salR.GradeCode, //GradeCode
                                    Fld7 = salR.LocCode == null ? "" : salR.LocCode, //LocCode
                                    Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                    Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                    Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                    Fld12 = salR.Reason, //Reason
                                    Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                    Fld14 = salR.PrevMonth.ToString(),
                                    Fld17 = salR.salhead, //salhead
                                    Fld18 = salR.LocName == null ? "" : salR.LocName,//LocationName
                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //}
                                // }
                                // }
                                //}
                                //else
                                //{
                                //    var salhead = salR.SalaryHead.Name;
                                //    if (salR.DiffAmt != 0)
                                //    {
                                //        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                //        {
                                //            Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                //            Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                //            //Fld3 = salrec.Fld3.ToString(), //CompCode
                                //            ////Fld4 = salrec.Fld4.ToString(), //CompName
                                //            Fld5 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Code, //EmpGradeCode
                                //            Fld6 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Name, //EmpGradeName
                                //            Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                //            //Fld8 = salrec.Fld8.ToString(), //EmpLocationName
                                //            Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                //            Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                //            Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                //            Fld12 = salR.Reason, //Reason
                                //            Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                //            Fld14 = salR.PrevMonth.ToString(), //Prev month
                                //            //Fld15 = salrec.Fld15.ToString(), //Earning
                                //            //Fld16 = salrec.Fld16.ToString(), //LocationObjID
                                //            Fld17 = salhead.ToString(), //salhead
                                //            Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc //LocationName
                                //        };
                                //        OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //    }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;


                    case "SALARYRECONCILIATIONDED":

                        //var SalReconDataDed = db.GenericField100.Where(e => e.Fld15 == "D" && e.Fld11 != "0").GroupBy(e => e.Fld16).ToList();

                        //if (SalReconDataDed.Count == 0 || SalReconDataDed == null)
                        //{
                        //    return null;
                        //}

                        var paymnth2 = mPayMonth.SingleOrDefault();

                        //List<SalaryReconcilation> SalReconDataDed = new List<SalaryReconcilation>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    List<SalaryReconcilation> SalReconDataDed_temp = db.SalaryReconcilation
                        //    .Include(e => e.EmployeePayroll)
                        //    .Include(e => e.EmployeePayroll.Employee)
                        //    .Include(e => e.EmployeePayroll.Employee.GeoStruct)
                        //    .Include(e => e.EmployeePayroll.Employee.PayStruct)
                        //     .Include(e => e.EmployeePayroll.Employee.PayStruct.Grade)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                        //    .Include(e => e.EmployeePayroll.Employee.EmpName)
                        //    .Include(e => e.SalaryHead)
                        //     .Include(e => e.SalaryHead.Type)
                        //      .Where(e => e.EmployeePayroll.Employee.Id == item && e.CurrentMonth == paymnth2 && e.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION")
                        //         .ToList();
                        //    if (SalReconDataDed_temp.Count > 0)
                        //    {
                        //        SalReconDataDed.AddRange(SalReconDataDed_temp);
                        //    }
                        //}
                        var SalReconDataDed = db.SalaryReconcilation.Where(z => EmpPayrollIdList.Contains(z.EmployeePayroll.Employee.Id) &&

                            (salheadlist.Any() ? salheadlist.Contains(z.SalaryHead.Code + "-" + z.SalaryHead.Name) : true) && z.CurrentMonth == paymnth2)
                            .Select(z => new
                            {

                                EmpCode = z.EmployeePayroll.Employee.EmpCode,
                                EmpName = z.EmployeePayroll.Employee.EmpName.FullNameFML,
                                LocName = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                LocCode = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode,
                                GradeCode = z.EmployeePayroll.Employee.PayStruct.Grade.Code,
                                Gradename = z.EmployeePayroll.Employee.PayStruct.Grade.Name,
                                CurrentMonth = z.CurrentMonth,
                                PrevMonth = z.PrevMonth,
                                salhead = z.SalaryHead.Code + "-" + z.SalaryHead.Name,
                                CurrentAmt = z.CurrentAmt,
                                PrevAmt = z.PrevAmt,
                                DiffAmt = z.DiffAmt,
                                Reason = z.Reason,

                            }).Where(e=>e.CurrentAmt!=0 || e.PrevAmt!=0).ToList();

                        if (SalReconDataDed.Count == 0 || SalReconDataDed == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var salR in SalReconDataDed)
                            {
                                //if (salheadlist != null && salheadlist.Count() != 0)
                                //{
                                //    foreach (var item2 in salheadlist)
                                //    {
                                //        var salhead = salR.SalaryHead.Name;
                                //        var salheadcode = salR.SalaryHead.Code;
                                //        var items = item2.Split('-');
                                //        string salcode = items[0];
                                //        string salname = items[1];

                                //        if (salheadcode == salcode)
                                //        {
                                //if (salR.DiffAmt != 0)
                                //{
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld1 = salR.EmpCode == null ? "" : salR.EmpCode, //EmpCode
                                    Fld2 = salR.EmpName == null ? "" : salR.EmpName, //EmpName
                                    Fld5 = salR.Gradename == null ? "" : salR.Gradename,//gradename
                                    Fld6 = salR.GradeCode == null ? "" : salR.GradeCode, //GradeCode
                                    Fld7 = salR.LocCode == null ? "" : salR.LocCode, //LocCode
                                    Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                    Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                    Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                    Fld12 = salR.Reason, //Reason
                                    Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                    Fld14 = salR.PrevMonth.ToString(), //Prev month
                                    Fld17 = salR.salhead, //salhead
                                    Fld18 = salR.LocName == null ? "" : salR.LocName,//LocationName

                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //        }
                                //    }
                                //}
                                // }
                                //else
                                //{
                                //    var salhead = salR.SalaryHead.Name;
                                //    if (salR.DiffAmt != 0)
                                //    {
                                //        GenericField100 OGenericGeoObjStatement = new GenericField100()
                                //        {
                                //            Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                //            Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                //            //Fld3 = salrec.Fld3.ToString(), //CompCode
                                //            //Fld4 = salrec.Fld4.ToString(), //CompName
                                //            Fld5 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Code, //EmpGradeCode
                                //            Fld6 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Name, //EmpGradeName
                                //            Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                //            //Fld8 = salrec.Fld8.ToString(), //EmpLocationName
                                //            Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                //            Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                //            Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                //            Fld12 = salR.Reason, //Reason
                                //            Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                //            Fld14 = salR.PrevMonth.ToString(), //Prev month
                                //            //Fld15 = salrec.Fld15.ToString(), //Earning
                                //            //Fld16 = salrec.Fld16.ToString(), //LocationObjID
                                //            Fld17 = salhead.ToString(), //salhead
                                //            Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc //LocationName
                                //        };
                                //   OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //  }
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;



                    case "SALARYRECONCILIATIONSUMMARYLOC":

                        //var paymnth3 = mPayMonth.SingleOrDefault();

                        //List<SalaryReconcilation> SalReconDataSumLoc = new List<SalaryReconcilation>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    List<SalaryReconcilation> SalReconDataSumLoc_temp = db.SalaryReconcilation.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == item && e.CurrentMonth == mPayMonth.FirstOrDefault())
                        //    .Include(e => e.EmployeePayroll)
                        //    .Include(e => e.EmployeePayroll.Employee)
                        //    .Include(e => e.EmployeePayroll.Employee.GeoStruct)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                        //    .Include(e => e.SalaryHead)
                        //     .ToList();

                        //    if (SalReconDataSumLoc_temp.Count > 0)
                        //    {
                        //        SalReconDataSumLoc.AddRange(SalReconDataSumLoc_temp);
                        //    }
                        //}
                        var SalReconDataSumLoc = db.SalaryReconcilation.Where(z => EmpPayrollIdList.Contains(z.EmployeePayroll.Employee.Id) &&

                            (salheadlist.Any() ? salheadlist.Contains(z.SalaryHead.Code + "-" + z.SalaryHead.Name) : true) && z.CurrentMonth == mPayMonth.FirstOrDefault()).Select(z => new
                            {
                                EmpCode = z.EmployeePayroll.Employee.EmpCode,
                                EmpName = z.EmployeePayroll.Employee.EmpName.FullNameFML,
                                LocName = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                LocCode = z.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode,
                                GradeCode = z.EmployeePayroll.Employee.PayStruct.Grade.Code,
                                CurrentMonth = z.CurrentMonth,
                                PrevMonth = z.PrevMonth,
                                salhead = z.SalaryHead.Code + "-" + z.SalaryHead.Name,
                                CurrentAmt = z.CurrentAmt,
                                PrevAmt = z.PrevAmt,
                                DiffAmt = z.DiffAmt,
                            }).Where(e=>e.CurrentAmt!=0 || e.PrevAmt!=0).ToList();


                        if (SalReconDataSumLoc.Count == 0 || SalReconDataSumLoc == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var salR in SalReconDataSumLoc)
                            {
                                //    var salhead = salR.SalaryHead.Name;
                                //    if (salheadlist != null && salheadlist.Count() != 0)
                                //    {
                                //        foreach (var item2 in salheadlist)
                                //        {
                                //            var salheadcode = salR.SalaryHead.Code;
                                //            var items = item2.Split('-');
                                //            string salcode = items[0];
                                //            string salname = items[1];

                                //            if (salheadcode == salcode)
                                //            {
                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                    Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                    Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                    Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                    Fld14 = salR.PrevMonth.ToString(), //Prev month
                                    Fld17 = salR.salhead, //salhead
                                    //Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                    //Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc, //LocationName
                                    Fld7 = salR.LocCode == null ? "" : salR.LocCode, //LocCode
                                    Fld18 = salR.LocName == null ? "" : salR.LocName,//LocationName
                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                //    {
                                //        //Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                //        //Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                //        //Fld3 = salrec.Fld3.ToString(), //CompCode
                                //        //Fld4 = salrec.Fld4.ToString(), //CompName
                                //        //Fld5 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Code, //EmpGradeCode
                                //        //Fld6 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Name, //EmpGradeName
                                //        //Fld8 = salrec.Fld8.ToString(), //EmpLocationName
                                //        Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                //        Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                //        Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                //        //Fld12 = salrec.Fld12.ToString(), //Reason
                                //        Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                //        Fld14 = salR.PrevMonth.ToString(), //Prev month
                                //        //Fld15 = salrec.Fld15.ToString(), //Earning
                                //        //Fld16 = salrec.Fld16.ToString(), //LocationObjID
                                //        Fld17 = salhead.ToString(), //salhead
                                //        Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                //        Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc, //LocationName
                                //        //Fld19 = salrec.Fld19.ToString(), //DepartmentCode
                                //        //Fld20 = salrec.Fld20.ToString()  //DepartmentDesc

                                //    };
                                //    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;

                    case "SALARYRECONCILIATIONSUMMARYDEP":

                        ////var paymnth4 = mPayMonth.SingleOrDefault();
                        //List<SalaryReconcilation> SalReconDataSumDep = new List<SalaryReconcilation>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    List<SalaryReconcilation> SalReconDataSumDep_temp = db.SalaryReconcilation.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == item && e.CurrentMonth == mPayMonth.FirstOrDefault())
                        //    .Include(e => e.EmployeePayroll)
                        //    .Include(e => e.EmployeePayroll.Employee)
                        //    .Include(e => e.EmployeePayroll.Employee.GeoStruct)
                        //        //.Include(e => e.EmployeePayroll.Employee.PayStruct)
                        //        // .Include(e => e.EmployeePayroll.Employee.PayStruct.Grade)
                        //        //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                        //     .Include(e => e.EmployeePayroll.Employee.GeoStruct.Department)
                        //       .Include(e => e.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj)
                        //        //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                        //        //.Include(e => e.EmployeePayroll.Employee.EmpName)
                        //    .Include(e => e.SalaryHead)
                        //        //.Include(e => e.SalaryHead.Type)

                        //         .ToList();
                        //    if (SalReconDataSumDep_temp.Count > 0)
                        //    {
                        //        SalReconDataSumDep.AddRange(SalReconDataSumDep_temp);
                        //    }
                        //}

                        var SalReconDataSumDep = db.SalaryReconcilation.Where(z => EmpPayrollIdList.Contains(z.EmployeePayroll.Employee.Id) &&

                            (salheadlist.Any() ? salheadlist.Contains(z.SalaryHead.Code + "-" + z.SalaryHead.Name) : true) && z.CurrentMonth == mPayMonth.FirstOrDefault()).Select(z => new
                            {
                                deptName = z.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                deptCode = z.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                CurrentMonth = z.CurrentMonth,
                                PrevMonth = z.PrevMonth,
                                salhead = z.SalaryHead.Code + "-" + z.SalaryHead.Name,
                                CurrentAmt = z.CurrentAmt,
                                PrevAmt = z.PrevAmt,
                                DiffAmt = z.DiffAmt,
                            }).Where(e=>e.CurrentAmt!=0 || e.PrevAmt!=0).ToList();

                        if (SalReconDataSumDep.Count == 0 || SalReconDataSumDep == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var salR in SalReconDataSumDep)
                            {
                                //var salhead = salR.SalaryHead.Name;
                                //if (salheadlist != null && salheadlist.Count() != 0)
                                //{
                                //    foreach (var item2 in salheadlist)
                                //    {
                                //        var salheadcode = salR.SalaryHead.Code;
                                //        var items = item2.Split('-');
                                //        string salcode = items[0];
                                //        string salname = items[1];

                                //        if (salheadcode == salcode)
                                //        {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    //Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                    //Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                    //Fld3 = salrec.Fld3.ToString(), //CompCode
                                    //Fld4 = salrec.Fld4.ToString(), //CompName
                                    //Fld5 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Code, //EmpGradeCode
                                    //Fld6 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Name, //EmpGradeName
                                    //Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                    //Fld8 = salrec.Fld8.ToString(), //EmpLocationName
                                    Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                    Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                    Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                    //Fld12 = salrec.Fld12.ToString(), //Reason
                                    Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                    Fld14 = salR.PrevMonth.ToString(), //Prev month
                                    //Fld15 = salrec.Fld15.ToString(), //Earning
                                    //Fld16 = salrec.Fld16.ToString(), //LocationObjID
                                    Fld17 = salR.salhead, //salhead
                                    //Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc, //LocationName
                                    //Fld19 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptCode, //DepartmentCode
                                    //Fld20 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,  //DepartmentDesc

                                    Fld19 = salR.deptCode == null ? "" : salR.deptCode, //LocCode
                                    Fld20 = salR.deptName == null ? "" : salR.deptName,//Lo
                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    //var salhead = salR.SalaryHead.Name;
                                //    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                //    {
                                //        //Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                //        //Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                //        //Fld3 = salrec.Fld3.ToString(), //CompCode
                                //        //Fld4 = salrec.Fld4.ToString(), //CompName
                                //        //Fld5 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Code, //EmpGradeCode
                                //        //Fld6 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Name, //EmpGradeName
                                //        //Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                //        //Fld8 = salrec.Fld8.ToString(), //EmpLocationName
                                //        Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                //        Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                //        Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                //        //Fld12 = salrec.Fld12.ToString(), //Reason
                                //        Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                //        Fld14 = salR.PrevMonth.ToString(), //Prev month
                                //        //Fld15 = salrec.Fld15.ToString(), //Earning
                                //        //Fld16 = salrec.Fld16.ToString(), //LocationObjID
                                //        Fld17 = salhead.ToString(), //salhead
                                //        //Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc, //LocationName
                                //        Fld19 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptCode, //DepartmentCode
                                //        Fld20 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptDesc  //DepartmentDesc
                                //    };
                                //    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break; 

                    case "SALARYRECONCILIATIONSUMMARYGRADE":
                        ////var paymnth5 = mPayMonth.SingleOrDefault();
                        //List<SalaryReconcilation> SalReconDataSumGrade = new List<SalaryReconcilation>();
                        //foreach (var item in EmpPayrollIdList)
                        //{
                        //    List<SalaryReconcilation> SalReconDataSumGrade_temp = db.SalaryReconcilation.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == item && e.CurrentMonth == mPayMonth.FirstOrDefault())
                        //    .Include(e => e.EmployeePayroll)
                        //    .Include(e => e.EmployeePayroll.Employee)
                        //        //.Include(e => e.EmployeePayroll.Employee.GeoStruct)
                        //    .Include(e => e.EmployeePayroll.Employee.PayStruct)
                        //     .Include(e => e.EmployeePayroll.Employee.PayStruct.Grade)
                        //        //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Location)
                        //        //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Department)
                        //        //  .Include(e => e.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj)
                        //        //.Include(e => e.EmployeePayroll.Employee.GeoStruct.Location.LocationObj)
                        //        //.Include(e => e.EmployeePayroll.Employee.EmpName)
                        //    .Include(e => e.SalaryHead)
                        //        //.Include(e => e.SalaryHead.Type)

                        //         .ToList();
                        //    if (SalReconDataSumGrade_temp.Count > 0)
                        //    {
                        //        SalReconDataSumGrade.AddRange(SalReconDataSumGrade_temp);
                        //    }
                        //}
                        var SalReconDataSumGrade = db.SalaryReconcilation.Where(z => EmpPayrollIdList.Contains(z.EmployeePayroll.Employee.Id) &&

                            (salheadlist.Any() ? salheadlist.Contains(z.SalaryHead.Code + "-" + z.SalaryHead.Name) : true) && z.CurrentMonth == mPayMonth.FirstOrDefault()).Select(z => new
                            {
                                GradeCode = z.EmployeePayroll.Employee.PayStruct.Grade.Code,
                                Gradename = z.EmployeePayroll.Employee.PayStruct.Grade.Name,
                                CurrentMonth = z.CurrentMonth,
                                PrevMonth = z.PrevMonth,
                                salhead = z.SalaryHead.Code + "-" + z.SalaryHead.Name,
                                CurrentAmt = z.CurrentAmt,
                                PrevAmt = z.PrevAmt,
                                DiffAmt = z.DiffAmt,
                            }).Where(e=>e.CurrentAmt!=0 || e.PrevAmt!=0).ToList();

                        if (SalReconDataSumGrade.Count == 0 || SalReconDataSumGrade == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var salR in SalReconDataSumGrade)
                            {
                                //var salhead = salR.SalaryHead.Name;
                                //if (salheadlist != null && salheadlist.Count() != 0)
                                //{
                                //    foreach (var item2 in salheadlist)
                                //    {
                                //        var salheadcode = salR.SalaryHead.Code;
                                //        var items = item2.Split('-');
                                //        string salcode = items[0];
                                //        string salname = items[1];

                                //        if (salheadcode == salcode)
                                //        {

                                GenericField100 OGenericGeoObjStatement = new GenericField100()
                                {
                                    Fld5 = salR.GradeCode == null ? "" : salR.GradeCode,
                                    Fld6 = salR.Gradename == null ? "" : salR.Gradename, //GradeCode
                                    Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                    Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                    Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                    Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                    Fld14 = salR.PrevMonth.ToString(), //Prev monthD
                                    Fld17 = salR.salhead, //salhead
                                };
                                OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    //var salhead = salR.SalaryHead.Name;
                                //    GenericField100 OGenericGeoObjStatement = new GenericField100()
                                //    {
                                //        //Fld1 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpCode, //EmpCode
                                //        //Fld2 = salR.EmployeePayroll.Employee == null ? "" : salR.EmployeePayroll.Employee.EmpName.FullNameFML, //EmpName
                                //        //Fld3 = salrec.Fld3.ToString(), //CompCode
                                //        //Fld4 = salrec.Fld4.ToString(), //CompName
                                //        Fld5 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Code, //EmpGradeCode
                                //        Fld6 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade == null ? "" : salR.EmployeePayroll.Employee.PayStruct.Grade.Name, //EmpGradeName
                                //        //Fld7 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocCode, //LocCode
                                //        //Fld8 = salrec.Fld8.ToString(), //EmpLocationName
                                //        Fld9 = salR.CurrentAmt.ToString(), //Current month Amt
                                //        Fld10 = salR.PrevAmt.ToString(), //Pre Month Amt
                                //        Fld11 = salR.DiffAmt.ToString(), //Amt Diff
                                //        //Fld12 = salrec.Fld12.ToString(), //Reason
                                //        Fld13 = salR.CurrentMonth.ToString(), //Curr month
                                //        Fld14 = salR.PrevMonth.ToString(), //Prev month
                                //        //Fld15 = salrec.Fld15.ToString(), //Earning
                                //        //Fld16 = salrec.Fld16.ToString(), //LocationObjID
                                //        Fld17 = salhead.ToString(), //salhead
                                //        //Fld18 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Location.LocationObj.LocDesc, //LocationName
                                //        //Fld19 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptCode, //DepartmentCode
                                //        //Fld20 = salR.EmployeePayroll.Employee.GeoStruct == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department == null ? "" : salR.EmployeePayroll.Employee.GeoStruct.Department.DepartmentObj.DeptDesc  //DepartmentDesc
                                //    };
                                //    OGenericPayrollStatement.Add(OGenericGeoObjStatement);
                                //}
                            }
                            return OGenericPayrollStatement;
                        }
                        break;
                }

            }
            return null;
        }

    }
}