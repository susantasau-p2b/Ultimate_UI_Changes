using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Models;
using P2B.SERVICES;
using P2B.UTILS;
using System.Configuration;

using P2B.MOBILE;
using P2BUltimate.Controllers.Leave.MainController;


namespace P2BUltimate.Controllers
{
    public class ResetPasswordController : Controller
    {
        //
        // GET: /ResetPassword/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_ResetPassword.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();
        public String PassCheck(String data)
        {
            if (data != null)
            {
                return UserManager.CheckPass(data);
            }
            else
            {
                return "1";
            }
        }
        public ActionResult GetEmp(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).Where(e => e.Login != null).ToList();
                //// var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).ToList();
                //var SelectListItem = new List<SelectListItem>();
                //foreach (var item in EmpData)
                //{
                //    SelectListItem.Add(new SelectListItem
                //    {
                //        Text = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML,
                //        Value = item.EmpCode.ToString()
                //    });
                //}

                //return Json(SelectListItem, JsonRequestBehavior.AllowGet);

                var rall = db.Employee.Include(e => e.EmpName).Include(e => e.Login).Where(e => e.Login != null).ToList();
                var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).ToList();
                var SelectListItem = new List<SelectListItem>();
                foreach (var item in EmpData)
                {
                    SelectListItem.Add(new SelectListItem
                        {
                            Text = item.FullDetails,
                            Value = item.Id.ToString()

                        });
                }
                var r = (from ca in SelectListItem select new { srno = ca.Value, lookupvalue = ca.Text }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        public class P2bGrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Branch { get; set; }
            public string Lock { get; set; }
            public string Suspend { get; set; }

        }

        //[HttpGet]
        public ActionResult getEmpData(string pass)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var empDT = db.Employee.Select(a => new
                    {
                        oEMpId = a.Id,
                        oEmpCode = a.EmpCode,
                        oEmpName = a.EmpName.FullNameFML,
                        oBranch = a.GeoStruct.Location.LocationObj.LocDesc,
                        oServiceLastDt = a.ServiceBookDates.ServiceLastDate,
                        oLogRegister = a.LogRegister.Select(b => new
                        {
                            oLogRegId = b.Id,
                            oLock = b.Lock.ToString(),
                            oSuspend = b.Suspend.ToString(),

                        }).OrderByDescending(o => o.oLogRegId).FirstOrDefault(),

                    }).Where(e => e.oServiceLastDt == null && e.oLogRegister.oLock == "true" || e.oLogRegister.oSuspend == "true").ToList();

                    List<P2bGrid> P2BGridList = new List<P2bGrid>();
                    foreach (var item in empDT)
                    {
                        P2BGridList.Add(new P2bGrid
                        {
                            Id = item.oEMpId.ToString(),
                            EmpCode = item.oEmpCode.ToString(),
                            EmpName = item.oEmpName.ToString(),
                            Branch = item.oBranch.ToString(),
                            Lock = item.oLogRegister != null ? item.oLogRegister.oLock.ToString() : "",
                            Suspend = item.oLogRegister != null ? item.oLogRegister.oSuspend.ToString() : "",
                        });

                    }



                    return Json(P2BGridList, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception)
            {

                throw;
            }

        }

        public ActionResult EmployeeChecked(string EmpCHKED)
        {
            if (!String.IsNullOrEmpty(EmpCHKED))
            {
               TempData["empchk"] = EmpCHKED;
            }
            return Json(TempData["empchk"] = EmpCHKED, JsonRequestBehavior.AllowGet);
        }


        public class returnJsonClass
        {
            public String data { get; set; }
            public bool success { get; set; }
            public String responseText { get; set; }
        }

        public class resetData
        {
            public string ApplicationUI { get; set; }
            public string ProcessType { get; set; }
            public string UserId { get; set; }
           // public List<int> Emp_Ids { get; set; }
        }

        public ActionResult create(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string employee = form["forwarddata"] != null ? form["forwarddata"] : null;
                string resetemppassW = TempData["empchk"].ToString();
                //List<string> empidss = employee.Split(',').ToList();
                List<string> empidss = resetemppassW.Split(',').ToList();


                PasswordPolicy passlogin_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                var ShowMessageCode = "";
                var ShowMessage = "";
                var ShowErrorMessage = "";
                List<int> ids = null;
                if (empidss.Count() > 0)
                {
                    foreach (var iteme in empidss)
                    {
                        if (iteme != null && iteme != "" && iteme != "\"\"" && iteme != "[\"\"" && iteme != "\"\"]")
                        {
                            //ids = Utility.StringIdsToListIds(iteme); 
                            int Employeeid = Convert.ToInt32(iteme);
                            var SingleEmp = db.Employee.Include(e => e.Login).Where(e => e.Id == Employeeid).SingleOrDefault().Login.UserId;

                            if (String.IsNullOrEmpty(SingleEmp))
                            {
                                return Json(new { success = false, responseText = "UserID not found for EmployeeId :: "+iteme });
                            }
                            string ApiBaseUrl = ConfigurationManager.AppSettings["APIURL"];

                            using (P2BHttpClient p2BHttpClient = new P2BHttpClient(ApiBaseUrl))
                            {
                                var response = p2BHttpClient.request("Global/getLoginPolicy",
                                     new resetData()
                                     {
                                         UserId = SingleEmp,
                                         ApplicationUI = passlogin_policy.ApplicationUI.LookupVal.ToString(),
                                         ProcessType = "RESETPASSWORD",
                                     });

                                responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>>(response.Content.ReadAsStringAsync().Result);

                                ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                                
                                if (responseDeserializeData.Data != null)
                                {

                                    ShowMessage = responseDeserializeData.Message.ToString();
                                    
                                }
                                

                            }
                        }
                    }
                    
                }
                else
                {
                    return Json(new { success = false, responseText = "Select Employee" });
                }

                
               

                //foreach (var item in ids)
                //{

                //    PasswordPolicy passlogin_policy = db.PasswordPolicy.FirstOrDefault();

                //    var EmpCheck = db.Employee.Include(e => e.Login).Where(e => e.Id == item).SingleOrDefault();

                //    if (responseDeserializeData == null && ShowMessageCode == "OK")
                        


                //    }
                //    else
                //    {
                //        return Json(new { success = false, responseText = "Employee is not logged in" });

                //    }
                //}
                //}
                //return Json(new { success = true, responseText = "Password Reset Successfully!" });
                return Json(new { success = true, responseText = ShowMessage });

            }
        }
    }
}