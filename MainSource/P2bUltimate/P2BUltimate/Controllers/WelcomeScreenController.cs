﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers
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