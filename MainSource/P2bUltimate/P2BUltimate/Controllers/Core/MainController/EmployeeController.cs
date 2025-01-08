using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Threading.Tasks;
using System.Collections;
using P2BUltimate.Security;
using Payroll;
using Leave;
using Attendance;
using Recruitment;
using Training;
using Appraisal;
using EMS;
using System.Data.SqlClient;
using System.Configuration;
using IR;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class EmployeeController : Controller
    {

        public void sessions(int id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Employee.Where(x => x.Id == id).Select(i => new { EmpName = i.EmpName.FName, EmpCode = i.EmpCode }).SingleOrDefault();
                Session["EmpName"] = a;
            }
        }
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/Employee/Index.cshtml");
        }
        public ActionResult _EmpOfficial()
        {
            return View("~/Views/Shared/Core/_EmpOfficial.cshtml");
        }
        public List<Corporate> Get_Corporate(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Corporate> Corporate_selectlist = new List<Corporate>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Corporate).Where(e => e.Corporate != null).Select(e => e.Corporate.Id).Distinct().ToList();

                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Corporate.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Region> Get_Region(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Region> Corporate_selectlist = new List<Region>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Region).Where(e => e.Region != null).Select(e => e.Region.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Region.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Region).Where(e => (e.Corporate.Id == Corp_id && e.Region != null)).Select(e => e.Region.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Region.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Company> Get_Company(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Company> Corporate_selectlist = new List<Company>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Company).Where(e => e.Company != null).Select(e => e.Company.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Company.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Company)
                        .Where(e => (e.Corporate.Id == Corp_id && e.Company != null) || (e.Region.Id == Corp_id && e.Company != null))
                        .Select(e => e.Company.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Company.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Division> Get_Division(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Division> Corporate_selectlist = new List<Division>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Division).Where(e => e.Division != null).Select(e => e.Division.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Division.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);

                    var region_data = db.GeoStruct.Include(e => e.Division).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Division != null) ||
                        (e.Region.Id == Corp_id && e.Division != null) ||
                        (e.Company.Id == Corp_id && e.Division != null))
                        .Select(e => e.Division.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Division.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Location> Get_Location(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Location> Corporate_selectlist = new List<Location>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Location).Where(e => e.Location != null).Select(e => e.Location.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var ids = Utility.StringIdsToListString(data);
                    var region_data = new List<int>();
                    var Corp_id = Convert.ToInt32(ids[0]);
                    if (ids.Count > 1)
                    {
                        var skip = ids[1].ToString().ToUpper();
                        if (skip == "DIVISION")
                        {
                            region_data = db.GeoStruct.Include(e => e.Location).Where(e =>
                            e.Company.Id == Corp_id && e.Division == null && e.Location != null
                            ).Select(e => e.Location.Id).Distinct().ToList();
                        }
                    }
                    else
                    {
                        region_data = db.GeoStruct.Include(e => e.Location).Where(e =>
                          (e.Corporate.Id == Corp_id && e.Location != null) ||
                          (e.Region.Id == Corp_id && e.Location != null) ||
                          (e.Company.Id == Corp_id && e.Division == null && e.Location != null) ||
                          (e.Division.Id == Corp_id && e.Location != null))
                          .Select(e => e.Location.Id).Distinct().ToList();
                    }
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Location.Include(e => e.LocationObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Department> Get_Department(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Department> Corporate_selectlist = new List<Department>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Department).Where(e => e.Department != null).Select(e => e.Department.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Department).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Department != null) ||
                        (e.Region.Id == Corp_id && e.Department != null) ||
                        (e.Company.Id == Corp_id && e.Department != null) ||
                        (e.Division.Id == Corp_id && e.Department != null) ||
                        (e.Location.Id == Corp_id && e.Department != null))
                        .Select(e => e.Department.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Department.Include(e => e.DepartmentObj).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Group> Get_Group(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Group> Corporate_selectlist = new List<Group>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Group).Where(e => e.Group != null).Select(e => e.Group.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Group.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Group).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Group != null) ||
                        (e.Region.Id == Corp_id && e.Group != null) ||
                        (e.Company.Id == Corp_id && e.Group != null) ||
                        (e.Division.Id == Corp_id && e.Group != null) ||
                        (e.Location.Id == Corp_id && e.Group != null) ||
                        (e.Department.Id == Corp_id && e.Group != null))
                        .Select(e => e.Group.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Group.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public List<Unit> Get_Unit(String data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Unit> Corporate_selectlist = new List<Unit>();
                if (data == null)
                {
                    var Corporate_data = db.GeoStruct.Include(e => e.Unit).Where(e => e.Unit != null).Select(e => e.Unit.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Unit.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Unit).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Unit != null) ||
                        (e.Region.Id == Corp_id && e.Unit != null) ||
                        (e.Company.Id == Corp_id && e.Unit != null) ||
                        (e.Division.Id == Corp_id && e.Unit != null) ||
                        (e.Location.Id == Corp_id && e.Unit != null) ||
                        (e.Department.Id == Corp_id && e.Unit != null) ||
                        (e.Group.Id == Corp_id && e.Unit != null))
                        .Select(e => e.Unit.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Unit.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public Object ExitanceFirstlevelGeo()
        {
            var data = (String)null;
            var a = Get_Corporate(data);
            if (a != null)
            {
                var selectlistjson = new
                {
                    SelectlistType = "CorporateList_DDL",
                    selectlist = new SelectList(a, "Id", "Name", "")
                };
                return selectlistjson;
            }
            else
            {
                var b = Get_Region(data);
                if (b != null)
                {
                    var selectlistjson = new
                    {
                        SelectlistType = "RegionList_DDL",
                        selectlist = new SelectList(b, "Id", "Name", "")
                    };
                    return selectlistjson;
                }
                else
                {
                    var c = Get_Company(data);
                    if (c != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "CompanyList_DDL",
                            selectlist = new SelectList(c, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var d = Get_Division(data);
                        if (d != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "DivisionList_DDL",
                                selectlist = new SelectList(d, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var e = Get_Location(data);
                            if (e != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "LocationList_DDL",
                                    selectlist = new SelectList(e, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var f = Get_Department(data);
                                if (f != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "DepartmentList_DDL",
                                        selectlist = new SelectList(f, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var g = Get_Group(data);
                                    if (g != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "GroupList_DDL",
                                            selectlist = new SelectList(g, "Id", "Name", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var h = Get_Unit(data);
                                        if (f != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "UnitList_DDL",
                                                selectlist = new SelectList(h, "Id", "Name", "")
                                            };
                                            return selectlistjson;
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
                }
            }
        }
        public Object ExitanceFirstlevelPay()
        {
            var data = (string)null;
            var a = Get_Grade(data);
            if (a != null)
            {
                var selectlistjson = new
                {
                    SelectlistType = "Grade_drop",
                    selectlist = new SelectList(a, "Id", "FullDetails", "")
                };
                return selectlistjson;
            }
            else
            {
                var b = Get_level(data);
                if (b != null)
                {
                    var selectlistjson = new
                    {
                        SelectlistType = "Level_drop",
                        selectlist = new SelectList(a, "Id", "FullDetails", "")
                    };
                    return selectlistjson;
                }
            }
            var c = Get_JobStatus(data);
            if (c != null)
            {
                var selectlistjson = new
                {
                    SelectlistType = "JobStatus_drop",
                    selectlist = new SelectList(a, "Id", "FullDetails", "")
                };
                return selectlistjson;
            }
            else
            {
                return null;
            }
        }
        private List<JobStatus> Get_JobStatus(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<JobStatus> Corporate_selectlist = new List<JobStatus>();
                if (data == null)
                {
                    var Corporate_data = db.PayStruct.Include(e => e.JobStatus).Where(e => e.JobStatus != null).Select(e => e.JobStatus.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.JobStatus.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.PayStruct.Include(e => e.JobStatus)
                        .Where(e => (e.Grade.Id == Corp_id && e.JobStatus != null) || (e.Level.Id == Corp_id && e.JobStatus != null))
                        .Select(e => e.JobStatus.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.JobStatus.Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).Where(e => e.Id == ca).SingleOrDefault();
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        private List<Level> Get_level(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Level> Corporate_selectlist = new List<Level>();
                if (data == null)
                {
                    var Corporate_data = db.PayStruct.Include(e => e.Level).Where(e => e.Level != null).Select(e => e.Level.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Level.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.PayStruct.Include(e => e.Level)
                        .Where(e => (e.Grade.Id == Corp_id && e.Level != null))
                        .Select(e => e.Level.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.Level.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        private List<Grade> Get_Grade(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Grade> Corporate_selectlist = new List<Grade>();
                if (data == null)
                {
                    var Corporate_data = db.PayStruct.Include(e => e.Grade).Where(e => e.Grade != null).Select(e => e.Grade.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Grade.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public Object ExitanceFirstlevelFun()
        {
            var data = (string)null;
            var a = Get_Job(data);
            if (a != null)
            {
                var selectlistjson = new
                {
                    SelectlistType = "JobList_DDL",
                    selectlist = new SelectList(a, "Id", "Name", "")
                };
                return selectlistjson;
            }
            else
            {
                var b = Get_JobPosition(data);
                if (b != null)
                {
                    var selectlistjson = new
                    {
                        SelectlistType = "JobPositionList_DDL",
                        selectlist = new SelectList(a, "Id", "Name", "")
                    };
                    return selectlistjson;
                }
                else
                {
                    return null;
                }
            }
        }
        private List<JobPosition> Get_JobPosition(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<JobPosition> Corporate_selectlist = new List<JobPosition>();
                if (data == null)
                {
                    var Corporate_data = db.FuncStruct.Include(e => e.JobPosition).Where(e => e.JobPosition != null).Select(e => e.JobPosition.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.JobPosition.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (data != "" && data != "0")
                {
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.FuncStruct.Include(e => e.JobPosition)
                        .Where(e => (e.Job.Id == Corp_id && e.JobPosition != null))
                        .Select(e => e.JobPosition.Id).Distinct().ToList();
                    if (region_data != null && region_data.Count != 0)
                    {
                        foreach (var ca in region_data)
                        {
                            var a = db.JobPosition.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        private List<Job> Get_Job(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Job> Corporate_selectlist = new List<Job>();
                if (data == null)
                {
                    var Corporate_data = db.FuncStruct.Include(e => e.Job).Where(e => e.Job != null).Select(e => e.Job.Id).Distinct().ToList();
                    if (Corporate_data != null && Corporate_data.Count != 0)
                    {
                        foreach (var ca in Corporate_data)
                        {
                            var a = db.Job.Find(ca);
                            Corporate_selectlist.Add(a);
                        }
                        return Corporate_selectlist;
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
        public ActionResult PopulateCorporateList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Corporate.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Corporate.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateRegionList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Region.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Region.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateCompanyList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Company.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Company.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDivisionList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Division.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Division.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateLocationList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Location.Include(e => e.LocationObj).ToList();
                if (data2 != null && data2 != "")
                {
                    selected = data2;
                }
                if (data != null && data != "")
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Location.Include(e => e.LocationObj).Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDepartmentList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Department.Include(e => e.DepartmentObj).ToList();
                var selected = (String)null;
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Department.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateGroupList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Group.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Group.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateUnitList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Unit.ToList();
                var selected = (String)null;
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Unit.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateGradeList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Grade.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Grade.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateLevelList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Level.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Level.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateJobStatusList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.JobStatus.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.JobStatus.Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateJobNameList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.Job.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.Job.Where(e => e.Id == id).ToList();
                }
                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulatePositionList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var selected = (String)null;
                var qurey = db.JobPosition.ToList();
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    qurey = db.JobPosition.Where(e => e.Id == id).ToList();
                }

                SelectList s = new SelectList(qurey, "Id", "JobPositionDesc", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListGeo(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = ExitanceFirstlevelGeo();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PopulateDropDownListPay(string data, string data2)
        {
            var a = ExitanceFirstlevelPay();
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateDropDownListFun(string data, string data2)
        {
            var a = ExitanceFirstlevelFun();
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public class selectlistjsonClass
        {
            public String SelectlistType { get; set; }
            public SelectList selectlist { get; set; }
        }
        public Object DropDownListCheckLevelGeo(string data, string data2)
        {
            //var a = ExitanceFirstlevel();

            switch (data2)
            {
                case "Corporate":

                    var b = Get_Region(data);
                    if (b != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "RegionList_DDL",
                            selectlist = new SelectList(b, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var c = Get_Company(data);
                        if (c != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "CompanyList_DDL",
                                selectlist = new SelectList(c, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var d = Get_Division(data);
                            if (d != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "DivisionList_DDL",
                                    selectlist = new SelectList(d, "Id", "FullDetails", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var e = Get_Location(data);
                                if (e != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "LocationList_DDL",
                                        selectlist = new SelectList(e, "Id", "FullDetails", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var f = Get_Department(data);
                                    if (f != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "DepartmentList_DDL",
                                            selectlist = new SelectList(f, "Id", "FullDetails", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var g = Get_Group(data);
                                        if (g != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "GroupList_DDL",
                                                selectlist = new SelectList(g, "Id", "Name", "")
                                            };
                                            return selectlistjson;
                                        }
                                        else
                                        {
                                            var h = Get_Unit(data);
                                            if (h != null)
                                            {
                                                var selectlistjson = new
                                                {
                                                    SelectlistType = "UnitList_DDL",
                                                    selectlist = new SelectList(h, "Id", "Name", "")
                                                };
                                                return selectlistjson;
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
                    }
                    break;
                case "Region":
                    var i = Get_Company(data);
                    if (i != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "CompanyList_DDL",
                            selectlist = new SelectList(i, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var j = Get_Division(data);
                        if (j != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "DivisionList_DDL",
                                selectlist = new SelectList(j, "Id", "FullDetails", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var k = Get_Location(data);
                            if (k != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "LocationList_DDL",
                                    selectlist = new SelectList(k, "Id", "FullDetails", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var l = Get_Department(data);
                                if (l != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "DepartmentList_DDL",
                                        selectlist = new SelectList(l, "Id", "FullDetails", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var m = Get_Group(data);
                                    if (m != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "GroupList_DDL",
                                            selectlist = new SelectList(m, "Id", "Name", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        var n = Get_Unit(data);
                                        if (n != null)
                                        {
                                            var selectlistjson = new
                                            {
                                                SelectlistType = "UnitList_DDL",
                                                selectlist = new SelectList(n, "Id", "Name", "")
                                            };
                                            return selectlistjson;
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
                    break;
                case "Company":
                    var o = Get_Division(data);
                    if (o != null)
                    {
                        var returnlist = new List<selectlistjsonClass>();
                        //returnlist.Add(new selectlistjsonClass
                        //{
                        //    SelectlistType = "DivisionList_DDL",
                        //    selectlist = new SelectList(o, "Id", "FullDetails", "")
                        //});

                        var selectlistjson = new
                          {
                              SelectlistType = "DivisionList_DDL",
                              selectlist = new SelectList(o, "Id", "FullDetails", "")
                          };
                        return selectlistjson;

                        var p1 = Get_Location(data + ",Division");
                        if (p1 != null)
                        {
                            returnlist.Add(new selectlistjsonClass
                            {
                                SelectlistType = "LocationList_DDL",
                                selectlist = new SelectList(p1, "Id", "FullDetails", "")
                            });
                            // return selectlistjson;
                        }
                        return returnlist;
                    }
                    else
                    {
                        var p = Get_Location(data);
                        if (p != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "LocationList_DDL",
                                selectlist = new SelectList(p, "Id", "FullDetails", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var q = Get_Department(data);
                            if (q != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "DepartmentList_DDL",
                                    selectlist = new SelectList(q, "Id", "FullDetails", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var r = Get_Group(data);
                                if (r != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "GroupList_DDL",
                                        selectlist = new SelectList(r, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    var s = Get_Unit(data);
                                    if (s != null)
                                    {
                                        var selectlistjson = new
                                        {
                                            SelectlistType = "UnitList_DDL",
                                            selectlist = new SelectList(s, "Id", "Name", "")
                                        };
                                        return selectlistjson;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "Division":
                    var t = Get_Location(data);
                    if (t != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "LocationList_DDL",
                            selectlist = new SelectList(t, "Id", "FullDetails", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var x = Get_Department(data);
                        if (x != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "DepartmentList_DDL",
                                selectlist = new SelectList(x, "Id", "FullDetails", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var y = Get_Group(data);
                            if (y != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "GroupList_DDL",
                                    selectlist = new SelectList(y, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                var z = Get_Unit(data);
                                if (z != null)
                                {
                                    var selectlistjson = new
                                    {
                                        SelectlistType = "UnitList_DDL",
                                        selectlist = new SelectList(z, "Id", "Name", "")
                                    };
                                    return selectlistjson;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                    break;
                case "Location":
                    var x1 = Get_Department(data);
                    if (x1 != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "DepartmentList_DDL",
                            selectlist = new SelectList(x1, "Id", "FullDetails", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var y1 = Get_Group(data);
                        if (y1 != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "GroupList_DDL",
                                selectlist = new SelectList(y1, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var z1 = Get_Unit(data);
                            if (z1 != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "UnitList_DDL",
                                    selectlist = new SelectList(z1, "Id", "Name", "")
                                };
                                return selectlistjson;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    break;
                case "Department":
                    var y11 = Get_Group(data);
                    if (y11 != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "GroupList_DDL",
                            selectlist = new SelectList(y11, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var z11 = Get_Unit(data);
                        if (z11 != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "UnitList_DDL",
                                selectlist = new SelectList(z11, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case "Group":
                    var z111 = Get_Unit(data);
                    if (z111 != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "UnitList_DDL",
                            selectlist = new SelectList(z111, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                default:
                    return null;
                    break;
            }
        }
        public ActionResult PopulateDropDownlistOnChangeGeo(string data, string data2)
        {
            var a = DropDownListCheckLevelGeo(data, data2);

            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopulateDropDownlistOnChangePay(string data, string data2)
        {
            var a = DropDownListCheckLevelPay(data, data2);

            return Json(a, JsonRequestBehavior.AllowGet);
        }
        private object DropDownListCheckLevelPay(string data, string data2)
        {
            switch (data2)
            {
                case "Grade":
                    var a = Get_Grade(data);
                    if (a != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "Grade_drop",
                            selectlist = new SelectList(a, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        var d = Get_level(data);
                        if (d != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "Level_drop",
                                selectlist = new SelectList(d, "Id", "Name", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            var c = Get_JobStatus(data);
                            if (c != null)
                            {
                                var selectlistjson = new
                                {
                                    SelectlistType = "JobStatus_drop",
                                    selectlist = new SelectList(c, "Id", "FullDetails", "")
                                };
                                return selectlistjson;
                            }
                        }
                    }
                    break;
                case "Level":
                    var de = Get_JobStatus(data);
                    if (de != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "JobStatus_drop",
                            selectlist = new SelectList(de, "Id", "FullDetails", "")
                        };
                        return selectlistjson;
                    }
                    break;
                default:
                    return null;
                    break;
            }
            return null;
        }
        public ActionResult PopulateDropDownlistOnChangeFun(string data, string data2)
        {
            var a = DropDownListCheckLevelFun(data, data2);
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        private object DropDownListCheckLevelFun(string data, string data2)
        {
            switch (data2)
            {
                case "Job":
                    var a = Get_Job(data);
                    if (a != null)
                    {
                        var selectlistjson = new
                        {
                            SelectlistType = "JobList_DDL",
                            selectlist = new SelectList(a, "Id", "Name", "")
                        };
                        return selectlistjson;
                    }
                    else
                    {
                        //var c = DropDownListCheckLevelFun(data, "JobPosition");
                        //if (c != null)
                        //{
                        //    return c;
                        //}
                        var b = Get_JobPosition(data);
                        if (b != null)
                        {
                            var selectlistjson = new
                            {
                                SelectlistType = "JobPositionList_DDL",
                                selectlist = new SelectList(b, "Id", "JobPositionDesc", "")
                            };
                            return selectlistjson;
                        }
                        else
                        {
                            return null;
                        }
                    }
                //break;
                //case "JobPosition":
                //    var b = Get_JobPosition(data);
                //    if (b != null)
                //    {
                //        var selectlistjson = new
                //        {
                //            SelectlistType = "JobPositionList_DDL",
                //            selectlist = new SelectList(b, "Id", "JobPositionDesc", "")
                //        };
                //        return selectlistjson;
                //    }
                //    else
                //    {
                //        return null;
                //    }
                //    break;
                default:
                    return null;
                    break;
            }
            return null;
        }
        public ActionResult GetAddressLKDetails(string data, string Empcode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                //                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                //                     .Include(e => e.Taluka).ToList();
                //IEnumerable<Address> all;
                //if (!string.IsNullOrEmpty(data))
                //{
                //    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                //}
                //else
                //{
                //    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    
                //    return Json(r, JsonRequestBehavior.AllowGet);
                //}
                //var result = (from c in all
                //              select new { c.Id, c.FullAddress }).Distinct();
                //return Json(result, JsonRequestBehavior.AllowGet);
                Dictionary<int, string> objDict = new Dictionary<int, string>();

                if (!String.IsNullOrEmpty(Empcode))
                {
                    var Employeedata = db.Employee.Select(a => new
                    {
                        EmpCode = a.EmpCode,
                        ResAddrId = a.ResAddr_Id != null ? db.Address.Where(e => e.Id == a.ResAddr_Id).FirstOrDefault().Id : 0,
                        ResAddrFullAddress = a.ResAddr_Id != null ? db.Address.Where(e => e.Id == a.ResAddr_Id).FirstOrDefault().FullAddress : "",

                        PermaAddrId = a.PerAddr_Id != null ? db.Address.Where(e => e.Id == a.PerAddr_Id).FirstOrDefault().Id : 0,
                        PermaFullAddress = a.PerAddr_Id != null ? db.Address.Where(e => e.Id == a.PerAddr_Id).FirstOrDefault().FullAddress : "",

                        CorresAddrId = a.CorAddr_Id != null ? db.Address.Where(e => e.Id == a.CorAddr_Id).FirstOrDefault().Id : 0,
                        CorresFullAddress = a.CorAddr_Id != null ? db.Address.Where(e => e.Id == a.CorAddr_Id).FirstOrDefault().FullAddress : "",


                    }).Where(e => e.EmpCode == Empcode).ToList();
                    
                    if (Employeedata.Count() > 0)
                    {
                        foreach (var item in Employeedata)
                        {
                            if (item.ResAddrId != 0)
                            {
                                objDict.Add(item.ResAddrId, item.ResAddrFullAddress); 
                            }
                            if (item.PermaAddrId != 0)
                            {
                                objDict.Add(item.PermaAddrId, item.PermaFullAddress);
                            }
                            if (item.CorresAddrId != 0)
                            {
                                objDict.Add(item.CorresAddrId, item.CorresFullAddress);
                            }
                            
                        }

                        var result = (from ca in objDict select new { srno = ca.Key, lookupvalue = ca.Value }).Distinct();

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }  
                }
                return null;
                
            }
            
        }
        public ActionResult GetContactDetLKDetails(string data, string Empcode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                ////var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                ////IEnumerable<ContactDetails> all;
                ////if (!string.IsNullOrEmpty(data))
                ////{
                ////    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                ////}
                ////else
                ////{
                ////    //var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                ////    //var list2 = fall.Except(list1);
                ////    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                ////    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                ////    //var result_1 = (from c in fall
                ////    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                ////    return Json(r, JsonRequestBehavior.AllowGet);
                ////}
                ////var result = (from c in all
                ////              select new { c.Id, c.FullContactDetails }).Distinct();
                ////return Json(result, JsonRequestBehavior.AllowGet);

                Dictionary<int, string> myContactDict = new Dictionary<int, string>();

                if (!String.IsNullOrEmpty(Empcode))
                {
                    var Employeedata = db.Employee.Select(a => new
                    {
                        EmpCode = a.EmpCode,
                        ResContactId = a.ResContact_Id != null ?  db.ContactDetails.Where(e => e.Id == a.ResContact_Id).FirstOrDefault().Id : 0,
                        ResContactFullContactDetails = a.ResContact_Id != null ?  db.ContactDetails.Where(e => e.Id == a.ResContact_Id).FirstOrDefault().FullContactDetails : "",

                        PermaContactId = a.PerContact_Id != null ?  db.ContactDetails.Where(e => e.Id == a.PerContact_Id).FirstOrDefault().Id : 0,
                        PermaFullContactDetails = a.PerContact_Id != null ? db.ContactDetails.Where(e => e.Id == a.PerContact_Id).FirstOrDefault().FullContactDetails : "",

                        CorresContactId =  a.CorContact_Id != null ? db.ContactDetails.Where(e => e.Id == a.CorContact_Id).FirstOrDefault().Id : 0,
                        CorresFullContactDetails = a.CorContact_Id != null ? db.ContactDetails.Where(e => e.Id == a.CorContact_Id).FirstOrDefault().FullContactDetails : "",

                    }).Where(e => e.EmpCode == Empcode).ToList();

                    if (Employeedata.Count() > 0)
                    {
                        foreach (var item in Employeedata)
                        {
                            if (item.ResContactId != 0)
                            {
                                myContactDict.Add(item.ResContactId, item.ResContactFullContactDetails);
                            }
                            if (item.PermaContactId != 0)
                            {
                                myContactDict.Add(item.PermaContactId, item.PermaFullContactDetails);
                            }
                            if (item.CorresContactId != 0)
                            {
                                myContactDict.Add(item.CorresContactId, item.CorresFullContactDetails);
                            }
                           
                        }

                        var result = (from ca in myContactDict select new { srno = ca.Key, lookupvalue = ca.Value }).Distinct();

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                return null;

            }
            
        }
        // [HttpPost]
        public ActionResult CheckIfProcessInWOrk()
        {
            string chk = SessionManager.CheckProcessStatusFunc;
            if (chk != "")
            {
                return Json(new Utility.JsonReturnClass { success = true, responseText = chk }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new Utility.JsonReturnClass { success = false, responseText = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(Employee Emp, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                if (SessionManager.CheckProcessStatus != "")
                {
                    Msg.Add(SessionManager.CheckProcessStatus);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                string ReportingHR = form["ReportingHR_drop"] == "0" ? "" : form["ReportingHR_drop"];
                string ReportingLevel1 = form["ReportingLevel1_drop"] == "0" ? "" : form["ReportingLevel1_drop"];
                string ReportingLevel2 = form["ReportingLevel2_drop"] == "0" ? "" : form["ReportingLevel2_drop"];
                string ReportingLevel3 = form["ReportingLevel3_drop"] == "0" ? "" : form["ReportingLevel3_drop"];
                string ReportingStructRights = form["ReportingStructRightslist"] == null ? null : form["ReportingStructRightslist"];

                string MaritalStatus = form["MaritalStatus_drop"] == "0" ? "" : form["MaritalStatus_drop"];
                string Gender = form["Gender_drop"] == "0" ? "" : form["Gender_drop"];
                string Category = form["Category_drop"] == "0" ? "" : form["Category_drop"];
                string Caste = form["Caste_drop"] == "0" ? "" : form["Caste_drop"];
                string Religion = form["Religion_drop"] == "0" ? "" : form["Religion_drop"];
                string BloodGroup = form["BloodGroup_drop"] == "0" ? "" : form["BloodGroup_drop"];
                string ResAddrs = form["ResAddrslist"] == "0" ? "" : form["ResAddrslist"];
                string ResContactDetails = form["ResContactDetailslist"] == "0" ? "" : form["ResContactDetailslist"];
                string PermAddrs = form["PermAddrslist"] == "0" ? "" : form["PermAddrslist"];
                string PermContactDetails = form["PermContactDetailslist"] == "0" ? "" : form["PermContactDetailslist"];
                string CorrsAddrs = form["CorrsAddrslist"] == "0" ? "" : form["CorrsAddrslist"];
                string CorrsContactDetails = form["CorrsContactDetailslist"] == "0" ? "" : form["CorrsContactDetailslist"];
                string Corporate = form["CorporateList_DDL"] == "0" ? "" : form["CorporateList_DDL"];
                string Region = form["RegionList_DDL"] == "0" ? "" : form["RegionList_DDL"];
                string Company = form["CompanyList_DDL"] == "0" ? "" : form["CompanyList_DDL"];
                string Division = form["DivisionList_DDL"] == "0" ? "" : form["DivisionList_DDL"];
                string Location = form["LocationList_DDL"] == "0" ? "" : form["LocationList_DDL"];
                string Department = form["DepartmentList_DDL"] == "0" ? "" : form["DepartmentList_DDL"];
                string Group = form["GroupList_DDL"] == "0" ? "" : form["GroupList_DDL"];
                string Unit = form["UnitList_DDL"] == "0" ? "" : form["UnitList_DDL"];
                string Grade = form["Grade_drop"] == "0" ? "" : form["Grade_drop"];
                string Level = form["Level_drop"] == "0" ? "" : form["Level_drop"];
                string JobStatus = form["JobStatus_drop"] == "0" ? "" : form["JobStatus_drop"];
                string EmpActingStatus = form["EmpActingStatus_drop"] == "0" ? "" : form["EmpActingStatus_drop"];
                string Job = form["JobList_DDL"] == "0" ? "" : form["JobList_DDL"];
                string Position = form["Position_drop"] == "0" ? "" : form["Position_drop"];
                string TimingCode = form["TimingCode_drop"] == "0" ? "" : form["TimingCode_drop"];

                int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);
                int FatherName_FName = form["FatherName_id"] == "" ? 0 : Convert.ToInt32(form["FatherName_id"]);
                int MotherName_FName = form["MotherName_id"] == "" ? 0 : Convert.ToInt32(form["MotherName_id"]);
                int HusbandName_FName = form["HusbandName_id"] == "" ? 0 : Convert.ToInt32(form["HusbandName_id"]);
                int BeforeMarriageName_FName = form["BeforeMarriageName_id"] == "" ? 0 : Convert.ToInt32(form["BeforeMarriageName_id"]);

                string CandidateDocumentslist = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];

                using (DataBaseContext db = new DataBaseContext())
                {
                    var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
                             .Include(e => e.Department)
                             .Include(e => e.Department.DepartmentObj)
                             .Include(e => e.Division)
                             .Include(e => e.Location)
                             .Include(e => e.Location.LocationObj)
                             .Include(e => e.Group)
                             .Include(e => e.Unit);

                    Emp.EmpName = db.NameSingle.Find(EmpName_FName);
                    Emp.FatherName = db.NameSingle.Find(FatherName_FName);
                    Emp.MotherName = db.NameSingle.Find(MotherName_FName);
                    Emp.HusbandName = db.NameSingle.Find(HusbandName_FName);
                    Emp.BeforeMarriageName = db.NameSingle.Find(BeforeMarriageName_FName);

                    var Get_Pay_Id = db.PayStruct.Include(e => e.Grade)
                           .Include(e => e.Level).Include(e => e.JobStatus);

                    var Get_Fun_Id = db.FuncStruct.Include(e => e.Job)
                           .Include(e => e.JobPosition);

                    if (Grade != "")
                    {
                        var id = Convert.ToInt32(Grade);
                        Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade.Id == id);
                    }
                    if (Level != "")
                    {
                        var id = Convert.ToInt32(Level);
                        Get_Pay_Id = Get_Pay_Id.Where(e => e.Level.Id == id);
                    }

                    if (JobStatus != "")
                    {
                        var id = Convert.ToInt32(JobStatus);
                        Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus.Id == id);
                    }

                    //var pay_data = Get_Pay_Id.SingleOrDefault();


                    if (Job != "")
                    {
                        var id = Convert.ToInt32(Job);
                        Get_Fun_Id = Get_Fun_Id.Where(e => e.Job.Id == id);
                    }
                    if (Position != "")
                    {
                        var id = Convert.ToInt32(Position);
                        Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition.Id == id);
                    }

                    // var fun_data = Get_Fun_Id.SingleOrDefault();

                    if (Corporate != "")
                    {
                        var id = Convert.ToInt32(Corporate);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate.Id == id);
                    }
                    if (Region != "")
                    {
                        var id = Convert.ToInt32(Region);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Region.Id == id);
                    }
                    if (Company != "")
                    {
                        var id = Convert.ToInt32(Company);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Company.Id == id);
                    }
                    if (Division != "")
                    {
                        var id = Convert.ToInt32(Division);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Division.Id == id);
                    }
                    if (Location != "")
                    {
                        var id = Convert.ToInt32(Location);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Location.Id == id);
                    }
                    if (Department != "")
                    {
                        var id = Convert.ToInt32(Department);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Department.Id == id);
                    }
                    if (Group != "")
                    {
                        var id = Convert.ToInt32(Group);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Group.Id == id);
                    }
                    if (Unit != "")
                    {
                        var id = Convert.ToInt32(Unit);
                        Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit.Id == id);
                    }

                    //var geo_data = Get_Geo_Id.SingleOrDefault();
                    // Get PayStuct id
                    //if (ReportingHR != "")
                    //{
                    //    var val = db.ReportingStructRights.Find(int.Parse(ReportingHR));
                    //    Emp.ReportingStructRights = val;
                    //}
                    //if (ReportingLevel1 != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel1));
                    //    Emp.ReportingLevel1 = val;
                    //}
                    //if (ReportingLevel2 != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel2));
                    //    Emp.ReportingLevel2 = val;
                    //}
                    //if (ReportingLevel3 != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel3));
                    //    Emp.ReportingLevel3 = val;
                    //}

                    if (MaritalStatus != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "128").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(MaritalStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(MaritalStatus));
                        Emp.MaritalStatus = val;
                    }

                    if (Gender != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "129").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Gender)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Gender));
                        Emp.Gender = val;
                    }

                    if (ResAddrs != null && ResAddrs != "")
                    {
                        int AddId = Convert.ToInt32(ResAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Emp.ResAddr = val;
                    }

                    if (ReportingStructRights != null)
                    {
                        var ids = Utility.StringIdsToListIds(ReportingStructRights);
                        var reportingrightslist = new List<ReportingStructRights>();
                        foreach (var item in ids)
                        {
                            var id = Convert.ToInt32(item);
                            var data = db.ReportingStructRights.Find(id);
                            reportingrightslist.Add(data);

                        }

                        Emp.ReportingStructRights = reportingrightslist;
                    }

                    if (PermAddrs != null && PermAddrs != "")
                    {
                        int AddId = Convert.ToInt32(PermAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Emp.PerAddr = val;
                    }

                    if (CorrsAddrs != null && CorrsAddrs != "")
                    {
                        int AddId = Convert.ToInt32(CorrsAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Emp.CorAddr = val;
                    }

                    if (ResContactDetails != null && ResContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ResContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Emp.ResContact = val;
                    }

                    if (PermContactDetails != null && PermContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(PermContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Emp.PerContact = val;
                    }

                    if (CorrsContactDetails != null && CorrsContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(CorrsContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Emp.CorContact = val;
                    }


                    if (Get_Geo_Id != null)
                    {
                        var geo_data = Get_Geo_Id.FirstOrDefault();
                        if (geo_data != null)
                        {
                            Emp.GeoStruct = db.GeoStruct.Find(geo_data.Id);
                        }
                    }

                    if (Get_Fun_Id != null)
                    {
                        var fun_data = Get_Fun_Id.FirstOrDefault();

                        if (fun_data != null)
                        {
                            Emp.FuncStruct = db.FuncStruct.Find(fun_data.Id);
                        }

                    }
                    if (Get_Pay_Id != null)
                    {
                        var pay_data = Get_Pay_Id.FirstOrDefault();
                        if (pay_data != null)
                        {
                            Emp.PayStruct = db.PayStruct.Find(pay_data.Id);
                        }
                    }
                    if (Emp.GeoStruct == null)
                    {
                        Msg.Add("selected division, location , department not configured to each other.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                    }
                    if (Location == "")
                    {
                        Msg.Add(" Please Select Location.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                   
                    if (Job == "")
                    {
                        Msg.Add(" Please Select Job.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (Grade == "")
                    {
                        Msg.Add(" Please Select Grade.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (JobStatus == "")
                    {
                        Msg.Add(" Please Select JobStatus.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (Gender == "")
                    {
                        Msg.Add(" Please Select Gender.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    List<EmployeeDocuments> ObjEmployeeDocuments = new List<EmployeeDocuments>();
                    if (CandidateDocumentslist != null && CandidateDocumentslist != "")
                    {
                        var ids = Utility.StringIdsToListIds(CandidateDocumentslist);
                        foreach (var ca in ids)
                        {
                            var value = db.EmployeeDocuments.Find(ca);
                            ObjEmployeeDocuments.Add(value);
                            Emp.EmployeeDocuments = ObjEmployeeDocuments;
                        }

                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            if (db.Employee.Any(o => o.EmpCode == Emp.EmpCode))
                            {
                                Msg.Add("  Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            //if (db.Employee.Any(o => o.EmpName.EmpTitle == Emp.EmpName.EmpTitle && o.EmpName.FName == Emp.EmpName.MName && o.EmpName.MName == Emp.EmpName.MName && o.EmpName.LName == o.EmpName.LName))
                            //{
                            //    Msg.Add("  Person with this Name Already Exists.  ");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}
                            if (Emp.FatherName != null)
                            {
                                if (db.Employee.Any(o => o.FatherName.EmpTitle == Emp.FatherName.EmpTitle && o.FatherName.FName == Emp.FatherName.FName && o.FatherName.MName == Emp.FatherName.MName && o.FatherName.LName == Emp.FatherName.LName))
                                {
                                    Msg.Add("   Person with this Name Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (Emp.MotherName != null)
                            {
                                if (db.Employee.Any(o => o.MotherName.EmpTitle == Emp.MotherName.EmpTitle && o.MotherName.FName == Emp.MotherName.FName && o.MotherName.MName == Emp.MotherName.MName && o.MotherName.LName == Emp.MotherName.LName))
                                {
                                    Msg.Add("   Person with this Name Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (Emp.HusbandName != null)
                            {
                                if (db.Employee.Any(o => o.HusbandName.EmpTitle == Emp.HusbandName.EmpTitle && o.HusbandName.FName == Emp.HusbandName.FName && o.HusbandName.MName == Emp.HusbandName.MName && o.HusbandName.LName == Emp.HusbandName.LName))
                                {
                                    Msg.Add("   Person with this Name Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (Emp.BeforeMarriageName != null)
                            {
                                if (db.Employee.Any(o => o.BeforeMarriageName.EmpTitle == Emp.BeforeMarriageName.EmpTitle && o.BeforeMarriageName.FName == Emp.BeforeMarriageName.FName && o.BeforeMarriageName.MName == Emp.BeforeMarriageName.MName && o.BeforeMarriageName.LName == Emp.BeforeMarriageName.LName))
                                {
                                    Msg.Add("   Person with this Name Already Exists.  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (db.Employee.Any(o => o.CardCode == Emp.CardCode))
                            {
                                Msg.Add(" Card Code Already Exists.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                            var Id = Convert.ToInt32(SessionManager.CompanyId);
                            var CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                            if (CompCode != null)
                            {
                                if (CompCode.Code.ToUpper() == "KDCC" && CompCode.RegistrationDate != null)
                                {
                                    string dob = "01/01/1940";
                                    if (Emp.ServiceBookDates.JoiningDate < CompCode.RegistrationDate.Value.Date)
                                    {
                                        Msg.Add("Joining date Should be Grater Than bank Registration Date");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }
                                    if (Emp.ServiceBookDates.BirthDate < Convert.ToDateTime(dob).Date)
                                    {
                                        Msg.Add("Birth date Should be Grater Than 01/01/1940");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }

                                }
                            }
                            Emp.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            Employee employee = new Employee()
                            {
                                EmpCode = Emp.EmpCode == null ? "" : Emp.EmpCode.Trim(),
                                EmpName = Emp.EmpName,
                                FatherName = Emp.FatherName,
                                HusbandName = Emp.HusbandName,
                                MotherName = Emp.MotherName,
                                BeforeMarriageName = Emp.BeforeMarriageName,
                                BirthPlace = Emp.BirthPlace == null ? "" : Emp.BirthPlace.Trim(),
                                CardCode = Emp.CardCode,
                                //Caste = Emp.Caste,
                                //Category = Emp.Category,
                                CorAddr = Emp.CorAddr,
                                CorContact = Emp.CorContact,
                                //EmpNameDetail = Emp.EmpNameDetail,
                                EMPRESIGNSTAT = Emp.EMPRESIGNSTAT,
                                FuncStruct = Emp.FuncStruct,
                                Gender = Emp.Gender,
                                GeoStruct = Emp.GeoStruct,
                                //IdentityMarks = Emp.IdentityMarks,
                                MaritalStatus = Emp.MaritalStatus,
                                PayStruct = Emp.PayStruct,
                                PerAddr = Emp.PerAddr,
                                PerContact = Emp.PerContact,
                                Photo = Emp.Photo,
                                // Religion = Emp.Religion,
                                ResAddr = Emp.ResAddr,
                                ResContact = Emp.ResContact,
                                ServiceBookDates = Emp.ServiceBookDates,
                                TimingCode = Emp.TimingCode,
                                ValidFromDate = Emp.ValidFromDate,
                                ValidToDate = Emp.ValidToDate,


                                //ReportingHR = Emp.ReportingHR,
                                //ReportingLevel1 = Emp.ReportingLevel1,
                                //ReportingLevel2 = Emp.ReportingLevel2,
                                //ReportingLevel3 =Emp.ReportingLevel3,
                                ReportingStructRights = Emp.ReportingStructRights,

                                EmployeeDocuments = Emp.EmployeeDocuments,

                                DBTrack = Emp.DBTrack
                            };
                            try
                            {
                                db.Employee.Add(employee);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Emp.DBTrack);
                                DT_Employee DT_Emp = (DT_Employee)rtn_Obj;
                                DT_Emp.EmpName_Id = Emp.EmpName == null ? 0 : Emp.EmpName.Id;
                                DT_Emp.MotherName_Id = Emp.MotherName == null ? 0 : Emp.MotherName.Id;
                                DT_Emp.FatherName_Id = Emp.FatherName == null ? 0 : Emp.FatherName.Id;
                                DT_Emp.HusbandName_Id = Emp.HusbandName == null ? 0 : Emp.HusbandName.Id;
                                DT_Emp.BeforeMarriageName_Id = Emp.BeforeMarriageName == null ? 0 : Emp.BeforeMarriageName.Id;
                                DT_Emp.GeoStruct_Id = Emp.GeoStruct == null ? 0 : Emp.GeoStruct.Id;
                                DT_Emp.FuncStruct_Id = Emp.FuncStruct == null ? 0 : Emp.FuncStruct.Id;
                                DT_Emp.PayStruct_Id = Emp.PayStruct == null ? 0 : Emp.PayStruct.Id;
                                DT_Emp.CorAddr_Id = Emp.CorAddr == null ? 0 : Emp.CorAddr.Id;
                                DT_Emp.CorContact_Id = Emp.CorContact == null ? 0 : Emp.CorContact.Id;
                                DT_Emp.PerAddr_Id = Emp.PerAddr == null ? 0 : Emp.PerAddr.Id;
                                DT_Emp.PerContact_Id = Emp.PerContact == null ? 0 : Emp.PerContact.Id;
                                DT_Emp.ResAddr_Id = Emp.ResAddr == null ? 0 : Emp.ResAddr.Id;
                                DT_Emp.ResContact_Id = Emp.ResContact == null ? 0 : Emp.ResContact.Id;
                                DT_Emp.ServiceBookDates_Id = Emp.ServiceBookDates == null ? 0 : Emp.ServiceBookDates.Id;
                                // DT_Emp.Nominees_Id = Emp.Nominees == null ? 0 : Emp.Nominees.Id;
                                DT_Emp.EmpmedicalInfo_Id = Emp.EmpmedicalInfo == null ? 0 : Emp.EmpmedicalInfo.Id;
                                DT_Emp.EmpSocialInfo_Id = Emp.EmpSocialInfo == null ? 0 : Emp.EmpSocialInfo.Id;
                                DT_Emp.EmpAcademicInfo_Id = Emp.EmpAcademicInfo == null ? 0 : Emp.EmpAcademicInfo.Id;
                                //DT_Emp.PassportDetails_Id = Emp.PassportDetails == null ? 0 : Emp.PassportDetails.Id;
                                //DT_Emp.VisaDetails_Id = Emp.VisaDetails == null ? 0 : Emp.VisaDetails.Id;
                                //DT_Emp.ForeignTrips_Id = Emp.ForeignTrips == null ? 0 : Emp.ForeignTrips.Id;
                                //DT_Emp.FamilyDetails_Id = Emp.FamilyDetails == null ? 0 : Emp.FamilyDetails.Id;
                                //DT_Emp.GuarantorDetails_Id = Emp.GuarantorDetails == null ? 0 : Emp.GuarantorDetails.Id;
                                //DT_Emp.InsuranceDetails_Id = Emp.InsuranceDetails == null ? 0 : Emp.InsuranceDetails.Id;
                                DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Emp.Gender.Id;
                                DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : Emp.MaritalStatus.Id;

                                db.Create(DT_Emp);
                                db.SaveChanges();
                                //employee.GeoStruct.Company.Id

                                if (Convert.ToString(Session["CompId"]) != null)
                                {
                                    var oEmployeePayroll = new EmployeePayroll();
                                    oEmployeePayroll.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    oEmployeePayroll.Employee = employee;
                                    db.EmployeePayroll.Add(oEmployeePayroll);
                                    db.SaveChanges();

                                    var oEmployeeLeave = new EmployeeLeave();
                                    oEmployeeLeave.Employee = employee;
                                    oEmployeeLeave.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.EmployeeLeave.Add(oEmployeeLeave);
                                    db.SaveChanges();

                                    var oEmployeeAppraisal = new EmployeeAppraisal();
                                    oEmployeeAppraisal.Employee = employee;
                                    oEmployeeAppraisal.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.EmployeeAppraisal.Add(oEmployeeAppraisal);
                                    db.SaveChanges();

                                    var oEmployeeTraining = new EmployeeTraining();
                                    oEmployeeTraining.Employee = employee;
                                    oEmployeeTraining.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.EmployeeTraining.Add(oEmployeeTraining);
                                    db.SaveChanges();

                                    var oEmployeeAttendance = new EmployeeAttendance();
                                    oEmployeeAttendance.Employee = employee;
                                    oEmployeeAttendance.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.EmployeeAttendance.Add(oEmployeeAttendance);
                                    db.SaveChanges();

                                    var oEmployeeExit = new EmployeeExit();
                                    oEmployeeExit.Employee = employee;
                                    oEmployeeExit.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    db.EmployeeExit.Add(oEmployeeExit);
                                    db.SaveChanges();

                                    var oEmployeeIR = new EmployeeIR();
                                    oEmployeeIR.Employee = employee;
                                    db.EmployeeIR.Add(oEmployeeIR);
                                    db.SaveChanges();


                                    //atach comp leave
                                    var id = int.Parse(Convert.ToString(Session["CompId"]));
                                    var CompLvData = db.CompanyLeave.Where(e => e.Company.Id == id).SingleOrDefault();
                                    List<EmployeeLeave> ListEmployeeLeave = new List<EmployeeLeave>();
                                    ListEmployeeLeave.Add(oEmployeeLeave);
                                    CompLvData.EmployeeLeave = ListEmployeeLeave;
                                    db.Entry(CompLvData).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    var company_data = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
                                    List<EmployeePayroll> emp = new List<EmployeePayroll>();
                                    emp.Add(oEmployeePayroll);
                                    company_data.EmployeePayroll = emp;
                                    db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    var companyTraining = db.CompanyTraining.Where(e => e.Company.Id == id).SingleOrDefault();
                                    List<EmployeeTraining> empTraining = new List<EmployeeTraining>();
                                    empTraining.Add(oEmployeeTraining);
                                    companyTraining.EmployeeTraining = empTraining;
                                    db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    var companyAppraisal = db.CompanyAppraisal.Where(e => e.Company.Id == id).SingleOrDefault();
                                    List<EmployeeAppraisal> empApparaisal = new List<EmployeeAppraisal>();
                                    empApparaisal.Add(oEmployeeAppraisal);
                                    companyAppraisal.EmployeeAppraisal = empApparaisal;
                                    db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    var companyAttendance = db.CompanyAttendance.Where(e => e.Company.Id == id).SingleOrDefault();
                                    List<EmployeeAttendance> empAttendance = new List<EmployeeAttendance>();
                                    empAttendance.Add(oEmployeeAttendance);
                                    companyAttendance.EmployeeAttendance = empAttendance;
                                    db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();

                                    var CompanyExit = db.CompanyExit.Where(e => e.Company.Id == id).SingleOrDefault();
                                    List<EmployeeExit> empExit = new List<EmployeeExit>();
                                    empExit.Add(oEmployeeExit);
                                    CompanyExit.EmployeeExit = empExit;
                                    db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();


                                    

                                }
                                int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                                string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                                using (SqlConnection con = new SqlConnection(connString))
                                {
                                    using (SqlCommand cmd = new SqlCommand("Insert_EmpReporting", con))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;

                                        cmd.Parameters.Add("@CompCode", SqlDbType.VarChar).Value = db.Company.Where(e => e.Id == Compid).SingleOrDefault().Code;
                                        cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = employee.EmpCode;

                                        con.Open();
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                ts.Complete();
                                Msg.Add("  Data  Saved successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = Emp.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new Object[] { "", "", "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelState in ModelState.Values)
                        {
                            foreach (ModelError error in modelState.Errors)
                            {
                                sb.Append(error.ErrorMessage);
                                sb.Append("." + "\n");
                            }
                        }
                        var errorMsg = sb.ToString();
                        Msg.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }

                }
            }
            catch (Exception ex)
            {
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
        public class returnDataClass
        {
            public Array id { get; set; }
            public Array val { get; set; }

            public Array EmpDoc_id { get; set; }
            public Array EmpDoc_val { get; set; }
        }
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.Employee//.Include(e => e.PayScale) 
                    .Where(e => e.Id == data)
                     .Include(e => e.CorContact)
                    .Include(e => e.PerAddr)
                    .Include(e => e.PerContact)
                    .Include(e => e.ResAddr)
                    .Include(e => e.ResContact)
                    
                    .Select(e => new
                    {
                        BirthPlace = e.BirthPlace,

                        //BloodGroup_Id = e.BloodGroup.Id == null ? 0 : e.BloodGroup.Id,
                        CardCode = e.CardCode,
                        //Caste_Id = e.Caste.Id == null ? 0 : e.Caste.Id,
                        //Category_Id = e.Category.Id == null ? 0 : e.Category.Id,
                        Code = e.EmpCode,
                        //EmpNameDetail = e.EmpNameDetail,
                        EMPRESIGNSTAT = e.EMPRESIGNSTAT,
                        Gender_Id = e.Gender.Id == null ? 0 : e.Gender.Id,
                        MaritalStatus_Id = e.MaritalStatus.Id == null ? 0 : e.MaritalStatus.Id,
                        //Religion_Id = e.Religion.Id == null ? 0 : e.Religion.Id,
                        ValidFromDate = e.ValidFromDate,
                        ValidToDate = e.ValidToDate,
                        ServiceBookDates = e.ServiceBookDates == null ? null : e.ServiceBookDates,
                        GeoStruct_Corporate_Id = e.GeoStruct.Corporate == null ? 0 : e.GeoStruct.Corporate.Id,
                        GeoStruct_Region_Id = e.GeoStruct.Region == null ? 0 : e.GeoStruct.Region.Id,
                        GeoStruct_Company_Id = e.GeoStruct.Company == null ? 0 : e.GeoStruct.Company.Id,
                        GeoStruct_Division_Id = e.GeoStruct.Division == null ? 0 : e.GeoStruct.Division.Id,
                        GeoStruct_Location_Id = e.GeoStruct.Location == null ? 0 : e.GeoStruct.Location.Id,
                        GeoStruct_Department_Id = e.GeoStruct.Department == null ? 0 : e.GeoStruct.Department.Id,
                        GeoStruct_Group_Id = e.GeoStruct.Group == null ? 0 : e.GeoStruct.Group.Id,
                        GeoStruct_Unit_Id = e.GeoStruct.Unit == null ? 0 : e.GeoStruct.Unit.Id,
                        FuncStruct_Job_Id = e.FuncStruct.Job == null ? 0 : e.FuncStruct.Job.Id,
                        FuncStruct_Position_Id = e.FuncStruct.JobPosition == null ? 0 : e.FuncStruct.JobPosition.Id,
                        PayStruct_Grade_Id = e.PayStruct.Grade == null ? 0 : e.PayStruct.Grade.Id,
                        PayStruct_Level_Id = e.PayStruct.Level == null ? 0 : e.PayStruct.Level.Id,
                        PayStruct_JobStatus_Id = e.PayStruct.JobStatus == null ? 0 : e.PayStruct.JobStatus.Id,
                        PayStruct_EmpActingStatus_Id = e.PayStruct.JobStatus.EmpActingStatus == null ? 0 : e.PayStruct.JobStatus.EmpActingStatus.Id,

                        EmpName_FullNameFML = e.EmpName == null ? "" : e.EmpName.FullNameFML,
                        EmpName_Id = e.EmpName == null ? 0 : e.EmpName.Id,

                        FatherName_FullNameFML = e.FatherName == null ? "" : e.FatherName.FullNameFML,
                        FatherName_Id = e.FatherName == null ? 0 : e.FatherName.Id,

                        MotherName_FullNameFML = e.MotherName == null ? "" : e.MotherName.FullNameFML,
                        MotherNam_Id = e.MotherName == null ? 0 : e.MotherName.Id,

                        HusbandName_FullNameFML = e.HusbandName == null ? "" : e.HusbandName.FullNameFML,
                        HusbandName_Id = e.HusbandName == null ? 0 : e.HusbandName.Id,

                        BeforeMarriageName_FullNameFML = e.BeforeMarriageName == null ? "" : e.BeforeMarriageName.FullNameFML,
                        BeforeMarriageName_Id = e.BeforeMarriageName == null ? 0 : e.BeforeMarriageName.Id,


                        CorrsAddrs_Id = e.CorAddr == null ? null : e.CorAddr.Id.ToString(),
                        CorrsAddrs_FullDetails = e.CorAddr == null && e.CorAddr.FullAddress == null ? null : e.CorAddr.FullAddress,

                        PermAddrs_Id = e.PerAddr == null ? null : e.PerAddr.Id.ToString(),
                        PermAddrs_FullDetails = e.PerAddr == null && e.PerAddr.FullAddress == null ? null : e.PerAddr.FullAddress,

                        ResAddrs_Id = e.ResAddr == null ? null : e.ResAddr.Id.ToString(),
                        ResAddrs_FullDetails = e.ResAddr == null && e.ResAddr.FullAddress == null ? null : e.ResAddr.FullAddress,

                        CorrsContactDetails_Id = e.CorContact == null ? null : e.CorContact.Id.ToString(),
                        CorrsContactDetails_FullDetails = e.CorContact == null && e.CorContact.FullContactDetails == null ? null : e.CorContact.FullContactDetails,

                        PermContactDetails_Id = e.PerContact == null ? null : e.PerContact.Id.ToString(),
                        PermContactDetails_FullDetails = e.PerContact == null && e.PerContact.FullContactDetails == null ? null : e.PerContact.FullContactDetails,

                        ResContactDetails_Id = e.ResContact == null ? null : e.ResContact.Id.ToString(),
                        ResContactDetails_FullDetails = e.ResContact == null && e.ResContact.FullContactDetails == null ? null : e.ResContact.FullContactDetails,



                    }).ToList();

                var add_data = db.Employee
                    .Where(e => e.Id == data)
                    .Include(e => e.ReportingStructRights)
                    .Include(e => e.ReportingStructRights.Select(a => a.FuncModules))
                    .Include(e => e.ReportingStructRights.Select(a => a.FuncSubModules))
                    .Include(e => e.ReportingStructRights.Select(a => a.AccessRights))
                    .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct))
                    .Include(e => e.EmployeeDocuments)
                    .Include(e => e.EmployeeDocuments.Select(q => q.SubDocumentName))
                    .Include(e => e.EmployeeDocuments.Select(q => q.DocumentType))
                  .ToList();

                var ListreturnDataClass = new List<returnDataClass>();
                foreach (var item in add_data)
                {
                    ListreturnDataClass.Add(new returnDataClass
                    {
                        id = item.ReportingStructRights.Select(e => e.Id.ToString()).ToArray(),
                        val = item.ReportingStructRights.Select(e => e.FullDetails).ToArray(),


                        EmpDoc_id = item.EmployeeDocuments.Select(e => e.Id.ToString()).ToArray(),
                        EmpDoc_val = item.EmployeeDocuments.Select(e => "DocType: " + e.DocumentType.LookupVal.ToString() + ",SubDocName : " + e.SubDocumentName.LookupVal.ToString()).ToArray(),
                        
                    });
                }

                //ReportingStructRights_Id = e.ReportingStructRights.Count() > 0 ? e.ReportingStructRights.Select(a => a.Id).ToArray().ToString() : null,
                //ReportingStructRights_Val = e.ReportingStructRights.Count() > 0 ? e.ReportingStructRights.Select(a => a.FullDetails).ToArray().ToString() : null
                //var W = db.DT_EmpOff
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         AccountNo = e.AccountNo == null ? "" : e.AccountNo,
                //         PFAppl = e.PFAppl,
                //         SelfHandicap = e.SelfHandicap,
                //         FamilyHandicap = e.FamilyHandicap,
                //         HandicapRemark = e.HandicapRemark == null ? "" : e.HandicapRemark,
                //         PTAppl = e.PTAppl,
                //         ESICAppl = e.ESICAppl,
                //         LWFAppl = e.LWFAppl,
                //         VPFAppl = e.VPFAppl,
                //         VPFAmt = e.VPFAmt,
                //         VPFPerc = e.VPFPerc,
                //         PayMode_Val = e.PayMode_Id == 0 ? "" : db.LookupValue
                //                 .Where(x => x.Id == e.PayMode_Id)
                //                 .Select(x => x.LookupVal).FirstOrDefault(),
                //         AccountType_Val = e.AccountType_Id == 0 ? "" : db.LookupValue
                //                 .Where(x => x.Id == e.AccountType_Id)
                //                 .Select(x => x.LookupVal).FirstOrDefault(),
                //         Bank_Val = e.Bank_Id == 0 ? "" : db.Bank
                //              .Where(x => x.Id == e.Bank_Id)
                //              .Select(x => x.FullDetails).FirstOrDefault(),
                //         Branch_Val = e.Branch_Id == 0 ? "" : db.Branch
                //              .Where(x => x.Id == e.Branch_Id)
                //              .Select(x => x.FullDetails).FirstOrDefault(),
                //         PayProcessGroup_Val = e.PayProcessGroup_Id == 0 ? "" : db.PayProcessGroup
                //              .Where(x => x.Id == e.PayProcessGroup_Id)
                //              .Select(x => x.FullDetails).FirstOrDefault()


                //         //Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         //Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Emp = db.Employee.Find(data);
                TempData["RowVersion"] = Emp.RowVersion;
                var Auth = Emp.DBTrack.IsModified;
                return Json(new Object[] { Q, ListreturnDataClass, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(Employee Emp, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            if (SessionManager.CheckProcessStatus != "")
            {
                Msg.Add(SessionManager.CheckProcessStatus + " Process is Ongoing,Can't make changes.");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
            try
            {
                string ReportingHR = form["ReportingHR_drop"] == "0" ? "" : form["ReportingHR_drop"];
                string ReportingLevel1 = form["ReportingLevel1_drop"] == "0" ? "" : form["ReportingLevel1_drop"];
                string ReportingLevel2 = form["ReportingLevel2_drop"] == "0" ? "" : form["ReportingLevel2_drop"];
                string ReportingLevel3 = form["ReportingLevel3_drop"] == "0" ? "" : form["ReportingLevel3_drop"];
                string ReportingStructRights = form["ReportingStructRightslist"] == null ? null : form["ReportingStructRightslist"];

                string MaritalStatus = form["MaritalStatus_drop"] == "0" ? "" : form["MaritalStatus_drop"];
                string Gender = form["Gender_drop"] == "0" ? "" : form["Gender_drop"];
                string Category = form["Category_drop"] == "0" ? "" : form["Category_drop"];
                string Caste = form["Caste_drop"] == "0" ? "" : form["Caste_drop"];
                string Religion = form["Religion_drop"] == "0" ? "" : form["Religion_drop"];
                string BloodGroup = form["BloodGroup_drop"] == "0" ? "" : form["BloodGroup_drop"];
                string ResAddrs = form["ResAddrslist"] == "0" ? "" : form["ResAddrslist"];
                string ResContactDetails = form["ResContactDetailslist"] == "0" ? "" : form["ResContactDetailslist"];
                string PermAddrs = form["PermAddrslist"] == "0" ? "" : form["PermAddrslist"];
                string PermContactDetails = form["PermContactDetailslist"] == "0" ? "" : form["PermContactDetailslist"];
                string CorrsAddrs = form["CorrsAddrslist"] == "0" ? "" : form["CorrsAddrslist"];
                string CorrsContactDetails = form["CorrsContactDetailslist"] == "0" ? "" : form["CorrsContactDetailslist"];
                string Corporate = form["CorporateList_DDL"] == "0" ? "" : form["CorporateList_DDL"];
                string Region = form["RegionList_DDL"] == "0" ? "" : form["RegionList_DDL"];
                string Company = form["CompanyList_DDL"] == "0" ? "" : form["CompanyList_DDL"];
                string Division = form["DivisionList_DDL"] == "0" ? "" : form["DivisionList_DDL"];
                string Location = form["LocationList_DDL"] == "0" ? "" : form["LocationList_DDL"];
                string Department = form["DepartmentList_DDL"] == "0" ? "" : form["DepartmentList_DDL"];
                string Group = form["GroupList_DDL"] == "0" ? "" : form["GroupList_DDL"];
                string Unit = form["UnitList_DDL"] == "0" ? "" : form["UnitList_DDL"];
                string Grade = form["Grade_drop"] == "0" ? "" : form["Grade_drop"];
                string Level = form["Level_drop"] == "0" ? "" : form["Level_drop"];
                string EmpStatus = form["EmpStatus_drop"] == "0" ? "" : form["EmpStatus_drop"];
                string EmpActingStatus = form["EmpActingStatus_drop"] == "0" ? "" : form["EmpActingStatus_drop"];
                string JobName = form["JobName_drop"] == "0" ? "" : form["JobName_drop"];
                string Position = form["Position_drop"] == "0" ? "" : form["Position_drop"];
                string TimingCode = form["TimingCode_drop"] == "0" ? "" : form["TimingCode_drop"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);

                var CardCode = form["CardCode"] == null ? null : Convert.ToString(form["CardCode"]);
                int FatherName_FName = form["FatherName_id"] == "" ? 0 : Convert.ToInt32(form["FatherName_id"]);
                int MotherName_FName = form["MotherName_id"] == "" ? 0 : Convert.ToInt32(form["MotherName_id"]);
                int HusbandName_FName = form["HusbandName_id"] == "" ? 0 : Convert.ToInt32(form["HusbandName_id"]);
                int BeforeMarriageName_FName = form["BeforeMarriageName_id"] == "" ? 0 : Convert.ToInt32(form["BeforeMarriageName_id"]);

                string EmployeeDocumentslist = form["EmployeeDocumentslist"] == "0" ? "" : form["EmployeeDocumentslist"];

                using (DataBaseContext db = new DataBaseContext())
                {
                    Emp.EmpName = db.NameSingle.Find(EmpName_FName);
                    Emp.FatherName = db.NameSingle.Find(FatherName_FName);
                    Emp.MotherName = db.NameSingle.Find(MotherName_FName);
                    Emp.HusbandName = db.NameSingle.Find(HusbandName_FName);
                    Emp.BeforeMarriageName = db.NameSingle.Find(BeforeMarriageName_FName);
                    if (CardCode != null)
                    {
                        Emp.CardCode = CardCode;
                    }
                    {

                    }
                    if (MaritalStatus != null && MaritalStatus != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "128").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(MaritalStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(MaritalStatus));
                        Emp.MaritalStatus = val;
                    }

                    if (Gender != null && Gender != "")
                    {
                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "129").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Gender)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(Gender));
                        Emp.Gender = val;
                    }

                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    var CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                    if (CompCode != null)
                    {
                        if (CompCode.Code.ToUpper() == "KDCC" && CompCode.RegistrationDate != null)
                        {
                            string dob = "01/01/1940";
                            if (Emp.ServiceBookDates.JoiningDate < CompCode.RegistrationDate.Value.Date)
                            {
                                Msg.Add("Joining date Should be Grater Than bank Registration Date");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }
                            if (Emp.ServiceBookDates.BirthDate < Convert.ToDateTime(dob).Date)
                            {
                                Msg.Add("Birth date Should be Grater Than 01/01/1940");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            }

                        }
                    }
                    if (DateTime.Now < Emp.ServiceBookDates.BirthDate)
                    {
                        Msg.Add("Birth Should be Grater Than ToDay Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (Emp.ServiceBookDates.BirthDate > Emp.ServiceBookDates.JoiningDate)
                    {
                        Msg.Add("Birth Should be Grater Than ToDay Date");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.PensionJoingDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.PensionJoingDate))
                    {
                        Msg.Add("PensionJoingDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Emp.ServiceBookDates.PFExitDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.PFExitDate))
                    {
                        Msg.Add("PFExitDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.PensionExitDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.PensionExitDate))
                    {
                        Msg.Add("PensionExitDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.ServiceLastDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.ServiceLastDate))
                    {
                        Msg.Add("ServiceLastDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.ResignationDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.ResignationDate))
                    {
                        Msg.Add("ResignationDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.RetirementDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.RetirementDate))
                    {
                        Msg.Add("RetirementDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.SeniorityDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.SeniorityDate))
                    {
                        Msg.Add("SeniorityDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.LastTransferDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.LastTransferDate))
                    {
                        Msg.Add("LastTransferDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (Emp.ServiceBookDates.LastPromotionDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.LastPromotionDate))
                    {
                        Msg.Add("LastPromotionDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.LastIncrementDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.LastIncrementDate))
                    {
                        Msg.Add("LastIncrementDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.ConfirmationDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.ConfirmationDate))
                    {
                        Msg.Add("ConfirmationDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Emp.ServiceBookDates.ProbationDate != null && (Emp.ServiceBookDates.JoiningDate > Emp.ServiceBookDates.ProbationDate))
                    {
                        Msg.Add("ProbationDate Should be Grater Than JoiningDate");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (GetExtnServBook(Emp.EmpCode).Data != null)
                    {
                        ExtClass OExt = (ExtClass)GetExtnServBook(Emp.EmpCode).Data;
                        if (OExt.Chk == true)
                        {
                            if (OExt.PolicyName.ToUpper() == "RETIREMENT")
                            { Emp.ServiceBookDates.RetirementDate = OExt.Date; }
                            if (OExt.PolicyName.ToUpper() == "PROBATION")
                            { 
                                Emp.ServiceBookDates.ProbationDate = OExt.Date;
                                Emp.ServiceBookDates.ProbationPeriod = Convert.ToInt32(OExt.Period);
                            }
                            
                        }
                       
                    }
                    //if (ReportingHR != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingHR));
                    //    Emp.ReportingHR = val;
                    //}
                    //if (ReportingLevel1 != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel1));
                    //    Emp.ReportingLevel1 = val;
                    //}
                    //if (ReportingLevel2 != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel2));
                    //    Emp.ReportingLevel2 = val;
                    //}
                    //if (ReportingLevel3 != "")
                    //{
                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel3));
                    //    Emp.ReportingLevel3 = val;
                    //}

                    if (ReportingStructRights != null)
                    {
                        var ids = Utility.StringIdsToListIds(ReportingStructRights);
                        var reportingrightslist = new List<ReportingStructRights>();
                        foreach (var item in ids)
                        {
                            var id = Convert.ToInt32(item);
                            var qurey = db.ReportingStructRights.Find(id);
                            reportingrightslist.Add(qurey);

                        }

                        Emp.ReportingStructRights = reportingrightslist;
                    }
                    //Emp.PayStruct = new PayStruct();
                    //Emp.PayStruct.JobStatus = new JobStatus();
                    //if (EmpStatus != null && EmpStatus != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(EmpStatus));
                    //    Emp.PayStruct.JobStatus.EmpStatus = val;
                    //}

                    //if (EmpActingStatus != null && EmpActingStatus != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(EmpActingStatus));
                    //    Emp.PayStruct.JobStatus.EmpActingStatus = val;
                    //}

                    //if (Category != null && Category != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(Category));
                    //    Emp.Category = val;
                    //}

                    //if (Caste != null && Caste != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(Caste));
                    //    Emp.Caste = val;
                    //}

                    //if (Religion != null && Religion != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(Religion));
                    //    Emp.Religion = val;
                    //}

                    //if (BloodGroup != null && BloodGroup != "")
                    //{
                    //    var val = db.LookupValue.Find(int.Parse(BloodGroup));
                    //    Emp.BloodGroup = val;
                    //}

                    if (ResAddrs != null && ResAddrs != "")
                    {
                        int AddId = Convert.ToInt32(ResAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Emp.ResAddr = val;
                    }

                    if (PermAddrs != null && PermAddrs != "")
                    {
                        int AddId = Convert.ToInt32(PermAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Emp.PerAddr = val;
                    }

                    if (CorrsAddrs != null && CorrsAddrs != "")
                    {
                        int AddId = Convert.ToInt32(CorrsAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Emp.CorAddr = val;
                    }

                    if (ResContactDetails != null && ResContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ResContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Emp.ResContact = val;
                    }

                    if (PermContactDetails != null && PermContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(PermContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Emp.PerContact = val;
                    }
                    if (CorrsContactDetails != null && CorrsContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(CorrsContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Emp.CorContact = val;
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                                {
                                    Employee blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Employee.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                             .Include(e => e.EmpAcademicInfo).Include(e => e.EmpmedicalInfo).Include(e => e.EmpOffInfo)
                                             .Include(e => e.EmpSocialInfo).Include(e => e.FamilyDetails).Include(e => e.FatherName)
                                             .Include(e => e.ForeignTrips).Include(e => e.FuncStruct).Include(e => e.Gender).Include(e => e.GeoStruct)
                                             .Include(e => e.GuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                             .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.Nominees).Include(e => e.PassportDetails)
                                             .Include(e => e.PayStruct).Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.PreCompExp)
                                             .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.VisaDetails)
                                             .Include(e => e.WorkExp).Include(e => e.EmployeeDocuments).FirstOrDefault(e => e.Id == data);
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    Emp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    //public int EditS(string AccType, string PayMode, string bank,string branch, string national, string PAyProcGrp,int data, EmpOff EOff, DBTrack dbT)
                                    //int a = EditS(MaritalStatus, Gender, ResAddrs, PermAddrs, CorrsAddrs, ResContactDetails, PermContactDetails,
                                    //    CorrsContactDetails, ReportingHR, ReportingLevel1, ReportingLevel2, ReportingLevel3, ReportingStructRights
                                    //    , data, Emp, Emp.DBTrack);

                                    Employee typedetails = null;

                                    //if (ReportingHR != null & ReportingHR != "")
                                    //{
                                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingHR));
                                    //    Emp.ReportingHR = val;

                                    //    var type = db.Employee.Include(e => e.ReportingHR).Where(e => e.Id == data).SingleOrDefault();

                                    //    if (type.ReportingHR != null)
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.ReportingHR.Id == type.ReportingHR.Id && x.Id == data).SingleOrDefault();
                                    //    }
                                    //    else
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                    //    }
                                    //    typedetails.Gender = Emp.Gender;
                                    //}
                                    //else
                                    //{
                                    //    typedetails = db.Employee.Include(e => e.ReportingHR).Where(x => x.Id == data).SingleOrDefault();
                                    //    typedetails.ReportingHR = null;
                                    //}
                                    //if (ReportingLevel1 != null & ReportingLevel1 != "")
                                    //{
                                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel1));
                                    //    Emp.ReportingLevel1 = val;

                                    //    var type = db.Employee.Include(e => e.ReportingLevel1).Where(e => e.Id == data).SingleOrDefault();

                                    //    if (type.ReportingLevel1 != null)
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.ReportingLevel1.Id == type.ReportingLevel1.Id && x.Id == data).SingleOrDefault();
                                    //    }
                                    //    else
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                    //    }
                                    //    typedetails.Gender = Emp.Gender;
                                    //}
                                    //else
                                    //{
                                    //    typedetails = db.Employee.Include(e => e.ReportingLevel1).Where(x => x.Id == data).SingleOrDefault();
                                    //    typedetails.ReportingLevel1 = null;
                                    //}
                                    //if (ReportingLevel2 != null & ReportingLevel2 != "")
                                    //{
                                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel2));
                                    //    Emp.ReportingLevel2 = val;

                                    //    var type = db.Employee.Include(e => e.ReportingLevel2).Where(e => e.Id == data).SingleOrDefault();

                                    //    if (type.ReportingLevel2 != null)
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.ReportingLevel2.Id == type.ReportingLevel2.Id && x.Id == data).SingleOrDefault();
                                    //    }
                                    //    else
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                    //    }
                                    //    typedetails.Gender = Emp.Gender;
                                    //}
                                    //else
                                    //{
                                    //    typedetails = db.Employee.Include(e => e.ReportingLevel2).Where(x => x.Id == data).SingleOrDefault();
                                    //    typedetails.ReportingLevel2 = null;
                                    //}
                                    //if (ReportingLevel3 != null & ReportingLevel3 != "")
                                    //{
                                    //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel3));
                                    //    Emp.ReportingLevel3 = val;

                                    //    var type = db.Employee.Include(e => e.ReportingLevel3).Where(e => e.Id == data).SingleOrDefault();

                                    //    if (type.ReportingLevel3 != null)
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.ReportingLevel3.Id == type.ReportingLevel3.Id && x.Id == data).SingleOrDefault();
                                    //    }
                                    //    else
                                    //    {
                                    //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                    //    }
                                    //    typedetails.Gender = Emp.Gender;
                                    //}
                                    //else
                                    //{
                                    //    typedetails = db.Employee.Include(e => e.ReportingLevel3).Where(x => x.Id == data).SingleOrDefault();
                                    //    typedetails.ReportingLevel3 = null;
                                    //}
                                    List<ReportingStructRights> ReportingStructRightslist = new List<ReportingStructRights>();
                                    typedetails = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == data).SingleOrDefault();
                                    if (ReportingStructRights != null && ReportingStructRights != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(ReportingStructRights);
                                        foreach (var ca in ids)
                                        {
                                            var PayScaleval_val = db.ReportingStructRights.Find(ca);
                                            ReportingStructRightslist.Add(PayScaleval_val);
                                            typedetails.ReportingStructRights = ReportingStructRightslist;
                                        }
                                    }
                                    else
                                    {
                                        typedetails.ReportingStructRights = null;
                                    }
                                    //AccountType
                                    if (MaritalStatus != null & MaritalStatus != "")
                                    {
                                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "128").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(MaritalStatus)).FirstOrDefault();  //db.LookupValue.Find(int.Parse(MaritalStatus));
                                        Emp.MaritalStatus = val;

                                        var type = db.Employee.Include(e => e.MaritalStatus).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.MaritalStatus != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.MaritalStatus.Id == type.MaritalStatus.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.MaritalStatus = Emp.MaritalStatus;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.MaritalStatus).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.MaritalStatus = null;
                                    }
                                    /* end */

                                    //Paymode
                                    if (Gender != null & Gender != "")
                                    {
                                        var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "129").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Gender)).FirstOrDefault(); //db.LookupValue.Find(int.Parse(Gender));
                                        Emp.Gender = val;

                                        var type = db.Employee.Include(e => e.Gender).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.Gender != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.Gender.Id == type.Gender.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.Gender = Emp.Gender;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.Gender).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.Gender = null;
                                    }
                                    // Paymode end */


                                    if (ResAddrs != null & ResAddrs != "")
                                    {
                                        var val = db.Address.Find(int.Parse(ResAddrs));
                                        Emp.ResAddr = val;

                                        var type = db.Employee.Include(e => e.ResAddr).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.ResAddr != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.ResAddr.Id == type.ResAddr.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.ResAddr = Emp.ResAddr;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.ResAddr).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.ResAddr = null;
                                    }

                                    if (PermAddrs != null & PermAddrs != "")
                                    {
                                        var val = db.Address.Find(int.Parse(PermAddrs));
                                        Emp.PerAddr = val;

                                        var type = db.Employee.Include(e => e.PerAddr).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.PerAddr != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.PerAddr.Id == type.PerAddr.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.PerAddr = Emp.PerAddr;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.PerAddr).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.PerAddr = null;
                                    }


                                    if (CorrsAddrs != null & CorrsAddrs != "")
                                    {
                                        var val = db.Address.Find(int.Parse(CorrsAddrs));
                                        Emp.CorAddr = val;

                                        var type = db.Employee.Include(e => e.CorAddr).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.CorAddr != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.CorAddr.Id == type.CorAddr.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.CorAddr = Emp.CorAddr;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.CorAddr).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.CorAddr = null;
                                    }


                                    if (ResContactDetails != null & ResContactDetails != "")
                                    {
                                        var val = db.ContactDetails.Find(int.Parse(ResContactDetails));
                                        Emp.ResContact = val;

                                        var type = db.Employee.Include(e => e.ResContact).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.ResContact != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.ResContact.Id == type.ResContact.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.ResContact = Emp.ResContact;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.ResContact).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.ResContact = null;
                                    }

                                    if (PermContactDetails != null & PermContactDetails != "")
                                    {
                                        var val = db.ContactDetails.Find(int.Parse(PermContactDetails));
                                        Emp.PerContact = val;

                                        var type = db.Employee.Include(e => e.PerContact).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.PerContact != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.PerContact.Id == type.PerContact.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.PerContact = Emp.PerContact;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.PerContact).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.PerContact = null;
                                    }


                                    if (CorrsContactDetails != null & CorrsContactDetails != "")
                                    {
                                        var val = db.ContactDetails.Find(int.Parse(CorrsContactDetails));
                                        Emp.CorContact = val;

                                        var type = db.Employee.Include(e => e.CorContact).Where(e => e.Id == data).SingleOrDefault();

                                        if (type.CorContact != null)
                                        {
                                            typedetails = db.Employee.Where(x => x.CorContact.Id == type.CorContact.Id && x.Id == data).SingleOrDefault();
                                        }
                                        else
                                        {
                                            typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
                                        }
                                        typedetails.CorContact = Emp.CorContact;
                                    }
                                    else
                                    {
                                        typedetails = db.Employee.Include(e => e.CorContact).Where(x => x.Id == data).SingleOrDefault();
                                        typedetails.CorContact = null;
                                    }
                                    db.Employee.Attach(typedetails);
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = typedetails.RowVersion;
                                    db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                                    if (Emp.ServiceBookDates != null)
                                    {
                                        Employee Employee = db.Employee.Include(e => e.ServiceBookDates)
                                                                         .Where(e => e.Id == data).SingleOrDefault();
                                        Employee.ServiceBookDates.BirthDate = Emp.ServiceBookDates.BirthDate;
                                        Employee.ServiceBookDates.ConfirmationDate = Emp.ServiceBookDates.ConfirmationDate;
                                        Employee.ServiceBookDates.ConfirmPeriod = Emp.ServiceBookDates.ConfirmPeriod;
                                        Employee.ServiceBookDates.JoiningDate = Emp.ServiceBookDates.JoiningDate;
                                        Employee.ServiceBookDates.LastIncrementDate = Emp.ServiceBookDates.LastIncrementDate;
                                        Employee.ServiceBookDates.LastPromotionDate = Emp.ServiceBookDates.LastPromotionDate;
                                        Employee.ServiceBookDates.LastTransferDate = Emp.ServiceBookDates.LastTransferDate;
                                        Employee.ServiceBookDates.ProbationDate = Emp.ServiceBookDates.ProbationDate;
                                        Employee.ServiceBookDates.ProbationPeriod = Emp.ServiceBookDates.ProbationPeriod;
                                        Employee.ServiceBookDates.ResignationDate = Emp.ServiceBookDates.ResignationDate;
                                        Employee.ServiceBookDates.ResignReason = Emp.ServiceBookDates.ResignReason;
                                        Employee.ServiceBookDates.RetirementDate = Emp.ServiceBookDates.RetirementDate;
                                        Employee.ServiceBookDates.SeniorityDate = Emp.ServiceBookDates.SeniorityDate;
                                        Employee.ServiceBookDates.SeniorityNo = Emp.ServiceBookDates.SeniorityNo;
                                        Employee.ServiceBookDates.ServiceLastDate = Emp.ServiceBookDates.ServiceLastDate;
                                        Employee.ServiceBookDates.PFJoingDate = Emp.ServiceBookDates.PFJoingDate;
                                        Employee.ServiceBookDates.PensionJoingDate = Emp.ServiceBookDates.PensionJoingDate;
                                        Employee.ServiceBookDates.PFExitDate = Emp.ServiceBookDates.PFExitDate;
                                        Employee.ServiceBookDates.PensionExitDate = Emp.ServiceBookDates.PensionExitDate;
                                        db.Employee.Attach(Employee);
                                        db.Entry(Employee).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(Employee).State = System.Data.Entity.EntityState.Detached;
                                    }


                                    if (Emp.BeforeMarriageName != null)
                                    {
                                        var db_data = db.Employee.Include(e => e.BeforeMarriageName).Where(e => e.Id == data).SingleOrDefault();
                                        var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.BeforeMarriageName.Id).SingleOrDefault();
                                        db_data.BeforeMarriageName = NameObj;
                                        db.Employee.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    }


                                    if (Emp.EmpName != null)
                                    {
                                        var db_data = db.Employee.Include(e => e.EmpName).Where(e => e.Id == data).SingleOrDefault();

                                        var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.EmpName.Id).SingleOrDefault();
                                        db_data.EmpName = NameObj;
                                        db.Employee.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                    }


                                    if (Emp.FatherName != null)
                                    {
                                        var db_data = db.Employee.Include(e => e.EmpName).Where(e => e.Id == data).SingleOrDefault();
                                        var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.FatherName.Id).SingleOrDefault();
                                        db_data.FatherName = NameObj;
                                        db.Employee.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                    }


                                    if (Emp.HusbandName != null)
                                    {
                                        var db_data = db.Employee.Include(e => e.EmpName).Where(e => e.Id == data).SingleOrDefault();
                                        var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.HusbandName.Id).SingleOrDefault();
                                        db_data.HusbandName = NameObj;
                                        db.Employee.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    if (Emp.MotherName != null)
                                    {
                                        var db_data = db.Employee.Include(e => e.EmpName).Where(e => e.Id == data).SingleOrDefault();
                                        var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.MotherName.Id).SingleOrDefault();
                                        db_data.MotherName = NameObj;
                                        db.Employee.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                    }

                                    if (Emp.GeoStruct != null)
                                    {
                                        Employee Employee = db.Employee.Include(e => e.GeoStruct)
                                                                         .Where(e => e.Id == data).SingleOrDefault();

                                        Employee.GeoStruct = new GeoStruct
                                        {
                                            Company = Emp.GeoStruct.Company,
                                            Corporate = Emp.GeoStruct.Corporate,
                                            Department = Emp.GeoStruct.Department,
                                            Division = Emp.GeoStruct.Division,
                                            Group = Emp.GeoStruct.Group,
                                            Location = Emp.GeoStruct.Location,
                                            Region = Emp.GeoStruct.Region,
                                            Unit = Emp.GeoStruct.Unit
                                        };
                                    }

                                    if (Emp.FuncStruct != null)
                                    {
                                        Employee Employee = db.Employee.Include(e => e.FuncStruct)
                                                                         .Where(e => e.Id == data).SingleOrDefault();

                                        Employee.FuncStruct = new FuncStruct
                                        {
                                            Job = Emp.FuncStruct.Job,
                                            JobPosition = Emp.FuncStruct.JobPosition
                                        };
                                    }

                                    if (Emp.PayStruct != null)
                                    {
                                        Employee Employee = db.Employee.Include(e => e.PayStruct)
                                                                         .Where(e => e.Id == data).SingleOrDefault();

                                        Employee.PayStruct = new PayStruct
                                        {
                                            Grade = Emp.PayStruct.Grade,
                                            JobStatus = Emp.PayStruct.JobStatus,
                                            Level = Emp.PayStruct.Level
                                        };
                                    }

                                    List<EmployeeDocuments> ObjEmployeeDocuments = new List<EmployeeDocuments>();

                                    
                                    Employee ITdetails = db.Employee.Include(e => e.EmployeeDocuments).Where(e => e.Id == data).SingleOrDefault();
                                    if (EmployeeDocumentslist != null && EmployeeDocumentslist != "")
                                    {
                                        var ids = Utility.StringIdsToListIds(EmployeeDocumentslist);
                                        foreach (var ca in ids)
                                        {
                                            var ITSeclist = db.EmployeeDocuments.Find(ca);
                                            ObjEmployeeDocuments.Add(ITSeclist);
                                            ITdetails.EmployeeDocuments = ObjEmployeeDocuments;
                                        }
                                    }
                                    else
                                    {
                                        ITdetails.EmployeeDocuments = null;
                                    }


                                    var s = db.Employee.Where(e => e.Id == data).FirstOrDefault();
                                     
                                        // s.AppraisalPeriodCalendar = null;
                                        s.EmpCode = Emp.EmpCode == null ? "" : Emp.EmpCode.Trim();
                                        s.BirthPlace = Emp.BirthPlace == null ? "" : Emp.BirthPlace.Trim();
                                        s.CardCode = Emp.CardCode;
                                        s.Photo = Emp.Photo;
                                        s.TimingCode = Emp.TimingCode;
                                        s.ValidFromDate = Emp.ValidFromDate;
                                        s.ValidToDate = Emp.ValidToDate;
                                        s.DBTrack = Emp.DBTrack;
                                        db.Employee.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;

                                    ////var m1 = db.Employee.Where(e => e.Id == data).ToList();
                                    ////foreach (var s in m1)
                                    ////{
                                    ////    // s.AppraisalPeriodCalendar = null;
                                    ////    s.EmpCode = Emp.EmpCode == null ? "" : Emp.EmpCode.Trim();
                                    ////    s.BirthPlace = Emp.BirthPlace == null ? "" : Emp.BirthPlace.Trim();
                                    ////    s.CardCode = Emp.CardCode;
                                    ////    s.Photo = Emp.Photo;
                                    ////    s.TimingCode = Emp.TimingCode;
                                    ////    s.ValidFromDate = Emp.ValidFromDate;
                                    ////    s.ValidToDate = Emp.ValidToDate;
                                    ////    s.DBTrack = Emp.DBTrack;
                                    ////    db.Employee.Attach(s);
                                    ////    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    ////    //await db.SaveChangesAsync();
                                    ////    db.SaveChanges();
                                    ////    TempData["RowVersion"] = s.RowVersion;
                                    ////    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    ////}


                                    bool GeoFenceAppl = db.GeoFencing.FirstOrDefault() != null ? true : false;
                                    if (GeoFenceAppl == true)
                                    {
                                        int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                                        string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                                        using (SqlConnection con = new SqlConnection(connString))
                                        {
                                            using (SqlCommand cmd = new SqlCommand("Insert_EmpReporting", con))
                                            {
                                                cmd.CommandType = CommandType.StoredProcedure;

                                                cmd.Parameters.Add("@CompCode", SqlDbType.VarChar).Value = db.Company.Where(e => e.Id == Compid).SingleOrDefault().Code;
                                                cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = Emp.EmpCode;

                                                con.Open();
                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                    ts.Complete();
                                    Msg.Add(" Record Updated Successfully ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    //return Json(new Object[] { "", "", "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Corporate)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (Corporate)databaseEntry.ToObject();
                                    Emp.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            Employee blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            Employee Old_Emp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.Employee.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            Emp.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            Employee employee = new Employee()
                            {
                                EmpCode = Emp.EmpCode == null ? "" : Emp.EmpCode.Trim(),
                                BirthPlace = Emp.BirthPlace == null ? "" : Emp.BirthPlace.Trim(),
                                CardCode = Emp.CardCode,
                                Photo = Emp.Photo,
                                TimingCode = Emp.TimingCode,
                                ValidFromDate = Emp.ValidFromDate,
                                ValidToDate = Emp.ValidToDate,
                                DBTrack = Emp.DBTrack,
                                Id = data
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, employee, "EmpOff", Emp.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Emp = context.Employee.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                             .Include(e => e.EmpAcademicInfo).Include(e => e.EmpmedicalInfo).Include(e => e.EmpOffInfo)
                                             .Include(e => e.EmpSocialInfo).Include(e => e.FamilyDetails).Include(e => e.FatherName)
                                             .Include(e => e.ForeignTrips).Include(e => e.FuncStruct).Include(e => e.Gender).Include(e => e.GeoStruct)
                                             .Include(e => e.GuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                             .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.Nominees).Include(e => e.PassportDetails)
                                             .Include(e => e.PayStruct).Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.PreCompExp)
                                             .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.VisaDetails)
                                             .Include(e => e.WorkExp).FirstOrDefault(e => e.Id == data);
                                DT_Employee DT_Emp = (DT_Employee)obj;
                                DT_Emp.EmpName_Id = DBTrackFile.ValCompare(Old_Emp.EmpName, Emp.EmpName);
                                DT_Emp.MotherName_Id = DBTrackFile.ValCompare(Old_Emp.MotherName, Emp.MotherName);
                                DT_Emp.FatherName_Id = DBTrackFile.ValCompare(Old_Emp.FatherName, Emp.FatherName);
                                DT_Emp.HusbandName_Id = DBTrackFile.ValCompare(Old_Emp.HusbandName, Emp.HusbandName);
                                DT_Emp.BeforeMarriageName_Id = DBTrackFile.ValCompare(Old_Emp.BeforeMarriageName, Emp.BeforeMarriageName);
                                DT_Emp.GeoStruct_Id = DBTrackFile.ValCompare(Old_Emp.GeoStruct, Emp.GeoStruct);
                                DT_Emp.FuncStruct_Id = DBTrackFile.ValCompare(Old_Emp.FuncStruct, Emp.FuncStruct);
                                DT_Emp.PayStruct_Id = DBTrackFile.ValCompare(Old_Emp.PayStruct, Emp.PayStruct);
                                DT_Emp.CorAddr_Id = DBTrackFile.ValCompare(Old_Emp.CorAddr, Emp.CorAddr);
                                DT_Emp.CorContact_Id = DBTrackFile.ValCompare(Old_Emp.CorContact, Emp.CorContact);
                                DT_Emp.PerAddr_Id = DBTrackFile.ValCompare(Old_Emp.PerAddr, Emp.PerAddr);
                                DT_Emp.PerContact_Id = DBTrackFile.ValCompare(Old_Emp.PerContact, Emp.PerContact);
                                DT_Emp.ResAddr_Id = DBTrackFile.ValCompare(Old_Emp.ResAddr, Emp.ResAddr);
                                DT_Emp.ResContact_Id = DBTrackFile.ValCompare(Old_Emp.ResContact, Emp.ResContact);
                                DT_Emp.ServiceBookDates_Id = DBTrackFile.ValCompare(Old_Emp.ServiceBookDates, Emp.ServiceBookDates);
                                DT_Emp.Nominees_Id = DBTrackFile.ValCompare(Old_Emp.Nominees, Emp.Nominees);
                                DT_Emp.EmpmedicalInfo_Id = DBTrackFile.ValCompare(Old_Emp.EmpmedicalInfo, Emp.EmpmedicalInfo);
                                DT_Emp.EmpSocialInfo_Id = DBTrackFile.ValCompare(Old_Emp.EmpSocialInfo, Emp.EmpSocialInfo);
                                DT_Emp.EmpAcademicInfo_Id = DBTrackFile.ValCompare(Old_Emp.EmpAcademicInfo, Emp.EmpAcademicInfo);
                                //DT_Emp.PassportDetails_Id =  DBTrackFile.ValCompare(Old_Emp.PassportDetails, Emp.PassportDetails);
                                //DT_Emp.VisaDetails_Id =  DBTrackFile.ValCompare(Old_Emp.VisaDetails, Emp.VisaDetails);
                                //DT_Emp.ForeignTrips_Id =  DBTrackFile.ValCompare(Old_Emp.ForeignTrips, Emp.ForeignTrips);
                                //DT_Emp.FamilyDetails_Id = DBTrackFile.ValCompare(Old_Emp.FamilyDetails, Emp.FamilyDetails);
                                //DT_Emp.GuarantorDetails_Id =  DBTrackFile.ValCompare(Old_Emp.GuarantorDetails, Emp.GuarantorDetails);
                                //DT_Emp.InsuranceDetails_Id =  DBTrackFile.ValCompare(Old_Emp.InsuranceDetails, Emp.InsuranceDetails);
                                DT_Emp.Gender_Id = DBTrackFile.ValCompare(Old_Emp.Gender, Emp.Gender);
                                DT_Emp.MaritalStatus_Id = DBTrackFile.ValCompare(Old_Emp.MaritalStatus, Emp.MaritalStatus);
                                db.Create(DT_Emp);
                            }
                            blog.DBTrack = Emp.DBTrack;
                            db.Employee.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = Emp.EmpName.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { blog.Id, Emp.EmpName.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
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
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    Employee Emp = db.Employee.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                             .Include(e => e.EmpAcademicInfo).Include(e => e.EmpmedicalInfo).Include(e => e.EmpOffInfo)
                                             .Include(e => e.EmpSocialInfo).Include(e => e.FamilyDetails).Include(e => e.FatherName)
                                             .Include(e => e.ForeignTrips).Include(e => e.FuncStruct).Include(e => e.Gender).Include(e => e.GeoStruct)
                                             .Include(e => e.GuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                             .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.Nominees).Include(e => e.PassportDetails)
                                             .Include(e => e.PayStruct).Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.PreCompExp)
                                             .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.VisaDetails)
                                             .Include(e => e.WorkExp).Include(e => e.EmployeeDocuments).FirstOrDefault(e => e.Id == data);

                    //Address add = corporates.Address;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //LookupValue val = corporates.BusinessType;

                    NameSingle EmpName = Emp.EmpName;
                    NameSingle MotherName = Emp.EmpName;
                    NameSingle FatherName = Emp.EmpName;
                    NameSingle HusbandName = Emp.EmpName;
                    NameSingle BeforeMarriageName = Emp.EmpName;
                    GeoStruct GeoStruct = Emp.GeoStruct;
                    FuncStruct FuncStruct = Emp.FuncStruct;
                    PayStruct PayStruct = Emp.PayStruct;
                    Address CorAddr = Emp.CorAddr;
                    ContactDetails CorContact = Emp.CorContact;
                    Address PerAddr = Emp.PerAddr;
                    ContactDetails PerContact = Emp.PerContact;
                    Address ResAddr = Emp.ResAddr;
                    ContactDetails ResContact = Emp.ResContact;
                    ServiceBookDates ServiceBookDates = Emp.ServiceBookDates;
                    //Nominees Nominees = Emp.Nominees;
                    EmpMedicalInfo EmpmedicalInfo = Emp.EmpmedicalInfo;
                    EmpSocialInfo EmpSocialInfo = Emp.EmpSocialInfo;
                    EmpAcademicInfo EmpAcademicInfo = Emp.EmpAcademicInfo;
                    LookupValue Gender = Emp.Gender;
                    LookupValue MaritalStatus = Emp.MaritalStatus;



                    //
                    var v2 = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.EmpSalStruct).Where(e => e.Employee.Id == Emp.Id).FirstOrDefault();
                    if (v2.EmpSalStruct.Count != 0)
                    {
                        Msg.Add("Salary Structure is Defined,So Unable to Delete ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    var v = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == Emp.Id).FirstOrDefault();
                    if (v != null)
                    {
                        db.EmployeePayroll.Remove(v);
                        db.SaveChanges();
                    }


                    var v1 = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == Emp.Id).FirstOrDefault();
                    if (v1 != null)
                    {
                        db.EmployeeLeave.Remove(v1);
                        db.SaveChanges();
                    }

                    if (Emp.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = Emp.DBTrack.CreatedBy != null ? Emp.DBTrack.CreatedBy : null,
                                CreatedOn = Emp.DBTrack.CreatedOn != null ? Emp.DBTrack.CreatedOn : null,
                                IsModified = Emp.DBTrack.IsModified == true ? true : false
                            };
                            Emp.DBTrack = dbT;
                            db.Entry(Emp).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Emp.DBTrack);
                            DT_Employee DT_Emp = (DT_Employee)rtn_Obj;
                            DT_Emp.EmpName_Id = Emp.EmpName == null ? 0 : Emp.EmpName.Id;
                            DT_Emp.MotherName_Id = Emp.MotherName == null ? 0 : Emp.MotherName.Id;
                            DT_Emp.FatherName_Id = Emp.FatherName == null ? 0 : Emp.FatherName.Id;
                            DT_Emp.HusbandName_Id = Emp.HusbandName == null ? 0 : Emp.HusbandName.Id;
                            DT_Emp.BeforeMarriageName_Id = Emp.BeforeMarriageName == null ? 0 : Emp.BeforeMarriageName.Id;
                            DT_Emp.GeoStruct_Id = Emp.GeoStruct == null ? 0 : Emp.GeoStruct.Id;
                            DT_Emp.FuncStruct_Id = Emp.FuncStruct == null ? 0 : Emp.FuncStruct.Id;
                            DT_Emp.PayStruct_Id = Emp.PayStruct == null ? 0 : Emp.PayStruct.Id;
                            DT_Emp.CorAddr_Id = Emp.CorAddr == null ? 0 : Emp.CorAddr.Id;
                            DT_Emp.CorContact_Id = Emp.CorContact == null ? 0 : Emp.CorContact.Id;
                            DT_Emp.PerAddr_Id = Emp.PerAddr == null ? 0 : Emp.PerAddr.Id;
                            DT_Emp.PerContact_Id = Emp.PerContact == null ? 0 : Emp.PerContact.Id;
                            DT_Emp.ResAddr_Id = Emp.ResAddr == null ? 0 : Emp.ResAddr.Id;
                            DT_Emp.ResContact_Id = Emp.ResContact == null ? 0 : Emp.ResContact.Id;
                            DT_Emp.ServiceBookDates_Id = Emp.ServiceBookDates == null ? 0 : Emp.ServiceBookDates.Id;
                            //   DT_Emp.Nominees_Id = Emp.Nominees == null ? 0 : Emp.Nominees.Id;
                            DT_Emp.EmpmedicalInfo_Id = Emp.EmpmedicalInfo == null ? 0 : Emp.EmpmedicalInfo.Id;
                            DT_Emp.EmpSocialInfo_Id = Emp.EmpSocialInfo == null ? 0 : Emp.EmpSocialInfo.Id;
                            DT_Emp.EmpAcademicInfo_Id = Emp.EmpAcademicInfo == null ? 0 : Emp.EmpAcademicInfo.Id;
                            //DT_Emp.PassportDetails_Id = Emp.PassportDetails == null ? 0 : Emp.PassportDetails.Id;
                            //DT_Emp.VisaDetails_Id = Emp.VisaDetails == null ? 0 : Emp.VisaDetails.Id;
                            //DT_Emp.ForeignTrips_Id = Emp.ForeignTrips == null ? 0 : Emp.ForeignTrips.Id;
                            //DT_Emp.FamilyDetails_Id = Emp.FamilyDetails == null ? 0 : Emp.FamilyDetails.Id;
                            //DT_Emp.GuarantorDetails_Id = Emp.GuarantorDetails == null ? 0 : Emp.GuarantorDetails.Id;
                            //DT_Emp.InsuranceDetails_Id = Emp.InsuranceDetails == null ? 0 : Emp.InsuranceDetails.Id;
                            DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Emp.Gender.Id;
                            DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : Emp.MaritalStatus.Id;

                            db.Create(DT_Emp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data will be removed after Authorized  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data will be removed after Authorized.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = Emp.DBTrack.CreatedBy != null ? Emp.DBTrack.CreatedBy : null,
                                    CreatedOn = Emp.DBTrack.CreatedOn != null ? Emp.DBTrack.CreatedOn : null,
                                    IsModified = Emp.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                db.Entry(Emp).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                DT_Employee DT_Emp = (DT_Employee)rtn_Obj;
                                DT_Emp.EmpName_Id = EmpName == null ? 0 : EmpName.Id;
                                DT_Emp.MotherName_Id = MotherName == null ? 0 : MotherName.Id;
                                DT_Emp.FatherName_Id = FatherName == null ? 0 : FatherName.Id;
                                DT_Emp.HusbandName_Id = HusbandName == null ? 0 : HusbandName.Id;
                                DT_Emp.BeforeMarriageName_Id = BeforeMarriageName == null ? 0 : BeforeMarriageName.Id;
                                DT_Emp.GeoStruct_Id = GeoStruct == null ? 0 : GeoStruct.Id;
                                DT_Emp.FuncStruct_Id = FuncStruct == null ? 0 : FuncStruct.Id;
                                DT_Emp.PayStruct_Id = PayStruct == null ? 0 : PayStruct.Id;
                                DT_Emp.CorAddr_Id = CorAddr == null ? 0 : CorAddr.Id;
                                DT_Emp.CorContact_Id = CorContact == null ? 0 : CorContact.Id;
                                DT_Emp.PerAddr_Id = PerAddr == null ? 0 : PerAddr.Id;
                                DT_Emp.PerContact_Id = PerContact == null ? 0 : PerContact.Id;
                                DT_Emp.ResAddr_Id = ResAddr == null ? 0 : ResAddr.Id;
                                DT_Emp.ResContact_Id = ResContact == null ? 0 : ResContact.Id;
                                DT_Emp.ServiceBookDates_Id = ServiceBookDates == null ? 0 : ServiceBookDates.Id;
                                // DT_Emp.Nominees_Id = Nominees == null ? 0 : Nominees.Id;
                                DT_Emp.EmpmedicalInfo_Id = EmpmedicalInfo == null ? 0 : EmpmedicalInfo.Id;
                                DT_Emp.EmpSocialInfo_Id = EmpSocialInfo == null ? 0 : EmpSocialInfo.Id;
                                DT_Emp.EmpAcademicInfo_Id = EmpAcademicInfo == null ? 0 : EmpAcademicInfo.Id;
                                //DT_Emp.PassportDetails_Id = Emp.PassportDetails == null ? 0 : Emp.PassportDetails.Id;
                                //DT_Emp.VisaDetails_Id = Emp.VisaDetails == null ? 0 : Emp.VisaDetails.Id;
                                //DT_Emp.ForeignTrips_Id = Emp.ForeignTrips == null ? 0 : Emp.ForeignTrips.Id;
                                //DT_Emp.FamilyDetails_Id = Emp.FamilyDetails == null ? 0 : Emp.FamilyDetails.Id;
                                //DT_Emp.GuarantorDetails_Id = Emp.GuarantorDetails == null ? 0 : Emp.GuarantorDetails.Id;
                                //DT_Emp.InsuranceDetails_Id = Emp.InsuranceDetails == null ? 0 : Emp.InsuranceDetails.Id;
                                // DT_Emp.ValidFromDate = Emp.ValidFromDate.Value.ToShortDateString() ==0;
                                DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Gender.Id;
                                DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : MaritalStatus.Id;

                                db.Create(DT_Emp);
                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            Employee Emp = db.Employee.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                             .Include(e => e.EmpAcademicInfo).Include(e => e.EmpmedicalInfo).Include(e => e.EmpOffInfo)
                                             .Include(e => e.EmpSocialInfo).Include(e => e.FamilyDetails).Include(e => e.FatherName)
                                             .Include(e => e.ForeignTrips).Include(e => e.FuncStruct).Include(e => e.Gender).Include(e => e.GeoStruct)
                                             .Include(e => e.GuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                             .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.Nominees).Include(e => e.PassportDetails)
                                             .Include(e => e.PayStruct).Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.PreCompExp)
                                             .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.VisaDetails)
                                             .Include(e => e.WorkExp).FirstOrDefault(e => e.Id == auth_id);

                            Emp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = Emp.DBTrack.ModifiedBy != null ? Emp.DBTrack.ModifiedBy : null,
                                CreatedBy = Emp.DBTrack.CreatedBy != null ? Emp.DBTrack.CreatedBy : null,
                                CreatedOn = Emp.DBTrack.CreatedOn != null ? Emp.DBTrack.CreatedOn : null,
                                IsModified = Emp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Employee.Attach(Emp);
                            db.Entry(Emp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(Emp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Emp.DBTrack);
                            DT_Employee DT_Emp = (DT_Employee)rtn_Obj;
                            DT_Emp.EmpName_Id = Emp.EmpName == null ? 0 : Emp.EmpName.Id;
                            DT_Emp.MotherName_Id = Emp.MotherName == null ? 0 : Emp.MotherName.Id;
                            DT_Emp.FatherName_Id = Emp.FatherName == null ? 0 : Emp.FatherName.Id;
                            DT_Emp.HusbandName_Id = Emp.HusbandName == null ? 0 : Emp.HusbandName.Id;
                            DT_Emp.BeforeMarriageName_Id = Emp.BeforeMarriageName == null ? 0 : Emp.BeforeMarriageName.Id;
                            DT_Emp.GeoStruct_Id = Emp.GeoStruct == null ? 0 : Emp.GeoStruct.Id;
                            DT_Emp.FuncStruct_Id = Emp.FuncStruct == null ? 0 : Emp.FuncStruct.Id;
                            DT_Emp.PayStruct_Id = Emp.PayStruct == null ? 0 : Emp.PayStruct.Id;
                            DT_Emp.CorAddr_Id = Emp.CorAddr == null ? 0 : Emp.CorAddr.Id;
                            DT_Emp.CorContact_Id = Emp.CorContact == null ? 0 : Emp.CorContact.Id;
                            DT_Emp.PerAddr_Id = Emp.PerAddr == null ? 0 : Emp.PerAddr.Id;
                            DT_Emp.PerContact_Id = Emp.PerContact == null ? 0 : Emp.PerContact.Id;
                            DT_Emp.ResAddr_Id = Emp.ResAddr == null ? 0 : Emp.ResAddr.Id;
                            DT_Emp.ResContact_Id = Emp.ResContact == null ? 0 : Emp.ResContact.Id;
                            DT_Emp.ServiceBookDates_Id = Emp.ServiceBookDates == null ? 0 : Emp.ServiceBookDates.Id;
                            //   DT_Emp.Nominees_Id = Emp.Nominees == null ? 0 : Emp.Nominees.Id;
                            DT_Emp.EmpmedicalInfo_Id = Emp.EmpmedicalInfo == null ? 0 : Emp.EmpmedicalInfo.Id;
                            DT_Emp.EmpSocialInfo_Id = Emp.EmpSocialInfo == null ? 0 : Emp.EmpSocialInfo.Id;
                            DT_Emp.EmpAcademicInfo_Id = Emp.EmpAcademicInfo == null ? 0 : Emp.EmpAcademicInfo.Id;
                            //DT_Emp.PassportDetails_Id = Emp.PassportDetails == null ? 0 : Emp.PassportDetails.Id;
                            //DT_Emp.VisaDetails_Id = Emp.VisaDetails == null ? 0 : Emp.VisaDetails.Id;
                            //DT_Emp.ForeignTrips_Id = Emp.ForeignTrips == null ? 0 : Emp.ForeignTrips.Id;
                            //DT_Emp.FamilyDetails_Id = Emp.FamilyDetails == null ? 0 : Emp.FamilyDetails.Id;
                            //DT_Emp.GuarantorDetails_Id = Emp.GuarantorDetails == null ? 0 : Emp.GuarantorDetails.Id;
                            //DT_Emp.InsuranceDetails_Id = Emp.InsuranceDetails == null ? 0 : Emp.InsuranceDetails.Id;
                            DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Emp.Gender.Id;
                            DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : Emp.MaritalStatus.Id;
                            db.Create(DT_Emp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("Record Authorized");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { Emp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Employee Old_Emp = db.Employee.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                                .Include(e => e.EmpAcademicInfo).Include(e => e.EmpmedicalInfo).Include(e => e.EmpOffInfo)
                                                .Include(e => e.EmpSocialInfo).Include(e => e.FamilyDetails).Include(e => e.FatherName)
                                                .Include(e => e.ForeignTrips).Include(e => e.FuncStruct).Include(e => e.Gender).Include(e => e.GeoStruct)
                                                .Include(e => e.GuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                                .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.Nominees).Include(e => e.PassportDetails)
                                                .Include(e => e.PayStruct).Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.PreCompExp)
                                                .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.VisaDetails)
                                                .Include(e => e.WorkExp).FirstOrDefault(e => e.Id == auth_id);


                        DT_Employee Curr_Emp = db.DT_Employee.FirstOrDefault(e => e.Id == auth_id);

                        if (Curr_Emp != null)
                        {
                            Employee Emp = new Employee();

                            string MaritalStatus = Curr_Emp.MaritalStatus_Id == null ? null : Curr_Emp.MaritalStatus_Id.ToString();
                            string Gender = Curr_Emp.Gender_Id == null ? null : Curr_Emp.Gender_Id.ToString();
                            string ResAddrs = Curr_Emp.ResAddr_Id == null ? null : Curr_Emp.ResAddr_Id.ToString();
                            string ResContactDetails = Curr_Emp.ResContact_Id == null ? null : Curr_Emp.ResContact_Id.ToString();
                            string PermAddrs = Curr_Emp.PerAddr_Id == null ? null : Curr_Emp.PerAddr_Id.ToString();
                            string PermContactDetails = Curr_Emp.PerContact_Id == null ? null : Curr_Emp.PerContact_Id.ToString();
                            string CorrsAddrs = Curr_Emp.CorAddr_Id == null ? null : Curr_Emp.CorAddr_Id.ToString();
                            string CorrsContactDetails = Curr_Emp.CorContact_Id == null ? null : Curr_Emp.CorContact_Id.ToString();

                            Emp.BirthPlace = Curr_Emp.BirthPlace == null ? Old_Emp.BirthPlace : Curr_Emp.BirthPlace;
                            Emp.CardCode = Curr_Emp.CardCode == null ? Old_Emp.CardCode : Curr_Emp.CardCode;
                            Emp.EmpCode = Curr_Emp.EmpCode == null ? Old_Emp.BirthPlace : Curr_Emp.EmpCode;
                            Emp.TimingCode = Curr_Emp.TimingCode == null ? Old_Emp.TimingCode : Curr_Emp.TimingCode;
                            Emp.ValidFromDate = Curr_Emp.ValidFromDate == null ? Old_Emp.ValidFromDate : Curr_Emp.ValidFromDate;
                            Emp.ValidToDate = Curr_Emp.ValidToDate == null ? Old_Emp.ValidToDate : Curr_Emp.ValidToDate;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        Emp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Emp.DBTrack.CreatedBy == null ? null : Old_Emp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Emp.DBTrack.CreatedOn == null ? null : Old_Emp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Emp.DBTrack.ModifiedBy == null ? null : Old_Emp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Emp.DBTrack.ModifiedOn == null ? null : Old_Emp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        // int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);

                                        //int a = EditS(MaritalStatus, Gender, ResAddrs, PermAddrs, CorrsAddrs, ResContactDetails, PermContactDetails, CorrsContactDetails, ReportingHR, ReportingLevel1, ReportingLevel2, ReportingLevel3, ReportingStructRights auth_id, Emp, Emp.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("Record Authorized");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { Emp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add("Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
                                        Emp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //  return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                            Msg.Add("Data removed from history");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            Employee Emp = db.Employee.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                             .Include(e => e.EmpAcademicInfo).Include(e => e.EmpmedicalInfo).Include(e => e.EmpOffInfo)
                                             .Include(e => e.EmpSocialInfo).Include(e => e.FamilyDetails).Include(e => e.FatherName)
                                             .Include(e => e.ForeignTrips).Include(e => e.FuncStruct).Include(e => e.Gender).Include(e => e.GeoStruct)
                                             .Include(e => e.GuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                             .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.Nominees).Include(e => e.PassportDetails)
                                             .Include(e => e.PayStruct).Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.PreCompExp)
                                             .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.VisaDetails)
                                             .Include(e => e.WorkExp).FirstOrDefault(e => e.Id == auth_id);

                            //Address add = corp.Address;
                            //ContactDetails conDet = corp.ContactDetails;
                            //LookupValue val = corp.BusinessType;

                            Emp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = Emp.DBTrack.ModifiedBy != null ? Emp.DBTrack.ModifiedBy : null,
                                CreatedBy = Emp.DBTrack.CreatedBy != null ? Emp.DBTrack.CreatedBy : null,
                                CreatedOn = Emp.DBTrack.CreatedOn != null ? Emp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Employee.Attach(Emp);
                            db.Entry(Emp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, Emp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Employee DT_Emp = (DT_Employee)rtn_Obj;
                            DT_Emp.EmpName_Id = Emp.EmpName == null ? 0 : Emp.EmpName.Id;
                            DT_Emp.MotherName_Id = Emp.MotherName == null ? 0 : Emp.MotherName.Id;
                            DT_Emp.FatherName_Id = Emp.FatherName == null ? 0 : Emp.FatherName.Id;
                            DT_Emp.HusbandName_Id = Emp.HusbandName == null ? 0 : Emp.HusbandName.Id;
                            DT_Emp.BeforeMarriageName_Id = Emp.BeforeMarriageName == null ? 0 : Emp.BeforeMarriageName.Id;
                            DT_Emp.GeoStruct_Id = Emp.GeoStruct == null ? 0 : Emp.GeoStruct.Id;
                            DT_Emp.FuncStruct_Id = Emp.FuncStruct == null ? 0 : Emp.FuncStruct.Id;
                            DT_Emp.PayStruct_Id = Emp.PayStruct == null ? 0 : Emp.PayStruct.Id;
                            DT_Emp.CorAddr_Id = Emp.CorAddr == null ? 0 : Emp.CorAddr.Id;
                            DT_Emp.CorContact_Id = Emp.CorContact == null ? 0 : Emp.CorContact.Id;
                            DT_Emp.PerAddr_Id = Emp.PerAddr == null ? 0 : Emp.PerAddr.Id;
                            DT_Emp.PerContact_Id = Emp.PerContact == null ? 0 : Emp.PerContact.Id;
                            DT_Emp.ResAddr_Id = Emp.ResAddr == null ? 0 : Emp.ResAddr.Id;
                            DT_Emp.ResContact_Id = Emp.ResContact == null ? 0 : Emp.ResContact.Id;
                            DT_Emp.ServiceBookDates_Id = Emp.ServiceBookDates == null ? 0 : Emp.ServiceBookDates.Id;
                            //  DT_Emp.Nominees_Id = Emp.Nominees == null ? 0 : Emp.Nominees.Id;
                            DT_Emp.EmpmedicalInfo_Id = Emp.EmpmedicalInfo == null ? 0 : Emp.EmpmedicalInfo.Id;
                            DT_Emp.EmpSocialInfo_Id = Emp.EmpSocialInfo == null ? 0 : Emp.EmpSocialInfo.Id;
                            DT_Emp.EmpAcademicInfo_Id = Emp.EmpAcademicInfo == null ? 0 : Emp.EmpAcademicInfo.Id;
                            //DT_Emp.PassportDetails_Id = Emp.PassportDetails == null ? 0 : Emp.PassportDetails.Id;
                            //DT_Emp.VisaDetails_Id = Emp.VisaDetails == null ? 0 : Emp.VisaDetails.Id;
                            //DT_Emp.ForeignTrips_Id = Emp.ForeignTrips == null ? 0 : Emp.ForeignTrips.Id;
                            //DT_Emp.FamilyDetails_Id = Emp.FamilyDetails == null ? 0 : Emp.FamilyDetails.Id;
                            //DT_Emp.GuarantorDetails_Id = Emp.GuarantorDetails == null ? 0 : Emp.GuarantorDetails.Id;
                            //DT_Emp.InsuranceDetails_Id = Emp.InsuranceDetails == null ? 0 : Emp.InsuranceDetails.Id;
                            DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Emp.Gender.Id;
                            DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : Emp.MaritalStatus.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(Emp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add("Record Authorized");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //  return Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
                        }

                    }
                    return View();
                }
            }
            catch (Exception ex)
            {
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
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<Employee> Employee = null;
                if (gp.IsAutho == true)
                {
                    Employee = db.Employee.Include(e => e.EmpName).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Employee = db.Employee.Include(e => e.EmpName).AsNoTracking().ToList();
                }

                IEnumerable<Employee> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {

                    IE = Employee;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                             || (e.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                             || (e.Id.ToString().Contains(gp.searchString))
                             ).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                        //jsonData = IE.Where(e => (e.Id.ToString() == gp.searchString) || (e.EmpCode.ToLower() == gp.searchString.ToLower()) || (e.EmpName.FullNameFML.ToLower() == gp.searchString.ToLower())).Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName.FullNameFML }).ToList();
                        //jsonData = IE.Where(e => (e.EmpCode.ToLower() == gp.searchString.ToLower()) || (e.EmpName.FullNameFML.ToLower().Contains(gp.searchString.ToLower())) || (e.Id.ToString() == gp.searchString)).Select(a => new Object[] { a.EmpCode, a.EmpName.FullNameFML, a.Id }).ToList();
                        //  jsonData = IE.Select(a => new { a.Id, a.EmpCode, a.EmpName }).Where((e => (e.Id.ToString() == gp.searchString) || (e.EmpCode.ToLower() == gp.searchString.ToLower()) || (e.EmpName.FullNameFML.ToLower() == gp.searchString.ToLower()))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName == null ? "" : a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Employee;
                    Func<Employee, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmpCode :
                                         gp.sidx == "EmpName" ? c.EmpName.FullNameFML : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName == null ? "": a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.EmpCode), a.EmpName == null ? "" : a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName == null ? "" : a.EmpName.FullNameFML, a.Id }).ToList();
                    }
                    totalRecords = Employee.Count();
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
                throw ex;
            }
        }
        public ActionResult GetLookup_Name()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lall = db.NameSingle.Select(e => e.Id.ToString()).Distinct().ToList();
                var rall = db.Employee.Include(e => e.EmpName).Where(e => e.EmpName != null).Select(e => e.EmpName.Id.ToString()).Distinct().ToList();
                var all = lall.Except(rall);
                var EmpList = new List<NameSingle>();
                foreach (var item in all)
                {
                    EmpList.Add(db.NameSingle.Find(Convert.ToInt32(item)));
                }
                var r = (from ca in EmpList select new { srno = ca.Id, lookupvalue = ca.FullNameFML }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetRetirementDay(string data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var a = db.RetirementDay.SingleOrDefault();
                    string RetirementDate = "";
                    if (a != null)
                    {
                        int Year = a.Year;
                        DateTime? RetirDate = Convert.ToDateTime(data).AddYears(Year);
                        if (a.ServiceEndMonLastDay == true)
                        {
                            string Day = RetirDate.Value.Day.ToString().Length == 1 ? "0" + RetirDate.Value.Day.ToString() : RetirDate.Value.Day.ToString();
                            if (Day == "01")
                            {
                                if (a.FirstdayExclude==true)
                                {
                                    var lastDayOfMonth = DateTime.DaysInMonth(RetirDate.Value.Year, RetirDate.Value.Month);
                                    string Month = RetirDate.Value.Month.ToString().Length == 1 ? "0" + RetirDate.Value.Month.ToString() : RetirDate.Value.Month.ToString();
                                    RetirementDate = lastDayOfMonth + "/" + Month + "/" + RetirDate.Value.Year;
                                }
                                else
                                {
                                    RetirDate = RetirDate.Value.AddDays(-1);
                                    RetirementDate = RetirDate.Value.ToShortDateString();
                                }
                              
                            }
                            else
                            {
                                var lastDayOfMonth = DateTime.DaysInMonth(RetirDate.Value.Year, RetirDate.Value.Month);
                                string Month = RetirDate.Value.Month.ToString().Length == 1 ? "0" + RetirDate.Value.Month.ToString() : RetirDate.Value.Month.ToString();
                                RetirementDate = lastDayOfMonth + "/" + Month + "/" + RetirDate.Value.Year;
                            }

                        }
                        if (a.ServiceEndMonRegDay == true)
                        {
                            RetirDate = RetirDate.Value.AddDays(-1);
                            RetirementDate = RetirDate.Value.ToShortDateString();
                        }
                    }
                    return Json(RetirementDate, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
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

        public class ExtClass
        {
            public bool Chk { get; set; }
            public DateTime? Date { get; set; }
            public string PolicyName { get; set; }
            public string Period { get; set; }
        }
        


        public JsonResult GetExtnServBook(string data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    List<ExtnRednServiceBook> OExtnRednServiceBook = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.ExtnRednServiceBook).Include(e => e.ExtnRednServiceBook.Select(t => t.ExtnRednActivity))
                        .Include(e => e.ExtnRednServiceBook.Select(t => t.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType))
                        .Where(e => e.Employee.EmpCode == data).FirstOrDefault().ExtnRednServiceBook.ToList();

                    Employee OEmp = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.EmpCode == data).FirstOrDefault();

                    bool chk = false;
                    ExtClass OExtClass = new ExtClass();
                    if (OExtnRednServiceBook.Count() > 0)
                    {
                        if (OExtnRednServiceBook.Where(e => e.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "RETIREMENTPERIOD" && e.ExpiryDate >= OEmp.ServiceBookDates.RetirementDate).Any() == true)
                        {
                            OExtClass = new ExtClass 
                            {
                                Chk = true,
                                Date = OEmp.ServiceBookDates.RetirementDate,
                                PolicyName = "RETIREMENT"
                            };
                           
                        }
                        if (OExtnRednServiceBook.Where(e => e.ExtnRednActivity.ExtnRednPolicy.ExtnRednCauseType.LookupVal.ToUpper() == "PROBATIONPERIOD" && e.ExpiryDate >= DateTime.Now).Any() == true)
                        {
                            OExtClass = new ExtClass
                            {
                                Chk = true,
                                Date = OEmp.ServiceBookDates.ProbationDate,
                                PolicyName = "PROBATION",
                                Period = Convert.ToString(OEmp.ServiceBookDates.ProbationPeriod),
                            };

                        }
                    }
                    return Json(OExtClass, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
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

        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpName).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EmpCode }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}