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
//using System.Data.SqlClient;
using P2BUltimate.Models;
using System.Data.SqlClient;
using System.Data;
namespace P2BUltimate.Process
{
    public class AttendanceProcess
    {
        public class returnClass
        {
            public string ErrNo { get; set; }
            public string ErrDesc { get; set; }
        }
        public static List<TimingPolicy> GetCompanyTimingpolicy(TimingPolicyBatchAssignment oTimingPolicyBatchAssignment,
                                                                ReportingTimingStruct oEmpReportingTimingStruct, DateTime _PayMonth)
        {

            var oTimingPolicy = new List<TimingPolicy>();
            DateTime _LastDate = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
            if (oTimingPolicyBatchAssignment.IsRoaster)
            {
                // var DaysWise = oTimingPolicyBatchAssignment.TimingweeklySchedule.ToList();
                for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
                {
                    var day = _Date.ToString("dddd").ToUpper();
                    var DaysWise = oTimingPolicyBatchAssignment.TimingweeklySchedule.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.TimingGroup).ToList();
                    foreach (var item in DaysWise)
                    {
                        oTimingPolicy.AddRange(item.TimingPolicy);
                    }
                }
            }
            else if (oTimingPolicyBatchAssignment.IsTimingGroup)
            {
                oTimingPolicy.AddRange(oTimingPolicyBatchAssignment.TimingGroup.TimingPolicy);
            }
            else if (oTimingPolicyBatchAssignment.IsWeeklyTimingSchedule)
            {
                for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
                {
                    var day = _Date.ToString("dddd").ToUpper();
                    var DaysWise = oTimingPolicyBatchAssignment.TimingweeklySchedule.Where(e => e.WeekDays.LookupVal.ToUpper() == day).Select(e => e.TimingGroup).ToList();
                    foreach (var item in DaysWise)
                    {
                        oTimingPolicy.AddRange(item.TimingPolicy);
                    }
                }
                //var DaysWise = oTimingPolicyBatchAssignment.TimingweeklySchedule.Select(a => a.TimingGroup).ToList();
                //foreach (var item in DaysWise)
                //{
                //    oTimingPolicy.AddRange(item.TimingPolicy);
                //}
            }
            return oTimingPolicy;
        }

        //public static List<EmpTimingMonthlyRoaster> _returnEmpTimingMonthlyRoasterData(Int32 CompId, List<EmpTimingRoasterData> oEmpTimingRoasterData,
        //                                                                        DateTime _PayMonth, DateTime _LastDate)
        //{
        //    List<EmpTimingMonthlyRoaster> oEmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
        //        {
        //            string TimeingCode = "";
        //            //Check It Company holiday/weakly off
        //            oEmpTimingRoasterData.Where(e => e.RoasterDate == _Date).SingleOrDefault();
        //            //----------------------------------
        //            oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
        //            {
        //                RoasterDate = _Date,
        //                DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault(),
        //                TimingPolicy = null,
        //                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //            });
        //        }
        //    }
        //    return oEmpTimingMonthlyRoaster;
        //}

        public static string Check_HWWO(Int32 CompId, EmployeeAttendance _fetch_emp_policy, DateTime _Date)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string TimeingCode = null;
                IEnumerable<HolidayCalendar> _Holiday_Company = db.Company
                             .Where(e => e.Id == CompId && e.Location.Any(a => a.Id == _fetch_emp_policy.Employee.GeoStruct.Location.Id))
                             .Select(a => a.HolidayCalendar.Where(x => x.HolidayList.Any(e => e.HolidayDate == _Date)))
                             .AsParallel().SingleOrDefault();

                var day = _Date.ToString("dddd");

