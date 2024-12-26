using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;
using System.Web.Script.Serialization;
using System.Collections.Specialized;
using System.IO;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class SalaryTController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SalaryT/Index.cshtml");
        }
        public ActionResult Negativepartial()
        {
            return View("~/Views/Shared/Payroll/_Negativepartial.cshtml");
        }
        public ActionResult Negpartial()
        {
            return View("~/Views/Shared/Payroll/_Negpartial.cshtml");
        }
        public ActionResult Suspendedpartial()
        {
            return View("~/Views/Shared/Payroll/_Suspendedpartial.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_NegativeSal.cshtml");

        }
        public ActionResult IsPayslipGenrated()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var paymonth = db.SalaryT.OrderByDescending(a => a.Id).FirstOrDefault().PayMonth;
                var payslipChk = db.PaySlipR.Where(a => a.PayMonth == paymonth).ToList();
                if (payslipChk != null && payslipChk.Count > 0)
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }
            }
        }
        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                //List<SalaryT> query = db.EmployeePayroll.SelectMany(r => r.SalaryT).ToList();
                //var a = query.Where(t => t.PayMonth == month).ToList();
                //if (a.Count > 0)
                //{
                //    selected = true;
                //}
                var Emplist = db.SalaryT.Where(r => r.PayMonth == month).Select(r => r.EmployeePayroll_Id).ToList();
                foreach (var emp in Emplist)
                {
                    var EmpExist = 0;
                    EmpExist = db.EmployeePayroll.Where(e => e.Id == emp).Select(e => e.Id).FirstOrDefault();
                    if (EmpExist != 0)
                    {
                        selected = true;
                        break;
                    }
                };
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }




        public class ProcType
        {
            public int Id { get; set; }
            public string Text { get; set; }
        };

        public ActionResult Polulate_ProcTypeChk(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    List<ProcType> a = new List<ProcType>();
                    if (data.ToString().Split('/')[0] == "03")
                    {
                        a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" }
                    };
                    }
                    else
                    {
                        a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" },
                        new ProcType() { Id = 1, Text = "Declare Investment & Projected Income" },
                        new ProcType() { Id = 2, Text = "Actual Investment & Projected Income" }
                    };
                    }

                    SelectList s = new SelectList(a, "Id", "Text");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public EmployeePayroll _returnEmployeePayrollOne(Int32 empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeePayroll oEmployeePayroll = db.EmployeePayroll
                                   .Include(e => e.Employee.EmpName)
                                   .Include(e => e.Employee.ServiceBookDates)
                                   .Where(e => e.Id == empid)
                                   .FirstOrDefault();
                return oEmployeePayroll;
            }
        }

        public ActionResult ValidateForm(Calendar c, FormCollection form)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
                string PayMonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
                string AmountList = "";
                string ProcTypeList = form["ProcTypeList"] == "" ? "" : form["ProcTypeList"];

                List<string> Msg = new List<string>();
                bool AutoIncomeTax = false;
                try
                {
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    if (form["lblManualIT"] == "1")
                        AutoIncomeTax = false;
                    else if (form["lblManualIT"] == "2")
                        AutoIncomeTax = true;
                    else if (form["lblManualIT"] == "3")
                        AutoIncomeTax = false;

                    //Employee OEmployee = null;
                    //EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;
                    OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Company.Id == CompId).SingleOrDefault();
                    var OFinancia = OCompanyPayroll.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();

                    ids = db.Employee.Where(e => ids.Contains(e.Id)).Select(r => r.Id).ToList();
                    int ProcType = 0;
                    if (AutoIncomeTax == true)
                    {

                        List<EmployeePayroll> emp_list = new List<EmployeePayroll>();
                        List<int> HRAt = new List<int>();
                        List<int> ITSection10Payment = new List<int>();


                        int fyyear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;

                        var HRAt1 = db.EmployeePayroll
                        .Where(t => t.HRATransT.Count() > 0 && ids.Contains(t.Employee.Id)).Select(e => new
                        {
                            HRATransT = e.HRATransT.Where(s => s.Financialyear.Id == fyyear).ToList(),
                            Id = e.Id,
                        }).ToList();

                        HRAt = HRAt1.Select(e => e.Id).ToList();

                        var ITSection10Payment1 = db.EmployeePayroll
                           .Where(t => t.ITSection10Payment.Count() > 0 && ids.Contains(t.Employee.Id)).Select(e => new
                           {
                               ITSection10Payment = e.ITSection10Payment.Where(s => s.FinancialYear.Id == fyyear).ToList(),
                               Id = e.Id,
                           }).ToList();


                        ITSection10Payment = ITSection10Payment1.Select(e => e.Id).ToList();

                        var list2 = HRAt.Except(ITSection10Payment);

                        if (list2.Count() > 0)
                        {
                            foreach (var EmpId in list2)
                            {
                                EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpId);

                                emp_list.Add(EmpPay);
                                Msg.Add(EmpPay.Employee.FullDetails + ", HRA rent is entered and ITSection10Payment of this employee is not entered ,Please enter ITSection10Payment and then process salary ,");

                            }
                        }

                        if (Msg.Count() > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }


                        if (ProcTypeList != "")
                        {
                            ProcType = Convert.ToInt32(ProcTypeList);
                            if (ProcType == 0)
                                AmountList = "Actual";
                            if (ProcType == 1)
                                AmountList = "Declared";
                            if (ProcType == 2)
                                AmountList = "Actual";
                        }
                        else
                        {
                            Msg.Add(" Kindly select Tax Calculation method  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    SessionManager.CheckProcessStatus = "Salary Gen";
                    //return null;
                    //int ErrNo = SalaryGen.EmployeePayrollProcess(OEmployeePayroll.Id, PayMonth, AutoIncomeTax, OCompanyPayroll.Id, AmountList, ProcType);
                    Msg = SalaryGen.PreSalCheck(0, PayMonth, AutoIncomeTax, OCompanyPayroll.Id, AmountList, ProcType, ids);
                    if (Msg.Count() == 0)
                    {
                        Msg = SalaryGen.SalaryProcess(0, PayMonth, AutoIncomeTax, OCompanyPayroll.Id, AmountList, ProcType, ids);

                        var element = Msg.Where(x => x.Contains("--Err")).FirstOrDefault();

                        if (element != null)
                        {
                            
                            SessionManager.CheckProcessStatus = "";
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                        }

                        // CheckProcessStatus OCheckProcessStatus = db.CheckProcessStatus.SingleOrDefault();
                        //DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                        //CheckProcessStatus OCheckProcessStatus = new CheckProcessStatus()
                        //{
                        //    //ProcessId = "1011",
                        //    ProcessName = "SalaryGen",
                        //    ProcessStatus = 0,
                        //    NoOfRows = 0,
                        //    //ProcessParameters = "PayMonth=" + PayMonth + "; AutoIncomeTax = " + AutoIncomeTax + "; AmountList=" + AmountList + "; ProcType=" + ProcType + "; ids=" + Emp,
                        //    //ProcessParameterValues =  PayMonth + "," + AutoIncomeTax + "," + OCompanyPayroll.Id + "," + AmountList + "," + ProcType + "," + ids,
                        //    StartDate = DateTime.Now.Date,
                        //    DBTrack = dbt
                        //};
                        //db.CheckProcessStatus.Add(OCheckProcessStatus);
                        //db.SaveChanges();
                    }
                    else
                    {
                        SessionManager.CheckProcessStatus = "";
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                }
                catch (Exception ex)
                {
                    SessionManager.CheckProcessStatus = "";
                    Utility.DumpProcessStatus(ex.Message);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                   // throw;

                    //LogFile Logfile = new LogFile();
                    //ErrorLog Err = new ErrorLog()
                    //{
                    //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    //    ExceptionMessage = ex.Message,
                    //    ExceptionStackTrace = ex.StackTrace,
                    //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    //    LogTime = DateTime.Now
                    //};
                    //throw;
                    ////return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                SessionManager.CheckProcessStatus = "";
                watch.Stop();
                Utility.DumpProcessStatus(watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + ":" + watch.Elapsed.Milliseconds);
                Msg.Add("Salary generation process over.");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Polulate_AmountChk(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    List<string> a = new List<string>();
                    if (data.ToString().Split('/')[0] == "03")
                    {
                        a.Add("Actual");
                    }
                    else
                    {
                        a.Add("Actual");
                        a.Add("Declared");
                    }

                    SelectList s = new SelectList(a);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Polulate_PayProcessGroup(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.Company.Include(e => e.PayProcessGroup).Include(e => e.Bank).Where(e => e.Id == CompId).SingleOrDefault();
                    var selected = (Object)null;
                    if (query.Code == "KB")
                    {
                        selected = query.Bank.Select(e => e.Id).ToList();
                    }
                    else
                    {
                        selected = query.PayProcessGroup.Select(e => e.Id).FirstOrDefault();
                    }

                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(query.Bank, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.PayProcessGroup.Find(int.Parse(data));
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<int> ids = null; try
                {
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }
                    else
                    {
                        Msg.Add("  Kindly Select Employee.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    List<SalaryT> salaryt = new List<SalaryT>();
                    foreach (var i in ids)
                    {
                        // OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Employee.Id == i).SingleOrDefault();
                        SalaryT SalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                        if (SalT != null)
                        {
                            salaryt.Add(SalT);
                            //SalT.ReleaseDate = DateTime.Now.Date;
                            //db.SalaryT.Attach(SalT);
                            //db.Entry(SalT).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //TempData["RowVersion"] = SalT.RowVersion;
                            //db.Entry(SalT).State = System.Data.Entity.EntityState.Detached;
                        }
                        //ts.Complete();
                    }
                    if (salaryt.Count > 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {

                            salaryt.ForEach(q => q.ReleaseDate = DateTime.Now.Date);
                            // db.Entry;
                            //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            // TempData["RowVersion"] = SalT.RowVersion;
                            //  db.Entry(salaryt).State = System.Data.Entity.EntityState.Detached;
                        }
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
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true, responseText = "Salary released for employee." }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LoadEmpByDefault(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                if (data != "" && data != null)
                {
                    dt = Convert.ToDateTime("01/" + data).AddMonths(-1);
                }
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.CompanyPayroll.Where(e => e.Company.Id == CompId).Include(e => e.EmployeePayroll).Include(a => a.EmployeePayroll.Select(e => e.Employee)).SingleOrDefault();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    foreach (var ca in query.EmployeePayroll.ToList())
                    {
                        if (ca.Employee.ServiceBookDates.ServiceLastDate.Value.Month == dt.Value.Month && ca.Employee.ServiceBookDates.ServiceLastDate.Value.Year == dt.Value.Year)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Employee.Id.ToString(),
                                value = ca.Employee.FullDetails,
                            });
                        }
                    }
                    var jsondata = new
                    {
                        tablename = "Employee-Table",
                        data = returndata,
                    };
                    return Json(jsondata, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteProcess(string forwardata, string PayMonth)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg1 = new List<string>();
                try
                {

                    List<int> ids = null;
                    // List<SalaryT> salaryt = new List<SalaryT>();

                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }

                    EmployeePayroll OEmployeePayroll = null;

                    foreach (var i in ids)
                    {
                        // OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();
                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee)
                        .Where(e => e.Employee.Id == i).AsNoTracking().SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {
                            //using (DataBaseContext db2 = new DataBaseContext())
                            //{
                            var SalT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Id == OEmployeePayroll.Id)
                                .Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.NegSalData))
                                .SingleOrDefault();
                            if (SalT.ReleaseDate != null)
                            {
                                Msg1.Add("Salary Released For Employee Code=" + OEmployeePayroll.Employee.EmpCode + ",Unable To Delete" + "\n");
                                continue;
                                //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //  return Json(new { success = true, responseText = "Salary released for employee " + OEmployeePayroll.Employee.EmpCode + ". Unable To Delete" }, JsonRequestBehavior.AllowGet);
                            }
                            if (SalT.SalEarnDedT.Where(r => r.NegSalData != null && r.NegSalData.ReleaseFlag == true).Count() > 0)
                            {
                                Msg1.Add("Salary has been changed For Employee Code=" + OEmployeePayroll.Employee.EmpCode + ",Unable To Delete" + "\n");
                                continue;
                            }
                            if (SalT != null)
                            {
                                //     salaryt.Add(SalT);
                                //  SalaryGen.DeleteSalary(SalT.Id, PayMonth);
                                SalaryGen.DeleteSalaryList(SalT.Id, PayMonth);
                            }
                            //}
                            ts.Complete();

                        }

                    }
                }
                catch (Exception ex)
                {
                    //  Msg.Add(ex.Message);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                Msg1.Add("Salary deleted for selected employee." + "\n");
                watch.Stop();
                Utility.DumpProcessStatus(watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + ":" + watch.Elapsed.Milliseconds);
                //return Json(new { success = true, responseText = Msg1 }, JsonRequestBehavior.AllowGet);
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg1 }, JsonRequestBehavior.AllowGet);
            }
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public double TotalEarning { get; set; }
            public double TotalDeduction { get; set; }
            public double TotalNet { get; set; }
            public string ReleaseDate { get; set; }
        }
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> SalaryList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";
                    string Month = "";
                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    else
                    {
                        if (DateTime.Now.Date.Month < 10)
                            Month = "0" + DateTime.Now.Date.Month;
                        else
                            Month = DateTime.Now.Date.Month.ToString();
                        PayMonth = Month + "/" + DateTime.Now.Date.Year;
                    }

                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT.Where(q => q.PayMonth == PayMonth)).AsNoTracking().AsParallel().ToList();
                    // var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT).AsNoTracking().AsParallel().ToList();
                    var tt = db.SalaryT.Where(e => e.PayMonth == PayMonth).Select(r => r.EmployeePayroll_Id).ToList();
                    var BindEmpList = db.EmployeePayroll.Where(e => tt.Contains(e.Id)).Select(e => new { Employee = e.Employee, SalaryT = e.SalaryT.Where(x => x.PayMonth == PayMonth).FirstOrDefault(), EmpName = e.Employee.EmpName }).ToList();

                    foreach (var z in BindEmpList)
                    {
                        //var chkk = z.SalaryT.Where(q => q.PayMonth == PayMonth).SingleOrDefault();
                        //if (chkk != null)
                        //{
                        //foreach (var Sal in z.SalaryT)
                        //{
                        //    if (Sal.PayMonth == PayMonth)
                        //    {
                        view = new P2BGridData()
                        {
                            Id = z.Employee.Id,
                            Code = z.Employee.EmpCode,
                            Name = z.Employee.EmpName.FullNameFML,
                            PayMonth = z.SalaryT.PayMonth,
                            TotalEarning = z.SalaryT.TotalEarning,
                            TotalDeduction = z.SalaryT.TotalDeduction,
                            TotalNet = z.SalaryT.TotalNet,
                            ReleaseDate = z.SalaryT.ReleaseDate == null ? "" : z.SalaryT.ReleaseDate.Value.ToShortDateString()
                        };
                        model.Add(view);
                        //    }
                        //}
                        // }

                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                  || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.PayMonth.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.TotalEarning.ToString().Contains(gp.searchString))
                                  || (e.TotalDeduction.ToString().Contains(gp.searchString))
                                  || (e.TotalNet.ToString().Contains(gp.searchString))
                                  || (e.ReleaseDate.ToString().Contains(gp.searchString))
                                  || (e.Id.ToString().Contains(gp.searchString))
                                  )
                              .Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                             gp.sidx == "TotalEarning" ? c.TotalEarning.ToString() :
                                             gp.sidx == "TotalDeduction" ? c.TotalDeduction.ToString() :
                                             gp.sidx == "TotalNet" ? c.TotalDeduction.ToString() :
                                             gp.sidx == "ReleaseDate" ? c.ReleaseDate.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet, a.ReleaseDate, a.Id }).ToList();
                        }
                        totalRecords = SalaryList.Count();
                    }
                    if (totalRecords > 0)
                    {
                        totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                    }
                    if (gp.page > totalPages)
                    {
                        gp.page = totalPages;
                    }
                    var JsonData = new
                    {
                        page = gp.page,
                        rows = jsonData,
                        records = totalRecords,
                        total = totalPages
                    };
                    return Json(JsonData, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public class empdetails
        {
            public string ded { get; set; }
            public string amount { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Amount { get; set; }
            public int SalaryTId { get; set; }
        }
        public class empdetails1
        {
            public int Id { get; set; }
            public string earn { get; set; }
            public string amt { get; set; }
        }
        public ActionResult getdata(FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            //"Autho_Action=&Autho_Allow=&txtPayMonth1=06%2F2019&employee-table1=36"
            var NewObj = new NameValueCollection();
            var employeeids = (String)null;
            var PayMonth = (String)null;

            NewObj = HttpUtility.ParseQueryString(data);
            if (NewObj != null)
            {
                employeeids = NewObj["employee-table1"];
                PayMonth = NewObj["txtPayMonth1"];
            }
            if (employeeids == null && PayMonth == null)
            {
                Msg.Add(" Kindly select employee  ");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            using (DataBaseContext db = new DataBaseContext())
            {
                // List<string> Msg = new List<string>();
                //string Emp = form["employee-table1"] == "0" ? "" : form["employee-table1"];
                //string PayMonth = form["txtPayMonth1"] == "0" ? "" : form["txtPayMonth1"];

                int ids = Convert.ToInt32(employeeids);
                //if (Emp != null && Emp != "0" && Emp != "false")
                //{
                //    ids = int.Parse(Emp);
                //}
                //else
                //{

                //    Msg.Add(" Kindly select employee  ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}

                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                CompanyPayroll OCompanyPayroll = null;



                OEmployee = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                   .Where(r => r.Id == ids).SingleOrDefault();

                OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Employee.Id == ids).SingleOrDefault();

                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                var SalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                var Empnm = (OEmployee.EmpCode + " - " + OEmployee.EmpName.FName);

                //var sal = db.EmployeePayroll.Include(e => e.SalaryT).Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                //  .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead)))
                //    .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.Type)))
                //     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.SalHeadOperationType)))
                //  .Include(e => e.Employee)
                //                     .Where(e => e.Employee.Id == OEmployee.Id)
                //                         .SingleOrDefault();
                //var b = sal.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                var b = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.SalaryHead))
                    .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.Type)).Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType))
                    .Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.PayMonth == PayMonth).FirstOrDefault();

                List<empdetails1> p = new List<empdetails1>();
                {
                    foreach (var item in b.SalEarnDedT.Where(c => c.SalaryHead.Type.LookupVal.ToUpper() == "EARNING"))
                    {
                        p.Add(new empdetails1
                        {
                            earn = item.SalaryHead.Name.ToString(),
                            amt = item.Amount.ToString(),

                        });
                    }

                }
                var d = b.SalEarnDedT.Where(c => (c.SalaryHead.Type.LookupVal.ToString().ToUpper() == "DEDUCTION") &&
                    (c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "EPF" && c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PTAX" && c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ITAX"
                        && c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "LWF" && c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ESIC" && c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "CPF"
                        && c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PENSION")).ToList();

                        string FileCompCode = "";
                        string requiredPathpdcc = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                        bool existspdcc = System.IO.Directory.Exists(requiredPathpdcc);
                        string localPathpdcc;
                        if (!existspdcc)
                        {
                            localPathpdcc = new Uri(requiredPathpdcc).LocalPath;
                            System.IO.Directory.CreateDirectory(localPathpdcc);
                        }
                        string pathpdcc = requiredPathpdcc + @"\LoanInterfacePerkCalc" + ".ini";
                        localPathpdcc = new Uri(pathpdcc).LocalPath;
                        using (var streamReader = new StreamReader(localPathpdcc))
                        {
                            string line;

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var comp = line;
                                FileCompCode = comp;
                            }
                        }
                        var CompId = Convert.ToInt32(SessionManager.CompanyId);
                        Company OCompany = null;
                        OCompany = db.Company.Find(CompId);
                        
                            List<empdetails> a = new List<empdetails>();
                            {
                                foreach (var item in d)
                                {
                                    if (OCompany.Code == FileCompCode)
                                    {
                                        if (item.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN")
                                        {
                                            LoanAdvRequest LoanAdvRequest = db.LoanAdvRequest.Include(e => e.LoanAdvRepaymentT)
                                                    .Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.LoanAdvanceHead.SalaryHead_Id == item.SalaryHead_Id && e.IsActive == true).FirstOrDefault();
                                            LoanAdvRepaymentT OLoanRepay = LoanAdvRequest.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).FirstOrDefault();
                                            a.Add(new empdetails
                                            {
                                                Id = item.Id,
                                                Name = item.SalaryHead.Name.ToString(),
                                                Amount = OLoanRepay.MonthlyPricipalAmount.ToString(),
                                                Code = item.SalaryHead.Code.ToString(),
                                                SalaryTId = b.Id,
                                                ded = OLoanRepay.MonthlyInterest.ToString(),
                                                amount = OLoanRepay.InstallmentPaid.ToString()
                                            });
                                        }
                                        else
                                        {
                                            a.Add(new empdetails
                                            {
                                                Id = item.Id,
                                                Name = item.SalaryHead.Name.ToString(),
                                                Amount = item.Amount.ToString(),
                                                Code = item.SalaryHead.Code.ToString(),
                                                SalaryTId = b.Id,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        a.Add(new empdetails
                                        {
                                            Id = item.Id,
                                            Name = item.SalaryHead.Name.ToString(),
                                            Amount = item.Amount.ToString(),
                                            Code = item.SalaryHead.Code.ToString(),
                                            SalaryTId = b.Id,
                                        });
                                    }
                                }

                            }
                        
              
                var SalaryTId = b.Id;
                double s1 = SalT.TotalEarning;
                double s2 = SalT.TotalDeduction;
                double s3 = SalT.TotalNet;

                var negsal = db.NegSalAct.SingleOrDefault();
                double SalPercentage = 0;
                double exceed = 0;
                double exceedround = 0;
                double percongross = negsal.SalPercentage;
                if (negsal != null)
                {
                    // SalPercentage = 100 - negsal.SalPercentage;
                    double perc = (s1 * negsal.SalPercentage) / 100;
                    if (s2 > perc)
                    {
                        exceed = s3 - perc;
                    }

                }
                if (exceed < 0)
                {
                    exceedround = Math.Round(exceed - 0.001, 0);
                }
                else
                {
                    exceedround = Math.Round(exceed + 0.001, 0);
                }
                var result = new { Name = Empnm, Sal = p, Salded = a, totearn = s1, totded = s2, grossearn = s3, SalaryTId = SalaryTId, excessval = exceedround, month = PayMonth, percent = percongross };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getdataSusp(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                string Emp = form["employee-table1"] == "0" ? "" : form["employee-table1"];
                string PayMonth = form["txtPayMonth1"] == "0" ? "" : form["txtPayMonth1"];

                int ids = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = int.Parse(Emp);
                }
                else
                {

                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                CompanyPayroll OCompanyPayroll = null;



                OEmployee = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                   .Where(r => r.Id == ids).SingleOrDefault();

                OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Employee.Id == ids).SingleOrDefault();

                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                var SalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                var Empnm = (OEmployee.EmpCode + " - " + OEmployee.EmpName.FName);

                var sal = db.EmployeePayroll.Include(e => e.SalaryT).Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                  .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead)))
                    .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.Type)))
                     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.SalHeadOperationType)))
                  .Include(e => e.Employee)
                                     .Where(e => e.Employee.Id == OEmployee.Id)
                                         .SingleOrDefault();
                var b = sal.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                List<empdetails1> p = new List<empdetails1>();
                {
                    foreach (var item in b.SalEarnDedT.Where(c => c.SalaryHead.Type.LookupVal == "Earning"))
                    {
                        p.Add(new empdetails1
                        {
                            Id = item.Id,
                            earn = item.SalaryHead.Name.ToString(),
                            amt = item.Amount.ToString(),

                        });
                    }

                }
                var d = b.SalEarnDedT.Where(c => (c.SalaryHead.Type.LookupVal.ToString().ToUpper() == "DEDUCTION")).ToList();

                List<empdetails> a = new List<empdetails>();
                {
                    foreach (var item in d)
                    {
                        a.Add(new empdetails
                        {
                            Id = item.Id,
                            Name = item.SalaryHead.Name.ToString(),
                            Amount = item.Amount.ToString(),
                            Code = item.SalaryHead.Code.ToString(),
                            SalaryTId = b.Id,
                        });
                    }

                }
                var SalaryTId = b.Id;
                var s1 = SalT.TotalEarning;
                var s2 = SalT.TotalDeduction;
                var s3 = SalT.TotalNet;
                var result = new { Name = Empnm, Sal = p, Salded = a, totearn = s1, totded = s2, grossearn = s3, SalaryTId = SalaryTId };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Get_EmployelistSusp(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy"));
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var Emp = db.EmployeePayroll
                    .Include(e => e.SalaryT)
                    .Include(e => e.Employee).AsNoTracking().AsParallel().ToList();
                var GetMonthSal = Emp.Select(a => new { Employee = a.Employee, SalaryT = a.SalaryT.Where(e => e.PayMonth == monthyr).SingleOrDefault() }).ToList();
                var tempEmp = new List<Employee>();
                foreach (var i in GetMonthSal)
                {
                    EmployeePayroll OEmpSusp = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.JobStatus)
                        .Include(e => e.Employee.PayStruct.JobStatus.EmpActingStatus)
                        .Include(e => e.OtherServiceBook)
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct.JobStatus))
                        .Include(e => e.OtherServiceBook.Select(r => r.NewPayStruct.JobStatus.EmpActingStatus))
                        .Include(e => e.OtherServiceBook.Select(r => r.OthServiceBookActivity))
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Where(e => e.Employee.Id == i.Employee.Id
                            && (e.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "SUSPEND"
                             || e.Employee.PayStruct.JobStatus.EmpActingStatus.LookupVal.ToUpper() == "SUSPENDED")).AsNoTracking().AsParallel().SingleOrDefault();

                    if (OEmpSusp != null)
                    {
                        OtherServiceBook OthServBkSus = OEmpSusp.OtherServiceBook
                          .Where(e => (e.NewPayStruct != null && e.NewPayStruct.JobStatus != null &&
                              e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal != null) &&
                              e.NewPayStruct.JobStatus.EmpActingStatus.LookupVal.ToString().ToUpper() == "SUSPEND" &&
                              e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").OrderByDescending(e => e.Id).SingleOrDefault();

                        if (OthServBkSus != null)
                        {
                            tempEmp.Add(OEmpSusp.Employee);
                        }
                    }
                }
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = tempEmp.Where(e => e.GeoStruct != null && e.GeoStruct.Id == id).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                                if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                                {
                                    data.Add(item);
                                }


                            }
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = tempEmp.Where(e => e.PayStruct != null && e.PayStruct.Id == id).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                                if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                                {
                                    data.Add(item);
                                }

                            }
                        }
                    }
                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = tempEmp.Where(e => e.FuncStruct != null && e.FuncStruct.Id == id).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                                if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                                {
                                    data.Add(item);
                                }
                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode))
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table1"
                    };
                    return Json(returnjson, JsonRequestBehavior.AllowGet);
                }
                else if (tempEmp.ToList().Count > 0)
                {
                    foreach (var item in tempEmp.Distinct())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table1"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found" }, JsonRequestBehavior.AllowGet);

                    // return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class EditDataClass
        {
            public string Id { get; set; }
            public string Val { get; set; }
            public string SalId { get; set; }
            public string Val2 { get; set; }
            public string Val3 { get; set; }
        }

        public ActionResult LockNegData(List<EditDataClass> stringify_JsonObj)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (stringify_JsonObj.Count > 0)
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    int SalTId = Convert.ToInt32(stringify_JsonObj.First().SalId);
                    SalaryT OSalT = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.NegSalData)).Where(e => e.Id == SalTId).SingleOrDefault();
                    int? EmpPayId = OSalT.EmployeePayroll_Id;
                    if (OSalT.ReleaseDate != null)
                    {
                        var res = new { totearn = OSalT.TotalEarning, totded = OSalT.TotalDeduction, grossearn = OSalT.TotalNet, success = false, responseText = "Salary released..!" };

                        return Json(res, JsonRequestBehavior.AllowGet);
                    }
                    OSalT.ReleaseDate = DateTime.Now;
                    db.Entry(OSalT).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(OSalT).State = System.Data.Entity.EntityState.Detached;
                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (var item in stringify_JsonObj)
                        {
                            var id = Convert.ToInt32(item.Id);
                            var Val = Convert.ToDouble(item.Val);
                            var query = db.SalEarnDedT.Include(q => q.NegSalData).Where(e => e.Id == id).SingleOrDefault().NegSalData;
                            query.ReleaseFlag = true;
                            db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(query).State = System.Data.Entity.EntityState.Detached;
                        }
                        ts.Complete();
                    }
                    var result = new { totearn = 0, totded = 0, grossearn = 0, success = true, responseText = "Salary Locked..!" };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult LockSuspData(List<EditDataClass> stringify_JsonObj)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (stringify_JsonObj.Count > 0)
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    int SalTId = Convert.ToInt32(stringify_JsonObj.First().SalId);
                    SalaryT OSalT = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.NegSalData)).Where(e => e.Id == SalTId).SingleOrDefault();
                    int? EmpPayId = OSalT.EmployeePayroll_Id;
                    if (OSalT.ReleaseDate != null)
                    {
                        var res = new { totearn = OSalT.TotalEarning, totded = OSalT.TotalDeduction, grossearn = OSalT.TotalNet, success = false, responseText = "Salary released..!" };

                        return Json(res, JsonRequestBehavior.AllowGet);
                    }
                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (var item in stringify_JsonObj)
                        {
                            var id = Convert.ToInt32(item.Id);
                            var Val = Convert.ToDouble(item.Val);
                            var query = db.SalEarnDedT.Include(q => q.NegSalData).Where(e => e.Id == id).SingleOrDefault().NegSalData;
                            query.ReleaseFlag = true;
                            db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(query).State = System.Data.Entity.EntityState.Detached;
                        }
                        ts.Complete();
                    }
                    var result = new { totearn = 0, totded = 0, grossearn = 0, success = true, responseText = "Salary Locked..!" };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }

            }
        }
 

        public ActionResult editdata(List<EditDataClass> stringify_JsonObj)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (stringify_JsonObj != null && stringify_JsonObj.Count > 0)
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    int SalTId = Convert.ToInt32(stringify_JsonObj.First().SalId);
                    SalaryT OSalT = db.SalaryT.Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.NegSalData)).Where(e => e.Id == SalTId).SingleOrDefault();
                    int? EmpPayId = OSalT.EmployeePayroll_Id;
                    if (OSalT.ReleaseDate != null)
                    {
                        var res = new { totearn = OSalT.TotalEarning, totded = OSalT.TotalDeduction, grossearn = OSalT.TotalNet, success = false, responseText = "Salary released.. Can not update now..!" };

                        return Json(res, JsonRequestBehavior.AllowGet);
                    }
                    if (OSalT.SalEarnDedT.Where(r => r.NegSalData != null && r.NegSalData.ReleaseFlag == true).Count() > 0)
                    {
                        var res = new { totearn = OSalT.TotalEarning, totded = OSalT.TotalDeduction, grossearn = OSalT.TotalNet, success = false, responseText = "Salary locked.. Can not update now..!" };

                        return Json(res, JsonRequestBehavior.AllowGet);
                    }
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var CompId = Convert.ToInt32(SessionManager.CompanyId);
                        Company OCompany = null;
                        OCompany = db.Company.Find(CompId);
                        string FileCompCode = "";
                        string requiredPathpdcc = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                        bool existspdcc = System.IO.Directory.Exists(requiredPathpdcc);
                        string localPathpdcc;
                        if (!existspdcc)
                        {
                            localPathpdcc = new Uri(requiredPathpdcc).LocalPath;
                            System.IO.Directory.CreateDirectory(localPathpdcc);
                        }
                        string pathpdcc = requiredPathpdcc + @"\LoanInterfacePerkCalc" + ".ini";
                        localPathpdcc = new Uri(pathpdcc).LocalPath;
                        using (var streamReader = new StreamReader(localPathpdcc))
                        {
                            string line;

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var comp = line;
                                FileCompCode = comp;
                            }
                        }

                        if (OCompany.Code == FileCompCode)
                        {
                          
                            foreach (var item in stringify_JsonObj)
                            {
                                double total = 0; int Id = 0;
                                if (item.Id.Contains("E") || item.Id.Contains("P") ||item.Id.Contains("I")) 
                                    Id = Convert.ToInt32(item.Id.Remove(0, 4)); 
                                else
                                    Id = Convert.ToInt32(item.Id);

                                var query = db.SalEarnDedT.Include(q => q.SalaryHead).Include(q => q.SalaryHead.SalHeadOperationType).Where(e => e.Id == Id).FirstOrDefault();
                                var Val = Convert.ToDouble(item.Val);


                                if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN")
                                {
                                    var Val2 = Convert.ToDouble(item.Val2);
                                    var Val3 = Convert.ToDouble(item.Val3);


                                    if (Val != null && Val2 != null && Val3 != null)
                                    {
                                        total = (Val2 + Val3);
                                        if (Val < total || Val > total)
                                        {
                                            return Json(new { success = false, responseText = "Kindly Enter Proper Amount in " + query.SalaryHead.Name + " - Principle & Interest..!" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                    if (query.Amount != Val)
                                    {

                                        NegSalData NegData = new NegSalData()
                                        {
                                            Amount = total,
                                            StdAmount = query.Amount,
                                            DiffAmount = query.Amount - total,
                                            ReleaseFlag = false,
                                            DBTrack = dbt
                                        };
                                        db.NegSalData.Add(NegData);
                                        db.SaveChanges();
                                        query.Amount = total;
                                        query.NegSalData = NegData;
                                        db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();

                                        string PayMonth = db.SalaryT.Where(e => e.Id == SalTId).SingleOrDefault().PayMonth;
                                        //LoanAdvRequest OLoanAdvReq = db.LoanAdvRequest.Include(e => e.LoanAdvanceHead).Include(e=> e.LoanAdvRepaymentT)
                                        //    .Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper() &&  ).SingleOrDefault();
                                        EmployeePayroll OEmpPay = db.EmployeePayroll.Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                            .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT)).Include(e => e.PerkTransT)
                                            .Where(e => e.Id == EmpPayId).SingleOrDefault();
                                        LoanAdvRequest OLoanAdvReq = OEmpPay.LoanAdvRequest.Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper()).SingleOrDefault();
                                        if (OLoanAdvReq != null)
                                        {
                                            LoanAdvRepaymentT OLoanAdvRepay = OLoanAdvReq.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                                            OLoanAdvRepay.MonthlyPricipalAmount = Val2;
                                            OLoanAdvRepay.MonthlyInterest = Val3;
                                            OLoanAdvRepay.InstallmentPaid = Val;
                                            db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Detached;

                                            var PerktranstData = OEmpPay.PerkTransT
                                                      .Where(q => q.PayMonth == PayMonth && q.SalaryHead_Id == OLoanAdvReq.LoanAdvanceHead.PerkHead_Id)
                                                      .FirstOrDefault();
                                            if (PerktranstData != null)
                                            {
                                                var LoanHeadId = db.LoanAdvanceHead.Include(e => e.PerkHead).Include(e => e.LoanAdvancePolicy).Where(a => a.Code == query.SalaryHead.Code.ToUpper()).FirstOrDefault();

                                                #region Perk calculation
                                                DateTime FileDate = Convert.ToDateTime("01/" + PayMonth);
                                                double SBIIntrest = 0;
                                                double TblBankInt = 0;
                                                if (LoanHeadId.LoanAdvancePolicy != null && LoanHeadId.LoanAdvancePolicy.Count() > 0)
                                                {
                                                    SBIIntrest = LoanHeadId.LoanAdvancePolicy.Where(e => FileDate >= e.EffectiveDate.Value && FileDate <= e.EndDate.Value).FirstOrDefault().GovtIntRate;
                                                    TblBankInt = LoanHeadId.LoanAdvancePolicy.Where(e => FileDate >= e.EffectiveDate.Value && FileDate <= e.EndDate.Value).FirstOrDefault().IntRate;

                                                }

                                                double PerkActualAmt = 0;
                                                double IntDiff = SBIIntrest - TblBankInt;

                                                PerkActualAmt = ((Val3 / TblBankInt) * IntDiff);
                                                PerkActualAmt = Math.Round(PerkActualAmt, 0);


                                                PerktranstData.ActualAmount = Math.Round(PerkActualAmt + 0.001, 0);
                                                PerktranstData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                db.Entry(PerktranstData).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();

                                                var PerkSalEarn = db.SalEarnDedT.Where(e => e.SalaryHead_Id == PerktranstData.SalaryHead_Id && e.SalaryT_Id == SalTId).FirstOrDefault();
                                                if (PerkSalEarn != null)
                                                {
                                                    PerkSalEarn.Amount = PerkActualAmt;
                                                    db.Entry(PerkSalEarn).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(PerkSalEarn).State = System.Data.Entity.EntityState.Detached;
                                                }
                                                #endregion
                                            }
                                        }

                                    }

                                }

                                else
                                {
                                    if (query.Amount != Val)
                                    {
                                        NegSalData NegData = new NegSalData()
                                        {
                                            Amount = Val,
                                            StdAmount = query.Amount,
                                            DiffAmount = query.Amount - Val,
                                            ReleaseFlag = false,
                                            DBTrack = dbt
                                        };
                                        db.NegSalData.Add(NegData);
                                        db.SaveChanges();
                                        query.Amount = Val;
                                        query.NegSalData = NegData;
                                        db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }

                              

                               
                            
                            }
                        }
                        else
                        {
                            foreach (var item in stringify_JsonObj)
                            {
                                var id = Convert.ToInt32(item.Id.Remove(0, 4));
                                var Val = Convert.ToDouble(item.Val); 
                                var query = db.SalEarnDedT.Include(q => q.SalaryHead).Include(q => q.SalaryHead.SalHeadOperationType).Where(e => e.Id == id).SingleOrDefault();
                             
                                NegSalData NegData = new NegSalData()
                                {
                                    Amount = Val,
                                    StdAmount = query.Amount,
                                    DiffAmount = query.Amount - Val,
                                    ReleaseFlag = false,
                                    DBTrack = dbt
                                };
                                db.NegSalData.Add(NegData);
                                db.SaveChanges();
                                if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN")
                                {
                                    string PayMonth = db.SalaryT.Where(e => e.Id == SalTId).SingleOrDefault().PayMonth;
                                    //LoanAdvRequest OLoanAdvReq = db.LoanAdvRequest.Include(e => e.LoanAdvanceHead).Include(e=> e.LoanAdvRepaymentT)
                                    //    .Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper() &&  ).SingleOrDefault();
                                    EmployeePayroll OEmpPay = db.EmployeePayroll.Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                        .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                                        .Where(e => e.Id == EmpPayId).SingleOrDefault();
                                    LoanAdvRequest OLoanAdvReq = OEmpPay.LoanAdvRequest.Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper()).SingleOrDefault();
                                    if (OLoanAdvReq != null)
                                    {
                                        LoanAdvRepaymentT OLoanAdvRepay = OLoanAdvReq.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                                        //OLoanAdvRepay.InstallmentAmount = Val;
                                        OLoanAdvRepay.InstallmentPaid = Val;
                                        // OLoanAdvRepay.MonthlyPricipalAmount = Val;
                                        db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                query.Amount = Val;
                                query.NegSalData = NegData;
                                db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(query).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    
                        ts.Complete();
                    }
                    double TotEar = db.SalaryT.Where(e => e.Id == SalTId).Select(e => e.SalEarnDedT.Where(r => r.SalaryHead.InPayslip == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(r => r.Amount)).SingleOrDefault();
                    double TotDed = db.SalaryT.Where(e => e.Id == SalTId).Select(e => e.SalEarnDedT.Where(r => r.SalaryHead.InPayslip == true && r.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").Sum(r => r.Amount)).SingleOrDefault();
                    double TotNet = Math.Round((TotEar - TotDed), 2);

                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var CompData = db.CompanyPayroll.Include(e => e.NegSalAct)
                       .Where(e => e.Company.Id == compid).SingleOrDefault();
                    if (CompData.NegSalAct == null)
                    {
                        return Json(null);
                    }
                    var NegSalActPolicy = CompData.NegSalAct.SingleOrDefault();
                    var TotalGross = Math.Round((TotNet / TotEar) * 100, 0);

                    var que = db.SalaryT.Where(e => e.Id == SalTId).SingleOrDefault();
                    que.TotalEarning = TotEar;
                    que.TotalDeduction = TotDed;
                    que.TotalNet = TotNet;
                    db.Entry(que).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(que).State = System.Data.Entity.EntityState.Detached;

                    if (TotalGross < NegSalActPolicy.SalPercentage)
                    {
                        var result1 = new { totearn = TotEar, totded = TotDed, grossearn = TotNet, success = true, responseText = "You have to edit more changes..!" };

                        return Json(result1, JsonRequestBehavior.AllowGet);
                    }


                    //return Json(new { success = true, responseText = "ReCord Updated..!" }, JsonRequestBehavior.AllowGet);
                    var result = new { totearn = TotEar, totded = TotDed, grossearn = TotNet, success = true, responseText = "ReCord Updated..!" };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult editdataSusp(List<EditDataClass> stringify_JsonObj)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (stringify_JsonObj.Count > 0)
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    int SalTId = Convert.ToInt32(stringify_JsonObj.First().SalId);
                    SalaryT OSalT = db.SalaryT.Include(e => e.PTaxTransT).Include(e => e.PFECRR).Include(e => e.ESICTransT).Include(e => e.LWFTransT)
                                  .Include(e => e.ITaxTransT).Where(e => e.Id == SalTId).SingleOrDefault();

                    if (OSalT.ReleaseDate != null)
                    {
                        var res = new { totearn = OSalT.TotalEarning, totded = OSalT.TotalDeduction, grossearn = OSalT.TotalNet, success = false, responseText = "Salary released.. Can not update now..!" };

                        return Json(res, JsonRequestBehavior.AllowGet);
                    }
                    //   CompanyPayroll OCompanyPayroll=db.CompanyPayroll.Include(q=>q.PFMaster).
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company)
                                                          .Include(e => e.Company.Calendar.Select(r => r.Name))
                                                          .Include(e => e.Company.Calendar)
                                                           .Include(e => e.PFMaster)
                                                           .Include(e => e.PTaxMaster)
                                                           .Include(e => e.LWFMaster.Select(t => t.LWFStates))
                                                           .Include(e => e.LWFMaster.Select(t => t.LWFStatutoryEffectiveMonths))
                                                           .Include(e => e.ESICMaster)
                                                           .Include(e => e.PTaxMaster.Select(a => a.States))
                                                            .Where(d => d.Company.Id == CompId).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();
                    int? EmpPayId = OSalT.EmployeePayroll_Id;
                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (var item in stringify_JsonObj)
                        {
                            var id = Convert.ToInt32(item.Id);
                            var Val = Convert.ToDouble(item.Val);
                            var query = db.SalEarnDedT
                                .Include(e => e.SalaryHead)
                                .Include(e => e.SalaryHead.SalHeadOperationType)
                                .Include(e => e.SalaryHead.Type)
                                .Where(e => e.Id == id).SingleOrDefault();
                            ////added on 28/08/2019
                            NegSalData NegData = new NegSalData()
                            {
                                Amount = Val,
                                StdAmount = query.Amount,
                                DiffAmount = query.Amount - Val,
                                ReleaseFlag = false,
                                DBTrack = dbt
                            };
                            db.NegSalData.Add(NegData);
                            db.SaveChanges();
                            ////

                            if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                            {
                                if (OSalT.PFECRR != null)
                                {
                                    var EmpPayrollempid = db.EmployeePayroll.Where(e => e.Id == OSalT.EmployeePayroll_Id).SingleOrDefault().Employee_Id;
                                    var chkPFApplicable = db.Employee.Include(e => e.EmpOffInfo).Where(e => e.Id == EmpPayrollempid).SingleOrDefault();
                                    if (chkPFApplicable.EmpOffInfo.PFAppl == true)
                                    {


                                      //  PFMaster OCompPFMaster = OCompanyPayroll.PFMaster.Where(e => e.EstablishmentID == chkPFApplicable.EmpOffInfo.PFTrust_EstablishmentId && e.EndDate == null || e.EndDate.Value.Date > Convert.ToDateTime("01/" + OSalT.PayMonth).Date).SingleOrDefault();
                                        PFMaster OCompPFMaster = null;
                                        foreach (var itemPFmas in OCompanyPayroll.PFMaster.ToList())
                                        {
                                            if (itemPFmas.EstablishmentID == chkPFApplicable.EmpOffInfo.PFTrust_EstablishmentId && (itemPFmas.EndDate == null || itemPFmas.EndDate.Value.Date > Convert.ToDateTime("01/" + OSalT.PayMonth).Date))
                                            {
                                                OCompPFMaster = itemPFmas;
                                            }
                                        }
                                        var OPFMaster = db.PFMaster
                                                                .Include(e => e.EPSWages)
                                                                .Include(e => e.EPSWages.RateMaster.Select(a => a.SalHead))
                                                                .Include(e => e.PFAdminWages)
                                                                .Include(e => e.PFEDLIWages)
                                                                .Include(e => e.PFInspWages)
                                                                .Include(e => e.EPFWages)
                                                                .Include(e => e.PFAdminWages.RateMaster)
                                                                .Include(e => e.PFEDLIWages.RateMaster)
                                                                .Include(e => e.PFInspWages.RateMaster)
                                                                .Include(e => e.EPFWages.RateMaster)
                                                                .Include(e => e.EPFWages.RateMaster.Select(a => a.SalHead))
                                                                .Include(e => e.PFTrustType)
                                                                .Where(e => e.Id == OCompPFMaster.Id && e.EstablishmentID == chkPFApplicable.EmpOffInfo.PFTrust_EstablishmentId).OrderBy(e => e.Id)
                                                                .FirstOrDefault();

                                        double mAge = 0;
                                        var mDateofBirth = db.EmployeePayroll.Where(e => e.Id == EmpPayId).Include(e => e.Employee)
                                            .Include(e => e.Employee.ServiceBookDates)
                                            .SingleOrDefault().Employee.ServiceBookDates.BirthDate;// db.Employee.Where(e => e.Id == mEmployeeID).Select(e => e.ServiceBookDates.BirthDate);
                                        DateTime start = mDateofBirth.Value;
                                        DateTime end = Convert.ToDateTime("01/" + OSalT.PayMonth).AddMonths(1).AddDays(-1).Date;// DateTime.Now.Date;
                                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                        double daysInEndMonth = (end - end.AddMonths(1)).Days;
                                        double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                        mAge = months / 12;
                                        if (compMonth == OPFMaster.PensionAge * 12 && start.Month == end.Month)
                                        {
                                            mAge = compMonth / 12;
                                        }

                                        double mCompPF = 0;
                                        double mEmpPension = 0;
                                        double mEmpPF = 0;
                                        double mEmpVPF = 0;
                                        double mTotalCompPF = 0;
                                        double mGrossWages = OSalT.SalEarnDedT.Where(r => r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && r.SalaryHead.InPayslip == true)
                                            .Sum(e => e.Amount);
                                        mGrossWages = Math.Round(mGrossWages, 0);

                                        List<SalEarnDedT> OSalaryDetails = OSalT.SalEarnDedT.Where(r => r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING" && r.SalaryHead.InPayslip == true).ToList();

                                        double mPFWages = (Val * 100 / OPFMaster.EmpPFPerc);
                                        mPFWages = Process.SalaryHeadGenProcess.RoundingFunction(query.SalaryHead, mPFWages);


                                        double mPensionWages = 0;
                                        if (mPFWages < OPFMaster.EPSWages.CeilingMax)
                                        {
                                            mPensionWages = mPFWages;
                                        }
                                        else
                                        {
                                            mPensionWages = OPFMaster.EPSWages.CeilingMax;
                                        }
                                        mPensionWages = Math.Round(mPensionWages, 0);

                                        mEmpPF = Val;
                                        mEmpPF = Math.Round(mEmpPF, 0, MidpointRounding.AwayFromZero);

                                        if (mAge <= OPFMaster.PensionAge)
                                        {
                                            mEmpPension = Math.Round(mPensionWages * OPFMaster.EPSPerc / 100, 0);
                                        }
                                        else
                                        {
                                            mEmpPension = 0;
                                        }
                                        mCompPF = mEmpPF - mEmpPension;

                                        if (mCompPF > OPFMaster.CompPFCeiling)
                                        {
                                            mCompPF = OPFMaster.CompPFCeiling;
                                        }


                                        PFECRR OPFECR = db.PFECRR.Where(e => e.Id == OSalT.PFECRR.Id).SingleOrDefault();
                                        OPFECR.EDLI_Wages = 0;
                                        OPFECR.EE_Share = mEmpPF;
                                        OPFECR.EE_VPF_Share = 0;
                                        OPFECR.EPF_Wages = mPFWages;
                                        OPFECR.EPS_Share = mEmpPension;
                                        OPFECR.EPS_Wages = mPensionWages;
                                        OPFECR.ER_Share = mCompPF;
                                        OPFECR.Gross_Wages = mGrossWages;
                                        db.Entry(OPFECR).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(OPFECR).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                            if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "PTAX")
                            {
                                if (OSalT.PTaxTransT != null)
                                {
                                    var Ptaxmaster = db.PTaxMaster.Include(q => q.PTWagesMaster).SingleOrDefault();

                                    PTaxTransT OPTaxTrans = db.PTaxTransT.Where(e => e.Id == OSalT.PTaxTransT.Id).SingleOrDefault();
                                    OPTaxTrans.PTAmount = Val;
                                    OPTaxTrans.PTWages = Ptaxmaster.PTWagesMaster.Percentage / 100 * Val;
                                    db.Entry(OPTaxTrans).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OPTaxTrans).State = System.Data.Entity.EntityState.Detached;
                                }

                            }
                            if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LWF")
                            {
                                if (OSalT.LWFTransT != null)
                                {
                                    LWFTransT OLWFTransT = db.LWFTransT.Where(e => e.Id == OSalT.LWFTransT.Id).SingleOrDefault();
                                    OLWFTransT.CompAmt = 0;
                                    OLWFTransT.EmpAmt = 0;
                                    OLWFTransT.LWFWages = 0;
                                    db.Entry(OLWFTransT).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OLWFTransT).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ESIC")
                            {
                                if (OSalT.ESICTransT != null)
                                {
                                    ESICTransT OESICTransT = db.ESICTransT.Where(e => e.Id == OSalT.ESICTransT.Id).SingleOrDefault();
                                    OESICTransT.CompAmt = 0;
                                    OESICTransT.EmpAmt = 0;
                                    OESICTransT.ESICWagesPay = 0;
                                    OESICTransT.ESICQualify = 0;
                                    db.Entry(OESICTransT).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OESICTransT).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")
                            {
                                if (OSalT.ITaxTransT != null)
                                {
                                    ITaxTransT OITaxTransT = db.ITaxTransT.Where(e => e.Id == OSalT.ITaxTransT.Id).SingleOrDefault();
                                    OITaxTransT.EduCess = 0;
                                    OITaxTransT.Surcharge = 0;
                                    OITaxTransT.TaxOnIncome = 0;
                                    OITaxTransT.TaxPaid = Val;
                                    db.Entry(OITaxTransT).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OITaxTransT).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            if (query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN")
                            {
                                string PayMonth = db.SalaryT.Where(e => e.Id == SalTId).SingleOrDefault().PayMonth;
                                //LoanAdvRequest OLoanAdvReq = db.LoanAdvRequest.Include(e => e.LoanAdvanceHead).Include(e=> e.LoanAdvRepaymentT)
                                //    .Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper() &&  ).SingleOrDefault();
                                EmployeePayroll OEmpPay = db.EmployeePayroll.Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                    .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                                    .Where(e => e.Id == EmpPayId).SingleOrDefault();
                                LoanAdvRequest OLoanAdvReq = OEmpPay.LoanAdvRequest.Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper()).SingleOrDefault();
                                if (OLoanAdvReq != null)
                                {
                                    LoanAdvRepaymentT OLoanAdvRepay = OLoanAdvReq.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                                    //  OLoanAdvRepay.InstallmentAmount = Val;
                                    OLoanAdvRepay.InstallmentPaid = Val;
                                    //OLoanAdvRepay.MonthlyPricipalAmount = Val;
                                    db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            query.Amount = Val;
                            ////added on 28/08/2019
                            query.NegSalData = NegData;
                            //
                            db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(query).State = System.Data.Entity.EntityState.Detached;
                        }
                        ts.Complete();
                    }
                    double TotEar = db.SalaryT.Where(e => e.Id == SalTId).Select(e => e.SalEarnDedT.Where(r => r.SalaryHead.InPayslip == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(r => r.Amount)).SingleOrDefault();
                    double TotDed = db.SalaryT.Where(e => e.Id == SalTId).Select(e => e.SalEarnDedT.Where(r => r.SalaryHead.InPayslip == true && r.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").Sum(r => r.Amount)).SingleOrDefault();
                    double TotNet = Math.Round((TotEar - TotDed), 2);

                    //var compid = Convert.ToInt32(Session["CompId"].ToString());
                    //var CompData = db.CompanyPayroll.Include(e => e.NegSalAct)
                    //   .Where(e => e.Company.Id == compid).SingleOrDefault();

                    //    var result1 = new { totearn = TotEar, totded = TotDed, grossearn = TotNet, success = true, responseText = "You have to edit more changes..!" };

                    //    return Json(result1, JsonRequestBehavior.AllowGet);

                    var que = db.SalaryT.Where(e => e.Id == SalTId).SingleOrDefault();
                    que.TotalEarning = TotEar;
                    que.TotalDeduction = TotDed;
                    que.TotalNet = TotNet;
                    db.Entry(que).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(que).State = System.Data.Entity.EntityState.Detached;
                    //return Json(new { success = true, responseText = "ReCord Updated..!" }, JsonRequestBehavior.AllowGet);
                    var result = new { totearn = TotEar, totded = TotDed, grossearn = TotNet, success = true, responseText = "ReCord Updated..!" };

                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult ChkIFManual(FormCollection form, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<int> ids = null;

                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];


                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = Utility.StringIdsToListIds(Emp);
                }
                else
                {
                    Msg.Add("Kindly select employee");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                var ItaxCheck = db.ITaxTransT
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Employee)
                    .AsNoTracking().Where(e => e.PayMonth == PayMonth && e.Mode.ToUpper() == "MAN" && ids.Contains(e.EmployeePayroll.Employee.Id)).Select(e => e.EmployeePayroll.Id).ToList();

                foreach (var EmpId in ItaxCheck)
                {
                    EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpId);


                    if ((EmpPay.Employee.ServiceBookDates.ServiceLastDate == null))
                    {
                        Msg.Add(EmpPay.Employee.FullDetails + ", Manual IncomeTax entry of this employee is entered ,");
                    }
                }
                if (Msg.Count() > 0)
                {
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(new Utility.JsonReturnClass { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public class AutoDataClass
        {
            public string Autho_Action { get; set; }
            public string Autho_Allow { get; set; }
            public string txtPayMonth1 { get; set; }
            public string employeetable1 { get; set; }
        }

        public ActionResult Autosavedata(string form)
        {
            List<string> Msg = new List<string>();
            //"Autho_Action=&Autho_Allow=&txtPayMonth1=06%2F2019&employee-table1=36"
            string[] values = (form.Split(new string[] { "&employee-table1=" }, StringSplitOptions.None));
            //string Emp = form.Substring(66);
            string PayMonth = form.Substring(40, 9).Replace("%2F", "/");
            List<string> Emp = values.ToList();
            //List<int> employeeids = Utility.StringIdsToListIds(Emp);            
            Emp.RemoveAt(0);
            List<int> employeeids = new List<int>();
            foreach (var item in Emp)
            {
                int empids = Convert.ToInt32(item);
                employeeids.Add(empids);
            }

            if (employeeids == null && PayMonth == null)
            {
                Msg.Add(" Kindly select employee  ");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

            string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existschk = System.IO.Directory.Exists(requiredPathchk);
            string localPathchk;
            if (!existschk)
            {
                string localPath = new Uri(requiredPathchk).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            string pathchk = requiredPathchk + @"\NegativeSalPara" + ".ini";
            localPathchk = new Uri(pathchk).LocalPath;

            if (!System.IO.File.Exists(localPathchk))
            {

                using (var fs = new FileStream(localPathchk, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);

                    str.Flush();
                    str.Close();
                    fs.Close();
                }


            }

            foreach (var emp_id in employeeids)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                    EmployeePayroll OEmpPay = db.EmployeePayroll.Where(e => e.Employee_Id == emp_id).FirstOrDefault();
                    SalaryT OSalT = db.SalaryT.Where(e => e.EmployeePayroll_Id == OEmpPay.Id && e.PayMonth == PayMonth).FirstOrDefault();
                    double Val = 0;
                    using (var streamReader = new StreamReader(localPathchk))
                    {
                        string line;
                        var compid = Convert.ToInt32(Session["CompId"].ToString());
                        var CompData = db.CompanyPayroll.Include(e => e.NegSalAct)
                           .Where(e => e.Company.Id == compid).SingleOrDefault();
                        //if (CompData.NegSalAct == null)
                        //{
                        //    return Json(null);
                        //}
                        var NegSalActPolicy = CompData.NegSalAct.SingleOrDefault();

                        double perc = ((OSalT.TotalEarning * NegSalActPolicy.SalPercentage) / 100);
                        double amountwillded = OSalT.TotalDeduction - (OSalT.TotalEarning - perc);
                        amountwillded = Math.Round(amountwillded + 0.001, 0);
                        double remamt = amountwillded;
                        double FinalAmt = 0;
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                string SalName = line;

                                if (OSalT != null)
                                {
                                    var id = db.SalaryHead.Where(e => e.Code == SalName).FirstOrDefault();

                                    var query = db.SalEarnDedT.Include(q => q.SalaryHead).Include(e => e.SalaryHead.SalHeadOperationType)
                                        .Where(e => e.SalaryHead_Id == id.Id && e.SalaryT_Id == OSalT.Id).FirstOrDefault();

                                    double headdedamt = query != null ? query.Amount : 0;

                                    double extraAmt = 0;
                                    if (headdedamt > remamt)
                                    {
                                        extraAmt = headdedamt - remamt;
                                        headdedamt = headdedamt - extraAmt;
                                        //remamt = perc;
                                        FinalAmt = FinalAmt + headdedamt;
                                    }
                                    else
                                    {
                                        extraAmt = headdedamt - (query != null ? query.Amount : 0);
                                        remamt = remamt - headdedamt;
                                        FinalAmt = FinalAmt + headdedamt;
                                    }

                                    NegSalData NegData = new NegSalData()
                                    {
                                        Amount = headdedamt,
                                        StdAmount = query != null ? query.Amount : 0,
                                        DiffAmount = (query != null ? query.Amount : 0) - headdedamt,
                                        ReleaseFlag = false,
                                        DBTrack = dbt
                                    };
                                    db.NegSalData.Add(NegData);
                                    db.SaveChanges();

                                    if (query != null && query.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "LOAN")
                                    {
                                        // string PayMonth1 = db.SalaryT.Where(e => e.Id == SalTId).SingleOrDefault().PayMonth;
                                        //LoanAdvRequest OLoanAdvReq = db.LoanAdvRequest.Include(e => e.LoanAdvanceHead).Include(e=> e.LoanAdvRepaymentT)
                                        //    .Where(e => e.LoanAdvanceHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper() &&  ).SingleOrDefault();
                                        EmployeePayroll OEmpPay1 = db.EmployeePayroll.Include(e => e.LoanAdvRequest).Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                                            .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead.SalaryHead))
                                            .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                                            .Where(e => e.Id == OEmpPay.Id).FirstOrDefault();
                                        LoanAdvRequest OLoanAdvReq = OEmpPay1.LoanAdvRequest.Where(e => e.LoanAdvanceHead.SalaryHead.Code.ToUpper() == query.SalaryHead.Code.ToUpper()).SingleOrDefault();
                                        if (OLoanAdvReq != null)
                                        {
                                            LoanAdvRepaymentT OLoanAdvRepay = OLoanAdvReq.LoanAdvRepaymentT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                                            //OLoanAdvRepay.InstallmentAmount = Val;
                                            OLoanAdvRepay.InstallmentPaid = extraAmt;
                                            //OLoanAdvRepay.MonthlyPricipalAmount = Val;
                                            db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OLoanAdvRepay).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    //if (FinalAmt == perc)
                                    //    query.Amount = Val - exceed; 
                                    //else
                                    if (query != null)
                                    {
                                        query.Amount = extraAmt;
                                        query.NegSalData = NegData;
                                        db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(query).State = System.Data.Entity.EntityState.Detached;
                                    }


                                    if (FinalAmt == amountwillded)
                                    {
                                        break;
                                    }
                                };
                            }
                            if (FinalAmt == amountwillded)
                            {
                                ts.Complete();
                            }

                        }

                        if (FinalAmt == amountwillded)
                        {
                            double TotEar = db.SalaryT.Where(e => e.Id == OSalT.Id).Select(e => e.SalEarnDedT.Where(r => r.SalaryHead.InPayslip == true && r.SalaryHead.Type.LookupVal.ToUpper() == "EARNING").Sum(r => r.Amount)).FirstOrDefault();
                            double TotDed = db.SalaryT.Where(e => e.Id == OSalT.Id).Select(e => e.SalEarnDedT.Where(r => r.SalaryHead.InPayslip == true && r.SalaryHead.Type.LookupVal.ToUpper() == "DEDUCTION").Sum(r => r.Amount)).FirstOrDefault();
                            double TotNet = Math.Round((TotEar - TotDed), 2);

                            var que = db.SalaryT.Where(e => e.Id == OSalT.Id).FirstOrDefault();
                            //que.TotalEarning = perc;
                            que.TotalDeduction = TotDed;
                            que.TotalNet = TotNet;
                            db.Entry(que).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(que).State = System.Data.Entity.EntityState.Detached;

                        }

                        //if (TotalGross == NegSalActPolicy.SalPercentage)
                        //{
                        //    break;
                        //}

                    }




                }
            }
            Msg.Add("Process Completed Successfully");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        }

    }
}