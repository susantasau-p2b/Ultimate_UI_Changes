using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
namespace P2BUltimate.Models
{
    public class FilterSession
    {
        public class Session
        {
            public class SessionData
            {
                public string type { get; set; }
                public int comp_code { get; set; }

            }
            public SessionData Check_flow()
            {
                if (HttpContext.Current.Session["object"] != null && HttpContext.Current.Session["CompId"] != null)
                {
                    var session_data = new SessionData
                    { 
                        type=HttpContext.Current.Session["object"].ToString(),
                        comp_code = Convert.ToInt32(HttpContext.Current.Session["CompId"].ToString()),
                    };
                    return session_data;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}