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
using System.IO;
using Attendance;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class SalAttendanceTController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SalAttendanceT/Index.cshtml");
        }
        #region DDL
        public ActionResult PopulateTransactionDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayProcessGroup.ToList();
                var selected = (Object)null;
                selected = query.Select(e => e.Id).FirstOrDefault();
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


        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }

        #region P2BGridDetails
        // [HttpPost]
        public ActionResult LoadEmp(P2BGrid_Parameters gp, string param)
        {

            try
            {
                //string monthyr = "";
                //if (gp.filter != null)
                //    monthyr = gp.filter;

                string monthyr = param;
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;

                //////if (gp.filter != null)
                //////    PayMonth = gp.filter;
                //////else
                //////{
                //////    if (DateTime.Now.Date.Month < 10)
                //////        Month = "0" + DateTime.Now.Date.Month;
                //////    else
                //////        Month = DateTime.Now.Date.Month.ToString();
                //////    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                //////}

                //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                if (monthyr != null)
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                        .Include(e => e.EmployeePayroll)
                        .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                        .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                        .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                        .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                        .Include(e => e.EmployeePayroll.Select(a => a.SalAttendance)).AsNoTracking().OrderBy(e => e.Id)
                       .SingleOrDefault();

                    var emp = empdata.EmployeePayroll.ToList();

                    foreach (var z in emp)
                    {
                        SalAttendanceT OAtt = z.SalAttendance.Where(e => e.PayMonth == monthyr).SingleOrDefault();
                        if (OAtt == null)
                        {
                            view = new P2BCrGridData()
                            {
                                Id = z.Employee.Id,
                                Code = z.Employee.EmpCode,
                                Name = z.Employee.EmpName.FullNameFML
                            };
                            if (z.Employee.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + z.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                            {
                                model.Add(view);
                            }
                            ////else if (Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                            ////{
                            ////    model.Add(view);
                            ////}
                            //else if (z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != PayMonth)
                            //{
                            //    model.Add(view);
                            //}
                        }

                    }
                }
                EmpList = model;

                IEnumerable<P2BCrGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                            || (e.Name.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.Code, a.Name, a.Id }).ToList();
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
                                .Include(e => e.Employee.EmpOffInfo.PayProcessGroup).AsNoTracking().ToList();

                foreach (var z in OEmployee)
                {
                    var OSalAttendance = db.EmployeePayroll.Where(e => e.Id == z.Id).Select(e => e.SalAttendance).AsNoTracking()
                                        .SingleOrDefault();


                    DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in OSalAttendance)
                    {
                        if (a.PayMonth == PayMonth)
                        {
                            Eff_Date = Convert.ToDateTime(a.PayMonth);
                            var aa = db.SalAttendanceT.Where(e => e.Id == a.Id).AsNoTracking().SingleOrDefault();
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
                        jsonData = IE.Where(e => (e.Employee.EmpCode != "null" && e.Employee.EmpCode.ToString().Contains(gp.searchString))
                            || (e.Employee.EmpName.FullNameFML != "null" && e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                            || (e.PayMonth != "null" && e.PayMonth.ToString().Contains(gp.searchString))
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
                                         gp.sidx == "LWPDays" ? c.LWPDays.ToString() : "");
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
            public Employee Employee { get; set; }
            public string PayMonth { get; set; }
            public double LWPDays { get; set; }
            public string Editable { get; set; }
            public double PaybleDays { get; set; }
            public double WeeklyOff_Cnt { get; set; }
        }
        public class DeserializeClass
        {
            public string Id { get; set; }
            public string PaybleDays { get; set; }
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
                //var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                //    .Include(o => o.SalAttendance).ToList();
                string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existschk = System.IO.Directory.Exists(requiredPathchk);
                string localPathchk;
                if (!existschk)
                {
                    string localPath = new Uri(requiredPathchk).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string pathchk = requiredPathchk + @"\ButtonVisible" + ".ini";
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

                var tt = db.SalAttendanceT.Where(e => e.PayMonth == PayMonth).Select(r => r.EmployeePayroll_Id).ToList();
                var BindEmpList = db.EmployeePayroll.Where(e => tt.Contains(e.Id)).Select(e => new { Employee = e.Employee, SalAttendance = e.SalAttendance.Where(x => x.PayMonth == PayMonth).FirstOrDefault(), EmpName = e.Employee.EmpName }).ToList();


                foreach (var z in BindEmpList)
                {
                    //foreach (var S in z.SalAttendance)
                    //{
                    //    if (S.PayMonth == PayMonth)
                    //    {

                    bool EditAppl = true;
                    view = new EditData()
                    {
                        Id = z.SalAttendance.Id,
                        Employee = z.Employee != null ? z.Employee : null,
                        PaybleDays = z.SalAttendance.PaybleDays,
                        PayMonth = z.SalAttendance.PayMonth != null ? z.SalAttendance.PayMonth : null,
                        Editable = EditAppl.ToString().ToLower()
                    };

                    model.Add(view);
                    //    }
                    //}
                }

                SalAttendanceT = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalAttendanceT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Employee.EmpCode != null ? e.Employee.EmpCode.ToString().Contains(gp.searchString) : false)
                           || (e.Employee.EmpName.FullNameFML != null ? e.Employee.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()) : false)
                           || (e.PayMonth != null ? e.PayMonth.ToString().Contains(gp.searchString) : false)
                           || (e.PaybleDays.ToString().Contains(gp.searchString))
                           || (e.Editable.ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.Id.ToString().Contains(gp.searchString))
                       )).Select(a => new Object[] { a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable, a.Id }).ToList();
                        //jsonData = IE.Where((e => (e.Contains(gp.searchString)))).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable, a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "PaybleDays" ? c.PaybleDays.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable, a.Id }).ToList();
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
        public ActionResult Checkvisiblebtn()
        {
            string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
            bool existschk = System.IO.Directory.Exists(requiredPathchk);
            string localPathchk;
            bool FileName = false;
            if (!existschk)
            {
                localPathchk = new Uri(requiredPathchk).LocalPath;
                System.IO.Directory.CreateDirectory(localPathchk);
            }
            string pathchk = requiredPathchk + @"\ButtonVisible" + ".ini";
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

            using (var streamReader = new StreamReader(localPathchk))
            {
                string line;

                while ((line = streamReader.ReadLine()) != null)
                {

                    return Json(line, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        public ActionResult ChkProcessAction()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = true;
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult ActiononAttendanceLwp(string PayMonth)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    int Processgrpid = 0;
                    DateTime Pswipedate = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1);
                    List<int> ids = null;
                    ids = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(r => r.Id).ToList();
                    List<string> remark = new List<string>();
                    List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).ToList();
                    foreach (var item in RC)
                    {
                        remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                    }
                    foreach (var i in ids)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                        {
                            var employeeLeave = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == i).FirstOrDefault();

                            var BindEmpList = db.EmployeeAttendance.Select(e => new
                            {
                                Id = e.Id,
                                Employee = e.Employee,
                                EmpName = e.Employee.EmpName,
                                EmpOffInfo = e.Employee.EmpOffInfo,
                                ServiceBookDates = e.Employee.ServiceBookDates,
                                PayProcessGroup = e.Employee.EmpOffInfo.PayProcessGroup,
                                ProcessedData = e.ProcessedData.Select(r => new
                                {
                                    PresentStatus = r.PresentStatus,
                                    MusterRemarks = r.MusterRemarks,
                                    SwipeDate = r.SwipeDate,
                                    Id = r.Id,
                                    ManualReason = r.ManualReason
                                }).Where(r => r.SwipeDate >= Pswipedate).ToList()
                            }).Where(e => e.Employee.Id == i).FirstOrDefault();


                            Processgrpid = BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id;

                            var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                           .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                            int startday = query1.StartDate;
                            int endday = query1.EndDate;
                            DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                            DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                            int daysInEndMonth = end.Day;
                            int daysInstartMonth = 1;
                            DateTime FromPeriod;
                            DateTime EndPeriod;
                            DateTime Currentmonthstart;
                            DateTime CurrentmonthEnd;
                            DateTime Prevmonthstart;
                            DateTime PrevmonthEnd;
                            int ProDays = 0;
                            int RetProDays = 0;
                            int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                            Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                            if (endday > daym)
                            {
                                endday = daym;
                            }
                            ProDays = daym - endday;
                            RetProDays = ProDays;
                            if (startday == daysInstartMonth && endday == daysInEndMonth)
                            {
                                FromPeriod = _PayMonth;
                                EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                            }
                            else
                            {
                                DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                                startday = endday + 1;
                                string pmonth = prvmonth.ToString("MM/yyyy");
                                FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                                EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                            }
                            CurrentmonthEnd = EndPeriod;
                            Prevmonthstart = FromPeriod;
                            if (startday == daysInstartMonth && endday == daysInEndMonth)
                            {
                                PrevmonthEnd = FromPeriod.AddDays(ProDays);
                            }
                            else
                            {
                                PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                            }

                            if ((BindEmpList.Employee != null && BindEmpList.Employee.EmpOffInfo != null && BindEmpList.Employee.EmpOffInfo.PayProcessGroup != null
                           && BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                           (BindEmpList.Employee != null && BindEmpList.Employee.ServiceBookDates != null && BindEmpList.Employee.ServiceBookDates.ServiceLastDate != null))
                            {
                                continue;
                            }

                            var _Prv_EmpLvData_exclude = db.EmployeeLeave.AsNoTracking()
                                          .Where(a => a.Id == employeeLeave.Id)
                                          .Include(a => a.LvNewReq)
                                          .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                          .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                           .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
                                          .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                          .SingleOrDefault();

                            var _Prv_EmpLvData_exclude1 = _Prv_EmpLvData_exclude.LvNewReq
                               .Where(a => a.LeaveHead != null
                                    && a.LeaveHead.LvCode == "LWP"
                                    && a.IsCancel == false && a.WFStatus.LookupVal != "2"
                           ).ToList();

                            var LvOrignal_id = _Prv_EmpLvData_exclude.LvNewReq.Where(e => e.LvOrignal != null && e.WFStatus.LookupVal != "2").Select(e => e.LvOrignal.Id).ToList();
                            var listLvs = _Prv_EmpLvData_exclude1.Where(e => !LvOrignal_id.Contains(e.Id) && e.FromDate != null && e.ToDate != null).OrderBy(e => e.Id).ToList();
                            double DebitSum = 0;
                            if (listLvs != null)
                            {
                                for (DateTime _Date = Prevmonthstart; _Date <= CurrentmonthEnd; _Date = _Date.AddDays(1))
                                {
                                    var xyz = listLvs.Where(q => _Date >= q.FromDate && _Date <= q.ToDate).FirstOrDefault();
                                    if (xyz != null)
                                    {
                                        DebitSum = DebitSum + 1;
                                    }
                                }


                            }

                            int EmpPayrollIds = db.EmployeePayroll.Where(e => e.Employee_Id == i).FirstOrDefault().Id;
                            var EmpSalAttendanceT = db.SalAttendanceT.Where(e => e.EmployeePayroll_Id == EmpPayrollIds && e.PayMonth == PayMonth).FirstOrDefault();
                            EmpSalAttendanceT.LWPDays = DebitSum;
                            db.SalAttendanceT.Attach(EmpSalAttendanceT);
                            db.Entry(EmpSalAttendanceT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }


        #endregion

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

        #region Create
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(SalAttendanceT S, FormCollection form, String forwarddata) //Create submit
        {

            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string PayProcessgropp = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                    string PayMonth = form["Create_Paymonth"] == "0" ? null : form["Create_Paymonth"];
                    string PayableDays = form["Create_PayableDays"] == "0" ? "0" : form["Create_PayableDays"];
                    int paydays = Convert.ToInt32(PayableDays);
                    if (PayMonth == "")
                    {
                        Msg.Add(" Kindly Select Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (PayableDays == "")
                    {
                        Msg.Add(" Kindly Select Payable Days");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<int> ids = null;
                    if (Emp != "null")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly Select Employee ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    List<String> paychk = PayMonth.Split('/').Select(e => e).ToList();
                    int monch = Convert.ToInt32(paychk[0]);
                    int yerch = Convert.ToInt32(paychk[1]);
                    int days = DateTime.DaysInMonth(yerch, monch);

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    //   ids = new List<int> { 133 };
                    //////////if (ids != null)
                    //////////{
                    //////////    foreach (var i in ids)
                    //////////    {

                    //////////        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.PayProcessGroup).Include(e => e.EmpOffInfo.PayProcessGroup.PayMonthConcept).Include(e => e.PayStruct)
                    //////////                   .Where(r => r.Id == i).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();

                    //////////        Utility.DumpProcessStatus("" + OEmployee.EmpCode + "", 00);

                    //////////        OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalAttendance).Where(e => e.Employee.Id == i).SingleOrDefault();

                    //////////        var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).SingleOrDefault();

                    //////////        string Emppayconceptf30 = OEmployee.EmpOffInfo.PayProcessGroup.PayMonthConcept.LookupVal.ToString();

                    //////////        if (Emppayconceptf30 == "FIXED30DAYS")
                    //////////        {
                    //////////            if (paydays > 30)
                    //////////            {
                    //////////                Msg.Add(" Pay month concept is FIXED30DAYS for this employee. Max limit is 30 days .  ");
                    //////////                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //////////            }
                    //////////        }

                    //////////        if (Emppayconceptf30 == "CALENDAR")
                    //////////        {
                    //////////            if (paydays > days)
                    //////////            {
                    //////////                Msg.Add(" Pay month concept is CALENDAR for this employee. Max limit is " + days + " days .");
                    //////////                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //////////            }
                    //////////        }
                    //////////        var EmpSalT = OEmpSalT.SalAttendance != null ? OEmpSalT.SalAttendance.Where(e => e.PayMonth == PayMonth) : null;
                    //////////        if (EmpSalT != null && EmpSalT.Count() > 0)
                    //////////        {
                    //////////            if (EmpCode == null || EmpCode == "")
                    //////////            {
                    //////////                EmpCode = OEmployee.EmpCode;
                    //////////            }
                    //////////            else
                    //////////            {
                    //////////                EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                    //////////            }
                    //////////        }

                    //////////        var OEmpSalRelT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalAttendance).SingleOrDefault();
                    //////////        var EmpSalRelT = OEmpSalRelT.SalaryT != null ? OEmpSalRelT.SalaryT.Where(e => e.PayMonth == PayMonth && e.ReleaseDate != null) : null;

                    //////////        if (EmpSalRelT != null && EmpSalRelT.Count() > 0)
                    //////////        {
                    //////////            if (EmpRel == null || EmpRel == "")
                    //////////            {
                    //////////                EmpRel = OEmployee.EmpCode;
                    //////////            }
                    //////////            else
                    //////////            {
                    //////////                EmpRel = EmpRel + ", " + OEmployee.EmpCode;
                    //////////            }
                    //////////        }
                    //////////    }
                    //////////}
                    //////////if (EmpCode != null)
                    //////////    //  Msg.Add("Attendance already exists for employee " + EmpCode + ".");
                    //////////    return Json(new { success = true, responseText = "Attendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);
                    //////////// return Json(new Object[] { "", "", "Attendance already exists for employee " + EmpCode + "." }, JsonRequestBehavior.AllowGet);

                    //////////if (EmpRel != null)
                    //////////    //    Msg.Add("Salary released for employee " + EmpRel + ". You can't change attendance now.");

                    //////////    return Json(new { success = true, responseText = "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                    ////////////return Json(new Object[] { "", "", "Salary released for employee " + EmpRel + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);



                    if (PayMonth != null && PayMonth != "")
                    {
                        S.PayMonth = PayMonth;
                        int mon = int.Parse(PayMonth.Split('/')[0].StartsWith("0") == true ? PayMonth.Split('/')[0].Remove(0, 1) : PayMonth.Split('/')[0]);
                        int DaysInMonth = System.DateTime.DaysInMonth(int.Parse(PayMonth.Split('/')[1]), mon);
                        S.MonthDays = DaysInMonth;
                    }

                    if (PayableDays != null && PayableDays != "")
                    {
                        var Payable_days = int.Parse(PayableDays);
                        S.PaybleDays = Payable_days;
                    }


                    string Emppayconceptf30 = db.PayProcessGroup.Include(a => a.PayMonthConcept).AsNoTracking().OrderByDescending(e => e.Id).FirstOrDefault().PayMonthConcept.LookupVal;

                    if (Emppayconceptf30 == "FIXED30DAYS")
                    {
                        if (paydays > 30)
                        {
                            Msg.Add(" Pay month concept is FIXED30DAYS. Max limit is 30 days .  ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (Emppayconceptf30 == "CALENDAR")
                    {
                        if (paydays > days)
                        {
                            Msg.Add(" Pay month concept is CALENDAR. Max limit is " + days + " days .");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    //S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false ,CreatedOn=DateTime.Now};
                    SalAttendanceT ObjFAT = new SalAttendanceT();
                    {
                        ObjFAT.PayMonth = S.PayMonth;
                        ObjFAT.PaybleDays = S.PaybleDays;
                        ObjFAT.MonthDays = S.MonthDays;

                        //OEmpSalStruct.PayStruct = db.PayStruct.Find( OEmployeePayroll.PayStruct.Id);


                    }
					
					//LWP leave upload given so this Comment (Goa Urban)
                    // if (Emppayconceptf30 == "FIXED30DAYS")
                    // {
                        // ObjFAT.LWPDays = 30 - S.PaybleDays;
                    // }
                    // if (Emppayconceptf30 == "CALENDAR")
                    // {
                        // ObjFAT.LWPDays = S.MonthDays - S.PaybleDays;
                    // }
                    // if (Emppayconceptf30 == "30DAYS")
                    // {
                        // ObjFAT.LWPDays = S.MonthDays - S.PaybleDays;
                    // }


                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            List<string> PayMnth = new List<string>();
                            DateTime Paydate = Convert.ToDateTime("01/" + PayMonth);

                            var EmployeeDetails = db.Employee.Include(e => e.SalaryHoldDetails).Where(e => e.Id == i).AsNoTracking().FirstOrDefault().SalaryHoldDetails.Where(e => (e.FromDate >= Paydate) && (Paydate <= e.ToDate));

                            if (EmployeeDetails.Count() == 0)
                            {
                                ObjFAT.PaybleDays = S.PaybleDays;
                            }
                            else
                            {
                                ObjFAT.PaybleDays = 0;
                            }

                            var test = db.EmployeePayroll
                           .Join(db.Employee, p => p.Employee.Id, pc => pc.Id, (p, pc) => new { p, pc })
                          .Where(p => p.p.Employee.Id == i).AsNoTracking()
                          .Select(m => new
                          {
                              GeoStruct_id = m.pc.GeoStruct.Id,
                              PayStruct_id = m.pc.PayStruct.Id,
                              FuncStruct_id = m.pc.FuncStruct.Id,
                              Id = m.p.Id
                          }).FirstOrDefault();
                            // OEmployee = db.Employee.Where(r => r.Id == i).Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct).FirstOrDefault();

                            //OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).FirstOrDefault();


                            ObjFAT.GeoStruct = db.GeoStruct.OrderBy(e => e.Id).Where(e => e.Id == test.GeoStruct_id).FirstOrDefault();

                            ObjFAT.FuncStruct = db.FuncStruct.OrderBy(e => e.Id).Where(e => e.Id == test.FuncStruct_id).FirstOrDefault();

                            ObjFAT.PayStruct = db.PayStruct.OrderBy(e => e.Id).Where(e => e.Id == test.PayStruct_id).FirstOrDefault();
                            ObjFAT.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now };

                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    db.SalAttendanceT.Add(ObjFAT);
                                    db.SaveChanges();
                                    List<SalAttendanceT> OFAT = new List<SalAttendanceT>();
                                    OFAT.Add(db.SalAttendanceT.Find(ObjFAT.Id));

                                    if (test == null)
                                    {
                                        EmployeePayroll OTEP = new EmployeePayroll()
                                        {
                                            Employee = db.Employee.Find(i),
                                            SalAttendance = OFAT,
                                            DBTrack = S.DBTrack

                                        };


                                        db.EmployeePayroll.Add(OTEP);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var aa = db.EmployeePayroll.Find(test.Id);
                                        //aa.SalAttendance = null;
                                        if (aa.SalAttendance != null)
                                        {
                                            OFAT.AddRange(aa.SalAttendance);
                                        }

                                        aa.SalAttendance = OFAT;
                                        //OEmployeePayroll.DBTrack = dbt;
                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    Msg.Add("Data Saved successfully for" + i);

                                    ts.Complete();

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
                                    // return Json(new { success = false, Msg}, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                        }
                        //return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        //List<string> Msgh = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    List<string> Msgu = new List<string>();
                    Msg.Add("  Unable to create...  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new { success = false, responseText = "Unable to create..!" }, JsonRequestBehavior.AllowGet);
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

        public ActionResult EditSave(String forwarddata, String PayMonth) // Edit submit
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
                        return Json(new { sucess = true, responseText = "You have to change days to update attendance." }, JsonRequestBehavior.AllowGet);
                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();

                    List<string> EmpCodes = obj.Select(e => e.EmpCode).ToList();

                    //string PayMonth = form["PayMonthEdit"] != "" ? form["PayMonthEdit"] : "";

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;

                    List<String> paychk = PayMonth.Split('/').Select(e => e).ToList();
                    int monch = Convert.ToInt32(paychk[0]);
                    int yerch = Convert.ToInt32(paychk[1]);
                    int days = DateTime.DaysInMonth(yerch, monch);

                    foreach (string ca in EmpCodes)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.PayProcessGroup).Include(e => e.EmpOffInfo.PayProcessGroup.PayMonthConcept).Include(e => e.PayStruct)
                                    .Where(r => r.EmpCode == ca).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();
                        var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalAttendance).Include(e => e.SalaryT).SingleOrDefault();

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
                    }

                    if (EmpCode != null)
                        return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);
                    //return Json(new { success = true, responseText = "Salary released for employee " + EmpCode + ". You can't change attendance now." }, JsonRequestBehavior.AllowGet);

                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            foreach (int ca in ids)
                            {
                                SalAttendanceT SalAttT = db.SalAttendanceT.Find(ca);
                                SalAttT.PaybleDays = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.PaybleDays).Single());

                                string Emppayconceptf30 = OEmployee.EmpOffInfo.PayProcessGroup.PayMonthConcept.LookupVal.ToString();
                                // int con1=Convert.ToInt32(Emppayconceptf30);
                                if (Emppayconceptf30 == "FIXED30DAYS")
                                {
                                    if (SalAttT.PaybleDays > 30)
                                    {
                                        //Msg.Add(" Pay month concept is FIXED30DAYS for this employee. Max limit is 30 days .  ");
                                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        return Json(new Object[] { "", "", "Pay month concept is FIXED30DAYS for this employee. Max limit is 30 days . " }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                if (Emppayconceptf30 == "CALENDAR")
                                {
                                    if (SalAttT.PaybleDays > days)
                                    {
                                        //Msg.Add(" Pay month concept is CALENDAR for this employee. Max limit is " + days + " days .");
                                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        return Json(new Object[] { "", "", "Pay month concept is CALENDAR for this employee. Max limit is " + days + " days . " }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                DateTime Paydate = Convert.ToDateTime("01/" + PayMonth);
                                String Empcode = (obj.Where(e => e.Id == ca.ToString()).Select(e => e.EmpCode).Single());
                                var EmployeeDetails = db.Employee.Include(e => e.SalaryHoldDetails).Where(e => e.EmpCode == Empcode).FirstOrDefault().SalaryHoldDetails.Where(e => (e.FromDate >= Paydate) && (Paydate <= e.ToDate));
                                if (EmployeeDetails.Count() > 0)
                                {
                                    return Json(new Object[] { "", "", "Pay days Can not change for this employee.This employee has Stop salry for this month " }, JsonRequestBehavior.AllowGet);
                                }

								//LWP leave upload given so this Comment (Goa Urban)
                                // if (Emppayconceptf30 == "FIXED30DAYS")
                                // {
                                    // SalAttT.LWPDays = 30 - SalAttT.PaybleDays;
                                // }
                                // if (Emppayconceptf30 == "CALENDAR")
                                // {
                                    // SalAttT.LWPDays = days - SalAttT.PaybleDays;
                                // }
                                // if (Emppayconceptf30 == "30DAYS")
                                // {
                                    // SalAttT.LWPDays = days - SalAttT.PaybleDays;
                                // }


                                db.SalAttendanceT.Attach(SalAttT);
                                db.Entry(SalAttT).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(SalAttT).State = System.Data.Entity.EntityState.Detached;
                            }

                            foreach (string ca in EmpCodes)
                            {
                                //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.EmpCode == ca).SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                                var EmpSalDelT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault() : null;

                                if (EmpSalDelT != null)
                                {
                                    SalaryGen.DeleteSalary(EmpSalDelT.Id, PayMonth);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //List<string> Msg = new List<string>();
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
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        ts.Complete();
                        //return this.Json(new { success = true, responseText = "Payable Days Updated Successfully", JsonRequestBehavior.AllowGet });
                        return Json(new Object[] { "", "", "Payable Days Updated Successfully." }, JsonRequestBehavior.AllowGet);
                        //List<string> Msgs = new List<string>();
                        //Msg.Add("Data Saved successfully");
                        //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

            }
        }
        #endregion

        #region Delete

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            //var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.SalAttendance).SingleOrDefault();
                            //SalAttendanceT SalAttendanceT = Emp.Where(e => e.LWPDays == null).SingleOrDefault();

                            var v = db.SalAttendanceT.Where(a => a.Id == data).Select(a => new {
                                oPayMonth = a.PayMonth, EmpPayroll_id = a.EmployeePayroll_Id
                            }).ToList();
                            foreach (var item in v)
                            {
                                if (db.SalaryT.Any(s => s.PayMonth == item.oPayMonth && s.EmployeePayroll_Id == item.EmpPayroll_id))
                                {
                                    //    Msg.Add("Salary is generated  So,Unable to Delete");
                                    Msg.Add("Salary is generated for PayMonth" + item.oPayMonth.ToString() + "So, Unable to Delete");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            SalAttendanceT SalAttendanceT = db.SalAttendanceT.Find(data);
                            db.Entry(SalAttendanceT).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            //  List<string> Msgn = new List<string>();
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var db_data = db.YearlyPaymentT.Where(e => e.Id == data).SingleOrDefault();
                if (db_data != null)
                {
                    db.YearlyPaymentT.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    Msg.Add("  Record Deleted");
                    return Json(new Utility.JsonReturnClass { data = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new { data = "", responseText = "Record Deleted..!" }, JsonRequestBehavior.AllowGet);
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
        public ActionResult GridEditSave(YearlyPaymentT ypay, FormCollection from, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    try
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.YearlyPaymentT.Where(e => e.Id == id).SingleOrDefault();
                        db_data.AmountPaid = ypay.AmountPaid;

                        db.YearlyPaymentT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { data = db_data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    Msg.Add(" Data Is Null ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion

        //public ActionResult getdays(String monthyear)
        //{
        //    DateTime mnthy = DateTime.Parse(monthyear);
        //    var monthdays = db.PayProcessGroup.Where(a => a.PayMonthConcept.LookupVal.ToUpper() == "FIXED30DAYS").ToList();
        //    if (monthdays != null)
        //     {
        //         int data = 30;
        //         return Json(data);



        //      }
        //      else
        //         {
        //             int year = mnthy.Year;
        //             int month = mnthy.Month;

        //             var data = DateTime.DaysInMonth(year, month);

        //             return Json(data);
        //         }

        //         //int days = 0;
        //         //if (monthdays.PayMonthConcept.LookupVal.ToUpper() == "FIXED30DAYS")
        //         //{
        //         //    int data = 30;
        //         //    return Json(data);
        //         //}
        //         //else
        //         //{
        //         //    int year = mnthy.Year;
        //         //    int month = mnthy.Month;

        //         //    var tdays = DateTime.DaysInMonth(year, month);

        //         //    return Json(tdays);
        //         //}
        //     }

        //    // var d=v.Day.ToString();
        //    //if (d=="30")
        //    //{
        //    //    return Json("30");
        //    //}
        //    //else
        //    //{
        //    //    return Json(d);
        //    //}

        public ActionResult getdays(String monthyear)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime mnthy = DateTime.Parse(monthyear);
                var monthdays = db.PayProcessGroup.Include(a => a.PayMonthConcept).OrderByDescending(e => e.Id).FirstOrDefault();
                if (monthdays != null)
                {


                    if (monthdays.PayMonthConcept.LookupVal.ToUpper() == "FIXED30DAYS")
                    {
                        var data = 30;
                        return Json(data);
                    }
                    else
                    {
                        int year = mnthy.Year;
                        int month = mnthy.Month;

                        var data = DateTime.DaysInMonth(year, month);

                        return Json(data);
                    }
                }
                else
                {
                    int year = mnthy.Year;
                    int month = mnthy.Month;

                    var data = DateTime.DaysInMonth(year, month);

                    return Json(data);
                }
                //int days = 0;
                //if (monthdays.PayMonthConcept.LookupVal.ToUpper() == "FIXED30DAYS")
                //{
                //    int data = 30;
                //    return Json(data);
                //}
                //else
                //{
                //    int year = mnthy.Year;
                //    int month = mnthy.Month;

                //    var tdays = DateTime.DaysInMonth(year, month);

                //    return Json(tdays);
                //}
            }
        }
    }
}
