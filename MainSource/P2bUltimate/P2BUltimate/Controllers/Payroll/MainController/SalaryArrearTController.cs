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
using Payroll;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class SalaryArrearTController : Controller
    {
        private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/SalaryArrearT/Index.cshtml");
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string TotalDays { get; set; }
            public string ArrearType { get; set; }

        }

        public static List<string> StringIdsToListIds(String form)
        {

            List<string> ids = form.Split(',').Select(e => e).ToList();
            return (ids);

        }
        public ActionResult Get_Employelist(SalaryArrearT S, string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                ////if (deserialize.Filter != "" && deserialize.Filter != null)
                ////{
                ////    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}
                ////else
                ////{
                ////    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}
                DateTime FromDate = new DateTime();
                DateTime ToDate = new DateTime();
                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    monthyr = deserialize.Filter;
                    var one_id = StringIdsToListIds(monthyr);
                    var ttt = Convert.ToDateTime(one_id[0]);
                    var ttt1 = Convert.ToDateTime(one_id[1]);
                    FromDate = one_id.Count > 0 ? ttt : DateTime.Now;
                    ToDate = one_id.Count > 0 ? ttt1 : DateTime.Now;
                }
                else
                {
                    monthyr = DateTime.Now.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Include(e => e.EmployeePayroll.Select(a => a.IncrementServiceBook))
                    .Include(e => e.EmployeePayroll.Select(a => a.PromotionServiceBook))
                    .SingleOrDefault();

                //if (deserialize.GeoStruct != null)
                //{
                //    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                //    foreach (var ca in one_id)
                //    {
                //        var id = Convert.ToInt32(ca);
                //        var emp1 = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id && (e.PromotionServiceBook != null && e.PromotionServiceBook.Count != 0 || e.IncrementServiceBook != null && e.IncrementServiceBook.Count != 0)).ToList();
                //        List<Employee> emp = new List<Employee>();

                //        foreach (var item in emp1)
                //        {
                //            var inc = item.IncrementServiceBook.ToList();
                //            var promo = item.PromotionServiceBook.ToList();
                //            if (inc.Any(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate) || promo.Any(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate))
                //            {
                //                emp = emp1.Select(q => q.Employee).ToList();
                //            }

                //        }
                //        //var emp= emp1.Where(q=>q.IncrementServiceBook)
                //        //.Select(e => e.Employee).ToList();
                //        if (emp != null && emp.Count != 0)
                //        {
                //            foreach (var item in emp)
                //            {
                //                data.Add(item);
                //            }
                //        }
                //    }
                //}
                //if (deserialize.PayStruct != null)
                //{
                //    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                //    foreach (var ca in one_id)
                //    {
                //        var id = Convert.ToInt32(ca);
                //        var emp1 = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && e.Employee.PayStruct.Id == id && (e.PromotionServiceBook != null || e.IncrementServiceBook != null)).ToList();
                //        List<Employee> emp = new List<Employee>();
                //        foreach (var item in emp1)
                //        {
                //            var inc = item.IncrementServiceBook.ToList();
                //            var promo = item.PromotionServiceBook.ToList();
                //            if (inc.Any(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate) || promo.Any(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate))
                //            {
                //                emp = emp1.Select(q => q.Employee).ToList();
                //            }
                //        }
                //        if (emp != null && emp.Count != 0)
                //        {
                //            foreach (var item in emp)
                //            {
                //                data.Add(item);
                //            }
                //        }
                //    }
                //}
                //if (deserialize.FunStruct != null)
                //{
                //    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                //    foreach (var ca in one_id)
                //    {
                //        var id = Convert.ToInt32(ca);
                //        var emp1 = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && e.Employee.FuncStruct.Id == id && (e.PromotionServiceBook != null || e.IncrementServiceBook != null)).ToList();
                //        List<Employee> emp = new List<Employee>();
                //        foreach (var item in emp1)
                //        {
                //            var inc = item.IncrementServiceBook.ToList();
                //            var promo = item.PromotionServiceBook.ToList();
                //            if (inc.Any(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate) || promo.Any(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate))
                //            {
                //                emp = emp1.Select(q => q.Employee).ToList();
                //            }
                //        }
                //        if (emp != null && emp.Count != 0)
                //        {
                //            foreach (var item in emp)
                //            {
                //                data.Add(item);
                //            }
                //        }
                //    }
                //}

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                //if (data != null && data.Count != 0)
                //{
                //    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode))
                //    {
                //        if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= ToDate)
                //        {
                //            returndata.Add(new Utility.returndataclass
                //            {
                //                code = item.Id.ToString(),
                //                value = item.FullDetails,
                //            });
                //        }
                //    }
                //    var returnjson = new
                //    {
                //        data = returndata,
                //        tablename = "Employee-Table"
                //    };
                //    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                //}
                //else
                if (empdata.EmployeePayroll.Select(e => e.Employee).ToList().Count > 0)
                {
                    var emp1 = empdata.EmployeePayroll.Where(e => (e.PromotionServiceBook != null && e.PromotionServiceBook.Count != 0) || (e.IncrementServiceBook != null && e.IncrementServiceBook.Count != 0)).ToList();
                    List<Employee> emp = new List<Employee>();
                    if (emp1 != null && emp1.Count() != 0)
                    {
                        foreach (var item in emp1)
                        {
                            var inc = item.IncrementServiceBook.Where(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate).ToList();
                            var promo = item.PromotionServiceBook.Where(qa => qa.ReleaseDate >= FromDate && qa.ReleaseDate <= ToDate).ToList();
                            if ((inc != null && inc.Count != 0) || (promo != null && promo.Count != 0))
                            {
                                emp.Add(item.Employee);
                            }
                        }
                    }
                    foreach (var item in emp)
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult LoadEmpByDefault(string data, string data2)
        {

            if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
            {
                int CompId = int.Parse(Session["CompId"].ToString());
                var query = db.CompanyPayroll.Where(e => e.Company.Id == CompId).Include(e => e.EmployeePayroll)
                    .Include(a => a.EmployeePayroll.Select(e => e.Employee))
                    .Include(a => a.EmployeePayroll.Select(e => e.Employee.ServiceBookDates)).SingleOrDefault();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                foreach (var ca in query.EmployeePayroll.ToList())
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = ca.Employee.Id.ToString(),
                        value = ca.Employee.FullDetails,
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
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
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

                var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT)
                    .Include(x => x.SalaryArrearT.Select(e => e.ArrearType)).ToList();

                foreach (var z in BindEmpList)
                {
                    if (z.SalaryArrearT != null && z.SalaryArrearT.Count > 0)
                    {

                        foreach (var Sal in z.SalaryArrearT)
                        {
                            if (Sal.PayMonth == PayMonth)
                            {
                                view = new P2BGridData()
                                {
                                    Id = Sal.Id,
                                    Code = z.Employee.EmpCode,
                                    Name = z.Employee.EmpName.FullNameFML,
                                    PayMonth = Sal.PayMonth,
                                    FromDate = Sal.FromDate.Value.ToString("dd/MM/yyyy"),
                                    ToDate = Sal.ToDate.Value.ToString("dd/MM/yyyy"),
                                    TotalDays = Sal.TotalDays.ToString(),
                                    ArrearType = Sal.ArrearType.Id == 0 ? "" : db.LookupValue.Where(e => e.Id == Sal.ArrearType.Id).Select(x => x.LookupVal).FirstOrDefault()
                                };
                                model.Add(view);
                            }
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

                        jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
                              || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                              || (e.PayMonth != null ? e.PayMonth.ToString().ToUpper().Contains(gp.searchString.ToUpper()) : false)
                              || (e.FromDate != null ? e.FromDate.ToString().Contains(gp.searchString) : false)
                              || (e.ToDate != null ? e.ToDate.ToString().Contains(gp.searchString) : false)
                              || (e.TotalDays != null ? e.TotalDays.ToString().Contains(gp.searchString) : false)
                              || (e.ArrearType != null ? e.ArrearType.ToString().ToUpper().Contains(gp.searchString.ToUpper()) : false)
                              || (e.Id.ToString().Contains(gp.searchString))
                              ).Select(a => new Object[] { a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "", a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.Id }).ToList();


                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "", a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                         gp.sidx == "Name" ? c.Name.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                         gp.sidx == "ToDate" ? c.ToDate.ToString() :
                                         gp.sidx == "TotalDays" ? c.TotalDays.ToString() :
                                         gp.sidx == "ArrearType" ? c.ArrearType.ToString() :
                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);           //{ a.Id, a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "" }).ToList();
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "", a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "", a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "", a.ArrearType != null ? Convert.ToString(a.ArrearType) : "", a.Id }).ToList();
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


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(SalaryArrearT S, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string arrears_drop = form["arrears_drop"] == "0" ? "" : form["arrears_drop"];
                string IsPaySlip = form["IsPaySlip"] == "0" ? "" : form["IsPaySlip"];
                string IsRecovery = form["IsRecovery"] == "0" ? "" : form["IsRecovery"];
                string IsAuto = form["IsAuto"] == "0" ? "" : form["IsAuto"];
                S.IsPaySlip = Convert.ToBoolean(IsPaySlip);
                S.IsRecovery = Convert.ToBoolean(IsRecovery);
                S.IsAuto = Convert.ToBoolean(IsAuto);
                List<int> ids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = one_ids(Emp);
                }
                else
                {
                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                LookupValue lk = null;
                if (arrears_drop != null && arrears_drop != "")
                {
                    lk = db.LookupValue.Find(int.Parse(arrears_drop));
                    S.ArrearType = lk;
                }

                DateTime mAfromc = Convert.ToDateTime(S.FromDate.ToString());
                DateTime mAtoc = Convert.ToDateTime(S.ToDate.ToString());
                if (mAtoc < mAfromc)
                {
                    Msg.Add(" To date Should not be Less Than From Date  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                if (ModelState.IsValid)
                {
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            Employee OEmployee = null;
                            EmployeePayroll OEmployeePayroll = null;
                            CompanyPayroll OCompanyPayroll = null;

                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                       .Where(r => r.Id == i).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                            OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();
                            var Lwpt = db.LookupValue.Where(e => e.Id == lk.Id).SingleOrDefault();
                            SalaryArrearT OSalaryArrearT = new SalaryArrearT();
                            List<SalaryArrearT> OFAT = new List<SalaryArrearT>();
                            //OSalaryArrearT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                            //OSalaryArrearT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                            //OSalaryArrearT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                            DateTime mAfrom = Convert.ToDateTime(S.FromDate.ToString());
                            DateTime mAto = Convert.ToDateTime(S.ToDate.ToString());
                            int monthsApart = 12 * (S.ToDate.Value.Year - S.FromDate.Value.Year) + S.ToDate.Value.Month - S.FromDate.Value.Month;
                            for (int iq = 0; iq <= monthsApart; iq++)
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 30, 0)))
                                {
                                    DateTime DArrfrom = DateTime.Now;
                                    DateTime DArrTo = DateTime.Now;
                                    if (iq == 0)
                                    {
                                        DArrfrom = mAfrom;
                                    }
                                    else
                                    {
                                        var DArrfrom1 = mAfrom.AddMonths(iq);
                                        var Fd = new DateTime(DArrfrom1.Year, DArrfrom1.Month, 1);
                                        DArrfrom = Fd;
                                    }

                                    if (Lwpt.LookupVal.ToUpper() != "LWP" && Lwpt.LookupVal.ToUpper() != "INCREMENT")
                                    {
                                        //DArrTo = DArrfrom.AddMonths(1);
                                        //DArrTo = DArrTo.AddDays(-1);
                                        if (mAto < new DateTime(DArrfrom.Year, DArrfrom.Month, 1).AddMonths(1).AddDays(-1))
                                        {
                                            DArrTo = mAto;
                                        }
                                        else
                                        {
                                            DArrTo = new DateTime(DArrfrom.Year, DArrfrom.Month, 1).AddMonths(1).AddDays(-1);

                                        }
                                    }
                                    else
                                    {
                                        // DArrTo = mAto;
                                        if (mAto < new DateTime(DArrfrom.Year, DArrfrom.Month, 1).AddMonths(1).AddDays(-1))
                                        {
                                            DArrTo = mAto;
                                        }
                                        else
                                        {
                                            DArrTo = new DateTime(DArrfrom.Year, DArrfrom.Month, 1).AddMonths(1).AddDays(-1);

                                        }

                                    }
                                    if (Lwpt.LookupVal.ToUpper() == "HOLIDAY")
                                    {
                                        //  DArrTo = mAto;
                                        if (mAto < new DateTime(DArrfrom.Year, DArrfrom.Month, 1).AddMonths(1).AddDays(-1))
                                        {
                                            DArrTo = mAto;
                                        }
                                        else
                                        {
                                            DArrTo = new DateTime(DArrfrom.Year, DArrfrom.Month, 1).AddMonths(1).AddDays(-1);

                                        }
                                    }
                                    var totD = (DArrTo - DArrfrom).TotalDays + 1;
                                    Boolean arrdup = false;

                                    for (DateTime mTempDate = DArrfrom; mTempDate <= DArrTo; mTempDate = mTempDate.AddDays(1))
                                    {
                                        var OarrearT = db.EmployeePayroll.Where(t => t.Employee.Id == i).Include(z => z.SalaryArrearT)
                                            .Include(z => z.SalaryArrearT.Select(a => a.ArrearType))
                                            //.Select(e => e.SalaryArrearT.Where(r => r.PayMonth == S.PayMonth && r.ArrearType.LookupVal == S.ArrearType.LookupVal && (r.FromDate <= mTempDate && r.ToDate >= mTempDate)).FirstOrDefault()).SingleOrDefault();
                                         .Select(e => e.SalaryArrearT.Where(r => r.IsRecovery == S.IsRecovery && r.ArrearType.LookupVal == S.ArrearType.LookupVal && (r.FromDate <= mTempDate && r.ToDate >= mTempDate)).FirstOrDefault()).SingleOrDefault();
                                        if (OarrearT != null)
                                        {
                                            Msg.Add("  Arrear date Should not be Duplicate For " + OEmployee.EmpCode);
                                            arrdup = true;
                                            continue;
                                        }

                                    }

                                    if (arrdup == false)
                                    {


                                        OSalaryArrearT = new SalaryArrearT();
                                        {
                                            OSalaryArrearT.ArrearType = S.ArrearType;
                                            OSalaryArrearT.FromDate = DArrfrom;
                                            OSalaryArrearT.TotalDays = totD;
                                            OSalaryArrearT.ToDate = DArrTo;
                                            OSalaryArrearT.IsAuto = S.IsAuto;
                                            OSalaryArrearT.IsPaySlip = S.IsPaySlip;
                                            OSalaryArrearT.IsRecovery = S.IsRecovery;
                                            OSalaryArrearT.PayMonth = S.PayMonth;
                                            OSalaryArrearT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                                            OSalaryArrearT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                                            OSalaryArrearT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                                            OSalaryArrearT.DBTrack = S.DBTrack;
                                        }

                                        // Surendra start
                                        var ggg = DArrfrom.Month + "/" + DArrfrom.Year;
                                        var Attmon = (OSalaryArrearT.FromDate.Value.Month.ToString().Length == 1 ? "0" + OSalaryArrearT.FromDate.Value.Month.ToString() : OSalaryArrearT.FromDate.Value.Month.ToString()) + "/" + OSalaryArrearT.FromDate.Value.Year;
                                        var OSalattendanceTc = db.EmployeePayroll.Where(t => t.Employee.Id == i)
                                          .Select(e => e.SalAttendance.Where(r => r.PayMonth == Attmon).FirstOrDefault()).SingleOrDefault();
                                        if (OSalattendanceTc == null)
                                        {
                                            Msg.Add("Salary not available for " + OEmployee.EmpCode + " and process month is " + Attmon);
                                            continue;

                                        }
                                        string Emppayconceptf30 = db.PayProcessGroup.Include(a => a.PayMonthConcept).AsNoTracking().OrderByDescending(e => e.Id).FirstOrDefault().PayMonthConcept.LookupVal;

                                        double mArrearDaychk = 0;
                                        if (Lwpt.LookupVal == "LWP")
                                        {
                                            if (OSalattendanceTc != null)
                                            {
                                                if (S.IsRecovery == true)
                                                {
                                                    mArrearDaychk = OSalattendanceTc.PaybleDays + OSalattendanceTc.ArrearDays - totD;//S.TotalDays;
                                                }
                                                else
                                                {
                                                    mArrearDaychk = OSalattendanceTc.PaybleDays + OSalattendanceTc.ArrearDays + totD;//S.TotalDays;
                                                }

                                                if (Emppayconceptf30 == "FIXED30DAYS")
                                                {
                                                    if (mArrearDaychk > 30)
                                                    {
                                                        Msg.Add("  Arrear Days Should not greater than paid month days  ");
                                                        continue;
                                                    }
                                                }
                                                else if (Emppayconceptf30 == "30DAYS")
                                                {
                                                    if (mArrearDaychk > 30)
                                                    {
                                                        Msg.Add("  Arrear Days Should not greater than paid month days  ");
                                                        continue;
                                                    }
                                                }
                                                else if (Emppayconceptf30 == "CALENDAR")
                                                {
                                                    if (mArrearDaychk > OSalattendanceTc.MonthDays)
                                                    {
                                                        Msg.Add("  Arrear Days Should not greater than paid month days  ");
                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    if (mArrearDaychk > OSalattendanceTc.MonthDays)
                                                    {
                                                        Msg.Add("  Arrear Days Should not greater than paid month days  ");
                                                        continue;
                                                    }
                                                }


                                            }
                                        }
                                        // Surendra End
                                        db.SalaryArrearT.Add(OSalaryArrearT);
                                        db.SaveChanges();

                                        OFAT.Add(db.SalaryArrearT.Find(OSalaryArrearT.Id));
                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeePayroll OTEP = new EmployeePayroll()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                SalaryArrearT = OFAT,
                                                DBTrack = S.DBTrack
                                            };
                                            db.EmployeePayroll.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                            aa.SalaryArrearT = OFAT;
                                            db.EmployeePayroll.Attach(aa);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                        }

                                        string Month = OSalaryArrearT.FromDate.Value.Month.ToString().Length == 1 ? "0" + OSalaryArrearT.FromDate.Value.Month.ToString() : OSalaryArrearT.FromDate.Value.Month.ToString();
                                        string PayMonth = Month + "/" + OSalaryArrearT.FromDate.Value.Year;

                                        if (OSalaryArrearT.IsAuto == true)
                                        {
                                            SalaryGen.EmployeeArrearProcess(OEmployeePayroll.Id, PayMonth, false, OCompanyPayroll.Id, OSalaryArrearT);
                                        }
                                    }
                                    ts.Complete();
                                }
                            }
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
                }


                Msg.Add("  Data Saved successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
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

        [HttpPost]
        public ActionResult Edit(int data, string param)
        {
            int sts = 3;
            SalaryArrearT Q = db.SalaryArrearT.Include(e => e.ArrearType).Where(e => e.Id == data).SingleOrDefault();
            Employee OEmployee = db.Employee.Include(q => q.GeoStruct.Company).Where(r => r.EmpCode == param).AsNoTracking().AsParallel().SingleOrDefault();
            CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();
            EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.EmpCode == param).AsNoTracking().AsParallel().SingleOrDefault();
            var OEmpSalT = db.EmployeePayroll
                .Include(e => e.Employee)
                .Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.SalaryT).AsNoTracking().AsParallel().SingleOrDefault();

            var EmpSalT = OEmpSalT.SalaryT.Where(e => e.PayMonth == Q.PayMonth).SingleOrDefault();

            if (EmpSalT != null)
            {
                var chkk = OEmpSalT.SalaryT.Any(e => e.PayMonth == Q.PayMonth && e.ReleaseDate != null);
                if (chkk)
                {
                    sts = 0;
                }
                else
                {
                    sts = 1;
                }
            }

            var b = new
            {
                ArrearType_id = Q.ArrearType.Id == null ? 0 : Q.ArrearType.Id,
                ArrearType_details = Q.ArrearType != null ? Q.ArrearType.LookupVal : null,
                FromDate = Q.FromDate.Value.ToShortDateString(),
                ToDate = Q.ToDate.Value.ToShortDateString(),
                TotalDays = Q.TotalDays,
                IsPaySlip = Q.IsPaySlip,
                IsRecovery = Q.IsRecovery,
                PayMonth = Q.PayMonth,
                IsAuto = Q.IsAuto,
                IsStatus = sts
            };
            var SalaryArrearT = db.SalaryArrearT.Find(data);
            TempData["RowVersion"] = SalaryArrearT.RowVersion;
            var Auth = SalaryArrearT.DBTrack.IsModified;
            return Json(new Object[] { b, "", "", Auth, JsonRequestBehavior.AllowGet });
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(SalaryArrearT ESalaryArrearT, int data, FormCollection form, string param) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string ArrearType = form["arrears_drop"] == "0" ? "" : form["arrears_drop"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                //string emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                //List<int> empii = null;
                //empii = one_ids(emp);

                // int eid = 0;
                // string ecode;
                if (ArrearType != null && ArrearType != "")
                {
                    var val = db.LookupValue.Find(int.Parse(ArrearType));
                    ESalaryArrearT.ArrearType = val;
                }

                Employee OEmployee = null;
                //   EmployeePayroll OEmployeePayroll = null;
                //  string EmpCode = null;
                //  SalaryArrearT SalaryArrearT = null;

                //var BindEmpList1 = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT)
                //    .Include(x => x.SalaryArrearT.Select(e => e.ArrearType)).ToList();

                //foreach (var z in BindEmpList1)
                //{
                //    if (z.SalaryArrearT != null && z.SalaryArrearT.Count > 0)
                //    {

                //        foreach (var Sal in z.SalaryArrearT)
                //        {
                //            if (Sal.Id == data)
                //            {
                //                eid = z.Employee.Id;
                //                ecode = z.Employee.EmpCode;
                //                break;
                //            }
                //        }
                //    }

                //}

                //OEmployee = db.Employee.Where(r => r.EmpCode == param).SingleOrDefault();
                //    //.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)

                //OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryArrearT).Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                //var OEmpSalT = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(e => e.SalaryT).SingleOrDefault();
                //var EmpSalT = OEmpSalT.SalaryT != null ? OEmpSalT.SalaryT.Where(e => e.PayMonth == ESalaryArrearT.PayMonth && e.ReleaseDate != null) : null;

                //if (EmpSalT.Count() > 0)
                //{
                //    if (EmpCode == null || EmpCode == "")
                //    {
                //        EmpCode = OEmployee.EmpCode;
                //    }
                //    else
                //    {
                //        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                //    }
                //}

                ////var EmpSalRel = db.EmployeePayroll.Where()


                //if (EmpCode != null)
                //{
                //    Msg.Add(" Salary released for employee " + EmpCode + ". You can't change Arrear days. ");
                //    return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Arrear days" }, JsonRequestBehavior.AllowGet);
                //}


                ////Surendra start check process

                //int eidd = 0;
                //string ecoded;
                //string dpaymonth = null;
                //Employee OEmployeep = null;
                //EmployeePayroll OEmployeePayrollp = null;
                //string EmpCodep = null;
                //SalaryArrearPaymentT SalaryArrearPaymentT = null;



                //var BindEmpList1p = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT)
                //    .Include(x => x.SalaryArrearT.Select(e => e.ArrearType)).ToList();

                //foreach (var z in BindEmpList1p)
                //{
                //    if (z.SalaryArrearT != null && z.SalaryArrearT.Count > 0)
                //    {

                //        foreach (var Sal in z.SalaryArrearT)
                //        {
                //            if (Sal.Id == data)
                //            {
                //                eidd = z.Employee.Id;
                //                ecoded = z.Employee.EmpCode;
                //                dpaymonth = Sal.PayMonth;
                //                break;
                //            }
                //        }
                //    }
                //}




                //OEmployeep = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                //               .Where(r => r.Id == eidd).SingleOrDefault();

                //OEmployeePayrollp = db.EmployeePayroll
                //    .Include(e => e.SalaryArrearT)
                //     .Include(e => e.SalaryArrearT.Select(x => x.SalaryArrearPaymentT))
                //    .Where(e => e.Employee.Id == eidd).SingleOrDefault();


                //var OEmpSalTp = db.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Include(x => x.SalaryArrearT.Select(c => c.SalaryArrearPaymentT)).SingleOrDefault();
                ////var EmpSalT = OEmpSalT.SalaryArrearPaymentT != null ? OEmpSalT.SalaryArrearPaymentT.Where(e => e.PayMonth == dpaymonth && e.ReleaseDate != null) : null;
                //var EmpSalTp = OEmpSalT.SalaryArrearT.Where(e => e.Id == data).Where(e => e.SalaryArrearPaymentT.Count > 0).SingleOrDefault();

                //if (EmpSalTp != null)
                //{
                //    if (EmpCodep == null || EmpCodep == "")
                //    {
                //        EmpCodep = OEmployee.EmpCode;
                //    }
                //    else
                //    {
                //        EmpCodep = EmpCodep + ", " + OEmployeep.EmpCode;
                //    }
                //}


                //if (EmpCodep != null)
                //{
                //    Msg.Add(" Arrear has Processed for employee " + EmpCodep + ". Please Delete and try Again... ");
                //    return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    // return Json(new Object[] { "", "", "Salary released for employee " + EmpCode + ".You can't change Arrear days" }, JsonRequestBehavior.AllowGet);
                //}



                //Surendra end check process
                OEmployee = db.Employee.Include(q => q.GeoStruct.Company).Where(r => r.EmpCode == param).AsNoTracking().AsParallel().SingleOrDefault();
                CompanyPayroll OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();
                EmployeePayroll OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.EmpCode == param).AsNoTracking().AsParallel().SingleOrDefault();
                var OEmpSalT = db.EmployeePayroll
                    .Include(e => e.Employee)
                    .Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.SalaryT).Include(x => x.SalaryArrearT.Select(c => c.SalaryArrearPaymentT)).AsNoTracking().AsParallel().SingleOrDefault();

                var EmpSalT = OEmpSalT.SalaryT.Where(e => e.PayMonth == ESalaryArrearT.PayMonth).SingleOrDefault();

                if (EmpSalT != null)
                {
                    var chkk = OEmpSalT.SalaryT.Any(e => e.PayMonth == ESalaryArrearT.PayMonth && e.ReleaseDate != null);
                    if (chkk)
                    {
                        Msg.Add(" Salary has released for employee " + OEmployee.EmpCode + ". You can't Delete this Record. ");
                        return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Msg.Add(" Salary has processed for employee " + OEmployee.EmpCode + ". First delete salary and try again ");
                        return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }

                var EmpSalTp = OEmpSalT.SalaryArrearT.Where(e => e.Id == data).Where(e => e.SalaryArrearPaymentT.Count > 0).SingleOrDefault();
                if (EmpSalTp != null)
                {

                    SalaryArrearT SAT = db.SalaryArrearT.Include(q => q.SalaryArrearPaymentT).Where(q => q.Id == data).SingleOrDefault();

                    SAT.SalaryArrearPaymentT = null;
                    db.SalaryArrearT.Attach(SAT);
                    db.Entry(SAT).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(SAT).State = System.Data.Entity.EntityState.Detached;
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (ArrearType != null)
                            {
                                if (ArrearType != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(ArrearType));
                                    ESalaryArrearT.ArrearType = val;

                                    var type = db.SalaryArrearT.Include(e => e.ArrearType).Where(e => e.Id == data).SingleOrDefault();
                                    IList<SalaryArrearT> typedetails = null;
                                    if (type.ArrearType != null)
                                    {
                                        typedetails = db.SalaryArrearT.Where(x => x.ArrearType.Id == type.ArrearType.Id && x.Id == data).ToList();
                                    }
                                    else
                                    {
                                        typedetails = db.SalaryArrearT.Where(x => x.Id == data).ToList();
                                    }
                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                    foreach (var s in typedetails)
                                    {
                                        s.ArrearType = ESalaryArrearT.ArrearType;
                                        s.FromDate = ESalaryArrearT.FromDate;
                                        s.ToDate = ESalaryArrearT.ToDate;
                                        s.TotalDays = ESalaryArrearT.TotalDays;
                                        s.IsPaySlip = ESalaryArrearT.IsPaySlip;
                                        s.IsRecovery = ESalaryArrearT.IsRecovery;
                                        s.PayMonth = ESalaryArrearT.PayMonth;
                                        s.IsAuto = ESalaryArrearT.IsAuto;
                                        // s.SalaryArrearPaymentT = null;
                                        db.SalaryArrearT.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                else
                                {
                                    var WFTypeDetails = db.SalaryArrearT.Include(e => e.ArrearType).Where(x => x.Id == data).ToList();
                                    foreach (var s in WFTypeDetails)
                                    {
                                        s.ArrearType = null;
                                        db.SalaryArrearT.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                            }
                            else
                            {
                                var CreditdateypeDetails = db.SalaryArrearT.Include(e => e.ArrearType).Where(x => x.Id == data).ToList();
                                foreach (var s in CreditdateypeDetails)
                                {
                                    s.ArrearType = null;
                                    db.SalaryArrearT.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            if (ESalaryArrearT.IsAuto == true)
                            {
                                var ESalaryArrearTNew = db.SalaryArrearT.Where(x => x.Id == data).SingleOrDefault();

                                string Month = ESalaryArrearTNew.FromDate.Value.Month.ToString().Length == 1 ? "0" + ESalaryArrearTNew.FromDate.Value.Month.ToString() : ESalaryArrearTNew.FromDate.Value.Month.ToString();
                                string PayMonth = Month + "/" + ESalaryArrearTNew.FromDate.Value.Year;

                                SalaryGen.EmployeeArrearProcess(OEmployeePayroll.Id, PayMonth, false, OCompanyPayroll.Id, ESalaryArrearTNew);
                            }
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = ESalaryArrearT.Id, Val = ESalaryArrearT.PayMonth, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { EOff.Id, EOff.AccountNo, "Record Updated", JsonRequestBehavior.AllowGet });

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (SalaryArrearT)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            var databaseValues = (SalaryArrearT)databaseEntry.ToObject();
                            ESalaryArrearT.RowVersion = databaseValues.RowVersion;

                        }
                    }
                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
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
            return View();

        }
        public ActionResult PopulateDropDownList(string data, string data2)
        {
            //var qurey = db.Lookup.Include(e=>e.LookupValues).Where(e=>e.Id==50).Select(e=>e.LookupValues).ToList();
            var qurey = db.Lookup.Where(e => e.Code == "417").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
            var selected = (Object)null;
            if (data2 != "" && data != "0" && data2 != "0")
            {
                selected = Convert.ToInt32(data2);
            }

            SelectList s = new SelectList(qurey, "Id", "LookupVal", selected);
            return Json(s, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data, string param, int types)
        {
            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                List<string> Msg = new List<string>();
                try
                {
                    //Surendra start
                    // int eidd = 0;
                    string ecoded = "";
                    //  string dpaymonth = null;
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    SalaryArrearT SalaryArrearT = null;

                    if (param != "" && param != null)
                    {
                        ecoded = param;
                        //   eidd = Convert.ToInt32(param);
                    }

                    //var BindEmpList1 = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryArrearT)
                    //    .Include(x => x.SalaryArrearT.Select(e => e.ArrearType)).ToList();

                    //foreach (var z in BindEmpList1)
                    //{
                    //    if (z.SalaryArrearT != null && z.SalaryArrearT.Count > 0)
                    //    {

                    //        foreach (var Sal in z.SalaryArrearT)
                    //        {
                    //            if (Sal.Id == data)
                    //            {
                    //                eidd = z.Employee.Id;
                    //                ecoded = z.Employee.EmpCode;
                    //                dpaymonth = Sal.PayMonth;
                    //                break;
                    //            }
                    //        }
                    //    }

                    //}

                    OEmployee = db.Employee.Where(r => r.EmpCode == ecoded).AsNoTracking().SingleOrDefault();

                    //OEmployeePayroll = db.EmployeePayroll
                    //    .Include(e => e.SalaryArrearT)
                    //    .Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                    // && e.ReleaseDate != null
                    //    if (EmpCode == null || EmpCode == "")
                    //    {
                    //        EmpCode = OEmployee.EmpCode;
                    //    }
                    //    else
                    //    {
                    //        EmpCode = EmpCode + ", " + OEmployee.EmpCode;
                    //    }
                    SalaryArrearT SAT = db.SalaryArrearT.Include(e=>e.SalaryArrearPFT).Include(q => q.SalaryArrearPaymentT).Where(q => q.Id == data).SingleOrDefault();
                    var OEmpSalT = db.EmployeePayroll
                        .Include(e => e.Employee)
                        .Where(e => e.Employee.Id == OEmployee.Id).Include(e => e.SalaryT).SingleOrDefault();
                    var EmpSalT = OEmpSalT.SalaryT.Where(e => e.PayMonth == SAT.PayMonth).SingleOrDefault();

                    if (EmpSalT != null)
                    {
                        var chkk = OEmpSalT.SalaryT.Any(e => e.PayMonth == SAT.PayMonth && e.ReleaseDate != null);
                        if (chkk)
                        {
                            Msg.Add(" Salary has released for employee " + EmpCode + ". You can't Delete this Record. ");
                            return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Msg.Add(" Salary has processed for employee " + EmpCode + ". First delete salary and try again ");
                            return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        //Surendra end
                        //var Emp = db.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.SalAttendance).SingleOrDefault();
                        //SalAttendanceT SalAttendanceT = Emp.Where(e => e.LWPDays == null).SingleOrDefault();
                        if (types == 0)
                        {

                            SAT.SalaryArrearPaymentT = null;
                            SAT.SalaryArrearPFT = null;
                            db.SalaryArrearT.Attach(SAT);
                            db.Entry(SAT).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(SAT).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                        }
                        else if (types == 1)
                        {
                            SAT.SalaryArrearPaymentT = null;
                            SAT.SalaryArrearPFT = null;
                            db.Entry(SAT).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();
                            ts.Complete();
                        }
                        //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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