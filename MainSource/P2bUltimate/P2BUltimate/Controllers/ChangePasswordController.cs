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
using P2B.SERVICES;
using P2B.UTILS;

namespace P2BUltimate.Controllers
{
    public class ChangePasswordController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/_ChangePassword.cshtml");
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
        public ActionResult Create(LoginDetails oLoginDetails, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = (Employee)null;
                var DecryOld = ""; var DecryDBold = "";
                if (SessionManager.EmpId != null && SessionManager.EmpId != "")
                {
                    Emp = db.Employee.Find(Convert.ToInt32(SessionManager.EmpId));
                }
                //initialchange page start
                string Oldpassword = form["OldPassword"] == "0" ? "" : form["OldPassword"];
                string Newpassword = form["NewPassword"] == "0" ? "" : form["NewPassword"];
                string Emailid = form["EmailId"] == "" ? "" : form["EmailId"];
               // string Seqq = form["SecurityQuestion"] == "" ? "" : form["SecurityQuestion"];
                string Seqq ="";
                string initialpasschsecq = form["iSecQQ"] == "0" ? "" : form["iSecQQ"];
                if (initialpasschsecq != null && initialpasschsecq != "")
                {
                    var val = db.LookupValue.Find(int.Parse(initialpasschsecq));
                    Seqq = val.LookupVal;
                }


                string SeqA = form["SecurityAnswer"] == "" ? "" : form["SecurityAnswer"];
                //initialchange page End

                //Password change page start
                string passchsecq = form["SecQQ"] == "0" ? "" : form["SecQQ"];
                if (passchsecq != null && passchsecq != "")
                {
                    var val = db.LookupValue.Find(int.Parse(passchsecq));
                    oLoginDetails.SecQ = val.LookupVal;
                }

                //Password change page End

                //PasswordPolicy login_policy = db.PasswordPolicy.FirstOrDefault();
                PasswordPolicy login_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                var logindetails = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.Id == Emp.Id && e.Login != null).FirstOrDefault();
                string newPwd = "";
                string OldPwd = "";
                string Emailidp = "";
                string SecurityQu = "";
                string SecurityA = "";
                if (login_policy != null)
                {
                    string encryptOldpwd = "";
                    // string encryptLoginpwd = "";
                    if (oLoginDetails.OldPwd != null && logindetails != null && logindetails.Login.Password != null && login_policy.AllowEncryption == true)
                    {
                        encryptOldpwd = P2BSecurity.Encrypt(oLoginDetails.OldPwd);
                       
                      
                    }
                    else if (oLoginDetails.OldPwd == null && logindetails != null && logindetails.Login.Password != null && login_policy.AllowEncryption == true)
                    {
                        encryptOldpwd = P2BSecurity.Encrypt(logindetails.Login.Password);
                    }
                    else if (oLoginDetails.OldPwd == null && logindetails != null && logindetails.Login.Password != null && login_policy.AllowEncryption == false)
                    {
                        encryptOldpwd = logindetails.Login.Password;
                    }
                    else
                    {
                        encryptOldpwd = oLoginDetails.OldPwd;
                    }


                    if (encryptOldpwd != null && login_policy.AllowEncryption == true)
                    {
                        DecryOld = P2BSecurity.Decrypt(encryptOldpwd);
                        DecryDBold = P2BSecurity.Decrypt(logindetails.Login.Password);
                    }
                    else
                    {
                        DecryOld = encryptOldpwd;
                        DecryDBold = logindetails.Login.Password;
                    }

                    //if (logindetails != null && oLoginDetails.OldPwd != null && P2BSecurity.Decrypt(oLoginDetails.OldPwd) != P2BSecurity.Decrypt(logindetails.Login.Password))
                    //{
                    //    return Json(new { success = false, data = "", responseText = "Wrong Password..!" }, JsonRequestBehavior.AllowGet);
                    //}


                }
                if (DecryOld != DecryDBold)
                {
                    Msg.Add("Old password is wrong..!");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    
                   // return Json(new { success = false, data = "", responseText = "Old password is wrong..!" }, JsonRequestBehavior.AllowGet);
                    
                }
                //newPwd = login_policy.AllowEncryption == true ? P2BSecurity.Encrypt(oLoginDetails.NewPwd) : oLoginDetails.NewPwd;
                //OldPwd = login_policy.AllowEncryption == true ? P2BSecurity.Encrypt(oLoginDetails.OldPwd) : oLoginDetails.OldPwd;
                if (oLoginDetails.OldPwd == null)
                {
                    newPwd = Newpassword;
                    OldPwd = Oldpassword;
                    SecurityQu = Seqq;
                    SecurityA = SeqA;
                    oLoginDetails.NewPwd = newPwd;
                    oLoginDetails.OldPwd = OldPwd;
                }
                else
                {
                    newPwd = oLoginDetails.NewPwd;
                    OldPwd = oLoginDetails.OldPwd;
                    SecurityQu = oLoginDetails.SecQ;
                    SecurityA = oLoginDetails.SecAns;
                  
                }

