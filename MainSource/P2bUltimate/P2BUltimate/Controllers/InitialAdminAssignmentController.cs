using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using System.Data.Entity;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using System.Net;
using P2BUltimate.Models;
using Payroll;
//using P2BUltimate.Process;
using System.Transactions;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class InitialAdminAssignmentController : Controller
    {
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_InitialAdminAssignment.cshtml");
        }
        public ActionResult Employee()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Employee.ToList();
                var s = new SelectList(qurey, "id", "empcode", "");
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Update(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp_code = form["Emp_code"] != null ? form["Emp_code"] : null;
                var id = Convert.ToInt32(Emp_code);
                var qurey = db.Employee.Include(e => e.Login).Where(e => e.Id == id).SingleOrDefault();
                var login = db.Login.Where(e => e.UserId == "admin").SingleOrDefault();
                qurey.Login = login;

                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                SessionManager.UserName = null;
                return Json(new { success = true, responseText = "Employee Assigned..!", data = Url.Action("index", "Login") }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}