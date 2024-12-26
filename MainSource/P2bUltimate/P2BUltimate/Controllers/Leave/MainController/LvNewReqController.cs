using Leave;
using P2b.Global;
using P2BUltimate.App_Start;
using System;
using System.Collections.Generic;
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
using P2BUltimate.Process;
using P2BUltimate.Security;
using System.Diagnostics;
using P2BUltimate.Process;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using P2B.UTILS;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Configuration;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvNewReqController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Leave/MainViews/LvNewReq/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Leave/_LvNewReqGridPartial.cshtml");
        }
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult GetLeaveHalfdayappl(string data, string data2) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Empids = int.Parse(data2);
                int EmployeeLvStructids = db.EmployeeLeave.Where(e => e.Employee_Id == Empids).FirstOrDefault().Id;
                var OLvSalStruct = db.EmployeeLvStruct
                                        .Include(e => e.EmployeeLvStructDetails)
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvDebitPolicy))
                                        .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                            .Where(e => e.EndDate == null && e.EmployeeLeave_Id == EmployeeLvStructids).SingleOrDefault();//.ToList();

                if (OLvSalStruct == null)
                {
                    return JavaScript("alert('Please Create Leave structure for this employee ..!!')");

                }
                int LvHead = int.Parse(data);
                // int Emp = int.Parse(data2);
                var lvheadhalfdayapp = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == LvHead && e.LvHeadFormula != null && e.LvHeadFormula.LvDebitPolicy != null).Select(r => r.LvHeadFormula.LvDebitPolicy).FirstOrDefault();

                // var lvheadhalfdayapp = db.LvDebitPolicy.Include(a => a.LvHead).Where(a => a.LvHead.Id == LvHead).SingleOrDefault();
                if (lvheadhalfdayapp != null)
                {
                    if (lvheadhalfdayapp.HalfDayAppl == true)
                    {

                        var fs = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION" || a.LookupVal.ToUpper() == "FIRSTSESSION" || a.LookupVal.ToUpper() == "SECONDSESSION").Distinct().ToList();
                        return Json(new SelectList(fs, "ID", "LookupVal"), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        var fs = db.LookupValue.Where(a => a.LookupVal.ToUpper() == "FULLSESSION").ToList();
                        return Json(new SelectList(fs, "ID", "LookupVal"), JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return JavaScript("alert('Please Define Leave Debit Policy ..!!')");
                }




            }
        }

        public ActionResult GetLookupInchargenew(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.EmpCode }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBranchInfo(string EmpId) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Emp = int.Parse(EmpId);
                var Empinfo = db.Employee.Include(e => e.GeoStruct.Location)
                                  .Include(e => e.GeoStruct.Location.LocationObj)
                                  .Include(e => e.PayStruct.Grade)
                                  .Include(e => e.FuncStruct.Job)
                                  .Where(e => e.Id == Emp).SingleOrDefault();

                return Json(new { Locname = Empinfo.GeoStruct.Location.LocationObj.LocDesc, Jobname = Empinfo.FuncStruct.Job.Name, Gradename = Empinfo.PayStruct.Grade.Name }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult PopulateDropDownListIncharge(string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Emp = int.Parse(data2);
                var excep = db.Employee.Include(e => e.EmpName).Where(e => e.Id == Emp).ToList();
                var query = db.Employee.Include(e => e.EmpName).Include(e => e.ServiceBookDates).Where(e => e.ServiceBookDates.ServiceLastDate == null).OrderBy(e => e.EmpCode).ToList();
                var query2 = query.Except(excep);
                //   var qurey = db.Calendar.Include(e => e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(query2, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeaveBalance(string LvHeadId, string EmpId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int LvHead = int.Parse(LvHeadId);
                int Emp = int.Parse(EmpId);
                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                    .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                    .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                // Leave calendar id default year checking discard and when enter new leave open balance at that time lvnewreq table data auto save
                //so openbal table checking not required
                var openballvnewreq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead).OrderByDescending(e => e.Id).FirstOrDefault();
                var PrevReq1 = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead)
               .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.CloseBal + openballvnewreq.LVCount, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                if (PrevReq1 != null)
                {
                    return Json(PrevReq1, JsonRequestBehavior.AllowGet);

                }



                //var openbal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvHead && e.LvCalendar.Id == LvCalendar.Id).LastOrDefault();

                //if (openbal == null)
                //{
                //    var openballvnewreq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id).OrderByDescending(e => e.Id).FirstOrDefault();
                //    var PrevReq1 = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id

                //   )
                //   .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.OpenBal + openballvnewreq.CreditDays-openballvnewreq.LvLapsed, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                //    return Json(PrevReq1, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id
                //        //&& e.IsCancel == false
                //   )
                //   .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openbal.LvOpening, LvClosing = e.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                //    return Json(PrevReq, JsonRequestBehavior.AllowGet);
                //    if (PrevReq != null)
                //    {
                //        var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvHead && e.LvCalendar.Id == LvCalendar.Id).SingleOrDefault();

                //        return Json(PrevOpenBal, JsonRequestBehavior.AllowGet);
                //    }
                //}

            }
            return null;
        }
        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new { Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString() }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        //Add this
        [HttpPost]
        public ActionResult GetLookupCalendar(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.Calendar.Include(e => e.Name).Where(e => !e.Id.ToString().Contains(a.ToString()) && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }


                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        public class ReturnData_LeaveValidation
        {
            public double DebitDays { get; set; }

            public int ErrNo { get; set; }

            public int InfoNo { get; set; }

            public double LvnewReqprefix { get; set; }

            public double LvnewReqSuffix { get; set; }

            public bool PrefixSufix { get; set; }

            public bool IsLvDebitSharing { get; set; }

            public bool IsLvHolidayWeeklyoffExclude { get; set; }

            public double LvCountPrefixSuffix { get; set; }

            public double LvFinalPrefixSuffixCount { get; set; }

            public bool IsCertAppl { get; set; }
            public bool IsCertOptional { get; set; }
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

        public class ServiceResult<T>
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
            public T Data { get; set; }
        }

        public class ELMS_Lv_DebitDays
        {
            public int Emp_Id { get; set; }

            public string Emp_Code { get; set; }

            public int? Lv_Head_Id { get; set; }

            public string Lv_Head { get; set; }

            public DateTime? ReqDate { get; set; }

            public DateTime? FromDate { get; set; }

            public DateTime? ToDate { get; set; }

            public int? FromStat_Id { get; set; }

            public string FromStat { get; set; }

            public int? ToStat_Id { get; set; }

            public string ToStat { get; set; }
        }

        public ActionResult Process(LvNewReq LvReq, FormCollection form)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                    //string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
                    string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
                    int ids = 0;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        //ids = one_ids(Emp);
                        ids = Convert.ToInt32(Emp);
                    }
                    else
                    {
                        Msg.Add("  Please Select Employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                    }
                    //var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                    //if (calendar != null && calendar != "")
                    //{
                    var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    LvReq.LeaveCalendar = Cal;
                    // }
                    if (FrmStat != null && FrmStat != "")
                    {
                        var id = Convert.ToInt32(FrmStat);
                        var value = db.LookupValue.Where(e => e.Id == id).FirstOrDefault();
                        LvReq.FromStat = value;
                    }
                    if (Tostat != null && Tostat != "")
                    {
                        var id = Convert.ToInt32(Tostat);
                        var value = db.LookupValue.Where(e => e.Id == id).FirstOrDefault();
                        LvReq.ToStat = value;
                    }
                    if (LvHeadList != null && LvHeadList != "")
                    {
                        var id = Convert.ToInt32(LvHeadList);
                        var value = db.LvHead.Where(e => e.Id == id).FirstOrDefault();
                        LvReq.LeaveHead = value;
                    }
                    Session["FilePath"] = null;
                    Session["IsCertAppl"] = null;
                    Session["IsCertOptional"] = null;
                    //if (ContactNosList != null && ContactNosList != "")
                    //{
                    //    int ContactNoId = Convert.ToInt32(ContactNosList);
                    //    var val = db.ContactNumbers.Where(e => e.Id == ContactNoId).SingleOrDefault();
                    //    LvReq.ContactNo = val;
                    //}

                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    //  var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                    Employee OEmployee = null;
                    int OEmployeeLvId = 0;



                    LvNewReq LvNewReq = new LvNewReq()
                    {

                        FromDate = LvReq.FromDate,
                        FromStat = LvReq.FromStat,
                        LeaveHead = LvReq.LeaveHead,
                        ReqDate = LvReq.ReqDate,
                        ResumeDate = LvReq.ResumeDate,
                        ToDate = LvReq.ToDate,
                        ToStat = LvReq.ToStat,
                        LeaveCalendar = LvReq.LeaveCalendar,
                        Narration = "Leave Requisition"
                    };
                    if (ids != 0)
                    {
                        //foreach (var i in ids)
                        //{
                        OEmployee = db.Employee.Where(r => r.Id == ids)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.GeoStruct.Location)
                                .Include(e => e.FuncStruct)
                                .Include(e => e.PayStruct)

                                .SingleOrDefault();

                        OEmployeeLvId
                        = db.EmployeeLeave.Where(e => e.Employee.Id == ids).SingleOrDefault().Id;

                        LvNewReq.GeoStruct = OEmployee.GeoStruct;
                        LvNewReq.PayStruct = OEmployee.PayStruct;
                        LvNewReq.FuncStruct = OEmployee.FuncStruct;
                        // Check leave head policy as calendar,joining,increment start
                        DateTime? Leaveyearfrom;
                        DateTime? LeaveyearTo;
                        LeaveHeadProcess.ReturnDatacalendarpara RetDataparam = new LeaveHeadProcess.ReturnDatacalendarpara();
                        RetDataparam = LeaveHeadProcess.LeaveCalendarpara(LvReq.LeaveHead.Id, OEmployeeLvId);
                        if (RetDataparam.ErrNo != 0)
                        {
                            return Json(new { RetDataparam.ErrNo });
                        }
                        else
                        {
                            Leaveyearfrom = RetDataparam.Leaveyearfrom;
                            LeaveyearTo = RetDataparam.LeaveyearTo;
                        }


                        // Check leave head policy as calendar,joining,increment End


                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                   new System.TimeSpan(0, 30, 0)))
                        {
                            try
                            {
                                LeaveHeadProcess.ReturnData_LeaveValidation retD = new LeaveHeadProcess.ReturnData_LeaveValidation();
                                //var ComCode = db.Company.Where(e => e.Id == Comp_Id).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault().Code;
                                //if (ComCode.ToUpper() == "MSCB")
                                //{
                                //    double pld=0;
                                //    retD = LeaveHeadProcess.MSCLvValidate(LvNewReq, Comp_Id, OEmployeeLvId, LvReq.LeaveCalendar, Leaveyearfrom, LeaveyearTo, pld);
                                //    if (retD.ErrNo != 0 )
                                //    {
                                //        return Json(new { retD.ErrNo });
                                //    }
                                //}
                                //  retD = LeaveHeadProcess.LeaveValidation(LvNewReq, OEmployeeLvId, Leaveyearfrom, LeaveyearTo);

                                // ====================================== Api call Start ==============================================

                                int errorno = 0;
                                double debdays = 0.0;
                                bool IsCertAppl = false;
                                bool IsCertOptional = false;
                                ReturnData_LeaveValidation returnDATA = new ReturnData_LeaveValidation();

                                var ShowMessageCode = "";
                                var ShowMessage = "";

                                ServiceResult<ReturnData_LeaveValidation> responseDeserializeData = new ServiceResult<ReturnData_LeaveValidation>();
                                string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                                using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                                {
                                    var response = p2BHttpClient.request("ELMS/getUserLvDebitDaysRequest",
                                        new ELMS_Lv_DebitDays()
                                        {
                                            Emp_Id = ids,
                                            Lv_Head_Id = LvReq.LeaveHead.Id,
                                            ReqDate = LvReq.ReqDate,
                                            FromDate = LvReq.FromDate,
                                            ToDate = LvReq.ToDate,
                                            FromStat_Id = Convert.ToInt32(FrmStat),
                                            ToStat_Id = Convert.ToInt32(Tostat)
                                        });

                                    var data = response.Content.ReadAsStringAsync().Result;
                                    //if (data!=null)
                                    //{
                                    //    var xx = 0; 
                                    //}
                                    // var result = data.;

                                    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<ReturnData_LeaveValidation>>(response.Content.ReadAsStringAsync().Result);


                                    //ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                                    //ShowMessage = responseDeserializeData.Message.ToString();


                                    //using (DataBaseContext db = new DataBaseContext())
                                    //{
                                    if (responseDeserializeData.Data != null)
                                    {

                                        LvReq.DebitDays = responseDeserializeData.Data.DebitDays;
                                        LvReq.PrefixCount = responseDeserializeData.Data.LvnewReqprefix;
                                        LvReq.SufixCount = responseDeserializeData.Data.LvnewReqSuffix;
                                        LvReq.PrefixSuffix = responseDeserializeData.Data.PrefixSufix;
                                        LvReq.LvCountPrefixSuffix = responseDeserializeData.Data.LvCountPrefixSuffix;
                                        LvReq.IsDebitSharing = responseDeserializeData.Data.IsLvDebitSharing;
                                       
                                        Session["LvReq"] = LvReq;

                                        errorno = responseDeserializeData.Data.ErrNo;
                                        debdays = responseDeserializeData.Data.DebitDays;
                                        IsCertAppl = responseDeserializeData.Data.IsCertAppl;
                                        IsCertOptional = responseDeserializeData.Data.IsCertOptional;
                                        Session["IsCertAppl"] = IsCertAppl;
                                        Session["IsCertOptional"] = IsCertOptional;

                                        int errno = responseDeserializeData.Data.ErrNo;

                                        var oErrorlookup = db.ErrorLookup.Where(e => e.Message_Code == errno).FirstOrDefault();
                                        ShowMessage = errno + ' ' + oErrorlookup.Message_Description.ToString();

                                    }

                                    else
                                    {
                                        errorno = 1;
                                        ShowMessage = responseDeserializeData.Message.ToString();
                                    }


                                    //}

                                }

                                // ====================================== Api call End ==============================================
                                //debdays = 1;
                                if (errorno > 0)
                                {
                                    return Json(new { errorno, ShowMessage, IsCertAppl, IsCertOptional });
                                }
                                else
                                {
                                    return Json(new { errorno, debdays, IsCertAppl, IsCertOptional });

                                }


                                //if (ComCode.ToUpper() == "MSCB" && retD.ErrNo == 0)
                                //{

                                //    if (LvNewReq.LeaveHead.LvCode == "PL") //IF CL Balance is zero then if  Employee Requested for PL , It exclude Holiday and Weekly off but it is allowed for only 5 working days
                                //    {
                                //        double pld = retD.DebitDays;
                                //        retD = LeaveHeadProcess.MSCLvValidate(LvNewReq, Comp_Id, OEmployeeLvId, LvReq.LeaveCalendar, Leaveyearfrom, LeaveyearTo, pld);
                                //        if (retD.ErrNo == 0)
                                //        {
                                //            return Json(new { retD.ErrNo, retD.DebitDays });
                                //        }
                                //    }
                                //}
                                //if (retD.ErrNo != 0)
                                //{
                                //    return Json(new { retD.ErrNo });
                                //}
                                //else
                                //{
                                //    retD.ErrNo = 0;
                                //    return Json(new { retD.ErrNo, retD.DebitDays });


                                //else
                                //{
                                //  //  retD.ErrNo = 0;
                                //    return Json(new { errorno, ShowMessage });
                                //}

                                //}

                            }
                            catch (DataException ex)
                            {
                                //LogFile Logfile = new LogFile();
                                //ErrorLog Err = new ErrorLog()
                                //{
                                //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                //    ExceptionMessage = ex.Message,
                                //    ExceptionStackTrace = ex.StackTrace,
                                //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                //    LogTime = DateTime.Now
                                //};
                                //Logfile.CreateLogFile(Err);
                                //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        //}
                    }
                    Msg.Add("  Date Has Been Process  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Date Has Been Process" }, JsonRequestBehavior.AllowGet);
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
                Msg.Add(ex.ToString());
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLVHEAD(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(data2);
                SelectList s = (SelectList)null;
                var selected = "";

                // Goa urban spl leave can not apply before 5 year from joining/confirmation date
                string requiredPathLoan = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPathLoan);
                string localPathLoan;
                if (!exists)
                {
                    localPathLoan = new Uri(requiredPathLoan).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathLoan);
                }
                string pathLoan = requiredPathLoan + @"\Leaveapplyafteryear" + ".ini";
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


                string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                bool existschk = System.IO.Directory.Exists(requiredPathchk);
                string localPathchk;
                if (!existschk)
                {
                    localPathchk = new Uri(requiredPathchk).LocalPath;
                    System.IO.Directory.CreateDirectory(localPathchk);
                }
                string pathchk = requiredPathchk + @"\Leaveapplyafteryear" + ".ini";
                localPathchk = new Uri(pathchk).LocalPath;
                List<int> oLvHeadid = new List<int>();

                using (var streamReader = new StreamReader(localPathchk))
                {
                    string line;

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var LVCODE = line.Split('_')[0];
                        var Joinconfdt = line.Split('_')[1];
                        var yearp = line.Split('_')[2];
                        int yearpara = Convert.ToInt32(yearp);
                        if (LVCODE != "")
                        {
                            double mAge = 0;
                            var employee = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.Id == empid).SingleOrDefault();
                            if (Joinconfdt.ToUpper() == "JOINING")
                            {

                                var mDateofBirth = employee.ServiceBookDates.JoiningDate;
                                DateTime start = mDateofBirth.Value;
                                DateTime end = DateTime.Now.Date;// DateTime.Now.Date;
                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                mAge = months / 12;


                            }
                            if (Joinconfdt.ToUpper() == "CONFIRMATION")
                            {
                                var mDateofBirth = employee.ServiceBookDates.ConfirmationDate;
                                DateTime start = mDateofBirth.Value;
                                DateTime end = DateTime.Now.Date;// DateTime.Now.Date;
                                int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                double months = compMonth + (start.Day - end.Day) / daysInEndMonth;
                                mAge = months / 12;
                            }

                            if (mAge <= yearpara)
                            {
                                var lvheadid = db.LvHead.Where(e => e.LvCode == LVCODE).Select(e => e.Id).ToList();
                                oLvHeadid.AddRange(lvheadid);
                            }

                        }
                    }
                }

                //if (data != "" && data != null)
                //{
                // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                //var qurey = db.LvHead.ToList();

                //before
                //var query1 = db.EmployeeLeave.Where(e => e.Employee.Id == empid)
                //    .Include(e => e.LvOpenBal)
                //    .Include(e => e.LvOpenBal.Select(r => r.LvHead))
                //    .Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                //    .Include(a => a.LvNewReq)
                //    .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                //    .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                //    .SingleOrDefault();
                //var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();
                //var aa = query1.LvNewReq.Where(e => e.LeaveCalendar.Id == lvcalendar.Id).Count();
                //List<LvHead> oLvHead = new List<LvHead>();
                //if (aa == 0)
                //{
                //    oLvHead = query1.LvOpenBal.Where(e => e.LvCalendar.Id == lvcalendar.Id).Select(e => e.LvHead).ToList();
                //}
                //else
                //{
                //    oLvHead = query1.LvNewReq.Where(e => e.LeaveCalendar.Id == lvcalendar.Id).Select(a => a.LeaveHead).Distinct().ToList();
                //}
                //if (oLvHead.Count() > 0)
                //{
                //    s = new SelectList(oLvHead, "Id", "FullDetails", selected);
                //}

                //new vr
                var query1 = db.EmployeeLeave.Where(e => e.Employee.Id == empid)
                   .Include(e => e.LvOpenBal)
                   .Include(e => e.LvOpenBal.Select(r => r.LvHead))
                   .Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                   .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                   .Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).SingleOrDefault();

                var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();

                //var CheckingLeavheadFromlvnereq = query1.LvNewReq.Where(e => e.LeaveCalendar.Id == lvcalendar.Id).Select(t => t.LeaveHead).Distinct().ToList();
                var CheckingLeavheadFromlvnereq = query1.LvNewReq.Where(e => !oLvHeadid.Contains(e.LeaveHead.Id)).Select(t => t.LeaveHead).Distinct().ToList();

                List<LvHead> oLvHead = new List<LvHead>();

                if (CheckingLeavheadFromlvnereq.Count() == 0)
                {

                    oLvHead = query1.LvOpenBal.Where(e => e.LvCalendar.Id == lvcalendar.Id && !oLvHeadid.Contains(e.LvHead.Id)).Select(e => e.LvHead).Distinct().ToList();
                    if (oLvHead.Count() > 0)
                    {
                        s = new SelectList(oLvHead, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    if (CheckingLeavheadFromlvnereq.Count() > 0)
                    {
                        s = new SelectList(CheckingLeavheadFromlvnereq, "Id", "FullDetails", selected);
                        return Json(s, JsonRequestBehavior.AllowGet);
                    }
                }
                return null;

            }
        }
        //CreateOLD ActionResult
        //public ActionResult CreateOld(LvNewReq LvReq, FormCollection form, String forwarddata, string DebitDays) //Create submit
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
        //        string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
        //        string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
        //        string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
        //        string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
        //        DebitDays = form["DebitDays"] == "0" ? "" : form["DebitDays"];
        //        string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];


        //        int EmpId = 0;
        //        if (Emp != null && Emp != "0" && Emp != "false")
        //        {
        //            EmpId = int.Parse(Emp);
        //        }
        //        else
        //        {
        //            Msg.Add("  Please Select Employee  ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
        //        }
        //        if (DebitDays == "0" || DebitDays == "")
        //        {
        //            Msg.Add("  Debit Days zero leave record can not save  ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //        }
        //        //var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
        //        //if (calendar != null && calendar != "")
        //        //{
        //        //    var value = db.Calendar.Find(int.Parse(calendar));
        //        //    LvReq.LeaveCalendar = value;
        //        //}
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            if (Incharge_DDL != null && Incharge_DDL != "-Select-")
        //            {
        //                var value = db.Employee.Find(int.Parse(Incharge_DDL));
        //                LvReq.Incharge = value;

        //            }
        //            if (FrmStat != null && FrmStat != "")
        //            {
        //                var value = db.LookupValue.Find(int.Parse(FrmStat));
        //                LvReq.FromStat = value;
        //            }
        //            else
        //            {
        //                Msg.Add("  Please Select Leave FromState  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "", "Please Select Leave FromState " }, JsonRequestBehavior.AllowGet);
        //            }
        //            if (Tostat != null && Tostat != "")
        //            {
        //                var value = db.LookupValue.Find(int.Parse(Tostat));
        //                LvReq.ToStat = value;
        //            }
        //            else
        //            {
        //                Msg.Add("  Please Select Leave FromState  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "", "Please Select Leave Tostate " }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (LvHeadList != null && LvHeadList != "")
        //            {
        //                var val = db.LvHead.Find(int.Parse(LvHeadList));
        //                LvReq.LeaveHead = val;
        //            }
        //            else
        //            {
        //                Msg.Add("  Please Select Leave Head  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return Json(new Object[] { "", "", "Please Select Leave Head" }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (ContactNosList != null && ContactNosList != "")
        //            {
        //                int ContactNoId = Convert.ToInt32(ContactNosList);
        //                var val = db.ContactNumbers.Where(e => e.Id == ContactNoId).SingleOrDefault();
        //                LvReq.ContactNo = val;
        //            }





        //            var Comp_Id = 0;
        //            Comp_Id = Convert.ToInt32(Session["CompId"]);
        //            var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).FirstOrDefault();
        //            Employee OEmployee = null;
        //            EmployeeLeave OEmployeeLeave = null;
        //            Company OCompany = null;
        //            LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //            /* Added By Rekha 04-03-2017*/
        //            LvReq.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
        //            int Empid = int.Parse(Emp);
        //            //var PrevReq = db.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id).OrderByDescending(e => e.Id).FirstOrDefault();

        //            OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal)
        //            .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == Empid).SingleOrDefault();

        //            OCompany = db.Company.Find(Comp_Id);

        //           // var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
        //                 var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id )
        //                .OrderByDescending(e => e.Id).FirstOrDefault();
        //            if (PrevReq != null)
        //            {
        //                LvReq.OpenBal = PrevReq.CloseBal;
        //                LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
        //                LvReq.LvOccurances = PrevReq.LvOccurances + 1;

        //            }
        //            else
        //            {
        //                var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvReq.LeaveHead.Id && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
        //                LvReq.OpenBal = PrevOpenBal.LvOpening;
        //                LvReq.LVCount = LvReq.DebitDays;
        //                LvReq.LvOccurances = 1;


        //            }


        //            OEmployee = db.Employee
        //                    .Include(e => e.GeoStruct)
        //                    .Include(e => e.GeoStruct.Location)
        //                    .Include(e => e.FuncStruct)
        //                    .Include(e => e.PayStruct)
        //                    .Where(r => r.Id == Empid)
        //                    .SingleOrDefault();

        //            var OEmployeePayroll
        //            = db.EmployeeLeave
        //          .Where(e => e.Employee.Id == Empid).SingleOrDefault();

        //            LvReq.GeoStruct = OEmployee.GeoStruct;
        //            LvReq.PayStruct = OEmployee.PayStruct;
        //            LvReq.FuncStruct = OEmployee.FuncStruct;
        //            // Check leave head policy as calendar,joining,increment start
        //            DateTime? Leaveyearfrom;
        //            DateTime? LeaveyearTo;
        //            LeaveHeadProcess.ReturnDatacalendarpara RetDataparam = new LeaveHeadProcess.ReturnDatacalendarpara();
        //            RetDataparam = LeaveHeadProcess.LeaveCalendarpara(LvReq.LeaveHead.Id, OEmployeeLeave.Id);
        //            if (RetDataparam.ErrNo != 0)
        //            {
        //                return Json(new { RetDataparam.ErrNo });
        //            }
        //            else
        //            {
        //                Leaveyearfrom = RetDataparam.Leaveyearfrom;
        //                LeaveyearTo = RetDataparam.LeaveyearTo;
        //            }


        //            // Check leave head policy as calendar,joining,increment End


        //            //var retD = LeaveHeadProcess.LeaveValidation(LvReq, OEmployeeLeave.Id, Leaveyearfrom, LeaveyearTo);


        //            //LvReq.DebitDays = retD.DebitDays;
        //            //LvReq.PrefixCount = retD.LvnewReqprefix;
        //            //LvReq.SufixCount = retD.LvnewReqSuffix;
        //            //LvReq.PrefixSuffix = retD.PrefixSufix;
        //            LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
        //            LvReq.InputMethod = 0;//apply through main source
        //            /**/
        //            DateTime? OldToDate = LvReq.ToDate;
        //            double OldDebDays = LvReq.DebitDays;
        //            //if (OCompany.Code.ToString() == "KDCC" && LvReq.LeaveHead.LvCode.ToUpper() == "SL")
        //            //{
        //            //    //LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
        //            //    if (LvReq.CloseBal <= 0)
        //            //    {
        //            //        LvReq.CloseBal = 0;
        //            //        LvReq.DebitDays = LvReq.OpenBal;
        //            //        if (PrevReq != null)
        //            //        {
        //            //            LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
        //            //        }
        //            //        else
        //            //        {
        //            //            LvReq.LVCount = LvReq.DebitDays;

        //            //        }
        //            //        LvReq.ToDate = LvReq.FromDate.Value.AddDays(LvReq.DebitDays - 1);
        //            //    }
        //            //}

        //            LvNewReq LvNewReq = new LvNewReq()
        //            {
        //                ContactNo = LvReq.ContactNo,
        //                DebitDays = LvReq.DebitDays,
        //                FromDate = LvReq.FromDate,
        //                FromStat = LvReq.FromStat,
        //                LeaveHead = LvReq.LeaveHead,
        //                Reason = LvReq.Reason,
        //                ReqDate = LvReq.ReqDate,
        //                ResumeDate = LvReq.ToDate.Value.AddDays(1),
        //                ToDate = LvReq.ToDate,
        //                ToStat = LvReq.ToStat,
        //                LeaveCalendar = LvReq.LeaveCalendar,
        //                DBTrack = LvReq.DBTrack,
        //                CloseBal = LvReq.CloseBal,
        //                OpenBal = LvReq.OpenBal,
        //                LVCount = LvReq.LVCount,
        //                LvOccurances = LvReq.LvOccurances,
        //                PrefixCount = LvReq.PrefixCount,
        //                SufixCount = LvReq.SufixCount,
        //                Incharge = LvReq.Incharge,
        //                TrClosed = true,
        //                Narration = "Leave Requisition",
        //                WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
        //                PrefixSuffix = LvReq.PrefixSuffix
        //            };
        //            List<LvNewReq> OFAT = new List<LvNewReq>();
        //            if (ModelState.IsValid)
        //            {

        //                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
        //                            .Where(r => r.Id == EmpId).SingleOrDefault();
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                           new System.TimeSpan(0, 30, 0)))
        //                {
        //                    try
        //                    {
        //                        LvNewReq.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
        //                        LvNewReq.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
        //                        LvNewReq.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
        //                        db.LvNewReq.Add(LvNewReq);

        //                        if (Z.Company.Code.ToString() == "KDCC" && LvReq.LeaveHead.LvCode.ToUpper() == "SL")
        //                        {
        //                            if (LvReq.CloseBal == 0 && OldDebDays > LvReq.DebitDays)
        //                            {
        //                                OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal)
        //                               .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
        //                               .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
        //                               .Where(e => e.Employee.Id == Empid).SingleOrDefault();

        //                                PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.LvCode == "PL" && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
        //                                    .OrderByDescending(e => e.Id).FirstOrDefault();

        //                                LvNewReq LvPLReq = new LvNewReq();

        //                                LvPLReq.DebitDays = OldDebDays - LvReq.DebitDays;

        //                                if (PrevReq != null)
        //                                {
        //                                    LvPLReq.OpenBal = PrevReq.CloseBal;
        //                                    LvPLReq.LVCount = PrevReq.LVCount + LvPLReq.DebitDays;
        //                                    LvPLReq.LvOccurances = PrevReq.LvOccurances;
        //                                    LvPLReq.LeaveHead = PrevReq.LeaveHead;
        //                                }
        //                                else
        //                                {
        //                                    var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.LvCode.ToUpper() == "PL" && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
        //                                    LvPLReq.OpenBal = PrevOpenBal.LvOpening;
        //                                    LvPLReq.LVCount = LvPLReq.DebitDays;
        //                                    LvPLReq.LvOccurances = 1;
        //                                    LvPLReq.LeaveHead = PrevOpenBal.LvHead;
        //                                }


        //                                LvPLReq.CloseBal = LvPLReq.OpenBal - LvPLReq.DebitDays;
        //                                LvPLReq.GeoStruct = OEmployee.GeoStruct;
        //                                LvPLReq.PayStruct = OEmployee.PayStruct;
        //                                LvPLReq.FuncStruct = OEmployee.FuncStruct;

        //                                if (LvPLReq.CloseBal < 0 || LvPLReq.OpenBal < LvPLReq.DebitDays)
        //                                {
        //                                    Msg.Add("  SL combined with PL Can't be utilised as your PL balcance is low.  ");
        //                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                                }


        //                                LvNewReq LvNewReqNew = new LvNewReq()
        //                                {
        //                                    ContactNo = LvReq.ContactNo,
        //                                    DebitDays = LvPLReq.DebitDays,
        //                                    FromDate = LvReq.ToDate.Value.AddDays(1),
        //                                    FromStat = LvReq.FromStat,
        //                                    LeaveHead = LvPLReq.LeaveHead,
        //                                    Reason = "On leave finished. Leave Occurances not changed.",
        //                                    ReqDate = LvReq.ReqDate,
        //                                    ResumeDate = OldToDate.Value.AddDays(1),
        //                                    ToDate = OldToDate,
        //                                    ToStat = LvReq.ToStat,
        //                                    LeaveCalendar = LvReq.LeaveCalendar,
        //                                    DBTrack = LvReq.DBTrack,
        //                                    CloseBal = LvPLReq.CloseBal,
        //                                    OpenBal = LvPLReq.OpenBal,
        //                                    LVCount = LvPLReq.LVCount,
        //                                    LvOccurances = LvPLReq.LvOccurances,
        //                                    PrefixCount = LvReq.PrefixCount,
        //                                    SufixCount = LvReq.SufixCount,
        //                                    Incharge = LvReq.Incharge,
        //                                    TrClosed = true,
        //                                    Narration = "Leave Requisition",
        //                                    WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
        //                                    PrefixSuffix = LvPLReq.PrefixSuffix,
        //                                    GeoStruct = LvReq.GeoStruct,
        //                                    PayStruct = LvReq.PayStruct,
        //                                    FuncStruct = LvReq.FuncStruct
        //                                };
        //                                OFAT.Add(LvNewReqNew);
        //                            }


        //                        }




        //                        OFAT.Add(LvNewReq);
        //                        db.SaveChanges();

        //                        if (OEmployeeLeave == null)
        //                        {
        //                            EmployeeLeave OTEP = new EmployeeLeave()
        //                            {
        //                                Employee = db.Employee.Find(OEmployee.Id),
        //                                LvNewReq = OFAT,
        //                                DBTrack = LvReq.DBTrack

        //                            };


        //                            db.EmployeeLeave.Add(OTEP);
        //                            db.SaveChanges();
        //                        }
        //                        else
        //                        {
        //                            var aa = db.EmployeeLeave.Find(OEmployeeLeave.Id);
        //                            OFAT.AddRange(aa.LvNewReq);
        //                            aa.LvNewReq = OFAT;
        //                            //OEmployeePayroll.DBTrack = dbt;
        //                            db.EmployeeLeave.Attach(aa);
        //                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                            db.SaveChanges();
        //                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

        //                        }


        //                        ts.Complete();
        //                        // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
        //                        Msg.Add("  Data Saved successfully  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    catch (DataException ex)
        //                    {
        //                        LogFile Logfile = new LogFile();
        //                        ErrorLog Err = new ErrorLog()
        //                        {
        //                            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                            ExceptionMessage = ex.Message,
        //                            ExceptionStackTrace = ex.StackTrace,
        //                            LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                            LogTime = DateTime.Now
        //                        };
        //                        Logfile.CreateLogFile(Err);
        //                        return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
        //                    }


        //                }
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder("");
        //                foreach (ModelState modelState in ModelState.Values)
        //                {
        //                    foreach (ModelError error in modelState.Errors)
        //                    {
        //                        sb.Append(error.ErrorMessage);
        //                        sb.Append("." + "\n");
        //                    }
        //                }
        //                var errorMsg = sb.ToString();
        //                Msg.Add(errorMsg);
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //return this.Json(new { msg = errorMsg });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public class ELMS_Lv_NewRequest
        {
            public int Emp_Id { get; set; }

            public string Emp_Code { get; set; }

            public int? Lv_Head_Id { get; set; }

            public string Lv_Head { get; set; }

            public int? LeaveCalendar_Id { get; set; }

            public int? InputMethod { get; set; }

            public DateTime? ReqDate { get; set; }

            public DateTime? FromDate { get; set; }

            public DateTime? ToDate { get; set; }

            public int? FromStat_Id { get; set; }

            public int? ToStat_Id { get; set; }

            public string FromStat { get; set; }

            public string ToStat { get; set; }

            public double DebitDays { get; set; }

            public double? CreditDays { get; set; }

            public string Reason { get; set; }

            public int? ContactNo_Id { get; set; }

            public string MobileNo { get; set; }

            public string LandlineNo { get; set; }

            public int? Incharge_Id { get; set; }

            public string Incharge_Code { get; set; }

            public bool IsDebitSharing { get; set; }

            public double PrefixCount { get; set; }

            public double SufixCount { get; set; }

            public bool PrefixSuffix { get; set; }

            public double LvCountPrefixSuffix { get; set; }

            public string Path { get; set; }

            public string User_Code { get; set; }

            public int LvWFStatus { get; set; }

            public string Comments { get; set; }
        }

        public ActionResult Create(LvNewReq LvReq, FormCollection form, String forwarddata, string DebitDays) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
                string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
                DebitDays = form["DebitDays"] == "0" ? "" : form["DebitDays"];
                // string Incharge_DDL = form["Incharge_DDL"] == "" ? null : form["Incharge_DDL"];
                string Incharge_DDL = form["InchargeList"] == "" ? null : form["InchargeList"];

                int EmpId = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    EmpId = int.Parse(Emp);
                }
                else
                {
                    Msg.Add("  Please Select Employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                }
                if (DebitDays == "0" || DebitDays == "")
                {
                    Msg.Add("  Debit Days zero leave record can not save  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                //var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                //if (calendar != null && calendar != "")
                //{
                //    var value = db.Calendar.Find(int.Parse(calendar));
                //    LvReq.LeaveCalendar = value;
                //}
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (Incharge_DDL != null && Incharge_DDL != "-Select-")
                    {
                        var value = db.Employee.Find(int.Parse(Incharge_DDL));
                        LvReq.Incharge = value;

                    }
                    if (FrmStat != null && FrmStat != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(FrmStat));
                        LvReq.FromStat = value;
                    }
                    else
                    {
                        Msg.Add("  Please Select Leave FromStat..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Please Select Leave FromState " }, JsonRequestBehavior.AllowGet);
                    }
                    if (Tostat != null && Tostat != "")
                    {
                        var value = db.LookupValue.Find(int.Parse(Tostat));
                        LvReq.ToStat = value;
                    }
                    else
                    {
                        Msg.Add("  Please Select Leave ToStat..  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Please Select Leave Tostate " }, JsonRequestBehavior.AllowGet);
                    }

                    if (LvHeadList != null && LvHeadList != "")
                    {
                        var val = db.LvHead.Find(int.Parse(LvHeadList));
                        LvReq.LeaveHead = val;
                    }
                    else
                    {
                        Msg.Add("  Please Select Leave Head  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Please Select Leave Head" }, JsonRequestBehavior.AllowGet);
                    }

                    if (ContactNosList != null && ContactNosList != "")
                    {
                        int ContactNoId = Convert.ToInt32(ContactNosList);
                        var val = db.ContactNumbers.Where(e => e.Id == ContactNoId).SingleOrDefault();
                        LvReq.ContactNo = val;
                    }





                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).FirstOrDefault();
                    Employee OEmployee = null;
                    EmployeeLeave OEmployeeLeave = null;
                    Company OCompany = null;
                    LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    /* Added By Rekha 04-03-2017*/
                    LvReq.LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    int Empid = int.Parse(Emp);
                    //var PrevReq = db.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id).OrderByDescending(e => e.Id).FirstOrDefault();

                    OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal)
                    .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar)).Where(e => e.Employee.Id == Empid).SingleOrDefault();

                    OCompany = db.Company.Find(Comp_Id);

                    // // var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
                    // var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id)
                    //.OrderByDescending(e => e.Id).FirstOrDefault();
                    // if (PrevReq != null)
                    // {
                    //     LvReq.OpenBal = PrevReq.CloseBal;
                    //     LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
                    //     LvReq.LvOccurances = PrevReq.LvOccurances + 1;

                    // }
                    // else
                    // {
                    //     var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvReq.LeaveHead.Id && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
                    //     LvReq.OpenBal = PrevOpenBal.LvOpening;
                    //     LvReq.LVCount = LvReq.DebitDays;
                    //     LvReq.LvOccurances = 1;


                    // }


                    OEmployee = db.Employee
                            .Include(e => e.GeoStruct)
                            .Include(e => e.GeoStruct.Location)
                            .Include(e => e.FuncStruct)
                            .Include(e => e.PayStruct)
                            .Where(r => r.Id == Empid)
                            .SingleOrDefault();

                    var OEmployeePayroll
                    = db.EmployeeLeave
                  .Where(e => e.Employee.Id == Empid).SingleOrDefault();

                    LvReq.GeoStruct = OEmployee.GeoStruct;
                    LvReq.PayStruct = OEmployee.PayStruct;
                    LvReq.FuncStruct = OEmployee.FuncStruct;
                    // Check leave head policy as calendar,joining,increment start
                    //DateTime? Leaveyearfrom;
                    //DateTime? LeaveyearTo;
                    //LeaveHeadProcess.ReturnDatacalendarpara RetDataparam = new LeaveHeadProcess.ReturnDatacalendarpara();
                    //RetDataparam = LeaveHeadProcess.LeaveCalendarpara(LvReq.LeaveHead.Id, OEmployeeLeave.Id);
                    //if (RetDataparam.ErrNo != 0)
                    //{
                    //    return Json(new { RetDataparam.ErrNo });
                    //}
                    //else
                    //{
                    //    Leaveyearfrom = RetDataparam.Leaveyearfrom;
                    //    LeaveyearTo = RetDataparam.LeaveyearTo;
                    //}


                    // Check leave head policy as calendar,joining,increment End


                    //var retD = LeaveHeadProcess.LeaveValidation(LvReq, OEmployeeLeave.Id, Leaveyearfrom, LeaveyearTo);


                    //LvReq.DebitDays = retD.DebitDays;
                    //LvReq.PrefixCount = retD.LvnewReqprefix;
                    //LvReq.SufixCount = retD.LvnewReqSuffix;
                    //LvReq.PrefixSuffix = retD.PrefixSufix;
                    //LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
                    //LvReq.InputMethod = 0;//apply through main source
                    /**/
                    //DateTime? OldToDate = LvReq.ToDate;
                    //double OldDebDays = LvReq.DebitDays;


                    //LvNewReq LvNewReq = new LvNewReq()
                    //{
                    //ContactNo = LvReq.ContactNo,
                    //DebitDays = LvReq.DebitDays,
                    //FromDate = LvReq.FromDate,
                    //FromStat = LvReq.FromStat,
                    //LeaveHead = LvReq.LeaveHead,
                    //Reason = LvReq.Reason,
                    //ReqDate = LvReq.ReqDate,
                    //ResumeDate = LvReq.ToDate.Value.AddDays(1),
                    //ToDate = LvReq.ToDate,
                    //ToStat = LvReq.ToStat,
                    //LeaveCalendar = LvReq.LeaveCalendar,
                    //DBTrack = LvReq.DBTrack,
                    //CloseBal = LvReq.CloseBal,
                    //OpenBal = LvReq.OpenBal,
                    //LVCount = LvReq.LVCount,
                    //LvOccurances = LvReq.LvOccurances,
                    //PrefixCount = LvReq.PrefixCount,
                    //SufixCount = LvReq.SufixCount,
                    //Incharge = LvReq.Incharge,
                    //TrClosed = true,
                    //Narration = "Leave Requisition",
                    //WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                    //PrefixSuffix = LvReq.PrefixSuffix

                    //};
                    //List<LvNewReq> OFAT = new List<LvNewReq>();
                    if (Session["FilePath"] != null)
                    {
                        LvReq.Path = Session["FilePath"].ToString();
                    }

                    // upload certificate if leave taken after no of times ex. if 3 time take SL then not require after that require certificate start
                    var leavecalendarid = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    EmployeeLeave OEmployeeLeavechk = null;
                    OEmployeeLeavechk = db.EmployeeLeave.Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(y => y.LeaveCalendar))
                   .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
               .Where(e => e.Employee.Id == Empid).SingleOrDefault();
                    var LvOrignal_idchk = OEmployeeLeavechk.LvNewReq.Where(e => e.LvOrignal != null && e.LeaveHead_Id == LvReq.LeaveHead.Id).Select(e => e.LvOrignal.Id).ToList();
                    var AntCancelchk = OEmployeeLeavechk.LvNewReq.Where(e => e.IsCancel == false && e.TrReject == false && e.LeaveHead_Id == LvReq.LeaveHead.Id && e.LeaveCalendar_Id == leavecalendarid.Id).OrderBy(e => e.Id).ToList();
                    var listLvschk = AntCancelchk.Where(e => !LvOrignal_idchk.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.Path=="").OrderBy(e => e.Id).ToList();


                    //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    //bool exists = System.IO.Directory.Exists(requiredPath);
                    //string localPath;
                    //if (!exists)
                    //{
                    //    localPath = new Uri(requiredPath).LocalPath;
                    //    System.IO.Directory.CreateDirectory(localPath);
                    //}
                    //string path = requiredPath + @"\Leavecertificateonreqcount" + ".ini";
                    //localPath = new Uri(path).LocalPath;
                    //if (!System.IO.File.Exists(localPath))
                    //{

                    //    using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                    //    {
                    //        StreamWriter str = new StreamWriter(fs);
                    //        str.BaseStream.Seek(0, SeekOrigin.Begin);

                    //        str.Flush();
                    //        str.Close();
                    //        fs.Close();
                    //    }


                    //}

                    //string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    //bool existschk = System.IO.Directory.Exists(requiredPathchk);
                    //string localPathchk;
                    //if (!existschk)
                    //{
                    //    localPath = new Uri(requiredPathchk).LocalPath;
                    //    System.IO.Directory.CreateDirectory(localPath);
                    //}
                    //string pathchk = requiredPathchk + @"\Leavecertificateonreqcount" + ".ini";
                    //localPathchk = new Uri(pathchk).LocalPath;
                    //string Leave_code = "";
                    //int Lvreqcnt = 0;
                    //int paramcnt = 0;
                    //using (var streamReader = new StreamReader(localPathchk))
                    //{
                    //    string line;

                    //    while ((line = streamReader.ReadLine()) != null)
                    //    {
                    //        var LVCode = line.Split('_')[0];
                    //        var Reqparamcnt = line.Split('_')[1];
                    //        if (LVCode != "")
                    //        {
                    //            var val = db.LvHead.Find(int.Parse(LvHeadList));
                    //            if (val.LvCode == LVCode)
                    //            {
                    //                Leave_code = val.LvCode;
                    //                if (listLvschk != null)
                    //                {
                    //                    Lvreqcnt = listLvschk.Count();
                    //                }

                    //                paramcnt = Convert.ToInt32(Reqparamcnt);
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                    //if (Leave_code != "")
                    //{
                    //    if (LvReq.LeaveHead.Id != 0)
                    //    {
                    //        LvDebitPolicy ODebitPolicy = db.LvDebitPolicy.Where(e => e.LvHead_Id == LvReq.LeaveHead.Id).FirstOrDefault();

                    //        if (ODebitPolicy != null)
                    //        {
                    //            if (ODebitPolicy.IsCertificateAppl == true)
                    //            {
                    //                if (Lvreqcnt + 1 > paramcnt)
                    //                {

                    //                    if ((Lvreqcnt + 1 > paramcnt) && LvReq.Path == null)//Lvreqcnt =already avail leave req count and 1=new leave req
                    //                    {
                    //                        Msg.Add("Kindly upload certificate for this leave..!");
                    //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                    }
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (LvReq.Path != null)
                    //                {
                    //                    Msg.Add("You can't upload certificate for this leave..!");
                    //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //                }
                    //            }

                    //        }
                    //    }

                    //}
                    // upload certificate if leave taken after no of times ex. if 3 time take SL then not require after that require certificate end


                    //if (LvReq.LeaveHead.Id != 0)
                    //{
                    //    LvDebitPolicy ODebitPolicy = db.LvDebitPolicy.Where(e => e.LvHead_Id == LvReq.LeaveHead.Id).FirstOrDefault();

                    //    if (ODebitPolicy != null && Leave_code == "")
                    //    {
                    //        if (ODebitPolicy.IsCertificateAppl == true)
                    //        {
                    //            //if ((ODebitPolicy.MinLvDays >= LvReq.DebitDays) && LvReq.Path != null)
                    //            //{
                    //            //    Msg.Add("You can't upload cerificate for this leave..!");
                    //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //            //}
                    //            if ((ODebitPolicy.MinLvDays <= LvReq.DebitDays) && LvReq.Path == null)
                    //            {
                    //                Msg.Add("Kindly upload certificate for this leave..!");
                    //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (LvReq.Path != null)
                    //            {
                    //                Msg.Add("You can't upload certificate for this leave..!");
                    //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //            }
                    //        }

                    //    }
                    //}


                    if (Convert.ToBoolean(Session["IsCertAppl"]) == true && Convert.ToBoolean(Session["IsCertOptional"])==false && Session["FilePath"] == null)
                    {
                          Msg.Add("Kindly upload certificate for this leave..!");
                          return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ModelState.IsValid)
                    {

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == EmpId).SingleOrDefault();
                        //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                        //           new System.TimeSpan(0, 30, 0)))
                        //{
                        try
                        {
                            // ====================================== Api call Start ==============================================

                            if (Session["LvReq"] != null && Session["LvReq"] != "")
                            {
                                LvNewReq Onew = (LvNewReq)Session["LvReq"];
                                LvReq.DebitDays = Onew.DebitDays;
                                LvReq.IsDebitSharing = Onew.IsDebitSharing;
                                LvReq.PrefixCount = Onew.PrefixCount;
                                LvReq.SufixCount = Onew.SufixCount;
                                LvReq.PrefixSuffix = Onew.PrefixSuffix;
                                LvReq.LvCountPrefixSuffix = Onew.LvCountPrefixSuffix;
                                LvReq.IsDebitSharing = Onew.IsDebitSharing;

                            }
                            
                            int errorno = 0;
                            //double debdays = 0.0;
                            int bossid = Convert.ToInt32(SessionManager.EmpId);
                            string bossempcode = "";
                            var bosscode = db.Employee.Where(e => e.Id == bossid).SingleOrDefault();
                            if (bosscode!=null)
                            {
                               bossempcode= bosscode.EmpCode;
                            }
                            ReturnData_LeaveValidation returnDATA = new ReturnData_LeaveValidation();

                            var ShowMessageCode = "";
                            var ShowMessage = "";
                            int CalenderID = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).Select(e => e.Id).FirstOrDefault();

                            ServiceResult<ReturnData_LeaveValidation> responseDeserializeData = new ServiceResult<ReturnData_LeaveValidation>();
                            string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                            using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                            {
                                var response = p2BHttpClient.request("ELMS/getUserLvRequest",
                                    new ELMS_Lv_NewRequest()
                                    {   Emp_Code=bossempcode,
                                        Emp_Id = EmpId,
                                        Lv_Head_Id = LvReq.LeaveHead.Id,
                                        ReqDate = LvReq.ReqDate,
                                        FromDate = LvReq.FromDate,
                                        ToDate = LvReq.ToDate,
                                        FromStat_Id = Convert.ToInt32(FrmStat),
                                        ToStat_Id = Convert.ToInt32(Tostat),
                                        Reason = LvReq.Reason,
                                        Path = LvReq.Path,
                                        ContactNo_Id = Convert.ToInt32(ContactNosList),
                                        Incharge_Id = Convert.ToInt32(Incharge_DDL),
                                        LeaveCalendar_Id = CalenderID,
                                        InputMethod = 0,
                                        DebitDays = LvReq.DebitDays,
                                        IsDebitSharing = LvReq.IsDebitSharing,
                                        PrefixCount = LvReq.PrefixCount,
                                        SufixCount = LvReq.SufixCount,
                                        PrefixSuffix = LvReq.PrefixSuffix,
                                        LvCountPrefixSuffix = LvReq.LvCountPrefixSuffix,
                                        LvWFStatus = 5,
                                        Comments = "Approved By HR"

                                    });

                                var data = response.Content.ReadAsStringAsync().Result;

                                responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<ReturnData_LeaveValidation>>(response.Content.ReadAsStringAsync().Result);

                                //ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                                //ShowMessage = responseDeserializeData.Message.ToString();

                                if (responseDeserializeData.Data != null)
                                {

                                    //LvReq.DebitDays = responseDeserializeData.Data.DebitDays;
                                    //LvReq.PrefixCount = responseDeserializeData.Data.LvnewReqprefix;
                                    //LvReq.SufixCount = responseDeserializeData.Data.LvnewReqSuffix;
                                    //LvReq.PrefixSuffix = responseDeserializeData.Data.PrefixSufix;

                                    errorno = responseDeserializeData.Data.ErrNo;
                                    //debdays = responseDeserializeData.Data.DebitDays;

                                    int errno = responseDeserializeData.Data.ErrNo;

                                    var oErrorlookup = db.ErrorLookup.Where(e => e.Message_Code == errno).FirstOrDefault();
                                    ShowMessage = errno + ' ' + oErrorlookup.Message_Description.ToString();

                                }
                                else
                                {
                                    errorno = 1;
                                    ShowMessage = responseDeserializeData.Message.ToString();
                                }


                                //}

                            }
                            // ====================================== Api call End ==============================================



                            //if (OEmployeeLeave == null)
                            //{
                            //    EmployeeLeave OTEP = new EmployeeLeave()
                            //    {
                            //        Employee = db.Employee.Find(OEmployee.Id),
                            //        LvNewReq = OFAT,
                            //        DBTrack = LvReq.DBTrack

                            //    };


                            //    db.EmployeeLeave.Add(OTEP);
                            //    db.SaveChanges();
                            //}
                            //else
                            //{
                            //    var aa = db.EmployeeLeave.Find(OEmployeeLeave.Id);
                            //    OFAT.AddRange(aa.LvNewReq);
                            //    aa.LvNewReq = OFAT;
                            //    //OEmployeePayroll.DBTrack = dbt;
                            //    db.EmployeeLeave.Attach(aa);
                            //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            //}


                            // ts.Complete();
                            // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                            if (responseDeserializeData == null && ShowMessageCode == "OK")
                            {
                                Msg.Add(ShowMessage);
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                Msg.Add(ShowMessage);
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            //Msg.Add("  Data Saved successfully  ");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                            return Json(new { success = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                        // Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new { msg = errorMsg });
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

        public static double DayDateIsWeaklyOff(DateTime Odate, int EmployeeId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                var OEmp = db.Employee.Where(e => e.Id == EmployeeId)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.GeoStruct.Location)
                                .Include(e => e.GeoStruct.Location.WeeklyOffCalendar)
                               .Include(e => e.GeoStruct.Location.WeeklyOffCalendar.Select(t => t.WOCalendar))
                               .Include(e => e.GeoStruct.Location.WeeklyOffCalendar.Select(t => t.WeeklyOffList))
                               .Include(e => e.GeoStruct.Location.WeeklyOffCalendar.Select(t => t.WeeklyOffList.Select(y => y.WeekDays))).AsNoTracking().OrderBy(e => e.Id)
                               .FirstOrDefault();
                List<WeeklyOffCalendar> OWeklyOffChk = OEmp.GeoStruct.Location.WeeklyOffCalendar.OrderBy(e => e.Id).ToList();
                if (OWeklyOffChk.Count() == 0)
                {
                    return 100;//holiday calendar not defined : throw alert message
                }
                List<WeeklyOffList> OnWeaklyOff = OEmp.GeoStruct.Location.WeeklyOffCalendar.SelectMany(r => r.WeeklyOffList.Where(t => t.WeekDays.LookupVal.ToString() == Odate.Date.DayOfWeek.ToString())).OrderBy(e => e.Id).ToList();
                // .Where(e => e.WeeklyOffList.Any(r => r.WeekDays.LookupVal.ToString() == Odate.Date.DayOfWeek.ToString())).OrderBy(e => e.Id).ToList();
                if (OnWeaklyOff.Count > 0)
                {
                    //error
                    return 0;

                }
                else
                {
                    //success
                    return 1;
                }

            }
        }
        public class GetResumeDate //childgrid
        {

            public string Resumedate { get; set; }
            public int samedatehalfdayappl { get; set; }
            public Boolean halfdayappl { get; set; }

        }
        [HttpPost]
        public ActionResult GetSuffixDate(FormCollection form, int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Frmdate = form["FromDate"] == "0" ? "" : form["FromDate"];
                    string Todate = form["ToDate"] == "0" ? "" : form["ToDate"];
                    DateTime pfromdate = Convert.ToDateTime(Frmdate.ToString());
                    DateTime pTodate = Convert.ToDateTime(Todate.ToString());

                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    int Empid = int.Parse(Emp);
                    var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                    // Halfday code
                    Boolean halfday = false;
                    string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];

                    var val = db.LvHead.Find(int.Parse(LvHeadList));

                    int LvHead = val.Id;
                    int EmployeeLvStructids = db.EmployeeLeave.Where(e => e.Employee_Id == Empid).FirstOrDefault().Id;
                    var OLvSalStruct = db.EmployeeLvStruct
                                            .Include(e => e.EmployeeLvStructDetails)
                                            .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                            .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                            .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvDebitPolicy))
                                            .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                                .Where(e => e.EndDate == null && e.EmployeeLeave_Id == EmployeeLvStructids).SingleOrDefault();//.ToList();
                    var lvheadhalfdayapp = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == LvHead && e.LvHeadFormula != null && e.LvHeadFormula.LvDebitPolicy != null).Select(r => r.LvHeadFormula.LvDebitPolicy).FirstOrDefault();
                    if (lvheadhalfdayapp != null)
                    {

                        if (lvheadhalfdayapp.HalfDayAppl == true)
                        {
                            halfday = true;
                        }
                        else
                        {
                            halfday = false;
                        }
                    }
                    // Halfday code

                    // var value = db.Calendar.Find(int.Parse(calendar));
                    double mSuffix = 0;

                    Employee OEmployee = null;
                    OEmployee = db.Employee
                       .Include(e => e.GeoStruct)
                       .Include(e => e.GeoStruct.Location)
                       .Include(e => e.FuncStruct)
                       .Include(e => e.PayStruct)
                       .Where(r => r.Id == Empid)
                       .SingleOrDefault();

                    var OCompany = db.Location.Where(e => e.Id == OEmployee.GeoStruct.Location.Id)
                        .Include(e => e.HolidayCalendar)
                        .Include(e => e.HolidayCalendar.Select(r => r.HoliCalendar))
                        .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday)))
                        .Include(e => e.HolidayCalendar.Select(r => r.HolidayList.Select(t => t.Holiday.HolidayType)))
                        .Include(e => e.WeeklyOffCalendar)
                        .Include(e => e.WeeklyOffCalendar.Select(r => r.WOCalendar))
                        .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList))
                        .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeekDays)))
                        .Include(e => e.WeeklyOffCalendar.Select(r => r.WeeklyOffList.Select(t => t.WeeklyOffStatus)))
                        .AsNoTracking().OrderBy(e => e.Id).FirstOrDefault();

                    var OHolidayCalendarChk = OCompany.HolidayCalendar.OrderBy(e => e.Id).ToList();
                    if (OHolidayCalendarChk != null)
                    {
                        var OHolidayList = OHolidayCalendarChk
              .OrderBy(e => e.Id).ToList();

                        for (var mdatep = pTodate.Date.AddDays(+1).Date; mdatep <= pTodate.Date.AddDays(+10).Date; mdatep = mdatep.AddDays(+1))
                        {
                            //var aa = 0;
                            //foreach (var ca in OHolidayList)
                            //{
                            //    var OHolidayChk = ca.HolidayList.Where(e => e.HolidayDate.Value == mdatep
                            //        // && OLocHolidayNameList.HoliCalendar.Default == true
                            //        ).OrderBy(e => e.Id).FirstOrDefault();
                            var OHolidayChk = OHolidayList.SelectMany(e => e.HolidayList.Where(t => t.HolidayDate.Value == mdatep)
                                                  ).OrderBy(e => e.Id).ToList();
                            double OnWeaklyOff = DayDateIsWeaklyOff(mdatep, OEmployee.Id);
                            //var listLvscnt = DayLeaveHeadValidation(mdatep, CompanyId, EmployeeId, OLeaveCalendar, OLvNewReq.LeaveHead.Id, lvrec);
                            if ((OHolidayChk.Count() == 0) && (OnWeaklyOff != 0))
                            {
                                break;

                            }
                            else
                            {
                                if (OnWeaklyOff == 0 || OHolidayChk.Count() > 0)
                                {
                                    mSuffix = mSuffix + 1;

                                }
                            }


                        }

                    }


                    List<GetResumeDate> returndata = new List<GetResumeDate>();

                    returndata.Add(new GetResumeDate
                    {
                        Resumedate = pTodate.AddDays(mSuffix + 1).ToShortDateString(),
                        samedatehalfdayappl = (pTodate.Date - pfromdate.Date).Days + 1,
                        halfdayappl = halfday,

                    });
                    return Json(returndata, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LvNewReq L, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            try
            {
                string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
                string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
                string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
                var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                if (ModelState.IsValid)
                {
                    try
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                LvCreditPolicy blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
                                                            .Include(e => e.ConvertLeaveHeadBal)
                                                            .Include(e => e.ExcludeLeaveHeads)
                                                            .Include(e => e.CreditDate)
                                                            .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                L.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                if (LvHeadList != null)
                                {
                                    if (LvHeadList != "")
                                    {
                                        var val = db.LvHead.Find(int.Parse(LvHeadList));
                                        L.LeaveHead = val;

                                        var type = db.LvNewReq.Include(e => e.LeaveHead).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvNewReq> typedetails = null;
                                        if (type.LeaveHead != null)
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.LeaveHead.Id == type.LeaveHead.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.LeaveHead = L.LeaveHead;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.LvNewReq.Include(e => e.LeaveHead).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.LeaveHead = null;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }
                                else
                                {
                                    var CreditdateypeDetails = db.LvNewReq.Include(e => e.LeaveHead).Where(x => x.Id == data).ToList();
                                    foreach (var s in CreditdateypeDetails)
                                    {
                                        s.LeaveHead = null;
                                        db.LvNewReq.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                if (ContactNosList != null)
                                {
                                    if (ContactNosList != "")
                                    {
                                        var val = db.ContactNumbers.Find(int.Parse(ContactNosList));
                                        L.ContactNo = val;

                                        var type = db.LvNewReq.Include(e => e.ContactNo).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvNewReq> typedetails = null;
                                        if (type.ContactNo != null)
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.ContactNo.Id == type.ContactNo.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.ContactNo = L.ContactNo;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.LvNewReq.Include(e => e.ContactNo).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.ContactNo = null;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }
                                else
                                {
                                    var CreditdateypeDetails = db.LvNewReq.Include(e => e.ContactNo).Where(x => x.Id == data).ToList();
                                    foreach (var s in CreditdateypeDetails)
                                    {
                                        s.ContactNo = null;
                                        db.LvNewReq.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }


                                if (calendar != null)
                                {
                                    if (calendar != "")
                                    {
                                        var val = db.Calendar.Find(int.Parse(ContactNosList));
                                        L.LeaveCalendar = val;

                                        var type = db.LvNewReq.Include(e => e.LeaveCalendar).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvNewReq> typedetails = null;
                                        if (type.LeaveCalendar != null)
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.LeaveCalendar.Id == type.LeaveCalendar.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.LeaveCalendar = L.LeaveCalendar;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.LvNewReq.Include(e => e.LeaveCalendar).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.LeaveCalendar = null;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }
                                else
                                {
                                    var CreditdateypeDetails = db.LvNewReq.Include(e => e.LeaveCalendar).Where(x => x.Id == data).ToList();
                                    foreach (var s in CreditdateypeDetails)
                                    {
                                        s.LeaveCalendar = null;
                                        db.LvNewReq.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                if (FrmStat != null)
                                {
                                    if (FrmStat != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(FrmStat));
                                        L.FromStat = val;

                                        var type = db.LvNewReq.Include(e => e.FromStat).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvNewReq> typedetails = null;
                                        if (type.FromStat != null)
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.FromStat.Id == type.FromStat.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.FromStat = L.FromStat;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.LvNewReq.Include(e => e.FromStat).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.FromStat = null;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }
                                else
                                {
                                    var CreditdateypeDetails = db.LvNewReq.Include(e => e.FromStat).Where(x => x.Id == data).ToList();
                                    foreach (var s in CreditdateypeDetails)
                                    {
                                        s.FromStat = null;
                                        db.LvNewReq.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }

                                if (Tostat != null)
                                {
                                    if (Tostat != "")
                                    {
                                        var val = db.LookupValue.Find(int.Parse(Tostat));
                                        L.ToStat = val;

                                        var type = db.LvNewReq.Include(e => e.ToStat).Where(e => e.Id == data).SingleOrDefault();
                                        IList<LvNewReq> typedetails = null;
                                        if (type.ToStat != null)
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.ToStat.Id == type.ToStat.Id && x.Id == data).ToList();
                                        }
                                        else
                                        {
                                            typedetails = db.LvNewReq.Where(x => x.Id == data).ToList();
                                        }
                                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                        foreach (var s in typedetails)
                                        {
                                            s.ToStat = L.FromStat;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    else
                                    {
                                        var WFTypeDetails = db.LvNewReq.Include(e => e.ToStat).Where(x => x.Id == data).ToList();
                                        foreach (var s in WFTypeDetails)
                                        {
                                            s.ToStat = null;
                                            db.LvNewReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                }
                                else
                                {
                                    var CreditdateypeDetails = db.LvNewReq.Include(e => e.ToStat).Where(x => x.Id == data).ToList();
                                    foreach (var s in CreditdateypeDetails)
                                    {
                                        s.ToStat = null;
                                        db.LvNewReq.Attach(s);
                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = s.RowVersion;
                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    }
                                }
                                var CurCorp = db.LvCreditPolicy.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    LvNewReq Lvnewreq = new LvNewReq()
                                    {
                                        ReqDate = L.ReqDate,
                                        ResumeDate = L.ResumeDate,
                                        FromDate = L.FromDate,
                                        ToDate = L.ToDate,
                                        FromStat = L.FromStat,
                                        ToStat = L.ToStat,
                                        DebitDays = L.DebitDays,
                                        Reason = L.Reason,
                                        Id = data,
                                        DBTrack = L.DBTrack
                                    };
                                    db.LvNewReq.Attach(Lvnewreq);
                                    db.Entry(Lvnewreq).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(Lvnewreq).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                }
                                // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    //var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                    //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
                                    //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
                                    //db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                            }
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (LvCreditPolicy)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                            L.RowVersion = databaseValues.RowVersion;

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



        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvNewReq
                    .Include(e => e.FromStat)
                    .Include(e => e.ToStat)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ReqDate = e.ReqDate,
                        FromDate = e.FromDate,
                        ToDate = e.ToDate,
                        DebitDays = e.DebitDays,
                        Reason = e.Reason,
                        FromStat_Id = e.FromStat.Id == null ? 0 : e.FromStat.Id,
                        ToStat_Id = e.ToStat.Id == null ? 0 : e.ToStat.Id,
                        Lvcalendar_Id = e.LeaveCalendar.Id == null ? 0 : e.LeaveCalendar.Id,
                        Lvcalendar_Fulldetails = e.LeaveCalendar.FullDetails == null ? "" : e.LeaveCalendar.FullDetails,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.LvNewReq
                  .Include(e => e.ContactNo)
                    .Include(e => e.LeaveHead)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        ContactNo_FullAddress = e.ContactNo.FullContactNumbers == null ? "" : e.ContactNo.FullContactNumbers,
                        ContactNo_Id = e.ContactNo.Id == null ? "" : e.ContactNo.Id.ToString(),
                        LvHead_Id = e.LeaveHead.Id == null ? "" : e.LeaveHead.Id.ToString(),
                        LvHead_FullDetails = e.LeaveHead.FullDetails == null ? "" : e.LeaveHead.FullDetails
                    }).ToList();


                var W = db.DT_LvNewReq
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ReqDate = e.ReqDate == null ? "" : e.ReqDate.ToString(),
                         FromDate = e.FromDate == null ? "" : e.FromDate.ToString(),
                         ToDate = e.ToDate == null ? "" : e.FromDate.ToString(),
                         DebitDays = e.DebitDays == null ? "" : e.FromDate.ToString(),
                         Reason = e.Reason,
                         FromStat_Val = e.FromStat_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.FromStat_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         ToStat_Val = e.ToStat_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.ToStat_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),
                         Contact_Val = e.ContactNo_Id == 0 ? "" : db.ContactNumbers.Where(x => x.Id == e.ContactNo_Id).Select(x => x.FullContactNumbers).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var NewReq = db.LvNewReq.Find(data);
                TempData["RowVersion"] = NewReq.RowVersion;
                var Auth = NewReq.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }


        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save Authorized data
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                            Corporate corp = db.Corporate.Include(e => e.Address)
                                .Include(e => e.ContactDetails)
                                .Include(e => e.BusinessType).FirstOrDefault(e => e.Id == auth_id);

                            corp.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = corp.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Corporate.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorized");
                            return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        Corporate Old_Corp = db.Corporate.Include(e => e.BusinessType)
                                                          .Include(e => e.Address)
                                                          .Include(e => e.ContactDetails).Where(e => e.Id == auth_id).SingleOrDefault();


                        DT_Corporate Curr_Corp = db.DT_Corporate
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_Corp != null)
                        {
                            Corporate corp = new Corporate();

                            string Corp = Curr_Corp.BusinessType_Id == null ? null : Curr_Corp.BusinessType_Id.ToString();
                            string Addrs = Curr_Corp.Address_Id == null ? null : Curr_Corp.Address_Id.ToString();
                            string ContactDetails = Curr_Corp.ContactDetails_Id == null ? null : Curr_Corp.ContactDetails_Id.ToString();
                            corp.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                            corp.Code = Curr_Corp.Code == null ? Old_Corp.Code : Curr_Corp.Code;
                            //      corp.Id = auth_id;

                            if (ModelState.IsValid)
                            {
                                try
                                {

                                    //DbContextTransaction transaction = db.Database.BeginTransaction();

                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        // db.Configuration.AutoDetectChangesEnabled = false;
                                        corp.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                            CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(Corp, Addrs, ContactDetails, auth_id, corp, corp.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorized");
                                        return Json(new Utility.JsonReturnClass { Id = corp.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { corp.Id, "Record Authorized", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (Corporate)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                    }
                                    else
                                    {
                                        var databaseValues = (Corporate)databaseEntry.ToObject();
                                        corp.RowVersion = databaseValues.RowVersion;
                                    }
                                }
                                Msg.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            }
                        }
                        else
                        {
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        // return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //Corporate corp = db.Corporate.Find(auth_id);
                            Corporate corp = db.Corporate.AsNoTracking().Include(e => e.Address)
                                                                        .Include(e => e.BusinessType)
                                                                        .Include(e => e.ContactDetails).FirstOrDefault(e => e.Id == auth_id);

                            Address add = corp.Address;
                            ContactDetails conDet = corp.ContactDetails;
                            LookupValue val = corp.BusinessType;

                            corp.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                                CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                                CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.Corporate.Attach(corp);
                            db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corp.DBTrack);
                            DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            DT_Corp.Address_Id = corp.Address == null ? 0 : corp.Address.Id;
                            DT_Corp.BusinessType_Id = corp.BusinessType == null ? 0 : corp.BusinessType.Id;
                            DT_Corp.ContactDetails_Id = corp.ContactDetails == null ? 0 : corp.ContactDetails.Id;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            Msg.Add(" Record Authorized ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //eturn Json(new Object[] { "", "Record Authorized", JsonRequestBehavior.AllowGet });
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

        public int EditS(string Corp, string Addrs, string ContactDetails, int data, Corporate c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Corp));
                        c.BusinessType = val;

                        var type = db.Corporate.Include(e => e.BusinessType).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> typedetails = null;
                        if (type.BusinessType != null)
                        {
                            typedetails = db.Corporate.Where(x => x.BusinessType.Id == type.BusinessType.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.BusinessType = c.BusinessType;
                            db.Corporate.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.BusinessType = null;
                            db.Corporate.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var BusiTypeDetails = db.Corporate.Include(e => e.BusinessType).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.BusinessType = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Address.Find(int.Parse(Addrs));
                        c.Address = val;

                        var add = db.Corporate.Include(e => e.Address).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> addressdetails = null;
                        if (add.Address != null)
                        {
                            addressdetails = db.Corporate.Where(x => x.Address.Id == add.Address.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Address = c.Address;
                                db.Corporate.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.Corporate.Include(e => e.Address).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Address = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (ContactDetails != null)
                {
                    if (ContactDetails != "")
                    {
                        var val = db.ContactDetails.Find(int.Parse(ContactDetails));
                        c.ContactDetails = val;

                        var add = db.Corporate.Include(e => e.ContactDetails).Where(e => e.Id == data).SingleOrDefault();
                        IList<Corporate> contactsdetails = null;
                        if (add.ContactDetails != null)
                        {
                            contactsdetails = db.Corporate.Where(x => x.ContactDetails.Id == add.ContactDetails.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            contactsdetails = db.Corporate.Where(x => x.Id == data).ToList();
                        }
                        foreach (var s in contactsdetails)
                        {
                            s.ContactDetails = c.ContactDetails;
                            db.Corporate.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                }
                else
                {
                    var contactsdetails = db.Corporate.Include(e => e.ContactDetails).Where(x => x.Id == data).ToList();
                    foreach (var s in contactsdetails)
                    {
                        s.ContactDetails = null;
                        db.Corporate.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                var CurCorp = db.Corporate.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Corporate corp = new Corporate()
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.Corporate.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    LvNewReq LvNewReq = db.LvNewReq.Include(e => e.LeaveHead)
                                                       .Include(e => e.ContactNo).Include(e => e.ToStat)
                                                       .Include(e => e.FromStat).Where(e => e.Id == data).SingleOrDefault();

                    ContactNumbers ContNos = LvNewReq.ContactNo;
                    LvHead LvHead = LvNewReq.LeaveHead;
                    LookupValue FromStat = LvNewReq.FromStat;
                    LookupValue ToStat = LvNewReq.ToStat;

                    if (LvNewReq.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = LvNewReq.DBTrack.CreatedBy != null ? LvNewReq.DBTrack.CreatedBy : null,
                                CreatedOn = LvNewReq.DBTrack.CreatedOn != null ? LvNewReq.DBTrack.CreatedOn : null,
                                IsModified = LvNewReq.DBTrack.IsModified == true ? true : false
                            };
                            LvNewReq.DBTrack = dbT;
                            db.Entry(LvNewReq).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, LvNewReq.DBTrack);
                            DT_LvNewReq DT_LvReq = (DT_LvNewReq)rtn_Obj;
                            DT_LvReq.LeaveHead_Id = LvNewReq.LeaveHead == null ? 0 : LvNewReq.LeaveHead.Id;
                            DT_LvReq.LeaveCalendar_Id = LvNewReq.LeaveCalendar == null ? 0 : LvNewReq.LeaveCalendar.Id;
                            DT_LvReq.ContactNo_Id = LvNewReq.ContactNo == null ? 0 : LvNewReq.ContactNo.Id;
                            DT_LvReq.FromStat_Id = LvNewReq.FromStat == null ? 0 : LvNewReq.LeaveCalendar.Id;
                            DT_LvReq.ToStat_Id = LvNewReq.ToStat == null ? 0 : LvNewReq.ToStat.Id;
                            db.Create(DT_LvReq);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = LvNewReq.DBTrack.CreatedBy != null ? LvNewReq.DBTrack.CreatedBy : null,
                                    CreatedOn = LvNewReq.DBTrack.CreatedOn != null ? LvNewReq.DBTrack.CreatedOn : null,
                                    IsModified = LvNewReq.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(LvNewReq).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                                DT_LvNewReq DT_LvReq = (DT_LvNewReq)rtn_Obj;
                                DT_LvReq.LeaveHead_Id = LvNewReq.LeaveHead == null ? 0 : LvNewReq.LeaveHead.Id;
                                DT_LvReq.LeaveCalendar_Id = LvNewReq.LeaveCalendar == null ? 0 : LvNewReq.LeaveCalendar.Id;
                                DT_LvReq.ContactNo_Id = LvNewReq.ContactNo == null ? 0 : LvNewReq.ContactNo.Id;
                                DT_LvReq.FromStat_Id = LvNewReq.FromStat == null ? 0 : LvNewReq.LeaveCalendar.Id;
                                DT_LvReq.ToStat_Id = LvNewReq.ToStat == null ? 0 : LvNewReq.ToStat.Id;
                                db.Create(DT_LvReq);

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
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public class LvNewReqChildDataClass
        {
            public int Id { get; set; }
            public string LvHead { get; set; }
            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string TotalDays { get; set; }
            public string Reason { get; set; }
            public bool IsCancel { get; set; }
            public string Status { get; set; }
            public string CreditDays { get; set; }
            public string FromStat { get; set; }
            public string DebitDays { get; set; }
            public string ToStat { get; set; }
            public string LvyrFrom { get; set; }
            public string LvyrTo { get; set; }
            public string Incharge { get; set; }
            public string GetMonth { get; set; }
        }
        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeeLeave.Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null)
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct)
                        .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        .Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job)
                        .Include(e => e.Employee.PayStruct)
                        .Include(e => e.Employee.PayStruct.Grade)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeeLeave> fall;

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

                    Func<EmployeeLeave, string> orderfunc = (c =>
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
                                JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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

        public ActionResult Get_LvNewReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LvNewReqChildDataClass> returndata = new List<LvNewReqChildDataClass>();
                List<LvNewReqChildDataClass> resultdata = new List<LvNewReqChildDataClass>();
                var LvCalendard = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();

                try
                {
                    var db_data = db.EmployeeLeave
                        .Include(e => e.LvNewReq.Select(q => q.LeaveCalendar))
                        .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        .Include(e => e.LvNewReq.Select(a => a.ToStat))
                        .Include(e => e.LvNewReq.Select(a => a.FromStat))
                        .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                        .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                        .Include(e => e.LvNewReq.Select(a => a.Incharge.EmpName))
                         .Where(e => e.Id == data).SingleOrDefault();


                    if (db_data != null)
                    {
                        List<int> lvcode = db_data.LvNewReq.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                        foreach (var item1 in lvcode)
                        {
                            DateTime? lvcrdate = db_data.LvNewReq.Where(e => e.LeaveHead.Id == item1 && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                            if (lvcrdate != null)
                            {
                                DateTime? Lvyearfrom = lvcrdate;
                                DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                LvyearTo = LvyearTo.Value.AddYears(1);

                                foreach (var item in db_data.LvNewReq.Where(a => a.WFStatus.LookupVal != "8" && a.WFStatus.LookupVal != "2" && a.WFStatus.LookupVal != "0" && a.WFStatus.LookupVal != "3" && a.FromDate >= Lvyearfrom && a.FromDate <= LvyearTo).ToList())
                                {
                                    //if (item.IsCancel == false)
                                    //{
                                    var status = "--";
                                    if ((item.InputMethod == 1 || item.InputMethod == 2) && item.LvWFDetails.Count > 0)
                                    {
                                        status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                                        .Select(e => e.Value).SingleOrDefault();
                                    }
                                    if (item.InputMethod == 0)
                                    {
                                        // status = "Approved By HRM (M)";
                                        status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                                        .Select(e => e.Value).SingleOrDefault();
                                        if (status == null || status == "")
                                        {
                                            status = "Approved By HRM (M)";
                                        }
                                    }
                                    resultdata.Add(new LvNewReqChildDataClass
                                    {
                                        Id = item.Id,
                                        LvHead = item.LeaveHead != null ? item.LeaveHead.LvCode : null,
                                        ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null,
                                        FromDate = item.FromDate != null ? item.FromDate.Value.ToString("dd/MM/yyyy") : null,
                                        ToDate = item.ToDate != null ? item.ToDate.Value.ToString("dd/MM/yyyy") : null,
                                        IsCancel = item.IsCancel,
                                        DebitDays = item.DebitDays.ToString(),
                                        CreditDays = item.CreditDays.ToString(),
                                        FromStat = item.FromStat != null ? item.FromStat.LookupVal.ToString() : null,
                                        ToStat = item.ToStat != null ? item.ToStat.LookupVal.ToString() : null,
                                        TotalDays = item.DebitDays.ToString(),
                                        // Status = Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
                                        Status = status,
                                        Reason = item.Reason,
                                        LvyrFrom = item.LeaveCalendar != null ? item.LeaveCalendar.FromDate.Value.ToString("dd/MM/yyyy") : null,
                                        LvyrTo = item.LeaveCalendar != null ? item.LeaveCalendar.ToDate.Value.ToShortDateString() : "",
                                        Incharge = item.Incharge == null ? "" : item.Incharge.EmpName == null ? "" : item.Incharge.EmpName.FullNameFML == null ? "" : item.Incharge.EmpName.FullNameFML.ToString(),
                                        GetMonth = item.FromDate != null ? item.FromDate.Value.Month.ToString() : ""
                                    });
                                    // }
                                }
                                foreach (var slotse in resultdata.GroupBy(m => m.FromDate).Select(r => r.Last()))
                                {
                                    returndata.Add(new LvNewReqChildDataClass
                                    {
                                        Id = slotse.Id,
                                        LvHead = slotse.LvHead,
                                        ReqDate = slotse.ReqDate,
                                        FromDate = slotse.FromDate,
                                        ToDate = slotse.ToDate,
                                        IsCancel = slotse.IsCancel,
                                        DebitDays = slotse.DebitDays,
                                        CreditDays = slotse.CreditDays,
                                        FromStat = slotse.FromStat,
                                        ToStat = slotse.ToStat,
                                        TotalDays = slotse.DebitDays,
                                        // Status = Utility.GetStatusName().Where(e => item.WFStatus != null && e.Key == item.WFStatus.LookupVal).Select(e => e.Value).SingleOrDefault(),
                                        Status = slotse.Status,
                                        Reason = slotse.Reason,
                                        LvyrFrom = slotse.LvyrFrom,
                                        LvyrTo = slotse.LvyrTo,
                                        Incharge = slotse.Incharge,
                                        GetMonth = slotse.GetMonth
                                    });
                                }
                                var result = returndata.OrderByDescending(e => e.GetMonth).ThenByDescending(s => s.FromDate).ToList();
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                        }
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
            return null;
        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvNewReq
                 .Include(e => e.FromStat)
                 .Include(e => e.ToStat)
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (e => new
                 {
                     ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                     FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
                     ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
                     TotalDays = e.DebitDays,
                     Reason = e.Reason,
                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault(); // added by rekha 26-12-16
                    var selectedid = qurey.Where(e => e.LookupVal == "FULLSESSION").Select(e => e.Id.ToString()).SingleOrDefault();
                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey != null)
                    {
                        s = new SelectList(qurey, "Id", "LookupVal", selectedid);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult Polulate_LeaveCalendar(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").ToList();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult UploadLeaveCertificate()
        {
            return View("~/Views/Shared/_UploadInvestment.cshtml");
            //D:\LATESTCHECKOUT\P2bUltimate\P2BUltimate\Views\Shared\_Upload.cshtml
        }
        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\" + FolderName + "\\";
            String localPath = "";
            bool exists = System.IO.Directory.Exists(requiredPath);
            if (!exists)
            {
                localPath = new Uri(requiredPath).LocalPath;
                System.IO.Directory.CreateDirectory(localPath);
            }
            return localPath;
        }
        [HttpPost]
        public ActionResult LeaveCertificateUpload(HttpPostedFileBase[] files, FormCollection form, string data, string Id, string SubId)
        {
            if (ModelState.IsValid)
            {
                List<string> Msg = new List<string>();
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string Fromdate = form["FromDate"] == "0" ? "" : form["FromDate"];
                string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                string ReqDate = DateTime.Now.ToShortDateString();
                string ReqTime = DateTime.Now.ToShortTimeString();
                string s1 = "";
                string s2 = "";
                if (ReqTime!=null)
                {
                    string[] values = (ReqTime.Split(new string[] { ":" }, StringSplitOptions.None));
                    s1 = values.ElementAtOrDefault(0);
                    s2 = values.ElementAtOrDefault(1);
                }
                int EmpId = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    EmpId = int.Parse(Emp);
                }
                else
                {
                    Msg.Add("  Please Select Employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                }
                string EmpCode = "";
                string LvCode = "";

                int Empid = int.Parse(Emp);
                Session["FilePath"] = null;
                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;
                string investmentid = "0", empcode = "";
                LvNewReq lvnewreqcertificate = null;
                // ITSubInvestmentPayment itsubinvestment = null;
                using (DataBaseContext db = new DataBaseContext())
                {
                    var Employeevar = db.Employee.Where(e => e.Id == Empid).SingleOrDefault();
                    if (Employeevar != null)
                    {
                        EmpCode = Employeevar.EmpCode;
                    }
                    if (LvHeadList != null && LvHeadList != "")
                    {
                        var val = db.LvHead.Find(int.Parse(LvHeadList));
                        LvCode = val.LvCode;
                    }

                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        lvnewreqcertificate = db.LvNewReq.Where(e => e.Id == itid).SingleOrDefault();



                        List<EmployeeLeave> emcodedata = db.EmployeeLeave.
                                                       Include(e => e.Employee)
                                                     .Include(e => e.LvNewReq).ToList();
                        foreach (var item in emcodedata)
                        {
                            var emplvdata = item.LvNewReq.Where(e => e.Id == itid).SingleOrDefault();
                            if (emplvdata != null)
                            {
                                empcode = item.Employee.EmpCode;
                            }
                        }

                        //if (itinvestment.ITInvestment != null)
                        //{
                        //    investmentid = itinvestment.ITInvestment.Id.ToString();
                        //}
                        investmentid = lvnewreqcertificate.Id.ToString();
                        //OFinYr = itinvestment.FinancialYear.Id.ToString();
                        //fromdate = itinvestment.FinancialYear.FromDate.Value.Year.ToString();
                        //todate = itinvestment.FinancialYear.ToDate.Value.Year.ToString();
                        deletefilepath = lvnewreqcertificate.Path;
                    }

                    var Module_Name = HttpContext.Session["ModuleType"];
                    string ModuleName = Module_Name.ToString();

                    //if (SubId != null && SubId != "")
                    //{
                    //    int subid = Convert.ToInt32(SubId);
                    //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                    //    subinvestmentid = SubId;
                    //    deletefilepath = itsubinvestment.Path;
                    //}
                    var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", ".jpeg", ".pdf" };
                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file == null)
                        {
                            return Json(new { success = false, responseText = "Please Select The File..!" }, JsonRequestBehavior.AllowGet);
                        }
                        extension = Path.GetExtension(file.FileName);
                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, responseText = "File Type Is Not Supported..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    foreach (HttpPostedFileBase file in files)
                    {

                        if (file != null)
                        {
                            
                            //extension = Path.GetExtension(file.FileName);26/11/2024
                            //newfilename = Id + extension;
                            //String FolderName = "LeaveCertificate" + "\\" + empcode + "\\";

                            //var InputFileName = Path.GetFileName(file.FileName);
                            //string ServerSavePath = InvestmentUploadFile(FolderName);
                            //string ServerMappath = ServerSavePath + newfilename;26/11/2024
                           
                            extension = Path.GetExtension(file.FileName);
                            newfilename = LvCode + "-" + Fromdate.Replace("/", "") + "-" + ReqDate.Replace("/", "")+"-"+s1+"-"+s2+extension;
                            String FolderName = Empid + "\\" +ModuleName+"\\" +"LeaveCertificate";
                           
                            //var InputFileName = Path.GetFileName(file.FileName);
                            string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                            if (ServerSavePath == null)
                            {
                                return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                            }
                            string ServerMappath = ServerSavePath + FolderName +"\\"+ newfilename;
                            deletefilepath = ServerMappath;

                            if (deletefilepath != null && deletefilepath != "")
                            {
                                FileInfo File = new FileInfo(deletefilepath);
                                bool exists = File.Exists;
                                if (exists)
                                {
                                    System.IO.File.Delete(deletefilepath);
                                }
                            }
                            if (!Directory.Exists(ServerSavePath + FolderName))
                            {
                                Directory.CreateDirectory(ServerSavePath + FolderName);
                            }
                            file.SaveAs(Path.Combine(ServerMappath));

                            Session["FilePath"] = FolderName + "\\" + newfilename;
                            
                            if (Id != null)
                            {
                                db.LvNewReq.Attach(lvnewreqcertificate);
                                db.Entry(lvnewreqcertificate).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = lvnewreqcertificate.RowVersion;
                                db.Entry(lvnewreqcertificate).State = System.Data.Entity.EntityState.Detached;
                                lvnewreqcertificate.DBTrack = new DBTrack
                                {
                                    CreatedBy = lvnewreqcertificate.DBTrack.CreatedBy == null ? null : lvnewreqcertificate.DBTrack.CreatedBy,
                                    CreatedOn = lvnewreqcertificate.DBTrack.CreatedOn == null ? null : lvnewreqcertificate.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };
                                LvNewReq ContactDet = lvnewreqcertificate;
                                ContactDet.Path = FolderName + "\\" + newfilename;
                                ContactDet.DBTrack = lvnewreqcertificate.DBTrack;

                                db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                db.SaveChanges();
                            }
                            //else
                            //{
                            //    db.ITSubInvestmentPayment.Attach(itsubinvestment);
                            //    db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    TempData["RowVersion"] = itsubinvestment.RowVersion;
                            //    db.Entry(itsubinvestment).State = System.Data.Entity.EntityState.Detached;
                            //    itsubinvestment.DBTrack = new DBTrack
                            //    {
                            //        CreatedBy = itsubinvestment.DBTrack.CreatedBy == null ? null : itsubinvestment.DBTrack.CreatedBy,
                            //        CreatedOn = itsubinvestment.DBTrack.CreatedOn == null ? null : itsubinvestment.DBTrack.CreatedOn,
                            //        Action = "M",
                            //        ModifiedBy = SessionManager.UserName,
                            //        ModifiedOn = DateTime.Now
                            //    };
                            //    ITSubInvestmentPayment ContactDet = itsubinvestment;
                            //    ContactDet.Path = ServerMappath;
                            //    ContactDet.DBTrack = itsubinvestment.DBTrack;

                            //    db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                            //    db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //    db.SaveChanges();
                            //}
                            Count++;
                        }
                        else
                        {

                        }
                    }

                    int LvHeadId = form["LvHeadlist"] != null ? int.Parse(form["LvHeadlist"]) : 0;
                    double DebitDays = form["DebitDays"] != null ? int.Parse(form["DebitDays"]) : 0;

                    // upload certificate if leave taken after no of times ex. if 3 time take SL then not require after that require certificate start
                    var leavecalendarid = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    EmployeeLeave OEmployeeLeavechk = null;
                    OEmployeeLeavechk = db.EmployeeLeave.Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(y => y.LeaveCalendar))
                   .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
               .Where(e => e.Employee.Id == Empid).SingleOrDefault();
                    var LvOrignal_idchk = OEmployeeLeavechk.LvNewReq.Where(e => e.LvOrignal != null && e.LeaveHead_Id == LvHeadId).Select(e => e.LvOrignal.Id).ToList();
                    var AntCancelchk = OEmployeeLeavechk.LvNewReq.Where(e => e.IsCancel == false && e.TrReject == false && e.LeaveHead_Id == LvHeadId && e.LeaveCalendar_Id == leavecalendarid.Id).OrderBy(e => e.Id).ToList();
                    var listLvschk = AntCancelchk.Where(e => !LvOrignal_idchk.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.Path == "").OrderBy(e => e.Id).ToList();


                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool exists1 = System.IO.Directory.Exists(requiredPath);
                    string localPath;
                    if (!exists1)
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + @"\Leavecertificateonreqcount" + ".ini";
                    localPath = new Uri(path).LocalPath;
                    if (!System.IO.File.Exists(localPath))
                    {

                        using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
                        {
                            StreamWriter str = new StreamWriter(fs);
                            str.BaseStream.Seek(0, SeekOrigin.Begin);

                            str.Flush();
                            str.Close();
                            fs.Close();
                        }


                    }

                    string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\App_Data\\Menu_Json";
                    bool existschk = System.IO.Directory.Exists(requiredPathchk);
                    string localPathchk;
                    if (!existschk)
                    {
                        localPath = new Uri(requiredPathchk).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string pathchk = requiredPathchk + @"\Leavecertificateonreqcount" + ".ini";
                    localPathchk = new Uri(pathchk).LocalPath;
                    string Leave_code = "";
                    int Lvreqcnt = 0;
                    int paramcnt = 0;
                    using (var streamReader = new StreamReader(localPathchk))
                    {
                        string line;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var LVCode = line.Split('_')[0];
                            var Reqparamcnt = line.Split('_')[1];
                            if (LVCode != "")
                            {
                                var val = db.LvHead.Where(e => e.Id == LvHeadId).FirstOrDefault();
                                if (val.LvCode == LVCode)
                                {
                                    Leave_code = val.LvCode;
                                    if (listLvschk != null)
                                    {
                                        Lvreqcnt = listLvschk.Count();
                                    }

                                    paramcnt = Convert.ToInt32(Reqparamcnt);
                                    break;
                                }
                            }
                        }
                    }
                    if (Leave_code != "")
                    {
                        if (LvHeadId != 0)
                        {
                            LvDebitPolicy ODebitPolicy = db.LvDebitPolicy.Where(e => e.LvHead_Id == LvHeadId).FirstOrDefault();

                            if (ODebitPolicy != null)
                            {
                                if (ODebitPolicy.IsCertificateAppl == true)
                                {
                                    if (Lvreqcnt + 1 > paramcnt)
                                    {

                                        if ((Lvreqcnt + 1 > paramcnt) && Session["FilePath"] == null)//Lvreqcnt =already avail leave req count and 1=new leave req
                                        {
                                            return Json(new { success = false, responseText = "Kindly upload certificate for this leave..!" }, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                                else
                                {
                                    if (Session["FilePath"] != null)
                                    {
                                        return Json(new { success = false, responseText = "You can't upload certificate for this leave..!" }, JsonRequestBehavior.AllowGet);
                                    }
                                }

                            }
                        }

                    }
                    // upload certificate if leave taken after no of times ex. if 3 time take SL then not require after that require certificate end


                    if (LvHeadId != 0 && Leave_code == "")
                    {
                        LvDebitPolicy ODebitPolicy = db.LvDebitPolicy.Where(e => e.LvHead_Id == LvHeadId).FirstOrDefault();

                        if (ODebitPolicy.IsCertificateAppl == true)
                        {
                            //if ((ODebitPolicy.MinLvDays >= DebitDays) && Session["FilePath"] != null)
                            //{
                            //    return Json(new { success = false, responseText = "You can't upload cerificate for this leave..!" }, JsonRequestBehavior.AllowGet);  
                            //}
                            if ((ODebitPolicy.MinLvDays <= DebitDays) && Session["FilePath"] == null)
                            {
                                return Json(new { success = false, responseText = "Kindly upload certificate for this leave..!" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            if (Session["FilePath"] != null)
                            {
                                return Json(new { success = false, responseText = "You can't upload certificate for this leave..!" }, JsonRequestBehavior.AllowGet);

                            }
                        }
                    }
                    if (Count > 0)
                    {
                        return Json(new { success = true, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { success = false, responseText = "Something is Wrong..!" }, JsonRequestBehavior.AllowGet);

                    //}
                }
                return View();

            }
            return View();
        }

        public ActionResult CheckLeveCertificateUpload(string id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                LvNewReq itinvestment = null;
                // ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "")
                {
                    int itid = Convert.ToInt32(id);
                    itinvestment = db.LvNewReq.Where(e => e.Id == itid).SingleOrDefault();

                }
                if (itinvestment.Path != null)
                {
                    localpath = itinvestment.Path;
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                //newfilename = OFinYr + "_" + investmentid + "_" + subinvestmentid + "_" + id + ".jpg";
                //String FolderName = "FinancialYear" + fromdate + "-" + todate + "\\Investment\\" + newfilename;
                //string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                //System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\P2BUltimate\\Images\\" + FolderName;
                //if (SubId == "")
                //{

                //    if (itinvestment.Path != null)
                //    {
                //        localpath = itinvestment.Path;
                //    }
                //    else
                //    {
                //        return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                //else
                //{
                //    int subid = Convert.ToInt32(SubId);
                //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                //    if (itsubinvestment.Path != null)
                //    {
                //        localpath = itsubinvestment.Path;
                //    }
                //    else
                //    {
                //        return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                //    }

                //}
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }
        public ActionResult CheckUploadLeaveFile(string id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                LvNewReq itinvestment = null;
                //  ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "")
                {
                    int itid = Convert.ToInt32(id);
                    itinvestment = db.LvNewReq.Where(e => e.Id == itid).SingleOrDefault();
                }

                if (itinvestment.Path != null)
                {
                    localpath = itinvestment.Path;
                    FileInfo File = new FileInfo(localpath);
                    bool iExists = File.Exists;
                    if (iExists)
                    {
                        localpath = localpath;
                    }
                    else
                    {
                        localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                    }
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }

                //else
                //{
                //    int subid = Convert.ToInt32(SubId);
                //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                //    if (itsubinvestment.Path != null)
                //    {
                //        localpath = itsubinvestment.Path;
                //    }
                //    else
                //    {
                //        return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                //    }

                //}
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }
        public ActionResult GetLeaveImage(string id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";
                LvNewReq itinvestment = null;
                // ITSubInvestmentPayment itsubinvestment = null;
                if (id != null && id != "")
                {
                    int itid = Convert.ToInt32(id);
                    itinvestment = db.LvNewReq.Where(e => e.Id == itid).SingleOrDefault();
                }
                if (itinvestment.Path != null)
                {
                    localpath = itinvestment.Path;
                    FileInfo File = new FileInfo(localpath);
                    bool iExists = File.Exists;
                    if (iExists)
                    {
                        localpath = localpath;
                    }
                    else
                    {
                        localpath = ConfigurationManager.AppSettings["EmployeeDocuments"] + localpath;
                    }
                }
                else
                {
                    return View("File Not Found");
                    // return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                }

                // else
                //{
                //    int subid = Convert.ToInt32(SubId);
                //    itsubinvestment = db.ITSubInvestmentPayment.Where(e => e.Id == subid).SingleOrDefault();
                //    if (itsubinvestment.Path != null)
                //    {
                //        localpath = itsubinvestment.Path;
                //    }
                //    else
                //    {
                //        return View("File Not Found");
                //        //return Content("File Not Found");
                //        //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension == ".pdf")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");
                        //string pdf="pdf";
                        //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                        //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension == ".jpg")
                    {
                        // return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension == ".png")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
                }
                return null;
            }


        }
    }
}