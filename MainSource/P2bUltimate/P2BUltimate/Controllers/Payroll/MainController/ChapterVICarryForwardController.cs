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
    public class ChapterVICarryForwardController : Controller
    {
        //
        // GET: /ChapterVICarryForward/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ChapterVICarryForward/Index.cshtml");
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
        public ActionResult AddCarryForwadold(string OldFinanacial_id, string NewFinancial_id, string ItInvestmentList)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> ItInvestmentids = null;
                if (ItInvestmentList != null && ItInvestmentList != "")
                {
                    ItInvestmentids = one_ids(ItInvestmentList);
                }

                int NewFinanceCalendarid = Convert.ToInt32(NewFinancial_id);
                int OldFinanceCalendarid = Convert.ToInt32(OldFinanacial_id);
               // var CheckExistingdata = db.ITInvestmentPayment.Where(e => e.FinancialYear.Id == NewFinanceCalendarid).ToList();
                var defaultyear = db.Calendar.Where(e => e.Id == NewFinanceCalendarid).FirstOrDefault();
                foreach (var item in ItInvestmentids)
                {
                    var CheckExistingdata = db.ITInvestmentPayment.Include(e => e.ITInvestment).Where(e => e.ITInvestment.Id == item && e.FinancialYear.Id == NewFinanceCalendarid).ToList();
                    if (CheckExistingdata.Count() > 0)
                    {
                        return this.Json(new { success = true, responseText = "Data already carry forwarded for This Investment " + CheckExistingdata.FirstOrDefault().ITInvestment.ITInvestmentName + "Name", JsonRequestBehavior.AllowGet });
                    }
                }
                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                //                   new System.TimeSpan(0, 30, 0)))
                //{
                    var EmpList = db.EmployeePayroll
                        .Include(e => e.ITInvestmentPayment)
                        .Include(e => e.ITInvestmentPayment.Select(x => x.ITSection))
                        .Include(e => e.ITInvestmentPayment.Select(x => x.FinancialYear))
                        .Include(e => e.ITInvestmentPayment.Select(x => x.ITInvestment))
                        .Include(e => e.ITInvestmentPayment.Select(x => x.ITInvestment.SalaryHead))
                        .Include(e => e.Employee)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();

                    try
                    {
                        foreach (var i in EmpList)
                        {
                            var CheckItisLinkForSalaryhead = i.ITInvestmentPayment.Where(e => e.ITInvestment != null && ItInvestmentids.Contains(e.ITInvestment.Id) && e.ITInvestment.SalaryHead == null && e.FinancialYear.Id == OldFinanceCalendarid).ToList();
                            foreach (var j in CheckItisLinkForSalaryhead)
                            {
                                //var PrevMonth = i.SalaryT.OrderByDescending(e => e.Id).FirstOrDefault();
                                //PrevDate = Convert.ToDateTime("01/" + PrevMonth.PayMonth);
                                //var PrevFuncAtt = i.FunctAttendanceT.Where(e => e.PayMonth == PrevMonth.PayMonth).ToList();
                                //Month = PrevDate.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevDate.AddMonths(1).Month.ToString() : PrevDate.AddMonths(1).Month.ToString();
                                //CurMonth = Month + "/" + PrevDate.AddMonths(1).Year.ToString();

                                //if (i.Employee.ServiceBookDates.ServiceLastDate == null || i.Employee.ServiceBookDates.ServiceLastDate != null && Convert.ToDateTime("01/" + i.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + CurMonth))
                                //{
                                //EmpSalStruct SalStruct = i.EmpSalStruct.Where(e => e.EffectiveDate.Value.ToString("MM/yyyy") == CurMonth).OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                                //DateTime ToDate = Convert.ToDateTime(DateTime.DaysInMonth(SalStruct.EffectiveDate.Value.Year, SalStruct.EffectiveDate.Value.Month) + "/" + CurMonth);
                                //if (SalStruct != null)
                                //{
                                //foreach (var F in PrevFuncAtt)
                                //{
                                DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                ITInvestmentPayment FuncAtt = new ITInvestmentPayment()
                                {
                                    DBTrack = DBTrack,
                                    FinancialYear = defaultyear,
                                    ITSection = j.ITSection,
                                    ITInvestment = j.ITInvestment,
                                    LoanAdvanceHead = j.LoanAdvanceHead,
                                    Narration = j.Narration,
                                    ActualInvestment = j.ActualInvestment,
                                    DeclaredInvestment = j.DeclaredInvestment,
                                    InvestmentDate = defaultyear.FromDate,
                                    EmployeePayroll_Id = j.EmployeePayroll_Id
                                };
                                db.ITInvestmentPayment.Add(FuncAtt);
                                db.SaveChanges();

                                List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                                OFAT.Add(db.ITInvestmentPayment.Find(FuncAtt.Id));

                                var aa = db.EmployeePayroll.Find(i.Id);
                                OFAT.AddRange(aa.ITInvestmentPayment);
                                aa.ITInvestmentPayment = OFAT;
                                //OEmployeePayroll.DBTrack = dbt;

                                db.EmployeePayroll.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                // }
                                // }
                                // }
                                //break;
                            }

                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                //    ts.Complete();
                //}

                return this.Json(new { success = true, responseText = "Data carry forwarded for month :", JsonRequestBehavior.AllowGet });
            }
        }
        public ActionResult AddCarryForwad(string OldFinanacial_id, string NewFinancial_id, string ItInvestmentList)
        {
            List<string> Msg = new List<string>();

            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    List<int> ItInvestmentids = null;
                    if (ItInvestmentList != null && ItInvestmentList != "")
                    {
                        ItInvestmentids = one_ids(ItInvestmentList);
                    }

                    int NewFinanceCalendarid = Convert.ToInt32(NewFinancial_id);
                    int OldFinanceCalendarid = Convert.ToInt32(OldFinanacial_id);
                   // var CheckExistingdata = db.ITInvestmentPayment.Where(e => e.FinancialYear.Id == NewFinanceCalendarid).ToList();
                    var defaultyear = db.Calendar.Where(e => e.Id == NewFinanceCalendarid).FirstOrDefault();
                    //if (CheckExistingdata.Count() > 0)
                    //{
                    //    return this.Json(new { success = true, responseText = "Data already carry forwarded for This" + defaultyear.FromDate.Value.ToShortDateString() + "To" + defaultyear.ToDate.Value.ToShortDateString() + "Year", JsonRequestBehavior.AllowGet });
                    //}
                    foreach (var item in ItInvestmentids)
                    {
                        var CheckExistingdata = db.ITInvestmentPayment.Include(e => e.ITInvestment).Where(e => e.ITInvestment.Id == item && e.FinancialYear.Id == NewFinanceCalendarid).ToList();
                        if (CheckExistingdata.Count() > 0)
                        {
                            return this.Json(new { success = true, responseText = "Data already carry forwarded for This Investment " + CheckExistingdata.FirstOrDefault().ITInvestment.ITInvestmentName + "Name", JsonRequestBehavior.AllowGet });
                        }
                    }

                    //var TEST = db.ITaxTransT.Include(e => e.EmployeePayroll.Employee.ServiceBookDates)
                    //  .Where(e => e.PayMonth == PrevSal_string ).AsNoTracking().ToList();


                    var TEST = db.ITInvestmentPayment
                        .Include(e => e.ITSection)
                        .Include(e => e.FinancialYear)
                        .Include(e => e.ITInvestment)
                        .Include(e => e.ITInvestment.SalaryHead)
                        .Include(e => e.EmployeePayroll.Employee.ServiceBookDates)
                        .Where(e => e.ITInvestment != null && ItInvestmentids.Contains(e.ITInvestment.Id) && e.ITInvestment.SalaryHead == null && e.FinancialYear.Id == OldFinanceCalendarid).ToList();
                    foreach (var i in TEST)
                    {
                        //  var PrevITtaxTransT = i.ITaxTransT.Where(e => e.PayMonth == PrevSal_string).FirstOrDefault();

                        if (i.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null )
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                              new System.TimeSpan(0, 30, 0)))
                            {

                                DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                ITInvestmentPayment FuncAtt = new ITInvestmentPayment()
                                {
                                    DBTrack = DBTrack,
                                    FinancialYear = defaultyear,
                                    ITSection = i.ITSection,
                                    ITInvestment = i.ITInvestment,
                                    LoanAdvanceHead = i.LoanAdvanceHead,
                                    Narration = i.Narration,
                                    ActualInvestment = i.ActualInvestment,
                                    DeclaredInvestment = i.DeclaredInvestment,
                                    InvestmentDate = defaultyear.FromDate,
                                    EmployeePayroll_Id = i.EmployeePayroll_Id
                                };
                                db.ITInvestmentPayment.Add(FuncAtt);
                                db.SaveChanges();


                                List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                                OFAT.Add(db.ITInvestmentPayment.Find(FuncAtt.Id));


                                var aa = db.EmployeePayroll.Where(e => e.Id == i.EmployeePayroll.Id).Include(e => e.ITInvestmentPayment).FirstOrDefault();

                                OFAT.AddRange(aa.ITInvestmentPayment);
                                aa.ITInvestmentPayment = OFAT;

                                db.EmployeePayroll.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                ts.Complete();
                            }

                        }

                    }
                    //    Msg.Add(" Data already carry forwarded for month =" + CurMonth);
                    return Json(new Utility.JsonReturnClass { success = true, responseText = " Data carry forwarded for :" + defaultyear.FromDate.Value.ToShortDateString() + "To" + defaultyear.ToDate.Value.ToShortDateString() + "Year" }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { success = true, responseText = "Data carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
                }
            }
            catch (Exception ex)
            {
                LogFile Logfile = new LogFile();
                ErrorLog Err = new ErrorLog()
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    ExceptionMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace,
                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);
                Msg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Get_ItInvestment(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITInvestment.Where(e => e.SalaryHead == null).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITInvestment.Where(e => a != e.Id && e.SalaryHead == null).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

    }
}