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


namespace P2BUltimate.Controllers.Core.MainController
{
    public class FuncStructController : Controller
    {
        //
        // GET: /GeoStruct/


        public DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PopulateDropDownList(string data, string data2)
        {
            var qurey = db.FuncStruct.ToList();
            var selected = (Object)null;
            if (data2 != "" && data != "0" && data2 != "0")
            {
                selected = Convert.ToInt32(data2);
            }

            SelectList s = new SelectList(qurey, "Id", "Id", selected);
            return Json(s, JsonRequestBehavior.AllowGet);
        }
    }
}