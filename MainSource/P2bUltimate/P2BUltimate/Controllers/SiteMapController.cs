using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
     [AuthoriseManger]
    public class SiteMapController : Controller
    {
        //
        // GET: /SiteMap/
        public ActionResult Index()
        {
            return View();
        }
	}
}