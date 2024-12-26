using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Script.Serialization;
using P2b.Global;
using Newtonsoft.Json.Linq;

namespace P2BUltimate.Controllers
{
    public class FormAuthorizationController : Controller
    {
        //
        // GET: /FormAuthorization/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetEmp1(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpData = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Include(e => e.Login).Where(e => e.Login != null && e.Login.IsUltimateHOAppl == true && e.ServiceBookDates != null && e.ServiceBookDates.ServiceLastDate == null && e.Login.UserId != "admin").OrderBy(e => e.EmpCode).ToList();


                var SelectListItem = new List<SelectListItem>();
                foreach (var item in EmpData)
                {
                    var path = "";
                    path = Server.MapPath("~/App_Data/Menu_Json/" + item.EmpCode + ".json");

                    if (System.IO.File.Exists(path))
                    {
                        SelectListItem.Add(new SelectListItem
                        {
                            Text = "EmpCode :" + item.EmpCode + " EmpName :" + item.EmpName.FullNameFML,
                            Value = item.Id.ToString()
                        });
                    }

                }

                var r = (from ca in SelectListItem select new { srno = ca.Value, lookupvalue = ca.Text }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
                // return Json(SelectListItem, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetJsonEmpWise(string EmpID)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (String.IsNullOrEmpty(SessionManager.EmpId))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                var id = Convert.ToInt32(EmpID);
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
        public class DeserializeClass
        {
            public string urlauthority { get; set; }

        }
        [HttpPost]
        public JsonResult PutJson(string EmpCode)
        {
            try
            {

                using (DataBaseContext db = new DataBaseContext())
                {


                    //}
                    var browserjson = "";
                    var group_type = "";
                    if (Request.InputStream != null)
                    {
                        Stream sr = Request.InputStream;
                        sr.Seek(0, System.IO.SeekOrigin.Begin);
                        string json = new StreamReader(sr).ReadToEnd();
                        JObject NewFileData = JObject.Parse(json);
                        var NewUrlAuthoritydata = NewFileData["urlauthority"].ToList();
                        // string menuname = "";
                        var id = Convert.ToInt32(EmpCode);
                        var path1 = "";
                        Employee Emp = new Employee();
                        if (id != 0)
                        {
                            Emp = db.Employee.Where(e => e.Id == id).SingleOrDefault();
                            path1 = Server.MapPath("~/App_Data/Menu_Json/" + Emp.EmpCode + ".json");
                        }
                        var json1 = System.IO.File.ReadAllText(path1);
                        var OldFiledata = JObject.Parse(json1);
                        var OldUrlAuthoritydata = OldFiledata["urlauthority"];
                        foreach (JProperty item in NewUrlAuthoritydata)
                        {
                            if (item != null)
                            {
                                //string propertyname = item.Name;
                                //if (propertyname!="")
                                //{

                                //}
                                //JToken val =  item;
                                //menuname = item.ToString().Substring(0, item.ToString().IndexOf(":"));
                                if (OldUrlAuthoritydata != null)
                                {
                                    if (OldUrlAuthoritydata[item.Name] != null)
                                        OldUrlAuthoritydata[item.Name].Replace(item.Value);
                                    if (OldUrlAuthoritydata != null)
                                    {

                                    }
                                }
                            }
                        }
                       
                        String path = Server.MapPath("~/App_Data/Menu_Json/");
                        if (string.IsNullOrEmpty(Convert.ToString(Session["NewUserId"])))
                        {
                            Session["NewUserId"] = Emp.EmpCode;
                        }
                        if (!String.IsNullOrEmpty(Convert.ToString(Session["NewUserId"])))
                        {
                            System.IO.File.WriteAllText(path + "" + Emp.EmpCode + ".json", OldFiledata.ToString());
                            Request.InputStream.Dispose();
                            Request.InputStream.Flush();
                            return Json(new { success = true, responseText = "File Updated Successfully" });
                        }
                        else
                        {
                            return Json(new { success = false });

                        }
                    }
                    else
                    {
                        return Json(new { success = false, responseText = "Post Json is null" });
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, responseText = e.ToString() });
            }
        }


    }
}