using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class EPFTDashBoardController : Controller
    {
        //
        // GET: /EPFTDashBoard/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/EPFTDashBoard/Index.cshtml");
        }
	}
}