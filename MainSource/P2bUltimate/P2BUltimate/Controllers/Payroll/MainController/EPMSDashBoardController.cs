using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using P2BUltimate.Security;
using Newtonsoft.Json;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class EPMSDashBoardController : Controller
    {
     
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/EPMSDashBoard/Index.cshtml");
        }
        public class GradeWiseEmp
        {
            public string Grade { get; set; }
            public Int32 EmpCount { get; set; }
        }
        //private DataBaseContext db = new DataBaseContext();

        public JsonResult GetSalary()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();

                var aa = db.SalaryT.Where(e => e.PayMonth != null).Select(a => a.PayMonth).AsParallel().ToList();
                List<DateTime> _tempDatetime = new List<DateTime>();
                if (aa != null)
                {
                    foreach (var item in aa)
                    {
                        _tempDatetime.Add(Convert.ToDateTime(item));
                    }
                }
                var _Datetime = _tempDatetime.Distinct().OrderByDescending(e => e.Date).Take(5);
                var label = new List<String>();
                List<GradeWiseEmp> GradeWiseEmpList = new List<GradeWiseEmp>();
                var datas = new List<string>();
                foreach (var item in _Datetime)
                {
                    var month = "" + item.Month + "";
                    if (item.Month < 10)
                    {
                        month = "0" + item.Month + "";
                    }
                    var temp = month + "/" + item.Year;
                    datas.Add(db.SalaryT.Where(e => e.PayMonth != null && e.PayMonth == temp).Select(e => e.TotalEarning).ToList().Sum().ToString());
                    label.Add(item.ToShortDateString());
                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Net Value",
                    data = datas.ToArray(),

                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getJsondata()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();
                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();
                var GradeId = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.PayStruct != null &&
                    e.ServiceBookDates.ServiceLastDate == null).Select(e => e.PayStruct.Grade).Distinct().ToList();

                List<GradeWiseEmp> GradeWiseEmpList = new List<GradeWiseEmp>();
                var label = new List<String>();
                var datas = new List<String>();
                foreach (var item in GradeId)
                {
                    datas.Add(db.Employee.Where(e => e.PayStruct != null && e.PayStruct.Grade.Id == item.Id &&
                        e.ServiceBookDates.ServiceLastDate == null).ToList().Count().ToString());
                    label.Add(item.Name);

                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Net Value",
                    data = datas.ToArray(),
                    //   backgroundColor = { "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0" }

                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getRetirementDate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();

                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();
                var aa = db.SalaryT.Where(e => e.PayMonth != null).Select(a => a.PayMonth).AsParallel().ToList();
                List<DateTime> _tempDatetime = new List<DateTime>();
                if (aa != null)
                {
                    foreach (var item in aa)
                    {
                        _tempDatetime.Add(Convert.ToDateTime(item));
                    }
                }
                var _Datetime = _tempDatetime.Distinct().OrderByDescending(e => e.Date).Take(5);
                var label = new List<String>();
                var datas = new List<String>();
                foreach (var item in _Datetime)
                {
                    var _month = "" + item.Month + "";
                    if (item.Month < 10)
                    {
                        _month = "0" + _month + "";
                    }
                    var _lastDateOfMonth = Convert.ToDateTime(DateTime.DaysInMonth(item.Year, item.Month) + "/" + _month + "/" + item.Year);

                    label.Add(item.ToShortDateString());
                    datas.Add(db.Employee.Where(e => e.ServiceBookDates.RetirementDate != null && e.ServiceBookDates.ServiceLastDate == null &&
                        (e.ServiceBookDates.RetirementDate >= item && e.ServiceBookDates.RetirementDate <= _lastDateOfMonth)).ToList().Count().ToString());
                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Total Employee",
                    data = datas.ToArray(),
                    //backgroundColor = { "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0" }

                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult getIncrementDueList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();

                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();
                var aa = db.SalaryT.Where(e => e.PayMonth != null).Select(a => a.PayMonth).AsParallel().ToList();
                List<DateTime> _tempDatetime = new List<DateTime>();
                if (aa != null)
                {
                    foreach (var item in aa)
                    {
                        _tempDatetime.Add(Convert.ToDateTime(item));
                    }
                }
                var _Datetime = _tempDatetime.Distinct().OrderByDescending(e => e.Date).Take(5);

                List<GradeWiseEmp> GradeWiseEmpList = new List<GradeWiseEmp>();

                var label = new List<String>();
                var datas = new List<String>();
                foreach (var item in _Datetime)
                {
                    var _month = "" + item.Month + "";
                    if (item.Month < 10)
                    {
                        _month = "0" + _month + "";
                    }
                    var _lastDateOfMonth = Convert.ToDateTime(DateTime.DaysInMonth(item.Year, item.Month) + "/" + _month + "/" + item.Year);

                    datas.Add(db.EmployeePayroll.Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null
                        && e.IncrDataCalc.ProcessIncrDate >= item && e.IncrDataCalc.ProcessIncrDate <= _lastDateOfMonth).ToList().Count().ToString());
                    label.Add(item.ToShortDateString());
                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Total Employee",
                    data = datas.ToArray(),
                    //backgroundColor = { "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0" }
                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();

                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
