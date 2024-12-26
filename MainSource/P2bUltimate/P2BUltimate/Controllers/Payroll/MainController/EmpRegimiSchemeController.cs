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
    public class EmpRegimiSchemeController : Controller
    {
        private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/EmpRegimiScheme/Index.cshtml");
        }

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Financialyear { get; set; }
            public string SchemeType { get; set; }

        }

        public static List<string> StringIdsToListIds(String form)
        {

            List<string> ids = form.Split(',').Select(e => e).ToList();
            return (ids);

        }
        public JsonResult GetFinancialyear(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                var query = db.Calendar.Where(e => e.Name.LookupVal.ToUpper().ToString() == "FINANCIALYEAR" && e.Default == true).ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
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




                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

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
                int FinYear = 0;
                if (gp.filter != null)
                {
                    var fromdate = gp.filter.Substring(33, 10);
                    var todate = gp.filter.Substring(53, 11);

                    DateTime finfromdate = Convert.ToDateTime(fromdate);
                    DateTime finTodate = Convert.ToDateTime(todate);
                    var Cal = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.FromDate == finfromdate && e.ToDate == finTodate).AsNoTracking().FirstOrDefault();
                    FinYear = Cal.Id;
                    //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                    //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();

                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(x => x.FinancialYear))
                        .Include(e => e.RegimiScheme.Select(x => x.FinancialYear.Name))
                        .Include(e => e.RegimiScheme.Select(x => x.Scheme)).AsNoTracking()
                       .ToList();

                    //var BindEmpList = db.RegimiScheme.Include(e => e.EmployeePayroll) 
                    //    .Where(e => e.FinancialYear_Id == FinYear).FirstOrDefault();

                    foreach (var z in BindEmpList)
                    {
                        //EmployeePayroll OEmp = db.EmployeePayroll.Find(z.Id);
                        //OEmp.Employee = db.Employee.Find(OEmp.Id);
                        //OEmp.Employee.EmpName = db.NameSingle.Find(OEmp.Employee.EmpName_Id);
                        if (z.RegimiScheme != null && z.RegimiScheme.Count > 0)
                        {
                            var RegSch = z.RegimiScheme.Where(e => e.FinancialYear_Id == FinYear).FirstOrDefault();
                            view = new P2BGridData()
                           {
                               Id = z.Employee.Id,
                               Code = z.Employee.EmpCode,
                               Name = z.Employee.EmpName.FullNameFML,
                               Financialyear = Cal.FullDetails,
                               SchemeType = RegSch.Scheme_Id == 0 ? "" : db.LookupValue.Where(e => e.Id == RegSch.Scheme_Id).Select(x => x.LookupVal).FirstOrDefault()
                           };
                            model.Add(view);
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
                                  || (e.Financialyear != null ? e.Financialyear.ToString().Contains(gp.searchString) : false)
                                  || (e.SchemeType != null ? e.SchemeType.ToString().ToUpper().Contains(gp.searchString.ToUpper()) : false)
                                  || (e.Id.ToString().Contains(gp.searchString))
                                  ).Select(a => new Object[] { a.Code, a.Name, a.Financialyear != null ? Convert.ToString(a.Financialyear) : "", a.SchemeType != null ? Convert.ToString(a.SchemeType) : "", a.Id }).ToList();


                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Financialyear != null ? Convert.ToString(a.Financialyear) : "", a.SchemeType != null ? Convert.ToString(a.SchemeType) : "", a.Id }).ToList();
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
                                             gp.sidx == "Financialyear" ? c.Financialyear.ToString() :
                                             gp.sidx == "SchemeType" ? c.SchemeType.ToString() :
                                        "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);           //{ a.Id, a.Code, a.Name, a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.FromDate != null ? Convert.ToString(a.FromDate) : "", a.ToDate != null ? Convert.ToString(a.ToDate) : "", a.TotalDays != null ? Convert.ToString(a.TotalDays) : "" }).ToList();
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Financialyear != null ? Convert.ToString(a.Financialyear) : "", a.SchemeType != null ? Convert.ToString(a.SchemeType) : "", a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.Financialyear != null ? Convert.ToString(a.Financialyear) : "", a.SchemeType != null ? Convert.ToString(a.SchemeType) : "", a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.Financialyear != null ? Convert.ToString(a.Financialyear) : "", a.SchemeType != null ? Convert.ToString(a.SchemeType) : "", a.Id }).ToList();
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(EmployeePayroll S, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string RegimiSchemelist = form["RegimiSchemelist"] == "" ? null : form["RegimiSchemelist"];

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


                if (RegimiSchemelist != null)
                {
                    var ids1 = Utility.StringIdsToListIds(RegimiSchemelist);
                    var RegimiSchemellist = new List<RegimiScheme>();
                    foreach (var item in ids1)
                    {

                        int RegSchid = Convert.ToInt32(item);
                        var val = db.RegimiScheme.Find(RegSchid);
                        if (val != null)
                        {
                            RegimiSchemellist.Add(val);
                        }
                    }
                    S.RegimiScheme = RegimiSchemellist;
                }
                else
                {
                    Msg.Add(" Kindly select RegimeScheme  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                //if (mAtoc < mAfromc)
                //{
                //    Msg.Add(" To date Should not be Less Than From Date  ");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}

                S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                if (ModelState.IsValid)
                {
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            Employee OEmployee = null;
                            EmployeePayroll OEmployeePayroll = null;

                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                       .Where(r => r.Id == i).FirstOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).FirstOrDefault();


                            RegimiScheme ORegimiScheme = new RegimiScheme();
                            List<RegimiScheme> OFAT = new List<RegimiScheme>();

                            var regimecheck = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                            var duplicaterec = regimecheck.RegimiScheme.Where(e => e.FinancialYear_Id == S.RegimiScheme.FirstOrDefault().FinancialYear_Id).FirstOrDefault();
                            if (duplicaterec != null)
                            {
                                Msg.Add("You Have Already define Regime scheme " + OEmployee.EmpCode + " If you want Change Edit This employee and Change Scheme");
                                continue;
                            }
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 60, 0)))
                            {

                                if (OEmployeePayroll == null)
                                {
                                    EmployeePayroll OTEP = new EmployeePayroll()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),
                                        RegimiScheme = S.RegimiScheme,
                                        DBTrack = S.DBTrack
                                    };
                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    aa.RegimiScheme = S.RegimiScheme;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                }


                                ts.Complete();
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
        public ActionResult Regimiupdate() //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                int fyyear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;
                string onetimeupload2024 = "01/04/2024";
                DateTime onetimeupload = Convert.ToDateTime(onetimeupload2024);
                Calendar temp_OFinancialYear = db.Calendar.Where(e => e.Id == fyyear).SingleOrDefault();
                if (temp_OFinancialYear.FromDate.Value.Date != onetimeupload.Date)
                {
                    Msg.Add(" you can process only for 01/04/2024 financial year ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);  
                }
                var ids = db.Employee.Include(e => e.ServiceBookDates).Where(e=>e.ServiceBookDates.ServiceLastDate==null).ToList().Select(r=>r.Id);
                //string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                //string RegimiSchemelist = form["RegimiSchemelist"] == "" ? null : form["RegimiSchemelist"];
                var regimischeme = db.RegimiScheme.Include(e=>e.Scheme).Include(e => e.FinancialYear).ToList();
                if (regimischeme.Count()==0)
                {
                    Msg.Add(" Kindly Define RegimiScheme ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (regimischeme.Count() != 2)
                {
                    Msg.Add(" Kindly Define RegimiScheme OLDTAX and NEWTAX ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in regimischeme)
                {
                    if (item.FinancialYear.FromDate.Value.Date!=onetimeupload.Date)
                    {
                        Msg.Add(" kindly define RegimiScheme for 01/04/2024 financial year ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);  
                    }
                }
             
               
              //  S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                if (ModelState.IsValid)
                {
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            Employee OEmployee = null;
                            EmployeePayroll OEmployeePayroll = null;

                            OEmployee = db.Employee.Include(e=>e.EmpOffInfo).Include(e=>e.EmpOffInfo.NationalityID).Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                       .Where(r => r.Id == i).FirstOrDefault();
                           

                            OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).FirstOrDefault();


                            RegimiScheme ORegimiScheme = new RegimiScheme();
                            List<RegimiScheme> OFAT = new List<RegimiScheme>();

                            var regimecheck = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmployeePayroll.Id).FirstOrDefault();
                            var duplicaterec = regimecheck.RegimiScheme.Where(e => e.FinancialYear_Id == fyyear).FirstOrDefault();
                            if (duplicaterec != null)
                            {
                                Msg.Add("You Have Already define Regime scheme " + OEmployee.EmpCode + " If you want Change Edit This employee and Change Scheme");
                                continue;
                            }
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 60, 0)))
                            {

                                if (OEmployeePayroll == null)
                                {
                                    EmployeePayroll OTEP = new EmployeePayroll()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),

                                        RegimiScheme = OEmployee.EmpOffInfo.NationalityID.No2 == "Yes" ? db.RegimiScheme.Include(e => e.Scheme).Where(e => e.Scheme.LookupVal.ToUpper() == "NEWTAX").ToList() : db.RegimiScheme.Include(e => e.Scheme).Where(e => e.Scheme.LookupVal.ToUpper() == "OLDTAX").ToList(),
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                    };
                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    aa.RegimiScheme = OEmployee.EmpOffInfo.NationalityID.No2 == "Yes" ? db.RegimiScheme.Include(e => e.Scheme).Where(e => e.Scheme.LookupVal.ToUpper() == "NEWTAX").ToList() : db.RegimiScheme.Include(e => e.Scheme).Where(e => e.Scheme.LookupVal.ToUpper() == "OLDTAX").ToList();
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    // db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                }


                                ts.Complete();
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
            int FinYearId = int.Parse(param);

            RegimiScheme RegSch = null;
            EmployeePayroll EmpPayroll = db.EmployeePayroll.Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(t => t.Scheme)).Where(e => e.Employee_Id == data).AsNoTracking().AsParallel().FirstOrDefault();
            if (EmpPayroll != null)
            {
                RegSch = EmpPayroll.RegimiScheme.Where(e => e.FinancialYear_Id == FinYearId).FirstOrDefault();
            }



            if (RegSch != null)
            {
                var b = new
                {
                    Scheme_id = RegSch.Id == null ? 0 : RegSch.Id,
                    FinancialYear_id = db.Calendar.Include(e => e.Name).Where(e => e.Id == RegSch.FinancialYear_Id).FirstOrDefault().FullDetails + " " + RegSch.Scheme.LookupVal.ToUpper(),
                };

                TempData["RowVersion"] = RegSch.RowVersion;

                return Json(new Object[] { b, "", "", "", JsonRequestBehavior.AllowGet });
            }
            return Json(new Object[] { null, "", "", "", JsonRequestBehavior.AllowGet });
        }
        public ActionResult GetLookupDetailsRegimi(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.RegimiScheme
                    .Include(e => e.FinancialYear)
                    .Include(e => e.FinancialYear.Name)
                    .Include(e => e.Scheme).ToList();
                // IEnumerable<WeeklyOffCalendar> all;
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.RegimiScheme.Include(e => e.FinancialYear)
                    .Include(e => e.Scheme)
                    .Where(e => e.Id != a).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();
                    }
                }

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FinancialYear.FullDetails + " " + ca.Scheme.LookupVal.ToUpper() }).Distinct();
                //var result_1 = (from c in fall
                //                select new { c.Id, c.DepartmentCode, c.DepartmentName });
                return Json(r, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EmployeePayroll ERegimiScheme, int data, FormCollection form, string param) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                //    string ArrearType = form["arrears_drop"] == "0" ? "" : form["arrears_drop"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                string FinancialYear_drop = param;

                string RegimiScheme_drop = form["RegimiSchemelist"] == "" ? null : form["RegimiSchemelist"];

                if (RegimiScheme_drop == null || RegimiScheme_drop == "")
                {
                    Msg.Add(" Kindly select RegimeScheme  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                 
                EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Where(r => r.Employee.EmpCode == param).AsNoTracking().AsParallel().FirstOrDefault();


                if (ModelState.IsValid)
                {
                    try
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            var db_data = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmpPayroll.Id).FirstOrDefault();
                            List<RegimiScheme> LvH = new List<RegimiScheme>();


                            if (RegimiScheme_drop != null && RegimiScheme_drop != "")
                            {
                                int RegId = int.Parse(RegimiScheme_drop);
                                List<RegimiScheme> val = db.RegimiScheme.Where(e => e.Id == RegId).ToList();
                                db_data.RegimiScheme = val;
                            }
                            else
                            {
                                db_data.RegimiScheme = null;
                            }


                            db.EmployeePayroll.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                        }
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = 0, Val = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            var qurey = db.Lookup.Where(e => e.Code == "3036").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
            var selected = (Object)null;
            if (data2 != "" && data != "0" && data2 != "0")
            {
                selected = Convert.ToInt32(data2);
            }

            SelectList s = new SelectList(qurey, "Id", "LookupVal", selected);
            return Json(s, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateDropDownListFina(string data, string data2)
        {
            //var qurey = db.Lookup.Include(e=>e.LookupValues).Where(e=>e.Id==50).Select(e=>e.LookupValues).ToList();
            //var qurey = db.Lookup.Where(e => e.Code == "3036").Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault();
            //var selected = (Object)null;
            //if (data2 != "" && data != "0" && data2 != "0")
            //{
            //    selected = Convert.ToInt32(data2);
            //}

            //SelectList s = new SelectList(qurey, "Id", "LookupVal", selected);
            //return Json(s, JsonRequestBehavior.AllowGet);
            var query = db.Calendar.Where(e => e.Name.LookupVal.ToUpper().ToString() == "FINANCIALYEAR" && e.Default == true).ToList();
            var selected = (Object)null;
            if (data2 != "" && data != "0" && data2 != "0")
            {
                selected = Convert.ToInt32(data2);
            }

            SelectList s = new SelectList(query, "Id", "FullDetails", selected);
            return Json(s, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public  ActionResult Delete(int data, string param)
        {

            List<string> Msg = new List<string>();

            int FinYearId = int.Parse(param.Split('-')[1]);
            string Empcode =  param.Split('-')[0];

            EmployeePayroll OEmpPayroll = db.EmployeePayroll.Include(e => e.Employee).Where(r => r.Employee.EmpCode == Empcode).AsNoTracking().AsParallel().FirstOrDefault();


            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var db_data = db.EmployeePayroll.Include(e => e.RegimiScheme).Where(e => e.Id == OEmpPayroll.Id).FirstOrDefault();
                        var ORegSch = db_data.RegimiScheme.Where(e => e.FinancialYear_Id == FinYearId).FirstOrDefault();
                        db_data.RegimiScheme = null;
                        
                        db.EmployeePayroll.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("  Record Updated");
                    return Json(new Utility.JsonReturnClass { Id = 0, Val = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
            }
            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        }
    }
}