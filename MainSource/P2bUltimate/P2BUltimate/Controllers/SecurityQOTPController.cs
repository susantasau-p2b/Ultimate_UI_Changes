using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2BUltimate.Security;
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
    [AuthoriseManger]
    public class SecurityQOTPController : Controller
    {
        //
        // GET: /WelcomeScreen/
        public ActionResult Index()
        {
            return View("~/Views/Shared/_SecurityQOTP.cshtml");
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
        public ActionResult GetQuestion()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = (Employee)null;
                if (SessionManager.EmpId != null && SessionManager.EmpId != "")
                {
                    Emp = db.Employee.Find(Convert.ToInt32(SessionManager.EmpId));
                }

                //var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).AsEnumerable()
                //   .Select(e => new
                //   {
                //       Id = e.Id,
                //       Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                //   }).SingleOrDefault();
                var qurey1 = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.Id == Emp.Id).SingleOrDefault(); // added by rekha 26-12-16
                var qurey2 = qurey1.Login.LoginDetails.OrderByDescending(e => e.Id).FirstOrDefault();
                if (qurey2 != null)
                {
                    var qurey = qurey2.SecQ.ToString();
                    return Json(qurey, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);


            }
        }

        public ActionResult Create(LoginDetails oLoginDetails, FormCollection form, int seqid)
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



                //PasswordPolicy login_policy = db.PasswordPolicy.FirstOrDefault();
                PasswordPolicy login_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
                var logindetails = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.Id == Emp.Id && e.Login != null).FirstOrDefault();



                if (logindetails.Login.LoginDetails == null)
                {
                    Msg.Add("Security Question not available please change password then try!");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                var SecurityAns = logindetails.Login.LoginDetails.OrderByDescending(e => e.Id).FirstOrDefault();

                if (seqid == 2)//Security Question Appl
                {

                    if (oLoginDetails.SecAns == SecurityAns.SecAns)
                    {
                        return Json(new { success = true, data = Url.Action("index", "dashboard") }, JsonRequestBehavior.AllowGet);
                        //  return Json(new { success = true, data = Url.Action("logout", "login"), responseText = " Password Changed Successfully..! " }, JsonRequestBehavior.AllowGet);  
                    }
                    else
                    {
                        //Msg.Add("Answer is not match please try again..!");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new { success = false, data = Url.Action("logout", "login"), responseText = "Answer is not match please try again..! " }, JsonRequestBehavior.AllowGet);
                    }
                }


                if (seqid == 1)//Security Question and OTP Appl
                {
                }
                if (seqid == 3)//Security OTP Appl
                {
                }

                return null;

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
                //Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy> responseDeserializeData = new Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>();
                //int? errorno = 0;
                //var ShowMessage = "";
                try
                {
                    //if (ModelState.IsValid)
                    //{
                    //string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                    //using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                    //{
                    //    var response = p2BHttpClient.request("Global/getLoginPolicy",
                    //        new CORE_LoginPolicy()
                    //        {
                    //            ApplicationUI = login_policy.ApplicationUI.LookupVal.ToUpper(),
                    //            LoginPolicy_Id = login_policy.Id,
                    //            ProcessType = "PASSWORDCHANGE",
                    //            UserId = logindetails.EmpCode.ToString(),
                    //            Password = newPwd,
                    //            SecurityQ = SecurityQu,
                    //            SecurityAns = SecurityA,
                    //            OldPassword = OldPwd,
                    //            Employee_Id = Emp.Id,
                    //            GUID = logindetails.Login.Guid,

                    //        });

                    //    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<Leave.MainController.LvNewReqController.ServiceResult<P2B.MOBILE.CORE_LoginPolicy>>(response.Content.ReadAsStringAsync().Result);
                    //    if (responseDeserializeData.Data != null)
                    //    {
                    //        errorno = responseDeserializeData.Data.ErrNo;
                    //        ShowMessage = responseDeserializeData.Data.ErrMsg;
                    //    }
                    //    else
                    //    {
                    //        errorno = 1;
                    //        ShowMessage = responseDeserializeData.Message.ToString();
                    //    }

                    //    if (errorno == 0)
                    //    {
                    //        return Json(new { success = true, data = Url.Action("logout", "login"), responseText = " Password Changed Successfully..! " }, JsonRequestBehavior.AllowGet);
                    //    }
                    //    else
                    //    {
                    //        return Json(new { success = false, responseText = ShowMessage }, JsonRequestBehavior.AllowGet);

                    //    }

                    //}



                }
                catch (Exception e)
                {
                    return Json(new { success = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }

    }
}