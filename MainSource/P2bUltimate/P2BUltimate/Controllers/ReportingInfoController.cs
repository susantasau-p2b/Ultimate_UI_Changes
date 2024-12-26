using System;
using P2b.Global;
using P2BUltimate.App_Start;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using P2BUltimate.Models;
using Payroll;
using P2BUltimate.Security;
using System.IO;
using Leave;
using System.Web.Mvc;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
namespace P2BUltimate.Controllers
{
    [AuthoriseManger]
    public class ReportingInfoController : Controller
    {
        // GET: ReportingInfo
        public ActionResult Index()
        {
            return View("~/Views/ReportingInfo/Index.cshtml");
        }
        public ActionResult GridDelete(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListIds(data);
                var main = ids[0];
                var emp = ids[1];
                var qurey = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == emp).SingleOrDefault();
                List<ReportingStructRights> ReportingStructRightsList = new List<ReportingStructRights>();
                foreach (var item in qurey.ReportingStructRights.ToList())
                {
                    if (item.Id != main)
                    {
                        ReportingStructRightsList.Add(item);
                    }
                }
               qurey.ReportingStructRights = ReportingStructRightsList;
               db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(Employee COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            try
            {
                var Rpttimstr = form["ReportingStructRightslist"] == "0" ? "" : form["ReportingStructRightslist"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                List<int> idse = null;
                List<int> ReportingStructRightsids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    idse = Utility.StringIdsToListIds(Emp);
                }
                else
                {
                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (Rpttimstr != null && Rpttimstr != "0" && Rpttimstr != "false")
                {
                    ReportingStructRightsids = Utility.StringIdsToListIds(Rpttimstr);
                }
                else
                {
                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                foreach (var id in idse)
                {
                    using (DataBaseContext db = new DataBaseContext())
                    {
                        var Employee = db.Employee.Include(e => e.ReportingStructRights).Where(e => e.Id == id).SingleOrDefault();

                        List<ReportingStructRights> oReportingStructRights = new List<ReportingStructRights>();
                        foreach (var item in ReportingStructRightsids)
                        {
                            var temp = db.ReportingStructRights.Where(e => e.Id == item).SingleOrDefault();
                            if (temp != null)
                            {
                                oReportingStructRights.Add(temp);
                            }
                        }
                        if (Employee.ReportingStructRights.Count() == 0)
                        {
                            Employee.ReportingStructRights = oReportingStructRights;
                        }
                        else
                        {
                            for (int i = 0; i < oReportingStructRights.Count; i++)
                            {
                                Employee.ReportingStructRights.Add(oReportingStructRights[i]);
                            }
                        }
                        db.Entry(Employee).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        try
                        {
                            int Compid = int.Parse(Convert.ToString(Session["CompId"]));
                            string connString = ConfigurationManager.ConnectionStrings["DataBaseContext"].ConnectionString;
                            using (SqlConnection con = new SqlConnection(connString))
                            {
                                using (SqlCommand cmd = new SqlCommand("Insert_EmpReporting", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    cmd.Parameters.Add("@CompCode", SqlDbType.VarChar).Value = db.Company.Where(e => e.Id == Compid).SingleOrDefault().Code;
                                    cmd.Parameters.Add("@EmpCode", SqlDbType.VarChar).Value = Employee.EmpCode;

                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception e)
                        { 
                            Msg.Add(e.InnerException.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                      
                    }
                }
                Msg.Add("  Data Saved successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
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
            }
        }
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }
        public class EmpRepoChildDataClass
        {
            public int Id { get; set; }
            public string EffectiveDate { get; set; }
            public string EndDate { get; set; }
            public string ReportingTimingStruct { get; set; }

            public string ReportingStruct { get; set; }

            public string AccessRights { get; set; }

            public string FuncModules { get; set; }

            public string FuncSubModules { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.Employee.Include(e => e.EmpName)
                        .Where(e => e.ReportingStructRights.Count > 0)
                        .ToList();
                    // for search
                    IEnumerable<Employee> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.EmpCode.ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                                              ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Employee, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmpCode : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderByDescending(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.EmpCode,
                                Name = item.EmpName != null ? item.EmpName.FullNameFML : null,
                                //JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job.Name : null,
                                //Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade.Name : null,
                                //Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
                            });
                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.EmpCode, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
        public ActionResult Get_EmpRepoTimingStructData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.Employee.Include(e => e.ReportingStructRights)
                        .Include(e => e.ReportingStructRights.Select(a => a.AccessRights))
                        .Include(e => e.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                        .Include(e => e.ReportingStructRights.Select(a => a.FuncModules))
                        .Include(e => e.ReportingStructRights.Select(a => a.FuncSubModules))
                        .Include(e => e.ReportingStructRights.Select(a => a.ReportingStruct))
                       .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpRepoChildDataClass> returndata = new List<EmpRepoChildDataClass>();
                        foreach (var item in db_data.ReportingStructRights.ToList())
                        {
                            returndata.Add(new EmpRepoChildDataClass
                            {
                                Id = item.Id,
                                AccessRights = item.AccessRights != null ? item.AccessRights.ActionName.LookupVal : null,
                                FuncModules = item.FuncModules != null ? item.FuncModules.LookupVal : null,
                                FuncSubModules = item.FuncSubModules != null ? item.FuncSubModules.LookupVal : null,
                                ReportingStruct = item.ReportingStruct != null ? item.ReportingStruct.FullDetails : null,
                            });

                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}