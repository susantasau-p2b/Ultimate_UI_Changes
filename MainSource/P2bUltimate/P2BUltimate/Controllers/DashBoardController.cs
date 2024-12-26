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
    public class DashBoardController : Controller
    {
        //
        // GET: /DashBoard/
        public ActionResult Index()
        {
            return View();
        }

        public class GradeWiseEmp
        {
            public string Grade { get; set; }
            public Int32 EmpCount { get; set; }
        }

        public static List<DateTime> SortDescending(List<DateTime> list)
        {
            list.Sort((a, b) => b.CompareTo(a));
            return list;
        }

        public JsonResult GetEmployeeStraingth()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();

                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset>();

                var totalEmp = db.Employee.ToList().Count().ToString();
                var Emp = db.Employee.Include(a => a.ServiceBookDates).Where(a => a.ServiceBookDates != null && a.ServiceBookDates.ServiceLastDate != null).AsEnumerable()
                    .Select(a => new
                {
                    Month = a.ServiceBookDates.ServiceLastDate.Value
                }).ToList();
                List<DateTime> MonthList = SortDescending(Emp.Select(a => a.Month).Distinct().ToList());

                var label = new List<String>();
                var datas = new List<String>();
                foreach (var item in MonthList.OrderByDescending(e => e.Date).Take(5))
                {
                    var datetime = Convert.ToDateTime(item);
                    label.Add(item.ToShortDateString());
                    datas.Add(db.Employee.Include(a => a.ServiceBookDates).AsEnumerable().Where(a =>
                            (a.ServiceBookDates != null && a.ServiceBookDates.ServiceLastDate == null) ||
                            (a.ServiceBookDates != null && a.ServiceBookDates.ServiceLastDate != null
                            && a.ServiceBookDates.ServiceLastDate.Value < datetime)).ToList().Count().ToString());
                }
                dataset.Add(new P2BUltimate.Models.ChartModel.dataset
                {
                    label = "Total Employee",
                    data = datas.ToArray()
                });
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();

                var output = JsonConvert.SerializeObject(chartdatasource);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpSalaryGradeWise()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
                P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();
                var data = new List<P2BUltimate.Models.ChartModel.Data>();
                var dataset = new List<P2BUltimate.Models.ChartModel.dataset2>();
                var label = new List<String>();
                var datas = new List<String>();
                var SalData = db.EmployeePayroll
                    .Where(e =>
                   e.Employee.PayStruct != null &&
                   e.Employee.ServiceBookDates.ServiceLastDate == null)
                   .Select(e => new
                   {
                       Grade = e.Employee.PayStruct.Grade.Name,
                   }).ToList();

                if (SalData != null)
                {
                    foreach (var item in SalData.Select(e => e.Grade).Distinct())
                    {
                        label.Add(item);
                    }
                }
                List<P2BUltimate.Models.ChartModel.dataset> odataset_TotalEarning = new List<P2BUltimate.Models.ChartModel.dataset>();
                var GradeList = SalData.Select(a => a.Grade.ToUpper()).ToList();
                var vTotalEarning = db.EmployeePayroll
                    .Where(e => GradeList.Contains(e.Employee.PayStruct.Grade.Name.ToUpper()))
                    .Select(e => new
                    {
                        SalaryT = e.SalaryT
                    }).ToList();
                List<string> odata_TotalEarning = new List<string>();

                foreach (var item in vTotalEarning.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalEarning.Add(item.TotalEarning.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalEarning",
                    data = odata_TotalEarning.ToArray(),
                    backgroundColor = "#c9cbcf",
                    borderColor = "#c9cbcf",

                });
                var vTotalDeduction = db.EmployeePayroll
                 .Where(e => GradeList.Contains(e.Employee.PayStruct.Grade.Name.ToUpper())
                 )
                 .Select(e => new
                 {
                     SalaryT = e.SalaryT
                 }).ToList();

                List<P2BUltimate.Models.ChartModel.dataset> odataset_TotalDeduction = new List<P2BUltimate.Models.ChartModel.dataset>();
                List<string> odata_TotalDeduction = new List<string>();

                foreach (var item in vTotalDeduction.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalDeduction.Add(item.TotalDeduction.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalDeduction",
                    data = odata_TotalDeduction.ToArray(),
                    backgroundColor = "#ff6384",
                    borderColor = "#ff6384"

                });
                var vTotalNet = db.EmployeePayroll
               .Where(e => GradeList.Contains(e.Employee.PayStruct.Grade.Name.ToUpper())
               )
               .Select(e => new
               {
                   SalaryT = e.SalaryT
               }).ToList();

                List<P2BUltimate.Models.ChartModel.dataset> odataset_TotalNet = new List<P2BUltimate.Models.ChartModel.dataset>();
                List<string> odata_TotalNet = new List<string>();
                foreach (var item in vTotalNet.SelectMany(e => e.SalaryT).OrderByDescending(a => a.Id))
                {
                    odata_TotalNet.Add(item.TotalNet.ToString());
                }
                dataset.Add(new ChartModel.dataset2
                {
                    label = "TotalNet",
                    data = odata_TotalNet.ToArray(),
                    backgroundColor = "#36a2eb",
                    borderColor = "#36a2eb",
                });
                //var aa = dataset.ToArray();
                //if (aa.Count() > 0)
                //{
                //}
                chartdatasource.datasets = dataset.ToArray();
                chartdatasource.labels = label.ToArray();
                var output = JsonConvert.SerializeObject(chartdatasource, Formatting.None);
                return Json(output, JsonRequestBehavior.AllowGet);
            }
        }





        public ActionResult CheckEmp(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int EmpId = 0;
                
                EmpId = Convert.ToInt32(SessionManager.UserName);
                
                int emp = db.Employee
                    .Include(e => e.Login)
                    .Where(e => e.Id == EmpId && e.Login != null).FirstOrDefault().Login.Id;

                var logincheck = db.Login.Where(e => e.Id == emp).FirstOrDefault();

                var data = "false";
                if (logincheck.UserId.ToUpper() == "ADMIN")
                {

                    data = "true";

                }

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
