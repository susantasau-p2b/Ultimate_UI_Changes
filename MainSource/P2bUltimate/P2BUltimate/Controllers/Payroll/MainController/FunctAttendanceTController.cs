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
using System.Data.SqlClient;
using System.Configuration;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class FunctAttendanceTController : Controller
    {
       // List<String> Msg = new List<string>();
        List<string> Msg = new List<string>();
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/FunctAttendanceT/Index.cshtml");
        }

        #region DDL
        public ActionResult PopulateTransactionDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayProcessGroup.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var query1 = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.SalaryT).Where(e => e.Employee.EmpCode == EmpCode).Select(e => e.SalaryT).SingleOrDefault();
                var query = query1.Where(e => e.PayMonth == month).ToList();

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


        public ActionResult PopulateDropDownStructureList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var serialize = new JavaScriptSerializer();

                dynamic json = serialize.DeserializeObject(data2);

                var Processmonth = data;


                var query = db.EmpSalStruct.ToList();
                SelectList k = null;
                foreach (var ca in json)
                {
                    int id = Convert.ToInt32(ca);
                    var a = (from s in db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                     .Where(e => e.Employee.Id == id)
                                    .ToList()
                             from p in s.EmpSalStruct
                             select new
                             {
                                 Id = p.Id,
                                 EffectiveDate = p.EffectiveDate.Value.ToString("MM/yyyy"),
                                 Effectivedate_Enddate = p.FullDetails
                             }).Where(e => e.EffectiveDate == Processmonth).Distinct().OrderBy(e => e.EffectiveDate);
                    int[] Selected = a.Select(e => e.Id).ToArray();
                    k = new SelectList(a, "Id", "Effectivedate_Enddate", Selected[0]);


                }

                return Json(k, JsonRequestBehavior.AllowGet);


            }

        }

        public ActionResult GetSalaryHeadDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  List<string> Ids = SkipIds.ToString();
                var fall = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetStructureDates(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var serialize = new JavaScriptSerializer();

                dynamic json = serialize.DeserializeObject(data2);

                var Processmonth = data;
                String FromDate = null;
                String ToDate = null;

                foreach (var ca in json)
                {
                    int id = Convert.ToInt32(ca);
                    var a = (from s in db.EmployeePayroll.Include(e => e.EmpSalStruct)
                                     .Where(e => e.Employee.Id == id)
                                    .ToList()
                             from p in s.EmpSalStruct
                             select new
                             {

                                 Id = p.Id,
                                 EffectiveDate = p.EffectiveDate.Value.ToString("MM/yyyy"),
                                 FromDate = p.EffectiveDate.Value.ToString("dd/MM/yyyy"),
                                 Enddate = p.EndDate != null ? p.EndDate.Value.ToString("dd/MM/yyyy") : null
                             }).Where(e => e.EffectiveDate == Processmonth).Distinct().OrderBy(e => e.EffectiveDate).LastOrDefault();
                    if (a != null)
                    {
                        FromDate = a.FromDate.ToString();
                        if (a.Enddate != null)
                            ToDate = a.Enddate.ToString();
                    }

                }
                var jsondata = new
                {
                    FromDate = FromDate,
                    ToDate = ToDate
                };
                return Json(new { data = jsondata }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region P2BGridDetails
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }
        public ActionResult AddNewRecord(P2BGrid_Parameters gp)
        {

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BGridData> SalAttendanceT = null;
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


                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                                .Include(e => e.Employee.EmpOffInfo.PayProcessGroup).ToList();

                foreach (var z in OEmployee)
                {
                    var OSalAttendance = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.FunctAttendanceT)
                                        .SingleOrDefault();


                    DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in OSalAttendance)
                    {

                        Eff_Date = Convert.ToDateTime(a.PayMonth);
                        var aa = db.FunctAttendanceT.Where(e => e.Id == a.Id).SingleOrDefault();
                        view = new P2BGridData()
                        {
                            Id = z.Employee.Id,
                            Employee = z.Employee,
                            PayMonth = a.PayMonth,
                            ProcessMonth = a.ProcessMonth,
                            HourDays = a.HourDays,
                            Reason = a.Reason,
                            Salcode = a.SalaryHead != null ? a.SalaryHead.Code : null,
                            PayProcessGroup_Id = z.Employee.EmpOffInfo == null ? 0 : z.Employee.EmpOffInfo.PayProcessGroup == null ? 0 : z.Employee.EmpOffInfo.PayProcessGroup.Id
                        };

                        model.Add(view);
                    }

                }

                SalAttendanceT = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        if (gp.searchField == "EmpCode")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
                        if (gp.searchField == "EmpName")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.FullNameFML.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "PayMonth")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.PayMonth.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "ProcessMonth")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.ProcessMonth.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "HourDays")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.HourDays.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Reason")
                            jsonData = IE.Select(a => new { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).Where((e => (e.Reason.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalAttendanceT;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "ProcessMonth" ? c.ProcessMonth.ToString() :
                                         gp.sidx == "HourDays" ? c.HourDays.ToString() :
                                         gp.sidx == "Reason" ? c.Reason.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id }).ToList();
                    }
                    totalRecords = SalAttendanceT.Count();
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
        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;
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

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).SingleOrDefault();

                var emp = empdata.EmployeePayroll.Select(e => e.Employee).ToList();

                foreach (var z in emp)
                {

                    view = new P2BCrGridData()
                    {
                        Id = z.Id,
                        Code = z.EmpCode,
                        Name = z.EmpName.FullNameFML
                    };
                    if (z.ServiceBookDates.ServiceLastDate == null)
                    {
                        model.Add(view);
                    }
                    else if (Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    {
                        model.Add(view);
                    }
                    //else if (z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != PayMonth)
                    //{
                    //    model.Add(view);
                    //}


                }
                EmpList = model;

                IEnumerable<P2BCrGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<P2BCrGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => 
                                         gp.sidx == "EmployeeCode" ? c.Code.ToString() :
                                         gp.sidx == "EmployeeName" ? c.Name.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.Id }).ToList();
                    }
                    totalRecords = EmpList.Count();
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
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "HOURLY" || e.Frequency.LookupVal.ToUpper() == "DAILY").ToList();


                // var qurey = db.SalaryHead.Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string PayMonth { get; set; }
            public string ProcessMonth { get; set; }
            public Double HourDays { get; set; }
            public string Reason { get; set; }
            public int PayProcessGroup_Id { get; set; }
            public string Salcode { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BGridData> SalAttendanceT = null;
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


                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                                .Include(e => e.Employee.EmpOffInfo.PayProcessGroup).ToList();

                foreach (var z in OEmployee)
                {
                    var OSalAttendance = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.FunctAttendanceT)
                                        .SingleOrDefault();


                    DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in OSalAttendance)
                    {
                        if (a.PayMonth == PayMonth)
                        {
                            Eff_Date = Convert.ToDateTime(a.PayMonth);
                            var aa = db.FunctAttendanceT.Where(e => e.Id == a.Id).SingleOrDefault();
                            view = new P2BGridData()
                            {
                                Id = z.Employee.Id,
                                Employee = z.Employee,
                                PayMonth = a.PayMonth,
                                ProcessMonth = a.ProcessMonth,
                                HourDays = a.HourDays,
                                Reason = a.Reason,
                                Salcode = a.SalaryHead != null ? a.SalaryHead.Code : null,
                                PayProcessGroup_Id = z.Employee.EmpOffInfo == null ? 0 : z.Employee.EmpOffInfo.PayProcessGroup == null ? 0 : z.Employee.EmpOffInfo.PayProcessGroup.Id
                            };

                            model.Add(view);
                        }
                    }

                }

                SalAttendanceT = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>  (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                             || (e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                             || (e.PayMonth.ToString().Contains(gp.searchString))
                             || (e.ProcessMonth.ToString().Contains(gp.searchString))
                             || (e.HourDays.ToString().Contains(gp.searchString))
                             || (e.Reason.ToString().Contains(gp.searchString))
                             || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalAttendanceT;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "ProcessMonth" ? c.ProcessMonth.ToString() :
                                         gp.sidx == "HourDays" ? c.HourDays.ToString() :
                                         gp.sidx == "Reason" ? c.Reason.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.ProcessMonth, a.HourDays, a.Reason, a.PayProcessGroup_Id, a.Id }).ToList();
                    }
                    totalRecords = SalAttendanceT.Count();
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


        public class EditData
        {
            public int Id { get; set; }
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string PayMonth { get; set; }
            public string ProcessMonth { get; set; }
            public bool Editable { get; set; }
            public Double HourDays { get; set; }
            public string Reason { get; set; }
            public string Salcode { get; set; }
        }


        public class DeserializeClass
        {
            public String Id { get; set; }
            public Double HourDays { get; set; }
            public string EmpCode { get; set; }
            public string Reason { get; set; }
        }


        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> SalAttendanceT = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();
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
                var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(o => o.FunctAttendanceT).Include(e => e.FunctAttendanceT.Select(a => a.SalaryHead)).ToList();



                foreach (var z in OEmployeePayroll)
                {
                    foreach (var S in z.FunctAttendanceT.Where(e => e.PayMonth == PayMonth && e.isCancel == false && e.TrClosed == true && e.TrReject == false))
                    {
                        
                            bool EditAppl = true;
                            view = new EditData()
                            {
                                Id = S.Id,
                                EmpName = z.Employee.EmpName.FullNameFML != null ? Convert.ToString(z.Employee.EmpName.FullNameFML) : "",
                                EmpCode = z.Employee.EmpCode != null ? z.Employee.EmpCode : null,
                                HourDays = S.HourDays,
                                Reason = S.Reason != null ? Convert.ToString(S.Reason) : "",
                                Salcode = S.SalaryHead != null ? S.SalaryHead.Code : null,
                                PayMonth = S.PayMonth != null ? Convert.ToString(S.PayMonth) : "",
                                Editable = EditAppl
                            };
                            model.Add(view);
                        

                    }
                }

                SalAttendanceT = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                               || (e.EmpName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Salcode.ToString().Contains(gp.searchString))
                               || (e.PayMonth.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.HourDays.ToString().Contains(gp.searchString))
                               || (e.Reason.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                               || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.Id.ToString().Contains(gp.searchString))
                               ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.PayMonth, a.HourDays, a.Reason, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.PayMonth, a.HourDays, a.Reason, a.Editable,a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalAttendanceT;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "Salcode" ? c.Salcode.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "HourDays" ? c.HourDays.ToString() :
                                         gp.sidx == "Reason" ? c.Reason.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.PayMonth, a.HourDays, a.Reason, a.Editable,a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.PayMonth, a.HourDays, a.Reason, a.Editable,a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.Salcode, a.PayMonth, a.HourDays, a.Reason, a.Editable,a.Id }).ToList();
                    }
                    totalRecords = SalAttendanceT.Count();
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

        #endregion

      

        #region Create
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(FunctAttendanceT S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Empstruct_drop = form["Empstruct_drop"] == "0" ? "" : form["Empstruct_drop"];

                    List<int> ids = null;
                    if (Emp != "null" && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                        if (Empstruct_drop != null && Empstruct_drop != "" && Empstruct_drop != "-Select-")
                        {
                            var value = db.EmpSalStruct.Find(int.Parse(Empstruct_drop));
                            S.EmpSalStruct = value;
                        }
                        else
                        {
                            Msg.Add("  Please Select the Process Month And Load the Structure...!  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string PayProcessgropp = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];

                    string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];
                    if (ProcessMonth == "")
                    {
                        Msg.Add(" Kindly Select Process Month");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    if (PayMonth == "")
                    {
                        Msg.Add(" Kindly Select Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string HourDays = form["Create_HourDays"] == "0" ? "" : form["Create_HourDays"];
                    if (HourDays == "")
                    {
                        Msg.Add(" Kindly Select Hour Days ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string Reason = form["Create_Reason"] == "0" ? "" : form["Create_Reason"];

                    List<int> SalHead = null;
                    string SalaryHead = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    if (SalaryHead != null && SalaryHead != "")
                    {
                        SalHead = one_ids(SalaryHead);
                    }
                    else
                    {
                        Msg.Add("  Kindly select Value For SalaryHead  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                    string Todate = form["Todate"] == "0 " ? "" : form["Todate"];

                    var EmpVariable = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                                    .Where(r => r.Id == 1).SingleOrDefault();

                    DateTime prevmonth = DateTime.Parse(PayMonth);
                    prevmonth = prevmonth.AddMonths(-1);
                    string prev = prevmonth.ToString("MM/yyyy");
                    //int month = prevmonth.Month;
                    //int year = prevmonth.Year;
                    //var prevm = month + "/" + year;
                    List<String> paychk = ProcessMonth.Split('/').Select(e => e).ToList();
                    int monch = Convert.ToInt32(paychk[0]);
                    int yerch= Convert.ToInt32(paychk[1]);
                    double days = DateTime.DaysInMonth(yerch, monch);
                    var OPayProcGrp = db.PayProcessGroup.Include(e => e.PayMonthConcept).Include(e => e.PayrollPeriod).Include(e => e.PayFrequency).SingleOrDefault();
                    if (OPayProcGrp != null)
                    {
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "FIXED30DAYS")
                        {

                            if (monch == 2)
                            {
                                if (days == 28 || days == 29)
                                {
                                    days = 30;
                                }
                            }
                        }
                        if (OPayProcGrp.PayMonthConcept.LookupVal.ToString().ToUpper() == "30DAYS")
                        {
                            if (monch == 2)
                            {
                                if (days == 28 || days == 29)
                                {
                                    days = 30;
                                }
                            }
                        }
                    }
                    double HourDaysch =Convert.ToDouble(HourDays);
                   
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;

                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            foreach (int SH in SalHead)
                            {
                                var val = db.SalaryHead.Include(e=>e.Frequency).Where(e=>e.Id==SH).FirstOrDefault();
                                if (val.Frequency.LookupVal.ToUpper()=="DAILY")
                                {
                                    if (HourDaysch > days)
                                    {
                                        Msg.Add("Check Hour Days");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).AsNoTracking().SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Include(a => a.SalaryT).Where(e => e.Employee.Id == i).AsNoTracking().SingleOrDefault();

                                var v = OEmployeePayroll.SalaryT.Where(a => a.ReleaseDate == null && a.PayMonth == prev).FirstOrDefault();
                                if (v != null)
                                {
                                    Msg.Add("Lock the Salary for month=" + prev);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id)
                                    .Include(e => e.FunctAttendanceT)
                                    .Include(e => e.FunctAttendanceT.Select(a => a.SalaryHead)).AsNoTracking()
                                    .SingleOrDefault();
                                var EmpSalT = OEmpSalT.FunctAttendanceT != null ? OEmpSalT.FunctAttendanceT.Where(e => e.PayMonth == PayMonth && e.isCancel == false && e.TrReject == false && e.TrClosed == true && e.SalaryHead.Id == SH && ProcessMonth.Contains(e.ProcessMonth)) : null;
                                if (EmpSalT != null && EmpSalT.Count() > 0)
                                {
                                    if (EmpCode == null || EmpCode == "")
                                    {
                                        EmpCode = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                                    }
                                }

                                var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).AsNoTracking().SingleOrDefault();
                                var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                                if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                                {
                                    if (EmpRel == null || EmpRel == "")
                                    {
                                        EmpRel = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                                    }
                                }
                            }
                        }
                    }
                    if (EmpCode != null)
                        return Json(new { success = true, responseText = "FunctAttendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "", "Attendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);

                    if (EmpRel != null)
                        return Json(new { success = true, responseText = "Salary released for employee " + EmpRel + ". You can't change Functional Attendance now." }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                    if (PayMonth != null && PayMonth != "")
                    {
                        S.PayMonth = PayMonth;
                        int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                        int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);
                        //S.MonthDays = DaysInMonth;
                    }

                    if (ProcessMonth != null && ProcessMonth != "")
                    {
                        S.ProcessMonth = ProcessMonth;
                    }
                    else
                    {
                        Msg.Add("  Kindly select the process month and load the structure  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (fromdate != null && fromdate != "")
                    {
                        var value = Convert.ToDateTime(fromdate);
                        S.FromDate = value;
                    }
                    if (Todate != null && Todate != "")
                    {
                        var value = Convert.ToDateTime(Todate);
                        S.ToDate = value;
                    }
                    if (HourDays != null && HourDays != "")
                    {
                        double val = Convert.ToDouble(HourDays);
                        S.HourDays = val;
                    }
                    if (Reason != null && Reason != "")
                    {
                        var val = Reason;
                        S.Reason = val;
                    }
                    DateTime Todaydate = DateTime.Now;

                   


                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            foreach (int SH in SalHead)
                            {
                                S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                FunctAllWFDetails FunctAllWFDetails = new FunctAllWFDetails
                                {
                                    WFStatus = 5,
                                    Comments = S.Reason == null ? "" : S.Reason.ToString(),
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                };

                                List<FunctAllWFDetails> FunctAllWFDetails_List = new List<FunctAllWFDetails>();
                                FunctAllWFDetails_List.Add(FunctAllWFDetails);

                                var val = db.SalaryHead.Find(SH);
                                S.SalaryHead = val;
                                FunctAttendanceT ObjFAT = new FunctAttendanceT();
                                {
                                    ObjFAT.PayMonth = S.PayMonth;
                                    ObjFAT.HourDays = S.HourDays;
                                    ObjFAT.ProcessMonth = S.ProcessMonth;
                                    ObjFAT.SalaryHead = S.SalaryHead;
                                    ObjFAT.Reason = S.Reason;
                                    ObjFAT.ReqDate = Todaydate;
                                    ObjFAT.FromDate = S.FromDate;
                                    ObjFAT.ToDate = S.ToDate;
                                    ObjFAT.TrClosed = true;
                                    //OEmpSalStruct.PayStruct = db.PayStruct.Find( OEmployeePayroll.PayStruct.Id);
                                    ObjFAT.DBTrack = S.DBTrack;
                                    ObjFAT.FunctAllWFDetails = FunctAllWFDetails_List;

                                }
                              //  int Id = int.Parse(Empstruct_drop);

                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeePayroll
                                .Include(e=>e.FunctAttendanceT)
                                .Include(e => e.EmpSalStruct)
                              .Where(e => e.Employee.Id == i).SingleOrDefault();

                                
                                DateTime Fromdate = (Convert.ToDateTime("01/" + S.ProcessMonth).Date);
                                DateTime Todatee = Convert.ToDateTime("01/" + S.ProcessMonth).AddMonths(1).AddDays(-1).Date;

                                var EmpSalStructTotal = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && e.EffectiveDate >= Fromdate && e.EffectiveDate <= Todatee).OrderByDescending(r => r.Id).FirstOrDefault();
                                                         
                              
                                int Id = 0;
                                if (EmpSalStructTotal!=null)
                                {
                                    Id = EmpSalStructTotal.Id;
                                }
                              

                                ObjFAT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                                //OEmpSalStruct.GeoStruct = db.GeoStruct.Find( OEmployeePayroll.GeoStruct.Id);

                                ObjFAT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                                ObjFAT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);

                                var k = db.EmpSalStruct.Where(e => e.Id == Id).Select(e => new
                                {
                                    EffectiveDate = e.EffectiveDate,
                                    EndDate = e.EndDate,
                                    EffectiveDate_EndDate = e.EffectiveDate != null ? e.EffectiveDate.ToString() : "" + e.EndDate != null ? e.EndDate.ToString() : ""
                                }).SingleOrDefault();
                                var Q = db.EmployeePayroll.Where(e => e.Employee.Id == i).Select(e => e.EmpSalStruct.Where(r => r.EffectiveDate == k.EffectiveDate && r.EndDate == k.EndDate)).SingleOrDefault();
                                var empsal_Id = Q.Select(r => r.Id).SingleOrDefault();
                                ObjFAT.EmpSalStruct = db.EmpSalStruct.Find(empsal_Id);


                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {
                                        db.FunctAttendanceT.Add(ObjFAT);
                                        db.SaveChanges();
                                        List<FunctAttendanceT> OFAT = new List<FunctAttendanceT>();
                                        OFAT.Add(db.FunctAttendanceT.Find(ObjFAT.Id));

                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                FunctAttendanceT = OFAT,
                                                DBTrack = S.DBTrack

                                            };
                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            OFAT.AddRange(aa.FunctAttendanceT);
                                            aa.FunctAttendanceT = OFAT;
                                            //OEmployeePayroll.DBTrack = dbt;

                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();

                                    }
                                    catch (Exception ex)
                                    {
                                        //   List<string> Msg = new List<string>();
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
                                        //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }


                                }
                            }
                        }
                        // List<string> Msgs = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { success = true, responseText = "Data Saved Successfully...", JsonRequestBehavior.AllowGet });
                    }
                    // List<string> Msg = new List<string>();
                    Msg.Add("  Unable to create...  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { success = false, responseText = "Unable to create...", JsonRequestBehavior.AllowGet });
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        #endregion

        #region EDIT & EDITSAVE

        public async Task<ActionResult> EditSave(FunctAttendanceT F, String forwarddata, FormCollection form, String PayMonth) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var serialize = new JavaScriptSerializer();
                var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                if (obj.Count < 0)
                {
                    return Json(new { sucess = false, responseText = "You have to change days to update Functattendance." }, JsonRequestBehavior.AllowGet);
                }
                List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
                List<string> EmpCodes = obj.Select(e => e.EmpCode).ToList();
                // string PayMonth = form["PayMonth"] != "" ? form["PayMonth"] : "";
                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                string EmpCode = null;
                foreach (string ca in EmpCodes)
                {
                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                .Where(r => r.EmpCode == ca).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                    var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                    var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                    if (EmpSalT.Count() > 0)
                    {
                        if (EmpCode == null || EmpCode == "")
                        {
                            EmpCode = ca;
                        }
                        else
                        {
                            EmpCode = EmpCode + ", " + ca;
                        }
                    }

                    //var EmpSalRel = db.EmployeePayroll.Where()
                }

                if (EmpCode != null)
                    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ". You can't change Functattendance now." }, JsonRequestBehavior.AllowGet);
                    return Json(new { success = true, responseText = "Salary released for employee " + EmpCode + ". You can't change Function attendance now." }, JsonRequestBehavior.AllowGet);

                using (TransactionScope ts = new TransactionScope())
                {
                    try
                    {
                        foreach (int ca in ids)
                        {
                            FunctAttendanceT SalAttT = db.FunctAttendanceT.Find(ca);
                            SalAttT.HourDays = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.HourDays).Single());
                            SalAttT.Reason = obj.Where(e => e.Id == ca.ToString()).Select(e => e.Reason).Single();
                            db.FunctAttendanceT.Attach(SalAttT);
                            db.Entry(SalAttT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(SalAttT).State = System.Data.Entity.EntityState.Detached;
                        }

                        foreach (string ca in EmpCodes)
                        {
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.EmpCode == ca).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                            var OEmpSalT = db.EmployeePayroll.AsNoTracking().Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                            var EmpSalDelT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault() : null;

                            if (EmpSalDelT != null)
                            {
                                SalaryGen.DeleteSalary(EmpSalDelT.Id, PayMonth);
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
                        //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    ts.Complete();
                    return this.Json(new Object[] { "", "", "Functional Attendance Updated Successsfully... " }, JsonRequestBehavior.AllowGet);
                    //    List<string> Msgs = new List<string>();
                    //    Msgs.Add("Functional Attendance Updated Successsfully... ");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                }

                return View();
            }
        }

        #endregion

        #region Delete

        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //        {
        //            try
        //            {
        //                var OSalAttendance = db.FunctAttendanceT.Where(e => e.Id == data)
        //                                   .SingleOrDefault();
                     
        //               // var emp_ids=db.FunctAttendanceT.
        //                FunctAttendanceT FunctAttendanceT = db.FunctAttendanceT.Find(data);
        //                FunctAttendanceT functwfdetails = db.FunctAttendanceT.Include(e => e.FunctAllWFDetails)
        //                                    .Where(e => e.Id == data).FirstOrDefault();
        //                var functwfdetailsObj = functwfdetails.FunctAllWFDetails.ToList();

        //                var emp_ids = db.EmployeePayroll.Include(e => e.FunctAttendanceT).ToList();

        //                foreach (var a in emp_ids)
        //                {
        //                    var z = a.FunctAttendanceT.Where(e => e.Id == data);

        //                    if (z != null)
        //                    {
        //                        var sal = db.EmployeePayroll.Where(e => e.Id == a.Id).Select(r => r.SalaryT).SingleOrDefault();
        //                        foreach (var s in sal)
        //                        {
        //                            if (s.PayMonth == FunctAttendanceT.PayMonth && s.ReleaseDate != null)
        //                            {
        //                                List<string> Msgu = new List<string>();
        //                                Msgu.Add(" You can't delete functional attendance as this month salary is released. ");
        //                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
        //                            }
        //                        }

        //                    }
        //                }

        //                foreach (var functitem in functwfdetailsObj)
        //                {
        //                    FunctAllWFDetails funcTdata = functitem;
        //                    db.Entry(funcTdata).State = System.Data.Entity.EntityState.Deleted;
        //                    //functwfdetails.FunctAllWFDetails.Remove(funcTdata);
        //                }



        //                db.Entry(functwfdetails).State = System.Data.Entity.EntityState.Deleted;
        //                await db.SaveChangesAsync();
        //                ts.Complete();
        //                Msg.Add("  Data removed successfully.  ");
        //                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //            catch (RetryLimitExceededException /* dex */)
        //            {
        //                return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //            }
        //            catch (Exception ex)
        //            {
        //                LogFile Logfile = new LogFile();
        //                ErrorLog Err = new ErrorLog()
        //                {
        //                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                    ExceptionMessage = ex.Message,
        //                    ExceptionStackTrace = ex.StackTrace,
        //                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                    LogTime = DateTime.Now
        //                };
        //                Logfile.CreateLogFile(Err);
        //                List<string> Msg = new List<string>();
        //                Msg.Add(ex.Message);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //}
        //#endregion

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var query = db.FunctAttendanceT.Include(e => e.FunctAllWFDetails).Where(e => e.Id == data && e.TrClosed == true
                                     && e.TrReject == false && e.isCancel == false).FirstOrDefault();

                        FunctAllWFDetails oFunctAllWFDetails = new FunctAllWFDetails
                        {
                            WFStatus = 6,
                            Comments = "Cancelled by Admin",
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };
                        List<FunctAllWFDetails> oFuncAllWFDetails_List = new List<FunctAllWFDetails>();
                        if (query.FunctAllWFDetails.Count() > 0)
                        {
                            oFuncAllWFDetails_List.AddRange(query.FunctAllWFDetails);
                        }
                        oFuncAllWFDetails_List.Add(oFunctAllWFDetails);

                        query.isCancel = true;
                        query.TrClosed = true;
                        query.FunctAllWFDetails = oFuncAllWFDetails_List;

                        db.FunctAttendanceT.Attach(query);
                        db.Entry(query).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                        //db.SaveChanges();
                        ts.Complete();
                       
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
                        //List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Msg.Add("Data removed successfully.");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                   
                }
            }
        }
        #endregion
 
        public ActionResult ChkProcesscarry(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                if (month != null)
                {
                    //  var finanialyear = db.Calendar.Find(int.Parse(month));
                    bool selected = false;
                    // var query = db.AnnualSalaryR.Include(e => e.FinancialYear).ToList();
                    var query = db.FunctAttendanceT.Where(e => e.PayMonth == month).ToList();

                    //var financialyear = query.Select(e => e.FinancialYear == finanialyear).SingleOrDefault();
                    // var financialyearR = query.Where(f => f.FinancialYear == finanialyear);

                    if (query == null || query.Count() == 0)
                    {
                        selected = true;
                    }
                    else
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
        public ActionResult AddCarryForwad(string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime CurDate = Convert.ToDateTime("01/" + month);
                string curmon = CurDate.ToString("MM/yyyy");
                DateTime prevmn = Convert.ToDateTime("01/" + month).AddDays(-1).Date;
                string prevmnon = prevmn.ToString("MM/yyyy");

                var DataChkcur = db.SalaryT.Where(e => e.PayMonth == curmon).ToList();
                if (DataChkcur.Count > 0)
                {
                    // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "Please delete salary for month :" + curmon + ". and Try Again.." }, JsonRequestBehavior.AllowGet);

                }
                var DataChkprv = db.SalaryT.Where(e => e.PayMonth == prevmnon).ToList();
                if (DataChkprv.Count() == 0)
                {
                    // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                    return Json(new Utility.JsonReturnClass { success = true, responseText = "Please First Process salary for month :" + prevmnon + ". Then Go to Next Month" }, JsonRequestBehavior.AllowGet);

                }
           


                var PrevSal = db.SalaryT.OrderByDescending(e => e.Id).FirstOrDefault();
                DateTime PrevDate = Convert.ToDateTime("01/" + PrevSal.PayMonth);
                string Month = PrevDate.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevDate.AddMonths(1).Month.ToString() : PrevDate.AddMonths(1).Month.ToString();
                string CurMonth = Month + "/" + PrevDate.AddMonths(1).Year.ToString();
                var DataChk = db.FunctAttendanceT.Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee)
                                .Where(e => e.PayMonth == CurMonth).ToList();
                if (DataChk != null && DataChk.Count > 0)
                {
                    StringBuilder SB = new StringBuilder();
                    List<string> ListEmpcodes = new List<string>();
                    Msg.Add("Data already carry forwarded for month = " + CurMonth + " and EmpCodes are :" + "\n");
                    string EmployeeCode = "";
                   // SB.AppendLine("Data already carry forwarded for month = " + CurMonth + " and EmpCodes are :");
                    foreach (var getitem in DataChk)
                    {
                        EmployeeCode = getitem.EmployeePayroll.Employee.EmpCode;
                       // SB.Append(EmployeeCode + "\n");
                       // SB.AppendLine(EmployeeCode);
                        Msg.Add(EmployeeCode);
                        
                       
                    }
                   // var getempcodes = SB.ToString();
                    //Msg.Add(getempcodes);
                   // return this.Json(new { success = true, responseText = Msg, JsonRequestBehavior.AllowGet });
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //sb.Append(error.ErrorMessage);
                    //sb.Append("." + "\n");
                }

                var DataChk1 = db.CPIEntryT.Where(e => e.PayMonth == CurMonth).ToList();
                if (DataChk1 == null && DataChk1.Count == 0)
                {
                    return this.Json(new { success = true, responseText = "CPI Entry has not been done for =" + CurMonth, JsonRequestBehavior.AllowGet });
                }

                //if (DataChk == null && DataChk.Count == 0)
                //{
                //    return this.Json(new { success = true, responseText = "If You want to Process AddCarryForward for the Current Month :" + CurMonth + " then Click on YES or Click on NO for Previous Month  :" + prevmnon, JsonRequestBehavior.AllowGet });
                //}

               


                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 30, 0)))
                {
                    var EmpList = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.PayStruct)
                        //.Include(e => e.SalaryT)
                        //.Include(e => e.FunctAttendanceT)
                        //.Include(e => e.FunctAttendanceT.Select(r => r.SalaryHead))
                        //.Include(e => e.EmpSalStruct)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();

                    foreach (var i in EmpList)
                    {
                       int empPayrollId = i.Id;
                        #region SALARYT LOOP
                        //foreach (var j in i.SalaryT)
                        //{
                        //    var PrevMonth = i.SalaryT.OrderByDescending(e => e.Id).FirstOrDefault();
                        //    PrevDate = Convert.ToDateTime("01/" + PrevMonth.PayMonth);
                        //    var PrevFuncAtt = i.FunctAttendanceT.Where(e => e.PayMonth == PrevMonth.PayMonth).ToList();
                        //    Month = PrevDate.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevDate.AddMonths(1).Month.ToString() : PrevDate.AddMonths(1).Month.ToString();
                        //    CurMonth = Month + "/" + PrevDate.AddMonths(1).Year.ToString();

                        //    if (i.Employee.ServiceBookDates.ServiceLastDate == null || i.Employee.ServiceBookDates.ServiceLastDate != null && Convert.ToDateTime("01/" + i.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + CurMonth))
                        //    {
                        //        EmpSalStruct SalStruct = i.EmpSalStruct.Where(e => e.EffectiveDate.Value.ToString("MM/yyyy") == CurMonth).OrderByDescending(e => e.EffectiveDate).FirstOrDefault();
                               
                        //        if (SalStruct != null)
                        //        {
                        //            DateTime ToDate = Convert.ToDateTime(DateTime.DaysInMonth(SalStruct.EffectiveDate.Value.Year, SalStruct.EffectiveDate.Value.Month) + "/" + CurMonth);

                        //            foreach (var F in PrevFuncAtt)
                        //            {
                        //                DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        //                FunctAttendanceT FuncAtt = new FunctAttendanceT()
                        //                {
                        //                    DBTrack = DBTrack,
                        //                    EmpSalStruct = SalStruct,
                        //                    FromDate = SalStruct.EffectiveDate,
                        //                    ToDate = ToDate,
                        //                    FuncStruct = i.Employee.FuncStruct,
                        //                    GeoStruct = i.Employee.GeoStruct,
                        //                    HourDays = F.HourDays,
                        //                    PayMonth = CurMonth,
                        //                    PayProcessGroup = F.PayProcessGroup,
                        //                    PayStruct = i.Employee.PayStruct,
                        //                    ProcessMonth = CurMonth,
                        //                    Reason = F.Reason,
                        //                    ReqDate = DateTime.Today,
                        //                    SalaryHead = F.SalaryHead,
                        //                    TrClosed = true
                        //                };
                        //                db.FunctAttendanceT.Add(FuncAtt);
                        //                db.SaveChanges();

                        //                List<FunctAttendanceT> OFAT = new List<FunctAttendanceT>();
                        //                OFAT.Add(db.FunctAttendanceT.Find(FuncAtt.Id));

                        //                var aa = db.EmployeePayroll.Find(i.Id);
                        //                OFAT.AddRange(aa.FunctAttendanceT);
                        //                aa.FunctAttendanceT = OFAT;
                        //                //OEmployeePayroll.DBTrack = dbt;

                        //                db.EmployeePayroll.Attach(aa);
                        //                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        //                db.SaveChanges();
                        //                //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                        //            }
                        //        }
                        //    }
                        //    break;
                        //}
                        #endregion SALARYT LOOP

                      //  int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                        string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                        using (SqlConnection con = new SqlConnection(connString))
                        {
                            using (SqlCommand cmd = new SqlCommand("FunctAtt_carryforward", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.Add("@PrevMon", SqlDbType.VarChar).Value = prevmnon;
                                cmd.Parameters.Add("@CurrMon", SqlDbType.VarChar).Value = curmon;
                                cmd.Parameters.Add("@PayRollId", SqlDbType.Float).Value = empPayrollId;

                                con.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }


                    }
                    ts.Complete();
                }

                return this.Json(new { success = true, responseText = "Data carry forwarded for month :" + CurMonth, JsonRequestBehavior.AllowGet });
            }
        }
    }
}