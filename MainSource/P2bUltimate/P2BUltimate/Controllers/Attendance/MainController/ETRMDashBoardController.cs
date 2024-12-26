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
    public class ETRMDashBoardController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/ETRMDashBoard/Index.cshtml");
        }
        public class GradeWiseEmp
        {
            public string Grade { get; set; }
            public Int32 EmpCount { get; set; }
        }

        public JsonResult GetEmployeeAttendance()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ChartModel.ChartDataSource chartdatasource = new ChartModel.ChartDataSource();
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
                var _Datetime = _tempDatetime.Distinct().OrderByDescending(e => e.Date).Take(10);
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
                    var sum_pay = db.SalAttendanceT.Where(e => e.PayMonth != null && e.PayMonth == temp).Select(e => e.PaybleDays).ToList();
                    var ag_sum_pay = sum_pay.Sum() / sum_pay.Count();

                    var Per_month_per = (ag_sum_pay /
                        DateTime.DaysInMonth(item.Year, item.Month)) * 100;
                    datas.Add(Per_month_per.ToString());
                    label.Add(item.ToShortDateString());
                }
                dataset.Add(new ChartModel.dataset
                {
                    label = "Percentage Attendance",
                    data = datas.ToArray(),

                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
