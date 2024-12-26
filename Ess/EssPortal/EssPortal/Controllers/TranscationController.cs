using EssPortal.App_Start;
using P2b.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using EssPortal.Models;
using System.Web.Script.Serialization;
using EssPortal.Security;
namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class TranscationController : Controller
    {
      //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Advance_partial()
        {
            return View("~/Views/Shared/_AdvanceFilter.cshtml");
        }
        public ActionResult Geo_partial()
        {
            return View("~/Views/Shared/_GeoFilter.cshtml");
        }
        public ActionResult fun_partial()
        {
            return View("~/Views/Shared/_FunFilter.cshtml");
        }
        public ActionResult pay_partial()
        {
            return View("~/Views/Shared/_PayFilter.cshtml");
        }
       // private DataBaseContext db = new DataBaseContext();

        public ActionResult Check_ExitanceFirstLevelGeo()
        {
            var a = ExitanceFirstlevelGeo();
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "No Data Found In GeoStruct" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Check_ExitanceFirstLevelPay()
        {
            var a = ExitanceFirstlevelPay();
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "No Data Found In PayStruct" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Check_ExitanceFirstLevelFun()
        {
            var a = ExitanceFirstlevelFun();
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "No Data Found In FunStruct" }, JsonRequestBehavior.AllowGet);
            }
        }
        public Object ExitanceFirstlevelGeo()
        {
            /*
             * After Company login
             * Get Geo Data
             */
            var data = (string)null;
            var a = Get_Division(data);
            if (a != null)
            {
                var json_data = CreateJson("division", a);
                return json_data;
            }
            else
            {
                var b = Get_location(data);
                if (b != null)
                {
                    var json_data = CreateJson("location", b);
                    return json_data;

                }
                else
                {
                    var e = Get_DepartMent(data);

                    if (e != null)
                    {
                        var json_data = CreateJson("department", e);
                        return json_data;

                    }
                    else
                    {
                        var c = Get_Group(data);
                        if (c != null)
                        {
                            var json_data = CreateJson("group", c);
                            return json_data;


                        }
                        else
                        {
                            var d = Get_Unit(data);
                            if (d != null)
                            {
                                var json_data = CreateJson("unit", d);
                                return json_data;

                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }

        }
        public Object ExitanceFirstlevelPay()
        {
            var data = (string)null;
            var a = Get_Grade(data);
            if (a != null)
            {
                var json_data = CreateJson("grade", a);
                return json_data;
            }
            else
            {
                var b = Get_Level(data);
                if (b != null)
                {
                    var json_data = CreateJson("level", b);
                    return json_data;
                }
                else
                {
                    return null;
                }
            }
        }
        public Object ExitanceFirstlevelFun()
        {
            var data = (string)null;
            var a = Get_Job(data);
            if (a != null)
            {
                var tablename = "job-table";
                var return_data = new
                {
                    data = a,
                    tablename = tablename
                };
                return return_data;
            }
            else
            {
                var b = Get_JobPosition(data);
                if (b != null)
                {
                    var tablename = "jobposition-table";
                    var return_data = new
                    {
                        data = b,
                        tablename = tablename
                    };
                    return return_data;
                }
                else
                {
                    return null;
                }
            }
        }
        private Object CreateJson(string tablename, List<Utility.returndataclass> data)
        {
            var json = new
            {
                tablename = tablename + "-table",
                data = data,
            };
            return json;
        }

        private List<Utility.returndataclass> Get_Job(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var Corporate_data = db.FuncStruct.Include(e => e.Job).Where(e => e.Job != null).Select(e => e.Job).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var ca in Corporate_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        private List<Utility.returndataclass> Get_JobPosition(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var Corporate_data = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.JobPosition != null).Select(e => e.JobPosition).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var ca in Corporate_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.JobPositionDesc
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.FuncStruct.Include(e => e.JobPosition).Where(e => (e.JobPosition.Id == id) && (e.JobPosition != null)).Select(e => e.JobPosition).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.JobPositionDesc,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }

        private List<Utility.returndataclass> Get_Grade(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var Corporate_data = db.PayStruct.Include(e => e.Grade).Where(e => e.Grade != null).Select(e => e.Grade).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var ca in Corporate_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.FullDetails
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        private List<Utility.returndataclass> Get_Level(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var Corporate_data = db.PayStruct.Include(e => e.Level).Where(e => e.Level != null).Select(e => e.Level).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var ca in Corporate_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.FullDetails
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.PayStruct.Include(e => e.Level).Where(e => (e.Grade.Id == id) && (e.Level != null)).Select(e => e.Level).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }
        public List<Utility.returndataclass> Get_JobStatus(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.PayStruct
                        .Include(e => e.JobStatus)
                        .Include(e => e.JobStatus.EmpActingStatus)
                        .Include(e => e.JobStatus.EmpStatus)
                        .Where(e => e.JobStatus != null).Distinct().ToList();
                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var item in geo_filter_data.Select(e => e.JobStatus))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.PayStruct
                        .Include(e => e.JobStatus)
                        .Include(e => e.JobStatus.EmpActingStatus)
                        .Include(e => e.JobStatus.EmpStatus)
                        .Where(e => ((e.Grade.Id == id) || (e.Level.Id == id)) &&
                        (e.JobStatus != null)).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data.Count > 0)
                    {
                        foreach (var item in data.Select(e => e.JobStatus))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        private List<Utility.returndataclass> Get_Division(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.GeoStruct.Include(e => e.Division)
                        .Where(e => e.Division.Id != null)
                        .Select(e => e.Division).Distinct().ToList();

                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var ca in geo_filter_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.GeoStruct.Where(e => e.Company.Id == id && e.Division.Id != null).Select(e => e.Division).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data[0] != null && data.Count > 0)
                    {
                        foreach (var ca in data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }
        private List<Utility.returndataclass> Get_location(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.GeoStruct.Include(e => e.Location)
                        .Include(e => e.Location.LocationObj)
                        .Where(e => e.Location.Id != null)
                        .ToList();
                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var item in geo_filter_data.Select(e => e.Location).Distinct())
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.GeoStruct.Include(e => e.Location.LocationObj).Where(e => e.Division.Id == id && e.Location.Id != null).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data.Count > 0)
                    {
                        foreach (var item in data.Select(e => e.Location).Distinct())
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }
        private List<Utility.returndataclass> Get_DepartMent(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.GeoStruct.Include(e => e.Department)
                        .Include(e => e.Department.DepartmentObj)
                        .Where(e => e.Department.Id != null)
                        .ToList();
                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var item in geo_filter_data.Select(e => e.Department).Distinct())
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.GeoStruct.Include(e => e.Department.DepartmentObj).Where(e => e.Location.Id == id && e.Department.Id != null).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data.Count > 0)
                    {
                        foreach (var item in data.Select(e => e.Department).Distinct())
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        private List<Utility.returndataclass> Get_Group(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.GeoStruct.Include(e => e.Group)
                        .Where(e => e.Group.Id != null)
                        .Select(e => e.Group).Distinct().ToList();
                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var item in geo_filter_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.Name,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.GeoStruct.Where(e => ((e.Location.Id == id) || (e.Department.Id == id)) && e.Group.Id != null).Select(e => e.Group).Distinct().ToList();
                    if (data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

                        foreach (var item in data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.Name,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }

                }
            }
        }
        private List<Utility.returndataclass> Get_Unit(string id_string)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.GeoStruct.Include(e => e.Unit)
                        .Where(e => e.Unit.Id != null)
                        .Select(e => e.Unit).Distinct().ToList();
                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var item in geo_filter_data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.Name,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var id = Convert.ToInt32(id_string);
                    var data = db.GeoStruct.Where(e => ((e.Location.Id == id) || (e.Department.Id == id) || (e.Group.Id == id)) && e.Unit.Id != null).Select(e => e.Unit).Distinct().ToList();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    if (data.Count > 0)
                    {
                        foreach (var item in data)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        return returndata;
                    }
                    else
                    {
                        return null;
                    }

                }

            }
        }

        private Object CheckLevelGeo(String typeoftable, string data)
        {
            if (typeoftable != null)
            {
                switch (typeoftable)
                {
                    case "division-table":

                        var i = Get_location(data);
                        if (i != null)
                        {
                            var json_data = CreateJson("location", i);
                            return json_data;

                        }
                        else
                        {
                            var f = Get_DepartMent(data);

                            if (f != null)
                            {
                                var json_data = CreateJson("department", f);
                                return json_data;

                            }
                            else
                            {
                                var g = Get_Group(data);
                                if (g != null)
                                {
                                    var json_data = CreateJson("group", g);
                                    return json_data;

                                }
                                else
                                {
                                    var h = Get_Unit(data);
                                    if (h != null)
                                    {
                                        var json_data = CreateJson("unit", h);
                                        return json_data;

                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        break;

                    case "location-table":

                        var j = Get_DepartMent(data);

                        if (j != null)
                        {
                            var json_data = CreateJson("department", j);
                            return json_data;
                        }
                        else
                        {
                            var k = Get_Group(data);
                            if (k != null)
                            {
                                var json_data = CreateJson("group", k);
                                return json_data;
                            }
                            else
                            {
                                var l = Get_Unit(data);
                                if (l != null)
                                {
                                    var json_data = CreateJson("unit", l);
                                    return json_data;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        break;
                    case "department-table":

                        var kl = Get_Group(data);
                        if (kl != null)
                        {
                            var json_data = CreateJson("group", kl);
                            return json_data;
                        }
                        else
                        {
                            var l = Get_Unit(data);
                            if (l != null)
                            {
                                var json_data = CreateJson("unit", l);
                                return json_data;

                            }
                            else
                            {
                                return null;
                            }
                        }
                        break;
                    case "group-table":
                        var m = Get_Unit(data);
                        if (m != null)
                        {
                            var json_data = CreateJson("unit", m);
                            return json_data;

                        }
                        else
                        {
                            return null;
                        }
                        break;

                    case "unit-table":
                        return null;
                        break;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }

        }
        public ActionResult GenrationGeo(String data, String typeoftable)
        {
            List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

            var a = CheckLevelGeo(typeoftable, data);
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "Further No Data Found \n Click on Apply" }, JsonRequestBehavior.AllowGet);
            }
        }

        private Object CheckLevelPay(String typeoftable, string data)
        {
            switch (typeoftable)
            {
                case "grade-table":
                    var a = Get_Level(data);
                    if (a != null)
                    {
                        var Jsondata = CreateJson("level", a);
                        return Jsondata;
                    }
                    else
                    {
                        var b = Get_JobStatus(data);
                        if (b != null)
                        {
                            var Jsondata = CreateJson("jobstatus", b);
                            return Jsondata;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;

                case "level-table":
                    return null;
                    break;

                case "jobstatus-table":
                    return null;
                    break;
            }
            return "";
        }
        public ActionResult GenrationPay(String data, String typeoftable)
        {
            List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

            var a = CheckLevelPay(typeoftable, data);
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "Further No Data Found \n Click on Load Employee" }, JsonRequestBehavior.AllowGet);
            }
        }

        private Object CheckLevelFun(String typeoftable, string data)
        {

            switch (typeoftable)
            {
                case "job-table":
                    var a = Get_JobPosition(data);
                    if (a != null)
                    {
                        var json_data = CreateJson("jobposition", a);
                        return json_data;
                    }
                    else
                    {
                        var b = Get_JobPosition(data);
                        if (b != null)
                        {
                            var json_data = CreateJson("jobposition", b);
                            return json_data;

                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;

                case "jobposition-table":
                    return null;
                    break;
                default:
                    return null;
            }
        }
        public ActionResult GenrationFun(String data, String typeoftable)
        {
            List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

            var a = CheckLevelFun(typeoftable, data);
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "Further No Data Found \n Click on Load Employee" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Get_Geoid(string data, FormCollection form_data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var company_ids = form_data["company-table"];
                var division_ids = form_data["division-table"];
                var location_ids = form_data["location-table"];
                var department_ids = form_data["department-table"];
                var group_ids = form_data["group-table"];
                var unit_ids = form_data["unit-table"];
                if (company_ids == null && division_ids == null && location_ids == null && department_ids == null && group_ids == null && unit_ids == null)
                {
                    return Json("GeoStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
                var comp_id = Convert.ToInt32(company_ids);

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<int> Geo_list = new List<int>();
                if (division_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(division_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Division != null && e.Division.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (location_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(location_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Location != null && e.Location.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (department_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(department_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Department != null && e.Department.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (group_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(group_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Group != null && e.Group.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }
                if (unit_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(unit_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.GeoStruct.Where(e => e.Unit != null && e.Unit.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }
                var comp_single_data = Geo_list.Distinct().ToList();
                List<string> geo_id = new List<string>();
                if (comp_single_data.Count != 0)
                {
                    foreach (var item in comp_single_data)
                    {
                        geo_id.Add(item.ToString());
                    }
                    return Json(geo_id, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Geo Id Not Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Get_Funid(string data, FormCollection form_data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var job_ids = form_data["job-table"];
                var jobposition_ids = form_data["jobposition-table"];
                if (job_ids == null && jobposition_ids == null)
                {
                    return Json("FunStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
                List<int> FuncStruct_List = new List<int>();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (job_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(job_ids);

                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.FuncStruct.Where(e => e.Job != null && e.Job.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                        }
                    }
                }
                if (jobposition_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobposition_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.FuncStruct.Where(e => e.JobPosition != null && e.JobPosition.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                        }
                    }
                }

                var comp_single_data = FuncStruct_List.Distinct().ToList();
                List<string> geo_id = new List<string>();
                if (comp_single_data.Count != 0)
                {
                    foreach (var item in comp_single_data)
                    {
                        geo_id.Add(item.ToString());
                    }
                    return Json(geo_id, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("FunStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Get_Payid(string data, FormCollection form_data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var grade_ids = form_data["grade-table"];
                var level_ids = form_data["level-table"];
                var jobstatus_ids = form_data["jobstatus-table"];

                if (grade_ids == null && level_ids == null && jobstatus_ids == null)
                {
                    return Json("PayStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }

                List<int> PayStruct_List = new List<int>();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (grade_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(grade_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.PayStruct.Where(e => e.Grade != null && e.Grade.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                }
                if (level_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(level_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.PayStruct.Where(e => e.Level != null && e.Level.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                }
                if (jobstatus_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobstatus_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db.PayStruct.Where(e => e.JobStatus != null && e.JobStatus.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                }
                var comp_single_data = PayStruct_List.Distinct().ToList();
                List<string> geo_id = new List<string>();
                if (comp_single_data.Count != 0)
                {
                    foreach (var item in comp_single_data)
                    {
                        geo_id.Add(item.ToString());
                    }
                    return Json(new Object[] { geo_id }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("PayStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_Employelist(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).SingleOrDefault();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                                if (item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != monthyr)
                                {
                                    data.Add(item);
                                }


                            }
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && e.Employee.PayStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                                if (item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != monthyr)
                                {
                                    data.Add(item);
                                }

                            }
                        }
                    }
                }
                if (deserialize.FuncStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FuncStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && e.Employee.FuncStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                                if (item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != monthyr)
                                {
                                    data.Add(item);
                                }
                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        if (item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != monthyr)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table"
                    };
                    return Json(returnjson, JsonRequestBehavior.AllowGet);
                }
                else if (empdata.EmployeePayroll.Select(e => e.Employee).ToList().Count > 0)
                {
                    foreach (var item in empdata.EmployeePayroll.Select(e => e.Employee).Distinct())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else if (item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != monthyr)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table"
                    };
                    return Json(returnjson, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Get_Emplist(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Where(e => e.Company.Id == compid).SingleOrDefault();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                data.Add(item);

                            }
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && e.Employee.PayStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                data.Add(item);

                            }
                        }
                    }
                }
                if (deserialize.FuncStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FuncStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && e.Employee.FuncStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                data.Add(item);
                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct())
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Id.ToString(),
                            value = item.FullDetails,
                        });
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee_table"
                    };
                    return Json(returnjson, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult ByDefaultLoadEmp()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var comp_id = Session["CompId"].ToString();
                List<string> GeoStruct = new List<string>();
                List<string> PayStruct = new List<string>();
                List<string> FunStruct = new List<string>();
                if (comp_id != null)
                {
                    var id = Convert.ToInt32(comp_id);
                    var geo_id = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == id).Distinct().ToList();
                    if (geo_id.Count > 0)
                    {
                        foreach (var ca in geo_id.Select(e => e.Id.ToString()))
                        {
                            GeoStruct.Add(ca);
                        }
                    }

                    var pay_id = db.PayStruct.Where(e => e.Company != null && e.Company.Id == id).Distinct().ToList();
                    if (pay_id.Count > 0)
                    {
                        foreach (var ca in pay_id.Select(e => e.Id.ToString()))
                        {
                            PayStruct.Add(ca);
                        }
                    }

                    var fun_id = db.FuncStruct.Where(e => e.Company != null && e.Company.Id == id).Distinct().ToList();
                    if (fun_id.Count > 0)
                    {
                        foreach (var ca in fun_id.Select(e => e.Id.ToString()))
                        {
                            FunStruct.Add(ca);
                        }
                    }
                }
                else
                {
                    GeoStruct = null;
                    PayStruct = null;
                    FunStruct = null;
                }
                var jsondata = new
                {
                    GeoStruct = GeoStruct,
                    PayStruct = PayStruct,
                    FunStruct = FunStruct
                };
                return Json(jsondata, JsonRequestBehavior.AllowGet);
            }
        }

        #region EMPSELECT GETSTRUCT

        public ActionResult Get_Struct(String empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var emp_id = Convert.ToInt32(empid);
                var emp_data = db.Employee.Include(e => e.PayStruct).Include(e => e.PayStruct.Grade).Include(e => e.PayStruct.Level)
                                    .Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition)
                                    .Include(e => e.GeoStruct).Include(e => e.GeoStruct.Corporate).Include(e => e.GeoStruct.Division)
                                    .Include(e => e.GeoStruct.Location).Include(e => e.GeoStruct.Department)
                                    .Include(e => e.GeoStruct.Group).Include(e => e.GeoStruct.Unit)
                                    .Where(e => e.Id == emp_id).SingleOrDefault();
                var pay_data = (Object)null;
                if (emp_data.PayStruct != null)
                {
                    pay_data = db.PayStruct.Where(e => e.Id == emp_data.PayStruct.Id)
                       .Select(e => new
                       {
                           code = e.Id.ToString(),
                           value = e.Grade.Name.ToString() + " " + e.Level.Name.ToString()
                       }).ToList();

                }
                var fun_data = (Object)null;
                if (emp_data.FuncStruct != null)
                {

                    fun_data = db.FuncStruct.Where(e => e.Id == emp_data.FuncStruct.Id).Select(e => new
                    {
                        code = e.Id.ToString(),
                        value = e.Job.Name.ToString() + "" + e.JobPosition.JobPositionDesc.ToString(),

                    }).ToList();
                }
                var geo_data = (Object)null;
                if (emp_data.GeoStruct != null)
                {

                    geo_data = db.GeoStruct.Where(e => e.Id == emp_data.GeoStruct.Id).Select(e => new
                    {
                        Code = e.Id.ToString(),
                        value = e.Id.ToString() + " " + e.Location.LocationObj.LocDesc.ToString() + " " + e.Department.DepartmentObj.DeptDesc.ToString()
                    }).ToList();
                }
                //var geo_data = db.GeoStruct.Where(e=>e.Id==emp_data.GeoStruct.Id)
                return Json(new Object[] { fun_data, pay_data, geo_data }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
	}
}