using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System.Web.Script.Serialization;
using P2BUltimate.Models;
using Recruitment;
using Payroll;
using IR;
using P2BUltimate.Security;
using Attendance;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class TranscationController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Advance_partial()
        {
            return View("~/Views/Shared/_AdvanceFilter.cshtml");
        }
        public ActionResult Advance_partialNew()
        {
            return View("~/Views/Shared/_AdvanceFilterNew.cshtml");
        }
        public ActionResult FormulaFilter()
        {
            return View("~/Views/Shared/_FormulaFilter.cshtml");
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
        public ActionResult pay_partialFor()
        {
            return View("~/Views/Shared/_PayFilterFor.cshtml");
        }
        //    private DataBaseContext db = new DataBaseContext();

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
        public List<Object> ExitanceFirstlevelGeo()
        {
            /*
             * After Company login
             * Get Geo Data
             */
            var data = (string)null;
            var a = Get_Division(data);
            var b1 = Get_location(data, "COMPANY");
            List<Object> returnList = new List<object>();
            if (b1 != null)
            {
                var json_data = CreateJson("location", b1);
                returnList.Add(json_data);
            }
            if (a != null)
            {
                var json_data = CreateJson("division", a);
                returnList.Add(json_data);
                return returnList;
            }
            else
            {
                var b = Get_location(data);
                if (b != null)
                {
                    var json_data = CreateJson("location", b);
                    return new List<object> { json_data };

                }
                else
                {
                    var e = Get_DepartMent(data);

                    if (e != null)
                    {
                        var json_data = CreateJson("department", e);
                        return new List<object> { json_data };


                    }
                    else
                    {
                        var c = Get_Group(data);
                        if (c != null)
                        {
                            var json_data = CreateJson("group", c);
                            return new List<object> { json_data };


                        }
                        else
                        {
                            var d = Get_Unit(data);
                            if (d != null)
                            {
                                var json_data = CreateJson("unit", d);
                                return new List<object> { json_data };

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
        public List<Object> ExitanceFirstlevelPay()
        {
            var data = (string)null;

            List<Object> returnList = new List<object>();
            var b1 = Get_JobStatus(data);
            if (b1 != null)
            {
                var json_data = CreateJson("jobstatus", b1);
                returnList.Add(json_data);

            }

            var a = Get_Grade(data);
            if (a != null)
            {
                var json_data = CreateJson("grade", a);
                returnList.Add(json_data);
                return returnList;
            }
            else
            {
                var b = Get_Level(data);
                if (b != null)
                {
                    var json_data = CreateJson("level", b);
                    return new List<object> { json_data };
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Object> ExitanceFirstlevelFun()
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
                return new List<object> { return_data };
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
                    return new List<object> { return_data };
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
                    var data = db.FuncStruct.Include(e => e.JobPosition).Where(e => (e.Job.Id == id) && (e.JobPosition != null)).Select(e => e.JobPosition).Distinct().ToList();
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
                        .Where(e => e.JobStatus != null).Select(e => e.JobStatus).Distinct().ToList();
                    if (geo_filter_data.Count > 0)
                    {
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        foreach (var item in geo_filter_data)
                        {
                            var jobstatusfulldeatils = db.JobStatus.Where(e => e.Id == item.Id).Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).FirstOrDefault();
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = jobstatusfulldeatils.FullDetails,
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
                        .Where(e => (e.Grade.Id == id) && (e.JobStatus != null)).Distinct().ToList();

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
        private List<Utility.returndataclass> Get_location(string id_string, string level = "DIVISION")
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (id_string == null)
                {
                    var geo_filter_data = db.GeoStruct.Include(e => e.Location)
                        .Include(e => e.Location.LocationObj)
                        .Include(e => e.Location.BusinessCategory)
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
                                value = item.FullDetails, //+ "  " + (item.BusinessCategory != null ? item.BusinessCategory.LookupVal.ToString() : ""),
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
                    if (level == "DIVISION")
                    {
                        var id = Convert.ToInt32(id_string);
                        var data = db.GeoStruct.Include(e => e.Location.LocationObj).Include(e => e.Location.BusinessCategory).Where(e => e.Division.Id == id && e.Location.Id != null).Distinct().ToList();
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        if (data.Count > 0)
                        {
                            foreach (var item in data.Select(e => e.Location).Distinct())
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails, // + "  " + (item.BusinessCategory != null ? item.BusinessCategory.LookupVal.ToString() : ""),
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
                        var data = db.GeoStruct.Include(e => e.Location.LocationObj).Include(e => e.Location.BusinessCategory).Where(e => e.Company.Id == id && e.Location.Id != null).Distinct().ToList();
                        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                        if (data.Count > 0)
                        {
                            foreach (var item in data.Select(e => e.Location).Distinct())
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails, //+ "  " + (item.BusinessCategory != null ? item.BusinessCategory.LookupVal.ToString() : ""),
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

        private List<Object> CheckLevelGeo(String typeoftable, string data)
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
                            return new List<object> { json_data };

                        }
                        else
                        {
                            var f = Get_DepartMent(data);

                            if (f != null)
                            {
                                var json_data = CreateJson("department", f);
                                return new List<object> { json_data };
                            }
                            else
                            {
                                var g = Get_Group(data);
                                if (g != null)
                                {
                                    var json_data = CreateJson("group", g);
                                    return new List<object> { json_data };
                                }
                                else
                                {
                                    var h = Get_Unit(data);
                                    if (h != null)
                                    {
                                        var json_data = CreateJson("unit", h);
                                        return new List<object> { json_data };
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
                            return new List<object> { json_data };
                        }
                        else
                        {
                            var k = Get_Group(data);
                            if (k != null)
                            {
                                var json_data = CreateJson("group", k);
                                return new List<object> { json_data };
                            }
                            else
                            {
                                var l = Get_Unit(data);
                                if (l != null)
                                {
                                    var json_data = CreateJson("unit", l);
                                    return new List<object> { json_data };
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
                            return new List<object> { json_data };
                        }
                        else
                        {
                            var l = Get_Unit(data);
                            if (l != null)
                            {
                                var json_data = CreateJson("unit", l);
                                return new List<object> { json_data };

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
                            return new List<object> { json_data };

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

        private List<Object> CheckLevelPay(String typeoftable, string data)
        {
            switch (typeoftable)
            {
                case "grade-table":
                    var a = Get_Level(data);
                    if (a != null)
                    {
                        var Jsondata = CreateJson("level", a);
                        return new List<object> { Jsondata };
                    }
                    else
                    {
                        var b = Get_JobStatus(data);
                        if (b != null)
                        {
                            var Jsondata = CreateJson("jobstatus", b);
                            return new List<object> { Jsondata };

                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;

                case "level-table":

                    var bc = Get_JobStatus(data);
                    if (bc != null)
                    {
                        var Jsondata = CreateJson("jobstatus", bc);
                        return new List<object> { Jsondata };

                    }
                    else
                    {
                        return null;
                    }
                    break;

                case "jobstatus-table":
                    return null;
                    break;
            }
            return new List<object> { };
        }

        private List<Object> CheckLevelPayFor(String typeoftable, string data)
        {
            switch (typeoftable)
            {
                case "grade-table":
                    var a = Get_Level(data);
                    if (a != null)
                    {
                        var Jsondata = CreateJson("level", a);
                        return new List<object> { Jsondata };
                    }
                    else
                    {
                        var b = Get_JobStatus(data);
                        if (b != null)
                        {
                            var Jsondata = CreateJson("jobstatus", b);
                            return new List<object> { Jsondata };

                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;

                case "level-table":

                    var bc = Get_JobStatus(data);
                    if (bc != null)
                    {
                        var Jsondata = CreateJson("jobstatus", bc);
                        return new List<object> { Jsondata };

                    }
                    else
                    {
                        return null;
                    }
                    break;

                case "jobstatus-table":
                    return null;
                    break;
            }
            return new List<object> { };
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

        public ActionResult GenrationPayFor(String data, String typeoftable)
        {
            List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

            var a = CheckLevelPayFor(typeoftable, data);
            if (a != null)
            {
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { responseText = "Further No Data Found \n Click on Load Employee" }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<Object> CheckLevelFun(String typeoftable, string data)
        {

            switch (typeoftable)
            {
                case "job-table":
                    var a = Get_JobPosition(data);
                    if (a != null)
                    {
                        var json_data = CreateJson("jobposition", a);
                        return new List<object> { json_data };
                    }
                    else
                    {
                        var b = Get_JobPosition(data);
                        if (b != null)
                        {
                            var json_data = CreateJson("jobposition", b);
                            return new List<object> { json_data };
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
                //bool IsDivApp = db.GeoStruct.Where(r => r.Division != null).Count() > 0 ? true : false;
                //bool IsLocApp = db.GeoStruct.Where(r => r.Location != null).Count() > 0 ? true : false;
                //bool IsDeptApp = db.GeoStruct.Where(r => r.Department != null).Count() > 0 ? true : false;
                //bool IsGrpApp = db.GeoStruct.Where(r => r.Group != null).Count() > 0 ? true : false;
                //bool IsUnitApp = db.GeoStruct.Where(r => r.Unit != null).Count() > 0 ? true : false;

                var comp_id = Convert.ToInt32(company_ids);
                var db_data = db.GeoStruct.Include(e => e.Division)
                    .Include(e => e.Location)
                    .Include(e => e.Group)
                    .Include(e => e.Unit)
                    .Include(e => e.Department).AsNoTracking().ToList();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<GeoStruct> Geo_list = new List<GeoStruct>();

                if (division_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(division_ids);
                    List<GeoStruct> a = null;
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        a = db_data.Where(e => e.Division != null && e.Division.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            Geo_list.AddRange(a);
                        }
                    }
                    db_data = Geo_list;
                }
                if (location_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(location_ids);
                    Geo_list = new List<GeoStruct>();
                    foreach (var item in ca_id)
                    {
                        var loc_id = Convert.ToInt32(item);
                        var a = db_data.Where(e => e.Location != null && e.Location.Id == loc_id).ToList();
                        if (a.Count > 0)
                        {
                            Geo_list.AddRange(a);
                        }
                    }
                    db_data = Geo_list;
                }
                ////else if (IsLocApp == true)
                ////{
                ////    Geo_list = new List<GeoStruct>();
                ////    var ca_id = Utility.StringIdsToListIds(division_ids);
                ////    foreach (var item in ca_id)
                ////    {
                ////        var div_id = Convert.ToInt32(item);
                ////        List<GeoStruct> a = db_data.Where(e => e.Division != null && e.Division.Id == div_id && e.Location != null).ToList();
                ////        if (a.Count > 0)
                ////        {
                ////            Geo_list.AddRange(a);
                ////        }
                ////        else
                ////        {
                ////            GeoStruct g = db.GeoStruct.Where(e => e.Division != null && e.Division.Id == div_id).SingleOrDefault();
                ////            Geo_list.Add(g);
                ////        }
                ////    }
                ////    db_data = Geo_list;
                ////}
                if (department_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(department_ids);
                    Geo_list = new List<GeoStruct>();
                    foreach (var item in ca_id)
                    {
                        var dept_id = Convert.ToInt32(item);
                        var a = db_data.Where(e => e.Department != null && e.Department.Id == dept_id).ToList();
                        if (a.Count > 0)
                        {
                            Geo_list.AddRange(a);
                        }
                    }
                    db_data = Geo_list;
                }

                if (group_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(group_ids);
                    foreach (var item in ca_id)
                    {
                        var grp_id = Convert.ToInt32(item);
                        var a = db_data.Where(e => e.Group != null && e.Group.Id == grp_id).ToList();
                        if (a.Count > 0)
                        {
                            Geo_list.AddRange(a);
                        }
                    }
                    db_data = Geo_list;
                }
                //else if (IsGrpApp == true)
                //{
                //    Geo_list = new List<GeoStruct>();
                //    var ca_id = Utility.StringIdsToListIds(department_ids);
                //    foreach (var item in ca_id)
                //    {
                //        var dept_id = Convert.ToInt32(item);
                //        List<GeoStruct> a = db_data.Where(e => e.Department != null && e.Department.Id == dept_id && e.Group != null).ToList();
                //        if (a.Count > 0)
                //        {
                //            Geo_list.AddRange(a);
                //        }
                //        else
                //        {
                //            GeoStruct g = db.GeoStruct.Where(e => e.Department != null && e.Department.Id == dept_id).SingleOrDefault();
                //            Geo_list.Add(g);
                //        }
                //    }
                //    db_data = Geo_list;
                //}

                if (unit_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(group_ids);
                    foreach (var item in ca_id)
                    {
                        var unit_id = Convert.ToInt32(item);
                        var a = db_data.Where(e => e.Unit != null && e.Unit.Id == unit_id).ToList();
                        if (a.Count > 0)
                        {
                            Geo_list.AddRange(a);
                        }
                    }
                    db_data = Geo_list;
                }
                //else if (IsUnitApp == true)
                //{
                //    Geo_list = new List<GeoStruct>();
                //    var ca_id = Utility.StringIdsToListIds(group_ids);
                //    foreach (var item in ca_id)
                //    {
                //        var grp_id = Convert.ToInt32(item);
                //        List<GeoStruct> a = db_data.Where(e => e.Group != null && e.Group.Id == grp_id && e.Unit != null).ToList();
                //        if (a.Count > 0)
                //        {
                //            Geo_list.AddRange(a);
                //        }
                //        else
                //        {
                //            GeoStruct g = db.GeoStruct.Where(e => e.Group != null && e.Group.Id == grp_id).SingleOrDefault();
                //            Geo_list.Add(g);
                //        }
                //    }
                //    db_data = Geo_list;
                //}


                List<string> geo_id = new List<string>();
                if (db_data.Count != 0)
                {
                    foreach (var item in db_data)
                    {
                        geo_id.Add(item.Id.ToString());
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
                //bool IsJobApp = db.FuncStruct.Where(r => r.Job != null).Count() > 0 ? true : false;
                //bool IsJobPosApp = db.FuncStruct.Where(r => r.JobPosition != null).Count() > 0 ? true : false;

                List<FuncStruct> FuncStruct_List = new List<FuncStruct>();
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var db_FuncStruct = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList();

                if (job_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(job_ids);
                    FuncStruct_List = new List<FuncStruct>();
                    foreach (var item in ca_id)
                    {
                        var job_id = Convert.ToInt32(item);
                        var a = db_FuncStruct.Where(e => e.Job != null && e.Job.Id == job_id).ToList();
                        if (a.Count > 0)
                        {
                            FuncStruct_List.AddRange(a);
                        }
                    }
                    db_FuncStruct = FuncStruct_List;
                }

                if (jobposition_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobposition_ids);
                    FuncStruct_List = new List<FuncStruct>();
                    foreach (var item in ca_id)
                    {
                        var jobpos_id = Convert.ToInt32(item);
                        var a = db_FuncStruct.Where(e => e.JobPosition != null && e.JobPosition.Id == jobpos_id).ToList();
                        if (a.Count > 0)
                        {
                            FuncStruct_List.AddRange(a);
                        }
                    }
                    db_FuncStruct = FuncStruct_List;
                }
                //else if (IsJobPosApp == true)
                //{
                //    FuncStruct_List = new List<FuncStruct>();
                //    var ca_id = Utility.StringIdsToListIds(job_ids);
                //    foreach (var item in ca_id)
                //    {
                //        var job_id = Convert.ToInt32(item);
                //        List<FuncStruct> a = db_FuncStruct.Where(e => e.Job != null && e.Job.Id == job_id && e.JobPosition != null).ToList();
                //        if (a.Count > 0)
                //        {
                //            FuncStruct_List.AddRange(a);
                //        }
                //        else
                //        {
                //            FuncStruct g = db_FuncStruct.Where(e => e.Job != null && e.Job.Id == job_id).SingleOrDefault();
                //            FuncStruct_List.Add(g);
                //        }
                //    }
                //    db_FuncStruct = FuncStruct_List;
                //}

                List<string> fun_id = new List<string>();
                if (db_FuncStruct.Count != 0)
                {
                    foreach (var item in db_FuncStruct)
                    {
                        fun_id.Add(item.Id.ToString());
                    }
                    return Json(fun_id, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("FunStruct Not Found", JsonRequestBehavior.AllowGet);
                }

                ////if (job_ids != null)
                ////{
                ////    var ca_id = Utility.StringIdsToListIds(job_ids);

                ////    foreach (var item in ca_id)
                ////    {
                ////        var divison_id = Convert.ToInt32(item);
                ////        var a = db_FuncStruct.Where(e => e.Job != null && e.Job.Id == divison_id && e.JobPosition == null).ToList();
                ////        if (a.Count > 0)
                ////        {
                ////            foreach (var ca in a.Select(e => e.Id))
                ////            {
                ////                FuncStruct_List.Add(ca);
                ////            }
                ////            db_FuncStruct = a;
                ////        }
                ////    }
                ////}
                ////if (jobposition_ids != null)
                ////{
                ////    var ca_id = Utility.StringIdsToListIds(jobposition_ids);
                ////    foreach (var item in ca_id)
                ////    {
                ////        var Job_id = Convert.ToInt32(job_ids);
                ////        var divison_id = Convert.ToInt32(item);
                ////        var a = db.FuncStruct.Where(e => e.JobPosition != null && e.JobPosition.Id == divison_id && e.Job.Id == Job_id).ToList();
                ////        if (a.Count > 0)
                ////        {
                ////            FuncStruct_List.Clear();
                ////            foreach (var ca in a.Select(e => e.Id))
                ////            {
                ////                FuncStruct_List.Add(ca);
                ////            }
                ////            db_FuncStruct = a;

                ////        }
                ////    }
                ////}

                ////var comp_single_data = FuncStruct_List.Distinct().ToList();
                ////List<string> geo_id = new List<string>();
                ////if (comp_single_data.Count != 0)
                ////{
                ////    foreach (var item in comp_single_data)
                ////    {
                ////        geo_id.Add(item.ToString());
                ////    }
                ////    return Json(geo_id, JsonRequestBehavior.AllowGet);
                ////}
                ////else
                ////{
                ////    return Json("FunStruct Id Not Found", JsonRequestBehavior.AllowGet);
                ////}
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
                //bool IsGradeApp = db.PayStruct.Where(r => r.Grade != null).Count() > 0 ? true : false;
                //bool IsLevelApp = db.PayStruct.Where(r => r.Level != null).Count() > 0 ? true : false;
                //bool IsJobStatusApp = db.PayStruct.Where(r => r.JobStatus != null).Count() > 0 ? true : false;


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<PayStruct> db_PayStruct = db.PayStruct.Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.Level).AsNoTracking().ToList();
                List<PayStruct> Pay_list = new List<PayStruct>();

                if (grade_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(grade_ids);
                    List<PayStruct> a = null;
                    foreach (var item in ca_id)
                    {
                        var grade_id = Convert.ToInt32(item);
                        a = db_PayStruct.Where(e => e.Grade != null && e.Grade.Id == grade_id).ToList();
                        if (a.Count > 0)
                        {
                            Pay_list.AddRange(a);
                        }
                    }
                    db_PayStruct = Pay_list;
                }

                if (level_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(level_ids);
                    Pay_list = new List<PayStruct>();
                    foreach (var item in ca_id)
                    {
                        var level_id = Convert.ToInt32(item);
                        var a = db_PayStruct.Where(e => e.Level != null && e.Level.Id == level_id).ToList();
                        if (a.Count > 0)
                        {
                            Pay_list.AddRange(a);
                        }
                    }
                    db_PayStruct = Pay_list;
                }
                //////else if (IsLevelApp == true)
                //////{
                //////    Pay_list = new List<PayStruct>();
                //////    var ca_id = Utility.StringIdsToListIds(grade_ids);
                //////    foreach (var item in ca_id)
                //////    {
                //////        var grade_id = Convert.ToInt32(item);
                //////        List<PayStruct> a = db_PayStruct.Where(e => e.Grade != null && e.Grade.Id == grade_id && e.Level != null).ToList();
                //////        if (a.Count > 0)
                //////        {
                //////            Pay_list.AddRange(a);
                //////        }
                //////        else
                //////        {
                //////            PayStruct g = db.PayStruct.Where(e => e.Grade != null && e.Grade.Id == grade_id).SingleOrDefault();
                //////            Pay_list.Add(g);
                //////        }
                //////    }
                //////    db_PayStruct = Pay_list;

                //////    //Pay_list = new List<PayStruct>();
                //////    //var a = db_PayStruct.Where(e => e.Level != null).ToList();
                //////    //if (a.Count > 0)
                //////    //{
                //////    //    Pay_list.AddRange(a);
                //////    //}
                //////    //db_PayStruct = Pay_list;
                //////}

                if (jobstatus_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobstatus_ids);
                    Pay_list = new List<PayStruct>();
                    foreach (var item in ca_id)
                    {
                        var jobstatus_id = Convert.ToInt32(item);
                        var a = db_PayStruct.Where(e => e.JobStatus != null && e.JobStatus.Id == jobstatus_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a)
                            {
                                Pay_list.Add(ca);
                            }
                        }
                    }
                    db_PayStruct = Pay_list;
                }
                //else if (IsJobStatusApp == true)
                //{
                //    Pay_list = new List<PayStruct>();
                //    var a = db_PayStruct.Where(e => e.JobStatus != null).ToList();
                //    if (a.Count > 0)
                //    {
                //        Pay_list.AddRange(a);
                //    }
                //    db_PayStruct = Pay_list;
                //}


                List<string> pay_id = new List<string>();
                if (db_PayStruct.Count != 0)
                {
                    foreach (var item in db_PayStruct)
                    {
                        pay_id.Add(item.Id.ToString());
                    }
                    return Json(new Object[] { pay_id }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("PayStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_Geoid_h(string data, FormCollection form_data)
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

                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();
                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e =>
                        Geo_list.Contains(e.Id) &&
                        e.Location != null &&
                       ca_id.Contains(e.Location.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e =>
                        e.Location != null &&
                        ca_id.Contains(e.Location.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (department_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(department_ids);
                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();
                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e => Geo_list.Count > 0 &&
                          Geo_list.Contains(e.Id) && e.Department != null && ca_id.Contains(e.Department.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Department != null && ca_id.Contains(e.Department.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (group_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(group_ids);


                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();

                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e => Geo_list.Count > 0 &&
                            Geo_list.Contains(e.Id) && e.Group != null && ca_id.Contains(e.Group.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Group != null && ca_id.Contains(e.Group.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }

                    }
                }
                if (unit_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(unit_ids);


                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();


                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e => Geo_list.Count > 0 && Geo_list.Contains(e.Id) &&
                            e.Unit != null && ca_id.Contains(e.Unit.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Unit != null && ca_id.Contains(e.Unit.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
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
        public ActionResult Get_Funid_h(string data, FormCollection form_data)
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
                    List<FuncStruct> oFuncStruct = new List<FuncStruct>();
                    if (FuncStruct_List.Count > 0)
                    {
                        oFuncStruct = db.FuncStruct.Where(e => FuncStruct_List.Count > 0 && FuncStruct_List.Contains(e.Id)
                            && e.JobPosition != null && ca_id.Contains(e.JobPosition.Id)).ToList();
                        if (oFuncStruct.Count > 0)
                        {
                            FuncStruct_List = new List<int>();
                            foreach (var ca in oFuncStruct.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oFuncStruct = db.FuncStruct.Where(e => e.JobPosition != null && ca_id.Contains(e.JobPosition.Id)).ToList();
                    }
                    if (oFuncStruct.Count > 0)
                    {
                        foreach (var ca in oFuncStruct.Select(e => e.Id))
                        {
                            FuncStruct_List.Add(ca);
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
        public ActionResult Get_Payid_h(string data, FormCollection form_data)
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

                    //  var divison_id = Convert.ToInt32(item);
                    var a = db.PayStruct.Where(e => e.Grade != null && ca_id.Contains(e.Grade.Id)).ToList();
                    if (a.Count > 0)
                    {
                        foreach (var ca in a.Select(e => e.Id))
                        {
                            PayStruct_List.Add(ca);
                        }
                    }
                }
                if (level_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(level_ids);

                    //   var divison_id = Convert.ToInt32(item);
                    List<PayStruct> oPayStruct = new List<PayStruct>();
                    if (PayStruct_List.Count > 0)
                    {
                        oPayStruct = db.PayStruct.Where(e => PayStruct_List.Count > 0 && PayStruct_List.Contains(e.Id) &&
                            e.Level != null && ca_id.Contains(e.Level.Id
                            )).ToList();
                        if (oPayStruct.Count > 0)
                        {
                            PayStruct_List = new List<int>();
                            foreach (var ca in oPayStruct.Select(e => e.Id))
                            {
                                PayStruct_List.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oPayStruct = db.PayStruct.Where(e => e.Level != null && ca_id.Contains(e.Level.Id
                            )).ToList();

                    }
                    if (oPayStruct.Count > 0)
                    {
                        foreach (var ca in oPayStruct.Select(e => e.Id))
                        {
                            PayStruct_List.Add(ca);
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

        public ActionResult Get_Geoid_Single(string data, FormCollection form_data)
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

                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();
                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e =>
                        Geo_list.Contains(e.Id) &&
                        e.Location != null &&
                        e.Division != null &&
                        e.Department == null &&
                       ca_id.Contains(e.Location.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e =>
                        e.Location != null &&
                        ca_id.Contains(e.Location.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (department_ids != null)
                {
                    var ca_id1 = Utility.StringIdsToListIds(location_ids);
                    var ca_id = Utility.StringIdsToListIds(department_ids);
                    var divid = division_ids == null ? "0" : division_ids;
                    //   var ca_id2 = Utility.StringIdsToListIds(division_ids);
                    var ca_id2 = Utility.StringIdsToListIds(divid);
                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();
                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Location != null && ca_id1.Contains(e.Location.Id) && e.Division != null && ca_id2.Contains(e.Division.Id)
                           && e.Department != null && ca_id.Contains(e.Department.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Department != null && ca_id.Contains(e.Department.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                }

                if (group_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(group_ids);


                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();

                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e => Geo_list.Count > 0 &&
                            Geo_list.Contains(e.Id) && e.Group != null && ca_id.Contains(e.Group.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Group != null && ca_id.Contains(e.Group.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }

                    }
                }
                if (unit_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(unit_ids);


                    List<GeoStruct> oGeoStruct = new List<GeoStruct>();


                    if (Geo_list.Count > 0)
                    {
                        oGeoStruct = db.GeoStruct.Where(e => Geo_list.Count > 0 && Geo_list.Contains(e.Id) &&
                            e.Unit != null && ca_id.Contains(e.Unit.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            Geo_list = new List<int>();
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
                            {
                                Geo_list.Add(ca);
                            }
                        }
                    }
                    else
                    {
                        oGeoStruct = db.GeoStruct.Where(e => e.Unit != null && ca_id.Contains(e.Unit.Id)).ToList();
                        if (oGeoStruct.Count > 0)
                        {
                            foreach (var ca in oGeoStruct.Select(e => e.Id))
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

        public ActionResult Get_Funid_Single(string data, FormCollection form_data)
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
                var db_FuncStruct = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList();
                if (job_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(job_ids);

                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db_FuncStruct.Where(e => e.Job != null && e.Job.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                            db_FuncStruct = a;
                        }
                    }
                }
                if (jobposition_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobposition_ids);
                    foreach (var item in ca_id)
                    {
                        var divison_id = Convert.ToInt32(item);
                        var a = db_FuncStruct.Where(e => e.JobPosition != null && e.JobPosition.Id == divison_id).ToList();
                        if (a.Count > 0)
                        {
                            FuncStruct_List = new List<int>();
                            foreach (var ca in a.Select(e => e.Id))
                            {
                                FuncStruct_List.Add(ca);
                            }
                            db_FuncStruct = a;

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

        public ActionResult Get_Payid_Single(string data, FormCollection form_data)
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

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<PayStruct> db_PayStruct = db.PayStruct.Include(e => e.Grade).Include(e => e.JobStatus).Include(e => e.Level).AsNoTracking().ToList();
                List<PayStruct> Pay_list = new List<PayStruct>();

                if (grade_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(grade_ids);
                    List<PayStruct> a = null;
                    foreach (var item in ca_id)
                    {
                        var grade_id = Convert.ToInt32(item);
                        a = db_PayStruct.Where(e => e.Grade != null && e.Grade.Id == grade_id).ToList();
                        if (a.Count > 0)
                        {
                            Pay_list.AddRange(a);
                        }
                    }
                    db_PayStruct = Pay_list;
                }

                if (level_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(level_ids);
                    Pay_list = new List<PayStruct>();
                    foreach (var item in ca_id)
                    {
                        var level_id = Convert.ToInt32(item);
                        var a = db_PayStruct.Where(e => e.Level != null && e.Level.Id == level_id).FirstOrDefault();
                        //if (a.Count > 0)
                        //{
                        Pay_list.Add(a);
                        //}
                    }
                    db_PayStruct = Pay_list;
                }


                if (jobstatus_ids != null)
                {
                    var ca_id = Utility.StringIdsToListIds(jobstatus_ids);
                    Pay_list = new List<PayStruct>();
                    foreach (var item in ca_id)
                    {
                        var jobstatus_id = Convert.ToInt32(item);
                        var a = db_PayStruct.Where(e => e.JobStatus != null && e.JobStatus.Id == jobstatus_id).ToList();
                        if (a.Count > 0)
                        {
                            foreach (var ca in a)
                            {
                                Pay_list.Add(ca);
                            }
                        }
                    }
                    db_PayStruct = Pay_list;
                }

                List<string> pay_id = new List<string>();
                if (db_PayStruct != null && db_PayStruct.Count() > 0)
                {
                    foreach (var item in db_PayStruct)
                    {
                        pay_id.Add(item.Id.ToString());
                    }
                    return Json(new Object[] { pay_id }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("PayStruct Id Not Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_EmployelistPFT(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                ////if (deserialize.Filter != "" && deserialize.Filter != null)
                ////{
                ////    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}
                ////else
                ////{
                ////    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    monthyr = deserialize.Filter;
                }
                else
                {
                    monthyr = DateTime.Now.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpOffInfo))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).OrderBy(e => e.Id)
                    .AsNoTracking().AsParallel()
                    .SingleOrDefault();

                //if (empdata != null)
                //{
                //    empdata.EmployeePayroll = db.EmployeePayroll.Where(e => e.CompanyPayroll_Id == empdata.Id).ToList();
                //    foreach (var item in empdata.EmployeePayroll)
                //    {
                //        item.Employee = db.Employee.Find(item.Employee_Id); 
                //        item.Employee.EmpName = db.NameSingle.Find(item.Employee.EmpName_Id);
                //        item.Employee.ServiceBookDates = db.ServiceBookDates.Find(item.Employee.ServiceBookDates_Id);
                //    }
                //}
                List<string> ESTBID = new List<string>();
                List<string> Notsettleempcode = new List<string>();
                var exempted = db.PFMaster.Include(e => e.PFTrustType).Where(x => x.PFTrustType.LookupVal.ToUpper() == "EXEMPTED" && x.EndDate == null).ToList();

                foreach (var pfmasterid in exempted)
                {
                    var pftrustmapid = db.PFMasterPFT.Where(e => e.PFMaster_Id == pfmasterid.Id).SingleOrDefault();
                    if (pftrustmapid != null)
                    {
                        ESTBID.Add(pfmasterid.EstablishmentID);
                    }
                }


                var Notsettleemprec = db.EmployeePFTrust.Select(d => new
                {
                    EmployeePFTrustId = d.Id,
                    oempcode = d.Employee.EmpCode,
                    OServiceLastDate = d.Employee.ServiceBookDates.ServiceLastDate,
                    OPFTEmployeeLedger = d.PFTEmployeeLedger.Select(p => new
                    {
                        OPassbookActivity = p.PassbookActivity.LookupVal,
                        OEmployeePFTrustId = p.EmployeePFTrust_Id
                    }).FirstOrDefault(),

                }).Where(x => x.OServiceLastDate != null && x.EmployeePFTrustId == x.OPFTEmployeeLedger.OEmployeePFTrustId).ToList();
                if (Notsettleemprec.Count() > 0)
                {
                    foreach (var NTSETTLE in Notsettleemprec)
                    {
                        var PFTEmpLedgerOldsett = db.PFTEmployeeLedger.Include(e => e.PassbookActivity).Where(e => e.EmployeePFTrust_Id == NTSETTLE.EmployeePFTrustId && e.PassbookActivity.LookupVal.ToUpper() == "SETTLEMENT BALANCE").AsParallel().SingleOrDefault();
                        if (PFTEmpLedgerOldsett == null)
                        {
                            Notsettleempcode.Add(NTSETTLE.oempcode);
                        }
                    }
                }
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct_Id != null && e.Employee.GeoStruct_Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).Select(e => e.Employee).AsParallel().ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {


                                if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                                {
                                    data.Add(item);
                                }




                            }
                        }
                        // Not settle empcode
                        var empnotsettle = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct_Id != null && e.Employee.GeoStruct_Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId) && Notsettleempcode.Contains(e.Employee.EmpCode)).Select(e => e.Employee).AsParallel().ToList();
                        if (empnotsettle != null && empnotsettle.Count != 0)
                        {
                            foreach (var item in empnotsettle)
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
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct_Id != null && e.Employee.PayStruct_Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).Select(e => e.Employee).AsParallel().ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {

                                if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                                {
                                    data.Add(item);
                                }


                            }
                        }
                        // Not settle empcode
                        var empnotsettle = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct_Id != null && e.Employee.PayStruct_Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId) && Notsettleempcode.Contains(e.Employee.EmpCode)).Select(e => e.Employee).AsParallel().ToList();
                        if (empnotsettle != null && empnotsettle.Count != 0)
                        {
                            foreach (var item in empnotsettle)
                            {
                                data.Add(item);
                            }
                        }
                    }
                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct_Id != null && e.Employee.FuncStruct_Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).Select(e => e.Employee).AsParallel().ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {

                                if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                                {
                                    data.Add(item);
                                }

                            }
                        }
                        // Not settle empcode
                        var empnotsettle = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct_Id != null && e.Employee.FuncStruct_Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId) && Notsettleempcode.Contains(e.Employee.EmpCode)).Select(e => e.Employee).AsParallel().ToList();
                        if (empnotsettle != null && empnotsettle.Count != 0)
                        {
                            foreach (var item in empnotsettle)
                            {
                                data.Add(item);
                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode).AsParallel())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else if (item.ServiceBookDates.ServiceLastDate != null && Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) < Convert.ToDateTime("01/" + monthyr))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        //if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                        //{
                        //    returndata.Add(new Utility.returndataclass
                        //    {
                        //        code = item.Id.ToString(),
                        //        value = item.FullDetails,
                        //    });
                        //}
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else if (empdata.EmployeePayroll.Where(e => e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).Select(e => e.Employee).AsParallel().ToList().Count > 0)
                {
                    foreach (var item in empdata.EmployeePayroll.Where(e => e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).Select(e => e.Employee).OrderBy(x => x.EmpCode).AsParallel().Distinct())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else if (item.ServiceBookDates.ServiceLastDate != null && Notsettleempcode.Contains(item.EmpCode))
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Get_EmployeeListLogin(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                List<Employee> data = new List<Employee>();

                var empdata = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.GeoStruct)
                    .Include(e => e.PayStruct)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.ServiceBookDates)
                    .Include(e => e.Login)
                    .Where(e => e.Login.Id == null).ToList();

                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.Where(e => e.GeoStruct_Id != null && e.GeoStruct_Id == id).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
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
                        var emp = empdata.Where(e => e.PayStruct_Id != null && e.PayStruct_Id == id).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }
                            }
                        }

                    }
                }

                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.Where(e => e.FuncStruct_Id != null && e.FuncStruct_Id == id).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null)
                                {
                                    data.Add(item);
                                }

                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count() > 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode))
                    {

                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Id.ToString(),
                            value = item.FullDetails,
                        });

                    }

                    if (returndata.Count() > 0)
                    {
                        var returnjson = new
                        {
                            data = returndata,
                            tablename = "Employee-Table"
                        };

                        return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return JavaScript("alert('There is not any employee for login assign..!!')");
                    }


                }

                else if (empdata.Count > 0)
                {
                    foreach (var item in empdata.Distinct().OrderBy(x => x.EmpCode))
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

                    }
                    if (returndata.Count() > 0)
                    {
                        var returnjson = new
                        {
                            data = returndata,
                            tablename = "Employee-Table"
                        };
                        return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return JavaScript("alert('There is not any employee for login assign..!!')");
                    }

                }
            }
            return null;
        }
        public ActionResult Get_Employelist(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                ////if (deserialize.Filter != "" && deserialize.Filter != null)
                ////{
                ////    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}
                ////else
                ////{
                ////    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    monthyr = deserialize.Filter;
                }
                else
                {
                    monthyr = DateTime.Now.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).OrderBy(e => e.Id)
                    .AsNoTracking().AsParallel()
                    .SingleOrDefault();

                //if (empdata != null)
                //{
                //    empdata.EmployeePayroll = db.EmployeePayroll.Where(e => e.CompanyPayroll_Id == empdata.Id).ToList();
                //    foreach (var item in empdata.EmployeePayroll)
                //    {
                //        item.Employee = db.Employee.Find(item.Employee_Id); 
                //        item.Employee.EmpName = db.NameSingle.Find(item.Employee.EmpName_Id);
                //        item.Employee.ServiceBookDates = db.ServiceBookDates.Find(item.Employee.ServiceBookDates_Id);
                //    }
                //}

                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct_Id != null && e.Employee.GeoStruct_Id == id).Select(e => e.Employee).AsParallel().ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                                {
                                    data.Add(item);
                                }
                                //if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                                //{
                                //    data.Add(item);
                                //}


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
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct_Id != null && e.Employee.PayStruct_Id == id).Select(e => e.Employee).AsParallel().ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                                {
                                    data.Add(item);
                                }
                                //if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                                //{
                                //    data.Add(item);
                                //}

                            }
                        }
                    }
                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct_Id != null && e.Employee.FuncStruct_Id == id).Select(e => e.Employee).AsParallel().ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                                {
                                    data.Add(item);
                                }
                                //if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                                //{
                                //    data.Add(item);
                                //}
                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode).AsParallel())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        //if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                        //{
                        //    returndata.Add(new Utility.returndataclass
                        //    {
                        //        code = item.Id.ToString(),
                        //        value = item.FullDetails,
                        //    });
                        //}
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else if (empdata.EmployeePayroll.Select(e => e.Employee).AsParallel().ToList().Count > 0)
                {
                    foreach (var item in empdata.EmployeePayroll.Select(e => e.Employee).AsParallel().Distinct())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GetLocationList(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                ////if (deserialize.Filter != "" && deserialize.Filter != null)
                ////{
                ////    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}
                ////else
                ////{
                ////    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                ////    monthyr = dt.Value.ToString("MM/yyyy");
                ////    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                ////}

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    monthyr = deserialize.Filter;
                }
                else
                {
                    monthyr = DateTime.Now.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.Company.Where(e => e.Id == compid)
                    .Include(e => e.Location)
                    .Include(e => e.Location.Select(t => t.LocationObj))
                    .AsNoTracking()
                    .SingleOrDefault();

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (empdata != null)
                {
                    foreach (var item in empdata.Location)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Id.ToString(),
                            value = item.FullDetails,
                        });

                        //if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
                        //{
                        //    returndata.Add(new Utility.returndataclass
                        //    {
                        //        code = item.Id.ToString(),
                        //        value = item.FullDetails,
                        //    });
                        //}
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }



        public ActionResult GeoLocationListWoHo(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    monthyr = deserialize.Filter;
                }
                else
                {
                    monthyr = DateTime.Now.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.Company.Where(e => e.Id == compid)
                    .Include(e => e.Location)
                    .Include(e => e.Location.Select(t => t.LocationObj))
                    .AsNoTracking()
                    .SingleOrDefault();

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (empdata != null)
                {
                    foreach (var item in empdata.Location)
                    {
                        if (!String.IsNullOrEmpty(deserialize.GeoStruct))   // GEOSTRUCT Filter Code
                        {
                            string[] FilterGeoIds = deserialize.GeoStruct.Split(',');
                            if (FilterGeoIds.Count() > 0)
                            {
                                foreach (var tempGeoid in FilterGeoIds)
                                {
                                    List<int?> LocationIdList = new List<int?>();
                                    int Geoid = Convert.ToInt32(tempGeoid);
                                    var getLocationId = db.GeoStruct.Include(e => e.Location).Where(e => e.Id == Geoid).SingleOrDefault();
                                    if (getLocationId.Location != null)
                                    {
                                        LocationIdList.Add(getLocationId.Location.Id);
                                    }


                                    if (LocationIdList.Contains(item.Id))
                                    {
                                        returndata.Add(new Utility.returndataclass
                                        {
                                            code = item.Id.ToString(),
                                            value = item.FullDetails,
                                        });
                                    }


                                }
                            }
                        }
                        else
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }


                    }
                    var result = returndata.GroupBy(x => x.value).Select(y => y.First()).ToList();
                    var returnjson = new
                    {
                        data = result,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Get_Employelist_h_PFT(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpOffInfo))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().SingleOrDefault();
                List<EmployeePayroll> List_all = new List<EmployeePayroll>();
                List<EmployeePayroll> All_Emp = new List<EmployeePayroll>();
                List<string> ESTBID = new List<string>();
                var exempted = db.PFMaster.Include(e => e.PFTrustType).Where(x => x.PFTrustType.LookupVal.ToUpper() == "EXEMPTED" && x.EndDate == null).ToList();

                foreach (var pfmasterid in exempted)
                {
                    var pftrustmapid = db.PFMasterPFT.Where(e => e.PFMaster_Id == pfmasterid.Id).SingleOrDefault();
                    if (pftrustmapid != null)
                    {
                        ESTBID.Add(pfmasterid.EstablishmentID);
                    }
                }
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all.AddRange(List_all_temp);
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id) && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id) && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).ToList();
                    }
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id) && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id) && e.Employee.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.Employee.EmpOffInfo.PFTrust_EstablishmentId)).ToList();
                    }
                    //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.Select(e => e.Employee).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = db.Employee.Include(e => e.EmpOffInfo).Include(e => e.ServiceBookDates).Include(e => e.EmpName).Where(e => e.EmpOffInfo.PFAppl == true && ESTBID.Contains(e.EmpOffInfo.PFTrust_EstablishmentId)).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Get_Employelist_h(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().SingleOrDefault();
                List<EmployeePayroll> List_all = new List<EmployeePayroll>();
                List<EmployeePayroll> All_Emp = new List<EmployeePayroll>();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all.AddRange(List_all_temp);
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.Select(e => e.Employee).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.EmpName).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
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
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).AsNoTracking().AsParallel()
                    .SingleOrDefault();
                // .Where(e => e.Company.Id == compid).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();

                // empdata.EmployeePayroll.OrderBy(a => a.Employee.EmpCode).ToList();
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
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
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
                    foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                            item.ServiceBookDates.ServiceLastDate.Value > dtChk))
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Get_Emplist_h(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize.Filter != "" && deserialize.Filter != null)
                {
                    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                else
                {
                    dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                    monthyr = dt.Value.ToString("MM/yyyy");
                    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                }
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().SingleOrDefault();
                List<EmployeePayroll> List_all = new List<EmployeePayroll>();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all.AddRange(List_all_temp);
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.Select(e => e.Employee).ToList();

                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };

                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.EmpName).ToList();
                    if (deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
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
                    var geo_id = db.GeoStruct.Where(e => e.Company != null && e.Company.Id == id).OrderBy(e => e.Id).AsNoTracking().Distinct().ToList();
                    if (geo_id.Count > 0)
                    {
                        foreach (var ca in geo_id.Select(e => e.Id.ToString()))
                        {
                            GeoStruct.Add(ca);
                        }
                    }

                    var pay_id = db.PayStruct.Where(e => e.Company != null && e.Company.Id == id).OrderBy(e => e.Id).AsNoTracking().Distinct().ToList();
                    if (pay_id.Count > 0)
                    {
                        foreach (var ca in pay_id.Select(e => e.Id.ToString()))
                        {
                            PayStruct.Add(ca);
                        }
                    }

                    var fun_id = db.FuncStruct.Where(e => e.Company != null && e.Company.Id == id).OrderBy(e => e.Id).AsNoTracking().Distinct().ToList();
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

        public ActionResult Get_Candidatelist(string databatch, string session, string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;


                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                List<int> evalparaidlist = null;
                List<ResumeCollection> empdata = null;
                List<Candidate> Emp = new List<Candidate>();
                empdata = db.ResumeCollection
                       .Include(e => e.Candidate)
                       .Include(e => e.ResumeSortlistingStatus)
                       .Include(e => e.Candidate.CanName)
                       .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.RecruitEvaluationPara))
                       .Include(e => e.RecruitEvaluationProcessResult.Select(t => t.ActivityResult))
                       .Include(e => e.ShortlistingCriteria)
                       .ToList();

                //if (databatch != null)
                //{
                //    foreach (var item in empdata)
                //    {
                //        Emp.Add(item.Candidate);
                //    }
                //}


                foreach (var item in empdata)
                {

                    if (item != null)
                    {
                        Emp.Add(item.Candidate);
                    }

                }


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (Emp != null && Emp.Count != 0)
                {
                    foreach (var item in Emp)
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
                        tablename = "Candidate_table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }


        public ActionResult Get_Victimlist(string databatch, string session, string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;


                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                List<int> evalparaidlist = null;
                List<EmployeeIR> Victimdata = new List<EmployeeIR>();
                // List<EmpDisciplineProcedings> Emp = new List<EmpDisciplineProcedings>();

                //var OOEmployee = db.Employee.FirstOrDefault();

                //    var DbIRids = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings).Include(e => e.Employee)
                //                      .Where(e => e.Employee.Id == OOEmployee.Id).ToList();

                //    var EmpIrs = db.EmployeeIR.FirstOrDefault();
                //    var empdispli = db.EmpDisciplineProcedings.ToList();

                var all = db.EmployeeIR.Include(e => e.EmpDisciplineProcedings).Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id != null).ToList();
                foreach (var itemIR in all)
                {
                    if (itemIR != null)
                    {
                        var CheckEmpdiscipline = itemIR.EmpDisciplineProcedings.Count() > 0;
                        if (CheckEmpdiscipline == true)
                        {
                            EmployeeIR IrObj = new EmployeeIR()
                              {
                                  Id = itemIR.Id,
                                  EmpDisciplineProcedings = itemIR.EmpDisciplineProcedings,
                                  Employee = itemIR.Employee

                              };
                            Victimdata.Add(IrObj);
                        }
                    }
                }







                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (Victimdata != null && Victimdata.Count != 0)
                {
                    foreach (var item in Victimdata)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Employee.Id.ToString(),
                            value = item.Employee.FullDetails,
                        });
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Victim_table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<string> Msg = new List<string>();
                    Msg.Add("  No Victim Found.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }

                return null;

            }
        }

        public ActionResult Get_EmployelistWOGeoId(string Filter)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string monthyr = "";
                DateTime? dtChk = null;

                if (Filter != "" && Filter != null)
                {
                    monthyr = Filter;
                }
                else
                {
                    monthyr = DateTime.Now.ToString("MM/yyyy");
                }

                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    //.Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates)).OrderBy(e => e.Id)
                    .AsNoTracking().AsParallel()
                    .SingleOrDefault();

                //if (empdata != null)
                //{
                //    empdata.EmployeePayroll = db.EmployeePayroll.Where(e => e.CompanyPayroll_Id == empdata.Id).ToList();
                //    foreach (var item in empdata.EmployeePayroll)
                //    {
                //        item.Employee = db.Employee.Find(item.Employee_Id); 
                //        item.Employee.EmpName = db.NameSingle.Find(item.Employee.EmpName_Id);
                //        item.Employee.ServiceBookDates = db.ServiceBookDates.Find(item.Employee.ServiceBookDates_Id);
                //    }
                //}


                //var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                //foreach (var ca in one_id)
                //{

                var emp = empdata.EmployeePayroll.Where(e => e.Employee.GeoStruct_Id != null && (e.Employee.ServiceBookDates.ServiceLastDate == null || Convert.ToDateTime("01/" + e.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + monthyr))).Select(e => e.Employee).AsParallel().ToList();
                if (emp != null && emp.Count != 0)
                {
                    data.AddRange(emp);
                }


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode).AsParallel())
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else if (empdata.EmployeePayroll.Select(e => e.Employee).AsParallel().ToList().Count > 0)
                {
                    foreach (var item in empdata.EmployeePayroll.Select(e => e.Employee).AsParallel().Distinct())
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else if (item.ServiceBookDates.ServiceLastDate != null && item.ServiceBookDates.ServiceLastDate.Value >= dtChk)
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult Get_Employelist_PayBank(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;
                //var Serialize = new JavaScriptSerializer();
                //var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                //if (deserialize.Filter != "" && deserialize.Filter != null)
                //{
                //    dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                //    monthyr = dt.Value.ToString("MM/yyyy");
                //    dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                //}
                //else
                //{
                dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                monthyr = dt.Value.ToString("MM/yyyy");
                dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                //}
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpOffInfo))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpOffInfo.Bank))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().SingleOrDefault();
                List<EmployeePayroll> List_all = new List<EmployeePayroll>();
                List<EmployeePayroll> All_Emp = new List<EmployeePayroll>();
                if (geo_id != null)
                {
                    var one_id = Utility.StringIdsToListIds(geo_id);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var List_all_temp = empdata.EmployeePayroll.Where(e => e.Employee.EmpOffInfo != null && e.Employee.EmpOffInfo.Bank != null && e.Employee.EmpOffInfo.Bank.Id == id).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all.AddRange(List_all_temp);
                        }
                    }
                }


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all.Count() > 0)
                {

                    foreach (var item in List_all.Distinct().OrderBy(a => a.Employee.EmpCode))
                    {
                        if (item.Employee.ServiceBookDates.ServiceLastDate == null || (item.Employee.ServiceBookDates.ServiceLastDate != null &&
                                item.Employee.ServiceBookDates.ServiceLastDate.Value > dtChk))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Employee.Id.ToString(),
                                value = item.Employee.FullDetails,
                            });
                        }
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult Get_Employeelist_ESSAtt(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                monthyr = dt.Value.ToString("MM/yyyy");
                dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                
                var empdata = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location).Include(e => e.GeoStruct.Location.GeoFencingParameter)
                    .Include(e => e.ServiceBookDates).Include(e => e.EmpName)
                    .Where(e => e.GeoStruct != null && e.GeoStruct.Location != null && e.GeoStruct.Location.GeoFencingParameter != null && e.GeoStruct.Location.GeoFencingParameter.ESSAttAppl == true)
                    .AsNoTracking().AsParallel().ToList();


             

                List<Employee> List_all = new List<Employee>();
                List<Employee> All_Emp = new List<Employee>();
                if (deserialize != null)
                {
                    if (deserialize.GeoStruct != null)
                    {
                        var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                        foreach (var ca in one_id)
                        {
                            var id = Convert.ToInt32(ca);
                            var List_all_temp = empdata.Where(e => e.GeoStruct != null && e.GeoStruct.Id == id).ToList();
                            if (List_all_temp != null && List_all_temp.Count != 0)
                            {
                                List_all.AddRange(List_all_temp);
                            }
                        }
                    }
                    if (deserialize.PayStruct != null)
                    {
                        var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                        var List_all_temp = new List<Employee>();
                        if (List_all.Count > 0)
                        {
                            List_all_temp = List_all.Where(e => e.PayStruct != null && one_id.Contains(e.PayStruct.Id)).ToList();
                        }
                        else
                        {
                            List_all_temp = empdata.Where(e => e.PayStruct != null && one_id.Contains(e.PayStruct.Id)).ToList();
                        }
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all = List_all_temp;
                        }

                    }
                    if (deserialize.FunStruct != null)
                    {
                        var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                        var List_all_temp = new List<Employee>();
                        if (List_all.Count > 0)
                        {
                            List_all_temp = List_all.Where(e => e.FuncStruct != null && one_id.Contains(e.FuncStruct.Id)).ToList();
                        }
                        else
                        {
                            List_all_temp = empdata.Where(e => e.FuncStruct != null && one_id.Contains(e.FuncStruct.Id)).ToList();
                        }
                        //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all = List_all_temp;
                        }

                    }
                }
               


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.ToList();
                    if (deserialize != null && deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = empdata;
                    if (deserialize != null && deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }

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
                           value = e.Grade.Code + " " + e.Grade.Name.ToString() + " " + e.Level.Name.ToString()
                       }).ToList();

                }
                var fun_data = (Object)null;
                if (emp_data.FuncStruct != null)
                {

                    fun_data = db.FuncStruct.Where(e => e.Id == emp_data.FuncStruct.Id).Select(e => new
                    {
                        code = e.Id.ToString(),
                        value = e.Job.Code + " " + e.Job.Name.ToString() + "" + e.JobPosition.JobPositionDesc.ToString(),

                    }).ToList();
                }
                var geo_data = (Object)null;
                if (emp_data.GeoStruct != null)
                {

                    geo_data = db.GeoStruct.Where(e => e.Id == emp_data.GeoStruct.Id).Select(e => new
                    {
                        Code = e.Id.ToString(),
                        value = e.Location.LocationObj.LocCode.ToString() + " " + e.Location.LocationObj.LocDesc.ToString() + " " + e.Department.DepartmentObj.DeptDesc.ToString()
                    }).ToList();
                }
                //var geo_data = db.GeoStruct.Where(e=>e.Id==emp_data.GeoStruct.Id)
                return Json(new Object[] { fun_data, pay_data, geo_data }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

    }
}