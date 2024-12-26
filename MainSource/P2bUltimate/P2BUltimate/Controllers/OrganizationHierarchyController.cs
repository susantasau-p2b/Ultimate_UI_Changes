using P2BUltimate.App_Start;
using Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Data.Entity;
using System.Transactions;
using System.Text;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class OrganizationHierarchyController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult getlookup()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LWF = db.Lookup.Select(e => new { srno = e.Id, lookupvalue = e.Name }).ToList();

                return Json(LWF, JsonRequestBehavior.AllowGet);
            }
        }
    }
}