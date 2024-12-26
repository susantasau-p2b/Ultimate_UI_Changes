using P2b.Global;
using Payroll;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;
using Attendance;
using System.Web.Script.Serialization;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;
using P2BUltimate.Process;
namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class OrgTimingPolicyBatchAssignmentController : Controller
    {
        public ActionResult Index()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.CompanyId);
                var check = db.CompanyAttendance.Where(e => e.Company.Id == id).SingleOrDefault();
                if (check == null)
                {
                    CompanyAttendance oCompanyAttendance = new CompanyAttendance
                    {
                        Company = db.Company.Where(e => e.Id == id).SingleOrDefault(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                    };
                    db.CompanyAttendance.Add(oCompanyAttendance);
                    db.SaveChanges();
                }
            }
            return View("~/Views/Attendance/MainViews/OrgTimingPolicyBatchAssignment/Index.cshtml");
        }

        public ActionResult GetGeostructTemp(string geostruct_id)
        {
            TempData["GetGeoStructTemp"] = geostruct_id;
            return null;
        }
        List<string> Msg = new List<string>();
        public ActionResult GetGeostruct(string data, string boolcheck)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> ids = null;
                IEnumerable<GeoStruct> all;
                IEnumerable<GeoStruct> fall12;
                var fall11 = db.GeoStruct.Include(e => e.Company)
                    .Include(a => a.Location.LocationObj)
                    .Include(a => a.Department.DepartmentObj)
                    .Where(e => e.Company != null && e.Location != null).ToList();
                var geostructid = TempData["GetGeoStructTemp"];
                if (geostructid != null && boolcheck == "1")
                {
                    string geostruct = Convert.ToString(geostructid);
                    if (!string.IsNullOrEmpty(geostruct))
                    {
                        ids = Utility.StringIdsToListIds(geostruct);

                        fall12 = fall11.Where(e => ids.Contains(e.Id));

                        var r = (from ca in fall12 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                        JsonResult json1 = Json(r, JsonRequestBehavior.AllowGet);
                        json1.MaxJsonLength = int.MaxValue;
                        return json1;
                        //return Json(r, JsonRequestBehavior.AllowGet);

                    }
                }
                else if (geostructid == null && boolcheck == "1")
                {
                    return null;
                }
                else
                {
                    fall12 = fall11;
                    var r = (from ca in fall12 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    JsonResult json1 = Json(r, JsonRequestBehavior.AllowGet);
                    json1.MaxJsonLength = int.MaxValue;
                    return json1;
                    //return Json(r, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.GeoStruct.ToList().Where(d => d.Location != null && d.FullDetails.Contains(data));
                    var result = (from c in all
                                  select new { c.Id, c.FullDetails }).Distinct();
                    JsonResult json2 = Json(result, JsonRequestBehavior.AllowGet);
                    json2.MaxJsonLength = int.MaxValue;
                    return json2;
                    //return Json(result, JsonRequestBehavior.AllowGet);

                }
                return null;
                //else
                //{
                //    var r = (from ca in fall12 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                //    JsonResult json1 = Json(r, JsonRequestBehavior.AllowGet);
                //    json1.MaxJsonLength = int.MaxValue;
                //    return json1;
                //    //return Json(r, JsonRequestBehavior.AllowGet);
                //}
                //var result = (from c in all
                //              select new { c.Id, c.FullDetails }).Distinct();
                //JsonResult json2 = Json(result, JsonRequestBehavior.AllowGet);
                //json2.MaxJsonLength = int.MaxValue;
                //return json2;
                ////return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetFuncStruct(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList();
                IEnumerable<FuncStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FuncStruct.Include(e => e.Company).Include(e => e.Job).Include(e => e.JobPosition).ToList().Where(d => d.FullDetails.Contains(data)).ToList();
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

        public ActionResult Create(OrgTimingPolicyBatchAssignment COBJ, FormCollection form)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                string geo_id = form["geo_id"] == "0" ? "" : form["geo_id"];
                string fun_id = form["fun_id"] == "0" ? "" : form["fun_id"];
                string pay_id = form["pay_id"] == "0" ? "" : form["pay_id"];

                if (geo_id == "" && fun_id == "" && pay_id == "")
                {
                    Msg.Add("Kindly Apply Advance Filter..!!! ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                var GeostructList = form["GeostructList"] == "0" ? "" : form["GeostructList"];
                var FuncStructList = form["FuncStructList"] == "0" ? "" : form["FuncStructList"];
                if (GeostructList == null || GeostructList == "")
                {
                    Msg.Add("Please Select Geostruct");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                //if (FuncStructList != null)
                //{
                //    var id = Convert.ToInt32(FuncStructList);
                //    var data = db.FuncStruct.Find(id);
                //    COBJ.FuncStruct = data;
                //}
                var TimingPolicyBatchAssignment = form["TimingPolicyBatchAssignmentlist"] == "0" ? "" : form["TimingPolicyBatchAssignmentlist"];
                if (TimingPolicyBatchAssignment != null)
                {
                    var ids = Utility.StringIdsToListIds(TimingPolicyBatchAssignment);
                    List<TimingPolicyBatchAssignment> List_TimingPolicyBatchAssignment = new List<TimingPolicyBatchAssignment>();
                    foreach (var item in ids)
                    {
                        List_TimingPolicyBatchAssignment.Add(db.TimingPolicyBatchAssignment.Find(item));
                    }
                    COBJ.TimingPolicyBatchAssignment = List_TimingPolicyBatchAssignment;
                }
                // List<string> Msg = new List<string>();
                try
                {

                    if (ModelState.IsValid)
                    {
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 60, 0)))
                        //{
                        COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        List<OrgTimingPolicyBatchAssignment> OList = new List<OrgTimingPolicyBatchAssignment>();

                        if (GeostructList != null)
                        {
                            var Geoids = Utility.StringIdsToListIds(GeostructList);
                            foreach (var item in Geoids)
                            {
                                using (TransactionScope ts2 = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 30, 0)))
                                {   
                                    if (FuncStructList != null)
                                    {
                                        var funids = Utility.StringIdsToListIds(FuncStructList);
                                        foreach (var item1 in funids)
                                        {
                                            var idf = Convert.ToInt32(item1);
                                            //var dataf = db.FuncStruct.Find(idf);
                                            //COBJ.FuncStruct = dataf;
                                            COBJ.FuncStruct_Id = idf;
                                            // geo
                                            var id = Convert.ToInt32(item);
                                            //var data = db.GeoStruct.Find(id);
                                            //COBJ.Geostruct = data;
                                            COBJ.Geostruct_Id = id;

                                            OrgTimingPolicyBatchAssignment emprep = new OrgTimingPolicyBatchAssignment()
                                            {
                                                FuncStruct_Id = COBJ.FuncStruct_Id,
                                                Geostruct_Id = COBJ.Geostruct_Id,
                                                TimingPolicyBatchAssignment = COBJ.TimingPolicyBatchAssignment,
                                                DBTrack = COBJ.DBTrack,
                                            };
                                            db.OrgTimingPolicyBatchAssignment.Add(emprep);
                                            db.SaveChanges();
                                            OList.Add(emprep);

                                        }
                                    }
                                    else
                                    {
                                        var id = Convert.ToInt32(item);
                                        //var data = db.GeoStruct.Find(id);
                                        //COBJ.Geostruct = data;
                                          COBJ.Geostruct_Id = id;
                                        OrgTimingPolicyBatchAssignment emprep = new OrgTimingPolicyBatchAssignment()
                                        {
                                            FuncStruct_Id = COBJ.FuncStruct_Id,
                                            Geostruct_Id = COBJ.Geostruct_Id,
                                            TimingPolicyBatchAssignment = COBJ.TimingPolicyBatchAssignment,
                                            DBTrack = COBJ.DBTrack,
                                        };
                                        db.OrgTimingPolicyBatchAssignment.Add(emprep);
                                        db.SaveChanges();
                                        OList.Add(emprep);

                                    }
                                    ts2.Complete();
                                }
                            }
                        }
                        try
                        {
                            var id = Convert.ToInt32(SessionManager.CompanyId);
                            var CompanyAttendance = db.CompanyAttendance.Include(e => e.OrgTimingPolicyBatchAssignment).Where(e => e.Company.Id == id).SingleOrDefault();
                            if (CompanyAttendance.OrgTimingPolicyBatchAssignment.Count() > 0)
                            {
                                OList.AddRange(CompanyAttendance.OrgTimingPolicyBatchAssignment);
                            }
                            CompanyAttendance.OrgTimingPolicyBatchAssignment = OList;

                            db.CompanyAttendance.Attach(CompanyAttendance);
                            db.Entry(CompanyAttendance).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(CompanyAttendance).State = System.Data.Entity.EntityState.Detached;
                            db.SaveChanges();

                            //ts.Complete();
                            Msg.Add("  Data Created successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = 0, Val = "", success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new Object[] { ReportingStruct.Id, ReportingStruct.FullDetails, "Data Created Successfully.", JsonRequestBehavior.AllowGet });
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = COBJ.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                        }
                        //}
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        // return this.Json(new { msg = errorMsg });
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
        }

        public class GeostructC
        {
            public Array GeoGraphList_Id { get; set; }
            public Array GeoGraphList_val { get; set; }
        }
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {



                List<GeostructC> return_data = new List<GeostructC>();

                var Q = db.OrgTimingPolicyBatchAssignment.Include(e => e.Geostruct)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        Geostruct = e.Geostruct,
                        FuncStruct = e.FuncStruct,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data1 = db.OrgTimingPolicyBatchAssignment.Include(e => e.Geostruct)
                //   .Where(e => e.Id == data).SingleOrDefault();

                var add_data = db.OrgTimingPolicyBatchAssignment.Include(e => e.Geostruct)
                                        .Where(e => e.Id == data).ToList();

                //foreach (var ca in add_data)
                //{
                //    return_data.Add(
                //    new GeostructC
                //    {
                //        GeoGraphList_Id = ca.Geostruct.Count > 0 ? ca.Geostruct.Select(e => e.Id.ToString()).ToArray() : null,
                //        GeoGraphList_val = ca.Geostruct.Count > 0 ? ca.Geostruct.Select(e => e.FullDetails.ToString()).ToArray() : null,
                //    });
                //}

                var Corp = db.OrgTimingPolicyBatchAssignment.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public async Task<ActionResult> EditSave(OrgTimingPolicyBatchAssignment L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    var Rpttimstr = form["Geostructlist"] == "0" ? "" : form["Geostructlist"];
                    var blog1 = db.OrgTimingPolicyBatchAssignment.Include(e => e.Geostruct).Where(e => e.Id == data).SingleOrDefault();

                    blog1.Geostruct = null;
                    blog1.Geostruct = L.Geostruct;
                    blog1.FuncStruct = L.FuncStruct;

                    OrgTimingPolicyBatchAssignment pd = null;

                    pd = db.OrgTimingPolicyBatchAssignment.Include(q => q.Geostruct)
                        //.Include(e => e.Geostruct.Select(q => q.GeoGraphList))
                        //          .Include(e => e.Geostruct.Select(q => q.TimingPolicy))
                                    .Where(e => e.Id == data).SingleOrDefault();



                    if (Rpttimstr != null && Rpttimstr != "")
                    {
                        var ids = Utility.StringIdsToListIds(Rpttimstr);
                        foreach (var ca in ids)
                        {
                            var value = db.GeoStruct.Find(ca);
                            //    ObjITsection.Add(value);
                            pd.Geostruct = value;
                        }
                    }
                    else
                    {
                        pd.Geostruct = null;
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            //  using (DataBaseContext db = new DataBaseContext())
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // PostDetails blog = null; // to retrieve old data
                                    // DbPropertyValues originalBlogValues = null;
                                    using (var context = new DataBaseContext())
                                        blog1.DBTrack = new DBTrack
                                        {
                                            CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                            CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = SessionManager.UserName,
                                            ModifiedOn = DateTime.Now
                                        };

                                    var CurCorp = db.OrgTimingPolicyBatchAssignment.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        OrgTimingPolicyBatchAssignment post = new OrgTimingPolicyBatchAssignment()
                                        {
                                            Geostruct = blog1.Geostruct,
                                            FuncStruct = blog1.FuncStruct,
                                            TimingPolicyBatchAssignment = blog1.TimingPolicyBatchAssignment,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.OrgTimingPolicyBatchAssignment.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.Geostruct.FullDetailsLD, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (OrgTimingPolicyBatchAssignment)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (OrgTimingPolicyBatchAssignment)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

                            }
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });

                    }
                    return Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
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
        public class returndatagridclass
        {
            public string Id { get; set; }

            public string FuncStruct { get; set; }

            public string Geostruct { get; set; }
        }
        public class EmpRepoChildDataClass
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

            public string TimingPolicyBatchAssignment { get; set; }
        }

        public ActionResult GridDelete(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Cid = Convert.ToInt32(data.Split(',')[0]);
                int Pid = Convert.ToInt32(data.Split(',')[1]);
                var OTimingPolicy = db.TimingPolicyBatchAssignment.Where(e => e.Id == Cid).FirstOrDefault();
                var OORgTiming = db.OrgTimingPolicyBatchAssignment.Where(e => e.Id == Pid).FirstOrDefault();

                OORgTiming.TimingPolicyBatchAssignment = null;
                db.OrgTimingPolicyBatchAssignment.Attach(OORgTiming);
                db.Entry(OORgTiming).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                db.OrgTimingPolicyBatchAssignment.Remove(OORgTiming);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult OrgTimingPolicyBatchAssignment_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var id = Convert.ToInt32(SessionManager.CompanyId);
                    //var all = db.CompanyAttendance.Include(e => e.OrgTimingPolicyBatchAssignment)
                    //    .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.Geostruct))
                    //    .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.Geostruct.Location))
                    //    .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.Geostruct.Location.LocationObj))
                    //    .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.Geostruct.Department))
                    //    .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.Geostruct.Department.DepartmentObj))
                    //    .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.FuncStruct))
                    //     .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.FuncStruct.Job))
                    //      .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.FuncStruct.Job.JobPosition))
                    //       .Include(e => e.OrgTimingPolicyBatchAssignment.Select(a => a.TimingPolicyBatchAssignment)).Where(e => e.Company.Id == id)
                    //    .SingleOrDefault();
                    // for searchs
                    //IEnumerable<OrgTimingPolicyBatchAssignment> fall;
                    //fall = all.OrgTimingPolicyBatchAssignment;

                    var all = db.OrgTimingPolicyBatchAssignment.Select(r => new
                    {   Id=r.Id,
                        OGeostruct = db.GeoStruct.Where(t => t.Id == r.Geostruct_Id).Select(d => new
                        {
                            FullDetailsLD = d.FullDetailsLD
                        }).FirstOrDefault(),
                        OFuncStruct = db.FuncStruct.Where(e => e.Id == r.FuncStruct_Id).Select(d=> new {
                            FullDetails = d.FullDetails
                        }).FirstOrDefault()                                              
                    }).ToList();
                  
                    if (param.sSearch == null)
                    {
                        all = all;

                    }
                    else
                    {
                        //fall = all.OrgTimingPolicyBatchAssignment.Where(e => e.Id.ToString() == param.sSearch).ToList();

                        all = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.OGeostruct != null ? e.OGeostruct.FullDetailsLD.ToString().ToUpper().Contains(param.sSearch.ToUpper()) : false)
                                  || (e.OFuncStruct != null ? e.OFuncStruct.FullDetails.ToString().ToUpper().Contains(param.sSearch.ToUpper()) : false)
                                  ).ToList();
                    }

                    //for column sorting
                    //var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    //Func<OrgTimingPolicyBatchAssignment, string> orderfunc = (c =>
                    //                                            Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                    //                                            sortindex == 1 ? c.Geostruct.Id.ToString() : "");
                    //var sortcolumn = Request["sSortDir_0"];
                    //if (sortcolumn == "asc")
                    //{
                    //    fall = all.OrderBy(orderfunc);
                    //}
                    //else
                    //{
                    //    fall = all.OrderByDescending(orderfunc);
                    //}
                    // Paging 
                    //var dcompanies = fall
                    //        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    //if (dcompanies.Count == 0)
                    //{
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in all)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Geostruct = item.OGeostruct != null ? item.OGeostruct.FullDetailsLD : "",
                                FuncStruct = item.OFuncStruct != null ? item.OFuncStruct.FullDetails : ""
                            });
                        }

                        var jsonResult = Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = all.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;
                        return jsonResult;
                    //}
                    //else
                    //{
                    //    var result = from c in dcompanies
                    //                 select new[] { null, Convert.ToString(c.Id), c.Geostruct.Location.LocationObj.LocDesc, c.FuncStruct.FullDetails };

                    //    return Json(new
                    //    {
                    //        sEcho = param.sEcho,
                    //        iTotalRecords = all.OrgTimingPolicyBatchAssignment.Count(),
                    //        iTotalDisplayRecords = fall.Count(),
                    //        data = result
                    //    }, JsonRequestBehavior.AllowGet);
                    //}//for data reterivation
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
                    var db_data = db.OrgTimingPolicyBatchAssignment
                        .Include(e => e.TimingPolicyBatchAssignment.Select(a => a.TimingweeklySchedule))
                        .Include(e => e.TimingPolicyBatchAssignment.Select(a => a.TimingGroup))
                       .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpRepoChildDataClass> returndata = new List<EmpRepoChildDataClass>();
                        foreach (var item in db_data.TimingPolicyBatchAssignment.ToList())
                        {
                            returndata.Add(new EmpRepoChildDataClass
                            {
                                Id = item.Id,
                                TimingPolicyBatchAssignment = item.FullDetails,
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                List<string> Msg = new List<string>();
                try
                {
                    OrgTimingPolicyBatchAssignment emprepo = db.OrgTimingPolicyBatchAssignment.Include(e => e.Geostruct).Where(e => e.Id == data).SingleOrDefault();

                    if (emprepo.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = emprepo.DBTrack.CreatedBy != null ? emprepo.DBTrack.CreatedBy : null,
                                CreatedOn = emprepo.DBTrack.CreatedOn != null ? emprepo.DBTrack.CreatedOn : null,
                                IsModified = emprepo.DBTrack.IsModified == true ? true : false
                            };
                            emprepo.DBTrack = dbT;
                            db.Entry(emprepo).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, emprepo.DBTrack);
                            // DT_OrgTimingPolicyBatchAssignment DT_Post = (DT_PostDetails)rtn_Obj;
                            //DT_Post.ExpFilter_Id = Postdetails.ExpFilter == null ? 0 : Postdetails.ExpFilter.Id;
                            //DT_Post.RangeFilter_Id = Postdetails.RangeFilter == null ? 0 : Postdetails.RangeFilter.Id;
                            //DT_Post.Gender_Id = Postdetails.Gender == null ? 0 : Postdetails.Gender.Id;                                      // the declaratn for lookup is remain 
                            //DT_Post.MaritalStatus_Id = Postdetails.MaritalStatus == null ? 0 : Postdetails.MaritalStatus.Id;
                            //DT_Post.FuncStruct_Id = Postdetails.FuncStruct == null ? 0 : Postdetails.FuncStruct.Id;
                            //db.Create(DT_Post);

                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var selectedJobP = emprepo.Geostruct;
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            if (selectedJobP == null)
                            {

                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = emprepo.DBTrack.CreatedBy != null ? emprepo.DBTrack.CreatedBy : null,
                                    CreatedOn = emprepo.DBTrack.CreatedOn != null ? emprepo.DBTrack.CreatedOn : null,
                                    IsModified = emprepo.DBTrack.IsModified == true ? false : false//,
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(emprepo).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                // var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //DT_PostDetails DT_Post = (DT_PostDetails)rtn_Obj;
                                //DT_Post.ExpFilter_Id = exp == null ? 0 : exp.Id;
                                //DT_Post.RangeFilter_Id = ag == null ? 0 : ag.Id;                                                             // the declaratn for lookup is remain 
                                //DT_Post.Gender_Id = gen == null ? 0 : gen.Id;
                                //DT_Post.MaritalStatus_Id = marsts == null ? 0 : marsts.Id;
                                //DT_Post.FuncStruct_Id = fun == null ? 0 : 0;
                                //db.Create(DT_Post);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                Msg.Add("  Data removed.  ");                                                                                             // the original place 
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (selectedJobP != null)
                            {
                                Msg.Add(" Child record exists.Cannot remove it..  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        }
    }
}