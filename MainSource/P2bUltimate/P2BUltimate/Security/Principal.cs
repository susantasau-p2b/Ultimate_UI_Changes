using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Web.Mvc;

namespace P2BUltimate.Security
{
    public class Principal:IPrincipal
    {
        
        public Principal(string username)
        {
            this.Identity=new GenericIdentity(username);
        }
        public IIdentity Identity
        {
            get;
            set;
        }
        public bool IsInRole(string role)
        {
            return false;
            //var roles = role.Split(new char[] { ',' });
            //return roles.Any(r=>this.Account.roles);
        }
    }
}