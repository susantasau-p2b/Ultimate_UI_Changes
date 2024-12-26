using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.PFTrust
{
    public class PFSettlementFormController : Controller
    {
        // GET: PFSettlementForm
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/PFSettlementForm/Index.cshtml");
        }
    }
}