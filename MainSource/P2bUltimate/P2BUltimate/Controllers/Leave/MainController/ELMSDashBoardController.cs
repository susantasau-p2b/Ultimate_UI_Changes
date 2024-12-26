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
    public class ELMSDashBoardController : Controller
    {
        //
        // GET: /DashBoard/
        public ActionResult Index()
        {
            return View("~/Views/Leave/MainViews/ELMSDashBoard/Index.cshtml");
        }
        //public JsonResult getJsondata()
        //{
        //    P2BUltimate.Models.ChartModel.ChartDataSource chartdatasource = new P2BUltimate.Models.ChartModel.ChartDataSource();
        //    P2BUltimate.Models.ChartModel.ChartParameter chartparameter = new P2BUltimate.Models.ChartModel.ChartParameter();
        //    chartparameter.theme = "fint";
        //    chartparameter.caption = "Monthly Revenue";
        //    chartparameter.bgColor = "#F8F8F8";
        //    chartparameter.xAxisName = "Month";
        //    chartparameter.yAxisName = "Amount (In USD)";
        //    chartparameter.numberPrefix = "$";
        //    chartparameter.subCaption = "Last year";

        //    chartdatasource.Chart = chartparameter;

        //    var data = new List<P2BUltimate.Models.ChartModel.Data>();
        //    data.Add(new P2BUltimate.Models.ChartModel.Data { label = "jan", value = "420000" });
        //    data.Add(new P2BUltimate.Models.ChartModel.Data { label = "Feb", value = "810000" });

        //    chartdatasource.Data = data.ToArray();
        //    var output = JsonConvert.SerializeObject(chartdatasource);
        //    return Json(output, JsonRequestBehavior.AllowGet);

        //}
    }
}
