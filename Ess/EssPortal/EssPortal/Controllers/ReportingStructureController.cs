using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using EssPortal.Process;
using EssPortal.Security;
using System.Web.Mvc;
using P2B.UTILS;
using System.Configuration;
using P2B.API.Models;
using System.Net;
using System.Data;

namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class ReportingStructureController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public class WhoisClass
        {
            public string Whois { get; set; }
            public string FuncModules { get; set; }
            public bool isclose { get; set; }
        }

         public class CORE_BossAccessRights
        {
                public int ErrNo { get; set; }
                public List<int> EmpIds { get; set; }
                public int? LvNoOfDaysTo { get; set; }
                public int? LvNoOfDaysFrom { get; set; }
                public bool? IsApproveRejectAppl { get; set; }
                public bool? IsComments { get; set; }
                public bool? IsClose { get; set; }
                public string ActionName { get; set; }
                public string FuncSubModule { get; set; }
                public string FuncModule { get; set; }
                public int? AccessRight_Id { get; set; }
                public int? FuncSubModules_Id { get; set; }
                public int? FuncModules_Id { get; set; }
                public string Boss_Code { get; set; }
                public int? Boss_Id { get; set; }
                public int InfoNo { get; set; }
                public string ErrMsg { get; set; }
         }

        public class ServiceResultList<T>
        {
            /// <summary>
            /// 
            /// </summary>
            public HttpStatusCode MessageCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<T> Data { get; set; }
        }

        public class ServiceResult<T>
        {
            /// <summary>
            /// 
            /// </summary>
            public HttpStatusCode MessageCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public T Data { get; set; }
        }

        public class ELMS_Lv_DebitDays
        {
            public int Emp_Id { get; set; }

            public string Emp_Code { get; set; }

            public int? Lv_Head_Id { get; set; }

            public string Lv_Head { get; set; }

            public DateTime? ReqDate { get; set; }

            public DateTime? FromDate { get; set; }

            public DateTime? ToDate { get; set; }

            public int? FromStat_Id { get; set; }

            public string FromStat { get; set; }

            public int? ToStat_Id { get; set; }

            public string ToStat { get; set; }
        }

        public JsonResult GetWhois()
        {
            //List<ReportingStructRights> oReportingStructRights = null;
            //if (!string.IsNullOrEmpty(SessionManager.EmpId) && !string.IsNullOrEmpty(SessionManager.CompanyId))
            //{
            //    oReportingStructRights = PortalRights.ScanRights(0, Convert.ToInt32(SessionManager.EmpId), Convert.ToInt32(SessionManager.CompanyId));
            //}
            //if (oReportingStructRights == null)
            //{
            //    return Json(null, JsonRequestBehavior.AllowGet);
            //}
            //List<WhoisClass> WhoisList = new List<WhoisClass>();

            //foreach (var item in oReportingStructRights)
            //{
            //    WhoisList.Add(new WhoisClass
            //    {
            //        Whois = item.AccessRights.ActionName.LookupVal,
            //        isclose = item.AccessRights.IsClose,
            //        //  FuncModules = item.FuncModules.LookupVal.ToString(),
            //        //item.
            //    });
            //}
            //return Json(WhoisList, JsonRequestBehavior.AllowGet);
            List<WhoisClass> WhoisList = new List<WhoisClass>();
            try
            {

                LeaveHeadProcess.ReturnData retD = new LeaveHeadProcess.ReturnData();


                // ====================================== Api call Start ==============================================


                int errorno = 0;
                double debdays = 0.0;

                CORE_BossAccessRights returnDATA = new CORE_BossAccessRights();

                var ShowMessageCode = "";
                var ShowMessage = "";
               
                ServiceResult<List<CORE_BossAccessRights>> responseDeserializeData = new ServiceResult<List<CORE_BossAccessRights>>();
                string APIUrl = ConfigurationManager.AppSettings["APIURL"];
                using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                {
                    var response = p2BHttpClient.request("GLOBAL/Generate_InchargeAccess",
                        new CORE_BossAccessRights()
                        {
                            Boss_Id = int.Parse(SessionManager.EmpId),

                        });

                    var data = response.Content.ReadAsStringAsync().Result;
                    //if (data!=null)
                    //{
                    //    var xx = 0; 
                    //}
                    // var result = data.;

                    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<CORE_BossAccessRights>>>(response.Content.ReadAsStringAsync().Result);


                    //ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                    //ShowMessage = responseDeserializeData.Message.ToString();


                    //using (DataBaseContext db = new DataBaseContext())
                    //{
                  
                    if (responseDeserializeData.Data != null)
                    {
                      

                        List<CORE_BossAccessRights> empAccessRightList = new List<CORE_BossAccessRights>();
                        empAccessRightList.AddRange(responseDeserializeData.Data);
                        foreach (var item in responseDeserializeData.Data)
                        {
                            if (item.ActionName == "SANCTION")
                            {

                            }
                        }
               

                        using (DataBaseContext db= new DataBaseContext())
                        {
                            //var oErrorlookup = db.ErrorLookup.Where(e => e.Message_Code == errno).FirstOrDefault();
                            //ShowMessage = errno + ' ' + oErrorlookup.Message_Description.ToString();  
                        }
                    }
                    else
                    {
                        errorno = 1;
                        ShowMessage = responseDeserializeData.Message.ToString();
                    }


                    //}

                }
               
                // ====================================== Api call End ==============================================
                //debdays = 1;
                if (errorno > 0)
                {
                    return Json(new { errorno, ShowMessage });
                }
                else
                {
                    return Json(new { errorno, debdays });

                }
            }
            catch (DataException ex)
            {
            }
            return Json(WhoisList, JsonRequestBehavior.AllowGet);
        }

        public class SelectListItemCustome
        {
            public string Text { get; set; }
            public string Value { get; set; }
            public string Data { get; set; }
        }

        public ActionResult GetReportingDetails()
        {
            var EmpId = Convert.ToInt32(SessionManager.EmpId);
            ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>> responseDeserializeData = new ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>();
             string APIUrl = ConfigurationManager.AppSettings["APIURL"];

             using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
             {
                 var response = p2BHttpClient.request("GLOBAL/Generate_InchargeAccess",
                     new EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights()
                     {
                         Boss_Id = EmpId,

                     });

                 responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<EssPortal.Controllers.ReportingStructureController.CORE_BossAccessRights>>>(response.Content.ReadAsStringAsync().Result);

                 Session["ReportingData"] = responseDeserializeData.Data;
             }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWhoisDropDown()
        {
            var EmpId = 0;
            var CompId = 0;
            if (!string.IsNullOrEmpty(SessionManager.EmpId))
            {
                EmpId = Convert.ToInt32(SessionManager.EmpId);
                CompId = Convert.ToInt32(SessionManager.CompanyId);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            
            List<SelectListItemCustome> selectlist = new List<SelectListItemCustome>();
            selectlist.Add(new SelectListItemCustome
            {
                Text = "MySelf",
                Value = "-1"
            });

            //if (oReportingStructRights != null && oReportingStructRights.Count > 0)
            //{
            //    foreach (var item in oReportingStructRights)
            //    {
            //        selectlist.Add(new SelectListItemCustome
            //        {
            //            Text = item.AccessRights.ActionName.LookupVal,
            //            Value = item.AccessRights.Id.ToString(),
            //        });
            //    }
            //}

            try
            {
                // ====================================== Api call Start ==============================================

                CORE_BossAccessRights returnDATA = new CORE_BossAccessRights();

                ServiceResult<List<CORE_BossAccessRights>> responseDeserializeData = new ServiceResult<List<CORE_BossAccessRights>>();
                string APIUrl = ConfigurationManager.AppSettings["APIURL"];
                using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                {
                    var response = p2BHttpClient.request("GLOBAL/Generate_InchargeAccess",
                        new CORE_BossAccessRights()
                        {
                            Boss_Id = EmpId,

                        });

                    var data = response.Content.ReadAsStringAsync().Result;
                    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<List<CORE_BossAccessRights>>>(response.Content.ReadAsStringAsync().Result);

                    if (responseDeserializeData.Data != null)
                    {
                        List<CORE_BossAccessRights> empAccessRightList = new List<CORE_BossAccessRights>();
                        empAccessRightList.AddRange(responseDeserializeData.Data);
                        foreach (var item in responseDeserializeData.Data)
                        {
                            selectlist.Add(new SelectListItemCustome
                       {
                           Text = item.ActionName,
                           Value = item.AccessRight_Id.ToString(),
                       });
                        }
                    }
                }

                // ====================================== Api call End ==============================================
              
            }
            catch (DataException ex)
            {
            }
            
            selectlist = selectlist.GroupBy(e => e.Text).Select(e => e.First()).ToList();
            return Json(selectlist, JsonRequestBehavior.AllowGet);
        }
        public class EmpCodeEmpNameClass
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string funcsubmodule { get; set; }
        }
        public class returnreportingEmpClass
        {
            public EmpCodeEmpNameClass Viewer { get; set; }
            public EmpCodeEmpNameClass Recommendation { get; set; }
            public EmpCodeEmpNameClass Sanction { get; set; }
            public EmpCodeEmpNameClass Approval { get; set; }
        }
        public class returnParentEmpClass
        {
            public List<returnreportingEmpClass> EPMS { get; set; }
            public List<returnreportingEmpClass> ELMS { get; set; }
            public List<returnreportingEmpClass> EEIS { get; set; }
            public List<returnreportingEmpClass> ETRM { get; set; }         
            public List<returnreportingEmpClass> EAMS { get; set; }
            public List<returnreportingEmpClass> EBMS { get; set; }
            public List<returnreportingEmpClass> EEMS { get; set; }
            public List<returnreportingEmpClass> ETMS { get; set; }
            public List<returnreportingEmpClass> EDMS { get; set; }
            public List<returnreportingEmpClass> BULLETIN { get; set; }
            public List<returnreportingEmpClass> REIMBURSEMENT { get; set; }
        }
        public EmpCodeEmpNameClass fun_EmpCodeEmpNameClass(Employee OEmp, ReportingStruct oReportingStructRights)
        {
            var CompId = Convert.ToInt32(SessionManager.CompanyId);
            var EmpCode = ""; var EmpName = "";
            var GeoGraphicCheck = oReportingStructRights.GeographicalAppl == true ? oReportingStructRights.GeoGraphList.LookupVal : null;
            if (GeoGraphicCheck != null)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (GeoGraphicCheck.ToLower() == "location")
                    {
                        var loc = db.Company.Include(e => e.Location)
                            .Include(e => e.Location.Select(a => a.Incharge))
                            .Include(e => e.Location.Select(a => a.Incharge.EmpName))
                            .Where(e => e.Id == CompId && e.Location.Any(a => a.Id == OEmp.GeoStruct.Location.Id)).SingleOrDefault();
                        var Incharge = loc.Location.Where(e => e.Id == OEmp.GeoStruct.Location.Id).SingleOrDefault();
                        EmpCode = Incharge.Incharge == null ? "" : Incharge.Incharge.EmpCode;
                        EmpName = Incharge.Incharge == null ? "" : Incharge.Incharge.EmpName.FullNameFML;
                    }
                    if (GeoGraphicCheck.ToLower() == "division")
                    {
                        var loc = db.Company.Include(e => e.Divisions)
                            .Include(e => e.Divisions.Select(a => a.Incharge))
                            .Include(e => e.Divisions.Select(a => a.Incharge.EmpName))
                            .Where(e => e.Id == CompId && e.Divisions.Any(a => a.Id == OEmp.GeoStruct.Division.Id)).SingleOrDefault();
                        var Incharge = loc.Divisions.Where(e => e.Id == OEmp.GeoStruct.Division.Id).SingleOrDefault();
                        EmpCode = Incharge.Incharge == null ? "" : Incharge.Incharge.EmpCode;
                        EmpName = Incharge.Incharge == null ? "" : Incharge.Incharge.EmpName.FullNameFML;
                    }

                }
            }
            var FunctionalApplCheck = oReportingStructRights.FunctionalAppl == true ? oReportingStructRights.GeoFuncList.LookupVal : null;
            if (FunctionalApplCheck != null)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (FunctionalApplCheck.ToLower() == "department")
                    {
                        var oDepartment = db.Company.Include(e => e.Department)
                             .Include(e => e.Department.Select(a => a.Incharge))
                             .Include(e => e.Department.Select(a => a.Incharge.EmpName))
                             .Where(e => e.Id == CompId && e.Department.Any(a => a.Id == OEmp.GeoStruct.Department.Id)).SingleOrDefault();
                        var Incharge = oDepartment.Department.Where(e => e.Id == OEmp.GeoStruct.Department.Id).SingleOrDefault();
                        EmpCode = Incharge.Incharge == null ? "" : Incharge.Incharge.EmpCode;
                        EmpName = Incharge.Incharge == null ? "" : Incharge.Incharge.EmpName.FullNameFML;
                    }
                }
            }
            var RoleBasedApplCheck = oReportingStructRights.RoleBasedAppl == true ? oReportingStructRights.FuncStruct : null;
            if (RoleBasedApplCheck != null)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var rol = db.Employee.Include(e => e.GeoStruct).Include(e => e.EmpName).Include(e => e.GeoStruct.Company)
                        .Where(e => e.ServiceBookDates != null && e.ServiceBookDates.ServiceLastDate == null && e.GeoStruct.Company.Id == CompId
                            && e.FuncStruct.Id == RoleBasedApplCheck.Id).FirstOrDefault();
                    var Incharge = rol;
                    EmpCode = Incharge != null ? Incharge.EmpCode : null;
                    EmpName = Incharge != null && Incharge.EmpName != null ? Incharge.EmpName.FullNameFML : null;
                }
            }
            var IndividualApplCheck = oReportingStructRights.IndividualAppl == true ? oReportingStructRights.BossEmp : null;
            if (IndividualApplCheck != null)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var rol = db.Employee.Include(e => e.GeoStruct).Include(e => e.EmpName).Include(e => e.GeoStruct.Company)
                        .Where(e => e.ServiceBookDates != null && e.ServiceBookDates.ServiceLastDate == null && e.GeoStruct.Company.Id == CompId
                            && e.Id == IndividualApplCheck.Id).FirstOrDefault();
                    var Incharge = rol;
                    EmpCode = Incharge != null ? Incharge.EmpCode : null;
                    EmpName = Incharge != null && Incharge.EmpName != null ? Incharge.EmpName.FullNameFML : null;
                }
            }

            if (EmpCode != "" && EmpName != "")
            {
                var oEmpCodeEmpNameClass = new EmpCodeEmpNameClass();
                oEmpCodeEmpNameClass.EmpCode = EmpCode;
                oEmpCodeEmpNameClass.EmpName = EmpName;
                return oEmpCodeEmpNameClass;
            }
            else
            {
                return null;
            }
        }
        public List<returnreportingEmpClass> fun_returnreportingEmpClass(Employee Get_EmpData, String ModelName)
        {
            var Obj_returnreportingEmpClass = new returnreportingEmpClass();
            List<returnreportingEmpClass> Obj_returnreportingEmpListData = new List<returnreportingEmpClass>();
            var Modelchk = Get_EmpData.ReportingStructRights.Where(e => e.FuncModules != null && e.FuncModules.LookupVal.ToUpper() == ModelName && e.AccessRights != null && e.AccessRights.ActionName != null).ToList();
            if (Modelchk.Count() == 0)
            {
                return null;
            }
            var VIEWER_check = Get_EmpData.ReportingStructRights.Where(e => e.FuncModules.LookupVal.ToUpper() == ModelName && e.AccessRights.ActionName.LookupVal.ToUpper() == "VIEWER").ToList();
            if (VIEWER_check != null)
            {
                foreach (var item in VIEWER_check)
                {
                    //  var Obj_returnreportingEmpClass1 = new returnreportingEmpClass();
                    var SanctionData = fun_EmpCodeEmpNameClass(Get_EmpData, item.ReportingStruct);
                    if (SanctionData != null)
                    {
                        returnreportingEmpClass Obj_returnreportingEmpClass1 = new returnreportingEmpClass
                        {
                            Sanction = new EmpCodeEmpNameClass
                            {
                                EmpCode = SanctionData.EmpCode,
                                EmpName = SanctionData.EmpName,
                                funcsubmodule = item.FuncSubModules == null ? "" : item.FuncSubModules.LookupVal,
                            }
                        };
                        Obj_returnreportingEmpListData.Add(Obj_returnreportingEmpClass1);
                    }
                }

            }
            var RECOMMANDATION_check = Get_EmpData.ReportingStructRights.Where(e => e.FuncModules.LookupVal.ToUpper() == ModelName && e.AccessRights.ActionName.LookupVal.ToUpper() == "RECOMMANDATION").ToList();
            if (RECOMMANDATION_check != null)
            {
                foreach (var item in RECOMMANDATION_check)
                {
                    //  var Obj_returnreportingEmpClass1 = new returnreportingEmpClass();
                    var SanctionData = fun_EmpCodeEmpNameClass(Get_EmpData, item.ReportingStruct);
                    if (SanctionData != null)
                    {
                        returnreportingEmpClass Obj_returnreportingEmpClass1 = new returnreportingEmpClass
                        {
                            Sanction = new EmpCodeEmpNameClass
                            {
                                EmpCode = SanctionData.EmpCode,
                                EmpName = SanctionData.EmpName,
                                funcsubmodule = item.FuncSubModules == null ? "" : item.FuncSubModules.LookupVal,
                            }
                        };
                        Obj_returnreportingEmpListData.Add(Obj_returnreportingEmpClass1);
                    }
                }
            }
            var Sanction_check = Get_EmpData.ReportingStructRights.Where(e => e.FuncModules.LookupVal.ToUpper() == ModelName && e.AccessRights.ActionName.LookupVal.ToUpper() == "SANCTION").ToList();
            if (Sanction_check != null)
            {
                foreach (var item in Sanction_check)
                {
                    //  var Obj_returnreportingEmpClass1 = new returnreportingEmpClass();
                    var SanctionData = fun_EmpCodeEmpNameClass(Get_EmpData, item.ReportingStruct);
                    if (SanctionData != null)
                    {
                        returnreportingEmpClass Obj_returnreportingEmpClass1 = new returnreportingEmpClass
                        {
                            Sanction = new EmpCodeEmpNameClass
                            {
                                EmpCode = SanctionData.EmpCode,
                                EmpName = SanctionData.EmpName,
                                funcsubmodule = item.FuncSubModules == null ? "" : item.FuncSubModules.LookupVal,
                            }
                        };
                        Obj_returnreportingEmpListData.Add(Obj_returnreportingEmpClass1);
                    }
                }
            }
            var APPROVAL_check = Get_EmpData.ReportingStructRights.Where(e => e.FuncModules.LookupVal.ToUpper() == ModelName && e.AccessRights.ActionName.LookupVal.ToUpper() == "APPROVAL").ToList();
            if (APPROVAL_check != null)
            {
                foreach (var item in APPROVAL_check)
                {
                    //  var Obj_returnreportingEmpClass1 = new returnreportingEmpClass();
                    var SanctionData = fun_EmpCodeEmpNameClass(Get_EmpData, item.ReportingStruct);
                    if (SanctionData != null)
                    {
                        returnreportingEmpClass Obj_returnreportingEmpClass1 = new returnreportingEmpClass
                        {
                            Approval = new EmpCodeEmpNameClass
                            {
                                EmpCode = SanctionData.EmpCode,
                                EmpName = SanctionData.EmpName,
                                funcsubmodule = item.FuncSubModules == null ? "" : item.FuncSubModules.LookupVal,
                            }
                        };
                        Obj_returnreportingEmpListData.Add(Obj_returnreportingEmpClass1);
                    }
                }
            }
            return Obj_returnreportingEmpListData;
        }
        public ActionResult GetReportingView()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = 0;
                var CompId = 0;
                if (!string.IsNullOrEmpty(SessionManager.EmpId))
                {
                    EmpId = Convert.ToInt32(SessionManager.EmpId);
                    CompId = Convert.ToInt32(SessionManager.CompanyId);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Get_EmpData = db.Employee
                    .Include(e => e.ReportingStructRights)
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Division)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Department)
                    .Include(e => e.FuncStruct)
                    //.Include(e => e.PayStruct)
                    .Include(e => e.EmpName)
                    .Include(e => e.ReportingStructRights.Select(a => a.AccessRights))
                    .Include(e => e.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                    .Include(e => e.ReportingStructRights.Select(a => a.FuncModules))
                    .Include(e => e.ReportingStructRights.Select(a => a.FuncSubModules))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.GeoGraphList))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.GeoFuncList))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.FuncStruct))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.BossEmp))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct.BossEmp.EmpName))
                    .Where(e => e.Id == EmpId).SingleOrDefault();
                if (Get_EmpData == null || Get_EmpData.ReportingStructRights.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            
                var Parent_Obj = new returnParentEmpClass();
                var EPMS = fun_returnreportingEmpClass(Get_EmpData, "EPMS");
                var ELMS = fun_returnreportingEmpClass(Get_EmpData, "ELMS");
                var ETRM = fun_returnreportingEmpClass(Get_EmpData, "ETRM");
                var EEIS = fun_returnreportingEmpClass(Get_EmpData, "EEIS");             
                var EAMS = fun_returnreportingEmpClass(Get_EmpData, "EAMS");               
                var EBMS = fun_returnreportingEmpClass(Get_EmpData, "EBMS");
                var EEMS = fun_returnreportingEmpClass(Get_EmpData, "EEMS");
                var ETMS = fun_returnreportingEmpClass(Get_EmpData, "ETMS");
                var EDMS = fun_returnreportingEmpClass(Get_EmpData, "EDMS");              
                var BULLETIN = fun_returnreportingEmpClass(Get_EmpData, "BULLETIN");
                var REIMBURSEMENT = fun_returnreportingEmpClass(Get_EmpData, "REIMBURSEMENT");

                Parent_Obj = new returnParentEmpClass
                {
                    EPMS = EPMS,
                    ETRM = ETRM,
                    ELMS = ELMS,
                    EEIS = EEIS,                  
                    EAMS = EAMS,                 
                    EBMS = EBMS,
                    EEMS = EEMS,
                    ETMS = ETMS,
                    EDMS=  EDMS,
                    BULLETIN = BULLETIN,
                    REIMBURSEMENT = REIMBURSEMENT

                };
                return Json(new { status = true, data = Parent_Obj }, JsonRequestBehavior.AllowGet);


            }
        }
    }
}