using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using System.Text;
namespace P2BUltimate.Controllers
{
    public class ETMSDashBoardController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/EDMSDashBoard/Index.cshtml");
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
