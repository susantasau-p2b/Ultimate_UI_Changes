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
namespace EssPortal.Controllers
{
    public class LeaveCancelReqController : Controller
    {
        //
        // GET: /LeaveCancelReq/
        public ActionResult LeaveCancelIndex()
        {
            return View("~/Views/LeaveCancelReq/Index.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/_LvCancelReqGridPartial.cshtml");
        }
        [HttpPost]
        public ActionResult Create(LvCancelReq L, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
                    string ContactNolist = form["ContactNolist"] == "0" ? "" : form["ContactNolist"];

                    string Emp = form["Emp_LvCancelReq_id"] == "0" ? "" : form["Emp_LvCancelReq_id"];


                    int EmpId = 0;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        EmpId = int.Parse(Emp);
                    }
                    else
                    {
                        Msg.Add("  Please Select Employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (LvNewReqlist != null && LvNewReqlist != "")
                    {
                        int id = int.Parse(LvNewReqlist);
                        var value = db.LvNewReq.Where(e => e.Id == id)
                            .Include(e => e.LeaveCalendar)
                            .Include(e => e.LeaveHead)
                            .Include(e => e.GeoStruct)
                            .Include(e => e.PayStruct)
                            .Include(e => e.FuncStruct)
                            .FirstOrDefault();

                        L.LvNewReq = value;

                    }
                    else
                    {
                        Msg.Add("  Please Select  Leave Cancel Requisition");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    //  var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    var OEmployeeLv = db.EmployeeLeave
                        .Include(e => e.LvCancelReq)
                        .Include(e => e.Employee)
                        .Where(e => e.Employee.Id == EmpId)
                        .SingleOrDefault();
                    //var PrevReq = OEmployeeLv.LvNewReq
                    //    //.Where(e => e.LeaveHead.Id == L.LvNewReq.LeaveHead.Id && e.LeaveCalendar.Id == LvCalendar.Id
                    //   .Where(e => e.LeaveHead.Id == L.LvNewReq.LeaveHead.Id
                    //    //&& e.IsCancel == false
                    //        )
                    //    .OrderByDescending(e => e.Id).FirstOrDefault();


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
                    LvWFDetails oLvWFDetails = new LvWFDetails
                    {
                        WFStatus = 0,
                        Comments = L.Reason,
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };
                    List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                    oLvWFDetails_List.Add(oLvWFDetails);
                    LvCancelReq oLvCancelReq = new LvCancelReq()
                    {
                        ReqDate = L.ReqDate,
                        ContactNo = L.ContactNo,
                        // CreditDays = L.LvNewReq.DebitDays,
                        Reason = L.Reason,
                        Calendar = L.LvNewReq.LeaveCalendar,
                        DBTrack = L.DBTrack,
                        LvNewReq = L.LvNewReq,
                        // OpenBal = PrevReq.CloseBal,
                        // CloseBal = PrevReq.CloseBal + L.LvNewReq.DebitDays,
                        InputMethod = 1,
                        GeoStruct = L.LvNewReq.GeoStruct,
                        PayStruct = L.LvNewReq.PayStruct,
                        FuncStruct = L.LvNewReq.FuncStruct,
                        LvWFDetails = oLvWFDetails_List,
                        WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                    };
                    if (ModelState.IsValid)
                    {
                        if (Z != null)
                        {

                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    db.LvCancelReq.Add(oLvCancelReq);
                                    db.SaveChanges();
                                    List<LvCancelReq> OFAT = new List<LvCancelReq>();
                                    OFAT.Add(db.LvCancelReq.Find(oLvCancelReq.Id));
                                    var aa = db.EmployeeLeave.Find(OEmployeeLv.Id);
                                    OFAT.AddRange(aa.LvCancelReq);
                                    aa.LvCancelReq = OFAT;
                                    db.EmployeeLeave.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    // Msg.Add("  Data Saved successfully  ");
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
                                    //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                    return Json(new { status = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        Msg.Add("  Unable to create...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        public class GetLvNewReqClass
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LvHead { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string Status { get; set; }
            public string WF { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
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
        public class GetLvNewReqClass2
        {
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string LvHead { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }

            public ChildGetLvNewReqClass2 RowData { get; set; }
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
                      .Include(e => e.LvCancelReq.Select(a => a.LvWFDetails))
                      .Include(e => e.LvCancelReq.Select(a => a.WFStatus))
                      .Include(e => e.LvCancelReq.Select(a => a.Calendar))
                      .Include(e => e.LvCancelReq.Select(a => a.LvNewReq))
                      .Include(e => e.LvCancelReq.Select(a => a.LvNewReq.LeaveHead))
                     .SingleOrDefault();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();

                //var LvOrignal_id = db_data.LvNewReq.Where(e => e.LvOrignal != null).Select(e => e.LvOrignal.Id).ToList();
                //var AntCancel = db_data.LvNewReq.OrderBy(e => e.Id).ToList();
                //var listLvs = AntCancel.Where(e => !LvOrignal_id.Contains(e.Id)).OrderBy(e => e.Id).ToList();
                //if (listLvs != null && listLvs.Count() > 0)
                //{
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                returndata.Add(new GetLvNewReqClass
                {
                    ReqDate = "Requisition Date",
                    LvHead = "Leave Head",
                    FromDate = "From Date",
                    ToDate = "To Date",
                    Status = "Status"
                });
                // List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                foreach (var item in db_data.LvCancelReq)
                {
                    //DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                    //if (lvcrdate != null)
                    //{
                    //DateTime? Lvyearfrom = lvcrdate;
                    //DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                    //LvyearTo = LvyearTo.Value.AddYears(1);
                    //foreach (var item in listLvs.Where(e => e.Narration != "Leave Encash Payment" && e.LeaveHead.Id == item1 &&
                    //    e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0" && e.ReqDate >= Lvyearfrom && e.ReqDate <= LvyearTo).OrderByDescending(a => a.ReqDate).ToList())
                    //{
                    //  var FromDate = item.FromDate != null ? item.FromDate.Value.ToShortDateString() : null;
                    // var ToDate = item.ToDate != null ? item.ToDate.Value.ToShortDateString() : null;
                    var Status = "--";
                    if (item.InputMethod == 1 && item.LvWFDetails.Count > 0)
                    {
                        Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                        .Select(e => e.Value).SingleOrDefault();
                    }
                    if (item.InputMethod == 0)
                    {
                        Status = "Approved By HRM (M)";
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
                            LvHead_Id = item.LvNewReq.LeaveHead.Id.ToString(),
                        },
                        ReqDate = item.ReqDate.Value.ToShortDateString(),
                        LvHead = item.LvNewReq.LeaveHead.LvName,
                        FromDate = item.LvNewReq.FromDate == null ? "" : item.LvNewReq.FromDate.Value.ToShortDateString(),
                        ToDate = item.LvNewReq.ToDate == null ? "" : item.LvNewReq.ToDate.Value.ToShortDateString(),
                        Status = Status
                    });
                    // }
                    // }
                }
                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
            }
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
                               .Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = lookupdata.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                //var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //var Funcsubmodulename = Session["functionsubmodule"] as List<string>;
                //if (Funcsubmodulename.Count() > 0)
                //{
                //    foreach (var item in Funcsubmodulename)
                //    {
                //        string FuncSubModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == item.ToString().ToUpper()).Select(e => e.LookupVal).SingleOrDefault();
                //        Funcsubid.Add(FuncSubModule);
                //    }
                //}
                //else
                //{
                //    List<LvHead> Allleavhead = db.LvHead.Distinct().ToList();
                //    foreach (var item in Allleavhead)
                //    {
                //        Funcsubid.Add(item.LvCode);
                //    }
                //}
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //  var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);

                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                List<int> EmpIds = new List<int>();
                string funsubmodule = "";
                List<GetLvNewReqClass2> ListreturnLvnewClass = new List<GetLvNewReqClass2>();
                List<EmployeeLeave> LvList = new List<EmployeeLeave>();
                //foreach (var item in EmpIds)
                //{
                //    var temp = db.EmployeeLeave
                //  .Include(e => e.Employee)
                //   .Include(e => e.Employee.EmpName)
                //   .Include(e => e.LvNewReq)
                //   .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                //   .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                //   .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                //        //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                //   .Include(e => e.LvNewReq.Select(a => a.LvWFDetails))
                //   .Where(e => e.Employee.Id == item).FirstOrDefault();
                //    if (temp != null)
                //    {
                //        LvList.Add(temp);
                //    }
                //}
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
                           .Include(e => e.LvCancelReq.Select(a => a.Calendar))
                           .Include(e => e.LvCancelReq.Select(a => a.LvNewReq))
                           .Include(e => e.LvCancelReq.Select(a => a.LvNewReq.LeaveHead))
                            //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                           .Include(e => e.LvCancelReq.Select(a => a.LvWFDetails));
                        //.Where(e => EmpIds.Contains(e.Employee.Id)).ToList();

                        LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                        var LvIds = UserManager.FilterLvCancelReq(LvList.SelectMany(e => e.LvCancelReq).OrderByDescending(e => e.ReqDate).ToList(),
                            Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                        var session = Session["auho"].ToString().ToUpper();

                        //if (item1.SubModuleName != null)
                        //{
                        //    Funcsubid.Add(item1.SubModuleName);
                        //}
                        //else
                        //{
                        //    List<LvHead> Allleavhead = db.LvNewReq.Include(e => e.LeaveHead).AsNoTracking().Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead).Distinct().ToList();
                        //  //  List<string> DistinctLvhead = Allleavhead.Distinct().Select(e=>e.LvCode).ToList();
                        //    foreach (var item2 in Allleavhead)
                        //    {
                        //        Funcsubid.Add(item2.LvCode);
                        //    }
                        //}
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

                            var query = db.EmployeeLeave.Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                                .Include(e => e.LvCancelReq.Select(t => t.LvNewReq))
                                .Include(e => e.LvCancelReq.Select(t => t.LvNewReq.LeaveHead))
                                .Include(e => e.LvCancelReq.Select(t => t.Calendar))
                               .Where(e => e.LvCancelReq.Any(a => a.Id == item))
                                //.Select(e => new
                                //{
                                //    LvNewReq = e.LvNewReq.Where(a => a.Id == item),
                                //    EmpLVid = e.Id,
                                //    IsClose = e.Employee.ReportingStructRights
                                //    .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session)
                                //    .Select(a => a.AccessRights.IsClose).FirstOrDefault(),
                                //    EmpId = e.Employee.Id,
                                //    EmpName = e.Employee.EmpName.FullNameFML,
                                //    EmpCode = e.Employee.EmpCode
                                //})
                                .SingleOrDefault();

                            //foreach (var lvcode in Funcsubid)
                            //{
                            if (item1.SubModuleName == null)
                            {
                                //List<int> lvcode = query.LvNewReq.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                                //foreach (var Lvheadid in lvcode)
                                //{
                                //DateTime? lvcrdate = query.LvNewReq.Where(a => a.LeaveHead.Id == Lvheadid && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                                //if (lvcrdate != null)
                                //{
                                // DateTime? Lvyearfrom = lvcrdate;
                                // DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                // LvyearTo = LvyearTo.Value.AddYears(1);
                                var LvReq = query.LvCancelReq.Where(a => a.Id == item).FirstOrDefault();
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
                                            LvHead_Id = LvReq.LvNewReq.Id.ToString(),
                                        },
                                        Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                        ReqDate = LvReq.ReqDate.Value.ToShortDateString(),
                                        LvHead = LvReq.LvNewReq.LeaveHead.LvName,
                                        FromDate = LvReq.LvNewReq.FromDate.Value.ToShortDateString(),
                                        ToDate = LvReq.LvNewReq.ToDate.Value.ToShortDateString(),

                                    });
                                }
                                //}
                                // }
                            }
                            else
                            {
                                //List<int> lvcode = query.LvNewReq.Where(e => e.LvCreditDate != null && e.LeaveHead.LvCode.ToUpper() == item1.SubModuleName.ToUpper()).Select(e => e.LeaveHead.Id).Distinct().ToList();
                                //foreach (var Lvheadid in lvcode)
                                //{
                                //DateTime? lvcrdate = query.LvNewReq.Where(a => a.LeaveHead.Id == Lvheadid && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                                //if (lvcrdate != null)
                                //{
                                //DateTime? Lvyearfrom = lvcrdate;
                                // DateTime? LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                // LvyearTo = LvyearTo.Value.AddYears(1);
                                var LvReq = query.LvCancelReq.Where(a => a.Id == item).FirstOrDefault();
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
                                            LvHead_Id = LvReq.LvNewReq.LeaveHead.Id.ToString(),
                                        },
                                        Emp = query.Employee.EmpCode + " " + query.Employee.EmpName.FullNameFML,
                                        ReqDate = LvReq.ReqDate.Value.ToShortDateString(),
                                        LvHead = LvReq.LvNewReq.LeaveHead.LvName,
                                        FromDate = LvReq.LvNewReq.FromDate.Value.ToShortDateString(),
                                        ToDate = LvReq.LvNewReq.ToDate.Value.ToShortDateString(),

                                    });
                                }
                                // }
                                //}

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
        public class EmpmLVdata
        {

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string Req_Date { get; set; }
            public string Branch { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public string FromDate { get; set; }
            public string SanctionCode { get; set; }
            public double Debit_Days { get; set; }
            public string SanctionEmpname { get; set; }
            public string Todate { get; set; }
            public string Reason { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string Wf { get; set; }
            public Int32 Id { get; set; }
            public Int32 Lvnewreq { get; set; }
            public bool TrClosed { get; set; }
            public string EmployeeName { get; set; }
            public string Empcode { get; set; }

            public int EmployeeId { get; set; }
        }
        public class LvNewReqData
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }

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
                                 .Include(e => e.LvCancelReq)
                                 .Include(e => e.LvCancelReq.Select(t => t.WFStatus))
                               .Include(e => e.LvCancelReq.Select(t => t.LvNewReq))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveHead))
                    .Include(e => e.LvCancelReq.Select(r => r.LvNewReq))
                    .Include(e => e.LvNewReq.Select(r => r.WFStatus))
                    .Include(e => e.LvNewReq.Select(r => r.LeaveCalendar))
                  .Where(e => e.Employee.Id == Id).SingleOrDefault();


                var lvcancelreqidslist = OEmployeeLeave.LvCancelReq.Where(e => e.WFStatus.LookupVal == "0" || e.WFStatus.LookupVal == "1").Select(e => e.LvNewReq.Id).ToList();
                // var DefCal = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //Cl max date
                List<int> lvcode = OEmployeeLeave.LvNewReq.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
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
                        var OEmpLeave = OEmployeeLeave.LvNewReq.Where(e => !lvcancelreqidslist.Contains(e.Id) && e.LeaveHead.Id == item && e.IsCancel == false && e.TrClosed == true && e.LvOrignal == null && e.WFStatus.LookupVal != "2" && e.ReqDate >= Lvyearfrom.Value && e.ReqDate <= LvyearTo.Value).ToList();

                        var query = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == item && (e.TrReject == true || e.IsCancel == true) && e.TrClosed == true && e.LvOrignal != null && e.ReqDate >= Lvyearfrom.Value && e.ReqDate <= LvyearTo.Value).ToList();


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

        public ActionResult GetEmpLvData(string data)
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

                var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeeLeave
                    .Include(e => e.LvCancelReq)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.GeoStruct)
                    .Include(e => e.Employee.FuncStruct)
                    .Include(e => e.Employee.FuncStruct.Job)
                     .Include(e => e.Employee.GeoStruct.Location)
                     .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                    .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                    .Include(e => e.LvCancelReq.Select(t => t.WFStatus))
                    .Include(e => e.LvCancelReq.Select(t => t.LvNewReq))
                    .Include(e => e.LvCancelReq.Select(t => t.LvNewReq.LeaveHead))
                    .Include(e => e.LvCancelReq.Select(t => t.LvWFDetails))
                    .Where(e => e.Employee.Id == EmpLvIdint && e.LvCancelReq.Any(w => w.Id == id)).SingleOrDefault();

                var v = W.LvCancelReq.Where(e => e.Id == id).Select(s => new EmpmLVdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Lvnewreq = s.Id,
                    // filepath = s.Path,
                    Empcode = W.Employee.EmpCode,
                    //   Status = s.WFStatus.LookupVal.ToString(),
                    Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                    Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                    Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                    Status = status,
                    Emplvhead = s.LvNewReq.LeaveHead != null ? s.LvNewReq.LeaveHead.FullDetails : null,
                    // lvcalendar = s.LeaveCalendar != null ? s.LeaveCalendar.FullDetails : null,
                    //  lvClosing = s.CloseBal,
                    //   lvOpening = s.OpenBal,
                    //  Lvoccurance = s.LvOccurances,
                    //  FromStat = s.FromStat != null ? s.FromStat.LookupVal.ToUpper().ToString() : null,
                    // ToStat = s.ToStat != null ? s.ToStat.LookupVal.ToUpper().ToString() : null,
                    //  Resume_Date = s.ResumeDate != null ? s.ResumeDate.Value.ToShortDateString() : null,
                    Req_Date = s.ReqDate != null ? s.ReqDate.Value.ToShortDateString() : null,
                    FromDate = s.LvNewReq.FromDate != null ? s.LvNewReq.FromDate.Value.ToShortDateString() : null,
                    Todate = s.LvNewReq.ToDate != null ? s.LvNewReq.ToDate.Value.ToShortDateString() : null,
                    Debit_Days = s.LvNewReq.DebitDays,
                    //  EmpContactNO = s.ContactNo != null ? s.ContactNo.FullContactNumbers : null,
                    Reason = s.Reason,
                    Id = s.Id,
                    TrClosed = s.TrClosed,
                    SanctionCode = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    SanctionComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    ApporavalComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Wf = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null
                }).SingleOrDefault();


                //var EmpCheck = db.EmployeeLeave.Include(e => e.LvNewReq)
                //                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                //                .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                //                .Where(e => e.Id == EmpLvIdint && e.LvNewReq.Any(w => w.LeaveHead.Id == lvheadidint)).SingleOrDefault();
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
                //if Emp Bal updated
                var listOfObject = new List<dynamic>();
                //var EmpCheck_Id = EmpCheck.LvNewReq.Where(e => e.LeaveHead.Id == lvheadidint).Select(e => e.Id).LastOrDefault();
                //if (v.Id != EmpCheck_Id)
                //{

                //    var a = EmpCheck.LvNewReq.Where(e => e.LeaveHead.Id == lvheadidint).LastOrDefault();
                //    v.lvOpening = a.OpenBal;
                //    v.lvClosing = a.CloseBal;

                //    listOfObject.Add(v);
                //    return Json(listOfObject, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                listOfObject.Add(v);
                return Json(listOfObject, JsonRequestBehavior.AllowGet);
                //}
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
            bool SanctionRejected = false;
            bool HrRejected = false;
            List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();

            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.LvCancelReq
                    .Include(e => e.WFStatus)
                    .Include(e => e.Calendar)
                    //.Include(e => e.LeaveHead)
                    .Include(e => e.LvWFDetails)
                    // .Include(e => e.FromStat)
                    .Include(e => e.GeoStruct)
                    //.Include(e => e.LvOrignal)
                    .Include(e => e.PayStruct)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.LvNewReq.LeaveHead)
                    .Where(e => e.Id == lvnewreqid).SingleOrDefault();

                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                if (authority.ToUpper() == "MYSELF")
                {
                    qurey.Reason = ReasonMySelf;
                    // qurey.IsCancel = true;
                    qurey.TrClosed = true;
                    qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
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
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 1,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                        qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        if (qurey.TrClosed == true)
                        {
                            SanctionRejected = true;
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
                        LvWFDetails oLvWFDetails = new LvWFDetails
                        {
                            WFStatus = 2,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };
                        // oLvWFDetails_List.Add(oLvWFDetails);
                        qurey.LvWFDetails.Add(oLvWFDetails);
                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                        qurey.TrClosed = true;
                        qurey.TrReject = true;
                        // SanctionRejected = true;
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
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 3,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                        if (qurey.TrClosed == true)
                        {
                            HrRejected = true;
                        }
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
                        LvWFDetails oLvWFDetails = new LvWFDetails
                        {
                            WFStatus = 4,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };
                        oLvWFDetails_List.Add(oLvWFDetails);
                        qurey.LvWFDetails.Add(oLvWFDetails);
                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                        qurey.TrClosed = true;
                        qurey.TrReject = true;
                    }
                }
                else if (authority.ToUpper() == "RECOMMAND")
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
                                   .Include(e => e.Employee)
                                   .Include(e => e.LvNewReq)
                                   .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                   .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                   .Where(e => e.Id == EmpLvId)
                                //.Select(e => new { EmpId = e.Id, LvNewReq = e.LvNewReq })
                                  .SingleOrDefault();

                            var PrevReq = OEmployeeLv.LvNewReq
                                .Where(e => e.LeaveHead != null && e.LeaveHead.Id == qurey.LvNewReq.LeaveHead.Id
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
                                LvOrignal = qurey.LvNewReq,
                                CreditDays = qurey.LvNewReq.DebitDays,
                                FromDate = qurey.LvNewReq.FromDate,
                                FromStat = qurey.LvNewReq.FromStat,
                                LeaveHead = qurey.LvNewReq.LeaveHead,
                                Reason = qurey.Reason,
                                ResumeDate = qurey.LvNewReq.ResumeDate,
                                ToDate = qurey.LvNewReq.ToDate,
                                ToStat = qurey.LvNewReq.ToStat,
                                LeaveCalendar = qurey.LvNewReq.LeaveCalendar,
                                DBTrack = qurey.DBTrack,
                                OpenBal = PrevReq.CloseBal,
                                CloseBal = PrevReq.CloseBal + qurey.LvNewReq.DebitDays,
                                LVCount = PrevReq.LVCount - qurey.LvNewReq.DebitDays,
                                LvOccurances = PrevReq.LvOccurances - 1,
                                GeoStruct = qurey.GeoStruct,
                                PayStruct = qurey.PayStruct,
                                FuncStruct = qurey.FuncStruct,
                                LvWFDetails = oLvWFDetails_List,
                                Narration = "Leave Cancelled",
                                LvCountPrefixSuffix = PrevReq.LvCountPrefixSuffix,
                                WFStatus = db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault()
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
                            qurey.OpenBal = oLvNewReq.OpenBal;
                            qurey.CloseBal = oLvNewReq.CloseBal;
                            qurey.CreditDays = oLvNewReq.CreditDays;
                        }
                        db.LvCancelReq.Attach(qurey);
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

    }
}