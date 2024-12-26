using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EssPortal.Controllers
{
    public class LoginDetailsController : Controller
    {
        //
        // GET: /LoginDetails/
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult create()
        {
            return View();
        }
	}
}