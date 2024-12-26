using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class PasswordPolicyController : Controller
    {
        //
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(PasswordPolicy p, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var firstlog = TempData["Firsttimeloginkey"];
                    string ApplicationUIList = form["ApplicationUIList"] == "0" ? "" : form["ApplicationUIList"];
                    if (ApplicationUIList != null && ApplicationUIList != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ApplicationUIList));
                        p.ApplicationUI = val;
                    }
                    else
                    {
                        Msg.Add("Please Select ApplicationUI .");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var id = Convert.ToInt32(SessionManager.CompanyId);
                    var company_value = db.Company.Include(a => a.PasswordPolicy).Include(a => a.PasswordPolicy.Select(e=>e.ApplicationUI)).Where(e => e.Id == id).SingleOrDefault();
                    var passwordpolicydatacheckk = company_value != null ? company_value.PasswordPolicy.ToList() : null;
                    if (passwordpolicydatacheckk == null)
                    {
                        if (p.ApplicationUI != null)
                        {
                            if (p.ApplicationUI.LookupVal.ToUpper()!="ULTIMATEHO")
                            {
                                Msg.Add("First define UltimateHo Password Policy .");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
 
                    }
                    var passwordpolicydatacheck = db.PasswordPolicy.Include(z => z.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").SingleOrDefault();
                    //if (passwordpolicydatacheck == null)
                    //{
                    //    Msg.Add("First define UltimateHo Password Policy .");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    bool getAllowEncryption = false;

                    string getApplicationUiLkVal = "";

                    if (p.ApplicationUI != null)
                    {
                        getApplicationUiLkVal = p.ApplicationUI.LookupVal.ToUpper();
                    }

                    if (getApplicationUiLkVal == "ESS" || getApplicationUiLkVal == "MOBILE")
                    {
                        getAllowEncryption = passwordpolicydatacheck.AllowEncryption;
                    }
                    else
                    {
                        getAllowEncryption = p.AllowEncryption;
                    }
                   
                    //string getApplicationUI = form["ApplicationUIList"] == "0" ? "" : form["ApplicationUIList"];
                    //if (getApplicationUI != null && getApplicationUI != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(getApplicationUI));
                    //    p.ApplicationUI = val;
                    //}

                    var passwordpolicydata = db.PasswordPolicy.Include(z => z.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == getApplicationUiLkVal.ToUpper()).SingleOrDefault();
                    if (passwordpolicydata != null)
                    {
                        Msg.Add("Password Policy Already Exits..!");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var passlength = form["MinPwdLength"];

                    try
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                PasswordPolicy ps = new PasswordPolicy()
                                {
                                    MinLowChars = p.MinLowChars,
                                    MinUpChars = p.MinUpChars,
                                    MinNoNos = p.MinNoNos,
                                    MinSymbols = p.MinSymbols,
                                    AllowAttempt = p.AllowAttempt,
                                    ForChangedPwd = p.ForChangedPwd,
                                    MultipleLogin = p.MultipleLogin,
                                    MinPwdLength = p.MinPwdLength,
                                    OldPwdAllow = p.OldPwdAllow,
                                    Captcha = p.Captcha,
                                    //AllowEncryption = p.AllowEncryption,
                                    AllowEncryption = getAllowEncryption,

                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },

                                    ApplicationUI = p.ApplicationUI,
                                    Authority = p.Authority,
                                    BadLogin = p.BadLogin,
                                    CharsOnly = p.CharsOnly,
                                    DefaultPasswordChangeInit = p.DefaultPasswordChangeInit,
                                    DisallowedChars = p.DisallowedChars,
                                    DualAuthenticationAppl = p.DualAuthenticationAppl,
                                    EmpStatus = p.EmpStatus,
                                    Forgot = p.Forgot,
                                    IsLoginSecurityAppl = p.IsLoginSecurityAppl,
                                    IsPasswordRotationSecurity = p.IsPasswordRotationSecurity,
                                    IsPasswordSecurityAppl = p.IsPasswordSecurityAppl,
                                    Lock = p.Lock,
                                    LockMin = p.LockMin,
                                    LoginRandomNoGenAppl = p.LoginRandomNoGenAppl,
                                    LoginSecurityQuestionAppl = p.LoginSecurityQuestionAppl,
                                    MinAllowRep = p.MinAllowRep,
                                    MinLowCharsStrength = p.MinLowCharsStrength,
                                    MinUpCharsStrength = p.MinUpCharsStrength,
                                    OldPwdAllowCount = p.OldPwdAllowCount,
                                    PasswordRandomNoGenAppl = p.PasswordRandomNoGenAppl,
                                    PasswordSecurityQuestion = p.PasswordSecurityQuestion,
                                    PolicyName = p.PolicyName,
                                    Pwd = p.Pwd,
                                    PwdChangedAfterDays = p.PwdChangedAfterDays == null ? null : p.PwdChangedAfterDays,
                                    PwdStregth = p.PwdStregth,
                                    SecurityExpiryTimeSec = p.SecurityExpiryTimeSec,
                                    SecurityRetryCount = p.SecurityRetryCount,
                                    StartNumericAppl = p.StartNumericAppl,
                                    Suspend = p.Suspend,
                                    Sessions = p.Sessions,
                                    PasswordOTPAppl = p.PasswordOTPAppl,
                                    OTPOnMobileAppl = p.OTPOnMobileAppl,
                                    OTPOnEmailAppl = p.OTPOnEmailAppl,
                                    OTPRetainDaysAppl = p.OTPRetainDaysAppl,
                                    OTPActiveTime = p.OTPActiveTime,
                                    OTPRetainDays = p.OTPRetainDays,
                                    LoginOTPAppl = p.LoginOTPAppl,


                                };

                                var companyExist = db.Company.Include(e => e.PasswordPolicy).FirstOrDefault();
                                
                                var PasswordPolicy_list = new List<PasswordPolicy>();
                                PasswordPolicy_list.Add(ps);
                                if (company_value == null && companyExist.PasswordPolicy.Count() == 0)
                                {
                                    db.PasswordPolicy.Add(ps);
                                    db.SaveChanges();
                                    companyExist.PasswordPolicy = PasswordPolicy_list;
                                    db.Company.Attach(companyExist);
                                    db.Entry(companyExist).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    PasswordPolicy_list.AddRange(company_value.PasswordPolicy);
                                   
                                    company_value.PasswordPolicy = PasswordPolicy_list;
                                    db.Company.Attach(company_value);
                                    db.Entry(company_value).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                

                                
                                if (firstlog != null)
                                {
                                    var adminpass = db.Login.Where(e => e.UserId == "admin").SingleOrDefault();
                                    if (adminpass != null)
                                    {
                                        adminpass.Password = getAllowEncryption == true ? P2BSecurity.Encrypt("p2b@1234") : "p2b@1234";
                                        db.Entry(adminpass).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                    }
                                }
                                ts.Complete();
                                if (firstlog != null)
                                {
                                    var url = "./Login/Index";
                                    return JavaScript("alert('Password Policy Created.') ;location.replace('" + url + "')");
                                    //return Json(new { success = true, data = RedirectToAction("Logout", "Login"), responseText = "Password Policy Created." }, JsonRequestBehavior.AllowGet);
                                }
                                Msg.Add("Password Policy Created..!");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            StringBuilder SB = new StringBuilder("");
                            foreach (ModelState modelstate in ModelState.Values)
                            {
                                foreach (ModelError error in modelstate.Errors)
                                {
                                    SB.Append(error.ErrorMessage + ". \n");
                                }

                            }
                            var ErrorMsg = SB.ToString();
                            Msg.Add(ErrorMsg);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception e)
                    {
                        Msg.Add(e.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }

        public ActionResult GetPasswordPolicy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PasswordPolicy.OrderByDescending(e => e.Id).FirstOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.PasswordPolicy
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        MinLowChars = e.MinLowChars,
                        MinUpChars = e.MinUpChars,
                        MinNoNos = e.MinNoNos,
                        MinSymbols = e.MinSymbols,
                        AllowAttempt = e.AllowAttempt,
                        ForChangedPwd = e.ForChangedPwd,
                        MultipleLogin = e.MultipleLogin,
                        MinPwdLength = e.MinPwdLength,
                        OldPwdAllow = e.OldPwdAllow,
                        Captcha = e.Captcha,
                        AllowEncryption = e.AllowEncryption,

                        ApplicationUILK = e.ApplicationUI != null ? e.ApplicationUI.Id : 0,
                        Authority = e.Authority,
                        BadLogin = e.BadLogin,
                        CharsOnly = e.CharsOnly,
                        DefaultPasswordChangeInit = e.DefaultPasswordChangeInit,
                        DisallowedChars = e.DisallowedChars,
                        DualAuthenticationAppl = e.DualAuthenticationAppl,
                        EmpStatus = e.EmpStatus,
                        Forgot = e.Forgot,
                        IsLoginSecurityAppl = e.IsLoginSecurityAppl,
                        IsPasswordRotationSecurity = e.IsPasswordRotationSecurity,
                        IsPasswordSecurityAppl = e.IsPasswordSecurityAppl,
                        Lock = e.Lock,
                        LockMin = e.LockMin,
                        LoginRandomNoGenAppl = e.LoginRandomNoGenAppl,
                        LoginSecurityQuestionAppl = e.LoginSecurityQuestionAppl,
                        MinAllowRep = e.MinAllowRep,
                        MinLowCharsStrength = e.MinLowCharsStrength,
                        MinUpCharsStrength = e.MinUpCharsStrength,
                        OldPwdAllowCount = e.OldPwdAllowCount,
                        PasswordRandomNoGenAppl = e.PasswordRandomNoGenAppl,
                        PasswordSecurityQuestion = e.PasswordSecurityQuestion,
                        PolicyName = e.PolicyName,
                        Pwd = e.Pwd,
                        PwdChangedAfterDays = e.PwdChangedAfterDays == null ? null : e.PwdChangedAfterDays,
                        PwdStregth = e.PwdStregth,
                        SecurityExpiryTimeSec = e.SecurityExpiryTimeSec,
                        SecurityRetryCount = e.SecurityRetryCount,
                        StartNumericAppl = e.StartNumericAppl,
                        Suspend = e.Suspend,
                        Sessions = e.Sessions,
                        PasswordOTPAppl = e.PasswordOTPAppl,
                        OTPOnMobileAppl = e.OTPOnMobileAppl,
                        OTPOnEmailAppl = e.OTPOnEmailAppl,
                        OTPRetainDaysAppl = e.OTPRetainDaysAppl,
                        OTPActiveTime = e.OTPActiveTime,
                        OTPRetainDays = e.OTPRetainDays,
                        LoginOTPAppl = e.LoginOTPAppl,
                    }).ToList();

                var Corp = db.PasswordPolicy.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;

                return Json(new Object[] { returndata, JsonRequestBehavior.AllowGet });
            }

        }


        [HttpPost]
        public async Task<ActionResult> EditSave(PasswordPolicy c, int data, FormCollection form) // Edit submit 
        {
            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string getApplicationUI = form["ApplicationUIList"] == "0" ? "" : form["ApplicationUIList"];
                    //int getApplicationUiId = getApplicationUI != "" ? Convert.ToInt32(getApplicationUI) : 0;
                    int getApplicationUiId = Convert.ToInt32(getApplicationUI);
                    //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                    //{
                    //var db_data = db.PasswordPolicy.Where(e => e.Id == data).FirstOrDefault();

                    //db.PasswordPolicy.Attach(db_data);
                    //db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    //db.SaveChanges();
                    //TempData["RowVersion"] = db_data.RowVersion;

                    var db_datacheck = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.Id == data).FirstOrDefault();
                    bool encryptfirst = db_datacheck.AllowEncryption;
                    bool decryptfirst = db_datacheck.AllowEncryption;


                    PasswordPolicy PassPolicy = db.PasswordPolicy.Find(data);
                    TempData["CurrRowVersion"] = PassPolicy.RowVersion;
                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                    {
                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = PassPolicy.DBTrack.CreatedBy == null ? null : PassPolicy.DBTrack.CreatedBy,
                            CreatedOn = PassPolicy.DBTrack.CreatedOn == null ? null : PassPolicy.DBTrack.CreatedOn,
                            Action = "M",
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        PassPolicy.ApplicationUI = db.LookupValue.Find(getApplicationUiId); // For LookupValue of ApplicationUI

                        PassPolicy.MinPwdLength = c.MinPwdLength; // For Textboxes
                        PassPolicy.MinUpChars = c.MinUpChars;
                        PassPolicy.MinLowChars = c.MinLowChars;
                        PassPolicy.MinNoNos = c.MinNoNos;
                        PassPolicy.MinSymbols = c.MinSymbols;
                        PassPolicy.AllowAttempt = c.AllowAttempt;

                        PassPolicy.Authority = c.Authority;
                        PassPolicy.BadLogin = c.BadLogin;
                        PassPolicy.DisallowedChars = c.DisallowedChars;
                        PassPolicy.EmpStatus = c.EmpStatus;
                        PassPolicy.LockMin = c.LockMin;
                        PassPolicy.MinAllowRep = c.MinAllowRep;
                        PassPolicy.OldPwdAllowCount = c.OldPwdAllowCount;
                        PassPolicy.OTPActiveTime = c.OTPActiveTime;
                        PassPolicy.OTPOnEmailAppl = c.OTPOnEmailAppl;
                        PassPolicy.OTPRetainDays = c.OTPRetainDays;
                        PassPolicy.MinLowCharsStrength = c.MinLowCharsStrength;
                        PassPolicy.MinUpCharsStrength = c.MinUpCharsStrength;
                        PassPolicy.PolicyName = c.PolicyName;
                        PassPolicy.Pwd = c.Pwd;
                        PassPolicy.PwdChangedAfterDays = c.PwdChangedAfterDays;
                        PassPolicy.PwdStregth = c.PwdStregth;
                        PassPolicy.SecurityRetryCount = c.SecurityRetryCount;
                        PassPolicy.Sessions = c.Sessions;
                        PassPolicy.SecurityExpiryTimeSec = c.SecurityExpiryTimeSec;




                        PassPolicy.MultipleLogin = c.MultipleLogin; // For Radio Buttons
                        PassPolicy.OldPwdAllow = c.OldPwdAllow;
                        PassPolicy.PasswordSecurityQuestion = c.PasswordSecurityQuestion;
                        PassPolicy.LoginSecurityQuestionAppl = c.LoginSecurityQuestionAppl;
                        PassPolicy.Captcha = c.Captcha;
                        PassPolicy.AllowEncryption = c.AllowEncryption;

                        PassPolicy.CharsOnly = c.CharsOnly;
                        PassPolicy.DefaultPasswordChangeInit = c.DefaultPasswordChangeInit;
                        PassPolicy.DualAuthenticationAppl = c.DualAuthenticationAppl;
                        PassPolicy.Forgot = c.Forgot;
                        PassPolicy.IsLoginSecurityAppl = c.IsLoginSecurityAppl;
                        PassPolicy.IsPasswordRotationSecurity = c.IsPasswordRotationSecurity;
                        PassPolicy.IsPasswordSecurityAppl = c.IsPasswordSecurityAppl;
                        PassPolicy.Lock = c.Lock;
                        PassPolicy.LoginOTPAppl = c.LoginOTPAppl;
                        PassPolicy.LoginRandomNoGenAppl = c.LoginRandomNoGenAppl;
                        PassPolicy.LoginSecurityQuestionAppl = c.LoginSecurityQuestionAppl;
                        PassPolicy.OTPOnMobileAppl = c.OTPOnMobileAppl;
                        PassPolicy.OTPRetainDaysAppl = c.OTPRetainDaysAppl;
                        PassPolicy.PasswordOTPAppl = c.PasswordOTPAppl;
                        PassPolicy.PasswordRandomNoGenAppl = c.PasswordRandomNoGenAppl;
                        PassPolicy.PasswordSecurityQuestion = c.PasswordSecurityQuestion;
                        PassPolicy.StartNumericAppl = c.StartNumericAppl;
                        PassPolicy.Suspend = c.Suspend;

                        var Emp = (Employee)null;

                        if (SessionManager.EmpId != null && SessionManager.EmpId != "")
                        {
                            Emp = db.Employee.Find(Convert.ToInt32(SessionManager.EmpId));
                        }
                        var id = Convert.ToInt32(SessionManager.EmpId);


                        if (db_datacheck.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO")
                        {


                            if (encryptfirst == false)// first time edit save encryption allow(if encrypted second time will not encrypt)
                            {
                                if (PassPolicy.AllowEncryption == true)
                                {
                                    var emppassword = db.Employee.ToList();

                                    foreach (var emppid in emppassword)
                                    {

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                                        {

                                            var logindetails = db.Employee.Select(d => new
                                            {
                                                Id = d.Id,
                                                Login = d.Login,
                                                Login_Id = d.Login_Id,
                                                LoginDetails = d.Login.LoginDetails.ToList()
                                            }).Where(e => e.Id == emppid.Id && e.Login != null).FirstOrDefault();

                                            //var logindetails = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.Id == emppid.Id && e.Login != null).FirstOrDefault();

                                            //var LoginId = db.Employee.Include(e => e.Login)
                                            //                         .Where(e => e.Id == emppid.Id).FirstOrDefault().Login_Id;

                                            if (logindetails != null)
                                            {
                                                var LoginId = logindetails.Login_Id;
                                                int logID = Convert.ToInt32(LoginId);


                                                string LoginOld_pwd = db.Login.Find(LoginId).Password;
                                                string EncryptAdminPassword = P2BSecurity.Encrypt(LoginOld_pwd);

                                                Login LogIn = db.Login.Find(logID);
                                                LogIn.Password = EncryptAdminPassword;

                                                logindetails.Login.Password = LogIn.Password;
                                                db.Entry(LogIn).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();

                                                var LogIndet = db.Login.Include(e => e.LoginDetails).Where(e => e.Id == logID).SingleOrDefault();
                                                foreach (var item in LogIndet.LoginDetails)
                                                {
                                                    var logdetpass = db.LoginDetails.Find(item.Id);
                                                    string Encryptlogdetoldpass = P2BSecurity.Encrypt(item.OldPwd);
                                                    string EncryptlogdetNewpass = P2BSecurity.Encrypt(item.NewPwd);

                                                    logdetpass.OldPwd = Encryptlogdetoldpass;
                                                    logdetpass.NewPwd = EncryptlogdetNewpass;
                                                    db.LoginDetails.Attach(logdetpass);
                                                    db.Entry(logdetpass).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }



                                            }
                                            ts.Complete();
                                        }

                                    }
                                }
                            }
                            //decrypt



                            if (decryptfirst == true)// first time edit save encryption allow(if encrypted second time will not encrypt)
                            {
                                if (PassPolicy.AllowEncryption == false)
                                {
                                    var emppassword = db.Employee.ToList();

                                    foreach (var emppid in emppassword)
                                    {

                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                                        {

                                            var logindetails = db.Employee.Select(d => new
                                            {
                                                Id = d.Id,
                                                Login = d.Login,
                                                Login_Id = d.Login_Id,
                                                LoginDetails = d.Login.LoginDetails.ToList()
                                            }).Where(e => e.Id == emppid.Id && e.Login != null).FirstOrDefault();

                                            //var logindetails = db.Employee.Include(e => e.Login).Include(e => e.Login.LoginDetails).Where(e => e.Id == emppid.Id && e.Login != null).FirstOrDefault();

                                            //var LoginId = db.Employee.Include(e => e.Login)
                                            //                         .Where(e => e.Id == emppid.Id).FirstOrDefault().Login_Id;

                                            if (logindetails != null)
                                            {
                                                var LoginId = logindetails.Login_Id;
                                                int logID = Convert.ToInt32(LoginId);


                                                string LoginOld_pwd = db.Login.Find(LoginId).Password;
                                                string EncryptAdminPassword = P2BSecurity.Decrypt(LoginOld_pwd);

                                                Login LogIn = db.Login.Find(logID);
                                                LogIn.Password = EncryptAdminPassword;

                                                logindetails.Login.Password = LogIn.Password;
                                                db.Entry(LogIn).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();

                                                var LogIndet = db.Login.Include(e => e.LoginDetails).Where(e => e.Id == logID).SingleOrDefault();
                                                foreach (var item in LogIndet.LoginDetails)
                                                {
                                                    var logdetpass = db.LoginDetails.Find(item.Id);
                                                    string Encryptlogdetoldpass = P2BSecurity.Decrypt(item.OldPwd);
                                                    string EncryptlogdetNewpass = P2BSecurity.Decrypt(item.NewPwd);

                                                    logdetpass.OldPwd = Encryptlogdetoldpass;
                                                    logdetpass.NewPwd = EncryptlogdetNewpass;
                                                    db.LoginDetails.Attach(logdetpass);
                                                    db.Entry(logdetpass).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                }



                                            }
                                            ts.Complete();
                                        }

                                    }
                                }
                            }

                            PassPolicy.Id = data;
                            PassPolicy.DBTrack = c.DBTrack;
                            db.Entry(PassPolicy).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                        }

                        if (db_datacheck.ApplicationUI.LookupVal.ToUpper() == "ESS")
                        {
                            PassPolicy.Id = data;
                            PassPolicy.DBTrack = c.DBTrack;
                            db.Entry(PassPolicy).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        if (db_datacheck.ApplicationUI.LookupVal.ToUpper() == "MOBILE")
                        {
                            PassPolicy.Id = data;
                            PassPolicy.DBTrack = c.DBTrack;
                            db.Entry(PassPolicy).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }

                        var passwordpolicydatacheck = db.PasswordPolicy.Include(z => z.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").SingleOrDefault();

                        var passwordpolicydatamobileoress = db.PasswordPolicy.Include(z => z.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() != "ULTIMATEHO").ToList();
                        foreach (var item in passwordpolicydatamobileoress)
                        {
                            item.AllowEncryption = passwordpolicydatacheck.AllowEncryption;
                            db.PasswordPolicy.Attach(item);
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                       
                        //}

                        //await db.SaveChangesAsync();
                        //   ts.Complete();
                    }
                    Msg.Add(" Record Updated Successfully ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }

        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<PasswordPolicy> PasswordPolicy = null;
                IEnumerable<PasswordPolicy> IE;

                if (gp.IsAutho == true)
                {
                    PasswordPolicy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {

                    PasswordPolicy = db.PasswordPolicy.Include(e => e.ApplicationUI).ToList();
                }
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = PasswordPolicy;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.MinPwdLength, a.MinUpChars, a.MinLowChars, a.MinSymbols, a.AllowEncryption, a.ApplicationUI.LookupVal })
                            .Where((e => (e.Id.ToString() == gp.searchString) ||
                                   (e.MinPwdLength.ToString().ToLower() == gp.searchString.ToLower())
                            || (e.MinUpChars.ToString().ToLower() == gp.searchString.ToLower())
                            || (e.MinLowChars.ToString().ToLower() == gp.searchString.ToLower())
                            || (e.MinSymbols.ToString().ToLower() == gp.searchString.ToLower())
                            || (e.AllowEncryption.ToString().ToLower() == gp.searchString.ToLower())
                             || (e.LookupVal.ToString().ToLower() == gp.searchString.ToLower())
                            )).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.PayScale) != null ? Convert.ToString(a.PayScale.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.MinPwdLength, a.MinUpChars, a.MinLowChars, a.MinSymbols, a.AllowEncryption, a.ApplicationUI.LookupVal, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PasswordPolicy;
                    Func<PasswordPolicy, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "MinPwdLength" ? c.MinPwdLength.ToString() :
                                         gp.sidx == "MinUpChars" ? c.MinUpChars.ToString() :
                                         gp.sidx == "MinLowChars" ? c.MinLowChars.ToString() :
                                         gp.sidx == "MinSymbols" ? c.MinSymbols.ToString() :
                                         gp.sidx == "AllowEncryption" ? c.AllowEncryption.ToString() :
                                          gp.sidx == "Application" ? c.AllowEncryption.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.MinPwdLength, a.MinUpChars, a.MinLowChars, a.MinSymbols, a.AllowEncryption, a.ApplicationUI.LookupVal, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.MinPwdLength, a.MinUpChars, a.MinLowChars, a.MinSymbols, a.AllowEncryption, a.ApplicationUI.LookupVal, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.MinPwdLength, a.MinUpChars, a.MinLowChars, a.MinSymbols, a.AllowEncryption, a.ApplicationUI.LookupVal, a.Id }).ToList();
                    }
                    totalRecords = PasswordPolicy.Count();
                }
                if (totalRecords > 0)
                {
                    totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                }
                if (gp.page > totalPages)
                {
                    gp.page = totalPages;
                }
                var JsonData = new
                {
                    page = gp.page,
                    rows = jsonData,
                    records = totalRecords,
                    total = totalPages
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}