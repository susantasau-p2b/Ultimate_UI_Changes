using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.EExMS.MainController
{
    public class ExpenseCalendarController : Controller
    {
        //
        // GET: /ExpenseCalendar/
        public ActionResult Index()
        {
            return View("~/views/EExMS/MainViews/ExpenseCalendar/Index.cshtml");
        }
	}
}