using P2b.Global;
using P2BUltimate.Models;
using Payroll;
using Leave;
using EMS;
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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class NoticePeriodSalaryProcessController : Controller
    {
        List<String> Msg = new List<string>();
        //
        // GET: /FFSSettlementDetailT/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/NoticePeriodSalaryProcess/Index.cshtml");
        }
        //public class P2BGridData
        //{
        //    public int Id { get; set; }
        //    public string Code { get; set; }
        //    public string Name { get; set; }
        //    public string LastWorkDateByComp { get; set; }
        //    public string LastWorkDateApproved { get; set; }


        //}
        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public double TotalEarning { get; set; }
            public double TotalDeduction { get; set; }
            public double TotalNet { get; set; }
            public string PayMonth { get; set; }
            public string ReleaseDate { get; set; }
            public Boolean Hold { get; set; }


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


                //   IEnumerable<NoticePeriodProcess> Corporate = null;

                var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
                    .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                     .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                    .Include(e => e.SeperationProcessT.NoticePeriodProcess)
                    .AsNoTracking().ToList();

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyExit.Where(e => e.Company.Id == compid)
                   .Include(e => e.EmployeeExit)
                   .Include(e => e.EmployeeExit.Select(a => a.Employee))
                   .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                   .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                   .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT))
                   .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT.NoticePeriodProcess))
                   .AsNoTracking().OrderBy(e => e.Id)
                  .SingleOrDefault();

                //var empdata = db.CompanyExit.Select(r => new
                //{
                //    compid = r.Company.Id,
                //    Employee = r.Id,

                //    EmployeeExit = r.EmployeeExit.Select(t => new
                //    {
                //        Employee = t.Employee.EmpName.FullNameFML,
                //        SeperationProcessT = t.SeperationProcessT.NoticePeriodProcess,
                //    }).ToList(),


                //}).SingleOrDefault();
                var emp = empdata.EmployeeExit.ToList();


                foreach (var z in emp)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.NoticePeriodProcess != null)
                    {
                        var EmployeeSeperationStruct = db.SalaryT
                                       .Where(e => e.EmployeePayroll.Employee.Id == z.Employee.Id && e.PayMonth == PayMonth).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                        if (EmployeeSeperationStruct!=null)
                        {
                                                 
                                view = new P2BGridData()
                                {
                                    Id = EmployeeSeperationStruct.Id,
                                    EmpCode = z.Employee.EmpCode,
                                    EmpName = z.Employee.EmpName.FullNameFML,
                                    TotalEarning = EmployeeSeperationStruct.TotalEarning,
                                    TotalDeduction = EmployeeSeperationStruct.TotalDeduction,
                                    TotalNet = EmployeeSeperationStruct.TotalNet,
                                    PayMonth = EmployeeSeperationStruct.PayMonth.ToString(),
                                    ReleaseDate = EmployeeSeperationStruct.ReleaseDate == null ? "" : EmployeeSeperationStruct.ReleaseDate.Value.Date.ToShortDateString(),
                                    Hold = EmployeeSeperationStruct.IsHold,

                                };

                                model.Add(view);
                        }
                        
                    }
                }

                SalaryList = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = SalaryList;
                    if (gp.searchOper.Equals("eq"))
                    {

                        //jsonData = IE.Where(e => (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))                             
                        //      || (e.Id.ToString().Contains(gp.searchString))
                        //      ).Select(a => new Object[] { a.Narration, a.Id }).ToList();
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                             || (e.EmpName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                             || (e.TotalNet != null ? e.TotalNet.ToString().Contains(gp.searchString) : false)
                              || (e.TotalEarning != null ? e.TotalEarning.ToString().Contains(gp.searchString) : false)
                             || (e.TotalDeduction != null ? e.TotalDeduction.ToString().Contains(gp.searchString) : false)
                              
                               || (e.PayMonth != null ? e.PayMonth.ToString().Contains(gp.searchString) : false)
                            || (e.ReleaseDate != null ? e.ReleaseDate.ToString().Contains(gp.searchString) : false)
                               || (e.Hold != null ? e.Hold.ToString().Contains(gp.searchString) : false)

                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalNet, a.TotalEarning, a.TotalDeduction, a.PayMonth, a.ReleaseDate != null ? Convert.ToString(a.ReleaseDate) : "", a.Hold, a.Id }).ToList();



                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalNet, a.TotalEarning, a.TotalDeduction, a.PayMonth, a.ReleaseDate != null ? Convert.ToString(a.ReleaseDate) : "", a.Hold, a.Id }).ToList();
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
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "TotalNet" ? c.TotalNet.ToString() :
                                          gp.sidx == "TotalEarning" ? c.TotalEarning.ToString() :
                                           gp.sidx == "TotalDeduction" ? c.TotalDeduction.ToString() :
                                            
                                             gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                              gp.sidx == "ReleaseDate" ? c.ReleaseDate.ToString() :
                                         gp.sidx == "Hold" ? c.Hold.ToString() :

                                    "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalNet, a.TotalEarning, a.TotalDeduction, a.PayMonth, a.ReleaseDate != null ? Convert.ToString(a.ReleaseDate) : "", a.Hold, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalNet, a.TotalEarning, a.TotalDeduction, a.PayMonth, a.ReleaseDate != null ? Convert.ToString(a.ReleaseDate) : "", a.Hold, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalNet, a.TotalEarning, a.TotalDeduction, a.PayMonth, a.ReleaseDate != null ? Convert.ToString(a.ReleaseDate) : "", a.Hold, a.Id }).ToList();
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
        //        IEnumerable<P2BGridData> SalaryList = null;
        //        List<P2BGridData> model = new List<P2BGridData>();
        //        P2BGridData view = null;

        //        //   IEnumerable<NoticePeriodProcess> Corporate = null;

        //        var empresig = db.EmployeeExit.Include(e => e.Employee.EmpName).Include(e => e.SeperationProcessT)
        //            .Include(e => e.SeperationProcessT.NoticePeriodProcess)
        //            .AsNoTracking().ToList();

        //        foreach (var z in empresig)
        //        {
        //            if (z.SeperationProcessT != null && z.SeperationProcessT.NoticePeriodProcess != null)
        //            {
        //                view = new P2BGridData()
        //                {
        //                    Id = z.SeperationProcessT.NoticePeriodProcess.Id,
        //                    Code = z.Employee.EmpCode,
        //                    Name = z.Employee.EmpName.FullNameFML,
        //                    LastWorkDateByComp = z.SeperationProcessT.NoticePeriodProcess.LastWorkDateByComp.Value.Date.ToShortDateString(),
        //                    LastWorkDateApproved = z.SeperationProcessT.NoticePeriodProcess.LastWorkDateApproved.Value.Date.ToShortDateString(),

        //                };
        //                model.Add(view);
        //            }
        //        }

        //        SalaryList = model;

        //        IEnumerable<P2BGridData> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField))
        //        {
        //            IE = SalaryList;
        //            if (gp.searchOper.Equals("eq"))
        //            {

        //                //jsonData = IE.Where(e => (e.Narration.ToUpper().ToString().Contains(gp.searchString.ToUpper()))                             
        //                //      || (e.Id.ToString().Contains(gp.searchString))
        //                //      ).Select(a => new Object[] { a.Narration, a.Id }).ToList();
        //                jsonData = IE.Where(e => (e.Code.ToString().Contains(gp.searchString))
        //                     || (e.Name.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
        //                      || (e.LastWorkDateByComp != null ? e.LastWorkDateByComp.ToString().Contains(gp.searchString) : false)
        //                     || (e.LastWorkDateApproved != null ? e.LastWorkDateApproved.ToString().Contains(gp.searchString) : false)

        //                     || (e.Id.ToString().Contains(gp.searchString))
        //                     ).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();



        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = SalaryList;
        //            Func<P2BGridData, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
        //                                 gp.sidx == "Name" ? c.Name.ToString() :
        //                                 gp.sidx == "LastWorkDateByComp" ? c.LastWorkDateByComp.ToString() :
        //                                 gp.sidx == "LastWorkDateApproved" ? c.LastWorkDateApproved.ToString() :


        //                            "");
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Select(a => new Object[] { a.Code, a.Name, a.LastWorkDateByComp != null ? Convert.ToString(a.LastWorkDateByComp) : "", a.LastWorkDateApproved != null ? Convert.ToString(a.LastWorkDateApproved) : "", a.Id }).ToList();
        //            }
        //            totalRecords = SalaryList.Count();
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
        public class ProcType
        {
            public int Id { get; set; }
            public string Text { get; set; }
        };
        public ActionResult Polulate_ProcTypeChk(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    List<ProcType> a = new List<ProcType>();
                    if (data.ToString().Split('/')[0] == "03")
                    {
                        a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" }
                    };
                    }
                    else
                    {
                        a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" },
                        new ProcType() { Id = 1, Text = "Declare Investment & Projected Income" },
                        new ProcType() { Id = 2, Text = "Actual Investment & Projected Income" }
                    };
                    }

                    SelectList s = new SelectList(a, "Id", "Text");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }


        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public EmployeePayroll _returnEmployeePayrollOne(Int32 empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeePayroll oEmployeePayroll = db.EmployeePayroll
                                   .Include(e => e.Employee.EmpName)
                                   .Include(e => e.Employee.ServiceBookDates)
                                   .Where(e => e.Id == empid)
                                   .FirstOrDefault();
                return oEmployeePayroll;
            }
        }
        public ActionResult ChkIFManual(string forwardata, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                List<int> ids = null;

                //string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string Emp = forwardata == "0" ? "" : forwardata;

                if (Emp != null && Emp != "0" && Emp != "" && Emp != "false")
                {
                    ids = one_ids(Emp);
                }
                else
                {
                    Msg.Add("Kindly select employee");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                var ItaxCheck = db.ITaxTransT
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Employee)
                    .AsNoTracking().Where(e => e.PayMonth == PayMonth && e.Mode.ToUpper() == "MAN" && ids.Contains(e.EmployeePayroll.Employee.Id)).Select(e => e.EmployeePayroll.Id).ToList();

                foreach (var EmpId in ItaxCheck)
                {
                    EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpId);


                    if ((EmpPay.Employee.ServiceBookDates.ServiceLastDate == null))
                    {
                        Msg.Add(EmpPay.Employee.FullDetails + ", Manual IncomeTax entry of this employee is entered ,");
                    }
                }
                if (Msg.Count() > 0)
                {
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return Json(new Utility.JsonReturnClass { success = true, responseText = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create(FFSSettlementDetailT S, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Lvhead = form["LvHead"] == "" ? "0" : Convert.ToString(form["LvHead"]);
                    // notice period salary auto tax Actual income Actual Investment
                    string ProcTypeList = "0";
                   // string ProcTypeList = form["ProcTypeList"] == "" ? "" : form["ProcTypeList"];
                    string AmountList = "";
                    List<int> LvHead_ids = null;
                    if (Lvhead != null && Lvhead != "0")
                    {
                        LvHead_ids = Utility.StringIdsToListIds(Lvhead);
                    }



                    //  string Empstruct_drop = form["Empstruct_drop"] == "0" ? "" : form["Empstruct_drop"];
                    bool AutoIncomeTax = false;
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }

                    List<int> ids = null;
                    if (Emp != "null" && Emp != "0" && Emp != "" && Emp != "false")
                    {
                        ids = one_ids(Emp);

                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    var GetProcessType = db.Lookup
                                         .Include(e => e.LookupValues)
                                          .Where(e => e.Code == "3030").FirstOrDefault();
                    if (GetProcessType == null)
                    {
                        Msg.Add("  Kindly Define in Lookup FFSETTLEMENT Process Type and code will 3030  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Boolean prtype = false;
                        var processtype = GetProcessType.LookupValues.ToList();
                        foreach (var item in processtype)
                        {
                            if (item.LookupVal.ToUpper() == "NOTICE PERIOD SALARY PAY")
                            {
                                prtype = true;
                                S.ProcessType = db.LookupValue.Find(item.Id);
                            }

                        }

                        if (prtype == false)
                        {
                            Msg.Add(" Kindly Define in Lookup 'Notice Period Salary Pay' under FFSETTLEMENT ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    string ProcessMonth = form["Create_Processmonth"] == "0 " ? "" : form["Create_Processmonth"];


                    string PaymentMonth = form["Create_Paymonth"] == "0" ? "" : form["Create_Paymonth"];
                    if (PaymentMonth == "")
                    {
                        Msg.Add(" Kindly Select Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }


                    string Paymentdate = form["Create_Paymentdate"] == "0" ? "" : form["Create_Paymentdate"];
                    if (Paymentdate == "")
                    {
                        Msg.Add(" Kindly Select Payment date ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //  string Reason = form["Create_Reason"] == "0" ? "" : form["Create_Reason"];

                    // List<int> SalHead = null;
                    int SalaryHead = form["SalaryHeadlist"] == "0" ? 0 : Convert.ToInt32(form["SalaryHeadlist"]);
                    Employee OEmployee = null;

                    string fromdate = form["fromdate"] == "0 " ? "" : form["fromdate"];
                    string Todate = form["Todate"] == "0 " ? "" : form["Todate"];
                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {

                            OEmployee = db.Employee
                          .Include(e => e.EmpName)
                          .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                          .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                          .Include(e => e.ServiceBookDates)
                          .Where(r => r.Id == i).SingleOrDefault();
                            EmployeePayroll OEmployeePayrollid = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault();

                            DateTime mFromPeriod = Convert.ToDateTime(fromdate).Date;

                            DateTime mEndDate = Convert.ToDateTime("01/" + Convert.ToDateTime(Todate).ToString("MM/yyyy")).AddMonths(1).Date;
                            mEndDate = mEndDate.AddDays(-1).Date;
                            for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
                            {
                                var attmonth = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                SalAttendanceT OSalattendanceT = db.EmployeePayroll.Where(t => t.Id == OEmployeePayrollid.Id).AsNoTracking().OrderBy(e => e.Id)
                    .Select(e => e.SalAttendance.Where(r => r.PayMonth == attmonth).FirstOrDefault()).SingleOrDefault();

                                if (OSalattendanceT == null)
                                {
                                    Msg.Add(OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + "-Attendance not enter for month " + Convert.ToDateTime(mTempDate).ToString("MM/yyyy"));
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                var EmpList1 = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.CPIEntryT).Where(e => e.Employee.Id == i).SingleOrDefault();
                                if (!EmpList1.CPIEntryT.Any(d => d.PayMonth == attmonth))
                                {
                                    Msg.Add(OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + "-cpindex is not defined for month " + attmonth);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                            }
                            ///
                            var OEmpSalT = db.EmployeeExit.Where(e => e.Employee.Id == i)
                                  .Include(e => e.SeperationProcessT)
                                  .Include(e => e.SeperationProcessT.FFSSettlementDetailT)
                                  .Include(e => e.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType))
                                  .AsNoTracking()
                                  .SingleOrDefault();
                            //if (OEmpSalT != null)
                            //{

                            //    var processtypechk = OEmpSalT.SeperationProcessT.FFSSettlementDetailT.Select(x => x.ProcessType).ToList();
                            //    Boolean salprocessed = false;
                            //    if (processtypechk != null)
                            //    {
                            //        foreach (var item in processtypechk)
                            //        {
                            //            if (item.LookupVal.ToUpper() == "NOTICE PERIOD SALARY PAY")
                            //            {
                            //                salprocessed = true;
                            //            }
                            //        }
                            //        if (salprocessed == true)
                            //        {
                            //            Msg.Add(OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + "Notice period salary processed you can not reprocess ");
                            //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //        }
                            //    }
                            //}
                        }

                    }

                    if (form["lblManualIT"] == "1")
                        AutoIncomeTax = false;
                    else if (form["lblManualIT"] == "2")
                        AutoIncomeTax = true;
                    else if (form["lblManualIT"] == "3")
                        AutoIncomeTax = false;


                    var salaryheadret = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == SalaryHead).FirstOrDefault();



                    EmployeePayroll OEmployeePayroll = null;
                    string EmpCode = null;
                    string EmpRel = null;




                    if (PaymentMonth != null && PaymentMonth != "")
                    {
                        S.PayMonth = Convert.ToDateTime(PaymentMonth);

                    }




                    DateTime Todaydate = DateTime.Now;

                    S.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    // notice period salary auto tax
                    AutoIncomeTax = true;

                    if (ids != null)
                    {
                        foreach (var i in ids)
                        {
                            List<FFSSettlementDetailT> ObjFAT = new List<FFSSettlementDetailT>();
                            // Salary Process Start
                            DateTime mFromPeriod = Convert.ToDateTime(fromdate).Date;

                            DateTime mEndDate = Convert.ToDateTime("01/" + Convert.ToDateTime(Todate).ToString("MM/yyyy")).AddMonths(1).Date;
                            mEndDate = mEndDate.AddDays(-1).Date;
                            using (TransactionScope ts1 = new TransactionScope())
                            {
                                for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddMonths(1))
                                {
                                    string PayMonth = Convert.ToDateTime(mTempDate).ToString("MM/yyyy");
                                    Msg.Clear();

                                    OEmployee = db.Employee
                                     .Include(e => e.EmpName)
                                     .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                                     .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                     .Include(e => e.ServiceBookDates)
                                     .Where(r => r.Id == i).SingleOrDefault();

                                    var EmpList1 = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.CPIEntryT).Where(e => e.Employee.Id == i).SingleOrDefault();
                                    if (!EmpList1.CPIEntryT.Any(d => d.PayMonth == PayMonth))
                                    {
                                        Msg.Add(OEmployee.CardCode + " " + OEmployee.EmpName.FullNameFML + "-cpindex is not defined for month " + PayMonth);
                                        continue;
                                    }

                                    CompanyPayroll OCompanyPayroll = null;
                                    OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Company.Id == CompId).SingleOrDefault();
                                    var OFinancia = OCompanyPayroll.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();

                                    ids = db.Employee.Where(e => ids.Contains(e.Id)).Select(r => r.Id).ToList();
                                    int ProcType = 0;
                                    if (AutoIncomeTax == true)
                                    {

                                        List<EmployeePayroll> emp_list = new List<EmployeePayroll>();
                                        List<int> HRAt = new List<int>();
                                        List<int> ITSection10Payment = new List<int>();


                                        int fyyear = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && e.Default == true).FirstOrDefault().Id;

                                        var HRAt1 = db.EmployeePayroll
                                        .Where(t => t.HRATransT.Count() > 0 && ids.Contains(t.Employee.Id)).Select(e => new
                                        {
                                            HRATransT = e.HRATransT.Where(s => s.Financialyear.Id == fyyear).ToList(),
                                            Id = e.Id,
                                        }).ToList();

                                        HRAt = HRAt1.Select(e => e.Id).ToList();

                                        var ITSection10Payment1 = db.EmployeePayroll
                                           .Where(t => t.ITSection10Payment.Count() > 0 && ids.Contains(t.Employee.Id)).Select(e => new
                                           {
                                               ITSection10Payment = e.ITSection10Payment.Where(s => s.FinancialYear.Id == fyyear).ToList(),
                                               Id = e.Id,
                                           }).ToList();


                                        ITSection10Payment = ITSection10Payment1.Select(e => e.Id).ToList();

                                        var list2 = HRAt.Except(ITSection10Payment);

                                        if (list2.Count() > 0)
                                        {
                                            foreach (var EmpId in list2)
                                            {
                                                EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpId);

                                                emp_list.Add(EmpPay);
                                                Msg.Add(EmpPay.Employee.FullDetails + ", HRA rent is entered and ITSection10Payment of this employee is not entered ,Please enter ITSection10Payment and then process salary ,");

                                            }
                                        }

                                        if (Msg.Count() > 0)
                                        {
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }


                                        if (ProcTypeList != "")
                                        {
                                            ProcType = Convert.ToInt32(ProcTypeList);
                                            if (ProcType == 0)
                                                AmountList = "Actual";
                                            if (ProcType == 1)
                                                AmountList = "Declared";
                                            if (ProcType == 2)
                                                AmountList = "Actual";
                                        }
                                        else
                                        {
                                            Msg.Add(" Kindly select Tax Calculation method  ");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }

                                    }

                                    SessionManager.CheckProcessStatus = "Salary Gen";
                                    Msg = SalaryGen.PreSalCheck(0, PayMonth, AutoIncomeTax, OCompanyPayroll.Id, AmountList, ProcType, ids);
                                    if (Msg.Count() == 0)
                                    {
                                        int ErrNo = 0;

                                        Utility.DumpProcessStatus("Emp Salary Process Started");

                                        ErrNo = SalaryGen.EmployeePayrollProcess(EmpList1.Id, PayMonth, AutoIncomeTax, OCompanyPayroll.Id, AmountList, ProcType);
                                        if (ErrNo != 0 || ErrNo == 0)
                                            Msg.Add(OEmployee.EmpCode + " - " + OEmployee.EmpName.FullNameFML);
                                        // for auotax cal start
                                        using (DataBaseContext dbauto = new DataBaseContext())
                                        {

                                            if (ErrNo == 0 && AutoIncomeTax == true)
                                            {
                                                Employee OEmployeeauto = dbauto.Employee
                                                 .Include(e => e.EmpName)
                                                 .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company)
                                                 .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                                 .Include(e => e.ServiceBookDates)
                                                 .Where(r => r.Id == i).SingleOrDefault();
                                                int CompIdnew = Convert.ToInt32(SessionManager.CompanyId);
                                                CompanyPayroll OCompanyPayrollnew = dbauto.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Id == OCompanyPayroll.Id).SingleOrDefault();
                                                var OFinancianew = OCompanyPayrollnew.Company.Calendar.Where(r => r.Default == true && r.Name.LookupVal.ToString().ToUpper() == "FINANCIALYEAR").SingleOrDefault();

                                                DateTime FromPeriod = Convert.ToDateTime(OFinancianew.FromDate);
                                                DateTime ToPeriod = Convert.ToDateTime(OFinancianew.ToDate);

                                                if (OEmployeeauto.ServiceBookDates.JoiningDate >= OFinancianew.FromDate)
                                                {
                                                    FromPeriod = Convert.ToDateTime(OEmployeeauto.ServiceBookDates.JoiningDate);
                                                }
                                                else if (OEmployeeauto.ServiceBookDates.ServiceLastDate >= OFinancianew.FromDate &&
                                                   OEmployeeauto.ServiceBookDates.ServiceLastDate <= OFinancianew.ToDate)
                                                {
                                                    ToPeriod = Convert.ToDateTime(OEmployeeauto.ServiceBookDates.ServiceLastDate);
                                                }
                                                else if (OEmployeeauto.ServiceBookDates.RetirementDate >= OFinancianew.FromDate &&
                                                   OEmployeeauto.ServiceBookDates.RetirementDate <= OFinancianew.ToDate)
                                                {
                                                    ToPeriod = Convert.ToDateTime(OEmployeeauto.ServiceBookDates.RetirementDate);
                                                }

                                                //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                //                 new System.TimeSpan(0, 30, 0)))
                                                //{
                                                Utility.DumpProcessStatus("Emp IncomeTax Process Started");
                                                OEmployeePayroll = dbauto.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == OEmployee.Id)//.Include(e => e.EmpSalStruct)

                                 .Include(e => e.Employee.EmpOffInfo)
                                 .Include(e => e.Employee.EmpOffInfo.NationalityID)
                                 .Include(e => e.Employee.Gender)
                                   .Include(e => e.Employee.ServiceBookDates)
                                    .Include(e => e.Employee.EmpName)
                                    .Include(e => e.ITProjection)
                                    .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                                     .Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme))
                                   .FirstOrDefault();//.AsParallel().SingleOrDefault();
                                                OEmployeePayroll.RegimiScheme = OEmployeePayroll.RegimiScheme.Where(e => e.FinancialYear_Id == OFinancia.Id).ToList();

                                                List<ITProjection> FinalOITProjectionDataList = new List<ITProjection>();

                                                double SalAttendanceT_monthDays = 0;
                                                double SalAttendanceT_PayableDays = 0;
                                                SalAttendanceT OSalattendanceT = dbauto.EmployeePayroll.Where(t => t.Id == OEmployeePayroll.Id).AsNoTracking().OrderBy(e => e.Id)
                                        .Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                                                if (OSalattendanceT != null)
                                                {
                                                    SalAttendanceT_monthDays = OSalattendanceT.MonthDays;
                                                    SalAttendanceT_PayableDays = OSalattendanceT.PaybleDays;
                                                }
                                                else
                                                {
                                                    //SalAttendanceT_monthDays=
                                                    SalAttendanceT_monthDays = Convert.ToDouble(DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0])));
                                                    SalAttendanceT_PayableDays = 0;//Changed by Rohit
                                                }

                                                double TaxPaidAmt = 0;
                                                SalaryT OSal = dbauto.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll_Id == OEmployeePayroll.Id).SingleOrDefault();

                                                TaxPaidAmt = IncomeTaxCalc.ITCalculation(OEmployeePayroll, CompId, OFinancia.Id, FromPeriod, ToPeriod, DateTime.Now, OSal, AmountList, 1, ProcType);
                                                if (SalAttendanceT_PayableDays == 0)
                                                {
                                                    TaxPaidAmt = 0;
                                                }


                                                using (DataBaseContext db1 = new DataBaseContext())
                                                {
                                                    ITaxTransT ITaxEmp = db1.EmployeePayroll.Where(e => e.Id == OEmployeePayroll.Id).Select(e => e.ITaxTransT.Where(r => r.PayMonth == PayMonth).FirstOrDefault()).SingleOrDefault();
                                                    if (ITaxEmp.Mode == "AUTO")
                                                    {
                                                        ITaxTransT OITaxT = db1.ITaxTransT.Find(ITaxEmp.Id);
                                                        OITaxT.TaxPaid = TaxPaidAmt;
                                                        db1.ITaxTransT.Attach(OITaxT);
                                                        db1.Entry(OITaxT).State = System.Data.Entity.EntityState.Modified;
                                                        db1.SaveChanges();
                                                        db1.Entry(OITaxT).State = System.Data.Entity.EntityState.Detached;

                                                        //Not require data inseret here data will go direct salary process table

                                                        //SalaryT OSalT = db.SalaryT.Where(e => e.Id == OSal.Id)
                                                        //    .Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.SalaryHead))
                                                        //    .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)).SingleOrDefault();
                                                        //var OSalEarnDet = OSalT.SalEarnDedT.ToList();
                                                       
                                                        //foreach (var item in OSalEarnDet)
                                                        //{
                                                        //    if (item.Amount != 0)
                                                        //    {

                                                        //        var val = db.SalaryHead
                                                        //        .Include(e => e.Type)
                                                        //        .Include(e => e.ProcessType)
                                                        //         .Include(e => e.SalHeadOperationType)
                                                        //        .Where(e => e.Id == item.SalaryHead.Id).FirstOrDefault();
                                                               
                                                        //        S.SalaryHead = val.Code;
                                                        //        S.SalaryHeadDesc = val.Name;
                                                        //        S.SalType = val.Type.LookupVal.ToUpper().ToString();
                                                        //        FFSSettlementDetailT ObjFATnew = new FFSSettlementDetailT();
                                                        //        {
                                                                    
                                                        //            ObjFATnew.PayAmount = item.Amount;
                                                        //            ObjFATnew.PayMonth = S.PayMonth;
                                                        //            ObjFATnew.SalaryHead = S.SalaryHead;
                                                        //            ObjFATnew.SalaryHeadDesc = S.SalaryHeadDesc;
                                                        //            ObjFATnew.ProcessType = S.ProcessType;
                                                        //            ObjFATnew.SalType = S.SalType;
                                                        //            ObjFATnew.PayDate = Convert.ToDateTime(Paymentdate);
                                                        //            ObjFATnew.IsPaid = false;
                                                        //            ObjFATnew.DBTrack = S.DBTrack;
                                                        //            if (val.SalHeadOperationType.LookupVal.ToUpper() == "ITAX")
                                                        //            {
                                                        //                ObjFATnew.PayAmount = TaxPaidAmt;
                                                        //            }
                                                        //        }
                                                        //        ObjFAT.Add(ObjFATnew);
                                                        //    }

                                                        //}
                                                        //Not require data inseret here data will go direct salary process table

                                                        SalaryT OSalT = db.SalaryT.Where(e => e.Id == OSal.Id)
                                       .Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.SalaryHead))
                                       .Include(e => e.SalEarnDedT.Select(r => r.SalaryHead.SalHeadOperationType)).SingleOrDefault();
                                                        SalEarnDedT OSalEarnDet = OSalT.SalEarnDedT.Where(e => e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "ITAX").SingleOrDefault();
                                                        OSalT.TotalDeduction = OSalT.TotalDeduction + TaxPaidAmt;
                                                        OSalT.TotalNet = OSalT.TotalEarning - OSalT.TotalDeduction;
                                                        OSalT.IsHold = true;
                                                        db1.SalaryT.Attach(OSalT);
                                                        db1.Entry(OSalT).State = System.Data.Entity.EntityState.Modified;
                                                        db1.SaveChanges();
                                                        OSalEarnDet.Amount = TaxPaidAmt;
                                                        OSalEarnDet.StdAmount = TaxPaidAmt;
                                                        db1.SalEarnDedT.Attach(OSalEarnDet);
                                                        db1.Entry(OSalEarnDet).State = System.Data.Entity.EntityState.Modified;
                                                        db1.SaveChanges();

                                                    }


                                                }

                                            }
                              

                                        }

                                        // for auotax cal end

                                        //   Msg = SalaryGen.SalaryProcess(0, PayMonth, AutoIncomeTax, OCompanyPayroll.Id, AmountList, ProcType, ids);
                                    }
                                    else
                                    {
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                ts1.Complete();
                            }
                            // Salary Process End



                            //using (TransactionScope ts = new TransactionScope())
                            //{
                            //    try
                            //    {
                            //        List<FFSSettlementDetailT> OFFSList = new List<FFSSettlementDetailT>();
                            //        db.FFSSettlementDetailT.AddRange(ObjFAT);
                            //        db.SaveChanges();
                            //        OFFSList.AddRange(ObjFAT);
                            //        var aa = db.EmployeeExit.Include(x => x.SeperationProcessT).Include(e => e.SeperationProcessT.FFSSettlementDetailT).Where(e => e.Employee.Id == i).FirstOrDefault();
                            //        if (aa.SeperationProcessT == null)
                            //        {
                            //            SeperationProcessT OTEP = new SeperationProcessT()
                            //            {
                            //                FFSSettlementDetailT = OFFSList,
                            //                DBTrack = S.DBTrack
                            //            };


                            //            db.SeperationProcessT.Add(OTEP);

                            //            db.SaveChanges();

                            //            aa.SeperationProcessT = OTEP;
                            //            db.EmployeeExit.Attach(aa);
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //            db.SaveChanges();
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            //        }
                            //        else
                            //        {
                            //            OFFSList.AddRange(aa.SeperationProcessT.FFSSettlementDetailT);
                            //            aa.SeperationProcessT.FFSSettlementDetailT = OFFSList;
                            //            db.EmployeeExit.Attach(aa);
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //            db.SaveChanges();
                            //            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            //        }
                            //        ts.Complete();

                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        //   List<string> Msg = new List<string>();
                            //        Msg.Add(ex.Message);
                            //        LogFile Logfile = new LogFile();
                            //        ErrorLog Err = new ErrorLog()
                            //        {
                            //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                            //            ExceptionMessage = ex.Message,
                            //            ExceptionStackTrace = ex.StackTrace,
                            //            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                            //            LogTime = DateTime.Now
                            //        };
                            //        Logfile.CreateLogFile(Err);
                            //        //return Json(new { sucess = false, Msg }, JsonRequestBehavior.AllowGet);
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }


                            //}

                           
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



        public ActionResult getResigndate(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ResignPerioddt ResignPerioddt = new ResignPerioddt();
                int Id = int.Parse(data);

                if (Id != 0)
                {

                    var query = db.EmployeeExit.Where(e => e.Employee.Id == Id).Include(e => e.SeperationProcessT)
                     .Include(e => e.SeperationProcessT.NoticePeriodProcess)
                      .AsEnumerable()
                      .Select(e => new
                      {

                          LastWorkDateByComp = e.SeperationProcessT.NoticePeriodProcess.LastWorkDateByComp.Value.ToShortDateString(),
                          LastWorkDateApproved = e.SeperationProcessT.NoticePeriodProcess.LastWorkDateApproved.Value.ToShortDateString(),
                          Waive = e.SeperationProcessT.NoticePeriodProcess.WaiveDays.ToString()
                      })

                      .SingleOrDefault();

                    ResignPerioddt = new ResignPerioddt()
                    {
                        Fromperiod = query.LastWorkDateByComp.ToString(),
                        Toperiod = query.LastWorkDateApproved.ToString(),
                        Waive = query.Waive.ToString(),

                    };

                    return Json(new { ResignPerioddt }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }

            }
        }
        public class ResignPerioddt
        {

            public string Fromperiod { get; set; }
            public string Toperiod { get; set; }
            public string Waive { get; set; }

        }
        public class Perioddt
        {

            public string Periodfrom { get; set; }

        }

        public ActionResult getSalaryProcessdate(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Perioddt Perioddtt = new Perioddt();
                DateTime FromPerioddt;
                int Id = int.Parse(data);
                if (Id != 0)
                {


                    int OEmployeePayrollId
                             = db.EmployeeLeave.Where(e => e.Employee.Id == Id).SingleOrDefault().Id;

                    var query = db.SalaryT.Where(e => e.EmployeePayroll.Id == OEmployeePayrollId).OrderByDescending(e => e.Id)
                        .Select(e => new
                        {
                            Fromperiod = e.PayMonth.ToString()
                        })
                        .FirstOrDefault();
                    FromPerioddt = Convert.ToDateTime("01/" + query.Fromperiod).AddMonths(1).Date;

                    Perioddtt = new Perioddt()
                    {
                        Periodfrom = FromPerioddt.ToShortDateString(),

                    };

                    return Json(new { Perioddtt }, JsonRequestBehavior.AllowGet);
                    //   return Json(query, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }





        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }


        }
        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            DataBaseContext db = new DataBaseContext();

            List<int> lvheadids = null;



            try
            {
                DateTime? dt = null;
                string monthyr = "";
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;

                List<Employee> EmpList1 = new List<Employee>();

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


                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var CompanyPayroll_Id = db.CompanyPayroll.Where(e => e.Company.Id == compid).SingleOrDefault();
                CompanyExit CompanyExit = new CompanyExit();
                CompanyExit = db.CompanyExit.Include(e => e.Company).Where(e => e.Company.Id == compid).FirstOrDefault();

                var empdata = db.CompanyExit.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeExit)
                    .Include(e => e.EmployeeExit.Select(a => a.Employee))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT))
                    .Include(e => e.EmployeeExit.Select(a => a.SeperationProcessT.NoticePeriodProcess))
                    .AsNoTracking().OrderBy(e => e.Id)
                   .SingleOrDefault();

                var emp = empdata.EmployeeExit.ToList();

                foreach (var z in emp)
                {
                    if (z.SeperationProcessT != null && z.SeperationProcessT.NoticePeriodProcess != null)
                    {

                        var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                                       .Include(e => e.EmployeeSeperationStructDetails)
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_Config_Policy))
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                                       .Where(e => e.EmployeeExit.Id == z.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();


                        view = new P2BCrGridData()
                        {
                            Id = z.Employee.Id,
                            Code = z.Employee.EmpCode,
                            Name = z.Employee.EmpName.FullNameFML
                        };
                        // Retire employee
                        if (EmployeeSeperationStruct != null)
                        {
                            model.Add(view);

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
                                         gp.sidx == "EmpCode" ? c.Code.ToString() :
                                         gp.sidx == "EmpName" ? c.Name.ToString() : ""


                        );
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


    }
}