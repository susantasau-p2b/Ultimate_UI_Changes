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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace P2BUltimate.Controllers
{
    public class MenuAuthorityController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_MenuAuthority.cshtml");
        }
        [HttpPost]
        public JsonResult PutJson(string EmpCode)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var browserjson = "";
                    var group_type = "";
                    if (Request.InputStream != null)
                    {
                        Stream sr = Request.InputStream;
                        sr.Seek(0, System.IO.SeekOrigin.Begin);
                        string json = new StreamReader(sr).ReadToEnd();
                        JObject NewFileData = JObject.Parse(json);
                        var NewUrlAuthoritydata = NewFileData["urlauthority"];
                        // string menuname = "";
                        var id = Convert.ToInt32(EmpCode);
                        var path1 = "";
                        List<string> lastnameOfoldurlauthority = new List<string>();
                        Employee Emp = new Employee();
                        if (id != 0)
                        {
                            Emp = db.Employee.Where(e => e.Id == id).SingleOrDefault();
                            path1 = Server.MapPath("~/App_Data/Menu_Json/" + Emp.EmpCode + ".json");
                        }
                        if (System.IO.File.Exists(path1))
                        {

                            var json1 = System.IO.File.ReadAllText(path1);
                            var OldFiledata = JObject.Parse(json1);
                            var result = OldFiledata.SelectToken("urlauthority");
                            var DeleteOldUrlAuthoritydata = OldFiledata["urlauthority"];
                            var OldUrlAuthoritydata = OldFiledata["urlauthority"].ToList();

                            foreach (JProperty item in OldUrlAuthoritydata)
                            {
                                if (item != null)
                                {
                                    if (NewUrlAuthoritydata != null)
                                    {
                                        if (NewUrlAuthoritydata[item.Name] == null)
                                        {
                                            DeleteOldUrlAuthoritydata[item.Name].Parent.Remove();
                                        }
                                        else
                                        {
                                            lastnameOfoldurlauthority.Add(item.Name);
                                        }

                                    }
                                }
                            }

                            foreach (JProperty item in NewUrlAuthoritydata.ToList())
                            {
                                // JToken item1 = OldUrlAuthoritydata.FirstOrDefault().First;
                                if (item != null)
                                {
                                    if (DeleteOldUrlAuthoritydata != null)
                                    {
                                        if (DeleteOldUrlAuthoritydata[item.Name] == null)
                                        {
                                            OldFiledata.SelectToken("urlauthority." + lastnameOfoldurlauthority.LastOrDefault() + "").Parent.AddAfterSelf(new JProperty(item.Name, item.Value));
                                        }

                                    }
                                }
                            }
                            //StreamReader sr = new System.IO.StreamReader(Request.InputStream);

                            //browserjson = sr.ReadToEnd();
                            //   var serializer = new JavaScriptSerializer();
                            //  var dese = serializer.Deserialize<String>(json);
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
                            String path = Server.MapPath("~/App_Data/Menu_Json/");
                            if (string.IsNullOrEmpty(Convert.ToString(Session["NewUserId"])))
                            {
                                Session["NewUserId"] = Emp.EmpCode;
                            }
                            if (!String.IsNullOrEmpty(Convert.ToString(Session["NewUserId"])))
                            {
                                System.IO.File.WriteAllText(path + "" + Emp.EmpCode + ".json", json);
                                Request.InputStream.Dispose();
                                Request.InputStream.Flush();
                                return Json(new { success = true, responseText = "File Updated Successfully" });
                            }
                            else
                            {
                                return Json(new { success = false });

                            }
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
        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string Fulldetails { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Id = Convert.ToInt32(gp.id);
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> ITProjectionList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";

                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    if (PayMonth != null && PayMonth != "")
                    {

                    }
                    var BindEmpList = db.Employee
                           .Include(e => e.EmpName)
                           .ToList();

                    foreach (var z in BindEmpList)
                    {
                        var path = "";
                        path = Server.MapPath("~/App_Data/Menu_Json/" + z.EmpCode + ".json");

                        if (System.IO.File.Exists(path))
                        {
                            view = new P2BGridData()
                            {
                                Id = z.Id,
                                EmpCode = z.EmpCode,
                                Fulldetails = z.FullDetails,
                                //   ShortListingStatus = z.ResumeSortlistingStatus.LookupVal,
                                // FromPeriod = all.PeriodFrom,
                                /// Toperiod = all.PeriodTo,
                                // Islock = all.IsLocked,
                                // ReportDate = all.ReportDate.Value.ToString("dd/MM/yyyy")
                            };

                            model.Add(view);
                        }

                        //}
                    }
                    ITProjectionList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ITProjectionList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                                || (e.Fulldetails.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.EmpCode, a.Fulldetails.ToString(), a.Id }).ToList();
                            //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.Fulldetails.ToString(), a.Id }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ITProjectionList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                             gp.sidx == "Fulldetails" ? c.Fulldetails.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.Fulldetails, a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.Fulldetails, a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmpCode), a.Fulldetails, a.Id }).ToList();
                        }
                        totalRecords = ITProjectionList.Count();
                    }
                    if (totalRecords > 0)
                    {
                        totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
                    }
                    if (gp.page > totalPages)
                    {
                        gp.page = totalPages;
                    }
                    var JsonData = new
                    {
                        page = gp.page,
                        rows = jsonData,
                        records = totalRecords,
                        total = totalPages
                    };
                    return Json(JsonData, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    List<string> Msg = new List<string>();
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GetJsonEmpWise(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (String.IsNullOrEmpty(SessionManager.EmpId))
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                // var id = Convert.ToInt32(SessionManager.EmpId);
                var path = "";
                if (data != 0)
                {
                    var Emp = db.Employee.Where(e => e.Id == data).SingleOrDefault();
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

                    if (!System.IO.File.Exists(path))
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


    }
}