using P2b.Global;
using P2BUltimate.Process;
using Payroll;
using Leave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using P2BUltimate.App_Start;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Security;
using Attendance;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using P2B.UTILS;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Configuration;


namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class ManualAttendanceProcessController : Controller
    {
        // GET: Default
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/ManualAttendanceProcess/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("");  //not created
        }
        public ActionResult Negativepartial()
        {
            return View("~/Views/Shared/Payroll/_Negativepartial.cshtml");
        }
        public ActionResult Negpartial()
        {
            return View("~/Views/Shared/Payroll/_Negpartial.cshtml");
        }
        public ActionResult IsPayslipGenrated()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var paymonth = db.SalaryT.OrderByDescending(a => a.Id).FirstOrDefault().PayMonth;
                var payslipChk = db.PaySlipR.Where(a => a.PayMonth == paymonth).ToList();
                if (payslipChk != null && payslipChk.Count > 0)
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }
            }
        }
        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var date = Convert.ToDateTime(month);
                var query = db.ProcessedData.Where(e => e.AttendProcessDate == date).AsNoTracking().AsParallel().Select(e => e.Id);

                if (query != null)
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public class Perioddt
        {

            public string Periodfrom { get; set; }
            public string PeriodTo { get; set; }
            public string Msg { get; set; }
        }
        public ActionResult getperiod(string forwardata, string Processgrp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Perioddt Perioddtt = new Perioddt();
                //List<Perioddt> PerioddttList = new List<Perioddt>();
                var SalMonths = "";
                int ids = Convert.ToInt32(Processgrp);
                if (forwardata != null && forwardata != "0" && forwardata != "false")
                {
                    SalMonths = forwardata;
                }
                else
                {
                    Perioddtt = new Perioddt()
                    {
                        Periodfrom = null,
                        PeriodTo = null,
                        Msg = " Kindly Select Salary Month. "
                    };
                }
                if (ids == null && ids == 0)
                {

                    Perioddtt = new Perioddt()
                    {
                        Periodfrom = null,
                        PeriodTo = null,
                        Msg = " Kindly Select Payprocess Group. "
                    };
                }



                var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod)
                    .Where(e => e.Id == ids).SingleOrDefault().PayrollPeriod.FirstOrDefault();

                int startday = query1.StartDate;
                int endday = query1.EndDate;
                DateTime _PayMonth = Convert.ToDateTime("01/" + SalMonths);

                DateTime end = Convert.ToDateTime("01/" + SalMonths).AddMonths(1).AddDays(-1).Date;
                //int daysInEndMonth = (end - end.AddMonths(1)).Days
                int daysInEndMonth = end.Day;
                int daysInstartMonth = 1;
                DateTime FromPeriod;
                DateTime EndPeriod;

                int daym = (Convert.ToDateTime("01/" + SalMonths).AddMonths(1).AddDays(-1).Date).Day;
                if (endday > daym)
                {
                    endday = daym;
                }

                if (startday == daysInstartMonth && endday == daysInEndMonth)
                {
                    FromPeriod = _PayMonth;
                    EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                }
                else
                {
                    DateTime prvmonth = Convert.ToDateTime("01/" + SalMonths).AddMonths(-1).Date;
                    startday = endday + 1;
                    string pmonth = prvmonth.ToString("MM/yyyy");
                    FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                    EndPeriod = Convert.ToDateTime(endday + "/" + SalMonths);

                }
                if (DateTime.Now < EndPeriod)
                {
                    EndPeriod = DateTime.Now;
                }

                Perioddtt = new Perioddt()
                 {
                     Periodfrom = FromPeriod.ToShortDateString(),
                     PeriodTo = EndPeriod.ToShortDateString()
                 };
                //PerioddttList.Add(Perioddtt);
                return Json(new { Perioddtt }, JsonRequestBehavior.AllowGet);
                //var qurey = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location.Address.City).Where(e => ids.Contains(e.Id))
                //    .Select(e => new
                //    {
                //        Id = e.Id,
                //        Citydesc = e.GeoStruct.Location.Address.City.FullDetails.ToString(),

                //    }).SingleOrDefault();

            }
        }
        public class ETRM_AttendanceProcess
        {
            public List<int> Emp_Ids { get; set; }

            public DateTime? FromPeriod { get; set; }

            public DateTime? ToPeriod { get; set; }
        }

        public class ReturnData_AttendanceProcess
        {
            public int? ErrNo { get; set; }

            public int? InfoNo { get; set; }

            public string EmpCode { get; set; }

            public string ErrMsg { get; set; }
        }

        public class ServiceResultList<T>
        {
            /// <summary>
            /// 
            /// </summary>
            public HttpStatusCode MessageCode { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<T> Data { get; set; }
        }

        public ActionResult ValidateForm(FormCollection form)
        {
            string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
            string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
            //  string PayMonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
            string PeriodFrom = form["PeriodFrom"] != null ? form["PeriodFrom"] : string.Empty;
            string PeriodTo = form["PeriodTo"] != null ? form["PeriodTo"] : string.Empty;
            string SalMonths = form["PayMonth"] == "0" ? "" : form["PayMonth"];

            List<string> Msg = new List<string>();

            try
            {
                int CompId = 0;
                int idpg = Convert.ToInt32(PayProcessGroupList);

                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    CompId = int.Parse(Session["CompId"].ToString());
                }
                List<int> ids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = Utility.StringIdsToListIds(Emp);

                }
                else
                {
                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (PeriodFrom != null && PeriodFrom != "" && PeriodTo != null && PeriodTo != "")
                {

                    if (Convert.ToDateTime(PeriodTo) < Convert.ToDateTime(PeriodFrom))
                    {
                        Msg.Add(" To date Should be grater than from date  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    Msg.Add(" Please Select From date and To Date  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (SalMonths == "" || SalMonths == null || SalMonths == "0")
                {
                    Msg.Add(" Please Select Salary Month  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                // payprocess group start
                PayrollPeriod query1 = null;
                using (DataBaseContext db = new DataBaseContext())
                {
                    query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod)
                       .Where(e => e.Id == idpg).SingleOrDefault().PayrollPeriod.FirstOrDefault();
                }
                int startday = query1.StartDate;
                int endday = query1.EndDate;
                DateTime _PayMonth = Convert.ToDateTime("01/" + SalMonths);

                DateTime end = Convert.ToDateTime("01/" + SalMonths).AddMonths(1).AddDays(-1).Date;
                //int daysInEndMonth = (end - end.AddMonths(1)).Days
                int daysInEndMonth = end.Day;
                int daysInstartMonth = 1;
                DateTime FromPeriod;
                DateTime EndPeriod;
                int daym = (Convert.ToDateTime("01/" + SalMonths).AddMonths(1).AddDays(-1).Date).Day;
                if (endday > daym)
                {
                    endday = daym;
                }

                //if (startday == daysInstartMonth && endday == daysInEndMonth)
                //{
                //    FromPeriod = _PayMonth;
                //    EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                //}
                //else
                //{
                //    DateTime prvmonth = Convert.ToDateTime("01/" + SalMonths).AddMonths(-1).Date;
                //    startday = endday + 1;
                //    string pmonth = prvmonth.ToString("MM/yyyy");
                //    FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);

                //    EndPeriod = Convert.ToDateTime(endday + "/" + SalMonths);

                //}
                //if (DateTime.Now < EndPeriod)
                //{
                //    EndPeriod = DateTime.Now;
                //}
                var _PeriodFrom = DateTime.Now;
                var _PeriodTo = DateTime.Now;
                //if (Convert.ToDateTime(PeriodFrom) < FromPeriod)
                //{
                //    _PeriodFrom = Convert.ToDateTime(FromPeriod);
                //}
                //else
                //{
                //    _PeriodFrom = Convert.ToDateTime(PeriodFrom);
                //}
                //if (Convert.ToDateTime(PeriodTo) > EndPeriod)
                //{
                //    _PeriodTo = Convert.ToDateTime(EndPeriod);
                //}
                //else
                //{
                //    _PeriodTo = Convert.ToDateTime(EndPeriod);
                //}
                //if (Convert.ToDateTime(_PeriodFrom) > Convert.ToDateTime(_PeriodTo))
                //{
                //    _PeriodFrom = Convert.ToDateTime(FromPeriod);
                //}
                List<EmpTimingMonthlyRoaster> _Extance_EmpTimingMonthlyRoaster = null;

                FromPeriod = Convert.ToDateTime(PeriodFrom);
                EndPeriod = Convert.ToDateTime(PeriodTo);

                _PeriodFrom = Convert.ToDateTime(PeriodFrom);
                _PeriodTo = Convert.ToDateTime(PeriodTo);

                using (DataBaseContext db = new DataBaseContext())
                {
                    _Extance_EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster
                       .Where(e => (e.RoasterDate.Value) >= FromPeriod && (e.RoasterDate.Value) <= EndPeriod).AsNoTracking()
                      .OrderBy(e => e.RoasterDate).ToList();


                    if (_Extance_EmpTimingMonthlyRoaster.Count() == 0)
                    {
                        Msg.Add(" Please create roster for selected period  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }

                // payprocess group end
                //var _PeriodFrom = Convert.ToDateTime(PeriodFrom);
                //var _PeriodTo = Convert.ToDateTime(PeriodTo);

                // ====================================== Api call Start ==============================================
                ReturnData_AttendanceProcess returnDATA = new ReturnData_AttendanceProcess();

                var ShowMessageCode = "";
                var ShowMessage = "";
                var ShowErrorMessage = "";

                ServiceResultList<ReturnData_AttendanceProcess> responseDeserializeData = new ServiceResultList<ReturnData_AttendanceProcess>();
                string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                {
                    var response = p2BHttpClient.request("ETRM/getUserAttendanceMusterProcessRequest",
                        new ETRM_AttendanceProcess() { Emp_Ids = ids, FromPeriod = _PeriodFrom, ToPeriod = _PeriodTo });

                    var data = response.Content.ReadAsStringAsync().Result;
                    // var result = data.;

                    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResultList<ReturnData_AttendanceProcess>>(response.Content.ReadAsStringAsync().Result);


                    ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                    ShowMessage = responseDeserializeData.Message.ToString();
                    foreach (var item in responseDeserializeData.Data)
                    {
                        ShowErrorMessage = item.ErrMsg.ToString();
                    }

                    if (responseDeserializeData != null && ShowMessageCode != "OK")
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            if (responseDeserializeData.Data.Count() > 0)
                            {


                                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\LogFile";
                                //System.Web.HttpContext.Current.Server.MapPath("\\JVFile");
                                string localPath = new Uri(requiredPath).LocalPath;
                                if (!System.IO.Directory.Exists(localPath))
                                {
                                    localPath = new Uri(requiredPath).LocalPath;
                                    System.IO.Directory.CreateDirectory(localPath);
                                }
                                string path = requiredPath + @"\MAN_Attendance_" + Convert.ToDateTime(DateTime.Now.Date).ToString("ddMMyyyy") + ".txt";
                                //System.Web.HttpContext.Current.Server.MapPath("\\JVFile")
                                path = new Uri(path).LocalPath;
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }

                                using (var fs = new FileStream(path, System.IO.FileMode.OpenOrCreate))
                                {
                                    StreamWriter str = new StreamWriter(fs);
                                    str.BaseStream.Seek(0, SeekOrigin.Begin);
                                    foreach (var item in responseDeserializeData.Data)
                                    {
                                        string empid = item.EmpCode;
                                        int eid = Convert.ToInt32(empid);
                                        var oemployee = db.Employee.Include(e => e.EmpName).Where(e => e.Id == eid).FirstOrDefault();

                                        str.WriteLine(oemployee.EmpCode.ToString() + ' ' + oemployee.EmpName.FullNameFML.ToString() + ' '
                                         + item.ErrNo + ' ' + item.ErrMsg + ' ' + DateTime.Now);

                                    }
                                    str.Flush();
                                    str.Close();
                                    fs.Close();
                                }
                                System.Diagnostics.Process.Start("notepad.exe", path);

                            }
                        }
                    }


                }


                // ====================================== Api call End ==============================================



                //AttendanceProcess.Generate_Attendance(
                //    //  _CompId: Convert.ToInt32(SessionManager.CompanyId),
                //        _EmpIds: ids,
                //    // _PayMonth_p: PayMonth
                //         _PeriodFrom: _PeriodFrom,
                //                _PeriodTo: _PeriodTo
                //        );
                //AttendanceProcess.Monthly_Roaster_check(_CompId: Convert.ToInt32(SessionManager.CompanyId), _EmpIds: ids, _PeriodFrom: _PeriodFrom,
                //        _PeriodTo: _PeriodTo);

                // ====================================== Api call Start 2nd Part ==============================================
                try
                {

                    if (responseDeserializeData == null && ShowMessageCode == "OK")
                    {
                        Msg.Add(ShowMessage);
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Msg.Add(ShowMessage + " ,  " + ShowErrorMessage);
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Create", new { concurrencyError = true, id = 0 });
                }
                catch (DataException /* dex */)
                {
                    // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }


                //if (Msg.Count() == 0)
                //{ Msg.Add("Data Saved Successfully..!"); }

                //// ts.Complete();
                //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                StackTrace stacktrace = new StackTrace();
                StackFrame[] frames = stacktrace.GetFrames();
                string stackName = string.Empty;
                foreach (StackFrame sf in frames)
                {
                    System.Reflection.MethodBase method = sf.GetMethod();
                    stackName += (method.Name + "|");
                }
                //end_foreach
                LogFile Logfile = new LogFile();
                ErrorLog Err = new ErrorLog()
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    ExceptionMessage = ex.Message,
                    ExceptionStackTrace = ex.StackTrace,
                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Msg.Add(ex.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }
        // ====================================== Api call End 2nd Part ==============================================


        // =========================================== Ultimate call Start ==============================================

        //////public ActionResult ValidateForm(FormCollection form)
        //////{
        //////    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
        //////    string PayProcessGroupList = form["PayProcessGroupList"] == "0" ? "" : form["PayProcessGroupList"];
        //////    //  string PayMonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
        //////    string PeriodFrom = form["PeriodFrom"] != null ? form["PeriodFrom"] : string.Empty;
        //////    string PeriodTo = form["PeriodTo"] != null ? form["PeriodTo"] : string.Empty;
        //////    string SalMonths = form["PayMonth"] == "0" ? "" : form["PayMonth"];

        //////    List<string> ErrMsg = new List<string>();
        //////    List<string> SucMsg = new List<string>();
        //////    try
        //////    {
        //////        int CompId = 0;
        //////        int idpg = Convert.ToInt32(PayProcessGroupList);

        //////        if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
        //////        {
        //////            CompId = int.Parse(Session["CompId"].ToString());
        //////        }
        //////        List<int> ids = null;
        //////        if (Emp != null && Emp != "0" && Emp != "false")
        //////        {
        //////            ids = Utility.StringIdsToListIds(Emp);

        //////        }
        //////        else
        //////        {
        //////            ErrMsg.Add(" Kindly select employee  ");
        //////            return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////        }
        //////        if (PeriodFrom != null && PeriodFrom != "" && PeriodTo != null && PeriodTo != "")
        //////        {

        //////            if (Convert.ToDateTime(PeriodTo) < Convert.ToDateTime(PeriodFrom))
        //////            {
        //////                ErrMsg.Add(" To date Should be grater than from date  ");
        //////                return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////            }
        //////        }
        //////        else
        //////        {
        //////            ErrMsg.Add(" Please Select From date and To Date  ");
        //////            return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////        }
        //////        if (SalMonths == "" || SalMonths == null || SalMonths == "0")
        //////        {
        //////            ErrMsg.Add(" Please Select Salary Month  ");
        //////            return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////        }
        //////        // payprocess group start
        //////        PayrollPeriod query1 = null;
        //////        using (DataBaseContext db = new DataBaseContext())
        //////        {
        //////            query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod)
        //////               .Where(e => e.Id == idpg).SingleOrDefault().PayrollPeriod.FirstOrDefault();
        //////        }
        //////        int startday = query1.StartDate;
        //////        int endday = query1.EndDate;
        //////        DateTime _PayMonth = Convert.ToDateTime("01/" + SalMonths);

        //////        DateTime end = Convert.ToDateTime("01/" + SalMonths).AddMonths(1).AddDays(-1).Date;
        //////        //int daysInEndMonth = (end - end.AddMonths(1)).Days
        //////        int daysInEndMonth = end.Day;
        //////        int daysInstartMonth = 1;
        //////        DateTime FromPeriod;
        //////        DateTime EndPeriod;
        //////        int daym = (Convert.ToDateTime("01/" + SalMonths).AddMonths(1).AddDays(-1).Date).Day;
        //////        if (endday > daym)
        //////        {
        //////            endday = daym;
        //////        }

        //////        if (startday == daysInstartMonth && endday == daysInEndMonth)
        //////        {
        //////            FromPeriod = _PayMonth;
        //////            EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
        //////        }
        //////        else
        //////        {
        //////            DateTime prvmonth = Convert.ToDateTime("01/" + SalMonths).AddMonths(-1).Date;
        //////            startday = endday + 1;
        //////            string pmonth = prvmonth.ToString("MM/yyyy");
        //////            FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);

        //////            EndPeriod = Convert.ToDateTime(endday + "/" + SalMonths);

        //////        }
        //////        if (DateTime.Now < EndPeriod)
        //////        {
        //////            EndPeriod = DateTime.Now;
        //////        }
        //////        var _PeriodFrom = DateTime.Now;
        //////        var _PeriodTo = DateTime.Now;
        //////        if (Convert.ToDateTime(PeriodFrom) < FromPeriod)
        //////        {
        //////            _PeriodFrom = Convert.ToDateTime(FromPeriod);
        //////        }
        //////        else
        //////        {
        //////            _PeriodFrom = Convert.ToDateTime(PeriodFrom);
        //////        }
        //////        if (Convert.ToDateTime(PeriodTo) > EndPeriod)
        //////        {
        //////            _PeriodTo = Convert.ToDateTime(EndPeriod);
        //////        }
        //////        else
        //////        {
        //////            _PeriodTo = Convert.ToDateTime(EndPeriod);
        //////        }
        //////        if (Convert.ToDateTime(_PeriodFrom) > Convert.ToDateTime(_PeriodTo))
        //////        {
        //////            _PeriodFrom = Convert.ToDateTime(FromPeriod);
        //////        }
        //////        List<EmpTimingMonthlyRoaster> _Extance_EmpTimingMonthlyRoaster = null;

        //////        using (DataBaseContext db = new DataBaseContext())
        //////        {
        //////            _Extance_EmpTimingMonthlyRoaster = db.EmpTimingMonthlyRoaster
        //////               .Where(e => (e.RoasterDate.Value) >= FromPeriod && (e.RoasterDate.Value) <= EndPeriod).AsNoTracking()
        //////              .OrderBy(e => e.RoasterDate).ToList();


        //////            if (_Extance_EmpTimingMonthlyRoaster.Count() == 0)
        //////            {
        //////                ErrMsg.Add(" Please create roster for selected period  ");
        //////                return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////            }
        //////        }

        //////        // payprocess group end
        //////        //var _PeriodFrom = Convert.ToDateTime(PeriodFrom);
        //////        //var _PeriodTo = Convert.ToDateTime(PeriodTo);

        //////        string ErrMsgL = "";
        //////        string SuccessMsg = "";
        //////        List<AttendanceProcess.ReturnData_AttendanceProcess> msglist = new List<AttendanceProcess.ReturnData_AttendanceProcess>();

        //////        msglist = AttendanceProcess.Generate_Attendance(
        //////                //  _CompId: Convert.ToInt32(SessionManager.CompanyId),
        //////                _EmpIds: ids,
        //////                 // _PayMonth_p: PayMonth
        //////                 _PeriodFrom: _PeriodFrom,
        //////                        _PeriodTo: _PeriodTo
        //////                );
        //////        //AttendanceProcess.Monthly_Roaster_check(_CompId: Convert.ToInt32(SessionManager.CompanyId), _EmpIds: ids, _PeriodFrom: _PeriodFrom,
        //////        //        _PeriodTo: _PeriodTo);

        //////        try
        //////        {

        //////            if (msglist.Count() > 0)
        //////            {
        //////                foreach (var item in msglist)
        //////                {
        //////                    if (item.ErrNo != 0)
        //////                    {
        //////                        using (DataBaseContext db = new DataBaseContext())
        //////                        {
        //////                            ErrMsgL = item.EmpCode + " - " +  db.ErrorLookup.Where(e => (e.Message_Code == item.ErrNo && item.ErrNo > 0)).Select(e => e.Message_Description).FirstOrDefault();
        //////                        }
        //////                        ErrMsg.Add(ErrMsgL);
        //////                    }
        //////                    else
        //////                    {
        //////                        using (DataBaseContext db = new DataBaseContext())
        //////                        {
        //////                            SuccessMsg = item.EmpCode + " - " +  db.ErrorLookup.Where(e => e.Info_code == item.InfoNo && item.InfoNo > 0).Select(e => e.Message_Description).FirstOrDefault();
        //////                        }
        //////                        SucMsg.Add(SuccessMsg);

        //////                    }
        //////                }
        //////            }
        //////            else
        //////            {
        //////                SucMsg.Add("  Data Saved successfully  ");
        //////                return Json(new Utility.JsonReturnClass { success = true, responseText = SucMsg }, JsonRequestBehavior.AllowGet);
        //////            }

        //////            if (ErrMsg.Count() > 0)
        //////            {
        //////                return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////            }
        //////            else
        //////            {
        //////                return Json(new Utility.JsonReturnClass { success = true, responseText = SucMsg }, JsonRequestBehavior.AllowGet);
        //////            }


        //////        }
        //////        catch (DbUpdateConcurrencyException)
        //////        {
        //////            return RedirectToAction("Create", new { concurrencyError = true, id = 0 });
        //////        }
        //////        catch (DataException /* dex */)
        //////        {
        //////            // return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //////            ErrMsg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //////            return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);

        //////        }


        //////        //if (Msg.Count() == 0)
        //////        //{ Msg.Add("Data Saved Successfully..!"); }

        //////        //// ts.Complete();
        //////        //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        StackTrace stacktrace = new StackTrace();
        //////        StackFrame[] frames = stacktrace.GetFrames();
        //////        string stackName = string.Empty;
        //////        foreach (StackFrame sf in frames)
        //////        {
        //////            System.Reflection.MethodBase method = sf.GetMethod();
        //////            stackName += (method.Name + "|");
        //////        }
        //////        //end_foreach
        //////        LogFile Logfile = new LogFile();
        //////        ErrorLog Err = new ErrorLog()
        //////        {
        //////            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //////            ExceptionMessage = ex.Message,
        //////            ExceptionStackTrace = ex.StackTrace,
        //////            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //////            LogTime = DateTime.Now
        //////        };
        //////        ErrMsg.Add(ex.Message);
        //////        return Json(new Utility.JsonReturnClass { success = false, responseText = ErrMsg }, JsonRequestBehavior.AllowGet);
        //////    }
        //////}

        // ====================================== Ultimate call End ==============================================


        public ActionResult Polulate_PayProcessGroup(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.Company.Include(e => e.PayProcessGroup).Where(e => e.Id == CompId).SingleOrDefault();
                    var selected = (Object)null;
                    selected = query.PayProcessGroup.Select(e => e.Id).FirstOrDefault();
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }

                    SelectList s = new SelectList(query.PayProcessGroup, "Id", "FullDetails", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPayscaleagreement(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.PayProcessGroup.Find(int.Parse(data));
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ChkProcessAction(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = true;
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                List<int> ids = null; try
                {
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }
                    else
                    {
                        Msg.Add("  Kindly Select Employee.  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Employee.Id == i).SingleOrDefault();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(0, 30, 0)))
                        {

                            using (DataBaseContext db2 = new DataBaseContext())
                            {

                                var SalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                                if (SalT != null)
                                {
                                    SalT.ReleaseDate = DateTime.Now.Date;
                                    db.SalaryT.Attach(SalT);
                                    db.Entry(SalT).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = SalT.RowVersion;
                                    db.Entry(SalT).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                            ts.Complete();

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
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true, responseText = "Salary released for employee." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadEmpByDefault(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                if (data != "" && data != null)
                {
                    dt = Convert.ToDateTime("01/" + data).AddMonths(-1);
                }
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    int CompId = int.Parse(Session["CompId"].ToString());
                    var query = db.CompanyPayroll.Where(e => e.Company.Id == CompId).Include(e => e.EmployeePayroll).Include(a => a.EmployeePayroll.Select(e => e.Employee)).SingleOrDefault();
                    List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                    foreach (var ca in query.EmployeePayroll.ToList())
                    {
                        if (ca.Employee.ServiceBookDates.ServiceLastDate.Value.Month == dt.Value.Month && ca.Employee.ServiceBookDates.ServiceLastDate.Value.Year == dt.Value.Year)
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = ca.Employee.Id.ToString(),
                                value = ca.Employee.FullDetails,
                            });
                        }
                    }
                    var jsondata = new
                    {
                        tablename = "Employee-Table",
                        data = returndata,
                    };
                    return Json(jsondata, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ActiononAttendance(string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime Pswipedate = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1);
                List<string> Msg = new List<string>();
                List<int> ids = null;
                int Processgrpid;
                ids = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(r => r.Id).ToList();
                string localPath;
                string requiredPathLoan = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPathLoan);
                string localPathLoan;
                if (!exists)
                {
                    localPathLoan = new Uri(requiredPathLoan).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathLoan);
                }
                string pathLoan = requiredPathLoan + @"\ODACTIONONATTENDANCE" + ".ini";
                localPathLoan = new Uri(pathLoan).LocalPath;
                if (!System.IO.File.Exists(localPathLoan))
                {

                    using (var fs = new FileStream(localPathLoan, FileMode.OpenOrCreate))
                    {
                        StreamWriter str = new StreamWriter(fs);
                        str.BaseStream.Seek(0, SeekOrigin.Begin);

                        str.Flush();
                        str.Close();
                        fs.Close();
                    }


                }

                List<string> remark = new List<string>();
                List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).Where(e => e.IsODAppl == true).ToList();
                foreach (var item in RC)
                {
                    remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                }

                foreach (var i in ids)
                {

                    var employeeLeave = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == i).FirstOrDefault();

                    //var BindEmpList1 = db.EmployeeAttendance.Include(e => e.Employee.EmpName)
                    // .Include(e => e.Employee.EmpOffInfo)
                    // .Include(e => e.Employee.ServiceBookDates)
                    // .Include(e => e.Employee.EmpOffInfo.PayProcessGroup)
                    // .Include(e => e.Employee.ServiceBookDates)
                    // .Include(e => e.ProcessedData)
                    // .Include(e => e.ProcessedData.Select(x => x.PresentStatus))
                    // .Include(e => e.ProcessedData.Select(x => x.MusterRemarks))
                    // .Where(e => e.Employee.Id == i).AsNoTracking().AsParallel().FirstOrDefault();

                    var BindEmpList = db.EmployeeAttendance.Select(e => new
                    {
                        Id = e.Id,
                        Employee = e.Employee,
                        EmpName = e.Employee.EmpName,
                        EmpOffInfo = e.Employee.EmpOffInfo,
                        ServiceBookDates = e.Employee.ServiceBookDates,
                        PayProcessGroup = e.Employee.EmpOffInfo.PayProcessGroup,
                        ProcessedData = e.ProcessedData.Select(r => new
                        {
                            PresentStatus = r.PresentStatus,
                            MusterRemarks = r.MusterRemarks,
                            SwipeDate = r.SwipeDate,
                            Id = r.Id,
                            ManualReason = r.ManualReason
                        }).Where(r => r.SwipeDate >= Pswipedate).ToList()
                    }).Where(e => e.Employee.Id == i).FirstOrDefault();


                    Processgrpid = BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id;

                    var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
               .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                    int startday = query1.StartDate;
                    int endday = query1.EndDate;
                    DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                    DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                    int daysInEndMonth = end.Day;
                    int daysInstartMonth = 1;
                    DateTime FromPeriod;
                    DateTime EndPeriod;
                    DateTime Currentmonthstart;
                    DateTime CurrentmonthEnd;
                    DateTime Prevmonthstart;
                    DateTime PrevmonthEnd;
                    int ProDays = 0;
                    int RetProDays = 0;
                    int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                    Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                    if (endday > daym)
                    {
                        endday = daym;
                    }
                    ProDays = daym - endday;
                    RetProDays = ProDays;
                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                    {
                        FromPeriod = _PayMonth;
                        EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                    }
                    else
                    {
                        DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                        startday = endday + 1;
                        string pmonth = prvmonth.ToString("MM/yyyy");
                        FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                        EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                    }
                    CurrentmonthEnd = EndPeriod;
                    Prevmonthstart = FromPeriod;
                    if (startday == daysInstartMonth && endday == daysInEndMonth)
                    {
                        PrevmonthEnd = FromPeriod.AddDays(ProDays);
                    }
                    else
                    {
                        PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                    }

                    if ((BindEmpList.Employee != null && BindEmpList.Employee.EmpOffInfo != null && BindEmpList.Employee.EmpOffInfo.PayProcessGroup != null
                           && BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                           (BindEmpList.Employee != null && BindEmpList.Employee.ServiceBookDates != null && BindEmpList.Employee.ServiceBookDates.ServiceLastDate != null))
                    {
                        continue;
                    }

                    Int32 GeoStruct = 0;
                    Int32 PayStruct = 0;
                    Int32 FuncStruct = 0;
                    GeoStruct = Convert.ToInt32(BindEmpList.Employee.GeoStruct_Id.ToString());
                    PayStruct = Convert.ToInt32(BindEmpList.Employee.PayStruct_Id.ToString());
                    FuncStruct = Convert.ToInt32(BindEmpList.Employee.FuncStruct_Id.ToString());

                    var chkk = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                       a.SwipeDate.Value.Date <= CurrentmonthEnd && a.MusterRemarks.LookupVal.ToUpper() != "PP" && a.MusterRemarks.LookupVal.ToUpper() != "WO" && a.MusterRemarks.LookupVal.ToUpper() != "HO" && a.MusterRemarks.LookupVal.ToUpper() != "LV").OrderBy(e => e.SwipeDate).ToList();
                    EmployeeLeave OEmployeeLeave = null;
                    OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == i).SingleOrDefault();
                    EmployeeAttendance OemloyeeAttendance = null;
                    OemloyeeAttendance = db.EmployeeAttendance.Where(e => e.Employee.Id == i).SingleOrDefault();

                    var empActionstruct = db.EmployeeAttendanceActionPolicyStruct
                        .Include(e => e.EmployeeAttendanceActionPolicyStructDetails)
                        .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula))
                         .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.PolicyName))
                        .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula.AttendanceAbsentPolicy))
                          .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula.AttendanceLeavePriority))
                        .Where(e => e.EmployeeAttendance_Id == OemloyeeAttendance.Id && e.EndDate == null).FirstOrDefault();
                    if (empActionstruct == null)
                    {
                        continue;
                        // Msg.Add(" Kindly Define Attendance Action Structure " + BindEmpList.Employee.EmpCode);  
                    }
                    var lvheadassign = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id)).ToList();
                    if (lvheadassign.Count() == 0)
                    {
                        continue;
                    }




                    foreach (var item in chkk)
                    {
                        // Shindudurg OD start 
                        OutDoorDutyReq outdooreq = db.OutDoorDutyReq.Where(e => e.EmployeeAttendance_Id == BindEmpList.Id && e.isCancel == false && e.ProcessedData_Id == item.Id).FirstOrDefault();
                        ProcessedData processdata = db.ProcessedData.Where(e => e.Id == item.Id).FirstOrDefault();

                        ProcessedData processdatanotodfill = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == item.Id && remark.Contains(e.MusterRemarks.LookupVal.ToString())).FirstOrDefault();

                        string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                        bool existschk = System.IO.Directory.Exists(requiredPathchk);
                        string localPathchk;
                        if (!existschk)
                        {
                            localPath = new Uri(requiredPathchk).LocalPath;
                            System.IO.Directory.CreateDirectory(localPath);
                        }
                        string pathchk = requiredPathchk + @"\ODACTIONONATTENDANCE" + ".ini";
                        localPathchk = new Uri(pathchk).LocalPath;
                        string Leave_code = "";
                        int Lvreqcnt = 0;
                        int paramcnt = 0;
                        using (var streamReader = new StreamReader(localPathchk))
                        {
                            string line;

                            while ((line = streamReader.ReadLine()) != null)
                            {
                                var NoofDay = line.Split('_')[0];

                                var LVFULLOrHALF = line.Split('_')[1];
                                if (NoofDay != "")
                                {
                                    int minday = Convert.ToInt32(NoofDay);
                                    if (outdooreq != null)
                                    {
                                        int reqday = (outdooreq.ReqDate.Value.Date - item.SwipeDate.Value.Date).Days + 1;

                                        if (reqday >= minday)
                                        {
                                            List<int> idslv = new List<int>();
                                            var lvhead = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id).ToList()).FirstOrDefault();
                                            //foreach (var lvhid in leaveheadseq)
                                            //{
                                            //    idslv.Add(Convert.ToInt32(lvhid)); 
                                            //}


                                            //var lvhead = db.LvHead.Where(e => idslv.Contains(e.Id)).OrderBy(e => e.Id).ToList();
                                            foreach (var LVH in lvhead)
                                            {

                                                var LvNewReqDebitLast = db.LvNewReq.Where(e => e.EmployeeLeave_Id == employeeLeave.Id && e.LeaveHead_Id == LVH).OrderByDescending(e => e.Id).FirstOrDefault();

                                                // new Leave Request update

                                                if (LvNewReqDebitLast != null && LvNewReqDebitLast.CloseBal > 0)
                                                {
                                                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                 new System.TimeSpan(0, 30, 0)))
                                                    {
                                                        double dbtday = 0;
                                                        if (LVFULLOrHALF.ToUpper().ToString() == "FULLSESSION")
                                                        {
                                                            dbtday = 1;
                                                        }
                                                        else
                                                        {
                                                            dbtday = 0.5;
                                                        }

                                                        LvWFDetails oLvWFDetails = new LvWFDetails
                                                        {
                                                            WFStatus = 5,
                                                            Comments = "Late OD Action On Leave",
                                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now }
                                                        };

                                                        List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                                                        oLvWFDetails_List.Add(oLvWFDetails);


                                                        LvNewReq newLvConvertobjrm = new LvNewReq
                                                        {
                                                            InputMethod = 0,
                                                            ReqDate = DateTime.Now,
                                                            CloseBal = (LvNewReqDebitLast.CloseBal - dbtday),
                                                            OpenBal = LvNewReqDebitLast.CloseBal,
                                                            IsLock = false,
                                                            LvLapsed = LvNewReqDebitLast.LvLapsed,
                                                            LVCount = LvNewReqDebitLast.LVCount + dbtday,
                                                            LvOccurances = LvNewReqDebitLast.LvOccurances + 1,
                                                            FromStat = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal == LVFULLOrHALF.ToUpper().ToString()).FirstOrDefault(),
                                                            ToStat = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal == LVFULLOrHALF.ToUpper().ToString()).FirstOrDefault(),
                                                            //CreditDays = oLvClosingData,
                                                            DebitDays = dbtday,
                                                            ToDate = item.SwipeDate.Value.Date,
                                                            FromDate = item.SwipeDate.Value.Date,
                                                            ResumeDate = item.SwipeDate.Value.AddDays(1),
                                                            LvCreditDate = LvNewReqDebitLast.LvCreditDate,
                                                            LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                                e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                            GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                            PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                            FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                            LeaveHead = db.LvHead.Where(e => e.Id == LVH).SingleOrDefault(),
                                                            Narration = "LEAVE REQUISITION",
                                                            Reason = "Late OD Action On Leave",
                                                            TrClosed = true,
                                                            LvCreditNextDate = LvNewReqDebitLast.LvCreditNextDate,
                                                            WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                            DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                            LvWFDetails = oLvWFDetails_List,
                                                        };
                                                        db.LvNewReq.Add(newLvConvertobjrm);
                                                        db.SaveChanges();
                                                        List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
                                                        _List_oLvNewReq.Add(db.LvNewReq.Find(newLvConvertobjrm.Id));



                                                        if (OEmployeeLeave == null)
                                                        {
                                                            EmployeeLeave OTEP = new EmployeeLeave()
                                                            {
                                                                Employee = db.Employee.Find(BindEmpList.Employee.Id),
                                                                LvNewReq = _List_oLvNewReq,
                                                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                                                            };


                                                            db.EmployeeLeave.Add(OTEP);
                                                            db.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            var aa = db.EmployeeLeave.Include(e => e.LvNewReq).Where(e => e.Id == OEmployeeLeave.Id).FirstOrDefault();
                                                            _List_oLvNewReq.AddRange(aa.LvNewReq);
                                                            aa.LvNewReq = _List_oLvNewReq;
                                                            db.EmployeeLeave.Attach(aa);
                                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                            db.SaveChanges();
                                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                        }


                                                        // OD Cancel update
                                                        AttWFDetails oAttWFDetails = new AttWFDetails
                                                        {
                                                            WFStatus = 4,
                                                            Comments = "Late OD Action On Leave",
                                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                                        };

                                                        List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                                                        oAttWFDetails_List.Add(oAttWFDetails);

                                                        outdooreq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                                        outdooreq.Reason = "Late OD Action On Leave" + " :Swipe Date" + item.SwipeDate.Value.Date;
                                                        outdooreq.isCancel = true;
                                                        outdooreq.TrClosed = true;
                                                        outdooreq.ProcessedData = null;
                                                        db.Entry(outdooreq).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();

                                                        outdooreq.WFDetails = oAttWFDetails_List;
                                                        db.OutDoorDutyReq.Attach(outdooreq);
                                                        db.Entry(outdooreq).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();

                                                        // Attendance remark update
                                                        RemarkConfig remarkConfig = db.RemarkConfig.Include(e => e.AlterMusterRemark).Include(e => e.PresentStatus).Include(e => e.MusterRemarks).Where(e => e.MusterRemarks.LookupVal.ToUpper() == "LV").FirstOrDefault();
                                                        processdata.MusterRemarks = remarkConfig.MusterRemarks;
                                                        processdata.PresentStatus = remarkConfig.PresentStatus;
                                                        processdata.ManualReason = "Late OD Action On Leave";
                                                        db.Entry(processdata).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();

                                                        ts.Complete();
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    continue;
                                                }







                                            }

                                        }

                                    }
                                    else// OD not fill
                                    {
                                        if (processdatanotodfill != null)
                                        {
                                            int reqday = (DateTime.Now.Date - item.SwipeDate.Value.Date).Days + 1;

                                            if (reqday >= minday)
                                            {
                                                List<int> idslv = new List<int>();
                                                var lvhead = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id).ToList()).FirstOrDefault();
                                                //foreach (var lvhid in leaveheadseq)
                                                //{
                                                //    idslv.Add(Convert.ToInt32(lvhid)); 
                                                //}


                                                //var lvhead = db.LvHead.Where(e => idslv.Contains(e.Id)).OrderBy(e => e.Id).ToList();
                                                foreach (var LVH in lvhead)
                                                {

                                                    var LvNewReqDebitLast = db.LvNewReq.Where(e => e.EmployeeLeave_Id == employeeLeave.Id && e.LeaveHead_Id == LVH).OrderByDescending(e => e.Id).FirstOrDefault();

                                                    // new Leave Request update

                                                    if (LvNewReqDebitLast != null && LvNewReqDebitLast.CloseBal > 0)
                                                    {
                                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                     new System.TimeSpan(0, 30, 0)))
                                                        {
                                                            double dbtday = 0;
                                                            if (LVFULLOrHALF.ToUpper().ToString() == "FULLSESSION")
                                                            {
                                                                dbtday = 1;
                                                            }
                                                            else
                                                            {
                                                                dbtday = 0.5;
                                                            }



                                                            LvNewReq newLvConvertobjrm = new LvNewReq
                                                            {
                                                                InputMethod = 0,
                                                                ReqDate = DateTime.Now,
                                                                CloseBal = (LvNewReqDebitLast.CloseBal - dbtday),
                                                                OpenBal = LvNewReqDebitLast.CloseBal,
                                                                IsLock = false,
                                                                LvLapsed = LvNewReqDebitLast.LvLapsed,
                                                                LVCount = LvNewReqDebitLast.LVCount + dbtday,
                                                                LvOccurances = LvNewReqDebitLast.LvOccurances + 1,
                                                                FromStat = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal == LVFULLOrHALF.ToUpper().ToString()).FirstOrDefault(),
                                                                ToStat = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal == LVFULLOrHALF.ToUpper().ToString()).FirstOrDefault(),
                                                                //CreditDays = oLvClosingData,
                                                                DebitDays = dbtday,
                                                                ToDate = item.SwipeDate.Value.Date,
                                                                FromDate = item.SwipeDate.Value.Date,
                                                                ResumeDate = item.SwipeDate.Value.AddDays(1),
                                                                LvCreditDate = LvNewReqDebitLast.LvCreditDate,
                                                                LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                                GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                                PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                                FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                                LeaveHead = db.LvHead.Where(e => e.Id == LVH).SingleOrDefault(),
                                                                Narration = "LEAVE REQUISITION",
                                                                Reason = "Not Fill OD Due to Action On Leave",
                                                                TrClosed = true,
                                                                LvCreditNextDate = LvNewReqDebitLast.LvCreditNextDate,
                                                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                                DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                            };
                                                            db.LvNewReq.Add(newLvConvertobjrm);
                                                            db.SaveChanges();
                                                            List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
                                                            _List_oLvNewReq.Add(db.LvNewReq.Find(newLvConvertobjrm.Id));



                                                            if (OEmployeeLeave == null)
                                                            {
                                                                EmployeeLeave OTEP = new EmployeeLeave()
                                                                {
                                                                    Employee = db.Employee.Find(BindEmpList.Employee.Id),
                                                                    LvNewReq = _List_oLvNewReq,
                                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                                                                };


                                                                db.EmployeeLeave.Add(OTEP);
                                                                db.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                var aa = db.EmployeeLeave.Include(e => e.LvNewReq).Where(e => e.Id == OEmployeeLeave.Id).FirstOrDefault();
                                                                _List_oLvNewReq.AddRange(aa.LvNewReq);
                                                                aa.LvNewReq = _List_oLvNewReq;
                                                                db.EmployeeLeave.Attach(aa);
                                                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                                db.SaveChanges();
                                                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                            }



                                                            // Attendance remark update
                                                            RemarkConfig remarkConfig = db.RemarkConfig.Include(e => e.AlterMusterRemark).Include(e => e.PresentStatus).Include(e => e.MusterRemarks).Where(e => e.MusterRemarks.LookupVal.ToUpper() == "LV").FirstOrDefault();
                                                            processdatanotodfill.MusterRemarks = remarkConfig.MusterRemarks;
                                                            processdatanotodfill.PresentStatus = remarkConfig.PresentStatus;
                                                            processdatanotodfill.ManualReason = "Not Fill OD Due to Action On Leave";
                                                            db.Entry(processdatanotodfill).State = System.Data.Entity.EntityState.Modified;
                                                            db.SaveChanges();

                                                            ts.Complete();
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }







                                                }

                                            }


                                        }
                                    }
                                }

                            }
                        }
                        // Shindudurg OD end

                    }


                }


                Msg.Add("Attendance Action Process for month : " + PayMonth);
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                // return this.Json(new { success = true, responseText = "Action Process for month :" + "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult ActiononAttendanceLvNewReq(string PayMonth)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    using (TransactionScope ts1 = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 30, 0)))
                    {
                        DateTime Pswipedate = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1);

                        List<int> ids = null;
                        int Processgrpid;
                        ids = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).Select(r => r.Id).ToList();


                        List<string> remark = new List<string>();
                        List<RemarkConfig> RC = db.RemarkConfig.Include(e => e.MusterRemarks).ToList();
                        foreach (var item in RC)
                        {
                            remark.Add(item.MusterRemarks.LookupVal.ToUpper());
                        }

                        foreach (var i in ids)
                        {
                            var employeeLeave = db.EmployeeLeave.Include(e => e.Employee).Where(e => e.Employee.Id == i).FirstOrDefault();


                            var BindEmpList = db.EmployeeAttendance.Select(e => new
                            {
                                Id = e.Id,
                                Employee = e.Employee,
                                EmpName = e.Employee.EmpName,
                                EmpOffInfo = e.Employee.EmpOffInfo,
                                ServiceBookDates = e.Employee.ServiceBookDates,
                                PayProcessGroup = e.Employee.EmpOffInfo.PayProcessGroup,
                                ProcessedData = e.ProcessedData.Select(r => new
                                {
                                    PresentStatus = r.PresentStatus,
                                    MusterRemarks = r.MusterRemarks,
                                    SwipeDate = r.SwipeDate,
                                    Id = r.Id,
                                    ManualReason = r.ManualReason
                                }).Where(r => r.SwipeDate >= Pswipedate).ToList()
                            }).Where(e => e.Employee.Id == i).FirstOrDefault();


                            Processgrpid = BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id;

                            var query1 = db.PayProcessGroup.Include(e => e.PayrollPeriod).Where(e => e.Id == Processgrpid)
                       .SingleOrDefault().PayrollPeriod.FirstOrDefault();

                            int startday = query1.StartDate;
                            int endday = query1.EndDate;
                            DateTime _PayMonth = Convert.ToDateTime("01/" + PayMonth);

                            DateTime end = Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date;
                            int daysInEndMonth = end.Day;
                            int daysInstartMonth = 1;
                            DateTime FromPeriod;
                            DateTime EndPeriod;
                            DateTime Currentmonthstart;
                            DateTime CurrentmonthEnd;
                            DateTime Prevmonthstart;
                            DateTime PrevmonthEnd;
                            int ProDays = 0;
                            int RetProDays = 0;
                            int daym = (Convert.ToDateTime("01/" + PayMonth).AddMonths(1).AddDays(-1).Date).Day;
                            Currentmonthstart = Convert.ToDateTime("01/" + PayMonth);


                            if (endday > daym)
                            {
                                endday = daym;
                            }
                            ProDays = daym - endday;
                            RetProDays = ProDays;
                            if (startday == daysInstartMonth && endday == daysInEndMonth)
                            {
                                FromPeriod = _PayMonth;
                                EndPeriod = Convert.ToDateTime(DateTime.DaysInMonth(_PayMonth.Year, _PayMonth.Month) + "/" + _PayMonth.Month + "/" + _PayMonth.Year);
                            }
                            else
                            {
                                DateTime prvmonth = Convert.ToDateTime("01/" + PayMonth).AddMonths(-1).Date;
                                startday = endday + 1;
                                string pmonth = prvmonth.ToString("MM/yyyy");
                                FromPeriod = Convert.ToDateTime(startday + "/" + pmonth);


                                EndPeriod = Convert.ToDateTime(endday + "/" + PayMonth);

                            }
                            CurrentmonthEnd = EndPeriod;
                            Prevmonthstart = FromPeriod;
                            if (startday == daysInstartMonth && endday == daysInEndMonth)
                            {
                                PrevmonthEnd = FromPeriod.AddDays(ProDays);
                            }
                            else
                            {
                                PrevmonthEnd = Convert.ToDateTime("01/" + FromPeriod.ToString("MM/yyyy")).AddMonths(1).AddDays(-1).Date;
                            }

                            if ((BindEmpList.Employee != null && BindEmpList.Employee.EmpOffInfo != null && BindEmpList.Employee.EmpOffInfo.PayProcessGroup != null
                           && BindEmpList.Employee.EmpOffInfo.PayProcessGroup.Id != Processgrpid) ||
                           (BindEmpList.Employee != null && BindEmpList.Employee.ServiceBookDates != null && BindEmpList.Employee.ServiceBookDates.ServiceLastDate != null))
                            {
                                continue;
                            }

                            Int32 GeoStruct = 0;
                            Int32 PayStruct = 0;
                            Int32 FuncStruct = 0;
                            GeoStruct = Convert.ToInt32(BindEmpList.Employee.GeoStruct_Id.ToString());
                            PayStruct = Convert.ToInt32(BindEmpList.Employee.PayStruct_Id.ToString());
                            FuncStruct = Convert.ToInt32(BindEmpList.Employee.FuncStruct_Id.ToString());

                            var chkk = BindEmpList.ProcessedData.Where(a => a.SwipeDate.Value.Date >= Prevmonthstart &&
                                                                               a.SwipeDate.Value.Date <= CurrentmonthEnd && a.MusterRemarks.LookupVal.ToUpper() == "LF" || a.MusterRemarks.LookupVal.ToUpper() == "LH").OrderBy(e => e.SwipeDate).ToList();

                            EmployeeLeave OEmployeeLeave = null;
                            OEmployeeLeave = db.EmployeeLeave.Where(e => e.Employee.Id == i).SingleOrDefault();
                            EmployeeAttendance OemloyeeAttendance = null;
                            OemloyeeAttendance = db.EmployeeAttendance.Where(e => e.Employee.Id == i).SingleOrDefault();

                            var empActionstruct = db.EmployeeAttendanceActionPolicyStruct
                                .Include(e => e.EmployeeAttendanceActionPolicyStructDetails)
                                .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula))
                                 .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.PolicyName))
                                .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula.AttendanceAbsentPolicy))
                                  .Include(e => e.EmployeeAttendanceActionPolicyStructDetails.Select(x => x.AttendanceActionPolicyFormula.AttendanceLeavePriority))
                                .Where(e => e.EmployeeAttendance_Id == OemloyeeAttendance.Id && e.EndDate == null).FirstOrDefault();
                            if (empActionstruct == null)
                            {
                                continue;
                                // Msg.Add(" Kindly Define Attendance Action Structure " + BindEmpList.Employee.EmpCode);  
                            }
                            var lvheadassign = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id)).ToList();
                            if (lvheadassign.Count() == 0)
                            {
                                continue;
                            }
                            Boolean actiononleaveifbalance = false;
                            foreach (var item in chkk)
                            {
                                ProcessedData processdatanotodfill = db.ProcessedData.Include(e => e.MusterRemarks).Where(e => e.Id == item.Id && remark.Contains(e.MusterRemarks.LookupVal.ToString())).FirstOrDefault();

                                if (processdatanotodfill != null)
                                {
                                    actiononleaveifbalance = false;

                                    List<int> idslv = new List<int>();
                                    var lvhead = empActionstruct.EmployeeAttendanceActionPolicyStructDetails.Where(x => x.PolicyName.LookupVal.ToUpper() == "LEAVE PRIORITY").Select(e => e.AttendanceActionPolicyFormula.AttendanceLeavePriority.OrderBy(y => y.Seqno).Select(m => m.LvHead_Id).ToList()).FirstOrDefault();

                                    foreach (var LVH in lvhead)
                                    {

                                        var LvNewReqDebitLast = db.LvNewReq.Where(e => e.EmployeeLeave_Id == employeeLeave.Id && e.LeaveHead_Id == LVH).OrderByDescending(e => e.Id).FirstOrDefault();

                                        // new Leave Request update

                                        if (LvNewReqDebitLast != null && LvNewReqDebitLast.CloseBal > 0)
                                        {
                                            double dbtday = 0; LookupValue getState = null;
                                            if (item.MusterRemarks.LookupVal.ToUpper() == "LH")
                                            {
                                                dbtday = 0.5;
                                                getState = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FIRSTSESSION".ToUpper()).FirstOrDefault();
                                            }
                                            else
                                            {
                                                dbtday = 1;
                                                getState = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "478").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "FULLSESSION".ToUpper()).FirstOrDefault();
                                            }
                                            if (dbtday <= LvNewReqDebitLast.CloseBal)
                                            {

                                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                             new System.TimeSpan(0, 30, 0)))
                                                {



                                                    actiononleaveifbalance = true;
                                                    LvWFDetails oLvWFDetails = new LvWFDetails
                                                    {
                                                        WFStatus = 5,
                                                        Comments = "Late Action on Attendance",
                                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false, CreatedOn = DateTime.Now }
                                                    };

                                                    List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                                                    oLvWFDetails_List.Add(oLvWFDetails);

                                                    LvNewReq newLvConvertobjrm = new LvNewReq
                                                    {
                                                        InputMethod = 0,
                                                        ReqDate = DateTime.Now,
                                                        CloseBal = (LvNewReqDebitLast.CloseBal - dbtday),
                                                        OpenBal = LvNewReqDebitLast.CloseBal,
                                                        IsLock = false,
                                                        LvLapsed = LvNewReqDebitLast.LvLapsed,
                                                        LVCount = LvNewReqDebitLast.LVCount + dbtday,
                                                        LvOccurances = LvNewReqDebitLast.LvOccurances + 1,
                                                        FromStat = getState,
                                                        ToStat = getState,

                                                        DebitDays = dbtday,
                                                        ToDate = item.SwipeDate.Value.Date,
                                                        FromDate = item.SwipeDate.Value.Date,
                                                        ResumeDate = item.SwipeDate.Value.AddDays(1),
                                                        LvCreditDate = LvNewReqDebitLast.LvCreditDate,
                                                        LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                            e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                        GeoStruct = db.GeoStruct.Where(e => e.Id == GeoStruct).SingleOrDefault(),
                                                        PayStruct = db.PayStruct.Where(e => e.Id == PayStruct).SingleOrDefault(),
                                                        FuncStruct = db.FuncStruct.Where(e => e.Id == FuncStruct).SingleOrDefault(),
                                                        LeaveHead = db.LvHead.Where(e => e.Id == LVH).SingleOrDefault(),
                                                        Narration = "LEAVE REQUISITION",
                                                        Reason = "Late Action on Attendance",
                                                        TrClosed = true,
                                                        LvCreditNextDate = LvNewReqDebitLast.LvCreditNextDate,
                                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault(),
                                                        DBTrack = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now },
                                                        LvWFDetails = oLvWFDetails_List,
                                                    };
                                                    db.LvNewReq.Add(newLvConvertobjrm);
                                                    db.SaveChanges();
                                                    List<LvNewReq> _List_oLvNewReq = new List<LvNewReq>();
                                                    _List_oLvNewReq.Add(db.LvNewReq.Find(newLvConvertobjrm.Id));



                                                    if (OEmployeeLeave == null)
                                                    {
                                                        EmployeeLeave OTEP = new EmployeeLeave()
                                                        {
                                                            Employee = db.Employee.Find(BindEmpList.Employee.Id),
                                                            LvNewReq = _List_oLvNewReq,
                                                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }

                                                        };


                                                        db.EmployeeLeave.Add(OTEP);
                                                        db.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        var aa = db.EmployeeLeave.Include(e => e.LvNewReq).Where(e => e.Id == OEmployeeLeave.Id).FirstOrDefault();
                                                        _List_oLvNewReq.AddRange(aa.LvNewReq);
                                                        aa.LvNewReq = _List_oLvNewReq;
                                                        db.EmployeeLeave.Attach(aa);
                                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();
                                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                                    }


                                                    processdatanotodfill.OldMusterRemarks_Id = processdatanotodfill.MusterRemarks_Id;
                                                    // Attendance remark update
                                                    RemarkConfig remarkConfig = db.RemarkConfig.Include(e => e.AlterMusterRemark).Include(e => e.PresentStatus).Include(e => e.MusterRemarks).Where(e => e.MusterRemarks.LookupVal.ToUpper() == "LV").FirstOrDefault();
                                                    processdatanotodfill.MusterRemarks = remarkConfig.MusterRemarks;
                                                    processdatanotodfill.PresentStatus = remarkConfig.PresentStatus;
                                                    processdatanotodfill.ManualReason = "Late Action On Leave";
                                                    db.Entry(processdatanotodfill).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();

                                                    ts.Complete();
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                    }
                                    if (actiononleaveifbalance == false)
                                    {
                                        processdatanotodfill.OldMusterRemarks_Id = processdatanotodfill.MusterRemarks_Id;
                                        db.Entry(processdatanotodfill).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();


                                    }
                                }
                            }


                        }
                        ts1.Complete();
                        Msg.Add("Attendance Action on LV Process for month : " + PayMonth);
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };

                Msg.Add(ex.Message);

            }
            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult DeleteProcess(string forwardata, string PayMonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg1 = new List<string>();
                try
                {
                    List<int> ids = null;
                    if (forwardata != null && forwardata != "0" && forwardata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwardata);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    foreach (var i in ids)
                    {
                        //  OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Where(r => r.Id == i).SingleOrDefault();

                        OEmployeePayroll = db.EmployeePayroll.AsNoTracking().Where(e => e.Employee.Id == i).Include(e => e.SalaryT).SingleOrDefault();


                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {

                            //using (DataBaseContext db2 = new DataBaseContext())
                            //{

                            // var SalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();
                            var SalT = db.SalaryT.Where(e => e.PayMonth == PayMonth && e.EmployeePayroll.Id == OEmployeePayroll.Id)
                              .Include(e => e.SalEarnDedT).Include(e => e.SalEarnDedT.Select(r => r.NegSalData))
                              .SingleOrDefault();
                            if (SalT.ReleaseDate != null)
                            {
                                Msg1.Add("Salary Released For Employee Code=" + OEmployeePayroll.Employee.EmpCode + ",Unable To Delete" + "\n");
                                continue;
                                //  return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //  return Json(new { success = true, responseText = "Salary released for employee " + OEmployeePayroll.Employee.EmpCode + ". Unable To Delete" }, JsonRequestBehavior.AllowGet);
                            }
                            if (SalT.SalEarnDedT.Where(r => r.NegSalData != null && r.NegSalData.ReleaseFlag == true).Count() > 0)
                            {
                                Msg1.Add("Salary has been changed For Employee Code=" + OEmployeePayroll.Employee.EmpCode + ",Unable To Delete" + "\n");
                                continue;
                            }
                            if (SalT != null)
                            {
                                SalaryGen.DeleteSalary(SalT.Id, PayMonth);
                            }
                            //}
                            ts.Complete();

                        }

                    }
                }
                catch (Exception ex)
                {
                    //  Msg.Add(ex.Message);
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = ex.Message,
                        ExceptionStackTrace = ex.StackTrace,
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                return Json(new { success = true, responseText = "Salary deleted for selected employee." }, JsonRequestBehavior.AllowGet);
            }
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string PayMonth { get; set; }
            public double TotalEarning { get; set; }
            public double TotalDeduction { get; set; }
            public double TotalNet { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BGridData> SalaryList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }

                //var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT)
                //    .Where(e => e.SalaryT.Any(u => u.PayMonth != "01/2017")).ToList();
                using (DataBaseContext db = new DataBaseContext())
                {

                    var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.SalaryT).ToList();

                    foreach (var z in BindEmpList)
                    {
                        if (z.SalaryT != null)
                        {

                            foreach (var Sal in z.SalaryT)
                            {
                                if (Sal.PayMonth == PayMonth)
                                {
                                    view = new P2BGridData()
                                    {
                                        Id = z.Employee.Id,
                                        Code = z.Employee.EmpCode,
                                        Name = z.Employee.EmpName.FullNameFML,
                                        PayMonth = Sal.PayMonth,
                                        TotalEarning = Sal.TotalEarning,
                                        TotalDeduction = Sal.TotalDeduction,
                                        TotalNet = Sal.TotalNet
                                    };
                                    model.Add(view);
                                }
                            }
                        }

                    }

                    SalaryList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = SalaryList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            //jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                            //    || (e.Code.ToString().Contains(gp.searchString))
                            //    || (e.Name.ToString().Contains(gp.searchString))
                            //    || (e.PayMonth.ToString().Contains(gp.searchString))
                            //    || (e.TotalEarning.ToString().Contains(gp.searchString))
                            //    || (e.TotalDeduction.ToString().Contains(gp.searchString))
                            //    || (e.TotalNet.ToString().Contains(gp.searchString))
                            //    ).Select(a => new Object[] { a.Id, a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet }).ToList();


                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                                  || (e.Code.ToString().Contains(gp.searchString.ToUpper())) || (e.Name.ToString().Contains(gp.searchString.ToUpper()))
                                    || (e.PayMonth.ToString().Contains(gp.searchString.ToUpper())) || (e.TotalEarning.ToString().Contains(gp.searchString.ToUpper()))
                                      || (e.TotalDeduction.ToString().Contains(gp.searchString.ToUpper())) || (e.TotalNet.ToString().Contains(gp.searchString.ToUpper()))

                                  )
                              .Select(a => new Object[] { a.Id, a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet }).ToList();



                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = SalaryList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                             gp.sidx == "TotalEarning" ? c.TotalEarning.ToString() :
                                             gp.sidx == "TotalDeduction" ? c.TotalDeduction.ToString() :
                                             gp.sidx == "TotalNet" ? c.TotalNet.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name, a.PayMonth, a.TotalEarning, a.TotalDeduction, a.TotalNet }).ToList();
                        }
                        totalRecords = SalaryList.Count();
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public class empdetails
        {
            public string ded { get; set; }
            public string amount { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
            public string Amount { get; set; }
            public int SalaryTId { get; set; }
        }
        public class empdetails1
        {
            public string earn { get; set; }
            public string amt { get; set; }
        }

        public ActionResult getdata(FormCollection form)
        {

            List<string> Msg = new List<string>();
            string Emp = form["employee-table1"] == "0" ? "" : form["employee-table1"];
            string PayMonth = form["txtPayMonth1"] == "0" ? "" : form["txtPayMonth1"];

            int ids = 0;
            if (Emp != null && Emp != "0" && Emp != "false")
            {
                ids = int.Parse(Emp);
            }
            else
            {

                Msg.Add(" Kindly select employee  ");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

            Employee OEmployee = null;
            EmployeePayroll OEmployeePayroll = null;
            CompanyPayroll OCompanyPayroll = null;


            using (DataBaseContext db = new DataBaseContext())
            {

                OEmployee = db.Employee.Include(e => e.EmpName).Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                   .Where(r => r.Id == ids).SingleOrDefault();

                OEmployeePayroll = db.EmployeePayroll.Include(e => e.SalaryT).Where(e => e.Employee.Id == ids).SingleOrDefault();

                OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployee.GeoStruct.Company.Id).SingleOrDefault();

                var SalT = OEmployeePayroll.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                var Empnm = (OEmployee.EmpCode + " - " + OEmployee.EmpName.FName);

                var sal = db.EmployeePayroll.Include(e => e.SalaryT).Include(e => e.SalaryT.Select(r => r.SalEarnDedT))
                  .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead)))
                    .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.Type)))
                     .Include(e => e.SalaryT.Select(r => r.SalEarnDedT.Select(t => t.SalaryHead.SalHeadOperationType)))
                  .Include(e => e.Employee)
                                     .Where(e => e.Employee.Id == OEmployee.Id)
                                         .SingleOrDefault();
                var b = sal.SalaryT.Where(e => e.PayMonth == PayMonth).SingleOrDefault();

                List<empdetails1> p = new List<empdetails1>();
                {
                    foreach (var item in b.SalEarnDedT.Where(c => c.SalaryHead.Type.LookupVal == "Earning"))
                    {
                        p.Add(new empdetails1
                        {
                            earn = item.SalaryHead.Name.ToString(),
                            amt = item.StdAmount.ToString(),

                        });
                    }

                }
                var d = b.SalEarnDedT.Where(c => c.SalaryHead.Type.LookupVal == "Deduction" &&
                    (c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "EPF" || c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PT" || c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ITAX"
                        || c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "LWF" || c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "ESIC" || c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "CPF" || c.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "PENSION"
                        || c.SalaryHead.Type.LookupVal != "Earning")).ToList();

                List<empdetails> a = new List<empdetails>();
                {
                    foreach (var item in d)
                    {
                        a.Add(new empdetails
                        {
                            Id = item.Id,
                            Name = item.SalaryHead.Name.ToString(),
                            Amount = item.Amount.ToString(),
                            Code = item.SalaryHead.Code.ToString(),
                            SalaryTId = b.Id,
                        });
                    }

                }
                var SalaryTId = b.Id;
                var s1 = SalT.TotalEarning;
                var s2 = SalT.TotalDeduction;
                var s3 = SalT.TotalNet;
                var result = new { Name = Empnm, Sal = p, Salded = a, totearn = s1, totded = s2, grossearn = s3, SalaryTId = SalaryTId };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public class EditDataClass
        {
            public string Id { get; set; }
            public string Val { get; set; }
        }
        public ActionResult editdata(List<EditDataClass> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                if (data.Count > 0)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (var item in data)
                        {
                            var id = Convert.ToInt32(item.Id);
                            var Val = Convert.ToDouble(item.Val);
                            var qurey = db.SalEarnDedT.Where(e => e.Id == id).SingleOrDefault();
                            qurey.Amount = Val;
                            db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        }
                        ts.Complete();
                        return Json(new { success = true, responseText = "ReCord Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);

                }
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

        public class EmpManulattChildDataClass
        {
            public int Id { get; set; }
            public string TimingCode { get; set; }
            public string SwipeDate { get; set; }
            public string InTime { get; set; }
            public string OutTime { get; set; }
            public string MusterRemarks { get; set; }
            public string IsLocked { get; set; }
            public string EarlyCount { get; set; }
            public string LateCount { get; set; }
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

        public ActionResult Get_ManualAttendanceProcessData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAttendance.Include(e => e.ProcessedData).Include(e => e.ProcessedData.Select(q => q.TimingCode))
                        .Include(e => e.ProcessedData.Select(q => q.MusterRemarks)).Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<EmpManulattChildDataClass> returndata = new List<EmpManulattChildDataClass>();
                        foreach (var item in db_data.ProcessedData.ToList())
                        {

                            returndata.Add(new EmpManulattChildDataClass
                            {
                                Id = item.Id,
                                TimingCode = item.TimingCode != null ? item.TimingCode.TimingCode.ToString() : "",
                                SwipeDate = item.SwipeDate != null ? item.SwipeDate.Value.ToString("dd/MM/yyyy") : "",
                                InTime = item.InTime != null ? item.InTime.Value.ToShortTimeString() : "",
                                OutTime = item.OutTime != null ? item.OutTime.Value.ToShortTimeString() : "",
                                MusterRemarks = item.MusterRemarks != null ? item.MusterRemarks.LookupVal.ToString() : "",
                                IsLocked = item.IsLocked != null ? item.IsLocked.ToString() : "",
                                EarlyCount = item.EarlyCount != null ? item.EarlyCount.ToString() : "",
                                LateCount = item.LateCount != null ? item.LateCount.ToString() : "",
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