using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using System.Data.Entity;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using System.Net;
using P2BUltimate.Models;
using Payroll;
//using P2BUltimate.Process;
using System.Transactions;

namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class IntialSelectionController : Controller
    {
        // GET: IntialSelection
        public ActionResult Index()
        {
            return View("~/Views/Shared/_InitialSelection.cshtml");
            //D:\P2b Ultimate source\With Svn\Latest\Bhavnagar\P2bUltimate\P2BUltimate\Views\Shared\_InitialSelection.cshtml
        }
        public ActionResult GetFinancialYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var data = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR").Select(a => new { Id = a.Id, Name = a.FullDetails }).ToList();
                var selected = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).Select(a => new { Id = a.Id }).SingleOrDefault();

                SelectList s = new SelectList(data, "Id", "Name", selected.Id);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeaveYear()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var data = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "LEAVECALENDAR").Select(a => new { Id = a.Id, Name = a.FullDetails }).ToList();
                var selected = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && a.Default == true).Select(a => new { Id = a.Id }).SingleOrDefault();
                int Id = 0;
                if (selected != null)
                {
                    Id = selected.Id;
                }
                SelectList s = new SelectList(data, "Id", "Name", Id);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Update(FormCollection from)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fy = from["Fy_id"] != null ? Convert.ToInt32(from["Fy_id"]) : 0;
                var Ly = from["Ly_id"] != null ? Convert.ToInt32(from["Ly_id"]) : 0;
                if (fy != 0)
                {
                    SessionManager.FinancialYear = fy.ToString();
                }
                if (Ly != 0)
                {
                    SessionManager.LeaveYear = Ly.ToString();
                }
                return Json(new { success = true, responseText = "", data = Url.Action("index", "Login") }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetFyLy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string fy = "";
                string ly = "";
                int fy_id = Convert.ToInt32(SessionManager.FinancialYear);
                Calendar fy_st = db.Calendar.Where(e => e.Id == fy_id).FirstOrDefault();
                if (fy_st != null)
                {
                    fy = fy_st.FromDate.Value.ToShortDateString() + " to " +
                    fy_st.ToDate.Value.ToShortDateString();
                }
             

                int ly_id = Convert.ToInt32(SessionManager.LeaveYear);
                Calendar ly_st = db.Calendar.Where(e => e.Id == ly_id).FirstOrDefault();

                if (ly_st != null)
                {
                    ly = ly_st.FromDate.Value.ToShortDateString() + " to " +
                    ly_st.ToDate.Value.ToShortDateString();
                }
                 
                return Json(new { fy, ly }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}