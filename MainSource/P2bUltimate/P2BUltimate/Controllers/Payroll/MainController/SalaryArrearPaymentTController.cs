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
    public class SalaryArrearPaymentTController : Controller
    {
        private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            //   db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Payroll/MainViews/SalaryArrearPaymentT/Index.cshtml");
        }
        public ActionResult FindArrEmpId()
        {
            dynamic EmpArrIds = System.Web.HttpContext.Current.Session["SalaryArrProcessEmpids"];
            return Json(EmpArrIds, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Process(string paymonth, string selectedids) //Create submit
        {
            //string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
            //string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
            //string PayMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
            //bool AutoIncomeTax = false;

            int CompId = 0;
            if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
            {
                CompId = int.Parse(Session["CompId"].ToString());
            }

            List<int> ids = null;
            if (selectedids != null && selectedids != "0" && selectedids != "false")
            {
                ids = Utility.StringIdsToListIds(selectedids);
            }
            else
            {
                return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
            }

            //if (form["lblManualIT"] == "1")
            //    AutoIncomeTax = false;
            //else if (form["lblManualIT"] == "2")
            //    AutoIncomeTax = true;
            //else if (form["lblManualIT"] == "3")
            //    AutoIncomeTax = false;

            Employee OEmployee = null;
            EmployeePayroll OEmployeePayroll = null;
            CompanyPayroll OCompanyPayroll = null;
            SalaryArrearT OSalArrearT = null;
            List<string> Msg = new List<string>();

            foreach (var i in ids)
            {
                OSalArrearT = db.SalaryArrearT.Find(i);
                string Month = OSalArrearT.FromDate.Value.Month.ToString().Length == 1 ? "0" + OSalArrearT.FromDate.Value.Month.ToString() : OSalArrearT.FromDate.Value.Month.ToString();

                string PayMonth = Month + "/" + OSalArrearT.FromDate.Value.Year;

                var OSalattendanceTc = db.EmployeePayroll.Where(t => t.Id == OSalArrearT.EmployeePayroll_Id)
                                         .Select(e => e.SalaryT.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                if (OSalattendanceTc == null)
                {
                    var OSalEmp1 = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == OSalArrearT.EmployeePayroll_Id).FirstOrDefault();
                    Msg.Add("Salary not available for " + OSalEmp1.Employee.EmpCode + " and process month is " + PayMonth);
                    continue;

                }

                //var OSalEmp = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.SalaryArrearT).ToList();


                //foreach (var a in OSalEmp)
                //{
                var b = db.SalaryArrearT.Where(e => e.Id == i).SingleOrDefault();

                if (b != null)
                {
                    var a = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == b.EmployeePayroll_Id).FirstOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == a.Employee.Id).SingleOrDefault();

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                .Where(r => r.Id == a.Employee.Id).SingleOrDefault();



                    OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                    //Surendra start check process

                    int eidd = 0;
                    string ecoded;
                    string dpaymonth = null;
                    string EmpCodep = null;




                    //var BindEmpList1p = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT)
                    //    .Include(x => x.SalaryArrearT.Select(e => e.ArrearType)).Where(e=>e.Id==b.EmployeePayroll_Id).ToList();

                    //foreach (var z in BindEmpList1p)
                    //{
                    //    if (z.SalaryArrearT != null && z.SalaryArrearT.Count > 0)
                    //    {

                    //        foreach (var Sal in z.SalaryArrearT)
                    //        {
                    //            if (Sal.Id == i)
                    //            {
                    //                eidd = z.Employee.Id;
                    //                ecoded = z.Employee.EmpCode;
                    //                dpaymonth = Sal.PayMonth;
                    //                break;
                    //            }
                    //        }
                    //    }

                    //}




                    //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                    //         .Where(r => r.Id == eidd).SingleOrDefault();

                    //OEmployeePayroll = db.EmployeePayroll
                    //    .Include(e => e.SalaryArrearT)
                    //    .Where(e => e.Employee.Id == eidd).SingleOrDefault();



                    //var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                    //var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == OSalArrearT.PayMonth && e.ReleaseDate != null) : null;

                    //if (EmpSalT.Count() > 0)
                    //{
                    //    if (EmpCodep == null || EmpCodep == "")
                    //    {
                    //        EmpCodep = OEmployee.EmpCode;
                    //    }
                    //    else
                    //    {
                    //        EmpCodep = EmpCodep + ", " + OEmployee.EmpCode;
                    //    }
                    //}

                    //var EmpSalRel = db.EmployeePayroll.Where()

                    var salaryprocess = db.SalaryT.Where(e => e.EmployeePayroll_Id == b.EmployeePayroll_Id && e.PayMonth == b.PayMonth).FirstOrDefault();
                    if (salaryprocess != null)
                    {
                        var OSalEmp1 = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Id == b.EmployeePayroll_Id).FirstOrDefault();
                        Msg.Add(" Salary Process for employee " + OSalEmp1.Employee.EmpCode + " of month " + salaryprocess.PayMonth + ". so you can't Process Arrears. ");
                        continue;
                        //return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Arrear days" }, JsonRequestBehavior.AllowGet);
                    }
                    //Surendra end check process



                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                    {
                        try
                        {
                            //using (DataBaseContext db2 = new DataBaseContext())
                            //{
                            SalaryGen.EmployeeArrearProcess(OEmployeePayroll.Id, PayMonth, false, OCompanyPayroll.Id, OSalArrearT);

                            //}
                            ts.Complete();

                        }
                        catch (DataException ex)
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
                            return Json(new Object[] { "", "", ex.InnerException }, JsonRequestBehavior.AllowGet);


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
                //  }
            }
            //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
            //  List<string> Msgs = new List<string>();
            Msg.Add("  Data Saved successfully  ");
            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
        public class returndatagridclass //Parentgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public string ArrearType { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string TotalDays { get; set; }
        }

        public class SalArrPayChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string PayMonth { get; set; }
            public string SalaryHead { get; set; }
            public string Amount { get; set; }

        }

        public ActionResult Emp_Grid(ParamModel param, string y, string PayMonth)
        {
            try
            {
                List<returndatagridclass> model = new List<returndatagridclass>();
                returndatagridclass view = null;

                //var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Include(e => e.Employee.EmpName)
                //    .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj)
                //    .Include(e => e.SalaryArrearT).Include(e => e.SalaryArrearT.Select(r => r.ArrearType))
                //    .Include(e => e.SalaryArrearT.Select(r => r.SalaryArrearPaymentT))
                //    .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();

                var all = db.SalaryArrearT.Select(d => new
                {
                    OId = d.Id,
                    OPayMonth = d.PayMonth,
                    OEmpCode = d.EmployeePayroll.Employee.EmpCode,
                    OEmpName = d.EmployeePayroll.Employee.EmpName.FullNameFML,
                    OServiceLastDate = d.EmployeePayroll.Employee.ServiceBookDates.ServiceLastDate,
                    OSalaryArrearPaymentT = d.SalaryArrearPaymentT.ToList(),
                    OArrearTypeLookupVal = d.ArrearType.LookupVal,
                    OFromDate = d.FromDate,
                    OToDate = d.ToDate,
                    OTotalDays = d.TotalDays

                }).Where(e => e.OSalaryArrearPaymentT.Count() > 0 && e.OPayMonth == PayMonth && e.OServiceLastDate == null).ToList();

                //IEnumerable<EmployeePayroll> fall;
                if (param.sSearch == null)
                {
                    all = all;
                }
                else
                {
                    all = all.Where(e => (e.OId.ToString().Contains(param.sSearch))
                               || (e.OEmpCode.ToString().Contains(param.sSearch))
                               || (e.OEmpName.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                               || (e.OArrearTypeLookupVal.ToString().ToUpper()).Contains(param.sSearch.ToUpper())
                        //||(e.LoanAdvRequest.Select(q=>q.LoanAdvanceHead.Name).ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                               ).ToList();
                }
                //foreach (var z in fall)
                //{
                //foreach (var a in z.SalaryArrearT)
                //{
                //if (a.SalaryArrearPaymentT.Count() > 0)
                //{
                foreach (var a in all)
                {
                    view = new returndatagridclass()
                    {
                        Id = a.OId,
                        Code = a.OEmpCode == null ? "" : a.OEmpCode,
                        Name = a.OEmpName == null ? "" : a.OEmpName,
                        PayMonth = a.OPayMonth == null ? "" : a.OPayMonth,
                        ArrearType = a.OArrearTypeLookupVal == null ? "" : a.OArrearTypeLookupVal,
                        FromDate = a.OFromDate == null ? "" : a.OFromDate.Value.ToString("dd/MM/yyyy"),
                        ToDate = a.OToDate == null ? "" : a.OToDate.Value.ToString("dd/MM/yyyy"),
                        TotalDays = a.OTotalDays == null ? "" : a.OTotalDays.ToString()
                    };

                    model.Add(view);
                }
                //}

                //}

                //}

                //for column sorting
                var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                Func<returndatagridclass, string> orderfunc = (c =>
                                                            Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                            sortindex == 1 ? c.Name : "");
                var sortcolumn = Request["sSortDir_0"];
                //if (sortcolumn == "asc")
                //{
                //    fall = fall.OrderBy(orderfunc);
                //}
                //else
                //{
                //    fall = fall.OrderByDescending(orderfunc);
                //}
                // Paging 
                var dcompanies = model
                        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                if (dcompanies.Count == 0)
                {
                    List<returndatagridclass> result = new List<returndatagridclass>();
                    result = model;
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = model.Count(),
                        iTotalDisplayRecords = all.Count(),
                        data = result
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = from c in dcompanies

                                 select new[] { null, Convert.ToString(c.Id), c.Name };
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = model.Count(),
                        iTotalDisplayRecords = all.Count(),
                        data = result
                    }, JsonRequestBehavior.AllowGet);
                }//for data reterivation
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public string ArrearType { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string TotalDays { get; set; }
        }

        public JsonResult EditGridDetails(int data)
        {
            var db_data = db.SalaryArrearT.Include(e => e.SalaryArrearPaymentT.Select(r => r.SalaryHead))
                .Include(e => e.SalaryArrearPaymentT.Select(r => r.SalaryHead.Type))
                .Include(e => e.SalaryArrearPaymentT).Where(e => e.Id == data).ToList();

            if (db_data.Count > 0)
            {
                List<SalArrPayChildDataClass> returndata = new List<SalArrPayChildDataClass>();

                foreach (var item in db_data.SelectMany(e => e.SalaryArrearPaymentT).Where(e => e.SalHeadAmount != 0).OrderByDescending(e => e.SalaryHead.Type.LookupVal.ToString()).ThenBy(e => e.SalaryHead.SeqNo))
                {

                    returndata.Add(new SalArrPayChildDataClass
                    {
                        Id = item.Id,
                        PayMonth = item.PayMonth,
                        SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name : null,
                        Amount = item.SalHeadAmount.ToString(),

                    });
                }
                foreach (var item in db_data.SelectMany(e => e.SalaryArrearPaymentT).Where(e => e.SalHeadAmount == 0).OrderByDescending(e => e.SalaryHead.Type.LookupVal.ToString()).ThenBy(e => e.SalaryHead.SeqNo))
                {

                    returndata.Add(new SalArrPayChildDataClass
                    {
                        Id = item.Id,
                        PayMonth = item.PayMonth,
                        SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name : null,
                        Amount = item.SalHeadAmount.ToString(),

                    });
                }

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }

        }

        public class EditData
        {
            public int Id { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double Amount { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp ,string param)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BGridData> SalaryArrList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                string PayMonth = "";
                //string Month = "";

                //if (gp.filter != null)
                //    PayMonth = gp.filter;

                if (!string.IsNullOrEmpty(param))
                {
                    PayMonth = param;
                }
                else
                {
                    if (gp.filter != null)
                    {
                        PayMonth = gp.filter;
                    }
                    //if (DateTime.Now.Date.Month < 10)
                    //    Month = "0" + DateTime.Now.Date.Month;
                    //else
                    //    Month = DateTime.Now.Date.Month.ToString();
                    //PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }
                //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT).Include(x => x.SalaryArrearT.Select(m => m.SalaryArrearPaymentT)).Include(e => e.SalaryArrearT.Select(r => r.ArrearType)).ToList();

                var BindEmpList = db.SalaryArrearT.Select(d => new
                {
                    OId = d.Id,
                    OPayMonth = d.PayMonth,
                    OEmpCode = d.EmployeePayroll.Employee.EmpCode,
                    OFullNameFML = d.EmployeePayroll.Employee.EmpName.FullNameFML,
                    OSalaryArrearPaymentT = d.SalaryArrearPaymentT.ToList(),
                    OArrearTypeLookupVal = d.ArrearType.LookupVal,
                    OFromDate = d.FromDate,
                    OToDate = d.ToDate,
                    OTotalDays = d.TotalDays

                }).Where(e => e.OPayMonth == PayMonth && e.OSalaryArrearPaymentT.Count() == 0).ToList();


                //foreach (var z in BindEmpList)
                //{
                //if (z.SalaryArrearT != null)
                //{

                foreach (var Sal in BindEmpList)
                {
                    //if (Sal.OPayMonth == PayMonth && Sal.SalaryArrearPaymentT.Count() == 0)
                    //{
                    view = new P2BGridData()
                    {
                        Id = Sal.OId,
                        Code = Sal.OEmpCode,
                        Name = Sal.OFullNameFML,
                        PayMonth = Sal.OPayMonth,
                        ArrearType = Sal.OArrearTypeLookupVal == null ? "" : Sal.OArrearTypeLookupVal,
                        FromDate = Sal.OFromDate == null ? "" : Sal.OFromDate.Value.ToString("dd/MM/yyyy"),
                        ToDate = Sal.OToDate == null ? "" : Sal.OToDate.Value.ToString("dd/MM/yyyy"),
                        TotalDays = Sal.OTotalDays.ToString()
                    };

                    model.Add(view);

                    //}
                }
                //}

                //}

                SalaryArrList = model;

                System.Web.HttpContext.Current.Session["SalaryArrProcessEmpids"] = SalaryArrList.Select(r => r.Id).ToArray();

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SalaryArrList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                    || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                    || (e.PayMonth.ToString().Contains(gp.searchString))
                    || (e.ArrearType.ToString().Contains(gp.searchString))
                    || (e.FromDate.ToString().Contains(gp.searchString))
                    || (e.ToDate.ToString().Contains(gp.searchString))
                    || (e.TotalDays.ToString().Contains(gp.searchString))
                    || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.ArrearType, a.FromDate, a.ToDate, a.TotalDays, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.FromDate, a.ToDate, a.TotalDays, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SalaryArrList;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Code :
                                         gp.sidx == "EmpName" ? c.Name :
                                         gp.sidx == "PayMonth" ? c.PayMonth :
                                         gp.sidx == "ArrearType" ? c.ArrearType :
                                         gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                         gp.sidx == "ToDate" ? c.ToDate.ToString() :
                                         gp.sidx == "TotalDays" ? c.TotalDays.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.FromDate, a.ToDate, a.TotalDays, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.FromDate, a.ToDate, a.TotalDays, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth, a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.FromDate, a.ToDate, a.TotalDays, a.Id }).ToList();
                    }
                    totalRecords = SalaryArrList.Count();
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
                IEnumerable<EditData> EmpSalStruct = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();

                int EmpId = int.Parse(gp.id);
                bool EditAppl = true;


                var OEmployeeSalStruct = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalHeadFormula)))

                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                        .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                                        .Where(e => e.Employee.Id == EmpId)
                                       .SingleOrDefault();
                var id = Convert.ToInt32(gp.filter);
                var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.Id == id);

                foreach (var x in OEmpSalStruct)
                {

                    var OEmpSalStructDet = x.EmpSalStructDetails;
                    foreach (var SalForAppl in OEmpSalStructDet)
                    {
                        var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();

                        var SalHeadForm = m.SalHeadFormula;// SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);


                        if (SalHeadForm != null)
                        {
                            if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "BASIC")
                                EditAppl = true;
                            else
                                EditAppl = false;
                        }
                        else
                        {
                            if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "EPF" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PT" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ITAX" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "LWF" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "ESIC" || SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "CPF" ||
                                SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() == "PENSION")
                                EditAppl = false;
                            else
                                EditAppl = true;
                        }
                        if (SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() != "LOAN")
                        {
                            view = new EditData()
                            {
                                Id = SalForAppl.Id,
                                SalaryHead = SalForAppl.SalaryHead,
                                Amount = SalForAppl.Amount,
                                Editable = EditAppl
                            };

                            model.Add(view);
                        }
                    }
                }

                EmpSalStruct = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpSalStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.SalaryHead.Name.ToString().Contains(gp.searchString))
                     || (e.Amount.ToString().Contains(gp.searchString))
                     || (e.SalaryHead.Frequency.LookupVal.ToString().Contains(gp.searchString))
                     || (e.SalaryHead.Type.LookupVal.ToString().Contains(gp.searchString))
                     || (e.SalaryHead.SalHeadOperationType.LookupVal.ToString().Contains(gp.searchString))
                     || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                     || (e.Id.ToString().Contains(gp.searchString))
                                 ).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.Id }).ToList();


                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType.LookupVal, a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpSalStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "SalaryHead" ? c.SalaryHead.Name.ToString() :
                                         gp.sidx == "Amount" ? c.Amount.ToString() :
                                         gp.sidx == "Frequency" ? c.SalaryHead.Frequency.LookupVal.ToString() :
                                         gp.sidx == "Type" ? c.SalaryHead.Type.LookupVal.ToString() :
                                         gp.sidx == "SalHeadOperationType" ? c.SalaryHead.SalHeadOperationType.LookupVal.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency != null ? Convert.ToString(a.SalaryHead.Frequency.LookupVal) : "", a.SalaryHead.Type != null ? Convert.ToString(a.SalaryHead.Type.LookupVal) : "", a.SalaryHead.SalHeadOperationType != null ? Convert.ToString(a.SalaryHead.SalHeadOperationType.LookupVal) : "", a.Editable, a.Id }).ToList();
                    }
                    totalRecords = EmpSalStruct.Count();
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

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Payroll/_ArrearPaymentGridPartial.cshtml");
        }

        public JsonResult GridEditData(int data)
        {
            var Q = db.SalaryArrearPaymentT
                 .Include(e => e.SalHeadAmount)

                 .Where(e => e.Id == data).Select
                 (e => new
                 {
                     SalHeadaa = e.SalaryHead.Name,
                     SalHeadAmountaa = e.SalHeadAmount,
                     Action = e.DBTrack.Action
                 }).SingleOrDefault();

            var yearlypymentT = db.SalaryArrearPaymentT.Find(data);
            Session["RowVersion"] = yearlypymentT.RowVersion;
            var Auth = yearlypymentT.DBTrack.IsModified;
            //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
            return Json(new { data = Q }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GridEditSave(SalaryArrearPaymentT ID, FormCollection form, string data)
        {

            if (data != null)
            {
                var ids = Utility.StringIdsToListIds(data);
                // var id = Convert.ToInt32(data);
                int pi = ids[0];
                int di = ids[1];
                SalaryArrearPaymentT db_data = db.SalaryArrearPaymentT.Where(e => e.Id == pi).SingleOrDefault();

                // string SalHeadaa = form["SalHeadaa"] == "0" ? "" : form["SalHeadaa"];

                // var xr = db.SalaryArrearT.Include(s => s.SalaryArrearPaymentT).ToList();
                var xr = db.SalaryArrearT.Include(e => e.SalaryArrearPFT).Where(x => x.Id == di).SingleOrDefault();
                int eid = 0;
                string ecode;
                string pmon;
                // surendra start 27/12/2018
                List<string> Msg = new List<string>();
                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                string EmpCode = null;
                SalaryArrearT SalaryArrearT = null;



                var BindEmpList1 = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT)
                    .Include(x => x.SalaryArrearT.Select(e => e.ArrearType)).ToList();

                foreach (var z in BindEmpList1)
                {
                    if (z.SalaryArrearT != null && z.SalaryArrearT.Count > 0)
                    {

                        foreach (var Sal in z.SalaryArrearT)
                        {
                            if (Sal.Id == di)
                            {
                                eid = z.Employee.Id;
                                ecode = z.Employee.EmpCode;

                                break;
                            }
                        }
                    }

                }

                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                              .Where(r => r.Id == eid).SingleOrDefault();

                OEmployeePayroll = db.EmployeePayroll
                    .Include(e => e.SalaryArrearT)
                    .Where(e => e.Employee.Id == eid).SingleOrDefault();


                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == xr.PayMonth && e.ReleaseDate != null) : null;
                var EmpSalTProcess = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == xr.PayMonth && e.ReleaseDate == null) : null;

                if (EmpSalT.Count() > 0)
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
                if (EmpCode != null)
                {
                    Msg.Add(" Salary released for employee " + EmpCode + ". You can't change Arrear Amount. ");
                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Arrear days" }, JsonRequestBehavior.AllowGet);
                }

                string EmpCodep = null;

                if (EmpSalTProcess.Count() > 0)
                {
                    if (EmpCodep == null || EmpCodep == "")
                    {
                        EmpCodep = OEmployee.EmpCode;
                    }
                    else
                    {
                        EmpCodep = EmpCodep + ", " + OEmployee.EmpCode;
                    }
                }
                if (EmpCodep != null)
                {
                    Msg.Add(" Please delete Salary for employee " + EmpCodep + ". then change Amount. ");
                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Arrear days" }, JsonRequestBehavior.AllowGet);
                }
                string SalHeadAmountaa = form["SalHeadAmountaa"] == "0" ? "0" : form["SalHeadAmountaa"];
                double minamt = Convert.ToDouble(SalHeadAmountaa);
                if (xr.IsRecovery == false) //if recovery true then minus amount permission ADCC requirement sunil and shankar discuss
                {
                    if (minamt < 0)
                    {

                        Msg.Add(" Amount Should not be less than zero");
                        return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                // surendra end 27/12/2018
                //db_data.SalHeadAmount = ID.SalHeadAmount;

                try
                {
                    
                    db_data.SalHeadAmount = Convert.ToDouble(SalHeadAmountaa);

                    db.SalaryArrearPaymentT.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();


                    var salarrt = db.SalaryArrearT.Include(e => e.SalaryArrearPaymentT).Where(x => x.Id == di).SingleOrDefault();
                    var salid = salarrt.SalaryArrearPaymentT.Select(e => e.SalaryHead_Id).ToList();

                    var EmpPayrollempid = db.EmployeePayroll.Where(e => e.Id == salarrt.EmployeePayroll_Id).SingleOrDefault().Employee_Id;
                    var chkPFApplicable = db.Employee.Include(e => e.EmpOffInfo).Where(e => e.Id == EmpPayrollempid).SingleOrDefault();
                    double epfwages = 0.0;
                    double salarywages = 0.0;
                    double salarywagesdedution = 0.0;
                    double salarywagesnet = 0.0;
                    double emppfvalue = 0.0;
                    foreach (var saldata in salid)
                    {
                        int salidc = Convert.ToInt16(saldata);
                        SalaryHead salheadtype = db.SalaryHead.Include(e => e.Type).Include(e => e.SalHeadOperationType).Where(q => q.Id == salidc).SingleOrDefault();
                        var arrpaamt = salarrt.SalaryArrearPaymentT.Where(e => e.SalaryHead_Id == salidc).FirstOrDefault();
                        double paidamt = 0;
                        if (arrpaamt != null)
                        {
                            paidamt = arrpaamt.SalHeadAmount;
                        }
                       
                        

                        if (salheadtype.Type.LookupVal.ToUpper() == "Earning".ToUpper())
                        {
                            salarywages += Convert.ToDouble(paidamt);
                        }
                        if (salheadtype.Type.LookupVal.ToUpper() != "Earning".ToUpper())
                        {
                            salarywagesdedution += Convert.ToDouble(paidamt);
                        }
                        if (chkPFApplicable != null && chkPFApplicable.EmpOffInfo.PFAppl == true)
                        {
                            var epfmaster = db.PFMaster.Include(q => q.EPFWages).Include(q => q.EPFWages.RateMaster)
                                 .Include(q => q.EPFWages.RateMaster.Select(a => a.SalHead))
                                 .Include(q => q.EPFWages.RateMaster.Select(a => a.SalHead.Type)).Where(e => e.EstablishmentID == chkPFApplicable.EmpOffInfo.PFTrust_EstablishmentId).SingleOrDefault();

                            var ratemasterwages = epfmaster.EPFWages.RateMaster.Select(q => q.SalHead.Id).ToList();

                            foreach (var wagesfromepf in ratemasterwages)
                            {
                                if (wagesfromepf == salheadtype.Id)
                                {
                                    epfwages += Convert.ToDouble(paidamt);
                                }
                            }
                        }

                        
                        if (salheadtype.SalHeadOperationType.LookupVal.ToUpper() == "EPF")
                        {
                            emppfvalue = Convert.ToDouble(paidamt);

                        }


                    }
                    if (xr.SalaryArrearPFT != null)
                    {

                        SalaryArrearPFT db_PFT = db.SalaryArrearPFT.Where(e => e.Id == xr.SalaryArrearPFT.Id).SingleOrDefault();
                        db_PFT.EmpPF = Convert.ToDouble(emppfvalue);
                        db_PFT.CompPF = Convert.ToDouble(emppfvalue);
                        db_PFT.EPFWages = Convert.ToDouble(epfwages);
                        db_PFT.SalaryWages = Convert.ToDouble(salarywages);

                        db.SalaryArrearPFT.Attach(db_PFT);
                        db.Entry(db_PFT).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_PFT).State = System.Data.Entity.EntityState.Detached;
                    }
                    SalaryArrearT db_salArr = db.SalaryArrearT.Where(e => e.Id == xr.Id).SingleOrDefault();
                    salarywagesnet = salarywages - salarywagesdedution;
                    db_salArr.ArrTotalEarning = salarywages;
                    db_salArr.ArrTotalDeduction = salarywagesdedution;
                    db_salArr.ArrTotalNet = salarywagesnet;
                    db.SalaryArrearT.Attach(db_salArr);
                    db.Entry(db_salArr).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(db_salArr).State = System.Data.Entity.EntityState.Detached;


                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                    return this.Json(new { status = true, responseText = "Data Updated Successfully.", JsonRequestBehavior.AllowGet });
                }
                catch (Exception e)
                {

                    throw e;
                }
            }
            else
            {
                return this.Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReleaseProcess(string forwardata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<int> ids = null;
                    if (forwardata == "false" || forwardata == null || forwardata == "0")
                    {
                        Msg.Add(" Unable To Forward Data  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //  return this.Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
                    }
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }

                    EmployeePayroll OEmployeePayroll = null;
                    foreach (var i in ids)
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            using (DataBaseContext db2 = new DataBaseContext())
                            {

                                var ITProject = db.SalaryArrearT.Include(e => e.SalaryArrearPaymentT).Where(e => e.Id == i).FirstOrDefault();
                                if (ITProject.SalaryArrearPaymentT.Count() > 0)
                                {
                                    return Json(new { success = false, responseText = "Arrear Allready Process!! You Can not Enter Manual Entry." }, JsonRequestBehavior.AllowGet);

                                }

                                string EmpCode = null;
                                Employee OEmployee = null;

                                OEmployeePayroll = db.EmployeePayroll
                                    .Include(e=>e.Employee)
                  .Include(e => e.SalaryArrearT)
                  .Where(e => e.Employee.Id == ITProject.EmployeePayroll_Id).SingleOrDefault();

                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                              .Where(r => r.Id == OEmployeePayroll.Employee.Id).SingleOrDefault();


                                var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                                var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == ITProject.PayMonth && e.ReleaseDate != null) : null;
                                var EmpSalTProcess = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == ITProject.PayMonth && e.ReleaseDate == null) : null;

                                if (EmpSalT.Count() > 0)
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
                                if (EmpCode != null)
                                {
                                    Msg.Add(" Salary released for employee " + EmpCode + ". You can't change Arrear Amount. ");
                                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                   
                                }

                                string EmpCodep = null;

                                if (EmpSalTProcess.Count() > 0)
                                {
                                    if (EmpCodep == null || EmpCodep == "")
                                    {
                                        EmpCodep = OEmployee.EmpCode;
                                    }
                                    else
                                    {
                                        EmpCodep = EmpCodep + ", " + OEmployee.EmpCode;
                                    }
                                }
                                if (EmpCodep != null)
                                {
                                    Msg.Add(" Please delete Salary for employee " + EmpCodep + ". then change Amount. ");
                                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                   
                                }


                            }
                            ts.Complete();

                        }
                        return Json(new { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
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
                return View();
            }
        }



        #region DELETE
        [HttpPost]
        public async Task<ActionResult> GridDelete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        //var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.SalAttendance).SingleOrDefault();
                        //SalAttendanceT SalAttendanceT = Emp.Where(e => e.LWPDays == null).SingleOrDefault();
                        SalaryArrearPaymentT SalaryArrearPaymentT = db.SalaryArrearPaymentT.Find(data);
                        db.Entry(SalaryArrearPaymentT).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        ts.Complete();
                        return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //  return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
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
                        // List<string> Msg = new List<string>();
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
        #endregion

    }
}