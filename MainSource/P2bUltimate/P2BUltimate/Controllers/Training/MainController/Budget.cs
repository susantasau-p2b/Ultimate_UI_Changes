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
using Training;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class BudgetController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/Budget/Index.cshtml");
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
            public string Category { get; set; }
            public string BudgetAmt { get; set; }

        }

        #region P2BGridDetails
        // [HttpPost]
        public ActionResult LoadCategory(P2BGrid_Parameters gp)
        {

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData1> Category = null;

                List<EditData1> model = new List<EditData1>();
                var view = new EditData1();

                var fall = db.Category.ToList();




                if (gp.IsAutho == true)
                {
                    fall = db.Category
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    fall = db.Category
                        .AsNoTracking().ToList();
                }


                List<int> SkipIds = db.Budget.Where(t => t.Category.Id != null).Select(e => e.Category.Id).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Category.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                foreach (var z in fall)
                {
                    bool EditAppl = true;

                    view = new EditData1()
                    {
                        Id = z.Id,
                        FullDetails = z.FullDetails,
                        BudgetAmt = 0,
                        Editable = EditAppl.ToString().ToLower()
                    };

                    model.Add(view);
                }

                Category = model;

                IEnumerable<EditData1> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Category;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.FullDetails.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.BudgetAmt.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Editable.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                           ).Select(a => new { a.FullDetails, a.BudgetAmt, a.Editable, a.Id }).ToList();


                        //jsonData = IE.Select(a => new { a.Id, a.FullDetails, a.BudgetAmt, a.Editable }).Where((e => (e.Id.ToString() == gp.searchString) || (e.FullDetails.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Category;
                    Func<EditData1, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Category" ? c.FullDetails.ToString() :
                                         gp.sidx == "BudgetAmt" ? c.BudgetAmt.ToString() :
                                          gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = Category.Count();
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

        public ActionResult LoadProgramList(P2BGrid_Parameters gp)
        {

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData1> Category = null;

                List<EditData1> model = new List<EditData1>();
                var view = new EditData1();


                var fall = db.ProgramList.ToList();

                if (gp.IsAutho == true)
                {
                    fall = db.ProgramList
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    fall = db.ProgramList
                        .AsNoTracking().ToList();
                }

                List<int> SkipIds = db.Budget.Where(t => t.ProgramList.Id != null).Select(e => e.ProgramList.Id).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ProgramList.Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }

                foreach (var z in fall)
                {
                    bool EditAppl = true;

                    view = new EditData1()
                    {
                        Id = z.Id,
                        FullDetails = z.FullDetails,
                        BudgetAmt = 0,
                        Editable = EditAppl.ToString().ToLower()
                    };

                    model.Add(view);
                }

                Category = model;

                IEnumerable<EditData1> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Category;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.FullDetails.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.BudgetAmt.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Editable.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                           ).Select(a => new { a.FullDetails, a.BudgetAmt, a.Editable, a.Id }).ToList();

                        //jsonData = IE.Select(a => new { a.Id, a.FullDetails, a.BudgetAmt, a.Editable }).Where((e => (e.Id.ToString() == gp.searchString) || (e.FullDetails.ToLower() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Category;
                    Func<EditData1, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ProgramList" ? c.FullDetails.ToString() :
                                         gp.sidx == "BudgetAmt" ? c.BudgetAmt.ToString() :
                                          gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.FullDetails), a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = Category.Count();
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
                        jsonData = IE.Where(e => (e.Employee.EmpCode != "null" && e.Employee.EmpCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.Employee.EmpName.FullNameFML != "null" && e.Employee.EmpName.FullNameFML.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.PayMonth != "null" && e.PayMonth.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                            || (e.MonthDays.ToString().ToUpper().Contains(gp.searchString.ToString().ToUpper()))
                            || (e.PaybleDays.ToString().ToUpper().Contains(gp.searchString.ToString().ToUpper()))
                            || (e.LWPDays.ToString().ToUpper().Contains(gp.searchString.ToString().ToUpper()))
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
            public Category Category { get; set; }
            public Category SubCategory { get; set; }
            public ProgramList ProgramList { get; set; }
            public GeoStruct Location { get; set; }
            public FuncStruct Job { get; set; }
            public string Editable { get; set; }
            public double BudgetAmt { get; set; }
            //public double WeeklyOff_Cnt { get; set; }
        }

        public class EditData1
        {
            public int Id { get; set; }
            public string Category { get; set; }
            public string FullDetails { get; set; }
            public string ProgramList { get; set; }
            public string Editable { get; set; }
            public double BudgetAmt { get; set; }
            //public double WeeklyOff_Cnt { get; set; }
        }


        public class DeserializeClass
        {
            public string Id { get; set; }
            public string Category { get; set; }
            public string ProgramList { get; set; }
            public string BudgetAmt { get; set; }
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
                IEnumerable<EditData> CategoryData = null;

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
                var OEmployeePayroll = db.Budget.Include(e => e.Category)
                    .ToList();

                int CompTrId = Convert.ToInt32(SessionManager.CompTrainingId);

                //var OCategoryList = db.Budget.Include(e => e.ProgramList)
                //                             .Include(e => e.GeoStruct)
                //                             .Include(e => e.GeoStruct.Location)
                //                             .Include(e => e.GeoStruct.Location.LocationObj)
                //                             .Include(e => e.FuncStruct)
                //                             .Include(e => e.FuncStruct.Job)
                //                             .Include(e => e.FuncStruct.Job.JobPosition)
                //                             .Include(e => e.Category).ToList();

                List<YearlyProgramAssignment> OCategoryList;
                if (gp.filter != null || gp.filter != "")
                {
                    int calendar_Id = Convert.ToInt32(gp.filter);

                    OCategoryList = db.YearlyProgramAssignment
                    .Include(e => e.Budget)
                    .Include(e => e.TrainingYear)
                    .Include(e => e.Budget.Select(t => t.ProgramList))
                    .Include(e => e.Budget.Select(t => t.GeoStruct))
                    .Include(e => e.Budget.Select(t => t.GeoStruct.Location))
                    .Include(e => e.Budget.Select(t => t.GeoStruct.Location.LocationObj))
                    .Include(e => e.Budget.Select(t => t.FuncStruct))
                    .Include(e => e.Budget.Select(t => t.FuncStruct.Job))
                    .Include(e => e.Budget.Select(t => t.FuncStruct.Job.JobPosition))
                    .Include(e => e.Budget.Select(t => t.Category)).Where(e => e.TrainingYear.Id == calendar_Id).ToList();
                }
                else
                {

                    OCategoryList = db.YearlyProgramAssignment
                       .Include(e => e.Budget)
                       .Include(e => e.TrainingYear)
                       .Include(e => e.Budget.Select(t => t.ProgramList))
                       .Include(e => e.Budget.Select(t => t.GeoStruct))
                       .Include(e => e.Budget.Select(t => t.GeoStruct.Location))
                       .Include(e => e.Budget.Select(t => t.GeoStruct.Location.LocationObj))
                       .Include(e => e.Budget.Select(t => t.FuncStruct))
                       .Include(e => e.Budget.Select(t => t.FuncStruct.Job))
                       .Include(e => e.Budget.Select(t => t.FuncStruct.Job.JobPosition))
                       .Include(e => e.Budget.Select(t => t.Category)).ToList();
                }


                foreach (var z1 in OCategoryList)
                {
                    foreach (var z in z1.Budget)
                    {
                        bool EditAppl = true;
                        view = new EditData()
                        {
                            Id = z.Id,
                            Category = z.Category,
                            ProgramList = z.ProgramList,
                            Location = z.GeoStruct,
                            Job = z.FuncStruct,
                            BudgetAmt = z.BudgetCredit,
                            Editable = EditAppl.ToString().ToLower()
                        };

                        model.Add(view);
                    }

                }

                CategoryData = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = CategoryData;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Id.ToString().Contains(gp.searchString))
                           || (e.Category.FullDetails.ToString().ToUpper().Contains(gp.searchString))
                           || (e.ProgramList.FullDetails.ToString().ToUpper().Contains(gp.searchString))
                           || (e.Location.FullDetails.ToString().ToUpper().Contains(gp.searchString))
                           || (e.Job.FullDetails.ToString().ToUpper().Contains(gp.searchString))
                           || (e.BudgetAmt.ToString().Contains(gp.searchString))
                           || (e.Editable.ToUpper().Contains(gp.searchString.ToUpper()))
                       )).Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.FullDetails) : "", a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.Location != null ? Convert.ToString(a.Location.FullDetails) : "", a.Job != null ? Convert.ToString(a.Job.FullDetails) : "", a.BudgetAmt, a.Editable, a.Id }).ToList();
                        //jsonData = IE.Where((e => (e.Contains(gp.searchString)))).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.FullDetails) : "", a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.Location != null ? Convert.ToString(a.Location.FullDetails) : "", a.Job != null ? Convert.ToString(a.Job.FullDetails) : "", a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = CategoryData;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Category" ? c.Category != null ? c.Category.FullDetails.ToString() : "" :
                                         gp.sidx == "ProgramList" ? c.ProgramList != null ? c.ProgramList.FullDetails.ToString() : "" :
                                         gp.sidx == "Location" ? c.Location != null ? c.Location.FullDetails.ToString() : "" :
                                         gp.sidx == "Job" ? c.Job != null ? c.Job.FullDetails.ToString() : "" :
                                         gp.sidx == "BudgetAmt" ? c.BudgetAmt != null ? c.BudgetAmt.ToString() : "" :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.FullDetails) : "", a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.Location != null ? Convert.ToString(a.Location.FullDetails) : "", a.Job != null ? Convert.ToString(a.Job.FullDetails) : "", a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.FullDetails) : "", a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.Location != null ? Convert.ToString(a.Location.FullDetails) : "", a.Job != null ? Convert.ToString(a.Job.FullDetails) : "", a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Category != null ? Convert.ToString(a.Category.FullDetails) : "", a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.Location != null ? Convert.ToString(a.Location.FullDetails) : "", a.Job != null ? Convert.ToString(a.Job.FullDetails) : "", a.BudgetAmt, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = CategoryData.Count();
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


        public ActionResult P2BInlineGrid1(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> CategoryData = null;

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
                var OEmployeePayroll = db.Budget.Include(e => e.Category)
                    .ToList();

                int CompTrId = Convert.ToInt32(SessionManager.CompTrainingId);

                var OCategoryList = db.Budget.Include(e => e.ProgramList).ToList();

                foreach (var z in OCategoryList)
                {
                    bool EditAppl = true;
                    view = new EditData()
                    {
                        Id = z.Id,
                        ProgramList = z.ProgramList,
                        BudgetAmt = z.BudgetCredit,
                        Editable = EditAppl.ToString().ToLower()
                    };

                    model.Add(view);


                }

                CategoryData = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = CategoryData;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Id.ToString().Contains(gp.searchString))
                           || (e.ProgramList.FullDetails.ToString().Contains(gp.searchString))
                           || (e.BudgetAmt.ToString().Contains(gp.searchString))
                           || (e.Editable.ToUpper().Contains(gp.searchString.ToUpper()))
                       )).Select(a => new Object[] { a.Id, a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.BudgetAmt, a.Editable }).ToList();
                        //jsonData = IE.Where((e => (e.Contains(gp.searchString)))).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.BudgetAmt, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = CategoryData;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ProgramList" ? c.ProgramList.FullDetails.ToString() :
                                         gp.sidx == "BudgetAmt" ? c.BudgetAmt.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.BudgetAmt, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.BudgetAmt, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ProgramList != null ? Convert.ToString(a.ProgramList.FullDetails) : "", a.BudgetAmt, a.Editable }).ToList();
                    }
                    totalRecords = CategoryData.Count();
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

        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                int? Id = Convert.ToInt32(EmpCode);
                var query1 = db.Budget.Include(e => e.Category).Where(e => e.Id == Id).SingleOrDefault();

                if (query1 != null)
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




        public ActionResult Create(FormCollection form, String forwarddata, string TrainingYear) //Create submit
        {

            List<string> Msg = new List<string>();
            bool IsAdvFilterAppl = form["IsAdvFilterAppl"] == "true" ? true : false;
            bool ISCatAppl = form["IsCategory"] == "true" ? true : false;
            bool ISProgAppl = form["IsProgram"] == "true" ? true : false;

            string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
            string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
            string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];

            string Budget_Amt = form["BudgetCredit"] == "0" ? "" : form["BudgetCredit"];

            //int geo_id =Convert.ToInt32( form["geo_id"]);
            //int fun_id = Convert.ToInt32( form["func_id"]);
            //int pay_id = Convert.ToInt32( form["pay_id"]);
            //double Budget_Amt = Convert.ToDouble(form["BudgetCredit"]);

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var serialize = new JavaScriptSerializer();

                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);
                    List<int> ids = null;

                    if (obj != null)
                    {
                        if (obj.Count < 0)
                        {
                            return Json(new { sucess = true, responseText = "You have to change amount to update record." }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (IsAdvFilterAppl == false)
                    { ids = obj.Select(e => int.Parse(e.Id)).ToList(); }

                    int TrYear = Convert.ToInt32(TrainingYear);

                    var a = db.YearlyProgramAssignment.Include(e => e.TrainingYear).Include(e=>e.Budget).Where(e => e.TrainingYear.Id == TrYear).FirstOrDefault();
                    List<Budget> BudgetLst = new List<Budget>();

                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            if (IsAdvFilterAppl == true)
                            {
                                Budget bud = null;

                                int Budget_Amt1 = Convert.ToInt32(Budget_Amt);

                                if (geo_id != "" && geo_id != null)
                                {
                                    var geo_id1 = Utility.StringIdsToListIds(form["geo_id"]);
                                    // int geo_id1 = Convert.ToInt32(form["geo_id"]);

                                    foreach (var geoitems in geo_id1)
                                    {
                                        GeoStruct GEOSTRUCT = db.GeoStruct.Find(geoitems);


                                        bud = new Budget()
                                       {

                                           BudgetCredit = Budget_Amt1,
                                           GeoStruct = GEOSTRUCT,
                                           DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                       };
                                    }
                                }
                                if (fun_id != "" && fun_id != null)
                                {
                                    var fun_id1 = Utility.StringIdsToListIds(form["fun_id"]);
                                    //int fun_id1 = Convert.ToInt32(form["fun_id"]);

                                    foreach (var funitems in fun_id1)
                                    {
                                        FuncStruct FUNCSTRUCT = db.FuncStruct.Find(funitems);


                                        bud = new Budget()
                                       {
                                           BudgetCredit = Budget_Amt1,
                                           FuncStruct = FUNCSTRUCT,
                                           DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                       };
                                    }
                                }
                                if (pay_id != "" && pay_id != null)
                                {
                                    var pay_id1 = Utility.StringIdsToListIds(form["pay_id"]);
                                    //int pay_id1 = Convert.ToInt32(form["pay_id"]);

                                    foreach (var payitems in pay_id1)
                                    {
                                        PayStruct PAYSTRUCT = db.PayStruct.Find(payitems);


                                        bud = new Budget()
                                       {
                                           BudgetCredit = Budget_Amt1,
                                           PayStruct = PAYSTRUCT,
                                           DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                       };
                                    }
                                }

                                db.Budget.Add(bud);
                                db.SaveChanges();
                                BudgetLst.Add(bud);
                            }

                            if (ISCatAppl == true)
                            {
                                foreach (int ca in ids)
                                {
                                    int CatId = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Id).Single());
                                    Budget bud = new Budget()
                                    {
                                        BudgetCredit = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.BudgetAmt).Single()),
                                        Category = db.Category.Find(CatId),
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    db.Budget.Add(bud);
                                    db.SaveChanges();
                                    BudgetLst.Add(bud);

                                    //Budget bud = db.Budget.Where(e => e.Category.Id == ca).FirstOrDefault();
                                    //bud.BudgetCredit = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.BudgetAmt).Single());
                                    //BudgetLst.Add(bud);
                                    //db.Budget.Attach(bud);
                                    //db.Entry(bud).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    //db.Entry(bud).State = System.Data.Entity.EntityState.Detached;


                                }
                            }

                            if (ISProgAppl == true)
                            {
                                foreach (int ca in ids)
                                {
                                    int ProgListId = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Id).Single());
                                    Budget bud = new Budget()
                                    {
                                        BudgetCredit = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.BudgetAmt).Single()),
                                        ProgramList = db.ProgramList.Find(ProgListId),
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    db.Budget.Add(bud);
                                    db.SaveChanges();
                                    BudgetLst.Add(bud);

                                    ////Budget bud = db.Budget.Where(e => e.ProgramList.Id == ca).FirstOrDefault();
                                    ////bud.BudgetCredit = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.BudgetAmt).Single());
                                    ////BudgetLst.Add(bud);
                                    ////db.Budget.Attach(bud);
                                    ////db.Entry(bud).State = System.Data.Entity.EntityState.Modified;
                                    ////db.SaveChanges();
                                    ////db.Entry(bud).State = System.Data.Entity.EntityState.Detached;

                                }
                            }

                            a.Budget = BudgetLst;
                            db.YearlyProgramAssignment.Attach(a);
                            db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(a).State = System.Data.Entity.EntityState.Detached;

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
                        //return Json(new Object[] { "", "", "Reason Updated Successfully." }, JsonRequestBehavior.AllowGet);
                        Msg.Add("Data Saved Successfully.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                        return Json(new { sucess = true, responseText = "You have to change amount to update record." }, JsonRequestBehavior.AllowGet);
                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();



                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;




                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            foreach (int ca in ids)
                            {
                                Budget BudgetT = db.Budget.Find(ca);
                                BudgetT.BudgetCredit = Convert.ToDouble(obj.Where(e => e.Id == ca.ToString()).Select(e => e.BudgetAmt).Single());

                                db.Budget.Attach(BudgetT);
                                db.Entry(BudgetT).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(BudgetT).State = System.Data.Entity.EntityState.Detached;
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
                        return Json(new Object[] { "", "", "Record Updated Successfully." }, JsonRequestBehavior.AllowGet);
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

                            var v = db.Budget.Where(a => a.Id == data).ToList();

                            Budget Budget = db.Budget.Find(data);
                            db.Entry(Budget).State = System.Data.Entity.EntityState.Deleted;
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

        public ActionResult GetCalendarDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR").OrderByDescending(e => e.Id).ToList();
                IEnumerable<Calendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult getParameter(string forwardata)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var result = "";
                var qurey = db.BudgetParameters
                    .Select(e => new
                    {
                        Id = e.Id,
                        IsCategory = e.IsCategory.ToString(),
                        IsProgram = e.IsProgram.ToString(),
                        IsGeoStruct = e.IsGeoStruct.ToString(),
                        IsFuncStruct = e.IsFuncStruct.ToString(),
                        IsPayStruct = e.IsPayStruct.ToString(),

                    }).SingleOrDefault();

                if (qurey.IsCategory == "True")
                {
                    result = "IsCategory";
                }
                else if (qurey.IsProgram == "True")
                {
                    result = "IsProgram";
                }
                else
                {
                    result = "IsGeoStruct";
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
