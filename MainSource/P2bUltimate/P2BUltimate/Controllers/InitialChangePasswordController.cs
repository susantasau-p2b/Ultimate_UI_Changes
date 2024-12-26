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
    public class InitialChangePasswordController : Controller
    {
        //
       // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_InitialChangePassword.cshtml");
        }
        public ActionResult getdata()
        {
            var qurey = Utility.GetUserData();
            return Json(qurey, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Fill_CompanyDrop()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<SelectListItem> payscale_list = new List<SelectListItem>();
                var payscaleagreement_data = db.Company.ToList();
                foreach (var ca in payscaleagreement_data)
                {
                    payscale_list.Add(new SelectListItem
                    {
                        Value = ca.Id.ToString(),
                        Text = "CompCode:" + ca.Code + " CompName:" + ca.Name,
                        Selected = false
                    });
                }
                return Json(payscale_list, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Create(ChangePassword cp, FormCollection form, String capcha_text)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var checkstatus = db.PasswordPolicy.Include(z => z.ApplicationUI).Where(e => e.ApplicationUI.LookupVal.ToUpper() == "ULTIMATEHO").OrderByDescending(e => e.Id).FirstOrDefault();
                var user = db.Employee.Include(e => e.Login).Where(e => e.Id == cp.Employee.Id && e.Login != null).SingleOrDefault();

                var capcha_val = Convert.ToString(Session["captchaText"]);
                if (capcha_text != capcha_val && checkstatus.Captcha == true)
                {
                    return Json(new { success = false, responseText = "Invaild CaptCha..!" });
                }
                if (cp.OldPassword != user.Login.Password)
                {
                    return Json(new { success = false, responseText = "Invaild Old PassWord..!" });
                }

                if (checkstatus.OldPwdAllow == false && cp.NewPassword == user.Login.Password)
                {
                    return Json(new { success = false, responseText = "Old PassWord is Not Allowed..!" });
                }

                if (checkstatus.PasswordSecurityQuestion == false && (!String.IsNullOrEmpty(cp.SecurityQuestion) || !String.IsNullOrEmpty(cp.SecurityAnswer)))
                {
                    return Json(new { success = false, responseText = "Something is Wrong..!" });
                }

                var id = form["comp_drop"];
                if (id == null)
                {
                    return Json(new { success = false, responseText = "Select Comapny" });
                }
                cp.Company = db.Company.Find(Convert.ToInt32(id));
                cp.Employee = db.Employee.Find(Convert.ToInt32(SessionManager.EmpId));
                cp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName };
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        ChangePassword changepass = new ChangePassword
                        {
                            NewPassword = cp.NewPassword,
                            OldPassword = cp.OldPassword,
                            SecurityQuestion = cp.SecurityQuestion,
                            SecurityAnswer = cp.SecurityAnswer,
                            Employee = cp.Employee,
                            Company = cp.Company,
                            DBTrack = cp.DBTrack
                        };
                        db.ChangePassword.Add(changepass);
                        db.SaveChanges();

                        //saving new pass to login table
                        var EmpId = Convert.ToInt32(SessionManager.EmpId);
                        var qurey = db.Employee.Include(e => e.Login).Where(e => e.Id == EmpId).SingleOrDefault();
                        qurey.Login.Password = changepass.NewPassword;
                        db.SaveChanges();
                        ts.Complete();

                        return Json(new { success = true, responseText = "Password Updated..!" });
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}