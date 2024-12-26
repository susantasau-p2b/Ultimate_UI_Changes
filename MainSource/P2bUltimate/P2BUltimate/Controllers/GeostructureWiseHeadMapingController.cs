using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Collections;
using System.Web.Mvc;


namespace P2BUltimate.Controllers
{
    public class GeostructureWiseHeadMapingController : Controller
    {
        List<string> msg = new List<string>();
        // GET: GeostructureWiseHeadMaping
        public ActionResult Index()
        {
            return View();
        }
        public class newReturnClass
        {
            public Array id { get; set; }
            public Array val { get; set; }
        };
        public ActionResult GetHolidayMaster(Int32 data, string type)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (type.ToUpper() == "LOCATION")
                {
                    var qurey = db.Location
                        .Include(a => a.HolidayCalendar)
                        .Include(a => a.HolidayCalendar.Select(e => e.HoliCalendar.Name))
                        .Include(a => a.HolidayCalendar.Select(e => e.HolidayList))
                        .Where(a => a.Id == data)
                        .SingleOrDefault();
                    var newList = new List<newReturnClass>();

                    newList.Add(new newReturnClass
                    {
                        id = qurey.HolidayCalendar.Select(A => A.Id.ToString()).ToArray(),
                        val = qurey.HolidayCalendar.Select(A => A.FullDetails).ToArray(),
                    });

                    return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                }
                if (type.ToUpper() == "DEPARTMENT")
                {
                    var qurey = db.Department
                        .Include(a => a.HolidayCalendar)
                        .Include(a => a.HolidayCalendar.Select(e => e.HoliCalendar.Name))
                        .Include(a => a.HolidayCalendar.Select(e => e.HolidayList))
                        .Where(a => a.Id == data)
                        .SingleOrDefault();
                    var newList = new List<newReturnClass>();

                    newList.Add(new newReturnClass
                    {
                        id = qurey.HolidayCalendar.Select(A => A.Id.ToString()).ToArray(),
                        val = qurey.HolidayCalendar.Select(A => A.FullDetails).ToArray(),
                    });

                    return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                } 

