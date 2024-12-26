using EssPortal.App_Start;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace EssPortal.Process
{
    public class PortalRights
    {
        public class AccessRightsData
        {
            public int AccessRights { get; set; }
            public string AccessRightsLookup { get; set; }
            public string ModuleName { get; set; }
            public string SubModuleName { get; set; }
            public List<int> ReportingEmployee { get; set; }
            public int LvNoOfDaysFrom { get; set; }
            public int LvNoOfDaysTo { get; set; }
        };
        //    public static DataBaseContext db = new DataBaseContext();
        public static List<ReportingStructRights> ScanRights(int User_Id, int EmployeeId, int CompanyId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = "555", CreatedOn = DateTime.Now };
                var OEmployee = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.ReportingStructRights.Select(r => r.AccessRights))
                    .Include(e => e.ReportingStructRights.Select(r => r.AccessRights.ActionName))
                    .Include(e => e.ReportingStructRights.Select(r => r.FuncModules))
                    .Include(e => e.ReportingStructRights.Select(r => r.FuncSubModules))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct.FuncStruct))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct.GeoFuncList))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct.GeoGraphList))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.BossEmp.EmpName))
                    .Where(e => e.Id == EmployeeId)
                    .SingleOrDefault();
                List<ReportingStructRights> ReportStructRightList = new List<ReportingStructRights>();
                #region rights
                var OEmpReportStruct = db.Company
                    .Include(e => e.ReportingStructRights)
                    .Include(e => e.ReportingStructRights.Select(r => r.AccessRights))
                    .Include(e => e.ReportingStructRights.Select(r => r.AccessRights.ActionName))
                    .Include(e => e.ReportingStructRights.Select(r => r.FuncModules))
                    .Include(e => e.ReportingStructRights.Select(r => r.FuncSubModules))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct.FuncStruct))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct.GeoFuncList))
                    .Include(e => e.ReportingStructRights.Select(r => r.ReportingStruct.GeoGraphList))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.BossEmp.EmpName))
                    .Where(e => e.Id == CompanyId)
                    .SingleOrDefault();
                if (OEmpReportStruct.ReportingStructRights != null && OEmpReportStruct.ReportingStructRights.Count() > 0)
                {
                    foreach (var ca in OEmpReportStruct.ReportingStructRights)
                    {
                        bool IsDivIncharge = false;
                        bool IsLocIncharge = false;
                        bool IsUnitIncharge = false;
                        bool IsGrpIncharge = false;
                        bool IsDeptIncharge = false;
                        bool IsRoleIncharge = false;
                        bool IsIndividualIncharge = false;
                        Int32 FuncStructIds = 0;
                        Int32 IndEmpIds = 0;
                        #region rightscheck
                        if (ca.ReportingStruct != null)
                        {
                            if (ca.ReportingStruct.GeographicalAppl == true)
                            {
                                if (ca.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "DIVISION")
                                {
                                    IsDivIncharge = true;
                                    //check wheather division incharge
                                }
                                else if (ca.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "LOCATION")
                                {
                                    IsLocIncharge = true;
                                    //check wheather location incharge
                                }
                                else if (ca.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "UNIT")
                                {
                                    IsUnitIncharge = true;
                                    //check wheather unit incharge
                                }
                                else if (ca.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "GROUP")
                                {
                                    IsGrpIncharge = true;
                                    //check wheather group incharge
                                }
                                //check for employee 
                            }
                            if (ca.ReportingStruct.FunctionalAppl == true)
                            {
                                if (ca.ReportingStruct.GeoFuncList.LookupVal.ToUpper() == "DEPARTMENT")
                                {
                                    IsDeptIncharge = true;
                                    //check wheather department incharge
                                }
                                //check for employee 
                            }
                            if (ca.ReportingStruct.RoleBasedAppl == true)
                            {
                                //get funcstruct id

                                FuncStructIds = ca.ReportingStruct.FuncStruct.Id;
                                IsRoleIncharge = true;
                                //check wheather job incharge

                                //check for employee 
                            }
                            if (ca.ReportingStruct.IndividualAppl == true)
                            {
                                IndEmpIds = ca.ReportingStruct.BossEmp.Id;
                                IsIndividualIncharge = true;
                            }
                        #endregion rightscheck
                            #region rightsvalidation
                            if (IsDivIncharge == true)
                            {
                                var OObj = db.Company
                                    .Include(e => e.Divisions)
                                    .Include(e => e.Divisions.Select(r => r.Incharge))
                                    .Where(e => e.Id == CompanyId).SingleOrDefault();
                                var OObjChk = OObj.Divisions.Where(e => e.Incharge != null && e.Incharge.Id == EmployeeId).FirstOrDefault();
                                if (OObjChk == null)
                                {
                                    IsDivIncharge = false;
                                }
                                else
                                {
                                    ReportStructRightList.Add(ca);
                                }
                            }
                            if (IsLocIncharge == true)
                            {
                                var OObj = db.Company
                                    .Include(e => e.Location)
                                    .Include(e => e.Location.Select(r => r.Incharge))
                                    .Where(e => e.Id == CompanyId).SingleOrDefault();
                                var OObjChk = OObj.Location.Where(e => e.Incharge != null && e.Incharge.Id == EmployeeId).FirstOrDefault();
                                if (OObjChk == null)
                                {
                                    IsLocIncharge = false;
                                }
                                else
                                {
                                    ReportStructRightList.Add(ca);
                                }
                            }
                            if (IsUnitIncharge == true)
                            {
                                var OObj = db.Company
                                    .Include(e => e.Unit)
                                    .Include(e => e.Unit.Select(r => r.Incharge))
                                    .Where(e => e.Id == CompanyId).SingleOrDefault();
                                var OObjChk = OObj.Unit.Where(e => e.Incharge != null && e.Incharge.Id == EmployeeId).FirstOrDefault();
                                if (OObjChk == null)
                                {
                                    IsUnitIncharge = false;
                                }
                                else
                                {
                                    ReportStructRightList.Add(ca);
                                }
                            }
                            if (IsGrpIncharge == true)
                            {
                                var OObj = db.Company
                                    .Include(e => e.Group)
                                    .Include(e => e.Group.Select(r => r.Incharge))
                                    .Where(e => e.Id == CompanyId).SingleOrDefault();
                                var OObjChk = OObj.Group.Where(e => e.Incharge != null && e.Incharge.Id == EmployeeId).FirstOrDefault();
                                if (OObjChk == null)
                                {
                                    IsGrpIncharge = false;
                                }
                                else
                                {
                                    ReportStructRightList.Add(ca);
                                }
                            }
                            if (IsDeptIncharge == true)
                            {
                                var OObj = db.Company
                                    .Include(e => e.Department)
                                    .Include(e => e.Department.Select(r => r.Incharge))
                                    .Where(e => e.Id == CompanyId).SingleOrDefault();
                                var OObjChk = OObj.Department.Where(e => e.Incharge != null && e.Incharge.Id == EmployeeId).FirstOrDefault();
                                if (OObjChk == null)
                                {
                                    IsDeptIncharge = false;
                                }
                                else
                                {
                                    ReportStructRightList.Add(ca);
                                }
                            }
                            if (IsRoleIncharge == true)
                            {
                                if (FuncStructIds == OEmployee.FuncStruct.Id != true)
                                {
                                    IsRoleIncharge = false;
                                }
                                else
                                    ReportStructRightList.Add(ca);
                            }
                            if (IsIndividualIncharge == true)
                            {
                                if (IndEmpIds == OEmployee.Id != true)
                                {
                                    IsIndividualIncharge = false;
                                }
                                else
                                    ReportStructRightList.Add(ca);
                            }



                            #endregion rightsvalidation
                        }
                    }
                    return ReportStructRightList;
                }
                return null;
                #endregion rights
            }
        }
        //return employee ids,module name, submodule,right
        public static List<AccessRightsData> AssignRightsEmpList(int CompanyId, int User_Id, Employee ReportedEmployee, int FuncModulesId, int FuncSubModulesId, List<ReportingStructRights> OReportingStructRightsList)
        {
            //OReportingStructRightsList = ScanRights(User_Id, ReportedEmployee.Id, CompanyId);
            using (DataBaseContext db = new DataBaseContext())
            {
                List<AccessRightsData> OARData = new List<AccessRightsData>();
                var OReportingStruct = FuncSubModulesId == 0 ? OReportingStructRightsList.Where(e => e.FuncModules.Id == FuncModulesId).FirstOrDefault() : OReportingStructRightsList.Where(e => e.FuncModules.Id == FuncModulesId && e.FuncSubModules.Id == FuncSubModulesId).FirstOrDefault();
                if (OReportingStruct != null)
                {
                    #region geo
                    if (OReportingStruct.ReportingStruct.GeographicalAppl == true)
                    {
                        if (OReportingStruct.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "DIVISION")
                        {
                            var OEmpId = db.Employee
                                .Include(e => e.GeoStruct.Company)
                                .Include(e => e.GeoStruct.Division)
                                .Where(e =>
                                    //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id) &&
                                    e.GeoStruct.Company.Id == CompanyId
                                    && e.GeoStruct.Division != null
                                    && e.GeoStruct.Division.Id == ReportedEmployee.GeoStruct.Division.Id
                                    //&& e.Id != ReportedEmployee.Id
                                    )
                                .Select(r => r.Id)
                                .ToList();
                            AccessRightsData Odata = new AccessRightsData();
                            Odata.AccessRights = OReportingStruct.AccessRights.Id;
                            Odata.AccessRightsLookup = OReportingStruct.AccessRights.ActionName.LookupVal;
                            Odata.ReportingEmployee = OEmpId;
                            Odata.ModuleName = OReportingStruct.FuncModules.LookupVal.ToUpper();
                            Odata.SubModuleName = OReportingStruct.FuncSubModules != null ? OReportingStruct.FuncSubModules.LookupVal.ToUpper() : null;

                            OARData.Add(Odata);

                        }
                        if (OReportingStruct.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "LOCATION")
                        {
                            var OEmpId = db.Employee
                                .Include(e => e.GeoStruct.Company)
                                .Include(e => e.GeoStruct.Location)
                                .Where(e =>
                                    //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id)
                                    e.GeoStruct.Company != null
                                    && e.GeoStruct.Company.Id == CompanyId
                                    && e.GeoStruct.Location != null
                                    && e.GeoStruct.Location.Id == ReportedEmployee.GeoStruct.Location.Id
                                    //&& e.Id != ReportedEmployee.Id
                                    )
                                .Select(r => r.Id)
                                .ToList();
                            AccessRightsData Odata = new AccessRightsData();
                            Odata.AccessRights = OReportingStruct.AccessRights.Id;
                            Odata.AccessRightsLookup = OReportingStruct.AccessRights.ActionName.LookupVal;
                            Odata.ReportingEmployee = OEmpId;
                            Odata.ModuleName = OReportingStruct.FuncModules.LookupVal.ToUpper();
                            Odata.SubModuleName = OReportingStruct.FuncSubModules != null ? OReportingStruct.FuncSubModules.LookupVal.ToUpper() : null;

                            OARData.Add(Odata);

                        }
                        if (OReportingStruct.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "UNIT")
                        {
                            var OEmpId = db.Employee
                                .Include(e => e.GeoStruct.Company)
                                .Include(e => e.GeoStruct.Unit)
                                .Where(e =>
                                    //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id)&&
                                     e.GeoStruct.Company.Id == CompanyId
                                    && e.GeoStruct.Unit.Id == ReportedEmployee.GeoStruct.Unit.Id
                                    //&& e.Id != ReportedEmployee.Id
                                    )
                                .Select(r => r.Id)
                                .ToList();
                            AccessRightsData Odata = new AccessRightsData();
                            Odata.AccessRights = OReportingStruct.AccessRights.Id;
                            Odata.AccessRightsLookup = OReportingStruct.AccessRights.ActionName.LookupVal;
                            Odata.ReportingEmployee = OEmpId;
                            Odata.ModuleName = OReportingStruct.FuncModules.LookupVal.ToUpper();
                            Odata.SubModuleName = OReportingStruct.FuncSubModules != null ? OReportingStruct.FuncSubModules.LookupVal.ToUpper() : null;

                            OARData.Add(Odata);

                        }
                        if (OReportingStruct.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "GROUP")
                        {
                            var OEmpId = db.Employee
                                .Include(e => e.GeoStruct.Company)
                                .Include(e => e.GeoStruct.Group)
                                .Where(e =>
                                    //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id)&&
                                     e.GeoStruct.Company.Id == CompanyId
                                    && e.GeoStruct.Group.Id == ReportedEmployee.GeoStruct.Group.Id
                                    //&& e.Id != ReportedEmployee.Id
                                    )
                                .Select(r => r.Id)
                                .ToList();
                            AccessRightsData Odata = new AccessRightsData();
                            Odata.AccessRights = OReportingStruct.AccessRights.Id;
                            Odata.AccessRightsLookup = OReportingStruct.AccessRights.ActionName.LookupVal;
                            Odata.ReportingEmployee = OEmpId;
                            Odata.ModuleName = OReportingStruct.FuncModules.LookupVal.ToUpper();
                            Odata.SubModuleName = OReportingStruct.FuncSubModules != null ? OReportingStruct.FuncSubModules.LookupVal.ToUpper() : null;

                            OARData.Add(Odata);

                        }
                    }
                    #endregion geo
                    #region func
                    if (OReportingStruct.ReportingStruct.FunctionalAppl == true)
                    {
                        if (OReportingStruct.ReportingStruct.GeoFuncList.LookupVal.ToUpper() == "DEPARTMENT")
                        {
                            var OEmpId = db.Employee
                                .Include(e => e.GeoStruct.Company)
                                .Include(e => e.GeoStruct.Department)
                                .Where(e =>
                                    e.GeoStruct.Company.Id == CompanyId
                                    && e.GeoStruct.Department.Id == ReportedEmployee.GeoStruct.Department.Id
                                    //&& e.Id != ReportedEmployee.Id
                                    )
                                .Select(r => r.Id)
                                .ToList();
                            AccessRightsData Odata = new AccessRightsData();
                            Odata.AccessRights = OReportingStruct.AccessRights.Id;
                            Odata.AccessRightsLookup = OReportingStruct.AccessRights.ActionName.LookupVal;
                            Odata.ReportingEmployee = OEmpId;
                            OARData.Add(Odata);

                        }

                    }
                    #endregion function
                    #region role
                    if (OReportingStruct.ReportingStruct.RoleBasedAppl == true)
                    {
                        var OEmpId = db.Employee
                             .Include(e => e.GeoStruct.Company)
                             .Include(e => e.FuncStruct)
                             .Where(e =>
                                 //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id) &&
                                 e.ReportingStructRights.Any(a => a.ReportingStruct.FuncStruct.Id == ReportedEmployee.FuncStruct.Id) &&
                                 e.GeoStruct.Company.Id == CompanyId
                                 //&& e.Id != ReportedEmployee.Id
                                 )
                             .Select(r => r.Id)
                             .ToList();
                        AccessRightsData Odata = new AccessRightsData();
                        Odata.AccessRights = OReportingStruct.AccessRights.Id;
                        Odata.AccessRightsLookup = OReportingStruct.AccessRights.ActionName.LookupVal;
                        Odata.ReportingEmployee = OEmpId;
                        Odata.ModuleName = OReportingStruct.FuncModules.LookupVal.ToUpper();
                        Odata.SubModuleName = OReportingStruct.FuncSubModules != null ? OReportingStruct.FuncSubModules.LookupVal.ToUpper() : null;

                        OARData.Add(Odata);

                    }
                    #endregion role
                    return OARData;
                }
            }
            return null;
        }
        public static List<AccessRightsData> AssignRightsEmpListWithFunSubmodule(int CompanyId, int User_Id, Employee ReportedEmployee, int FuncModulesId, int FuncSubModulesId, List<ReportingStructRights> OReportingStructRightsList, string Accessright)
        {
            //OReportingStructRightsList = ScanRights(User_Id, ReportedEmployee.Id, CompanyId);
            using (DataBaseContext db = new DataBaseContext())
            {
                List<AccessRightsData> OARData = new List<AccessRightsData>();
                var OReportingStruct = FuncSubModulesId == 0 ? OReportingStructRightsList.Where(e => e.FuncModules.Id == FuncModulesId && e.AccessRights.ActionName.LookupVal.ToUpper() == Accessright.ToUpper()).ToList() : OReportingStructRightsList.Where(e => e.FuncModules.Id == FuncModulesId && e.FuncSubModules.Id == FuncSubModulesId).ToList();
                if (OReportingStruct != null)
                {
                    foreach (var item1 in OReportingStruct)
                    {

                        List<int> Employeeid = new List<int>();
                        #region geo
                        if (item1.ReportingStruct.GeographicalAppl == true)
                        {
                            if (item1.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "DIVISION")
                            {
                                //var OEmpId = db.Employee
                                //    .Include(e => e.GeoStruct.Company)
                                //    .Include(e => e.GeoStruct.Division)
                                //    .Include(e => e.ReportingStructRights)
                                //    .Where(e =>
                                //        //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id) &&
                                //        e.GeoStruct.Company.Id == CompanyId
                                //        && e.GeoStruct.Division != null
                                //            // && e.ReportingStructRights.SingleOrDefault().Id==OReportingStruct.Id
                                //       // && e.GeoStruct.Division.Id == ReportedEmployee.GeoStruct.Division.Id
                                //    //&& e.Id != ReportedEmployee.Id
                                //        )
                                //    .Select(r => r)
                                //    .ToList();
                                var geostructid = db.GeoStruct
                                  .Include(e => e.Division)
                                  .AsNoTracking()
                                  .Where(e => e.Division.Incharge.Id == ReportedEmployee.Id)
                                  .Select(e => e.Id)
                                  .ToList();
                                var OEmpId = db.Employee
                                      .Include(e => e.GeoStruct)
                                      .Include(e => e.GeoStruct.Company)
                                       .Include(e => e.ReportingStructRights)
                                      .AsNoTracking()
                                      .Where(e => geostructid.Contains(e.GeoStruct.Id) && e.GeoStruct.Company.Id == CompanyId).ToList();
                                foreach (var item in OEmpId)
                                {
                                    // item.Id
                                    var Empreportingid = item.ReportingStructRights.Where(e => e.Id == item1.Id).ToList();
                                    if (Empreportingid.Count() > 0)
                                    {
                                        Employeeid.Add(item.Id);
                                    }
                                }
                                AccessRightsData Odata = new AccessRightsData();
                                Odata.AccessRights = item1.AccessRights.Id;
                                Odata.AccessRightsLookup = item1.AccessRights.ActionName.LookupVal;
                                Odata.ReportingEmployee = Employeeid;
                                Odata.ModuleName = item1.FuncModules.LookupVal.ToUpper();
                                Odata.SubModuleName = item1.FuncSubModules != null ? item1.FuncSubModules.LookupVal.ToUpper() : null;
                                Odata.LvNoOfDaysFrom = item1.AccessRights.LvNoOfDaysFrom;
                                Odata.LvNoOfDaysTo = item1.AccessRights.LvNoOfDaysTo;
                                OARData.Add(Odata);

                            }
                            if (item1.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "LOCATION")
                            {
                                //var OEmpId = db.Employee
                                //    .Include(e => e.GeoStruct.Company)
                                //    .Include(e => e.GeoStruct.Location)
                                //    .Include(e => e.ReportingStructRights)
                                //    .Where(e =>
                                //        //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id)
                                //        e.GeoStruct.Company != null
                                //        && e.GeoStruct.Company.Id == CompanyId
                                //        && e.GeoStruct.Location != null
                                //       // && e.GeoStruct.Location.Id == ReportedEmployee.GeoStruct.Location.Id
                                //    //&& e.Id != ReportedEmployee.Id
                                //        )
                                //    .Select(r => r)
                                //    .ToList();

                                var geostructid = db.GeoStruct
                                    .Include(e => e.Location)
                                    .AsNoTracking()
                                    .Where(e => e.Location.Incharge.Id == ReportedEmployee.Id)
                                    .Select(e => e.Id)
                                    .ToList();
                                var OEmpId = db.Employee
                                      .Include(e => e.GeoStruct)
                                       .Include(e => e.ReportingStructRights)
                                      .Include(e => e.GeoStruct.Company)
                                      .AsNoTracking()
                                      .Where(e => geostructid.Contains(e.GeoStruct.Id) && e.GeoStruct.Company.Id == CompanyId).ToList();
                                foreach (var item in OEmpId)
                                {
                                    // item.Id
                                    var Empreportingid = item.ReportingStructRights.Where(e => e.Id == item1.Id).ToList();
                                    if (Empreportingid.Count() > 0)
                                    {
                                        Employeeid.Add(item.Id);
                                    }
                                }
                                AccessRightsData Odata = new AccessRightsData();
                                Odata.AccessRights = item1.AccessRights.Id;
                                Odata.AccessRightsLookup = item1.AccessRights.ActionName.LookupVal;
                                Odata.ReportingEmployee = Employeeid;
                                Odata.ModuleName = item1.FuncModules.LookupVal.ToUpper();
                                Odata.SubModuleName = item1.FuncSubModules != null ? item1.FuncSubModules.LookupVal.ToUpper() : null;
                                Odata.LvNoOfDaysFrom = item1.AccessRights.LvNoOfDaysFrom;
                                Odata.LvNoOfDaysTo = item1.AccessRights.LvNoOfDaysTo;
                                OARData.Add(Odata);

                            }
                            if (item1.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "UNIT")
                            {
                                //var OEmpId = db.Employee
                                //    .Include(e => e.GeoStruct.Company)
                                //    .Include(e => e.GeoStruct.Unit)
                                //    .Include(e => e.ReportingStructRights)
                                //    .Where(e =>
                                //        //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id)&&
                                //         e.GeoStruct.Company.Id == CompanyId
                                //      //  && e.GeoStruct.Unit.Id == ReportedEmployee.GeoStruct.Unit.Id
                                //    //&& e.Id != ReportedEmployee.Id
                                //        )
                                //    .Select(r => r)
                                //    .ToList();
                                var geostructid = db.GeoStruct
                                  .Include(e => e.Unit)
                                  .AsNoTracking()
                                  .Where(e => e.Unit.Incharge.Id == ReportedEmployee.Id)
                                  .Select(e => e.Id)
                                  .ToList();
                                var OEmpId = db.Employee
                                      .Include(e => e.GeoStruct)
                                       .Include(e => e.ReportingStructRights)
                                      .Include(e => e.GeoStruct.Company)
                                      .AsNoTracking()
                                      .Where(e => geostructid.Contains(e.GeoStruct.Id) && e.GeoStruct.Company.Id == CompanyId).ToList();
                                foreach (var item in OEmpId)
                                {
                                    // item.Id
                                    var Empreportingid = item.ReportingStructRights.Where(e => e.Id == item1.Id).ToList();
                                    if (Empreportingid.Count() > 0)
                                    {
                                        Employeeid.Add(item.Id);
                                    }
                                }
                                AccessRightsData Odata = new AccessRightsData();
                                Odata.AccessRights = item1.AccessRights.Id;
                                Odata.AccessRightsLookup = item1.AccessRights.ActionName.LookupVal;
                                Odata.ReportingEmployee = Employeeid;
                                Odata.ModuleName = item1.FuncModules.LookupVal.ToUpper();
                                Odata.SubModuleName = item1.FuncSubModules != null ? item1.FuncSubModules.LookupVal.ToUpper() : null;
                                Odata.LvNoOfDaysFrom = item1.AccessRights.LvNoOfDaysFrom;
                                Odata.LvNoOfDaysTo = item1.AccessRights.LvNoOfDaysTo;
                                OARData.Add(Odata);

                            }
                            if (item1.ReportingStruct.GeoGraphList.LookupVal.ToUpper() == "GROUP")
                            {
                                //var OEmpId = db.Employee
                                //    .Include(e => e.GeoStruct.Company)
                                //    .Include(e => e.GeoStruct.Group)
                                //    .Include(e => e.ReportingStructRights)
                                //    .Where(e =>
                                //        //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id)&&
                                //         e.GeoStruct.Company.Id == CompanyId
                                //        && e.GeoStruct.Group.Id == ReportedEmployee.GeoStruct.Group.Id
                                //    //&& e.Id != ReportedEmployee.Id
                                //        )
                                //    .Select(r => r)
                                //    .ToList();
                                var geostructid = db.GeoStruct
                                  .Include(e => e.Group)
                                  .AsNoTracking()
                                  .Where(e => e.Group.Incharge.Id == ReportedEmployee.Id)
                                  .Select(e => e.Id)
                                  .ToList();
                                var OEmpId = db.Employee
                                      .Include(e => e.GeoStruct)
                                       .Include(e => e.ReportingStructRights)
                                      .Include(e => e.GeoStruct.Company)
                                      .AsNoTracking()
                                      .Where(e => geostructid.Contains(e.GeoStruct.Id) && e.GeoStruct.Company.Id == CompanyId).ToList();
                                foreach (var item in OEmpId)
                                {
                                    // item.Id
                                    var Empreportingid = item.ReportingStructRights.Where(e => e.Id == item1.Id).ToList();
                                    if (Empreportingid.Count() > 0)
                                    {
                                        Employeeid.Add(item.Id);
                                    }
                                }
                                AccessRightsData Odata = new AccessRightsData();
                                Odata.AccessRights = item1.AccessRights.Id;
                                Odata.AccessRightsLookup = item1.AccessRights.ActionName.LookupVal;
                                Odata.ReportingEmployee = Employeeid;
                                Odata.ModuleName = item1.FuncModules.LookupVal.ToUpper();
                                Odata.SubModuleName = item1.FuncSubModules != null ? item1.FuncSubModules.LookupVal.ToUpper() : null;
                                Odata.LvNoOfDaysFrom = item1.AccessRights.LvNoOfDaysFrom;
                                Odata.LvNoOfDaysTo = item1.AccessRights.LvNoOfDaysTo;
                                OARData.Add(Odata);

                            }
                        }
                        #endregion geo
                        #region func
                        if (item1.ReportingStruct.FunctionalAppl == true)
                        {
                            if (item1.ReportingStruct.GeoFuncList.LookupVal.ToUpper() == "DEPARTMENT")
                            {
                                //var OEmpId = db.Employee
                                //    .Include(e => e.GeoStruct.Company)
                                //    .Include(e => e.GeoStruct.Department)
                                //    .Include(e => e.ReportingStructRights)
                                //    .Where(e =>
                                //        e.GeoStruct.Company.Id == CompanyId
                                //       // && e.GeoStruct.Department.Id == ReportedEmployee.GeoStruct.Department.Id
                                //    //&& e.Id != ReportedEmployee.Id
                                //        )
                                //    .Select(r => r)
                                //    .ToList();
                                var geostructid = db.GeoStruct
                                   .Include(e => e.Department)
                                   .AsNoTracking()
                                   .Where(e => e.Department.Incharge.Id == ReportedEmployee.Id)
                                   .Select(e => e.Id)
                                   .ToList();
                                var OEmpId = db.Employee
                                     .Include(e => e.GeoStruct)
                                      .Include(e => e.ReportingStructRights)
                                      .Include(e => e.GeoStruct.Company)
                                      .AsNoTracking()
                                      .Where(e => geostructid.Contains(e.GeoStruct.Id) && e.GeoStruct.Company.Id == CompanyId).ToList();
                                foreach (var item in OEmpId)
                                {
                                    // item.Id
                                    var Empreportingid = item.ReportingStructRights.Where(e => e.Id == item1.Id).ToList();
                                    if (Empreportingid.Count() > 0)
                                    {
                                        Employeeid.Add(item.Id);
                                    }
                                }
                                AccessRightsData Odata = new AccessRightsData();
                                Odata.AccessRights = item1.AccessRights.Id;
                                Odata.AccessRightsLookup = item1.AccessRights.ActionName.LookupVal;
                                Odata.ReportingEmployee = Employeeid;
                                Odata.ModuleName = item1.FuncModules.LookupVal.ToUpper();
                                Odata.SubModuleName = item1.FuncSubModules != null ? item1.FuncSubModules.LookupVal.ToUpper() : null;
                                Odata.LvNoOfDaysFrom = item1.AccessRights.LvNoOfDaysFrom;
                                Odata.LvNoOfDaysTo = item1.AccessRights.LvNoOfDaysTo;
                                OARData.Add(Odata);

                            }

                        }
                        #endregion function
                        #region role
                        if (item1.ReportingStruct.RoleBasedAppl == true)
                        {
                            var OEmpId = db.Employee
                                 .Include(e => e.GeoStruct.Company)
                                 .Include(e => e.FuncStruct)
                                 .Include(e => e.ReportingStructRights)
                                 .Where(e =>
                                     //e.ReportingStructRights.Any(a => a.Id == OReportingStruct.Id) &&
                                     //   e.ReportingStructRights.Any(a => a.ReportingStruct.FuncStruct.Id == ReportedEmployee.FuncStruct.Id) &&
                                     e.GeoStruct.Company.Id == CompanyId
                                //&& e.Id != ReportedEmployee.Id
                                     )
                                 .Select(r => r)
                                 .ToList();
                            foreach (var item in OEmpId)
                            {
                                // item.Id
                                var Empreportingid = item.ReportingStructRights.Where(e => e.Id == item1.Id).ToList();
                                if (Empreportingid.Count() > 0)
                                {
                                    Employeeid.Add(item.Id);
                                }
                            }
                            AccessRightsData Odata = new AccessRightsData();
                            Odata.AccessRights = item1.AccessRights.Id;
                            Odata.AccessRightsLookup = item1.AccessRights.ActionName.LookupVal;
                            Odata.ReportingEmployee = Employeeid;
                            Odata.ModuleName = item1.FuncModules.LookupVal.ToUpper();
                            Odata.SubModuleName = item1.FuncSubModules != null ? item1.FuncSubModules.LookupVal.ToUpper() : null;
                            Odata.LvNoOfDaysFrom = item1.AccessRights.LvNoOfDaysFrom;
                            Odata.LvNoOfDaysTo = item1.AccessRights.LvNoOfDaysTo;
                            OARData.Add(Odata);

                        }
                        #endregion role

                    }
                    return OARData;
                }
            }

            return null;
        }
    }
}
