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
namespace P2BUltimate.Core.Controllers
{
    public class EEISDashBoardController : Controller
    {
        //
        // GET: /DashBoard/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/EEISDashBoard/Index.cshtml");
        }
      
    }
}
