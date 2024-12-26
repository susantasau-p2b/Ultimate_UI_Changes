using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
    public class XmlController : Controller
    {
        //
        // GET: /Xml/
        public ActionResult ReadXml()
        {
            return File(Server.MapPath("~/App_Data/Help_Page_link.xml"), "text/xml");
        }
	}
}