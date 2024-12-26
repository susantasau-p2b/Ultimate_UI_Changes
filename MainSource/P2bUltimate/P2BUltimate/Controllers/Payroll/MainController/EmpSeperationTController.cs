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
using P2BUltimate.Process;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace P2BUltimate.Controllers.Leave.MainController
{
    public class EmpSeperationTController : Controller
    {

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/EmpSeperationT/Index.cshtml");
        }
        public List<KeyValuePair<Int32, string>> ErrorMsg()
        {
            List<KeyValuePair<Int32, string>> LookupStatus = new List<KeyValuePair<Int32, string>>()
                {
                    new KeyValuePair<Int32, string>(0, "Wrong Entry"),
                    new KeyValuePair<Int32, string>(1, "In time Missing"),
                    new KeyValuePair<Int32, string>(2, "Out time Missing"),
                    new KeyValuePair<Int32, string>(3, "Holiday WO"),
                    new KeyValuePair<Int32, string>(4, "In Late"),
                     new KeyValuePair<Int32, string>(5, "Manual entry"),
                     new KeyValuePair<Int32, string>(6, "Out Early"),
                };
            return LookupStatus;
        }
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string LvCreditDate { get; set; }
            public string ClosedBalance { get; set; }
            public string LeaveCode { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }

        }
        public class P2BCrGridData1
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string LvCreditDate { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }

        }
        public ActionResult LoadEmp(P2BGrid_Parameters gp, string param1, string param2, string param3, string param4)
        {
            DataBaseContext db = new DataBaseContext();

            List<int> lvheadids = null;
            if (param4 != null && param4 != "0" && param4 != "false")
            {
                lvheadids = Utility.StringIdsToListIds(param4);
            }

            DateTime? fromdate = null;
            DateTime? todate = null;
            if (param1 != "" && param2 != "")
            {
                fromdate = Convert.ToDateTime(param1);
                todate = Convert.ToDateTime(param2);
            }
            var lookupid = Convert.ToInt32(param3);
            var CreditDatelist = db.LookupValue.Where(e => e.Id == lookupid).SingleOrDefault();

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


                //var compid = Convert.ToInt32(Session["CompId"].ToString());


                //var empdata = db.CompanyLeave
                //   .Include(e => e.EmployeeLeave)
                //   .Include(e => e.EmployeeLeave.Select(a => a.Employee))
                //   .Include(e => e.EmployeeLeave.Select(a => a.Employee.EmpName))
                //   .Include(e => e.EmployeeLeave.Select(a => a.Employee.ServiceBookDates))
                //   .Where(e => e.Company.Id == compid).SingleOrDefault();

                //var emp = empdata.EmployeeLeave.Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null || e.Employee.ServiceBookDates.ServiceLastDate >= System.DateTime.Now.Date).Select(e => e.Employee).ToList();


                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var CompanyPayroll_Id = db.CompanyPayroll.Where(e => e.Company.Id == compid).SingleOrDefault();
                CompanyExit CompanyExit = new CompanyExit();
                CompanyExit = db.CompanyExit.Include(e => e.Company).Where(e => e.Company.Id == compid).FirstOrDefault();

                var empdata = db.CompanyExit.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeExit)
                    .Include(e => e.EmployeeExit.Select(a => a.Employee))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeExit.Select(a => a.Employee.ServiceBookDates))
                    .AsNoTracking().OrderBy(e => e.Id)
                   .SingleOrDefault();

                var emp = empdata.EmployeeExit.ToList();

                foreach (var z in emp)
                {
                    if (z.Employee.ServiceBookDates.FFSCompletionDate == null)
                    {

                        var EmployeeSeperationStruct = db.EmployeeSeperationStruct
                                       .Include(e => e.EmployeeSeperationStructDetails)
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationMaster))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula))
                                        .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyFormula.ExitProcess_Config_Policy))
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment))
                                       .Include(e => e.EmployeeSeperationStructDetails.Select(t => t.SeperationPolicyAssignment.PayScaleAgreement))
                                       .Where(e => e.EmployeeExit.Id == z.Id && e.EndDate == null).AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                        //SalAttendanceT OAtt = z.SalAttendance.Where(e => e.PayMonth == monthyr).SingleOrDefault();
                        //if (OAtt == null)
                        //{


                        view = new P2BCrGridData()
                        {
                            Id = z.Employee.Id,
                            Code = z.Employee.EmpCode,
                            Name = z.Employee.EmpName.FullNameFML
                        };

                        // Resign,EXPIRED,TERMINATION
                        var OOtherServiceBook = db.EmployeePayroll.Where(e => e.Employee.Id == z.Employee.Id)
                        .Include(e => e.OtherServiceBook)
                         .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity))
                         .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity.OtherSerBookActList))
                       .SingleOrDefault();
                        var OOOtherServiceBook = OOtherServiceBook.OtherServiceBook.Where(e => e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "RESIGNED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "EXPIRED" || e.OthServiceBookActivity.OtherSerBookActList.LookupVal.ToUpper() == "TERMINATION").Select(x => x.OthServiceBookActivity.OtherSerBookActList)
                                                 .FirstOrDefault();
                        if (OOOtherServiceBook != null)
                        {
                            string check = OOOtherServiceBook.LookupVal.ToUpper().ToString();
                            if (check != null)
                            {
                                if (check == "RESIGNED" || check == "EXPIRED" || check == "TERMINATION")
                                {
                                    model.Add(view);
                                    continue;
                                }
                            }
                        }

                        // Retire employee
                        if (EmployeeSeperationStruct != null)
                        {
                            var OEmployeeSeperationStructDet = EmployeeSeperationStruct.EmployeeSeperationStructDetails.Where(r => r.SeperationPolicyFormula.ExitProcess_Config_Policy.Count() > 0).FirstOrDefault();
                            if (OEmployeeSeperationStructDet != null)
                            {
                                var exitpolicyday = OEmployeeSeperationStructDet.SeperationPolicyFormula.ExitProcess_Config_Policy.FirstOrDefault();

                                DateTime actRetdate = Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.AddDays(exitpolicyday.FFSSettlementPeriod_FromLastWorkDay));
                                // FFSSettlementPeriod_FromLastWorkDay +5 days
                                if ((DateTime.Now.Date >= Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy")) && DateTime.Now.Date <= actRetdate.Date))
                                {
                                    model.Add(view);
                                }
                                // FFSSettlementPeriod_FromLastWorkDay -5 days
                               // if ((DateTime.Now.Date >= actRetdate.Date && Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy")) <= DateTime.Now.Date))
                                if ((DateTime.Now.Date >= actRetdate.Date && DateTime.Now.Date <= Convert.ToDateTime(z.Employee.ServiceBookDates.RetirementDate.Value.ToString("dd/MM/yyyy"))))
                                {
                                    model.Add(view);
                                }
                            }
                            continue;
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
                                || (e.LvCreditDate.ToString().Contains(gp.searchString))
                                || (e.Status.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
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
                                         gp.sidx == "EmpName" ? c.Name.ToString() :
                                         gp.sidx == "Creditdate" ? c.LvCreditDate.ToString() :
                                         gp.sidx == "Status" ? c.Status.ToString() : ""
                        );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.Status, a.Id }).ToList();
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
                System.Web.HttpContext.Current.Session["LeaveCreditProcessEmpids"] = JsonData;
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult LoadEmpWithCreditDate(P2BGrid_Parameters gp)
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
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).AsNoTracking()
                    .Where(e => e.Company.Id == compid).SingleOrDefault();

                var emp = empdata.EmployeePayroll.Select(e => e.Employee).ToList();

                foreach (var z in emp)
                {
                  //  var EmplvList1 = db.EmployeeLeave.Include(e => e.LvNewReq).AsNoTracking().Where(e => e.Employee.Id == z.Id && e.LvNewReq.Any(a => a.LvCreditDate != null)).SingleOrDefault();
                    var EmplvList1 = db.EmployeeLeave.Include(e => e.LvNewReq).AsNoTracking().Where(e => e.Employee.Id == z.Id && e.LvNewReq.Any(a => a.Narration == "Settlement Process")).SingleOrDefault();

                    if (EmplvList1 != null)
                    {



                        //view = new P2BCrGridData()
                        //{
                        //    Id = z.Id,
                        //    Code = z.EmpCode,
                        //    Name = z.EmpName.FullNameFML
                        //};
                       
                            //var EmplvList = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(a => a.WFStatus)).AsNoTracking().Where(e => e.Employee.Id == z.Id && e.LvNewReq.Any(a => a.LvCreditDate != null)).SingleOrDefault();
                        var EmplvList = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(a => a.WFStatus)).AsNoTracking().Where(e => e.Employee.Id == z.Id && e.LvNewReq.Any(a => a.Narration == "Settlement Process")).SingleOrDefault();


                            if (EmplvList != null)
                            {
                                var LvNewReqList = EmplvList.LvNewReq.Where(e => e.LvCreditDate != null && e.WFStatus.LookupVal == "3").OrderByDescending(e => e.Id).ToList();
                                if (LvNewReqList != null)
                                {
                                    foreach (var item in LvNewReqList)
                                    {
                                        view = new P2BCrGridData();
                                        view.Id = z.Id;
                                        view.Code = z.EmpCode;
                                        view.Name = z.EmpName.FullNameFML;
                                        view.LvCreditDate = item.LvCreditDate.Value.ToShortDateString();
                                        view.ClosedBalance = item.CloseBal.ToString();
                                        view.LeaveCode = item.LeaveHead.LvCode.ToString();

                                        view.Status = "true";
                                        model.Add(view);
                                    }

                                }
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
                                || (e.LvCreditDate != null ? e.LvCreditDate.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.ClosedBalance != null ? e.ClosedBalance.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.LeaveCode != null ? e.LeaveCode.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.Status != null ? e.Status.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate != null ? a.LvCreditDate : "", a.Status != null ? a.Status : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.Name, a.LvCreditDate, a.ClosedBalance, a.LeaveCode, a.Status, a.Id }).ToList();
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
                                         gp.sidx == "EmpName" ? c.Name.ToString() :
                                         gp.sidx == "LvCreditDate" ? c.LvCreditDate.ToString() :
                                         gp.sidx == "ClosedBalance" ? c.ClosedBalance.ToString() :
                                         gp.sidx == "LeaveCode" ? c.LeaveCode.ToString() :
                                         gp.sidx == "Status" ? c.Status.ToString() : ""

                                         );
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.ClosedBalance, a.LeaveCode, a.Status, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.ClosedBalance, a.LeaveCode, a.Status, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.Code), a.Name, a.LvCreditDate, a.ClosedBalance, a.LeaveCode, a.Status, a.Id }).ToList();
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

        public ActionResult FindLeaveEmpId()
        {
            dynamic EmpLvIds = System.Web.HttpContext.Current.Session["LeaveCreditProcessEmpids"];
            return Json(EmpLvIds, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingLvCheck(String LvHead, String pendingleaveempids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> ids = new List<int>();
                List<dynamic> Empids = JsonConvert.DeserializeObject<List<dynamic>>(pendingleaveempids);

                foreach (dynamic item in Empids)
                {
                    int ids1 = Convert.ToInt32(item);
                    ids.Add(ids1);
                }

                if (ids == null)
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please select the employee and try again..!" }, JsonRequestBehavior.AllowGet);
                }

                Session["EmployeeLvIdReport"] = ids;

                //if (lvheadList != null && lvheadList != "0" && lvheadList != "false")
                //{
                //    lvheadids = Utility.StringIdsToListIds(lvheadList);
                //}
                List<dynamic> lvheadid = JsonConvert.DeserializeObject<List<dynamic>>(LvHead);

                List<int> lvheadids = new List<int>();

                foreach (dynamic item1 in lvheadid)
                {
                    int lvheadids1 = Convert.ToInt32(item1);
                    lvheadids.Add(lvheadids1);
                }
                var emp = db.Employee.Where(e => ids.Contains(e.Id)).ToList().Select(q => q.Id);
                foreach (var oEmployeeId in emp)
                {
                    EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                                   .Include(a => a.LvNewReq)
                                   .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                   .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                   .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                   .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                   .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                   .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                   .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null
                                   && a.InputMethod == 1 && a.TrClosed == false
                                   && lvheadids.Contains(a.LeaveHead.Id))).SingleOrDefault();

                    if (_Prv_EmpLvData != null)
                    {
                        bool status1 = true;
                        var data1 = new
                        {
                            status = status1,

                        };
                        return Json(data1, JsonRequestBehavior.AllowGet);
                    }

                }
                //var LvNewReqData = db.LvNewReq.Where(e => e.InputMethod == 1 && e.TrClosed == false && lvheadids.Contains(e.LeaveHead.Id)).ToList();

                //if (LvNewReqData.Count() > 0)
                //{
                //    bool status1 = true;
                //    var data1 = new
                //    {
                //        status = status1,

                //    };
                //    return Json(data1, JsonRequestBehavior.AllowGet);
                //}
                return null;
            }
        }


        [HttpPost]
        public ActionResult GetLvhead(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                int forwardata = Convert.ToInt32(data);

                var dd = db.LookupValue.Where(e => e.Id == forwardata).Select(q => q.LookupVal).SingleOrDefault();
                if (dd.ToUpper() == "CALENDAR")
                {

                    var a = db.LvCreditPolicy.Include(e => e.LvHead).Where(e => e.CreditDate.Id == forwardata ).ToList();
                    if (a.Count > 0)
                    {
                        SelectList s = new SelectList(a.Select(e => e.LvHead).ToList(), "id", "FullDetails");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    var a = db.LvCreditPolicy.Include(e => e.LvHead).Where(e => e.CreditDate.Id == forwardata ).ToList();
                    if (a.Count > 0)
                    {
                        SelectList s = new SelectList(a.Select(e => e.LvHead).ToList(), "id", "FullDetails");
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);

                    }
                }
                return null;
            }
        }
        [HttpPost]
        public ActionResult Create(FormCollection form, String forwarddata)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    string Emp = forwarddata == "0" ? "" : forwarddata;
                    string Lvhead = form["LvHead"] == "" ? "0" : Convert.ToString(form["LvHead"]);
                    string FromDate1 = form["FromDate"] == "" ? "" : form["FromDate"];
                    string ToDate1 = form["ToDate"] == "" ? "" : form["ToDate"];
                    string CreditDatelist = form["CreditDatelist"] == "" ? "0" : Convert.ToString(form["CreditDatelist"]);
                    DateTime? FromDate = null;
                    DateTime? ToDate = null;
                    if (FromDate1 != "")
                    {
                        FromDate = Convert.ToDateTime(FromDate1);
                        ToDate = Convert.ToDateTime(ToDate1);

                    }

                    List<int> ids = null;
                    List<int> LvHead_ids = null;

                    if (Emp == "null")
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = "Please select the employee and try again..!" }, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }

                    if (Lvhead != null && Lvhead != "0")
                    {
                        LvHead_ids = Utility.StringIdsToListIds(Lvhead);
                    }


                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = int.Parse(Session["CompId"].ToString());
                    }
                    //DateTime? joinincrpromdate=
                    CompanyPayroll OCompanyPayroll = null;
                    OCompanyPayroll = db.CompanyPayroll.Include(e => e.Company).Include(e => e.Company.Calendar).Include(e => e.Company.Calendar.Select(r => r.Name)).Where(e => e.Company.Id == CompId).SingleOrDefault();


                    //var LeavenewReq = db.LvNewReq.Where(e => e.InputMethod == 1 && e.TrClosed == false && LvHead_ids.Contains(e.LeaveHead.Id)).ToList();

                    //if (LeavenewReq.Count() > 0)
                    //{
                    //    var url = "rptview/rptpendingleavecreditlvr?parm=22";
                    //    return JavaScript("alert('Please Approve the pending leave and try again..!!') ;window.location = '" + url + "'");
                    //}

                    var emp = db.Employee.Where(e => ids.Contains(e.Id)).ToList().Select(q => q.Id);
                    int pend = 0;
                    foreach (var oEmployeeId in emp)
                    {
                        EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
                            .Include(a => a.Employee)
                                       .Include(a => a.LvNewReq)
                                       .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                       .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                       .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                       .Include(e => e.LvNewReq.Select(q => q.LvWFDetails))
                                       .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.InputMethod == 1 && a.TrClosed == false && LvHead_ids.Contains(a.LeaveHead.Id) && a.Narration.ToUpper() == "Settlement Process".ToUpper()))
                                           .SingleOrDefault();
                        if (_Prv_EmpLvData != null)
                        {

                            pend = 1;

                            var Lv = _Prv_EmpLvData.LvNewReq.Where(a => LvHead_ids.Contains(a.LeaveHead.Id)).ToList();
                            foreach (var ca1 in Lv)
                            {
                                if (ca1.TrClosed == false && ca1.InputMethod == 1)
                                {
                                    foreach (var ca2 in ca1.LvWFDetails)
                                    {
                                        Msg.Add("Please Approve the pending leave and try again..!!");
                                        Msg.Add(_Prv_EmpLvData.Employee.EmpCode + "-" + ca1.FromDate + " - " + ca1.ToDate + " - " + ca1.LeaveHead.LvCode);
                                    }
                                }
                            }


                        }


                    }
                    if (pend == 1)
                    {
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeeLeave oEmployeeLeave = null;
                    var CompLvId = Convert.ToInt32(SessionManager.CompLvId);
                    //using (DataBaseContext db = new DataBaseContext())
                    //{
                    //var CompCreditPolicyList = db.CompanyLeave
                    //     .Include(e => e.LvCreditPolicy)
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHead))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.LvHead))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.CreditDate))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.ConvertLeaveHeadBal))
                    //     .Include(e => e.LvCreditPolicy.Select(a => a.ExcludeLeaveHeads))
                    //     .Where(e => e.Id == CompLvId).SingleOrDefault();
                    var lvcalendarid = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").Select(e => e.Id).SingleOrDefault();

                    if (ids != null)
                    {
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //               new System.TimeSpan(0, 30, 0)))
                        //{
                        foreach (var i in ids)
                        {
                            //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                            //                                                                new System.TimeSpan(0, 30, 0)))
                            //{
                            //foreach (var LvHead_id in LvHead_ids)
                            //{
                            // Int32 _returnParm = Delete_CreditRecord(i, lvcalendarid, LvHead_id);
                            // delete function comment because converted leave has to delete for new year
                            //if (_returnParm == 0)
                            //{
                            //    continue;
                            //}

                            var ErrNo = LvCreditProcessController.LvCreditProceess(i, CompLvId, null, LvHead_ids, lvcalendarid, 1, OCompanyPayroll.Id, FromDate, ToDate, CreditDatelist,true);
                            //}
                            //    ts.Complete();
                            //}
                        }
                        //  ts.Complete();

                        return Json(new Utility.JsonReturnClass { success = true, responseText = "Data Saved SuccessFully..!" }, JsonRequestBehavior.AllowGet);
                        //}
                    }
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

        //public static Int32 LvCreditProceess(Int32 oEmployeeId, Int32 CompLvId, CompanyLeave CompCreditPolicyLists, List<Int32> LvHead_ids_list, Int32 LvCalendarId, int trial, int compid, DateTime? FromDate, DateTime? ToDate, string CreditDatelist)
        //{
        //    try
        //    {
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                                                     new System.TimeSpan(0, 30, 0)))
        //        {
        //            using (DataBaseContext db = new DataBaseContext())
        //            {
        //                int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId).Select(e => e.Id).FirstOrDefault();
        //                //Employee Lv Data
        //                int crd = Convert.ToInt32(CreditDatelist);
        //                string creditdtlist = db.LookupValue.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();

        //                foreach (var LvHead_ids in LvHead_ids_list)
        //                {
        //                    var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
        //                        .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().AsParallel().ToList();
        //                    int lastyearid = 0;
        //                    double mSusDayFull = 0;


        //                    Company OCompany = null;
        //                    OCompany = db.Company.Find(compid);

        //                    foreach (var item in EmpLvHeadList)
        //                    {
        //                        //prev credit process check
        //                        //end
        //                        //Get LvCredit Policy For Particular Lv

        //                        LvNewReq OLvCreditRecord = new LvNewReq();
        //                        EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
        //                             .Include(e => e.EmployeeLvStructDetails)
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
        //                             .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
        //                                 .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();

        //                        LvCreditPolicy oLvCreditPolicy = null;
        //                        if (OLvSalStruct != null)
        //                        {
        //                            oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();

        //                        }

        //                        if (oLvCreditPolicy == null)
        //                        {
        //                            continue;
        //                        }
        //                        if (oLvCreditPolicy.CreditDate.LookupVal.ToUpper() != creditdtlist)
        //                        {
        //                            continue;
        //                        }

        //                        DateTime? Lastyear = null, CreditDate = null, tempCreditDate = null, Cal_Wise_Date = null, Lastyearj = null;
        //                        Double CreditDays = 0, SumDays = 0, oOpenBal = 0, oCloseingbal = 0;
        //                        Calendar LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

        //                        DateTime? tempRetireDate = null, tempRetirecrDate = null, leaveyearfrom = null, leaveyearto = null;

        //                        switch (oLvCreditPolicy.CreditDate.LookupVal.ToUpper())
        //                        {
        //                            case "CALENDAR":
        //                                Cal_Wise_Date = LvCalendar.FromDate;
        //                                CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearid1 = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyear
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearid1 != null)
        //                                {

        //                                    lastyearid = lastyearid1.Id;
        //                                }

        //                                break;
        //                            case "YEARLY":
        //                                Cal_Wise_Date = LvCalendar.FromDate;
        //                                CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearid11 = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyear
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearid11 != null)
        //                                {

        //                                    lastyearid = lastyearid11.Id;
        //                                }

        //                                break;
        //                            case "JOININGDATE":
        //                                //var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = JoiningDate.ServiceBookDates.JoiningDate;


        //                                var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aa = null;
        //                                DateTime? joinincr = null;
        //                                var Fulldate = "";
        //                                if (LvNewReqData != null)
        //                                {
        //                                    aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aa != null)
        //                                {
        //                                    Fulldate = aa.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.JoiningDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                    if (ServiceBookData != null)
        //                                    {
        //                                        if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
        //                                        {

        //                                            Fulldate = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                }
        //                                joinincr = Convert.ToDateTime(Fulldate);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidj = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidj != null)
        //                                {
        //                                    lastyearid = lastyearidj.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "CONFIRMATIONDATE":
        //                                //var ConfirmationDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = ConfirmationDate.ServiceBookDates.ConfirmationDate;

        //                                var LvNewReqDatac = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aac = null;
        //                                DateTime? joinincrc = null;
        //                                var Fulldatec = "";
        //                                if (LvNewReqDatac != null)
        //                                {
        //                                    aac = LvNewReqDatac.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aac != null)
        //                                {
        //                                    Fulldatec = aac.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                    if (ServiceBookData != null)
        //                                    {
        //                                        if (ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month <= ToDate.Value.Month)
        //                                        {

        //                                            Fulldatec = ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                }
        //                                joinincrc = Convert.ToDateTime(Fulldatec);
        //                                CreditDate = joinincrc.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincrc.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidc = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidc != null)
        //                                {
        //                                    lastyearid = lastyearidc.Id;
        //                                }
        //                                Cal_Wise_Date = joinincrc;



        //                                break;
        //                            case "INCREMENTDATE":
        //                                //var LastIncrementDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = LastIncrementDate.ServiceBookDates.LastIncrementDate;
        //                                DateTime? joinincrI = null;
        //                                var FulldateI = "";
        //                                var Emp_lvcr = db.EmployeePayroll
        //                                    .Include(a => a.Employee)
        //                                    .Include(e => e.IncrementServiceBook)
        //                                    .Include(e => e.IncrementServiceBook.Select(q => q.IncrActivity))
        //                                    .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
        //                                if (Emp_lvcr != null)
        //                                {
        //                                    var fgds = Emp_lvcr.IncrementServiceBook.Where(q => q.ReleaseDate != null
        //                                         && q.IncrActivity.Id == 1).OrderBy(q => q.Id).LastOrDefault();
        //                                    if (fgds != null)
        //                                    {
        //                                        if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
        //                                        {
        //                                            FulldateI = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastIncrementDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                        if (ServiceBookData != null)
        //                                        {
        //                                            if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
        //                                            {

        //                                                FulldateI = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                            }
        //                                        }
        //                                    }

        //                                }

        //                                joinincrI = Convert.ToDateTime(FulldateI);
        //                                CreditDate = joinincrI.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincrI.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidI = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidI != null)
        //                                {
        //                                    lastyearid = lastyearidI.Id;
        //                                }
        //                                Cal_Wise_Date = joinincrI;

        //                                break;
        //                            case "PROMOTIONDATE":
        //                                //var LastPromotionDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = LastPromotionDate.ServiceBookDates.LastPromotionDate;

        //                                DateTime? joinincrP = null;
        //                                var FulldateP = "";
        //                                var Emp_lvcrP = db.EmployeePayroll
        //                                    .Include(a => a.Employee)
        //                                    .Include(e => e.PromotionServiceBook)
        //                                    .Include(e => e.PromotionServiceBook.Select(q => q.PromotionActivity))
        //                                    .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
        //                                if (Emp_lvcrP != null)
        //                                {
        //                                    var fgds = Emp_lvcrP.PromotionServiceBook.Where(q => q.ReleaseDate != null
        //                                         && q.PromotionActivity.Id == 2).OrderBy(q => q.Id).LastOrDefault();
        //                                    if (fgds != null)
        //                                    {
        //                                        if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
        //                                        {
        //                                            FulldateP = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastPromotionDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                        if (ServiceBookData != null)
        //                                        {
        //                                            if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
        //                                            {

        //                                                FulldateP = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                            }
        //                                        }
        //                                    }

        //                                }

        //                                joinincrP = Convert.ToDateTime(FulldateP);
        //                                CreditDate = joinincrP.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincrP.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidP = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidP != null)
        //                                {
        //                                    lastyearid = lastyearidP.Id;
        //                                }
        //                                Cal_Wise_Date = joinincrP;

        //                                break;
        //                            case "FIXDAYS":
        //                                if (OCompany.Code.ToString() == "ACABL")
        //                                {
        //                                    DateTime? Fixdays = null;
        //                                    Fixdays = Convert.ToDateTime("01/01/" + DateTime.Now.Year);

        //                                    Cal_Wise_Date = Fixdays;
        //                                    CreditDate = Fixdays.Value.AddDays(-1);
        //                                    Lastyear = Convert.ToDateTime(Fixdays.Value.AddYears(-1));

        //                                }
        //                                break;
        //                            case "QUARTERLY":
        //                                var LvNewReqDataQUARTERLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaQUARTERLY = null;
        //                                var FulldateQUARTERLY = "";
        //                                if (LvNewReqDataQUARTERLY != null)
        //                                {
        //                                    aaQUARTERLY = LvNewReqDataQUARTERLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaQUARTERLY != null)
        //                                {
        //                                    FulldateQUARTERLY = aaQUARTERLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateQUARTERLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateQUARTERLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjQUARTERLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjQUARTERLY != null)
        //                                {
        //                                    lastyearid = lastyearidjQUARTERLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;

        //                                break;
        //                            case "HALFYEARLY":
        //                                var LvNewReqDataHALFYEARLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaHALFYEARLY = null;
        //                                var FulldateHALFYEARLY = "";
        //                                if (LvNewReqDataHALFYEARLY != null)
        //                                {
        //                                    aaHALFYEARLY = LvNewReqDataHALFYEARLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaHALFYEARLY != null)
        //                                {
        //                                    FulldateHALFYEARLY = aaHALFYEARLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateHALFYEARLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateHALFYEARLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjHALFYEARLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjHALFYEARLY != null)
        //                                {
        //                                    lastyearid = lastyearidjHALFYEARLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "MONTHLY":
        //                                var LvNewReqDataMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaMONTHLY = null;
        //                                var FulldateMONTHLY = "";
        //                                if (LvNewReqDataMONTHLY != null)
        //                                {
        //                                    aaMONTHLY = LvNewReqDataMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaMONTHLY != null)
        //                                {
        //                                    FulldateMONTHLY = aaMONTHLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateMONTHLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateMONTHLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjMONTHLY != null)
        //                                {
        //                                    lastyearid = lastyearidjMONTHLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "BIMONTHLY":
        //                                var LvNewReqDataBIMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaBIMONTHLY = null;
        //                                var FulldateBIMONTHLY = "";
        //                                if (LvNewReqDataBIMONTHLY != null)
        //                                {
        //                                    aaBIMONTHLY = LvNewReqDataBIMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaBIMONTHLY != null)
        //                                {
        //                                    FulldateBIMONTHLY = aaBIMONTHLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateBIMONTHLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateBIMONTHLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjBIMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjBIMONTHLY != null)
        //                                {
        //                                    lastyearid = lastyearidjBIMONTHLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;

        //                            case "OTHER":
        //                                //var EmpService = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = EmpService.ServiceBookDates.JoiningDate;
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                        if (CreditDate == null && Lastyear == null)
        //                        {
        //                            CreditDate = Convert.ToDateTime(tempCreditDate.Value.Day + "/" + tempCreditDate.Value.Month + "/" + DateTime.Now.Year);
        //                            leaveyearfrom = CreditDate;
        //                            leaveyearto = leaveyearfrom.Value.AddDays(-1);
        //                            CreditDate = CreditDate.Value.AddDays(-1);
        //                            Lastyear = CreditDate.Value.AddYears(-1);
        //                        }


        //                        leaveyearfrom = LvCalendar.FromDate;
        //                        leaveyearto = LvCalendar.ToDate;
        //                        double retmonth = 0;
        //                        var retiredate = db.Employee.Include(e => e.ServiceBookDates)
        //                                    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                        tempRetireDate = retiredate.ServiceBookDates.RetirementDate;
        //                        if (tempRetireDate != null)
        //                        {
        //                            if (leaveyearto > tempRetireDate)
        //                            {
        //                                tempRetirecrDate = tempRetireDate;
        //                            }
        //                            else
        //                            {
        //                                tempRetirecrDate = leaveyearto;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            tempRetirecrDate = leaveyearto;
        //                        }
        //                        if (tempRetirecrDate.Value.Day >= 15)
        //                        {
        //                            // retmonth=
        //                            int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
        //                            //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
        //                            //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
        //                            retmonth = compMonth + 1;

        //                        }
        //                        else
        //                        {
        //                            int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
        //                            //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
        //                            //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
        //                            retmonth = compMonth;
        //                        }
        //                        if (retmonth < 0)
        //                        {
        //                            retmonth = 0;
        //                        }
        //                        retmonth = Math.Round(retmonth, 0);

        //                        int compMonthLYR = (leaveyearto.Value.Month + leaveyearto.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
        //                        double daysInEndMonthLYR = (leaveyearto - leaveyearto.Value.AddMonths(1)).Value.Days;
        //                        double LYRmonth = compMonthLYR + (leaveyearfrom.Value.Day - leaveyearto.Value.Day) / daysInEndMonthLYR;
        //                        LYRmonth = Math.Round(LYRmonth, 0);

        //                        // DateTime NextCreditDays = Cal_Wise_Date.Value.AddYears(1); //comment line because credit frequeny avilable in system if yearly then 12 half yearly then 6 if monthyly then 1 frequency 
        //                        DateTime NextCreditDays = Cal_Wise_Date.Value.AddMonths(oLvCreditPolicy.ProCreditFrequency);

        //                        EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
        //                            .Include(a => a.LvNewReq)
        //                            .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
        //                            .Include(a => a.LvNewReq.Select(e => e.WFStatus))
        //                            .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
        //                            .Include(a => a.LvNewReq.Select(e => e.PayStruct))
        //                            .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
        //                            .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
        //                            .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
        //                                //  && a.LeaveCalendar.FromDate == Lastyear && a.LeaveCalendar.ToDate == CreditDate
        //                                ))
        //                                .SingleOrDefault();
        //                        List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
        //                        // Check if leave Credited for that year then should not credit start
        //                        if (_Prv_EmpLvData != null)
        //                        {
        //                            var LvCreditedInNewYrChk1 = _Prv_EmpLvData.LvNewReq
        //                                    .Where(a => a.LvCreditNextDate != null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
        //                                    && a.LeaveHead.Id == item.Id).FirstOrDefault();
        //                            if (LvCreditedInNewYrChk1 != null)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        // Check if leave Credited for that year then should not credit end

        //                        if (_Prv_EmpLvData != null)
        //                        {
        //                            LvNewReq LvCreditedInNewYrChk = _Prv_EmpLvData.LvNewReq
        //                                .Where(a => a.LeaveCalendar.FromDate == LvCalendar.FromDate && a.LvCreditNextDate == NextCreditDays.Date
        //                                && a.LeaveHead.Id == item.Id).SingleOrDefault();
        //                            if (LvCreditedInNewYrChk == null)
        //                            {
        //                                Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
        //                                    .Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id)
        //                                    //      && a.ReqDate >= Lastyear &&
        //                                    //a.ReqDate <= CreditDate)
        //                                  .ToList();
        //                            }
        //                            else
        //                            {
        //                                Filter_oEmpLvData.Add(LvCreditedInNewYrChk);
        //                            }
        //                        }
        //                        double oLvClosingData = 0;
        //                        double UtilizedLv = 0;
        //                        Int32 GeoStruct = 0;
        //                        Int32 PayStruct = 0;
        //                        Int32 FuncStruct = 0;
        //                        if (Filter_oEmpLvData.Count == 0)
        //                        {
        //                            //get Data from opening
        //                            EmployeeLeave _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == item.Id))
        //                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                .Include(e => e.Employee.GeoStruct)
        //                                .Include(e => e.Employee.PayStruct)
        //                                .Include(e => e.Employee.FuncStruct)
        //                                .SingleOrDefault();

        //                            double _EmpOpeningData = 0;
        //                            if (_Emp_EmpOpeningData != null)
        //                            {
        //                                _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == item.Id).Select(e => e.LvOpening).SingleOrDefault();

        //                            }
        //                            if (_EmpOpeningData == 0)
        //                            {
        //                                // continue;
        //                            }
        //                            oLvClosingData = _EmpOpeningData;
        //                            UtilizedLv = 0;
        //                            if (_Emp_EmpOpeningData != null)
        //                            {
        //                                GeoStruct = _Emp_EmpOpeningData.Employee.GeoStruct.Id;
        //                                PayStruct = _Emp_EmpOpeningData.Employee.PayStruct.Id;
        //                                FuncStruct = _Emp_EmpOpeningData.Employee.FuncStruct.Id;

        //                            }
        //                            else
        //                            {
        //                                EmployeeLeave _Emp = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId)
        //                              .Include(e => e.Employee.GeoStruct)
        //                              .Include(e => e.Employee.PayStruct)
        //                              .Include(e => e.Employee.FuncStruct)
        //                              .SingleOrDefault();

        //                                GeoStruct = _Emp.Employee.GeoStruct.Id;
        //                                PayStruct = _Emp.Employee.PayStruct.Id;
        //                                FuncStruct = _Emp.Employee.FuncStruct.Id;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

        //                            oLvClosingData = LastLvData.CloseBal;
        //                            UtilizedLv = LastLvData.LVCount;
        //                            GeoStruct = LastLvData.GeoStruct != null ? LastLvData.GeoStruct.Id : db.Employee.Find(oEmployeeId).GeoStruct.Id;
        //                            PayStruct = LastLvData.PayStruct != null ? LastLvData.PayStruct.Id : db.Employee.Find(oEmployeeId).PayStruct.Id;
        //                            FuncStruct = LastLvData.FuncStruct != null ? LastLvData.FuncStruct.Id : db.Employee.Find(oEmployeeId).FuncStruct.Id;
        //                        }

        //                        if (oLvCreditPolicy.ProdataFlag == true)
        //                        {

        //                            var AttendaceData = db.EmployeePayroll.Include(e => e.SalAttendance)
        //                                .Where(e => e.Employee.Id == oEmployeeId
        //                                //&&
        //                                //e.SalAttendance.Any(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
        //                                //Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate)
        //                                ).FirstOrDefault();

        //                            if (AttendaceData != null)
        //                            {
        //                                SumDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
        //                              Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.PaybleDays != 0).Select(a => a.PaybleDays).ToList().Sum();
        //                            }

        //                            ////suspended days check

        //                            EmployeePayroll othser = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
        //                                                    .Include(e => e.OtherServiceBook)
        //                                                    .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
        //                                                    .SingleOrDefault();

        //                            if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
        //                            {
        //                                List<OtherServiceBook> OthServBkSus = othser.OtherServiceBook.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED" || e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").OrderByDescending(e => e.ReleaseDate).ToList();
        //                                if (OthServBkSus.Count() > 0)
        //                                {
        //                                    var checkSuspenddays = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").Select(e => e.ReleaseDate).SingleOrDefault();
        //                                    var checkRejoindays_temp = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").Select(e => e.ReleaseDate).SingleOrDefault();
        //                                    var checkRejoindays = "";
        //                                    if (checkRejoindays_temp != null)
        //                                    {
        //                                        checkRejoindays = checkRejoindays_temp.ToString();
        //                                    }
        //                                    else
        //                                    {
        //                                        checkRejoindays = CreditDate.ToString();
        //                                    }
        //                                    if (checkSuspenddays != null && checkRejoindays != null)
        //                                    {
        //                                        if (Convert.ToDateTime(checkSuspenddays).Date < Lastyear)
        //                                        {
        //                                            checkSuspenddays = Lastyear;
        //                                        }
        //                                        if (Convert.ToDateTime(checkRejoindays).Date >= Lastyear && Convert.ToDateTime(checkRejoindays).Date <= CreditDate)
        //                                        {
        //                                            mSusDayFull = Math.Round((Convert.ToDateTime(checkRejoindays).Date - Convert.ToDateTime(checkSuspenddays).Date).TotalDays) + 1;
        //                                            SumDays = SumDays - mSusDayFull;
        //                                        }
        //                                        if (SumDays < 0)
        //                                        {
        //                                            SumDays = 0;
        //                                        }
        //                                    }
        //                                }
        //                            }


        //                            var OSalArrT = db.EmployeePayroll
        //                                .Include(e => e.SalaryArrearT)
        //                                .Include(e => e.SalaryArrearT.Select(q => q.ArrearType))
        //                                .Where(e => e.Employee.Id == oEmployeeId)
        //                             .FirstOrDefault();
        //                            if (OSalArrT != null)
        //                            {
        //                                double ArrDays = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
        //                                      && q.FromDate >= Lastyear
        //                                      && q.FromDate <= CreditDate && q.IsRecovery == false).Select(q => q.TotalDays).Sum();
        //                                SumDays = SumDays + ArrDays;


        //                                double ArrDaysrec = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
        //                                && q.FromDate >= Lastyear
        //                                && q.FromDate <= CreditDate && q.IsRecovery == true).Select(q => q.TotalDays).Sum();
        //                                SumDays = SumDays - ArrDaysrec;

        //                                if (SumDays < 0)
        //                                {
        //                                    SumDays = 0;
        //                                }
        //                            }

        //                        }

        //                        if (oLvCreditPolicy.ExcludeLeaves == true)
        //                        {
        //                            List<LvHead> GetExculdeLvHeads = oLvCreditPolicy.ExcludeLeaveHeads.ToList();
        //                            foreach (var GetExculdeLvHead in GetExculdeLvHeads)
        //                            {

        //                                var _Prv_EmpLvData_exclude = db.EmployeeLeave.AsNoTracking()
        //                                    .Where(a => a.Employee.Id == oEmployeeId)
        //                                    .Include(a => a.LvNewReq)
        //                                    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
        //                                    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
        //                                     .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
        //                                    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
        //                                    .SingleOrDefault();

        //                                var _Prv_EmpLvData_exclude1 = _Prv_EmpLvData_exclude.LvNewReq
        //                                   .Where(a => a.LeaveHead != null
        //                                       && a.LeaveHead.Id == GetExculdeLvHead.Id
        //                                       //&& a.LeaveCalendar.Id == lastyearid && a.IsCancel == false && a.WFStatus.LookupVal != "2"
        //                                        && a.IsCancel == false && a.WFStatus.LookupVal != "2"
        //                               ).ToList();

        //                                var LvOrignal_id = _Prv_EmpLvData_exclude.LvNewReq.Where(e => e.LvOrignal != null && e.WFStatus.LookupVal != "2").Select(e => e.LvOrignal.Id).ToList();
        //                                var listLvs = _Prv_EmpLvData_exclude1.Where(e => !LvOrignal_id.Contains(e.Id) && e.FromDate != null && e.ToDate != null).OrderBy(e => e.Id).ToList();
        //                                double DebitSum = 0;
        //                                if (listLvs != null)
        //                                {
        //                                    for (DateTime _Date = Lastyear.Value; _Date <= CreditDate; _Date = _Date.AddDays(1))
        //                                    {
        //                                        var xyz = listLvs.Where(q => _Date >= q.FromDate && _Date <= q.ToDate).FirstOrDefault();
        //                                        if (xyz != null)
        //                                        {
        //                                            DebitSum = DebitSum + 1;
        //                                        }
        //                                    }
        //                                    // double DebitSum = listLvs.Sum(e => e.DebitDays);
        //                                    SumDays = SumDays - DebitSum;
        //                                }
        //                                else
        //                                {
        //                                    EmployeeLeave _Emp_EmpOpeningData_exclude = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == GetExculdeLvHead.Id))
        //                                                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                                                        .Include(e => e.Employee.GeoStruct)
        //                                                                        .Include(e => e.Employee.PayStruct)
        //                                                                        .Include(e => e.Employee.FuncStruct)
        //                                                                        .SingleOrDefault();

        //                                    double _EmpOpeningData_exclude = 0;
        //                                    if (_Emp_EmpOpeningData_exclude != null)
        //                                    {
        //                                        _EmpOpeningData_exclude = _Emp_EmpOpeningData_exclude.LvOpenBal.Where(e => e.LvHead.Id == GetExculdeLvHead.Id).Select(e => e.LVCount).SingleOrDefault();

        //                                    }
        //                                    SumDays = SumDays - _EmpOpeningData_exclude;

        //                                }
        //                                //double CloseBalSum = Filter_oEmpLvData.Where(e => e.LeaveHead.Id == GetExculdeLvHead.Id).Select(e => e.CloseBal).ToList().Sum();
        //                                //SumDays = SumDays - CloseBalSum;
        //                            }
        //                            if (SumDays < 0)
        //                            {
        //                                SumDays = 0;
        //                            }
        //                        }

        //                        //if (SumDays == 0)
        //                        //{
        //                        //    return 0;
        //                        //}

        //                        List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
        //                        if (oLvCreditPolicy.LVConvert == true)
        //                        {
        //                            //if (oLvClosingData > 0)
        //                            //{
        //                            double LastMonthBal = 0, _LvLapsed = 0, Prv_bal = 0;
        //                            LvHead ConvertLeaveHead = oLvCreditPolicy.ConvertLeaveHead.LastOrDefault();
        //                            //Check Exitance
        //                            List<LvNewReq> Filter_oEmpLvDataCon = new List<LvNewReq>();
        //                            if (_Prv_EmpLvData != null)
        //                            {
        //                                LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
        //                                      .Where(a => a.LeaveHead != null
        //                                          && a.LeaveHead.Id == ConvertLeaveHead.Id
        //                                          && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();


        //                                if (DefaultyearLvHeadCreditedCHeck == null)
        //                                {
        //                                    Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
        //                                        .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHead.Id && a.ReqDate >= Lastyear &&
        //                                      a.ReqDate <= CreditDate).ToList();
        //                                }
        //                                else
        //                                {
        //                                    Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
        //                                }
        //                            }
        //                            var _LvNewReq_Prv_bal = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHead.Id)
        //                                .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
        //                            if (_LvNewReq_Prv_bal == 0)
        //                            {
        //                                EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
        //                                    .Include(e => e.LvOpenBal)
        //                                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                    .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHead.Id)).FirstOrDefault();
        //                                if (_Emp_LvOpenBal != null)
        //                                {
        //                                    Prv_bal = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHead.Id).Select(e => e.LvOpening).LastOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                Prv_bal = _LvNewReq_Prv_bal;
        //                            }


        //                            double conevrtedlv = 0;
        //                            double afterconvrtedremainlv = 0;
        //                            if (oLvClosingData > oLvCreditPolicy.LvConvertLimit)
        //                            {
        //                                conevrtedlv = oLvCreditPolicy.LvConvertLimit;
        //                                afterconvrtedremainlv = oLvClosingData - conevrtedlv;
        //                            }
        //                            else
        //                            {
        //                                conevrtedlv = oLvClosingData;
        //                            }

        //                            //  LastMonthBal = Prv_bal + oLvClosingData;
        //                            LastMonthBal = Prv_bal + conevrtedlv;
        //                            //-------------------------------------

        //                            LvNewReq newLvConvertobj = new LvNewReq
        //                            {
        //                                InputMethod = 0,
        //                                ReqDate = DateTime.Now,
        //                                CloseBal = LastMonthBal,
        //                                OpenBal = Prv_bal,
        //                                LvOccurances = 0,
        //                                IsLock = true,
        //                                LvLapsed = _LvLapsed,
        //                                //CreditDays = oLvClosingData,
        //                                CreditDays = conevrtedlv,
        //                                //ToDate = CreditDate,
        //                                //FromDate = CreditDate,
        //                                LvCreditDate = Cal_Wise_Date,
        //                                LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
        //                                GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
        //                                PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
        //                                FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
        //                                LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault(),
        //                                Narration = "Credit Process",
        //                                LvCreditNextDate = NextCreditDays,
        //                                WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
        //                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                            };
        //                            _List_oLvNewReq.Add(newLvConvertobj);

        //                            // after conversion remaining leave conevrt another leave
        //                            if (oLvCreditPolicy.LVConvertBal == true)
        //                            {
        //                                if (afterconvrtedremainlv > 0)
        //                                {
        //                                    // 20/01/2020 start
        //                                    double LastMonthBalrm = 0, _LvLapsedrm = 0, Prv_balrm = 0;
        //                                    LvHead ConvertLeaveHeadrm = oLvCreditPolicy.ConvertLeaveHeadBal.LastOrDefault();
        //                                    //Check Exitance
        //                                    List<LvNewReq> Filter_oEmpLvDataConrm = new List<LvNewReq>();
        //                                    if (_Prv_EmpLvData != null)
        //                                    {
        //                                        LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
        //                                              .Where(a => a.LeaveHead != null
        //                                                  && a.LeaveHead.Id == ConvertLeaveHeadrm.Id
        //                                                  && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();


        //                                        if (DefaultyearLvHeadCreditedCHeck == null)
        //                                        {
        //                                            Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
        //                                                .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHeadrm.Id && a.ReqDate >= Lastyear &&
        //                                              a.ReqDate <= CreditDate).ToList();
        //                                        }
        //                                        else
        //                                        {
        //                                            Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
        //                                        }
        //                                    }
        //                                    var _LvNewReq_Prv_balrm = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHeadrm.Id)
        //                                        .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
        //                                    if (_LvNewReq_Prv_balrm == 0)
        //                                    {
        //                                        EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
        //                                            .Include(e => e.LvOpenBal)
        //                                            .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                            .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHeadrm.Id)).FirstOrDefault();
        //                                        if (_Emp_LvOpenBal != null)
        //                                        {
        //                                            Prv_balrm = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHeadrm.Id).Select(e => e.LvOpening).LastOrDefault();
        //                                        }
        //                                        else
        //                                        {
        //                                            continue;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        Prv_balrm = _LvNewReq_Prv_balrm;
        //                                    }


        //                                    double conevrtedlvrm = 0;

        //                                    if (afterconvrtedremainlv > oLvCreditPolicy.LvConvertLimitBal)
        //                                    {
        //                                        conevrtedlvrm = oLvCreditPolicy.LvConvertLimitBal;

        //                                    }
        //                                    else
        //                                    {
        //                                        conevrtedlvrm = afterconvrtedremainlv;
        //                                    }

        //                                    //  LastMonthBal = Prv_bal + oLvClosingData;
        //                                    LastMonthBalrm = Prv_balrm + conevrtedlvrm;
        //                                    //-------------------------------------

        //                                    LvNewReq newLvConvertobjrm = new LvNewReq
        //                                    {
        //                                        InputMethod = 0,
        //                                        ReqDate = DateTime.Now,
        //                                        CloseBal = LastMonthBalrm,
        //                                        OpenBal = Prv_balrm,
        //                                        LvOccurances = 0,
        //                                        IsLock = true,
        //                                        LvLapsed = _LvLapsed,
        //                                        //CreditDays = oLvClosingData,
        //                                        CreditDays = conevrtedlvrm,
        //                                        //ToDate = CreditDate,
        //                                        //FromDate = CreditDate,
        //                                        LvCreditDate = Cal_Wise_Date,
        //                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
        //                                        GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
        //                                        PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
        //                                        FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
        //                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHeadrm.Id).SingleOrDefault(),
        //                                        Narration = "Credit Process",
        //                                        LvCreditNextDate = NextCreditDays,
        //                                        WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
        //                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                                    };
        //                                    _List_oLvNewReq.Add(newLvConvertobjrm);


        //                                    // 20/01/2020 end

        //                                }
        //                            }

        //                            //}
        //                        }

        //                        if (oLvCreditPolicy.FixedCreditDays == true)
        //                        {
        //                            //CreditDays += oLvCreditPolicy.CreditDays;
        //                            CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonth, 0);
        //                        }
        //                        else
        //                        {
        //                            if (OCompany.Code.ToString() == "KDCC")
        //                            {
        //                                double WorkingDays1 = oLvCreditPolicy.WorkingDays;
        //                                DateTime CreditDate1 = LvCalendar.FromDate.Value.AddDays(-1);
        //                                DateTime Lastyear1 = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                int totactday = (CreditDate1 - Lastyear1).Days + 1;
        //                                double TotcreditDays = Convert.ToDouble(totactday / 11.40);
        //                                TotcreditDays = Convert.ToInt32(TotcreditDays);
        //                                double totleaveLwp = (totactday - SumDays);
        //                                Double LWPLeave = Convert.ToDouble(totleaveLwp / WorkingDays1);

        //                                int LWPLeave1 = (int)LWPLeave;
        //                                if (LWPLeave > LWPLeave1)
        //                                {
        //                                    LWPLeave1 = LWPLeave1 + 1;
        //                                }

        //                                CreditDays += TotcreditDays - LWPLeave1;
        //                            }
        //                            else
        //                            {
        //                                double WorkingDays = oLvCreditPolicy.WorkingDays;
        //                                double MinMiseWorkingDays = Convert.ToDouble(SumDays / WorkingDays);
        //                                CreditDays += Convert.ToInt32(MinMiseWorkingDays);

        //                            }
        //                        }
        //                        if (CreditDays < 0)
        //                        {
        //                            CreditDays = 0;
        //                        }
        //                        double oLvOccurances = 0;
        //                        if (oLvCreditPolicy.ServiceLink == true)
        //                        {
        //                            var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                   .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                            DateTime tempCreditDate1 = Convert.ToDateTime(JoiningDate.ServiceBookDates.JoiningDate);
        //                            DateTime till = CreditDate.Value.AddDays(1);
        //                            int compMonths = (till.Month + till.Year * 12) - (tempCreditDate1.Month + tempCreditDate1.Year * 12);
        //                            double daysInEndMonths = (till - till.AddMonths(1)).Days;
        //                            double monthss = compMonths + (tempCreditDate1.Day - till.Day) / daysInEndMonths;
        //                            var GetServiceYear = Math.Round(monthss / 12, 0);

        //                            // var GetServiceYear = Convert.ToDateTime(DateTime.Now - tempCreditDate).Year;
        //                            if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit && oLvCreditPolicy.AboveServiceMaxYears == false)
        //                            {
        //                                CreditDays = 0;
        //                            }
        //                            if (oLvCreditPolicy.AboveServiceMaxYears == true)
        //                            {
        //                                if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit)
        //                                {
        //                                    //double FinalServiceyear = GetServiceYear + oLvCreditPolicy.AboveServiceSteps;
        //                                    //if (GetServiceYear >= FinalServiceyear && GetServiceYear <= FinalServiceyear)
        //                                    //{
        //                                    //    CreditDays = 0;
        //                                    //}
        //                                    bool Chkcrdyr = false;
        //                                    for (double i = oLvCreditPolicy.ServiceYearsLimit; i <= GetServiceYear; )
        //                                    {
        //                                        var updatenew = i + oLvCreditPolicy.AboveServiceSteps;
        //                                        i = updatenew;
        //                                        if (updatenew == GetServiceYear)
        //                                        {
        //                                            //will credit
        //                                            Chkcrdyr = true;
        //                                            break;
        //                                        }
        //                                    }
        //                                    if (Chkcrdyr == false)
        //                                    {
        //                                        CreditDays = 0;
        //                                    }
        //                                }
        //                            }

        //                            if (oLvClosingData > oLvCreditPolicy.MaxLeaveDebitInService)
        //                            {
        //                                CreditDays = 0;
        //                            }

        //                        }
        //                        if (oLvCreditPolicy.OccInServAppl == true)
        //                        {
        //                            if (item.LvCode.ToUpper() == "ML" || item.LvCode.ToUpper() == "PTL")
        //                            {
        //                                if (UtilizedLv >= oLvCreditPolicy.OccInServ)
        //                                {
        //                                    CreditDays = 0;
        //                                }
        //                                else if (oLvCreditPolicy.OccCarryForward == true)
        //                                {
        //                                    oLvOccurances = UtilizedLv;
        //                                }
        //                            }
        //                        }

        //                        //lvbank start
        //                        //var _LvBankPolicy = db.LvBankPolicy.Include(e => e.LvHeadCollection)
        //                        //    .Where(q => q.LvHeadCollection.Any(r => r.Id == item.Id)).SingleOrDefault();

        //                        LvBankPolicy _LvBankPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvBankPolicy).FirstOrDefault();

        //                        if (_LvBankPolicy != null)
        //                        {

        //                            LvBankOpenBal OLvCreditRecordLvBankOpenBal = new LvBankOpenBal();
        //                            var _LvBankOpenBalprv = db.LvBankOpenBal
        //                               .Where(a => a.CreditDate >= Lastyear &&
        //                                          a.CreditDate <= CreditDate
        //                              ).SingleOrDefault();
        //                            if (_LvBankOpenBalprv != null)
        //                            {
        //                                var _LvBankOpenBal = db.LvBankOpenBal
        //                                .Where(e => e.LvCalendar.Id == LvCalendarId
        //                               ).SingleOrDefault();

        //                                if (_LvBankOpenBal == null)
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
        //                                    {
        //                                        OpeningBalance = _LvBankOpenBalprv.ClosingBalance,
        //                                        CreditDays = _LvBankPolicy.LvDebitInCredit,

        //                                        UtilizedDays = 0,
        //                                        CreditDate = DateTime.Now,
        //                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                                        LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
        //                                    };

        //                                    db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
        //                                    db.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.UtilizedDays = 0;
        //                                    _LvBankOpenBal.CreditDate = DateTime.Now;
        //                                    _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
        //                                    _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

        //                                    db.LvBankOpenBal.Attach(_LvBankOpenBal);
        //                                    db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();
        //                                }

        //                            }
        //                            else
        //                            {
        //                                var _LvBankOpenBal = db.LvBankOpenBal
        //                               .Where(e => e.LvCalendar.Id == LvCalendarId
        //                              ).SingleOrDefault();

        //                                if (_LvBankOpenBal == null)
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
        //                                    {
        //                                        OpeningBalance = 0,
        //                                        CreditDays = _LvBankPolicy.LvDebitInCredit,
        //                                        UtilizedDays = 0,
        //                                        CreditDate = DateTime.Now,
        //                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                                        LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
        //                                    };

        //                                    db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
        //                                    db.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.UtilizedDays = 0;
        //                                    _LvBankOpenBal.CreditDate = DateTime.Now;
        //                                    _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
        //                                    _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

        //                                    db.LvBankOpenBal.Attach(_LvBankOpenBal);
        //                                    db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();

        //                                }
        //                            }


        //                        }
        //                        //lvbank end
        //                        double newBal = 0, LvLapsed = 0;
        //                        if (oLvCreditPolicy.Accumalation == true)
        //                        {
        //                            double tempcreditdays = CreditDays;
        //                            CreditDays += oLvClosingData;

        //                            if (CreditDays > oLvCreditPolicy.AccumalationLimit)
        //                            {
        //                                LvLapsed = CreditDays - oLvCreditPolicy.AccumalationLimit;
        //                                CreditDays = oLvCreditPolicy.AccumalationLimit;
        //                            }
        //                            if (oLvCreditPolicy.AccumulationWithCredit == true)
        //                            {
        //                                if (CreditDays >= oLvCreditPolicy.AccumalationLimit)
        //                                {
        //                                    double diff = oLvCreditPolicy.AccumalationLimit - oLvClosingData;
        //                                    tempcreditdays = diff;
        //                                    //newBal = oLvClosingData - diff;
        //                                    //LvLapsed = newBal;
        //                                    //CreditDays += newBal;
        //                                    //if (CreditDays > oLvCreditPolicy.AccumalationLimit)
        //                                    //{
        //                                    //    CreditDays = oLvCreditPolicy.AccumalationLimit;
        //                                    //}
        //                                }
        //                            }
        //                            OLvCreditRecord.CreditDays = tempcreditdays;
        //                            OLvCreditRecord.OpenBal = oLvClosingData;
        //                            CreditDays -= OLvCreditRecord.DebitDays;
        //                        }
        //                        else
        //                        {
        //                            // OLvCreditRecord.OpenBal = CreditDays;
        //                            OLvCreditRecord.CreditDays = CreditDays;
        //                            CreditDays -= OLvCreditRecord.DebitDays;
        //                        }


        //                        if (CreditDays != 0)
        //                        {
        //                            //  var NextCreditDays = CreditDate.Value.AddYears(1);
        //                            if (OLvCreditRecord.CreditDays == 0)
        //                            {
        //                                // OLvCreditRecord.CreditDays = CreditDays;
        //                            }
        //                            if (OLvCreditRecord.OpenBal == 0)
        //                            {
        //                                //OLvCreditRecord.OpenBal = CreditDays;
        //                            }
        //                            OLvCreditRecord.LvCreditDate = Cal_Wise_Date;
        //                            OLvCreditRecord.InputMethod = 0;
        //                            OLvCreditRecord.IsLock = true;
        //                            OLvCreditRecord.ReqDate = DateTime.Now;
        //                            OLvCreditRecord.CloseBal = CreditDays;
        //                            OLvCreditRecord.LVCount = oLvOccurances;
        //                            OLvCreditRecord.LvLapsed = LvLapsed;
        //                            //OLvCreditRecord.ToDate = CreditDate;
        //                            //OLvCreditRecord.FromDate = CreditDate;
        //                            OLvCreditRecord.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                            OLvCreditRecord.GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault();
        //                            OLvCreditRecord.PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault();
        //                            OLvCreditRecord.FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault();
        //                            OLvCreditRecord.LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault();
        //                            OLvCreditRecord.Narration = "Credit Process";
        //                            OLvCreditRecord.LvCreditNextDate = NextCreditDays;
        //                            OLvCreditRecord.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
        //                            OLvCreditRecord.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
        //                            _List_oLvNewReq.Add(OLvCreditRecord);

        //                            if (_List_oLvNewReq.Count > 0)
        //                            {
        //                                if (_List_oLvNewReq.Count >= 2)
        //                                {
        //                                    _List_oLvNewReq[0].LvOrignal = OLvCreditRecord;
        //                                    if (_List_oLvNewReq.Count == 3)
        //                                    {
        //                                        _List_oLvNewReq[1].LvOrignal = OLvCreditRecord;
        //                                    }
        //                                }
        //                                var _Emp = db.EmployeeLeave.Include(e => e.LvNewReq)
        //                                    .Where(e => e.Employee.Id == oEmployeeId).SingleOrDefault();
        //                                for (int i = 0; i < _List_oLvNewReq.Count; i++)
        //                                {
        //                                    _Emp.LvNewReq.Add(_List_oLvNewReq[i]);
        //                                }
        //                                db.Entry(_Emp).State = System.Data.Entity.EntityState.Modified;
        //                                db.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                }
        //                if (trial == 1)
        //                {
        //                    ts.Complete();
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {

        //        throw e;
        //    }

        //    return 0;
        //}
        public class LvNewReqForReport
        {
            public string LeaveHead { get; set; }
            public string OpenBal { get; set; }
            public string CreditDays { get; set; }
            public string CloseBal { get; set; }
            public string LastLeave { get; set; }
            public string LvLapsed { get; set; }
            public string LvCount { get; set; }
            public string DebitDays { get; set; }

        }

        public ActionResult GetTrialData(string val, String forwardata)
        {
            var Employeelist = new List<string>();
            var Emplist = new List<string>();
            var Lvheadlist = new List<string>();
            var Empid = new List<string>();
            var Leaveid = (String)null;
            var Lvheadid = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var NewObj = new NameValueCollection();



                string Emp = forwardata == "0" ? "" : forwardata;


                if (Emp != null)
                {
                    Emplist = Utility.StringIdsToListString(Emp);
                    foreach (var item in Emplist)
                    {
                        Employeelist.Add(Convert.ToString(item));
                    }
                }

                //////////////////

                NewObj = HttpUtility.ParseQueryString(val);
                if (NewObj != null)
                {
                    Leaveid = NewObj["LvHead"];
                    if (Leaveid != null)
                    {
                        Lvheadid = Utility.StringIdsToListString(Leaveid);
                        foreach (var item in Lvheadid)
                        {
                            Lvheadlist.Add(Convert.ToString(item));
                        }
                    }

                }

                return Json(new Object[] { Employeelist, Lvheadlist, JsonRequestBehavior.AllowGet });
            }
        }

        //public static List<LvNewReqForReport> LvCreditProceessForReport(Int32 oEmployeeId, Int32 CompLvId, CompanyLeave CompCreditPolicyLists, List<Int32> LvHead_ids_list, Int32 LvCalendarId, int compid, DateTime? FromDate, DateTime? ToDate, List<string> creditdatelist)
        //{
        //    try
        //    {


        //        List<LvNewReqForReport> _List_oLvNewReqRpt = new List<LvNewReqForReport>();
        //        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                                                     new System.TimeSpan(0, 30, 0)))
        //        {
        //            using (DataBaseContext db = new DataBaseContext())
        //            {

        //                //Employee Lv Data
        //                foreach (var LvHead_ids in LvHead_ids_list)
        //                {
        //                    double mSusDayFull = 0;
        //                    int lastyearid = 0;
        //                    var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
        //                        .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().ToList();
        //                    Company OCompany = null;
        //                    OCompany = db.Company.Find(compid);
        //                    int crd = Convert.ToInt32(creditdatelist.FirstOrDefault());
        //                    string creditdtlist = db.LookupValue.Where(e => e.Id == crd).FirstOrDefault().LookupVal.ToUpper();


        //                    foreach (var item in EmpLvHeadList)
        //                    {
        //                        //prev credit process check
        //                        //end
        //                        //Get LvCredit Policy For Particular Lv
        //                        LvNewReq OLvCreditRecord = new LvNewReq();
        //                        LvNewReqForReport OLvCreditRecordForRpt = new LvNewReqForReport();
        //                        int OEmpLvID = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId).FirstOrDefault().Id;

        //                        EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
        //                              .Include(e => e.EmployeeLvStructDetails)
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.CreditDate))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHead))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.LvHead))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ConvertLeaveHeadBal))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy.ExcludeLeaveHeads))
        //                              .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
        //                                  .Where(e => e.EndDate == null && e.EmployeeLeave_Id == OEmpLvID).SingleOrDefault();

        //                        LvCreditPolicy oLvCreditPolicy = null;
        //                        if (OLvSalStruct != null)
        //                        {
        //                            oLvCreditPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id && e.LvHeadFormula != null && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();

        //                        }

        //                        //LvCreditPolicy oLvCreditPolicy = CompCreditPolicyLists.LvCreditPolicy.Where(e => e.CreditDate != null &&
        //                        //    e.LvHead != null && e.LvHead.Id == item.Id).SingleOrDefault();
        //                        if (oLvCreditPolicy == null)
        //                        {
        //                            continue;
        //                        }
        //                        if (oLvCreditPolicy.CreditDate.LookupVal.ToUpper() != creditdtlist)
        //                        {
        //                            continue;
        //                        }
        //                        DateTime? Lastyear = null, CreditDate = null, tempCreditDate = null, Cal_Wise_Date = null, Lastyearj = null;
        //                        Double CreditDays = 0, SumDays = 0, oOpenBal = 0, oCloseingbal = 0;
        //                        Calendar LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

        //                        DateTime? tempRetireDate = null, tempRetirecrDate = null, leaveyearfrom = null, leaveyearto = null;

        //                        switch (oLvCreditPolicy.CreditDate.LookupVal.ToUpper())
        //                        {
        //                            case "CALENDAR":
        //                                Cal_Wise_Date = LvCalendar.FromDate;
        //                                CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearid1 = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyear
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearid1 != null)
        //                                {

        //                                    lastyearid = lastyearid1.Id;
        //                                }

        //                                break;
        //                            case "YEARLY":
        //                                Cal_Wise_Date = LvCalendar.FromDate;
        //                                CreditDate = LvCalendar.FromDate.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearid11 = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyear
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearid11 != null)
        //                                {

        //                                    lastyearid = lastyearid11.Id;
        //                                }

        //                                break;
        //                            case "JOININGDATE":
        //                                //var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = JoiningDate.ServiceBookDates.JoiningDate;


        //                                var LvNewReqData = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aa = null;
        //                                DateTime? joinincr = null;
        //                                var Fulldate = "";
        //                                if (LvNewReqData != null)
        //                                {
        //                                    aa = LvNewReqData.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aa != null)
        //                                {
        //                                    Fulldate = aa.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.JoiningDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                    if (ServiceBookData != null)
        //                                    {
        //                                        if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
        //                                        {

        //                                            Fulldate = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                }
        //                                joinincr = Convert.ToDateTime(Fulldate);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidj = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidj != null)
        //                                {
        //                                    lastyearid = lastyearidj.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "CONFIRMATIONDATE":
        //                                //var ConfirmationDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = ConfirmationDate.ServiceBookDates.ConfirmationDate;

        //                                var LvNewReqDatac = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aac = null;
        //                                DateTime? joinincrc = null;
        //                                var Fulldatec = "";
        //                                if (LvNewReqDatac != null)
        //                                {
        //                                    aac = LvNewReqDatac.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aac != null)
        //                                {
        //                                    Fulldatec = aac.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.ConfirmationDate != null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                    if (ServiceBookData != null)
        //                                    {
        //                                        if (ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.Month <= ToDate.Value.Month)
        //                                        {

        //                                            Fulldatec = ServiceBookData.Employee.ServiceBookDates.ConfirmationDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                }
        //                                joinincrc = Convert.ToDateTime(Fulldatec);
        //                                CreditDate = joinincrc.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincrc.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidc = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidc != null)
        //                                {
        //                                    lastyearid = lastyearidc.Id;
        //                                }
        //                                Cal_Wise_Date = joinincrc;



        //                                break;
        //                            case "INCREMENTDATE":
        //                                //var LastIncrementDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = LastIncrementDate.ServiceBookDates.LastIncrementDate;
        //                                DateTime? joinincrI = null;
        //                                var FulldateI = "";
        //                                var Emp_lvcr = db.EmployeePayroll
        //                                    .Include(a => a.Employee)
        //                                    .Include(e => e.IncrementServiceBook)
        //                                    .Include(e => e.IncrementServiceBook.Select(q => q.IncrActivity))
        //                                    .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
        //                                if (Emp_lvcr != null)
        //                                {
        //                                    var fgds = Emp_lvcr.IncrementServiceBook.Where(q => q.ReleaseDate != null
        //                                         && q.IncrActivity.Id == 1).OrderBy(q => q.Id).LastOrDefault();
        //                                    if (fgds != null)
        //                                    {
        //                                        if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
        //                                        {
        //                                            FulldateI = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastIncrementDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                        if (ServiceBookData != null)
        //                                        {
        //                                            if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
        //                                            {

        //                                                FulldateI = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                            }
        //                                        }
        //                                    }

        //                                }

        //                                joinincrI = Convert.ToDateTime(FulldateI);
        //                                CreditDate = joinincrI.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincrI.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidI = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidI != null)
        //                                {
        //                                    lastyearid = lastyearidI.Id;
        //                                }
        //                                Cal_Wise_Date = joinincrI;

        //                                break;
        //                            case "PROMOTIONDATE":
        //                                //var LastPromotionDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = LastPromotionDate.ServiceBookDates.LastPromotionDate;

        //                                DateTime? joinincrP = null;
        //                                var FulldateP = "";
        //                                var Emp_lvcrP = db.EmployeePayroll
        //                                    .Include(a => a.Employee)
        //                                    .Include(e => e.PromotionServiceBook)
        //                                    .Include(e => e.PromotionServiceBook.Select(q => q.PromotionActivity))
        //                                    .Where(q => q.Employee.Id == oEmployeeId).SingleOrDefault();
        //                                if (Emp_lvcrP != null)
        //                                {
        //                                    var fgds = Emp_lvcrP.PromotionServiceBook.Where(q => q.ReleaseDate != null
        //                                         && q.PromotionActivity.Id == 2).OrderBy(q => q.Id).LastOrDefault();
        //                                    if (fgds != null)
        //                                    {
        //                                        if (fgds.ReleaseDate.Value.Month == FromDate.Value.Month)
        //                                        {
        //                                            FulldateP = fgds.ReleaseDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        var ServiceBookData = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(q => q.Employee.ServiceBookDates.LastPromotionDate == null && q.Employee.Id == oEmployeeId).SingleOrDefault();

        //                                        if (ServiceBookData != null)
        //                                        {
        //                                            if (ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month >= FromDate.Value.Month && ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.Month <= ToDate.Value.Month)
        //                                            {

        //                                                FulldateP = ServiceBookData.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;
        //                                            }
        //                                        }
        //                                    }

        //                                }

        //                                joinincrP = Convert.ToDateTime(FulldateP);
        //                                CreditDate = joinincrP.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincrP.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidP = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidP != null)
        //                                {
        //                                    lastyearid = lastyearidP.Id;
        //                                }
        //                                Cal_Wise_Date = joinincrP;

        //                                break;
        //                            case "FIXDAYS":
        //                                if (OCompany.Code.ToString() == "ACABL")
        //                                {
        //                                    DateTime? Fixdays = null;
        //                                    Fixdays = Convert.ToDateTime("01/01/" + DateTime.Now.Year);

        //                                    Cal_Wise_Date = Fixdays;
        //                                    CreditDate = Fixdays.Value.AddDays(-1);
        //                                    Lastyear = Convert.ToDateTime(Fixdays.Value.AddYears(-1));

        //                                }
        //                                break;
        //                            case "QUARTERLY":
        //                                var LvNewReqDataQUARTERLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaQUARTERLY = null;
        //                                var FulldateQUARTERLY = "";
        //                                if (LvNewReqDataQUARTERLY != null)
        //                                {
        //                                    aaQUARTERLY = LvNewReqDataQUARTERLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaQUARTERLY != null)
        //                                {
        //                                    FulldateQUARTERLY = aaQUARTERLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateQUARTERLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateQUARTERLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjQUARTERLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjQUARTERLY != null)
        //                                {
        //                                    lastyearid = lastyearidjQUARTERLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;

        //                                break;
        //                            case "HALFYEARLY":
        //                                var LvNewReqDataHALFYEARLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaHALFYEARLY = null;
        //                                var FulldateHALFYEARLY = "";
        //                                if (LvNewReqDataHALFYEARLY != null)
        //                                {
        //                                    aaHALFYEARLY = LvNewReqDataHALFYEARLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaHALFYEARLY != null)
        //                                {
        //                                    FulldateHALFYEARLY = aaHALFYEARLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateHALFYEARLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateHALFYEARLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjHALFYEARLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjHALFYEARLY != null)
        //                                {
        //                                    lastyearid = lastyearidjHALFYEARLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "MONTHLY":
        //                                var LvNewReqDataMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaMONTHLY = null;
        //                                var FulldateMONTHLY = "";
        //                                if (LvNewReqDataMONTHLY != null)
        //                                {
        //                                    aaMONTHLY = LvNewReqDataMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaMONTHLY != null)
        //                                {
        //                                    FulldateMONTHLY = aaMONTHLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateMONTHLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateMONTHLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjMONTHLY != null)
        //                                {
        //                                    lastyearid = lastyearidjMONTHLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "BIMONTHLY":
        //                                var LvNewReqDataBIMONTHLY = db.EmployeeLeave.Include(q => q.Employee).Include(q => q.LvNewReq).Include(q => q.LvNewReq.Select(t => t.LeaveHead)).Where(q => q.Employee.Id == oEmployeeId).FirstOrDefault();
        //                                LvNewReq aaBIMONTHLY = null;
        //                                var FulldateBIMONTHLY = "";
        //                                if (LvNewReqDataBIMONTHLY != null)
        //                                {
        //                                    aaBIMONTHLY = LvNewReqDataBIMONTHLY.LvNewReq.Where(q => q.LvCreditNextDate != null && q.LvCreditNextDate >= FromDate && q.LvCreditNextDate <= ToDate && q.LeaveHead.Id == item.Id).OrderByDescending(q => q.Id).FirstOrDefault();
        //                                }

        //                                if (aaBIMONTHLY != null)
        //                                {
        //                                    FulldateBIMONTHLY = aaBIMONTHLY.LvCreditNextDate.Value.ToShortDateString();
        //                                }
        //                                else
        //                                {
        //                                    FulldateBIMONTHLY = OLvSalStruct.EffectiveDate.Value.ToString("dd/MM") + "/" + FromDate.Value.Year;

        //                                }
        //                                joinincr = Convert.ToDateTime(FulldateBIMONTHLY);
        //                                CreditDate = joinincr.Value.AddDays(-1);
        //                                Lastyear = Convert.ToDateTime(joinincr.Value.AddYears(-1));
        //                                Lastyearj = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                                Calendar lastyearidjBIMONTHLY = db.Calendar.Include(e => e.Name).Where(e =>
        //                                    e.FromDate == Lastyearj
        //                                    && e.Default == false
        //                                    && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                                if (lastyearidjBIMONTHLY != null)
        //                                {
        //                                    lastyearid = lastyearidjBIMONTHLY.Id;
        //                                }
        //                                Cal_Wise_Date = joinincr;
        //                                break;
        //                            case "OTHER":
        //                                //var EmpService = db.Employee.Include(e => e.ServiceBookDates)
        //                                //    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                                //tempCreditDate = EmpService.ServiceBookDates.JoiningDate;
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                        if (CreditDate == null && Lastyear == null)
        //                        {
        //                            CreditDate = Convert.ToDateTime(tempCreditDate.Value.Day + "/" + tempCreditDate.Value.Month + "/" + DateTime.Now.Year);
        //                            leaveyearfrom = CreditDate;
        //                            leaveyearto = leaveyearfrom.Value.AddDays(-1);
        //                            CreditDate = CreditDate.Value.AddDays(-1);
        //                            Lastyear = CreditDate.Value.AddYears(-1);
        //                        }


        //                        leaveyearfrom = LvCalendar.FromDate;
        //                        leaveyearto = LvCalendar.ToDate;
        //                        double retmonth = 0;
        //                        var retiredate = db.Employee.Include(e => e.ServiceBookDates)
        //                                    .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                        tempRetireDate = retiredate.ServiceBookDates.RetirementDate;
        //                        if (tempRetireDate != null)
        //                        {
        //                            if (leaveyearto > tempRetireDate)
        //                            {
        //                                tempRetirecrDate = tempRetireDate;
        //                            }
        //                            else
        //                            {
        //                                tempRetirecrDate = leaveyearto;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            tempRetirecrDate = leaveyearto;
        //                        }
        //                        if (tempRetirecrDate.Value.Day >= 15)
        //                        {
        //                            // retmonth=
        //                            int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
        //                            //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
        //                            //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
        //                            retmonth = compMonth + 1;

        //                        }
        //                        else
        //                        {
        //                            int compMonth = (tempRetirecrDate.Value.Month + tempRetirecrDate.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
        //                            //double daysInEndMonth = (tempRetirecrDate - tempRetirecrDate.Value.AddMonths(1)).Value.Days;
        //                            //retmonth = compMonth + (leaveyearfrom.Value.Day - tempRetirecrDate.Value.Day) / daysInEndMonth;
        //                            retmonth = compMonth;
        //                        }
        //                        if (retmonth < 0)
        //                        {
        //                            retmonth = 0;
        //                        }
        //                        retmonth = Math.Round(retmonth, 0);

        //                        int compMonthLYR = (leaveyearto.Value.Month + leaveyearto.Value.Year * 12) - (leaveyearfrom.Value.Month + leaveyearfrom.Value.Year * 12);
        //                        double daysInEndMonthLYR = (leaveyearto - leaveyearto.Value.AddMonths(1)).Value.Days;
        //                        double LYRmonth = compMonthLYR + (leaveyearfrom.Value.Day - leaveyearto.Value.Day) / daysInEndMonthLYR;
        //                        LYRmonth = Math.Round(LYRmonth, 0);

        //                        //   DateTime NextCreditDays = Cal_Wise_Date.Value.AddYears(1); //comment line because credit frequeny avilable in system if yearly then 12 half yearly then 6 if monthyly then 1 frequency define
        //                        DateTime NextCreditDays = Cal_Wise_Date.Value.AddMonths(oLvCreditPolicy.ProCreditFrequency);

        //                        EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == oEmployeeId)
        //                            .Include(a => a.LvNewReq)
        //                            .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
        //                            .Include(a => a.LvNewReq.Select(e => e.WFStatus))
        //                            .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
        //                            .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id
        //                                //  && a.LeaveCalendar.FromDate == Lastyear && a.LeaveCalendar.ToDate == CreditDate
        //                                ))
        //                                .SingleOrDefault();
        //                        List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
        //                        // Check if leave Credited for that year then should not credit start
        //                        if (_Prv_EmpLvData != null)
        //                        {
        //                            var LvCreditedInNewYrChk1 = _Prv_EmpLvData.LvNewReq
        //                                    .Where(a => a.LvCreditNextDate != null && a.LvCreditNextDate.Value.Date == NextCreditDays.Date
        //                                    && a.LeaveHead.Id == item.Id).FirstOrDefault();
        //                            if (LvCreditedInNewYrChk1 != null)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        // Check if leave Credited for that year then should not credit end
        //                        if (_Prv_EmpLvData != null)
        //                        {
        //                            var LvCreditedInNewYrChk = _Prv_EmpLvData.LvNewReq
        //                                .Where(a => a.LeaveCalendar.FromDate == LvCalendar.FromDate
        //                                && a.LeaveHead.Id == item.Id).ToList();
        //                            if (LvCreditedInNewYrChk.Count() == 0)
        //                            {
        //                                Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
        //                                    .Where(a => a.LeaveHead != null && a.LeaveHead.Id == item.Id)
        //                                    //      && a.ReqDate >= Lastyear &&
        //                                    //a.ReqDate <= CreditDate)
        //                                  .ToList();
        //                            }
        //                            else
        //                            {
        //                                Filter_oEmpLvData.AddRange(LvCreditedInNewYrChk);
        //                            }
        //                        }
        //                        double oLvClosingData = 0;
        //                        double UtilizedLv = 0;
        //                        if (Filter_oEmpLvData.Count == 0)
        //                        {
        //                            //get Data from opening
        //                            EmployeeLeave _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == item.Id))
        //                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                .Include(e => e.Employee.GeoStruct)
        //                                .Include(e => e.Employee.PayStruct)
        //                                .Include(e => e.Employee.FuncStruct)
        //                                .SingleOrDefault();

        //                            double _EmpOpeningData = 0;
        //                            if (_Emp_EmpOpeningData != null)
        //                            {
        //                                _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == item.Id).Select(e => e.LvOpening).SingleOrDefault();

        //                            }
        //                            if (_EmpOpeningData == 0)
        //                            {
        //                                // continue;
        //                            }
        //                            oLvClosingData = _EmpOpeningData;
        //                            UtilizedLv = 0;
        //                        }
        //                        else
        //                        {
        //                            var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

        //                            oLvClosingData = LastLvData.CloseBal;
        //                            UtilizedLv = LastLvData.LVCount;
        //                        }

        //                        if (oLvCreditPolicy.ProdataFlag == true)
        //                        {

        //                            var AttendaceData = db.EmployeePayroll.Include(e => e.SalAttendance)
        //                                .Where(e => e.Employee.Id == oEmployeeId
        //                                //&&
        //                                //e.SalAttendance.Any(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
        //                                //Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate)
        //                                ).FirstOrDefault();

        //                            if (AttendaceData != null)
        //                            {
        //                                SumDays = AttendaceData.SalAttendance.Where(a => Convert.ToDateTime("01/" + a.PayMonth) >= Lastyear &&
        //                              Convert.ToDateTime("01/" + a.PayMonth) <= CreditDate && a.PaybleDays != 0).Select(a => a.PaybleDays).ToList().Sum();
        //                            }

        //                            ////suspended days check

        //                            EmployeePayroll othser = db.EmployeePayroll.Where(e => e.Employee.Id == oEmployeeId)
        //                                                    .Include(e => e.OtherServiceBook)
        //                                                    .Include(e => e.OtherServiceBook.Select(x => x.OthServiceBookActivity)).AsNoTracking().OrderBy(e => e.Id)
        //                                                    .SingleOrDefault();

        //                            if (othser.OtherServiceBook != null && othser.OtherServiceBook.Count() > 0)
        //                            {
        //                                List<OtherServiceBook> OthServBkSus = othser.OtherServiceBook.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED" || e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").OrderByDescending(e => e.ReleaseDate).ToList();
        //                                if (OthServBkSus.Count() > 0)
        //                                {
        //                                    var checkSuspenddays = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "SUSPENDED").Select(e => e.ReleaseDate).SingleOrDefault();
        //                                    var checkRejoindays_temp = OthServBkSus.Where(e => e.OthServiceBookActivity.Name.ToUpper() == "REJOIN").Select(e => e.ReleaseDate).SingleOrDefault();
        //                                    var checkRejoindays = "";
        //                                    if (checkRejoindays_temp != null)
        //                                    {
        //                                        checkRejoindays = checkRejoindays_temp.ToString();
        //                                    }
        //                                    else
        //                                    {
        //                                        checkRejoindays = CreditDate.ToString();
        //                                    }
        //                                    if (checkSuspenddays != null && checkRejoindays != null)
        //                                    {
        //                                        if (Convert.ToDateTime(checkSuspenddays).Date < Lastyear)
        //                                        {
        //                                            checkSuspenddays = Lastyear;
        //                                        }
        //                                        if (Convert.ToDateTime(checkRejoindays).Date >= Lastyear && Convert.ToDateTime(checkRejoindays).Date <= CreditDate)
        //                                        {

        //                                            mSusDayFull = Math.Round((Convert.ToDateTime(checkRejoindays).Date - Convert.ToDateTime(checkSuspenddays).Date).TotalDays) + 1;
        //                                            SumDays = SumDays - mSusDayFull;
        //                                        }
        //                                        if (SumDays < 0)
        //                                        {
        //                                            SumDays = 0;
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            var OSalArrT = db.EmployeePayroll
        //                                .Include(e => e.SalaryArrearT)
        //                                .Include(e => e.SalaryArrearT.Select(q => q.ArrearType))
        //                                .Where(e => e.Employee.Id == oEmployeeId)
        //                             .FirstOrDefault();
        //                            if (OSalArrT != null)
        //                            {
        //                                double ArrDays = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
        //                                      && q.FromDate >= Lastyear
        //                                      && q.FromDate <= CreditDate && q.IsRecovery == false).Select(q => q.TotalDays).Sum();
        //                                SumDays = SumDays + ArrDays;

        //                                double ArrDaysrec = OSalArrT.SalaryArrearT.Where(q => q.ArrearType.LookupVal.ToUpper() == "LWP"
        //                                && q.FromDate >= Lastyear
        //                                && q.FromDate <= CreditDate && q.IsRecovery == true).Select(q => q.TotalDays).Sum();
        //                                SumDays = SumDays - ArrDaysrec;

        //                                if (SumDays < 0)
        //                                {
        //                                    SumDays = 0;
        //                                }
        //                            }

        //                        }


        //                        if (oLvCreditPolicy.ExcludeLeaves == true)
        //                        {
        //                            List<LvHead> GetExculdeLvHeads = oLvCreditPolicy.ExcludeLeaveHeads.ToList();
        //                            foreach (var GetExculdeLvHead in GetExculdeLvHeads)
        //                            {

        //                                var _Prv_EmpLvData_exclude = db.EmployeeLeave.AsNoTracking()
        //                                    .Where(a => a.Employee.Id == oEmployeeId)
        //                                    .Include(a => a.LvNewReq)
        //                                    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
        //                                    .Include(a => a.LvNewReq.Select(e => e.WFStatus))
        //                                     .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
        //                                    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
        //                                    .SingleOrDefault();

        //                                var _Prv_EmpLvData_exclude1 = _Prv_EmpLvData_exclude.LvNewReq
        //                                    .Where(a => a.LeaveHead != null
        //                                        && a.LeaveHead.Id == GetExculdeLvHead.Id
        //                                        // && a.LeaveCalendar.Id == lastyearid && a.IsCancel == false && a.WFStatus.LookupVal != "2"
        //                                        && a.IsCancel == false && a.WFStatus.LookupVal != "2"

        //                                ).ToList();

        //                                var LvOrignal_id = _Prv_EmpLvData_exclude.LvNewReq.Where(e => e.LvOrignal != null && e.WFStatus.LookupVal != "2").Select(e => e.LvOrignal.Id).ToList();
        //                                var listLvs = _Prv_EmpLvData_exclude1.Where(e => !LvOrignal_id.Contains(e.Id) && e.FromDate != null && e.ToDate != null).OrderBy(e => e.Id).ToList();
        //                                double DebitSum = 0;
        //                                if (listLvs != null)
        //                                {
        //                                    for (DateTime _Date = Lastyear.Value; _Date <= CreditDate; _Date = _Date.AddDays(1))
        //                                    {
        //                                        var xyz = listLvs.Where(q => _Date >= q.FromDate && _Date <= q.ToDate).FirstOrDefault();
        //                                        if (xyz != null)
        //                                        {
        //                                            DebitSum = DebitSum + 1;
        //                                        }
        //                                    }
        //                                    //  double DebitSum = listLvs.Sum(e => e.DebitDays);
        //                                    SumDays = SumDays - DebitSum;
        //                                }
        //                                else
        //                                {
        //                                    EmployeeLeave _Emp_EmpOpeningData_exclude = db.EmployeeLeave
        //                                        .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == GetExculdeLvHead.Id))
        //                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                        .SingleOrDefault();

        //                                    double _EmpOpeningData_exclude = 0;
        //                                    if (_Emp_EmpOpeningData_exclude != null)
        //                                    {
        //                                        _EmpOpeningData_exclude = _Emp_EmpOpeningData_exclude.LvOpenBal.Where(e => e.LvHead.Id == GetExculdeLvHead.Id).Select(e => e.LVCount).SingleOrDefault();
        //                                    }
        //                                    SumDays = SumDays - _EmpOpeningData_exclude;

        //                                }
        //                                //double CloseBalSum = Filter_oEmpLvData.Where(e => e.LeaveHead.Id == GetExculdeLvHead.Id).Select(e => e.CloseBal).ToList().Sum();
        //                                //SumDays = SumDays - CloseBalSum;
        //                            }
        //                            if (SumDays < 0)
        //                            {
        //                                SumDays = 0;
        //                            }
        //                        }

        //                        //if (SumDays == 0)
        //                        //{
        //                        //    return 0;
        //                        //}

        //                        List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
        //                        if (oLvCreditPolicy.LVConvert == true)
        //                        {
        //                            //if (oLvClosingData > 0)
        //                            //{
        //                            double LastMonthBal = 0, _LvLapsed = 0, Prv_bal = 0;
        //                            LvHead ConvertLeaveHead = oLvCreditPolicy.ConvertLeaveHead.LastOrDefault();
        //                            //Check Exitance
        //                            List<LvNewReq> Filter_oEmpLvDataCon = new List<LvNewReq>();
        //                            if (_Prv_EmpLvData != null)
        //                            {
        //                                LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
        //                                      .Where(a => a.LeaveHead != null
        //                                          && a.LeaveHead.Id == ConvertLeaveHead.Id
        //                                          && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();


        //                                if (DefaultyearLvHeadCreditedCHeck == null)
        //                                {
        //                                    Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
        //                                        .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHead.Id && a.ReqDate >= Lastyear &&
        //                                      a.ReqDate <= CreditDate).ToList();
        //                                }
        //                                else
        //                                {
        //                                    Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
        //                                }
        //                            }
        //                            var _LvNewReq_Prv_bal = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHead.Id)
        //                                .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
        //                            if (_LvNewReq_Prv_bal == 0)
        //                            {
        //                                EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
        //                                    .Include(e => e.LvOpenBal)
        //                                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                    .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHead.Id)).FirstOrDefault();
        //                                if (_Emp_LvOpenBal != null)
        //                                {
        //                                    Prv_bal = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHead.Id).Select(e => e.LvOpening).LastOrDefault();
        //                                }
        //                                else
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                Prv_bal = _LvNewReq_Prv_bal;
        //                            }


        //                            double conevrtedlv = 0;
        //                            double afterconvrtedremainlv = 0;
        //                            if (oLvClosingData > oLvCreditPolicy.LvConvertLimit)
        //                            {
        //                                conevrtedlv = oLvCreditPolicy.LvConvertLimit;
        //                                afterconvrtedremainlv = oLvClosingData - conevrtedlv;
        //                            }
        //                            else
        //                            {
        //                                conevrtedlv = oLvClosingData;
        //                            }

        //                            //  LastMonthBal = Prv_bal + oLvClosingData;
        //                            LastMonthBal = Prv_bal + conevrtedlv;
        //                            //-------------------------------------

        //                            LvNewReq newLvConvertobj = new LvNewReq
        //                            {
        //                                InputMethod = 0,
        //                                ReqDate = DateTime.Now,
        //                                CloseBal = LastMonthBal,
        //                                OpenBal = Prv_bal,
        //                                LvOccurances = 0,
        //                                IsLock = true,
        //                                LvLapsed = _LvLapsed,
        //                                //CreditDays = oLvClosingData,
        //                                CreditDays = conevrtedlv,
        //                                //ToDate = CreditDate,
        //                                //FromDate = CreditDate,
        //                                LvCreditDate = Cal_Wise_Date,
        //                                LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
        //                                LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault(),
        //                                Narration = "Credit Process",
        //                                LvCreditNextDate = NextCreditDays,
        //                                WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
        //                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                            };

        //                            _List_oLvNewReq.Add(newLvConvertobj);

        //                            LvNewReqForReport newLvConvertobjRpt = new LvNewReqForReport
        //                            {
        //                                // ReqDate = DateTime.Now,
        //                                CloseBal = Convert.ToString(LastMonthBal),
        //                                LvLapsed = Convert.ToString(_LvLapsed),
        //                                CreditDays = Convert.ToString(conevrtedlv),
        //                                OpenBal = Convert.ToString(Prv_bal),
        //                                LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHead.Id).SingleOrDefault().LvName,
        //                                // WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
        //                            };
        //                            _List_oLvNewReqRpt.Add(newLvConvertobjRpt);

        //                            // after conversion remaining leave conevrt another leave
        //                            if (oLvCreditPolicy.LVConvertBal == true)
        //                            {
        //                                if (afterconvrtedremainlv > 0)
        //                                {
        //                                    // 20/01/2020 start
        //                                    double LastMonthBalrm = 0, _LvLapsedrm = 0, Prv_balrm = 0;
        //                                    LvHead ConvertLeaveHeadrm = oLvCreditPolicy.ConvertLeaveHeadBal.LastOrDefault();
        //                                    //Check Exitance
        //                                    List<LvNewReq> Filter_oEmpLvDataConrm = new List<LvNewReq>();
        //                                    if (_Prv_EmpLvData != null)
        //                                    {
        //                                        LvNewReq DefaultyearLvHeadCreditedCHeck = _Prv_EmpLvData.LvNewReq
        //                                              .Where(a => a.LeaveHead != null
        //                                                  && a.LeaveHead.Id == ConvertLeaveHeadrm.Id
        //                                                  && a.ReqDate >= LvCalendar.FromDate).SingleOrDefault();


        //                                        if (DefaultyearLvHeadCreditedCHeck == null)
        //                                        {
        //                                            Filter_oEmpLvDataCon = _Prv_EmpLvData.LvNewReq
        //                                                .Where(a => a.LeaveHead != null && a.LeaveHead.Id == ConvertLeaveHeadrm.Id && a.ReqDate >= Lastyear &&
        //                                              a.ReqDate <= CreditDate).ToList();
        //                                        }
        //                                        else
        //                                        {
        //                                            Filter_oEmpLvDataCon.Add(DefaultyearLvHeadCreditedCHeck);
        //                                        }
        //                                    }
        //                                    var _LvNewReq_Prv_balrm = Filter_oEmpLvDataCon.Count() > 0 ? Filter_oEmpLvDataCon.Where(e => e.LeaveHead.Id == ConvertLeaveHeadrm.Id)
        //                                        .OrderByDescending(e => e.Id).Select(e => e.CloseBal).FirstOrDefault() : 0;
        //                                    if (_LvNewReq_Prv_balrm == 0)
        //                                    {
        //                                        EmployeeLeave _Emp_LvOpenBal = db.EmployeeLeave
        //                                            .Include(e => e.LvOpenBal)
        //                                            .Include(e => e.LvOpenBal.Select(a => a.LvHead))
        //                                            .Where(e => e.Employee.Id == oEmployeeId && e.LvOpenBal.Any(a => a.LvHead.Id == ConvertLeaveHeadrm.Id)).FirstOrDefault();
        //                                        if (_Emp_LvOpenBal != null)
        //                                        {
        //                                            Prv_balrm = _Emp_LvOpenBal.LvOpenBal.Where(a => a.LvHead.Id == ConvertLeaveHeadrm.Id).Select(e => e.LvOpening).LastOrDefault();
        //                                        }
        //                                        else
        //                                        {
        //                                            continue;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        Prv_balrm = _LvNewReq_Prv_balrm;
        //                                    }


        //                                    double conevrtedlvrm = 0;

        //                                    if (afterconvrtedremainlv > oLvCreditPolicy.LvConvertLimitBal)
        //                                    {
        //                                        conevrtedlvrm = oLvCreditPolicy.LvConvertLimitBal;

        //                                    }
        //                                    else
        //                                    {
        //                                        conevrtedlvrm = afterconvrtedremainlv;
        //                                    }

        //                                    //  LastMonthBal = Prv_bal + oLvClosingData;
        //                                    LastMonthBalrm = Prv_balrm + conevrtedlvrm;
        //                                    //-------------------------------------

        //                                    LvNewReq newLvConvertobjrm = new LvNewReq
        //                                    {
        //                                        InputMethod = 0,
        //                                        ReqDate = DateTime.Now,
        //                                        CloseBal = LastMonthBalrm,
        //                                        OpenBal = Prv_balrm,
        //                                        LvOccurances = 0,
        //                                        IsLock = true,
        //                                        LvLapsed = _LvLapsed,
        //                                        //CreditDays = oLvClosingData,
        //                                        CreditDays = conevrtedlvrm,
        //                                        //ToDate = CreditDate,
        //                                        //FromDate = CreditDate,
        //                                        LvCreditDate = Cal_Wise_Date,
        //                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
        //                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
        //                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHeadrm.Id).SingleOrDefault(),
        //                                        Narration = "Credit Process",
        //                                        LvCreditNextDate = NextCreditDays,
        //                                        WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
        //                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                                    };
        //                                    _List_oLvNewReq.Add(newLvConvertobjrm);

        //                                    LvNewReqForReport newLvConvertobjrmRpt = new LvNewReqForReport
        //                                    {
        //                                        CloseBal = Convert.ToString(LastMonthBalrm),
        //                                        OpenBal = Convert.ToString(Prv_balrm),
        //                                        LvLapsed = Convert.ToString(_LvLapsed),
        //                                        CreditDays = Convert.ToString(conevrtedlvrm),
        //                                        LeaveHead = db.LvHead.Where(e => e.Id == ConvertLeaveHeadrm.Id).SingleOrDefault().LvName,
        //                                    };
        //                                    _List_oLvNewReqRpt.Add(newLvConvertobjrmRpt);


        //                                    // 20/01/2020 end

        //                                }
        //                            }

        //                            //}
        //                        }

        //                        if (oLvCreditPolicy.FixedCreditDays == true)
        //                        {
        //                            //CreditDays += oLvCreditPolicy.CreditDays;
        //                            CreditDays += Math.Round((oLvCreditPolicy.CreditDays / LYRmonth) * retmonth, 0);
        //                        }
        //                        else
        //                        {
        //                            DateTime CreditDate1 = LvCalendar.FromDate.Value.AddDays(-1);
        //                            DateTime Lastyear1 = Convert.ToDateTime(LvCalendar.FromDate.Value.AddYears(-1));
        //                            if (OCompany.Code.ToString() == "KDCC")
        //                            {
        //                                double WorkingDays1 = oLvCreditPolicy.WorkingDays;
        //                                int totactday = (CreditDate1 - Lastyear1).Days + 1;
        //                                double TotcreditDays = Convert.ToDouble(totactday / 11.40);
        //                                TotcreditDays = Convert.ToInt32(TotcreditDays);
        //                                double totleaveLwp = (totactday - SumDays);
        //                                Double LWPLeave = Convert.ToDouble(totleaveLwp / WorkingDays1);

        //                                int LWPLeave1 = (int)LWPLeave;
        //                                if (LWPLeave > LWPLeave1)
        //                                {
        //                                    LWPLeave1 = LWPLeave1 + 1;
        //                                }

        //                                CreditDays += TotcreditDays - LWPLeave1;

        //                            }
        //                            else
        //                            {

        //                                double WorkingDays = oLvCreditPolicy.WorkingDays;
        //                                double MinMiseWorkingDays = Convert.ToDouble(SumDays / WorkingDays);
        //                                CreditDays += Convert.ToInt32(MinMiseWorkingDays);

        //                            }
        //                        }
        //                        if (CreditDays < 0)
        //                        {
        //                            CreditDays = 0;
        //                        }

        //                        double oLvOccurances = 0;
        //                        if (oLvCreditPolicy.ServiceLink == true)
        //                        {
        //                            var JoiningDate = db.Employee.Include(e => e.ServiceBookDates)
        //                                   .Where(a => a.Id == oEmployeeId).SingleOrDefault();
        //                            DateTime tempCreditDate1 = Convert.ToDateTime(JoiningDate.ServiceBookDates.JoiningDate);
        //                            DateTime till = CreditDate.Value.AddDays(1);
        //                            int compMonths = (till.Month + till.Year * 12) - (tempCreditDate1.Month + tempCreditDate1.Year * 12);
        //                            double daysInEndMonths = (till - till.AddMonths(1)).Days;
        //                            double monthss = compMonths + (tempCreditDate1.Day - till.Day) / daysInEndMonths;
        //                            var GetServiceYear = Math.Round(monthss / 12, 0);

        //                            // var GetServiceYear = Convert.ToDateTime(DateTime.Now - tempCreditDate).Year;
        //                            if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit && oLvCreditPolicy.AboveServiceMaxYears == false)
        //                            {
        //                                CreditDays = 0;
        //                            }
        //                            if (oLvCreditPolicy.AboveServiceMaxYears == true)
        //                            {
        //                                if (GetServiceYear > oLvCreditPolicy.ServiceYearsLimit)
        //                                {
        //                                    //double FinalServiceyear = GetServiceYear + oLvCreditPolicy.AboveServiceSteps;
        //                                    //if (GetServiceYear >= FinalServiceyear && GetServiceYear <= FinalServiceyear)
        //                                    //{
        //                                    //    CreditDays = 0;
        //                                    //}
        //                                    bool Chkcrdyr = false;
        //                                    for (double i = oLvCreditPolicy.ServiceYearsLimit; i <= GetServiceYear; )
        //                                    {
        //                                        var updatenew = i + oLvCreditPolicy.AboveServiceSteps;
        //                                        i = updatenew;
        //                                        if (updatenew == GetServiceYear)
        //                                        {
        //                                            //will credit
        //                                            Chkcrdyr = true;
        //                                            break;
        //                                        }
        //                                    }
        //                                    if (Chkcrdyr == false)
        //                                    {
        //                                        CreditDays = 0;
        //                                    }
        //                                }
        //                            }

        //                            if (oLvClosingData > oLvCreditPolicy.MaxLeaveDebitInService)
        //                            {
        //                                CreditDays = 0;
        //                            }

        //                        }
        //                        if (oLvCreditPolicy.OccInServAppl == true)
        //                        {
        //                            if (item.LvCode.ToUpper() == "ML" || item.LvCode.ToUpper() == "PTL")
        //                            {
        //                                if (UtilizedLv >= oLvCreditPolicy.OccInServ)
        //                                {
        //                                    CreditDays = 0;
        //                                }
        //                                else if (oLvCreditPolicy.OccCarryForward == true)
        //                                {
        //                                    oLvOccurances = UtilizedLv;
        //                                }
        //                            }
        //                        }

        //                        //lvbank start
        //                        //var _LvBankPolicy = db.LvBankPolicy.Include(e => e.LvHeadCollection)
        //                        //    .Where(q => q.LvHeadCollection.Any(r => r.Id == item.Id)).SingleOrDefault();

        //                        LvBankPolicy _LvBankPolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == item.Id
        //                            && e.LvHeadFormula.LvCreditPolicy != null)
        //                            .Select(r => r.LvHeadFormula.LvBankPolicy).FirstOrDefault();



        //                        if (_LvBankPolicy != null)
        //                        {

        //                            LvBankOpenBal OLvCreditRecordLvBankOpenBal = new LvBankOpenBal();
        //                            var _LvBankOpenBalprv = db.LvBankOpenBal
        //                               .Where(a => a.CreditDate >= Lastyear &&
        //                                          a.CreditDate <= CreditDate
        //                              ).SingleOrDefault();
        //                            if (_LvBankOpenBalprv != null)
        //                            {
        //                                var _LvBankOpenBal = db.LvBankOpenBal
        //                                .Where(e => e.LvCalendar.Id == LvCalendarId
        //                               ).SingleOrDefault();

        //                                if (_LvBankOpenBal == null)
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
        //                                    {
        //                                        OpeningBalance = _LvBankOpenBalprv.ClosingBalance,
        //                                        CreditDays = _LvBankPolicy.LvDebitInCredit,

        //                                        UtilizedDays = 0,
        //                                        CreditDate = DateTime.Now,
        //                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                                        LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
        //                                    };

        //                                    db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
        //                                    db.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.UtilizedDays = 0;
        //                                    _LvBankOpenBal.CreditDate = DateTime.Now;
        //                                    _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
        //                                    _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

        //                                    db.LvBankOpenBal.Attach(_LvBankOpenBal);
        //                                    db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();
        //                                }

        //                            }
        //                            else
        //                            {
        //                                var _LvBankOpenBal = db.LvBankOpenBal
        //                               .Where(e => e.LvCalendar.Id == LvCalendarId
        //                              ).SingleOrDefault();

        //                                if (_LvBankOpenBal == null)
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    LvBankOpenBal _OLvCreditRecordLvBankOpenBal = new LvBankOpenBal()
        //                                    {
        //                                        OpeningBalance = 0,
        //                                        CreditDays = _LvBankPolicy.LvDebitInCredit,
        //                                        UtilizedDays = 0,
        //                                        CreditDate = DateTime.Now,
        //                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
        //                                        LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault()
        //                                    };
        //                                    db.LvBankOpenBal.Add(_OLvCreditRecordLvBankOpenBal);
        //                                    db.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    OLvCreditRecord.DebitDays = _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.CreditDays = _LvBankOpenBal.CreditDays + _LvBankPolicy.LvDebitInCredit;
        //                                    _LvBankOpenBal.UtilizedDays = 0;
        //                                    _LvBankOpenBal.CreditDate = DateTime.Now;
        //                                    _LvBankOpenBal.DBTrack = new DBTrack() { Action = "M", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
        //                                    _LvBankOpenBal.LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

        //                                    db.LvBankOpenBal.Attach(_LvBankOpenBal);
        //                                    db.Entry(_LvBankOpenBal).State = System.Data.Entity.EntityState.Modified;
        //                                    db.SaveChanges();

        //                                }
        //                            }


        //                        }
        //                        //lvbank end
        //                        double newBal = 0, LvLapsed = 0;
        //                        if (oLvCreditPolicy.Accumalation == true)
        //                        {
        //                            double tempcreditdays = CreditDays;
        //                            CreditDays += oLvClosingData;

        //                            if (CreditDays > oLvCreditPolicy.AccumalationLimit)
        //                            {
        //                                LvLapsed = CreditDays - oLvCreditPolicy.AccumalationLimit;
        //                                CreditDays = oLvCreditPolicy.AccumalationLimit;
        //                            }
        //                            if (oLvCreditPolicy.AccumulationWithCredit == true)
        //                            {
        //                                if (CreditDays >= oLvCreditPolicy.AccumalationLimit)
        //                                {
        //                                    double diff = oLvCreditPolicy.AccumalationLimit - oLvClosingData;
        //                                    tempcreditdays = diff;
        //                                    //newBal = oLvClosingData - diff;
        //                                    // LvLapsed = newBal;
        //                                    // CreditDays += newBal;
        //                                    //if (CreditDays > oLvCreditPolicy.AccumalationLimit)
        //                                    //{
        //                                    //    CreditDays = oLvCreditPolicy.AccumalationLimit;
        //                                    //}
        //                                }
        //                            }
        //                            OLvCreditRecord.CreditDays = tempcreditdays;
        //                            OLvCreditRecord.OpenBal = oLvClosingData;
        //                            CreditDays -= OLvCreditRecord.DebitDays;
        //                        }
        //                        else
        //                        {
        //                            // OLvCreditRecord.OpenBal = CreditDays;
        //                            OLvCreditRecord.CreditDays = CreditDays;
        //                            CreditDays -= OLvCreditRecord.DebitDays;
        //                        }


        //                        if (CreditDays != 0)
        //                        {
        //                            //  var NextCreditDays = CreditDate.Value.AddYears(1);
        //                            if (OLvCreditRecord.CreditDays == 0)
        //                            {
        //                                // OLvCreditRecord.CreditDays = CreditDays;
        //                            }
        //                            if (OLvCreditRecord.OpenBal == 0)
        //                            {
        //                                //OLvCreditRecord.OpenBal = CreditDays;
        //                            }
        //                            OLvCreditRecord.LvCreditDate = Cal_Wise_Date;
        //                            OLvCreditRecord.InputMethod = 0;
        //                            OLvCreditRecord.IsLock = true;
        //                            OLvCreditRecord.ReqDate = DateTime.Now;
        //                            OLvCreditRecord.CloseBal = CreditDays;
        //                            OLvCreditRecord.LVCount = oLvOccurances;
        //                            OLvCreditRecord.LvLapsed = LvLapsed;
        //                            //OLvCreditRecord.ToDate = CreditDate;
        //                            //OLvCreditRecord.FromDate = CreditDate;
        //                            OLvCreditRecord.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
        //                            OLvCreditRecord.LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault();
        //                            OLvCreditRecord.Narration = "Credit Process";
        //                            OLvCreditRecord.LvCreditNextDate = NextCreditDays;
        //                            OLvCreditRecord.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
        //                            OLvCreditRecord.DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
        //                            _List_oLvNewReq.Add(OLvCreditRecord);

        //                            OLvCreditRecordForRpt.OpenBal = Convert.ToString(OLvCreditRecord.OpenBal);
        //                            OLvCreditRecordForRpt.CreditDays = Convert.ToString(OLvCreditRecord.CreditDays);
        //                            OLvCreditRecordForRpt.DebitDays = Convert.ToString(OLvCreditRecord.DebitDays);
        //                            OLvCreditRecordForRpt.CloseBal = Convert.ToString(CreditDays);
        //                            OLvCreditRecordForRpt.LvCount = Convert.ToString(oLvOccurances);
        //                            OLvCreditRecordForRpt.LvLapsed = Convert.ToString(LvLapsed);
        //                            OLvCreditRecordForRpt.LeaveHead = db.LvHead.Where(e => e.Id == item.Id).SingleOrDefault().LvName;
        //                            _List_oLvNewReqRpt.Add(OLvCreditRecordForRpt);


        //                            if (_List_oLvNewReq.Count > 0)
        //                            {
        //                                if (_List_oLvNewReq.Count >= 2)
        //                                {
        //                                    _List_oLvNewReq[0].LvOrignal = OLvCreditRecord;
        //                                    if (_List_oLvNewReq.Count == 3)
        //                                    {
        //                                        _List_oLvNewReq[1].LvOrignal = OLvCreditRecord;
        //                                    }
        //                                }
        //                                var _Emp = db.EmployeeLeave.Include(e => e.LvNewReq)
        //                                    .Where(e => e.Employee.Id == oEmployeeId).SingleOrDefault();
        //                                for (int i = 0; i < _List_oLvNewReq.Count; i++)
        //                                {
        //                                    _Emp.LvNewReq.Add(_List_oLvNewReq[i]);
        //                                }
        //                                db.Entry(_Emp).State = System.Data.Entity.EntityState.Modified;
        //                                db.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //        }
        //        if (_List_oLvNewReqRpt.Count > 0)
        //        {
        //            return _List_oLvNewReqRpt;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //    return null;
        //}

        public static Int32 Delete_CreditRecord(Int32 EmpId, Int32 LvCalendarId, Int32 LvHead_ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeeLeave oEmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == EmpId)
                      .Include(a => a.LvNewReq)
                      .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                      .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                      .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                      .Include(a => a.LvNewReq.Select(e => e.LvOrignal))
                      .Where(e => e.LvNewReq.Any(a => a.LeaveCalendar.Id == LvCalendarId))
                      .AsParallel()
                      .SingleOrDefault();
                var EmpLvHeadList = db.LvHead.Where(e => e.Id == LvHead_ids)
                      .Select(a => new { Id = a.Id, LvCode = a.LvCode }).Distinct().AsParallel().ToList();
                foreach (var item in EmpLvHeadList)
                {
                    List<LvNewReq> prevCreditProcessData = new List<LvNewReq>();
                    if (oEmpLvData != null)
                    {
                        Int32 _Chk_lv_apply_or_not = oEmpLvData.LvNewReq.Where(a =>
                            a.LeaveCalendar.Id == LvCalendarId &&
                              a.LeaveHead.Id == item.Id
                              && a.WFStatus.LookupVal != "3").Count();

                        if (_Chk_lv_apply_or_not == 0)
                        {
                            List<LvNewReq> _temp_prevCreditProcessData = new List<LvNewReq>();

                            _temp_prevCreditProcessData = oEmpLvData.LvNewReq.Where(a => a.LeaveCalendar.Id == LvCalendarId && a.LeaveHead.Id == item.Id &&
                               a.WFStatus.LookupVal == "3").ToList();
                            foreach (var s in _temp_prevCreditProcessData)
                            {
                                var bb = oEmpLvData.LvNewReq.Where(a => a.LeaveCalendar.Id == LvCalendarId && a.LvOrignal != null && a.LvOrignal.Id == s.Id).ToList();
                                if (bb.Count > 0)
                                {
                                    prevCreditProcessData.AddRange(bb);
                                }
                            }
                            prevCreditProcessData.AddRange(_temp_prevCreditProcessData);
                        }
                        else
                        {
                            return 0;
                            //lv apply can't credit for head
                        }
                    }
                    else
                    {
                        return 1;
                    }
                    if (prevCreditProcessData != null && prevCreditProcessData.Count > 0)
                    {
                        db.LvNewReq.RemoveRange(prevCreditProcessData);
                        db.SaveChanges();
                        return 1;
                    }
                    else
                    {
                        return 1;

                    }
                }
                return 1;
            }
        }

    }
}