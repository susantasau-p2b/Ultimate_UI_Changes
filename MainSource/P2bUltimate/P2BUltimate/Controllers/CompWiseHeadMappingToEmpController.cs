using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Security;
using Newtonsoft.Json;

namespace P2BUltimate.Controllers
{
    public class CompWiseHeadMappingToEmpController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Polulate_Salaryhead()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.PayScaleAssignment.Where(e => e.SalHeadFormula.Count > 0 && e.SalHeadFormula.Any(z => z.GeoStruct != null)).Select(a => a.SalaryHead).OrderBy(e => e.Code).ToList();
                var s = new SelectList(qurey, "Id", "Name");
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public class FromCollectionClasss
        {
            public string Salhead { get; set; }
            //   public string Employee-Table { get; set; }
        }
        public ActionResult Create(FromCollectionClasss oFromCollectionClasss, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpIds = form["Employee-Table"] != null ? form["Employee-Table"] : null;
                var Salheads = oFromCollectionClasss.Salhead;
                return View();
            }
        }
    }
}