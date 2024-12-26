using P2b.Global;
using System;
using System.Collections.Generic;
using P2BUltimate.App_Start;
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
using Attendance;
namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class EmpReportingTimingStructController : Controller
    {
        public ActionResult Index()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EmpId = db.Employee.Select(e => e.Id).ToList();
                List<Int32> Find_Emp_Witch_Employee_Attendance_null = new List<int>();
                foreach (var item in EmpId)
                {
                    var oEmployeeAttendance = db.EmployeeAttendance
                          .Where(e => e.Employee != null && e.Employee.Id == item).SingleOrDefault();
                    if (oEmployeeAttendance == null)
                    {
                        Find_Emp_Witch_Employee_Attendance_null.Add(item);
                    }
                }
                List<EmployeeAttendance> _List_EmployeeAttendance = new List<EmployeeAttendance>();
                foreach (var Id in Find_Emp_Witch_Employee_Attendance_null)
                {
                    _List_EmployeeAttendance.Add(new EmployeeAttendance
                    {
                        Employee = db.Employee.Where(e => e.Id == Id).SingleOrDefault(),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                    });
                }
                db.EmployeeAttendance.AddRange(_List_EmployeeAttendance);
                db.SaveChanges();
            }
            return View("~/Views/Attendance/MainViews/EmpReportingTimingStruct/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Attendance/_EmpReportingTimingStructGridPartial.cshtml");
        }
        List<string> Msg = new List<string>();
        public ActionResult GetReportingTimingStructLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ReportingTimingStruct.ToList();
                IEnumerable<ReportingTimingStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ReportingTimingStruct.ToList().Where(d => d.RSName.Contains(data));

                }
                else
                {
                    //var data1 = db.QualificationDetails
                    //.Select(e => new
                    //{
                    //    value = "University :" + e.Qualification + ",Institute : " + e.University
                    //}).ToString();  
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                ////string univer = db.QualificationDetails.Include(a => a.University).ToString();
                ////string inst =  db.QualificationDetails.Include(a=>a.Institute).ToString();  
                //string ca121 = "University :"+fall.Select(a=>a.Institute) +",Institute :"+fall.Select(a=>a.University) ;  

                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(EmpReportingTimingStruct COBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Rpttimstr = form["ReportingTimingStructlist"] == "0" ? "" : form["ReportingTimingStructlist"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    List<int> idse = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        idse = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Rpttimstr == null)
                    {
                         Msg.Add(" Kindly select Reporting Timing Struct ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    COBJ.ReportingTimingStruct = null;
                    //if (COBJ.EffectiveDate > COBJ.EndDate)
                    //{
                    //    Msg.Add(" End Date should be greater than Effective Date.  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                   
                    if (Rpttimstr != null)
                    {
                        int ids = Convert.ToInt32(Rpttimstr);
                          var data = db.ReportingTimingStruct.Find(ids);

                          COBJ.ReportingTimingStruct = data;
                    }
                    foreach (var id in idse)
                    {
                        if (ModelState.IsValid)
                        {
                            var _prv_EmployeeAttendance = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct)
                                .Include(e => e.Employee.FuncStruct).Include(e => e.Employee.PayStruct).Include(e => e.Employee.GeoStruct)
                                .Where(e => e.Employee.Id == id).SingleOrDefault();
                            var oEmpReportingTimingStruct = _prv_EmployeeAttendance.EmpReportingTimingStruct.Where(e=>e.EndDate==null).ToList();
                            if (oEmpReportingTimingStruct.Any(a => a.EffectiveDate >= COBJ.EffectiveDate))
                            {
                                Msg.Add("Invalid Effective Date..!");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                COBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                //check for previous
                                // var _prv_EmployeeAttendance = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct).Where(e => e.Employee.Id == id).SingleOrDefault();
                                foreach (EmpReportingTimingStruct EmpReportingTimingStruct in oEmpReportingTimingStruct)
                                {
                                    EmpReportingTimingStruct.EndDate = COBJ.EffectiveDate.Value.AddDays(-1);
                                }
                                db.Entry(_prv_EmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();

                                if (Rpttimstr != null)
                                {
                                    Boolean empbatch = false;
                                    int ids = Convert.ToInt32(Rpttimstr);
                                    var data = db.ReportingTimingStruct.Find(ids);
                                    if (data.GeographicalAppl == true)
                                    {
                                        //if same location,dept two or more batch create then system will take last batch when create structure
                                        //for latest batch create structure(if employee has same location)
                                        var TimingPolicyBatch = db.TimingPolicyBatchAssignment
                                            .Include(e => e.OrgTimingPolicyBatchAssignment)
                                             .Include(e => e.OrgTimingPolicyBatchAssignment.Select(s => s.FuncStruct))
                                               .Include(e => e.OrgTimingPolicyBatchAssignment.Select(s => s.Geostruct))
                                            .OrderByDescending(e=>e.Id).ToList();
                                        foreach (var batch in TimingPolicyBatch)
                                        {
                                            //New Code
                                            if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.FuncStruct_Id == null))
                                            {
                                                if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                                {
                                                    empbatch = true;
                                                    COBJ.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                                   
                                                    break;
                                                }
                                            }
                                            if (batch.OrgTimingPolicyBatchAssignment.Any(e => e.Geostruct_Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id && e.FuncStruct_Id == _prv_EmployeeAttendance.Employee.FuncStruct_Id))
                                            {
                                                if (batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                                                {
                                                    empbatch = true;
                                                    COBJ.TimingPolicyBatchAssignment = batch.OrgTimingPolicyBatchAssignment.Where(e => e.Geostruct_Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id && e.FuncStruct_Id == _prv_EmployeeAttendance.Employee.FuncStruct_Id).OrderByDescending(e => e.Id).FirstOrDefault().TimingPolicyBatchAssignment.LastOrDefault();
                                                   
                                                    break;
                                                }
                                            }
                                            //New Code

                                            // old code
                                            //var batchAssignment = batch.OrgTimingPolicyBatchAssignment.ToList();
                                            //foreach (var item in batchAssignment)
                                            //{
                                            //    if (item.FuncStruct == null)
                                            //    {
                                            //        if (item.Geostruct.Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id)
                                            //        {

                                            //            empbatch = true;
                                            //            COBJ.TimingPolicyBatchAssignment = batch;
                                            //            break;
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        if (item.Geostruct.Id == _prv_EmployeeAttendance.Employee.GeoStruct_Id && item.FuncStruct.Id == _prv_EmployeeAttendance.Employee.FuncStruct_Id)
                                            //        {
                                            //            empbatch = true;
                                            //            COBJ.TimingPolicyBatchAssignment = batch;
                                            //            break;
                                            //        }
                                            //    }
                                            //}
                                            // old code

                                            if (empbatch == true)
                                            {
                                                break;
                                            }
                                        }
                                        // var GeoGraphListVal = data.GeoGraphList.LookupVal == null ? "" : data.GeoGraphList.LookupVal;
                                    }

                                }

                                EmpReportingTimingStruct emprep = new EmpReportingTimingStruct()
                                {
                                    TimingPolicyBatchAssignment=COBJ.TimingPolicyBatchAssignment,
                                    EffectiveDate = COBJ.EffectiveDate,
                                    ReportingTimingStruct = COBJ.ReportingTimingStruct,
                                    DBTrack = COBJ.DBTrack,
                                    FuncStruct = db.FuncStruct.Find(_prv_EmployeeAttendance.Employee.FuncStruct.Id),
                                    GeoStruct = db.GeoStruct.Find(_prv_EmployeeAttendance.Employee.GeoStruct.Id),
                                    PayStruct = db.PayStruct.Find(_prv_EmployeeAttendance.Employee.PayStruct.Id),
                                };


                                try
                                {
                                    db.EmpReportingTimingStruct.Add(emprep);
                                    var EmployeeAttendance = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct).Where(e => e.Employee.Id == id).SingleOrDefault();
                                    EmployeeAttendance.EmpReportingTimingStruct.Add(emprep);
                                    db.Entry(EmployeeAttendance).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();
                                    ts.Complete();

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
                            //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            // return this.Json(new { msg = errorMsg });
                        }
                    }
                    Msg.Add("  Data Created successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                return View();
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
                IEnumerable<EmpReportingTimingStruct> PostDetails = null;
                if (gp.IsAutho == true)
                {
                    PostDetails = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct).AsNoTracking().ToList();
                }
                else
                {
                    PostDetails = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct).AsNoTracking().ToList();
                }

                IEnumerable<EmpReportingTimingStruct> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PostDetails;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                               || (e.EffectiveDate.ToString().Contains(gp.searchString))
                               || (e.EndDate.ToString().Contains(gp.searchString))
                               ).Select(a => new { a.Id, a.EffectiveDate, a.EndDate }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EffectiveDate, a.EndDate }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PostDetails;
                    Func<EmpReportingTimingStruct, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EffectiveDate" ? c.EffectiveDate.ToString() :
                                         gp.sidx == "EndDate" ? c.EndDate.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EffectiveDate == null ? "" : a.EffectiveDate.Value.ToShortDateString(), a.EndDate == null ? "" : a.EndDate.Value.ToShortDateString() }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EffectiveDate == null ? "" : Convert.ToString(a.EffectiveDate), a.EndDate == null ? "" : Convert.ToString(a.EndDate) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EffectiveDate == null ? "" : a.EffectiveDate.Value.ToShortDateString(), a.EndDate == null ? "" : a.EndDate.Value.ToShortDateString() }).ToList();
                    }
                    totalRecords = PostDetails.Count();
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
            catch (Exception e)
            {
                LogFile Logfile = new LogFile();
                ErrorLog Err = new ErrorLog()
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    ExceptionMessage = e.Message,
                    ExceptionStackTrace = e.StackTrace,
                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);
                Msg.Add(e.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            return View();
        }
        public class ReportingTimingStructC
        {
            public string GeoGraphList_Id { get; set; }
            public string GeoGraphList_val { get; set; }
        }
        public ActionResult Edit(int data)
        {
            //string tableName = "EmpMedicalInfo";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            using (DataBaseContext db = new DataBaseContext())
            {
                List<ReportingTimingStructC> return_data = new List<ReportingTimingStructC>();

                var Q = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EffectiveDate = e.EffectiveDate,
                        EndDate = e.EndDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                //var add_data1 = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct)
                //   .Where(e => e.Id == data).SingleOrDefault();

                var add_data = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct).Include(e => e.ReportingTimingStruct.GeoGraphList)
                                        .Include(e => e.ReportingTimingStruct.TimingPolicy).Where(e => e.Id == data).ToList();

                foreach (var ca in add_data)
                {
                    return_data.Add(
                    new ReportingTimingStructC
                    {
                        GeoGraphList_Id = ca.ReportingTimingStruct!=null ? ca.ReportingTimingStruct.Id.ToString() : null,
                        GeoGraphList_val = ca.ReportingTimingStruct != null ? ca.ReportingTimingStruct.FullDetails.ToString() : null,
                    });
                }

                var Corp = db.EmpReportingTimingStruct.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(EmpReportingTimingStruct L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    var Rpttimstr = form["ReportingTimingStructlist"] == "0" ? "" : form["ReportingTimingStructlist"];
                    var blog1 = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct).Where(e => e.Id == data).SingleOrDefault();

                    blog1.ReportingTimingStruct = null;
                    blog1.EffectiveDate = L.EffectiveDate;
                    blog1.EndDate = L.EndDate;

                    EmpReportingTimingStruct pd = null;

                    pd = db.EmpReportingTimingStruct.Include(q => q.ReportingTimingStruct).Include(e => e.ReportingTimingStruct.GeoGraphList)
                                    .Include(e => e.ReportingTimingStruct.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();

                  //  List<ReportingTimingStruct> ObjITsection = new List<ReportingTimingStruct>();

                    if (Rpttimstr != null && Rpttimstr != "")
                    {
                        int ids = Convert.ToInt32(Rpttimstr);
                        //foreach (var ca in ids)
                        //{
                        var value = db.ReportingTimingStruct.Find(ids);
                         //   ObjITsection.Add(value);
                        pd.ReportingTimingStruct = value;
                       // }
                    }
                    else
                    {
                        pd.ReportingTimingStruct = null;
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

                                    var CurCorp = db.EmpReportingTimingStruct.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        EmpReportingTimingStruct post = new EmpReportingTimingStruct()
                                        {
                                            EffectiveDate = blog1.EffectiveDate,
                                            // EndDate = blog1.EndDate,
                                            ReportingTimingStruct = blog1.ReportingTimingStruct,
                                            Id = data,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.EmpReportingTimingStruct.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("  Record Updated");
                                        return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (EmpReportingTimingStruct)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (EmpReportingTimingStruct)databaseEntry.ToObject();
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
            public string Applicable { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeAttendance.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                        .Where(e => e.Employee != null && e.EmpReportingTimingStruct.Count > 0)
                        .ToList();
                    // for search
                    IEnumerable<EmployeeAttendance> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.Employee.EmpCode.ToUpper().Contains(param.sSearch.ToUpper()))
                                              || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                                              ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeAttendance, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
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
                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
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
                    var db_data = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct).Include(e => e.EmpReportingTimingStruct.Select(q => q.ReportingTimingStruct))
                       .Where(e => e.Employee.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpRepoChildDataClass> returndata = new List<EmpRepoChildDataClass>();
                        foreach (var item in db_data.EmpReportingTimingStruct.ToList())
                        {                          
                            if (item.ReportingTimingStruct!=null)
                            {                           
                                returndata.Add(new EmpRepoChildDataClass
                                {
                                    Id = item.Id,
                                    EffectiveDate = item.EffectiveDate != null ? item.EffectiveDate.Value.ToString("dd/MM/yyyy") : "",
                                    EndDate = item.EndDate != null ? item.EndDate.Value.ToString("dd/MM/yyyy") : "",
                                    ReportingTimingStruct = item.ReportingTimingStruct.FullDetails,
                                    Applicable = (item.ReportingTimingStruct.GeographicalAppl == true || item.ReportingTimingStruct.IsTimeRoaster == true || item.ReportingTimingStruct.IndividualAppl == true) ? (item.ReportingTimingStruct.GeographicalAppl == true ? "Location" : "") + (item.ReportingTimingStruct.IsTimeRoaster == true ? "Time Roaster" : "") + (item.ReportingTimingStruct.IndividualAppl == true ? "Individual" : "") : " "                                                                  
                                });
                            }                         
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
        //changes needed --- create box in partial for ReportingTimingStruct
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var P = db.EmployeeAttendance.Include(e => e.EmpReportingTimingStruct)
                    .Include(e => e.EmpReportingTimingStruct.Select(a => a.ReportingTimingStruct))
                       .Where(e => e.Id == data).SingleOrDefault();
                var Q = P.EmpReportingTimingStruct.Select
                 (e => new
                 {
                     EffectiveDate = e.EffectiveDate != null ? e.EffectiveDate.Value.ToString("dd/MM/yyyy") : "",
                     EndDate = e.EndDate != null ? e.EndDate.Value.ToString("dd/MM/yyyy") : "",
                     ReportingTimingStruct = e.ReportingTimingStruct != null ? e.ReportingTimingStruct.FullDetails.ToString() : "",
                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    EmpReportingTimingStruct emprepo = db.EmpReportingTimingStruct.Include(e => e.ReportingTimingStruct).Where(e => e.Id == data).SingleOrDefault();

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
                            // DT_EmpReportingTimingStruct DT_Post = (DT_PostDetails)rtn_Obj;
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
                        var selectedJobP = emprepo.ReportingTimingStruct;
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