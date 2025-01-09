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
using Recruitment;
using System.Data.Entity.Validation;
using Recruitment;

namespace P2BUltimate.Controllers.recruitment.MainController
{
    public class ResumeCollectionController : Controller
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
            return View("~/Views/Recruitement/MainView/ResumeCollection/Index.cshtml");
        }
        public ActionResult _EmpOfficial()
        {
            return View("~/Views/Shared/Core/_CanOfficial.cshtml");
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
                        .Select(e => e.Company.Id).Distinct().ToList();
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
                    var Corp_id = Convert.ToInt32(data);
                    var region_data = db.GeoStruct.Include(e => e.Location).Where(e =>
                        (e.Corporate.Id == Corp_id && e.Location != null) ||
                        (e.Region.Id == Corp_id && e.Location != null) ||
                        (e.Company.Id == Corp_id && e.Location != null) ||
                        (e.Division.Id == Corp_id && e.Location != null))
                        .Select(e => e.Location.Id).Distinct().ToList();
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
                    selectlist = new SelectList(a, "Id", "Name", "")
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
                        selectlist = new SelectList(a, "Id", "Name", "")
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
                if (data2 != null)
                {
                    selected = data2;
                }
                if (data != null)
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
                                selectlist = new SelectList(j, "Id", "Name", "")
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
                        var selectlistjson = new
                        {
                            SelectlistType = "DivisionList_DDL",
                            selectlist = new SelectList(o, "Id", "Name", "")
                        };
                        return selectlistjson;
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
                            selectlist = new SelectList(t, "Id", "Name", "")
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
                    return null;
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
                        var c = DropDownListCheckLevelFun(data, "JobPosition");
                        if (c != null)
                        {
                            return c;
                        }
                    }
                    break;
                case "JobPosition":
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
                    break;
                default:
                    return null;
                    break;
            }
            return null;
        }
        public ActionResult GetAddressLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Address.Include(e => e.Area).Include(e => e.City).Include(e => e.Country)
                                     .Include(e => e.District).Include(e => e.State).Include(e => e.StateRegion)
                                     .Include(e => e.Taluka).ToList();
                IEnumerable<Address> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Address.ToList().Where(d => d.FullAddress.Contains(data));

                }
                else
                {
                    //var list1 = db.Employee.ToList().Select(e => e.Address);
                    //var list2 = fall.Except(list1);

                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullAddress }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullAddress }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        public ActionResult GetContactDetLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactDetails.Include(e => e.ContactNumbers).ToList();
                IEnumerable<ContactDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ContactDetails.ToList().Where(d => d.FullContactDetails.Contains(data));

                }
                else
                {
                    //var list1 = db.Corporate.ToList().Select(e => e.ContactDetails);
                    //var list2 = fall.Except(list1);
                    //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullContactDetails }).Distinct();
                    //var result_1 = (from c in fall
                    //                select new { c.Id, c.CorporateCode, c.CorporateName });
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullContactDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            // return View();
        }
        //public ActionResult Create(ResumeCollection Can, FormCollection form) //Create submit
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        string ReportingHR = form["ReportingHR_drop"] == "0" ? "" : form["ReportingHR_drop"];
        //        string ReportingLevel1 = form["ReportingLevel1_drop"] == "0" ? "" : form["ReportingLevel1_drop"];
        //        string ReportingLevel2 = form["ReportingLevel2_drop"] == "0" ? "" : form["ReportingLevel2_drop"];
        //        string ReportingLevel3 = form["ReportingLevel3_drop"] == "0" ? "" : form["ReportingLevel3_drop"];
        //        string ReportingStructRights = form["ReportingStructRightslist"] == null ? null : form["ReportingStructRightslist"];

        //        string MaritalStatus = form["MaritalStatusList_DDL"] == "0" ? "" : form["MaritalStatusList_DDL"];
        //        string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];
        //        string Category = form["Category_drop"] == "0" ? "" : form["Category_drop"];
        //        string Caste = form["Caste_drop"] == "0" ? "" : form["Caste_drop"];
        //        string Religion = form["Religion_drop"] == "0" ? "" : form["Religion_drop"];
        //        string BloodGroup = form["BloodGroup_drop"] == "0" ? "" : form["BloodGroup_drop"];
        //        string ResAddrs = form["ResAddrslist"] == "0" ? "" : form["ResAddrslist"];
        //        string ResContactDetails = form["ResContactDetailslist"] == "0" ? "" : form["ResContactDetailslist"];
        //        string PermAddrs = form["PermAddrslist"] == "0" ? "" : form["PermAddrslist"];
        //        string PermContactDetails = form["PermContactDetailslist"] == "0" ? "" : form["PermContactDetailslist"];
        //        string CorrsAddrs = form["CorrsAddrslist"] == "0" ? "" : form["CorrsAddrslist"];
        //        string CorrsContactDetails = form["CorrsContactDetailslist"] == "0" ? "" : form["CorrsContactDetailslist"];
        //        string Corporate = form["CorporateList_DDL"] == "0" ? "" : form["CorporateList_DDL"];
        //        string Region = form["RegionList_DDL"] == "0" ? "" : form["RegionList_DDL"];
        //        string Company = form["CompanyList_DDL"] == "0" ? "" : form["CompanyList_DDL"];
        //        string Division = form["DivisionList_DDL"] == "0" ? "" : form["DivisionList_DDL"];
        //        string Location = form["LocationList_DDL"] == "0" ? "" : form["LocationList_DDL"];
        //        string Department = form["DepartmentList_DDL"] == "0" ? "" : form["DepartmentList_DDL"];
        //        string Group = form["GroupList_DDL"] == "0" ? "" : form["GroupList_DDL"];
        //        string Unit = form["UnitList_DDL"] == "0" ? "" : form["UnitList_DDL"];
        //        string Grade = form["Grade_drop"] == "0" ? "" : form["Grade_drop"];
        //        string Level = form["Level_drop"] == "0" ? "" : form["Level_drop"];
        //        string JobStatus = form["JobStatus_drop"] == "0" ? "" : form["JobStatus_drop"];
        //        string EmpActingStatus = form["EmpActingStatus_drop"] == "0" ? "" : form["EmpActingStatus_drop"];
        //        string Job = form["JobList_DDL"] == "0" ? "" : form["JobList_DDL"];
        //        string Position = form["Position_drop"] == "0" ? "" : form["Position_drop"];
        //        string TimingCode = form["TimingCode_drop"] == "0" ? "" : form["TimingCode_drop"];

        //        int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);
        //        int FatherName_FName = form["FatherName_id"] == "" ? 0 : Convert.ToInt32(form["FatherName_id"]);
        //        int MotherName_FName = form["MotherName_id"] == "" ? 0 : Convert.ToInt32(form["MotherName_id"]);
        //        int HusbandName_FName = form["HusbandName_id"] == "" ? 0 : Convert.ToInt32(form["HusbandName_id"]);
        //        int BeforeMarriageName_FName = form["BeforeMarriageName_id"] == "" ? 0 : Convert.ToInt32(form["BeforeMarriageName_id"]);
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            var Get_Geo_Id = db.GeoStruct.Include(e => e.Company)
        //                     .Include(e => e.Department)
        //                     .Include(e => e.Department.DepartmentObj)
        //                     .Include(e => e.Division)
        //                     .Include(e => e.Location)
        //                     .Include(e => e.Location.LocationObj)
        //                     .Include(e => e.Group)
        //                     .Include(e => e.Unit);

        //            Can.Candidate.CanName = db.NameSingle.Find(EmpName_FName);
        //            Can.Candidate.FatherName = db.NameSingle.Find(FatherName_FName);
        //            Can.Candidate.MotherName = db.NameSingle.Find(MotherName_FName);
        //            Can.Candidate.HusbandName = db.NameSingle.Find(HusbandName_FName);
        //            Can.Candidate.BeforeMarriageName = db.NameSingle.Find(BeforeMarriageName_FName);

        //            var Get_Pay_Id = db.PayStruct.Include(e => e.Grade)
        //                   .Include(e => e.Level).Include(e => e.JobStatus);

        //            var Get_Fun_Id = db.FuncStruct.Include(e => e.Job)
        //                   .Include(e => e.JobPosition);

        //            if (Grade != "" && Grade != null)
        //            {
        //                var id = Convert.ToInt32(Grade);
        //                Get_Pay_Id = Get_Pay_Id.Where(e => e.Grade.Id == id);
        //            }
        //            if (Level != "" && Level != null)
        //            {
        //                var id = Convert.ToInt32(Level);
        //                Get_Pay_Id = Get_Pay_Id.Where(e => e.Level.Id == id);
        //            }

        //            if (JobStatus != "" && JobStatus != null)
        //            {
        //                var id = Convert.ToInt32(JobStatus);
        //                Get_Pay_Id = Get_Pay_Id.Where(e => e.JobStatus.Id == id);
        //            }

        //            //var pay_data = Get_Pay_Id.SingleOrDefault();


        //            if (Job != "" && Job != null)
        //            {
        //                var id = Convert.ToInt32(Job);
        //                Get_Fun_Id = Get_Fun_Id.Where(e => e.Job.Id == id);
        //            }
        //            if (Position != "" && Position != null)
        //            {
        //                var id = Convert.ToInt32(Position);
        //                Get_Fun_Id = Get_Fun_Id.Where(e => e.JobPosition.Id == id);
        //            }

        //            // var fun_data = Get_Fun_Id.SingleOrDefault();

        //            if (Corporate != "" && Corporate != null)
        //            {
        //                var id = Convert.ToInt32(Corporate);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Corporate.Id == id);
        //            }
        //            if (Region != "" && Region != null)
        //            {
        //                var id = Convert.ToInt32(Region);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Region.Id == id);
        //            }
        //            if (Company != "" && Company != null)
        //            {
        //                var id = Convert.ToInt32(Company);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Company.Id == id);
        //            }
        //            if (Division != "" && Division != null)
        //            {
        //                var id = Convert.ToInt32(Division);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Division.Id == id);
        //            }
        //            if (Location != "" && Location != null)
        //            {
        //                var id = Convert.ToInt32(Location);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Location.Id == id);
        //            }
        //            if (Department != "" && Department != null)
        //            {
        //                var id = Convert.ToInt32(Department);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Department.Id == id);
        //            }
        //            if (Group != "" && Group != null)
        //            {
        //                var id = Convert.ToInt32(Group);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Group.Id == id);
        //            }
        //            if (Unit != "" && Unit != null)
        //            {
        //                var id = Convert.ToInt32(Unit);
        //                Get_Geo_Id = Get_Geo_Id.Where(e => e.Unit.Id == id);
        //            }

        //            //var geo_data = Get_Geo_Id.SingleOrDefault();
        //            // Get PayStuct id
        //            //if (ReportingHR != "")
        //            //{
        //            //    var val = db.ReportingStructRights.Find(int.Parse(ReportingHR));
        //            //    Emp.ReportingStructRights = val;
        //            //}
        //            //if (ReportingLevel1 != "")
        //            //{
        //            //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel1));
        //            //    Emp.ReportingLevel1 = val;
        //            //}
        //            //if (ReportingLevel2 != "")
        //            //{
        //            //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel2));
        //            //    Emp.ReportingLevel2 = val;
        //            //}
        //            //if (ReportingLevel3 != "")
        //            //{
        //            //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel3));
        //            //    Emp.ReportingLevel3 = val;
        //            //}

        //            if (MaritalStatus != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(MaritalStatus));
        //                Can.Candidate.MaritalStatus = val;
        //            }

        //            if (Gender != "")
        //            {
        //                var val = db.LookupValue.Find(int.Parse(Gender));
        //                Can.Candidate.Gender = val;
        //            }

        //            if (ResAddrs != null && ResAddrs != "")
        //            {
        //                int AddId = Convert.ToInt32(ResAddrs);
        //                var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
        //                Can.Candidate.ResAddr = val;
        //            }

        //            //if (ReportingStructRights != null)
        //            //{
        //            //    var ids = Utility.StringIdsToListIds(ReportingStructRights);
        //            //    var reportingrightslist = new List<ReportingStructRights>();
        //            //    foreach (var item in ids)
        //            //    {
        //            //        var id = Convert.ToInt32(item);
        //            //        var data = db.ReportingStructRights.Find(id);
        //            //        reportingrightslist.Add(data);

        //            //    }

        //            //    Emp.ReportingStructRights = reportingrightslist;
        //            //}

        //            if (PermAddrs != null && PermAddrs != "")
        //            {
        //                int AddId = Convert.ToInt32(PermAddrs);
        //                var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
        //                Can.Candidate.PerAddr = val;
        //            }

        //            if (CorrsAddrs != null && CorrsAddrs != "")
        //            {
        //                int AddId = Convert.ToInt32(CorrsAddrs);
        //                var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
        //                Can.Candidate.CorAddr = val;
        //            }

        //            if (ResContactDetails != null && ResContactDetails != "")
        //            {
        //                int ContId = Convert.ToInt32(ResContactDetails);
        //                var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
        //                Can.Candidate.ResContact = val;
        //            }

        //            if (PermContactDetails != null && PermContactDetails != "")
        //            {
        //                int ContId = Convert.ToInt32(PermContactDetails);
        //                var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
        //                Can.Candidate.PerContact = val;
        //            }

        //            if (CorrsContactDetails != null && CorrsContactDetails != "")
        //            {
        //                int ContId = Convert.ToInt32(CorrsContactDetails);
        //                var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
        //                Can.Candidate.CorContact = val;
        //            }


        //            //if (Get_Geo_Id != null)
        //            //{
        //            //    var geo_data = Get_Geo_Id.FirstOrDefault();
        //            //    if (geo_data != null)
        //            //    {
        //            //        Can.GeoStruct = db.GeoStruct.Find(geo_data.Id);
        //            //    }
        //            //}

        //            //if (Get_Fun_Id != null)
        //            //{
        //            //    var fun_data = Get_Fun_Id.FirstOrDefault();

        //            //    if (fun_data != null)
        //            //    {
        //            //        Emp.FuncStruct = db.FuncStruct.Find(fun_data.Id);
        //            //    }

        //            //}
        //            //if (Get_Pay_Id != null)
        //            //{
        //            //    var pay_data = Get_Pay_Id.FirstOrDefault();
        //            //    if (pay_data != null)
        //            //    {
        //            //        Emp.PayStruct = db.PayStruct.Find(pay_data.Id);
        //            //    }
        //            //}


        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    if (db.Candidate.Any(o => o.CanCode == Can.Candidate.CanCode))
        //                    {
        //                        Msg.Add("  Code Already Exists.  ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    //if (db.Employee.Any(o => o.EmpName.EmpTitle == Emp.EmpName.EmpTitle && o.EmpName.FName == Emp.EmpName.MName && o.EmpName.MName == Emp.EmpName.MName && o.EmpName.LName == o.EmpName.LName))
        //                    //{
        //                    //    Msg.Add("  Person with this Name Already Exists.  ");
        //                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //}
        //                    if (Can.Candidate.FatherName != null)
        //                    {
        //                        if (db.Candidate.Any(o => o.FatherName.EmpTitle == Can.Candidate.FatherName.EmpTitle && o.FatherName.FName == Can.Candidate.FatherName.FName && o.FatherName.MName == Can.Candidate.FatherName.MName && o.FatherName.LName == Can.Candidate.FatherName.LName))
        //                        {
        //                            Msg.Add("   Person with this Name Already Exists.  ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }

        //                    if (Can.Candidate.MotherName != null)
        //                    {
        //                        if (db.Candidate.Any(o => o.MotherName.EmpTitle == Can.Candidate.MotherName.EmpTitle && o.MotherName.FName == Can.Candidate.MotherName.FName && o.MotherName.MName == Can.Candidate.MotherName.MName && o.MotherName.LName == Can.Candidate.MotherName.LName))
        //                        {
        //                            Msg.Add("   Person with this Name Already Exists.  ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }

        //                    if (Can.Candidate.HusbandName != null)
        //                    {
        //                        if (db.Candidate.Any(o => o.HusbandName.EmpTitle == Can.Candidate.HusbandName.EmpTitle && o.HusbandName.FName == Can.Candidate.HusbandName.FName && o.HusbandName.MName == Can.Candidate.HusbandName.MName && o.HusbandName.LName == Can.Candidate.HusbandName.LName))
        //                        {
        //                            Msg.Add("   Person with this Name Already Exists.  ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }

        //                    if (Can.Candidate.BeforeMarriageName != null)
        //                    {
        //                        if (db.Candidate.Any(o => o.BeforeMarriageName.EmpTitle == Can.Candidate.BeforeMarriageName.EmpTitle && o.BeforeMarriageName.FName == Can.Candidate.BeforeMarriageName.FName && o.BeforeMarriageName.MName == Can.Candidate.BeforeMarriageName.MName && o.BeforeMarriageName.LName == Can.Candidate.BeforeMarriageName.LName))
        //                        {
        //                            Msg.Add("   Person with this Name Already Exists.  ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        }
        //                    }

        //                    //if (db.Employee.Any(o => o.CardCode == Can.CardCode))
        //                    //{
        //                    //    Msg.Add(" Card Code Already Exists.  ");
        //                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //}
        //                    Can.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                    Candidate candidate = new Candidate()
        //                    {
        //                        CanCode = Can.Candidate.CanCode == null ? "" : Can.Candidate.CanCode.Trim(),
        //                        CanName = Can.Candidate.CanName,
        //                        FatherName = Can.Candidate.FatherName,
        //                        HusbandName = Can.Candidate.HusbandName,
        //                        MotherName = Can.Candidate.MotherName,
        //                        BeforeMarriageName = Can.Candidate.BeforeMarriageName,
        //                        BirthPlace = Can.Candidate.BirthPlace == null ? "" : Can.Candidate.BirthPlace.Trim(),
        //                        CorAddr = Can.Candidate.CorAddr,
        //                        CorContact = Can.Candidate.CorContact,
        //                        Gender = Can.Candidate.Gender,
        //                        MaritalStatus = Can.Candidate.MaritalStatus,
        //                        PerAddr = Can.Candidate.PerAddr,
        //                        PerContact = Can.Candidate.PerContact,
        //                        ResAddr = Can.Candidate.ResAddr,
        //                        ResContact = Can.Candidate.ResContact,
        //                        ServiceBookDates = Can.Candidate.ServiceBookDates,
        //                        DBTrack = Can.DBTrack
        //                    };
        //                    try
        //                    {
        //                        db.Candidate.Add(candidate);
        //                        //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, Can.DBTrack);
        //                        //DT_Candidate DT_Can = (DT_Candidate)rtn_Obj;
        //                        //DT_Can.CanName_Id = Can.CanName == null ? 0 : Can.CanName.Id;
        //                        //DT_Can.MotherName_Id = Can.MotherName == null ? 0 : Can.MotherName.Id;
        //                        //DT_Can.FatherName_Id = Can.FatherName == null ? 0 : Can.FatherName.Id;
        //                        //DT_Can.HusbandName_Id = Can.HusbandName == null ? 0 : Can.HusbandName.Id;
        //                        //DT_Can.BeforeMarriageName_Id = Can.BeforeMarriageName == null ? 0 : Can.BeforeMarriageName.Id;
        //                        //DT_Can.CorAddr_Id = Can.CorAddr == null ? 0 : Can.CorAddr.Id;
        //                        //DT_Can.CorContact_Id = Can.CorContact == null ? 0 : Can.CorContact.Id;
        //                        //DT_Can.PerAddr_Id = Can.PerAddr == null ? 0 : Can.PerAddr.Id;
        //                        //DT_Can.PerContact_Id = Can.PerContact == null ? 0 : Can.PerContact.Id;
        //                        //DT_Can.ResAddr_Id = Can.ResAddr == null ? 0 : Can.ResAddr.Id;
        //                        //DT_Can.ResContact_Id = Can.ResContact == null ? 0 : Can.ResContact.Id;
        //                        //DT_Can.ServiceBookDates_Id = Can.ServiceBookDates == null ? 0 : Can.ServiceBookDates.Id;
        //                        //DT_Can.CanNominees_Id = Can.CanNominees == null ? 0 : Can.CanNominees.Id;
        //                        //DT_Can.CanMedicalInfo_Id = Can.CanMedicalInfo == null ? 0 : Can.CanMedicalInfo.Id;
        //                        //DT_Can.CanSocialInfo_Id = Can.CanSocialInfo == null ? 0 : Can.CanSocialInfo.Id;
        //                        //DT_Can.CanAcademicInfo_Id = Can.CanAcademicInfo == null ? 0 : Can.CanAcademicInfo.Id; 
        //                        //DT_Can.Gender_Id = Can.Gender == null ? 0 : Can.Gender.Id;
        //                        //DT_Can.MaritalStatus_Id = Can.MaritalStatus == null ? 0 : Can.MaritalStatus.Id;

        //                        //db.Create(DT_Can);
        //                        db.SaveChanges();
        //                        if (Convert.ToString(Session["CompId"]) != null)
        //                        {
        //                            //var oEmployeePayroll = new EmployeePayroll();
        //                            //oEmployeePayroll.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                            //oEmployeePayroll.Employee = employee;
        //                            //db.SaveChanges();

        //                            //var oEmployeeLeave = new EmployeeLeave();
        //                            //oEmployeeLeave.Employee = employee;
        //                            //oEmployeeLeave.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                            //db.EmployeeLeave.Add(oEmployeeLeave);
        //                            //db.SaveChanges();

        //                            ////atach comp leave
        //                            //var id = int.Parse(Convert.ToString(Session["CompId"]));
        //                            //var CompLvData = db.CompanyLeave.Where(e => e.Company.Id == id).SingleOrDefault();
        //                            //List<EmployeeLeave> ListEmployeeLeave = new List<EmployeeLeave>();
        //                            //ListEmployeeLeave.Add(oEmployeeLeave);
        //                            //CompLvData.EmployeeLeave = ListEmployeeLeave;
        //                            //db.Entry(CompLvData).State = System.Data.Entity.EntityState.Modified;
        //                            //db.SaveChanges();


        //                            //var company_data = db.CompanyPayroll.Where(e => e.Company.Id == id).SingleOrDefault();
        //                            //List<EmployeePayroll> emp = new List<EmployeePayroll>();
        //                            //emp.Add(oEmployeePayroll);
        //                            //company_data.EmployeePayroll = emp;
        //                            //db.Entry(company_data).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                        }
        //                        ts.Complete();
        //                        Msg.Add("  Data  Saved successfully.  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        return RedirectToAction("Create", new { concurrencyError = true, id = Can.Id });
        //                    }
        //                    catch (DataException /* dex */)
        //                    {
        //                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                        //return this.Json(new Object[] { "", "", "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                        //return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //var errorMsg = sb.ToString();
        //                //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //                //return this.Json(new { msg = errorMsg });
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult Create(ResumeCollection Can, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);
                int FatherName_FName = form["FatherName_id"] == "" ? 0 : Convert.ToInt32(form["FatherName_id"]);
                int MotherName_FName = form["MotherName_id"] == "" ? 0 : Convert.ToInt32(form["MotherName_id"]);
                int HusbandName_FName = form["HusbandName_id"] == "" ? 0 : Convert.ToInt32(form["HusbandName_id"]);
                int BeforeMarriageName_FName = form["BeforeMarriageName_id"] == "" ? 0 : Convert.ToInt32(form["BeforeMarriageName_id"]);
                string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];
                string MaritalStatus = form["MaritalStatusList_DDL"] == "0" ? "" : form["MaritalStatusList_DDL"];

                string Religion = form["Religion_drop"] == "0" ? "" : form["Religion_drop"];
                string Category = form["Category_drop"] == "0" ? "" : form["Category_drop"];
                string Caste = form["Caste_drop"] == "0" ? "" : form["Caste_drop"];
                string SubCaste = form["SubCaste_drop"] == "0" ? "" : form["SubCaste_drop"];

                string Relocation = form["Relocation_App"] == "0" ? "" : form["Relocation_App"];

                string ResAddrs = form["ResAddrslist"] == "0" ? "" : form["ResAddrslist"];
                string PermAddrs = form["PermAddrslist"] == "0" ? "" : form["PermAddrslist"];
                string CorrsAddrs = form["CorrsAddrslist"] == "0" ? "" : form["CorrsAddrslist"];

                string ResContactDetails = form["ResContactDetailslist"] == "0" ? "" : form["ResContactDetailslist"];
                string PermContactDetails = form["PermContactDetailslist"] == "0" ? "" : form["PermContactDetailslist"];
                string CorrsContactDetails = form["CorrsContactDetailslist"] == "0" ? "" : form["CorrsContactDetailslist"];

                string QualificationDetails = form["QualificationDetailsList"] == "0" ? "" : form["QualificationDetailsList"];
                string SkillDetails = form["SkillList"] == "0" ? "" : form["SkillList"];
                string ScolarshipDetails = form["Scolarshiplist"] == "0" ? "" : form["Scolarshiplist"];
                string HobbyDetails = form["Hobbylist"] == "0" ? "" : form["Hobbylist"];
                string AwardDetails = form["AwardsList"] == "0" ? "" : form["AwardsList"];

                string BloodGroupDetails = form["BloodGroup_drop"] == "0" ? "" : form["BloodGroup_drop"];
                string AllergyDetails = form["AlergyList"] == "0" ? "" : form["AlergyList"];
                string DiseaseDetails = form["DiseaseList"] == "0" ? "" : form["DiseaseList"];

                string PreCompExpDetails = form["PreCompExplist"] == "0" ? "" : form["PreCompExplist"];

                string ReferenceDetails = form["Referencelist"] == "0" ? "" : form["Referencelist"];

                string RDocumentDetails = form["RDocumentlist"] == "0" ? "" : form["RDocumentlist"];



                List<String> Msg = new List<String>();
                try
                {

                    Can.Candidate.CanName = db.NameSingle.Find(EmpName_FName);
                    Can.Candidate.FatherName = db.NameSingle.Find(FatherName_FName);
                    Can.Candidate.MotherName = db.NameSingle.Find(MotherName_FName);
                    Can.Candidate.HusbandName = db.NameSingle.Find(HusbandName_FName);
                    Can.Candidate.BeforeMarriageName = db.NameSingle.Find(BeforeMarriageName_FName);

                    if (MaritalStatus != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(MaritalStatus));
                        Can.Candidate.MaritalStatus = val;
                    }

                    if (Gender != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Gender));
                        Can.Candidate.Gender = val;
                    }

                    ///// social info

                    LookupValue catval = null;
                    if (Category != null)
                    {
                        if (Category != "")
                        {
                            catval = db.LookupValue.Find(int.Parse(Category));
                            //Can.Candidate.CanSocialInfo.Category = val;
                        }
                    }
                    LookupValue casteval = null;
                    if (Caste != null)
                    {
                        if (Caste != "")
                        {
                            casteval = db.LookupValue.Find(int.Parse(Caste));
                            //Can.Candidate.CanSocialInfo.Caste = val;
                        }
                    }

                    LookupValue religionval = null;
                    if (Religion != null)
                    {
                        if (Religion != "")
                        {
                            religionval = db.LookupValue.Find(int.Parse(Religion));
                            //Can.Candidate.CanSocialInfo.Religion = val;
                        }
                    }

                    LookupValue subcasteval = null;
                    if (SubCaste != null)
                    {
                        if (SubCaste != "")
                        {
                            subcasteval = db.LookupValue.Find(int.Parse(SubCaste));
                            //Can.Candidate.CanSocialInfo.SubCaste = val;
                        }
                    }
                    //////////
                    ////Relocation
                    ////////

                    if (ResAddrs != null && ResAddrs != "")
                    {
                        int AddId = Convert.ToInt32(ResAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Can.Candidate.ResAddr = val;
                    }

                    if (PermAddrs != null && PermAddrs != "")
                    {
                        int AddId = Convert.ToInt32(PermAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Can.Candidate.PerAddr = val;
                    }

                    if (CorrsAddrs != null && CorrsAddrs != "")
                    {
                        int AddId = Convert.ToInt32(CorrsAddrs);
                        var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                        Can.Candidate.CorAddr = val;
                    }

                    if (ResContactDetails != null && ResContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(ResContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Can.Candidate.ResContact = val;
                    }

                    if (PermContactDetails != null && PermContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(PermContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Can.Candidate.PerContact = val;
                    }

                    if (CorrsContactDetails != null && CorrsContactDetails != "")
                    {
                        int ContId = Convert.ToInt32(CorrsContactDetails);
                        var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                        Can.Candidate.CorContact = val;
                    }

                    ////////////////academic info
                    List<QualificationDetails> lookupval = new List<QualificationDetails>();
                    if (QualificationDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(QualificationDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.QualificationDetails.Find(ca);
                            lookupval.Add(Lookup_val);
                            //Can.Candidate.CanAcademicInfo.QualificationDetails = lookupval;
                        }
                    }


                    List<Skill> lookupSkill = new List<Skill>();
                    if (SkillDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(SkillDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Skill.Find(ca);
                            lookupSkill.Add(Lookup_val);
                            //Can.Candidate.CanAcademicInfo.Skill = lookupSkill;
                        }
                    }


                    List<Scolarship> lookupScolar = new List<Scolarship>();

                    if (ScolarshipDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(ScolarshipDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Scolarship.Find(ca);
                            lookupScolar.Add(Lookup_val);
                            //Can.Candidate.CanAcademicInfo.Scolarship = lookupScolar;
                        }
                    }


                    List<Hobby> lookupHobby = new List<Hobby>();

                    if (HobbyDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(HobbyDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Hobby.Find(ca);
                            lookupHobby.Add(Lookup_val);
                            //Can.Candidate.CanAcademicInfo.Hobby = lookupHobby;
                        }
                    }


                    List<Awards> lookupaward = new List<Awards>();

                    if (AwardDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(AwardDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Awards.Find(ca);
                            lookupaward.Add(Lookup_val);
                            //Can.Candidate.CanAcademicInfo.Awards = lookupaward;
                        }
                    }



                    //////////////////medical info

                    if (BloodGroupDetails != null)
                    {
                        if (BloodGroupDetails != "")
                        {
                            var val = db.LookupValue.Find(int.Parse(BloodGroupDetails));
                            Can.Candidate.CanMedicalInfo.BloodGroup = val;
                        }
                    }

                    List<Allergy> Allergyval = new List<Allergy>();

                    if (AllergyDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(AllergyDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Allergy.Find(ca);
                            Allergyval.Add(Lookup_val);
                            Can.Candidate.CanMedicalInfo.Allergy = Allergyval;
                        }
                    }

                    List<Disease> DiseaseVal = new List<Disease>();
                    if (DiseaseDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(DiseaseDetails);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.Disease.Find(ca);
                            DiseaseVal.Add(Lookup_val);
                            Can.Candidate.CanMedicalInfo.Disease = DiseaseVal;
                        }
                    }

                    //////prev comp exp

                    /////

                    //////candidate doc

                    Can.CandidateDocuments = null;
                    List<CandidateDocuments> OBJ = new List<CandidateDocuments>();

                    if (RDocumentDetails != null && RDocumentDetails != "")
                    {
                        var ids = Utility.StringIdsToListIds(RDocumentDetails);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.CandidateDocuments.Find(ca);
                            OBJ.Add(OBJ_val);
                            Can.CandidateDocuments = OBJ;
                        }
                    }





                    //if (ModelState.IsValid)
                    //{
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if (db.ResumeCollection.Any(o => o.Code == c.Code))
                        //{
                        //    Msg.Add("Code Already Exists.");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}

                        Can.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        DBTrack DBTrack1 = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        EmpAcademicInfo canacademic = new EmpAcademicInfo()
                        {
                            QualificationDetails = lookupval,
                            Skill = lookupSkill,
                            Scolarship = lookupScolar,
                            Hobby = lookupHobby,
                            Awards = lookupaward,

                            DBTrack = DBTrack1
                        };
                        db.EmpAcademicInfo.Add(canacademic);
                        db.SaveChanges();

                        EmpMedicalInfo canmedical = new EmpMedicalInfo()
                        {
                            BloodGroup = Can.Candidate.CanMedicalInfo.BloodGroup,
                            Allergy = Can.Candidate.CanMedicalInfo.Allergy,
                            Disease = Can.Candidate.CanMedicalInfo.Disease,
                            Height = Can.Candidate.CanMedicalInfo.Height,
                            Weight = Can.Candidate.CanMedicalInfo.Weight,
                            IDMark = Can.Candidate.CanMedicalInfo.IDMark,

                            DBTrack = DBTrack1
                        };
                        db.EmpMedicalInfo.Add(canmedical);
                        db.SaveChanges();

                        EmpSocialInfo canSocial = new EmpSocialInfo()
                        {
                            Religion = religionval,
                            Category = catval,
                            Caste = casteval,
                            SubCaste = subcasteval,

                            DBTrack = DBTrack1
                        };
                        db.EmpSocialInfo.Add(canSocial);
                        db.SaveChanges();
                        NationalityID canNationalityID = null;

                        try
                        {

                            canNationalityID = new NationalityID()
                             {
                                 AdharNo = Can.Candidate.CanOffInfo.NationalityID.AdharNo,
                                 PANNo = Can.Candidate.CanOffInfo.NationalityID.PANNo,
                                 No1 = Can.Candidate.CanOffInfo.NationalityID.No1,

                             };
                            db.NationalityID.Add(canNationalityID);
                            db.SaveChanges();
                        }


                        catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                        {
                            Exception raise = dbEx;
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    string message = string.Format("{0}:{1}",
                                        validationErrors.Entry.Entity.ToString(),
                                        validationError.ErrorMessage);
                                    // raise a new exception nesting  
                                    // the current instance as InnerException  
                                    raise = new InvalidOperationException(message, raise);
                                }
                            }
                            throw raise;
                        }
                        ServiceBookDates canServiceBookDates = null;
                        try
                        {
                            canServiceBookDates = new ServiceBookDates()
                            {

                                BirthDate = Can.Candidate.ServiceBookDates.BirthDate,

                            };
                            db.ServiceBookDates.Add(canServiceBookDates);
                            db.SaveChanges();
                        }

                        catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                        {
                            Exception raise = dbEx;
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    string message = string.Format("{0}:{1}",
                                        validationErrors.Entry.Entity.ToString(),
                                        validationError.ErrorMessage);
                                    // raise a new exception nesting  
                                    // the current instance as InnerException  
                                    raise = new InvalidOperationException(message, raise);
                                }
                            }
                            throw raise;
                        }

                        EmpOff canoff = new EmpOff()
                        {
                            NationalityID = canNationalityID,
                            DBTrack = DBTrack1
                        };
                        db.EmpOff.Add(canoff);
                        db.SaveChanges();

                        Candidate corporate = new Candidate()
                        {
                            CanCode = Can.Candidate.CanCode,
                            CanName = Can.Candidate.CanName,
                            FatherName = Can.Candidate.FatherName,
                            MotherName = Can.Candidate.MotherName,
                            HusbandName = Can.Candidate.HusbandName,
                            BeforeMarriageName = Can.Candidate.BeforeMarriageName,
                            CorAddr = Can.Candidate.CorAddr,
                            CorContact = Can.Candidate.CorContact,
                            Gender = Can.Candidate.Gender,
                            MaritalStatus = Can.Candidate.MaritalStatus,
                            PerAddr = Can.Candidate.PerAddr,
                            PerContact = Can.Candidate.PerContact,
                            ResAddr = Can.Candidate.ResAddr,
                            ResContact = Can.Candidate.ResContact,
                            ServiceBookDates = canServiceBookDates,
                            BirthPlace = Can.Candidate.BirthPlace == null ? "" : Can.Candidate.BirthPlace.Trim(),

                            CanAcademicInfo = canacademic,
                            CanMedicalInfo = canmedical,
                            CanSocialInfo = canSocial,

                            CanOffInfo = canoff,

                            DBTrack = DBTrack1
                        };

                        db.Candidate.Add(corporate);
                        db.SaveChanges();


                        ResumeCollection rc = new ResumeCollection()
                        {
                            Candidate = corporate,
                            CurrentCTC = Can.CurrentCTC,
                            ExpectedCTC = Can.ExpectedCTC,
                            YrsofExperience = Can.YrsofExperience,
                            CandidateLocation = Can.CandidateLocation,
                            Relocation = Can.Relocation,
                            RelocationArea = Can.RelocationArea,

                            DBTrack = Can.DBTrack
                        };

                        db.ResumeCollection.Add(rc);
                        db.SaveChanges();

                        ts.Complete();
                        Msg.Add("Data Saved Successfully.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

            }

        }


        public class returnDataClass
        {
            public Array QualificationId { get; set; }
            public Array QualificationVal { get; set; }

            public Array SkillId { get; set; }
            public Array SkillVal { get; set; }

            public Array ScolarshipId { get; set; }
            public Array ScolarshipVal { get; set; }

            public Array HobbyId { get; set; }
            public Array HobbyVal { get; set; }

            public Array AwardsId { get; set; }
            public Array AwardsVal { get; set; }

            public Array AllergyId { get; set; }
            public Array AllergyVal { get; set; }

            public Array DiseaseId { get; set; }
            public Array DiseaseVal { get; set; }

            public Array PrevCompId { get; set; }
            public Array PrevCompVal { get; set; }



            public Array FamilyDetailsId { get; set; }
            public Array FamilyDetailsVal { get; set; }

            public int BloodGroup { get; set; }
            public double Height { get; set; }
            public double Weight { get; set; }
            public string IDMark { get; set; }


            public string BirthDate { get; set; }

            public string AadharNo { get; set; }
            public string PanNo { get; set; }
            public string Nationality { get; set; }

            public int Religion { get; set; }
            public int Category { get; set; }
            public int Caste { get; set; }
            public int SubCaste { get; set; }

        }

        public class returnDataClass1
        {
            public Array CanDocId { get; set; }
            public Array CanDocVal { get; set; }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string[] para = { };
                var Q = db.Candidate//.Include(e => e.PayScale) 
                    .Where(e => e.Id == data)
                     .Include(e => e.CorContact)
                    .Include(e => e.PerAddr)
                    .Include(e => e.PerContact)
                    .Include(e => e.ResAddr)
                    .Include(e => e.ResContact)
                    .Select(e => new
                    {
                        BirthPlace = e.BirthPlace,
                        Code = e.CanCode,
                        Gender_Id = e.Gender.Id == null ? 0 : e.Gender.Id,
                        MaritalStatus_Id = e.MaritalStatus.Id == null ? 0 : e.MaritalStatus.Id,
                        ServiceBookDates = e.ServiceBookDates == null ? null : e.ServiceBookDates,

                        EmpName_FullNameFML = e.CanName == null ? "" : e.CanName.FullNameFML,
                        EmpName_Id = e.CanName == null ? 0 : e.CanName.Id,

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

                var R = db.ResumeCollection//.Include(e => e.PayScale) 
                    .Include(e => e.Candidate)
                    .Where(e => e.Candidate.Id == data)

                    .Select(e => new
                    {
                        CurrentCTC = e.CurrentCTC,
                        ExpectedCTC = e.ExpectedCTC,
                        YrsofExperience = e.YrsofExperience,
                        CandidateLocation = e.CandidateLocation,
                        Relocation = e.Relocation,
                        RelocationArea = e.RelocationArea == null ? "" : e.RelocationArea,


                    }).ToList();

                var RC = db.Candidate
                    .Where(e => e.Id == data)
                    .Include(e => e.CanAcademicInfo)

                    .Include(e => e.CanAcademicInfo.QualificationDetails)
                    .Include(e => e.CanAcademicInfo.Skill)
                    .Include(e => e.CanAcademicInfo.Scolarship)
                    .Include(e => e.CanAcademicInfo.Hobby)
                    .Include(e => e.CanAcademicInfo.Awards)

                    .Include(e => e.CanMedicalInfo)
                    .Include(e => e.CanMedicalInfo.Allergy)
                    .Include(e => e.CanMedicalInfo.BloodGroup)
                    .Include(e => e.CanMedicalInfo.Disease)
                    .Include(e => e.CanMedicalInfo.Disease.Select(t => t.DiseaseType))

                    .Include(e => e.CanOffInfo)
                    .Include(e => e.CanOffInfo.NationalityID)

                    .Include(e => e.ServiceBookDates)


                    .Include(e => e.CanSocialInfo)
                    .Include(e => e.CanSocialInfo.Religion)
                    .Include(e => e.CanSocialInfo.Category)
                    .Include(e => e.CanSocialInfo.Caste)
                    .Include(e => e.CanSocialInfo.SubCaste)

                    .Include(e => e.CanFamilyDetails)
                    .Include(e => e.CanFamilyDetails.Select(t => t.MemberName))
                    .Include(e => e.CanPreCompExp)

                  .ToList();

                var ListreturnDataClass = new List<returnDataClass>();


                foreach (var item in RC)
                {
                    ListreturnDataClass.Add(new returnDataClass
                    {

                        ////academic

                        QualificationId = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.QualificationDetails.Select(e => e.Id.ToString()).ToArray(),
                        QualificationVal = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.QualificationDetails.Select(e => e.FullDetails).ToArray(),

                        SkillId = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Skill.Select(e => e.Id.ToString()).ToArray(),
                        SkillVal = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Skill.Select(e => e.FullDetails).ToArray(),

                        ScolarshipId = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Scolarship.Select(e => e.Id.ToString()).ToArray(),
                        ScolarshipVal = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Scolarship.Select(e => e.FullDetails).ToArray(),

                        HobbyId = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Hobby.Select(e => e.Id.ToString()).ToArray(),
                        HobbyVal = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Hobby.Select(e => e.HobbyName).ToArray(),

                        AwardsId = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Awards.Select(e => e.Id.ToString()).ToArray(),
                        AwardsVal = item.CanAcademicInfo == null ? para : item.CanAcademicInfo.Awards.Select(e => e.FullDetails).ToArray(),

                        ///medical
                        ///

                        BloodGroup = item.CanMedicalInfo.BloodGroup == null ? 0 : item.CanMedicalInfo.BloodGroup.Id,
                        Height = item.CanMedicalInfo.Height == null ? 0 : item.CanMedicalInfo.Height,
                        Weight = item.CanMedicalInfo.Weight == null ? 0 : item.CanMedicalInfo.Weight,
                        IDMark = item.CanMedicalInfo.IDMark == null ? null : item.CanMedicalInfo.IDMark,

                        AllergyId = item.CanMedicalInfo == null ? para : item.CanMedicalInfo.Allergy.Select(e => e.Id.ToString()).ToArray(),
                        AllergyVal = item.CanMedicalInfo == null ? para : item.CanMedicalInfo.Allergy.Select(e => e.FullDetails).ToArray(),

                        DiseaseId = item.CanMedicalInfo == null ? para : item.CanMedicalInfo.Disease.Select(e => e.Id.ToString()).ToArray(),
                        DiseaseVal = item.CanMedicalInfo == null ? para : item.CanMedicalInfo.Disease.Select(e => e.FullDetails).ToArray(),

                        BirthDate = item.ServiceBookDates.BirthDate.Value.ToShortDateString(),
                        AadharNo = item.CanOffInfo.NationalityID.AdharNo.ToString(),
                        PanNo = item.CanOffInfo.NationalityID.PANNo.ToString(),
                        Nationality = item.CanOffInfo.NationalityID.No1.ToString(),

                        Religion = item.CanSocialInfo.Religion == null ? 0 : item.CanSocialInfo.Religion.Id,
                        Category = item.CanSocialInfo.Category == null ? 0 : item.CanSocialInfo.Category.Id,
                        Caste = item.CanSocialInfo.Caste == null ? 0 : item.CanSocialInfo.Caste.Id,
                        SubCaste = item.CanSocialInfo.SubCaste == null ? 0 : item.CanSocialInfo.SubCaste.Id,

                        PrevCompId = item.CanPreCompExp == null ? para : item.CanPreCompExp.Select(e => e.Id.ToString()).ToArray(),
                        PrevCompVal = item.CanPreCompExp == null ? para : item.CanPreCompExp.Select(e => e.FullDetails).ToArray(),

                        FamilyDetailsId = item.CanFamilyDetails == null ? para : item.CanFamilyDetails.Select(e => e.Id.ToString()).ToArray(),
                        FamilyDetailsVal = item.CanFamilyDetails == null ? para : item.CanFamilyDetails.Select(e => e.FullDetails).ToArray(),
                    });
                }

                var cd = db.ResumeCollection
                    .Include(e => e.Candidate)
                    .Include(e => e.CandidateDocuments)
                    .Include(e => e.CandidateDocuments.Select(t => t.DocumentType))
                    .Include(e => e.CandidateDocuments.Select(t => t.SubDocumentName))
                    .Where(e => e.Candidate.Id == data).ToList();

                var ListreturnDataClass1 = new List<returnDataClass1>();
                foreach (var item in cd)
                {
                    ListreturnDataClass1.Add(new returnDataClass1
                    {


                        CanDocId = item.CandidateDocuments == null ? para : item.CandidateDocuments.Select(e => e.Id.ToString()).ToArray(),
                        CanDocVal = item.CandidateDocuments == null ? para : item.CandidateDocuments.Select(e => e.FullDetails).ToArray(),
                    });
                }


                var Emp = db.ResumeCollection.Where(e => e.Candidate.Id == data).FirstOrDefault();
                TempData["RowVersion"] = Emp.RowVersion;
                var Auth = Emp.DBTrack.IsModified;
                return Json(new Object[] { Q, R, "", ListreturnDataClass, ListreturnDataClass1, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(ResumeCollection Can1, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                int EmpName_FName = form["EmpName_id"] == "" ? 0 : Convert.ToInt32(form["EmpName_id"]);
                int FatherName_FName = form["FatherName_id"] == "" ? 0 : Convert.ToInt32(form["FatherName_id"]);
                int MotherName_FName = form["MotherName_id"] == "" ? 0 : Convert.ToInt32(form["MotherName_id"]);
                int HusbandName_FName = form["HusbandName_id"] == "" ? 0 : Convert.ToInt32(form["HusbandName_id"]);
                int BeforeMarriageName_FName = form["BeforeMarriageName_id"] == "" ? 0 : Convert.ToInt32(form["BeforeMarriageName_id"]);
                string Gender = form["GenderList_DDL"] == "0" ? "" : form["GenderList_DDL"];
                string MaritalStatus = form["MaritalStatusList_DDL"] == "0" ? "" : form["MaritalStatusList_DDL"];

                string Religion = form["Religion_drop"] == "0" ? "" : form["Religion_drop"];
                string Category = form["Category_drop"] == "0" ? "" : form["Category_drop"];
                string Caste = form["Caste_drop"] == "0" ? "" : form["Caste_drop"];
                string SubCaste = form["SubCaste_drop"] == "0" ? "" : form["SubCaste_drop"];

                bool Relocation = Convert.ToBoolean(form["Relocation_App"]);

                string ResAddrs = form["ResAddrslist"] == "0" ? "" : form["ResAddrslist"];
                string PermAddrs = form["PermAddrslist"] == "0" ? "" : form["PermAddrslist"];
                string CorrsAddrs = form["CorrsAddrslist"] == "0" ? "" : form["CorrsAddrslist"];

                string ResContactDetails = form["ResContactDetailslist"] == "0" ? "" : form["ResContactDetailslist"];
                string PermContactDetails = form["PermContactDetailslist"] == "0" ? "" : form["PermContactDetailslist"];
                string CorrsContactDetails = form["CorrsContactDetailslist"] == "0" ? "" : form["CorrsContactDetailslist"];

                string QualificationDetails = form["QualificationDetailsList"] == "0" ? "" : form["QualificationDetailsList"];
                string SkillDetails = form["SkillList"] == "0" ? "" : form["SkillList"];
                string ScolarshipDetails = form["Scolarshiplist"] == "0" ? "" : form["Scolarshiplist"];
                string HobbyDetails = form["Hobbylist"] == "0" ? "" : form["Hobbylist"];
                string AwardDetails = form["AwardsList"] == "0" ? "" : form["AwardsList"];

                string BloodGroupDetails = form["BloodGroup_drop"] == "0" ? "" : form["BloodGroup_drop"];
                string AllergyDetails = form["AlergyList"] == "0" ? "" : form["AlergyList"];
                string DiseaseDetails = form["DiseaseList"] == "0" ? "" : form["DiseaseList"];

                string PreCompExpDetails = form["PreCompExplist"] == "0" ? "" : form["PreCompExplist"];

                string ReferenceDetails = form["Referencelist"] == "0" ? "" : form["Referencelist"];

                string RDocumentDetails = form["CandidateDocumentslist"] == "0" ? "" : form["CandidateDocumentslist"];

                string Height = form["Candidate.CanMedicalInfo.Height"] == "0" ? "" : form["Candidate.CanMedicalInfo.Height"];
                string Weight = form["Candidate.CanMedicalInfo.Weight"] == "0" ? "" : form["Candidate.CanMedicalInfo.Weight"];
                string IDMark = form["Candidate.CanMedicalInfo.IDMark"] == "" ? null : form["Candidate.CanMedicalInfo.IDMark"];

                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                using (DataBaseContext db = new DataBaseContext())
                {

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                            {
                                ResumeCollection blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.ResumeCollection
                                         .Include(e => e.Candidate)
                                         .Where(e => e.Candidate.Id == data).SingleOrDefault();

                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                Candidate Can = db.Candidate.Where(e => e.Id == data)
                                    .Include(e => e.CanName)
                                     .Include(e => e.BeforeMarriageName)
                                         .Include(e => e.CorAddr)
                                         .Include(e => e.CorContact)
                                         .Include(e => e.ServiceBookDates)
                                         .Include(e => e.FatherName)
                                         .Include(e => e.Gender)
                                         .Include(e => e.HusbandName)
                                         .Include(e => e.MaritalStatus)
                                         .Include(e => e.MotherName)
                                         .Include(e => e.PerAddr)
                                         .Include(e => e.PerContact)
                                         .Include(e => e.CanPreCompExp)
                                         .Include(e => e.ResAddr)
                                         .Include(e => e.ResContact)


                                    .SingleOrDefault();


                                Can.CanName = db.NameSingle.Find(EmpName_FName);
                                Can.FatherName = db.NameSingle.Find(FatherName_FName);
                                Can.MotherName = db.NameSingle.Find(MotherName_FName);
                                Can.HusbandName = db.NameSingle.Find(HusbandName_FName);
                                Can.BeforeMarriageName = db.NameSingle.Find(BeforeMarriageName_FName);


                                if (MaritalStatus != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(MaritalStatus));
                                    Can.MaritalStatus = val;
                                }
                                else
                                {
                                    Can.MaritalStatus = null;
                                }

                                if (Gender != "")
                                {
                                    var val = db.LookupValue.Find(int.Parse(Gender));
                                    Can.Gender = val;
                                }
                                else
                                {
                                    Can.Gender = null;
                                }

                                if (ResAddrs != null && ResAddrs != "")
                                {
                                    int AddId = Convert.ToInt32(ResAddrs);
                                    var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                                    Can.ResAddr = val;
                                }
                                else
                                {
                                    Can.ResAddr = null;
                                }

                                if (PermAddrs != null && PermAddrs != "")
                                {
                                    int AddId = Convert.ToInt32(PermAddrs);
                                    var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                                    Can.PerAddr = val;
                                }
                                else
                                {
                                    Can.PerAddr = null;
                                }

                                if (CorrsAddrs != null && CorrsAddrs != "")
                                {
                                    int AddId = Convert.ToInt32(CorrsAddrs);
                                    var val = db.Address.Where(e => e.Id == AddId).SingleOrDefault();
                                    Can.CorAddr = val;
                                }
                                else
                                {
                                    Can.CorAddr = null;
                                }

                                if (ResContactDetails != null && ResContactDetails != "")
                                {
                                    int ContId = Convert.ToInt32(ResContactDetails);
                                    var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                                    Can.ResContact = val;
                                }
                                else
                                {
                                    Can.ResContact = null;
                                }

                                if (PermContactDetails != null && PermContactDetails != "")
                                {
                                    int ContId = Convert.ToInt32(PermContactDetails);
                                    var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                                    Can.PerContact = val;
                                }
                                else
                                {
                                    Can.PerContact = null;
                                }

                                if (CorrsContactDetails != null && CorrsContactDetails != "")
                                {
                                    int ContId = Convert.ToInt32(CorrsContactDetails);
                                    var val = db.ContactDetails.Where(e => e.Id == ContId).SingleOrDefault();
                                    Can.CorContact = val;
                                }
                                else
                                {
                                    Can.CorContact = null;
                                }


                                Can1.Candidate = Can;

                                var candidateinfo = db.Candidate
                                    .Include(e => e.CanSocialInfo)
                                    .Include(e => e.CanAcademicInfo)
                                    .Include(e => e.CanMedicalInfo)
                                    .Include(e => e.CanOffInfo)
                                    .Include(e => e.CanPreCompExp)
                                    .Include(e => e.CanFamilyDetails)


                                    .Where(e => e.Id == data).SingleOrDefault();

                                //////////////////prev comp exp start
                                List<PrevCompExp> ObjPrevCompExp = new List<PrevCompExp>();

                                if (PreCompExpDetails != null && PreCompExpDetails != "")
                                {
                                    var ids = Utility.StringIdsToListIds(PreCompExpDetails);
                                    foreach (var ca in ids)
                                    {
                                        var PrevCompExplist = db.PrevCompExp.Find(ca);
                                        ObjPrevCompExp.Add(PrevCompExplist);
                                        candidateinfo.CanPreCompExp = ObjPrevCompExp;
                                    }
                                }
                                else
                                {
                                    candidateinfo.CanPreCompExp = null;
                                }

                                ///////////// prev comp exp end

                                //////////////////family details start
                                List<FamilyDetails> ObjFamilyDetails = new List<FamilyDetails>();

                                if (ReferenceDetails != null && ReferenceDetails != "")
                                {
                                    var ids = Utility.StringIdsToListIds(ReferenceDetails);
                                    foreach (var ca in ids)
                                    {
                                        var FamilyDetailslist = db.FamilyDetails.Find(ca);
                                        ObjFamilyDetails.Add(FamilyDetailslist);
                                        candidateinfo.CanFamilyDetails = ObjFamilyDetails;
                                    }
                                }
                                else
                                {
                                    candidateinfo.CanFamilyDetails = null;
                                }

                                ///////////// family details end

                                //////////////////candidate docs start

                                List<CandidateDocuments> ObjCandidateDocuments = new List<CandidateDocuments>();


                                ResumeCollection ITdetails = db.ResumeCollection.Include(e => e.CandidateDocuments).Where(e => e.Candidate.Id == data).SingleOrDefault();
                                if (RDocumentDetails != null && RDocumentDetails != "")
                                {
                                    var ids = Utility.StringIdsToListIds(RDocumentDetails);
                                    foreach (var ca in ids)
                                    {
                                        var ITSeclist = db.CandidateDocuments.Find(ca);
                                        ObjCandidateDocuments.Add(ITSeclist);
                                        ITdetails.CandidateDocuments = ObjCandidateDocuments;
                                    }
                                }
                                else
                                {
                                    ITdetails.CandidateDocuments = null;
                                }
                                //////////////////candidate docs end

                                ///// social info start

                                EmpSocialInfo CanSocial = db.EmpSocialInfo
                                                            .Include(e => e.Category)
                                                            .Include(e => e.Caste)
                                                            .Include(e => e.SubCaste)
                                                            .Include(e => e.Religion)
                                    .Where(e => e.Id == candidateinfo.CanSocialInfo.Id).SingleOrDefault();



                                LookupValue catval = null;
                                if (Category != null && Category != "")
                                {
                                    catval = db.LookupValue.Find(int.Parse(Category));
                                    CanSocial.Category = catval;
                                }
                                else
                                {
                                    CanSocial.Category = null;
                                }

                                LookupValue casteval = null;
                                if (Caste != null && Caste != "")
                                {

                                    casteval = db.LookupValue.Find(int.Parse(Caste));
                                    CanSocial.Caste = casteval;
                                }
                                else
                                {
                                    CanSocial.Caste = null;
                                }

                                LookupValue religionval = null;
                                if (Religion != null)
                                {
                                    if (Religion != "")
                                    {
                                        religionval = db.LookupValue.Find(int.Parse(Religion));

                                        CanSocial.Religion = religionval;
                                    }
                                }
                                else
                                {
                                    CanSocial.Religion = null;
                                }

                                LookupValue subcasteval = null;
                                if (SubCaste != null)
                                {
                                    if (SubCaste != "")
                                    {
                                        subcasteval = db.LookupValue.Find(int.Parse(SubCaste));
                                        CanSocial.SubCaste = subcasteval;
                                    }
                                }
                                else
                                {
                                    CanSocial.SubCaste = null;
                                }

                                db.EmpSocialInfo.Attach(CanSocial);
                                db.Entry(CanSocial).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = CanSocial.RowVersion;
                                db.Entry(CanSocial).State = System.Data.Entity.EntityState.Detached;

                                //////// social info end

                                ////////////////academic info start

                                EmpAcademicInfo CanEmpAcademicInfo = db.EmpAcademicInfo
                                                           .Include(e => e.QualificationDetails)
                                                           .Include(e => e.Skill)
                                                           .Include(e => e.Hobby)
                                                           .Include(e => e.Scolarship)
                                                           .Include(e => e.Awards)
                                   .Where(e => e.Id == candidateinfo.CanAcademicInfo.Id).SingleOrDefault();


                                List<QualificationDetails> lookupval = new List<QualificationDetails>();
                                if (QualificationDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(QualificationDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.QualificationDetails.Find(ca);
                                        lookupval.Add(Lookup_val);
                                        CanEmpAcademicInfo.QualificationDetails = lookupval;
                                    }
                                }
                                else
                                {
                                    CanEmpAcademicInfo.QualificationDetails = null;
                                }


                                List<Skill> lookupSkill = new List<Skill>();
                                if (SkillDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(SkillDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.Skill.Find(ca);
                                        lookupSkill.Add(Lookup_val);
                                        CanEmpAcademicInfo.Skill = lookupSkill;
                                    }
                                }
                                else
                                {
                                    CanEmpAcademicInfo.Skill = null;
                                }

                                List<Scolarship> lookupScolar = new List<Scolarship>();

                                if (ScolarshipDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(ScolarshipDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.Scolarship.Find(ca);
                                        lookupScolar.Add(Lookup_val);
                                        CanEmpAcademicInfo.Scolarship = lookupScolar;
                                    }
                                }
                                else
                                {
                                    CanEmpAcademicInfo.Scolarship = null;
                                }

                                List<Hobby> lookupHobby = new List<Hobby>();

                                if (HobbyDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(HobbyDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.Hobby.Find(ca);
                                        lookupHobby.Add(Lookup_val);
                                        CanEmpAcademicInfo.Hobby = lookupHobby;
                                    }
                                }
                                else
                                {
                                    CanEmpAcademicInfo.Hobby = null;
                                }

                                List<Awards> lookupaward = new List<Awards>();

                                if (AwardDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(AwardDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.Awards.Find(ca);
                                        lookupaward.Add(Lookup_val);
                                        CanEmpAcademicInfo.Awards = lookupaward;
                                    }
                                }
                                else
                                {
                                    CanEmpAcademicInfo.Awards = null;
                                }


                                db.EmpAcademicInfo.Attach(CanEmpAcademicInfo);
                                db.Entry(CanEmpAcademicInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = CanEmpAcademicInfo.RowVersion;
                                db.Entry(CanEmpAcademicInfo).State = System.Data.Entity.EntityState.Detached;

                                ////////////////academic info end

                                //////////////////medical info start

                                EmpMedicalInfo CanEmpMedicalInfo = db.EmpMedicalInfo
                                                          .Include(e => e.BloodGroup)
                                                          .Include(e => e.Allergy)
                                                          .Include(e => e.Disease)

                                  .Where(e => e.Id == candidateinfo.CanMedicalInfo.Id).SingleOrDefault();

                                if (Height != "")
                                {
                                    CanEmpMedicalInfo.Height = Convert.ToInt32(Height);
                                }
                                if (Weight != "")
                                {
                                    CanEmpMedicalInfo.Weight = Convert.ToInt32(Weight);
                                }
                                if (IDMark != null || IDMark != "")
                                {
                                    CanEmpMedicalInfo.IDMark = IDMark;
                                }

                                if (BloodGroupDetails != null)
                                {
                                    if (BloodGroupDetails != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(BloodGroupDetails));
                                        CanEmpMedicalInfo.BloodGroup = val;
                                    }
                                }
                                else
                                {
                                    CanEmpMedicalInfo.BloodGroup = null;
                                }

                                List<Allergy> Allergyval = new List<Allergy>();

                                if (AllergyDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(AllergyDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.Allergy.Find(ca);
                                        Allergyval.Add(Lookup_val);
                                        CanEmpMedicalInfo.Allergy = Allergyval;
                                    }
                                }
                                else
                                {
                                    CanEmpMedicalInfo.Allergy = null;
                                }

                                List<Disease> DiseaseVal = new List<Disease>();
                                if (DiseaseDetails != null)
                                {
                                    var ids = Utility.StringIdsToListIds(DiseaseDetails);
                                    foreach (var ca in ids)
                                    {
                                        var Lookup_val = db.Disease.Find(ca);
                                        DiseaseVal.Add(Lookup_val);
                                        CanEmpMedicalInfo.Disease = DiseaseVal;
                                    }
                                }
                                else
                                {
                                    CanEmpMedicalInfo.Disease = null;
                                }

                                db.EmpMedicalInfo.Attach(CanEmpMedicalInfo);
                                db.Entry(CanEmpMedicalInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = CanEmpMedicalInfo.RowVersion;
                                db.Entry(CanEmpMedicalInfo).State = System.Data.Entity.EntityState.Detached;

                                //////////////////medical info end

                                Can1.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                var m1 = db.ResumeCollection.Where(e => e.Candidate.Id == data).ToList();
                                foreach (var s in m1)
                                {
                                    db.ResumeCollection.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }

                                int m2 = db.ResumeCollection.Where(e => e.Candidate.Id == data).SingleOrDefault().Id;
                                var CurEmp = db.ResumeCollection.Find(m2);
                                TempData["CurrRowVersion"] = CurEmp.RowVersion;
                                db.Entry(CurEmp).State = System.Data.Entity.EntityState.Detached;




                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    ResumeCollection rc = new ResumeCollection()
                                    {
                                        CurrentCTC = Can1.CurrentCTC,
                                        ExpectedCTC = Can1.ExpectedCTC,
                                        YrsofExperience = Can1.YrsofExperience,
                                        CandidateLocation = Can1.CandidateLocation,
                                        Relocation = Relocation,
                                        RelocationArea = Can1.RelocationArea,
                                        Id = m2,
                                        DBTrack = Can1.DBTrack,

                                    };

                                    db.ResumeCollection.Attach(rc);
                                    db.Entry(rc).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(rc).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    db.SaveChanges();
                                    db.Entry(rc).State = System.Data.Entity.EntityState.Detached;

                                }


                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add(" Record Updated Successfully ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);



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
                                Can1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //public int EditS(string MaritalStatus, string Gender, string ResAddrs, string PermAddrs, string CorrsAddrs, string ResContactDetails, string PermContactDetails, string CorrsContactDetails, string ReportingHR, string ReportingLevel1, string ReportingLevel2, string ReportingLevel3, string ReportingStructRights, int data, Employee Emp, DBTrack dbT)
        //{

        //        Employee typedetails = null;

        //        //if (ReportingHR != null & ReportingHR != "")
        //        //{
        //        //    var val = db.ReportingStruct.Find(int.Parse(ReportingHR));
        //        //    Emp.ReportingHR = val;

        //        //    var type = db.Employee.Include(e => e.ReportingHR).Where(e => e.Id == data).SingleOrDefault();

        //        //    if (type.ReportingHR != null)
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.ReportingHR.Id == type.ReportingHR.Id && x.Id == data).SingleOrDefault();
        //        //    }
        //        //    else
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //        //    }
        //        //    typedetails.Gender = Emp.Gender;
        //        //}
        //        //else
        //        //{
        //        //    typedetails = db.Employee.Include(e => e.ReportingHR).Where(x => x.Id == data).SingleOrDefault();
        //        //    typedetails.ReportingHR = null;
        //        //}
        //        //if (ReportingLevel1 != null & ReportingLevel1 != "")
        //        //{
        //        //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel1));
        //        //    Emp.ReportingLevel1 = val;

        //        //    var type = db.Employee.Include(e => e.ReportingLevel1).Where(e => e.Id == data).SingleOrDefault();

        //        //    if (type.ReportingLevel1 != null)
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.ReportingLevel1.Id == type.ReportingLevel1.Id && x.Id == data).SingleOrDefault();
        //        //    }
        //        //    else
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //        //    }
        //        //    typedetails.Gender = Emp.Gender;
        //        //}
        //        //else
        //        //{
        //        //    typedetails = db.Employee.Include(e => e.ReportingLevel1).Where(x => x.Id == data).SingleOrDefault();
        //        //    typedetails.ReportingLevel1 = null;
        //        //}
        //        //if (ReportingLevel2 != null & ReportingLevel2 != "")
        //        //{
        //        //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel2));
        //        //    Emp.ReportingLevel2 = val;

        //        //    var type = db.Employee.Include(e => e.ReportingLevel2).Where(e => e.Id == data).SingleOrDefault();

        //        //    if (type.ReportingLevel2 != null)
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.ReportingLevel2.Id == type.ReportingLevel2.Id && x.Id == data).SingleOrDefault();
        //        //    }
        //        //    else
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //        //    }
        //        //    typedetails.Gender = Emp.Gender;
        //        //}
        //        //else
        //        //{
        //        //    typedetails = db.Employee.Include(e => e.ReportingLevel2).Where(x => x.Id == data).SingleOrDefault();
        //        //    typedetails.ReportingLevel2 = null;
        //        //}
        //        //if (ReportingLevel3 != null & ReportingLevel3 != "")
        //        //{
        //        //    var val = db.ReportingStruct.Find(int.Parse(ReportingLevel3));
        //        //    Emp.ReportingLevel3 = val;

        //        //    var type = db.Employee.Include(e => e.ReportingLevel3).Where(e => e.Id == data).SingleOrDefault();

        //        //    if (type.ReportingLevel3 != null)
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.ReportingLevel3.Id == type.ReportingLevel3.Id && x.Id == data).SingleOrDefault();
        //        //    }
        //        //    else
        //        //    {
        //        //        typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //        //    }
        //        //    typedetails.Gender = Emp.Gender;
        //        //}
        //        //else
        //        //{
        //        //    typedetails = db.Employee.Include(e => e.ReportingLevel3).Where(x => x.Id == data).SingleOrDefault();
        //        //    typedetails.ReportingLevel3 = null;
        //        //}
        //        List<ReportingStructRights> ReportingStructRightslist = new List<ReportingStructRights>();
        //        typedetails = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == data).SingleOrDefault();
        //        if (ReportingStructRights != null && ReportingStructRights != "")
        //        {
        //            var ids = Utility.StringIdsToListIds(ReportingStructRights);
        //            foreach (var ca in ids)
        //            {
        //                var PayScaleval_val = db.ReportingStructRights.Find(ca);
        //                ReportingStructRightslist.Add(PayScaleval_val);
        //                typedetails.ReportingStructRights = ReportingStructRightslist;
        //            }
        //        }
        //        else
        //        {
        //            typedetails.ReportingStructRights = null;
        //        }
        //        //AccountType
        //        if (MaritalStatus != null & MaritalStatus != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(MaritalStatus));
        //            Emp.MaritalStatus = val;

        //            var type = db.Employee.Include(e => e.MaritalStatus).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.MaritalStatus != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.MaritalStatus.Id == type.MaritalStatus.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.MaritalStatus = Emp.MaritalStatus;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.MaritalStatus).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.MaritalStatus = null;
        //        }
        //        /* end */

        //        //Paymode
        //        if (Gender != null & Gender != "")
        //        {
        //            var val = db.LookupValue.Find(int.Parse(Gender));
        //            Emp.Gender = val;

        //            var type = db.Employee.Include(e => e.Gender).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.Gender != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.Gender.Id == type.Gender.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.Gender = Emp.Gender;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.Gender).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.Gender = null;
        //        }
        //        // Paymode end */


        //        if (ResAddrs != null & ResAddrs != "")
        //        {
        //            var val = db.Address.Find(int.Parse(ResAddrs));
        //            Emp.ResAddr = val;

        //            var type = db.Employee.Include(e => e.ResAddr).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.ResAddr != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.ResAddr.Id == type.ResAddr.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.ResAddr = Emp.ResAddr;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.ResAddr).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.ResAddr = null;
        //        }

        //        if (PermAddrs != null & PermAddrs != "")
        //        {
        //            var val = db.Address.Find(int.Parse(PermAddrs));
        //            Emp.PerAddr = val;

        //            var type = db.Employee.Include(e => e.PerAddr).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.PerAddr != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.PerAddr.Id == type.PerAddr.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.PerAddr = Emp.PerAddr;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.PerAddr).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.PerAddr = null;
        //        }


        //        if (CorrsAddrs != null & CorrsAddrs != "")
        //        {
        //            var val = db.Address.Find(int.Parse(CorrsAddrs));
        //            Emp.CorAddr = val;

        //            var type = db.Employee.Include(e => e.CorAddr).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.CorAddr != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.CorAddr.Id == type.CorAddr.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.CorAddr = Emp.CorAddr;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.CorAddr).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.CorAddr = null;
        //        }


        //        if (ResContactDetails != null & ResContactDetails != "")
        //        {
        //            var val = db.ContactDetails.Find(int.Parse(ResContactDetails));
        //            Emp.ResContact = val;

        //            var type = db.Employee.Include(e => e.ResContact).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.ResContact != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.ResContact.Id == type.ResContact.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.ResContact = Emp.ResContact;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.ResContact).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.ResContact = null;
        //        }

        //        if (PermContactDetails != null & PermContactDetails != "")
        //        {
        //            var val = db.ContactDetails.Find(int.Parse(PermContactDetails));
        //            Emp.PerContact = val;

        //            var type = db.Employee.Include(e => e.PerContact).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.PerContact != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.PerContact.Id == type.PerContact.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.PerContact = Emp.PerContact;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.PerContact).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.PerContact = null;
        //        }


        //        if (CorrsContactDetails != null & CorrsContactDetails != "")
        //        {
        //            var val = db.ContactDetails.Find(int.Parse(CorrsContactDetails));
        //            Emp.CorContact = val;

        //            var type = db.Employee.Include(e => e.CorContact).Where(e => e.Id == data).SingleOrDefault();

        //            if (type.CorContact != null)
        //            {
        //                typedetails = db.Employee.Where(x => x.CorContact.Id == type.CorContact.Id && x.Id == data).SingleOrDefault();
        //            }
        //            else
        //            {
        //                typedetails = db.Employee.Where(x => x.Id == data).SingleOrDefault();
        //            }
        //            typedetails.CorContact = Emp.CorContact;
        //        }
        //        else
        //        {
        //            typedetails = db.Employee.Include(e => e.CorContact).Where(x => x.Id == data).SingleOrDefault();
        //            typedetails.CorContact = null;
        //        }

        //        if (Emp.ServiceBookDates != null)
        //        {
        //            Employee Employee = db.Employee.Include(e => e.ServiceBookDates)
        //                                             .Where(e => e.Id == data).SingleOrDefault();
        //            Employee.ServiceBookDates.BirthDate = Emp.ServiceBookDates.BirthDate;
        //            Employee.ServiceBookDates.ConfirmationDate = Emp.ServiceBookDates.ConfirmationDate;
        //            Employee.ServiceBookDates.ConfirmPeriod = Emp.ServiceBookDates.ConfirmPeriod;
        //            Employee.ServiceBookDates.JoiningDate = Emp.ServiceBookDates.JoiningDate;
        //            Employee.ServiceBookDates.LastIncrementDate = Emp.ServiceBookDates.LastIncrementDate;
        //            Employee.ServiceBookDates.LastPromotionDate = Emp.ServiceBookDates.LastPromotionDate;
        //            Employee.ServiceBookDates.LastTransferDate = Emp.ServiceBookDates.LastTransferDate;
        //            Employee.ServiceBookDates.ProbationDate = Emp.ServiceBookDates.ProbationDate;
        //            Employee.ServiceBookDates.ProbationPeriod = Emp.ServiceBookDates.ProbationPeriod;
        //            Employee.ServiceBookDates.ResignationDate = Emp.ServiceBookDates.ResignationDate;
        //            Employee.ServiceBookDates.ResignReason = Emp.ServiceBookDates.ResignReason;
        //            Employee.ServiceBookDates.RetirementDate = Emp.ServiceBookDates.RetirementDate;
        //            Employee.ServiceBookDates.SeniorityDate = Emp.ServiceBookDates.SeniorityDate;
        //            Employee.ServiceBookDates.SeniorityNo = Emp.ServiceBookDates.SeniorityNo;
        //            Employee.ServiceBookDates.ServiceLastDate = Emp.ServiceBookDates.ServiceLastDate;
        //            db.Employee.Attach(Employee);
        //            db.Entry(Employee).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            db.Entry(Employee).State = System.Data.Entity.EntityState.Detached;
        //        }


        //        if (Emp.BeforeMarriageName != null)
        //        {
        //            var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.BeforeMarriageName.Id).SingleOrDefault();
        //            typedetails.BeforeMarriageName = NameObj;

        //        }


        //        if (Emp.EmpName != null)
        //        {
        //            var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.EmpName.Id).SingleOrDefault();
        //            typedetails.EmpName = NameObj;
        //        }


        //        if (Emp.FatherName != null)
        //        {
        //            var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.FatherName.Id).SingleOrDefault();
        //            typedetails.FatherName = NameObj;
        //        }


        //        if (Emp.HusbandName != null)
        //        {
        //            var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.HusbandName.Id).SingleOrDefault();
        //            typedetails.HusbandName = NameObj;
        //        }

        //        if (Emp.MotherName != null)
        //        {
        //            var NameObj = db.NameSingle.Include(e => e.EmpTitle).Where(e => e.Id == Emp.MotherName.Id).SingleOrDefault();
        //            typedetails.MotherName = NameObj;
        //        }

        //        if (Emp.GeoStruct != null)
        //        {
        //            Employee Employee = db.Employee.Include(e => e.GeoStruct)
        //                                             .Where(e => e.Id == data).SingleOrDefault();

        //            Employee.GeoStruct = new GeoStruct
        //            {
        //                Company = Emp.GeoStruct.Company,
        //                Corporate = Emp.GeoStruct.Corporate,
        //                Department = Emp.GeoStruct.Department,
        //                Division = Emp.GeoStruct.Division,
        //                Group = Emp.GeoStruct.Group,
        //                Location = Emp.GeoStruct.Location,
        //                Region = Emp.GeoStruct.Region,
        //                Unit = Emp.GeoStruct.Unit
        //            };
        //        }

        //        if (Emp.FuncStruct != null)
        //        {
        //            Employee Employee = db.Employee.Include(e => e.FuncStruct)
        //                                             .Where(e => e.Id == data).SingleOrDefault();

        //            Employee.FuncStruct = new FuncStruct
        //            {
        //                Job = Emp.FuncStruct.Job,
        //                JobPosition = Emp.FuncStruct.JobPosition
        //            };
        //        }

        //        if (Emp.PayStruct != null)
        //        {
        //            Employee Employee = db.Employee.Include(e => e.PayStruct)
        //                                             .Where(e => e.Id == data).SingleOrDefault();

        //            Employee.PayStruct = new PayStruct
        //            {
        //                Grade = Emp.PayStruct.Grade,
        //                JobStatus = Emp.PayStruct.JobStatus,
        //                Level = Emp.PayStruct.Level
        //            };
        //        }



        //        db.Employee.Attach(typedetails);
        //        db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        TempData["RowVersion"] = typedetails.RowVersion;
        //        db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;


        //        var CurEmp = db.Employee.Find(data);
        //        TempData["CurrRowVersion"] = CurEmp.RowVersion;
        //        db.Entry(CurEmp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            Emp.DBTrack = dbT;
        //            Employee employee = new Employee()
        //            {
        //                EmpCode = Emp.EmpCode == null ? "" : Emp.EmpCode.Trim(),
        //                BirthPlace = Emp.BirthPlace == null ? "" : Emp.BirthPlace.Trim(),
        //                CardCode = Emp.CardCode,
        //                Photo = Emp.Photo,
        //                TimingCode = Emp.TimingCode,
        //                ValidFromDate = Emp.ValidFromDate,
        //                ValidToDate = Emp.ValidToDate,
        //                DBTrack = Emp.DBTrack,
        //                Id = data
        //            };


        //            db.Employee.Attach(employee);
        //            db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(employee).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            db.SaveChanges();
        //            db.Entry(employee).State = System.Data.Entity.EntityState.Detached;

        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }

        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    Candidate Emp = db.Candidate.Include(e => e.BeforeMarriageName).Include(e => e.CorAddr).Include(e => e.CorContact)
                                             .Include(e => e.CanAcademicInfo).Include(e => e.CanMedicalInfo).Include(e => e.CanOffInfo)
                                             .Include(e => e.CanSocialInfo).Include(e => e.CanFamilyDetails).Include(e => e.FatherName)
                                             .Include(e => e.CanForeignTrips).Include(e => e.Gender)
                                             .Include(e => e.CanGuarantorDetails).Include(e => e.HusbandName).Include(e => e.InsuranceDetails)
                                             .Include(e => e.MaritalStatus).Include(e => e.MotherName).Include(e => e.CanNominees).Include(e => e.CanPassportDetails)
                                            .Include(e => e.PerAddr).Include(e => e.PerContact).Include(e => e.CanPreCompExp)
                                             .Include(e => e.ResAddr).Include(e => e.ResContact).Include(e => e.ServiceBookDates).Include(e => e.CanVisaDetails)
                                             .Include(e => e.CanWorkExp).FirstOrDefault(e => e.Id == data);

                    //Address add = corporates.Address;
                    //ContactDetails conDet = corporates.ContactDetails;
                    //LookupValue val = corporates.BusinessType;

                    NameSingle EmpName = Emp.CanName;
                    NameSingle MotherName = Emp.CanName;
                    NameSingle FatherName = Emp.CanName;
                    NameSingle HusbandName = Emp.CanName;
                    NameSingle BeforeMarriageName = Emp.CanName;

                    Address CorAddr = Emp.CorAddr;
                    ContactDetails CorContact = Emp.CorContact;
                    Address PerAddr = Emp.PerAddr;
                    ContactDetails PerContact = Emp.PerContact;
                    Address ResAddr = Emp.ResAddr;
                    ContactDetails ResContact = Emp.ResContact;
                    ServiceBookDates ServiceBookDates = Emp.ServiceBookDates;
                    //Nominees Nominees = Emp.Nominees;
                    EmpMedicalInfo EmpmedicalInfo = Emp.CanMedicalInfo;
                    EmpSocialInfo EmpSocialInfo = Emp.CanSocialInfo;
                    EmpAcademicInfo EmpAcademicInfo = Emp.CanAcademicInfo;
                    LookupValue Gender = Emp.Gender;
                    LookupValue MaritalStatus = Emp.MaritalStatus;

                    //
                    //var v2 = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.EmpSalStruct).Where(e => e.Employee.Id == Emp.Id).FirstOrDefault();
                    //if (v2.EmpSalStruct.Count != 0)
                    //{
                    //    Msg.Add("Salary Structure is Defined,So Unable to Delete ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //}

                    //var v = db.EmployeePayroll.Include(e => e.Employee).Where(e => e.Employee.Id == Emp.Id).FirstOrDefault();
                    //if (v != null)
                    //{
                    //    db.EmployeePayroll.Remove(v);
                    //    db.SaveChanges();
                    //}


                    //var v1 = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == Emp.Id).FirstOrDefault();
                    //if (v1 != null)
                    //{
                    //    db.EmployeeLeave.Remove(v1);
                    //    db.SaveChanges();
                    //}

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
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, Emp.DBTrack);
                            //DT_Candidate DT_Emp = (DT_Candidate)rtn_Obj;
                            //DT_Emp.CanName_Id = Emp.CanName == null ? 0 : Emp.CanName.Id;
                            //DT_Emp.MotherName_Id = Emp.MotherName == null ? 0 : Emp.MotherName.Id;
                            //DT_Emp.FatherName_Id = Emp.FatherName == null ? 0 : Emp.FatherName.Id;
                            //DT_Emp.HusbandName_Id = Emp.HusbandName == null ? 0 : Emp.HusbandName.Id;
                            //DT_Emp.BeforeMarriageName_Id = Emp.BeforeMarriageName == null ? 0 : Emp.BeforeMarriageName.Id;

                            //DT_Emp.CorAddr_Id = Emp.CorAddr == null ? 0 : Emp.CorAddr.Id;
                            //DT_Emp.CorContact_Id = Emp.CorContact == null ? 0 : Emp.CorContact.Id;
                            //DT_Emp.PerAddr_Id = Emp.PerAddr == null ? 0 : Emp.PerAddr.Id;
                            //DT_Emp.PerContact_Id = Emp.PerContact == null ? 0 : Emp.PerContact.Id;
                            //DT_Emp.ResAddr_Id = Emp.ResAddr == null ? 0 : Emp.ResAddr.Id;
                            //DT_Emp.ResContact_Id = Emp.ResContact == null ? 0 : Emp.ResContact.Id;
                            //DT_Emp.ServiceBookDates_Id = Emp.ServiceBookDates == null ? 0 : Emp.ServiceBookDates.Id;
                            //DT_Emp.CanNominees_Id = Emp.CanNominees == null ? 0 : Emp.CanNominees.Id;
                            //DT_Emp.CanMedicalInfo_Id = Emp.CanMedicalInfo == null ? 0 : Emp.CanMedicalInfo.Id;
                            //DT_Emp.CanSocialInfo_Id = Emp.CanSocialInfo == null ? 0 : Emp.CanSocialInfo.Id;
                            //DT_Emp.CanAcademicInfo_Id = Emp.CanAcademicInfo == null ? 0 : Emp.CanAcademicInfo.Id; 
                            //DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Emp.Gender.Id;
                            //DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : Emp.MaritalStatus.Id;

                            //db.Create(DT_Emp);
                            //// db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data will be removed after authorised  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { "", "Data will be removed after authorised.", JsonRequestBehavior.AllowGet });
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
                                //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, dbT);
                                //DT_Candidate DT_Emp = (DT_Candidate)rtn_Obj;
                                //DT_Emp.CanName_Id = EmpName == null ? 0 : EmpName.Id;
                                //DT_Emp.MotherName_Id = MotherName == null ? 0 : MotherName.Id;
                                //DT_Emp.FatherName_Id = FatherName == null ? 0 : FatherName.Id;
                                //DT_Emp.HusbandName_Id = HusbandName == null ? 0 : HusbandName.Id;
                                //DT_Emp.BeforeMarriageName_Id = BeforeMarriageName == null ? 0 : BeforeMarriageName.Id; 
                                //DT_Emp.CorAddr_Id = CorAddr == null ? 0 : CorAddr.Id;
                                //DT_Emp.CorContact_Id = CorContact == null ? 0 : CorContact.Id;
                                //DT_Emp.PerAddr_Id = PerAddr == null ? 0 : PerAddr.Id;
                                //DT_Emp.PerContact_Id = PerContact == null ? 0 : PerContact.Id;
                                //DT_Emp.ResAddr_Id = ResAddr == null ? 0 : ResAddr.Id;
                                //DT_Emp.ResContact_Id = ResContact == null ? 0 : ResContact.Id;
                                //DT_Emp.ServiceBookDates_Id = ServiceBookDates == null ? 0 : ServiceBookDates.Id; 
                                //DT_Emp.CanMedicalInfo_Id = EmpmedicalInfo == null ? 0 : EmpmedicalInfo.Id;
                                //DT_Emp.CanSocialInfo_Id = EmpSocialInfo == null ? 0 : EmpSocialInfo.Id;
                                //DT_Emp.CanAcademicInfo_Id = EmpAcademicInfo == null ? 0 : EmpAcademicInfo.Id; 
                                //DT_Emp.Gender_Id = Emp.Gender == null ? 0 : Gender.Id;
                                //DT_Emp.MaritalStatus_Id = Emp.MaritalStatus == null ? 0 : MaritalStatus.Id;

                                //db.Create(DT_Emp);
                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

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
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
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
                            db.Create(DT_Emp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("Record Authorised");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { Emp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
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
                                        Msg.Add("Record Authorised");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { Emp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
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
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(Emp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add("Record Authorised");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //  return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                IEnumerable<Candidate> Candidate = null;
                if (gp.IsAutho == true)
                {
                    Candidate = db.Candidate.Include(e => e.CanName).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Candidate = db.Candidate.Include(e => e.CanName).AsNoTracking().ToList();
                }

                IEnumerable<Candidate> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {

                    IE = Candidate;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.CanCode.ToString().Contains(gp.searchString))
                                 || (e.CanName.FullDetails.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                 || (e.Id.ToString().Contains(gp.searchString))
                                 ).Select(a => new Object[] { a.CanCode, a.CanName != null ? a.CanName.FullNameFML : "", a.Id }).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CanCode, a.CanName != null ? a.CanName.FullNameFML : "", a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Candidate;
                    Func<Candidate, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "CandidateCode" ? c.CanCode :
                                         gp.sidx == "FullDetails" ? c.CanName != null ? c.CanName.FullNameFML : "" : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CanCode, a.CanName != null ? a.CanName.FullNameFML : "", a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.CanCode, a.CanName != null ? a.CanName.FullDetails : "", a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.CanCode, a.CanName != null ? a.CanName.FullNameFML : "", a.Id }).ToList();
                    }
                    totalRecords = Candidate.Count();
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
                var rall = db.Candidate.Include(e => e.CanName).Where(e => e.CanName != null).Select(e => e.CanName.Id.ToString()).Distinct().ToList();
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
                            var lastDayOfMonth = DateTime.DaysInMonth(RetirDate.Value.Year, RetirDate.Value.Month);
                            string Month = RetirDate.Value.Month.ToString().Length == 1 ? "0" + RetirDate.Value.Month.ToString() : RetirDate.Value.Month.ToString();
                            RetirementDate = lastDayOfMonth + "/" + Month + "/" + RetirDate.Value.Year;
                        }
                        if (a.ServiceEndMonRegDay == true)
                        {
                            RetirDate = RetirDate.Value.AddDays(-1);
                            RetirementDate = RetirDate.ToString();
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

        public ActionResult GetCandDocsLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CandidateDocuments.Include(e => e.DocumentType).ToList();
                IEnumerable<CandidateDocuments> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.CandidateDocuments.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
    }
}