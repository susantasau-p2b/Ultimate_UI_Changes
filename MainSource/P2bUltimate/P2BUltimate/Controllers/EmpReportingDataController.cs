using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2B.SERVICES;
using P2BUltimate.Controllers.Leave.MainController;
using System.Configuration;
using P2B.UTILS;
using System.Transactions;
using System.Diagnostics;

namespace P2BUltimate.Controllers
{
    public class EmpReportingDataController : Controller
    {
        //
        // GET: /EmpReportingData/
        public ActionResult Index()
        {
            return View();
        }

        public class P2BGridData
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string FuncModules { get; set; }
            public string FuncSubModules { get; set; }
            public string AccessRights { get; set; }
            public string ReportingStruct { get; set; }
            public string BossData { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;                
                    string PayMonth = "";
                    string Month = "";
                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    else
                    {
                        if (DateTime.Now.Date.Month < 10)
                            Month = "0" + DateTime.Now.Date.Month;
                        else
                            Month = DateTime.Now.Date.Month.ToString();
                        PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    }

                    var IE = db.EmpReportingData.Select(d => new
                    {
                        Id = d.Id,
                        Employee = db.Employee.Where(e => e.Id == d.Emp_Id).Select(z => new
                        {
                            EmpCode = z.EmpCode,
                            EmpName = z.EmpName.FullNameFML,
                        }).FirstOrDefault(),
                        BossData = db.Employee.Where(e => e.Id == d.BossData_Id).Select(p => new { 
                           EmpName = p.EmpName.FullNameFM,
                           EmpCode= p.EmpCode
                        }).FirstOrDefault(),
                        ReportingStructRights = db.ReportingStructRights.Where(e => e.Id == d.ReportingStructRights_Id).Select(r => new
                        {
                            FuncModules = db.LookupValue.Where(e => e.Id == d.FuncModules_Id).FirstOrDefault().LookupVal,
                            FuncSubModules = db.LookupValue.Where(e => e.Id == d.FuncSubModules_Id).FirstOrDefault().LookupVal,
                            AccessRights = db.AccessRights.Where(e => e.Id == d.Access_Id).Select(y => new
                            {
                                ActionName = y.ActionName.LookupVal,
                                IsApproveRejectAppl = y.IsApproveRejectAppl == true ? "Yes" : "No",
                                IsClose = y.IsClose == true ? "Yes" : "No",
                                LvNoOfDaysFrom = y.LvNoOfDaysFrom,
                                LvNoOfDaysTo = y.LvNoOfDaysTo,
                            }).FirstOrDefault(),
                            ReportingStruct = db.ReportingStruct.Where(e => e.Id == d.ReportingStruct_Id).Select(g => new
                            {
                                FunctionalAppl = g.FunctionalAppl == true ? "Yes" : "No",
                                GeographicalAppl = g.GeographicalAppl == true ? "Yes" : "No",
                                IndividualAppl = g.IndividualAppl == true ? "Yes" : "No",
                                RoleBasedAppl = g.RoleBasedAppl == true ? "Yes" : "No",
                                RSName = g.RSName
                            }).FirstOrDefault(),
                        }).FirstOrDefault()
                    }).ToList();   
                    
                    List<P2BGridData> IH = new List<P2BGridData>();

                    foreach (var item in IE)
                    {
                        IH.Add(new P2BGridData
                        {
                            Id = item.Id.ToString(),
                            EmpCode = item.Employee == null ? " " : item.Employee.EmpCode == null ? "" : item.Employee.EmpCode,
                            EmpName = item.Employee == null ? " " : item.Employee.EmpName == null ? "" : item.Employee.EmpName,
                            FuncModules = item.ReportingStructRights == null ? " " : item.ReportingStructRights.FuncModules == null ? "" : item.ReportingStructRights.FuncModules,
                            FuncSubModules = item.ReportingStructRights == null ? " " : item.ReportingStructRights.FuncSubModules == null ? "" : item.ReportingStructRights.FuncSubModules,
                            AccessRights = (item.ReportingStructRights == null ? "" : item.ReportingStructRights.AccessRights == null ? "" : item.ReportingStructRights.AccessRights.ActionName == null ? "" : item.ReportingStructRights.AccessRights.ActionName) + "  " + "IsApproveRejectAppl:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.AccessRights == null ? "" : item.ReportingStructRights.AccessRights.IsApproveRejectAppl) + "  " + "IsClose:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.AccessRights == null ? "" : item.ReportingStructRights.AccessRights.IsClose) + "  " + " LvNoOfDaysFrom:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.AccessRights == null ? "" : item.ReportingStructRights.AccessRights.LvNoOfDaysFrom.ToString()) + "  " + " LvNoOfDaysTo:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.AccessRights == null ? "" : item.ReportingStructRights.AccessRights.LvNoOfDaysTo.ToString()),
                            ReportingStruct = (item.ReportingStructRights == null ? "" : item.ReportingStructRights.ReportingStruct == null ? "" : item.ReportingStructRights.ReportingStruct.RSName == null ? "" : item.ReportingStructRights.ReportingStruct.RSName) + "  " + "GeographicalAppl:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.ReportingStruct == null ? "" : item.ReportingStructRights.ReportingStruct.GeographicalAppl) + " " + "FunctionalAppl:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.ReportingStruct == null ? "" : item.ReportingStructRights.ReportingStruct.FunctionalAppl) + " " + "RoleBasedAppl:" + (item.ReportingStructRights == null ? "" : item.ReportingStructRights.ReportingStruct == null ? "" : item.ReportingStructRights.ReportingStruct.RoleBasedAppl),
                            BossData = (item.BossData == null ? "" : item.BossData.EmpCode == null ? "" : "" + "EmpCode:" + item.BossData.EmpCode) + "," + (item.BossData == null ? " " : item.BossData.EmpName == null ? "" : "" + "EmpName:" + item.BossData.EmpName)
                        });
                    }

                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {

                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IH.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.EmpCode.ToString().Contains(gp.searchString))
                                || (e.EmpName.ToString().Contains(gp.searchString))
                                || (e.FuncModules.ToString().Contains(gp.searchString))
                                || (e.FuncSubModules.ToString().Contains(gp.searchString))
                                || (e.AccessRights.ToString().Contains(gp.searchString))
                                || (e.ReportingStruct.ToString().Contains(gp.searchString))
                                || (e.BossData.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { Convert.ToString(a.EmpCode), Convert.ToString(a.EmpName), Convert.ToString(a.FuncModules), Convert.ToString(a.FuncSubModules), Convert.ToString(a.AccessRights), Convert.ToString(a.ReportingStruct), Convert.ToString(a.BossData), a.Id }).Where(a => a.Contains((gp.searchString))).ToList();

                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IH.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), Convert.ToString(a.EmpName), Convert.ToString(a.FuncModules), Convert.ToString(a.FuncSubModules), Convert.ToString(a.AccessRights), Convert.ToString(a.ReportingStruct), Convert.ToString(a.BossData), a.Id }).ToList();
                        }
                        totalRecords = IH.Count();
                    }
                    else
                    {
                       
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                             gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                             gp.sidx == "FuncModules" ? c.FuncModules.ToString() :
                                             gp.sidx == "FuncSubModules" ? c.FuncSubModules.ToString() :
                                             gp.sidx == "AccessRights" ? c.AccessRights.ToString() :
                                             gp.sidx == "ReportingStruct" ? c.ReportingStruct.ToString() :
                                             gp.sidx == "BossData" ? c.BossData.ToString() : "");
                                             
                        }
                        
                       jsonData = IH.Select(a => new Object[] { Convert.ToString(a.EmpCode), Convert.ToString(a.EmpName), Convert.ToString(a.FuncModules), Convert.ToString(a.FuncSubModules), Convert.ToString(a.AccessRights), Convert.ToString(a.ReportingStruct), Convert.ToString(a.BossData), a.Id  }).ToList();
                        
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IH.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), Convert.ToString(a.EmpName), Convert.ToString(a.FuncModules), Convert.ToString(a.FuncSubModules), Convert.ToString(a.AccessRights), Convert.ToString(a.ReportingStruct), Convert.ToString(a.BossData), a.Id }).ToList();
                        }
                        totalRecords = IH.Count();
                    }
                    if (totalRecords > 0)
                    {
                        totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                    }
                    if (gp.page > totalPages)
                    {
                        gp.page = totalPages;
                    }
                    //var JsonData = new
                    //{
                    //    page = gp.page,
                    //    rows = jsonData,
                    //    records = totalRecords,
                    //    total = totalPages
                    //};
                    var JsonData = Json(new
                    {
                        page = gp.page,
                        rows = jsonData,
                        records = totalRecords,
                        total = totalPages

                    }, JsonRequestBehavior.AllowGet);
                    JsonData.MaxJsonLength = int.MaxValue;
                    //return Json(JsonData, JsonRequestBehavior.AllowGet);
                    return JsonData;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public class CORE_EmpData
        {
            public int Id { get; set; }

            public string EmpCode { get; set; }

            public string EmpName { get; set; }

            public string CardCode { get; set; }

            public int FuncStruct_Id { get; set; }
        }


        public ActionResult ProcessBossBtn()
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                    {
                        List<P2b.Global.EmpReportingData> responseDeserializeData = new List<P2b.Global.EmpReportingData>();
                        string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                        var ShowMessageCode = "";
                        var ShowMessage = ""; var ShowErrorMessage = "";

                        using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                        {
                            var response = p2BHttpClient.request("GLOBAL/test1", new CORE_EmpData() { });

                            responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<P2b.Global.EmpReportingData>>(response.Content.ReadAsStringAsync().Result);

                            if (responseDeserializeData.Count() > 0)
                            {
                                foreach (var item in responseDeserializeData)
                                {
                                    var Empcode = db.Employee.Where(e => e.Id == item.Emp_Id).FirstOrDefault().EmpCode;
                                    if (item.ErrNo != 0 && !String.IsNullOrEmpty(item.ErrMsg))
                                    {
                                        ShowErrorMessage += "EmpCode :: " + Empcode + ", ErrNo :: " + item.ErrNo + ", ErrMsg :: " + item.ErrMsg;
                                    }
                                    else
                                    {
                                        ShowMessage = "Process is Over.";
                                    }
                                }
                            }
                            else
                            {
                                return Json("Response is Empty.", JsonRequestBehavior.AllowGet);
                            }


                            if (!String.IsNullOrEmpty(ShowErrorMessage))
                            {

                                return Json(ShowErrorMessage, JsonRequestBehavior.AllowGet);

                            }

                            else
                            {
                                return Json(ShowMessage, JsonRequestBehavior.AllowGet);
                            }


                            //}

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                List<string> Msg = new List<string>();

                var getLineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber());
                var getExceptionMsg = ex.Message.ToString();
                var getInnerExcetionMsg = ex.InnerException != null ? ex.InnerException.Message.ToString() : "";
                Msg.Add("LineNo :: " + getLineNo);
                Msg.Add("ExceptionMsg :: " + getExceptionMsg);
                Msg.Add("InnerExcetionMsg :: " + getInnerExcetionMsg);

                return Json(new { data = Msg, success = false }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}