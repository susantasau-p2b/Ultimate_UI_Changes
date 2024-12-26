using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EssPortal.Controllers
{
    public class BMSController : Controller
    {
        //
        // GET: /BMS/
        public ActionResult Index()
        {
            return View("~/Views/BMS/HotelBooking/Index.cshtml");
        }
	}
}