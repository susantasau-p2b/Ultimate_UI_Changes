using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using P2BUltimate.Models;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Core.MainController
{
    [AuthoriseManger]
    public class GeoStructController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/GeoStruct/Index.cshtml");
        }

        public ActionResult GetLookup_region(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Region.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Region.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Corporate.SelectMany(e => e.Regions).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_company(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Company.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Company.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Region.SelectMany(e => e.Companies).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_division(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Division.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Division.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Company.SelectMany(e => e.Divisions).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_location(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Location.Include(e => e.LocationObj).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Location.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Division.SelectMany(e => e.Locations).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_department(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Department.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Department.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Location.SelectMany(e => e.Department).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_group(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Group.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Group.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Department.SelectMany(e => e.Groups).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_unit(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Unit.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Unit.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Group.SelectMany(e => e.Units).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookup_jobposition(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JobPosition.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.JobPosition.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Job.SelectMany(e => e.JobPosition).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.JobPositionDesc }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookup_level(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Level.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.Level.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var list1 = db.Grade.Include(e => e.Levels).SelectMany(e => e.Levels).ToList();
                var list2 = fall.Except(list1);
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookup_jobstatus(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.JobStatus.Include(e => e.EmpStatus).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                        {
                            fall = db.JobStatus.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                        else
                        {
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        }
                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Get_Corporate()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<P2BUltimate.Models.Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var data = db.Corporate.ToList();
                foreach (var s in data)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = s.Id.ToString(),
                        value = s.Name
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Get_Region(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Corporate.Include(e => e.Regions).Where(e => e.Id == id).Select(e => e.Regions).SingleOrDefault();
                        foreach (var ca in query)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Corporate.Include(e => e.Regions).SelectMany(e => e.Regions).ToList();
                    var db_data = db.Region.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Company2(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Region.Include(e => e.Companies).Where(e => e.Id == id).Select(e => e.Companies).SingleOrDefault();
                        foreach (var ca in query)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Region.Include(e => e.Companies).SelectMany(e => e.Companies).ToList();
                    var db_data = db.Company.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Company1(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Region.Include(e => e.Companies).Where(e => e.Id == id).Select(e => e.Companies).SingleOrDefault();
                        foreach (var ca in query)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Region.Include(e => e.Companies).SelectMany(e => e.Companies).ToList();
                    var db_data = db.Company.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Company(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Region.Include(e => e.Companies).Where(e => e.Id == id).Select(e => e.Companies).SingleOrDefault();
                        foreach (var ca in query)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Region.Include(e => e.Companies).SelectMany(e => e.Companies).ToList();
                    var db_data = db.Company.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Division(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Company.Include(e => e.Divisions).Where(e => e.Id == id).Select(e => e.Divisions).SingleOrDefault();
                        if (query != null)
                        {

                            foreach (var ca in query)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    var reference_data = db.Company.Include(e => e.Divisions).SelectMany(e => e.Divisions).ToList();
                    var db_data = db.Division.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public bool Check_Location(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data.Count > 0)
                {

                    var query = db.Company.Include(e => e.Location).Where(e => data.Contains(e.Id)).Select(e => e.Location).SingleOrDefault();
                    if (query.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        public bool Check_Division(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data.Count > 0)
                {

                    var query = db.Company.Include(e => e.Divisions).Where(e => data.Contains(e.Id)).Select(e => e.Divisions).SingleOrDefault();
                    if (query.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        public ActionResult Get_Location(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        //company
                        var comploc = db.Company.Include(e => e.Location.Select(a => a.LocationObj)).Where(e => e.Id == id).SingleOrDefault();
                        if (comploc != null)
                        {
                            foreach (var ca in comploc.Location)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.FullDetails
                                });
                            }

                        }
                        else
                        {
                            //division
                            var query = db.Division.Include(e => e.Locations.Select(x => x.LocationObj)).Where(e => e.Id == id).SingleOrDefault();
                            if (query != null)
                            {
                                foreach (var ca in query.Locations)
                                {
                                    returndata.Add(new Utility.returndataclass
                                    {
                                        code = ca.Id.ToString(),
                                        value = ca.FullDetails
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    var reference_data = db.Division.Include(e => e.Locations).SelectMany(e => e.Locations).ToList();
                    if (reference_data.Count != 0)
                    {
                        var db_data = db.Location.Include(e => e.LocationObj).ToList();
                        var expact_list = db_data.Except(reference_data);
                        foreach (var s in expact_list)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = s.Id.ToString(),
                                value = s.FullDetails.ToString()
                            });
                        }
                    }
                    else
                    {
                        var a = db.Company.Include(e => e.Location).Include(e => e.Location.Select(z => z.LocationObj)).ToList();
                        var b = a.SelectMany(e => e.Location);
                      //  var b = db.Location.Include(e => e.LocationObj).ToList();
                       // var c = b.Except(a);
                        foreach (var i in b)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = i.Id.ToString(),
                                value = i.FullDetails.ToString()
                            });
                        }
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Department(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Location.Include(e => e.Department)
                           .Include(e => e.Department.Select(x =>x.Address))
                           .Include(e => e.Department.Select(x => x.DepartmentObj))
                            .Where(e => e.Id == id).SingleOrDefault();
                        foreach (var ca in query.Department)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.FullDetails.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Location.Include(e => e.Department)
                        .Include(e => e.Department.Select(x => x.Address))
                        .SelectMany(e => e.Department).ToList();
                    var db_data = db.Department.Include(e => e.DepartmentObj).Include(e => e.Address).ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.FullDetails.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Group(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Department.Include(e => e.Address).Include(e => e.Groups).Where(e => e.Id == id).Select(e => e.Groups).SingleOrDefault();
                        foreach (var ca in query)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Department.Include(e => e.Address).Include(e => e.Groups).SelectMany(e => e.Groups).ToList();
                    //var reference_data = db.Department.SelectMany(e => e.Groups).ToList();
                    var db_data = db.Group.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Unit(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Unit.Where(e => e.Id == id).ToList();
                        foreach (var ca in query)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.Name.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Group.Include(e => e.Units).SelectMany(e => e.Units).ToList();
                    var db_data = db.Unit.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_job(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var query = db.Job.ToList();
                foreach (var ca in query)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = ca.Id.ToString(),
                        value = ca.Name.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Get_jobPosition(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Job.Include(e => e.JobPosition).Where(e => e.Id == id).ToList();
                        foreach (var ca in query.SelectMany(e => e.JobPosition))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = "Code :" + ca.JobPositionCode.ToString() + ", Position :" + ca.JobPositionDesc.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Job.Include(e => e.JobPosition).SelectMany(e => e.JobPosition).ToList();
                    var db_data = db.JobPosition.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.JobPositionDesc.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_grade(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var query = db.Grade.ToList();
                foreach (var ca in query)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = ca.Id.ToString(),
                        value = ca.Name.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Get_level(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != 0)
                {
                    foreach (var s in data)
                    {
                        int id = Convert.ToInt32(s);
                        var query = db.Grade.Include(e => e.Levels).Where(e => e.Id == id).ToList();
                        foreach (var ca in query.SelectMany(e => e.Levels))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Id.ToString(),
                                value = ca.FullDetails.ToString()
                            });
                        }
                    }
                }
                else
                {
                    var reference_data = db.Grade.Include(e => e.Levels).SelectMany(e => e.Levels).ToList();
                    var db_data = db.Level.ToList();
                    var expact_list = db_data.Except(reference_data);
                    foreach (var s in expact_list)
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = s.Id.ToString(),
                            value = s.Name.ToString()
                        });
                    }
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Jobstatus(List<int> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                var query = db.JobStatus.Include(e => e.EmpActingStatus).Include(e => e.EmpStatus).ToList();
                foreach (var ca in query)
                {
                    returndata.Add(new Utility.returndataclass
                    {
                        code = ca.Id.ToString(),
                        value = ca.FullDetails.ToString()
                    });
                }
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public Corporate Map_Corporate(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    var id = Convert.ToInt32(ids);
                    var data = db.Corporate.Find(id);
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Region> Map_Region(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Region> _Regionlist = new List<Region>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var region = Convert.ToInt32(ca);
                        var data = db.Region.Find(region);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Company> Map_Company(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Company> _Regionlist = new List<Company>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var Company = Convert.ToInt32(ca);
                        var data = db.Company.Find(Company);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Division> Map_Division(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Division> _Regionlist = new List<Division>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var Company = Convert.ToInt32(ca);
                        var data = db.Division.Find(Company);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Location> Map_Location(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Location> _Regionlist = new List<Location>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var Company = Convert.ToInt32(ca);
                        var data = db.Location.Find(Company);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Department> Map_Department(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Department> _Regionlist = new List<Department>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var Company = Convert.ToInt32(ca);
                        var data = db.Department.Find(Company);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Group> Map_Group(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Group> _Regionlist = new List<Group>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var Company = Convert.ToInt32(ca);
                        var data = db.Group.Find(Company);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Unit> Map_Unit(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Unit> _Regionlist = new List<Unit>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var Company = Convert.ToInt32(ca);
                        var data = db.Unit.Find(Company);
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<Job> Map_Job(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Job> _Regionlist = new List<Job>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var data = db.Job.Find(Convert.ToInt32(ca));
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<JobPosition> Map_JobPosition(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<JobPosition> _Regionlist = new List<JobPosition>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var data = db.JobPosition.Find(Convert.ToInt32(ca));
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<Grade> Map_Grade(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Grade> _Regionlist = new List<Grade>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var data = db.Grade.Find(Convert.ToInt32(ca));
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<Level> Map_Level(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<Level> _Regionlist = new List<Level>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var data = db.Level.Find(Convert.ToInt32(ca));
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }
        public List<JobStatus> Map_JobStatus(String ids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ids != null)
                {
                    List<JobStatus> _Regionlist = new List<JobStatus>();
                    var id = Utility.StringIdsToListIds(ids);
                    foreach (var ca in id)
                    {
                        var data = db.JobStatus.Find(Convert.ToInt32(ca));
                        _Regionlist.Add(data);
                    }
                    return _Regionlist;
                }
                else
                {
                    return null;
                }
            }
        }

        public ActionResult Create(FormCollection form)
        {
            string corporate_ids = form["corporate-table"];
            string region_ids = form["region-table"];
            string company_ids = form["company-table"];
            string division_ids = form["division-table"];
            string location_ids = form["location-table"];
            string department_ids = form["department-table"];
            string group_ids = form["group-table"];
            string unit_ids = form["unit-table"];

            string fun_company_ids = form["company1-table"];
            string job_ids = form["job-table"];
            string jobposition_ids = form["jobposition-table"];

            string pay_company_ids = form["company2-table"];
            string grade_ids = form["grade-table"];
            string jobstatus_ids = form["jobstatus-table"];
            string level_ids = form["level-table"];

            string struct_type = form["struct_type"];
            //check structtype
            using (DataBaseContext db = new DataBaseContext())
            {
                if (struct_type.ToLower() == "geostruct")
                {
                    var Geostructlevel = 0;

                    if (!string.IsNullOrEmpty(corporate_ids))
                    {
                        Geostructlevel = 1;
                    }
                    if (!string.IsNullOrEmpty(region_ids))
                    {
                        Geostructlevel = 2;
                    }
                    if (!string.IsNullOrEmpty(company_ids))
                    {
                        Geostructlevel = 3;
                    }
                    if (!string.IsNullOrEmpty(division_ids))
                    {
                        Geostructlevel = 4;
                    }
                    if (!string.IsNullOrEmpty(location_ids))
                    {
                        Geostructlevel = 5;
                    }
                    if (!string.IsNullOrEmpty(department_ids))
                    {
                        Geostructlevel = 6;
                    }
                    if (!string.IsNullOrEmpty(group_ids))
                    {
                        Geostructlevel = 7;
                    }
                    if (!string.IsNullOrEmpty(unit_ids))
                    {
                        Geostructlevel = 8;
                    }
                    switch (Geostructlevel)
                    {
                        case 2:
                            //region
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 3:
                            //compnay
                            if (!string.IsNullOrEmpty(region_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Region..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(corporate_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 4:
                            //division
                            if (!string.IsNullOrEmpty(company_ids))
                            {
                                if (Utility.StringIdsToListIds(company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(region_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Region..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(corporate_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 5:
                            //location
                            if (!string.IsNullOrEmpty(division_ids))
                            {
                                if (Utility.StringIdsToListIds(division_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Division..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(company_ids))
                            {
                                if (Utility.StringIdsToListIds(company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(region_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Region..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(corporate_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 6:
                            //deprartment
                            if (!string.IsNullOrEmpty(location_ids))
                            {
                                if (Utility.StringIdsToListIds(location_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Location..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(division_ids))
                            {
                                if (Utility.StringIdsToListIds(division_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Division..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(company_ids))
                            {
                                if (Utility.StringIdsToListIds(company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(region_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Region..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(corporate_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 7:
                            //Group
                            if (!string.IsNullOrEmpty(department_ids))
                            {
                                if (Utility.StringIdsToListIds(department_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Department..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(location_ids))
                            {
                                if (Utility.StringIdsToListIds(location_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Location..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(division_ids))
                            {
                                if (Utility.StringIdsToListIds(division_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Division..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(company_ids))
                            {
                                if (Utility.StringIdsToListIds(company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(region_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Region..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(corporate_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 8:
                            //unit
                            if (!string.IsNullOrEmpty(group_ids))
                            {
                                if (Utility.StringIdsToListIds(group_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Group..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(department_ids))
                            {
                                if (Utility.StringIdsToListIds(department_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Department..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(location_ids))
                            {
                                if (Utility.StringIdsToListIds(location_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Location..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(division_ids))
                            {
                                if (Utility.StringIdsToListIds(division_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Division..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(company_ids))
                            {
                                if (Utility.StringIdsToListIds(company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(region_ids))
                            {
                                if (Utility.StringIdsToListIds(region_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Region..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(corporate_ids))
                            {
                                if (Utility.StringIdsToListIds(corporate_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Corporate..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (struct_type.ToLower() == "functionalstruct")
                {
                    var funlevel = 0;
                    if (!string.IsNullOrEmpty(fun_company_ids))
                    {
                        funlevel = 1;
                    }
                    if (!string.IsNullOrEmpty(job_ids))
                    {
                        funlevel = 2;
                    }
                    if (!string.IsNullOrEmpty(jobposition_ids))
                    {
                        funlevel = 3;
                    }
                    switch (funlevel)
                    {
                        case 2:
                            if (!string.IsNullOrEmpty(fun_company_ids))
                            {
                                if (Utility.StringIdsToListIds(fun_company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        //case 3:
                        //    if (!string.IsNullOrEmpty(jobposition_ids))
                        //    {
                        //        if (Utility.StringIdsToListIds(jobposition_ids).Count > 1)
                        //        {
                        //            return Json(new Object[] { "", "", "Select Only One JobPostion..!" }, JsonRequestBehavior.AllowGet);
                        //        }
                        //    }
                        //    break;
                    }

                }
                else if (struct_type.ToLower() == "paystruct")
                {
                    var paylevel = 0;
                    if (!string.IsNullOrEmpty(pay_company_ids))
                    {
                        paylevel = 1;
                    }
                    if (!string.IsNullOrEmpty(grade_ids))
                    {
                        paylevel = 2;
                    }
                    if (!string.IsNullOrEmpty(level_ids))
                    {
                        paylevel = 3;
                    }
                    if (!string.IsNullOrEmpty(jobstatus_ids))
                    {
                        paylevel = 3;
                    }
                    switch (paylevel)
                    {
                        case 2:
                            //grade
                            if (!string.IsNullOrEmpty(pay_company_ids))
                            {
                                if (Utility.StringIdsToListIds(pay_company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 3:
                            //level


                            if (!string.IsNullOrEmpty(grade_ids))
                            {
                                if (Utility.StringIdsToListIds(grade_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One JobPostion..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(pay_company_ids))
                            {
                                if (Utility.StringIdsToListIds(pay_company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One JobPostion..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                        case 4:
                            //jobstatus
                            if (!string.IsNullOrEmpty(level_ids))
                            {
                                if (Utility.StringIdsToListIds(level_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One JobPostion..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(grade_ids))
                            {
                                if (Utility.StringIdsToListIds(grade_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One JobPostion..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            if (!string.IsNullOrEmpty(pay_company_ids))
                            {
                                if (Utility.StringIdsToListIds(pay_company_ids).Count > 1)
                                {
                                    return Json(new Object[] { "", "", "Select Only One Company..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            break;
                    }

                }

                //GeoStruct _GeoStruct = new GeoStruct();
                //PayStruct _PayStruct = new PayStruct();
                //FuncStruct _FuncStruct = new FuncStruct();
                //check structtype

                if (!string.IsNullOrEmpty(job_ids) && !string.IsNullOrEmpty(jobposition_ids))
                {
                    if (job_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(job_ids);
                        var data_corporate = db.Job.Include(e => e.JobPosition).Where(e => e.Id == corp_id).SingleOrDefault();
                        //var Reg_Corp = db.Corporate.Include(e => e.Regions).Where(e => e.Id == corp_id).SingleOrDefault();
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<JobPosition> region = new List<JobPosition>();
                                var region_id = Utility.StringIdsToListIds(jobposition_ids);
                                if (region_id != null)
                                {
                                    foreach (var ca in region_id)
                                    {

                                        var Lookup_val = db.JobPosition.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                //Reg_Corp.Regions = region;
                                data_corporate.JobPosition = region;
                                db.Job.Attach(data_corporate);
                                db.Entry(data_corporate).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_corporate).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }

                    }
                }
                //grade
                if (!string.IsNullOrEmpty(grade_ids) && !string.IsNullOrEmpty(level_ids))
                {
                    if (grade_ids.Length == 1)
                    {
                        var grade_id = Convert.ToInt32(grade_ids);
                        var data_corporate = db.Grade.Include(e => e.Levels).Where(e => e.Id == grade_id).SingleOrDefault();
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Level> region = new List<Level>();
                                var region_id = Utility.StringIdsToListIds(level_ids);
                                if (region_id != null)
                                {
                                    foreach (var ca in region_id)
                                    {

                                        var Lookup_val = db.Level.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                //Reg_Corp.Regions = region;
                                data_corporate.Levels = region;
                                db.Grade.Attach(data_corporate);
                                db.Entry(data_corporate).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_corporate).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }

                    }
                }
                //link-region
                if (!string.IsNullOrEmpty(corporate_ids) && !string.IsNullOrEmpty(region_ids))
                {
                    if (corporate_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(corporate_ids);
                        var data_corporate = db.Corporate.Where(e => e.Id == corp_id).SingleOrDefault();
                        //var Reg_Corp = db.Corporate.Include(e => e.Regions).Where(e => e.Id == corp_id).SingleOrDefault();
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Region> region = new List<Region>();
                                var region_id = Utility.StringIdsToListIds(region_ids);
                                if (region_id != null)
                                {
                                    foreach (var ca in region_id)
                                    {

                                        var Lookup_val = db.Region.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                //Reg_Corp.Regions = region;
                                data_corporate.Regions = region;
                                db.Corporate.Attach(data_corporate);
                                db.Entry(data_corporate).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_corporate).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }

                    }
                }

                //company
                if (!string.IsNullOrEmpty(region_ids) && !string.IsNullOrEmpty(company_ids))
                {
                    if (region_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(region_ids);
                        var data_region = db.Region.Where(e => e.Id == corp_id).SingleOrDefault();

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Company> region = new List<Company>();
                                var company_id = Utility.StringIdsToListIds(company_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {

                                        var Lookup_val = db.Company.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                data_region.Companies = region;
                                // Reg_Corp.Companies = region;
                                db.Region.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                //return Json(new Object[] { "", "Created" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }
                }

                //division
                if (!string.IsNullOrEmpty(company_ids) && !string.IsNullOrEmpty(division_ids))
                {
                    if (company_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(company_ids);
                        var data_region = db.Company.Include(e => e.Divisions).Where(e => e.Id == corp_id).SingleOrDefault();

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Division> region = new List<Division>();
                                var company_id = Utility.StringIdsToListIds(division_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {

                                        var Lookup_val = db.Division.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                region.AddRange(data_region.Divisions);
                                data_region.Divisions = region;
                                db.Company.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }

                }

                //location
                if (!string.IsNullOrEmpty(division_ids) && !string.IsNullOrEmpty(location_ids))
                {
                    if (division_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(division_ids);
                        var data_region = db.Division.Where(e => e.Id == corp_id).SingleOrDefault();

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Location> region = new List<Location>();
                                var company_id = Utility.StringIdsToListIds(location_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {

                                        var Lookup_val = db.Location.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                data_region.Locations = region;
                                // Reg_Corp.Companies = region;
                                db.Division.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }
                }

                //Department
                if (!string.IsNullOrEmpty(location_ids) && !string.IsNullOrEmpty(department_ids))
                {
                    if (location_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(location_ids);
                        var data_region = db.Location.Where(e => e.Id == corp_id).SingleOrDefault();

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Department> region = new List<Department>();
                                var company_id = Utility.StringIdsToListIds(department_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {

                                        var Lookup_val = db.Department.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                data_region.Department = region;
                                // Reg_Corp.Companies = region;
                                db.Location.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }

                }
                //Group
                if (!string.IsNullOrEmpty(department_ids) && !string.IsNullOrEmpty(group_ids))
                {
                    if (department_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(location_ids);
                        var data_region = db.Department.Where(e => e.Id == corp_id).SingleOrDefault();
                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Group> region = new List<Group>();
                                var company_id = Utility.StringIdsToListIds(group_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {

                                        var Lookup_val = db.Group.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                data_region.Groups = region;
                                // Reg_Corp.Companies = region;
                                db.Department.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }
                }
                //Unit
                if (!string.IsNullOrEmpty(group_ids) && !string.IsNullOrEmpty(unit_ids))
                {
                    if (group_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(location_ids);
                        var data_region = db.Group.Where(e => e.Id == corp_id).SingleOrDefault();

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Unit> region = new List<Unit>();
                                var company_id = Utility.StringIdsToListIds(unit_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {
                                        var Lookup_val = db.Unit.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                data_region.Units = region;
                                // Reg_Corp.Companies = region;
                                db.Group.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }

                }
                //Corporate-Location
                if (!string.IsNullOrEmpty(company_ids) && !string.IsNullOrEmpty(location_ids))
                {
                    if (company_ids.Length == 1)
                    {
                        var corp_id = Convert.ToInt32(company_ids);
                        var data_region = db.Company.Where(e => e.Id == corp_id).SingleOrDefault();

                        try
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                List<Location> region = new List<Location>();
                                var company_id = Utility.StringIdsToListIds(location_ids);
                                if (company_id != null)
                                {
                                    foreach (var ca in company_id)
                                    {

                                        var Lookup_val = db.Location.Find(Convert.ToInt32(ca));
                                        region.Add(Lookup_val);
                                    }
                                }
                                data_region.Location = region;
                                db.Company.Attach(data_region);
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(data_region).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(e.InnerException.ToString());
                        }
                    }
                }
                //assign Company Id to all level 
                if (company_ids != null)
                {
                    var company_id = Utility.StringIdsToListIds(company_ids);
                    var id = Convert.ToInt32(company_id[0]);
                    var comp_data = db.Company.Include(e => e.Divisions).Where(e => e.Id == id).SingleOrDefault();

                    if (division_ids != null)
                    {
                        var division_id = Utility.StringIdsToListIds(division_ids);
                        List<Division> division = new List<Division>();
                        foreach (var ca in division_id)
                        {
                            var division_data = db.Division.Find(ca);
                            division.Add(division_data);
                        }
                        division.AddRange(comp_data.Divisions);
                        comp_data.Divisions = division;
                       
                    }
                    if (location_ids != null)
                    {
                        var location_id = Utility.StringIdsToListIds(location_ids);
                        List<Location> division = new List<Location>();
                        foreach (var ca in location_id)
                        {
                            var division_data = db.Location.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Location = division;
                    }
                    if (department_ids != null)
                    {
                        var department_id = Utility.StringIdsToListIds(department_ids);
                        List<Department> division = new List<Department>();
                        foreach (var ca in department_id)
                        {
                            var division_data = db.Department.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Department = division;
                    }
                    if (group_ids != null)
                    {
                        var group_id = Utility.StringIdsToListIds(group_ids);
                        List<Group> division = new List<Group>();
                        foreach (var ca in group_id)
                        {
                            var division_data = db.Group.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Group = division;
                    }
                    if (unit_ids != null)
                    {
                        var unit_id = Utility.StringIdsToListIds(unit_ids);
                        List<Unit> division = new List<Unit>();
                        foreach (var ca in unit_id)
                        {
                            var division_data = db.Unit.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Unit = division;
                    }
                    db.Company.Attach(comp_data);
                    db.Entry(comp_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(comp_data).State = System.Data.Entity.EntityState.Detached;
                }
                else
                {

                }
                //Assign Company to paystruct
                if (pay_company_ids != null)
                {
                    var company_id = Utility.StringIdsToListIds(pay_company_ids);
                    var id = Convert.ToInt32(company_id[0]);
                    var comp_data = db.Company.Where(e => e.Id == id).SingleOrDefault();

                    if (grade_ids != null)
                    {
                        var division_id = Utility.StringIdsToListIds(grade_ids);
                        List<Grade> division = new List<Grade>();
                        foreach (var ca in division_id)
                        {
                            var division_data = db.Grade.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Grade = division;
                    }
                    if (level_ids != null)
                    {
                        var location_id = Utility.StringIdsToListIds(level_ids);
                        List<Level> division = new List<Level>();
                        foreach (var ca in location_id)
                        {
                            var division_data = db.Level.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Level = division;
                    }
                    if (jobstatus_ids != null)
                    {
                        var department_id = Utility.StringIdsToListIds(jobstatus_ids);
                        List<JobStatus> division = new List<JobStatus>();
                        foreach (var ca in department_id)
                        {
                            var division_data = db.JobStatus.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.JobStatus = division;
                    }
                    db.Company.Attach(comp_data);
                    db.Entry(comp_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(comp_data).State = System.Data.Entity.EntityState.Detached;
                }
                //assign funsruct
                if (fun_company_ids != null)
                {
                    var company_id = Utility.StringIdsToListIds(fun_company_ids);
                    var id = Convert.ToInt32(company_id[0]);
                    var comp_data = db.Company.Where(e => e.Id == id).SingleOrDefault();

                    if (job_ids != null)
                    {
                        var division_id = Utility.StringIdsToListIds(job_ids);
                        List<Job> division = new List<Job>();
                        foreach (var ca in division_id)
                        {
                            var division_data = db.Job.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.Job = division;
                    }
                    if (jobposition_ids != null)
                    {
                        var location_id = Utility.StringIdsToListIds(jobposition_ids);
                        List<JobPosition> division = new List<JobPosition>();
                        foreach (var ca in location_id)
                        {
                            var division_data = db.JobPosition.Find(ca);
                            division.Add(division_data);
                        }
                        comp_data.JobPosition = division;
                    }

                    db.Company.Attach(comp_data);
                    db.Entry(comp_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(comp_data).State = System.Data.Entity.EntityState.Detached;
                }
                if (corporate_ids != null)
                {
                    var a = Map_Corporate(corporate_ids);
                    if (a != null)
                    {
                        _GeoStruct.Corporate = a;
                    }
                }

                if (region_ids != null)
                {
                    var b = Map_Region(region_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Region = i;
                            Save_GeoStruct();
                        }
                    }

                }

                if (company_ids != null)
                {
                    var b = Map_Company(company_ids);

                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Company = i;
                            Save_GeoStruct();
                        }
                    }
                }
                if (division_ids != null)
                {
                    var b = Map_Division(division_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Division = i;
                            Save_GeoStruct();
                        }
                    }
                }
                if (location_ids != null)
                {
                    var b = Map_Location(location_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Location = i;
                            Save_GeoStruct();
                        }
                    }
                }
                if (department_ids != null)
                {
                    var b = Map_Department(department_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Department = i;
                            Save_GeoStruct();
                        }
                    }
                }
                if (group_ids != null)
                {
                    var b = Map_Group(group_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Group = i;
                            Save_GeoStruct();
                        }
                    }
                }
                if (unit_ids != null)
                {
                    var b = Map_Unit(unit_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _GeoStruct.Unit = i;
                            Save_GeoStruct();
                        }
                    }
                }
                //create Fun
                if (fun_company_ids != null)
                {
                    var b = Map_Company(fun_company_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _FuncStruct.Company = i;
                            //Save_FuncStruct();
                        }
                    }
                }
                if (job_ids != null)
                {
                    var b = Map_Job(job_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _FuncStruct.Job = i;
                            if (jobposition_ids != null)
                            {
                                var c = Map_JobPosition(jobposition_ids);
                                if (c != null)
                                {
                                    foreach (var j in c)
                                    {
                                        _FuncStruct.JobPosition = j;
                                        Save_FuncStruct();
                                    }
                                }
                            }
                            else
                            {
                                Save_FuncStruct();
                            }
                        }
                    }
                }
                //if (jobposition_ids != null && job_ids == null)
                //{
                //    var b = Map_JobPosition(jobposition_ids);
                //    if (b != null)
                //    {
                //        foreach (var i in b)
                //        {
                //            _FuncStruct.JobPosition = i;
                //            Save_FuncStruct();
                //        }
                //    }
                //}
                //if (job_ids != null)
                //{
                //    var b = Map_Job(job_ids);
                //    if (b != null)
                //    {
                //        foreach (var i in b)
                //        {
                //            _FuncStruct.Job = i;
                //            Save_FuncStruct();
                //        }
                //    }
                //}
                //if (jobposition_ids != null)
                //{
                //    var b = Map_JobPosition(jobposition_ids);
                //    if (b != null)
                //    {
                //        foreach (var i in b)
                //        {
                //            _FuncStruct.JobPosition = i;
                //            Save_FuncStruct();
                //        }
                //    }
                //}
                if (pay_company_ids != null)
                {
                    var a = Map_Company(pay_company_ids);
                    if (a != null)
                    {
                        foreach (var i in a)
                        {
                            _PayStruct.Company = i;
                        }
                    }
                }
                if (grade_ids != null)
                {
                    var b = Map_Grade(grade_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _PayStruct.Grade = i;
                            if (level_ids != null)
                            {
                                var c = Map_Level(level_ids);
                                if (c != null)
                                {
                                    foreach (var j in c)
                                    {
                                        _PayStruct.Level = j;
                                        Save_PayStruct();
                                    }
                                }
                            }
                            else
                            {
                                Save_PayStruct();
                            }
                        }
                    }
                }
                if (level_ids != null && grade_ids == null)
                {
                    var b = Map_Level(level_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _PayStruct.Level = i;
                            Save_PayStruct();
                        }
                    }
                }
                if (jobstatus_ids != null)
                {
                    var b = Map_JobStatus(jobstatus_ids);
                    if (b != null)
                    {
                        foreach (var i in b)
                        {
                            _PayStruct.JobStatus = i;
                            Save_PayStruct();
                        }
                    }
                }
                return Json(new Object[] { "", "", "Record Updated" }, JsonRequestBehavior.AllowGet);
            }
        }

        private GeoStruct _GeoStruct = new GeoStruct();
        private PayStruct _PayStruct = new PayStruct();
        private FuncStruct _FuncStruct = new FuncStruct();

        private void Save_GeoStruct()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {


                    //Company checking
                    var OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Region == null && e.Corporate == null && e.Division == null
                                                        && e.Location == null && e.Department == null && e.Group == null && e.Unit == null).SingleOrDefault();

                    //Corporate checking
                    if (_GeoStruct.Company != null && _GeoStruct.Corporate != null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => (e.Company.Id == _GeoStruct.Company.Id && e.Corporate.Id == _GeoStruct.Corporate.Id)).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id, Corporate_Id = _GeoStruct.Corporate.Id };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Corporate != null && _GeoStruct.Region != null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => (e.Company.Id == _GeoStruct.Company.Id && e.Corporate.Id == _GeoStruct.Corporate.Id
                            && e.Region.Id == _GeoStruct.Region.Id)).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id, Corporate_Id = _GeoStruct.Corporate.Id, Region_Id = _GeoStruct.Region.Id };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }



                    if (_GeoStruct.Company != null && _GeoStruct.Division == null && _GeoStruct.Location == null && _GeoStruct.Department == null
                        && _GeoStruct.Group == null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Division == null
                            && e.Location == null && e.Department == null && e.Group == null && e.Unit == null).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            //using (TransactionScope ts = new TransactionScope())
                            //{
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            ////int CompId = _GeoStruct.Company.Id;
                            ////_GeoStruct.Company = null;
                            ////// db.GeoStruct.Attach(_GeoStruct);
                            ////db.GeoStruct.Add(_GeoStruct);
                            ////db.SaveChanges();

                            ////_GeoStruct.Company = db.Company.Find(CompId);
                            ////db.GeoStruct.Attach(_GeoStruct);
                            ////db.Entry(_GeoStruct).State = System.Data.Entity.EntityState.Modified;
                            ////db.SaveChanges();
                            ////db.Entry(_GeoStruct).State = System.Data.Entity.EntityState.Detached;
                            ///
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id /* don't set country */ };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();

                            //}

                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division != null && _GeoStruct.Location == null && _GeoStruct.Department == null
                        && _GeoStruct.Group == null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Division.Id == _GeoStruct.Division.Id
                            && e.Location == null && e.Department == null
                        && e.Group == null && e.Unit == null).SingleOrDefault();


                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id, Division_Id = _GeoStruct.Division.Id };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }


                    if (_GeoStruct.Company != null && _GeoStruct.Division != null && _GeoStruct.Location != null && _GeoStruct.Department == null
                        && _GeoStruct.Group == null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Division.Id == _GeoStruct.Division.Id
                            && e.Location.Id == _GeoStruct.Location.Id && e.Department == null
                        && e.Group == null && e.Unit == null).SingleOrDefault();


                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id, Division_Id = _GeoStruct.Division.Id, Location_Id = _GeoStruct.Location.Id };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division != null && _GeoStruct.Location != null && _GeoStruct.Department != null
                        && _GeoStruct.Group == null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Division.Id == _GeoStruct.Division.Id
                           && e.Location.Id == _GeoStruct.Location.Id && e.Department.Id == _GeoStruct.Department.Id
                           && e.Group == null && e.Unit == null).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id, Division_Id = _GeoStruct.Division.Id, Location_Id = _GeoStruct.Location.Id, Department_Id = _GeoStruct.Department.Id };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division != null && _GeoStruct.Location != null && _GeoStruct.Department != null
                        && _GeoStruct.Group != null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Division.Id == _GeoStruct.Division.Id
                           && e.Location.Id == _GeoStruct.Location.Id && e.Department.Id == _GeoStruct.Department.Id
                           && e.Group.Id == _GeoStruct.Group.Id && e.Unit == null).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct { DBTrack = _GeoStruct.DBTrack, Company_Id = _GeoStruct.Company.Id, Division_Id = _GeoStruct.Division.Id, Location_Id = _GeoStruct.Location.Id, Department_Id = _GeoStruct.Department.Id, Group_Id = _GeoStruct.Group.Id };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division != null && _GeoStruct.Location != null && _GeoStruct.Department != null
                        && _GeoStruct.Group != null && _GeoStruct.Unit != null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => (e.Company.Id == _GeoStruct.Company.Id && e.Division.Id == _GeoStruct.Division.Id
                          && e.Location.Id == _GeoStruct.Location.Id && e.Department.Id == _GeoStruct.Department.Id
                          && e.Group.Id == _GeoStruct.Group.Id && e.Unit.Id == _GeoStruct.Unit.Id)).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct
                            {
                                DBTrack = _GeoStruct.DBTrack,
                                Company_Id = _GeoStruct.Company.Id,
                                Division_Id = _GeoStruct.Division.Id,
                                Location_Id = _GeoStruct.Location.Id,
                                Department_Id = _GeoStruct.Department.Id,
                                Group_Id = _GeoStruct.Group.Id,
                                Unit_Id = _GeoStruct.Unit.Id
                            };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }


                    if (_GeoStruct.Company != null && _GeoStruct.Division == null && _GeoStruct.Location != null && _GeoStruct.Department == null
                        && _GeoStruct.Group == null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => e.Company.Id == _GeoStruct.Company.Id && e.Division == null
                            && e.Location.Id == _GeoStruct.Location.Id && e.Department == null
                        && e.Group == null && e.Unit == null).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct
                            {
                                DBTrack = _GeoStruct.DBTrack,
                                Company_Id = _GeoStruct.Company.Id,
                                Location_Id = _GeoStruct.Location.Id
                            };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division == null && _GeoStruct.Location != null && _GeoStruct.Department != null
                        && _GeoStruct.Group == null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => (e.Company.Id == _GeoStruct.Company.Id && e.Division == null
                           && e.Location.Id == _GeoStruct.Location.Id && e.Department.Id == _GeoStruct.Department.Id
                           && e.Group == null && e.Unit == null)).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct
                            {
                                DBTrack = _GeoStruct.DBTrack,
                                Company_Id = _GeoStruct.Company.Id,
                                Location_Id = _GeoStruct.Location.Id,
                                Department_Id = _GeoStruct.Department.Id
                            };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division == null && _GeoStruct.Location != null && _GeoStruct.Department != null
                        && _GeoStruct.Group != null && _GeoStruct.Unit == null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => (e.Company.Id == _GeoStruct.Company.Id && e.Division == null
                           && e.Location.Id == _GeoStruct.Location.Id && e.Department.Id == _GeoStruct.Department.Id
                           && e.Group.Id == _GeoStruct.Group.Id && e.Unit == null)).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct
                            {
                                DBTrack = _GeoStruct.DBTrack,
                                Company_Id = _GeoStruct.Company.Id,
                                Location_Id = _GeoStruct.Location.Id,
                                Department_Id = _GeoStruct.Department.Id,
                                Group_Id = _GeoStruct.Group.Id
                            };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_GeoStruct.Company != null && _GeoStruct.Division == null && _GeoStruct.Location != null && _GeoStruct.Department != null && _GeoStruct.Group != null && _GeoStruct.Unit != null)
                    {
                        OGeoStruct = null;
                        OGeoStruct = db.GeoStruct.Where(e => (e.Company.Id == _GeoStruct.Company.Id && e.Division == null
                          && e.Location.Id == _GeoStruct.Location.Id && e.Department.Id == _GeoStruct.Department.Id
                          && e.Group.Id == _GeoStruct.Group.Id && e.Unit.Id == _GeoStruct.Unit.Id)).SingleOrDefault();

                        if (OGeoStruct == null)
                        {
                            _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var GeoStruct = new GeoStruct
                            {
                                DBTrack = _GeoStruct.DBTrack,
                                Company_Id = _GeoStruct.Company.Id,
                                Location_Id = _GeoStruct.Location.Id,
                                Group_Id = _GeoStruct.Group.Id,
                                Unit_Id = _GeoStruct.Unit.Id
                            };
                            db.GeoStruct.Add(GeoStruct);
                            db.SaveChanges();
                        }
                    }

                    //if (OGeoStruct == null)
                    //{
                    //    _GeoStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    //    db.GeoStruct.Attach(_GeoStruct);
                    //    db.GeoStruct.Add(_GeoStruct);
                    //    db.SaveChanges();
                    //}
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void Save_PayStruct()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //Company checking
                    var OPayStruct = db.PayStruct.Where(e => e.Company.Id == _PayStruct.Company.Id && e.Grade == null && e.Level == null && e.JobStatus == null
                                                         ).SingleOrDefault();

                    //Corporate checking
                    if (_PayStruct.Company != null && _PayStruct.Grade == null && _PayStruct.Level == null && _PayStruct.JobStatus == null)
                    {
                        OPayStruct = null;
                        OPayStruct = db.PayStruct.Where(e => (e.Company.Id == _PayStruct.Company.Id && e.Grade == null && e.Level == null && e.JobStatus == null))
                            .SingleOrDefault();

                        if (OPayStruct == null)
                        {
                            _PayStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var PayStruct = new PayStruct { DBTrack = _PayStruct.DBTrack, Company_Id = _PayStruct.Company.Id };
                            db.PayStruct.Add(PayStruct);
                            db.SaveChanges();
                        }

                    }

                    if (_PayStruct.Company != null && _PayStruct.Grade != null && _PayStruct.Level == null && _PayStruct.JobStatus == null)
                    {
                        OPayStruct = null;
                        OPayStruct = db.PayStruct.Where(e => (e.Company.Id == _PayStruct.Company.Id && e.Grade.Id == _PayStruct.Grade.Id && e.Level == null
                            && e.JobStatus == null)).SingleOrDefault();

                        if (OPayStruct == null)
                        {
                            _PayStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var PayStruct = new PayStruct { DBTrack = _PayStruct.DBTrack, Company_Id = _PayStruct.Company.Id, Grade_Id = _PayStruct.Grade.Id };
                            db.PayStruct.Add(PayStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_PayStruct.Company != null && _PayStruct.Grade != null && _PayStruct.Level != null && _PayStruct.JobStatus == null)
                    {
                        OPayStruct = null;
                        OPayStruct = db.PayStruct.Where(e => (e.Company.Id == _PayStruct.Company.Id && e.Grade.Id == _PayStruct.Grade.Id && e.Level.Id == _PayStruct.Level.Id
                            && e.JobStatus == null)).SingleOrDefault();


                        if (OPayStruct == null)
                        {
                            _PayStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var PayStruct = new PayStruct { DBTrack = _PayStruct.DBTrack, Company_Id = _PayStruct.Company.Id, Grade_Id = _PayStruct.Grade.Id, Level_Id = _PayStruct.Level.Id };
                            db.PayStruct.Add(PayStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_PayStruct.Company != null && _PayStruct.Grade != null && _PayStruct.Level != null && _PayStruct.JobStatus != null)
                    {
                        OPayStruct = null;
                        OPayStruct = db.PayStruct.Where(e => (e.Company.Id == _PayStruct.Company.Id && e.Grade.Id == _PayStruct.Grade.Id && e.Level.Id == _PayStruct.Level.Id
                            && e.JobStatus.Id == _PayStruct.JobStatus.Id)).SingleOrDefault();


                        if (OPayStruct == null)
                        {
                            _PayStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var PayStruct = new PayStruct { DBTrack = _PayStruct.DBTrack, Company_Id = _PayStruct.Company.Id, Grade_Id = _PayStruct.Grade.Id, Level_Id = _PayStruct.Level.Id, JobStatus_Id = _PayStruct.JobStatus.Id };
                            db.PayStruct.Add(PayStruct);
                            db.SaveChanges();
                        }
                    }

                    if (_PayStruct.Company != null && _PayStruct.Grade != null && _PayStruct.Level == null && _PayStruct.JobStatus != null)
                    {
                        OPayStruct = null;
                        OPayStruct = db.PayStruct.Where(e => (e.Company.Id == _PayStruct.Company.Id && e.Grade.Id == _PayStruct.Grade.Id 
                            && e.JobStatus.Id == _PayStruct.JobStatus.Id)).SingleOrDefault();


                        if (OPayStruct == null)
                        {
                            _PayStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var PayStruct = new PayStruct { DBTrack = _PayStruct.DBTrack, Company_Id = _PayStruct.Company.Id, Grade_Id = _PayStruct.Grade.Id,  JobStatus_Id = _PayStruct.JobStatus.Id };
                            db.PayStruct.Add(PayStruct);
                            db.SaveChanges();
                        }
                    }

                    //if (OPayStruct == null)
                    //{
                    //    _PayStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    //    db.PayStruct.Attach(_PayStruct);
                    //    db.PayStruct.Add(_PayStruct);
                    //    db.SaveChanges();
                    //}
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        private void Save_FuncStruct()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    //Company checking
                    var OFuncStruct = db.FuncStruct.Where(e => e.Company.Id == _FuncStruct.Company.Id && e.Job == null && e.JobPosition == null).SingleOrDefault();

                    //Corporate checking
                    if (_FuncStruct.Company != null && _FuncStruct.Job == null && _FuncStruct.JobPosition == null)
                    {
                        OFuncStruct = null;
                        OFuncStruct = db.FuncStruct.Where(e => (e.Company.Id == _FuncStruct.Company.Id && e.Job == null && e.JobPosition == null))
                            .SingleOrDefault();

                        if (OFuncStruct == null)
                        {
                            _FuncStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var Funcstruct = new FuncStruct { DBTrack = _FuncStruct.DBTrack, Company_Id = _FuncStruct.Company.Id };
                            db.FuncStruct.Add(Funcstruct);
                            db.SaveChanges();
                        }
                    }

                    if (_FuncStruct.Company != null && _FuncStruct.Job != null && _FuncStruct.JobPosition == null)
                    {
                        OFuncStruct = null;
                        OFuncStruct = db.FuncStruct.Where(e => (e.Company.Id == _FuncStruct.Company.Id && e.Job.Id == _FuncStruct.Job.Id && e.JobPosition == null))
                             .SingleOrDefault();

                        if (OFuncStruct == null)
                        {
                            _FuncStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var Funcstruct = new FuncStruct { DBTrack = _FuncStruct.DBTrack, Company_Id = _FuncStruct.Company.Id, Job_Id = _FuncStruct.Job.Id };
                            db.FuncStruct.Add(Funcstruct);
                            db.SaveChanges();
                        }
                    }

                    if (_FuncStruct.Company != null && _FuncStruct.Job != null && _FuncStruct.JobPosition != null)
                    {
                        OFuncStruct = null;
                        OFuncStruct = db.FuncStruct.Where(e => (e.Company.Id == _FuncStruct.Company.Id && e.Job.Id == _FuncStruct.Job.Id && e.JobPosition.Id == _FuncStruct.JobPosition.Id))
                             .SingleOrDefault();

                        if (OFuncStruct == null)
                        {
                            _FuncStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            var Funcstruct = new FuncStruct { DBTrack = _FuncStruct.DBTrack, Company_Id = _FuncStruct.Company.Id, Job_Id = _FuncStruct.Job.Id, JobPosition_Id = _FuncStruct.JobPosition.Id };
                            db.FuncStruct.Add(Funcstruct);
                            db.SaveChanges();
                        }
                    }

                    //if (OFuncStruct == null)
                    //{
                    //    _FuncStruct.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    //    db.FuncStruct.Attach(_FuncStruct);
                    //    db.FuncStruct.Add(_FuncStruct);
                    //    db.SaveChanges();
                    //}
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public class GeoStructClass
        {
            public string Company { get; set; }
            public string Corporate { get; set; }
            public string Department { get; set; }
            public string Division { get; set; }
            public string Group { get; set; }
            public string Location { get; set; }
            public string Region { get; set; }
            public string Unit { get; set; }

            public string Id { get; set; }
        }
        public class PayStructClass
        {
            public string Id { get; set; }

            public string Grade { get; set; }

            public string Level { get; set; }

            public string JobStatus { get; set; }

            public string Company { get; set; }
        }
        public class FuncStructClass
        {
            public string Id { get; set; }


            public string JobPosition { get; set; }

            public string Job { get; set; }

            public string Company { get; set; }
        }

        public ActionResult P2BGrid_Geo(P2BGrid_Parameters gp)
        {
            List<string> Msg = new List<string>();
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                List<GeoStruct> GeoStruct = new List<GeoStruct>();
                List<GeoStructClass> GeoStructL = new List<GeoStructClass>();
                if (gp.IsAutho == true)
                {
                    GeoStruct = db.GeoStruct
                        .Include(e => e.Company)
                        .Include(e => e.Corporate)
                        .Include(e => e.Region)
                        .Include(e => e.Division)
                        .Include(e => e.Location)
                        .Include(e => e.Department)
                        .Include(e => e.Group)
                        .Include(e => e.Unit)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    GeoStruct = db.GeoStruct.Include(e => e.Company)
                        .Include(e => e.Corporate)
                        .Include(e => e.Region)
                        .Include(e => e.Division)
                        .Include(e => e.Location)
                        .Include(e => e.Location.LocationObj)
                        .Include(e => e.Department)
                        .Include(e => e.Department.DepartmentObj)
                        .Include(e => e.Group)
                        .Include(e => e.Unit)
                        .ToList();
                }

                foreach (var item in GeoStruct)
                {
                    GeoStructL.Add(new GeoStructClass
                    {
                        Id = item.Id.ToString(),
                        Corporate = item.Corporate != null ? "Corporate->" + item.Corporate.Name : "",
                        Region = item.Region != null ? "Region->" + item.Region.Name : "",
                        Company = item.Company != null ? "Company->" + item.Company.FullDetails : "",
                        Division = item.Division != null ? "Division->" + item.Division.Name : "",
                        Location = item.Location != null && item.Location.LocationObj != null ? "Location->" + item.Location.LocationObj.LocDesc : "",
                        Department = item.Department != null && item.Department.DepartmentObj != null ? "Department->" + item.Department.DepartmentObj.DeptDesc : "",
                        Group = item.Group != null ? "Group->" + item.Group.Name : "",
                        Unit = item.Unit != null ? "Unit->" + item.Unit.Name : "",
                    });
                }
                IEnumerable<GeoStructClass> IE = GeoStructL;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = GeoStructL;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Corporate, a.Region, a.Company, a.Division, a.Location, a.Department, a.Group, a.Unit })
                                .Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Corporate")
                            jsonData = IE.Select(a => new { a.Id, a.Corporate, a.Region, a.Division, a.Company, a.Location, a.Department, a.Group, a.Unit })
                                .Where((e => (e.Corporate.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Region")
                            jsonData = IE.Select(a => new { a.Id, a.Corporate, a.Region, a.Division, a.Company, a.Location, a.Department, a.Group, a.Unit })
                                .Where((e => (e.Region.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Corporate, a.Region, a.Company, a.Division, a.Location, a.Department, a.Group, a.Unit }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = GeoStructL;
                    Func<GeoStructClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Corporate" ? c.Corporate :
                                         gp.sidx == "Region" ? c.Region :
                                         gp.sidx == "Division" ? c.Division :
                                         gp.sidx == "Company" ? c.Company :
                                         gp.sidx == "Location" ? c.Location :
                                         gp.sidx == "Department" ? c.Department :
                                         gp.sidx == "Group" ? c.Group :
                                         gp.sidx == "Unit" ? c.Unit : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Corporate, a.Region, a.Company, a.Division, a.Location, a.Department, a.Group, a.Unit }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Corporate, a.Region, a.Company, a.Division, a.Location, a.Department, a.Group, a.Unit }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Corporate, a.Region, a.Company, a.Division, a.Location, a.Department, a.Group, a.Unit }).ToList();
                    }
                    totalRecords = GeoStruct.Count();
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
        public ActionResult P2BGrid_Pay(P2BGrid_Parameters gp)
        {

            List<string> Msg = new List<string>();
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                List<PayStruct> PayStruct = new List<PayStruct>();
                List<PayStructClass> PayStructL = new List<PayStructClass>();
                if (gp.IsAutho == true)
                {
                    PayStruct = db.PayStruct
                        .Include(e => e.Company)
                        .Include(e => e.Grade)
                        .Include(e => e.Level)
                        .Include(e => e.JobStatus)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    PayStruct = db.PayStruct
                        .Include(e => e.Company)
                        .Include(e => e.Grade)
                        .Include(e => e.Level)
                        .Include(e => e.JobStatus.EmpActingStatus)
                        .AsNoTracking().ToList();
                }

                foreach (var item in PayStruct)
                {
                    PayStructL.Add(new PayStructClass
                    {
                        Id = item.Id.ToString(),
                        Company = "Company->" + item.Company.FullDetails,
                        Grade = item.Grade != null ? "Grade->" + item.Grade.Name : null,
                        Level = item.Level != null ? "Level->" + item.Level.Name : null,
                        JobStatus = item.JobStatus != null ? "JobStatus->" + item.JobStatus.FullDetails : null,
                    });
                }
                IEnumerable<PayStructClass> IE = PayStructL;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PayStructL;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Company, a.Grade, a.Level, a.JobStatus })
                                .Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Company")
                            jsonData = IE.Select(a => new { a.Id, a.Company, a.Grade, a.Level, a.JobStatus })
                                .Where((e => (e.Company.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Region")
                            jsonData = IE.Select(a => new { a.Id, a.Company, a.Grade, a.Level, a.JobStatus })
                                .Where((e => (e.Grade.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Company, a.Grade, a.Level, a.JobStatus }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PayStructL;
                    Func<PayStructClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Company" ? c.Company :
                                         gp.sidx == "Grade" ? c.Grade :
                                         gp.sidx == "Level" ? c.Level :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Company, a.Grade, a.Level, a.JobStatus }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Company, a.Grade, a.Level, a.JobStatus }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Company, a.Grade, a.Level, a.JobStatus }).ToList();
                    }
                    totalRecords = PayStruct.Count();
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
        public ActionResult P2BGrid_Fun(P2BGrid_Parameters gp)
        {

            List<string> Msg = new List<string>();
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                List<FuncStruct> FuncStruct = new List<FuncStruct>();
                List<FuncStructClass> FuncStructL = new List<FuncStructClass>();
                if (gp.IsAutho == true)
                {
                    FuncStruct = db.FuncStruct
                        .Include(e => e.Company)
                        .Include(e => e.Job)
                        .Include(e => e.JobPosition)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    FuncStruct = db.FuncStruct
                         .Include(e => e.Company)
                        .Include(e => e.Job)
                        .Include(e => e.JobPosition)
                        .AsNoTracking().ToList();
                }

                foreach (var item in FuncStruct)
                {
                    FuncStructL.Add(new FuncStructClass
                    {
                        Id = item.Id.ToString(),
                        Company = "Company->" + item.Company.FullDetails,
                        Job = item.Job != null ? "Job->" + item.Job.Name : null,
                        JobPosition = item.JobPosition != null ? "JobPosition->" + item.JobPosition.JobPositionDesc : null,
                    });
                }
                IEnumerable<FuncStructClass> IE = FuncStructL;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = FuncStructL;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Job, a.JobPosition })
                                .Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Job")
                            jsonData = IE.Select(a => new { a.Id, a.Job, a.JobPosition })
                                .Where((e => (e.Job.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "JobPosition")
                            jsonData = IE.Select(a => new { a.Id, a.Job, a.JobPosition })
                                .Where((e => (e.JobPosition.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Company, a.Job, a.JobPosition }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = FuncStructL;
                    Func<FuncStructClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : "0");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Company" ? c.Company :
                             gp.sidx == "Job" ? c.Job :
                                         gp.sidx == "JobPosition" ? c.JobPosition :

                                       "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Company, a.Job, a.JobPosition }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Company, a.Job, a.JobPosition }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Company, a.Job, a.JobPosition }).ToList();
                    }
                    totalRecords = FuncStruct.Count();
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

    }
}