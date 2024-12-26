using EssPortal.App_Start;
using P2b.Global;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace EssPortal.Controllers
{
    public class CalendarController : Controller
    {
        //
        // GET: /Calendar/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetCalendarDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").ToList();
                IEnumerable<Calendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetFinancialYear(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").SingleOrDefault();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
    }
}