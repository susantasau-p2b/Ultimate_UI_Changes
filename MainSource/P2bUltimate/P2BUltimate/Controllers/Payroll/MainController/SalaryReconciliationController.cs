using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class SalaryReconciliationController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {

            return View("~/Views/Payroll/MainViews/SalaryReconciliation/Index.cshtml");
        }
        public ActionResult SaveProcess1(string typeofbtn, string month)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                ////delete record
                var OProcessDataSumDel = db.SalaryReconcilation.Where(e => e.CurrentMonth == month).ToList();

                if (OProcessDataSumDel != null && OProcessDataSumDel.Count() > 0)
                {
                    db.SalaryReconcilation.RemoveRange(OProcessDataSumDel);
                    db.SaveChanges();
                }
                ////delete record end

                string prepaymonth = Convert.ToDateTime("01/" + month).AddMonths(-1).ToString("MM/yyyy");

                SalaryReconciliationProcess.SalReconciliation(month, prepaymonth);

                Msg.Add("Salary Reconciliation processed.");
                watch.Stop();
                Utility.DumpProcessStatus(watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + ":" + watch.Elapsed.Milliseconds);
            }
            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }


  
        public string GetPrevMonth(string data)
        {
            DateTime CurDate = Convert.ToDateTime("01/" + data);
            DateTime PrevDate = CurDate.AddMonths(-1);
            string PrevMonth = PrevDate.ToString("MM/yyyy");
            return PrevMonth;
        }
    }
}