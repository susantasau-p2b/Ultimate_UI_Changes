using P2b.Global;
using P2BUltimate.Models;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using P2BUltimate.App_Start;
using P2BUltimate.Security;
using Newtonsoft.Json;
using P2BUltimate.Process;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class Itsection24PaymentCarryForwardController : Controller
    {
        //
        // GET: /Itsection24PaymentCarryForward/
        #region start
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/Itsection24PaymentCarryForward/Index.cshtml");
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult LoadOldFinanCialYear(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default != true).ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult LoadNewFinanCialYear(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult AddCarryForwad(string OldFinanacial_id, string NewFinancial_id, string LoanAdvanceHeadList)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> loanAdvanceheadids = null;
                if (LoanAdvanceHeadList != null && LoanAdvanceHeadList != "")
                {
                    loanAdvanceheadids = one_ids(LoanAdvanceHeadList);
                }

                int NewFinanceCalendarid = Convert.ToInt32(NewFinancial_id);
                int OldFinanceCalendarid = Convert.ToInt32(OldFinanacial_id);
                //var CheckExistingdata = db.ITInvestmentPayment.Where(e => e.FinancialYear.Id == NewFinanceCalendarid).ToList();
                var defaultyear = db.Calendar.Where(e => e.Id == NewFinanceCalendarid).FirstOrDefault();
                foreach (var item in loanAdvanceheadids)
                {
                    var CheckExistingdata = db.ITSection24Payment.Include(e => e.LoanAdvanceHead).Where(e => e.LoanAdvanceHead.Id == item && e.FinancialYear.Id == NewFinanceCalendarid).ToList();
                    if (CheckExistingdata.Count() > 0)
                    {
                        return this.Json(new { success = true, responseText = "Data already carry forwarded for This LoanAdvancehead " + CheckExistingdata.FirstOrDefault().LoanAdvanceHead.Name + "Name", JsonRequestBehavior.AllowGet });
                    }
                }
                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                //                   new System.TimeSpan(0, 30, 0)))
                //{
                var EmpList = db.EmployeePayroll
                    .Include(e => e.ITSection24Payment)
                    .Include(e => e.ITSection24Payment.Select(x => x.ITSection))
                    .Include(e => e.ITSection24Payment.Select(x => x.FinancialYear))
                    .Include(e => e.ITSection24Payment.Select(x => x.LoanAdvanceHead))
                    .Include(e => e.ITSection24Payment.Select(x => x.LoanAdvanceHead.SalaryHead))
                    .Include(e => e.Employee)
                    .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                    .ToList();

                try
                {
                    foreach (var i in EmpList)
                    {
                        var CheckItisLinkForSalaryhead = i.ITSection24Payment.Where(e => e.LoanAdvanceHead != null && loanAdvanceheadids.Contains(e.LoanAdvanceHead.Id) && e.LoanAdvanceHead.SalaryHead == null && e.FinancialYear.Id == OldFinanceCalendarid).ToList();
                        foreach (var j in CheckItisLinkForSalaryhead)
                        {
                            DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ITSection24Payment FuncAtt = new ITSection24Payment()
                            {
                                DBTrack = DBTrack,
                                FinancialYear = defaultyear,
                                ITSection = j.ITSection,
                                LoanAdvanceHead = j.LoanAdvanceHead,
                                Narration = j.Narration,
                                ActualInterest = j.ActualInterest,
                                DeclaredInterest = j.DeclaredInterest,
                                InvestmentDate = defaultyear.FromDate,
                                PaymentName = j.PaymentName
                                // EmployeePayroll_Id = j.EmployeePayroll_Id
                            };
                            db.ITSection24Payment.Add(FuncAtt);
                            db.SaveChanges();

                            List<ITSection24Payment> OFAT = new List<ITSection24Payment>();
                            OFAT.Add(db.ITSection24Payment.Find(FuncAtt.Id));

                            var aa = db.EmployeePayroll.Find(i.Id);
                            OFAT.AddRange(aa.ITSection24Payment);
                            aa.ITSection24Payment = OFAT;
                            db.EmployeePayroll.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                        }
                        // }
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                //    ts.Complete();
                //}

                return this.Json(new { success = true, responseText = "Data carry forwarded for  :" + defaultyear.FromDate.Value.ToShortDateString() + "To" + defaultyear.ToDate.Value.ToShortDateString() + "Year", JsonRequestBehavior.AllowGet });
            }
        }
        //public ActionResult AddCarryForwad(string OldFinanacial_id, string NewFinancial_id, string ItInvestmentList)
        //{
        //    List<string> Msg = new List<string>();

        //    try
        //    {
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            List<int> ItInvestmentids = null;
        //            if (ItInvestmentList != null && ItInvestmentList != "")
        //            {
        //                ItInvestmentids = one_ids(ItInvestmentList);
        //            }

        //            int NewFinanceCalendarid = Convert.ToInt32(NewFinancial_id);
        //            int OldFinanceCalendarid = Convert.ToInt32(OldFinanacial_id);
        //            var CheckExistingdata = db.ITInvestmentPayment.Where(e => e.FinancialYear.Id == NewFinanceCalendarid).ToList();
        //            var defaultyear = db.Calendar.Where(e => e.Id == NewFinanceCalendarid).FirstOrDefault();
        //            if (CheckExistingdata.Count() > 0)
        //            {
        //                return this.Json(new { success = true, responseText = "Data already carry forwarded for This" + defaultyear.FromDate.Value.ToShortDateString() + "To" + defaultyear.ToDate.Value.ToShortDateString() + "Year", JsonRequestBehavior.AllowGet });
        //            }


        //            //var TEST = db.ITaxTransT.Include(e => e.EmployeePayroll.Employee.ServiceBookDates)
        //            //  .Where(e => e.PayMonth == PrevSal_string ).AsNoTracking().ToList();


        //            var TEST = db.ITInvestmentPayment
        //                .Include(e => e.ITSection)
        //                .Include(e => e.FinancialYear)
        //                .Include(e => e.ITInvestment)
        //                .Include(e => e.ITInvestment.SalaryHead)
        //                .Include(e => e.EmployeePayroll.Employee.ServiceBookDates)
        //                .Where(e => e.ITInvestment != null && ItInvestmentids.Contains(e.ITInvestment.Id) && e.ITInvestment.SalaryHead == null && e.FinancialYear.Id == OldFinanceCalendarid).ToList();
        //            foreach (var i in TEST)
        //            {
        //                //  var PrevITtaxTransT = i.ITaxTransT.Where(e => e.PayMonth == PrevSal_string).FirstOrDefault();

        //                if (i.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null )
        //                {

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                      new System.TimeSpan(0, 30, 0)))
        //                    {

        //                        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                        ITInvestmentPayment FuncAtt = new ITInvestmentPayment()
        //                        {
        //                            DBTrack = DBTrack,
        //                            FinancialYear = defaultyear,
        //                            ITSection = i.ITSection,
        //                            ITInvestment = i.ITInvestment,
        //                            LoanAdvanceHead = i.LoanAdvanceHead,
        //                            Narration = i.Narration,
        //                            ActualInvestment = i.ActualInvestment,
        //                            DeclaredInvestment = i.DeclaredInvestment,
        //                            InvestmentDate = defaultyear.FromDate,
        //                            EmployeePayroll_Id = i.EmployeePayroll_Id
        //                        };
        //                        db.ITInvestmentPayment.Add(FuncAtt);
        //                        db.SaveChanges();


        //                        List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
        //                        OFAT.Add(db.ITInvestmentPayment.Find(FuncAtt.Id));


        //                        var aa = db.EmployeePayroll.Where(e => e.Id == i.EmployeePayroll.Id).Include(e => e.ITInvestmentPayment).FirstOrDefault();

        //                        OFAT.AddRange(aa.ITInvestmentPayment);
        //                        aa.ITInvestmentPayment = OFAT;

        //                        db.EmployeePayroll.Attach(aa);
        //                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();

        //                        ts.Complete();
        //                    }

        //                }

        //            }
        //            //    Msg.Add(" Data already carry forwarded for month =" + CurMonth);
        //            return Json(new Utility.JsonReturnClass { success = true, responseText = " Data carry forwarded for :" + defaultyear.FromDate.Value.ToShortDateString() + "To" + defaultyear.ToDate.Value.ToShortDateString() + "Year" }, JsonRequestBehavior.AllowGet);
        //            //return this.Json(new { success = true, responseText = "Data carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }

        //}

        public ActionResult Get_LoanAdvancedHead(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LoanAdvanceHead.Where(e => e.SalaryHead == null).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvanceHead.Where(e => a != e.Id && e.SalaryHead == null).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion start
    }
}