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
using P2BUltimate.Controllers.Leave.MainController;
namespace P2BUltimate.Process
{
    public class LeaveReportGen
    {

        public static List<GenericField100> GenerateLeaveReport(int CompanyLeaveId, string mObjectName, List<int> EmpLeaveIdList, DateTime mFromDate, DateTime mToDate, List<string> lvhdlist, List<string> forithead, List<string> lvnamelist, List<string> salheadlist, List<string> SpecialGroupslist, DateTime pfromdate, DateTime ptodate, DateTime LvFromdate, DateTime LvTodate, DateTime ReqDate, Boolean settlementemp, string PayMonth)
        {
            ///////////////////////Master Report////////////////////

            List<GenericField100> OGenericLvTransStatement = new List<GenericField100>();
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;
                switch (mObjectName)
                {
                    case "HIERARCHY":
                        Employee OEmp;
                        var OHIERARCHYLvDetails = new List<Employee>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var Employees = db.Employee
                             .Include(e => e.EmpName)
                                .Include(e => e.FuncStruct)
                                // .Include(e => e.FuncStruct.Job)
                                // .Include(e => e.FuncStruct.JobPosition)
                                .Include(e => e.PayStruct)
                                //.Include(e => e.PayStruct.Grade)
                                //.Include(e => e.PayStruct.Level)
                                //.Include(e => e.PayStruct.JobStatus)
                                .Include(e => e.GeoStruct)
                                // .Include(e => e.GeoStruct.Division)
                                // .Include(e => e.GeoStruct.Location)
                                // .Include(e => e.GeoStruct.Location.LocationObj)
                                // .Include(e => e.GeoStruct.Department)
                                // .Include(e => e.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.GeoStruct.Group)
                                // .Include(e => e.GeoStruct.Unit)
                            .Include(q => q.ReportingStructRights)
                            .Include(q => q.ReportingStructRights.Select(e => e.FuncModules))
                            .Include(q => q.ReportingStructRights.Select(e => e.FuncSubModules))
                            .Include(q => q.ReportingStructRights.Select(e => e.ReportingStruct.GeoFuncList))
                            .Include(q => q.ReportingStructRights.Select(e => e.ReportingStruct.GeoGraphList))
                            .Include(q => q.ReportingStructRights.Select(e => e.ReportingStruct.FuncStruct))
                            .Include(q => q.ReportingStructRights.Select(e => e.AccessRights))
                            .Include(q => q.ReportingStructRights.Select(e => e.AccessRights.ActionName))
                            .Where(e => e.Id == item).AsNoTracking().AsParallel()
                                .FirstOrDefault();
                            if (Employees != null)
                            {
                                OHIERARCHYLvDetails.Add(Employees);
                            }
                        }

                        if (OHIERARCHYLvDetails == null || OHIERARCHYLvDetails.Count() == 0)
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

                            bool sanction = false, approval = false;
                            if (SpecialGroupslist.Count() > 0 && SpecialGroupslist.Count() != 0)
                            {

                                foreach (var item in SpecialGroupslist)
                                {
                                    if (item == "Sanction")
                                    {
                                        sanction = true;
                                    }
                                    if (item == "Approval")
                                    {
                                        approval = true;
                                    }
                                }
                            }

                            List<string> Module = new List<string>();
                            if (salheadlist.Count() != 0)
                            {
                                Module.AddRange(salheadlist);
                            }
                            else
                            {
                                var a = db.ReportingStructRights.Select(b => b.FuncModules.LookupVal).Distinct().ToList();
                                List<string> te = new List<string>();
                                foreach (var item in a)
                                {
                                    Module.Add(item);
                                }
                            }
                            foreach (var l1 in OHIERARCHYLvDetails)
                            {
                                int geoid = l1.GeoStruct.Id;

                                int payid = l1.PayStruct.Id;

                                int funId = l1.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funId, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funId, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funId, Funcdata, 2);
                                foreach (var item1 in Module)
                                {
                                    var ReportingStructRightsdata = l1.ReportingStructRights.Where(e => e.FuncModules.LookupVal.ToUpper() == item1.ToUpper()).GroupBy(e => e.FuncModules.LookupVal).ToList();
                                    foreach (var l2 in ReportingStructRightsdata)
                                    {
                                        string Sanctionecode = "";
                                        string Sanctionname = "";
                                        string Approvalecode = "";
                                        string Approvalname = "";
                                        string FuncModules = "";
                                        string FuncSubModules = "";
                                        foreach (var item in l2)
                                        {

                                            FuncModules = item.FuncModules.LookupVal;
                                            if (item.FuncSubModules != null)
                                            {
                                                FuncSubModules = item.FuncSubModules.LookupVal;
                                            }
                                            switch (FuncModules)
                                            {
                                                case "ELMS":
                                                    if (item.AccessRights != null)
                                                    {
                                                        var ff = l1.GeoStruct != null ? l1.GeoStruct.Id : 0;
                                                        if (ff != 0)
                                                        {
                                                            var EmpCode = ""; var EmpName = "";
                                                            //  Employee loc = new Employee();
                                                            GeoStruct temp = new GeoStruct();
                                                            if (item.AccessRights.ActionName.LookupVal == "Sanction")
                                                            {
                                                                if (item.ReportingStruct != null)
                                                                {
                                                                    if (item.ReportingStruct.GeographicalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoGraphList.LookupVal)
                                                                        {
                                                                            case "LOCATION":
                                                                                temp = db.GeoStruct.Include(a => a.Location.Incharge).Include(a => a.Location.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Location != null && temp.Location.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Location.Incharge.EmpCode;
                                                                                    EmpName = temp.Location.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }

                                                                                break;

                                                                            case "DIVISION":
                                                                                temp = db.GeoStruct.Include(a => a.Division.Incharge).Include(a => a.Division.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Division != null && temp.Division.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Division.Incharge.EmpCode;
                                                                                    EmpName = temp.Division.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "GROUP":
                                                                                temp = db.GeoStruct.Include(a => a.Group.Incharge).Include(a => a.Group.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Group != null && temp.Group.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Group.Incharge.EmpCode;
                                                                                    EmpName = temp.Group.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }

                                                                                break;

                                                                            case "UNIT":
                                                                                temp = db.GeoStruct.Include(a => a.Unit.Incharge).Include(a => a.Unit.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Unit != null && temp.Unit.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Unit.Incharge.EmpCode;
                                                                                    EmpName = temp.Unit.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }

                                                                    }
                                                                    else if (item.ReportingStruct.FunctionalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoFuncList.LookupVal)
                                                                        {
                                                                            case "DEPARTMENT":
                                                                                temp = db.GeoStruct.Include(a => a.Department.Incharge).Include(a => a.Department.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Department != null && temp.Department.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Department.Incharge.EmpCode;
                                                                                    EmpName = temp.Department.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }
                                                                    }
                                                                    else if (item.ReportingStruct.RoleBasedAppl == true)
                                                                    {
                                                                        if (item.ReportingStruct.FuncStruct != null)
                                                                        {
                                                                            var funid = db.FuncStruct.Where(q => q.Id == item.ReportingStruct.FuncStruct.Id).Select(q => q.Id).SingleOrDefault();
                                                                            var loc = db.Employee.Include(q => q.EmpName).Where(q => q.FuncStruct.Id == funid).SingleOrDefault();  //change afterward make it to list
                                                                            EmpName = loc.EmpName.FullNameFML;
                                                                            EmpCode = loc.EmpCode;
                                                                        }
                                                                    }
                                                                    //if (loc != null)
                                                                    //{
                                                                    Sanctionecode = EmpCode;
                                                                    Sanctionname = EmpName;
                                                                    //}
                                                                }
                                                            }
                                                            else if (item.AccessRights.ActionName.LookupVal == "Approval")
                                                            {
                                                                if (item.ReportingStruct != null)
                                                                {
                                                                    if (item.ReportingStruct.GeographicalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoGraphList.LookupVal)
                                                                        {
                                                                            case "LOCATION":
                                                                                temp = db.GeoStruct.Include(a => a.Location.Incharge).Include(a => a.Location.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Location != null && temp.Location.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Location.Incharge.EmpCode;
                                                                                    EmpName = temp.Location.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "DIVISION":
                                                                                temp = db.GeoStruct.Include(a => a.Division.Incharge).Include(a => a.Division.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Division != null && temp.Division.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Division.Incharge.EmpCode;
                                                                                    EmpName = temp.Division.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "GROUP":
                                                                                temp = db.GeoStruct.Include(a => a.Group.Incharge).Include(a => a.Group.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Group != null && temp.Group.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Group.Incharge.EmpCode;
                                                                                    EmpName = temp.Group.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "UNIT":
                                                                                temp = db.GeoStruct.Include(a => a.Unit.Incharge).Include(a => a.Unit.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Unit != null && temp.Unit.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Unit.Incharge.EmpCode;
                                                                                    EmpName = temp.Unit.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }

                                                                    }
                                                                    else if (item.ReportingStruct.FunctionalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoFuncList.LookupVal)
                                                                        {
                                                                            case "DEPARTMENT":
                                                                                temp = db.GeoStruct.Include(a => a.Department.Incharge).Include(a => a.Department.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Department != null && temp.Department.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Department.Incharge.EmpCode;
                                                                                    EmpName = temp.Department.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }
                                                                    }
                                                                    else if (item.ReportingStruct.RoleBasedAppl == true)
                                                                    {
                                                                        if (item.ReportingStruct.FuncStruct != null)
                                                                        {
                                                                            var funid = db.FuncStruct.Where(q => q.Id == item.ReportingStruct.FuncStruct.Id).Select(q => q.Id).SingleOrDefault();
                                                                            var loc = db.Employee.Include(q => q.EmpName).Include(q => q.FuncStruct).Where(q => q.FuncStruct.Id == funid).FirstOrDefault();  //change afterward make it to list
                                                                            EmpName = loc.EmpName.FullNameFML;
                                                                            EmpCode = loc.EmpCode;
                                                                        }
                                                                    }
                                                                    //if (loc != null)
                                                                    //{
                                                                    Approvalecode = EmpCode;
                                                                    Approvalname = EmpName;
                                                                    //}
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case "ETRM":

                                                    break;
                                                case "EPMS":
                                                    if (item.AccessRights != null)
                                                    {
                                                        var ff = l1.GeoStruct != null ? l1.GeoStruct.Id : 0;
                                                        if (ff != 0)
                                                        {
                                                            var EmpCode = ""; var EmpName = "";
                                                            // Employee loc = new Employee();
                                                            GeoStruct temp = new GeoStruct();
                                                            if (item.AccessRights.ActionName.LookupVal == "Sanction")
                                                            {
                                                                if (item.ReportingStruct != null)
                                                                {
                                                                    if (item.ReportingStruct.GeographicalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoGraphList.LookupVal)
                                                                        {
                                                                            case "LOCATION":
                                                                                temp = db.GeoStruct.Include(a => a.Location.Incharge).Include(a => a.Location.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Location != null && temp.Location.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Location.Incharge.EmpCode;
                                                                                    EmpName = temp.Location.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "DIVISION":
                                                                                temp = db.GeoStruct.Include(a => a.Division.Incharge).Include(a => a.Division.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Division != null && temp.Division.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Division.Incharge.EmpCode;
                                                                                    EmpName = temp.Division.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "GROUP":
                                                                                temp = db.GeoStruct.Include(a => a.Group.Incharge).Include(a => a.Group.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Group != null && temp.Group.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Group.Incharge.EmpCode;
                                                                                    EmpName = temp.Group.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "UNIT":
                                                                                temp = db.GeoStruct.Include(a => a.Unit.Incharge).Include(a => a.Unit.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Unit != null && temp.Unit.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Unit.Incharge.EmpCode;
                                                                                    EmpName = temp.Unit.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }

                                                                    }
                                                                    else if (item.ReportingStruct.FunctionalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoFuncList.LookupVal)
                                                                        {
                                                                            case "DEPARTMENT":
                                                                                temp = db.GeoStruct.Include(a => a.Department.Incharge).Include(a => a.Department.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Department != null && temp.Department.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Department.Incharge.EmpCode;
                                                                                    EmpName = temp.Department.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }
                                                                    }
                                                                    else if (item.ReportingStruct.RoleBasedAppl == true)
                                                                    {
                                                                        if (item.ReportingStruct.FuncStruct != null)
                                                                        {
                                                                            var funid = db.FuncStruct.Where(q => q.Id == item.ReportingStruct.FuncStruct.Id).Select(q => q.Id).SingleOrDefault();
                                                                            var loc = db.Employee.Include(q => q.EmpName).Where(q => q.FuncStruct.Id == funid).SingleOrDefault();  //change afterward make it to list
                                                                            EmpName = loc.EmpName.FullNameFML;
                                                                            EmpCode = loc.EmpCode;
                                                                        }
                                                                    }
                                                                    //if (loc != null)
                                                                    //{
                                                                    Sanctionecode = EmpCode;
                                                                    Sanctionname = EmpName;
                                                                    // }
                                                                }
                                                            }
                                                            else if (item.AccessRights.ActionName.LookupVal == "Approval")
                                                            {
                                                                if (item.ReportingStruct != null)
                                                                {
                                                                    if (item.ReportingStruct.GeographicalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoGraphList.LookupVal)
                                                                        {
                                                                            case "LOCATION":
                                                                                temp = db.GeoStruct.Include(a => a.Location.Incharge).Include(a => a.Location.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Location != null && temp.Location.Incharge != null)
                                                                                    if (temp.Location != null && temp.Location.Incharge != null)
                                                                                    {
                                                                                        EmpCode = temp.Location.Incharge.EmpCode;
                                                                                        EmpName = temp.Location.Incharge.EmpName.FullNameFML;
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        EmpCode = "";
                                                                                        EmpName = "";
                                                                                    }
                                                                                break;

                                                                            case "DIVISION":
                                                                                temp = db.GeoStruct.Include(a => a.Division.Incharge).Include(a => a.Division.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Division != null && temp.Division.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Division.Incharge.EmpCode;
                                                                                    EmpName = temp.Division.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "GROUP":
                                                                                temp = db.GeoStruct.Include(a => a.Group.Incharge).Include(a => a.Group.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Group != null && temp.Group.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Group.Incharge.EmpCode;
                                                                                    EmpName = temp.Group.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;

                                                                            case "UNIT":
                                                                                temp = db.GeoStruct.Include(a => a.Unit.Incharge).Include(a => a.Unit.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Unit != null && temp.Unit.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Unit.Incharge.EmpCode;
                                                                                    EmpName = temp.Unit.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }

                                                                    }
                                                                    else if (item.ReportingStruct.FunctionalAppl == true)
                                                                    {
                                                                        switch (item.ReportingStruct.GeoFuncList.LookupVal)
                                                                        {
                                                                            case "DEPARTMENT":
                                                                                temp = db.GeoStruct.Include(a => a.Department.Incharge).Include(a => a.Department.Incharge.EmpName).Where(q => q.Id == ff).SingleOrDefault();
                                                                                if (temp.Department != null && temp.Department.Incharge != null)
                                                                                {
                                                                                    EmpCode = temp.Department.Incharge.EmpCode;
                                                                                    EmpName = temp.Department.Incharge.EmpName.FullNameFML;
                                                                                }
                                                                                else
                                                                                {
                                                                                    EmpCode = "";
                                                                                    EmpName = "";
                                                                                }
                                                                                break;
                                                                        }
                                                                    }
                                                                    else if (item.ReportingStruct.RoleBasedAppl == true)
                                                                    {
                                                                        if (item.ReportingStruct.FuncStruct != null)
                                                                        {
                                                                            var funid = db.FuncStruct.Where(q => q.Id == item.ReportingStruct.FuncStruct.Id).Select(q => q.Id).SingleOrDefault();
                                                                            var loc = db.Employee.Include(q => q.EmpName).Include(q => q.FuncStruct).Where(q => q.FuncStruct.Id == funid).FirstOrDefault();  //change afterward make it to list
                                                                            EmpName = loc.EmpName.FullNameFML;
                                                                            EmpCode = loc.EmpCode;
                                                                        }
                                                                    }
                                                                    //if (loc != null)
                                                                    //{
                                                                    Approvalecode = EmpCode;
                                                                    Approvalname = EmpName;
                                                                    // }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case "EEIS":

                                                    break;
                                            }
                                        }
                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //write data to generic class
                                        {
                                            Fld1 = l1.EmpCode.ToString(),
                                            Fld2 = l1.EmpName.FullNameFML.ToString(),
                                            Fld3 = GeoDataInd.LocDesc,
                                            Fld4 = GeoDataInd.DeptDesc,
                                            Fld5 = PaydataInd.Grade_Name,
                                            Fld6 = FuncdataInd.JobPositionDesc,
                                            Fld7 = Sanctionecode,
                                            Fld8 = Sanctionname,
                                            Fld9 = Approvalecode,
                                            Fld10 = Approvalname,
                                            Fld11 = FuncModules,
                                            Fld12 = FuncSubModules
                                        };
                                        if (comp)
                                        {
                                            OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                        }
                                        if (div)
                                        {
                                            OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                        }
                                        if (loca)
                                        {
                                            OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                        }
                                        if (dept)
                                        {
                                            OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                        }
                                        if (grp)
                                        {
                                            OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                        }
                                        if (unit)
                                        {
                                            OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                        }
                                        if (grade)
                                        {
                                            OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                        }
                                        if (lvl)
                                        {
                                            OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                        }
                                        if (jobstat)
                                        {
                                            OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                        }
                                        if (job)
                                        {
                                            OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                        }
                                        if (jobpos)
                                        {
                                            OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                        }
                                        if (emp)
                                        {
                                            OGenericLvTransObjStatement.Fld88 = l1.EmpName.FullNameFML.ToString();
                                        }
                                        if (sanction)
                                        {
                                            OGenericLvTransObjStatement.Fld87 = Sanctionecode;
                                        }
                                        if (approval)
                                        {
                                            OGenericLvTransObjStatement.Fld86 = Approvalecode;
                                        }
                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    }
                                }
                            }

                            return OGenericLvTransStatement;
                        }
                        return null;
                        break;

                    case "LVDETAILS":
                        var OLvDetails = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvDetailst = db.EmployeeLeave
                                .Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(t => t.LvWFDetails))
                                .Include(e => e.LvNewReq.Select(t => t.LvOrignal))
                                //.Include(e => e.LvNewReq.Select(t => t.WFStatus))
                                .Include(e => e.Employee)
                                .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.PayStruct)
                                .Include(e => e.Employee.FuncStruct)
                                .Where(e => e.Id == item).AsNoTracking().AsParallel()
                           .FirstOrDefault();


                            if (OLvDetailst != null)
                            {
                                OLvDetails.Add(OLvDetailst);
                            }
                        }
                        if (OLvDetails == null || OLvDetails.Count() == 0)
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
                            List<int> Statuslist = new List<int>();
                            if (SpecialGroupslist.Count() > 0 && SpecialGroupslist.Count() != 0)
                            {
                                foreach (var item in SpecialGroupslist)
                                {
                                    if (item == "Applied")
                                    {
                                        Statuslist.Add(0);
                                    }
                                    else if (item == "Sanctioned")
                                    {
                                        Statuslist.Add(1);
                                    }
                                    else if (item == "Sanction Rejected")
                                    {
                                        Statuslist.Add(2);
                                    }
                                    else if (item == "Apporved")
                                    {
                                        Statuslist.Add(3);
                                    }
                                    else if (item == "Approved Rejected")
                                    {
                                        Statuslist.Add(4);
                                    }
                                    else if (item == "Cancel")
                                    {
                                        Statuslist.Add(6);
                                    }
                                    else if (item == "Approved By HRM (M)")
                                    {
                                        Statuslist.Add(5);
                                    }
                                }

                            }
                            else
                            {
                                Statuslist.Add(0);
                                Statuslist.Add(1);
                                Statuslist.Add(2);
                                Statuslist.Add(3);
                                Statuslist.Add(4);
                                Statuslist.Add(5);
                                Statuslist.Add(6);
                            }

                            List<string> lvname = new List<string>();
                            if (salheadlist.Count() != 0)
                            {
                                lvname.AddRange(salheadlist);
                            }
                            else
                            {
                                var a = db.LvHead.Select(e => e.LvName).Distinct().ToList();
                                List<string> te = new List<string>();
                                foreach (var item in a)
                                {
                                    lvname.Add(item);
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

                            foreach (var ca in OLvDetails)
                            {
                                int geoid = ca.Employee.GeoStruct.Id;

                                int payid = ca.Employee.PayStruct.Id;

                                int funid = ca.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                foreach (var item in lvname)
                                {
                                    var status = "";
                                    var details1 = ca.LvNewReq.Where(e => e.LvWFDetails.Count() > 0).ToList();
                                    var details6 = details1.Where(q => q.LeaveHead.LvName == item.ToString() && q.FromDate != null && (q.InputMethod == 0 || q.InputMethod == 1 || q.InputMethod == 2) && q.FromDate.Value.Date >= pfromdate.Date && q.ToDate.Value.Date <= ptodate.Date).Select(e => new
                                    {
                                        id = e.Id,
                                        WFStatus = e.LvWFDetails.LastOrDefault().WFStatus,
                                        lvheadname = e.LeaveHead.LvName,
                                        ReqDate = e.ReqDate.Value.ToShortDateString(),
                                        FromDate = e.FromDate.Value.ToShortDateString(),
                                        ToDate = e.ToDate.Value.ToShortDateString(),
                                        TrClosed = e.TrClosed,
                                        Comment = e.LvWFDetails.LastOrDefault().Comments,

                                    }).ToList();
                                    //var details5 = ca.LvNewReq.Where(q => q.LeaveHead.LvName == item.ToString() && q.FromDate != null && q.InputMethod == 0 && q.FromDate.Value.Date >= pfromdate.Date && q.ToDate.Value.Date <= ptodate.Date).Select(e => new
                                    //{
                                    //    WFStatus = 5,
                                    //    lvheadname = e.LeaveHead != null ? e.LeaveHead.LvName : null,
                                    //    ReqDate = e.ReqDate.Value.ToShortDateString(),
                                    //    FromDate = e.FromDate.Value.ToShortDateString(),
                                    //    ToDate = e.ToDate.Value.ToShortDateString(),
                                    //    TrClosed = e.TrClosed,

                                    //}).ToList();
                                    //var details4 = details6.Union(details5).GroupBy(e => e.lvheadname).ToList();

                                    var details4 = details6.ToList();
                                    foreach (var ca1 in details4)
                                    {
                                        //foreach (var item3 in ca1)
                                        //{

                                        foreach (int item1 in Statuslist)
                                        {
                                            //var lvdata1 = ca1.Where(e => e.WFStatus == item1).ToList();
                                            int wf = 100;
                                            //foreach (var item3 in lvdata1)
                                            //{
                                            wf = ca1.WFStatus;
                                            bool cd = item1.Equals(wf);
                                            if (cd)
                                            {
                                                if (wf == 0)
                                                    status = "Applied";
                                                if (wf == 1)
                                                    status = "Sanctioned";
                                                if (wf == 2)
                                                    status = "Sanction Rejected";
                                                if (wf == 3)
                                                    status = "Apporved";
                                                if (wf == 4)
                                                    status = "Approved Rejected";
                                                if (wf == 5)
                                                    status = "Approved By HRM (M)";
                                                if (wf == 6)
                                                    status = "Cancel";
                                            }
                                            else
                                            {
                                                break;
                                            }
                                            if (status == "Applied" || status == "Sanctioned")
                                            {
                                                if (ca1.TrClosed != false)
                                                {
                                                    break;
                                                }
                                            }

                                            if (status == "Applied" || status == "Sanctioned")
                                            {
                                                int lvreqid = ca1.id;
                                                var Canceldetails = ca.LvNewReq.Where(e => e.LvWFDetails.Count() > 0 && e.LvOrignal_Id == lvreqid && e.FromDate.Value.ToShortDateString() == ca1.FromDate && e.ToDate.Value.ToShortDateString() == ca1.ToDate && e.LeaveHead.LvName == ca1.lvheadname).ToList();
                                                if (Canceldetails.Count() > 0)
                                                {

                                                    break;
                                                }
                                            }

                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML,
                                                Fld3 = ca1.lvheadname.ToString(),
                                                Fld4 = ca1.ReqDate,
                                                Fld5 = ca1.FromDate,
                                                Fld6 = ca1.ToDate,
                                                Fld7 = status,
                                                Fld11 = GeoDataInd.LocDesc + " " + (GeoDataInd.DeptDesc != null ? "/" : " ") + GeoDataInd.DeptDesc,
                                                Fld12 = FuncdataInd.Job_Name,
                                                Fld8 = ca1.Comment == null ? "" : ca1.Comment,

                                            };
                                            if (comp)
                                            {
                                                OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                            }
                                            if (div)
                                            {
                                                OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                            }
                                            if (loca)
                                            {
                                                OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                            }
                                            if (dept)
                                            {
                                                OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                            }
                                            if (grp)
                                            {
                                                OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                            }
                                            if (unit)
                                            {
                                                OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                            }
                                            if (grade)
                                            {
                                                OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                            }
                                            if (lvl)
                                            {
                                                OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                            }
                                            if (jobstat)
                                            {
                                                OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                            }
                                            if (job)
                                            {
                                                OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                            }
                                            if (jobpos)
                                            {
                                                OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                            }
                                            if (emp)
                                            {
                                                OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                            }
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                            // }
                                            //}
                                        }
                                        //}
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }
                        return null;
                        break;

                    case "LVCREDITPOLICY":
                        var OLvCreditPolicyData = db.CompanyLeave
                            .Include(e => e.LvCreditPolicy)
                            .Include(e => e.LvCreditPolicy.Select(r => r.ConvertLeaveHead))
                            .Include(e => e.LvCreditPolicy.Select(r => r.ExcludeLeaveHeads))
                            // .Include(e => e.LvCreditPolicy.Select(r => r.LVConvert))
                            .Include(e => e.LvCreditPolicy.Select(r => r.LvHead))
                            .Include(e => e.LvCreditPolicy.Select(r => r.ConvertLeaveHeadBal))
                            .Include(e => e.LvCreditPolicy.Select(r => r.CreditDate)).AsNoTracking()
                        .Where(d => d.Id == CompanyLeaveId).SingleOrDefault();

                        List<LvCreditPolicy> OLvCreditPolicyDataC = new List<LvCreditPolicy>();
                        if (OLvCreditPolicyData.LvCreditPolicy == null || OLvCreditPolicyData.LvCreditPolicy.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var lvnamehcr = salheadlist.ToList();
                            if (lvnamehcr.Count() != 0)
                            {
                                foreach (var item in lvnamehcr)
                                {
                                    var test = OLvCreditPolicyData.LvCreditPolicy.Where(q => q.LvHead.LvName == item).ToList();
                                    if (test != null)
                                    {
                                        OLvCreditPolicyDataC.AddRange(test);
                                    }
                                }
                            }
                            else
                            {
                                OLvCreditPolicyDataC.AddRange(OLvCreditPolicyData.LvCreditPolicy);
                            }

                            foreach (var ca in OLvCreditPolicyDataC)
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
                                    Fld17 = ca.ConvertLeaveHead != null ? ca.ConvertLeaveHead.LvName.ToString() : "",
                                    Fld18 = ca.ConvertedDays.ToString(),
                                    Fld19 = ca.ConvertLeaveHeadBal != null ? ca.ConvertLeaveHeadBal.LvName.ToString() : "",
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

                        break;
                    case "LVDEBITPOLICY":
                        var OLvDebitPolicyData = db.CompanyLeave
                            .Include(e => e.LvDebitPolicy)
                            .Include(e => e.LvDebitPolicy.Select(r => r.CombinedLvHead))
                            .Include(e => e.LvDebitPolicy.Select(r => r.CombinedLvHead.Select(t => t.LvHead)))
                            .Include(e => e.LvDebitPolicy.Select(r => r.LvHead)).AsNoTracking()
                        .Where(d => d.Id == CompanyLeaveId).SingleOrDefault();

                        List<LvDebitPolicy> OLvDebitPolicyDataC = new List<LvDebitPolicy>();
                        if (OLvDebitPolicyData.LvDebitPolicy == null || OLvDebitPolicyData.LvDebitPolicy.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var lvnamehdr = salheadlist.ToList();
                            if (lvnamehdr.Count() != 0)
                            {
                                foreach (var item in lvnamehdr)
                                {
                                    var test = OLvDebitPolicyData.LvDebitPolicy.Where(q => q.LvHead.LvName == item).ToList();
                                    if (test != null)
                                    {
                                        OLvDebitPolicyDataC.AddRange(test);
                                    }
                                }
                            }
                            else
                            {
                                OLvDebitPolicyDataC.AddRange(OLvDebitPolicyData.LvDebitPolicy);
                            }

                            foreach (var ca in OLvDebitPolicyDataC)
                            {
                                var mCombinedLv = "";
                                if (ca.CombinedLvHead != null && ca.CombinedLvHead.Count() > 0)
                                {
                                    foreach (var ca1 in ca.CombinedLvHead.Select(r => r.LvHead))
                                    {
                                        mCombinedLv = mCombinedLv == "" ? ca1.LvName : mCombinedLv + " : " + ca1.LvName;
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
                            List<LvEncashPolicy> OLvEncashPolicyDataC = new List<LvEncashPolicy>();
                            var lvnamehdr = salheadlist.ToList();
                            if (lvnamehdr.Count() != 0)
                            {
                                foreach (var item in lvnamehdr)
                                {
                                    var test = OLvEncashPolicyData.LvEncashPolicy.Where(q => q.LvHead.LvName == item).ToList();
                                    if (test != null)
                                    {
                                        OLvEncashPolicyDataC.AddRange(test);
                                    }
                                }
                            }
                            else
                            {
                                OLvEncashPolicyDataC.AddRange(OLvEncashPolicyData.LvEncashPolicy);
                            }

                            foreach (var ca in OLvEncashPolicyDataC)
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

                        break;
                    case "LVBANKPOLICY":
                        var OLvBankPolicyData = db.LvBankPolicy.Include(e => e.LvHeadCollection)
                        .ToList();

                        if (OLvBankPolicyData == null || OLvBankPolicyData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            List<LvBankPolicy> OLvBankPolicyDataC = new List<LvBankPolicy>();
                            var lvnamehdr = salheadlist.ToList();
                            if (lvnamehdr.Count() != 0)
                            {

                            }
                            else
                            {
                                OLvBankPolicyDataC.AddRange(OLvBankPolicyData);
                            }

                            foreach (var ca in OLvBankPolicyDataC)
                            {
                                var lvh = ca.LvHeadCollection.ToList();
                                foreach (var item in lvh)
                                {


                                    GenericField100 OGenericLvMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.LeaveBankName,
                                        Fld3 = item.LvCode,
                                        Fld4 = item.LvName,
                                        Fld5 = ca.OccuranceInService.ToString(),
                                        Fld6 = ca.LvMaxDays.ToString(),
                                        Fld7 = ca.LvDebitInCredit.ToString(),


                                    };
                                    OGenericLvTransStatement.Add(OGenericLvMasterObjStatement);
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;
                    case "LVBANK":
                        var OLvBankData = db.LvBank.Include(e => e.LvHeadCollection)
                        .ToList();

                        if (OLvBankData == null || OLvBankData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            List<LvBank> OLvBankDataC = new List<LvBank>();
                            var lvnamehdr = salheadlist.ToList();
                            if (lvnamehdr.Count() != 0)
                            {

                            }
                            else
                            {
                                OLvBankDataC.AddRange(OLvBankData);
                            }

                            foreach (var ca in OLvBankDataC)
                            {
                                var lvh = ca.LvHeadCollection.ToList();
                                foreach (var item in lvh)
                                {


                                    GenericField100 OGenericLvMasterObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Id.ToString(),
                                        Fld2 = ca.LeaveBankName,
                                        Fld3 = item.LvCode,
                                        Fld4 = item.LvName,
                                        Fld5 = ca.CreditDate.Value.ToShortDateString(),
                                        Fld6 = ca.CreditDays.ToString(),
                                        Fld7 = ca.LvDebitInCredit.ToString(),
                                        Fld8 = ca.LvMaxDays.ToString(),
                                        Fld9 = ca.OccuranceInService.ToString(),
                                        Fld10 = ca.OpeningBalance.ToString(),
                                        Fld11 = ca.UtilizedDays.ToString(),

                                    };
                                    OGenericLvTransStatement.Add(OGenericLvMasterObjStatement);
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;
                    case "LVHEAD":
                        List<LvHead> OLvHeadData = new List<LvHead>();
                        var lvnameh = salheadlist.ToList();
                        if (lvnameh.Count() != 0)
                        {
                            foreach (var lvsingle in salheadlist)
                            {
                                var temp = db.LvHead.Include(e => e.LvHeadOprationType).Where(q => q.LvName.ToUpper() == lvsingle.ToUpper()).SingleOrDefault();
                                OLvHeadData.Add(temp);
                            }
                        }
                        else
                        {
                            OLvHeadData = db.LvHead.Include(e => e.LvHeadOprationType).ToList();
                        }
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

                        break;

                    //////////////////////Transaction Reports//////////////////////////
                    case "LVOPENBAL2":
                        var OLvOpenBalData = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvOpenBalData_t = db.EmployeeLeave
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
                                 .Where(e => e.Id == item)
                             .FirstOrDefault();



                            if (OLvOpenBalData_t != null)
                            {
                                OLvOpenBalData.Add(OLvOpenBalData_t);
                            }
                        }
                        if (OLvOpenBalData == null || OLvOpenBalData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            var List_salhead = "";
                            foreach (var ca in OLvOpenBalData)
                            {
                                if (ca.LvOpenBal != null && ca.LvOpenBal.Count() != 0)
                                {
                                    var OLvOpen = ca.LvOpenBal.Where(e => e.LvCalendar.FromDate.Value <= mFromDate && e.LvCalendar.ToDate.Value >= mToDate).ToList();
                                    //  var OLvOpen = ca.LvOpenBal.Where(e => e.LvCalendar.FromDate.Value <= mFromDate && e.LvCalendar.ToDate.Value >= mToDate && List_salhead.Contains(e.)).ToList();
                                    if (OLvOpen != null && OLvOpen.Count() != 0)
                                    {
                                        //Fld1 = ca1.Id.ToString(),
                                        //        Fld2 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                        //        Fld3 = ca1.LeaveHead.LvCode,
                                        //        Fld4 = ca1.LeaveHead.LvName,
                                        //        Fld5 = ca1.OpenBal.ToString(),
                                        foreach (var ca1 in OLvOpen)
                                        {

                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
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
                                            };

                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }

                                    //else
                                    //{
                                    //    //return null;
                                    //}
                                }

                                //else
                                //{
                                //    //return null;
                                //}
                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    case "LEAVENOTCREDIT":
                        List<EmployeeLeave> EmployeeinLvnewreq = new List<EmployeeLeave>();
                        List<LvNewReq> leavenameCheckingInlvnewreq = null;
                        EmployeeinLvnewreq = db.EmployeeLeave
                                             .Include(e => e.LvNewReq)
                                             .Include(e => e.Employee)
                                             .Include(e => e.Employee.GeoStruct)
                                             .Include(e => e.Employee.GeoStruct.Location)
                                             .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                             .Include(e => e.Employee.GeoStruct.Department)
                                             .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                             .Include(e => e.Employee.FuncStruct)
                                             .Include(e => e.Employee.FuncStruct.Job)
                                             .Include(e => e.Employee.PayStruct)
                                             .Include(e => e.Employee.PayStruct.JobStatus)
                                             .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                                             .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                                             .Include(e => e.Employee.ServiceBookDates)
                                             .Include(e => e.Employee.EmpName)
                                             .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                             .Include(e => e.LvNewReq.Select(t => t.LeaveHead)).AsNoTracking()
                                             .ToList();
                        //var DefaultLeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();
                        List<LvHead> LvHeadNameList = new List<Leave.LvHead>();
                        var LvnameSelectList = salheadlist.ToList();
                        if (LvnameSelectList.Count() != 0)
                        {
                            foreach (var item in LvnameSelectList)
                            {
                                var LvHeadList11q = db.LvHead.Where(q => q.LvCode == item).ToList();
                                if (LvHeadList11q != null)
                                {
                                    LvHeadNameList.AddRange(LvHeadList11q);
                                }
                            }
                        }
                        else
                        {
                            LvHeadNameList = db.LvNewReq.Include(t => t.LeaveHead).Where(t => t.LeaveHead != null && t.LvCreditDate != null).Select(t => t.LeaveHead).Distinct().ToList();
                        }
                        if (EmployeeinLvnewreq == null || EmployeeinLvnewreq.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {

                            var CheckRetireornot = EmployeeinLvnewreq.Where(t => t.Employee.ServiceBookDates.ServiceLastDate == null).ToList();
                            foreach (var ca in CheckRetireornot)
                            {
                                foreach (var item in LvHeadNameList)
                                {
                                    leavenameCheckingInlvnewreq = ca.LvNewReq.Where(e => e.LeaveHead.Id == item.Id).ToList();
                                    if (leavenameCheckingInlvnewreq.Count() == 0)
                                    {
                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Id.ToString(),
                                                Fld2 = ca.Employee.EmpCode,

                                                Fld3 = ca.Employee.EmpName.FullNameFML,
                                                Fld4 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal,
                                                Fld5 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal,
                                                Fld6 = ca.Employee.ServiceBookDates == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                                Fld7 = item.LvCode,
                                                Fld11 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null) ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                Fld12 = (ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null) ? ca.Employee.FuncStruct.Job.Name : " ",
                                                Fld13 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null) ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : ""
                                            };
                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    }
                                    else
                                    {
                                        DateTime? lvcrdate = ca.LvNewReq.Where(e => e.LeaveHead.Id == item.Id && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                                        if (lvcrdate != null)
                                        {
                                            DateTime? Lvyearfrom = lvcrdate;
                                            DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                            LvyearTo = LvyearTo.Value.AddYears(1);
                                            leavenameCheckingInlvnewreq = ca.LvNewReq.Where(t => t.LeaveHead.Id == item.Id && t.ReqDate >= Lvyearfrom && t.ReqDate <= LvyearTo).ToList();

                                            if (leavenameCheckingInlvnewreq.Count() == 0)
                                            {

                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                {
                                                    Fld1 = ca.Id.ToString(),
                                                    Fld2 = ca.Employee.EmpCode,
                                                    Fld3 = ca.Employee.EmpName.FullNameFML,
                                                    Fld4 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal,
                                                    Fld5 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal,
                                                    Fld6 = ca.Employee.ServiceBookDates == null ? "" : ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                                    Fld7 = item.LvCode,
                                                    Fld11 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null) ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                    Fld12 = (ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null) ? ca.Employee.FuncStruct.Job.Name : " ",
                                                    Fld13 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null) ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : ""
                                                };
                                                OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;


                    case "LVOPENBAL3":
                        var OLvopb = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvLedger_t = db.EmployeeLeave
                                //.Include(e => e.LvNewReq)
                                //.Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                //.Include(e => e.LvNewReq.Select(r => r.FromStat))
                                //.Include(e => e.LvNewReq.Select(r => r.ToStat))
                                // .Include(e => e.LvOpenBal)
                                //.Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                                //.Include(e => e.LvOpenBal.Select(r => r.LvHead))
                                //.Include(e => e.LvNewReq.Select(r => r.WFStatus))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                // .Include(e => e.Employee.GeoStruct)
                                // .Include(e => e.Employee.PayStruct)
                                // .Include(e => e.Employee.FuncStruct).AsNoTracking()
                                .Where(e => e.Id == item).AsNoTracking().AsParallel().FirstOrDefault();


                            if (OLvLedger_t != null)
                            {
                                OLvLedger_t.Employee.EmpName = db.NameSingle.Find(OLvLedger_t.Employee.EmpName_Id);
                                OLvLedger_t.LvNewReq = db.EmployeeLeave.Where(e => e.Id == OLvLedger_t.Id).Select(r => r.LvNewReq.Where(t => t.ReqDate != null && DbFunctions.TruncateTime(t.ReqDate.Value) >= pfromdate && DbFunctions.TruncateTime(t.ReqDate.Value) <= ptodate).ToList()).FirstOrDefault();
                                foreach (var i in OLvLedger_t.LvNewReq)
                                {
                                    i.ContactNo = db.ContactNumbers.Find(i.ContactNo_Id);
                                    i.LeaveHead = db.LvHead.Find(i.LeaveHead_Id);
                                    i.LeaveCalendar = db.Calendar.Find(i.LeaveCalendar_Id);
                                    i.WFStatus = db.LookupValue.Find(i.WFStatus_Id);
                                    i.FromStat = db.LookupValue.Find(i.FromStat_Id);
                                    i.ToStat = db.LookupValue.Find(i.ToStat_Id);
                                }
                                OLvLedger_t.LvOpenBal = db.EmployeeLeave.Where(e => e.Id == OLvLedger_t.Id).Select(r => r.LvOpenBal.Where(t => DbFunctions.TruncateTime(t.DBTrack.CreatedOn.Value) >= pfromdate && DbFunctions.TruncateTime(t.DBTrack.CreatedOn.Value) <= ptodate).ToList()).FirstOrDefault();
                                foreach (var j in OLvLedger_t.LvOpenBal)
                                {
                                    j.LvCalendar = db.Calendar.Find(j.LvCalendar_Id);
                                    j.LvHead = db.LvHead.Find(j.LvHead_Id);
                                }
                                OLvopb.Add(OLvLedger_t);
                            }
                        }
                        if (OLvopb == null || OLvopb.Count() == 0)
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

                            var vc = db.LookupValue.AsNoTracking().Where(a => forithead.Contains(a.Id.ToString())).AsParallel().ToList();

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

                            foreach (var ca in OLvopb)
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


                                List<LvHead> LvHeadList1 = new List<Leave.LvHead>();
                                var lvnamehdr = salheadlist.ToList();
                                if (lvnamehdr.Count() != 0)
                                {
                                    foreach (var item in lvnamehdr)
                                    {
                                        var LvHeadList11q = db.LvHead.Where(q => q.LvName == item).FirstOrDefault();
                                        if (LvHeadList11q != null)
                                        {
                                            LvHeadList1.Add(LvHeadList11q);
                                        }
                                    }
                                }
                                else
                                {
                                    LvHeadList1 = db.LvHead.ToList();
                                }
                                //  var LvHead = db.LvHead.ToList();
                                foreach (var LvHd in LvHeadList1)
                                {
                                    if (ca.LvNewReq != null && ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id).Count() != 0)
                                    {
                                        //var OLvReq = ca.LvNewReq.Where(e => (e.ReqDate.Value >= mFromDate && e.ReqDate.Value <= mToDate)&& e.LeaveHead.LvName == lvsingle && e.LeaveHead.Id == LvHd.Id &&) || (e.ReqDate.Value >= mFromDate && e.ReqDate.Value >= mToDate)).ToList();

                                        //var lvname = salheadlist.ToList();
                                        List<LvNewReq> OLvReq = new List<LvNewReq>();
                                        //if (lvname.Count() != 0)
                                        //{
                                        //foreach (var lvsingle in lvnamelist)
                                        //{
                                        //OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && e.LeaveHead.LvName == LvHd.LvName &&
                                        //           ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).ToList();
                                        OLvReq = ca.LvNewReq.Where(e => (e.ReqDate != null) && e.LeaveHead.LvName == LvHd.LvName &&
                                                 ((e.ReqDate.Value.Date >= pfromdate.Date && e.ReqDate.Value.Date <= ptodate.Date))).ToList();

                                        if (OLvReq != null && OLvReq.Count() != 0)
                                        {
                                            foreach (var ca1 in OLvReq)
                                            {
                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    //if required any data first check in LVCREDIT case code is same
                                                    //Fld1 = ca1.Id.ToString(),
                                                    Fld2 = ca1.Id.ToString(),
                                                    // Fld3 = ca1.LeaveHead.LvCode,
                                                    Fld4 = ca1.LeaveHead.LvName,
                                                    Fld13 = ca1.OpenBal.ToString(),
                                                    Fld14 = ca1.CloseBal.ToString(),
                                                    Fld50 = ca1.CreditDays.ToString(),
                                                    Fld51 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                    Fld25 = ca1.Narration.ToString(),
                                                    Fld27 = ca.Employee.EmpCode,
                                                    Fld28 = ca.Employee.EmpName.FullNameFML,
                                                    Fld37 = ca1.DebitDays.ToString(),
                                                    //Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                    Fld32 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                    Fld41 = GeoDataInd.FuncStruct_Job_Name,
                                                    Fld42 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                };
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
                                                OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }
                                        // }
                                        // }
                                        //else
                                        //{
                                        //    OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) &&
                                        //       ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).ToList();


                                        //    if (OLvReq != null && OLvReq.Count() != 0)
                                        //    {
                                        //        foreach (var ca1 in OLvReq)
                                        //        {
                                        //            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //            //write data to generic class
                                        //            {
                                        //                //if required any data first check in LVCREDIT case code is same
                                        //                //Fld1 = ca1.Id.ToString(),
                                        //                Fld2 = ca1.Id.ToString(),
                                        //                // Fld3 = ca1.LeaveHead.LvCode,
                                        //                Fld4 = ca1.LeaveHead.LvName,
                                        //                Fld13 = ca1.OpenBal.ToString(),
                                        //                Fld14 = ca1.CloseBal.ToString(),
                                        //                Fld50 = ca1.CreditDays.ToString(),
                                        //                Fld27 = ca.Employee.EmpCode,
                                        //                Fld28 = ca.Employee.EmpName.FullNameFML,
                                        //                Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,

                                        //            }; 
                                        //            if (comp)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                        //            }
                                        //            if (div)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                        //            }
                                        //            if (loca)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                        //            }
                                        //            if (dept)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                        //            }
                                        //            if (grp)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                        //            }
                                        //            if (unit)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                        //            }
                                        //            if (grade)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                        //            }
                                        //            if (lvl)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                        //            }
                                        //            if (jobstat)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                        //            }
                                        //            if (job)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                        //            }
                                        //            if (jobpos)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                        //            }
                                        //            if (emp)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                        //            }
                                        //            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        //        }
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        //var lvname = lvnamelist.ToList();
                                        List<LvOpenBal> LvOpenBal1 = new List<LvOpenBal>();
                                        //if (lvname.Count() != 0)
                                        //{
                                        foreach (var lvsingle in LvHeadList1)
                                        {
                                            LvOpenBal1 = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && e.LvHead.LvName == lvsingle.LvName && (e.DBTrack.CreatedOn.Value.Date >= pfromdate.Date && e.DBTrack.CreatedOn.Value.Date <= ptodate.Date)).ToList();
                                            if (LvOpenBal1 != null && LvOpenBal1.Count() != 0)
                                            {
                                                foreach (var LvOpenBal in LvOpenBal1)
                                                {
                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld4 = LvOpenBal.LvHead.LvName == null ? "" : LvOpenBal.LvHead.LvName.ToString(),
                                                        Fld13 = LvOpenBal.LvOpening.ToString(),
                                                        Fld14 = LvOpenBal.LvClosing.ToString(),
                                                        Fld50 = LvOpenBal != null ? LvOpenBal.LvCredit.ToString() : null,
                                                        Fld25 = "",
                                                        Fld51 = LvOpenBal.DBTrack.CreatedOn != null ? LvOpenBal.DBTrack.CreatedOn.Value.ToShortDateString() : null,
                                                        Fld27 = ca.Employee.EmpCode,
                                                        Fld28 = ca.Employee.EmpName.FullNameFML,
                                                        Fld37 = LvOpenBal.LvUtilized.ToString(),
                                                        //Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                        Fld32 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                        Fld41 = GeoDataInd.FuncStruct_Job_Name,
                                                        Fld42 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name
                                                    };
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
                                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }
                                        }
                                        //}
                                        //else
                                        //{
                                        //    LvOpenBal1 = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id).ToList();

                                        //    if (LvOpenBal1 != null && LvOpenBal1.Count() != 0)
                                        //    {
                                        //        foreach (var LvOpenBal in LvOpenBal1)
                                        //        {
                                        //            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //            //write data to generic class
                                        //            {
                                        //                Fld4 = LvOpenBal.LvHead.LvName == null ? "" : LvOpenBal.LvHead.LvName.ToString(),
                                        //                Fld13 = LvOpenBal.LvOpening.ToString(),
                                        //                Fld14 = LvOpenBal.LvClosing.ToString(),
                                        //                Fld50 = LvOpenBal.LvCredit != null ? LvOpenBal.LvCredit.ToString() : null,
                                        //                Fld27 = ca.Employee.EmpCode,
                                        //                Fld28 = ca.Employee.EmpName.FullNameFML,
                                        //                Fld32 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                        //            };
                                        //            if (comp)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                        //            }
                                        //            if (div)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                        //            }
                                        //            if (loca)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                        //            }
                                        //            if (dept)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                        //            }
                                        //            if (grp)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                        //            }
                                        //            if (unit)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                        //            }
                                        //            if (grade)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                        //            }
                                        //            if (lvl)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                        //            }
                                        //            if (jobstat)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                        //            }
                                        //            if (job)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                        //            }
                                        //            if (jobpos)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                        //            }
                                        //            if (emp)
                                        //            {
                                        //                OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                        //            }
                                        //            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        //        }
                                        //    }
                                        //}
                                    }
                                }
                            }
                            return OGenericLvTransStatement;

                        }

                        break;

                    case "LVNEWREQ2":

                        var OLvNewReqData = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvNewReqData_t = db.EmployeeLeave
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


                                .Where(e => e.Id == item && e.LvNewReq.Count > 0).AsParallel()
                                                   .FirstOrDefault();


                            if (OLvNewReqData_t != null)
                            {
                                OLvNewReqData.Add(OLvNewReqData_t);
                            }
                        }
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
                                    var OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
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
                                                Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                Fld8 = ca1.FromStat.LookupVal.ToUpper(),
                                                Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                Fld10 = ca1.ToStat.LookupVal.ToUpper(),
                                                Fld11 = ca1.DebitDays.ToString(),
                                                Fld12 = ca1.ResumeDate != null ? ca1.ResumeDate.Value.ToShortDateString() : null,
                                                Fld13 = ca1.OpenBal.ToString(),
                                                Fld14 = ca1.CloseBal.ToString(),
                                                Fld15 = ca1.InputMethod == null ? "" : ca1.InputMethod.ToString(),
                                                Fld16 = ca1.LVCount == null ? "" : ca1.LVCount.ToString(),
                                                Fld50 = ca1.CreditDays != null ? ca1.CreditDays.ToString() : null,
                                                Fld17 = ca1.LvOccurances.ToString(),
                                                Fld18 = ca1.PrefixCount == null ? "" : ca1.PrefixCount.ToString(),
                                                Fld19 = ca1.SufixCount == null ? "" : ca1.SufixCount.ToString(),
                                                Fld20 = ca1.TrClosed == null ? "" : ca1.TrClosed.ToString(),
                                                Fld21 = ca1.TrReject == null ? "" : ca1.TrReject.ToString(),
                                                Fld22 = ca1.WFStatus == null ? "" : ca1.WFStatus.LookupVal.ToUpper(),
                                                Fld23 = ca1.IsCancel == null ? "" : ca1.IsCancel.ToString(),
                                                Fld24 = ca1.ContactNo == null ? "" : ca1.ContactNo.FullContactNumbers,
                                                Fld25 = ca1.Reason != null ? ca1.Reason.ToString() : null,


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
                                    else
                                    {

                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;


                    case "LVLEDGER":
                        var OLvLedger = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvLedger_t = db.EmployeeLeave
                                //.Include(e => e.LvNewReq)
                                //  .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                //  .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                //.Include(e => e.LvNewReq.Select(r => r.FromStat))
                                //.Include(e => e.LvNewReq.Select(r => r.ToStat))
                                //.Include(e => e.LvNewReq.Select(r => r.WFStatus))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                // .Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.PayStruct)
                                //  .Include(e => e.Employee.GeoStruct)
                                 .Where(e => e.Employee.Id == item).AsNoTracking()
                                                   .FirstOrDefault();

                            if (OLvLedger_t != null)
                            {
                                OLvLedger_t.Employee.EmpName = db.NameSingle.Find(OLvLedger_t.Employee.EmpName_Id);
                                OLvLedger_t.LvNewReq = db.LvNewReq.Where(e => e.ReqDate != null && DbFunctions.TruncateTime(e.ReqDate.Value) >= pfromdate && DbFunctions.TruncateTime(e.ReqDate.Value) <= ptodate && e.EmployeeLeave_Id == OLvLedger_t.Id).OrderBy(e => e.Id).AsNoTracking().ToList();
                                foreach (var item1 in OLvLedger_t.LvNewReq)
                                {
                                    item1.LeaveHead = db.LvHead.Find(item1.LeaveHead_Id);
                                    item1.FromStat = db.LookupValue.Find(item1.FromStat_Id);
                                    item1.ToStat = db.LookupValue.Find(item1.ToStat_Id);
                                }
                                OLvLedger.Add(OLvLedger_t);
                            }
                        }
                        if (OLvLedger == null || OLvLedger.Count() == 0)
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

                            // List<Utility.ReportClass> GeoData = new List<Utility.ReportClass>();
                            // List<Utility.ReportClass> Paydata = new List<Utility.ReportClass>();
                            // List<Utility.ReportClass> Funcdata = new List<Utility.ReportClass>();

                            // Utility.ReportClass GeoDataInd = new Utility.ReportClass();
                            // Utility.ReportClass PaydataInd = new Utility.ReportClass();
                            // Utility.ReportClass FuncdataInd = new Utility.ReportClass();

                            // GeoData = GetViewData(0);
                            // Paydata = GetViewData(1);
                            // Funcdata = GetViewData(2);
                            Utility.GetOrganizationDataClass GeoDataInd = new Utility.GetOrganizationDataClass();

                            foreach (var ca in OLvLedger)
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
                                //  PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                // FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);

                                if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                                {
                                    var lvname = salheadlist.ToList();
                                    List<LvNewReq> OLvReq = new List<LvNewReq>();
                                    if (lvname.Count() != 0)
                                    {
                                        foreach (var lvsingle in salheadlist)
                                        {

                                            //OLvReq = ca.LvNewReq.Where(e => e.ReqDate != null && e.LeaveHead.LvName == lvsingle &&
                                            //      ((e.ReqDate.Value.Date >= pfromdate.Date && e.ReqDate.Value.Date <= ptodate.Date))).ToList();
                                            OLvReq = ca.LvNewReq.Where(e => e.LeaveHead.LvName == lvsingle).ToList();

                                            if (OLvReq != null && OLvReq.Count() != 0)
                                            {
                                                foreach (var ca1 in OLvReq)
                                                {


                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld4 = ca1.LeaveHead.LvName,
                                                        Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                                        Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                        Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                        Fld8 = ca1.FromStat != null ? ca1.FromStat.LookupVal.ToUpper() : null,
                                                        Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                        Fld10 = ca1.ToStat != null ? ca1.ToStat.LookupVal.ToUpper() : null,
                                                        Fld11 = ca1.DebitDays.ToString(),
                                                        Fld12 = ca1.ResumeDate != null ? ca1.ResumeDate.Value.ToShortDateString() : null,
                                                        Fld13 = ca1.OpenBal.ToString(),
                                                        Fld14 = ca1.CloseBal.ToString(),
                                                        Fld15 = ca1.InputMethod.ToString(),
                                                        Fld16 = ca1.LVCount.ToString(),
                                                        Fld17 = ca1.Narration.ToString(),
                                                        Fld18 = ca1.LvCreditDate != null ? ca1.LvCreditDate.Value.ToShortDateString() : null,
                                                        Fld19 = ca1.LvCreditNextDate != null ? ca1.LvCreditNextDate.Value.ToShortDateString() : null,

                                                        Fld23 = ca1.IsCancel.ToString(),

                                                        Fld50 = ca1.CreditDays.ToString(),

                                                        Fld27 = ca.Employee.EmpCode,
                                                        Fld28 = ca.Employee.EmpName.FullNameFML,
                                                        Fld30 = ca1.Reason != null ? ca1.Reason.ToString() : "",
                                                        Fld32 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,


                                                    };
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
                                                        // OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        //  OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        //  OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        // OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }


                                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).ToList();
                                        OLvReq = ca.LvNewReq.ToList();
                                        if (OLvReq != null && OLvReq.Count() != 0)
                                        {
                                            foreach (var ca1 in OLvReq)
                                            {
                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                //write data to generic class
                                                {

                                                    Fld4 = ca1.LeaveHead.LvName,
                                                    Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                                    Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                    Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                    Fld8 = ca1.FromStat != null ? ca1.FromStat.LookupVal.ToUpper() : null,
                                                    Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                    Fld10 = ca1.ToStat != null ? ca1.ToStat.LookupVal.ToUpper() : null,
                                                    Fld11 = ca1.DebitDays.ToString(),
                                                    Fld12 = ca1.ResumeDate != null ? ca1.ResumeDate.Value.ToShortDateString() : null,
                                                    Fld13 = ca1.OpenBal.ToString(),
                                                    Fld14 = ca1.CloseBal.ToString(),
                                                    Fld15 = ca1.InputMethod.ToString(),
                                                    Fld16 = ca1.LVCount.ToString(),
                                                    Fld17 = ca1.Narration.ToString(),

                                                    Fld23 = ca1.IsCancel.ToString(),
                                                    Fld50 = ca1.CreditDays.ToString(),
                                                    Fld27 = ca.Employee.EmpCode,
                                                    Fld28 = ca.Employee.EmpName.FullNameFML,
                                                    Fld30 = ca1.Reason != null ? ca1.Reason.ToString() : "",
                                                    Fld32 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,


                                                };
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
                                                    // OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    //  OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericLvTransObjStatement.Fld91 = GeoDataInd.PayStruct_JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    //  OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    // OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                                }
                                                if (emp)
                                                {
                                                    OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                }


                                                OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;
                    //aj

                    case "LVCREDIT":
                        var OLvcredit = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvLedger_t = db.EmployeeLeave
                                .Include(e => e.LvNewReq)
                                //   .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                //   .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                //   .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                //   .Include(e => e.LvNewReq.Select(r => r.FromStat))
                                //   .Include(e => e.LvNewReq.Select(r => r.ToStat))
                                //   //.Include(e => e.LvNewReq.Select(r => r.FuncStruct))
                                //   //.Include(e => e.LvNewReq.Select(r => r.GeoStruct))
                                //  .Include(e => e.LvNewReq.Select(r => r.PayStruct))
                                //   .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                                .Include(e => e.Employee)
                                //   .Include(e => e.Employee.EmpName)
                                //   .Include(e => e.Employee.FuncStruct)
                                //   .Include(e => e.Employee.PayStruct)
                                //   .Include(e => e.Employee.GeoStruct)
                                //   //  .Include(e => e.Employee.FuncStruct.Job)

                             ////   .Include(e => e.Employee.FuncStruct.JobPosition)
                                //   //  .Include(e => e.Employee.PayStruct.Grade)

                             //  // .Include(e => e.Employee.PayStruct.Level)

                             // //  .Include(e => e.Employee.PayStruct.JobStatus)
                                //   //   .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                                //   // .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                                //   // .Include(e => e.Employee.GeoStruct.Division)

                             //   //.Include(e => e.Employee.GeoStruct.Location)
                                //   //.Include(e => e.Employee.GeoStruct.Location.LocationObj)

                             //   //.Include(e => e.Employee.GeoStruct.Department)
                                //   //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                             // //  .Include(e => e.Employee.GeoStruct.Group)

                             // //  .Include(e => e.Employee.GeoStruct.Unit)

                              .AsNoTracking().Where(e => e.Employee.Id == item).AsParallel().FirstOrDefault();


                            if (OLvLedger_t != null)
                            {
                                OLvLedger_t.Employee.EmpName = db.NameSingle.Find(OLvLedger_t.Employee.EmpName_Id);
                                OLvLedger_t.LvNewReq = db.LvNewReq.Where(e => e.EmployeeLeave_Id == OLvLedger_t.Id && e.ReqDate != null && DbFunctions.TruncateTime(e.ReqDate.Value) >= pfromdate && DbFunctions.TruncateTime(e.ReqDate.Value) <= ptodate).ToList();
                                foreach (var i in OLvLedger_t.LvNewReq)
                                {
                                    i.ContactNo = db.ContactNumbers.Find(i.ContactNo_Id);
                                    i.LeaveHead = db.LvHead.Find(i.LeaveHead_Id);
                                    i.LeaveCalendar = db.Calendar.Find(i.LeaveCalendar_Id);
                                    i.FromStat = db.LookupValue.Find(i.FromStat_Id);
                                    i.ToStat = db.LookupValue.Find(i.ToStat_Id);
                                    i.PayStruct = db.PayStruct.Find(i.PayStruct_Id);
                                    i.WFStatus = db.LookupValue.Find(i.WFStatus_Id);
                                }
                                OLvcredit.Add(OLvLedger_t);
                            }
                        }
                        if (OLvcredit == null || OLvcredit.Count() == 0)
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

                            foreach (var ca in OLvcredit)
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


                                if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                                {
                                    //var OLvReq = ca.LvNewReq.Where(e => (e.ReqDate.Value >= mFromDate && e.ReqDate.Value <= mToDate) || (e.ReqDate.Value >= mFromDate && e.ReqDate.Value >= mToDate)).ToList();

                                    var lvname = salheadlist.ToList();
                                    List<LvNewReq> OLvReq = new List<LvNewReq>();
                                    if (lvname.Count() != 0)
                                    {
                                        foreach (var lvsingle in salheadlist)
                                        {
                                            //var OLvReq11 = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && e.LeaveHead.LvName == lvsingle &&
                                            //    ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).FirstOrDefault();
                                            var OLvReq11 = ca.LvNewReq.Where(e => (e.FromDate == null && e.ToDate == null) && e.ReqDate != null && e.LeaveHead.LvName == lvsingle &&
                                               ((e.ReqDate.Value.Date >= pfromdate.Date && e.ReqDate.Value.Date <= ptodate.Date))).FirstOrDefault();

                                            if (OLvReq11 != null)
                                            {
                                                OLvReq.Add(OLvReq11);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) &&
                                        //   ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).ToList();
                                        OLvReq = ca.LvNewReq.Where(e => (e.FromDate == null && e.ToDate == null) && e.ReqDate != null &&
                                              ((e.ReqDate.Value.Date >= pfromdate.Date && e.ReqDate.Value.Date <= ptodate.Date))).ToList();

                                    }

                                    if (OLvReq != null && OLvReq.Count() != 0)
                                    {
                                        foreach (var ca1 in OLvReq)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                //Fld1 = ca1.Id.ToString(),
                                                Fld2 = ca1.Id.ToString(),
                                                // Fld3 = ca1.LeaveHead.LvCode,
                                                Fld4 = ca1.LeaveHead.LvCode,
                                                //Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                                //Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                //Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                //Fld8 = ca1.FromStat != null ? ca1.FromStat.LookupVal.ToUpper() : null,
                                                //Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                //Fld10 = ca1.ToStat != null ? ca1.ToStat.LookupVal.ToUpper() : null,
                                                //Fld11 = ca1.DebitDays.ToString(),
                                                //Fld12 = ca1.ResumeDate != null ? ca1.ResumeDate.Value.ToShortDateString() : null,
                                                Fld13 = ca1.OpenBal.ToString(),
                                                Fld14 = ca1.CloseBal.ToString(),
                                                //Fld15 = ca1.InputMethod == null ? "" : ca1.InputMethod.ToString(),
                                                //Fld16 = ca1.LVCount == null ? "" : ca1.LVCount.ToString(),
                                                //Fld17 = ca1.LvOccurances.ToString(),
                                                //Fld18 = ca1.PrefixCount == null ? "" : ca1.PrefixCount.ToString(),
                                                //Fld19 = ca1.SufixCount == null ? "" : ca1.SufixCount.ToString(),
                                                //Fld20 = ca1.TrClosed == null ? "" : ca1.TrClosed.ToString(),
                                                //Fld21 = ca1.TrReject == null ? "" : ca1.TrReject.ToString(),
                                                //Fld22 = ca1.WFStatus == null ? "" : ca1.WFStatus.LookupVal.ToUpper(),
                                                //Fld23 = ca1.IsCancel == null ? "" : ca1.IsCancel.ToString(),
                                                //Fld24 = ca1.ContactNo == null ? "" : ca1.ContactNo.FullContactNumbers,
                                                //Fld25 = ca1.Reason,
                                                Fld50 = ca1.CreditDays.ToString(),
                                                Fld51 = ca1.LvLapsed.ToString(),


                                                // Fld26 = ca.Id.ToString(),
                                                Fld27 = ca.Employee.EmpCode,
                                                Fld28 = ca.Employee.EmpName.FullNameFML,
                                                //Fld32 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                Fld31 = GeoDataInd.GeoStruct_Location_Name + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                Fld32 = GeoDataInd.FuncStruct_Job_Name
                                                //  Fld29 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Id.ToString(),
                                                //Fld19=ca.Employee.GeoStruct.Division==null?"":ca.Employee.GeoStruct.Division.Code,
                                                // Fld30 = ca.Employee.GeoStruct.Division == null ? "" : ca.Employee.GeoStruct.Division.Name,
                                                //Fld31 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.Id.ToString(),
                                                //Fld22=ca.Employee.GeoStruct.Location==null?"":ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                // Fld33 = ca.Employee.GeoStruct.Department == null ? "" : ca.Employee.GeoStruct.Department.Id.ToString(),
                                                //Fld25=ca.Employee.GeoStruct.Department==null?"":ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                                // Fld35 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Id.ToString(),
                                                //Fld28=ca.Employee.GeoStruct.Group==null?"":ca.Employee.GeoStruct.Group.Code,
                                                //Fld36 = ca.Employee.GeoStruct.Group == null ? "" : ca.Employee.GeoStruct.Group.Name,
                                                //Fld37 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Id.ToString(),
                                                //Fld31=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                                                //Fld38 = ca.Employee.GeoStruct.Unit == null ? "" : ca.Employee.GeoStruct.Unit.Name,

                                                //Fld39 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Id.ToString(),
                                                //Fld34=ca.Employee.FuncStruct.Job==null?"":ca.Employee.FuncStruct.Job.Code,
                                                //Fld40 = ca.Employee.FuncStruct.Job == null ? "" : ca.Employee.FuncStruct.Job.Name,

                                                //Fld41 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                                                //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                                //Fld42 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                                //Fld43 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                                //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                                //Fld44 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                                //Fld45 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                                                //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                                //Fld46 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                                //Fld47 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                                                //Fld48 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                //Fld49 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;



                    //aJ
                    case "LVCANCEL":
                        var OLvcanc = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvcanc_t = db.EmployeeLeave
                                //.Include(e => e.LvNewReq)
                                //.Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                //.Include(e => e.LvNewReq.Select(r => r.FromStat))
                                //.Include(e => e.LvNewReq.Select(r => r.ToStat))
                                //.Include(e => e.LvNewReq.Select(r => r.LvOrignal))
                                ////.Include(e => e.LvNewReq.Select(r => r.FuncStruct))
                                ////.Include(e => e.LvNewReq.Select(r => r.GeoStruct))
                                ////.Include(e => e.LvNewReq.Select(r => r.PayStruct))
                                //.Include(e => e.LvNewReq.Select(r => r.WFStatus))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.FuncStruct)
                                ////.Include(e => e.Employee.FuncStruct.Job)
                                //// .Include(e => e.Employee.FuncStruct.JobPosition)
                                //.Include(e => e.Employee.PayStruct)
                                ////.Include(e => e.Employee.PayStruct.Grade)
                                ////.Include(e => e.Employee.PayStruct.Level)
                                ////.Include(e => e.Employee.PayStruct.JobStatus)
                                ////.Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                                ////.Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                                //.Include(e => e.Employee.GeoStruct)
                                ////.Include(e => e.Employee.GeoStruct.Division)
                                ////.Include(e => e.Employee.GeoStruct.Location)
                                ////.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                ////.Include(e => e.Employee.GeoStruct.Department)
                                ////.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                ////.Include(e => e.Employee.GeoStruct.Group)
                                ////.Include(e => e.Employee.GeoStruct.Unit)
                                .AsNoTracking()
                                  .Where(e => e.Id == item).AsParallel()
                                                   .FirstOrDefault();


                            if (OLvcanc_t != null)
                            {
                                OLvcanc_t.Employee.EmpName = db.NameSingle.Find(OLvcanc_t.Employee.EmpName_Id);
                                OLvcanc_t.LvNewReq = db.LvNewReq.Include(r => r.LvOrignal).Where(e => e.EmployeeLeave_Id == OLvcanc_t.Id && e.ReqDate >= pfromdate.Date && e.ReqDate <= ptodate.Date).AsNoTracking().ToList();
                                foreach (var i in OLvcanc_t.LvNewReq)
                                {
                                    i.ContactNo = db.ContactNumbers.Find(i.ContactNo_Id);
                                    i.LeaveHead = db.LvHead.Find(i.LeaveHead_Id);
                                    i.LeaveCalendar = db.Calendar.Find(i.LeaveCalendar_Id);
                                    i.FromStat = db.LookupValue.Find(i.FromStat_Id);
                                    i.ToStat = db.LookupValue.Find(i.ToStat_Id);
                                    i.WFStatus = db.LookupValue.Find(i.WFStatus_Id);
                                }
                                OLvcanc.Add(OLvcanc_t);
                            }
                        }
                        if (OLvcanc == null || OLvcanc.Count() == 0)
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

                            foreach (var ca in OLvcanc)
                            {
                                int? geoid = ca.Employee.GeoStruct_Id;

                                int? payid = ca.Employee.PayStruct_Id;

                                int? funid = ca.Employee.FuncStruct_Id;

                                GeoStruct geostruct = db.GeoStruct.Find(geoid);

                                PayStruct paystruct = db.PayStruct.Find(payid);

                                FuncStruct funstruct = db.FuncStruct.Find(funid);

                                GeoDataInd = ReportRDLCObjectClass.GetOrganizationDataInd(geostruct, paystruct, funstruct, 0);


                                if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                                {
                                    //   var OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).ToList();
                                    var OLvReq = ca.LvNewReq.Where(e => (e.ReqDate != null) && ((e.ReqDate.Value.Date >= pfromdate.Date && e.ReqDate.Value.Date <= ptodate.Date))).ToList();
                                    if (lvhdlist != null && lvhdlist.Count() != 0)
                                    {
                                        foreach (var item in lvhdlist)
                                        {

                                            var olv = OLvReq.Where(a => a.LeaveHead.LvName.ToString() == item.ToString()).ToList();
                                            var ol = olv.Where(a => a.IsCancel.ToString() == "True").ToList();
                                            if (ol != null && ol.Count() != 0)
                                            {
                                                foreach (var ca1 in ol)
                                                {
                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {

                                                        Fld50 = ca1.CreditDays.ToString(),
                                                        Fld23 = ca1.IsCancel == null ? "" : ca1.IsCancel.ToString(),
                                                        Fld27 = ca.Employee.EmpCode,
                                                        Fld28 = ca.Employee.EmpName.FullNameFML,
                                                        Fld4 = ca1.LeaveHead.LvName,
                                                        Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                        Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                        Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                        Fld26 = ca.Id.ToString(),
                                                        Fld71 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                        Fld72 = GeoDataInd.FuncStruct_Job_Name
                                                    };
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
                                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (OLvReq != null && OLvReq.Count() != 0)
                                        {
                                            var OLvReq1 = OLvReq.Where(e => e.IsCancel.ToString() == "True" && (e.ReqDate != null) && ((e.ReqDate.Value.Date >= pfromdate.Date && e.ReqDate.Value.Date <= ptodate.Date))).ToList();
                                            foreach (var ca1 in OLvReq1)
                                            {


                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    //Fld1 = ca1.Id.ToString(),
                                                    Fld2 = ca1.Id.ToString(),
                                                    Fld3 = ca1.LeaveHead.LvCode,
                                                    Fld4 = ca1.LeaveHead.LvName,
                                                    Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                                    Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                    Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                    Fld8 = ca1.FromStat != null ? ca1.FromStat.LookupVal.ToUpper() : null,
                                                    Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                    Fld10 = ca1.ToStat != null ? ca1.ToStat.LookupVal.ToUpper() : null,
                                                    Fld11 = ca1.DebitDays.ToString(),
                                                    Fld12 = ca1.ResumeDate != null ? ca1.ResumeDate.Value.ToShortDateString() : null,
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
                                                    Fld25 = ca1.Reason,
                                                    Fld50 = ca1.CreditDays.ToString(),



                                                    Fld26 = ca.Id.ToString(),
                                                    Fld27 = ca.Employee.EmpCode,
                                                    Fld28 = ca.Employee.EmpName.FullNameFML,

                                                    Fld29 = GeoDataInd.GeoStruct_Division_Id == null ? "" : GeoDataInd.GeoStruct_Division_Id.ToString(),
                                                    //Fld19=ca.Employee.GeoStruct.Division==null?"":ca.Employee.GeoStruct.Division.Code,
                                                    Fld30 = GeoDataInd.GeoStruct_Division_Name == null ? "" : GeoDataInd.GeoStruct_Division_Name,
                                                    Fld31 = GeoDataInd.GeoStruct_Location_Id == null ? "" : GeoDataInd.GeoStruct_Location_Id.ToString(),
                                                    //Fld22=ca.Employee.GeoStruct.Location==null?"":ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                    Fld32 = GeoDataInd.FuncStruct_JobPosition_Name == null ? "" : GeoDataInd.FuncStruct_JobPosition_Name,
                                                    Fld33 = GeoDataInd.GeoStruct_Department_Id == null ? "" : GeoDataInd.GeoStruct_Department_Id.ToString(),
                                                    //Fld25=ca.Employee.GeoStruct.Department==null?"":ca.Employee.GeoStruct.Department.DepartmentObj.DeptCode,
                                                    Fld34 = GeoDataInd.GeoStruct_Department_Name == null ? "" : GeoDataInd.GeoStruct_Department_Name,
                                                    Fld35 = GeoDataInd.PayStruct_Grade_Id == null ? "" : GeoDataInd.PayStruct_Grade_Id.ToString(),
                                                    //Fld28=ca.Employee.GeoStruct.Group==null?"":ca.Employee.GeoStruct.Group.Code,
                                                    Fld36 = GeoDataInd.GeoStruct_Group_Name == null ? "" : GeoDataInd.GeoStruct_Group_Name,

                                                    Fld37 = GeoDataInd.GeoStruct_Unit_Id == null ? "" : GeoDataInd.GeoStruct_Unit_Id.ToString(),
                                                    //Fld31=ca.Employee.GeoStruct.Unit==null?"":ca.Employee.GeoStruct.Unit.Code,
                                                    Fld38 = GeoDataInd.GeoStruct_Unit_Name == null ? "" : GeoDataInd.GeoStruct_Unit_Name,

                                                    Fld39 = GeoDataInd.FuncStruct_Job_Id == null ? "" : GeoDataInd.FuncStruct_Job_Id.ToString(),
                                                    //Fld34=ca.Employee.FuncStruct.Job==null?"":ca.Employee.FuncStruct.Job.Code,
                                                    Fld40 = GeoDataInd.FuncStruct_Job_Name == null ? "" : GeoDataInd.FuncStruct_Job_Name,

                                                    Fld41 = GeoDataInd.FuncStruct_JobPosition_Id == null ? "" : GeoDataInd.FuncStruct_JobPosition_Id.ToString(),
                                                    //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                                    Fld42 = GeoDataInd.FuncStruct_JobPosition_Name == null ? "" : GeoDataInd.FuncStruct_JobPosition_Name,

                                                    Fld43 = GeoDataInd.PayStruct_Grade_Id == null ? "" : GeoDataInd.PayStruct_Grade_Id.ToString(),
                                                    //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                                    Fld44 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,

                                                    Fld45 = GeoDataInd.PayStruct_Level_Id == null ? "" : GeoDataInd.PayStruct_Level_Id.ToString(),
                                                    //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                                    Fld46 = GeoDataInd.PayStruct_Level_Name == null ? "" : GeoDataInd.PayStruct_Level_Name,

                                                    //Fld47 = GeoDataInd.FuncStruct_JobPosition_Id== null ? "" : GeoDataInd.FuncStruct_JobPosition_Id.ToString(),
                                                    //Fld48 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                    //Fld49 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),
                                                    Fld48 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_JobStatus_Id == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpStatus == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpStatus,
                                                    Fld49 = GeoDataInd == null ? "" : GeoDataInd.PayStruct_JobStatus_Id == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpActingStatus == null ? "" : GeoDataInd.PayStruct_JobStatus_EmpActingStatus,
                                                    Fld71 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                    Fld72 = GeoDataInd.FuncStruct_Job_Name

                                                };
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
                                                OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    case "LVCANCELREQ":
                        var OLvCancelReqData = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvCancelReqData_t = db.EmployeeLeave
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


                                .Where(e => e.Id == item)
                                                   .FirstOrDefault();


                            if (OLvCancelReqData_t != null)
                            {
                                OLvCancelReqData.Add(OLvCancelReqData_t);
                            }
                        }
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
                                    var OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && ((e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value <= mToDate))).ToList();
                                    if (OLvReq != null && OLvReq.Count() != 0)
                                    {
                                        List<LvNewReq> LvHeadList1 = new List<LvNewReq>();
                                        var lvnamehdr = salheadlist.ToList();

                                        if (lvnamehdr.Count() != 0)
                                        {
                                            foreach (var item in lvnamehdr)
                                            {
                                                var LvHeadList11q = OLvReq.Where(a => a.LeaveHead.LvName.ToString() == item.ToString()).ToList();
                                                if (LvHeadList11q != null)
                                                {
                                                    LvHeadList1.AddRange(LvHeadList11q);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LvHeadList1 = OLvReq;
                                        }

                                        foreach (var ca1 in LvHeadList1)
                                        {


                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                //Fld1 = ca1.Id.ToString(),
                                                Fld2 = ca1.Id.ToString(),
                                                Fld3 = ca1.LeaveHead.LvCode,
                                                Fld4 = ca1.LeaveHead.LvName,
                                                Fld5 = ca1.LeaveCalendar == null ? "" : ca1.LeaveCalendar.FullDetails,
                                                Fld6 = ca1.ReqDate != null ? ca1.ReqDate.Value.ToShortDateString() : null,
                                                Fld7 = ca1.FromDate != null ? ca1.FromDate.Value.ToShortDateString() : null,
                                                Fld8 = ca1.FromStat.LookupVal.ToUpper(),
                                                Fld9 = ca1.ToDate != null ? ca1.ToDate.Value.ToShortDateString() : null,
                                                Fld10 = ca1.ToStat.LookupVal.ToUpper(),
                                                Fld11 = ca1.DebitDays.ToString(),
                                                Fld12 = ca1.ResumeDate != null ? ca1.ResumeDate.Value.ToShortDateString() : null,
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

                        break;

                    case "LVOPENBAL":
                        var OLvBalanceData = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvBalanceData_t = db.EmployeeLeave
                                //  .Include(e => e.LvNewReq)
                                //  .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                //  .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                //  .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                .Include(e => e.Employee)
                                //  .Include(e => e.Employee.EmpName)
                                //   .Include(e => e.Employee.FuncStruct)
                                //  //  .Include(e => e.Employee.FuncStruct.Job)

                              //  //.Include(e => e.Employee.FuncStruct.JobPosition)
                                //  .Include(e => e.Employee.PayStruct)
                                //  //  .Include(e => e.Employee.PayStruct.Grade)

                              // // .Include(e => e.Employee.PayStruct.Level)

                              // // .Include(e => e.Employee.PayStruct.JobStatus)
                                //  // .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                                //  // .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                                //   .Include(e => e.Employee.GeoStruct)
                                //  //.Include(e => e.Employee.GeoStruct.Division)

                              //  //.Include(e => e.Employee.GeoStruct.Location)
                                //  //.Include(e => e.Employee.GeoStruct.Location.LocationObj)

                              ////  .Include(e => e.Employee.GeoStruct.Department)
                                //  // .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                              //  //.Include(e => e.Employee.GeoStruct.Group)

                              // // .Include(e => e.Employee.GeoStruct.Unit)

                              //  .Include(e => e.LvOpenBal)
                                //  .Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                                //  .Include(e => e.LvOpenBal.Select(r => r.LvHead))

                                .Where(e => e.Id == item).AsParallel()
                           .FirstOrDefault();


                            if (OLvBalanceData_t != null)
                            {
                                OLvBalanceData_t.Employee.EmpName = db.NameSingle.Find(OLvBalanceData_t.Employee.EmpName_Id);
                                OLvBalanceData_t.LvNewReq = db.EmployeeLeave.Where(e => e.Id == OLvBalanceData_t.Id).Select(r => r.LvNewReq.Where(t => t.ReqDate != null && DbFunctions.TruncateTime(t.ReqDate.Value) <= ReqDate).OrderByDescending(t => t.Id).ToList()).AsNoTracking().FirstOrDefault();
                                foreach (var b in OLvBalanceData_t.LvNewReq)
                                {
                                    b.ContactNo = db.ContactNumbers.Find(b.ContactNo_Id);
                                    b.LeaveHead = db.LvHead.Find(b.LeaveHead_Id);
                                    b.LeaveCalendar = db.Calendar.Find(b.LeaveCalendar_Id);
                                }
                                OLvBalanceData_t.LvOpenBal = db.EmployeeLeave.Where(e => e.Id == OLvBalanceData_t.Id).Select(r => r.LvOpenBal.Where(t => t.DBTrack.CreatedOn != null && DbFunctions.TruncateTime(t.DBTrack.CreatedOn.Value) <= ReqDate).OrderByDescending(t => t.Id).ToList()).AsNoTracking().FirstOrDefault();
                                foreach (var c in OLvBalanceData_t.LvOpenBal)
                                {
                                    c.LvCalendar = db.Calendar.Find(c.LvCalendar_Id);
                                    c.LvHead = db.LvHead.Find(c.LvHead_Id);
                                }
                                OLvBalanceData.Add(OLvBalanceData_t);
                            }
                        }
                        List<LvHead> LvHeadList12 = new List<Leave.LvHead>();

                        var lvnamehdr12 = salheadlist.ToList();
                        if (lvnamehdr12.Count() != 0)
                        {
                            foreach (var item in lvnamehdr12)
                            {
                                var LvHeadList11q = db.LvHead.Where(q => q.LvName == item).FirstOrDefault();
                                if (LvHeadList11q != null)
                                {
                                    LvHeadList12.Add(LvHeadList11q);
                                }
                            }
                        }
                        else
                        {
                            LvHeadList12 = db.LvHead.ToList();
                        }


                        // var LvHeadList = db.LvHead.ToList();

                        if (OLvBalanceData == null || OLvBalanceData.Count() == 0)
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

                            foreach (var ca in OLvBalanceData.Where(e => e.LvNewReq.Count() > 0 || e.LvOpenBal.Count() > 0).ToList())
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

                                foreach (var LvHd in LvHeadList12)
                                {
                                    if (ca.LvNewReq.Count() != 0 && ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id).Count() != 0)
                                    {
                                        //var OLvReq1 = ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id && e.ReqDate != null && e.ReqDate <= ReqDate).OrderByDescending(e => e.Id).FirstOrDefault();
                                        var OLvReq1 = ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id && e.ReqDate != null).OrderByDescending(e => e.Id).FirstOrDefault();
                                        if (OLvReq1 != null)
                                        {

                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            {
                                                Fld3 = OLvReq1.LeaveHead.LvCode,
                                                Fld4 = OLvReq1.LeaveHead.LvName,
                                                Fld5 = OLvReq1.OpenBal.ToString(),
                                                Fld6 = OLvReq1.OpenBal.ToString(),
                                                Fld7 = OLvReq1.CloseBal.ToString(),
                                                Fld9 = OLvReq1.LvCreditDate != null ? OLvReq1.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld10 = OLvReq1.LvOccurances.ToString(),
                                                //Fld11 = OLvReq.ToString(),
                                                Fld11 = OLvReq1.LvLapsed.ToString(),
                                                Fld12 = OLvReq1.CreditDays.ToString(),
                                                Fld13 = OLvReq1.DebitDays.ToString(),
                                                Fld14 = OLvReq1.ReqDate.Value.ToShortDateString(),
                                                Fld39 = ca.Employee.EmpCode,
                                                Fld40 = ca.Employee.EmpName.FullNameFML,
                                                Fld41 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                // Fld42 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,                     
                                                // Fld43 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                // Fld44 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                // Fld45 = ("name"),
                                                Fld42 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name

                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }


                                    }
                                    else
                                    {
                                        // var LvOpenBal1 = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && e.DBTrack.CreatedOn != null && e.DBTrack.CreatedOn <= ReqDate).OrderByDescending(e => e.Id).FirstOrDefault();
                                        var LvOpenBal1 = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && e.DBTrack.CreatedOn != null).OrderByDescending(e => e.Id).FirstOrDefault();
                                        if (LvOpenBal1 != null)
                                        {

                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld3 = LvOpenBal1.LvHead.LvCode,
                                                Fld4 = LvOpenBal1.LvHead.LvName,
                                                Fld5 = LvOpenBal1.LvOpening.ToString(),
                                                Fld6 = LvOpenBal1.LvOpening.ToString(),
                                                Fld7 = LvOpenBal1.LvClosing.ToString(),
                                                Fld9 = LvOpenBal1.LvCreditDate != null ? LvOpenBal1.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld10 = LvOpenBal1.LvOccurances.ToString(),
                                                Fld11 = LvOpenBal1.LvLapseBal.ToString(),
                                                Fld14 = LvOpenBal1.DBTrack.CreatedOn.Value.ToShortDateString(),
                                                Fld39 = ca.Employee.EmpCode,
                                                Fld40 = ca.Employee.EmpName.FullNameFML,
                                                Fld41 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                //Fld41 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                                // Fld42 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                // Fld43 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                // Fld44 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                Fld42 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }


                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }
                        break;
                    case "LVNEWREQ":
                        //var OLvBalanceData = new List<EmployeeLeave>();
                        //foreach (var item in EmpLeaveIdList)
                        //{
                        //    var OLvBalanceData_t = db.EmployeeLeave
                        //        .Include(e => e.LvNewReq)
                        //        .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                        //        .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        //        .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                        //        .Include(e => e.Employee)
                        //        .Include(e => e.Employee.EmpName)
                        //         .Include(e => e.Employee.FuncStruct)
                        //        //  .Include(e => e.Employee.FuncStruct.Job)

                        //        //.Include(e => e.Employee.FuncStruct.JobPosition)
                        //        .Include(e => e.Employee.PayStruct)
                        //        //  .Include(e => e.Employee.PayStruct.Grade)

                        //       // .Include(e => e.Employee.PayStruct.Level)

                        //       // .Include(e => e.Employee.PayStruct.JobStatus)
                        //        // .Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                        //        // .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                        //         .Include(e => e.Employee.GeoStruct)
                        //        //.Include(e => e.Employee.GeoStruct.Division)

                        //        //.Include(e => e.Employee.GeoStruct.Location)
                        //        //.Include(e => e.Employee.GeoStruct.Location.LocationObj)

                        //      //  .Include(e => e.Employee.GeoStruct.Department)
                        //        // .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)

                        //        //.Include(e => e.Employee.GeoStruct.Group)

                        //       // .Include(e => e.Employee.GeoStruct.Unit)

                        //        .Include(e => e.LvOpenBal)
                        //        .Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                        //        .Include(e => e.LvOpenBal.Select(r => r.LvHead))

                        //        .Where(e => e.Id == item).AsParallel()
                        //   .FirstOrDefault();


                        //    if (OLvBalanceData_t != null)
                        //    {
                        //        OLvBalanceData.Add(OLvBalanceData_t);
                        //    }
                        //}
                        List<int> LvHeadListForLvreq = new List<int>();

                        var Lvheadlistlvreq = salheadlist.ToList();
                        if (Lvheadlistlvreq.Count() != 0)
                        {
                            foreach (var item in Lvheadlistlvreq)
                            {
                                var LvHeadList11q = db.LvHead.Where(q => q.LvName == item).Select(q => q.Id).FirstOrDefault();
                                if (LvHeadList11q != null)
                                {
                                    LvHeadListForLvreq.Add(LvHeadList11q);
                                }
                            }
                        }
                        else
                        {
                            LvHeadListForLvreq = db.LvHead.Select(e => e.Id).ToList();
                        }
                        var OLvReqData_t1 = db.EmployeeLeave.Where(e => EmpLeaveIdList.Contains(e.Id)).Select(t => new
                        {
                            EmpCode = t.Employee.EmpCode,
                            EmpName = t.Employee.EmpName.FullNameFML,
                            FuncStruct = t.Employee.FuncStruct.Id,
                            PayStruct = t.Employee.PayStruct.Id,
                            GeoStruct = t.Employee.GeoStruct.Id,
                            LvNewReq = t.LvNewReq.Where(x => DbFunctions.TruncateTime(x.ReqDate.Value) >= pfromdate.Date && DbFunctions.TruncateTime(x.ReqDate.Value) <= ptodate.Date && LvHeadListForLvreq.Contains(x.LeaveHead.Id) && x.FromDate != null && x.ToDate != null && x.IsCancel == false && x.WFStatus.LookupVal != "2").Select(y => new
                               {
                                   ReqDate = y.ReqDate,
                                   FromDate = y.FromDate,
                                   ToDate = y.ToDate,
                                   ResumeDate = y.ResumeDate,
                                   IsCancel = y.IsCancel,
                                   ContactNo = y.ContactNo,
                                   Reason = y.Reason,
                                   CreditDays = y.CreditDays,
                                   DebitDays = y.DebitDays,
                                   LeaveHead = y.LeaveHead.Id,
                                   LeaveCode = y.LeaveHead.LvCode,
                                   LeaveName = y.LeaveHead.LvName,
                                   OpenBal = y.OpenBal,
                                   CloseBal = y.CloseBal,
                                   LvCreditDate = y.LvCreditDate,
                                   LvOccurances = y.LvOccurances,
                                   LvLapsed = y.LvLapsed,
                                   Path=y.Path
                               }).ToList(),
                            LvOpenBal = t.LvOpenBal.Where(x => DbFunctions.TruncateTime(x.DBTrack.CreatedOn.Value) >= pfromdate.Date && DbFunctions.TruncateTime(x.DBTrack.CreatedOn.Value) <= ptodate.Date && LvHeadListForLvreq.Contains(x.LvHead.Id)).Select(y => new
                               {
                                   LvHead = y.LvHead.Id,
                                   LeaveCode = y.LvHead.LvCode,
                                   LeaveName = y.LvHead.LvName,
                                   LvCreditDate = y.LvCreditDate,
                                   LvOccurances = y.LvOccurances,
                                   LvOpening = y.LvOpening,
                                   LvClosing = y.LvClosing,
                                   LvLapsed = y.LvLapseBal,
                                   ReqDate = y.DBTrack.CreatedOn
                               }).ToList()
                        }).AsNoTracking().ToList();
                        var LvHeadList = db.LvHead.ToList();
                        if (OLvReqData_t1 == null || OLvReqData_t1.Count() == 0)
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

                            foreach (var ca in OLvReqData_t1)
                            {
                                int geoid = ca.GeoStruct;

                                int payid = ca.PayStruct;

                                int funid = ca.FuncStruct;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                foreach (var LvHd in LvHeadList)
                                {
                                    if (ca.LvNewReq.Count() != 0)
                                    {
                                        var OLvReq1 = ca.LvNewReq.Where(e => e.LeaveHead == LvHd.Id).ToList();
                                        if (OLvReq1.Count > 0)
                                        {
                                            foreach (var OLvReq in OLvReq1)
                                            {
                                                GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                //write data to generic class
                                                {
                                                    //  Fld2 = OLvReq.Id.ToString(),

                                                    Fld3 = OLvReq.LeaveCode,
                                                    Fld4 = OLvReq.LeaveName,
                                                    Fld5 = OLvReq.OpenBal.ToString(),
                                                    Fld6 = OLvReq.OpenBal.ToString(),
                                                    Fld7 = OLvReq.CloseBal.ToString(),
                                                    Fld9 = OLvReq.CreditDays.ToString(),
                                                    Fld10 = OLvReq.LvOccurances.ToString(),
                                                    Fld27 = OLvReq.ReqDate != null ? OLvReq.ReqDate.Value.ToShortDateString() : null,
                                                    Fld28 = OLvReq.FromDate != null ? OLvReq.FromDate.Value.ToShortDateString() : null,
                                                    Fld29 = OLvReq.ToDate != null ? OLvReq.ToDate.Value.ToShortDateString() : null,
                                                    Fld11 = OLvReq.DebitDays.ToString(),
                                                    Fld12 = OLvReq.ResumeDate != null ? OLvReq.ResumeDate.Value.ToShortDateString() : null,
                                                    Fld13 = OLvReq.OpenBal.ToString(),
                                                    Fld14 = OLvReq.CloseBal.ToString(),
                                                    Fld50 = OLvReq.CreditDays != null ? OLvReq.CreditDays.ToString() : null,
                                                    Fld17 = OLvReq.LvOccurances.ToString(),
                                                    Fld23 = OLvReq.IsCancel == null ? "" : OLvReq.IsCancel.ToString(),
                                                    Fld24 = OLvReq.ContactNo == null ? "" : OLvReq.ContactNo.FullContactNumbers,
                                                    Fld25 = OLvReq.Reason != null ? OLvReq.Reason.ToString() : null,
                                                    Fld67=  !string.IsNullOrEmpty(OLvReq.Path) ? "Yes" : "No",



                                                    //Fld15 = ca.Id.ToString(),
                                                    // Fld32 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(), 
                                                    Fld39 = ca.EmpCode,
                                                    Fld40 = ca.EmpName,
                                                    Fld41 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                                    //Fld42 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,

                                                    //Fld43 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                    //Fld44 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                    //Fld45 = ("name"),
                                                    Fld42 = GeoDataInd.LocDesc + " " + (GeoDataInd.DeptDesc != null ? "/" : " ") + GeoDataInd.DeptDesc,

                                                };
                                                if (comp)
                                                {
                                                    OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                }
                                                if (div)
                                                {
                                                    OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                }
                                                if (loca)
                                                {
                                                    OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                }
                                                if (dept)
                                                {
                                                    OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                }
                                                if (grp)
                                                {
                                                    OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                }
                                                if (unit)
                                                {
                                                    OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                                }
                                                if (grade)
                                                {
                                                    OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                                }
                                                if (lvl)
                                                {
                                                    OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                                }
                                                if (jobstat)
                                                {
                                                    OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                                }
                                                if (job)
                                                {
                                                    OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                                }
                                                if (jobpos)
                                                {
                                                    OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                                }
                                                if (emp)
                                                {
                                                    OGenericLvTransObjStatement.Fld88 = ca.EmpName;
                                                }
                                                OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                            }
                                        }

                                    }
                                    else
                                    {

                                        //var LvOpenBal1 = ca.LvOpenBal.ToList();
                                        //if (LvOpenBal1 != null && LvOpenBal1.Count() != 0)
                                        //{
                                        //    foreach (var LvOpenBal in LvOpenBal1)
                                        //    {
                                        //        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                        //        //write data to generic class
                                        //        {
                                        //            Fld3 = LvOpenBal.LeaveCode,
                                        //            Fld4 = LvOpenBal.LeaveName,
                                        //            Fld5 = LvOpenBal.LvOpening.ToString(),
                                        //            Fld6 = LvOpenBal.LvOpening.ToString(),
                                        //            Fld7 = LvOpenBal.LvClosing.ToString(),
                                        //            Fld9 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                        //            Fld10 = LvOpenBal.LvOccurances.ToString(),
                                        //            Fld11 = LvOpenBal.LvLapsed.ToString(),

                                        //            Fld27 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                        //            Fld28 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                        //            Fld29 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,

                                        //            Fld13 = LvOpenBal.LvOpening.ToString(),
                                        //            Fld14 = LvOpenBal.LvOpening.ToString(),
                                        //            Fld17 = LvOpenBal.LvOccurances.ToString(),
                                        //            Fld39 = ca.EmpCode,
                                        //            Fld40 = ca.EmpName,
                                        //            Fld41 = PaydataInd.Grade_Name == null ? "" : PaydataInd.Grade_Name,
                                        //            Fld42 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                        //            Fld43 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                        //            Fld44 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,

                                        //        }; if (comp)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                        //        }
                                        //        if (div)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                        //        }
                                        //        if (loca)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                        //        }
                                        //        if (dept)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                        //        }
                                        //        if (grp)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                        //        }
                                        //        if (unit)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                        //        }
                                        //        if (grade)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                        //        }
                                        //        if (lvl)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                        //        }
                                        //        if (jobstat)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                        //        }
                                        //        if (job)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                        //        }
                                        //        if (jobpos)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                        //        }
                                        //        if (emp)
                                        //        {
                                        //            OGenericLvTransObjStatement.Fld88 = ca.EmpName;
                                        //        }
                                        //        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        //    }
                                        //}
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;
                    //AJ
                    case "LEAVEINTRA":
                        var OLvBalancData = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvBalanceData_t = db.EmployeeLeave
                                .Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
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

                                .Include(e => e.LvOpenBal)
                                .Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                                .Include(e => e.LvOpenBal.Select(r => r.LvHead))

                                .Where(e => e.Id == item)
                           .FirstOrDefault();


                            if (OLvBalanceData_t != null)
                            {
                                OLvBalancData.Add(OLvBalanceData_t);
                            }
                        }
                        var LvHeadList11 = db.LvHead.ToList();
                        if (OLvBalancData == null || OLvBalancData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OLvBalancData)
                            {
                                foreach (var LvHd in LvHeadList11)
                                {
                                    if (ca.LvNewReq.Count() != 0 && ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id).Count() != 0)
                                    {

                                        var OLvReq = ca.LvNewReq.Where(e => (e.FromDate != null && e.ToDate != null) && (e.LeaveHead.Id == LvHd.Id && (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate))).LastOrDefault();

                                        if (OLvReq != null)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld2 = OLvReq.Id.ToString(),


                                                Fld3 = OLvReq.LeaveHead.LvCode == null ? "" : OLvReq.LeaveHead.LvCode.ToString(),
                                                Fld4 = OLvReq.LeaveHead.LvName == null ? "" : OLvReq.LeaveHead.LvName.ToString(),
                                                // Fld5 = OLvReq.LeaveCalendar == null ? "" : OLvReq.LeaveCalendar.FullDetails,
                                                // Fld14 = OLvReq.CloseBal.ToString(),

                                                Fld5 = OLvReq.OpenBal.ToString(),
                                                Fld6 = OLvReq.OpenBal.ToString(),
                                                Fld7 = OLvReq.CloseBal.ToString(),
                                                Fld9 = OLvReq.CreditDays.ToString(),
                                                Fld10 = OLvReq.LvOccurances.ToString(),
                                                //Fld11 = OLvReq.ToString(),

                                                Fld26 = OLvReq.LeaveCalendar == null ? "" : OLvReq.LeaveCalendar.FullDetails,
                                                Fld27 = OLvReq.ReqDate != null ? OLvReq.ReqDate.Value.ToShortDateString() : null,
                                                Fld28 = OLvReq.FromDate != null ? OLvReq.FromDate.Value.ToShortDateString() : null,
                                                Fld29 = OLvReq.ToDate != null ? OLvReq.ToDate.Value.ToShortDateString() : null,
                                                Fld30 = OLvReq.FromStat != null ? OLvReq.FromStat.LookupVal.ToUpper() : null,
                                                Fld31 = OLvReq.ToStat != null ? OLvReq.ToStat.LookupVal.ToUpper() : null,
                                                Fld11 = OLvReq.DebitDays.ToString(),
                                                Fld12 = OLvReq.ResumeDate != null ? OLvReq.ResumeDate.Value.ToShortDateString() : null,
                                                Fld13 = OLvReq.OpenBal.ToString(),
                                                Fld14 = OLvReq.CloseBal.ToString(),
                                                Fld15 = OLvReq.InputMethod == null ? "" : OLvReq.InputMethod.ToString(),
                                                Fld16 = OLvReq.LVCount == null ? "" : OLvReq.LVCount.ToString(),
                                                Fld50 = OLvReq.CreditDays != null ? OLvReq.CreditDays.ToString() : null,
                                                Fld17 = OLvReq.LvOccurances.ToString(),
                                                Fld18 = OLvReq.PrefixCount == null ? "" : OLvReq.PrefixCount.ToString(),
                                                Fld19 = OLvReq.SufixCount == null ? "" : OLvReq.SufixCount.ToString(),
                                                Fld20 = OLvReq.TrClosed == null ? "" : OLvReq.TrClosed.ToString(),
                                                Fld21 = OLvReq.TrReject == null ? "" : OLvReq.TrReject.ToString(),
                                                Fld22 = OLvReq.WFStatus == null ? "" : OLvReq.WFStatus.LookupVal.ToUpper(),
                                                Fld23 = OLvReq.IsCancel == null ? "" : OLvReq.IsCancel.ToString(),
                                                Fld24 = OLvReq.ContactNo == null ? "" : OLvReq.ContactNo.FullContactNumbers,
                                                Fld25 = OLvReq.Reason != null ? OLvReq.Reason.ToString() : null,



                                                //Fld15 = ca.Id.ToString(),
                                                // Fld32 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                                Fld39 = ca.Employee.EmpCode,
                                                Fld40 = ca.Employee.EmpName.FullNameFML,
                                                Fld41 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,
                                                Fld42 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,

                                                Fld43 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                Fld44 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                Fld45 = ("name"),

                                            };
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }
                                    else
                                    {

                                        var LvOpenBal = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && (e.LvCalendar.FromDate.Value >= mFromDate && e.LvCalendar.ToDate.Value <= mToDate)).SingleOrDefault();
                                        if (LvOpenBal != null)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                //Fld1 = ca1.Id.ToString(),
                                                Fld2 = LvOpenBal.Id.ToString(),

                                                Fld3 = LvOpenBal.LvHead.LvCode == null ? "" : LvOpenBal.LvHead.LvCode.ToString(),
                                                Fld4 = LvOpenBal.LvHead.LvName == null ? "" : LvOpenBal.LvHead.LvName.ToString(),

                                                // Fld5 = LvOpenBal.LvCalendar == null ? "" : LvOpenBal.LvCalendar.FullDetails,
                                                // Fld14 = LvOpenBal.LvOpening.ToString(),

                                                Fld5 = LvOpenBal.LvOpening.ToString(),
                                                Fld6 = LvOpenBal.LvOpening.ToString(),
                                                Fld7 = LvOpenBal.LvClosing.ToString(),
                                                Fld9 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld10 = LvOpenBal.LvOccurances.ToString(),
                                                Fld11 = LvOpenBal.LvLapseBal.ToString(),

                                                Fld26 = LvOpenBal.LvCalendar == null ? "" : LvOpenBal.LvCalendar.FullDetails,
                                                Fld27 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld28 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld29 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,

                                                Fld13 = LvOpenBal.LvOpening.ToString(),
                                                Fld14 = LvOpenBal.LvOpening.ToString(),
                                                //  Fld15 = LvOpenBal.InputMethod == null ? "" : LvOpenBal.InputMethod.ToString(),
                                                Fld16 = LvOpenBal.LVCount == null ? "" : LvOpenBal.LVCount.ToString(),
                                                //  Fld50 = LvOpenBal.CreditDays != null ? LvOpenBal.CreditDays.ToString() : null,
                                                Fld17 = LvOpenBal.LvOccurances.ToString(),
                                                Fld18 = LvOpenBal.PrefixCount == null ? "" : LvOpenBal.PrefixCount.ToString(),
                                                Fld19 = LvOpenBal.SufixCount == null ? "" : LvOpenBal.SufixCount.ToString(),

                                                Fld39 = ca.Employee.EmpCode,
                                                Fld40 = ca.Employee.EmpName.FullNameFML,
                                                Fld41 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,
                                                Fld42 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,

                                                Fld43 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                Fld44 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,


                                                Fld30 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.Id.ToString(),
                                                //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                                Fld31 = ca.Employee.FuncStruct.JobPosition == null ? "" : ca.Employee.FuncStruct.JobPosition.JobPositionDesc,

                                                Fld32 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                                //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                                Fld33 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Name,

                                                Fld34 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Id.ToString(),
                                                //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                                Fld35 = ca.Employee.PayStruct.Level == null ? "" : ca.Employee.PayStruct.Level.Name,

                                                Fld36 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.Id.ToString(),
                                                Fld37 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                Fld38 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                            };
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;


                    //AJ
                    case "LVCLOSING":
                        var OLvclosingData = new List<EmployeeLeave>();
                        foreach (var item in EmpLeaveIdList)
                        {
                            var OLvclosingData_t = db.EmployeeLeave
                                //.Include(e => e.LvNewReq)
                                //.Include(e => e.LvNewReq.Select(r => r.ContactNo))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                //.Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.FuncStruct.Job)
                                //.Include(e => e.Employee.FuncStruct.JobPosition)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.PayStruct.Grade)
                                //.Include(e => e.Employee.PayStruct.Level)
                                //.Include(e => e.Employee.PayStruct.JobStatus)
                                //.Include(e => e.Employee.PayStruct.JobStatus.EmpStatus)
                                //.Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Division)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                //.Include(e => e.Employee.GeoStruct.Department)
                                //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                //.Include(e => e.Employee.GeoStruct.Group)
                                //.Include(e => e.Employee.GeoStruct.Unit)

                                //.Include(e => e.LvOpenBal)
                                //.Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                                //.Include(e => e.LvOpenBal.Select(r => r.LvHead))

                               .Where(e => e.Id == item).AsNoTracking()
                           .FirstOrDefault();


                            if (OLvclosingData_t != null)
                            {
                                OLvclosingData_t.Employee.EmpName = db.NameSingle.Find(OLvclosingData_t.Employee.EmpName_Id);
                                OLvclosingData_t.LvNewReq = db.EmployeeLeave.Where(e => e.Id == OLvclosingData_t.Id).Select(r => r.LvNewReq.Where(t => t.ReqDate != null && DbFunctions.TruncateTime(t.ReqDate.Value) <= ReqDate).OrderByDescending(t => t.Id).ToList()).FirstOrDefault();
                                OLvclosingData_t.LvOpenBal = db.EmployeeLeave.Where(e => e.Id == OLvclosingData_t.Id).Select(r => r.LvOpenBal.Where(t => t.DBTrack.CreatedOn != null && DbFunctions.TruncateTime(t.DBTrack.CreatedOn.Value) <= ReqDate).OrderByDescending(t => t.Id).ToList()).FirstOrDefault();
                                foreach (var d in OLvclosingData_t.LvNewReq)
                                {
                                    d.ContactNo = db.ContactNumbers.Find(d.ContactNo_Id);
                                    d.LeaveHead = db.LvHead.Find(d.LeaveHead_Id);
                                    d.LeaveCalendar = db.Calendar.Find(d.LeaveCalendar_Id);
                                }
                                foreach (var b in OLvclosingData_t.LvOpenBal)
                                {
                                    b.LvCalendar = db.Calendar.Find(b.LvCalendar_Id);
                                    b.LvHead = db.LvHead.Find(b.LvHead_Id);
                                }

                                OLvclosingData.Add(OLvclosingData_t);
                            }
                        }

                        if (OLvclosingData == null || OLvclosingData.Count() == 0)
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

                            foreach (var ca in OLvclosingData)
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



                                List<LvHead> LvHeadList1 = new List<Leave.LvHead>();
                                var lvnamehdr = salheadlist.ToList();
                                if (lvnamehdr.Count() != 0)
                                {
                                    foreach (var item in lvnamehdr)
                                    {
                                        var LvHeadList11q = db.LvHead.Where(q => q.LvName == item).FirstOrDefault();
                                        if (LvHeadList11q != null)
                                        {
                                            LvHeadList1.Add(LvHeadList11q);
                                        }
                                    }
                                }
                                else
                                {
                                    LvHeadList1 = db.LvHead.ToList();
                                }
                                foreach (var LvHd in LvHeadList1)
                                {
                                    if (ca.LvNewReq.Count() != 0 && ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id).Count() != 0)
                                    {
                                        // var OLvReq = ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id && e.ReqDate != null && e.ReqDate <= ReqDate).OrderByDescending(e => e.Id).FirstOrDefault();
                                        var OLvReq = ca.LvNewReq.Where(e => e.LeaveHead.Id == LvHd.Id && e.ReqDate != null).OrderByDescending(e => e.Id).FirstOrDefault();
                                        //foreach (var OLvReq in OLvReq1)
                                        //{
                                        if (OLvReq != null)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld2 = OLvReq.Id.ToString(),
                                                Fld3 = OLvReq.LeaveHead.LvCode,
                                                Fld4 = OLvReq.LeaveHead.LvName,
                                                // Fld5 = OLvReq.LeaveCalendar == null ? "" : OLvReq.LeaveCalendar.FullDetails,
                                                // Fld14 = OLvReq.CloseBal.ToString(),

                                                Fld5 = OLvReq.OpenBal.ToString(),
                                                Fld6 = OLvReq.OpenBal.ToString(),
                                                Fld7 = OLvReq.CloseBal.ToString(),
                                                Fld9 = OLvReq.CreditDays.ToString(),
                                                Fld10 = OLvReq.LvOccurances.ToString(),
                                                //Fld11 = OLvReq.ToString(),

                                                Fld26 = OLvReq.LeaveCalendar == null ? "" : OLvReq.LeaveCalendar.FullDetails,
                                                Fld27 = OLvReq.ReqDate != null ? OLvReq.ReqDate.Value.ToShortDateString() : null,
                                                Fld28 = OLvReq.FromDate != null ? OLvReq.FromDate.Value.ToShortDateString() : null,
                                                Fld29 = OLvReq.ToDate != null ? OLvReq.ToDate.Value.ToShortDateString() : null,
                                                Fld30 = OLvReq.FromStat != null ? OLvReq.FromStat.LookupVal.ToUpper() : null,
                                                Fld31 = OLvReq.ToStat != null ? OLvReq.ToStat.LookupVal.ToUpper() : null,
                                                Fld11 = OLvReq.DebitDays.ToString(),
                                                Fld12 = OLvReq.ResumeDate != null ? OLvReq.ResumeDate.Value.ToShortDateString() : null,
                                                Fld13 = OLvReq.OpenBal.ToString(),
                                                Fld14 = OLvReq.CloseBal.ToString(),
                                                Fld15 = OLvReq.InputMethod.ToString(),
                                                Fld16 = OLvReq.LVCount.ToString(),
                                                Fld50 = OLvReq.CreditDays.ToString(),
                                                Fld17 = OLvReq.LvOccurances.ToString(),
                                                Fld18 = OLvReq.PrefixCount.ToString(),
                                                Fld19 = OLvReq.SufixCount.ToString(),
                                                Fld20 = OLvReq.TrClosed.ToString(),
                                                Fld21 = OLvReq.TrReject.ToString(),
                                                Fld22 = OLvReq.WFStatus == null ? "" : OLvReq.WFStatus.LookupVal.ToUpper(),
                                                Fld23 = OLvReq.IsCancel.ToString(),
                                                Fld24 = OLvReq.ContactNo == null ? "" : OLvReq.ContactNo.FullContactNumbers,
                                                Fld25 = OLvReq.Reason != null ? OLvReq.Reason.ToString() : null,



                                                //Fld15 = ca.Id.ToString(),
                                                // Fld32 = ca.Employee.PayStruct.Grade == null ? "" : ca.Employee.PayStruct.Grade.Id.ToString(),
                                                Fld39 = ca.Employee.EmpCode,
                                                Fld40 = ca.Employee.EmpName.FullNameFML,
                                                Fld41 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                //Fld42 = GeoDataInd.GeoStruct_Location_Code == null ? "" : GeoDataInd.GeoStruct_Location_Code,
                                                //Fld43 = GeoDataInd.GeoStruct_Location_Code == null ? "" : GeoDataInd.GeoStruct_Location_Code,
                                                //Fld44 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                //Fld45 = ("name"),
                                                Fld42 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name
                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }


                                        //   }
                                    }

                                    else
                                    {

                                        //   var LvOpenBal = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && (e.LvCalendar.FromDate.Value >= mFromDate && e.LvCalendar.ToDate.Value <= mToDate)).SingleOrDefault();
                                        // var LvOpenBal = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && e.DBTrack.CreatedOn != null && e.DBTrack.CreatedOn <= ReqDate).OrderByDescending(e => e.Id).FirstOrDefault();
                                        var LvOpenBal = ca.LvOpenBal.Where(e => e.LvHead.Id == LvHd.Id && e.DBTrack.CreatedOn != null).OrderByDescending(e => e.Id).FirstOrDefault();
                                        if (LvOpenBal != null)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                //Fld1 = ca1.Id.ToString(),
                                                Fld2 = LvOpenBal.Id.ToString(),
                                                Fld3 = LvOpenBal.LvHead.LvCode,
                                                Fld4 = LvOpenBal.LvHead.LvName,
                                                // Fld5 = LvOpenBal.LvCalendar == null ? "" : LvOpenBal.LvCalendar.FullDetails,
                                                // Fld14 = LvOpenBal.LvOpening.ToString(),

                                                Fld5 = LvOpenBal.LvOpening.ToString(),
                                                Fld6 = LvOpenBal.LvOpening.ToString(),
                                                Fld7 = LvOpenBal.LvClosing.ToString(),
                                                Fld9 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld10 = LvOpenBal.LvOccurances.ToString(),
                                                Fld11 = LvOpenBal.LvLapseBal.ToString(),

                                                Fld26 = LvOpenBal.LvCalendar == null ? "" : LvOpenBal.LvCalendar.FullDetails,
                                                Fld27 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld28 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                Fld29 = LvOpenBal.LvCreditDate != null ? LvOpenBal.LvCreditDate.Value.ToShortDateString() : null,
                                                //Fld30 = LvOpenBal.FromStat.LookupVal.ToUpper(),
                                                //Fld31 = LvOpenBal.ToStat.LookupVal.ToUpper(),
                                                // Fld11 = LvOpenBal.DebitDays.ToString(),
                                                // Fld12 = LvOpenBal.ResumeDate != null ? LvOpenBal.ResumeDate.Value.ToShortDateString() : null,
                                                Fld13 = LvOpenBal.LvOpening.ToString(),
                                                Fld14 = LvOpenBal.LvClosing.ToString(),
                                                //  Fld15 = LvOpenBal.InputMethod == null ? "" : LvOpenBal.InputMethod.ToString(),
                                                Fld16 = LvOpenBal.LVCount == null ? "" : LvOpenBal.LVCount.ToString(),
                                                //  Fld50 = LvOpenBal.CreditDays != null ? LvOpenBal.CreditDays.ToString() : null,
                                                Fld17 = LvOpenBal.LvOccurances.ToString(),
                                                Fld18 = LvOpenBal.PrefixCount == null ? "" : LvOpenBal.PrefixCount.ToString(),
                                                Fld19 = LvOpenBal.SufixCount == null ? "" : LvOpenBal.SufixCount.ToString(),
                                                //Fld20 = LvOpenBal.TrClosed == null ? "" : LvOpenBal.TrClosed.ToString(),
                                                //Fld21 = LvOpenBal.TrReject == null ? "" : LvOpenBal.TrReject.ToString(),
                                                //Fld22 = LvOpenBal.WFStatus == null ? "" : LvOpenBal.WFStatus.LookupVal.ToUpper(),
                                                //Fld23 = LvOpenBal.IsCancel == null ? "" : LvOpenBal.IsCancel.ToString(),
                                                //Fld24 = LvOpenBal.ContactNo == null ? "" : LvOpenBal.ContactNo.FullContactNumbers,
                                                //Fld25 = LvOpenBal.Reason != null ? LvOpenBal.Reason.ToString() : null,

                                                Fld39 = ca.Employee.EmpCode,
                                                Fld40 = ca.Employee.EmpName.FullNameFML,
                                                Fld41 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                Fld42 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                //Fld42 = GeoDataInd.GeoStruct_Location_Code == null ? "" : GeoDataInd.GeoStruct_Location_Code,
                                                //Fld43 = GeoDataInd.GeoStruct_Location_Code == null ? "" : GeoDataInd.GeoStruct_Location_Code,
                                                //Fld44 = GeoDataInd.GeoStruct_Location_Name == null ? "" : GeoDataInd.GeoStruct_Location_Name,
                                                Fld30 = GeoDataInd.FuncStruct_JobPosition_Id == null ? "" : GeoDataInd.FuncStruct_JobPosition_Id.ToString(),
                                                //Fld37=ca.Employee.FuncStruct.JobPosition==null?"":ca.Employee.FuncStruct.JobPosition.JobPositionCode,
                                                Fld31 = GeoDataInd.FuncStruct_JobPosition_Name == null ? "" : GeoDataInd.FuncStruct_JobPosition_Name,
                                                Fld32 = GeoDataInd.PayStruct_Grade_Id == null ? "" : GeoDataInd.PayStruct_Grade_Id.ToString(),
                                                //Fld40=ca.Employee.PayStruct.Grade==null?"":ca.Employee.PayStruct.Grade.Code,
                                                Fld33 = GeoDataInd.PayStruct_Grade_Name == null ? "" : GeoDataInd.PayStruct_Grade_Name,
                                                Fld34 = GeoDataInd.PayStruct_Level_Id == null ? "" : GeoDataInd.PayStruct_Level_Id.ToString(),
                                                //Fld43=ca.Employee.PayStruct.Level==null?"":ca.Employee.PayStruct.Level.Code,
                                                Fld35 = GeoDataInd.PayStruct_Level_Name == null ? "" : GeoDataInd.PayStruct_Level_Name,
                                                Fld36 = GeoDataInd.FuncStruct_JobPosition_Id == null ? "" : GeoDataInd.FuncStruct_JobPosition_Id.ToString(),
                                                Fld37 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                Fld38 = ca.Employee.PayStruct == null ? "" : ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    case "LEAVEENCASHREQ":
                        var Emp_y1q = db.EmployeeLeave.Include(a => a.Employee).AsNoTracking().Where(a => EmpLeaveIdList.Contains(a.Id)).Select(a => a.Employee.Id).AsParallel().ToList();
                        //   var EmpPay_y1q = db.EmployeePayroll.Include(a => a.Employee).AsNoTracking().Where(a => Emp_y1q.Contains(a.Employee.Id)).Select(a => a.Id).AsParallel().ToList();

                        var OyearlypaymentData1q = new List<EmployeeLeave>();
                        foreach (var item in Emp_y1q)
                        {
                            var OyearlypaymentData_t = db.EmployeeLeave
                                // .Include(a => a.LeaveEncashReq)
                                //.Include(a => a.LeaveEncashReq.Select(b => b.LvNewReq))
                                //.Include(a => a.LeaveEncashReq.Select(b => b.LvNewReq.ContactNo))
                                //.Include(a => a.LeaveEncashReq.Select(b => b.LvNewReq.LeaveHead))
                                //.Include(a => a.LeaveEncashReq.Select(b => b.WFStatus))
                                //.Include(a => a.LeaveEncashReq.Select(b => b.LvHead))
                                //.Include(a => a.LeaveEncashReq.Select(b => b.LeaveCalendar))
                                .Include(e => e.Employee)
                                //.Include(e => e.Employee.EmpName)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.GeoStruct.Location)
                                //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                .Where(e => e.Id == item).AsNoTracking().FirstOrDefault();

                            if (OyearlypaymentData_t != null)
                            {
                                OyearlypaymentData_t.Employee.EmpName = db.NameSingle.Find(OyearlypaymentData_t.Employee.EmpName_Id);
                                OyearlypaymentData_t.LeaveEncashReq = db.EmployeeLeave.Where(e => e.Id == OyearlypaymentData_t.Id).Select(r => r.LeaveEncashReq.Where(t => t.LeaveCalendar.FromDate >= mFromDate && t.LeaveCalendar.ToDate <= mToDate).ToList()).FirstOrDefault();
                                foreach (var b in OyearlypaymentData_t.LeaveEncashReq)
                                {
                                    b.LvNewReq = db.LvNewReq.Find(b.LvNewReq_Id);
                                    if (b.LvNewReq != null)
                                    {
                                        b.LvNewReq.ContactNo = db.ContactNumbers.Find(b.LvNewReq.ContactNo_Id);
                                        b.LvNewReq.LeaveHead = db.LvHead.Find(b.LvNewReq.LeaveHead_Id);
                                    }

                                    b.WFStatus = db.LookupValue.Find(b.WFStatus_Id);
                                    b.LvHead = db.LvHead.Find(b.LvHead_Id);
                                    b.LeaveCalendar = db.Calendar.Find(b.LeaveCalendar_Id);
                                }
                                OyearlypaymentData1q.Add(OyearlypaymentData_t);
                            }
                        }
                        // var LvHeadList1 = db.LvHead.ToList();
                        if (OyearlypaymentData1q == null || OyearlypaymentData1q.Count() == 0)
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


                            foreach (var ca in OyearlypaymentData1q)
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


                                string LocCode = "";
                                string LocDesc = "";
                                //this checking for location not written in one line using && for optimization
                                //if (ca.Employee.GeoStruct != null)
                                //{
                                //    if (ca.Employee.GeoStruct.Location != null)
                                //    {
                                //        if (ca.Employee.GeoStruct.Location.LocationObj != null)
                                //        {
                                //            LocCode = ca.Employee.GeoStruct.Location.LocationObj.LocCode;
                                //            LocDesc = ca.Employee.GeoStruct.Location.LocationObj.LocDesc;
                                //        }
                                //    }
                                //}
                                var lvname1 = salheadlist.ToList();
                                if (lvname1.Count() != 0)
                                {
                                    foreach (var lvsingle in salheadlist)
                                    {
                                        var lvencashreq = ca.LeaveEncashReq.Where(e => e.LvHead.LvName == lvsingle && e.LeaveCalendar.FromDate >= mFromDate && e.LeaveCalendar.ToDate <= mToDate).ToList();

                                        if (lvencashreq.Count() != 0)
                                        {
                                            foreach (var er in lvencashreq)
                                            {
                                                if (er.LvNewReq != null && ca.Employee != null)
                                                {
                                                    LvNewReq nr = er.LvNewReq;
                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML,
                                                        //Fld3 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                        //Fld4 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                        Fld3 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                        Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                        Fld5 = nr.ReqDate.Value.ToShortDateString(),
                                                        Fld6 = er.FromPeriod.Value.ToShortDateString(),
                                                        Fld7 = er.ToPeriod.Value.ToShortDateString(),
                                                        Fld8 = er.EncashDays.ToString(),
                                                        Fld9 = nr.LeaveHead.LvCode + ", " + nr.FromDate.Value.ToShortDateString() + ", " + nr.FromDate.Value.ToShortDateString(),
                                                        Fld10 = nr.InputMethod.ToString(),
                                                        Fld11 = nr.TrClosed.ToString(),
                                                        Fld12 = nr.TrReject.ToString(),
                                                        Fld13 = er.WFStatus == null ? "" : er.WFStatus.LookupVal.ToString(),
                                                        Fld14 = nr.IsCancel.ToString(),
                                                        Fld15 = nr.ContactNo != null ? nr.ContactNo.MobileNo : "",
                                                        Fld16 = er.Narration,
                                                        Fld17 = er.LvHead.LvName.ToString()


                                                    };
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
                                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                }
                                                else if (ca.Employee != null)
                                                {
                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML,
                                                        //Fld3 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                        //Fld4 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                        Fld3 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                        Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                        Fld5 = er.DBTrack.CreatedOn.Value.ToShortDateString(),
                                                        Fld6 = er.FromPeriod.Value.ToShortDateString(),
                                                        Fld7 = er.ToPeriod.Value.ToShortDateString(),
                                                        Fld8 = er.EncashDays.ToString(),
                                                        Fld9 = "",
                                                        Fld10 = "",
                                                        Fld11 = "",
                                                        Fld12 = "",
                                                        Fld13 = er.WFStatus == null ? "" : er.WFStatus.LookupVal.ToString(),
                                                        Fld14 = "",
                                                        Fld15 = "",
                                                        Fld16 = er.Narration,
                                                        Fld17 = er.LvHead.LvName.ToString()

                                                    };
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
                                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                }

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var lvencashreq1 = ca.LeaveEncashReq.Where(e => e.LeaveCalendar.FromDate >= mFromDate && e.LeaveCalendar.ToDate <= mToDate).ToList();

                                    foreach (var er in lvencashreq1)
                                    {
                                        if (er.LvNewReq != null && ca.Employee != null)
                                        {
                                            LvNewReq nr = er.LvNewReq;
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML,
                                                //Fld3 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                //Fld4 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                Fld3 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld5 = nr.ReqDate.Value.ToShortDateString(),
                                                Fld6 = er.FromPeriod.Value.ToShortDateString(),
                                                Fld7 = er.ToPeriod.Value.ToShortDateString(),
                                                Fld8 = er.EncashDays.ToString(),
                                                Fld9 = nr.LeaveHead.LvCode + ", " + nr.FromDate.Value.ToShortDateString() + ", " + nr.FromDate.Value.ToShortDateString(),
                                                Fld10 = nr.InputMethod.ToString(),
                                                Fld11 = nr.TrClosed.ToString(),
                                                Fld12 = nr.TrReject.ToString(),
                                                Fld13 = er.WFStatus == null ? "" : er.WFStatus.LookupVal.ToString(),
                                                Fld14 = nr.IsCancel.ToString(),
                                                Fld15 = nr.ContactNo != null ? nr.ContactNo.MobileNo : "",
                                                Fld16 = er.Narration,
                                                Fld17 = er.LvHead.LvName.ToString()

                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                        else if (ca.Employee != null)
                                        {
                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML,
                                                //Fld3 = GeoDataInd.LocCode == null ? "" : GeoDataInd.LocCode,
                                                //Fld4 = GeoDataInd.LocDesc == null ? "" : GeoDataInd.LocDesc,
                                                Fld3 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                                Fld4 = GeoDataInd.FuncStruct_Job_Name,
                                                Fld5 = er.DBTrack.CreatedOn.Value.ToShortDateString(),
                                                Fld6 = er.FromPeriod.Value.ToShortDateString(),
                                                Fld7 = er.ToPeriod.Value.ToShortDateString(),
                                                Fld8 = er.EncashDays.ToString(),
                                                Fld9 = "",
                                                Fld10 = "",
                                                Fld11 = "",
                                                Fld12 = "",
                                                Fld13 = er.WFStatus == null ? "" : er.WFStatus.LookupVal.ToString(),
                                                Fld14 = "",
                                                Fld15 = "",
                                                Fld16 = er.Narration,
                                                Fld17 = er.LvHead.LvName.ToString()

                                            };
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
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }

                                    }

                                }

                            }
                            return OGenericLvTransStatement;
                        }


                        break;

                    case "LEAVEENCASHPROJECTION":
                        DateTime ReqDatecur = DateTime.Now.Date;
                        var PREmp = db.EmployeeLeave.Include(a => a.Employee).Where(a => EmpLeaveIdList.Contains(a.Id)).Select(a => a.Employee.Id).ToList();
                        var PREncashData = new List<EmployeeLeave>();
                        foreach (var item in PREmp)
                        {
                            var PREncashData_t = db.EmployeeLeave

                                .Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                                .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                                .Include(e => e.Employee)
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.GeoStruct.Location)
                                .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                .Include(e => e.Employee.GeoStruct.Department)
                                .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                .Include(e => e.Employee.FuncStruct)
                                .Include(e => e.Employee.FuncStruct.Job)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.Employee.Gender)
                                .Include(e => e.Employee.ServiceBookDates)
                                .Where(e => e.Id == item).FirstOrDefault();

                            if (PREncashData_t != null)
                            {
                                PREncashData.Add(PREncashData_t);
                            }
                        }
                        if (PREncashData == null || PREncashData.Count() == 0)
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

                            foreach (var ca in PREncashData)
                            {
                                if (ca.LvNewReq != null && ca.LvNewReq.Count() != 0)
                                {
                                    if (salheadlist != null && salheadlist.Count() != 0)
                                    {
                                        foreach (var Lvname in salheadlist)
                                        {
                                            var OLvEncash = ca.LvNewReq.Where(a => a.LeaveHead.LvName == Lvname).OrderByDescending(r => r.Id).FirstOrDefault();

                                            if (OLvEncash != null)
                                            {
                                                var slclosebal = "";
                                                var lablelvcode = "";
                                                foreach (var LvName in salheadlist)
                                                {
                                                    var SLbal = ca.LvNewReq.Where(a => SpecialGroupslist.Contains(a.LeaveHead.LvCode)).OrderByDescending(r => r.Id).FirstOrDefault();
                                                    if (SLbal != null)
                                                    {
                                                        slclosebal = SLbal.CloseBal.ToString();
                                                        lablelvcode = SLbal.LeaveHead.LvCode + " " + "Balance";
                                                    }
                                                    var employeepayroll = db.EmployeePayroll.Where(e => e.Employee.Id == ca.Employee.Id).Select(e => e.Id).FirstOrDefault();
                                                    List<double> ena = LeaveEncashCalcProjection(employeepayroll, PayMonth, OLvEncash.CloseBal);
                                                    if (ena.Count() > 0)
                                                    {
                                                        GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                        {
                                                            Fld1 = ca.Employee.EmpCode,
                                                            Fld2 = ca.Employee.EmpName.FullNameFML,
                                                            Fld3 = ca.Employee.Gender.LookupVal.ToUpper().ToString(),
                                                            Fld4 = ca.Employee.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                                                            Fld5 = ca.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString(),
                                                            Fld6 = ca.Employee.ServiceBookDates.RetirementDate.Value.ToShortDateString(),
                                                            Fld7 = ena[0].ToString(),
                                                            Fld8 = OLvEncash.CloseBal.ToString(),
                                                            Fld9 = OLvEncash.LeaveHead == null ? "" : OLvEncash.LeaveHead.LvName.ToString(),
                                                            Fld10 = ena[1].ToString(),
                                                            Fld11 = slclosebal,
                                                            Fld16 = lablelvcode,
                                                            Fld21 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null) ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                            Fld22 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null) ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : "",
                                                            Fld23 = (ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null) ? ca.Employee.FuncStruct.Job.Name : ""
                                                        };
                                                        OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                    }
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    case "LEAVEENCASHPAYMENT":
                        var Emp_Lv = db.EmployeeLeave.Include(a => a.Employee).AsNoTracking().Where(a => EmpLeaveIdList.Contains(a.Id)).Select(a => a.Employee.Id).AsParallel().ToList();
                        var EmpPay_y1q = db.EmployeePayroll.Include(a => a.Employee).AsNoTracking().Where(a => Emp_Lv.Contains(a.Employee.Id)).Select(a => a.Id).AsParallel().ToList();

                        List<int> LvHeadListen = new List<int>();
                        var lvnamehdren = salheadlist.ToList();
                        if (lvnamehdren.Count() != 0)
                        {
                            foreach (var item in lvnamehdren)
                            {
                                var LvHeadList11q = db.LvHead.Where(q => q.LvName == item).Select(e => e.Id).FirstOrDefault();
                                if (LvHeadList11q != null)
                                {
                                    LvHeadListen.Add(LvHeadList11q);
                                }
                            }
                        }
                        else
                        {
                            LvHeadListen = db.LvHead.Select(e => e.Id).ToList();
                        }

                        var OyearlypaymentData1 = new List<EmployeePayroll>();
                        foreach (var item in EmpPay_y1q)
                        {
                            var OyearlypaymentData_temp = db.EmployeePayroll
                                //.Include(a => a.LvEncashPayment)
                                //.Include(a => a.LvEncashPayment.Select(b => b.LvEncashReq))
                                //.Include(a => a.LvEncashPayment.Select(b => b.LvEncashReq.LvHead))
                                .Include(e => e.Employee)
                                .Include(e => e.Employee.EmpOffInfo)
                                .Include(e => e.Employee.EmpOffInfo.AccountType)
                                //.Include(e => e.Employee.GeoStruct)
                                //.Include(e => e.Employee.PayStruct)
                                //.Include(e => e.Employee.FuncStruct)
                                //.Include(e => e.Employee.EmpName)
                                .AsNoTracking()
                                .Where(e => e.Id == item).AsParallel().FirstOrDefault();


                            if (OyearlypaymentData_temp != null)
                            {
                                OyearlypaymentData_temp.Employee.EmpName = db.NameSingle.Find(OyearlypaymentData_temp.Employee.EmpName_Id);
                                OyearlypaymentData_temp.LvEncashPayment = db.EmployeePayroll.Where(e => e.Id == OyearlypaymentData_temp.Id).Select(r => r.LvEncashPayment.Where(t => t.PaymentDate >= pfromdate.Date && t.PaymentDate <= ptodate.Date).ToList()).FirstOrDefault();
                                foreach (var b in OyearlypaymentData_temp.LvEncashPayment)
                                {
                                    b.LvEncashReq = db.LvEncashReq.Find(b.LvEncashReq_Id);
                                    b.LvEncashReq.LvHead = db.LvHead.Find(b.LvEncashReq.LvHead_Id);
                                }
                                OyearlypaymentData1.Add(OyearlypaymentData_temp);
                            }
                        }
                        if (OyearlypaymentData1 == null || OyearlypaymentData1.Count() == 0)
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

                            foreach (var ca in OyearlypaymentData1)
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

                                var lvencpay = ca.LvEncashPayment.Where(e => e.PaymentDate >= pfromdate.Date && e.PaymentDate <= ptodate.Date && LvHeadListen.Contains(e.LvEncashReq.LvHead.Id)).ToList();

                                foreach (var ca1 in lvencpay)
                                {
                                    EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                                                   .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                                                   .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                                                                   .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                                                                   .Where(e => e.Id == ca.Employee.Id).OrderBy(e => e.Id).SingleOrDefault();

                                    var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                                    var leavebal = LvCalendarFilter.Where(e => e.WFStatus.LookupVal == "2" && e.LeaveHead.Id == ca1.LvEncashReq.LvHead.Id && e.FromDate == ca1.LvEncashReq.FromPeriod && e.ToDate == ca1.LvEncashReq.ToPeriod).FirstOrDefault();

                                    // var lvencreq = ca.LvEncashPayment.Select(e => e.LvEncashReq).ToList();

                                    //foreach (var ca2 in lvencreq)
                                    //{

                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                    //write data to generic class
                                    {
                                        Fld1 = ca.Employee.EmpCode,
                                        Fld2 = ca.Employee.EmpName.FullNameFML,
                                        Fld3 = ca1.LvEncashReq == null ? "" : ca1.LvEncashReq.FromPeriod.Value.ToShortDateString(),
                                        Fld4 = ca1.LvEncashReq == null ? "" : ca1.LvEncashReq.ToPeriod.Value.ToShortDateString(),
                                        Fld5 = ca1.LvEncashReq.EncashDays.ToString(),
                                        Fld6 = ca1.AmountPaid.ToString(),
                                        Fld7 = ca1.TDSAmount.ToString(),
                                        Fld8 = ca1.OtherDeduction.ToString(),
                                        Fld9 = ca1.LvEncashReq == null ? "" : ca1.LvEncashReq.LvHead == null ? "" : ca1.LvEncashReq.LvHead.LvName,
                                        Fld10 = ca1.PaymentMonth != null ? ca1.PaymentMonth.ToString() : "",
                                        Fld11 = ca1.LvEncashReq == null ? "" : ca1.LvEncashReq.DBTrack == null ? "" : ca1.LvEncashReq.DBTrack.CreatedOn.Value.ToShortDateString(),
                                        Fld12 = leavebal.OpenBal.ToString(),
                                        Fld13 = leavebal.CloseBal.ToString(),
                                        Fld19 = leavebal.DebitDays.ToString(),
                                        Fld14 = GeoDataInd.PayStruct_Grade_Name,
                                        Fld15 = GeoDataInd.GeoStruct_Location_Name + " " + (GeoDataInd.GeoStruct_Department_Name != null ? "/" : " ") + GeoDataInd.GeoStruct_Department_Name,
                                        Fld16 = ca1.ProcessMonth != null ? ca1.ProcessMonth.ToString() : "",
                                        Fld17 = ca.Employee.EmpOffInfo.AccountNo == null ? "" : ca.Employee.EmpOffInfo.AccountNo,
                                        Fld18 = ca.Employee.EmpOffInfo.AccountType == null ? "" : ca.Employee.EmpOffInfo.AccountType.LookupVal.ToString(),

                                    };
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
                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                    //}
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;




                    case "LVENCASHMENT":

                        var Emp = db.EmployeeLeave.Include(a => a.Employee).Where(a => EmpLeaveIdList.Contains(a.Id)).Select(a => a.Employee.Id).ToList();
                        var EmpPay = db.EmployeePayroll.Include(a => a.Employee).Where(a => Emp.Contains(a.Employee.Id)).Select(a => a.Id).ToList();

                        var OLvNewEncashData = new List<EmployeePayroll>();
                        foreach (var item in EmpPay)
                        {
                            var OLvNewEncashData_t = db.EmployeePayroll.Include(a => a.LvEncashPayment)
                               .Include(a => a.LvEncashPayment.Select(b => b.LvEncashReq))
                                .Include(e => e.LvEncashPayment.Select(r => r.LvEncashReq).Select(a => a.LvNewReq).Select(c => c.LeaveHead))
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


                                //.Where(e => EmpLeaveIdList.Contains(e.Employee.Id))

                                  .Where(e => e.Id == item)
                                                   .FirstOrDefault();


                            if (OLvNewEncashData_t != null)
                            {
                                OLvNewEncashData.Add(OLvNewEncashData_t);
                            }
                        }
                        if (OLvNewEncashData == null || OLvNewEncashData.Count() == 0)
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
                            foreach (var ca11 in OLvNewEncashData)
                            {

                                int geoid = ca11.Employee.GeoStruct.Id;

                                int payid = ca11.Employee.PayStruct.Id;

                                int funid = ca11.Employee.FuncStruct.Id;

                                GeoDataInd = GetViewDataIndiv(geoid, payid, funid, GeoData, 0);
                                PaydataInd = GetViewDataIndiv(geoid, payid, funid, Paydata, 1);
                                FuncdataInd = GetViewDataIndiv(geoid, payid, funid, Funcdata, 2);
                                foreach (var ca in OLvNewEncashData)
                                {
                                    if (ca.LvEncashPayment != null && ca.LvEncashPayment.Count() != 0)
                                    {
                                        //   var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                        var OLvEncash = ca.LvEncashPayment.Where(a => a.Id != null).ToList();
                                        if (OLvEncash != null && OLvEncash.Count() != 0)
                                        {
                                            foreach (var ca1 in OLvEncash)
                                            {
                                                var EmpPayroll_Id = db.EmployeePayroll.Where(e => e.Employee.Id == ca.Employee.Id).SingleOrDefault().Id;
                                                IList<EncashHeadData> a = LeaveEncashCalc(EmpPayroll_Id, ca1);
                                                foreach (var item in a)
                                                {
                                                    GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                                    //write data to generic class
                                                    {
                                                        Fld1 = ca.Employee.EmpCode,
                                                        Fld2 = ca.Employee.EmpName.FullNameFML,
                                                        Fld3 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                        Fld4 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                        //  Fld5 = ca1.LvEncashReq.LvNewReq.LeaveHead.LvName,
                                                        Fld5 = ca1.LvEncashReq.LvNewReq == null ? "" : ca1.LvEncashReq.LvNewReq.LeaveHead == null ? "" : ca1.LvEncashReq.LvNewReq.LeaveHead.LvName,
                                                        Fld6 = ca1.ProcessMonth,
                                                        Fld7 = ca1.LvEncashReq.EncashDays.ToString(),
                                                        Fld8 = ca1.AmountPaid.ToString(),
                                                        Fld9 = ca1.LvEncashReq.FromPeriod.Value.ToShortDateString(),
                                                        Fld10 = ca1.LvEncashReq.ToPeriod.Value.ToShortDateString(),
                                                        Fld11 = item.SalHead.Name.ToString(),
                                                        Fld12 = item.Amount.ToString(),

                                                        Fld13 = ca1.LvEncashReq.LvNewReq == null ? "" : ca1.LvEncashReq.LvNewReq.OpenBal == null ? "" :

                                                        ca1.LvEncashReq.LvNewReq.OpenBal.ToString(),
                                                        Fld14 = ca1.LvEncashReq.LvNewReq == null ? "" : ca1.LvEncashReq.LvNewReq.CloseBal == null ? "" :
                                                        ca1.LvEncashReq.LvNewReq.CloseBal.ToString(),

                                                        Fld26 = ca.Id.ToString(),
                                                        Fld27 = ca.Employee.EmpCode,
                                                        Fld28 = ca.Employee.EmpName.FullNameFML,

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
                                                        Fld48 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpStatus == null ? "" :
                                                        ca.Employee.PayStruct.JobStatus.EmpStatus.LookupVal.ToUpper(),
                                                        Fld49 = ca.Employee.PayStruct.JobStatus == null ? "" : ca.Employee.PayStruct.JobStatus.EmpActingStatus == null ? "" :
                                                        ca.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper(),


                                                    };
                                                    if (comp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld99 = GeoDataInd.Company_Name;
                                                    }
                                                    if (div)
                                                    {
                                                        OGenericLvTransObjStatement.Fld98 = GeoDataInd.Division_Name;
                                                    }
                                                    if (loca)
                                                    {
                                                        OGenericLvTransObjStatement.Fld97 = GeoDataInd.LocDesc;
                                                    }
                                                    if (dept)
                                                    {
                                                        OGenericLvTransObjStatement.Fld96 = GeoDataInd.DeptDesc;
                                                    }
                                                    if (grp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld95 = GeoDataInd.Group_Name;
                                                    }
                                                    if (unit)
                                                    {
                                                        OGenericLvTransObjStatement.Fld94 = GeoDataInd.Unit_Name;
                                                    }
                                                    if (grade)
                                                    {
                                                        OGenericLvTransObjStatement.Fld93 = PaydataInd.Grade_Name;
                                                    }
                                                    if (lvl)
                                                    {
                                                        OGenericLvTransObjStatement.Fld92 = PaydataInd.Level_Name;
                                                    }
                                                    if (jobstat)
                                                    {
                                                        OGenericLvTransObjStatement.Fld91 = GeoDataInd.JobStatus_Id.ToString();
                                                    }
                                                    if (job)
                                                    {
                                                        OGenericLvTransObjStatement.Fld90 = FuncdataInd.Job_Name;
                                                    }
                                                    if (jobpos)
                                                    {
                                                        OGenericLvTransObjStatement.Fld89 = FuncdataInd.JobPositionDesc;
                                                    }
                                                    if (emp)
                                                    {
                                                        OGenericLvTransObjStatement.Fld88 = ca.Employee.EmpName.FullNameFML;
                                                    }
                                                    //add spl filter
                                                    OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                                }
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    case "LEAVECREDITTRAILBALANCE":

                        var Emp_lvcr = db.EmployeeLeave.Include(a => a.Employee).Include(a => a.Employee.EmpName).Where(a => EmpLeaveIdList.Contains(a.Employee.Id)).ToList();
                        // var EmpPay_lvcr = db.EmployeePayroll.Include(a => a.Employee).Include(a => a.Employee.EmpName).Where(a => Emp_lvcr.Contains(a.Employee.Id)).ToList();

                        var CompLvId = Convert.ToInt32(SessionManager.CompLvId);
                        int CompId = 0;


                        CompId = Convert.ToInt32(SessionManager.CompanyId);
                        DateTime? FromDate = DateTime.Now;
                        DateTime? ToDate = DateTime.Now;

                        CompanyPayroll OCompanyPayroll = null;
                        OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Company.Id == CompId).SingleOrDefault();

                        var CompCreditPolicyList = db.CompanyLeave
                             .Include(e => e.LvCreditPolicy)
                             .Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHead))
                             .Include(e => e.LvCreditPolicy.Select(a => a.LvHead))
                             .Include(e => e.LvCreditPolicy.Select(a => a.CreditDate))
                             .Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHeadBal))
                             .Include(e => e.LvCreditPolicy.Select(a => a.ExcludeLeaveHeads))
                             .AsNoTracking()
                             .Where(e => e.Id == CompLvId).SingleOrDefault();

                        var lvcalendar = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                        List<LvHead> leaveheaddata = db.LvHead.ToList();
                        List<string> creditdatelist = SpecialGroupslist;
                        List<Int32> salheadlists = salheadlist.ConvertAll(int.Parse); ;
                        foreach (var i in Emp_lvcr)
                        {
                            //foreach (string LvHead_ in salheadlist)
                            //{
                            int emp = Convert.ToInt32(i.Employee.Id);

                            List<P2BUltimate.Controllers.Leave.MainController.LvCreditProcessController.LvNewReqForReport> ErrNo =
                                LvCreditProcessController.LvCreditProceessForReport(emp, CompLvId, CompCreditPolicyList, salheadlists, lvcalendar.Id, OCompanyPayroll.Id, LvFromdate, LvTodate, creditdatelist, settlementemp);
                            if (ErrNo != null)
                            {
                                foreach (var item in ErrNo)
                                {
                                    GenericField100 Lvtrailbal = new GenericField100()
                                    {
                                        Fld1 = i.Employee.EmpCode,
                                        Fld2 = i.Employee.EmpName.FullNameFML,
                                        Fld3 = item.LeaveHead,
                                        Fld4 = item.OpenBal,
                                        Fld5 = item.CloseBal,
                                        Fld6 = item.CreditDays,
                                        Fld7 = item.LvLapsed,
                                        Fld8 = item.LvCount,
                                        Fld9 = item.DebitDays == null || item.DebitDays == "" ? "0" : item.DebitDays,
                                    };
                                    OGenericLvTransStatement.Add(Lvtrailbal);
                                }
                            }


                            //}
                        }
                        string lvbalancecreditprocesreportsmsg = "";
                        dynamic leavebalancecreditprocessreportmesg = System.Web.HttpContext.Current.Session["LeaveCreditProcessReportMsg"];
                        if (leavebalancecreditprocessreportmesg != null)
                        {
                            lvbalancecreditprocesreportsmsg = string.Join(",", leavebalancecreditprocessreportmesg);
                            System.Web.HttpContext.Current.Response.Write("<script type='text/javascript'>alert('" + lvbalancecreditprocesreportsmsg + "')</script>");
                        }
                        return OGenericLvTransStatement;
                        break;

                    case "YEARLYPAYMENT":

                        var Emp_y = db.EmployeeLeave.Include(a => a.Employee).Where(a => EmpLeaveIdList.Contains(a.Id)).Select(a => a.Employee.Id).ToList();
                        var EmpPay_y = db.EmployeePayroll.Include(a => a.Employee).Where(a => Emp_y.Contains(a.Employee.Id)).Select(a => a.Id).ToList();



                        var OyearlypaymentData = new List<EmployeePayroll>();
                        foreach (var item in EmpPay_y)
                        {
                            var OyearlypaymentData_t = db.EmployeePayroll.Include(a => a.YearlyPaymentT)
                               .Include(a => a.YearlyPaymentT.Select(b => b.LvEncashReq))
                                  .Include(a => a.YearlyPaymentT.Select(b => b.SalaryHead))
                                .Include(e => e.YearlyPaymentT.Select(r => r.LvEncashReq).Select(a => a.LvNewReq).Select(c => c.LeaveHead))
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


                                .Where(e => e.Id == item)
                                                   .FirstOrDefault();


                            if (OyearlypaymentData_t != null)
                            {
                                OyearlypaymentData.Add(OyearlypaymentData_t);
                            }
                        }
                        if (OyearlypaymentData == null || OyearlypaymentData.Count() == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OyearlypaymentData)
                            {
                                if (ca.YearlyPaymentT != null && ca.YearlyPaymentT.Count() != 0)
                                {
                                    //   var OLvReq = ca.LvNewReq.Where(e => (e.FromDate.Value >= mFromDate && e.FromDate.Value <= mToDate) || (e.ToDate.Value >= mFromDate && e.ToDate.Value >= mToDate)).ToList();
                                    var OLvEncash = ca.YearlyPaymentT.Where(a => a.Id != null).ToList();
                                    if (OLvEncash != null && OLvEncash.Count() != 0)
                                    {
                                        foreach (var ca1 in OLvEncash)
                                        {


                                            GenericField100 OGenericLvTransObjStatement = new GenericField100()
                                            //write data to generic class
                                            {
                                                Fld1 = ca.Employee.EmpCode,
                                                Fld2 = ca.Employee.EmpName.FullNameFML,
                                                Fld3 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocCode,
                                                Fld4 = ca.Employee.GeoStruct.Location == null ? "" : ca.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                                //Fld5 = ca1.LvEncashReq.LvNewReq.LeaveHead.LvName,
                                                Fld5 = ca1.SalaryHead.Name == null ? "" : ca1.SalaryHead.Name,
                                                Fld6 = ca1.ProcessMonth == null ? "" : ca1.ProcessMonth,
                                                // Fld7 = ca1.LvEncashReq.EncashDays.ToString(),
                                                Fld8 = ca1.AmountPaid.ToString(),
                                                Fld9 = ca1.FromPeriod == null ? "" : ca1.FromPeriod.ToString(),
                                                Fld10 = ca1.ToPeriod == null ? "" : ca1.ToPeriod.ToString(),
                                                //Fld10 = ca1.ToPeriod.Value == null ? "" : ca1.ToPeriod.Value.ToShortDateString(),
                                                Fld11 = ca1.TDSAmount.ToString(),
                                                Fld12 = ca1.OtherDeduction.ToString(),


                                            };
                                            OGenericLvTransStatement.Add(OGenericLvTransObjStatement);
                                        }
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    case "PENDINGLEAVECREDIT":

                        List<Int32> lvheadlists = salheadlist.ConvertAll(int.Parse);

                        var OPENDINGLEAVECREDIT = db.EmployeeLeave
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Location)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.Employee.GeoStruct.Department)
                        .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(q => q.LeaveHead))
                        .Include(e => e.LvNewReq.Select(q => q.WFStatus))
                        .Include(e => e.LvNewReq.Select(q => q.LvWFDetails))
                        .Where(a => EmpLeaveIdList.Contains(a.Employee.Id)).AsNoTracking().ToList();

                        if (OPENDINGLEAVECREDIT == null)
                        {
                            return null;
                        }
                        else
                        {
                            foreach (var ca in OPENDINGLEAVECREDIT)
                            {
                                //       EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                //.Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                // .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                                // .Include(e => e.LvNewReq.Select(q => q.LvWFDetails))
                                //  .Include(e => e.LvNewReq.Select(q => q.LeaveHead))
                                //           // .Where(e => e.Id == ca.Id).OrderBy(e => e.Id)
                                //.Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.EmployeeLeave_Id == ca.Id && a.TrClosed == false && a.WFStatus.LookupVal != "0" && a.WFStatus.LookupVal != "2" && a.WFStatus.LookupVal != "3" && lvheadlists.Contains(a.LeaveHead.Id)))
                                // .SingleOrDefault();
                                var LvOrignal_id = db.LvNewReq.Where(e => e.LvOrignal != null && e.EmployeeLeave_Id == ca.Id).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                                var oEmployeeLeave = db.LvNewReq.Include(e => e.LeaveHead).Include(e => e.LvWFDetails).Where(a => !LvOrignal_id.Contains(a.Id) && a.LeaveHead != null && a.Narration.ToUpper() == "Leave Requisition".ToUpper() && (a.InputMethod == 1 || a.InputMethod == 2) && a.EmployeeLeave_Id == ca.Id && a.TrClosed == false && a.WFStatus.LookupVal != "0" && a.WFStatus.LookupVal != "2" && a.WFStatus.LookupVal != "3" && lvheadlists.Contains(a.LeaveHead.Id)).ToList();

                                if (oEmployeeLeave.Count() > 0)
                                {


                                    // var LvCalendarFilter = oEmployeeLeave.OrderBy(e => e.Id).ToList();

                                    //  var LvOrignal_id = oEmployeeLeave.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();

                                    // var Lv = ca.LvNewReq.Where(a => lvheadlists.Contains(a.LeaveHead.Id) && a.TrClosed == false && a.WFStatus.LookupVal != "0" && a.WFStatus.LookupVal != "2" && a.WFStatus.LookupVal != "3" && !LvOrignal_id.Contains(a.Id)).ToList();

                                    var Lv = oEmployeeLeave.ToList();

                                    foreach (var ca1 in Lv)
                                    {
                                        //if (ca1.TrClosed == false && (ca1.InputMethod == 1 || ca1.InputMethod == 2))
                                        //{
                                        foreach (var ca2 in ca1.LvWFDetails)
                                        {
                                            var WFStatusValue = "";
                                            if (ca2.WFStatus == 0)
                                            {
                                                WFStatusValue = "Applied";
                                            }
                                            if (ca2.WFStatus == 1)
                                            {
                                                WFStatusValue = "Sanction Apporved";
                                            }
                                            if (ca2.WFStatus == 2)
                                            {
                                                WFStatusValue = "Sanction Rejected";
                                            }
                                            if (ca2.WFStatus == 3)
                                            {
                                                WFStatusValue = "HR Apporved";
                                            }
                                            if (ca2.WFStatus == 4)
                                            {
                                                WFStatusValue = "HR Rejected";
                                            }
                                            if (ca2.WFStatus == 5)
                                            {
                                                WFStatusValue = "HRM(M)";
                                            }
                                            if (ca2.WFStatus == 6)
                                            {
                                                WFStatusValue = "Cancel";
                                            }

                                            GenericField100 OGenericGeoObjStatement = new GenericField100()
                                            {
                                                Fld1 = ca.Id.ToString(),
                                                Fld2 = ca.Employee.EmpCode.ToString(),
                                                Fld3 = ca.Employee.EmpName.FullNameFML.ToString(),
                                                Fld4 = ca1.LeaveHead == null ? "" : ca1.LeaveHead.LvCode.ToString(),
                                                Fld5 = ca1.FromDate == null ? "" : ca1.FromDate.Value.ToShortDateString(),
                                                Fld6 = ca1.ToDate == null ? "" : ca1.ToDate.Value.ToShortDateString(),
                                                Fld7 = ca1.DebitDays == null ? "" : ca1.DebitDays.ToString(),
                                                Fld8 = WFStatusValue.ToString(),
                                                Fld11 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null) ? ca.Employee.GeoStruct.Location.LocationObj.LocDesc : "",
                                                Fld12 = (ca.Employee.FuncStruct != null && ca.Employee.FuncStruct.Job != null) ? ca.Employee.FuncStruct.Job.Name : " ",
                                                Fld13 = (ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Department != null && ca.Employee.GeoStruct.Department.DepartmentObj != null) ? ca.Employee.GeoStruct.Department.DepartmentObj.DeptDesc : ""

                                            };
                                            //write data to generic class
                                            OGenericLvTransStatement.Add(OGenericGeoObjStatement);
                                        }
                                    }
                                }
                                //}
                            }
                            string lvpendingcreditprocesreportsmsg = "";
                            dynamic leavependingcreditprocessreportmesg = System.Web.HttpContext.Current.Session["LeaveCreditProcessReportMsg"];
                            if (leavependingcreditprocessreportmesg != null)
                            {
                                lvpendingcreditprocesreportsmsg = string.Join(",", leavependingcreditprocessreportmesg);
                                System.Web.HttpContext.Current.Response.Write("<script type='text/javascript'>alert('" + lvpendingcreditprocesreportsmsg + "')</script>");

                            }
                            return OGenericLvTransStatement;
                        }

                        break;

                    default:
                        return null;
                        break;
                }
            }
        }

        public class EncashHeadData
        {
            public SalaryHead SalHead { get; set; }
            public double Amount { get; set; }
        };

        public static List<double> LeaveEncashCalcProjection(int mEmployeePayroll_Id, string Processmonth, double encashdays)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var PREmployeePayroll = new List<EmployeePayroll>();
                List<int> oemp = db.EmployeePayroll.Where(e => e.Id == mEmployeePayroll_Id).Select(e => e.Id).ToList();
                foreach (var item in oemp)
                {
                    var OEmployeePayroll_t = db.EmployeePayroll

                        //.Include(e => e.SalaryT)
                        //.Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                        //.Include(e => e.EmpSalStruct)
                        //.Include(e => e.SalAttendance)
                        //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                        //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))

                        .Where(e => e.Id == item).SingleOrDefault();

                    if (OEmployeePayroll_t != null)
                    {

                        OEmployeePayroll_t.SalaryT = db.SalaryT.Where(e => e.PayMonth == Processmonth && e.EmployeePayroll_Id == OEmployeePayroll_t.Id).ToList();
                        foreach (var i in OEmployeePayroll_t.SalaryT)
                        {
                            i.SalEarnDedT = db.SalaryT.Where(e => e.Id == i.Id).Select(r => r.SalEarnDedT.ToList()).FirstOrDefault();
                        }
                        OEmployeePayroll_t.SalAttendance = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == OEmployeePayroll_t.Id && e.PayMonth == Processmonth).ToList();
                        OEmployeePayroll_t.EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll_t.Id && e.EndDate == null).ToList();
                        foreach (var j in OEmployeePayroll_t.EmpSalStruct)
                        {
                            j.EmpSalStructDetails = db.EmpSalStruct.Where(e => e.Id == j.Id).Select(r => r.EmpSalStructDetails.ToList()).FirstOrDefault();
                            foreach (var k in j.EmpSalStructDetails)
                            {
                                k.SalaryHead = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == k.SalaryHead_Id).FirstOrDefault();
                                k.SalHeadFormula = db.EmpSalStructDetails.Where(e => e.Id == k.Id).Select(r => r.SalHeadFormula).FirstOrDefault();
                            }
                        }
                        PREmployeePayroll.Add(OEmployeePayroll_t);
                    }

                }

                List<double> mLvEncashAmount = new List<double>();

                foreach (var ca in PREmployeePayroll)
                {
                    var OEmpSalStruct = ca.EmpSalStruct
                                                      .Where(e => e.EndDate == null).SingleOrDefault();
                    var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").SingleOrDefault();

                    if (OEmpSalDetails == null)//lvencash head present
                    {
                        return null;
                    }

                    //check salary data
                    var OSalChk = ca.SalaryT.Where(e => e.PayMonth == Processmonth && e.EmployeePayroll_Id == ca.Id).SingleOrDefault();
                    var CompanyId = Convert.ToInt32(SessionManager.CompanyId);
                    var Loc = db.Company.Include(e => e.Address).Include(e => e.Address.State)
                        .Where(e => e.Id == CompanyId).SingleOrDefault().Address.State.Code.ToUpper();

                    var companyCode = db.Company.Where(e => e.Id == CompanyId).SingleOrDefault();
                    if (OSalChk == null)
                    {
                        SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                        if (LVEncashFormula != null)
                        {
                            double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, null, OEmpSalStruct.EmpSalStructDetails.ToList());
                            //check for month days
                            //mLvEncashAmount[1] = mLvEncashWages;
                            mLvEncashAmount.Add(mLvEncashWages);
                            var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
                            if (OPayProcGrp != null)
                            {

                            }
                            //  25863/1*15
                            //var oDay = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth).Day;
                            //mLvEncashAmount = (mLvEncashWages / oDay * OLvEncashData.LvEncashReq.EncashDays);
                            DateTime processdt = Convert.ToDateTime("01/" + Processmonth);
                            int year = processdt.Year;
                            int month = processdt.Month;
                            var oDay = DateTime.DaysInMonth(year, month);
                            if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                            {
                                //mLvEncashAmount[0] = (mLvEncashWages / 30) * encashdays;
                                double EncashmentAMT = (mLvEncashWages / 30) * encashdays;
                                mLvEncashAmount.Add(EncashmentAMT);


                            }
                            else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                            {
                                //mLvEncashAmount[0] = (mLvEncashWages) - ((oDay - encashdays) / 30) * mLvEncashWages;
                                double EncashmentAmt = (mLvEncashWages) - ((oDay - encashdays) / 30) * mLvEncashWages;
                                mLvEncashAmount.Add(EncashmentAmt);
                            }
                            else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                            {
                                if (companyCode.Code == "KDCC")
                                {
                                    //mLvEncashAmount[0] = (mLvEncashWages / 30) * encashdays;
                                    double Encashmentamt = (mLvEncashWages / 30) * encashdays;
                                    mLvEncashAmount.Add(Encashmentamt);
                                }
                                else
                                {
                                    //mLvEncashAmount[0] = (mLvEncashWages / oDay) * encashdays;
                                    double Encashmentamts = (mLvEncashWages / oDay) * encashdays;
                                    mLvEncashAmount.Add(Encashmentamts);
                                }
                            }


                            mLvEncashAmount[1] = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount[1]);
                        }

                    }
                    else
                    {
                        SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                        if (LVEncashFormula != null)
                        {
                            double mLvEncashWages = Process.SalaryHeadGenProcess.Wagecalc(LVEncashFormula, OSalChk.SalEarnDedT.ToList(), null);
                            //mLvEncashAmount[1] = mLvEncashWages;
                            mLvEncashAmount.Add(mLvEncashWages);

                            var OAttChk = ca.SalAttendance.Where(e => e.PayMonth == Processmonth).SingleOrDefault();
                            var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
                            if (OPayProcGrp != null)
                            {
                                //For Chennai hardcode
                                if (Loc == "TAMILNADU")
                                {
                                    OPayProcGrp.PayMonthConcept.LookupVal = "FIXED30DAYS";
                                }
                                if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                {
                                    //mLvEncashAmount[0] = (mLvEncashWages / 30) * encashdays;
                                    double EnCASHAmt = (mLvEncashWages / 30) * encashdays;
                                    mLvEncashAmount.Add(EnCASHAmt);
                                }
                                else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                {
                                    // mLvEncashAmount[0] = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                                    double EnCASHAMT = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                                    mLvEncashAmount.Add(EnCASHAMT);
                                }
                                else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                {
                                    //ca.Amount * ((SalAttendanceT_PayableDays) / (SalAttendanceT_monthDays));
                                    //mLvEncashAmount = (mLvEncashWages) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * mLvEncashWages;
                                    if (companyCode.Code == "KDCC")
                                    {

                                        // mLvEncashAmount[0] = (((mLvEncashWages * OAttChk.MonthDays) / OAttChk.PaybleDays) / 30) * encashdays;
                                        double EncashAmtt = (((mLvEncashWages * OAttChk.MonthDays) / OAttChk.PaybleDays) / 30) * encashdays;
                                        mLvEncashAmount.Add(EncashAmtt);
                                    }
                                    else
                                    {
                                        // mLvEncashAmount[0] = (mLvEncashWages / OAttChk.PaybleDays) * encashdays;
                                        double EncashAmts = (mLvEncashWages / OAttChk.PaybleDays) * encashdays;
                                        mLvEncashAmount.Add(EncashAmts);
                                    }
                                }
                            }
                            mLvEncashAmount[1] = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount[1]);

                        }
                    }

                }
                return mLvEncashAmount;
            }
        }


        public static IList<EncashHeadData> LeaveEncashCalc(int mEmployeePayroll_Id, LvEncashPayment OLvEncashData)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var OEmployeePayroll = db.EmployeePayroll
                    .Include(e => e.LvEncashPayment)
                    .Include(e => e.SalaryT)
                    .Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                    .Include(e => e.EmpSalStruct)
                    .Include(e => e.SalAttendance)
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))
                    .Where(e => e.Id == mEmployeePayroll_Id).SingleOrDefault();

                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct
                                   .Where(e => e.EndDate == null).SingleOrDefault();
                var OEmpSalDetails = OEmpSalStruct.EmpSalStructDetails.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").SingleOrDefault();
                double mLvEncashAmount = 0;
                IList<EncashHeadData> encashdata = null;

                //check salary data
                var OSalChk = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).SingleOrDefault();
                if (OSalChk == null)
                {
                    SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                    if (LVEncashFormula != null)
                    {
                        encashdata = Wagecalc(LVEncashFormula, null, OEmpSalStruct.EmpSalStructDetails.ToList(), OLvEncashData, null);
                        //check for month days
                        var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();

                        var oDay = Convert.ToDateTime("01/" + OLvEncashData.ProcessMonth).Day;
                        // mLvEncashAmount = (mLvEncashWages / oDay * OLvEncashData.LvEncashReq.EncashDays);
                        mLvEncashAmount = SalaryHeadGenProcess.RoundingFunction(OEmpSalDetails.SalaryHead, mLvEncashAmount);
                    }

                }
                else
                {
                    SalHeadFormula LVEncashFormula = OEmpSalDetails.SalHeadFormula;// Process.SalaryHeadGenProcess.SalFormulaFinder(OEmpSalStruct, OEmpSalDetails.SalaryHead.Id, db);
                    if (LVEncashFormula != null)
                    {
                        var OAttChk = OEmployeePayroll.SalAttendance.Where(e => e.PayMonth == OLvEncashData.ProcessMonth).SingleOrDefault();
                        encashdata = Wagecalc(LVEncashFormula, OSalChk.SalEarnDedT.ToList(), null, OLvEncashData, OAttChk);

                    }
                }
                return encashdata;
            }
        }

        public static IList<EncashHeadData> Wagecalc(SalHeadFormula OSalHeadFormula, List<SalEarnDedT> OSalaryDetails, List<EmpSalStructDetails> OEmpSalStructDetails, LvEncashPayment OLvEncashData, SalAttendanceT OAttChk)
        {
            List<EncashHeadData> model = new List<EncashHeadData>();
            var view = new EncashHeadData();
            double mLvEncashAmount = 0;
            using (DataBaseContext db = new DataBaseContext())
            {

                var OSalHeadFormualQuery = db.SalHeadFormula.Where(e => e.Id == OSalHeadFormula.Id)
                    .Include(e => e.SalWages)
                    .Include(e => e.SalWages.RateMaster).Include(e => e.SalWages.RateMaster.Select(r => r.SalHead)).SingleOrDefault();

                var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayFrequency).SingleOrDefault();
                if (OSalHeadFormualQuery.SalWages != null)
                {
                    if (OSalaryDetails != null)
                    {
                        var SalHeadDet = OSalHeadFormualQuery.SalWages.RateMaster
                            .Join(OSalaryDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir }).Select(e => e.uir);
                        foreach (var i in SalHeadDet)
                        {


                            if (OPayProcGrp != null)
                            {
                                if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                {
                                    mLvEncashAmount = (i.Amount / 30) * OLvEncashData.LvEncashReq.EncashDays;
                                }
                                else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                {
                                    mLvEncashAmount = (i.Amount) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * i.Amount;
                                }
                                else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                {
                                    mLvEncashAmount = (i.Amount / OAttChk.PaybleDays) * OLvEncashData.LvEncashReq.EncashDays;
                                }
                            }
                            view = new EncashHeadData()
                            {
                                SalHead = i.SalaryHead,
                                Amount = mLvEncashAmount
                            };
                            model.Add(view);
                        }
                    }
                    else if (OEmpSalStructDetails != null)
                    {
                        var SalHeadDet = OSalHeadFormualQuery.SalWages.RateMaster
                            .Join(OEmpSalStructDetails, u => u.SalHead.Id, uir => uir.SalaryHead.Id,
                                    (u, uir) => new { u, uir }).Select(e => e.uir);



                        foreach (var i in SalHeadDet)
                        {
                            if (OPayProcGrp != null)
                            {
                                if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                                {
                                    mLvEncashAmount = (i.Amount / 30) * OLvEncashData.LvEncashReq.EncashDays;
                                }
                                else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                                {
                                    mLvEncashAmount = (i.Amount) - ((OAttChk.MonthDays - OAttChk.PaybleDays) / 30) * i.Amount;
                                }
                                else if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "CALENDAR")
                                {
                                    mLvEncashAmount = (i.Amount / OAttChk.PaybleDays) * OLvEncashData.LvEncashReq.EncashDays;
                                }
                            }
                            view = new EncashHeadData()
                            {
                                SalHead = i.SalaryHead,
                                Amount = mLvEncashAmount
                            };
                            model.Add(view);
                        }
                    }

                    else
                        return model;
                }
                else
                    return model;
            }
            return model;
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