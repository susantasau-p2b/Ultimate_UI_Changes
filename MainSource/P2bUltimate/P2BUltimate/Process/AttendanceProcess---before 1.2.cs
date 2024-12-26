using System;
using P2BUltimate.App_Start;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Attendance;
using Leave;
using P2b.Global;
using P2BUltimate.Security;
using System.Data.OleDb;
using P2BUltimate.Models;
using System.Data.SqlClient;
using System.Data;
using System.Transactions;
using System.IO;

namespace P2BUltimate.Process
{
    public class AttendanceProcess
    {
        public class returnClass
        {
            public string ErrNo { get; set; }
            public string ErrDesc { get; set; }
        }
        public class remark_return_class
        {
            public string _Remark { get; set; }
            public int _LateCount { get; set; }
            public int _EarlyCount { get; set; }
        }
        public static void _oTimingPolicyBatchAssignment_IsRoasterGroup(DateTime _First, DateTime _Last, TimingGroup oTimingGroup, Int32 EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var employee = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                var _fetch_emp_policy1 = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                           .Include(a => a.Employee.GeoStruct)
                           .Include(a => a.Employee.GeoStruct.Location)
                           .Include(a => a.Employee.FuncStruct)
                           .Include(a => a.EmpTimingRoasterData)
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.GeoGraphList))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.TimingPolicy))
                           .Where(a => a.Employee.Id == EmpId && a.EmpReportingTimingStruct.Count > 0)
                           .AsParallel().SingleOrDefault();

                List<EmpTimingMonthlyRoaster> EmpTimingMonthlyRoasterList = new List<EmpTimingMonthlyRoaster>();
                for (DateTime _Date = _First; _Date <= _Last; _Date = _Date.AddDays(1))
                {
                    string TimeingCode = "";
                    var _CompId = Convert.ToInt32(SessionManager.CompanyId);


                    string strHWWO = AttendanceProcess.Check_HWWO(_CompId, _fetch_emp_policy1, _Date);
                    if (strHWWO != null)
                    {
                        TimeingCode = strHWWO;
                    }
                    // check monthly roaster manualy created or not if created then process roater will not create
                    EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance.Include(a => a.EmpTimingMonthlyRoaster)
                                                                                           .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                                                                                           .Where(a => a.Employee.Id == EmpId
                                                                                               && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _Date &&
                                                                                                   DbFunctions.TruncateTime(e.RoasterDate) <= _Date)).AsParallel().SingleOrDefault();


                    if (_Prv_Month_Struct == null)
                    {

                        EmpTimingMonthlyRoaster oEmpTimingMonthlyRoaster = new EmpTimingMonthlyRoaster();
                        if (oTimingGroup.TimingPolicy.Count() == 1)
                        {
                            var TimingPolicy_id = oTimingGroup.TimingPolicy.LastOrDefault();
                            oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                            oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            oEmpTimingMonthlyRoaster.TimingPolicy = db.TimingPolicy.Where(e => e.Id == TimingPolicy_id.Id).SingleOrDefault();
                            oEmpTimingMonthlyRoaster.DayType = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == TimeingCode.ToUpper()).FirstOrDefault();//db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault();

                        }
                        else
                        {
                            var TimingPolicy_id = oTimingGroup.TimingPolicy.FirstOrDefault();
                            oEmpTimingMonthlyRoaster.TimingPolicy = db.TimingPolicy.Where(e => e.Id == TimingPolicy_id.Id).SingleOrDefault();// manual roaster code entry time by default this code will display as sugges by sir
                            oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                            oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            oEmpTimingMonthlyRoaster.DayType = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == TimeingCode.ToUpper()).FirstOrDefault(); //db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault();
                            oEmpTimingMonthlyRoaster.TimingMonthlyRoaster = new TimingMonthlyRoaster
                            {
                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                RoasterName = oTimingGroup.GroupName,
                                TimingGroup = db.TimingGroup.Where(e => e.Id == oTimingGroup.Id).SingleOrDefault()
                            };
                        }
                        EmpTimingMonthlyRoasterList.Add(oEmpTimingMonthlyRoaster);
                    }
                }
                foreach (EmpTimingMonthlyRoaster item in EmpTimingMonthlyRoasterList)
                {
                    employee.EmpTimingMonthlyRoaster.Add(item);
                }
                db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

        }
        public static void _oTimingPolicyBatchAssignment_IsTimingGroup(DateTime _First, DateTime _Last, TimingGroup oTimingGroup, Int32 EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var employee = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                var _fetch_emp_policy1 = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                           .Include(a => a.Employee.GeoStruct)
                           .Include(a => a.Employee.GeoStruct.Location)
                           .Include(a => a.Employee.FuncStruct)
                           .Include(a => a.EmpTimingRoasterData)
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.GeoGraphList))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.TimingPolicy))
                           .Where(a => a.Employee.Id == EmpId && a.EmpReportingTimingStruct.Count > 0)
                           .AsParallel().SingleOrDefault();

                List<EmpTimingMonthlyRoaster> EmpTimingMonthlyRoasterList = new List<EmpTimingMonthlyRoaster>();
                for (DateTime _Date = _First; _Date <= _Last; _Date = _Date.AddDays(1))
                {
                    string TimeingCode = "";
                    var _CompId = Convert.ToInt32(SessionManager.CompanyId);


                    string strHWWO = AttendanceProcess.Check_HWWO(_CompId, _fetch_emp_policy1, _Date);
                    if (strHWWO != null)
                    {
                        TimeingCode = strHWWO;
                    }
                    EmpTimingMonthlyRoaster oEmpTimingMonthlyRoaster = new EmpTimingMonthlyRoaster();
                    if (oTimingGroup.TimingPolicy.Count() == 1)
                    {
                        var TimingPolicy_id = oTimingGroup.TimingPolicy.LastOrDefault();
                        oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                        oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        oEmpTimingMonthlyRoaster.TimingPolicy = db.TimingPolicy.Where(e => e.Id == TimingPolicy_id.Id).SingleOrDefault();
                        oEmpTimingMonthlyRoaster.DayType = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == TimeingCode.ToUpper()).FirstOrDefault();//db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault();

                    }
                    else
                    {
                        oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                        oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        oEmpTimingMonthlyRoaster.DayType = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == TimeingCode.ToUpper()).FirstOrDefault();//db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault();
                        oEmpTimingMonthlyRoaster.TimingMonthlyRoaster = new TimingMonthlyRoaster
                         {
                             DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                             RoasterName = oTimingGroup.GroupName,
                             TimingGroup = db.TimingGroup.Where(e => e.Id == oTimingGroup.Id).SingleOrDefault()
                         };
                    }
                    EmpTimingMonthlyRoasterList.Add(oEmpTimingMonthlyRoaster);
                }
                foreach (EmpTimingMonthlyRoaster item in EmpTimingMonthlyRoasterList)
                {
                    employee.EmpTimingMonthlyRoaster.Add(item);
                }
                db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

        }
        public static void oTimingPolicyBatchAssignment_IsWeeklyTimingSchedule(DateTime _First, DateTime _Last,
                                                                                List<TimingWeeklySchedule> oTimingWeeklySchedule, Int32 EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var employee = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                List<EmpTimingMonthlyRoaster> EmpTimingMonthlyRoasterList = new List<EmpTimingMonthlyRoaster>();
                for (DateTime _Date = _First; _Date <= _Last; _Date = _Date.AddDays(1))
                {
                    var day = _Date.ToString("dddd").ToUpper();
                    var DaysWise = oTimingWeeklySchedule.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.TimingGroup).SingleOrDefault();
                    EmpTimingMonthlyRoaster oEmpTimingMonthlyRoaster = new EmpTimingMonthlyRoaster();
                    if (DaysWise.IsManualRotateShift)
                    {

                    }
                    if (DaysWise.TimingPolicy.Count() == 1)
                    {
                        var TimingPolicy_id = DaysWise.TimingPolicy.LastOrDefault();
                        oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                        oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        oEmpTimingMonthlyRoaster.TimingPolicy = db.TimingPolicy.Where(e => e.Id == TimingPolicy_id.Id).SingleOrDefault();

                    }
                    else
                    {
                        oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                        oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        oEmpTimingMonthlyRoaster.TimingMonthlyRoaster = new TimingMonthlyRoaster
                        {
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                            RoasterName = DaysWise.GroupName,
                            //TimingGroup = DaysWise
                            TimingGroup = db.TimingGroup.Where(e => e.Id == DaysWise.Id).SingleOrDefault()

                        };
                    }
                    EmpTimingMonthlyRoasterList.Add(oEmpTimingMonthlyRoaster);
                }
                foreach (EmpTimingMonthlyRoaster item in EmpTimingMonthlyRoasterList)
                {
                    employee.EmpTimingMonthlyRoaster.Add(item);
                }
                db.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

        }
        public static void Genrate_InTimePolicyRoaster(DateTime _First, DateTime _Last, Int32 EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // DateTime _Last = Convert.ToDateTime(DateTime.DaysInMonth(_First.Year, _First.Month) + "/" + _First.Month + "/" + _First.Year);

                var EmployeeAttendance_EmpTimingRoasterData = db.EmployeeAttendance
                    .Include(a => a.EmpTimingRoasterData)
                    .Include(a => a.EmpTimingRoasterData.Select(e => e.DayType))
                    .Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                List<EmpTimingMonthlyRoaster> EmpTimingMonthlyRoasterList = new List<EmpTimingMonthlyRoaster>();

                for (DateTime _Date = _First; _Date <= _Last; _Date = _Date.AddDays(1))
                {
                    var Roaster = EmployeeAttendance_EmpTimingRoasterData.EmpTimingRoasterData.Where(e => e.RoasterDate == _Date).SingleOrDefault();
                    if (Roaster == null)
                    {
                        continue;
                    }
                    // var timingpolicy = db.TimingPolicy.Where(e => e.TimingCode == Roaster.InTime.Value.Hour + "_Global").SingleOrDefault();
                    var timingpolicy = db.TimingPolicy.Where(e => e.TimingCode.Contains("_Global")).SingleOrDefault(); // only one time policy create for Global as sugges by sir
                    if (timingpolicy == null)
                    {
                        continue;
                    }
                    EmpTimingMonthlyRoaster oEmpTimingMonthlyRoaster = new EmpTimingMonthlyRoaster();
                    oEmpTimingMonthlyRoaster.DayType = Roaster.DayType;
                    oEmpTimingMonthlyRoaster.RoasterDate = _Date;
                    oEmpTimingMonthlyRoaster.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    oEmpTimingMonthlyRoaster.TimingPolicy = timingpolicy;
                    EmpTimingMonthlyRoasterList.Add(oEmpTimingMonthlyRoaster);

                }
                foreach (EmpTimingMonthlyRoaster item in EmpTimingMonthlyRoasterList)
                {
                    if (EmployeeAttendance_EmpTimingRoasterData.EmpTimingMonthlyRoaster == null)
                    {
                        EmployeeAttendance_EmpTimingRoasterData.EmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster> { item };
                    }
                    else
                    {
                        EmployeeAttendance_EmpTimingRoasterData.EmpTimingMonthlyRoaster.Add(item);
                    }
                }
                db.Entry(EmployeeAttendance_EmpTimingRoasterData).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }
        public static List<TimingPolicy> GetCompanyTimingpolicy(TimingPolicyBatchAssignment oTimingPolicyBatchAssignment,
                                                                ReportingTimingStruct oEmpReportingTimingStruct, DateTime _PayMonth, DateTime _LastDate, Int32 EmpId)
        {
            var oTimingPolicy = new List<TimingPolicy>();
            // DateTime _LastDate = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);

            if (oTimingPolicyBatchAssignment.IsRoaster)
            {
                _oTimingPolicyBatchAssignment_IsRoasterGroup(_PayMonth, _LastDate, oTimingPolicyBatchAssignment.TimingGroup, EmpId);
            }
            else if (oTimingPolicyBatchAssignment.IsTimingGroup)
            {
                _oTimingPolicyBatchAssignment_IsTimingGroup(_PayMonth, _LastDate, oTimingPolicyBatchAssignment.TimingGroup, EmpId);
            }
            else if (oTimingPolicyBatchAssignment.IsWeeklyTimingSchedule)
            {
                oTimingPolicyBatchAssignment_IsWeeklyTimingSchedule(_PayMonth, _LastDate, oTimingPolicyBatchAssignment.TimingweeklySchedule.ToList(), EmpId);
            }
            return oTimingPolicy;
        }
        public static string Check_HWWO(Int32 CompId, EmployeeAttendance _fetch_emp_policy, DateTime _Date)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TimeingCode = null;
                int default_DateIsHolidayDay = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "HOLIDAYCALENDAR" && e.Default == true)
                   .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();
                int default_DateIsWeeklyoff = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "WEEKLYOFFCALENDAR" && e.Default == true)
                    .AsNoTracking().OrderBy(e => e.Id).Select(e => e.Id).FirstOrDefault();

                int LocId = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location)
                              .Where(e => e.Id == _fetch_emp_policy.Employee.Id).FirstOrDefault().GeoStruct.Location.Id;

                int OHOList = db.Location.Where(e => e.Id == LocId).Select(e => e.HolidayCalendar.Where(a => a.HoliCalendar.Id == default_DateIsHolidayDay).Select(z => z.Id).FirstOrDefault())
                                .FirstOrDefault();

                int OWOList = db.Location.Where(e => e.Id == LocId).Select(e => e.WeeklyOffCalendar.Where(a => a.WOCalendar.Id == default_DateIsWeeklyoff).Select(z => z.Id).FirstOrDefault())
                                .FirstOrDefault();

                IEnumerable<HolidayCalendar> _Holiday_Company = db.Company
                            .Where(e => e.Id == CompId && e.Location.Any(a => a.Id == _fetch_emp_policy.Employee.GeoStruct.Location.Id))
                            .Select(a => a.HolidayCalendar.Where(x => x.Id == OHOList && x.HolidayList.Any(e => e.HolidayDate == _Date)))
                            .AsParallel().SingleOrDefault();

                var day = _Date.ToString("dddd");

                IEnumerable<WeeklyOffCalendar> _Wealyoff_Calendar =
                    db.Company.Where(e => e.Id == CompId && e.Location.Any(a => a.Id == _fetch_emp_policy.Employee.GeoStruct.Location.Id))
                    .Select(a => a.WeeklyOffCalendar.Where(v => v.Id == OWOList && v.WeeklyOffList.Any(c => c.WeekDays.LookupVal.ToUpper() == day.ToUpper())))
                .AsParallel().SingleOrDefault();
                if (_Holiday_Company.Count() > 0)
                {
                    TimeingCode = "HolidayOff";
                }
                else
                {
                    if (_Wealyoff_Calendar.Count() > 0)
                    {
                        TimeingCode = "Weeklyoff";
                    }
                    else
                    {
                        TimeingCode = "Working";
                    }
                }
                return TimeingCode;
            }
        }
        public static List<EmpTimingMonthlyRoaster> _returnEmpTimingMonthlyRoaster(Int32 CompId, EmployeeAttendance _fetch_emp_policy,
                                                                                    List<TimingPolicy> _oTimingPolicy, DateTime _PayMonth, DateTime _LastDate)
        {
            List<EmpTimingMonthlyRoaster> oEmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster>();
            using (DataBaseContext db = new DataBaseContext())
            {
                for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
                {
                    string TimeingCode = "";
                    //Check It Company holiday/weakly off
                    string strHWWO = AttendanceProcess.Check_HWWO(CompId, _fetch_emp_policy, _Date);
                    if (strHWWO != null)
                    {
                        TimeingCode = strHWWO;
                    }
                    //----------------------------------
                    oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                    {
                        RoasterDate = _Date,
                        DayType = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == TimeingCode.ToUpper()).FirstOrDefault(), //db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault(),
                        TimingPolicy = TimeingCode == "Working" ? _oTimingPolicy.LastOrDefault() : null,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    });
                }
            }
            return oEmpTimingMonthlyRoaster;
        }
        public static void Monthly_Roaster_check(Int32 _CompId, List<int> _EmpIds, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            // Precheck strat
            DateTime _PeriodFromchk = Convert.ToDateTime(_PeriodFrom);
            DateTime _PeriodTochk = Convert.ToDateTime(_PeriodTo);
            string LinePrint;
            LinePrint = "";
            List<string> Msg = new List<string>();
            Int32 comp_id = Convert.ToInt32(SessionManager.CompanyId);

            foreach (var EmpId in _EmpIds)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var _fetch_emp_policy = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                           .Include(a => a.Employee)
                           .Include(a => a.Employee.GeoStruct)
                           .Include(a => a.Employee.GeoStruct.Location)
                           .Include(a => a.Employee.FuncStruct)
                           .Include(a => a.EmpTimingRoasterData)
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.GeoGraphList))
                           .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.TimingPolicy))
                           .Where(a => a.Employee.Id == EmpId)
                           .AsParallel().SingleOrDefault();
                    if (_fetch_emp_policy != null)
                    {
                        var _EmpReportingTimingStruct = _fetch_emp_policy.EmpReportingTimingStruct.Where(a => a.EndDate == null).FirstOrDefault();
                        if (_EmpReportingTimingStruct != null)
                        {
                            var _emp_reporting_struct = _EmpReportingTimingStruct.ReportingTimingStruct;


                            // first Condition
                            if (_emp_reporting_struct.GeographicalAppl)
                            {
                                var _Comp_Attendance = db.CompanyAttendance
                      .Include(a => a.OrgTimingPolicyBatchAssignment)
                      .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.Geostruct))
                      .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.FuncStruct))
                      .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment))
                      .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule)))
                       .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(y => y.WeekDays))))
                       .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(z => z.TimingGroup))))
                       .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(x => x.TimingweeklySchedule.Select(b => b.TimingGroup.TimingPolicy))))
                      .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup)))
                      .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup.TimingPolicy)))
                      .Where(a => a.Company.Id == comp_id)
                     .AsParallel().SingleOrDefault();

                                OrgTimingPolicyBatchAssignment _Comp_Attendance_policy = _Comp_Attendance.OrgTimingPolicyBatchAssignment
                               .Where(a => (a.Geostruct != null && a.FuncStruct != null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id && a.FuncStruct.Id == _fetch_emp_policy.Employee.FuncStruct.Id) || (a.Geostruct != null && a.FuncStruct == null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id)).LastOrDefault();

                                if (_Comp_Attendance_policy != null)
                                {
                                    TimingPolicyBatchAssignment oTimingPolicyBatchAssignmentdel = _Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault();

                                    if (oTimingPolicyBatchAssignmentdel.IsTimingGroup || oTimingPolicyBatchAssignmentdel.IsWeeklyTimingSchedule)
                                    {

                                        EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance
                                       .Include(a => a.Employee)
                                       .Include(a => a.EmpTimingMonthlyRoaster)
                          .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                          .Where(a => a.Employee.Id == EmpId
                              && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PeriodFromchk &&
                                  DbFunctions.TruncateTime(e.RoasterDate) <= _PeriodTochk)).AsParallel().SingleOrDefault();
                                        if (_Prv_Month_Struct != null)
                                        {
                                            for (DateTime _Date = _PeriodFromchk; _Date <= _PeriodTochk; _Date = _Date.AddDays(1))
                                            {
                                                var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date == _Date).FirstOrDefault();
                                                if (removeR == null)
                                                {
                                                    LinePrint = "";
                                                    LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created On" + _Date;
                                                    Msg.Add(LinePrint);
                                                    // employee roaster not create this date
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LinePrint = "";
                                            LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created between" + _PeriodFromchk + " and" + _PeriodTochk;
                                            Msg.Add(LinePrint);
                                            //Roaster Not Created Between _PeriodFromchk _PeriodTochk
                                        }
                                    }
                                    else
                                    {

                                        EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance
                                       .Include(a => a.Employee)
                                       .Include(a => a.EmpTimingMonthlyRoaster)
                          .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                          .Where(a => a.Employee.Id == EmpId
                              && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PeriodFromchk &&
                                  DbFunctions.TruncateTime(e.RoasterDate) <= _PeriodTochk)).AsParallel().SingleOrDefault();
                                        if (_Prv_Month_Struct != null)
                                        {
                                            for (DateTime _Date = _PeriodFromchk; _Date <= _PeriodTochk; _Date = _Date.AddDays(1))
                                            {
                                                var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date == _Date).FirstOrDefault();
                                                if (removeR == null)
                                                {
                                                    LinePrint = "";
                                                    LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created On" + _Date;
                                                    Msg.Add(LinePrint);
                                                    // employee roaster not create this date
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LinePrint = "";
                                            LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created between" + _PeriodFromchk + " and" + _PeriodTochk;
                                            Msg.Add(LinePrint);
                                            //Roaster Not Created Between _PeriodFromchk _PeriodTochk
                                        }


                                    }

                                }
                                else
                                {
                                    // Fld30 = ca.Employee.GeoStruct != null && ca.Employee.GeoStruct.Location != null && ca.Employee.GeoStruct.Location.LocationObj != null ? ca.Employee.GeoStruct.Location.LocationObj.LocCode : null,

                                    LinePrint = "";
                                    if (_fetch_emp_policy.Employee.GeoStruct != null && _fetch_emp_policy.Employee.GeoStruct.Location != null && _fetch_emp_policy.Employee.GeoStruct.Location.LocationObj != null)
                                    {
                                        LinePrint = "Loc Code:" + _fetch_emp_policy.Employee.GeoStruct.Location.LocationObj.LocCode + " Policy Not Define";
                                    }
                                    else
                                    {
                                        LinePrint = "Emp Code:" + _fetch_emp_policy.Employee.EmpCode + " employee Location Policy Not Define";
                                    }
                                    Msg.Add(LinePrint);
                                    //Locaton policy not define
                                }
                            }
                            else if (_emp_reporting_struct.IsTimeRoaster)
                            {
                                EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance
                                    .Include(a => a.Employee)
                                    .Include(a => a.EmpTimingMonthlyRoaster)
                       .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                       .Where(a => a.Employee.Id == EmpId
                           && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PeriodFromchk &&
                               DbFunctions.TruncateTime(e.RoasterDate) <= _PeriodTochk)).AsParallel().SingleOrDefault();
                                if (_Prv_Month_Struct != null)
                                {
                                    for (DateTime _Date = _PeriodFromchk; _Date <= _PeriodTochk; _Date = _Date.AddDays(1))
                                    {
                                        var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date == _Date && e.TimingMonthlyRoaster != null).FirstOrDefault();
                                        if (removeR == null)
                                        {
                                            LinePrint = "";
                                            LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created On" + _Date;
                                            Msg.Add(LinePrint);
                                            // employee roaster not create this date
                                        }
                                    }
                                }
                                else
                                {
                                    LinePrint = "";
                                    LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created between" + _PeriodFromchk + " and" + _PeriodTochk;
                                    Msg.Add(LinePrint);
                                    //Roaster Not Created Between _PeriodFromchk _PeriodTochk
                                }


                            }
                            else if (_emp_reporting_struct.IndividualAppl)
                            {
                                EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance
                                                                       .Include(a => a.Employee)
                                                                       .Include(a => a.EmpTimingMonthlyRoaster)
                                                          .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                                                          .Where(a => a.Employee.Id == EmpId
                                                              && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PeriodFromchk &&
                                                                  DbFunctions.TruncateTime(e.RoasterDate) <= _PeriodTochk)).AsParallel().SingleOrDefault();
                                if (_Prv_Month_Struct != null)
                                {
                                    for (DateTime _Date = _PeriodFromchk; _Date <= _PeriodTochk; _Date = _Date.AddDays(1))
                                    {
                                        var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date == _Date).FirstOrDefault();
                                        if (removeR == null)
                                        {
                                            LinePrint = "";
                                            LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created On" + _Date;
                                            Msg.Add(LinePrint);
                                            // employee roaster not create this date
                                        }
                                    }
                                }
                                else
                                {
                                    LinePrint = "";
                                    LinePrint = "Emp Code:" + _Prv_Month_Struct.Employee.EmpCode + " Roaster Not Created between" + _PeriodFromchk + " and" + _PeriodTochk;
                                    Msg.Add(LinePrint);
                                    //Roaster Not Created Between _PeriodFromchk _PeriodTochk
                                }

                            }


                        }
                        else
                        {
                            LinePrint = "";
                            LinePrint = "Emp Code:" + _fetch_emp_policy.Employee.EmpCode + " Reporting Structure Not Availble";
                            Msg.Add(LinePrint);


                            //reporting struct not define for this employee
                        }

                    }
                    else
                    {

                        LinePrint = "";
                        LinePrint = "Emp Code:" + _fetch_emp_policy.Employee.EmpCode + " Attendance ID Not Genrated";
                        Msg.Add(LinePrint);
                        // attendance id not genrate for this employee
                    }


                }
            }

            if (Msg.Count() > 0)
            {
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\MAN_ROASTER_" + Convert.ToDateTime(DateTime.Now.Date).ToString("ddMMyyyy") + ".txt";
                localPath = new Uri(path).LocalPath;

                if (File.Exists(localPath))
                {
                    File.Delete(localPath);

                }

                using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                {
                    StreamWriter str = new StreamWriter(fs);
                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                    foreach (var ca in Msg)
                    {
                        if (ca != "" && ca != null)
                        {

                            str.WriteLine(ca);

                        }
                    }
                    str.Flush();
                    str.Close();
                    fs.Close();
                }
                System.Diagnostics.Process.Start("notepad.exe", localPath);




            }

            //precheck end
        }

        public static void Genrate_Monthly_Roaster(Int32 _CompId, List<int> _EmpIds, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            foreach (var EmpId in _EmpIds)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (_PeriodFrom != null)
                    {
                        List<TimingPolicy> _oTimingPolicy = new List<TimingPolicy>();
                        DateTime _PayMonth = Convert.ToDateTime(_PeriodFrom);
                        DateTime _LastDate = Convert.ToDateTime(_PeriodTo);
                        // DateTime _LastDate = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                        List<EmpTimingMonthlyRoaster> oEmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster>();
                        _PayMonth = _PayMonth.Date;
                        _LastDate = _LastDate.Date;


                        //find emp wise polcy


                        var _fetch_emp_policy = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                            .Include(a => a.Employee.GeoStruct)
                            .Include(a => a.Employee.GeoStruct.Location)
                            .Include(a => a.Employee.FuncStruct)
                            .Include(a => a.EmpTimingRoasterData)
                            .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                            .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.GeoGraphList))
                            .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.TimingPolicy))
                            .Where(a => a.Employee.Id == EmpId && a.EmpReportingTimingStruct.Count > 0)
                            .AsParallel().SingleOrDefault();
                        bool isEmplIndividual = false;
                        List<EmpTimingRoasterData> _List_EmpTimingRoasterData = new List<EmpTimingRoasterData>();
                        if (_fetch_emp_policy != null)
                        {
                            var _EmpReportingTimingStruct = _fetch_emp_policy.EmpReportingTimingStruct.Where(a => a.EndDate == null).FirstOrDefault();
                            if (_EmpReportingTimingStruct != null)
                            {

                                var _emp_reporting_struct = _EmpReportingTimingStruct.ReportingTimingStruct;
                                if (_emp_reporting_struct.IsTimeRoaster || _emp_reporting_struct.IndividualAppl)
                                {
                                    EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance.Include(a => a.EmpTimingMonthlyRoaster)
                                .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                                .Where(a => a.Employee.Id == EmpId
                                    && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PayMonth &&
                                        DbFunctions.TruncateTime(e.RoasterDate) <= _LastDate)).AsParallel().SingleOrDefault();

                                    if (_Prv_Month_Struct != null)
                                    {
                                        //delete _Prv_Month_Struct
                                        var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth && e.RoasterDate.Value.Date <= _LastDate && e.TimingMonthlyRoaster != null).ToList();
                                        foreach (var item in removeR)
                                        {
                                            var timemonroaster = db.TimingMonthlyRoaster.Where(x => x.Id == item.TimingMonthlyRoaster.Id).SingleOrDefault();
                                            db.TimingMonthlyRoaster.Remove(timemonroaster);
                                            db.SaveChanges();
                                        }

                                        db.EmpTimingMonthlyRoaster.RemoveRange(_Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth &&
                                           e.RoasterDate.Value.Date <= _LastDate).ToList());
                                        db.SaveChanges();
                                        //------------------------------------------------------------------------------------------
                                    }
                                }
                                if (_emp_reporting_struct.GeographicalAppl)
                                {
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
                              .Where(a => a.Company.Id == _CompId)
                             .AsParallel().SingleOrDefault();

                                    OrgTimingPolicyBatchAssignment _Comp_Attendance_policy = _Comp_Attendance.OrgTimingPolicyBatchAssignment
                                        .Where(a => (a.Geostruct != null && a.FuncStruct != null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id && a.FuncStruct.Id == _fetch_emp_policy.Employee.FuncStruct.Id) || (a.Geostruct != null && a.FuncStruct == null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id)).LastOrDefault();


                                    if (_Comp_Attendance_policy != null)
                                    {
                                        TimingPolicyBatchAssignment oTimingPolicyBatchAssignmentdel = _Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault();

                                        if (oTimingPolicyBatchAssignmentdel.IsTimingGroup || oTimingPolicyBatchAssignmentdel.IsWeeklyTimingSchedule)
                                        {
                                            EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance.Include(a => a.EmpTimingMonthlyRoaster)
                                                        .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                                                        .Where(a => a.Employee.Id == EmpId
                                                            && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PayMonth &&
                                                                DbFunctions.TruncateTime(e.RoasterDate) <= _LastDate)).AsParallel().SingleOrDefault();

                                            if (_Prv_Month_Struct != null)
                                            {
                                                //delete _Prv_Month_Struct
                                                var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth && e.RoasterDate.Value.Date <= _LastDate && e.TimingMonthlyRoaster != null).ToList();
                                                foreach (var item in removeR)
                                                {
                                                    var timemonroaster = db.TimingMonthlyRoaster.Where(x => x.Id == item.TimingMonthlyRoaster.Id).SingleOrDefault();
                                                    db.TimingMonthlyRoaster.Remove(timemonroaster);
                                                    db.SaveChanges();
                                                }

                                                db.EmpTimingMonthlyRoaster.RemoveRange(_Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth &&
                                                   e.RoasterDate.Value.Date <= _LastDate).ToList());
                                                db.SaveChanges();
                                                //------------------------------------------------------------------------------------------
                                            }
                                        }
                                        else
                                        {
                                            EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance.Include(a => a.EmpTimingMonthlyRoaster)
                                                                                                .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingMonthlyRoaster))
                                                                                                .Where(a => a.Employee.Id == EmpId
                                                                                                    && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PayMonth &&
                                                                                                        DbFunctions.TruncateTime(e.RoasterDate) <= _LastDate)).AsParallel().SingleOrDefault();




                                            if (_Prv_Month_Struct != null)
                                            {

                                                var timemonthroaster = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.TimingMonthlyRoaster != null).ToList();
                                                if (timemonthroaster.Count() > 0)
                                                {


                                                    //delete _Prv_Month_Struct
                                                    var removeR = _Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth && e.RoasterDate.Value.Date <= _LastDate).ToList();
                                                    foreach (var item in removeR)
                                                    {
                                                        var timemonroaster = db.TimingMonthlyRoaster.Where(x => x.Id == item.TimingMonthlyRoaster.Id).SingleOrDefault();
                                                        db.TimingMonthlyRoaster.Remove(timemonroaster);
                                                        db.SaveChanges();
                                                    }

                                                    db.EmpTimingMonthlyRoaster.RemoveRange(_Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth &&
                                                       e.RoasterDate.Value.Date <= _LastDate).ToList());
                                                    db.SaveChanges();
                                                }
                                                //------------------------------------------------------------------------------------------
                                            }
                                        }


                                        GetCompanyTimingpolicy(_Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault(),
                                            _emp_reporting_struct, _PayMonth, _LastDate, EmpId);
                                    }
                                }
                                else if (_emp_reporting_struct.IsTimeRoaster)
                                {

                                    Genrate_InTimePolicyRoaster(_PayMonth, _LastDate, EmpId);
                                }
                                else if (_emp_reporting_struct.IndividualAppl)
                                {
                                    _oTimingPolicy.Add(_emp_reporting_struct.TimingPolicy);
                                    isEmplIndividual = true;
                                }
                            }
                        }
                        else
                        {
                            continue;
                            //Employee reporting timing not found
                        }
                        if (_oTimingPolicy.Count() > 0 && isEmplIndividual)
                        {
                            //Loop through days and put timing Code
                            // oEmpTimingMonthlyRoaster = _returnEmpTimingMonthlyRoaster(CompId, _fetch_emp_policy, _oTimingPolicy, _PayMonth, _LastDate);
                            for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
                            {
                                string TimeingCode = "";
                                //Check It Company holiday/weakly off
                                string strHWWO = AttendanceProcess.Check_HWWO(_CompId, _fetch_emp_policy, _Date);
                                if (strHWWO != null)
                                {
                                    TimeingCode = strHWWO;
                                }
                                //----------------------------------

                                foreach (var item in _oTimingPolicy)
                                {
                                    oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                                    {
                                        RoasterDate = _Date,
                                        DayType = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == TimeingCode.ToUpper()).FirstOrDefault(),//db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault(),
                                        TimingPolicy = item,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    });
                                }
                            }
                        }

                        if (oEmpTimingMonthlyRoaster.Count > 0)
                        {
                            db.EmpTimingMonthlyRoaster.AddRange(oEmpTimingMonthlyRoaster);
                            db.SaveChanges();

                            var EmpAttendance = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster)
                                .Where(a => a.Employee.Id == EmpId).AsParallel().SingleOrDefault();

                            if (EmpAttendance.EmpTimingMonthlyRoaster != null)
                            {
                                for (int i = 0; i < oEmpTimingMonthlyRoaster.Count; i++)
                                {
                                    EmpAttendance.EmpTimingMonthlyRoaster.Add(oEmpTimingMonthlyRoaster[i]);
                                }
                            }
                            else
                            {
                                EmpAttendance.EmpTimingMonthlyRoaster = oEmpTimingMonthlyRoaster;
                            }

                            db.Entry(EmpAttendance).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
        public static TimeSpan[] _returnTimeInOutTime(List<RawData> _List_oRawData)
        {
            TimeSpan[] _Time = new TimeSpan[2];

            DateTime _min = Convert.ToDateTime(_List_oRawData.Min(a => a.SwipeTime));
            DateTime _max = Convert.ToDateTime(_List_oRawData.Max(a => a.SwipeTime));
            _Time[0] = Utility._returnTimeSpan(_min);
            _Time[1] = Utility._returnTimeSpan(_max);
            return _Time;
        }
        public static TimeSpan[] _returnTimeInOutTimeForOverNightShift(List<RawData> _List_oRawData, DateTime? _Date = null, Int32? _EmpId = 0)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TimeSpan[] _Time = new TimeSpan[2];
                DateTime _PrvDate = _Date.Value.AddDays(-1);
                DateTime? _in;
                DateTime? _out;
                //var _EmployeeAttendance = db.EmployeeAttendance.Include(e => e.RawData).Where(e => e.Employee.Id == _EmpId)
                //    .Select(e => e.RawData.Where(a => a.SwipeDate == _PrvDate)).FirstOrDefault();
                List<RawData> _EmployeeAttendance = db.RawData.Where(e => e.EmployeeAttendance.Employee.Id == _EmpId && e.SwipeDate == _PrvDate).AsNoTracking()
                   .ToList();
                if (_EmployeeAttendance.Count() >= 2 && _List_oRawData.Count() >= 2)
                {
                    _in = _EmployeeAttendance.Max(e => e.SwipeTime).Value;
                    _out = _List_oRawData.Min(e => e.SwipeTime).Value;
                }
                else
                {
                    if (_EmployeeAttendance.Count() == 1 && _EmployeeAttendance.LastOrDefault().SwipeTime.Value.Hour >= 12)
                    {
                        _in = _EmployeeAttendance.Max(e => e.SwipeTime).Value;
                        _out = _List_oRawData.Min(e => e.SwipeTime).Value;
                    }
                    else
                    {
                        _in = null;
                        _out = _List_oRawData.Min(e => e.SwipeTime).Value;
                    }

                    if (_List_oRawData.Count() == 1 && _List_oRawData.LastOrDefault().SwipeTime.Value.Hour <= 12)
                    {
                        _in = _EmployeeAttendance.Max(e => e.SwipeTime).Value;
                        _out = _List_oRawData.Min(e => e.SwipeTime).Value;
                    }
                    else
                    {
                        if (_EmployeeAttendance.Count() > 0)
                        {
                            _in = _EmployeeAttendance.Max(e => e.SwipeTime).Value;
                        }
                        _out = null;
                    }
                }
                if (_in != null)
                {
                    _Time[0] = Utility._returnTimeSpan(_in);
                }
                if (_out != null)
                {
                    _Time[1] = Utility._returnTimeSpan(_out);
                }
                return _Time;
            }
        }
        public static string _returnLunchRemark(List<RawData> _List_oRawData, TimingPolicy oTimingPolicy)
        {
            /*
             * Call only if lunch policy is applied
             * 
             */

            /*
             * Find Punch Time
             */
            //   Utility._returnTimSpan
            List<RawData> _PunchsBeforeLunch = _List_oRawData
                                                .Where(e => e.SwipeTime < oTimingPolicy.LunchEndTime
                                                && e.SwipeTime > oTimingPolicy.InTimeSpan).ToList();
            List<RawData> _PunchsAfterLunch = _List_oRawData
                                                .Where(e => e.SwipeTime > oTimingPolicy.LunchEndTime
                                                && e.SwipeTime < oTimingPolicy.GraceEarlyAction).ToList();

            if (_PunchsBeforeLunch.Count() == 0 && _PunchsAfterLunch.Count() == 0)
            {
                /*
                 * No Lunch time found
                 */
                return "NL";
            }

            if (_PunchsBeforeLunch.Count() == 0)
            {
                /*
                 * Lunch in time not found
                 */
                return "LI?";
            }
            if (_PunchsAfterLunch.Count() == 0)
            {
                /*
                 * Lunch out time not found
                 */
                return "LO?";
            }

            TimeSpan _LunchIn = Utility._returnTimeSpan(_PunchsBeforeLunch.Max(e => e.SwipeTime));
            TimeSpan _LunchOut = Utility._returnTimeSpan(_PunchsAfterLunch.Max(e => e.SwipeTime));

            TimeSpan _LunchStartTime = Utility._returnTimeSpan(oTimingPolicy.LunchStartTime) - Utility._returnTimeSpan(oTimingPolicy.GraceLunchEarly);
            TimeSpan _LunchEndTime = Utility._returnTimeSpan(oTimingPolicy.LunchEndTime) + Utility._returnTimeSpan(oTimingPolicy.GraceLunchLate);

            if (_LunchIn < _LunchStartTime)
            {
                return "LE";
            }
            else if (_LunchOut > _LunchEndTime)
            {
                return "LL";
            }
            return "**";

        }
        public static String CalculateTimePolicy(TimingPolicy oTimingPolicy, TimeSpan _Time_In, TimeSpan _Time_Out)
        {
            var workingHR = _Time_Out - _Time_In;
            if (workingHR.TotalHours >= oTimingPolicy.FlexiDailyHours)
            {
                return "PP";
            }
            else if (oTimingPolicy.FlexiAction.LookupVal.ToUpper() == "HALFDAY")
            {
                return "LH";
            }
            else if (oTimingPolicy.FlexiAction.LookupVal.ToUpper() == "NOACTION")
            {
                return "PP";
            }
            else if (oTimingPolicy.FlexiAction.LookupVal.ToUpper() == "FULLDAY")
            {
                return "LF";
            }
            else
            {
                return "PP";
            }

        }
        public static string Find_Remark(List<RawData> _List_oRawData,
                                        TimingPolicy oTimingPolicy, ProcessedData oProcessedData, bool ODReject, ProcessedData oProcessedDataReject, DateTime? _Date = null, Int32? _EmpId = 0)
        {
            string _Remark = String.Empty;

            if (_List_oRawData.Count > 0)
            {
                TimingPolicy _oTimingPolicy = oTimingPolicy;
                TimeSpan _Time_In, _Time_Out;
                TimeSpan[] _In_Time_Out_Time = new TimeSpan[2];
                bool OverNightPolicy = false;
                if (_oTimingPolicy.InTime.Value.Hour >= 12 && _oTimingPolicy.OutTime.Value.Hour <= 12)
                {
                    //overnight
                    OverNightPolicy = true;
                    _In_Time_Out_Time = _returnTimeInOutTimeForOverNightShift(_List_oRawData, _Date, _EmpId);
                    if (_In_Time_Out_Time[0].Hours == 0 && _In_Time_Out_Time[1].Hours == 0)
                    {
                        _Remark = "**";
                    }
                }
                else
                {
                    _In_Time_Out_Time = _returnTimeInOutTime(_List_oRawData);
                }
                _Time_In = _In_Time_Out_Time[0];
                _Time_Out = _In_Time_Out_Time[1];

                if (oTimingPolicy.TimingType != null && oTimingPolicy.TimingType.LookupVal.ToUpper() == "FLEXIBLE")
                {
                    if (ODReject == true)
                    {
                        oProcessedDataReject.InLateTime = oProcessedDataReject.InEarlyTime = oProcessedDataReject.InTime = Convert.ToDateTime(_Time_In.ToString());
                        oProcessedDataReject.OutEarlyTime = oProcessedDataReject.OutLateTime = oProcessedDataReject.OutTime = Convert.ToDateTime(_Time_Out.ToString());
                        return CalculateTimePolicy(oTimingPolicy, _Time_In, _Time_Out);
                    }
                    else
                    {
                        oProcessedData.InLateTime = oProcessedData.InEarlyTime = oProcessedData.InTime = Convert.ToDateTime(_Time_In.ToString());
                        oProcessedData.OutEarlyTime = oProcessedData.OutLateTime = oProcessedData.OutTime = Convert.ToDateTime(_Time_Out.ToString());
                        return CalculateTimePolicy(oTimingPolicy, _Time_In, _Time_Out);
                    }
                }
                /*
                 * Call Lunch Policy
                 */
                // Lunch Policy Not Include in Process As Told By Prashant Sir
                //if (oTimingPolicy.LunchStartTime != null && oTimingPolicy.LunchEndTime != null)
                //{
                //    string _LunchRemark = _returnLunchRemark(_List_oRawData, oTimingPolicy);
                //    if (_LunchRemark != "NL")
                //    {
                //        oProcessedData.InTime = Convert.ToDateTime(_Time_In.ToString());
                //        oProcessedData.OutTime = Convert.ToDateTime(_Time_Out.ToString());
                //        return _LunchRemark;
                //    }
                //}
                TimeSpan _InTimeSpan = Utility._returnTimeSpan(_oTimingPolicy.InTimeSpan.Value);
                TimeSpan _OutTimeSpanTime = Utility._returnTimeSpan(_oTimingPolicy.OutTimeSpanTime.Value);
                TimeSpan _GraceNoAction = Utility._returnTimeSpan(_oTimingPolicy.GraceNoAction.Value);
                TimeSpan _GraceLateAction = Utility._returnTimeSpan(_oTimingPolicy.GraceLateAction.Value);
                TimeSpan _GraceEarlyAction = Utility._returnTimeSpan(_oTimingPolicy.GraceEarlyAction.Value);

                if (
                    ((TimeSpan.Compare(_Time_In, _Time_Out) != 0 || TimeSpan.Compare(_Time_In, _Time_Out) < 0)
                    &&
                    Utility._returnTimeSpan(_oTimingPolicy.MissingEntryMarker.Value) <= _Time_Out &&
                    Utility._returnTimeSpan(_oTimingPolicy.MissingEntryMarker.Value) >= _Time_In) ||
                    ((OverNightPolicy && TimeSpan.Compare(_Time_In, _Time_Out) != 0 || TimeSpan.Compare(_Time_In, _Time_Out) < 0))
                    )
                {
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                            (
                            ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.InTimeStart.Value)) <= _Time_In) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) >= _Time_In))
                            &&
                            (((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value)) <= _Time_Out) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) + _OutTimeSpanTime) >= _Time_Out))
                            )
                        {
                            //PP
                            _Remark = "PP";
                        }
                    }

                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                          (((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) <= _Time_In) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) + _GraceLateAction >= _Time_In))
                            &&
                            (((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.GraceEarlyAction.Value)) <= _Time_Out) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) + _OutTimeSpanTime) >= _Time_Out))
                            )
                        {
                            //In late-policy 
                            _Remark = "IL";
                        }
                    }
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                          ((((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.InTimeStart.Value)) <= _Time_In) &&
                          ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) >= _Time_In))
                          &&
                          ((_Time_Out >= Utility._returnTimeSpan(_oTimingPolicy.LunchStartTime.Value)) &&
                          (_Time_Out <= (Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.GraceEarlyAction.Value)))))
                          )
                        {
                            //First Half
                            _Remark = "FH";
                        }

                    }
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                            ((Utility._returnTimeSpan(_oTimingPolicy.LunchStartTime.Value) <= _Time_In) &&
                            (Utility._returnTimeSpan(_oTimingPolicy.LunchEndTime.Value) >= _Time_In)) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) <= _Time_Out) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) + _OutTimeSpanTime) >= _Time_Out))
                            ||
                            (
                            ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) <= _Time_In) &&
                            (((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - _GraceEarlyAction) <= _Time_Out) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) + _OutTimeSpanTime) >= _Time_Out))
                            )
                            )
                        {
                            //Second Half
                            _Remark = "SH";
                        }
                    }
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                        (((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.InTimeStart.Value)) <= _Time_In) &&
                        ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) >= _Time_In))
                        &&
                        (_Time_Out >= (Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - _GraceEarlyAction)) &&
                        (_Time_Out <= (Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value)))
                        )
                        {
                            //Out Early
                            _Remark = "OE";
                        }
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (Utility._returnTimeSpan(_oTimingPolicy.MissingEntryMarker.Value) >= _Time_In)
                        {
                            //out time missing
                            _Remark = "O?";
                        }
                    }
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                            Utility._returnTimeSpan(_oTimingPolicy.MissingEntryMarker.Value) <= _Time_In
                            )
                        {
                            //in time missing
                            _Remark = "I?";

                        }
                    }

                }
                if (_List_oRawData.Any(e => (Utility._returnTimeSpan(e.SwipeTime) == _Time_In) && e.InputType != null && e.GeoFencingVerify == false) == true)
                {

                    if (_List_oRawData.Any(e => (Utility._returnTimeSpan(e.SwipeTime) == _Time_In) && e.InputType.LookupVal.ToUpper().ToString() == "MOBILE" && e.GeoFencingVerify == false) == true)
                    {
                        _Remark = "*L";
                    }
                }
                if (_List_oRawData.Any(e => Utility._returnTimeSpan(e.SwipeTime) == _Time_Out && e.InputType != null && e.GeoFencingVerify == false) == true)
                {

                    if (_List_oRawData.Any(e => Utility._returnTimeSpan(e.SwipeTime) == _Time_Out && e.InputType.LookupVal.ToUpper().ToString() == "MOBILE" && e.GeoFencingVerify == false) == true)
                    {
                        _Remark = "*L";
                    }
                }
                if (String.IsNullOrEmpty(_Remark))
                {
                    _Remark = "**";
                }
                if (ODReject == true)
                {
                    oProcessedDataReject.InLateTime = oProcessedDataReject.InEarlyTime = oProcessedDataReject.InTime = Convert.ToDateTime(_Time_In.ToString());
                    oProcessedDataReject.OutEarlyTime = oProcessedDataReject.OutLateTime = oProcessedDataReject.OutTime = Convert.ToDateTime(_Time_Out.ToString());

                    oProcessedDataReject.InTime = Convert.ToDateTime(_Time_In.ToString());
                    oProcessedDataReject.OutTime = Convert.ToDateTime(_Time_Out.ToString());
                }
                else
                {

                    oProcessedData.InLateTime = oProcessedData.InEarlyTime = oProcessedData.InTime = Convert.ToDateTime(_Time_In.ToString());
                    oProcessedData.OutEarlyTime = oProcessedData.OutLateTime = oProcessedData.OutTime = Convert.ToDateTime(_Time_Out.ToString());

                    oProcessedData.InTime = Convert.ToDateTime(_Time_In.ToString());
                    oProcessedData.OutTime = Convert.ToDateTime(_Time_Out.ToString());
                }

            }
            else
            {
                //check its leave apply or not
                //absent
                _Remark = "UA";

            }
            return _Remark;
        }
        public static remark_return_class Reamrk_Analysis(Int32 _CompId, Int32 _EmpId, DateTime _Date, DateTime _PayMonth, List<RawData> _List_oRawData,
                                                            TimingPolicy _oTimingPolicy,
                                                            ProcessedData _oProcessedData,
                                                            List<ProcessedData> _PrevProcessedData, bool _ODReject, ProcessedData _oProcessedDataReject)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string _oRemark = Find_Remark(_List_oRawData, _oTimingPolicy, _oProcessedData, _ODReject, _oProcessedDataReject, _Date, _EmpId);
                int _oLateCount = 0;
                int _oEarlyCount = 0;

                EmployeeLeave OEmployeeLeaveChk = null;
                OEmployeeLeaveChk = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal).Include(e => e.LvNewReq.Select(c => c.WFStatus))
                .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();

                List<LvNewReq> _LV_Exitschk = OEmployeeLeaveChk.LvNewReq
                                  .Where(e => e.IsCancel == false
                                      && e.TrReject == false
                                      && e.FromDate <= _Date.Date
                                      && e.ToDate >= _Date.Date
                                      && e.WFStatus.LookupVal != "2")
                    //&& e.LeaveCalendar.Id == lvyr.Id)
                    .ToList();



                if (_oRemark == "UA" || _LV_Exitschk.Count() > 0)
                {
                    //   List<EmployeeLeave> _LV_Exits = new List<EmployeeLeave>();
                    //_LV_Exits = db.CompanyLeave
                    //    .Include(z => z.EmployeeLeave)
                    //    .Include(z => z.EmployeeLeave.Select(x => x.LvNewReq))
                    //    .Where(e => e.Company.Id == _CompId
                    //        && e.EmployeeLeave.Select(c => c.LvNewReq) != null
                    //        && e.EmployeeLeave.Select(c => c.LvNewReq).Count() > 0
                    //        && e.EmployeeLeave.Any(a => a.Employee.Id == _EmpId)).SelectMany(a => a.EmployeeLeave
                    //            .Where(z => z.LvNewReq.Any(x => (x.LeaveCalendar.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && x.LeaveCalendar.Default == true) &&
                    //                (x.IsCancel == false && x.TrReject == false && DbFunctions.TruncateTime(x.FromDate) <= _Date.Date && DbFunctions.TruncateTime(x.ToDate) >= _Date.Date)))).AsParallel().ToList();
                    Calendar lvyr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    EmployeeLeave OEmployeeLeave = null;
                    OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal).Include(e => e.LvNewReq.Select(c => c.WFStatus))
                    .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();

                    List<LvNewReq> _LV_Exits = OEmployeeLeave.LvNewReq
                                      .Where(e => e.IsCancel == false
                                          && e.TrReject == false
                                          && e.FromDate <= _Date.Date
                                          && e.ToDate >= _Date.Date
                                          && e.WFStatus.LookupVal != "2")
                        //&& e.LeaveCalendar.Id == lvyr.Id)
                        .ToList();

                    if (_LV_Exits.Count > 0)//(_LV_Exits.Count() > 0)
                    {
                        //_LV_Exits = db.CompanyLeave
                        // .Include(z => z.EmployeeLeave)
                        // .Where(e => e.Company.Id == _CompId &&
                        //     e.EmployeeLeave.Any(a => a.Employee.Id == _EmpId)).SelectMany(a => a.EmployeeLeave
                        //         .Where(z => z.LvNewReq.Any(x => (x.LeaveCalendar.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && x.LeaveCalendar.Default == true) &&
                        //             (x.IsCancel == false && x.TrReject == false && DbFunctions.TruncateTime(x.FromDate) <= _Date.Date && DbFunctions.TruncateTime(x.ToDate) >= _Date.Date) &&
                        //             (x.LeaveHead.LvCode.ToUpper() == "LWP")))).AsParallel().ToList();
                        //Calendar lvyrlwp = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                        //EmployeeLeave OEmployeeLeavelwp = null;
                        //OEmployeeLeavelwp = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal).Include(e => e.LvNewReq.Select(c => c.WFStatus))
                        //.Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();

                        List<LvNewReq> _LV_Exitslwp = OEmployeeLeave.LvNewReq
                            .Where(e => e.IsCancel == false
                                && e.LeaveHead.LvCode.ToUpper() == "LWP"
                                && e.TrReject == false
                                && e.FromDate <= _Date.Date
                                && e.ToDate >= _Date.Date
                                && e.WFStatus.LookupVal != "2")
                            //&& e.LeaveCalendar.Id == lvyr.Id)
                            .ToList();

                        if (_LV_Exitslwp.Count > 0)
                        {
                            _oRemark = "AA";
                        }
                        else
                        {
                            _oRemark = "LV";
                        }
                    }
                    else
                    {
                        _oRemark = "UA";
                    }
                }
                else if (_oRemark == "IL")
                {
                    TimingPolicy _TimingPolicy = _oTimingPolicy;
                    if (_oTimingPolicy.IsLateCountInit)
                    {
                        EmployeeAttendance _Emp_Histry = db.EmployeeAttendance.Include(a => a.ProcessedData)
                            .Where(a => a.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();
                        if (_Emp_Histry == null)
                        {
                            _oRemark = "IL";
                        }
                        List<ProcessedData> _Histry = _Emp_Histry.ProcessedData.Count > 0 ?
                            _Emp_Histry.ProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList() :
                            _PrevProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList();


                        int _LateCount = _Histry.Where(e => e.MusterRemarks.LookupVal == "LH" ||
                               e.MusterRemarks.LookupVal == "LF" ||
                               e.MusterRemarks.LookupVal == "IL").Select(a => a.LateCount).LastOrDefault();

                        string _LateAction = _oTimingPolicy.LateAction.LookupVal.ToUpper();

                        if (_Histry != null)
                        {
                            if (_LateCount >= _oTimingPolicy.LateCount)
                            {
                                if (_LateAction == "FULLDAY")
                                {
                                    _oRemark = "LF";
                                    _oLateCount = _LateCount + 1;
                                }
                                else if (_LateAction == "HALFDAY")
                                {
                                    _oRemark = "LH";
                                    _oLateCount = _LateCount + 1;
                                }
                                else if (_LateAction == "NOACTION")
                                {
                                    _oRemark = "IL";
                                    _oLateCount = _LateCount + 1;
                                }
                            }
                            else
                            {
                                _oRemark = "IL";
                                _oLateCount = _LateCount + 1;
                            }
                        }
                        else
                        {
                            _oRemark = "IL";
                            _oLateCount = _LateCount + 1;
                        }
                    }
                    else
                    {
                        _oRemark = "IL";
                    }
                }
                else if (_oRemark == "OE")
                {
                    TimingPolicy _TimingPolicy = _oTimingPolicy;

                    if (_oTimingPolicy.IsEarlyCountInit)
                    {
                        EmployeeAttendance _Emp_Histry = db.EmployeeAttendance.Include(a => a.ProcessedData)
                            .Where(a => a.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();
                        if (_Emp_Histry == null)
                        {
                            _oRemark = "OE";
                        }
                        List<ProcessedData> _Histry = _Emp_Histry.ProcessedData.Count > 0 ?
                            _Emp_Histry.ProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList() :
                            _PrevProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList();

                        int _EarlyCount = _Histry.Where(e => e.MusterRemarks.LookupVal == "LH" ||
                               e.MusterRemarks.LookupVal == "LF" ||
                               e.MusterRemarks.LookupVal == "OE").Select(a => a.EarlyCount).LastOrDefault();

                        string _EarlyAction = _oTimingPolicy.EarlyAction.LookupVal.ToUpper();

                        if (_Histry != null)
                        {
                            if (_EarlyCount >= _oTimingPolicy.EarlyCount)
                            {
                                if (_EarlyAction == "FULLDAY")
                                {
                                    _oRemark = "LF";
                                    _oEarlyCount = _EarlyCount + 1;
                                }
                                else if (_EarlyAction == "HALFDAY")
                                {
                                    _oRemark = "LH";
                                    _oEarlyCount = _EarlyCount + 1;
                                }
                                else if (_EarlyAction == "NOACTION")
                                {
                                    _oRemark = "OE";
                                    _oEarlyCount = _EarlyCount + 1;
                                }
                            }
                            else
                            {
                                _oRemark = "OE";
                                _oEarlyCount = _EarlyCount + 1;
                            }
                        }
                        else
                        {
                            _oRemark = "OE";
                            _oEarlyCount = _EarlyCount + 1;
                        }
                    }
                    else
                    {
                        _oRemark = "OE";
                    }
                }
                else if (_oRemark == "LE")
                {
                    EmployeeAttendance _Emp_Histry = db.EmployeeAttendance.Include(a => a.ProcessedData)
                        .Where(a => a.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();
                    if (_Emp_Histry == null)
                    {
                        _oRemark = "LE";
                    }
                    List<ProcessedData> _Histry = _Emp_Histry.ProcessedData.Count > 0 ?
                        _Emp_Histry.ProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList() :
                        _PrevProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList();

                    int _LateCount = _Histry.Where(e =>
                        e.MusterRemarks.LookupVal == "LE" ||
                        e.MusterRemarks.LookupVal == "LNF" ||
                        e.MusterRemarks.LookupVal == "LNH"
                        ).Select(a => a.LateCount).LastOrDefault();

                    string _LateAction = _oTimingPolicy.LunchEarlyAction.LookupVal.ToUpper();

                    if (_Histry != null)
                    {
                        if (_LateCount >= _oTimingPolicy.GraceLunchEarlyCount)//make it as GraceLunchEarlyCount
                        {
                            if (_LateAction == "FULLDAY")
                            {
                                _oRemark = "LNF";
                                _oLateCount = _LateCount + 1;
                            }
                            else if (_LateAction == "HALFDAY")
                            {
                                _oRemark = "LNH";
                                _oLateCount = _LateCount + 1;
                            }
                            else if (_LateAction == "NOACTION")
                            {
                                _oRemark = "LE";
                                _oLateCount = _LateCount + 1;
                            }
                        }
                        else
                        {
                            _oRemark = "LE";
                            _oLateCount = _LateCount + 1;
                        }
                    }
                    else
                    {
                        _oRemark = "LE";
                        _oLateCount = _LateCount + 1;
                    }
                }
                else if (_oRemark == "LL")
                {
                    EmployeeAttendance _Emp_Histry = db.EmployeeAttendance.Include(a => a.ProcessedData)
                       .Where(a => a.Employee.Id == _EmpId).AsNoTracking().SingleOrDefault();
                    if (_Emp_Histry == null)
                    {
                        _oRemark = "LL";
                    }
                    List<ProcessedData> _Histry = _Emp_Histry.ProcessedData.Count > 0 ?
                        _Emp_Histry.ProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList() :
                        _PrevProcessedData.Where(b => b.SwipeDate.Value.Date >= _PayMonth.Date && b.SwipeDate.Value.Date <= _Date.Date).ToList();

                    int _LateCount = _Histry.Where(e =>
                        e.MusterRemarks.LookupVal == "LE" ||
                        e.MusterRemarks.LookupVal == "LNF" ||
                        e.MusterRemarks.LookupVal == "LNH")
                        .Select(a => a.LateCount).LastOrDefault();

                    string _LateAction = _oTimingPolicy.LunchEarlyAction.LookupVal.ToUpper();

                    if (_Histry != null)
                    {
                        if (_LateCount >= _oTimingPolicy.GraceLunchLateCount)//make it as GraceLunchLateCount
                        {
                            if (_LateAction == "FULLDAY")
                            {
                                _oRemark = "LNF";
                                _oLateCount = _LateCount + 1;
                            }
                            else if (_LateAction == "HALFDAY")
                            {
                                _oRemark = "LNH";
                                _oLateCount = _LateCount + 1;
                            }
                            else if (_LateAction == "NOACTION")
                            {
                                _oRemark = "LE";
                                _oLateCount = _LateCount + 1;
                            }
                        }
                        else
                        {
                            _oRemark = "LL";
                            _oLateCount = _LateCount + 1;
                        }
                    }
                    else
                    {
                        _oRemark = "LL";
                        _oLateCount = _LateCount + 1;
                    }
                }
                return new remark_return_class
                {
                    _EarlyCount = _oEarlyCount,
                    _LateCount = _oLateCount,
                    _Remark = _oRemark
                };
            }
        }
        public static TimingPolicy Find_TimingPolicy(Int32 EmpId, List<RawData> oRawData, TimingGroup oTimingGroups)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TimingPolicy oTimingPolicy = new TimingPolicy();
                TimeSpan[] _In_Time_Out_Time = _returnTimeInOutTime(oRawData);
                TimeSpan InTime = _In_Time_Out_Time[0];
                TimeSpan OutTime = _In_Time_Out_Time[1];
                //change the logic
                foreach (TimingPolicy _oTimingPolicy in oTimingGroups.TimingPolicy)
                {
                    if (
                        ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.InTimeStart.Value)) <= InTime) &&
                        (Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + Utility._returnTimeSpan(_oTimingPolicy.InTimeSpan) >= InTime)
                        ||
                        ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.GraceEarlyAction) <= OutTime) &&
                        ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) + Utility._returnTimeSpan(_oTimingPolicy.OutTimeSpanTime) >= OutTime)))
                        )
                    {
                        oTimingPolicy = _oTimingPolicy;
                        break;
                    }
                    else
                    {
                        // if emp in and out time not match Policy then hard code time code UA will pickup. note- if not define time UA then record not display in table i.e. date
                        TimingPolicy _TimingPolicy = null;
                        if (oRawData.Count() > 0)
                        {
                            DateTime Lastdate = oRawData.Max(x => x.SwipeDate).Value;
                            Lastdate = Lastdate.AddDays(-1);
                            EmployeeAttendance employeeData = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                            EmployeeAttendance employeeProcessData = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location)
                                .Include(e => e.ProcessedData)
                                .Include(e => e.ProcessedData.Select(x => x.TimingCode))
                                .Where(e => e.Id == employeeData.Id).SingleOrDefault();
                            ProcessedData empprocessr = employeeProcessData.ProcessedData.Where(e => e.SwipeDate == Lastdate).FirstOrDefault();

                            if (empprocessr != null)
                            {
                                _TimingPolicy = empprocessr.TimingCode;
                            }
                            else
                            {
                                _TimingPolicy = oTimingGroups.TimingPolicy.FirstOrDefault();
                            }
                        }
                        else
                        {
                            _TimingPolicy = oTimingGroups.TimingPolicy.FirstOrDefault();
                        }
                        //  TimingPolicy _TimingPolicy = db.TimingPolicy.Where(e => e.TimingCode == "UA").SingleOrDefault();
                        if (_TimingPolicy != null)
                        {
                            oTimingPolicy = _TimingPolicy;
                            break;
                        }
                        else
                        {
                            oTimingPolicy = null;

                        }
                    }
                }

                return oTimingPolicy;
            }
        }
        public static List<string> Genrate_Attendance(Int32 _CompId, List<int> _EmpIds, DateTime _PeriodFrom, DateTime _PeriodTo)
        {
            List<string> Msg = new List<string>();
            foreach (int EmpId in _EmpIds)
            {
                if (_PeriodFrom != null)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 30, 0)))
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            //DateTime _PayMonth = Convert.ToDateTime("01/" + _PayMonth_p);
                            //DateTime _LastDate = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                            DateTime _PayMonth = _PeriodFrom.Date;
                            DateTime _LastDate = _PeriodTo.Date;

                            string EmpCode = db.Employee.Find(EmpId).EmpCode;

                            List<ProcessedData> _Emp_Process_Data = db.ProcessedData
                                                                 .Where(a => a.EmployeeAttendance.Employee.Id == EmpId &&
                                                                      DbFunctions.TruncateTime(a.SwipeDate) >= _PayMonth &&
                                                                         DbFunctions.TruncateTime(a.SwipeDate) <= _LastDate && a.IsLocked == false)
                                                                    .ToList();

                            if (_Emp_Process_Data.Count > 0)
                            {



                                EmployeeAttendance EmployeeAttendance_del = db.EmployeeAttendance.Include(a => a.ProcessedData)
                                    .Include(a => a.ProcessedData.Select(x => x.MusterRemarks))
                                                                 .Where(a => a.Employee.Id == EmpId).SingleOrDefault();


                                List<int> employeeODData = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location)
                  .Include(e => e.OutDoorDutyReq)
                   .Include(e => e.OutDoorDutyReq.Select(a => a.WFStatus))
                  .Where(e => e.Employee.Id == EmpId).SingleOrDefault()
                  .OutDoorDutyReq.Where(e => e.EmployeeAttendance.Id == EmployeeAttendance_del.Id && e.ProcessedData.SwipeDate >= _PayMonth && e.ProcessedData.SwipeDate <= _LastDate && e.ProcessedData.IsLocked == false).Select(t => t.ProcessedData.Id).ToList();


                                List<ProcessedData> _Emp_Process_Data_Del = EmployeeAttendance_del.ProcessedData
                                    .Where(e => e.SwipeDate != null && e.SwipeDate.Value.Date >= _PayMonth && e.SwipeDate.Value.Date <= _LastDate && e.IsLocked == false && !employeeODData.Contains(e.Id)).ToList();
                                db.ProcessedData.RemoveRange(_Emp_Process_Data_Del);
                                db.SaveChanges();


                            }
                            //-----------------Find Structure Created Or Not-------------------------
                            int AttId = db.EmployeeAttendance.Where(e => e.Employee.Id == EmpId).Select(x => x.Id).FirstOrDefault();


                            List<EmpTimingMonthlyRoaster> _Extance_EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster
                                .Include(a => a.DayType)
                                .Include(a => a.TimingPolicy)
                                .Include(a => a.TimingPolicy.EarlyAction)
                                .Include(a => a.TimingPolicy.LateAction)
                                .Include(a => a.TimingPolicy.LunchEarlyAction)
                                .Include(a => a.TimingPolicy.GraceLunchLateAction)
                                .Include(a => a.TimingPolicy.FlexiAction)
                                .Include(a => a.TimingPolicy.TimingType)
                                .Where(e => e.EmployeeAttendance.Id == AttId && (e.RoasterDate.Value) >= _PayMonth && (e.RoasterDate.Value) <= _LastDate).AsNoTracking()
                                //.Include(a => a.TimingMonthlyRoaster)
                                //.Include(a => a.TimingMonthlyRoaster.TimingGroup)
                                //.Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy)
                                //.Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.LunchEarlyAction))
                                //.Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.GraceLunchLateAction))
                                //.Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.LateAction))
                                //.Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.EarlyAction))
                                //.Where(e => e.EmployeeAttendance.Id == AttId && DbFunctions.TruncateTime(e.RoasterDate) >= _PayMonth && DbFunctions.TruncateTime(e.RoasterDate) <= _LastDate).AsNoTracking().ToList();
                               .OrderBy(e => e.RoasterDate).ToList();


                            if (_Extance_EmpTimingMonthlyRoaster.Count() > 0)
                            {
                                // continue;
                                List<ProcessedData> _oProcessedData = new List<ProcessedData>();
                                List<ProcessedData> _oProcessedDataReject = new List<ProcessedData>();
                                //EmployeeAttendance _Emp_Row_Data = db.EmployeeAttendance.Include(a => a.RawData)
                                //           .Where(a => a.Employee.Id == EmpId &&
                                //               a.RawData.Any(e => DbFunctions.TruncateTime(e.SwipeDate) >= _PayMonth &&
                                //                   DbFunctions.TruncateTime(e.SwipeDate) <= _LastDate)).SingleOrDefault();
                                List<RawData> _Emp_Row_Data = db.RawData.Include(x => x.InputType)
                                        .Where(a => a.EmployeeAttendance.Id == AttId &&
                                                                      DbFunctions.TruncateTime(a.SwipeDate) >= _PayMonth &&
                                                                         DbFunctions.TruncateTime(a.SwipeDate) <= _LastDate
                                          ).AsNoTracking().ToList();

                                CompanyAttendance _Comp_Attendance = db.CompanyAttendance
                                                                   .Include(a => a.OrgTimingPolicyBatchAssignment)
                                                                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.Geostruct))
                                                                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.FuncStruct))
                                                                   .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment))
                                                                   .Where(a => a.Company.Id == _CompId)
                                                                   .AsNoTracking().FirstOrDefault();




                                if (_Emp_Row_Data.Count() >= 0)
                                {
                                    List<EmpReportingTimingStruct> oEmployeeAttendance = db.EmpReportingTimingStruct
                                       .Include(e => e.ReportingTimingStruct)
                                       .Include(e => e.EmployeeAttendance.Employee.GeoStruct)
                                       .Where(e => e.EmployeeAttendance.Id == AttId).AsNoTracking().OrderBy(e => e.Id).ToList();

                                    for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
                                    {
                                        EmpTimingMonthlyRoaster _EmpTimingMonthlyRoaster = _Extance_EmpTimingMonthlyRoaster
                                            .Where(e => e.RoasterDate == _Date).AsParallel().FirstOrDefault();


                                        String _DayType = "";
                                        TimingPolicy _Date_TimingPolicy = (TimingPolicy)null;
                                        string _Remark = "";
                                        //Attendance Lock check
                                        ProcessedData lockdatacheck = db.ProcessedData.Where(e => e.EmployeeAttendance.Id == AttId && e.SwipeDate == _Date && e.IsLocked == true).AsNoTracking().FirstOrDefault();
                                        if (lockdatacheck != null)
                                        {
                                            continue;
                                        }

                                        if (oEmployeeAttendance.Count() == 0)
                                        {
                                            break;
                                        }
                                        if (_EmpTimingMonthlyRoaster == null)
                                        {
                                            Msg.Add(EmpCode + " - Roster is not created for the date -" + _Date.ToString("dd/MM/yyyy"));
                                            continue;
                                        }


                                        int employeeODData = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location)
                                                                                   .Include(e => e.OutDoorDutyReq)
                                                                                   .Include(e => e.ProcessedData)
                                                                                    .Include(e => e.OutDoorDutyReq.Select(a => a.WFStatus))
                                                                                   .Where(e => e.Employee.Id == EmpId).FirstOrDefault()
                                                                                   .OutDoorDutyReq.Where(e => e.EmployeeAttendance.Id == AttId && e.ProcessedData.SwipeDate >= _Date && e.ProcessedData.SwipeDate <= _Date).Select(t => t.ProcessedData.Id).FirstOrDefault();// e.isCancel == false && e.TrReject == false && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "4"


                                        OutDoorDutyReq employeeODDataReq = db.EmployeeAttendance.Include(e => e.Employee.GeoStruct.Location)
                                         .Include(e => e.OutDoorDutyReq)
                                         .Include(e => e.ProcessedData)
                                          .Include(e => e.OutDoorDutyReq.Select(a => a.WFStatus))
                                         .Where(e => e.Employee.Id == EmpId).FirstOrDefault()
                                         .OutDoorDutyReq.Where(e => e.EmployeeAttendance.Id == AttId && e.ProcessedData.SwipeDate >= _Date && e.ProcessedData.SwipeDate <= _Date).FirstOrDefault();


                                        ProcessedData empprocessr = db.ProcessedData.Where(e => e.SwipeDate >= _Date && e.SwipeDate <= _Date && e.Id == employeeODData).FirstOrDefault();
                                        bool ODReject = false;

                                        if (empprocessr != null)
                                        {
                                            int mobid = 0;
                                            var lookid = db.Lookup.Where(e => e.Code == "5000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MOBILE").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MOBILE").FirstOrDefault();
                                            if (lookid != null)
                                            {
                                                mobid = lookid.Id;
                                            }
                                            RemarkConfig get_remark = db.RemarkConfig
                                                     .Include(e => e.AlterMusterRemark)
                                                     .Include(e => e.PresentStatus)
                                                     .Include(e => e.MusterRemarks)
                                                     .Where(e => e.MusterRemarks.LookupVal.ToUpper() == "M").FirstOrDefault();
                                            if (get_remark != null)
                                            {
                                                if ((employeeODDataReq.InputMethod == 0 && employeeODDataReq.isCancel == false) || (employeeODDataReq.InputMethod == 1 && employeeODDataReq.isCancel == false && employeeODDataReq.TrReject == false && employeeODDataReq.TrClosed == true) || (employeeODDataReq.InputMethod == mobid && employeeODDataReq.isCancel == false && employeeODDataReq.TrReject == false && employeeODDataReq.TrClosed == true))
                                                {
                                                    if (empprocessr.InTime == null && empprocessr.OutTime == null)
                                                    {
                                                        empprocessr.InTime = Convert.ToDateTime(employeeODDataReq.InTime.ToString());
                                                        empprocessr.OutTime = Convert.ToDateTime(employeeODDataReq.OutTime.ToString());
                                                    }
                                                    empprocessr.MInTime = Convert.ToDateTime(employeeODDataReq.InTime.ToString());
                                                    empprocessr.MOutTime = Convert.ToDateTime(employeeODDataReq.OutTime.ToString());
                                                    empprocessr.ManualReason = (employeeODDataReq.Reason.ToString());
                                                    empprocessr.MusterRemarks = get_remark.AlterMusterRemark;
                                                    empprocessr.PresentStatus = get_remark.PresentStatus;
                                                    db.ProcessedData.Attach(empprocessr);
                                                    db.Entry(empprocessr).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    continue;
                                                }
                                                else
                                                {
                                                    ODReject = true;

                                                }
                                                //Tempdata["RowVersion"] = empprocessr.RowVersion;
                                                //db.Entry(empprocessr).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }

                                        // manual remark M End 

                                        EmpReportingTimingStruct oEmpReportingTimingStruct = oEmployeeAttendance.Where(z => z.EndDate == null).SingleOrDefault();

                                        ReportingTimingStruct oReportingTimingStruct = oEmpReportingTimingStruct.ReportingTimingStruct;

                                        if (oReportingTimingStruct.IsTimeRoaster)
                                        {
                                            EmployeeAttendance _EmployeeAttendance = db.EmployeeAttendance
                                                .Include(e => e.EmpTimingRoasterData)
                                                .Include(e => e.EmpTimingRoasterData.Select(a => a.DayType))
                                                .Where(e => e.EmpTimingRoasterData.Count > 0 &&
                                                e.EmpTimingRoasterData.Any(a => a.RoasterDate == _Date)).FirstOrDefault();

                                            if (_EmployeeAttendance == null)
                                            {
                                                Msg.Add(EmpCode + " - Roster data not available");
                                                continue;
                                            }
                                            else
                                            {
                                                _Date_TimingPolicy = _EmpTimingMonthlyRoaster.TimingPolicy;


                                            }
                                        }
                                        else if (oReportingTimingStruct.IndividualAppl)
                                        {
                                            _Date_TimingPolicy = _EmpTimingMonthlyRoaster.TimingPolicy;
                                        }
                                        else if (oReportingTimingStruct.GeographicalAppl)
                                        {

                                            OrgTimingPolicyBatchAssignment _Comp_Attendance_policy = _Comp_Attendance.OrgTimingPolicyBatchAssignment
                                               .Where(a => (a.Geostruct != null && a.FuncStruct != null && a.Geostruct.Id == oEmpReportingTimingStruct.EmployeeAttendance.Employee.GeoStruct.Id && a.FuncStruct.Id == oEmpReportingTimingStruct.EmployeeAttendance.Employee.FuncStruct.Id)
                                                   || (a.Geostruct != null && a.FuncStruct == null && a.Geostruct.Id == oEmpReportingTimingStruct.EmployeeAttendance.Employee.GeoStruct.Id)).LastOrDefault();


                                            TimingPolicyBatchAssignment oTimingPolicyBatchAssignment = _Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault();
                                            // List<RawData> _Row_Data = _Emp_Row_Data.RawData.Where(e => e.SwipeDate.Value.Date == _Date).ToList();
                                            List<RawData> _Row_Data = _Emp_Row_Data.Where(e => e.SwipeDate.Value.Date == _Date).ToList();
                                            if (oTimingPolicyBatchAssignment.IsTimingGroup)
                                            {
                                                EmpTimingMonthlyRoaster _oTimingMonthlyRoster = db.EmpTimingMonthlyRoaster
                                              .Include(a => a.TimingMonthlyRoaster)
                                              .Include(a => a.TimingMonthlyRoaster.TimingGroup)
                                              .Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy)
                                              .Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.LunchEarlyAction))
                                              .Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.GraceLunchLateAction))
                                              .Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.LateAction))
                                              .Include(a => a.TimingMonthlyRoaster.TimingGroup.TimingPolicy.Select(z => z.EarlyAction))
                                               .Where(e => (e.RoasterDate.Value) == _Date && e.Id == _EmpTimingMonthlyRoaster.Id).AsNoTracking()

                                              .OrderBy(e => e.RoasterDate).FirstOrDefault();

                                                if (_oTimingMonthlyRoster.TimingMonthlyRoaster == null)
                                                {
                                                    _Date_TimingPolicy = _EmpTimingMonthlyRoaster.TimingPolicy;
                                                }
                                                else
                                                {


                                                    if (_Row_Data.Count >= 0)
                                                    {
                                                        _Date_TimingPolicy = Find_TimingPolicy(EmpId, _Row_Data, _oTimingMonthlyRoster.TimingMonthlyRoaster.TimingGroup);
                                                    }
                                                }
                                            }
                                            else if (oTimingPolicyBatchAssignment.IsWeeklyTimingSchedule)
                                            {
                                                _Date_TimingPolicy = _EmpTimingMonthlyRoaster.TimingPolicy;

                                            }
                                            else if (oTimingPolicyBatchAssignment.IsRoaster)
                                            {
                                                //roaster and weakly schedule
                                                _Date_TimingPolicy = _EmpTimingMonthlyRoaster.TimingPolicy;

                                            }

                                        }

                                        if (_Date_TimingPolicy != null)
                                        {

                                            EmpTimingMonthlyRoaster _oEmployeeAttendance = db.EmpTimingMonthlyRoaster
                                              .Include(a => a.DayType)
                                              .Include(a => a.TimingPolicy)
                                              .Include(a => a.TimingPolicy.LateAction)
                                              .Include(a => a.TimingPolicy.EarlyAction)
                                              .Where(a => a.EmployeeAttendance.Id == AttId && a.RoasterDate == _Date)
                                              .AsNoTracking().FirstOrDefault();


                                            ProcessedData oProcessedData = new ProcessedData();
                                            ProcessedData oProcessedDataReject = db.ProcessedData.Where(e => e.SwipeDate >= _Date && e.SwipeDate <= _Date && e.Id == employeeODData).FirstOrDefault();

                                            if (_oEmployeeAttendance != null)
                                            {


                                                if (_oEmployeeAttendance.DayType == null)
                                                {
                                                    EmployeeAttendance _fetch_emp_policy = db.EmployeeAttendance
                                                        .Include(a => a.Employee.GeoStruct)
                                                        .Include(a => a.Employee.GeoStruct.Location)
                                                        .Include(a => a.Employee.FuncStruct)
                                                        .Where(a => a.Employee.Id == EmpId)
                                                        .AsParallel().SingleOrDefault();

                                                    _DayType = Check_HWWO(Convert.ToInt32(SessionManager.CompanyId), _fetch_emp_policy, _Date);
                                                }
                                                else
                                                {
                                                    _DayType = _oEmployeeAttendance.DayType.LookupVal;
                                                }
                                                if (_oEmployeeAttendance != null && _DayType.ToUpper() == "WORKING" && _Emp_Row_Data != null)
                                                {

                                                    //Find how many record 4 that day
                                                    List<RawData> _Row_Data = _Emp_Row_Data.Where(e => e.SwipeDate.Value.Date == _Date).ToList();

                                                    remark_return_class _Reamrk_Analysis = Reamrk_Analysis(_CompId, EmpId, _Date, _PayMonth, _Row_Data, _Date_TimingPolicy,
                                                                                                            oProcessedData, _oProcessedData, ODReject, oProcessedDataReject);
                                                    _Remark = _Reamrk_Analysis._Remark;


                                                    if (ODReject == true)
                                                    {
                                                        oProcessedDataReject.LateCount = _Reamrk_Analysis._LateCount;
                                                        oProcessedDataReject.EarlyCount = _Reamrk_Analysis._EarlyCount;
                                                    }
                                                    else
                                                    {
                                                        oProcessedData.LateCount = _Reamrk_Analysis._LateCount;
                                                        oProcessedData.EarlyCount = _Reamrk_Analysis._EarlyCount;
                                                    }
                                                }
                                                else if (_EmpTimingMonthlyRoaster != null)
                                                {
                                                    if (_DayType.ToUpper() == "HOLIDAYOFF")
                                                    {
                                                        //modifcation due to HO remark in daytype as well as remark
                                                        _Remark = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "HO").FirstOrDefault().LookupVal; //db.LookupValue.Where(a => a.LookupVal.ToUpper() == "HO".ToUpper()).Select(e => e.LookupVal).SingleOrDefault();
                                                    }
                                                    else if (_DayType.ToUpper() == "WEEKLYOFF")
                                                    {
                                                        _Remark = db.Lookup.Where(e => e.Code == "611").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "WO").FirstOrDefault().LookupVal;//db.LookupValue.Where(a => a.LookupVal.ToUpper() == "WO").Select(e => e.LookupVal).SingleOrDefault();
                                                    }

                                                    //if (oProcessedData.InTime != null && oProcessedData.OutTime != null)
                                                    //{
                                                    List<RawData> _Row_Data = _Emp_Row_Data.Where(e => e.SwipeDate.Value.Date == _Date).ToList();
                                                    if (_Row_Data.Count > 0)
                                                    {
                                                        TimeSpan[] _In_Time_Out_Time = _returnTimeInOutTime(_Row_Data);
                                                        if (ODReject == true)
                                                        {
                                                            oProcessedDataReject.InTime = Convert.ToDateTime(_In_Time_Out_Time[0].ToString());
                                                            oProcessedDataReject.OutTime = Convert.ToDateTime(_In_Time_Out_Time[1].ToString());
                                                        }
                                                        else
                                                        {
                                                            oProcessedData.InTime = Convert.ToDateTime(_In_Time_Out_Time[0].ToString());
                                                            oProcessedData.OutTime = Convert.ToDateTime(_In_Time_Out_Time[1].ToString());
                                                        }

                                                    }
                                                    //}
                                                }
                                                if (_Remark != "")
                                                {
                                                    if (_Remark == null)
                                                        _Remark = "UA";

                                                    RemarkConfig get_remark = db.RemarkConfig
                                                          .Include(e => e.AlterMusterRemark)
                                                          .Include(e => e.PresentStatus)
                                                          .Include(e => e.MusterRemarks)
                                                          .Where(e => e.MusterRemarks.LookupVal.ToUpper() == _Remark).FirstOrDefault();

                                                    if (ODReject == true)
                                                    {
                                                        oProcessedDataReject.AttendProcessDate = DateTime.Now;
                                                        oProcessedDataReject.SwipeDate = _Date;
                                                        oProcessedDataReject.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true, ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                                        oProcessedDataReject.MusterRemarks = get_remark.AlterMusterRemark;
                                                        oProcessedDataReject.PresentStatus = get_remark.PresentStatus;
                                                    }
                                                    else
                                                    {
                                                        oProcessedData.AttendProcessDate = DateTime.Now;
                                                        oProcessedData.SwipeDate = _Date;
                                                        oProcessedData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                                        oProcessedData.MusterRemarks = get_remark.AlterMusterRemark;
                                                        oProcessedData.PresentStatus = get_remark.PresentStatus;

                                                    }
                                                    EmployeeAttendance EmployeeAttendance = db.EmployeeAttendance.Include(e => e.EmpTimingMonthlyRoaster)
                                                        .Where(e => e.Employee.Id == EmpId).SingleOrDefault();

                                                    EmpTimingMonthlyRoaster EmpTimingMonthlyRoaster = EmployeeAttendance.EmpTimingMonthlyRoaster
                                                        .Where(e => e.RoasterDate == _Date).SingleOrDefault();

                                                    if (EmpTimingMonthlyRoaster != null)
                                                    {
                                                        EmpTimingMonthlyRoaster.TimingPolicy = db.TimingPolicy.Where(e => e.Id == _Date_TimingPolicy.Id).FirstOrDefault();
                                                        db.Entry(EmpTimingMonthlyRoaster).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                    }

                                                    if (ODReject == true)
                                                    {
                                                        oProcessedDataReject.TimingCode = db.TimingPolicy.Where(e => e.Id == _Date_TimingPolicy.Id).FirstOrDefault();
                                                        if (oProcessedDataReject.InTime != null && oProcessedDataReject.OutTime != null)
                                                        {
                                                            DateTime a = Convert.ToDateTime(oProcessedDataReject.InTime);
                                                            DateTime b = Convert.ToDateTime(oProcessedDataReject.OutTime);
                                                            TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
                                                            oProcessedDataReject.TotWhLunch = Convert.ToDateTime(diffT.ToString());

                                                            DateTime ls = Convert.ToDateTime(oProcessedDataReject.TimingCode.LunchStartTime);
                                                            DateTime le = Convert.ToDateTime(oProcessedDataReject.TimingCode.LunchEndTime);
                                                            TimeSpan diffTLE = le.TimeOfDay - ls.TimeOfDay;

                                                            DateTime workwithlunch = Convert.ToDateTime(diffT.ToString());
                                                            DateTime workNOlunch = Convert.ToDateTime(diffTLE.ToString());
                                                            DateTime x = Convert.ToDateTime(workwithlunch);
                                                            DateTime y = Convert.ToDateTime(workNOlunch);

                                                            TimeSpan wNoLunchdiff = x.TimeOfDay - y.TimeOfDay;

                                                            DateTime swiptime = Convert.ToDateTime(oProcessedDataReject.SwipeDate);
                                                            TimeSpan swt = swiptime.TimeOfDay;

                                                            if (x.TimeOfDay > y.TimeOfDay)
                                                            {

                                                                oProcessedDataReject.TotWhNoLunch = Convert.ToDateTime(wNoLunchdiff.ToString());


                                                            }
                                                            else
                                                            {

                                                                oProcessedDataReject.TotWhNoLunch = Convert.ToDateTime(swt.ToString());

                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oProcessedData.TimingCode = db.TimingPolicy.Where(e => e.Id == _Date_TimingPolicy.Id).FirstOrDefault();
                                                        if (oProcessedData.InTime != null && oProcessedData.OutTime != null)
                                                        {
                                                            DateTime a = Convert.ToDateTime(oProcessedData.InTime);
                                                            DateTime b = Convert.ToDateTime(oProcessedData.OutTime);
                                                            TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
                                                            oProcessedData.TotWhLunch = Convert.ToDateTime(diffT.ToString());

                                                            DateTime ls = Convert.ToDateTime(oProcessedData.TimingCode.LunchStartTime);
                                                            DateTime le = Convert.ToDateTime(oProcessedData.TimingCode.LunchEndTime);
                                                            TimeSpan diffTLE = le.TimeOfDay - ls.TimeOfDay;

                                                            DateTime workwithlunch = Convert.ToDateTime(diffT.ToString());
                                                            DateTime workNOlunch = Convert.ToDateTime(diffTLE.ToString());
                                                            DateTime x = Convert.ToDateTime(workwithlunch);
                                                            DateTime y = Convert.ToDateTime(workNOlunch);

                                                            TimeSpan wNoLunchdiff = x.TimeOfDay - y.TimeOfDay;

                                                            DateTime swiptime = Convert.ToDateTime(oProcessedData.SwipeDate);
                                                            TimeSpan swt = swiptime.TimeOfDay;

                                                            if (x.TimeOfDay > y.TimeOfDay)
                                                            {

                                                                oProcessedData.TotWhNoLunch = Convert.ToDateTime(wNoLunchdiff.ToString());


                                                            }
                                                            else
                                                            {

                                                                oProcessedData.TotWhNoLunch = Convert.ToDateTime(swt.ToString());

                                                            }
                                                        }
                                                    }


                                                    //Timemonthlyroaster not null and roaster has group if employee will not punch  on holiday then time policy should not find and give error so code comment
                                                    if (ODReject == true)
                                                    {
                                                        if (oProcessedDataReject.InTime == null && oProcessedDataReject.OutTime == null && (_Remark == "WO" || _Remark == "HO" || _Remark == "LV"))
                                                        {
                                                            DateTime a = Convert.ToDateTime(oProcessedDataReject.TimingCode.InTime);
                                                            DateTime b = Convert.ToDateTime(oProcessedDataReject.TimingCode.OutTime);
                                                            TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
                                                            oProcessedDataReject.TotWhLunch = Convert.ToDateTime(diffT.ToString());

                                                            DateTime ls = Convert.ToDateTime(oProcessedDataReject.TimingCode.LunchStartTime);
                                                            DateTime le = Convert.ToDateTime(oProcessedDataReject.TimingCode.LunchEndTime);
                                                            TimeSpan diffTLE = le.TimeOfDay - ls.TimeOfDay;

                                                            DateTime workwithlunch = Convert.ToDateTime(diffT.ToString());
                                                            DateTime workNOlunch = Convert.ToDateTime(diffTLE.ToString());
                                                            DateTime x = Convert.ToDateTime(workwithlunch);
                                                            DateTime y = Convert.ToDateTime(workNOlunch);
                                                            TimeSpan wNoLunchdiff = x.TimeOfDay - y.TimeOfDay;

                                                            DateTime swiptime = Convert.ToDateTime(oProcessedDataReject.SwipeDate);
                                                            TimeSpan swt = swiptime.TimeOfDay;

                                                            if (x.TimeOfDay > y.TimeOfDay)
                                                            {
                                                                oProcessedDataReject.TotWhNoLunch = Convert.ToDateTime(wNoLunchdiff.ToString());


                                                            }
                                                            else
                                                            {
                                                                oProcessedDataReject.TotWhNoLunch = Convert.ToDateTime(swt.ToString());
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (oProcessedData.InTime == null && oProcessedData.OutTime == null && (_Remark == "WO" || _Remark == "HO" || _Remark == "LV"))
                                                        {
                                                            DateTime a = Convert.ToDateTime(oProcessedData.TimingCode.InTime);
                                                            DateTime b = Convert.ToDateTime(oProcessedData.TimingCode.OutTime);
                                                            TimeSpan diffT = b.TimeOfDay - a.TimeOfDay;
                                                            oProcessedData.TotWhLunch = Convert.ToDateTime(diffT.ToString());

                                                            DateTime ls = Convert.ToDateTime(oProcessedData.TimingCode.LunchStartTime);
                                                            DateTime le = Convert.ToDateTime(oProcessedData.TimingCode.LunchEndTime);
                                                            TimeSpan diffTLE = le.TimeOfDay - ls.TimeOfDay;

                                                            DateTime workwithlunch = Convert.ToDateTime(diffT.ToString());
                                                            DateTime workNOlunch = Convert.ToDateTime(diffTLE.ToString());
                                                            DateTime x = Convert.ToDateTime(workwithlunch);
                                                            DateTime y = Convert.ToDateTime(workNOlunch);
                                                            TimeSpan wNoLunchdiff = x.TimeOfDay - y.TimeOfDay;

                                                            DateTime swiptime = Convert.ToDateTime(oProcessedData.SwipeDate);
                                                            TimeSpan swt = swiptime.TimeOfDay;

                                                            if (x.TimeOfDay > y.TimeOfDay)
                                                            {
                                                                oProcessedData.TotWhNoLunch = Convert.ToDateTime(wNoLunchdiff.ToString());


                                                            }
                                                            else
                                                            {
                                                                oProcessedData.TotWhNoLunch = Convert.ToDateTime(swt.ToString());
                                                            }

                                                        }
                                                    }
                                                    //oProcessedData.MInTime = _Date_TimingPolicy.InTime;
                                                    //oProcessedData.MOutTime = _Date_TimingPolicy.OutTime;
                                                    if (ODReject == true)
                                                    {
                                                        db.ProcessedData.Attach(oProcessedDataReject);
                                                        db.Entry(oProcessedDataReject).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        _oProcessedData.Add(oProcessedData);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (_oProcessedData.Count > 0)
                                {
                                    EmployeeAttendance oEmployeeAttendance = db.EmployeeAttendance.Include(a => a.Employee)
                                        .Include(e => e.ProcessedData).Where(e => e.Employee.Id == EmpId).FirstOrDefault();
                                    if (oEmployeeAttendance.ProcessedData.Count() > 0)
                                    {
                                        _oProcessedData.AddRange(oEmployeeAttendance.ProcessedData);
                                    }
                                    oEmployeeAttendance.ProcessedData = _oProcessedData;
                                    db.EmployeeAttendance.Attach(oEmployeeAttendance);
                                    db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                            else
                            {
                                Msg.Add(EmpCode + "Roster not generated ");
                                continue;
                            }
                        }
                        ts.Complete();
                    }
                }
                else
                {
                    //paymonth undefine
                }
            }
            return Msg;
        }
        public static string Recovery_AttendanceData(Int32 _CompId, List<string> _CardCodes, DateTime _PeriodFrom, DateTime _PeriodTo, Int32 _ReCoveryType)
        {

            //  var _Last = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
            // var _Paymonth_string = _PayMonth.ToString("MM/dd/yyyy");
            //  var _Last_string = _Last.ToString("MM/dd/yyyy");
            //  _PayMonth = _PayMonth.Date;
            // _Last = _Last.Date;
            var _Paymonth_string = _PeriodFrom.ToString("MM/dd/yyyy");
            var _Last_string = _PeriodTo.ToString("MM/dd/yyyy");
            var _PayMonth = _PeriodFrom.Date;
            var _Last = _PeriodTo.Date;

            foreach (var CardCode in _CardCodes)
            {
                try
                {
                    LookupValue InputType = null;
                    MachineInterface oMachineInterface = new MachineInterface();
                    List<RawData> oList_RawData = new List<RawData>();
                    List<RawDataFailure> oList_RawDataFailure = new List<RawDataFailure>();
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            InputType = db.Lookup.Where(e => e.Code == "5000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "MACHINE").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal.ToUpper() == "MACHINE").FirstOrDefault();
                            List<RawData> _RawData = db.EmployeeAttendance.Where(e => e.Employee.CardCode == CardCode
                                && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last))
                                .SelectMany(a => a.RawData).ToList();
                            if (_RawData.Count > 0)
                            {
                                var _Check_ProcessData = db.EmployeeAttendance.Where(e => e.Employee.CardCode == CardCode &&
                                    e.ProcessedData.Any(w => DbFunctions.TruncateTime(w.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(w.SwipeDate) <= _Last)).ToList();

                                if (_Check_ProcessData.Count > 0)
                                {
                                    //Attendance generated For month
                                    var _EmployeeAttendance_del = db.EmployeeAttendance.Include(x => x.RawData)
                                        .Include(x => x.RawData.Select(e => e.InputType))
                                        .Where(e => e.Employee.CardCode == CardCode
                                && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last)).SingleOrDefault();

                                    if (_EmployeeAttendance_del != null)
                                    {

                                        var _RawDatadel = _EmployeeAttendance_del.RawData.Where(z => (z.SwipeDate) >= _PayMonth &&
                                         (z.SwipeDate) <= _Last && z.InputType.LookupVal.ToUpper() != "MOBILE").ToList();

                                        db.RawData.RemoveRange(_RawDatadel);
                                        db.SaveChanges();
                                    }
                                    var _EmployeeAttendance_delf = db.EmployeeAttendance.Include(e => e.RawDataFailure).Where(e => e.Employee.CardCode == CardCode
                              && e.RawDataFailure.Any(y => DbFunctions.TruncateTime(y.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(y.SwipeDate) <= _Last)).SingleOrDefault();
                                    if (_EmployeeAttendance_delf != null)
                                    {

                                        var _RawDatadelf = _EmployeeAttendance_delf.RawDataFailure.Where(y => (y.SwipeDate) >= _PayMonth &&
                                           (y.SwipeDate) <= _Last).ToList();
                                        db.RawDataFailure.RemoveRange(_RawDatadelf);
                                        db.SaveChanges();
                                    }

                                }
                                else
                                {
                                    var _EmployeeAttendance_del = db.EmployeeAttendance.Include(x => x.RawData)
                                                                            .Include(x => x.RawData.Select(e => e.InputType))
                                                                            .Where(e => e.Employee.CardCode == CardCode
                                                                    && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last)).SingleOrDefault();

                                    if (_EmployeeAttendance_del != null)
                                    {

                                        var _RawDatadel = _EmployeeAttendance_del.RawData.Where(z => (z.SwipeDate) >= _PayMonth &&
                                         (z.SwipeDate) <= _Last && z.InputType.LookupVal.ToUpper() != "MOBILE").ToList();

                                        db.RawData.RemoveRange(_RawDatadel);
                                        db.SaveChanges();
                                    }
                                    var _EmployeeAttendance_delf = db.EmployeeAttendance.Include(e => e.RawDataFailure).Where(e => e.Employee.CardCode == CardCode
                              && e.RawDataFailure.Any(y => DbFunctions.TruncateTime(y.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(y.SwipeDate) <= _Last)).SingleOrDefault();
                                    if (_EmployeeAttendance_delf != null)
                                    {

                                        var _RawDatadelf = _EmployeeAttendance_delf.RawDataFailure.Where(y => (y.SwipeDate) >= _PayMonth &&
                                           (y.SwipeDate) <= _Last).ToList();
                                        db.RawDataFailure.RemoveRange(_RawDatadelf);
                                        db.SaveChanges();
                                    }

                                }
                            }
                            var _CompanyAttendance = db.CompanyAttendance.Include(a => a.MachineInterface)
                                .Include(a => a.MachineInterface.Select(e => e.DatabaseType))
                                .Include(a => a.MachineInterface.Select(e => e.InterfaceName))
                                .Where(a => a.Company.Id == _CompId && a.MachineInterface.Any(z => z.DatabaseType.Id == _ReCoveryType)).SingleOrDefault();

                            if (_CompanyAttendance != null)
                            {
                                oMachineInterface = _CompanyAttendance.MachineInterface.Where(z => z.DatabaseType.Id == _ReCoveryType).SingleOrDefault();
                            }
                            else
                            {
                                continue;//  return;
                                //MachineInterface not define for recovery type
                            }

                            string _type = oMachineInterface.DatabaseType.LookupVal.ToUpper();
                            if (_type == "ACCESS")
                            {
                                string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;OLE DB Services = -4;Data Source=" + oMachineInterface.DatabaseName + "";
                                //  string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;OLE DB Services = -4;Data Source=D:\TimeData.mdb";
                                string strSQL =
                                    "Select * from " + oMachineInterface.TableName +
                                    " where " + oMachineInterface.CardCode + "='" + CardCode +
                                    "' and " + oMachineInterface.DateField + ">=" + "#" + _Paymonth_string + "#" +
                                    " and " + oMachineInterface.DateField + "<=" + "#" + _Last_string + "#" +
                                    " order by " + oMachineInterface.InTimeField;

                                using (OleDbConnection connection = new OleDbConnection(connectionString))
                                {
                                    OleDbCommand command = new OleDbCommand(strSQL, connection);
                                    connection.Open();
                                    command.CommandTimeout = 60;
                                    using (OleDbDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            if (oMachineInterface.CardCode != null)
                                            {
                                                if (oMachineInterface.InterfaceName.LookupVal.ToUpper() == "SMARTTECH")
                                                {

                                                    RawData oRawData = new RawData();
                                                    int fild = Convert.ToInt32(reader.GetValue(7)); //Status value
                                                    if (fild == 0)
                                                    {
                                                        // verify pass data
                                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                        {
                                                            oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                        }
                                                        //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        //{
                                                        //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        //}
                                                        if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                        {
                                                            oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                        }

                                                        if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                        {
                                                            oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                        }
                                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        {
                                                            // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                            oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                        }
                                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                        {
                                                            // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                            oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                        }
                                                        oRawData.InputType = InputType;
                                                        oRawData.DownloadDate = DateTime.Now;
                                                        oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        oList_RawData.Add(oRawData);
                                                    }
                                                    else
                                                    {
                                                        // verify faild data
                                                        RawDataFailure oRawDataFailure = new RawDataFailure();
                                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                        {
                                                            oRawDataFailure.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                        }
                                                        //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        //{
                                                        //    oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        //}
                                                        if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                        {
                                                            oRawDataFailure.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                        }

                                                        if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                        {
                                                            oRawDataFailure.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                        }
                                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                        {
                                                            // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                            oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                        }
                                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                        {
                                                            //oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                            oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                        }

                                                        oRawDataFailure.DownloadDate = DateTime.Now;
                                                        oRawDataFailure.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        oList_RawDataFailure.Add(oRawDataFailure);


                                                    }
                                                }
                                                else
                                                {


                                                    RawData oRawData = new RawData();
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                    {
                                                        oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                    {
                                                        oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                    }

                                                    if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                    {
                                                        oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                    }
                                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    {
                                                        //oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                    }
                                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                    {
                                                        // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                    }
                                                    oRawData.InputType = InputType;
                                                    oRawData.DownloadDate = DateTime.Now;
                                                    oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawData.Add(oRawData);
                                                }
                                            }
                                        }
                                        connection.Close();
                                    }
                                }

                            }
                            else if (_type == "SQL")
                            {
                                SqlConnection conn = new SqlConnection();
                                conn.ConnectionString =
                                    @"Data Source=" + oMachineInterface.ServerName +
                                    ";Initial Catalog=" + oMachineInterface.DatabaseName +
                                    ";User ID=" + oMachineInterface.UserId +
                                    ";Password=" + oMachineInterface.Password +
                                    ";Integrated Security=false;Enlist=False;";

                                //using (SqlConnection cnn = new SqlConnection(connetionString))
                                //{
                                // if client attendance database connection not open check P2b Db and client attendance DB collation and compability
                                //both database Should same if not same do same.Other checking on database machine controll pannel->component services->computer->my computer->distributed Transaction co ordinator-
                                // >Local DTC->right click properties->security-> Netwok DTC access,Allow Remote Client,Allow inbound,Allow outbound,Enable XA transaction,Enable SNA LU 6.2 Transaction all this
                                // check box tick and apply.after this activity may coonect attendance db for recovery.(this activity has done on KDCC)

                                conn.Open();
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "Select * from " + oMachineInterface.TableName + " where " + oMachineInterface.CardCode + "='" +
                                               CardCode + "' and " + oMachineInterface.DateField + ">=" + "'" + _Paymonth_string + "'" + " and " +
                                               oMachineInterface.DateField + "<=" + "'" + _Last_string + "'" + " order by " + oMachineInterface.InTimeField;
                                cmd.CommandType = CommandType.Text;
                                cmd.Connection = conn;
                                // reader = cmd.ExecuteReader();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        if (oMachineInterface.CardCode != null)
                                        {
                                            if (oMachineInterface.InterfaceName.LookupVal.ToUpper() == "SMARTTECH")
                                            {

                                                RawData oRawData = new RawData();
                                                int fild = Convert.ToInt32(reader.GetValue(6)); //Status value
                                                if (fild == 0)
                                                {
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                    {
                                                        oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                    {
                                                        oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                    }

                                                    if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                    {
                                                        oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                    }
                                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    {
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                        //oRawData.SwipeTime = DateTime.ParseExact(string.Format("{0} {1}", (oRawData.SwipeDate), reader[oMachineInterface.InTimeField]));
                                                        //oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    }
                                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                    {
                                                        oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));
                                                        //oRawData.SwipeTime = DateTime.ParseExact(oRawData.SwipeDate + " " + reader[oMachineInterface.OutTimeField], "dd/MM/yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                                        //reader[oMachineInterface.DateField] + reader[oMachineInterface.OutTimeField];
                                                    }

                                                    oRawData.InputType = InputType;

                                                    oRawData.DownloadDate = DateTime.Now;
                                                    oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawData.Add(oRawData);
                                                }
                                                else
                                                {
                                                    // verify faild data
                                                    RawDataFailure oRawDataFailure = new RawDataFailure();
                                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                    {
                                                        oRawDataFailure.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                    }
                                                    //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    //{
                                                    //    oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    //}
                                                    if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                    {
                                                        oRawDataFailure.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                    }

                                                    if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                    {
                                                        oRawDataFailure.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                    }
                                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                    {
                                                        // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                        oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));

                                                    }
                                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                    {
                                                        // oRawDataFailure.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                        oRawDataFailure.SwipeTime = Convert.ToDateTime(oRawDataFailure.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));

                                                    }
                                                    oRawDataFailure.DownloadDate = DateTime.Now;
                                                    oRawDataFailure.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                    oList_RawDataFailure.Add(oRawDataFailure);

                                                }
                                            }
                                            else
                                            {
                                                RawData oRawData = new RawData();
                                                if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                                {
                                                    oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                                }
                                                //if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                //{
                                                //    oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                //}
                                                if (oMachineInterface.UnitNoField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.UnitNoField])))
                                                {
                                                    oRawData.UnitCode = Convert.ToInt32(reader[oMachineInterface.UnitNoField]);
                                                }

                                                if (oMachineInterface.DateField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.DateField])))
                                                {
                                                    oRawData.SwipeDate = Convert.ToDateTime(reader[oMachineInterface.DateField]);
                                                }
                                                if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                                {
                                                    //oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                                    oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.InTimeField].ToString()));
                                                }
                                                if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                                {
                                                    // oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                                    oRawData.SwipeTime = Convert.ToDateTime(oRawData.SwipeDate.ToString()).Add(TimeSpan.Parse(reader[oMachineInterface.OutTimeField].ToString()));

                                                }
                                                oRawData.InputType = InputType;
                                                oRawData.DownloadDate = DateTime.Now;
                                                oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                oList_RawData.Add(oRawData);
                                            }
                                        }
                                    }
                                }
                                conn.Close();
                                //}
                            }
                            else if (_type == "MYSQL")
                            {

                            }
                            else if (_type == "ORACLE")
                            {

                            }
                            if (oList_RawData.Count > 0)
                            {
                                //using (DataBaseContext db = new DataBaseContext())
                                //{
                                var oEmployeeAttendance = db.EmployeeAttendance.Include(a => a.RawData).Where(e => e.Employee.CardCode == CardCode).SingleOrDefault();
                                for (int i = 0; i < oList_RawData.Count; i++)
                                {
                                    oEmployeeAttendance.RawData.Add(oList_RawData[i]);
                                }
                                db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                //}
                            }
                            // failure data
                            if (oList_RawDataFailure.Count > 0)
                            {
                                //using (DataBaseContext db = new DataBaseContext())
                                //{
                                var oEmployeeAttendanceF = db.EmployeeAttendance.Include(a => a.RawDataFailure).Where(e => e.Employee.CardCode == CardCode).SingleOrDefault();
                                for (int i = 0; i < oList_RawDataFailure.Count; i++)
                                {
                                    oEmployeeAttendanceF.RawDataFailure.Add(oList_RawDataFailure[i]);
                                }
                                db.Entry(oEmployeeAttendanceF).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                //}
                            }
                            ts.Complete();
                            // return "0";
                        }
                    }


                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ex.Message;
                }
            }
            return "0";
        }
    }
}
