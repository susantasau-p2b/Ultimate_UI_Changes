using P2b.Global;
using P2B.EExMS;
using Payroll;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.EExMS.MainController
{
    public class ExpenseBudgetController : Controller
    {
        //
        // GET: /ExpenseBudget/
        public ActionResult Index()
        {
            return View("~/views/EExMS/MainViews/ExpenseBudget/Index.cshtml");
        }


        [HttpPost]
        public ActionResult Create(ExpenseBudget c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //string geoStruct = form["geo_id"] == "0" ? "" : form["geo_id"];
                    var geoStruct = form["GeostructList"] == "0" ? "" : form["GeostructList"];
                    string expenseCalendar = form["ExpenseCalendarList"] == "0" ? "" : form["ExpenseCalendarList"];
                    string expenseType = form["ExpenseTypelist"] == "0" ? "" : form["ExpenseTypelist"];
                    string sanctionAmt = form["SanctionAmount"] == "0" ? "" : form["SanctionAmount"];
                    string narration = form["Narration"] == "0" ? "" : form["Narration"];

                    int company_Id = 0;
                    company_Id = Convert.ToInt32(Session["CompId"]);
                    var companyExpense = new CompanyExpense();
                    companyExpense = db.CompanyExpense.Where(e => e.Company.Id == company_Id).SingleOrDefault();

                    if (geoStruct == null || geoStruct == "")
                    {
                        Msg.Add("Please Select Geostruct");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    if (expenseCalendar != null && expenseCalendar != "")
                    {
                        int expCalId = Convert.ToInt32(expenseCalendar);
                        ExpenseCalendar val = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Id == expCalId).SingleOrDefault();
                        c.ExpenseCalendar = val;
                    }

                    if (expenseType != null && expenseType != "")
                    {
                        int ExpTypes = Convert.ToInt32(expenseType);
                        LookupValue expType = db.LookupValue.Find(ExpTypes);
                        c.ExpenseType = expType;
                    }

                    if (sanctionAmt != null && sanctionAmt != "")
                    {
                        var sanctionAMT = Convert.ToDouble(sanctionAmt);
                        c.SanctionAmount = sanctionAMT;
                    }

                    if (narration != null && narration != "")
                    {
                        c.Narration = narration;
                    }


                    if (geoStruct != null && geoStruct != "")
                    {
                        var idss = Utility.StringIdsToListIds(geoStruct);
                        foreach (var Geoitem in idss)
                        {
                            int geoId = Convert.ToInt32(Geoitem);
                            GeoStruct geostrucT = db.GeoStruct.Include(e => e.Department).Where(e => e.Department_Id != null && e.Id == geoId).FirstOrDefault();
                            if (geostrucT != null)
                            {
                                Msg.Add("please define all department budget");
                                break;
                            }

                        }
                    }

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            ExpenseBudget expBudget = new ExpenseBudget();

                            if (geoStruct != null && geoStruct != "")
                            {
                                var idss = Utility.StringIdsToListIds(geoStruct);
                                foreach (var Geoitem in idss)
                                {
                                    GeoStruct val = db.GeoStruct.Find(Geoitem);
                                    c.GeoStruct = val;
                                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                    //ExpenseBudget expBudget = new ExpenseBudget()
                                    //{
                                    expBudget.ExpenseCalendar = c.ExpenseCalendar;
                                    expBudget.ExpenseType = c.ExpenseType;
                                    expBudget.GeoStruct = c.GeoStruct;
                                    expBudget.DBTrack = c.DBTrack;
                                    expBudget.SanctionAmount = c.SanctionAmount;
                                    expBudget.Narration = c.Narration;
                                    //};
                                    db.ExpenseBudget.Add(expBudget);
                                    db.SaveChanges();

                                    if (companyExpense != null)
                                    {
                                        List<ExpenseBudget> Expensebudget_list = new List<ExpenseBudget>();
                                        Expensebudget_list.Add(expBudget);
                                        if (companyExpense.ExpenseBudget != null)
                                        {
                                            Expensebudget_list.AddRange(companyExpense.ExpenseBudget);
                                        }
                                        companyExpense.ExpenseBudget = Expensebudget_list;
                                        db.CompanyExpense.Attach(companyExpense);
                                        db.Entry(companyExpense).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(companyExpense).State = System.Data.Entity.EntityState.Detached;

                                    }


                                }


                            }
                            try
                            {

                                ts.Complete();
                                Msg.Add("Data Saved successfully");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelState in ModelState.Values)
                        {
                            foreach (ModelError error in modelState.Errors)
                            {
                                sb.Append(error.ErrorMessage);
                                sb.Append("." + "\n");
                            }
                        }
                        var errorMsg = sb.ToString();
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
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


        public ActionResult GetExpCalendar(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "EXPENSECALENDAR").OrderByDescending(e => e.Id).ToList();
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


        public ActionResult GetExpCalendarDrop(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Flag = true;
                var qurey1 = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Calendar.Default == Flag).ToList();
                var selected1 = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected1 = Convert.ToInt32(data2);
                }

                SelectList s1 = new SelectList(qurey1, "Id", "FullDetails", selected1);

                return Json(s1, JsonRequestBehavior.AllowGet);
            }

        }

        //[HttpPost]
        //public ActionResult LoadExpBudget(P2BGrid_Parameters gp, string ExpCalendar)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
        //            {
        //                int ExpCalId = Convert.ToInt32(ExpCalendar);
        //                if (ExpCalId != null && ExpCalId != 0)
        //                {

        //                    var expbudgetload = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Calendar.Id == ExpCalId).SingleOrDefault();
        //                    if (expbudgetload != null)
        //                    {
        //                        IEnumerable<ExpenseBudget> ExpBudGrid = null;
        //                        ExpBudGrid = db.ExpenseBudget.Include(e => e.ExpenseCalendar).Include(e => e.GeoStruct).Include(e => e.ExpenseType)
        //                             .Where(e => e.ExpenseCalendar.Id == expbudgetload.Id).ToList();

        //                        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //                        int pageSize = gp.rows;
        //                        int totalPages = 0;
        //                        int totalRecords = 0;
        //                        var jsonData = (Object)null;



        //                        IEnumerable<ExpenseBudget> IE;
        //                        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //                        {
        //                            IE = ExpBudGrid;
        //                            if (gp.searchOper.Equals("eq"))
        //                            {
        //                                if (gp.searchField == "Id")
        //                                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                                else if (gp.searchField == "ExpenseType")
        //                                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.ExpenseType.LookupVal.ToString().Contains(gp.searchString)))).ToList();
        //                                else if (gp.searchField == "Narration")
        //                                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.Narration.ToString().Contains(gp.searchString)))).ToList();
        //                                else if (gp.searchField == "SanctionAmount")
        //                                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.SanctionAmount.ToString().Contains(gp.searchString)))).ToList();
        //                                else if (gp.searchField == "GeoStruct")
        //                                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.GeoStruct.Location.LocationObj.LocDesc.ToString().Contains(gp.searchString)))).ToList();


        //                                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //                            }
        //                            if (pageIndex > 1)
        //                            {
        //                                int h = pageIndex * pageSize;
        //                                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //                            }
        //                            totalRecords = IE.Count();
        //                        }
        //                        else
        //                        {
        //                            IE = ExpBudGrid;
        //                            Func<ExpenseBudget, dynamic> orderfuc;
        //                            if (gp.sidx == "Id")
        //                            {
        //                                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //                            }
        //                            else
        //                            {
        //                                orderfuc = (c => gp.sidx == "SanctionAmount" ? c.SanctionAmount.ToString() :
        //                                                 gp.sidx == "ExpenseType" ? c.ExpenseType.LookupVal.ToString() :
        //                                                 gp.sidx == "GeoStruct" ? c.GeoStruct.Location.LocationObj.LocDesc.ToString() :
        //                                                 gp.sidx == "Narration" ? c.Narration.ToString() :
        //                                                   "");
        //                            }
        //                            if (gp.sord == "asc")
        //                            {
        //                                IE = IE.OrderBy(orderfuc);
        //                                jsonData = IE.Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //                            }
        //                            else if (gp.sord == "desc")
        //                            {
        //                                IE = IE.OrderByDescending(orderfuc);
        //                                jsonData = IE.Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //                            }
        //                            if (pageIndex > 1)
        //                            {
        //                                int h = pageIndex * pageSize;
        //                                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //                            }
        //                            totalRecords = ExpBudGrid.Count();
        //                        }
        //                        if (totalRecords > 0)
        //                        {
        //                            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //                        }
        //                        if (gp.page > totalPages)
        //                        {
        //                            gp.page = totalPages;
        //                        }
        //                        var JsonData = new
        //                        {
        //                            page = gp.page,
        //                            rows = jsonData,
        //                            records = totalRecords,
        //                            total = totalPages
        //                        };
        //                        return Json(JsonData, JsonRequestBehavior.AllowGet);

        //                    }
        //                    else
        //                    {
        //                        Msg.Add(" No Record Found for this Expense Period ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }

        //                }

        //            }
        //            return null;

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

        public class returndatagridDataclass //Parentgrid
        {
            public string Id { get; set; }
            public string Geostruct { get; set; }
            public string Calender { get; set; }
        }

        public ActionResult ExpenseBudget_Grid(ParamModel param, string y, string ExpCalendar)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<ExpenseBudget> all = new List<ExpenseBudget>();

                    int ExpCalId = Convert.ToInt32(ExpCalendar);
                    var ExpenseCalenderT = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Id == ExpCalId).SingleOrDefault();
                    if (ExpenseCalenderT != null)
                    {
                        all = db.ExpenseBudget.Include(e => e.ExpenseCalendar)
                       .Include(e => e.ExpenseCalendar.Calendar)
                       .Include(e => e.GeoStruct)
                       .Include(e => e.GeoStruct.Location)
                       .Include(e => e.GeoStruct.Location.LocationObj)
                       .Include(e => e.GeoStruct.Department)
                       .Include(e => e.GeoStruct.Department.DepartmentObj)
                       .Where(e => e.ExpenseCalendar.Id == ExpenseCalenderT.Id)
                       .ToList();
                    }
                    else {

                        all = db.ExpenseBudget.Include(e => e.ExpenseCalendar)
                      .Include(e => e.ExpenseCalendar.Calendar)
                      .Include(e => e.GeoStruct)
                      .Include(e => e.GeoStruct.Location)
                      .Include(e => e.GeoStruct.Location.LocationObj)
                      .Include(e => e.GeoStruct.Department)
                      .Include(e => e.GeoStruct.Department.DepartmentObj)                     
                      .ToList();
                    }

                    IEnumerable<ExpenseBudget> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.GeoStruct.FullDetailsLD.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  ).ToList();
                    }
                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<ExpenseBudget, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.GeoStruct.FullDetailsLD : "");
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
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridDataclass> result = new List<returndatagridDataclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridDataclass
                            {
                                Id = item.Id.ToString(),
                                Geostruct = item.GeoStruct != null ? item.GeoStruct.FullDetailsLD : "",
                                Calender = item.ExpenseCalendar != null ? item.ExpenseCalendar.FullDetails : ""
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
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.ExpenseType.LookupVal, };
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
        public class returndatagridChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string ExpenseType { get; set; }
            public string SanctionAmount { get; set; }
            public string Narration { get; set; }
        }

        public ActionResult A_ExpenseBudget_Grid(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var ExpenseTBUD = db.ExpenseBudget.Include(e => e.GeoStruct).Where(e => e.Id == data).SingleOrDefault();
                    var db_data = db.ExpenseBudget
                        .Include(e => e.ExpenseType)
                        .Include(e => e.GeoStruct)
                        .Include(e => e.GeoStruct.Location)
                        .Include(e => e.GeoStruct.Location.LocationObj)
                        .Include(e => e.GeoStruct.Department)
                        .Include(e => e.GeoStruct.Department.DepartmentObj).Where(e => e.GeoStruct.Id == ExpenseTBUD.GeoStruct.Id).ToList();

                    if (db_data != null)
                    {
                        List<returndatagridChildDataClass> returndata = new List<returndatagridChildDataClass>();
                        foreach (var item in db_data)
                        {
                            returndata.Add(new returndatagridChildDataClass
                            {
                                Id = item.Id,
                                ExpenseType = item.ExpenseType != null ? item.ExpenseType.LookupVal : "",
                                SanctionAmount = item.SanctionAmount.ToString(),
                                Narration = item.Narration
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
        //        IEnumerable<ExpenseBudget> ExpenseBudget = null;
        //        if (gp.IsAutho == true)
        //        {
        //            ExpenseBudget = db.ExpenseBudget
        //                .Include(e => e.ExpenseCalendar)
        //                .Include(e => e.GeoStruct)
        //                .Include(e => e.GeoStruct.Location)
        //                .Include(e => e.GeoStruct.Location.LocationObj)
        //                .Include(e => e.ExpenseType)
        //                .AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            ExpenseBudget = db.ExpenseBudget
        //                .Include(e => e.ExpenseCalendar)
        //                .Include(e => e.GeoStruct)
        //                .Include(e => e.GeoStruct.Location)
        //                .Include(e => e.GeoStruct.Location.LocationObj)
        //                .Include(e => e.ExpenseType)
        //                .AsNoTracking().ToList();
        //        }

        //        IEnumerable<ExpenseBudget> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = ExpenseBudget;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                if (gp.searchField == "Id")
        //                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "ExpenseType")
        //                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.ExpenseType.LookupVal.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "Narration")
        //                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.Narration.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "SanctionAmount")
        //                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.SanctionAmount.ToString().Contains(gp.searchString)))).ToList();
        //                else if (gp.searchField == "GeoStruct")
        //                    jsonData = IE.Select(a => new { a.Id, a.ExpenseType, a.SanctionAmount, a.Narration, a.GeoStruct }).Where((e => (e.GeoStruct.Location.LocationObj.LocDesc.ToString().Contains(gp.searchString)))).ToList();


        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = ExpenseBudget;
        //            Func<ExpenseBudget, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "SanctionAmount" ? c.SanctionAmount.ToString() :
        //                                 gp.sidx == "ExpenseType" ? c.ExpenseType.LookupVal.ToString() :
        //                                 gp.sidx == "GeoStruct" ? c.GeoStruct.Location.LocationObj.LocDesc.ToString() :
        //                                 gp.sidx == "Narration" ? c.Narration.ToString() :
        //                                   "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.GeoStruct.Location.LocationObj.LocDesc), Convert.ToString(a.ExpenseType.LookupVal), Convert.ToString(a.SanctionAmount), Convert.ToString(a.Narration), a.Id }).ToList();
        //            }
        //            totalRecords = ExpenseBudget.Count();
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

        public class returnEditClass
        {
            public int GeoStructId { get; set; }
            public string GeoStructFullDetails { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returndata = db.ExpenseBudget
                            .Include(e => e.ExpenseType)
                            .Include(e => e.GeoStruct)
                                  .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Narration = e.Narration,
                        //GeoStruct = e.GeoStruct,
                        SanctionAmount = e.SanctionAmount,
                        ExpenseType_Id = e.ExpenseType == null ? "" : e.ExpenseType.Id.ToString(),
                        ExpenseCalendar_Id = e.ExpenseCalendar == null ? "" : e.ExpenseCalendar.Id.ToString(),

                    }).ToList();

                var item = db.ExpenseBudget
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.LocationObj)
                    .Include(e => e.GeoStruct.Department)
                    .Include(e => e.GeoStruct.Department.DepartmentObj)
                    .Where(e => e.Id == data).SingleOrDefault();

                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                if (item.GeoStruct != null)
                {
                    oreturnEditClass.Add(new returnEditClass
                    {
                        GeoStructId = item.Id,
                        GeoStructFullDetails = item.GeoStruct.FullDetailsLD
                    });

                }

                return Json(new Object[] { returndata, oreturnEditClass, "", "", JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> EditSave(ExpenseBudget expbud, int data, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //string geoStruct = form["geo_id"] == "0" ? "" : form["geo_id"];
                        var geoStruct = form["GeostructList"] == "0" ? "" : form["GeostructList"];
                        string expenseCalendar = form["ExpenseCalendarList"] == "0" ? "" : form["ExpenseCalendarList"];
                        string expenseType = form["ExpenseTypelist"] == "0" ? "" : form["ExpenseTypelist"];
                        string sanctionAmt = form["SanctionAmount"] == "0" ? "" : form["SanctionAmount"];
                        string narration = form["Narration"] == "0" ? "" : form["Narration"];

                        if (geoStruct == null || geoStruct == "")
                        {
                            Msg.Add("Please Select Geostruct");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }


                        if (expenseCalendar != null && expenseCalendar != "")
                        {
                            int expCalId = Convert.ToInt32(expenseCalendar);
                            ExpenseCalendar val = db.ExpenseCalendar.Include(e => e.Calendar).Where(e => e.Id == expCalId).SingleOrDefault();
                            expbud.ExpenseCalendar = val;
                        }

                        if (expenseType != null && expenseType != "")
                        {
                            int ExpTypes = Convert.ToInt32(expenseType);
                            LookupValue expType = db.LookupValue.Find(ExpTypes);
                            expbud.ExpenseType = expType;
                        }

                        if (sanctionAmt != null && sanctionAmt != "")
                        {
                            var sanctionAMT = Convert.ToDouble(sanctionAmt);
                            expbud.SanctionAmount = sanctionAMT;
                        }

                        if (narration != null && narration != "")
                        {
                            expbud.Narration = narration;
                        }


                        if (geoStruct != null && geoStruct != "")
                        {
                            var idss = Utility.StringIdsToListIds(geoStruct);
                            foreach (var Geoitem in idss)
                            {
                                int geoId = Convert.ToInt32(Geoitem);
                                GeoStruct geostrucT = db.GeoStruct.Include(e => e.Department).Where(e => e.Department_Id != null && e.Id == geoId).FirstOrDefault();
                                if (geostrucT != null)
                                {
                                    Msg.Add("please define all department budget");
                                    break;
                                }

                            }
                        }

                        //var Geodata = db.ExpenseBudget.Include(e => e.GeoStruct).Where(e => e.Id == data).Select(g => g.GeoStruct).FirstOrDefault();

                        var db_data = db.ExpenseBudget.Where(e => e.Id == data).SingleOrDefault();
                        TempData["RowVersion"] = db_data.RowVersion;
                        ExpenseBudget ExpenseBudgetdata = db.ExpenseBudget.Find(data);
                        TempData["CurrRowVersion"] = ExpenseBudgetdata.RowVersion;
                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                        {
                            expbud.DBTrack = new DBTrack
                            {
                                CreatedBy = ExpenseBudgetdata.DBTrack.CreatedBy == null ? null : ExpenseBudgetdata.DBTrack.CreatedBy,
                                CreatedOn = ExpenseBudgetdata.DBTrack.CreatedOn == null ? null : ExpenseBudgetdata.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (geoStruct != null && geoStruct != "")
                            {
                                var idss = Utility.StringIdsToListIds(geoStruct);
                                foreach (var Geoitem in idss)
                                {
                                    GeoStruct val = db.GeoStruct.Find(Geoitem);
                                    expbud.GeoStruct = val;

                                    ExpenseBudgetdata.Id = data;
                                    ExpenseBudgetdata.ExpenseType = expbud.ExpenseType;
                                    //ExpenseBudgetdata.GeoStruct = expbud.GeoStruct == null ? Geodata : expbud.GeoStruct;
                                    ExpenseBudgetdata.GeoStruct = expbud.GeoStruct;
                                    ExpenseBudgetdata.ExpenseCalendar = expbud.ExpenseCalendar;
                                    ExpenseBudgetdata.SanctionAmount = expbud.SanctionAmount;
                                    ExpenseBudgetdata.Narration = expbud.Narration;
                                    ExpenseBudgetdata.DBTrack = expbud.DBTrack;

                                    db.Entry(ExpenseBudgetdata).State = System.Data.Entity.EntityState.Modified;
                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                        ts.Complete();
                        Msg.Add(" Record Updated Successfully. ");
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
        public ActionResult GetGeostruct(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.GeoStruct.Include(e => e.Company)
                    .Include(a => a.Location.LocationObj)
                    .Include(a => a.Department.DepartmentObj)
                    .Where(e => e.Company != null && e.Location != null).ToList();
                IEnumerable<GeoStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.GeoStruct.ToList().Where(d => d.Location != null && d.FullDetailsLD.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetailsLD }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetailsLD }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ExpenseBudget corporates = db.ExpenseBudget.Include(e => e.GeoStruct)
                                                             .Include(e => e.ExpenseCalendar)
                                                             .Include(e => e.ExpenseType)
                                                       .Where(e => e.Id == data).SingleOrDefault();


                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        // var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy = SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;

                            await db.SaveChangesAsync();


                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.)
                    //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                    //return RedirectToAction("Delete");
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

    }
}