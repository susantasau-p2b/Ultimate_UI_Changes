using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
    public class AdvanceFilterNewController : Controller
    {
        //
        // GET: /TranscationMapping/
        public ActionResult Index()
        {
            
            return View("~/Views/Shared/_AdvanceFilterNew.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_AdvanceFilterNew.cshtml");
        }
	}
}