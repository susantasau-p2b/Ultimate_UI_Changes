using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.Core.MainController
{
    public class IncrDueListController : Controller
    {
        //
        // GET: /IncrDueListProcess/
        public ActionResult Index()
        {
			return View("~/Views/Core/MainViews/IncrDueListProcess/Index.cshtml");
        }
	}
}