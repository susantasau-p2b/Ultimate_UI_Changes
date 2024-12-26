using EssPortal.App_Start;
using Leave;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using EssPortal.Security;
using System.Diagnostics;
using Newtonsoft.Json;
using EssPortal.Process;

namespace EssPortal.Controllers
{
    public class ReportFilterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
      //  private DataBaseContext db = new DataBaseContext();
        public ActionResult GetSalaryHead(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.SalaryHead.ToList();
                SelectList drop = new SelectList(a, "Id", "Name", "");
                return Json(drop, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Calender()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Calendar.Select(e => new { srno = e.Id, lookupvalue = e.Name.LookupVal }).ToList();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public string GenrateReport(ReportUtility.FormCollectionClass form, string url)
        {
            var where = ReportUtility.EarningStatementWhereClause(form);
            return url + where;
        }
	}
}