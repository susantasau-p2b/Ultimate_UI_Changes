using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EssPortal.Security;

namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class WelcomeScreenController : Controller
    {
        //
        // GET: /WelcomeScreen/
        public ActionResult Index()
        {
            return View("~/Views/Shared/_WelcomeScreen.cshtml");
        }
	}
}