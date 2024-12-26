using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class EmployeeAttendanceActionPolicyStructDetailsController : Controller
    {
        //
        // GET: /EmployeeAttendanceActionPolicyStructDetails/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/EmployeeAttendanceActionPolicyStructDetails/Index.cshtml");
        }
	}
}