                return null;
            }
        }
        public ActionResult GetWeaklyoffMaster(Int32 data, string type)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (type.ToUpper()=="LOCATION")
                {
                var qurey = db.Location
                    .Include(a => a.WeeklyOffCalendar)
                    .Include(a => a.WeeklyOffCalendar.Select(e => e.WeeklyOffList))
                    .Include(a => a.WeeklyOffCalendar.Select(e => e.WOCalendar.Name))
                    .Where(a => a.Id == data)
                    .SingleOrDefault();
                var newList = new List<newReturnClass>();

                newList.Add(new newReturnClass
                {
                    id = qurey.WeeklyOffCalendar.Select(A => A.Id.ToString()).ToArray(),
                    val = qurey.WeeklyOffCalendar.Select(A => A.FullDetails).ToArray(),
                });
              
                return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                }
                if (type.ToUpper() == "DEPARTMENT")
                {
                    var qurey = db.Department
                        .Include(a => a.WeeklyOffCalendar)
                        .Include(a => a.WeeklyOffCalendar.Select(e => e.WeeklyOffList))
                        .Include(a => a.WeeklyOffCalendar.Select(e => e.WOCalendar.Name))
                        .Where(a => a.Id == data)
                        .SingleOrDefault();
                    var newList = new List<newReturnClass>();

                    newList.Add(new newReturnClass
                    {
                        id = qurey.WeeklyOffCalendar.Select(A => A.Id.ToString()).ToArray(),
                        val = qurey.WeeklyOffCalendar.Select(A => A.FullDetails).ToArray(),
                    });

                    return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }
        public ActionResult GetInCharge(Int32 data, string type)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (type.ToUpper() == "LOCATION")
                {
                    var qurey = db.Location
                        .Include(a => a.Incharge)

                        .Where(a => a.Id == data)
                      .ToList();
                    var newList = new List<newReturnClass>();

                    newList.Add(new newReturnClass
                    {
                        id = qurey.Select(a => a.Incharge.Id).ToArray(),
                        val = qurey.Select(a => a.Incharge.FullDetails).ToArray(),
                    });
               
                return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                }
                if (type.ToUpper() == "DEPARTMENT")
                {
                    var qurey = db.Department
                        .Include(a => a.Incharge)

                        .Where(a => a.Id == data)
                      .ToList();
                    var newList = new List<newReturnClass>();

                    newList.Add(new newReturnClass
                    {
                        id = qurey.Select(a => a.Incharge.Id).ToArray(),
                        val = qurey.Select(a => a.Incharge.FullDetails).ToArray(),
                    });

                    return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                }
                if (type.ToUpper() == "DIVISION")
                {
                    var qurey = db.Division
                        .Include(a => a.Incharge)

                        .Where(a => a.Id == data)
                      .ToList();
                    var newList = new List<newReturnClass>();

                    newList.Add(new newReturnClass
                    {
                        id = qurey.Select(a=>a.Incharge.Id).ToArray(),
                        val = qurey.Select(a => a.Incharge.FullDetails).ToArray(),
                    });

                    return Json(new { success = true, data = newList }, JsonRequestBehavior.AllowGet);
                }

                return null;
            }
        }
        public ActionResult Get_Data(List<String> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data.Count != 0 && data[0] != null)
                {
                    switch (data[0])
                    {
                        case "Corporate":
                            var query = db.Corporate.ToList();
                            foreach (var ca in query)
                            {
                                returndata.Add(new Utility.returndataclass

                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                            break;
                        case "Company":
                            var comp = db.Company.ToList();
                            foreach (var ca in comp)
                            {
                                returndata.Add(new Utility.returndataclass

                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                            break;
                        case "Region":

                            var Region = db.Region.ToList();
                            foreach (var ca in Region)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                            break;
                        case "Location":

                            var Location = db.Location.Include(a => a.LocationObj).ToList();
                            foreach (var ca in Location)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.LocationObj.LocDesc.ToString()
                                });
                            }
                            break;
                        case "Division":

                            var Division = db.Division.ToList();
                            foreach (var ca in Division)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                            break;
                        case "Department":

                            var Department = db.Department.Include(a => a.DepartmentObj).ToList();
                            foreach (var ca in Department)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.DepartmentObj.DeptDesc.ToString()
                                });
                            }
                            break;
                        case "Group":

                            var Group = db.Group.ToList();
                            foreach (var ca in Group)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                            break;
                        case "Unit":

                            var Unit = db.Unit.ToList();
                            foreach (var ca in Unit)
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = ca.Id.ToString(),
                                    value = ca.Name.ToString()
                                });
                            }
                            break;
                        default:
                            break;
                    }
                }

                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public List<String> GetFirstLevelStruc()
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //find first level
                var firstLevel = new List<String>();

                var Corporate = db.GeoStruct
                    .Include(a => a.Corporate)
                    .Where(a => a.Corporate != null).FirstOrDefault();
                if (Corporate != null)
                {
                    firstLevel.Add("Corporate");
                }
                var Region = db.GeoStruct
                    .Include(a => a.Region)
                    .Where(a => a.Region != null).FirstOrDefault();
                if (Region != null)
                {
                    firstLevel.Add("Region");
                    //   return firstLevel = "Region";
                }
                //var Company = db.GeoStruct
                //    .Include(a => a.Company)
                //    .Where(a => a.Company != null).FirstOrDefault();
                //if (Company != null)
                //{
                //    firstLevel.Add("Company");
                //    //   return firstLevel = "Company";
                //}
                var Location = db.GeoStruct
                    .Include(a => a.Location)
                    .Where(a => a.Location != null).FirstOrDefault();
                if (Location != null)
                {
                    firstLevel.Add("Location");
                    //   return firstLevel = "Location";
                }
                var Department = db.GeoStruct
                    .Include(a => a.Department)
                    .Where(a => a.Department != null).FirstOrDefault();
                if (Department != null)
                {
                    firstLevel.Add("Department");
                    // return firstLevel = "Department";
                }
                var Division = db.GeoStruct
                    .Include(a => a.Division)
                    .Where(a => a.Division != null).FirstOrDefault();
                if (Division != null)
                {
                    firstLevel.Add("Division");
                    //   return firstLevel = "Division";
                }
                var Group = db.GeoStruct
                    .Include(a => a.Group)
                    .Where(a => a.Group != null).FirstOrDefault();
                if (Group != null)
                {
                    firstLevel.Add("Group");
                    //     return firstLevel = "Group";
                }
                var Unit = db.GeoStruct
                    .Include(a => a.Unit)
                    .Where(a => a.Unit != null).FirstOrDefault();
                if (Unit != null)
                {
                    firstLevel.Add("Unit");
                }
                return firstLevel;
            }
        }
        public ActionResult GetStruc()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var firstLevel = GetFirstLevelStruc();
                return Json(new { data = firstLevel }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupDetailsIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee.Include(e => e.EmpName).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.Include(e => e.EmpName).ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "EmpCode :" + ca.EmpCode + " EmpName :" + ca.EmpName.FullNameFML }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { srno = c.Id, lookupvalue = "EmpCode :" + c.EmpCode + " EmpName :" + c.EmpName.FullNameFML }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult Create(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                var type = form["type"] != null ? form["type"] : null;
                if (type == null)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
                var Objtype = form["Obj_drop"] != null ? form["Obj_drop"] : null;
                if (Objtype == null)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
                var HolidayMasterlist = form["HolidayMasterlist"] != null ? form["HolidayMasterlist"] : null;
                var WeaklyoffMasterlist = form["WeaklyoffMasterlist"] != null ? form["WeaklyoffMasterlist"] : null;
                var InChargelist = form["InChargelist"] != null ? form["InChargelist"] : null;

                var HolidayCalendar = new List<HolidayCalendar>();
                var WeeklyOffCalendar = new List<WeeklyOffCalendar>();
                var Incharge = new List<Employee>();

                switch (Objtype)
                {
                    case "HolidayMaster":
                        var ids = Utility.StringIdsToListIds(HolidayMasterlist);
                        foreach (var item in ids)
                        {
                            var aa = db.HolidayCalendar.Where(a => a.Id == item).FirstOrDefault();
                            HolidayCalendar.Add(aa);
                        }
                        break;
                    case "WeaklyoffMaster":
                        var ids1 = Utility.StringIdsToListIds(WeaklyoffMasterlist);
                        foreach (var item in ids1)
                        {
                            var aa = db.WeeklyOffCalendar.Where(a => a.Id == item).FirstOrDefault();
                            WeeklyOffCalendar.Add(aa);
                        }
                        break;
                    case "InCharge":
                        var ids2 = Utility.StringIdsToListIds(InChargelist);
                        foreach (var item in ids2)
                        {
                            var aa = db.Employee.Where(a => a.Id == item).FirstOrDefault();
                            Incharge.Add(aa);
                        }
                        break;
                    default:
                        break;
                }
                using (TransactionScope ts = new TransactionScope())
                {
                    switch (type.ToUpper())
                    {
                        case "LOCATION":
                       
                            var ids = form["corporate-table"] != null ? form["corporate-table"] : null;
                            var ids1 = Utility.StringIdsToListIds(ids);
                            if (Objtype == "HolidayMaster")
                            {
                                foreach (var item in ids1)
                                {
                                    var oLocation = db.Location.Include(a => a.HolidayCalendar).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.HolidayCalendar = HolidayCalendar;
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else if (Objtype == "WeaklyoffMaster")
                            {
                                foreach (var item in ids1)
                                {
                                    var oLocation = db.Location.Include(a => a.HolidayCalendar).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.WeeklyOffCalendar = WeeklyOffCalendar;
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else if (Objtype == "InCharge")
                            {
                                foreach (var item in ids1)
                                {
                                    var oLocation = db.Location.Include(a => a.Incharge).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.Incharge = Incharge.FirstOrDefault();
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            break;

                        case "DEPARTMENT":

                            var idss = form["corporate-table"] != null ? form["corporate-table"] : null;
                            var idss1 = Utility.StringIdsToListIds(idss);
                            if (Objtype == "HolidayMaster")
                            {
                                foreach (var item in idss1)
                                {
                                    var oLocation = db.Department.Include(a => a.HolidayCalendar).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.HolidayCalendar = HolidayCalendar;
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else if (Objtype == "WeaklyoffMaster")
                            {
                                foreach (var item in idss1)
                                {
                                    var oLocation = db.Department.Include(a => a.HolidayCalendar).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.WeeklyOffCalendar = WeeklyOffCalendar;
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else if (Objtype == "InCharge")
                            {
                                foreach (var item in idss1)
                                {
                                    var oLocation = db.Department.Include(a => a.Incharge).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.Incharge = Incharge.FirstOrDefault();
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            break;


                        case "DIVISION":

                            var idsss = form["corporate-table"] != null ? form["corporate-table"] : null;
                            var idsss1 = Utility.StringIdsToListIds(idsss);
                          
                            if (Objtype == "InCharge")
                            {
                                foreach (var item in idsss1)
                                {
                                    var oLocation = db.Division.Include(a => a.Incharge).Where(a => a.Id == item).SingleOrDefault();
                                    oLocation.Incharge = Incharge.FirstOrDefault();
                                    db.Entry(oLocation).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            break;


                        default:
                            break;
                    }
                    ts.Complete();
                    msg.Add("Record Saved..!");
                    return Json(new { success = true, responseText = msg }, JsonRequestBehavior.AllowGet);
                
                }
            }
          
        }
    }
}