                if (newPwd == "" || newPwd==null)
                {
                    Msg.Add("New password is Require..!");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (OldPwd == "" || OldPwd == null)
                {
                    Msg.Add("Old password is Require..!");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }



                //else
                //{
                //    if (logindetails != null && oLoginDetails.OldPwd != null && oLoginDetails.OldPwd != logindetails.Login.Password)
                //    {
                //        return Json(new { success = false, data = "", responseText = "Wrong Password..!" }, JsonRequestBehavior.AllowGet);
                //    }
                //    newPwd = oLoginDetails.NewPwd;
                //    OldPwd = oLoginDetails.OldPwd;
                //}

                //LoginDetails _LoginDetails = new LoginDetails()
                //{
                //    EmailId = oLoginDetails.EmailId,
                //    NewPwd = newPwd,
                //    OldPwd = OldPwd,
                //    SecQ = oLoginDetails.SecQ,
                //    SecAns = oLoginDetails.SecAns,
                //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                //};
                Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                int? errorno = 0;
                var ShowMessage = "";
                try
                {
                    //if (ModelState.IsValid)
                    //{
                        string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                        using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                        {
                            var response = p2BHttpClient.request("Global/getLoginPolicy",
                                new CORE_LoginPolicy()
                                {
                                    ApplicationUI = login_policy.ApplicationUI.LookupVal.ToUpper(),
                                    LoginPolicy_Id = login_policy.Id,
                                    ProcessType = "PASSWORDCHANGE",
                                    UserId = logindetails.EmpCode.ToString(),
                                    Password = newPwd,
                                    SecurityQ = SecurityQu,
                                    SecurityAns = SecurityA,
                                    OldPassword = OldPwd,
                                    Employee_Id = Emp.Id,
                                    GUID = logindetails.Login.Guid,
                                   
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
                                    return Json(new { success = true, data = Url.Action("logout", "login"), responseText = " Password Changed Successfully..! " }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    return Json(new { success = false,  responseText=ShowMessage }, JsonRequestBehavior.AllowGet);

                                }

                        }
                      

                        //using (TransactionScope ts = new TransactionScope())
                        //{
                        //    db.LoginDetails.Add(_LoginDetails);
                        //    db.SaveChanges();
                        //    logindetails.Login.LoginDetails.Add(_LoginDetails);
                        //    logindetails.Login.Password = _LoginDetails.NewPwd;
                        //    db.Entry(logindetails).State = EntityState.Modified;
                        //    db.SaveChanges();
                        //    ts.Complete();
                        //}
                        //return Json(new { success = true, data = Url.Action("logout", "login"), responseText = " Password Changed Successfully..! " }, JsonRequestBehavior.AllowGet);
                   // }
                    //else
                    //{
                    //    StringBuilder sb = new StringBuilder("");
                    //    foreach (ModelState modelState in ModelState.Values)
                    //    {
                    //        foreach (ModelError error in modelState.Errors)
                    //        {
                    //            sb.Append(error.ErrorMessage);
                    //            sb.Append("." + "\n");
                    //        }
                    //    }
                    //    var errorMsg = sb.ToString();
                    //    return Json(new { success = false, data = "", responseText = errorMsg.ToString(), JsonRequestBehavior.AllowGet });
                    //}
                }
                catch (Exception e)
                {
                    return Json(new { success = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }
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
    }
}