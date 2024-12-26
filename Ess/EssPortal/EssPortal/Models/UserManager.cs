using System.Data.Entity.Core.Objects;
using Leave;
using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using EssPortal.Security;
using EssPortal.Process;
using Payroll;
using Training;
using Attendance;
using Appraisal;
using P2B.EExMS;
using P2B.API.Models;
using System.Data;
using P2B.UTILS;
using System.Configuration;



namespace EssPortal.Models
{
    public static class UserManager
    {
        public static String CheckPass(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                
                if (data != null)
                {
                    var id = Convert.ToInt32(SessionManager.EmpId);
                    PasswordPolicy passlogin_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ESS").FirstOrDefault();;
                    var Logindetails = db.LoginDetails.ToList().LastOrDefault();


                   
                 //  db.RefreshAllEntites(System.Data.Entity.Core.Objects.RefreshMode.StoreWins);
                    var LoginId = db.Employee.Include(e => e.Login)
                                     .Where(e => e.Id == id).FirstOrDefault().Login_Id;


                    string LoginOld_pwd = db.Login.Find(LoginId).Password;

                    string LoginOldPassword = Convert.ToString(LoginOld_pwd);


                    string password = "";
                    
                    if (passlogin_policy.AllowEncryption == true)
                    {
                        password = P2BSecurity.Decrypt(LoginOldPassword);
                    }
                    else
                    {
                        password = LoginOldPassword;
                    }

                    var a = db.Employee.Include(e => e.Login).Where(e => e.Id == id && e.Login != null && password == data).FirstOrDefault();
                    if (a != null)
                    {
                        return "0";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    return "1";
                }
            }

        }
        public static string RoleCheck()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.EmpId == null)
                {
                    return "0";
                }
                var id = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.Login).Where(e => e.Id == id && e.Login != null && e.Login.UserId == "admin").FirstOrDefault();
                if (qurey != null)
                {
                    return "admin";
                }
                else
                {
                    return "0";
                }
            }
        }
        //public static List<int> GeEmployeeList(int FuncModulesId, int FuncSubModulesId)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (SessionManager.CompanyId == null && SessionManager.EmpId == null)
        //        {
        //            return new List<int>();
        //        }
        //        var compId = Convert.ToInt32(SessionManager.CompanyId);
        //        var EmpId = Convert.ToInt32(SessionManager.EmpId);
        //        var Emp = db.Employee
        //            .Include(e => e.GeoStruct)
        //            .Include(e => e.GeoStruct.Company)
        //            .Include(e => e.GeoStruct.Division)
        //            .Include(e => e.GeoStruct.Location)
        //            .Include(e => e.GeoStruct.Region)
        //            .Include(e => e.GeoStruct.Department)
        //            .Include(e => e.FuncStruct)
        //            .Include(e => e.FuncStruct.Job)
        //            .Include(e => e.FuncStruct.JobPosition)
        //            .Include(e => e.PayStruct)
        //            .Include(e => e.PayStruct.Grade)
        //            .Include(e => e.PayStruct.JobStatus)
        //            .Include(e => e.PayStruct.Level)
        //            .Where(e => e.Id == EmpId).FirstOrDefault();
        //        if (Emp == null)
        //        {
        //            return new List<int>();
        //        }

        //        List<ReportingStructRights> oReportingStructRights = PortalRights.ScanRights(0, Convert.ToInt32(SessionManager.EmpId), Convert.ToInt32(SessionManager.CompanyId));

        //        if (oReportingStructRights == null && oReportingStructRights.Count == 0)
        //        {
        //            return new List<int>();
        //        }

        //        List<EssPortal.Process.PortalRights.AccessRightsData> oAssignRightsEmpList = PortalRights.AssignRightsEmpList(compId, 0, Emp, FuncModulesId, FuncSubModulesId, oReportingStructRights);
        //        if (oAssignRightsEmpList == null)
        //        {
        //            return new List<int>();
        //        }

        //        List<int> EmpIds = new List<int>();
        //        foreach (var item in oAssignRightsEmpList)
        //        {
        //            EmpIds = item.ReportingEmployee;
        //        }
        //        return EmpIds;
        //    }
        //}

        public static List<int> GeEmployeeList(int FuncModulesId, int FuncSubModulesId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.CompanyId == null && SessionManager.EmpId == null)
                {
                    return new List<int>();
                }
                var compId = Convert.ToInt32(SessionManager.CompanyId);
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                List<int> EmpIds = new List<int>();
                try
                {
                    // ====================================== Api call Start ==============================================

                    EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights returnDATA = new EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights();

                    ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>> responseDeserializeData = new ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>();
                    ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>> responseDeserializeData1 = new ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>();
                    string APIUrl = ConfigurationManager.AppSettings["APIURL"];
                     
                    using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                    {
                        var response = p2BHttpClient.request("GLOBAL/Generate_InchargeAccess",
                            new EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights()
                            {
                                Boss_Id = EmpId,

                            });

                        
                        responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>>(response.Content.ReadAsStringAsync().Result);

                        if (responseDeserializeData.Data != null)
                        {
                           
                            List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights> filter = responseDeserializeData.Data.Where(e => e.FuncModules_Id == FuncModulesId).ToList();

                            var response1 = p2BHttpClient.request("GLOBAL/Generate_ReportingEmployeeIds", filter);

                                
                                responseDeserializeData1 = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>>(response1.Content.ReadAsStringAsync().Result);

                                if (responseDeserializeData1.Data != null)
                                {
                                    foreach (var item1 in responseDeserializeData1.Data)
                                    {
                                        EmpIds = item1.EmpIds;
                                    }
                                }

                            }
                        
                    }

                    // ====================================== Api call End ==============================================

                }
                catch (DataException ex)
                {
                }
           
                
                return EmpIds;
            }
        }
        public static List<PortalRights.AccessRightsData> GeEmployeeListWithfunsub(int FuncModulesId, int FuncSubModulesId, string AccessRight)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (SessionManager.CompanyId == null && SessionManager.EmpId == null)
                {
                    return null;
                }
                var compId = Convert.ToInt32(SessionManager.CompanyId);
                var EmpId = Convert.ToInt32(SessionManager.EmpId);
                List<int> EmpIds = new List<int>();
                List<PortalRights.AccessRightsData> OAccessRightsData = new List<PortalRights.AccessRightsData>();
                //var Emp = db.Employee
                //    .Include(e => e.GeoStruct)
                //    .Include(e => e.GeoStruct.Company)
                //    .Include(e => e.GeoStruct.Division)
                //    .Include(e => e.GeoStruct.Location)
                //    .Include(e => e.GeoStruct.Region)
                //    .Include(e => e.GeoStruct.Department)
                //    .Include(e => e.FuncStruct)
                //    .Include(e => e.FuncStruct.Job)
                //    .Include(e => e.FuncStruct.JobPosition)
                //    .Include(e => e.PayStruct)
                //    .Include(e => e.PayStruct.Grade)
                //    .Include(e => e.PayStruct.JobStatus)
                //    .Include(e => e.PayStruct.Level)
                //    .Where(e => e.Id == EmpId).FirstOrDefault();
                //if (Emp == null)
                //{
                //    return null;
                //}

                //List<ReportingStructRights> oReportingStructRights = PortalRights.ScanRights(0, Convert.ToInt32(SessionManager.EmpId), Convert.ToInt32(SessionManager.CompanyId));

                //if (oReportingStructRights == null && oReportingStructRights.Count == 0)
                //{
                //    return null;
                //}

                //List<EssPortal.Process.PortalRights.AccessRightsData> oAssignRightsEmpList = PortalRights.AssignRightsEmpListWithFunSubmodule(compId, 0, Emp, FuncModulesId, FuncSubModulesId, oReportingStructRights, AccessRight);
                //if (oAssignRightsEmpList == null)
                //{
                //    return null;
                //}

                //List<int> EmpIds = new List<int>();
                //foreach (var item in oAssignRightsEmpList)
                //{
                //    EmpIds = item.ReportingEmployee;

                //}

                try
                {
                    // ====================================== Api call Start ==============================================

                    EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights returnDATA = new EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights();

                    ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>> responseDeserializeData = new ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>();
                    ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>> responseDeserializeData1 = new ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>();
                    string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                    using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                    {
                        var response = p2BHttpClient.request("GLOBAL/Generate_InchargeAccess",
                            new EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights()
                            {
                                Boss_Id = EmpId,

                            });


                        responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>>(response.Content.ReadAsStringAsync().Result);

                        if (responseDeserializeData.Data != null)
                        {

                            List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights> filter = responseDeserializeData.Data.Where(e => e.FuncModules_Id == FuncModulesId && e.ActionName == AccessRight ).ToList();


                            var response1 = p2BHttpClient.request("GLOBAL/Generate_ReportingEmployeeIds", filter);


                            responseDeserializeData1 = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>>(response1.Content.ReadAsStringAsync().Result);

                            if (responseDeserializeData1.Data != null)
                            {
                                foreach (var item1 in responseDeserializeData1.Data)
                                {
                                    PortalRights.AccessRightsData OAccRights = new PortalRights.AccessRightsData() 
                                    {
                                        AccessRights = (int)item1.AccessRight_Id,
                                        AccessRightsLookup = item1.ActionName,
                                        LvNoOfDaysFrom = (int)item1.LvNoOfDaysFrom,
                                        LvNoOfDaysTo = (int)item1.LvNoOfDaysTo,
                                        ModuleName = item1.FuncModule,
                                        ReportingEmployee = item1.EmpIds,
                                        SubModuleName = item1.FuncSubModule
                                    };
                                    OAccessRightsData.Add(OAccRights);
                                }
                            }

                        }

                    }

                    // ====================================== Api call End ==============================================

                }
                catch (DataException ex)
                {
                }
                return OAccessRightsData;
            }
        }

        public static List<int> FilterLv(List<LvNewReq> LvIds, String autho, Int32 CompId, PortalRights.AccessRightsData Item1)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 0 && e.TrClosed == false && e.DebitDays > Item1.LvNoOfDaysFrom && e.DebitDays <= Item1.LvNoOfDaysTo)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false && e.DebitDays>Item1.LvNoOfDaysFrom && e.DebitDays<=Item1.LvNoOfDaysTo)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.LastOrDefault().WFStatus == 1 || e.LvWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false && e.DebitDays > Item1.LvNoOfDaysFrom && e.DebitDays <= Item1.LvNoOfDaysTo)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && (e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 1 || e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 7) && e.TrClosed == false && e.DebitDays > Item1.LvNoOfDaysFrom && e.DebitDays <= Item1.LvNoOfDaysTo)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }
        public static List<int> FilterLvCancelReq(List<LvCancelReq> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> OutdoorodFilter(List<OutDoorDutyReq> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> FilterTrainingNeed(List<EmpTrainingNeed> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> FilterLvencash(List<LvEncashReq> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.LvWFDetails.Count > 0 && e.LvWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }
        public static List<int> FilterITInvestmentPayment(List<ITInvestmentPayment> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                       .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                       .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> FilterOfficiating(List<BMSPaymentReq> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.OffWFDetails.Count > 0 && e.OffWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.OffWFDetails.Count > 0 && e.OffWFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.OffWFDetails.Count > 0 && e.OffWFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.OffWFDetails.Count > 0 && e.OffWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> FilterFunctAttendanceT(List<FunctAttendanceT> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.FunctAllWFDetails.Count > 0 && e.FunctAllWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.FunctAllWFDetails.Count > 0 && e.FunctAllWFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.FunctAllWFDetails.Count > 0 && e.FunctAllWFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.FunctAllWFDetails.Count > 0 && e.FunctAllWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> FutureodFilter(List<FutureOD> LvIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 0)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 1)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LvIds
                        .Where(e => e.WFDetails.Count > 0 && e.WFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }

        public static List<int> FilterBusiApp(List<BA_TargetT> TargetIds, String autho, Int32 CompId, PortalRights.AccessRightsData Item1)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = TargetIds
                        .Where(e => e.BA_WorkFlow.Count > 0 && e.BA_WorkFlow.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = TargetIds
                        .Where(e => e.BA_WorkFlow.Count > 0 && e.BA_WorkFlow.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = TargetIds
                        .Where(e => e.BA_WorkFlow.Count > 0 && (e.BA_WorkFlow.LastOrDefault().WFStatus == 1 || e.BA_WorkFlow.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = TargetIds
                        .Where(e => e.BA_WorkFlow.Count > 0 && (e.BA_WorkFlow.LastOrDefault().WFStatus == 1 || e.BA_WorkFlow.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }



        public static List<int> FilterHotelBOOK(List<HotelBookingRequest> HotelBookIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = HotelBookIds
                        .Where(e => e.HotBookReqDetails.Count() > 0 && e.HotBookReqDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = HotelBookIds
                        .Where(e => e.HotBookReqDetails.Count() > 0 && e.HotBookReqDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = HotelBookIds
                        .Where(e => e.HotBookReqDetails.Count > 0 && (e.HotBookReqDetails.LastOrDefault().WFStatus == 1 || e.HotBookReqDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = HotelBookIds
                        .Where(e => e.HotBookReqDetails.Count > 0 && (e.HotBookReqDetails.LastOrDefault().WFStatus == 1 || e.HotBookReqDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }


        public static List<int> FilterTicketBOOK(List<TicketBookingRequest> TicketBookIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = TicketBookIds
                        .Where(e => e.HotBookReqDetails.Count() > 0 && e.HotBookReqDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = TicketBookIds
                        .Where(e => e.HotBookReqDetails.Count() > 0 && e.HotBookReqDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = TicketBookIds
                        .Where(e => e.HotBookReqDetails.Count > 0 && (e.HotBookReqDetails.LastOrDefault().WFStatus == 1 || e.HotBookReqDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = TicketBookIds
                        .Where(e => e.HotBookReqDetails.Count > 0 && (e.HotBookReqDetails.LastOrDefault().WFStatus == 1 || e.HotBookReqDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }


        public static List<int> FilterVehicleBOOK(List<VehicleBookingRequest> VehicleBookIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = VehicleBookIds
                        .Where(e => e.VehicleBookReqDetails.Count() > 0 && e.VehicleBookReqDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = VehicleBookIds
                        .Where(e => e.VehicleBookReqDetails.Count() > 0 && e.VehicleBookReqDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = VehicleBookIds
                        .Where(e => e.VehicleBookReqDetails.Count > 0 && (e.VehicleBookReqDetails.LastOrDefault().WFStatus == 1 || e.VehicleBookReqDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = VehicleBookIds
                        .Where(e => e.VehicleBookReqDetails.Count > 0 && (e.VehicleBookReqDetails.LastOrDefault().WFStatus == 1 || e.VehicleBookReqDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }


        public static List<int> FilterLTCadvanceCLAIM(List<LTCAdvanceClaim> LTCadvclaimIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = LTCadvclaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LTCadvclaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LTCadvclaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LTCadvclaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }


        public static List<int> FilterTADAadvanceCLAIM(List<TADAAdvanceClaim> TADAadvclaimIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = TADAadvclaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = TADAadvclaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = TADAadvclaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = TADAadvclaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }


        public static List<int> FilterTADASettlementClaim(List<TADASettlementClaim> TADASettlementClaimIds, String autho, Int32 CompId)
        {
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = TADASettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = TADASettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = TADASettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = TADASettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();


        }


        public static List<int> FilterLTCSettlementClaim(List<LTCSettlementClaim> LTCSettlementClaimIds, String autho, Int32 CompId)
        {
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = LTCSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = LTCSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = LTCSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = LTCSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();


        }


        public static List<int> FilterDIARYAdvanceClaim(List<DIARYAdvanceClaim> DIARYAdvanceClaimIds, String autho, Int32 CompId)
        {
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = DIARYAdvanceClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = DIARYAdvanceClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = DIARYAdvanceClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = DIARYAdvanceClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();


        }


        public static List<int> FilterDIARYSettlementClaim(List<DIARYSettlementClaim> DIARYSettlementClaimIds, String autho, Int32 CompId)
        {
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = DIARYSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = DIARYSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count() > 0 && e.LTCWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = DIARYSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = DIARYSettlementClaimIds
                        .Where(e => e.LTCWFDetails.Count > 0 && (e.LTCWFDetails.LastOrDefault().WFStatus == 1 || e.LTCWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();


        }

        public static List<int> FilterExpenaseT(List<ExpenseT> ExpenseTIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = ExpenseTIds
                        .Where(e => e.ExMSWFDetails.Count() > 0 && e.ExMSWFDetails.LastOrDefault().WFStatus == 0 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = ExpenseTIds
                        .Where(e => e.ExMSWFDetails.Count() > 0 && e.ExMSWFDetails.LastOrDefault().WFStatus == 1 && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = ExpenseTIds
                        .Where(e => e.ExMSWFDetails.Count > 0 && (e.ExMSWFDetails.LastOrDefault().WFStatus == 1 || e.ExMSWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = ExpenseTIds
                        .Where(e => e.ExMSWFDetails.Count > 0 && (e.ExMSWFDetails.LastOrDefault().WFStatus == 1 || e.ExMSWFDetails.LastOrDefault().WFStatus == 7) && e.TrClosed == false)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }


        public static List<int> FilterAppraisaL(List<EmpAppEvaluation> EmpAppEvalIds, String autho, Int32 CompId)
        {
            /*
             * Sanction-0
             * Hr-1
             */
            List<Int32> lvdata = new List<Int32>();

            switch (autho.ToUpper())
            {
                case "SANCTION":

                    lvdata = EmpAppEvalIds
                        .Where(e => e.EmpAppRatingConclusion.Count() > 0 && e.IsTrClose == true)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "RECOMMENDATION":
                    lvdata = EmpAppEvalIds
                        .Where(e => e.EmpAppRatingConclusion.Count() > 0 && e.IsTrClose == true)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //break;
                case "HR":
                    lvdata = EmpAppEvalIds
                        .Where(e => e.EmpAppRatingConclusion.Count > 0 && e.IsTrClose == true)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                // break;
                case "APPROVAL":
                    lvdata = EmpAppEvalIds
                        .Where(e => e.EmpAppRatingConclusion.Count > 0 && e.IsTrClose == true)
                        .Select(e => e.Id)
                        .ToList();
                    return lvdata;
                //   break;

            }
            return new List<int>();
        }



    }
}