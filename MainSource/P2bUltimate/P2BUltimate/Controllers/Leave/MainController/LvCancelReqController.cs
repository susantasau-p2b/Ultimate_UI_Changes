using P2b.Global;
using P2BUltimate;
using P2BUltimate.App_Start;
using P2BUltimate.Controllers;
using P2BUltimate.Models;
using Payroll;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Leave;
using P2BUltimate.Security;
using System.Diagnostics;
using P2B.MOBILE;
using System.Configuration;
using P2B.API.Models;
using P2B.UTILS;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvCancelReqController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);
            return View("~/Views/Leave/MainViews/LvCancelReq/Index.cshtml");
        }

        public class LvNewReqData
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

        }

        public ActionResult PopulateLvnewReqDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Id = Convert.ToInt32(data2);
                var ca = new SelectList((from lv in db.EmployeeLeave.Include(e => e.LvNewReq).Where(e => e.Employee.Id == Id).ToList()
                                         from p in lv.LvNewReq
                                         select new
                                         {
                                             Id = p.Id,
                                             Fulldetails = "Debit Days: " + p.DebitDays + "  " + "FromDate: " + p.FromDate.Value.ToShortDateString() + "  " + "ToDate: " + p.ToDate.Value.ToShortDateString()
                                         }


                                             ).Distinct(),

                                              "Id",
                              "Fulldetails",
                              null
                                             );

                return Json(ca, JsonRequestBehavior.AllowGet);


            }
        }
        public ActionResult GetLVNewReqDetails(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                int Count = 0;
                List<LvNewReqData> model = new List<LvNewReqData>();
                LvNewReqData view = null;

                var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                                      //  .Include(e => e.LvCancelReq)
                                // .Include(e => e.LvCancelReq.Select(t => t.WFStatus))
                               //.Include(e => e.LvCancelReq.Select(t => t.LvNewReq))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                    .Include(e => e.LvNewReq.Select(r => r.LvOrignal))
                    .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                  .Where(e => e.Employee.Id == Id).SingleOrDefault();

              //  var lvcancelreqidslist = OEmployeeLeave.LvNewReq.Where(e => e.WFStatus.LookupVal == "0" || e.WFStatus.LookupVal == "1" && e.LvOrignal!=null).Select(e => e.LvOrignal.Id).ToList();
                var lvcancelreqidslist = OEmployeeLeave.LvNewReq.Where(e => e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();
               // var DefCal = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //Cl max date
                List<int> lvcode = OEmployeeLeave.LvNewReq.Where(e=>e.LvCreditDate!=null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();

                foreach (var item in lvcode)
                {
                    DateTime? lvcrdate = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                    if (lvcrdate != null)
                    {
                        DateTime? Lvyearfrom = lvcrdate;
                        DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                        LvyearTo = LvyearTo.Value.AddYears(1);

                        //var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == DefCal.Id && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2").ToList();

                        //var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveCalendar.Id == DefCal.Id && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null).ToList();
                     //   var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => !lvcancelreqidslist.Contains(e.Id) && e.LeaveHead.Id == item && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2" && e.ReqDate >= Lvyearfrom.Value && e.ReqDate<=LvyearTo.Value).ToList();
                        var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => !lvcancelreqidslist.Contains(e.Id) && e.LeaveHead.Id == item && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2" && e.LeaveCalendar.Id == LvCalendar.Id ).ToList();

                       // var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null && e.ReqDate >= Lvyearfrom.Value && e.ReqDate <= LvyearTo.Value).ToList();
                        var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && e.IsCancel == true && e.TrClosed == true && e.LvOrignal != null && e.LeaveCalendar.Id ==LvCalendar.Id).ToList();


                        foreach (var a in OEmpLeave)
                        {
                            //if (query.Count > 0)
                            //{
                            Count = 0;
                            foreach (var b in query)
                            {
                                if (b.LvOrignal.Id == a.Id)
                                {
                                    Count = 1;
                                }
                            }
                            if (Count == 0)
                            {

                                view = new LvNewReqData()
                                {
                                    Id = a.Id,
                                    FullDetails = a.FullDetails
                                };
                                model.Add(view);
                            }
                            //}
                            //else
                            //{
                            //    view = new LvNewReqData()
                            //    {
                            //        Id = a.Id,
                            //        FullDetails = a.FullDetails
                            //    };
                            //    model.Add(view);
                            //}
                        }

                    }
                }

                var selected = "";
                if (data2 != null)
                {
                    selected = data2;
                }
                if (model != null && model.Count() > 0)
                {
                    SelectList s = new SelectList(model, "Id", "Fulldetails", selected);
                    return Json(new { status = true, data = s }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GridPartial()
        {
            return View();
        }
        public ActionResult GridEditData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvCancelReq

                 .Include(e => e.ContactNo)

                 .Include(e => e.LvNewReq)
                 .Where(e => e.Id == data).AsEnumerable()
                 .Select(e => new
                 {

                     ReqDate = e.ReqDate != null ? e.ReqDate.Value.ToShortDateString() : null,
                     LvNewReq = e.LvNewReq != null ? e.LvNewReq.FullDetails : null,
                     Reason = e.Reason,
                 }).SingleOrDefault();
                return Json(Q, JsonRequestBehavior.AllowGet);
            }
        }



        public class lvcancelreqclass
        {
            public string ContactNo_Id { get; set; }
            public string Contact_Fulldtl { get; set; }
            public string LvWFDetails_Id { get; set; }
            public string LvWFDetails_val { get; set; }
            public string LvNewReq_Id { get; set; }
            public string LvNewReq_val { get; set; }
        }
        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvCancelReq

                    .Include(e => e.ContactNo)

                    .Include(e => e.LvNewReq)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {

                        ReqDate = e.ReqDate,
                        ContactNo_Id = e.ContactNo == null ? 0 : e.ContactNo.Id,
                        Contact_Fulldtl = e.ContactNo.FullContactNumbers == null ? "" : e.ContactNo.FullContactNumbers,
                        LvNewReq_Id = e.LvNewReq.Id == null ? 0 : e.LvNewReq.Id,
                        LvNewReq_val = e.LvNewReq.FullDetails == null ? "" : e.LvNewReq.FullDetails,
                        Reason = e.Reason,
                        Action = e.DBTrack.Action
                    }).ToList();

                var W = db.DT_LvCancelReq
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         InputMethod = e.InputMethod,
                         Reason = e.Reason == null ? "" : e.Reason,
                         ReqDate = e.ReqDate.ToString(),
                         Contact_Val = e.ContactNo_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactNo_Id).Select(x => x.FullContactDetails).FirstOrDefault(),
                         //LeaveWF_Val = e.LvWFDetails_Id == 0 ? "" : db.LvWFDetails.Where(x => x.Id == e.ContactNo_Id).Select(x => x.FullDetails).FirstOrDefault()
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvCancelReq.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }
        public class condtlnoDetails
        {
            public int conDtl_Id { get; set; }
            public string conDtl_val { get; set; }


        }


        public class ELMS_Lv_NewRequest
        {
            public int Emp_Id { get; set; }
            //public int EmployeeLeave_Id { get; set; }
            public string Emp_Code { get; set; }

            public int? Lv_Head_Id { get; set; }
            public string Lv_Head { get; set; }
            public int? LeaveCalendar_Id { get; set; }
            public int? InputMethod { get; set; }//through ESS or Main software

            public DateTime? ReqDate { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            //public DateTime? ResumeDate { get; set; }
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


        public ActionResult Create(LvCancelReq L, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
                    string ContactNolist = form["ContactNolist"] == "0" ? "" : form["ContactNolist"];

                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];


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

                    if (LvNewReqlist != null && LvNewReqlist != "")
                    {
                        int id = int.Parse(LvNewReqlist);
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
                        //.Where(e => e.LeaveHead.Id == L.LvNewReq.LeaveHead.Id && e.LeaveCalendar.Id == LvCalendar.Id
                       .Where(e => e.LeaveHead.Id == L.LvNewReq.LeaveHead.Id 
                        //&& e.IsCancel == false
                            )
                        .OrderByDescending(e => e.Id).FirstOrDefault();


                    if (ContactNolist != null && ContactNolist != "")
                    {
                        var value = db.ContactNumbers.Find(int.Parse(ContactNolist));
                        L.ContactNo = value;

                    }
                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Z = db.CompanyLeave.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeeLeave.Select(f => f.Employee)).SingleOrDefault();
                    //Employee OEmployee = null;
                    //EmployeeLeave OEmployeePayroll = null;

                    L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    L.Calendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    //LvNewReq oLvNewReq = new LvNewReq()
                    //{
                    //    ReqDate = L.ReqDate,
                    //    ContactNo = L.LvNewReq.ContactNo,
                    //    DebitDays = 0,
                    //    CreditDays = L.LvNewReq.DebitDays,
                    //    FromDate = L.LvNewReq.FromDate,
                    //    FromStat = L.LvNewReq.FromStat,
                    //    LeaveHead = L.LvNewReq.LeaveHead,
                    //    Reason = L.Reason,
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
                    //    LvOrignal = L.LvNewReq,
                    //    GeoStruct = L.LvNewReq.GeoStruct,
                    //    PayStruct = L.LvNewReq.PayStruct,
                    //    FuncStruct = L.LvNewReq.FuncStruct,
                    //    IsCancel = true,
                    //    LvCountPrefixSuffix = PrevReq.LvCountPrefixSuffix,
                    //    WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "7").FirstOrDefault(),  //db.LookupValue.Where(e => e.LookupVal == "7").SingleOrDefault(),
                    //    Narration = "Leave Cancelled"

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


                            //////using (TransactionScope ts = new TransactionScope())
                            //////{
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
                                        var response = p2BHttpClient.request("ELMS/getUserLvCancelRequestHRMS",
                                            new ELMS_Lv_NewRequest()
                                            {
                                                Emp_Id = EmpId,
                                                Lv_Req_Id= L.LvNewReq.Id,
                                                ContactNo_Id = L.ContactNo_Id,
                                                Reason = L.Reason,
                                                InputMethod = 0

                                            });

                                        var data = response.Content.ReadAsStringAsync().Result;

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
                                    //////db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Modified;
                                    //////db.SaveChanges();
                                    //////db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Detached;
                                    //}

                                    //var LvReq = db.LvNewReq.Find(L.LvNewReq.Id);
                                    //LvReq.IsCancel = true;
                                    //db.LvNewReq.Attach(LvReq);
                                    //db.Entry(LvReq).State = System.Data.Entity.EntityState.Modified;
                                    //db.SaveChanges();
                                    //db.Entry(LvReq).State = System.Data.Entity.EntityState.Detached;
                                    //////ts.Complete();
                                    //////Msg.Add("  Data Saved successfully  ");
                                    //////return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                    //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                                    if (responseDeserializeData.Data == null && ShowMessageCode == "OK")
                                    {
                                        Msg.Add(ShowMessage);
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        Msg.Add(ShowMessage);
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //public ActionResult Create(LvCancelReq c, FormCollection form)
        //{

        //    string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
        //    string ContactNolist = form["ContactNolist"] == "0" ? "" : form["ContactNolist"];

        //    if(LvNewReqlist!=null && LvNewReqlist !="")
        //    {
        //        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
        //        c.LvNewReq = value;

        //    }
        //    if (ContactNolist != null && ContactNolist != "")
        //    {
        //        var value = db.ContactNumbers.Find(int.Parse(ContactNolist));
        //        c.ContactNo = value;

        //    }

        //    if (ModelState.IsValid)
        //    {

        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            //if (db.LvCancelReq.Any(o => o.InputMethod == c.InputMethod))
        //            //{
        //            //    return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
        //            //}

        //            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //            LvCancelReq lvcanreq = new LvCancelReq()
        //            {

        //                ReqDate = c.ReqDate,
        //                Reason = c.Reason,
        //                ContactNo = c.ContactNo,
        //                LvNewReq=c.LvNewReq,

        //                DBTrack = c.DBTrack
        //            };
        //            try
        //            {
        //                db.LvCancelReq.Add(lvcanreq);
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, c.DBTrack);
        //                DT_LvCancelReq DT_Corp = (DT_LvCancelReq)rtn_Obj;
        //                DT_Corp.ContactNo_Id = c.ContactNo == null ? 0 : c.ContactNo.Id;
        //                //DT_Corp.LvWFDetails_Id = c.LvWFDetails.Id == null ? 0 : c.LvWFDetails.Id;
        //                db.Create(DT_Corp);
        //                db.SaveChanges();
        //                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


        //                ts.Complete();
        //                return this.Json(new Object[] { lvcanreq.Id, lvcanreq.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        //return this.Json(new { msg = errorMsg });
        //    }
        //}

        public ActionResult GetLookupContactNo(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ContactNumbers.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ContactNumbers.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list2 = fall.ToList();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullContactNumbers }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLookupLvWFDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvWFDetails.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LvWFDetails.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var list2 = fall.ToList();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class LeaveCancelRequestLvdetails
        {
            public string id { get; set; }
            public string wfstatus { get; set; }
            public string comments { get; set; }
            public string lvdetailsfull { get; set; }

        }
        public class LvWFDetails
        {
            public Array LvWFDetails_Id { get; set; }
            public Array LvWFDetails_val { get; set; }


        }

        /*------------------------------------Edit save ------------------------------------*/

        [HttpPost]
        public async Task<ActionResult> EditSave(LvCancelReq L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
                    string ContactNolist = form["ContactNolist"] == "0" ? "" : form["ContactNolist"];


                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvCancelReq blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvCancelReq.Where(e => e.Id == data).Include(e => e.LvNewReq)
                                                                .Include(e => e.ContactNo).SingleOrDefault();

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

                                    if (LvNewReqlist != null)
                                    {
                                        if (LvNewReqlist != "")
                                        {
                                            var val = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                                            L.LvNewReq = val;

                                            var type = db.LvCancelReq.Include(e => e.LvNewReq).Where(e => e.Id == data).SingleOrDefault();
                                            IList<LvCancelReq> typedetails = null;
                                            if (type.WFStatus != null)
                                            {
                                                typedetails = db.LvCancelReq.Where(x => x.LvNewReq.Id == type.LvNewReq.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.LvCancelReq.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.LvNewReq = L.LvNewReq;
                                                db.LvCancelReq.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var NewReqTypeDetails = db.LvCancelReq.Include(e => e.LvNewReq).Where(x => x.Id == data).ToList();
                                            foreach (var s in NewReqTypeDetails)
                                            {
                                                s.LvNewReq = null;
                                                db.LvCancelReq.Attach(s);
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
                                        var NewReqTypeDetails = db.LvCancelReq.Include(e => e.LvNewReq).Where(x => x.Id == data).ToList();
                                        foreach (var s in NewReqTypeDetails)
                                        {
                                            s.LvNewReq = null;
                                            db.LvCancelReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (ContactNolist != null)
                                    {
                                        if (ContactNolist != "")
                                        {
                                            var val = db.ContactNumbers.Find(int.Parse(ContactNolist));
                                            L.ContactNo = val;

                                            var type = db.LvCancelReq.Include(e => e.ContactNo).Where(e => e.Id == data).SingleOrDefault();
                                            IList<LvCancelReq> typedetails = null;
                                            if (type.ContactNo != null)
                                            {
                                                typedetails = db.LvCancelReq.Where(x => x.ContactNo.Id == type.ContactNo.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.LvCancelReq.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.ContactNo = L.ContactNo;
                                                db.LvCancelReq.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var ContactNoTypeDetails = db.LvCancelReq.Include(e => e.ContactNo).Where(x => x.Id == data).ToList();
                                            foreach (var s in ContactNoTypeDetails)
                                            {
                                                s.ContactNo = null;
                                                db.LvCancelReq.Attach(s);
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
                                        var ContactNoTypeDetails = db.LvCancelReq.Include(e => e.ContactNo).Where(x => x.Id == data).ToList();
                                        foreach (var s in ContactNoTypeDetails)
                                        {
                                            s.ContactNo = null;
                                            db.LvCancelReq.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    var CurCorp = db.LvCancelReq.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        LvCancelReq LvCreditPolicy = new LvCancelReq()
                                        {
                                            ReqDate = L.ReqDate,
                                            Reason = L.Reason,
                                            ContactNo = L.ContactNo,
                                            LvNewReq = L.LvNewReq,

                                            Id = data,
                                            DBTrack = L.DBTrack
                                        };
                                        db.LvCancelReq.Attach(LvCreditPolicy);
                                        db.Entry(LvCreditPolicy).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(LvCreditPolicy).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    }
                                    // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        DT_LvCancelReq DT_Corp = (DT_LvCancelReq)obj;
                                        DT_Corp.LvNewReq_Id = blog.LvNewReq == null ? 0 : blog.LvNewReq.Id;
                                        DT_Corp.ContactNo_Id = blog.ContactNo == null ? 0 : blog.ContactNo.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.ReqDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.ReqDate, "Record Updated", JsonRequestBehavior.AllowGet });

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
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvCancelReq blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvCancelReq Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvCancelReq.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LvCancelReq corp = new LvCancelReq()
                            {
                                ReqDate = L.ReqDate,
                                Reason = L.Reason,
                                ContactNo = L.ContactNo,
                                LvNewReq = L.LvNewReq,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvCancelReq", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvCancelReq.Where(e => e.Id == data).Include(e => e.ContactNo).Include(e => e.LvNewReq)
                                                           .SingleOrDefault();
                                DT_LvCancelReq DT_Corp = (DT_LvCancelReq)obj;
                                DT_Corp.ContactNo_Id = blog.ContactNo == null ? 0 : blog.ContactNo.Id;
                                DT_Corp.LvNewReq_Id = blog.LvNewReq == null ? 0 : blog.LvNewReq.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvCancelReq.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.ReqDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.ReqDate, "Record Updated", JsonRequestBehavior.AllowGet });
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
                return View();
            }

        }


        /* -------------------------------- Delete ------------------------------------ */

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvCancelReq corporates = db.LvCancelReq.Include(e => e.LvNewReq)
                                                       .Include(e => e.ContactNo)
                                                       .Where(e => e.Id == data).SingleOrDefault();

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
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, corporates.DBTrack);
                            DT_LvCancelReq DT_Corp = (DT_LvCancelReq)rtn_Obj;
                            //DT_Corp.Venue_Id = corporates.Venue == null ? 0 : corporates.Venue.Id;
                            //DT_Corp.City_Id = corporates.City == null ? 0 : corporates.City.Id;
                            //DT_Corp.Expenses_Id = corporates.Expenses == null ? 0 : corporates.Expenses.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        //var selectedRegions = corporates.Session;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Session.Select(e => e.Id));
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
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, dbT);
                                DT_LvCancelReq DT_Corp = (DT_LvCancelReq)rtn_Obj;
                                //DT_Corp.Expenses_Id = add == null ? 0 : add.Id;
                                //DT_Corp.City_Id = val == null ? 0 : val.Id;
                                //DT_Corp.Venue_Id = conDet == null ? 0 : conDet.Id;

                                db.Create(DT_Corp);

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
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        public class LvCancelReqChildDataClass
        {
            public int Id { get; set; }
            public string ReqDate { get; set; }
            public string LvNewReq { get; set; }
            public string Reason { get; set; }
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
                                                                sortindex == 1 ? c.Employee.EmpCode :

                                                                "");
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
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : "",
                                JoiningDate = item.Employee.ServiceBookDates != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : "",
                                Job = item.Employee.FuncStruct != null ? item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : "" : "",
                                Grade = item.Employee.PayStruct != null ? item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : "" : "",
                                Location = item.Employee.GeoStruct != null ? item.Employee.GeoStruct.Location != null ? item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : "" : "" : "",
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

        public ActionResult Get_LvCancelReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeLeave
                        .Include(e => e.LvNewReq)
                         .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<LvCancelReqChildDataClass> returndata = new List<LvCancelReqChildDataClass>();
                        foreach (var item in db_data.LvNewReq)
                        {
                            if (item.IsCancel == true)
                            {
                                returndata.Add(new LvCancelReqChildDataClass
                                {
                                    Id = item.Id,
                                    ReqDate = item.ReqDate != null ? item.ReqDate.Value.ToString("dd/MM/yyyy") : null,
                                    LvNewReq = item.FullDetails,
                                    Reason = item.Reason
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

        public JsonResult Polulate_LeaveCalendar(string data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //int id = int.Parse(data);
                var a = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").ToList();

                return Json(a, JsonRequestBehavior.AllowGet);
            }
        }

    }

}