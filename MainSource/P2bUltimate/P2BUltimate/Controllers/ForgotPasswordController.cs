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
using System.Configuration;
using P2B.UTILS;

namespace P2BUltimate.Controllers
{
    public class ForgotPasswordController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public class CORE_LoginPolicy
        {

            public int? InfoNo { get; set; }
            public string ErrMsg { get; set; }
            public int Employee_Id { get; set; }
            public bool IsLoginSecurityAppl { get; set; }
            public string GUID { get; set; }
            public bool LoginSecurityQuestionAppl { get; set; }
            public int? ErrNo { get; set; }
            public bool DefaultPasswordChangeInit { get; set; }
            public bool IsPasswordSecurityAppl { get; set; }
            public bool PasswordOTPAppl { get; set; }
            public bool PasswordRandomNoGenAppl { get; set; }
            public bool LoginRandomNoGenAppl { get; set; }
            public bool AttendGeoFencingAppl { get; set; }
            public int ResendOTPCount { get; set; }
            public bool LoginOTPAppl { get; set; }
            public int OTPTimer { get; set; }
            public int LogInAttempt { get; set; }
            public bool Lock { get; set; }
            public string ApplicationUI { get; set; }
            public int LoginPolicy_Id { get; set; }
            public string ProcessType { get; set; }
            public string UserId { get; set; }
            public string Password { get; set; }
            public string MobileNumber { get; set; }
            public bool Suspend { get; set; }
            public string OriginalCaptcha { get; set; }
            public string SecurityQ { get; set; }
            public string SecurityAns { get; set; }
            public string OTPRecd { get; set; }
            public string OTPSend { get; set; }
            public string OldPassword { get; set; }
            public string MaxNoBadLogin { get; set; }
            public string InputCaptcha { get; set; }
            public bool forgotPassword { get; set; }
        }
        public ActionResult GetDetails(string data, string newpass, string confirmpass)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (newpass != confirmpass)
                {
                    Msg.Add("Newpassword and confirm password are not same..!");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                var logindetails = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.EmpCode == data && e.Login != null).FirstOrDefault();
                PasswordPolicy login_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                int? errorno = 0;
                var ShowMessage = "";
                if (db.Database.Exists())
                {
                    string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                    using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                    {
                        var response = p2BHttpClient.request("Global/getLoginPolicy",
                            new CORE_LoginPolicy()
                            {
                                ApplicationUI = login_policy.ApplicationUI.LookupVal.ToUpper(),
                                LoginPolicy_Id = login_policy.Id,
                                ProcessType = "FORGOTPASSWORD",
                                UserId = data,
                                Password = newpass,
                                Employee_Id = logindetails.Id,
                                GUID = logindetails.Login.Guid,
                                forgotPassword=true

                            });

                        responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>>(response.Content.ReadAsStringAsync().Result);
                        if (responseDeserializeData.Data != null)
                        {
                            errorno = responseDeserializeData.Data.ErrNo;
                            ShowMessage = responseDeserializeData.Data.ErrMsg;
                        }
                        else
                        {
                            errorno = 1;
                            ShowMessage = responseDeserializeData.Message.ToString();
                        }

                        if (errorno == 0)
                        {
                            var qurey = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.EmpCode == data).SingleOrDefault();
                            if (qurey != null && qurey.Login != null)
                            {
                                var Ques = qurey.Login.LoginDetails.Select(e => e.SecQ).LastOrDefault();
                                var id = qurey.Login.LoginDetails.Select(e => e.Id).LastOrDefault();
                                if (Ques != null)
                                {
                                    Session["CurrentEmp"] = qurey.Id;
                                    return Json(new { success = true, type = "0", data = Ques, id = id }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            //return Json(new { success = true, data = Url.Action("logout", "login"), responseText = " Password Changed Successfully..! " }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, responseText = ShowMessage }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return Json(new { success = false, responseText = ShowMessage }, JsonRequestBehavior.AllowGet);
                    //var qurey = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.EmpCode == data).SingleOrDefault();
                    //if (qurey != null && qurey.Login != null)
                    //{
                    //    var Ques = qurey.Login.LoginDetails.Select(e => e.SecQ).LastOrDefault();
                    //    var id = qurey.Login.LoginDetails.Select(e => e.Id).LastOrDefault();
                    //    if (Ques != null)
                    //    {
                    //        Session["CurrentEmp"] = qurey.Id;
                    //        return Json(new { success = true, type = "0", data = Ques, id = id }, JsonRequestBehavior.AllowGet);
                    //    }
                    //    else
                    //    {
                    //        return Json(new { success = false, type = "1" }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    //else
                    //{
                    //    return Json(new { success = false, type = "2" }, JsonRequestBehavior.AllowGet);
                    //}
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult CheckAns(Int32 qus, string ans, string newpassw)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!string.IsNullOrEmpty(ans) && !string.IsNullOrWhiteSpace(ans))
                {
                    var qurey = db.LoginDetails.Where(e => e.Id == qus).SingleOrDefault();
                    if (qurey.SecAns == ans)
                    {
                        if (!string.IsNullOrEmpty(Session["CurrentEmp"].ToString()))
                        {
                            var id = Convert.ToInt32(Session["CurrentEmp"]);
                            var logindetails = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.Id == id && e.Login != null).FirstOrDefault();
                            PasswordPolicy login_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                            Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                            int? errorno = 0;
                            var ShowMessage = "";
                            string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                            using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                            {
                                var response = p2BHttpClient.request("Global/getLoginPolicy",
                                    new CORE_LoginPolicy()
                                    {
                                        ApplicationUI = login_policy.ApplicationUI.LookupVal.ToUpper(),
                                        LoginPolicy_Id = login_policy.Id,
                                        ProcessType = "PASSWORDSECURITY",
                                        UserId = logindetails.EmpCode,
                                        Password = newpassw,
                                        Employee_Id = logindetails.Id,
                                        GUID = logindetails.Login.Guid,
                                        forgotPassword = true,
                                        SecurityQ = qurey.SecQ,
                                        SecurityAns=qurey.SecAns,
                                         OldPassword =qurey.NewPwd,

                                    });

                                responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>>(response.Content.ReadAsStringAsync().Result);
                                if (responseDeserializeData.Data != null)
                                {
                                    errorno = responseDeserializeData.Data.ErrNo;
                                    ShowMessage = responseDeserializeData.Data.ErrMsg;
                                }
                                else
                                {
                                    errorno = 1;
                                    ShowMessage = responseDeserializeData.Message.ToString();
                                }

                                if (errorno == 0)
                                {
                                    return Json(new { success = true, data = Url.Action("index", "login") }, JsonRequestBehavior.AllowGet);
                                    //return Json(new { success = true, data = Url.Action("logout", "login"), responseText = " Password Changed Successfully..! " }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { success = false, responseText = ShowMessage }, JsonRequestBehavior.AllowGet);

                                }

                            }


                            //var login_data = db.Employee.Include(e => e.Login).Where(e => e.Id == id).SingleOrDefault();
                            //login_data.Login.UserId = login_data.EmpCode;
                            //login_data.Login.Password = login_data.EmpCode;
                            //db.Entry(login_data).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //Session["CurrentEmp"] = string.Empty;
                            //return Json(new { success = true, data = Url.Action("index", "login") }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult ResetPassword(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpCode = data;
                var EmpData = db.Employee.Include(e => e.Login).Where(e => e.EmpCode == EmpCode).SingleOrDefault();
                EmpData.Login.IsActive = false;
                try
                {
                    db.Employee.Attach(EmpData);
                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(EmpData).State = System.Data.Entity.EntityState.Detached;
                    return Json(new { status = true, data = Url.Action("index", "login"), responseText = "Password Reset Request Send To Approval Authority,Wait For Apporaval..!" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}