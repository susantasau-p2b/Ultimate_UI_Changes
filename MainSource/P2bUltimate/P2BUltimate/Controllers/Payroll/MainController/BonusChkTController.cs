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
    public class BonusChkTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/BonusChkT/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_BonusChkT.cshtml");

        }

        public class BonusChkTChildDataClass
        {
            public string Id { get; set; }
            public string ProcessDate { get; set; }
            public string TotalWorkingDays { get; set; }
            public string TotalBonus { get; set; }
            public string TotalExGracia { get; set; }
            public string TotalAmount { get; set; }
        }
        public ActionResult Get_BonusChkTDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll.Include(e => e.BonusChkT)
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<BonusChkTChildDataClass> returndata = new List<BonusChkTChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.BonusChkT))
                        {
                            returndata.Add(new BonusChkTChildDataClass
                            {
                                Id = item.Id.ToString(),
                                ProcessDate = item.ProcessDate != null ? item.ProcessDate.Value.ToString("dd/MM/yyyy") : null,
                                TotalWorkingDays = item.TotalWorkingDays.ToString(),
                                TotalBonus = item.TotalBonus.ToString(),
                                TotalExGracia = item.TotalExGracia.ToString(),
                                TotalAmount = item.TotalAmount.ToString()
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

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.BonusChkT

                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         ProcessDate = e.ProcessDate,
                         TotalWorkingDays = e.TotalWorkingDays,
                         TotalBonus = e.TotalBonus,
                         TotalExGracia = e.TotalExGracia,
                         TotalAmount = e.TotalAmount
                     }).ToList();

                var BonusChk = db.BonusChkT.Find(data);
                Session["RowVersion"] = BonusChk.RowVersion;
                var Auth = BonusChk.DBTrack.IsModified;

                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public ActionResult GetCalendarDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == "BONUSYEAR") && e.Default == true).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Calendar.Where(e => !e.Id.ToString().Contains(a.ToString()) && e.Name.LookupVal.ToUpper() == "BONUSYEAR").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }
        public ActionResult Create(FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msgs = new List<string>();
            string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
            string Calendar = form["Calendarlist"] == "0" ? "" : form["Calendarlist"];
            bool TaxCalcIsallowed = form["TaxCalcIsallowed"] == "0" ? false : Convert.ToBoolean(form["TaxCalcIsallowed"]);
            string ProcessType1 = form["ProcTypeList"] == "" ? "" : form["ProcTypeList"];

            int ProcessType = 0;
            if (TaxCalcIsallowed == true)
            {
                if (ProcessType1 == "")
                {
                    Msgs.Add(" Kindly select Tax Calculation method");
                    return Json(new { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                }
                ProcessType = Convert.ToInt32(ProcessType1);
            }
           

            //if (TaxCalcIsallowed == true)
            //{
            //    if (ProcessType == 0)
            //    {
            //        Msgs.Add("Please select tax calculation on..!!  ");
            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgs }, JsonRequestBehavior.AllowGet);
            //    }
            //}

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }


                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    CompanyPayroll OCompanyPayroll = null;

                    int CalendarId = int.Parse(Calendar);
                    Calendar Cal = db.Calendar.Where(e => e.Id == CalendarId).SingleOrDefault();
                    List<string> Msg = new List<string>();
                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                                //using (TransactionScope ts = new TransactionScope())
                                //{
                                //    Process.PayrollReportGen.BonusCalc(OCompanyPayroll.Id, OEmployeePayroll.Id, Cal, TaxCalcIsallowed, ProcessType);
                                //    ts.Complete();

                                //}
                                if (db.SalaryT.Include(e => e.FinnanceYearId).Any(e => e.EmployeePayroll_Id == OEmployeePayroll.Id && (Cal.FromDate >= e.FinnanceYearId.FromDate && Cal.ToDate <= e.FinnanceYearId.ToDate)))
                                {
                                    using (TransactionScope ts = new TransactionScope())
                                    {
                                        Process.PayrollReportGen.BonusCalc(OCompanyPayroll.Id, OEmployeePayroll.Id, Cal, TaxCalcIsallowed, ProcessType);
                                        ts.Complete();

                                    }
                                }
                                else
                                {
                                    Msg.Add(OEmployee.EmpCode + " - Salary data not available in Selected Financial Year.");
                                }
                            }

                        }

                        // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        //List<string> Msg = new List<string>();
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.Employee.EmpCode.ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                                              ).ToList();
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
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null//,
                                //JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                //Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
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

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
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
                    throw e;
                }
            }
        }

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
                    data = DateTime.Now.ToString("MM/yyyy");
                    List<ProcType> a = new List<ProcType>();

                    a = new List<ProcType>()
                    {
                        new ProcType() { Id = 0, Text = "Actual Investment & Actual Income" }, 
                        new ProcType() { Id = 1, Text = "Declare Investment & Projected Income" },
                        new ProcType() { Id = 2, Text = "Actual Investment & Projected Income" }
                    };


                    SelectList s = new SelectList(a, "Id", "Text");
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
    }
}