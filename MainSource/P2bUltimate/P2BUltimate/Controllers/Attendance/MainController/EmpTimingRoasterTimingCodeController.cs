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
using System.Web.Script.Serialization;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;
using P2BUltimate.Process;
using Attendance;
namespace P2BUltimate.Controllers.Attendance.MainController
{
    [AuthoriseManger]
    public class EmpTimingRoasterTimingCodeController : Controller
    {
        // GET: EmpTimingRoasterData
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/EmpTimingRoasterTimingCode/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Attendance/MainViews/EmpTimingRoasterData/Index.cshtml");
        }
        public class remark_return_class
        {
            public string _dater { get; set; }
            public string _Remark { get; set; }
            public string _Pcode { get; set; }
        }
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult EmpTimingRoasterDatesprv(string employeeids, string paymonth)
        {
            var firstDate = Convert.ToDateTime("01/" + paymonth);
            var lastDate = new DateTime(firstDate.Year, firstDate.Month, DateTime.DaysInMonth(firstDate.Year, firstDate.Month));
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(employeeids);
                var employeeData = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location).Where(e => e.Employee.Id == id).SingleOrDefault();

                var _fetch_emp_policy = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                          .Include(a => a.Employee.GeoStruct)
                          .Include(a => a.Employee.GeoStruct.Location)
                          .Include(a => a.Employee.FuncStruct)
                          .Include(a => a.EmpTimingRoasterData)
                          .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                          .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.GeoGraphList))
                          .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.TimingPolicy))
                          //.Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.Select(w => w.GeoGraphList)))
                         // .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.Select(w => w.TimingPolicy)))
                          .Where(a => a.Employee.Id == id && a.EmpReportingTimingStruct.Count > 0)
                          .AsParallel().SingleOrDefault();
                List<EmpTimingRoasterData> _List_EmpTimingRoasterData = new List<EmpTimingRoasterData>();
                remark_return_class ORetClass = new remark_return_class();
                List<remark_return_class> ORetClassList = new List<remark_return_class>();
                if (_fetch_emp_policy != null)
                {
                    var _EmpReportingTimingStruct = _fetch_emp_policy.EmpReportingTimingStruct.Where(a => a.EndDate == null).FirstOrDefault();
                    var _emp_reporting_struct = _EmpReportingTimingStruct.ReportingTimingStruct;
                    if (_emp_reporting_struct.GeographicalAppl)
                    {
                        int compid = Convert.ToInt32(SessionManager.CompanyId);
                        var _Comp_Attendance = db.CompanyAttendance
                  .Include(a => a.OrgTimingPolicyBatchAssignment)
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.Geostruct))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.FuncStruct))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule)))
                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(y => y.WeekDays))))
                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(c => c.TimingGroup))))
                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(b => b.TimingGroup.TimingPolicy))))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup)))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup.TimingPolicy)))
                  .Where(a => a.Company.Id == compid)
                 .AsParallel().SingleOrDefault();

                        OrgTimingPolicyBatchAssignment _Comp_Attendance_policy = _Comp_Attendance.OrgTimingPolicyBatchAssignment
                            .Where(a => (a.Geostruct != null && a.FuncStruct != null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id && a.FuncStruct.Id == _fetch_emp_policy.Employee.FuncStruct.Id) || (a.Geostruct != null && a.FuncStruct == null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id)).LastOrDefault();
                        if (_Comp_Attendance_policy != null)
                        {
                            TimingPolicyBatchAssignment oTimingPolicyBatchAssignment = _Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault();

                            if (oTimingPolicyBatchAssignment.IsRoaster)
                            {

                                var employeeDataprv = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster)
                                    .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                                    .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingPolicy))
                                    .Where(e => e.Employee.Id == id && e.EmpTimingMonthlyRoaster.Any(x => DbFunctions.TruncateTime(x.RoasterDate) >= firstDate &&
                                                                                                                    DbFunctions.TruncateTime(x.RoasterDate) <= lastDate)).SingleOrDefault();

                                // var dates = new Dictionary<string, string>();
                                //var dates = new List<>();

                                if (employeeDataprv != null)
                                {

                                    var atcode = "";
                                    for (DateTime i = firstDate; i <= lastDate; i = i.AddDays(1))
                                    {
                                        var removeR = employeeDataprv.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date == i).Select(x => x.TimingPolicy).FirstOrDefault();
                                        if (removeR != null)
                                        {
                                            var attcode = removeR.TimingCode.ToString();
                                            atcode = attcode;
                                        }

                                        var remark = AttendanceProcess.Check_HWWO(Convert.ToInt32(SessionManager.CompanyId), employeeData, i);
                                        ORetClass = new remark_return_class()
                                        {
                                            _dater = i.ToShortDateString(),
                                            _Pcode = atcode,
                                            _Remark = remark
                                        };
                                        ORetClassList.Add(ORetClass);
                                    }
                                }

                            }

                        }
                    }
                }

                return Json(new { ORetClassList }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult EmpTimingRoasterDates(string employeeids, string paymonth)
        {
            var firstDate = Convert.ToDateTime("01/" + paymonth);
            var lastDate = new DateTime(firstDate.Year, firstDate.Month, DateTime.DaysInMonth(firstDate.Year, firstDate.Month));
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(employeeids);
                var employeeData = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location).Where(e => e.Id == id).SingleOrDefault();
                var dates = new Dictionary<string, string>();
                for (DateTime i = firstDate; i <= lastDate; i = i.AddDays(1))
                {
                    var remark = AttendanceProcess.Check_HWWO(Convert.ToInt32(SessionManager.CompanyId), employeeData, i);
                    dates.Add(i.ToShortDateString(), remark);
                }
                return Json(new { dates }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetLVReqLKDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);

                var _fetch_emp_policy = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                           .Include(a => a.Employee.GeoStruct)
                           .Include(a => a.Employee.GeoStruct.Location)
                           .Include(a => a.Employee.FuncStruct)
                           .Include(a => a.EmpTimingRoasterData)
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.GeoGraphList))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.TimingPolicy))
                           .Where(a => a.Employee.Id == Id && a.EmpReportingTimingStruct.Count > 0)
                           .AsParallel().SingleOrDefault();
                List<EmpTimingRoasterData> _List_EmpTimingRoasterData = new List<EmpTimingRoasterData>();

                if (_fetch_emp_policy != null)
                {
                    var _EmpReportingTimingStruct = _fetch_emp_policy.EmpReportingTimingStruct.Where(a => a.EndDate == null).FirstOrDefault();
                    var _emp_reporting_struct = _EmpReportingTimingStruct.ReportingTimingStruct;
                    if (_emp_reporting_struct.GeographicalAppl)
                    {
                        int compid = Convert.ToInt32(SessionManager.CompanyId);
                        var _Comp_Attendance = db.CompanyAttendance
                  .Include(a => a.OrgTimingPolicyBatchAssignment)
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.Geostruct))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.FuncStruct))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule)))
                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(y => y.WeekDays))))
                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(c => c.TimingGroup))))
                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(b => b.TimingGroup.TimingPolicy))))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup)))
                  .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup.TimingPolicy)))
                  .Where(a => a.Company.Id == compid)
                 .AsParallel().SingleOrDefault();

                        OrgTimingPolicyBatchAssignment _Comp_Attendance_policy = _Comp_Attendance.OrgTimingPolicyBatchAssignment
                            .Where(a => (a.Geostruct != null && a.FuncStruct != null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id && a.FuncStruct.Id == _fetch_emp_policy.Employee.FuncStruct.Id) || (a.Geostruct != null && a.FuncStruct == null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id)).LastOrDefault();
                        if (_Comp_Attendance_policy != null)
                        {
                            TimingPolicyBatchAssignment oTimingPolicyBatchAssignment = _Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault();

                            if (oTimingPolicyBatchAssignment.IsRoaster)
                            {
                                var list1 = db.TimingGroup.Where(e => e.GroupCode == oTimingPolicyBatchAssignment.TimingGroup.GroupCode).ToList();
                                var result = new SelectList(list1, "GroupCode", "GroupCode");
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }

                        }
                    }
                }


            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTimingGroup(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var list1 = db.TimingGroup.ToList();
                var result = new SelectList(list1, "GroupCode", "GroupCode");
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }
        //GetTimingPolicy
        public ActionResult GetTimingPolicy(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != "")
                {
                    var fall = db.TimingGroup.Include(e => e.TimingPolicy).Where(e => e.GroupCode == data).SingleOrDefault();
                    var result = new SelectList(fall.TimingPolicy.ToList(), "TimingCode", "TimingCode");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var fall = db.TimingGroup.Include(e => e.TimingPolicy).ToList();
                    var result = new SelectList(fall.SelectMany(a => a.TimingPolicy).ToList(), "TimingCode", "TimingCode");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

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
                IEnumerable<EmpTimingRoasterData> timingMonthlyRoaster = null;
                if (gp.IsAutho == true)
                {
                    timingMonthlyRoaster = db.EmpTimingRoasterData
                        //.Include(e => e.TimingMonthlyRoaster)
                        .Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    timingMonthlyRoaster = db.EmpTimingRoasterData
                        //.Include(e => e.TimingMonthlyRoaster)
                        .AsNoTracking().ToList();
                }

                IEnumerable<EmpTimingRoasterData> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = timingMonthlyRoaster;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.RoasterDate }).Where((e => (e.Id.ToString() == gp.searchString) || (e.RoasterDate.ToString() == gp.searchString.ToString()))).ToList();

                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RoasterDate }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = timingMonthlyRoaster;
                    Func<EmpTimingRoasterData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "RoasterDate" ? c.RoasterDate.Value.ToShortDateString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.RoasterDate.Value.ToShortDateString() }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.RoasterDate.Value.ToShortDateString() }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.RoasterDate.Value.ToShortDateString() }).ToList();
                    }
                    totalRecords = timingMonthlyRoaster.Count();
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
        [HttpPost]
        public ActionResult Create(EmpTimingRoasterData c, FormCollection form) //Create submit
        {

            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {


                    string Category = form["DayTypelist"] == "0" ? "" : form["DayTypelist"];
                    string tpol = form["TimingPolicylist1"] == "0" ? "" : form["TimingPolicylist1"];
                    string tgop = form["TimingGrouplist1"] == "0" ? "" : form["TimingGrouplist1"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.EmpTimingRoasterData.Any(q => q.RoasterDate == c.RoasterDate))
                    {
                        Msg.Add("  Code Already Exists.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (Category != null && Category != "")
                    { 
                            var val = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1001").FirstOrDefault().LookupValues.Where(e => e.Id == int.Parse(Category)).FirstOrDefault(); 
                            c.DayType = val; 
                    }
                    TimingGroup oTimingGroup = new TimingGroup();
                    if (tgop != null)
                    {
                        if (tgop != "")
                        {
                            var val = db.TimingGroup.Find(int.Parse(tgop));
                            // c.DayType = val;
                            oTimingGroup = val;
                        }
                    }
                    TimingPolicy oTimingPolicy = new TimingPolicy();
                    if (tpol != null)
                    {
                        if (tpol != "")
                        {
                            var val = db.TimingPolicy.Where(e => e.TimingCode == tpol).SingleOrDefault();
                            // c.DayType = val;
                            oTimingPolicy = val;
                        }
                    }
                    foreach (var id in ids)
                    {
                        if (ModelState.IsValid)
                        {
                            using (TransactionScope ts = new TransactionScope())
                            {
                                c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                TimingMonthlyRoaster timingMonthlyRoaster = new TimingMonthlyRoaster()
                                {
                                    DBTrack = c.DBTrack,
                                    TimingGroup = oTimingGroup,
                                    RoasterName = oTimingGroup.GroupName
                                };
                                db.TimingMonthlyRoaster.Add(timingMonthlyRoaster);
                                db.SaveChanges();
                                //timingMonthlyRoaster
                                EmpTimingMonthlyRoaster oEmpTimingMonthlyRoaster = new EmpTimingMonthlyRoaster
                                {
                                    RoasterDate = c.RoasterDate,
                                    DayType = c.DayType,
                                    //InTime = c.InTime,
                                    TimingPolicy = oTimingPolicy,
                                    DBTrack = c.DBTrack,
                                    TimingMonthlyRoaster = timingMonthlyRoaster

                                };
                                try
                                {
                                    db.EmpTimingMonthlyRoaster.Add(oEmpTimingMonthlyRoaster);
                                    var EmployeeAttendance = db.EmployeeAttendance.Include(e => e.EmpTimingRoasterData).Where(e => e.Employee != null && e.Employee.Id == id).SingleOrDefault();
                                    EmployeeAttendance.EmpTimingMonthlyRoaster.Add(oEmpTimingMonthlyRoaster);
                                    db.Entry(EmployeeAttendance).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();

                                    ts.Complete();
                                    //  return this.Json(new Object[] { timingMonthlyRoaster.Id, timingMonthlyRoaster.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
                            return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                            //return this.Json(new { msg = errorMsg });
                        }
                    }
                    Msg.Add("  Data Saved successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {


                    EmpTimingRoasterData corporates = db.EmpTimingRoasterData
                        //.Include(e => e.TimingMonthlyRoaster)
                        //                             .Include(e => e.TimingPolicy)
                                                       .Include(e => e.DayType).Where(e => e.Id == data).SingleOrDefault();

                    //TimingMonthlyRoaster add = corporates.TimingMonthlyRoaster;
                    //TimingPolicy conDet = corporates.TimingPolicy;
                    LookupValue val = corporates.DayType;
                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_EmpTimingRoasterData DT_Corp = (DT_EmpTimingRoasterData)rtn_Obj;
                            //DT_Corp.TimingMonthlyRoaster_Id = corporates.TimingMonthlyRoaster == null ? 0 : corporates.TimingMonthlyRoaster.Id;
                            //DT_Corp.DayType_Id = corporates.DayType == null ? 0 : corporates.DayType.Id;
                            //DT_Corp.TimingPolicy_Id = corporates.TimingPolicy == null ? 0 : corporates.TimingPolicy.Id;
                            //db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Regions;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Regions.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Attendance/Attendance", null, db.ChangeTracker, dbT);
                                //DT_EmpTimingRoasterData DT_Corp = (DT_EmpTimingRoasterData)rtn_Obj;
                                //DT_Corp.TimingMonthlyRoaster_Id = add == null ? 0 : add.Id;
                                //DT_Corp.DayType_Id = val == null ? 0 : val.Id;
                                //DT_Corp.TimingPolicy_Id = conDet == null ? 0 : conDet.Id;
                                //  db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    corporates.Address = add;
                                //    corporates.ContactDetails = conDet;
                                //    corporates.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                                //}
                                ts.Complete();
                                // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.EmpTimingRoasterData
                    //  .Include(e => e.TimingMonthlyRoaster)
                    //.Include(e => e.TimingPolicy)
                    .Include(e => e.DayType)
                    //    .Include(e => e.EmployeeID)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        RoasterDate = e.RoasterDate,
                        DayType_Id = e.DayType.Id == null ? 0 : e.DayType.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.EmpTimingRoasterData
                    //  .Include(e => e.TimingMonthlyRoaster)
                    //   .Include(e => e.TimingPolicy)
                    .Include(e => e.DayType)
                    //   .Include(e => e.EmployeeID)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        //TimingMonthlyRoaster_FullDetails = e.TimingMonthlyRoaster.Id == null ? "" : e.TimingMonthlyRoaster.FullDetails,
                        //    TimingMonthlyRoaster_Id = e.TimingMonthlyRoaster.Id == null ? "" : e.TimingMonthlyRoaster.Id.ToString(),
                        //    TimingPolicy_Id = e.TimingPolicy.Id == null ? "" : e.TimingPolicy.Id.ToString(),
                        //    TimingPolicy_FullDetails = e.TimingPolicy.FullDetails == null ? "" : e.TimingPolicy.FullDetails
                        //Employee_Id = e.EmployeeID.Id == null ? "" : e.EmployeeID.Id.ToString(),
                        //Employee_FullDetails = e.EmployeeID.EmpName == null ? "" : e.EmployeeID.EmpName.ToString()
                    }).ToList();


                //var W = db.DT_EmpTimingRoasterData
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         RoasterDate = e.RoasterDate,
                //         DayType_Val = e.DayType_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.DayType_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //         TimingMonthlyRoaster_Val = e.TimingMonthlyRoaster_Id == 0 ? "" : db.TimingMonthlyRoaster.Where(x => x.Id == e.TimingMonthlyRoaster_Id).Select(x => x.RoasterName).FirstOrDefault(),
                //         TimingPolicy_Val = e.TimingPolicy_Id == 0 ? "" : db.TimingPolicy.Where(x => x.Id == e.TimingPolicy_Id).Select(x => x.FullDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.EmpTimingRoasterData.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public int EditS(/*string Corp,*/ string Addrs, string ContactDetails, int data, EmpTimingRoasterData c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //if (Corp != null)
                //{
                //    if (Corp != "")
                //    {
                //        var val = db.LookupValue.Find(int.Parse(Corp));
                //        c.DayType = val;

                //        var type = db.EmpTimingRoasterData.Include(e => e.DayType).Where(e => e.Id == data).SingleOrDefault();
                //        IList<EmpTimingRoasterData> typedetails = null;
                //        if (type.DayType != null)
                //        {
                //            typedetails = db.EmpTimingRoasterData.Where(x => x.DayType.Id == type.DayType.Id && x.Id == data).ToList();
                //        }
                //        else
                //        {
                //            typedetails = db.EmpTimingRoasterData.Where(x => x.Id == data).ToList();
                //        }
                //        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                //        foreach (var s in typedetails)
                //        {
                //            s.DayType = c.DayType;
                //            db.EmpTimingRoasterData.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //    else
                //    {
                //        var BusiTypeDetails = db.EmpTimingRoasterData.Include(e => e.DayType).Where(x => x.Id == data).ToList();
                //        foreach (var s in BusiTypeDetails)
                //        {
                //            s.DayType = null;
                //            db.EmpTimingRoasterData.Attach(s);
                //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //            //await db.SaveChangesAsync();
                //            db.SaveChanges();
                //            TempData["RowVersion"] = s.RowVersion;
                //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //        }
                //    }
                //}
                //else
                //{
                //    var BusiTypeDetails = db.EmpTimingRoasterData.Include(e => e.DayType).Where(x => x.Id == data).ToList();
                //    foreach (var s in BusiTypeDetails)
                //    {
                //        s.DayType = null;
                //        db.EmpTimingRoasterData.Attach(s);
                //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                //        //await db.SaveChangesAsync();
                //        db.SaveChanges();
                //        TempData["RowVersion"] = s.RowVersion;
                //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                //    }
                //}

                if (Addrs != null)
                {
                    //if (Addrs != "")
                    //{
                    //    var val = db.TimingMonthlyRoaster.Find(int.Parse(Addrs));
                    //    c.TimingMonthlyRoaster = val;

                    //    var add = db.EmpTimingRoasterData.Include(e => e.TimingMonthlyRoaster).Where(e => e.Id == data).SingleOrDefault();
                    //    IList<EmpTimingRoasterData> addressdetails = null;
                    //    if (add.TimingMonthlyRoaster != null)
                    //    {
                    //        addressdetails = db.EmpTimingRoasterData.Where(x => x.TimingMonthlyRoaster.Id == add.TimingMonthlyRoaster.Id && x.Id == data).ToList();
                    //    }
                    //    else
                    //    {
                    //        addressdetails = db.EmpTimingRoasterData.Where(x => x.Id == data).ToList();
                    //    }
                    //    if (addressdetails != null)
                    //    {
                    //        foreach (var s in addressdetails)
                    //        {
                    //            s.TimingMonthlyRoaster = c.TimingMonthlyRoaster;
                    //            db.EmpTimingRoasterData.Attach(s);
                    //            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //            // await db.SaveChangesAsync(false);
                    //            db.SaveChanges();
                    //            TempData["RowVersion"] = s.RowVersion;
                    //            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //        }
                    //    }
                    //}
                }
                else
                {
                    //var addressdetails = db.EmpTimingRoasterData.Include(e => e.TimingMonthlyRoaster).Where(x => x.Id == data).ToList();
                    //foreach (var s in addressdetails)
                    //{
                    //    s.TimingMonthlyRoaster = null;
                    //    db.EmpTimingRoasterData.Attach(s);
                    //    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //    //await db.SaveChangesAsync();
                    //    db.SaveChanges();
                    //    TempData["RowVersion"] = s.RowVersion;
                    //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //}
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        //var val = db.TimingPolicy.Find(int.Parse(ContactDetails));
                        //c.TimingPolicy = val;

                        //var add = db.EmpTimingRoasterData.Include(e => e.TimingPolicy).Where(e => e.Id == data).SingleOrDefault();
                        //IList<EmpTimingRoasterData> contactsdetails = null;
                        //if (add.TimingPolicy != null)
                        //{
                        //    contactsdetails = db.EmpTimingRoasterData.Where(x => x.TimingPolicy.Id == add.TimingPolicy.Id && x.Id == data).ToList();
                        //}
                        //else
                        //{
                        //    contactsdetails = db.EmpTimingRoasterData.Where(x => x.Id == data).ToList();
                        //}
                        //foreach (var s in contactsdetails)
                        //{
                        //    s.TimingPolicy = c.TimingPolicy;
                        //    db.EmpTimingRoasterData.Attach(s);
                        //    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //    //await db.SaveChangesAsync();
                        //    db.SaveChanges();
                        //    TempData["RowVersion"] = s.RowVersion;
                        //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        //}
                    }
                }
                else
                {
                    //var contactsdetails = db.EmpTimingRoasterData.Include(e => e.TimingPolicy).Where(x => x.Id == data).ToList();
                    //foreach (var s in contactsdetails)
                    //{
                    //    s.TimingPolicy = null;
                    //    db.EmpTimingRoasterData.Attach(s);
                    //    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                    //    //await db.SaveChangesAsync();
                    //    db.SaveChanges();
                    //    TempData["RowVersion"] = s.RowVersion;
                    //    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    //}
                }

                var CurCorp = db.EmpTimingRoasterData.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    EmpTimingRoasterData corp = new EmpTimingRoasterData()
                    {
                        DayType = c.DayType,
                        RoasterDate = c.RoasterDate,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.EmpTimingRoasterData.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        [HttpPost]
        public async Task<ActionResult> EditSave(EmpTimingRoasterData c, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    // string Corp = form["DayTypelist"] == "0" ? "" : form["DayTypelist"];
                    string Addrs = form["TimingMonthlyRoasterlist"] == "0" ? "" : form["TimingMonthlyRoasterlist"];
                    string ContactDetails = form["TimingPolicylist1"] == "0" ? "" : form["TimingPolicylist1"];
                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    //if (Corp != null)
                    //{
                    //    if (Corp != "")
                    //    {
                    //        var val = db.LookupValue.Find(int.Parse(Corp));
                    //        c.DayType = val;
                    //    }
                    //}

                    //if (Addrs != null)
                    //{
                    //    if (Addrs != "")
                    //    {
                    //        int AddId = Convert.ToInt32(Addrs);
                    //        var val = db.TimingMonthlyRoaster.Include(e => e.TimingGroup)
                    //                            .Where(e => e.Id == AddId).SingleOrDefault();
                    //        c.TimingMonthlyRoaster = val;
                    //    }
                    //}

                    //if (ContactDetails != null)
                    //{
                    //    if (ContactDetails != "")
                    //    {
                    //        int ContId = Convert.ToInt32(ContactDetails);
                    //        var val = db.TimingPolicy.Where(e => e.Id == ContId).SingleOrDefault();
                    //        c.TimingPolicy = val;
                    //    }
                    //}


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    EmpTimingRoasterData blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.EmpTimingRoasterData.Where(e => e.Id == data).Include(e => e.DayType).SingleOrDefault();
                                        //  .Include(e => e.TimingMonthlyRoaster)
                                        // .Include(e => e.TimingPolicy).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };
                                    c.DayType = blog.DayType;
                                    c.RoasterDate = blog.RoasterDate;
                                    //int a = EditS(Corp, Addrs, ContactDetails, data, c, c.DBTrack);
                                    int a = EditS(Addrs, ContactDetails, data, c, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Attendance/Attendance", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_EmpTimingRoasterData DT_Corp = (DT_EmpTimingRoasterData)obj;
                                        //   DT_Corp.TimingMonthlyRoaster_Id = blog.TimingMonthlyRoaster == null ? 0 : blog.TimingMonthlyRoaster.Id;
                                        //  DT_Corp.TimingPolicy_Id = blog.TimingPolicy == null ? 0 : blog.TimingPolicy.Id;
                                        //   DT_Corp.DayType_Id = blog.DayType == null ? 0 : blog.DayType.Id;
                                        //       db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();


                                    //return Json(new Object[] { c.Id, c.RoasterDate, "Record Updated", JsonRequestBehavior.AllowGet });
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.RoasterDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (EmpTimingRoasterData)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    var databaseValues = (EmpTimingRoasterData)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            EmpTimingRoasterData blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            EmpTimingRoasterData Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.EmpTimingRoasterData.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            c.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            EmpTimingRoasterData corp = new EmpTimingRoasterData()
                            {
                                RoasterDate = c.RoasterDate,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Attendance/Attendance", "M", blog, corp, "EmpTimingRoasterData", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                //  Old_Corp = context.EmpTimingRoasterData.Where(e => e.Id == data).Include(e => e.DayType)
                                //  .Include(e => e.TimingMonthlyRoaster).Include(e => e.TimingPolicy).SingleOrDefault();
                                //    DT_EmpTimingRoasterData DT_Corp = (DT_EmpTimingRoasterData)obj;
                                //    DT_Corp.TimingMonthlyRoaster_Id = DBTrackFile.ValCompare(Old_Corp.TimingMonthlyRoaster, c.TimingMonthlyRoaster);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                //    DT_Corp.DayType_Id = DBTrackFile.ValCompare(Old_Corp.DayType, c.DayType); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                //  DT_Corp.TimingPolicy_Id = DBTrackFile.ValCompare(Old_Corp.TimingPolicy, c.TimingPolicy); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                //     db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.EmpTimingRoasterData.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            //return Json(new Object[] { blog.Id, c.RoasterDate, "Record Updated", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.RoasterDate.Value.ToShortDateString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

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
            public string DayType { get; set; }
            public string InTime { get; set; }
            public string RoasterDate { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeAttendance.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                        //.Include(e => e.Employee.ServiceBookDates)
                        //.Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        //.Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeeAttendance> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
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
                                //   JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //  Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job.Name : null,
                                //   Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade.Name : null,
                                //  Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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

        public ActionResult Get_EmpTimingRostData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAttendance
                        .Include(e => e.EmpTimingMonthlyRoaster)
                        .Include(e => e.EmpTimingMonthlyRoaster.Select(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy))
                        .Include(e => e.EmpTimingMonthlyRoaster.Select(q => q.DayType))
                       .Where(e => e.Employee.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpRepoChildDataClass> returndata = new List<EmpRepoChildDataClass>();
                        foreach (var item in db_data.EmpTimingMonthlyRoaster.OrderByDescending(e=>e.RoasterDate).ToList())
                        {
                            returndata.Add(new EmpRepoChildDataClass
                            {

                                Id = item.Id,
                                DayType = item.DayType != null ? item.DayType.LookupVal.ToString() : "",
                                InTime = item.TimingMonthlyRoaster != null && item.TimingMonthlyRoaster.TimingGroup != null && item.TimingMonthlyRoaster.TimingGroup.TimingPolicy != null ? string.Join(",", item.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(e => e.TimingCode).ToList()) : item.TimingPolicy.TimingCode,
                                RoasterDate = item.RoasterDate != null ? item.RoasterDate.Value.ToString("dd/MM/yyyy") : "",
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
        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //  var context = DataContextFactory.GetDataContext();
                var changedEntries = db.ChangeTracker.Entries()
                    .Where(x => x.State != System.Data.Entity.EntityState.Unchanged).ToList();

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Modified))
                {
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Added))
                {
                    entry.State = System.Data.Entity.EntityState.Detached;
                }

                foreach (var entry in changedEntries.Where(x => x.State == System.Data.Entity.EntityState.Deleted))
                {
                    entry.State = System.Data.Entity.EntityState.Unchanged;
                }

            }
        }
        public class RoasterClass
        {
            public string Date { get; set; }
            public string SNo { get; set; }
            public string Time { get; set; }
            public string Type { get; set; }
            public string TG { get; set; }
        }
        public class returnObjClass
        {
            public int status { get; set; }
            public List<string> MSG { get; set; }
        }
        public ActionResult CreateRoaster(List<RoasterClass> data, List<Int32> EmpId, bool overide = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();
                //List<returnObjClass> returnObjList = new List<returnObjClass>();

                try
                {
                    foreach (var Id in EmpId)
                    {

                        var month = Convert.ToDateTime(data[0].Date);

                        var getEmpAttendance = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster)
                             .Include(e => e.EmpTimingMonthlyRoaster.Select(x => x.TimingMonthlyRoaster))
                            .Include(e => e.Employee)
                            .Where(e => e.Employee.Id == Id && e.EmpTimingMonthlyRoaster.Any(a => a.RoasterDate >= month)).FirstOrDefault();
                        List<EmpTimingMonthlyRoaster> oEmpTimingRoasterDataList = new List<EmpTimingMonthlyRoaster>();

                        //delete roaster
                        if (overide)
                        {
                            var removeR = getEmpAttendance.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Month == month.Month && e.RoasterDate.Value.Year == month.Year).ToList();
                            foreach (var item in removeR)
                            {
                                if (item.TimingMonthlyRoaster != null)
                                {

                                    var timemonroaster = db.TimingMonthlyRoaster.Where(x => x.Id == item.TimingMonthlyRoaster.Id).SingleOrDefault();
                                    db.TimingMonthlyRoaster.Remove(timemonroaster);
                                    db.SaveChanges();
                                }
                            }
                            db.EmpTimingMonthlyRoaster.RemoveRange(removeR);
                            db.SaveChanges();
                        }
                        foreach (RoasterClass item in data)
                        {
                            if (getEmpAttendance != null && getEmpAttendance.EmpTimingMonthlyRoaster.Any(a => a.RoasterDate == Convert.ToDateTime(item.Date)))
                            {
                                returnObjClass returnObj = new returnObjClass();
                                MSG.Add(item.Date + " Already Roaster Present");
                                return Json(new { status = 1, MSG = MSG }, JsonRequestBehavior.AllowGet);
                            }

                            var TG = db.TimingGroup.Where(e => e.GroupCode == item.TG).SingleOrDefault();
                            EmpTimingMonthlyRoaster oEmpTimingRoasterData = new EmpTimingMonthlyRoaster
                            {
                                DayType = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1001").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToLower() == item.Type.ToLower()).FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal.ToLower() == item.Type.ToLower()).SingleOrDefault(),
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                RoasterDate = Convert.ToDateTime(item.Date),
                                // TimingMonthlyRoaster table data not save because time code assign from page
                                //TimingMonthlyRoaster = new TimingMonthlyRoaster
                                //{
                                //    TimingGroup = TG,
                                //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                //},
                                TimingPolicy = db.TimingPolicy.Where(e => e.TimingCode == item.Time).SingleOrDefault()
                            };
                            oEmpTimingRoasterDataList.Add(oEmpTimingRoasterData);
                        }
                        if (oEmpTimingRoasterDataList.Count > 0)
                        {
                            foreach (EmpTimingMonthlyRoaster oEmpTimingRoasterData in oEmpTimingRoasterDataList)
                            {
                                if (getEmpAttendance != null)
                                {
                                    getEmpAttendance.EmpTimingMonthlyRoaster.Add(oEmpTimingRoasterData);
                                }
                                else
                                {
                                    getEmpAttendance = db.EmployeeAttendance
                                        .Include(e => e.EmpTimingRoasterData)
                                        .Include(e => e.Employee)
                                        .Where(e => e.Employee.Id == Id).FirstOrDefault();
                                    getEmpAttendance.EmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster> { oEmpTimingRoasterData };
                                }
                            }

                            db.Entry(getEmpAttendance).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            MSG.Add(" Data Saved Successfully");
                            return Json(new { status = 0, MSG = MSG }, JsonRequestBehavior.AllowGet);

                        }
                        return Json(MSG, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (Exception e)
                {
                    MSG.Add(e.InnerException.Message.ToString());
                    return Json(MSG, JsonRequestBehavior.AllowGet);

                }
                return View();
            }
        }
    }
}
