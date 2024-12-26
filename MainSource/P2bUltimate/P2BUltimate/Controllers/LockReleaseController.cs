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

namespace P2BUltimate.Controllers
{
    public class LockReleaseController : Controller
    {
        //
        // GET: /LogRelease/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_LockRelease.cshtml");
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
        public ActionResult GetEmp(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).Where(e => e.Login.IsActive != true && e.Login != null).ToList();
                // var EmpData = db.Employee.Include(e => e.EmpName).Include(e => e.Login).ToList();
                var SelectListItem = new List<SelectListItem>();
                foreach (var item in EmpData)
                {
                    SelectListItem.Add(new SelectListItem
                    {
                        Text = item.FullDetails,
                        Value = item.Id.ToString(),
                    });
                }
                var r = (from ca in SelectListItem select new { srno = ca.Value, lookupvalue = ca.Text }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);

            }
            // return View();
        }

        public ActionResult create(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string employee = form["EmployeeNamelist"] != null ? form["EmployeeNamelist"] : null;
                List<int> ids = null;
                if (employee != null && employee != "0" && employee != "false")
                {
                    ids = Utility.StringIdsToListIds(employee);
                }
                else
                {
                    return Json(new { success = false, responseText = "Select Employee" });
                }
                //if (employee != null || employee != "")
                //{
                foreach (var item in ids)
                {
                    // string set1 = employee.Substring(0, employee.IndexOf(","));
                    // string set12 = set1.Substring(7);
                    // var id = Convert.ToString(set12);
                    var EmpCheck = db.Employee.Include(e => e.Login).Where(e => e.Id == item).SingleOrDefault();

                    if (EmpCheck.Login != null)
                    {
                        //EmpCheck.Login.IsUltimateAppl = 0;
                        EmpCheck.Login.IsActive = true; ;

                        db.Entry(EmpCheck).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(EmpCheck).State = System.Data.Entity.EntityState.Detached;

                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Employee is not logged in" });

                    }
                }
                return Json(new { success = true, responseText = "User Lock Released Successfully!" });

                // }
                //return Json(new { success = false, responseText = "Select Employee" });
            }

        }
    }
}