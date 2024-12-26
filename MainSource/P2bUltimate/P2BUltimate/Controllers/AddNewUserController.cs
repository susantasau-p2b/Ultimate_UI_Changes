using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using P2BUltimate.App_Start;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;
using Leave;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class AddNewUserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_AddNewUser.cshtml");
        }
        // private DataBaseContext db = new DataBaseContext();
        public class returnJsonClass
        {
            public String data { get; set; }
            public bool success { get; set; }
            public String responseText { get; set; }
        }
        public Object Create(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Group = form["Group"] != null ? form["Group"] : null;
                string employee = form["employee"] != null ? form["employee"] : null;
                string username = form["username"] != null ? form["username"] : null;
                if (employee != null || employee != "")
                {
                    var id = Convert.ToString(employee);
                    var EmpCheck = db.Employee.Include(e => e.Login).Where(e => e.EmpCode == id).SingleOrDefault();
                    if (EmpCheck.Login != null)
                    {
                       // EmpCheck.Login.IsUltimateAppl = false;
                        EmpCheck.Login.IsUltimateHOAppl = true;
                        db.Entry(EmpCheck).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(EmpCheck).State = System.Data.Entity.EntityState.Detached;
                        return new returnJsonClass { success = false, responseText = "Already Register, Now User Can Access..!" };
                    }
                }
                else
                {

                }

                Login _Login = new Login();
                //var a=GroupDataList
                if (Group != null)
                {
                    var GroupDataList = new List<KeyValuePair<string, string>>();
                    GroupDataList.Add(new KeyValuePair<string, string>("1", "Auditor"));
                    GroupDataList.Add(new KeyValuePair<string, string>("2", "Checker"));
                    GroupDataList.Add(new KeyValuePair<string, string>("3", "Maker"));

                }
                if (username != null)
                {
                    _Login.UserId = username.Trim();
                    _Login.Password = username.Trim();
                    _Login.IsActive = true;
                    _Login.IsUltimateHOAppl = true;
                    _Login.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                }
                if (employee == null)
                {
                    return new returnJsonClass { success = false, responseText = "Select Employee..!" };
                }
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var id = employee;
                        var EmpData = db.Employee.Include(e => e.Login).Where(e => e.EmpCode == id).SingleOrDefault();
                        EmpData.Login = _Login;
                        db.Entry(EmpData).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        var LoginCheck = db.Employee.Include(e => e.Login).Where(e => e.Login != null && e.Login.Id == _Login.Id).SingleOrDefault();
                        Session["NewUserId"] = LoginCheck != null ? LoginCheck.EmpCode.ToString() : null;
                        ts.Complete();
                        return new returnJsonClass { success = true, data = "", responseText = "New User Added,<b> Now go Assign Authority for that Employee</b>" };
                    }
                }
                catch (Exception e)
                {

                    throw e;
                }
            }
        }
        public ActionResult GetEmp(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).Where(e => e.Login == null || (e.Login != null && e.Login.IsUltimateHOAppl == false)).OrderBy(e => e.EmpCode).ToList();
                var SelectListItem = new List<SelectListItem>();
                foreach (var item in EmpData)
                {
                    SelectListItem.Add(new SelectListItem
                    {
                        Text = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML,
                        Value = item.EmpCode.ToString()
                    });

                }

                return Json(SelectListItem, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmp1(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpData = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Include(e => e.Login).Where(e => e.Login != null && e.Login.IsUltimateHOAppl == true && e.ServiceBookDates != null && e.ServiceBookDates.ServiceLastDate == null && e.Login.UserId != "admin").OrderBy(e => e.EmpCode).ToList();

               
                var SelectListItem = new List<SelectListItem>();
                foreach (var item in EmpData)
                {
                      var path = "";
                      path = Server.MapPath("~/App_Data/Menu_Json/" + item.EmpCode + ".json");

                        if (!System.IO.File.Exists(path))
                        {
                            SelectListItem.Add(new SelectListItem
                            {
                                Text = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML,
                                Value = item.EmpCode.ToString()
                            });
                        }

                }

                var r = (from ca in SelectListItem select new { srno = ca.Value, lookupvalue = ca.Text }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
                // return Json(SelectListItem, JsonRequestBehavior.AllowGet);
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

        public Object RemoveEmp(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp_code = form["Emp_drop"] != null ? form["Emp_drop"] : null;
                if (Emp_code == null)
                {
                    return new returnJsonClass { success = false };
                }
                var id = Convert.ToInt32(Emp_code);
                var qurey = db.Employee.Include(e => e.Login).Where(e => e.Id == id).SingleOrDefault();
                var login_id = qurey.Login;
                qurey.Login = null;
                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.Entry(login_id).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();

                SessionManager.UserName = null;
                return new returnJsonClass { success = true, responseText = "Employee Assigned..!", data = Url.Action("index", "Login") };
            }
        }
        public ActionResult AddOrRemove(FormCollection form)
        {
            var filter = form["action"] == null ? null : form["action"];
            if (filter.ToUpper() == "ADD")
            {
                var a = Create(form);
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else if (filter.ToUpper() == "DEL")
            {
                var b = RemoveEmp(form);
                return Json(b, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult EmployeeDrop()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).Where(e => e.Login != null).ToList();
                var SelectListItem = new List<SelectListItem>();
                foreach (var item in EmpData)
                {
                    SelectListItem.Add(new SelectListItem
                    {
                        Text = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML,
                        Value = item.Id.ToString()
                    });
                }
                return Json(SelectListItem, JsonRequestBehavior.AllowGet);
            }
        }
    }
}