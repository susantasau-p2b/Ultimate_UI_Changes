using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers
{
    public class HelpController : Controller
    {
        //
        // ET: /Test/
        public ActionResult tempurl(String data)
        {
            var url = UrlHelper.GenerateUrl(
                null,"Index","Help", null, null,data, null,Url.RouteCollection,Url.RequestContext, false
                );
            return Redirect(url);
        }
        public ActionResult Index()
        {
            return View();
        }

        public string Help { get; set; }
    }
}