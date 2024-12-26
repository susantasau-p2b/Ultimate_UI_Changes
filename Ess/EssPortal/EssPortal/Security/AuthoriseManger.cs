using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EssPortal.Security;
namespace EssPortal.Security
{
    public class AuthoriseManger : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (string.IsNullOrEmpty(SessionManager.UserName))
            {
                filterContext.Result = new RedirectToRouteResult
                    (new RouteValueDictionary
                        (new { Controller = "login", Action = "index",id="00" }));
                 
            }
            else
            {
                Principal cp = new Principal(SessionManager.UserName);
            }
        }
    }
}