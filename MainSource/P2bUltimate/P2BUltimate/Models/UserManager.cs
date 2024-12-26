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
using System.Data.Linq;
using System.Data.Entity.Core.Objects;

namespace P2BUltimate.Models
{
    public static class UserManager
    {
        private static DataBaseContext db = new DataBaseContext();

        
        public static String CheckPass(String data)
        {
            List<string> Msg = new List<string>();

            if (data != null)
            {
                var id = Convert.ToInt32(SessionManager.EmpId);
                PasswordPolicy passlogin_policy = db.PasswordPolicy.Include(e => e.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").FirstOrDefault();
               // string oldpass = passlogin_policy.AllowEncryption == true ? P2BSecurity.Encrypt(data) : data;

                var Logindetails = db.LoginDetails.ToList().LastOrDefault();
                //string oldLogdetailspass = Convert.ToString(Logindetails.OldPwd);
                //var LoginOld_pwd = db.Employee.Include(e => e.Login)
                //                          .Include(e => e.Login.LoginDetails)
                //                 .Where(e => e.Id == id).SingleOrDefault().Login.LoginDetails.ToList().LastOrDefault().OldPwd;

               // db.RefreshAllEntites(System.Data.Entity.Core.Objects.RefreshMode.StoreWins);
                var LoginId = db.Employee.Include(e => e.Login)
                                 .Where(e => e.Id == id).FirstOrDefault().Login_Id;


                string LoginOld_pwd = db.Login.Find(LoginId).Password;
                
                string LoginOldPassword = Convert.ToString(LoginOld_pwd);
                
                
                string password = "";
                //string EncryptPassword = "";
                //EncryptPassword = P2BSecurity.Encrypt(OLoginUserPass.Login.Password);
                
                if (passlogin_policy.AllowEncryption == true)
                {
                    //password = P2BSecurity.Decrypt(OLoginUserPass.Login.Password);
                    password = P2BSecurity.Decrypt(LoginOldPassword);
                }
                else
                {
                    //password = OLoginUserPass.Login.Password;
                    password = LoginOldPassword;
                }






                var a = db.Employee.Include(e => e.Login).Where(e => e.Id == id && e.Login != null && password == data).SingleOrDefault();


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
        public static string RoleCheck()
        {

            if (SessionManager.EmpId == null)
            {
                return "0";
            }
            var id = Convert.ToInt32(SessionManager.EmpId);
            var qurey = db.Employee.Include(e => e.Login).Where(e => e.Id == id && e.Login != null && e.Login.UserId == "admin").SingleOrDefault();
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
}