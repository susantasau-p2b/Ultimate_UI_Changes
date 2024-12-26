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
using P2BUltimate.Process;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITaxTransTController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITaxTransT/Index.cshtml");
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
        #endregion

        #region Partialview Link
        public ActionResult partial()
        {
            return View("~/Views/Shared/_SalAttendanceT.cshtml");

        }
        #endregion

        #region P2BGridDetails
        //public ActionResult LoadEmp(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<Employee> Employee = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Employee = db.Employee.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Employee = db.Employee.Include(e => e.EmpName);
        //        }

        //        IEnumerable<Employee> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Employee;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.CardCode }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EmpCode")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpCode }).Where((e => (e.EmpCode.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "EmpName")
        //                    jsonData = IE.Select(a => new { a.Id, a.EmpName.FullNameFML }).Where((e => (e.FullNameFML.ToString().Contains(gp.searchString)))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.CardCode }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Employee;
        //            Func<Employee, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
        //                                 gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
        //                                 gp.sidx == "EmpName" ? c.EmpName.FullNameFML.ToString() : "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName.FullNameFML }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName.FullNameFML }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.EmpCode), a.EmpName.FullNameFML }).ToList();
        //            }
        //            totalRecords = Employee.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

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
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                ).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
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
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                         gp.sidx == "EmpCode" ? c.Code.ToString() :
                                         gp.sidx == "EmpName" ? c.Name.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
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
        //public JsonResult GetPayprocessgroup(string data)
        //{
        //    var a = db.PayProcessGroup.Find(int.Parse(data));
        //    return Json(a, JsonRequestBehavior.AllowGet);
        //}


        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<SalAttendanceT> SalAttendanceT = null;
        //        if (gp.IsAutho == true)
        //        {
        //            SalAttendanceT = db.SalAttendanceT.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            SalAttendanceT = db.SalAttendanceT.AsNoTracking().ToList();
        //        }

        //        IEnumerable<SalAttendanceT> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = SalAttendanceT;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Select(a => new { a.Id, a.MonthDays, a.PaybleDays }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MonthDays, a.PaybleDays }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = SalAttendanceT;
        //            Func<SalAttendanceT, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "MonthDays" ? Convert.ToString(c.MonthDays) :
        //                                 gp.sidx == "MonthYear" ?Convert.ToString(c.PaybleDays) :
        //                                 "");
        //            }

        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.MonthDays, a.PaybleDays }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.MonthDays, a.PaybleDays }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.MonthDays, a.PaybleDays }).ToList();
        //            }
        //            totalRecords = SalAttendanceT.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public class P2BGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public string PayMonth { get; set; }
            public double MonthDays { get; set; }
            public double PaybleDays { get; set; }
            public double LWPDays { get; set; }
            public int PayProcessGroup_Id { get; set; }
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
                    var OSalAttendance = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.SalAttendance)
                                        .SingleOrDefault();


                    DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in OSalAttendance)
                    {
                        if (a.PayMonth == PayMonth)
                        {
                            Eff_Date = Convert.ToDateTime(a.PayMonth);
                            var aa = db.SalAttendanceT.Where(e => e.Id == a.Id).SingleOrDefault();
                            view = new P2BGridData()
                            {
                                Id = z.Employee.Id,
                                Employee = z.Employee,
                                PayMonth = a.PayMonth,
                                MonthDays = a.MonthDays,
                                PaybleDays = a.PaybleDays,
                                LWPDays = a.LWPDays,
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
                              || (e.MonthDays.ToString().Contains(gp.searchString))
                              || (e.PaybleDays.ToString().Contains(gp.searchString))
                              || (e.LWPDays.ToString().Contains(gp.searchString))
                              || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.MonthDays, a.PaybleDays, a.LWPDays, a.PayProcessGroup_Id, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.MonthDays, a.PaybleDays, a.LWPDays, a.PayProcessGroup_Id, a.Id }).ToList();
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
                                         gp.sidx == "MonthDays" ? c.MonthDays.ToString() :
                                         gp.sidx == "PaybleDays" ? c.PaybleDays.ToString() :
                                         gp.sidx == "LWPDays" ? c.LWPDays.ToString() :
                                         gp.sidx == "PayProcessGroup_Id" ? c.PayProcessGroup_Id.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.MonthDays, a.PaybleDays, a.LWPDays, a.PayProcessGroup_Id, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.MonthDays, a.PaybleDays, a.LWPDays, a.PayProcessGroup_Id, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PayMonth, a.MonthDays, a.PaybleDays, a.LWPDays, a.PayProcessGroup_Id, a.Id }).ToList();
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
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }
            public string PayMonth { get; set; }
            public string Surcharge { get; set; }
            public string TaxOnIncome { get; set; }
            public string TaxPaid { get; set; }
            public string Mode { get; set; }
            public bool Editable { get; set; }
        }
        public class DeserializeClass
        {
            public String Id { get; set; }
            public double TaxPaid { get; set; }
            public string EmpCode { get; set; }

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
                IEnumerable<EditData> ITaxTranst = null;

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
                    .Include(o => o.ITaxTransT).ToList();



                foreach (var z in OEmployeePayroll)
                {
                    foreach (var S in z.ITaxTransT)
                    {
                        if (S.PayMonth == PayMonth)
                        {


                            bool EditAppl = true;
                            view = new EditData()
                            {
                                Id = S.Id,
                                EmployeeCode = z.Employee.EmpCode != null ? Convert.ToString(z.Employee.EmpCode) : null,
                                EmployeeName = z.Employee.EmpName.FullNameFML != null ? Convert.ToString(z.Employee.EmpName.FullNameFML) : null,
                                PayMonth = S.PayMonth,
                                //  Surcharge = S.Surcharge.ToString(),
                                //  TaxOnIncome = S.TaxOnIncome.ToString(),
                                TaxPaid = S.TaxPaid.ToString(),
                                //  Mode = S.Mode,
                                Editable = EditAppl
                            };
                            model.Add(view);
                        }


                    }
                }

                ITaxTranst = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITaxTranst;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e =>  (e.EmployeeCode != null ? e.EmployeeCode.ToString().Contains(gp.searchString) : false) 
                                        || (e.EmployeeName != null ? e.EmployeeName.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                        || (e.PayMonth != null ? e.PayMonth.ToString().Contains(gp.searchString.ToUpper()) : false)
                                        || (e.TaxPaid != null ? e.TaxPaid.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                        || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                        || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                                  ).Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();


                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITaxTranst;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmployeeCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmployeeName.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "TaxPaid" ? c.TaxPaid.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() :
                                        "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    totalRecords = ITaxTranst.Count();
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

        public ActionResult Create(ITaxTransT ITX, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    // string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string PayProcessgropp = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                    string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    string PayableDays = form["Create_PayableDays"] == "0" ? "" : form["Create_PayableDays"];
                  

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;
                    int chkitmon = db.ITaxTransT.Where(e => e.PayMonth == PayMonth).Count();
                    if (chkitmon > 0)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.AsNoTracking().Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                           .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.AsNoTracking().Where(e => e.Employee.Id == i).SingleOrDefault();

                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.ITaxTransT).AsNoTracking().SingleOrDefault();
                                var EmpSalT = OEmpSalT.ITaxTransT != null ? OEmpSalT.ITaxTransT.Where(e => e.PayMonth == PayMonth) : null;
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

                                var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.ITaxTransT).AsNoTracking().SingleOrDefault();
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
                    {
                        Msg.Add(" ITaxTrans already exists for employee " + EmpCode + ". ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    // return this.Json(new Object[] { "", "", "ITaxTrans already exists for employee " + EmpCode + ".", JsonRequestBehavior.AllowGet });


                    if (EmpRel != null)
                    {
                        Msg.Add(" Salary released for employee " + EmpRel + ". You can't change ITaxTrans now. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return this.Json(new Object[] { "", "", "Salary released for employee " + EmpRel + ". You can't change ITaxTrans now.", JsonRequestBehavior.AllowGet });
                        // return Json(new { success = true, responseText = "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                    }
                    if (PayMonth != null && PayMonth != "")
                    {
                        ITX.PayMonth = PayMonth;
                        int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                        int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);

                    }

                    if (PayableDays != null && PayableDays != "")
                    {
                        var Payable_days = int.Parse(PayableDays);
                        //ITX.PaybleDays = Payable_days;
                    }


                    ITX.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    ITaxTransT ObjITX = new ITaxTransT();
                    {
                        ObjITX.PayMonth = ITX.PayMonth;
                        ObjITX.TaxPaid = ITX.TaxPaid;
                        ObjITX.Mode = "MAN";
                        //ObjFAT.PaybleDays = ITX.PaybleDays;
                        //ObjFAT.MonthDays = ITX.MonthDays;

                        //OEmpSalStruct.PayStruct = db.PayStruct.Find( OEmployeePayroll.PayStruct.Id);
                        ObjITX.DBTrack = ITX.DBTrack;

                    }
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).AsNoTracking()
                                        .Where(r => r.Id == i).SingleOrDefault();

                            OEmployeePayroll
                            = db.EmployeePayroll.AsNoTracking()
                          .Where(e => e.Employee.Id == i).SingleOrDefault();

                            ObjITX.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                            //OEmpSalStruct.GeoStruct = db.GeoStruct.Find( OEmployeePayroll.GeoStruct.Id);

                            ObjITX.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                            ObjITX.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);


                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    db.ITaxTransT.Add(ObjITX);
                                    db.SaveChanges();
                                    List<ITaxTransT> OITAX = new List<ITaxTransT>();
                                    OITAX.Add(db.ITaxTransT.Find(ObjITX.Id));

                                    if (OEmployeePayroll == null)
                                    {
                                        EmployeePayroll OTEP = new EmployeePayroll()
                                        {
                                            Employee = db.Employee.Find(OEmployee.Id),
                                            ITaxTransT = OITAX,
                                            DBTrack = ITX.DBTrack
                                        };
                                        db.EmployeePayroll.Add(OTEP);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                      //  OITAX.AddRange(aa.ITaxTransT);
                                        aa.ITaxTransT = OITAX;
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
                                    // return this.Json(new Object[] { "", "", Msg, JsonRequestBehavior.AllowGet });
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                            }
                        }
                        //return this.Json(new Object[] { "", "", "Data Saved Successfully...", JsonRequestBehavior.AllowGet });
                        //    List<string> Msgs = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    // List<string> Msgu = new List<string>();
                    Msg.Add("Unable to create...");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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


        //public JsonResult Edit(string data, string data1)
        //{
        //    int Id = Convert.ToInt32(data);

        //    var aa = db.EmployeePayroll.Where(e => e.Employee.Id == Id).Select(e => e.SalAttendance).SingleOrDefault();

        //    var Q = db.SalAttendanceT
        //        .Where(e => e.Id == ID).Select
        //        (e => new
        //        {
        //            PaybleDays = e.PaybleDays,
        //            PayMonth = e.PayMonth,
        //            LWPDays = e.LWPDays,
        //            Action = e.DBTrack.Action
        //        }).ToList();


        //    //var SalAtt = db.SalAttendanceT.Find(ID);
        //    //TempData["RowVersion"] = SalAtt.RowVersion;
        //    //var Auth = SalAtt.DBTrack.IsModified;
        //    return Json(new Object[] { Q, "", "" }, JsonRequestBehavior.AllowGet);
        //}

        public async Task<ActionResult> EditSave(ITaxTransT ITX, String forwarddata, FormCollection form, String PayMonth) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var serialize = new JavaScriptSerializer();

                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                    if (obj.Count < 0)
                    {
                        //   return Json(new { sucess = false, responseText = "You have to change  to update ITax." }, JsonRequestBehavior.AllowGet);
                        Msg.Add(" You have to change  to update ITax.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<string> EmpCodes = obj.Select(e => e.EmpCode).ToList();
                    //string PayMonth = form["PayMonth"] != "" ? form["PayMonth"] : "";
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    foreach (string ca in EmpCodes)
                    {
//                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.EmpCode == ca).SingleOrDefault();
                        OEmployee = db.Employee.Where(r => r.EmpCode == ca).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                        var OEmpSalT = db.EmployeePayroll.AsNoTracking().Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
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
                    {
                        // Msg.Add(" Salary released for employee " + EmpCode + ". You can't change Income tax now. ");
                        //  return Json(new  { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Income tax now" }, JsonRequestBehavior.AllowGet);
                    }
                    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);

                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            foreach (int ca in ids)
                            {

                                ITaxTransT ITaxT = db.ITaxTransT.Find(ca);
                                ITaxT.TaxPaid = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.TaxPaid).Single());
                                db.ITaxTransT.Attach(ITaxT);
                                db.Entry(ITaxT).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(ITaxT).State = System.Data.Entity.EntityState.Detached;

                            }
                            foreach (string ca in EmpCodes)
                            {
                               // OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.EmpCode == ca).SingleOrDefault();

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
                            // List<string> Msg = new List<string>();
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
                        //  List<string> Msg = new List<string>();
                        Msg.Add("Data Saved successfully");
                        // return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    // return View();
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

        #region Delete

       // [HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                try
        //                {
        //                    //var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.SalAttendance).SingleOrDefault();
        //                    //SalAttendanceT SalAttendanceT = Emp.Where(e => e.LWPDays == null).SingleOrDefault();
        //                    ITaxTransT ITAX = db.ITaxTransT.Find(data);
        //                    db.Entry(ITAX).State = System.Data.Entity.EntityState.Deleted;
        //                    await db.SaveChangesAsync();
        //                    ts.Complete();
        //                    //  Msg.Add("  Data removed successfully.  ");

        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //                catch (RetryLimitExceededException /* dex */)
        //                {
        //                    // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
        //                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //                catch (Exception ex)
        //                {
        //                    LogFile Logfile = new LogFile();
        //                    ErrorLog Err = new ErrorLog()
        //                    {
        //                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                        ExceptionMessage = ex.Message,
        //                        ExceptionStackTrace = ex.StackTrace,
        //                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                        LogTime = DateTime.Now
        //                    };
        //                    Logfile.CreateLogFile(Err);
        //                    // List<string> Msg = new List<string>();
        //                    Msg.Add(ex.Message);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public async Task<ActionResult> Delete(ITaxTransT ITX, String forwarddata, FormCollection form, String PayMonth) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    JavaScriptSerializer serialize = new JavaScriptSerializer();

                    var obj = (DeserializeClass)serialize.Deserialize<DeserializeClass>(forwarddata);

                    if (obj.EmpCode == "")
                    {
                        //   return Json(new { sucess = false, responseText = "You have to change  to update ITax." }, JsonRequestBehavior.AllowGet);
                        Msg.Add(" You have to change  to update ITax.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    int ids = Convert.ToInt32(obj.Id);
                    string EmpCodes = obj.EmpCode;
                    //string PayMonth = form["PayMonth"] != "" ? form["PayMonth"] : "";
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    //foreach (string ca in EmpCodes)
                    //{
                    OEmployee = db.Employee.Where(r => r.EmpCode == EmpCodes).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                    var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                    var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                    if (EmpSalT.Count() > 0)
                    {
                        if (EmpCode == null || EmpCode == "")
                        {
                            EmpCode = EmpCodes;
                        }
                        else
                        {
                            EmpCode = EmpCode + ", " + EmpCodes;
                        }
                    }

                    //var EmpSalRel = db.EmployeePayroll.Where()
                    // }

                    if (EmpCode != null)
                    {
                        // Msg.Add(" Salary released for employee " + EmpCode + ". You can't change Income tax now. ");
                        //  return Json(new  { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Income tax now" }, JsonRequestBehavior.AllowGet);
                    }
                    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);

                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            //foreach (int ca in ids)
                            //{

                            ITaxTransT ITaxT = db.ITaxTransT.Find(ids);
                            db.Entry(ITaxT).State = System.Data.Entity.EntityState.Deleted;
                            //    ITaxT.TaxPaid = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.TaxPaid).Single());
                            //    db.ITaxTransT.Attach(ITaxT);
                            // db.Entry(ITaxT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            //    db.Entry(ITaxT).State = System.Data.Entity.EntityState.Detached;

                            //}
                            //foreach (string ca in EmpCodes)
                            //{
                            OEmployee = db.Employee.Where(r => r.EmpCode == EmpCodes).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                            var OEmpSalT1 = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                            var EmpSalDelT = OEmpSalT1.SalaryT != null ? OEmpSalT1.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault() : null;

                            if (EmpSalDelT != null)
                            {
                                SalaryGen.DeleteSalary(EmpSalDelT.Id, PayMonth);
                            }
                            // }
                        }
                        catch (Exception ex)
                        {
                            // List<string> Msg = new List<string>();
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
                        //  List<string> Msg = new List<string>();
                        Msg.Add("Recored Remove successfully");
                        // return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        // return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Recored Remove Successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    // return View();
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

        #region TreeGridDetails
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var db_data = db.YearlyPaymentT.Where(e => e.Id == data).SingleOrDefault();
                if (db_data != null)
                {
                    db.YearlyPaymentT.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    Msg.Add("  Data removed successfully.  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new { data = "", responseText = "Record Deleted..!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.YearlyPaymentT).ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompaines = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompaines.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.CardCode,

                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompaines

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class YearlyPaymentChildDataClass
        {
            public String Id { get; set; }
            public String TDSAmount { get; set; }
            public String AmonutPaid { get; set; }
            public String OtherDeduction { get; set; }
            public String Narration { get; set; }
        }
        public ActionResult Get_YearlyPayment(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.YearlyPaymentT)
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<YearlyPaymentChildDataClass> returndata = new List<YearlyPaymentChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.YearlyPaymentT))
                        {
                            returndata.Add(new YearlyPaymentChildDataClass
                            {
                                Id = item.Id.ToString(),
                                AmonutPaid = item.AmountPaid.ToString(),
                                OtherDeduction = item.OtherDeduction.ToString(),
                                Narration = item.Narration,
                                TDSAmount = item.TDSAmount.ToString()
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public ActionResult GridEditSave(YearlyPaymentT ypay, FormCollection from, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.YearlyPaymentT.Where(e => e.Id == id).SingleOrDefault();
                    db_data.AmountPaid = ypay.AmountPaid;
                    try
                    {
                        db.YearlyPaymentT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { data = db_data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
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
                else
                {
                    Msg.Add("  Data Is Null  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

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
                    var query = db.ITaxTransT.Where(e => e.PayMonth == month).ToList();

                    //var financialyear = query.Select(e => e.FinancialYear == finanialyear).SingleOrDefault();
                   // var financialyearR = query.Where(f => f.FinancialYear == finanialyear);

                    if (query == null || query.Count()==0)
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
            List<string> Msg = new List<string>();
            
                try
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {

                        DateTime _CarryMonth = Convert.ToDateTime("01/" + month);
                        DateTime prevmn = Convert.ToDateTime("01/" + month).AddDays(-1).Date;
                        string prevmnon = prevmn.ToString("MM/yyyy");
                        string curmon = _CarryMonth.ToString("MM/yyyy");
                        var DataChkprv = db.ITaxTransT.Where(e => e.PayMonth == prevmnon).ToList();
                        if ( DataChkprv.Count() == 0)
                        {
                           // Msg.Add(" Income tax Not available for month =" + prevmn.ToString("MM/yyyy"));
                            return Json(new Utility.JsonReturnClass { success = true, responseText = "Income tax Not available for month :" + prevmnon + "." }, JsonRequestBehavior.AllowGet);
                           
                        }
                        var DataChkcur = db.ITaxTransT.Where(e => e.PayMonth == curmon).ToList();
                        if (DataChkcur.Count > 0)
                        {

                            return Json(new Utility.JsonReturnClass { success = true, responseText = "Data already carry forwarded for month :" + curmon + "." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { success = true, responseText = "Data already carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
                        }
                        var DataChkcursal = db.SalaryT.Where(e => e.PayMonth == curmon).ToList();
                        if (DataChkcursal.Count > 0)
                        {

                            return Json(new Utility.JsonReturnClass { success = true, responseText = "Please Delete Salary for month :" + curmon + ". Then Try Again." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { success = true, responseText = "Data already carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
                        }

                        var _temp_Paymonth = db.ITaxTransT.AsNoTracking().Select(e => e.PayMonth).ToList();

                        DateTime PrevSal = Utility._Convert_PayMonth_To_DateTime(_temp_Paymonth).OrderByDescending(e => e.Date).FirstOrDefault();
                        var PrevSal_string = Utility._Convert_DateTime_To_PayMonth(new List<DateTime>() { PrevSal }).LastOrDefault();

                        //  var PrevSal = db.SalaryT.OrderByDescending(e => e.Id).FirstOrDefault();
                        // DateTime PrevDate = Convert.ToDateTime("01/" + PrevSal.PayMonth);
                        string Month = PrevSal.AddMonths(1).Month.ToString().Length == 1 ? "0" + PrevSal.AddMonths(1).Month.ToString() : PrevSal.AddMonths(1).Month.ToString();
                        string CurMonth = Month + "/" + PrevSal.AddMonths(1).Year.ToString();
                        var DataChk = db.ITaxTransT.Where(e => e.PayMonth == CurMonth).ToList();

                        if (DataChk != null && DataChk.Count > 0)
                        {
                            Msg.Add(" Data already carry forwarded for month =" + CurMonth);
                            return Json(new Utility.JsonReturnClass { success = true, responseText = "Data already carry forwarded for month :" + CurMonth + "." }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { success = true, responseText = "Data already carry forwarded for month =" + CurMonth, JsonRequestBehavior.AllowGet });
                        }



                        //var EmpList = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Include(e => e.Employee.FuncStruct)
                        //                .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.PayStruct)
                        //              .Include(e => e.ITaxTransT).ToList();
                        var TEST = db.ITaxTransT.Include(e => e.EmployeePayroll.Employee.ServiceBookDates)
                          .Where(e => e.PayMonth == PrevSal_string ).AsNoTracking().ToList();
                        foreach (var i in TEST)
                        {
                          //  var PrevITtaxTransT = i.ITaxTransT.Where(e => e.PayMonth == PrevSal_string).FirstOrDefault();

                            var test = db.EmployeePayroll
                     .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
                    .Where(p => p.p.Id == i.EmployeePayroll.Id).AsNoTracking()
                    .Select(m => new
                    {
                        GeoStruct_id = m.pc.GeoStruct.Id,
                        PayStruct_id = m.pc.PayStruct.Id,
                        FuncStruct_id = m.pc.FuncStruct.Id,
                        Id = m.p.Id
                    }).FirstOrDefault();

                            if (i.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate == null || i.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate != null && Convert.ToDateTime("01/" + i.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + CurMonth))
                            {
                               
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                  new System.TimeSpan(0, 30, 0)))
                                {
                                   
                                        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        ITaxTransT ITaxTransT = new ITaxTransT()
                                        {
                                            DBTrack = DBTrack,
                                            EduCess = i.EduCess,
                                            FuncStruct = db.FuncStruct.OrderBy(e => e.Id).Where(e => e.Id == test.FuncStruct_id).FirstOrDefault(),
                                            GeoStruct = db.GeoStruct.OrderBy(e => e.Id).Where(e => e.Id == test.GeoStruct_id).FirstOrDefault(),
                                            Mode = i.Mode,
                                            Surcharge = i.Surcharge,
                                            TaxOnIncome = i.TaxOnIncome,
                                            TaxPaid = i.TaxPaid,
                                            PayMonth = CurMonth,
                                            PayStruct = db.PayStruct.OrderBy(e => e.Id).Where(e => e.Id == test.PayStruct_id).FirstOrDefault(),

                                        };
                                        db.ITaxTransT.Add(ITaxTransT);
                                        db.SaveChanges();

                                        List<ITaxTransT> OFAT = new List<ITaxTransT>();
                                        OFAT.Add(db.ITaxTransT.Find(ITaxTransT.Id));

                                        var aa = db.EmployeePayroll.Where(e => e.Id == i.EmployeePayroll.Id).Include(e => e.ITaxTransT).FirstOrDefault();
                                        OFAT.AddRange(aa.ITaxTransT);
                                        aa.ITaxTransT = OFAT;

                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                  
                                        ts.Complete();
                                }

                            }

                        }
                        //    Msg.Add(" Data already carry forwarded for month =" + CurMonth);
                        return Json(new Utility.JsonReturnClass { success = true, responseText = " Data carry forwarded for month :" + CurMonth + "." }, JsonRequestBehavior.AllowGet);
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
        #endregion
            }
        
    }
}