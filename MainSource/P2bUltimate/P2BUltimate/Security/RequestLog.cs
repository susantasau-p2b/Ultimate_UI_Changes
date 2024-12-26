using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using P2BUltimate.Security;
using P2BUltimate.Models;

namespace P2BUltimate.Security
{
    public class RequestLog : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.HttpMethod == "POST")
                Utility.DumpRequest(filterContext.HttpContext.Request.Url.ToString(), SessionManager.EmpId);
            this.OnActionExecuting(filterContext);
        }
    }
}