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
using System.Configuration;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class PaySlipRController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/PaySlipR/Index.cshtml");
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var query = db.PaySlipR.Where(e => e.PayMonth == month).ToList();

                if (query.Count > 0)
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Polulate_PayProcessGroup(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.Company.Include(e => e.PayProcessGroup).Where(e => e.Id == CompId).SingleOrDefault();
                    var selected = (Object)null;
                    selected = query.PayProcessGroup.Select(e => e.Id).FirstOrDefault();
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(query.PayProcessGroup, "Id", "FullDetails", selected);
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

        public ActionResult LoadEmpByDefault(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.CompanyPayroll.Where(e => e.Company.Id == CompId).Include(e => e.EmployeePayroll).Select(e => e.EmployeePayroll).SingleOrDefault();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    foreach (var ca in query.Select(e => e.Employee))
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = ca.Id.ToString(),
                            value = ca.FullDetails,
                        });
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


        public ActionResult Create(FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msgs = new List<string>();
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
                string PayMonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
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
                        //return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                        List<string> Msgu = new List<string>();
                        Msgu.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet); 
                    }

                      

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;
                    //int PayScaleAgrId = int.Parse(PayScaleAgr);
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                    foreach (var i in ids)
                    {
                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).FirstOrDefault();
                        OEmployee = db.Employee.Where(r => r.Id == i).SingleOrDefault();
                        SalaryT OSalaryT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayroll.Id).FirstOrDefault();
                        if (OSalaryT == null)
                        {
                            Msgs.Add(OEmployee.EmpCode + " - Salary Data Not Found.");
                        }
                    }
                    if (Msgs.Count() > 0)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                    }

                    foreach (var i in ids)
                    {
                      //  Utility.DumpProcessStatus("" + i + "", 163);

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                       // OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {
                            using (DataBaseContext db2 = new DataBaseContext())
                            {
                                PayrollReportGen.GereratePaySlipR(OEmployeePayroll.Id, PayMonth);
                            }
                            ts.Complete();

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

                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                
                Msgs.Add("Data Saved successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

            }


        }

        //public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            List<int> ids = null;
        //            if (forwardata != null && forwardata != "0" && forwardata != "false")
        //            {
        //                ids = Utility.StringIdsToListIds(forwardata);
        //            }
        //            Employee OEmployee = null;
        //            EmployeePayroll OEmployeePayroll = null;
        //            SalaryT osalT = null;
        //            var z = new List<int>();

        //            foreach (var i in ids)
        //            {
        //                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
        //                            .Where(r => r.Id == i).SingleOrDefault();

        //                OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT.Select(r => r.PayslipR)).Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

        //                z.Add(OEmployeePayroll.Id);

        //                osalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                                   new System.TimeSpan(0, 30, 0)))
        //                {

        //                    using (DataBaseContext db2 = new DataBaseContext())
        //                    {

        //                        var Salt = osalT.PayslipR.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

        //                        if (Salt != null)
        //                        {
        //                            Salt.PayslipLockDate = DateTime.Now.Date;
        //                            Salt.PaySlipLock = true;
        //                            db.PaySlipR.Attach(Salt);
        //                            db.Entry(Salt).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            TempData["RowVersion"] = Salt.RowVersion;
        //                            db.Entry(Salt).State = System.Data.Entity.EntityState.Detached;
        //                        }
        //                    }
        //                    ts.Complete();


        //                }
        //            }
 
                
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            List<string> Msg = new List<string>();
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }
        //        return Json(new { success = true, responseText = "Salary released for employee." }, JsonRequestBehavior.AllowGet);
        //    }
        //}

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

                    List<PaySlipR> PaySlipR = new List<PaySlipR>();
                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();
                       
                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT.Select(r => r.PayslipR)).Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();


                        SalaryT osalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                        var Salt = osalT.PayslipR.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                        if (Salt != null)
                        {
                            PaySlipR.Add(Salt);
                            //SalT.ReleaseDate = DateTime.Now.Date;
                            //db.SalaryT.Attach(SalT);
                            //db.Entry(SalT).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            //TempData["RowVersion"] = SalT.RowVersion;
                            //db.Entry(SalT).State = System.Data.Entity.EntityState.Detached;
                        }
                        //ts.Complete();
                    }
                    if (PaySlipR.Count > 0)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {

                            PaySlipR.ForEach(q => q.PayslipLockDate = DateTime.Now.Date);
                            PaySlipR.ForEach(q => q.PaySlipLock = true);
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

        public ActionResult EmailSend(string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string filePath = ConfigurationManager.AppSettings["EmailProcessPath"];
                    

                    System.Diagnostics.ProcessStartInfo info =
                      new System.Diagnostics.ProcessStartInfo(filePath, "");
                     
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo = info; 
                    p.Start(); 

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
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true, responseText = "Email sending process activated." }, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public string EmpActingStatus { get; set; }
            public DateTime? PayslipLockDate { get; set; }
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

                    var EmpPay = db.EmployeePayroll.ToList();// .Include(r=>r.Employee.Id).ToList(); //add company filter

                    foreach (var i in EmpPay)
                    {
                        var EmpList = db.Employee.Include(e => e.EmpName).Where(r => r.Id == i.Employee_Id).SingleOrDefault();

                        var EmpSal1 = db.SalaryT.Where(e => e.EmployeePayroll_Id == i.Id && e.PayMonth == PayMonth).SingleOrDefault();

                        if (EmpSal1 != null)
                        {

                            var EmpSal = db.PaySlipR.Where(e => e.SalaryT_Id == EmpSal1.Id).SingleOrDefault();

                            if (EmpSal != null)
                            {
                                view = new P2BGridData()
                                {




                                    Id = EmpList.Id,
                                    Code = EmpList.EmpCode,
                                    Name = EmpList.EmpName.FullNameFML,// z.Employee.EmpName.FullNameFML,
                                    PayMonth = EmpSal.PayMonth,
                                    EmpActingStatus = EmpSal.EmpActingStatus,
                                    PayslipLockDate = EmpSal.PayslipLockDate
                                };
                                model.Add(view);


                            }
                        }

                    } 
                    SalaryList = model;



                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e =>  (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                || (e.PayMonth.ToString().Contains(gp.searchString))
                                || (e.EmpActingStatus.ToString().Contains(gp.searchString))
                                || (e.PayslipLockDate.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new { a.Code, a.Name, a.PayMonth, a.EmpActingStatus, a.PayslipLockDate, a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, Convert.ToString(a.PayMonth), Convert.ToString(a.EmpActingStatus), Convert.ToString(a.PayslipLockDate), a.Id }).ToList();
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
                            orderfuc = (c =>    gp.sidx == "Code" ? c.Code.ToString() :
                                                gp.sidx == "Name" ? c.Name.ToString() :
                                                gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                                gp.sidx == "EmpActingStatus" ? c.EmpActingStatus.ToString() :
                                                gp.sidx == "PayslipLockDate" ? c.PayslipLockDate.ToString() :
                                                "");
                        }   
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, Convert.ToString(a.PayMonth), Convert.ToString(a.EmpActingStatus), Convert.ToString(a.PayslipLockDate), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, Convert.ToString(a.PayMonth), Convert.ToString(a.EmpActingStatus), Convert.ToString(a.PayslipLockDate), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, Convert.ToString(a.PayMonth), Convert.ToString(a.EmpActingStatus), Convert.ToString(a.PayslipLockDate), a.Id }).ToList();
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

    }
}