                IEnumerable<WeeklyOffCalendar> _Wealyoff_Calendar =
                    db.Company.Where(e => e.Id == CompId && e.Location.Any(a => a.Id == _fetch_emp_policy.Employee.GeoStruct.Location.Id))
                    .Select(a => a.WeeklyOffCalendar.Where(v => v.WeeklyOffList.Any(c => c.WeekDays.LookupVal.ToUpper() == day.ToUpper())))
                .AsParallel().SingleOrDefault();
                if (_Holiday_Company.Count() > 0)
                {
                    TimeingCode = "Holiday";
                }
                else
                {
                    if (_Wealyoff_Calendar.Count() > 0)
                    {
                        TimeingCode = "weeklyoff";
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
                        DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault(),
                        TimingPolicy = TimeingCode == "Working" ? _oTimingPolicy.LastOrDefault() : null,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    });
                }
            }
            return oEmpTimingMonthlyRoaster;
        }

        public static void Genrate_Monthly_Roaster(Int32 _CompId, List<int> _EmpIds, string _PayMonth_p)
        {
            foreach (var EmpId in _EmpIds)
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (_PayMonth_p != null)
                    {
                        List<TimingPolicy> _oTimingPolicy = new List<TimingPolicy>();
                        DateTime _PayMonth = Convert.ToDateTime("01/" + _PayMonth_p);
                        DateTime _LastDate = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                        List<EmpTimingMonthlyRoaster> oEmpTimingMonthlyRoaster = new List<EmpTimingMonthlyRoaster>();
                        _PayMonth = _PayMonth.Date;
                        _LastDate = _LastDate.Date;
                        EmployeeAttendance _Prv_Month_Struct = db.EmployeeAttendance.Include(a => a.EmpTimingMonthlyRoaster)
                            .Where(a => a.Employee.Id == EmpId
                                && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PayMonth &&
                                    DbFunctions.TruncateTime(e.RoasterDate) <= _LastDate)).AsParallel().SingleOrDefault();

                        if (_Prv_Month_Struct != null)
                        {
                            //delete _Prv_Month_Struct
                            db.EmpTimingMonthlyRoaster.RemoveRange(_Prv_Month_Struct.EmpTimingMonthlyRoaster.Where(e => e.RoasterDate.Value.Date >= _PayMonth &&
                               e.RoasterDate.Value.Date <= _LastDate).ToList());
                            db.SaveChanges();
                            //------------------------------------------------------------------------------------------
                        }

                        //find emp wise polcy
                        var _Comp_Attendance = db.CompanyAttendance
                            .Include(a => a.OrgTimingPolicyBatchAssignment)
                            .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.Geostruct))
                            .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment))
                            .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup)))
                            .Include(a => a.OrgTimingPolicyBatchAssignment.Select(e => e.TimingPolicyBatchAssignment.Select(r => r.TimingGroup.TimingPolicy)))
                            .Where(a => a.Company.Id == _CompId)
                           .AsParallel().SingleOrDefault();

                        var _fetch_emp_policy = db.EmployeeAttendance.Include(a => a.EmpReportingTimingStruct)
                            .Include(a => a.Employee.GeoStruct)
                            .Include(a => a.Employee.GeoStruct.Location)
                            .Include(a => a.Employee.FuncStruct)
                            .Include(a => a.EmpTimingRoasterData)
                            .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct))
                            .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.Select(w => w.GeoGraphList)))
                            .Include(a => a.EmpReportingTimingStruct.Select(e => e.ReportingTimingStruct.Select(w => w.TimingPolicy)))
                            .Where(a => a.Employee.Id == EmpId && a.EmpReportingTimingStruct.Count > 0)
                            .AsParallel().SingleOrDefault();

                        List<EmpTimingRoasterData> _List_EmpTimingRoasterData = new List<EmpTimingRoasterData>();
                        if (_fetch_emp_policy != null)
                        {
                            var _EmpReportingTimingStruct = _fetch_emp_policy.EmpReportingTimingStruct.Where(a => a.EndDate == null).FirstOrDefault();
                            var _emp_reporting_struct = _EmpReportingTimingStruct.ReportingTimingStruct.LastOrDefault();
                            if (_emp_reporting_struct.GeographicalAppl)
                            {
                                OrgTimingPolicyBatchAssignment _Comp_Attendance_policy = _Comp_Attendance.OrgTimingPolicyBatchAssignment
                                    .Where(a => a.Geostruct != null && a.Geostruct.Id == _fetch_emp_policy.Employee.GeoStruct.Id).LastOrDefault();
                                if (_Comp_Attendance_policy != null)
                                {
                                    _oTimingPolicy = GetCompanyTimingpolicy(_Comp_Attendance_policy.TimingPolicyBatchAssignment.LastOrDefault(),
                                        _emp_reporting_struct, _PayMonth);
                                }
                            }
                            else if (_emp_reporting_struct.IsTimeRoaster)
                            {
                                if (_fetch_emp_policy.EmpTimingRoasterData.Count == 0)
                                {
                                    //roaster not define
                                    continue;
                                }

                                _List_EmpTimingRoasterData = _fetch_emp_policy.EmpTimingRoasterData.Where(e => e.RoasterDate.Value.Date >= _PayMonth &&
                                    e.RoasterDate.Value.Date <= _PayMonth).ToList();

                                if (_List_EmpTimingRoasterData.Count() > 0)
                                {
                                    // oEmpTimingMonthlyRoaster = _returnEmpTimingMonthlyRoasterData(CompId, _List_EmpTimingRoasterData, _PayMonth, _LastDate);
                                }

                            }
                            else if (_emp_reporting_struct.IndividualAppl)
                            {
                                _oTimingPolicy.Add(_emp_reporting_struct.TimingPolicy);
                            }
                        }
                        else
                        {
                            continue;
                            //Employee reporting timing not found
                        }
                        if (_oTimingPolicy.Count() > 0)
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
                                oEmpTimingMonthlyRoaster.Add(new EmpTimingMonthlyRoaster
                                {
                                    RoasterDate = _Date,
                                    DayType = db.LookupValue.Where(a => a.LookupVal.ToUpper() == TimeingCode.ToUpper()).AsParallel().SingleOrDefault(),
                                    TimingPolicy = TimeingCode == "Working" ? _oTimingPolicy.LastOrDefault() : null,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                });
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
        public static string Find_Remark(List<RawData> _List_oRawData,
                                        EmpTimingMonthlyRoaster oEmpTimingMonthlyRoaster, ProcessedData oProcessedData)
        {
            string _Remark = String.Empty;

            if (_List_oRawData.Count > 0)
            {
                TimingPolicy _oTimingPolicy = oEmpTimingMonthlyRoaster.TimingPolicy;
                TimeSpan _Time_In, _Time_Out;
                DateTime _min = Convert.ToDateTime(_List_oRawData.Min(a => a.SwipeTime));
                DateTime _max = Convert.ToDateTime(_List_oRawData.Max(a => a.SwipeTime));
                _Time_In = Utility._returnTimeSpan(_min);
                _Time_Out = Utility._returnTimeSpan(_max);

                TimeSpan _InTimeSpan = Utility._returnTimeSpan(_oTimingPolicy.InTimeSpan.Value);
                TimeSpan _OutTimeSpanTime = Utility._returnTimeSpan(_oTimingPolicy.OutTimeSpanTime.Value);
                TimeSpan _GraceNoAction = Utility._returnTimeSpan(_oTimingPolicy.GraceNoAction.Value);
                TimeSpan _GraceLateAction = Utility._returnTimeSpan(_oTimingPolicy.GraceLateAction.Value);
                TimeSpan _GraceEarlyAction = Utility._returnTimeSpan(_oTimingPolicy.GraceEarlyAction.Value);

                if (TimeSpan.Compare(_Time_In, _Time_Out) != 0 || TimeSpan.Compare(_Time_In, _Time_Out) < 0)
                {
                    if (String.IsNullOrEmpty(_Remark))
                    {
                        if (
                            (
                            ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.InTimeStart.Value)) <= _Time_In) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.InTime.Value) + _GraceNoAction) >= _Time_In))
                            &&
                            (((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - Utility._returnTimeSpan(_oTimingPolicy.GraceEarlyAction.Value)) <= _Time_Out) &&
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
                            ((Utility._returnTimeSpan(_oTimingPolicy.LunchStartTime.Value) <= _Time_Out) &&
                            (Utility._returnTimeSpan(_oTimingPolicy.LunchEndTime.Value) >= _Time_Out)))
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
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) >= _Time_Out) &&
                            ((Utility._returnTimeSpan(_oTimingPolicy.OutTime.Value) - _GraceEarlyAction) >= _Time_Out))
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

                if (String.IsNullOrEmpty(_Remark))
                {
                    _Remark = "**";
                }
                oProcessedData.InLateTime = oProcessedData.InEarlyTime = oProcessedData.InTime = _min;
                oProcessedData.OutEarlyTime = oProcessedData.OutLateTime = oProcessedData.OutTime = _max;
            }
            else
            {
                //check its leave apply or not
                //absent
                _Remark = "UA";

            }
            return _Remark;
        }

        public class remark_return_class
        {
            public string _Remark { get; set; }
            public int _LateCount { get; set; }
        }

        public static remark_return_class Reamrk_Analysis(Int32 _CompId,
                                                          Int32 _EmpId,
                                                          DateTime _Date, DateTime _PayMonth, List<RawData> _List_oRawData,
                                                          EmpTimingMonthlyRoaster _oEmpTimingMonthlyRoaster, ProcessedData _oProcessedData)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string _oRemark = Find_Remark(_List_oRawData, _oEmpTimingMonthlyRoaster, _oProcessedData);
                int _oLateCount = 0;

                if (_oRemark == "UA")
                {
                    List<EmployeeLeave> _LV_Exits = new List<EmployeeLeave>();
                    _LV_Exits = db.CompanyLeave
                        .Include(z => z.EmployeeLeave)
                        .Where(e => e.Company.Id == _CompId &&
                            e.EmployeeLeave.Any(a => a.Employee.Id == _EmpId)).SelectMany(a => a.EmployeeLeave
                                .Where(z => z.LvNewReq.Any(x => (x.LeaveCalendar.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && x.LeaveCalendar.Default == true) &&
                                    (x.IsCancel == false && x.TrReject == false && DbFunctions.TruncateTime(x.FromDate) <= _Date.Date && DbFunctions.TruncateTime(x.ToDate) >= _Date.Date)))).AsParallel().ToList();
                    if (_LV_Exits.Count() > 0)
                    {
                        _LV_Exits = db.CompanyLeave
                         .Include(z => z.EmployeeLeave)
                         .Where(e => e.Company.Id == _CompId &&
                             e.EmployeeLeave.Any(a => a.Employee.Id == _EmpId)).SelectMany(a => a.EmployeeLeave
                                 .Where(z => z.LvNewReq.Any(x => (x.LeaveCalendar.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && x.LeaveCalendar.Default == true) &&
                                     (x.IsCancel == false && x.TrReject == false && DbFunctions.TruncateTime(x.FromDate) <= _Date.Date && DbFunctions.TruncateTime(x.ToDate) >= _Date.Date) &&
                                     (x.LeaveHead.LvCode.ToUpper() == "LWP")))).AsParallel().ToList();
                        if (_LV_Exits.Count > 0)
                        {
                            _oRemark = "AA";
                        }
                        else
                        {
                            _oRemark = "UA";
                        }
                    }
                    else
                    {
                        _oRemark = "UA";
                    }
                }
                else if (_oRemark == "IL")
                {
                    var _oTimingPolicy = _oEmpTimingMonthlyRoaster.TimingPolicy;
                    if (_oTimingPolicy.IsLateCountInit)
                    {
                        var _Emp_Histry = db.EmployeeAttendance.Include(a => a.ProcessedData)
                            .Where(a => a.Employee.Id == _EmpId).SingleOrDefault();
                        if (_Emp_Histry == null)
                        {
                            _oRemark = "IL";
                        }
                        var _Histry = _Emp_Histry.ProcessedData.Where(b => b.AttendProcessDate.Value.Date >= _PayMonth.Date &&
                            b.AttendProcessDate.Value.Date <= _Date.Date).ToList();

                        var _LateCount = _Histry.Where(e => e.MusterRemarks.LookupVal == "LH" ||
                               e.MusterRemarks.LookupVal == "LF" ||
                               e.MusterRemarks.LookupVal == "IL").Select(a => a.LateCount).LastOrDefault();

                        var _LateAction = _oEmpTimingMonthlyRoaster.TimingPolicy.LateAction.LookupVal.ToUpper();

                        if (_Histry != null)
                        {
                            if (_LateCount >= _oTimingPolicy.LateCount)
                            {
                                if (_LateAction == "FULLDAY")
                                {
                                    _oRemark = "LF";
                                    _oLateCount = _LateCount++;
                                }
                                else if (_LateAction == "HALFDAY")
                                {
                                    _oRemark = "LH";
                                    _oLateCount = _LateCount++;
                                }
                                else if (_LateAction == "NOACTION")
                                {
                                    _oRemark = "IL";
                                    _oLateCount = _LateCount++;
                                }
                            }
                            else
                            {
                                _oRemark = "IL";
                                _oLateCount = _LateCount++;
                            }
                        }
                        else
                        {
                            _oRemark = "IL";
                            _oLateCount = _LateCount++;
                        }
                    }
                    else
                    {
                        _oRemark = "IL";
                    }
                }
                return new remark_return_class
                {
                    _LateCount = _oLateCount,
                    _Remark = _oRemark
                };
            }
        }

        public static void Genrate_Attendance(Int32 _CompId, List<int> _EmpIds, string _PayMonth_p)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                foreach (var EmpId in _EmpIds)
                {
                    if (_PayMonth_p != null)
                    {
                        DateTime _PayMonth = Convert.ToDateTime("01/" + _PayMonth_p);
                        DateTime _LastDate = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                        _PayMonth = _PayMonth.Date;
                        _LastDate = _LastDate.Date;
                        List<ProcessedData> _Emp_Process_Data = db.EmployeeAttendance.Include(a => a.RawData)
                                                              .Where(a => a.Employee.Id == EmpId &&
                                                                  a.ProcessedData.Any(e => DbFunctions.TruncateTime(e.SwipeDate) >= _PayMonth &&
                                                                      DbFunctions.TruncateTime(e.SwipeDate) <= _LastDate))
                                                                  .SelectMany(a => a.ProcessedData).ToList();
                        if (_Emp_Process_Data.Count > 0)
                        {
                            var EmployeeAttendance_del = db.EmployeeAttendance.Include(a => a.RawData)
                                                             .Where(a => a.Employee.Id == EmpId).SingleOrDefault();

                            List<ProcessedData> _Emp_Process_Data_Del = EmployeeAttendance_del.ProcessedData
                                .Where(e => e.SwipeDate != null && e.SwipeDate.Value.Date >= _PayMonth && e.SwipeDate.Value.Date <= _LastDate).ToList();

                            db.ProcessedData.RemoveRange(_Emp_Process_Data_Del);
                            db.SaveChanges();
                        }
                        //-----------------Find Structure Created Or Not-------------------------
                        var _Extance_EmpTimingMonthlyRoaster = db.EmployeeAttendance
                            .Where(a => a.Employee.Id == EmpId && a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) >= _PayMonth
                                && DbFunctions.TruncateTime(e.RoasterDate) <= _PayMonth)).ToList();

                        if (_Extance_EmpTimingMonthlyRoaster.Count() == 0)
                        {
                            continue;
                            //return monthly timing Structure not Found
                        }
                        else
                        {

                        }

                        //----------------------------------------------------------------------

                        List<ProcessedData> _oProcessedData = new List<ProcessedData>();
                        EmployeeAttendance _Emp_Row_Data = db.EmployeeAttendance.Include(a => a.RawData)
                                       .Where(a => a.Employee.Id == EmpId &&
                                           a.RawData.Any(e => DbFunctions.TruncateTime(e.SwipeDate) >= _PayMonth &&
                                               DbFunctions.TruncateTime(e.SwipeDate) <= _LastDate)).SingleOrDefault();
                        for (DateTime _Date = _PayMonth; _Date <= _LastDate; _Date = _Date.AddDays(1))
                        {

                            //--------Find Employee Monthly Reporting Structure--------------------------
                            EmployeeAttendance _oEmployeeAttendance = db.EmployeeAttendance
                                .Include(a => a.Employee)
                                .Include(a => a.Employee.GeoStruct)
                                .Include(a => a.Employee.PayStruct)
                                .Include(a => a.Employee.FuncStruct)
                                .Include(a => a.EmpTimingMonthlyRoaster)
                                .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.DayType))
                                .Include(a => a.EmpTimingMonthlyRoaster.Select(e => e.TimingPolicy))
                                .Where(a => a.Employee.Id == EmpId &&
                                    a.EmpTimingMonthlyRoaster.Any(e => DbFunctions.TruncateTime(e.RoasterDate) == _Date)).AsParallel().SingleOrDefault();

                            ProcessedData oProcessedData = new ProcessedData();

                            if (_oEmployeeAttendance != null)
                            {
                                EmpTimingMonthlyRoaster _EmpTimingMonthlyRoaster = _oEmployeeAttendance.EmpTimingMonthlyRoaster
                                     .Where(a => a.RoasterDate.Value.Date == _Date).AsParallel().SingleOrDefault();
                                if (_EmpTimingMonthlyRoaster.DayType.LookupVal.ToUpper() == "WORKING")
                                {
                                    if (_Emp_Row_Data != null)
                                    {
                                        //Find how many record 4 that day
                                        List<RawData> _Row_Data = _Emp_Row_Data.RawData.Where(e => e.SwipeDate.Value.Date == _Date).ToList();

                                        remark_return_class _Remark = Reamrk_Analysis(_CompId, EmpId, _Date, _PayMonth, 
                                            _Row_Data, _EmpTimingMonthlyRoaster, oProcessedData);
                                        var get_remark = db.RemarkConfig
                                            .Include(e => e.AlterMusterRemark)
                                            .Include(e => e.PresentStatus)
                                            .Where(e => e.MusterRemarks.LookupVal == _Remark._Remark).SingleOrDefault();
                                        oProcessedData.MusterRemarks = get_remark.AlterMusterRemark;
                                        //db.LookupValue.Where(a => a.LookupVal.ToUpper() == _Remark._Remark).SingleOrDefault();
                                        oProcessedData.LateCount = _Remark._LateCount;
                                        oProcessedData.MInTime = _EmpTimingMonthlyRoaster.TimingPolicy.InTime;
                                        oProcessedData.MOutTime = _EmpTimingMonthlyRoaster.TimingPolicy.OutTime;
                                        oProcessedData.PresentStatus = get_remark.PresentStatus;
                                    }
                                    else
                                    {
                                        continue;
                                        // return;
                                        //Row Data Not Found
                                    }
                                }
                                else
                                {
                                    if (_EmpTimingMonthlyRoaster.DayType.LookupVal.ToUpper() == "HOLIDAY")
                                    {
                                        oProcessedData.MusterRemarks = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "Holiday".ToUpper()).SingleOrDefault();
                                    }
                                    else if (_EmpTimingMonthlyRoaster.DayType.LookupVal.ToUpper() == "WEEKLYOFF")
                                    {
                                        oProcessedData.MusterRemarks = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "WEEKLYOFF").SingleOrDefault();
                                    }
                                }
                                oProcessedData.AttendProcessDate = DateTime.Now;
                                oProcessedData.SwipeDate = _Date;
                                oProcessedData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                oProcessedData.GeoStruct = _oEmployeeAttendance.Employee.GeoStruct;
                                oProcessedData.FuncStruct = _oEmployeeAttendance.Employee.FuncStruct;
                                oProcessedData.PayStruct = _oEmployeeAttendance.Employee.PayStruct;
                                oProcessedData.TimingCode = _EmpTimingMonthlyRoaster.TimingPolicy;
                                _oProcessedData.Add(oProcessedData);
                            }
                            else
                            {

                                continue;
                                //return;
                                //EmpTimingMonthlyRoaster not generated
                            }
                            //---------------------------------------------------------------
                        }
                        if (_oProcessedData.Count > 0)
                        {
                            var oEmployeeAttendance = db.EmployeeAttendance.Include(e => e.ProcessedData).Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                            for (int i = 0; i < _oProcessedData.Count(); i++)
                            {
                                oEmployeeAttendance.ProcessedData.Add(_oProcessedData[i]);
                            }
                            db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        public static void Recovery_AttendanceData(Int32 _CompId, List<string> _CardCodes, DateTime _PayMonth, Int32 _ReCoveryType)
        {

            var _Last = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
            var _Paymonth_string = _PayMonth.ToString("MM/dd/yyyy");
            var _Last_string = _Last.ToString("MM/dd/yyyy");
            _PayMonth = _PayMonth.Date;
            _Last = _Last.Date;
            foreach (var CardCode in _CardCodes)
            {
                try
                {
                    MachineInterface oMachineInterface = new MachineInterface();
                    List<RawData> oList_RawData = new List<RawData>();
                    using (DataBaseContext db = new DataBaseContext())
                    {
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
                            }
                            else
                            {

                                var _EmployeeAttendance_del = db.EmployeeAttendance.Where(e => e.Employee.CardCode == CardCode
                            && e.RawData.Any(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth && DbFunctions.TruncateTime(z.SwipeDate) <= _Last)).SingleOrDefault();

                                var _RawDatadel = _EmployeeAttendance_del.RawData.Where(z => DbFunctions.TruncateTime(z.SwipeDate) >= _PayMonth &&
                                    DbFunctions.TruncateTime(z.SwipeDate) <= _Last).ToList();
                                db.RawData.RemoveRange(_RawDatadel);
                                db.SaveChanges();
                            }
                        }
                        var _CompanyAttendance = db.CompanyAttendance.Include(a => a.MachineInterface)
                            .Include(a => a.MachineInterface.Select(e => e.DatabaseType))
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
                    }
                    string _type = oMachineInterface.DatabaseType.LookupVal.ToUpper();
                    if (_type == "ACCESS")
                    {
                        string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;OLE DB Services = -4;Data Source=" + oMachineInterface.DatabaseName + "";

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
                                        RawData oRawData = new RawData();
                                        if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                        {
                                            oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                        }
                                        if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                        {
                                            oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                        }
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
                                            oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                        }
                                        if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                        {
                                            oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                        }
                                        oRawData.DownloadDate = DateTime.Now;
                                        oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                        oList_RawData.Add(oRawData);
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
                            ";Integrated Security=false;";
                        //using (SqlConnection cnn = new SqlConnection(connetionString))
                        //{
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
                                    RawData oRawData = new RawData();
                                    if (!string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.CardCode])))
                                    {
                                        oRawData.CardCode = Convert.ToString(reader[oMachineInterface.CardCode]);
                                    }
                                    if (oMachineInterface.InTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.InTimeField])))
                                    {
                                        oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                    }
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
                                        oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.InTimeField]);
                                    }
                                    if (oMachineInterface.OutTimeField != null && !string.IsNullOrEmpty(Convert.ToString(reader[oMachineInterface.OutTimeField])))
                                    {
                                        oRawData.SwipeTime = Convert.ToDateTime(reader[oMachineInterface.OutTimeField]);
                                    }
                                    oRawData.DownloadDate = DateTime.Now;
                                    oRawData.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    oList_RawData.Add(oRawData);
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
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            var oEmployeeAttendance = db.EmployeeAttendance.Include(a => a.RawData).Where(e => e.Employee.CardCode == CardCode).SingleOrDefault();
                            for (int i = 0; i < oList_RawData.Count; i++)
                            {
                                oEmployeeAttendance.RawData.Add(oList_RawData[i]);
                            }
                            db.Entry(oEmployeeAttendance).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }
    }
}
