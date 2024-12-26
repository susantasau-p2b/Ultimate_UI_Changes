using P2b.Global;
using EssPortal.App_Start;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using EssPortal.Models;
using System.Threading.Tasks;
using EssPortal.Security;
using Leave;
using Training;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace EssPortal.Controllers
{
    public class TrainingPresentyController : Controller
    {
        public ActionResult Partial()
        {
            return View("~/Views/TrainingPresenty/Index.cshtml");
        }
        public ActionResult Partial_View()
        {
            return View("~/Views/Shared/_TrainingNeedReqPartial.cshtml");
        }
        public ActionResult Partial_View2()
        {
            return View("~/Views/Shared/_TrainingPresentyReq.cshtml");
        }
        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        public ActionResult GetEmpTrainingPresentyData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //string authority = Convert.ToString(Session["auho"]);
                //var WfStatus = new List<Int32>();
                //var SanctionStatus = new List<Int32>();
                //var ApporvalStatus = new List<Int32>();
                //if (authority.ToUpper() == "SANCTION")
                //{
                //    WfStatus.Add(1);
                //    WfStatus.Add(2);
                //}
                //else if (authority.ToUpper() == "APPROVAL")
                //{
                //    WfStatus.Add(1);
                //    WfStatus.Add(2);
                //}
                //else if (authority.ToUpper() == "MYSELF")
                //{
                //    SanctionStatus.Add(1);
                //    SanctionStatus.Add(2);

                //    ApporvalStatus.Add(3);
                //    ApporvalStatus.Add(4);
                //}
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                //var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                // var LvHeadId = ids.Count > 0 ? ids[3] : null;

                // var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var w = db.TrainigDetailSessionInfo
                        .Include(e => e.TrainingSession)
                        .Include(e => e.TrainingSession.SessionType)
                        .Include(e => e.TrainingSession.TrainingProgramCalendar)
                        .Include(e => e.TrainingSession.TrainingProgramCalendar.ProgramList)
                        .Where(e => e.Id == id).SingleOrDefault();
                //var W = db.EmployeeTraining
                //   // .Include(e => e.Employee.EmpName)
                //   // .Include(e => e.Employee.GeoStruct)
                //   // .Include(e => e.Employee.FuncStruct)
                //   // .Include(e => e.Employee.FuncStruct.Job)
                //   //  .Include(e => e.Employee.GeoStruct.Location)
                //   //  .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                //    //.Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                //    .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo))
                //    .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo))
                //    .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
                //    .Include(e => e.EmpTrainingNeed.Select(t => t.LvWFDetails))
                //    .Where(e => e.Employee.Id == EmpLvIdint && e.TrainingDetails.Any(w => w.Id == id)).SingleOrDefault();

                var v = new EmpmLVdata
                {
                    //EmployeeId = W.Employee.Id,
                    //EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    //Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                    //Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                    //Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                    //Status = status,
                    Id = w.Id,
                    //TrClosed = s.TrClosed,
                    ProgrameName = w.TrainingSession != null && w.TrainingSession.TrainingProgramCalendar != null && w.TrainingSession.TrainingProgramCalendar.ProgramList != null ? w.TrainingSession.TrainingProgramCalendar.ProgramList.FullDetails : null,
                    SessionName = w.TrainingSession != null ? w.TrainingSession.FullDetails : null,
                    Present = w.IsPresent,
                    Reason = w.CancelReason,
                    IsCancel = w.IsCancelled
                    //EmployeeComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => z.WFStatus == 0).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    // SanctionCode = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    //SanctionComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    //ApporavalComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    //Wf = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null
                };


                //if (v.SanctionCode != null)
                //{
                //    int sanctionid = Convert.ToInt32(v.SanctionCode);
                //    var sanctioncode = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == sanctionid).SingleOrDefault();
                //    if (sanctioncode != null)
                //    {
                //        v.SanctionCode = sanctioncode.Employee.EmpCode;
                //        v.SanctionEmpname = sanctioncode.Employee.EmpName.FullNameFML;
                //    }
                //}
                //if Emp Bal updated
                var listOfObject = new List<dynamic>();
                listOfObject.Add(v);
                return Json(listOfObject, JsonRequestBehavior.AllowGet);
            }
        }

        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public int EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public class GetLvNewReqClass2
        {
            public string EmpName { get; set; }
            public string EmpCode { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public class EmpTrainingNeedClass
        {
            public int Id { get; set; }
            public string Present { get; set; }
            public string Reason { get; set; }
            public string IsCancel { get; set; }
        }

        [HttpPost]
        public ActionResult Create(EmpTrainingNeed LvReq, FormCollection form, String forwarddata, string TrainingData)
        {

            var serialize = new JavaScriptSerializer();
            List<EmpTrainingNeedClass> obj = new List<EmpTrainingNeedClass>();
            if (TrainingData != "")
            {
                obj = serialize.Deserialize<List<EmpTrainingNeedClass>>(TrainingData);

                //var modifyornot = obj.Where(e => e.Comment != "").ToList();
                //if (obj.Count() == 0)
                //{
                //    return Json(new { sucess = true, responseText = "You have to Modify record to update." }, JsonRequestBehavior.AllowGet);
                //}
            }

            if (TrainingData == "")
            {
                return Json(new { sucess = true, responseText = "You have to Modify record to update." }, JsonRequestBehavior.AllowGet);
            }
            string Emp = form["Employee_id"] == "0" ? "" : form["Employee_id"];

            string comment = "";
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                //int EmpId = 0;
                //if (Emp != null && Emp != "0" && Emp != "false")
                //{
                //    EmpId = int.Parse(Emp);
                //}
                //else
                //{
                //    return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                //}
                //if (ProgramListlist != null)
                //{
                //    var val = db.ProgramList.Find(int.Parse(ProgramListlist));
                //    LvReq.ProgramList = val;
                //}
                //if (Comments != null)
                //{
                //    //   comment = Comments;
                //}
                //if (EmployeeTrainingSource != null && EmployeeTrainingSource != "")
                //{
                //}
                DBTrack DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true };
                // LvReq.TrainingCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();

                //var val1 = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "TRAININGNEED").SingleOrDefault();
                //LvReq.EmployeeTrainingSource = val1;
                //Employee OEmployee = null;
                //EmployeeTraining Oemployeetraining = null;
                //Oemployeetraining = db.EmployeeTraining.Include(e => e.EmpTrainingNeed).Include(e => e.Employee)
                //.Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                //List<EmpTrainingNeed> OFAT = new List<EmpTrainingNeed>();
                //LvReq.RequisitionDate = DateTime.Now.Date;
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        string iscancelreason = "";
                        foreach (var a in obj)
                        {
                            if (a.Present != "" && a.IsCancel != "")
                            {
                                iscancelreason = a.Reason;
                                bool IsCancelled = false;
                                bool IsPresent = false;

                                if (a.IsCancel.ToString().ToUpper() == "YES")
                                {
                                    IsCancelled = true;
                                }
                                if (a.Present.ToString().ToUpper() == "YES")
                                {
                                    IsPresent = true;
                                }

                                if (IsCancelled == false && IsPresent == false)
                                {
                                    iscancelreason = "Absent";
                                }
                                var SessionLstInfo = db.TrainigDetailSessionInfo.Where(t => t.Id == a.Id).FirstOrDefault();
                                if (SessionLstInfo != null)
                                {
                                    SessionLstInfo.DBTrack = DBTrack;
                                    SessionLstInfo.IsCancelled = IsCancelled;
                                    SessionLstInfo.IsPresent = IsPresent;
                                    SessionLstInfo.CancelReason = iscancelreason;
                                    db.TrainigDetailSessionInfo.Attach(SessionLstInfo);
                                    db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Detached;
                                }
                            }
                        }

                        ts.Complete();
                        Msg.Add("Data Saved Successfully");
                        return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    DBTrack DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true };
                    string PresentList = form["IsCancel"] == "0" ? "" : form["IsCancel"];
                    string IscancelList = form["IsCancelList"] == "0" ? "" : form["IsCancelList"];
                    string Trainingdetailssession = form["LvId"] == "0" ? "" : form["LvId"];
                    string Reason = form["Presenty_reason"] == "0" ? "" : form["Presenty_reason"];
                    var PresentStatus = false;
                    var IscancelStatus = false;
                    string Reasoniscancel = "";
                    var Trainingdetailssessionid = 0;
                    if (Trainingdetailssession != null)
                    {
                        Trainingdetailssessionid = Convert.ToInt32(Trainingdetailssession);
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Try Again" }, JsonRequestBehavior.AllowGet);
                    }
                    if (Reason != "")
                    {
                        Reasoniscancel = Reason;
                    }

                    if (PresentList != null)
                    {
                        PresentStatus = Convert.ToBoolean(PresentList);
                        //if (LvCancelchk == false)
                        //{
                        //    return Json(new { status = true, responseText = "Set cancel True" }, JsonRequestBehavior.AllowGet);
                        //}
                    }
                    if (IscancelList != null)
                    {
                        IscancelStatus = Convert.ToBoolean(IscancelList);
                    }

                    if (PresentStatus == true && IscancelStatus == true)
                    {
                        return Json(new { status = true, responseText = "IsCancel and Present Never Be Same" }, JsonRequestBehavior.AllowGet);
                    }
                    //else
                    //{
                    //    return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
                    //}

                    int lvnewreqid = Convert.ToInt32(data);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            Reasoniscancel = Reason;
                            if (IscancelStatus == false && PresentStatus == false)
                            {
                                Reasoniscancel = "Absent";
                            }

                            var SessionLstInfo = db.TrainigDetailSessionInfo.Where(t => t.Id == Trainingdetailssessionid).FirstOrDefault();
                            if (SessionLstInfo != null)
                            {
                                SessionLstInfo.DBTrack = DBTrack;
                                SessionLstInfo.IsCancelled = IscancelStatus;
                                SessionLstInfo.IsPresent = PresentStatus;
                                SessionLstInfo.CancelReason = Reasoniscancel;
                                db.TrainigDetailSessionInfo.Attach(SessionLstInfo);
                                db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Detached;
                            }
                            // }

                            ts.Complete();
                            Msg.Add("Data Saved Successfully");
                            return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
            // }
        }
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

        }
        public class GetLvNewReqClass
        {
            public string Session { get; set; }
            public string Program { get; set; }
            public string Present { get; set; }
            public string IsCancel { get; set; }
            public string CancelReason { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetEmptrainingNeeddata
        {
            public int Id { get; set; }
            public string IsCancel { get; set; }
            public string SessionDetail { get; set; }
            public string ProgrameDetail { get; set; }
            public string Present { get; set; }
            public string Reason { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public ActionResult GetEmployeeProgrameList(string RecruitCalendarid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                DateTime date = DateTime.Now.Date;
                var Empprogramlistids = db.EmployeeTraining
                                        .Include(e => e.Employee)
                                        .Include(e => e.TrainingDetails)
                                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo))
                                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(r => r.TrainingSession.SessionType)))
                                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(r => r.TrainingSession.TrainingProgramCalendar)))
                                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(r => r.TrainingSession.TrainingProgramCalendar.ProgramList)))
                    //  .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
                    //.Include(e => e.EmpTrainingNeed.Select(t => t.TrainingCalendar))
                                        .Where(e => e.Employee.Id == Id).SingleOrDefault();
                Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
                //var Appliedprogramlistid = Empprogramlistids.EmpTrainingNeed.Where(e => e.IsCancel != true && e.TrReject != true && e.TrainingCalendar.Id == DefaultCalendaryearid.Id).Select(e => e.ProgramList.Id).ToList();
                //var db_data = db.YearlyTrainingCalendar
                //      .Include(e => e.TrainingCalendar)
                //      .Include(e => e.TrainigProgramCalendar)
                //      .Include(e => e.TrainigProgramCalendar.Select(t => t.ProgramList))
                //      .Where(e => e.TrainingCalendar.Id == DefaultCalendaryearid.Id)
                //     .ToList();

                //var filtercurrentdateids = db.TrainingProgramCalendar.Where(e => e.EndDate <= date).Select(e => e.Id).ToList();
                List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();

                foreach (var item in Empprogramlistids.TrainingDetails)
                {
                    //var appliedprogrammeids = item.TrainigDetailSessionInfo.Where(e => !Appliedprogramlistid.Contains(e.ProgramList.Id) && !filtercurrentdateids.Contains(e.Id)).ToList();
                    if (item.TrainigDetailSessionInfo.Where(t => t.IsPresent == false && t.IsCancelled == false && t.CancelReason != "Absent").ToList().Count() > 0)
                    {
                        foreach (var item1 in item.TrainigDetailSessionInfo)
                        {

                            if (item1.TrainingSession != null && item1.TrainingSession.TrainingProgramCalendar != null && item1.TrainingSession.TrainingProgramCalendar.ProgramList != null)
                            {
                                string ProgrameFulldetails = "Start Date:" + item1.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() + ", End Date:"
                                                          + item1.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() + ", Subject Details:" + item1.TrainingSession.TrainingProgramCalendar.ProgramList.SubjectDetails;
                                returndata.Add(new GetEmptrainingNeeddata
                                {
                                    //RowData = new ChildGetLvNewReqClass
                                    //{
                                    //    LvNewReq = item2.Id.ToString(),
                                    //    EmpLVid = db_data.Employee.Id.ToString(),
                                    //    // IsClose = Status,
                                    //    //Status = Status,
                                    //    // LvHead_Id = item.LeaveHead.Id.ToString(),
                                    //},
                                    Id = item1.Id,
                                    SessionDetail = item1.TrainingSession.FullDetails,
                                    ProgrameDetail = ProgrameFulldetails,
                                    IsCancel = item1.IsPresent == false && item1.IsCancelled == false && item1.CancelReason == null ? "" : item1.IsCancelled == false && item1.CancelReason != "Absent" ? "No" : "Yes",
                                    Reason = item1.IsPresent == false && item1.IsCancelled == false ? "" : item1.CancelReason,
                                    Present = item1.IsPresent == false && item1.IsCancelled == false && item1.CancelReason == null ? "" : item1.IsPresent == false && item1.CancelReason == "Absent" ? "Absent" : item1.IsPresent == true ? "Present" : "",

                                });
                            }
                        }
                    }
                }
                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

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
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
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
                List<EmployeeTraining> LvList = new List<EmployeeTraining>();
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
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    Location = "Location",
                    Department = "Department",
                    RowData = new ChildGetLvNewReqClass2
                    {
                        EmpLVid = 0,
                    },
                });
                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        List<string> Funcsubid = new List<string>();
                        var temp = db.EmployeeTraining
                          .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.EmpTrainingNeed)
                           .Include(e => e.EmpTrainingNeed.Select(a => a.ProgramList))
                            //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                           .Include(e => e.EmpTrainingNeed.Select(a => a.LvWFDetails));
                        //.Where(e => EmpIds.Contains(e.Employee.Id)).ToList();

                        LvList = temp.Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();

                        var LvIds = UserManager.FilterTrainingNeed(LvList.SelectMany(e => e.EmpTrainingNeed).OrderBy(e => e.Id).ToList(),
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
                        if (listlvids.Count() > 0)
                        {
                            foreach (var item in listlvids)
                            {
                                var query = db.EmployeeTraining.Include(e => e.Employee)
                                .Include(e => e.Employee.EmpName)
                                .Include(e => e.Employee.GeoStruct)
                                .Include(e => e.Employee.GeoStruct.Department)
                                .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                                .Include(e => e.Employee.GeoStruct.Location)
                                .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights))
                                 .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                             .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                                .Include(e => e.EmpTrainingNeed)
                               .Where(e => e.EmpTrainingNeed.Any(a => a.Id == item))

                                .SingleOrDefault();


                                if (item1.SubModuleName == null)
                                {

                                    var LvReq = query.EmpTrainingNeed.Where(a => a.Id == item).FirstOrDefault();
                                    if (LvReq != null)
                                    {
                                        ListreturnLvnewClass.Add(new GetLvNewReqClass2
                                        {
                                            RowData = new ChildGetLvNewReqClass2
                                            {
                                                LvNewReq = LvReq.Id.ToString(),
                                                EmpLVid = query.Employee.Id,
                                                IsClose = query.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                                                // LvHead_Id = LvReq.LeaveHead.Id.ToString(),
                                            },
                                            EmpCode = query.Employee.EmpCode,
                                            EmpName = query.Employee.EmpName.FullNameFML,
                                            Location = query.Employee.GeoStruct == null ? "" : query.Employee.GeoStruct.Location == null ? "" : query.Employee.GeoStruct.Location.LocationObj == null ? "" : query.Employee.GeoStruct.Location.LocationObj.LocDesc,
                                            Department = query.Employee.GeoStruct == null ? "" : query.Employee.GeoStruct.Department == null ? "" : query.Employee.GeoStruct.Department.DepartmentObj == null ? "" : query.Employee.GeoStruct.Department.DepartmentObj.DeptDesc,
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                var data1 = ListreturnLvnewClass.Select(cust => new { cust.EmpCode, cust.EmpName, cust.Department, cust.Location, RowData = new { EmpLVid = cust.RowData.EmpLVid }, }).Distinct();
                if (data1 != null)
                {

                }
                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = data1, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new { Lvcalendardesc = "FromData :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString() }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public class tempClass
        {
            public string LvName { get; set; }
            public string LvCode { get; set; }
            public string LvBal { get; set; }
            public string FullDetails { get; set; }
        }
        public class EmpLvClass
        {
            public string EmpName { get; set; }
            public List<ReqLvHeadWise> LvHeadName { get; set; }
        }
        public class ReqLvHeadWise
        {
            public string LvHeadName { get; set; }
            public string LvHeadCode { get; set; }
            public string LvHeadBal { get; set; }
            public Array LvReq { get; set; }
        }


        public ActionResult GetMyTrainingPresenty()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeTraining
                      .Where(e => e.Employee.Id == Id)
                      .Include(e => e.EmpTrainingNeed)
                      .Include(e => e.Employee)
                      .Include(e => e.Employee.EmpName)
                      .Include(e => e.TrainingDetails.Select(a => a.TrainigDetailSessionInfo))
                      .Include(e => e.TrainingDetails.Select(a => a.TrainigDetailSessionInfo.Select(t => t.TrainingSession.SessionType)))
                      .Include(e => e.TrainingDetails.Select(a => a.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                     .SingleOrDefault();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();


                //if (listLvs != null && listLvs.Count() > 0)
                //{
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                returndata.Add(new GetLvNewReqClass
                {
                    Session = "Session",
                    Program = "Program Name",
                    IsCancel = "IsCancel",
                    CancelReason = "CancelReason",
                    Present = "Present"
                });
                //List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                //foreach (var item1 in lvcode)
                //{
                // DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                //if (lvcrdate != null)
                //{

                foreach (var item in db_data.TrainingDetails)
                {
                    foreach (var item2 in item.TrainigDetailSessionInfo)
                    {
                        if (item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null)
                        {
                            string ProgrameFulldetails = "Start Date:" + item2.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() + ", End Date:"
                                                      + item2.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() + ", Subject Details:" + item2.TrainingSession.TrainingProgramCalendar.ProgramList.SubjectDetails;
                            returndata.Add(new GetLvNewReqClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item2.Id.ToString(),
                                    EmpLVid = db_data.Employee.Id.ToString(),
                                    //  IsClose = Status,
                                    //  Status = Status,
                                    // LvHead_Id = item.LeaveHead.Id.ToString(),
                                },
                                Session = item2.TrainingSession.FullDetails,
                                Program = ProgrameFulldetails,
                                IsCancel = item2.IsPresent == false && item2.IsCancelled == false && item2.CancelReason == null ? "" : item2.IsCancelled == false && item2.CancelReason != "Absent" ? "No" : "Yes",
                                CancelReason = item2.IsPresent == false && item2.IsCancelled == false ? "" : item2.CancelReason,
                                Present = item2.IsPresent == false && item2.IsCancelled == false && item2.CancelReason == null ? "" : item2.IsPresent == false && item2.CancelReason == "Absent" ? "Absent" : item2.IsPresent == true ? "Present" : "",

                            });
                        }
                    }
                }
                // }
                // }
                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                // }
                //else
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
            }
        }
        public class EmpmLVdata
        {

            //public string Status { get; set; }
            //public string Emplvhead { get; set; }
            //public string Req_Date { get; set; }
            //public string Branch { get; set; }
            //public string Department { get; set; }
            //public string Designation { get; set; }
            //public string SanctionCode { get; set; }
            //public string SanctionEmpname { get; set; }
            //public string SanctionComment { get; set; }
            //public string ApporavalComment { get; set; }
            //public string EmployeeComment { get; set; }
            public string ProgrameName { get; set; }
            public string SessionName { get; set; }
            public bool Present { get; set; }
            public bool IsCancel { get; set; }
            public string Reason { get; set; }
            //public string Wf { get; set; }
            public Int32 Id { get; set; }
            //public bool TrClosed { get; set; }
            //public string EmployeeName { get; set; }

            //public int EmployeeId { get; set; }
        }

        //public ActionResult GetEmpLvData(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string AccessRight = "";
        //        string funmodule = "";
        //        if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
        //        {
        //            funmodule = Convert.ToString(Session["user-module"]);
        //            //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
        //        }
        //        if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
        //        {
        //            AccessRight = Convert.ToString(Session["auho"]);
        //        }
        //        var ids = Utility.StringIdsToListString(data);
        //        // var id = Convert.ToInt32(ids[0]);
        //        // var status = ids.Count > 0 ? ids[2] : null;
        //        var emplvId = ids.Count > 0 ? ids[1] : null;
        //        // var LvHeadId = ids.Count > 0 ? ids[3] : null;

        //        // var lvheadidint = Convert.ToInt32(LvHeadId);
        //        var EmpLvIdint = Convert.ToInt32(emplvId);

        //        var Empprogramlistids = db.EmployeeTraining
        //                               .Include(e => e.Employee)
        //                               .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
        //                               .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
        //                               .Include(e => e.EmpTrainingNeed)
        //                               .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
        //                               .Include(e => e.EmpTrainingNeed.Select(t => t.LvWFDetails))
        //                               .Include(e => e.EmpTrainingNeed.Select(t => t.TrainingCalendar))
        //                               .Where(e => e.Employee.Id == EmpLvIdint).SingleOrDefault();


        //        var LvIds = UserManager.FilterTrainingNeed(Empprogramlistids.EmpTrainingNeed.ToList(),
        //                   Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

        //        // Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
        //        var Appliedprogramlistid = Empprogramlistids.EmpTrainingNeed.Where(e => LvIds.Contains(e.Id)).ToList();
        //        //var db_data = db.YearlyTrainingCalendar
        //        //      .Include(e => e.TrainingCalendar)
        //        //      .Include(e => e.TrainigProgramCalendar)
        //        //      .Include(e => e.TrainigProgramCalendar.Select(t => t.ProgramList))
        //        //      .Where(e => e.TrainingCalendar.Id == DefaultCalendaryearid.Id)
        //        //     .ToList();

        //        //var filtercurrentdateids = db.TrainingProgramCalendar.Where(e => e.EndDate <= date).Select(e => e.Id).ToList();
        //        List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();

        //        foreach (var item in Appliedprogramlistid)
        //        {
        //            // var appliedprogrammeids = item..Where(e => Appliedprogramlistid.Contains(e.ProgramList.Id)).ToList();
        //            //foreach (var item1 in appliedprogrammeids)
        //            //{
        //            // string programFulldetails = "Subject:" + item1.ProgramList.Subject + ",SubjectDetails:" + item1.ProgramList.SubjectDetails;
        //            returndata.Add(new GetEmptrainingNeeddata
        //            {
        //                //RowData = new ChildGetLvNewReqClass
        //                //{
        //                //    // LvNewReq = item.Id.ToString(),
        //                //    //EmpLVid = db_data.Employee.Id.ToString(),
        //                //   // IsClose = Empprogramlistids.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == AccessRight.ToUpper() && a.FuncModules.LookupVal.ToUpper() == funmodule.ToUpper())
        //                //                           //.Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
        //                //    //Status = Status,
        //                //    // LvHead_Id = item.LeaveHead.Id.ToString(),
        //                //},
        //                Id = item.Id,
        //                Reqdate = item.RequisitionDate.Value.ToShortDateString(),
        //                ProgramName = item.ProgramList == null ? "" : item.ProgramList.FullDetails,
        //                Status = "",
        //                Comment = "",
        //                isClose = Empprogramlistids.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == AccessRight.ToUpper() && a.FuncModules.LookupVal.ToUpper() == funmodule.ToUpper())
        //                                           .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString()
        //            });
        //            // }
        //        }
        //        return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);



        //        //if (v.SanctionCode != null)
        //        //{
        //        //    int sanctionid = Convert.ToInt32(v.SanctionCode);
        //        //    var sanctioncode = db.EmployeeLeave.Include(e => e.Employee).Include(e => e.Employee.EmpName).Where(e => e.Employee.Id == sanctionid).SingleOrDefault();
        //        //    if (sanctioncode != null)
        //        //    {
        //        //        v.SanctionCode = sanctioncode.Employee.EmpCode;
        //        //        v.SanctionEmpname = sanctioncode.Employee.EmpName.FullNameFML;
        //        //    }
        //        //}
        //        ////if Emp Bal updated
        //        //var listOfObject = new List<dynamic>();
        //        //listOfObject.Add(v);
        //        //return Json(listOfObject, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}