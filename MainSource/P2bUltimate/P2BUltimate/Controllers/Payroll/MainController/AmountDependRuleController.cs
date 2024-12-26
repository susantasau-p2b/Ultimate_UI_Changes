using P2b.Global;
using P2BUltimate.App_Start;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
    public class AmountDependRuleController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /AmountDependRule/

        public ActionResult Index()
        {
            return View();
        }


       

    }
}
