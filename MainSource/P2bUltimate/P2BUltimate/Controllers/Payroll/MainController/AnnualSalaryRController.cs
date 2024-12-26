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
    public class AnnualSalaryRController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
         
        public ActionResult Index()
        {

            return View("~/Views/Payroll/MainViews/AnnualSalaryR/Index.cshtml");
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (month != null)
                {
                    var finanialyear = db.Calendar.Find(int.Parse(month));
                    bool selected = false;
                    var query = db.AnnualSalaryR.Include(e => e.FinancialYear).ToList();
                    //var financialyear = query.Select(e => e.FinancialYear == finanialyear).SingleOrDefault();
                    var financialyearR = query.Where(f => f.FinancialYear == finanialyear);
                    if (query != null)
                    {
                        selected = true;
                    }
                    var data = new
                    {
                        status = selected,
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
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
                try
                {
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
                    string FinancialYear = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                    var FinancialYear_id = Convert.ToInt32(FinancialYear);
                    var OFinancialYear = db.Calendar.Where(e => e.Id == FinancialYear_id).SingleOrDefault();

                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    List<int> ids = null;
                    var EmpPayrolllist = db.EmployeePayroll.Include(e => e.EmpSalStruct).Where(e => e.EmpSalStruct.Count > 0).Select(e => e.Employee.Id).ToList();
                    if (Emp != null && Emp != "0")
                    {
                       // ids = EmpPayrolllist;
                        ids = Utility.StringIdsToListIds(Emp); 
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }



                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;
                    //int PayScaleAgrId = int.Parse(PayScaleAgr);
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                        OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            using (DataBaseContext db2 = new DataBaseContext())
                            {
                                PayrollReportGen.GererateAnnualSalaryR(OEmployeePayroll.Id, OFinancialYear);

                            }
                            ts.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
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
                    //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                List<string> Msgs = new List<string>();
                Msgs.Add("Data Saved successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCalendarDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar
                    .Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" )
                    .ToList();


                IEnumerable<Calendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    //var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string ProcessDate { get; set; }
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

                    IEnumerable<P2BGridData> CalendarList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";

                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    if (PayMonth != null && PayMonth != "")
                    {

                        var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                        //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                        //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                        var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.AnnualSalary).ToList();

                        foreach (var z in BindEmpList)
                        {
                            if (z.AnnualSalary != null)
                            {

                                foreach (var Sal in z.AnnualSalary)
                                {
                                    if (Sal.FinancialYear == financialyear)
                                    {
                                        view = new P2BGridData()
                                        {
                                            Id = z.Employee.Id,
                                            Code = z.Employee.EmpCode,
                                            Name = z.Employee.EmpName.FullNameFML,
                                            ProcessDate = Sal.DBTrack.CreatedOn.ToString()
                                        };
                                        model.Add(view);
                                    }
                                }
                            }

                        }

                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Financial Year Not Selected  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Financial Year Not Selected", JsonRequestBehavior.AllowGet });
                    }
                    CalendarList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = CalendarList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.ProcessDate.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.ProcessDate, a.Id }).ToList();

                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, Convert.ToString(a.ProcessDate), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = CalendarList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "ProcessDate" ? c.ProcessDate.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, Convert.ToString(a.ProcessDate) }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, Convert.ToString(a.ProcessDate) }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name, Convert.ToString(a.ProcessDate) }).ToList();
                        }
                        totalRecords = CalendarList.Count();
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