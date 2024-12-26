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
using System.Diagnostics;
using DocumentFormat.OpenXml.Office2010.Excel;
using P2B.UTILS;
using System.IO;
using System.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Leave;

namespace P2BUltimate.Controllers.Attendance.MainController
{
    public class FutureODController : Controller
    {
        //
        // GET: /FutureOD/
        public ActionResult Index()
        {
            return View("~/Views/Attendance/MainViews/FutureOD/Index.cshtml");
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

        public class FutureODChildDataClass
        {
            public int Id { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string ReqDate { get; set; }
            public string Reason { get; set; }

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

        public ActionResult Get_EmpFutureODData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAttendance.Include(e => e.FutureOD)
                        .Include(e => e.FutureOD.Select(q => q.ProcessedData))
                       .Where(e => e.Employee.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<FutureODChildDataClass> returndata = new List<FutureODChildDataClass>();
                        foreach (var item in db_data.FutureOD.ToList())
                        {
                            //foreach (var item1 in item.ReportingTimingStruct.ToList())
                            //{
                            returndata.Add(new FutureODChildDataClass
                            {
                                Id = item.Id,
                                ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToShortDateString() : "",
                                FromDate = item.FromDate != null ? item.FromDate.Value.ToShortDateString() : "",
                                ToDate = item.ToDate != null ? item.ToDate.Value.ToShortDateString() : "",
                                Reason = item.Reason != null ? item.Reason : "",
                            });
                            // }
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

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(FutureOD FOD, FormCollection form) //Create submit
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {
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
                // Future OD and past OD can Apply because they can not apply OD in system in remark analysis futureod is false,So Sir Given Permission for Past OD Enter
                //if (FOD.FromDate < DateTime.Now || FOD.ToDate < DateTime.Now)
                //{
                //    Msg.Add("Future OD can be applied for Future Dates only.");
                //    return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}
                
                if (FOD.FromDate > FOD.ToDate)
                {
                    Msg.Add("To Date should be greater than From Date.");
                    return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                foreach (var id in ids)
                {
                    DateTime mFromPeriod = Convert.ToDateTime(FOD.FromDate);
                    DateTime mEndDate = Convert.ToDateTime(FOD.ToDate);
                    var emp = db.Employee.Where(e => e.Id == id).FirstOrDefault();
                    for (DateTime mTempDate = mFromPeriod; mTempDate <= mEndDate; mTempDate = mTempDate.AddDays(1))
                    {

                        int FutureOD = db.FutureOD.Where(e => e.EmployeeAttendance.Employee.Id == id && e.FromDate.Value <= mTempDate.Date && e.ToDate.Value >= mTempDate.Date
                                                   && (e.isCancel == false && e.TrReject == false) ).Select(e => e.Id).SingleOrDefault();
                        if (FutureOD > 0)
                        {
                            Msg.Add("You have already enter " + mTempDate.Date + " for this employee " + emp.EmpCode);
                            return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        // Leave Apply then od can not apply start

                        EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                       .Where(e => e.Employee.Id == id).OrderBy(e => e.Id).SingleOrDefault();

                        if (oEmployeeLeave != null)
                        {
                            // var LvCalendarFilter = oEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == OLeaveCalendar.Id).OrderBy(e => e.Id).ToList();
                            var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                            var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                            var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                            var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();

                            if (listLvs.Where(e => e.FromDate <= mTempDate && e.ToDate >= mTempDate).Count() != 0)
                            {
                                //already exits
                                Msg.Add("You have already apply leave on " + mTempDate.ToShortDateString() + " So you can not apply Future OD.");
                                return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }

                        }
                        // Leave Apply then od can not apply end


                    }
                }
                AttWFDetails oAttWFDetails = new AttWFDetails
                {
                    WFStatus = 5,
                    Comments = FOD.Reason.ToString(),
                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                };

                List<AttWFDetails> oAttWFDetails_List = new List<AttWFDetails>();
                oAttWFDetails_List.Add(oAttWFDetails);
                FOD.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                foreach (var id in ids)
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                           new System.TimeSpan(0, 30, 0)))
                        {
                            try
                            {

                                FutureOD OFutureOD = new FutureOD()
                                {
                                   
                                    FromDate = FOD.FromDate,
                                    Reason = FOD.Reason,
                                    ReqDate = FOD.ReqDate,
                                    InputMethod = 1,
                                    ToDate = FOD.ToDate,
                                    DBTrack = FOD.DBTrack,
                                    TrClosed = true,
                                    WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                    WFDetails = oAttWFDetails_List
                                };

                                db.FutureOD.Add(OFutureOD);
                                db.SaveChanges();
                                
                                var EmpAttendance = db.EmployeeAttendance.Include(e => e.FutureOD)
                                    .Include(e => e.ProcessedData).Where(e => e.Employee.Id == id).SingleOrDefault();

                                if (EmpAttendance != null && EmpAttendance.FutureOD != null)
                                {
                                    if (EmpAttendance.FutureOD != null)
                                    {
                                        EmpAttendance.FutureOD.Add(OFutureOD);
                                    }
                                    else
                                    {
                                        EmpAttendance.FutureOD = new List<FutureOD> { OFutureOD };
                                    }
                                }
                                else
                                {
                                    var oEmpAttendace = new EmployeeAttendance();
                                    oEmpAttendace.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                    oEmpAttendace.FutureOD = new List<FutureOD> { OFutureOD };
                                }
                                db.Entry(EmpAttendance).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();


                                ts.Complete();


                            }
                            catch (DataException ex)
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
                                Msg.Add(ex.InnerException.Message.ToString());
                                return Json(new { status = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }
            Msg.Add("Data Saved Successfully"); 
            return this.Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet );
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


        [HttpPost]
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    FutureOD OFutureOD = db.FutureOD.Include(e => e.WFDetails).Where(e => e.Id == data).FirstOrDefault();
                    AttWFDetails OWFDet = OFutureOD.WFDetails.OrderByDescending(e => e.Id).FirstOrDefault();
                    EmployeeAttendance OEmpAtt = db.EmployeeAttendance.Find(OFutureOD.EmployeeAttendance_Id);
                    List<int> EmpIds = new List<int>();
                    EmpIds.Add(Convert.ToInt32(OEmpAtt.Employee_Id));
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            if (OWFDet != null && OWFDet.WFStatus == 0)
                            {
                                db.FutureOD.Attach(OFutureOD);
                                db.Entry(OFutureOD).State = System.Data.Entity.EntityState.Deleted;
                                db.SaveChanges();

                                ReturnData_AttendanceProcess returnDATA = new ReturnData_AttendanceProcess();

                                var ShowMessageCode = "";
                                var ShowMessage = "";

                                ServiceResultList<ReturnData_AttendanceProcess> responseDeserializeData = new ServiceResultList<ReturnData_AttendanceProcess>();
                                string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                                using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                                {
                                    var response = p2BHttpClient.request("ETRM/getUserAttendanceMusterProcessRequest",
                                        new ETRM_AttendanceProcess() { Emp_Ids = EmpIds, FromPeriod = OFutureOD.FromDate, ToPeriod = OFutureOD.ToDate });

                                    var dataRes = response.Content.ReadAsStringAsync().Result;
                                    // var result = data.;

                                    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResultList<ReturnData_AttendanceProcess>>(response.Content.ReadAsStringAsync().Result);


                                    ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                                    ShowMessage = responseDeserializeData.Message.ToString();

                                    if (responseDeserializeData != null && ShowMessageCode != "OK")
                                    {
                                        if (responseDeserializeData.Data.Count() > 0)
                                        {
                                            foreach (var item in responseDeserializeData.Data)
                                            { 
                                                Msg.Add(item.ErrMsg);

                                            } 
                                        }
                                         
                                    }


                                }

                                //db.Entry(PromoServBook).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                return this.Json(new { status = true, responseText = Msg, JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                return this.Json(new { status = false, responseText = "This record is already sanctioned so you can't delete the record.", JsonRequestBehavior.AllowGet });
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
                            // List<string> Msg = new List<string>();
                            Msg.Add(ex.Message);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

	}
}