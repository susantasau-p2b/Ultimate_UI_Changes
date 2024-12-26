using Leave;
using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using System.Diagnostics;
using EssPortal.Security;
using EssPortal.Process;
using System.Web;
using System.IO;
using System.Data.Entity.Infrastructure;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using P2B.UTILS;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Configuration;


namespace EssPortal.Controllers
{
    [AuthoriseManger]
    public class ELMSController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/LvNewReq/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_LvNewReqGridPartial.cshtml");
        }
        public ActionResult Emp_Leave_History_Partial()
        {
            return View("~/Views/Shared/_EmpLvHistory.cshtml");
        }
        public ActionResult Emp_Leave_Span_History_Partial()
        {
            return View("~/Views/Shared/_EmpSpanLvHistory.cshtml");
        }
        public ActionResult LvHolidayList_Partial()
        {
            return View("~/Views/Shared/_LvHolidayList.cshtml");
        }
        public ActionResult LvWeaklyOff_Partial()
        {
            return View("~/Views/Shared/_LvWeaklyOff.cshtml");
        }

        public ActionResult Partial_LvopenbalofReqGrid()
        {
            return View("~/Views/Shared/_LvopenbalofReqGrid.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
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
                //   .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.OpenBal + openballvnewreq.CreditDays - openballvnewreq.LvLapsed, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
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
                // }

            }
            return null;
        }
        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new { Lvcalendardesc = "FromData :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString() }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetLookupCalendar(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
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

            public DateTime? ResumeDate { get; set; }

            public int? FromStat_Id { get; set; }

            public string FromStat { get; set; }

            public int? ToStat_Id { get; set; }

            public string ToStat { get; set; }
        }


        public ActionResult Process(LvNewReq LvReq, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
                string Emp = Session["TempEmpId"].ToString();
                string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
                string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
                //List<int> ids = null;
                //if (Emp != null && Emp != "0" && Emp != "false")
                //{
                //    ids = one_ids(Emp);
                //}
                int ids = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    //ids = one_ids(Emp);
                    ids = Convert.ToInt32(Emp);
                }
                else
                {
                    return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                }
                //var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                //if (calendar != null && calendar != "")
                //{
                var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                LvReq.LeaveCalendar = Cal;
                // }
                if (FrmStat != null && FrmStat != "")
                {
                    var value = db.LookupValue.Find(int.Parse(FrmStat));
                    LvReq.FromStat = value;
                }
                if (Tostat != null && Tostat != "")
                {
                    var value = db.LookupValue.Find(int.Parse(Tostat));
                    LvReq.ToStat = value;
                }
                if (LvHeadList != null && LvHeadList != "")
                {
                    var val = db.LvHead.Find(int.Parse(LvHeadList));
                    LvReq.LeaveHead = val;
                }

                if (ContactNosList != null && ContactNosList != "")
                {
                    int ContactNoId = Convert.ToInt32(ContactNosList);
                    var val = db.ContactNumbers.Where(e => e.Id == ContactNoId).SingleOrDefault();
                    LvReq.ContactNo = val;
                }
                Session["FilePath"] = null;
                Session["IsCertAppl"] = null;
                Session["IsCertOptional"] = null;
                var Comp_Id = 0;
                Comp_Id = Convert.ToInt32(Session["CompId"]);
                var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                Employee OEmployee = null;
                // EmployeeLeave OEmployeePayroll = null;
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

                };
                if (ids != null)
                {
                    //foreach (var i in ids)
                    //{
                    OEmployee = db.Employee
                      .Include(e => e.GeoStruct)
                      .Include(e => e.GeoStruct.Location)
                      .Include(e => e.FuncStruct)
                      .Include(e => e.PayStruct)
                      .Where(r => r.Id == ids)
                      .SingleOrDefault();

                    //  OEmployeePayroll
                    //  = db.EmployeeLeave.Include(e => e.LvNewReq)
                    //.Where(e => e.Employee.Id == i).SingleOrDefault();
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

                    // var lvcheck = OEmployeePayroll.LvNewReq.Where(e => e.TrClosed == false).ToList();
                    //if (lvcheck.Any(o => o.FromDate <= LvReq.FromDate && o.ToDate <= LvReq.ToDate))
                    //{

                    //    LeaveHeadProcess.ReturnData retD = new LeaveHeadProcess.ReturnData();
                    //    retD.ErrNo = 16;
                    //    retD.DebitDays = 0;

                    //    return Json(new { retD.ErrNo, retD.DebitDays });

                    //}
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 30, 0)))
                    {

                        try
                        {

                            LeaveHeadProcess.ReturnData retD = new LeaveHeadProcess.ReturnData();
                            var ComCode = db.Company.Where(e => e.Id == Comp_Id).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault().Code;

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
                                        ToStat_Id = Convert.ToInt32(Tostat),
                                        ResumeDate = LvReq.ResumeDate
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



                            //if (ComCode.ToUpper() == "MSCB")
                            //{
                            //    double pld = 0;
                            //    retD = LeaveHeadProcess.MSCLvValidate(LvNewReq, Comp_Id, OEmployeeLvId, LvReq.LeaveCalendar, Leaveyearfrom, LeaveyearTo, pld);
                            //    if (retD.ErrNo != 0)
                            //    {
                            //        return Json(new { retD.ErrNo });
                            //    }
                            //}
                            //retD = LeaveHeadProcess.LeaveValidation(LvNewReq, Comp_Id, OEmployeeLvId, LvReq.LeaveCalendar, Leaveyearfrom, LeaveyearTo);
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
                    //  }
                }
            }
            return Json(new Object[] { "", "", "Date Has Been Process" }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetLVHEAD(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empid = Convert.ToInt32(data2);
                SelectList s = (SelectList)null;
                var selected = "";

                // Goa urban spl leave can not apply before 5 year from joining/confirmation date
                string requiredPathLoan = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
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


                string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
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
                var query1 = db.EmployeeLeave.Where(e => e.Employee.Id == empid)
                    .Include(e => e.LvOpenBal.Select(r => r.LvHead))
                    .Include(e => e.LvOpenBal)
                    .Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                    .SingleOrDefault();


                if (data != "" && data != "0")
                {
                    selected = data;
                }
                var lvcalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).FirstOrDefault();

                // var LeavCheckingInlvnereq = query1.LvNewReq.Where(e => e.LeaveCalendar.Id == lvcalendar.Id).Select(e => e.LeaveHead).Distinct().ToList();
                var LeavCheckingInlvnereq = query1.LvNewReq.Where(e => e.LeaveHead.ESS == true && !oLvHeadid.Contains(e.LeaveHead.Id)).Select(t => t.LeaveHead).Distinct().ToList();

                if (LeavCheckingInlvnereq == null && LeavCheckingInlvnereq.Count() == 0)
                {
                    var query = query1.LvOpenBal.Where(e => e.LvCalendar.Id == lvcalendar.Id && e.LvHead.ESS == true && !oLvHeadid.Contains(e.LvHead.Id)).Select(e => e.LvHead).Distinct().ToList();
                    if (query != null)
                    {
                        s = new SelectList(query, "Id", "FullDetails", selected);
                    }
                    //}
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (LeavCheckingInlvnereq.Count() > 0)
                    {
                        s = new SelectList(LeavCheckingInlvnereq, "Id", "FullDetails", selected);
                    }
                    //}
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                return null;

            }
        }

        //CreateOLD ActionResult ESS
        //public ActionResult CreateOLD(LvNewReq LvReq, FormCollection form, String forwarddata, string DebitDays)
        //{

        //    string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
        //    string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
        //    string Emp = form["EmpLvnereq_Id"] == "0" ? "" : form["EmpLvnereq_Id"];
        //    string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
        //    string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
        //    DebitDays = form["DebitDays"] == "0" ? "" : form["DebitDays"];
        //    string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        int EmpId = 0;
        //        if (Emp != null && Emp != "0" && Emp != "false")
        //        {
        //            EmpId = int.Parse(Emp);
        //        }
        //        else
        //        {
        //            return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
        //        }
        //        //var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
        //        //if (calendar != null && calendar != "")
        //        //{
        //        //    var value = db.Calendar.Find(int.Parse(calendar));
        //        //    LvReq.LeaveCalendar = value;
        //        //}
        //        if (ddlIncharge != null && ddlIncharge != "-Select-")
        //        {
        //            var value = db.Employee.Find(int.Parse(ddlIncharge));
        //            LvReq.Incharge = value;

        //        }
        //        if (FrmStat != null && FrmStat != "")
        //        {
        //            var value = db.LookupValue.Find(int.Parse(FrmStat));
        //            LvReq.FromStat = value;
        //        }
        //        else
        //        {
        //            return Json(new Object[] { "", "", "Please Select Leave FromState " }, JsonRequestBehavior.AllowGet);
        //        }
        //        if (Tostat != null && Tostat != "")
        //        {
        //            var value = db.LookupValue.Find(int.Parse(Tostat));
        //            LvReq.ToStat = value;
        //        }
        //        else
        //        {
        //            return Json(new Object[] { "", "", "Please Select Leave Tostate " }, JsonRequestBehavior.AllowGet);
        //        }

        //        if (LvHeadList != null && LvHeadList != "")
        //        {
        //            var val = db.LvHead.Find(int.Parse(LvHeadList));
        //            LvReq.LeaveHead = val;
        //        }
        //        else
        //        {
        //            return Json(new Object[] { "", "", "Please Select Leave Head" }, JsonRequestBehavior.AllowGet);
        //        }

        //        if (ContactNosList != null && ContactNosList != "")
        //        {
        //            int ContactNoId = Convert.ToInt32(ContactNosList);
        //            var val = db.ContactNumbers.Where(e => e.Id == ContactNoId).SingleOrDefault();
        //            LvReq.ContactNo = val;
        //        }

        //        var Comp_Id = 0;
        //        Comp_Id = Convert.ToInt32(Session["CompId"]);
        //        var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
        //        Employee OEmployee = null;
        //        EmployeeLeave OEmployeeLeave = null;
        //        Company OCompany = null;
        //        OCompany = db.Company.Find(Comp_Id);
        //        LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //        /* Added By Rekha 04-03-2017*/
        //        LvReq.LeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
        //        int Empid = int.Parse(Emp);
        //        //var PrevReq = db.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id).OrderByDescending(e => e.Id).FirstOrDefault();
        //        OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal).Include(e => e.LvNewReq.Select(t => t.LeaveHead))
        //        .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar)).Where(e => e.Employee.Id == Empid).SingleOrDefault();

        //        //if (OEmployeeLeave.LvNewReq.Where(e => e.IsCancel == false || e.TrReject == false).Any(o => o.FromDate == LvReq.FromDate))
        //        //{
        //        //    return Json(new Object[] { "", "", "Requisition Already Exists.", JsonRequestBehavior.AllowGet });
        //        //}

        //        //var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
        //        var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id)
        //          .OrderByDescending(e => e.Id).FirstOrDefault();
        //        if (PrevReq != null)
        //        {
        //            LvReq.OpenBal = PrevReq.CloseBal;
        //            LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
        //            LvReq.LvOccurances = PrevReq.LvOccurances + 1;
        //        }
        //        else
        //        {
        //            var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvReq.LeaveHead.Id && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
        //            LvReq.OpenBal = PrevOpenBal.LvOpening;
        //            LvReq.LVCount = LvReq.DebitDays;
        //            LvReq.LvOccurances = 1;
        //        }

        //        OEmployee = db.Employee
        //                      .Include(e => e.GeoStruct)
        //                      .Include(e => e.GeoStruct.Location)
        //                      .Include(e => e.FuncStruct)
        //                      .Include(e => e.PayStruct).AsNoTracking()
        //                      .Where(r => r.Id == Empid).AsParallel()
        //                      .SingleOrDefault();

        //        var OEmployeePayroll
        //        = db.EmployeeLeave
        //      .Where(e => e.Employee.Id == Empid).SingleOrDefault();

        //        LvReq.GeoStruct = OEmployee.GeoStruct;
        //        LvReq.PayStruct = OEmployee.PayStruct;
        //        LvReq.FuncStruct = OEmployee.FuncStruct;

        //        // Check leave head policy as calendar,joining,increment start
        //        DateTime? Leaveyearfrom;
        //        DateTime? LeaveyearTo;
        //        LeaveHeadProcess.ReturnDatacalendarpara RetDataparam = new LeaveHeadProcess.ReturnDatacalendarpara();
        //        RetDataparam = LeaveHeadProcess.LeaveCalendarpara(LvReq.LeaveHead.Id, OEmployeeLeave.Id);
        //        if (RetDataparam.ErrNo != 0)
        //        {
        //            return Json(new { RetDataparam.ErrNo });
        //        }
        //        else
        //        {
        //            Leaveyearfrom = RetDataparam.Leaveyearfrom;
        //            LeaveyearTo = RetDataparam.LeaveyearTo;
        //        }


        //        // Check leave head policy as calendar,joining,increment End


        //        //var retD = LeaveHeadProcess.LeaveValidation(LvReq, Comp_Id, OEmployeeLeave.Id, LvReq.LeaveCalendar, Leaveyearfrom, LeaveyearTo);
        //        //LvReq.DebitDays = retD.DebitDays;
        //        //LvReq.PrefixCount = retD.LvnewReqprefix;
        //        //LvReq.SufixCount = retD.LvnewReqSuffix;
        //        //LvReq.PrefixSuffix = retD.PrefixSufix;
        //        LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
        //        LvReq.InputMethod = 1;  //apply through eeis source
        //        DateTime? OldToDate = LvReq.ToDate;
        //        double OldDebDays = LvReq.DebitDays;


        //        LvWFDetails oLvWFDetails = new LvWFDetails
        //        {
        //            WFStatus = 0,
        //            Comments = LvReq.Reason,
        //            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //        };

        //        List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
        //        oLvWFDetails_List.Add(oLvWFDetails);

        //        if (OCompany.Code.ToString() == "KDCC" && LvReq.LeaveHead.LvCode.ToUpper() == "SL")
        //        {
        //            //LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
        //            if (LvReq.CloseBal <= 0)
        //            {
        //                LvReq.CloseBal = 0;
        //                LvReq.DebitDays = LvReq.OpenBal;
        //                if (PrevReq != null)
        //                {
        //                    LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
        //                }
        //                else
        //                {
        //                    LvReq.LVCount = LvReq.DebitDays;

        //                }
        //                //LvReq.LVCount = LvReq.DebitDays;
        //                LvReq.ToDate = LvReq.FromDate.Value.AddDays(LvReq.DebitDays - 1);
        //            }
        //        }

        //        if (Session["FilePath"] != null)
        //        {
        //            LvReq.Path = Session["FilePath"].ToString();
        //        }

        //        LvNewReq LvNewReq = new LvNewReq()
        //        {
        //            ContactNo = LvReq.ContactNo,
        //            DebitDays = LvReq.DebitDays,
        //            FromDate = LvReq.FromDate,
        //            FromStat = LvReq.FromStat,
        //            LeaveHead = LvReq.LeaveHead,
        //            Reason = LvReq.Reason,
        //            ReqDate = LvReq.ReqDate,
        //            InputMethod = LvReq.InputMethod,
        //            ResumeDate = LvReq.ResumeDate,
        //            ToDate = LvReq.ToDate,
        //            ToStat = LvReq.ToStat,
        //            LeaveCalendar = LvReq.LeaveCalendar,
        //            DBTrack = LvReq.DBTrack,
        //            CloseBal = LvReq.CloseBal,
        //            OpenBal = LvReq.OpenBal,
        //            LVCount = LvReq.LVCount,
        //            //main
        //            LvOccurances = LvReq.LvOccurances,
        //            PrefixCount = LvReq.PrefixCount,
        //            SufixCount = LvReq.SufixCount,
        //            Incharge = LvReq.Incharge,
        //            TrClosed = false,
        //            Narration = "Leave Requisition",
        //            PrefixSuffix = LvReq.PrefixSuffix,
        //            WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
        //            LvWFDetails = oLvWFDetails_List,
        //            Path = LvReq.Path
        //        };
        //        List<LvNewReq> OFAT = new List<LvNewReq>();
        //        if (ModelState.IsValid)
        //        {

        //            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
        //                        .Where(r => r.Id == EmpId).SingleOrDefault();
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
        //                       new System.TimeSpan(0, 30, 0)))
        //            {
        //                try
        //                {
        //                    LvNewReq.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
        //                    LvNewReq.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
        //                    LvNewReq.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
        //                    db.LvNewReq.Add(LvNewReq);
        //                    if (Z.Company.Code.ToString() == "KDCC" && LvReq.LeaveHead.LvCode.ToUpper() == "SL")
        //                    {
        //                        if (LvReq.CloseBal == 0 && OldDebDays > LvReq.DebitDays)
        //                        {
        //                            LvWFDetails oLvWFDetails1 = new LvWFDetails
        //                            {
        //                                WFStatus = 0,
        //                                Comments = LvReq.Reason,
        //                                DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //                            };
        //                            List<LvWFDetails> oLvWFDetails_List1 = new List<LvWFDetails>();
        //                            oLvWFDetails_List1.Add(oLvWFDetails1);
        //                            OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal)
        //                           .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
        //                           .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
        //                           .Where(e => e.Employee.Id == Empid).SingleOrDefault();

        //                            PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.LvCode == "PL" && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
        //                                .OrderByDescending(e => e.Id).FirstOrDefault();

        //                            LvNewReq LvPLReq = new LvNewReq();

        //                            LvPLReq.DebitDays = OldDebDays - LvReq.DebitDays;

        //                            if (PrevReq != null)
        //                            {
        //                                LvPLReq.OpenBal = PrevReq.CloseBal;
        //                                LvPLReq.LVCount = PrevReq.LVCount + LvPLReq.DebitDays;
        //                                LvPLReq.LvOccurances = PrevReq.LvOccurances;
        //                                LvPLReq.LeaveHead = PrevReq.LeaveHead;
        //                            }
        //                            else
        //                            {
        //                                var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.LvCode.ToUpper() == "PL" && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
        //                                LvPLReq.OpenBal = PrevOpenBal.LvOpening;
        //                                LvPLReq.LVCount = LvPLReq.DebitDays;
        //                                LvPLReq.LvOccurances = 1;
        //                                LvPLReq.LeaveHead = PrevOpenBal.LvHead;
        //                            }


        //                            LvPLReq.CloseBal = LvPLReq.OpenBal - LvPLReq.DebitDays;
        //                            LvPLReq.GeoStruct = OEmployee.GeoStruct;
        //                            LvPLReq.PayStruct = OEmployee.PayStruct;
        //                            LvPLReq.FuncStruct = OEmployee.FuncStruct;

        //                            if (LvPLReq.CloseBal < 0 || LvPLReq.OpenBal < LvPLReq.DebitDays)
        //                            {
        //                                Msg.Add("  SL combined with PL Can't be utilised as your PL balcance is low.  ");
        //                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                            }

        //                            LvNewReq LvNewReqNew = new LvNewReq()
        //                            {
        //                                ContactNo = LvReq.ContactNo,
        //                                DebitDays = LvPLReq.DebitDays,
        //                                FromDate = LvReq.ToDate.Value.AddDays(1),
        //                                FromStat = LvReq.FromStat,
        //                                LeaveHead = LvPLReq.LeaveHead,
        //                                Reason = "On leave finished. Leave Occurances not changed.",
        //                                ReqDate = LvReq.ReqDate,
        //                                ResumeDate = OldToDate.Value.AddDays(1),
        //                                ToDate = OldToDate,
        //                                ToStat = LvReq.ToStat,
        //                                LeaveCalendar = LvReq.LeaveCalendar,
        //                                DBTrack = LvReq.DBTrack,
        //                                InputMethod = 1,//apply through ess
        //                                CloseBal = LvPLReq.CloseBal,
        //                                OpenBal = LvPLReq.OpenBal,
        //                                LVCount = LvPLReq.LVCount,
        //                                LvOccurances = LvPLReq.LvOccurances,
        //                                PrefixCount = LvReq.PrefixCount,
        //                                SufixCount = LvReq.SufixCount,
        //                                Incharge = LvReq.Incharge,
        //                                TrClosed = false,
        //                                Narration = "Leave Requisition",
        //                                WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
        //                                PrefixSuffix = LvPLReq.PrefixSuffix,
        //                                GeoStruct = LvPLReq.GeoStruct,
        //                                PayStruct = LvPLReq.PayStruct,
        //                                FuncStruct = LvPLReq.FuncStruct,
        //                                LvWFDetails = oLvWFDetails_List1
        //                            };
        //                            OFAT.Add(LvNewReqNew);
        //                        }


        //                    }


        //                    OFAT.Add(LvNewReq);
        //                    db.SaveChanges();
        //                    if (OEmployeeLeave == null)
        //                    {
        //                        EmployeeLeave OTEP = new EmployeeLeave()
        //                        {
        //                            Employee = db.Employee.Find(OEmployee.Id),
        //                            LvNewReq = OFAT,
        //                            DBTrack = LvReq.DBTrack
        //                        };
        //                        db.EmployeeLeave.Add(OTEP);
        //                        db.SaveChanges();
        //                    }
        //                    else
        //                    {
        //                        var aa = db.EmployeeLeave.Find(OEmployeeLeave.Id);
        //                        OFAT.AddRange(aa.LvNewReq);
        //                        aa.LvNewReq = OFAT;
        //                        db.EmployeeLeave.Attach(aa);
        //                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

        //                    }
        //                    ts.Complete();
        //                    return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

        //                }
        //                catch (DataException ex)
        //                {
        //                    LogFile Logfile = new LogFile();
        //                    ErrorLog Err = new ErrorLog()
        //                    {
        //                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                        ExceptionMessage = ex.Message,
        //                        ExceptionStackTrace = ex.StackTrace,
        //                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                        LogTime = DateTime.Now
        //                    };
        //                    Logfile.CreateLogFile(Err);
        //                    return Json(new { status = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            StringBuilder sb = new StringBuilder("");
        //            foreach (ModelState modelState in ModelState.Values)
        //            {
        //                foreach (ModelError error in modelState.Errors)
        //                {
        //                    sb.Append(error.ErrorMessage);
        //                    sb.Append("." + "\n");
        //                }
        //            }
        //            var errorMsg = sb.ToString();
        //            return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
        //            //return this.Json(new { msg = errorMsg });
        //        }
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

            public int? Lv_Req_Id { get; set; }

        }


        public ActionResult Create(LvNewReq LvReq, FormCollection form, String forwarddata, string DebitDays)
        {

            string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
            string ContactNosList = form["ContactNos_List"] == "0" ? "" : form["ContactNos_List"];
            string Emp = form["EmpLvnereq_Id"] == "0" ? "" : form["EmpLvnereq_Id"];
            string FrmStat = form["FromStatlist"] == "0" ? "" : form["FromStatlist"];
            string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
            DebitDays = form["DebitDays"] == "0" ? "" : form["DebitDays"];
            //   string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];
            string ddlIncharge = form["InchargeList"] == "" ? null : form["InchargeList"];
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {

                int EmpId = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    EmpId = int.Parse(Emp);
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                }
                //var calendar = form["LvCalendarlist"] == "0" ? "" : form["LvCalendarlist"];
                //if (calendar != null && calendar != "")
                //{
                //    var value = db.Calendar.Find(int.Parse(calendar));
                //    LvReq.LeaveCalendar = value;
                //}
                if (ddlIncharge != null && ddlIncharge != "-Select-")
                {
                    var value = db.Employee.Find(int.Parse(ddlIncharge));
                    LvReq.Incharge = value;

                }
                if (FrmStat != null && FrmStat != "")
                {
                    var value = db.LookupValue.Find(int.Parse(FrmStat));
                    LvReq.FromStat = value;
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please Select Leave FromState" }, JsonRequestBehavior.AllowGet);
                }
                if (Tostat != null && Tostat != "")
                {
                    var value = db.LookupValue.Find(int.Parse(Tostat));
                    LvReq.ToStat = value;
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please Select Leave Tostate " }, JsonRequestBehavior.AllowGet);
                }

                if (LvHeadList != null && LvHeadList != "")
                {
                    var val = db.LvHead.Find(int.Parse(LvHeadList));
                    LvReq.LeaveHead = val;
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Please Select Leave Head" }, JsonRequestBehavior.AllowGet);
                }

                if (ContactNosList != null && ContactNosList != "")
                {
                    int ContactNoId = Convert.ToInt32(ContactNosList);
                    var val = db.ContactNumbers.Where(e => e.Id == ContactNoId).SingleOrDefault();
                    LvReq.ContactNo = val;
                }

                var Comp_Id = 0;
                Comp_Id = Convert.ToInt32(Session["CompId"]);
                var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                Employee OEmployee = null;
                EmployeeLeave OEmployeeLeave = null;
                Company OCompany = null;
                OCompany = db.Company.Find(Comp_Id);
                LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                /* Added By Rekha 04-03-2017*/
                LvReq.LeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                int Empid = int.Parse(Emp);
                //var PrevReq = db.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id).OrderByDescending(e => e.Id).FirstOrDefault();
                OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(y => y.LeaveCalendar))
                    //  .Include(e => e.LvOpenBal)
                    .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                     .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                    //.Include(e => e.LvOpenBal.Select(r => r.LvHead))
                    //.Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                .Where(e => e.Employee.Id == Empid).SingleOrDefault();

                var LvOrignal_id = OEmployeeLeave.LvNewReq.Where(e => e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();
                var AntCancel = OEmployeeLeave.LvNewReq.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3").OrderBy(e => e.Id).ToList();

                if (listLvs.Where(e => e.FromDate >= LvReq.FromDate && e.FromDate <= LvReq.ToDate).Count() != 0 ||
                      listLvs.Where(e => e.ToDate >= LvReq.FromDate && e.ToDate <= LvReq.ToDate).Count() != 0)
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Requisition Already Exists." }, JsonRequestBehavior.AllowGet);
                }
                if (listLvs.Where(e => e.FromDate <= LvReq.FromDate && e.ToDate >= LvReq.ToDate).Count() != 0)
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "Requisition Already Exists." }, JsonRequestBehavior.AllowGet);
                }
                //if (listLvs.Where(e => e.IsCancel == false || e.TrReject == false).Any(o => o.FromDate == LvReq.FromDate))
                //{
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = "Requisition Already Exists." }, JsonRequestBehavior.AllowGet); 
                //}

                //var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
                //var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvReq.LeaveHead.Id)
                //  .OrderByDescending(e => e.Id).FirstOrDefault();
                //if (PrevReq != null)
                //{
                //    LvReq.OpenBal = PrevReq.CloseBal;
                //    LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
                //    LvReq.LvOccurances = PrevReq.LvOccurances + 1;
                //}
                //else
                //{
                //    var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvReq.LeaveHead.Id && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
                //    LvReq.OpenBal = PrevOpenBal.LvOpening;
                //    LvReq.LVCount = LvReq.DebitDays;
                //    LvReq.LvOccurances = 1;
                //}

                OEmployee = db.Employee
                              .Include(e => e.GeoStruct)
                              .Include(e => e.GeoStruct.Location)
                              .Include(e => e.FuncStruct)
                              .Include(e => e.PayStruct).AsNoTracking()
                              .Where(r => r.Id == Empid).AsParallel()
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


                //// Check leave head policy as calendar,joining,increment End


                ////var retD = LeaveHeadProcess.LeaveValidation(LvReq, Comp_Id, OEmployeeLeave.Id, LvReq.LeaveCalendar, Leaveyearfrom, LeaveyearTo);
                ////LvReq.DebitDays = retD.DebitDays;
                ////LvReq.PrefixCount = retD.LvnewReqprefix;
                ////LvReq.SufixCount = retD.LvnewReqSuffix;
                ////LvReq.PrefixSuffix = retD.PrefixSufix;
                //LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
                //LvReq.InputMethod = 1;  //apply through eeis source
                //DateTime? OldToDate = LvReq.ToDate;
                //double OldDebDays = LvReq.DebitDays;


                //LvWFDetails oLvWFDetails = new LvWFDetails
                //{
                //    WFStatus = 0,
                //    Comments = LvReq.Reason,
                //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                //};

                //List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                //oLvWFDetails_List.Add(oLvWFDetails);

                //if (OCompany.Code.ToString() == "KDCC" && LvReq.LeaveHead.LvCode.ToUpper() == "SL")
                //{
                //    //LvReq.CloseBal = LvReq.OpenBal - LvReq.DebitDays;
                //    if (LvReq.CloseBal <= 0)
                //    {
                //        LvReq.CloseBal = 0;
                //        LvReq.DebitDays = LvReq.OpenBal;
                //        if (PrevReq != null)
                //        {
                //            LvReq.LVCount = PrevReq.LVCount + LvReq.DebitDays;
                //        }
                //        else
                //        {
                //            LvReq.LVCount = LvReq.DebitDays;

                //        }
                //        //LvReq.LVCount = LvReq.DebitDays;
                //        LvReq.ToDate = LvReq.FromDate.Value.AddDays(LvReq.DebitDays - 1);
                //    }
                //}

                if (Convert.ToBoolean(Session["IsCertAppl"]) == true && Session["FilePath"] == null)
                {
                    Msg.Add("Kindly upload certificate for this leave..!");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                if (Session["FilePath"] != null)
                {
                    LvReq.Path = Session["FilePath"].ToString();
                }

                // upload certificate if leave taken after no of times ex. if 3 time take SL then not require after that require certificate start
           //     var leavecalendarid = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
           //     EmployeeLeave OEmployeeLeavechk = null;
           //     OEmployeeLeavechk = db.EmployeeLeave.Include(e => e.LvNewReq)
           //    .Include(e => e.LvNewReq.Select(y => y.LeaveCalendar))
           //    .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
           //     .Include(e => e.LvNewReq.Select(a => a.WFStatus))
           //.Where(e => e.Employee.Id == Empid).SingleOrDefault();
           //     var LvOrignal_idchk = OEmployeeLeavechk.LvNewReq.Where(e => e.LvOrignal != null && e.LeaveHead_Id == LvReq.LeaveHead.Id).Select(e => e.LvOrignal.Id).ToList();
           //     var AntCancelchk = OEmployeeLeavechk.LvNewReq.Where(e => e.IsCancel == false && e.TrReject == false && e.LeaveHead_Id == LvReq.LeaveHead.Id && e.LeaveCalendar_Id == leavecalendarid.Id).OrderBy(e => e.Id).ToList();
           //     var listLvschk = AntCancelchk.Where(e => !LvOrignal_idchk.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.Path=="").OrderBy(e => e.Id).ToList();


           //     string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
           //     bool exists = System.IO.Directory.Exists(requiredPath);
           //     string localPath;
           //     if (!exists)
           //     {
           //         localPath = new Uri(requiredPath).LocalPath;
           //         System.IO.Directory.CreateDirectory(localPath);
           //     }
           //     string path = requiredPath + @"\Leavecertificateonreqcount" + ".ini";
           //     localPath = new Uri(path).LocalPath;
           //     if (!System.IO.File.Exists(localPath))
           //     {

           //         using (var fs = new FileStream(localPath, FileMode.OpenOrCreate))
           //         {
           //             StreamWriter str = new StreamWriter(fs);
           //             str.BaseStream.Seek(0, SeekOrigin.Begin);

           //             str.Flush();
           //             str.Close();
           //             fs.Close();
           //         }


           //     }

           //     string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
           //     bool existschk = System.IO.Directory.Exists(requiredPathchk);
           //     string localPathchk;
           //     if (!existschk)
           //     {
           //         localPath = new Uri(requiredPathchk).LocalPath;
           //         System.IO.Directory.CreateDirectory(localPath);
           //     }
           //     string pathchk = requiredPathchk + @"\Leavecertificateonreqcount" + ".ini";
           //     localPathchk = new Uri(pathchk).LocalPath;
           //     string Leave_code = "";
           //     int Lvreqcnt = 0;
           //     int paramcnt = 0;
           //     using (var streamReader = new StreamReader(localPathchk))
           //     {
           //         string line;

           //         while ((line = streamReader.ReadLine()) != null)
           //         {
           //             var LVCode = line.Split('_')[0];
           //             var Reqparamcnt = line.Split('_')[1];
           //             if (LVCode != "")
           //             {
           //                 var val = db.LvHead.Find(int.Parse(LvHeadList));
           //                 if (val.LvCode == LVCode)
           //                 {
           //                     Leave_code = val.LvCode;
           //                     if (listLvschk != null)
           //                     {
           //                         Lvreqcnt = listLvschk.Count();
           //                     }

           //                     paramcnt = Convert.ToInt32(Reqparamcnt);
           //                     break;
           //                 }
           //             }
           //         }
           //     }
           //     if (Leave_code != "")
           //     {
           //         if (LvReq.LeaveHead.Id != 0)
           //         {
           //             LvDebitPolicy ODebitPolicy = db.LvDebitPolicy.Where(e => e.LvHead_Id == LvReq.LeaveHead.Id).FirstOrDefault();

           //             if (ODebitPolicy != null)
           //             {
           //                 if (ODebitPolicy.IsCertificateAppl == true)
           //                 {
           //                     if (Lvreqcnt + 1 > paramcnt)
           //                     {
           //                         if ((Lvreqcnt + 1 > paramcnt) && LvReq.Path == null)//Lvreqcnt =already avail leave req count and 1=new leave req
           //                         {
           //                             Msg.Add("Kindly upload certificate for this leave..!");
           //                             return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
           //                         }
           //                     }
           //                 }
           //                 else
           //                 {
           //                     if (LvReq.Path != null)
           //                     {
           //                         Msg.Add("You can't upload certificate for this leave..!");
           //                         return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
           //                     }
           //                 }

           //             }
           //         }

           //     }
           //     // upload certificate if leave taken after no of times ex. if 3 time take SL then not require after that require certificate end

           //     if (LvReq.LeaveHead.Id != 0)
           //     {
           //         LvDebitPolicy ODebitPolicy = db.LvDebitPolicy.Where(e => e.LvHead_Id == LvReq.LeaveHead.Id).FirstOrDefault();

           //         if (ODebitPolicy != null && Leave_code == "")
           //         {
           //             if (ODebitPolicy.IsCertificateAppl == true)
           //             {
           //                 //if ((ODebitPolicy.MinLvDays >= LvReq.DebitDays) && LvReq.Path != null)
           //                 //{
           //                 //    Msg.Add("You can't upload cerificate for this leave..!");
           //                 //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
           //                 //}
           //                 if ((ODebitPolicy.MinLvDays <= LvReq.DebitDays) && LvReq.Path == null)
           //                 {
           //                     Msg.Add("Kindly upload cerificate for this leave..!");
           //                     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
           //                 }
           //             }
           //             else
           //             {
           //                 if (LvReq.Path != null)
           //                 {
           //                     Msg.Add("You can't upload cerificate for this leave..!");
           //                     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
           //                 }
           //             }

           //         }
           //     }

                //LvNewReq LvNewReq = new LvNewReq()
                //{
                //    ContactNo = LvReq.ContactNo,
                //    DebitDays = LvReq.DebitDays,
                //    FromDate = LvReq.FromDate,
                //    FromStat = LvReq.FromStat,
                //    LeaveHead = LvReq.LeaveHead,
                //    Reason = LvReq.Reason,
                //    ReqDate = LvReq.ReqDate,
                //    InputMethod = LvReq.InputMethod,
                //    ResumeDate = LvReq.ResumeDate,
                //    ToDate = LvReq.ToDate,
                //    ToStat = LvReq.ToStat,
                //    LeaveCalendar = LvReq.LeaveCalendar,
                //    DBTrack = LvReq.DBTrack,
                //    CloseBal = LvReq.CloseBal,
                //    OpenBal = LvReq.OpenBal,
                //    LVCount = LvReq.LVCount,
                //    //main
                //    LvOccurances = LvReq.LvOccurances,
                //    PrefixCount = LvReq.PrefixCount,
                //    SufixCount = LvReq.SufixCount,
                //    Incharge = LvReq.Incharge,
                //    TrClosed = false,
                //    Narration = "Leave Requisition",
                //    PrefixSuffix = LvReq.PrefixSuffix,
                //    WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                //    LvWFDetails = oLvWFDetails_List,
                //    Path = LvReq.Path
                //};
                //List<LvNewReq> OFAT = new List<LvNewReq>();
                if (ModelState.IsValid)
                {

                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                .Where(r => r.Id == EmpId).SingleOrDefault();
                    //using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                    //           new System.TimeSpan(0, 30, 0)))
                    //{
                    try
                    {
                        //LvNewReq.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                        //LvNewReq.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);
                        //LvNewReq.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                        //db.LvNewReq.Add(LvNewReq);
                        //if (Z.Company.Code.ToString() == "KDCC" && LvReq.LeaveHead.LvCode.ToUpper() == "SL")
                        //{
                        //    if (LvReq.CloseBal == 0 && OldDebDays > LvReq.DebitDays)
                        //    {
                        //        LvWFDetails oLvWFDetails1 = new LvWFDetails
                        //        {
                        //            WFStatus = 0,
                        //            Comments = LvReq.Reason,
                        //            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        //        };
                        //        List<LvWFDetails> oLvWFDetails_List1 = new List<LvWFDetails>();
                        //        oLvWFDetails_List1.Add(oLvWFDetails1);
                        //        OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvOpenBal)
                        //       .Include(e => e.LvOpenBal.Select(r => r.LvHead)).Include(e => e.LvOpenBal.Select(r => r.LvCalendar))
                        //       .Include(e => e.LvNewReq.Select(r => r.LeaveHead)).Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                        //       .Where(e => e.Employee.Id == Empid).SingleOrDefault();

                        //       PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.LvCode == "PL" && e.LeaveCalendar.Id == LvReq.LeaveCalendar.Id)
                        //            .OrderByDescending(e => e.Id).FirstOrDefault();

                        //        LvNewReq LvPLReq = new LvNewReq();

                        //        LvPLReq.DebitDays = OldDebDays - LvReq.DebitDays;

                        //        if (PrevReq != null)
                        //        {
                        //            LvPLReq.OpenBal = PrevReq.CloseBal;
                        //            LvPLReq.LVCount = PrevReq.LVCount + LvPLReq.DebitDays;
                        //            LvPLReq.LvOccurances = PrevReq.LvOccurances;
                        //            LvPLReq.LeaveHead = PrevReq.LeaveHead;
                        //        }
                        //        else
                        //        {
                        //            var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.LvCode.ToUpper() == "PL" && e.LvCalendar.Id == LvReq.LeaveCalendar.Id).SingleOrDefault();
                        //            LvPLReq.OpenBal = PrevOpenBal.LvOpening;
                        //            LvPLReq.LVCount = LvPLReq.DebitDays;
                        //            LvPLReq.LvOccurances = 1;
                        //            LvPLReq.LeaveHead = PrevOpenBal.LvHead;
                        //        }


                        //        LvPLReq.CloseBal = LvPLReq.OpenBal - LvPLReq.DebitDays;
                        //        LvPLReq.GeoStruct = OEmployee.GeoStruct;
                        //        LvPLReq.PayStruct = OEmployee.PayStruct;
                        //        LvPLReq.FuncStruct = OEmployee.FuncStruct;

                        //        if (LvPLReq.CloseBal < 0 || LvPLReq.OpenBal < LvPLReq.DebitDays)
                        //        {
                        //            Msg.Add("  SL combined with PL Can't be utilised as your PL balcance is low.  ");
                        //            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //        }

                        //        LvNewReq LvNewReqNew = new LvNewReq()
                        //        {
                        //            ContactNo = LvReq.ContactNo,
                        //            DebitDays = LvPLReq.DebitDays,
                        //            FromDate = LvReq.ToDate.Value.AddDays(1),
                        //            FromStat = LvReq.FromStat,
                        //            LeaveHead = LvPLReq.LeaveHead,
                        //            Reason = "On leave finished. Leave Occurances not changed.",
                        //            ReqDate = LvReq.ReqDate,
                        //            ResumeDate = OldToDate.Value.AddDays(1),
                        //            ToDate = OldToDate,
                        //            ToStat = LvReq.ToStat,
                        //            LeaveCalendar = LvReq.LeaveCalendar,
                        //            DBTrack = LvReq.DBTrack,
                        //            InputMethod = 1,//apply through ess
                        //            CloseBal = LvPLReq.CloseBal,
                        //            OpenBal = LvPLReq.OpenBal,
                        //            LVCount = LvPLReq.LVCount,
                        //            LvOccurances = LvPLReq.LvOccurances,
                        //            PrefixCount = LvReq.PrefixCount,
                        //            SufixCount = LvReq.SufixCount,
                        //            Incharge = LvReq.Incharge,
                        //            TrClosed = false,
                        //            Narration = "Leave Requisition",
                        //            WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                        //            PrefixSuffix = LvPLReq.PrefixSuffix,
                        //            GeoStruct = LvPLReq.GeoStruct,
                        //            PayStruct = LvPLReq.PayStruct,
                        //            FuncStruct = LvPLReq.FuncStruct,
                        //            LvWFDetails = oLvWFDetails_List1
                        //        };
                        //        OFAT.Add(LvNewReqNew);
                        //    }


                        //}


                        //OFAT.Add(LvNewReq);
                        //db.SaveChanges();
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
                        //    db.EmployeeLeave.Attach(aa);
                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                        //    db.SaveChanges();
                        //    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                        //}
                        //     ts.Complete();

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

                        ReturnData_LeaveValidation returnDATA = new ReturnData_LeaveValidation();

                        var ShowMessageCode = "";
                        var ShowMessage = "";
                        int CalenderID = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).Select(e => e.Id).FirstOrDefault();
                        int bossid = Empid;
                        string bossempcode = "";
                        var bosscode = db.Employee.Where(e => e.Id == bossid).SingleOrDefault();
                        if (bosscode != null)
                        {
                            bossempcode = bosscode.EmpCode;
                        }
                        ServiceResult<ReturnData_LeaveValidation> responseDeserializeData = new ServiceResult<ReturnData_LeaveValidation>();
                        string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                        using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                        {
                            var response = p2BHttpClient.request("ELMS/getUserLvRequest",
                                new ELMS_Lv_NewRequest()
                                {
                                    Emp_Code=bossempcode,
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
                                    Incharge_Id = Convert.ToInt32(ddlIncharge),
                                    LeaveCalendar_Id = CalenderID,
                                    InputMethod = 1,
                                    DebitDays = LvReq.DebitDays,
                                    IsDebitSharing = LvReq.IsDebitSharing,
                                    PrefixCount = LvReq.PrefixCount,
                                    SufixCount = LvReq.SufixCount,
                                    PrefixSuffix = LvReq.PrefixSuffix,
                                    LvCountPrefixSuffix = LvReq.LvCountPrefixSuffix,
                                    LvWFStatus = 0,
                                    Comments = "Applied"

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

                        //if (responseDeserializeData == null && ShowMessageCode == "OK")
                        //{
                        //    Msg.Add(ShowMessage);
                        //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}
                        //else
                        //{
                        //    Msg.Add(ShowMessage);
                        //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}

                        return Json(new { status = true, responseText = ShowMessage }, JsonRequestBehavior.AllowGet);

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
                        return Json(new { status = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                    // }
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = errorMsg }, JsonRequestBehavior.AllowGet);
                    //  return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                    //return this.Json(new { msg = errorMsg });
                }
            }
        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvNewReq
                    .Include(e => e.FromStat)
                    .Include(e => e.ToStat)
                       .Include(e => e.ContactNo)
                    .Include(e => e.LeaveHead)
                    .Where(e => e.Id == data).AsEnumerable()
                    .Select(e => new
                    {
                        ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                        FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
                        ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
                        DebitDays = e.DebitDays,
                        Reason = e.Reason,
                        FromStat_Id = e.FromStat == null ? 0 : e.FromStat.Id,
                        ToStat_Id = e.ToStat == null ? 0 : e.ToStat.Id,
                        Lvcalendar_Id = e.LeaveCalendar == null ? 0 : e.LeaveCalendar.Id,
                        Lvcalendar_Fulldetails = e.LeaveCalendar == null ? null : e.LeaveCalendar.FullDetails,
                        ContactNo_FullAddress = e.ContactNo == null ? null : e.ContactNo.FullContactNumbers,
                        ContactNo_Id = e.ContactNo == null ? null : e.ContactNo.Id.ToString(),
                        LvHead_Id = e.LeaveHead == null ? null : e.LeaveHead.Id.ToString(),
                        LvHead_FullDetails = e.LeaveHead == null ? null : e.LeaveHead.FullDetails,
                        Action = e.DBTrack.Action
                    }).SingleOrDefault();
                return Json(Q, JsonRequestBehavior.AllowGet);
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

                    string Emp = form["EmpLvnereq_Id"] == "0" ? "" : form["EmpLvnereq_Id"];
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

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
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
            //int id = int.Parse(data);
            using (DataBaseContext db = new DataBaseContext())
            {
                var a = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").ToList();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }
        public class EmpmLVdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string lvcalendar { get; set; }
            public double lvClosing { get; set; }
            public double lvOpening { get; set; }
            public double Lvoccurance { get; set; }
            public string FromStat { get; set; }
            public string ToStat { get; set; }
            public string Resume_Date { get; set; }
            public string Req_Date { get; set; }
            public string Branch { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public string FromDate { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string RecomendationCode { get; set; }
            public string RecomendationEmpname { get; set; }
            public string Todate { get; set; }
            public double Debit_Days { get; set; }
            public string EmpContactNO { get; set; }
            public string Reason { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public bool TrClosed { get; set; }
            public string EmployeeName { get; set; }
            public string filepath { get; set; }
            public string Empcode { get; set; }

            public int EmployeeId { get; set; }
            public string Incharge { get; set; }
        }

        public class EmpmLVEncashdata
        {

            public string Status { get; set; }
            public string lvhead { get; set; }
            public string lvcalendar { get; set; }
            public string lvFromDate { get; set; }
            public string lvTodate { get; set; }
            public string EncashDays { get; set; }
            public string Narration { get; set; }
            public string Reason { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public bool TrClosed { get; set; }
            public string EmployeeName { get; set; }

            public int EmployeeId { get; set; }
        }

        public ActionResult GetEmpLvData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                var RecomendationStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                    RecomendationStatus.Add(7);
                    RecomendationStatus.Add(8);
                }
                else if (authority.ToUpper() == "RECOMMENDATION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                var LvHeadId = ids.Count > 0 ? ids[3] : null;

                var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeeLeave
                    .Include(e => e.LvNewReq)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.GeoStruct)
                    .Include(e => e.Employee.FuncStruct)
                    .Include(e => e.Employee.FuncStruct.Job)
                     .Include(e => e.Employee.GeoStruct.Location)
                     .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                    .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                    .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                    .Include(e => e.LvNewReq.Select(t => t.WFStatus))
                    .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar.Name))
                    .Include(e => e.LvNewReq.Select(t => t.FromStat))
                    .Include(e => e.LvNewReq.Select(t => t.ToStat))
                    .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                    .Include(e => e.LvNewReq.Select(t => t.LvWFDetails))
                    .Include(e => e.LvNewReq.Select(t => t.Incharge))
                     .Include(e => e.LvNewReq.Select(t => t.Incharge.EmpName))
                    .Where(e => e.Id == EmpLvIdint && e.LvNewReq.Any(w => w.Id == id)).SingleOrDefault();

                var v = W.LvNewReq.Where(e => e.Id == id).Select(s => new EmpmLVdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Lvnewreq = s.Id,
                    filepath = s.Path,
                    Empcode = W.Employee.EmpCode,
                    //   Status = s.WFStatus.LookupVal.ToString(),
                    Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                    Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                    Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                    Status = status,
                    Emplvhead = s.LeaveHead != null ? s.LeaveHead.FullDetails : null,
                    lvcalendar = s.LeaveCalendar != null ? s.LeaveCalendar.FullDetails : null,
                    lvClosing = s.CloseBal,
                    lvOpening = s.OpenBal,
                    Lvoccurance = s.LvOccurances,
                    FromStat = s.FromStat != null ? s.FromStat.LookupVal.ToUpper().ToString() : null,
                    ToStat = s.ToStat != null ? s.ToStat.LookupVal.ToUpper().ToString() : null,
                    Resume_Date = s.ResumeDate != null ? s.ResumeDate.Value.ToShortDateString() : null,
                    Req_Date = s.ReqDate != null ? s.ReqDate.Value.ToShortDateString() : null,
                    FromDate = s.FromDate != null ? s.FromDate.Value.ToShortDateString() : null,
                    Todate = s.ToDate != null ? s.ToDate.Value.ToShortDateString() : null,
                    Debit_Days = s.DebitDays,
                    EmpContactNO = s.ContactNo != null ? s.ContactNo.FullContactNumbers : null,
                    Reason = s.Reason,
                    Id = s.Id,
                    TrClosed = s.TrClosed,
                    SanctionCode = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    SanctionComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    ApporavalComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Wf = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
                    RecomendationCode = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    RecomendationEmpname = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
                }).SingleOrDefault();


                var EmpCheck = db.EmployeeLeave.Include(e => e.LvNewReq)
                                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                .Where(e => e.Id == EmpLvIdint && e.LvNewReq.Any(w => w.LeaveHead.Id == lvheadidint)).SingleOrDefault();
                if (v.SanctionCode != null)
                {
                    int sanctionid = Convert.ToInt32(v.SanctionCode);
                    var sanctioncode = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == sanctionid).SingleOrDefault();
                    if (sanctioncode != null)
                    {
                        v.SanctionCode = sanctioncode.Employee.EmpCode;
                        v.SanctionEmpname = sanctioncode.Employee.EmpName.FullNameFML;
                    }
                }
                if (v.RecomendationCode != null)
                {
                    int Recomendationid = Convert.ToInt32(v.RecomendationCode);
                    var Recomendationode = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == Recomendationid).SingleOrDefault();
                    if (Recomendationode != null)
                    {
                        v.RecomendationCode = Recomendationode.Employee.EmpCode;
                        v.RecomendationEmpname = Recomendationode.Employee.EmpName.FullNameFML;
                    }
                }
                //if Emp Bal updated
                var listOfObject = new List<dynamic>();
                var EmpCheck_Id = EmpCheck.LvNewReq.Where(e => e.LeaveHead.Id == lvheadidint && e.Id == id).Select(e => e.Id).LastOrDefault();
                if (v.Id != EmpCheck_Id)
                {

                    var a = EmpCheck.LvNewReq.Where(e => e.LeaveHead.Id == lvheadidint).LastOrDefault();
                    v.lvOpening = a.OpenBal;
                    v.lvClosing = a.CloseBal;

                    listOfObject.Add(v);
                    return Json(listOfObject, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    listOfObject.Add(v);
                    return Json(listOfObject, JsonRequestBehavior.AllowGet);
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
        public ActionResult GetLookupIncharge(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var empinchargeloc = db.Employee
                    .Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Location)
                    .Include(e => e.GeoStruct.Location.Incharge)
                    .Where(e => e.EmpCode == data).FirstOrDefault();
                int inchid = 0;
                if (empinchargeloc != null)
                {
                    if (empinchargeloc.GeoStruct.Location.Incharge != null)
                    {
                        inchid = empinchargeloc.GeoStruct.Location.Incharge.Id;// if dep inchagre on leave then that location incharge should be incharge of that dep.
                        //other loc,dep,division incharge not come in list. as suggest sir
                    }

                }
                var exceploc = db.Location.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDep = db.Department.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var excepDivision = db.Division.Where(e => e.Incharge.Id != null && e.Incharge.Id != inchid).Select(e => e.Incharge.Id).ToList();
                var exceptot = exceploc.Union(excepDep).Union(excepDivision).ToList();

                var fall = db.Employee
                    .Include(e => e.EmpName)
                    .Include(e => e.ServiceBookDates)
                    .Where(e => e.ServiceBookDates.ServiceLastDate == null && !exceptot.Contains(e.Id)).ToList();
                IEnumerable<Employee> all;
                if (!string.IsNullOrEmpty(data))
                {
                    // all = db.Employee.ToList().Where(d => d.EmpCode.Contains(data));
                    all = fall;

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { srno = c.Id, lookupvalue = c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetEmpLvEncashData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //   var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeeLeave
                    .Include(e => e.LeaveEncashReq)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.LeaveEncashReq.Select(t => t.LeaveCalendar))
                    .Include(e => e.LeaveEncashReq.Select(t => t.LeaveCalendar.Name))
                    .Include(e => e.LeaveEncashReq.Select(t => t.WFStatus))
                    .Include(e => e.LeaveEncashReq.Select(t => t.LvHead))
                    .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq))
                    //.Include(e => e.LeaveEncashReq.Select(t => t.LeaveHead))
                    .Include(e => e.LeaveEncashReq.Select(t => t.LvWFDetails))
                    .Where(e => e.Employee.Id == EmpLvIdint && e.LeaveEncashReq.Any(w => w.Id == id)).SingleOrDefault();

                var v = W.LeaveEncashReq.Where(e => e.Id == id).Select(s => new EmpmLVEncashdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    //   Status = s.WFStatus.LookupVal.ToString(),
                    Status = status,
                    lvhead = s.LvHead != null ? s.LvHead.FullDetails : null,
                    lvcalendar = s.LeaveCalendar != null ? s.LeaveCalendar.FullDetails : null,
                    lvFromDate = s.FromPeriod != null ? s.FromPeriod.Value.ToShortDateString() : null,
                    lvTodate = s.ToPeriod != null ? s.ToPeriod.Value.ToShortDateString() : null,
                    EncashDays = s.EncashDays.ToString(),
                    Narration = s.Narration,
                    Id = s.Id,
                    TrClosed = s.TrClosed,
                    SanctionComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).SingleOrDefault() : null,
                    ApporavalComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).SingleOrDefault() : null,
                    Wf = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null
                }).SingleOrDefault();


                var listOfObject = new List<dynamic>();
                listOfObject.Add(v);
                return Json(listOfObject, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateStatus(LvNewReq LvReq, FormCollection form, String data)
        {

            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var lvnewreqid = Convert.ToInt32(ids[0]);
            var EmpLvId = Convert.ToInt32(ids[1]);
            string Sanction = form["Sanction"];
            string ReasonSanction = form["ReasonSanction"];
            string HR = form["HR"] == null ? null : form["HR"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            string Approval = form["Approval"];
            string ReasonApproval = form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            string Recomendation = form["Recomendation"];
            string ReasonRecomendation = form["ReasonRecomendation"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            string SanInchargeid = form["SanIncharge_id"];
            string RecInchargeid = form["RecIncharge_id"];
            string AppInchargeid = form["AppIncharge_id"];

            List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();

            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                //  access right no of levaefrom days and to days check start
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();


                }

                var qurey = db.LvNewReq
                  .Include(e => e.WFStatus)
                  .Include(e => e.LeaveCalendar)
                  .Include(e => e.LeaveHead)
                  .Include(e => e.LvWFDetails)
                  .Include(e => e.FromStat)
                  .Include(e => e.GeoStruct)
                  .Include(e => e.LvOrignal)
                  .Include(e => e.PayStruct)
                  .Include(e => e.FuncStruct)
                  .Where(e => e.Id == lvnewreqid).SingleOrDefault();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                double LvNoOfDaysFrom = 0;
                double LvNoOfDaysTo = 0;
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        if (item1.SubModuleName != null && item1.SubModuleName != "")
                        {
                            if (item1.AccessRightsLookup.ToString() == AccessRight && item1.ModuleName.ToUpper() == FuncModule.LookupVal.ToUpper() && item1.SubModuleName == qurey.LeaveHead.LvCode && qurey.DebitDays > item1.LvNoOfDaysFrom && qurey.DebitDays <= item1.LvNoOfDaysTo)
                            {
                                LvNoOfDaysFrom = item1.LvNoOfDaysFrom;
                                LvNoOfDaysTo = item1.LvNoOfDaysTo;
                                break;
                            }

                        }
                        else
                        {

                            if (item1.AccessRightsLookup.ToString() == AccessRight && item1.ModuleName.ToUpper() == FuncModule.LookupVal.ToUpper())
                            {
                                LvNoOfDaysFrom = item1.LvNoOfDaysFrom;
                                LvNoOfDaysTo = item1.LvNoOfDaysTo;

                            }
                        }

                    }
                }
                if (LvNoOfDaysFrom == 0 && LvNoOfDaysTo == 0)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = "Please Assign in Acess Right Leave No Of Days From and Leave No Of Days To " }, JsonRequestBehavior.AllowGet);
                }

                //  access right no of levaefrom days and to days check end






                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                if (authority.ToUpper() == "MYSELF")
                {
                    qurey.Reason = ReasonMySelf;
                    qurey.IsCancel = true;
                    qurey.TrClosed = true;
                    qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                }
                if (authority.ToUpper() == "SANCTION")
                {

                    if (Sanction == null)
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status" }, JsonRequestBehavior.AllowGet);
                    }
                    if (ReasonSanction == "")
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                    }
                    if (Convert.ToBoolean(Sanction) == true)
                    {
                        //sanction yes -1
                        var CheckAllreadySanction = qurey.LvWFDetails.Where(e => e.WFStatus == 1).ToList();
                        if (CheckAllreadySanction.Count() > 0)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Sanction....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                        }
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 1,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.LvWFDetails.Add(LvWFDetails);
                        //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        if (qurey.DebitDays > LvNoOfDaysFrom && qurey.DebitDays <= LvNoOfDaysTo)
                        {
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            if (SanInchargeid != null && SanInchargeid != "" && SanInchargeid != "-Select-")
                            {
                                var value = db.Employee.Find(int.Parse(SanInchargeid));
                                qurey.Incharge = value;

                            }

                        }
                    }
                    else if (Convert.ToBoolean(Sanction) == false)
                    {
                        //sanction no -2
                        //var LvWFDetails = new LvWFDetails
                        //{
                        //    WFStatus = 2,
                        //    Comments = ReasonSanction,
                        //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        //};
                        //qurey.LvWFDetails.Add(LvWFDetails);
                        var CheckAllreadySanction = qurey.LvWFDetails.Where(e => e.WFStatus == 2).ToList();
                        if (CheckAllreadySanction.Count() > 0)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                        }
                        LvWFDetails oLvWFDetails = new LvWFDetails
                        {
                            WFStatus = 2,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };
                        oLvWFDetails_List.Add(oLvWFDetails);

                        //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        qurey.TrClosed = true;
                        SanctionRejected = true;
                    }
                }
                else if (authority.ToUpper() == "APPROVAL")//Hr
                {
                    if (Approval == null)
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status" }, JsonRequestBehavior.AllowGet);
                    }
                    if (ReasonApproval == "")
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                    }
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        //approval yes-3
                        var CheckAllreadySanction = qurey.LvWFDetails.Where(e => e.WFStatus == 3).ToList();
                        if (CheckAllreadySanction.Count() > 0)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Approved....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                        }
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 3,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        if (qurey.DebitDays > LvNoOfDaysFrom && qurey.DebitDays <= LvNoOfDaysTo)
                        {
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            if (AppInchargeid != null && AppInchargeid != "" && AppInchargeid != "-Select-")
                            {
                                var value = db.Employee.Find(int.Parse(AppInchargeid));
                                qurey.Incharge = value;

                            }
                        }
                        qurey.LvWFDetails.Add(LvWFDetails);
                        //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                    }
                    else if (Convert.ToBoolean(Approval) == false)
                    {
                        //approval no-4
                        //var LvWFDetails = new LvWFDetails
                        //{
                        //    WFStatus = 4,
                        //    Comments = ReasonApproval,
                        //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        //};
                        //qurey.LvWFDetails.Add(LvWFDetails);

                        //qurey.LvWFDetails.Add(LvWFDetails);
                        var CheckAllreadySanction = qurey.LvWFDetails.Where(e => e.WFStatus == 4).ToList();
                        if (CheckAllreadySanction.Count() > 0)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                        }
                        LvWFDetails oLvWFDetails = new LvWFDetails
                        {
                            WFStatus = 4,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };
                        oLvWFDetails_List.Add(oLvWFDetails);
                        //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        qurey.TrClosed = true;
                        HrRejected = true;
                    }
                }
                else if (authority.ToUpper() == "RECOMMENDATION")
                {

                    if (Recomendation == null)
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Recomendation Status" }, JsonRequestBehavior.AllowGet);
                    }
                    if (ReasonRecomendation == "")
                    {
                        return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                    }
                    if (Convert.ToBoolean(Recomendation) == true)
                    {
                        //Recomendation yes -7
                        var CheckAllreadyRecomendation = qurey.LvWFDetails.Where(e => e.WFStatus == 7).ToList();
                        if (CheckAllreadyRecomendation.Count() > 0)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Recomendation....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                        }
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 7,
                            Comments = ReasonRecomendation,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.LvWFDetails.Add(LvWFDetails);
                        //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        if (qurey.DebitDays > LvNoOfDaysFrom && qurey.DebitDays <= LvNoOfDaysTo)
                        {
                            qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            if (RecInchargeid != null && RecInchargeid != "" && RecInchargeid != "-Select-")
                            {
                                var value = db.Employee.Find(int.Parse(RecInchargeid));
                                qurey.Incharge = value;

                            }

                        }
                    }
                    else if (Convert.ToBoolean(Recomendation) == false)
                    {
                        //Recommendation no -8

                        var CheckAllreadyRecomendation = qurey.LvWFDetails.Where(e => e.WFStatus == 8).ToList();
                        if (CheckAllreadyRecomendation.Count() > 0)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = "This Leave Allready Rejected....Refresh The Page!" }, JsonRequestBehavior.AllowGet);
                        }
                        LvWFDetails oLvWFDetails = new LvWFDetails
                        {
                            WFStatus = 8,
                            Comments = ReasonRecomendation,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };
                        oLvWFDetails_List.Add(oLvWFDetails);

                        //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        qurey.TrClosed = true;
                        SanctionRejected = true;
                    }
                }
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if someone reject lv
                        if (SanctionRejected == true || HrRejected == true)
                        {
                            var OEmployeeLv = db.EmployeeLeave
                                  .Include(e => e.Employee)
                                   .Include(e => e.LvNewReq)
                                   .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                   .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                  .Where(e => e.Id == EmpLvId)
                                //.Select(e => new { EmpId = e.Id, LvNewReq = e.LvNewReq })
                                  .SingleOrDefault();

                            var PrevReq = OEmployeeLv.LvNewReq
                               .Where(e => e.LeaveHead != null && e.LeaveHead.Id == qurey.LeaveHead.Id
                                //&& e.IsCancel == false
                                    ).OrderByDescending(e => e.Id).FirstOrDefault();

                            LvNewReq oLvNewReq = new LvNewReq()
                            {
                                ReqDate = DateTime.Now,
                                ContactNo = qurey.ContactNo,
                                DebitDays = 0,
                                InputMethod = 1,
                                TrClosed = true,
                                TrReject = true,
                                IsCancel = true,
                                LvOrignal = qurey,
                                CreditDays = qurey.DebitDays,
                                FromDate = qurey.FromDate,
                                FromStat = qurey.FromStat,
                                LeaveHead = qurey.LeaveHead,
                                Reason = qurey.Reason,
                                ResumeDate = qurey.ResumeDate,
                                ToDate = qurey.ToDate,
                                ToStat = qurey.ToStat,
                                LeaveCalendar = qurey.LeaveCalendar,
                                DBTrack = qurey.DBTrack,
                                OpenBal = PrevReq.CloseBal,
                                CloseBal = PrevReq.CloseBal + qurey.DebitDays,
                                LVCount = PrevReq.LVCount - qurey.DebitDays,
                                LvOccurances = PrevReq.LvOccurances - 1,
                                GeoStruct = qurey.GeoStruct,
                                PayStruct = qurey.PayStruct,
                                FuncStruct = qurey.FuncStruct,
                                LvWFDetails = oLvWFDetails_List,
                                Narration = "Leave Cancelled",
                                LvCountPrefixSuffix = PrevReq.LvCountPrefixSuffix,
                                WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault() //db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault()
                            };
                            try
                            {
                                db.LvNewReq.Add(oLvNewReq);
                                db.SaveChanges();
                                var emplv = db.EmployeeLeave
                                    .Include(e => e.LvNewReq)
                                    //.Include(e => e.LvNewReq.Select(a => a.GeoStruct))
                                    //.Include(e => e.LvNewReq.Select(a => a.PayStruct))
                                    //.Include(e => e.LvNewReq.Select(a => a.FuncStruct))
                                    .Where(e => e.Employee.Id == OEmployeeLv.Employee.Id)
                                    .SingleOrDefault();
                                emplv.LvNewReq.Add(oLvNewReq);
                                db.Entry(emplv).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(emplv).State = System.Data.Entity.EntityState.Detached;
                            }
                            catch (Exception e)
                            {
                                return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        db.LvNewReq.Attach(qurey);
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        //public ActionResult UpdateStatus(LvNewReq LvReq, FormCollection form, String data)
        //{

        //    string authority = form["authority"] == null ? null : form["authority"];
        //    var isClose = form["isClose"] == null ? null : form["isClose"];
        //    if (authority == null && isClose == null)
        //    {
        //        return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //    }
        //    var ids = Utility.StringIdsToListString(data);
        //    var lvnewreqid = Convert.ToInt32(ids[0]);
        //    var EmpLvId = Convert.ToInt32(ids[1]);
        //    string Sanction = form["Sanction"];
        //    string ReasonSanction = form["ReasonSanction"];
        //    string HR = form["HR"] == null ? null : form["HR"];
        //    string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
        //    string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
        //    string Approval = form["Approval"];
        //    string ReasonApproval = form["ReasonApproval"];
        //    string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
        //    bool SanctionRejected = false;
        //    bool HrRejected = false;
        //    List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();

        //    //bool self = false;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var qurey = db.LvNewReq
        //            .Include(e => e.WFStatus)
        //            .Include(e => e.LeaveCalendar)
        //            .Include(e => e.LeaveHead)
        //            .Include(e => e.LvWFDetails)
        //            .Include(e => e.FromStat)
        //            .Include(e => e.GeoStruct)
        //            .Include(e => e.LvOrignal)
        //            .Include(e => e.PayStruct)
        //            .Include(e => e.FuncStruct)
        //            .Where(e => e.Id == lvnewreqid).SingleOrDefault();

        //        var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
        //        if (authority.ToUpper() == "MYSELF")
        //        {
        //            qurey.Reason = ReasonMySelf;
        //            qurey.IsCancel = true;
        //            qurey.TrClosed = true;
        //            qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
        //        }
        //        if (authority.ToUpper() == "SANCTION")
        //        {

        //            if (Sanction == null)
        //            {
        //                return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status" }, JsonRequestBehavior.AllowGet);
        //            }
        //            if (ReasonSanction == "")
        //            {
        //                return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
        //            }
        //            if (Convert.ToBoolean(Sanction) == true)
        //            {
        //                //sanction yes -1
        //                var LvWFDetails = new LvWFDetails
        //                {
        //                    WFStatus = 1,
        //                    Comments = ReasonSanction,
        //                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
        //                };
        //                qurey.LvWFDetails.Add(LvWFDetails);
        //                //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
        //                qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
        //            }
        //            else if (Convert.ToBoolean(Sanction) == false)
        //            {
        //                //sanction no -2
        //                //var LvWFDetails = new LvWFDetails
        //                //{
        //                //    WFStatus = 2,
        //                //    Comments = ReasonSanction,
        //                //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
        //                //};
        //                //qurey.LvWFDetails.Add(LvWFDetails);
        //                LvWFDetails oLvWFDetails = new LvWFDetails
        //                {
        //                    WFStatus = 2,
        //                    Comments = ReasonSanction,
        //                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //                };
        //                oLvWFDetails_List.Add(oLvWFDetails);

        //                //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
        //                qurey.TrClosed = true;
        //                SanctionRejected = true;
        //            }
        //        }
        //        else if (authority.ToUpper() == "APPROVAL")//Hr
        //        {
        //            if (Approval == null)
        //            {
        //                return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status" }, JsonRequestBehavior.AllowGet);
        //            }
        //            if (ReasonApproval == "")
        //            {
        //                return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
        //            }
        //            if (Convert.ToBoolean(Approval) == true)
        //            {
        //                //approval yes-3
        //                var LvWFDetails = new LvWFDetails
        //                {
        //                    WFStatus = 3,
        //                    Comments = ReasonApproval,
        //                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
        //                };
        //                qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
        //                qurey.LvWFDetails.Add(LvWFDetails);
        //                //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
        //            }
        //            else if (Convert.ToBoolean(Approval) == false)
        //            {
        //                //approval no-4
        //                //var LvWFDetails = new LvWFDetails
        //                //{
        //                //    WFStatus = 4,
        //                //    Comments = ReasonApproval,
        //                //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
        //                //};
        //                //qurey.LvWFDetails.Add(LvWFDetails);

        //                //qurey.LvWFDetails.Add(LvWFDetails);
        //                LvWFDetails oLvWFDetails = new LvWFDetails
        //                {
        //                    WFStatus = 4,
        //                    Comments = ReasonApproval,
        //                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
        //                };
        //                oLvWFDetails_List.Add(oLvWFDetails);
        //                //  qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
        //                qurey.TrClosed = true;
        //                HrRejected = true;
        //            }
        //        }
        //        else if (authority.ToUpper() == "RECOMMAND")
        //        {

        //        }
        //        try
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                //if someone reject lv
        //                if (SanctionRejected == true || HrRejected == true)
        //                {
        //                    var OEmployeeLv = db.EmployeeLeave
        //                           .Include(e => e.Employee)
        //                           .Include(e => e.LvNewReq)
        //                           .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
        //                           .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
        //                           .Where(e => e.Id == EmpLvId)
        //                        //.Select(e => new { EmpId = e.Id, LvNewReq = e.LvNewReq })
        //                          .SingleOrDefault();

        //                    var PrevReq = OEmployeeLv.LvNewReq
        //                        .Where(e => e.LeaveHead != null && e.LeaveHead.Id == qurey.LeaveHead.Id
        //                        //&& e.IsCancel == false
        //                            ).OrderByDescending(e => e.Id).FirstOrDefault();

        //                    LvNewReq oLvNewReq = new LvNewReq()
        //                    {
        //                        ReqDate = DateTime.Now,
        //                        ContactNo = qurey.ContactNo,
        //                        DebitDays = 0,
        //                        InputMethod = 1,
        //                        TrClosed = true,
        //                        TrReject = true,
        //                        IsCancel = true,
        //                        LvOrignal = qurey,
        //                        CreditDays = qurey.DebitDays,
        //                        FromDate = qurey.FromDate,
        //                        FromStat = qurey.FromStat,
        //                        LeaveHead = qurey.LeaveHead,
        //                        Reason = qurey.Reason,
        //                        ResumeDate = qurey.ResumeDate,
        //                        ToDate = qurey.ToDate,
        //                        ToStat = qurey.ToStat,
        //                        LeaveCalendar = qurey.LeaveCalendar,
        //                        DBTrack = qurey.DBTrack,
        //                        OpenBal = PrevReq.CloseBal,
        //                        CloseBal = PrevReq.CloseBal + qurey.DebitDays,
        //                        LVCount = PrevReq.LVCount - qurey.DebitDays,
        //                        LvOccurances = PrevReq.LvOccurances - 1,
        //                        GeoStruct = qurey.GeoStruct,
        //                        PayStruct = qurey.PayStruct,
        //                        FuncStruct = qurey.FuncStruct,
        //                        LvWFDetails = oLvWFDetails_List,
        //                        Narration = "Leave Cancelled",
        //                        WFStatus = db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault()
        //                    };
        //                    try
        //                    {
        //                        db.LvNewReq.Add(oLvNewReq);
        //                        db.SaveChanges();
        //                        var emplv = db.EmployeeLeave
        //                            .Include(e => e.LvNewReq)
        //                            //.Include(e => e.LvNewReq.Select(a => a.GeoStruct))
        //                            //.Include(e => e.LvNewReq.Select(a => a.PayStruct))
        //                            //.Include(e => e.LvNewReq.Select(a => a.FuncStruct))
        //                            .Where(e => e.Employee.Id == OEmployeeLv.Employee.Id)
        //                            .SingleOrDefault();
        //                        emplv.LvNewReq.Add(oLvNewReq);
        //                        db.Entry(emplv).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(emplv).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //                db.LvNewReq.Attach(qurey);
        //                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
        //                ts.Complete();
        //                return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public class EmpLvClass
        {
            public string EmpId { get; set; }
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public List<ReqLvHeadWise> LvHeadName { get; set; }
        }
        public class ReqLvHeadWise
        {
            public string LvHeadName { get; set; }
            public string LvHeadCode { get; set; }
            public string LvHeadBal { get; set; }
            public Array LvReq { get; set; }
        }
        public ActionResult GetEmpLvHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";

                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                              .Include(e => e.LookupValues)
                              .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();


                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);

                //if (EmpIds == null && EmpIds.Count == 0)
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
                List<int> Funcsubid = new List<int>();
                List<EmployeeLeave> Emps = null;
                List<int> EmpsAll = new List<int>();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();
                foreach (var item1 in EmpidsWithfunsub)
                {
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        // List<int> Funcsubid = new List<int>();
                        Emps = db.EmployeeLeave
                           .Where(e => item1.ReportingEmployee.Contains(e.Employee.Id))
                           .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                           .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                           .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                           .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                           .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                           .Include(e => e.Employee.EmpName)
                           .ToList();
                        if (item1.SubModuleName != null)
                        {
                            List<LvHead> Allleavhead = db.LvHead.Distinct().AsNoTracking().ToList(); //  Below line commented because empcode multiple times come according to lvheadwise
                            // List<LvHead> Allleavhead = db.LvHead.Where(e => e.LvCode.ToUpper() == item1.SubModuleName).Distinct().AsNoTracking().ToList();
                            foreach (var item2 in Allleavhead)
                            {
                                Funcsubid.Add(item2.Id);
                            }
                        }
                        else
                        {
                            List<LvHead> Allleavhead = db.LvHead.Distinct().AsNoTracking().ToList();
                            foreach (var item2 in Allleavhead)
                            {
                                Funcsubid.Add(item2.Id);
                            }
                        }
                        if (Emps.Count() > 0)
                        {
                            foreach (var emid in Emps)
                            {
                                EmpsAll.Add(emid.Id);
                            }
                        }

                    }
                }
                //  var allLvHead = db.LvHead.ToList();
                Emps = db.EmployeeLeave
                         .Where(e => EmpsAll.Contains(e.Id))
                         .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                         .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                         .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                         .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                         .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                         .Include(e => e.Employee.EmpName)
                          .Include(e => e.Employee.ServiceBookDates)
                          .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                         .ToList();

                foreach (var ca in Emps)
                {
                    var oEmpLvClass = new EmpLvClass();
                    foreach (var lvhead in Funcsubid.Distinct())
                    {
                        double Debitdays = 0, Lvopening = 0, Lvclosing = 0;

                        var lvcal = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == "LEAVECALENDAR")
                                && (e.Default == true)).SingleOrDefault();
                        //var openinbal = ca.LvOpenBal.Where(e => e.LvHead.Id == lvhead && e.LvCalendar.Id == lvcal.Id).LastOrDefault();
                        //if (openinbal == null)
                        //{
                        var openballvnewreq = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead).OrderByDescending(e => e.Id).FirstOrDefault();
                        var PrevReq1 = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead)
                       .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.CloseBal + openballvnewreq.LVCount, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                        if (PrevReq1 != null)
                        {
                            Debitdays = PrevReq1.LvOccurances;
                            Lvopening = PrevReq1.LvOpening;
                            Lvclosing = PrevReq1.LvClosing;
                        }
                        //  }
                        //else
                        //{
                        //    var PrevReq = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead && e.LeaveCalendar.Id == lvcal.Id
                        //        //&& e.IsCancel == false
                        //          )
                        //    .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openinbal.LvOpening, LvClosing = e.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                        //    if (PrevReq != null)
                        //    {
                        //        Debitdays = PrevReq.LvOccurances;
                        //        Lvopening = PrevReq.LvOpening;
                        //        Lvclosing = PrevReq.LvClosing;
                        //    }
                        //}
                        //double debitdays = 0;
                        //var openinbal1 = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead && e.LeaveCalendar.Id == lvcal.Id).LastOrDefault();
                        //if (openinbal1 != null)
                        //{
                        //    debitdays = openinbal1.LVCount;
                        //}
                        //var bal = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead).OrderByDescending(e => e.ReqDate)
                        //    .Select(e => new { LvOpening = openinbal.LvOpening, e.CloseBal, debitdays }).FirstOrDefault();
                        var temp = new List<tempClass>();
                        var LvData = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead && e.LeaveCalendar.Id == lvcal.Id).ToList();
                        foreach (var item in LvData)
                        {
                            var Status = "--";
                            if ((item.InputMethod == 1 || item.InputMethod == 2) && item.LvWFDetails.Count > 0)
                            {
                                Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                                .Select(e => e.Value).SingleOrDefault();
                                if (Status.ToUpper() == "SANCTIONED" && item.TrClosed == true)
                                {
                                    Status = "Sanctioned and Approved";
                                }
                                else if (Status.ToUpper() == "RECOMMENDATION" && item.TrClosed == true)
                                {
                                    Status = "Recomendation and Approved";
                                }
                            }
                            if (item.InputMethod == 0)
                            {
                                Status = "Approved By HRM (M)";
                            }
                            var ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null;
                            var ToDate = item.ToDate != null ? item.ToDate.Value.ToString("dd/MM/yyyy") : null;
                            var FromDate = item.FromDate != null ? item.FromDate.Value.ToString("dd/MM/yyyy") : null;
                            temp.Add(new tempClass
                            {
                                LvName = item.LeaveHead.LvName,
                                LvCode = item.LeaveHead.LvCode,
                                LvBal = "Lv Opening Bal :" + Lvopening + " Debit Days :" + Debitdays + " Balance :" + Lvclosing,
                                FullDetails =
                                "ReqDate :" + ReqDate +
                                " FromDate :" + FromDate +
                                " ToDate :" + ToDate +
                                " OpenBal :" + item.OpenBal +
                                " DebitDays :" + item.DebitDays +
                                " CreditDays :" + item.CreditDays +
                                " CloseBal :" + item.CloseBal +
                                " Reason :" + item.Reason +
                                    // " Status :" + Utility.GetStatusName().Where(a => item.LvWFDetails.Count > 0 && a.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault()
                               " Status :" + Status
                            });
                        }
                        //var LvData = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id).OrderByDescending(e => e.ReqDate)
                        //    .Select(e => new
                        //    {
                        //        LvName = e.LeaveHead.LvName,
                        //        LvCode = e.LeaveHead.LvCode,
                        //        LvBal = "Lv Opening Bal :" + bal.LvOpening + " Balance :" + bal.CloseBal + " LvOccurances :" + bal.LvOccurances,
                        //        FullDetails =
                        //        "ReqDate :" + e.ReqDate.Value.ToString("dd/MM/yyyy") +
                        //        " FromDate :" + e.FromDate.Value.ToString("dd/MM/yyyy") +
                        //        " ToDate :" + e.ToDate.Value.ToString("dd/MM/yyyy") +
                        //        " OpenBal :" + e.OpenBal +
                        //        " DebitDays :" + e.DebitDays +
                        //        " CloseBal :" + e.CloseBal +
                        //        " Reason :" + e.Reason +
                        //        " Status :" + Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault()
                        //    }).ToList();

                        if (temp != null && temp.Count > 0)
                        {
                            oEmpLvClass.EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                            if (oEmpLvClass.LvHeadName == null)
                            {
                                oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=temp.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=temp.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                            LvReq =temp.Select(e=>e.FullDetails).ToArray(),
                                            LvHeadBal=temp.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                            }
                            else
                            {
                                oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                {
                                    LvHeadName = temp.Select(e => e.LvName).FirstOrDefault().ToString(),
                                    LvHeadCode = temp.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                    LvReq = temp.Select(e => e.FullDetails).ToArray(),
                                    LvHeadBal = temp.Select(e => e.LvBal).FirstOrDefault().ToString()
                                });
                            }

                        }
                        else
                        {
                            var LvDataOpening = ca.LvOpenBal.Where(e => e.LvHead.Id == lvhead && e.LvCalendar.Id == lvcal.Id)
                                 .Select(e => new
                                 {
                                     LvName = e.LvHead.LvName,
                                     LvCode = e.LvHead.LvCode,
                                     LvBal = "Lv Opening Bal :" + e.LvOpening + " Balance :" + e.LvOpening + " Debit Days :" + e.LVCount,

                                 }).ToList();

                            if (LvDataOpening != null && LvDataOpening.Count > 0)
                            {
                                oEmpLvClass.EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                                if (oEmpLvClass.LvHeadName == null)
                                {
                                    oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=LvDataOpening.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=LvDataOpening.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                             LvHeadBal=LvDataOpening.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                                }
                                else
                                {
                                    oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                    {
                                        LvHeadName = LvDataOpening.Select(e => e.LvName).FirstOrDefault().ToString(),
                                        LvHeadCode = LvDataOpening.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                        LvHeadBal = LvDataOpening.Select(e => e.LvBal).FirstOrDefault().ToString()
                                    });
                                }
                            }
                        }

                    }
                    if (oEmpLvClass.EmpName != null)
                    {
                        ListEmpLvClass.Add(oEmpLvClass);
                    }
                }
                // }
                //}
                return Json(new Utility.JsonClass { status = true, Data = ListEmpLvClass }, JsonRequestBehavior.AllowGet);
            }
        }
        public class AttachDataClass
        {
            public Int32 EmpId { get; set; }
            public Int32 EmpLVid { get; set; }
            public Int32 emppayrollid { get; set; }
            public Int32 LvNewReq { get; set; }
            public String status { get; set; }
            public string val { get; set; }
            public string Empname { get; set; }
            public bool IsClose { get; set; }
            public Int32 LvHead_Id { get; set; }
            public Int32 ItinvestmentId { get; set; }

            public String Id { get; set; }
        }
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }
        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public class GetLvNewReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LvHead { get; set; }
            public DateTime? FromDate { get; set; }
            public string ToDate { get; set; }
            public string Debitdays { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public string Actiondate { get; set; }
            //public string BossName { get; set; }
            public string ActionBy { get; set; }
            public string WF { get; set; }
            public string GetMonth { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetLvNewReqClass1
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LvHead { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string Debitdays { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public string Actiondate { get; set; }
            //public string BossName { get; set; }
            public string ActionBy { get; set; }
            public string WF { get; set; }
            public string GetMonth { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetLvNewReqClass2
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LvHead { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }

            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public ActionResult GetLvNewReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();

                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var lookupdata = db.Lookup
                               .Include(e => e.LookupValues)
                               .Where(e => e.Code == "601").AsNoTracking().FirstOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();


                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //  var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);

                // Leave Permission out of leave year ini file define date in dd/mm/yyyy formate
                string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                bool exists = System.IO.Directory.Exists(requiredPath);
                string localPath;
                if (!exists)
                {
                    localPath = new Uri(requiredPath).LocalPath;
                    System.IO.Directory.CreateDirectory(localPath);
                }
                string path = requiredPath + @"\LeaveSanPermission" + ".ini";
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
                // Leave Permission out of leave year
                // Leave sanction Permission out of leave year formate dd/mm/yyyy
                string text = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                bool flag2 = Directory.Exists(text);
                string text2 = "";
                string localPath1;
                if (!flag2)
                {
                    localPath1 = new Uri(text).LocalPath;
                    Directory.CreateDirectory(localPath1);
                }

                string uriString = text + "\\LeaveSanPermission.ini";
                localPath1 = new Uri(uriString).LocalPath;
                using (StreamReader streamReader = new StreamReader(localPath1))
                {
                    string text3;
                    while ((text3 = streamReader.ReadLine()) != null)
                    {
                        text2 = text3;
                    }
                }
                // Leave sanction Permission out of leave year formate dd/mm/yyyy


                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                List<int> EmpIds = new List<int>();
                string funsubmodule = "";
                List<GetLvNewReqClass2> ListreturnLvnewClass = new List<GetLvNewReqClass2>();
                List<EmployeeLeave> LvList = new List<EmployeeLeave>();

                ListreturnLvnewClass.Add(new GetLvNewReqClass2
                {
                    Emp = "Employee",
                    ReqDate = "Requisition Date",
                    LvHead = "Leave Head",
                    FromDate = "From Date",
                    ToDate = "To Date"
                });
                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        List<string> Funcsubid = new List<string>();
                        var temp = db.EmployeeLeave
                          .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.LvNewReq)
                           .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                           .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                           .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                            //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                           .Include(e => e.LvNewReq.Select(a => a.LvWFDetails));
                        //.Where(e => EmpIds.Contains(e.Employee.Id)).ToList();

                         LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                        var LvIds = UserManager.FilterLv(LvList.SelectMany(e => e.LvNewReq).OrderByDescending(e => e.ReqDate).ToList(),
                            Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId), item1);

                        var session = Session["auho"].ToString().ToUpper();

                        var listlvids = new List<int>();
                        if (LvIds.Count() >= 100)
                        {
                            listlvids = LvIds.Take(100).ToList();
                        }
                        else
                        {
                            listlvids = LvIds.ToList();
                        }
                        foreach (var item in listlvids)
                        {

                            var lvoriginal = db.LvNewReq.Where(e => e.LvOrignal_Id == item).SingleOrDefault();
                            if (lvoriginal != null)
                            {
                                continue;
                            }
                            var query = db.EmployeeLeave.Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                            .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                                .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                               .Where(e => e.LvNewReq.Any(a => a.Id == item))

                                .SingleOrDefault();



                            //foreach (var lvcode in Funcsubid)
                            //{
                            if (item1.SubModuleName == null || item1.SubModuleName =="")
                            {
                                List<int> lvcode = query.LvNewReq.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                                foreach (var Lvheadid in lvcode)
                                {
                                    DateTime? lvcrdate = query.LvNewReq.Where(a => a.LeaveHead.Id == Lvheadid && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                                    if (lvcrdate != null)
                                    {
                                        DateTime? Lvyearfrom = lvcrdate;
                                        DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                        LvyearTo = LvyearTo.Value.AddYears(1);

                                        if (text2 != "")
                                        {
                                            DateTime dateTime3 = Convert.ToDateTime(text2);
                                            if (!(DateTime.Now.Date <= dateTime3.Date))
                                            {
                                                Lvyearfrom = lvcrdate;
                                                LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                                LvyearTo = LvyearTo.Value.AddYears(1);
                                            }
                                            else
                                            {
                                                Lvyearfrom = lvcrdate.Value.AddMonths(-6); // for half yearly permission  july leave year (half) jan to june leave sanction
                                                LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                                LvyearTo = LvyearTo.Value.AddYears(1);
                                            }
                                        }
                                        else
                                        {
                                            Lvyearfrom = lvcrdate;
                                            LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                            LvyearTo = LvyearTo.Value.AddYears(1);
                                        }
                                        //var LvReq = query.LvNewReq.Where(a => a.Id == item && a.LeaveHead.Id == Lvheadid && a.ReqDate >= Lvyearfrom && a.ReqDate <= LvyearTo).FirstOrDefault();
                                        var LvReq = query.LvNewReq.Where(a => a.Id == item && a.LeaveHead.Id == Lvheadid && a.FromDate >= Lvyearfrom && a.FromDate <= LvyearTo).FirstOrDefault();
                                        if (LvReq != null)
                                        {
                                            ListreturnLvnewClass.Add(new GetLvNewReqClass2
                                            {
                                                RowData = new ChildGetLvNewReqClass2
                                                {
                                                    LvNewReq = LvReq.Id.ToString(),
                                                    EmpLVid = query.Id.ToString(),
                                                    IsClose = query.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                    .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                                                    LvHead_Id = LvReq.LeaveHead.Id.ToString(),
                                                },
                                                Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                                ReqDate = LvReq.ReqDate.Value.ToShortDateString(),
                                                LvHead = LvReq.LeaveHead.LvName,
                                                FromDate = LvReq.FromDate.Value.ToShortDateString(),
                                                ToDate = LvReq.ToDate.Value.ToShortDateString(),

                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                List<int> lvcode = query.LvNewReq.Where(e => e.LvCreditDate != null && e.LeaveHead.LvCode.ToUpper() == item1.SubModuleName.ToUpper()).Select(e => e.LeaveHead.Id).Distinct().ToList();
                                foreach (var Lvheadid in lvcode)
                                {
                                    DateTime? lvcrdate = query.LvNewReq.Where(a => a.LeaveHead.Id == Lvheadid && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                                    if (lvcrdate != null)
                                    {
                                        DateTime? Lvyearfrom = lvcrdate;
                                        DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                        LvyearTo = LvyearTo.Value.AddYears(1);
                                        // var LvReq = query.LvNewReq.Where(a => a.Id == item && a.LeaveHead.Id == Lvheadid && a.ReqDate >= Lvyearfrom && a.ReqDate <= LvyearTo).FirstOrDefault();
                                        var LvReq = query.LvNewReq.Where(a => a.Id == item && a.LeaveHead.Id == Lvheadid && a.FromDate >= Lvyearfrom && a.FromDate <= LvyearTo).FirstOrDefault();
                                        if (LvReq != null)
                                        {
                                            ListreturnLvnewClass.Add(new GetLvNewReqClass2
                                            {
                                                RowData = new ChildGetLvNewReqClass2
                                                {
                                                    LvNewReq = LvReq.Id.ToString(),
                                                    EmpLVid = query.Id.ToString(),
                                                    IsClose = query.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                    .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                                                    LvHead_Id = LvReq.LeaveHead.Id.ToString(),
                                                },
                                                Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                                ReqDate = LvReq.ReqDate.Value.ToShortDateString(),
                                                LvHead = LvReq.LeaveHead.LvName,
                                                FromDate = LvReq.FromDate.Value.ToShortDateString(),
                                                ToDate = LvReq.ToDate.Value.ToShortDateString(),

                                            });
                                        }
                                    }
                                }

                            }
                            // }

                        }
                    }
                }

                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GetMyLvReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeLeave
                      .Where(e => e.Id == Id)
                      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                      .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                     .SingleOrDefault();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var lvcal = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == "LEAVECALENDAR")
                              && (e.Default == true)).SingleOrDefault();
                var LvOrignal_id = db_data.LvNewReq.Where(e => e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();
                var AntCancel = db_data.LvNewReq.OrderBy(e => e.Id).ToList();
                var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.LeaveCalendar.Id == lvcal.Id).OrderBy(e => e.Id).ToList();

                if (listLvs != null && listLvs.Count() > 0)
                {
                    List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                    List<GetLvNewReqClass> returndata11 = new List<GetLvNewReqClass>();
                    List<GetLvNewReqClass1> returndata1 = new List<GetLvNewReqClass1>();
                    returndata1.Add(new GetLvNewReqClass1
                    {
                        ReqDate = "Requisition Date",
                        LvHead = "Leave Head",
                        FromDate = "From Date",
                        ToDate = "To Date",
                        Debitdays = "Days",
                        Reason = "Reason",
                        Status = "Status",
                        Actiondate = "Actiondate",
                        //BossName = "BossName"
                        ActionBy = "Action By"
                    });
                    List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                    foreach (var item1 in lvcode)
                    {
                        DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                        if (lvcrdate != null)
                        {
                            DateTime? Lvyearfrom = lvcrdate;
                            DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                            LvyearTo = LvyearTo.Value.AddYears(1);
                            foreach (var item in listLvs.Where(e => e.Narration != "Leave Encash Payment" && e.LeaveHead.Id == item1 &&
                             // e.WFStatus != null && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.WFStatus.LookupVal != "4").OrderByDescending(a => a.ReqDate).ToList())
                             //e.WFStatus != null && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" ).OrderByDescending(a => a.ReqDate).ToList())\\04062024
                             e.WFStatus != null && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.WFStatus.LookupVal != "2").OrderBy(a => a.FromDate).ToList())
                            {
                                var FromDate = item.FromDate != null ? item.FromDate.Value.ToShortDateString() : null;
                                var ToDate = item.ToDate != null ? item.ToDate.Value.ToShortDateString() : null;
                                var Status = "--";
                                var Actiondate="";
                                //var BossName = "--";
                                var ActionBy = "--";
                                if ((item.InputMethod == 1 || item.InputMethod == 2) && item.LvWFDetails.Count > 0)
                                {
                                    Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                                    .Select(e => e.Value).SingleOrDefault();

                                    var bossaction = item.LvWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().DBTrack;
                                    if (bossaction!=null)
                                    {
                                        if (item.LvWFDetails.ToList().OrderByDescending(e => e.Id).FirstOrDefault().WFStatus!=0)
                                        {
                                            Actiondate = Convert.ToDateTime(bossaction.CreatedOn).ToString("dd/MM/yyyy");  
                                        }
                                      
                                      var  empcode = bossaction.CreatedBy != null ? bossaction.CreatedBy : null;
                                      if (empcode!=null)
                                      {
                                          int empidl=Convert.ToInt32(empcode);
                                          var emp = db.Employee.Include(e => e.EmpName).Where(e => e.Id == empidl).SingleOrDefault();
                                          ActionBy = emp.EmpName != null ? emp.EmpName.FullNameFML.ToString() : "";
                                      }
                                    }
                                  
                                    if (Status.ToUpper() == "SANCTIONED" && item.TrClosed == true)
                                    {
                                        Status = "Sanctioned and Approved";
                                    }
                                    else if (Status.ToUpper() == "RECOMMENDATION" && item.TrClosed == true)
                                    {
                                        Status = "Recomendation and Approved";
                                    }
                                }
                                if (item.InputMethod == 0)
                                {
                                    if (item.IsCancel == true)
                                    {
                                        Status = "Cancel By HRM (M)";
                                    }
                                    else
                                    {
                                        Status = "Approved By HRM (M)";
                                    }

                                }
                                //if (item.InputMethod == 1 && item.IsCancel == true)
                                //{
                                //    // Status = item.LvWFDetails.LastOrDefault().WFStatus.ToString() == "2" ? "Lv Cancel Compensation" : "--";
                                //    if (item.LvWFDetails.LastOrDefault().WFStatus.ToString() == "2")
                                //    {
                                //        Status = "Sanction Rejected";
                                //    }
                                //    else if (item.LvWFDetails.LastOrDefault().WFStatus.ToString() == "4")
                                //    {
                                //        Status = "Approved Rejected";
                                //    }
                                //}

                                returndata.Add(new GetLvNewReqClass
                                {
                                    RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = item.Id.ToString(),
                                        EmpLVid = db_data.Id.ToString(),
                                        IsClose = Status,
                                        Status = Status,
                                        LvHead_Id = item.LeaveHead.Id.ToString(),
                                    },
                                    ReqDate = item.ReqDate.Value.ToShortDateString(),
                                    LvHead = item.LeaveHead.LvName,
                                    FromDate = item.FromDate,
                                    ToDate = item.ToDate == null ? "" : item.ToDate.Value.ToShortDateString(),
                                    Debitdays = item.DebitDays != 0 ? item.DebitDays.ToString() : item.CreditDays.ToString(),//credit days taken if cancel leave
                                    Reason = item.Reason == null ? "" : item.Reason,
                                    Status = Status,
                                    Actiondate = Actiondate,
                                    ActionBy = ActionBy,
                                   
                                });
                        
                            }
                        }
                    }
                   // var resultq = returndata.GroupBy(x => x.FromDate).Select(y => y.First()).ToList();
                    var resultq = returndata.OrderByDescending(r => r.FromDate).ToList();

                    foreach (var item1 in resultq)
                    {
                        returndata1.Add(new GetLvNewReqClass1
                        {
                            RowData = new ChildGetLvNewReqClass
                                    {
                                        LvNewReq = item1.RowData.LvNewReq,
                                        EmpLVid = item1.RowData.EmpLVid,
                                        IsClose = item1.RowData.IsClose,
                                        Status = item1.RowData.Status,
                                        LvHead_Id = item1.RowData.LvHead_Id,
                                    },
                                    ReqDate = item1.ReqDate,
                                    LvHead = item1.LvHead,
                                    FromDate = item1.FromDate == null ? "" : item1.FromDate.Value.ToShortDateString(),
                                    ToDate = item1.ToDate == null ? "" : item1.ToDate,
                                    Debitdays = item1.Debitdays,
                                    Reason = item1.Reason,
                                    Status = item1.Status,
                                    Actiondate = item1.Actiondate,
                                    ActionBy = item1.ActionBy,
                                   
                                });
                        
                          }

                    return Json(new { status = true, data = returndata1 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class LVDataClass
        {
            public string Lvhead { get; set; }
            public string OpenBal { get; set; }
            public string Credit { get; set; }
            public string Dedit { get; set; }
            public string Utilized { get; set; }
            public string Closing { get; set; }
            public string Lapes { get; set; }
        }

        public List<LVDataClass> GetLeaveCDebit(int EmployeeId, string LvCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<LVDataClass> resultdata = new List<LVDataClass>();

                var OEmployeeLVData = db.EmployeeLeave
                .Include(e => e.LvNewReq)
                .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                .Include(e => e.Employee)
                .Include(e => e.Employee.EmpName).Where(e => e.Employee_Id == EmployeeId).ToList();

                var lvhead = db.LvHead.ToList();

                foreach (var item11 in OEmployeeLVData)
                {
                    foreach (var lvid in lvhead)
                    {
                        bool firstrec = false;
                        double openbal = 0;
                        double Closebal = 0;
                        double Creditday = 0;
                        double utilise = 0;
                        double Lapes = 0;

                        var lvreq = item11.LvNewReq.Where(e => e.LeaveHead_Id == lvid.Id && e.LeaveHead.LvCode == LvCode).OrderByDescending(e => e.Id).ToList();
                        if (lvreq.Count() == 1)
                        {
                            foreach (var item12 in lvreq)
                            {
                                if (firstrec == false)
                                {
                                    Closebal = item12.CloseBal;
                                    openbal = item12.OpenBal;
                                    Creditday = item12.CreditDays - item12.LvLapsed;
                                    utilise = item12.OpenBal + (item12.CreditDays - item12.LvLapsed) - item12.CloseBal;
                                    Lapes = item12.LvLapsed;
                                    resultdata.Add(new LVDataClass
                                    {
                                        Lvhead = item12.LeaveHead == null ? " " : item12.LeaveHead.LvCode,
                                        OpenBal = openbal.ToString(),
                                        Credit = Creditday.ToString(),
                                        Utilized = utilise.ToString(),
                                        Closing = Closebal.ToString(),
                                        Lapes = Lapes.ToString(),
                                    });

                                }
                            }
                        }
                        else
                        {
                            foreach (var item12 in lvreq)
                            {
                                if (firstrec == false)
                                {
                                    Closebal = item12.CloseBal;
                                }

                                if (firstrec == true && (item12.Narration == "Credit Process" || item12.Narration == "Leave Opening Balance"))
                                {
                                    openbal = item12.OpenBal;
                                    Creditday = item12.CreditDays;
                                    utilise = item12.OpenBal + item12.CreditDays - Closebal;
                                    Lapes = item12.LvLapsed;
                                    resultdata.Add(new LVDataClass
                                    {
                                        Lvhead = item12.LeaveHead == null ? " " : item12.LeaveHead.LvCode,
                                        OpenBal = openbal.ToString(),
                                        Credit = Creditday.ToString(),
                                        Utilized = utilise.ToString(),
                                        Closing = Closebal.ToString(),
                                        Lapes = Lapes.ToString(),
                                    });
                                    break;
                                }

                                firstrec = true;
                            }
                        }

                    }

                }

                return resultdata;
            }
        }
        public class tempClass
        {
            public string LvName { get; set; }
            public string LvCode { get; set; }
            public string LvBal { get; set; }
            public string FullDetails { get; set; }
        }
        public ActionResult GetMyLvBalnceHistory(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var id = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[1]);
                var lvheadcode = ids[3];

                var ivalue = GetLeaveCDebit(id, lvheadcode);

                var Emps = db.EmployeeLeave
                      .Where(e => e.Employee.Id == id)
                      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                      .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                      .Include(e => e.LvOpenBal.Select(a => a.LvCalendar.Name))
                      .Include(e => e.Employee.EmpName).AsNoTracking().AsParallel()
                      .ToList();

                var allLvHead = db.LvHead.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps)
                {
                    var oEmpLvClass = new EmpLvClass();
                    foreach (var lvhead in allLvHead)
                    {
                        double Debitdays = 0, Lvopening = 0, Lvclosing = 0;

                        var lvcal = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == "LEAVECALENDAR")
                                && (e.Default == true)).SingleOrDefault();

                        var openballvnewreq = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id && e.LeaveHead.LvCode == lvheadcode).OrderByDescending(e => e.Id).FirstOrDefault();
                        var PrevReq1 = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id && e.LeaveHead.LvCode == lvheadcode)
                       .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.CloseBal + openballvnewreq.LVCount, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                        if (PrevReq1 != null)
                        {
                            Debitdays = PrevReq1.LvOccurances;
                            Lvopening = PrevReq1.LvOpening;
                            Lvclosing = PrevReq1.LvClosing;
                        }
                        var temp = new List<tempClass>();
                        var LvData = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id && e.LeaveCalendar.Id == lvcal.Id && e.LeaveHead.LvCode == lvheadcode).OrderBy(e => e.Id).ToList();
                        foreach (var item in LvData)
                        {
                            var Status = "--";
                            if ((item.InputMethod == 1 || item.InputMethod == 2) && item.LvWFDetails.Count > 0)
                            {
                                Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                                .Select(e => e.Value).SingleOrDefault();
                                if (Status.ToUpper() == "SANCTIONED" && item.TrClosed == true)
                                {
                                    Status = "Sanctioned and Approved";
                                }
                                else if (Status.ToUpper() == "RECOMMENDATION" && item.TrClosed == true)
                                {
                                    Status = "Recomendation and Approved";
                                }

                            }
                            if (item.InputMethod == 0)
                            {
                                Status = "Approved By HRM (M)";
                            }
                            var ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null;
                            var ToDate = item.ToDate != null ? item.ToDate.Value.ToString("dd/MM/yyyy") : null;
                            var FromDate = item.FromDate != null ? item.FromDate.Value.ToString("dd/MM/yyyy") : null;
                            temp.Add(new tempClass
                            {
                                LvName = item.LeaveHead.LvName,
                                LvCode = item.LeaveHead.LvCode,
                                LvBal = ":" + Lvopening + ":" + Debitdays + ":" + Lvclosing,
                                FullDetails =
                                ":" + ReqDate +
                                ":" + FromDate +
                                ":" + ToDate +
                                ":" + item.OpenBal +
                                ":" + item.DebitDays +
                                ":" + item.CreditDays +
                                ":" + item.CloseBal +
                                ":" + item.Reason +
                                ":" + Status +
                                ":" + item.Narration
                            });
                        }

                        if (LvData != null && LvData.Count > 0)
                        {
                            oEmpLvClass.EmpCode = ca.Employee.EmpCode;
                            oEmpLvClass.EmpName = ca.Employee.EmpName.FullNameFML;
                            if (oEmpLvClass.LvHeadName == null)
                            {
                                oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=temp.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=temp.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                            LvReq =temp.Select(e=>e.FullDetails).ToArray(),
                                            LvHeadBal=temp.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                            }
                            else
                            {
                                oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                {
                                    LvHeadName = temp.Select(e => e.LvName).FirstOrDefault().ToString(),
                                    LvHeadCode = temp.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                    LvReq = temp.Select(e => e.FullDetails).ToArray(),
                                    LvHeadBal = temp.Select(e => e.LvBal).FirstOrDefault().ToString()
                                });
                            }

                        }
                        else
                        {
                            var LvDataOpening = ca.LvOpenBal.Where(e => e.LvHead.Id == lvhead.Id && e.LvCalendar.Id == lvcal.Id && e.LvHead.LvCode == lvheadcode)
                                 .Select(e => new
                                 {
                                     LvName = e.LvHead.LvName,
                                     LvCode = e.LvHead.LvCode,
                                     LvBal = ":" + e.LvOpening + ":" + e.LvOpening + ":" + e.LVCount,

                                 }).ToList();

                            if (LvDataOpening != null && LvDataOpening.Count > 0)
                            {
                                oEmpLvClass.EmpCode = ca.Employee.EmpCode;
                                oEmpLvClass.EmpName = ca.Employee.EmpName.FullNameFML;
                                if (oEmpLvClass.LvHeadName == null)
                                {
                                    oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=LvDataOpening.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=LvDataOpening.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                             LvHeadBal=LvDataOpening.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                                }
                                else
                                {
                                    oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                    {
                                        LvHeadName = LvDataOpening.Select(e => e.LvName).FirstOrDefault().ToString(),
                                        LvHeadCode = LvDataOpening.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                        LvHeadBal = LvDataOpening.Select(e => e.LvBal).FirstOrDefault().ToString()
                                    });
                                }
                            }
                        }
                    }
                    if (oEmpLvClass.EmpName != null)
                    {
                        ListEmpLvClass.Add(oEmpLvClass);
                    }
                    List<GetTotalBalanceHistoryClass> return_data = new List<GetTotalBalanceHistoryClass>();

                    if (ListEmpLvClass.Count() != 0)
                    {
                        foreach (var item in ListEmpLvClass)
                        {
                            //foreach (var item1 in item.LvHeadName)
                            //{
                                //string[] values = (item1.LvHeadBal.Split(new string[] { ":" }, StringSplitOptions.None));
                            foreach (var iz in ivalue)
                            {
                                return_data.Add(new GetTotalBalanceHistoryClass
                                {
                                    EmpCode = item.EmpCode,
                                    EmpName = item.EmpName,
                                    LvCode = iz.Lvhead,
                                    LvOpenBal = iz.OpenBal,
                                    CreditDays = iz.Credit,
                                    DebitDays = iz.Utilized,
                                    Balance = iz.Closing
                                });
                            }
                               
                            //}
                        }

                    }
                    if (ListEmpLvClass.Count() != 0)
                    {

                        List<BalnceHistoryofalldataclass> returndata = new List<BalnceHistoryofalldataclass>();
                        foreach (var item in ListEmpLvClass)
                        {
                            foreach (var item1 in item.LvHeadName.Where(e=>e.LvReq!=null))
                            {
                                foreach (var item2 in item1.LvReq)
                                {
                                    string[] values = (item2.ToString().Split(new string[] { ":" }, StringSplitOptions.None));

                                    returndata.Add(new BalnceHistoryofalldataclass
                                    {
                                        EmpCode=item.EmpCode,
                                        EmpName=item.EmpName,
                                        LvCode=item1.LvHeadCode,
                                        ReqDate = values[1],
                                        FromDate = values[2],
                                        ToDate = values[3],
                                        OpenBal = values[4],
                                        DebitDays = values[5],
                                        CreditDays = values[6],
                                        CloseBal = values[7],
                                        Reason = values[8],
                                        Status = values[9],
                                        Narration = values[10]
                                    });
                                }
                            }
                        }
                        return Json(new Utility.JsonNewObjClass { status = true, Data1 = return_data, Data2 = returndata }, JsonRequestBehavior.AllowGet);
                    }

                }

                return null;
            }
        }

        public class ChildGetBalanceHistoryClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public class GetTotalBalanceHistoryClass
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string LvCode { get; set; }
            public string LvHeadName { get; set; }
            public string LvOpenBal { get; set; }
            public string CreditDays { get; set; }
            public string DebitDays { get; set; }
            public string Balance { get; set; }       
        }

        public class GetBalanceHistoryClass
        {
            //public string EmpCode {get; set;}
            //public string EmpName { get; set; }
            //public string LvCode { get; set; }
            public string LvHeadName { get; set; }
            //public string LvOpenBal { get; set; }
            //public string DebitDays { get; set; }
            //public string Balance { get; set; }
            public ChildGetBalanceHistoryClass RowData { get; set; }
        }
        public ActionResult BalnceHistoryofLvHead()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var id = Convert.ToInt32(SessionManager.EmpId);
                var Emps = db.EmployeeLeave
                      .Where(e => e.Employee.Id == id)
                      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                      .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                      .Include(e => e.LvOpenBal.Select(a => a.LvCalendar.Name))
                      .Include(e => e.Employee.EmpName).AsNoTracking().AsParallel()
                      .ToList();

                var allLvHead = db.LvHead.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps)
                {
                    var oEmpLvClass = new EmpLvClass();
                    foreach (var lvhead in allLvHead)
                    {
                        double Debitdays = 0, Lvopening = 0, Lvclosing = 0;

                        var lvcal = db.Calendar.Where(e => (e.Name.LookupVal.ToUpper() == "LEAVECALENDAR")
                               && (e.Default == true)).SingleOrDefault();

                        var openballvnewreq = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id).OrderByDescending(e => e.Id).FirstOrDefault();
                        var PrevReq1 = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id)
                       .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.CloseBal + openballvnewreq.LVCount, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                        if (PrevReq1 != null)
                        {
                            Debitdays = PrevReq1.LvOccurances;
                            Lvopening = PrevReq1.LvOpening;
                            Lvclosing = PrevReq1.LvClosing;
                        }

                        var temp = new List<tempClass>();
                        var LvData = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id && e.LeaveCalendar.Id == lvcal.Id).OrderBy(e => e.FromDate).ToList();
                        foreach (var item in LvData)
                        {
                            temp.Add(new tempClass
                            {
                                LvName = item.LeaveHead.LvName,
                                LvCode = item.LeaveHead.LvCode,
                                LvBal = ":" + Lvopening + ":" + Debitdays + ":" + Lvclosing,
                            });
                        }

                        if (LvData != null && LvData.Count > 0)
                        {
                            oEmpLvClass.EmpId = ca.Employee.Id.ToString();
                            oEmpLvClass.EmpCode = ca.Employee.EmpCode;
                            oEmpLvClass.EmpName = ca.Employee.EmpName.FullNameFML;
                            if (oEmpLvClass.LvHeadName == null)
                            {
                                oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=temp.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=temp.Select(e=>e.LvCode).FirstOrDefault().ToString(),                                          
                                            LvHeadBal=temp.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }
                                };
                            }
                            else
                            {
                                oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                {
                                    LvHeadName = temp.Select(e => e.LvName).FirstOrDefault().ToString(),
                                    LvHeadCode = temp.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                    LvHeadBal = temp.Select(e => e.LvBal).FirstOrDefault().ToString()
                                });
                            }

                        }
                        else
                        {
                            var LvDataOpening = ca.LvOpenBal.Where(e => e.LvHead.Id == lvhead.Id && e.LvCalendar.Id == lvcal.Id)
                                 .Select(e => new
                                 {
                                     LvName = e.LvHead.LvName,
                                     LvCode = e.LvHead.LvCode,
                                     LvBal = ":" + e.LvOpening + ":" + e.LvOpening + ":" + e.LVCount,
                                 }).ToList();

                            if (LvDataOpening != null && LvDataOpening.Count > 0)
                            {
                                oEmpLvClass.EmpId = ca.Employee.Id.ToString();
                                oEmpLvClass.EmpCode = ca.Employee.EmpCode;
                                oEmpLvClass.EmpName = ca.Employee.EmpName.FullNameFML;
                                if (oEmpLvClass.LvHeadName == null)
                                {
                                    oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=LvDataOpening.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=LvDataOpening.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                            LvHeadBal=LvDataOpening.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                                }
                                else
                                {
                                    oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                    {
                                        LvHeadName = LvDataOpening.Select(e => e.LvName).FirstOrDefault().ToString(),
                                        LvHeadCode = LvDataOpening.Select(e => e.LvCode).FirstOrDefault().ToString(),
                                        LvHeadBal = LvDataOpening.Select(e => e.LvBal).FirstOrDefault().ToString()
                                    });
                                }
                            }
                        }
                    }
                    if (oEmpLvClass.EmpName != null)
                    {
                        ListEmpLvClass.Add(oEmpLvClass);
                    }

                    if (ListEmpLvClass.Count() != 0)
                    {
                        List<GetBalanceHistoryClass> returndata = new List<GetBalanceHistoryClass>();
                        returndata.Add(new GetBalanceHistoryClass
                        {
                            LvHeadName = "LvHeadName",
                            //LvOpenBal = "LvOpenBal",
                            //DebitDays = "DebitDays",
                            //Balance = "Balance"
                        });

                        foreach (var item in ListEmpLvClass)
                        {
                            foreach (var item1 in item.LvHeadName)
                            {

                                string[] values = (item1.LvHeadBal.Split(new string[] { ":" }, StringSplitOptions.None));

                                returndata.Add(new GetBalanceHistoryClass
                                {
                                    RowData = new ChildGetBalanceHistoryClass
                                    {
                                        LvNewReq = "",
                                        EmpLVid = item.EmpId,
                                        IsClose = "",
                                        Status = "",
                                        LvHead_Id = item1.LvHeadCode
                                    },
                                    LvHeadName = item1.LvHeadName,
                                    //LvOpenBal = values[1],
                                    //DebitDays = values[2],
                                    //Balance = values[3]
                                });
                            }
                        }
                        return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                    }
                }

                return null;
            }
        }

        public class BalnceHistoryofalldataclass
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string LvCode { get; set; }
            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string OpenBal { get; set; }
            public string DebitDays { get; set; }
            public string CreditDays { get; set; }
            public string CloseBal { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }
            public string Narration { get; set; }
        }


        public class ChildLvPolicyListClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Policy_Data { get; set; }
        }
        public class LvPolicyList
        {
            //public string LvNewReq { get; set; }
            public string val { get; set; }
            public ChildLvPolicyListClass RowData { get; set; }

        }
        public class LvHolidayList
        {
            public string HolidayName { get; set; }
            public string valHolidayType { get; set; }
            public string HolidayDate { get; set; }
            public ChildLvPolicyListClass RowData { get; set; }

        }

        public class LvWeeklyList
        {
            public string WeekDays { get; set; }
            public string WeeklyOffStatus { get; set; }
            public ChildLvPolicyListClass RowData { get; set; }

        }
        public ActionResult LvCreditPolicy_Partial()
        {
            return View("~/Views/Shared/_LvCreditPolicy.cshtml");
        }


        public ActionResult GetLvCreditPolicy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var ComplvId = Convert.ToInt32(SessionManager.CompLvId);

                var db_data = db.CompanyLeave.Include(q => q.LvCreditPolicy).Where(e => e.Id == ComplvId).SingleOrDefault();

                if (db_data != null)
                {
                    List<LvPolicyList> returndata = new List<LvPolicyList>();
                    returndata.Add(new LvPolicyList
                    {
                        //  LvNewReq= "Id",
                        val = "Policy Name",
                    });
                    foreach (var item in db_data.LvCreditPolicy.OrderByDescending(a => a.PolicyName).ToList())
                    {
                        returndata.Add(new LvPolicyList
                        {
                            RowData = new ChildLvPolicyListClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                Policy_Data = item.PolicyName.ToString(),
                            },
                            //LvNewReq= item.Id.ToString(),
                            val = item.PolicyName,
                        });

                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GetLvCreditPolicyData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                //var EmpLvIdint = Convert.ToInt32(emplvId);

                var LvCreditPolicyData = db.LvCreditPolicy
                    .Include(e => e.CreditDate)
                    .Include(e => e.ConvertLeaveHead)
                    .Include(e => e.ConvertLeaveHeadBal)
                    .Include(e => e.LvHead)
                    .Include(e => e.ExcludeLeaveHeads)
                    .Where(e => e.Id == id).SingleOrDefault();

                var oLvCreditPolicyClass = new LvCreditPolicyClass()
                {
                    AboveServiceMaxYears = LvCreditPolicyData.AboveServiceMaxYears,
                    AboveServiceSteps = LvCreditPolicyData.AboveServiceSteps,
                    Accumalation = LvCreditPolicyData.Accumalation,
                    AccumalationLimit = LvCreditPolicyData.AccumalationLimit,
                    AccumulationWithCredit = LvCreditPolicyData.AccumulationWithCredit,
                    ConvertedDays = LvCreditPolicyData.ConvertedDays,
                    CreditDays = LvCreditPolicyData.CreditDays,
                    ExcludeLeaves = LvCreditPolicyData.ExcludeLeaves,
                    FixedCreditDays = LvCreditPolicyData.FixedCreditDays,
                    IsCreditDatePolicy = LvCreditPolicyData.IsCreditDatePolicy,
                    LVConvert = LvCreditPolicyData.LVConvert,
                    LVConvertBal = LvCreditPolicyData.LVConvertBal,
                    LvConvertLimit = LvCreditPolicyData.LvConvertLimit,
                    LvConvertLimitBal = LvCreditPolicyData.LvConvertLimitBal,
                    MaxLeaveDebitInService = LvCreditPolicyData.MaxLeaveDebitInService,
                    OccCarryForward = LvCreditPolicyData.OccCarryForward,
                    OccInServAppl = LvCreditPolicyData.OccInServAppl,
                    OccInServ = LvCreditPolicyData.OccInServ,
                    PolicyName = LvCreditPolicyData.PolicyName,
                    ProCreditFrequency = LvCreditPolicyData.ProCreditFrequency,
                    ProdataFlag = LvCreditPolicyData.ProdataFlag,
                    ServiceLink = LvCreditPolicyData.ServiceLink,
                    ServiceYearsLimit = LvCreditPolicyData.ServiceYearsLimit,
                    WorkingDays = LvCreditPolicyData.WorkingDays,
                    CreditDate = LvCreditPolicyData.CreditDate == null ? null : LvCreditPolicyData.CreditDate.LookupVal,
                    LvHead = LvCreditPolicyData.LvHead == null ? null : LvCreditPolicyData.LvHead.FullDetails,
                    //Lvhead_FullDetails = LvCreditPolicyData.LvHead.LvName,
                };
                //var CLH = db.LvCreditPolicy.Include(e => e.ConvertLeaveHead).Where(e => e.Id == id && e.ConvertLeaveHead.Count > 0).Select(e => e.ConvertLeaveHead).ToList();
                //if (CLH != null)
                //{
                //    var Arr_salhead_list = CLH.ToArray().Distinct();
                //    var ConvertLeaveHead = Arr_salhead_list != null ? string.Join(",", Arr_salhead_list) : null;
                //    oLvCreditPolicyClass.ConvertLeaveHead = ConvertLeaveHead;
                //}

                //var CLHB = db.LvCreditPolicy.Include(e => e.ConvertLeaveHeadBal).Where(e => e.Id == id && e.ConvertLeaveHeadBal.Count > 0).Select(e => e.ConvertLeaveHeadBal).ToList();
                //if (CLHB != null)
                //{
                //    var CLHB_list = CLHB.ToArray().Distinct();
                //    var ConvertLeaveHeadBal = CLHB_list != null ? string.Join(",", CLHB_list) : null;
                //    oLvCreditPolicyClass.ConvertLeaveHeadBal = ConvertLeaveHeadBal;
                //}

                var ELH = db.LvCreditPolicy.Include(e => e.ExcludeLeaveHeads).Where(e => e.Id == id && e.ExcludeLeaveHeads.Count > 0).Select(e => e.ExcludeLeaveHeads).ToList();
                if (ELH != null)
                {
                    var ExcludeLeaveHeads_list = ELH.ToArray().Distinct();
                    var ExcludeLeaveHeads = ExcludeLeaveHeads_list != null ? string.Join(",", ExcludeLeaveHeads_list) : null;
                    oLvCreditPolicyClass.ExcludeLeaveHeads = ExcludeLeaveHeads;
                }
                return Json(oLvCreditPolicyClass);
            }
        }
        public ActionResult LvDebitPolicy_Partial()
        {
            return View("~/Views/Shared/_LvDebitPolicy.cshtml");
        }

        public ActionResult GetLvDebitPolicyData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                //var EmpLvIdint = Convert.ToInt32(emplvId);

                var LvCreditPolicyData = db.LvDebitPolicy
                    //.Include(e => e.CombineLeaveHeads)
                             .Include(e => e.LvHead)
                             .Where(e => e.Id == id).SingleOrDefault();
                var oLvCreditPolicyClass = new LvCreditPolicyClass()
                {
                    ApplyFutureGrace = LvCreditPolicyData.ApplyFutureGraceMonths,
                    ApplyPastGrace = LvCreditPolicyData.ApplyPastGraceMonths,
                    Combined = LvCreditPolicyData.Combined == true ? "YES" : "No",
                    HalfDayAppl = LvCreditPolicyData.HalfDayAppl,
                    HolidayInclusive = LvCreditPolicyData.HolidayInclusive,
                    MaxUtilDays = LvCreditPolicyData.MaxUtilDays,
                    MinUtilDays = LvCreditPolicyData.MinUtilDays,
                    PolicyName = LvCreditPolicyData.PolicyName,
                    PostApplied = LvCreditPolicyData.PostApplied,
                    PostApplyPrefixSuffix = LvCreditPolicyData.PostApplyPrefixSuffix,
                    PostDays = LvCreditPolicyData.PostDays,
                    PreApplied = LvCreditPolicyData.PreApplied,
                    PreApplyPrefixSuffix = LvCreditPolicyData.PreApplyPrefixSuffix,
                    PreDays = LvCreditPolicyData.PreDays,
                    PrefixGraceCount = LvCreditPolicyData.PrefixGraceCount,
                    PrefixMaxCount = LvCreditPolicyData.PrefixMaxCount,
                    PrefixSuffix = LvCreditPolicyData.PrefixSuffix,
                    Sandwich = LvCreditPolicyData.Sandwich,
                    LvHead = LvCreditPolicyData.LvHead != null ? LvCreditPolicyData.LvHead.FullDetails : null,
                    YearlyOccurances = LvCreditPolicyData.YearlyOccurances,
                };

                var CLH = db.LvDebitPolicy.
                    Include(e => e.CombinedLvHead)
                    .Include(e => e.CombinedLvHead.Select(r => r.LvHead))
                    .Where(e => e.Id == id && e.CombinedLvHead.Count > 0)
                    .SingleOrDefault();
                if (CLH != null)
                {
                    var Arr_salhead_list = CLH.CombinedLvHead.Select(e => e.FullDetails).ToList();
                    var CombineLeaveHeads = Arr_salhead_list != null ? string.Join(",", Arr_salhead_list) : null;
                    oLvCreditPolicyClass.CombineLeaveHeads = CombineLeaveHeads;
                }
                return Json(oLvCreditPolicyClass);
            }
        }
        public ActionResult LvEnCashPolicy_Partial()
        {
            return View("~/Views/Shared/_LvEnCashPolicy.cshtml");
        }

        public ActionResult GetLvDebitPolicy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var ComplvId = Convert.ToInt32(SessionManager.CompLvId);

                var db_data = db.CompanyLeave.Include(q => q.LvDebitPolicy).Where(e => e.Id == ComplvId).SingleOrDefault();

                if (db_data != null)
                {
                    List<LvPolicyList> returndata = new List<LvPolicyList>();
                    returndata.Add(new LvPolicyList
                    {
                        //  LvNewReq= "Id",
                        val = "Policy Name",
                    });
                    foreach (var item in db_data.LvDebitPolicy.OrderByDescending(a => a.PolicyName).ToList())
                    {
                        returndata.Add(new LvPolicyList
                        {
                            RowData = new ChildLvPolicyListClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                Policy_Data = item.PolicyName.ToString(),
                            },
                            //LvNewReq= item.Id.ToString(),
                            val = item.PolicyName,
                        });

                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetHolidayList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var ComplvId = Convert.ToInt32(SessionManager.CompLvId);
                var location_id = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location).Where(e => e.Id == Id).SingleOrDefault();

                var db_data = db.Location.Include(e => e.HolidayCalendar)
                    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList))
                    .Include(e => e.HolidayCalendar.Select(a => a.HoliCalendar))
                    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList.Select(q => q.Holiday)))
                    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList.Select(q => q.Holiday.HolidayName)))
                    .Include(e => e.HolidayCalendar.Select(a => a.HolidayList.Select(q => q.Holiday.HolidayType))).AsNoTracking()
                    .Where(e => e.Id == location_id.GeoStruct.Location.Id && e.HolidayCalendar.Count() > 0).AsParallel().ToList();

                if (db_data.Count > 0)
                {
                    List<LvHolidayList> returndata = new List<LvHolidayList>();
                    returndata.Add(new LvHolidayList
                    {

                        HolidayName = "Holiday Name",
                        valHolidayType = "Holiday Type",
                        HolidayDate = "Holiday Date",
                    });

                    foreach (var HolidayCalendar in db_data.SelectMany(e => e.HolidayCalendar))
                    {
                        var holilist = HolidayCalendar.HolidayList;
                        if (holilist.Count > 0)
                        {
                            foreach (var HolidayList in holilist)
                            {
                                Holiday holi = HolidayList.Holiday;
                                returndata.Add(new LvHolidayList
                                {
                                    RowData = new ChildLvPolicyListClass
                                    {
                                        EmpLVid = HolidayList.Id.ToString(),
                                        // EmpLVid = db_data.Id.ToString(),
                                        Policy_Data = HolidayList.Holiday.ToString(),
                                    },
                                    HolidayName = holi.HolidayName != null ? holi.HolidayName.LookupVal.ToString() : "",
                                    valHolidayType = holi.HolidayType != null ? holi.HolidayType.LookupVal.ToString() : "",
                                    HolidayDate = HolidayList.HolidayDate.Value.ToShortDateString(),
                                });
                            }
                        }
                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class LvCreditPolicyClass
        {

            public bool AboveServiceMaxYears { get; set; }

            public double AboveServiceSteps { get; set; }

            public double AccumalationLimit { get; set; }

            public bool AccumulationWithCredit { get; set; }

            public double ConvertedDays { get; set; }

            public double CreditDays { get; set; }

            public bool ExcludeLeaves { get; set; }

            public bool FixedCreditDays { get; set; }

            public bool IsCreditDatePolicy { get; set; }

            public bool LVConvert { get; set; }

            public bool LVConvertBal { get; set; }

            public double LvConvertLimit { get; set; }

            public double LvConvertLimitBal { get; set; }

            public double MaxLeaveDebitInService { get; set; }

            public bool OccCarryForward { get; set; }

            public bool OccInServAppl { get; set; }

            public double OccInServ { get; set; }

            public string PolicyName { get; set; }

            public bool Accumalation { get; set; }

            public int ProCreditFrequency { get; set; }

            public bool ProdataFlag { get; set; }

            public bool ServiceLink { get; set; }

            public int ServiceYearsLimit { get; set; }

            public double WorkingDays { get; set; }

            public string CreditDate { get; set; }

            public string LvHead { get; set; }

            public string Lvhead_FullDetails { get; set; }

            public string Action { get; set; }

            public string ConvertLeaveHead { get; set; }

            public string ConvertLeaveHeadBal { get; set; }

            public string ExcludeLeaveHeads { get; set; }

            public bool HolidayInclusive { get; set; }

            public int ApplyFutureGrace { get; set; }

            public bool HalfDayAppl { get; set; }

            public double MaxUtilDays { get; set; }

            public double MinUtilDays { get; set; }

            public bool PostApplied { get; set; }

            public bool PostApplyPrefixSuffix { get; set; }

            public double PostDays { get; set; }

            public bool PreApplied { get; set; }

            public double PreDays { get; set; }

            public double PrefixGraceCount { get; set; }

            public double PrefixMaxCount { get; set; }

            public bool PrefixSuffix { get; set; }

            public bool Sandwich { get; set; }

            public int Lvhead_Id { get; set; }

            public double YearlyOccurances { get; set; }

            public bool PreApplyPrefixSuffix { get; set; }

            public string Combined { get; set; }

            public int ApplyPastGrace { get; set; }

            public string CombineLeaveHeads { get; set; }

            public string Lvhead { get; set; }

            public string WeekDays { get; set; }

            public string HolidayName { get; set; }

            public string HolidayType { get; set; }

            public string HolidayDate { get; set; }

            public string WeeklyOffStatus { get; set; }

        }

        public ActionResult GetHolidayListData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[1]);
                //var status = ids.Count > 0 ? ids[2] : null;
                //var emplvId = ids.Count > 0 ? ids[1] : null;
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                //var EmpLvIdint = Convert.ToInt32(emplvId);

                var LvCreditPolicyData = db.HolidayList
                    .Include(e => e.Holiday)
                             .Include(e => e.Holiday.HolidayName)
                             .Include(e => e.Holiday.HolidayType)
                             .Where(e => e.Id == id).SingleOrDefault();
                var oLvCreditPolicyClass = new LvCreditPolicyClass()
                {
                    HolidayName = LvCreditPolicyData.Holiday.HolidayName.LookupVal.ToString(),
                    HolidayType = LvCreditPolicyData.Holiday.HolidayType.LookupVal.ToString(),
                    HolidayDate = LvCreditPolicyData.HolidayDate.Value.ToShortDateString(),
                };

                var CLH = db.LvDebitPolicy.Include(e => e.CombinedLvHead)
                    .Where(e => e.Id == id && e.CombinedLvHead.Count > 0)
                    .Select(e => e.CombinedLvHead).SingleOrDefault();
                if (CLH != null)
                {
                    var Arr_salhead_list = CLH.ToArray().Distinct();
                    var CombineLeaveHeads = Arr_salhead_list != null ? string.Join(",", Arr_salhead_list) : null;
                    oLvCreditPolicyClass.CombineLeaveHeads = CombineLeaveHeads;
                }
                return Json(oLvCreditPolicyClass);
            }
        }

        public ActionResult GetWeaklyOffData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                //var status = ids.Count > 0 ? ids[2] : null;
                //var emplvId = ids.Count > 0 ? ids[1] : null;
                //var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                //var EmpLvIdint = Convert.ToInt32(emplvId);

                var LvCreditPolicyData = db.WeeklyOffList
                    .Include(e => e.WeekDays)
                             .Include(e => e.WeeklyOffStatus)
                             .Where(e => e.Id == id).SingleOrDefault();
                var oLvCreditPolicyClass = new LvCreditPolicyClass()
                {
                    WeekDays = LvCreditPolicyData.WeekDays.LookupVal.ToString(),
                    WeeklyOffStatus = LvCreditPolicyData.WeeklyOffStatus.LookupVal.ToString(),
                };

                var CLH = db.LvDebitPolicy.Include(e => e.CombinedLvHead)
                    .Where(e => e.Id == id && e.CombinedLvHead.Count > 0)
                    .Select(e => e.CombinedLvHead).SingleOrDefault();
                if (CLH != null)
                {
                    var Arr_salhead_list = CLH.ToArray().Distinct();
                    var CombineLeaveHeads = Arr_salhead_list != null ? string.Join(",", Arr_salhead_list) : null;
                    oLvCreditPolicyClass.CombineLeaveHeads = CombineLeaveHeads;
                }
                return Json(oLvCreditPolicyClass);
            }
        }
        public ActionResult GetLvEnCashPolicy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var ComplvId = Convert.ToInt32(SessionManager.CompLvId);

                var db_data = db.CompanyLeave.Include(q => q.LvEncashPolicy).Where(e => e.Id == ComplvId).SingleOrDefault();

                if (db_data != null)
                {
                    List<LvPolicyList> returndata = new List<LvPolicyList>();
                    returndata.Add(new LvPolicyList
                    {
                        //  LvNewReq= "Id",
                        val = "Policy Name",
                    });
                    foreach (var item in db_data.LvEncashPolicy.OrderByDescending(a => a.PolicyName).ToList())
                    {
                        returndata.Add(new LvPolicyList
                        {
                            RowData = new ChildLvPolicyListClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                Policy_Data = item.PolicyName.ToString(),
                            },
                            //LvNewReq= item.Id.ToString(),
                            val = item.PolicyName,
                        });

                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetLvEnCashPolicyData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                var LvHeadId = ids.Count > 0 ? ids[3] : null;

                //var lvheadidint = Convert.ToInt32(LvHeadId);
                //var EmpLvIdint = Convert.ToInt32(emplvId);
                var Q = db.LvEncashPolicy.Include(e => e.LvHead)
                              .Where(e => e.Id == id).Select
                              (e => new
                              {
                                  PolicyName = e.PolicyName,
                                  EncashSpanYear = e.EncashSpanYear,
                                  MinBalance = e.MinBalance,
                                  MinEncashment = e.MinEncashment,
                                  MaxEncashment = e.MaxEncashment,
                                  MinUtilized = e.MinUtilized,
                                  IsLvMultiple = e.IsLvMultiple,
                                  IsOnBalLv = e.IsOnBalLv,
                                  LvBalPercent = e.LvBalPercent,
                                  LvMultiplier = e.LvMultiplier,
                                  IsLvRequestAppl = e.IsLvRequestAppl,
                                  LvHead = e.LvHead == null ? null : e.LvHead.FullDetails,
                              }).SingleOrDefault();
                return Json(Q);
            }
        }


        //public ActionResult GetHolidayList()
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //        var Id = Convert.ToInt32(SessionManager.EmpLvId);
        //        var ComplvId = Convert.ToInt32(SessionManager.CompLvId);
        //        var location_id = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location).Where(e => e.Id == Id).SingleOrDefault();

        //        var db_data = db.Location.Include(e => e.HolidayCalendar)
        //            .Include(e => e.HolidayCalendar.Select(a => a.HolidayList))
        //            .Include(e => e.HolidayCalendar.Select(a => a.HolidayList.Select(q => q.Holiday)))
        //            .Include(e => e.HolidayCalendar.Select(a => a.HolidayList.Select(q => q.Holiday.HolidayName)))
        //            .Include(e => e.HolidayCalendar.Select(a => a.HolidayList.Select(q => q.Holiday.HolidayType)))
        //            .Where(e => e.Id == location_id.GeoStruct.Location.Id && e.HolidayCalendar.Count() > 0).ToList();

        //        if (db_data.Count > 0)
        //        {
        //            List<LvHolidayList> returndata = new List<LvHolidayList>();
        //            returndata.Add(new LvHolidayList
        //            {

        //                HolidayName = "Holiday Name",
        //                valHolidayType = "Holiday Type",
        //                HolidayDate = "Holiday Date",
        //            });

        //            foreach (var HolidayCalendar in db_data.SelectMany(e => e.HolidayCalendar))
        //            {
        //                foreach (var HolidayList in HolidayCalendar.HolidayList)
        //                {
        //                    returndata.Add(new LvHolidayList
        //                    {
        //                        RowData = new ChildLvPolicyListClass
        //                        {
        //                            LvNewReq = HolidayList.Id.ToString(),
        //                            // EmpLVid = db_data.Id.ToString(),
        //                            Policy_Data = HolidayList.Holiday.ToString(),
        //                        },
        //                        HolidayName = HolidayList.Holiday.HolidayName.LookupVal.ToString(),
        //                        valHolidayType = HolidayList.Holiday.HolidayType.LookupVal.ToString(),
        //                        HolidayDate = HolidayList.HolidayDate.Value.ToShortDateString(),
        //                    });
        //                }
        //            }
        //            return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}


        public ActionResult GetWeaklyOffList()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var ComplvId = Convert.ToInt32(SessionManager.CompLvId);
                var location_id = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Location).Where(e => e.Id == Id).SingleOrDefault();

                var db_data = db.Location.Include(e => e.WeeklyOffCalendar)
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList))
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList.Select(q => q.WeeklyOffStatus)))
                    .Include(e => e.WeeklyOffCalendar.Select(a => a.WeeklyOffList.Select(q => q.WeekDays)))
                    .Where(e => e.Id == location_id.GeoStruct.Location.Id && e.WeeklyOffCalendar.Count() > 0).ToList();

                List<LvWeeklyList> returndata = new List<LvWeeklyList>();
                returndata.Add(new LvWeeklyList
                {

                    WeekDays = "Week Days",
                    WeeklyOffStatus = "Weekly Off Status",
                });

                if (db_data.Count > 0)
                {
                    foreach (var HolidayCalendar in db_data.SelectMany(e => e.WeeklyOffCalendar))
                    {
                        foreach (var HolidayList in HolidayCalendar.WeeklyOffList)
                        {
                            returndata.Add(new LvWeeklyList
                            {
                                RowData = new ChildLvPolicyListClass
                                {
                                    LvNewReq = HolidayList.Id.ToString(),
                                    // EmpLVid = db_data.Id.ToString(),
                                    Policy_Data = HolidayList.WeekDays.ToString(),
                                },
                                WeekDays = HolidayList.WeekDays.LookupVal.ToString(),
                                WeeklyOffStatus = HolidayList.WeeklyOffStatus.LookupVal.ToString(),
                            });
                        }
                    }

                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class returnClassGet_Employee_History
        {
            public string LeaveCalendar { get; set; }
            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string ResumeDate { get; set; }
            public string LvName { get; set; }
            public string OpenBal { get; set; }
            public string DebitDays { get; set; }
            public string CloseBal { get; set; }
            public string FromStat { get; set; }
            public string ToStat { get; set; }
            public string IsCancel { get; set; }
            public string LvOccurances { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }

        }

        public class returnClassGet_Employee_Span_period_History
        {
            public string EmpName { get; set; }
            public string LeaveCalendar { get; set; }
            public string ReqDate { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string ResumeDate { get; set; }
            public string LvName { get; set; }
            public string OpenBal { get; set; }
            public string DebitDays { get; set; }
            public string CloseBal { get; set; }
            public string FromStat { get; set; }
            public string ToStat { get; set; }
            public string IsCancel { get; set; }
            public string LvOccurances { get; set; }
            public string Reason { get; set; }
            public string Status { get; set; }

        }
        public ActionResult Get_Employee_History(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emps = db.EmployeeLeave
                  .Where(e => e.Employee.Id == data)
                  .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                  .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                  .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                  .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                  .Include(e => e.Employee.EmpName)
                  .ToList();
                var allLvHead = db.LvHead.ToList();
                List<returnClassGet_Employee_History> ListEmpLvClass = new List<returnClassGet_Employee_History>();
                ListEmpLvClass.Add(new returnClassGet_Employee_History
                {
                    LeaveCalendar = "LeaveCalendar",
                    ReqDate = "Requestion Date",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    ResumeDate = "Resume Date",
                    LvName = "Leave Name",
                    OpenBal = "Open Bal",
                    DebitDays = "Debit Days",
                    CloseBal = "Close Bal",
                    FromStat = "From Stat",
                    ToStat = "To Stat",
                    IsCancel = "Is Cancel",
                    LvOccurances = "Lv Occurances",
                    Reason = "Reason",
                    Status = "Status"
                });
                foreach (var ca in Emps)
                {
                    foreach (var lvhead in allLvHead)
                    {
                        var LvData = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id).OrderByDescending(e => e.ReqDate).ToList();
                        LvData = LvData.OrderByDescending(e => e.FromDate).ToList();
                        foreach (var e in LvData)
                        {
                            var Status = Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault();
                            if (Status.ToUpper() == "SANCTIONED" && e.TrClosed == true)
                            {
                                Status = "Sanctioned and Approved";
                            }
                            else if (Status.ToUpper() == "RECOMMENDATION" && e.TrClosed == true)
                            {
                                Status = "Recomendation and Approved";
                            }
                            ListEmpLvClass.Add(new returnClassGet_Employee_History
                            {
                                LeaveCalendar = "FromDate :" + e.LeaveCalendar.FromDate.Value.ToShortDateString() + " ToDate :" + e.LeaveCalendar.ToDate.Value.ToShortDateString(),
                                ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                                FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
                                ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
                                ResumeDate = e.ResumeDate != null ? e.ResumeDate.Value.ToShortDateString() : null,
                                LvName = e.LeaveHead.LvName,
                                OpenBal = e.OpenBal.ToString(),
                                DebitDays = e.DebitDays.ToString(),
                                CloseBal = e.CloseBal.ToString(),
                                FromStat = e.FromStat != null ? e.FromStat.LookupVal : null,
                                ToStat = e.ToStat != null ? e.ToStat.LookupVal : null,
                                IsCancel = e.IsCancel.ToString(),
                                LvOccurances = e.LvOccurances.ToString(),
                                Reason = e.Reason,
                                //Status = Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault()
                                Status = Status

                            });
                        }
                    }
                }
                return Json(new { data = ListEmpLvClass, status = true }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult Get_Employee_Span_period_History(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var ids = Utility.StringIdsToListString(data);
                var fromdate = Convert.ToDateTime(ids[0]);
                var todate = Convert.ToDateTime(ids[1]);
                List<EmployeeLeave> EmpList = new List<EmployeeLeave>();
                EmpIds.RemoveAll(e => Convert.ToInt32(SessionManager.EmpId) == e);
                foreach (var item in EmpIds)
                {
                    var temp = db.EmployeeLeave
                  .Include(e => e.Employee)
                   .Include(e => e.Employee.EmpName)
                   .Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                   .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                   .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                   .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                   .Where(e => e.Employee.Id == item).FirstOrDefault();
                    if (temp != null)
                    {
                        EmpList.Add(temp);
                    }
                }
                var allLvHead = db.LvHead.ToList();
                List<returnClassGet_Employee_Span_period_History> ListEmpLvClass = new List<returnClassGet_Employee_Span_period_History>();
                ListEmpLvClass.Add(new returnClassGet_Employee_Span_period_History
                {
                    EmpName = "Employee Name",
                    LeaveCalendar = "LeaveCalendar",
                    ReqDate = "Requestion Date",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    ResumeDate = "Resume Date",
                    LvName = "Leave Name",
                    OpenBal = "Open Bal",
                    DebitDays = "Debit Days",
                    CloseBal = "Close Bal",
                    FromStat = "From Stat",
                    ToStat = "To Stat",
                    IsCancel = "Is Cancel",
                    LvOccurances = "Lv Occurances",
                    Reason = "Reason",
                    Status = "Status"
                });
                foreach (var ca in EmpList)
                {
                    foreach (var lvhead in allLvHead)
                    {
                        var LvData = ca.LvNewReq.Where(e => e.LeaveHead.Id == lvhead.Id && (e.FromDate >= fromdate && e.ToDate <= todate))
                            .OrderByDescending(e => e.ReqDate).ToList();
                        LvData = LvData.OrderByDescending(e => e.FromDate).ToList();
                        foreach (var e in LvData)
                        {
                            var Status = Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault();
                            if (Status.ToUpper() == "SANCTIONED" && e.TrClosed == true)
                            {
                                Status = "Sanctioned and Approved";
                            }
                            else if (Status.ToUpper() == "RECOMMENDATION" && e.TrClosed == true)
                            {
                                Status = "Recomendation and Approved";
                            }
                            ListEmpLvClass.Add(new returnClassGet_Employee_Span_period_History
                            {
                                EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML,
                                LeaveCalendar = "FromDate :" + e.LeaveCalendar.FromDate.Value.ToShortDateString() + " ToDate :" + e.LeaveCalendar.ToDate.Value.ToShortDateString(),
                                ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                                FromDate = e.FromDate != null ? e.FromDate.Value.ToShortDateString() : null,
                                ToDate = e.ToDate != null ? e.ToDate.Value.ToShortDateString() : null,
                                ResumeDate = e.ResumeDate != null ? e.ResumeDate.Value.ToShortDateString() : null,
                                LvName = e.LeaveHead.LvName,
                                OpenBal = e.OpenBal.ToString(),
                                DebitDays = e.DebitDays.ToString(),
                                CloseBal = e.CloseBal.ToString(),
                                FromStat = e.FromStat != null ? e.FromStat.LookupVal : null,
                                ToStat = e.ToStat != null ? e.ToStat.LookupVal : null,
                                IsCancel = e.IsCancel.ToString(),
                                LvOccurances = e.LvOccurances.ToString(),
                                Reason = e.Reason,
                                //  Status = Utility.GetStatusName().Where(a => e.LvWFDetails.Count > 0 && a.Key == e.LvWFDetails.LastOrDefault().WFStatus.ToString()).Select(a => a.Value).SingleOrDefault()
                                Status = Status

                            });
                        }
                    }
                }
                return Json(new { data = ListEmpLvClass, status = true }, JsonRequestBehavior.AllowGet);

            }
        }

        //Cancel FloW
        public ActionResult Create_Cancel(LvNewReq LvReq, FormCollection form, String forwarddata, string DebitDays)
        {

            string LvCancellist = form["IsCancel"] == "0" ? "" : form["IsCancel"];
            string LvId = form["LvId"] == "0" ? "" : form["LvId"];
            string Reason = form["ReasonIsCancel"] == "0" ? "" : form["ReasonIsCancel"];
            var LvCancelchk = false;
            var LeaveId = 0;
            if (LvId != null)
            {
                LeaveId = Convert.ToInt32(LvId);
            }
            else
            {
                return Json(new { status = true, responseText = "Try Again" }, JsonRequestBehavior.AllowGet);
            }

            if (LvCancellist != null)
            {
                LvCancelchk = Convert.ToBoolean(LvCancellist);
            }
            else
            {
                return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
            }

            using (DataBaseContext db = new DataBaseContext())
            {

                int EmpId = 0;
                EmpId = int.Parse(SessionManager.UserName);

                var Comp_Id = 0;
                Comp_Id = Convert.ToInt32(Session["CompId"]);
                LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                LvReq.LeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var OEmployeeLeave = db.EmployeeLeave
                   .Where(e => e.Employee.Id == EmpId).SelectMany(e => e.LvNewReq.Where(a => a.LvOrignal.Id == LeaveId &&
                       (a.IsCancel == false || a.TrReject == false))).SingleOrDefault();
                if (OEmployeeLeave != null)
                {
                    return Json(new Object[] { "", "", "Record Already Exits..!" }, JsonRequestBehavior.AllowGet);
                }

                LvNewReq LvNewReq = new LvNewReq()
                {
                    Reason = Reason,
                    IsCancel = true,
                    ReqDate = DateTime.Now,
                    InputMethod = 1,
                    //  LvOrignal = db.LvNewReq.Where(e => e.Id == LvCancelId).SingleOrDefault(),   //uncoment afterwords error code change
                    LeaveCalendar = LvReq.LeaveCalendar,
                    WFStatus = db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),
                };

                if (ModelState.IsValid)
                {


                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 30, 0)))
                    {
                        try
                        {
                            LvNewReq.GeoStruct = db.GeoStruct.Find(OEmployeeLeave.GeoStruct.Id);
                            LvNewReq.PayStruct = db.PayStruct.Find(OEmployeeLeave.PayStruct.Id);
                            LvNewReq.FuncStruct = db.FuncStruct.Find(OEmployeeLeave.FuncStruct.Id);
                            db.LvNewReq.Add(LvNewReq);
                            db.SaveChanges();
                            List<LvNewReq> OFAT = new List<LvNewReq>();
                            OFAT.Add(db.LvNewReq.Find(LvNewReq.Id));

                            if (OEmployeeLeave == null)
                            {
                                EmployeeLeave OTEP = new EmployeeLeave()
                                {
                                    Employee = db.Employee.Find(OEmployeeLeave.Id),
                                    LvNewReq = OFAT,
                                    DBTrack = LvReq.DBTrack
                                };
                                db.EmployeeLeave.Add(OTEP);
                                db.SaveChanges();
                            }
                            else
                            {
                                var aa = db.EmployeeLeave.Find(OEmployeeLeave.Id);
                                OFAT.AddRange(aa.LvNewReq);
                                aa.LvNewReq = OFAT;
                                db.EmployeeLeave.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }
                            ts.Complete();
                            return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

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
                            return Json(new { status = false, responseText = ex.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
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
        }


        public ActionResult Create_CancelReq(LvCancelReq L, FormCollection form, String data) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    //string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
                    //string ContactNolist = form["ContactNolist"] == "0" ? "" : form["ContactNolist"];
                    //                    string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];

                    string LvCancellist = form["IsCancel"] == "0" ? "" : form["IsCancel"];
                    string LvId = form["LvId"] == "0" ? "" : form["LvId"];
                    string Reason = form["ReasonIsCancel"] == "0" ? "" : form["ReasonIsCancel"];
                    var LvCancelchk = false;
                    var LeaveId = 0;
                    if (LvId != null)
                    {
                        LeaveId = Convert.ToInt32(LvId);
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Try Again" }, JsonRequestBehavior.AllowGet);
                    }

                    if (LvCancellist != null)
                    {
                        LvCancelchk = Convert.ToBoolean(LvCancellist);
                        if (LvCancelchk == false)
                        {
                            return Json(new { status = true, responseText = "Set cancel True" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
                    }

                    int EmpId = Convert.ToInt32(SessionManager.EmpId);

                    if (LvId != null && LvId != "")
                    {
                        int id = int.Parse(LvId);
                        var value = db.LvNewReq.Where(e => e.Id == id)
                            .Include(e => e.LeaveCalendar).Include(e => e.LeaveHead).Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct)
                            .FirstOrDefault();

                        L.LvNewReq = value;

                    }
                    var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    var OEmployeeLv = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                        .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                        .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                        .Where(e => e.Employee.Id == EmpId)
                        .SingleOrDefault();
                    var PrevReq = OEmployeeLv.LvNewReq
                        .Where(e => e.LeaveHead.Id == L.LvNewReq.LeaveHead.Id
                        //&& e.IsCancel == false
                            )
                        .OrderByDescending(e => e.Id).FirstOrDefault();

                    //Bhagesh Added Code
                    int lvnewreqid = Convert.ToInt32(data);

                    var qurey = db.LvNewReq
                    .Include(e => e.WFStatus)
                    .Include(e => e.LeaveCalendar)
                    .Include(e => e.LeaveHead)
                    .Include(e => e.LvWFDetails)
                    .Include(e => e.FromStat)
                    .Include(e => e.GeoStruct)
                    .Include(e => e.LvOrignal)
                    .Include(e => e.PayStruct)
                    .Include(e => e.FuncStruct)
                    .Where(e => e.Id == lvnewreqid).SingleOrDefault();

                    //////LvWFDetails oLvWFDetails = new LvWFDetails
                    //////{
                    //////    WFStatus = 6,
                    //////    Comments = L.LvNewReq.Reason,
                    //////    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    //////};
                    //////List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                    //////oLvWFDetails_List.Add(oLvWFDetails);

                    qurey.TrClosed = true;
                    //Bhagesh code end


                    //if (ContactNolist != null && ContactNolist != "")
                    //{
                    //    var value = db.ContactNumbers.Find(int.Parse(ContactNolist));
                    //    L.ContactNo = value;

                    //}
                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Z = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeeLeave.Select(f => f.Employee)).SingleOrDefault();
                    //Employee OEmployee = null;
                    //EmployeeLeave OEmployeePayroll = null;

                    L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    L.Calendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    //LvNewReq oLvNewReq = new LvNewReq()
                    //{
                    //    ReqDate = DateTime.Now,
                    //    ContactNo = L.LvNewReq.ContactNo,
                    //    DebitDays = 0,
                    //    CreditDays = L.LvNewReq.DebitDays,
                    //    FromDate = L.LvNewReq.FromDate,
                    //    FromStat = L.LvNewReq.FromStat,
                    //    LeaveHead = L.LvNewReq.LeaveHead,
                    //    Reason = L.LvNewReq.Reason,
                    //    ResumeDate = L.LvNewReq.ResumeDate,
                    //    ToDate = L.LvNewReq.ToDate,
                    //    ToStat = L.LvNewReq.ToStat,
                    //    LeaveCalendar = L.LvNewReq.LeaveCalendar,
                    //    DBTrack = L.DBTrack,
                    //    OpenBal = PrevReq.CloseBal,
                    //    CloseBal = PrevReq.CloseBal + L.LvNewReq.DebitDays,
                    //    LVCount = PrevReq.LVCount - L.LvNewReq.DebitDays,
                    //    LvOccurances = PrevReq.LvOccurances - 1,
                    //    TrClosed = true,
                    //    TrReject = true,
                    //    LvOrignal = L.LvNewReq,
                    //    GeoStruct = L.LvNewReq.GeoStruct,
                    //    PayStruct = L.LvNewReq.PayStruct,
                    //    FuncStruct = L.LvNewReq.FuncStruct,
                    //    InputMethod = 1,
                    //    IsCancel = true,
                    //    LvWFDetails = oLvWFDetails_List,
                    //    LvCountPrefixSuffix = PrevReq.LvCountPrefixSuffix,
                    //    WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault(), //db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault(),
                    //    Narration = "Leave Cancelled",

                    //};
                    if (ModelState.IsValid)
                    {
                        if (Z != null)
                        {
                            //foreach (var i in Z.EmployeePayroll.Select(e => e.Employee.Id))
                            //{
                            //  var OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                            //              .Where(r => r.Id == EmpId).SingleOrDefault();

                            //  var OEmployeeLeave
                            //  = db.EmployeeLeave
                            //.Where(e => e.Employee.Id == EmpId).Include(e => e.LvNewReq).SingleOrDefault();


                            //using (TransactionScope ts = new TransactionScope())
                            //{
                            try
                            {

                                // ====================================== Api call Start ==============================================
                                var ShowMessageCode = "";
                                var ShowMessage = "";
                                int errorno = 0;
                                ServiceResult<ReturnData_LeaveValidation> responseDeserializeData = new ServiceResult<ReturnData_LeaveValidation>();
                                string APIUrl = ConfigurationManager.AppSettings["APIURL"];

                                using (P2BHttpClient p2BHttpClient = new P2BHttpClient(APIUrl))
                                {
                                    var response = p2BHttpClient.request("ELMS/getUserLvCancelRequest",
                                        new ELMS_Lv_NewRequest()
                                        {
                                            Emp_Id = EmpId,
                                            Lv_Req_Id = L.LvNewReq.Id,
                                            ContactNo_Id = L.ContactNo_Id,
                                            Reason = Reason,
                                            InputMethod = 1

                                        });

                                    var Resdata = response.Content.ReadAsStringAsync().Result;

                                    responseDeserializeData = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResult<ReturnData_LeaveValidation>>(response.Content.ReadAsStringAsync().Result);

                                    if (responseDeserializeData.Data != null)
                                    {


                                        errorno = responseDeserializeData.Data.ErrNo;


                                        int errno = responseDeserializeData.Data.ErrNo;

                                        var oErrorlookup = db.ErrorLookup.Where(e => e.Message_Code == errno).FirstOrDefault();
                                        ShowMessage = errno + ' ' + oErrorlookup.Message_Description.ToString();

                                    }
                                    else
                                    {
                                        errorno = 1;
                                        ShowMessage = responseDeserializeData.Message.ToString();
                                        ShowMessageCode = responseDeserializeData.MessageCode.ToString();
                                    }
                                }


                                // ====================================== Api call End ==============================================



                                //////db.LvNewReq.Add(oLvNewReq);
                                //////db.SaveChanges();
                                //////OEmployeeLv.LvNewReq.Add(oLvNewReq);

                                //List<LvNewReq> OFAT = new List<LvNewReq>();
                                //OFAT.Add(db.LvNewReq.Find(LvNewReq.Id));

                                //if (OEmployeeLeave == null)
                                //{
                                //    EmployeeLeave OTEP = new EmployeeLeave()
                                //    {
                                //        Employee = db.Employee.Find(OEmployee.Id),
                                //        LvNewReq = OFAT,
                                //        DBTrack = L.DBTrack

                                //    };


                                //    db.EmployeeLeave.Add(OTEP);
                                //    db.SaveChanges();
                                //}
                                //else
                                //{
                                //     var aa = db.EmployeeLeave.Find(OEmployeeLeave.Id);
                                //OFAT.AddRange(aa.LvNewReq);
                                //aa.LvNewReq = OFAT;
                                //OEmployeePayroll.DBTrack = dbt;
                                //  db.EmployeeLeave.Attach(OEmployeeLv);
                                ////////db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Modified;
                                ////////db.SaveChanges();
                                ////////db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Detached;
                                //}

                                //var LvReq = db.LvNewReq.Find(L.LvNewReq.Id);
                                //LvReq.IsCancel = true;
                                //db.LvNewReq.Attach(LvReq);
                                //db.Entry(LvReq).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                //db.Entry(LvReq).State = System.Data.Entity.EntityState.Detached;
                                ////////db.LvNewReq.Attach(qurey);//Bhagesh Added code 
                                ////////db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                                ////////db.SaveChanges();
                                ////////db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;//bhagesh code end
                                ////////ts.Complete();
                                ////////return Json(new { status = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);

                                if (responseDeserializeData.Data == null && ShowMessageCode == "OK")
                                {
                                    Msg.Add(ShowMessage);
                                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else
                                {
                                    Msg.Add(ShowMessage);
                                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

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
                                return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                            }
                            // }
                        }
                        Msg.Add("  Unable to create...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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

        public ActionResult Update_Cancel_Status(LvNewReq LvReq, FormCollection form, String data)
        {
            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            var ids = Utility.StringIdsToListString(data);
            var lvnewreqid = Convert.ToInt32(ids[0]);
            var EmpLvId = Convert.ToInt32(ids[1]);
            string Sanction = form["Sanction"] == null ? "false" : form["Sanction"];
            string Approval = form["Approval"] == null ? "false" : form["Approval"];
            string HR = form["HR"] == null ? null : form["HR"];
            string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];

            string ReasonSanction = form["ReasonSanction"] == null ? null : form["ReasonSanction"];
            string ReasonApproval = form["ReasonApproval"] == null ? null : form["ReasonApproval"];
            string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.LvNewReq
                    .Include(e => e.WFStatus)
                    .Include(e => e.LeaveCalendar)
                    .Include(e => e.LeaveHead)
                    .Include(e => e.LvWFDetails)
                    .Include(e => e.FromStat)
                    .Include(e => e.GeoStruct)
                    .Include(e => e.LvOrignal)
                    .Include(e => e.PayStruct)
                    .Include(e => e.FuncStruct)
                    .Where(e => e.Id == lvnewreqid).SingleOrDefault();

                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                if (authority.ToUpper() == "MYSELF")
                {
                    qurey.Reason = ReasonMySelf;
                    qurey.IsCancel = true;
                    qurey.TrClosed = true;
                    qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                }
                if (authority.ToUpper() == "SANCTION")
                {
                    if (Convert.ToBoolean(Sanction) == true)
                    {
                        //sanction yes -1
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 1,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                        qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                    }
                    else if (Convert.ToBoolean(Sanction) == false)
                    {
                        //sanction no -2
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 2,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault(); // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        qurey.TrClosed = true;
                        SanctionRejected = true;
                    }
                }
                else if (authority.ToUpper() == "APPROVAL")//Hr
                {
                    if (Convert.ToBoolean(Approval) == true)
                    {
                        //approval yes-3
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 3,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                    }
                    else if (Convert.ToBoolean(Approval) == false)
                    {
                        //approval no-4
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 4,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();  //db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                        qurey.TrClosed = true;
                        HrRejected = true;
                    }
                }
                else if (authority.ToUpper() == "RECOMMENDATION")
                {

                }
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if someone reject lv
                        if (SanctionRejected == true || HrRejected == true)
                        {
                            var OEmployeeLv = db.EmployeeLeave
                                  .Where(e => e.Id == EmpLvId)
                                  .Select(e => new { EmpId = e.Id, LvNewReq = e.LvNewReq })
                                  .SingleOrDefault();

                            var LvNewReq = OEmployeeLv.LvNewReq.Where(e => e.Id == lvnewreqid).FirstOrDefault();
                            LvNewReq.TrClosed = true;
                            try
                            {
                                db.Entry(LvNewReq).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(LvNewReq).State = System.Data.Entity.EntityState.Detached;
                                return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

                            }
                            catch (Exception e)
                            {
                                return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        var ogLvNewReq = db.LvNewReq.Where(e => e.Id == lvnewreqid).FirstOrDefault();
                        var CreditDays = ogLvNewReq.LvOrignal.DebitDays;
                        ogLvNewReq.CreditDays = CreditDays;
                        db.LvNewReq.Attach(qurey);
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class P2BCrGridData
        {
            public string RequisitionDate { get; set; }
            public string LeaveHead { get; set; }
            public string FromDate { get; set; }
            public string Todate { get; set; }
            public string Status { get; set; }
        }

        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;
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
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeLeave
                      .Where(e => e.Id == Id)
                      .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                      .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                      .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                     .SingleOrDefault();

                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();

                var LvOrignal_id = db_data.LvNewReq.Where(e => e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();
                var AntCancel = db_data.LvNewReq.OrderBy(e => e.Id).ToList();
                var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id)).OrderBy(e => e.Id).ToList();
                if (listLvs != null && listLvs.Count() > 0)
                {
                    foreach (var item in listLvs.Where(e => e.Narration != "Leave Encash Payment" && e.LeaveCalendar.Id != LvCalendar.Id && e.WFStatus != null && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0").OrderByDescending(a => a.ReqDate).ToList())
                    {
                        var FromDate = item.FromDate != null ? item.FromDate.Value.ToShortDateString() : null;
                        var ToDate = item.ToDate != null ? item.ToDate.Value.ToShortDateString() : null;
                        var Status = "--";
                        if ((item.InputMethod == 1 || item.InputMethod == 2) && item.LvWFDetails.Count > 0)
                        {
                            Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                            .Select(e => e.Value).SingleOrDefault();
                            if (Status.ToUpper() == "SANCTIONED" && item.TrClosed == true)
                            {
                                Status = "Sanctioned and Approved";
                            }
                            else if (Status.ToUpper() == "RECOMMENDATION" && item.TrClosed == true)
                            {
                                Status = "Recomendation and Approved";
                            }
                        }
                        if (item.InputMethod == 0)
                        {
                            Status = "Approved By HRM (M)";
                        }
                        view = new P2BCrGridData()
                        {
                            RequisitionDate = item.ReqDate.Value.ToShortDateString(),
                            LeaveHead = item.LeaveHead.LvName,
                            FromDate = item.FromDate == null ? "" : item.FromDate.Value.ToShortDateString(),
                            Todate = item.ToDate == null ? "" : item.ToDate.Value.ToShortDateString(),
                            Status = Status
                        };

                        model.Add(view);
                    }


                }

                EmpList = model;

                IEnumerable<P2BCrGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "RequisitionDate")
                            jsonData = IE.Select(a => new { a.RequisitionDate, a.LeaveHead, a.FromDate, a.Todate, a.Status }).Where((e => (e.RequisitionDate.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "LeaveHead")
                            jsonData = IE.Select(a => new { a.RequisitionDate, a.LeaveHead, a.FromDate, a.Todate, a.Status }).Where((e => (e.LeaveHead.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "Status")
                            jsonData = IE.Select(a => new { a.RequisitionDate, a.LeaveHead, a.FromDate, a.Todate, a.Status }).Where((e => (e.Status.ToString().Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.RequisitionDate, a.LeaveHead, a.FromDate, a.Todate, a.Status }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<P2BCrGridData, dynamic> orderfuc;
                    if (gp.sidx == "RequisitionDate")
                    {
                        orderfuc = (c => gp.sidx == "RequisitionDate" ? c.RequisitionDate.ToString() : "");
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "RequisitionDate" ? c.RequisitionDate.ToString() :
                                         gp.sidx == "LeaveHead" ? c.LeaveHead.ToString() :
                                         gp.sidx == "FromDate" ? c.FromDate.ToString() :
                                         gp.sidx == "Todate" ? c.Todate.ToString() :
                                         gp.sidx == "Status" ? c.Status.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.RequisitionDate, Convert.ToString(a.LeaveHead), a.FromDate, a.Todate, a.Status }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.RequisitionDate, Convert.ToString(a.LeaveHead), a.FromDate, a.Todate, a.Status }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.RequisitionDate, Convert.ToString(a.LeaveHead), a.FromDate, a.Todate, a.Status }).ToList();
                    }
                    totalRecords = EmpList.Count();
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
        public string InvestmentUploadFile(string FolderName)
        {
            string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\Images\\" + FolderName + "\\";
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
        public ActionResult LeaveCertificateUpload(HttpPostedFileBase[] files, FormCollection form, string data)
        {
            if (ModelState.IsValid)
            {
                string Id = form["EmpLvnereq_Id"] == null ? null : form["EmpLvnereq_Id"];
                string EmpCode = "";
                string extension, newfilename, deletefilepath = "";
                Int32 Count = 0;
                string LvHeadList = form["LvHeadlist"] == "0" ? "" : form["LvHeadlist"];
                string LvCode = "";
                string FromDate = form["FromDate"] == "0" ? "" : form["FromDate"];
                string ReqDate = DateTime.Now.ToShortDateString();
                string ReqTime = DateTime.Now.ToShortTimeString();
                string s1 = "";
                string s2 = "";
                if (ReqTime != null)
                {
                    string[] values = (ReqTime.Split(new string[] { ":" }, StringSplitOptions.None));
                    s1 = values.ElementAtOrDefault(0);
                    s2 = values.ElementAtOrDefault(1);
                }

                int Empid = int.Parse(Id);
                using (DataBaseContext db = new DataBaseContext())
                {
                    if (LvHeadList != null && LvHeadList != "")
                    {
                        var val = db.LvHead.Find(int.Parse(LvHeadList));
                        LvCode = val.LvCode;
                    }

                    if (Id != null)
                    {
                        int itid = Convert.ToInt32(Id);
                        EmpCode = db.Employee.Find(itid).EmpCode;
                    }

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
                    string Module_Name = Convert.ToString(Session["user-module"]);
                    string ModuleName = Module_Name.ToUpper();

                    foreach (HttpPostedFileBase file in files)
                    {
                        if (file != null)
                        {

                            extension = Path.GetExtension(file.FileName);
                            newfilename = LvCode + "-" + FromDate.Replace("/", "") + "-" + ReqDate.Replace("/", "") + "-" + s1 + "-" + s2 + extension;
                            String FolderName = Empid + "\\" + ModuleName + "\\" + "LeaveCertificate";

                            //var InputFileName = Path.GetFileName(file.FileName);
                            string ServerSavePath = ConfigurationManager.AppSettings["EmployeeDocuments"];
                            if (ServerSavePath == null)
                            {
                                return Json(new { success = false, responseText = "Please contact the admin to define the document path." }, JsonRequestBehavior.AllowGet);
                            }
                            string ServerMappath = ServerSavePath + FolderName + "\\"+ newfilename;
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
                            Session["IsCertAppl"] = true;
                            //if (Id != null)
                            //{
                            //    db.LvNewReq.Attach(lvnewreqcertificate);
                            //    db.Entry(lvnewreqcertificate).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //    TempData["RowVersion"] = lvnewreqcertificate.RowVersion;
                            //    db.Entry(lvnewreqcertificate).State = System.Data.Entity.EntityState.Detached;
                            //    lvnewreqcertificate.DBTrack = new DBTrack
                            //    {
                            //        CreatedBy = lvnewreqcertificate.DBTrack.CreatedBy == null ? null : lvnewreqcertificate.DBTrack.CreatedBy,
                            //        CreatedOn = lvnewreqcertificate.DBTrack.CreatedOn == null ? null : lvnewreqcertificate.DBTrack.CreatedOn,
                            //        Action = "M",
                            //        ModifiedBy = SessionManager.UserName,
                            //        ModifiedOn = DateTime.Now
                            //    };
                            //    LvNewReq ContactDet = lvnewreqcertificate;
                            //    ContactDet.Path = ServerMappath;
                            //    ContactDet.DBTrack = lvnewreqcertificate.DBTrack;

                            //    db.Entry(ContactDet).State = System.Data.Entity.EntityState.Modified;
                            //    db.Entry(ContactDet).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //    db.SaveChanges();
                            //}
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
                    var listLvschk = AntCancelchk.Where(e => !LvOrignal_idchk.Contains(e.Id) && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.Path=="").OrderBy(e => e.Id).ToList();


                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
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

                    string requiredPathchk = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
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

        [HttpPost]
        public ActionResult Filename(string filepath)
        {
            if (filepath != null && filepath != "")
            {

                return Json(new { data = filepath }, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public ActionResult CheckUploadFile(string filepath)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                if (filepath != null)
                {
                    localpath = filepath;

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

                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);
                if (exists)
                {
                    return Json(new { success = true, fileextension = extension }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, responseText = "File Not Uploaded..!" }, JsonRequestBehavior.AllowGet);

                }
            }
            return null;
        }

        public ActionResult Imageviewr()
        {
            return View("~/Views/Shared/_ImageViewer.cshtml");
            //D:\LATESTCHECKOUT\P2bUltimate\P2BUltimate\Views\Shared\_Upload.cshtml
        }

        public ActionResult GetCompImage(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string localpath = "";

                if (data != null)
                {
                    localpath = data;

                    if (localpath != null)
                    {
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


                }
                else
                {
                    return View("File Not Found");
                    //return Content("File Not Found");
                    //return Json(new { success = false, responseText = "File Not Found..!" }, JsonRequestBehavior.AllowGet);
                }

                FileInfo file = new FileInfo(localpath);
                bool exists = file.Exists;
                string extension = Path.GetExtension(file.Name);

                if (exists)
                {
                    if (extension.ToUpper() == ".PDF")
                    {
                        return File(file.FullName, "application/pdf", file.Name + " ");


                        //string pdf="pdf";
                        //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");

                        //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = pdf }, JsonRequestBehavior.AllowGet);
                    }
                    if (extension.ToUpper() == ".JPG")
                    {
                        // return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                    if (extension.ToUpper() == ".PNG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

                    }
                    if (extension.ToUpper() == ".JPEG")
                    {
                        //return File(file.FullName, "image/png", file.Name + " ");
                        byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
                        string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                        //return Json(new { data = base64ImageRepresentation, status = extension }, JsonRequestBehavior.AllowGet);

                        JsonRequestBehavior behaviou = new JsonRequestBehavior();
                        return new JsonResult()
                        {
                            Data = base64ImageRepresentation,
                            MaxJsonLength = 86753090,
                            JsonRequestBehavior = behaviou
                        };

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

        //public ActionResult GetCompImage(string filepath)
        //{
        //    //string id = form["HiddenInvestmentid"] == null ? null : form["HiddenInvestmentid"];
        //    //string SubId = form["HiddenSubinvestment_Id"] == null ? null : form["HiddenSubinvestment_Id"];
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        if (filepath != null && filepath != "")
        //        {

        //            FileInfo file = new FileInfo(filepath);
        //            bool exists = file.Exists;
        //            string extension = Path.GetExtension(file.Name);

        //            if (exists)
        //            {
        //                if (extension == ".pdf")
        //                {
        //                    //return File(file.FullName, "application/pdf", file.Name + " ");

        //                    return File(file.FullName, "application/force-download", Path.GetFileName(file.Name));
        //                }
        //                if (extension == ".jpg")
        //                {
        //                    return File(file.FullName, "image/png", file.Name + " ");
        //                    //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
        //                    //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
        //                    //return Json(new { data = base64ImageRepresentation, status = true }, JsonRequestBehavior.AllowGet);
        //                }
        //                if (extension == ".png")
        //                {
        //                    return File(file.FullName, "image/png", file.Name + " ");
        //                    //byte[] imageArray = System.IO.File.ReadAllBytes("" + file + "");
        //                    //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
        //                    //return Json(new { data = base64ImageRepresentation, status = true }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                return Content("File Not Found");
        //                //return Json(new { success = false, responseText = "File Uploaded..!" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        return null;
        //    }


        //}
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
        [HttpPost]
        public ActionResult CreateBatchProc(List<GetLvNewReqClass2> data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();

                try
                {
                    LvNewReq lvnewReq = new LvNewReq();



                    foreach (GetLvNewReqClass2 item in data)
                    {

                        using (TransactionScope ts = new TransactionScope())
                        {

                            try
                            {
                                string EmpCode = item.Emp.ToString().Split(' ')[0];
                                var LvCalendarFilter = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.LvNewReq)
                                    .Include(e => e.LvNewReq.Select(t => t.LeaveHead)).Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                    .Include(e => e.LvNewReq.Select(t => t.WFStatus)).Include(e => e.LvNewReq.Select(r => r.LvWFDetails))
                                    .Where(e => e.Employee.EmpCode == EmpCode).FirstOrDefault().LvNewReq.OrderBy(e => e.Id).ToList();
                                //var LvCalendarFilter = OEmpLv.LvNewReq.OrderBy(e => e.Id).ToList();

                                var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();
                                var AntCancel = LvCalendarFilter.Where(e => e.IsCancel == false && e.TrReject == false).OrderBy(e => e.Id).ToList();
                                var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id) && e.WFStatus != null && e.WFStatus.LookupVal == "1").OrderBy(e => e.Id).ToList();

                                DateTime FromDate = Convert.ToDateTime(item.FromDate);
                                DateTime ToDate = Convert.ToDateTime(item.ToDate);
                                LvNewReq OLvNewReq = listLvs.Where(e => e.FromDate.Value == FromDate && e.ToDate.Value == ToDate && (e.LeaveHead != null && e.LeaveHead.LvName == item.LvHead) && e.Narration.ToUpper() == "LEAVE REQUISITION").FirstOrDefault();
                                LvWFDetails LvWFDetails = new LvWFDetails
                                {
                                    WFStatus = 3,
                                    Comments = "OK-AutoApproval",
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                };

                                if (OLvNewReq != null)
                                {
                                    OLvNewReq.TrClosed = true;
                                    OLvNewReq.LvWFDetails.Add(LvWFDetails);
                                    //OLvNewReq.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();

                                }

                                db.LvNewReq.Attach(OLvNewReq);
                                db.Entry(OLvNewReq).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(OLvNewReq).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();

                                // return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });

                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = lvnewReq.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
                            }
                        }
                    }

                    List<string> Msgs = new List<string>();
                    Msgs.Add("Data Saved successfully");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception e)
                {
                    MSG.Add(e.InnerException.Message.ToString());
                    return Json(MSG, JsonRequestBehavior.AllowGet);

                }

            }
        }
    }
}