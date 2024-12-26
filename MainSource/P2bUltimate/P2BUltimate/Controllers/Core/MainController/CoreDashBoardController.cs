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
using P2BUltimate.Models;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class CoreDashBoardController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/CoreDashBoard/Index.cshtml");
        }

        public JsonResult GetEmployeeStraingthLocationWise()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();

                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();


                var totalEmp = db.Employee.ToList().Count().ToString();
                var Emp = db.Employee.Include(a => a.GeoStruct).Include(a => a.GeoStruct.Location).Include(a => a.GeoStruct.Location.LocationObj)
                    .Where(a => a.GeoStruct != null && a.GeoStruct.Location != null).AsEnumerable()
                    .Select(a => new
                    {
                        Location = a.GeoStruct.Location
                    }).ToList();

                var label = new List<String>();
                var datas = new List<String>();
                foreach (var item in Emp.Select(a => a.Location).Distinct().OrderByDescending(a => a.Id))
                {
                    //var datetime = Convert.ToDateTime(item);
                    label.Add(item.LocationObj.LocDesc);
                    datas.Add(db.Employee.Include(a => a.GeoStruct).Include(a => a.GeoStruct.Location).Include(a => a.GeoStruct.Location.LocationObj)
                        .Where(a => a.GeoStruct.Location.Id == item.Id).ToList().Count().ToString());
                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Total Employee",
                    data = datas.ToArray(),
                  //  backgroundColor = { "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0" }

                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAttendancePaymonthWise()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();

                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var GradeId = db.SalAttendanceT.Where(e => e.PayMonth != null).Select(e => e.PayMonth).Distinct().ToList();
                List<DateTime> _tempDatetime = new List<DateTime>();
                if (GradeId != null)
                {
                    foreach (var item in GradeId)
                    {
                        _tempDatetime.Add(Convert.ToDateTime(item));
                    }
                }
                var _Datetime = _tempDatetime.Distinct().OrderByDescending(e => e.Date).Take(5);
                var label = new List<String>();
                var datas = new List<String>();
                foreach (var item in _Datetime)
                {
                    var month = "" + item.Month + "";
                    if (item.Month < 10)
                    {
                        month = "0" + item.Month + "";
                    }
                    var temp = month + "/" + item.Year;
                    var empCount = db.EmployeePayroll.Where(a => a.SalAttendance.Count() > 0 && a.SalAttendance.Any(x => x.PayMonth == temp)).Select(a => new { Id = a.Id }).ToList();
                    var id = empCount.Select(a => a.Id).ToList();
                    var Salattendance = db.EmployeePayroll.Where(a => id.Contains(a.Id))
                        .Select(a => new
                        {
                            SumSalAttendance = a.SalAttendance.Select(x => x.PaybleDays).Sum(),
                        }).ToList();
                    var Per = Convert.ToInt32(((Salattendance.Select(a => a.SumSalAttendance).Sum() / Convert.ToDouble(Salattendance.Select(a => a.SumSalAttendance).Count())) / 365) * 100);


                    label.Add(item.ToShortDateString());
                    datas.Add(Per.ToString());
                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Total Employee",
                    data = datas.ToArray(),
                   // backgroundColor = { "#c9cbcf", "#ff6384", "#36a2eb", "#4bc0c0" }
                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
