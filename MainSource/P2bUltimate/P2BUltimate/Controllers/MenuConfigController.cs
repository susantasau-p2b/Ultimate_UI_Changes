using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml;
using P2BUltimate.App_Start;
using System.Net;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using System.Text;
using System.Threading.Tasks;
using P2BUltimate.Security;
using Payroll;
using Leave;


namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class MenuConfigController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
       // private DataBaseContext db = new DataBaseContext();
        public void check_objectornot(String data)
        {
            Session["object"] = data.ToString();
        }
        private class returnclass
        {
            public string val { get; set; }
        }
        public JsonResult GetUrl(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Lookup.Include(e => e.LookupValues)
                    .Where(e => e.Code == data).SingleOrDefault();
                List<returnclass> returndata = new List<returnclass>();
                if (qurey != null)
                {
                    foreach (var ca in qurey.LookupValues.Distinct())
                    {
                        returndata.Add(new returnclass
                        {
                            val = ca.LookupVal,
                        });
                    }
                    return Json(new { success = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true, data = returndata }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetJson(String data)
        {
            var MenuType = "Default";
            return File(Server.MapPath("~/App_Data/Menu_Json/" + MenuType + ".json"), "text/json");
        }
        public void firstrun(string data)
        {
            if (Session["ModuleType"] == null && data == null)
            {
                Session["ModuleType"] = "core";
            }

        }
        public ActionResult GetJsonEmpWise()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (String.IsNullOrEmpty(SessionManager.EmpId))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                var id = Convert.ToInt32(SessionManager.EmpId);
                var path = "";
                if (id != 0)
                {
                    var Emp = db.Employee.Where(e => e.Id == id).SingleOrDefault();
                    path = Server.MapPath("~/App_Data/Menu_Json/" + Emp.EmpCode + ".json");
                }
                //var id = Convert.ToInt32(SessionManager.EmpId);
                //  var admincheck = db.Login.Include(e => e.Employee).Where(e => e.Employee.Id == id && e.UserId == "admin").SingleOrDefault();
                //if (admincheck != null)
                //{
                //    return Json(null, JsonRequestBehavior.AllowGet);
                //}
                if (System.IO.File.Exists(path))
                {
                    return File(path, "text/json");
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public void Moduletype(String data)
        {
            if (data != null)
            {
                Session["ModuleType"] = data;
            }
            else
            {
                Session["ModuleType"] = string.Empty;
            }

        }
        public JsonResult PutJson()
        {
            try
            {
                var browserjson = "";
                var group_type = "";
                if (Request.InputStream != null)
                {
                    StreamReader sr = new System.IO.StreamReader(Request.InputStream);
                    browserjson = sr.ReadToEnd();

                    if (Request.QueryString["type"] == "0" || Request.QueryString["type"] == null)
                    {
                        group_type = "Default";
                    }
                    else
                    {
                        group_type = Request.QueryString["type"];
                    }
                    String path = Server.MapPath("~/App_Data/Menu_Json/");
                    System.IO.File.WriteAllText(path + "" + group_type + ".json", browserjson);
                    Request.InputStream.Dispose();
                    Request.InputStream.Flush();
                    return Json(new { success = true, responseText = "File Updated Successfully" });
                }
                else
                {
                    return Json(new { success = false, responseText = "Post Json is null" });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.ToString() });
            }
        }
    }
}