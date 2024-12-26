using P2BUltimate.Security;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequestLog());
        }
    }
}
